using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MangaParser.Graphics;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Xml.Linq;

namespace MangaParserTest
{
    [TestClass]
    public class SegmentationTests
    {
        DataLoader loader = new DataLoader();

        private Color checkPolygonColor(Bitmap expected, IPolygon polygon)
        {
            Color? result = null;

            foreach (var point in polygon.Points)
            {
                Color c = (expected.GetPixel(point.X, point.Y));

                if (result != null)
                {
                    Assert.AreEqual(result.Value, c, "Polygon spans multiple colors");
                }
                else
                {
                    result = c;
                }
            }

            if (!result.HasValue) Assert.Fail("No point in polygon!");

            return result.Value;
        }

        private void checkExtractedInExpected(IExtractionResult result, Bitmap expected, HashSet<Color> testColor)
        {
            HashSet<Color> usedColors = new HashSet<Color>();

            foreach (IPolygon polygon in result.Polygons)
            {
                Color polygonColor = checkPolygonColor(expected, polygon);
                Assert.IsFalse(usedColors.Contains(polygonColor), "Many polygons for the same cell");
                Assert.IsTrue(testColor.Contains(polygonColor), "Polygon outside any test color");
                usedColors.Add(polygonColor);
            }
        }

        private void FloodFill(Bitmap b, int x, int y, Color c)
        {
            if (b.GetPixel(x, y) != c) return;

            b.SetPixel(x, y, Color.White);
            //This assumes that the area to be filled is rather small (< 30 px radius)
            FloodFill(b, x + 1, y, c);
            FloodFill(b, x - 1, y, c);
            FloodFill(b, x, y + 1, c);
            FloodFill(b, x, y - 1, c);
        }

        private void FloodFill(Bitmap b, int x, int y, HashSet<Color> testColors)
        {
            Color c = b.GetPixel(x, y);
            if (!testColors.Contains(c)) return;
            FloodFill(b, x, y, c);
        }

        private unsafe bool CheckColors(HashSet<Color> testColors, BitmapData data)
        {
            byte* stride = (byte*)data.Scan0;

            for (int i = 0; i < data.Height; i++)
            {
                int* pixel = (int*)stride;

                for (int j = 0; j < data.Width; j++)
                {
                    if (testColors.Contains(Color.FromArgb(*pixel)))
                    {
                        return false;
                    }

                    pixel++;
                }

                stride += data.Stride;
            }

            return true;
        }

        private void CheckColors(HashSet<Color> testColors, Bitmap b)
        {
            bool result;
            b = b.To32bpp();
            BitmapData data = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            result = CheckColors(testColors, data);
            b.UnlockBits(data);

            Assert.IsTrue(result, "Required point missing");
        }

        private void ClearPolygon(Bitmap b, IPolygon polygon, HashSet<Color> testColors)
        {
            foreach (var point in polygon.Points)
            {
                FloodFill(b, point.X, point.Y, testColors);
            }
        }

        private void ClearPolygons(Bitmap b, IExtractionResult result, HashSet<Color> testColors)
        {
            foreach (var polygon in result.Polygons)
            {
                ClearPolygon(b, polygon, testColors);
            }
        }

        private void CheckAllExpected(Bitmap b, IExtractionResult result, HashSet<Color> testColors)
        {
            ClearPolygons(b, result, testColors);
            CheckColors(testColors, b);
        }

        [TestMethod]
        public void TestSegmenterInExpectedArea()
        {
            ICellsSegmentation segmenter = new AFThresholdBlobSegmentation();

            //Load the test data
            var testColors = new HashSet<Color>(loader.TestColors);

            foreach (var test in loader.Tests)
            {
                IExtractionResult result;
                using (Bitmap input = test.GetBitmap(DataLoader.TestDirectoryInput))
                using (Bitmap expected = test.GetBitmap(DataLoader.TestDirectoryAny)) 
                {
                    result = segmenter.Extract(input);
                    checkExtractedInExpected(result, expected, testColors);
                }
            }
        }

        [TestMethod]
        public void TestSegmenterRequiredPoints()
        {
            ICellsSegmentation segmenter = new AFThresholdBlobSegmentation();

            //Load the test data
            var testColors = new HashSet<Color>(loader.TestColors);

            foreach (var test in loader.Tests)
            {
                IExtractionResult result;
                using (Bitmap input = test.GetBitmap(DataLoader.TestDirectoryInput))
                using (Bitmap expected = test.GetBitmap(DataLoader.TestDirectoryAll)) {
                    result = segmenter.Extract(input);
                    CheckAllExpected(expected, result, testColors);
                }
            }
        }
    }
}
