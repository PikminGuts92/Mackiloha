using System;
using System.Collections.Generic;
using System.Text;
using Veldrid;

namespace Mackiloha.UI
{
    public interface IApplicationWindow
    {
        event Action<float, long> Rendering;
        event Action<GraphicsDevice, ResourceFactory, Swapchain> GraphicsDeviceCreated;
        event Action GraphicsDeviceDestroyed;
        event Action Resized;
        event Action<KeyEvent> KeyPressed;

        uint Width { get; }
        uint Height { get; }

        void Run();
    }
}
