using ArcGIS.Core.Internal.CIM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using Path = System.IO.Path;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Win32;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using System.Windows;

namespace ArcSWATPlus
{
    public partial class HRUsForm : Form
    {
        public GlobalVars _gv;
        public HRUs _parent;
        public DBUtils _db;
        public CreateHRUs _createHRUs;

        public HRUsForm(GlobalVars gv, HRUs parent) {
            this._gv = gv;
            this._parent = parent;
            this._db = gv.db;
            // parent.CreateHRUs is null at this point: set in init
            //this._createHRUs = parent.CreateHRUs;
            InitializeComponent();
        }

        // Set up HRUs dialog.
        public async Task init() {
            this._createHRUs = this._parent.CreateHRUs;
            this._parent._db.populateTableNames();
            this.selectLanduseTable.Items.AddRange(this._parent._db.landuseTableNames.ToArray());
            this.selectLanduseTable.Items.Add(Parameters._USECSV);
            this.selectSoilTable.Items.AddRange(this._parent._db.soilTableNames.ToArray());
            this.selectSoilTable.Items.Add(Parameters._USECSV);
            await this.readProj();
            this.setLanduseData();
            this.setSoilData();
            //this.usersoilButton.toggled.connect(this.setSoilData);
            //this.STATSGOButton.toggled.connect(this.setSoilData);
            //this.SSURGOButton.toggled.connect(this.setSoilData);
            //this.selectPlantSoilDatabaseButton.clicked.connect(this.selectPlantSoilDatabase);
            this._gv.getExemptSplit();
            this.subbasinsLabel.Text = "";
            this.channelsLabel.Text = "";
            this.fullHRUsLabel.Text = "";
            this.optionGroup.Enabled = true;
            this.landuseSoilSlopeGroup.Enabled = false;
            this.areaGroup.Enabled = false;
            this.targetGroup.Enabled = false;
            this.createButton.Enabled = false;
            this.progressBar.Visible = false;
            this.shortChannelGroup.Visible = !this._gv.useGridModel;
            this.remerge.Visible = !this._gv.useGridModel;
            // this.channelPercentButton.toggled.connect(this.setChannelChoice);
            //this.channelAreaButton.toggled.connect(this.setChannelChoice);
            //this.channelMergeVal.setValidator(QIntValidator());
            //this.channelMergeVal.textChanged.connect(this.readChannelThreshold);
            //this.channelMergeSlider.valueChanged.connect(this.changeChannelThreshold);
            //this.reservoirThreshold.valueChanged.connect(this.setRead);
            this.setRead();
            //this.floodplainCombo.activated.connect(this.setToReadFromMaps);
            //this.readFromMaps.toggled.connect(this.setReadChoice);
            //this.readFromPrevious.toggled.connect(this.setReadChoice);
            //this.dominantHRUButton.toggled.connect(this.setHRUChoice);
            //this.dominantLanduseButton.toggled.connect(this.setHRUChoice);
            //this.filterLanduseButton.toggled.connect(this.setHRUChoice);
            //this.filterAreaButton.toggled.connect(this.setHRUChoice);
            //this.targetButton.toggled.connect(this.setHRUChoice);
            //this.percentButton.toggled.connect(this.setHRUChoice);
            //this.areaButton.toggled.connect(this.setHRUChoice);
            this.slopeBrowser.Text = Utils.slopesToString(this._db.slopeLimits);
            //this.selectLanduseButton.clicked.connect(this.getLanduseFile);
            //this.selectSoilButton.clicked.connect(this.getSoilFile);
            //this.selectLanduseTable.activated.connect(this.setLanduseTable);
            //this.selectSoilTable.activated.connect(this.setSoilTable);
            //this.selectPlantTable.activated.connect(this.setPlantTable);
            //this.selectUrbanTable.activated.connect(this.setUrbanTable);
            //this.selectUsersoilTable.activated.connect(this.setUsersoilTable);
            //this.insertButton.clicked.connect(this.insertSlope);
            //this.landuseVal.setValidator(QIntValidator());
            //this.soilVal.setValidator(QIntValidator());
            //this.slopeVal.setValidator(QIntValidator());
            //this.areaVal.setValidator(QIntValidator());
            //this.targetVal.setValidator(QIntValidator());
            //this.slopeBand.setValidator(QDoubleValidator());
            this.floodplainCombo.Items.Add("Select floodplain map (optional)");
            this.floodplainCombo.SelectedIndex = 0;
            this.initFloodplain();
            //this.slopeBand.returnPressed.connect(this.insertSlope);
            //this.clearButton.clicked.connect(this.clearSlopes);
            //this.readButton.clicked.connect(this.readFiles);
            //this.createButton.clicked.connect(this.calcHRUs);
            //this.cancelButton.clicked.connect(this.close);
            //this.areaVal.textChanged.connect(this.readAreaThreshold);
            //this.areaSlider.valueChanged.connect(this.changeAreaThreshold);
            //this.landuseVal.textChanged.connect(this.readLanduseThreshold);
            //this.landuseSlider.valueChanged.connect(this.changeLanduseThreshold);
            //this.soilVal.textChanged.connect(this.readSoilThreshold);
            //this.soilSlider.valueChanged.connect(this.changeSoilThreshold);
            //this.targetVal.textChanged.connect(this.readTargetThreshold);
            //this.targetSlider.valueChanged.connect(this.changeTargetThreshold);
            //this.slopeVal.textChanged.connect(this.readSlopeThreshold);
            //this.slopeSlider.valueChanged.connect(this.changeSlopeThreshold);
            //this.landuseButton.clicked.connect(this.setLanduseThreshold);
            //this.soilButton.clicked.connect(this.setSoilThreshold);
            //this.exemptButton.clicked.connect(this.doExempt);
            //this.splitButton.clicked.connect(this.doSplit);
            //this.elevBandsButton.clicked.connect(this.doElevBands);
            //this.HRUsTab.currentChanged.connect(this.switchTabs);
        }

        // Debugging information when tab switched in HRUs form.
        public virtual void switchTabs() {
            // @UnusedVariable
            //Utils.information('Switched to tab {}', i), False)
            this.progress("");
        }

        // 
        //         Add floodplain rasters to floodplainCombo.  Disable the combo if there are none.
        //         
        //         Potential floodplain rasters are of the form *flood*.tif in the flood directory.
        //         Also sets combo current item according to (previously read) project file.
        //         
        public virtual void initFloodplain() {
            Matcher matcher = new Matcher();
            var pattern = "*.tif";
            matcher.AddInclude(pattern);
            var found = false;
            foreach (var f in matcher.GetResultsInFullPath(this._gv.floodDir)) {
                this.floodplainCombo.Items.Add(Path.GetFileName(f));
                found = true;
            }
            if (!found) {
                this.floodplainCombo.Enabled = false;
                this._gv.useLandscapes = false;
            }
            if (this._gv.useLandscapes) {
                var index = this.floodplainCombo.FindString(Path.GetFileName(this._gv.floodFile));
                if (index > 0) {
                    this.floodplainCombo.SelectedIndex = index;
                }
            }
        }

