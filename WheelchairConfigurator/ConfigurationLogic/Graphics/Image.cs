using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationLogic.Graphics
{
    internal class Image
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public byte[] Pixels { get; set; }

        public Image(int width, int height)
        {
            Width = width;
            Height = height;
            Pixels = new byte[width * height * 4];
        }

        public void SetPixel(int x, int y, byte r, byte g, byte b, byte a)
        {
            int index = (y * Width + x) * 4;
            Pixels[index + 0] = r;
            Pixels[index + 1] = g;
            Pixels[index + 2] = b;
            Pixels[index + 3] = a;
        }

        public (byte r, byte g, byte b, byte a) GetPixel(int x, int y)
        {
            int index = (y * Width + x) * 4;
            return (Pixels[index], Pixels[index + 1], Pixels[index + 2], Pixels[index + 3]);
        }

        public void Overlay(Image top)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (top.Width <= x || top.Height <= y)
                        continue;

                    var (rB, gB, bB, aB) = GetPixel(x, y);
                    var (rT, gT, bT, aT) = top.GetPixel(x, y);

                    float alphaT = aT / 255f;
                    float alphaB = aB / 255f * (1 - alphaT);

                    byte r = (byte)(rT * alphaT + rB * alphaB);
                    byte g = (byte)(gT * alphaT + gB * alphaB);
                    byte b = (byte)(bT * alphaT + bB * alphaB);
                    byte a = (byte)((alphaT + alphaB) * 255);

                    SetPixel(x, y, r, g, b, a);
                }
            }
        }
    }
}
