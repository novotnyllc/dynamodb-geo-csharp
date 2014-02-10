using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Geo.Model;
using Google.Common.Geometry;

namespace Amazon.Geo.S2
{
    internal static class S2Manager
    {
        public static S2CellUnion FindCellIds(S2LatLngRect latLngRect)
        {
            var queue = new ConcurrentQueue<S2CellId>();


            var cellIds = new List<S2CellId>();

            for (var c = S2CellId.Begin(0); !c.Equals(S2CellId.End(0)); c = c.Next)
            {
                if (ContainsGeodataToFind(c, latLngRect))
                {
                    queue.Enqueue(c);
                }
            }

            ProcessQueue(queue, cellIds, latLngRect);
            Debug.Assert(queue.Count == 0);

            queue = null;

            if (cellIds.Count > 0)
            {
                var cellUnion = new S2CellUnion();
                cellUnion.InitFromCellIds(cellIds); // This normalize the cells.
                // cellUnion.initRawCellIds(cellIds); // This does not normalize the cells.
                cellIds = null;

                return cellUnion;
            }

            return null;
        }

        private static bool ContainsGeodataToFind(S2CellId c, S2LatLngRect latLngRect)
        {
            return latLngRect.Intersects(new S2Cell(c));
        }

        private static void ProcessQueue(ConcurrentQueue<S2CellId> queue, List<S2CellId> cellIds,
                                         S2LatLngRect latLngRect)
        {
            S2CellId cell;
            while (queue.TryDequeue(out cell))
            {
                if (!cell.IsValid)
                {
                    break;
                }

                ProcessChildren(cell, latLngRect, queue, cellIds);
            }
        }

        private static void ProcessChildren(S2CellId parent, S2LatLngRect latLngRect,
                                            ConcurrentQueue<S2CellId> queue, List<S2CellId> cellIds)
        {
            var children = new List<S2CellId>(4);

            for (var c = parent.ChildBegin; !c.Equals(parent.ChildEnd); c = c.Next)
            {
                if (ContainsGeodataToFind(c, latLngRect))
                {
                    children.Add(c);
                }
            }

            /*
		 * TODO: Need to update the strategy!
		 * 
		 * Current strategy:
		 * 1 or 2 cells contain cellIdToFind: Traverse the children of the cell.
		 * 3 cells contain cellIdToFind: Add 3 cells for result.
		 * 4 cells contain cellIdToFind: Add the parent for result.
		 * 
		 * ** All non-leaf cells contain 4 child cells.
		 */
            if (children.Count == 1 || children.Count == 2)
            {
                foreach (var child in children)
                {
                    if (child.IsLeaf)
                    {
                        cellIds.Add(child);
                    }
                    else
                    {
                        queue.Enqueue(child);
                    }
                }
            }
            else if (children.Count == 3)
            {
                cellIds.AddRange(children);
            }
            else if (children.Count == 4)
            {
                cellIds.Add(parent);
            }
            else
            {
                Debug.Assert(false); // This should not happen.
            }
        }

        public static ulong GenerateGeohash(GeoPoint geoPoint)
        {
            var latLng = S2LatLng.FromDegrees(geoPoint.Latitude, geoPoint.Longitude);
            var cell = new S2Cell(latLng);
            var cellId = cell.Id;

            return cellId.Id;
        }

        public static ulong GenerateHashKey(ulong geohash, int hashKeyLength)
        {
            var geohashString = geohash.ToString(CultureInfo.InvariantCulture);
            var denominator = (ulong)Math.Pow(10, geohashString.Length - hashKeyLength);
            return geohash/denominator;
        }
    }
}