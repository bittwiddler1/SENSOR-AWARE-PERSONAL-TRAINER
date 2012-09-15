using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
namespace Sensor_Aware_PT
{
    public class SensorDataPlayer
    {
        private const String CFG_DIR = "Sensor-Aware-PT";

        /** Path to config file **/
        private static String AppDataDirPath = Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData );
        private DateTime mStartTime;
        private object mObserverLock = new object();
        private int mCurrentIndex = 0;
        private int mMaxIndex = 0;
        List<IObserver<SensorDataEntry>> mObservers = new List<IObserver<SensorDataEntry>>();
        Timer mReplayTimer = new Timer();
        private List<SensorDataEntry> mDataList;

        #region ObserverPattern
        public IDisposable Subscribe( IObserver<SensorDataEntry> observer )
        {
            lock( mObserverLock )
            {
                if( mObservers.Contains( observer ) == false )
                {
                    mObservers.Add( observer );
                }

                return new Unsubscriber( mObservers, observer );
            }
        }

        private class Unsubscriber : IDisposable
        {
            private List<IObserver<SensorDataEntry>> mList;
            private IObserver<SensorDataEntry> mObserver;

            public Unsubscriber( List<IObserver<SensorDataEntry>> list, IObserver<SensorDataEntry> observer )
            {
                this.mList = list;
                this.mObserver = observer;
            }

            public void Dispose()
            {
                if( this.mObserver != null && this.mList.Contains( mObserver ) )
                {
                    this.mList.Remove( mObserver );
                }
            }
        }

        public void NotifyObservers( SensorDataEntry dataFrame )
        {
            foreach( IObserver<SensorDataEntry> observer in mObservers )
            {
                observer.OnNext( dataFrame );

            }
        }
        #endregion

        public SensorDataPlayer()
        {
            mReplayTimer.Interval = 1;
            mReplayTimer.Elapsed += new ElapsedEventHandler( mReplayTimer_Elapsed );
            mReplayTimer.Enabled = false;
        }

        public void replayFile( string filename )
        {
            String inputPath = Path.Combine( new String[] { AppDataDirPath, CFG_DIR, filename } );
            Stream inputStream = File.OpenRead( inputPath );
            BinaryFormatter inputFormatter = new BinaryFormatter();

            mDataList = ( List<SensorDataEntry> ) inputFormatter.Deserialize( inputStream );
            mMaxIndex = mDataList.Count;
            mStartTime = DateTime.Now;
            mReplayTimer.Start();
        }

        void mReplayTimer_Elapsed( object sender, ElapsedEventArgs e )
        {
            if( mCurrentIndex < mMaxIndex )
            {
                TimeSpan t = DateTime.Now - mStartTime;
                if( t.TotalMilliseconds >= mDataList[ mCurrentIndex ].Timestamp )
                {
                    NotifyObservers( mDataList[ mCurrentIndex ] );
                    mCurrentIndex++;
                }
            }
            else
            {
                mReplayTimer.Stop();
                mMaxIndex = 0;
                mCurrentIndex = 0;
                mDataList.Clear();
            }
        }

    }
}
