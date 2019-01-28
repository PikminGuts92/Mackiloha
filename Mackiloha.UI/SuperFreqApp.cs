using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Veldrid;
using Veldrid.SPIRV;
using Mackiloha.Render;
using Mackiloha.UI.Components;

namespace Mackiloha.UI
{
    struct VertexPositionColor
    {
        public System.Numerics.Vector3 Position; // This is the position, in normalized device coordinates.
        public RgbaFloat Color; // This is the color of the vertex.
        public VertexPositionColor(System.Numerics.Vector3 position, RgbaFloat color)
        {
            Position = position;
            Color = color;
        }
        public const uint SizeInBytes = 28;
    }

    public class SuperFreqApp : BaseApp
    {
        private long TotalTicks;
        private readonly System.Numerics.Vector3 ClearColor = new System.Numerics.Vector3(0.2f, 0.2f, 0.3f); // Purple
        private readonly Main Main = new Main();

        private ImGuiRenderer Renderer;

        private Matrix4x4 ProjectionMatrix = Matrix4x4.Identity;
        private Matrix4x4 ViewMatrix = Matrix4x4.Identity;
        private Matrix4x4 WorldMatrix = Matrix4x4.Identity;

        private DeviceBuffer ProjectionBuffer;
        private DeviceBuffer ViewBuffer;
        private DeviceBuffer WorldBuffer;

        private ResourceSet ProjViewSet;
        private ResourceSet WorldTextureSet;

        private DeviceBuffer VertexBuffer;
        private DeviceBuffer IndexBuffer;
        private uint IndexSize;

        private CommandList CommandList;
        private Pipeline Pipeline;
        private Shader[] Shaders;

        public SuperFreqApp(IApplicationWindow window) : base(window)
        {
            Main.MiloChanged += LoadMilo;
        }

