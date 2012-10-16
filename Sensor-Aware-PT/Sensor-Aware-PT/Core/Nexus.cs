using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Management;
using System.Xml.Serialization;


namespace Sensor_Aware_PT
{
    public class Nexus : IObservable<SensorDataEntry>
    {
        #region singleton implementation variables
        private static volatile Nexus mInstance;
        private static object mInstanceLock = new object();
        #endregion
        #region Constants
        /** baud rate for the com ports */
        public const int SENSOR_BAUD_RATE = 57600;
        public const double SERIAL_ENUMERATION_TIMEOUT_SECONDS = 5;
        


        /** Path to config file **/
  
        #endregion

        #region instance variables
        /** List to hold the Sensor objects */
        internal Dictionary<String, Sensor> mSensorDict;
        internal Dictionary<String, SensorIdentification> mSensorIDDict;

        internal Dictionary<String, BoneType> mBoneSensorDict;
        private Sensor[] mAvailableSensors;


        /** This keeps track of the # of ready events we hve received */
        private static int mReadySensorCount = 0;
        private System.DateTime mStartOfInit;

        /** Observable pattern & lock */
        private List<IObserver<SensorDataEntry>> mObservers = new List<IObserver<SensorDataEntry>>();
        private readonly object mObserverLock = new object();

        /** Keeps the # of active sensors */

        private int mActiveSensorCount = 0;
        private static object mFrameLock = new object();

        ConfigurationDialog mConfigurator;

        #endregion

        #region Event handling stuff

        /** Delegate & event for when all sensors are initialized and reading */
        public delegate void InitializationCompleteHandler( object sender, EventArgs e );
        public event InitializationCompleteHandler InitializationComplete;
 
        #endregion

        
        /// <summary>
        /// Default constructor is private since this is a singleton. Use Nexus.Instance instead
        /// </summary>
        private Nexus()
        {
            initializeVariables();
        }

        public static Nexus Instance
        {
            get
            {
                /** Double check locking because the .net guide said to use it */
                if( mInstance == null )
                {
                    lock( mInstanceLock )
                    {
                        if( mInstance == null )
                            mInstance = new Nexus();
                    }
                }
                return mInstance;
            }
        }

        public Dictionary<String, BoneType> BoneMappings
        {
            get
            {
                return mBoneSensorDict;
            }
        }

        

        public void initialize()
        {
            this.mConfigurator = new ConfigurationDialog();
            this.mConfigurator.ShowDialog(); 

            mAvailableSensors = mSensorDict.Values.ToArray();
            foreach (Sensor s in mAvailableSensors)
            {
                /** Register the ready event */
                s.InitializationComplete += new Sensor.InitializationCompleteHandler(Sensor_InitializationCompleteEvent);
                s.ReInitializationComplete += new Sensor.ReInitializationCompleteHandler( Sensor_ReInitializationComplete );
            }

            /** Initialize the first member */
            mAvailableSensors[0].initialize();
            
        }

        #region ObserverPattern
        public IDisposable Subscribe( IObserver<SensorDataEntry> observer )
        {
            lock (mObserverLock)
            {
                if (mObservers.Contains(observer) == false)
                {
                    mObservers.Add(observer);
                }

                return new Unsubscriber(mObservers, observer);
            }
        }

        private class Unsubscriber : IDisposable
        {
            private List<IObserver<SensorDataEntry>> mList;
            private IObserver<SensorDataEntry> mObserver;

            public Unsubscriber( List<IObserver<SensorDataEntry>> list, IObserver<SensorDataEntry> observer )
            {
                this.mList     = list;
                this.mObserver = observer;
            }

            public void Dispose()
            {
                if (this.mObserver != null && this.mList.Contains(mObserver))
                {
                    this.mList.Remove(mObserver);
                }
            }
        }

        public void NotifyObservers( SensorDataEntry dataFrame )
        {
            foreach( IObserver<SensorDataEntry> observer in mObservers )
            {
                if( mInvert )
                {
                    OpenTK.Matrix4 m = dataFrame.orientation;
                    m.Invert();
                    dataFrame.orientation = m;
                }
                observer.OnNext(dataFrame);
            }
        }

        /// <summary>
        /// When a sensor has new data this event gets called.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Sensor_DataReceived( object sender, Sensor.DataReceivedEventArgs e )
        {
            NotifyObservers( e.Data );
        }
        #endregion

