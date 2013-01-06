using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge;
using AForge.Imaging;
using AForge.Math.Geometry;
using MangaParser.Graphics;
using MangaParser.Reader;

namespace VisualMangaParser
{
    public partial class Form1 : Form
    {
        private List<Func<Bitmap, Bitmap>> transforms = new List<Func<Bitmap,Bitmap>>();
        private static Size INDICATOR_SIZE = new Size(20, 20);
        private static Size INDICATOR_OFFSET = new Size(-INDICATOR_SIZE.Width/2, -INDICATOR_SIZE.Height/2);
        private AFThresholdBlobSegmentation.AFThresholdBlobSegmentationResult currentResult = null;

        enum Stages
        {
            Threshold,
            FilteredBoundingBoxes,
            UnfilteredBoundingBoxes,
            ConvexHull,
            MergedConvexHull,
            DependencyGraph,
            ReadingOrder,
            CellsView,
            StripView
        }

        struct DirectiondisplayItem
        {
            public String name;
            public Page.ReadingDirection direction;

            public override string ToString()
            {
                return name;
            }

            public DirectiondisplayItem(String name, Page.ReadingDirection direction)
            {
                this.name = name;
                this.direction = direction;
            }
        }


        public Form1()
        {
            InitializeComponent();

            radThreshold.Tag = Stages.Threshold;
            radUnfilteredBoundingBoxes.Tag = Stages.UnfilteredBoundingBoxes;
            radFilteredBoundingBoxes.Tag = Stages.FilteredBoundingBoxes;
            radConvexHull.Tag = Stages.ConvexHull;
            radMergedConvexHull.Tag = Stages.MergedConvexHull;
            radDependencyGraph.Tag = Stages.DependencyGraph;
            radReadingOrder.Tag = Stages.ReadingOrder;
            radCellViewOrder.Tag = Stages.CellsView;
            radStripView.Tag = Stages.StripView;

            cboReadingOrder.Items.Add(new DirectiondisplayItem("Right to Left", Page.ReadingDirection.DownLeft));
            cboReadingOrder.Items.Add(new DirectiondisplayItem("Left to Right", Page.ReadingDirection.DownRight));
            cboReadingOrder.Items.Add(new DirectiondisplayItem("Down, then left", Page.ReadingDirection.LeftDown));
            cboReadingOrder.Items.Add(new DirectiondisplayItem("Down, then right", Page.ReadingDirection.RightDown));

            cboReadingOrder.SelectedIndex = 0;
        }

        private Bitmap getOriginalPicture()
        {
            return new Bitmap(this.pictureBox1.Image);
        }

        private void updatePicture()
        {
            Bitmap b = getOriginalPicture();

            AFThresholdBlobSegmentation extractor = new AFThresholdBlobSegmentation(getThresholdValue());
            currentResult = (AFThresholdBlobSegmentation.AFThresholdBlobSegmentationResult) extractor.Extract(b);
            label1.Text =  "";
            if (currentResult.FullPage)
            {
                label1.Text = "Full page; ";
            }
            if (currentResult.InvertedBorders)
            {
                label1.Text += "Inverted";
            }

            foreach (var transform in transforms)
            {
                b = transform(b);
            }

            pictureBox2.Image = b;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            updatePicture();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            updatePicture();
        }

        private Bitmap MakeBitmap(int width, int height, Action<Graphics> painter)
        {
            Bitmap b = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(b))
            {
                painter(g);
            }

            return b;
        }

        private int getThresholdValue()
        {
            return trackBar1.Value;
        }

        private Bitmap threshold(Bitmap input)
        {
            Bitmap result = AFThresholdBlobSegmentation.Threshold(input, getThresholdValue());

            MakeBitmap(input.Width, input.Height, (Graphics g) =>
            {
                g.DrawImageUnscaled(result, 0, 0);
            });

            return result;
        }

        private Bitmap drawUnfiltered(Bitmap input)
        {
            return MakeBitmap(input.Width, input.Height, (Graphics g) =>
            {
                using (Pen p = new Pen(Color.Red, 3)) {
                    g.DrawImageUnscaled(input, 0, 0);
                    foreach (var blob in AFThresholdBlobSegmentation.GetBlobs(input))
                    {
                        g.DrawRectangle(p, blob.Rectangle);
                    }
                }
            });
        }

        private IEnumerable<Blob> getFilteredBlobs(Bitmap input)
        {
            return AFThresholdBlobSegmentation.FilterBlobs(input,
                            AFThresholdBlobSegmentation.GetBlobs(input));
        }

