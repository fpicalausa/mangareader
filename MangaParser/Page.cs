using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MangaParser.Graphics;

namespace MangaParser.Reader
{
    public class Page
    {
        /// <summary>
        /// Indicates the proper reading direction for the manga page
        /// </summary>
        public enum ReadingDirection
        {
            /// <summary>
            /// Read left to right, then go down to the next line
            /// </summary>
            DownRight,

            /// <summary>
            /// Read right to left, then go down to the next line
            /// </summary>
            DownLeft,

            /// <summary>
            /// Read down, then move right to the next column
            /// </summary>
            RightDown,

            /// <summary>
            /// Read down, then move left to the next column
            /// </summary>
            LeftDown,
        }

        /// <summary>
        /// The graphic representation of the page
        /// </summary>
        public Bitmap Content { get; private set; }

        /// <summary>
        /// The cells that compose the page
        /// </summary>
        public IEnumerable<IPolygon> Cells { get; private set; }

        /// <summary>
        /// The cells that compose the page, normalized with respect to the
        /// ReadingTransformationMatrix.
        /// </summary>
        public IEnumerable<IPolygon> TransformedCells { get; private set; }

        /// <summary>
        /// How the page is supposed to be read
        /// </summary>
        public ReadingDirection Direction { get; private set; }

        public Page(System.Drawing.Bitmap page, ReadingDirection direction): this(page, direction, new AFThresholdBlobSegmentation(), new DefaultReadingOrder())
        {
        }

        public Page(System.Drawing.Bitmap page, ReadingDirection direction, ICellsSegmentation extractor, ICellReadingOrder order)
        {
            this.Content = page;
            this.Direction = direction;

            var ExtractedCells = extractor.Extract(page);

            if (ExtractedCells.FullPage)
            {
                this.Cells = new List<IPolygon>(new Polygon[] { Polygon.Rectangle(0, 0, page.Width, page.Height) });
                this.TransformedCells = (from cell in this.Cells select (new TransformedPolygon(cell, ReadingDirectionToMatrix(this.Direction))));
            }
            else
            {
                var transformedCells = (from cell in ExtractedCells.Polygons
                                        select new TransformedPolygon(cell, ReadingDirectionToMatrix(direction)));

                this.TransformedCells = order.GetReadingOrder(transformedCells);
                this.Cells = (from cell in this.TransformedCells select ((TransformedPolygon)cell).BasePolygon);
            }
        }

        /// <summary>
        /// A matrix that transforms the page in such a way that it can be read 
        /// left to right, top to bottom.
        /// </summary>
        public static Matrix ReadingDirectionToMatrix(ReadingDirection direction)
        {
            switch (direction)
            {
                case ReadingDirection.DownLeft:
                    return new Matrix(-1, 0, 0, 1, 0, 0);
                case ReadingDirection.RightDown:
                    return new Matrix(0, 1, 1, 0, 0, 0);
                case ReadingDirection.LeftDown:
                    return new Matrix(0, -1, 1, 0, 0, 0);
                default:
                    break;
            }
            return new Matrix(1, 0, 0, 1, 0, 0);
        }

        /// <summary>
        /// A matrix that transform left to right, top to bottom content into
        /// content readable in the specified direction.
        /// </summary>
        public static Matrix ReadingDirectionToInverseMatrix(ReadingDirection direction)
        {
            Matrix m = ReadingDirectionToMatrix(direction);
            m.Invert();
            return m;
        }
    }
}