        protected void LoadMilo(MiloObjectDir milo)
        {
            var textures = milo.Entries
                .Where(x => x is Tex)
                .Select(y => y as Tex)
                .ToList();

            var views = milo.Entries
                .Where(x => x is View)
                .Select(y => y as View)
                .ToList();

            var meshes = milo.Entries
                .Where(x => x is Mesh)
                .Select(y => y as Mesh)
                .Where(z => !string.IsNullOrEmpty(z.Material)) // Don't care about bone meshes for now
                .ToList();

            var materials = milo.Entries
                .Where(x => x is Mat)
                .Select(y => y as Mat)
                //.Where(z => z.TextureEntries.Count > 0 && z.TextureEntries.Any(w => !string.IsNullOrEmpty(w.Texture))) // TODO: Idk?
                .ToList();

            var cams = milo.Entries
                .Where(x => x is Cam)
                .Select(y => y as Cam)
                .ToList();

            var environs = milo.Entries
                .Where(x => x is Environ)
                .Select(y => y as Environ)
                .ToList();

            var miloEntries = textures
                .Union<MiloObject>(views)
                .Union(meshes)
                .Union(materials)
                .Union(cams)
                .Union(environs)
                .ToList();

            var transEntries = miloEntries
                .Where(x => x is ITrans)
                .Select(y => y as ITrans)
                .ToList();

            var drawEntries = miloEntries
                .Where(x => x is IDraw)
                .Select(y => y as IDraw)
                .ToList();
            
            // Note: Doesn't take into account view transforms
            var verts = meshes
                .SelectMany(x =>
                {
                    var trans = transEntries.First(u => u.Name == x.Transform);
                    var mat = new Matrix4x4()
                    {
                        M11 = trans.Mat2.M11,
                        M12 = trans.Mat2.M12,
                        M13 = trans.Mat2.M13,
                        M14 = trans.Mat2.M14,
                        M21 = trans.Mat2.M21,
                        M22 = trans.Mat2.M22,
                        M23 = trans.Mat2.M23,
                        M24 = trans.Mat2.M24,
                        M31 = trans.Mat2.M31,
                        M32 = trans.Mat2.M32,
                        M33 = trans.Mat2.M33,
                        M34 = trans.Mat2.M34,
                        M41 = trans.Mat2.M41,
                        M42 = trans.Mat2.M42,
                        M43 = trans.Mat2.M43,
                        M44 = trans.Mat2.M44,
                    };
                    
                    var vs = x.Vertices
                        .Select(u =>
                        {
                            var pos = new System.Numerics.Vector3(u.X, u.Y, u.Z);
                            var norm = new System.Numerics.Vector3(u.NormalX, u.NormalY, u.NormalZ);

                            pos = System.Numerics.Vector3.Transform(pos, mat);
                            norm = System.Numerics.Vector3.TransformNormal(norm, mat);
                            
                            var v = u;
                            v.X = pos.X;
                            v.Y = pos.Y;
                            v.Z = pos.Z;
                            v.NormalX = norm.X;
                            v.NormalY = norm.Y;
                            v.NormalZ = norm.Z;

                            return v;
                        })
                        .ToArray();
                    
                    return vs;
                })
                .Select(y =>
                    new VertexPositionColor(new System.Numerics.Vector3(y.X, y.Y, y.Z),
                    new RgbaFloat(y.ColorR, y.ColorG, y.ColorB, y.ColorA)))
                .ToArray();

            // Get bounding box
            var bounding = new float[]
            {
                verts.Select(x => x.Position.X).Max() - verts.Select(x => x.Position.X).Min(),
                verts.Select(x => x.Position.Y).Max() - verts.Select(x => x.Position.Y).Min(),
                verts.Select(x => x.Position.Z).Max() - verts.Select(x => x.Position.Z).Min()
            };

            var center = new float[]
            {
                verts.Select(x => x.Position.X).Min() + bounding[0] / 2,
                verts.Select(x => x.Position.Y).Min() + bounding[1] / 2,
                verts.Select(x => x.Position.Z).Min() + bounding[2] / 2
            };

            /*
            var maxPos = verts
                .SelectMany(x => new float[] { x.Position.X, x.Position.Y, x.Position.Z })
                .Select( => Math.Abs(z))
                .Max(w => w);*/

            ProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(
                1.0f,
                (float)Window.Width / Window.Height,
                0.5f,
                bounding.Max() * 4);

            ViewMatrix = Matrix4x4.CreateLookAt(
                new System.Numerics.Vector3(0, bounding.Max() + 10, 50),
                System.Numerics.Vector3.Zero,
                System.Numerics.Vector3.UnitZ);

            WorldMatrix = Matrix4x4.CreateTranslation(-center[0], -center[1], -center[2]);

            verts = verts.Select(x =>
            {
                //x.Position = System.Numerics.Vector3.Transform(x.Position, camera);
                //x.Position = System.Numerics.Vector3.Transform(x.Position, perspective);

                return x;
            })
            .ToArray();


            int vertCount = 0;
            var faces = meshes
                .SelectMany(x =>
                {
                    var indices = x.Faces
                        .Select(y => new ushort[]
                        {
                            (ushort)(y.V1 + vertCount),
                            (ushort)(y.V2 + vertCount),
                            (ushort)(y.V3 + vertCount)
                        })
                        .ToArray();

                    vertCount += x.Vertices.Count;
                    return indices.SelectMany(z => z);
                })
                .ToArray();
            
            VertexBuffer.Dispose();
            IndexBuffer.Dispose();
            
            VertexBuffer = ResourceFactory.CreateBuffer(new BufferDescription((uint)verts.Length * VertexPositionColor.SizeInBytes, BufferUsage.VertexBuffer));
            IndexBuffer = ResourceFactory.CreateBuffer(new BufferDescription((uint)faces.Length * sizeof(ushort), BufferUsage.IndexBuffer));

            GraphicsDevice.UpdateBuffer(VertexBuffer, 0, verts);
            GraphicsDevice.UpdateBuffer(IndexBuffer, 0, faces);
            IndexSize = (uint)faces.Length;

            //System.Numerics.Vector3.Transform()
        }

