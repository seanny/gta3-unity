using System.Collections.Generic;

namespace RenderWareIo.Structs.Ifp
{
    public class IfpObjectAnimation
    {
        public string Name { get; set; }
        public int FrameType { get; set; }
        public int BoneId { get; set; }
        public List<IfpFrame> Frames { get; set; }

        public IfpObjectAnimation()
        {
            Name = string.Empty;
            Frames = new List<IfpFrame>();
        }
    }
}