        // Set plant and urban comboBoxes.
        public virtual void setLanduseData() {
            if (!this._db.plantSoilDatabaseSelected) {
                this.plantSoilDatabase.Text = this._db.dbFile;
                this._db.plantSoilDatabase = this._db.dbFile;
            }
            this._db.plantTableNames = this._db.collectPlantSoilTableNames("plant", this.selectPlantTable);
            var plantSearch = this._db.plantTable.Contains("plant") ? this._db.plantTable : "plant";
            var plantIndex = this.selectPlantTable.FindString(plantSearch);
            if (plantIndex >= 0) {
                this.selectPlantTable.SelectedIndex = plantIndex;
            }
            this._db.plantTable = this.selectPlantTable.Text;
            this._db.urbanTableNames = this._db.collectPlantSoilTableNames("urban", this.selectUrbanTable);
            var urbanSearch = this._db.urbanTable.Contains("urban") ? this._db.urbanTable : "urban";
            var urbanIndex = this.selectUrbanTable.FindString(urbanSearch);
            if (urbanIndex >= 0) {
                this.selectUrbanTable.SelectedIndex = urbanIndex;
            }
            this._db.urbanTable = this.selectUrbanTable.Text;
        }       // Read usersoil/STATSGO/SSURGO choice and set variables.
        public virtual void setSoilData() {
            if (this.usersoilButton.Checked) {
                this.dbLabel.Text = "Select landuse and soil database";
                this._db.useSTATSGO = false;
                this._db.useSSURGO = false;
                this.soilTableLabel.Enabled = true;
                this.selectSoilTable.Enabled = true;
                this.selectUsersoilTable.Visible = true;
                this.selectUsersoilTableLabel.Visible = true;
                if (!this._db.plantSoilDatabaseSelected) {
                    this.plantSoilDatabase.Text = this._db.dbFile;
                    this._db.plantSoilDatabase = this._db.dbFile;
                }
                this._db.usersoilTableNames = this._db.collectPlantSoilTableNames("usersoil", this.selectUsersoilTable);
                var searchTable = this._db.usersoilTable.Contains("usersoil") ? this._db.usersoilTable : "usersoil";
                var usersoilIndex = this.selectUsersoilTable.FindString(searchTable);
                if (usersoilIndex >= 0) {
                    this.selectUsersoilTable.SelectedIndex = usersoilIndex;
                }
                this._db.usersoilTable = this.selectUsersoilTable.Text;
            } else if (this.STATSGOButton.Checked) {
                this.dbLabel.Text = "Select landuse database";
                this._db.useSTATSGO = true;
                this._db.useSSURGO = false;
                this.soilTableLabel.Enabled = true;
                this.selectSoilTable.Enabled = true;
                this.selectUsersoilTable.Visible = false;
                this.selectUsersoilTableLabel.Visible = false;
                this._db.soilDatabase = Utils.join(this._gv.dbPath, Parameters._SOILDB);
                if (!File.Exists(this._db.soilDatabase)) {
                    Utils.information(string.Format("To use STATSGO soils with QSWAT+ you need to download the SWAT+ STATSGO/SSURGO soil database {0} and save it as {1}.", "https://bitbucket.org/swatplus/swatplus.editor/downloads/swatplus_soils.sqlite", this._db.soilDatabase), this._gv.isBatch);
                }
            } else if (this.SSURGOButton.Checked) {
                this.dbLabel.Text = "Select landuse database";
                this._db.useSTATSGO = false;
                this._db.useSSURGO = true;
                this.soilTableLabel.Enabled = false;
                this.selectSoilTable.Enabled = false;
                this.selectUsersoilTable.Visible = false;
                this.selectUsersoilTableLabel.Visible = false;
                this._db.soilDatabase = Utils.join(this._gv.dbPath, Parameters._SOILDB);
                if (!File.Exists(this._db.soilDatabase)) {
                    Utils.information(string.Format("To use SSURGO soils with QSWAT+ you need to download the SWAT+ STATSGO/SSURGO soil database {0} and save it as {1}.", "https://bitbucket.org/swatplus/swatplus.editor/downloads/swatplus_soils.sqlite", this._db.soilDatabase), this._gv.isBatch);
                }
            }
        }       // Allow user to select plant and soil database.
        public virtual void selectPlantSoilDatabase() {
            string path = "";
            //var settings = QSettings();
            //if (settings.contains("/QSWATPlus/LastInputPath")) {
            //    path = settings.value("/QSWATPlus/LastInputPath").ToString();
            //} else {
            //    path = "";
            //}
            string db = null;
            var ft = FileTypes._SQLITE;
            using (OpenFileDialog dlg = new OpenFileDialog()) {
                dlg.InitialDirectory = path;
                dlg.Title = "Select landuse and soil database";
                dlg.Filter = FileTypes.filter(ft);
                //dlg.RestoreDirectory = false;
                if (dlg.ShowDialog() == DialogResult.OK) {
                    db = dlg.FileName;
                }
            }
            if (!string.IsNullOrEmpty(db)) {
                this.plantSoilDatabase.Text = db;
                this._db.plantSoilDatabase = db;
                this._db.plantSoilDatabaseSelected = true;
                this._db.plantTableNames = this._db.collectPlantSoilTableNames("plant", this.selectPlantTable);
                this._db.urbanTableNames = this._db.collectPlantSoilTableNames("urban", this.selectUrbanTable);
                // combo boxes may have changed, so reset plant, urban and usersoil choices
                // according to first entry in combo box
                this.setPlantTable();
                this.setUrbanTable();
                if (!(this._db.useSTATSGO || this._db.useSSURGO)) {
                    this._db.usersoilTableNames = this._db.collectPlantSoilTableNames("usersoil", this.selectUsersoilTable);
                    this.setUsersoilTable();
                }
            }
        }        // Set dialog to read from maps or from previous run.
        public virtual void setRead() {
            // for safety always rerun reading files for HUC projects
            Utils.loginfo("Checking BASINSDATA");
            if (this._db.hasData("BASINSDATA") && !this._gv.isHUC) {
                this.readFromPrevious.Enabled = true;
                this.readFromPrevious.Checked = true;
            } else {
                this.readFromMaps.Checked = true;
                this.readFromPrevious.Enabled = false;
            }
            Utils.loginfo("Finished checking BASINSDATA");
            this.setReadChoice();
        }

        // Remove 'read from previous' option.
        public virtual void setToReadFromMaps() {
            this.readFromMaps.Checked = true;
            this.readFromPrevious.Enabled = false;
        }

