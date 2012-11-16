using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;

namespace Sensor_Aware_PT
{
    public class AccelerometerGrapher : IDrawable, IObserver<SensorDataEntry>
    {
        Dictionary<String, List<Vector3>> mPoints;
        private const int MAX_POINTS = 250;
        private Vector2 mDimensions;
        private float mMaxHeight = 500f;
        public Vector2 WindowDimensions
        {
            get
            {
                return mDimensions;
            }
            set
            {
                mDimensions = value;
            }
        }

        public AccelerometerGrapher(float width, float height)
        {
            mPoints = new Dictionary<String,List<Vector3>>();
            var activeSensors = Nexus.Instance.getActivatedSensors();


            foreach( var s in activeSensors )
            {
                mPoints.Add( s.Id, new List<Vector3>() );
                for( int i = 0; i < MAX_POINTS; i++ )
                    mPoints[ s.Id ].Add( new Vector3() );
            }

            mDimensions = new Vector2( width, height/(float)(mPoints.Count+1) );
            
        }

        public void setDimensions( Vector2 newDim )
        {
            mDimensions = new Vector2( newDim.X, newDim.Y/ ( float ) ( mPoints.Count + 1 ) );
        }

        public void draw()
        {
            float offset = 0;
            float scale = ( mMaxHeight * 2f ) / mDimensions.Y;
            foreach( var kvp in mPoints )
            {
                offset+= mDimensions.Y;
                GL.Begin( BeginMode.LineStrip );
                GL.Color3( Color.Red );
                for( int i = 0; i < kvp.Value.Count; i++ )
                {
                    GL.Vertex2( ( float ) ( float ) i * ( WindowDimensions.X / ( float ) MAX_POINTS ), (kvp.Value[ i ].X/scale) + offset );
                }
                GL.End();

                GL.Begin( BeginMode.LineStrip );
                GL.Color3( Color.Green );
                for( int i = 0; i < kvp.Value.Count; i++ )
                {
                    GL.Vertex2(  ( float ) i * ( WindowDimensions.X / ( float ) MAX_POINTS ), (kvp.Value[ i ].Y/scale)  + offset );
                }
                GL.End();

                GL.Begin( BeginMode.LineStrip );
                GL.Color3( Color.Blue );
                for( int i = 0; i < kvp.Value.Count; i++ )
                {
                    GL.Vertex2(  ( float ) i * ( WindowDimensions.X / ( float ) MAX_POINTS ), (kvp.Value[ i ].Z/scale)  + offset );
                }
                GL.End();
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
            List<Vector3> point;
            if( mPoints.TryGetValue( value.id, out point ) )
            {
                point.Add( value.accelerometer );
                point.RemoveAt( 0 );
            }
        }

        #endregion
    }
}
