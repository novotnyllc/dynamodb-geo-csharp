using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Geo.Model
{
    public abstract class GeoObject
    {
        public string Type { get; protected set; }
    }
}
