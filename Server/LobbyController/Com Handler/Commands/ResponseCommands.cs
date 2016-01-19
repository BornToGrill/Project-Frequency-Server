using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NetworkLibrary;

namespace LobbyController.Com_Handler.Commands {
    internal static class ResponseCommands {
        private const string ResponsePrefix = "Response";


        internal static void SendResponse(this UdpClient client, IPEndPoint target, IPEndPoint value) {
            client.Send($"{ResponsePrefix}:{value.Address}|{value.Port}", target);
        }
    }
}
