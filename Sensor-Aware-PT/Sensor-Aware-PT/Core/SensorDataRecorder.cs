
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
            if( isRecording )
            {
                mDataList.Add( value );
            }
        }

        #endregion

        private const String CFG_DIR = "Sensor-Aware-PT";

        /** Path to config file **/
        private static String AppDataDirPath = Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData );


        List<SensorDataEntry> mDataList = new List<SensorDataEntry>();
        DateTime mStartTime;
        public bool isRecording = false;

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

        public List<SensorDataEntry> stopRecording()
        {
            if( isRecording )
            {
                isRecording = false;

                /** We subtract the start time from each of the entries. This makes it so that the first entry is at time 0...etc
                 * so that replaying it is easier*/
                for( int i = 0; i < mDataList.Count; i++ )
                {
                    DateTime dd = mDataList[ i ].timeStamp;
                    TimeSpan t = mDataList[ i ].timeStamp.Subtract( mStartTime );
                    DateTime t2 = mDataList[ i ].timeStamp.Add( -t );
                    mDataList[ i ].timeStamp = new DateTime( t.Ticks );

                    Logger.Info( "now={0}, span={1}, later={2}, start={3}", dd.Ticks, t.Ticks, t2.Ticks, mStartTime.Ticks );

                }

                return mDataList;
            }
            return null;
        }

        public void writeFile( String outputFile )
        {
            String outputPath = Path.Combine( new String[] { AppDataDirPath, CFG_DIR, outputFile } );
            Stream outStream = File.Open( outputPath, FileMode.Create );
            BinaryFormatter outputFormatter = new BinaryFormatter();

            outputFormatter.Serialize( outStream, mDataList );
            outStream.Flush();
            outStream.Close();

        }
    }
}
