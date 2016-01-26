namespace Lobby.Com_Handler.Data_Processing.Types {

    interface INotifiable {

        void EndTurn(string guid);
        void MoveUnit(string guid, string tileOne, string tileTwo);
        void CreateUnit(string guid, string tileTarget);
        void AttackUnit(string guid, string tileOne, string tileTwo);
    }
}
