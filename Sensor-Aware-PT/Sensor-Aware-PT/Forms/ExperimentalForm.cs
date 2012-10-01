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

//DONT FORGET TO SET THE GLCONTROL CONSTRUCTOR
//: base(new GraphicsMode(32, 24, 8, 4), 3, 0, GraphicsContextFlags.ForwardCompatible)
namespace Sensor_Aware_PT
{
    public partial class ExperimentalForm : Form, IObserver<SensorDataEntry>
    {
        private Bone[] mBones = new Bone[ 4 ];
        Dictionary<String, SensorDataEntry> mLastSensorData = new Dictionary<string, SensorDataEntry>();
        Skeleton mUpperSkeleton = new Skeleton( SkeletonType.UpperBody );
        Vector3 mViewRotations = new Vector3(-90,0,90);
        Vector3 mViewTranslations = new Vector3();
        Matrix4 mCamRotation = new Matrix4();
        Matrix4 mTransform = Matrix4.Identity;
        Matrix4 mCalibTrans = Matrix4.Identity;
        Matrix4 mLastTransform = Matrix4.Identity;

        bool[] mKeyState = new bool[ 256 ];
        bool[] mKeyStatePrev = new bool[ 256 ];
        private bool mLoaded = false;
        public ExperimentalForm()
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
            
            //simpleOpenGlControl.SwapBuffers();
            simpleOpenGlControl_SizeChanged( sender, ex );
            GL.ShadeModel( ShadingModel.Smooth );
            GL.Enable( EnableCap.LineSmooth);
            
            					    // Enable Texture Mapping            
            //GL.Enable( GL._NORMALIZE );
            GL.Enable( EnableCap.ColorMaterial );
            GL.Enable( EnableCap.DepthTest);						    // Enables Depth Testing
            GL.Enable( EnableCap.Blend );
            GL.Enable( EnableCap.Lighting );
            GL.Enable( EnableCap.Light0 );
            
            GL.Hint( HintTarget.PolygonSmoothHint, HintMode.Nicest);     // Really Nice Point Smoothing
            
            //this.Text = "Sensor " + mSensor.Id;
            formUpdateTimer = new Timer();
            formUpdateTimer.Interval = 20;
            formUpdateTimer.Tick += new EventHandler( formUpdateTimer_Tick );
            formUpdateTimer.Start();

            GL.ClearColor( Color.CornflowerBlue );


            mUpperSkeleton.createMapping( "C", BoneType.ArmUpperL );
            mUpperSkeleton.createMapping( "A", BoneType.ArmLowerL );
            mUpperSkeleton.createMapping( "B", BoneType.ArmLowerR );
            mUpperSkeleton.createMapping( "D", BoneType.ArmUpperR );
            simpleOpenGlControl.Focus();

            for( int i = 0; i < 256; i++ )
            {
                mKeyState[ i ] = false;
                mKeyStatePrev[ i ] = false;
            }


            Matrix4 rx, ry, rz;
            rx = Matrix4.CreateRotationX(-MathHelper.PiOver2);
            ry = Matrix4.Identity;
            rz = Matrix4.CreateRotationZ(MathHelper.PiOver2);
            mCamRotation = rx * ry * rz;
            
            mCamRotation.Transpose();
            mCamRotation.Row2 *= -1f;
        }

        public void subscribeToSource( IObservable<SensorDataEntry> source )
        {
            source.Subscribe( this );
            source.Subscribe( mUpperSkeleton );
        }

        void formUpdateTimer_Tick( object sender, EventArgs e )
        {
            lock( this )
            {
                simpleOpenGlControl.Refresh();
                handleInput();
            }
        }

        void handleInput()
        {


            if(mKeyState[ (int)Keys.Q]){
            mViewRotations.X += 1f;
            }
        if(mKeyState[ (int)Keys.W]){
            mViewRotations.X -= 1f;
            }
        if(mKeyState[ (int)Keys.A]){
            mViewRotations.Y += 1f;
            }
        if(mKeyState[ (int)Keys.S]){
            mViewRotations.Y -= 1f;
            }
        if(mKeyState[ (int)Keys.Z]){
            mViewRotations.Z += 1f;
            }
        if(mKeyState[ (int)Keys.X]){
            mViewRotations.Z -= 1f;
                    }

        }

