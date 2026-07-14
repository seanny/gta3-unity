using System.Collections.Generic;
using System.Linq;

namespace RenderWareIo.Structs.Ifp
{
    public class IfpAnimation
    {
        public string Name { get; set; }
        public List<IfpObjectAnimation> Objects { get; set; }

        public float Duration
        {
            get
            {
                float duration = 0.0f;

                foreach (IfpObjectAnimation obj in Objects)
                {
                    if (obj.Frames.Count > 0)
                    {
                        duration = System.Math.Max(duration, obj.Frames.Max(frame => frame.Time));
                    }
                }

                return duration;
            }
        }

        public IfpAnimation()
        {
            Name = string.Empty;
            Objects = new List<IfpObjectAnimation>();
        }
    }
}
