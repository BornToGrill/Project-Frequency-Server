using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using LobbyController.Com_Handler.DataProcessing.Types;
using LobbyController.Interfaces;
using NetworkLibrary;

namespace LobbyController.Com_Handler.DataProcessing {
    internal class DataProcessor {
        private const char DataSeparator = ':';

        private readonly Invoke _invoke;

        internal DataProcessor(UdpClient client, IInvokable invoke) {
            _invoke = new Invoke(client, invoke);
        }

        internal void ProcessMessage(IPEndPoint sender, string message) {
            var data = message.GetFirst();

            switch (data.Item1) {
                case "Invoke":
                    _invoke.HandleInvoke(sender, data.Item2);
                    break;

                default:
                    Console.WriteLine($"UDP Client received a message that could not be handled : {message}");
                    return;

            }
        }

    }
}
