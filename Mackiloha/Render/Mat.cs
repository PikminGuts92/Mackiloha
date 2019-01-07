using System;
using System.Collections.Generic;
using System.Text;

namespace Mackiloha.Render
{
    public struct TextureEntry
    {
        public int Unknown1;
        public int Unknown2;
        public Matrix4 Mat;
        public int Unknown3;
        public MiloString Texture;
    }

    public enum BlendFactor : int
    {
        Unknown = 0,
        Zero = 1,
        One = 2,
        SrColor = 3,
        InvSrColor = 4
    }

    public class Mat : RenderObject, ISerializable
    {
        public List<TextureEntry> TextureEntries { get; } = new List<TextureEntry>();

        public Color4 BaseColor;
        public BlendFactor Blend;

        public override MiloString Type => "Mat";
    }
}
