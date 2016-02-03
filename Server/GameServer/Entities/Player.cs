using System;
using System.Net;
using TcpClient = NetworkLibrary.TcpClient;
namespace Lobby.Entities {
    internal class Player : IDisposable {

        internal string Name { get; set; }
        internal int CornerId { get; set; }
        internal string Guid { get; }

        internal bool IsHost { get; set; }
        internal bool Ready { get; set; }
        internal bool GameLoaded { get; set; }
        internal bool IsAlive { get; set; } = true;

        internal TcpClient TcpClient { get; }
        internal IPEndPoint ClientIp { get; }
        internal bool UdpVerified { get; set; }
        private volatile bool _isDisposed;

        public Player(Guid guid, TcpClient tcpClient) : this(guid.ToString(), tcpClient) { }
        public Player(string guid, TcpClient tcpClient) {
            Guid = guid;
            TcpClient = tcpClient;
            ClientIp = (IPEndPoint) tcpClient.Socket.RemoteEndPoint;
        }

        public void Reset() {
            Ready = false;
            GameLoaded = false;
        }

        #region IDisposable Implementation Members
        private void Dispose(bool disposing) {
            if (_isDisposed)
                return;
            if(disposing)
                TcpClient?.Dispose();
            _isDisposed = true;
        }
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}
