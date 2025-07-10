using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ChatApplication.Network;

namespace ChatApplication.Chat
{
    public class ChatClient : IDisposable
    {
        private readonly TcpConnectionManager _connectionManager;
        private StreamWriter _writer;
        private StreamReader _reader;
        private bool _isDisposed;

        public event EventHandler<string> MessageReceived;
        public event EventHandler<string> MessageSent;
        public event EventHandler<Exception> ChatError;

        public ChatClient(TcpConnectionManager connectionManager)
        {
            _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
            _connectionManager.ConnectionStatusChanged += OnConnectionStatusChanged;
        }

        private void OnConnectionStatusChanged(object _, string status)
        {
            if (status == "Connected")
            {
                InitializeStreams();
            }
            else if (status == "Disconnected")
            {
                CloseStreams();
            }
        }

        private void InitializeStreams()
        {
            try
            {
                var networkStream = _connectionManager.NetworkStream;
                if (networkStream != null)
                {
                    _writer = new StreamWriter(networkStream, Encoding.UTF8) { AutoFlush = true };
                    _reader = new StreamReader(networkStream, Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                OnChatError(ex);
            }
        }

        private void CloseStreams()
        {
            try
            {
                _writer?.Close();
                _reader?.Close();
            }
            catch (Exception ex)
            {
                OnChatError(ex);
            }
            finally
            {
                _writer = null;
                _reader = null;
            }
        }

        public async Task<bool> SendMessageAsync(string message)
        {
            if (string.IsNullOrEmpty(message) || _writer == null)
            {
                return false;
            }

            try
            {
                await _writer.WriteLineAsync(message);
                OnMessageSent(message);
                return true;
            }
            catch (Exception ex)
            {
                OnChatError(ex);
                return false;
            }
        }

        public async Task StartListeningAsync()
        {
            if (_reader == null)
            {
                return;
            }

            try
            {
                while (_connectionManager.IsConnected && !_isDisposed)
                {
                    string message = await _reader.ReadLineAsync();
                    if (message != null)
                    {
                        OnMessageReceived(message);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                OnChatError(ex);
            }
        }

        protected virtual void OnMessageReceived(string message)
        {
            MessageReceived?.Invoke(this, message);
        }

        protected virtual void OnMessageSent(string message)
        {
            MessageSent?.Invoke(this, message);
        }

        protected virtual void OnChatError(Exception error)
        {
            ChatError?.Invoke(this, error);
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                CloseStreams();
                _connectionManager.ConnectionStatusChanged -= OnConnectionStatusChanged;
            }
        }
    }
}