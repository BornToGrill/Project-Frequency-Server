using NetworkLibrary;

namespace Lobby.Com_Handler.Data_Processing.Types {

    interface INotifiable {

        void SetName(TcpClient sender, string name);
        void PlayerReady(string guid, bool value);
        void StartGame(string guid);
        void EndTurn(string guid);
        void MoveUnit(string guid, string moveType, string tileOne, string tileTwo);
        void CreateUnit(string guid, string tileTarget, string unitType);
        void SplitUnit(string guid, string tileOne, string tileTwo, int amount);
        void CashChanged(string guid, int newValue);
        void GameWon(string guid);
        void GameLost(string guid);
        void GameLoaded(string guid);
    }
}
