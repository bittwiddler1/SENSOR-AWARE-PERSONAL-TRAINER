using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;


namespace Sensor_Aware_PT
{
    /** Created to encapsulate a sensor */
    class Sensor
    {
        /** Timeout in ms for IO */
        static int SERIAL_IO_TIMEOUT = 500;
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
            mSerialPort = new SerialPort(mPortName, SensorManager.SENSOR_BAUD_RATE);
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
            if (mSensorState == SensorState.Initialized)
            {
                changeState(SensorState.ReadingInput);
                while (true)
                {
                    string dataLine = mSerialPort.ReadLine();
                }
            }
            else
            {
                throw new Exception("Tried to read data before initialized");
            }
        }

        private void changeState(SensorState newState)
        {
            mSensorState = newState;
        }
    }
}