        // Read read choice and set variables.
        public virtual void setReadChoice() {
            if (this.readFromMaps.Checked) {
                this.slopeGroup.Enabled = true;
                this.generateFullHRUs.Enabled = true;
                this.elevBandsButton.Enabled = true;
                if (this.floodplainCombo.Items.Count > 1) {
                    this.floodplainCombo.Enabled = true;
                }
            } else {
                this.slopeGroup.Enabled = false;
                this.generateFullHRUs.Enabled = false;
                this.elevBandsButton.Enabled = false;
                this.floodplainCombo.Enabled = false;
            }
            this.splitButton.Enabled = false;
            this.exemptButton.Enabled = false;
            this.hruChoiceGroup.Enabled = false;
            this.areaPercentChoiceGroup.Enabled = false;
            this.landuseSoilSlopeGroup.Enabled = false;
            this.areaGroup.Enabled = false;
            this.targetGroup.Enabled = false;
            this.createButton.Enabled = false;
            //self.subbasinsLabel.Text = '')
            //self.channelsLabel.Text = '')
            //self.fullHRUsLabel.Text = '')
        }       // Save channel merge parameters.
        public virtual void setChannelMergeChoice() {
            this._createHRUs.channelMergeByPercent = this.channelPercentButton.Checked;
            this._createHRUs.channelMergeThreshold = this.channelMergeSlider.Value;
        }        // Set dialog according to choice of multiple/single HRUs.
        public virtual void setHRUChoice() {
            this.landuseSoilSlopeGroup.Visible = false;
            this.areaGroup.Visible = false;
            this.targetGroup.Visible = false;
            if (this.dominantHRUButton.Checked || this.dominantLanduseButton.Checked) {
                this._createHRUs.isMultiple = false;
                this._createHRUs.isDominantHRU = this.dominantHRUButton.Checked;
                this.areaPercentChoiceGroup.Enabled = false;
                this.landuseSoilSlopeGroup.Enabled = false;
                this.areaGroup.Enabled = false;
                this.targetGroup.Enabled = false;
                this.createButton.Enabled = true;
            } else {
                this.areaPercentChoiceGroup.Enabled = true;
                this._createHRUs.isMultiple = true;
                if (this.filterLanduseButton.Checked) {
                    //this.stackedWidget.SelectedIndex = 0;
                    this.landuseSoilSlopeGroup.Visible = true;
                    this.landuseSoilSlopeGroup.Enabled = true;
                    this.landuseSlider.Enabled = true;
                    this.landuseVal.Enabled = true;
                    this.landuseButton.Enabled = true;
                    this.soilSlider.Enabled = false;
                    this.soilVal.Enabled = false;
                    this.soilButton.Enabled = false;
                    this.slopeSlider.Enabled = false;
                    this.slopeVal.Enabled = false;
                    this.areaGroup.Enabled = false;
                    this.targetGroup.Enabled = false;
                    this.createButton.Enabled = false;
                    this._createHRUs.isArea = false;
                    this._createHRUs.isTarget = false;
                } else if (this.filterAreaButton.Checked) {
                    //this.stackedWidget.SelectedIndex = 1;
                    this.areaGroup.Visible = true;
                    this.landuseSoilSlopeGroup.Enabled = false;
                    this.areaGroup.Enabled = true;
                    this.targetGroup.Enabled = false;
                    this.createButton.Enabled = true;
                    this._createHRUs.isArea = true;
                    this._createHRUs.isTarget = false;
                } else {
                    this.landuseSoilSlopeGroup.Enabled = false;
                    this.areaGroup.Enabled = false;
                    //this.stackedWidget.SelectedIndex = 2;
                    this.targetGroup.Visible = true;
                    this.targetGroup.Enabled = true;
                    this.createButton.Enabled = true;
                    this._createHRUs.isArea = false;
                    this._createHRUs.isTarget = true;
                }
                this.setAreaPercentChoice();
            }
        }       // Set dialog according to choice of area or percent thresholds.
        public virtual void setAreaPercentChoice() {
            if (!this._createHRUs.isMultiple) {
                return;
            }
            this._createHRUs.useArea = this.areaButton.Checked;
            if (this._createHRUs.useArea) {
                this.landuseLabel.Text = "Landuse (ha)";
                this.soilLabel.Text = "Soil (ha)";
                this.slopeLabel.Text = "Slope (ha)";
                this.areaLabel.Text = "Area (ha)";
            } else {
                this.landuseLabel.Text = "Landuse (%)";
                this.soilLabel.Text = "Soil (%)";
                this.slopeLabel.Text = "Slope (%)";
                this.areaLabel.Text = "Area (%)";
            }
            if (this._createHRUs.isArea) {
                var displayMaxArea = this._createHRUs.useArea ? Convert.ToInt32(this._createHRUs.maxLandscapeArea()) : 100;
                this.areaMax.Text = displayMaxArea.ToString();
                this.areaSlider.Maximum = displayMaxArea;
                var _tmp_1 = this._createHRUs.areaVal;
                if (0 < _tmp_1 && _tmp_1 <= displayMaxArea) {
                    this.areaSlider.Value = Convert.ToInt32(this._createHRUs.areaVal);
                }
            } else if (this._createHRUs.isTarget) {
                // Setting the minimum for the slider changes the slider value
                // which in turn changes CreateHRUs.targetVal.
                // So we remember the value of CreateHRUs.targetVal and restore it later.
                var target = this._createHRUs.targetVal;
                var minHRUs = this._createHRUs.countLsus();
                this.targetSlider.Minimum = minHRUs;
                this.targetMin.Text = minHRUs.ToString();
                var numHRUs = this._createHRUs.countHRUs();
                this.targetSlider.Maximum = numHRUs;
                this.targetMax.Text = numHRUs.ToString();
                // restore the target and use it to set the slider
                this._createHRUs.targetVal = target;
                var _tmp_2 = Convert.ToInt32(this._createHRUs.targetVal);
                if (minHRUs <= _tmp_2 && _tmp_2 <= numHRUs) {
                    this.targetSlider.Value = Convert.ToInt32(this._createHRUs.targetVal);
                }
            } else {
                var minCropVal = Convert.ToInt32(this._createHRUs.minMaxCropVal(this._createHRUs.useArea));
                this.landuseMax.Text = minCropVal.ToString();
                this.landuseSlider.Maximum = minCropVal;
                var _tmp_3 = this._createHRUs.landuseVal;
                if (0 <= _tmp_3 && _tmp_3 <= minCropVal) {
                    this.landuseSlider.Value = Convert.ToInt32(this._createHRUs.landuseVal);
                }
            }
        }        // Load landuse file.
        public async Task getLanduseFile() {
            await Utils.removeLayerByLegend(FileTypes.legend(FileTypes._LANDUSES));
            (string landuseFile, Layer landuseLayer) = await Utils.openAndLoadFile(FileTypes._LANDUSES, this.selectLanduse, this._gv.landuseDir, this._gv, null, Utils._LANDUSE_GROUP_NAME, clipToDEM: true);
            if (landuseFile is not null  && landuseLayer is not null) {
                this._parent.landuseFile = landuseFile;
                this._parent.landuseLayer = landuseLayer as RasterLayer;
            }
        }

        // Load soil file.
        public async Task getSoilFile() {
            await Utils.removeLayerByLegend(FileTypes.legend(FileTypes._SOILS));
            (string soilFile, Layer soilLayer) = await Utils.openAndLoadFile(FileTypes._SOILS, this.selectSoil, this._gv.soilDir, this._gv, null, Utils._SOIL_GROUP_NAME, clipToDEM: true);
            if (soilFile is not null && soilLayer is not null) {
                this._parent.soilFile = soilFile;
                this._parent.soilLayer = soilLayer as RasterLayer;
            }
        }

