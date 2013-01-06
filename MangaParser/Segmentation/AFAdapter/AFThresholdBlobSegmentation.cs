using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;

namespace MangaParser.Graphics
{
    public class AFThresholdBlobSegmentation : ICellsSegmentation
    {
        public class AFThresholdBlobSegmentationResult : IExtractionResult
        {
            private Bitmap page;
            private int thresholdValue;
            private bool? isFullPage = null;
            private IEnumerable<IPolygon> polygons = null;

            public AFThresholdBlobSegmentationResult(Bitmap page, int thresholdValue)
            {
                this.page = page;
                this.thresholdValue = thresholdValue;
            }

            private void extractPolygons()
            {
                if (polygons == null)
                    polygons = Extract(page, thresholdValue);
            }

            public bool FullPage
            {
                get
                {
                    if (isFullPage == null)
                    {
                        extractPolygons();
                        isFullPage = AFThresholdBlobSegmentation.IsFullPage(page, polygons);
                    }
                    return isFullPage.Value;
                }
            }

            public bool InvertedBorders
            {
                get
                {
                    return AFThresholdBlobSegmentation.IsInvertedColor(
                        page, thresholdValue);
                }
            }

            public Bitmap Page
            {
                get { return page; }
            }

            public IEnumerable<IPolygon> Polygons
            {
                get { extractPolygons(); return polygons; }
            }
        }

        public int ThresholdValue { get; set; }

        public AFThresholdBlobSegmentation()
        {
            ThresholdValue = 128;
        }

        public AFThresholdBlobSegmentation(int threshold)
        {
            ThresholdValue = Math.Max(0, Math.Min(255, threshold));
        }


        public IExtractionResult Extract(Bitmap page)
        {
            return new AFThresholdBlobSegmentationResult(page, ThresholdValue);
        }

        public static IEnumerable<Polygon> Extract(System.Drawing.Bitmap page, int threshold)
        {
            Bitmap image = AFThresholdBlobSegmentation.Threshold(page, threshold);
            return FilterContainedPolygons(GetPolygons(image).ToList());
        }


        #region Blob Extraction
        public static IEnumerable<Blob> GetBlobs(Bitmap input, BlobCounterBase counter)
        {
            AForge.Imaging.Filters.Invert inv = new AForge.Imaging.Filters.Invert();

            if (!IsInvertedColor(input))
            {
                inv.ApplyInPlace(input);
            }

            counter.ProcessImage(input);
            return counter.GetObjects(input, false);
        }

        public static IEnumerable<Blob> GetBlobs(Bitmap input)
        {
            var counter = new AForge.Imaging.BlobCounter();
            return GetBlobs(input, counter);
        }

        public static IEnumerable<Blob> FilterBlobs(Bitmap input, IEnumerable<Blob> blobs)
        {
            return from Blob blob in blobs
                   where blob.Rectangle.Width > input.Width * 0.05 && blob.Rectangle.Height > input.Height * 0.05
                   where blob.Rectangle.Width < input.Width * 0.95 || blob.Rectangle.Height < input.Height * 0.95
                   select blob;
        }
        #endregion
        #region Polygon Extraction
        public static IEnumerable<Polygon> GetPolygons(IEnumerable<Blob> blobs, BlobCounter blobCounter)
        {
            return from blob in blobs select blob.ToPolygon(blobCounter);
        }

        public static IEnumerable<Polygon> GetPolygons(Bitmap input)
        {
            BlobCounter b = new BlobCounter();
            var blobs = FilterBlobs(input, GetBlobs(input, b));
            return GetPolygons(blobs, b);
        }

