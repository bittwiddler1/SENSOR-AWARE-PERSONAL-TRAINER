using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using QuickFont;

namespace Sensor_Aware_PT
{
    public class AccelerometerGrapher : IDrawable, IObserver<SensorDataEntry>
    {
        Dictionary<String, List<Vector3>> mPoints;
        private const int MAX_POINTS = 250;
        private Vector2 mDimensions;
        private float mMaxHeight = 500f;
        private QFont mLargeFont;
        private QFont mSmallFont;

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

            var config = new QFontBuilderConfiguration()
            {
                UseVertexBuffer = true,
                TextGenerationRenderHint = TextGenerationRenderHint.SystemDefault
            };

            mLargeFont = new QFont( "Core/times.ttf", 16, config );
            mSmallFont = new QFont( "Core/times.ttf", 10, config );
        }

        public void setDimensions( Vector2 newDim )
        {
            mDimensions = new Vector2( newDim.X, newDim.Y/ ( float ) ( mPoints.Count + 1 ) );
            QFont.RefreshViewport();

            float scale = ( mMaxHeight * 2f ) / mDimensions.Y;
            float yOffset = mDimensions.Y;
            mLargeFont.ResetVBOs();
            mSmallFont.ResetVBOs();
            //Prints sensor labels
            foreach( var kvp in mPoints )
            {

                mLargeFont.PrintToVBO( "Sensor " + kvp.Key, new Vector3( 0, yOffset-15f, 0 ), Color.White );
                mSmallFont.PrintToVBO("1g", new Vector3( mDimensions.X-(mDimensions.X/8), (220f/scale)+yOffset, 0 ), Color.WhiteSmoke);
                mSmallFont.PrintToVBO( "2g", new Vector3( mDimensions.X - ( mDimensions.X / 8 ),( 440f / scale ) + yOffset, 0 ), Color.WhiteSmoke );
                mSmallFont.PrintToVBO( "1g", new Vector3( mDimensions.X - ( mDimensions.X / 8 ), yOffset - ( 220f / scale ), 0 ), Color.WhiteSmoke );
                mSmallFont.PrintToVBO( "2g", new Vector3( mDimensions.X - ( mDimensions.X / 8 ), yOffset - ( 440f / scale ), 0 ), Color.WhiteSmoke );
                yOffset += mDimensions.Y;
            }
            mLargeFont.LoadVBOs();
            mSmallFont.LoadVBOs();
        }

        public void draw()
        {
            float offset = 0;
            float scale = ( mMaxHeight * 2f ) / mDimensions.Y;
            
            QFont.Begin();
            GL.Disable( EnableCap.DepthTest );
            GL.PushMatrix();

            mLargeFont.DrawVBOs();
            mSmallFont.DrawVBOs();
            GL.PopMatrix();
            QFont.End();

            GL.Enable( EnableCap.DepthTest );
            GL.Disable( EnableCap.Texture2D );
            

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

                
                GL.Enable( EnableCap.LineStipple );
                GL.LineStipple( 6, Convert.ToInt16( "0011011100110011", 2 ) );
                GL.Begin( BeginMode.Lines );
                    GL.Color3( Color.White );
                    GL.Vertex2( WindowDimensions.X - ( WindowDimensions.X / 8 ), offset - ( 440f / scale ) );
                    GL.Vertex2( WindowDimensions.X, offset - ( 440f / scale ) );
                    GL.Vertex2( WindowDimensions.X - ( WindowDimensions.X / 8 ), offset - ( 220f / scale ) );
                    GL.Vertex2( WindowDimensions.X , offset - ( 220f / scale ) );

                    GL.Vertex2( WindowDimensions.X - ( WindowDimensions.X / 8 ), offset + ( 440f / scale ) );
                    GL.Vertex2( WindowDimensions.X , offset + ( 440f / scale ) );
                    GL.Vertex2( WindowDimensions.X - ( WindowDimensions.X / 8 ), offset + ( 220f / scale ) );
                    GL.Vertex2( WindowDimensions.X , offset + ( 220f / scale ) );      
                GL.End();
                GL.Disable( EnableCap.LineStipple );
                /** Draw the dotted 1g/2g lines */
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
