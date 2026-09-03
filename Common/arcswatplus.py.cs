


using ArcGIS.Core.Data;
using ArcGIS.Core.Data.Raster;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Internal.CIM;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Events;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.Framework;
using ArcGIS.Desktop.Internal.Framework.Utilities;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design.Behavior;
using System.Windows.Shapes;
using System.Xml;
using static System.Net.WebRequestMethods;
using LinearUnit = ArcGIS.Core.Geometry.LinearUnit;
using Path = System.IO.Path;

namespace ArcSWATPlus
{


    // ArcGIS Pro plugin to prepare geographic data for SWAT Editor.
    public class ArcSWATPlus
    {

        public bool _demIsProcessed;

        public GlobalVars _gv;

        public MainForm _odlg;

        public Delineation delin;

        public HRUs hrus;

        public bool loadFailed;

        public static Map mainMap;

        public static MapProjectItem mainMapItem;

        public string plugin_dir;

        public object translator;

        public Visualise vis;

        public ExportTable export;

        public static string @__version__ = getVersion();

        public ArcSWATPlus() {
            //object locale;
            // this import is a dependency on a Cython produuced .pyd file which will fail if the wrong architecture
            // and so gives an immediate exit before the plugin is loaded
            //# flag to show if init ran successfully
            this.loadFailed = false;
            // if not TYPE_CHECKING:
            //     try:
            //         from . import polygonizeInC2  # @UnusedImport @UnresolvedImport
            //     except Exception:
            //         Utils.loginfo("Failed to load Cython module: wrong architecture?")
            //         self.loadFailed = True
            //         return
            // uncomment next line for debugging
            // import pydevd; pydevd.settrace()
            // initialize plugin directory
            //# plugin directory
            //this.plugin_dir = os.path.dirname(@__file__);
            //// add to PYTHONPATH
            //sys.path.append(this.plugin_dir);
            //var settings = QSettings();
            // initialize locale
            // in testing with a dummy iface object this settings value can be None
            //try {
            //    locale = settings.value("locale/userLocale")[0:2:];
            //} catch (Exception) {
            //    locale = "en";
            //}
            //var localePath = os.path.join(this.plugin_dir, "i18n", "ArcSWAT_{}.qm", locale));
            //// set default behaviour for loading files with no CRS to prompt - the safest option
            //QSettings().Value = "Projections/defaultBehaviour", "prompt");
            ////# translator
            //if (System.IO.File.Exists(localePath)) {
            //    this.translator = QTranslator();
            //    this.translator.load(localePath);
            //    if (qVersion() > "4.3.3") {
            //        QCoreApplication.installTranslator(this.translator);
            //    }
            //}
            this._gv = null;
            // font = QFont("MS Shell Dlg 2", 8)
            // Create the dialog (after translation) and keep reference
            this._odlg = new MainForm(this);
            //self._odlg.setWindowFlags(self._odlg.windowFlags() & ~Qt.WindowContextHelpButtonHint & Qt.WindowMinimizeButtonHint)
            // TODO: this._odlg.Move(0, 0);
            //=======================================================================
            // font = self._odlg.font()
            // fm = QFontMetrics(font)
            // txt = "The quick brown fox jumps over the lazy dog."
            // family = font.family()
            // size = font.pointSize()
            // Utils.information("Family: {2}.  Point size: {3}.\nWidth of "{0}" is {1} pixels.", txt, fm.width(txt), family, size), False)
            //=======================================================================
            this._odlg.Text = String.Format("ArcSWAT+ {0}", ArcSWATPlus.@__version__);
            // flag used in initialising delineation form
            this._demIsProcessed = false;
            //# deineation window
            this.delin = null;
            //# create hrus window
            this.hrus = null;
            //# visualise window
            this.vis = null;
            // export table window
            this.export = null;
            Utils.openLog();
            // report version
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetEntryAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string fileVersion = String.Format("{0}.{1}.{2}", fvi.FileMajorPart, fvi.FileMinorPart, fvi.FileBuildPart);
            Utils.loginfo(String.Format("ArcGIS Pro version {0}; ArcSWAT+ version: {1}", fileVersion, ArcSWATPlus.@__version__));
        }

        // def initGui(self) -> None:
        //     """Create ArcSWAT button in the toolbar."""
        //     if self.loadFailed:
        //         return
        //     ## Action that will start plugin configuration
        //     self.action = QAction(
        //         QIcon(":/ArcSWAT/SWAT32.png"),
        //         u"ArcSWAT", self._iface.mainWindow())
        //     # connect the action to the run method
        //     self.action.triggered.connect(self.run)
        //
        //     # Add toolbar button and menu item
        //     self._iface.addToolBarIcon(self.action)
        //     self._iface.addPluginToMenu(u"&ArcSWAT", self.action)
        //
        // def unload(self) -> None:
        //     """Remove the ArcSWAT menu item and icon."""
        //     # allow for it not to have been loaded
        //     try:
        //         self._iface.removePluginMenu(u"&ArcSWAT", self.action)
        //         self._iface.removeToolBarIcon(self.action)
        //     except Exception:
        //         pass
        // Run ArcSWAT.

