using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using OpenTK;
using System.IO;
using Sensor_Aware_PT.Forms;

namespace Sensor_Aware_PT
{

    /** Created to encapsulate a sensor */
    public class Sensor
    {
        #region Constants and Variables
        /** Timeout in ms for IO */
        static int SERIAL_IO_TIMEOUT = 1000;
        /** Max number of values to keep in history */
        static int HISTORY_BUFFER_SIZE = 500;
        /** Max number of retries after disconnected */
        static int MAX_RECONNECT_RETRIES = 3;
        /** Amount of time to wait between reconnects, in ms */
        static int RECONNECT_TIMEOUT = 65000;
        /** Maximum # of disconnects before never reconnecting */
        private const int MAX_READ_ERRORS = 7;
        System.Diagnostics.Stopwatch mTimeoutWatch;

        /** Friendly sensor ID A-D */
        private string mID;
        //private int mReadErrors;

        public string Id
        {
            get { return mID;  }
            set { mID = value; }
        }

        /** Last 4 Digits of mac address, used for ID purposes */
        private string mMAC;

        public string MacAddress
        {
            get { return mMAC;  }
            set { mMAC = value; }
        }
        /** COM port of sensor */
        private string mPortName;

        public string SerialPortName
        {
            get { return mPortName;  }
            set { mPortName = value; }
        }
        /** Serial Port for data*/
        private SerialPort mSerialPort;
        /** Thread to handle the constant stream of data input */
        private Thread mReadThread;

        /** Keeps track of the # of the data packet since last reset */
        private int mSequenceNum;

        /** Circular buffer to hold the data input */
        private RingBuffer<SensorDataEntry> mData;

        /** Lock on the read thread when resynchronizing */
        private readonly object mSynchronizationLock = new object();
        /** Keeps track of the # of reconnects attempted */
        private int mReconnectRetryCount = 0;

        /** keeps track of the # of disconnects */
        private int mDisconnectCount = 0;

        /// <summary>
        /// Holds the state of the sensor
        /// </summary>
        enum SensorState
        {
            Uninitialized, /** Nothing has happened yet */
            Initialized, /** Serial port was created and opened */
            Activated, /** Serial port is now reading data */
            NotPresent, /** Serial port did not open */
            Deactivated, /** Serial port was open but failed to resync */
            PreActivated, /** Serial port is open but has not started reading data */
            ReInitialized,
            ReConnecting
        };
        private SensorState mCurrentSensorState = SensorState.Uninitialized; /** Default state for the sensor */
        private SensorState mPreviousSensorState = SensorState.Uninitialized;
        
        #endregion

        #region event handling stuff
        
        /** Event arguments container class */
        public class DataReceivedEventArgs : System.EventArgs
        {
            string mID;
            SensorDataEntry mData;

            public DataReceivedEventArgs( string id, SensorDataEntry data)
            {
                mID = id;
                mData = data;
            }            
            public string Id
            {
                get{ return mID;}
            }

            public SensorDataEntry Data
            {
                get{ return mData; }
            }
        }

        /** Event handler for new data recieved event */
        public delegate void DataReceivedHandler( object sender, DataReceivedEventArgs e );

        public event DataReceivedHandler DataReceived;

        /** Event delegate and handler for sensor ready. Note that if a sensor fails to open, it is still marked as ready
         * A call to IsActive is expected to check if it's active or not
         */
        public delegate void InitializationCompleteHandler( object sender, EventArgs e );
        public event InitializationCompleteHandler InitializationComplete;

        /** Event delegate and handler for sensor ready. Note that if a sensor fails to open, it is still marked as ready
        * A call to IsActive is expected to check if it's active or not
        */
        public delegate void ActivationCompleteHandler( object sender, EventArgs e );
        public event ActivationCompleteHandler ActivationComplete;
        
        /** Event delegate and handler for reactivation */
        public delegate void ReActivationCompleteHandler( object sender, EventArgs e );
        public event ReActivationCompleteHandler ReActivationComplete;
        
        /** Event delegate and handler for reinitialization */
        public delegate void ReInitializationCompleteHandler( object sender, EventArgs e );
        public event ReInitializationCompleteHandler ReInitializationComplete;

