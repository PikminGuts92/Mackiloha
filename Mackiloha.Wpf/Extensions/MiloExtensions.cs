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
                    ByteStride = null
                }
            };

            int buffer12Offset = scene.BufferViews[0].ByteOffset.Value;
            int buffer8Offset = scene.BufferViews[1].ByteOffset.Value;
            int buffer4Offset = scene.BufferViews[2].ByteOffset.Value;

            var bw = new BinaryWriter(new MemoryStream(new byte[bufferSize12 + bufferSize8 + bufferSize4]));
            Dictionary<string, int> meshIndex = new Dictionary<string, int>();
            var currentOffset = 0;

            foreach (var mesh in meshes)
            {
                if (mesh.Vertices.Length <= 0 || mesh.Faces.Length <= 0) continue;
                meshIndex.Add(mesh.Name, currentOffset++);

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
            
            currentOffset = 0;
            scene.Materials = materials.Select(x => new Material()
            {
                Name = Path.GetFileNameWithoutExtension(x.Name),
                PbrMetallicRoughness = new PbrMetallicRoughness()
                {
                    BaseColorTexture = new BaseColorTexture()
                    {
                        Index = currentOffset++
                    },
                    BaseColorFactor = new Vector4<double>(1.0f),
                    MetallicFactor = 0,
                    RoughnessFactor = 1
                },
                EmissiveFactor = new Vector3<double>(),
                AlphaMode = AlphaMode.Opaque,
                DoubleSided = true
            }).ToArray();

            scene.Meshes = sceneMeshes.ToArray();
            
            var nodes = new List<Node>();
            var nodeIndex = new Dictionary<string, int>();

            // TODO: Make milo objects with transforms data
            MiloOG.AbstractEntry GetAbstractEntry(string name)
            {
                var entry = milo.Entries.FirstOrDefault(x => x.Name == name);
                if (entry == null) return null;

                switch (entry.Type)
                {
                    case "Mesh":
                        return meshes.First(y => y.Name == entry.Name);
                    case "Trans":
                        return transforms.First(y => y.Name == entry.Name);
                    case "View":
                        return views.First(y => y.Name == entry.Name);
                    default:
                        return null;
                }
            }
            
            Matrix4<float>? GetTransform(string transform)
            {
                var transEntry = milo.Entries.FirstOrDefault(y => y.Name == transform);
                if (transEntry == null) return null;

                switch (transEntry.Type)
                {
                    case "Mesh":
                        var mesh = meshes.First(y => y.Name == transEntry.Name);
                        return mesh.Mat2.ToGLMatrix();
                    case "Trans":
                        var trans = transforms.First(y => y.Name == transEntry.Name);
                        return trans.Mat2.ToGLMatrix();
                    case "View":
                        var view2 = views.First(y => y.Name == transEntry.Name);
                        return view2.Mat2.ToGLMatrix();
                    default:
                        return null;
                }
            }

            string GetTransformName(MiloOG.AbstractEntry entry)
            {
                switch (entry.Type)
                {
                    case "Mesh":
                        var mesh = meshes.First(y => y.Name == entry.Name);
                        return mesh.Transform;
                    case "Trans":
                        var trans = transforms.First(y => y.Name == entry.Name);
                        return trans.Name;
                    case "View":
                        var view = views.First(y => y.Name == entry.Name);
                        return view.Transform;
                    default:
                        return null;
                }
            }

            var children = new Dictionary<string, List<string>>();
            foreach (var entry in meshes.Union<MiloOG.AbstractEntry>(views).Union<MiloOG.AbstractEntry>(transforms))
            {
                var trans = GetTransformName(entry);
                if (trans == null) continue;

                if (!children.ContainsKey(trans))
                    children.Add(trans, new List<string>(new string[] { entry.Name }));
                else if (!children[trans].Contains(entry.Name))
                    children[trans].Add(entry.Name);
            }


            var rootIndex = new List<int>();
            foreach (var key in children.Keys)
            {
                rootIndex.Add(nodes.Count);

                dynamic entry = GetAbstractEntry(key);

                var node = new Node()
                {
                    Name = "Root_" + entry.Name,
                    //Mesh = meshIndex.ContainsKey(key) ? (int?)meshIndex[key] : null,
                    Matrix = ToGLMatrix(entry.Mat2),
                    Children = Enumerable.Range(nodes.Count + 1, children[key].Count).ToArray()
                };
                nodes.Add(node);

                foreach (var child in children[key])
                {
                    dynamic subEntry = GetAbstractEntry(child);

                    var subNode = new Node()
                    {
                        Name = subEntry.Name,
                        Mesh = meshIndex.ContainsKey(subEntry.Name) ? (int?)meshIndex[subEntry.Name] : null,
                        //Matrix = ToGLMatrix(subEntry.Mat1)
                    };
                    nodeIndex.Add(child, rootIndex.Last());
                    nodes.Add(subNode);
                }
            }

            int CreateNode(string name) // Returns index of node
            {
                if (nodeIndex.ContainsKey(name))
                    return nodeIndex[name];
                
                dynamic entry = GetAbstractEntry(name);
                dynamic transformEntry = GetAbstractEntry(entry.Transform);
                List<string> subNodes = entry.Meshes;

                var node = new Node()
                {
                    Name = name,
                    Mesh = meshIndex.ContainsKey(name) ? (int?)meshIndex[name] : null,
                    Matrix = ToGLMatrix(entry.Mat1),
                    //Matrix = GetTransform(entry.Transform),
                    Children = (subNodes.Count > 0) ? subNodes.Select(x => CreateNode(x)).ToArray() : null
                };

                nodeIndex.Add(name, nodes.Count);
                nodes.Add(node);
                return nodeIndex[name];
            }

            /*
            foreach (var n in meshes.Union<MiloOG.AbstractEntry>(views).Union<MiloOG.AbstractEntry>(transforms)) CreateNode(n.Name);

            scene.Scene = 0;
            scene.Scenes = new Scene[] { new Scene() { Nodes = Enumerable.Range(0, nodes.Count).ToArray() } };
            */

            /*
            foreach (var view in views) CreateNode(view.Name);
            
            // Finds root node
            var childrenNodes = nodes.SelectMany(x => x.Children ?? new int[0]).Distinct();
            var parentNodes = Enumerable.Range(0, nodes.Count);
            var rootIdx = parentNodes.Except(childrenNodes).Single();

            scene.Scene = 0;
            scene.Scenes = new Scene[] { new Scene() { Nodes = new int[] { rootIdx } } };
            */

            List<string> GetAllSubs(MiloOG.AbstractEntry aEntry)
            {
                dynamic entry = aEntry;
                List<string> subsEntriesNames = entry.Meshes;
                dynamic subEntries = subsEntriesNames.Select(x => GetAbstractEntry(x)).ToList();

                foreach (var subEntry in subEntries)
                    subsEntriesNames.AddRange(GetAllSubs(subEntry));

                return subsEntriesNames;
            }

            scene.Scene = 0;
            //scene.Scenes = new Scene[] { new Scene() { Nodes = rootIndex.ToArray() } };

            scene.Scenes = views.Select(x => new Scene()
            {
                Nodes = GetAllSubs(x).Select(y => nodeIndex[y]).Distinct().ToArray()
            }).ToArray();

            scene.Nodes = nodes.ToArray();

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
            
            currentOffset = 0;
            scene.Textures = scene.Images.Select(x => new Texture()
            {
                Name = x.Name,
                Sampler = 0,
                Source = currentOffset++
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
                // Swaps x and y values (columns 2 and 3)
                M11 = miloMatrix.M11,
                M12 = miloMatrix.M13,
                M13 = miloMatrix.M12,
                M14 = miloMatrix.M14,

                M21 = miloMatrix.M21,
                M22 = miloMatrix.M23,
                M23 = miloMatrix.M22,
                M24 = miloMatrix.M24,

                M31 = miloMatrix.M31,
                M32 = miloMatrix.M33,
                M33 = miloMatrix.M32,
                M34 = miloMatrix.M34,

                M41 = miloMatrix.M41,
                M42 = miloMatrix.M43,
                M43 = miloMatrix.M42,
                M44 = miloMatrix.M44
            };

        public static void WriteTree(this MiloFile milo, string path)
        {
            using (StreamWriter sw = new StreamWriter(path, false, Encoding.UTF8))
            {
                foreach (var view in milo.Entries.Where(x => x.Type == "View"))
                    WriteTree(milo, view.Name, sw, 0);
            }
        }

        public static void WriteTree2(this MiloFile milo, string path)
        {
            MiloOG.AbstractEntry GetOGEntry(string name)
            {
                var e = milo.Entries.First(x => x.Name == name) as MiloEntry;

                switch (e.Type)
                {
                    case "Mesh":
                        var mesh = MiloOG.Mesh.FromStream(new MemoryStream(e.Data));
                        mesh.Name = e.Name;
                        return mesh;
                    case "Trans":
                        var trans = MiloOG.Trans.FromStream(new MemoryStream(e.Data));
                        trans.Name = e.Name;
                        return trans;
                    case "View":
                        var view = MiloOG.View.FromStream(new MemoryStream(e.Data));
                        view.Name = e.Name;
                        return view;
                    default:
                        return null;
                }
            }

            string GetTransformName(string name)
            {
                var e = milo.Entries.First(x => x.Name == name) as MiloEntry;

                switch (e.Type)
                {
                    case "Mesh":
                        var mesh = MiloOG.Mesh.FromStream(new MemoryStream(e.Data));
                        mesh.Name = e.Name;
                        return mesh.Transform;
                    case "Trans":
                        var trans = MiloOG.Trans.FromStream(new MemoryStream(e.Data));
                        trans.Name = e.Name;
                        return trans.Name;
                    case "View":
                        var view = MiloOG.View.FromStream(new MemoryStream(e.Data));
                        view.Name = e.Name;
                        return view.Transform;
                    default:
                        return null;
                }
            }

            using (StreamWriter sw = new StreamWriter(path, false, Encoding.UTF8))
            {
                var children = new Dictionary<string, List<string>>();
                
                foreach (var entry in milo.Entries)
                {
                    /*
                    if (!children.ContainsKey(entry.Name))
                        children.Add(entry.Name, new List<string>());
                    */

                    var trans = GetTransformName(entry.Name);
                    if (trans == null || trans == entry.Name) continue;

                    if (!children.ContainsKey(trans))
                        children.Add(trans, new List<string>(new string[] { entry.Name }));
                    else if (!children[trans].Contains(entry.Name))
                        children[trans].Add(entry.Name);

                    //WriteTree(milo, view.Name, sw, 0);
                }
            }
        }

        private static void WriteTree(MiloFile milo, string entry, StreamWriter sw, int depth, bool bone = false)
        {
            MiloOG.AbstractEntry GetOGEntry(string name)
            {
                var e = milo.Entries.First(x => x.Name == name) as MiloEntry;

                switch (e.Type)
                {
                    case "Mesh":
                        var mesh = MiloOG.Mesh.FromStream(new MemoryStream(e.Data));
                        mesh.Name = e.Name;
                        return mesh;
                    case "Trans":
                        var trans = MiloOG.Trans.FromStream(new MemoryStream(e.Data));
                        trans.Name = e.Name;
                        return trans;
                    case "View":
                        var view = MiloOG.View.FromStream(new MemoryStream(e.Data));
                        view.Name = e.Name;
                        return view;
                    default:
                        return null;
                }
            }

            dynamic transEntry = GetOGEntry(entry);
            List<string> subBones = transEntry.Meshes;
            List<string> subEntries = transEntry.Meshes;
            string type = bone ? "Bone" : "Mesh";

            sw.WriteLine($"{new string('\t', depth)}{type}: {transEntry.Name} ({transEntry.Transform})");

            foreach (var sub in subBones)
            {
                WriteTree(milo, sub, sw, depth + 1, true);
            }

            foreach (var sub in subEntries)
            {
                WriteTree(milo, sub, sw, depth + 1);
            }
        }
    }
}
