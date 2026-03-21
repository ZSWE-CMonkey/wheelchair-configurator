using System.Runtime.InteropServices;

namespace ConfigurationLogic.Graphics
{
    internal class VulkanGraphicsPluginImpl : IGraphicsPlugin
    {
        [DllImport("WheelchairGraphics")]
        private static extern void wgInitializeVulkanGraphicsWIN32([MarshalAs(UnmanagedType.LPStr)] string appName, IntPtr window);
        
        [DllImport("WheelchairGraphics")]
        private static extern void wgDeinitializeGraphics();

        [DllImport("WheelchairGraphics")]
        private static extern void wgRender();

        public bool Initialize()
        {
            //wgInitializeVulkanGraphicsWIN32
            throw new NotImplementedException();
        }

        public void Render()
        {
            //wgRender()
            throw new NotImplementedException();
        }

        public void Deinitialize()
        {
            //wgDeinitializeGraphics()
            throw new NotImplementedException();
        }
    }
}
