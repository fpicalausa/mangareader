using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MangaParser.Graphics;
using MangaParser.Geometry;

namespace MangaParser.Reader
{
    using RelationGraphRelation = Tuple<CellsRelation, IPolygon>;
    using RelationGraphConnectivity = List<Tuple<CellsRelation, IPolygon>>;
    using RelationGraph = Dictionary<IPolygon, List<Tuple<CellsRelation, IPolygon>>>;

    public enum CellsRelation
    {
        None,
        BottomOf,
        RightOf,
    }

    public class DefaultReadingOrder: ICellReadingOrder
    {

        /// <summary>
        /// Determines where a cell is located in relation to another.
        /// </summary>
        /// <param name="p1">A polygon delimiting the first cell under consideration</param>
        /// <param name="p2">A polygon delimiting the second cell under consideration</param>
        /// <returns>Returns the relation between the two cells p2 and p1, namely if
        /// p2 is under p1 (BottomOf), p2 is right of p2 (RightOf), or neighter (None).</returns>
        private static CellsRelation Compare(IPolygon p1, IPolygon p2)
        {
            Point       g1      = p1.CenterOfGravity;
            Point       g2      = p2.CenterOfGravity;
            Rectangle   b1      = p1.BoundingBox;
            Rectangle   b2      = p2.BoundingBox;
            Rectangle   b1prime = Rectangle.Inflate(b1, (int)(-b1.Width * 0.1), (int)(-b1.Height * 0.1));
            Rectangle   b2prime = Rectangle.Inflate(b2, (int)(-b2.Width * 0.1), (int)(-b2.Height * 0.1));
            bool        intery  = b1prime.HeightIntersect(b2prime);

            // Cases where the bounding boxes do not intersect.
            if (b2prime.Right < b1prime.Left || b2prime.Bottom < b1prime.Top)
            {
                return CellsRelation.None;
            }
            // We want the bounding boxes to shared some Y coordinates to have a left-right relation
            else if (intery) 
            {
                return (g2.X > g1.X) ? CellsRelation.RightOf : CellsRelation.None;
            }
            else if (g2.Y > g1.Y)
            {
                return CellsRelation.BottomOf;
            }
            else
            {
                return CellsRelation.None;
            }
        }

        /// <summary>
        /// Construct a graph of relations between all cells of the manga page
        /// </summary>
        /// <param name="polygons">The polygons that each delimit one cell</param>
        /// <returns>A graph of the relations between the cells</returns>
        public static RelationGraph BuildRelationGraph<T>(IEnumerable<T> polygons) where T: IPolygon
        {
            RelationGraph graph = new RelationGraph();

            foreach (var p1 in polygons)
            {
                RelationGraphConnectivity connectivity = new RelationGraphConnectivity();

                foreach (var p2 in polygons) 
                {
                    if (!p1.Equals(p2))
                    {
                        CellsRelation relation = Compare(p1, p2);

                        if (relation != CellsRelation.None)
                        {
                            connectivity.Add(new RelationGraphRelation(relation, p2));
                        }
                    }
                }

                connectivity.Sort((r1, r2) => r1.Item1.CompareTo(r2.Item1));
                graph[p1] = connectivity;
            }

            return graph;
        }

        public static void SimplifyTransitiveClosure(RelationGraph graph)
        {
            List<Tuple<IPolygon, RelationGraphRelation>> removes = new List<Tuple<IPolygon,RelationGraphRelation>>();
            
            foreach (var polygonRelations in graph)
            {
                foreach (var relation in polygonRelations.Value)
                {
                    foreach (var relation2 in graph[relation.Item2])
                    {
                        if (relation.Item1 == CellsRelation.BottomOf)
                        {
                            removes.Add(new Tuple<IPolygon, RelationGraphRelation>(polygonRelations.Key, relation2));
                        }
                        else if (relation.Item1 == CellsRelation.RightOf && relation2.Item1 == CellsRelation.RightOf)
                        {
                            removes.Add(new Tuple<IPolygon, RelationGraphRelation>(polygonRelations.Key, relation2));
                        }
                    }
                }
            }

            foreach (var item in removes)
            {
                graph[item.Item1].Remove(item.Item2);
            }
        }