        private static string getVersion() {
            // this does not work: GetAddInInfos has no definition
            //var addInInfo = FrameworkApplication.GetAddInInfos().First(addIn => addIn.Name == "ArcSWAT3");
            //return addInInfo.Version.ToString();

            // this always return 1.0.0.0
            //var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            //var assemblyName = assembly.GetName();
            //return assemblyName.Version.ToString();

            // get version from config.daml.  Note this depends on daml being included in build (eg copy if newer)
            string version = "";
            XmlDocument xDoc = new XmlDocument();
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            string configDamlPath = Path.Combine(Path.GetDirectoryName(assembly.Location), "Config.daml");
            using (StreamReader streamReader = new StreamReader(configDamlPath)) {
                var daml = streamReader.ReadToEnd();
                xDoc.LoadXml(daml); // @"<?xml version=""1.0"" encoding=""utf - 8""?>" + 
            }
            XmlNodeList items = xDoc.GetElementsByTagName("AddInInfo");
            foreach (XmlNode xItem in items) {
                version = xItem.Attributes["version"].Value;
            }
            return version;
        }

        public async Task run() {

            // make sure we clear data from previous runs
            this.delin = null;
            this.hrus = null;
            this.vis = null;
            // initially only new/existing project buttons visible if project not set
            this._odlg.initButtons();
            this._odlg.Show();
            if (MapView.Active is not null) {
                await setupProject(false);
            }
        }

        // Create a new project.
        public async Task newProject() {
            var title = Utils.trans("Choose parent directory for project");
            string parentDir = "";
            using (FolderBrowserDialog dialog = new FolderBrowserDialog()) {
                dialog.Description = title;
                dialog.UseDescriptionForTitle = true;
                if (dialog.ShowDialog() == DialogResult.OK) {
                    parentDir = dialog.SelectedPath;
                } else { return; }
            }
            title = Utils.trans("Pleae provide a project name");
            string projName = Interaction.InputBox(title, "", "", 10, 20);
            if (string.IsNullOrEmpty(projName)) { return; }
            CreateProjectSettings settings = new CreateProjectSettings() {
                LocationPath = parentDir,
                Name = projName,
                TemplateType = TemplateType.Map
            };
            await Project.CreateAsync(settings);
            while (MapView.Active is null) {
                await Task.Delay(1000);
            }
            this._odlg.initButtons();
            await this.setupProject(false);
            this._gv.writeProjectConfig(0, 0);
        }

        // Open an existing project.
        public async Task existingProject() {
            var title = Utils.trans("Choose project (*.aprx) file");
            var filtr = Utils.trans("Project files (*.aprx)|*.aprx");
            string projPath = "";
            using (OpenFileDialog dlg = new OpenFileDialog()) {
                dlg.Title = title;
                dlg.Filter = filtr;
                dlg.RestoreDirectory = false;
                if (dlg.ShowDialog() == DialogResult.OK) {
                    projPath = dlg.FileName;
                } else { return; }
            }
            if (!string.IsNullOrEmpty(projPath)) {
                await Project.OpenAsync(projPath);
                while (MapView.Active is null) {
                    await Task.Delay(1000);
                }
                this._odlg.initButtons();
                this._odlg.setProject("Restarting project ...");
                await this.setupProject(false);
            }
        }

