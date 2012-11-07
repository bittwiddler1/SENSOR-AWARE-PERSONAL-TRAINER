using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;

namespace Sensor_Aware_PT
{
    /// <summary>
    /// Handles all the 3d shit
    /// </summary>
    public class Scene3D : IDrawable
    {
        private Matrix4 mCameraTransform;
        private Vector3 mCameraPosition;
        private Vector3 mCameraRotation;
        private List<IDrawable> mObjectList;
        private Vector3 mTargetPosition;
        /// <summary>
        /// Holds the up vector for the world
        /// </summary>
        private Vector3 mWorldNormal;

        #region properties
        public bool DrawWireframe
        {
            get;
            set;
        }

        public bool DrawAxes
        {
            get;
            set;
        }

        public Vector3 CameraRotation
        {
            get
            {
                return mCameraRotation;
            }
            set
            {
                mCameraRotation = value;
            }
        }

        public Vector3 CameraPosition
        {
            get
            {
                return mCameraPosition;
            }

            set
            {
                mCameraPosition = value;
                Logger.Info( "Camera pos {0}, {1}, {2}", mCameraPosition.X, mCameraPosition.Y, mCameraPosition.Z );
                recalculateCameraTransform();
            }
        }

        public Vector3 CameraLookAt
        {
            get
            {
                return mTargetPosition;
            }

            set
            {
                mTargetPosition = value;
                mCameraTransform = Matrix4.LookAt( mCameraPosition, mTargetPosition, mWorldNormal );
            }
        }
        #endregion


        /// <summary>
        /// Constructs the 3d scene
        /// </summary>
        /// <param name="camLocation">location of the camera</param>
        /// <param name="targetLocation">location of the target the camera will look at</param>
        /// <param name="upVec">normal vector for up direction</param>
        public Scene3D( Vector3 camLocation, Vector3 targetLocation, Vector3 upVec )
        {
            mCameraPosition = camLocation;
            mTargetPosition = targetLocation;
            mWorldNormal = upVec;
            mCameraTransform = Matrix4.LookAt( camLocation, targetLocation, upVec );
            mCameraRotation = new Vector3(-90,0,90);

            mObjectList = new List<IDrawable>();

            DrawWireframe = false;
            DrawAxes = true;
            setupScene();
        }

        /// <summary>
        /// Internally called to set up initial openGL parameters
        /// </summary>
        private void setupScene()
        {
            GL.ShadeModel( ShadingModel.Smooth );
            GL.Enable( EnableCap.LineSmooth );

            // Enable Texture Mapping            
            //GL.Enable( GL._NORMALIZE );
            GL.Enable( EnableCap.ColorMaterial );
            GL.Enable( EnableCap.DepthTest );						    // Enables Depth Testing
            GL.Enable( EnableCap.Blend );
            GL.Enable( EnableCap.Lighting );
            GL.Enable( EnableCap.Light0 );

            GL.Hint( HintTarget.PolygonSmoothHint, HintMode.Nicest );     // Really Nice Point Smoothing
            GL.ClearColor( Color.CornflowerBlue );
        }

        /// <summary>
        /// Add objects to the draw list for the 3d scene
        /// </summary>
        /// <param name="obj"></param>
        public void addSceneObject( IDrawable obj )
        {
            if( mObjectList.Contains( obj ) )
                throw new ArgumentException( "That drawable has already been added to the list of objects!" );
            else
                mObjectList.Add( obj );
        }

        /// <summary>
        /// Called per frame update, sets up drawing and then draws all objects in the frame list
        /// </summary>
        public void draw()
        {
            /** First prepare stuff */
            preDraw();

            foreach( IDrawable drawObj in mObjectList )
            {
                drawObj.draw();
            }
        }

        /// <summary>
        /// Internally called to set opengl options and camera options before each draw call
        /// </summary>
        private void preDraw()
        {
            /** 1. Clear screen, set polygon fill mode */
            GL.Clear( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit );
            GL.PolygonMode( MaterialFace.Front, PolygonMode.Fill );

            /** 2. Set modelview mode, load camera */
            GL.MatrixMode( MatrixMode.Modelview );
            GL.LoadMatrix( ref mCameraTransform );

            /** 3. Rotate the view by user specified */
            GL.Rotate( mCameraRotation.X, 1f, 0, 0 );
            GL.Rotate( mCameraRotation.Y, 0, 1f, 0 );
            GL.Rotate( mCameraRotation.Z, 0, 0, 1f );

            if( DrawAxes )
                drawAxes();
        }

