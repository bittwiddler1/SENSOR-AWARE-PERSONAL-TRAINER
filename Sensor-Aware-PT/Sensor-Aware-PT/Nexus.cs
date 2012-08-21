using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using OpenTK;
namespace Sensor_Aware_PT
{
    /** Holds ID and MAC of each sensor */
    public class SensorConfigData
    {
        public string Id;
        public string Mac;
        public string PortName;
        public SensorConfigData(string _id, string _mac)
        {
            Id = _id;
            Mac = _mac;
            PortName = "";
        }
    }

    /** Struct to hold info on each line of sensor data */
    public struct SensorDataEntry
    {
        public Vector3 orientation;
        public DateTime timeStamp;
        public int sequenceNumber;
    }

    class Nexus
    {
        /** baud rate for the com ports */
        public const int SENSOR_BAUD_RATE = 9600;
        public const double SERIAL_ENUMERATION_TIMEOUT_SECONDS = 5;
        #region instance variables 
        /** Holds the config data for each sensor. Eventually user can use a configurator to set this up
         * though for now it's hardcoded to our sensors */
        List<SensorConfigData> mSensorInfoList;
        /** List to hold the Sensor objects */
        List<Sensor> mSensorList;
        /** Thread for the serial enumeration */
        Thread mSerialPortThread;
        
        #endregion

        public Nexus()
        {
            initializeVariables();
            initializeSensorConfig();
            //initializeSensors();
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
                Sensor sensor = new Sensor(config);   
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
            Logger.Info("Serial port enumeration started");
            string[] ports = SerialPort.GetPortNames();
            Logger.Info("Serial ports are: {0}", ports);
            mSerialPortThread = new Thread(enumeratePort);
            /** Go through each port, attempt to set it up, send handshake, wait for response */
            foreach (string portName in ports)
            {
                Logger.Info("Attempting to enumerate port {0}", portName);
                SerialPort port = new SerialPort(portName, SENSOR_BAUD_RATE);
                /** Open the port, start the enumerate thread */
                port.Open();
                mSerialPortThread.Start(port);
                /** Wait until its complete OR the timeout elapses */
                mSerialPortThread.Join(TimeSpan.FromSeconds(SERIAL_ENUMERATION_TIMEOUT_SECONDS));
                Logger.Info("enumeratePort: match found or timeout occured");
                port.Close();
                port.Dispose();
            }
        }

        /** Sends the handshake command and waits for a reply until timeout. This maps the portname for each sensor
         * the data is the SerialPort which has been opened */
        private void enumeratePort(object data)
        {
            
            SerialPort port = (SerialPort)data;
            Logger.Info("enumeratePort thread started for port {0}", port.PortName);
            string dataLine;
            try
            {
                Logger.Info("enumeratePort: sending identify command");
                /** Send the identify command */
                port.WriteLine("COMMAND_IDENTIFY");
                Logger.Info("enumeratePort: waiting for response");
                /** Wait for the response */
                while (true)
                {
                    dataLine = port.ReadLine();

                    /** Check if the response matches any of our preset macs */
                    foreach(SensorConfigData sensor in mSensorInfoList)
                    {
                        if (dataLine.Equals(sensor.Mac))
                        {
                            /** Match found, save the mapping and return */
                            Logger.Info("enumeratePort: Match found between {0} and {1}", sensor.PortName, sensor.Mac);
                            sensor.PortName = port.PortName;
                            return;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error("enumeratePort: exception  in port {0} {1}", port.PortName, e.Message);
                
            }
        }
    }
}
