using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Sensor_Aware_PT
{
    public partial class RawDataForm : Form, IObserver<SensorDataEntry>
    {
        public RawDataForm()
        {
            InitializeComponent();
        }

        public RawDataForm( string sensorID )
        {
            InitializeComponent();
            sensorLabel.Text = sensorID;
            Nexus.Instance.Subscribe( this );
        }


        private void RawDataForm_Load( object sender, EventArgs e )
        {
            
        }

        internal void updateValues( float yaw, float pitch, float roll )
        {
            yawLabel.Text = yaw.ToString();
            pitchLabel.Text = pitch.ToString();
            rollLabel.Text = roll.ToString();
        }

        #region IObserver<SensorDataEntry> Members

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError( Exception error )
        {
            throw new NotImplementedException();
        }

        public void OnNext( SensorDataEntry value )
        {
            if( sensorLabel.Text == value.id )
            {
                this.Invoke( (MethodInvoker)delegate
                {
                    updateValues( value.accelerometer.X, value.accelerometer.Y, value.accelerometer.Z );
                } );
            }
        }

        #endregion

        private void tableLayoutPanel1_Paint( object sender, PaintEventArgs e )
        {

        }
    }
}
