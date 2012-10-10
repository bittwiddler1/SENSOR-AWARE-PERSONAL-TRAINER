namespace Sensor_Aware_PT
{
    partial class MappingDialog
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
            this.mPanel = new System.Windows.Forms.Panel();
            this.mSplitter = new System.Windows.Forms.SplitContainer();
            this.mTabControl = new System.Windows.Forms.TabControl();
            this.mPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mSplitter)).BeginInit();
            this.mSplitter.Panel1.SuspendLayout();
            this.mSplitter.SuspendLayout();
            this.SuspendLayout();
            // 
            // mPanel
            // 
            this.mPanel.Controls.Add(this.mSplitter);
            this.mPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mPanel.Location = new System.Drawing.Point(0, 0);
            this.mPanel.Name = "mPanel";
            this.mPanel.Size = new System.Drawing.Size(292, 273);
            this.mPanel.TabIndex = 0;
            // 
            // mSplitter
            // 
            this.mSplitter.IsSplitterFixed = true;
            this.mSplitter.Location = new System.Drawing.Point(0, 0);
            this.mSplitter.Name = "mSplitter";
            this.mSplitter.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // mSplitter.Panel1
            // 
            this.mSplitter.Panel1.Controls.Add(this.mTabControl);
            this.mSplitter.Size = new System.Drawing.Size(292, 273);
            this.mSplitter.SplitterDistance = 200;
            this.mSplitter.TabIndex = 0;
            // 
            // mTabControl
            // 
            this.mTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mTabControl.Location = new System.Drawing.Point(0, 0);
            this.mTabControl.Name = "mTabControl";
            this.mTabControl.SelectedIndex = 0;
            this.mTabControl.Size = new System.Drawing.Size(292, 200);
            this.mTabControl.TabIndex = 0;
            // 
            // MappingDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 273);
            this.Controls.Add(this.mPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MappingDialog";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "MappingDialog";
            this.mPanel.ResumeLayout(false);
            this.mSplitter.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.mSplitter)).EndInit();
            this.mSplitter.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel mPanel;
        private System.Windows.Forms.SplitContainer mSplitter;
        private System.Windows.Forms.TabControl mTabControl;

    }
}
