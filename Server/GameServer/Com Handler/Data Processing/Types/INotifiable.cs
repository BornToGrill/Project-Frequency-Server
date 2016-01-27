namespace Lobby.Com_Handler.Data_Processing.Types {

    interface INotifiable {

        void EndTurn(string guid);
        void MoveUnit(string guid, string moveType, string tileOne, string tileTwo);
        void CreateUnit(string guid, string tileTarget, string unitType);
        void AttackUnit(string guid, string tileOne, string tileTwo);
    }
}
