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
using Mackiloha;
using Mackiloha.Milo;
using HelixToolkit;
using HelixToolkit.Wpf;
using System.IO;
using System.Windows.Media.Media3D;


namespace SuperFreq
{
    /// <summary>
    /// Interaction logic for MiloEditor.xaml
    /// </summary>
    public partial class MiloEditor : UserControl
    {
        public MiloEditor()
        {
            InitializeComponent();
        }

        public void OpenMiloFile(Stream source)
        {
            if (source == null) return;

            HelixViewport3D.Children.Clear();
            HelixViewport3D.Children.Add(new DefaultLights());

            // Opens input milo file
            MiloFile milo = MiloFile.FromStream(source);
            
            foreach (var entry in milo.Entries)
            {
                if (!(entry is Mesh)) continue;
                Mesh mesh = entry as Mesh;

                HelixViewport3D.Children.Add(CreateModel(mesh));
            }
        }

        public ModelVisual3D CreateModel(Mesh mesh)
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
                uvs.Add(new Point(vert.U, vert.V));

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

            // ----- MATERIAL STUFF -----
            DiffuseMaterial mat = new DiffuseMaterial();
            SolidColorBrush brush = new SolidColorBrush();
            brush.Color = Color.FromRgb(0x80, 0x80, 0x80); // Grey
            mat.Brush = brush;

            // Putting it all together
            ModelVisual3D model3d = new ModelVisual3D();
            GeometryModel3D geom3d = new GeometryModel3D();

            geom3d.Geometry = mesh3d;
            geom3d.Material = mat;
            model3d.Content = geom3d;
            
            return model3d;
        }
    }
}
