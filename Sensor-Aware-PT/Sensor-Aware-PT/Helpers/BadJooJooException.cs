using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sensor_Aware_PT.Forms
{
    class BadJooJooException : Exception
    {
        public BadJooJooException( string message ) : base( message )
        {
            ;
        }
    }
}