        /** Event delegate and handler for sensor disconnected */
        public delegate void DisconnectedHandler( object sender, EventArgs e );
        public event DisconnectedHandler Disconnected;

        #endregion
        

        public Sensor(SensorIdentification config)
        {
            mID = config.Id;
            mMAC = config.Mac;
            mPortName = config.PortName;
            mData = new RingBuffer<SensorDataEntry>(HISTORY_BUFFER_SIZE);
            /** Add an empty entry in case getLastEntry is called before data comes in */
            mData.Add( new SensorDataEntry() );
            mTimeoutWatch = new System.Diagnostics.Stopwatch();
            
            
        }

        /// <summary>
        /// Initializes the sensor using the ID and MAC that has been provided already
        /// </summary>
        public void initialize()
        {
            try
            {
                /** Setup the reading thread */
                mReadThread = new Thread( readThreadRun );
                /** Format for name of each read thread is
                 * readThreadRunA, readThreadRunB...etc */
                mReadThread.Name = String.Format( "readThreadRun{0}", mID );
                mReadThread.IsBackground = true;
                /** Setup the serial port */
                mSerialPort = new SerialPort( mPortName, Nexus.SENSOR_BAUD_RATE );
                mSerialPort.ReadTimeout = SERIAL_IO_TIMEOUT;
                mSerialPort.WriteTimeout = SERIAL_IO_TIMEOUT;
                mSerialPort.DataReceived += new SerialDataReceivedEventHandler( mSerialPort_DataReceived );
                try
                {
                    mSerialPort.Open();
                    /** This readByte is because some BT serial ports open even if not connected, but fail the read */
                    mSerialPort.ReadByte();
                    changeState( SensorState.Initialized );
                    
                    Logger.Info( "Sensor {0} initialized", mID );
                    OnInitializationComplete();
                }
                catch(Exception)
                {
                    Logger.Error( "Sensor {0} is not available", mID );
                    changeState( SensorState.NotPresent );
                    /** Send the sensor ready, assume listeners check for IsActive */
                    OnInitializationComplete();
                    return;
                }

            }
            catch( Exception e)
            {
                 Logger.Error( e.Message );
            }
  
        }

        /// <summary>
        /// Reinitialize the sensors after a disconnect
        /// </summary>
        private void reinitialize()
        {
            Logger.Warning( "Sensor {0} waiting {1} seconds before attempting to reconnect", mID, RECONNECT_TIMEOUT / 1000 );
            //mReadThread.Abort();
            Thread.Sleep( RECONNECT_TIMEOUT );
            mReconnectRetryCount++;
            mSerialPort.Close();
            if( mReconnectRetryCount < MAX_RECONNECT_RETRIES )
            {
                try
                {
                    /** Setup the reading thread */
                    mReadThread = new Thread( readThreadRun );
                    /** Format for name of each read thread is
                     * readThreadRunA, readThreadRunB...etc */
                    mReadThread.Name = String.Format( "readThreadRun{0}", mID );
                    mReadThread.IsBackground = true;
                    /** Setup the serial port */
                    mSerialPort = new SerialPort( mPortName, Nexus.SENSOR_BAUD_RATE );
                    mSerialPort.ReadTimeout = SERIAL_IO_TIMEOUT;
                    mSerialPort.WriteTimeout = SERIAL_IO_TIMEOUT;
                    //mSerialPort.DataReceived += new SerialDataReceivedEventHandler( mSerialPort_DataReceived );
                    try
                    {
                        mSerialPort.Open();
                        /** This readByte is because some BT serial ports open even if not connected, but fail the read */
                        mSerialPort.ReadByte();
                        Logger.Info( "Sensor {0} re-initialized", mID );
                        changeState( SensorState.ReInitialized );
                        OnReInitializationComplete();
                    }
                    catch( Exception Ex)
                    {
                        
                        Logger.Error( "Sensor {0} unable to reconnect - {1}", mID, Ex.Message );
                    }
                    finally
                    {
                        /** Reconnect unless init complete */
                        switch( mCurrentSensorState )
                        {
                            case SensorState.ReInitialized:
                                mReadThread.Start();
                                break;
                            case SensorState.ReConnecting:
                                reinitialize();
                                break;
                            default:
                                break;
                        }
                    }

                }
                catch( Exception e )
                {
                    Logger.Error( e.Message );
                }
            }
            else
            {
                Logger.Error( "Sensor {0} exhausted all reconnect attempts", mID );
                changeState( SensorState.NotPresent );
            }
        }