        #region Configuration
        //internal void Configure()
        //{
        //    try
        //    {
                
        //        {
        //            String response;
        //            Logger.Info("Config file detected at {0}", this.ConfigFilePath);
        //            Logger.Info("Prompting user to determine whether to use it...");
        //            do
        //            {
        //                Console.WriteLine("Use the detected config file? [Y/N]");
        //                Console.WriteLine("If \"No\", you will be prompted for sensor identification data and a config will be saved for future use.");
        //                response = Console.ReadLine()[0].ToString();

        //                if (response.ToLower() == "y")
        //                {
        //                    Logger.Info("User chose to use existing config file. (User Input = {0})", response.ToLower()[0]);

        //                    if (this.readConfigFile() == true)
        //                    {
        //                        this.bGenerateConfig = false;
        //                    }
        //                }
        //                else if (response.ToLower() == "n")
        //                {
        //                    Logger.Info("User chose not to use existing config file. (User Input = {0})", response.ToLower()[0]);
        //                    this.bGenerateConfig = true;
        //                }
        //                else
        //                {
        //                    Console.WriteLine("Input not understood. Please try again. Enter something starting with y or n");
        //                }
        //            } while (response.ToLower() != "y" && response.ToLower() != "n");
                    
        //        } 
        //        else
        //        {
        //            this.bGenerateConfig = true;
        //        }

        //        if (this.bGenerateConfig)
        //        {
        //            mStartOfInit = System.DateTime.Now;
        //            this.probeWmiComPorts();
        //            this.saveConfigFile();

        //            System.TimeSpan timeSpent = System.DateTime.Now - mStartOfInit;
        //            double timeLeft = 60.0 - (int)timeSpent.TotalSeconds;
        //            if (timeLeft > 0)
        //            {
        //                Logger.Info("Delaying for {0} seconds while sensors initialize...", (int)timeLeft);
        //                Thread.Sleep((int)(timeLeft * 1000));   
        //            }
        //        }
        //        else
        //        {
        //            foreach (SensorIdentification idStruct in mSensorIDDict.Values)
        //            {
        //                /** Create the Sensor with its SensorID **/
        //                mSensorDict[idStruct.Id] = new Sensor(idStruct);
        //            }
        //        }

                
       
        //    catch( Exception e )
        //    {
        //        Logger.Warning( "{0}", e.Message );
        //        bGenerateConfig = true;
        //        Logger.Info( "Manual input will be required to configure the sensor array" );
        //    }
        //}


        /// <summary>
        /// Event fired after a sensor reinitializes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Sensor_ReInitializationComplete( object sender, EventArgs e )
        {
            //throw new NotImplementedException();
        }

        //private void saveConfigFile()
        //{
        //    StreamWriter fileStream;

        //    /** Generate the config file **/
        //    try
        //    {
        //        fileStream = new StreamWriter(ConfigFilePath, false, Encoding.ASCII);
        //    }
        //    catch (DirectoryNotFoundException)
        //    {
        //        Directory.CreateDirectory(Path.Combine(AppDataDirPath, CFG_DIR));
        //        fileStream = new StreamWriter(ConfigFilePath, false, Encoding.ASCII);
        //    }

        //    SensorIdentification[] tmpArray = mSensorIDDict.Values.ToArray();
        //    XmlSerializer serializer = new XmlSerializer(tmpArray.GetType());
        //    serializer.Serialize(fileStream, tmpArray);

        //    //XmlSerializer serializer = new XmlSerializer(mConfigFileDataDict.GetType());
        //    //serializer.Serialize(fileStream, mConfigFileDataDict);
        //    fileStream.Flush();
        //    fileStream.Close();
        //}



        #endregion
        
        #region Initialization and enumeration
        /// <summary>
        ///  Initialize all the member variables
        /// </summary>
        private void initializeVariables()
        {
    
            mSensorDict   = new Dictionary<String,Sensor>();                   // The actual sensor objects
            mSensorIDDict = new Dictionary<String, SensorIdentification>();
            mBoneSensorDict = new Dictionary<String, BoneType>();
        }

