using System;
using System.Net;
using Lobby.Com_Handler.Data_Processing;
using Lobby.Com_Handler.Data_Processing.Types;
using Lobby.Entities;
using Lobby.Interfaces;
using NetworkLibrary;

namespace Lobby.Com_Handler {
    internal class CommunicationHandler {

        private readonly TcpListener _tcpListener;
        private readonly UdpClient _udpClient;
        private readonly IPlayerContainer _playerContainer;

        private readonly DataProcessor _processor;

        public CommunicationHandler(int port, IPlayerContainer container, INotifiable notify, IRequestable request) { // TODO: Interface ILobby

            _playerContainer = container;
            _tcpListener = new TcpListener(port);
            _tcpListener.SocketAccepted += SocketAccepted;
            _tcpListener.Start();

            _udpClient = new UdpClient(port);
            _udpClient.DataReceived += UdpClient_DataReceived;
            _udpClient.Start();

            _processor = new DataProcessor(notify, request);
            Console.WriteLine("Server started listening on port : {0}", ((IPEndPoint)_tcpListener.Socket.LocalEndPoint).Port);
        }

        private void SocketAccepted(SocketAcceptedEventArgs e) {
            IPEndPoint sender = (IPEndPoint)e.Socket.RemoteEndPoint;
            Console.WriteLine("Client connected at address : {0}:{1}", sender.Address, sender.Port);
            TcpClient tcpClient = new TcpClient(e.Socket);

            tcpClient.DataReceived += TcpClient_DataReceived;
            tcpClient.Disconnected += Client_Disconnected;
            tcpClient.Start();

        }
        private void TcpClient_DataReceived(TcpDataReceivedEventArgs e) {
            Console.WriteLine("Received : " + e.ReceivedString);
            if (e.ReceivedData.Length > 0)
                _processor.ProcessMessage(e.Sender, e.ReceivedString);

        }

        private void UdpClient_DataReceived(UdpDataReceivedEventArgs e) {


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
            _playerContainer.AddCorner(player.CornerId);
            player.Dispose();

        }
    }
}
