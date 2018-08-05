using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mackiloha;
using Mackiloha.Milo2;
using MiloOG = Mackiloha.Milo;
using GLTFTools;

namespace Mackiloha.Wpf.Extensions
{
    public static class MiloExtensions
    {
        public static int Size(this IMiloEntry entry) => entry is MiloEntry ? (entry as MiloEntry).Data.Length : -1;

        public static string Extension(this IMiloEntry entry)
        {
            if (entry == null || !entry.Name.Contains('.')) return "";
            return Path.GetExtension(entry.Name); // Returns .cs
        }

        public static void ExportToGLTF(this MiloFile milo, string path)
        {
            var pathDirectory = Path.GetDirectoryName(path);

            var textures = milo.Entries
                .Where(x => x.Type.Equals("Tex", StringComparison.CurrentCultureIgnoreCase))
                .Select(y =>
                {
                    var tex = MiloOG.Tex.FromStream(new MemoryStream((y as MiloEntry).Data));
                    tex.Name = y.Name;
                    return tex;
                }).ToList();

            var materials = milo.Entries
                .Where(x => x.Type.Equals("Mat", StringComparison.CurrentCultureIgnoreCase))
                .Select(y =>
                {
                    var mat = MiloOG.Mat.FromStream(new MemoryStream((y as MiloEntry).Data));
                    mat.Name = y.Name;
                    return mat;
                }).ToList();

            var meshes = milo.Entries
                .Where(x => x.Type.Equals("Mesh", StringComparison.CurrentCultureIgnoreCase))
                .Select(y =>
                {
                    var mesh = MiloOG.Mesh.FromStream(new MemoryStream((y as MiloEntry).Data));
                    mesh.Name = y.Name;
                    return mesh;
                }).Where(z => !string.IsNullOrEmpty(z.Material)).ToList();

            var transforms = milo.Entries
                .Where(x => x.Type.Equals("Trans", StringComparison.CurrentCultureIgnoreCase))
                .Select(y =>
                {
                    var trans = MiloOG.Trans.FromStream(new MemoryStream((y as MiloEntry).Data));
                    trans.Name = y.Name;
                    return trans;
                }).ToList();

            var views = milo.Entries
                .Where(x => x.Type.Equals("View", StringComparison.CurrentCultureIgnoreCase))
                .Select(y =>
                {
                    var view = MiloOG.View.FromStream(new MemoryStream((y as MiloEntry).Data));
                    view.Name = y.Name;
                    return view;
                }).ToList();

            var scene = new GLTF()
            {
                Asset = new Asset()
                {
                    Generator = "Mackiloha v1.0"
                },
                Images = textures.Select(x => new Image()
                {
                    Name = Path.GetFileNameWithoutExtension(x.Name),
                    Uri = Path.GetFileNameWithoutExtension(x.Name) + ".png"
                }).ToArray(),
                Scene = 0
            };

            // Saves textures
            for (int i = 0; i < textures.Count; i++)
                textures[i].Image.SaveAs(Path.Combine(pathDirectory, scene.Images[i].Uri));

            var accessors = new List<Accessor>();
            var sceneMeshes = new List<Mesh>();

            int bufferSize12 = meshes.Select(x => x.Vertices.Length * 12 * 2).Sum(); // Verts + norms
            int bufferSize8 = meshes.Select(x => x.Vertices.Length * 8).Sum(); // UV
            int bufferSize4 = meshes.Select(x => x.Faces.Length * 6).Sum(); // Faces
            if (bufferSize4 % 4 != 0) bufferSize4 += 4 - (bufferSize4 % 4);

            scene.Buffers = new GLTFTools.Buffer[]
            {
                new GLTFTools.Buffer()
                {
                    Name = Path.GetFileNameWithoutExtension(path),
                    ByteLength = (bufferSize4 + bufferSize8 + bufferSize12),
                    Uri = Path.GetFileNameWithoutExtension(path) + ".bin"
                }
            };

            scene.BufferViews = new BufferView[]
            {
                new BufferView()
                {
                    Name = "vertsAndNorms",
                    ByteLength = bufferSize12,
                    ByteOffset = 0,
                    ByteStride = 12
                },
                new BufferView()
                {
                    Name = "uvs",
                    ByteLength = bufferSize8,
                    ByteOffset = bufferSize12,
                    ByteStride = 8
                },
                new BufferView()
                {
                    Name = "faces",
                    ByteLength = bufferSize4,
                    ByteOffset = bufferSize12 + bufferSize8,
                    ByteStride = 4
                }
            };

            int buffer12Offset = scene.BufferViews[0].ByteOffset.Value;
            int buffer8Offset = scene.BufferViews[1].ByteOffset.Value;
            int buffer4Offset = scene.BufferViews[2].ByteOffset.Value;

            var bw = new BinaryWriter(new MemoryStream(new byte[bufferSize12 + bufferSize8 + bufferSize4]));
            
            foreach (var mesh in meshes)
            {
                if (mesh.Vertices.Length <= 0 || mesh.Faces.Length <= 0) continue;

                // Finds related material + texture
                var mat = materials.First(x => x.Name.Equals(mesh.Material, StringComparison.CurrentCultureIgnoreCase));
                var tex = textures.First(x => x.Name.Equals(mat.Textures[0], StringComparison.CurrentCultureIgnoreCase));

                var texIdx = 0;
                while (texIdx < scene.Images.Length)
                {
                    if (scene.Images[texIdx].Name == Path.GetFileNameWithoutExtension(tex.Name))
                        break;

                    texIdx++;
                }

                sceneMeshes.Add(new Mesh()
                {
                    Name = Path.GetFileNameWithoutExtension(mesh.Name),
                    Primitives = new MeshPrimitive[]
                    {
                        new MeshPrimitive()
                        {
                            Attributes = new MeshPrimitiveAttributes()
                            {
                                Position = accessors.Count,
                                Normal = accessors.Count + 1,
                                TextureCoordinate0 = accessors.Count + 2
                            },
                            Indices = accessors.Count + 3,
                            Material = texIdx,
                            Mode = RenderMode.Triangles
                        }
                    }
                });

                // Vertices
                accessors.Add(new Accessor()
                {
                    Name = Path.GetFileNameWithoutExtension(mesh.Name) + "_positions",
                    ComponentType = ComponentType.Float,
                    Count = mesh.Vertices.Length,
                    Min = new double[]
                    {
                        mesh.Vertices.Select(x => x.VertX).Min(),
                        mesh.Vertices.Select(x => x.VertY).Min(),
                        mesh.Vertices.Select(x => x.VertZ).Min()
                    },
                    Max = new double[]
                    {
                        mesh.Vertices.Select(x => x.VertX).Max(),
                        mesh.Vertices.Select(x => x.VertY).Max(),
                        mesh.Vertices.Select(x => x.VertZ).Max()
                    },
                    Type = GLType.Vector3,
                    BufferView = 0,
                    ByteOffset = buffer12Offset - scene.BufferViews[0].ByteOffset.Value
                });
                bw.BaseStream.Seek(buffer12Offset, SeekOrigin.Begin);
                foreach (var vert in mesh.Vertices)
                {
                    bw.Write(vert.VertX);
                    bw.Write(vert.VertY);
                    bw.Write(vert.VertZ);
                }
                buffer12Offset = (int)bw.BaseStream.Position;

                // Normals
                accessors.Add(new Accessor()
                {
                    Name = Path.GetFileNameWithoutExtension(mesh.Name) + "_normals",
                    ComponentType = ComponentType.Float,
                    Count = mesh.Vertices.Length,
                    Min = new double[]
                    {
                        mesh.Vertices.Select(x => x.NormX).Min(),
                        mesh.Vertices.Select(x => x.NormY).Min(),
                        mesh.Vertices.Select(x => x.NormZ).Min()
                    },
                    Max = new double[]
                    {
                        mesh.Vertices.Select(x => x.NormX).Max(),
                        mesh.Vertices.Select(x => x.NormY).Max(),
                        mesh.Vertices.Select(x => x.NormZ).Max()
                    },
                    Type = GLType.Vector3,
                    BufferView = 0,
                    ByteOffset = buffer12Offset - scene.BufferViews[0].ByteOffset.Value
                });
                bw.BaseStream.Seek(buffer12Offset, SeekOrigin.Begin);
                foreach (var vert in mesh.Vertices)
                {
                    bw.Write(vert.NormX);
                    bw.Write(vert.NormY);
                    bw.Write(vert.NormZ);
                }
                buffer12Offset = (int)bw.BaseStream.Position;

                // UV coordinates
                accessors.Add(new Accessor()
                {
                    Name = Path.GetFileNameWithoutExtension(mesh.Name) + "_texcoords",
                    ComponentType = ComponentType.Float,
                    Count = mesh.Vertices.Length,
                    Min = new double[]
                    {
                        mesh.Vertices.Select(x => x.U).Min(),
                        mesh.Vertices.Select(x => x.V).Min()
                    },
                    Max = new double[]
                    {
                        mesh.Vertices.Select(x => x.U).Max(),
                        mesh.Vertices.Select(x => x.V).Max()
                    },
                    Type = GLType.Vector2,
                    BufferView = 1,
                    ByteOffset = buffer8Offset - scene.BufferViews[1].ByteOffset.Value
                });
                bw.BaseStream.Seek(buffer8Offset, SeekOrigin.Begin);
                foreach (var vert in mesh.Vertices)
                {
                    bw.Write(vert.U);
                    bw.Write(vert.V);
                }
                buffer8Offset = (int)bw.BaseStream.Position;

                // Faces
                accessors.Add(new Accessor()
                {
                    Name = Path.GetFileNameWithoutExtension(mesh.Name) + "_indicies",
                    ComponentType = ComponentType.UnsignedShort,
                    Count = mesh.Faces.Length * 3,
                    Min = new double[]
                    {
                        mesh.Faces.SelectMany(x => x).Min()
                    },
                    Max = new double[]
                    {
                        mesh.Faces.SelectMany(x => x).Max()
                    },
                    Type = GLType.Scalar,
                    BufferView = 2,
                    ByteOffset = buffer4Offset - scene.BufferViews[2].ByteOffset.Value
                });
                bw.BaseStream.Seek(buffer4Offset, SeekOrigin.Begin);
                foreach (var face in mesh.Faces)
                {
                    bw.Write(face[0]);
                    bw.Write(face[1]);
                    bw.Write(face[2]);
                }
                buffer4Offset = (int)bw.BaseStream.Position;
            }
            
            scene.Accessors = accessors.ToArray();
            
            scene.Materials = new Material[]
            {
                new Material()
                {
                    Name = "Default",
                    EmissiveFactor = new Vector3<double>(),
                    AlphaMode = AlphaMode.Opaque,
                    DoubleSided = false
                }
            };

            var meshOffset = 0;
            scene.Materials =materials.Select(x => new Material()
            {
                Name = Path.GetFileNameWithoutExtension(x.Name),
                PbrMetallicRoughness = new PbrMetallicRoughness()
                {
                    BaseColorTexture = new BaseColorTexture()
                    {
                        Index = meshOffset++
                    },
                    BaseColorFactor = new Vector4<double>(1.0f),
                    MetallicFactor = 0,
                    RoughnessFactor = 1
                },
                EmissiveFactor = new Vector3<double>(),
                AlphaMode = AlphaMode.Opaque,
                DoubleSided = false
            }).ToArray();

            scene.Meshes = sceneMeshes.ToArray();

            meshOffset = 0;
            scene.Nodes = new Node[]
            {
                new Node()
                {
                    Children = Enumerable.Range(1, scene.Meshes.Length).ToArray(),
                    //Matrix = Matrix4<float>.Identity()
                    Matrix = new Matrix4<float>()
                    {
                        M11 = 1.0f,
                        M23 = 1.0f,
                        M32 = 1.0f,
                        M44 = 1.0f
                    }
                }
            }.Concat(scene.Meshes.Select(x =>
            {
                var node = new Node()
                {
                    Name = x.Name,
                    Mesh = meshOffset++,
                };

                var nodeMesh = meshes.First(y => y.Name == x.Name + ".mesh");
                var transEntry = milo.Entries.FirstOrDefault(y => y.Name == nodeMesh.Transform);
                if (transEntry == null) return node;
                
                switch (transEntry.Type)
                {
                    case "Mesh":
                        var mesh = meshes.First(y => y.Name == transEntry.Name);
                        node.Matrix = mesh.Mat2.ToGLMatrix();
                        break;
                    case "Trans":
                        var trans = transforms.First(y => y.Name == transEntry.Name);
                        node.Matrix = trans.Mat2.ToGLMatrix();
                        break;
                    case "View":
                        var view = views.First(y => y.Name == transEntry.Name);
                        node.Matrix = view.Mat2.ToGLMatrix();
                        break;
                    default:
                        break;
                }

                return node;
            })).ToArray();

            scene.Samplers = new Sampler[]
            {
                new Sampler()
                {
                    MagFilter = MagFilter.Linear,
                    MinFilter = MinFilter.Nearest,
                    WrapS = WrapMode.Repeat,
                    WrapT = WrapMode.Repeat
                }
            };

            scene.Scenes = new Scene[]
            {
                new Scene()
                {
                    //Nodes = scene.Nodes.Select(x => x.Mesh.Value).ToArray()
                    Nodes = new int[1]
                }
            };

            meshOffset = 0;
            scene.Textures = scene.Images.Select(x => new Texture()
            {
                Name = x.Name,
                Sampler = 0,
                Source = meshOffset++
            }).ToArray();

            using (var fs = File.OpenWrite(Path.Combine(pathDirectory, scene.Buffers[0].Uri)))
            {
                // Copies stream to file
                bw.BaseStream.Seek(0, SeekOrigin.Begin);
                bw.BaseStream.CopyTo(fs);
                bw.Dispose();
            }

            var json = scene.ToJson();
            File.WriteAllText(path, json);
        }

        public static Matrix4<float> ToGLMatrix(this Matrix miloMatrix) =>
            new Matrix4<float>()
            {
                M11 = miloMatrix.RX,
                M12 = miloMatrix.RY,
                M13 = miloMatrix.RZ,
                M14 = miloMatrix.RW,

                M21 = miloMatrix.UX,
                M22 = miloMatrix.UY,
                M23 = miloMatrix.UZ,
                M24 = miloMatrix.UW,

                M31 = miloMatrix.FX,
                M32 = miloMatrix.FY,
                M33 = miloMatrix.FZ,
                M34 = miloMatrix.FW,

                M41 = miloMatrix.PX,
                M42 = miloMatrix.PY,
                M43 = miloMatrix.PZ,
                M44 = miloMatrix.PW
            };
    }
}
