#if TESTING
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ObjectiveC;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.TextFormatting;

namespace ArcSWATPlus {

    public class mimicTextBox {
        public string Text;
        public void AppendText(string txt) { Text = Text + txt; }
        public void Clear() { Text = ""; }
        public Color ForeColor = Color.Black;

        public bool ReadOnly = false;
    }

    public class mimicComboBox {
        public string Text {
            get {
                if (0 <= SelectedIndex && SelectedIndex < Items.Count) {
                    return Items[SelectedIndex].ToString();
                } else if (SelectedItem is not null) {
                    return SelectedItem.ToString();
                } else { return null; }
            }
            set { 
                var str = value;
                SelectedItem = str;
                var indx = FindString(str);
                if (indx != -1) { 
                    SelectedIndex = indx; 
                } else {
                    throw new Exception(string.Format("Illegal use of ComboBox setting Text to {0} when not in Items", str));
                }
            }
        }
        public List<object> Items = new List<object>();
        public int SelectedIndex;
        public object SelectedItem;
        public bool Enabled;
        public int FindString(string str) {
            return Items.FindIndex(e => e.ToString().StartsWith(str));
        }
    }

    public class MainFormStub {

        public string Text;

        public GlobalVars _gv;

        public MainFormStub() {

        }

        public void initButtons() { }

        public void Show() { }

        public void setProject(string project) { }

        public void checkReports() { }

        public void allowEdit() { }

        public void allowCreateHRU() { }

        public void showReports(List<string> reports) { }

        public void allowVisualise() { }

    }

    public class ParamFormStub {
        private Parameters _parent;
        public bool useMPI = false;

        public string MPIText = "";

        public System.Drawing.Point? Location = null;
        public int pointSizeValue = 0;
        public double burninDepthValue = 0;
        public double widthMultValue = 0;
        public double widthExpValue = 0;
        public double depthMultValue = 0;
        public double depthExpValue = 0;
        public double reachSlope = 0;
        public double tributarySlope = 0;
        public double meanSlope = 0;
        public double mainLength = 0;
        public double tributaryLength = 0;
        public double upslopeHRUDrainValue = 0;
        public string SWATPlusText = "";
        public int decimalPlacesValue;
        public ParamFormStub(Parameters parent) { 
        this._parent = parent;
        }
        public void ShowDialog() { }
        public void Close() { }
    }

    public class ExportTableFormStub {
        private ExportTable _parent;

        public Func<string, int> databasesAdd = null;
        public int databasesIndex = 0;
        public string databasesText = "";
        public Action tablesClear = null;
        public Func<string, int> tablesAdd = null;
        public int tablesIndex = 0;
        public string tablesText = string.Empty;

        public ExportTableFormStub(ExportTable parent) {
            this._parent = parent;
        }
        public void ShowDialog() { }
        public void Close() { }
    }

    public class DelineationFormStub {

        private Delineation _parent;

        private GlobalVars _gv;

        public double areaOfCell;

        public bool changing;

        public List<int> dX;

        public List<int> dY;

        public bool Enabled = true;
        public bool delinRecalcButtonChecked = false;
        public bool lakeMessages = false;
        public bool delinLakesTabEnabled = true;
        public bool delinMergeTabEnabled = true;
        public mimicTextBox delinSnapThreshold = new mimicTextBox();
        public string numCellsChText = "";
        public string numCellsStText = "";
        public bool useOutletsChecked = false;
        public bool checkBurnChecked = false;
        public bool showTaudemChecked = false;
        //public FormWindowState windowState;
        public int numberOfProcesses = 0;
        public mimicComboBox delinLakeIdCombo = new mimicComboBox();
        public int delinTabWidgetIndex = 0;
        public string snappedLabelText = "";
        public string selectSubbasinsLabelText = "";
        public string textBrowserText = "";
        public string crsLabelText = "";
        public mimicComboBox delinHorizontalCombo = new mimicComboBox();
        public mimicTextBox delinSizeEdit = new mimicTextBox();
        public mimicTextBox delinAreaEdit = new mimicTextBox();
        public mimicTextBox delinNorthEdit = new mimicTextBox();
        public mimicTextBox delinSouthEdit = new mimicTextBox();
        public mimicTextBox delinEastEdit = new mimicTextBox();
        public mimicTextBox delinWestEdit = new mimicTextBox();
        public string selectOutletsInteractiveLabelText = "";
        public int gridSizeValue = 0;
        public string areaStText = "";

