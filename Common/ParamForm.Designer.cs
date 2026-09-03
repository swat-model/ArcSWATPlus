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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ParamForm));
            SWATPlusBox = new System.Windows.Forms.TextBox();
            SWATPlusButton = new System.Windows.Forms.Button();
            MPIBox = new System.Windows.Forms.TextBox();
            MPIButton = new System.Windows.Forms.Button();
            checkUseMPI = new System.Windows.Forms.CheckBox();
            label1 = new System.Windows.Forms.Label();
            MPILabel = new System.Windows.Forms.Label();
            groupBox = new System.Windows.Forms.GroupBox();
            label6 = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            depthExp = new System.Windows.Forms.TextBox();
            widthExp = new System.Windows.Forms.TextBox();
            depthMult = new System.Windows.Forms.TextBox();
            widthMult = new System.Windows.Forms.TextBox();
            groupBox1 = new System.Windows.Forms.GroupBox();
            label11 = new System.Windows.Forms.Label();
            label10 = new System.Windows.Forms.Label();
            label9 = new System.Windows.Forms.Label();
            label8 = new System.Windows.Forms.Label();
            label7 = new System.Windows.Forms.Label();
            mainLengthMultiplier = new System.Windows.Forms.NumericUpDown();
            meanSlopeMultiplier = new System.Windows.Forms.NumericUpDown();
            tributarySlopeMultiplier = new System.Windows.Forms.NumericUpDown();
            tributaryLengthMultiplier = new System.Windows.Forms.NumericUpDown();
            reachSlopeMultiplier = new System.Windows.Forms.NumericUpDown();
            burninDepth = new System.Windows.Forms.TextBox();
            label12 = new System.Windows.Forms.Label();
            upslopeHRUDrain = new System.Windows.Forms.TextBox();
            label13 = new System.Windows.Forms.Label();
            pointSizeBox = new System.Windows.Forms.NumericUpDown();
            label14 = new System.Windows.Forms.Label();
            saveButton = new System.Windows.Forms.Button();
            cancelButton = new System.Windows.Forms.Button();
            label15 = new System.Windows.Forms.Label();
            decimalPlaces = new System.Windows.Forms.NumericUpDown();
            decimalPlacesTip = new System.Windows.Forms.ToolTip(components);
            label15 = new System.Windows.Forms.Label();
            decimalPlaces = new System.Windows.Forms.NumericUpDown();
            decimalPlacesTip = new System.Windows.Forms.ToolTip(components);
            groupBox.SuspendLayout();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)mainLengthMultiplier).BeginInit();
            ((System.ComponentModel.ISupportInitialize)meanSlopeMultiplier).BeginInit();
            ((System.ComponentModel.ISupportInitialize)tributarySlopeMultiplier).BeginInit();
            ((System.ComponentModel.ISupportInitialize)tributaryLengthMultiplier).BeginInit();
            ((System.ComponentModel.ISupportInitialize)reachSlopeMultiplier).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pointSizeBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)decimalPlaces).BeginInit();
            SuspendLayout();
            // 
            // SWATPlusBox
            // 
            SWATPlusBox.Location = new System.Drawing.Point(12, 36);
            SWATPlusBox.Name = "SWATPlusBox";
            SWATPlusBox.Size = new System.Drawing.Size(324, 23);
            SWATPlusBox.TabIndex = 0;
            // 
            // SWATPlusButton
            // 
            SWATPlusButton.Location = new System.Drawing.Point(342, 36);
            SWATPlusButton.Name = "SWATPlusButton";
            SWATPlusButton.Size = new System.Drawing.Size(44, 23);
            SWATPlusButton.TabIndex = 1;
            SWATPlusButton.Text = "...";
            SWATPlusButton.UseVisualStyleBackColor = true;
            SWATPlusButton.Click += SWATPlusButton_Click;
            // 
            // MPIBox
            // 
            MPIBox.Location = new System.Drawing.Point(12, 101);
            MPIBox.Name = "MPIBox";
            MPIBox.Size = new System.Drawing.Size(324, 23);
            MPIBox.TabIndex = 2;
            // 
            // MPIButton
            // 
            MPIButton.Location = new System.Drawing.Point(342, 101);
            MPIButton.Name = "MPIButton";
            MPIButton.Size = new System.Drawing.Size(44, 23);
            MPIButton.TabIndex = 3;
            MPIButton.Text = "...";
            MPIButton.UseVisualStyleBackColor = true;
            MPIButton.Click += MPIButton_Click;
            // 
            // checkUseMPI
            // 
            checkUseMPI.AutoSize = true;
            checkUseMPI.Location = new System.Drawing.Point(13, 74);
            checkUseMPI.Name = "checkUseMPI";
            checkUseMPI.Size = new System.Drawing.Size(69, 19);
            checkUseMPI.TabIndex = 4;
            checkUseMPI.Text = "Use MPI";
            checkUseMPI.UseVisualStyleBackColor = true;
            checkUseMPI.CheckedChanged += checkUseMPI_CheckedChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(134, 9);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(109, 15);
            label1.TabIndex = 5;
            label1.Text = "SWATPlus directory";
            // 
            // MPILabel
            // 
            MPILabel.AutoSize = true;
            MPILabel.Location = new System.Drawing.Point(134, 76);
            MPILabel.Name = "MPILabel";
            MPILabel.Size = new System.Drawing.Size(102, 15);
            MPILabel.TabIndex = 6;
            MPILabel.Text = "MPI bin dorectory";
            // 
            // groupBox
            // 
            groupBox.Controls.Add(label6);
            groupBox.Controls.Add(label5);
            groupBox.Controls.Add(label4);
            groupBox.Controls.Add(label3);
            groupBox.Controls.Add(depthExp);
            groupBox.Controls.Add(widthExp);
            groupBox.Controls.Add(depthMult);
            groupBox.Controls.Add(widthMult);
            groupBox.Location = new System.Drawing.Point(13, 143);
            groupBox.Name = "groupBox";
            groupBox.Size = new System.Drawing.Size(373, 102);
            groupBox.TabIndex = 7;
            groupBox.TabStop = false;
            groupBox.Text = "Channel widths and depths";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(210, 66);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(85, 30);
            label6.TabIndex = 7;
            label6.Text = "Channel depth\r\nexponent\r\n";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(22, 66);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(85, 30);
            label5.TabIndex = 6;
            label5.Text = "Channel depth\r\nmultiplier\r\n";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(210, 24);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(84, 30);
            label4.TabIndex = 5;
            label4.Text = "Channel width\r\nexponent\r\n";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(22, 24);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(84, 30);
            label3.TabIndex = 4;
            label3.Text = "Channel width\r\nmultiplier\r\n";
            // 
            // depthExp
            // 
            depthExp.Location = new System.Drawing.Point(300, 73);
            depthExp.Name = "depthExp";
            depthExp.Size = new System.Drawing.Size(53, 23);
            depthExp.TabIndex = 3;
            // 
            // widthExp
            // 
            widthExp.Location = new System.Drawing.Point(300, 28);
            widthExp.Name = "widthExp";
            widthExp.Size = new System.Drawing.Size(53, 23);
            widthExp.TabIndex = 2;
            // 
            // depthMult
            // 
            depthMult.Location = new System.Drawing.Point(113, 73);
            depthMult.Name = "depthMult";
            depthMult.Size = new System.Drawing.Size(53, 23);
            depthMult.TabIndex = 1;
            // 
            // widthMult
            // 
            widthMult.Location = new System.Drawing.Point(112, 28);
            widthMult.Name = "widthMult";
            widthMult.Size = new System.Drawing.Size(53, 23);
            widthMult.TabIndex = 0;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(label11);
            groupBox1.Controls.Add(label10);
            groupBox1.Controls.Add(label9);
            groupBox1.Controls.Add(label8);
            groupBox1.Controls.Add(label7);
            groupBox1.Controls.Add(mainLengthMultiplier);
            groupBox1.Controls.Add(meanSlopeMultiplier);
            groupBox1.Controls.Add(tributarySlopeMultiplier);
            groupBox1.Controls.Add(tributaryLengthMultiplier);
            groupBox1.Controls.Add(reachSlopeMultiplier);
            groupBox1.Location = new System.Drawing.Point(20, 263);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(366, 100);
            groupBox1.TabIndex = 8;
            groupBox1.TabStop = false;
            groupBox1.Text = "Slope and length multipliers";
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new System.Drawing.Point(189, 63);
            label11.Name = "label11";
            label11.Size = new System.Drawing.Size(98, 30);
            label11.TabIndex = 10;
            label11.Text = "Tributary channel\r\nlength";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new System.Drawing.Point(259, 22);
            label10.Name = "label10";
            label10.Size = new System.Drawing.Size(40, 30);
            label10.TabIndex = 9;
            label10.Text = "Mean\r\nslopes";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new System.Drawing.Point(130, 19);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(53, 30);
            label9.TabIndex = 8;
            label9.Text = "Tributary\r\nslopes";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new System.Drawing.Point(6, 63);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(79, 30);
            label8.TabIndex = 7;
            label8.Text = "Main channel\r\nlength\r\n";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(6, 19);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(40, 30);
            label7.TabIndex = 6;
            label7.Text = "Reach\r\nslopes";
            // 
            // mainLengthMultiplier
            // 
            mainLengthMultiplier.DecimalPlaces = 1;
            mainLengthMultiplier.Location = new System.Drawing.Point(91, 63);
            mainLengthMultiplier.Name = "mainLengthMultiplier";
            mainLengthMultiplier.Size = new System.Drawing.Size(44, 23);
            mainLengthMultiplier.TabIndex = 5;
            mainLengthMultiplier.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // meanSlopeMultiplier
            // 
            meanSlopeMultiplier.DecimalPlaces = 1;
            meanSlopeMultiplier.Location = new System.Drawing.Point(305, 29);
            meanSlopeMultiplier.Name = "meanSlopeMultiplier";
            meanSlopeMultiplier.Size = new System.Drawing.Size(44, 23);
            meanSlopeMultiplier.TabIndex = 4;
            meanSlopeMultiplier.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // tributarySlopeMultiplier
            // 
            tributarySlopeMultiplier.DecimalPlaces = 1;
            tributarySlopeMultiplier.Location = new System.Drawing.Point(189, 26);
            tributarySlopeMultiplier.Name = "tributarySlopeMultiplier";
            tributarySlopeMultiplier.Size = new System.Drawing.Size(44, 23);
            tributarySlopeMultiplier.TabIndex = 3;
            tributarySlopeMultiplier.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // tributaryLengthMultiplier
            // 
            tributaryLengthMultiplier.DecimalPlaces = 1;
            tributaryLengthMultiplier.Location = new System.Drawing.Point(305, 63);
            tributaryLengthMultiplier.Name = "tributaryLengthMultiplier";
            tributaryLengthMultiplier.Size = new System.Drawing.Size(44, 23);
            tributaryLengthMultiplier.TabIndex = 1;
            tributaryLengthMultiplier.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // reachSlopeMultiplier
            // 
            reachSlopeMultiplier.DecimalPlaces = 1;
            reachSlopeMultiplier.Location = new System.Drawing.Point(55, 22);
            reachSlopeMultiplier.Name = "reachSlopeMultiplier";
            reachSlopeMultiplier.Size = new System.Drawing.Size(44, 23);
            reachSlopeMultiplier.TabIndex = 0;
            reachSlopeMultiplier.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // burninDepth
            // 
            burninDepth.Location = new System.Drawing.Point(125, 381);
            burninDepth.Name = "burninDepth";
            burninDepth.Size = new System.Drawing.Size(53, 23);
            burninDepth.TabIndex = 9;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new System.Drawing.Point(21, 381);
            label12.Name = "label12";
            label12.Size = new System.Drawing.Size(87, 30);
            label12.TabIndex = 10;
            label12.Text = "Stream burn-in\r\ndepth\r\n";
            // 
            // upslopeHRUDrain
            // 
            upslopeHRUDrain.Location = new System.Drawing.Point(313, 381);
            upslopeHRUDrain.Name = "upslopeHRUDrain";
            upslopeHRUDrain.Size = new System.Drawing.Size(53, 23);
            upslopeHRUDrain.TabIndex = 11;
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new System.Drawing.Point(223, 381);
            label13.Name = "label13";
            label13.Size = new System.Drawing.Size(77, 30);
            label13.TabIndex = 12;
            label13.Text = "Upslope HRU\r\ndrain %";
            // 
            // pointSizeBox
            // 
            pointSizeBox.Location = new System.Drawing.Point(61, 433);
            pointSizeBox.Name = "pointSizeBox";
            pointSizeBox.Size = new System.Drawing.Size(44, 23);
            pointSizeBox.TabIndex = 13;
            pointSizeBox.Value = new decimal(new int[] { 9, 0, 0, 0 });
            pointSizeBox.ValueChanged += pointSizeBox_ValueChanged;
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Location = new System.Drawing.Point(21, 426);
            label14.Name = "label14";
            label14.Size = new System.Drawing.Size(38, 45);
            label14.TabIndex = 14;
            label14.Text = "Font \r\npoint \r\nsize";
            // 
            // saveButton
            // 
            saveButton.Location = new System.Drawing.Point(223, 447);
            saveButton.Name = "saveButton";
            saveButton.Size = new System.Drawing.Size(75, 23);
            saveButton.TabIndex = 15;
            saveButton.Text = "Save";
            saveButton.UseVisualStyleBackColor = true;
            saveButton.Click += saveButton_Click;
            // 
            // cancelButton
            // 
            cancelButton.Location = new System.Drawing.Point(311, 447);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new System.Drawing.Size(75, 23);
            cancelButton.TabIndex = 16;
            cancelButton.Text = "Cancel";
            cancelButton.UseVisualStyleBackColor = true;
            cancelButton.Click += cancelButton_Click;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(111, 433);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(50, 30);
            this.label15.TabIndex = 17;
            this.label15.Text = "Decimal\r\nplaces";
            // 
            // decimalPlaces
            // 
            decimalPlaces.Location = new System.Drawing.Point(167, 433);
            decimalPlaces.Name = "decimalPlaces";
            decimalPlaces.Size = new System.Drawing.Size(38, 23);
            decimalPlaces.TabIndex = 18;
            decimalPlaces.Value = new decimal(new int[] { 2, 0, 0, 0 });
            // 
            // decimalPlacesTip
            // 
            decimalPlacesTip.Tag = "The number of decimal places used in reporting numbers.";
            // 
            // ParamForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(400, 482);

            Controls.Add(decimalPlaces);
            Controls.Add(label15);
            Controls.Add(cancelButton);
            Controls.Add(saveButton);
            Controls.Add(label14);
            Controls.Add(pointSizeBox);
            Controls.Add(label13);
            Controls.Add(upslopeHRUDrain);
            Controls.Add(label12);
            Controls.Add(burninDepth);
            Controls.Add(groupBox1);
            Controls.Add(groupBox);
            Controls.Add(MPILabel);
            Controls.Add(label1);
            Controls.Add(checkUseMPI);
            Controls.Add(MPIButton);
            Controls.Add(MPIBox);
            Controls.Add(SWATPlusButton);
            Controls.Add(SWATPlusBox);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "ParamForm";
            Text = "ParamForm";
            groupBox.ResumeLayout(false);
            groupBox.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)mainLengthMultiplier).EndInit();
            ((System.ComponentModel.ISupportInitialize)meanSlopeMultiplier).EndInit();
            ((System.ComponentModel.ISupportInitialize)tributarySlopeMultiplier).EndInit();
            ((System.ComponentModel.ISupportInitialize)tributaryLengthMultiplier).EndInit();
            ((System.ComponentModel.ISupportInitialize)reachSlopeMultiplier).EndInit();
            ((System.ComponentModel.ISupportInitialize)pointSizeBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)decimalPlaces).EndInit();
            ResumeLayout(false);
            PerformLayout();

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
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.NumericUpDown decimalPlaces;
        private System.Windows.Forms.ToolTip decimalPlacesTip;
    }
}