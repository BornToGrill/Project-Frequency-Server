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

        private bool _gameStarted;

        private readonly string _lobbyId;

        public ServerLobby(string id, int port) {
            _lobbyId = id;
            _comHandler = new CommunicationHandler(id, port, this, this);
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
                    if (player != joined)
                        player.TcpClient.Send($"[Lobby:PlayerJoined:{joined.CornerId}|{joined.Name}]");
                }
            }
            // TODO: ^^^^
            lock (_players)
                if (_players.Count == 0) {
                    // TODO: Start timer.
                }
                else {
                    // TODO: End timer.
                }
        }

        #region IPlayerContainer Implementation Members

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
            lock (_players) {
                _players.Remove(player);
                foreach (Player pl in _players)
                    if (_gameStarted)
                        pl.TcpClient.Send("[Notify:PlayerLeft.....]"); //TODO
                    else
                        pl.TcpClient.Send($"[Lobby:PlayerLeft:{player.CornerId}|{player.Name}]");
            }

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

        public void StartGame(string guid) {
            if (_gameStarted)
                return;
            _gameStarted = true;
            lock (_players) {
                _currentPlayer = _players[0];
                string playersData = string.Empty;
                foreach (Player pl in _players)
                    playersData += "(" + pl.Name + "|" + pl.CornerId + ")";

                foreach (Player player in _players) {
                    player.TcpClient.Send($"[Lobby:StartGame:{playersData}]");
                }
            }
        }

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

        public void MoveUnit(string guid, string moveType, string tileOne, string tileTwo) {
            lock (_currentPlayer) {
                if (_currentPlayer.Guid != guid) {
                    GetPlayer(guid).TcpClient.Send("[Error:Bitches can't be movin on other turns]");
                    return;
                }
                lock (_players)
                   foreach (Player player in _players.Where(x => x != _currentPlayer))
                        player.TcpClient.Send($"[Invoke:{moveType}:{tileOne}|{tileTwo}]");
            }
            
        }

        public void CreateUnit(string guid, string tileTarget, string unitType) {
            lock (_currentPlayer) {
                if (_currentPlayer.Guid != guid) {
                    GetPlayer(guid).TcpClient.Send("[Error:Can't create unit because it's not your turn]");
                    return;
                }
                lock (_players)
                    foreach (Player player in _players.Where(x => x != _currentPlayer))
                        player.TcpClient.Send($"[Invoke:CreateUnit:{tileTarget}|{unitType}|{_currentPlayer.CornerId}]");
            }
        }

        public void CashChanged(string guid, int newValue) {
            lock (_players) {
                int id = GetPlayer(guid).CornerId;
                foreach (Player player in _players.Where(x => x.Guid != guid)) {
                    player.TcpClient.Send($"[Invoke:CashChanged:{id}|{newValue}]");
                }
            }
        }

        public void AttackUnit(string guid, string tileOne, string tileTwo) {
            throw new NotImplementedException();
        }
        #endregion
    }
}
