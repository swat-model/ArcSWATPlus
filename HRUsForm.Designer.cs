namespace ArcSWATPlus
{
    partial class HRUsForm
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.selectLnduse = new System.Windows.Forms.TextBox();
            this.selectLanduseButton = new System.Windows.Forms.Button();
            this.selectSoil = new System.Windows.Forms.TextBox();
            this.plantSoilDatabase = new System.Windows.Forms.TextBox();
            this.selectPlantSoilDatabaseButton = new System.Windows.Forms.Button();
            this.selectSoilButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.soilGroup = new System.Windows.Forms.GroupBox();
            this.usersoilButton = new System.Windows.Forms.RadioButton();
            this.STATSGOButton = new System.Windows.Forms.RadioButton();
            this.SSURGOButton = new System.Windows.Forms.RadioButton();
            this.tableGroup = new System.Windows.Forms.GroupBox();
            this.selectLanduseTable = new System.Windows.Forms.ComboBox();
            this.selectUrbanTable = new System.Windows.Forms.ComboBox();
            this.selectPlantTable = new System.Windows.Forms.ComboBox();
            this.selectUsersoilTable = new System.Windows.Forms.ComboBox();
            this.selectSoilTable = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.slopeGroup = new System.Windows.Forms.GroupBox();
            this.slopeBand = new System.Windows.Forms.TextBox();
            this.slopeBrowser = new System.Windows.Forms.TextBox();
            this.insertButton = new System.Windows.Forms.Button();
            this.clearButton = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.reservoirThreshold = new System.Windows.Forms.NumericUpDown();
            this.label10 = new System.Windows.Forms.Label();
            this.optionGroup = new System.Windows.Forms.GroupBox();
            this.elevBandsButton = new System.Windows.Forms.Button();
            this.floodplainombo = new System.Windows.Forms.ComboBox();
            this.shortChannelMerge = new System.Windows.Forms.GroupBox();
            this.channelPercentButton = new System.Windows.Forms.RadioButton();
            this.channelAreaButton = new System.Windows.Forms.RadioButton();
            this.channelMergeSlider = new System.Windows.Forms.TrackBar();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.readChoiceGroup = new System.Windows.Forms.GroupBox();
            this.readFromMaps = new System.Windows.Forms.RadioButton();
            this.readFromPrevious = new System.Windows.Forms.RadioButton();
            this.remerge = new System.Windows.Forms.RadioButton();
            this.generateFullHRUs = new System.Windows.Forms.CheckBox();
            this.subbasinsLabel = new System.Windows.Forms.Label();
            this.channelsLabel = new System.Windows.Forms.Label();
            this.fullHRUsLabel = new System.Windows.Forms.Label();
            this.readButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.progressLabel1 = new System.Windows.Forms.Label();
            this.optionGroup_2 = new System.Windows.Forms.GroupBox();
            this.splitButton = new System.Windows.Forms.Button();
            this.exemptButton = new System.Windows.Forms.Button();
            this.hruChoiceGroup = new System.Windows.Forms.GroupBox();
            this.targetButton = new System.Windows.Forms.RadioButton();
            this.dominantHRUButton = new System.Windows.Forms.RadioButton();
            this.filterLanduseButton = new System.Windows.Forms.RadioButton();
            this.filterAreaButton = new System.Windows.Forms.RadioButton();
            this.dominantLanduseButton = new System.Windows.Forms.RadioButton();
            this.areaPercentChoiceGroup = new System.Windows.Forms.GroupBox();
            this.percentButton = new System.Windows.Forms.RadioButton();
            this.areaButton = new System.Windows.Forms.RadioButton();
            this.landuseSoilSlopeGroup = new System.Windows.Forms.GroupBox();
            this.slopeVal = new System.Windows.Forms.TextBox();
            this.slopeMax = new System.Windows.Forms.Label();
            this.slopeLabel = new System.Windows.Forms.Label();
            this.slopeMin = new System.Windows.Forms.Label();
            this.slopeSlider = new System.Windows.Forms.TrackBar();
            this.soilButton = new System.Windows.Forms.Button();
            this.soilVal = new System.Windows.Forms.TextBox();
            this.soilMax = new System.Windows.Forms.Label();
            this.soilLabel = new System.Windows.Forms.Label();
            this.soilMin = new System.Windows.Forms.Label();
            this.soilSlider = new System.Windows.Forms.TrackBar();
            this.landuseButton = new System.Windows.Forms.Button();
            this.landuseVal = new System.Windows.Forms.TextBox();
            this.landuseMax = new System.Windows.Forms.Label();
            this.landuseLabel = new System.Windows.Forms.Label();
            this.landuseMin = new System.Windows.Forms.Label();
            this.landuseSlider = new System.Windows.Forms.TrackBar();
            this.areaGroup = new System.Windows.Forms.GroupBox();
            this.areaVal = new System.Windows.Forms.TextBox();
            this.areaMax = new System.Windows.Forms.Label();
            this.areaLabel = new System.Windows.Forms.Label();
            this.areaMin = new System.Windows.Forms.Label();
            this.areaSlider = new System.Windows.Forms.TrackBar();
            this.targetGroup = new System.Windows.Forms.GroupBox();
            this.targetVal = new System.Windows.Forms.TextBox();
            this.targetMax = new System.Windows.Forms.Label();
            this.targetLabel = new System.Windows.Forms.Label();
            this.targetMin = new System.Windows.Forms.Label();
            this.targetSlider = new System.Windows.Forms.TrackBar();
            this.gwGroupBox = new System.Windows.Forms.GroupBox();
            this.SWATAquifersButton1 = new System.Windows.Forms.RadioButton();
            this.gwflowButton = new System.Windows.Forms.RadioButton();
            this.createButton = new System.Windows.Forms.Button();
            this.progressLabel2 = new System.Windows.Forms.Label();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.soilGroup.SuspendLayout();
            this.tableGroup.SuspendLayout();
            this.slopeGroup.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.reservoirThreshold)).BeginInit();
            this.optionGroup.SuspendLayout();
            this.shortChannelMerge.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.channelMergeSlider)).BeginInit();
            this.readChoiceGroup.SuspendLayout();
            this.optionGroup_2.SuspendLayout();
            this.hruChoiceGroup.SuspendLayout();
            this.areaPercentChoiceGroup.SuspendLayout();
            this.landuseSoilSlopeGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.slopeSlider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.soilSlider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.landuseSlider)).BeginInit();
            this.areaGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.areaSlider)).BeginInit();
            this.targetGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.targetSlider)).BeginInit();
            this.gwGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(10, 10);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(549, 642);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.progressLabel1);
            this.tabPage1.Controls.Add(this.progressBar);
            this.tabPage1.Controls.Add(this.readButton);
            this.tabPage1.Controls.Add(this.fullHRUsLabel);
            this.tabPage1.Controls.Add(this.channelsLabel);
            this.tabPage1.Controls.Add(this.subbasinsLabel);
            this.tabPage1.Controls.Add(this.generateFullHRUs);
            this.tabPage1.Controls.Add(this.readChoiceGroup);
            this.tabPage1.Controls.Add(this.shortChannelMerge);
            this.tabPage1.Controls.Add(this.floodplainombo);
            this.tabPage1.Controls.Add(this.optionGroup);
            this.tabPage1.Controls.Add(this.groupBox1);
            this.tabPage1.Controls.Add(this.slopeGroup);
            this.tabPage1.Controls.Add(this.tableGroup);
            this.tabPage1.Controls.Add(this.soilGroup);
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.selectSoilButton);
            this.tabPage1.Controls.Add(this.selectPlantSoilDatabaseButton);
            this.tabPage1.Controls.Add(this.plantSoilDatabase);
            this.tabPage1.Controls.Add(this.selectSoil);
            this.tabPage1.Controls.Add(this.selectLanduseButton);
            this.tabPage1.Controls.Add(this.selectLnduse);
            this.tabPage1.Location = new System.Drawing.Point(4, 24);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(541, 614);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Landuse and Soil";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.progressLabel2);
            this.tabPage2.Controls.Add(this.createButton);
            this.tabPage2.Controls.Add(this.gwGroupBox);
            this.tabPage2.Controls.Add(this.targetGroup);
            this.tabPage2.Controls.Add(this.areaGroup);
            this.tabPage2.Controls.Add(this.landuseSoilSlopeGroup);
            this.tabPage2.Controls.Add(this.areaPercentChoiceGroup);
            this.tabPage2.Controls.Add(this.hruChoiceGroup);
            this.tabPage2.Controls.Add(this.optionGroup_2);
            this.tabPage2.Location = new System.Drawing.Point(4, 24);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(541, 614);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "HRUs";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // selectLnduse
            // 
            this.selectLnduse.Location = new System.Drawing.Point(6, 28);
            this.selectLnduse.Name = "selectLnduse";
            this.selectLnduse.Size = new System.Drawing.Size(459, 23);
            this.selectLnduse.TabIndex = 0;
            // 
            // selectLanduseButton
            // 
            this.selectLanduseButton.Location = new System.Drawing.Point(478, 28);
            this.selectLanduseButton.Name = "selectLanduseButton";
            this.selectLanduseButton.Size = new System.Drawing.Size(41, 23);
            this.selectLanduseButton.TabIndex = 1;
            this.selectLanduseButton.Text = "...";
            this.selectLanduseButton.UseVisualStyleBackColor = true;
            // 
            // selectSoil
            // 
            this.selectSoil.Location = new System.Drawing.Point(6, 77);
            this.selectSoil.Name = "selectSoil";
            this.selectSoil.Size = new System.Drawing.Size(459, 23);
            this.selectSoil.TabIndex = 2;
            // 
            // plantSoilDatabase
            // 
            this.plantSoilDatabase.Location = new System.Drawing.Point(6, 126);
            this.plantSoilDatabase.Name = "plantSoilDatabase";
            this.plantSoilDatabase.Size = new System.Drawing.Size(459, 23);
            this.plantSoilDatabase.TabIndex = 3;
            // 
            // selectPlantSoilDatabaseButton
            // 
            this.selectPlantSoilDatabaseButton.Location = new System.Drawing.Point(478, 125);
            this.selectPlantSoilDatabaseButton.Name = "selectPlantSoilDatabaseButton";
            this.selectPlantSoilDatabaseButton.Size = new System.Drawing.Size(41, 23);
            this.selectPlantSoilDatabaseButton.TabIndex = 4;
            this.selectPlantSoilDatabaseButton.Text = "...";
            this.selectPlantSoilDatabaseButton.UseVisualStyleBackColor = true;
            // 
            // selectSoilButton
            // 
            this.selectSoilButton.Location = new System.Drawing.Point(478, 77);
            this.selectSoilButton.Name = "selectSoilButton";
            this.selectSoilButton.Size = new System.Drawing.Size(41, 23);
            this.selectSoilButton.TabIndex = 5;
            this.selectSoilButton.Text = "...";
            this.selectSoilButton.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(109, 15);
            this.label1.TabIndex = 6;
            this.label1.Text = "Select landuse map";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 108);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(176, 15);
            this.label2.TabIndex = 7;
            this.label2.Text = "Select landuse and soil database";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 59);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(86, 15);
            this.label3.TabIndex = 8;
            this.label3.Text = "Select soil map";
            // 
            // soilGroup
            // 
            this.soilGroup.Controls.Add(this.SSURGOButton);
            this.soilGroup.Controls.Add(this.STATSGOButton);
            this.soilGroup.Controls.Add(this.usersoilButton);
            this.soilGroup.Location = new System.Drawing.Point(6, 155);
            this.soilGroup.Name = "soilGroup";
            this.soilGroup.Size = new System.Drawing.Size(200, 100);
            this.soilGroup.TabIndex = 9;
            this.soilGroup.TabStop = false;
            this.soilGroup.Text = " Soil data";
            // 
            // usersoilButton
            // 
            this.usersoilButton.AutoSize = true;
            this.usersoilButton.Location = new System.Drawing.Point(16, 20);
            this.usersoilButton.Name = "usersoilButton";
            this.usersoilButton.Size = new System.Drawing.Size(65, 19);
            this.usersoilButton.TabIndex = 0;
            this.usersoilButton.TabStop = true;
            this.usersoilButton.Text = "usersoil";
            this.usersoilButton.UseVisualStyleBackColor = true;
            // 
            // STATSGOButton
            // 
            this.STATSGOButton.AutoSize = true;
            this.STATSGOButton.Location = new System.Drawing.Point(16, 45);
            this.STATSGOButton.Name = "STATSGOButton";
            this.STATSGOButton.Size = new System.Drawing.Size(72, 19);
            this.STATSGOButton.TabIndex = 1;
            this.STATSGOButton.TabStop = true;
            this.STATSGOButton.Text = "STATSGO";
            this.STATSGOButton.UseVisualStyleBackColor = true;
            // 
            // SSURGOButton
            // 
            this.SSURGOButton.AutoSize = true;
            this.SSURGOButton.Location = new System.Drawing.Point(16, 70);
            this.SSURGOButton.Name = "SSURGOButton";
            this.SSURGOButton.Size = new System.Drawing.Size(127, 19);
            this.SSURGOButton.TabIndex = 2;
            this.SSURGOButton.TabStop = true;
            this.SSURGOButton.Text = "SSURGO/STATSGO2";
            this.SSURGOButton.UseVisualStyleBackColor = true;
            // 
            // tableGroup
            // 
            this.tableGroup.Controls.Add(this.label8);
            this.tableGroup.Controls.Add(this.label7);
            this.tableGroup.Controls.Add(this.label6);
            this.tableGroup.Controls.Add(this.label5);
            this.tableGroup.Controls.Add(this.label4);
            this.tableGroup.Controls.Add(this.selectSoilTable);
            this.tableGroup.Controls.Add(this.selectUsersoilTable);
            this.tableGroup.Controls.Add(this.selectPlantTable);
            this.tableGroup.Controls.Add(this.selectUrbanTable);
            this.tableGroup.Controls.Add(this.selectLanduseTable);
            this.tableGroup.Location = new System.Drawing.Point(244, 155);
            this.tableGroup.Name = "tableGroup";
            this.tableGroup.Size = new System.Drawing.Size(260, 170);
            this.tableGroup.TabIndex = 10;
            this.tableGroup.TabStop = false;
            this.tableGroup.Text = "Tables";
            // 
            // selectLanduseTable
            // 
            this.selectLanduseTable.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.selectLanduseTable.FormattingEnabled = true;
            this.selectLanduseTable.Location = new System.Drawing.Point(109, 20);
            this.selectLanduseTable.Name = "selectLanduseTable";
            this.selectLanduseTable.Size = new System.Drawing.Size(145, 23);
            this.selectLanduseTable.TabIndex = 0;
            // 
            // selectUrbanTable
            // 
            this.selectUrbanTable.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.selectUrbanTable.FormattingEnabled = true;
            this.selectUrbanTable.Location = new System.Drawing.Point(109, 136);
            this.selectUrbanTable.Name = "selectUrbanTable";
            this.selectUrbanTable.Size = new System.Drawing.Size(145, 23);
            this.selectUrbanTable.TabIndex = 1;
            this.selectUrbanTable.SelectedIndexChanged += new System.EventHandler(this.comboBox2_SelectedIndexChanged);
            // 
            // selectPlantTable
            // 
            this.selectPlantTable.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.selectPlantTable.FormattingEnabled = true;
            this.selectPlantTable.Location = new System.Drawing.Point(109, 107);
            this.selectPlantTable.Name = "selectPlantTable";
            this.selectPlantTable.Size = new System.Drawing.Size(145, 23);
            this.selectPlantTable.TabIndex = 2;
            // 
            // selectUsersoilTable
            // 
            this.selectUsersoilTable.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.selectUsersoilTable.FormattingEnabled = true;
            this.selectUsersoilTable.Location = new System.Drawing.Point(109, 78);
            this.selectUsersoilTable.Name = "selectUsersoilTable";
            this.selectUsersoilTable.Size = new System.Drawing.Size(145, 23);
            this.selectUsersoilTable.TabIndex = 3;
            // 
            // selectSoilTable
            // 
            this.selectSoilTable.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.selectSoilTable.FormattingEnabled = true;
            this.selectSoilTable.Location = new System.Drawing.Point(109, 49);
            this.selectSoilTable.Name = "selectSoilTable";
            this.selectSoilTable.Size = new System.Drawing.Size(145, 23);
            this.selectSoilTable.TabIndex = 4;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.label4.Location = new System.Drawing.Point(12, 22);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(91, 15);
            this.label4.TabIndex = 5;
            this.label4.Text = "Landuse lookup";
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.label5.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.label5.Location = new System.Drawing.Point(37, 51);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(66, 15);
            this.label5.TabIndex = 6;
            this.label5.Text = "Soil lookup";
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label6.AutoSize = true;
            this.label6.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.label6.Location = new System.Drawing.Point(55, 80);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(48, 15);
            this.label6.TabIndex = 7;
            this.label6.Text = "Usersoil";
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label7.AutoSize = true;
            this.label7.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.label7.Location = new System.Drawing.Point(69, 109);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(34, 15);
            this.label7.TabIndex = 8;
            this.label7.Text = "Plant";
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(64, 138);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(39, 15);
            this.label8.TabIndex = 9;
            this.label8.Text = "Urban";
            // 
            // slopeGroup
            // 
            this.slopeGroup.Controls.Add(this.label9);
            this.slopeGroup.Controls.Add(this.clearButton);
            this.slopeGroup.Controls.Add(this.insertButton);
            this.slopeGroup.Controls.Add(this.slopeBrowser);
            this.slopeGroup.Controls.Add(this.slopeBand);
            this.slopeGroup.Location = new System.Drawing.Point(9, 261);
            this.slopeGroup.Name = "slopeGroup";
            this.slopeGroup.Size = new System.Drawing.Size(140, 136);
            this.slopeGroup.TabIndex = 11;
            this.slopeGroup.TabStop = false;
            this.slopeGroup.Text = "Set slope bands (%)";
            // 
            // slopeBand
            // 
            this.slopeBand.Location = new System.Drawing.Point(11, 21);
            this.slopeBand.Name = "slopeBand";
            this.slopeBand.Size = new System.Drawing.Size(40, 23);
            this.slopeBand.TabIndex = 0;
            // 
            // slopeBrowser
            // 
            this.slopeBrowser.Location = new System.Drawing.Point(11, 94);
            this.slopeBrowser.Name = "slopeBrowser";
            this.slopeBrowser.Size = new System.Drawing.Size(121, 23);
            this.slopeBrowser.TabIndex = 1;
            // 
            // insertButton
            // 
            this.insertButton.Location = new System.Drawing.Point(57, 21);
            this.insertButton.Name = "insertButton";
            this.insertButton.Size = new System.Drawing.Size(75, 23);
            this.insertButton.TabIndex = 2;
            this.insertButton.Text = "Insert";
            this.insertButton.UseVisualStyleBackColor = true;
            // 
            // clearButton
            // 
            this.clearButton.Location = new System.Drawing.Point(57, 50);
            this.clearButton.Name = "clearButton";
            this.clearButton.Size = new System.Drawing.Size(75, 23);
            this.clearButton.TabIndex = 3;
            this.clearButton.Text = "Clear";
            this.clearButton.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(11, 76);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(71, 15);
            this.label9.TabIndex = 4;
            this.label9.Text = "Slope bands";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.reservoirThreshold);
            this.groupBox1.Location = new System.Drawing.Point(11, 403);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(138, 66);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Reservoir threshold";
            // 
            // reservoirThreshold
            // 
            this.reservoirThreshold.Location = new System.Drawing.Point(10, 27);
            this.reservoirThreshold.Maximum = new decimal(new int[] {
            101,
            0,
            0,
            0});
            this.reservoirThreshold.Name = "reservoirThreshold";
            this.reservoirThreshold.Size = new System.Drawing.Size(39, 23);
            this.reservoirThreshold.TabIndex = 0;
            this.reservoirThreshold.Value = new decimal(new int[] {
            101,
            0,
            0,
            0});
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(57, 31);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(49, 15);
            this.label10.TabIndex = 1;
            this.label10.Text = "% water";
            // 
            // optionGroup
            // 
            this.optionGroup.Controls.Add(this.elevBandsButton);
            this.optionGroup.Location = new System.Drawing.Point(11, 475);
            this.optionGroup.Name = "optionGroup";
            this.optionGroup.Size = new System.Drawing.Size(138, 61);
            this.optionGroup.TabIndex = 13;
            this.optionGroup.TabStop = false;
            this.optionGroup.Text = "Optional";
            // 
            // elevBandsButton
            // 
            this.elevBandsButton.Location = new System.Drawing.Point(11, 22);
            this.elevBandsButton.Name = "elevBandsButton";
            this.elevBandsButton.Size = new System.Drawing.Size(106, 23);
            this.elevBandsButton.TabIndex = 0;
            this.elevBandsButton.Text = "Elevation bands";
            this.elevBandsButton.UseVisualStyleBackColor = true;
            // 
            // floodplainombo
            // 
            this.floodplainombo.FormattingEnabled = true;
            this.floodplainombo.Location = new System.Drawing.Point(247, 333);
            this.floodplainombo.Name = "floodplainombo";
            this.floodplainombo.Size = new System.Drawing.Size(257, 23);
            this.floodplainombo.TabIndex = 14;
            // 
            // shortChannelMerge
            // 
            this.shortChannelMerge.Controls.Add(this.textBox1);
            this.shortChannelMerge.Controls.Add(this.channelMergeSlider);
            this.shortChannelMerge.Controls.Add(this.channelAreaButton);
            this.shortChannelMerge.Controls.Add(this.channelPercentButton);
            this.shortChannelMerge.Location = new System.Drawing.Point(246, 366);
            this.shortChannelMerge.Name = "shortChannelMerge";
            this.shortChannelMerge.Size = new System.Drawing.Size(258, 100);
            this.shortChannelMerge.TabIndex = 15;
            this.shortChannelMerge.TabStop = false;
            this.shortChannelMerge.Text = "Short channel merge";
            // 
            // channelPercentButton
            // 
            this.channelPercentButton.AutoSize = true;
            this.channelPercentButton.Location = new System.Drawing.Point(9, 18);
            this.channelPercentButton.Name = "channelPercentButton";
            this.channelPercentButton.Size = new System.Drawing.Size(129, 19);
            this.channelPercentButton.TabIndex = 0;
            this.channelPercentButton.TabStop = true;
            this.channelPercentButton.Text = "Percent of subbasin";
            this.channelPercentButton.UseVisualStyleBackColor = true;
            // 
            // channelAreaButton
            // 
            this.channelAreaButton.AutoSize = true;
            this.channelAreaButton.Location = new System.Drawing.Point(179, 18);
            this.channelAreaButton.Name = "channelAreaButton";
            this.channelAreaButton.Size = new System.Drawing.Size(73, 19);
            this.channelAreaButton.TabIndex = 1;
            this.channelAreaButton.TabStop = true;
            this.channelAreaButton.Text = "Area (ha)";
            this.channelAreaButton.UseVisualStyleBackColor = true;
            // 
            // channelMergeSlider
            // 
            this.channelMergeSlider.Location = new System.Drawing.Point(6, 43);
            this.channelMergeSlider.Maximum = 100;
            this.channelMergeSlider.Name = "channelMergeSlider";
            this.channelMergeSlider.Size = new System.Drawing.Size(182, 45);
            this.channelMergeSlider.TabIndex = 2;
            this.channelMergeSlider.TickFrequency = 10;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(194, 53);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(58, 23);
            this.textBox1.TabIndex = 3;
            // 
            // readChoiceGroup
            // 
            this.readChoiceGroup.Controls.Add(this.remerge);
            this.readChoiceGroup.Controls.Add(this.readFromPrevious);
            this.readChoiceGroup.Controls.Add(this.readFromMaps);
            this.readChoiceGroup.Location = new System.Drawing.Point(155, 472);
            this.readChoiceGroup.Name = "readChoiceGroup";
            this.readChoiceGroup.Size = new System.Drawing.Size(179, 93);
            this.readChoiceGroup.TabIndex = 16;
            this.readChoiceGroup.TabStop = false;
            this.readChoiceGroup.Text = "Read choice";
            // 
            // readFromMaps
            // 
            this.readFromMaps.AutoSize = true;
            this.readFromMaps.Location = new System.Drawing.Point(9, 21);
            this.readFromMaps.Name = "readFromMaps";
            this.readFromMaps.Size = new System.Drawing.Size(112, 19);
            this.readFromMaps.TabIndex = 0;
            this.readFromMaps.TabStop = true;
            this.readFromMaps.Text = "Read from maps";
            this.readFromMaps.UseVisualStyleBackColor = true;
            // 
            // readFromPrevious
            // 
            this.readFromPrevious.AutoSize = true;
            this.readFromPrevious.Location = new System.Drawing.Point(9, 42);
            this.readFromPrevious.Name = "readFromPrevious";
            this.readFromPrevious.Size = new System.Drawing.Size(149, 19);
            this.readFromPrevious.TabIndex = 1;
            this.readFromPrevious.TabStop = true;
            this.readFromPrevious.Text = "Read from previous run";
            this.readFromPrevious.UseVisualStyleBackColor = true;
            // 
            // remerge
            // 
            this.remerge.AutoSize = true;
            this.remerge.Location = new System.Drawing.Point(9, 63);
            this.remerge.Name = "remerge";
            this.remerge.Size = new System.Drawing.Size(157, 19);
            this.remerge.TabIndex = 2;
            this.remerge.TabStop = true;
            this.remerge.Text = "Re-merge short channels";
            this.remerge.UseVisualStyleBackColor = true;
            // 
            // generateFullHRUs
            // 
            this.generateFullHRUs.AutoSize = true;
            this.generateFullHRUs.Location = new System.Drawing.Point(353, 472);
            this.generateFullHRUs.Name = "generateFullHRUs";
            this.generateFullHRUs.Size = new System.Drawing.Size(124, 34);
            this.generateFullHRUs.TabIndex = 17;
            this.generateFullHRUs.Text = "Generate FullHRUs\r\nshapefile\r\n";
            this.generateFullHRUs.UseVisualStyleBackColor = true;
            // 
            // subbasinsLabel
            // 
            this.subbasinsLabel.AutoSize = true;
            this.subbasinsLabel.Location = new System.Drawing.Point(351, 516);
            this.subbasinsLabel.Name = "subbasinsLabel";
            this.subbasinsLabel.Size = new System.Drawing.Size(97, 15);
            this.subbasinsLabel.TabIndex = 18;
            this.subbasinsLabel.Text = "Subbasins count:";
            // 
            // channelsLabel
            // 
            this.channelsLabel.AutoSize = true;
            this.channelsLabel.Location = new System.Drawing.Point(351, 536);
            this.channelsLabel.Name = "channelsLabel";
            this.channelsLabel.Size = new System.Drawing.Size(96, 15);
            this.channelsLabel.TabIndex = 19;
            this.channelsLabel.Text = "Channels count: ";
            // 
            // fullHRUsLabel
            // 
            this.fullHRUsLabel.AutoSize = true;
            this.fullHRUsLabel.Location = new System.Drawing.Point(351, 556);
            this.fullHRUsLabel.Name = "fullHRUsLabel";
            this.fullHRUsLabel.Size = new System.Drawing.Size(98, 15);
            this.fullHRUsLabel.TabIndex = 20;
            this.fullHRUsLabel.Text = "Full HRUs count: ";
            // 
            // readButton
            // 
            this.readButton.Location = new System.Drawing.Point(440, 584);
            this.readButton.Name = "readButton";
            this.readButton.Size = new System.Drawing.Size(75, 23);
            this.readButton.TabIndex = 21;
            this.readButton.Text = "Read";
            this.readButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(454, 658);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(164, 585);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(255, 23);
            this.progressBar.TabIndex = 22;
            // 
            // progressLabel1
            // 
            this.progressLabel1.AutoSize = true;
            this.progressLabel1.Location = new System.Drawing.Point(14, 588);
            this.progressLabel1.Name = "progressLabel1";
            this.progressLabel1.Size = new System.Drawing.Size(72, 15);
            this.progressLabel1.TabIndex = 23;
            this.progressLabel1.Text = "dummy text";
            // 
            // optionGroup_2
            // 
            this.optionGroup_2.Controls.Add(this.exemptButton);
            this.optionGroup_2.Controls.Add(this.splitButton);
            this.optionGroup_2.Location = new System.Drawing.Point(6, 15);
            this.optionGroup_2.Name = "optionGroup_2";
            this.optionGroup_2.Size = new System.Drawing.Size(133, 104);
            this.optionGroup_2.TabIndex = 14;
            this.optionGroup_2.TabStop = false;
            this.optionGroup_2.Text = "Optional";
            // 
            // splitButton
            // 
            this.splitButton.Location = new System.Drawing.Point(11, 22);
            this.splitButton.Name = "splitButton";
            this.splitButton.Size = new System.Drawing.Size(106, 23);
            this.splitButton.TabIndex = 0;
            this.splitButton.Text = "Split landuses";
            this.splitButton.UseVisualStyleBackColor = true;
            // 
            // exemptButton
            // 
            this.exemptButton.Location = new System.Drawing.Point(11, 62);
            this.exemptButton.Name = "exemptButton";
            this.exemptButton.Size = new System.Drawing.Size(106, 23);
            this.exemptButton.TabIndex = 1;
            this.exemptButton.Text = "Exempt landuses";
            this.exemptButton.UseVisualStyleBackColor = true;
            // 
            // hruChoiceGroup
            // 
            this.hruChoiceGroup.Controls.Add(this.dominantLanduseButton);
            this.hruChoiceGroup.Controls.Add(this.filterAreaButton);
            this.hruChoiceGroup.Controls.Add(this.filterLanduseButton);
            this.hruChoiceGroup.Controls.Add(this.dominantHRUButton);
            this.hruChoiceGroup.Controls.Add(this.targetButton);
            this.hruChoiceGroup.Location = new System.Drawing.Point(145, 15);
            this.hruChoiceGroup.Name = "hruChoiceGroup";
            this.hruChoiceGroup.Size = new System.Drawing.Size(192, 142);
            this.hruChoiceGroup.TabIndex = 15;
            this.hruChoiceGroup.TabStop = false;
            this.hruChoiceGroup.Text = "Single/Mutiple HRUs";
            // 
            // targetButton
            // 
            this.targetButton.AutoSize = true;
            this.targetButton.Location = new System.Drawing.Point(9, 111);
            this.targetButton.Name = "targetButton";
            this.targetButton.Size = new System.Drawing.Size(148, 19);
            this.targetButton.TabIndex = 0;
            this.targetButton.TabStop = true;
            this.targetButton.Text = "Target number of HRUs";
            this.targetButton.UseVisualStyleBackColor = true;
            // 
            // dominantHRUButton
            // 
            this.dominantHRUButton.AutoSize = true;
            this.dominantHRUButton.Location = new System.Drawing.Point(9, 45);
            this.dominantHRUButton.Name = "dominantHRUButton";
            this.dominantHRUButton.Size = new System.Drawing.Size(105, 19);
            this.dominantHRUButton.TabIndex = 1;
            this.dominantHRUButton.TabStop = true;
            this.dominantHRUButton.Text = "Dominant HRU";
            this.dominantHRUButton.UseVisualStyleBackColor = true;
            // 
            // filterLanduseButton
            // 
            this.filterLanduseButton.AutoSize = true;
            this.filterLanduseButton.Location = new System.Drawing.Point(9, 67);
            this.filterLanduseButton.Name = "filterLanduseButton";
            this.filterLanduseButton.Size = new System.Drawing.Size(169, 19);
            this.filterLanduseButton.TabIndex = 2;
            this.filterLanduseButton.TabStop = true;
            this.filterLanduseButton.Text = "Filter by landuse, soil, slope";
            this.filterLanduseButton.UseVisualStyleBackColor = true;
            // 
            // filterAreaButton
            // 
            this.filterAreaButton.AutoSize = true;
            this.filterAreaButton.Location = new System.Drawing.Point(9, 89);
            this.filterAreaButton.Name = "filterAreaButton";
            this.filterAreaButton.Size = new System.Drawing.Size(92, 19);
            this.filterAreaButton.TabIndex = 3;
            this.filterAreaButton.TabStop = true;
            this.filterAreaButton.Text = "Filter by area";
            this.filterAreaButton.UseVisualStyleBackColor = true;
            // 
            // dominantLanduseButton
            // 
            this.dominantLanduseButton.AutoSize = true;
            this.dominantLanduseButton.Location = new System.Drawing.Point(9, 23);
            this.dominantLanduseButton.Name = "dominantLanduseButton";
            this.dominantLanduseButton.Size = new System.Drawing.Size(180, 19);
            this.dominantLanduseButton.TabIndex = 4;
            this.dominantLanduseButton.TabStop = true;
            this.dominantLanduseButton.Text = "Dominant landuse, soil, slope";
            this.dominantLanduseButton.UseVisualStyleBackColor = true;
            // 
            // areaPercentChoiceGroup
            // 
            this.areaPercentChoiceGroup.Controls.Add(this.areaButton);
            this.areaPercentChoiceGroup.Controls.Add(this.percentButton);
            this.areaPercentChoiceGroup.Location = new System.Drawing.Point(147, 168);
            this.areaPercentChoiceGroup.Name = "areaPercentChoiceGroup";
            this.areaPercentChoiceGroup.Size = new System.Drawing.Size(190, 75);
            this.areaPercentChoiceGroup.TabIndex = 16;
            this.areaPercentChoiceGroup.TabStop = false;
            this.areaPercentChoiceGroup.Text = "Threshold method";
            // 
            // percentButton
            // 
            this.percentButton.AutoSize = true;
            this.percentButton.Location = new System.Drawing.Point(9, 23);
            this.percentButton.Name = "percentButton";
            this.percentButton.Size = new System.Drawing.Size(159, 19);
            this.percentButton.TabIndex = 0;
            this.percentButton.TabStop = true;
            this.percentButton.Text = "Percent of landscape unit";
            this.percentButton.UseVisualStyleBackColor = true;
            // 
            // areaButton
            // 
            this.areaButton.AutoSize = true;
            this.areaButton.Location = new System.Drawing.Point(9, 45);
            this.areaButton.Name = "areaButton";
            this.areaButton.Size = new System.Drawing.Size(73, 19);
            this.areaButton.TabIndex = 1;
            this.areaButton.TabStop = true;
            this.areaButton.Text = "Area (ha)";
            this.areaButton.UseVisualStyleBackColor = true;
            // 
            // landuseSoilSlopeGroup
            // 
            this.landuseSoilSlopeGroup.Controls.Add(this.slopeVal);
            this.landuseSoilSlopeGroup.Controls.Add(this.slopeMax);
            this.landuseSoilSlopeGroup.Controls.Add(this.slopeLabel);
            this.landuseSoilSlopeGroup.Controls.Add(this.slopeMin);
            this.landuseSoilSlopeGroup.Controls.Add(this.slopeSlider);
            this.landuseSoilSlopeGroup.Controls.Add(this.soilButton);
            this.landuseSoilSlopeGroup.Controls.Add(this.soilVal);
            this.landuseSoilSlopeGroup.Controls.Add(this.soilMax);
            this.landuseSoilSlopeGroup.Controls.Add(this.soilLabel);
            this.landuseSoilSlopeGroup.Controls.Add(this.soilMin);
            this.landuseSoilSlopeGroup.Controls.Add(this.soilSlider);
            this.landuseSoilSlopeGroup.Controls.Add(this.landuseButton);
            this.landuseSoilSlopeGroup.Controls.Add(this.landuseVal);
            this.landuseSoilSlopeGroup.Controls.Add(this.landuseMax);
            this.landuseSoilSlopeGroup.Controls.Add(this.landuseLabel);
            this.landuseSoilSlopeGroup.Controls.Add(this.landuseMin);
            this.landuseSoilSlopeGroup.Controls.Add(this.landuseSlider);
            this.landuseSoilSlopeGroup.Location = new System.Drawing.Point(343, 15);
            this.landuseSoilSlopeGroup.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.landuseSoilSlopeGroup.Name = "landuseSoilSlopeGroup";
            this.landuseSoilSlopeGroup.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.landuseSoilSlopeGroup.Size = new System.Drawing.Size(185, 239);
            this.landuseSoilSlopeGroup.TabIndex = 17;
            this.landuseSoilSlopeGroup.TabStop = false;
            this.landuseSoilSlopeGroup.Text = "Set landuse, soil, slope thresholds";
            // 
            // slopeVal
            // 
            this.slopeVal.Location = new System.Drawing.Point(147, 166);
            this.slopeVal.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.slopeVal.Name = "slopeVal";
            this.slopeVal.Size = new System.Drawing.Size(32, 23);
            this.slopeVal.TabIndex = 16;
            // 
            // slopeMax
            // 
            this.slopeMax.AutoSize = true;
            this.slopeMax.Location = new System.Drawing.Point(118, 167);
            this.slopeMax.Name = "slopeMax";
            this.slopeMax.Size = new System.Drawing.Size(25, 15);
            this.slopeMax.TabIndex = 15;
            this.slopeMax.Text = "100";
            // 
            // slopeLabel
            // 
            this.slopeLabel.AutoSize = true;
            this.slopeLabel.Location = new System.Drawing.Point(43, 167);
            this.slopeLabel.Name = "slopeLabel";
            this.slopeLabel.Size = new System.Drawing.Size(57, 15);
            this.slopeLabel.TabIndex = 14;
            this.slopeLabel.Text = "Slope (%)";
            // 
            // slopeMin
            // 
            this.slopeMin.AutoSize = true;
            this.slopeMin.Location = new System.Drawing.Point(16, 167);
            this.slopeMin.Name = "slopeMin";
            this.slopeMin.Size = new System.Drawing.Size(13, 15);
            this.slopeMin.TabIndex = 13;
            this.slopeMin.Text = "0";
            // 
            // slopeSlider
            // 
            this.slopeSlider.Location = new System.Drawing.Point(6, 185);
            this.slopeSlider.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.slopeSlider.Name = "slopeSlider";
            this.slopeSlider.Size = new System.Drawing.Size(137, 45);
            this.slopeSlider.TabIndex = 12;
            this.slopeSlider.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
            // 
            // soilButton
            // 
            this.soilButton.Location = new System.Drawing.Point(148, 129);
            this.soilButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.soilButton.Name = "soilButton";
            this.soilButton.Size = new System.Drawing.Size(32, 23);
            this.soilButton.TabIndex = 11;
            this.soilButton.Text = "Go";
            this.soilButton.UseVisualStyleBackColor = true;
            // 
            // soilVal
            // 
            this.soilVal.Location = new System.Drawing.Point(148, 102);
            this.soilVal.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.soilVal.Name = "soilVal";
            this.soilVal.Size = new System.Drawing.Size(32, 23);
            this.soilVal.TabIndex = 10;
            // 
            // soilMax
            // 
            this.soilMax.AutoSize = true;
            this.soilMax.Location = new System.Drawing.Point(121, 103);
            this.soilMax.Name = "soilMax";
            this.soilMax.Size = new System.Drawing.Size(25, 15);
            this.soilMax.TabIndex = 9;
            this.soilMax.Text = "100";
            // 
            // soilLabel
            // 
            this.soilLabel.AutoSize = true;
            this.soilLabel.Location = new System.Drawing.Point(52, 103);
            this.soilLabel.Name = "soilLabel";
            this.soilLabel.Size = new System.Drawing.Size(47, 15);
            this.soilLabel.TabIndex = 8;
            this.soilLabel.Text = "Soil (%)";
            // 
            // soilMin
            // 
            this.soilMin.AutoSize = true;
            this.soilMin.Location = new System.Drawing.Point(16, 103);
            this.soilMin.Name = "soilMin";
            this.soilMin.Size = new System.Drawing.Size(13, 15);
            this.soilMin.TabIndex = 7;
            this.soilMin.Text = "0";
            // 
            // soilSlider
            // 
            this.soilSlider.Location = new System.Drawing.Point(6, 121);
            this.soilSlider.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.soilSlider.Name = "soilSlider";
            this.soilSlider.Size = new System.Drawing.Size(137, 45);
            this.soilSlider.TabIndex = 6;
            this.soilSlider.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
            // 
            // landuseButton
            // 
            this.landuseButton.Location = new System.Drawing.Point(148, 65);
            this.landuseButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.landuseButton.Name = "landuseButton";
            this.landuseButton.Size = new System.Drawing.Size(32, 23);
            this.landuseButton.TabIndex = 5;
            this.landuseButton.Text = "Go";
            this.landuseButton.UseVisualStyleBackColor = true;
            // 
            // landuseVal
            // 
            this.landuseVal.Location = new System.Drawing.Point(148, 38);
            this.landuseVal.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.landuseVal.Name = "landuseVal";
            this.landuseVal.Size = new System.Drawing.Size(32, 23);
            this.landuseVal.TabIndex = 4;
            // 
            // landuseMax
            // 
            this.landuseMax.AutoSize = true;
            this.landuseMax.Location = new System.Drawing.Point(121, 39);
            this.landuseMax.Name = "landuseMax";
            this.landuseMax.Size = new System.Drawing.Size(25, 15);
            this.landuseMax.TabIndex = 3;
            this.landuseMax.Text = "100";
            // 
            // landuseLabel
            // 
            this.landuseLabel.AutoSize = true;
            this.landuseLabel.Location = new System.Drawing.Point(43, 39);
            this.landuseLabel.Name = "landuseLabel";
            this.landuseLabel.Size = new System.Drawing.Size(72, 15);
            this.landuseLabel.TabIndex = 2;
            this.landuseLabel.Text = "Landuse (%)";
            // 
            // landuseMin
            // 
            this.landuseMin.AutoSize = true;
            this.landuseMin.Location = new System.Drawing.Point(16, 39);
            this.landuseMin.Name = "landuseMin";
            this.landuseMin.Size = new System.Drawing.Size(13, 15);
            this.landuseMin.TabIndex = 1;
            this.landuseMin.Text = "0";
            // 
            // landuseSlider
            // 
            this.landuseSlider.Location = new System.Drawing.Point(6, 57);
            this.landuseSlider.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.landuseSlider.Name = "landuseSlider";
            this.landuseSlider.Size = new System.Drawing.Size(137, 45);
            this.landuseSlider.TabIndex = 0;
            this.landuseSlider.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
            // 
            // areaGroup
            // 
            this.areaGroup.Controls.Add(this.areaVal);
            this.areaGroup.Controls.Add(this.areaMax);
            this.areaGroup.Controls.Add(this.areaLabel);
            this.areaGroup.Controls.Add(this.areaMin);
            this.areaGroup.Controls.Add(this.areaSlider);
            this.areaGroup.Location = new System.Drawing.Point(343, 105);
            this.areaGroup.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.areaGroup.Name = "areaGroup";
            this.areaGroup.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.areaGroup.Size = new System.Drawing.Size(185, 75);
            this.areaGroup.TabIndex = 24;
            this.areaGroup.TabStop = false;
            this.areaGroup.Text = "Set area threshold";
            // 
            // areaVal
            // 
            this.areaVal.Location = new System.Drawing.Point(147, 45);
            this.areaVal.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.areaVal.Name = "areaVal";
            this.areaVal.Size = new System.Drawing.Size(32, 23);
            this.areaVal.TabIndex = 10;
            // 
            // areaMax
            // 
            this.areaMax.AutoSize = true;
            this.areaMax.Location = new System.Drawing.Point(121, 22);
            this.areaMax.Name = "areaMax";
            this.areaMax.Size = new System.Drawing.Size(31, 15);
            this.areaMax.TabIndex = 9;
            this.areaMax.Text = "9999";
            // 
            // areaLabel
            // 
            this.areaLabel.AutoSize = true;
            this.areaLabel.Location = new System.Drawing.Point(51, 22);
            this.areaLabel.Name = "areaLabel";
            this.areaLabel.Size = new System.Drawing.Size(55, 15);
            this.areaLabel.TabIndex = 8;
            this.areaLabel.Text = "Area (ha)";
            // 
            // areaMin
            // 
            this.areaMin.AutoSize = true;
            this.areaMin.Location = new System.Drawing.Point(5, 22);
            this.areaMin.Name = "areaMin";
            this.areaMin.Size = new System.Drawing.Size(13, 15);
            this.areaMin.TabIndex = 7;
            this.areaMin.Text = "0";
            // 
            // areaSlider
            // 
            this.areaSlider.Location = new System.Drawing.Point(5, 37);
            this.areaSlider.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.areaSlider.Name = "areaSlider";
            this.areaSlider.Size = new System.Drawing.Size(137, 45);
            this.areaSlider.TabIndex = 6;
            this.areaSlider.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
            // 
            // targetGroup
            // 
            this.targetGroup.Controls.Add(this.targetVal);
            this.targetGroup.Controls.Add(this.targetMax);
            this.targetGroup.Controls.Add(this.targetLabel);
            this.targetGroup.Controls.Add(this.targetMin);
            this.targetGroup.Controls.Add(this.targetSlider);
            this.targetGroup.Location = new System.Drawing.Point(343, 105);
            this.targetGroup.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.targetGroup.Name = "targetGroup";
            this.targetGroup.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.targetGroup.Size = new System.Drawing.Size(185, 75);
            this.targetGroup.TabIndex = 25;
            this.targetGroup.TabStop = false;
            this.targetGroup.Text = "Set target";
            // 
            // targetVal
            // 
            this.targetVal.Location = new System.Drawing.Point(148, 45);
            this.targetVal.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.targetVal.Name = "targetVal";
            this.targetVal.Size = new System.Drawing.Size(32, 23);
            this.targetVal.TabIndex = 10;
            // 
            // targetMax
            // 
            this.targetMax.AutoSize = true;
            this.targetMax.Location = new System.Drawing.Point(121, 22);
            this.targetMax.Name = "targetMax";
            this.targetMax.Size = new System.Drawing.Size(31, 15);
            this.targetMax.TabIndex = 9;
            this.targetMax.Text = "9999";
            // 
            // targetLabel
            // 
            this.targetLabel.AutoSize = true;
            this.targetLabel.Location = new System.Drawing.Point(28, 22);
            this.targetLabel.Name = "targetLabel";
            this.targetLabel.Size = new System.Drawing.Size(97, 15);
            this.targetLabel.TabIndex = 8;
            this.targetLabel.Text = "Number of HRUs";
            // 
            // targetMin
            // 
            this.targetMin.AutoSize = true;
            this.targetMin.Location = new System.Drawing.Point(5, 22);
            this.targetMin.Name = "targetMin";
            this.targetMin.Size = new System.Drawing.Size(13, 15);
            this.targetMin.TabIndex = 7;
            this.targetMin.Text = "0";
            // 
            // targetSlider
            // 
            this.targetSlider.Location = new System.Drawing.Point(5, 37);
            this.targetSlider.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.targetSlider.Name = "targetSlider";
            this.targetSlider.Size = new System.Drawing.Size(137, 45);
            this.targetSlider.TabIndex = 6;
            this.targetSlider.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
            // 
            // gwGroupBox
            // 
            this.gwGroupBox.Controls.Add(this.gwflowButton);
            this.gwGroupBox.Controls.Add(this.SWATAquifersButton1);
            this.gwGroupBox.Location = new System.Drawing.Point(262, 365);
            this.gwGroupBox.Name = "gwGroupBox";
            this.gwGroupBox.Size = new System.Drawing.Size(154, 84);
            this.gwGroupBox.TabIndex = 26;
            this.gwGroupBox.TabStop = false;
            this.gwGroupBox.Text = "Ground water";
            // 
            // SWATAquifersButton1
            // 
            this.SWATAquifersButton1.AutoSize = true;
            this.SWATAquifersButton1.Location = new System.Drawing.Point(11, 28);
            this.SWATAquifersButton1.Name = "SWATAquifersButton1";
            this.SWATAquifersButton1.Size = new System.Drawing.Size(130, 19);
            this.SWATAquifersButton1.TabIndex = 0;
            this.SWATAquifersButton1.TabStop = true;
            this.SWATAquifersButton1.Text = "Use SWAT+ aquifers";
            this.SWATAquifersButton1.UseVisualStyleBackColor = true;
            // 
            // gwflowButton
            // 
            this.gwflowButton.AutoSize = true;
            this.gwflowButton.Location = new System.Drawing.Point(11, 53);
            this.gwflowButton.Name = "gwflowButton";
            this.gwflowButton.Size = new System.Drawing.Size(86, 19);
            this.gwflowButton.TabIndex = 1;
            this.gwflowButton.TabStop = true;
            this.gwflowButton.Text = "Use gwflow";
            this.gwflowButton.UseVisualStyleBackColor = true;
            // 
            // createButton
            // 
            this.createButton.Location = new System.Drawing.Point(424, 576);
            this.createButton.Name = "createButton";
            this.createButton.Size = new System.Drawing.Size(91, 23);
            this.createButton.TabIndex = 27;
            this.createButton.Text = "Create HRUs";
            this.createButton.UseVisualStyleBackColor = true;
            // 
            // progressLabel2
            // 
            this.progressLabel2.AutoSize = true;
            this.progressLabel2.Location = new System.Drawing.Point(30, 580);
            this.progressLabel2.Name = "progressLabel2";
            this.progressLabel2.Size = new System.Drawing.Size(72, 15);
            this.progressLabel2.TabIndex = 28;
            this.progressLabel2.Text = "dummy text";
            // 
            // HRUsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 690);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.tabControl1);
            this.Name = "HRUsForm";
            this.Text = "HRUsForm";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.soilGroup.ResumeLayout(false);
            this.soilGroup.PerformLayout();
            this.tableGroup.ResumeLayout(false);
            this.tableGroup.PerformLayout();
            this.slopeGroup.ResumeLayout(false);
            this.slopeGroup.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.reservoirThreshold)).EndInit();
            this.optionGroup.ResumeLayout(false);
            this.shortChannelMerge.ResumeLayout(false);
            this.shortChannelMerge.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.channelMergeSlider)).EndInit();
            this.readChoiceGroup.ResumeLayout(false);
            this.readChoiceGroup.PerformLayout();
            this.optionGroup_2.ResumeLayout(false);
            this.hruChoiceGroup.ResumeLayout(false);
            this.hruChoiceGroup.PerformLayout();
            this.areaPercentChoiceGroup.ResumeLayout(false);
            this.areaPercentChoiceGroup.PerformLayout();
            this.landuseSoilSlopeGroup.ResumeLayout(false);
            this.landuseSoilSlopeGroup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.slopeSlider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.soilSlider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.landuseSlider)).EndInit();
            this.areaGroup.ResumeLayout(false);
            this.areaGroup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.areaSlider)).EndInit();
            this.targetGroup.ResumeLayout(false);
            this.targetGroup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.targetSlider)).EndInit();
            this.gwGroupBox.ResumeLayout(false);
            this.gwGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.GroupBox tableGroup;
        private System.Windows.Forms.ComboBox selectSoilTable;
        private System.Windows.Forms.ComboBox selectUsersoilTable;
        private System.Windows.Forms.ComboBox selectPlantTable;
        private System.Windows.Forms.ComboBox selectUrbanTable;
        private System.Windows.Forms.ComboBox selectLanduseTable;
        private System.Windows.Forms.GroupBox soilGroup;
        private System.Windows.Forms.RadioButton SSURGOButton;
        private System.Windows.Forms.RadioButton STATSGOButton;
        private System.Windows.Forms.RadioButton usersoilButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button selectSoilButton;
        private System.Windows.Forms.Button selectPlantSoilDatabaseButton;
        private System.Windows.Forms.TextBox plantSoilDatabase;
        private System.Windows.Forms.TextBox selectSoil;
        private System.Windows.Forms.Button selectLanduseButton;
        private System.Windows.Forms.TextBox selectLnduse;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.GroupBox optionGroup;
        private System.Windows.Forms.Button elevBandsButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.NumericUpDown reservoirThreshold;
        private System.Windows.Forms.GroupBox slopeGroup;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button clearButton;
        private System.Windows.Forms.Button insertButton;
        private System.Windows.Forms.TextBox slopeBrowser;
        private System.Windows.Forms.TextBox slopeBand;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label progressLabel1;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Button readButton;
        private System.Windows.Forms.Label fullHRUsLabel;
        private System.Windows.Forms.Label channelsLabel;
        private System.Windows.Forms.Label subbasinsLabel;
        private System.Windows.Forms.CheckBox generateFullHRUs;
        private System.Windows.Forms.GroupBox readChoiceGroup;
        private System.Windows.Forms.RadioButton remerge;
        private System.Windows.Forms.RadioButton readFromPrevious;
        private System.Windows.Forms.RadioButton readFromMaps;
        private System.Windows.Forms.GroupBox shortChannelMerge;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TrackBar channelMergeSlider;
        private System.Windows.Forms.RadioButton channelAreaButton;
        private System.Windows.Forms.RadioButton channelPercentButton;
        private System.Windows.Forms.ComboBox floodplainombo;
        private System.Windows.Forms.GroupBox targetGroup;
        private System.Windows.Forms.TextBox targetVal;
        private System.Windows.Forms.Label targetMax;
        private System.Windows.Forms.Label targetLabel;
        private System.Windows.Forms.Label targetMin;
        private System.Windows.Forms.TrackBar targetSlider;
        private System.Windows.Forms.GroupBox areaGroup;
        private System.Windows.Forms.TextBox areaVal;
        private System.Windows.Forms.Label areaMax;
        private System.Windows.Forms.Label areaLabel;
        private System.Windows.Forms.Label areaMin;
        private System.Windows.Forms.TrackBar areaSlider;
        private System.Windows.Forms.GroupBox landuseSoilSlopeGroup;
        private System.Windows.Forms.TextBox slopeVal;
        private System.Windows.Forms.Label slopeMax;
        private System.Windows.Forms.Label slopeLabel;
        private System.Windows.Forms.Label slopeMin;
        private System.Windows.Forms.TrackBar slopeSlider;
        private System.Windows.Forms.Button soilButton;
        private System.Windows.Forms.TextBox soilVal;
        private System.Windows.Forms.Label soilMax;
        private System.Windows.Forms.Label soilLabel;
        private System.Windows.Forms.Label soilMin;
        private System.Windows.Forms.TrackBar soilSlider;
        private System.Windows.Forms.Button landuseButton;
        private System.Windows.Forms.TextBox landuseVal;
        private System.Windows.Forms.Label landuseMax;
        private System.Windows.Forms.Label landuseLabel;
        private System.Windows.Forms.Label landuseMin;
        private System.Windows.Forms.TrackBar landuseSlider;
        private System.Windows.Forms.GroupBox areaPercentChoiceGroup;
        private System.Windows.Forms.RadioButton areaButton;
        private System.Windows.Forms.RadioButton percentButton;
        private System.Windows.Forms.GroupBox hruChoiceGroup;
        private System.Windows.Forms.RadioButton dominantLanduseButton;
        private System.Windows.Forms.RadioButton filterAreaButton;
        private System.Windows.Forms.RadioButton filterLanduseButton;
        private System.Windows.Forms.RadioButton dominantHRUButton;
        private System.Windows.Forms.RadioButton targetButton;
        private System.Windows.Forms.GroupBox optionGroup_2;
        private System.Windows.Forms.Button exemptButton;
        private System.Windows.Forms.Button splitButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Label progressLabel2;
        private System.Windows.Forms.Button createButton;
        private System.Windows.Forms.GroupBox gwGroupBox;
        private System.Windows.Forms.RadioButton gwflowButton;
        private System.Windows.Forms.RadioButton SWATAquifersButton1;
    }
}