using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Lobby.Entities;
using NetworkLibrary;
using TcpClient = NetworkLibrary.TcpClient;
using TcpListener = NetworkLibrary.TcpListener;
using UdpClient = NetworkLibrary.UdpClient;

namespace Lobby {

    internal interface IGetPlayer {
        Player GetPlayer(string guid);
        Player GetPlayer(Guid guid);
    }

    internal sealed class ServerLobby : IGetPlayer {

        private readonly List<Player> _players;
        private readonly TcpListener _tcpListener;
        private readonly UdpClient _udpClient;
        //private readonly MessageHandler;

        public ServerLobby(int port) {
            _players = new List<Player>();
            _tcpListener = new TcpListener(port);
            _tcpListener.SocketAccepted += SocketAccepted;
            _tcpListener.Start();

            _udpClient = new UdpClient(port);
            _udpClient.DataReceived += UdpClient_DataReceived;
            _udpClient.Start();


            Console.WriteLine("Server started listening on port : {0}", ((IPEndPoint)_tcpListener.Socket.LocalEndPoint).Port);
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
                return _players.Exists(x => x.Guid == guid) ? _players.Single(x => x.Guid == guid) : null;
        }

        public Player GetPlayer(Guid guid) {
            return GetPlayer(guid.ToString("N"));
        }

        #endregion
    }
}
