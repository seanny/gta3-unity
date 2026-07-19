using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GTA3Unity.Utility;
using UnityEngine;

namespace GTA3Unity.Img
{
    public sealed class ImgFile : IEnumerable<FileEntry>
    {
        public const string MainImgPath = "models/gta3.img";

        private bool m_Loaded;
        private int m_EntriesCount;
        private FileEntry[] m_EntriesArray;
        private Dictionary<string, FileEntry> m_Entries;

        public int EntriesCount
        {
            get
            {
                if (!m_Loaded)
                {
                    LoadEntries();
                }

                return m_EntriesCount;
            }
        }

        public string FilePath => ArchiveFile.FilePath;

        public FileEntry ArchiveFile { get; private set; }
        public FileEntry[] Entries
        {
            get
            {
                if (!m_Loaded)
                {
                    LoadEntries();
                }

                return m_EntriesArray;
            }
        }

        public FileEntry this[int index]
        {
            get
            {
                if (!m_Loaded)
                {
                    LoadEntries();
                }

                return m_EntriesArray[index];
            }
        }

        public FileEntry this[string fileName]
        {
            get
            {
                if (!m_Loaded)
                {
                    LoadEntries();
                }

                return m_Entries[fileName];
            }
        }

        public ImgFile(string path)
            : this(new FileEntry(path))
        {
            
        }

        public ImgFile(FileEntry file)
        {
            ArchiveFile = file;
            LoadEntries();
        }

        public bool Contains(string fileName)
        {
            if (!m_Loaded)
            {
                LoadEntries();
            }

            return m_Entries.ContainsKey(fileName);
        }

        private void LoadEntries()
        {
            var timer = System.Diagnostics.Stopwatch.StartNew();
            string dirPath = Path.ChangeExtension(ArchiveFile.FilePath, "dir");

            if (!File.Exists(dirPath))
            {
                throw new FileNotFoundException(
                    $"There should be a .dir file alongside '{ArchiveFile.FileName}'.",
                    ArchiveFile.FileName);
            }

            using BufferReader reader = new BufferReader(new FileStream(dirPath, FileMode.Open));
            m_EntriesCount = (int)reader.Length / 32;

            m_Entries = new Dictionary<string, FileEntry>(m_EntriesCount, StringComparer.OrdinalIgnoreCase);
            reader.PrewarmBuffer(m_EntriesCount * 32);

            Debug.Log($"Reading {m_EntriesCount} entries from '{FilePath}'.");

            for (int entryIndex = 0; entryIndex < m_EntriesCount; entryIndex++)
            {
                int position = reader.ReadInt32() * 2048;
                int length = reader.ReadInt32() * 2048;
                string name = reader.ReadBytes(24).GetNullTerminatedString();

                if (!m_Entries.ContainsKey(name))
                {
                    try
                    {
                        m_Entries.Add(name, new FileEntry(ArchiveFile, name, position, length));
                    }
                    catch (Exception exception)
                    {
                        Debug.LogError($"Failed to add entry '{name}' in '{FilePath}': {exception.Message}");
                    }
                }
                else
                {
                    Debug.LogError($"Duplicated entry name '{name}' in '{FilePath}'.");
                }
            }

            if (m_Entries.Count != m_EntriesCount)
            {
                Debug.LogWarning($"Expected {m_EntriesCount} entries, found {m_Entries.Count} at '{FilePath}'.");
            }
            else
            {
                Debug.Log($"Loaded {m_EntriesCount} entries at '{FilePath}'.");
            }
            reader.Dispose();
            timer.Stop();
            Debug.Log($"Loaded {FilePath} in {timer.Elapsed.Milliseconds} ms");

            m_EntriesArray = m_Entries.Values.ToArray();
            m_Loaded = true;
        }

        public static ImgFile GetMainImg()
        {
            return new ImgFile(MainImgPath);
        }

        public static ImgFile GetMainImg(string gtaPath)
        {
            return new ImgFile(Path.Combine(gtaPath, MainImgPath));
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
