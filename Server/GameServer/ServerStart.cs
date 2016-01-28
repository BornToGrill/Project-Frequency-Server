using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Security.Principal;

namespace Lobby {
    public class Lobby {

        public static void Main(string[] args) {
            if (args.Length <= 0)
                throw new Exception("Lobby was not started by a named pipe server.");
            string[] param = args[0].Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);

            var dict = new Dictionary<string, string>();
            for (int i = 0; i < args.Length; i += 2) {
                if (i + 1 >= args.Length)
                    break;
                dict.Add(args[i].TrimStart('-'), args[i + 1]);
            }

            ServerLobby lobby = new ServerLobby(dict["name"], int.Parse(dict["port"]));
            Console.WriteLine("Server lobby started");

            // TODO: First startup lobby then connect.
            if (dict["name"] != "Headless-Client") { // TODO: Remove ! Params are set in Lobby debug settings.
                NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", dict["name"], PipeDirection.InOut,
                    PipeOptions.None, TokenImpersonationLevel.Impersonation);
                pipeClient.Connect(15000); //TODO: Timeout?
                Console.WriteLine($"Client succesfully connected to pipe name : {dict["name"]}");
            }

            Console.ReadLine();

        }
    }

}