        public DelineationFormStub(GlobalVars gv, Delineation parent) {
            this._parent = parent;
            this._gv = gv;
            this.delinHorizontalCombo.Items.Add(Parameters._METRES);
            this.delinHorizontalCombo.Items.Add(Parameters._FEET);
            this.delinHorizontalCombo.Items.Add(Parameters._DEGREES);
            this.delinHorizontalCombo.Items.Add(Parameters._UNKNOWN);
            delinSnapThreshold.Text = "300";
        }

        public void changeUseGrid() { }
        public void Show() { }
        public void saveProj() { }

        public mimicTextBox getTaudemOutput = new mimicTextBox();
        public mimicTextBox delinSelectDem = new mimicTextBox();

        public mimicTextBox delinSelectBurn = new mimicTextBox();

        public mimicTextBox delinSelectStreams = new mimicTextBox();

        public mimicTextBox delinSelectOutlets = new mimicTextBox();

        public mimicTextBox delinSelectExistOutlets = new mimicTextBox();

        public mimicTextBox delinSelectSubbasins = new mimicTextBox();

        public mimicTextBox delinSelectWshed = new mimicTextBox();

        public mimicTextBox delinSelectLakes = new mimicTextBox();

        public void setNumCellsSt() { }
        public void setNumCellsCh() { }
        public void setAreaOfCell(double areaSqM) { 
            this._gv.cellArea = areaSqM;
        }
        public void setAreaCh() { }
        public void setAreaSt() { }
        public void setDrainage() { }
        public void setMergeResGroups(bool isDelineated) { }
        public void enableThresholds() { }
        public void progress(string msg) { }
        public void Close() { }
    }

    public class LandscapeFormStub {

        public GlobalVars _gv;
        public Landscape _parent;

        public int bufferMultiplierValue = 10;

        public string ridgeThresholdCellsText = "";

        public string branchThresholdText = "";
        public int methodTabSelectedIndex = 0;
        public float slopePositionSpinBoxValue = 0;

        public bool Visible = true;
        public LandscapeFormStub(GlobalVars _gv, Landscape _parent) {
            this._gv = _gv;
            this._parent = _parent;
        }

        public void init(int numCellsSt, int branchThresh) {
            bufferMultiplierValue = 10;
            ridgeThresholdCellsText = numCellsSt.ToString();
            branchThresholdText = branchThresh.ToString();
            slopePositionSpinBoxValue = 0.1F;
        }
        public void Show() { }
        public void Close() { }
    }

    public class SelectSubbasinStub {

        public Delineation _parent;
        public GlobalVars _gv;
        public FeatureLayer _subbasinsLayer;
        public mimicTextBox threshold = new();
        public double meanArea;

        public bool areaButtonChecked = false;

        public int areaIndx;
        public SelectSubbasinStub(FeatureLayer subbasinsLayer, Delineation parent, GlobalVars gv) {
            this._gv = gv;
            this._parent = parent;
            this._subbasinsLayer = subbasinsLayer;
            this.threshold.Text = "5";
        }

        public async Task setup() {
            this.areaIndx = await _gv.topo.getIndex(this._subbasinsLayer, Topology._AREA);
            double area = 0;
            var count = 0;
            await QueuedTask.Run(() => {
                using (RowCursor rowCursor = this._subbasinsLayer.Search(null)) {
                    while (rowCursor.MoveNext()) {
                        using (Row polygon = rowCursor.Current) {
                            area += Convert.ToDouble(polygon[this.areaIndx]);
                            count++;
                        }
                    }
                }
            });
            if (count == 0) {
                this.meanArea = 0;
            } else {
                this.meanArea = area / count;
            }
        }

