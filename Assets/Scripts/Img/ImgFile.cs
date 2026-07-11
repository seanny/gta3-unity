using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GTA3Unity.Utility;
using UnityEngine;

namespace GTA3Unity.Img
{
    // Credits: mukaschultze
    public class ImgFile : IEnumerable<FileEntry> {
        public const string IMG_MAIN = "models/gta3.img";

        private bool loaded;
        private int entriesCount;
        private FileEntry[] entriesArray;
        private Dictionary<string, FileEntry> entries;
        private BufferReader reader;

        public int EntriesCount { get { if(!loaded) LoadEntries(); return entriesCount; } }
        public string FilePath { get { return ArchiveFile.FilePath; } }

        public FileEntry ArchiveFile { get; private set; }
        public FileEntry[] Entries { get { if(!loaded) LoadEntries(); return entriesArray; } }
        public FileEntry this[int index] { get { if(!loaded) LoadEntries(); return entriesArray[index]; } }
        public FileEntry this[string fileName] { get { if(!loaded) LoadEntries(); return entries[fileName]; } }

        public ImgFile(string path) : this(new FileEntry(path)) { }

        public ImgFile(FileEntry file) {
            ArchiveFile = file;
        }

        public bool Contains(string fileName) {
            if(!loaded)
                LoadEntries();

            return entries.ContainsKey(fileName);
        }

        private void LoadEntries()
        {
            var dirPath = Path.ChangeExtension(ArchiveFile.FilePath, "dir");

            if(!File.Exists(dirPath))
            {
                throw new FileNotFoundException(string.Format("There should be a .dir file along the \"{0}\" file", ArchiveFile.FileName), ArchiveFile.FileName);
            }

            reader = new BufferReader(new FileStream(dirPath, FileMode.Open));
            entriesCount = (int)reader.Length / 32;

            entries = new Dictionary<string, FileEntry>(entriesCount, StringComparer.OrdinalIgnoreCase);
            reader.PrewarmBuffer(entriesCount * 32);

            Debug.Log($"Reading {entriesCount} entries from \"{FilePath}\"");

            for(var i = 0; i < entriesCount; i++)
            {
                var pos = reader.ReadInt32() * 2048;
                var length = reader.ReadInt32() * 2048;
                var name = reader.ReadBytes(24).GetNullTerminatedString();

                if(!entries.ContainsKey(name))
                {
                    try
                    {
                        entries.Add(name, new FileEntry(ArchiveFile, name, pos, length));
                    }
                    catch(Exception err)
                    {
                        Debug.LogError($"Fail to add entry \"{name}\" in \"{FilePath}\": {err}");
                    }
                }
                else
                {
                    Debug.LogError($"Duplicated entry name \"{name}\" in \"{FilePath}\"");
                }
            }

            if(entries.Count != entriesCount)
            {
                Debug.LogWarning($"Expected {entriesCount} entries, found {entries.Count} at \"{FilePath}\"");
            }
            else
            {
                Debug.Log("Loaded {entriesCount} entries at \"{FilePath}\"");
            }

            reader.Dispose(); //Dispose the DIR file, not the IMG

            entriesArray = entries.Values.ToArray();
            loaded = true;
        }

        public static ImgFile GetMainImg()
        {
            return new ImgFile(IMG_MAIN);
        }

        public static ImgFile GetMainImg(string gtaPath)
        {
            return new ImgFile(Path.Combine(gtaPath, IMG_MAIN));
        }

        IEnumerator<FileEntry> IEnumerable<FileEntry>.GetEnumerator()
        {
            return ((IEnumerable<FileEntry>)Entries).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<FileEntry>)Entries).GetEnumerator();
        }
    }
}