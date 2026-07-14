


using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using ImageMagick;
using Microsoft.Extensions.FileSystemGlobbing;
using OSGeo.OGR;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
//using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.OleDb;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Layer = ArcGIS.Desktop.Mapping.Layer;
using Path = System.IO.Path;

namespace ArcSWATPlus { 
    
    // Support visualisation of SWAT outputs, using data in SWAT output database.
    public class Visualise {
        
        public CompareScenariosForm _comparedlg;
        
        public VisualiseForm _dlg;
        
        public GlobalVars _gv;
        
        public List<double> allAnimateVals;
        
        public string animateFile;
        
        public Dictionary<string, int> animateIndexes;

        public Dictionary<string, string> animateVars;

        public FeatureLayer animateLayer;

        public Map animateMap;

        public Timer animateTimer;
        
        public string animateVar;
        
        public bool animating;

        public object animationDOM;

        public object animationLayout;

        public bool animationPaused;

        public object animationTemplate;

        public bool animationTemplateDirty;
        
        public FeatureLayer aquResultsLayer;
        
        public Dictionary<int, double> areas;
        
        public bool capturing;
        
        public int compositionCount;
        
        public SQLiteConnection conn;
        
        public FeatureLayer currentResultsLayer;
        
        public int currentStillNumber;
        
        public string db;
        
        public FeatureLayer deepAquResultsLayer;
        
        public double dQpResult;
        
        public int finishDay;
        
        public int finishMonth;
        
        public int finishYear;
        
        public bool hasHRUs;
        
        public Dictionary<int, Dictionary<int, int>> hruNums;
        
        public FeatureLayer hruResultsLayer;
        
        public List<string> ignoredTables;
        
        public List<string> ignoredVars;
        
        public bool internalChangeToAquRenderer;
        
        public bool internalChangeToDeepAquRenderer;
        
        public bool internalChangeToHRURenderer;
        
        public bool internalChangeToLSURenderer;
        
        public bool internalChangeToRivRenderer;
        
        public bool internalChangeToSubRenderer;
        
        public int interval;
        
        public bool isAA;
        
        public bool isAnnual;
        
        public bool isDaily;
        
        public bool isMonthly;
        
        public int julianFinishDay;
        
        public int julianStartDay;
        
        public bool keepAquColours;
        
        public bool keepDeepAquColours;
        
        public bool keepHRUColours;
        
        public bool keepLSUColours;
        
        public bool keepRivColours;
        
        public bool keepSubColours;
        
        public string lastdQpResultsFile;
        
        public string lastQbResultsFile;
        
        public string lastQqResultsFile;
        
        public FeatureLayer lsuResultsLayer;
        
        public string mapTitle;
        
        public int numHRUs;
        
        public int numLSUs;
        
        public int numSubbasins;
        
        public string observedFileName;
        
        public int periodDays;
        
        public double periodMonths;
        
        public bool periodsUpToDate;
        
        public double periodYears;
        
        public List<string> plotOnlyTables;
        
        public double QbResult;

        public Dictionary<int, Dictionary<string, double>> staticResultsData;

        public Dictionary<string, Dictionary<int, Dictionary<int, double>>> animateResultsData;

        public string resultsFile;
        
        public bool resultsFileUpToDate;
        
        public FeatureLayer rivResultsLayer;
        
        public string scenario;
        
        public string scenario1;
        
        public string scenario2;
        
        public int startDay;
        
        public int startMonth;
        
        public int startYear;
        
        public Dictionary<string, Dictionary<int, Dictionary<string, Dictionary<int, Dictionary<int, double>>>>> staticData;
        
        public string stillFileBase;
        
        public Dictionary<int, int> subbasinOutletChannels;
        
        public FeatureLayer subResultsLayer;
        
        public bool summaryChanged;
        
        public string table;
        
        public string title;
        
        public string videoFile;
        
        public static string _TOTALS = "Totals";
        
        public static string _DAILYMEANS = "Daily means";
        
        public static string _MONTHLYMEANS = "Monthly means";
        
        public static string _ANNUALMEANS = "Annual means";
        
        public static string _MAXIMA = "Maxima";
        
        public static string _MINIMA = "Minima";
        
        public static string _AREA = "AREAkm2";
        
        public static List<string> _MONTHS = new List<string> {
            "January",
            "February",
            "March",
            "April",
            "May",
            "June",
            "July",
            "August",
            "September",
            "October",
            "November",
            "December"
        };
        
        public static string _NORTHARROW = "apps/qgis/svg/wind_roses/WindRose_01.svg";
        
        public static string _EFLOWTABLE = "channel_sdmorph_day";
        
        public static string _CHOOSEPLOT = "Plot type";
        
        public static string _GRAPHORBAR = "Graph/bar chart";
        
        public static string _FLOWDURATION = "Duration curve";
        
        public static string _SCATTER = "Scatter plot";
        
        public static string _BOX = "Box plot";

        private DataTable plotData;

        private DataTable QqData;

        private DataTable QbData;
        
        public Visualise(GlobalVars gv) {
            this._gv = gv;
            this._dlg = new VisualiseForm(this);
            //this._dlg.setWindowFlags(this._dlg.windowFlags() & ~Qt.WindowContextHelpButtonHint & Qt.WindowMinimizeButtonHint);
            //this._dlg.move(this._gv.visualisePos);
            this._comparedlg = new CompareScenariosForm(this);
            //# variables found in various tables that do not contain values used in results
            // note we also exclude variables that do not have typr REAL
            this.ignoredVars = new List<string> {
                "id",
                "output_type_id",
                "output_interval_id",
                "time",
                "year",
                "unit",
                "plantnm",
                "ob_typ",
                "props",
                "type",
                "obj_type",
                "hyd_typ",
                "resnum",
                "hru",
                "area",
                "vol"
            };
            //# tables not suitable for visualisation
            this.ignoredTables = new List<string> {
                "crop_yld_",
                "deposition_",
                "hydin_",
                "hydout_",
                "ru_",
                "basin_psc_",
                "recall_"
            };
            //# tables only included when plotting
            this.plotOnlyTables = new List<string> {
                "reservoir_",
                "wetland_"
            };
            //# current scenario
            this.scenario = "";
            //# Current output database
            this.db = "";
            //# Current connection
            this.conn = null;
            //# Current table
            this.table = "";
            //# Number of subbasins in current watershed TODO:
            this.numSubbasins = 0;
            //# number of landscape units in current watershed TODO:
            this.numLSUs = 0;
            //# number of HURUs in current watershed TODO:
            this.numHRUs = 0;
            //         ## map SWATBasin -> SWATBasin -> SWATChannel -> LSUId -> hruNum-set
            //         self.topology = None 
            //         ## map SWATBasin -> SWATChannel -> LSUId -> reservoir number
            //         self.reservoirs = None 
            //         ## map SWATBasin -> SWATChannel -> LSUId -> pond number
            //         self.ponds = None
            //# flag to show if there is a HRUs shapefile available
            this.hasHRUs = false;
            //# Data read from db table
            //
            // data takes the form
            // layerId -> gis_id -> variable_name -> year -> time -> value
            this.staticData = new Dictionary<string, Dictionary<int, Dictionary<string, Dictionary<int, Dictionary<int, double>>>>>();
            //# Data to write to shapefile
            //
            // takes the form subbasin number -> variable_name -> value for static use
            // takes the form layerId -> date -> subbasin number -> val for animation
            // where date is YYYY or YYYYMM or YYYYDDD according to period of input
            this.staticResultsData = new Dictionary<int, Dictionary<string, double>>();
            this.animateResultsData = new Dictionary<string, Dictionary<int, Dictionary<int, double>>>();
            //# Areas of subbasins (drainage area for reaches)
            this.areas = new Dictionary<int, double>();
            //# true if output is daily
            this.isDaily = false;
            //# true if output is monthly
            this.isMonthly = false;
            //# true if output is annual
            this.isAnnual = false;
            //# true if output is average annual
            this.isAA = false;
            //# julian start day
            this.julianStartDay = 0;
            //# julian finish day
            this.julianFinishDay = 0;
            //# start year of period (of output: includes any nyskip)
            this.startYear = 0;
            //# start month of period
            this.startMonth = 0;
            //# start day of period
            this.startDay = 0;
            //# finish year of period
            this.finishYear = 0;
            //# finish month of period
            this.finishMonth = 0;
            //# finish day of period
            this.finishDay = 0;
            //# length of simulation in days
            this.periodDays = 0;
            //# length of simulation in months (may be fractional)
            this.periodMonths = 0.0;
            //# length of simulation in years (may be fractional)
            this.periodYears = 0.0;
            //# print interval
            this.interval = 1;
            //# map canvas title
            this.mapTitle = null;
            //# flag to decide if we need to create a new results file:
            // changes to summary method or result variable don't need a new file
            this.resultsFileUpToDate = false;
            //# flag to decide if we need to reread data because period has changed
            this.periodsUpToDate = false;
            //# current streams results layer
            this.rivResultsLayer = null;
            //# current aquifers results layer
            this.aquResultsLayer = null;
            //# current deep aquifers results layer
            this.deepAquResultsLayer = null;
            //# current subbasins results layer
            this.subResultsLayer = null;
            //# current LSUs results layer
            this.lsuResultsLayer = null;
            //# current HRUs results layer
            this.hruResultsLayer = null;
            //# current results layer: equal to one of the riv, sub, lsu or hruResultsLayer
            this.currentResultsLayer = null;
            //# current resultsFile
            this.resultsFile = "";
            //# flag to indicate if summary has changed since last write to results file
            this.summaryChanged = true;
            //# current animation layer
            this.animateLayer = null;
            //# current animation file (a temporary file)
            this.animateFile = "";
            //# map layerId -> index of animation variable in results file
            this.animateIndexes = new Dictionary<string, int>();
            //# map layerId -> name of animation variable in results file
            this.animateVars = new Dictionary<string, string>();
            //# all values involved in animation, for calculating Jenks breaks
            this.allAnimateVals = new List<double>();
            //# timer used to run animation
            this.animateTimer = this._dlg.Ptimer;
            //# flag to indicate if animation running
            this.animating = false;
            //# flag to indicate if animation paused
            this.animationPaused = false;
            //# animation variable
            this.animateVar = "";
            //# flag to indicate if capturing video
            this.capturing = false;
            //# base filename of capture stills
            this.stillFileBase = "";
            //# name of latest video file
            this.videoFile = "";
            //# number of next still frame
            this.currentStillNumber = 0;
            //# flag to indicate if stream renderer being changed by code
            this.internalChangeToRivRenderer = false;
            //# flag to indicate if subbasin renderer being changed by code
            this.internalChangeToSubRenderer = false;
            //# flag to indicate if LSU renderer being changed by code
            this.internalChangeToLSURenderer = false;
            //# flag to indicate if HRU renderer being changed by code
            this.internalChangeToHRURenderer = false;
            //# flag to indicate if aquifer renderer being changed by code
            this.internalChangeToAquRenderer = false;
            //# flag to indicate if deep aquifer renderer being changed by code
            this.internalChangeToDeepAquRenderer = false;
            //# flag to indicate if colours for rendering streams should be inherited from existing results layer
            this.keepRivColours = false;
            //# flag to indicate if colours for rendering subbasins should be inherited from existing results layer
            this.keepSubColours = false;
            //# flag to indicate if colours for rendering LSUs should be inherited from existing results layer
            this.keepLSUColours = false;
            //# flag to indicate if colours for rendering HRUs should be inherited from existing results layer
            this.keepHRUColours = false;
            //# flag to indicate if colours for rendering aquifers should be inherited from existing results layer
            this.keepAquColours = false;
            //# flag to indicate if colours for rendering deep aquifers should be inherited from existing results layer
            this.keepDeepAquColours = false;
            //# map sub -> LSU -> HRU numbers
            this.hruNums = new Dictionary<int, Dictionary<int, int>>();
            //# file with observed data for plotting
            this.observedFileName = "";
            //# project title
            this.title = "";
            //# count to keep layout titles unique
            this.compositionCount = 0;
            //# animation layout
            this.animationLayout = null;
            //# animation template DOM document
            this.animationDOM = null;
            //# animation template file
            this.animationTemplate = "";
            //# flag to show when user has perhaps changed the animation template
            this.animationTemplateDirty = false;
            // layout animation not possible in ArcSWAT+
            this._dlg.removeAnimationMode();
            //# map from subbasin to outlet channel
            this.subbasinOutletChannels = new Dictionary<int, int>();
            //# last file used for Qq results
            this.lastQqResultsFile = "";
            //# last file used for dQp results
            this.lastdQpResultsFile = "";
            //# last file used for Qb results
            this.lastQbResultsFile = "";
            //# dQp result
            this.dQpResult = -1.0;
            //# Qb result
            this.QbResult = -1.0;
            // empty animation and png directories
            this.clearAnimationDir();
            this.clearPngDir();
            //# first scenario to compare
            this.scenario1 = "";
            //# second scenario to compare
            this.scenario2 = "";
        }
        
        // Initialise the visualise form.
        public virtual async Task init() {
            this._dlg.PtabWidget.SelectedIndex = 0;
            this.setSummary();
            this.fillScenarios();
            //this._dlg.scenariosCombo.activated.connect(this.setupDb);
            this._dlg.PscenariosCombo.SelectedIndex = this._dlg.PscenariosCombo.Items.IndexOf("Default");
            if (this.db == "") {
                this.setupDb();
            }
            this.initQResults();
            //this._dlg.variableList.setMouseTracking(true);
            //this._dlg.outputCombo.activated.connect(this.setVariables);
            //this._dlg.variableCombo.activated.connect(this.changeVariableCombo);
            //this._dlg.startYear.setValidator(QIntValidator());
            //this._dlg.finishYear.setValidator(QIntValidator());
            //this._dlg.unitEdit.setValidator(QIntValidator());
            //this._dlg.PsummaryCombo.activated.connect(this.changeSummary);
            //this._dlg.addButton.clicked.connect(this.addClick);
            //this._dlg.allButton.clicked.connect(this.allClick);
            //this._dlg.delButton.clicked.connect(this.delClick);
            //this._dlg.clearButton.clicked.connect(this.clearClick);
            //this._dlg.resultsFileButton.clicked.connect(this.setResultsFile);
            //this._dlg.tabWidget.currentChanged.connect(this.modeChange);
            //this._dlg.saveButton.clicked.connect(this.makeResults);
            //this._dlg.printButton.clicked.connect(this.printResults);
            //this._dlg.canvasAnimation.clicked.connect(this.changeAnimationMode);
            //this._dlg.printAnimation.clicked.connect(this.changeAnimationMode);
            this.changeAnimationMode();
            //this._dlg.animationVariableCombo.activated.connect(this.setupAnimateLayer);
            //this._dlg.slider.valueChanged.connect(this.changeAnimate);
            //this._dlg.slider.sliderPressed.connect(this.pressSlider);
            //this._dlg.playCommand.clicked.connect(this.doPlay);
            //this._dlg.pauseCommand.clicked.connect(this.doPause);
            //this._dlg.rewindCommand.clicked.connect(this.doRewind);
            //this._dlg.recordButton.clicked.connect(this.record);
            //this._dlg.recordButton.setStyleSheet("background-color: green; border: none;");
            //this._dlg.playButton.clicked.connect(this.playRecording);
            //this._dlg.spinBox.valueChanged.connect(this.changeSpeed);
            //this.animateTimer.timeout.connect(this.doStep);
            this.setupPlot();
            //this._dlg.unitPlot.activated.connect(this.plotSetUnit);
            //this._dlg.unitPlot.highlighted.connect(this.plotSelectUnit);
            //this._dlg.unitEdit.textEdited.connect(this.plotEditUnit);
            //this._dlg.unitEdit.returnPressed.connect(this.plotEditUnit);
            //this._dlg.variablePlot.activated.connect(this.plotSetVar);
            //this._dlg.addPlot.clicked.connect(this.doAddPlot);
            //this._dlg.deletePlot.clicked.connect(this.doDelPlot);
            //this._dlg.copyPlot.clicked.connect(this.doCopyPlot);
            //this._dlg.upPlot.clicked.connect(this.doUpPlot);
            //this._dlg.downPlot.clicked.connect(this.doDownPlot);
            //this._dlg.observedFileButton.clicked.connect(this.setObservedFile);
            //this._dlg.addObserved.clicked.connect(this.addObervedPlot);
            //this._dlg.plotButton.clicked.connect(this.writePlotData);
            //this._dlg.QqSubbasin.activated.connect(this.setQqTableHead);
            //this._dlg.QqSpin.valueChanged.connect(this.setQqTableHead);
            //this._dlg.QqCalculate.clicked.connect(this.calculateQq);
            //this._dlg.QqSave.clicked.connect(this.saveQq);
            //this._dlg.dQpSubbasin.activated.connect(this.cleardQpResult);
            //this._dlg.dQpStartMonth.activated.connect(this.cleardQpResult);
            //this._dlg.dQpSpinD.valueChanged.connect(this.cleardQpResult);
            //this._dlg.dQpSpinP.valueChanged.connect(this.cleardQpResult);
            //this._dlg.dQpPercentile.toggled.connect(this.dQpButtons);
            //this._dlg.dQpMean.toggled.connect(this.dQpButtons);
            //this._dlg.dQpCalculate.clicked.connect(this.calculatedQp);
            //this._dlg.dQpSave.clicked.connect(this.savedQp);
            //this._dlg.QbSubbasin.activated.connect(this.setQbTableHead);
            //this._dlg.QbStartMonth.activated.connect(this.setQbTableHead);
            //this._dlg.QbCalculate.clicked.connect(this.calculateQb);
            //this._dlg.QbSave.clicked.connect(this.saveQb);
            //this._dlg.closeButton.clicked.connect(this.doClose);
            //this._dlg.destroyed.connect(this.doClose);
            //this._dlg.compareButton.clicked.connect(this.startCompareScenarios);
            //this._comparedlg.okButton.clicked.connect(this.setupCompareScenarios);
            //this._comparedlg.cancelButton.clicked.connect(this.closeCompareScenarios);
            var proj = _gv.proj;
            await this.setBackgroundLayers();
            //var leftShortCut = QShortcut(QKeySequence(Qt.Key_Left), this._dlg);
            //var rightShortCut = QShortcut(QKeySequence(Qt.Key_Right), this._dlg);
            //leftShortCut.activated.connect(this.animateStepLeft);
            //rightShortCut.activated.connect(this.animateStepRight);
            this.title = proj.Name;
            (string observedFile, bool found) = proj.readEntry(this.title, "observed/observedFile", "");
            if (found && File.Exists(observedFile)) {
                this.observedFileName = observedFile;
                this._dlg.PobservedFileEdit.Text = observedFileName;
            }
            //var animationGroup = Utils.getGroupLayerByName(Utils._ANIMATION_GROUP_NAME);
            //Debug.Assert(animationGroup is not null);
            //animationGroup.PropertyChanged += new PropertyChangedEventHandler(changeAnimation);
            //animationGroup.visibilityChanged.connect(this.setAnimateLayer);
            //animationGroup.removedChildren.connect(this.setAnimateLayer);
            //animationGroup.addedChildren.connect(this.setAnimateLayer);
            //var resultsGroup = Utils.getGroupLayerByName(Utils._RESULTS_GROUP_NAME);
            //Debug.Assert(resultsGroup is not null);
            // make sure results group is expanded and visible
            //await QueuedTask.Run(() => {
            //    resultsGroup.SetExpanded(true);
            //    resultsGroup.SetVisibility(true);
            //});
            //resultsGroup.PropertyChanged += new PropertyChangedEventHandler(setResults);
            //resultsGroup.visibilityChanged.connect(this.setResultsLayer);
            //resultsGroup.removedChildren.connect(this.setResultsLayer);
            //resultsGroup.addedChildren.connect(this.setResultsLayer);
            // in case restart with existing animation layers
            //await this.setAnimateLayer(); // TODO
            // in case restart with existing results layers
            //await this.setResultsLayer(); // TODO
        }
        
        // Do visualisation.
        public virtual async Task run() {
            Cursor.Current = Cursors.WaitCursor;
            await this.init();
            Cursor.Current = Cursors.Default;
            this._dlg.Show();
            //this._gv.visualisePos = this._dlg.pos();
        }

        // Put scenarios in PscenariosCombo and months in start and finish month combos.
        public virtual void fillScenarios() {
            foreach (var direc in Directory.GetDirectories(this._gv.scenariosDir)) {
                var db = Utils.join(Utils.join(direc, Parameters._RESULTS), Parameters._OUTPUTDB);
                if (this._gv.forTNC) {
                    db = db.Replace(".mdb", ".sqlite");
                }
                if (File.Exists(db)) {
                    this._dlg.PscenariosCombo.Items.Add(Path.GetFileNameWithoutExtension(direc));
                }
            }
            if (this._dlg.PscenariosCombo.Items.Count > 1) {
                foreach (var i in Enumerable.Range(0, this._dlg.PscenariosCombo.Items.Count)) {
                    this._comparedlg.Pscenario1.Items.Add(this._dlg.PscenariosCombo.Items[i]);
                    this._comparedlg.Pscenario2.Items.Add(this._dlg.PscenariosCombo.Items[i]);
                }
                this._dlg.setCompare(true);
            } else {
                this._dlg.setCompare(false);
            }
            foreach (var month in Visualise._MONTHS) {
                var m = Utils.trans(month);
                this._dlg.PstartMonth.Items.Add(m);
                this._dlg.PfinishMonth.Items.Add(m);
            }
            foreach (var i in Enumerable.Range(0, 31)) {
                this._dlg.PstartDay.Items.Add((i + 1).ToString());
                this._dlg.PfinishDay.Items.Add((i + 1).ToString());
            }
        }
        
        // Reduce visible layers to channels, LSUs, HRUs, aquifers and subbasins by making all others not visible,
        //         loading LSUs, HRUs, aquifers if necessary.
        //         Leave Results group in case we already have some layers there.
        public virtual async Task setBackgroundLayers() {
            Func<string, bool> keepVisible;
            var slopeGroup = Utils.getGroupLayerByName(Utils._SLOPE_GROUP_NAME);
            if (slopeGroup is not null) {
                foreach (var layer in slopeGroup.GetLayersAsFlattenedList()) {
                    if (layer is not null) {
                        await QueuedTask.Run(() => {
                            layer.SetVisibility(false);
                        });
                    }
                }
            }
            var soilGroup = Utils.getGroupLayerByName(Utils._SOIL_GROUP_NAME);
            if (soilGroup is not null) {
                foreach (var layer in soilGroup.GetLayersAsFlattenedList()) {
                    if (layer is not null) {
                        await QueuedTask.Run(() => {
                            layer.SetVisibility(false);
                        });
                    }
                }
            }
            var landuseGroup = Utils.getGroupLayerByName(Utils._LANDUSE_GROUP_NAME);
            if (landuseGroup is not null) {
                foreach (var layer in landuseGroup.GetLayersAsFlattenedList()) {
                    if (layer is not null) {
                        await QueuedTask.Run(() => {
                            layer.SetVisibility(false);
                        });
                    }
                }
            }
            // laod HRUS, LSUS and Aquifers layers if necessary
            var hrusLayer = Utils.getLayerByLegend(Utils._FULLHRUSLEGEND);
            var hrusFile = Utils.join(this._gv.resultsDir, Parameters._HRUS + ".shp");
            var hasHRUs = File.Exists(hrusFile);
            var lsusLayer = Utils.getLayerByLegend(Utils._FULLLSUSLEGEND);
            var aquifersLayer = Utils.getLayerByLegend(Utils._AQUIFERSLEGEND);
            if (hrusLayer is null && hasHRUs || lsusLayer is null || aquifersLayer is null) {
                // set sublayer as DEM
                //var hillshadeLayer = Utils.getLayerByLegend(Utils._HILLSHADELEGEND);
                var demLayer = Utils.getLayerByLegend(Utils._DEMLEGEND);
                Layer subLayer = demLayer;
                //if (hillshadeLayer is not null) {
                //    subLayer = hillshadeLayer;
                //} else if (demLayer is not null) {
                //    subLayer = demLayer;
                //}
                if (hrusLayer is null && hasHRUs) {
                    hrusLayer = (await Utils.getLayerByFilename(hrusFile, FileTypes._HRUS, this._gv, subLayer, Utils._WATERSHED_GROUP_NAME)).Item1;
                }
                if (lsusLayer is null) {
                    var lsusFile = Utils.join(this._gv.resultsDir, Parameters._LSUS + ".shp");
                    lsusLayer = (await Utils.getLayerByFilename(lsusFile, FileTypes._LSUS, this._gv, subLayer, Utils._WATERSHED_GROUP_NAME)).Item1;
                }
                if (aquifersLayer is null) {
                    var aquifersFile = Utils.join(this._gv.resultsDir, Parameters._AQUIFERS + ".shp");
                    aquifersLayer = (await Utils.getLayerByFilename(aquifersFile, FileTypes._AQUIFERS, this._gv, subLayer, Utils._WATERSHED_GROUP_NAME)).Item1;
                }
            }
            var watershedLayers = Utils.getLayersInGroup(Utils._WATERSHED_GROUP_NAME);
            // make subbasins, channels, LSUs, HRUs and aquifers visible
            if (this._gv.useGridModel) {
                keepVisible = n => n.StartsWith(Utils._GRIDSTREAMSLEGEND) || n.StartsWith(Utils._DRAINSTREAMSLEGEND) || n.StartsWith(Utils._GRIDLEGEND) || n.StartsWith(Utils._AQUIFERSLEGEND) || n.StartsWith(Utils._LSUSLEGEND) || n.StartsWith(Utils._HRUSLEGEND);
            } else {
                keepVisible = n => n.StartsWith(Utils._SUBBASINSLEGEND) || n.StartsWith(Utils._CHANNELREACHESLEGEND) || n.StartsWith(Utils._ACTLSUSLEGEND) || n.StartsWith(Utils._ACTHRUSLEGEND) || n.StartsWith(Utils._AQUIFERSLEGEND);
            }
            await QueuedTask.Run(() => {
                foreach (var layer in watershedLayers) {
                    if (layer is not null) {
                        if (keepVisible(layer.Name)) {
                            layer.SetVisibility(true);
                        } else {
                            layer.SetVisibility(false);
                            layer.SetExpanded(false);
                        }
                    }
                }
            });
        }
        
        // Set current database and connection to it; put table names in outputCombo.
        public virtual void setupDb() {
            this.resultsFileUpToDate = false;
            var scen = this._dlg.PscenariosCombo.SelectedItem;
            if (scen is not null) {
                this.scenario = scen.ToString();
            } else {
                this.scenario = this._dlg.PscenariosCombo.Text;
            }
            this.setConnection(this.scenario);
            var scenDir = Utils.join(this._gv.scenariosDir, this.scenario);
            var txtInOutDir = Utils.join(scenDir, Parameters._TXTINOUT);
            var resultsDir = Utils.join(scenDir, Parameters._RESULTS);
            this.db = Utils.join(resultsDir, Parameters._OUTPUTDB);
            this.hasHRUs = File.Exists(Utils.join(resultsDir, Parameters._HRUS + ".shp"));
            var simFile = Utils.join(txtInOutDir, Parameters._SIM);
            if (!File.Exists(simFile)) {
                Utils.error($"Cannot find simulation file {simFile}", this._gv.isBatch);
                return;
            }
            var prtFile = Utils.join(txtInOutDir, Parameters._PRT);
            if (!File.Exists(prtFile)) {
                Utils.error($"Cannot find print file {prtFile}", this._gv.isBatch);
                return;
            }
            this.readPrt(prtFile, simFile, this.scenario);
            //         self.topology, self.reservoirs, self.ponds = self._gv.db.createTopology()
            this.populateOutputTables();
        }
        
        // Add daily, monthly and annual output tables to output combo.
        //         For post processing find subbasin outlet channels.
        public virtual void populateOutputTables() {
            this._dlg.PoutputCombo.Items.Clear();
            //this._dlg.PoutputCombo.Items.Add("");
            var tables = new List<string>();
            // if we are plotting and have at least one non-observed row, only include tables of same frequency
            var isPlot = this._dlg.PtabWidget.SelectedIndex == 2;
            var plotTable = isPlot ? this.getPlotTable() : "";
            var keepDaily = plotTable == "" || Visualise.tableIsDaily(plotTable);
            if (keepDaily) {
                this.addTablesByTerminator(ref tables, "_day");
            }
            var keepMonthly = plotTable == "" || Visualise.tableIsMonthly(plotTable);
            if (keepMonthly) {
                this.addTablesByTerminator(ref tables, "_mon");
            }
            var keepYearly = plotTable == "" || Visualise.tableIsYearly(plotTable);
            if (keepYearly) {
                this.addTablesByTerminator(ref tables, "_yr");
                if (this._dlg.PtabWidget.SelectedIndex == 0) {
                    // static
                    this.addTablesByTerminator(ref tables, "_aa");
                }
            }
            foreach (var table in tables) {
                this._dlg.PoutputCombo.Items.Add(table);
            }
            this.makeDeepAquiferTables();
            this._dlg.PoutputCombo.SelectedIndex = -1;
            this.table = "";
            this.plotSetUnit();
            this._dlg.PvariablePlot.Items.Clear();
            //this._dlg.PvariablePlot.Items.Add("");
            //this._dlg.PvariablePlot.Text = "";
            this.updateCurrentPlotRow(0);
            //if (this._dlg.PtabWidget.SelectedIndex == 3) {
                this.setSubbasinOutletChannels();
            //}
        }

