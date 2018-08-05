using System;
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
using System.ComponentModel; // SortDescription
using Mackiloha;
using Mackiloha.Ark;
using Mackiloha.Milo;
using HelixToolkit;
using HelixToolkit.Wpf;
using System.IO;
using System.Windows.Media.Media3D;
//using System.Drawing; // TODO: Use something else for images
using System.Windows.Interop;

namespace SuperFreq
{
    /// <summary>
    /// Interaction logic for MiloEditor.xaml
    /// </summary>
    public partial class MiloEditor : UserControl
    {
        private MiloFile milo;
        private Archive ark;
        private string arkFilePath;

        public MiloEditor()
        {
            InitializeComponent();

            TreeView_MiloArchive.SelectedItemChanged += TreeView_MiloArchive_SelectedItemChanged;
        }

        private void TreeView_MiloArchive_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            OpenSelectedFile();
        }

        public void SetArk(Archive arkInput) => this.ark = arkInput;
        public void SetFilePath(string path) => this.arkFilePath = path;

        public void OpenMiloFile(Stream source)
        {
            ClearTreeView();
            if (source == null) return;
            
            // Opens input milo file
            milo = MiloFile.FromStream(source);

            /*
            foreach (var entry in milo.Entries)
            {
                if (!(entry is Mesh)) continue;
                Mesh mesh = entry as Mesh;

                HelixViewport3D.Children.Add(CreateModel(mesh, milo));
            }*/

            RefreshTreeView();
        }

        private void ClearTreeView()
        {
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
        }

        private void UnregisterNode(TreeViewItem parent)
        {
            foreach (TreeViewItem child in parent.Items)
            {
                UnregisterNode(child);
            }

            // Unregisters name
            TreeView_MiloArchive.UnregisterName(parent.Name);
        }

        private void SortNode(TreeViewItem parent)
        {
            foreach (TreeViewItem child in parent.Items)
            {
                SortNode(child);
            }

            // Sorts nodes
            parent.Items.SortDescriptions.Clear();
            parent.Items.SortDescriptions.Add(new SortDescription("Name", System.ComponentModel.ListSortDirection.Ascending));
        }

        private string CreateKey(string path, bool folder)
        {
            if (path == "") return "_";

            List<char> newKey = new List<char>();

            newKey.Add((folder) ? 'a' : 'b'); // So folders sort first

            foreach (char c in path)
            {
                if ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
                    newKey.Add(c);
                else if (c == '.')
                {
                    // So that periods sort before slashes
                    newKey.Add('_');
                    newKey.Add('a');
                }
                else if (c == '_')
                {
                    newKey.Add('_');
                    newKey.Add('b');
                }
                else
                    newKey.Add('_');
            }

            // Returns unique key
            return string.Concat(newKey);
        }

        private TreeViewItem AddNode(TreeViewItem parent, string currentPath, string text, bool folder)
        {
            //node.Items.Cast<TreeViewItem>().
            string key = CreateKey(currentPath, folder);
            object needle = TreeView_MiloArchive.FindName(key);

            if (needle != null)
                return needle as TreeViewItem;
            else
            {
                //TreeFileEntry temp = new TreeFileEntry();
                TreeViewItem child = new TreeViewItem();
                child.Header = text;
                child.Name = key;
                //temp.Path = currentPath;
                TreeView_MiloArchive.RegisterName(key, child);

                int returnIdx;
                returnIdx = parent.Items.Add(child);
                //child.Tag = new TreeArkEntryInfo(currentPath, folder, key);
                //SetNodeProperties(child);


                return parent.Items[returnIdx] as TreeViewItem;
            }
        }

