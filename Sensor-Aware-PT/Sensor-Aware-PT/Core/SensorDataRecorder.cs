
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

        /// <summary>
        /// Holds the data that is recoreded
        /// </summary>
        private List<SensorDataEntry> mDataList = new List<SensorDataEntry>();
        
        /// <summary>
        /// Used to keep track of the start of record time, to calculate timespans/offsets
        /// since the start of recording
        /// </summary>
        private DateTime mStartTime;

        private bool mIsRecording = false;

        /// <summary>
        /// Determines if the recorder is recording right now
        /// </summary>
        public bool IsRecording
        {
            get
            {
                return mIsRecording;
            }
        }

        /// <summary>
        /// Determines if the recorder has any data recorded yet
        /// </summary>
        public bool HasData
        {
            get
            {
                return mDataList.Count > 0;
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
            Logger.Info( "Started recording sensor data" );
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
                DateTime stopTime = DateTime.Now;
                /** We subtract the start time from each of the entries. This makes it so that the first entry is at time 0...etc
                 * so that replaying it is easier*/
                for( int i = 0; i < mDataList.Count; i++ )
                {
                    TimeSpan t = mDataList[ i ].timeStamp.Subtract( mStartTime );
                    mDataList[ i ].timeStamp = new DateTime( t.Ticks );
                    mDataList[ i ].timeSpan = new TimeSpan( t.Ticks );
                }

                Logger.Info( "Recorded {0} seconds of data with {1} entries", ( stopTime - mStartTime ).TotalSeconds, mDataList.Count );
                return mDataList;
            }
            return null;
        }

        /// <summary>
        /// Saves the recorded data to a file
        /// </summary>
        /// <param name="outputFile">File to record to</param>
        public void saveRecording( String outputFile )
        {
            Logger.Info( "Writing sensor recording to file {0}", outputFile );
            Stream outStream = File.Open( outputFile, FileMode.Create );
            BinaryFormatter outputFormatter = new BinaryFormatter();
            outputFormatter.Serialize( outStream, mDataList );
            outStream.Flush();
            outStream.Close();
        }
    }
}
