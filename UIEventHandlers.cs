using System;
using System.Threading.Tasks;
using ChatApplication.Core;
using ChatApplication.Models;

namespace ChatApplication.UI
{
    public class UIEventHandlers
    {
        private readonly ChatApplicationManager _applicationManager;
        private ServerInfo _selectedServer;

        public UIEventHandlers(ChatApplicationManager applicationManager)
        {
            _applicationManager = applicationManager ?? throw new ArgumentNullException(nameof(applicationManager));
        }

        public async Task OnConnectButtonClickAsync()
        {
            if (_selectedServer == null)
            {
                throw new InvalidOperationException("No server selected. Please select a server from the dropdown list.");
            }

            await _applicationManager.ConnectToServerAsync(_selectedServer.PcName);
        }

        public void OnDisconnectButtonClick()
        {
            _applicationManager.Disconnect();
        }

        public void OnServerDropdownSelectionChanged(object sender, EventArgs e)
        {
            if (sender is System.Windows.Forms.ComboBox comboBox && comboBox.SelectedItem is ServerInfo selectedServer)
            {
                _selectedServer = selectedServer;
            }
        }

        public async Task OnSendMessageButtonClickAsync(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            await _applicationManager.SendMessageAsync(message);
        }

        public void OnStartListeningButtonClick(int port)
        {
            _applicationManager.StartListening(port);
        }

        public void OnStopListeningButtonClick()
        {
            _applicationManager.StopListening();
        }

        public void LoadServerListFromDatabase(string jsonData)
        {
            _applicationManager.LoadServerList(jsonData);
        }

        public ServerInfo[] GetAvailableServers()
        {
            return _applicationManager.GetAvailableServers();
        }

        public void PopulateServerDropdown(System.Windows.Forms.ComboBox comboBox)
        {
            var servers = GetAvailableServers();
            comboBox.DataSource = servers;
            comboBox.DisplayMember = "DisplayName";
            comboBox.ValueMember = "PcName";
        }
    }
}