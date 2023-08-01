using System;
using System.Globalization;

namespace PakRebuilder
{
    public class PakRecord
    {
        public string FileName { get; private set; }
        public byte[] DataHash { get; private set; }
        public ulong Offset { get; private set; }
        public ulong Size { get; private set; }

        public PakRecord(string source)
        {
            var split = source.Split('|');

            FileName = split[0];
            DataHash = FastStringToByteArray(split[1]);
            Offset = ulong.Parse(split[2], NumberStyles.HexNumber);
            Size = ulong.Parse(split[3], NumberStyles.HexNumber);
        }

        private byte[] FastStringToByteArray(string hex)
        {
            if (hex.Length % 2 == 1)
                throw new Exception("The binary key cannot have an odd number of digits.");

            var result = new byte[hex.Length >> 1];
            for (var i = 0; i < hex.Length >> 1; ++i)
                result[i] = (byte) ((GetHexValue(hex[i << 1]) << 4) + GetHexValue(hex[(i << 1) + 1]));

            return result;
        }

        private int GetHexValue(char hex)
        {
            var value = (int) hex;
            return value - (value < 58 ? 48 : (value < 97 ? 55 : 87));
        }
    }
}
