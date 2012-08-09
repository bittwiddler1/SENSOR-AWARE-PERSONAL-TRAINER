using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace Sensor_Aware_PT
{
    /** Holds ID and MAC of each sensor */
    struct SensorConfigData
    {
        public string Id;
        public string Mac;
        public SensorConfigData(string _id, string _mac)
        {
            Id = _id;
            Mac = _mac;
        }
    }

    class SensorManager
    {
        /** baud rate for the com ports */
        const int SENSOR_BAUD_RATE = 9600;
        const double SERIAL_ENUMERATION_TIMEOUT_SECONDS = 5;
        #region instance variables 
        /** Holds the config data for each sensor. Eventually user can use a configurator to set this up
         * though for now it's hardcoded to our sensors */
        List<SensorConfigData> mSensorInfoList;
        /** List to hold the Sensor objects */
        List<Sensor> mSensorList;
        /** Thread for the serial enumeration */
        Thread mSerialPortThread;
        
        #endregion

        public SensorManager()
        {
            initializeVariables();
            initializeSensorConfig();
            initializeSensors();
        }

        /** Initialize all the class variables...duh */
        private void initializeVariables()
        {
            mSensorInfoList = new List<SensorConfigData>();
            mSensorList = new List<Sensor>();
        }

        /** Creates the Sensor objects based on the SensorConfigData and tries to find them */
        private void initializeSensors()
        {
            foreach (SensorConfigData config in mSensorInfoList)
            {
                Sensor sensor = new Sensor(config.Id, config.Mac);
                
            }
        }

        /** Set up the sensor configuration data, sensor ID and mac */
        private void initializeSensorConfig()
        {
            mSensorInfoList.Add(new SensorConfigData("A", "EC21"));
            mSensorInfoList.Add(new SensorConfigData("B", "EC21"));
            mSensorInfoList.Add(new SensorConfigData("C", "EC21"));
            mSensorInfoList.Add(new SensorConfigData("D", "EC21"));
        }
        
        /** Goes through every available serial port, opens a connection, sends a handshake, waits for a response and terminates
         * This allows us to grab the serial ports we identify as our sensors */
        public void enumerateSerialPorts()
        {
            string[] ports = SerialPort.GetPortNames();
            mSerialPortThread = new Thread(enumeratePort);
            /** Go through each port, attempt to set it up, send handshake, wait for response */
            foreach (string portName in ports)
            {
                SerialPort port = new SerialPort(portName, SENSOR_BAUD_RATE);
                /** Open the port, start the enumerate thread */
                mSerialPortThread.Start(port);
                /** Wait until its complete OR the timeout elapses */
                mSerialPortThread.Join(TimeSpan.FromSeconds(SERIAL_ENUMERATION_TIMEOUT_SECONDS));
            }
        }

        /** Called after setting up a serial port and opening it */
        private void enumeratePort(object data)
        {
            SerialPort port = (SerialPort)data;
            string dataLine;
            port.Open();
            while (true)
            {
                dataLine = port.ReadLine();

            }
        }
    }
}
