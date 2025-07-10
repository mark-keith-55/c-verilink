using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace ChatApplication.Models
{
    public class ServerInfo
    {
        public string PcName { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public string DisplayName { get; set; }
        public bool IsActive { get; set; }
    }

    public class ServerInfoManager
    {
        private List<ServerInfo> _servers;
        private readonly object _lockObject = new object();

        public event EventHandler<List<ServerInfo>> ServerListUpdated;
        public event EventHandler<Exception> DataError;

        public List<ServerInfo> Servers
        {
            get
            {
                lock (_lockObject)
                {
                    return _servers?.ToList() ?? new List<ServerInfo>();
                }
            }
        }

        public void LoadServersFromJson(string jsonData)
        {
            try
            {
                var servers = JsonConvert.DeserializeObject<List<ServerInfo>>(jsonData);
                
                lock (_lockObject)
                {
                    _servers = servers ?? new List<ServerInfo>();
                }

                OnServerListUpdated(_servers);
            }
            catch (Exception ex)
            {
                OnDataError(ex);
            }
        }

        public ServerInfo FindServerByPcName(string pcName)
        {
            if (string.IsNullOrEmpty(pcName))
            {
                return null;
            }

            lock (_lockObject)
            {
                return _servers?.FirstOrDefault(s => 
                    string.Equals(s.PcName, pcName, StringComparison.OrdinalIgnoreCase));
            }
        }

        public List<ServerInfo> GetActiveServers()
        {
            lock (_lockObject)
            {
                return _servers?.Where(s => s.IsActive).ToList() ?? new List<ServerInfo>();
            }
        }

        public void UpdateServerStatus(string pcName, bool isActive)
        {
            lock (_lockObject)
            {
                var server = _servers?.FirstOrDefault(s => 
                    string.Equals(s.PcName, pcName, StringComparison.OrdinalIgnoreCase));
                
                if (server != null)
                {
                    server.IsActive = isActive;
                    OnServerListUpdated(_servers);
                }
            }
        }

        public string GetCurrentMachineName()
        {
            return Environment.MachineName;
        }

        public bool IsCurrentMachineInList()
        {
            var currentMachine = GetCurrentMachineName();
            return FindServerByPcName(currentMachine) != null;
        }

        protected virtual void OnServerListUpdated(List<ServerInfo> servers)
        {
            ServerListUpdated?.Invoke(this, servers);
        }

        protected virtual void OnDataError(Exception error)
        {
            DataError?.Invoke(this, error);
        }
    }
}