        /// <summary>
        /// Window resize event to adjusts perspective.
        /// </summary>
        private void simpleOpenGlControl_SizeChanged( object sender, EventArgs e )
        {
            int height = simpleOpenGlControl.Size.Height;
            int width = simpleOpenGlControl.Size.Width;
            GL.MatrixMode( MatrixMode.Projection );
            
            GL.LoadIdentity();
            GL.Viewport( 0, 0, width, height );
            //Glu.gluPerspective( 10, ( float ) width / ( float ) height, 1.0, 250 );
            setPerspective( 10f,(float) width / height, 1.0f, 250f );


            GL.MatrixMode( MatrixMode.Modelview);
            //jPopMatrix();
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
                       
                    GL.Clear( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit );    // Clear screen and DepthBuffer

                    GL.PolygonMode( MaterialFace.Front, PolygonMode.Fill );

                    // Set camera view and distance
                    Matrix4 lookat = Matrix4.LookAt( 40, 35, 40, 0, 0, 0, 0, 1, 0 );

                    GL.MatrixMode( MatrixMode.Modelview );
                    GL.LoadIdentity();
                    //Tao.OpenGL.u.gluLookAt( 0, 0, 15, 0, 0, 0, 0, 1, 0 );
                    GL.LoadMatrix( ref lookat );

                    GL.Translate( 0, 0, 0 );
                    /*
                    GL.Rotate( mViewRotations.X, 1f, 0, 0 );
                    GL.Rotate( mViewRotations.Y, 0, 1f, 0 );
                    GL.Rotate( mViewRotations.Z, 0, 0, 1f );
                    */
                    GL.MultMatrix(ref mCamRotation);
                    
                    GL.LineWidth( 2f );
                    //GL.Enable( EnableCap.LineStipple );
                    GL.LineStipple(1, Convert.ToInt16("1000110001100011", 2));
                    
                    //x+
                    GL.Begin( BeginMode.Lines );
                    GL.Color3( Color.Red );
                    GL.Vertex3( 0, 0, 0 );
                    GL.Vertex3( 100, 0, 0 );
                    GL.End();

                    GL.Enable(EnableCap.LineStipple);
                    //x-
                    GL.Begin(BeginMode.Lines);
                    GL.Color3(Color.Red);
                    GL.Vertex3(0, 0, 0);
                    GL.Vertex3(-100, 0, 0);
                    GL.End();
                    GL.Disable(EnableCap.LineStipple);

                    
                    GL.Begin( BeginMode.Lines );
                    GL.Color3( Color.Green );
                    GL.Vertex3( 0, 0, 0 );
                    GL.Vertex3( 0, 100, 0 );
                    GL.End();

                    GL.Enable(EnableCap.LineStipple);
                    GL.Begin(BeginMode.Lines);
                    GL.Color3(Color.Green);
                    GL.Vertex3(0, 0, 0);
                    GL.Vertex3(0, -100, 0);
                    GL.End();

                    GL.Disable(EnableCap.LineStipple);
                    GL.Begin( BeginMode.Lines );
                    GL.Color3( Color.Blue );
                    GL.Vertex3( 0, 0, 0 );
                    GL.Vertex3( 0, 0, 100 );
                    GL.End();
                    GL.Enable(EnableCap.LineStipple);
                    GL.Begin(BeginMode.Lines);
                    GL.Color3(Color.Blue);
                    GL.Vertex3(0, 0, 0);
                    GL.Vertex3(0, 0, -100);
                    GL.End();

                    GL.Disable( EnableCap.LineStipple );
                    GL.LineWidth( 1f );
                    /*
                    GL.PushMatrix();
                    //////////////////////////////
                    //mTransform.Transpose();
                    GL.MultMatrix(ref mTransform);
                    //GL.MultTransposeMatrix(ref mTransform);
                    GL.LineWidth(4f);
                    //GL.Enable( EnableCap.LineStipple );
                    GL.LineStipple(1, Convert.ToInt16("1000110001100011", 2));

                    //x+
                    GL.Begin(BeginMode.Lines);
                    GL.Color3(Color.Red);
                    GL.Vertex3(0, 0, 0);
                    GL.Vertex3(100, 0, 0);
                    GL.End();

                    GL.Enable(EnableCap.LineStipple);
                    //x-
                    GL.Begin(BeginMode.Lines);
                    GL.Color3(Color.Red);
                    GL.Vertex3(0, 0, 0);
                    GL.Vertex3(-100, 0, 0);
                    GL.End();
                    GL.Disable(EnableCap.LineStipple);


                    GL.Begin(BeginMode.Lines);
                    GL.Color3(Color.Green);
                    GL.Vertex3(0, 0, 0);
                    GL.Vertex3(0, 100, 0);
                    GL.End();

                    GL.Enable(EnableCap.LineStipple);
                    GL.Begin(BeginMode.Lines);
                    GL.Color3(Color.Green);
                    GL.Vertex3(0, 0, 0);
                    GL.Vertex3(0, -100, 0);
                    GL.End();

                    GL.Disable(EnableCap.LineStipple);
                    GL.Begin(BeginMode.Lines);
                    GL.Color3(Color.Blue);
                    GL.Vertex3(0, 0, 0);
                    GL.Vertex3(0, 0, 100);
                    GL.End();
                    GL.Enable(EnableCap.LineStipple);
                    GL.Begin(BeginMode.Lines);
                    GL.Color3(Color.Blue);
                    GL.Vertex3(0, 0, 0);
                    GL.Vertex3(0, 0, -100);
                    GL.End();

                    GL.Disable(EnableCap.LineStipple);
                    GL.PopMatrix();
                    */
                    //GL.PushMatrix();
                    //mBones[ 0 ].drawBone();
                    mUpperSkeleton.draw();
                    simpleOpenGlControl.SwapBuffers();
                    //GL.PopMatrix();
                    //GL.Flush();
                }
            }
        }

        #endregion
        
        private void button1_Click( object sender, EventArgs e )
        {
            //foreach( Bone b in mBones )
              //  b.setYawOffset();
            mUpperSkeleton.calibrateZero();
            mCalibTrans = mLastTransform;
            mCalibTrans.Invert();
        }

        private void button2_Click( object sender, EventArgs e )
        {
            Nexus.Instance.resynchronize();
            mCalibTrans = Matrix4.Identity;
            
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
            mLastTransform = value.orientation;
            mLastTransform.Transpose();
            mTransform = mCalibTrans * mLastTransform;
            /*
            switch(value.id)
            {

                case "A":
                    mBones[ 0 ].updateOrientation( value.orientation );
                    break;
                case "B":
                    mBones[ 1 ].updateOrientation( value.orientation );
                    break;
                case "C":
                    mBones[ 2 ].updateOrientation( value.orientation );
                    break;
                case "D":
                    mBones[ 3 ].updateOrientation( value.orientation );
                    break;
            }
             * */
            lock( mLastSensorData )
            {
                mLastSensorData[ value.id ] = value;
            }
            
            
        }

        #endregion

        private void button3_Click( object sender, EventArgs e )
        {
            lock( mLastSensorData )
            {
                foreach( SensorDataEntry s in mLastSensorData.Values )
                {
                    Logger.Info( "{0}", s.ToString() );
                }
            }

            Logger.Info("{0},{1},{2}", mViewRotations.X, mViewRotations.Y, mViewRotations.Z);
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

        private void simpleOpenGlControl_KeyPress( object sender, System.Windows.Forms.KeyPressEventArgs e )
        {

        }

        private void simpleOpenGlControl_KeyDown( object sender, KeyEventArgs e )
        {
            mKeyState[ (int)e.KeyCode ] = true;
            
            switch( e.KeyCode)
            {
                case Keys.E:
                    mUpperSkeleton.toggleBox();
                    break;
                case Keys.R:
                    mUpperSkeleton.toggleWireframe();
                    break;
                default:
                    break;
            }
        }

        private void simpleOpenGlControl_KeyUp( object sender, KeyEventArgs e )
        {
            mKeyState[ ( int ) e.KeyCode ] = false;
        }
    }
}
