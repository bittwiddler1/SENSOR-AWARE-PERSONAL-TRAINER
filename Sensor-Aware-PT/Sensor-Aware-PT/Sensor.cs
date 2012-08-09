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

        public Sensor(string id, string mac)
        {
            mID = id;
            mMAC = mac;
            initialize();
        }

        /** Initializes the sensor using the ID and MAC */
        public void initialize()
        {
            /** Setup the reading thread */
            mReadThread = new Thread(readThreadRun);
            /** Format for name of each read thread is
             * readThreadRunA, readThreadRunB...etc */
            mReadThread.Name = String.Format("readThreadRun{0}", mID);
            mReadThread.IsBackground = true;

            /** Setup the serial port */
            setupSerialPort();
        }

        private void setupSerialPort()
        {
            
            throw new NotImplementedException();
        }


        private void readThreadRun()
        {

        }
    }
}
