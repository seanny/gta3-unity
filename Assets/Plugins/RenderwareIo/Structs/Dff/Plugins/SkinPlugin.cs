using RenderWareIo.ReadWriteHelpers;
using RenderWareIo.Structs.Common;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace RenderWareIo.Structs.Dff.Plugins
{
    public class SkinPlugin : IExtensionPlugin
    {
        public const uint PluginId = 0x0116;

        public ChunkHeader Header { get; set; }
        public byte BoneCount { get; set; }
        public byte UsedBoneCount { get; set; }
        public byte MaxWeightsPerVertex { get; set; }
        public byte Flags { get; set; }
        public List<byte> UsedBones { get; set; }
        public List<byte[]> VertexBoneIndices { get; set; }
        public List<float[]> VertexBoneWeights { get; set; }
        public List<Matrix4x4> InverseBindMatrices { get; set; }

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

        public SkinPlugin()
        {
            Header = new ChunkHeader(PluginId);
            UsedBones = new List<byte>();
            VertexBoneIndices = new List<byte[]>();
            VertexBoneWeights = new List<float[]>();
            InverseBindMatrices = new List<Matrix4x4>();
        }

        public void Read(Stream stream)
        {
            Read(stream, -1);
        }

        public void Read(Stream stream, int vertexCount)
        {
            Header = new ChunkHeader().Read(stream);
            long end = stream.Position + Header.Size;

            BoneCount = RenderWareFileHelper.ReadByte(stream);
            UsedBoneCount = RenderWareFileHelper.ReadByte(stream);
            MaxWeightsPerVertex = RenderWareFileHelper.ReadByte(stream);
            Flags = RenderWareFileHelper.ReadByte(stream);

            UsedBones = new List<byte>();
            for (int i = 0; i < UsedBoneCount && stream.Position < end; i++)
            {
                UsedBones.Add(RenderWareFileHelper.ReadByte(stream));
            }

            VertexBoneIndices = new List<byte[]>();
            VertexBoneWeights = new List<float[]>();

            if (vertexCount > 0 && stream.Position + vertexCount * 20 <= end)
            {
                for (int vertexIndex = 0; vertexIndex < vertexCount; vertexIndex++)
                {
                    VertexBoneIndices.Add(new[]
                    {
                        RenderWareFileHelper.ReadByte(stream),
                        RenderWareFileHelper.ReadByte(stream),
                        RenderWareFileHelper.ReadByte(stream),
                        RenderWareFileHelper.ReadByte(stream)
                    });
                }

                for (int vertexIndex = 0; vertexIndex < vertexCount; vertexIndex++)
                {
                    VertexBoneWeights.Add(new[]
                    {
                        RenderWareFileHelper.ReadFloat(stream),
                        RenderWareFileHelper.ReadFloat(stream),
                        RenderWareFileHelper.ReadFloat(stream),
                        RenderWareFileHelper.ReadFloat(stream)
                    });
                }
            }

            InverseBindMatrices = new List<Matrix4x4>();
            while (stream.Position + 64 <= end)
            {
                InverseBindMatrices.Add(new Matrix4x4(
                    RenderWareFileHelper.ReadFloat(stream),
                    RenderWareFileHelper.ReadFloat(stream),
                    RenderWareFileHelper.ReadFloat(stream),
                    RenderWareFileHelper.ReadFloat(stream),
                    RenderWareFileHelper.ReadFloat(stream),
                    RenderWareFileHelper.ReadFloat(stream),
                    RenderWareFileHelper.ReadFloat(stream),
                    RenderWareFileHelper.ReadFloat(stream),
                    RenderWareFileHelper.ReadFloat(stream),
                    RenderWareFileHelper.ReadFloat(stream),
                    RenderWareFileHelper.ReadFloat(stream),
                    RenderWareFileHelper.ReadFloat(stream),
                    RenderWareFileHelper.ReadFloat(stream),
                    RenderWareFileHelper.ReadFloat(stream),
                    RenderWareFileHelper.ReadFloat(stream),
                    RenderWareFileHelper.ReadFloat(stream)));
            }

            stream.Position = end;
        }

        public void Write(Stream stream)
        {
            Header.Write(stream);
            RenderWareFileHelper.WriteByte(stream, BoneCount);
            RenderWareFileHelper.WriteByte(stream, UsedBoneCount);
            RenderWareFileHelper.WriteByte(stream, MaxWeightsPerVertex);
            RenderWareFileHelper.WriteByte(stream, Flags);

            foreach (byte usedBone in UsedBones)
            {
                RenderWareFileHelper.WriteByte(stream, usedBone);
            }
        }
    }
}
