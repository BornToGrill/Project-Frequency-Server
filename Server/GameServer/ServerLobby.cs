using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Security.Principal;
using Lobby.Com_Handler;
using Lobby.Com_Handler.Data_Processing.Types;
using Lobby.Entities;
using NetworkLibrary;
using Lobby.Interfaces;

namespace Lobby {

    internal sealed class ServerLobby : IPlayerContainer, INotifiable {

        private readonly ObservableCollection<Player> _players;
        private readonly CommunicationHandler _comHandler;

        private List<int> _corners = Enumerable.Range(1, 4).ToList();

        private Player _currentPlayer;

        public ServerLobby(int port) {
            _comHandler = new CommunicationHandler(port, this, this);
            _players = new ObservableCollection<Player>();
            _players.CollectionChanged += Players_CollectionChanged;

            //Console.WriteLine("Server started listening on port : {0}", ((IPEndPoint)_tcpListener.Socket.LocalEndPoint).Port);
        }

        private void Players_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            // TODO: TEMP
            lock (_players) {
                if (e.NewItems == null)
                    return;
                foreach (Player player in _players) {
                    Player joined = (Player) e.NewItems[0];
                    if (player != joined) {
                        player.TcpClient.Send($"[Notify:PlayerJoined:{_players.IndexOf(joined) + 1}|{joined.Name}]");
                    }
                }
            }
            // TODO: ^^^^
            lock (_players)
                if (_players.Count == 0) {
                    // TODO: Start timer.
                }
                else if (_players.Count == 2) { //TODO: REMOVE
                    foreach (Player player in _players)
                        player.TcpClient.Send("[Invoke:StartGame]");
                    _currentPlayer = _players[0];
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

        public ObservableCollection<Player> GetPlayers() {
            return _players;
        }  

        public void AddPlayer(Player player) {
            lock (_players)
                _players.Add(player);
        }

        public void RemovePlayer(Player player) {
            lock (_players)
                _players.Remove(player);
        }

        public int GetRandomCorner() {
            lock (_corners) {
                int index = new Random().Next(_corners.Count);
                int corner = _corners[index];
                _corners.RemoveAt(index);
                return corner;
            }
        }

        public void AddCorner(int id) {
            lock (_corners)
                _corners.Add(id);
        }

        #endregion
        #region INotifiable Implementation Members
        public void EndTurn(string guid) {
            lock (_currentPlayer) {
                if (_currentPlayer.Guid != guid) {
                    Player player = GetPlayer(guid);
                    player.TcpClient.Send("[Error:Can't end turn if it's not your turn...]"); //TODO: Resources
                }
                else {
                    lock (_players) {
                        int currentIndex = _players.IndexOf(_currentPlayer) + 1;
                        if (currentIndex > _players.Count - 1)
                            currentIndex = 0;

                        _currentPlayer = _players[currentIndex];
                        foreach (Player player in _players)
                            player.TcpClient.Send($"[Notify:TurnEnd:{_currentPlayer.CornerId}|{_currentPlayer.Name}]");
                    }
                    Console.WriteLine("Turn ended");
                }
            }

        }

        public void MoveUnit(string guid, string tileOne, string tileTwo) {
            throw new NotImplementedException();
        }

        public void CreateUnit(string guid, string tileTarget) {
            throw new NotImplementedException();
        }

        public void AttackUnit(string guid, string tileOne, string tileTwo) {
            throw new NotImplementedException();
        }
        #endregion
    }
}
