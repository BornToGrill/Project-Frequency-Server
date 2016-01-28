using System.Net;
using System.Threading.Tasks;

namespace LobbyController.Interfaces {
    interface IRequestable {

        Task<IPEndPoint> CreateLobby();
        IPEndPoint JoinLobby(string lobbyId);
    }
}