        /// <summary>
        /// Raises the reinitialization complete event
        /// </summary>
        private void OnReInitializationComplete()
        {
            ReInitializationCompleteHandler handler = ReInitializationComplete;
            try
            {
                if( handler != null )
                {
                    handler( this, EventArgs.Empty );
                }
            }
            catch( Exception e )
            {
                throw e;
            }
        }

        /// <summary>
        /// Raises the disconnected event
        /// </summary>
        private void OnDisconnected()
        {
            DisconnectedHandler handler = Disconnected;
            try
            {
                if( handler != null )
                {
                    handler( this, EventArgs.Empty );
                }
            }
            catch( Exception e )
            {
                throw e;
            }
        }

        /// <summary>
        /// Event to handle data coming in on the serial port. This is only important after opening the serial port while
        /// waiting for it to begin the activation process. Data comes in during the wait and must be purged or the buffer will overflow
        /// on some implementations and cause problems.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void mSerialPort_DataReceived( object sender, SerialDataReceivedEventArgs e )
        {
            if( mCurrentSensorState == SensorState.Initialized || mCurrentSensorState == SensorState.ReInitialized)
            {
                if( mSerialPort.BytesToRead > 0 )
                    mSerialPort.ReadExisting();
            }
        }

        /// <summary>
        /// Activates the sensor after it has been initialized.
        /// </summary>
        /// <exception cref="Exception">Thrown when unable to activate a sensor due to not being initialized</exception>
        public void activate()
        {
            if( mCurrentSensorState == SensorState.Initialized )
                mReadThread.Start();
            else
                throw new Exception( String.Format( "Cannot activate sensor {0} since it is not initialized", mID ) );
        }

        /// <summary>
        /// Reads a float from the serial port. The Razor sends out 4-byte floats which are little endian, which
        /// are converted to big endian.
        /// </summary>
        /// <returns>4-byte float read from the serial stream</returns>
        private float readFloat()
        {
            /** Convert from little endian (Razor) to big endian (.net) and interpret as float */
            byte[] val = new byte[4];
            for( int k = 0; k < 4; k++ )
            {
                val[ k ] = (byte)mSerialPort.ReadByte();
            }
            /** BitConverter is assumed to be big-endian by default */
            return BitConverter.ToSingle( val, 0 );

        }

        /// <summary>
        /// Reads a token from the serial port, optionally waiting until the token is recieved.
        /// </summary>
        /// <returns>True if the token was read, false if not</returns>
        private bool readToken(String token) 
        {
            //if(mSerialPort.BytesToRead != 0)
                //Logger.Info( "Sensor {0} bytes to read {1}", mID, mSerialPort.BytesToRead );
            if( mSerialPort.BytesToRead < token.Length )
            {
                
                return false;
            }
                
            /** Check if incoming bytes match token */
            for( int i = 0; i < token.Length; i++ )
            {
                if( mSerialPort.ReadChar() != token[ i ] )
                    return false;
            }

            return true;
        }



