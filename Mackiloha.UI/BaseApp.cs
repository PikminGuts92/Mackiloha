using System;
using System.Collections.Generic;
using System.Text;
using Veldrid;

namespace Mackiloha.UI
{
    public abstract class BaseApp
    {
        public IApplicationWindow Window { get; }
        public GraphicsDevice GraphicsDevice { get; private set; }
        public ResourceFactory ResourceFactory { get; private set; }
        public Swapchain MainSwapchain { get; private set; }

        public BaseApp(IApplicationWindow window)
        {
            Window = window;
            Window.GraphicsDeviceCreated += OnGraphicsDeviceCreated;
            Window.GraphicsDeviceDestroyed += OnDeviceDestroyed;
            Window.Rendering += Draw;
            Window.KeyPressed += OnKeyDown;
        }

        public void OnGraphicsDeviceCreated(GraphicsDevice gd, ResourceFactory factory, Swapchain sc)
        {
            GraphicsDevice = gd;
            ResourceFactory = factory;
            MainSwapchain = sc;
            CreateResources(factory);
            CreateSwapchainResources(factory);
        }

        protected virtual void OnDeviceDestroyed()
        {
            GraphicsDevice = null;
            ResourceFactory = null;
            MainSwapchain = null;
        }

        protected virtual string GetTitle() => GetType().Name;

        protected abstract void CreateResources(ResourceFactory resourceFactory);

        protected virtual void CreateSwapchainResources(ResourceFactory resourceFactory) { }
        
        protected abstract void Draw(float deltaSeconds, long deltaTicks);
        
        protected virtual void OnKeyDown(KeyEvent ke) { }

        protected static byte[] GetEmbeddedResourceBytes(string resourceName)
        {
            var assembly = typeof(BaseApp).Assembly;
            using (var s = assembly.GetManifestResourceStream(resourceName))
            {
                byte[] ret = new byte[s.Length];
                s.Read(ret, 0, (int)s.Length);

                return ret;
            }
        }
    }
}