        public async Task selectSubbasinsByThreshold(double threshold) {
            //double threshold, thresholdM2;
            //var num = this.threshold.Text;
            //if (string.IsNullOrEmpty(num)) {
            //    Utils.error("No threshold is set", this._gv.isBatch);
            //    return;
            //}
            //try {
            //    threshold = Double.Parse(num);
            //}
            double thresholdM2;
            if (this.areaButtonChecked) {
                thresholdM2 = threshold * 10000;
            } else {
                thresholdM2 = this.meanArea * threshold / 100;
            }
            Selection selection = null;
            IReadOnlyList<long> selected = new List<long>();
            await QueuedTask.Run(() => {
                selection = this._subbasinsLayer.GetSelection();
                selected = selection.GetObjectIDs();
                var rows = new List<long>();
                using (RowCursor rowCursor = this._subbasinsLayer.Search(null)) {
                    while (rowCursor.MoveNext()) {
                        using (Row polygon = rowCursor.Current) {
                            var area = Convert.ToDouble(polygon[this.areaIndx]);
                            if (area < thresholdM2) {
                                var id = polygon.GetObjectID();
                                if (!selected.Contains(id)) {
                                    rows.Add(id);
                                }
                            }
                        }
                    }
                }
                selection.Add(rows);
                this._subbasinsLayer.SetSelection(selection);
            });
        }
    }

    public class HRUsFormStub {

        public HRUs _parent;
        public CreateHRUs _createHRUs;
        public GlobalVars _gv;

        public DBUtils _db;

        public GwflowHruStub _gw;

        public GridGenerator _gridGen;

        public mimicTextBox plantSoilDatabase = new mimicTextBox();

        public mimicTextBox hrusSelectLanduse = new mimicTextBox();

        public mimicComboBox hrusSelectLanduseTable = new mimicComboBox();

        public mimicTextBox hrusSelectSoil = new mimicTextBox();

        public mimicComboBox hrusSelectSoilTable = new mimicComboBox();

        public mimicComboBox hrusSelectPlantTable = new mimicComboBox();

        public mimicComboBox hrusSelectUrbanTable = new mimicComboBox();

        public mimicComboBox hrusSelectUsersoilTable = new mimicComboBox();

        public bool usersoilButtonChecked = true;
        public bool STATSGOButtonChecked = false;
        public bool SSURGOButtonChecked = false;
        public void setToReadFromMaps() { }

        public int progressBarValue = 0;
        public mimicComboBox hrusFloodplainCombo = new mimicComboBox();

        public bool remergeChecked = false;

        public bool remergeEnabled = true;

        public int channelMergeSliderValue = 0;

        public string subbasinsLabelText = "";

        public string channelsLabelText = "";

        public string fullHRUsLabelText = "";

        public bool hruChoiceGroupEnabled = true;

        public bool splitButtonEnabled = true;

        public bool exemptButtonEnabled = true;

        public bool areaPercentChoiceGroupEnabled = true;

        public bool readFromMapsChecked = false;

        public mimicTextBox hrusSlopeBand = new mimicTextBox();

        public mimicTextBox hrusSlopeBrowser = new mimicTextBox();

        public bool slopeGroupEnabled = true;

        public bool elevBandsButtonEnabled = true;

        public bool generateFullHRUsEnabled = true; 

        public bool generateFullHRUsChecked = true;

        public bool readFromPreviousChecked = false;

        public bool readFromPreviousEnabled = true;

        public bool slopeSliderEnabled = true;

        public bool slopeValEnabled = true;

        public bool areaGroupEnabled = true;

        public bool targetGroupEnabled = true;

        public bool landuseSoilSlopeGroupEnabled = true;

        public bool dominantHRUButtonChecked = false;

        public bool dominantLanduseButtonChecked = false;

        public bool areaButtonChecked = false;

        public mimicTextBox landuseVal = new mimicTextBox();

        public mimicTextBox soilVal = new mimicTextBox();

        public mimicTextBox slopeVal = new mimicTextBox();

        public mimicTextBox targetVal = new mimicTextBox();

        public mimicTextBox areaVal = new mimicTextBox();

        public int reservoirThresholdValue = 101;
        public bool gwflowButtonChecked = false;

        public string _aquiferThickness = "";

        public string _aquiferPermeability = "";

        public string _tileDrains = "";

        public bool isStructured = false;

        public double transitionRate = 0;

        public bool _refineStreams = false;

        public bool _refineTopography = false;

        public bool _refineBoundary = false;

        public bool _refineWells = false;

        public bool _useTileDrains = false;

        public bool alignToGrid = false;

        public int _cellSize = 0;

        public int _maxCellSize = 0;

        public int _levels = 0;

        public int _elevTolerance = 0;

        public HRUsFormStub(HRUs parent) {
            this._parent = parent;
            this._gv = parent._gv;
            this._db = parent._db;
            this._createHRUs = parent.CreateHRUs;
            this._gw = new GwflowHruStub(parent);
        }

