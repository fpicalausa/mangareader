using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MangaParser.Graphics;
using MangaParser.Reader;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MangaParserTest
{
    [TestClass]
    public class CellsOrderingTest
    {
        DataLoader loader = new DataLoader();

        [TestMethod]
        public void TestCellsOrdering()
        {
            ICellsSegmentation segmenter = new AFThresholdBlobSegmentation();

            //Load the test data
            var testColors = new List<Color>(loader.TestColors);

            foreach (var test in loader.Tests)
            {
                IExtractionResult result;
                using (Bitmap input = test.GetBitmap(DataLoader.TestDirectoryInput))
                using (Bitmap polyColors = test.GetBitmap(DataLoader.TestDirectoryAny)) {
                    result = segmenter.Extract(input);

                    //Left to right, top to bottom reading order. Other orders can 
                    // be obtained through polygon transformations, outside of the 
                    // scope of this test.
                    ICellReadingOrder readingOrder = new DefaultReadingOrder();

                    List<Color> outputSequence = new List<Color>(
                        from polygon in readingOrder.GetReadingOrder(result.Polygons)
                        let point = polygon.Points.First()
                        select polyColors.GetPixel(point.X, point.Y));

                    if (outputSequence.Count > testColors.Count)
                        throw new FormatException("Wrong test");

                    for (int i = 0; i < outputSequence.Count; i++)
                    {
                        Assert.AreEqual(testColors[i], outputSequence[i]);
                    }
                }
            }
        }
    }
}