        // For every table aquifer_... in output tables, if there is no corresponding deep_aquifer
        //         table, create it by removing deepe aquifer entries from first table.
        public virtual void makeDeepAquiferTables() {
            var count = this._dlg.PoutputCombo.Items.Count;
            // ignore first row since it is empty
            var row = 1;
            while (row < count) {
                var txt = this._dlg.PoutputCombo.Items[row].ToString();
                if (txt.StartsWith("aquifer_")) {
                    var deeptxt = "deep_" + txt;
                    var deepIndx = this._dlg.PoutputCombo.Items.IndexOf(deeptxt);
                    if (deepIndx < 0) {
                        var sql1 = string.Format("CREATE TABLE {0} AS SELECT * FROM {1} WHERE name LIKE 'aqu_deep%'", deeptxt, txt);
                        this.execNonQuery(sql1);
                        var sql2 = string.Format("DELETE FROM {0} WHERE name LIKE 'squ_deep%'", txt);
                        this.execNonQuery(sql2);
                        row += 1;
                        this._dlg.PoutputCombo.Items.Insert(row, deeptxt);
                        count += 1;
                    }
                }
                row += 1;
            }
        }
        
        // Set connection to scenario output database.
        public virtual void setConnection(string scenario) {
            var scenDir = Utils.join(this._gv.scenariosDir, scenario);
            var outDir = Utils.join(scenDir, Parameters._RESULTS);
            this.db = Utils.join(outDir, Parameters._OUTPUTDB);
            this.conn = new SQLiteConnection(string.Format("DataSource={0}", this.db));
            if (this.conn is null) {
                Utils.error(string.Format("Failed to connect to output database {0}", this.db), this._gv.isBatch);
            } else {
                this.conn.Open();
            }
        }
        
        // Add to tables table names terminating with terminator, provided they have data.
        //         
        //         The names added are sorted.
        public virtual void addTablesByTerminator(ref List<string> tables, string terminator) {

            bool hasColumn(string table, string columnName) {
                var sql = DBUtils.sqlSelect(table, "*", "", "");
                using (var reader = this.getReader(sql)) {
                    try {
                        var index = reader.GetOrdinal(columnName);
                        return true;
                    } catch (Exception) {
                        return false;
                    }
                }
            }

            var sql = "SELECT name FROM sqlite_master WHERE TYPE='table'";
            var tempTables = new List<string>();
            using (var reader = this.getReader(sql)) {
                while (reader.Read()) {
                    var table = reader.GetString(0);
                    //Utils.loginfo("Considering " + table + " with terminator " + terminator);
                    if (this._dlg.PtabWidget.SelectedIndex != 2 && table.StartsWith("basin_")) {
                        //Utils.loginfo("continue 1");
                        continue;
                    }
                    if (!this.hasHRUs && this._dlg.PtabWidget.SelectedIndex != 2 && table.StartsWith("hru_")) {
                        //Utils.loginfo("continue 2");
                        continue;
                    }
                    if (table.EndsWith(terminator)) {
                        var ignore = false;
                        // check table is not ignored
                        foreach (var ignored in this.ignoredTables) {
                            if (table.StartsWith(ignored)) {
                                ignore = true;
                                //Utils.loginfo("ignored as " + ignored);
                                break;
                            }
                        }
                        if (ignore) {
                            continue;
                        }
                        foreach (var plotOnly in this.plotOnlyTables) {
                            if (table.StartsWith(plotOnly)) {
                                ignore = this._dlg.PtabWidget.SelectedIndex != 2;
                                //Utils.loginfo("ignored for plot");
                                break;
                            }
                        }
                        if (ignore) {
                            continue;
                        }
                        // check table has data and a gis_id
                        if (hasColumn(table, "gis_id")) {
                            //Utils.loginfo($"adding {table} to temp");
                            tempTables.Add(table);
                        }
                    }
                }
            }
            //Utils.loginfo($"temp has {tempTables.Count} entries");
            tables = tables.Concat(tempTables.OrderBy(_p_1 => _p_1).ToList()).ToList();
            //Utils.loginfo($"tables has {tables.Count} entries");
        }
        
        // RestrictOutputTables combo to those ending with terminator.
        public virtual void restrictOutputTablesByTerminator(string terminator) {
            var toDelete = new List<int>();
            foreach (var indx in Enumerable.Range(0, this._dlg.PoutputCombo.Items.Count)) {
                var txt = this._dlg.PoutputCombo.Items[indx].ToString();
                if (txt == "" || txt.EndsWith(terminator)) {
                    continue;
                }
                toDelete.Add(indx);
            }
            // remove from bottom as indexes affected by removal
            toDelete.Reverse();
            foreach (var indx in toDelete) {
                this._dlg.PoutputCombo.Items.RemoveAt(indx);
            }
        }
        
        // Initialise the plot table.
        public virtual void setupPlot() {
            this.plotData = new DataTable();
            this.plotData.Columns.Add("Scenario", typeof(string));
            this.plotData.Columns.Add("Table", typeof(string));
            this.plotData.Columns.Add("Unit", typeof(string));
            this.plotData.Columns.Add("Variable", typeof(string));
            this._dlg.PtableWidget.DataSource = this.plotData;
            this._dlg.PtableWidget.Columns[0].Width = 100;
            this._dlg.PtableWidget.Columns[1].Width = 100;
            this._dlg.PtableWidget.Columns[2].Width = 45;
            this._dlg.PtableWidget.Columns[3].Width = 90;
            this._dlg.PplotType.Items.Clear();
            //this._dlg.PplotType.Items.Add(Visualise._CHOOSEPLOT);
            this._dlg.PplotType.Items.Add(Visualise._GRAPHORBAR);
            this._dlg.PplotType.Items.Add(Visualise._FLOWDURATION);
            this._dlg.PplotType.Items.Add(Visualise._SCATTER);
            this._dlg.PplotType.Items.Add(Visualise._BOX);
        }
        
        // Fill variables combos from selected table; set default results file name.
        public virtual void setVariables() {
            var table = this._dlg.PoutputCombo.SelectedItem.ToString();
            if (this.table == table) {
                // no change: do nothing
                return;
            }
            this.table = table;
            if (this.table == "") {
                return;
            }
            if (this.conn is null) {
                return;
            }
            (this.isDaily, this.isMonthly, this.isAnnual, this.isAA) = Visualise.tableIsDailyMonthlyOrAnnual(this.table);
            var scenDir = Utils.join(this._gv.scenariosDir, this.scenario);
            var outDir = Utils.join(scenDir, Parameters._RESULTS);
            var outFile = Utils.join(outDir, this.table + "results.shp");
            this._dlg.PresultsFileEdit.Text = outFile;
            this.resultsFileUpToDate = false;
            this._dlg.PsummaryBox.Visible = !this.isAA;
            // add gis id numbers to unitPlot combo
            this.setGisIdPlot();
            this._dlg.PvariableCombo.Items.Clear();
            this._dlg.PanimationVariableCombo.Items.Clear();
            //this._dlg.PanimationVariableCombo.Items.Add("");
            this._dlg.PvariablePlot.Items.Clear();
            //this._dlg.PvariablePlot.Items.Add("");
            //this._dlg.PvariablePlot.Text = "";
            this._dlg.PvariableList.Items.Clear();
            var sql = $"PRAGMA TABLE_INFO({table})";
            using (SQLiteDataReader reader = this.getReader(sql)) {
                if (reader.HasRows) {
                    while (reader.Read()) {
                        var vari = reader.GetString(1);
                        var varType = reader.GetString(2);
                        // only include fields of type REAL
                        if (varType == "REAL" && !this.ignoredVars.Contains(vari)) {
                            this._dlg.PvariableCombo.Items.Add(vari);
                            this._dlg.PanimationVariableCombo.Items.Add(vari);
                            this._dlg.PvariablePlot.Items.Add(vari);
                        }
                    }
                }
            }
            //this.setVarComboTips(this._dlg.PvariableCombo, "Select variable to be added to list");
            //this.setVarComboTips(this._dlg.PanimationVariableCombo, "Select variable for animation");
            //this.setVarComboTips(this._dlg.PvariablePlot, "Select the current plot\'s variable");
            this.updateCurrentPlotRow(1);
        }
        
        // Return true if plot tab open and plot table has a selected row.
        public virtual bool plotting() {
            if (this._dlg.PtabWidget.SelectedIndex != 2) {
                return false;
            }
            var row = this._dlg.PtableWidget.CurrentRow;
            return row is not null;
        }

        // Fill summary combo.
        public virtual void setSummary() {
            this._dlg.PsummaryCombo.Items.Clear();
            this._dlg.PsummaryCombo.Items.Add(Visualise._TOTALS);
            this._dlg.PsummaryCombo.Items.Add(Visualise._DAILYMEANS);
            this._dlg.PsummaryCombo.Items.Add(Visualise._MONTHLYMEANS);
            this._dlg.PsummaryCombo.Items.Add(Visualise._ANNUALMEANS);
            this._dlg.PsummaryCombo.Items.Add(Visualise._MAXIMA);
            this._dlg.PsummaryCombo.Items.Add(Visualise._MINIMA);
        }

        // Read time.sim file.  This just sets outer limits for start and end dates.  Return true if no errors.
        public virtual bool readSim(string simFile) {
            try {
                using (var sim = new StreamReader(simFile)) {
                    // skip first line
                    sim.ReadLine();
                    // skip headings
                    sim.ReadLine();
                    var dates = sim.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    this.julianStartDay = Convert.ToInt32(dates[0]);
                    // interpret 0 as 1
                    if (this.julianStartDay == 0) {
                        this.julianStartDay = 1;
                    }
                    this.startYear = Convert.ToInt32(dates[1]);
                    this.julianFinishDay = Convert.ToInt32(dates[2]);
                    this.finishYear = Convert.ToInt32(dates[3]);
                    // interpret Julian 0 as last day of year
                    if (this.julianFinishDay == 0) {
                        this.julianFinishDay = Visualise.isLeap(this.finishYear) ? 366 : 365;
                    }
                    // STEP can be ignored - does not affect outputs
                    return true;
                }
            } catch (Exception) {
                Utils.exceptionError($"Failed to read {simFile}", this._gv.isBatch);
                return false;
            }
        }
        
        // Read print.prt file to get print period and print interval.  Return true if no errors.
        public virtual bool readPrt(string prtFile, string simFile, string scenario) {
            // first read time.sim to reset start/finish dates to simulation
            if (!this.readSim(simFile)) {
                return false;
            }
            try {
                using (var prt = new StreamReader(prtFile)) {
                    // skip first line
                    prt.ReadLine();
                    // skip headings
                    prt.ReadLine();
                    var dates = prt.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    var nyskip = Convert.ToInt32(dates[0]);
                    this.startYear += nyskip;
                    startDay = Convert.ToInt32(dates[1]);
                    if (startDay > 0 && startDay > this.julianStartDay) {
                        this.julianStartDay = startDay;
                    }
                    finishDay = Convert.ToInt32(dates[3]);
                    if (finishDay > 0 && finishDay < this.julianFinishDay) {
                        this.julianFinishDay = finishDay;
                    }
                    var startYear0 = Convert.ToInt32(dates[2]);
                    if (startYear0 > 0 && startYear0 > this.startYear) {
                        this.startYear = startYear0;
                    }
                    var finishYear0 = Convert.ToInt32(dates[4]);
                    if (finishYear0 > 0 && finishYear0 < this.finishYear) {
                        this.finishYear = finishYear0;
                    }
                    // make sure finish day matches the year if set to year end
                    if (this.julianFinishDay == 365 || this.julianFinishDay == 366) {
                        this.julianFinishDay = Visualise.isLeap(this.finishYear) ? 366 : 365;
                    }
                    this.interval = Convert.ToInt32(dates[5]);
                    if (this.interval == 0) {
                        this.interval = 1;
                    }
                }
                this.setDates(scenario);
                return true;
            } catch (Exception ex) {
                Utils.exceptionError(string.Format("Failed to read {0}: {1}", prtFile, ex.Message), this._gv.isBatch);
                return false;
            }
        }
        
        // Set requested start and finish dates to smaller period of length of scenario and requested dates (if any).
        public virtual void setDates(string scenario) {
            var startDate = this.julianToDate(this.julianStartDay, this.startYear);
            var finishDate = this.julianToDate(this.julianFinishDay, this.finishYear);
            var requestedStartDate = this.readStartDate();
            if (requestedStartDate is null) {
                this.setStartDate(startDate);
            } else {
                DateOnly rsd = (DateOnly)requestedStartDate;
                if (rsd < startDate) {
                    Utils.information(string.Format("Scenario {0} period starts later than current scenario {1} period: changing start", scenario, this.scenario), this._gv.isBatch);
                    this.setStartDate(startDate);
                }
            }
            var requestedFinishDate = this.readFinishDate();
            if (requestedFinishDate is null) {
                this.setFinishDate(finishDate);
            } else {
                DateOnly rfd = (DateOnly)requestedFinishDate;
                if (rfd > finishDate) {
                    Utils.information(string.Format("Scenario {0} period finishes earlier than current scenario {1} period: changing finish", scenario, this.scenario), this._gv.isBatch);
                    this.setFinishDate(finishDate);
                }
            }
        }

        // Define period of current scenario in days, months and years.  Return true if OK.
        public virtual bool setPeriods() {
            var requestedStartDate = this.readStartDate();
            var requestedFinishDate = this.readFinishDate();
            if (requestedStartDate is null || requestedFinishDate is null) {
                Utils.error("Cannot read chosen period", this._gv.isBatch);
                return false;
            }
            DateOnly rsd = (DateOnly)requestedStartDate;
            DateOnly rfd = (DateOnly)requestedFinishDate;
            if (requestedFinishDate <= requestedStartDate) {
                Utils.error("Finish date must be later than start date", this._gv.isBatch);
                return false;
            }
            this.periodsUpToDate = this.startDay == rsd.Day && this.startMonth == rsd.Month && this.startYear == rsd.Year && this.finishDay == rfd.Day && this.finishMonth == rfd.Month && this.finishYear == rfd.Year;
            if (this.periodsUpToDate) {
                return true;
            }
            this.startDay = rsd.Day;
            this.startMonth = rsd.Month;
            this.startYear = rsd.Year;
            this.finishDay = rfd.Day;
            this.finishMonth = rfd.Month;
            this.finishYear = rfd.Year;
            this.julianStartDay = rsd.DayOfYear;
            this.julianFinishDay = rfd.DayOfYear;
            this.periodDays = 0;
            this.periodMonths = 0.0;
            this.periodYears = 0.0;
            foreach (var year in Enumerable.Range(this.startYear, this.finishYear + 1 - this.startYear)) {
                var leapAdjust = Visualise.isLeap(year) ? 1 : 0;
                var yearDays = 365 + leapAdjust;
                var start = year == this.startYear ? this.julianStartDay : 1;
                var finish = year == this.finishYear ? this.julianFinishDay : yearDays;
                var numDays = finish - start + 1;
                this.periodDays += numDays;
                var fracYear = (double)numDays / yearDays;
                this.periodYears += fracYear;
                this.periodMonths += fracYear * 12;
            }
            // Utils.loginfo('Period is {0} days, {1} months, {2} years', self.periodDays, self.periodMonths, self.periodYears))
            return true;
        }
    
        
        // Return date from start date from form.  None if any problems.
        public virtual DateOnly? readStartDate() {
            try {
                var day = Convert.ToInt32(this._dlg.PstartDay.SelectedItem);
                var month = this._dlg.PstartMonth.SelectedIndex + 1;
                var year = Convert.ToInt32(this._dlg.PstartYear.Text);
                return new DateOnly(year, month, day);
            } catch (Exception) {
                return null;
            }
        }

        // Return date from finish date from form.  None if any problems.
        public virtual DateOnly? readFinishDate() {
            try {
                var day = Convert.ToInt32(this._dlg.PfinishDay.SelectedItem);
                var month = this._dlg.PfinishMonth.SelectedIndex + 1;
                var year = Convert.ToInt32(this._dlg.PfinishYear.Text);
                return new DateOnly(year, month, day);
            }
            catch (Exception) {
                return null;
            }
        }

        // Set start date on form.
        public virtual void setStartDate(DateOnly date) {
            this._dlg.PstartDay.SelectedIndex = date.Day - 1;
            this._dlg.PstartYear.Text = date.Year.ToString();
            this._dlg.PstartMonth.SelectedIndex = date.Month - 1;
        }

        // Set finish date on form.
        public virtual void setFinishDate(DateOnly date) {
            this._dlg.PfinishDay.SelectedIndex = date.Day - 1;
            this._dlg.PfinishYear.Text = date.Year.ToString();
            this._dlg.PfinishMonth.SelectedIndex = date.Month - 1;
        }

        // Append item to variableList.
        public virtual void addClick() {
            this.resultsFileUpToDate = false;
            var item = this._dlg.PvariableCombo.SelectedIndex;
            if (item != -1) {
                var @var = this._dlg.PvariableCombo.SelectedItem.ToString();
                this._dlg.PvariableList.Items.Add(@var);
            }
        }
        
        // Clear variableList and insert all items from variableCombo.
        public virtual void allClick() {
            this.resultsFileUpToDate = false;
            this._dlg.PvariableList.Items.Clear();
            foreach (var i in Enumerable.Range(0, this._dlg.PvariableCombo.Items.Count)) {
                var item = this._dlg.PvariableCombo.Items[i].ToString();
                this._dlg.PvariableList.Items.Add(item);
            }
        }
        
        // Delete item from variableList.
        public virtual void delClick() {
            this.resultsFileUpToDate = false;
            var item = this._dlg.PvariableList.SelectedItem;
            if (item is not null) {
                var index = this._dlg.PvariableList.Items.IndexOf(item.ToString());
                if (index >= 0) {
                    this._dlg.PvariableList.Items.RemoveAt(index);
                }
            }
        }

        // Clear variableList.
        public virtual void clearClick() {
            this.resultsFileUpToDate = false;
            this._dlg.PvariableList.Items.Clear();
        }
        
        // Return tool tip based on units and description from looking up current table and var in table column_description.
        public virtual string getVarTip(string vari) {
            var sql = "SELECT [units], [description] FROM column_description WHERE table_name=? AND column_name=?";
            try {
                using (SQLiteDataReader reader = this.getReader(sql, new object[] { this.table, vari })) {
                    if (reader.HasRows) {
                        reader.Read();
                        string unit = reader.GetString(0);
                        string descr = reader.GetString(1);
                        var str1 = string.IsNullOrEmpty(unit) || unit == "---" ? "" : $"Units: {unit}";
                        var str2 = string.IsNullOrEmpty(descr) ? "" : $"Description: {descr}";
                        return str1 == "" ? str2 : str2 == "" ? str1 : $"{str1} {str2}";
                    }
                }
            } catch (Exception) {
                return "";
            }
            return "";
        }
        
        // Return tool tip based on description from looking up table in table table_description.
        public virtual string getTableTip(string table) {
            var sql = "SELECT [description] FROM table_description WHERE table_name=?";
            try {
                using (SQLiteDataReader reader = this.getReader(sql, new object[] { table })) {
                    if (reader.HasRows) {
                        reader.Read();
                        var tip = reader.GetString(0);
                        return string.IsNullOrEmpty(tip) ? "" : tip;
                    }
                }
            } catch (Exception) {
                return "";
            }
            return "";
        }
        
        // Close the db connection, timer, clean up from animation, remove title, and close the form.
        public async Task doClose() {
            this.animateTimer.Stop();
            // remove animation layers
            await Utils.clearAnimationMaps();
            // empty animation and png directories
            this.clearAnimationDir();
            this.clearPngDir();
            // only close connection after removing animation layers as the map title is affected and recalculation needs connection
            this.conn.Close();
            this.conn.Dispose();
            this._dlg.Close();
        }
        
        // Update the unitEdit box and set the unit value in the current row.
        public virtual void plotSetUnit() {
            var unitStr = this._dlg.PunitPlot.Text;
            this._dlg.PunitEdit.Text = unitStr;
            this.updateCurrentPlotRow(2);
        }

        // Load file for showing unit as a selection if necessary.  Select chosen unit.
        public async Task plotSelectUnit(int index) {
            string strng;
            string field;
            FeatureLayer layer;
            if (index < 1 || this.conn is null || this.table == "") {
                return;
            }
            //Utils.loginfo('Index is {0}',index))
            if (this.table.StartsWith("channel")) {
                if (this._gv.useGridModel) {
                    layer = Utils.getLayerByLegend(Utils._GRIDSTREAMSLEGEND) as FeatureLayer;
                    if (layer is null) {
                        layer = Utils.getLayerByLegend(Utils._DRAINSTREAMSLEGEND) as FeatureLayer;
                    }
                    field = Topology._LINKNO;
                } else {
                    layer = Utils.getLayerByLegend(Utils._CHANNELREACHESLEGEND) as FeatureLayer;
                    field = Topology._CHANNEL;
                }
            } else if (this.table.StartsWith("lsunit")) {
                if (this._gv.useGridModel) {
                    layer = Utils.getLayerByLegend(Utils._LSUSLEGEND) as FeatureLayer;
                } else {
                    var lsuFile = Utils.join(this._gv.shapesDir, "lsus2.shp");
                    if (!File.Exists(lsuFile)) {
                        lsuFile = Utils.join(this._gv.shapesDir, "lsus1.shp");
                    }
                    layer = (await Utils.getLayerByFilename(lsuFile, FileTypes._LSUS, null, null, null)).Item1 as FeatureLayer;
                }
                field = Topology._LSUID;
            } else if (this.table.StartsWith("aquifer")) {
                var aquFile = Utils.join(this._gv.resultsDir, "aquifers.shp");
                layer = (await Utils.getLayerByFilename(aquFile, FileTypes._AQUIFERS, null, null, null)).Item1 as FeatureLayer;
                field = Topology._AQUIFER;
            } else if (this.table.StartsWith("hru")) {
                if (this._gv.useGridModel) {
                    layer = Utils.getLayerByLegend(Utils._HRUSLEGEND) as FeatureLayer;
                } else {
                    var hruFile = Utils.join(this._gv.shapesDir, "hrus2.shp");
                    if (!File.Exists(hruFile)) {
                        hruFile = Utils.join(this._gv.shapesDir, "hrus1.shp");
                        if (!File.Exists(hruFile)) {
                            return;
                        }
                    }
                    layer = (await Utils.getLayerByFilename(hruFile, FileTypes._HRUS, null, null, null)).Item1 as FeatureLayer;
                }
                field = Topology._HRUS;
            } else {
                return;
            }
            if (layer is null) {
                return;
            }
            //Utils.loginfo('Layer is {0}',layer))
            var val = this._dlg.PunitPlot.Items[index].ToString();
            //Utils.loginfo('Value is {0}',val))
            await Utils.setLayerVisibility(layer, true);
            await QueuedTask.Run(() => { layer.ClearSelection(); });
            //Utils.loginfo('Selection removed')
            if (field == Topology._HRUS) {
                // need to match n or n1, n2 or n1, n2, n3
                strng = string.Format("{0} = {1} OR {0} LIKE '{1},%' OR  {0} LIKE \'%, {1}\' OR  {0}  LIKE \'%, {1},%\'", field, val);
            } else {
                strng = string.Format("{0} = {1}", field, val);
            }
            var qf = new QueryFilter();
            qf.WhereClause = strng;
            layer.Select(qf);
        }
        
        // If the unitEdit contains a string in the unitPlot combo box, 
        //         Update the current index of the combo box, and set the unit value in the current row.
        public virtual void plotEditUnit() {
            var unitStr = this._dlg.PunitEdit.Text;
            var index = this._dlg.PunitPlot.FindStringExact(unitStr);
            if (index >= 1) {
                // avoid initial empty text as well as 'not found' value of -1
                this._dlg.PunitPlot.SelectedIndex = index;
                this.updateCurrentPlotRow(2);
            }
        }
        
        // Update the variable in the current plot row.
        public virtual void plotSetVar() {
            var vari = this._dlg.PvariablePlot.SelectedItem.ToString();
            this.updateCurrentPlotRow(3);
        }
        
