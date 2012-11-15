using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using QuickFont;

//DONT FORGET TO SET THE GLCONTROL CONSTRUCTOR
//: base(new GraphicsMode(32, 24, 8, 4), 3, 0, GraphicsContextFlags.ForwardCompatible)
namespace Sensor_Aware_PT
{
    public partial class LiveDataDisplayForm : Form, IObserver<SensorDataEntry>
    {
        
        Skeleton mSkeleton = new Skeleton( SkeletonType.UpperBody );
        Scene3D mScene;
        bool[] mKeyState = new bool[ 256 ];
        bool[] mKeyStatePrev = new bool[ 256 ];
        bool[] mMouseState = new bool[ 2 ];
        Vector2 mMouseLoc = new Vector2();
        //bool[] mMouseState = new bool[ 2 ];
        private bool mLoaded = false;

        public LiveDataDisplayForm()
        {
            InitializeComponent();
        }

        private Timer formUpdateTimer;
        #region SimpleOpenGlControl methods

        /// <summary>
        /// Form load event to initialises OpenGL graphics.
        /// </summary>
        private void simpleOpenGlControl_Load( object sender, EventArgs ex )
        {
            mLoaded = true;
            simpleOpenGlControl_SizeChanged( sender, ex );

            /** Setup the 3d scene object */
            mScene = new Scene3D(new Vector3(40, 35, 40), new Vector3(0, 0, 0), new Vector3( 0, 1, 0 ));

            initializeRedrawTimer();

            simpleOpenGlControl.Focus();

            for( int i = 0; i < 256; i++ )
            {
                mKeyState[ i ] = false;
                mKeyStatePrev[ i ] = false;
            }

/*
            Matrix4 rx, ry, rz;
            rx = Matrix4.CreateRotationX(-MathHelper.PiOver2);
            ry = Matrix4.Identity;
            rz = Matrix4.CreateRotationZ(MathHelper.PiOver2);
            mCamRotation = rx * ry * rz;
            
            mCamRotation.Transpose();
            mCamRotation.Row2 *= -1f;
            */

            try
            {
                setupSkeleton();
            }
            catch (Exception)
            {
                MappingDialog MD = new MappingDialog();
                MD.ShowDialog();
            }
           
        private void initializeRedrawTimer()
        {
            formUpdateTimer = new Timer();
            formUpdateTimer.Interval = 20;
            formUpdateTimer.Tick += new EventHandler( formUpdateTimer_Tick );
            formUpdateTimer.Start();
        }

        public void subscribeToSource( IObservable<SensorDataEntry> source )
        {

            // source.Subscribe( this ); // the view shouldnt have anything to do with the data
            source.Subscribe( mSkeleton );
        }

        void formUpdateTimer_Tick( object sender, EventArgs e )
        {
            lock( this )
            {
                simpleOpenGlControl.Refresh();
                handleInput();
                updateDebugText();
            }
        }

        
        void handleInput()
        {
            /// ROTATION
            if(mKeyState[ (int)Keys.Q])
                mScene.incrementCameraRotation( 1, 0, 0 );
            
            if(mKeyState[ (int)Keys.W])
                mScene.incrementCameraRotation( -1, 0, 0 );
            
            if(mKeyState[ (int)Keys.A])
                mScene.incrementCameraRotation( 0, 1, 0 );
            
            if(mKeyState[ (int)Keys.S])
                mScene.incrementCameraRotation( 0, -1, 0 );
            
            if(mKeyState[ (int)Keys.Z])
                mScene.incrementCameraRotation( 0, 0, 1 );
            
            if(mKeyState[ (int)Keys.X])
                mScene.incrementCameraRotation( 0, 0, -1 );

            ///TRANSLATION camera ONLY
            if( mKeyState[ ( int ) Keys.I ] )
                mScene.incrementCameraPosition( 1, 0, 0 );

            if( mKeyState[ ( int ) Keys.O ] )
                mScene.incrementCameraPosition( -1, 0, 0 );

            if( mKeyState[ ( int ) Keys.K ] )
                mScene.incrementCameraPosition( 0, 1, 0 );

            if( mKeyState[ ( int ) Keys.L ] )
                mScene.incrementCameraPosition( 0, -1, 0 );

            if( mKeyState[ ( int ) Keys.Oemcomma ] )
                mScene.incrementCameraPosition( 0, 0, 1 );

            if( mKeyState[ ( int ) Keys.OemPeriod] )
                mScene.incrementCameraPosition( 0, 0, -1 );


            ///TRANSLATION LOOKAT
            if( mKeyState[ ( int ) Keys.T ] )
                mScene.incrementTargetPosition( 1, 0, 0 );

            if( mKeyState[ ( int ) Keys.Y ] )
                mScene.incrementTargetPosition( -1, 0, 0 );

            if( mKeyState[ ( int ) Keys.G ] )
                mScene.incrementTargetPosition( 0, 1, 0 );

            if( mKeyState[ ( int ) Keys.H ] )
                mScene.incrementTargetPosition( 0, -1, 0 );

            if( mKeyState[ ( int ) Keys.B ] )
                mScene.incrementTargetPosition( 0, 0, 1 );

            if( mKeyState[ ( int ) Keys.N ] )
                mScene.incrementTargetPosition( 0, 0, -1 );


            if( mKeyState[ ( int ) Keys.C ] )
                mScene.incrementPositionTowardsTarget(1f);
            if( mKeyState[ ( int ) Keys.V ] )
                mScene.incrementPositionTowardsTarget(-1f);
        }

        void updateDebugText()
        {
            Dictionary<String, Vector3> values = this.mSkeleton.getAngles();

            Vector3 ElbowL = values["ArmLowerL"];
            String LeftElbow = String.Format("{0:F2} {1:F2} {2:F2}", ElbowL.X, ElbowL.Y, ElbowL.Z);

            Vector3 ElbowR = values["ArmLowerR"];
            String RightElbow = String.Format("{0:F2} {1:F2} {2:F2}", ElbowR.X, ElbowR.Y, ElbowR.Z);

            txtDebug.Text = String.Format("Angles\r\nLeft Elbow: {0}\r\nRight Elbow:{1}", LeftElbow, RightElbow);

            var armend = mSkeleton.getLastViewBonePositionStart();

            mScene.resetFontText();
            mScene.addFontText( txtDebug.Text, armend, Color.Red );
            mScene.renderFontText();
            

        }

        /// <summary>
        /// Window resize event to adjusts perspective.
        /// </summary>
        private void simpleOpenGlControl_SizeChanged( object sender, EventArgs e )
        {
            simpleOpenGlControl.MakeCurrent();
            int height = simpleOpenGlControl.Size.Height;
            int width = simpleOpenGlControl.Size.Width;
            GL.MatrixMode( MatrixMode.Projection );
            
            GL.LoadIdentity();
            GL.Viewport( 0, 0, width, height );
            //Glu.gluPerspective( 10, ( float ) width / ( float ) height, 1.0, 250 );
            setPerspective( 10f,(float) width / height, 1.0f, 250f );


            GL.MatrixMode( MatrixMode.Modelview);

            QFont.RefreshViewport();
            
        }

        private void setPerspective( float fovy, float aspect, float zNear, float zFar )
        {
            float fH =(float)Math.Tan( (fovy / 360.0f * 3.14159f) ) * zNear;
            float fW = fH * aspect;
            GL.Frustum(-fW, fW, -fH, fH, zNear, zFar);
            
        }
        
        /// <summary>
        /// Redraw cuboid polygons.
        /// </summary>
        private void simpleOpenGlControl_Paint( object sender, PaintEventArgs e )
        {
            if( mLoaded )
            {
                lock( this )
                {
                    simpleOpenGlControl.MakeCurrent();
                    mScene.preDraw();
                    mScene.draw();
                    simpleOpenGlControl.SwapBuffers();
                }
            }
        }

        #endregion
        
        private void btnCalibrate_Click( object sender, EventArgs e )
        {
            mSkeleton.calibrateZero();
        }

        private void btnSynchronize_Click( object sender, EventArgs e )
        {
            Nexus.Instance.resynchronize();
        }

        private void ExperimentalForm_Load( object sender, EventArgs e )
        {


        }

        #region IObserver<DataFrame> Members

        void IObserver<SensorDataEntry>.OnCompleted()
        {
            //throw new NotImplementedException();
        }

        void IObserver<SensorDataEntry>.OnError( Exception error )
        {
            //throw new NotImplementedException();
        }

        void IObserver<SensorDataEntry>.OnNext( SensorDataEntry value )
        {

        }

        #endregion

        private void button3_Click( object sender, EventArgs e )
        {

        }

        private void ExperimentalForm_FormClosing( object sender, FormClosingEventArgs e )
        {
            lock( this )
            {
                formUpdateTimer.Stop();
                mLoaded = false;
                simpleOpenGlControl.Context.Dispose();
                simpleOpenGlControl.Dispose();
            }
        }

        private void simpleOpenGlControl_KeyDown( object sender, KeyEventArgs e )
        {
            mKeyState[ (int)e.KeyCode ] = true;
            
            switch( e.KeyCode)
            {
                case Keys.E:
                    mSkeleton.toggleBox();
                    break;
                case Keys.R:
                    mSkeleton.toggleWireframe();
                    break;
                default:
                    break;
            }
        }

        private void simpleOpenGlControl_KeyUp( object sender, KeyEventArgs e )
        {
            mKeyState[ ( int ) e.KeyCode ] = false;
            
        }

        private void button4_Click( object sender, EventArgs e )
        {
            mSkeleton.spitAngles();
            mSkeleton.debugWritePositions();
        }

        private void setupSkeletonBoneMappings()
        {
            foreach( KeyValuePair<string, BoneType> kvp in Nexus.Instance.BoneMappings )
            {
                mSkeleton.createMapping( kvp.Key, kvp.Value );
            }

        }

        private void cameraFocusDropdown_SelectedIndexChanged( object sender, EventArgs e )
        {
            switch( (string)cameraFocusDropdown.SelectedItem)
            {
                case "Arms L":
                    mScene.CameraPosition = mSkeleton.getSkeletonView( "Arms L" ).position;
                    mScene.CameraLookAt = mSkeleton.getSkeletonView( "Arms L" ).lookAt;
                    break;
                case "Arms R":
                    mScene.CameraPosition = mSkeleton.getSkeletonView( "Arms R" ).position;
                    mScene.CameraLookAt = mSkeleton.getSkeletonView( "Arms R" ).lookAt;
                    break;
                case "Legs L":
                    mScene.CameraPosition = mSkeleton.getSkeletonView( "Legs L" ).position;
                    mScene.CameraLookAt = mSkeleton.getSkeletonView( "Legs L" ).lookAt;
                    break;
                case "Legs R":
                    mScene.CameraPosition = mSkeleton.getSkeletonView( "Legs R" ).position;
                    mScene.CameraLookAt = mSkeleton.getSkeletonView( "Legs R" ).lookAt;
                    break;
                case "Torso":
                    mScene.CameraPosition = mSkeleton.getSkeletonView( "Torso" ).position;
                    mScene.CameraLookAt = mSkeleton.getSkeletonView( "Torso" ).lookAt;
                    break;
                case "Hip":
                    break;
                case "Full Body":
                    mScene.CameraPosition = mSkeleton.getSkeletonView( "Full Body" ).position;
                    mScene.CameraLookAt = mSkeleton.getSkeletonView( "Full Body" ).lookAt;
                    break;
            }
        }

        private void simpleOpenGlControl_MouseDown( object sender, System.Windows.Forms.MouseEventArgs e )
        {
            switch( e.Button )
            {
                case MouseButtons.Left:
                    mMouseState[ 0 ] = true;
                    break;
                case MouseButtons.Middle:
                    break;
                case MouseButtons.None:
                    break;
                case MouseButtons.Right:
                    mMouseState[ 1 ] = true;
                    break;
                case MouseButtons.XButton1:
                    break;
                case MouseButtons.XButton2:
                    break;
                default:
                    break;
            }

            mMouseLoc.X = e.X;
            mMouseLoc.Y = e.Y;
        }

        private void simpleOpenGlControl_Scroll( object sender, ScrollEventArgs e )
        {

        }

        //this.simpleOpenGlControl.MouseWheel += new System.Windows.Forms.MouseEventHandler( this.simpleOpenGlControl_MouseWheel );
        private void simpleOpenGlControl_MouseWheel( object sender, System.Windows.Forms.MouseEventArgs e )
        {
            if( e.Delta != 0 )
            {
                if( mKeyState[ ( int ) Keys.ShiftKey ] )
                    mScene.incrementCameraPosition( 0, 0, e.Delta / 100f );
                else
                    mScene.incrementPositionTowardsTarget( ( float ) e.Delta/100f );
            }
        } 

        private void simpleOpenGlControl_MouseMove( object sender, System.Windows.Forms.MouseEventArgs e )
        {
            Vector2 mouseNow = new Vector2( e.X, e.Y );
            Vector2 delta = mouseNow - mMouseLoc;
            //left drag, pan
            /** If you hold shift, we pan the camera without changing the target */
            if( mMouseState[ 0 ] == true )
            {
                if( mKeyState[ ( int ) Keys.ShiftKey ] )
                    mScene.incrementCameraPosition(delta.X/10f, delta.Y/10f, 0);
                else
                    mScene.incrementCameraPositionLookAt( delta.X/100f, delta.Y/100f, 0 );
            }


            //right drag
            if( mMouseState[ 1 ] == true )
            {
                mScene.incrementCameraRotationLookAt( delta.X / 1000f, delta.Y / 1000f, 0 );
            }

            mMouseLoc.X = e.X;
            mMouseLoc.Y = e.Y;

        }

        private void simpleOpenGlControl_MouseUp( object sender, System.Windows.Forms.MouseEventArgs e )
        {
            switch( e.Button )
            {
                case MouseButtons.Left:
                    mMouseState[ 0 ] = false;
                    break;
                case MouseButtons.Middle:
                    break;
                case MouseButtons.None:
                    break;
                case MouseButtons.Right:
                    mMouseState[ 1 ] = false;
                    break;
                case MouseButtons.XButton1:
                    break;
                case MouseButtons.XButton2:
                    break;
                default:
                    break;
            }

            mMouseLoc.X = e.X;
            mMouseLoc.Y = e.Y;
        }

        private void LiveDataDisplayForm_Resize( object sender, EventArgs e )
        {

        }
    }
}
