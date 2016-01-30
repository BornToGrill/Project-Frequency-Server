using NetworkLibrary;

namespace Lobby.Com_Handler.Data_Processing.Types {
    class Request {
        private readonly IRequestable _request;

        private const char ValueDelimiter = '|';

        public Request(IRequestable request) {
            _request = request;
        }

        public void HandleRequest(TcpClient client, string values) {
            var data = values.GetFirst();

            switch (data.Item1) {
                case "PlayerList":
                    PlayerList(client);
                    break;
            }
        }

        private void PlayerList(TcpClient sender) {
            string players = _request.PlayerList();
            sender.Send($"[Lobby:SetPlayers:{players}]");
        }
    }
}
