using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using OpenTK;

namespace Sensor_Aware_PT
{
<<<<<<< HEAD
=======
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



>>>>>>> 4d8b45f692eb6eb7aa7d26a8461d1886227449dc
    /** Created to encapsulate a sensor */
    class Sensor
    {
        /** Timeout in ms for IO */
        static int SERIAL_IO_TIMEOUT = 10000;
        /** Max number of values to keep in history */

        static int HISTORY_BUFFER_SIZE = 250;

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
            ReadingInput
        };

        private SensorState mSensorState = SensorState.Uninitialized; /** Default state for the sensor */

        public Sensor(SensorIdentification config)
        {
            mID = config.Id;
            mMAC = config.Mac;
            mPortName = config.PortName;
            mData = new RingBuffer<SensorDataEntry>(HISTORY_BUFFER_SIZE);
            
        }

        /** Initializes the sensor using the ID and MAC */
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
                /** Start the read thread */
                mReadThread.Start();
            }
            catch( Exception e)
            {
                Logger.Error( e.Message );
            }
  
        }

        /** Background thread that loops infinitely for incoming data */
        private void readThreadRun()
        {
            /** Setup the serial port */
            mSerialPort = new SerialPort( mPortName, Nexus.SENSOR_BAUD_RATE );
            mSerialPort.ReadTimeout = SERIAL_IO_TIMEOUT;
            mSerialPort.WriteTimeout = SERIAL_IO_TIMEOUT;

            mSerialPort.Open();
            changeState( SensorState.Initialized );
            Logger.Info("Sensor {0} read thread started", mID);
            if (mSensorState == SensorState.Initialized)
            {
                changeState(SensorState.ReadingInput);
                try
                {
                    while (true)
                    {
                        string dataLine = mSerialPort.ReadLine();
                        Logger.Info( "Sensor {0} data {1}", mID, dataLine );
                    }
                }
                catch (Exception e)
                {
                    mReadErrors++;
                    if( MAX_READ_ERRORS <= mReadErrors )
                    {
                        Logger.Error( "Sensor {0} read thread exception: {1}", mID, e.Message );
                        throw new Exception( String.Format( "Sensor {0} read thread exception: {1}", mID, e.Message ) );
                    }
                    else
                    {
                    }
 
                }
            }
            else
            {
                throw new Exception("Tried to read data before initialized");
            }
        }

        /** Self explanatory.*/
        private void changeState(SensorState newState)
        {
            Logger.Info("Sensor {0} changing state from {1} to {2}", mID, mSensorState, newState);
            mSensorState = newState;
        }

<<<<<<< HEAD
        /** Takes an input line and spits a vector 3 out */
        public static Vector3 parseInput(string input)
        {
            Vector3 retVal = new Vector3();
            //TODO actual parsing shit
            return retVal;
=======
        /** Takes an input line and determines what to do with it */
        public void parseInput(string input)
        {
            try
            {
                if( input.StartsWith( "#" ) )
                {


                    /** First get the vector of data, provided we have a complete line */
                    Vector3 vecData = parseDataLine( input );
                    /** Also get the previous entry */
                    SensorDataEntry prevEntry = mData.Last();
                    /** New frame check */
                    if( input.StartsWith( "#YPR" ) )
                    {
                        /** since its a new frame, print the previous frame */
                        printDataEntry( prevEntry );
                        /** New frame, add to buffer */
                        mData.Add( prepareEntry( vecData ) );
                        return;
                    }
                    /** Get the last added frame */
                    
                    /** Then determine which property to update */
                    if( input.StartsWith( "#ACC" ) )
                    {
                        prevEntry.accelerometer = vecData;
                        return;
                    }
                    else if( input.StartsWith( "#MAG" ) )
                    {
                        prevEntry.magnetometer = vecData;
                        return;
                    }
                    else if( input.StartsWith( "#GYR" ) )
                    {
                        prevEntry.gyroscope = vecData;
                        /** This also happens to be the end of this frame */
                    }
                }
            }
            catch( Exception e)
            {

                Logger.Error( "Sensor {0} string parsing exception: {1}", mID, e.Message );
            }
           
        }

        /** Prints a data entry to the log */
        private void printDataEntry( SensorDataEntry dataEntry )
        {
            try
            {
                /*
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
                */

                string output = String.Format( "Angle{{{0},{1},{2}}}",
dataEntry.orientation.X,
dataEntry.orientation.Y,
dataEntry.orientation.Z );

                Logger.Info( "Sensor {0}: {1}", mID, output );
            }
            catch( Exception e)
            {

                Logger.Error( "Sensor {0} string printing exception: {1}", mID, e.Message );
            }
            
        }

        /** Parses a line of data from the sensor */
        private Vector3 parseDataLine( String dataLine )
        {

            dataLine = dataLine.Substring( 6 ); /** Gets rid of the ##YPR or ##GYR or ##ACC*/
            String[] values = dataLine.Split( ',' ); /** Split on comma */
            Vector3 vecData = new Vector3();
            vecData.X = float.Parse( values[ 0 ] );
            vecData.Y = float.Parse( values[ 1 ] );
            vecData.Z = float.Parse( values[ 2 ] );
            return vecData;
>>>>>>> 4d8b45f692eb6eb7aa7d26a8461d1886227449dc
        }

        /** Packs an orientation into a data entry struct along with sequence and timestamp */
        private SensorDataEntry prepareEntry(Vector3 orientation)
        {
            SensorDataEntry newEntry = new SensorDataEntry();
            newEntry.orientation = orientation;
            newEntry.sequenceNumber = mSequenceNum++;
            newEntry.timeStamp = DateTime.Now;

            return newEntry;
        }

        public void reset()
        {

        }

        internal SensorDataEntry getEntry()
        {
            /** This is a crime, yes I know oh gods I know */
            if( mData.Count > 0 )
                return mData.Last();
            else
                return new SensorDataEntry();
        }
    }
}
