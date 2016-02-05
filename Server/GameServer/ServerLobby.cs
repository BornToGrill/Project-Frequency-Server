using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Linq;
using Lobby.Com_Handler;
using Lobby.Com_Handler.Data_Processing.Types;
using Lobby.Entities;
using NetworkLibrary;
using Lobby.Interfaces;

namespace Lobby {

    internal sealed class ServerLobby : IPlayerContainer, INotifiable, IRequestable {

        private readonly ObservableCollection<Player> _players;
        private readonly List<int> _midGameDisconnects; 
        private readonly CommunicationHandler _comHandler;

        private readonly List<int> _corners = Enumerable.Range(1, 4).ToList();

        private Player _currentPlayer;

        private bool _gameStarted;

        private readonly string _lobbyId;

        public ServerLobby(string id, int port) {
            _lobbyId = id;
            _comHandler = new CommunicationHandler(port, this, this, this);
            _players = new ObservableCollection<Player>();
            _players.CollectionChanged += Players_CollectionChanged;
            _midGameDisconnects = new List<int>();
        }

        private void Players_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            lock (_players) {
                if (_players.Count == 0)
                    Environment.Exit(0);
                if (e.NewItems == null)
                    return;
                foreach (Player player in _players) {
                    Player joined = (Player) e.NewItems[0];
                    if (player != joined)
                        player.TcpClient.Send($"[Lobby:SetPlayers:{PlayerList()}]");
                }
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
            lock (_players) {
                _players.Add(player);
                if (_players.Count == 1) {
                    player.IsHost = true;
                    player.Ready = false;
                }
            }
        }

        public void RemovePlayer(Player player) {
            lock (_players) {
                _players.Remove(player);
                if (player.IsHost) {
                    _players[0].IsHost = true;
                }
                foreach (Player pl in _players)
                    if (_gameStarted) {
                        pl.TcpClient.Send($"[Notify:PlayerLeft:{player.CornerId}|{player.Name}]");
                        lock (_midGameDisconnects)
                            _midGameDisconnects.Add(player.CornerId);
                    }
                    else {
                        pl.TcpClient.Send($"[Lobby:SetPlayers:{PlayerList()}]");
                    }
                if(_gameStarted)
                    if (_currentPlayer == player)
                        EndTurn(player.Guid);
            }
        }

        public int GetRandomCorner() {
            lock (_corners) {
                if (_corners.Count <= 0)
                    return -1;
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

        public void SetName(TcpClient sender, string name) {
            int corner= GetRandomCorner();
            if (corner < 0) {
                sender.Send("[Error:ERR4:The lobby you tried to join is full.]");
                sender.Dispose();
                return;
            }
            if (_gameStarted) {
                sender.Send("[Error:ERR5:The game you tried to join is already in progress.]");
                sender.Dispose();
                return;
            }
            Player player = new Player(Guid.NewGuid(), sender) {
                Name = name,
                CornerId = corner
            };
            AddPlayer(player);
            player.TcpClient.Send($"[Response:Authenticated:{player.Guid}|{player.CornerId}|{player.Name}|{_lobbyId}]" +
                                  $"[Response:SetPlayers:{PlayerList()}]");
        }

        public void PlayerReady(string guid, bool value) {
            Player target = GetPlayer(guid);
            if (target == null) return;
            target.Ready = value;
            lock(_players)
                foreach(Player player in _players)
                    player.TcpClient.Send($"[Lobby:SetPlayers:{PlayerList()}]");
        }

        public void StartGame(string guid) {
            if (_gameStarted)
                return;
            lock (_players) {
                if (_players.Any(x => !x.Ready && !x.IsHost))
                    return;
                if (!GetPlayer(guid).IsHost)
                    return;

                _gameStarted = true;
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

                        if (_players.All(x => !x.IsAlive)) {
                            GameWon(_players[0].Guid);
                            return;
                        }
                        do {
                            int currentIndex = _players.IndexOf(_currentPlayer) + 1;
                            if (currentIndex > _players.Count - 1)
                                currentIndex = 0;

                            _currentPlayer = _players[currentIndex];

                        } while (!_currentPlayer.IsAlive);

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
                    GetPlayer(guid)?.TcpClient.Send("[Error:Bitches can't be movin on other turns]");
                    return;
                }
                lock (_players)
                   foreach (Player player in _players.Where(x => x != _currentPlayer))
                        player.TcpClient.Send($"[Invoke:{moveType}:{tileOne}|{tileTwo}]");
            }
            
        }

        public void Attack(string guid, string tileOne, string tileTarget) {
            lock (_currentPlayer) {
                if (_currentPlayer.Guid != guid) {
                    GetPlayer(guid)?.TcpClient.Send("[Error:Can't attack when it's not your turn]");
                    return;
                }
                lock (_players) {
                    foreach(Player player in _players.Where(x => x != _currentPlayer))
                        player.TcpClient.Send($"[Invoke:Attack:{tileOne}|{tileTarget}]");
                }
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

        public void SplitUnit(string guid, string tileOne, string tileTwo, int amount) {
            lock (_players) {
                Player sender = GetPlayer(guid);
                if (sender == null)
                    return;
                foreach(Player player in _players.Where(x => x.Guid != sender.Guid))
                    player.TcpClient.Send($"[Invoke:SplitUnit:{tileOne}|{tileTwo}|{amount}]");
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

        public void GameWon(string guid) {
            _gameStarted = false;
            lock (_players) {
                Player winner = GetPlayer(guid);
                string response = winner.CornerId + "|" +
                                  string.Join("|", _players.Where(x => x != winner).Select(c => c.CornerId).ToArray());
                lock (_midGameDisconnects) {
                    if (_midGameDisconnects.Count > 0) {
                        response += '|' + string.Join("|", _midGameDisconnects);
                    }
                    _midGameDisconnects.Clear();
                }
                foreach (Player player in _players) {
                    player.Reset();
                    player.TcpClient.Send($"[Notify:GameWon:{response}]");
                }
            }
        }

        public void GameLost(string guid) {
            lock (_players) {
                Player loser = GetPlayer(guid);
                if (loser == null)
                    return;
                loser.IsAlive = false;
            }
        }

        public void GameLoaded(string guid) {
            Console.WriteLine("Game loaded : " + guid);
            lock (_players) {
                Player loaded = GetPlayer(guid);
                loaded.GameLoaded = true;
                if(_players.All(x => x.GameLoaded))
                    foreach(Player player in _players)
                        player.TcpClient.Send("[Notify:GameLoaded:]"); //TODO: Check which players are still connected
            }
        }
        #endregion
        #region IRequestable Implementation Members
        public string PlayerList() {
            lock (_players) {
                return string.Join("|", _players.Select(x => $"({x.Name}:{x.CornerId}:{x.Ready}:{x.IsHost})"));
            }
        }
        #endregion
    }
}
