using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;
using System.Threading;
using Sensor_Aware_PT.Forms;
using System.ComponentModel;
namespace Sensor_Aware_PT
{
    /// <summary>
    /// Used to play back sensor data. Since this class is the same type of observable as Nexus,
    /// anything that subscribes to nexus can subscribe to this as well.
    /// </summary>
    public class SensorDataPlayer : IObservable<SensorDataEntry>
    {
        enum DataPlayerState
        {
            Uninitialized,
            Paused,
            Ready, //at the start but not playing
            Playing,
            Finished
        }
        /// <summary>
        /// Used to keep track of time
        /// </summary>
        MicroLibrary.MicroStopwatch mPreciseCounter = new MicroLibrary.MicroStopwatch();
        
        /// <summary>
        /// Index counters to go through the data sequentially on playback
        /// </summary>
        private int mCurrentIndex = 0;
        private int mMaxIndex = 0;
        private int mOffsetTime = 0;
        private TimeSpan mOffsetSpan = TimeSpan.Zero;
        private DataPlayerState mCurrentState = DataPlayerState.Uninitialized;
                    BackgroundWorker bg = new BackgroundWorker();

        /// <summary>
        /// Holds the data to be played
        /// </summary>
        //private List<SensorDataEntry> mData.mDataList;
        private ReplayData mData;

        public int Length
        {
            get
            {
                return mMaxIndex;
            }
        }

        public int CurrentPosition
        {
            get
            {
                return mCurrentIndex;
            }
        }
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
            bg.DoWork += new DoWorkEventHandler( delegate
            {
                this.play();

            } );
            bg.WorkerSupportsCancellation = true;
        }

        /// <summary>
        /// Loads a file with recorded sensor data and replays it through the observerable interface
        /// Note that this function blocks, so call it in a different thread
        /// </summary>
        /// <param name="filename">The file to play</param>
        public void replayFile( string filename )
        {
            loadFile( filename );
            mPreciseCounter.Start();
            
            while( mCurrentIndex < mMaxIndex )
            {
                SensorDataEntry data = mData.mDataList[ mCurrentIndex ];
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
            mData.mDataList.Clear();
             
            
        }

        public ReplayData loadFile( string filename )
        {
            Stream inputStream = File.OpenRead( filename );
            BinaryFormatter inputFormatter = new BinaryFormatter();

            mData = ( ReplayData ) inputFormatter.Deserialize( inputStream );
            mMaxIndex = mData.mDataList.Count;
            mCurrentState = DataPlayerState.Ready;
            return mData;
        }

        public void beginPlay()
        {
            Logger.Info( "Beginning to replay" );
            bg.RunWorkerAsync();

            
        }
        private void play()
        {
            if( mCurrentState == DataPlayerState.Uninitialized )
                throw new BadJooJooException( "Can't start playing data if uninitialized" );
            
            mCurrentState = DataPlayerState.Playing;
            
            mPreciseCounter.Start();
            while( (mCurrentIndex < mMaxIndex) && mCurrentState == DataPlayerState.Playing )
            {
                SensorDataEntry data = mData.mDataList[ mCurrentIndex ];
                if( mPreciseCounter.Elapsed.CompareTo( data.timeSpan - mOffsetSpan ) >= 0 )
                {
                    NotifyObservers( data );
                    mCurrentIndex++;
                }
                Thread.SpinWait( 500 );
            };

            Logger.Info( "Player leaving while loop" );
            if(mCurrentIndex >= mMaxIndex)
            {
                mPreciseCounter.Reset();
                //mMaxIndex = 0;
                mCurrentIndex = 0;
                mCurrentState = DataPlayerState.Finished;
                mOffsetSpan = TimeSpan.Zero;
                Logger.Info( "Player leaving beause current index > max" );
                return;
            }

            if( mCurrentState == DataPlayerState.Paused )
            {
                Logger.Info( "Player leaving beause state change to pause" );
                mPreciseCounter.Stop();
                return;
                
            }
            
        }

        public void pause()
        {
            if( mCurrentState == DataPlayerState.Playing )
            {
                mCurrentState = DataPlayerState.Paused;   
            }
            else if( mCurrentState == DataPlayerState.Paused )
            {
                beginPlay();
            }
        }

        public void seekTo( int seek )
        {

            mCurrentIndex = seek;
            mOffsetSpan = mData.mDataList[ mCurrentIndex ].timeSpan;
            mPreciseCounter.Reset();
            mPreciseCounter.Start();

            if( mCurrentState == DataPlayerState.Finished )
                beginPlay();

        }
    }
}
