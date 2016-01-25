using System;
using System.Net;
using LobbyController.Com_Handler.Commands;
using LobbyController.Com_Handler.DataProcessing;
using LobbyController.Interfaces;
using NetworkLibrary;

namespace LobbyController.Com_Handler {
    internal class CommunicationHandler : IDisposable {

        private readonly UdpClient _client;
        private readonly DataProcessor _processor;

        public CommunicationHandler(IInvokable invokable) {
            _processor = new DataProcessor(_client, invokable);

            _client = new UdpClient(Properties.Settings.Default.DefaultPort);
            _client.DataReceived += UdpClient_DataReceived;
            _client.Start();

        }

        private void UdpClient_DataReceived(UdpDataReceivedEventArgs e) {
            if (e.ReceivedString.Length > 0)
                _processor.ProcessMessage(e.Sender, e.ReceivedString);

            return;
            //TODO: Check for length if > 0 send to DataProcessor
            string[] readData = e.ReceivedString.Split(':');
            switch (readData[0]) {
                case "Request":
                    HandleRequests(readData, e);
                    break;
                default:
                    Console.WriteLine($"UDP Client received a message that could not be handled : {e.ReceivedString}");
                    return;
            }

        }

        private void HandleRequests(string[] values, UdpDataReceivedEventArgs e) {

            if (values.Length < 2) {
                Console.WriteLine($"Invalid Request message : {e.ReceivedString}");
                return;
            }
            switch (values[1]) {
                case "CreateLobby":
                    Create(e.Sender);
                    break;
                default:
                    Console.WriteLine($"No such request exists : {e.ReceivedString}");
                    return;
            }
        }

        private async void Create(IPEndPoint sender) {
            IPEndPoint lobby = await _manager.CreateLobby();
            if (lobby != null)
                _client.SendResponse(sender, lobby);
            else
                _client.SendError(sender, Resources.ErrorStrings.MaxLobbiesReached);
        }

        #region IDisposable Implementation Members

        protected void Dispose(bool disposing) {
            if (disposing)
                _client.Dispose();
        }
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
