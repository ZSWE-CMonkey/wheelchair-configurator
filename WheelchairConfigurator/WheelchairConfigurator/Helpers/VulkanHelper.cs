using ConfigurationLogic.Graphics;
using SkiaSharp;
using System.Runtime.InteropServices;

namespace WheelchairConfigurator.Helpers
{
    /// <summary>
    /// Vulkan helper for single render of image lol
    /// </summary>
    internal class VulkanHelper
    {
        private int _width;
        private int _height;

        private List<string> _objectsId;

        private IGraphicsPlugin _graphicsPlugin;

        public VulkanHelper(string name, int widht, int height)
        {
            _width = widht;
            _height = height;
            _objectsId = new List<string>();
            _graphicsPlugin = GraphicsPluginFactory.CreateVulkanGraphicsPlugin(name, widht, height);
        }

        /// <summary>
        /// Adds object to the rendered scene. You must add it first before rendering call skibidi
        /// </summary>
        /// <param name="name">Object id name in format like (without any extensions): [subfolder]/[name]</param>
        public void AddObject(string name)
        {
            _objectsId.Add(name);
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

        /// <summary>
        /// Initialize and renders once the scene and deinitialize vulkan engine. 
        /// Outputs final image of that rendering.
        /// You must add object first before this call :3
        /// </summary>
        /// <returns>ImageSource of pixel buffer</returns>        
        public ImageSource GetRenderedImageSource()
        {
            foreach(string id in _objectsId)
            {
                _graphicsPlugin.AddResource(id);
            }

            _graphicsPlugin.Initialize();
            _graphicsPlugin.Render(out byte[] pixelBuffer);
            _graphicsPlugin.Deinitialize();

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
}