        private static RelationGraph invertGraph(RelationGraph graph)
        {
            RelationGraph result = new RelationGraph(graph.Count);

            foreach (var polygon in graph.Keys) {
                result[polygon] = new RelationGraphConnectivity();
            }

            foreach (var polygonRelations in graph)
            {
                foreach (var relation in polygonRelations.Value)
                {
                    result[relation.Item2].Add(new RelationGraphRelation(relation.Item1, polygonRelations.Key));
                }
            }

            foreach (var polygon in result.Keys) {
                result[polygon].Sort((r1, r2) => r1.Item1.CompareTo(r2.Item1));
            }

            return result;
        }

        /// <summary>
        /// Find which cells can be read next. That is, the cells such that all
        /// cells that came before have already been read.
        /// </summary>
        /// <param name="inverse">The inverted-edges reading graph.</param>
        /// <returns>Returns an enumeration of all cells that can be read neax</returns>
        private static IEnumerable<IPolygon> getEnabledPolygons(RelationGraph inverse) 
        {
            return (from prel in inverse where prel.Value.Count == 0 orderby prel.Key.BoundingBox.Y select prel.Key);
        }

        private static bool canEnablePoly(RelationGraph inverseGraph, IPolygon polygon, HashSet<IPolygon> processedPolys)
        {
            return inverseGraph[polygon].All((t) => (processedPolys.Contains(t.Item2)));
        }

        /// <summary>
        /// Fixup cyclic reading graph by getting the polygon that is the most likely to represent
        /// the cell that should be read next.
        /// </summary>
        private static IPolygon getNextPolygon(RelationGraph graph, RelationGraph inverse, HashSet<IPolygon> processedPoly)
        {
            IPolygon poly = null;

            //Find the topmost polygon
            foreach (var relation in graph)
            {
                if (!processedPoly.Contains(relation.Key))
                {
                    if (poly == null || relation.Key.BoundingBox.Top < poly.BoundingBox.Top)
                    {
                        poly = relation.Key;
                    }
                }
            }

            //Backtrack the RightOf relations each time getting the top most predecessor.
            // The idea is to rewind the current strip that we are in.
            bool changed = true;
            int limit = 0;

            while (changed && limit < 10)
            {
                changed = false;
                limit++;

                var newPolys = (from predecessor in inverse[poly]
                 where predecessor.Item1 == CellsRelation.RightOf &&
                       !processedPoly.Contains(predecessor.Item2)
                 orderby predecessor.Item2.BoundingBox.Top
                 select predecessor);

                if (newPolys.Any( (t) => true )) {
                    changed = true;
                    poly = newPolys.First().Item2;
                }
            }

            return poly;
        }

        /// <summary>
        /// Determines a reading order for all cells based on their relation position
        /// as determined by the RelationGraph graph.
        /// </summary>
        private IEnumerable<IPolygon> readGraph(RelationGraph graph)
        {
            RelationGraph inverseGraph = invertGraph(graph);
            Stack<IPolygon> enabledPolys = new Stack<IPolygon>(getEnabledPolygons(inverseGraph).Reverse());
            HashSet<IPolygon> processedPoly = new HashSet<IPolygon>();

            while (processedPoly.Count != graph.Count)
            {
                IPolygon poly = null;
                if (enabledPolys.Count != 0)
                {
                    poly = enabledPolys.Pop();
                }
                else
                {
                    //HACK: this happens when the graph is cyclic. The graph should 
                    // never be cyclic.
                    poly = getNextPolygon(graph, inverseGraph, processedPoly);
                }

                yield return poly;
                processedPoly.Add(poly);

                foreach (var relation in graph[poly])
                {
                    if (!processedPoly.Contains(relation.Item2))
                    {
                        if (canEnablePoly(inverseGraph, relation.Item2, processedPoly))
                        {
                            enabledPolys.Push(relation.Item2);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Determines a reading order for the given cells.
        /// </summary>
        /// <param name="cells">A set of polygons delimiting manga cells, for which a reading order is to be determined.</param>
        /// <returns>A sequence of the polygons delimiting cells, in the order in which they should be read.</returns>
        public IEnumerable<IPolygon> GetReadingOrder(IEnumerable<IPolygon> cells)
        {
            RelationGraph graph = BuildRelationGraph(cells.ToList());
            SimplifyTransitiveClosure(graph);

            return readGraph(graph);
        }
    }
}
