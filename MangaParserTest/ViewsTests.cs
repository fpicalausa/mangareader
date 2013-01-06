using System;
using System.Collections.Generic;
using System.Drawing;
using MangaParser.Reader;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MangaParserTest
{
    [TestClass]
    public class ViewsTests
    {
        [TestMethod]
        public void CellViewSanityTest1()
        {
            RectanglePoly rect = new RectanglePoly(100, 100, 200, 200);
            List<RectanglePoly> r = new List<RectanglePoly>() { rect };
            CellsPageViewer cv = new CellsPageViewer(200, 200);

            List<Rectangle> result = new List<Rectangle>(cv.ComputeView(r));

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(rect.BoundingBox, result[0]);
        }

        [TestMethod]
        public void CellViewSanityTest2()
        {
            List<RectanglePoly> r = new List<RectanglePoly>() { };
            CellsPageViewer cv = new CellsPageViewer(200, 200);
            List<Rectangle> result = new List<Rectangle>(cv.ComputeView(r));
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void MergeCells()
        {
            RectanglePoly rect1 = new RectanglePoly(100, 100, 200, 200);
            RectanglePoly rect2= new RectanglePoly(150, 100, 250, 200);
            List<RectanglePoly> r = new List<RectanglePoly>() { rect1, rect2 };
            CellsPageViewer cv = new CellsPageViewer(250, 250);

            List<Rectangle> result = new List<Rectangle>(cv.ComputeView(r));
            List<Rectangle> expected = new List<Rectangle>() { Rectangle.Union(rect1, rect2) };

            CollectionAssert.AreEqual(result, expected);
        }

        [TestMethod]
        public void MergeCellsWithLimit()
        {
            RectanglePoly rect1 = new RectanglePoly(100, 100, 200, 200);
            RectanglePoly rect2= new RectanglePoly(150, 100, 250, 200);
            List<RectanglePoly> r = new List<RectanglePoly>() { rect1, rect2 };
            CellsPageViewer cv = new CellsPageViewer(100, 100);

            List<Rectangle> result = new List<Rectangle>(cv.ComputeView(r));
            List<Rectangle> expected = new List<Rectangle>() { rect1, rect2 };

            CollectionAssert.AreEqual(result, expected);
        }

        [TestMethod]
        public void NoOverlapNoMerge()
        {
            RectanglePoly rect1 = new RectanglePoly(100, 100, 200, 200);
            RectanglePoly rect2= new RectanglePoly(200, 100, 250, 200);
            List<RectanglePoly> r = new List<RectanglePoly>() { rect1, rect2 };
            CellsPageViewer cv = new CellsPageViewer(250, 250);

            List<Rectangle> result = new List<Rectangle>(cv.ComputeView(r));
            List<Rectangle> expected = new List<Rectangle>() { rect1, rect2 };

            CollectionAssert.AreEqual(result, expected);
        }

        [TestMethod]
        public void StripMerge()
        {
            RectanglePoly rect1 = new RectanglePoly(100, 100, 200, 200);
            RectanglePoly rect2= new RectanglePoly(230, 100, 300, 180);
            RectanglePoly rect3= new RectanglePoly(310, 120, 400, 220);
            RectanglePoly rect4= new RectanglePoly(100, 220, 300, 340);
            RectanglePoly rect5= new RectanglePoly(330, 220, 400, 330);

            List<RectanglePoly> r = new List<RectanglePoly>() { rect1, rect2, rect3, rect4, rect5 };
            StripPageViewer cv = new StripPageViewer(2000, 2000);

            List<Rectangle> result = new List<Rectangle>(cv.ComputeView(r));
            List<Rectangle> expected = new List<Rectangle>() { 
                Rectangle.Union(rect1, Rectangle.Union(rect2, rect3)),
                Rectangle.Union(rect4, rect5) };

            CollectionAssert.AreEqual(result, expected);

        }
    }
}
