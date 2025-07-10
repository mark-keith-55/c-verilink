using System;
using System.Threading.Tasks;
using ChatApplication.Network;
using ChatApplication.Chat;
using ChatApplication.Models;

namespace ChatApplication.Core
{
    public class ChatApplicationManager : IDisposable
    {
        private readonly TcpConnectionManager _connectionManager;
        private readonly ChatClient _chatClient;
        private readonly ChatListener _chatListener;
        private readonly ServerInfoManager _serverInfoManager;
        private bool _isDisposed;

        public event EventHandler<string> ConnectionStatusChanged;
        public event EventHandler<string> MessageReceived;
        public event EventHandler<string> MessageSent;
        public event EventHandler<Exception> ApplicationError;

        public bool IsConnected => _connectionManager.IsConnected;
        public bool IsListening => _chatListener.IsListening;

        public ChatApplicationManager()
        {
            _connectionManager = new TcpConnectionManager();
            _chatClient = new ChatClient(_connectionManager);
            _chatListener = new ChatListener();
            _serverInfoManager = new ServerInfoManager();

            InitializeEventHandlers();
        }

        private void InitializeEventHandlers()
        {
            _connectionManager.ConnectionStatusChanged += (_, status) => 
                ConnectionStatusChanged?.Invoke(this, status);
            
            _connectionManager.ConnectionError += (_, error) => 
                ApplicationError?.Invoke(this, error);

            _chatClient.MessageReceived += (_, message) => 
                MessageReceived?.Invoke(this, message);
            
            _chatClient.MessageSent += (_, message) => 
                MessageSent?.Invoke(this, message);
            
            _chatClient.ChatError += (_, error) => 
                ApplicationError?.Invoke(this, error);

            _chatListener.ListenerError += (_, error) => 
                ApplicationError?.Invoke(this, error);

            _serverInfoManager.DataError += (_, error) => 
                ApplicationError?.Invoke(this, error);
        }

        public void LoadServerList(string jsonData)
        {
            _serverInfoManager.LoadServersFromJson(jsonData);
        }

        public async Task<bool> ConnectToServerAsync(string selectedPcName)
        {
            try
            {
                var serverInfo = _serverInfoManager.FindServerByPcName(selectedPcName);
                if (serverInfo == null)
                {
                    throw new ArgumentException($"Server with PC name '{selectedPcName}' not found.");
                }

                if (!serverInfo.IsActive)
                {
                    throw new InvalidOperationException($"Server '{selectedPcName}' is not active.");
                }

                bool connected = await _connectionManager.ConnectAsync(serverInfo.IpAddress, serverInfo.Port);
                
                if (connected)
                {
                    _ = Task.Run(async () => await _chatClient.StartListeningAsync());
                }

                return connected;
            }
            catch (Exception ex)
            {
                ApplicationError?.Invoke(this, ex);
                return false;
            }
        }

        public async Task<bool> ConnectToServerAsync(string ipAddress, int port)
        {
            try
            {
                bool connected = await _connectionManager.ConnectAsync(ipAddress, port);
                
                if (connected)
                {
                    _ = Task.Run(async () => await _chatClient.StartListeningAsync());
                }

                return connected;
            }
            catch (Exception ex)
            {
                ApplicationError?.Invoke(this, ex);
                return false;
            }
        }

        public void Disconnect()
        {
            _connectionManager.Disconnect();
        }

        public void StartListening(int port)
        {
            _chatListener.StartListening(port);
        }

        public void StopListening()
        {
            _chatListener.StopListening();
        }

        public async Task<bool> SendMessageAsync(string message)
        {
            return await _chatClient.SendMessageAsync(message);
        }

        public ServerInfo[] GetAvailableServers()
        {
            return _serverInfoManager.GetActiveServers().ToArray();
        }

        public string GetCurrentMachineName()
        {
            return _serverInfoManager.GetCurrentMachineName();
        }

        public bool IsCurrentMachineInServerList()
        {
            return _serverInfoManager.IsCurrentMachineInList();
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                
                _connectionManager?.Dispose();
                _chatClient?.Dispose();
                _chatListener?.Dispose();
            }
        }
    }
}