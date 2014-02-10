using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Geo.S2;

namespace Amazon.Geo.Model
{
    public sealed class GeohashRange
    {
        public GeohashRange(ulong range1, ulong range2)
        {
            RangeMin = Math.Min(range1, range2);
            RangeMax = Math.Max(range1, range2);
        }

        public ulong RangeMin { get; private set; }

        public ulong RangeMax { get; private set; }

        public bool TryMerge(GeohashRange range)
        {
            if (range.RangeMin - RangeMax <= GeoDataManagerConfiguration.MergeThreshold
                && range.RangeMin - RangeMax > 0)
            {
                RangeMax = range.RangeMax;
                return true;
            }

            if (RangeMin - range.RangeMax <= GeoDataManagerConfiguration.MergeThreshold
                && RangeMin - range.RangeMax > 0)
            {
                RangeMin = range.RangeMin;
                return true;
            }

            return false;
        }

        /*
         * Try to split the range to multiple ranges based on the hash key.
         * 
         * e.g., for the following range:
         * 
         * min: 123456789
         * max: 125678912
         * 
         * when the hash key length is 3, we want to split the range to:
         * 
         * 1
         * min: 123456789
         * max: 123999999
         * 
         * 2
         * min: 124000000
         * max: 124999999
         * 
         * 3
         * min: 125000000
         * max: 125678912
         * 
         * For this range:
         * 
         * min: -125678912
         * max: -123456789
         * 
         * we want:
         * 
         * 1
         * min: -125678912
         * max: -125000000
         * 
         * 2
         * min: -124999999
         * max: -124000000
         * 
         * 3
         * min: -123999999
         * max: -123456789
         */

        public IReadOnlyList<GeohashRange> TrySplit(int hashKeyLength)
        {
            var result = new List<GeohashRange>();

            var minHashKey = S2Manager.GenerateHashKey(RangeMin, hashKeyLength);
            var maxHashKey = S2Manager.GenerateHashKey(RangeMax, hashKeyLength);


            var denominator = (ulong)Math.Pow(10, RangeMin.ToString(CultureInfo.InvariantCulture).Length - minHashKey.ToString(CultureInfo.InvariantCulture).Length);

            if (minHashKey == maxHashKey)
            {
                result.Add(this);
            }
            else
            {
                for (var l = minHashKey; l <= maxHashKey; l++)
                {
                    if (l > 0)
                    {
                        result.Add(new GeohashRange(l == minHashKey ? RangeMin : l*denominator,
                                                    l == maxHashKey ? RangeMax : (l + 1)*denominator - 1));
                    }
                    else
                    {
                        result.Add(new GeohashRange(l == minHashKey ? RangeMin : (l - 1)*denominator + 1,
                                                    l == maxHashKey ? RangeMax : l*denominator));
                    }
                }
            }

            return result;
        }
    }
}