        private IEnumerable<Polygon> getFilteredPolygons(Bitmap input)
        {
            return AFThresholdBlobSegmentation.FilterContainedPolygons(
                            AFThresholdBlobSegmentation.GetPolygons(input).ToList());
        }


        private Bitmap drawFiltered(Bitmap input)
        {
            return MakeBitmap(input.Width, input.Height, (Graphics g) =>
            {
                using (Pen p = new Pen(Color.Red, 3)) {
                    g.DrawImageUnscaled(input, 0, 0);
                    foreach (var blob in getFilteredBlobs(input))
                    {
                        g.DrawRectangle(p, blob.Rectangle);
                    }
                }
            });
        }

        private Bitmap drawHulls(Bitmap input)
        {
            return MakeBitmap(input.Width, input.Height, (Graphics g) =>
            {
                using (Pen p = new Pen(Color.Red, 3)) {
                    g.DrawImageUnscaled(input, 0, 0);
                    foreach (var polygon in AFThresholdBlobSegmentation.GetPolygons(input))
                    {
                        g.DrawPolygon(p, polygon.Points.ToArray());
                    }
                }
            });
        }

        private Bitmap drawDependencyGraph(Bitmap input)
        {
            return MakeBitmap(input.Width, input.Height, (Graphics g) =>
            {
                var polygons = getFilteredPolygons(input);
                MangaParser.Reader.DefaultReadingOrder ro = new MangaParser.Reader.DefaultReadingOrder();

                var graph = DefaultReadingOrder.BuildRelationGraph(polygons.ToList());
                DefaultReadingOrder.SimplifyTransitiveClosure(graph);

                using (Pen p = new Pen(Color.Red, 3)) {
                    g.DrawImageUnscaled(input, 0, 0);

                    foreach (var poly in polygons)
                    {
                        g.DrawRectangle(p, poly.BoundingBox);
                        var origin = poly.CenterOfGravity;

                        foreach (var item in graph[poly])
                        {
                            Color c = item.Item1 == MangaParser.Reader.CellsRelation.BottomOf ? 
                                      Color.DarkGreen : Color.Blue;

                            var target = item.Item2.CenterOfGravity;
                            drawArrow(g, c, origin, target);
                        }
                    }
                }
            });
        }

        private Page.ReadingDirection getReadingDirection()
        {
            return (((DirectiondisplayItem)cboReadingOrder.SelectedItem).direction);
        }

        private IEnumerable<TransformedPolygon> getSortedPoly(Bitmap input, ICellsSegmentation segmentation)
        {
            Matrix tsf = Page.ReadingDirectionToMatrix(getReadingDirection());
            var polygons = currentResult.Polygons;
            var tpolys = (from poly in polygons select new TransformedPolygon(poly, tsf));

            MangaParser.Reader.ICellReadingOrder ro = new MangaParser.Reader.DefaultReadingOrder();

            return (from tpoly in ro.GetReadingOrder(tpolys) select (TransformedPolygon)tpoly);
        }

        private Page getPage(Bitmap input)
        {
            return new Page(input, getReadingDirection());
        }


        private IEnumerable<TransformedPolygon> getSortedPoly(Bitmap input)
        {
            AFThresholdBlobSegmentation extractor = new AFThresholdBlobSegmentation();
            return getSortedPoly(input, extractor);
        }

        private Bitmap drawReadingOrder(Bitmap input)
        {
            return MakeBitmap(input.Width, input.Height, (Graphics g) =>
            {
                using (Pen p = new Pen(Color.Red, 3)) 
                using (Brush qb = new SolidBrush(Color.DarkGreen))
                using (Pen q = new Pen(Color.DarkGreen, 5)) {
                    g.DrawImageUnscaled(input, 0, 0);
                    IPolygon lastPoly = null;

                    foreach (var tpoly in getSortedPoly(input))
                    {
                        var poly = tpoly.BasePolygon;
                        g.DrawRectangle(p, poly.BoundingBox);

                        if (lastPoly != null)
                        {
                            g.DrawLine(q, lastPoly.CenterOfGravity, poly.CenterOfGravity);
                        }

                        lastPoly = poly;
                    }
                }
            });
        }