        /// <summary>
        /// Background thread that initializes the serial port, opens it, begins communication,
        /// and loops infinitely for data.
        /// </summary>
        private void readThreadRun()
        {
            
            if ((mCurrentSensorState == SensorState.Initialized )||( mCurrentSensorState == SensorState.ReInitialized))
            {
                changeState( SensorState.PreActivated );

                try
                {
                    //Thread.Sleep( 3000 );
                    Logger.Info( "Sensor {0} synchronization started", mID );

                    mSerialPort.DiscardInBuffer();
                    mSerialPort.DiscardOutBuffer();
                    /** Sets the output parameters */
                    mSerialPort.Write( "#ob" );  /** Turn on binary output */
                    mSerialPort.Write( "#o1" );  /** Turn on continuous streaming output */
                    mSerialPort.Write( "#oe0" ); /** Disable error message output*/

                    /** Clear the input buffer and then request the sync token */
                    
                    bool synchronized = false;
                    bool synchronizationTimeout = false;
                    /** Wait until we are synchronized */
                    /** Clear the input buffer and then request the sync token */
                    mTimeoutWatch.Restart();
                    do
                    {
                        mSerialPort.DiscardInBuffer();
                        mSerialPort.DiscardOutBuffer();
                        mSerialPort.Write( "#s00" );
                        Thread.Sleep( 5 );
                        synchronized = readToken( "#SYNCH00\r\n" );
                        if( mTimeoutWatch.ElapsedMilliseconds >= 15000 )
                            synchronizationTimeout = true;
                    }
                    while( !synchronized  && !synchronizationTimeout);

                    mTimeoutWatch.Reset();

                    if( synchronizationTimeout )
                        throw new BadJooJooException( "Synchronization timed the fuck out" );


                    Logger.Info( "Sensor {0} synchronization complete", mID );

                    if( mPreviousSensorState == SensorState.ReInitialized )
                    {
                        changeState( SensorState.Activated );
                        OnReactivationComplete();
                    }
                    else if( mPreviousSensorState == SensorState.Initialized )
                    {
                        changeState( SensorState.Activated );
                        OnActivationComplete();
                    }

                    /** Send the sensor ready event */

                    while( mCurrentSensorState == SensorState.Activated )
                    {
                        SensorDataEntry newData;
                        lock( mSynchronizationLock )
                        {
                            /** Read the data and add to circular buffer */
                            newData = readDataEntry();
                            Thread.SpinWait( 10000 );
                        }

                        addDataEntry( newData );
                        //mData.Add( newData );
                        
                        /** Call the event to notify and listeners 
                        DataReceivedEventArgs dataEventArgs = new DataReceivedEventArgs( mID, newData );
                        OnDataReceived( dataEventArgs );
                        */
                        //Logger.Info( "Sensor {0} data: {1}, {2}, {3}", mID, newData.orientation.X, newData.orientation.Y, newData.orientation.Z );
                    }
                }
                catch( Exception e )
                {
                    Logger.Error( "Sensor {0} read thread exception: {1}", mID, e.Message );
                    //throw;
                    //throw new Exception( String.Format( "Sensor {0} read thread exception: {1}", mID, e.Message ) );
                }
                finally
                {

                    if( mSerialPort.IsOpen )
                        mSerialPort.Close();    
                    switch( mCurrentSensorState )
                    {
                        case SensorState.Uninitialized:
                            break;
                        case SensorState.Initialized:
                            break;
                            /** Fallthrough cases */
                        case SensorState.PreActivated:
                        case SensorState.Activated:
                            {
                                mDisconnectCount++;
                                if( mDisconnectCount < MAX_READ_ERRORS )
                                {
                                    Logger.Error( "Sensor {0} disconnected, attempting to reconnect", mID );
                                    changeState( SensorState.ReConnecting );
                                    /** Raise the disconnect event */
                                    OnDisconnected();
                                    Thread reconnectThread = new Thread( reinitialize );
                                    reconnectThread.IsBackground = true;
                                    reconnectThread.Start();
                                }
                                else
                                {
                                    Logger.Error( "Sensor {0} maximum read error limit reach, not going to reconnect", mID );
                                    changeState( SensorState.NotPresent );
                                    /** Raise the disconnect event */
                                    OnDisconnected();
                                }
                            }
                            break;
                        case SensorState.NotPresent:
                            break;
                        case SensorState.Deactivated:
                            break;
                        default:
                            break;
                    }
                    

                }
            }
            else
            {
                Logger.Error("Tried to read data before initialized");
            }
        }

        /// <summary>
        /// Raises the reactivation complete event
        /// </summary>
        private void OnReactivationComplete()
        {
            ReActivationCompleteHandler handler = ReActivationComplete;
            try
            {
                if( handler != null )
                {
                    handler( this, EventArgs.Empty );
                }
            }
            catch( Exception e )
            {
                throw e;
            }
        }

        int mBufferIndex = 0;
        const int MAX_BUFFER_INDEX = 5;
        SensorDataEntry[] mDataBuffer = new SensorDataEntry[ MAX_BUFFER_INDEX ];

