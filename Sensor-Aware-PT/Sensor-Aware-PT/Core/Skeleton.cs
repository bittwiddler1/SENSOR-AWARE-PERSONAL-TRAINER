using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using System.Drawing;

namespace Sensor_Aware_PT
{
    /// <summary>
    /// Defines different bone types on a body
    /// </summary>
    [Serializable()]
    public enum BoneType
    {
        None,
        ArmLowerL, ArmLowerR,
        ArmUpperL, ArmUpperR,
        
        LegLowerL, LegLowerR,
        LegUpperL, LegUpperR,

        BackLower, BackUpper,
        Neck,
        ShoulderL, ShoulderR,
        HipL, HipR,
        Head,
        
    }

    /// <summary>
    /// Defines the different skeletal types available
    /// </summary>
    public enum SkeletonType
    {
        UpperBody,
        LowerBody,
        MidBody,
        Dog,
        TRex
    }

    /// <summary>
    /// Models a simple skeleton
    /// </summary>
    public class Skeleton : IObserver<SensorDataEntry>
    {
        /// <summary>
        /// Holds all the <c>Bone</c>s in this Skeleton
        /// </summary>
        protected List<Bone> mBones = new List<Bone>();

        /// <summary>
        /// Maps a sensor ID to a specific bone
        /// </summary>
        protected Dictionary<String, Bone> mSensorBoneMapping = new Dictionary<String, Bone>();

        /// <summary>
        /// Maps a bone type to it's length
        /// </summary>
        protected static Dictionary<BoneType, float> mBoneLengthMapping = new Dictionary<BoneType, float>();

        /// <summary>
        /// Maps a bonetype to a specific bone 
        /// </summary>
        protected Dictionary<BoneType, Bone> mBoneTypeMapping = new Dictionary<BoneType, Bone>();
        
        /// <summary>
        /// Used to keep track of the fixed parent bone, which varies between skeleton types
        /// </summary>
        protected Bone mParentBone;

        /// <summary>
        /// These are default orientation values for bones
        /// </summary>
        /// 
        internal  static Matrix4 ORIENT_LEFT;
        internal static Matrix4 ORIENT_RIGHT;
        internal static Matrix4 ORIENT_UP;
        internal static Matrix4 ORIENT_DOWN;
        internal static Matrix4 ORIENT_FRONT;
        internal static Matrix4 ORIENT_DEFAULT = Matrix4.Identity;
        internal static Vector3 VECTOR_ORIENT_RIGHT = new Vector3( 90, 0, 0 );
        //internal static Vector3 VECTOR_ORIENT_LEFT = new Vector3(-90,0, 0 );
        internal static Vector3 VECTOR_ORIENT_LEFT = new Vector3( 90, 90, 0 );
        internal static Vector3 VECTOR_ORIENT_DOWN = new Vector3( 0, 180, 0 );
        internal static Vector3 VECTOR_ORIENT_UP = new Vector3( 0, 0, 0);
        internal static Vector3 VECTOR_ORIENT_FRONT = new Vector3( 0, 90, 0 );

        //straight down -180, -90, -180
        //straight up -90, 90, 90
        //LEFT -90, 0, 180
        //RIGHT 90, 0, -180

        private static bool mInitialized = false;

        public Skeleton()
        {
            if( !mInitialized )
            {
                initializeBoneLengths();
                initializeOrientations();
                mInitialized = true;
            }
        }

