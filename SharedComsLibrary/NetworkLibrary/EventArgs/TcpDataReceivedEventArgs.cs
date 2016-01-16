using System;
using System.Text;

namespace NetworkLibrary {
    public sealed class TcpDataReceivedEventArgs : EventArgs {
        internal TcpDataReceivedEventArgs(TcpClient sender, byte[] receivedData, Encoding encoding) {
            ReceivedData = receivedData;
            Sender = sender;
            Encoding = encoding;
        }

        private string _receivedString;
        public TcpClient Sender { get; set; }
        /// <summary>
        /// A value containing the received data in the form of a byte array.
        /// </summary>
        public byte[] ReceivedData { get; set; }
        /// <summary>
        /// A value containing the encoding used to convert the <see cref="ReceivedData"/>.
        /// </summary>
        public Encoding Encoding { get; set; }
        /// <summary>
        /// A value containing the received data in the form of a <see cref="string"/>.
        /// </summary>
        public string ReceivedString => _receivedString ?? (_receivedString = Encoding.GetString(ReceivedData));
    }
}
