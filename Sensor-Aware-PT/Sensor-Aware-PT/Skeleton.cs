using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    public class Skeleton
    {
        protected List<Bone> mBones = new List<Bone>();
        protected Dictionary<String, Bone> mBoneMapping = new Dictionary<String, Bone>();
        protected Dictionary<BoneType, float> mBoneLengthMapping = new Dictionary<BoneType, float>();

        public Skeleton()
        {
            // Default bone length mappings
            mBoneLengthMapping.Add(BoneType.BackUpper, 5f);
            mBoneLengthMapping.Add(BoneType.BackLower, 3f);
            mBoneLengthMapping.Add(BoneType.ArmLowerL, 2f);
            mBoneLengthMapping.Add(BoneType.ArmLowerR, 2f);
            mBoneLengthMapping.Add(BoneType.ArmUpperL, 2f);
            mBoneLengthMapping.Add(BoneType.ArmUpperR, 2f);
            mBoneLengthMapping.Add(BoneType.LegLowerL, 2.5f);
            mBoneLengthMapping.Add(BoneType.LegLowerR, 2.5f);
            mBoneLengthMapping.Add(BoneType.LegUpperL, 2.5f);
            mBoneLengthMapping.Add(BoneType.LegUpperR, 2.5f);
            mBoneLengthMapping.Add(BoneType.ShoulderL, 1f);
            mBoneLengthMapping.Add(BoneType.ShoulderR, 1f);
        }

        public Skeleton(SkeletonType skelType)
        {

        }
        public void draw();

        public void update( SensorDataEntry data )
        {
            Bone mappedBone;
            if(mBoneMapping.TryGetValue(data.id, out mappedBone))
            {
                mappedBone.updateOrientation( data.orientation );
            }
            else
            {
                //?
            }
        }
        public  void createMapping( string sensorID, BoneType mapping );
    }
}
