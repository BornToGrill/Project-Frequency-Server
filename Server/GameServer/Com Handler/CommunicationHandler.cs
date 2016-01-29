using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
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
        private readonly string lobbyId;

        public CommunicationHandler(string id, int port, IPlayerContainer container, INotifiable notify) { // TODO: Interface ILobby
            lobbyId = id;

            _playerContainer = container;
            _tcpListener = new TcpListener(port);
            _tcpListener.SocketAccepted += SocketAccepted;
            _tcpListener.Start();

            _udpClient = new UdpClient(port);
            _udpClient.DataReceived += UdpClient_DataReceived;
            _udpClient.Start();

            _processor = new DataProcessor(notify);
        }

        private void SocketAccepted(SocketAcceptedEventArgs e) {
            IPEndPoint sender = (IPEndPoint)e.Socket.RemoteEndPoint;
            Console.WriteLine("Client connected at address : {0}:{1}", sender.Address, sender.Port);
            //Guid guid = Guid.NewGuid();
            TcpClient tcpClient = new TcpClient(e.Socket);

            //Player player = new Player(guid, tcpClient) {
            //    Name = "Bert " + new Random().Next(100),
            //    CornerId = _playerContainer.GetRandomCorner()
            //};
            //player.TcpClient.DataReceived += TcpClient_DataReceived;
            //player.TcpClient.Disconnected += Client_Disconnected;
            //tcpClient.Start();
            tcpClient.DataReceived += TcpClient_DataReceived;
            tcpClient.Disconnected += Client_Disconnected;
            tcpClient.Start();


            /*var players = _playerContainer.GetPlayers();
            string playersData = string.Empty;
            lock (players) {
                foreach (Player pl in players)
                    playersData += $"{pl.Name}|";
            }
            playersData = playersData.TrimEnd('|');
            tcpClient.Send($"[Lobby:SetPlayers:{playersData}]");
            tcpClient.Send($"[Lobby:Authenticated:{player.Guid}|{player.CornerId}|{player.Name}|{lobbyId}]");
            _playerContainer.AddPlayer(player);*/
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
