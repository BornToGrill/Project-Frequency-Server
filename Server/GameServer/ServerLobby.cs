using System;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
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

            Console.WriteLine("Server started listening on port : {0}", ((IPEndPoint)_tcpListener.Socket.LocalEndPoint).Port);
        }

        private void Players_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            lock(_players)
                if (_players.Count == 0) {
                    // TODO: Start timer.
                }
                else {
                    // TODO: End timer.
                }
        }

        #region Private Methods
        private void SocketAccepted(SocketAcceptedEventArgs e) {
            IPEndPoint sender = (IPEndPoint) e.Socket.RemoteEndPoint;
            Console.WriteLine("Client connected at address : {0}:{1}", sender.Address, sender.Port);
            Guid guid = Guid.NewGuid();
            TcpClient tcpClient = new TcpClient(e.Socket);

            Player player = new Player(guid, tcpClient);
            player.TcpClient.DataReceived += TcpClient_DataReceived;
            player.TcpClient.Disconnected += Client_Disconnected;
            tcpClient.Start();
            tcpClient.Send($"[{player.Guid}:Handshake]");
            lock (_players)
                _players.Add(player);
        }
        private void TcpClient_DataReceived(TcpDataReceivedEventArgs e) {
            //Console.WriteLine(new ASCIIEncoding().GetString(data));
            // TEST ========= TEST
            // TODO: REMOVE



        }

        private void UdpClient_DataReceived(UdpDataReceivedEventArgs e) {
            // TODO : REMOVE
            string[] messages = e.ReceivedString.Split(new[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);   // TODO : Remove new [] (make const?)
            string[] message = messages[0].Split(':');
            if (message.Length == 2 && message[1] == "Handshake") {
                lock (_players)
                    for (int i = 0; i < _players.Count; i++)
                        if (_players[i].Guid == message[0]) {
                            _players[i].UdpVerified = true;
                            for(int x = 0; x < _players.Count; x++)
                                if(_players[x].Guid != message[0])
                                    _players[i].TcpClient.Send($"[{_players[x].Guid}:NewPlayer]");
                        }
                        else
                            _players[i].TcpClient.Send($"[{message[0]}:NewPlayer]"); // NOTE: TCP message send back.
                return;
            }

            lock (_players)
                for (int i = 0; i < _players.Count; i++)
                    if (!_players[i].ClientIp.Equals(e.Sender) && _players[i].UdpVerified)
                        _udpClient.Send(e.ReceivedData, _players[i].ClientIp);
        }
        private void Client_Disconnected(TcpClient sender) {
            // TODO: Server message : Player disconnected

            IPEndPoint senderPoint = (IPEndPoint)sender.Socket.RemoteEndPoint;
            Console.WriteLine("TcpClient at address \"{0}:{1}\" has disconnected.", senderPoint.Address, senderPoint.Port);
            lock (_players) {
                for(int i = 0;i < _players.Count; i++)
                    if (_players[i].TcpClient == sender) {
                        _players[i].Dispose();
                        _players.RemoveAt(i);
                        return;
                    }
            }
        }
        #endregion

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
