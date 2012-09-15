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


namespace Sensor_Aware_PT
{
    public partial class ExperimentalForm : Form, IObserver<SensorDataEntry>
    {
        private Bone[] mBones = new Bone[ 4 ];
        Dictionary<String, SensorDataEntry> mLastSensorData = new Dictionary<string, SensorDataEntry>();
        Skeleton mUpperSkeleton = new Skeleton( SkeletonType.UpperBody );
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
            //GL.Enable( EnableCap.Lighting );
            //GL.Enable( EnableCap.Light0 );
            GL.Hint( HintTarget.PolygonSmoothHint, HintMode.Nicest);     // Really Nice Point Smoothing
            
            //this.Text = "Sensor " + mSensor.Id;
            formUpdateTimer = new Timer();
            formUpdateTimer.Interval = 20;
            formUpdateTimer.Tick += new EventHandler( formUpdateTimer_Tick );
            formUpdateTimer.Start();

            GL.ClearColor( Color.CornflowerBlue );

            mUpperSkeleton.createMapping( "B", BoneType.ArmUpperL );
            mUpperSkeleton.createMapping( "C", BoneType.ArmLowerL );
            /*

            mBones[ 0 ]= new Bone( 2f, new Vector3( 0, 0, 0 ) );
            mBones[ 1 ] = new Bone(3f, new Vector3(0,0,0));
            mBones[ 2 ]= new Bone( 4f, new Vector3( 0, 0, 0 ) );
            mBones[ 3] = new Bone( 3f, new Vector3( 0, 0, 0 ) );

            mBones[ 0 ].addChild( mBones[ 1 ] );
            mBones[ 0 ].addChild( mBones[ 3 ] );
            mBones[ 1 ].addChild( mBones[ 2 ] );
            */


            
            //Nexus.Instance.Subscribe( this );
            //Nexus.Instance.Subscribe( mUpperSkeleton );
            SensorDataPlayer sdp = new SensorDataPlayer();
            sdp.Subscribe( this );
            sdp.Subscribe( mUpperSkeleton );
            sdp.replayFile( "data.bin" );




        }

        void formUpdateTimer_Tick( object sender, EventArgs e )
        {
            lock( this )
            {
                simpleOpenGlControl.Refresh();
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
            GL.Clear( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit );    // Clear screen and DepthBuffer
            
            GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill );

            // Set camera view and distance
            Matrix4 lookat = Matrix4.LookAt( 0, 0, 50, 0, 0, 0, 0, 1, 0 );
            
            GL.MatrixMode( MatrixMode.Modelview );
            GL.LoadIdentity();
            //Tao.OpenGL.u.gluLookAt( 0, 0, 15, 0, 0, 0, 0, 1, 0 );
            GL.LoadMatrix( ref lookat );

            //GL.PushMatrix();
            //mBones[ 0 ].drawBone();
            mUpperSkeleton.draw();
            simpleOpenGlControl.SwapBuffers();
            //GL.PopMatrix();
            //GL.Flush();
        }

        #endregion
        
        private void button1_Click( object sender, EventArgs e )
        {
            //foreach( Bone b in mBones )
              //  b.setYawOffset();
        }

        private void button2_Click( object sender, EventArgs e )
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
        }
    }
}
