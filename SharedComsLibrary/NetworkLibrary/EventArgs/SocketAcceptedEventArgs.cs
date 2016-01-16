using System;
using System.Net.Sockets;

namespace NetworkLibrary {
    public sealed class SocketAcceptedEventArgs : EventArgs {
        internal SocketAcceptedEventArgs(Socket socket) {
            Socket = socket;
        }
        public Socket Socket { get; }
    }
}
