using System;
using System.Net;
using System.Text;

namespace NetworkLibrary {
    public sealed class UdpDataReceivedEventArgs : EventArgs {
        internal UdpDataReceivedEventArgs(IPEndPoint sender, byte[] receivedData, Encoding encoding) {
            ReceivedData = receivedData;
            Sender = sender;
            Encoding = encoding;
        }

        private string _receivedString;
        /// <summary>
        /// A value containing the <see cref="IPEndPoint"/> of the data sender.
        /// </summary>
        public IPEndPoint Sender { get; set; }
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
