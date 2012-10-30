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
    public enum BoneOrientation
	{
        Up,
        Down,
        Left,
        Right,
        Front,
        Back
	}
    public class Bone
    {
        Vector3 mStartPoint;
        Vector3 mEndPoint;
        Vector3[] mBoxVerts;
        Vector3[] mInitialBoxVerts;
        BoneOrientation mOrientation;
        private Matrix4 mFinalTransform;
        private Matrix4 mCurrentOrientation;
        private Matrix4 mCalibratedOrientation;
        private Matrix4 mDrawTransform;
        private Color mColor = Color.Gold;
        private IntPtr mConeQuadric;
        private float mThickness = .2f;
        private float mDrawRatio = .3f; // Upper = .3f, lower = 1-.3f
        private static bool mDrawBox = false;
        private static bool mDrawWireframe = false;
        private static bool mDrawLineSegments = true;
        private List<Bone> mChildren;
        protected Bone mParentBone;
        private float mLength;
        private bool mDrawingEnabled = true;
        public Vector3 mYawPitchRoll = new Vector3();
        public Vector3 mCalibYawPitchRoll = new Vector3();
        private BoneType mBoneType;

        #region Properties
        public static bool DrawLineSegments
        {
            get
            {
                return Bone.mDrawLineSegments;
            }
            set
            {
                Bone.mDrawLineSegments = value;
            }
        }
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

        public bool DrawingEnabled
        {
            get
            {
                return mDrawingEnabled;
            }
            set
            {
                mDrawingEnabled = value;
            }
        }

        public Bone ParentBone
        {
            get
            {
                return mParentBone;
            }
        }

        public Matrix4 CurrentOrientation
        {
            get
            {
                return mCurrentOrientation;
            }
        }

        public Matrix4 CurrentZeroOrientation
        {
            get
            {
                return mCalibratedOrientation;
            }
        }

        #endregion

        public Bone( float length, BoneType bonetype )
        {
            mLength = length;
            mStartPoint = Vector3.Zero;
            mEndPoint = Vector3.Zero;
            mChildren = new List<Bone>();
            mParentBone = null;
            mCurrentOrientation = Matrix4.Identity;
            mCalibratedOrientation = Matrix4.Identity;
            mFinalTransform = Matrix4.Identity;
            mBoneType = bonetype;
            generateBoxGeometry();
            

        }

        /// <summary>
        /// Called to set the current yaw to be the offset. This is useful when trying to align the sensors to the screen.
        /// </summary>
        public Matrix4 calibrateZero()
        {
            mCalibratedOrientation =  transformOrientation(mCurrentOrientation);
            mCalibratedOrientation.Invert();
            mCalibYawPitchRoll = mYawPitchRoll;
            return mCalibratedOrientation;
        }

        internal void calibrateZero( Matrix4 calibratedData )
        {
            mCalibratedOrientation = calibratedData;
        }
        /// <summary>
        /// Updates orientation using whatever the last calculated orientation was
        /// </summary>
        public void updateOrientation()
        {
            updateOrientation( mCurrentOrientation, mYawPitchRoll );
        }

        private object mUpdateLock = new object();
        private Matrix4 transformOrientation( Matrix4 or )
        {

            Matrix4 rot = Matrix4.Identity;
            if( or == Matrix4.Identity )
                return or;
            switch( mBoneType )
            {
                case BoneType.ShoulderL:
                    rot = Matrix4.CreateRotationZ( MathHelper.PiOver2 );
                    rot *= Matrix4.CreateRotationY( MathHelper.PiOver2 );
                                rot *= or;
                    return rot;
                    
                case BoneType.ShoulderR:
                    rot = Matrix4.CreateRotationZ( MathHelper.PiOver2 );
                    rot *= Matrix4.CreateRotationY( MathHelper.PiOver2 );
                                rot *= or;
                    return rot;
            }

            return or;
        }

        /// <summary>
        /// Updates orientation with new data
        /// </summary>
        /// <param name="newOrientation">Rotation matrix (DCM) of new orientation values</param>
        /// <param name="ypr"> Yaw Pitch Roll of this orientation</param>
        public void updateOrientation( Matrix4 newOrientation, Vector3 ypr )
        {
            mCurrentOrientation = newOrientation;
            
            /** Save the new orientation as the current, then calculate final transform using our calibrated and new */
            Matrix4 transformedOrientation = transformOrientation( newOrientation );
            /** Calculate yaw pitch roll shit stuff */
            mYawPitchRoll = ypr;
            
            mYawPitchRoll.Y = -1f * ( float ) Math.Asin( transformedOrientation.M31 );
            mYawPitchRoll.Z = ( float ) Math.Atan2( transformedOrientation.M32, transformedOrientation.M33 );
            mYawPitchRoll.X = ( float ) Math.Atan2( transformedOrientation.M21, transformedOrientation.M11 );
            
           //mYawPitchRoll = ypr;

            if( mParentBone != null )
            {
                Matrix4 d = new Matrix4();
                if( (mParentBone.mBoneType == BoneType.ShoulderR) || (mParentBone.mBoneType == BoneType.ShoulderL ))
                {
                    d = Matrix4.CreateRotationY( ( mParentBone.mCalibYawPitchRoll.Y) - mParentBone.mYawPitchRoll.Y);
                    mFinalTransform = d * transformedOrientation * mCalibratedOrientation;
                }
                else
                {
                    d = Matrix4.CreateRotationZ( ( mParentBone.mCalibYawPitchRoll.X ) - mParentBone.mYawPitchRoll.X );
                    mFinalTransform = d * transformedOrientation * mCalibratedOrientation;
                }
                
            }
            else
            {
                mFinalTransform = transformedOrientation * mCalibratedOrientation;
            }  

            if( mParentBone != null )
            {
                /** This is a child bone so it's position depends on the parent
                    * Reset the end points back to the initial positions */
                resetEndPoints();
                /** Then apply the rotation followed by the translation to the endpt of the parent
                    * to both my start pt and endpt
                    */
                mFinalTransform = mFinalTransform * /*mParentBone.mCalibratedOrientation */ Matrix4.CreateTranslation( mParentBone.mEndPoint );
                mEndPoint = Vector3.Transform( mEndPoint, mFinalTransform );
                mStartPoint = Vector3.Transform( mStartPoint, mFinalTransform );
            }
            else
            {
                /**Not a child bone so position does not depend on anything except itself 
                    * Reset the end points back to the initial positions */
                resetEndPoints();
                /** Then apply the rotation, as this has no parent so no translate is required */
                mEndPoint = Vector3.Transform( mEndPoint, mFinalTransform );
            }

            /** Now that our position is finalized, go ahead and update the positions of all children */
            foreach( Bone child in mChildren )
            {
                child.updateOrientation();
            }
        }

        /// <summary>
        /// Resets the start and end point to the initial positions based on initial orientation
        /// </summary>
        private void resetEndPoints()
        {
            mStartPoint = Vector3.Zero;
            mEndPoint = Vector3.Zero;

            switch( mOrientation )
            {
                case BoneOrientation.Up:
                    mEndPoint.Z = mStartPoint.Z + mLength;
                    break;
                case BoneOrientation.Down:
                    mEndPoint.Z = mStartPoint.Z - mLength;
                    break;
                case BoneOrientation.Left:
                    mEndPoint.Y = mStartPoint.Y - mLength;
                    break;
                case BoneOrientation.Right:
                    mEndPoint.Y = mStartPoint.Y + mLength;
                    break;
                case BoneOrientation.Front:
                    mEndPoint.X = mStartPoint.X - mLength;
                    break;
                case BoneOrientation.Back:
                    mEndPoint.X = mStartPoint.X + mLength;
                    break;
                default:
                    break;
            }
            
        }

        public void setOrientationTest()
        {
            mStartPoint = Vector3.Zero;
            mDrawTransform = Matrix4.Identity;
        }

        /// <summary>
        /// Sets the orientation of this bone relative to it's starting position
        /// </summary>
        /// <param name="newOrientation"></param>
        public void setOrientation( BoneOrientation newOrientation )
        {
            //R = X (front/back)
            //G = Y (left/right)
            //B = Z (up/down)
            mOrientation = newOrientation;
            mStartPoint = Vector3.Zero;
            switch( newOrientation )
            {
                case BoneOrientation.Up:
                    mEndPoint.Z = mStartPoint.Z + mLength;
                    mDrawTransform = Skeleton.ORIENT_UP;
                    break;
                case BoneOrientation.Down:
                    mDrawTransform = Skeleton.ORIENT_DOWN;
                    mEndPoint.Z = mStartPoint.Z - mLength;
                    break;
                case BoneOrientation.Left:
                    mEndPoint.Y = mStartPoint.Y - mLength;
                    mDrawTransform = Skeleton.ORIENT_RIGHT;
                    break;
                case BoneOrientation.Right:
                    mEndPoint.Y = mStartPoint.Y + mLength;
                    mDrawTransform = Skeleton.ORIENT_LEFT;
                    break;
                case BoneOrientation.Front:
                    mEndPoint.X = mStartPoint.X - mLength;
                    mDrawTransform = Skeleton.ORIENT_FRONT;
                    break;
                case BoneOrientation.Back:
                    mEndPoint.X = mStartPoint.X + mLength;
                    mDrawTransform = Skeleton.ORIENT_FRONT; //TODO FIX
                    break;
                default:
                    break;
            }

            /** This is to update the vertex positions of the box geometry */
            for( int i = 0; i < mBoxVerts.Count(); i++ )
            {
                mBoxVerts[ i ] = Vector3.Transform( mInitialBoxVerts[ i ], mDrawTransform );
            }
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

        /// <summary>
        /// Adds a bone as a child to this bone
        /// </summary>
        /// <param name="child">The bone to add as a child</param>
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

        /// <summary>
        /// Generates the geometry for the 3d box which will serve as a visualization for the boundaries of this bone
        /// </summary>
        private void generateBoxGeometry()
        {
            /** We assume that everything starts at 0,0,0 */
            float length = mLength;
            float width = 1f;
            float height = 1f;
            float k = .5f;
            Vector3[] verts = new Vector3[ 8 ];

            verts[ 0 ] = new Vector3( -width, height, 0 ) * k;
            verts[ 1 ] = new Vector3( width, height, 0 ) * k;
            verts[ 2 ] = new Vector3( -width, -height, 0 ) * k;
            verts[ 3 ] = new Vector3( width, -height, 0 ) * k;
            verts[ 4 ] = new Vector3( -width, height, 2f * length ) * k;
            verts[ 5 ] = new Vector3( width, height, 2f * length ) * k;
            verts[ 6 ] = new Vector3( -width, -height, 2f * length ) * k;
            verts[ 7 ] = new Vector3( width, -height, 2f * length ) * k;
            mBoxVerts = verts;
            mInitialBoxVerts = verts;
        }

        /// <summary>
        /// Draws this bone using the computed transforms from updateOrientation()
        /// </summary>
        public void drawBone()
        {
            if( mDrawingEnabled )
            {
                /** Copy the camera transform for our own use */
                GL.PushMatrix();
                /** Draw wireframe or not....HOPE YOU LIKE THE INLINE */
                GL.PolygonMode( MaterialFace.FrontAndBack, mDrawWireframe ? PolygonMode.Line : PolygonMode.Fill );
                //GL.PolygonMode( MaterialFace.FrontAndBack, PolygonMode.Fill );
                /** Mult with our own self contained Rot+Translate transform matrix */
                GL.MultMatrix( ref mFinalTransform );

                #region draw cones                
                /** Draws the bone cone pieces using the draw transform that was calculated */
                GL.PushMatrix();
                GL.MultMatrix( ref mDrawTransform );
                //GL.MultMatrix( ref mFinalTransform );
                /** Sets the colors and stuff */
                GL.Color3( Color.Black );
                GL.ColorMaterial( MaterialFace.FrontAndBack, ColorMaterialParameter.Specular );
                GL.Material( MaterialFace.Front, MaterialParameter.Specular, .1f );

                GL.ColorMaterial( MaterialFace.FrontAndBack, ColorMaterialParameter.AmbientAndDiffuse );
                GL.Enable( EnableCap.ColorMaterial );

                mConeQuadric = OpenTK.Graphics.Glu.NewQuadric();
                OpenTK.Graphics.Glu.QuadricDrawStyle( mConeQuadric, OpenTK.Graphics.QuadricDrawStyle.Fill );
                OpenTK.Graphics.Glu.QuadricNormal( mConeQuadric, OpenTK.Graphics.QuadricNormal.Smooth );
                GL.Color3( mColor );
                GL.ColorMaterial( MaterialFace.FrontAndBack, ColorMaterialParameter.AmbientAndDiffuse );
                GL.Enable( EnableCap.ColorMaterial );
                OpenTK.Graphics.Glu.Cylinder( mConeQuadric, 0.0, mThickness, mLength * mDrawRatio, 12, 12 );
                GL.Rotate( 180f, 0, -1.0, 0 );
                GL.Translate( new Vector3( 0, 0, -mLength ) );
                OpenTK.Graphics.Glu.Cylinder( mConeQuadric, 0.0, mThickness, mLength * ( 1f - mDrawRatio ), 12, 12 );
                GL.PopMatrix();
                //GL.MultMatrix( ref mFinalTransform );
                OpenTK.Graphics.Glu.DeleteQuadric( mConeQuadric );
                #endregion
                #region draw bounding box
                if( mDrawBox )
                {
                    GL.PolygonMode( MaterialFace.FrontAndBack, PolygonMode.Line );
                    /** Draw the box */
                    GL.Begin( BeginMode.Quads );
                        GL.Color3( Color.Red );
                            GL.Vertex3( mBoxVerts[ 0 ] );
                            GL.Vertex3( mBoxVerts[ 4 ] );
                            GL.Vertex3( mBoxVerts[ 6 ] );
                            GL.Vertex3( mBoxVerts[ 2 ] );
                        GL.Color3( Color.Green );
                            GL.Vertex3( mBoxVerts[ 0 ] );
                            GL.Vertex3( mBoxVerts[ 1 ] );
                            GL.Vertex3( mBoxVerts[ 5 ] );
                            GL.Vertex3( mBoxVerts[ 4 ] );
                        GL.Color3( Color.Black );
                            GL.Vertex3( mBoxVerts[ 1 ] );
                            GL.Vertex3( mBoxVerts[ 5 ] );
                            GL.Vertex3( mBoxVerts[ 7 ] );
                            GL.Vertex3( mBoxVerts[ 3 ] );
                        GL.Color3( Color.Blue );
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

                    GL.LineWidth( 2f );
                    GL.Begin( BeginMode.Lines );
                    GL.Color3( Color.Red );
                    GL.Vertex3( -1, 0, 0 );
                    GL.Vertex3( 1, 0, 0 );
                    GL.End();

                    GL.Begin( BeginMode.Lines );
                    GL.Color3( Color.Green );
                    GL.Vertex3( 0, -1, 0 );
                    GL.Vertex3( 0, 1, 0 );
                    GL.End();

                    GL.Begin( BeginMode.Lines );
                    GL.Color3( Color.Blue );
                    GL.Vertex3( 0, 0, -1 );
                    GL.Vertex3( 0, 0, 1 );
                    GL.End();
                }
                #endregion
                /** By popping here, we remove our orientation+translation and reset the matrix to world space */
                GL.PopMatrix();

                /** Draw the line segment for this bone, in world-space coordinates */
                if( mDrawLineSegments )
                {
                    GL.LineWidth( 1f );
                    GL.Begin( BeginMode.Lines );
                    GL.Color3( Color.Black );
                    GL.Vertex3( mStartPoint );
                    GL.Vertex3( mEndPoint );
                    GL.End();
                }
            }

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
