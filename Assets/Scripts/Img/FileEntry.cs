using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GTA3Unity.Img
{
    // Credits: mukaschultze
    public class FileEntry
    {
        public string FileName { get; private set; }
        public string FileNameWithoutExtension { get { return Path.GetFileNameWithoutExtension(FileName); } }
        public string FilePath { get; private set; }
        public int Offset { get; private set; }
        public int Size { get; private set; }
        public BufferReader Reader
        {
            get
            {
                reader.Position = Offset;
                return reader;
            }
        }

        private readonly BufferReader reader;
        private static readonly Dictionary<BufferReader, int> dependents = new Dictionary<BufferReader, int>();

        public FileEntry(string filePath)
        {
            Offset = 0;
            Size = (int)new FileInfo(filePath).Length;
            FilePath = filePath;
            FileName = Path.GetFileName(FilePath);
            reader = new BufferReader(new FileStream(FilePath, FileMode.Open));
            dependents[reader] = 1;
        }

        public FileEntry(FileEntry file, string virtualFileName, int offset, int size)
        {
            Size = size;
            Offset = file.Offset + offset;
            FilePath = file.FilePath;
            FileName = virtualFileName;

            // // uncomment this to not create new streams for each file
            // reader = file.reader;
            // dependents[reader]++;
            // return; 

            var pos = file.reader.Position;
            file.reader.Position = Offset;
            var data = file.reader.ReadBytes(Size);
            file.reader.Position = pos;
            reader = new BufferReader(new MemoryStream(data));
            Offset = 0;
            dependents[reader] = 1;
        }

        ~FileEntry()
        {
            if (--dependents[reader] > 0)
                return;

            try
            {
                dependents.Remove(reader);
                reader.Dispose();
                // Log.Message("Closed stream for file: {0}", FilePath);
            }
            catch
            {
                Debug.LogError($"Failed to close stream for file: {FilePath}");
            }
        }

        public byte[] GetData()
        {
            reader.Position = Offset;
            return reader.ReadBytes(Size);
        }
    }
}