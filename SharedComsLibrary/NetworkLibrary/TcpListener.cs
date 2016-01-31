using System;
using System.Net;
using System.Net.Sockets;

namespace NetworkLibrary {
    /// <summary>
    /// Represents a TCP <see cref="System.Net.Sockets.Socket"/> that listens for connections.
    /// This class is fully asynchronous. TODO: Not really
    /// </summary>
    /// <remarks>
    /// TODO: Remarks
    /// </remarks>
    public class TcpListener : IDisposable {

        /// <summary>
        /// Gets a value indicating whether the <see cref="TcpListener"/> is listening for connections.
        /// </summary>
        public bool IsListening { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="TcpListener"/> is disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Gets a value indicating what port the <see cref="TcpListener"/> will be listening on once <see cref="Start"/> has been called.
        /// </summary>
        public int Port { get; }

        public delegate void SocketAcceptedHandler(SocketAcceptedEventArgs e);
        /// <summary>
        /// Occurs when an incomming <see cref="System.Net.Sockets.Socket"/> connection is accepted.
        /// </summary>
        public event SocketAcceptedHandler SocketAccepted;

        /// <summary>
        /// Gets the <see cref="System.Net.Sockets.Socket"/> that the <see cref="TcpListener"/> will be using to listen on.
        /// </summary>
        public readonly Socket Socket;

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpListener"/> class using the specified port.
        /// </summary>
        /// <param name="port">Port that the listener should listen on.</param>
        public TcpListener(int port) {
            Port = port;
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        #region Public Methods
        /// <summary>
        /// Starts the <see cref="TcpListener"/>.
        /// </summary>
        public void Start() {
            if (IsListening)
                return;
            Socket.Bind(new IPEndPoint(IPAddress.Any, Port));
            Socket.Listen(0);
            Socket.BeginAccept(Callback, Socket);
            IsListening = true;
        }

        /// <summary>
        /// Stops the <see cref="TcpListener"/>.
        /// </summary>
        /// <remarks>
        /// This method does not dispose the socket. So the <see cref="Start"/> method can be called again.
        /// </remarks>
        public void Stop() {
            if (!IsListening)
                return;
            if (IsDisposed)
                return;
            Dispose(true);
        }
        #endregion

        #region Private Methods
        private void Callback(IAsyncResult result) {
            try {
                Socket s = Socket.EndAccept(result);
                SocketAccepted?.Invoke(new SocketAcceptedEventArgs(s));
                Socket.BeginAccept(Callback, Socket);
            }
            catch {
                //Console.WriteLine("Unhandled exception in TcpListener class : TODO");
                
                throw;
                // TODO: Exception handling
            }
        }

        #endregion
        #region IDisposable implementation members
        /// <summary>
        /// Releases all the resources used by the <see cref="TcpListener"/> object.
        /// </summary>
        /// <param name="disposing">Value indicating whether to dispose of managed resources.</param>
        protected virtual void Dispose(bool disposing) {
            if (IsDisposed)
                return;
            if (disposing) {
                Socket.Close();
            }
            IsDisposed = true;
        }
        /// <summary>
        /// Releases all the resources used by the <see cref="TcpListener"/> object.
        /// </summary>
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