        // Write data for plot rows to csv file.
        public virtual void writePlotData() {
            string @ref;
            if (this.conn is null) {
                return;
            }
            if (!this.setPeriods()) {
                return;
            }
            if (!this.checkFrequencyConsistent()) {
                Utils.error("All rows in the table must have the same frequency: annual, monthly or daily", this._gv.isBatch);
                return;
            }
            if (this._dlg.PplotType.SelectedIndex < 0) {
                Utils.information("Please select a plot type", this._gv.isBatch);
                return;
            }
            var numRows = this.plotData.Rows.Count;
            if (numRows == 0) {
                Utils.information("There are no rows to plot", this._gv.isBatch);
                return;
            }
            if (this._dlg.PplotType.SelectedItem.ToString() == Visualise._SCATTER) {
                if (numRows == 1) {
                    Utils.information("You need two rows for a scatter plot", this._gv.isBatch);
                    return;
                }
                if (numRows > 2) {
                    Utils.information(string.Format("You need 2 rows for a scatter plot, and you have {0}.  Only the first two will be used.", numRows), this._gv.isBatch);
                }
            }
            var dataForPlot = new Dictionary<int, List<string>>();
            var labels = new Dictionary<int, string>();
            var dates = new List<string>();
            var datesDone = false;
            foreach (var i in Enumerable.Range(0, numRows)) {
                dataForPlot[i] = new List<string>();
                DataRow row = this.plotData.Rows[i];
                var scenario = row[0].ToString();
                var table = row[1].ToString();
                var gisId = row[2].ToString();
                var vari = row[3].ToString();
                if (scenario == "" || table == "" || gisId == "" || vari == "") {
                    Utils.information(string.Format("Row {0} is incomplete", i + 1), this._gv.isBatch);
                    return;
                }
                if (scenario == "observed" && table == "-") {
                    if (File.Exists(this.observedFileName)) {
                        labels[i] = string.Format("observed-{0}",vari.Trim());
                        dataForPlot[i] = this.readObservedFile(vari);
                    } else {
                        Utils.error(string.Format("Cannot find observed data file {0}", this.observedFileName), this._gv.isBatch);
                        return;
                    }
                } else {
                    var num = Convert.ToInt32(gisId);
                    var where = string.Format("gis_id={0}", num);
                    if (scenario != this.scenario) {
                        // need to change database
                        this.setConnection(scenario);
                        if (!this.readData("", false, table, vari, where)) {
                            return;
                        }
                        // restore database
                        this.setConnection(this.scenario);
                    } else if (!this.readData("", false, table, vari, where)) {
                        return;
                    }
                    var (units, _) = this.getUnitsAndDescription(table, vari);
                    labels[i] = string.Format("{0}-{1}-{2}-{3}({4})",scenario, table, num, vari, units);
                    (isDaily, isMonthly, isAnnual, _) = Visualise.tableIsDailyMonthlyOrAnnual(table);
                    var (year, tim) = this.startYearTime(isDaily, isMonthly);
                    var (finishYear, finishTime) = this.finishYearTime(isDaily, isMonthly);
                    var layerData = this.staticData[""];
                    while (year < finishYear || year == finishYear && tim <= finishTime) {
                        if (!layerData.ContainsKey(num)) {
                            Utils.error(string.Format("Insufficient data for unit {0} for plot {1}",num, i + 1), this._gv.isBatch);
                            return;
                        }
                        var unitData = layerData[num];
                        if (!unitData.ContainsKey(vari)) {
                            Utils.error(string.Format("Insufficient data for variable {0} for plot {1}",vari, i + 1), this._gv.isBatch);
                            return;
                        }
                        var varData = unitData[vari];
                        if (!varData.ContainsKey(year)) {
                            Utils.error(string.Format("Insufficient data for year {0} for plot {1}",year, i + 1), this._gv.isBatch);
                            return;
                        }
                        var yearData = varData[year];
                        double val = 0;
                        if (isAnnual) {
                            // time values in yearData are arbitrary and should be ignored
                            // there should be just one record
                            if (yearData.Count == 0) {
                                Utils.error(string.Format("Insufficient data for year {0} for plot {1}",year, i + 1), this._gv.isBatch);
                                return;
                            }
                            foreach (var val0 in yearData.Values) {
                                break;
                            }
                        } else {
                            if (!yearData.ContainsKey(tim)) {
                                if (isDaily) {
                                    @ref = string.Format("day {0}", tim);
                                } else {
                                    @ref = string.Format("month {0}", tim);
                                }
                                Utils.error(string.Format("Insufficient data for {0} for year {1} for plot {2}",@ref, year, i + 1), this._gv.isBatch);
                                return;
                            }
                            val = yearData[tim];
                        }
                        dataForPlot[i].Add(string.Format("{0:G3}", val));
                        if (!datesDone) {
                            if (isDaily) {
                                dates.Add((year * 1000 + tim).ToString());
                            } else if (isAnnual) {
                                dates.Add(year.ToString());
                            } else {
                                dates.Add(year.ToString() + "/" + tim.ToString());
                            }
                        }
                        (year, tim) = this.nextDate(year, tim, isDaily, isMonthly, isAnnual);
                    }
                    datesDone = true;
                }
            }
            // data all collected: write csv file
            var dlg = new SaveFileDialog() {
                Title = "Choose a csv file",
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                InitialDirectory = this._gv.scenariosDir
            };
            var result = dlg.ShowDialog();
            if (result != DialogResult.OK) {
                return;
            }
            var csvFile = dlg.FileName;
            // on Linux, extension not added automatically
            if (!Path.HasExtension(csvFile)) {
                csvFile += ".csv";
            }
            using (var fw = new StreamWriter(csvFile)) {
                fw.Write("Date,");
                foreach (var i in Enumerable.Range(0, numRows - 1)) {
                    fw.Write(labels[i]);
                    fw.Write(",");
                }
                fw.WriteLine(labels[numRows - 1]);
                foreach (var d in Enumerable.Range(0, dates.Count)) {
                    fw.Write(dates[d]);
                    fw.Write(",");
                    foreach (var i in Enumerable.Range(0, numRows)) {
                        if (!dataForPlot.ContainsKey(i)) {
                            Utils.error(string.Format("Missing data for plot {0}", i + 1), this._gv.isBatch);
                            fw.WriteLine("");
                            return;
                        }
                        if (!Enumerable.Range(0, dataForPlot[i].Count).Contains(d)) {
                            Utils.error(string.Format("Missing data for date {0} for plot {1}", dates[d], i + 1), this._gv.isBatch);
                            fw.WriteLine("");
                            return;
                        }
                        fw.Write(dataForPlot[i][d]);
                        if (i < numRows - 1) {
                            fw.Write(",");
                        } else {
                            fw.WriteLine("");
                            //         commands = []
                            //         settings = QSettings()
                            //         commands.Add(Utils.join(Utils.join(settings.value('/QSWAT/SWATEditorDir'), Parameters._SWATGRAPH), Parameters._SWATGRAPH))
                            //         commands.Add(csvFile)
                            //         subprocess.Popen(commands)
                            // above replaced with swatGraph form
                        }
                    }
                }
            }
            var filename = Path.Combine(Parameters._HOMEDIR, "SWATPlus/Tools/SWATGraph/runSWATGraphScript.bat");
            Process graph = new Process {
                StartInfo = new ProcessStartInfo() {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    FileName = filename,
                    Arguments = string.Format("{0} {1}", csvFile, this._dlg.PplotType.SelectedIndex + 1)
                }
            };
            //var graph = SWATGraph(csvFile, this._dlg.PplotType.SelectedItem.ToString());
            graph.Start();

            //var parms = Geoprocessing.MakeValueArray(csvFile, this._dlg.PplotType.SelectedIndex + 1);
            ////Utils.runPython("swatgraph.py", parms, this._gv);
            //Utils.runPython("SWATGraphScript.py", parms, this._gv);
        }
        
        // Read data from database table into staticData.  Return True if no error detected.
        public virtual bool readData(
            string layerId,
            bool isStatic,
            string table,
            string vari,
            string where) {
            double val;
            int gisId;
            int year;
            int tim;
            List<string> varz;
            if (this.conn is null) {
                return false;
            }
            // clear existing data for layerId
            this.staticData[layerId] = new Dictionary<int, Dictionary<string, Dictionary<int, Dictionary<int, double>>>>();
            var layerData = this.staticData[layerId];
            //self.areas = dict()
            //self.hasAreas = False
            this.animateResultsData[layerId] = new Dictionary<int, Dictionary<int, double>>();
            if (isStatic) {
                varz = this.varList(true);
            } else {
                varz = new List<string> {
                    "[" + vari + "]"
                };
            }
            var numVars = varz.Count;
            var isDaily = Visualise.tableIsDaily(table);
            var isMonthly = Visualise.tableIsMonthly(table);
            string selectString = "";
            string sql = "";
            int preLen = 0;
            if (isDaily) {
                selectString = "[jday], [yr], [gis_id], " + string.Join(',', varz);
                sql = DBUtils.sqlSelect(table, selectString, "[yr], [jday]", where);
                preLen = 3;
            } else if (isMonthly) {
                selectString = "[mon], [yr], [gis_id], " + string.Join(',', varz);
                sql = DBUtils.sqlSelect(table, selectString, "[yr], [mon]", where);
                preLen = 3;
            } else {
                // annual or average annual
                selectString = "[yr], [gis_id], " + string.Join(',', varz);
                sql = DBUtils.sqlSelect(table, selectString, "[yr]", where);
                preLen = 2;
            }
            // Utils.information('SQL: {0}',sql), self._gv.isBatch)
            try {
                using (var reader = this.getReader(sql)) {
                    while (reader.Read()) {
                        if (isDaily || isMonthly) {
                            tim = reader.GetInt32(0);
                            year = reader.GetInt32(1);
                            gisId = reader.GetInt32(2);
                        } else {
                            tim = reader.GetInt32(0);
                            year = tim;
                            gisId = reader.GetInt32(1);
                        }
                        if (!this.inPeriod(year, tim, table, isDaily, isMonthly)) {
                            continue;
                        }
                        if (!layerData.ContainsKey(gisId)) {
                            layerData[gisId] = new Dictionary<string, Dictionary<int, Dictionary<int, double>>>();
                        }
                        foreach (var i in Enumerable.Range(0, numVars)) {
                            // remove square brackets from each var
                            vari = varz[i];
                            vari = vari.Substring(1, vari.Length - 2);
                            val = reader.GetDouble(i + preLen);
                            if (!layerData[gisId].ContainsKey(vari)) {
                                layerData[gisId][vari] = new Dictionary<int, Dictionary<int, double>>();
                            }
                            if (!layerData[gisId][vari].ContainsKey(year)) {
                                layerData[gisId][vari][year] = new Dictionary<int, double>();
                            }
                            layerData[gisId][vari][year][tim] = val;
                        }
                    }
                }
            } catch {
                // get scenario name
                var scenario = this.scenarioFromDb();
                Utils.error(string.Format("Cannot find {0} data for scenario {1}", table, scenario), this._gv.isBatch);
                return false;
            }
            if (layerData.Count == 0) {
                Utils.error("No data has nbeen read.  Perhaps your dates are outside the dates of the table", this._gv.isBatch);
                return false;
            }
            this.summaryChanged = true;
            return true;
        }
        
        // Return scenario name from current setting of this.db.
        public virtual string scenarioFromDb() {
            var dbDir = Path.GetDirectoryName(this.db);
            var scenDir = Path.GetDirectoryName(dbDir);
            return Path.GetFileName(scenDir);
            //     def readDailyFlowData(self, table: str, channel: int, monthData):
            //         """Read data from table for channel, organised by month."""
        }
        
        // 
        //         Return true if year and tim are within requested period.
        //         
        //         Assumes self.[julian]startYear/Month/Day and self.[julian]finishYear/Month/Day already set.
        //         Assumes tim is within 1..365/6 when daily, and within 1..12 when monthly.
        //         
        public virtual bool inPeriod(
            int year,
            int tim,
            string table,
            bool isDaily,
            bool isMonthly) {
            if (year < this.startYear || year > this.finishYear) {
                return false;
            }
            if (Visualise.tableIsYearly(table)) {
                return true;
            }
            if (isDaily) {
                if (year == this.startYear) {
                    return tim >= this.julianStartDay;
                }
                if (year == this.finishYear) {
                    return tim <= this.julianFinishDay;
                }
                return true;
            }
            if (isMonthly) {
                if (year == this.startYear) {
                    return tim >= this.startMonth;
                }
                if (year == this.finishYear) {
                    return tim <= this.finishMonth;
                }
                return true;
            }
            // annual or average annual
            return true;
        }
        
        // Check all non-observed rows are daily, monthly or annual.
        public virtual bool checkFrequencyConsistent() {
            var isDaily = false;
            var isMonthly = false;
            var isAnnual = false;
            var frequencySet = false;
            var numRows = this.plotData.Rows.Count;
            foreach (var i in Enumerable.Range(0, numRows)) {
                var table = this.plotData.Rows[i][1].ToString();
                if (table == "-") {
                    // observed
                    continue;
                }
                if (Visualise.tableIsMonthly(table)) {
                    if (frequencySet) {
                        if (!isMonthly) {
                            return false;
                        }
                    } else {
                        isMonthly = true;
                        frequencySet = true;
                    }
                } else if (Visualise.tableIsAnnual(table)) {
                    if (frequencySet) {
                        if (!isAnnual) {
                            return false;
                        }
                    } else {
                        isAnnual = true;
                        frequencySet = true;
                    }
                } else if (Visualise.tableIsDaily(table)) {
                    if (frequencySet) {
                        if (!isDaily) {
                            return false;
                        }
                    } else {
                        isDaily = true;
                        frequencySet = true;
                    }
                } else {
                    // can we get here?  Assume it is OK
                }
            }
            return true;
        }
        
        // if isStatic, summarise data in staticData, else store all data for animate variable, saving in resultsData.
        public virtual void summariseData(string layerId, bool isStatic) {
            var layerData = this.staticData[layerId];
            if (isStatic) {
                foreach (var (index, vym) in layerData) {
                    foreach (var (vari, ym) in vym) {
                        var val = this.summarise(ym);
                        if (!this.staticResultsData.ContainsKey(index)) {
                            this.staticResultsData[index] = new Dictionary<string, double>();
                        }
                        this.staticResultsData[index][vari] = val;
                    }
                }
            } else {
                this.allAnimateVals = new List<double>();
                if (!this.animateResultsData.ContainsKey(layerId)) {
                    this.animateResultsData[layerId] = new Dictionary<int, Dictionary<int, double>>();
                }
                var results = this.animateResultsData[layerId];
                foreach (var (index, vym) in layerData) {
                    foreach (var ym in vym.Values) {
                        foreach (var (y, mval) in ym) {
                            foreach (var (m, val) in mval) {
                                var dat = this.makeDate(y, m);
                                if (!results.ContainsKey(dat)) {
                                    results[dat] = new Dictionary<int, double>();
                                }
                                results[dat][index] = val;
                                this.allAnimateVals.Add(val);
                            }
                        }
                    }
                }
            }
        }
        
        // 
        //         Make date number from year and tim according to period.
        //         
        //         tim is time field, which may be year, month or day according to period.
        //         
        public virtual int makeDate(int year, int tim) {
            if (this.isDaily) {
                return year * 1000 + tim;
            } else if (this.isMonthly) {
                return year * 100 + tim;
            } else {
                // annual or average annual
                return year;
            }
        }
        
        // Return (year, tim) pair for start date according to period.
        public virtual (int, int) startYearTime(bool isDaily, bool isMonthly) {
            if (isDaily) {
                return (this.startYear, this.julianStartDay);
            } else if (isMonthly) {
                return (this.startYear, this.startMonth);
            } else {
                // annual or average annual
                return (this.startYear, this.startYear);
            }
        }
        
        // Return (year, tim) pair for finish date according to period.
        public virtual (int, int) finishYearTime(bool isDaily, bool isMonthly) {
            if (isDaily) {
                return (this.finishYear, this.julianFinishDay);
            } else if (isMonthly) {
                return (this.finishYear, this.finishMonth);
            } else {
                // annual or average annual
                return (this.finishYear, this.finishYear);
            }
        }
        
        // Get next (year, tim) pair according to period.
        //         
        //         self.interval only used for daily data
        public virtual (int, int) nextDate(
            int year,
            int tim,
            bool isDaily,
            bool isMonthly,
            bool isAnnual) {
            if (isAnnual) {
                return (year + 1, year + 1);
            } else if (isDaily) {
                tim += this.interval;
                var yearLength = Visualise.isLeap(year) ? 366 : 365;
                while (tim > yearLength) {
                    year += 1;
                    tim -= yearLength;
                    yearLength = Visualise.isLeap(year) ? 366 : 365;
                }
                return (year, tim);
            } else if (isMonthly) {
                tim += 1;
                if (tim == 13) {
                    year += 1;
                    tim = 1;
                }
                return (year, tim);
            } else {
                return (year, tim);
            }
        }
        
        // Return isdaily, isMonthly, isAnnual, isAverageAnnual tuple of booleans for table.
        public static (bool, bool, bool, bool) tableIsDailyMonthlyOrAnnual(string table) {
            return (Visualise.tableIsDaily(table), Visualise.tableIsMonthly(table), Visualise.tableIsAnnual(table), Visualise.tableIsAvAnnual(table));
        }
        
        // Return true if daily.
        public static bool tableIsDaily(string table) {
            return table.EndsWith("_day");
        }
        
        // Return true if monthly.
        public static bool tableIsMonthly(string table) {
            return table.EndsWith("_mon");
        }
        
        // Return true if annual.
        public static bool tableIsAnnual(string table) {
            return table.EndsWith("_yr");
        }
        
        // Return true if average annual.
        public static bool tableIsAvAnnual(string table) {
            return table.EndsWith("_aa");
        }

        // Return true if annual or average annual.
        public static bool tableIsYearly(string table) {
            return Visualise.tableIsAnnual(table) || Visualise.tableIsAvAnnual(table);
        }
        
        // Summarise values according to summary method.
        public virtual double summarise(Dictionary<int, Dictionary<int, double>> data) {
            if (this.isAA) {
                // there will only be one value, so total will simply return it
                return this.summariseTotal(data);
            }
            if (this._dlg.PsummaryCombo.SelectedItem.ToString() == Visualise._TOTALS) {
                return this.summariseTotal(data);
            } else if (this._dlg.PsummaryCombo.SelectedItem.ToString() == Visualise._ANNUALMEANS) {
                return this.summariseAnnual(data);
            } else if (this._dlg.PsummaryCombo.SelectedItem.ToString() == Visualise._MONTHLYMEANS) {
                return this.summariseMonthly(data);
            } else if (this._dlg.PsummaryCombo.SelectedItem.ToString() == Visualise._DAILYMEANS) {
                return this.summariseDaily(data);
            } else if (this._dlg.PsummaryCombo.SelectedItem.ToString() == Visualise._MAXIMA) {
                return this.summariseMaxima(data);
            } else if (this._dlg.PsummaryCombo.SelectedItem.ToString() == Visualise._MINIMA) {
                return this.summariseMinima(data);
            } else {
                Utils.error("Internal error: unknown summary method: please report", this._gv.isBatch);
                return 0;
            }
        }
        
        // Sum values and return.
        public virtual double summariseTotal(Dictionary<int, Dictionary<int, double>> data) {
            var total = 0.0;
            foreach (var mv in data.Values) {
                foreach (var v in mv.Values) {
                    total += v;
                }
            }
            return total;
        }
        
        // Return total divided by period in years.
        public virtual double summariseAnnual(Dictionary<int, Dictionary<int, double>> data) {
            return (double)this.summariseTotal(data) / this.periodYears;
        }
        
        // Return total divided by period in months.
        public virtual double summariseMonthly(Dictionary<int, Dictionary<int, double>> data) {
            return (double)this.summariseTotal(data) / this.periodMonths;
        }
        
        // Return total divided by period in days.
        public virtual double summariseDaily(Dictionary<int, Dictionary<int, double>> data) {
            return (double)this.summariseTotal(data) / this.periodDays;
        }
        
        // Return maximum of values.
        public virtual double summariseMaxima(Dictionary<int, Dictionary<int, double>> data) {
            var maxv = 0.0;
            foreach (var mv in data.Values) {
                foreach (var v in mv.Values) {
                    maxv = Math.Max(maxv, v);
                }
            }
            return maxv;
        }
        
        // Return minimum of values.
        public virtual double summariseMinima(Dictionary<int, Dictionary<int, double>> data) {
            var minv = double.PositiveInfinity;
            foreach (var mv in data.Values) {
                foreach (var v in mv.Values) {
                    minv = Math.Min(minv, v);
                }
            }
            return minv;
        }
        
        // Return true if year is a leap year.
        public static bool isLeap(int year) {
            if (year % 4 == 0) {
                if (year % 100 == 0) {
                    return year % 400 == 0;
                } else {
                    return true;
                }
            } else {
                return false;
            }
        }
        
        // Add gis id numbers to unitPlot combo.
        public virtual void setGisIdPlot() {
            if (this.conn is null || this.table == "") {
                return;
            }
            this._dlg.PunitPlot.Items.Clear();
            //this._dlg.PunitPlot.Items.Add("");
            var sql = string.Format("SELECT [gis_id] FROM {0}",this.table);
            int firstId = -1;
            try {
                using (var reader = this.getReader(sql)) {
                    while (reader.Read()) {
                        var gisId = reader.GetInt32(0);
                        if (firstId < 0) {
                            firstId = gisId;
                        } else if (gisId == firstId) {
                            return;
                        }
                        this._dlg.PunitPlot.Items.Add(gisId.ToString());
                    }
                }
                this._dlg.PunitPlot.SelectedIndex = 0;
            } catch (Exception ex) {
                Utils.error(string.Format("Table {0} in {1} has no gis_id column: {2}",this.table, this.db, ex.Message), this._gv.isBatch);
            }
        }
        
        // Return variables in variableList as a list of strings, with square brackets if bracket is true.
        public virtual List<string> varList(bool bracket) {
            var result = new List<string>();
            var numRows = this._dlg.PvariableList.Items.Count;
            foreach (var row in Enumerable.Range(0, numRows)) {
                var vari = this._dlg.PvariableList.Items[row].ToString();
                // bracket variables when using in sql, to avoid reserved words and '/'
                if (bracket) {
                    vari = "[" + vari + "]";
                }
                result.Add(vari);
            }
            return result;
        }
        
        // Set results file by asking user.
        public virtual void setResultsFile() {
            string direcUpUp;
            string path;
            try {
                path = Path.GetDirectoryName(this._dlg.PresultsFileEdit.Text);
            } catch (Exception) {
                path = "";
            }
            var @base = this.selectBase().Item1;
            if (@base is null) {
                return;
            }
            var dlg = new OpenFileDialog() {
                Title = "results",
                Filter = "Shapefiles (*.shp)|*.shp|All files (*.*)|*.*",
                InitialDirectory = path
            };
            var result = dlg.ShowDialog();
            if (result != DialogResult.OK) {
                return;
            }
            var resultsFileName = dlg.FileName;
            var direc = Path.GetDirectoryName(resultsFileName);
            var name = Path.GetFileName(resultsFileName);
            var direcUp = Path.GetDirectoryName(direc);
            var direcName = Path.GetFileName(direc);
            if (direcName == Parameters._RESULTS) {
                //# check we are not overwriting a template
                direcUpUp = Path.GetDirectoryName(direcUp);
                if (Utils.samePath(direcUpUp, this._gv.scenariosDir)) {
                    @base = Path.GetFileNameWithoutExtension(name);
                    if (new HashSet<string> {
                        Parameters._SUBS,
                        Parameters._RIVS,
                        Parameters._HRUS,
                        Parameters._LSUS,
                        Parameters._AQUIFERS,
                        Parameters._DEEPAQUIFERS
                    }.Contains(@base)) {
                        Utils.information(string.Format("The file {0} should not be overwritten: please choose another file name.", Path.ChangeExtension(resultsFileName, null) + ".shp"), this._gv.isBatch);
                        return;
                    }
                }
            } else if (direcName == Parameters._ANIMATION) {
                //# check we are not using the Animation directory
                direcUpUp = Path.GetDirectoryName(direcUp);
                if (Utils.samePath(direcUpUp, this._gv.resultsDir)) {
                    Utils.information(string.Format("Please do not use {0} for results as it can be overwritten by animation.", Path.ChangeExtension(resultsFileName, null) + ".shp"), this._gv.isBatch);
                    return;
                }
            }
            this._dlg.PresultsFileEdit.Text = resultsFileName;
            this.resultsFileUpToDate = false;
        }
        
        // Get an observed data file from the user.
        public virtual void setObservedFile(string filename) {
            string path;
            try {
                path = Path.GetDirectoryName(filename);
            }
            catch (Exception) {
                path = "";
            }
            var dlg = new OpenFileDialog() {
                Title = "Choose observed data file",
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                InitialDirectory = path
            };
            var result = dlg.ShowDialog();
            if (result != DialogResult.OK) {
                return;
            }
            this.observedFileName = dlg.FileName;
            this._dlg.PobservedFileEdit.Text = observedFileName;
            Proj proj = this._gv.proj;
            this.title = this._gv.projName;
            proj.writeEntry(this.title, "observed/observedFile", this.observedFileName);
        }
        
        // Return base name of shapefile used for results according to table name and availability of actual hrus file,
        //         plus its file type
        public virtual (string, int) selectBase(string scenario = null) {
            bool hasHRUs;
            if (this.table.StartsWith("channel_")) {
                return (Parameters._RIVS, FileTypes._CHANNELREACHES);
            }
            if (this.table.StartsWith("aquifer_")) {
                return (Parameters._AQUIFERS, FileTypes._AQUIFERS);
            }
            if (this.table.StartsWith("deep_aquifer_")) {
                return (Parameters._DEEPAQUIFERS, FileTypes._AQUIFERS);
            }
            if (this.table.StartsWith("lsunit_")) {
                return (Parameters._LSUS, FileTypes._LSUS);
            }
            if (this.table.StartsWith("hru_")) {
                if (scenario is null) {
                    hasHRUs = this.hasHRUs;
                } else {
                    var scenDir = Utils.join(this._gv.scenariosDir, scenario);
                    var resultsDir = Utils.join(scenDir, Parameters._RESULTS);
                    hasHRUs = File.Exists(Utils.join(resultsDir, Parameters._HRUS + ".shp"));
                }
                if (hasHRUs) {
                    return (Parameters._HRUS, FileTypes._HRUS);
                } else {
                    Utils.error("Cannot show results for HRUs since no full HRUs file was created", this._gv.isBatch);
                    return (null, 0);
                }
            }
            Utils.error(string.Format("Do not know how to show results for table {0}", this.table), this._gv.isBatch);
            return (null, 0);
        }
        
        // 
        //         Create results shapefile.
        //         
        //         Assumes:
        //         - resultsFileEdit contains suitable text for results file name
        //         - one or more variables is selected in variableList (and uses the first one)
        //         - resultsData is suitably populated
        //         
        public virtual async Task<bool> createResultsFile() {
            var nextResultsFile = this._dlg.PresultsFileEdit.Text;
            if (File.Exists(nextResultsFile)) {
                var reply = Utils.question(string.Format("Results file {0} already exists.  Do you wish to overwrite it?",nextResultsFile), this._gv.isBatch, true);
                if (reply != MessageBoxResult.Yes) {
                    return false;
                }
                if (nextResultsFile == this.resultsFile) {
                    await Utils.removeLayerAndFiles(this.resultsFile);
                } else {
                    // layer removed as a precaution since removing files;  Could be present, eg after a restart.
                    await Utils.removeLayerAndFiles(nextResultsFile);
                    this.resultsFile = nextResultsFile;
                }
            } else {
                this.resultsFile = nextResultsFile;
            }
            var resultsDir = Path.GetDirectoryName(this.db);
            var (baseName, ft) = this.selectBase();
            if (baseName is null) {
                return false;
            }
            var resultsBase = Utils.join(resultsDir, baseName) + ".shp";
            var outdir = Path.GetDirectoryName(this.resultsFile);
            var outbase = Path.GetFileNameWithoutExtension(this.resultsFile);
            Utils.copyShapefile(resultsBase, outbase, outdir);
            var selectVar = this._dlg.PvariableList.SelectedItems[0].ToString();
            var summary = this.isAA ? Visualise._ANNUALMEANS : this._dlg.PsummaryCombo.SelectedItem.ToString();
            var legend = string.Format("{0} {1} {2}",this.scenario, selectVar, summary);
            if (baseName == Parameters._SUBS) {
                //this.subResultsLayer = QgsVectorLayer(this.resultsFile, legend, "ogr");
                //this.subResultsLayer.rendererChanged.connect(this.changeSubRenderer);
                this.internalChangeToSubRenderer = true;
                this.keepSubColours = false;
                this.currentResultsLayer = this.subResultsLayer;
            } else if (baseName == Parameters._LSUS) {
                //this.lsuResultsLayer = QgsVectorLayer(this.resultsFile, legend, "ogr");
                //this.lsuResultsLayer.rendererChanged.connect(this.changeLSURenderer);
                this.internalChangeToLSURenderer = true;
                this.keepLSUColours = false;
                this.currentResultsLayer = this.lsuResultsLayer;
            } else if (baseName == Parameters._HRUS) {
                //this.hruResultsLayer = QgsVectorLayer(this.resultsFile, legend, "ogr");
                //this.hruResultsLayer.rendererChanged.connect(this.changeHRURenderer);
                this.internalChangeToHRURenderer = true;
                this.keepHRUColours = false;
                this.currentResultsLayer = this.hruResultsLayer;
            } else if (baseName == Parameters._AQUIFERS) {
                //this.aquResultsLayer = QgsVectorLayer(this.resultsFile, legend, "ogr");
                //this.aquResultsLayer.rendererChanged.connect(this.changeAquRenderer);
                this.internalChangeToAquRenderer = true;
                this.keepAquColours = false;
                this.currentResultsLayer = this.aquResultsLayer;
            } else if (baseName == Parameters._DEEPAQUIFERS) {
                //this.deepAquResultsLayer = QgsVectorLayer(this.resultsFile, legend, "ogr");
                //this.deepAquResultsLayer.rendererChanged.connect(this.changeDeepAquRenderer);
                this.internalChangeToDeepAquRenderer = true;
                this.keepDeepAquColours = false;
                this.currentResultsLayer = this.deepAquResultsLayer;
            } else {
                //this.rivResultsLayer = QgsVectorLayer(this.resultsFile, legend, "ogr");
                //this.rivResultsLayer.rendererChanged.connect(this.changeRivRenderer);
                this.internalChangeToRivRenderer = true;
                this.keepRivColours = false;
                this.currentResultsLayer = this.rivResultsLayer;
            }
            using (var resultsDs = Ogr.Open(this.resultsFile, 1)) {
                var resultslayer = resultsDs.GetLayerByIndex(0);
                var resultsDef = resultslayer.GetLayerDefn();
                var varz = this.varList(false);
                foreach (var vari in varz) {
                    var field = new OSGeo.OGR.FieldDefn(vari, OSGeo.OGR.FieldType.OFTReal);
                    resultslayer.CreateField(field, 1);
                }
            }
            // create a map to hold results layer and background maps
            Map resultsMap = null;
            await QueuedTask.Run(() => 
                resultsMap = MapFactory.Instance.CreateMap(legend, MapType.Map, MapViewingMode.Map, Basemap.None)
            );
            FeatureLayer backLayer = null;
            var subs1File = Utils.join(this._gv.shapesDir, Parameters._SUBS1 + ".shp");
            if (File.Exists(subs1File)) {
                backLayer = await QueuedTask.Run(() =>
                    LayerFactory.Instance.CreateLayer(new Uri(subs1File), resultsMap, 0, Utils._SUBBASINSLEGEND) as FeatureLayer
                    );
                ft = FileTypes._SUBBASINS;
                await FileTypes.ApplySymbolToFeatureLayerAsync(backLayer, ft, this._gv);
                await Utils.setMapTip(backLayer, ft, this._gv);
                // add subbasin labels
                var label = backLayer.LabelClasses.FirstOrDefault();
                await QueuedTask.Run(() =>
                {
                    if (label != null)
                    {
                        label.SetExpression("$feature.Subbasin");
                        backLayer.SetLabelVisibility(true);
                    }
                    // avoid showing upslope subbasins in legend
                    backLayer.SetExpanded(false);
                });
            } 
            this.currentResultsLayer = await QueuedTask.Run(() =>
                LayerFactory.Instance.CreateLayer(new Uri(this.resultsFile),
                        resultsMap, 0, legend) as FeatureLayer);
            if (baseName == Parameters._SUBS) {
                this.subResultsLayer = this.currentResultsLayer;
            } else if (baseName == Parameters._LSUS) {
                this.lsuResultsLayer = this.currentResultsLayer;
            } else if (baseName == Parameters._HRUS) {
                this.hruResultsLayer = this.currentResultsLayer;
            } else if (baseName == Parameters._AQUIFERS) {
                this.aquResultsLayer = this.currentResultsLayer;
            } else if (baseName == Parameters._DEEPAQUIFERS) {
                this.deepAquResultsLayer = this.currentResultsLayer;
            } else {
                this.rivResultsLayer = this.currentResultsLayer;
            }
            await this.updateResultsFile();
            //this.currentResultsLayer = cast(QgsVectorLayer, proj.addMapLayer(this.currentResultsLayer, false));
            //var resultsGroup = Utils.getGroupLayerByName(Utils._RESULTS_GROUP_NAME);
            //Debug.Assert(resultsGroup is not null);
            //resultsGroup.insertLayer(0, this.currentResultsLayer);
            //this._gv.iface.setActiveLayer(this.currentResultsLayer);
            //if (baseName == Parameters._SUBS) {
            //    // add labels
            //    this.currentResultsLayer.loadNamedStyle(Utils.join(this._gv.plugin_dir, "subresults.qml"));
            //    this.internalChangeToSubRenderer = false;
            //} else if (baseName == Parameters._LSUS) {
            //    this.internalChangeToLSURenderer = false;
            //} else if (baseName == Parameters._HRUS) {
            //    this.internalChangeToHRURenderer = false;
            //} else if (baseName == Parameters._AQUIFERS) {
            //    this.internalChangeToAquRenderer = false;
            //} else if (baseName == Parameters._DEEPAQUIFERS) {
            //    this.internalChangeToDeepAquRenderer = false;
            //} else {
            //    this.internalChangeToRivRenderer = false;
            //}
            //var baseMapTip = FileTypes.mapTip(ft);
            //this.currentResultsLayer.setMapTipTemplate(baseMapTip + string.Format("<br/><b>{0}:</b> [% \"{0}\" %]",selectVar));
            //this.currentResultsLayer.updatedFields.connect(this.addResultsVars);
            if (resultsMap != MapView.Active.Map) {
                await resultsMap.OpenViewAsync();
            }
            return true;
        }
        