        // Set landuse table.
        public virtual void setLanduseTable() {
            var table = this.selectLanduseTable.Text;
            if (table == Parameters._USECSV) {
                table = this._db.readLanduseCsv();
                if (table != "") {
                    this.selectLanduseTable.Items.Insert(0, table);
                    this.selectLanduseTable.SelectedIndex = 0;
                }
            }
            this._parent.landuseTable = table;
            this.setToReadFromMaps();
        }

        // Set soil table.
        public virtual void setSoilTable() {
            var table = this.selectSoilTable.Text;
            if (table == Parameters._USECSV) {
                table = this._db.readSoilCsv();
                if (table != "") {
                    this.selectSoilTable.Items.Insert(0, table);
                    this.selectSoilTable.SelectedIndex = 0;
                }
            }
            this._parent.soilTable = table;
            this.setToReadFromMaps();
        }

        // Set plant table.
        public virtual void setPlantTable() {
            var table = this.selectPlantTable.Text;
            if (table == Parameters._USECSV) {
                table = this._db.readPlantCsv();
                if (table != "") {
                    this.selectPlantTable.Items.Insert(0, table);
                    this.selectPlantTable.SelectedIndex = 0;
                }
            }
            this._db.plantTable = table;
        }

        // Set urban table.
        public virtual void setUrbanTable() {
            var table = this.selectUrbanTable.Text;
            if (table == Parameters._USECSV) {
                table = this._db.readUrbanCsv();
                if (table != "") {
                    this.selectUrbanTable.Items.Insert(0, table);
                    this.selectUrbanTable.SelectedIndex = 0;
                }
            }
            this._db.urbanTable = table;
        }

        // Set usersoil table.
        public virtual void setUsersoilTable() {
            var table = this.selectUsersoilTable.Text;
            if (table == Parameters._USECSV) {
                table = this._db.readUsersoilCsv();
                if (table != "") {
                    this.selectUsersoilTable.Items.Insert(0, table);
                    this.selectUsersoilTable.SelectedIndex = 0;
                }
            }
            this._db.usersoilTable = table;
        }

        // Read channel merge threshold.
        public virtual void readChannelThreshold() {
            var @string = this.channelMergeVal.Text;
            if (@string == "") {
                return;
            }
            try {
                var val = Convert.ToInt32(@string);
                if (this.channelAreaButton.Checked && val > this.channelMergeSlider.Maximum) {
                    this.channelMergeSlider.Maximum = 2 * val;
                }
                this.channelMergeSlider.Value = val;
                //this.channelMergeVal.moveCursor(QTextCursor.End);
                this.setChannelMergeChoice();
            }
            catch (Exception) {
                return;
            }
        }

        // Change channel merge threshold.
        public virtual void changeChannelThreshold() {
            var val = this.channelMergeSlider.Value;
            this.channelMergeVal.Text = val.ToString();
            this.setChannelMergeChoice();
        }

        // Set maximum value for short channel merge slider for percent or hectares.
        public virtual void setChannelChoice() {
            if (this.channelPercentButton.Checked) {
                this.channelMergeSlider.Maximum = 100;
            } else {
                this.channelMergeSlider.Maximum = Convert.ToInt32(this._gv.channelThresholdArea / 10000);
            }
            this.setChannelMergeChoice();
        }

        // Read area threshold.
        public virtual void readAreaThreshold() {
            var @string = this.areaVal.Text;
            if (@string == "") {
                return;
            }
            try {
                var val = Convert.ToInt32(@string);
                // allow values outside slider range
                if (this.areaSlider.Minimum <= val && val <= this.areaSlider.Maximum) {
                    this.areaSlider.Value = val;
                }
                this._createHRUs.areaVal = val;
                //this.areaVal.moveCursor(QTextCursor.End);
            }
            catch (Exception) {
                return;
            }
            this.createButton.Enabled = true;
        }

        // Change area threshold and slider.
        public virtual void changeAreaThreshold() {
            var val = this.areaSlider.Value;
            this.areaVal.Text = val.ToString();
            this.createButton.Enabled = true;
            this._createHRUs.areaVal = val;
        }

        // Read landuse value.
        public virtual void readLanduseThreshold() {
            var @string = this.landuseVal.Text;
            if (@string == "") {
                return;
            }
            try {
                var val = Convert.ToInt32(@string);
                // allow values outside slider range
                if (this.landuseSlider.Minimum <= val && val <= this.landuseSlider.Maximum) {
                    this.landuseSlider.Value = val;
                }
                this._createHRUs.landuseVal = val;
                //this.landuseVal.moveCursor(QTextCursor.End);
            }
            catch (Exception) {
                return;
            }
        }

        // Change landuse value and slider.
        public virtual void changeLanduseThreshold() {
            var val = this.landuseSlider.Value;
            this.landuseVal.Text = val.ToString();
            this._createHRUs.landuseVal = val;
        }

        // Read soil value.
        public virtual void readSoilThreshold() {
            var @string = this.soilVal.Text;
            if (@string == "") {
                return;
            }
            try {
                var val = Convert.ToInt32(@string);
                // allow values outside slider range
                if (this.soilSlider.Minimum <= val && val <= this.soilSlider.Maximum) {
                    this.soilSlider.Value = val;
                }
                this._createHRUs.soilVal = val;
                //this.soilVal.moveCursor(QTextCursor.End);
            }
            catch (Exception) {
                return;
            }
        }

        // Change soil value and slider.
        public virtual void changeSoilThreshold() {
            var val = this.soilSlider.Value;
            this.soilVal.Text = val.ToString();
            this._createHRUs.soilVal = val;
        }

        // Read slope value.
        public virtual void readSlopeThreshold() {
            var @string = this.slopeVal.Text;
            if (@string == "") {
                return;
            }
            try {
                var val = Convert.ToInt32(@string);
                // allow values outside slider range
                if (this.slopeSlider.Minimum <= val && val <= this.slopeSlider.Maximum) {
                    this.slopeSlider.Value = val;
                }
                this._createHRUs.slopeVal = val;
                //this.slopeVal.moveCursor(QTextCursor.End);
            }
            catch (Exception) {
                return;
            }
        }

        // Change slope value and slider.
        public virtual void changeSlopeThreshold() {
            var val = this.slopeSlider.Value;
            this.slopeVal.Text = val.ToString();
            this._createHRUs.slopeVal = val;
        }

        // Read slope value.
        public virtual void readTargetThreshold() {
            var @string = this.targetVal.Text;
            if (@string == "") {
                return;
            }
            try {
                var val = Convert.ToInt32(@string);
                this.targetSlider.Value = val;
                this._createHRUs.targetVal = val;
                //this.targetVal.moveCursor(QTextCursor.End);
            }
            catch (Exception) {
                return;
            }
        }

        // Change slope value and slider.
        public virtual void changeTargetThreshold() {
            var val = this.targetSlider.Value;
            this.targetVal.Text = val.ToString();
            this._createHRUs.targetVal = val;
        }

