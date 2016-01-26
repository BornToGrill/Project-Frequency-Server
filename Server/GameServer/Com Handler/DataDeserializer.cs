using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lobby.Com_Handler {
    internal static class DataDeserializer {

        private const char DataSeparator = ':';

        internal static Tuple<string, string> GetFirst(this string data) {
            int index = data.IndexOf(DataSeparator);
            if (index < 0)
                throw new ArgumentException("string did not contain a data separator");
            return Tuple.Create(data.Substring(0, index), data.Substring(index + 1));
        }

        internal static Tuple<string, string[]> SplitFirst(this string data) {
            int index = data.IndexOf(DataSeparator);
            if (index < 0)
                throw new ArgumentException("string did not contain a data separator");
            return Tuple.Create(data.Substring(0, index), data.Substring(index + 1).Split(DataSeparator));
        }
    }
}
