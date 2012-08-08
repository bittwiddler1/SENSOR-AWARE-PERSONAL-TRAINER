using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Sensor_Aware_PT
{
    /** Created to encapsulate a sensor */
    class Sensor
    {
        /** Friendly sensor ID A-D */
        private string mSensorID;
        /** Last 4 Digits of mac address */
        private string mSensorMAC;
        /** Holds a history of previous positions */
        private List<OpenTK.Vector3> mDataHistory;


    }
}