        // Write resultsData to resultsFile.
        public virtual async Task updateResultsFile() {
            string @ref;
            int unit;
            (string @base, _) = this.selectBase();
            if (@base is null) {
                return;
            }
            var layer = @base == Parameters._SUBS ? this.subResultsLayer : @base == Parameters._LSUS ? this.lsuResultsLayer : @base == Parameters._HRUS ? this.hruResultsLayer : @base == Parameters._AQUIFERS ? this.aquResultsLayer : @base == Parameters._DEEPAQUIFERS ? this.deepAquResultsLayer : this.rivResultsLayer;
            var varz = this.varList(false);
            //var varIndexes = new dict();
            //foreach (var vari in varz) {
            //    varIndexes[var] = this._gv.topo.getIndex(layer, var);
            //}
            Debug.Assert(layer is not null);
            await QueuedTask.Run(async () => {
            using (var rc = layer.Search()) {
                while (rc.MoveNext()) {
                        using (var f = rc.Current as ArcGIS.Core.Data.Feature) {
                            bool result = false;
                            var op = new ArcGIS.Desktop.Editing.EditOperation();
                            if (@base == Parameters._HRUS) {
                                // May be split HRUs; just use first
                                // This is inadequate for some variables, but no way to know if correct val is sum of vals, mean, etc.
                                var unitText = f[Topology._HRUS].ToString();
                                if (unitText.Contains("PND") || unitText.Contains("RES")) {
                                    continue;
                                }
                                unit = Convert.ToInt32(unitText.Split(",")[0]);
                            } else if (@base == Parameters._LSUS) {
                                unit = Convert.ToInt32(f[Topology._LSUID]);
                            } else if (@base == Parameters._SUBS) {
                                unit = Convert.ToInt32(f[Topology._SUBBASIN]);
                            } else if (@base == Parameters._AQUIFERS || @base == Parameters._DEEPAQUIFERS) {
                                unit = Convert.ToInt32(f[Topology._AQUIFER]);
                            } else {
                                unit = Convert.ToInt32(f[Topology._CHANNEL]);
                            }
                            foreach (var vari in varz) {
                                Dictionary<string, double> subData;
                                double data = 0;
                                bool ok = false;
                                if (this.staticResultsData.TryGetValue(unit, out subData)) {
                                    if (subData.TryGetValue(vari, out data)) {
                                        ok = true;
                                    }
                                }
                                if (!ok) {
                                    if (@base == Parameters._HRUS) {
                                        @ref = $"HRU {unit}";
                                    } else if (@base == Parameters._LSUS) {
                                        @ref = $"LSU {unit}";
                                    } else if (@base == Parameters._SUBS) {
                                        @ref = $"subbasin {unit}";
                                    } else if (@base == Parameters._AQUIFERS) {
                                        @ref = $"aquifer {unit}";
                                    } else if (@base == Parameters._DEEPAQUIFERS) {
                                        @ref = $"deep aquifer {unit}";
                                    } else {
                                        @ref = $"channel {unit}";
                                    }
                                    Utils.error(string.Format("Cannot get data for variable {0} for {1} in table {2} in {3}: have you run SWAT+ and saved data since running QSWAT+?", vari, @ref, this.table, this.db), this._gv.isBatch);
                                    return;
                                }
                                var variShort = vari.Substring(0, Math.Min(10, vari.Length));
                                op.Modify(f, variShort, data);
                            }
                            if (!op.IsEmpty) {
                                result = await op.ExecuteAsync();
                            }
                            if (!result) {
                                Utils.error(string.Format("Could not set attributes in results file {0}", this.resultsFile), this._gv.isBatch);
                                return;
                            }
                        }
                    }
                }
            });
            this.summaryChanged = false;
            if (Project.Current.HasEdits) {
                await Project.Current.SaveEditsAsync();
            }
        }

        // Make renderer with Jenks algorithm using vals of var, setting colour rampe to ramp, inverted if invert
        public virtual async Task<CIMClassBreaksRenderer> makeJenksRenderer(List<double> vals, CIMColorRamp ramp, string vari) {
            var upperBound = vals.Max();
            var count = 5;
            // algoritm provides lower bounds for breaks
            var cbreaks = JenksFisher.CreateJenksFisherBreaksArray(vals, count);
            var breaks = "Breaks: ";
            for (int i = 0; i < cbreaks.Count; i++) {
                breaks += cbreaks[i];
                if (i < cbreaks.Count - 1) {
                    breaks += ", ";
                }
            }
            if (cbreaks.Count < count) {
                Utils.loginfo($"Only {cbreaks.Count} Jenks breaks generated");
                if (cbreaks.Count == 0) {
                    return null;
                } else {
                    count = cbreaks.Count;
                }
            }
            Utils.loginfo(breaks);
            var colours = await QueuedTask.Run(() => ColorFactory.Instance.GenerateColorsFromColorRamp(ramp, count));
            var classBreaks = new List<CIMClassBreak>();
            foreach (var i in Enumerable.Range(0, count)) {
                // adjust min and max by 1% to avoid rounding errors causing values to be outside the range
                var minVal = i == 0 ? cbreaks[0] * 0.99 : cbreaks[i];
                var maxVal = i == count - 1 ? upperBound * 1.01 : cbreaks[i + 1];
                var colour = colours[i];
                classBreaks.Add(this.makeSymbologyForRange(minVal, maxVal, colour, 3));
            }
            var renderer = new CIMClassBreaksRenderer() {
                Breaks = classBreaks.ToArray(),
                ColorRamp = ramp,
                Field = vari.Substring(0, Math.Min(10, vari.Length))
            };
            return renderer;
        }

        // 
        //         Colour results layer according to current results variable and update legend.
        //         
        //         If keepColours is true the existing colour ramp and number of classes can be reused
        //         
        public virtual async Task colourResultsFile(FeatureLayer layer = null, CIMClassBreaksRenderer renderer = null, CIMColorRamp ramp = null) {
            bool keepColours = false;
            CIMSymbol symbol;
            var haveLayer = layer is not null;
            if (haveLayer) {
                keepColours = false;
            }
            (string @base, _) = this.selectBase();
            if (@base is null) {
                return;
            }
            if (@base == Parameters._SUBS) {
                if (!haveLayer) {
                    layer = this.subResultsLayer;
                    keepColours = this.keepSubColours;
                }
                symbol = SymbolFactory.Instance.ConstructPolygonSymbol();
            } else if (@base == Parameters._LSUS) {
                if (!haveLayer) {
                    layer = this.lsuResultsLayer;
                    keepColours = this.keepLSUColours;
                }
                symbol = SymbolFactory.Instance.ConstructPolygonSymbol();
            } else if (@base == Parameters._HRUS) {
                if (!haveLayer) {
                    layer = this.hruResultsLayer;
                    keepColours = this.keepHRUColours;
                }
                symbol = SymbolFactory.Instance.ConstructPolygonSymbol();
            } else if (@base == Parameters._AQUIFERS) {
                if (!haveLayer) {
                    layer = this.aquResultsLayer;
                    keepColours = this.keepAquColours;
                }
                symbol = SymbolFactory.Instance.ConstructPolygonSymbol();
            } else if (@base == Parameters._DEEPAQUIFERS) {
                if (!haveLayer) {
                    layer = this.deepAquResultsLayer;
                    keepColours = this.keepDeepAquColours;
                }
                symbol = SymbolFactory.Instance.ConstructPolygonSymbol();
            } else {
                if (!haveLayer) {
                    layer = this.rivResultsLayer;
                    keepColours = this.keepRivColours;
                }
                //var props = new Dictionary<object, object> {
                //    {
                //        "width_expression",
                //        QSWATTopology._PENWIDTH}};
                symbol = SymbolFactory.Instance.ConstructLineSymbol();
            }
            var selectVar = this._dlg.PvariableList.SelectedItems[0].ToString();
            var selectVarShort = selectVar.Substring(0, Math.Min(selectVar.Length, 10));
            var summary = this.isAA ? Visualise._ANNUALMEANS : this._dlg.PsummaryCombo.SelectedItem.ToString();
            Debug.Assert(layer is not null);
            if (!haveLayer) {
                await QueuedTask.Run(() => {
                    layer.SetName(string.Format("{0} {1} {2}", this.scenario, selectVar, summary));
                });
            }
            int count = 5;
            double transparency = 0;
            // TODO for now at least calculate renderer afresh
            //if (!keepColours) {
            //    var count = 5;
            //    var opacity = @base == Parameters._RIVS ? 1.0 : 0.65;
            //} else {
            //    // same layer as currently - try to use same range size and colours, and same opacity
            //    try {
            //        var oldRenderer = cast(QgsGraduatedSymbolRenderer, layer.renderer());
            //        var oldRanges = oldRenderer.ranges();
            //        count = oldRanges.Count;
            //        ramp = oldRenderer.sourceColorRamp();
            //        opacity = layer.opacity();
            //    } catch (Exception) {
            //        // don't care if no suitable colours, so no message, just revert to defaults
            //        keepColours = false;
            //        count = 5;
            //        opacity = @base == Parameters._RIVS ? 1.0 : 0.65;
            //    }
            //}
            if (true) {   // was (!keepColours) {
                if (ramp is null) {
                    ramp = await this.chooseColorRamp(this.table, selectVar);
                }
                if (ramp is null) {
                    var fileName = await Utils.layerFilename(layer);
                    Utils.error(string.Format("Internal error: Failed to make renderer for {0}", fileName), this._gv.isBatch);
                }
            }
            GraduatedColorsRendererDefinition rendererDefn = null;
            if (renderer is null) {
                rendererDefn = new GraduatedColorsRendererDefinition() {
                    ClassificationField = selectVarShort,
                    ClassificationMethod = ArcGIS.Core.CIM.ClassificationMethod.NaturalBreaks,
                    BreakCount = count,
                    ColorRamp = ramp,
                    SymbolTemplate = symbol.MakeSymbolReference()
                };
            } else {
                //renderer..setSourceSymbol(symbol);
                //renderer.setLabelFormat(labelFmt);
            }
            if (@base == Parameters._SUBS) {
                this.internalChangeToSubRenderer = true;
            } else if (@base == Parameters._LSUS) {
                this.internalChangeToLSURenderer = true;
            } else if (@base == Parameters._HRUS) {
                this.internalChangeToHRURenderer = true;
            } else if (@base == Parameters._AQUIFERS) {
                this.internalChangeToAquRenderer = true;
            } else if (@base == Parameters._DEEPAQUIFERS) {
                this.internalChangeToDeepAquRenderer = true;
            } else {
                this.internalChangeToRivRenderer = true;
            }
            await QueuedTask.Run(() => {
                if (renderer is null) {
                    renderer = layer.CreateRenderer(rendererDefn) as CIMClassBreaksRenderer;
                }
                CIMVisualVariable[] visualVars = new CIMVisualVariable[1];
                var expressionInfo = new CIMExpressionInfo() {
                    Expression = "$feature.PenWidth",
                    Title = "Custom"
                };
                var sizeVar = new CIMSizeVisualVariable() {
                    ValueExpressionInfo = expressionInfo,
                    VariableType = SizeVisualVariableType.Expression,
                    MaxSize = 0,
                    MinValue = 1,
                    MaxValue = 8
                };
                visualVars[0] = sizeVar;
                renderer.VisualVariables = visualVars;
                layer.SetRenderer(renderer);
                layer.SetTransparency(transparency);
                //this.clearMapTitle();
                //this.mapTitle = MapTitle(canvas, this.title, layer);
                //MapView.Active.Redraw(true);
                this.setMapTitle(layer);
            });
            if (@base == Parameters._SUBS) {
                this.internalChangeToSubRenderer = false;
                this.keepSubColours = keepColours;
            } else if (@base == Parameters._LSUS) {
                this.internalChangeToLSURenderer = false;
                this.keepLSUColours = keepColours;
            } else if (@base == Parameters._HRUS) {
                this.internalChangeToHRURenderer = false;
                this.keepHRUColours = keepColours;
            } else if (@base == Parameters._AQUIFERS) {
                this.internalChangeToAquRenderer = false;
                this.keepAquColours = keepColours;
            } else if (@base == Parameters._DEEPAQUIFERS) {
                this.internalChangeToDeepAquRenderer = false;
                this.keepDeepAquColours = keepColours;
            } else {
                this.internalChangeToRivRenderer = false;
                this.keepRivColours = keepColours;
            }
            var tip = string.Format("\"{0}: \" + Text($feature.{1})", selectVar, selectVarShort);
            await Utils.setMapTip(this.currentResultsLayer, selectVarShort, tip);
        }
        
        // Add any extra fields to variableList.
        public virtual void addResultsVars() {
            if (!this.resultsLayerExists()) {
                return;
            }
            var newVars = new List<string>();
            Debug.Assert(this.currentResultsLayer is not null);
            var fields = (this.currentResultsLayer.GetDefinition() as CIMBasicFeatureLayer).FeatureTable.FieldDescriptions;
            //var indexes = fields.allAttributesList();
            //foreach (var i in indexes) {
            //    if (fields.fieldOrigin(i) == QgsFields.OriginEdit) {
            //        // added by editing
            //        newVars.Add(fields.at(i).name());
            //    }
            //}
            // TODO check if field added by editing
            foreach (int i in Enumerable.Range(0, fields.Length)) {
                newVars.Add(fields[i].FieldName);
            }
            foreach (var vari in newVars) {
                var i = this._dlg.PvariableList.FindString(vari);
                if (i == System.Windows.Forms.ListBox.NoMatches) {
                    // add vari to variableList
                    this._dlg.PvariableList.Items.Add(vari);
                }
            }
        }
        
        // Return true if current results layer is not null and its map is not null.
        public virtual bool resultsLayerExists() {
            if (this.currentResultsLayer is null) {
                return false;
            }
            if (this.currentResultsLayer.Map is null) {
                return false;
            }
            return true;
        }

        // 
        //         Create animation with new shapefile or existing one.
        //         
        //         Assumes:
        //         - animation variable is set
        //         
        public virtual async Task<bool> createAnimationLayer() {
            var (@base, ft) = this.selectBase();
            if (@base is null) {
                return false;
            }
            var resultsBase = Utils.join(this._gv.resultsDir, @base) + ".shp";
            var animateFileBase = Utils.join(this._gv.animationDir, @base) + ".shp";
            int num = 0;
            var animateFile = Utils.nextFileName(animateFileBase, ref num);
            int animateIndex = 0;
            Utils.copyShapefile(resultsBase, @base + num.ToString(), this._gv.animationDir);
            if (string.IsNullOrEmpty(this.stillFileBase)) {
                this.stillFileBase = Utils.join(this._gv.pngDir, Parameters._STILLPNG);
            }
            this.currentStillNumber = 0;
            using (var animateDs = Ogr.Open(animateFile, 1)) {
                //var animateLayer = QgsVectorLayer(animateFile, "{0} {1}", this.scenario, this.animateVar), "ogr");
                var animateLayer = animateDs.GetLayerByIndex(0);
                var defn = animateLayer.GetLayerDefn();
                //var provider = animateLayer.dataProvider();
                var field = new OSGeo.OGR.FieldDefn(this.animateVar, OSGeo.OGR.FieldType.OFTReal);
                // TODO this gives wrong number because Id field added later (where?)
                animateIndex = animateLayer.CreateField(field, 1);
                //if (!provider.addAttributes(new List<object> {
                //    field
                //})) {
                //    Utils.error("Could not add field {0} to animation file {1}", this.animateVar, animateFile), this._gv.isBatch);
                //    return false;
                //}
                //animateLayer.updateFields();
                //var animateIndex = this._gv.topo.getProviderIndex(provider, this.animateVar);
            }
            //// place layer at top of animation group if new,
            //// else above current animation layer, and remove that
            //var animationGroup = Utils.getGroupLayerByName(Utils._ANIMATION_GROUP_NAME);
            //Debug.Assert(animationGroup is not null);
            //var index = 0;
            //if (this._dlg.PcurrentAnimation.Checked) {
            //    var animations = animationGroup.Layers;
            //    if (animations.Count == 1) {
            //        await QueuedTask.Run(() => MapView.Active.Map.RemoveLayer(animations[0]));
            //        index = 0;
            //    } else {
            //        var currentLayers = MapView.Active.GetSelectedLayers();
            //        if (currentLayers.Count > 0) {
            //            var currentLayerName = currentLayers[0].Name;
            //            foreach (var i in Enumerable.Range(0, animations.Count)) {
            //                if (animations[i].Name == currentLayerName) {
            //                    index = i;
            //                    await QueuedTask.Run(() => MapView.Active.Map.RemoveLayer(animations[i]));
            //                    break;
            //                }
            //            }
            //        }
            //    }
            //}

            // create a map to hold animate layer and background maps
            this.animateMap = null;
            await QueuedTask.Run(() =>
                this.animateMap = MapFactory.Instance.CreateMap(string.Format("Animate {0} {1}", this.scenario, this.animateVar), 
                    MapType.Map, MapViewingMode.Map, Basemap.None)
            );
            Layer backLayer = null;
            var subs1File = Utils.join(this._gv.shapesDir, Parameters._SUBS1 + ".shp");
            if (File.Exists(subs1File)) {
                backLayer = await QueuedTask.Run(() =>
                    LayerFactory.Instance.CreateLayer(new Uri(subs1File), this.animateMap, 0, Utils._SUBBASINSLEGEND)
                    );
                ft = FileTypes._SUBBASINS;
                await FileTypes.ApplySymbolToFeatureLayerAsync((FeatureLayer)backLayer, ft, this._gv);
                await Utils.setMapTip((FeatureLayer)backLayer, ft, this._gv);
            }
            this.animateLayer = await QueuedTask.Run(() => LayerFactory.Instance.CreateLayer(new Uri(animateFile),
                this.animateMap, 0, string.Format("{0} {1}", this.scenario, this.animateVar))) as FeatureLayer;
            //Debug.Assert(this.animateLayer is not null);
            this.animateIndexes[this.animateLayer.Name] = animateIndex;
            this.animateVars[this.animateLayer.Name] = this.animateVar;
            // add labels if based on subbasins
            // TODO
            //if (this.useSubs()) {
            //    this.animateLayer.loadNamedStyle(Utils.join(this._gv.plugin_dir, "subsresults.qml"));
            //}
            return true;
        }

        // Colour animation layer.
        //         
        //         Assumes allAnimateVals is suitably populated.
        //         
        public virtual async Task colourAnimationLayer() {
            (string @base, _) = this.selectBase();
            if (@base is null) {
                return;
            }
            double transparency = @base == Parameters._RIVS ? 0 : 35;
            var ramp = await this.chooseColorRamp(this.table, this.animateVar);
            var renderer = await this.makeJenksRenderer(this.allAnimateVals, ramp, this.animateVar);
            Debug.Assert(this.animateLayer is not null);
            await QueuedTask.Run(() => {
                if (@base == Parameters._RIVS) {
                    CIMVisualVariable[] visualVars = new CIMVisualVariable[1];
                    var expressionInfo = new CIMExpressionInfo() {
                        Expression = "$feature.PenWidth",
                        Title = "Custom"
                    };
                    var sizeVar = new CIMSizeVisualVariable() {
                        ValueExpressionInfo = expressionInfo,
                        VariableType = SizeVisualVariableType.Expression,
                        MaxSize = 0,
                        MinValue = 1,
                        MaxValue = 8
                    };
                    visualVars[0] = sizeVar;
                    renderer.VisualVariables = visualVars;
                }
                this.animateLayer.SetRenderer(renderer);
                this.animateLayer.SetTransparency(transparency);
                this.setMapTitle(this.animateLayer);
            });
        }

        //TODO
        //// Create print composer to capture each animation step.
        //public virtual object createAnimationComposition() {
        //    object height;
        //    object width;
        //    object templ;
        //    var proj = QgsProject.instance();
        //    var root = proj.layerTreeRoot();
        //    var animationLayers = Utils.getLayersInGroup(Utils._ANIMATION_GROUP_NAME, root);
        //    var watershedLayers = Utils.getLayersInGroup(Utils._WATERSHED_GROUP_NAME, root, visible: true);
        //    // choose template file and set its width and height
        //    // width and height here need to be updated if template file is changed
        //    var count = this._dlg.composeCount.Value;
        //    var isLandscape = this._dlg.composeLandscape.Checked;
        //    if (count == 1) {
        //        if (isLandscape) {
        //            templ = "1Landscape.pagx";
        //            width = 230.0;
        //            height = 160.0;
        //        } else {
        //            templ = "1Portrait.pagx";
        //            width = 190.0;
        //            height = 200.0;
        //        }
        //    } else if (count == 2) {
        //        if (isLandscape) {
        //            templ = "2Landscape.pagx";
        //            width = 125.0;
        //            height = 120.0;
        //        } else {
        //            templ = "2Portrait.pagx";
        //            width = 150.0;
        //            height = 120.0;
        //        }
        //    } else if (count == 3) {
        //        if (isLandscape) {
        //            templ = "3Landscape.pagx";
        //            width = 90.0;
        //            height = 110.0;
        //        } else {
        //            templ = "3Portrait.pagx";
        //            width = 150.0;
        //            height = 80.0;
        //        }
        //    } else if (count == 4) {
        //        if (isLandscape) {
        //            templ = "4Landscape.pagx";
        //            width = 95.0;
        //            height = 80.0;
        //        } else {
        //            templ = "4Portrait.pagx";
        //            width = 85.0;
        //            height = 85.0;
        //        }
        //    } else if (count == 6) {
        //        if (isLandscape) {
        //            templ = "6Landscape.pagx";
        //            width = 90.0;
        //            height = 40.0;
        //        } else {
        //            templ = "6Portrait.pagx";
        //            width = 55.0;
        //            height = 80.0;
        //        }
        //    } else {
        //        Utils.error("There are composition templates only for 1, 2, 3, 4 or 6 result maps, not for {0}", count), this._gv.isBatch);
        //        return;
        //    }
        //    var templateIn = Utils.join(this._gv.plugin_dir, "PrintTemplate" + templ);
        //    this.animationTemplate = Utils.join(this._gv.tablesOutDir, "AnimationTemplate.pagx");
        //    // make substitution table
        //    var subs = new dict();
        //    var northArrow = Utils.join(os.getenv("OSGEO4W_ROOT"), Visualise._NORTHARROW);
        //    if (!File.Exists(northArrow)) {
        //        // may be qgis-ltr for example
        //        var northArrowRel = Visualise._NORTHARROW.replace("qgis", Utils.qgisName(), 1);
        //        northArrow = Utils.join(os.getenv("OSGEO4W_ROOT"), northArrowRel);
        //    }
        //    if (!File.Exists(northArrow)) {
        //        Utils.error("Failed to find north arrow {0}.  You will need to repair the layout.", northArrow), this._gv.isBatch);
        //    }
        //    subs["%%NorthArrow%%"] = northArrow;
        //    subs["%%ProjectName%%"] = this.title;
        //    var numLayers = animationLayers.Count;
        //    if (count > numLayers) {
        //        Utils.error("You want to make a print of {0} maps, but you only have {1} animation layers", count, numLayers), this._gv.isBatch);
        //        return;
        //    }
        //    var extent = this._iface.mapCanvas().extent();
        //    var xmax = extent.xMaximum();
        //    var xmin = extent.xMinimum();
        //    var ymin = extent.yMinimum();
        //    var ymax = extent.yMaximum();
        //    Utils.loginfo("Map canvas extent {0}, {1}, {2}, {3}", round(xmin).ToString(), round(ymin).ToString(), round(xmax).ToString(), round(ymax).ToString()));
        //    // need to expand either x or y extent to fit map shape
        //    var xdiff = (ymax - ymin) / height * width - (xmax - xmin);
        //    if (xdiff > 0) {
        //        // need to expand x extent
        //        xmin = xmin - xdiff / 2;
        //        xmax = xmax + xdiff / 2;
        //    } else {
        //        // expand y extent
        //        var ydiff = (xmax - xmin) / width * height - (ymax - ymin);
        //        ymin = ymin - ydiff / 2;
        //        ymax = ymax + ydiff / 2;
        //    }
        //    Utils.loginfo("Map extent set to {0}, {1}, {2}, {3}", round(xmin).ToString(), round(ymin).ToString(), round(xmax).ToString(), round(ymax).ToString()));
        //    // estimate of segment size for scale
        //    // aim is approx 10mm for 1 segment
        //    // we make size a power of 10 so that segments are 1km, or 10, or 100, etc.
        //    var segSize = Math.Pow(10, round(math.log10((xmax - xmin) / (width / 10))));
        //    var layerStr = "<Layer source=\"{0}\" provider=\"ogr\" name=\"{1}\">{2}</Layer>";
        //    foreach (var i in Enumerable.Range(0, count)) {
        //        var layer = animationLayers[i].layer();
        //        Debug.Assert(layer is not null);
        //        subs["%%LayerId{0}%%", i)] = layer.id();
        //        subs["%%LayerName{0}%%", i)] = layer.name();
        //        subs["%%YMin{0}%%", i)] = ymin.ToString();
        //        subs["%%XMin{0}%%", i)] = xmin.ToString();
        //        subs["%%YMax{0}%%", i)] = ymax.ToString();
        //        subs["%%XMax{0}%%", i)] = xmax.ToString();
        //        subs["%%ScaleSegSize%%"] = segSize.ToString();
        //        subs["%%Layer{0}%%", i)] = layerStr, Utils.layerFilename(layer), layer.name(), layer.id());
        //    }
        //    foreach (var i in Enumerable.Range(0, 6)) {
        //        // 6 entries in template for background layers
        //        if (i < watershedLayers.Count) {
        //            var wLayer = watershedLayers[i].layer();
        //            Debug.Assert(wLayer is not null);
        //            subs["%%WshedLayer{0}%%", i)] = layerStr, Utils.layerFilename(wLayer), wLayer.name(), wLayer.id());
        //        } else {
        //            // remove unused ones
        //            subs["%%WshedLayer{0}%%", i)] = "";
        //        }
        //    }
        //    // seems to do no harm to leave unused <Layer> items with original pattern, so we don't bother removing them
        //    using (var inFile = open(templateIn, "rU")) {
        //        using (var outFile = open(this.animationTemplate, "w")) {
        //            foreach (var line in inFile) {
        //                outFile.write(Visualise.replaceInLine(line, subs));
        //            }
        //        }
        //    }
        //    Utils.loginfo("Print layout template {0} written", this.animationTemplate));
        //    this.animationDOM = QDomDocument();
        //    var f = QFile(this.animationTemplate);
        //    if (f.open(QIODevice.ReadOnly)) {
        //        var OK = this.animationDOM.setContent(f)[0];
        //        if (!OK) {
        //            Utils.error("Cannot parse template file {0}", this.animationTemplate), this._gv.isBatch);
        //            return;
        //        }
        //    } else {
        //        Utils.error("Cannot open template file {0}", this.animationTemplate), this._gv.isBatch);
        //        return;
        //    }
        //    if (!this._gv.isBatch) {
        //        Utils.information(@"
        //    The layout designer is about to start, showing the current layout for the animation.