        public async Task init() {

            this._parent._db.populateTableNames();
            this.hrusSelectLanduseTable.Items.AddRange(this._parent._db.landuseTableNames.ToArray());
            this.hrusSelectLanduseTable.Items.Add(Parameters._USECSV);
            this.hrusSelectSoilTable.Items.AddRange(this._parent._db.soilTableNames.ToArray());
            this.hrusSelectSoilTable.Items.Add(Parameters._USECSV);
            //await this.readProj();
            this.setLanduseData();
            this.setSoilData();
        }



        // Set plant and urban comboBoxes.
        public virtual void setLanduseData() {
            if (!this._db.plantSoilDatabaseSelected) {
                this.plantSoilDatabase.Text = this._db.dbFile;
                this._db.plantSoilDatabase = this._db.dbFile;
            }
#if TESTING
            this._db.plantTableNames = this._db.collectPlantSoilTableNames("plant", this.hrusSelectPlantTable);
#else
            // will never get here
#endif
            var plantSearch = this._db.plantTable.Contains("plant") ? this._db.plantTable : "plant";
            var plantIndex = this.hrusSelectPlantTable.FindString(plantSearch);
            if (plantIndex >= 0) {
                this.hrusSelectPlantTable.SelectedIndex = plantIndex;
            }
            this._db.plantTable = this.hrusSelectPlantTable.Text;
#if TESTING
            this._db.urbanTableNames = this._db.collectPlantSoilTableNames("urban", this.hrusSelectUrbanTable);
#else
            // will never get here
#endif
            var urbanSearch = this._db.urbanTable.Contains("urban") ? this._db.urbanTable : "urban";
            var urbanIndex = this.hrusSelectUrbanTable.FindString(urbanSearch);
            if (urbanIndex >= 0) {
                this.hrusSelectUrbanTable.SelectedIndex = urbanIndex;
            }
            this._db.urbanTable = this.hrusSelectUrbanTable.Text;
        }

        // Read usersoil/STATSGO/SSURGO choice and set variables.
        public virtual void setSoilData() {
            if (this.usersoilButtonChecked) {
                //this.dbLabelText = "Select landuse and soil database";
                this._db.useSTATSGO = false;
                this._db.useSSURGO = false;
                //this.soilTableLabel.Enabled = true;
                //this.selectSoilTable.Enabled = true;
                //this.selectUsersoilTable.Visible = true;
                //this.selectUsersoilTableLabel.Visible = true;
                if (!this._db.plantSoilDatabaseSelected) {
                    this.plantSoilDatabase.Text = this._db.dbFile;
                    this._db.plantSoilDatabase = this._db.dbFile;
                }
#if TESTING
                this._db.usersoilTableNames = this._db.collectPlantSoilTableNames("usersoil", this.hrusSelectUsersoilTable);
#else
                // will never get here
#endif
                var searchTable = this._db.usersoilTable.Contains("usersoil") ? this._db.usersoilTable : "usersoil";
                var usersoilIndex = this.hrusSelectUsersoilTable.FindString(searchTable);
                if (usersoilIndex >= 0) {
                    this.hrusSelectUsersoilTable.SelectedIndex = usersoilIndex;
                }
                this._db.usersoilTable = this.hrusSelectUsersoilTable.Text;
            } else if (this.STATSGOButtonChecked) {
                //this.dbLabel.Text = "Select landuse database";
                this._db.useSTATSGO = true;
                this._db.useSSURGO = false;
                //this.soilTableLabel.Enabled = true;
                //this.selectSoilTable.Enabled = true;
                //this.selectUsersoilTable.Visible = false;
                //this.selectUsersoilTableLabel.Visible = false;
                this._db.soilDatabase = Utils.join(this._gv.dbPath, Parameters._SOILDB);
                if (!File.Exists(this._db.soilDatabase)) {
                    Utils.information(string.Format("To use STATSGO soils with QSWAT+ you need to download the SWAT+ STATSGO/SSURGO soil database {0} and save it as {1}.", "https://plus.swat.tamu.edu/downloads/swatplus_soils.zip", this._db.soilDatabase), this._gv.isBatch);
                }
            } else if (this.SSURGOButtonChecked) {
                //this.dbLabel.Text = "Select landuse database";
                this._db.useSTATSGO = false;
                this._db.useSSURGO = true;
                //this.soilTableLabel.Enabled = false;
                //this.selectSoilTable.Enabled = false;
                //this.selectUsersoilTable.Visible = false;
                //this.selectUsersoilTableLabel.Visible = false;
                this._db.soilDatabase = Utils.join(this._gv.dbPath, Parameters._SOILDB);
                if (!File.Exists(this._db.soilDatabase)) {
                    Utils.information(string.Format("To use SSURGO soils with QSWAT+ you need to download the SWAT+ STATSGO/SSURGO soil database {0} and save it as {1}.", "https://plus.swat.tamu.edu/downloads/swatplus_soils.zip", this._db.soilDatabase), this._gv.isBatch);
                }
            }
        }
        public void Show() { }

