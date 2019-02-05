using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using Veldrid.Utilities;

namespace Mackiloha.UI
{
    public class ApplicationWindow : IApplicationWindow
    {
        private GraphicsDevice GraphicsDevice;
        private DisposeCollectorResourceFactory ResourceFactory;
        private bool WindowResized = true;

        public event Action<float, long> Rendering;
        public event Action<GraphicsDevice, ResourceFactory, Swapchain> GraphicsDeviceCreated;
        public event Action GraphicsDeviceDestroyed;
        public event Action Resized;
        public event Action<KeyEvent> KeyPressed;

        internal Sdl2Window Window { get; private set; }
        public uint Width => (uint)Window.Width;
        public uint Height => (uint)Window.Height;

        public ApplicationWindow() { }

        public IApplicationWindow Init(string name)
        {
            // Create window
            var info = new WindowCreateInfo
            {
                X = 100,
                Y = 100,
                WindowWidth = 1280,
                WindowHeight = 720,
                WindowInitialState = WindowState.Maximized, // TODO: Read from config
                WindowTitle = name
            };
            Window = VeldridStartup.CreateWindow(ref info);
            Window.Resized += () => WindowResized = true;
            Window.KeyDown += OnKeyDown;

            return this;
        }

        public void Run()
        {
            // Graphics device options
            var options = new GraphicsDeviceOptions
            {
                Debug = false,
                SwapchainDepthFormat = PixelFormat.R16_UNorm, // PixelFormat.R16_UNorm,
                SyncToVerticalBlank = true,
                ResourceBindingModel = ResourceBindingModel.Improved,
                PreferDepthRangeZeroToOne = true,
                PreferStandardClipSpaceYDirection = true,
                HasMainSwapchain = true
            };
#if DEBUG
            options.Debug = true;
#endif

            // Setup graphics device
            GraphicsDevice = VeldridStartup.CreateGraphicsDevice(Window, options, GraphicsBackend.Direct3D11);
            ResourceFactory = new DisposeCollectorResourceFactory(GraphicsDevice.ResourceFactory);
            GraphicsDeviceCreated?.Invoke(GraphicsDevice, ResourceFactory, GraphicsDevice.MainSwapchain);
            var commandList = ResourceFactory.CreateCommandList();

            // Setup imgui
            //ImGuiRenderer renderer = new ImGuiRenderer(
            //    GraphicsDevice,
            //    GraphicsDevice.MainSwapchain.Framebuffer.OutputDescription,
            //    Window.Width,
            //    Window.Height);
            
            var sw = Stopwatch.StartNew();
            var prevElapsed = sw.Elapsed.TotalSeconds;
            var prevElapsedTicks = sw.Elapsed.Ticks;
            
            // Main loop
            while (Window.Exists)
            {
                double newElapsed = sw.Elapsed.TotalSeconds;
                long newElapsedTicks = sw.Elapsed.Ticks; 

                float deltaSeconds = (float)(newElapsed - prevElapsed);
                var deltaTicks = newElapsedTicks = prevElapsedTicks;

                //InputSnapshot snapshot = Window.PumpEvents();
                if (!Window.Exists) continue;

                // Feed the input events to our ImGui controller, which passes them through to ImGui
                //renderer.Update(1.0f / 60.0f, snapshot);
                
                prevElapsed = newElapsed;
                if (WindowResized)
                {
                    WindowResized = false;
                    GraphicsDevice.ResizeMainWindow((uint)Window.Width, (uint)Window.Height);
                    //renderer.WindowResized(Window.Width, Window.Height);
                    Resized?.Invoke();
                }

                Rendering?.Invoke(deltaSeconds, deltaTicks);
                
                // Draw imgui
                //commandList.Begin();
                
                //renderer.Render(GraphicsDevice, commandList);
                //commandList.End();

                //GraphicsDevice.SubmitCommands(commandList);
                //GraphicsDevice.SwapBuffers(GraphicsDevice.MainSwapchain);
                //GraphicsDevice.WaitForIdle();
            }

            // Clean up resources
            GraphicsDevice.WaitForIdle();
            //renderer.Dispose();
            commandList.Dispose();
            ResourceFactory.DisposeCollector.DisposeAll();
            GraphicsDevice.Dispose();
            GraphicsDeviceDestroyed?.Invoke();
        }

        protected void OnKeyDown(KeyEvent keyEvent)
        {
            KeyPressed?.Invoke(keyEvent);
        }
    }
}