        //    You can change the layout as you wish, and then you should 'Save as Template' in the designer menu, using {0} as the template file.  
        //    If this file already exists: you will have to confirm overwriting it.
        //    Then close the layout designer.
        //    If you don't change anything you can simply close the layout designer without saving.

        //    Then start the animation running.
        //    ", this.animationTemplate), false);
        //        var title = "Animation base";
        //        // remove layout from layout manager, in case still there
        //        try {
        //            Debug.Assert(this.animationLayout is not null);
        //            proj.layoutManager().removeLayout(this.animationLayout);
        //        } catch {
        //        }
        //        this.animationLayout = null;
        //        this.animationLayout = QgsPrintLayout(proj);
        //        this.animationLayout.initializeDefaults();
        //        this.animationLayout.setName(title);
        //        this.setDateInTemplate();
        //        var items = this.animationLayout.loadFromTemplate(this.animationDOM, QgsReadWriteContext());
        //        var ok = proj.layoutManager().addLayout(this.animationLayout);
        //        if (!ok) {
        //            Utils.error("Failed to add animation layout to layout manager.  Try removing some.", this._gv.isBatch);
        //            return;
        //        }
        //        var designer = this._gv.iface.openLayoutDesigner(layout: this.animationLayout);
        //        this.animationTemplateDirty = true;
        //    }
        //}

        // TODO
        //// Reread animation template file.
        //public virtual object rereadAnimationTemplate() {
        //    this.animationTemplateDirty = false;
        //    this.animationDOM = QDomDocument();
        //    var f = QFile(this.animationTemplate);
        //    if (f.open(QIODevice.ReadOnly)) {
        //        var OK = this.animationDOM.setContent(f)[0];
        //        if (!OK) {
        //            Utils.error("Cannot parse template file {0}", this.animationTemplate), this._gv.isBatch);
        //            return;
        //        }
        //    } else {
        //        Utils.error("Cannot open template file {0}", this.animationTemplate), this._gv.isBatch);
        //        return;
        //    }
        //}

        // TODO
        //// Set current animation date in title field.
        //public virtual object setDateInTemplate() {
        //    Debug.Assert(this.animationDOM is not null);
        //    var itms = this.animationDOM.elementsByTagName("LayoutItem");
        //    foreach (var i in Enumerable.Range(0, itms.length())) {
        //        var itm = itms.item(i);
        //        var attr = itm.attributes().namedItem("id").toAttr();
        //        if (attr is not null && attr.Value == "Date") {
        //            var title = itm.attributes().namedItem("labelText").toAttr();
        //            if (title is null) {
        //                Utils.error("Cannot find template date label", this._gv.isBatch);
        //                return;
        //            }
        //            title.setValue(this._dlg.dateLabel.Text);
        //            return;
        //        }
        //    }
        //    Utils.error("Cannot find template date label", this._gv.isBatch);
        //    return;
        //}

        // 
        //         Display animation data for current slider value.
        //         
        //         Get date from slider value; read animation data for date; write to animation file; redisplay.
        //         
        public virtual async Task changeAnimate() {
            string @ref;
            int unit;
            string fieldName;
            string @base;
            try {
                if (this._dlg.PanimationVariableCombo.SelectedItem.ToString() == "") {
                    Utils.information("Please choose an animation variable", this._gv.isBatch);
                    this.doRewind();
                    return;
                }
                if (this.animateMap != MapView.Active.Map) {
                    await this.animateMap.OpenViewAsync();
                }
                var view = MapView.Active;
                var extent = view.Extent;
                int height = (int)Math.Ceiling(extent.YMax - extent.YMin + 0.5);
                int width = (int)Math.Ceiling(extent.XMax - extent.XMin + 0.5);
                if (this.capturing) {
                    await this.capture(height, width);
                }
                var dat = this.sliderValToDate(this._dlg.Pslider.Value);
                var date = this.dateToString(dat);
                this._dlg.PdateLabel.Text = date;
                var layerName = this.animateLayer.Name;
                var animateFile = await Utils.layerFilename(this.animateLayer);
                var animateIndex = this.animateIndexes[layerName];
                var animateVar = this.animateVars[layerName];
                var animateVarShort = animateVar.Substring(0, Math.Min(10, animateVar.Length));
                var data = this.animateResultsData[layerName][dat];
                await QueuedTask.Run(() => {
                    this.setMapTitle(this.animateLayer, line2: date);
                });
                await QueuedTask.Run(async () => {
                    if (!animateLayer.CanEditData()) {
                        animateLayer.SetEditable(true);
                        if (!animateLayer.CanEditData()) {
                            Utils.error("Cannot edit animation layer " + layerName, false);
                            return;
                        }
                    }
                    var path = await Utils.layerFilename(animateLayer);
                    var fileName = Path.GetFileName(path);
                    if (fileName.StartsWith(Parameters._HRUS)) {
                        @base = Parameters._HRUS;
                        fieldName = Topology._HRUS;
                    } else if (fileName.StartsWith(Parameters._LSUS)) {
                        @base = Parameters._LSUS;
                        fieldName = Topology._LSUID;
                    } else if (fileName.StartsWith(Parameters._SUBS)) {
                        @base = Parameters._SUBS;
                        fieldName = Topology._SUBBASIN;
                    } else if (fileName.StartsWith(Parameters._RIVS)) {
                        @base = Parameters._RIVS;
                        fieldName = Topology._CHANNEL;
                    } else if (fileName.StartsWith(Parameters._AQUIFERS)) {
                        @base = Parameters._AQUIFERS;
                        fieldName = Topology._AQUIFER;
                    } else if (fileName.StartsWith(Parameters._DEEPAQUIFERS)) {
                        @base = Parameters._DEEPAQUIFERS;
                        fieldName = Topology._AQUIFER;
                    } else {
                        return;
                    }
                    var animateIndex = this.animateIndexes[layerName];
                    var layerDefn = animateLayer.GetFeatureClass().GetDefinition();
                    var unitIdx = layerDefn.FindField(fieldName);
                    if (unitIdx < 0) {
                        Utils.error(string.Format("Cannot find {0} field in {1}", fieldName, path), this._gv.isBatch);
                        this.animating = false;
                        return;
                    }
                    using (var rc = this.animateLayer.Search()) {
                        while (rc.MoveNext()) {
                            using (var f = rc.Current as ArcGIS.Core.Data.Feature) {
                                bool result = false;
                                var op = new ArcGIS.Desktop.Editing.EditOperation();
                                if (@base == Parameters._HRUS) {
                                    // May be split HRUs; just use first
                                    // This is inadequate for some variables, but no way to know if correct val is sum of vals, mean, etc.
                                    var unitText = f[unitIdx].ToString();
                                    if (unitText.Contains("PND")) {
                                        continue;
                                    }
                                    unit = Convert.ToInt32(unitText.Split(",")[0]);
                                } else {
                                    unit = Convert.ToInt32(f[unitIdx]);
                                }
                                double val;
                                if (data.ContainsKey(unit)) {
                                    val = data[unit];
                                } else {
                                    if (@base == Parameters._HRUS) {
                                        @ref = $"HRU {unit}";
                                    } else if (@base == Parameters._LSUS) {
                                        @ref = $"LSU {unit}";
                                    } else if (@base == Parameters._SUBS) {
                                        @ref = $"subbasin {unit}";
                                    } else {
                                        @ref = $"channel {unit}";
                                    }
                                    Utils.error(string.Format("Cannot get data for {0}: have you run SWAT+ and saved data since running QSWAT+?", @ref), this._gv.isBatch);
                                    this.animating = false;
                                    return;
                                }
                                op.Modify(f, animateVarShort, val);
                                if (!op.IsEmpty) {
                                    result = await op.ExecuteAsync();
                                }
                                if (!result) {
                                    Utils.error(string.Format("Could not set attributes in results file {0}", this.resultsFile), this._gv.isBatch);
                                    this.animating = false;
                                    return;
                                }
                            }
                        }
                    }
                });
                await Project.Current.SaveEditsAsync();
                await QueuedTask.Run(() => {
                    MapView.Active.Redraw(false);
                });
                this._dlg.PdateLabel.Refresh();
            } catch (Exception) {
                this.animating = false;
                throw;
            }
        }

        // Make image file of current canvas.
        public virtual async Task capture(int height, int width) {
            if (this.animateLayer is null) {
                return;
            }
            //await QueuedTask.Run(() => MapView.Active.Redraw(false));
            this.currentStillNumber += 1;
            var @base = Path.ChangeExtension(this.stillFileBase, null);
            var suffix = Path.GetExtension(this.stillFileBase);
            var nextStillFile = @base + string.Format("{0:D5}", this.currentStillNumber) + suffix;
            // this does not capture the title
            //self._iface.mapCanvas().saveAsImage(nextStillFile)
            var composingAnimation = this._dlg.PprintAnimation.Checked;
            if (composingAnimation) {
                // remove layout if any
                // TODO
                //try {
                //    Debug.Assert(this.animationLayout is not null);
                //    proj.layoutManager().removeLayout(this.animationLayout);
                //} catch {
                //}
                //this.animationLayout = null;
                //if (this.animationTemplateDirty) {
                //    this.rereadAnimationTemplate();
                //}
                //var title = "Animation {0}", this.compositionCount);
                //this.compositionCount += 1;
                //this.animationLayout = QgsPrintLayout(proj);
                //this.animationLayout.initializeDefaults();
                //this.animationLayout.setName(title);
                //this.setDateInTemplate();
                //Debug.Assert(this.animationDOM is not null);
                //var _ = this.animationLayout.loadFromTemplate(this.animationDOM, QgsReadWriteContext());
                //var ok = proj.layoutManager().addLayout(this.animationLayout);
                //if (!ok) {
                //    Utils.error("Failed to add animation layout to layout manager.  Try removing some.", this._gv.isBatch);
                //    return;
                //}
                //var exporter = QgsLayoutExporter(this.animationLayout);
                //var settings = QgsLayoutExporter.ImageExportSettings();
                //settings.exportMetadata = false;
                //var res = exporter.exportToImage(nextStillFile, settings);
                //if (res != QgsLayoutExporter.Success) {
                //    Utils.error("Failed with result {1} to save layout as image file {0}", nextStillFile, res), this._gv.isBatch);
                //}
            } else {
                // tempting bot omits canvas title
                // canvas.saveAsImage(nextStillFile)
                await QueuedTask.Run(() => {
                    var PNG = new PNGFormat();
                    PNG.Resolution = 96;
                    PNG.Width = Convert.ToInt32(width / 14.65);
                    PNG.Height = Convert.ToInt32(height / 14.65);
                    PNG.OutputFileName = nextStillFile;
                    if (PNG.ValidateOutputFilePath()) {
                        MapView.Active.Export(PNG);
                    } else {
                        Utils.error(string.Format("Failed to save canvas as image file {0}", nextStillFile), this._gv.isBatch);
                    }
                });
            }
        }

        // Create a range from minv to maxv with the colour.
        public virtual CIMClassBreak makeSymbologyForRange(double minv, double maxv, CIMColor colour, int precision) {
            string title;
            CIMSymbolReference symbolRef;
            (string @base, _) = this.selectBase();
            if (@base == Parameters._RIVS) {
                var symbol = SymbolFactory.Instance.ConstructLineSymbol(colour);
                symbolRef = symbol.MakeSymbolReference();
            } else {
                var symbol = SymbolFactory.Instance.ConstructPolygonSymbol(colour);
                symbolRef = symbol.MakeSymbolReference();
            }
            if (precision > 0) {
                title = string.Format("{0:G" + precision.ToString() + "} - {1:G" + precision.ToString() + "}", minv, maxv);
            } else {
                var factor = Convert.ToInt32(Math.Pow(10, Math.Abs(precision)));
                var minv1 = Math.Round((double)minv / factor) * factor;
                var maxv1 = Math.Round((double)maxv / factor) * factor;
                title = string.Format("{0} - {1}", minv1, maxv1);
            }
            var brk = new CIMClassBreak() {
                UpperBound = maxv,
                Label = title,
                Symbol = symbolRef
            };
            return brk;
        }
        
        // Select a colour ramp.
        public virtual async Task<CIMColorRamp> chooseColorRamp(string table, string vari) {
            CIMColorRamp ramp; 
            var chaWater = new List<string> {
                "flo_in",
                "flo_out",
                "evap",
                "tloss",
                "aqu_in",
                "peakr"
            };
            var aqWater = new List<string> {
                "flo",
                "stor",
                "rchrg",
                "seep",
                "revapflo_cha",
                "flo_res",
                "flo_ls"
            };
            var wbWater = new List<string> {
                "snowmlt",
                "sq_gen",
                "latq",
                "wtryld",
                "perc",
                "tloss",
                "sq_cont",
                "sw",
                "qtile",
                "irr",
                "sq_run",
                "lq_run",
                "ovbank",
                "surqcha",
                "surqres",
                "surq_ls",
                "latq_cha",
                "latq_res",
                "latq_ls"
            };
            var wbPrecip = new List<string> {
                "prec",
                "snow",
                "et",
                "eplant",
                "esoil",
                "pet"
            };
            if (table.StartsWith("channel") && !chaWater.Contains(vari) || (table.StartsWith("aquifer") || table.StartsWith("deep_aquifer")) && !aqWater.Contains(vari) || table.Contains("_ls_") || table.Contains("_nb_") || table.Contains("_wb_") && !wbWater.Contains(vari) && !wbPrecip.Contains(vari)) {
                // sediments and pollutants
                // Condition Number is the reversal of Red-Yellow-Green
                ramp = await FileTypes.SWATRamp("RdYlGn");
                return ramp;
            } else if (table.StartsWith("channel") && chaWater.Contains(vari) || (table.StartsWith("aquifer") || table.StartsWith("deep_aquifer")) && aqWater.Contains(vari) || table.Contains("_wb_") && wbWater.Contains(vari)) {
                // water
                ramp = await FileTypes.SWATRamp("YlGnBu");
                return ramp;
            } else if (table.Contains("_wb_") && wbPrecip.Contains(vari)) {
                // precipitation and transpiration:
                ramp = await FileTypes.SWATRamp("GnBu");
                return ramp;
            } else {
                ramp = await FileTypes.SWATRamp("YlOrRd");
                return ramp;
            }
        }

        //// Main tab has changed.  Show/hide Animation group.
        //public virtual async Task modeChange() {
        //    var expandAnimation = this._dlg.PtabWidget.SelectedIndex == 1;
        //    var animationGroup = Utils.getGroupLayerByName(Utils._ANIMATION_GROUP_NAME);
        //    Debug.Assert(animationGroup is not null);
        //    await QueuedTask.Run(() => {
        //        animationGroup.SetVisibility(expandAnimation);
        //    });
        //}

        // Make single results file or comparison results files if self.scenarios1 is not empty.
        public virtual async Task makeResults0() {
            if (this.scenario1 == "") {
                await this.makeResults();
                return;
            }
            var currentScenario = this.scenario;
            this.scenario = this.scenario1;
            var scenDir = Utils.join(this._gv.scenariosDir, this.scenario);
            var outDir = Utils.join(scenDir, Parameters._RESULTS);
            var outFile = Utils.join(outDir, this.table + "results.shp");
            this._dlg.PresultsFileEdit.Text = outFile;
            this.setConnection(this.scenario);
            await this.makeResults();
            this.scenario = this.scenario2;
            scenDir = Utils.join(this._gv.scenariosDir, this.scenario);
            outDir = Utils.join(scenDir, Parameters._RESULTS);
            outFile = Utils.join(outDir, this.table + "results.shp");
            this._dlg.PresultsFileEdit.Text = outFile;
            this.setConnection(this.scenario);
            await this.makeResults();
            // restore current scenario and results shapefile
            this.scenario = currentScenario;
            scenDir = Utils.join(this._gv.scenariosDir, this.scenario);
            outDir = Utils.join(scenDir, Parameters._RESULTS);
            outFile = Utils.join(outDir, this.table + "results.shp");
            this._dlg.PresultsFileEdit.Text = outFile;
            this.setConnection(this.scenario);
        }

        // 
        //         Create a results file and display.
        //         
        //         Only creates a new file if the variables have changed.
        //         If variables unchanged, only makes and writes summary data if necessary.
        //         
        public virtual async Task makeResults() {
            if (this.table == "") {
                Utils.information("Please choose a SWAT output table", this._gv.isBatch);
                return;
            }
            if (this._dlg.PresultsFileEdit.Text == "") {
                Utils.information("Please choose a results file", this._gv.isBatch);
                return;
            }
            if (this._dlg.PvariableList.Items.Count == 0) {
                Utils.information("Please choose some variables", this._gv.isBatch);
                return;
            }
            if (this._dlg.PvariableList.SelectedItems.Count == 0) {
                Utils.information("Please select a variable for display", this._gv.isBatch);
                return;
            }
            if (!this.isAA && this._dlg.PsummaryCombo.SelectedItem == null) {
                Utils.information("Please choose a summary", this._gv.isBatch);
                return;
            }
            if (!this.setPeriods()) {
                return;
            }
            if (this.scenario1 != "") {
                var _ = await this.makeCompareResults();
                return;
            }
            Cursor.Current = Cursors.WaitCursor;
            this.resultsFileUpToDate = this.resultsFileUpToDate && this.resultsFile == this._dlg.PresultsFileEdit.Text;
            if (!this.resultsFileUpToDate || !this.periodsUpToDate) {
                if (!this.readData("", true, this.table, "", "")) {
                    return;
                }
                this.periodsUpToDate = true;
            }
            if (this.summaryChanged) {
                this.summariseData("", true);
            }
            if (this.resultsFileUpToDate && this.resultsLayerExists() && this._dlg.PcurrentResultsMap.Checked) {
                Map map = MapView.Active.Map;
                if (map == ArcSWATPlus.mainMap) {
                    Utils.error("The current map for showing results cannot be the main map: choose a results map or click New map", this._gv.isBatch);
                    Cursor.Current = Cursors.Default;
                    return;
                }
                var selectVar = this._dlg.PvariableList.SelectedItems[0].ToString();
                var legend = string.Format("{0} {1} {2}", this.scenario, selectVar, this._dlg.PsummaryCombo.SelectedItem.ToString());
                await QueuedTask.Run(() => map.SetName(legend));
                if (this.summaryChanged) {
                    await this.updateResultsFile();
                }
            } else if (await this.createResultsFile()) {
                this.resultsFileUpToDate = true;
            } else {
                return;
            }
            await this.colourResultsFile();
            Cursor.Current = Cursors.Default;
        }
        
        // Create and add to results 4 shapefiles:
        //         1.  Results for scenario 1
        //         2.  Results for scenario 2
        //         3.  Results for difference scenario 2 - scenario 1
        //         4.  Results for % change from 1 to 2
        //         Assumes self.scenario1 and self.scenario2 are different existing scenarios directories.
        public virtual async Task<bool> makeCompareResults() {
            FeatureLayer layer4 = null;
            FeatureLayer layer3 = null;
            FeatureLayer layer2 = null;
            FeatureLayer layer1 = null;

            void removeComparisonLayers(List<FeatureLayer> layers) {
                QueuedTask.Run(() => {
                    MapView.Active.Map.RemoveLayers(layers);
                });
            }

            // make a directory for the results files
            var dirname = string.Format("{0}_{1}",this.scenario1, this.scenario2);
            var direc = Utils.join(this._gv.resultsDir, dirname);
            Directory.CreateDirectory(direc);
            var selectVar = this._dlg.PvariableList.SelectedItems[0].ToString();
            var file1Base = this.scenario1 + "-" + this.table + "-" + selectVar + "-results";
            var file1 = Utils.join(direc, file1Base + ".shp");
            var file1Exists = File.Exists(file1);
            var file2Base = this.scenario2 + "-" + this.table + "-" + selectVar + "-results";
            var file2 = Utils.join(direc, file2Base + ".shp");
            var file2Exists = File.Exists(file2);
            var diffBase = this.table + "-" + selectVar + "-diff";
            var diffFile = Utils.join(direc, diffBase + ".shp");
            var diffFileExists = File.Exists(diffFile);
            var changeBase = this.table + "-" + selectVar + "-%change";
            var changeFile = Utils.join(direc, changeBase + ".shp");
            var changeFileExists = File.Exists(changeFile);
            this.setConnection(this.scenario1);
            // set isStatic False so data for only one variable is read
            if (!this.readData("scenario1", false, this.table, selectVar, "")) {
                return false;
            }
            // assume results shapefiles are the same for scenarios 1 and 2, so use the first scenario for the source shapefiles
            var resultsDir = Path.GetDirectoryName(this.db);
            this.setConnection(this.scenario2);
            if (!this.readData("scenario2", false, this.table, selectVar, "")) {
                return false;
            }
            // restore self.db
            this.setConnection(this.scenario);
            var (baseName, ft) = this.selectBase(scenario: this.scenario1);
            if (baseName is null) {
                return false;
            }
            var resultsBase = Utils.join(resultsDir, baseName) + ".shp";
            var summary = this.isAA ? Visualise._ANNUALMEANS : this._dlg.PsummaryCombo.SelectedItem.ToString();
			// create a map to hold the comparison result layers
            Map comparisonMap = null;
            string legend = string.Format("Comparison {0}", dirname);
            await QueuedTask.Run(() =>
                comparisonMap = MapFactory.Instance.CreateMap(legend, MapType.Map, MapViewingMode.Map, Basemap.None)
            );
            var legend1 = string.Format("{0} {1} {2}", this.scenario1, selectVar, summary);
            var needLayer1 = true;
            if (file1Exists) {
                layer1 = await QueuedTask.Run(() =>
                    LayerFactory.Instance.CreateLayer(new Uri(file1), comparisonMap, 0, legend1) as FeatureLayer);
                if (layer1 is not null) {
                    needLayer1 = false;
                }
            } else {
                Utils.copyShapefile(resultsBase, file1Base, direc);
            }
            var legend2 = string.Format("{0} {1} {2}", this.scenario2, selectVar, summary);
            var needLayer2 = true;
            if (file2Exists) {
                layer2 = await QueuedTask.Run(() =>
                    LayerFactory.Instance.CreateLayer(new Uri(file2), comparisonMap, 0, legend2) as FeatureLayer);
                if (layer2 is not null) {
                    needLayer2 = false;
                }
            } else {
                Utils.copyShapefile(resultsBase, file2Base, direc);
            }
            // note that string 'difference' is used to find variable name in MapTitle, so change there if you change here
            var legend3 = string.Format("{0} {1} {2} {3} {4}", selectVar, "difference", this.scenario2, "minus", this.scenario1);
            var needLayer3 = true;
            if (diffFileExists) {
                layer3 = await QueuedTask.Run(() => 
                    LayerFactory.Instance.CreateLayer(new Uri(diffFile), comparisonMap, 0, legend3) as FeatureLayer);
                if (layer3 is not null) {
                    needLayer3 = false;
                }
            } else {
                Utils.copyShapefile(resultsBase, diffBase, direc);
            }
            // note that string '%change' is used to find variable name in MapTitle, so change there if you change here
            var legend4 = string.Format("{0} {1} {2} {3} {4}", selectVar, "%change from", this.scenario1, "to", this.scenario2);
            var needLayer4 = true;
            if (changeFileExists) {
                layer4 = await QueuedTask.Run(() => 
                    LayerFactory.Instance.CreateLayer(new Uri(changeFile), comparisonMap, 0, legend4) as FeatureLayer);
                if (layer4 is not null) {
                    needLayer4 = false;
                }
            } else {
                Utils.copyShapefile(resultsBase, changeBase, direc);
            }
            //var resultsGroup = Utils.getGroupLayerByName(Utils._RESULTS_GROUP_NAME);
            //Debug.Assert(resultsGroup is not null);
            var selectVarShort = selectVar.Substring(0, Math.Min(selectVar.Length, 10));
            var varField = new OSGeo.OGR.FieldDefn(selectVarShort, OSGeo.OGR.FieldType.OFTReal);
            var addedLayers = new List<FeatureLayer>();
            if (needLayer1) {
                using (var layer1Ds = Ogr.Open(file1, 1)) {
                    var glayer1 = layer1Ds.GetLayerByIndex(0);
                    var defn1 = glayer1.GetLayerDefn();
                    if (defn1.GetFieldIndex(selectVarShort) < 0) {
                        glayer1.CreateField(varField, 1);
                    }
                }
                await QueuedTask.Run(() => {
                    layer1 = LayerFactory.Instance.CreateLayer(new Uri(file1), comparisonMap, 0, legend1) as FeatureLayer;
                });
                addedLayers.Add(layer1);
            }
            if (needLayer2) {
                using (var layer2Ds = Ogr.Open(file2, 1)) {
                    var glayer2 = layer2Ds.GetLayerByIndex(0);
                    var defn2 = glayer2.GetLayerDefn();
                    if (defn2.GetFieldIndex(selectVarShort) < 0) {
                        glayer2.CreateField(varField, 1);
                    }
                }
                await QueuedTask.Run(() => {
                    layer2 = LayerFactory.Instance.CreateLayer(new Uri(file2), comparisonMap, 0, legend2) as FeatureLayer;
                });
                addedLayers.Add(layer2);
            }
            if (needLayer3) {
                using (var layer3Ds = Ogr.Open(diffFile, 1)) {
                    var glayer3 = layer3Ds.GetLayerByIndex(0);
                    var defn3 = glayer3.GetLayerDefn();
                    if (defn3.GetFieldIndex(selectVarShort) < 0) {
                        glayer3.CreateField(varField, 1);
                    }
                }
                await QueuedTask.Run(() => {
                    layer3 = LayerFactory.Instance.CreateLayer(new Uri(diffFile), comparisonMap, 0, legend3) as FeatureLayer;
                });
                addedLayers.Add(layer3);
            }
            if (needLayer4) {
                using (var layer4Ds = Ogr.Open(changeFile, 1)) {
                    var glayer4 = layer4Ds.GetLayerByIndex(0);
                    var defn4 = glayer4.GetLayerDefn();
                    if (defn4.GetFieldIndex(selectVarShort) < 0) {
                        glayer4.CreateField(varField, 1);
                    }
                }
                await QueuedTask.Run(() => {
                    layer4 = LayerFactory.Instance.CreateLayer(new Uri(changeFile), comparisonMap, 0, legend4) as FeatureLayer;
                });
                addedLayers.Add(layer4);
            }
            if (baseName == Parameters._SUBS) {
                //TODO
                //layer1.rendererChanged.connect(this.changeSubRenderer);
                //layer2.rendererChanged.connect(this.changeSubRenderer);
                //layer3.rendererChanged.connect(this.changeSubRenderer);
                //layer4.rendererChanged.connect(this.changeSubRenderer);
                this.internalChangeToSubRenderer = true;
                this.keepSubColours = false;
            } else if (baseName == Parameters._HRUS) {
                //TODO
                //layer1.rendererChanged.connect(this.changeHRURenderer);
                //layer2.rendererChanged.connect(this.changeHRURenderer);
                //layer3.rendererChanged.connect(this.changeHRURenderer);
                //layer4.rendererChanged.connect(this.changeHRURenderer);
                this.internalChangeToHRURenderer = true;
                this.keepHRUColours = false;
            } else if (baseName == Parameters._AQUIFERS) {
                //TODO
                //layer1.rendererChanged.connect(this.changeAquRenderer);
                //layer2.rendererChanged.connect(this.changeAquRenderer);
                //layer3.rendererChanged.connect(this.changeAquRenderer);
                //layer4.rendererChanged.connect(this.changeAquRenderer);
                this.internalChangeToAquRenderer = true;
                this.keepAquColours = false;
            } else if (baseName == Parameters._DEEPAQUIFERS) {
                //TODO
                //layer1.rendererChanged.connect(this.changeDeepAquRenderer);
                //layer2.rendererChanged.connect(this.changeDeepAquRenderer);
                //layer3.rendererChanged.connect(this.changeDeepAquRenderer);
                //layer4.rendererChanged.connect(this.changeDeepAquRenderer);
                this.internalChangeToDeepAquRenderer = true;
                this.keepDeepAquColours = false;
            } else {
                //TODO
                //layer1.rendererChanged.connect(this.changeRivRenderer);
                //layer2.rendererChanged.connect(this.changeRivRenderer);
                //layer3.rendererChanged.connect(this.changeRivRenderer);
                //layer4.rendererChanged.connect(this.changeRivRenderer);
                this.internalChangeToRivRenderer = true;
                this.keepRivColours = false;
            }
            List<double> allVals;
            bool invertRamp34;
            (allVals, invertRamp34) = await this.updateCompareLayers(layer1, layer2, layer3, layer4, selectVar, selectVarShort);
            if (allVals is null) {
                removeComparisonLayers(addedLayers);
                return false;
            }
            this.currentResultsLayer = layer4;
            if (baseName == Parameters._SUBS) {
                // add labels
                // TODO
                //if (!this._gv.useGridModel) {
                //    layer1.loadNamedStyle(Utils.join(this._gv.plugin_dir, "subresults.qml"));
                //    layer2.loadNamedStyle(Utils.join(this._gv.plugin_dir, "subresults.qml"));
                //    layer3.loadNamedStyle(Utils.join(this._gv.plugin_dir, "subresults.qml"));
                //    layer4.loadNamedStyle(Utils.join(this._gv.plugin_dir, "subresults.qml"));
                //}
                this.internalChangeToSubRenderer = false;
            } else if (baseName == Parameters._LSUS) {
                this.internalChangeToLSURenderer = false;
            } else if (baseName == Parameters._HRUS) {
                this.internalChangeToHRURenderer = false;
            } else if (baseName == Parameters._AQUIFERS) {
                this.internalChangeToAquRenderer = false;
            } else if (baseName == Parameters._DEEPAQUIFERS) {
                this.internalChangeToDeepAquRenderer = false;
            } else {
                this.internalChangeToRivRenderer = false;
            }
            // TODO
            //var baseMapTip = FileTypes.mapTip(ft);
            //layer1.setMapTipTemplate(baseMapTip + string.Format("<br/><b>{0}:</b> [% \"{0}\" %]",selectVarShort));
            //layer2.setMapTipTemplate(baseMapTip + string.Format("<br/><b>{0}:</b> [% \"{0}\" %]",selectVarShort));
            //layer3.setMapTipTemplate(baseMapTip + string.Format("<br/><b>{0}:</b> [% \"{0}\" %]",selectVarShort));
            //layer4.setMapTipTemplate(baseMapTip + string.Format("<br/><b>{0}:</b> [% \"{0}\" %]",selectVarShort));
            var ramp12 = await this.chooseColorRamp(this.table, selectVar);
            // need two renderers for two files, else crash when removing layers
            var renderer1 = await this.makeJenksRenderer(allVals, ramp12, selectVar);
            var renderer2 = CIMClassBreaksRenderer.Clone(renderer1) as CIMClassBreaksRenderer;
            var ramp34 = await this.makeComparisonRamp();

            await this.colourResultsFile(layer: layer1, renderer: renderer1);
            await this.colourResultsFile(layer: layer2, renderer: renderer2);
            await this.colourResultsFile(layer: layer3, ramp: ramp34);
            await this.colourResultsFile(layer: layer4, ramp: ramp34);
            if (comparisonMap != MapView.Active.Map) {
                await comparisonMap.OpenViewAsync();
            }
            return true;
        }

