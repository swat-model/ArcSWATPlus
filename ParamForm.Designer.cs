namespace ArcSWATPlus
{
    partial class ParamForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ParamForm));
            this.SWATPlusBox = new System.Windows.Forms.TextBox();
            this.SWATPlusButton = new System.Windows.Forms.Button();
            this.MPIBox = new System.Windows.Forms.TextBox();
            this.MPIButton = new System.Windows.Forms.Button();
            this.checkUseMPI = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.MPILabel = new System.Windows.Forms.Label();
            this.groupBox = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.depthExp = new System.Windows.Forms.TextBox();
            this.widthExp = new System.Windows.Forms.TextBox();
            this.depthMult = new System.Windows.Forms.TextBox();
            this.widthMult = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.mainLengthMultiplier = new System.Windows.Forms.NumericUpDown();
            this.meanSlopeMultiplier = new System.Windows.Forms.NumericUpDown();
            this.tributarySlopeMultiplier = new System.Windows.Forms.NumericUpDown();
            this.tributaryLengthMultiplier = new System.Windows.Forms.NumericUpDown();
            this.reachSlopeMultiplier = new System.Windows.Forms.NumericUpDown();
            this.burninDepth = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.upslopeHRUDrain = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.pointSizeBox = new System.Windows.Forms.NumericUpDown();
            this.label14 = new System.Windows.Forms.Label();
            this.saveButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.groupBox.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mainLengthMultiplier)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.meanSlopeMultiplier)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tributarySlopeMultiplier)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tributaryLengthMultiplier)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.reachSlopeMultiplier)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pointSizeBox)).BeginInit();
            this.SuspendLayout();
            // 
            // SWATPlusBox
            // 
            this.SWATPlusBox.Location = new System.Drawing.Point(12, 36);
            this.SWATPlusBox.Name = "SWATPlusBox";
            this.SWATPlusBox.Size = new System.Drawing.Size(324, 23);
            this.SWATPlusBox.TabIndex = 0;
            // 
            // SWATPlusButton
            // 
            this.SWATPlusButton.Location = new System.Drawing.Point(342, 36);
            this.SWATPlusButton.Name = "SWATPlusButton";
            this.SWATPlusButton.Size = new System.Drawing.Size(44, 23);
            this.SWATPlusButton.TabIndex = 1;
            this.SWATPlusButton.Text = "...";
            this.SWATPlusButton.UseVisualStyleBackColor = true;
            this.SWATPlusButton.Click += new System.EventHandler(this.SWATPlusButton_Click);
            // 
            // MPIBox
            // 
            this.MPIBox.Location = new System.Drawing.Point(12, 101);
            this.MPIBox.Name = "MPIBox";
            this.MPIBox.Size = new System.Drawing.Size(324, 23);
            this.MPIBox.TabIndex = 2;
            // 
            // MPIButton
            // 
            this.MPIButton.Location = new System.Drawing.Point(342, 101);
            this.MPIButton.Name = "MPIButton";
            this.MPIButton.Size = new System.Drawing.Size(44, 23);
            this.MPIButton.TabIndex = 3;
            this.MPIButton.Text = "...";
            this.MPIButton.UseVisualStyleBackColor = true;
            this.MPIButton.Click += new System.EventHandler(this.MPIButton_Click);
            // 
            // checkUseMPI
            // 
            this.checkUseMPI.AutoSize = true;
            this.checkUseMPI.Location = new System.Drawing.Point(13, 74);
            this.checkUseMPI.Name = "checkUseMPI";
            this.checkUseMPI.Size = new System.Drawing.Size(69, 19);
            this.checkUseMPI.TabIndex = 4;
            this.checkUseMPI.Text = "Use MPI";
            this.checkUseMPI.UseVisualStyleBackColor = true;
            this.checkUseMPI.CheckedChanged += new System.EventHandler(this.checkUseMPI_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(134, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(109, 15);
            this.label1.TabIndex = 5;
            this.label1.Text = "SWATPlus directory";
            // 
            // MPILabel
            // 
            this.MPILabel.AutoSize = true;
            this.MPILabel.Location = new System.Drawing.Point(134, 76);
            this.MPILabel.Name = "MPILabel";
            this.MPILabel.Size = new System.Drawing.Size(102, 15);
            this.MPILabel.TabIndex = 6;
            this.MPILabel.Text = "MPI bin dorectory";
            // 
            // groupBox
            // 
            this.groupBox.Controls.Add(this.label6);
            this.groupBox.Controls.Add(this.label5);
            this.groupBox.Controls.Add(this.label4);
            this.groupBox.Controls.Add(this.label3);
            this.groupBox.Controls.Add(this.depthExp);
            this.groupBox.Controls.Add(this.widthExp);
            this.groupBox.Controls.Add(this.depthMult);
            this.groupBox.Controls.Add(this.widthMult);
            this.groupBox.Location = new System.Drawing.Point(13, 143);
            this.groupBox.Name = "groupBox";
            this.groupBox.Size = new System.Drawing.Size(373, 102);
            this.groupBox.TabIndex = 7;
            this.groupBox.TabStop = false;
            this.groupBox.Text = "Channel widths and depths";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(210, 66);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(85, 30);
            this.label6.TabIndex = 7;
            this.label6.Text = "Channel depth\r\nexponent\r\n";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(22, 66);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(85, 30);
            this.label5.TabIndex = 6;
            this.label5.Text = "Channel depth\r\nmultiplier\r\n";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(210, 24);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(84, 30);
            this.label4.TabIndex = 5;
            this.label4.Text = "Channel width\r\nexponent\r\n";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(22, 24);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(84, 30);
            this.label3.TabIndex = 4;
            this.label3.Text = "Channel width\r\nmultiplier\r\n";
            // 
            // depthExp
            // 
            this.depthExp.Location = new System.Drawing.Point(300, 73);
            this.depthExp.Name = "depthExp";
            this.depthExp.Size = new System.Drawing.Size(53, 23);
            this.depthExp.TabIndex = 3;
            // 
            // widthExp
            // 
            this.widthExp.Location = new System.Drawing.Point(300, 28);
            this.widthExp.Name = "widthExp";
            this.widthExp.Size = new System.Drawing.Size(53, 23);
            this.widthExp.TabIndex = 2;
            // 
            // depthMult
            // 
            this.depthMult.Location = new System.Drawing.Point(113, 73);
            this.depthMult.Name = "depthMult";
            this.depthMult.Size = new System.Drawing.Size(53, 23);
            this.depthMult.TabIndex = 1;
            // 
            // widthMult
            // 
            this.widthMult.Location = new System.Drawing.Point(112, 28);
            this.widthMult.Name = "widthMult";
            this.widthMult.Size = new System.Drawing.Size(53, 23);
            this.widthMult.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label11);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.mainLengthMultiplier);
            this.groupBox1.Controls.Add(this.meanSlopeMultiplier);
            this.groupBox1.Controls.Add(this.tributarySlopeMultiplier);
            this.groupBox1.Controls.Add(this.tributaryLengthMultiplier);
            this.groupBox1.Controls.Add(this.reachSlopeMultiplier);
            this.groupBox1.Location = new System.Drawing.Point(20, 263);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(366, 100);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Slope and length multipliers";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(189, 63);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(98, 30);
            this.label11.TabIndex = 10;
            this.label11.Text = "Tributary channel\r\nlength";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(259, 22);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(40, 30);
            this.label10.TabIndex = 9;
            this.label10.Text = "Mean\r\nslopes";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(130, 19);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(53, 30);
            this.label9.TabIndex = 8;
            this.label9.Text = "Tributary\r\nslopes";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 63);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(79, 30);
            this.label8.TabIndex = 7;
            this.label8.Text = "Main channel\r\nlength\r\n";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 19);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(40, 30);
            this.label7.TabIndex = 6;
            this.label7.Text = "Reach\r\nslopes";
            // 
            // mainLengthMultiplier
            // 
            this.mainLengthMultiplier.DecimalPlaces = 1;
            this.mainLengthMultiplier.Location = new System.Drawing.Point(91, 63);
            this.mainLengthMultiplier.Name = "mainLengthMultiplier";
            this.mainLengthMultiplier.Size = new System.Drawing.Size(44, 23);
            this.mainLengthMultiplier.TabIndex = 5;
            this.mainLengthMultiplier.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // meanSlopeMultiplier
            // 
            this.meanSlopeMultiplier.DecimalPlaces = 1;
            this.meanSlopeMultiplier.Location = new System.Drawing.Point(305, 29);
            this.meanSlopeMultiplier.Name = "meanSlopeMultiplier";
            this.meanSlopeMultiplier.Size = new System.Drawing.Size(44, 23);
            this.meanSlopeMultiplier.TabIndex = 4;
            this.meanSlopeMultiplier.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // tributarySlopeMultiplier
            // 
            this.tributarySlopeMultiplier.DecimalPlaces = 1;
            this.tributarySlopeMultiplier.Location = new System.Drawing.Point(189, 26);
            this.tributarySlopeMultiplier.Name = "tributarySlopeMultiplier";
            this.tributarySlopeMultiplier.Size = new System.Drawing.Size(44, 23);
            this.tributarySlopeMultiplier.TabIndex = 3;
            this.tributarySlopeMultiplier.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // tributaryLengthMultiplier
            // 
            this.tributaryLengthMultiplier.DecimalPlaces = 1;
            this.tributaryLengthMultiplier.Location = new System.Drawing.Point(305, 63);
            this.tributaryLengthMultiplier.Name = "tributaryLengthMultiplier";
            this.tributaryLengthMultiplier.Size = new System.Drawing.Size(44, 23);
            this.tributaryLengthMultiplier.TabIndex = 1;
            this.tributaryLengthMultiplier.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // reachSlopeMultiplier
            // 
            this.reachSlopeMultiplier.DecimalPlaces = 1;
            this.reachSlopeMultiplier.Location = new System.Drawing.Point(55, 22);
            this.reachSlopeMultiplier.Name = "reachSlopeMultiplier";
            this.reachSlopeMultiplier.Size = new System.Drawing.Size(44, 23);
            this.reachSlopeMultiplier.TabIndex = 0;
            this.reachSlopeMultiplier.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // burninDepth
            // 
            this.burninDepth.Location = new System.Drawing.Point(125, 381);
            this.burninDepth.Name = "burninDepth";
            this.burninDepth.Size = new System.Drawing.Size(53, 23);
            this.burninDepth.TabIndex = 9;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(21, 381);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(87, 30);
            this.label12.TabIndex = 10;
            this.label12.Text = "Stream burn-in\r\ndepth\r\n";
            // 
            // upslopeHRUDrain
            // 
            this.upslopeHRUDrain.Location = new System.Drawing.Point(313, 381);
            this.upslopeHRUDrain.Name = "upslopeHRUDrain";
            this.upslopeHRUDrain.Size = new System.Drawing.Size(53, 23);
            this.upslopeHRUDrain.TabIndex = 11;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(223, 381);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(77, 30);
            this.label13.TabIndex = 12;
            this.label13.Text = "Upslope HRU\r\ndrain %";
            // 
            // pointSizeBox
            // 
            this.pointSizeBox.Location = new System.Drawing.Point(92, 433);
            this.pointSizeBox.Name = "pointSizeBox";
            this.pointSizeBox.Size = new System.Drawing.Size(44, 23);
            this.pointSizeBox.TabIndex = 13;
            this.pointSizeBox.Value = new decimal(new int[] {
            9,
            0,
            0,
            0});
            this.pointSizeBox.ValueChanged += new System.EventHandler(this.pointSizeBox_ValueChanged);
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(21, 426);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(65, 30);
            this.label14.TabIndex = 14;
            this.label14.Text = "Font point \r\nsize";
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(223, 433);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(75, 23);
            this.saveButton.TabIndex = 15;
            this.saveButton.Text = "Save";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(311, 433);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 16;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // ParamForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 468);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.pointSizeBox);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.upslopeHRUDrain);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.burninDepth);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox);
            this.Controls.Add(this.MPILabel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.checkUseMPI);
            this.Controls.Add(this.MPIButton);
            this.Controls.Add(this.MPIBox);
            this.Controls.Add(this.SWATPlusButton);
            this.Controls.Add(this.SWATPlusBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ParamForm";
            this.Text = "ParamForm";
            this.groupBox.ResumeLayout(false);
            this.groupBox.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mainLengthMultiplier)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.meanSlopeMultiplier)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tributarySlopeMultiplier)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tributaryLengthMultiplier)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.reachSlopeMultiplier)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pointSizeBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox SWATPlusBox;
        private System.Windows.Forms.Button SWATPlusButton;
        private System.Windows.Forms.TextBox MPIBox;
        private System.Windows.Forms.Button MPIButton;
        private System.Windows.Forms.CheckBox checkUseMPI;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label MPILabel;
        private System.Windows.Forms.GroupBox groupBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox depthExp;
        private System.Windows.Forms.TextBox widthExp;
        private System.Windows.Forms.TextBox depthMult;
        private System.Windows.Forms.TextBox widthMult;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown mainLengthMultiplier;
        private System.Windows.Forms.NumericUpDown meanSlopeMultiplier;
        private System.Windows.Forms.NumericUpDown tributarySlopeMultiplier;
        private System.Windows.Forms.NumericUpDown tributaryLengthMultiplier;
        private System.Windows.Forms.NumericUpDown reachSlopeMultiplier;
        private System.Windows.Forms.TextBox burninDepth;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox upslopeHRUDrain;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.NumericUpDown pointSizeBox;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Button cancelButton;
    }
}