        public void setMainTab(int index) { }

        public void enableTab2(bool enable) { }

        public bool Enabled;

        public void Close() { }

        public string progressLabel {
            get { return ""; }
            set { }
        }

        public void progress(string msg) { }

        public void setChannelMergeChoice() { }

        public void setHRUChoice() { }

        public void setHRUChoiceStub(int hru, bool useArea) {
            if (_createHRUs is null) { _createHRUs = _parent.CreateHRUs; }
            this._createHRUs.isDominantHRU = hru == 2;
            if (hru == 2 || hru == 1) { // dominant HRU or dominant landuse, soil, slope
                this._createHRUs.isMultiple = false;
            } else {
                this._createHRUs.isMultiple = true;
                if (hru == 3) { // filter by landuse, soil, slope
                    //this.stackedWidget.SelectedIndex = 0;
                    this._createHRUs.isArea = false;
                    this._createHRUs.isTarget = false;
                } else if (hru == 4) { // threshold area
                    //this.stackedWidget.SelectedIndex = 1;
                    this._createHRUs.isArea = true;
                    this._createHRUs.isTarget = false;
                } else { // target number of HRUs
                    //this.stackedWidget.SelectedIndex = 2;
                    this._createHRUs.isArea = false;
                    this._createHRUs.isTarget = true;
                }
                this._createHRUs.useArea = useArea;
            }
        }

        public void saveProj() { }

        public void saveProjPart1() { }

        public void setProgressBarVisible(bool visible) { }

        public void _toggleGridType(bool structured) { }

        public void _updateMinSizeLabel() { }
        public void enableCreateButton(bool enable) { }
    }

    public class SplitFormStub {

        public GlobalVars _gv;
        public SplitFormStub(GlobalVars gv) {
            this._gv = gv;
        }

        public void run() { }
    }

    public class ExemptFormStub {
        public GlobalVars _gv;
        public ExemptFormStub(GlobalVars _gv) {
            this._gv = _gv;
        }
        public void run() { }
    }

    public class ElevationBandsFormStub {
        public GlobalVars _gv;
        public ElevationBandsFormStub(GlobalVars _gv) {
            this._gv = _gv;
        }
        public void run() { }
    }

    public class GwflowDBStub {

        public GlobalVars _gv;
        public HRUsFormStub _dlg;
        public GwflowDBStub(GlobalVars _gv, HRUsFormStub _dlg) { 
        this._gv = _gv;
        this._dlg = _dlg;
        }

        public async Task<bool> run() {
            return true;
        }
    }

    public class GwflowHruStub {
        public HRUs _parent;

        public GwflowHruStub(HRUs parent) {
            this._parent = parent;
        }

        public void saveToProject() { }
    }

