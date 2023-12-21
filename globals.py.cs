



using System.Collections.Generic;

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml.Linq;

using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Mapping;
using System.Data.SQLite;
using System.Reflection;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Desktop.Internal.Catalog;
using System.Runtime.ConstrainedExecution;

namespace ArcSWATPlus
{

        // Data used across across the plugin, and some utilities on it.
        public class GlobalVars {

        public int fontSize;

            public string addinPath;

            public StyleProjectItem arcSWATPlusStyle;

            public object aboutPos;

            public string actHRUsFile;

        public string actLSUsFile;

        public string ad8File;

        public string animationDir;

        public int animationGroupIndex;

        public string basinFile;

            public int basinNoData;

            public string burnedDemFile;

            public string burnFile;

        public double burninDepth;

            public double cellArea;

        public string channelBasinFile;

        public double channelDepthExponent;

        public double channelDepthMultiplier;

        public string channelFile;

        public double channelThresholdArea;

        public double channelWidthExponent;

        public double channelWidthMultiplier;

        public string chBasinNoLakeFile;

        public int cropNoData;

        public SpatialReference crsProject;

        public DBUtils db;

        public string dbPath;

        public string dbProjTemplate;

            public string dbRefTemplate;

            public object delineatePos;

        public string delinStreamFile;

        public string demDir;

        public string demFile;

        public string distChFile;

        public double distChNoData;

        public string distStFile;

        public double distStNoData;

        public object elevationBandsPos;

            public double elevationNoData;

            public int elevBandsThreshold;

            public List<string> exemptLanduses;

            public object exemptPos;

            public bool existingWshed;

            public string extraOutletFile;

        public string felFile;

        public string floodDir;

        public string floodFile;

        public int floodNoData;

        public int fromArcChoice;

        public bool forTNC;

            public bool fromGRASS;

            public string fullHRUsFile;

        public string fullLSUsFile;

            public int gridSize;

            public string hd8File;

        public double horizontalFactor;

        public object hrusPos;

            public bool isBatch;

            public bool isHAWQS;

            public bool isHUC;

        public string lakeFile;

        public string landscapeDir;

        public string landscapePos;

        public string landuseDir;

            public string landuseFile;

        public int landuseGroupIndex;

            public string logFile;

        public double mainLengthMultiplier;

        public double meanSlopeMultiplier;

        public string mpiexecPath;

            public int numElevBands;

        public HashSet<Raster> openRasters;

        public string outletFile;

            public object outletsPos;

            public object parametersPos;

            public string pFile;

        public string playaFile;

        public string plotsDir;

            public string plugin_dir;

            public string pngDir;

            public string projDir;

            public string projName;

            public Proj proj;

        //public int QGISSubVersion;

        public string rastersDir;

        public double reachSlopeMultiplier;

        public string resultsDir;

        public int resultsGroupIndex;

        public string scenariosDir;

            public object selectLuPos;

            public object selectOutletFilePos;

            public object selectOutletPos;

            public object selectResPos;

            public object selectSubsPos;

            public string shapesDir;

            public string slopeBandsFile;

            public string slopeFile;

        public int slopeGroupIndex;

        public int slopeNoData;

        public string snapFile;

        public string soilDir;

            public string soilFile;

            public int soilNoData;

            public Dictionary<string, Dictionary<string, int>> splitLanduses;

            public object splitPos;

        public string srcChannelFile;

        public string streamFile;

        public string subbasinsFile;

        public string subsNoLakesFile;

        public string SWATPlusDir;

            public string TauDEMDir;

        public TauDEMUtils taudem;

            public string textDir;

            public Topology topo;

        public double tributaryLengthMultiplier;

        public double tributarySlopeMultiplier;

        public string txtInOutDir;

        public double upslopeHRUDrain;

        public bool useGridModel;

        public bool useLandscapes;

        public bool useLeftRight;

        public string valleyDepthsFile;

        //public object vectorFileWriterOptions;

        public string version;

        public double verticalFactor;

            public string verticalUnits;

            public object visualisePos;

