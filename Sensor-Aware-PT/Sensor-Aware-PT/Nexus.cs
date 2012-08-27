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
        public SensorConfigData(string _id, string _mac, string port = "")
        {
            Id = _id;
            Mac = _mac;
            PortName = port;
        }
    }

    class Nexus
    {
        /** baud rate for the com ports */
        public const int SENSOR_BAUD_RATE = 57600;
        public const double SERIAL_ENUMERATION_TIMEOUT_SECONDS = 5;
        #region instance variables
        /** Holds the config data for each sensor. Eventually user can use a configurator to set this up
         * though for now it's hardcoded to our sensors */
        List<SensorConfigData> mSensorInfoList;
        /** List to hold the Sensor objects */
        List<Sensor> mSensorList;
        

        #endregion

        public Nexus()
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
            foreach( SensorConfigData config in mSensorInfoList )
            {
                Sensor sensor = new Sensor( config );
                mSensorList.Add( sensor );
            }
        }

        /** Set up the sensor configuration data, sensor ID and mac */
        private void initializeSensorConfig()
        {
            //mSensorInfoList.Add(new SensorConfigData("A", "EC21"));
            //mSensorInfoList.Add( new SensorConfigData( "B", "EC21", "COM12" ) );
            mSensorInfoList.Add( new SensorConfigData( "C", "EC21", "COM8" ) );
            //mSensorInfoList.Add( new SensorConfigData( "D", "EC21", "COM18" ) );
        }

        /** Goes through every available serial port, opens a connection, sends a handshake, waits for a response and terminates
         * This allows us to grab the serial ports we identify as our sensors */
        public void enumerateSerialPorts()
        {
            Logger.Info( "Serial port enumeration started" );
            string[] ports = SerialPort.GetPortNames();
            Logger.Info( "Serial ports are: {0}", ports );
            /** Go through each port, attempt to set it up, send handshake, wait for response */
            foreach( string portName in ports )
            {
                Logger.Info( "Attempting to enumerate port {0}", portName );
                SerialPort port = new SerialPort( portName, SENSOR_BAUD_RATE );
                /** enumerate here? */
            }
        }

        public SensorDataEntry getEntry( int index )
        {
            return mSensorList[ index ].getLastEntry();
        }
    }
}
