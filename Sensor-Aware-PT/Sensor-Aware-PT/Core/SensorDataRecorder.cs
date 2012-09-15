
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

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
            if( mIsRecording )
            {
                mDataList.Add( value );
            }
        }

        #endregion


        private List<SensorDataEntry> mDataList = new List<SensorDataEntry>();
        private DateTime mStartTime;

        private bool mIsRecording = false;

        public bool IsRecording
        {
            get
            {
                return mIsRecording;
            }
        }

        public SensorDataRecorder()
        {
            Nexus.Instance.Subscribe( this );
        }

        /// <summary>
        /// Starts recording data from Nexus. Additionally, clears any old data.
        /// </summary>
        public void beginRecording()
        {
            mIsRecording = true;
            mDataList.Clear();
            mStartTime = DateTime.Now;
        }

        /// <summary>
        /// Stops recording data from nexus
        /// </summary>
        /// <returns>A <c>List<SensorDataEntry></c> which contains the recorded entries. 
        /// The timestamps have been changed into an appropriate form for a timespan, used by the replayer.</returns>
        public List<SensorDataEntry> stopRecording()
        {
            if( mIsRecording )
            {
                mIsRecording = false;

                /** We subtract the start time from each of the entries. This makes it so that the first entry is at time 0...etc
                 * so that replaying it is easier*/
                for( int i = 0; i < mDataList.Count; i++ )
                {
                    TimeSpan t = mDataList[ i ].timeStamp.Subtract( mStartTime );
                    mDataList[ i ].timeStamp = new DateTime( t.Ticks );
                }

                return mDataList;
            }
            return null;
        }

        public void saveRecording( String outputFile )
        {
            Stream outStream = File.Open( outputFile, FileMode.Create );
            BinaryFormatter outputFormatter = new BinaryFormatter();
            outputFormatter.Serialize( outStream, mDataList );
            outStream.Flush();
            outStream.Close();
        }
    }
}
