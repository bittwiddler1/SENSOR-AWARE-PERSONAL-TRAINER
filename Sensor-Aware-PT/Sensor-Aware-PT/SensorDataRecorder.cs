using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sensor_Aware_PT
{
    /// <summary>
    /// Component to record sensor data to file
    /// </summary>
    public class SensorDataRecorder : IObserver<SensorDataEntry>
    {
        #region IObserver<SensorDataEntry> Members

        void IObserver<SensorDataEntry>.OnCompleted()
        {
            throw new NotImplementedException();
        }

        void IObserver<SensorDataEntry>.OnError( Exception error )
        {
            throw new NotImplementedException();
        }

        void IObserver<SensorDataEntry>.OnNext( SensorDataEntry value )
        {
            if( isRecording )
            {
                mDataList.Add( value );
            }
        }

        #endregion

        List<SensorDataEntry> mDataList = new List<SensorDataEntry>();
        DateTime mStartTime;
        bool isRecording = false;

        public SensorDataRecorder()
        {
            Nexus.Instance.Subscribe( this );    
        }

        public SensorDataRecorder( String outputFile )
        {

        }

        public void beginRecording()
        {
            isRecording = true;
            mDataList.Clear();
            mStartTime = DateTime.Now;

        }

        public void stopRecording()
        {
            if( isRecording )
            {
                isRecording = false;

                /** We subtract the start time from each of the entries. This makes it so that the first entry is at time 0...etc
                 * so that replaying it is easier*/
                for( int i = 0; i < mDataList.Count; i++ )
                {
                    mDataList[ i ].timeStamp.Subtract( mStartTime );
                }
            }
        }
    }
}
