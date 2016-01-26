using System;
using System.Net.Sockets;
using System.Text;

namespace NetworkLibrary {
    /// <summary>
    /// TODO: Summary
    /// </summary>
    /// <remarks>
    /// TODO: Remarks
    /// </remarks>
    public class TcpClient : IDisposable {

        /// <summary>
        /// Gets a value indicating whether the <see cref="TcpClient"/> object has been disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }
        /// <summary>
        /// Gets a value indicating what <see cref="System.Text.Encoding"/> the <see cref="UdpClient"/> will be using. 
        /// </summary>
        public Encoding Encoding { get; } = new ASCIIEncoding();

        /// <summary>
        /// Provides data for the <see cref="DataReceived"/> event.
        /// </summary>
        public delegate void DataReceivedEventHandler(TcpDataReceivedEventArgs e);

        /// <summary>
        /// Provides data for the <see cref="Disconnected"/> event.
        /// </summary>
        public delegate void ClientDisconnectedHandler(TcpClient sender);

        /// <summary>
        /// Occurs when the <see cref="TcpClient"/> has received data.
        /// </summary>
        public event DataReceivedEventHandler DataReceived;
        /// <summary>
        /// Occurs when the connection with the <see cref="System.Net.Sockets.Socket"/> has been lost.
        /// </summary>
        public event ClientDisconnectedHandler Disconnected;

        /// <summary>
        /// Gets the <see cref="System.Net.Sockets.Socket"/> that the <see cref="TcpClient"/> will be using.
        /// </summary>
        public Socket Socket { get; }
        private const int BufferSize = 8192;    // TODO: Variable buffer size

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpClient"/> class using the specified <see cref="System.Net.Sockets.Socket"/>.
        /// </summary>
        /// <param name="socket">Socket used for the data transfer of the <see cref="TcpClient"/> object.</param>
        public TcpClient(Socket socket) {
            Socket = socket;
        }

        #region Public Methods
        /// <summary>
        /// Begins to asynchronously receive data.
        /// </summary>
        public void Start() {
            if(IsDisposed)
                throw new ObjectDisposedException(GetType().FullName);
            Socket.BeginReceive(new byte[] { 0 }, 0, 0, 0, Callback, Socket);
        }

        /// <summary>
        /// Sends the specified data to a connected <see cref="System.Net.Sockets.Socket"/>.
        /// </summary>
        /// <param name="data">An <see cref="Array"/> of type <see cref="byte"/> that contains the data to be send.</param>
        public void Send(byte[] data) {
            Socket.Send(data);
        }

        /// <summary>
        /// Sends the specified data to a connected <see cref="System.Net.Sockets.Socket"/>.
        /// </summary>
        /// <param name="data">A <see cref="string"/> that contains the data to be send.</param>
        public void Send(string data) {
            Socket.Send(Encoding.GetBytes(data));
        }

        /// <summary>
        /// Closes the TCP connection.
        /// </summary>
        public void Close() {
            if (!IsDisposed)
                return;
            Dispose(true);
        }
        #endregion

        #region Private Methods
        private void Callback(IAsyncResult result) {
            try {
                Socket.EndReceive(result);

                byte[] buffer = new byte[BufferSize];
                int received = Socket.Receive(buffer, buffer.Length, 0);

                if (buffer.Length > 0) {
                    if (received < buffer.Length)
                        Array.Resize(ref buffer, received);
                    DataReceived?.Invoke(new TcpDataReceivedEventArgs(this, buffer, Encoding));
                }
                else {
                    Disconnected?.Invoke(this);
                    Close();
                    return;
                }

                Socket.BeginReceive(new byte[] { 0 }, 0, 0, 0, Callback, Socket);
            }
            catch (SocketException) {
                Disconnected?.Invoke(this);
                Close();
            }
        }
        #endregion

        #region IDisposable Implementation Members
        /// <summary>
        /// Releases all the resources used by the <see cref="TcpClient"/> object.
        /// </summary>
        /// <param name="disposing">Value indicating whether to dispose of managed resources.</param>
        protected virtual void Dispose(bool disposing) {
            if (IsDisposed)
                return;
            if (disposing) {
                Socket?.Close();
            }
            IsDisposed = true;
        }
        /// <summary>
        /// Releases all the resources used by the <see cref="TcpClient"/> object.
        /// </summary>
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}