    public class VisualiseFormStub {
        private Visualise _parent;
        public Timer Ptimer = null;
        public int PtabWidgetIndex = 0;
        public mimicComboBox PscenariosCombo = new mimicComboBox();
        public mimicTextBox PobservedFileEdit = new mimicTextBox();
        public mimicComboBox PoutputCombo = new mimicComboBox();
        public mimicComboBox PstartDay = new mimicComboBox();
        public mimicComboBox PfinishDay = new mimicComboBox();
        public mimicComboBox PstartMonth = new mimicComboBox();
        public mimicComboBox PfinishMonth = new mimicComboBox();
        public DataTable PtableWidgetDataSource = null;
        public DataGridViewRow PtableWidgetCurrentRow = null;
        public DataGridViewRow PtableWidgetCurrentCellOwningRow = null;
        public int PtableWidgetCurrentColumn = 0;
        public int PtableWidgetRowCount = 0;
        public Action<int, int> setDataGridColumnWidth = (int index, int width) => { };
        public Func<bool> hasSelectedDataRow = () => false;
        public DataGridViewCell PtableWidgetCurrentCell = null;
        public DataGridViewCell PtableWidgetRowCell(int row, int cell) { return null; }
        public mimicTextBox PstartYear = new mimicTextBox();
        public mimicTextBox PfinishYear = new mimicTextBox();
        public mimicComboBox PplotType = new mimicComboBox();
        public mimicTextBox PresultsFileEdit = new mimicTextBox();
        public mimicComboBox PvariableCombo = new mimicComboBox();
        public mimicComboBox PvariablePlot = new mimicComboBox();
        public Action PvariableListClear = () => { };
        public int PvariableListCount = 0;
        public int PvariableListSelectedCount = 0;
        public Func<string, int> PvariableListAdd = (string key) => 0; 
        public object PvariableListItem = null;
        public Func<string, int> PvariableListIndexOf = (string key) => 0;
        public Action<int> PvariableListRemoveAt = (int i) => { };
        public object getVariableListItem(int i) { return null; }
        public Func<string, int> PvariableListFindString = (string key) => 0;
        public Action PanimationVariableComboClear = () => { };
        public Func<string, int> PanimationVariableComboAdd = (string key) => 0;
        public int PanimationVariableComboIndex = 0;
        public object PanimationVariableComboItem = null;
        public int PanimationVariableComboCount = 0;
        public mimicComboBox PanimationVariableCombo = new mimicComboBox();
        public bool PsummaryBoxVisible = true; 
        public mimicComboBox PsummaryCombo = new mimicComboBox();
        public string PunitPlotText = "";
        public Func<string, int> PunitPlotFindStringExact = (string key) => 0;
        public int PunitPlotIndex = 0;
        public object getUnitPlotItem(int i) { return null; }
        public Action PunitPlotClear = () => { };
        public Func<string, int> PunitPlotAdd = (string key) => 0;
        public object PunitPlotItem = null;
        public string PunitEditText = "";
        public int PsliderValue = 0;
        public int PsliderMinimum = 0;
        public int PsliderMaximum = 0;
        public System.Drawing.Color PrecordButtonBackColor = System.Drawing.Color.White;
        public string PrecordLabelText = string.Empty;
        public string PcompareLabelText = string.Empty;
        public string PunitLabelText = string.Empty;
        public bool PplayButtonEnabled = false;
        public decimal PspinBoxValue = 0;
        public decimal PprintCountValue = 0;
        public string PdateLabelText = "";
        public Action PdateLabelRefresh = () => { };
        public bool PprintAnimationChecked = false;
        public bool PcurrentResultsMapChecked = false;
        public bool PlandscapeButtonChecked = false;
        public bool PcomposeOptionsVisible = false;
        public decimal PcomposeCountValue = 0;
        public mimicComboBox PQqSubbasin = new mimicComboBox();
        public object PQqResultsDataSource = null;
        public void PQqResultsColumnsWidth(int col, int width) { }
        public decimal PQqSpinValue = 0;
        public mimicComboBox PdQpSubbasin = new mimicComboBox();
        public mimicComboBox PdQpStartMonth = new mimicComboBox();
        public mimicComboBox PQbSubbasin = new mimicComboBox();
        public object PQbResultsDataSource = null;
        public void PQbResultsColumnsWidth(int col, int width) { }
        public string PQbAnnualResultText = string.Empty; 
        public mimicComboBox PQbStartMonth = new mimicComboBox();
        public bool PQqAppendChecked = false;
        public string PdQpResultText = string.Empty;
        public bool PdQpSpinPEnabled = false;
        public decimal PdQpSpinPValue = 0;
        public bool PdQpPercentileChecked = false;
        public decimal PdQpSpinDValue = 0;
        public bool PdQpAppendChecked = false;
        public bool PQbAppendChecked = false;

        public VisualiseFormStub(Visualise _parent) { 
            this._parent = _parent; 
        }
        public void removeAnimationMode() { }
        public void Show() { }
        public void setCompare(bool b) { }
        public void Close() { }
    }

    public class CompareScenariosFormStub {

        private Visualise _parent;
        public Func<object, int> Pscenario1Add = null;
        public object Pscenario1Item = null;
        public Func<object, int> Pscenario2Add = null;
        public object Pscenario2Item = null;
        public CompareScenariosFormStub(Visualise _parent) {
            this._parent = _parent;
        }
        public void ShowDialog() { }
        public void Close() { }
    }
}
#else
#endif