        /// <summary>
        /// Sets up the orientations we use by default for bones
        /// </summary>
        private static void initializeOrientations()
        {
            /** First convert the degrees to radians */
            VECTOR_ORIENT_LEFT.X = MathHelper.DegreesToRadians( VECTOR_ORIENT_LEFT.X );
            VECTOR_ORIENT_LEFT.Y = MathHelper.DegreesToRadians( VECTOR_ORIENT_LEFT.Y );
            VECTOR_ORIENT_LEFT.Z = MathHelper.DegreesToRadians( VECTOR_ORIENT_LEFT.Z );

            VECTOR_ORIENT_RIGHT.X = MathHelper.DegreesToRadians( VECTOR_ORIENT_RIGHT.X );
            VECTOR_ORIENT_RIGHT.Y = MathHelper.DegreesToRadians( VECTOR_ORIENT_RIGHT.Y );
            VECTOR_ORIENT_RIGHT.Z = MathHelper.DegreesToRadians( VECTOR_ORIENT_RIGHT.Z );

            VECTOR_ORIENT_UP.X = MathHelper.DegreesToRadians( VECTOR_ORIENT_UP.X );
            VECTOR_ORIENT_UP.Y = MathHelper.DegreesToRadians( VECTOR_ORIENT_UP.Y );
            VECTOR_ORIENT_UP.Z = MathHelper.DegreesToRadians( VECTOR_ORIENT_UP.Z );

            VECTOR_ORIENT_DOWN.X = MathHelper.DegreesToRadians( VECTOR_ORIENT_DOWN.X );
            VECTOR_ORIENT_DOWN.Y = MathHelper.DegreesToRadians( VECTOR_ORIENT_DOWN.Y );
            VECTOR_ORIENT_DOWN.Z = MathHelper.DegreesToRadians( VECTOR_ORIENT_DOWN.Z );

            /** Build the orientation matrixes */

            ORIENT_LEFT = Matrix4.Identity;
            ORIENT_LEFT =   ORIENT_LEFT * 
                            Matrix4.CreateRotationX( VECTOR_ORIENT_LEFT.X ) * 
                            Matrix4.CreateRotationY( VECTOR_ORIENT_LEFT.Y ) *
                            Matrix4.CreateRotationZ( VECTOR_ORIENT_LEFT.Z );

            ORIENT_RIGHT = Matrix4.Identity;
            ORIENT_RIGHT = ORIENT_RIGHT *
                            Matrix4.CreateRotationX( VECTOR_ORIENT_RIGHT.X ) *
                            Matrix4.CreateRotationY( VECTOR_ORIENT_RIGHT.Y ) *
                            Matrix4.CreateRotationZ( VECTOR_ORIENT_RIGHT.Z );

            ORIENT_UP = Matrix4.Identity;
            ORIENT_UP = ORIENT_UP *
                            Matrix4.CreateRotationX( VECTOR_ORIENT_UP.X ) *
                            Matrix4.CreateRotationY( VECTOR_ORIENT_UP.Y ) *
                            Matrix4.CreateRotationZ( VECTOR_ORIENT_UP.Z );

            ORIENT_DOWN = Matrix4.Identity;
            ORIENT_DOWN = ORIENT_DOWN *
                            Matrix4.CreateRotationX( VECTOR_ORIENT_DOWN.X ) *
                            Matrix4.CreateRotationY( VECTOR_ORIENT_DOWN.Y ) *
                            Matrix4.CreateRotationZ( VECTOR_ORIENT_DOWN.Z );
            ORIENT_FRONT = Matrix4.Identity;

        }
        /// <summary>
        /// Sets up the default bonetype to bone length mappings
        /// </summary>
        private static void initializeBoneLengths()
        {
            mBoneLengthMapping.Add( BoneType.BackUpper, 4f );
            mBoneLengthMapping.Add( BoneType.BackLower, 3f );
            mBoneLengthMapping.Add( BoneType.ArmLowerL, 2f );
            mBoneLengthMapping.Add( BoneType.ArmLowerR, 2f );
            mBoneLengthMapping.Add( BoneType.ArmUpperL, 2.1f );
            mBoneLengthMapping.Add( BoneType.ArmUpperR, 2.1f);
            mBoneLengthMapping.Add( BoneType.LegLowerL, 2.2f );
            mBoneLengthMapping.Add( BoneType.LegLowerR, 2.2f );
            mBoneLengthMapping.Add( BoneType.LegUpperL, 2.5f );
            mBoneLengthMapping.Add( BoneType.LegUpperR, 2.5f );
            mBoneLengthMapping.Add( BoneType.ShoulderL, 1.5f );
            mBoneLengthMapping.Add( BoneType.ShoulderR, 1.5f );
            mBoneLengthMapping.Add( BoneType.HipL, 1f );
            mBoneLengthMapping.Add( BoneType.HipR, 1f );
            mBoneLengthMapping.Add( BoneType.Head, 1f );
        }

        /// <summary>
        /// Creates a new skeletal system
        /// </summary>
        /// <param name="skelType">The skeletal system type to create</param>
        public Skeleton(SkeletonType skelType) : this()
        {
            switch (skelType)
            {
                case SkeletonType.UpperBody:
                    createUpperBody();
                    break;
                case SkeletonType.LowerBody:
                    createLowerBody();
                    break;
                case SkeletonType.MidBody:
                    createMidBody();
                    break;
                case SkeletonType.Dog:
                    break;
                case SkeletonType.TRex:
                    break;
                default:
                    break;
            }
        }

        private void createMidBody()
        {
            throw new NotImplementedException();
        }

