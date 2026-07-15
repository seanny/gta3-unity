using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace GTA3Unity.Utility
{
    public static class StringExt
    {
        public static string GetNullTerminatedString(this byte[] data)
        {
            int length = Array.IndexOf(data, byte.MinValue);

            if (length == -1)
            {
                length = data.Length;
            }

            return Encoding.ASCII.GetString(data, 0, length);
        }

        public static string ReplaceInvalidSlash(string filename)
        {
            string normalized = filename.Replace(@"\", "_");

            foreach (char invalidCharacter in Path.GetInvalidFileNameChars())
            {
                normalized = normalized.Replace(invalidCharacter.ToString(), string.Empty);
            }

            return normalized.Replace("_", "/");
        }
    }
}
