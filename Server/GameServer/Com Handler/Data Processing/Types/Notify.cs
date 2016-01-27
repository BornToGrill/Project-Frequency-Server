using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                case "EndTurn":
                    _notify.EndTurn(data.Item2);
                    break;
                case "MoveToEmpty":
                    MoveToEmpty(data.Item2);
                    break;
                case "UnitCreated":
                    CreateUnit(data.Item2);
                    break;
            }
        }

        private void MoveToEmpty(string values) {
            string[] data = values.Split(ValueDelimiter);
            _notify.MoveUnit(data[0], "MoveToEmpty", data[1], data[2]);

        }

        private void CreateUnit(string values) {
            string[] data = values.Split(ValueDelimiter);
            _notify.CreateUnit(data[0], data[1], data[2]);
        }
    }
}
