using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using LobbyController.Com_Handler.Commands;
using LobbyController.Interfaces;
using NetworkLibrary;

namespace LobbyController.Com_Handler.DataProcessing.Types {
    internal class Request {
        private readonly UdpClient _client;
        private readonly IRequestable _request;
        internal Request(UdpClient client, IRequestable request) {
            _client = client;
            _request = request;
        }

        internal void HandleRequest(IPEndPoint sender, string values) {
            var data = values.GetFirst();
            switch (data.Item1) {
                case "CreateLobby":
                    CreateLobby(sender);
                    break;
                case "JoinLobby":
                    JoinLobby(sender, data.Item2);
                    break;
                default:
                    Console.WriteLine($"UDP Client received a message that could not be handled: at HandleRequest : {values}");
                    return;
            }

        }

        private async void CreateLobby(IPEndPoint sender) {
            IPEndPoint lobby = await _request.CreateLobby();
            if (lobby != null)
                _client.SendResponse(sender, new IPEndPoint(IPAddress.Parse("127.0.0.1"), lobby.Port));
                //_client.SendResponse(sender, lobby); // TODO: Remote IP
            else
                _client.SendError(sender, Resources.ErrorStrings.MaxLobbiesReached);
        }

        private void JoinLobby(IPEndPoint sender, string values) {
            IPEndPoint lobby = _request.JoinLobby(values);
            if (lobby != null)
                _client.SendResponse(sender, new IPEndPoint(IPAddress.Parse("127.0.0.1"), lobby.Port));
                //_client.SendResponse(sender, lobby); // TODO: Remote IP
            else
                _client.SendError(sender, "ERR1:NoSuchLobbyBitch"); //todo: resources
        }
    }
}
