namespace Sensor_Aware_PT
{
    partial class RawDataForm
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
            this.sensorLabel = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.yawLabel = new System.Windows.Forms.Label();
            this.pitchLabel = new System.Windows.Forms.Label();
            this.rollLabel = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // sensorLabel
            // 
            this.sensorLabel.AutoSize = true;
            this.sensorLabel.Location = new System.Drawing.Point(12, 9);
            this.sensorLabel.Name = "sensorLabel";
            this.sensorLabel.Size = new System.Drawing.Size(35, 13);
            this.sensorLabel.TabIndex = 0;
            this.sensorLabel.Text = "label1";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.yawLabel, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.pitchLabel, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.rollLabel, 2, 2);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(15, 26);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(200, 100);
            this.tableLayoutPanel1.TabIndex = 1;
            this.tableLayoutPanel1.Paint += new System.Windows.Forms.PaintEventHandler(this.tableLayoutPanel1_Paint);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(28, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Yaw";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 13);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(31, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Pitch";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 26);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(25, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Roll";
            // 
            // yawLabel
            // 
            this.yawLabel.AutoSize = true;
            this.yawLabel.Location = new System.Drawing.Point(40, 0);
            this.yawLabel.Name = "yawLabel";
            this.yawLabel.Size = new System.Drawing.Size(35, 13);
            this.yawLabel.TabIndex = 3;
            this.yawLabel.Text = "label4";
            // 
            // pitchLabel
            // 
            this.pitchLabel.AutoSize = true;
            this.pitchLabel.Location = new System.Drawing.Point(40, 13);
            this.pitchLabel.Name = "pitchLabel";
            this.pitchLabel.Size = new System.Drawing.Size(35, 13);
            this.pitchLabel.TabIndex = 4;
            this.pitchLabel.Text = "label4";
            // 
            // rollLabel
            // 
            this.rollLabel.AutoSize = true;
            this.rollLabel.Location = new System.Drawing.Point(40, 26);
            this.rollLabel.Name = "rollLabel";
            this.rollLabel.Size = new System.Drawing.Size(35, 13);
            this.rollLabel.TabIndex = 5;
            this.rollLabel.Text = "label4";
            // 
            // RawDataForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.sensorLabel);
            this.Name = "RawDataForm";
            this.Text = "RawDataForm";
            this.Load += new System.EventHandler(this.RawDataForm_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label sensorLabel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label yawLabel;
        private System.Windows.Forms.Label pitchLabel;
        private System.Windows.Forms.Label rollLabel;
    }
}