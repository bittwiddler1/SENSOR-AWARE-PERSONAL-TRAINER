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
    public partial class RawDataForm : Form
    {
        public RawDataForm()
        {
            InitializeComponent();
        }

        public RawDataForm( string sensorID )
        {
            InitializeComponent();
            sensorLabel.Text = sensorID;
        }


        private void RawDataForm_Load( object sender, EventArgs e )
        {
            
        }

        public void updateValues( float yaw, float pitch, float roll )
        {
            yawLabel.Text = yaw.ToString();
            pitchLabel.Text = pitch.ToString();
            rollLabel.Text = roll.ToString();
        }
    }
}
