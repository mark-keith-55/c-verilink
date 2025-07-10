using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ChatApplication.Network
{
    public class TcpConnectionManager : IDisposable
    {
        private TcpClient _tcpClient;
        private NetworkStream _networkStream;
        private bool _isConnected;
        private readonly object _lockObject = new object();

        public event EventHandler<string> ConnectionStatusChanged;
        public event EventHandler<Exception> ConnectionError;

        public bool IsConnected
        {
            get
            {
                lock (_lockObject)
                {
                    return _isConnected && _tcpClient?.Connected == true;
                }
            }
        }

        public NetworkStream NetworkStream
        {
            get
            {
                lock (_lockObject)
                {
                    return _networkStream;
                }
            }
        }

        public async Task<bool> ConnectAsync(string serverAddress, int port)
        {
            try
            {
                lock (_lockObject)
                {
                    if (_isConnected)
                    {
                        return true;
                    }

                    _tcpClient = new TcpClient();
                }

                await _tcpClient.ConnectAsync(serverAddress, port);

                lock (_lockObject)
                {
                    _networkStream = _tcpClient.GetStream();
                    _isConnected = true;
                }

                OnConnectionStatusChanged("Connected");
                return true;
            }
            catch (Exception ex)
            {
                OnConnectionError(ex);
                Disconnect();
                return false;
            }
        }

        public void Disconnect()
        {
            lock (_lockObject)
            {
                try
                {
                    _isConnected = false;
                    _networkStream?.Close();
                    _tcpClient?.Close();
                }
                catch (Exception ex)
                {
                    OnConnectionError(ex);
                }
                finally
                {
                    _networkStream = null;
                    _tcpClient = null;
                    OnConnectionStatusChanged("Disconnected");
                }
            }
        }

        protected virtual void OnConnectionStatusChanged(string status)
        {
            ConnectionStatusChanged?.Invoke(this, status);
        }

        protected virtual void OnConnectionError(Exception error)
        {
            ConnectionError?.Invoke(this, error);
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}