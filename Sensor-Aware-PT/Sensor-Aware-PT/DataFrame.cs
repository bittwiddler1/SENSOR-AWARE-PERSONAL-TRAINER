using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace Sensor_Aware_PT
{
    /// <summary>
    /// A data frame from Nexus, which contains sensor data for all active sensors
    /// </summary>
    public class DataFrame
    {
        public readonly int sequenceNumber;
        public readonly DateTime timeStamp;
        public int sensorCount;
        public ConcurrentDictionary<String, SensorDataEntry> concurrentData;

        public DataFrame( int seqNum, DateTime time )
        {
            concurrentData = new ConcurrentDictionary<string, SensorDataEntry>();
            sensorCount = 0;
            sequenceNumber = seqNum;
            timeStamp = time;
        }

        public void addDataEntry( String Id, SensorDataEntry data )
        {
            concurrentData.GetOrAdd( Id, data );
            sensorCount++;
        }
    }
}
