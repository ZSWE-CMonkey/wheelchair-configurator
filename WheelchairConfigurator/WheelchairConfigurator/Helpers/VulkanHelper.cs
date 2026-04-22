using ConfigurationLogic.Graphics;
using ConfigurationLogic.Graphics.Types;
using SkiaSharp;
using System.Runtime.InteropServices;

namespace WheelchairConfigurator.Helpers
{
    /// <summary>
    /// Vulkan helper for single render of image lol
    /// </summary>
    internal class VulkanHelper
    {
        private struct Camera
        {
            public float Zoom;
            public CameraPosition Position;
            public CameraRotation Rotation;

            public Camera(float zoom, CameraPosition position, CameraRotation rotation)
            {
                Zoom = zoom;
                Position = position;
                Rotation = rotation;
            }
        }

        private int _width;
        private int _height;

        private List<string> _objectsId;

        private IGraphicsPlugin _graphicsPlugin;

        private Camera _camera;

        private object _mutex = new();

        public VulkanHelper(string name, int widht, int height)
        {
            _width = widht;
            _height = height;
            _objectsId = new List<string>();
            _graphicsPlugin = GraphicsPluginFactory.CreateVulkanGraphicsPlugin(name, widht, height);


            _camera = new Camera(
                -5.5f,
                new CameraPosition(0.1f, 1.1f, 0.0f),
                new CameraRotation(-0.5f, -112.75f, 0.0f)
                );
        }

        /// <summary>
        /// Adds object to the rendered scene. You must add it first before rendering call skibidi
        /// </summary>
        /// <param name="name">Object id name in format like (without any extensions): [subfolder]/[name]</param>
        public VulkanHelper AddObject(string name)
        {
            _objectsId.Add(name);
            return this;
        }

        public void ClearObjects()
        {
            _objectsId.Clear();
        }

        public void ChangeWidthHeight(int width, int height)
        {
            _width = width;
            _height = height;
        }

        public void AddRotationXY(float x, float y)
        {
            _camera.Rotation.X += x;
            _camera.Rotation.Y += y;
        }

        /// <summary>
        /// Initialize and renders once the scene and deinitialize vulkan engine. 
        /// Outputs final image of that rendering.
        /// You must add object first before this call :3
        /// </summary>
        /// <returns>ImageSource of pixel buffer</returns>        
        public ImageSource GetRenderedImageSource()
        {
            lock (_mutex)
            {
                foreach (string id in _objectsId)
                {
                    _graphicsPlugin.AddResource(id);
                }
                _graphicsPlugin.SetCamera(_camera.Zoom, _camera.Position, _camera.Rotation);
                _graphicsPlugin.Initialize();
                _graphicsPlugin.Render(out byte[] pixelBuffer);
                _graphicsPlugin.Deinitialize();

                ConvertMangetaToTransparent(ref pixelBuffer);

                GCHandle handle = GCHandle.Alloc(pixelBuffer, GCHandleType.Pinned);
                IntPtr pixels = handle.AddrOfPinnedObject();

                SKImageInfo info = new SKImageInfo(_width, _height, SKColorType.Rgba8888, SKAlphaType.Unpremul);

                using SKBitmap bitmap = new SKBitmap();
                bitmap.InstallPixels(info, pixels, info.RowBytes);

                using SKImage image = SKImage.FromBitmap(bitmap);
                using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);

                byte[] bytes = data.ToArray();

                ImageSource result = ImageSource.FromStream(() => new MemoryStream(bytes));
                handle.Free();
                return result;
            }
        }

        private void ConvertMangetaToTransparent(ref byte[] pixels)
        {
            for (int i = 0; i < pixels.Length; i += 4)
            {
                byte b = pixels[i];
                byte g = pixels[i + 1];
                byte r = pixels[i + 2];

                if (r == 255 && g == 0 && b == 255)
                {
                    pixels[i + 3] = 0;
                }
            }
        }

    }
}
