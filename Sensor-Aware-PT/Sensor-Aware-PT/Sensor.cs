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
    /** Struct to hold info on each line of sensor data */
    public class SensorDataEntry
    {
        public Vector3 orientation = new Vector3();
        public Vector3 accelerometer = new Vector3();
        public Vector3 gyroscope = new Vector3();
        public Vector3 magnetometer = new Vector3();
        public DateTime timeStamp = new DateTime();
        public int sequenceNumber;
    }


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
        
        /** Sensor state */
        enum SensorState
        {
            Uninitialized,
            Initialized,
            Activated,
            NotPresent
        };
        private SensorState mSensorState = SensorState.Uninitialized; /** Default state for the sensor */
        
        #endregion

        #region event handling stuff
        
        /** Event arguments container class */
        public class SensorNewDataEventArgs : System.EventArgs
        {
            string mID;
            SensorDataEntry mData;

            public SensorNewDataEventArgs( string id, SensorDataEntry data)
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
        public delegate void SensorNewDataEventHandler( object sender, SensorNewDataEventArgs e );

        public event SensorNewDataEventHandler SensorNewDataEvent;

        /** Event delegate and handler for sensor ready. Note that if a sensor fails to open, it is still marked as ready
         * A call to IsActive is expected to check if it's active or not
         */
        public delegate void SensorInitializedEventHandler( object sender, EventArgs e );
        public event SensorInitializedEventHandler SensorInitializedEvent;

        /** Event delegate and handler for sensor ready. Note that if a sensor fails to open, it is still marked as ready
        * A call to IsActive is expected to check if it's active or not
        */
        public delegate void SensorActivatedEventHandler( object sender, EventArgs e );
        public event SensorActivatedEventHandler SensorActivatedEvent;

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
                mSerialPort.ReadTimeout = SERIAL_IO_TIMEOUT;
                mSerialPort.WriteTimeout = SERIAL_IO_TIMEOUT;
                try
                {
                    mSerialPort.Open();
                    changeState( SensorState.Initialized );
                    
                    Logger.Info( "Sensor {0} initialized", mID );
                    OnSensorInitializedEvent( new EventArgs() );
                }
                catch( Exception e )
                {
                    Logger.Error( "Sensor {0} serial port open exception: {1}", mID, e.Message );
                    changeState( SensorState.NotPresent );
                    /** Send the sensor ready, assume listeners check for IsActive */
                    OnSensorInitializedEvent( new EventArgs() );
                    return;
                }


                /** Start the read thread */
                //mReadThread.Start();
            }
            catch( Exception e)
            {
                 Logger.Error( e.Message );
            }
  
        }

        /// <summary>
        /// Called to activate the sensor, after it has been initialized.
        /// </summary>
        public void activate()
        {
            if (mSensorState == SensorState.Initialized)
                mReadThread.Start();
            else
                throw new Exception(String.Format("Cannot activate sensor {0} since it is not initialized", mID));
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

            if( mSerialPort.BytesToRead < token.Length )
                return false;
                
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
                    OnSensorActivatedEvent( new EventArgs() );
                    /** Send the sensor ready event */

                    while (true)
                    {
                        /** Read the data and add to circular buffer */
                        SensorDataEntry newData = readDataEntry();
                        mData.Add(newData);
                        /** Call the event to notify and listeners */
                        SensorNewDataEventArgs dataEventArgs = new SensorNewDataEventArgs( mID, newData );
                        OnNewSensorDataEvent( dataEventArgs );

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

        private void OnNewSensorDataEvent( SensorNewDataEventArgs arg )
        {
            /** This copy is for thread safety */
            SensorNewDataEventHandler handler = SensorNewDataEvent;
            try
            {
                if (handler != null) 
                {
                    handler(this, arg);
                }
            }
            catch
            {
                // Handle exceptions here
            }
        }

        private void OnSensorActivatedEvent( EventArgs arg )
        {
            SensorActivatedEventHandler handler = SensorActivatedEvent;
            try
            {
                if( handler != null )
                {
                    handler( this, arg );
                }
            }
            catch
            {
                // Handle exceptions here
            }
        }

        private void OnSensorInitializedEvent(EventArgs arg)
        {
            SensorInitializedEventHandler handler = SensorInitializedEvent;
            try
            {
                if (handler != null)
                {
                    handler(this, arg);
                }
            }
            catch
            {
                // Handle exceptions here
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
                        throw;
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
    }
}
