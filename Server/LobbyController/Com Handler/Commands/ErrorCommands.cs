using System;
using System.Net;
using NetworkLibrary;

namespace LobbyController.Com_Handler.Commands {
    internal static class ErrorCommands {
        private const string ErrorPrefix = "Error";
        private const char ErrorIdSeparator = ':';

        internal static void SendError(this UdpClient client, IPEndPoint target, string errorString) {
            var error = SplitError(errorString);
            client.Send($"{ErrorPrefix}:{error.Item1}:{error.Item2}", target);
        }

        private static Tuple<string,string> SplitError(string error) {
            int index = error.IndexOf(ErrorIdSeparator);
            if (index < 0)
                throw new ArgumentException("Error message did not have an ID and message separated by a character");
            return Tuple.Create(error.Substring(0, index), error.Substring(index + 1));
        }
    }
}
