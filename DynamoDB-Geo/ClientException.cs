using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Geo
{
    public class ClientException : Exception
    {
        internal ClientException(string message) : base(message)
        {
            
        }

        internal ClientException(string message, Exception e) : base(message, e)
        {
            
        }
    }
}
