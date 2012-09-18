using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using OpenTK.Graphics;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
namespace Sensor_Aware_PT
{
    public class Bone
    {
        Vector3 mStartPoint;
        Vector3 mEndPoint;
        Vector3 mOrientation;
        Vector3 mOrigin;
        Vector3[] mBoxVerts;
        Vector3[] mSphereVerts;
        protected Matrix4 mTransform;
        Vector3 mOffset;
        private Color mColor = Color.Gold;
        private IntPtr mConeQuadric;
        private float mThickness = .2f;
        private float mDrawRatio = .3f; // Upper = .3f, lower = 1-.3f
        private static bool mDrawBox = false;
        private static bool mDrawWireframe = false;

        public bool DrawWireFrame
        {
            get
            {
                return mDrawWireframe;
            }
            set
            {
                mDrawWireframe = value;
            }
        }

        public bool DrawBoundingBox
        {
            get{return mDrawBox;}
            set
            {
                mDrawBox = value;
            }
        }
        public float Thickness
        {
            get
            {
                return mThickness;
            }
            set
            {
                mThickness = value;
            }
        }

        public Color Color
        {
            get
            {
                return mColor;
            }
            set
            {
                mColor = value;
            }
        }
        
        List<Bone> mChildren;
        protected Bone mParentBone;
        float mLength;

        public Bone( float length, Vector3 startPt )
        {
            mLength = length;
            mStartPoint = startPt;
            // just add in the X direction
            startPt.X += length;

            mEndPoint = new Vector3( startPt );

            mOrientation = new Vector3(0,0,0);
            mOrigin = new Vector3();

            mTransform = Matrix4.Identity;
            mChildren = new List<Bone>();
            mParentBone = null;
            mOffset = new Vector3( 0, 0, 0 );
            generateGeometry( mLength, 1f, 1f, mOrigin.X, mOrigin.Y, mOrigin.Z );

        }

        /// <summary>
        /// Called to set the current yaw to be the offset. This is useful when trying to align the sensors to the screen.
        /// </summary>
        public void setYawOffset()
        {
            mOffset = mOrientation;
        }

        public void updateUsingLastOrientation()
        {
            updateOrientation( mOrientation );
        }
        public void updateOrientation( Vector3 or )
        {
            
            mOrientation = or;
            //XYZ = YPR
            //roll, yaw, then pitch
            Matrix4 rx = Matrix4.CreateRotationX( MathHelper.DegreesToRadians( (or.Z + 90f) - mOffset.Z ) );
            Matrix4 ry = Matrix4.CreateRotationZ( MathHelper.DegreesToRadians(  or.Y - mOffset.Y ) );
            Matrix4 rz = Matrix4.CreateRotationY( MathHelper.DegreesToRadians( mOffset.X - (or.X +90f) ));
            mTransform = Matrix4.Identity;
            mTransform = rx * ry * rz;

            if( mParentBone != null )
            {
                /** Pretend everything is aligned to the origin just like in the case when thsi is a non-child */
                mStartPoint.X = 0;
                mStartPoint.Y = 0;
                mStartPoint.Z = 0;

                mEndPoint.X = mStartPoint.X + mLength;
                mEndPoint.Y = mStartPoint.Y;
                mEndPoint.Z = mStartPoint.Z;


                /** Then apply the rotation followed by the translation to the endpt of the parent
                 * to both my start pt and endpt
                 */
                mTransform *= Matrix4.CreateTranslation( mParentBone.mEndPoint );
                mEndPoint = Vector3.Transform( mEndPoint, mTransform );
                mStartPoint = Vector3.Transform( mStartPoint, mTransform );

            }
            else
            {

                mEndPoint.X = mStartPoint.X;
                mEndPoint.Y = mStartPoint.Y;
                mEndPoint.Z = mStartPoint.Z;
                mEndPoint.X += mLength;
                mEndPoint = Vector3.Transform( mEndPoint, mTransform );
            }

            foreach( Bone child in mChildren )
            {
                child.updateUsingLastOrientation();
            }
           
            /*
            Logger.Info( "Start {0}, {1}, {2}", mStartPoint.X, mStartPoint.Y, mStartPoint.Z );
            Logger.Info( "End {0}, {1}, {2}", mEndPoint.X, mEndPoint.Y, mEndPoint.Z );
             */
        }