        public void addDataEntry( SensorDataEntry newData )
        {

            /*
            mDataBuffer[ mBufferIndex++ ] = newData;

            
            if( mBufferIndex == MAX_BUFFER_INDEX )
            {
             
                Matrix4 sum = new Matrix4();
                for( int i = 0; i < MAX_BUFFER_INDEX; i++ )
                {
                    sum.Row0 += mDataBuffer[ i ].orientation.Row0;
                    sum.Row1 += mDataBuffer[ i ].orientation.Row1;
                    sum.Row2 += mDataBuffer[ i ].orientation.Row2;
                    sum.Row3 += mDataBuffer[ i ].orientation.Row3;
                    
                }

                
                sum.Row0 /= (float)MAX_BUFFER_INDEX;
                sum.Row1 /= ( float ) MAX_BUFFER_INDEX;
                sum.Row2 /= ( float ) MAX_BUFFER_INDEX;
                sum.Row3 /= ( float ) MAX_BUFFER_INDEX;
                 

                newData.orientation = sum;

                Vector3 vSum = new Vector3();

                for( int i = 0; i < MAX_BUFFER_INDEX; i++ )
                {
                    vSum += mDataBuffer[ i ].accelerometer;
                }

                vSum /= ( float ) MAX_BUFFER_INDEX;


                newData.accelerometer = vSum;
                
                mBufferIndex = 0;
             */
                /** Call the event to notify and listeners */
                DataReceivedEventArgs dataEventArgs = new DataReceivedEventArgs( mID, newData );
                OnDataReceived( dataEventArgs );
            /*}*/


        }
        /// <summary>
        /// Raises the DataReceived event
        /// </summary>
        /// <param name="arg"></param>
        private void OnDataReceived( DataReceivedEventArgs arg )
        {
            /** This copy is for thread safety */
            DataReceivedHandler handler = DataReceived;
            try
            {
                if (handler != null) 
                {
                    //lock( handler )
                    {
                        handler( this, arg );
                    }
                }
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Raises the activation complete event
        /// </summary>
        private void OnActivationComplete()
        {
            ActivationCompleteHandler handler = ActivationComplete;
            try
            {
                if( handler != null )
                {
                    handler( this, EventArgs.Empty);
                }
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Raises the initialization complete event
        /// </summary>
        private void OnInitializationComplete()
        {
            InitializationCompleteHandler handler = InitializationComplete;
            try
            {
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Reads a full data entry from the serial stream and returns the prepared entry
        /// </summary>
        /// <returns>A packed SensorDataEntry</returns>
        private SensorDataEntry readDataEntry()
        {

            /** Read the 4 vectors of data */
            Matrix4 matData = Matrix4.Identity;
            Matrix4 transform = Matrix4.Identity;
            //Matrix4 transform = Matrix4.CreateRotationY( -MathHelper.PiOver2);
           //transform = transform * Matrix4.CreateRotationZ( MathHelper.Pi );
            Matrix4 rot = Matrix4.CreateRotationY( MathHelper.PiOver2 );

            /* 1 2 3
             * 4 5 6
             * 7 8 9
             * 
             * 
             */ 
            matData.M11 = readFloat();
            matData.M21 = readFloat();
            matData.M31 = readFloat();
            
            matData.M12 = readFloat();
            matData.M22 = readFloat();
            matData.M32 = readFloat();

            matData.M13 = readFloat();
            matData.M23 = readFloat();
            matData.M33 = readFloat();
            
            rot *= matData;
            matData = rot;
            
            
            //matData.Transpose();

            //transform2.M22 = -1f;
            //transform.Row0 *= -1f;
            //matData = matData * transform2;
            //matData = matData * transform;
            
            
            
            //matData.Transpose();
            float yaw, pitch, roll;
            pitch = -1f*(float)Math.Asin(matData.M31);
            roll = (float)Math.Atan2(matData.M32, matData.M33);
            yaw = (float)Math.Atan2(matData.M21, matData.M11);
            Vector3 ypr = new Vector3(yaw, pitch, roll);

            //transform.Row0 *= -1f;
           //matData = matData * transform;
            //matData = Matrix4.Transpose( matData );
            //matData.Row1 *= -1;
            
            Vector3 accData = new Vector3( readFloat(), readFloat(), readFloat() );
            /**
            Vector3 magData = new Vector3( readFloat(), readFloat(), readFloat() );
            Vector3 gyroData = new Vector3( readFloat(), readFloat(), readFloat() );
             */
            /** Returned the packed entry */
            return prepareEntry( matData, accData, Vector3.Zero, Vector3.Zero, ypr );
        }

        /// <summary>
        /// Change the state of this sensor
        /// </summary>
        /// <param name="newState">The new state</param>
        private void changeState(SensorState newState)
        {
            Logger.Info("Sensor {0} changing state from {1} to {2}", mID, mCurrentSensorState, newState);
            mPreviousSensorState = mCurrentSensorState;
            mCurrentSensorState = newState;
        }

        /// <summary>
        /// Prints a data entry to the log
        /// </summary>
        /// <param name="dataEntry">The entry to print</param>
        private void printDataEntry( SensorDataEntry dataEntry )
        {
            try
            {
                string output = dataEntry.ToString();
                Logger.Info( "Sensor {0} data: {1}", mID, output );
            }
            catch( Exception e)
            {
                Logger.Error( "Sensor {0} string printing exception: {1}", mID, e.Message );
            }
            
        }

        /// <summary>
        ///  Packs a data entry struct along with sequence and timestamp
        /// </summary>
        /// <param name="orientation">orientation angle</param>
        /// <param name="accel">accelerometer vector</param>
        /// <param name="mag">magnetometer vector</param>
        /// <param name="gyro">gyroscope vector</param>
        /// <returns></returns>
        private SensorDataEntry prepareEntry( Matrix4 orientation, Vector3 accel, Vector3 mag, Vector3 gyro, Vector3 ypr )
        {
            SensorDataEntry newEntry = new SensorDataEntry();
            newEntry.orientation = orientation;
            newEntry.yawpitchroll = ypr;
            newEntry.accelerometer = accel;
            newEntry.magnetometer = mag;
            newEntry.gyroscope = gyro;
            newEntry.sequenceNumber = mSequenceNum++;
            newEntry.timeStamp = DateTime.Now;
            newEntry.id = String.Copy( mID );
            return newEntry;
        }

        public void reset()
        {

        }

        /// <summary>
        /// Gets the newest entry from the history buffer
        /// </summary>
        /// <returns></returns>
        internal SensorDataEntry getLastEntry()
        {
            /** The list will never be empty because on creation I add an empty  entry */
            return mData.Last();
        }

         ~Sensor()
        {

            if(mSerialPort != null)
            {
                if( mSerialPort.IsOpen )
                    mSerialPort.Close();
            }
        }
        public bool IsActivated
        {
            get
            {
                return mCurrentSensorState == SensorState.Activated;
            }
        }

        public bool IsInitialized
        {
            get
            {
                return mCurrentSensorState == SensorState.Initialized;
            }
        }

        /// <summary>
        /// Resynchronizes the sensor. Sensor data may become out of sync, causing the data boundries to be wrong
        /// leading to junk data. The resynchronization process uses a lock to prevent the read thread
        /// from interfering until resynchronization is complete.
        /// </summary>
        public void resynchronize()
        {
            lock( mSynchronizationLock )
            {

                if( mCurrentSensorState == SensorState.Activated )
                {
                    Logger.Info( "Sensor {0} RE-synchronization started", mID );
                    /** Clear the input buffer and then request the sync token */

                    mSerialPort.DiscardInBuffer();
                    mSerialPort.Write( "#s00" );
                    bool synchronized = false;
                    //TODO add a timeout for waiting to resync
                    /** Wait until we are synchronized or fail*/
                    do
                    {
                        synchronized = readToken( "#SYNCH00\r\n" );
                    }
                    while( !synchronized );

                    if( synchronized )
                    {
                        /** Reset sequence and clear the buffer */
                        mData.Clear();
                        resetSequence();
                        Logger.Info( "Sensor {0} RE-synchronization complete", mID );

                    }
                    else
                    {
                        Logger.Error( "Sensor {0} RE-synchronization FAILED", mID );
                        changeState( SensorState.Deactivated );
                    }
                }
            }
        }

        /// <summary>
        /// Resets the sequence counter
        /// </summary>
        internal void resetSequence()
        {
            mSequenceNum = 0;
        }
    }
}
