using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO.Pipes;
using System.Net;
using System.Threading;

namespace LobbyController {

    internal class Program {

        [STAThread]
        private static void Main() {
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.RemoteIP))
                throw new NullReferenceException("IP was not set in settings file.");
            new LobbyManagerStart();

            Console.ReadLine();

        }

    }

}
