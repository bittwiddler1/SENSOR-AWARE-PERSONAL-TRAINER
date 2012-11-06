using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;

namespace Sensor_Aware_PT.Core
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
        #endregion

        public Scene3D( Vector3 camLocation, Vector3 targetLocation, Vector3 upVec )
        {
            mCameraPosition = camLocation;
            mTargetPosition = targetLocation;
            mWorldNormal = upVec;
            mCameraTransform = Matrix4.LookAt( camLocation, targetLocation, upVec );
            mCameraRotation = new Vector3(-90,0,90);

            mObjectList = new List<IDrawable>();

            DrawWireframe = false;

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

        public void draw()
        {
            /** First prepare the camera */
            foreach( IDrawable drawObj in mObjectList )
            {
                drawObj.draw();
            }
        }

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
    }
}
