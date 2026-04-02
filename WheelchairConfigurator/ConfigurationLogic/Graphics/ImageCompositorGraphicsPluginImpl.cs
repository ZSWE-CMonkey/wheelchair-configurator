using Android.Hardware.Lights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Android.Icu.Text.ListFormatter;

namespace ConfigurationLogic.Graphics
{
    internal class ImageCompositorGraphicsPluginImpl : IGraphicsPlugin
    {
        private Image _imageBuffer;
        private List<Image> _images = new List<Image>();

        public ImageCompositorGraphicsPluginImpl(int width, int height)
        {
            _imageBuffer = CreateBlankImage(width, height);
        }

        public void AddResource(string resourceId)
        {
            using Stream stream = Task.Run(() => FileSystem.OpenAppPackageFileAsync(resourceId)).Result;
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            Image image = new Image(_imageBuffer.Width, _imageBuffer.Height);
            image.Pixels = memoryStream.ToArray();
            _images.Add(image);
        }

        public void ClearResources()
        {
            _images.Clear();
        }

        public void Deinitialize()
        {
            //:)
        }

        public bool Initialize()
        {
            //:3
            return true;
        }

        public void Render(out byte[] image)
        {
            _images.ForEach(image => { _imageBuffer.Overlay(image); });
            image = new byte[_imageBuffer.Pixels.Length];
            Array.Copy(_imageBuffer.Pixels, image, _imageBuffer.Pixels.Length);

            int width = _imageBuffer.Width;
            int height = _imageBuffer.Height;

            _imageBuffer = new Image(width, height);
        }

        private Image CreateBlankImage(int width, int height)
        {
            Image res = new Image(width, height);
            for (int y = 0; y < width; y++)
                for (int x = 0; x < height; x++)
                    res.SetPixel(x, y, 0, 0, 0, 0);

            return res;
        }
    }
}
