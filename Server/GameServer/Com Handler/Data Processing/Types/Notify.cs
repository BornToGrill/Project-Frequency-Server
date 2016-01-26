using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkLibrary;

namespace Lobby.Com_Handler.Data_Processing.Types {
    class Notify {

        private INotifiable _notify;

        public Notify(INotifiable notify) {
            _notify = notify;
        }

        public void HandleNotify(TcpClient client, string values) {
            var data = values.GetFirst();

            switch (data.Item1) {
                case "EndTurn":
                    _notify.EndTurn(data.Item2);
                    break;
            }
        }
    }
}
