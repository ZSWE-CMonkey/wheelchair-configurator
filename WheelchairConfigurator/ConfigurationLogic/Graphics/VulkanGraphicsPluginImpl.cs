using ConfigurationLogic.Graphics.Types;
using System.Runtime.InteropServices;

namespace ConfigurationLogic.Graphics
{
    internal class VulkanGraphicsPluginImpl : IGraphicsPlugin
    {
        [DllImport("WheelchairGraphics")]
        private static extern void wgInitializeVulkanGraphicsWIN32(string appName, int width, int height);

        [DllImport("WheelchairGraphics")]
        private static extern void wgSetCamera(float zoom, float x, float y, float z, float rX, float rY, float rZ);

        [DllImport("WheelchairGraphics")]
        private static extern void wgAddObject(string objectId);

        [DllImport("WheelchairGraphics")]
        private static extern void wgRender(out IntPtr outBuffer);

        [DllImport("WheelchairGraphics")]
        private static extern void wgDeinitializeGraphics();

        private string _appName;
        private int _width;
        private int _height;

        public VulkanGraphicsPluginImpl(string appName, int width, int height)
        {
            _appName = appName;
            _width = width;
            _height = height;
        }

        public bool Initialize()
        {
            //here add wgSetCamera if you want different
            wgInitializeVulkanGraphicsWIN32(_appName, _width, _height);
            return true;
        }

        public void Render(out byte[] outBuffer)
        {
            wgRender(out IntPtr ptr);

            int bufferSize = _width * _height * 4;
            outBuffer = new byte[bufferSize];
            Marshal.Copy(ptr, outBuffer, 0, bufferSize);
        }

        public void Deinitialize()
        {
            wgDeinitializeGraphics();
        }

        public void AddResource(string resourceId)
        {
            wgAddObject(resourceId);
        }

        public void ClearResources()
        {
            //throw new NotImplementedException();
        }

        public void SetCamera(float zoom, CameraPosition position, CameraRotation rotation)
        {
            wgSetCamera(zoom, position.X, position.Y, position.Z, rotation.X, rotation.Y, rotation.Z);
        }
    }
}
