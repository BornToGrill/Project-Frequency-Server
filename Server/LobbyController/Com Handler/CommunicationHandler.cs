using System;
using LobbyController.Com_Handler.DataProcessing;
using LobbyController.Interfaces;
using NetworkLibrary;

namespace LobbyController.Com_Handler {
    internal class CommunicationHandler : IDisposable {

        private readonly UdpClient _client;
        private readonly DataProcessor _processor;

        public CommunicationHandler(IInvokable invokable, IRequestable requestable) {
            _client = new UdpClient(Properties.Settings.Default.DefaultPort);
            _client.DataReceived += UdpClient_DataReceived;
            _client.Start();

            _processor = new DataProcessor(_client, invokable, requestable);

        }

        private void UdpClient_DataReceived(UdpDataReceivedEventArgs e) {
            if (e.ReceivedString.Length > 0)
                _processor.ProcessMessage(e.Sender, e.ReceivedString);

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
