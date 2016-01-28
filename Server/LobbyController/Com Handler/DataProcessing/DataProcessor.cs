using System;
using System.Net;
using LobbyController.Com_Handler.DataProcessing.Types;
using LobbyController.Interfaces;
using NetworkLibrary;

namespace LobbyController.Com_Handler.DataProcessing {
    internal class DataProcessor {

        private readonly Invoke _invoke;
        private readonly Request _request;

        internal DataProcessor(UdpClient client, IInvokable invoke, IRequestable request) {
            _invoke = new Invoke(client, invoke);
            _request = new Request(client, request);
        }

        internal void ProcessMessage(IPEndPoint sender, string message) {
            var data = message.GetFirst();

            switch (data.Item1) {
                case "Invoke":
                    _invoke.HandleInvoke(sender, data.Item2);
                    break;
                case "Request":
                    _request.HandleRequest(sender, data.Item2);
                    break;
                default:
                    Console.WriteLine($"UDP Client received a message that could not be handled : {message}");
                    return;

            }
        }

    }
}
