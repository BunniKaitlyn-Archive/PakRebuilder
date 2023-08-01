using System.Collections.Generic;

namespace PakRebuilder
{
    public static class ByteArrayExtensions
    {
        private static readonly int[] _empty = new int[0];

        public static int[] Locate(this byte[] self, byte[] candidate)
        {
            if (IsEmptyLocate(self, candidate))
                return _empty;

            var list = new List<int>();
            for (var i = 0; i < self.Length; i++)
            {
                if (!IsMatch(self, i, candidate))
                    continue;

                list.Add(i);
            }

            return list.Count == 0 ? _empty : list.ToArray();
        }

        private static bool IsMatch(byte[] array, int position, byte[] candidate)
        {
            if (candidate.Length > (array.Length - position))
                return false;

            for (var i = 0; i < candidate.Length; i++)
            {
                if (array[position + i] != candidate[i])
                    return false;
            }

            return true;
        }

        private static bool IsEmptyLocate(byte[] array, byte[] candidate)
        {
            return array == null ||
                candidate == null ||
                array.Length == 0 ||
                candidate.Length == 0 ||
                candidate.Length > array.Length;
        }
    }
}
