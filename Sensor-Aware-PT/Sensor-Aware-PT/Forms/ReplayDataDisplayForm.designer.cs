using OpenTK.Graphics;
namespace Sensor_Aware_PT
{
    partial class ReplayDataDisplayForm
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
            this.button3 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.simpleOpenGlControl = new Sensor_Aware_PT.Forms.AntiAliasedGLControl();
            this.hScrollTime = new System.Windows.Forms.HScrollBar();
            this.btnPause = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(268, 29);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 5;
            this.button3.Text = "Spit";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label1.Location = new System.Drawing.Point(9, 67);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(180, 160);
            this.label1.TabIndex = 6;
            this.label1.Text = "Controls\r\n\r\nRotate X - Q, W\r\nRotate Y - A, S\r\nRotate Z - Z, X\r\n\r\nToggle BoundingB" +
    "ox - E\r\nToggle Wireframe - R";
            // 
            // simpleOpenGlControl
            // 
            this.simpleOpenGlControl.BackColor = System.Drawing.Color.Black;
            this.simpleOpenGlControl.Location = new System.Drawing.Point(195, 67);
            this.simpleOpenGlControl.Name = "simpleOpenGlControl";
            this.simpleOpenGlControl.Size = new System.Drawing.Size(703, 490);
            this.simpleOpenGlControl.TabIndex = 7;
            this.simpleOpenGlControl.VSync = true;
            this.simpleOpenGlControl.Load += new System.EventHandler(this.simpleOpenGlControl_Load);
            this.simpleOpenGlControl.SizeChanged += new System.EventHandler(this.simpleOpenGlControl_SizeChanged);
            this.simpleOpenGlControl.Paint += new System.Windows.Forms.PaintEventHandler(this.simpleOpenGlControl_Paint);
            this.simpleOpenGlControl.KeyDown += new System.Windows.Forms.KeyEventHandler(this.simpleOpenGlControl_KeyDown);
            this.simpleOpenGlControl.KeyUp += new System.Windows.Forms.KeyEventHandler(this.simpleOpenGlControl_KeyUp);
            this.simpleOpenGlControl.MouseDown += new System.Windows.Forms.MouseEventHandler(this.simpleOpenGlControl_MouseDown);
            this.simpleOpenGlControl.MouseMove += new System.Windows.Forms.MouseEventHandler(this.simpleOpenGlControl_MouseMove);
            this.simpleOpenGlControl.MouseUp += new System.Windows.Forms.MouseEventHandler(this.simpleOpenGlControl_MouseUp);
            this.simpleOpenGlControl.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.simpleOpenGlControl_MouseWheel);
            // 
            // hScrollTime
            // 
            this.hScrollTime.Location = new System.Drawing.Point(6, 277);
            this.hScrollTime.Name = "hScrollTime";
            this.hScrollTime.Size = new System.Drawing.Size(182, 30);
            this.hScrollTime.TabIndex = 8;
            this.hScrollTime.Scroll += new System.Windows.Forms.ScrollEventHandler(this.hScrollTime_Scroll);
            // 
            // btnPause
            // 
            this.btnPause.Location = new System.Drawing.Point(13, 311);
            this.btnPause.Name = "btnPause";
            this.btnPause.Size = new System.Drawing.Size(75, 23);
            this.btnPause.TabIndex = 9;
            this.btnPause.Text = "paus";
            this.btnPause.UseVisualStyleBackColor = true;
            this.btnPause.Click += new System.EventHandler(this.btnPause_Click);
            // 
            // ReplayDataDisplayForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(895, 558);
            this.Controls.Add(this.btnPause);
            this.Controls.Add(this.hScrollTime);
            this.Controls.Add(this.simpleOpenGlControl);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button3);
            this.Name = "ReplayDataDisplayForm";
            this.Text = "Sensor Aware PT Skeletal Viewer of Doom";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ExperimentalForm_FormClosing);
            this.Load += new System.EventHandler(this.ReplayDataDisplayForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.simpleOpenGlControl_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.simpleOpenGlControl_KeyUp);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.simpleOpenGlControl_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.simpleOpenGlControl_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.simpleOpenGlControl_MouseUp);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Label label1;
        private Forms.AntiAliasedGLControl simpleOpenGlControl;
        private System.Windows.Forms.HScrollBar hScrollTime;
        private System.Windows.Forms.Button btnPause;

    }
}