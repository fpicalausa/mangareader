using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using MangaParser.Graphics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MangaParserTest
{
    [TestClass]
    public class GeometryTests
    {
        [TestMethod]
        public void PolygonCreationTest()
        {
            Rectangle r = new Rectangle(50,150,200,250);
            List<Point> points = new List<Point>();
            points.Add(new Point(50, 180));
            points.Add(new Point(125, 150));
            points.Add(new Point(250, 180));
            points.Add(new Point(125, 150+200));

            Point cog = new Point(100, 200);

            IPolygon polygon = new Polygon(r, points, cog);

            Assert.AreEqual(polygon.BoundingBox, r);
            
            CollectionAssert.AreEqual(points, new List<Point>(polygon.Points));
            Assert.AreEqual(cog, polygon.CenterOfGravity);
        }


        private IPolygon createPolygon(List<Point> points)
        {
            Point topleft = new Point(
                (from point in points select point.X).Min(),
                (from point in points select point.Y).Min());
            Size boundsSize = new Size(
                (from point in points select point.X).Max() - topleft.X + 1,
                (from point in points select point.Y).Max() - topleft.Y + 1);
            Rectangle bounds = new Rectangle(topleft, boundsSize);

            Point cog = new Point(
                (int)(from point in points select point.X).Average(),
                (int)(from point in points select point.Y).Average());

            return new Polygon(bounds, points, cog);
        }

        [TestMethod]
        public void PointContainedTest()
        {
            IPolygon polygon = createPolygon(new List<Point>() {
                new Point(0,0),
                new Point(10,0),
                new Point(10,20),
                new Point(0,20)
            });

            Assert.IsTrue(polygon.Contains(new Point(0, 0)));
            Assert.IsTrue(polygon.Contains(new Point(3, 2)));
            Assert.IsTrue(polygon.Contains(new Point(5, 5)));
            Assert.IsTrue(polygon.Contains(new Point(9, 5)));
            Assert.IsTrue(polygon.Contains(new Point(9, 19)));

            Assert.IsFalse(polygon.Contains(new Point(-10, 20)));
            Assert.IsFalse(polygon.Contains(new Point(10, 5)));
            Assert.IsFalse(polygon.Contains(new Point(121, 43)));
        }

        [TestMethod]
        public void PointContainedInTriangle()
        {
            IPolygon polygon = createPolygon(new List<Point>() {
                new Point(0,0),
                new Point(10,0),
                new Point(0,20)
            });

            Assert.IsTrue(polygon.Contains(new Point(0, 0)));
            Assert.IsTrue(polygon.Contains(new Point(2, 1)));
            Assert.IsTrue(polygon.Contains(new Point(5, 5)));
            Assert.IsTrue(polygon.Contains(new Point(1, 15)));

            Assert.IsFalse(polygon.Contains(new Point(5, 16)));
            Assert.IsFalse(polygon.Contains(new Point(-10, 20)));
            Assert.IsFalse(polygon.Contains(new Point(10, 5)));
            Assert.IsFalse(polygon.Contains(new Point(121, 43)));
        }

        [TestMethod]
        public void RectangleArea()
        {
            IPolygon polygon = createPolygon(new List<Point>() {
                new Point(0,0),
                new Point(10,0),
                new Point(10,20),
                new Point(0,20)
            });

            Assert.AreEqual(200, polygon.Area);
        }

        [TestMethod]
        public void TriangleArea()
        {
            IPolygon polygon = createPolygon(new List<Point>() {
                new Point(10,0),
                new Point(10,20),
                new Point(0,20)
            });

            Assert.AreEqual(100, polygon.Area);
        }

        [TestMethod]
        public void RectangleInRectangle()
        {
            IPolygon polygonOuter = createPolygon(new List<Point>() {
                new Point(0,0),
                new Point(10,0),
                new Point(10,20),
                new Point(0,20)
            });
            IPolygon polygonInner = createPolygon(new List<Point>() {
                new Point(2,2),
                new Point(8,2),
                new Point(8,18),
                new Point(2,18)
            });

            Assert.IsTrue(polygonOuter.Contains(polygonInner));
            Assert.IsFalse(polygonInner.Contains(polygonOuter));
        }

        [TestMethod]
        public void RectangleIntersectRectangle()
        {
            IPolygon polygon1 = createPolygon(new List<Point>() {
                new Point(0,10),
                new Point(10,10),
                new Point(10,20),
                new Point(0,20)
            });
            IPolygon polygon2 = createPolygon(new List<Point>() {
                new Point(2,0),
                new Point(8,0),
                new Point(8,30),
                new Point(2,30)
            });

            Assert.IsFalse(polygon1.Contains(polygon2));
            Assert.IsFalse(polygon2.Contains(polygon1));
        }

    }


}