        /// <summary>
        /// This event gets raised after each sensor completes its initialize routine. Even if a sensor fails to initialize,
        /// it will fire this event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Sensor_InitializationCompleteEvent( object sender, EventArgs e )
        {
            Logger.Info( "Nexus has recieved ready notification from sensor {0}", ( ( Sensor ) sender ).Id );
            mReadySensorCount++;
            /** Check to see if all sensors have been initialized, if so then reset the count */
            if( mReadySensorCount == mAvailableSensors.Count() )
            {
                Logger.Info( "Nexus has intialized all sensors and is preparing to begin reading" );
                mReadySensorCount = 0;
                int notPresentCount = 0;
                /** Now take our initialized sensors, and start their respective read threads */
                foreach( Sensor s in mAvailableSensors )
                {
                    if( s.IsInitialized )
                    {
                        mReadySensorCount++;
                        s.ActivationComplete += new Sensor.ActivationCompleteHandler( Sensor_ActivationCompleteEvent );
                        s.ReActivationComplete += new Sensor.ReActivationCompleteHandler( Sensor_ReActivationComplete );
                        //Thread.Sleep( 2000 );
                        s.activate();
                    }
                    else
                    {
                        notPresentCount++;
                    }
                }

                if( notPresentCount == mAvailableSensors.Count())
                    OnNexusInitializationComplete();
                
            }
            else
            {
                {
                    Logger.Info( "Initializing sensor list index {0}", mReadySensorCount );
                    mAvailableSensors[ mReadySensorCount ].initialize();
                }
            }


        }

        /// <summary>
        /// Event fired after a sensor reactivates
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Sensor_ReActivationComplete( object sender, EventArgs e )
        {
            //throw new NotImplementedException();
        }

        /// <summary>
        /// This event is fired after each sensor begins its read thread and synchronizes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Sensor_ActivationCompleteEvent( object sender, EventArgs e )
        {
            mReadySensorCount--;
            if( mReadySensorCount == 0 )
            {
                Logger.Info( "Nexus has synchronized and started reading for all initialized sensors and is activated complete" );
                foreach( Sensor s in getActivatedSensors() )
                {
                    s.DataReceived += new Sensor.DataReceivedHandler( Sensor_DataReceived );
                }
                OnNexusInitializationComplete();
            }
        }

        private void OnNexusInitializationComplete()
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


        #endregion

        #region Nexus commands
        /// <summary>
        /// Gets the sensors which are available/reading already
        /// </summary>
        /// <returns>List of active sensors</returns>
        public List<Sensor> getActivatedSensors()
        {
            List<Sensor> activeSensors = new List<Sensor>();
            
            foreach(Sensor s in  mAvailableSensors)
            {
                if( s.IsActivated )
                    activeSensors.Add( s );
            }

            /** Update the active sensor count */
            if( mActiveSensorCount != activeSensors.Count )
                mActiveSensorCount = activeSensors.Count;

            return activeSensors;
        }

        public List<Sensor> getAllSensors()
        {
            return new List<Sensor>( mAvailableSensors );
        }

        internal Sensor GetSensor(string label)
        {
            try
            {

                return mSensorDict[label];
            }
            catch (KeyNotFoundException exc)
            {
                Logger.Warning("Exception- {0}", exc);
                Logger.Warning("One or more sensors is missing from the configuration Dictionary!");
                return null;
            }
        }

        /// <summary>
        /// Resynchronizes all activated sensors.
        /// </summary>
        public void resynchronize()
        {
            List<Sensor> activeSensors = getActivatedSensors();

            foreach( Sensor s in activeSensors )
            {
                s.resynchronize();
            }

          
        }

        /// <summary>
        /// Resets the sequence counters of all activated sensors
        /// </summary>
        private void resetSensorSequence()
        {
            foreach( Sensor s in getActivatedSensors() )
            {
                s.resetSequence();
            }
        }

        #endregion

        internal void fakeData()
        {
            SensorDataEntry sd = new SensorDataEntry();
            sd.orientation = OpenTK.Matrix4.Identity;
            sd.id = "A";
            NotifyObservers( sd );
        }

        bool mInvert = false;
        internal void Invert()
        {
            mInvert = !mInvert;
        }


        /// <summary>
        /// Holds the calibrated orientations of each bonetype
        /// </summary>
        public static Dictionary<BoneType, OpenTK.Matrix4> CalibratedOrientations
        {
            get;
            set;
        }
    }

}
