using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace RenderWareIo.Structs.Ide
{
    public struct Ped : IIdeEntity<Ped>, IModelTxd
    {
        public int Id { get; set; }
        public string ModelName { get; set; }
        public string TxdName { get; set; }
        public string PedType { get; set; }
        public string Behaviour { get; set; }
        public string AnimGroup { get; set; }
        public string CarMask { get; set; }

        public Ped Read(string line)
        {
            string[] splits = line.Split(',').Select((split) => split.Trim()).ToArray();

            this.Id = int.Parse(splits[0]);
            this.ModelName = splits[1];
            this.TxdName = splits[2];
            this.PedType = splits[3];
            this.Behaviour = splits[4];
            this.AnimGroup = splits[5];
            this.CarMask = splits[6];
            return this;
        }

        public string Write()
        {
            throw new NotImplementedException();
        }
    }
}
