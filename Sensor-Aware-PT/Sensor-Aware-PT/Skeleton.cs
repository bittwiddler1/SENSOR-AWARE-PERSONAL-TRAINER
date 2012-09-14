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
            ArmUL = new Bone(mBoneLengthMapping[BoneType.ArmUpperL], new Vector3());
            ArmUR = new Bone(mBoneLengthMapping[BoneType.ArmUpperR], new Vector3());
            ArmLL = new Bone(mBoneLengthMapping[BoneType.ArmLowerL], new Vector3());
            ArmLR = new Bone(mBoneLengthMapping[BoneType.ArmLowerR], new Vector3());
            ShoulderL = new Bone(mBoneLengthMapping[BoneType.ShoulderL], new Vector3());
            ShoulderR = new Bone(mBoneLengthMapping[BoneType.ShoulderR], new Vector3());

            // Set the orientations accordingly~
            
            /** Back */
            Back.addChild(ShoulderL);
            Back.addChild(ShoulderR);
            /** Shoulders */
            ShoulderR.updateOrientation(new Vector3());
            ShoulderL.updateOrientation(new Vector3());
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
