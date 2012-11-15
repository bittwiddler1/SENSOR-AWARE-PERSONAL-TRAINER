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
        List<Vector3> mPoints;
        private const int MAX_POINTS = 250;
        private Vector2 mDimensions;
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
            mPoints = new List<Vector3>();
            for( float i = 0; i < MAX_POINTS; i++ )
            {
                mPoints.Add( new Vector3() );
            }

            mDimensions = new Vector2( width, height );
        }

        public void draw()
        {
            GL.Begin( BeginMode.LineStrip );
            GL.Color3( Color.Red );
            for( int i = 0; i < mPoints.Count; i++ )
            {
                GL.Vertex2( ( float ) (float)i*(WindowDimensions.X/(float)MAX_POINTS), mPoints[ i ].X );
            }
            GL.End();

            GL.Begin( BeginMode.LineStrip );
            GL.Color3( Color.Green );
            for( int i = 0; i < mPoints.Count; i++ )
            {
                GL.Vertex2( ( float ) ( float ) i * ( WindowDimensions.X / ( float ) MAX_POINTS ), mPoints[ i ].Y );
            }
            GL.End();

            GL.Begin( BeginMode.LineStrip );
            GL.Color3( Color.Blue );
            for( int i = 0; i < mPoints.Count; i++ )
            {
                GL.Vertex2( ( float ) ( float ) i * ( WindowDimensions.X / ( float ) MAX_POINTS ), mPoints[ i ].Z );
            }
            GL.End();
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
            if( value.id == "D" )
            {
                mPoints.Add( value.accelerometer );
                mPoints.RemoveAt( 0 );
            }
        }

        #endregion
    }
}
