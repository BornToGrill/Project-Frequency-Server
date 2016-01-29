using System;

namespace LobbyController {

    internal class Program {
        private static void Main() {
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.RemoteIP))
                throw new NullReferenceException("IP was not set in settings file.");



            LobbyManager manager = new LobbyManager();
            AppDomain.CurrentDomain.ProcessExit += delegate {
                manager.Stop();
                manager.Dispose();
            };
            Console.ReadLine();
        }

    }

}
