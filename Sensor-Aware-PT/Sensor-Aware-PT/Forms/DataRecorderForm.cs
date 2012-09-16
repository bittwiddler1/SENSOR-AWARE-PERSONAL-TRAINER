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

            mRecorder = new SensorDataRecorder();
        }

        private void btnRecord_Click( object sender, EventArgs e )
        {
            if( !init )
            {
                
                init = true;
            }

            if( !mRecorder.IsRecording )
            {
                mRecorder.beginRecording();
                dataGridView1.DataSource = null;
                btnRecord.Text = "Stop Recording";
                this.Text = "RECORDING!!";
            }
            else
            {
                List<SensorDataEntry> datas = mRecorder.stopRecording();
                btnRecord.Text = "Start Recording";
                this.Text = "IDLE";
                BindingList<SensorDataEntry> blist = new BindingList<SensorDataEntry>( datas );
                dataGridView1.DataSource = blist;
            }
        }

        private void btnSave_Click( object sender, EventArgs e )
        {
            if( mRecorder.HasData )
            {
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Sensor Data|*.imu";
                saveDialog.Title = "Save sensor data";
                DialogResult result = saveDialog.ShowDialog();

                if( result == System.Windows.Forms.DialogResult.OK )
                {
                    mRecorder.saveRecording( saveDialog.FileName );
                }
            }
            else
            {
                MessageBox.Show( "You haven't recorded any data yet, so what exactly are you trying to save, sir?",
                    "YOU DUMBASS", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error );
            }
        }

        private void btnReplay_Click( object sender, EventArgs e )
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "Sensor Data|*.imu";
            openDialog.Title = "Load sensor replay data";

            DialogResult result = openDialog.ShowDialog();

            if( result == System.Windows.Forms.DialogResult.OK )
            {
                SensorDataPlayer sdp = new SensorDataPlayer();
                ExperimentalForm EF = new ExperimentalForm();
                EF.subscribeToSource( sdp );
                EF.Show();

                BackgroundWorker bg = new BackgroundWorker();

                bg.DoWork += new DoWorkEventHandler( delegate
                {
                    sdp.replayFile( openDialog.FileName );

                } );

                bg.RunWorkerAsync();

                
                
            }
        }
    }
}
