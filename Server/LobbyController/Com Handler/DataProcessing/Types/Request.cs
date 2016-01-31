using System;
using System.Net;
using LobbyController.Com_Handler.Commands;
using LobbyController.Interfaces;
using NetworkLibrary;

namespace LobbyController.Com_Handler.DataProcessing.Types {
    internal class Request {
        private readonly UdpClient _client;
        private readonly IRequestable _request;
        private const char ValueDelimiter = '|';

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
                case "CreateAccount":
                    CreateAccount(sender, data.Item2);
                    break;
                case "Login":
                    Login(sender, data.Item2);
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
            IPEndPoint lobby = _request.JoinLobby(values.ToUpper());
            if (lobby != null)
                _client.SendResponse(sender, new IPEndPoint(IPAddress.Parse("127.0.0.1"), lobby.Port));
                //_client.SendResponse(sender, lobby); // TODO: Remote IP
            else
                _client.SendError(sender, "ERR1:NoSuchLobby"); //todo: resources
        }

        private void CreateAccount(IPEndPoint sender, string values) {
            string[] data = values.Split(ValueDelimiter);
            if (_request.CreateAccount(data[0], data[1], data[2], data[3])) {
                _client.Send("Response:CreateAccount:Success", sender);
            }
            else {
                _client.SendError(sender, "ERR2:Can't create account");
                // TODO: SEND ERROR
            }
        }

        private void Login(IPEndPoint sender, string values) {
            string[] data = values.Split(ValueDelimiter);
            string name = _request.Login(data[0], data[1]);
            if (name != null) {
                _client.Send($"Response:Login:{name}", sender);
            }
            else {
                _client.SendError(sender, "ERR2:Invalid login");
                // TODO: Send error
            }
        }
    }
}
