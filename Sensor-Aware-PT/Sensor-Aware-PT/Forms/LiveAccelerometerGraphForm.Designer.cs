namespace Sensor_Aware_PT.Forms
{
    partial class LiveAccelerometerGraphForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing )
        {
            if( disposing && ( components != null ) )
            {
                components.Dispose();
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.antiAliasedGLControl1 = new Sensor_Aware_PT.Forms.AntiAliasedGLControl();
            this.SuspendLayout();
            // 
            // antiAliasedGLControl1
            // 
            this.antiAliasedGLControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.antiAliasedGLControl1.BackColor = System.Drawing.Color.Black;
            this.antiAliasedGLControl1.Location = new System.Drawing.Point(12, 12);
            this.antiAliasedGLControl1.Name = "antiAliasedGLControl1";
            this.antiAliasedGLControl1.Size = new System.Drawing.Size(278, 349);
            this.antiAliasedGLControl1.TabIndex = 0;
            this.antiAliasedGLControl1.VSync = true;
            this.antiAliasedGLControl1.Load += new System.EventHandler(this.antiAliasedGLControl1_Load);
            this.antiAliasedGLControl1.SizeChanged += new System.EventHandler(this.antiAliasedGLControl1_SizeChanged);
            this.antiAliasedGLControl1.Paint += new System.Windows.Forms.PaintEventHandler(this.antiAliasedGLControl1_Paint);
            // 
            // LiveAccelerometerGraphForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(302, 373);
            this.Controls.Add(this.antiAliasedGLControl1);
            this.Name = "LiveAccelerometerGraphForm";
            this.Text = "LiveAccelerometerGraphForm";
            this.ResumeLayout(false);

        }

        #endregion

        private AntiAliasedGLControl antiAliasedGLControl1;
    }
}