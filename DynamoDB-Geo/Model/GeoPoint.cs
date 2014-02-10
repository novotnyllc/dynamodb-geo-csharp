using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Amazon.Geo.Model
{
    public class GeoPoint : GeoObject
    {
        private double[] _coordinates;

        public GeoPoint()
        {
            Type = "Point";
            Coordinates = new double[2];
        }

        public GeoPoint(double latitude, double longitude) : this()
        {
            Coordinates = new[] {latitude, longitude};
        }

        [JsonIgnore]
        public double Latitude { get { return Coordinates[0]; }}

        [JsonIgnore]
        public double Longitude { get { return Coordinates[1]; }}

        public double[] Coordinates
        {
            get { return _coordinates; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                if (value.Length != 2)
                    throw new ArgumentOutOfRangeException("value", "Length must be 2: Lat, Lng");

                _coordinates = value;
            }
        }

        public override string ToString()
        {
            return string.Format("({0}, {1})", Latitude, Longitude);
        }
    }
}
