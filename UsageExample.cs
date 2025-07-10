using System;
using System.Threading.Tasks;
using ChatApplication.Core;
using ChatApplication.UI;

namespace ChatApplication.Examples
{
    public class UsageExample
    {
        private ChatApplicationManager _applicationManager;
        private UIEventHandlers _uiEventHandlers;

        public async Task InitializeApplicationAsync()
        {
            _applicationManager = new ChatApplicationManager();
            _uiEventHandlers = new UIEventHandlers(_applicationManager);

            SetupEventHandlers();

            string jsonServerList = @"[
                {
                    ""PcName"": ""SERVER-001"",
                    ""IpAddress"": ""192.168.1.100"",
                    ""Port"": 8080,
                    ""DisplayName"": ""メインサーバー"",
                    ""IsActive"": true
                },
                {
                    ""PcName"": ""SERVER-002"",
                    ""IpAddress"": ""192.168.1.101"",
                    ""Port"": 8080,
                    ""DisplayName"": ""バックアップサーバー"",
                    ""IsActive"": true
                }
            ]";

            _uiEventHandlers.LoadServerListFromDatabase(jsonServerList);
        }

        private void SetupEventHandlers()
        {
            _applicationManager.ConnectionStatusChanged += OnConnectionStatusChanged;
            _applicationManager.MessageReceived += OnMessageReceived;
            _applicationManager.MessageSent += OnMessageSent;
            _applicationManager.ApplicationError += OnApplicationError;
        }

        private void OnConnectionStatusChanged(object sender, string status)
        {
            Console.WriteLine($"接続状態: {status}");
        }

        private void OnMessageReceived(object sender, string message)
        {
            Console.WriteLine($"受信: {message}");
        }

        private void OnMessageSent(object sender, string message)
        {
            Console.WriteLine($"送信: {message}");
        }

        private void OnApplicationError(object sender, Exception error)
        {
            Console.WriteLine($"エラー: {error.Message}");
        }

        public async Task SimulateUIInteractionAsync()
        {
            try
            {
                await _uiEventHandlers.OnConnectButtonClickAsync();
                
                await Task.Delay(1000);
                
                await _uiEventHandlers.OnSendMessageButtonClickAsync("こんにちは、サーバー！");
                
                await Task.Delay(5000);
                
                _uiEventHandlers.OnDisconnectButtonClick();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"シミュレーション中にエラーが発生: {ex.Message}");
            }
        }

        public void Cleanup()
        {
            _applicationManager?.Dispose();
        }
    }

    public class Program
    {
        public static async Task Main(string[] args)
        {
            var example = new UsageExample();
            
            try
            {
                await example.InitializeApplicationAsync();
                await example.SimulateUIInteractionAsync();
            }
            finally
            {
                example.Cleanup();
            }
        }
    }
}