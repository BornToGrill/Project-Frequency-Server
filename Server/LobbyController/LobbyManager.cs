using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LobbyController.Com_Handler;
using LobbyController.Interfaces;

namespace LobbyController {
    class LobbyManager : IDisposable, IInvokable {

        internal List<LobbyInstance> Lobbies;

        private const int MaxServers = 100;
        private readonly List<int> _availablePorts;
        private readonly List<int> _availableIds;
        private readonly CommunicationHandler _comHandler;

        public LobbyManager() {
            Lobbies = new List<LobbyInstance>();
            _availablePorts = Enumerable.Range(9501, MaxServers).ToList();
            _availableIds = Enumerable.Range(1, MaxServers).ToList();
            _comHandler = new CommunicationHandler(this);
        }

        #region ILobbyManager Implementation Members

        public Task<IPEndPoint> CreateLobby() {
            int id;
            int port;
            // Get lobby id & port.
            lock(_availableIds)
                lock (_availablePorts) {
                    if (_availableIds.Count == 0 || _availablePorts.Count == 0)
                        return Task.FromResult<IPEndPoint>(null);
                    id = _availableIds.First();
                    port = _availablePorts.First();
                    _availableIds.RemoveAt(0);
                    _availablePorts.RemoveAt(0);
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
            // Wait for the client connection and return the IPendpoint of the client.
            return instance.LobbyInitialized.Task;
        }

        public bool JoinLobby(string lobbyId) {
            throw new NotImplementedException("It seems you've got a really lazy developer on your hands.");
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
                lock(_availableIds)
                    lock (_availablePorts) {
                        _availableIds.Add(sender.Id);
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
