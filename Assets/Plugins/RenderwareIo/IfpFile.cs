using RenderWareIo.Structs.Ifp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;

namespace RenderWareIo
{
    public class IfpFile
    {
        public Ifp Ifp { get; set; }

        public IfpFile(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"IFP file '{path}' does not exist", path);
            }

            using FileStream stream = File.OpenRead(path);
            Ifp = Read(stream);
        }

        public IfpFile(byte[] data)
        {
            using MemoryStream stream = new MemoryStream(data);
            Ifp = Read(stream);
        }

        private static Ifp Read(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream, Encoding.ASCII, leaveOpen: true);
            string signature = ReadFourCc(reader);
            int size = reader.ReadInt32();

            return signature switch
            {
                "ANPK" => ReadAnpk(reader, size),
                "ANP3" => ReadAnp3(reader, size),
                _ => throw new InvalidDataException($"Unsupported IFP signature '{signature}'.")
            };
        }

        private static Ifp ReadAnpk(BinaryReader reader, int size)
        {
            long end = reader.BaseStream.Position + size;
            Ifp ifp = new Ifp();
            int animationCount = ReadVersionOneInfoHeader(reader, ifp);

            for (int animationIndex = 0;
                 animationIndex < animationCount && reader.BaseStream.Position + 8 <= end;
                 animationIndex++)
            {
                IfpAnimation animation = ReadVersionOneAnimation(reader);
                ifp.Animations.Add(animation);
            }

            reader.BaseStream.Position = end;
            return ifp;
        }

        private static int ReadVersionOneInfoHeader(BinaryReader reader, Ifp ifp)
        {
            string infoId = ReadFourCc(reader);
            int infoSize = reader.ReadInt32();
            long infoEnd = reader.BaseStream.Position + infoSize;

            if (infoId != "INFO")
            {
                reader.BaseStream.Position = infoEnd;
                return 0;
            }

            int count = reader.ReadInt32();
            ifp.Name = ReadPaddedString(reader);
            reader.BaseStream.Position = Math.Max(reader.BaseStream.Position, infoEnd);
            return count;
        }

        private static IfpAnimation ReadVersionOneAnimation(BinaryReader reader)
        {
            IfpAnimation animation = new IfpAnimation();

            string nameId = ReadFourCc(reader);
            int nameSize = reader.ReadInt32();
            long nameEnd = reader.BaseStream.Position + nameSize;

            if (nameId == "NAME")
            {
                animation.Name = ReadPaddedString(reader);
            }

            reader.BaseStream.Position = Math.Max(reader.BaseStream.Position, nameEnd);

            string dataId = ReadFourCc(reader);
            int dataSize = reader.ReadInt32();
            long dataEnd = reader.BaseStream.Position + dataSize;

            if (dataId == "DGAN")
            {
                ReadVersionOneAnimationData(reader, animation, dataEnd);
            }

            reader.BaseStream.Position = Math.Max(reader.BaseStream.Position, dataEnd);
            return animation;
        }

        private static void ReadVersionOneAnimationData(
            BinaryReader reader,
            IfpAnimation animation,
            long dataEnd)
        {
            string infoId = ReadFourCc(reader);
            int infoSize = reader.ReadInt32();
            long infoEnd = reader.BaseStream.Position + infoSize;

            if (infoId != "INFO")
            {
                reader.BaseStream.Position = infoEnd;
                return;
            }

            int objectCount = reader.ReadInt32();
            ReadPaddedString(reader);
            reader.BaseStream.Position = Math.Max(reader.BaseStream.Position, infoEnd);

            for (int objectIndex = 0;
                 objectIndex < objectCount && reader.BaseStream.Position + 8 <= dataEnd;
                 objectIndex++)
            {
                string objectId = ReadFourCc(reader);
                int objectSize = reader.ReadInt32();
                long objectEnd = reader.BaseStream.Position + objectSize;

                if (objectId == "CPAN")
                {
                    IfpObjectAnimation obj = ReadVersionOneObject(reader, objectEnd);
                    if (obj != null)
                    {
                        animation.Objects.Add(obj);
                    }
                }

                reader.BaseStream.Position = Math.Max(reader.BaseStream.Position, objectEnd);
            }
        }

        private static IfpObjectAnimation ReadVersionOneObject(
            BinaryReader reader,
            long objectEnd)
        {
            string animId = ReadFourCc(reader);
            int animSize = reader.ReadInt32();
            long animEnd = reader.BaseStream.Position + animSize;

            if (animId != "ANIM")
            {
                reader.BaseStream.Position = animEnd;
                return null;
            }

            IfpObjectAnimation obj = new IfpObjectAnimation
            {
                Name = ReadFixedString(reader, 28)
            };

            int frameCount = reader.ReadInt32();
            reader.ReadInt32();
            reader.ReadInt32();
            reader.ReadInt32();
            obj.BoneId = reader.ReadInt32();

            reader.BaseStream.Position = animEnd;

            if (reader.BaseStream.Position + 8 <= objectEnd)
            {
                string keyFrameType = ReadFourCc(reader);
                int keyFrameSize = reader.ReadInt32();
                long keyFrameEnd = Math.Min(reader.BaseStream.Position + keyFrameSize, objectEnd);
                obj.Frames = ReadVersionOneFrames(reader, keyFrameType, frameCount, keyFrameSize);
                reader.BaseStream.Position = keyFrameEnd;
            }

            return obj;
        }

        private static List<IfpFrame> ReadVersionOneFrames(
            BinaryReader reader,
            string keyFrameType,
            int declaredFrameCount,
            int keyFrameSize)
        {
            bool hasPosition = keyFrameType == "KRT0" || keyFrameType == "KRTS";
            bool hasScale = keyFrameType == "KRTS";
            int transformSize = 16 + (hasPosition ? 12 : 0) + (hasScale ? 12 : 0);
            int count = declaredFrameCount;
            int withTimeSize = transformSize + 4;

            if (count <= 0 || count * withTimeSize > keyFrameSize)
            {
                count = withTimeSize > 0 ? keyFrameSize / withTimeSize : 0;
            }

            bool hasPerFrameTime = count > 0 && count * withTimeSize <= keyFrameSize;
            List<IfpFrame> frames = new List<IfpFrame>(count);

            for (int frameIndex = 0; frameIndex < count; frameIndex++)
            {
                IfpFrame frame = new IfpFrame
                {
                    Rotation = ReadQuaternion(reader),
                    HasPosition = hasPosition,
                    HasScale = hasScale
                };

                if (hasPosition)
                {
                    frame.Position = ReadVector3(reader);
                }

                if (hasScale)
                {
                    frame.Scale = ReadVector3(reader);
                }

                frame.Time = hasPerFrameTime ? reader.ReadSingle() : frameIndex;
                frames.Add(frame);
            }

            if (!hasPerFrameTime && frames.Count > 0 && reader.BaseStream.Position + 4 <= reader.BaseStream.Length)
            {
                float duration = reader.ReadSingle();
                float step = frames.Count > 1 ? duration / (frames.Count - 1) : 0.0f;

                for (int frameIndex = 0; frameIndex < frames.Count; frameIndex++)
                {
                    frames[frameIndex].Time = frameIndex * step;
                }
            }

            return frames;
        }

        private static Ifp ReadAnp3(BinaryReader reader, int size)
        {
            Ifp ifp = new Ifp
            {
                Name = ReadFixedString(reader, 24)
            };

            int animationCount = reader.ReadInt32();

            for (int animationIndex = 0; animationIndex < animationCount; animationIndex++)
            {
                IfpAnimation animation = new IfpAnimation
                {
                    Name = ReadFixedString(reader, 24)
                };

                int objectCount = reader.ReadInt32();
                reader.ReadInt32();
                reader.ReadInt32();

                for (int objectIndex = 0; objectIndex < objectCount; objectIndex++)
                {
                    IfpObjectAnimation obj = new IfpObjectAnimation
                    {
                        Name = ReadFixedString(reader, 24),
                        FrameType = reader.ReadInt32()
                    };

                    int frameCount = reader.ReadInt32();
                    obj.BoneId = reader.ReadInt32();
                    bool hasPosition = obj.FrameType == 4;

                    for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
                    {
                        IfpFrame frame = new IfpFrame
                        {
                            Rotation = ReadCompressedQuaternion(reader),
                            Time = reader.ReadInt16(),
                            HasPosition = hasPosition
                        };

                        if (hasPosition)
                        {
                            frame.Position = ReadCompressedVector3(reader);
                        }

                        obj.Frames.Add(frame);
                    }

                    animation.Objects.Add(obj);
                }

                ifp.Animations.Add(animation);
            }

            return ifp;
        }

        private static string ReadFourCc(BinaryReader reader)
        {
            return Encoding.ASCII.GetString(reader.ReadBytes(4));
        }

        private static string ReadFixedString(BinaryReader reader, int length)
        {
            byte[] bytes = reader.ReadBytes(length);
            int textLength = Array.IndexOf(bytes, (byte)0);

            if (textLength < 0)
            {
                textLength = bytes.Length;
            }

            return Encoding.ASCII.GetString(bytes, 0, textLength);
        }

        private static string ReadPaddedString(BinaryReader reader)
        {
            List<byte> bytes = new List<byte>();
            byte value;

            do
            {
                value = reader.ReadByte();

                if (value != 0)
                {
                    bytes.Add(value);
                }
            } while (value != 0);

            while (reader.BaseStream.Position % 4 != 0)
            {
                reader.ReadByte();
            }

            return Encoding.ASCII.GetString(bytes.ToArray());
        }

        private static Vector3 ReadVector3(BinaryReader reader)
        {
            return new Vector3(
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle());
        }

        private static Quaternion ReadQuaternion(BinaryReader reader)
        {
            return new Quaternion(
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle());
        }

        private static Vector3 ReadCompressedVector3(BinaryReader reader)
        {
            return new Vector3(
                reader.ReadInt16() / 1024.0f,
                reader.ReadInt16() / 1024.0f,
                reader.ReadInt16() / 1024.0f);
        }

        private static Quaternion ReadCompressedQuaternion(BinaryReader reader)
        {
            return new Quaternion(
                reader.ReadInt16() / 4096.0f,
                reader.ReadInt16() / 4096.0f,
                reader.ReadInt16() / 4096.0f,
                reader.ReadInt16() / 4096.0f);
        }
    }
}
