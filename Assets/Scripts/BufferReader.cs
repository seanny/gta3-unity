using System;
using System.IO;
using System.Text;

namespace GTA3Unity
{
    public sealed class BufferReader : IDisposable
    {
        private readonly BinaryReader m_Reader;

        public Stream BaseStream => m_Reader.BaseStream;

        public long Position
        {
            get { return BaseStream.Position; }
            set { BaseStream.Position = value; }
        }

        public long Length => BaseStream.Length;

        public BufferReader(Stream stream)
        {
            m_Reader = new BinaryReader(stream);
        }

        public void PrewarmBuffer(int count)
        {
        }

        public void Skip(int count)
        {
            Position += count;
        }

        public void Skip(long count)
        {
            Position += count;
        }

        public void SkipStream(int count)
        {
            Position += count;
        }

        public void SkipStream(long count)
        {
            Position += count;
        }

        public bool ReadBoolean()
        {
            return m_Reader.ReadBoolean();
        }

        public byte ReadByte()
        {
            return m_Reader.ReadByte();
        }

        public byte[] ReadBytes(int count)
        {
            return m_Reader.ReadBytes(count);
        }

        public char ReadChar()
        {
            return m_Reader.ReadChar();
        }

        public double ReadDouble()
        {
            return m_Reader.ReadDouble();
        }

        public short ReadInt16()
        {
            return m_Reader.ReadInt16();
        }

        public int ReadInt32()
        {
            return m_Reader.ReadInt32();
        }

        public long ReadInt64()
        {
            return m_Reader.ReadInt64();
        }

        public float ReadSingle()
        {
            return m_Reader.ReadSingle();
        }

        public string ReadString()
        {
            return m_Reader.ReadString();
        }

        public string ReadString(int size)
        {
            if (size <= 0)
            {
                return string.Empty;
            }

            return Encoding.ASCII.GetString(ReadBytes(size), 0, size);
        }

        public ushort ReadUInt16()
        {
            return m_Reader.ReadUInt16();
        }

        public uint ReadUInt32()
        {
            return m_Reader.ReadUInt32();
        }

        public ulong ReadUInt64()
        {
            return m_Reader.ReadUInt64();
        }

        public void Dispose()
        {
            m_Reader.Close();
        }
    }
}