        // Set threshold for soil according to landuse value.
        public virtual void setLanduseThreshold() {
            int minSoilVal;
            if (this._createHRUs.useArea) {
                minSoilVal = Convert.ToInt32(this._createHRUs.minMaxSoilArea());
            } else {
                minSoilVal = Convert.ToInt32(this._createHRUs.minMaxSoilPercent(this._createHRUs.landuseVal));
            }
            this.landuseSlider.Enabled = false;
            this.landuseVal.Enabled = false;
            this.landuseButton.Enabled = false;
            this.soilSlider.Enabled = true;
            this.soilVal.Enabled = true;
            this.soilButton.Enabled = true;
            this.soilSlider.Maximum = minSoilVal;
            this.soilMax.Text = minSoilVal.ToString();
            var _tmp_1 = this._createHRUs.soilVal;
            if (0 <= _tmp_1 && _tmp_1 <= minSoilVal) {
                this.soilSlider.Value = Convert.ToInt32(this._createHRUs.soilVal);
            }
        }

        // Set threshold for slope according to landuse and soil values.
        public virtual void setSoilThreshold() {
            int minSlopeVal;
            this.soilSlider.Enabled = false;
            this.soilVal.Enabled = false;
            this.soilButton.Enabled = false;
            if (this._db.slopeLimits.Count > 0) {
                if (this._createHRUs.useArea) {
                    minSlopeVal = Convert.ToInt32(this._createHRUs.minMaxSlopeArea());
                } else {
                    minSlopeVal = Convert.ToInt32(this._createHRUs.minMaxSlopePercent(this._createHRUs.landuseVal, this._createHRUs.soilVal));
                }
                this.slopeSlider.Enabled = true;
                this.slopeVal.Enabled = true;
                this.slopeSlider.Maximum = minSlopeVal;
                this.slopeMax.Text = minSlopeVal.ToString();
                var _tmp_1 = this._createHRUs.slopeVal;
                if (0 <= _tmp_1 && _tmp_1 <= minSlopeVal) {
                    this.slopeSlider.Value = Convert.ToInt32(this._createHRUs.slopeVal);
                }
            }
            this.createButton.Enabled = true;
        }

        // Insert a new slope limit.
        public virtual void insertSlope() {
            var txt = this.slopeBand.Text;
            double num;
            if (txt == "") {
                return;
            }
            try {
                num = Convert.ToDouble(txt);
            }
            catch (Exception) {
                Utils.information(string.Format("Cannot parse {0} as a number", txt), this._gv.isBatch);
                return;
            }
            ListFuns.insertIntoSortedList(num, this._db.slopeLimits, true);
            this.slopeBrowser.Text = Utils.slopesToString(this._db.slopeLimits);
            this.slopeBand.Clear();
        }

        // Reset to no slope bands.
        public virtual void clearSlopes() {
            this._db.slopeLimits = new List<double>();
            this.slopeBrowser.Text = "[0, 9999]";
            this.slopeBand.Clear();
        }       // Update progress label with message; emit message for display in testing.

        public virtual void progress(string msg) {
            var progressLabel = this.HRUsTab.SelectedIndex == 0 ? this.progressLabel1 : this.progressLabel2;
            Utils.progress(msg, progressLabel);
        }

