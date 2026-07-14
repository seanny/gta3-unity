using RenderWareIo.ReadWriteHelpers;
using RenderWareIo.Structs.Common;
using System.Collections.Generic;
using System.IO;

namespace RenderWareIo.Structs.Dff.Plugins
{
    public class HAnimPlugin : IExtensionPlugin
    {
        public const uint PluginId = 0x011E;

        public ChunkHeader Header { get; set; }
        public int Version { get; set; }
        public int NodeId { get; set; }
        public int NodeCount { get; set; }
        public int Flags { get; set; }
        public int KeyFrameSize { get; set; }
        public List<HAnimNodeInfo> Nodes { get; set; }

        public byte[] Bytes
        {
            get
            {
                using MemoryStream stream = new MemoryStream();
                Write(stream);
                return stream.ToArray();
            }
        }

        public int Type => (int)PluginId;
        public uint ByteCountWithHeader => Header.Size + 12;

        public HAnimPlugin()
        {
            Header = new ChunkHeader(PluginId);
            Nodes = new List<HAnimNodeInfo>();
        }

        public void Read(Stream stream)
        {
            Header = new ChunkHeader().Read(stream);
            long end = stream.Position + Header.Size;

            Version = ReadInt32(stream);
            NodeId = ReadInt32(stream);
            NodeCount = ReadInt32(stream);
            Nodes = new List<HAnimNodeInfo>();

            if (NodeCount > 0 && stream.Position + 8 <= end)
            {
                Flags = ReadInt32(stream);
                KeyFrameSize = ReadInt32(stream);

                for (int i = 0; i < NodeCount && stream.Position + 12 <= end; i++)
                {
                    Nodes.Add(new HAnimNodeInfo
                    {
                        NodeId = ReadInt32(stream),
                        NodeIndex = ReadInt32(stream),
                        Flags = ReadInt32(stream)
                    });
                }
            }

            stream.Position = end;
        }

        public void Write(Stream stream)
        {
            Header.Write(stream);
            RenderWareFileHelper.WriteUint32(stream, unchecked((uint)Version));
            RenderWareFileHelper.WriteUint32(stream, unchecked((uint)NodeId));
            RenderWareFileHelper.WriteUint32(stream, unchecked((uint)NodeCount));

            if (NodeCount > 0)
            {
                RenderWareFileHelper.WriteUint32(stream, unchecked((uint)Flags));
                RenderWareFileHelper.WriteUint32(stream, unchecked((uint)KeyFrameSize));

                foreach (HAnimNodeInfo node in Nodes)
                {
                    RenderWareFileHelper.WriteUint32(stream, unchecked((uint)node.NodeId));
                    RenderWareFileHelper.WriteUint32(stream, unchecked((uint)node.NodeIndex));
                    RenderWareFileHelper.WriteUint32(stream, unchecked((uint)node.Flags));
                }
            }
        }

        private static int ReadInt32(Stream stream)
        {
            return unchecked((int)RenderWareFileHelper.ReadUint32(stream));
        }
    }
}
