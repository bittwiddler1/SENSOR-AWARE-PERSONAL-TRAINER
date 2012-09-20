namespace Sensor_Aware_PT
{
    partial class ConfigurationDialog
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
            this.mTabControl = new System.Windows.Forms.TabControl();
            this.mFakeProgressBar = new System.Windows.Forms.ProgressBar();
            this.mScanLabel = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.mPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // mPanel
            // 
            this.mPanel.Controls.Add(this.mTabControl);
            this.mPanel.Controls.Add(this.mFakeProgressBar);
            this.mPanel.Controls.Add(this.mScanLabel);
            this.mPanel.Controls.Add(this.statusStrip1);
            this.mPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mPanel.Location = new System.Drawing.Point(0, 0);
            this.mPanel.Name = "mPanel";
            this.mPanel.Size = new System.Drawing.Size(292, 273);
            this.mPanel.TabIndex = 0;
            // 
            // mTabControl
            // 
            this.mTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mTabControl.Location = new System.Drawing.Point(0, 0);
            this.mTabControl.Name = "mTabControl";
            this.mTabControl.SelectedIndex = 0;
            this.mTabControl.Size = new System.Drawing.Size(292, 251);
            this.mTabControl.TabIndex = 3;
            // 
            // mFakeProgressBar
            // 
            this.mFakeProgressBar.Location = new System.Drawing.Point(93, 106);
            this.mFakeProgressBar.Name = "mFakeProgressBar";
            this.mFakeProgressBar.Size = new System.Drawing.Size(100, 23);
            this.mFakeProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.mFakeProgressBar.TabIndex = 2;
            this.mFakeProgressBar.Visible = false;
            // 
            // mScanLabel
            // 
            this.mScanLabel.AutoSize = true;
            this.mScanLabel.Location = new System.Drawing.Point(90, 90);
            this.mScanLabel.Name = "mScanLabel";
            this.mScanLabel.Size = new System.Drawing.Size(108, 13);
            this.mScanLabel.TabIndex = 1;
            this.mScanLabel.Text = "Scanning for Sensors";
            this.mScanLabel.Visible = false;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 251);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(292, 22);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // ConfigurationDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 273);
            this.Controls.Add(this.mPanel);
            this.Name = "ConfigurationDialog";
            this.Text = "ConfigurationDialog";
            this.mPanel.ResumeLayout(false);
            this.mPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel mPanel;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ProgressBar mFakeProgressBar;
        private System.Windows.Forms.Label mScanLabel;
        internal System.Windows.Forms.TabControl mTabControl;
    }
}