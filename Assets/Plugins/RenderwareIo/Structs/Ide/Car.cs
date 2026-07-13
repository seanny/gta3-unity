using System;
using System.Globalization;
using System.Linq;
using UnityEditor.Rendering;
using UnityEngine;

namespace RenderWareIo.Structs.Ide
{
    [Serializable]
    public struct Car : IIdeEntity<Car>
    {
        public int Id;
        public string ModelName;
        public string TxdName;
        public string Type;
        public string HandlingId;
        public string GameName;
        public string Anims;
        public string Class;
        public int Frequency;
        public int Level;
        public uint CompRules;
        public int WheelModelId;
        public float WheelScale;
        public int LODModel;


        public Car Read(string line)
        {
            line = line.Replace(" ", "").Replace("\t", "");

            while (line.IndexOf("\t\t") > 0)
                line = line.Replace("\t\t", "\t");

            string[] splits = line.Split(',');

            this.Id = int.Parse(splits[0]);
            this.ModelName = splits[1];
            this.TxdName = splits[2];
            this.Type = splits[3];
            this.HandlingId = splits[4];
            this.GameName = splits[5];
            this.Class = splits[6];
            this.Frequency = int.Parse(splits[7]);
            this.Level = int.Parse(splits[8]);
            Debug.Log(splits[9]);
            this.CompRules = Convert.ToUInt32(splits[9].ReplaceInvalidFileNameCharacters(""), 16);
            if(Type == "car")
            {
                this.WheelModelId = int.Parse(splits[10]);
                this.WheelScale = float.Parse(splits[11]);
            }
            else if(Type == "plane")
            {
                this.LODModel = int.Parse(splits[10]);
            }
            return this;
        }

        public string Write()
        {
            throw new NotImplementedException();
        }
    }
}
