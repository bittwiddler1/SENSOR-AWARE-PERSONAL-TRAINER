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
        Neck
    }

    public abstract class Skeleton
    {
        private List<Bone> mBones = new List<Bone>();
        private Dictionary<String, Bone> mBoneMapping = new Dictionary<String, Bone>();
        
        public abstract void draw();
        public virtual void update( SensorDataEntry data )
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
        public abstract void createMapping( string sensorID, BoneType mapping );
    }
}
