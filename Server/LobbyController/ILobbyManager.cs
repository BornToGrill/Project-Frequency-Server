using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LobbyController {
    internal interface ILobbyManager {

        Task<IPEndPoint> CreateLobby();
        bool JoinLobby(string lobbyId);
    }
}
