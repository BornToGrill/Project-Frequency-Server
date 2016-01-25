using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Lobby.Entities;
using Lobby.Interfaces;
using NetworkLibrary;

namespace Lobby {
    internal class CommunicationHandler {

        private readonly TcpListener _tcpListener;
        private readonly UdpClient _udpClient;
        private readonly IPlayerContainer _playerContainer;

        public CommunicationHandler(int port, IPlayerContainer container) { // TODO: Interface ILobby

            _playerContainer = container;
            _tcpListener = new TcpListener(port);
            _tcpListener.SocketAccepted += SocketAccepted;
            _tcpListener.Start();

            _udpClient = new UdpClient(port);
            _udpClient.DataReceived += UdpClient_DataReceived;
            _udpClient.Start();
        }

        private void SocketAccepted(SocketAcceptedEventArgs e) {
            IPEndPoint sender = (IPEndPoint)e.Socket.RemoteEndPoint;
            Console.WriteLine("Client connected at address : {0}:{1}", sender.Address, sender.Port);
            Guid guid = Guid.NewGuid();
            TcpClient tcpClient = new TcpClient(e.Socket);

            Player player = new Player(guid, tcpClient);
            player.TcpClient.DataReceived += TcpClient_DataReceived;
            player.TcpClient.Disconnected += Client_Disconnected;
            tcpClient.Start();
            tcpClient.Send($"[{player.Guid}:Handshake]");

            _playerContainer.AddPlayer(player);
        }
        private void TcpClient_DataReceived(TcpDataReceivedEventArgs e) {


        }

        private void UdpClient_DataReceived(UdpDataReceivedEventArgs e) {
            // TODO : REMOVE
            string[] message = e.ReceivedString.Split(':');
            if (message.Length == 2 && message[1] == "Handshake") {
                lock (_players)
                    for (int i = 0; i < _players.Count; i++)
                        if (_players[i].Guid == message[0]) {
                            _players[i].UdpVerified = true;
                            for (int x = 0; x < _players.Count; x++)
                                if (_players[x].Guid != message[0])
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

            IPEndPoint senderPoint = (IPEndPoint) sender.Socket.RemoteEndPoint;
            Console.WriteLine("TcpClient at address \"{0}:{1}\" has disconnected.", senderPoint.Address,
                senderPoint.Port);
            Player player = _playerContainer.GetPlayer(sender);
            if (player == null)
                return;
            _playerContainer.RemovePlayer(player);
            player.Dispose();

        }
    }
}
