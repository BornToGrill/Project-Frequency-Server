using System.Net;
using System.Threading.Tasks;

namespace LobbyController.Interfaces {
    interface IRequestable {

        Task<IPEndPoint> CreateLobby();
        IPEndPoint JoinLobby(string lobbyId);

        string Login(string username, string password);
        bool CreateAccount(string username, string displayName, string passwordHash, string passwordSalt);

    }
}
