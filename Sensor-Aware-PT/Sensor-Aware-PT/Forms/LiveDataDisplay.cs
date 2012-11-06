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
    public partial class LiveDataDisplayForm : Form, IObserver<SensorDataEntry>
    {
        
        Skeleton mUpperSkeleton = new Skeleton( SkeletonType.UpperBody );
        Scene3D mScene;
        bool[] mKeyState = new bool[ 256 ];
        bool[] mKeyStatePrev = new bool[ 256 ];
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

            setupSkeleton();

           
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
                mScene.incrementCameraRotation( 1, 0, 0 );
            }
        if(mKeyState[ (int)Keys.W]){
            mScene.incrementCameraRotation( -1, 0, 0 );
            }
        if(mKeyState[ (int)Keys.A]){
            mScene.incrementCameraRotation( 0, 1, 0 );
            }
        if(mKeyState[ (int)Keys.S]){
            mScene.incrementCameraRotation( 0, -1, 0 );
            }
        if(mKeyState[ (int)Keys.Z]){
            mScene.incrementCameraRotation( 0, 0, 1 );
            }
        if(mKeyState[ (int)Keys.X]){
            mScene.incrementCameraRotation( 0, 0, -1 );
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
                    
                    simpleOpenGlControl.SwapBuffers();
                }
            }
        }

        #endregion
        
        private void btnCalibrate_Click( object sender, EventArgs e )
        {
            mUpperSkeleton.calibrateZero();
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

        private void button4_Click( object sender, EventArgs e )
        {
            mUpperSkeleton.spitAngles();
        }

        private void setupSkeleton()
        {
            foreach( KeyValuePair<string, BoneType> kvp in Nexus.Instance.BoneMappings )
            {
                mUpperSkeleton.createMapping( kvp.Key, kvp.Value );
            }
        }

        private void label1_Click( object sender, EventArgs e )
        {

        }
    }
}
