using System;
namespace MangaParser.Graphics
{
    public interface IPolygon
    {
        /// <summary>
        /// The area covered by the polygon.
        /// </summary>
        long Area { get; }

        /// <summary>
        /// A rectangular bounding box arround the polygon.
        /// </summary>
        System.Drawing.Rectangle BoundingBox { get; }

        /// <summary>
        /// The center of gravity of the polygon. This center of gravity is 
        /// not the geometric center of gravity, but is determined by a density 
        /// function specific to the implementation.
        /// </summary>
        System.Drawing.Point CenterOfGravity { get; }

        /// <summary>
        /// Determines whether the given polygon is entirely included in this polygon
        /// </summary>
        /// <param name="included">The polygon whose inclusion is to be checked</param>
        /// <returns>Returns true if the test polygon is included in this polygon</returns>
        bool Contains(IPolygon included);

        /// <summary>
        /// Determines whether the given point is included in this polygon
        /// </summary>
        /// <param name="included">The point whose inclusion is to be checked</param>
        /// <returns>Returns true if the test point is included in this polygon</returns>
        bool Contains(System.Drawing.Point point);

        /// <summary>
        /// An ordered sequence of all point constituting the polygon.
        /// </summary>
        System.Collections.Generic.IEnumerable<System.Drawing.Point> Points { get; }
    }
}
