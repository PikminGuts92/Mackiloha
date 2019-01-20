using System;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using SuperFreq.Components;

namespace SuperFreq
{
    class Program
    {
        static void Main(string[] args)
        {
            VeldridStartup.CreateWindowAndGraphicsDevice(
                new WindowCreateInfo(50, 50, 1280, 720, WindowState.Maximized, "SuperFreQ"),
                new GraphicsDeviceOptions(true, null, true),
                out Sdl2Window window,
                out GraphicsDevice graphicsDevice);

            ImGuiRenderer renderer = new ImGuiRenderer(
                graphicsDevice,
                graphicsDevice.MainSwapchain.Framebuffer.OutputDescription,
                window.Width,
                window.Height);

            CommandList commandList = graphicsDevice.ResourceFactory.CreateCommandList();

            Vector3 clearColor = new Vector3(0.45f, 0.55f, 0.6f);

            window.Resized += () =>
            {
                graphicsDevice.MainSwapchain.Resize((uint)window.Width, (uint)window.Height);
                renderer.WindowResized(window.Width, window.Height);
            };

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
    }
}
