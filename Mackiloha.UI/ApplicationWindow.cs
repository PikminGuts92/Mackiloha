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
        private readonly Sdl2Window Window;
        private GraphicsDevice GraphicsDevice;
        private DisposeCollectorResourceFactory ResourceFactory;
        private bool WindowResized = true;

        public event Action<float> Rendering;
        public event Action<GraphicsDevice, ResourceFactory, Swapchain> GraphicsDeviceCreated;
        public event Action GraphicsDeviceDestroyed;
        public event Action Resized;
        public event Action<KeyEvent> KeyPressed;

        public uint Width => (uint)Window.Width;
        public uint Height => (uint)Window.Height;
        
        public ApplicationWindow(string name)
        {
            // Create window
            var info = new WindowCreateInfo
            {
                X = 100,
                Y = 100,
                WindowWidth = 1280,
                WindowHeight = 720,
                WindowInitialState = WindowState.FullScreen, // TODO: Read from config
                WindowTitle = name
            };
            Window = VeldridStartup.CreateWindow(ref info);
            Window.Resized += () => WindowResized = true;
            Window.KeyDown += OnKeyDown;
        }

        public void Run()
        {
            // Graphics device options
            var options = new GraphicsDeviceOptions
            {
                Debug = false,
                SwapchainDepthFormat = null, // PixelFormat.R16_UNorm,
                SyncToVerticalBlank = true,
                ResourceBindingModel = ResourceBindingModel.Improved,
                PreferDepthRangeZeroToOne = true,
                PreferStandardClipSpaceYDirection = true
            };
#if DEBUG
            options.Debug = true;
#endif

            // Setup graphics device
            GraphicsDevice = VeldridStartup.CreateGraphicsDevice(Window, options, GraphicsBackend.OpenGL);
            ResourceFactory = new DisposeCollectorResourceFactory(GraphicsDevice.ResourceFactory);
            GraphicsDeviceCreated?.Invoke(GraphicsDevice, ResourceFactory, GraphicsDevice.MainSwapchain);

            var sw = Stopwatch.StartNew();
            var previousElapsed = sw.Elapsed.TotalSeconds;

            // Main loop
            while (Window.Exists)
            {
                double newElapsed = sw.Elapsed.TotalSeconds;
                float deltaSeconds = (float)(newElapsed - previousElapsed);

                InputSnapshot inputSnapshot = Window.PumpEvents();
                if (!Window.Exists) continue;
                
                previousElapsed = newElapsed;
                if (WindowResized)
                {
                    WindowResized = false;
                    GraphicsDevice.ResizeMainWindow((uint)Window.Width, (uint)Window.Height);
                    Resized?.Invoke();
                }

                Rendering?.Invoke(deltaSeconds);
            }
        }

        protected void OnKeyDown(KeyEvent keyEvent)
        {
            KeyPressed?.Invoke(keyEvent);
        }
    }
}