        private void RefreshTreeView()
        {
            // Unregisters nodes
            foreach (TreeViewItem node in TreeView_MiloArchive.Items)
                UnregisterNode(node);

            this.TreeView_MiloArchive.Items.Clear();
            if (this.milo == null) return;

            TreeViewItem root = new TreeViewItem();
            root.Header = "Resources";
            root.Tag = ark;
            root.Name = "_";
            TreeView_MiloArchive.RegisterName("_", root);

            var miloEntries = milo.Entries.GroupBy(x => x.Type);

            foreach(var entryType in miloEntries)
            {
                TreeViewItem typeNode = new TreeViewItem();
                typeNode.Header = entryType.Key;
                typeNode.Name = CreateKey(entryType.Key, true);
                RegisterName(typeNode.Name, typeNode);

                foreach (var entry in entryType)
                {
                    TreeViewItem entryNode = new TreeViewItem();
                    entryNode.Header = entry.Name;
                    entryNode.Name = CreateKey(entry.Name, false);
                    entryNode.Tag = entry;
                    entryNode.MouseDoubleClick += EntryNode_MouseDoubleClick;

                    RegisterName(entryNode.Name, entryNode);
                    typeNode.Items.Add(entryNode);
                }

                root.Items.Add(typeNode);
            }

            // Sorts nodes
            SortNode(root);
            TreeView_MiloArchive.Items.Add(root);
        }