        // Set up the project.
        public async Task setupProject(
            bool isBatch,
            bool isHUC = false,
            bool isHAWQS = false,
            string logFile = null,
            bool fromGRASS = false,
            string TNCDir = "") {
            //this._odlg.mainBox.setVisible(true);
            //this._odlg.mainBox.setEnabled(false);
            //this._odlg.setCursor(Qt.WaitCursor);
            //this._odlg.projPath.setText("Restarting project ...");
            //Utils.information(string.Format("isHUC initially {0}", isHUC), isBatch);
            // now have project so initiate global vars
            // if we do this earlier we cannot for example find the project database
            this._gv = new GlobalVars(ArcSWATPlus.@__version__, isBatch, isHUC, isHAWQS, logFile);
            var title = this._gv.projName;
            Proj proj = this._gv.proj;
            bool found;
            // for isHUC etc parameters take precedence if set true
            if (isHUC) {
                proj.writeEntryBool(title, "delin/isHUC", isHUC);
            } else {
                (isHUC, found) = proj.readBoolEntry("", "delin/isHUC", false);
            }
            // # same as isHUC for isHAWQS
            if (isHAWQS) {
                proj.writeEntryBool(title, "delin/isHAWQS", isHAWQS);
            } else {
                (isHAWQS, found) = proj.readBoolEntry("", "delin/isHAWQS", false);
            }
            // # same as isHUC for fromGRASS
            if (fromGRASS) {
                proj.writeEntryBool(title, "delin/fromGRASS", fromGRASS);
            } else {
                (fromGRASS, found) = proj.readBoolEntry("", "delin/fromGRASS", false);
            }
            //TODO
            //if (!string.IsNullOrEmpty(TNCDir)) {
            //    proj.writeEntry(title, "delin/TNCDir", TNCDir);
            //} else { 
            //    (TNCDir, found) = proj.readEntry("", "delin/TNCDir", "");
            //}
            //this._gv.plugin_dir = this.plugin_dir;
            //this._odlg.projPath.repaint();
            this._odlg._gv = this._gv;
            this._odlg.checkReports();
            // identify the main project map
            var mapItems = Project.Current.GetItems<MapProjectItem>();
            int mapCount = mapItems.Count();
            if (mapCount == 1) {
                // maybe new project.  Store link to only map
                ArcSWATPlus.mainMapItem = mapItems.First();
                ArcSWATPlus.mainMap = await QueuedTask.Run(mainMapItem.GetMap);
            } else {
                foreach (var mapItem in mapItems) {
                    if (mapItem.Name.Equals("Map", StringComparison.CurrentCultureIgnoreCase)) {
                        ArcSWATPlus.mainMapItem = mapItem;
                        ArcSWATPlus.mainMap = await QueuedTask.Run(mapItem.GetMap);
                        break;
                    }
                }
            }
            // make mainMap active
            IMapPane mapViewPane = ProApp.Panes.OfType<IMapPane>().FirstOrDefault((p) => p.Caption == "Map");
            if (mapViewPane != null) {
                // activate the MapPane
                (mapViewPane as Pane).Activate();
            }
            ProjectItemRemovingEvent.Subscribe(OnItemRemoval);
            await setLegendGroups();
            // enable edit button if converted from Arc
            int choice;
            (choice, found) = proj.readNumEntry(title, "delin/fromArc", -1);
            if (found) {
                if (choice == 2) {   // NB values from convertFromArc.py, 0 for full, 1 for existing, 2 for no gis.
                    this._odlg.allowEdit();
                }
            }
            // also assume editor can be run if there are stream and hrus shapefiles in the results directory
            if (System.IO.File.Exists(Utils.join(this._gv.resultsDir, Parameters._RIVS + ".shp")) &&
                System.IO.File.Exists(Utils.join(this._gv.resultsDir, Parameters._SUBS + ".shp"))) {
                this._odlg.allowEdit();
            }
            this._gv.useGridModel = proj.readBoolEntry(title, "delin/useGridModel", false).Item1;
            if (this._gv.useGridModel)
                this._gv.gridSize = proj.readNumEntry(title, "delin/gridSize", 1).Item1;
            //Add ArcSWATPlus styles to project
            string path = Path.Combine(this._gv.addinPath, "ArcSWATPlus.stylx");
            var styleItem = ItemFactory.Instance.Create(path) as IProjectItem;
            await QueuedTask.Run(() => Project.Current.AddItem(styleItem));
            //ARCSWATPlus custom style
            // name seems sometimes to include suffix and sometimes not, so allow for both
            this._gv.arcSWATPlusStyle =
                Project.Current.GetItems<StyleProjectItem>().FirstOrDefault(s => s.Name.StartsWith("ArcSWATPlus") && System.IO.File.Exists(s.Path));
            if (this._gv.arcSWATPlusStyle == null) {
                Utils.error("Cannot find ArcSWATPlus style file", this._gv.isBatch);
                return;
            }
            Utils.loginfo(String.Format("Style file is {0}", this._gv.arcSWATPlusStyle.Path));
  
            if (await this.demProcessed(proj)) {
                this._demIsProcessed = true;
                this._odlg.allowCreateHRU();
                var hrus = new HRUs(this._gv, this._odlg.reportsCombo, this);
                if (hrus.HRUsAreCreated()) {
                    this._odlg.showReports();
                    this._odlg.allowEdit();
                }
            }
            var outputDb = Utils.join(this._gv.resultsDir, Parameters._OUTPUTDB);
            if (System.IO.File.Exists(outputDb)) {
                this._odlg.allowVisualise();
                // TODO: this.loadVisualisationLayers();
            }
            this._odlg.setProject(this._gv.projDir);
            //this._odlg.mainBox.setEnabled(true);
        }

        public Task OnItemRemoval(ProjectItemRemovingEventArgs args) {
            Item[] arg = args.ProjectItems;
            foreach (Item item in arg) {
                if (item.ToString().StartsWith("<MapProjectItem Map@")) {
                    Utils.error("You are trying to delete the main map", false);
                    args.Cancel = true;
                }
            }
            return Task.CompletedTask;
        }

        // Run parameters form.
        public void runParams() {
            var @params = new Parameters(this._gv);
            @params.run();
        }

        // Run the delineation dialog.
        public void doDelineation() {
            //Debug.Assert(this._gv is not null);
            // avoid getting second window
            if (this.delin is not null && this.delin._dlg.Enabled) {
                this.delin.doClose();
            }
            this.delin = new Delineation(this._gv, this._demIsProcessed, this);
            //Debug.Assert(this.delin is not null);
            this.delin.run();
        }

