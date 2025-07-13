using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatApplication.Host
{
    public class ChatServer : IDisposable
    {
        private readonly TcpListener _tcpListener;
        private readonly List<ClientConnection> _clients;
        private readonly object _clientsLock = new object();
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isRunning;
        private bool _isDisposed;

        public event EventHandler<string> ServerMessage;
        public event EventHandler<ClientMessageEventArgs> MessageReceived;
        public event EventHandler<ClientEventArgs> ClientConnected;
        public event EventHandler<ClientEventArgs> ClientDisconnected;
        public event EventHandler<Exception> ServerError;

        public bool IsRunning => _isRunning;
        public int ClientCount { get { lock (_clientsLock) { return _clients.Count; } } }

        public ChatServer(IPAddress ipAddress, int port)
        {
            _tcpListener = new TcpListener(ipAddress, port);
            _clients = new List<ClientConnection>();
        }

        public async Task StartAsync()
        {
            if (_isRunning || _isDisposed)
            {
                return;
            }

            try
            {
                _cancellationTokenSource = new CancellationTokenSource();
                _tcpListener.Start();
                _isRunning = true;
                
                OnServerMessage($"Server started on {_tcpListener.LocalEndpoint}");

                await AcceptClientsAsync(_cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                OnServerError(ex);
                await StopAsync();
            }
        }

        private async Task AcceptClientsAsync(CancellationToken cancellationToken)
        {
            while (_isRunning && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var tcpClient = await _tcpListener.AcceptTcpClientAsync();
                    var clientConnection = new ClientConnection(tcpClient);
                    
                    lock (_clientsLock)
                    {
                        _clients.Add(clientConnection);
                    }

                    OnClientConnected(new ClientEventArgs(clientConnection.Id, clientConnection.EndPoint));
                    
                    _ = Task.Run(() => HandleClientAsync(clientConnection, cancellationToken), cancellationToken);
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    if (_isRunning)
                    {
                        OnServerError(ex);
                    }
                }
            }
        }

        private async Task HandleClientAsync(ClientConnection client, CancellationToken cancellationToken)
        {
            try
            {
                var reader = new StreamReader(client.TcpClient.GetStream(), Encoding.UTF8);
                
                while (client.TcpClient.Connected && !cancellationToken.IsCancellationRequested)
                {
                    string message = await reader.ReadLineAsync();
                    if (message != null)
                    {
                        OnMessageReceived(new ClientMessageEventArgs(client.Id, client.EndPoint, message));
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                OnServerError(ex);
            }
            finally
            {
                await DisconnectClientAsync(client);
            }
        }

        public async Task<bool> SendMessageToClientAsync(string clientId, string message)
        {
            ClientConnection client = null;
            
            lock (_clientsLock)
            {
                client = _clients.Find(c => c.Id == clientId);
            }

            if (client == null || !client.TcpClient.Connected)
            {
                return false;
            }

            try
            {
                var writer = new StreamWriter(client.TcpClient.GetStream(), Encoding.UTF8) { AutoFlush = true };
                await writer.WriteLineAsync(message);
                return true;
            }
            catch (Exception ex)
            {
                OnServerError(ex);
                await DisconnectClientAsync(client);
                return false;
            }
        }

        public async Task<bool> BroadcastMessageAsync(string message)
        {
            List<ClientConnection> clientsCopy;
            
            lock (_clientsLock)
            {
                clientsCopy = new List<ClientConnection>(_clients);
            }

            bool allSent = true;
            foreach (var client in clientsCopy)
            {
                bool sent = await SendMessageToClientAsync(client.Id, message);
                if (!sent)
                {
                    allSent = false;
                }
            }

            return allSent;
        }

        private async Task DisconnectClientAsync(ClientConnection client)
        {
            lock (_clientsLock)
            {
                _clients.Remove(client);
            }

            try
            {
                client.TcpClient?.Close();
            }
            catch (Exception ex)
            {
                OnServerError(ex);
            }

            OnClientDisconnected(new ClientEventArgs(client.Id, client.EndPoint));
        }

        public async Task StopAsync()
        {
            if (!_isRunning)
            {
                return;
            }

            _isRunning = false;
            _cancellationTokenSource?.Cancel();

            try
            {
                _tcpListener?.Stop();
                
                List<ClientConnection> clientsCopy;
                lock (_clientsLock)
                {
                    clientsCopy = new List<ClientConnection>(_clients);
                    _clients.Clear();
                }

                foreach (var client in clientsCopy)
                {
                    await DisconnectClientAsync(client);
                }

                OnServerMessage("Server stopped");
            }
            catch (Exception ex)
            {
                OnServerError(ex);
            }
        }

        protected virtual void OnServerMessage(string message)
        {
            ServerMessage?.Invoke(this, message);
        }

        protected virtual void OnMessageReceived(ClientMessageEventArgs e)
        {
            MessageReceived?.Invoke(this, e);
        }

        protected virtual void OnClientConnected(ClientEventArgs e)
        {
            ClientConnected?.Invoke(this, e);
        }

        protected virtual void OnClientDisconnected(ClientEventArgs e)
        {
            ClientDisconnected?.Invoke(this, e);
        }

        protected virtual void OnServerError(Exception error)
        {
            ServerError?.Invoke(this, error);
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                StopAsync().Wait();
                _cancellationTokenSource?.Dispose();
            }
        }
    }

    public class ClientConnection
    {
        public string Id { get; }
        public TcpClient TcpClient { get; }
        public string EndPoint { get; }

        public ClientConnection(TcpClient tcpClient)
        {
            Id = Guid.NewGuid().ToString();
            TcpClient = tcpClient;
            EndPoint = tcpClient.Client.RemoteEndPoint?.ToString() ?? "Unknown";
        }
    }

    public class ClientEventArgs : EventArgs
    {
        public string ClientId { get; }
        public string EndPoint { get; }

        public ClientEventArgs(string clientId, string endPoint)
        {
            ClientId = clientId;
            EndPoint = endPoint;
        }
    }

    public class ClientMessageEventArgs : EventArgs
    {
        public string ClientId { get; }
        public string EndPoint { get; }
        public string Message { get; }

        public ClientMessageEventArgs(string clientId, string endPoint, string message)
        {
            ClientId = clientId;
            EndPoint = endPoint;
            Message = message;
        }
    }
}