using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Sensor_Aware_PT
{
    public enum BoneType
    {
        ArmLowerL, ArmLowerR,
        ArmUpperL, ArmUpperR,
        
        LegLowerL, LegLowerR,
        LegUpperL, LegUpperR,

        BackLower, BackUpper,
        Neck,
        ShoulderL, ShoulderR
    }

    public enum SkeletonType
    {
        UpperBody,
        LowerBody,
        MidBody,
        Dog,
        TRex
    }


    public class Skeleton : IObserver<SensorDataEntry>
    {
        protected List<Bone> mBones = new List<Bone>();
        /** Maps a sensor ID to a specific bone */
        protected Dictionary<String, Bone> mSensorBoneMapping = new Dictionary<String, Bone>();
        /** Maps a bone type to it's length */
        protected Dictionary<BoneType, float> mBoneLengthMapping = new Dictionary<BoneType, float>();
        /** Maps a bonetype to a specific bone */
        protected Dictionary<BoneType, Bone> mBoneTypeMapping = new Dictionary<BoneType, Bone>();
        protected Bone mParentBone;

        public Skeleton()
        {
            // Default bone length mappings
            mBoneLengthMapping.Add(BoneType.BackUpper, 3f);
            mBoneLengthMapping.Add(BoneType.BackLower, 3f);
            mBoneLengthMapping.Add(BoneType.ArmLowerL, 2f);
            mBoneLengthMapping.Add(BoneType.ArmLowerR, 2f);
            mBoneLengthMapping.Add(BoneType.ArmUpperL, 2f);
            mBoneLengthMapping.Add(BoneType.ArmUpperR, 2f);
            mBoneLengthMapping.Add(BoneType.LegLowerL, 2.5f);
            mBoneLengthMapping.Add(BoneType.LegLowerR, 2.5f);
            mBoneLengthMapping.Add(BoneType.LegUpperL, 2.5f);
            mBoneLengthMapping.Add(BoneType.LegUpperR, 2.5f);
            mBoneLengthMapping.Add(BoneType.ShoulderL, 1.5f);
            mBoneLengthMapping.Add(BoneType.ShoulderR, 1.5f);
        }

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

        private void createUpperBody()
        {
            Bone Back, ArmUL, ArmUR, ArmLL, ArmLR, ShoulderL, ShoulderR;
                //straight down -180, -90, -180

//straight up -90, 90, 90
            //LEFT -90, 0, 180
//RIGHT 90, 0, -180
            Back = new Bone( mBoneLengthMapping[ BoneType.BackUpper ], new Vector3() );
            ArmUL = new Bone(mBoneLengthMapping[BoneType.ArmUpperL], new Vector3());
            ArmUR = new Bone(mBoneLengthMapping[BoneType.ArmUpperR], new Vector3());
            ArmLL = new Bone(mBoneLengthMapping[BoneType.ArmLowerL], new Vector3());
            ArmLR = new Bone(mBoneLengthMapping[BoneType.ArmLowerR], new Vector3());
            ShoulderL = new Bone(mBoneLengthMapping[BoneType.ShoulderL], new Vector3());
            ShoulderR = new Bone(mBoneLengthMapping[BoneType.ShoulderR], new Vector3());

            mBoneTypeMapping.Add( BoneType.BackUpper, Back );
            mBoneTypeMapping.Add( BoneType.ArmUpperL, ArmUL);
            mBoneTypeMapping.Add( BoneType.ArmLowerL, ArmLL);
            mBoneTypeMapping.Add( BoneType.ArmUpperR, ArmUR );
            mBoneTypeMapping.Add( BoneType.ArmLowerR, ArmLR );
            mBoneTypeMapping.Add( BoneType.ShoulderL, ShoulderL);
            mBoneTypeMapping.Add( BoneType.ShoulderR, ShoulderR );

            // Set the orientations accordingly~
            
            /** Back */
            Back.updateOrientation( new Vector3( -90, 90f, 90 ) );
            Back.addChild(ShoulderL);
            Back.addChild(ShoulderR);
            /** Shoulders */
            ShoulderR.updateOrientation(new Vector3(90f, 0f, -180f));
            ShoulderL.updateOrientation(new Vector3(-90f, 0, 180f));
            /** Left Arm upper */
            ShoulderL.addChild(ArmUL);
            ArmUL.updateOrientation(new Vector3());
            /** Right arm upper */
            ShoulderR.addChild(ArmUR);
            ArmUR.updateOrientation(new Vector3());
            /** Left arm lower */
            ArmUL.addChild(ArmLL);
            ArmLL.updateOrientation(new Vector3());
            /** Right arm lower */
            ArmUR.addChild(ArmLR);
            ArmLR.updateOrientation(new Vector3());


            mParentBone = Back;
            

        }
        public void draw()
        {
            mParentBone.drawBone();
        }


        public void update( SensorDataEntry data )
        {

        }
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

        #endregion
    }
}
