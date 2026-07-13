using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;

namespace RenderWareIo.Structs.Ipl
{
    [Serializable]
    public struct Inst : IIplEntity<Inst>
    {
        public int Id;
        public string ModelName;
        public Vector3 Position;
        public Vector3 Scale;
        public Quaternion Rotation;

        public Inst Read(string line)
        {
            
            string[] splits = line.Split(',').Select((split) => split.Trim()).ToArray();

            this.Id = int.Parse(splits[0]);
            this.ModelName = splits[1];

            // .ipl seems to X,Z,Y instead of X,Y,Z
            this.Position = new Vector3(
                float.Parse(splits[2], CultureInfo.InvariantCulture),
                float.Parse(splits[4], CultureInfo.InvariantCulture),
                float.Parse(splits[3], CultureInfo.InvariantCulture)
            );
            // Could it be possible that the Scale is always 1,1,1?
            this.Scale = new Vector3(
                float.Parse(splits[5], CultureInfo.InvariantCulture),
                float.Parse(splits[6], CultureInfo.InvariantCulture),
                float.Parse(splits[7], CultureInfo.InvariantCulture)
            );
            this.Rotation = new Quaternion(
                float.Parse(splits[8], CultureInfo.InvariantCulture),
                float.Parse(splits[10], CultureInfo.InvariantCulture),
                float.Parse(splits[9], CultureInfo.InvariantCulture),
                float.Parse(splits[11], CultureInfo.InvariantCulture)
            );
            return this;
        }

        public string Write()
        {
            return $"{Id},{ModelName},{Position.X},{Position.Y},{Scale.X},{Scale.Y},{Scale.Z},{Position.Z},{Rotation.X},{Rotation.Y},{Rotation.Z},{Rotation.W}";
        }
    }
}
