using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LobbyController {
    public class LobbyInstance : IDisposable {

        public readonly NamedPipeServerStream PipeServer;
        public readonly int Id;
        public readonly int Port;

        public delegate void ClientConnectedEventHandler(LobbyInstance sender, int clientId);
        public event ClientConnectedEventHandler ClientConnected;

        public delegate void ClientDisconnectedEventHandler(LobbyInstance sender);
        public event ClientDisconnectedEventHandler ClientDisconnected;

        public delegate void MessageReceivedEventHandler(LobbyInstance sender, byte[] message); //TODO: EventArgs
        public event MessageReceivedEventHandler MessageReceived;

        public TaskCompletionSource<IPEndPoint> LobbyInitialized = new TaskCompletionSource<IPEndPoint>();

        private readonly byte[] _buffer = new byte[8192];

        public LobbyInstance(int id, int port) {
            Id = id;
            Port = port;
            PipeServer = new NamedPipeServerStream(id.ToString(), PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
        }

        public void Start() {
            try {
                Console.WriteLine($"Lobby instance with ID : <{Id}> started waiting for connections.");
                PipeServer.BeginWaitForConnection(ConnectionCallback, null);
            }
            catch {
                throw;
            }
        }

        private void ConnectionCallback(IAsyncResult result) {
            PipeServer.EndWaitForConnection(result);
            ClientConnected?.Invoke(this, Id);
            LobbyInitialized?.TrySetResult(new IPEndPoint(IPAddress.Parse(Properties.Settings.Default.RemoteIP), Port));
            PipeServer.BeginRead(_buffer, 0, 0, ReadCallback, null);
        }

        private void ReadCallback(IAsyncResult result) {
            int received = PipeServer.EndRead(result);


            if (received > 0) {
                byte[] messageBytes = new byte[received];
                Buffer.BlockCopy(_buffer, 0, messageBytes, 0, received);
                MessageReceived?.Invoke(this, messageBytes);
            }
            else {
                ClientDisconnected?.Invoke(this);
                return;
            }

            PipeServer.BeginRead(_buffer, 0, 0, ReadCallback, null);
        }

        #region IDisposable Implementation Members

        protected void Dispose(bool disposing) {
            if (disposing)
                PipeServer.Dispose();
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
