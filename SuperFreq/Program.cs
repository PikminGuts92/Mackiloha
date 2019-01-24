using System;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.SPIRV;
using Veldrid.StartupUtilities;
using SuperFreq.Components;
using System.Text;

namespace SuperFreq
{
    struct VertexPositionColor
    {
        public Vector2 Position; // This is the position, in normalized device coordinates.
        public RgbaFloat Color; // This is the color of the vertex.
        public VertexPositionColor(Vector2 position, RgbaFloat color)
        {
            Position = position;
            Color = color;
        }
        public const uint SizeInBytes = 24;
    }

    class Program
    {
        static void Main(string[] args)
        {
            VeldridStartup.CreateWindowAndGraphicsDevice(
                new WindowCreateInfo(50, 50, 1280, 720, WindowState.Maximized, "SuperFreQ"),
                new GraphicsDeviceOptions(true, null, true),
                GraphicsBackend.OpenGL,
                out Sdl2Window window,
                out GraphicsDevice graphicsDevice);

            ImGuiRenderer renderer = new ImGuiRenderer(
                graphicsDevice,
                graphicsDevice.MainSwapchain.Framebuffer.OutputDescription,
                window.Width,
                window.Height);
            
            //Vector3 clearColor = new Vector3(0.45f, 0.55f, 0.6f); // Blue
            Vector3 clearColor = new Vector3(0.2f, 0.2f, 0.3f); // Purple

            window.Resized += () =>
            {
                graphicsDevice.MainSwapchain.Resize((uint)window.Width, (uint)window.Height);
                renderer.WindowResized(window.Width, window.Height);
            };

            ResourceFactory resourceFactory = graphicsDevice.ResourceFactory;
            CreateResources(graphicsDevice, resourceFactory);
            CommandList commandList = resourceFactory.CreateCommandList();

            var main = new Main();
            // Main application loop
            while (window.Exists)
            {
                InputSnapshot snapshot = window.PumpEvents();
                if (!window.Exists) break;

                // Feed the input events to our ImGui controller, which passes them through to ImGui.
                renderer.Update(1.0f / 60.0f, snapshot);

                main.Render();

                commandList.Begin();
                commandList.SetFramebuffer(graphicsDevice.MainSwapchain.Framebuffer);
                commandList.ClearColorTarget(0, new RgbaFloat(clearColor.X, clearColor.Y, clearColor.Z, 1.0f));

                commandList.SetVertexBuffer(0, vertexBuffer);
                commandList.SetIndexBuffer(indexBuffer, IndexFormat.UInt16);
                commandList.SetPipeline(pipeline);
                commandList.DrawIndexed(4, 1, 0, 0, 0);

                renderer.Render(graphicsDevice, commandList);
                commandList.End();

                graphicsDevice.SubmitCommands(commandList);
                graphicsDevice.SwapBuffers(graphicsDevice.MainSwapchain);
            }

            // Clean up Veldrid resources
            graphicsDevice.WaitForIdle();
            renderer.Dispose();
            commandList.Dispose();
            graphicsDevice.Dispose();
        }
        
        private static DeviceBuffer vertexBuffer;
        private static DeviceBuffer indexBuffer;
        private static Pipeline pipeline;
        private static Shader[] shaders;

        private static byte[] GetEmbeddedResourceBytes(string resourceName)
        {
            var assembly = typeof(Program).Assembly;
            using (var s = assembly.GetManifestResourceStream(resourceName))
            {
                byte[] ret = new byte[s.Length];
                s.Read(ret, 0, (int)s.Length);
                
                return Encoding.UTF8.GetBytes(Encoding.UTF8.GetString(ret));

                //return ret;
            }
        }

        private static void CreateResources(GraphicsDevice graphicsDevice, ResourceFactory resourceFactory)
        {
            VertexPositionColor[] quadVertices =
            {
                new VertexPositionColor(new Vector2(-0.95f,  0.95f), RgbaFloat.Red),
                new VertexPositionColor(new Vector2( 0.95f,  0.95f), RgbaFloat.Green),
                new VertexPositionColor(new Vector2(-0.95f, -0.95f), RgbaFloat.Blue),
                new VertexPositionColor(new Vector2( 0.95f, -0.95f), RgbaFloat.Yellow)
            };

            ushort[] quadIndices = { 0, 1, 2, 3 };

            vertexBuffer = resourceFactory.CreateBuffer(new BufferDescription(4 * VertexPositionColor.SizeInBytes, BufferUsage.VertexBuffer));
            indexBuffer = resourceFactory.CreateBuffer(new BufferDescription(4 * sizeof(ushort), BufferUsage.IndexBuffer));

            graphicsDevice.UpdateBuffer(vertexBuffer, 0, quadVertices);
            graphicsDevice.UpdateBuffer(indexBuffer, 0, quadIndices);

            VertexLayoutDescription vertexLayout = new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.Position, VertexElementFormat.Float2),
                new VertexElementDescription("Color", VertexElementSemantic.Color, VertexElementFormat.Float4));
            
            // Shaders
            ShaderDescription vertexShaderDesc = new ShaderDescription(
                ShaderStages.Vertex,
                GetEmbeddedResourceBytes("vertex.glsl"),
                "main");
            ShaderDescription fragmentShaderDesc = new ShaderDescription(
                ShaderStages.Fragment,
                GetEmbeddedResourceBytes("fragment.glsl"),
                "main");

            shaders = resourceFactory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);

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
                frontFace: FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: false);

            pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
            pipelineDescription.ResourceLayouts = System.Array.Empty<ResourceLayout>();
            
            pipelineDescription.ShaderSet = new ShaderSetDescription(
                vertexLayouts: new VertexLayoutDescription[] { vertexLayout },
                shaders: shaders);

            pipelineDescription.Outputs = graphicsDevice.SwapchainFramebuffer.OutputDescription;
            pipeline = resourceFactory.CreateGraphicsPipeline(pipelineDescription);

            //var commandList = resourceFactory.CreateCommandList();
        }
    }
}
