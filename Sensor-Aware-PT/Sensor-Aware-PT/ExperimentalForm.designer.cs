namespace Sensor_Aware_PT
{
    partial class ExperimentalForm
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
            this.simpleOpenGlControl = new OpenTK.GLControl();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // simpleOpenGlControl
            // 
            this.simpleOpenGlControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.simpleOpenGlControl.BackColor = System.Drawing.Color.Black;
            this.simpleOpenGlControl.Location = new System.Drawing.Point(12, 67);
            this.simpleOpenGlControl.Name = "simpleOpenGlControl";
            this.simpleOpenGlControl.Size = new System.Drawing.Size(712, 497);
            this.simpleOpenGlControl.TabIndex = 2;
            this.simpleOpenGlControl.VSync = false;
            this.simpleOpenGlControl.Load += new System.EventHandler(this.simpleOpenGlControl_Load);
            this.simpleOpenGlControl.SizeChanged += new System.EventHandler(this.simpleOpenGlControl_SizeChanged);
            this.simpleOpenGlControl.Paint += new System.Windows.Forms.PaintEventHandler(this.simpleOpenGlControl_Paint);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(50, 29);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(166, 29);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 4;
            this.button2.Text = "button2";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // ExperimentalForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(724, 558);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.simpleOpenGlControl);
            this.Name = "ExperimentalForm";
            this.Text = "ExperimentalForm";
            this.Load += new System.EventHandler(this.ExperimentalForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private OpenTK.GLControl simpleOpenGlControl;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;

    }
}