        // Read landuse, soil, LSU, HRU settings from the project file.
        public async Task readProj() {
            string slopeBandsFile;
            int index;
            string soilFile;
            string possFile;
            Layer layer;
            string landuseFile;
            var proj = this._gv.proj;
            var title = proj.Name;
            bool found;
            (landuseFile, found) = proj.readEntry(title, "landuse/file", "");
            RasterLayer landuseLayer = null;
            if (found && landuseFile != "") {
                landuseLayer = (await Utils.getLayerByFilename(landuseFile, FileTypes._LANDUSES, this._gv, null, Utils._LANDUSE_GROUP_NAME)).Item1 as RasterLayer;
            } else {
                layer = Utils.getLayerByLegend(FileTypes.legend(FileTypes._LANDUSES));
                possFile = await Utils.layerFilename(layer);
                if (possFile is not null && File.Exists(possFile)) {
                    if (Utils.question(string.Format("Use {0} as {1} file?", possFile, FileTypes.legend(FileTypes._LANDUSES)), this._gv.isBatch, true) == MessageBoxResult.Yes) {
                        landuseLayer = layer as RasterLayer;
                        landuseFile = possFile;
                    }
                }
            }
            if (landuseLayer is not null) {
                this.selectLanduse.Text = landuseFile;
                this._parent.landuseFile = landuseFile;
                this._parent.landuseLayer = landuseLayer;
            }
            (soilFile, found) = proj.readEntry(title, "soil/file", "");
            RasterLayer soilLayer = null;
            if (found && soilFile != "") {
                soilLayer = (await Utils.getLayerByFilename(soilFile, FileTypes._SOILS, this._gv, null, Utils._SOIL_GROUP_NAME)).Item1 as RasterLayer;
            } else {
                layer = Utils.getLayerByLegend(FileTypes.legend(FileTypes._SOILS));
                possFile = await Utils.layerFilename(layer);
                if (possFile is not null && File.Exists(possFile)) {
                    if (Utils.question(string.Format("Use {0} as {1} file?", possFile, FileTypes.legend(FileTypes._SOILS)), this._gv.isBatch, true) == MessageBoxResult.Yes) {
                        soilLayer = layer as RasterLayer;
                        soilFile = possFile;
                    }
                }
            }
            if (soilLayer is not null) {
                this.selectSoil.Text = soilFile;
                this._parent.soilFile = soilFile;
                this._parent.soilLayer = soilLayer;
            }
            //usersoil is the default.  Unchecked if either STATSGO or SSURGO is checked
            this.usersoilButton.Checked = true;
            (this._db.useSTATSGO, found) = proj.readBoolEntry(title, "soil/useSTATSGO", false);
            if (found && this._db.useSTATSGO) {
                this.STATSGOButton.Checked = true;
            }
            (this._db.useSSURGO, found) = proj.readBoolEntry(title, "soil/useSSURGO", false);
            if (found && this._db.useSSURGO) {
                this.SSURGOButton.Checked = true;
            }
            (string landuseTable, found) = proj.readEntry("", "landuse/table", "");
            if (found && landuseTable != "") {
                index = this.selectLanduseTable.FindString(landuseTable);
                if (index >= 0) {
                    this.selectLanduseTable.SelectedIndex = index;
                }
                this._parent.landuseTable = landuseTable;
            }
            (this._db.waterLanduse, _) = proj.readNumEntry("", "landuse/water", -1);
            string plantTable;
            (plantTable, found) = proj.readEntry("", "landuse/plant", "");
            if (found && plantTable != "") {
                index = this.selectPlantTable.FindString(plantTable);
                if (index >= 0) {
                    this.selectPlantTable.SelectedIndex = index;
                }
            } else {
                plantTable = "plant";
            }
            this._db.plantTable = plantTable;
            string urbanTable;
            (urbanTable, found) = proj.readEntry("", "landuse/urban", "");
            if (found && urbanTable != "") {
                index = this.selectUrbanTable.FindString(urbanTable);
                if (index >= 0) {
                    this.selectUrbanTable.SelectedIndex = index;
                }
            } else {
                urbanTable = "urban";
            }
            this._db.urbanTable = urbanTable;
            string soilTable;
            (soilTable, found) = proj.readEntry("", "soil/table", "");
            if (found && soilTable != "") {
                index = this.selectSoilTable.FindString(soilTable);
                if (index >= 0) {
                    this.selectSoilTable.SelectedIndex = index;
                }
                this._parent.soilTable = soilTable;
            }
            string plantSoilDatabase;
            (plantSoilDatabase, found) = proj.readEntry(title, "soil/database", "");
            if (found && plantSoilDatabase != "") {
                this._db.plantSoilDatabase = plantSoilDatabase;
                this._db.plantSoilDatabaseSelected = true;
            } else {
                this._db.plantSoilDatabase = this._db.dbFile;
            }
            this.plantSoilDatabase.Text = this._db.plantSoilDatabase;
            this._db.soilDatabase = Utils.join(this._gv.dbPath, Parameters._SOILDB);
            string usersoilTable;
            (usersoilTable, found) = proj.readEntry("", "soil/databaseTable", "");
            if (found && usersoilTable != "") {
                this._db.usersoilTable = usersoilTable;
            } else {
                // usersoilTable only used for user-chosen tables
                if (this._db.useSSURGO || this._db.useSTATSGO) {
                    this._db.usersoilTable = "";
                } else {
                    this._db.usersoilTable = "usersoil";
                }
            }
            (bool useLandscapes, found) = proj.readBoolEntry(title, "lsu/useLandscapes", false);
            if (found) {
                this._gv.useLandscapes = useLandscapes;
            }
            (bool useLeftRight, found) = proj.readBoolEntry(title, "lsu/useLeftRight", false);
            if (found) {
                this._gv.useLeftRight = useLeftRight;
            }
            (string floodFile, found) = proj.readEntry(title, "lsu/floodplainFile", "");
            if (found) {
                if (File.Exists(floodFile)) {
                    this._gv.floodFile = floodFile;
                }
            }
            (int reservoirThresholdNoLandscape, _) = proj.readNumEntry(title, "lsu/thresholdResNoFlood", 101);
            (int reservoirThresholdFloodplain, _) = proj.readNumEntry(title, "lsu/thresholdResFlood", 101);
            if (useLandscapes) {
                this.reservoirThreshold.Value = reservoirThresholdFloodplain;
            } else {
                this.reservoirThreshold.Value = reservoirThresholdNoLandscape;
            }
            (double channelThresholdArea, found) = proj.readNumEntry(title, "delin/thresholdCh", 0);
            if (found && channelThresholdArea > 0) {
                this._gv.channelThresholdArea = channelThresholdArea * this._gv.cellArea;
            }
            (bool channelMergeByPercent, _) = proj.readBoolEntry(title, "lsu/channelMergeByPercent", true);
            if (channelMergeByPercent) {
                this.channelPercentButton.Checked = true;
            } else {
                this.channelAreaButton.Checked = true;
            }
            (double channelMergeVal, _) = proj.readNumEntry(title, "lsu/channelMergeVal", 0);
            this.channelMergeVal.Text = channelMergeVal.ToString();
            this.readChannelThreshold();
            (int elevBandsThreshold, found) = proj.readNumEntry(title, "hru/elevBandsThreshold", 0);
            if (found) {
                this._gv.elevBandsThreshold = elevBandsThreshold;
            }
            (int numElevBands, found) = proj.readNumEntry(title, "hru/numElevBands", 0);
            if (found) {
                this._gv.numElevBands = numElevBands;
            }
            (string slopeBands, found) = proj.readEntry("", "hru/slopeBands", "");
            if (found && slopeBands != "") {
                this._db.slopeLimits = Utils.parseSlopes(slopeBands);
            }
            (slopeBandsFile, found) = proj.readEntry(title, "hru/slopeBandsFile", "");
            RasterLayer slopeBandsLayer = null;
            if (found && slopeBandsFile != "") {
                slopeBandsLayer = (await Utils.getLayerByFilename(slopeBandsFile, FileTypes._SLOPEBANDS, this._gv, null, Utils._SLOPE_GROUP_NAME)).Item1 as RasterLayer;
            } else {
                layer = Utils.getLayerByLegend(FileTypes.legend(FileTypes._SLOPEBANDS));
                possFile = await Utils.layerFilename(layer);
                if (possFile is not null && File.Exists(possFile)) {
                    if (Utils.question(string.Format("Use {0} as {1} file?", possFile, FileTypes.legend(FileTypes._SLOPEBANDS)), this._gv.isBatch, true) == MessageBoxResult.Yes) {
                        slopeBandsLayer = layer as RasterLayer;
                        slopeBandsFile = possFile;
                    }
                }
            }
            if (slopeBandsLayer is not null) {
                this._gv.slopeBandsFile = slopeBandsFile;
            } else {
                this._gv.slopeBandsFile = "";
            }
            (this._createHRUs.isMultiple, found) = proj.readBoolEntry(title, "hru/isMultiple", false);
            (this._createHRUs.isDominantHRU, found) = proj.readBoolEntry(title, "hru/isDominantHRU", true);
            (this._createHRUs.isArea, found) = proj.readBoolEntry(title, "hru/isArea", false);
            (this._createHRUs.isTarget, found) = proj.readBoolEntry(title, "hru/isTarget", false);
            (this._createHRUs.useArea, found) = proj.readBoolEntry(title, "hru/useArea", false);
            if (this._createHRUs.isMultiple) {
                if (this._createHRUs.isArea) {
                    this.filterAreaButton.Checked = true;
                } else if (this._createHRUs.isTarget) {
                    this.targetButton.Checked = true;
                } else {
                    this.filterLanduseButton.Checked = true;
                }
            } else if (this._createHRUs.isDominantHRU) {
                this.dominantHRUButton.Checked = true;
            } else {
                this.dominantLanduseButton.Checked = true;
            }
            if (this._createHRUs.useArea) {
                this.areaButton.Checked = true;
            } else {
                this.percentButton.Checked = true;
            }
            (this._createHRUs.areaVal, found) = proj.readNumEntry(title, "hru/areaVal", 0);
            if (found && this._createHRUs.areaVal > 0) {
                this.areaVal.Text = this._createHRUs.areaVal.ToString();
            }
            (this._createHRUs.landuseVal, found) = proj.readNumEntry(title, "hru/landuseVal", 0);
            if (found && this._createHRUs.landuseVal > 0) {
                this.landuseVal.Text = this._createHRUs.landuseVal.ToString();
            }
            (this._createHRUs.soilVal, found) = proj.readNumEntry(title, "hru/soilVal", 0);
            if (found && this._createHRUs.soilVal > 0) {
                this.soilVal.Text = this._createHRUs.soilVal.ToString();
            }
            (this._createHRUs.slopeVal, found) = proj.readNumEntry(title, "hru/slopeVal", 0);
            if (found && this._createHRUs.slopeVal > 0) {
                this.slopeVal.Text = this._createHRUs.slopeVal.ToString();
            }
            (this._createHRUs.targetVal, found) = proj.readNumEntry(title, "hru/targetVal", 0);
            if (found && this._createHRUs.targetVal > 0) {
                this.targetVal.Text = this._createHRUs.targetVal.ToString();
            }
            (bool useGWFlow, found) = proj.readBoolEntry(title, "hru/useGWFlow", false);
            if (found && useGWFlow) {
                this.gwGroupBox.Visible = true;
                this.gwflowButton.Checked = true;
            }
        }