        Vector3[] createsphere( float R, float H, float K, float Z )
        {
            int n;
            int a;
            int b;
            int space = 20;
            List<Vector3> vertices = new List<Vector3>();
            float PI = ( float ) Math.PI;

            n = 0;
            for( b = 0; b <= 180 - space; b += space )
            {
                for( a = 0; a <= 360 - space; a += space )
                {
                    Vector3 v1 = new Vector3();
                    v1.X = R * ( ( float ) Math.Sin( ( float ) ( a * PI ) / 180 ) ) * ( ( float ) Math.Sin( ( float ) ( b * PI ) / 180 ) ) - H;
                    v1.Z = R * ( ( float ) Math.Cos( ( float ) ( b * PI ) / 180 ) ) * ( ( float ) Math.Sin( ( float ) ( a * PI ) / 180 ) ) - Z;
                    v1.Y = R * ( ( float ) Math.Cos( ( float ) ( a * PI ) / 180 ) ) - K;
                    vertices.Add( v1 );
                    n++;

                    Vector3 v2 = new Vector3();
                    v2.X = R * ( ( float ) Math.Sin( ( float ) ( a * PI ) / 180 ) ) * ( ( float ) Math.Sin( ( float ) ( ( b + space ) * PI ) / 180 ) ) - H;
                    v2.Z = R * ( ( float ) Math.Cos( ( float ) ( ( b + space ) * PI ) / 180 ) ) * ( ( float ) Math.Sin( ( float ) ( a * PI ) / 180 ) ) - Z;
                    v2.Y= R * ( ( float ) Math.Cos( ( float ) ( a * PI ) / 180 ) ) - K;
                    vertices.Add( v2 );
                    n++;
                    
                }
            }

            
            return vertices.ToArray();
        }

        public void addChild(Bone child)
        {
            if( mChildren.Contains( child ) )
                throw new Exception( "You can't add that child, it's already contained, sweet child of mine~" );
            else
            {
                mChildren.Add( child );
                child.mParentBone = this;
            }
        }

        private void generateGeometry( float length, float width, float height, float x, float y, float z )
        {
            /** We assume that everything starts at 0,0,0 */
            float k = .5f;
            Vector3[] verts = new Vector3[ 8 ];

            verts[ 0 ] = new Vector3( 0, height, -width ) * k;
            verts[ 1 ] = new Vector3( 0, height, width ) * k;
            verts[ 2 ] = new Vector3( 0, -height, -width ) * k;
            verts[ 3 ] = new Vector3( 0, -height, width )* k ;
            verts[ 4 ] = new Vector3( 2f*length, height, -width ) * k ;
            verts[ 5 ] = new Vector3( 2f*length, height, width ) * k ;
            verts[ 6 ] = new Vector3( 2f*length, -height, -width ) * k;
            verts[ 7 ] = new Vector3( 2f*length, -height, width ) * k;
            mBoxVerts = verts;
            mSphereVerts = createsphere( .5f, 0, 0, 0 );
        }


