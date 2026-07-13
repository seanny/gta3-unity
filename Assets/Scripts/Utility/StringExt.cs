using System;
using System.IO;
using System.Text;
using UnityEditor.Rendering;

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

        public static string ReplaceInvalidSlash(string filename)
        {
            // Hack to fix illegal character error when using Path.Combine on paths inside of DAT files. If you know a better method, be my guest.
            return filename.Replace(@"\", "_").ReplaceInvalidFileNameCharacters("").Replace("_", "/");
        }
    }
}