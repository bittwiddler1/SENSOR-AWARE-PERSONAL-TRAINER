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
    public enum BoneType
    {
        ArmLowerL, ArmLowerR,
        ArmUpperL, ArmUpperR,
        
        LegLowerL, LegLowerR,
        LegUpperL, LegUpperR,

        BackLower, BackUpper,
        Neck,
        ShoulderL, ShoulderR,
        HipL, HipR,
        Head
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
        protected static Vector3 ORIENT_LEFT = new Vector3( -90, 0, 180 );
        protected static Vector3 ORIENT_RIGHT = new Vector3( 90, 0, -180 );
        protected static Vector3 ORIENT_DOWN = new Vector3( -180, -90, -180 );
        protected static Vector3 ORIENT_UP = new Vector3( -90, 90f, 90 );
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
                mInitialized = true;
            }
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
                LegLL, LegLR, LegUL, LegUR;
            BackU = new Bone( mBoneLengthMapping[ BoneType.BackUpper ], new Vector3() );
            BackL = new Bone( mBoneLengthMapping[ BoneType.BackLower ], new Vector3() );
            ArmUL = new Bone(mBoneLengthMapping[BoneType.ArmUpperL], new Vector3());
            ArmUR = new Bone(mBoneLengthMapping[BoneType.ArmUpperR], new Vector3());
            ArmLL = new Bone(mBoneLengthMapping[BoneType.ArmLowerL], new Vector3());
            ArmLR = new Bone(mBoneLengthMapping[BoneType.ArmLowerR], new Vector3());
            ShoulderL = new Bone(mBoneLengthMapping[BoneType.ShoulderL], new Vector3());
            ShoulderR = new Bone(mBoneLengthMapping[BoneType.ShoulderR], new Vector3());
            HipR = new Bone( mBoneLengthMapping[ BoneType.HipR], new Vector3() );
            HipL= new Bone( mBoneLengthMapping[ BoneType.HipL], new Vector3() );
            Head = new Bone( mBoneLengthMapping[ BoneType.Head], new Vector3() );
            LegLL = new Bone( mBoneLengthMapping[ BoneType.LegLowerL], new Vector3() );
            LegLR = new Bone( mBoneLengthMapping[ BoneType.LegLowerR ], new Vector3() );
            LegUR = new Bone( mBoneLengthMapping[ BoneType.LegUpperR ], new Vector3() );
            LegUL = new Bone( mBoneLengthMapping[ BoneType.LegUpperL ], new Vector3() );

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

            // Set the orientations accordingly~
            
            /** Back */
            BackU.updateOrientation( ORIENT_UP);
            BackU.addChild(ShoulderL);
            BackU.addChild(ShoulderR);
            BackU.Color = Color.Green;
            BackU.Thickness = .5f;
            /** Shoulders */
            ShoulderR.updateOrientation(ORIENT_RIGHT);
            ShoulderL.updateOrientation(ORIENT_LEFT);
            ShoulderL.Color = Color.Gold;
            ShoulderR.Color = Color.Gold;
            /** Left Arm upper */
            ShoulderL.addChild(ArmUL);
            ArmUL.updateOrientation(ORIENT_DOWN);
            ArmUL.Color = Color.OrangeRed;
            
            /** Right arm upper */
            ShoulderR.addChild(ArmUR);
            ArmUR.updateOrientation( ORIENT_DOWN );
            ArmUR.Color = Color.OrangeRed;
            /** Left arm lower */
            ArmUL.addChild(ArmLL);
            ArmLL.updateOrientation( ORIENT_DOWN );
            ArmLL.Color = Color.OrangeRed;
            /** Right arm lower */
            ArmUR.addChild(ArmLR);
            ArmLR.updateOrientation( ORIENT_DOWN );
            ArmLR.Color = Color.OrangeRed;

            /** Hip and lower legs */
            HipR.updateOrientation( ORIENT_RIGHT );
            HipL.updateOrientation( ORIENT_LEFT );
            HipL.addChild( HipR );
            HipL.addChild( BackU );

            HipL.Color = Color.Blue;
            HipR.Color = Color.Blue;

            HipL.addChild( LegUL );
            LegUL.updateOrientation( ORIENT_DOWN );

            HipR.addChild( LegUR );
            LegUR.updateOrientation( ORIENT_DOWN );
            LegUR.Color = LegLR.Color = LegLL.Color = LegUL.Color = Color.DarkRed;

            LegUL.addChild( LegLL );
            LegLL.updateOrientation( ORIENT_DOWN );

            LegUR.addChild( LegLR );
            LegLR.updateOrientation( ORIENT_DOWN );

            mParentBone = HipL;
            

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

        public void setYawOffsets()
        {
            foreach( Bone b in mSensorBoneMapping.Values )
            {
                b.setYawOffset();
            }
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
    }
}