        /// <summary>
        /// Draws the x,y,z axes
        /// </summary>
        private void drawAxes()
        {
            GL.LineWidth( 2f );
            //GL.Enable( EnableCap.LineStipple );
            //GL.LineStipple( 1, Convert.ToInt16( "1000110001100011", 2 ) );

            //x+
            GL.Begin( BeginMode.Lines );
            GL.Color3( Color.Red );
            GL.Vertex3( 0, 0, 0 );
            GL.Vertex3( 100, 0, 0 );
            GL.End();

            //x-
            GL.Enable( EnableCap.LineStipple );
            GL.Begin( BeginMode.Lines );
            GL.Color3( Color.Red );
            GL.Vertex3( 0, 0, 0 );
            GL.Vertex3( -100, 0, 0 );
            GL.End();
            GL.Disable( EnableCap.LineStipple );

            //y+
            GL.Begin( BeginMode.Lines );
            GL.Color3( Color.Green );
            GL.Vertex3( 0, 0, 0 );
            GL.Vertex3( 0, 100, 0 );
            GL.End();

            //y-
            GL.Enable( EnableCap.LineStipple );
            GL.Begin( BeginMode.Lines );
            GL.Color3( Color.Green );
            GL.Vertex3( 0, 0, 0 );
            GL.Vertex3( 0, -100, 0 );
            GL.End();
            GL.Disable( EnableCap.LineStipple );
            
            //z+
            GL.Begin( BeginMode.Lines );
            GL.Color3( Color.Blue );
            GL.Vertex3( 0, 0, 0 );
            GL.Vertex3( 0, 0, 100 );
            GL.End();

            //z-
            GL.Enable( EnableCap.LineStipple );
            GL.Begin( BeginMode.Lines );
            GL.Color3( Color.Blue );
            GL.Vertex3( 0, 0, 0 );
            GL.Vertex3( 0, 0, -100 );
            GL.End();

            GL.Disable( EnableCap.LineStipple );
            GL.LineWidth( 1f );
        }

        /// <summary>
        /// used to add or remove x,y,z components from the world camera's rotation angles
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void incrementCameraRotation( float x, float y, float z )
        {
            mCameraRotation.X += x;
            mCameraRotation.Y += y;
            mCameraRotation.Z += z;
        }

        /// <summary>
        /// Increments the x, y, z parameters of the camera rotation (the target/look at gets rotated around the camera position)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void incrementCameraRotationLookAt( float x, float y, float z )
        {
            float magnitude = 0f;
            Vector3 diff = mTargetPosition - mCameraPosition;
            magnitude = diff.Length;
            diff.Normalize();

            /** diff is camera to target, rotate by 90 to get some value? */
            Matrix4 ry = Matrix4.CreateRotationX( y);
            Matrix4 rx = Matrix4.CreateRotationY( x);
            Vector3 xt = Vector3.TransformPosition( diff, rx );
            

            //mCameraPosition += x * xt;
            mTargetPosition = (xt*magnitude) + mCameraPosition;

            diff = mTargetPosition - mCameraPosition;
            magnitude = diff.Length;
            diff.Normalize();

            Vector3 yt = Vector3.TransformPosition( diff, ry );
            //mCameraPosition += y * yt;
            mTargetPosition = (yt * magnitude) + mCameraPosition;

            recalculateCameraTransform();
        }

        /// <summary>
        /// Increments the position of the camera and target together. 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void incrementCameraPositionLookAt( float x, float y, float z )
        {
            Vector3 diff = mTargetPosition - mCameraPosition;
            diff.Normalize();

            /** diff is camera to target, rotate by 90 to get some value? */
            Matrix4 ry= Matrix4.CreateRotationX( MathHelper.PiOver2 );
            Matrix4 rx= Matrix4.CreateRotationY( MathHelper.PiOver2 );
            Vector3 xt = Vector3.Transform( diff, rx );
            Vector3 yt = Vector3.Transform( diff, ry );
            
            mCameraPosition += x * xt;
            mTargetPosition += x * xt;

            mCameraPosition += y * yt;
            mTargetPosition += y * yt;


            recalculateCameraTransform();
        }

        /// <summary>
        /// Increments the position of the camera, without changing the look at
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void incrementCameraPosition( float x, float y, float z )
        {
            Vector3 diff = mTargetPosition - mCameraPosition;
            diff.Normalize();

            /** diff is camera to target, rotate by 90 to get some value? */
            Matrix4 ry = Matrix4.CreateRotationX( MathHelper.PiOver2 );
            Matrix4 rx = Matrix4.CreateRotationY( MathHelper.PiOver2 );
            Vector3 xt = Vector3.Transform( diff, rx );
            Vector3 yt = Vector3.Transform( diff, ry );

            mCameraPosition += diff * z;
            mCameraPosition += x * xt;
            mCameraPosition += y * yt;
            recalculateCameraTransform();
        }

        /// <summary>
        /// Increments the position of the target/lookat, without modifying camera position
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void incrementTargetPosition( float x, float y, float z )
        {
            /*
            mCameraPosition.X += x;
            mCameraPosition.Y += y;
            mCameraPosition.Z += z;
            */
            
            mTargetPosition.X += x;
            mTargetPosition.Y += y;
            mTargetPosition.Z += z;
            recalculateCameraTransform();
        }

        /// <summary>
        /// Increments the camera towards or away from the current lookat/target by an amount
        /// </summary>
        /// <param name="amt"></param>
        public void incrementPositionTowardsTarget( float amt )
        {
            Vector3 diff = mTargetPosition - mCameraPosition;
            diff.Normalize();

            mCameraPosition += (amt * diff);

            recalculateCameraTransform();

        }

        /// <summary>
        /// Camera transform must be recalculated after every modification of the camera position/rotation
        /// </summary>
        private void recalculateCameraTransform()
        {
            mCameraTransform = Matrix4.LookAt( mCameraPosition, mTargetPosition, mWorldNormal );
        }
    }
}