        // Write landuse, soil, and landscape choices to project file.
        public virtual void saveProjPart1() {
            var proj = this._gv.proj;
            this.saveEntries1(proj);
        }

        // Write entries for landuse, soil, and landscape choices to project file.
        public virtual void saveEntries1(Proj proj) {
            var title = proj.Name;
            proj.writeEntry(title, "landuse/file", proj.writePath(this._parent.landuseFile));
            proj.writeEntry(title, "soil/file", proj.writePath(this._parent.soilFile));
            proj.writeEntry(title, "landuse/table", this._parent.landuseTable);
            proj.writeEntry(title, "landuse/plant", this._db.plantTable);
            proj.writeEntry(title, "landuse/urban", this._db.urbanTable);
            proj.writeNumEntry(title, "landuse/water", this._db.waterLanduse);
            proj.writeEntry(title, "soil/table", this._parent.soilTable);
            proj.writeEntryBool(title, "soil/useSTATSGO", this._db.useSTATSGO);
            proj.writeEntryBool(title, "soil/useSSURGO", this._db.useSSURGO);
            proj.writeEntry(title, "soil/database", proj.writePath(this._db.plantSoilDatabase));
            proj.writeEntry(title, "soil/databaseTable", this._db.usersoilTable);
            proj.writeEntryBool(title, "lsu/useLandscapes", this._gv.useLandscapes);
            if (this._gv.useLandscapes) {
                proj.writeNumEntry(title, "lsu/thresholdResFlood", (int)this.reservoirThreshold.Value);
            } else {
                proj.writeNumEntry(title, "lsu/thresholdResNoFlood", (int)this.reservoirThreshold.Value);
            }
            proj.writeEntryBool(title, "lsu/useLeftRight", this._gv.useLeftRight);
            proj.writeEntry(title, "lsu/floodplainFile", proj.writePath(this._gv.floodFile));
            proj.writeEntryBool(title, "lsu/channelMergeByPercent", this.channelPercentButton.Checked);
            proj.writeNumEntry(title, "lsu/channelMergeVal", this.channelMergeSlider.Value);
        }

        // Write settings to the project file.
        public virtual void saveProj() {
            var proj = this._gv.proj;
            this.saveEntries1(proj);
            //this.saveEntries2(proj);
        }

        // Write entries for hru choices to project file.
        public virtual void saveEntries2(Proj proj) {
            var title = proj.Name;
            proj.writeNumEntry(title, "hru/elevBandsThreshold", this._gv.elevBandsThreshold);
            proj.writeNumEntry(title, "hru/numElevBands", this._gv.numElevBands);
            proj.writeEntry(title, "hru/slopeBands", Utils.slopesToString(this._db.slopeLimits));
            proj.writeEntry(title, "hru/slopeBandsFile", proj.writePath(this._gv.slopeBandsFile));
            proj.writeEntryBool(title, "hru/isMultiple", this._createHRUs.isMultiple);
            proj.writeEntryBool(title, "hru/isDominantHRU", this._createHRUs.isDominantHRU);
            proj.writeEntryBool(title, "hru/isArea", this._createHRUs.isArea);
            proj.writeEntryBool(title, "hru/isTarget", this._createHRUs.isTarget);
            proj.writeEntryBool(title, "hru/useArea", this._createHRUs.useArea);
            proj.writeNumEntry(title, "hru/areaVal", this._createHRUs.areaVal);
            proj.writeNumEntry(title, "hru/landuseVal", this._createHRUs.landuseVal);
            proj.writeNumEntry(title, "hru/soilVal", this._createHRUs.soilVal);
            proj.writeNumEntry(title, "hru/slopeVal", this._createHRUs.slopeVal);
            proj.writeNumEntry(title, "hru/targetVal", this._createHRUs.targetVal);
            proj.writeEntryBool(title, "hru/useGWFlow", this.gwflowButton.Checked);
        }

        private void usersoilButton_CheckedChanged(object sender, EventArgs e) {
            this.setSoilData();
        }

        private void STATSGOButton_CheckedChanged(object sender, EventArgs e) {
            this.setSoilData();
        }

        private void SSURGOButton_CheckedChanged(object sender, EventArgs e) {
            this.setSoilData();
        }

        private void selectPlantSoilDatabaseButton_Click(object sender, EventArgs e) {
            this.selectPlantSoilDatabase();
        }

        private void channelPercentButton_CheckedChanged(object sender, EventArgs e) {
            this.setChannelChoice();
        }

        private void channelAreaButton_CheckedChanged(object sender, EventArgs e) {
            this.setChannelChoice();
        }

        private void channelMergeVal_TextChanged(object sender, EventArgs e) {
            this.readChannelThreshold();
        }

        private void channelMergeSlider_Scroll(object sender, EventArgs e) {
            this.changeChannelThreshold();
        }

        private void reservoirThreshold_ValueChanged(object sender, EventArgs e) {
            this.setRead();
        }

        private void floodplainCombo_SelectedIndexChanged(object sender, EventArgs e) {
            this.setToReadFromMaps();
        }

        private void readFromMaps_CheckedChanged(object sender, EventArgs e) {
            this.setReadChoice();
        }

        private void readFromPrevious_CheckedChanged(object sender, EventArgs e) {
            this.setReadChoice();
        }

        private void dominantHRUButton_CheckedChanged(object sender, EventArgs e) {
            this.setHRUChoice();
        }

        private void dominantLanduseButton_CheckedChanged(object sender, EventArgs e) {
            this.setHRUChoice();
        }

        private void filterLanduseButton_CheckedChanged(object sender, EventArgs e) {
            this.setHRUChoice();
        }

        private void filterAreaButton_CheckedChanged(object sender, EventArgs e) {
            this.setHRUChoice();
        }

        private void targetButton_CheckedChanged(object sender, EventArgs e) {
            this.setHRUChoice();
        }

        private void percentButton_CheckedChanged(object sender, EventArgs e) {
            this.setHRUChoice();
        }

        private void areaButton_CheckedChanged(object sender, EventArgs e) {
            this.setHRUChoice();
        }

        private async void selectLanduseButton_Click(object sender, EventArgs e) {
            await this.getLanduseFile();
        }

        private async void selectSoilButton_Click(object sender, EventArgs e) {
            await this.getSoilFile();
        }

        private void selectLanduseTable_SelectedIndexChanged(object sender, EventArgs e) {
            this.setLanduseTable();
        }

        private void selectSoilTable_SelectedIndexChanged(object sender, EventArgs e) {
            this.setSoilTable();
        }

        private void selectPlantTable_SelectedIndexChanged(object sender, EventArgs e) {
            this.setPlantTable();
        }

        private void selectUrbanTable_SelectedIndexChanged(object sender, EventArgs e) {
            this.setUrbanTable();
        }

