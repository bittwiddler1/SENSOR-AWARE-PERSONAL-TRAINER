using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using OpenTK;
using System.IO;

namespace Sensor_Aware_PT
{

    /** Created to encapsulate a sensor */
    public class Sensor
    {
        #region Constants and Variables
        /** Timeout in ms for IO */
        static int SERIAL_IO_TIMEOUT = 20000;
        /** Max number of values to keep in history */
        static int HISTORY_BUFFER_SIZE = 500;

        private const int MAX_READ_ERRORS = 3;

        /** Friendly sensor ID A-D */
        private string mID;
        private int mReadErrors;

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
            PreActivated /** Serial port is open but has not started reading data */
        };
        private SensorState mSensorState = SensorState.Uninitialized; /** Default state for the sensor */
        
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

        #endregion
        

        public Sensor(SensorIdentification config)
        {
            mID = config.Id;
            mMAC = config.Mac;
            mPortName = config.PortName;
            mData = new RingBuffer<SensorDataEntry>(HISTORY_BUFFER_SIZE);
            /** Add an empty entry in case getLastEntry is called before data comes in */
            mData.Add( new SensorDataEntry() );
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
                mSerialPort.ReadTimeout = 1000;
                mSerialPort.WriteTimeout = 1000;
                mSerialPort.ErrorReceived += new SerialErrorReceivedEventHandler( mSerialPort_ErrorReceived );
                mSerialPort.DataReceived += new SerialDataReceivedEventHandler( mSerialPort_DataReceived );
                try
                {
                    mSerialPort.Open();
                    mSerialPort.ReadByte();
                    changeState( SensorState.Initialized );
                    
                    Logger.Info( "Sensor {0} initialized", mID );
                    OnInitializationComplete();
                }
                catch( Exception e )
                {
                    Logger.Error( "Sensor {0} serial port open exception: {1}", mID, e.Message );
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

        void mSerialPort_ErrorReceived( object sender, SerialErrorReceivedEventArgs e )
        {
            throw new NotImplementedException();
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
            if( mSensorState == SensorState.Initialized )
            {
                if( mSerialPort.BytesToRead > 0 )
                    mSerialPort.ReadExisting();
            }
        }

        /// <summary>
        /// Called to activate the sensor, after it has been initialized.
        /// </summary>
        public void activate()
        {
            if( mSensorState == SensorState.Initialized )
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
            
            if (mSensorState == SensorState.Initialized)
            {
                changeState( SensorState.PreActivated );
                
                try
                {
                    //Thread.Sleep( 3000 );
                    Logger.Info( "Sensor {0} synchronization started", mID );

                    mSerialPort.DiscardInBuffer();
                    
                    /** Sets the output parameters */
                    mSerialPort.Write( "#ob" );  /** Turn on binary output */
                    mSerialPort.Write( "#o1" );  /** Turn on continuous streaming output */
                    mSerialPort.Write( "#oe0" ); /** Disable error message output*/

                    /** Clear the input buffer and then request the sync token */
                    mSerialPort.DiscardInBuffer();
                    mSerialPort.Write( "#s00" );  
                    bool synchronized = false;

                    /** Wait until we are synchronized */
                    do
                    {
                        synchronized = readToken( "#SYNCH00\r\n");
                    }
                    while(!synchronized);

                    Logger.Info("Sensor {0} synchronization complete", mID);
                    changeState( SensorState.Activated );
                    OnActivationComplete();
                    /** Send the sensor ready event */

                    while (mSensorState == SensorState.Activated)
                    {
                        lock( mSynchronizationLock )
                        {
                            /** Read the data and add to circular buffer */
                            SensorDataEntry newData = readDataEntry();
                            mData.Add( newData );
                            /** Call the event to notify and listeners */
                            DataReceivedEventArgs dataEventArgs = new DataReceivedEventArgs( mID, newData );
                            OnDataReceived( dataEventArgs );
                        }
                        //Logger.Info( "Sensor {0} data: {1}, {2}, {3}", mID, newData.orientation.X, newData.orientation.Y, newData.orientation.Z );
                    }
                }
                catch (Exception e)
                {
                    //mReadErrors++;
                    //if( MAX_READ_ERRORS <= mReadErrors )
                    {
                        Logger.Error( "Sensor {0} read thread exception: {1}", mID, e.Message );
                        throw new Exception( String.Format( "Sensor {0} read thread exception: {1}", mID, e.Message ) );
                    }
                }
            }
            else
            {
                Logger.Error("Tried to read data before initialized");
            }
        }

        private void OnDataReceived( DataReceivedEventArgs arg )
        {
            /** This copy is for thread safety */
            DataReceivedHandler handler = DataReceived;
            try
            {
                if (handler != null) 
                {
                    handler(this, arg);
                }
            }
            catch(Exception e)
            {
                throw e;
            }
        }

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
            Vector3 vecData = new Vector3( readFloat(), readFloat(), readFloat() );
            Vector3 accData = new Vector3( readFloat(), readFloat(), readFloat() );
            Vector3 magData = new Vector3( readFloat(), readFloat(), readFloat() );
            Vector3 gyroData = new Vector3( readFloat(), readFloat(), readFloat() );
            /** Returned the packed entry */
            return prepareEntry( vecData, accData, magData, gyroData );
        }

        /// <summary>
        /// Change the state of this sensor
        /// </summary>
        /// <param name="newState">The new state</param>
        private void changeState(SensorState newState)
        {
            Logger.Info("Sensor {0} changing state from {1} to {2}", mID, mSensorState, newState);
            mSensorState = newState;
        }

        /// <summary>
        /// Prints a data entry to the log
        /// </summary>
        /// <param name="dataEntry">The entry to print</param>
        private void printDataEntry( SensorDataEntry dataEntry )
        {
            try
            {
                string output = String.Format( "Angle{{{0},{1},{2}}}, Accel{{{3},{4},{5}}}, Mag{{{6},{7},{8}}}, Gyro{{{9},{10},{11}}}",
                dataEntry.orientation.X,
                dataEntry.orientation.Y,
                dataEntry.orientation.Z,
                dataEntry.accelerometer.X,
                dataEntry.accelerometer.Y,
                dataEntry.accelerometer.Z,
                dataEntry.magnetometer.X,
                dataEntry.magnetometer.Y,
                dataEntry.magnetometer.Z,
                dataEntry.gyroscope.X,
                dataEntry.gyroscope.Y,
                dataEntry.gyroscope.Z );

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
        private SensorDataEntry prepareEntry( Vector3 orientation, Vector3 accel, Vector3 mag, Vector3 gyro )
        {
            SensorDataEntry newEntry = new SensorDataEntry();
            newEntry.orientation = orientation;
            newEntry.accelerometer = accel;
            newEntry.magnetometer = mag;
            newEntry.gyroscope = gyro;
            newEntry.sequenceNumber = mSequenceNum++;
            newEntry.timeStamp = DateTime.Now;
            newEntry.id = String.Copy( mID );
            return newEntry;
        }

        /// <summary>
        /// Dumps the contents of the history buffer to a text file
        /// </summary>
        public void dumpBuffer()
        {
            DateTime datet = DateTime.Now;
            String filePath = "CircularBufferDump " + mID + " "  + datet.ToString( "MM_dd" ) + ".log";
            
            
                FileStream files = File.Create( filePath );
                files.Close();
            
            try
            {
                StreamWriter sw = File.AppendText( filePath );

                foreach( SensorDataEntry dataEntry in mData )
                {

                    try
                    {
                        string output = String.Format( "Angle{{{0},{1},{2}}}, Accel{{{3},{4},{5}}}, Mag{{{6},{7},{8}}}, Gyro{{{9},{10},{11}}}",
                        dataEntry.orientation.X,
                        dataEntry.orientation.Y,
                        dataEntry.orientation.Z,
                        dataEntry.accelerometer.X,
                        dataEntry.accelerometer.Y,
                        dataEntry.accelerometer.Z,
                        dataEntry.magnetometer.X,
                        dataEntry.magnetometer.Y,
                        dataEntry.magnetometer.Z,
                        dataEntry.gyroscope.X,
                        dataEntry.gyroscope.Y,
                        dataEntry.gyroscope.Z );
                        sw.WriteLine( output );
                        
                    }
                    catch( Exception e )
                    {
                        throw e;
                    }
                }


                sw.Flush();
                sw.Close();
            }
            catch( Exception e )
            {
                Console.WriteLine( e.Message.ToString() );
            }
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
                return mSensorState == SensorState.Activated;
            }
        }

        public bool IsInitialized
        {
            get
            {
                return mSensorState == SensorState.Initialized;
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

                if( mSensorState == SensorState.Activated )
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
