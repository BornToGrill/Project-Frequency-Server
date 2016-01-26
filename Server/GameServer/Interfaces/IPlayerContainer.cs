using System;
using System.Collections.ObjectModel;
using Lobby.Entities;
using NetworkLibrary;

namespace Lobby.Interfaces {

    internal interface IPlayerContainer {
        Player GetPlayer(string guid);
        Player GetPlayer(Guid guid);
        Player GetPlayer(TcpClient client);
        ObservableCollection<Player> GetPlayers(); 
        void AddPlayer(Player player);
        void RemovePlayer(Player player);
        int GetRandomCorner();
        void AddCorner(int id);
    }
}
