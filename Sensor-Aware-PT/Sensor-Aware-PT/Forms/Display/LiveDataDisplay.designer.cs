using OpenTK.Graphics;
namespace Sensor_Aware_PT
{
    partial class LiveDataDisplayForm
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
            this.btnCalibrate = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.debug_btn = new System.Windows.Forms.Button();
            this.cameraFocusDropdown = new System.Windows.Forms.ComboBox();
            this.txtDebug = new System.Windows.Forms.TextBox();
            this.simpleOpenGlControl = new Sensor_Aware_PT.Forms.AntiAliasedGLControl();
            this.btnSynchronize = new System.Windows.Forms.Button();
            this.btnPause = new System.Windows.Forms.Button();
            this.hScrollTime = new System.Windows.Forms.HScrollBar();
            this.SuspendLayout();
            // 
            // btnCalibrate
            // 
            this.btnCalibrate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCalibrate.Image = global::Sensor_Aware_PT.Properties.Resources._32_settings;
            this.btnCalibrate.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCalibrate.Location = new System.Drawing.Point(12, 12);
            this.btnCalibrate.Name = "btnCalibrate";
            this.btnCalibrate.Size = new System.Drawing.Size(125, 40);
            this.btnCalibrate.TabIndex = 3;
            this.btnCalibrate.Text = "Align to screen";
            this.btnCalibrate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnCalibrate.UseVisualStyleBackColor = true;
            this.btnCalibrate.Click += new System.EventHandler(this.btnCalibrate_Click);
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
            // debug_btn
            // 
            this.debug_btn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.debug_btn.Location = new System.Drawing.Point(328, 29);
            this.debug_btn.Name = "debug_btn";
            this.debug_btn.Size = new System.Drawing.Size(75, 23);
            this.debug_btn.TabIndex = 8;
            this.debug_btn.Text = "debug_out";
            this.debug_btn.UseVisualStyleBackColor = true;
            this.debug_btn.Visible = false;
            this.debug_btn.Click += new System.EventHandler(this.button4_Click);
            // 
            // cameraFocusDropdown
            // 
            this.cameraFocusDropdown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cameraFocusDropdown.Items.AddRange(new object[] {
            "Arms L",
            "Arms R",
            "Legs L",
            "Legs R",
            "Torso",
            "Hip",
            "Full Body"});
            this.cameraFocusDropdown.Location = new System.Drawing.Point(13, 241);
            this.cameraFocusDropdown.Name = "cameraFocusDropdown";
            this.cameraFocusDropdown.Size = new System.Drawing.Size(121, 21);
            this.cameraFocusDropdown.TabIndex = 9;
            this.cameraFocusDropdown.SelectedIndexChanged += new System.EventHandler(this.cameraFocusDropdown_SelectedIndexChanged);
            // 
            // txtDebug
            // 
            this.txtDebug.Location = new System.Drawing.Point(9, 289);
            this.txtDebug.Multiline = true;
            this.txtDebug.Name = "txtDebug";
            this.txtDebug.Size = new System.Drawing.Size(162, 91);
            this.txtDebug.TabIndex = 10;
            // 
            // simpleOpenGlControl
            // 
            this.simpleOpenGlControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.simpleOpenGlControl.BackColor = System.Drawing.Color.Black;
            this.simpleOpenGlControl.Location = new System.Drawing.Point(195, 67);
            this.simpleOpenGlControl.Name = "simpleOpenGlControl";
            this.simpleOpenGlControl.Size = new System.Drawing.Size(688, 479);
            this.simpleOpenGlControl.TabIndex = 7;
            this.simpleOpenGlControl.VSync = true;
            this.simpleOpenGlControl.Load += new System.EventHandler(this.simpleOpenGlControl_Load);
            this.simpleOpenGlControl.Scroll += new System.Windows.Forms.ScrollEventHandler(this.simpleOpenGlControl_Scroll);
            this.simpleOpenGlControl.SizeChanged += new System.EventHandler(this.simpleOpenGlControl_SizeChanged);
            this.simpleOpenGlControl.Paint += new System.Windows.Forms.PaintEventHandler(this.simpleOpenGlControl_Paint);
            this.simpleOpenGlControl.KeyDown += new System.Windows.Forms.KeyEventHandler(this.simpleOpenGlControl_KeyDown);
            this.simpleOpenGlControl.KeyUp += new System.Windows.Forms.KeyEventHandler(this.simpleOpenGlControl_KeyUp);
            this.simpleOpenGlControl.MouseDown += new System.Windows.Forms.MouseEventHandler(this.simpleOpenGlControl_MouseDown);
            this.simpleOpenGlControl.MouseMove += new System.Windows.Forms.MouseEventHandler(this.simpleOpenGlControl_MouseMove);
            this.simpleOpenGlControl.MouseUp += new System.Windows.Forms.MouseEventHandler(this.simpleOpenGlControl_MouseUp);
            this.simpleOpenGlControl.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.simpleOpenGlControl_MouseWheel);
            // 
            // btnSynchronize
            // 
            this.btnSynchronize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSynchronize.Image = global::Sensor_Aware_PT.Properties.Resources._32_refresh1;
            this.btnSynchronize.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSynchronize.Location = new System.Drawing.Point(143, 12);
            this.btnSynchronize.Name = "btnSynchronize";
            this.btnSynchronize.Size = new System.Drawing.Size(125, 40);
            this.btnSynchronize.TabIndex = 4;
            this.btnSynchronize.Text = "Resynchronize";
            this.btnSynchronize.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnSynchronize.UseVisualStyleBackColor = true;
            this.btnSynchronize.Click += new System.EventHandler(this.btnSynchronize_Click);
            // 
            // btnPause
            // 
            this.btnPause.Location = new System.Drawing.Point(466, 46);
            this.btnPause.Name = "btnPause";
            this.btnPause.Size = new System.Drawing.Size(75, 23);
            this.btnPause.TabIndex = 12;
            this.btnPause.Text = "paus";
            this.btnPause.UseVisualStyleBackColor = true;
            this.btnPause.Click += new System.EventHandler(this.btnPause_Click);
            // 
            // hScrollTime
            // 
            this.hScrollTime.Location = new System.Drawing.Point(459, 12);
            this.hScrollTime.Name = "hScrollTime";
            this.hScrollTime.Size = new System.Drawing.Size(182, 30);
            this.hScrollTime.TabIndex = 11;
            this.hScrollTime.Scroll += new System.Windows.Forms.ScrollEventHandler(this.hScrollTime_Scroll);
            // 
            // LiveDataDisplayForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(895, 558);
            this.Controls.Add(this.btnPause);
            this.Controls.Add(this.hScrollTime);
            this.Controls.Add(this.txtDebug);
            this.Controls.Add(this.cameraFocusDropdown);
            this.Controls.Add(this.debug_btn);
            this.Controls.Add(this.simpleOpenGlControl);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnSynchronize);
            this.Controls.Add(this.btnCalibrate);
            this.Name = "LiveDataDisplayForm";
            this.Text = "Sensor Aware PT Skeletal Viewer of Doom";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ExperimentalForm_FormClosing);
            this.Load += new System.EventHandler(this.ExperimentalForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.LiveDataDisplayForm_KeyDown);
            this.Resize += new System.EventHandler(this.LiveDataDisplayForm_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCalibrate;
        private System.Windows.Forms.Button btnSynchronize;
        private System.Windows.Forms.Label label1;
        private Forms.AntiAliasedGLControl simpleOpenGlControl;
        private System.Windows.Forms.Button debug_btn;
        private System.Windows.Forms.ComboBox cameraFocusDropdown;
        private System.Windows.Forms.TextBox txtDebug;
        private System.Windows.Forms.Button btnPause;
        private System.Windows.Forms.HScrollBar hScrollTime;

    }
}