        protected override void CreateResources(ResourceFactory resourceFactory)
        {
            ProjectionBuffer = resourceFactory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
            ViewBuffer = resourceFactory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
            WorldBuffer = resourceFactory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));

            ResourceLayout projViewLayout = resourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("Projection", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                    new ResourceLayoutElementDescription("View", ResourceKind.UniformBuffer, ShaderStages.Vertex)));

            ResourceLayout worldTextureLayout = resourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("World", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                    new ResourceLayoutElementDescription("SurfaceSampler", ResourceKind.Sampler, ShaderStages.Fragment)));
            
            VertexPositionColor[] quadVertices =
            {
                new VertexPositionColor(new System.Numerics.Vector3(-1.0f,  1.0f, 0.0f), RgbaFloat.Red),
                new VertexPositionColor(new System.Numerics.Vector3( 1.0f,  1.0f, 0.0f), RgbaFloat.Green),
                new VertexPositionColor(new System.Numerics.Vector3(-1.0f, -1.0f, 0.0f), RgbaFloat.Blue),
                new VertexPositionColor(new System.Numerics.Vector3( 1.0f, -1.0f, 0.0f), RgbaFloat.Yellow)
            };

            ushort[] quadIndices = { 0, 1, 2, 2, 1, 3 };
            IndexSize = (uint)quadIndices.Length;

            VertexBuffer = resourceFactory.CreateBuffer(new BufferDescription(4 * VertexPositionColor.SizeInBytes, BufferUsage.VertexBuffer));
            IndexBuffer = resourceFactory.CreateBuffer(new BufferDescription(IndexSize * sizeof(ushort), BufferUsage.IndexBuffer));

            GraphicsDevice.UpdateBuffer(VertexBuffer, 0, quadVertices);
            GraphicsDevice.UpdateBuffer(IndexBuffer, 0, quadIndices);

            VertexLayoutDescription vertexLayout = new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4));

            // Shaders
            ShaderDescription vertexShaderDesc = new ShaderDescription(
                ShaderStages.Vertex,
                GetEmbeddedResourceBytes("vertex.glsl"),
                "main");
            ShaderDescription fragmentShaderDesc = new ShaderDescription(
                ShaderStages.Fragment,
                GetEmbeddedResourceBytes("fragment.glsl"),
                "main");

            Shaders = resourceFactory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);

            ProjViewSet = resourceFactory.CreateResourceSet(new ResourceSetDescription(
                projViewLayout,
                ProjectionBuffer,
                ViewBuffer));

            WorldTextureSet = resourceFactory.CreateResourceSet(new ResourceSetDescription(
                worldTextureLayout,
                WorldBuffer,
                GraphicsDevice.Aniso4xSampler));

            // Pipeline
            GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription();
            pipelineDescription.BlendState = BlendStateDescription.SingleOverrideBlend;

            pipelineDescription.DepthStencilState = new DepthStencilStateDescription(
                depthTestEnabled: true,
                depthWriteEnabled: true,
                comparisonKind: ComparisonKind.LessEqual);

            pipelineDescription.RasterizerState = new RasterizerStateDescription(
                cullMode: FaceCullMode.Back,
                fillMode: PolygonFillMode.Solid,
                frontFace: FrontFace.CounterClockwise,
                depthClipEnabled: true,
                scissorTestEnabled: false);

            pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleList;
            //pipelineDescription.ResourceLayouts = System.Array.Empty<ResourceLayout>();
            pipelineDescription.ResourceLayouts = new[] { projViewLayout, worldTextureLayout };

            pipelineDescription.ShaderSet = new ShaderSetDescription(
                vertexLayouts: new VertexLayoutDescription[] { vertexLayout },
                shaders: Shaders);

            pipelineDescription.Outputs = GraphicsDevice.SwapchainFramebuffer.OutputDescription;
            Pipeline = resourceFactory.CreateGraphicsPipeline(pipelineDescription);

            CommandList = resourceFactory.CreateCommandList();

            // Imgui
            Renderer = new ImGuiRenderer(
                GraphicsDevice,
                GraphicsDevice.MainSwapchain.Framebuffer.OutputDescription,
                (int)Window.Width,
                (int)Window.Height);

            Window.Resized += ()
                => Renderer.WindowResized((int)Window.Width, (int)Window.Height);
            
            var miloPath = Environment.GetCommandLineArgs()
                .Skip(1)
                .FirstOrDefault();

            Main.LoadMilo(miloPath);
        }

        protected override void OnDeviceDestroyed()
        {
            base.OnDeviceDestroyed();

            ProjectionBuffer.Dispose();
            ViewBuffer.Dispose();
            WorldBuffer.Dispose();

            ProjViewSet.Dispose();
            WorldTextureSet.Dispose();

            VertexBuffer.Dispose();
            IndexBuffer.Dispose();

            CommandList.Dispose();
            Pipeline.Dispose();
            Renderer.Dispose();
        }
        
        protected override void Draw(float deltaSeconds, long deltaTicks)
        {
            TotalTicks += deltaTicks;

            // Feed the input events to our ImGui controller, which passes them through to ImGui
            if (Window is ApplicationWindow appWin)
            {
                var snapshot = appWin.Window.PumpEvents();
                Renderer.Update(deltaSeconds, snapshot);
            }

            Main.Render();

            CommandList.Begin();

            // Updates matrices
            /*
            CommandList.UpdateBuffer(ProjectionBuffer, 0, Matrix4x4.CreatePerspectiveFieldOfView(
                1.0f,
                (float)Window.Width / Window.Height,
                0.5f,
                100f));

            CommandList.UpdateBuffer(
                ViewBuffer,
                0,
                Matrix4x4.CreateLookAt(
                    System.Numerics.Vector3.UnitZ * 2.5f,
                    System.Numerics.Vector3.Zero,
                    System.Numerics.Vector3.UnitY));

            Matrix4x4 rotation =
                Matrix4x4.CreateFromAxisAngle(System.Numerics.Vector3.UnitY, (TotalTicks * 2.0f / 20000f))
                * Matrix4x4.CreateFromAxisAngle(System.Numerics.Vector3.UnitX, (TotalTicks / 12000f));
                
            CommandList.UpdateBuffer(WorldBuffer, 0, ref rotation);*/

            //CommandList.UpdateBuffer(ProjectionBuffer, 0, Matrix4x4.Identity);
            //CommandList.UpdateBuffer(ViewBuffer, 0, Matrix4x4.Identity);
            //CommandList.UpdateBuffer(WorldBuffer, 0, Matrix4x4.Identity);

            
            Matrix4x4 rotation =
                Matrix4x4.CreateFromAxisAngle(System.Numerics.Vector3.UnitZ, (TotalTicks * 2.0f / 20000f));

            CommandList.UpdateBuffer(ProjectionBuffer, 0, ProjectionMatrix);
            CommandList.UpdateBuffer(ViewBuffer, 0, ViewMatrix);
            CommandList.UpdateBuffer(WorldBuffer, 0, WorldMatrix * rotation);

            CommandList.SetFramebuffer(GraphicsDevice.MainSwapchain.Framebuffer);
            CommandList.ClearColorTarget(0, new RgbaFloat(ClearColor.X, ClearColor.Y, ClearColor.Z, 1.0f));
            CommandList.ClearDepthStencil(1f);

            CommandList.SetPipeline(Pipeline);
            CommandList.SetVertexBuffer(0, VertexBuffer);
            CommandList.SetIndexBuffer(IndexBuffer, IndexFormat.UInt16);
            
            CommandList.SetGraphicsResourceSet(0, ProjViewSet);
            CommandList.SetGraphicsResourceSet(1, WorldTextureSet);
            CommandList.DrawIndexed(IndexSize, 1, 0, 0, 0);

            Renderer.Render(GraphicsDevice, CommandList);
            CommandList.End();

            GraphicsDevice.SubmitCommands(CommandList);
            GraphicsDevice.SwapBuffers(GraphicsDevice.MainSwapchain);
            GraphicsDevice.WaitForIdle();
        }
    }
}