        public static IEnumerable<Polygon> FilterContainedPolygons(IReadOnlyList<Polygon> polygons)
        {
            List<bool> contained = new List<bool>(polygons.Count);

            for (int i = 0; i < polygons.Count; i++) { contained.Add(false); }

            for (int i = 0; i < polygons.Count; i++)
            {
                if (!contained[i])
                {
                    for (int j = 0; j < polygons.Count; j++)
                    {
                        if (i != j && !contained[j])
                        {
                            contained[j] = polygons[i].Contains(polygons[j]);
                        }
                    }
                }
            }

            return from row in polygons.Zip(contained, (p, c) => new { Polygon = p, Contained = c })
                   where !row.Contained
                   select row.Polygon;
        }
        #endregion
        #region Full Page Check
        private static unsafe long aggregateImageIntensity(BitmapData data)
        {
            //Note: this code is specific to 8bpp images
            byte* p = (byte*)data.Scan0.ToPointer();
            int gap = data.Stride - data.Width;
            long total = 0;

            // for each pixel in the image
            for (int y = 0; y < data.Height; y++)
            {
                for (int x = 0; x < data.Width; x++, p++)
                {
                    total += *p;
                }

                p += gap;
            }

            return total;
        }

        private static long aggregateImageIntensity(Bitmap page)
        {
            if (page.PixelFormat != PixelFormat.Format8bppIndexed)
                throw new ArgumentException("Can only process 8bpp images");

            BitmapData data = page.LockBits(
                new Rectangle(0, 0, page.Width, page.Height),
                ImageLockMode.ReadOnly, page.PixelFormat);
            long result = aggregateImageIntensity(data);
            page.UnlockBits(data);

            return result;
        }

        private static bool isBlack(long value, int area, double threshold = 0.7)
        {
            return value <= area * 255 * (1 - threshold);
        }

        private static bool isWhite(long value, int area, double threshold = 0.95)
        {
            return value > area * 255 * threshold;
        }

        private static long cellsCover(IEnumerable<IPolygon> cells)
        {
            long total = 0;

            foreach (var poly in cells)
            {
                total += poly.Area;
            }

            return total;
        }

        public static bool IsFullPage(Bitmap page, IEnumerable<IPolygon> polygons)
        {
            Grayscale gs = new Grayscale(0.30, 0.59, 0.11);
            Bitmap result = gs.Apply(page.To32bpp());
            long color = aggregateImageIntensity(result);

            return isWhite(color, page.Width * page.Height, 0.95) ||
                   isBlack(color, page.Width * page.Height, 0.6) ||
                   cellsCover(polygons) < 0.4 * page.Width * page.Height;
        }
        #endregion

        public static Bitmap Threshold(Bitmap page, int threshold)
        {
            page = page.To32bpp();
            Grayscale gs = new Grayscale(0.30, 0.59, 0.11);
            Threshold ts = new Threshold(threshold);
            Bitmap result = gs.Apply(page);
            return ts.Apply(result);
        }

        /// <summary>
        /// Determines if the cells are white over a black background
        /// </summary>
        /// <param name="page">The bitmap that is the manga page</param>
        /// <param name="threshold">The threshold at which the colors of the picture are considered white, after converting to grayscale</param>
        /// <returns>Returns true if the background of the manga page is black</returns>
        public static bool IsInvertedColor(Bitmap page, int threshold)
        {
            page = Threshold(page, threshold);
            return IsInvertedColor(page);
        }


        /// <summary>
        /// Determines if the cells are white over a black background, for a black and white image
        /// </summary>
        /// <param name="page">The bitmap that is the manga page</param>
        /// <returns>Returns true if the background of the manga page is black</returns>
        public static bool IsInvertedColor(Bitmap page)
        {
            // The colors are inverted if the color of the borders are inverted.
            HorizontalIntensityStatistics his = new HorizontalIntensityStatistics(page);
            VerticalIntensityStatistics vis = new VerticalIntensityStatistics(page);

            int vband = (int)(page.Height * 0.05);
            int hband = (int)(page.Width * 0.05);

            decimal totalValue = 0;

            for (int i = 0; i < vband; i++)
            {
                totalValue += vis.Gray.Values[i] + vis.Gray.Values[page.Height - i - 1];
            }

            for (int i = 0; i < hband; i++)
            {
                totalValue += his.Gray.Values[i] + his.Gray.Values[page.Width - i - 1];
            }

            return totalValue < (decimal)(((page.Height * hband * 2) + (page.Width * vband * 2)) * 255 * 0.50);
        }
    }
}