        public void postDelineation(int result) {
            if (result == 1 && this._gv.isDelinDone()) {
                this._demIsProcessed = true;
                this._odlg.allowCreateHRU();
                // remove old data so cannot be reused
                var basinsdataTable = this._gv.isHUC || this._gv.isHAWQS ? "BASINSDATAHUC" : "BASINSDATA";
                this._gv.db.clearTable(basinsdataTable);
                // make sure HRUs starts from scratch
                if (this.hrus is not null && this.hrus._dlg.Enabled) {
                    this.hrus._dlg.Close();
                }
                this.hrus = null;
            } else {
                if (result == 0) {
                    this._demIsProcessed = false;
                    this._odlg.initButtons();
                }
            }
            //this._odlg.BringToFront();
        }

        // Run the HRU creation dialog.
        public async Task doCreateHRUs() {
            //Debug.Assert(this._gv is not null);
            // avoid getting second window
            if (this.hrus is not null && this.hrus._dlg.Enabled) {
                this.hrus._dlg.Close();
            }
            this.hrus = new HRUs(this._gv, this._odlg.reportsCombo, this);
            //Debug.Assert(this.hrus is not null);
            // non modal
            await this.hrus.run();
        }

        public void postCreateHRUs(int result) {
            if (result == 1) { 
                this._odlg.allowEdit();
            }
        }

