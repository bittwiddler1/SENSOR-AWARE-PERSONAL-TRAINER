using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Sensor_Aware_PT
{
    /** Struct to hold info on each line of sensor data */
    public class SensorDataEntry
    {
        public Vector3 orientation = new Vector3();
        public Vector3 accelerometer = new Vector3();
        public Vector3 gyroscope = new Vector3();
        public Vector3 magnetometer = new Vector3();
        public DateTime timeStamp = new DateTime();
        public int sequenceNumber;
        public string id;
    }

    /** Holds ID and MAC of each sensor */
    public class SensorIdentification
    {
        public string Id;
        public string Mac;
        public string PortName;

        public SensorIdentification()
        {
            Id = Mac = PortName = String.Empty;
        }

        public SensorIdentification( string _id, string _mac, string port = "" )
        {
            Id = _id;
            Mac = _mac;
            PortName = port;
        }
    }
}
