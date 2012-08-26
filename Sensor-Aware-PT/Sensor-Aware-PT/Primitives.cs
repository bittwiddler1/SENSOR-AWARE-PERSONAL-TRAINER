using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;
using System.Drawing;


namespace Sensor_Aware_PT
{
    static class Primitives
    {
        public static void mofDrawLine( float x1, float y1, float x2, float y2 )
        {
            GL.Begin( BeginMode.Lines );
            GL.Vertex2( x1, y1 );
            GL.Vertex2( x2, y2 );
            GL.End();
        }

        public static void mofDrawCircleFilled( float radius, float x, float y )
        {
            GL.Begin( BeginMode.TriangleFan );
            GL.Vertex2( x, y );
            
            for( float i = 0; i < 2 * Math.PI; i += (float)Math.PI / 180.0f )
            {
                GL.Vertex2( x + Math.Sin( i ) * radius, y + Math.Cos( i ) * radius );
            }
            GL.End();
        }

        public static void mofDrawCircle( float radius, float x, float y )
        {
            GL.Begin( BeginMode.LineLoop );
            //GL.Vertex2(x, y);
            for( float i = 0; i < 2 * Math.PI; i += (float)Math.PI / 180.0f )
            {
                GL.Vertex2( x + Math.Sin( i ) * radius, y + Math.Cos( i ) * radius );
            }
            GL.End();

        }

        public static void mofDrawCircleAgain( float r, float cx, float cy )
        {
            int num_segments = (int)( 10.0f * Math.Sqrt( r ) );
            float theta = 2f *3.1415926f / ( float ) ( num_segments );
            float c = (float)Math.Cos( theta );//precalculate the sine and Math.Cosine
            float s = (float)Math.Sin( theta );
            float t;

            float x = r;//we start at anGL.e = 0 
            float y = 0;



            GL.Begin( BeginMode.LineLoop );
            for( int ii = 0; ii < num_segments; ii++ )
            {
                GL.Vertex2( x + cx, y + cy );//output vertex 

                //apply the rotation matrix
                t = x;
                x = c * x - s * y;
                y = s * t + c * y;
            }
            GL.End();
            GL.Disable( EnableCap.LineSmooth);
        }

        public static void mofDrawCircleAgainFilled( float r, float cx, float cy )
        {
            int num_segments = (int)( 10.0f * Math.Sqrt( r ) );
            float theta = 2f * 3.1415926f / ( float ) ( num_segments );
            float c = (float)Math.Cos( theta );//precalculate the sine and Math.Cosine
            float s = (float)Math.Sin( theta );
            float t;

            float x = r;//we start at anGL.e = 0 
            float y = 0;

            GL.Enable( EnableCap.PolygonSmooth );
            GL.Hint( HintTarget.PolygonSmoothHint, HintMode.Nicest);

            GL.Begin( BeginMode.TriangleFan );
            GL.Vertex2( cx, cy );
            for( int ii = 0; ii < num_segments + 1; ii++ )
            {
                GL.Vertex2( x + cx, y + cy );//output vertex 

                //apply the rotation matrix
                t = x;
                x = c * x - s * y;
                y = s * t + c * y;
            }

            GL.End();
            GL.Disable( EnableCap.PolygonSmooth);
        }

        public static void mofDrawTriangle( float x1, float y1, float x2, float y2, float x3, float y3 )
        {

            GL.Enable( EnableCap.PolygonSmooth );
            GL.Hint( HintTarget.PolygonSmoothHint, HintMode.Nicest );

            GL.Begin(BeginMode.Triangles);
            GL.Vertex2( x1, y1 );
            GL.Vertex2( x2, y2 );
            GL.Vertex2( x3, y3 );
            GL.End();
            GL.Disable( EnableCap.PolygonSmooth );
        }

        /* x1 and x2 are a unit vector pointing towards wherever */
        public static void mofDrawArrow( float cx, float cy, float x1, float x2 )
        {
            mofDrawLine( cx, cy, x1, x2 );
            //draw triangular arrowhead
            //epic fail
        }

        public static void mofDrawBox(float x, float y, float width, float height)
        {

        }

        public static  void DrawCube(float x, float y, float z)
        {
            GL.Begin( BeginMode.Quads );

            GL.Color3( Color.Silver );
            GL.Vertex3( -1.0f*x, -1.0f * y, -1.0f * z );
            GL.Vertex3( -1.0f * x, 1.0f * y, -1.0f * z );
            GL.Vertex3( 1.0f * x, 1.0f * y, -1.0f * z );
            GL.Vertex3( 1.0f * x, -1.0f * y, -1.0f * z );

            GL.Color3( Color.Honeydew );
            GL.Vertex3( -1.0f * x, -1.0f * y, -1.0f * z );
            GL.Vertex3( 1.0f * x, -1.0f * y, -1.0f * z );
            GL.Vertex3( 1.0f * x, -1.0f * y, 1.0f * z );
            GL.Vertex3( -1.0f * x, -1.0f * y, 1.0f * z );

            GL.Color3( Color.Moccasin );

            GL.Vertex3( -1.0f * x, -1.0f * y, -1.0f * z );
            GL.Vertex3( -1.0f * x, -1.0f * y, 1.0f * z );
            GL.Vertex3( -1.0f * x, 1.0f * y, 1.0f * z );
            GL.Vertex3( -1.0f * x, 1.0f * y, -1.0f * z );

            GL.Color3( Color.IndianRed );
            GL.Vertex3( -1.0f * x, -1.0f * y, 1.0f * z );
            GL.Vertex3( 1.0f * x, -1.0f * y, 1.0f * z );
            GL.Vertex3( 1.0f * x, 1.0f * y, 1.0f * z );
            GL.Vertex3( -1.0f * x, 1.0f * y, 1.0f * z );

            GL.Color3( Color.PaleVioletRed );
            GL.Vertex3( -1.0f * x, 1.0f * y, -1.0f * z);
            GL.Vertex3( -1.0f * x, 1.0f * y, 1.0f * z );
            GL.Vertex3( 1.0f * x, 1.0f * y, 1.0f * z );
            GL.Vertex3( 1.0f * x, 1.0f * y, -1.0f * z );

            GL.Color3( Color.ForestGreen );
            GL.Vertex3( 1.0f * x, -1.0f * y, -1.0f * z );
            GL.Vertex3( 1.0f * x, 1.0f * y, -1.0f * z );
            GL.Vertex3( 1.0f * x, 1.0f * y, 1.0f * z );
            GL.Vertex3( 1.0f * x, -1.0f * y, 1.0f * z );

            GL.End();
        }

    }
}
