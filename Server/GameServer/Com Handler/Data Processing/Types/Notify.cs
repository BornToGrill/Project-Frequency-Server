using System;
using NetworkLibrary;

namespace Lobby.Com_Handler.Data_Processing.Types {
    class Notify {

        private INotifiable _notify;

        private const char ValueDelimiter = '|';

        public Notify(INotifiable notify) {
            _notify = notify;
        }

        public void HandleNotify(TcpClient client, string values) {
            var data = values.GetFirst();

            switch (data.Item1) {
                case "SetName":
                    SetName(client, data.Item2);
                    break;
                case "PlayerReady":
                    PlayerReady(data.Item2);
                    break;
                case "EndTurn":
                    _notify.EndTurn(data.Item2);
                    break;
                case "MoveToEmpty":
                case "MoveToMerge":
                case "MoveToAttack":
                    MoveUnit(data.Item1, data.Item2);
                    break;
                case "UnitCreated":
                    CreateUnit(data.Item2);
                    break;
                case "CashChanged":
                    CashChanged(data.Item2);
                    break;
                case "GameStart":
                    GameStart(data.Item2);
                    break;
                case "GameWon":
                    GameWon(data.Item2);
                    break;
                case "GameLoaded":
                    GameLoaded(data.Item2);
                    break;
            }
        }

        private void SetName(TcpClient sender, string values) {
            _notify.SetName(sender, values);
        }

        private void PlayerReady(string values) {
            string[] data = values.Split(ValueDelimiter);
            _notify.PlayerReady(data[0], bool.Parse(data[1]));
        }
        private void GameStart(string values) {
            _notify.StartGame(values);
        }

        private void MoveUnit(string action, string values) {
            string[] data = values.Split(ValueDelimiter);
            _notify.MoveUnit(data[0], action, data[1], data[2]);
        }
        private void MoveToEmpty(string values) {
            string[] data = values.Split(ValueDelimiter);
            _notify.MoveUnit(data[0], "MoveToEmpty", data[1], data[2]);

        }

        private void CreateUnit(string values) {
            string[] data = values.Split(ValueDelimiter);
            _notify.CreateUnit(data[0], data[1], data[2]);
        }

        private void CashChanged(string values) {
            string[] data = values.Split(ValueDelimiter);
            _notify.CashChanged(data[0], Int32.Parse(data[1]));
        }

        private void GameWon(string values) {
            _notify.GameWon(values);
        }

        private void GameLoaded(string values) {
            _notify.GameLoaded(values);
        }
    }
}
