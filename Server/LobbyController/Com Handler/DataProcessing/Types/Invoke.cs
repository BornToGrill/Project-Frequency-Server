using System;
using System.Net;
using LobbyController.Com_Handler.Commands;
using LobbyController.Interfaces;
using NetworkLibrary;

namespace LobbyController.Com_Handler.DataProcessing.Types {
    internal class Invoke {

        private readonly UdpClient _client;
        private readonly IInvokable _invoke;
        internal Invoke(UdpClient client, IInvokable invoke) {
            _client = client;
            _invoke = invoke;
        }

        internal void HandleInvoke(IPEndPoint sender, string values) {
            //TODO: On every GetFirst call check for string length etc...
            var data = values.GetFirst();
            switch (data.Item1) {
                default:
                    Console.WriteLine($"UDP Client received a message that could not be handled: at HandleInvoke : {values}");
                    return;
            }

        }
    }
}
