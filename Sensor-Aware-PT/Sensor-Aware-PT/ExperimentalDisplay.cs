using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
using OpenTK;
using System.Threading;
using System.Drawing;

namespace Sensor_Aware_PT
{
    public class ExperimentalDisplay : GameWindow
    {
        public static float yaw = 0f;
        public static float pitch = 0.0f;
        public static float roll = 0.0f;
        public static float yawOffset = 0.0f;

        /** Yes I know labeling variables with _2,3,...n is bad but I need to see if this works ok? */
        public static float yaw_2 = 0f;
        public static float pitch_2 = 0.0f;
        public static float roll_2 = 0.0f;
        public static float yawOffset_2 = 0.0f;
        Nexus mSensorManager;
        
        
        public ExperimentalDisplay()
            : base( 800, 600, new GraphicsMode(16, 16), "This be experimental, dawg")
        {

        }

        protected override void OnLoad( System.EventArgs e )
        {
            base.OnLoad( e );

            GL.ClearColor( Color.MidnightBlue );
            GL.Enable( EnableCap.DepthTest );
            mSensorManager = new Nexus();
            
        }

        /// <param name="e">Contains information necessary for frame rendering.</param>
        protected override void OnRenderFrame( OpenTK.FrameEventArgs e )
        {
            base.OnRenderFrame( e );

            GL.Clear( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit );

            Matrix4 lookat = Matrix4.LookAt( 0, 0, 15, 0, 0, 0, 0, 1, 0 );
            GL.MatrixMode( MatrixMode.Modelview );
            GL.LoadMatrix( ref lookat );


            GL.Translate( -2.5f, 0, 0 );
            GL.Rotate( ( yaw - yawOffset) , 0, 1, 0 );
            GL.Rotate(  pitch , 1, 0, 0 );
            GL.Rotate(  -roll , 0, 0, 1 );
            Primitives.DrawCube( .75f, .5f, 1f );

            GL.LoadMatrix( ref lookat );

            GL.Translate( 2.5f, 0, 0 );


            GL.Rotate( ( yaw_2 - yawOffset_2 ), 0, 1, 0 );
            GL.Rotate( pitch_2, 1, 0, 0 );
            GL.Rotate( -roll_2, 0, 0, 1 );
            Primitives.DrawCube( .75f, .5f, 1f );


            this.SwapBuffers();
            Thread.Sleep( 1 );
        }

        /// <param name="e">Contains information necessary for frame updating.</param>
        protected override void OnUpdateFrame( OpenTK.FrameEventArgs e )
        {
            base.OnUpdateFrame( e );

            SensorDataEntry a = mSensorManager.getEntry( 0 );
            SensorDataEntry b = mSensorManager.getEntry( 1 );

            yaw = a.orientation.X;
            pitch = a.orientation.Y;
            roll = a.orientation.Z;


            yaw_2 = b.orientation.X;
            pitch_2 = b.orientation.Y;
            roll_2 = b.orientation.Z;

            if( Keyboard[ OpenTK.Input.Key.A] )
            {
                yawOffset = yaw;
                yawOffset_2 = yaw_2;
            }
            
        }

        protected override void OnResize( System.EventArgs e )
        {
            base.OnResize( e );

            GL.Viewport( 0, 0, Width, Height );

            double aspect_ratio = Width / ( double ) Height;

            OpenTK.Matrix4 perspective = OpenTK.Matrix4.CreatePerspectiveFieldOfView( MathHelper.PiOver4, ( float ) aspect_ratio, 1, 64 );
            GL.MatrixMode( MatrixMode.Projection );
            GL.LoadMatrix( ref perspective );
        }
    }
}
