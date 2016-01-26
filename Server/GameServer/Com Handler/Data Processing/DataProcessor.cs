using System;
using System.Linq;
using Lobby.Com_Handler.Data_Processing.Types;
using NetworkLibrary;

namespace Lobby.Com_Handler.Data_Processing {
    internal class DataProcessor {
        private Notify _notify;
        public DataProcessor(INotifiable notify) {
            _notify = new Notify(notify);
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
                    default:
                        Console.WriteLine($"UDP Client received a message that could not be handled : {message}");
                        return;

                }
            }
        }


    }
}
