using System.Collections.Generic;

namespace RenderWareIo.Structs.Ifp
{
    public class Ifp
    {
        public string Name { get; set; }
        public List<IfpAnimation> Animations { get; set; }

        public Ifp()
        {
            Name = string.Empty;
            Animations = new List<IfpAnimation>();
        }
    }
}