        private void createLowerBody()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates the upper body skeletal system using the back as a fixed reference point
        /// </summary>
        private void createUpperBody()
        {
            Bone BackU, BackL, ArmUL, ArmUR, ArmLL, ArmLR, ShoulderL, ShoulderR, HipL, HipR, Head,
                LegLL, LegLR, LegUL, LegUR, FakeHip;
            BackU = new Bone( mBoneLengthMapping[ BoneType.BackUpper ] );
            BackL = new Bone( mBoneLengthMapping[ BoneType.BackLower ] );
            ArmUL = new Bone(mBoneLengthMapping[BoneType.ArmUpperL]);
            ArmUR = new Bone(mBoneLengthMapping[BoneType.ArmUpperR]);
            ArmLL = new Bone(mBoneLengthMapping[BoneType.ArmLowerL]);
            ArmLR = new Bone(mBoneLengthMapping[BoneType.ArmLowerR]);
            ShoulderL = new Bone(mBoneLengthMapping[BoneType.ShoulderL]);
            ShoulderR = new Bone(mBoneLengthMapping[BoneType.ShoulderR]);
            HipR = new Bone( mBoneLengthMapping[ BoneType.HipR] );
            HipL= new Bone( mBoneLengthMapping[ BoneType.HipL] );
            Head = new Bone( mBoneLengthMapping[ BoneType.Head] );
            LegLL = new Bone( mBoneLengthMapping[ BoneType.LegLowerL] );
            LegLR = new Bone( mBoneLengthMapping[ BoneType.LegLowerR ] );
            LegUR = new Bone( mBoneLengthMapping[ BoneType.LegUpperR ] );
            LegUL = new Bone( mBoneLengthMapping[ BoneType.LegUpperL ] );
            FakeHip = new Bone( 1f );
            
            mBoneTypeMapping.Add( BoneType.BackUpper, BackU );
            mBoneTypeMapping.Add( BoneType.ArmUpperL, ArmUL);
            mBoneTypeMapping.Add( BoneType.ArmLowerL, ArmLL);
            mBoneTypeMapping.Add( BoneType.ArmUpperR, ArmUR );
            mBoneTypeMapping.Add( BoneType.ArmLowerR, ArmLR );
            mBoneTypeMapping.Add( BoneType.ShoulderL, ShoulderL);
            mBoneTypeMapping.Add( BoneType.ShoulderR, ShoulderR );
            mBoneTypeMapping.Add( BoneType.BackLower, BackL );
            mBoneTypeMapping.Add( BoneType.HipL, HipL );
            mBoneTypeMapping.Add( BoneType.HipR, HipR );
            mBoneTypeMapping.Add( BoneType.Head, Head );
            mBoneTypeMapping.Add( BoneType.LegLowerL, LegLL);
            mBoneTypeMapping.Add( BoneType.LegLowerR, LegLR );
            mBoneTypeMapping.Add( BoneType.LegUpperL, LegUL );
            mBoneTypeMapping.Add( BoneType.LegUpperR, LegUR );

            // Set the orientations accordingly~
            
            /** Back */
            //BackU.InitialOrientation = ORIENT_UP;
            BackU.setOrientation( BoneOrientation.Up );


            BackU.addChild( ShoulderL );
            BackU.addChild(ShoulderR);
            BackU.Color = Color.Green;
            BackU.Thickness = .5f;
            /** Shoulders */
            //ShoulderR.InitialOrientation = ORIENT_RIGHT;
            ShoulderL.setOrientation( BoneOrientation.Left );
            //ShoulderL.InitialOrientation = ORIENT_LEFT;
            ShoulderR.setOrientation( BoneOrientation.Right );
            ShoulderL.Color = Color.Chartreuse;
            ShoulderR.Color = Color.Gold;
            /** Left Arm upper */
            ShoulderL.addChild( ArmUL );
            //ArmUL.InitialOrientation = ORIENT_DOWN;
            ArmUL.setOrientation( BoneOrientation.Down );
            ArmUL.Color = Color.OrangeRed;

            /** Right arm upper */
            ShoulderR.addChild( ArmUR );
            //ArmUR.InitialOrientation = ORIENT_DOWN;
            ArmUR.setOrientation( BoneOrientation.Down );
            ArmUR.Color = Color.OrangeRed;
            /** Left arm lower */
            ArmUL.addChild( ArmLL );
            //ArmLL.InitialOrientation = ORIENT_DOWN;
            ArmLL.setOrientation( BoneOrientation.Down );
            ArmLL.Color = Color.OrangeRed;
            /** Right arm lower */
            ArmUR.addChild( ArmLR );
            //ArmLR.InitialOrientation = ORIENT_DOWN;
            ArmLR.setOrientation( BoneOrientation.Down );
            ArmLR.Color = Color.OrangeRed;

            /** Hip and lower legs */
            //HipR.InitialOrientation = ORIENT_RIGHT;
            //HipL.InitialOrientation = ORIENT_LEFT;
            HipR.setOrientation( BoneOrientation.Right );
            HipL.setOrientation( BoneOrientation.Left );


            HipL.Color = Color.Blue;
            HipR.Color = Color.Gray;
            //LegUL.InitialOrientation = ORIENT_DOWN;
            LegUL.setOrientation( BoneOrientation.Down );
            HipL.addChild( LegUL );

            //LegUR.InitialOrientation = ORIENT_DOWN;
            LegUR.setOrientation( BoneOrientation.Down );
            LegUR.Color = LegLR.Color = LegLL.Color = LegUL.Color = Color.DarkRed;

            HipR.addChild( LegUR );

            //LegLL.InitialOrientation = ORIENT_DOWN;
            LegLL.setOrientation( BoneOrientation.Down );
            LegUL.addChild( LegLL );

            //LegLR.InitialOrientation = ORIENT_DOWN;
            LegLR.setOrientation( BoneOrientation.Down );
            LegUR.addChild( LegLR );

            //FakeHip.InitialOrientation = ORIENT_UP;
            FakeHip.setOrientation( BoneOrientation.Up );

            FakeHip.addChild( HipL );
            FakeHip.addChild( HipR );
            FakeHip.addChild( BackU );

            mParentBone = FakeHip;
            mParentBone.DrawingEnabled = false;
            mParentBone.updateOrientation();

        }
        /// <summary>
        /// Draws the entire skeletal structure
        /// </summary>
        public void draw()
        {
            /** Each child bone will automatically be drawn by it's parent so we only need to call draw on the parent bone of the structure */
            mParentBone.drawBone();
            
        }