        private Bitmap drawFilteredHulls(Bitmap input)
        {
            AFThresholdBlobSegmentation extractor = new AFThresholdBlobSegmentation();

            return MakeBitmap(input.Width, input.Height, (Graphics g) =>
            {
                IEnumerable<Polygon> polygons = getFilteredPolygons(input);

                using (Pen p = new Pen(Color.Red, 3)) {
                    g.DrawImageUnscaled(input, 0, 0);

                    foreach (var poly in polygons)
                    {
                        g.DrawPolygon(p, poly.Points.ToArray());
                    }
                }
            });
        }

        private System.Drawing.Point rectangleCenter(System.Drawing.Rectangle rectangle)
        {
            return rectangle.Location + new Size(rectangle.Width / 2, rectangle.Height / 2);
        }

        private System.Drawing.Point targetIndicatorLocation(System.Drawing.Point origin, System.Drawing.Point target)
        {
            return origin + new Size(3 * (target.X - origin.X) / 4, 3 * (target.Y - origin.Y) / 4);
        }

        private System.Drawing.Rectangle targetIndicatorRectangle(System.Drawing.Point origin, System.Drawing.Point target)
        {
            return new Rectangle(targetIndicatorLocation(origin, target) + INDICATOR_OFFSET, INDICATOR_SIZE);
        }

        private void drawArrow(Graphics g, Color color, System.Drawing.Point origin, System.Drawing.Point target)
        {
            using (LinearGradientBrush b = new LinearGradientBrush(origin, target, color, Color.FromArgb(64, color.R, color.G, color.B)))
            {
                using (Pen p = new Pen(b, INDICATOR_SIZE.Width / 4)) {
                    g.DrawLine(p, origin, target);
                }
            }

            using (Brush b = new SolidBrush(color))
            {
                g.FillEllipse(b, targetIndicatorRectangle(origin, target));
            }
        }

        private Bitmap drawPageView(Bitmap input, IPageViewer viewer)
        {
            return MakeBitmap(input.Width, input.Height, (Graphics g) =>
            {
                Page page = getPage(input);

                using (Pen p = new Pen(Color.Red, 3)) {
                    g.DrawImageUnscaled(input, 0, 0);
                    System.Drawing.Point? lastPoint = null;

                    foreach (var rectangle in page.ComputeView(viewer))
                    {
                        g.DrawRectangle(p, rectangle);

                        var newPoint = rectangleCenter(rectangle);
                        if (lastPoint != null)
                        {
                            drawArrow(g, Color.DarkGreen, lastPoint.Value, newPoint);
                        }
                        lastPoint = newPoint;
                    }
                }
            });
        }

        private void stagesChanged(object sender, EventArgs e)
        {
            RadioButton target = sender as RadioButton;

            if (target == null)
            {
                foreach (var control in panel1.Controls)
                {
                    if (control is RadioButton)
                    {
                        target = control as RadioButton;
                        if (target.Checked) break;
                    }
                }
            }

            if (!target.Checked) return;
            Stages stage = (Stages)target.Tag;

            transforms.Clear();

            if (stage >= Stages.Threshold) {
                transforms.Add(threshold);
            }

            if (stage == Stages.UnfilteredBoundingBoxes) {
                transforms.Add(drawUnfiltered);
            }
            if (stage == Stages.FilteredBoundingBoxes) {
                transforms.Add(drawFiltered);
            }
            if (stage == Stages.ConvexHull) {
                transforms.Add(drawHulls);
            }
            if (stage == Stages.MergedConvexHull) {
                transforms.Add(drawFilteredHulls);
            }
            if (stage == Stages.DependencyGraph) {
                transforms.Add(drawDependencyGraph);
            }
            if (stage == Stages.ReadingOrder) {
                transforms.Add(drawReadingOrder);
            }
            if (stage == Stages.CellsView) {
                transforms.Add((Bitmap x) => drawPageView(x, new CellsPageViewer(x.Width, x.Height)));
            }
            else if (stage == Stages.StripView) {
                transforms.Add((Bitmap x) => drawPageView(x, new StripPageViewer(x.Width, x.Height)));
            }

            updatePicture();
        }

        private void splitContainer1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {
                try
                {
                    Bitmap b = new Bitmap(file);
                    pictureBox1.Image = b;
                    updatePicture();
                    return;
                }
                catch (Exception) { }
            }
        }

        private void splitContainer1_DragEnter(object sender, DragEventArgs e)
        {
            if ( e.Data.GetDataPresent(DataFormats.FileDrop)) 
                e.Effect = DragDropEffects.Copy;
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
