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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sensorsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mappingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnLiveView
            // 
            this.btnLiveView.Location = new System.Drawing.Point(91, 60);
            this.btnLiveView.Name = "btnLiveView";
            this.btnLiveView.Size = new System.Drawing.Size(199, 68);
            this.btnLiveView.TabIndex = 0;
            this.btnLiveView.Text = "Measure ROM";
            this.btnLiveView.UseVisualStyleBackColor = true;
            this.btnLiveView.Click += new System.EventHandler(this.btnLiveView_Click);
            // 
            // btnRecorder
            // 
            this.btnRecorder.Location = new System.Drawing.Point(91, 134);
            this.btnRecorder.Name = "btnRecorder";
            this.btnRecorder.Size = new System.Drawing.Size(199, 66);
            this.btnRecorder.TabIndex = 1;
            this.btnRecorder.Text = "Motion Capture";
            this.btnRecorder.UseVisualStyleBackColor = true;
            this.btnRecorder.Click += new System.EventHandler(this.btnRecorder_Click);
            // 
            // btnMappings
            // 
            this.btnMappings.Location = new System.Drawing.Point(91, 206);
            this.btnMappings.Name = "btnMappings";
            this.btnMappings.Size = new System.Drawing.Size(199, 64);
            this.btnMappings.TabIndex = 2;
            this.btnMappings.Text = "Measure Limb Stability";
            this.btnMappings.UseVisualStyleBackColor = true;
            this.btnMappings.Click += new System.EventHandler(this.btnStability_Click);
            // 
            // sensorListView
            // 
            this.sensorListView.Location = new System.Drawing.Point(12, 293);
            this.sensorListView.Name = "sensorListView";
            this.sensorListView.Size = new System.Drawing.Size(358, 44);
            this.sensorListView.TabIndex = 3;
            this.sensorListView.UseCompatibleStateImageBehavior = false;
            this.sensorListView.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
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
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sensorsToolStripMenuItem,
            this.mappingsToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.viewToolStripMenuItem.Text = "Settings";
            // 
            // quitToolStripMenuItem
            // 
            this.quitToolStripMenuItem.Name = "quitToolStripMenuItem";
            this.quitToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.quitToolStripMenuItem.Text = "Quit";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.aboutToolStripMenuItem.Text = "About";
            // 
            // sensorsToolStripMenuItem
            // 
            this.sensorsToolStripMenuItem.Name = "sensorsToolStripMenuItem";
            this.sensorsToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.sensorsToolStripMenuItem.Text = "Sensors...";
            this.sensorsToolStripMenuItem.Click += new System.EventHandler(this.sensorsToolStripMenuItem_Click);
            // 
            // mappingsToolStripMenuItem
            // 
            this.mappingsToolStripMenuItem.Name = "mappingsToolStripMenuItem";
            this.mappingsToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.mappingsToolStripMenuItem.Text = "Mappings....";
            this.mappingsToolStripMenuItem.Click += new System.EventHandler(this.mappingsToolStripMenuItem_Click);
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

