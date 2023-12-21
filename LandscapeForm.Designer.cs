namespace ArcSWATPlus
{
    partial class LandscapeForm
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
            this.hillslopesCheckBox = new System.Windows.Forms.CheckBox();
            this.floodplainCheckBox = new System.Windows.Forms.CheckBox();
            this.methodTab = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.label2 = new System.Windows.Forms.Label();
            this.bufferMultiplier = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.areaUnitsBox = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.ridgeThresholdArea = new System.Windows.Forms.TextBox();
            this.ridgeThresholdCells = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.label8 = new System.Windows.Forms.Label();
            this.branchThreshold = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.slopePositionSpinBox = new System.Windows.Forms.NumericUpDown();
            this.slopePositionLabel = new System.Windows.Forms.Label();
            this.doneButton = new System.Windows.Forms.Button();
            this.createButton = new System.Windows.Forms.Button();
            this.methodTab.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bufferMultiplier)).BeginInit();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.slopePositionSpinBox)).BeginInit();
            this.SuspendLayout();
            // 
            // hillslopesCheckBox
            // 
            this.hillslopesCheckBox.AutoSize = true;
            this.hillslopesCheckBox.Location = new System.Drawing.Point(21, 12);
            this.hillslopesCheckBox.Name = "hillslopesCheckBox";
            this.hillslopesCheckBox.Size = new System.Drawing.Size(127, 19);
            this.hillslopesCheckBox.TabIndex = 0;
            this.hillslopesCheckBox.Text = "Calculate hillslopes";
            this.hillslopesCheckBox.UseVisualStyleBackColor = true;
            // 
            // floodplainCheckBox
            // 
            this.floodplainCheckBox.AutoSize = true;
            this.floodplainCheckBox.Checked = true;
            this.floodplainCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.floodplainCheckBox.Location = new System.Drawing.Point(204, 13);
            this.floodplainCheckBox.Name = "floodplainCheckBox";
            this.floodplainCheckBox.Size = new System.Drawing.Size(132, 19);
            this.floodplainCheckBox.TabIndex = 1;
            this.floodplainCheckBox.Text = "Calculate floodplain";
            this.floodplainCheckBox.UseVisualStyleBackColor = true;
            // 
            // methodTab
            // 
            this.methodTab.Controls.Add(this.tabPage1);
            this.methodTab.Controls.Add(this.tabPage2);
            this.methodTab.Controls.Add(this.tabPage3);
            this.methodTab.Location = new System.Drawing.Point(12, 38);
            this.methodTab.Name = "methodTab";
            this.methodTab.SelectedIndex = 0;
            this.methodTab.Size = new System.Drawing.Size(350, 163);
            this.methodTab.TabIndex = 2;
            this.methodTab.TabIndexChanged += new System.EventHandler(this.methodTab_TabIndexChanged);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.bufferMultiplier);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Location = new System.Drawing.Point(4, 24);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(342, 135);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Buffer channels";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(67, 89);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(93, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "Buffer multiplier";
            // 
            // bufferMultiplier
            // 
            this.bufferMultiplier.Location = new System.Drawing.Point(18, 87);
            this.bufferMultiplier.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.bufferMultiplier.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.bufferMultiplier.Name = "bufferMultiplier";
            this.bufferMultiplier.Size = new System.Drawing.Size(39, 23);
            this.bufferMultiplier.TabIndex = 3;
            this.bufferMultiplier.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(206, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Use a channel buffer as the floodplain";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.areaUnitsBox);
            this.tabPage2.Controls.Add(this.label6);
            this.tabPage2.Controls.Add(this.label5);
            this.tabPage2.Controls.Add(this.ridgeThresholdArea);
            this.tabPage2.Controls.Add(this.ridgeThresholdCells);
            this.tabPage2.Controls.Add(this.label4);
            this.tabPage2.Controls.Add(this.label3);
            this.tabPage2.Location = new System.Drawing.Point(4, 24);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(342, 135);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "DEM inversion";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // areaUnitsBox
            // 
            this.areaUnitsBox.FormattingEnabled = true;
            this.areaUnitsBox.Location = new System.Drawing.Point(226, 84);
            this.areaUnitsBox.Name = "areaUnitsBox";
            this.areaUnitsBox.Size = new System.Drawing.Size(94, 23);
            this.areaUnitsBox.TabIndex = 6;
            this.areaUnitsBox.SelectedIndexChanged += new System.EventHandler(this.areaUnitsBox_SelectedIndexChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(191, 90);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(31, 15);
            this.label6.TabIndex = 5;
            this.label6.Text = "Area";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(75, 87);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(51, 30);
            this.label5.TabIndex = 4;
            this.label5.Text = "Number\r\nof cells\r\n";
            // 
            // ridgeThresholdArea
            // 
            this.ridgeThresholdArea.Location = new System.Drawing.Point(129, 84);
            this.ridgeThresholdArea.Name = "ridgeThresholdArea";
            this.ridgeThresholdArea.Size = new System.Drawing.Size(51, 23);
            this.ridgeThresholdArea.TabIndex = 3;
            this.ridgeThresholdArea.TextChanged += new System.EventHandler(this.ridgeThresholdArea_TextChanged);
            // 
            // ridgeThresholdCells
            // 
            this.ridgeThresholdCells.Location = new System.Drawing.Point(15, 84);
            this.ridgeThresholdCells.Name = "ridgeThresholdCells";
            this.ridgeThresholdCells.Size = new System.Drawing.Size(51, 23);
            this.ridgeThresholdCells.TabIndex = 2;
            this.ridgeThresholdCells.TextChanged += new System.EventHandler(this.ridgeThresholdCells_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(114, 59);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(90, 15);
            this.label4.TabIndex = 1;
            this.label4.Text = "Ridge threshold";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(18, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(215, 15);
            this.label3.TabIndex = 0;
            this.label3.Text = "Use an inverted DEM to calculate ridges";
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.label8);
            this.tabPage3.Controls.Add(this.branchThreshold);
            this.tabPage3.Controls.Add(this.label7);
            this.tabPage3.Location = new System.Drawing.Point(4, 24);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(342, 135);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Branch length";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(79, 89);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(184, 15);
            this.label8.TabIndex = 3;
            this.label8.Text = "Theshold branch length in metres";
            // 
            // branchThreshold
            // 
            this.branchThreshold.Location = new System.Drawing.Point(15, 84);
            this.branchThreshold.Name = "branchThreshold";
            this.branchThreshold.Size = new System.Drawing.Size(51, 23);
            this.branchThreshold.TabIndex = 2;
            this.branchThreshold.TextChanged += new System.EventHandler(this.branchThreshold_TextChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(15, 19);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(207, 15);
            this.label7.TabIndex = 1;
            this.label7.Text = "Use branch lengths to calculate ridges";
            // 
            // slopePositionSpinBox
            // 
            this.slopePositionSpinBox.DecimalPlaces = 2;
            this.slopePositionSpinBox.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.slopePositionSpinBox.Location = new System.Drawing.Point(24, 219);
            this.slopePositionSpinBox.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.slopePositionSpinBox.Name = "slopePositionSpinBox";
            this.slopePositionSpinBox.Size = new System.Drawing.Size(61, 23);
            this.slopePositionSpinBox.TabIndex = 7;
            this.slopePositionSpinBox.Value = new decimal(new int[] {
            10,
            0,
            0,
            131072});
            // 
            // slopePositionLabel
            // 
            this.slopePositionLabel.AutoSize = true;
            this.slopePositionLabel.Location = new System.Drawing.Point(91, 221);
            this.slopePositionLabel.Name = "slopePositionLabel";
            this.slopePositionLabel.Size = new System.Drawing.Size(135, 15);
            this.slopePositionLabel.TabIndex = 4;
            this.slopePositionLabel.Text = "Slope position threshold";
            // 
            // doneButton
            // 
            this.doneButton.Location = new System.Drawing.Point(287, 247);
            this.doneButton.Name = "doneButton";
            this.doneButton.Size = new System.Drawing.Size(75, 23);
            this.doneButton.TabIndex = 5;
            this.doneButton.Text = "Done";
            this.doneButton.UseVisualStyleBackColor = true;
            this.doneButton.Click += new System.EventHandler(this.doneButton_Click);
            // 
            // createButton
            // 
            this.createButton.Location = new System.Drawing.Point(204, 247);
            this.createButton.Name = "createButton";
            this.createButton.Size = new System.Drawing.Size(75, 23);
            this.createButton.TabIndex = 6;
            this.createButton.Text = "Create";
            this.createButton.UseVisualStyleBackColor = true;
            this.createButton.Click += new System.EventHandler(this.createButton_Click);
            // 
            // LandscapeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(376, 283);
            this.Controls.Add(this.createButton);
            this.Controls.Add(this.doneButton);
            this.Controls.Add(this.slopePositionLabel);
            this.Controls.Add(this.slopePositionSpinBox);
            this.Controls.Add(this.methodTab);
            this.Controls.Add(this.floodplainCheckBox);
            this.Controls.Add(this.hillslopesCheckBox);
            this.Name = "LandscapeForm";
            this.Text = "LandscapeForm";
            this.Load += new System.EventHandler(this.LandscapeForm_Load);
            this.methodTab.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bufferMultiplier)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.slopePositionSpinBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox hillslopesCheckBox;
        private System.Windows.Forms.CheckBox floodplainCheckBox;
        private System.Windows.Forms.TabControl methodTab;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown bufferMultiplier;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.ComboBox areaUnitsBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox ridgeThresholdArea;
        private System.Windows.Forms.TextBox ridgeThresholdCells;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox branchThreshold;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown slopePositionSpinBox;
        private System.Windows.Forms.Label slopePositionLabel;
        private System.Windows.Forms.Button doneButton;
        private System.Windows.Forms.Button createButton;
    }
}