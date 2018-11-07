using System;
using System.Collections.Generic;
using System.Text;
using Mackiloha.System.Render.Interfaces;

namespace Mackiloha.System.Render
{
    public struct Vertex3
    {
        public float X;
        public float Y;
        public float Z;

        public float NormalX;
        public float NormalY;
        public float NormalZ;

        public float ColorR;
        public float ColorG;
        public float ColorB;
        public float ColorA;

        public float U;
        public float V;
    }

    public struct Face
    {
        public ushort V1;
        public ushort V2;
        public ushort V3;
    }

    public struct FaceGroup
    {
        public int Size;
        public List<int> Sections;
        public List<int> VertexIndicies;
    }

    public struct Bone
    {
        public MiloString Name;
        public Matrix4 Mat;
    }
    
    public class Mesh : RenderObject, ITrans, IAnim
    {
        public Trans Trans => new Trans();
        public Anim Anim => new Anim();

        public MiloString Material { get; set; }
        public MiloString MainMesh { get; set; }

        public bool Unknown { get; set; }

        public List<Vertex3> Vertices { get; } = new List<Vertex3>();
        public List<Face> Faces { get; } = new List<Face>();

        public List<FaceGroup> Groups { get; } = new List<FaceGroup>();
        public List<Bone> Bones { get; } = new List<Bone>();
    }
}
