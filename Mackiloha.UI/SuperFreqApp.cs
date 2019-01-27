using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Veldrid;
using Veldrid.SPIRV;
using Mackiloha.UI.Components;

namespace Mackiloha.UI
{
    struct VertexPositionColor
    {
        public System.Numerics.Vector2 Position; // This is the position, in normalized device coordinates.
        public RgbaFloat Color; // This is the color of the vertex.
        public VertexPositionColor(System.Numerics.Vector2 position, RgbaFloat color)
        {
            Position = position;
            Color = color;
        }
        public const uint SizeInBytes = 24;
    }

    public class SuperFreqApp : BaseApp
    {
        private readonly System.Numerics.Vector3 ClearColor = new System.Numerics.Vector3(0.2f, 0.2f, 0.3f); // Purple
        private readonly Main Main = new Main();

        private ImGuiRenderer Renderer;
        private DeviceBuffer VertexBuffer;
        private DeviceBuffer IndexBuffer;
        private CommandList CommandList;
        private Pipeline Pipeline;
        private Shader[] Shaders;

        public SuperFreqApp(ApplicationWindow window) : base(window) { }

        protected override void CreateResources(ResourceFactory resourceFactory)
        {
            VertexPositionColor[] quadVertices =
            {
                new VertexPositionColor(new System.Numerics.Vector2(-0.95f,  0.95f), RgbaFloat.Red),
                new VertexPositionColor(new System.Numerics.Vector2( 0.95f,  0.95f), RgbaFloat.Green),
                new VertexPositionColor(new System.Numerics.Vector2(-0.95f, -0.95f), RgbaFloat.Blue),
                new VertexPositionColor(new System.Numerics.Vector2( 0.95f, -0.95f), RgbaFloat.Yellow)
            };

            ushort[] quadIndices = { 0, 1, 2, 3 };

            VertexBuffer = resourceFactory.CreateBuffer(new BufferDescription(4 * VertexPositionColor.SizeInBytes, BufferUsage.VertexBuffer));
            IndexBuffer = resourceFactory.CreateBuffer(new BufferDescription(4 * sizeof(ushort), BufferUsage.IndexBuffer));

            GraphicsDevice.UpdateBuffer(VertexBuffer, 0, quadVertices);
            GraphicsDevice.UpdateBuffer(IndexBuffer, 0, quadIndices);

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

            Shaders = resourceFactory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);

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
        }

        protected override void OnDeviceDestroyed()
        {
            base.OnDeviceDestroyed();
            CommandList.Dispose();
            Pipeline.Dispose();
            Renderer.Dispose();
        }
        
        protected override void Draw(float deltaSeconds, long deltaTicks)
        {
            // Feed the input events to our ImGui controller, which passes them through to ImGui
            if (Window is ApplicationWindow appWin)
            {
                var snapshot = appWin.Window.PumpEvents();
                Renderer.Update(deltaSeconds, snapshot);
            }

            Main.Render();

            CommandList.Begin();
            CommandList.SetFramebuffer(GraphicsDevice.MainSwapchain.Framebuffer);
            CommandList.ClearColorTarget(0, new RgbaFloat(ClearColor.X, ClearColor.Y, ClearColor.Z, 1.0f));

            CommandList.SetVertexBuffer(0, VertexBuffer);
            CommandList.SetIndexBuffer(IndexBuffer, IndexFormat.UInt16);
            CommandList.SetPipeline(Pipeline);
            CommandList.DrawIndexed(4, 1, 0, 0, 0);

            Renderer.Render(GraphicsDevice, CommandList);
            CommandList.End();

            GraphicsDevice.SubmitCommands(CommandList);
            GraphicsDevice.SwapBuffers(GraphicsDevice.MainSwapchain);
        }
    }
}
