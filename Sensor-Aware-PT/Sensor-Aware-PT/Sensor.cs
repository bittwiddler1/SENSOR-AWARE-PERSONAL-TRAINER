using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using OpenTK;

namespace Sensor_Aware_PT
{
    /** Created to encapsulate a sensor */
    class Sensor
    {
        /** Timeout in ms for IO */
        static int SERIAL_IO_TIMEOUT = 500;
        /** Max number of values to keep in history */
        static int HISTORY_BUFFER_SIZE = 500;
        /** Friendly sensor ID A-D */
        private string mID;

        public string Id
        {
            get { return mID; }
            set { mID = value; }
        }
        /** Last 4 Digits of mac address, used for ID purposes */
        private string mMAC;

        public string MacAddress
        {
            get { return mMAC; }
            set { mMAC = value; }
        }
        /** COM port of sensor */
        private string mPortName;

        public string SerialPortName
        {
            get { return mPortName; }
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

        public Sensor(SensorConfigData config)
        {
            mID = config.Id;
            mMAC = config.Mac;
            mPortName = config.PortName;
            mData = new RingBuffer<SensorDataEntry>(HISTORY_BUFFER_SIZE);
            initialize();
        }

        /** Initializes the sensor using the ID and MAC */
        private void initialize()
        {
            /** Setup the reading thread */
            mReadThread = new Thread(readThreadRun);
            /** Format for name of each read thread is
             * readThreadRunA, readThreadRunB...etc */
            mReadThread.Name = String.Format("readThreadRun{0}", mID);
            mReadThread.IsBackground = true;

            /** Setup the serial port */
            mSerialPort = new SerialPort(mPortName, Nexus.SENSOR_BAUD_RATE);
            mSerialPort.ReadTimeout = SERIAL_IO_TIMEOUT;
            mSerialPort.WriteTimeout = SERIAL_IO_TIMEOUT;
            mSerialPort.Open();
            changeState(SensorState.Initialized);
            /** Start the read thread */
            mReadThread.Start();
        }

        /** Background thread that loops infinitely for incoming data */
        private void readThreadRun()
        {
            Logger.Info("Sensor {0} read thread started", mID);
            if (mSensorState == SensorState.Initialized)
            {
                changeState(SensorState.ReadingInput);
                try
                {
                    while (true)
                    {
                        string dataLine = mSerialPort.ReadLine();
                    }
                }
                catch (Exception e)
                {
                    Logger.Error("Sensor {0} read thread exception: {1}", mID, e.Message);
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

        /** Takes an input line and spits a vector 3 out */
        public static Vector3 parseInput(string input)
        {
            Vector3 retVal = new Vector3();
            //TODO actual parsing shit
            return retVal;
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
    }
}
