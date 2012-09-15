using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;
namespace Sensor_Aware_PT
{
    public class SensorDataPlayer : IObservable<SensorDataEntry>
    {
        private Stopwatch mTimeCounter = new Stopwatch();
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
            mReplayTimer.Interval = .1;
            mReplayTimer.Elapsed += new ElapsedEventHandler( mReplayTimer_Elapsed );
            mReplayTimer.Enabled = false;
        }

        public void replayFile( string filename )
        {
            Stream inputStream = File.OpenRead( filename );
            BinaryFormatter inputFormatter = new BinaryFormatter();

            mDataList = ( List<SensorDataEntry> ) inputFormatter.Deserialize( inputStream );
            mMaxIndex = mDataList.Count;
            mTimeCounter.Start();
            mReplayTimer.Start();
        }

        void mReplayTimer_Elapsed( object sender, ElapsedEventArgs e )
        {
            if( mCurrentIndex < mMaxIndex )
            {
                SensorDataEntry data = mDataList[ mCurrentIndex ];
                if( mTimeCounter.Elapsed.CompareTo(data.timeSpan) >= 0)
                {
                    NotifyObservers( data);
                    mCurrentIndex++;
                }
            }
            else
            {
                mReplayTimer.Stop();
                mTimeCounter.Reset();
                mMaxIndex = 0;
                mCurrentIndex = 0;
                mDataList.Clear();
            }
        }

    }
}
