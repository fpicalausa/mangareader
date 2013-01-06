using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Drawing.Imaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MangaParserTest
{
    [TestClass]
    public class CustomSegmenterTests
    {
        private Bitmap MakeBitmap(int width, int height, Action<Graphics> painter)
        {
            Bitmap b = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(b))
            {
                painter(g);
            }

            return b;
        }

        [TestMethod]
        public void TestThreshold()
        {
            //Create a bitmap consisting of a grayscale gradient
            int width = 10;
            int height = 100;
            int threshold = 64;

            Bitmap b = MakeBitmap(width, height, (Graphics g) =>
                {
                    for (int i = 0; i < height; i++)
                    {
                        int value = 255 * i / height;
                        g.DrawLine(new Pen(Color.FromArgb(
                            value, value, value)), 0, i, width, i);
                    }
                });

            Bitmap result = MangaParser.Graphics.Raster.Graphics.Threshold(b, threshold);

            //The resulting image should be of the same size
            Assert.AreEqual(b.Height, result.Height);
            Assert.AreEqual(b.Width, result.Width);

            //The threshold operation should split the image in a black rectangle
            // and a white one.
            for (int x = 0; x < result.Width; x++)
            {
                for (int y = 0; y < result.Height; y++)
                {
                    Color c = result.GetPixel(x,y);
                    if (b.GetPixel(x,y).R < threshold) {
                        Assert.AreEqual(Color.FromArgb(0,0,0), c, "Unexpected color for " + x + ", " + y);
                    }
                    else {
                        Assert.AreEqual(Color.FromArgb(255,255,255), c, "Unexpected color for " + x + ", " + y);
                    }
                }
            }
        }

        [TestMethod]
        public void TestRecognizeEmpty()
        {
            HashSet<Rectangle> expected = new HashSet<Rectangle>();
            expected.Add(new Rectangle(20,20,61,61));

            Bitmap b = MakeBitmap(100, 100, (Graphics g) =>
                {
                    g.FillRectangle(Brushes.White, 0, 0, 100, 100);
                });

            HashSet<Rectangle> blobs = new HashSet<Rectangle>(from blob in MangaParser.Graphics.Raster.Graphics.IdentifyBlobs(b)
                                                              select blob.BoundingBox);
            Assert.AreEqual(0, blobs.Count);
        }


        [TestMethod]
        public void TestRecognizeSquareBlob()
        {
            HashSet<Rectangle> expected = new HashSet<Rectangle>();
            expected.Add(new Rectangle(20,20,61,61));

            Bitmap b = MakeBitmap(100, 100, (Graphics g) =>
                {
                    g.DrawRectangle(Pens.Black, new Rectangle(20, 20, 60, 60));
                });

            HashSet<Rectangle> blobs = new HashSet<Rectangle>(from blob in MangaParser.Graphics.Raster.Graphics.IdentifyBlobs(b)
                                                              select blob.BoundingBox);
            Assert.AreEqual(1, blobs.Intersect(expected).Count());
        }

        [TestMethod]
        public void TestRecognizeSpirals()
        {
            Bitmap b = MakeBitmap(100, 100, (Graphics g) =>
                {
                    int top = 20;
                    int bottom = 75;

                    for (int i = 20; i <= 60; i += 10 )
                    {
                        g.DrawLine(Pens.Black, i, top, i, bottom);

                        if (i != 80) {
                            if ((i / 10) % 2 == 0) {
                                top += 10;
                                g.DrawLine(Pens.Black, i, bottom, i + 10, bottom);
                            }
                            else {
                                bottom -= 10;
                                g.DrawLine(Pens.Black, i, top, i + 10, top);
                            }
                        }
                    }

                    g.FillPolygon(Brushes.Black, new Point[] {
                        new Point(30, 20),
                        new Point(80, 20),
                        new Point(80, 45) });
                    g.DrawPolygon(Pens.Black, new Point[] {  //Make sure that we draw the borders too
                        new Point(30, 20),
                        new Point(80, 20),
                        new Point(80, 45) });

                    g.FillPolygon(Brushes.Black, new Point[] {
                        new Point(30, 80),
                        new Point(80, 80),
                        new Point(80, 55) });
                    g.DrawPolygon(Pens.Black, new Point[] {
                        new Point(30, 80),
                        new Point(80, 80),
                        new Point(80, 55) });
                });

            HashSet<Rectangle> expected = new HashSet<Rectangle>();
            HashSet<Rectangle> blobs = new HashSet<Rectangle>(from blob in MangaParser.Graphics.Raster.Graphics.IdentifyBlobs(b)
                                                              select blob.BoundingBox);

            expected.Add(new Rectangle(20, 20, 51, 56));
            expected.Add(new Rectangle(30, 20, 51, 26));
            expected.Add(new Rectangle(30, 55, 51, 26));

            Assert.AreEqual(expected.Count, blobs.Intersect(expected).Count());
        }
    }
}
