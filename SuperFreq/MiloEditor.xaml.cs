﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Mackiloha;
using Mackiloha.Milo;
using HelixToolkit;
using HelixToolkit.Wpf;
using System.IO;
using System.Windows.Media.Media3D;
//using System.Drawing; // TODO: Use something else for images
using System.Windows.Interop;
using GameArchives;
using GameArchives.Ark;

namespace SuperFreq
{
    /// <summary>
    /// Interaction logic for MiloEditor.xaml
    /// </summary>
    public partial class MiloEditor : UserControl
    {
        private ArkPackage ark;
        private string arkFilePath;

        public MiloEditor()
        {
            InitializeComponent();
        }

        public void SetArk(ArkPackage arkInput) => this.ark = arkInput;
        public void SetFilePath(string path) => this.arkFilePath = path;

        public void OpenMiloFile(Stream source)
        {
            if (source == null) return;

            HelixViewport3D.Children.Clear();
            HelixViewport3D.Children.Add(new DefaultLights());
            GridLinesVisual3D grid = new GridLinesVisual3D()
            {
                Width = 8,
                Length = 8,
                MinorDistance = 1,
                MajorDistance = 1,
                Thickness = 0.01f
            };
            HelixViewport3D.Children.Add(grid);

            // Opens input milo file
            MiloFile milo = MiloFile.FromStream(source);
            
            foreach (var entry in milo.Entries)
            {
                if (!(entry is Mesh)) continue;
                Mesh mesh = entry as Mesh;

                HelixViewport3D.Children.Add(CreateModel(mesh, milo));
            }
        }

        public ModelVisual3D CreateModel(Mesh mesh, MiloFile milo)
        {
            // ----- MESH STUFF -----
            MeshGeometry3D mesh3d = new MeshGeometry3D();
            
            Point3DCollection positions = new Point3DCollection();
            Vector3DCollection normals = new Vector3DCollection();
            PointCollection uvs = new PointCollection();
            Int32Collection indices = new Int32Collection();

            foreach (Vertex vert in mesh.Vertices)
                positions.Add(new Point3D(vert.VertX, vert.VertY, vert.VertZ));

            foreach (Vertex vert in mesh.Vertices)
                normals.Add(new Vector3D(vert.NormX, vert.NormY, vert.NormZ));

            foreach (Vertex vert in mesh.Vertices)
                uvs.Add(new System.Windows.Point(vert.U, vert.V));

            foreach (ushort[] face in mesh.Faces)
            {
                indices.Add(face[0]);
                indices.Add(face[1]);
                indices.Add(face[2]);
            }
            
            mesh3d.Positions = positions;
            mesh3d.TriangleIndices = indices;
            mesh3d.Normals = normals;
            mesh3d.TextureCoordinates = uvs;
            mesh3d.SetName(mesh.Name);

            // ----- MATERIAL STUFF -----
            //SolidColorBrush brush = new SolidColorBrush();
            //brush.Color = Color.FromRgb(0x80, 0x80, 0x80); // Grey
            //mat.Brush = brush;

            Material mat = GetMaterial(mesh.Material, milo);
            if (mat == null) mat = new DiffuseMaterial(new SolidColorBrush() { Color = Color.FromRgb(0x80, 0x80, 0x80) }); // Grey material

            // Putting it all together
            ModelVisual3D model3d = new ModelVisual3D();
            GeometryModel3D geom3d = new GeometryModel3D();
            
            geom3d.Geometry = mesh3d;
            geom3d.Material = mat;
            model3d.Content = geom3d;
            geom3d.BackMaterial = mat; // Visibility on both sides
            
            return model3d;
        }

        private Material GetMaterial(string matName, MiloFile milo)
        {
            // Materials can have multiple textures
            MaterialGroup group = new MaterialGroup();
            group.SetName(matName);

            
            AbstractEntry ab = milo[matName];
            if (ab == null || !(ab is Mat)) return null;

            // Enumerates textures in material
            foreach(string texName in (ab as Mat).Textures)
            {
                DiffuseMaterial mat = new DiffuseMaterial();

                ab = milo[texName];
                if (ab == null || !(ab is Tex)) continue;

                HMXImage img = (ab as Tex).Image;
                if (img == null) img = TryOpenExternalImage((ab as Tex).ExternalPath);
                if (img == null) continue;

                // Converts texture to WPF-acceptable format
                ImageBrush brush = new ImageBrush();
                brush.ImageSource = Imaging.CreateBitmapSourceFromHBitmap(img.Hbitmap, // Don't forget to dispose
                                    IntPtr.Zero,
                                    Int32Rect.Empty,
                                    BitmapSizeOptions.FromEmptyOptions());
                brush.ViewportUnits = BrushMappingMode.Absolute;

                mat.Brush = brush;
                group.Children.Add(mat);
            }

            return (group.Children.Count > 0) ? group : null;
        }

        private HMXImage TryOpenExternalImage(string bmpPath)
        {
            IFile bmpFile = this.ark.GetFile(GetAbsolutePath(System.IO.Path.GetDirectoryName(this.arkFilePath), bmpPath));

            return HMXImage.FromStream(bmpFile.Stream);
        }

        private string GetAbsolutePath(string absoluteDirectory, string relativeFilePath)
        {
            absoluteDirectory = absoluteDirectory.Replace("\\", "/");
            relativeFilePath = relativeFilePath.Replace("\\", "/");

            while (relativeFilePath.StartsWith("../"))
            {
                relativeFilePath = relativeFilePath.Remove(0, 3);
                absoluteDirectory = absoluteDirectory.Remove(absoluteDirectory.LastIndexOf("/"));
            }

            string path = absoluteDirectory + "/" + relativeFilePath;
            path = System.IO.Path.GetDirectoryName(path) + "/gen/" + System.IO.Path.GetFileName(path) + GetPlatformExtension();
            return path.Replace("\\", "/");
        }

        private string GetPlatformExtension()
        {
            string ext = System.IO.Path.GetExtension(this.arkFilePath);

            switch(ext)
            {
                case ".gh":
                    return "_ps2";
                default:
                    return ext.Remove(0, ext.Length - ext.LastIndexOf('_'));
            }
        }
    }
}
