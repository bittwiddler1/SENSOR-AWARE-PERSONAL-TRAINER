using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;

namespace Sensor_Aware_PT.Forms
{
    public partial class LiveAccelerometerGraphForm : Form, IObserver<SensorDataEntry>
    {
        private Timer formUpdateTimer;
        AccelerometerGrapher mGrapher;

        public LiveAccelerometerGraphForm()
        {
            InitializeComponent();
        }

        private void antiAliasedGLControl1_Load( object sender, EventArgs e )
        {
            initializeRedrawTimer();
            mGrapher = new AccelerometerGrapher( antiAliasedGLControl1.Width, antiAliasedGLControl1.Height );
            antiAliasedGLControl1_SizeChanged( sender, e );
            
        }

        private void initializeRedrawTimer()
        {
            formUpdateTimer = new Timer();
            formUpdateTimer.Interval = 5;
            formUpdateTimer.Tick += new EventHandler( formUpdateTimer_Tick );
            formUpdateTimer.Start();
        }

        void formUpdateTimer_Tick( object sender, EventArgs e )
        {
            antiAliasedGLControl1.Refresh();
        }

        private void antiAliasedGLControl1_SizeChanged( object sender, EventArgs e )
        {
            int height = antiAliasedGLControl1.Size.Height;
            int width = antiAliasedGLControl1.Size.Width;
            GL.MatrixMode( MatrixMode.Projection );

            GL.LoadIdentity();
            GL.Viewport( 0, 0, width, height );
            GL.Ortho(0, width, height, 0, -1000, 1000 );

            GL.MatrixMode( MatrixMode.Modelview );

            mGrapher.setDimensions(new OpenTK.Vector2( width, height ));
        }

        private void antiAliasedGLControl1_Paint( object sender, PaintEventArgs e )
        {
            antiAliasedGLControl1.MakeCurrent();

            GL.Clear( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit );
            GL.PolygonMode( MaterialFace.Front, PolygonMode.Fill );


            mGrapher.draw();
            antiAliasedGLControl1.SwapBuffers();
        }

        public void subscribeToSource( IObservable<SensorDataEntry> source )
        {

            // source.Subscribe( this ); // the view shouldnt have anything to do with the data
            source.Subscribe( mGrapher );
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
            throw new NotImplementedException();
        }

        #endregion

        private void btnResynchronize_Click( object sender, EventArgs e )
        {
            Nexus.Instance.resynchronize();
        }
    }
}
