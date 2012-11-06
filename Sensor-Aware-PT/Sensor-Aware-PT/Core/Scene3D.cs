using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Sensor_Aware_PT.Core
{
    /// <summary>
    /// Handles all the 3d shit
    /// </summary>
    public class Scene3D
    {
        private Matrix4 mCameraTransform;
        private Vector3 mCameraPosition;
        
        private Vector3 mTargetPosition;
        /// <summary>
        /// Holds the up vector for the world
        /// </summary>
        private Vector3 mWorldNormal;

        public Scene3D( Vector3 camLocation, Vector3 targetLocation, Vector3 upVec )
        {
            mCameraPosition = camLocation;
            mTargetPosition = targetLocation;
            mWorldNormal = upVec;
            mCameraTransform = Matrix4.LookAt( camLocation, targetLocation, upVec );
        }
    }
}
