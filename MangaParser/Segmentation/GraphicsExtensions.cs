using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaParser.Graphics
{
    public static class GraphicsExtensions
    {
        public static Bitmap To32bpp(this Bitmap input) 
        {
            Bitmap output = input;
            if (input.PixelFormat != PixelFormat.Format32bppArgb)
            {
                output = new Bitmap(input.Width, input.Height, PixelFormat.Format32bppArgb);
                using (var g = System.Drawing.Graphics.FromImage(output))
                {
                    g.DrawImage(input, 0, 0);
                }
            }
            return output;
        }
    }
}
