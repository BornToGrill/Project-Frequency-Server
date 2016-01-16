using System;
using System.Net;
using NetworkLibrary;

namespace LobbyController {
    internal class CommunicationHandler {

        private readonly UdpClient _client;
        private readonly ILobbyManager _manager;

        public CommunicationHandler(ILobbyManager lobbyManager) {
            _manager = lobbyManager;
            _client = new UdpClient(Properties.Settings.Default.DefaultPort);
            _client.DataReceived += UdpClient_DataReceived;
            _client.Start();

        }

        private void UdpClient_DataReceived(UdpDataReceivedEventArgs e) {
            string[] readData = e.ReceivedString.Split(':');
            switch (readData[0]) {
                case "Request":
                    HandleRequests(readData, e);
                    break;
                default:
                    Console.WriteLine($"UDP Client received a message that could not be handled : {e.ReceivedString}");
                    return;
            }

        }

        private void HandleRequests(string[] values, UdpDataReceivedEventArgs e) {
            if (values.Length < 2) {
                Console.WriteLine($"Invalid Request message : {e.ReceivedString}");
                return;
            }
            switch (values[1]) {
                case "CreateLobby":
                    Create(e.Sender);
                    break;
                default:
                    Console.WriteLine($"No such request exists : {e.ReceivedString}");
                    return;
            }
        }

        private async void Create(IPEndPoint sender) {
            IPEndPoint lobby = await _manager.CreateLobby();
            _client.Send($"Response:{lobby.Address}|{lobby.Port}", sender);
        }
    }
}
