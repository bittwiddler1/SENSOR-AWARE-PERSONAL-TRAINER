namespace Sensor_Aware_PT
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnLiveView = new System.Windows.Forms.Button();
            this.btnRecorder = new System.Windows.Forms.Button();
            this.btnMappings = new System.Windows.Forms.Button();
            this.sensorListView = new System.Windows.Forms.ListView();
            this.btnGraph = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnLiveView
            // 
            this.btnLiveView.Location = new System.Drawing.Point(91, 12);
            this.btnLiveView.Name = "btnLiveView";
            this.btnLiveView.Size = new System.Drawing.Size(199, 68);
            this.btnLiveView.TabIndex = 0;
            this.btnLiveView.Text = "Skeletal Viewer";
            this.btnLiveView.UseVisualStyleBackColor = true;
            this.btnLiveView.Click += new System.EventHandler(this.btnLiveView_Click);
            // 
            // btnRecorder
            // 
            this.btnRecorder.Location = new System.Drawing.Point(91, 86);
            this.btnRecorder.Name = "btnRecorder";
            this.btnRecorder.Size = new System.Drawing.Size(199, 66);
            this.btnRecorder.TabIndex = 1;
            this.btnRecorder.Text = "Record/Replayer";
            this.btnRecorder.UseVisualStyleBackColor = true;
            this.btnRecorder.Click += new System.EventHandler(this.btnRecorder_Click);
            // 
            // btnMappings
            // 
            this.btnMappings.Location = new System.Drawing.Point(91, 158);
            this.btnMappings.Name = "btnMappings";
            this.btnMappings.Size = new System.Drawing.Size(199, 72);
            this.btnMappings.TabIndex = 2;
            this.btnMappings.Text = "Mappings";
            this.btnMappings.UseVisualStyleBackColor = true;
            this.btnMappings.Click += new System.EventHandler(this.btnMappings_Click);
            // 
            // sensorListView
            // 
            this.sensorListView.Location = new System.Drawing.Point(91, 240);
            this.sensorListView.Name = "sensorListView";
            this.sensorListView.Size = new System.Drawing.Size(199, 97);
            this.sensorListView.TabIndex = 3;
            this.sensorListView.UseCompatibleStateImageBehavior = false;
            this.sensorListView.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            // 
            // btnGraph
            // 
            this.btnGraph.Location = new System.Drawing.Point(299, 12);
            this.btnGraph.Name = "btnGraph";
            this.btnGraph.Size = new System.Drawing.Size(109, 68);
            this.btnGraph.TabIndex = 4;
            this.btnGraph.Text = "Graph";
            this.btnGraph.UseVisualStyleBackColor = true;
            this.btnGraph.Click += new System.EventHandler(this.btnGraph_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(420, 349);
            this.Controls.Add(this.btnGraph);
            this.Controls.Add(this.sensorListView);
            this.Controls.Add(this.btnMappings);
            this.Controls.Add(this.btnRecorder);
            this.Controls.Add(this.btnLiveView);
            this.Name = "MainForm";
            this.Text = "Sensor Aware PT";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnLiveView;
        private System.Windows.Forms.Button btnRecorder;
        private System.Windows.Forms.Button btnMappings;
        private System.Windows.Forms.ListView sensorListView;
        private System.Windows.Forms.Button btnGraph;



    }
}

