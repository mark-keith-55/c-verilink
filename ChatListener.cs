using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ChatApplication.Network;

namespace ChatApplication.Chat
{
    public class ChatListener : IDisposable
    {
        private TcpListener _tcpListener;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isListening;
        private readonly object _lockObject = new object();

        public event EventHandler<TcpClient> ClientConnected;
        public event EventHandler<string> ListenerStatusChanged;
        public event EventHandler<Exception> ListenerError;

        public bool IsListening
        {
            get
            {
                lock (_lockObject)
                {
                    return _isListening;
                }
            }
        }

        public void StartListening(int port)
        {
            lock (_lockObject)
            {
                if (_isListening)
                {
                    return;
                }

                try
                {
                    _tcpListener = new TcpListener(IPAddress.Any, port);
                    _cancellationTokenSource = new CancellationTokenSource();
                    _tcpListener.Start();
                    _isListening = true;

                    OnListenerStatusChanged($"Listening on port {port}");
                    
                    Task.Run(() => AcceptClientsAsync(_cancellationTokenSource.Token));
                }
                catch (Exception ex)
                {
                    OnListenerError(ex);
                    StopListening();
                }
            }
        }

        public void StopListening()
        {
            lock (_lockObject)
            {
                if (!_isListening)
                {
                    return;
                }

                try
                {
                    _isListening = false;
                    _cancellationTokenSource?.Cancel();
                    _tcpListener?.Stop();
                    OnListenerStatusChanged("Stopped listening");
                }
                catch (Exception ex)
                {
                    OnListenerError(ex);
                }
                finally
                {
                    _cancellationTokenSource?.Dispose();
                    _cancellationTokenSource = null;
                    _tcpListener = null;
                }
            }
        }

        private async Task AcceptClientsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && _isListening)
            {
                try
                {
                    var tcpClient = await _tcpListener.AcceptTcpClientAsync();
                    OnClientConnected(tcpClient);
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        OnListenerError(ex);
                    }
                }
            }
        }

        protected virtual void OnClientConnected(TcpClient client)
        {
            ClientConnected?.Invoke(this, client);
        }

        protected virtual void OnListenerStatusChanged(string status)
        {
            ListenerStatusChanged?.Invoke(this, status);
        }

        protected virtual void OnListenerError(Exception error)
        {
            ListenerError?.Invoke(this, error);
        }

        public void Dispose()
        {
            StopListening();
        }
    }
}