        // 
        //         Return true if we can proceed with HRU creation.
        //         
        //         Return false if any required project setting is not found 
        //         in the project file
        //         Return true if:
        //         Using existing watershed and watershed grid exists and 
        //         is newer than dem
        //         or
        //         Not using existing watershed and filled dem exists and 
        //         is no older than dem, and
        //         watershed shapefile exists and is no older than filled dem
        //         
        public async Task<bool> demProcessed(Proj proj) {
            using (new CursorWait()) {
                var title = this._gv.projName;
                string demFile;
                bool found;
                (demFile, found) = proj.readEntry(title, "delin/DEM", "");
                if (!found || string.IsNullOrEmpty(demFile) || !System.IO.File.Exists(demFile)) {
                    Utils.loginfo("demProcessed failed: no DEM");
                    return false;
                }
                var demLayer = (await Utils.getLayerByFilename(demFile, FileTypes._DEM, this._gv, null, Utils._WATERSHED_GROUP_NAME)).Item1 as RasterLayer;
                if (demLayer is null) {
                    Utils.loginfo("demProcessed failed: no DEM layer");
                    return false;
                }
                this._gv.demFile = demFile;
            	dynamic nodata = null;
                await QueuedTask.Run(() => {
                    // set extent to DEM as otherwise defaults to full globe
                    var demExtent = demLayer.QueryExtent();
                    var map = ArcSWATPlus.mainMap;
                    map.SetCustomFullExtent(demExtent);
                	nodata = demLayer.GetRaster().GetNoDataValue();
                	if (nodata is not null) { 
                    	var typ = nodata.GetType();
                    	if (typ.IsArray) {
                        	this._gv.elevationNoData = Convert.ToDouble(nodata[0]);
                    	} else {
                        	this._gv.elevationNoData = Convert.ToDouble(nodata);
                    	}
                	}
                });

            if (nodata is null) {
                Utils.loginfo("demProcessed failed: dem does not have a nodata value");
                return false;
            }
            var crsProject = await QueuedTask.Run(() => demLayer.GetSpatialReference());
            if (!crsProject.IsProjected) { 
                Utils.loginfo(string.Format("demProcessed failed: DEM is not projected"));
                    return false;
                }
                var XYSizes = await QueuedTask.Run<Tuple<double, double>>(() => {
                    return demLayer.GetRaster().GetMeanCellSize();
                });
            	var units = crsProject.Unit;
                var (factor, OK) = Utils.getHorizontalFactor(units);
                if (!OK) {
                    Utils.loginfo(string.Format("demProcessed failed: units are {0}", units));
                    return false;
                }
                this._gv.cellArea = XYSizes.Item1 * XYSizes.Item2 * factor * factor;
                // hillshade
                //Delineation.addHillshade(demFile, root, demLayer, this._gv);
                string outletFile;
                (outletFile, found) = proj.readEntry(title, "delin/outlets", "");
                FeatureLayer outletLayer = null;
                if (found && !string.IsNullOrEmpty(outletFile) && System.IO.File.Exists(outletFile)) {
                    var ft = this._gv.isHUC || this._gv.isHAWQS ? FileTypes._OUTLETSHUC : FileTypes._OUTLETS;
                    outletLayer = (await Utils.getLayerByFilename(outletFile, ft, this._gv, null, Utils._WATERSHED_GROUP_NAME)).Item1 as FeatureLayer;
                    if (outletLayer is null) {
                        Utils.loginfo("demProcessed failed: no outlet layer");
                        return false;
                    }
                }
                this._gv.outletFile = outletFile;
                this._gv.existingWshed = proj.readBoolEntry(title, "delin/existingWshed", false).Item1;
                this._gv.useGridModel = proj.readBoolEntry(title, "delin/useGridModel", false).Item1;
                this._gv.useLandscapes = proj.readBoolEntry(title, "lsu/useLandscapes", false).Item1;
                string streamFile;
                (streamFile, found) = proj.readEntry(title, "delin/net", "");
                FeatureLayer streamLayer = null;
                if (this._gv.useGridModel || this._gv.isHUC || this._gv.isHAWQS || !this._gv.existingWshed) {
                    if (!found || string.IsNullOrEmpty(streamFile) || !System.IO.File.Exists(streamFile)) {
                        Utils.loginfo("demProcessed failed: no stream reaches shapefile");
                        return false;
                    }
                    var ft = this._gv.useGridModel ? FileTypes._GRIDSTREAMS : FileTypes._STREAMS;
                    streamLayer = (await Utils.getLayerByFilename(streamFile, ft, this._gv, null, Utils._WATERSHED_GROUP_NAME)).Item1 as FeatureLayer;
                    if (streamLayer is null) {
                        Utils.loginfo("demProcessed failed: no streams layer");
                        return false;
                    }
                    this._gv.streamFile = streamFile;
                }
                string channelFile = "";
                FeatureLayer channelLayer = null;
                if (this._gv.useGridModel) {
                    (this._gv.gridSize, found) = proj.readNumEntry(title, "delin/gridSize", 0);
                    if (!found || this._gv.gridSize <= 0) {
                        Utils.loginfo("demProcessed failed: grid size not set");
                        return false;
                    }
                } else {
                    (channelFile, found) = proj.readEntry(title, "delin/channels", "");
                    if (!found || string.IsNullOrEmpty(channelFile) || !System.IO.File.Exists(channelFile)) {
                        Utils.loginfo("demProcessed failed: no channels shapefile");
                        return false;
                    }
                    channelLayer = (await Utils.getLayerByFilename(channelFile, FileTypes._CHANNELS, this._gv, null, Utils._WATERSHED_GROUP_NAME)).Item1 as FeatureLayer;
                    if (channelLayer is null) {
                        Utils.loginfo("demProcessed failed: no channels layer");
                        return false;
                    }
                    this._gv.channelFile = channelFile;
                }
                string subbasinsFile;
                FeatureLayer subbasinsLayer = null;
                (subbasinsFile, found) = proj.readEntry(title, "delin/subbasins", "");
                if (!found || string.IsNullOrEmpty(subbasinsFile) || !System.IO.File.Exists(subbasinsFile)) {
                    Utils.loginfo("demProcessed failed: no subbasins shapefile");
                    return false;
                }
                var subbasinsTime = System.IO.File.GetLastWriteTime(subbasinsFile);
                subbasinsLayer = (await Utils.getLayerByFilename(subbasinsFile, FileTypes._SUBBASINS, this._gv, null, Utils._WATERSHED_GROUP_NAME)).Item1 as FeatureLayer;
                if (subbasinsLayer is null) {
                    Utils.loginfo("demProcessed failed: no subbasins layer");
                    return false;
                }
                this._gv.subbasinsFile = subbasinsFile;
                this._gv.subsNoLakesFile = proj.readEntry(title, "delin/subsNoLakes", "").Item1;
                string wshedFile;
                FeatureLayer wshedLayer = null;
                if (!this._gv.useGridModel) {
                    (wshedFile, found) = proj.readEntry(title, "delin/wshed", "");
                    if (!found || string.IsNullOrEmpty(wshedFile) || !System.IO.File.Exists(wshedFile)) {
                        Utils.loginfo("demProcessed failed: no wshed shapefile");
                        return false;
                    }
                    if (this._gv.existingWshed) {
                        wshedLayer = (await Utils.getLayerByFilename(wshedFile, FileTypes._EXISTINGWATERSHED, this._gv, null, Utils._WATERSHED_GROUP_NAME)).Item1 as FeatureLayer;
                        if (wshedLayer is null) {
                            Utils.loginfo("demProcessed failed: no wshed layer");
                            return false;
                        }
                    }
                    this._gv.wshedFile = wshedFile;
                }
                var @base = Path.ChangeExtension(demFile, null);
                string burnFile;
                if (!this._gv.existingWshed) {
                    (burnFile, found) = proj.readEntry(title, "delin/burn", "");
                    if (found && !string.IsNullOrEmpty(burnFile)) {
                        if (!System.IO.File.Exists(burnFile)) {
                            Utils.loginfo("demProcessed failed: no burn file");
                            return false;
                        }
                    }
                }
                this._gv.slopeFile = @base + "slp.tif";
                if (!System.IO.File.Exists(this._gv.slopeFile)) {
                    Utils.loginfo("demProcessed failed: no slope raster");
                    return false;
                }
                this._gv.basinFile = @base + "wStream.tif";
                //if (this._gv.useGridModel) {
                //    this._gv.isBig = wshedLayer.featureCount() > 100000 || this._gv.forTNC;
                //    Utils.loginfo("isBig is {0}", this._gv.isBig));
                //}
                if (!this._gv.useGridModel) {
                    this._gv.channelBasinFile = @base + "wChannel.tif";
                    this._gv.srcChannelFile = @base + "srcChannel.tif";
                }
                var streamDrainage = proj.readBoolEntry(title, "delin/streamDrainage", false).Item1;
                string ad8File;
                if (this._gv.existingWshed) {
                    if (!this._gv.useGridModel) {
                        if (!System.IO.File.Exists(this._gv.basinFile)) {
                            Utils.loginfo("demProcessed failed: no subbasins raster");
                            return false;
                        }
                    }
                } else {
                    this._gv.pFile = @base + "p.tif";
                    if (!System.IO.File.Exists(this._gv.pFile)) {
                        Utils.loginfo("demProcessed failed: no p raster");
                        return false;
                    }
                    if (!this._gv.fromGRASS) {
                        var felFile = @base + "fel.tif";
                        if (!System.IO.File.Exists(felFile)) {
                            Utils.loginfo("demProcessed failed: no filled raster");
                            return false;
                        }
                        ad8File = @base + "ad8.tif";
                        if (!System.IO.File.Exists(ad8File)) {
                            Utils.loginfo("demProcessed failed: no D8 accumulation raster");
                            return false;
                        }
                        var demTime = System.IO.File.GetLastWriteTime(demFile);
                        var felTime = System.IO.File.GetLastWriteTime(felFile);
                        if (!(demTime <= felTime && felTime <= subbasinsTime)) {
                            Utils.loginfo("demProcessed failed: not up to date");
                            return false;
                        }
                    }
                    this._gv.distStFile = @base + "distst.tif";
                    if (!System.IO.File.Exists(this._gv.distStFile)) {
                        Utils.loginfo("demProcessed failed: no distance to outlet raster");
                        return false;
                    }
                    this._gv.distChFile = @base + "distch.tif";
                    if (!this._gv.useGridModel) {
                        if (!System.IO.File.Exists(this._gv.distChFile)) {
                            Utils.loginfo("demProcessed failed: no distance to channel raster");
                            return false;
                        }
                    }
                    string valleyDepthsFile = @base + "feldepths.tif";
                    if (System.IO.File.Exists(valleyDepthsFile)) {
                        this._gv.valleyDepthsFile = valleyDepthsFile;
                    }
                }
                if (!this._gv.useGridModel) {
                    if (!System.IO.File.Exists(this._gv.channelBasinFile)) {
                        Utils.loginfo("demProcessed failed: no channel basins raster");
                        return false;
                    }
                }
                string snapFile;
                (snapFile, found) = proj.readEntry(title, "delin/snapOutlets", "");
                if (found && !string.IsNullOrEmpty(snapFile) && System.IO.File.Exists(snapFile)) {
                    this._gv.snapFile = snapFile;
                } else {
                    snapFile = "";
                }
                string lakeFile;
                FeatureLayer lakeLayer = null;
                (lakeFile, found) = proj.readEntry(title, "delin/lakes", "");
                if (found && !string.IsNullOrEmpty(lakeFile) && System.IO.File.Exists(lakeFile)) {
                    this._gv.lakeFile = lakeFile;
                    lakeLayer = (await Utils.getLayerByFilename(lakeFile, FileTypes._LAKES, this._gv, null, Utils._WATERSHED_GROUP_NAME)).Item1 as FeatureLayer;
                    if (this._gv.useGridModel) {
                        var gridLakesAdded = proj.readBoolEntry(title, "delin/gridLakesAdded", false).Item1;
                        if (!gridLakesAdded) {
                            Utils.loginfo("demProcessed failed: grid lakes not added");
                            return false;
                        }
                    } else {
                        var chBasinNoLakeFile = @base + "wChannelNoLake.tif";
                        if (System.IO.File.Exists(chBasinNoLakeFile)) {
                            this._gv.chBasinNoLakeFile = chBasinNoLakeFile;
                            if (!this._gv.existingWshed) {
                                var lakePointsAdded = proj.readBoolEntry(title, "delin/lakePointsAdded", false).Item1;
                                if (!lakePointsAdded) {
                                    Utils.loginfo("demProcessed failed: lake points not added");
                                    return false;
                                }
                            }
                        } else if (!(this._gv.isHUC || this._gv.isHAWQS)) { // which use channelBasins file with lakes already removed
                            Utils.loginfo("demProcessed failed: no channel basins without lakes raster");
                            return false;
                        }
                    }
                    var playaFile = System.IO.Path.ChangeExtension(this._gv.demFile, null) + "playa.tif";
                    if (Utils.isUpToDate(lakeFile, playaFile)) {
                        this._gv.playaFile = playaFile;
                    }
                }
                //Raster ad8Raster = null;
                var outFile = snapFile == "" ? outletFile : this._gv.snapFile;
                var chanFile = this._gv.useGridModel ? streamFile : channelFile;
                //if (!this._gv.existingWshed) {
                //    await QueuedTask.Run(() => {
                //        // Create a FileSystemConnectionPath using the folder path.
                //        FileSystemConnectionPath connectionPath = new FileSystemConnectionPath(new System.Uri(this._gv.demDir), FileSystemDatastoreType.Raster);
                //        // Create a new FileSystemDatastore using the FileSystemConnectionPath.
                //        FileSystemDatastore dataStore = new FileSystemDatastore(connectionPath);
                //        // Open the raster datasets.
                //        // calculate statistics for ad8File
                //        var parms = Geoprocessing.MakeValueArray(ad8File);
                //        Utils.runPython("runStatistics.py", parms, this._gv);
                //        RasterDataset ad8Ds = dataStore.OpenDataset<RasterDataset>(Path.GetFileName(ad8File));
                //        ad8Raster = ad8Ds.CreateFullRaster();
                //    });

                if (!await this._gv.topo.setUp0(demLayer, chanFile, outFile, null, this._gv)) {
                    return false;
                }
                var basinIndex = await this._gv.topo.getIndex(subbasinsLayer, Topology._POLYGONID);
                if (basinIndex < 0) {
                    Utils.loginfo("demProcessed failed: no PolygonId field in subbasins file");
                    return false;
                }
                //var basinNoIndex = await this._gv.topo.getIndex(channelLayer, Topology._BASINNO);
                //if (basinNoIndex < 0) {
                //    Utils.loginfo("demProcessed failed: no BasinNo field in channels file");
                //    return false;
                //}
                //int count = 1;
                await QueuedTask.Run(() => {
                    using (RowCursor rowCursor = subbasinsLayer.Search(null)) {
                        while (rowCursor.MoveNext()) {
                            using (var polygon = rowCursor.Current as ArcGIS.Core.Data.Feature) {
                                var basin = Convert.ToInt32(polygon[basinIndex]);
                                //Utils.loginfo("A");
                                var shape = polygon.GetShape();
                                //Utils.loginfo("B");
                                var centroid = GeometryEngine.Instance.Centroid(shape);
                                //Utils.loginfo("C");
                                double[] centre = new double[3] { centroid.X, centroid.Y, 0 };
                                //Utils.loginfo("D");
                                this._gv.topo.basinCentroids[basin] = centre;
                                //Utils.loginfo(string.Format("{0}", count));
                                //count += 1;
                            }
                        }
                    }
                });
                //Utils.loginfo("Finished centroids");
                if (lakeLayer is not null) {
                    if (!this._gv.topo.readLakesData(this._gv.db)) {
                        Utils.loginfo("demProcessed failed: lakes data not read");
                        return false;
                    }
                }
                // this can go wrong if eg the streams and watershed files exist but are inconsistent
                try {
                    if (!await this._gv.topo.setUp(demLayer, chanFile, subbasinsFile, outFile, lakeFile, this._gv, this._gv.existingWshed, false, this._gv.useGridModel, streamDrainage, false)) {
                        // type: ignore
                        Utils.loginfo("demProcessed failed: topo setup failed");
                        return false;
                    }
                    if (this._gv.topo.inlets.Count == 0) {
                        // no inlets, so no need to expand subbasins layer legend
                        //Debug.Assert(subbasinsLayer is not null);
                        if (subbasinsLayer != null) {
                            await QueuedTask.Run(() => subbasinsLayer.SetExpanded(false));
                        }
                    }
                }
                catch (Exception ex) {
                    Utils.loginfo(string.Format("demProcessed failed: topo setup raised exception: {0}", ex.Message));
                    return false;
                }
                return true;
            }
        }