        /// <summary>
        /// Creates a mapping between a sensor ID and a specific boneType
        /// </summary>
        /// <param name="sensorID">Sensor ID to map ("A","B",etc)</param>
        /// <param name="mapping">BoneType to map to</param>
        public void createMapping( string sensorID, BoneType mapping )
        {
            Bone mappedBone;
            if(mBoneTypeMapping.TryGetValue(mapping, out mappedBone))
            {
                /** Bonetype->bone mapping exists, create the sensor-> bone map */
                mSensorBoneMapping.Add( sensorID, mappedBone );
            }
            else
            {
                /** Bone mapping doesn't exist, error */
                throw new Exception("The requested BoneType has no mapping for this skeleton.");
            }
        }

        #region IObserver<SensorDataEntry> Members

        void IObserver<SensorDataEntry>.OnCompleted()
        {
            throw new NotImplementedException();
        }

        void IObserver<SensorDataEntry>.OnError( Exception error )
        {
            throw new NotImplementedException();
        }

        void IObserver<SensorDataEntry>.OnNext( SensorDataEntry value )
        {
            Bone mappedBone;
            if( mSensorBoneMapping.TryGetValue( value.id, out mappedBone ) )
            {
                mappedBone.updateOrientation( value.orientation );
            }
            else
            {
                //?
            }
        }

        public void calibrateZero()
        {
            Dictionary<BoneType, Matrix4> calibratedValues = new Dictionary<BoneType, Matrix4>();
            foreach( Bone b in mSensorBoneMapping.Values )
            {
                Matrix4 calibData = b.calibrateZero();
                KeyValuePair<BoneType, Bone> boneKvp = mBoneTypeMapping.Where( bone => bone.Value == b ).First();
                calibratedValues.Add( boneKvp.Key, calibData );
            }

            Nexus.CalibratedOrientations = calibratedValues;
        }

        #endregion

        internal void toggleBox()
        {
            mParentBone.DrawBoundingBox = !mParentBone.DrawBoundingBox;
        }

        internal void toggleWireframe()
        {
            mParentBone.DrawWireFrame = !mParentBone.DrawWireFrame;
        }

        internal void calibrateZero( Dictionary<BoneType, Matrix4> mCalibrationData )
        {
            foreach( KeyValuePair<BoneType, Matrix4> kvp in mCalibrationData )
            {
                mBoneTypeMapping[ kvp.Key ].calibrateZero( kvp.Value );
            }
        }
    }
}
