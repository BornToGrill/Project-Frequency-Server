using System;

namespace LobbyController {

    internal class Program {
        private static void Main() {
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.RemoteIP))
                throw new NullReferenceException("IP was not set in settings file.");

            using (LobbyManager manager = new LobbyManager()) {
                Console.ReadLine();
            }
            Console.WriteLine("Lobby manager has disposed all underlying lobbies.");
            Console.ReadLine();
        }

    }

}
