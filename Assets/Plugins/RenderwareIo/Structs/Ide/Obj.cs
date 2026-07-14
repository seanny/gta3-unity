using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace RenderWareIo.Structs.Ide
{
    [Serializable]
    public struct Obj : IIdeEntity<Obj>, IModelTxd
    {
        public int Id => m_Id;
        public string ModelName => m_ModelName;
        public string TxdName => m_TxdName;
        public int MeshCount => m_MeshCount;
        public float[] DrawDistances => m_DrawDistances;
        public int Flags => m_Flags;

        public int m_Id;
        public string m_ModelName;
        public string m_TxdName;
        public int m_MeshCount;
        public float[] m_DrawDistances;
        public int m_Flags;

        public Obj Read(string line)
        {
            string[] splits = line.Split(',').Select((split) => split.Trim()).ToArray();

            this.m_Id = int.Parse(splits[0]);
            this.m_ModelName = splits[1];
            this.m_TxdName = splits[2];

            if (splits.Length > 5)
            {
                this.m_MeshCount = int.Parse(splits[3]);

                this.m_DrawDistances = new float[splits.Length - 4];
                for (int i = 0; i < (splits.Length - 4); i++)
                {
                    this.m_DrawDistances[i] = float.Parse(splits[3 + i], NumberStyles.Any, CultureInfo.InvariantCulture);
                }

            } else
            {
                this.m_MeshCount = 1;
                this.m_DrawDistances = new float[1] { float.Parse(splits[3], NumberStyles.Any, CultureInfo.InvariantCulture) };
            }
            this.m_Flags = int.Parse(splits[splits.Length - 1]);

            return this;
        }

        public string Write()
        {
            throw new NotImplementedException();
        }
    }
}
