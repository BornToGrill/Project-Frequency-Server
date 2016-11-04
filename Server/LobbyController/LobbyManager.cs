using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using LobbyController.Com_Handler;
using LobbyController.Interfaces;
using LobbyController.Security;
using MySql.Data.MySqlClient;

namespace LobbyController {
    class LobbyManager : IDisposable, IInvokable, IRequestable {

        internal List<LobbyInstance> Lobbies;

        private const int MaxServers = 100;
        private readonly List<int> _availablePorts;
        private readonly CommunicationHandler _comHandler;

        public LobbyManager() {
            Console.WriteLine("Starting lobby manager.");
            Lobbies = new List<LobbyInstance>();
            _availablePorts = Enumerable.Range(9501, MaxServers).ToList();
            _comHandler = new CommunicationHandler(this, this);
        }

        #region IInvokable Implementation Members

        #endregion

        #region IRequestable Implementation Members

        public Task<IPEndPoint> CreateLobby() {
            string id;
            int port;
            // Get lobby id & port.
            lock (_availablePorts) {
                if (_availablePorts.Count == 0)
                    return Task.FromResult<IPEndPoint>(null);
                port = _availablePorts.First();
                _availablePorts.RemoveAt(0);
            }
            lock (Lobbies) {
                do {
                    id = Guid.NewGuid().ToString("N").ToUpper().Substring(0, 6);
                } while (Lobbies.Exists(x => x.Id == id));
            }

        // Create lobby and start listening for clients
            LobbyInstance instance = new LobbyInstance(id, port);
            instance.ClientConnected += LobbyCreationCompleted;
            instance.ClientDisconnected += LobbySessionEnded;
            instance.Start();

            // Start client. TODO: Start in background
            ProcessStartInfo process = new ProcessStartInfo() {
                FileName = "Lobby",
                UseShellExecute = true, //TODO: Makes sure it uses a different console! (true)
                Arguments = $"-name {id} -port {port}",
                //CreateNoWindow = true
            };
            Process.Start(process);

            lock(Lobbies)
                Lobbies.Add(instance); // TODO: Not here
            // Wait for the client connection and return the IPendpoint of the client.
            return instance.LobbyInitialized.Task;
        }

        public IPEndPoint JoinLobby(string lobbyId) {
            lock (Lobbies) {
                foreach(LobbyInstance lobby in Lobbies)
                    if (lobby.Id == lobbyId)
                        return lobby.ServerIp;
            }
            return null;
        }

        public bool CreateAccount(string username, string displayName, string passHash, string passSalt) {
            try {
                MySqlConnection conn =
                    new MySqlConnection(ConfigurationManager.ConnectionStrings["database"].ConnectionString);
                conn.Open();
                MySqlCommand com = new MySqlCommand("INSERT INTO UserAccounts VALUES (@user, @display, @hash, @salt)",
                    conn);
                com.Parameters.AddWithValue("@user", username);
                com.Parameters.AddWithValue("@display", displayName);
                com.Parameters.AddWithValue("@hash", passHash);
                com.Parameters.AddWithValue("@salt", passSalt);
                com.ExecuteNonQuery();
                return true;
            }
            catch {
                return false;
            }
        }

        public string Login(string username, string password) {
            try {
                MySqlConnection conn =
                    new MySqlConnection(ConfigurationManager.ConnectionStrings["database"].ConnectionString);
                conn.Open();
                MySqlCommand com = new MySqlCommand("SELECT displayname, passwordHash, passwordSalt FROM UserAccounts WHERE username=@user",
                    conn);
                com.Parameters.AddWithValue("@user", username);
                using (MySqlDataReader reader = com.ExecuteReader()) {
                    reader.Read();
                    string displayName = reader.GetString(0);
                    string hash = reader.GetString(1);
                    string salt = reader.GetString(2);
                    if (Cryptography.CompareSaltHash(password, hash, salt))
                        return displayName;
                    return null;
                }
            }
            catch {
                return null;
            }
        }
        #endregion

        #region Private Methods
        public void Stop() {
            lock (Lobbies) {
                foreach (LobbyInstance lobby in Lobbies)
                    lobby.Dispose();
                Lobbies.Clear();
            }
        }
        private void LobbyCreationCompleted(LobbyInstance sender, string clientId) {
            lock (Lobbies)
                Lobbies.Add(sender);
        }

        private void LobbySessionEnded(LobbyInstance sender) {
            Console.WriteLine($"Lobby instance with ID : <{sender.Id}> has shut down. Freeing up port.");
            lock (Lobbies)
                lock (_availablePorts) {
                    _availablePorts.Add(sender.Port);
                    Lobbies.Remove(sender);
                    sender.Dispose();
                }
        }
        #endregion


        #region IDisposable Implementation Members

        protected void Dispose(bool disposing) {
            if (disposing) {
                _comHandler.Dispose();
                // TODO: Dispose of objects
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
