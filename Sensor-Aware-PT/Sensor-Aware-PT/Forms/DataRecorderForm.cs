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
    public partial class SensorDataView : Form
    {
        SensorDataRecorder mRecorder;
        bool init = false;
        public SensorDataView()
        {
            InitializeComponent();
        }

        private void SensorDataView_Load( object sender, EventArgs e )
        {
            button1.Text = "START";

        }

        private void button1_Click( object sender, EventArgs e )
        {
            if( !init )
            {
                mRecorder = new SensorDataRecorder();
                init = true;
            }

            if( !mRecorder.isRecording )
            {
                mRecorder.beginRecording();
                button1.Text = "STOP";
                this.Text = "RECORDING!!";
            }
            else
            {
                List<SensorDataEntry> datas = mRecorder.stopRecording();
                button1.Text = "START";
                this.Text = "IDLE";
                BindingList<SensorDataEntry> blist = new BindingList<SensorDataEntry>( datas );
                dataGridView1.DataSource = blist;
                mRecorder.writeFile( "data.bin" );
                
            }
        }
    }
}
