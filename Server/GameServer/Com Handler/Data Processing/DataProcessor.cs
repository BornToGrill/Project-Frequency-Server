using System;
using System.Linq;
using Lobby.Com_Handler.Data_Processing.Types;
using NetworkLibrary;

namespace Lobby.Com_Handler.Data_Processing {
    internal class DataProcessor {
        private readonly Notify _notify;
        private readonly Request _request;
        public DataProcessor(INotifiable notify, IRequestable request) {
            _notify = new Notify(notify);
            _request = new Request(request);
        }
        internal void ProcessMessage(TcpClient sender, string message) {
            string[] separated =
            message.Split(new[] { "]" }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.TrimStart('[')).ToArray();

            foreach (string command in separated) {
                var data = command.GetFirst();

                switch (data.Item1) {
                    case "Notify":
                        _notify.HandleNotify(sender, data.Item2);
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
}