        // Legend groups are used to keep legend in reasonable order.  
        //         Create them if necessary.
        //         
        public static async Task setLegendGroups() {
            var map = ArcSWATPlus.mainMap;
            await QueuedTask.Run(async () => {
                var groups = new List<string> {
                    Utils._SLOPE_GROUP_NAME,
                    Utils._SOIL_GROUP_NAME,
                    Utils._LANDUSE_GROUP_NAME,
                    Utils._WATERSHED_GROUP_NAME
                };
                foreach (string name in groups) {
                    var layer = Utils.getGroupLayerByName(name);
                    if (layer is null) {
                        LayerFactory.Instance.CreateGroupLayer(map, 0, name);
                        //layer.SetExpanded(true);
                    }
                }
                // clear any animations left accidentally
                await Utils.clearAnimationMaps();
                // don't do this - hides landuse, soil and slope
                //// move world hillshade layer to base of watershed group
                //GroupLayer wshedGroup = Utils.getGroupLayerByName(Utils._WATERSHED_GROUP_NAME);
                //if (wshedGroup is not null) {
                //    List<Layer> layers = MapView.Active.Map.GetLayersAsFlattenedList().Where(layer => layer.Name == "World Hillshade").ToList();
                //    if (layers.Count > 0 ) {
                //        map.MoveLayer(layers[0], wshedGroup, -1);
                //    }
                //}
            });
        }