        private void selectUsersoilTable_SelectedIndexChanged(object sender, EventArgs e) {
            this.setUsersoilTable();
        }

        private void insertButton_Click(object sender, EventArgs e) {
            this.insertSlope();
        }

        private void slopeBand_KeyUp(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) {
                this.insertSlope();
            }
        }

        private void clearButton_Click(object sender, EventArgs e) {
            this.clearSlopes();
        }

        private async void readButton_Click(object sender, EventArgs e) {
            var result = await this._parent.readFiles();
            Utils.loginfo($"readFiles returned {result}");
        }

        private async void createButton_Click(object sender, EventArgs e) {
            await this._parent.calcHRUs();
        }

        private void cancelButton_Click(object sender, EventArgs e) {
            this._parent.cancel();
        }

        private void areaVal_TextChanged(object sender, EventArgs e) {
            this.readAreaThreshold();
        }

        private void areaSlider_Scroll(object sender, EventArgs e) {
            this.changeAreaThreshold();
        }

        private void landuseVal_TextChanged(object sender, EventArgs e) {
            this.readLanduseThreshold();

        }

        private void landuseSlider_Scroll(object sender, EventArgs e) {
            this.changeLanduseThreshold();
        }

        private void soilVal_TextChanged(object sender, EventArgs e) {
            this.readSoilThreshold();
        }

        private void soilSlider_Scroll(object sender, EventArgs e) {
            this.changeSoilThreshold();
        }

        private void slopeVal_TextChanged(object sender, EventArgs e) {
            this.readSlopeThreshold();
        }

        private void slopeSlider_Scroll(object sender, EventArgs e) {
            this.changeSlopeThreshold();
        }

        private void landuseButton_Click(object sender, EventArgs e) {
            this.setLanduseThreshold();
        }

        private void soilButton_Click(object sender, EventArgs e) {
            this.setSoilThreshold();
        }

        private void targetVal_TextChanged(object sender, EventArgs e) {
            this.readTargetThreshold();
        }

        private void targetSlider_Scroll(object sender, EventArgs e) {
            this.changeTargetThreshold();
        }

        private void exemptButton_Click(object sender, EventArgs e) {
            this._parent.doExempt();
        }

        private void splitButton_Click(object sender, EventArgs e) {
            this._parent.doSplit();
        }

        private void elevBandsButton_Click(object sender, EventArgs e) {
            this._parent.doElevBands();
        }

        private void HRUsTab_SelectedIndexChanged(object sender, EventArgs e) {
            this.switchTabs();
        }

        public GroupBox gwGroup {
            get { return this.gwGroupBox; }
        }

        public TabControl mainTab {
            get { return this.HRUsTab; } 
        }

        public TabPage tab1 {
            get { return this.tabPage1; }
        }

        public TabPage tab2 {
            get { return this.tabPage2; }
        }

        public Label progressLabel {
            get {
                if (this.HRUsTab.SelectedIndex == 0) {
                    return this.progressLabel1;
                } else {
                    return this.progressLabel2;
                }
            }
        }

        public ComboBox floodplainComboBox {
            get { return this.floodplainCombo; }
        }

        public string selectLanduseTableText {
            get { return this.selectLanduseTable.Text; }
        }

        public string selectSoilTableText {
            get { return this.selectSoilTable.Text; }
        }

        public bool remergeChecked {
            get { return this.remerge.Checked; }
            set { this.remerge.Checked = value; }
        }

        public bool readFromMapsChecked {
            get { return this.readFromMaps.Checked; }
        }

        public bool readFromPreviousChecked {
            get { return this.readFromPrevious.Checked; }
        }

        public bool generateFullHRUsChecked {
            get { return this.generateFullHRUs.Checked; }
        }

        public bool dominantHRUButtonChecked {
            get { return this.dominantHRUButton.Checked; }
        }

        public bool dominantLanduseButtonChecked {
            get { return this.dominantLanduseButton.Checked; }
        }

        public bool areaButtonChecked {
            get { return this.areaButton.Checked; }
        }

        public bool gwflowButtonChecked {
            get { return this.gwflowButton.Checked; }
        }

        public int reservoirThresholdValue {
            get { return (int)this.reservoirThreshold.Value; }
        }

        public int channelMergeSliderValue {
            get { return this.channelMergeSlider.Value; }
        }

        public ProgressBar getProgressBar {
            get { return this.progressBar; }
        }

        public bool remergeEnabled {
            get { return this.remerge.Enabled; }
            set { this.remerge.Enabled = value; }
        }

        public bool readFromPreviousEnabled {
            get { return this.readFromPrevious.Enabled;}
            set { this.readFromPrevious.Enabled = value;}
        }

        public bool hruChoiceGroupEnabled {
            get { return (this.hruChoiceGroup.Enabled); }
            set { this.hruChoiceGroup.Enabled = value; }
        }

        public bool areaPercentChoiceGroupEnabled {
            get { return (this.areaPercentChoiceGroup.Enabled); }
            set { this.areaPercentChoiceGroup.Enabled = value; }
        }

        public bool slopeGroupEnabled {
            get { return (this.slopeGroup.Enabled); }
            set { this.slopeGroup.Enabled = value; }
        }

        public bool generateFullHRUsEnabled {
            get { return (this.generateFullHRUs.Enabled); }
            set { this.generateFullHRUs.Enabled = value; }
        }

        public bool elevBandsButtonEnabled {
            get { return (this.elevBandsButton.Enabled); }
            set { this.elevBandsButton.Enabled = value; }
        }

        public bool splitButtonEnabled {
            get { return (this.splitButton.Enabled); }
            set { this.splitButton.Enabled = value; }
        }

        public bool exemptButtonEnabled {
            get { return (this.exemptButton.Enabled); }
            set { this.exemptButton.Enabled = value; }
        }

        public bool slopeSliderEnabled {
            get { return (this.slopeSlider.Enabled); }
            set { this.slopeSlider.Enabled = value; }
        }

        public bool slopeValEnabled {
            get { return (this.slopeVal.Enabled); }
            set { this.slopeVal.Enabled = value; }
        }

        public bool areaGroupEnabled {
            get { return (this.areaGroup.Enabled); }
            set { this.areaGroup.Enabled = value; }
        }

        public bool targetGroupEnabled {
            get { return (this.targetGroup.Enabled); }
            set { this.targetGroup.Enabled = value; }
        }

        public bool landuseSoilSlopeGroupEnabled {
            get { return (this.landuseSoilSlopeGroup.Enabled); }
            set { this.landuseSoilSlopeGroup.Enabled = value; }
        }

        public string channelsLabelText {
            get { return this.channelsLabel.Text; }
            set { this.channelsLabel.Text = value; }
        }

        public string fullHRUsLabelText {
            get { return this.fullHRUsLabel.Text; }
            set { this.fullHRUsLabel.Text = value; }
        }

        public string subbasinsLabelText {
            get { return this.subbasinsLabel.Text; }
            set { this.subbasinsLabel.Text = value; }
        }

        public string slopeBandText {
            get { return this.slopeBand.Text; }
        }
    }
}
