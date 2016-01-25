using System;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using Lobby.Com_Handler;
using Lobby.Entities;
using NetworkLibrary;
using Lobby.Interfaces;

namespace Lobby {

    internal sealed class ServerLobby : IPlayerContainer {

        private readonly ObservableCollection<Player> _players;
        private readonly CommunicationHandler _comHandler;

        public ServerLobby(int port) {
            _comHandler = new CommunicationHandler(port, this);
            _players = new ObservableCollection<Player>();
            _players.CollectionChanged += Players_CollectionChanged;

            //Console.WriteLine("Server started listening on port : {0}", ((IPEndPoint)_tcpListener.Socket.LocalEndPoint).Port);
        }

        private void Players_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            // TODO: TEMP
            lock (_players) {
                foreach (Player player in _players) {
                    player.TcpClient.Send($"Invoke:PlayerJoined:{_players.Count} players connected. Need {4 - _players.Count} more.");
                }
            }
            // TODO: ^^^^
            lock(_players)
                if (_players.Count == 0) {
                    // TODO: Start timer.
                }
                else {
                    // TODO: End timer.
                }
        }

        #region IGetPlayer Implementation Members

        public Player GetPlayer(string guid) {
            lock (_players)
                return _players.FirstOrDefault(player => player.Guid == guid);
            
        }

        public Player GetPlayer(Guid guid) {
            return GetPlayer(guid.ToString("N"));
        }

        public Player GetPlayer(TcpClient client) {
            lock (_players)
                return _players.FirstOrDefault(player => player.TcpClient == client);
        }

        public void AddPlayer(Player player) {
            lock (_players)
                _players.Add(player);
        }

        public void RemovePlayer(Player player) {
            lock (_players)
                _players.Remove(player);
        }

        #endregion
    }
}
