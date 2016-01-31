using System;
using System.Net;
using System.Text.RegularExpressions;

namespace LobbyController {

    internal class Program {
        private static void Main() {

            try {
                string ip = GetIp();
                Properties.Settings.Default.RemoteIP = ip;
                Properties.Settings.Default.Save();
            }
            catch { }
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.RemoteIP))
                throw new NullReferenceException("IP was not set in settings file.");


            LobbyManager manager = new LobbyManager();
            AppDomain.CurrentDomain.ProcessExit += delegate {
                manager.Stop();
                manager.Dispose();
            };
            Console.ReadLine();
        }

        public static string GetIp() {
            string externalIp = "";
            externalIp = (new WebClient()).DownloadString("http://checkip.dyndns.org/");
            externalIp = (new Regex(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}")).Matches(externalIp)[0].ToString();
            return externalIp;
        }

    }

}
