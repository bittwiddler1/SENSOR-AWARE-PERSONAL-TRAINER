
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
                mData.mDataList.Add( value );
            }
        }

        #endregion

        /// <summary>
        /// Holds the data that is recoreded
        /// </summary>
        //private List<SensorDataEntry> mData.mDataList = new List<SensorDataEntry>();
        private ReplayData mData = new ReplayData();
        
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
                return mData.mDataList.Count > 0;
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
            mData.mDataList.Clear();
            mData.mCalibrationData = Nexus.CalibratedOrientations;
            mData.mSensorBoneMapping = Nexus.Instance.BoneMappings;
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
                for( int i = 0; i < mData.mDataList.Count; i++ )
                {
                    TimeSpan t = mData.mDataList[ i ].timeStamp.Subtract( mStartTime );
                    mData.mDataList[ i ].timeStamp = new DateTime( t.Ticks );
                    mData.mDataList[ i ].timeSpan = new TimeSpan( t.Ticks );
                }

                Logger.Info( "Recorded {0} seconds of data with {1} entries", ( stopTime - mStartTime ).TotalSeconds, mData.mDataList.Count );
                return mData.mDataList;
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
            outputFormatter.Serialize( outStream, mData );
            outStream.Flush();
            outStream.Close();

            /** Create a bunch of writer objects, 1 per each sensor in the mapping struct */
            Dictionary<string, StreamWriter> idList = new Dictionary<string, StreamWriter>();
            foreach( var kvp in mData.mSensorBoneMapping )
            {
                StreamWriter writer = new StreamWriter( File.Open( outputFile + kvp.Key + ".csv", FileMode.Create ) );
                idList.Add(kvp.Key, writer);
            }


            for( int i = 0; i < mData.mDataList.Count; i++ )
            {
                
                idList[mData.mDataList[i].id].WriteLine("{0},{1},{2},{3}", 
                mData.mDataList[i].timeSpan,
                mData.mDataList[ i ].accelerometer.X,
                mData.mDataList[ i ].accelerometer.Y,
                mData.mDataList[ i ].accelerometer.Z
                );
            }

            foreach(var kvp in idList)
            {
                kvp.Value.Flush();
                kvp.Value.Close();
                FileStream theFile = File.Open( outputFile + kvp.Key + ".csv", FileMode.Open);
                if( theFile.Length > 0 )
                {
                    Logger.Info( "Sensor data recorder: File {0} has data and was written", kvp.Key );
                }
                else
                {
                    Logger.Info( "Sensor data recorder: File {0} DOES NOT have data and was deleted!", kvp.Key );
                    theFile.Close();
                    File.Delete( outputFile + kvp.Key + ".csv" );
                }

            }
        }
    }
}