        public void drawBone()
        {
            /** Copy the camera transform for our own use */
            GL.PushMatrix();
            
            /** Draw wireframe or not....HOPE YOU LIKE THE INLINE */
            GL.PolygonMode( MaterialFace.FrontAndBack, mDrawWireframe ? PolygonMode.Line : PolygonMode.Fill );
            //GL.PolygonMode( MaterialFace.FrontAndBack, PolygonMode.Fill );
            /** Mult with our own self contained transform matrix */
            GL.MultMatrix( ref mTransform );

            /** At this point, all following draw calls will begin with the origin translated and rotated where it needs to be
             * so we can draw in model-space
            /** Draw the sphere */
            /*
            GL.Begin( BeginMode.TriangleStrip );
            GL.Color3( Color.LightBlue );
            foreach( Vector3 v in mSphereVerts )
            {
                GL.Vertex3( v );
            }
            GL.End();
             */
            /*

            GL.Translate( new Vector3( mLength * .2f, 0, 0 ) );
            drawCone( new Vector3(1f,0,0), new Vector3(mLength*.8f, 0, 0), mLength*.8f, .2f, 24 );
            GL.Translate( new Vector3( -mLength * .2f, 0, 0 ) );
            drawCone( new Vector3( -1f, 0, 0 ), new Vector3( 0, 0, 0 ), mLength * .2f, .2f, 24 );
            */
            
            GL.ColorMaterial( MaterialFace.FrontAndBack, ColorMaterialParameter.Specular );
            GL.Material( MaterialFace.Front, MaterialParameter.Specular, .1f );
            
            GL.ColorMaterial( MaterialFace.FrontAndBack, ColorMaterialParameter.AmbientAndDiffuse );
            GL.Enable( EnableCap.ColorMaterial );

            GL.PushMatrix();
            GL.Rotate( 90f, 0, 1.0, 0 );
            mConeQuadric = OpenTK.Graphics.Glu.NewQuadric();

            OpenTK.Graphics.Glu.QuadricDrawStyle( mConeQuadric, OpenTK.Graphics.QuadricDrawStyle.Fill );
            OpenTK.Graphics.Glu.QuadricNormal( mConeQuadric, OpenTK.Graphics.QuadricNormal.Smooth );
            GL.Color3( mColor);
            OpenTK.Graphics.Glu.Cylinder( mConeQuadric, 0.0, mThickness, mLength* mDrawRatio, 12, 12 );
            GL.PopMatrix();
            
            GL.PushMatrix();

            GL.Translate( new Vector3( mLength , 0, 0 ) );
            GL.Rotate( 90f, 0, -1.0, 0 );
            OpenTK.Graphics.Glu.Cylinder( mConeQuadric, 0.0, mThickness, mLength * (1f-mDrawRatio), 12, 12 );
            GL.PopMatrix();

            OpenTK.Graphics.Glu.DeleteQuadric( mConeQuadric );

            if( mDrawBox )
            {
                GL.PolygonMode( MaterialFace.FrontAndBack, PolygonMode.Line );
                /** Draw the box */
                GL.Begin( BeginMode.Quads );
                //    Gl.Color3( Color.Red);
                GL.Color3( Color.Red );
                GL.Vertex3( mBoxVerts[ 0 ] );
                GL.Vertex3( mBoxVerts[ 4 ] );
                GL.Vertex3( mBoxVerts[ 6 ] );
                GL.Vertex3( mBoxVerts[ 2 ] );
                GL.Color3( Color.Green );
                //Gl.Color3( Color.Black );
                GL.Vertex3( mBoxVerts[ 0 ] );
                GL.Vertex3( mBoxVerts[ 1 ] );
                GL.Vertex3( mBoxVerts[ 5 ] );
                GL.Vertex3( mBoxVerts[ 4 ] );
                GL.Color3( Color.Black );
                //Gl.Color3( Color.Green );
                GL.Vertex3( mBoxVerts[ 1 ] );
                GL.Vertex3( mBoxVerts[ 5 ] );
                GL.Vertex3( mBoxVerts[ 7 ] );
                GL.Vertex3( mBoxVerts[ 3 ] );
                GL.Color3( Color.Blue );
                //Gl.Color3( Color.Blue );
                GL.Vertex3( mBoxVerts[ 3 ] );
                GL.Vertex3( mBoxVerts[ 7 ] );
                GL.Vertex3( mBoxVerts[ 6 ] );
                GL.Vertex3( mBoxVerts[ 2 ] );
                GL.Color3( Color.Orange );
                GL.Vertex3( mBoxVerts[ 0 ] );
                GL.Vertex3( mBoxVerts[ 1 ] );
                GL.Vertex3( mBoxVerts[ 3 ] );
                GL.Vertex3( mBoxVerts[ 2 ] );
                GL.Color3( Color.Brown );
                GL.Vertex3( mBoxVerts[ 4 ] );
                GL.Vertex3( mBoxVerts[ 5 ] );
                GL.Vertex3( mBoxVerts[ 7 ] );
                GL.Vertex3( mBoxVerts[ 6 ] );
                GL.End();
            }

            /** By popping here, we remove our orientation+translation and reset the matrix to world space */
            GL.PopMatrix();
            
            /** Draw the line segment for this bone, in world-space coordinates */
            GL.Begin( BeginMode.Lines );
            GL.Color3( Color.Black );
            GL.Vertex3( mStartPoint );
            GL.Vertex3( mEndPoint );
            GL.End();

            /** Now render each child bone */
            foreach( Bone child in mChildren )
            {
                child.drawBone();
            }
        }

        ~Bone()
        {
            
        }


    }

 
}
