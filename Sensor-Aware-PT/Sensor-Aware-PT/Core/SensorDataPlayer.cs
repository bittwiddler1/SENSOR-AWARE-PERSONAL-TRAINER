using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;
using System.Threading;
namespace Sensor_Aware_PT
{
    /// <summary>
    /// Used to play back sensor data. Since this class is the same type of observable as Nexus,
    /// anything that subscribes to nexus can subscribe to this as well.
    /// </summary>
    public class SensorDataPlayer : IObservable<SensorDataEntry>
    {
        /// <summary>
        /// Used to keep track of time
        /// </summary>
        MicroLibrary.MicroStopwatch mPreciseCounter = new MicroLibrary.MicroStopwatch();
        
        /// <summary>
        /// Index counters to go through the data sequentially on playback
        /// </summary>
        private int mCurrentIndex = 0;
        private int mMaxIndex = 0;
        
        /// <summary>
        /// Holds the data to be played
        /// </summary>
        private List<SensorDataEntry> mDataList;

        #region ObserverPattern

        /// <summary>
        /// Lock for the IObservable stuff
        /// </summary>
        private object mObserverLock = new object();

        /// <summary>
        /// Holds the Observers observing this object
        /// </summary>
        List<IObserver<SensorDataEntry>> mObservers = new List<IObserver<SensorDataEntry>>();

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
        }

        /// <summary>
        /// Loads a file with recorded sensor data and replays it through the observerable interface
        /// Note that this function blocks, so call it in a different thread
        /// </summary>
        /// <param name="filename">The file to play</param>
        public void replayFile( string filename )
        {
            Stream inputStream = File.OpenRead( filename );
            BinaryFormatter inputFormatter = new BinaryFormatter();

            mDataList = ( List<SensorDataEntry> ) inputFormatter.Deserialize( inputStream );
            mMaxIndex = mDataList.Count;
            
            mPreciseCounter.Start();
            
            while( mCurrentIndex < mMaxIndex )
            {
                SensorDataEntry data = mDataList[ mCurrentIndex ];
                if( mPreciseCounter.Elapsed.CompareTo( data.timeSpan ) >= 0 )
                {
                    NotifyObservers( data );
                    mCurrentIndex++;
                }
                Thread.SpinWait( 500 );
            };
            mPreciseCounter.Reset();
            mMaxIndex = 0;
            mCurrentIndex = 0;
            mDataList.Clear();
             
            
        }

    }
}
