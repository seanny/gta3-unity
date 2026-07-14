using System.Numerics;

namespace RenderWareIo.Structs.Ifp
{
    public class IfpFrame
    {
        public Quaternion Rotation { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Scale { get; set; }
        public float Time { get; set; }
        public bool HasPosition { get; set; }
        public bool HasScale { get; set; }

        public IfpFrame()
        {
            Rotation = Quaternion.Identity;
            Position = Vector3.Zero;
            Scale = Vector3.One;
        }
    }
}
