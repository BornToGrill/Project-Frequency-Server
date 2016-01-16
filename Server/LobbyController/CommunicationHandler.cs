using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using NetworkLibrary;

namespace LobbyController {
    internal class CommunicationHandler {

        private readonly UdpClient _client;
        private readonly ILobbyManager _manager;
        public CommunicationHandler(ILobbyManager lobbyManager) {
            _manager = lobbyManager;
            _client = new UdpClient(Properties.Settings.Default.DefaultPort);
            _client.DataReceived += TcpClient_DataReceived;
            _client.Start();

        }

        private void TcpClient_DataReceived(UdpDataReceivedEventArgs e) {

            switch (e.ReceivedString) {
                case "CreateLobby":
                    Create(e.Sender);
                    break;
            }

        }

        private async void Create(IPEndPoint sender) {
            IPEndPoint lobby = await _manager.CreateLobby();
            _client.Send($"Response:{lobby.Address}|{lobby.Port}", sender);
        }
    }
}
