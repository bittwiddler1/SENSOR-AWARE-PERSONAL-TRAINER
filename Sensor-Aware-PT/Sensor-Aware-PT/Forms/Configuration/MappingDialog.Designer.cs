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
            this.mQuitButton = new System.Windows.Forms.Button();
            this.mSaveButton = new System.Windows.Forms.Button();
            this.mPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mSplitter)).BeginInit();
            this.mSplitter.Panel1.SuspendLayout();
            this.mSplitter.Panel2.SuspendLayout();
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
            // 
            // mSplitter.Panel2
            // 
            this.mSplitter.Panel2.Controls.Add(this.mQuitButton);
            this.mSplitter.Panel2.Controls.Add(this.mSaveButton);
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
            // mQuitButton
            // 
            this.mQuitButton.Dock = System.Windows.Forms.DockStyle.Right;
            this.mQuitButton.Location = new System.Drawing.Point(142, 0);
            this.mQuitButton.Name = "mQuitButton";
            this.mQuitButton.Size = new System.Drawing.Size(150, 69);
            this.mQuitButton.TabIndex = 1;
            this.mQuitButton.Text = "Quit";
            this.mQuitButton.UseVisualStyleBackColor = true;
            this.mQuitButton.Click += new System.EventHandler(this.mQuitButton_Click);
            // 
            // mSaveButton
            // 
            this.mSaveButton.Dock = System.Windows.Forms.DockStyle.Left;
            this.mSaveButton.Location = new System.Drawing.Point(0, 0);
            this.mSaveButton.Name = "mSaveButton";
            this.mSaveButton.Size = new System.Drawing.Size(136, 69);
            this.mSaveButton.TabIndex = 0;
            this.mSaveButton.Text = "Save";
            this.mSaveButton.UseVisualStyleBackColor = true;
            this.mSaveButton.Click += new System.EventHandler(this.mSaveButton_Click);
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
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MappingDialog_FormClosing);
            this.mPanel.ResumeLayout(false);
            this.mSplitter.Panel1.ResumeLayout(false);
            this.mSplitter.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.mSplitter)).EndInit();
            this.mSplitter.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel mPanel;
        private System.Windows.Forms.SplitContainer mSplitter;
        private System.Windows.Forms.TabControl mTabControl;
        private System.Windows.Forms.Button mQuitButton;
        private System.Windows.Forms.Button mSaveButton;

    }
}
