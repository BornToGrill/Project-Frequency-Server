using System;
using Lobby.Entities;
using NetworkLibrary;

namespace Lobby.Interfaces {

    internal interface IPlayerContainer {
        Player GetPlayer(string guid);
        Player GetPlayer(Guid guid);
        Player GetPlayer(TcpClient client);
        void AddPlayer(Player player);
        void RemovePlayer(Player player);
    }
}