        private void EntryNode_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenSelectedFile();
        }

        private void OpenSelectedFile()
        {
            ClearTreeView(); // Clears view
            if (TreeView_MiloArchive.SelectedItem == null || !(TreeView_MiloArchive.SelectedItem is TreeViewItem)) return;

            TreeViewItem item = TreeView_MiloArchive.SelectedItem as TreeViewItem;
            if (item.Tag == null || !(item.Tag is AbstractEntry)) return;

            AbstractEntry entry = item.Tag as AbstractEntry;

            switch (entry.Type)
            {
                case "Mesh":
                    HelixViewport3D.Children.Add(CreateModel((Mesh)entry, milo));
                    break;
                case "View":
                    View view = entry as View;

                    List<ModelVisual3D> models = CreateModelsFromView(view, milo);
                    List<View> subviews = view.Views.Select(x => milo[x] as View).ToList();
                    subviews.ForEach(x => models.AddRange(CreateModelsFromView(x, milo)));
                    
                    models.ForEach(x => HelixViewport3D.Children.Add(x));
                    break;
                case "Tex":
                    return;
                case "Mat":
                    break;
                default:
                    return;
            }
            
            /*
            ObjExporter ex = new ObjExporter();
            //ex.ExportNormals = true;
            ex.MaterialsFile = "test.mtl";
            ex.TextureFolder = "textures";

            using (FileStream fs = File.OpenWrite("test.obj"))
            {
                ex.Export(HelixViewport3D.Viewport, fs);
            }
            */
        }

        public List<ModelVisual3D> CreateModelsFromView(View view, MiloFile milo)
        {
            List<ModelVisual3D> models = new List<ModelVisual3D>();
            if (view == null || milo == null) return models;

            foreach (string name in view.Meshes)
            {
                AbstractEntry entry = milo[name];

                if (entry == null) continue;
                else if (entry is Mesh)
                    models.Add(CreateModel(entry as Mesh, milo));
                else if (entry is View)
                    models.AddRange(CreateModelsFromView(entry as View, milo));
                //else
                    //throw new Exception("Unknown mesh type?");
            }

            return models;
        }
        
        public ModelVisual3D CreateModel(Mesh mesh, MiloFile milo)
        {
            // ----- MESH STUFF -----
            MeshGeometry3D mesh3d = new MeshGeometry3D();
            
            Int32Collection indices = new Int32Collection(mesh.Faces.Length * 3);
            
            foreach (ushort[] face in mesh.Faces)
            {
                indices.Add(face[0]);
                indices.Add(face[1]);
                indices.Add(face[2]);
            }
            
            mesh3d.Positions = new Point3DCollection(mesh.Vertices.Select(x => new Point3D(x.VertX, x.VertY, x.VertZ)));
            mesh3d.Normals = new Vector3DCollection(mesh.Vertices.Select(x => new Vector3D(x.NormX, x.NormY, x.NormZ)));
            mesh3d.TextureCoordinates = new PointCollection(mesh.Vertices.Select(x => new Point(x.U, x.V)));
            mesh3d.TriangleIndices = indices;

            //mesh3d.SetName(mesh.Name);

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
            geom3d.BackMaterial = mat; // Visibility on both sides

            model3d.Content = geom3d;
            model3d.SetName(mesh.Name);

            Matrix3D mat3d;

            if (mesh.Name == mesh.Transform)
                mat3d = ConvertMatrix(mesh.Mat2);
            else
            {
                AbstractEntry entry = milo[mesh.Transform];

                if (entry is Mesh)
                    mat3d = ConvertMatrix((entry as Mesh).Mat2);
                else if (entry is View)
                    mat3d = ConvertMatrix((entry as View).Mat2);
                else if (entry is Trans)
                    mat3d = ConvertMatrix((entry as Trans).Mat2);
                else if (entry == null)
                    // TODO: Update this!
                    // Hotfix until parsing of gh2 directory is complete
                    mat3d = ConvertMatrix(Mackiloha.Matrix.Identity());
                else
                    throw new Exception("Unknown mesh type");
            }
            
            model3d.Transform = new MatrixTransform3D(mat3d);
            model3d.SetName(mesh.Name);
            
            return model3d;
        }

        private Matrix3D ConvertMatrix(Mackiloha.Matrix mat)
        {
            Matrix3D mat3d = Matrix3D.Identity;

            mat3d.M11 = mat.M11;
            mat3d.M12 = mat.M12;
            mat3d.M13 = mat.M13;

            mat3d.M21 = mat.M21;
            mat3d.M22 = mat.M22;
            mat3d.M23 = mat.M23;

            mat3d.M31 = mat.M31;
            mat3d.M32 = mat.M32;
            mat3d.M33 = mat.M33;

            mat3d.OffsetX = mat.M41;
            mat3d.OffsetY = mat.M42;
            mat3d.OffsetZ = mat.M43;

            return mat3d;
        }

        private Dictionary<string, Material> _materials = new Dictionary<string, Material>();
        
        private Material GetMaterial(string matName, MiloFile milo)
        {
            Material material;

            if (_materials.TryGetValue(matName, out material))
                return material;

            // Materials can have multiple textures
            MaterialGroup group = new MaterialGroup();
            group.SetName(matName);
            
            AbstractEntry ab = milo[matName];
            if (ab == null || !(ab is Mat)) return null;

            // Enumerates textures in material
            foreach(string texName in (ab as Mat).Textures)
            {
                DiffuseMaterial mat = new DiffuseMaterial();
                
                // Converts texture to WPF-acceptable format
                ImageBrush brush = GetImage(texName, milo);
                if (brush == null) continue;

                mat.Brush = brush;
                group.Children.Add(mat);
            }

            if (group.Children.Count <= 0) return null;

            _materials.Add(matName, group);
            return group;
        }

        private Dictionary<string, ImageBrush> _images = new Dictionary<string, ImageBrush>();

        private ImageBrush GetImage(string texName, MiloFile milo)
        {
            ImageBrush imageBrush;

            if (_images.TryGetValue(texName, out imageBrush))
                return imageBrush;

            AbstractEntry ab = milo[texName];
            if (ab == null || !(ab is Tex)) return null;

            HMXImage img = (ab as Tex).Image;
            if (img == null) img = TryOpenExternalImage((ab as Tex).ExternalPath);
            //if (img == null) continue;
            if (img == null) throw new Exception("Couldn't find texture...");

            // Converts texture to WPF-acceptable format
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = Imaging.CreateBitmapSourceFromHBitmap(img.Hbitmap, // Don't forget to dispose
                                IntPtr.Zero,
                                Int32Rect.Empty,
                                BitmapSizeOptions.FromEmptyOptions());
            brush.ViewportUnits = BrushMappingMode.Absolute;
            
            brush.SetName(texName);
            _images.Add(texName, brush);

            return brush;
        }

        private HMXImage TryOpenExternalImage(string bmpPath)
        {
            try
            {
                ArkEntry bmpFile = ark[GetAbsolutePath(System.IO.Path.GetDirectoryName(this.arkFilePath), bmpPath)];

                return HMXImage.FromStream(ark.GetArkEntryFileStream(bmpFile));
            }
            catch
            {
                return null;
            }
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
