using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NetworkLibrary {
    /// <summary>
    /// TODO : Summary
    /// </summary>
    /// <remarks>
    /// TODO: Remarks
    /// </remarks>
    public class UdpClient : IDisposable {

        /// <summary>
        /// Gets a value indicating whether the <see cref="UdpClient"/> is disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="UdpClient"/> is bound to the specified port.
        /// </summary>
        public bool IsListening { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="UdpClient"/> is receiving data.
        /// </summary>
        public bool IsReceiving { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="UdpClient"/> is connected to an <see cref="EndPoint"/>.
        /// </summary>
        public bool Connected => Socket.Connected;

        /// <summary>
        /// Gets a value indicating what port the <see cref="UdpClient"/> will be listening on once <see cref="Start"/> has been called.
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// Gets the <see cref="System.Net.Sockets.Socket"/> that the <see cref="UdpClient"/> will be using.
        /// </summary>
        public Socket Socket { get; }

        /// <summary>
        /// Gets a value indicating what <see cref="System.Text.Encoding"/> the <see cref="UdpClient"/> will be using. 
        /// </summary>
        public Encoding Encoding { get; } = new ASCIIEncoding();

        /// <summary>
        /// Provides data for the <see cref="DataReceived"/> event.
        /// </summary>
        public delegate void DataReceivedEventHandler(UdpDataReceivedEventArgs e);

        /// <summary>
        /// Provides data for the <see cref="Disconnected"/> event.
        /// </summary>
        public delegate void ClientDisconnectedEventHandler(IPEndPoint client);

        /// <summary>
        /// Occurs when the <see cref="UdpClient"/> has received data.
        /// </summary>
        public event DataReceivedEventHandler DataReceived;

        /// <summary>
        /// Occurs when an attempt to send data was unsuccessful.
        /// </summary>
        public event ClientDisconnectedEventHandler Disconnected;   // TODO: Actually make this show disconnected clients.

        /// <summary>
        /// Gets a value indicating what the buffer size of the <see cref="UdpClient"/> is.
        /// </summary>
        public int BufferSize { get; private set; } = 8192;

        private byte[] _buffer = new byte[8192];


        /// <summary>
        /// Initializes a new instance of the <see cref="UdpClient"/> class.
        /// </summary>
        public UdpClient() : this(AddressFamily.InterNetwork) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="UdpClient"/> class using the specified port.
        /// </summary>
        /// <param name="port">Port used for communications.</param>
        public UdpClient(int port) : this(port, AddressFamily.InterNetwork) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="UdpClient"/> class using the specified <see cref="AddressFamily"/>.
        /// </summary>
        /// <param name="addressFamily"><see cref="AddressFamily"/> that the should be used.</param>
        public UdpClient(AddressFamily addressFamily) {
            Socket = new Socket(addressFamily, SocketType.Dgram, ProtocolType.Udp);
            System.Net.Sockets.UdpClient x = new System.Net.Sockets.UdpClient();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="UdpClient"/> class using the specified port and <see cref="AddressFamily"/>.
        /// </summary>
        /// <param name="port"></param>
        /// <param name="addressFamily"></param>
        public UdpClient(int port, AddressFamily addressFamily) {
            Socket = new Socket(addressFamily, SocketType.Dgram, ProtocolType.Udp);
            Port = port;
            Socket.Bind(new IPEndPoint(IPAddress.Any, port));
            IsListening = true;
        }

        #region Public Methods
        /// <summary>
        /// Begins to asynchronously receive data.
        /// </summary>
        public void Start() {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().FullName);
            if(IsReceiving)
                throw new InvalidOperationException($"The {GetType().FullName} is already receiving data.");    // TODO : Globalization
            EndPoint remoteEndPoint = Socket.AddressFamily == AddressFamily.InterNetwork
                ? new IPEndPoint(IPAddress.Any, 0)
                : new IPEndPoint(IPAddress.IPv6Any, 0);
            Socket.BeginReceiveFrom(_buffer, 0, BufferSize, 0, ref remoteEndPoint, Callback, remoteEndPoint);
            IsReceiving = true;
        }

        /// <summary>
        /// Begins to asynchronously receive data using the specified buffer size.
        /// </summary>
        /// <param name="bufferSize">Size that the receiving buffer should be.</param>
        public void Start(int bufferSize) {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (bufferSize < 0)
                throw new ArgumentOutOfRangeException(nameof(bufferSize), bufferSize, $"{bufferSize} can not be less than 0");
            BufferSize = bufferSize;
            _buffer = new byte[bufferSize];
            Start();
        }

        /// <summary>
        /// Closes the UDP connection.
        /// </summary>
        public void Close() {
            Dispose(true);
        }

        /// <summary>
        /// Connects to a specified target <see cref="IPEndPoint"/>.
        /// </summary>
        /// <param name="targetHost"><see cref="IPEndPoint"/> used for the connection.</param>
        public void Connect(IPEndPoint targetHost) {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (targetHost == null)
                throw new ArgumentNullException(nameof(targetHost));
            Socket.Connect(targetHost);
            // TODO : Exception handling
        }
        /// <summary>
        /// Sends the specified data to a connected <see cref="System.Net.Sockets.Socket"/>.
        /// </summary>
        /// <param name="data">An <see cref="Array"/> of type <see cref="byte"/> that contains the data to be send.</param>
        public void Send(byte[] data) {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            Socket.Send(data);
        }
        /// <summary>
        /// Sends the specified data to a connected <see cref="System.Net.Sockets.Socket"/>.
        /// </summary>
        /// <param name="data">A <see cref="string"/> that contains the data to be send.</param>
        public void Send(string data) {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            Socket.Send(Encoding.GetBytes(data));
        }
        /// <summary>
        /// Sends the specified data to the specified <see cref="IPEndPoint"/>.
        /// </summary>
        /// <param name="data">An <see cref="Array"/> of type <see cref="byte"/> that contains the data to be send.</param>
        /// <param name="target">The <see cref="IPEndPoint"/> that the data should be send to.</param>
        public void Send(byte[] data, IPEndPoint target) {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            Socket.SendTo(data, target);
        }
        /// <summary>
        /// Sends the specified data to the specified <see cref="IPEndPoint"/>.
        /// </summary>
        /// <param name="data">A <see cref="string"/> that contains the data to be send.</param>
        /// <param name="target">The <see cref="IPEndPoint"/> that the data should be send to.</param>
        public void Send(string data, IPEndPoint target) {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            Socket.SendTo(Encoding.GetBytes(data), target);
        }

        #endregion
        #region Private Methods
        private void Callback(IAsyncResult result) {
            EndPoint sender = Socket.AddressFamily == AddressFamily.InterNetwork
                    ? new IPEndPoint(IPAddress.Any, 0)
                    : new IPEndPoint(IPAddress.IPv6Any, 0);
            try {
                int received = Socket.EndReceiveFrom(result, ref sender);
                if (received > 0) {
                    if (received > BufferSize)
                        throw new OverflowException("More data was received than the UDP buffer can store.");
                            // TODO : Globalization
                    byte[] data = new byte[received];
                    Buffer.BlockCopy(_buffer, 0, data, 0, received);
                    DataReceived?.Invoke(new UdpDataReceivedEventArgs((IPEndPoint) sender, data, Encoding));
                }
                else {
                    Disconnected?.Invoke((IPEndPoint)sender);
                }
                Socket.BeginReceiveFrom(_buffer, 0, BufferSize, 0, ref sender, Callback, sender);
            }
            catch (SocketException) {
                Disconnected?.Invoke((IPEndPoint)sender);
                Close();
            }
        }
        #endregion

        #region IDisposable implementation members
        /// <summary>
        /// Releases all the resources used by the <see cref="UdpClient"/> object.
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
        /// Releases all the resources used by the <see cref="UdpClient"/> object.
        /// </summary>
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }


}
