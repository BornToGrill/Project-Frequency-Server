using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace LobbyController {
    class LobbyManagerStart : ILobbyManager {

        internal int MaxServers = 100;
        internal List<LobbyInstance> Lobbies;
        internal List<int> AvailablePorts;
        internal List<int> AvailableIds;
        private CommunicationHandler _comHandler;

        public LobbyManagerStart() {
            Lobbies = new List<LobbyInstance>();
            AvailablePorts = Enumerable.Range(9501, MaxServers).ToList();
            AvailableIds = Enumerable.Range(1, MaxServers).ToList();
            _comHandler = new CommunicationHandler(this);
        }

        #region ILobbyManager Implementation Members

        public Task<IPEndPoint> CreateLobby() {
            int id;
            int port;
            // Get lobby id & port.
            lock(AvailableIds)
                lock (AvailablePorts) {
                    id = AvailableIds.First();
                    port = AvailablePorts.First();
                    AvailableIds.RemoveAt(0);
                    AvailablePorts.RemoveAt(0);
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
                Arguments = $"-name {id} -port {port}"
            };
            Process.Start(process);
            // Wait for the client connection and return the IPendpoint of the client.
            return instance.LobbyInitialized.Task;
        }

        public bool JoinLobby(string lobbyId) {
            throw new NotImplementedException();
        }
        #endregion

        #region Private Methods
        private void Stop() {
            lock (Lobbies) {
                foreach (LobbyInstance lobby in Lobbies)
                    lobby.Dispose();
                Lobbies.Clear();
            }
        }
        private void LobbyCreationCompleted(LobbyInstance sender, int clientId) {
            lock (Lobbies)
                Lobbies.Add(sender);
        }

        private void LobbySessionEnded(LobbyInstance sender) {
            lock (Lobbies) 
                lock(AvailableIds)
                    lock (AvailablePorts) {
                        AvailableIds.Add(sender.Id);
                        AvailablePorts.Add(sender.Port);
                        Lobbies.Remove(sender);
                        sender.Dispose();
                    }
        }
        #endregion
    }
}