        // Return YlwGrnRd colour ramp.
        public async Task<CIMColorRamp> makeComparisonRamp() {
            // this ramp is inverted by SWATRamp
            return await FileTypes.SWATRamp("RdYlGn");
        }

        // Write data to compare layers.  Return list of all values in compared layers, plus flag indicating if colour ramp for diff and %change maps should be inverted.
        public virtual async Task<(List<double>, bool)> updateCompareLayers(
            FeatureLayer layer1,
            FeatureLayer layer2,
            FeatureLayer layer3,
            FeatureLayer layer4,
            string selectVar,
            string selectVarShort) {
            double val2 = 0;
            double val1 = 0;
            string @ref;
            double data = 0;
            Dictionary<string, double> subData;

            string baseToRef(string @base, int unit) {
                if (@base == Parameters._HRUS) {
                    @ref = string.Format("HRU {0}",unit);
                } else if (@base == Parameters._LSUS) {
                    @ref = string.Format("LSU {0}",unit);
                } else if (@base == Parameters._SUBS) {
                    @ref = string.Format("subbasin {0}",unit);
                } else if (@base == Parameters._AQUIFERS) {
                    @ref = string.Format("aquifer {0}",unit);
                } else if (@base == Parameters._DEEPAQUIFERS) {
                    @ref = string.Format("deep aquifer {0}",unit);
                } else {
                    @ref = string.Format("channel {0}",unit);
                }
                return @ref;
            }
            (string @base, _) = this.selectBase();
            if (@base is null) {
                return (null, false);
            }
            // note that all layers are copy of the same template shapefile, so all have the same structure
            // collect resultsData
            this.summariseData("scenario1", true);
            // summariseData puts data in this.staticResultsData, so save this before running again
            // need to copy data, not just a pointer to it
            var data1 = this.copyStaticResultsData();
            this.summariseData("scenario2", true);
            var allVals = new List<double>();
            var minDiff = double.PositiveInfinity;
            var maxDiff = double.NegativeInfinity;
            foreach (var layer in new HashSet<FeatureLayer> {
                layer1,
                layer2,
                layer3,
                layer4
            }) {
                Debug.Assert(layer is not null);
                var fileName = await Utils.layerFilename(layer);
                using (var ds = Ogr.Open(fileName, 1)) {
                    var glayer = ds.GetLayerByIndex(0);
                    var defn = glayer.GetLayerDefn();
                    var varIndex = defn.GetFieldIndex(selectVarShort);
                    int unit;
                    do {
                        var f = glayer.GetNextFeature();
                        if (f is null) { break; }
                        if (@base == Parameters._HRUS) {
                        // May be split HRUs; just use first
                        // This is inadequate for some variables, but no way to know if correct val is sum of vals, mean, etc.
                        var unitText = f.GetFieldAsString(Topology._HRUS);
                        if (unitText.Contains("PND") || unitText.Contains("RES")) {
                            continue;
                        }
                        unit = Convert.ToInt32(unitText.Split(",")[0]);
                    } else if (@base == Parameters._LSUS) {
                        unit = f.GetFieldAsInteger(Topology._LSUID);
                    } else if (@base == Parameters._SUBS) {
                        unit = f.GetFieldAsInteger(Topology._SUBBASIN);
                    } else if (@base == Parameters._AQUIFERS || @base == Parameters._DEEPAQUIFERS) {
                        unit = f.GetFieldAsInteger(Topology._AQUIFER);
                    } else {
                        unit = f.GetFieldAsInteger(Topology._CHANNEL);
                    }
                        if (layer == layer1) {
                            bool ok = true;
                            if (!data1.TryGetValue(unit, out subData)) {
                                ok = false;
                            } else if (!subData.TryGetValue(selectVar, out data)) {
                                ok = false;
                            }
                            if (!ok) {
                                @ref = baseToRef(@base, unit);
                                Utils.error(string.Format("Cannot get data for variable {0} in {1} in first scenario", selectVar, @ref), this._gv.isBatch);
                                return (null, false);
                            }
                            allVals.Add(data);
                        } else if (layer == layer2) {
                            bool ok = true;
                            if (!this.staticResultsData.TryGetValue(unit, out subData)) {
                                ok = false;
                            } else if (!subData.TryGetValue(selectVar, out data)) {
                                ok = false;
                            }
                            if (!ok) {
                                @ref = baseToRef(@base, unit);
                                Utils.error(string.Format("Cannot get data for variable {0} in {1} in second scenario", selectVar, @ref), this._gv.isBatch);
                                return (null, false);
                            }
                            allVals.Add(data);
                        } else {
                            bool ok = true;
                            if (!data1.TryGetValue(unit, out subData)) {
                                ok = false;
                            } else if (!subData.TryGetValue(selectVar, out val1)) {
                                ok = false;
                            }
                            if (!ok) {
                                @ref = baseToRef(@base, unit);
                                Utils.error(string.Format("Cannot get data for variable {0} in {1} in first scenario", selectVar, @ref), this._gv.isBatch);
                                return (null, false);
                            }
                            if (!this.staticResultsData.TryGetValue(unit, out subData)) {
                                ok = false;
                            } else if (!subData.TryGetValue(selectVar, out val2)) {
                                ok = false;
                            }
                            if (!ok) {
                                @ref = baseToRef(@base, unit);
                                Utils.error(string.Format("Cannot get data for variable {0} in {1} in second scenario", selectVar, @ref), this._gv.isBatch);
                                return (null, false);
                            }
                            if (layer == layer3) {
                                data = val2 - val1;
                                minDiff = Math.Min(minDiff, data);
                                maxDiff = Math.Max(maxDiff, data);
                            } else {
                                // layer 4
                                data = val1 == 0 ? 0 : (double)(val2 - val1) / val1 * 100;
                            }
                        }
                        f.SetField(varIndex, data);
                        glayer.SetFeature(f);
                    } while (true);
                }
            }
            this.summaryChanged = false;
            return (allVals, Math.Abs(minDiff) > maxDiff);
        }

        public Dictionary<int, Dictionary<string, double>> copyStaticResultsData() {
            //Return deep copy of this.staticResultsData.
            var data = new Dictionary<int, Dictionary<string, double>>();
            foreach (var (sub, varData) in this.staticResultsData) {
                var data2 = new Dictionary<string, double>();
                foreach (var (vari, val) in varData) {
                    data2[vari] = val;
                }
                data[sub] = data2;
            }
            return data;
        }

        // Create layout by opening template file.
        public async Task printResults() {
            string templ;
            // choose template file 
            var count = this._dlg.PprintCount.Value;
            if (count == 1) {
                if (this._dlg.PlandscapeButton.Checked) {
                    templ = "1Landscape.pagx";
                } else {
                    templ = "1Portrait.pagx";
                }
            } else if (count == 2) {
                if (this._dlg.PlandscapeButton.Checked) {
                    templ = "2Landscape.pagx";
                } else {
                    templ = "2Portrait.pagx";
                }
            } else if (count == 3) {
                if (this._dlg.PlandscapeButton.Checked) {
                    templ = "3Landscape.pagx";
                } else {
                    templ = "3Portrait.pagx";
                }
            } else if (count == 4) {
                if (this._dlg.PlandscapeButton.Checked) {
                    templ = "4Landscape.pagx";
                } else {
                    templ = "4Portrait.pagx";
                }
            } else if (count == 6) {
                if (this._dlg.PlandscapeButton.Checked) {
                    templ = "6Landscape.pagx";
                } else {
                    templ = "6Portrait.pagx";
                }
            } else {
                Utils.error(string.Format("There are layout templates only for 1, 2, 3, 4 or 6 result maps, not for {0}", count), this._gv.isBatch);
                return;
            }
            var templateIn = Utils.join(this._gv.addinPath, "LayoutTemplate" + templ);
            await QueuedTask.Run(() => {
                IProjectItem pagx = ItemFactory.Instance.Create(templateIn) as IProjectItem;
                if (pagx != null) {
                    Project.Current.AddItem(pagx);
                } else {
                    Utils.error("Failed to find layout template " + templateIn, this._gv.isBatch);
                }
            });
            //var templateOut = Utils.join(this._gv.tablesOutDir, this.title + templ);
            //Utils.loginfo(String.Format("Print composer template {0} written", templateOut));
        }
        
        // Use table of replacements to replace keys with items in returned line.
        public static string replaceInLine(string inLine, Dictionary<string, string> table) {
            foreach (var (patt, sub) in table) {
                inLine = inLine.Replace(patt, sub);
            }
            return inLine;
        }
        
        // Reveal or hide compose options group.
        public virtual void changeAnimationMode() {
            if (this._dlg.PprintAnimation.Checked) {
                this._dlg.PcomposeOptions.Visible = true;
                this._dlg.PcomposeCount.Value = 1; //  Utils.countLayersInGroup(Utils._ANIMATION_GROUP_NAME);
            } else {
                this._dlg.PcomposeOptions.Visible = false;
            }
        }
        
        // 
        //         Set up for animation.
        //         
        //         Collect animation data from database table according to animation variable; 
        //         set slider minimum and maximum;
        //         create animation layer;
        //         set speed accoring to spin box;
        //         set slider at minimum and display data for start time.
        //         
        public virtual async Task setupAnimateLayer() {
            if (this._dlg.PanimationVariableCombo.SelectedItem.ToString() == "") {
                return;
            }
            // can take a while so set a wait cursor
            Cursor.Current = Cursors.WaitCursor;
            this.doRewind();
            //this._dlg.calculateLabel.Text = "Calculating breaks ...");
            //this._dlg.repaint();
            try {
                if (!this.setPeriods()) {
                    return;
                }
                this.animateVar = this._dlg.PanimationVariableCombo.SelectedItem.ToString();
                if (!await this.createAnimationLayer()) {
                    return;
                }
                Debug.Assert(this.animateLayer is not null);
                var lid = this.animateLayer.Name;
                if (!this.readData(lid, false, this.table, this.animateVar, "")) {
                    return;
                }
                int animateLength = 0;
                this.summariseData(lid, false);
                if (this.isDaily) {
                    animateLength = this.periodDays;
                } else if (this.isAnnual) {
                    animateLength = Convert.ToInt32(this.periodYears);
                } else if (this.isMonthly) {
                    animateLength = Convert.ToInt32(this.periodMonths);
                } else {
                    animateLength = 0;
                }
                this._dlg.Pslider.Minimum = 1;
                this._dlg.Pslider.Maximum = animateLength;
                await this.colourAnimationLayer();
                this._dlg.Pslider.Value = 1;
                var sleep = this._dlg.PspinBox.Value;
                this.changeSpeed(sleep);
                this.resetSlider();
                await this.changeAnimate();
            } finally {
                //this._dlg.calculateLabel.Text = "");
                Cursor.Current = Cursors.Default;
            }
        }
        
