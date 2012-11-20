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
            this.sensorListView = new System.Windows.Forms.ListView();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mappingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnMappings = new System.Windows.Forms.Button();
            this.btnRecorder = new System.Windows.Forms.Button();
            this.btnLiveView = new System.Windows.Forms.Button();
            this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sensorsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // sensorListView
            // 
            this.sensorListView.Location = new System.Drawing.Point(33, 270);
            this.sensorListView.Name = "sensorListView";
            this.sensorListView.Size = new System.Drawing.Size(315, 64);
            this.sensorListView.TabIndex = 3;
            this.sensorListView.UseCompatibleStateImageBehavior = false;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(382, 24);
            this.menuStrip1.TabIndex = 4;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.quitToolStripMenuItem});
            this.fileToolStripMenuItem.Image = global::Sensor_Aware_PT.Properties.Resources._16_info;
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(53, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sensorsToolStripMenuItem,
            this.mappingsToolStripMenuItem});
            this.viewToolStripMenuItem.Image = global::Sensor_Aware_PT.Properties.Resources._16_settings;
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(77, 20);
            this.viewToolStripMenuItem.Text = "Settings";
            // 
            // mappingsToolStripMenuItem
            // 
            this.mappingsToolStripMenuItem.Image = global::Sensor_Aware_PT.Properties.Resources._16_map;
            this.mappingsToolStripMenuItem.Name = "mappingsToolStripMenuItem";
            this.mappingsToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.mappingsToolStripMenuItem.Text = "Mappings....";
            this.mappingsToolStripMenuItem.Click += new System.EventHandler(this.mappingsToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Image = global::Sensor_Aware_PT.Properties.Resources._16_info;
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(60, 20);
            this.helpToolStripMenuItem.Text = "Help";
            this.helpToolStripMenuItem.Click += new System.EventHandler(this.helpToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Image = global::Sensor_Aware_PT.Properties.Resources._16_faq;
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.aboutToolStripMenuItem.Text = "About";
            // 
            // btnMappings
            // 
            this.btnMappings.Image = global::Sensor_Aware_PT.Properties.Resources._64_pie_graph;
            this.btnMappings.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnMappings.Location = new System.Drawing.Point(91, 186);
            this.btnMappings.Name = "btnMappings";
            this.btnMappings.Size = new System.Drawing.Size(190, 64);
            this.btnMappings.TabIndex = 2;
            this.btnMappings.Text = "Measure Limb Stability";
            this.btnMappings.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnMappings.UseVisualStyleBackColor = true;
            this.btnMappings.Click += new System.EventHandler(this.btnStability_Click);
            // 
            // btnRecorder
            // 
            this.btnRecorder.Image = global::Sensor_Aware_PT.Properties.Resources._64_catalogue;
            this.btnRecorder.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnRecorder.Location = new System.Drawing.Point(91, 116);
            this.btnRecorder.Name = "btnRecorder";
            this.btnRecorder.Size = new System.Drawing.Size(190, 64);
            this.btnRecorder.TabIndex = 1;
            this.btnRecorder.Text = "Motion Capture";
            this.btnRecorder.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnRecorder.UseVisualStyleBackColor = true;
            this.btnRecorder.Click += new System.EventHandler(this.btnRecorder_Click);
            // 
            // btnLiveView
            // 
            this.btnLiveView.Image = global::Sensor_Aware_PT.Properties.Resources._64_settings;
            this.btnLiveView.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnLiveView.Location = new System.Drawing.Point(91, 46);
            this.btnLiveView.Name = "btnLiveView";
            this.btnLiveView.Size = new System.Drawing.Size(190, 64);
            this.btnLiveView.TabIndex = 0;
            this.btnLiveView.Text = "Measure ROM";
            this.btnLiveView.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnLiveView.UseVisualStyleBackColor = true;
            this.btnLiveView.Click += new System.EventHandler(this.btnLiveView_Click);
            // 
            // quitToolStripMenuItem
            // 
            this.quitToolStripMenuItem.Image = global::Sensor_Aware_PT.Properties.Resources._16_close;
            this.quitToolStripMenuItem.Name = "quitToolStripMenuItem";
            this.quitToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.quitToolStripMenuItem.Text = "Quit";
            // 
            // sensorsToolStripMenuItem
            // 
            this.sensorsToolStripMenuItem.Image = global::Sensor_Aware_PT.Properties.Resources._16_settings;
            this.sensorsToolStripMenuItem.Name = "sensorsToolStripMenuItem";
            this.sensorsToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.sensorsToolStripMenuItem.Text = "Sensors...";
            this.sensorsToolStripMenuItem.Click += new System.EventHandler(this.sensorsToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(382, 349);
            this.Controls.Add(this.sensorListView);
            this.Controls.Add(this.btnMappings);
            this.Controls.Add(this.btnRecorder);
            this.Controls.Add(this.btnLiveView);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "Sensor Aware PT";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnLiveView;
        private System.Windows.Forms.Button btnRecorder;
        private System.Windows.Forms.Button btnMappings;
        private System.Windows.Forms.ListView sensorListView;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sensorsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mappingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
    }
}