        public int watershedGroupIndex;

        public string wshedFile;

            public int xBlockSize;

            public int yBlockSize;

            public GlobalVars(
                string version,
                bool isBatch,
                bool isHUC = false,
                bool isHAWQS = false,
                string logFile = null) {

                string SWATPlusDir;
            // font size in points
            this.fontSize = Parameters._DEFAULTFONTSIZE;
            // location of addIn code (including here or below python scripts, stylx file)
            this.addinPath = System.IO.Path.GetDirectoryName(new System.Uri(typeof(GlobalVars).Assembly.Location).AbsolutePath);
            this.addinPath = Uri.UnescapeDataString(this.addinPath);
            SWATPlusDir = Parameters._SWATPLUSDEFAULTDIR;
            if (!Directory.Exists(SWATPlusDir)) {
                Utils.error(string.Format(@"Cannot find SWATPlus directory, expected to be {0}.
Please use the Parameters form to set its location.", SWATPlusDir), isBatch);
            } 
            //# SWATPlus directory
            this.SWATPlusDir = SWATPlusDir;
            this.dbPath = Utils.join(SWATPlusDir, Parameters._DBDIR);
            //# Path of template project database
            this.dbProjTemplate = isHAWQS ? Utils.join(this.dbPath, Parameters._DBPROJHAWQS) : Utils.join(this.dbPath, Parameters._DBPROJ);
            //# Path of template reference database
            this.dbRefTemplate = isHAWQS ? Utils.join(this.dbPath, Parameters._DBREFHAWQS) : Utils.join(this.dbPath, Parameters._DBREF);
            //# Directory of TauDEM executables, version 5.3.9
            this.TauDEMDir = TauDEMUtils.findTauDEMDir(SWATPlusDir, isBatch);
            //# Path of mpiexec
            this.mpiexecPath = TauDEMUtils.findMPIExecPath(Utils.join(Parameters._MPIEXECDEFAULTDIR, Parameters._MPIEXEC), isBatch);
            //# QGIS sub version number
            //this.QGISSubVersion = Convert.ToInt32(Qgis.QGIS_VERSION.split(".")[1]);
            //# Flag showing if using existing watershed
            this.existingWshed = false;
            //# Flag showing if using grid model
            this.useGridModel = false;
            //# flag to show if using landscape units
            this.useLandscapes = false;
            //# flag to show if dividing into left/right/headwater landscape units
            this.useLeftRight = false;
            //# grid size (grid models only)
            this.gridSize = 0;
            //# Directory containing QSWAT plugin
            this.plugin_dir = "";
            //# Path of DEM raster
            this.demFile = "";
            //# Path of filled DEM raster
            this.felFile = "";
            //# Path of stream burn-in shapefile
            this.burnFile = "";
            //# Path of DEM after burning-in
            this.burnedDemFile = "";
            //# Path of D8 flow direction raster
            this.pFile = "";
            //# Path of D8 flow accumulation raster
            this.ad8File = "";
            //# Path of basins raster
            this.basinFile = "";
            //# path of channel basins raster
            this.channelBasinFile = "";
            //# path of channel basins file with lakes masked out
            this.chBasinNoLakeFile = "";
            //# Path of channel raster
            this.srcChannelFile = "";
            //# Path of valleyDepthsFile
            // value at each point in this raster is the drop in metres
            // from the point to where its D8 flow path meets a channel
            // Channel elevations are measured at points adjacent to the channel
            // to avoid problems caused by burning-in
            this.valleyDepthsFile = "";
            //# Path of outlets shapefile
            this.outletFile = "";
            //# path of snapped outlets file
            this.snapFile = "";
            //# Path of outlets shapefile for extra reservoirs and point sources
            this.extraOutletFile = "";
            //# Path of stream reaches shapefile
            this.streamFile = "";
            //# Path of stream shapefile calculated by delineation
            // since streamFile is set to streams from grid when using a grid model
            this.delinStreamFile = "";
            //# Path of channel shapefile
            this.channelFile = "";
            //# Path of subbasins shapefile or grid file when using grids
            this.subbasinsFile = "";
            //# Path of subbasins shapefile before lakes removed.  Not used with grid models.
            this.subsNoLakesFile = "";
            //# Path of watershed shapefile
            this.wshedFile = "";
            //# Path of file like D8 contributing area but with heightened values at subbasin outlets
            this.hd8File = "";
            //# Path of distance to stream outlets raster
            this.distStFile = "";
            //# Path of distance to channel outlets raster
            this.distChFile = "";
            //# Path of slope raster
            this.slopeFile = "";
            //# path of lakes shapefile
            this.lakeFile = "";
            //# Path of slope bands raster
            this.slopeBandsFile = "";
            //# Path of landuse raster
            this.landuseFile = "";
            //# Path of soil raster
            this.soilFile = "";
            //# path of floodplain raster
            this.floodFile = "";
            //# path of raster generated from playa lakes
            this.playaFile = "";
            //# Nodata value for DEM
            this.elevationNoData = 0.0;
            //# DEM horizontal block size
            this.xBlockSize = 0;
            //# DEM vertical block size
            this.yBlockSize = 0;
            //# Nodata value for basins raster
            this.basinNoData = 0;
            //# Nodata value for distance to stream outlets raster
            this.distStNoData = 0;
            //# Nodata value for distance to channel outlets raster
            this.distChNoData = 0;
            //# Nodata value for slope raster
            this.slopeNoData = 0;
            //# Nodata value for landuse raster
            this.cropNoData = 0;
            //# Nodata value for soil raster
            this.soilNoData = 0;
            //# Nodata value for floodplain raster
            this.floodNoData = -1;
            //# Area of DEM cell in square metres
            this.cellArea = 0.0;
            //# channel threshold in square metres
            this.channelThresholdArea = 10000000.0;
            //# gridSize as count of DEM cells per side (grid model only)
            this.gridSize = 0;
            //# list of landuses exempt from HRU removal
            this.exemptLanduses = new List<string>();
            //# table of landuses being split
            this.splitLanduses = new Dictionary<string, Dictionary<string, int>>();
            //# Elevation bands threshold in metres
            this.elevBandsThreshold = 0;
            //# Number of elevation bands
            this.numElevBands = 0;
            // TauDEM
            this.taudem = new TauDEMUtils(this);
            //# Topology object
            this.topo = new Topology(isBatch, isHUC, isHAWQS);
            var projFile = Project.Current.Name;
            var projPath = Project.Current.Path;
            //# Project name
            // Project.Current.Name finishes with .aprx, which we need to remove
            int posn = projFile.LastIndexOf(".");
            this.projName = projFile.Substring(0, posn);
            //# Project directory
            this.projDir = Path.GetDirectoryName(projPath);
            // Proj
            this.proj = new Proj(this);
            var title = this.projName;
            var proj = this.proj;
            //# Stream burn-in depth
            this.burninDepth = proj.readDoubleEntry(title, "params/burninDepth", Parameters._BURNINDEPTH).Item1;
            //# Channel width multiplier
            this.channelWidthMultiplier = proj.readDoubleEntry(title, "params/channelWidthMultiplier", Parameters._CHANNELWIDTHMULTIPLIER).Item1;
            //# Channel width exponent
            this.channelWidthExponent = proj.readDoubleEntry(title, "params/channelWidthExponent", Parameters._CHANNELWIDTHEXPONENT).Item1;
            //# Channel depth multiplier
            this.channelDepthMultiplier = proj.readDoubleEntry(title, "params/channelDepthMultiplier", Parameters._CHANNELDEPTHMULTIPLIER).Item1;
            //# Channel depth exponent
            this.channelDepthExponent = proj.readDoubleEntry(title, "params/channelDepthExponent", Parameters._CHANNELDEPTHEXPONENT).Item1;
            //# reach slope multiplier
            this.reachSlopeMultiplier = proj.readDoubleEntry(title, "params/reachSlopeMultiplier", Parameters._MULTIPLIER).Item1;
            //# tributary slope multiplier
            this.tributarySlopeMultiplier = proj.readDoubleEntry(title, "params/tributarySlopeMultiplier", Parameters._MULTIPLIER).Item1;
            //# mean slope multiplier
            this.meanSlopeMultiplier = proj.readDoubleEntry(title, "params/meanSlopeMultiplier", Parameters._MULTIPLIER).Item1;
            //# main length multiplier
            this.mainLengthMultiplier = proj.readDoubleEntry(title, "params/mainLengthMultiplier", Parameters._MULTIPLIER).Item1;
            //# tributary length multiplier
            this.tributaryLengthMultiplier = proj.readDoubleEntry(title, "params/tributaryLengthMultiplier", Parameters._MULTIPLIER).Item1;
            //# upslope HRU drain percent
            this.upslopeHRUDrain = proj.readDoubleEntry(title, "params/upslopeHRUDrain", Parameters._UPSLOPEHRUDRAIN).Item1;
            //# ArcSWAT+ version
            this.version = version;
            //# DEM directory
            this.demDir = "";
            //# Landuse directory
            this.landuseDir = "";
            //# Soil directory
            this.soilDir = "";
            //# Landscape directory
            this.landscapeDir = "";
            //# Floodplain directory
            this.floodDir = "";
            //# text directory
            this.textDir = "";
            //# Shapes directory
            this.shapesDir = "";
            //# rasters directory
            this.rastersDir = "";
            //# Scenarios directory
            this.scenariosDir = "";
            //# TxtInOut directory
            this.txtInOutDir = "";
            //# Results directory
            this.resultsDir = "";
            //# Plots directory
            this.plotsDir = "";
            //# png directory for storing png images used to create animation videos
            this.pngDir = "";
            //# animation directory for storing animation files
            this.animationDir = "";
            this.createSubDirectories();
            //# path of full lsus shapefile
            this.fullLSUsFile = Utils.join(this.shapesDir, Parameters._LSUS1 + ".shp");
            //# path of actual lsus shapefile (after channel mergers
            this.actLSUsFile = Utils.join(this.shapesDir, Parameters._LSUS2 + ".shp");
            //# Path of FullHRUs shapefile
            this.fullHRUsFile = Utils.join(this.shapesDir, Parameters._HRUS1 + ".shp");
            //# Path of ActHRUs shapefile
            this.actHRUsFile = Utils.join(this.shapesDir, Parameters._HRUS2 + ".shp");
            //# Flag to show if running in batch mode
            this.isBatch = isBatch;
            //# flag for HUC projects
            this.isHUC = isHUC;
            //# flag for HAWQS projects
            this.isHAWQS = isHAWQS;
            //# log file for message output for HUC projects
            this.logFile = logFile;
            //# Path of project database
            this.db = new DBUtils(this.projDir, this.projName, this.dbProjTemplate, this.dbRefTemplate, this.isHUC, this.isHAWQS, this.logFile, this.isBatch);
            //# multiplier to turn DEM distances to metres
            this.horizontalFactor = 1.0;
            //# multiplier to turn elevations to metres
            this.verticalFactor = 1.0;
            //# vertical units
            this.verticalUnits = Parameters._METRES;
            // positions of sub windows
            //# Position of delineation form
            this.delineatePos = new Point(0, 100);
            //# Position of HRUs form
            this.hrusPos = new Point(0, 100);
            //# Position of parameters form
            this.parametersPos = new Point(50, 100);
            //# Position of select subbasins form
            this.selectSubsPos = new Point(50, 100);
            //# Position of select reservoirs form
            this.selectResPos = new Point(50, 100);
            //# Position of about form
            this.aboutPos = new Point(50, 100);
            //# Position of elevation bands form
            this.elevationBandsPos = new Point(50, 100);
            //# Position of split landuses form
            this.splitPos = new Point(50, 100);
            //# Position of select landuses form
            this.selectLuPos = new Point(50, 100);
            //# Position of exempt landuses form
            this.exemptPos = new Point(50, 100);
            //# Position of outlets form
            this.outletsPos = new Point(50, 100);
            //# Position of select outlets file form
            this.selectOutletFilePos = new Point(50, 100);
            //# Position of select outlets form
            this.selectOutletPos = new Point(50, 100);
            //# Position of visualise form
            this.visualisePos = new Point(0, 100);
            //# options for creating shapefiles
            //this.vectorFileWriterOptions = QgsVectorFileWriter.SaveVectorOptions();
            //this.vectorFileWriterOptions.ActionOnExistingFile = QgsVectorFileWriter.CreateOrOverwriteFile;
            //this.vectorFileWriterOptions.driverName = "ESRI Shapefile";
            //this.vectorFileWriterOptions.fileEncoding = "UTF-8";
            //# rasters open that need to be closed if memory exception occurs
            // not used. was tried in hrus2.py
            this.openRasters = new HashSet<Raster>();
        }

        // Create subdirectories under project file's directory.
        public void createSubDirectories() {
            Directory.CreateDirectory(this.projDir);
            var watershedDir = Utils.join(this.projDir, "Watershed");
            Directory.CreateDirectory(watershedDir);
            this.rastersDir = Utils.join(watershedDir, "Rasters"); 
            Directory.CreateDirectory(this.rastersDir);
            this.demDir = Utils.join(rastersDir, "DEM");
            Directory.CreateDirectory(this.demDir);
            this.soilDir = Utils.join(rastersDir, "Soil");
            Directory.CreateDirectory(this.soilDir);
            this.landuseDir = Utils.join(rastersDir, "Landuse");
            Directory.CreateDirectory(this.landuseDir);
            this.landscapeDir = Utils.join(rastersDir, "Landscape");
            Directory.CreateDirectory(this.landscapeDir);
            this.floodDir = Utils.join(this.landscapeDir, "Flood");
            Directory.CreateDirectory(this.floodDir);
            this.scenariosDir = Utils.join(this.projDir, "Scenarios");
            Directory.CreateDirectory(this.scenariosDir);
            var defaultDir = Utils.join(this.scenariosDir, "Default");
            Directory.CreateDirectory(defaultDir);
            this.txtInOutDir = Utils.join(defaultDir, "TxtInOut");
            Directory.CreateDirectory(txtInOutDir);
            this.resultsDir = Utils.join(defaultDir, Parameters._RESULTS);
            Directory.CreateDirectory(this.resultsDir);
            this.plotsDir = Utils.join(this.resultsDir, Parameters._PLOTS);
            Directory.CreateDirectory(this.plotsDir);
            this.animationDir = Utils.join(this.resultsDir, Parameters._ANIMATION);
            Directory.CreateDirectory(this.animationDir);
            this.pngDir = Utils.join(this.animationDir, Parameters._PNG);
            Directory.CreateDirectory(this.pngDir);
            this.textDir = Utils.join(watershedDir, "Text");
            Directory.CreateDirectory(this.textDir);
            this.shapesDir = Utils.join(watershedDir, "Shapes");
            Directory.CreateDirectory(this.shapesDir);
        }

        // Set vertical conversion factor according to vertical units.
        public void setVerticalFactor() {
            if (this.verticalUnits == Parameters._METRES) {
                this.verticalFactor = 1.0;
            } else if (this.verticalUnits == Parameters._FEET) {
                this.verticalFactor = Parameters._FEETTOMETRES;
            } else if (this.verticalUnits == Parameters._CM) {
                this.verticalFactor = Parameters._CMTOMETRES;
            } else if (this.verticalUnits == Parameters._MM) {
                this.verticalFactor = Parameters._MMTOMETRES;
            } else if (this.verticalUnits == Parameters._INCHES) {
                this.verticalFactor = Parameters._INCHESTOMETRES;
            } else if (this.verticalUnits == Parameters._YARDS) {
                this.verticalFactor = Parameters._YARDSTOMETRES;
            }
        }

        // Return true if landuse is exempt 
        //         or is part of a split of an exempt landuse.
        //         
        public bool isExempt(int landuseId) {
            var luse = this.db.getLanduseCode(landuseId);
            if (this.exemptLanduses.Contains(luse)) {
                return true;
            }
            foreach (KeyValuePair<string, Dictionary<string, int>> entry in this.splitLanduses) {
                if (this.exemptLanduses.Contains(entry.Key) && entry.Value.Keys.Contains(luse)) {
                    return true;
                }
            }
            return false;
        }

        // Save landuse exempt and split details in project database.
        public bool saveExemptSplit() {
            var exemptTable = "gis_landexempt";
            var splitTable = "gis_splithrus";
            this.db.connectToProjectDatabase();
            if (this.db.conn is null) {
                return false;
            }
            string clearSql = "DROP TABLE IF EXISTS " + exemptTable;
            this.db.execNonQuery(clearSql);
            this.db.execNonQuery(DBUtils._LANDEXEMPTCREATESQL);
            foreach (var luse in this.exemptLanduses)
            {
                this.db.execNonQuery(exemptTable, new string[1] {luse});
            }
            clearSql = "DROP TABLE IF EXISTS " + splitTable;
            this.db.execNonQuery(clearSql);
            this.db.execNonQuery(DBUtils._SPLITHRUSCREATESQL);
            foreach (KeyValuePair<string, Dictionary<string, int>> entry in this.splitLanduses) {
                foreach (KeyValuePair<string, int> entry1 in entry.Value) {
                    this.db.execNonQuery(splitTable, new object[3] { entry.Key, entry1.Key, entry1.Value });
                }
            }
            //conn.commit();
            //if (!(this.isHUC || this.isHAWQS || this.forTNC)) {
            //    this.db.hashDbTable(exemptTable);
            //    this.db.hashDbTable(splitTable);
            //}
            return true;
        }

        // Get landuse exempt and split details from project database.
        public void getExemptSplit() {
            // in case called twice
            this.exemptLanduses = new List<string>();
            this.splitLanduses = new Dictionary<string, Dictionary<string, int>>();
            var exemptTable = "gis_landexempt";
            var splitTable = "gis_splithrus";
            this.db.connectToProjectDatabase();
            if (this.db.conn is null) {
                return;
            }
            string sql = DBUtils.sqlSelect(exemptTable, "LANDUSE", "", "");
            var reader = this.db.getReader(sql);
            while (reader.Read()) {
                this.exemptLanduses.Add(reader.GetString(0));
            }
            sql = DBUtils.sqlSelect(splitTable, "LANDUSE, SUBLU, PERCENT", "", "");
            reader = this.db.getReader(sql);
            while (reader.Read()) {
                var luse = reader.GetString(0);
                if (!this.splitLanduses.ContainsKey(luse)) {
                    this.splitLanduses[luse] = new Dictionary<string, int>();
                }
                this.splitLanduses[luse][reader.GetString(1)] = Convert.ToInt32(reader.GetValue(2));
            }
        }

        // Put currently split landuse codes into combo.
        public void populateSplitLanduses(ComboBox combo) {
            foreach (var luse in this.splitLanduses.Keys) {
                combo.Items.Add(luse);
            }
        }

        // 
        //         Write information to MasterProgress table.
        //         
        //         done parameters may be -1 (leave as is) 0 (not done, default) or 1 (done)
        //         
        public void writeProjectConfig(int doneDelin, int doneHRUs) {
            this.db.connectToProjectDatabase();
            if (this.db.conn is null) {
                return;
            }
            string table = "project_config";
            var proj = this.proj;
            var projectName = this.projName;
            var projectDirectory = this.projDir;
            var projectDb = proj.writePath(this.db.dbFile);
            var referenceDb = proj.writePath(this.db.dbRefFile);
            int doneDelinNum;
            int doneHRUsNum;
            var gisType = "arcgis";
            var gisVersion = this.version;
            // allow table not to exist for HUC
            var reader = this.db.getReader(DBUtils.sqlSelect(table, "delineation_done, hrus_done", "", ""));
            if (reader.HasRows) {
                reader.Read();
                if (doneDelin == -1) {
                    doneDelinNum = Convert.ToInt32(reader.GetValue(0));
                } else {
                    doneDelinNum = doneDelin;
                }
                if (doneHRUs == -1) {
                    doneHRUsNum = Convert.ToInt32(reader.GetValue(1));
                } else {
                    doneHRUsNum = doneHRUs;
                }
                string sql = "UPDATE " + table + String.Format(" SET gis_type='{0}',gis_version='{1}',delineation_done={2},hrus_done={3}", gisType, gisVersion, doneDelinNum, doneHRUsNum);
                this.db.execNonQuery(sql);
            } else {
                reader.Close();
                reader.Dispose();
                if (doneDelin == -1) {
                    doneDelinNum = 0;
                } else {
                    doneDelinNum = doneDelin;
                }
                if (doneHRUs == -1) {
                    doneHRUsNum = 0;
                } else {
                    doneHRUsNum = doneHRUs;
                }
            }
            this.db.execNonQuery(string.Format("DROP TABLE IF EXISTS {0}", table));
            this.db.execNonQuery(DBUtils._CREATEPROJECTCONFIG);
            this.db.execNonQuery(DBUtils._INSERTPROJECTCONFIG, new object[] { 1, projectName, projectDirectory, null, gisType, gisVersion, projectDb, referenceDb, null, null, null, null, null, null, null, doneDelinNum, doneHRUsNum, DBUtils._SOILS_SOL_NAME, DBUtils._SOILS_SOL_LAYER_NAME, null, 0, 0 });
        }

    // Return true if delineation done according to project config table.
        public bool isDelinDone() {
            this.db.connectToProjectDatabase();
            if (this.db.conn is null) {
                return false;
            }
            string table = "project_config";
            var reader = this.db.getReader(DBUtils.sqlSelect(table, "delineation_done", "", ""));
            if (!reader.HasRows) return false;
            reader.Read();
            return Convert.ToInt32(reader.GetValue(0)) == 1;
    }

    // Return true if HRU creation is done according to MasterProgress table.
        public bool isHRUsDone() {
            this.db.connectToProjectDatabase();
            if (this.db.conn is null) {
                    return false;
            }
            string table = "project_config";
            var reader = this.db.getReader(DBUtils.sqlSelect(table, "hrus_done", "", ""));
            if (!reader.HasRows) return false;
            reader.Read();
            return Convert.ToInt32(reader.GetValue(0)) == 1;
        }

        // Return path to SWAT+ editor..
        public virtual string findSWATPlusEditor() {
            var editorDir = Utils.join(this.SWATPlusDir, Parameters._SWATEDITORDIR);
            var editor = Utils.join(editorDir, Parameters._SWATEDITOR);
            if (File.Exists(editor)) {
                return editor;
            }
            Utils.information(string.Format(@"Cannot find {0} in {1}.  
Have you installed SWAT+ as a different directory from C:\SWAT\SWATPlus?
If so use the ArcSWAT+ Parameters form to set the correct location.", Parameters._SWATEDITOR, editorDir), this.isBatch);
            return null;
        }

        public void SetFontSize(Form f) {
            if (f.Font.SizeInPoints != this.fontSize) {
                Font newFont = new Font(f.Font.Name, this.fontSize, f.Font.Style);
                foreach (Control ctr in f.Controls) {
                    ctr.Font = newFont;
                    // controls in groupboxes are not child of main form
                    if (ctr.HasChildren) {
                        foreach (Control childControl in ctr.Controls) {
                            childControl.Font = newFont;
                        }
                    }
                }
            }
        }

        // Close open rasters (to enable them to be reopened with new chunk size).
        public virtual void closeOpenRasters() {
            foreach (var raster in this.openRasters) {
                try {
                    raster.close();
                    this.openRasters.Remove(raster);
                }
                catch (Exception) {
                }
            }
        }

        // Clear list of open rasters.
        public virtual void clearOpenRasters() {
            this.openRasters.Clear();
        }
    }
}

