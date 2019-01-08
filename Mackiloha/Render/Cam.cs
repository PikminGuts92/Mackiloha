using System;
using System.Collections.Generic;
using System.Text;
using Mackiloha.Render.Interfaces;

namespace Mackiloha.Render
{
    public class Cam : RenderObject, ITrans, IDraw, ISerializable
    {
        public Trans Trans { get; } = new Trans();
        public Draw Draw { get; } = new Draw();
        
        public float NearPlane { get; set; }
        public float FarPlane { get; set; }
        public float FOV { get; set; }

        public Rectangle ScreenArea { get; set; }
        public Vector2 ZRange { get; set; }

        public MiloString TargetTexture { get; set; }

        public override MiloString Type => "Cam";
    }
}
