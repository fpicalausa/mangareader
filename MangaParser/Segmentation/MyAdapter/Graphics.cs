using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaParser.Graphics.Raster
{
    public static class Graphics
    {

        public static Bitmap Threshold(Image bitmap, int thresholdValue)
        {
            ColorMatrix grayscale =  new ColorMatrix(new float[][]{   
                new float[]{ 0.30f, 0.30f,  0.30f,  0,  0},
                new float[]{ 0.59f, 0.59f,  0.59f,  0,  0},
                new float[]{ 0.11f, 0.11f,  0.11f,  0,  0},
                new float[]{ 0,     0,      0,      1,  0},
                new float[]{ 0,     0,      0,      0,  1}});

            ImageAttributes attrib = new ImageAttributes();

            attrib.SetColorMatrix(grayscale);
            attrib.SetThreshold(thresholdValue / 255.0f);

            Rectangle dimensions = new Rectangle(0,0, bitmap.Width, bitmap.Height);
            Bitmap output = new Bitmap(dimensions.Width, dimensions.Height, PixelFormat.Format32bppArgb);

            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(output))
            {
                g.DrawImage(bitmap, dimensions, 0, 0, dimensions.Width, dimensions.Height,
                    GraphicsUnit.Pixel, attrib);
            }

            return output;
        }

        private unsafe static bool[] ParseStride(uint* stride, int width)
        {
            bool[] line = new bool[width];

            for (int i = 0; i < width; i++)
            {
                uint pixel = *stride;
                stride++;

                line[i] = (pixel == 0xff000000);
            }

            return line;
        }

        private static IEnumerable<Blob.Segment> packRow(bool[] row)
        {
            int i = 0;
            int start = 0;
            bool cont = false;

            while (i != row.Length)
            {
                if (row[i] && !cont)        { start = i; }
                else if (!row[i] && cont)   { yield return new Blob.Segment(start, i); }

                cont = row[i];
                i++;
            }

            if (cont) { yield return new Blob.Segment(start, i); }
        }

        private static IEnumerable<Blob> createBlobs(IEnumerable<Blob.Segment> segments, int y) {
            return from segment in segments
                   select new Blob(y, segment);
        }

        private static List<Blob> mergeBlobs(List<Blob> lastBlobs,
            List<Blob.Segment> lastRow,
            List<Blob.Segment> currentRow,
            HashSet<Blob> blobs,
            int y)
        {
            int i = 0, j = 0;
            List<Blob> result = new List<Blob>(currentRow.Count);
            Blob resultBlob = null;

            // We try to check which segments of currentRow intersect with 
            // segments of lastRow. If no intersection occurs for a given range, 
            // we create a new blob and register it in the result. Otherwise,
            // we merge the current segment into the blob corresponding to the
            // segment of the last row with which we intersect. If there are 
            // many such segments, we merge every blob into a larger one, and 
            // remove the smaller ones from the blobs set.
            while (i < currentRow.Count && j < lastRow.Count)
            {
                if (currentRow[i].from > lastRow[j].to) {
                    // Try to sync up lastRow with currentRow
                    j++;
                }
                else if (currentRow[i].to < lastRow[j].from) {
                    // lastRow next segment will only occur later. No interesection for this currentRow segment
                    if (resultBlob == null) {
                        // Keep track of this new blob
                        resultBlob = new Blob(y, currentRow[i]);
                        result.Add(resultBlob);
                    }

                    i++;
                    resultBlob = null;
                }
                else {
                    //We have an intersection. Identify the blob that we have in lastRow
                    Blob oldBlob = lastBlobs[j];

                    //If we are already in a known blob, merge the oldBlob with the resultBlob
                    if (resultBlob != null) {
                        if (oldBlob == resultBlob) {
                            // Do nothing. Already merged.
                        }
                        else {
                            resultBlob.MergeWith(oldBlob);

                            //Rename oldblob everywhere else
                            for (int k = j; k < lastBlobs.Count; k++)
                            {
                                if (lastBlobs[k] == oldBlob)
                                {
                                    lastBlobs[k] = resultBlob;
                                }
                            }

                            blobs.Remove(oldBlob);
                        }
                    }
                    else {
                        //Our segment is part of oldBlob.
                        resultBlob = oldBlob;
                        resultBlob.AddRegion(y, currentRow[i]);
                        result.Add(resultBlob);
                    }

                    if (lastRow[j].to < currentRow[i].to) {
                        j++;
                    }
                    else {
                        i++;
                        resultBlob = null;
                    }
                }
            }

            if (resultBlob != null)
            {
                i++;
                resultBlob = null;
            }

            //Add the remaining blobs
            while (i < currentRow.Count)
            {
                // Keep track of this new blob
                resultBlob = new Blob(y, currentRow[i]);
                result.Add(resultBlob);
                i++;
            }

            // Register the new blobs
            blobs.UnionWith(result);
            return result;
        }

        private unsafe static HashSet<Blob> identifyBlobs(BitmapData data)
        {
            HashSet<Blob> blobs = new HashSet<Blob>();
            List<Blob> lastBlobs = null;
            List<Blob.Segment> lastRow = null;
            List<Blob.Segment> currentRow = null;
            int total = 0;

            byte* row = (byte*)data.Scan0;

            for (int i = 0; i < data.Height; i++)
            {
                currentRow = packRow(ParseStride((uint*)row, data.Width)).ToList();

                total += currentRow.Count;

                if (lastRow != null && lastRow.Count != 0)
                {
                    lastBlobs = mergeBlobs(lastBlobs, lastRow, currentRow, blobs, i);
                }
                else
                {
                    lastBlobs = createBlobs(currentRow, i).AsParallel().ToList();
                    blobs.UnionWith(lastBlobs);
                }
                lastRow = currentRow;
                row += data.Stride;
            }

            return blobs;
        }

        public static HashSet<Blob> IdentifyBlobs(Bitmap bitmap)
        {
            BitmapData data = bitmap.LockBits(
                new Rectangle(0,0,bitmap.Width,bitmap.Height), 
                ImageLockMode.ReadWrite, 
                PixelFormat.Format32bppArgb);
            HashSet<Blob> blobs = identifyBlobs(data);
            bitmap.UnlockBits(data);
            return blobs;
        }
    }
}