        // Start the SWAT+ Editor
        public void startEditor() {
            Debug.Assert(this._gv is not null);
            var editor = this._gv.findSWATPlusEditor();
            if (editor is null) { return; }
            //close databases
            if (this._gv.db is not null) {
                if (this._gv.db.conn is not null) {
                    this._gv.db.conn.Close();
                    this._gv.db.conn.Dispose();
                    this._gv.db.conn = null;
                    Utils.loginfo("Project database closed");
                }
                if (this._gv.db.connRef is not null) {
                    this._gv.db.connRef.Close();
                    this._gv.db.connRef.Dispose();
                    this._gv.db.connRef = null;
                    Utils.loginfo("Reference database closed");
                }
                if (this._gv.db.SSURGOConn is not null) {
                    this._gv.db.SSURGOConn.Close();
                    this._gv.db.SSURGOConn.Dispose();
                    this._gv.db.SSURGOConn = null;
                    Utils.loginfo("SSURGO database closed");
                }
            }
            Utils.loginfo(string.Format("Starting SWAT+ editor with command: {0} {1}", editor, this._gv.db.dbFile));
            var process = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = editor
                }
            };
            process.StartInfo.ArgumentList.Add(this._gv.db.dbFile);
            process.Start();
            process.WaitForExit();
            // reopen project database
            if (this._gv.db is not null) {
                this._gv.db.connectToProjectDatabase();
            }
            if (System.IO.File.Exists(Utils.join(this._gv.resultsDir, Parameters._OUTPUTDB))) {
                this._odlg.allowVisualise();
            }
        }

        // Close the database connections and subsidiary forms.
        public async Task finish(DialogResult result) {
            if (result == DialogResult.OK) {
                if (Project.Current.HasEdits) {
                    await Project.Current.DiscardEditsAsync();
                }
                await Project.Current.SaveAsync();
            }
            Utils.loginfo("Closing databases");
            try {
                this.delin = null;
                this.hrus = null;
                if (this.vis is not null && this.vis.conn is not null) {
                    this.vis.conn.Close();
                }
                this.vis = null;
                this.export = null;
                if (this._gv is not null && this._gv.db is not null) {
                    if (this._gv.db.conn is not null) {
                        this._gv.db.conn.Close();
                    }
                    if (this._gv.db.connRef is not null) {
                        this._gv.db.connRef.Close();
                        if ((this._gv.isHUC || this._gv.isHAWQS) && this._gv.db.SSURGOConn is not null) {
                            this._gv.db.SSURGOConn.Close();
                        }
                    }
                }
                Utils.loginfo("Databases closed");
            }
            catch (Exception) {
            }
        }

        // Run visualise form.
        public async Task visualise() {
            // avoid getting second window
            if (this.vis is not null) {
                await this.vis.doClose();
            }
            this.vis = new Visualise(this._gv);
            Debug.Assert(this.vis is not null);
            await this.vis.run();
            this.vis = null;
        }

        public void exportTable() {
            if (this.export is not null) {
                this.export.close();
            }
            this.export = new ExportTable(this._gv);
            this.export.run();
        }

        //// If we have subs1.shp and riv1.shp and an empty watershed group then add these layers.
        ////         
        ////         Intended for use after a no gis conversion from ArcSWAT.
        //public virtual void loadVisualisationLayers() {
        //    Debug.Assert(this._gv is not null);
        //    var root = QgsProject.instance().layerTreeRoot();
        //    var wshedLayers = Utils.getLayersInGroup(Utils._WATERSHED_GROUP_NAME, root);
        //    // ad layers if we have empty Watershed group
        //    var addLayers = wshedLayers.Count == 0;
        //    if (addLayers) {
        //        var wshedFile = os.path.join(this._gv.shapesDir, "subs1.shp");
        //        var streamFile = os.path.join(this._gv.shapesDir, "riv1.shp");
        //        if (System.IO.File.Exists(wshedFile) && System.IO.File.Exists(streamFile)) {
        //            var group = root.findGroup(Utils._WATERSHED_GROUP_NAME);
        //            if (group is not null) {
        //                var proj = QgsProject.instance();
        //                var wshedLayer = QgsVectorLayer(wshedFile, "Subbasins", "ogr");
        //                wshedLayer = cast(QgsVectorLayer, proj.addMapLayer(wshedLayer, false));
        //                group.insertLayer(0, wshedLayer);
        //                // style file like wshed.qml but does not check for subbasins upstream frm inlets
        //                wshedLayer.loadNamedStyle(Utils.join(this._gv.plugin_dir, "wshed2.qml"));
        //                var streamLayer = QgsVectorLayer(streamFile, "Streams", "ogr");
        //                streamLayer = cast(QgsVectorLayer, proj.addMapLayer(streamLayer, false));
        //                group.insertLayer(0, wshedLayer);
        //                streamLayer.loadNamedStyle(Utils.join(this._gv.plugin_dir, "stream.qml"));
        //            }
        //        }
        //    }
        //}
    }
}
    
