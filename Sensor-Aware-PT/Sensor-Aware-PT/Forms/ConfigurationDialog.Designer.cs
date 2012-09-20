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
            this.mFakeProgressBar = new System.Windows.Forms.ProgressBar();
            this.mScanLabel = new System.Windows.Forms.Label();
            this.mPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // mPanel
            // 
            this.mPanel.Controls.Add(this.mFakeProgressBar);
            this.mPanel.Controls.Add(this.mScanLabel);
            this.mPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mPanel.Location = new System.Drawing.Point(0, 0);
            this.mPanel.Name = "mPanel";
            this.mPanel.Size = new System.Drawing.Size(292, 273);
            this.mPanel.TabIndex = 0;
            // 
            // mFakeProgressBar
            // 
            this.mFakeProgressBar.Location = new System.Drawing.Point(93, 106);
            this.mFakeProgressBar.Name = "mFakeProgressBar";
            this.mFakeProgressBar.Size = new System.Drawing.Size(100, 23);
            this.mFakeProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.mFakeProgressBar.TabIndex = 2;
            // 
            // mScanLabel
            // 
            this.mScanLabel.AutoSize = true;
            this.mScanLabel.Location = new System.Drawing.Point(90, 90);
            this.mScanLabel.Name = "mScanLabel";
            this.mScanLabel.Size = new System.Drawing.Size(108, 13);
            this.mScanLabel.TabIndex = 1;
            this.mScanLabel.Text = "Scanning for Sensors";
            // 
            // ConfigurationDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 273);
            this.Controls.Add(this.mPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConfigurationDialog";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "ConfigurationDialog";
            this.mPanel.ResumeLayout(false);
            this.mPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel mPanel;
        private System.Windows.Forms.ProgressBar mFakeProgressBar;
        private System.Windows.Forms.Label mScanLabel;

    }
}