        // Save animated GIF if still files found.
        public async virtual void saveVideo() {
            // capture final frame
            var extent = MapView.Active.Extent;
            int height = (int)Math.Ceiling(extent.YMax - extent.YMin + 0.5);
            int width = (int)Math.Ceiling(extent.XMax - extent.XMin + 0.5);
            await this.capture(height, width);
            // remove animation layout
        //    try {
        //        Debug.Assert(this.animationLayout is not null);
        //        QgsProject.instance().layoutManager().removeLayout(this.animationLayout);
        //    } catch {
        //    }
        //    var fileNames = (from fn in os.listdir(this._gv.pngDir)
        //        where fn.EndsWith(".png")
        //        select fn).OrderBy(_p_1 => _p_1).ToList();
        //    if (fileNames == new List<object>()) {
        //        return;
        //    }
        //    if (this._dlg.printAnimation.Checked) {
        //        var @base = Utils.join(this._gv.resultsDir, "Video.gif");
        //        this.videoFile = Utils.nextFileName(@base, 0)[0];
        //    } else {
            var resultsDir = this._gv.resultsDir;
            this.videoFile = Utils.join(resultsDir, this.animateVar + "Video.gif");
            try {
                File.Delete(this.videoFile);
            }
            catch (Exception) {
            }
            var fileNames = Directory.GetFiles(this._gv.pngDir);
            var numFiles = fileNames.Length;
            if (numFiles == 0) {
                return;
            }
            var period = 1.0 / Convert.ToDouble(this._dlg.PspinBox.Value);
            try {
                using (var images = new MagickImageCollection()) {
                    foreach (int i in Enumerable.Range(0, numFiles)) {
                        images.Add(new MagickImage(fileNames[i]));
                        images[i].AnimationDelay = 100;
                        images[i].AnimationIterations = 1;
                        images[i].GifDisposeMethod = GifDisposeMethod.Previous;
                    };
                    images.Optimize();
                    images.Write(this.videoFile);
                }
                // clear the png files:
                this.clearPngDir();
                Utils.information(string.Format("Animated gif {0} written", this.videoFile), this._gv.isBatch);
            }
            catch (Exception ex) {
                Utils.error(string.Format(@"
            Failed to generate animated gif: {0}.
            The .png files are in {1}: suggest you try using GIMP.
            ", ex.Message, this._gv.pngDir), this._gv.isBatch);
            }
        }
        
        // Set animating and not pause.
        public virtual void doPlay() {
            if (this._dlg.PanimationVariableCombo.SelectedItem.ToString() == "") {
                Utils.information("Please choose an animation variable", this._gv.isBatch);
                return;
            }
            this.animating = true;
            this.animationPaused = false;
        }
        
        // If animating change pause from on to off, or off to on.
        public virtual void doPause() {
            if (this.animating) {
                this.animationPaused = !this.animationPaused;
            }
        }
        
        // Turn off animating and pause and set slider to minimum.
        public virtual void doRewind() {
            this.animating = false;
            this.animationPaused = false;
            this.resetSlider();
        }
        
        // Move slide one step to right unless at maximum.
        public virtual void doStep() {
            if (this.animating && !this.animationPaused) {
                var val = this._dlg.Pslider.Value;
                if (val < this._dlg.Pslider.Maximum) {
                    this._dlg.Pslider.Value = val + 1;
                }
            }
        }
        
        // Stop any running animation and if possible move the animation slider one step left.
        public virtual void animateStepLeft() {
            if (this._dlg.PtabWidget.SelectedIndex == 1) {
                this.animating = false;
                this.animationPaused = false;
                var val = this._dlg.Pslider.Value;
                if (val > this._dlg.Pslider.Minimum) {
                    this._dlg.Pslider.Value = val - 1;
                }
            }
        }
        
        // Stop any running animation and if possible move the animation slider one step right.
        public virtual void animateStepRight() {
            if (this._dlg.PtabWidget.SelectedIndex == 1) {
                this.animating = false;
                this.animationPaused = false;
                var val = this._dlg.Pslider.Value;
                if (val < this._dlg.Pslider.Maximum) {
                    this._dlg.Pslider.Value = val + 1;
                }
            }
        }
        
        // 
        //         Starts or restarts the timer with speed set to val.
        //         
        //         Runs in a try ... except so that timer gets stopped if any exception.
        //         
        public virtual void changeSpeed(decimal val) {
            try {
                this.animateTimer.Interval = (int)(1000 / val);
                this.animateTimer.Start();
            }
            catch (Exception) {
                this.animating = false;
                this.animateTimer.Stop();
                // raise last exception again
                throw;
            }
        }
        
        // Turn off animating and pause.
        public virtual void pressSlider() {
            this.animating = false;
            this.animationPaused = false;
        }
        
        // Move slide to minimum.
        public virtual void resetSlider() {
            this._dlg.Pslider.Value = this._dlg.Pslider.Minimum;
        }
        
        // Convert slider value to date.
        public virtual int sliderValToDate(int sliderVal) {
            if (this.isDaily) {
                return this.addDays(this.julianStartDay + sliderVal - 1, this.startYear);
            } else if (this.isAnnual) {
                return this.startYear + sliderVal - 1;
            } else if (this.isMonthly) {
                var totalMonths = this.startMonth + sliderVal - 2;
                var year = (double)totalMonths / 12;
                var month = totalMonths % 12 + 1;
                return (int)(this.startYear + year) * 100 + month;
            } else {
                return this.startYear;
            }
        }
        
        // Run the compare scenarios form.
        public virtual void startCompareScenarios() {
            this._comparedlg.ShowDialog();
        }
        
        // Save chosen scenarios and exit form.
        public virtual void setupCompareScenarios() {
            string prtFile;
            string simFile;
            string txtInOutDir;
            string resultsDir;
            string scenDir;
            var scenario1 = this._comparedlg.Pscenario1.SelectedItem.ToString();
            var scenario2 = this._comparedlg.Pscenario2.SelectedItem.ToString();
            if (scenario1 == "" || scenario2 == "" || scenario1 == scenario2) {
                Utils.information("Please choose two different scenarios to compare", this._gv.isBatch);
                this.closeCompareScenarios();
                return;
            }
            this._dlg.PcompareLabel.Text = string.Format("Compare {0} and {1}", scenario1, scenario2);
            this.scenario1 = scenario1;
            this.scenario2 = scenario2;
            // constrict dates if necessary
            if (this.scenario1 != this.scenario) {
                scenDir = Utils.join(this._gv.scenariosDir, this.scenario1);
                if (this.table != "") {
                    resultsDir = Utils.join(scenDir, Parameters._RESULTS);
                    if (!this._gv.db.hasTable(Utils.join(resultsDir, Parameters._OUTPUTDB), this.table)) {
                        Utils.error(string.Format("There is no {0} output table for scenario {1}",this.table, this.scenario1), this._gv.isBatch);
                        this.closeCompareScenarios();
                        return;
                    }
                }
                txtInOutDir = Utils.join(scenDir, Parameters._TXTINOUT);
                simFile = Utils.join(txtInOutDir, Parameters._SIM);
                if (!File.Exists(simFile)) {
                    Utils.error(string.Format("Cannot find simulation file {0}",simFile), this._gv.isBatch);
                    this.closeCompareScenarios();
                    return;
                }
                prtFile = Utils.join(txtInOutDir, Parameters._PRT);
                if (!File.Exists(prtFile)) {
                    Utils.error(string.Format("Cannot find print file {0}",prtFile), this._gv.isBatch);
                    this.closeCompareScenarios();
                    return;
                }
                this.readPrt(prtFile, simFile, this.scenario1);
            }
            if (this.scenario2 != this.scenario) {
                scenDir = Utils.join(this._gv.scenariosDir, this.scenario2);
                if (this.table != "") {
                    resultsDir = Utils.join(scenDir, Parameters._RESULTS);
                    if (!this._gv.db.hasTable(Utils.join(resultsDir, Parameters._OUTPUTDB), this.table)) {
                        Utils.error(string.Format("There is no {0} output table for scenario {1}",this.table, this.scenario2), this._gv.isBatch);
                        this.closeCompareScenarios();
                        return;
                    }
                }
                txtInOutDir = Utils.join(scenDir, Parameters._TXTINOUT);
                simFile = Utils.join(txtInOutDir, Parameters._SIM);
                if (!File.Exists(simFile)) {
                    Utils.error(string.Format("Cannot find simulation file {0}",simFile), this._gv.isBatch);
                    this.closeCompareScenarios();
                    return;
                }
                prtFile = Utils.join(txtInOutDir, Parameters._PRT);
                if (!File.Exists(prtFile)) {
                    Utils.error(string.Format("Cannot find print file {0}",prtFile), this._gv.isBatch);
                    this.closeCompareScenarios();
                    return;
                }
                this.readPrt(prtFile, simFile, this.scenario2);
            }
            this._comparedlg.Close();
        }
        
        public virtual void closeCompareScenarios() {
            this.scenario1 = "";
            this.scenario2 = "";
            this._dlg.PcompareLabel.Text = "Compare X and Y";
            this._comparedlg.Close();
        }
        
        // Make Julian date from year + days.
        public virtual int addDays(int days, int year) {
            var leapAdjust = Visualise.isLeap(year) ? 1 : 0;
            var lenYear = 365 + leapAdjust;
            if (days <= lenYear) {
                return year * 1000 + days;
            } else {
                return this.addDays(days - lenYear, year + 1);
            }
        }
        
        // 
        //         Return datetime.date from year and number of days.
        //         
        //         The day may exceed the length of year, in which case a later year
        //         will be returned.
        //         
        public virtual DateOnly julianToDate(int day, int year) {
            if (day <= 31) {
                return new DateOnly(year, 1, day);
            }
            day -= 31;
            var leapAdjust = Visualise.isLeap(year) ? 1 : 0;
            if (day <= 28 + leapAdjust) {
                return new DateOnly(year, 2, day);
            }
            day -= 28 + leapAdjust;
            if (day <= 31) {
                return new DateOnly(year, 3, day);
            }
            day -= 31;
            if (day <= 30) {
                return new DateOnly(year, 4, day);
            }
            day -= 30;
            if (day <= 31) {
                return new DateOnly(year, 5, day);
            }
            day -= 31;
            if (day <= 30) {
                return new DateOnly(year, 6, day);
            }
            day -= 30;
            if (day <= 31) {
                return new DateOnly(year, 7, day);
            }
            day -= 31;
            if (day <= 31) {
                return new DateOnly(year, 8, day);
            }
            day -= 31;
            if (day <= 30) {
                return new DateOnly(year, 9, day);
            }
            day -= 30;
            if (day <= 31) {
                return new DateOnly(year, 10, day);
            }
            day -= 31;
            if (day <= 30) {
                return new DateOnly(year, 11, day);
            }
            day -= 30;
            if (day <= 31) {
                return new DateOnly(year, 12, day);
            } else {
                return this.julianToDate(day - 31, year + 1);
            }
        }
        
        // Convert integer date to string.
        public virtual string dateToString(int dat) {
            if (this.isDaily) {
                return this.julianToDate(dat % 1000, dat / 1000).ToLongDateString();
            }
            if (this.isMonthly) {
                int year = dat / 100;
                string month = Visualise._MONTHS[dat % 100 - 1];
                return month + " " + year.ToString();
            }
            // annual or average annual
            return dat.ToString();
        }
        
        // Switch between recording and not.
        public virtual void record() {
            this.capturing = !this.capturing;
            if (this.capturing) {
                // clear any existing png files (can be left eg if making gif failed)
                this.clearPngDir();
                if (this._dlg.PprintAnimation.Checked) {
                    // TODO
                    //this.createAnimationComposition();
                }
                this._dlg.PrecordButton.BackColor = System.Drawing.Color.Red;
                this._dlg.PrecordLabel.Text = "Stop recording";
                this._dlg.PplayButton.Enabled = false;
            } else {
                Cursor.Current = Cursors.WaitCursor;
                this._dlg.PrecordButton.BackColor = System.Drawing.Color.Green;
                this._dlg.PrecordLabel.Text = "Start recording";
                this.saveVideo();
                this._dlg.PplayButton.Enabled = true;
                Cursor.Current = Cursors.Default;
            }
        }
        
        // Use default application to play video file (an animated gif).
        public virtual void playRecording() {
            // stop recording if necessary
            if (this.capturing) {
                this.record();
            }
            if (!File.Exists(this.videoFile)) {
                Utils.information(string.Format("No video file for {0} exists at present",this.animateVar), this._gv.isBatch);
                return;
            }
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = this.videoFile;
            psi.UseShellExecute = true;
            Process.Start(psi);
        }

        // Flag change to summary method.
        public virtual void changeSummary() {
            this.summaryChanged = true;
        }
        
        // If user changes the aquifer renderer, flag to retain colour scheme.
        public virtual void changeAquRenderer() {
            if (!this.internalChangeToAquRenderer) {
                this.keepAquColours = true;
            }
        }
        
        // If user changes the deep aquifer renderer, flag to retain colour scheme.
        public virtual void changeDeepAquRenderer() {
            if (!this.internalChangeToDeepAquRenderer) {
                this.keepDeepAquColours = true;
            }
        }
        
        // If user changes the stream renderer, flag to retain colour scheme.
        public virtual void changeRivRenderer() {
            if (!this.internalChangeToRivRenderer) {
                this.keepRivColours = true;
            }
        }
        
        // If user changes the subbasin renderer, flag to retain colour scheme.
        public virtual void changeSubRenderer() {
            if (!this.internalChangeToSubRenderer) {
                this.keepSubColours = true;
            }
        }
        
        // If user changes the LSU renderer, flag to retain colour scheme.
        public virtual void changeLSURenderer() {
            if (!this.internalChangeToLSURenderer) {
                this.keepLSUColours = true;
            }
        }
        
        // If user changes the HRU renderer, flag to retain colour scheme.
        public virtual void changeHRURenderer() {
            if (!this.internalChangeToHRURenderer) {
                this.keepHRUColours = true;
            }
        }
        
        // 
        //         Update current plot row according to the colChanged index.
        //         
        //         If there are no rows, first makes one.
        //         
        public virtual void updateCurrentPlotRow(int colChanged) {
            if (!this.plotting()) {
                return;
            }
            var row = this._dlg.PtableWidget.CurrentRow;
            if (row is null) {
                this.doAddPlot();
                row = this._dlg.PtableWidget.CurrentRow;
            }
            var dataRow = this.plotData.Rows[row.Index];
            if (colChanged == 0) {
                dataRow[0] = this.scenario;
            } else if (colChanged == 1) {
                if ((string)dataRow[1] == "-") {
                    // observed plot: do not change
                    return;
                }
                dataRow[1] = this.table;
                this._dlg.PunitLabel.Text = this.tableUnitName(this.table);
                dataRow[2] = "";
                dataRow[3] = "";
            } else if (colChanged == 2) {
                dataRow[2] = this._dlg.PunitPlot.SelectedItem.ToString();
            } else {
                dataRow[3] = this._dlg.PvariablePlot.SelectedItem.ToString();
            }
        }
        
        public Dictionary<string, string> _unitNames = new Dictionary<string, string> {
            {
                "basin_",
                "Basin"},
            {
                "lsunit_",
                "LSU"},
            {
                "hru_",
                "HRU"},
            {
                "region_",
                "Region"},
            {
                "channel_sd_",
                "Channel_SD"},
            {
                "channel_sdmorph_",
                "Channel_SDMorph"},
            {
                "channel_",
                "Channel"},
            {
                "aquifer_",
                "Aquifer"},
            {
                "deep_aquifer_",
                "Deep aquifer"},
            {
                "reservoir_",
                "Reservoir"},
            {
                "wetland_",
                "Wetland"},
            {
                "hydin_",
                "Hydrograph in"},
            {
                "hydout_",
                "Hydrograph out"},
            {
                "ru_",
                "Routing unit"}};
        
        // Return name for table unit.
        public virtual string tableUnitName(string table) {
            foreach (var (key, uname) in this._unitNames) {
                if (table.Contains(key)) {
                    return uname;
                }
            }
            return "Unknown";
        }
        
        // Return number of non-observed plots.
        public virtual int countPlots() {
            var size = this.plotData.Rows.Count;
            var result = size;
            foreach (var row in Enumerable.Range(0, size)) {
                if ((string)this.plotData.Rows[row][1] == "-") {
                    // observed row
                    result -= 1;
                }
            }
            return result;
        }

        // used when current tableWidget cell changes.
        // If the cell is a variable cell (column index 3), set variables for observed 
        // if current cell row is observed, else according to current table
        public void setVariablesForRow() {
            try {
                if (this._dlg.PtableWidget.CurrentCell.ColumnIndex != 3) { return; }
                var row = this._dlg.PtableWidget.CurrentCell.OwningRow;
                var rowTable = (string)row.Cells[1].Value;
                if (rowTable == "-") {  // observed row
                    this.setObservedVars();
                } else {
                    this.setPlotVars(rowTable);
                }
            } catch {
                // allow to fail quietly
                return;
            }
        }
        
        // Add a plot row and make it current.
        public virtual void doAddPlot() {
            if (this._dlg.PunitPlot.SelectedItem is null) {
                if (!string.IsNullOrEmpty(this._dlg.PunitEdit.Text))
                    this.plotEditUnit();
            }
            if (this._dlg.PunitPlot.SelectedItem is null) {
                Utils.information("Please set a unit value for the plot to be added", this._gv.isBatch);
                return;
            }
            var unit = this._dlg.PunitPlot.SelectedItem.ToString();
            var size = this._dlg.PtableWidget.Rows.Count;
            if (size > 1 && (string)this.plotData.Rows[size - 2][1] == "-") {
                // last plot was observed: need to reset variables
                this.table = "";
                this.setVariables();
            }
            var vari = this._dlg.PvariablePlot.SelectedIndex < 0 ? "" : this._dlg.PvariablePlot.SelectedItem.ToString();
            var row = this.plotData.NewRow();
            row[0] = this.scenario;
            row[1] = this.table;
            row[2] = unit;
            row[3] = vari;
            this.plotData.Rows.InsertAt(row, size - 1);
            this._dlg.PtableWidget.CurrentCell = this._dlg.PtableWidget.Rows[size - 1].Cells[0];
            if (this.countPlots() == 1) {
                // just added first row - reduce tables to those with same frequency
                if (Visualise.tableIsDaily(this.table)) {
                    this.restrictOutputTablesByTerminator("_day");
                } else if (Visualise.tableIsMonthly(this.table)) {
                    this.restrictOutputTablesByTerminator("_mon");
                } else if (Visualise.tableIsAnnual(this.table)) {
                    this.restrictOutputTablesByTerminator("_yr");
                }
            }
        }
        
        // Return the table of a non-observed plot, or empty string if none.
        public virtual string getPlotTable() {
            foreach (var row in Enumerable.Range(0, this._dlg.PtableWidget.Rows.Count)) {
                var table = this._dlg.PtableWidget.Rows[row].Cells[1].ToString();
                if (table == "-") {
                    // observed
                    continue;
                } else {
                    return table;
                }
            }
            return "";
        }
        
        // Delete current plot row.
        public virtual void doDelPlot() {
            var row = this._dlg.PtableWidget.CurrentRow;
            if (row is null) {
                Utils.information("Please select a row for deletion", this._gv.isBatch);
                return;
            }
            // only remove current row
            this.plotData.Rows.RemoveAt(row.Index);
            if (this.countPlots() == 0) {
                // no non-observed plots - restore output tables combo so any frequency can be chosen
                this.populateOutputTables();
            }
        }
        
        // Add a copy of the current plot row and make it current.
        public virtual void doCopyPlot() {
            var row = this._dlg.PtableWidget.CurrentRow;
            if (row is null) {
                Utils.information("Please select a row to copy", this._gv.isBatch);
                return;
            }
            var size = this.plotData.Rows.Count;
            if (Enumerable.Range(0, size).Contains(row.Index)) {
                var plotRow = this.plotData.Rows[row.Index];
                var copyRow = this.plotData.NewRow();
                foreach (var col in Enumerable.Range(0, 4)) {
                    copyRow[col] = plotRow[col];
                }
                this.plotData.Rows.InsertAt(copyRow, size);
            }
            this._dlg.PtableWidget.CurrentCell = this._dlg.PtableWidget.Rows[size].Cells[0];

        }

        // Move current plot row up 1 place and keep it current.
        public virtual void doUpPlot() {
            var row = this._dlg.PtableWidget.CurrentRow;
            if (row is null) {
                Utils.information("Please select a row to move up", this._gv.isBatch);
                return;
            }
            var index = row.Index;
            if (1 <= index && index < this.plotData.Rows.Count) {
                var upper = this.plotData.Rows[index - 1];
                var lower = this.plotData.Rows[index];
                foreach (var col in Enumerable.Range(0, 4)) {
                    var item = upper[col];
                    upper[col] = lower[col];
                    lower[col] = item;
                }
            }
            this._dlg.PtableWidget.CurrentCell = this._dlg.PtableWidget.Rows[index - 1].Cells[0];
        }

        // Move current plot row down 1 place and keep it current.
        public virtual void doDownPlot() {
            var row = this._dlg.PtableWidget.CurrentRow;
            if (row is null) {
                Utils.information("Please select a row to move up", this._gv.isBatch);
                return;
            }
            var index = row.Index;
            if (0 <= index && index < this.plotData.Rows.Count - 1) {
                var upper = this.plotData.Rows[index];
                var lower = this.plotData.Rows[index + 1];
                foreach (var col in Enumerable.Range(0, 4)) {
                    var item = upper[col];
                    upper[col] = lower[col];
                    lower[col] = item;
                }
            }
            this._dlg.PtableWidget.CurrentCell = this._dlg.PtableWidget.Rows[index + 1].Cells[0];
        }

        // set plot variables according to table parameter
        public void setPlotVars(string table) {
            this._dlg.PvariablePlot.Items.Clear();
            //this._dlg.PvariablePlot.Items.Add("");
            //this._dlg.PvariablePlot.Text = "";
            if (!string.IsNullOrEmpty(table)) {
                var sql = $"PRAGMA TABLE_INFO({table})";
                using (SQLiteDataReader reader = this.getReader(sql)) {
                    while (reader.Read()) {
                        var vari = reader.GetString(1);
                        var varType = reader.GetString(2);
                        // only include fields of type REAL
                        if (varType == "REAL" && !this.ignoredVars.Contains(vari)) {
                            this._dlg.PvariablePlot.Items.Add(vari);
                        }
                    }
                }
            }
        }

        // Add a row for an observed plot, and make it current.
        public virtual void addObservedPlot() {
            if (!File.Exists(this.observedFileName)) {
                return;
            }
            this.setObservedVars();
            var row = this.plotData.NewRow();
            row[0] = "observed";
            row[1] = "-";
            row[2] = "-";
            row[3] = this._dlg.PvariablePlot.SelectedIndex < 0 ?
                (this._dlg.PvariablePlot.Items.Count == 1 ? this._dlg.PvariablePlot.Items[0].ToString() : "") :
                this._dlg.PvariablePlot.SelectedItem.ToString();
            this._dlg.PvariablePlot.Text = (string)row[3];
            this.plotData.Rows.Add(row);
            this._dlg.PtableWidget.CurrentCell = this._dlg.PtableWidget.Rows[this._dlg.PtableWidget.Rows.Count - 2].Cells[0];
        }

        // Add variables from 1st line of observed data file, ignoring 'date' if it occurs as the first column.
        public virtual void setObservedVars() {
            string line;
            using (var obs = new StreamReader(this.observedFileName)) {
                line = obs.ReadLine();
            }
            var varz = line.Split(',');
            int num = varz.Count();
            if (num == 0) {
                Utils.error(string.Format("Cannot find variables in first line of observed data file {0}", this.observedFileName), this._gv.isBatch);
                return;
            }
            var col1 = varz[0].Trim().ToLower();
            var start = col1 == "date" ? 1 : 0;
            this._dlg.PvariablePlot.Items.Clear();
            this._dlg.PvariablePlot.Text = "";
            foreach (var i in Enumerable.Range(start, num - start)) {
                // need to strip since last variable in csv header comes with newline
                this._dlg.PvariablePlot.Items.Add(varz[i].Trim());
            }
        }
        
        // 
        //         Read data for var from observed data file, returning a list of data as strings.
        //         
        //         Note that dates are not checked even if present in the observed data file.
        //         
        public virtual List<string> readObservedFile(string vari) {
            var result = new List<string>();
            using (var obs = new StreamReader(this.observedFileName)) {
                var line = obs.ReadLine();
                var varz = (from var1 in line.Split(',')
                            select var1.Trim()).ToList();
                int numVarz = varz.Count;
                if (numVarz == 0) {
                    Utils.error(string.Format("Cannot find variables in first line of observed data file {0}", this.observedFileName), this._gv.isBatch);
                    return result;
                }
                var idx = varz.FindIndex(x => x == vari);
                if (idx < 0) {
                    Utils.error(string.Format("Cannot find variable {0} in first line of observed data file {1}", vari, this.observedFileName), this._gv.isBatch);
                    return result;
                }
                while (true) {
                    line = obs.ReadLine();
                    if (line is null) {
                        break;
                    }
                    var vals = line.Split(',');
                    if (0 <= idx && idx < vals.Count()) {
                        result.Add(vals[idx].Trim());
                    } else {
                        break;
                    }
                }
            }
            return result;
        }
        
        // code from http://danieljlewis.org/files/2010/06/Jenks.pdf
        // described at http://danieljlewis.org/2010/06/07/jenks-natural-breaks-algorithm-in-python/
        // amended following style of http://www.macwright.org/simple-statistics/docs/simple_statistics.html#section-116
        // no longer used - replaced by Cython
        //===========================================================================
        // @staticmethod
        // def getJenksBreaks( dataList, numClass ):
        //     """Return Jenks breaks for dataList with numClass classes."""
        //     if not dataList:
        //         return [], 0
        //     # Use of sample unfortunate because gives poor animation results.
        //     # Tends to overestimate lower limit and underestimate upper limit, and areas go white in animation.
        //     # But can take a long time to calculate!
        //     # QGIS internal code uses 1000 here but 4000 runs in reasonable time
        //     maxSize = 4000
        //     # use a sample if size exceeds maxSize
        //     size = len(dataList)
        //     if size > maxSize:
        //         origSize = size
        //         size = Math.Max(maxSize, size / 10)
        //         Utils.loginfo('Jenks breaks: using a sample of size {0} from {1}',size, origSize))
        //         sample = random.sample(dataList, size)
        //     else:
        //         sample = dataList
        //     sample.sort()
        //     # at most one class: return singleton list
        //     if numClass <= 1:
        //         return [sample.last()]
        //     if numClass >= size:
        //         # nothing useful to do
        //         return sample
        //     lowerClassLimits = []
        //     varianceCombinations = []
        //     variance = 0
        //     for i in range(0,size+1):
        //         temp1 = []
        //         temp2 = []
        //         # initialize with lists of zeroes
        //         for j in range(0,numClass+1):
        //             temp1.Add(0)
        //             temp2.Add(0)
        //         lowerClassLimits.Add(temp1)
        //         varianceCombinations.Add(temp2)
        //     for i in range(1,numClass+1):
        //         lowerClassLimits[1][i] = 1
        //         varianceCombinations[1][i] = 0
        //         for j in range(2,size+1):
        //             varianceCombinations[j][i] = float('inf')
        //     for l in range(2,size+1):
        //         # sum of values seen so far
        //         summ = 0
        //         # sum of squares of values seen so far
        //         sumSquares = 0
        //         # for each potential number of classes. w is the number of data points considered so far
        //         w = 0
        //         i4 = 0
        //         for m in range(1,l+1):
        //             lowerClassLimit = l - m + 1
        //             val = float(sample[lowerClassLimit-1])
        //             w += 1
        //             summ += val
        //             sumSquares += val * val
        //             variance = sumSquares - (summ * summ) / w
        //             i4 = lowerClassLimit - 1
        //             if i4 != 0:
        //                 for j in range(2,numClass+1):
        //                     # if adding this element to an existing class will increase its variance beyond the limit, 
        //                     # break the class at this point, setting the lower_class_limit.
        //                     if varianceCombinations[l][j] >= (variance + varianceCombinations[i4][j - 1]):
        //                         lowerClassLimits[l][j] = lowerClassLimit
        //                         varianceCombinations[l][j] = variance + varianceCombinations[i4][j - 1]
        //         lowerClassLimits[l][1] = 1
        //         varianceCombinations[l][1] = variance
        //     k = size
        //     kclass = []
        //     for i in range(0,numClass+1):
        //         kclass.Add(0)
        //     kclass[numClass] = float(sample[size - 1])
        //     countNum = numClass
        //     while countNum >= 2:#print "rank = " + str(lowerClassLimits[k][countNum])
        //         idx = int((lowerClassLimits[k][countNum]) - 2)
        //         #print "val = " + str(sample[idx])
        //         kclass[countNum - 1] = sample[idx]
        //         k = int((lowerClassLimits[k][countNum] - 1))
        //         countNum -= 1
        //     return kclass, sample[0]
        //===========================================================================
        // copied like above but not used
        //===============================================================================
        //     @staticmethod
        //     def getGVF( sample, numClass ):
        //         """
        //         The Goodness of Variance Fit (GVF) is found by taking the
        //         difference between the squared deviations
        //         from the array mean (SDAM) and the squared deviations from the
        //         class means (SDCM), and dividing by the SDAM
        //         """
        //         breaks = Visualise.getJenksBreaks(sample, numClass)
        //         sample.sort()
        //         size = len(sample)
        //         listMean = sum(sample)/size
        //         print listMean
        //         SDAM = 0.0
        //         for i in range(0,size):
        //             sqDev = (sample[i] - listMean)**2
        //             SDAM += sqDev
        //         SDCM = 0.0
        //         for i in range(0,numClass):
        //             if breaks[i] == 0:
        //                 classStart = 0
        //             else:
        //                 classStart = sample.index(breaks[i])
        //             classStart += 1
        //             classEnd = sample.index(breaks[i+1])
        //             classList = sample[classStart:classEnd+1]
        //         classMean = sum(classList)/len(classList)
        //         print classMean
        //         preSDCM = 0.0
        //         for j in range(0,len(classList)):
        //             sqDev2 = (classList[j] - classMean)**2
        //             preSDCM += sqDev2
        //             SDCM += preSDCM
        //         return (SDAM - SDCM)/SDAM
        // 
        //     # written by Drew
        //     # used after running getJenksBreaks()
        //     @staticmethod
        //     def classify(value, breaks):
        //         """
        //         Return index of value in breaks.
        //         
        //         Returns i such that
        //         breaks = [] and i = -1, or
        //         value < breaks[1] and i = 1, or 
        //         breaks[i-1] <= value < break[i], or
        //         value >= breaks[len(breaks) - 1] and i = len(breaks) - 1
        //         """
        //         for i in range(1, len(breaks)):
        //             if value < breaks[i]:
        //                 return i
        //         return len(breaks) - 1 
        //===============================================================================

        // TODO
        //// Can often end up with more than one map title.  Remove all of them from the canvas, prior to resetting one required.
        //public virtual void clearMapTitle() {
        //    var canvas = this._gv.iface.mapCanvas();
        //    var scene = canvas.scene();
        //    if (this.mapTitle is not null) {
        //        scene.removeItem(this.mapTitle);
        //        this.mapTitle = null;
        //        // scene causes a crash for some users.
        //        // this code seems unnecessary in any case
        //        //         for item in scene:
        //        //             # testing by isinstance is insufficient as a MapTitle item can have a wrappertype
        //        //             # and the test returns false
        //        //             #if isinstance(item, MapTitle):
        //        //             try:
        //        //                 if item is None:
        //        //                     isMapTitle = False
        //        //                 else:
        //        //                     isMapTitle = item.identifyMapTitle() == 'MapTitle'
        //        //             except Exception:
        //        //                 isMapTitle = False
        //        //             if isMapTitle:
        //        //                 scene.removeItem(item)
        //        //         self.mapTitle = None
        //    }
        //    canvas.refresh();
        //}
        //public async void changeAnimation(object sender, PropertyChangedEventArgs e) {
        //    await this.setAnimateLayer();
        //}

        // Set self.animateLayer to first visible layer in Animations group, retitle as appropriate.
        //public async Task setAnimateLayer() { // TODO
            //var animationLayers = Utils.getLayersInGroup(Utils._ANIMATION_GROUP_NAME, visible: true);
            //if (animationLayers.Count == 0) {
            //    this.animateLayer = null;
            //    await this.setResultsLayer();
            //    return;
            //}
            //// expand the animation layer and hide the results group maps
            //var animationGroup = Utils.getGroupLayerByName(Utils._ANIMATION_GROUP_NAME);
            //if (!animationGroup.IsExpanded) {
            //    await QueuedTask.Run(() => { animationGroup.SetExpanded(true); });
            //}
            //var resultsGroup = Utils.getGroupLayerByName(Utils._RESULTS_GROUP_NAME);
            //if (resultsGroup.IsVisible) {
            //    await QueuedTask.Run(() => { resultsGroup.SetVisibility(false); });
            //}
            //// TODO
            //this.animateLayer = animationLayers[0] as FeatureLayer;
            //foreach (var mapLayer in animationLayers) {
            //    Debug.Assert(mapLayer is not null);
            //    if (this.mapTitle is null) {
            //        this.mapTitle = new MapTitle(canvas, this.title, mapLayer);
            //        canvas.refresh();
            //        this.animateLayer = cast(QgsVectorLayer, mapLayer);
            //        return;
            //    } else if (mapLayer == this.mapTitle.layer) {
            //        // nothing to do
            //        return;
            //    } else {
            //        // first visible animation layer not current titleLayer
            //        this.clearMapTitle();
            //        var dat = this.sliderValToDate();
            //        var date = this.dateToString(dat);
            //        this.mapTitle = new MapTitle(canvas, this.title, mapLayer, line2: date);
            //        canvas.refresh();
            //        this.animateLayer = cast(QgsVectorLayer, mapLayer);
            //        return;
            //    }
            //}
            //// if we get here, no visible animation layers
            //this.clearMapTitle();
            //this.animateLayer = null;
            //return;
        //}

        //public async void setResults(object sender, PropertyChangedEventArgs e) {
        //    await this.setResultsLayer();
        //}

        // Set self.currentResultsLayer to first visible layer in Results group, retitle as appropriate.
        //public async Task setResultsLayer() { // TODO
            //// only change results layer and title if there are no visible animate layers
            //var animationLayers = Utils.getLayersInGroup(Utils._ANIMATION_GROUP_NAME, visible: true);
            //if (animationLayers.Count > 0) {
            //    return;
            //}
            //// make sure results group is visible, since running animations may have made it invisible
            //var resultsGroup = Utils.getGroupLayerByName(Utils._RESULTS_GROUP_NAME);
            //if (!resultsGroup.IsVisible) {
            //    await QueuedTask.Run(() => { resultsGroup.SetVisibility(true); });
            //}
            ////TODO
            //var resultsLayers = Utils.getLayersInGroup(Utils._RESULTS_GROUP_NAME, visible: true);
            //if (resultsLayers.Count == 0) {
            //    this.currentResultsLayer = null;
            //    // no results or animation layers: clear graphics layer
            //    await QueuedTask.Run(() => {
            //        this.setMapTitle(null);
            //    });
            //    return;
            //} else {
            //    this.currentResultsLayer = resultsLayers[0] as FeatureLayer;
            //    await QueuedTask.Run(() => {
            //        this.setMapTitle(this.currentResultsLayer);
            //    });
            //    //TODO
            //    //foreach (var treeLayer in resultsLayers) {
            //    //    var mapLayer = treeLayer.layer();
            //    //    Debug.Assert(mapLayer is not null);
            //    //    this.currentResultsLayer = cast(QgsVectorLayer, mapLayer);
            //    //    Debug.Assert(this.currentResultsLayer is not null);
            //    //    this.mapTitle = new MapTitle(canvas, this.title, mapLayer);
            //    //    canvas.refresh();
            //    //}
            //    return;
                //}
        //}
        
        // Remove shape files (all components) from animation directory.
        public virtual void clearAnimationDir() {
            if (Directory.Exists(this._gv.animationDir)) {
                var pattern = "*.shp";
                Matcher matcher = new Matcher();
                matcher.AddInclude(pattern);
                foreach (var f in matcher.GetResultsInFullPath(this._gv.animationDir)) {
                    try {
                        Utils.removeFiles(f);
                    }
                    catch (Exception) {
                        continue;
                    }
                }
            }
        }

        // Remove .png files from Png directory.
        public virtual void clearPngDir() {
            if (Directory.Exists(this._gv.pngDir)) {
                var pattern = "*.png";
                Matcher matcher = new Matcher();
                matcher.AddInclude(pattern);
                foreach (var f in matcher.GetResultsInFullPath(this._gv.animationDir)) {
                    try {
                        File.Delete(f);
                    }
                    catch (Exception) {
                        continue;
                    }
                }
            }
            this.currentStillNumber = 0;
        }

        // Fill subbasinOutletChannels from rivs shapefile unless already done.  Fill QqSubbasins combo.
        public virtual void setSubbasinOutletChannels() {
            if (this.subbasinOutletChannels.Count > 0) {
                return;
            }
            var scenDir = Utils.join(this._gv.scenariosDir, this.scenario);
            var resultsDir = Utils.join(scenDir, Parameters._RESULTS);
            var rivsFile = Utils.join(resultsDir, Parameters._RIVS) + ".shp";
            if (!File.Exists(rivsFile)) {
                Utils.error(string.Format("Cannot find channels shapefile {0}",rivsFile), this._gv.isBatch);
                return;
            }
            var downChannel = new Dictionary<int, int>();
            var channelToSubbasin = new Dictionary<int, int>();
            var mainOutlet = 0;
            using (var rivDs = Ogr.Open(rivsFile, 0)) {
                var rivLayer = rivDs.GetLayerByIndex(0);
                var fields = rivLayer.GetLayerDefn();
                var chIdx = fields.GetFieldIndex(Topology._CHANNEL);
                var chRIdx = fields.GetFieldIndex(Topology._CHANNELR);
                var subIdx = fields.GetFieldIndex(Topology._SUBBASIN);
                if (chRIdx < 0 || subIdx < 0) {
                    var rivs1File = Utils.join(this._gv.shapesDir, Parameters._RIVS1) + ".shp";
                    if (File.Exists(rivs1File)) {
                        var riv1Ds = Ogr.Open(rivs1File, 0);
                        rivLayer = riv1Ds.GetLayerByIndex(0);
                        fields = rivLayer.GetLayerDefn();
                        chIdx = fields.GetFieldIndex(Topology._CHANNEL);
                        chRIdx = fields.GetFieldIndex(Topology._CHANNELR);
                        subIdx = fields.GetFieldIndex(Topology._SUBBASIN);
                        if (chIdx >= 0 && chRIdx >= 0 && subIdx >= 0) {
                            Utils.information(string.Format(@"Cannot find ChannelR or Subbasin fields in {0}.  
                        Will use {1}.
                        This is because your model was built with an older version of QSWAT+.
                        There will be no harm if your subbasins and channels have not chenged.
                        Otherwise you should rebuild and rerun your model.", rivsFile, rivs1File), this._gv.isBatch);
                            rivsFile = rivs1File;
                        } else {
                            Utils.error(string.Format("Cannot find ChannelR or Subbasin fields in {0}", rivsFile), this._gv.isBatch);
                            return;
                        }
                    }
                }
                rivLayer.ResetReading();
                OSGeo.OGR.Feature channel;
                do {
                    channel = rivLayer.GetNextFeature();
                    if (channel is null) { break; }
                    var chNum = channel.GetFieldAsInteger(chIdx);
                    downChannel[chNum] = channel.GetFieldAsInteger(chRIdx);
                    channelToSubbasin[chNum] = channel.GetFieldAsInteger(subIdx);
                } while (true);
                foreach (var (ch, chR) in downChannel) {
                    var sub = channelToSubbasin[ch];
                    int subR;
                    if (!channelToSubbasin.TryGetValue(chR, out subR)) { subR = -1; }
                    if (chR == 0 || subR >= 0 && sub != subR) {
                        this.subbasinOutletChannels[sub] = ch;
                    }
                    if (chR == 0) {
                        mainOutlet = sub;
                    }
                }
            }
            this._dlg.PQqSubbasin.Items.Clear();
            this._dlg.PdQpSubbasin.Items.Clear();
            this._dlg.PQbSubbasin.Items.Clear();
            foreach (var sub in this.subbasinOutletChannels.Keys.OrderBy(_p_1 => _p_1)) {
                this._dlg.PQqSubbasin.Items.Add(sub);
                this._dlg.PdQpSubbasin.Items.Add(sub);
                this._dlg.PQbSubbasin.Items.Add(sub);
            }
            // preselect main outlet subbasin
            if (mainOutlet > 0) {
                this._dlg.PQqSubbasin.Text = mainOutlet.ToString();
                this._dlg.PdQpSubbasin.Text = mainOutlet.ToString();
                this._dlg.PQbSubbasin.Text = mainOutlet.ToString();
            }
            this.QqData = new DataTable();
            this._dlg.PQqResults.DataSource = this.QqData;
            this.QbData = new DataTable();
            this._dlg.PQbResults.DataSource = this.QbData;
            this.QqData.Columns.Add("", typeof(string));
            this.QqData.Columns.Add("", typeof(string));
            this.QbData.Columns.Add("", typeof(string));
            this.QbData.Columns.Add("", typeof(string));
            this._dlg.PQqResults.Columns[0].Width = 100;
            this._dlg.PQqResults.Columns[1].Width = 100;
            this._dlg.PQbResults.Columns[0].Width = 100;
            this._dlg.PQbResults.Columns[1].Width = 100;
            this.setQqTableHead();
            this.cleardQpResult();
            this.setQbTableHead();
        }
        
        // Clears results table.  Sets header.
        public virtual void setQqTableHead() {
            this.QqData.Clear();
            this.QqData.Columns[1].ColumnName = "Q" + this._dlg.PQqSpin.Value.ToString();
        }
        
        // Clears results table.  Sets header.
        public virtual void setQbTableHead() {
            this._dlg.PQbAnnualResult.Text = "Annual result: ";
            this.QbData.Clear();
            this.QbData.Columns[1].ColumnName = "Qb";
        }
        
        // Initialise post processing results tables.
        public virtual void initQResults() {
            this._dlg.PdQpStartMonth.Items.Clear();
            this._dlg.PQbStartMonth.Items.Clear();
            foreach (var month in Visualise._MONTHS) {
                DataRow rowq = QqData.NewRow();
                rowq[0] = month;
                QqData.Rows.Add(rowq);
                DataRow rowb = QbData.NewRow();
                rowb[0] = month;
                QbData.Rows.Add(rowb);
                this._dlg.PdQpStartMonth.Items.Add(month);
                this._dlg.PQbStartMonth.Items.Add(month);
            }
            this._dlg.PdQpStartMonth.SelectedIndex = -1;
            this._dlg.PQbStartMonth.SelectedIndex = -1;
        }
        
        // Calcualte Qq results.
        public virtual void calculateQq() {
            if (!this.setPeriods()) {
                return;
            }
            var startDate = new DateOnly(this.startYear, this.startMonth, this.startDay);
            var finishDate = new DateOnly(this.finishYear, this.finishMonth, this.finishDay);
            var flowDataTable = Visualise._EFLOWTABLE;
            if (!this._gv.db.hasDataConn(flowDataTable, this.conn)) {
                Utils.error(string.Format("Table {0} is missing or empty",flowDataTable), this._gv.isBatch);
                return;
            }
            var monthData = new Dictionary<int, List<double>> {
                {
                    1,
                    new List<double>()},
                {
                    2,
                    new List<double>()},
                {
                    3,
                    new List<double>()},
                {
                    4,
                    new List<double>()},
                {
                    5,
                    new List<double>()},
                {
                    6,
                    new List<double>()},
                {
                    7,
                    new List<double>()},
                {
                    8,
                    new List<double>()},
                {
                    9,
                    new List<double>()},
                {
                    10,
                    new List<double>()},
                {
                    11,
                    new List<double>()},
                {
                    12,
                    new List<double>()}};
            var selectString = "[mon], [day], [yr], [flo_out]";
            var where = string.Format("gis_id = {0}",this.subbasinOutletChannels[Convert.ToInt32(this._dlg.PQqSubbasin.SelectedItem.ToString())]);
            var sql = DBUtils.sqlSelect(flowDataTable, selectString, "", where);
            using (var reader = this.getReader(sql)) {
                while (reader.Read()) {
                    var thisDay = new DateOnly(reader.GetInt32(2), reader.GetInt32(0), reader.GetInt32(1));
                    if (thisDay < startDate || finishDate < thisDay) {
                        continue;
                    }
                    monthData[reader.GetInt32(0)].Add(reader.GetDouble(3));
                }
            }
            foreach (var (m, l) in monthData) {
                if (l.Count > 0) {
                    l.Sort();
                    var percentile = Visualise.percentile(l, (100 - (double)this._dlg.PQqSpin.Value) / 100);
                    this.QqData.Rows[m - 1][1] =  string.Format("{0:F2}", percentile);
                    //         QqStore = Utils.join(self._gv.resultsDir, 'q{0}.txt',self._dlg.QqSpin.Value))
                    //         with open(QqStore, 'w', newline='') as f:
                    //             for m, l in monthData:
                    //                 f.write(Visualise._MONTHS[m-1])
                    //                 f.write('\n')
                    //                 f.write(str(l))
                    //                 f.write('\n')
                }
            }
        }
        
        public virtual void saveQq() {
            string startDir;
            if (string.IsNullOrEmpty((string)this.QqData.Rows[0][1])) {
                Utils.information("Please calculate before saving", this._gv.isBatch);
                return;
            }
            if (this.lastQqResultsFile != "") {
                startDir = Path.GetDirectoryName(this.lastQqResultsFile);
            } else {
                var scenDir = Utils.join(this._gv.scenariosDir, this.scenario);
                startDir = Utils.join(scenDir, Parameters._RESULTS);
            }
            FileDialog dlg;
            bool append;
            var title = "Choose results file";
            var filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            if (this._dlg.PQqAppend.Checked) {
                dlg = new OpenFileDialog() {
                    Title = title,
                    Filter = filter,
                    InitialDirectory = startDir
                };
                append = true;
            } else {
                dlg = new SaveFileDialog() {
                    Title = title,
                    Filter = filter,
                    InitialDirectory = startDir
                };
                append = false;
            }
            var result = dlg.ShowDialog();
            if (result != DialogResult.OK) {
                return;
            }
            using (var f = new StreamWriter(resultsFile, append)) {
                var startDate = new DateOnly(this.startYear, this.startMonth, this.startDay).ToString(Utils._DATEFORMAT);
                var finishDate = new DateOnly(this.finishYear, this.finishMonth, this.finishDay).ToString(Utils._DATEFORMAT);
                f.WriteLine(string.Format("Q{0} for {1} to {2}", this._dlg.PQqSpin.Value, startDate, finishDate));
                var subbasin = this._dlg.PQqSubbasin.SelectedItem.ToString();
                f.WriteLine(string.Format("Subbasin {0};  Channel {1}", subbasin, this.subbasinOutletChannels[Convert.ToInt32(subbasin)]));
                foreach (var m in Enumerable.Range(0, 12)) {
                    f.Write(Visualise._MONTHS[m].PadRight(12));
                    f.WriteLine(QqData.Rows[m][1].ToString());
                }
                f.WriteLine("");
            }
            this.lastQqResultsFile = resultsFile;
        }
        
        // Clear dQp result.
        public virtual void cleardQpResult() {
            this._dlg.PdQpResult.Text = "Result:";
            this.dQpResult = -1;
        }
        
        // Enable or not the percentile setting and clear the result.
        public virtual void dQpButtons() {
            this._dlg.PdQpSpinP.Enabled = this._dlg.PdQpPercentile.Checked;
            this.cleardQpResult();
        }
        
        // Calculate dQp result.
        public virtual void calculatedQp() {
            if (!this.setPeriods()) {
                return;
            }
            var startDate = new DateOnly(this.startYear, this.startMonth, this.startDay);
            var finishDate = new DateOnly(this.finishYear, this.finishMonth, this.finishDay);
            var flowDataTable = Visualise._EFLOWTABLE;
            if (!this._gv.db.hasDataConn(flowDataTable, this.conn)) {
                Utils.error(string.Format("Table {0} is missing or empty",flowDataTable), this._gv.isBatch);
                return;
            }
            var startMonth = this._dlg.PdQpStartMonth.SelectedIndex + 1;
            if (startMonth <= 0) {
                Utils.information("Please choose start month", this._gv.isBatch);
                return;
            }
            // data has structure yearIndex -> flow value list
            var flowData = new Dictionary<int, List<double>>();
            var yearIndex = -1;
            var count = 0;
            var expectedLength = 0;
            // in case startmonth starts in output after day 1.  This data will be ignored.
            var currentFlowData = new List<double>();
            var selectString = "[mon], [day], [yr], [flo_out]";
            var where = string.Format("gis_id = {0}",this.subbasinOutletChannels[Convert.ToInt32(this._dlg.PdQpSubbasin.SelectedItem.ToString())]);
            var orderBy = "[yr], [jday]";
            var sql = DBUtils.sqlSelect(flowDataTable, selectString, orderBy, where);
            using (var reader = this.getReader(sql)) {
                while (reader.Read()) {
                    var mon = reader.GetInt32(0);
                    var day = reader.GetInt32(1);
                    var year = reader.GetInt32(2);
                    var val = reader.GetDouble(3);
                    var thisDay = new DateOnly(year, mon, day);
                    if (thisDay < startDate || finishDate < thisDay) {
                        continue;
                    }
                    if (year == this.startYear && mon < startMonth) {
                        continue;
                    }
                    if (mon == startMonth && day == 1) {
                        // start new year
                        count = 0;
                        expectedLength = startMonth <= 2 && Visualise.isLeap(year) || startMonth > 2 && Visualise.isLeap(year + 1) ? 366 : 365;
                        yearIndex = yearIndex + 1;
                        flowData[yearIndex] = new List<double>();
                        currentFlowData = flowData[yearIndex];
                    }
                    currentFlowData.Add(val);
                    count += 1;
                }
            }
            if (count < expectedLength) {
                // last year incomplete: delete it
                flowData.Remove(yearIndex);
            }
            if (flowData.Count == 0) {
                Utils.error(string.Format("There is insufficient data.  There must be at least a yesr starting from 1 {0} {1}.",this._dlg.PdQpStartMonth.SelectedItem.ToString(), this.startYear), this._gv.isBatch);
                return;
            }
            var d = (int)this._dlg.PdQpSpinD.Value;
            // compute minimum moving total for each year: no point in dividing by d until the end
            var totals = new List<double>();
            //         dQpStore = Utils.join(self._gv.resultsDir, '{0}Q{1}.txt',self._dlg.dQpSpinD.Value, self._dlg.dQpSpinP.Value))
            //         f =  open(dQpStore, 'w', newline='')
            foreach (var (yearIndx, flowVals) in flowData) {
                //             f.write('Year: {0}',self.startYear + yearIndx))
                //             f.write('\n')
                //             f.write('Flows out: \n')
                //             f.write(str(flowVals))
                //             f.write('\n')
                var currentTotal = flowVals.Take(d).Sum();  // initial total for first d values
                var minTotal = currentTotal;
                foreach (var i in Enumerable.Range(0, flowVals.Count - d)) {
                    var lastTotal = currentTotal;
                    currentTotal = lastTotal - flowVals[i] + flowVals[i + d];
                    if (currentTotal < minTotal) {
                        minTotal = currentTotal;
                    }
                }
                totals.Add(minTotal);
            }
            totals.Sort();
            //         f.write('Sorted totals:\n')
            //         f.write(str(totals))
            //         f.write('\n')
            // return percentile or mean according to final selection choice
            // remember we need to return a moving average, not a moving total
            if (this._dlg.PdQpPercentile.Checked) {
                var p = (int)this._dlg.PdQpSpinP.Value;
                var fraction = p / 100.0;
                this.dQpResult = Visualise.percentile(totals, fraction) / d;
                this._dlg.PdQpResult.Text = string.Format("Result: {0}Q{1} is {2:F2}", d, p, this.dQpResult);
            } else {
                this.dQpResult = (totals.Sum() / totals.Count) / d;
                this._dlg.PdQpResult.Text = string.Format("Result: {0}Qm is {1:F2}", d, this.dQpResult);
                //         f.write('Result: {0}',self.dQpResult))
                //         f.write('\n')
                //         f.close()
            }
        }
        
        public virtual void savedQp() {
            string startDir;
            if (this.dQpResult < 0) {
                Utils.information("Please calculate before saving", this._gv.isBatch);
                return;
            }
            if (this.lastdQpResultsFile != "") {
                startDir = Path.GetDirectoryName(this.lastdQpResultsFile);
            } else {
                var scenDir = Utils.join(this._gv.scenariosDir, this.scenario);
                startDir = Utils.join(scenDir, Parameters._RESULTS);
            }
            FileDialog dlg;
            bool append;
            var title = "Choose results file";
            var filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            if (this._dlg.PdQpAppend.Checked) {
                dlg = new OpenFileDialog() {
                    Title = title,
                    Filter = filter,
                    InitialDirectory = startDir
                };
                append = true;
            } else {
                dlg = new SaveFileDialog() {
                    Title = title,
                    Filter = filter,
                    InitialDirectory = startDir
                };
                append = false;
            }
            var result = dlg.ShowDialog();
            if (result != DialogResult.OK) {
                return;
            }
            using (var f = new StreamWriter(resultsFile, append)) {
                var startDate = new DateOnly(this.startYear, this.startMonth, this.startDay).ToString(Utils._DATEFORMAT);
                var finishDate = new DateOnly(this.finishYear, this.finishMonth, this.finishDay).ToString(Utils._DATEFORMAT);
                if (this._dlg.PdQpPercentile.Checked) {
                    f.WriteLine(string.Format("{0}Q{1} for {2} to {3}",this._dlg.PdQpSpinD.Value, this._dlg.PdQpSpinP.Value, startDate, finishDate));
                } else {
                    f.WriteLine(string.Format("{0}Qm for {1} to {2}",this._dlg.PdQpSpinD.Value, startDate, finishDate));
                }
                var subbasin = this._dlg.PdQpSubbasin.SelectedItem.ToString();
                var month = this._dlg.PdQpStartMonth.SelectedItem.ToString();
                f.WriteLine(string.Format("Subbasin {0};  Channel {1};  Starting in {2};  Result: {3:F2}",subbasin, this.subbasinOutletChannels[Convert.ToInt32(subbasin)], month, this.dQpResult));
                f.WriteLine("");
            }
            this.lastdQpResultsFile = resultsFile;
        }
        
        // Calculate Qb results.
        public virtual void calculateQb() {
            if (!this.setPeriods()) {
                return;
            }
            var startDate = new DateOnly(this.startYear, this.startMonth, this.startDay);
            var finishDate = new DateOnly(this.finishYear, this.finishMonth, this.finishDay);
            var flowDataTable = Visualise._EFLOWTABLE;
            if (!this._gv.db.hasDataConn(flowDataTable, this.conn)) {
                Utils.error(string.Format("Table {0} is missing or empty",flowDataTable), this._gv.isBatch);
                return;
            }
            var startMonth = this._dlg.PQbStartMonth.SelectedIndex + 1;
            if (startMonth <= 0) {
                Utils.information("Please choose start month", this._gv.isBatch);
                return;
            }
            // data has structure yearIndex -> flow value list
            var flowData = new Dictionary<int, List<double>>();
            var monthData = new Dictionary<int, List<double>> {
                {
                    1,
                    new List<double>()},
                {
                    2,
                    new List<double>()},
                {
                    3,
                    new List<double>()},
                {
                    4,
                    new List<double>()},
                {
                    5,
                    new List<double>()},
                {
                    6,
                    new List<double>()},
                {
                    7,
                    new List<double>()},
                {
                    8,
                    new List<double>()},
                {
                    9,
                    new List<double>()},
                {
                    10,
                    new List<double>()},
                {
                    11,
                    new List<double>()},
                {
                    12,
                    new List<double>()}};
            var yearIndex = -1;
            var count = 0;
            var expectedLength = 0;
            var selectString = "[mon], [day], [yr], [flo_out]";
            var where = string.Format("gis_id = {0}",this.subbasinOutletChannels[Convert.ToInt32(this._dlg.PQbSubbasin.SelectedItem.ToString())]);
            var orderBy = "[yr], [jday]";
            var sql = DBUtils.sqlSelect(flowDataTable, selectString, orderBy, where);
            // in case startmonth starts in output after day 1.  This data will be ignored.
            var currentFlowData = new List<double>();
            using (var reader = this.getReader(sql)) {
                while (reader.Read()) {
                    var mon = reader.GetInt32(0);
                    var day = reader.GetInt32(1);
                    var year = reader.GetInt32(2);
                    var val = reader.GetDouble(3);
                    var thisDay = new DateOnly(year, mon, day);
                    if (thisDay < startDate || finishDate < thisDay) {
                        continue;
                    }
                    if (year == this.startYear && mon < startMonth) {
                        continue;
                    }
                    if (mon == startMonth && day == 1) {
                        // start new year
                        count = 0;
                        expectedLength = startMonth <= 2 && Visualise.isLeap(year) || startMonth > 2 && Visualise.isLeap(year + 1) ? 366 : 365;
                        yearIndex = yearIndex + 1;
                        flowData[yearIndex] = new List<double>();
                        currentFlowData = flowData[yearIndex];
                    }
                    currentFlowData.Add(val);
                    count += 1;
                    monthData[mon].Add(val);
                }
            }
            if (count < expectedLength) {
                // last year incomplete: delete it
                flowData.Remove(yearIndex);
            }
            if (flowData.Count == 0) {
                Utils.error(string.Format("There is insufficient data.  There must be at least a year starting from 1 {0} {1}.",this._dlg.PQbStartMonth.SelectedItem.ToString(), this.startYear), this._gv.isBatch);
                return;
            }
            // compute minimum moving averages with highest rate of change for 1 to 100 days for each year
            var avs = new List<double>();
            foreach (var (yearIndx, flowVals) in flowData) {
                var numVals = flowVals.Count;
                var maxRateOfIncrease = 0.0;
                var avForMaxRateOfIncrease = 0.0;
                var lastAv = 0.0;
                // compute with increasing i so that last value for i - 1 available when doing i
                foreach (var i in Enumerable.Range(1, 100)) {
                    var minTotal = flowVals.Take(i).Sum();  // initial total for first i values
                    var lastTotal = minTotal;
                    foreach (var j in Enumerable.Range(0, numVals - i)) {
                        var currentTotal = lastTotal - flowVals[j] + flowVals[j + i];
                        if (currentTotal < minTotal) {
                            minTotal = currentTotal;
                        }
                        lastTotal = currentTotal;
                    }
                    var currentAv = minTotal / i;
                    if (lastAv > 0) {
                        // note this avoids i = 1 since lastAv initialised to zero, as well as preventing division by zero
                        var rateOfIncrease = (currentAv - lastAv) / lastAv;
                        if (rateOfIncrease > maxRateOfIncrease) {
                            maxRateOfIncrease = rateOfIncrease;
                            avForMaxRateOfIncrease = currentAv;
                        }
                    }
                    lastAv = currentAv;
                }
                avs.Add(avForMaxRateOfIncrease);
            }
            this.QbResult = avs.Sum() / avs.Count;
            //         # calculate Q85 for each month
            //         q85s = dict()
            //         minQ85 = float('inf')
            //         for m, l in monthData:
            //             if len(l) > 0:
            //                 l.sort()
            //                 q85 = Visualise.percentile(l, 0.85)
            //                 q85s[m] = q85
            //                 if q85 < minQ85:
            //                     minQ85 = q85
            // calculate mean for each month
            var means = new Dictionary<int, double>();
            var minMean = double.PositiveInfinity;
            foreach (var (m, l) in monthData) {
                if (l.Count > 0) {
                    var mean = l.Sum() / l.Count;
                    means[m] = mean;
                    if (mean < minMean) {
                        minMean = mean;
                    }
                }
            }
            this._dlg.PQbAnnualResult.Text = string.Format("Annual result: {0:F2}", this.QbResult);
            // result for each month is Qb * variation factor
            // variation factor is square root of ratio of Q85 for month to minimum Q85
            // replace above with sqaure root of monthly mean to minimal monthly mean
            //         for m, q85m in q85s:
            //             factor = 1 if minQ85 == 0 else math.sqrt(q85m / minQ85)
            foreach (var (m, mean) in means) {
                var factor = minMean == 0 ? 1 : Math.Sqrt(mean / minMean);
                var Qbm = this.QbResult * factor;
                this.QbData.Rows[m - 1][1] = string.Format("{0:F2}", Qbm);
            }
        }
        
        public virtual void saveQb() {
            string startDir;
            if (string.IsNullOrEmpty((string)this.QbData.Rows[0][1])) {
                Utils.information("Please calculate before saving", this._gv.isBatch);
                return;
            }
            if (this.lastQbResultsFile != "") {
                startDir = Path.GetDirectoryName(this.lastQbResultsFile);
            } else {
                var scenDir = Utils.join(this._gv.scenariosDir, this.scenario);
                startDir = Utils.join(scenDir, Parameters._RESULTS);
            }
            FileDialog dlg;
            bool append;
            var title = "Choose results file";
            var filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            if (this._dlg.PQbAppend.Checked) {
                dlg = new OpenFileDialog() {
                    Title = title,
                    Filter = filter,
                    InitialDirectory = startDir
                };
                append = true;
            } else {
                dlg = new SaveFileDialog() {
                    Title = title,
                    Filter = filter,
                    InitialDirectory = startDir
                };
                append = false;
            }
            var result = dlg.ShowDialog();
            if (result != DialogResult.OK) {
                return;
            }
            using (var f = new StreamWriter(resultsFile, append)) {
                var startDate = new DateOnly(this.startYear, this.startMonth, this.startDay).ToString(Utils._DATEFORMAT);
                var finishDate = new DateOnly(this.finishYear, this.finishMonth, this.finishDay).ToString(Utils._DATEFORMAT);
                f.WriteLine(string.Format("Qb for {0} to {1}",startDate, finishDate));
                var subbasin = this._dlg.PQbSubbasin.SelectedItem.ToString();
                var month = this._dlg.PQbStartMonth.SelectedItem.ToString();
                f.WriteLine(string.Format("Subbasin {0};  Channel {1};  Starting in {2}",subbasin, this.subbasinOutletChannels[Convert.ToInt32(subbasin)], month));
                f.WriteLine(string.Format("Annual      {0:F2}", this.QbResult));
                foreach (var m in Enumerable.Range(0, 12)) {
                    f.Write(Visualise._MONTHS[m].PadRight(12));
                    f.WriteLine(QbData.Rows[m][1].ToString());
                }
                f.WriteLine("");
            }
            this.lastQbResultsFile = resultsFile;
        }
        
        // 
        //         Find the percentile of a sorted list of values.
        //     
        //         N - is a list of values. Note N MUST BE already sorted.
        //         percent - a float value from 0.0 to 1.0.
        //     
        //         return - the percentile of the values
        //         
        public static double percentile(List<double> N, double percent) {
            if (N is null) {
                return 0;
            }
            var k = (N.Count - 1) * percent;
            var f = Math.Floor(k);
            var c = Math.Ceiling(k);
            if (f == c) {
                return N[Convert.ToInt32(k)];
            }
            var d0 = N[Convert.ToInt32(f)] * (c - k);
            var d1 = N[Convert.ToInt32(c)] * (k - f);
            return d0 + d1;
        }
        
        // Return units and description for var and table using connection to results database.
        public (string, string) getUnitsAndDescription(string table, string vari) {
            var sql = "SELECT [units], [description] FROM column_description WHERE table_name=? AND column_name=?";
            // units can be '---'
            var units = "";
            var description = "";
            using (var reader = this.getReader(sql, new object[] { table, vari })) {
                if (reader.HasRows) {
                    reader.Read();
                    units = reader.GetString(0);
                    if (units is null) { units = ""; } else if (units == "---") { units = ""; }
                    description = reader.GetString(1);
                    if (description is null) { description = ""; }
                }
            }
            //Utils.loginfo($"Table: {table}, column_name: {vari}")
            //Utils.loginfo($"Units: {units}, Description: {description}")
            return (units, description);
        }

        //execute a non-query sql command

        public void execNonQuery(string sql) {
            using (var cmd = new SQLiteCommand(sql, this.conn)) {
                cmd.ExecuteNonQuery();
            }
        }

        public SQLiteDataReader getReader(string sql, object[] parms = null) {
            using (var cmd = new SQLiteCommand(sql, this.conn)) {
                if (parms != null) {
                    for (int i = 1; i <= parms.Length; i++) { 
                        var val = parms[i - 1];
                        if (val == null) { val = DBNull.Value; }
                        cmd.Parameters.AddWithValue(string.Format("@p{0}", i), val);
                    }
                }
                return cmd.ExecuteReader();
            }
        }
    
    // TODO
    //// Item for displaying title at top left of map canvas.
    //public class MapTitle
    //    : QgsMapCanvasItem {
        
    //    public object boundingRectangle;
        
    //    public object layer;
        
    //    public string line0;
        
    //    public object line1;
        
    //    public object line2;
        
    //    public object metrics;
        
    //    public object normFont;
        
    //    public object rect01;
        
    //    public MapTitle(
    //        object conn,
    //        object canvas,
    //        string table,
    //        string title,
    //        object layer,
    //        object line2 = null) {
    //        object varIndex;
    //        //# normal font
    //        this.normFont = QFont();
    //        //# normal metrics object
    //        this.metrics = QFontMetricsF(this.normFont);
    //        // bold metrics object
    //        var boldFont = QFont();
    //        boldFont.setBold(true);
    //        var metricsBold = QFontMetricsF(boldFont);
    //        //# titled layer
    //        this.layer = layer;
    //        //# project line of title
    //        this.line0 = string.Format("Project: {0}",title);
    //        //# First line of title
    //        // replace var with description and units if available
    //        var items = layer.name().split();
    //        //for i in range(len(items)):
    //        //    Utils.loginfo('Item {0}: {1}',i+1, items[i]))
    //        // guard against user changing the layer name
    //        if (items.Count >= 2) {
    //            // check for scenario comparison layers, when var comes first and is followed by 'difference' or '%change'
    //            // items may be (for static results) scenario, variable, summary 
    //            // or (for scenario comparisons) variable, 'difference' or '%change', scenarios
    //            // or (for animation) scenario, variable
    //            if (items[1] == "difference" || items[1] == "%change") {
    //                varIndex = 0;
    //            } else {
    //                varIndex = 1;
    //            }
    //            (units, description) = Visualise.getUnitsAndDescription(table, items[varIndex], conn);
    //            if (varIndex == 0) {
    //                this.line1 = string.Format("{0} ({1})",description, units);
    //            } else {
    //                this.line1 = string.Format("{0} {1} ({2})",items[0], description, units);
    //            }
    //            // add the rest
    //            foreach (var i in Enumerable.Range(varIndex + 1, items.Count - (varIndex + 1))) {
    //                this.line1 += string.Format(" {0}",items[i]);
    //            }
    //        } else {
    //            // layer name changed - just copy it
    //            this.line1 = layer.name();
    //        }
    //        //# second line of title (or None)
    //        this.line2 = line2;
    //        var rect0 = metricsBold.boundingRect(this.line0);
    //        var rect1 = this.metrics.boundingRect(this.line1);
    //        //# bounding rectange of first 2 lines 
    //        this.rect01 = QRectF(0, rect0.top() + rect0.height(), Math.Max(rect0.width(), rect1.width()), rect0.height() + rect1.height());
    //        //# bounding rectangle
    //        this.boundingRectangle = null;
    //        if (line2 is null) {
    //            this.boundingRectangle = this.rect01;
    //        } else {
    //            this.updateLine2(line2);
    //        }
    //    }
        
    //    // Paint the text.
    //    public virtual void paint(object painter, object option, object widget = null) {
    //        // type: ignore # @UnusedVariable
    //        //         if self.line2 is None:
    //        //             painter.drawText(self.rect, Qt.AlignLeft, '{0}\n{1}',self.line0, self.line1))
    //        //         else:
    //        //             painter.drawText(self.boundingRectangle, Qt.AlignLeft, '{0}\n{1}\n{2}',self.line0, self.line1, self.line2))
    //        var text = QTextDocument();
    //        text.setDefaultFont(this.normFont);
    //        if (this.line2 is null) {
    //            text.setHtml(string.Format("<p><b>{0}</b><br/>{1}</p>",this.line0, this.line1));
    //        } else {
    //            text.setHtml(string.Format("<p><b>{0}</b><br/>{1}<br/>{2}</p>",this.line0, this.line1, this.line2));
    //        }
    //        text.drawContents(painter);
    //    }
        
    //    // def boundingRect(self) -> QRectF:
    //    //     """Return the bounding rectangle."""
    //    //     assert self.boundingRectangle is not None
    //    //     return self.boundingRectangle
    //    // Change second line.
    //    public virtual void updateLine2(string line2) {
    //        this.line2 = line2;
    //        var rect2 = this.metrics.boundingRect(this.line2);
    //        this.boundingRectangle = QRectF(0, this.rect01.top(), Math.Max(this.rect01.width(), rect2.width()), this.rect01.height() + rect2.height());
    //    }
        
    //    // Function used to identify a MapTitle object even when it has a wrapper.
    //    public virtual string identifyMapTitle() {
    //        return "MapTitle";
    //    }
    //}
        public void setMapTitle(Layer layer, string line2 = null) {
            // set map title referring to layer.  Must be called on MCT.
            // if layer is null just clears the graphic layer
            string elementName = "title";
            var mapTitleLayer = MapView.Active.Map.GetLayersAsFlattenedList()
                .OfType<GraphicsLayer>().FirstOrDefault();
            if (mapTitleLayer == null) {
                var glParams = new GraphicsLayerCreationParams();
                mapTitleLayer = LayerFactory.Instance.CreateLayer<GraphicsLayer>(glParams, MapView.Active.Map);
            }
            while (true) {
                var currentElement = mapTitleLayer.FindElement(elementName);
                if (currentElement != null) {
                    mapTitleLayer.RemoveElement(currentElement);
                } else { break; }
            }
            if (layer == null) { return; }
            mapTitleLayer.SetName("Map Title");
            mapTitleLayer.SetExpanded(true);
            //# project line of title
            string line0 = string.Format("Project: {0}\n", this.title);
            //# First line of title
            string line1 = layer.Name;
            // mapTitle text
            string mapTitle;
            if (line2 == null) {
                mapTitle = line0 + line1;
            } else {
                mapTitle = line0 + line1 + "\n" + line2;
            }
            var extent = MapView.Active.Extent;
            int yDiff = 800;
            if (line2 == null) { yDiff = 600; }
            MapPoint location = MapPointBuilderEx.CreateMapPoint(extent.XMin + 200, extent.YMax - yDiff);
            var elemInfo = new ElementInfo() {
                Anchor = Anchor.TopLeftCorner
            };
            var element = mapTitleLayer.AddElement(location, mapTitle, null, elementName, false, elemInfo);
        }

    }

}
 

