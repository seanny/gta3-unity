using System;
using System.Text;

namespace GTA3Unity.Utility
{
    public static class StringExt
    {
        // Credits: mukaschultze
        public static string GetNullTerminatedString(this byte[] data)
        {
            var length = Array.IndexOf(data, byte.MinValue);

            if (length == -1)
                length = data.Length;

            return Encoding.ASCII.GetString(data, 0, length);
        }
    }
}