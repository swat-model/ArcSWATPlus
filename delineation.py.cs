

using ArcGIS.Core.Data;
using ArcGIS.Core.Data.Analyst3D;
using ArcGIS.Core.Data.Raster;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Internal.CIM;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.Catalog;
using ArcGIS.Desktop.Internal.Core.Assistant.EnterpriseClient.ChatRequest.GraphQueryGeneration;
using ArcGIS.Desktop.Internal.Editing;
using ArcGIS.Desktop.Mapping;
using Microsoft.Extensions.FileSystemGlobbing;
using OSGeo.GDAL;
using OSGeo.OGR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static ArcGIS.Desktop.Editing.Templates.EditingGroupTemplate;
using static System.Net.WebRequestMethods;

namespace ArcSWATPlus {

        // Data about grid cell.
        public class GridData {

            public double area;

            public int downCol;

            public int downNum;

            public int downRow;

            public double drainArea;

            public int dsNodeId;

            public double maxAcc;

            public int maxCol;

            public int maxRow;

            public int num;

            public GridData(
                int num,
                int area,
                double drainArea,
                double maxAcc,
                int maxRow,
                int maxCol) {
                //# PolygonId of this grid cell
                this.num = num;
                //# PolygonId of downstream grid cell
                this.downNum = -1;
                //# Row in storeGrid of downstream grid cell
                this.downRow = -1;
                //# Column in storeGrid of downstream grid cell
                this.downCol = -1;
                //# area of this grid cell in number cells in accumulation grid
                this.area = area;
                //# area being drained in sq km to start of stream in this grid cell
                this.drainArea = drainArea;
                //# accumulation at maximum accumulation point
                this.maxAcc = maxAcc;
                //# Row in accumulation grid of maximum accumulation point
                this.maxRow = maxRow;
                //# Column in accumulation grid of maximum accumulation point
                this.maxCol = maxCol;
                //# dsnodeid, or -1 if none (and initially)
                this.dsNodeId = -1;
            }
        }

        // Enumeration of inlet/outlets points.
        public class Points {

            public static int _OUTLET = 0;

            public static int _INLET = 1;

            public static int _RESERVOIR = 2;

            public static int _POND = 3;

            public static int _POINTSOURCE = 4;

            // String for each point.
            public static string toString(int point) {
                if (point == Points._OUTLET) {
                    return "outlet";
                } else if (point == Points._INLET) {
                    return "inlet";
                } else if (point == Points._RESERVOIR) {
                    return "reservoir";
                } else if (point == Points._POND) {
                    return "pond";
                } else {
                    return "point source";
                }
            }
        }

    // Do watershed delineation.
    public class Delineation {

        public DelineationForm _dlg;

        public GlobalVars _gv;

        public ArcSWATPlus _parent;

        public string clipperFile;

        public bool delineationFinishedOK;

        public int demHeight;

        public int demWidth;

        public string drainageTable;

        public FeatureLayer drawOutletLayer;

        public string drawOutletFile;

        public bool drawCurrent;

        public bool finishHasRun;

        public bool gridDrainage;

        public bool gridLakesAdded;

        public FeatureLayer gridLayer;

        public Dictionary<int, (int, double[])> inletOutletPoints;

        public bool isDelineated;

        public Landscape L;

        public bool lakePointsAdded;

        public bool lakesDone;

        public FeatureLayer selFromLayer;

        public bool snapErrors;

        public string snapFile;

        public bool streamDrainage;

        public bool thresholdChanged;

        public Delineation(GlobalVars gv, bool isDelineated, ArcSWATPlus parent) {
            this._gv = gv;
            this._parent = parent;
            this._dlg = new DelineationForm(gv, this);
            //this._dlg.move(this._gv.delineatePos);
            //# when a snap file is created this is set to the file path
            this.snapFile = "";
            //# when not all points are snapped this is set True so snapping can be rerun
            this.snapErrors = false;
            //# vector layer for drawing inlet/outlet points
            this.drawOutletLayer = null;
            //# Width of DEM as number of cells
            this.demWidth = 0;
            //# Height of DEM cell as number of cells
            this.demHeight = 0;
            //# inlet/outlet points, stored as map from inlet/outlet file id to QgsPoint: 
            //# used in grid model only
            this.inletOutletPoints = new Dictionary<int, (int, double[])>();
            //# flag to show basic delineation is done, so removing subbasins, 
            //# adding reservoirs and point sources may be done
            this.isDelineated = isDelineated;
            //# flag to show delineation completed successfully or not
            this.delineationFinishedOK = true;
            //# flag to show if threshold or outlet file changed since loading form; 
            //# if not can assume any existing watershed is OK
            this.thresholdChanged = false;
            //# flag to show finishDelineation has been run
            this.finishHasRun = false;
            //# flag to show if drainage for existing grid defined by attribute in grid (when true), else by streams shapefile or drainage table
            this.gridDrainage = false;
            //# flag to show if drainage for existing grid defined by streams shapefile (when true) or by drainage table
            this.streamDrainage = true;
            //# drainage table csv file
            this.drainageTable = "";
            //# landscape object
            this.L = null;
            //# copy of subbasins file, set when making grids before grid file overwrites it
            // while it is the empty string the subbasins file may be used
            this.clipperFile = "";
            //# flag to show if lakes file yet to be dealt with
            this.lakesDone = true;
            //# flag to show if lake inlet/outlet points added and channels split (for non-grid models)
            this.lakePointsAdded = false;
            //# flag to show if grid lakes have been created from lake shapefile when using grid model
            this.gridLakesAdded = false;
        }

        // Set connections to controls; read project delineation data.
        public virtual void init() {
            //this._dlg.numProcesses.Value = 8);
            //this._dlg.selectDemButton.clicked.connect(this.btnSetDEM);
            //this._dlg.checkBurn.stateChanged.connect(this.changeBurn);
            //this._dlg.useGrid.clicked.connect(this.changeUseGrid);
            //this._dlg.gridBox.clicked.connect(this.changeUseGrid);
            //this._dlg.burnButton.clicked.connect(this.btnSetBurn);
            //this._dlg.selectOutletsButton.clicked.connect(this.btnSetOutlets);
            //this._dlg.selectSubbasinsButton.clicked.connect(this.btnSetSubbasins);
            //this._dlg.selectWshedButton.clicked.connect(this.btnSetWshed);
            //this._dlg.selectStreamsButton.clicked.connect(this.btnSetStreams);
            //this._dlg.selectExistOutletsButton.clicked.connect(this.btnSetOutlets);
            //this._dlg.delinRunButton1.clicked.connect(this.runTauDEM1);
            //this._dlg.delinRunButton2.clicked.connect(this.runTauDEM2);
            //this._dlg.delinTabWidget.currentChanged.connect(this.changeExisting);
            //this._dlg.drainGridButton.clicked.connect(this.setDrainage);
            //this._dlg.drainStreamsButton.clicked.connect(this.setDrainage);
            //this._dlg.drainTableButton.clicked.connect(this.setDrainage);
            //this._dlg.existRunButton.clicked.connect(this.runExisting);
            //this._dlg.useOutlets.stateChanged.connect(this.changeUseOutlets);
            //this._dlg.drawOutletsButton.clicked.connect(this.drawOutlets);
            //this._dlg.selectOutletsInteractiveButton.clicked.connect(this.selectOutlets);
            //this._dlg.snapReviewButton.clicked.connect(this.snapReview);
            //this._dlg.landscapeButton.clicked.connect(this.runLandscape);
            //this._dlg.selectSubButton.clicked.connect(this.selectMergeSubbasins);
            //this._dlg.mergeButton.clicked.connect(this.mergeSubbasins);
            //this._dlg.selectLakesButton.clicked.connect(this.addLakesMap);
            //this._dlg.removeLakeCells.clicked.connect(this.removeLakeCells);
            //this._dlg.addLakeCells.clicked.connect(this.addLakeCells);
            //this._dlg.taudemHelpButton.clicked.connect(this._gv.taudem.taudemHelp);
            //this._dlg.OKButton.clicked.connect(this.finishDelineation);
            //this._dlg.cancelButton.clicked.connect(this.doClose);
            ////this._dlg.numCellsCh.setValidator(QIntValidator());
            ////this._dlg.numCellsSt.setValidator(QIntValidator());
            //this._dlg.numCellsCh.textChanged.connect(this.setAreaCh);
            //this._dlg.areaCh.textChanged.connect(this.setNumCellsCh);
            //this._dlg.numCellsSt.textChanged.connect(this.setAreaSt);
            //this._dlg.areaSt.textChanged.connect(this.setNumCellsSt);
            ////this._dlg.areaCh.setValidator(QDoubleValidator());
            ////this._dlg.areaSt.setValidator(QDoubleValidator());
            //this._dlg.areaUnitsBox.activated.connect(this.changeAreaOfCell);
            //this._dlg.verticalCombo.activated.connect(this.setVerticalUnits);
            ////this._dlg.snapThreshold.setValidator(QIntValidator());
            //this._odlg.resumeButton.clicked.connect(this.resumeDrawing);
            //this._dlg.readProj();
            this._dlg.changeUseGrid();
            this.thresholdChanged = false;
            this.checkMPI();
            // allow for cancellation without being considered an error
            this.delineationFinishedOK = true;
            // Prevent annoying "error 4 .shp not recognised" messages.
            // These should become exceptions but instead just disappear.
            // Safer in any case to raise exceptions if something goes wrong.
            OSGeo.GDAL.Gdal.UseExceptions();
            OSGeo.OGR.Ogr.UseExceptions();
        }

        // Do delineation; check done and save topology data.  Return 1 if delineation done and no errors, 2 if not delineated and nothing done, else 0.
        public virtual void run() {
            this.init();
            this._dlg.Show();
            //this._gv.delineatePos = this._dlg.pos();
        }

        // 
        //         In Windows, try to make sure there is just one msmpi.dll, either on the path or in the TauDEM directory.
        //         
        //         TauDEM executables are built on the assumption that MPI is available.
        //         But they can run without MPI if msmpi.dll is placed in their directory.
        //         MPI will fail if there is an msmpi.dll on the path and one in the TauDEM directory 
        //         (unless they happen to be the same version).
        //         QSWAT supplies msmpi_dll in the TauDEM directory that can be renamed to provide msmpi.dll 
        //         if necessary.
        //         This function is called every time delineation is started so that if the user installs MPI
        //         or uninstalls it the appropriate steps are taken.
        //         
        public virtual void checkMPI() {
            var dll = "msmpi.dll";
            var dummy = "msmpi_dll";
            var dllPath = Utils.join(this._gv.TauDEMDir, dll);
            var dummyPath = Utils.join(this._gv.TauDEMDir, dummy);
            // tried various methods here.  
            //'where msmpi.dll' succeeds if it was there and is moved or renamed - cached perhaps?
            // isfile fails similarly
            //'where mpiexec' always fails because when run interactively the path does not include the MPI directory
            // so just check for existence of mpiexec.exe and assume user will not leave msmpi.dll around
            // if MPI is installed and then uninstalled
            if (System.IO.File.Exists(this._gv.mpiexecPath)) {
                Utils.loginfo("mpiexec found");
                // MPI is on the path; rename the local dll if necessary
                if (System.IO.File.Exists(dllPath)) {
                    if (System.IO.File.Exists(dummyPath)) {
                        System.IO.File.Delete(dllPath);
                        Utils.loginfo("dll removed");
                    } else {
                        System.IO.File.Move(dllPath, dummyPath);
                        Utils.loginfo("dll renamed");
                    }
                }
            } else {
                Utils.loginfo("mpiexec not found");
                // we don't have MPI on the path; rename the local dummy if necessary
                if (System.IO.File.Exists(dllPath)) {
                    return;
                } else if (System.IO.File.Exists(dummyPath)) {
                    System.IO.File.Move(dummyPath, dllPath);
                    Utils.loginfo("dummy renamed");
                } else {
                    Utils.error(string.Format("Cannot find executable mpiexec in the system or {0} in {1}: TauDEM functions will not run.  Install MPI or reinstall ArcSWAT+.", dll, this._gv.TauDEMDir), this._gv.isBatch);
                }
            }
        }

        // noinspection PyArgumentList
        // 
        //         Finish delineation.
        //         
        //         Adds lakes if necessary, checks stream reaches and watersheds defined, sets DEM attributes, 
        //         checks delineation is complete, calculates flow distances,
        //         runs topology setup.  Writes reaches and points tables.  
        //         Sets delineationFinishedOK to true if all completed successfully.
        //         
        public async Task finishDelineation() {
            if (this._gv.lakeFile != "" && !this.lakesDone) {
                await this.addLakes();
            }
            this.delineationFinishedOK = false;
            this.finishHasRun = true;
            var chanFile = this._gv.useGridModel ? this._gv.streamFile : this._gv.channelFile;
            if (!System.IO.File.Exists(chanFile)) {
                if (this._gv.useGridModel) {
                    Utils.error("Grid streams file not found.", this._gv.isBatch);
                } else if (this._gv.existingWshed) {
                    Utils.error("Channels File not found.", this._gv.isBatch);
                } else {
                    Utils.error(string.Format("Channels file {0} not found: have you run TauDEM?", chanFile), this._gv.isBatch);
                }
                return;
            }
            string subbasinsFile;
            if (!this._gv.existingWshed && this._gv.useGridModel) {
                subbasinsFile = Utils.join(this._gv.shapesDir, "grid.shp");
                if (!System.IO.File.Exists(subbasinsFile)) {
                    Utils.error("Grid layer not found.", this._gv.isBatch);
                    return;
                }
            } else {
                subbasinsFile = this._gv.subbasinsFile;
                if (!System.IO.File.Exists(subbasinsFile)) {
                    if (this._gv.existingWshed) {
                        Utils.error("Subbasins layer not found.", this._gv.isBatch);
                    } else {
                        Utils.error("Subbasins layer not found: have you run TauDEM?", this._gv.isBatch);
                    }
                    return;
                }
            }
            var demLayer = (RasterLayer)(await Utils.getLayerByFilename(this._gv.demFile, FileTypes._DEM, null, null, null)).Item1;
            if (demLayer is null) {
                Utils.error("DEM layer not found: have you removed it?", this._gv.isBatch);
                return;
            }
            if (!await this.setDimensions(demLayer)) {
                return;
            }
            if (!this._gv.useGridModel && this._gv.basinFile == "") {
                // must have merged some subbasins: recreate the subbasins raster
                this._gv.basinFile = await this.createBasinFile(this._gv.subbasinsFile, demLayer, "wStream");
                // Utils.loginfo('Recreated watershed grid as {0}', self._gv.basinFile))
            }
            this._dlg.saveProj();
            string outletFile;
            if (this.checkDEMProcessed()) {
                var recalculate = this._gv.existingWshed && this._dlg.delinRecalcButton.Checked;
                this.progress("Constructing topology ...");
                if (this._gv.outletFile == "") {
                    outletFile = "";
                } else if (!this._gv.existingWshed && System.IO.File.Exists(this._gv.snapFile)) {
                    outletFile = this._gv.snapFile;
                } else {
                    outletFile = this._gv.outletFile;
                }
                string lakeFile;
                if (System.IO.File.Exists(this._gv.lakeFile)) {
                    lakeFile = this._gv.lakeFile;
                } else {
                    lakeFile = "";
                }
                if (await this._gv.topo.setUp(demLayer, chanFile, this._gv.subbasinsFile, outletFile, lakeFile, this._gv, this._gv.existingWshed, recalculate, this._gv.useGridModel, this.streamDrainage, true)) {
                    if (this._gv.topo.inlets.Count == 0) {
                        // no inlets, so no need to expand subbasins layer legend
                        var basinsLayer = Utils.getLayerByLegend(Utils._SUBBASINSLEGEND);
                        if (basinsLayer is not null) {
                            await QueuedTask.Run(() => basinsLayer.SetExpanded(false));
                        }
                    }
                    await this.makeDistancesToOutlets();
                    this.delineationFinishedOK = true;
                    this.progress("");
                    this.doClose();
                    return;
                } else {
                    this.progress("");
                    return;
                }
            }
            return;
        }

        // Add lakes from lakes shapefile. 
        public async Task addLakes() {
            DataSource lakesDs = null;
            DataSource channelsDs = null;
            DataSource streamsDs = null;
            DataSource subbasinsDs = null;
            OSGeo.OGR.Layer lakesLayer = null;
            OSGeo.OGR.Layer channelsLayer = null;
            OSGeo.OGR.Layer streamsLayer = null;
            OSGeo.OGR.Layer subbasinsLayer = null;
            var subbasinsFile = "";
            var chBasinsFile = "";
            bool reportErrors = true;
            Utils.loginfo(string.Format("{0} outlets before adding lakes", this._gv.topo.outlets.Count));
            if (this.lakesDone || this._gv.lakeFile == "") {
                return;
            }
            if (!System.IO.File.Exists(this._gv.lakeFile)) {
                Utils.error(string.Format("Cannot find lakes file {0}", this._gv.lakeFile), this._gv.isBatch);
                return;
            }
            try {
                string channelsFile;
                if (this._gv.useGridModel) {
                    channelsFile = "";
                } else {
                    channelsFile = this._gv.channelFile;
                    if (string.IsNullOrEmpty(channelsFile)) {
                        Utils.error("Channels file not found.", this._gv.isBatch);
                        return;
                    }
                }
                var demLayer = (RasterLayer)(await Utils.getLayerByFilename(this._gv.demFile, FileTypes._DEM, null, null, null)).Item1;
                if (demLayer is null) {
                    Utils.error("DEM layer not found.", this._gv.isBatch);
                    return;
                }
                // add spatial index to lake (advised for using erase later)
                var parms = Geoprocessing.MakeValueArray(this._gv.lakeFile);
                var environment = Geoprocessing.MakeEnvironmentArray(overwriteoutput: true);
                await Geoprocessing.ExecuteToolAsync("AddSpatialIndex_management", parms, environment, null, null, GPExecuteToolFlags.None);
                lakesDs = Ogr.Open(this._gv.lakeFile, 1);
                lakesLayer = lakesDs.GetLayerByIndex(0);
                //if (this._gv.isHUC || this._gv.isHAWQS) {
                //    subbasinsDs = Ogr.Open(this._gv.subbasinsFile, 0);
                //    subbasinsLayer = subbasinsDs.GetLayerByIndex(0);
                //    if (!await this.addHUCLakes(lakesLayer, channelsLayer, subbasinsLayer, demLayer)) {
                //        Utils.error("Failed to Add lakes", this._gv.isBatch);
                //        return;
                //    }
                //} else {
                reportErrors = !this._dlg.lakeMessages;
                if (!this._gv.useGridModel && !this.lakePointsAdded && !this._gv.existingWshed) {
                    this.progress("Splitting lake channels ...");
                    if (!await this.splitChannelsByLakes(lakesLayer, demLayer, reportErrors)) {
                        Utils.error("Failed to split lake channels", this._gv.isBatch);
                        return;
                    }
                    // and channels layer needs refreshing as has been rewritten
                    var channelsArcLayer = (FeatureLayer)(await Utils.getLayerByFilename(this._gv.channelFile, FileTypes._CHANNELS, null, null, null)).Item1;
                    if (channelsArcLayer is null) {
                        Utils.error("Channels layer not found.", this._gv.isBatch);
                        return;
                    }
                }
                if (this._gv.useGridModel || !this._gv.existingWshed) {
                    subbasinsFile = this._gv.subbasinsFile;
                    subbasinsDs = Ogr.Open(subbasinsFile, 1);
                    subbasinsLayer = subbasinsDs.GetLayerByIndex(0);
                    streamsDs = Ogr.Open(this._gv.streamFile, 0);
                    streamsLayer = streamsDs.GetLayerByIndex(0);
                    if (!this._gv.useGridModel && !this._gv.existingWshed) {
                        chBasinsFile = this._gv.wshedFile;
                    }
                }
                if (this._gv.useGridModel && !this.gridLakesAdded) {
                    this.progress("Making grid lakes ...");
                    //Debug.Assert(subbasinsLayer is not null);
                    await this.makeGridLakes(lakesLayer, subbasinsLayer);
                    this.progress("");
                    if (lakesLayer is null) {
                        Utils.error("Failed to make grid lakes", this._gv.isBatch);
                        return;
                    }
                }
                // avoid running again
                this.lakesDone = true;
                this._dlg.delinLakesTab.Enabled = false;
                this._dlg.delinMergeTab.Enabled = false;
                this.progress("Adding lakes ...");
                using (new CursorWait()) {
                    this._gv.topo.lakesData.Clear();
                    channelsDs = this._gv.useGridModel ? null : Ogr.Open(channelsFile, 1);
                    channelsLayer = this._gv.useGridModel ? null : channelsDs.GetLayerByIndex(0);
                    if (this._gv.useGridModel) {
                        await this._gv.topo.addGridReservoirsPondsAndWetlands(subbasinsLayer, streamsLayer, demLayer, this._gv);
                        this._gv.topo.addGridPlayas(lakesLayer, demLayer, this._gv);
                    } else if (this._gv.existingWshed) {
                        await this._gv.topo.addExistingLakes(lakesLayer, channelsLayer, demLayer, this._gv);
                    } else {
                        // make copy of subbasins shapefile before removing lakes
                        Utils.copyShapefile(this._gv.subbasinsFile, "subsNoLakes", this._gv.shapesDir);
                        this._gv.subsNoLakesFile = Utils.join(this._gv.shapesDir, "subsNoLakes.shp");
                        var snapThreshold = Convert.ToInt32(this._dlg.delinSnapThreshold.Text);
                        await this._gv.topo.addLakes(lakesLayer, subbasinsFile, chBasinsFile, streamsLayer, channelsLayer, demLayer, snapThreshold, this._gv, reportErrors: reportErrors);
                        this._gv.chBasinNoLakeFile = await this.createBasinFile(this._gv.projGdb + @"\chBasins", demLayer, "wChannelNoLake");
                    }
                    this._gv.topo.saveLakesData(this._gv.db);
                    this.progress("");
                }
                Utils.loginfo(string.Format("{0} outlets after adding lakes", this._gv.topo.outlets.Count));
            }
            finally {
                // clean up
                lakesDs.Dispose();
                if (channelsDs is not null) {
                    channelsDs.Dispose();
                }
                if (subbasinsDs is not null) {
                    subbasinsDs.Dispose();
                }
                if (streamsDs is not null) {
                    streamsDs.Dispose();
                }
            }
        }

        // Split channels crossing reservoir or pond lake boundaries into two, entering/leaving and within.  
        //         Then rerun channel basins calculation to split into areas draining to channels and areas draining to lakes.
        //         
        //         Not used with grid models or existing watersheds.
        public async Task<bool> splitChannelsByLakes(OSGeo.OGR.Layer lakesLayer, RasterLayer demLayer, bool reportErrors) {
            double[] source;
            double[] outlet;
            ReachData reachData;
            int chLink;
            int res;
            OSGeo.OGR.Geometry geom;
            int lakeId;
            int ptIdIndex;
            int resIndex;
            int srcIndex;
            int addIndex;
            int inletIndex;
            int idIndex;
            int dsLinkIndex = 0;
            int dsNodeIndex = 0;
            FeatureDefn defn;

            // intersect is a line of points inside the lake
            // a multi line if there are several crossing points.
            // We want to find the end point, which will be on the lake boundary.
            // Which end depends on the orientation of the line
            // and whether we are looking for in inflowing point or outflowing
            double[] getCrossingPoint(OSGeo.OGR.Geometry intersect, bool isIn) {

                bool isFirst = this._gv.topo.outletAtStart ? !isIn : isIn;
                double[] pt = new double[3];
                if (intersect.GetGeometryType() == wkbGeometryType.wkbLineString) {
                    //for (int i  = 0; i < intersect.GetPointCount(); i++) {
                    //    intersect.GetPoint(i, pt);
                    //    Utils.loginfo(string.Format("Point {0}: ({1}, {2})", i, Math.Round(pt[0]), Math.Round(pt[1])));
                    //}
                    if (isFirst) {
                        intersect.GetPoint(0, pt);
                    } else {
                        var numPoints = intersect.GetPointCount();
                        intersect.GetPoint(numPoints - 1, pt);
                    }
                } else if (intersect.GetGeometryType() == wkbGeometryType.wkbMultiLineString) {
                    OSGeo.OGR.Geometry part;
                    //for (int j = 0; j < intersect.GetGeometryCount(); j++) {
                    //    part = intersect.GetGeometryRef(j);
                    //    Utils.loginfo(string.Format("Part {0}", j));
                    //    for (int i = 0; i < part.GetPointCount(); i++) {
                    //        part.GetPoint(i, pt);
                    //        Utils.loginfo(string.Format("Point {0}: ({1}, {2})", i, Math.Round(pt[0]), Math.Round(pt[1])));
                    //    }
                    //}
                    if (isFirst) {
                        part = intersect.GetGeometryRef(0);
                    } else {
                        var numParts = intersect.GetGeometryCount();
                        part = intersect.GetGeometryRef(numParts - 1);
                    }
                    if (isFirst) {
                        part.GetPoint(0, pt);
                    } else {
                        var numPoints = part.GetPointCount();
                        part.GetPoint(numPoints - 1, pt);
                    }
                }
                //Utils.loginfo(string.Format("Chose ({0}, {1})", Math.Round(pt[0]), Math.Round(pt[1])));
                return pt;
            }


            int makeNewPoint(
                OSGeo.OGR.Geometry intersect,
                double[] source,
                double[] outlet,
                List<OSGeo.OGR.Feature> newPoints,
                int currentPointId,
                bool isInlet,
                int res,
                int chLink,
                int lakeId,
                FeatureDefn defn,
                double[] transform) {
                //Utils.loginfo(string.Format("Intersects for {0} channel {1}", isInlet ? "inlet" : "outlet", chLink));
                double[] crossingPoint = getCrossingPoint(intersect, isInlet);
                OSGeo.OGR.Feature point = new OSGeo.OGR.Feature(defn);
                currentPointId += 1;
                point.SetField(idIndex, currentPointId);
                point.SetField(resIndex, isInlet ? 0 : res);
                point.SetField(inletIndex, 0);
                point.SetField(srcIndex, 0);
                point.SetField(ptIdIndex, currentPointId);
                point.SetField(addIndex, 1);
                // check point not too close to (in same dem pixel as) source or outlet
                // else this point will be ignored by TauDEM
                double[] crossingPointMoved1 = Topology.separatePoints(source, crossingPoint, transform);
                Utils.loginfo(string.Format("Crossing point moved away from ({0}, {1}) to ({2}, {3}", 
                    Math.Round(source[0]), Math.Round(source[1]), Math.Round(crossingPointMoved1[0]), Math.Round(crossingPointMoved1[1])));
                double[] crossingPointMoved2 = Topology.separatePoints(outlet, crossingPointMoved1, transform);
                Utils.loginfo(string.Format("Crossing point moved away from ({0}, {1}) to ({2}, {3}",
                    Math.Round(outlet[0]), Math.Round(outlet[1]), Math.Round(crossingPointMoved2[0]), Math.Round(crossingPointMoved2[1])));
                point.SetGeometry(Topology.geometryFromPoint(crossingPointMoved2));
                newPoints.Add(point);
                if (isInlet) {
                    this._gv.topo.lakeInlets[lakeId].Add(currentPointId);
                } else {
                    this._gv.topo.lakeOutlets[lakeId].Add(currentPointId);
                    Utils.loginfo(string.Format("Outlet to lake {0} on link {1} at ({2},{3})", lakeId, chLink, Convert.ToInt32(crossingPointMoved2[0]), Convert.ToInt32(crossingPointMoved2[1])));
                }
                return currentPointId;
            }

            var (lakeIdIndex, hasResOrPondOrWetland, hasPlaya) = this.identifyLakes(lakesLayer);
            if (hasPlaya) {
                this._gv.playaFile = await this.createPlayaFile(this._gv.lakeFile, demLayer);
            } else {
                this._gv.playaFile = "";
            }
            if (!hasResOrPondOrWetland) {
                // nothing to do
                return true;
            }
            string RES = (this._gv.isHUC || this._gv.isHAWQS) ? Topology._HUCRES : Topology._RES;
            var lakeResIndex = this._gv.topo.getIndex(lakesLayer, RES, ignoreMissing: true);
            if (lakeIdIndex < 0) {
                return false;
            }
            var channelThreshold = this._dlg.numCellsChText;
            using (var channelsDs = Ogr.Open(this._gv.channelFile, 1)) {
                OSGeo.OGR.Layer channelsLayer = channelsDs.GetLayerByIndex(0);
                if (!this._dlg.useOutletsChecked) {
                    // create an inlets/outlets file
                    var outletsFile = Utils.join(this._gv.shapesDir, "madeoutlets.shp");
                    if (!await Delineation.createOutletFile(outletsFile, this._gv.demFile, false, this._gv)) {
                        return false;
                    }
                    using (var outletsDs = Ogr.Open(outletsFile, 1)) {
                        var outletsLayer = outletsDs.GetLayerByIndex(0);
                        defn = outletsLayer.GetLayerDefn();
                        idIndex = defn.GetFieldIndex(Topology._ID);
                        inletIndex = defn.GetFieldIndex(Topology._INLET);
                        resIndex = defn.GetFieldIndex(Topology._RES);
                        var ptsourceIndex = defn.GetFieldIndex(Topology._PTSOURCE);
                        ptIdIndex = defn.GetFieldIndex(Topology._POINTID);
                        dsLinkIndex = this._gv.topo.getIndex(channelsLayer, Topology._DSLINKNO, ignoreMissing: false);
                        // basin numbers should have been added to channels file by now
                        var basinIndex = this._gv.topo.getIndex(channelsLayer, Topology._BASINNO, ignoreMissing: false);
                        var dsNodeIdAdded = false;
                        dsNodeIndex = this._gv.topo.getIndex(channelsLayer, Topology._DSNODEID, ignoreMissing: true);
                        if (dsNodeIndex < 0) {
                            channelsLayer.CreateField(new FieldDefn(Topology._DSNODEID, OSGeo.OGR.FieldType.OFTInteger), 1);
                            dsNodeIndex = this._gv.topo.getIndex(channelsLayer, Topology._DSNODEID);
                            dsNodeIdAdded = true;
                        }
                        channelsLayer.ResetReading();
                        OSGeo.OGR.Feature channel;
                        do {
                            channel = channelsLayer.GetNextFeature();
                            if (channel is null) { break; }
                            var subbasin = channel.GetFieldAsInteger(basinIndex);
                            if (subbasin < 0 || channel.GetFieldAsInteger(dsLinkIndex) >= 0) {
                                // outside watershed or zero length channel or not an outlet
                                if (dsNodeIdAdded) {
                                    // need to set new field
                                    channel.SetField(dsNodeIndex, -1);
                                    channelsLayer.SetFeature(channel);
                                }
                                continue;
                            }
                            // self._gv.topo.outlets should already be populated
                            var (outletId, outletPt, _) = this._gv.topo.outlets[subbasin];
                            channel.SetField(dsNodeIndex, outletId);
                            var outletF = new OSGeo.OGR.Feature(defn);
                            outletF.SetField(idIndex, outletId);
                            outletF.SetField(inletIndex, 0);
                            outletF.SetField(resIndex, 0);
                            outletF.SetField(ptsourceIndex, 0);
                            outletF.SetField(ptIdIndex, outletId);
                            outletF.SetGeometry(Topology.geometryFromPoint(outletPt));
                            outletsLayer.SetFeature(outletF);
                        } while (channel is not null);
                        var snapFile = Utils.join(this._gv.shapesDir, "madeoutlets_snap.shp");
                        // since we used channel end points for outlets snap file will be identical to outlets file
                        await Utils.removeLayer(snapFile);
                        Utils.copyShapefile(outletsFile, "madeoutlets_snap", this._gv.shapesDir);
                        this._gv.outletFile = outletsFile;
                        this._gv.snapFile = snapFile;
                    }
                }
                if (this._gv.snapFile == "" || !System.IO.File.Exists(this._gv.snapFile)) {
                    Utils.error(string.Format("Cannot find snapped inlets/outlets file {0}", this._gv.snapFile), this._gv.isBatch);
                    return false;
                }
                var lakesToRemove = new List<long>();
                using (var snapDs = Ogr.Open(this._gv.snapFile, 1)) {
                    var snapLayer = snapDs.GetLayerByIndex(0);
                    defn = snapLayer.GetLayerDefn();
                    idIndex = defn.GetFieldIndex(Topology._ID);
                    resIndex = defn.GetFieldIndex(Topology._RES);
                    inletIndex = defn.GetFieldIndex(Topology._INLET);
                    srcIndex = defn.GetFieldIndex(Topology._PTSOURCE);
                    ptIdIndex = defn.GetFieldIndex(Topology._POINTID);
                    // Add ADDED field to define additional points
                    addIndex = defn.GetFieldIndex(Topology._ADDED);
                    if (addIndex < 0) {
                        snapLayer.CreateField(new FieldDefn(Topology._ADDED, OSGeo.OGR.FieldType.OFTInteger), 1);
                        addIndex = this._gv.topo.getIndex(snapLayer, Topology._ADDED);
                    }
                    var ds = OSGeo.GDAL.Gdal.Open(this._gv.demFile, OSGeo.GDAL.Access.GA_ReadOnly);
                    double[] transform = new double[6];
                    ds.GetGeoTransform(transform);
                    ds = null;
                    this._gv.topo.lakeInlets = new Dictionary<int, HashSet<int>>();
                    this._gv.topo.lakeOutlets = new Dictionary<int, HashSet<int>>();
                    var currentPointId = 0;
                    // calculate current maximum id in snap file
                    snapLayer.ResetReading();
                    OSGeo.OGR.Feature point;
                    do {
                        point = snapLayer.GetNextFeature();
                        if (point is null) { break; }
                        currentPointId = Math.Max(currentPointId, point.GetFieldAsInteger(ptIdIndex));
                    } while (point is not null);
                    // First pass: replace any multipart lake geometries with a single part.  
                    // Also eliminate any islands to prevent accidental crossings of channels with islands
                    // This assumes that dissolving has been done to get as much as possible of lake into
                    // one part, so remaining pieces are insignificant, and islands are also insignificant
                    var channelLinkIndex = this._gv.topo.getIndex(channelsLayer, Topology._LINKNO);
                    var areaFactor = this._gv.horizontalFactor * this._gv.horizontalFactor;
                    var geoMap = new Dictionary<long, OSGeo.OGR.Geometry>();
                    lakesLayer.ResetReading();
                    OSGeo.OGR.Feature lake;
                    do {
                        lake = lakesLayer.GetNextFeature();
                        if (lake is null) { break; }
                        if (lakeResIndex >= 0 && lake.GetFieldAsInteger(lakeResIndex) == (int)WaterRole._PLAYA) {
                            continue;
                        }
                        lakeId = lake.GetFieldAsInteger(lakeIdIndex);
                        geom = lake.GetGeometryRef();
                        if (geom.GetGeometryType() == wkbGeometryType.wkbMultiPolygon) {
                            // find the part with largest area to use as the lake
                            var maxArea = 0.0;
                            var secondArea = 0.0;
                            OSGeo.OGR.Geometry maxPart = null;
                            OSGeo.OGR.Geometry secondPart = null;
                            int count = geom.GetGeometryCount();
                            Utils.loginfo(string.Format("Lake {0} has {1} parts", lakeId, count));
                            var partNum = 0;
                            for (int i = 0; i < count; i++) {
                                // keep islands
                                // nextPart = QgsGeometry.fromPolygonXY([poly[0]]) # removes any islands within the polygon, by using outer (first) ring only
                                var nextPart = geom.GetGeometryRef(i);
                                var nextArea = nextPart.Area() * areaFactor;
                                var nextLen = nextPart.GetPointCount();
                                partNum += 1;
                                Utils.loginfo(string.Format("Part {0} has {1} perimeter vertices and area {2}", partNum, nextLen, Convert.ToInt32(nextArea)));
                                if (nextArea > maxArea) {
                                    if (maxArea > secondArea) {
                                        secondPart = maxPart;
                                        secondArea = maxArea;
                                    }
                                    maxPart = nextPart;
                                    maxArea = nextArea;
                                }
                            }
                            if (maxPart is null) {
                                Utils.error(string.Format("Lake {0} seems to be empty", lakeId), this._gv.isBatch);
                                return false;
                            }
                            if (secondPart is not null) {
                                var percent = Convert.ToInt32(secondArea * 100.0 / maxArea);
                                Utils.loginfo(string.Format("Second part area is {0}% of first", percent));
                                if (percent >= 5) {
                                    Utils.information(string.Format("Warning: part with {0} percent of main part of lake {1} is being ignored.", percent, lakeId), this._gv.isBatch, reportErrors: reportErrors);
                                }
                            }
                            geoMap[lake.GetFID()] = maxPart;
                        } else {
                            geoMap[lake.GetFID()] = geom;
                            if (geom.GetGeometryType() != wkbGeometryType.wkbPolygon) {
                                Utils.error(string.Format("Lake {0} does not seem to be a polygon shapefile", lakeId), this._gv.isBatch);
                                return false;
                            }
                        }
                    } while (lake is not null);
                    var newPoints = new List<OSGeo.OGR.Feature>();
                    lakesLayer.ResetReading();
                    do {
                        lake = lakesLayer.GetNextFeature();
                        if (lake is null) { break; }
                        if (lakeResIndex >= 0 && lake.GetFieldAsInteger(lakeResIndex) == (int)WaterRole._PLAYA) {
                            continue;
                        }
                        lakeId = lake.GetFieldAsInteger(lakeIdIndex);
                        if (lakeResIndex < 0) {
                            res = (int)WaterRole._RESERVOIR;
                        } else {
                            if (this._gv.isHUC || this._gv.isHAWQS) {
                                res = (lake.GetFieldAsString(lakeResIndex) == "Reservoir") ? 1 : 2;
                            } else {
                                res = lake.GetFieldAsInteger(lakeResIndex);
                            }
                        }
                        geom = geoMap[lake.GetFID()];
                        this._gv.topo.lakeInlets[lakeId] = new HashSet<int>();
                        this._gv.topo.lakeOutlets[lakeId] = new HashSet<int>();
                        var lakeBox = new OSGeo.OGR.Envelope();
                        geom.GetEnvelope(lakeBox);
                        // geometry was made single in first pass
                        // channels which enter and leave the lake, with their crossing points
                        // if just one of these and no other outlet will make inflowing and outflowing pair,
                        // else assumed not to interact with lake
                        var crossingChannels = new List<(OSGeo.OGR.Feature, OSGeo.OGR.Geometry)>();
                        channelsLayer.ResetReading();
                        OSGeo.OGR.Feature channel;
                        do {
                            channel = channelsLayer.GetNextFeature();
                            if (channel is null) { break; }
                            chLink = channel.GetFieldAsInteger(channelLinkIndex);
                            var line = channel.GetGeometryRef();
                            int numPoints = line.GetPointCount();
                            if (numPoints <= 1) { continue; } // degenerate line
                            var lineBox = new OSGeo.OGR.Envelope();
                            line.GetEnvelope(lineBox);
                            if (Topology.disjointBoxes(lakeBox, lineBox)) {
                                continue;
                            }
                            double[] start = new double[3];
                            double[] finish = new double[3];
                            line.GetPoint(0, start);
                            line.GetPoint(numPoints - 1, finish);
                            source = this._gv.topo.outletAtStart ? finish : start;
                            outlet = this._gv.topo.outletAtStart ? start : finish;
                            bool sourceInLake = Topology.polyContains(source, geom, lakeBox);
                            bool outletInLake = Topology.polyContains(outlet, geom, lakeBox);
                            var intersectLine = line.Intersection(geom);
                            if (intersectLine is null || intersectLine.IsEmpty()) { continue; }
                            if (Topology.withinPixels(1, source, outlet, transform)) {
                                Utils.error(string.Format(@"Channel with LINKNO {0} crosses the boundary of lake {1} 
but is too short to allow the creation of a crossing point.  You can try rerunning delineation with a different channel threshold,
either smaller to lengthen the stream or larger to remove it.  Or if the lake is very small you may need to remove it or enlarge it.", chLink, lakeId), this._gv.isBatch);
                                continue;
                            }
                            bool isMultiple = intersectLine.GetGeometryType() == wkbGeometryType.wkbMultiLineString;

                            if (outletInLake) {
                                if (sourceInLake) {
                                    if (isMultiple) {
                                        Utils.information(string.Format(@"Channel with LINKNO {0}  crosses the  boundary of lake {1}
more than once.  Since it starts and terminates in the lake it
will be assumed that its crossing the lake boundary is an 
inaccuaracy of delineation and that it is internal to the lake.
", chLink, lakeId), this._gv.isBatch, reportErrors: reportErrors);
                                    }
                                } else {
                                    if (isMultiple) {
                                        Utils.information(string.Format(@"Channel with LINKNO {0} crosses the  boundary of lake {1}
more than once.  Since it starts outside and terminates inside 
the lake it will be assumed that its extra crossings of the lake 
boundary are an inaccuaracy of delineation and that it enters 
the lake at its first crossing point.
", chLink, lakeId), this._gv.isBatch, reportErrors: reportErrors);
                                    }
                                    currentPointId = makeNewPoint(intersectLine, source, outlet, newPoints, currentPointId, true, res, chLink, lakeId, defn, transform);
                                }
                            } else {
                                if (sourceInLake) {
                                    if (isMultiple) {
                                        Utils.information(string.Format(@"Channel with LINKNO {0} crosses the  boundary of lake {1}
more than once.  Since it starts in the lake and terminates 
outside it will be assumed that its multiple crossings 
of the lake boundary are an inaccuaracy of delineation and 
that it flows out the lake at its last crossing point.
", chLink, lakeId), this._gv.isBatch, reportErrors: reportErrors);
                                    }
                                    currentPointId = makeNewPoint(intersectLine, source, outlet, newPoints, currentPointId, false, res, chLink, lakeId, defn, transform);
                                } else {
                                    // channel crosses the lake.  Store its intersection
                                    crossingChannels.Add((channel, intersectLine));
                                }
                            }
                        } while (channel is not null);
                        // check for channels which cross lake if no other outlets
                        var numOutlets = this._gv.topo.lakeOutlets[lakeId].Count;
                        if (numOutlets == 0 && crossingChannels.Count > 0) {
                            // arbtirarily select first such stream to generate the outlet
                            // users told to mark stream with a reservoir or pond point if this gives undesirable choice
                            var (channelF, intersect) = crossingChannels[0];
                            crossingChannels.RemoveAt(0);
                            reachData = await this._gv.topo.getOgrReachData(channelF, null);
                            outlet = new double[3] { reachData.lowerX, reachData.lowerY, 0 };
                            source = new double[3] { reachData.upperX, reachData.upperY, 0 };
                            chLink = channel.GetFieldAsInteger(channelLinkIndex);
                            currentPointId = makeNewPoint(intersect, source, outlet, newPoints, currentPointId, true, res, chLink, lakeId, defn, transform);
                            currentPointId = makeNewPoint(intersect, source, outlet, newPoints, currentPointId, false, res, chLink, lakeId, defn, transform);
                            // check the two points created (the outlet and inlet for the lake) are not in the same pixel.
                            // If they are, move the outlet downstream, else gridnet will not find both of them.
                            var count = newPoints.Count;
                            var lakeOutlet = Topology.geometryToPointAsDouble(newPoints[count - 2].GetGeometryRef());
                            var lakeInlet = Topology.geometryToPointAsDouble(newPoints[count - 1].GetGeometryRef());
                            if (Topology.withinPixels(0, lakeInlet, lakeOutlet, transform)) {
                                // shift outlet downstream until no longer in the same pixel as inlet
                                var newOutlet = this._gv.topo.shiftDownstream(lakeOutlet, channel, lakeInlet, transform);
                                if (newOutlet is null) {
                                    Utils.error(string.Format("Failed to find outlet far enough from inlet for lake {0}", lakeId), this._gv.isBatch, reportErrors: reportErrors);
                                } else {
                                    newPoints[count - 2].SetGeometry(Topology.geometryFromPoint(newOutlet));
                                    numOutlets += 1;
                                    Utils.loginfo(string.Format("Outlet of lake {0} shifted from ({1}, {2}) to ({3}, {4})", lakeId, lakeOutlet[0], lakeOutlet[1], newOutlet[0], newOutlet[1]));
                                }
                            } else {
                                numOutlets += 1;
                            }
                        }
                        foreach (var (channelF, intersect) in crossingChannels) {
                            var chLink0 = channelF.GetFieldAsInteger(channelLinkIndex);
                            Utils.information(string.Format(@"Channel with LINKNO {0} enters and then leaves lake {1}.  
Since it starts and terminates outside the lake it will be 
assumed that its crossing the lake boundary is an inaccuracy. 
", chLink0, lakeId), this._gv.isBatch, reportErrors: reportErrors);
                        }
                        if (numOutlets == 0) {
                            // last chance to include lake - check if it has a watershed outlet inside it
                            foreach (var (subbasin, (pointId, pt, _)) in this._gv.topo.outlets) {
                                if (this._gv.topo.isWatershedOutlet(pointId, channelsLayer, dsLinkIndex, dsNodeIndex) && Topology.polyContains(pt, geom, lakeBox)) {
                                    // outletsInLake needed for making deep aquifers later
                                    this._gv.topo.outletsInLake[subbasin] = lakeId;
                                    numOutlets += 1;
                                }
                            }
                        }
                        Utils.loginfo(string.Format("Lake {0} has {1} outlets", lakeId, numOutlets));
                        if (numOutlets == 0) {
                            var arcLakesLayer = Utils.getLayerByLegend(Utils._LAKESLEGEND) as FeatureLayer;
                            ArcGIS.Core.Data.QueryFilter qf = new ArcGIS.Core.Data.QueryFilter {
                                ObjectIDs = new List<long>() { lake.GetFID() }
                            };
                            if (ArcSWATPlus.mainMap != MapView.Active.Map) {
                                await ArcSWATPlus.mainMap.OpenViewAsync();
                            }
                            await QueuedTask.Run(() => {
                                arcLakesLayer.Select(qf);
                                MapView.Active.ZoomToSelected();
                            });
                            var text = string.Format(@"
    Failed to find any outlet points for lake {0}.
    It appears not to be on any channels.  Do you want to remove it?
    If you choose no you can try rerunning delineation with a smaller 
    channel threshold to get a channel to reach the lake, or you could
    try editing the lake boundary if it is already close to a channel, 
    or you could change it to a playa lake by setting its RES field to 4.
    ", lakeId);
                            this._dlg.WindowState = FormWindowState.Minimized;
                            var result = ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(Utils.trans(text), "Remove lake?", MessageBoxButton.YesNo);
                            this._dlg.WindowState = FormWindowState.Normal;
                            if (result == MessageBoxResult.Yes) {
                                lakesToRemove.Add(lake.GetFID());
                            }
                            await QueuedTask.Run(() => arcLakesLayer.ClearSelection());
                            continue;
                        }
                    } while (lake is not null);
                    foreach (var newPoint in newPoints) {
                        var result = snapLayer.CreateFeature(newPoint);
                        if (result != Ogr.OGRERR_NONE) {
                            Utils.error(string.Format("Failed to Add lake inlets and outlets to {0}", this._gv.snapFile), this._gv.isBatch);
                            return false;
                        }
                    }
                    foreach (var lakeid in lakesToRemove) {
                        var result = lakesLayer.DeleteFeature(lakeid);
                        if (result != Ogr.OGRERR_NONE) {
                            Utils.error("Failed to delete lake from lakes file", this._gv.isBatch);
                            return false;
                        }
                    }
                }
            }
            var numProcesses = this._dlg.numberOfProcesses;
            var mustRun = true;
            var suffix = System.IO.Path.GetExtension(this._gv.demFile);
            var @base = System.IO.Path.ChangeExtension(this._gv.demFile, null);
            // AreaD8 has an outlets shapefile input, but no need to rerun as adding lake inlets and outlets will not change the watershed area
            this.progress("GridNet ...");
            var gordFile = @base + "gord" + suffix;
            var plenFile = @base + "plen" + suffix;
            var tlenFile = @base + "tlen" + suffix;
            var ok = await this._gv.taudem.runGridNet(this._gv.pFile, plenFile, tlenFile, gordFile, this._gv.snapFile, numProcesses, this._dlg.getTaudemOutput, mustRun: mustRun);
            if (!ok) {
                this.cleanUp(3);
                return false;
            }
            var srcChannelFile = @base + "srcChannel" + suffix;
            this.progress("Threshold ...");
            await Utils.removeLayer(srcChannelFile);
            ok = await this._gv.taudem.runThreshold(this._gv.ad8File, srcChannelFile, channelThreshold, numProcesses, this._dlg.getTaudemOutput, mustRun: mustRun);
            if (!ok) {
                this.cleanUp(3);
                return false;
            }
            this._gv.srcChannelFile = srcChannelFile;
            this.progress("StreamNet ...");
            await Utils.removeLayer(this._gv.channelFile);
            await Utils.removeLayer(this._gv.channelBasinFile);
            // having this as a layer results in DSNODEIDs being set to zero
            await Utils.removeLayer(this._gv.snapFile);
            var ordChannelFile = @base + "ordChannel" + suffix;
            var treeChannelFile = @base + "treeChannel.dat";
            var coordChannelFile = @base + "coordChannel.dat";
            // if channel shapefile already exists and is a directory, set path to .shp
            this._gv.channelFile = Utils.dirToShapefile(this._gv.channelFile);
            ok = await this._gv.taudem.runStreamNet(this._gv.felFile, this._gv.pFile, this._gv.ad8File, this._gv.srcChannelFile, this._gv.snapFile, ordChannelFile, treeChannelFile, coordChannelFile, this._gv.channelFile, this._gv.channelBasinFile, false, numProcesses, this._dlg.getTaudemOutput, mustRun: mustRun);
            if (!ok) {
                this.cleanUp(3);
                return false;
            }
            // if channel shapefile is a directory, set path to .shp, since not done earlier if channel did not exist then
            this._gv.channelFile = Utils.dirToShapefile(this._gv.channelFile);
            // seems channel shapefile's prj file can be changed, so restore it
            Utils.copyPrj(this._gv.demFile, this._gv.channelFile);
            Utils.copyPrj(this._gv.demFile, this._gv.channelBasinFile);
            await Utils.removeLayer(this._gv.wshedFile);
            await this.createWatershedShapefile(this._gv.channelBasinFile, this._gv.wshedFile, FileTypes._WATERSHED);
            Utils.copyPrj(this._gv.demFile, this._gv.wshedFile);

            using (var channelsDs = Ogr.Open(this._gv.channelFile, 1)) {
                var channelsLayer = channelsDs.GetLayerByIndex(0);
                this._gv.topo.addBasinsToChannelFile(channelsLayer, this._gv.channelFile, this._gv.basinFile);
                // recalculate tables dependent on channels
                // reopen snap file
                using (var snapDs = Ogr.Open(this._gv.snapFile, 1)) {
                    var snapLayer = snapDs.GetLayerByIndex(0);
                    if (!this._gv.topo.saveOutletsAndSources(channelsLayer, snapLayer, this._gv.streamFile, false)) {
                        return false;
                    }
                }
            }

            // make demLayer (or hillshadelayer if exists) active so channelsArcLayer loads above it and below outlets
            // (or use Full HRUs layer if there is one)
            var fullHRUsLayer = Utils.getLayerByLegend(Utils._FULLHRUSLEGEND);
            ArcGIS.Desktop.Mapping.Layer subLayer;
            if (fullHRUsLayer is not null) {
                subLayer = fullHRUsLayer;
            } else {
                var hillshadeLayer = Utils.getLayerByLegend(Utils._HILLSHADELEGEND);
                if (hillshadeLayer is not null) {
                    subLayer = hillshadeLayer;
                } else {
                    subLayer = demLayer;
                }
            }
            if (this._gv.basinFile == "") {
                // some subbasins were merged
                this._gv.basinFile = await this.createBasinFile(this._gv.subbasinsFile, demLayer, "wStream");
            }
            var pair = await Utils.getLayerByFilename(this._gv.channelFile, FileTypes._CHANNELS, this._gv, subLayer, Utils._WATERSHED_GROUP_NAME);
            var channelsArcLayer = pair.Item1 as ArcGIS.Desktop.Mapping.FeatureLayer;
            bool loaded = pair.Item2; ;
            if (channelsArcLayer is null || !(loaded || this._gv.existingWshed)) {
                this.cleanUp(-1);
                return false;
            }
            this.lakePointsAdded = true;
            return true;

        }

        // Add lakes in HUC models.
        public async Task<bool> addHUCLakes(OSGeo.OGR.Layer lakesLayer, OSGeo.OGR.Layer channelsLayer, OSGeo.OGR.Layer subbasinsLayer, RasterLayer demLayer) {

            // Cannot run TauDEM as supplied channels will in general not coincide with calculated ones.
            // So we calculate new subbasins by subtracting lake from existing ones
            var subsNoLakes = System.IO.Path.GetDirectoryName(this._gv.subbasinsFile) + "/subsNoLakes.shp";
            await Utils.removeLayerAndFiles(subsNoLakes);
            var parms = Geoprocessing.MakeValueArray(System.IO.Path.GetDirectoryName(this._gv.subbasinsFile), "subsNoLakes.shp", "POLYGON");
            var result = await Geoprocessing.ExecuteToolAsync("management.CreateFeatureclass", parms);
            if (string.IsNullOrEmpty(result.ReturnValue)) {
                Utils.error("Failed to create subsNoLakes.shp: " + $@"{result.ErrorMessages}", this._gv.isBatch);
                return false;
            } else {
                Utils.loginfo("Created subsNoLakes.shp: " + $@"{result.ReturnValue}");
            }
            this._gv.chBasinNoLakeFile = this._gv.channelBasinFile;
            return true;
        }

        // If necessary Add an id attribute to the lakes shapefile.  Number lakes with water body ids.
        //         label lakes with the id value.  Add lake ids to lake ids combo.  Return the index to the id attribute.
        //         Also return true second result if lakes include reservoirs or ponds or wetlands, 
        //         and true third result if they include playas.
        public virtual (int, bool, bool) identifyLakes(OSGeo.OGR.Layer lakesLayer) {
            var defn = lakesLayer.GetLayerDefn();
            if (this._gv.isHUC || this._gv.isHAWQS) {
                return (defn.GetFieldIndex(Topology._HUCLAKEID), true, false);
            }
            // label lakes with water body numbers
            var lakeIdIndex = defn.GetFieldIndex(Topology._LAKEID);
            var lakeResIndex = defn.GetFieldIndex(Topology._RES);
            var lakeIds = new List<int>();
            var hasResOrPondOrWetland = lakeResIndex < 0;
            var hasPlaya = false;
            if (lakeIdIndex < 0) {
                var ok = lakesLayer.CreateField(new FieldDefn(Topology._LAKEID, OSGeo.OGR.FieldType.OFTInteger), 1);
                if (ok != Ogr.OGRERR_NONE) {
                    Utils.error(string.Format("Unable to edit lakes shapefile {0}", this._gv.lakeFile), this._gv.isBatch);
                    return (-1, false, false);
                }
                lakeIdIndex = defn.GetFieldIndex(Topology._LAKEID);
                this._gv.topo.waterBodyId = this.getMaxWaterId();
                lakesLayer.ResetReading();
                OSGeo.OGR.Feature lake;
                do {
                    lake = lakesLayer.GetNextFeature();
                    if (lake is null) { break; }
                    if (!hasResOrPondOrWetland) {
                        if (new HashSet<WaterRole> {
                            WaterRole._RESERVOIR,
                            WaterRole._POND,
                            WaterRole._WETLAND
                        }.Contains((WaterRole)lake.GetFieldAsInteger(lakeResIndex))) {
                            hasResOrPondOrWetland = true;
                        }
                    }
                    if (!hasPlaya) {
                        if (lakeResIndex >= 0 && lake.GetFieldAsInteger(lakeResIndex) == (int)WaterRole._PLAYA) {
                            hasPlaya = true;
                        }
                    }
                    this._gv.topo.waterBodyId += 1;
                    lake.SetField(lakeIdIndex, this._gv.topo.waterBodyId);
                    lakeIds.Add(this._gv.topo.waterBodyId);
                    ok = lakesLayer.SetFeature(lake);
                    if (ok != Ogr.OGRERR_NONE) {
                        Utils.error(string.Format("Unable to update lakes shapefile {0}", this._gv.lakeFile), this._gv.isBatch);
                        return (-1, false, false);
                    }

                } while (lake is not null);
            } else {
                OSGeo.OGR.Feature lake;
                do {
                    lake = lakesLayer.GetNextFeature();
                    if (lake is null) { break; }
                    int lakeId;
                    try {
                        lakeId = lake.GetFieldAsInteger(lakeIdIndex);
                    }
                    catch (Exception) {
                        Utils.error(string.Format("LakeId {0} cannot be parsed as an integer", lake.GetFieldAsString(lakeIdIndex)), this._gv.isBatch);
                        return (-1, false, false);
                    }
                    if (!hasResOrPondOrWetland) {
                        if (new HashSet<WaterRole> {
                            WaterRole._RESERVOIR,
                            WaterRole._POND,
                            WaterRole._WETLAND
                        }.Contains((WaterRole)lake.GetFieldAsInteger(lakeResIndex))) {
                            hasResOrPondOrWetland = true;
                        }
                    }
                    if (!hasPlaya) {
                        if (lakeResIndex >= 0 && lake.GetFieldAsInteger(lakeResIndex) == (int)WaterRole._PLAYA) {
                            hasPlaya = true;
                        }
                    }
                    if (lakeIds.Contains(lakeId)) {
                        Utils.error(string.Format("LakeId {0} appears at least twice in {1}: LakeId values must be unique", this._gv.lakeFile), this._gv.isBatch);
                        return (-1, false, false);
                    }
                    ListFuns.insertIntoSortedIntList(lakeId, lakeIds, true);
                    this._gv.topo.waterBodyId = Math.Max(this._gv.topo.waterBodyId, lakeId);
                } while (lake is not null);
            }
            if (this._gv.useGridModel) {
                if (this._dlg.delinLakeIdCombo.Items.Count == 0) {
                    foreach (var lakeId in lakeIds) {
                        this._dlg.delinLakeIdCombo.Items.Add(lakeId);
                    }
                }
            } else {
                //lakesLayer.setLabelsEnabled(true);
                //lakesLayer.triggerRepaint();
            }
            return (lakeIdIndex, hasResOrPondOrWetland, hasPlaya);
        }

        // Return maximum of 0 and reservoir or pond id values in inlets/outlets file if any.
        public virtual int getMaxWaterId() {
            var result = 0;
            if (this._dlg.useOutletsChecked && System.IO.File.Exists(this._gv.outletFile)) {
                using (var ds = Ogr.Open(this._gv.outletFile, 0)) {
                    var layer = ds.GetLayerByIndex(0);
                    var idIndex = this._gv.topo.getIndex(layer, Topology._ID);
                    var resIndex = this._gv.topo.getIndex(layer, Topology._RES);
                    if (resIndex >= 0) {
                        layer.SetAttributeFilter(string.Format("{0}>0", Topology._RES));
                        layer.ResetReading();
                        OSGeo.OGR.Feature point;
                        do {
                            point = layer.GetNextFeature();
                            if (point is null) { break; }
                            result = Math.Max(result, point.GetFieldAsInteger(idIndex));
                        } while (point is not null);
                    }
                }
            }
            return result;
        }

        // Put LakeId values into lakeIdCombo.
        public async Task populateLakeIdCombo(FeatureLayer lakesLayer) {
            var lakeIds = new List<int>();
            await QueuedTask.Run(() => {
                using (var rc = lakesLayer.Search()) {
                    while (rc.MoveNext()) {
                        var lake = rc.Current;
                        var lakeId = (int)lake[Topology._LAKEID];
                        ListFuns.insertIntoSortedIntList(lakeId, lakeIds, true);
                    }
                }
            });
            this._dlg.delinLakeIdCombo.Items.Clear();
            foreach (var lakeId in lakeIds) {
                this._dlg.delinLakeIdCombo.Items.Add(lakeId);
            }
        }

        // Convert reservoir, pond and wetland lakes into collections of grid cells and mark the cells with the lake id and RES value.
        public async Task makeGridLakes(OSGeo.OGR.Layer lakesLayer, OSGeo.OGR.Layer gridLayer) {
            WaterRole waterRole;
            var ok = Ogr.OGRERR_NONE;
            if (this.gridLakesAdded) {
                return;
            }
            var (lakeIdIndex, _, _) = this.identifyLakes(lakesLayer);
            if (lakeIdIndex < 0) {
                return;
            }
            var lakeResIndex = this._gv.topo.getIndex(lakesLayer, Topology._RES, ignoreMissing: true);
            if (lakeResIndex < 0) {
                Utils.information(string.Format("No RES field in lakes shapefile {0}: assuming lakes are reservoirs", lakesLayer.GetName()), this._gv.isBatch);
            }
            var areaIndex = this._gv.topo.getIndex(gridLayer, Parameters._AREA);
            var gridLakeIdIndex = this._gv.topo.getIndex(gridLayer, Topology._LAKEID);
            var gridResIndex = this._gv.topo.getIndex(gridLayer, Topology._RES, ignoreMissing: true);
            if (gridResIndex < 0) {
                ok = gridLayer.CreateField(new FieldDefn(Topology._RES, OSGeo.OGR.FieldType.OFTInteger), 1);
                if (ok != Ogr.OGRERR_NONE) {
                    Utils.error(string.Format("Unable to edit grid shapefile {0}", gridLayer.GetName()), this._gv.isBatch);
                    return;
                }
                gridResIndex = this._gv.topo.getIndex(gridLayer, Topology._RES);
            }
            var areaFactor = this._gv.horizontalFactor * this._gv.horizontalFactor;
            // list of ids of lakes already added to grid
            var done = new List<int>();
            var hasPlaya = false;
            lakesLayer.ResetReading();
            do {
                var lake = lakesLayer.GetNextFeature();
                if (lake is null) { break; }
                var lakeId = lake.GetFieldAsInteger(lakeIdIndex);
                if (lakeResIndex < 0) {
                    waterRole = WaterRole._RESERVOIR;
                } else {
                    waterRole = (WaterRole)lake.GetFieldAsInteger(lakeResIndex);
                    if (waterRole == WaterRole._PLAYA) {
                        hasPlaya = true;
                        continue;
                    }
                }
                var lakeGeom = lake.GetGeometryRef();
                var lakeBox = new OSGeo.OGR.Envelope();
                lakeGeom.GetEnvelope(lakeBox);
                gridLayer.ResetReading();
                do {
                    var cell = gridLayer.GetNextFeature();
                    if (cell is null) { break; }
                    var cellGeom = cell.GetGeometryRef();
                    var cellBox = new OSGeo.OGR.Envelope();
                    cellGeom.GetEnvelope(cellBox);
                    var cellAreaThreshold = 0.5 * cell.GetFieldAsDouble(areaIndex) * 10000.0;
                    // ensure we remove old lake ids, but not lake ids already added
                    try {
                        var currentLakeId = cell.GetFieldAsInteger(gridLakeIdIndex);
                        if (!done.Contains(currentLakeId)) {
                            cell.SetFieldNull(gridLakeIdIndex);
                            ok = gridLayer.SetFeature(cell);
                            if (ok != Ogr.OGRERR_NONE) {
                                Utils.error(string.Format("Cannot edit attributes of grid {0}", gridLayer.GetName()), this._gv.isBatch);
                                return;
                            }
                        }
                    } catch {
                    }
                    if (Topology.disjointBoxes(lakeBox, cellBox)) {
                        continue;
                    }
                    var lakePart = cellGeom.Intersection(lakeGeom);
                    if (lakePart is null) {
                        continue;
                    }
                    if (lakePart.Area() * areaFactor < cellAreaThreshold) {
                        continue;
                    }
                    cell.SetField(gridLakeIdIndex, lakeId);
                    cell.SetField(gridResIndex, (int)waterRole);
                    ok = gridLayer.SetFeature(cell);
                    if (ok != Ogr.OGRERR_NONE) {
                        Utils.error(string.Format("Cannot edit attributes of grid {0}", gridLayer.GetName()), this._gv.isBatch);
                        return;
                    }
                } while (true);
                if (hasPlaya) {
                    var demLayer = (await Utils.getLayerByFilename(this._gv.demFile, FileTypes._DEM, null, null, null)).Item1 as RasterLayer;
                    this._gv.playaFile = await this.createPlayaFile(this._gv.lakeFile, demLayer);
                } else {
                    this._gv.playaFile = "";
                }
                done.Add(lakeId);
            } while (true);
            this.gridLakesAdded = true;
            return;
        }

        // Create two raster files of distances to subbasin outlets and distances to channels, 
        //         unless they already exist or using existing watershed.
        public async Task makeDistancesToOutlets() {
            bool ok;
            if (!this._gv.existingWshed) {
                var haveDistSt = Utils.isUpToDate(this._gv.pFile, this._gv.distStFile);
                var haveDistCh = this._gv.useGridModel || Utils.isUpToDate(this._gv.pFile, this._gv.distChFile);
                //  haveValleyDepths = Utils.isUpToDate(self._gv.pFile, self._gv.valleyDepthsFile)
                if (haveDistSt && haveDistCh) {
                    // and haveValleyDepths:
                    return;
                }
                var demBase = System.IO.Path.ChangeExtension(this._gv.demFile, null);
                if (!haveDistSt) {
                    this.progress("Tributary stream lengths ...");
                    var threshold = await this._gv.topo.makeOutletThresholds(this._gv);
                    if (threshold > 0) {
                        this._gv.distStFile = demBase + "distst.tif";
                        // threshold is already double maximum ad8 value, so values anywhere near it can only occur at subbasin outlets; 
                        // use fraction of it to avoid any rounding problems
                        ok = await this._gv.taudem.runDistanceToStreams(this._gv.pFile, this._gv.hd8File, this._gv.distStFile, Convert.ToInt32(threshold * 0.9).ToString(), this._dlg.numberOfProcesses, this._dlg.getTaudemOutput, mustRun: this.thresholdChanged);
                        if (!ok) {
                            this.cleanUp(3);
                            return;
                        }
                    } else {
                        // Probably using existing watershed but switched tabs in delineation form
                        this._gv.distStFile = "";
                    }
                }
                if (!this._gv.useGridModel && !haveDistCh) {
                    this.progress("Channel flow lengths ...");
                    if (!Utils.isUpToDate(this._gv.demFile, this._gv.ad8File)) {
                        // Probably using existing watershed but switched tabs in delineation form
                        // At any rate, cannot calculate flow paths
                        Utils.error(string.Format("Flow accumulation file {0} missing or out of date", this._gv.ad8File), this._gv.isBatch);
                        return;
                    }
                    this._gv.distChFile = demBase + "distch.tif";
                    var channelThreshold = this._dlg.numCellsChText;
                    ok = await this._gv.taudem.runDistanceToStreams(this._gv.pFile, this._gv.ad8File, this._gv.distChFile, channelThreshold, this._dlg.numberOfProcesses, this._dlg.getTaudemOutput, mustRun: this.thresholdChanged);
                    if (!ok) {
                        this.cleanUp(3);
                        return;
                        // estimate valley depths for tributary slopes in hrus if not calculated for floodplain
                        // so don't calculate here
                        //             if not haveValleyDepths:
                        //                 self.progress('Valley depths ...')
                        //                 if self.L is None:
                        //                     self.L = Landscape(self._gv, self._dlg.getTaudemOutput, self._dlg.numberOfProcesses, self.progress)
                        //                 # mustRun probably left true by previous run: avoid unnecessary recalculation
                        //                 self.L.mustRun = False
                        //                 # this also generates valley depths
                        //                 threshold = int(self._dlg.numCellsSt.Text) if self._gv.useGridModel else int(self._dlg.numCellsCh.Text)
                        //                 clipper = self._gv.subbasinsFile if self.clipperFile == '' else self.clipperFile
                        //                 self._dlg.setCursor(Qt.WaitCursor)
                        //                 self.L.calcHillslopes(threshold, clipper)
                        //                 self._dlg.setCursor(Qt.ArrowCursor)
                        //                 Utils.loginfo('Created valley depths file {0}', self._gv.valleyDepthsFile))
                    }
                }
                this.progress("");
            } else {
                this._gv.distStFile = "";
            }
        }

        // 
        //         Return true if using grid model or basinFile is newer than subbasinsFile if using existing watershed,
        //         or subbasins file is newer than slopeFile file if using grid model,
        //         or  subbasinsFile is newer than DEM.
        //         
        public virtual bool checkDEMProcessed() {
            if (this._gv.existingWshed) {
                return this._gv.useGridModel || Utils.isUpToDate(this._gv.subbasinsFile, this._gv.basinFile);
            }
            if (this._gv.useGridModel) {
                return Utils.isUpToDate(this._gv.slopeFile, this._gv.subbasinsFile);
            } else {
                return Utils.isUpToDate(this._gv.demFile, this._gv.subbasinsFile);
            }
        }

        // Open and load DEM; set default threshold.
        public async Task btnSetDEM() {
            var pair = await Utils.openAndLoadFile(FileTypes._DEM, this._dlg.delinSelectDem, this._gv.demDir, this._gv, null, Utils._WATERSHED_GROUP_NAME);
            string demFile = pair.Item1;
            RasterLayer demMapLayer = pair.Item2 as RasterLayer;
            //Utils.printLayers(root, 1)
            if (!string.IsNullOrEmpty(demFile) && demMapLayer is not null) {
                this._gv.demFile = demFile;
                if (ArcSWATPlus.mainMap != MapView.Active.Map) {
                    await ArcSWATPlus.mainMap.OpenViewAsync();
                }
                await QueuedTask.Run(() => {
                    MapView.Active.ZoomTo(demMapLayer);
                    // set extent to DEM as otherwise defaults to full globe
                    var demExtent = demMapLayer.QueryExtent();
                    var map = ArcSWATPlus.mainMap;
                    map.SetCustomFullExtent(demExtent);
                    dynamic nodata = demMapLayer.GetRaster().GetNoDataValue();
                    var typ = nodata.GetType();
                    if (typ.IsArray) {
                        this._gv.elevationNoData = Convert.ToDouble(nodata[0]);
                    } else {
                        this._gv.elevationNoData = Convert.ToDouble(nodata);
                    }
                });
                await this.setDefaultNumCells(demMapLayer);
                // warn if large DEM
                var numCells = this.demWidth * this.demHeight;
                if (numCells > 4000000.0) {
                    var millions = Convert.ToInt32(numCells / 1000000.0);
                    Utils.information(string.Format("Large DEM", "This DEM has over {0} million cells and could take some time to process.  Be patient!", millions), this._gv.isBatch);
                }
                //Utils.printLayers(root, 2)
                //this.addHillshade(demFile, demMapLayer, this._gv);
                //Utils.printLayers(root, 6)
            }
        }

        // unnecessary with ArcGIS Pro
        ////  Create hillshade layer and load.
        //public static object addHillshade(string demFile, object demMapLayer, object gv) {
        //    var hillshadeFile = os.path.splitext(demFile)[0] + "hillshade.tif";
        //    if (!Utils.isUpToDate(demFile, hillshadeFile)) {
        //        processing.run("native:hillshade", new Dictionary<object, object> {
        //        {
        //            "INPUT",
        //            demFile},
        //        {
        //            "BAND",
        //            1},
        //        {
        //            "Z_FACTOR",
        //            5},
        //        {
        //            "SCALE",
        //            1},
        //        {
        //            "AZIMUTH",
        //            315},
        //        {
        //            "ALTITUDE",
        //            45},
        //        {
        //            "COMPUTE_EDGES",
        //            false},
        //        {
        //            "ZEVENBERGEN",
        //            false},
        //        {
        //            "COMBINED",
        //            false},
        //        {
        //            "MULTIDIRECTIONAL",
        //            false},
        //        {
        //            "OPTIONS",
        //            ""},
        //        {
        //            "EXTRA",
        //            ""},
        //        {
        //            "OUTPUT",
        //            hillshadeFile}});
        //        // run gdaldem to generate hillshade.tif
        //        //             if Parameters._ISWIN:
        //        //                 gdaldem = 'gdaldem.exe'
        //        //             else:
        //        //                 gdaldem = 'gdaldem'
        //        //             Utils.tryRemoveLayerAndFiles(hillshadeFile)
        //        //             command = '"{0}" hillshade -compute_edges -z 5 "{1}" "{2}"', gdaldem, demFile, hillshadeFile)
        //        //             proc = subprocess.run(command,
        //        //                                 shell=True,
        //        //                                 stdout=subprocess.PIPE,
        //        //                                 stderr=subprocess.STDOUT,
        //        //                                 universal_newlines=True)    # text=True) only in python 3.7
        //        //             Utils.loginfo('Creating hillshade ...')
        //        //             Utils.loginfo(command)
        //        //             for line in  proc.stdout.split('\n'):
        //        //                 Utils.loginfo(line)
        //        if (!File.Exists(hillshadeFile)) {
        //            Utils.information(string.Format("Failed to create hillshade file {0}", hillshadeFile), gv.isBatch);
        //            return;
        //        }
        //        Utils.copyPrj(demFile, hillshadeFile);
        //    }
        //    // Add hillshade above DEM
        //    // demLayer allowed to be None for batch running
        //    if (demMapLayer) {
        //        var demLayer = root.findLayer(demMapLayer.GetFID());
        //        //Utils.printLayers(root, 3)
        //        var hillMapLayer = Utils.getLayerByFilename(hillshadeFile, FileTypes._HILLSHADE, gv, demLayer, Utils._WATERSHED_GROUP_NAME)[0];
        //        //Utils.printLayers(root, 4)
        //        if (!hillMapLayer) {
        //            Utils.information("Failed to load hillshade file {0}", hillshadeFile), gv.isBatch);
        //            return;
        //        }
        //        // compress legend entry: need to find tree layer
        //        var hillTreeLayer = root.findLayer(hillMapLayer.GetFID());
        //        Debug.Assert(hillTreeLayer is not null);
        //        hillTreeLayer.setExpanded(false);
        //        hillMapLayer.renderer().setOpacity(0.4);
        //        hillMapLayer.triggerRepaint();
        //        //Utils.printLayers(root, 5)
        //    }
        //}

        // Open and load stream network to burn in.
        public async Task btnSetBurn() {
            (string burnFile, ArcGIS.Desktop.Mapping.Layer burnLayer) = await Utils.openAndLoadFile(FileTypes._BURN, this._dlg.delinSelectBurn, this._gv.shapesDir, this._gv, null, Utils._WATERSHED_GROUP_NAME);
            if (!string.IsNullOrEmpty(burnFile) && burnLayer is not null && burnLayer.GetType() == typeof(FeatureLayer)) {
                if (((FeatureLayer)burnLayer).ShapeType != ArcGIS.Core.CIM.esriGeometryType.esriGeometryPolyline) {
                    Utils.error(string.Format("Burn in file {0} is not a line shapefile", burnFile), this._gv.isBatch);
                } else {
                    this._gv.burnFile = burnFile;
                }
            }
        }

        // Open and load inlets/outlets shapefile.
        public async Task btnSetOutlets() {
            TextBox box;
            if (this._gv.existingWshed) {
                Debug.Assert(this._dlg.delinTabWidget.SelectedIndex == 1);
                box = this._dlg.delinSelectExistOutlets;
            } else {
                Debug.Assert(this._dlg.delinTabWidget.SelectedIndex == 0);
                box = this._dlg.delinSelectOutlets;
                this.thresholdChanged = true;
            }
            var ft = this._gv.isHUC || this._gv.isHAWQS ? FileTypes._OUTLETSHUC : FileTypes._OUTLETS;
            (string outletFile, ArcGIS.Desktop.Mapping.Layer outletLayer) = await Utils.openAndLoadFile(ft, box, this._gv.shapesDir, this._gv, null, Utils._WATERSHED_GROUP_NAME);
            if (!string.IsNullOrEmpty(outletFile) && outletLayer is not null && outletLayer.GetType() == typeof(FeatureLayer)) {
                this._gv.snapFile = "";
                this._dlg.snappedLabelText = "";
                if (((FeatureLayer)outletLayer).ShapeType != ArcGIS.Core.CIM.esriGeometryType.esriGeometryPoint &&
                    ((FeatureLayer)outletLayer).ShapeType != ArcGIS.Core.CIM.esriGeometryType.esriGeometryMultipoint) {
                    Utils.error(string.Format("Inlets/outlets file {0} is not a point shapefile", outletFile), this._gv.isBatch);
                } else {
                    this._gv.outletFile = outletFile;
                }
            }
        }

        // Open and load existing subbasins shapefile.
        public async Task btnSetSubbasins() {
            var ft = this._gv.useGridModel ? FileTypes._GRID : FileTypes._EXISTINGSUBBASINS;
            (string subbasinsFile, ArcGIS.Desktop.Mapping.Layer subbasinsLayer) = await Utils.openAndLoadFile(ft, this._dlg.delinSelectSubbasins, this._gv.shapesDir, this._gv, null, Utils._WATERSHED_GROUP_NAME);
            if (!string.IsNullOrEmpty(subbasinsFile) && subbasinsLayer is not null && subbasinsLayer.GetType() == typeof(FeatureLayer)) {
                if (((FeatureLayer)subbasinsLayer).ShapeType != ArcGIS.Core.CIM.esriGeometryType.esriGeometryPolygon) {
                    Utils.error(string.Format("Subbasins file {0} is not a polygon shapefile", subbasinsFile), this._gv.isBatch);
                } else {
                    this._gv.subbasinsFile = subbasinsFile;
                    //subbasinsLayer.setLabelsEnabled(true);
                    if (this._gv.useGridModel) {
                        // don't expand grid layer legend
                        var treeLayer = Utils.getLayerByLegend(Utils._GRIDLEGEND);
                        if (treeLayer is not null) {
                            await QueuedTask.Run(() => {
                                var featureDef = treeLayer.GetDefinition();
                                featureDef.Expanded = false;
                            });
                        }
                    }
                }
            }
        }

        // Open and load existing watershed shapefile.
        public async Task btnSetWshed() {
            var ft = FileTypes._EXISTINGWATERSHED;
            (string wshedFile, ArcGIS.Desktop.Mapping.Layer wshedLayer) = await Utils.openAndLoadFile(ft, this._dlg.delinSelectWshed, this._gv.shapesDir, this._gv, null, Utils._WATERSHED_GROUP_NAME);
            if (!string.IsNullOrEmpty(wshedFile) && wshedLayer is not null && wshedLayer.GetType() == typeof(FeatureLayer)) {
                if (((FeatureLayer)wshedLayer).ShapeType != ArcGIS.Core.CIM.esriGeometryType.esriGeometryPolygon) {
                    Utils.error(string.Format("Subbasins file {0} is not a polygon shapefile", wshedFile), this._gv.isBatch);
                } else {
                    this._gv.wshedFile = wshedFile;
                }
            }
        }

        // Open and load existing streams shapefile or drainage table.
        public async Task btnSetStreams() {
            string strng = "";
            int ft;
            var legend = this._gv.useGridModel ? Utils._GRIDLEGEND : Utils._SUBBASINSLEGEND;
            var subsLayer = Utils.getLayerByLegend(legend);
            if (this._gv.useGridModel) {
                if (!this.gridDrainage && !this.streamDrainage) {
                    ft = FileTypes._CSV;
                } else {
                    ft = FileTypes._GRIDSTREAMS;
                    strng = "Streams";
                }
            } else {
                ft = FileTypes._CHANNELS;
                strng = "Channels";
            }
            var saveDir = this._gv.shapesDir;
            (string channelFile, ArcGIS.Desktop.Mapping.Layer channelLayer) = await Utils.openAndLoadFile(ft, this._dlg.delinSelectStreams, saveDir, this._gv, subsLayer, Utils._WATERSHED_GROUP_NAME);
            if (!string.IsNullOrEmpty(channelFile) && channelLayer is not null && channelLayer.GetType() == typeof(FeatureLayer)) {
                if (((FeatureLayer)channelLayer).ShapeType != ArcGIS.Core.CIM.esriGeometryType.esriGeometryPolyline) {
                    Utils.error(string.Format("{0} file {1} is not a line shapefile", strng, channelFile), this._gv.isBatch);
                } else {
                    this._gv.channelFile = channelFile;
                    if (this._gv.useGridModel) {
                        this._gv.streamFile = channelFile;
                    }
                }
            } else if (ft == FileTypes._CSV && !string.IsNullOrEmpty(channelFile)) {
                this._dlg.delinSelectStreams.Text = channelFile;
                this.drainageTable = channelFile;
            }
        }

        // Run Taudem to create stream reach network.
        public virtual async Task runTauDEM1() {
            await this.runTauDEM(null, false);
        }

        // Run TauDEM to create watershed shapefile.
        public async Task runTauDEM2() {
            // first remove any existing shapesDir inlets/outlets file as will
            // probably be inconsistent with new subbasins
            // UNLESS the file is called arcextra.shp, when it was generated from an ArcSWAT project
            var extraFile = Utils.join(this._gv.shapesDir, "arcextra.shp");
            var extraLayer = (await Utils.getLayerByFilename(extraFile, FileTypes._OUTLETS, null, null, null)).Item1;
            if (extraLayer is null) {
                await Utils.removeLayerByLegend(Utils._EXTRALEGEND);
                this._gv.extraOutletFile = "";
            }
            if (!this._dlg.useOutletsChecked) {
                await this.runTauDEM(null, true);
            } else {
                var outletFile = this._dlg.delinSelectOutlets.Text;
                if (outletFile == "" || !System.IO.File.Exists(outletFile)) {
                    Utils.error("Please select an inlets/outlets file", this._gv.isBatch);
                    return;
                }
                // remove any snapped inlets/outlets file that is visible: causes DSNODEIDs to be set to zero
                await Utils.removeLayerByLegend(Utils._SNAPPEDLEGEND);
                await this.runTauDEM(outletFile, true);
            }
        }

        // Change between using existing and delineating watershed.
        public virtual void changeExisting() {
            var tab = this._dlg.delinTabWidget.SelectedIndex;
            if (tab > 1) {
                // DEM properties or TauDEM output
                return;
            }
            this._gv.existingWshed = tab == 1;
            this._dlg.setDrainage();
        }

        // Run TauDEM.
        public async Task runTauDEM(string outletFile, bool makeWshed) {
            ArcGIS.Desktop.Mapping.RasterLayer demLayer;
            ArcGIS.Desktop.Mapping.FeatureLayer chanLayer;
            ArcGIS.Desktop.Mapping.Layer subLayer;
            string delineationDem;
            string channelThreshold = null;
            this.delineationFinishedOK = false;
            var demFile = this._dlg.delinSelectDem.Text;
            if (string.IsNullOrEmpty(demFile) || !System.IO.File.Exists(demFile)) {
                Utils.error("Please select a DEM file", this._gv.isBatch);
                return;
            }
            this.isDelineated = false;
            this.lakesDone = false;
            this.lakePointsAdded = false;
            this.gridLakesAdded = false;
            this._gv.writeProjectConfig(0, 0);
            this._dlg.setMergeResGroups(this.isDelineated);
            this._gv.demFile = demFile;
            int streamCells;
            int channelCells;
            var streamThreshold = this._dlg.numCellsStText;
            if (!this._gv.useGridModel) {
                channelThreshold = this._dlg.numCellsChText;
                try {
                    streamCells = Convert.ToInt32(streamThreshold);
                } catch (Exception) {
                    Utils.exceptionError(string.Format("Cannot read stream threshold cell count {0}", streamThreshold), this._gv.isBatch);
                    return;
                }
                try {
                    channelCells = Convert.ToInt32(channelThreshold);
                } catch (Exception) {
                    Utils.exceptionError(string.Format("Cannot read channel threshold cell count {0}", channelThreshold), this._gv.isBatch);
                    return;
                }
                if (channelCells > streamCells) {
                    Utils.error(string.Format("Channel threshold {0} cannot be greater than stream threshold {1}", channelCells, streamCells), this._gv.isBatch);
                    return;
                }
            }
            // find dem layer (or load it)
            demLayer = (await Utils.getLayerByFilename(this._gv.demFile, FileTypes._DEM, this._gv, null, Utils._WATERSHED_GROUP_NAME)).Item1 as RasterLayer;
            if (demLayer is null) {
                Utils.error(string.Format("Cannot load DEM {0}", this._gv.demFile), this._gv.isBatch);
                return;
            }
            // changing default number of cells 
            if (!await this.setDefaultNumCells(demLayer)) {
                return;
            }
            // check stream and channel thresholds not inconsistent
            if (!this._gv.useGridModel) {
                var streamThresh = Convert.ToInt32(this._dlg.numCellsStText);
                var chanThresh = Convert.ToInt32(this._dlg.numCellsChText);
                if (chanThresh > streamThresh) {
                    Utils.error(string.Format("Channel threshold (currently {0} cells) cannot be greater than the stream threshold (currently {1} cells)", chanThresh, streamThresh), this._gv.isBatch);
                    return;
                }
            }
            var suffix = System.IO.Path.GetExtension(this._gv.demFile);
            var @base = System.IO.Path.ChangeExtension(this._gv.demFile, null);
            var baseDir = System.IO.Path.GetDirectoryName(this._gv.demFile);
            var baseName = System.IO.Path.GetFileName(@base);
            var shapesBase = Utils.join(this._gv.shapesDir, baseName);
            // burn in if required
            if (this._dlg.checkBurnChecked) {
                var burnFile = this._dlg.delinSelectBurn.Text;
                if (burnFile == "") {
                    Utils.error("Please select a burn-in stream network shapefile", this._gv.isBatch);
                    return;
                }
                if (!System.IO.File.Exists(burnFile)) {
                    Utils.error(string.Format("Cannot find burn-in file {0}", burnFile), this._gv.isBatch);
                    return;
                }
                var burnedDemFile = @base + String.Format("_burned{0}.tif", this._gv.burninDepth);
                if (!Utils.isUpToDate(demFile, burnedDemFile) || !Utils.isUpToDate(burnFile, burnedDemFile)) {
                    // just in case
                    await Utils.removeLayerAndFiles(burnedDemFile);
                    this.progress("Burning streams ...");
                    Topology.burnStream(burnFile, demFile, burnedDemFile, this._gv.burninDepth, this._gv.verticalFactor, this._gv.isBatch);
                    if (!System.IO.File.Exists(burnedDemFile)) {
                        return;
                    }
                }
                this._gv.burnedDemFile = burnedDemFile;
                delineationDem = burnedDemFile;
            } else {
                this._gv.burnedDemFile = "";
                delineationDem = demFile;
            }
            // delete existing subs and rivs layers from earlier runs as they will be out of date
            await Utils.removeLayerByLegend(Utils._SUBBASINSLEGEND);
            await Utils.removeLayerByLegend(Utils._CHANNELREACHESLEGEND);
            var numProcesses = this._dlg.numberOfProcesses;
            var mpiexecPath = this._gv.mpiexecPath;
            if (numProcesses > 0 && (mpiexecPath == "" || !System.IO.File.Exists(mpiexecPath))) {
                Utils.information(string.Format("Cannot find MPI program {0} so running TauDEM with just one process", mpiexecPath), this._gv.isBatch);
                numProcesses = 0;
                this._dlg.numberOfProcesses = 0;
            }
            if (this._dlg.showTaudemChecked) {
                this._dlg.delinTabWidget.SelectedIndex = 3;
            }
            using (new CursorWait()) {
                this._dlg.getTaudemOutput.Clear();
                // delet old flood files if any
                await this.deleteFloodFiles();
                var felFile = @base + "fel" + suffix;
                await Utils.removeLayer(felFile);
                this.progress("PitFill ...");
                var depmask = Utils.join(baseDir, "depmask.tif");
                if (!System.IO.File.Exists(depmask)) {
                    depmask = null;
                }
                var ok = await this._gv.taudem.runPitFill(delineationDem, depmask, felFile, numProcesses, this._dlg.getTaudemOutput);
                if (!ok) {
                    this.cleanUp(3);
                    return;
                }
                this._gv.felFile = felFile;
                var sd8File = @base + "sd8" + suffix;
                var pFile = @base + "p" + suffix;
                await Utils.removeLayer(sd8File);
                await Utils.removeLayer(pFile);
                this.progress("D8FlowDir ...");
                ok = await this._gv.taudem.runD8FlowDir(felFile, sd8File, pFile, numProcesses, this._dlg.getTaudemOutput);
                if (!ok) {
                    this.cleanUp(3);
                    return;
                }
                var slpFile = @base + "slp" + suffix;
                var angFile = @base + "ang" + suffix;
                await Utils.removeLayer(slpFile);
                await Utils.removeLayer(angFile);
                this.progress("DinfFlowDir ...");
                ok = await this._gv.taudem.runDinfFlowDir(felFile, slpFile, angFile, numProcesses, this._dlg.getTaudemOutput);
                if (!ok) {
                    this.cleanUp(3);
                    return;
                }
                var ad8File = @base + "ad8" + suffix;
                await Utils.removeLayer(ad8File);
                this.progress("AreaD8 ...");
                ok = await this._gv.taudem.runAreaD8(pFile, ad8File, null, null, numProcesses, this._dlg.getTaudemOutput, mustRun: this.thresholdChanged);
                if (!ok) {
                    this.cleanUp(3);
                    return;
                }
                var scaFile = @base + "sca" + suffix;
                await Utils.removeLayer(scaFile);
                this.progress("AreaDinf ...");
                ok = await this._gv.taudem.runAreaDinf(angFile, scaFile, null, numProcesses, this._dlg.getTaudemOutput, mustRun: this.thresholdChanged);
                if (!ok) {
                    this.cleanUp(3);
                    return;
                }
                var gordFile = @base + "gord" + suffix;
                var plenFile = @base + "plen" + suffix;
                var tlenFile = @base + "tlen" + suffix;
                await Utils.removeLayer(gordFile);
                await Utils.removeLayer(plenFile);
                await Utils.removeLayer(tlenFile);
                this.progress("GridNet ...");
                ok = await this._gv.taudem.runGridNet(pFile, plenFile, tlenFile, gordFile, null, numProcesses, this._dlg.getTaudemOutput, mustRun: this.thresholdChanged);
                if (!ok) {
                    this.cleanUp(3);
                    return;
                }
                var srcStreamFile = @base + "srcStream" + suffix;
                await Utils.removeLayer(srcStreamFile);
                this.progress("Threshold ...");
                streamThreshold = this._dlg.numCellsStText;
                if (this._gv.isBatch) {
                    Utils.information(string.Format("Stream threshold: {0} cells", streamThreshold), true);
                }
                ok = await this._gv.taudem.runThreshold(ad8File, srcStreamFile, streamThreshold, numProcesses, this._dlg.getTaudemOutput, mustRun: this.thresholdChanged);
                if (!ok) {
                    this.cleanUp(3);
                    return;
                }
                string srcChannelFile = "";
                if (!this._gv.useGridModel) {
                    srcChannelFile = @base + "srcChannel" + suffix;
                    await Utils.removeLayer(srcChannelFile);
                    channelThreshold = this._dlg.numCellsChText;
                    if (this._gv.isBatch) {
                        Utils.information(string.Format("Channel threshold: {0} cells", channelThreshold), true);
                    }
                    ok = await this._gv.taudem.runThreshold(ad8File, srcChannelFile, channelThreshold, numProcesses, this._dlg.getTaudemOutput, mustRun: this.thresholdChanged);
                    if (!ok) {
                        this.cleanUp(3);
                        return;
                    }
                }
                var ordStreamFile = @base + "ordStream" + suffix;
                var streamFile = shapesBase + "stream.shp";
                // if stream shapefile already exists and is a directory, set path to .shp
                streamFile = Utils.dirToShapefile(streamFile);
                var treeStreamFile = @base + "treeStream.dat";
                var coordStreamFile = @base + "coordStream.dat";
                var wStreamFile = @base + "wStream" + suffix;
                await Utils.removeLayer(ordStreamFile);
                await Utils.removeLayer(streamFile);
                await Utils.removeLayer(wStreamFile);
                this.progress("StreamNet ...");
                ok = await this._gv.taudem.runStreamNet(felFile, pFile, ad8File, srcStreamFile, null, ordStreamFile, treeStreamFile, coordStreamFile, streamFile, wStreamFile, false, numProcesses, this._dlg.getTaudemOutput, mustRun: this.thresholdChanged);
                if (!ok) {
                    this.cleanUp(3);
                    return;
                }
                // if stream shapefile is a directory, set path to .shp, since not done earlier if streamFile did not exist then
                streamFile = Utils.dirToShapefile(streamFile);
                Utils.copyPrj(demFile, streamFile);
                Utils.copyPrj(demFile, wStreamFile);
                var channelFile = shapesBase + "channel.shp";
                var wChannelFile = @base + "wChannel" + suffix;
                var ordChannelFile = @base + "ordChannel" + suffix;
                var treeChannelFile = @base + "treeChannel.dat";
                var coordChannelFile = @base + "coordChannel.dat";
                if (!this._gv.useGridModel) {
                    // if channel shapefile already exists and is a directory, set path to .shp
                    channelFile = Utils.dirToShapefile(channelFile);
                    await Utils.removeLayer(ordChannelFile);
                    await Utils.removeLayer(channelFile);
                    await Utils.removeLayer(wChannelFile);
                    ok = await this._gv.taudem.runStreamNet(felFile, pFile, ad8File, srcChannelFile, null, ordChannelFile, treeChannelFile, coordChannelFile, channelFile, wChannelFile, false, numProcesses, this._dlg.getTaudemOutput, mustRun: this.thresholdChanged);
                    if (!ok) {
                        this.cleanUp(3);
                        return;
                    }
                    // if stream shapefile is a directory, set path to .shp, since not done earlier if streamFile did not exist then
                    channelFile = Utils.dirToShapefile(channelFile);
                    Utils.copyPrj(demFile, channelFile);
                    Utils.copyPrj(demFile, wChannelFile);
                }
                // load stream network
                // load above fullHRUs or hillshade or DEM
                var fullHRUsLayer = Utils.getLayerByLegend(Utils._FULLHRUSLEGEND);
                var hillshadeLayer = Utils.getLayerByLegend(Utils._HILLSHADELEGEND);
                if (fullHRUsLayer is not null) {
                    subLayer = fullHRUsLayer;
                } else if (hillshadeLayer is not null) {
                    subLayer = hillshadeLayer;
                } else {
                    subLayer = demLayer;
                }
                ArcGIS.Desktop.Mapping.FeatureLayer streamLayer;
                bool loaded;
                var pair = await Utils.getLayerByFilename(streamFile, FileTypes._STREAMS, this._gv, subLayer, Utils._WATERSHED_GROUP_NAME);
                streamLayer = pair.Item1 as FeatureLayer;
                loaded = pair.Item2;
                if (streamLayer is null || !loaded) {
                    this.cleanUp(-1);
                    return;
                }
                this._gv.streamFile = streamFile;
                this._gv.delinStreamFile = streamFile;
                ArcGIS.Desktop.Mapping.FeatureLayer channelLayer = null;
                if (!this._gv.useGridModel) {
                    pair = await Utils.getLayerByFilename(channelFile, FileTypes._CHANNELS, this._gv, subLayer, Utils._WATERSHED_GROUP_NAME);
                    channelLayer = pair.Item1 as FeatureLayer;
                    loaded = pair.Item2;
                    if (channelLayer is null || !loaded) {
                        this.cleanUp(-1);
                        return;
                    }
                    this._gv.channelFile = channelFile;
                }
                if (!makeWshed) {
                    this._gv.snapFile = "";
                    this._dlg.snappedLabelText = "";
                    // initial run to enable placing of outlets, so finishes with load of stream network
                    this._dlg.getTaudemOutput.AppendText("------------------- TauDEM finished -------------------\n");
                    this._dlg.saveProj();
                    this.cleanUp(-1);
                    return;
                }
                ArcGIS.Desktop.Mapping.FeatureLayer outletLayer;
                if (this._dlg.useOutletsChecked) {
                    Debug.Assert(outletFile is not null);
                    var outletBase = System.IO.Path.ChangeExtension(outletFile, null);
                    var snapFile = outletBase + "_snap.shp";
                    pair = await Utils.getLayerByFilename(outletFile, FileTypes._OUTLETS, this._gv, null, Utils._WATERSHED_GROUP_NAME);
                    outletLayer = pair.Item1 as FeatureLayer;
                    loaded = pair.Item2;
                    if (outletLayer is null) {
                        this.cleanUp(-1);
                        return;
                    }
                    if (outletLayer.ShapeType != ArcGIS.Core.CIM.esriGeometryType.esriGeometryPoint) {
                        Utils.error(string.Format("Inlets/Outlets file {0} is not a point shapefile", this._dlg.delinSelectOutlets.Text), this._gv.isBatch);
                        this.cleanUp(-1);
                        return;
                    }
                    HashSet<string> missingFields = new HashSet<string>();
                    if (await this._gv.topo.getIndex(outletLayer, Topology._ID, ignoreMissing: true) < 0) {
                        missingFields.Add(Topology._ID);
                    }
                    if (await this._gv.topo.getIndex(outletLayer, Topology._INLET, ignoreMissing: true) < 0) {
                        missingFields.Add(Topology._INLET);
                    }
                    if (await this._gv.topo.getIndex(outletLayer, Topology._RES, ignoreMissing: true) < 0) {
                        missingFields.Add(Topology._RES);
                    }
                    if (await this._gv.topo.getIndex(outletLayer, Topology._PTSOURCE, ignoreMissing: true) < 0) {
                        missingFields.Add(Topology._PTSOURCE);
                    }
                    if (missingFields.Count > 0) {
                        string fields = "";
                        foreach (string field in missingFields) {
                            fields += field + ", ";
                        }
                        fields = fields.Remove(fields.Length - 2);
                        fields += ".";
                        Utils.error(string.Format("There are fields missing from the inlets/outlets file {0}: {1}",
                            this._dlg.delinSelectOutlets.Text, fields), this._gv.isBatch);
                        this.cleanUp(-1);
                        return;
                    }
                    this.progress("SnapOutletsToStreams ...");
                    chanLayer = this._gv.useGridModel ? streamLayer : channelLayer;
                    ok = await this.createSnapOutletFile(outletLayer, streamLayer, chanLayer, outletFile, snapFile);
                    if (!ok) {
                        this.cleanUp(-1);
                        return;
                    }
                    // replaced by snapping
                    // outletMovedFile = outletBase + '_moved.shp'
                    // Utils.removeLayer(outletMovedFile, li)
                    // self.progress('MoveOutletsToStreams ...')
                    // ok = await this._gv.taudem.runMoveOutlets(pFile, srcFile, outletFile, outletMovedFile, numProcesses, self._dlg.getTaudemOutput, mustRun=self.thresholdChanged)
                    // if not ok:
                    //   self.cleanUp(3)
                    //    return
                    // repeat AreaD8, GridNet, Threshold and StreamNet with snapped outlets
                    var mustRun = this.thresholdChanged;
                    await Utils.removeLayer(ad8File);
                    this.progress("AreaD8 ...");
                    ok = await this._gv.taudem.runAreaD8(pFile, ad8File, this._gv.snapFile, null, numProcesses, this._dlg.getTaudemOutput, mustRun: mustRun);
                    if (!ok) {
                        this.cleanUp(3);
                        return;
                    }
                    this.progress("GridNet ...");
                    ok = await this._gv.taudem.runGridNet(pFile, plenFile, tlenFile, gordFile, this._gv.snapFile, numProcesses, this._dlg.getTaudemOutput, mustRun: mustRun);
                    if (!ok) {
                        this.cleanUp(3);
                        return;
                    }
                    await Utils.removeLayer(srcStreamFile);
                    this.progress("Threshold ...");
                    ok = await this._gv.taudem.runThreshold(ad8File, srcStreamFile, streamThreshold, numProcesses, this._dlg.getTaudemOutput, mustRun: mustRun);
                    if (!ok) {
                        this.cleanUp(3);
                        return;
                    }
                    if (!this._gv.useGridModel) {
                        await Utils.removeLayer(srcChannelFile);
                        ok = await this._gv.taudem.runThreshold(ad8File, srcChannelFile, channelThreshold, numProcesses, this._dlg.getTaudemOutput, mustRun: mustRun);
                        if (!ok) {
                            this.cleanUp(3);
                            return;
                        }
                    }
                    // for subbasins only need to consider inlets and outlets: lakes and point sources added to channels later
                    var ioSnapFile = await this.reduceToInletsOutlets();
                    if (ioSnapFile is null) {
                        this.cleanUp(-1);
                        return;
                    }
                    await Utils.removeLayer(streamFile);
                    // having this as a layer results in DSNODEIDs being set to zero
                    await Utils.removeLayer(ioSnapFile);
                    this.progress("StreamNet ...");
                    ok = await this._gv.taudem.runStreamNet(felFile, pFile, ad8File, srcStreamFile, ioSnapFile, ordStreamFile, treeStreamFile, coordStreamFile, streamFile, wStreamFile, false, numProcesses, this._dlg.getTaudemOutput, mustRun: mustRun);
                    if (!ok) {
                        this.cleanUp(3);
                        return;
                    }
                    Utils.copyPrj(demFile, streamFile);
                    Utils.copyPrj(demFile, wStreamFile);
                    if (!this._gv.useGridModel) {
                        await Utils.removeLayer(channelFile);
                        // having this as a layer results in DSNODEIDs being set to zero
                        await Utils.removeLayer(this._gv.snapFile);
                        ok = await this._gv.taudem.runStreamNet(felFile, pFile, ad8File, srcChannelFile, this._gv.snapFile, ordChannelFile, treeChannelFile, coordChannelFile, channelFile, wChannelFile, false, numProcesses, this._dlg.getTaudemOutput, mustRun: mustRun);
                        if (!ok) {
                            this.cleanUp(3);
                            return;
                        }
                        Utils.copyPrj(demFile, channelFile);
                        Utils.copyPrj(demFile, wChannelFile);
                    }
                    // load above demLayer (or hillshadelayer if exists) so streamLayer loads above it and below outlets
                    // (or use Full HRUs layer if there is one)
                    if (fullHRUsLayer is not null) {
                        subLayer = fullHRUsLayer;
                    } else if (hillshadeLayer is not null) {
                        subLayer = hillshadeLayer;
                    } else {
                        subLayer = demLayer;
                    }
                    (var streamLayer0, loaded) = await Utils.getLayerByFilename(streamFile, FileTypes._STREAMS, this._gv, subLayer, Utils._WATERSHED_GROUP_NAME);
                    if (streamLayer0 is null || !loaded) {
                        this.cleanUp(-1);
                        return;
                    }
                    streamLayer = streamLayer0 as FeatureLayer;
                    // load this later after adding BasinNo field
                    //if (!this._gv.useGridModel) {
                    //    (var channelLayer0, loaded) = await Utils.getLayerByFilename(channelFile, FileTypes._CHANNELS, this._gv, subLayer, Utils._WATERSHED_GROUP_NAME);
                    //    if (channelLayer0 is null || !loaded) {
                    //        this.cleanUp(-1);
                    //        return;
                    //    }
                    //    channelLayer = channelLayer0 as FeatureLayer;
                    //}
                    // check if stream network has only one feature
                    bool hasOneStream = await QueuedTask.Run<bool>(() => {
                        using (var fc = ((FeatureLayer)streamLayer).GetFeatureClass()) {
                            return (fc.GetCount() == 1);
                        }
                    });
                    if (hasOneStream) {
                        Utils.information("There is only one stream in your watershed, so you will only get one subbasin.  You may want to reduce the threshold.", this._gv.isBatch);
                    }
                }
                this._dlg.getTaudemOutput.AppendText("------------------- TauDEM finished -------------------\n");
                this._gv.pFile = pFile;
                this._gv.ad8File = ad8File;
                this._gv.basinFile = wStreamFile;
                if (!this._gv.useGridModel) {
                    this._gv.channelBasinFile = wChannelFile;
                }
                this._gv.srcChannelFile = this._gv.useGridModel ? "" : srcChannelFile;
                this._gv.slopeFile = slpFile;
                this._gv.streamFile = streamFile;
                this._gv.delinStreamFile = streamFile;
                if (!this._gv.useGridModel) {
                    this._gv.channelFile = channelFile;
                }
                this._gv.outletFile = this._dlg.useOutletsChecked ? outletFile : "";
                var subbasinsFile = shapesBase + "subbasins.shp";
                await this.createWatershedShapefile(wStreamFile, subbasinsFile, FileTypes._SUBBASINS);
                this._gv.subbasinsFile = subbasinsFile;
                if (!this._gv.useGridModel) {
                    var wshedFile = shapesBase + "wshed.shp";
                    await this.createWatershedShapefile(wChannelFile, wshedFile, FileTypes._WATERSHED);
                    this._gv.wshedFile = wshedFile;
                }
                string chanFile = "";
                if (this._gv.useGridModel) {
                    await this.createGridShapefile(pFile, ad8File, wStreamFile);
                    //var gridStreamLayer = Utils.getLayerByLegend(Utils._GRIDSTREAMSLEGEND);
                    //if (gridStreamLayer is null) {
                    //    Utils.error("Cannot find grid streams layer", this._gv.isBatch);
                    //    this.cleanUp(-1);
                    //    return;
                    //}
                    chanFile = this._gv.streamFile;
                } else if (!this._gv.useGridModel) {
                    using (var channelsDs = Ogr.Open(this._gv.channelFile, 1)) {
                        var channelsOgrLayer = channelsDs.GetLayerByIndex(0);
                        this._gv.topo.addBasinsToChannelFile(channelsOgrLayer, this._gv.channelFile, this._gv.basinFile);
                        //await this._gv.topo.addBasinsToChannelFile(channelLayer, this._gv.basinFile);
                    }
                    chanFile = channelFile;
                }
                // now load channels Layer
                if (!this._gv.useGridModel) {
                    (var channelLayer0, loaded) = await Utils.getLayerByFilename(channelFile, FileTypes._CHANNELS, this._gv, subLayer, Utils._WATERSHED_GROUP_NAME);
                    if (channelLayer0 is null) {
                        this.cleanUp(-1);
                        return;
                    }
                    channelLayer = channelLayer0 as FeatureLayer;
                }
                var possSnapFile = this._dlg.useOutletsChecked ? this._gv.snapFile : "";
                if (!await this._gv.topo.setUp0(demLayer, chanFile, possSnapFile, null, this._gv)) {
                    this.cleanUp(-1);
                    return;
                }
                Utils.loginfo(string.Format("{0} outlets after setUp0", this._gv.topo.outlets.Count));
                this.isDelineated = true;
                this._dlg.setMergeResGroups(this.isDelineated);
                this._dlg.saveProj();
                this.cleanUp(-1);
            }
        }

        // Do delineation from existing streams, channels, subbasins and watershed.
        public async Task runExisting() {
            int yGridSize;
            int xGridSize;
            double ySize;
            double xSize;
            int ft;
            string channelFile = "";
            string streamFile = "";
            string wshedFile = "";
            this.delineationFinishedOK = false;
            this.lakesDone = false;
            var demFile = this._dlg.delinSelectDem.Text;
            if (demFile == "" || !System.IO.File.Exists(demFile)) {
                Utils.error("Please select a DEM file", this._gv.isBatch);
                return;
            }
            this._gv.demFile = demFile;
            var subbasinsFile = this._dlg.delinSelectSubbasins.Text;
            if (subbasinsFile == "" || !System.IO.File.Exists(subbasinsFile)) {
                Utils.error(string.Format("Please select a {0}", this._dlg.selectSubbasinsLabelText.ToLower()), this._gv.isBatch);
                return;
            }
            if (this._gv.useGridModel) {
                if (!this.gridDrainage) {
                    if (this.streamDrainage) {
                        streamFile = this._dlg.delinSelectStreams.Text;
                        if (streamFile == "" || !System.IO.File.Exists(streamFile)) {
                            Utils.error("Please select a streams shapefile", this._gv.isBatch);
                            return;
                        }
                    } else {
                        var drainageTable = this._dlg.delinSelectStreams.Text;
                        if (drainageTable == "" || !System.IO.File.Exists(drainageTable)) {
                            Utils.error("Please select a drainage table csv file", this._gv.isBatch);
                            return;
                        }
                    }
                }
            } else {
                wshedFile = this._dlg.delinSelectWshed.Text;
                if (wshedFile == "" || !System.IO.File.Exists(wshedFile)) {
                    Utils.error("Please select a watershed shapefile", this._gv.isBatch);
                    return;
                }
                channelFile = this._dlg.delinSelectStreams.Text;
                if (channelFile == "" || !System.IO.File.Exists(channelFile)) {
                    Utils.error("Please select a channels shapefile", this._gv.isBatch);
                    return;
                }
            }
            var outletFile = this._dlg.delinSelectExistOutlets.Text;
            if (outletFile != "") {
                if (!System.IO.File.Exists(outletFile)) {
                    Utils.error(string.Format("Cannot find inlets/outlets shapefile {0}", outletFile), this._gv.isBatch);
                    return;
                }
            }
            this.isDelineated = false;
            this._dlg.setMergeResGroups(this.isDelineated);
            // find layers (or load them)
            ArcGIS.Desktop.Mapping.RasterLayer demLayer = (await Utils.getLayerByFilename(this._gv.demFile, FileTypes._DEM, this._gv, null, Utils._WATERSHED_GROUP_NAME)).Item1 as RasterLayer;
            if (demLayer is null) {
                Utils.error(string.Format("Cannot load DEM {0}", this._gv.demFile), this._gv.isBatch);
                return;
            }
            FeatureLayer subbasinsLayer = (await Utils.getLayerByFilename(subbasinsFile, FileTypes._EXISTINGSUBBASINS, this._gv, demLayer, Utils._WATERSHED_GROUP_NAME)).Item1 as FeatureLayer;
            if (subbasinsLayer is null) {
                Utils.error(string.Format("Cannot load subbasins shapefile {0}", subbasinsFile), this._gv.isBatch);
                return;
            }
            FeatureLayer wshedLayer = null;
            if (!this._gv.useGridModel) {
                wshedLayer = (await Utils.getLayerByFilename(wshedFile, FileTypes._EXISTINGWATERSHED, this._gv, subbasinsLayer, Utils._WATERSHED_GROUP_NAME)).Item1 as FeatureLayer;
                if (wshedLayer is null) {
                    Utils.error(string.Format("Cannot load watershed shapefile {0}", wshedFile), this._gv.isBatch);
                    return;
                } else {
                    await Utils.setLayerVisibility(wshedLayer, false);
                }
            }
            FeatureLayer outletLayer = null;
            if (outletFile != "") {
                ft = this._gv.isHUC || this._gv.isHAWQS ? FileTypes._OUTLETSHUC : FileTypes._OUTLETS;
                outletLayer = (await Utils.getLayerByFilename(outletFile, ft, this._gv, null, Utils._WATERSHED_GROUP_NAME)).Item1 as FeatureLayer;
                if (outletLayer is null) {
                    Utils.error(string.Format("Cannot load inlets/outlets shapefile {0}", outletFile), this._gv.isBatch);
                    return;
                }
            }
            using (new CursorWait()) {
                if (this._gv.useGridModel) {
                    if (this.gridDrainage || !this.streamDrainage) {
                        streamFile = await this.writeDrainageStreamsShapefile(subbasinsFile, subbasinsLayer, outletLayer);
                        if (streamFile is null) {
                            this.cleanUp(-1);
                            return;
                        }
                        ft = FileTypes._DRAINSTREAMS;
                    } else {
                        ft = FileTypes._GRIDSTREAMS;
                        // use streams for channels for grids
                    }
                    channelFile = streamFile;
                } else {
                    ft = FileTypes._CHANNELS;
                }
                FeatureLayer channelLayer = (await Utils.getLayerByFilename(channelFile, ft, this._gv, null, Utils._WATERSHED_GROUP_NAME)).Item1 as FeatureLayer;
                if (channelLayer is null) {
                    Utils.error(string.Format("Cannot load channels shapefile {0}", channelFile), this._gv.isBatch);
                    this.cleanUp(-1);
                    return;
                }
                // for HUC projects load lakes file 
                var lakeFile = "";
                if (this._gv.isHUC || this._gv.isHAWQS) {
                    if (this._gv.isHUC) {
                        lakeFile = this._gv.projDir + "/../lakes.shp";
                    } else {
                        lakeFile = this._dlg.delinSelectLakes.Text;
                    }
                    var lakeLayer = (await Utils.getLayerByFilename(lakeFile, FileTypes._LAKES,
                                                                 this._gv, null, Utils._WATERSHED_GROUP_NAME)).Item1 as FeatureLayer;
                    if (lakeLayer is null) {
                        Utils.error(String.Format("Cannot load lakes shapefile {0}", lakeFile), this._gv.isBatch);
                        this.cleanUp(-1);
                        return;
                    } else {
                        this._gv.lakeFile = lakeFile;
                    }
                }
                // ready to start processing
                string suffix = System.IO.Path.GetExtension(this._gv.demFile);
                string @base = System.IO.Path.ChangeExtension(this._gv.demFile, null);
                var numProcesses = this._dlg.numberOfProcesses;
                this._dlg.getTaudemOutput.Clear();
                // create Dinf slopes
                var felFile = @base + "fel" + suffix;
                var slpFile = @base + "slp" + suffix;
                var angFile = @base + "ang" + suffix;
                await Utils.removeLayer(slpFile);
                await Utils.removeLayer(angFile);
                this.progress("DinfFlowDir ...");
                // avoid running with HAWQS projects if possible
                bool willRun;
                if (this._gv.isHAWQS && System.IO.File.Exists(slpFile)) {
                    willRun = false;
                } else {
                    willRun = !(Utils.isUpToDate(demFile, slpFile) && Utils.isUpToDate(demFile, angFile));
                }
                if (willRun) {
                    if (this._dlg.showTaudemChecked) {
                        this._dlg.delinTabWidget.SelectedIndex = 3;
                    }
                    var ok = await this._gv.taudem.runDinfFlowDir(demFile, slpFile, angFile, numProcesses, this._dlg.getTaudemOutput);
                    if (!ok) {
                        this.cleanUp(3);
                        return;
                    }
                }
                if (this._gv.useGridModel) {
                    using (var subbasinsDs = Ogr.Open(subbasinsFile, 0)) {
                        var subbasinsOgrLayer = subbasinsDs.GetLayerByIndex(0);
                        if (this._gv.topo.basinCentroids.Count == 0) {
                            // set centroids
                            var basinIndex = this._gv.topo.getIndex(subbasinsOgrLayer, Topology._POLYGONID);
                            if (basinIndex < 0) {
                                return;
                            }
                            subbasinsOgrLayer.ResetReading();
                            OSGeo.OGR.Feature feature;
                            do {
                                feature = subbasinsOgrLayer.GetNextFeature();
                                if (feature is null) { break; }
                                var basin = feature.GetFieldAsInteger(basinIndex);
                                var (centroid, (xmin, xmax), (ymin, ymax)) = Utils.centreGridCell(feature);
                                this._gv.topo.basinCentroids[basin] = centroid;
                                if (this._gv.gridSize == 0) {
                                    // not set from project file
                                    var XYSizes = await QueuedTask.Run<Tuple<double, double>>(() => {
                                        return demLayer.GetRaster().GetMeanCellSize();
                                    });
                                    xSize = XYSizes.Item1;
                                    ySize = XYSizes.Item2;
                                    xGridSize = Convert.ToInt32((xmax - xmin) / xSize);
                                    yGridSize = Convert.ToInt32((ymax - ymin) / ySize);
                                    if (xGridSize == yGridSize) {
                                        this._gv.gridSize = xGridSize;
                                        Utils.loginfo(string.Format("Grid size set to {0}", this._gv.gridSize));
                                    } else {
                                        Utils.error(string.Format("Horizontal ({0}) and vertical ({1}) estimates of grid size are different.  Is your watershed shapefile a grid?", xGridSize, yGridSize), this._gv.isBatch);
                                        return;
                                    }
                                }
                            } while (feature is not null);
                        } else if (this._gv.gridSize == 0) {
                            // check grid size is set
                            var cell = subbasinsOgrLayer.GetNextFeature();
                            var (_, (xmin, xmax), (ymin, ymax)) = Utils.centreGridCell(cell);
                            var XYSizes = await QueuedTask.Run<Tuple<double, double>>(() => {
                                return demLayer.GetRaster().GetMeanCellSize();
                            });
                            xSize = XYSizes.Item1;
                            ySize = XYSizes.Item2;
                            xGridSize = Convert.ToInt32((xmax - xmin) / xSize);
                            yGridSize = Convert.ToInt32((ymax - ymin) / ySize);
                            if (xGridSize == yGridSize) {
                                this._gv.gridSize = xGridSize;
                                Utils.loginfo(string.Format("Grid size set to {0}", this._gv.gridSize));
                            } else {
                                Utils.error(string.Format("Horizontal ({0}) and vertical ({1}) estimates of grid size are different.  Is your watershed shapefile a grid?", xGridSize, yGridSize), this._gv.isBatch);
                                return;
                            }
                        }
                    }
                } else {
                    // generate subbasins raster
                    var wStreamFile = @base + "wStream" + suffix;
                    if (!(Utils.isUpToDate(demFile, wStreamFile) && Utils.isUpToDate(subbasinsFile, wStreamFile))) {
                        this.progress("Generating subbasins raster ...");
                        wStreamFile = await this.createBasinFile(subbasinsFile, demLayer, "wStream");
                    }
                    this._gv.basinFile = wStreamFile;
                    // No need as this is a requirement for grid models.  Risk of changing values - can get -1 for channels in lakes
                    // self._gv.topo.addBasinsToChannelFile(channelLayer, self._gv.basinFile)
                    // generate watershed raster
                    if (!this._gv.useGridModel) {
                        var wChannelFile = @base + "wChannel" + suffix;
                        if (!(Utils.isUpToDate(demFile, wChannelFile) && Utils.isUpToDate(wshedFile, wChannelFile))) {
                            this.progress("Generating watershed raster ...");
                            wChannelFile = await this.createBasinFile(wshedFile, demLayer, "wChannel");
                        }
                        this._gv.channelBasinFile = wChannelFile;
                    }
                }
                this._gv.felFile = felFile;
                this._gv.slopeFile = slpFile;
                this._gv.subbasinsFile = subbasinsFile;
                if (!this._gv.useGridModel) {
                    this._gv.wshedFile = wshedFile;
                    this._gv.channelFile = channelFile;
                    if (this._gv.isHUC || this._gv.isHAWQS) {
                        this._gv.streamFile = channelFile.Replace("channels.shp", "streams.shp");
                    }
                }
                this._gv.outletFile = outletFile;
                if (await this._gv.topo.setUp0(demLayer, channelFile, outletFile, null, this._gv)) {
                    this.isDelineated = true;
                    this._dlg.setMergeResGroups(this.isDelineated);
                    this._dlg.saveProj();
                }
                this.cleanUp(-1);
                return;
            }
        }
        

        // Set threshold number of stream cells to default of 1% of number in grid, 
        //         unless already set, and similarly default channel cells to 10% of stream cells.
        //         
        public async Task<bool> setDefaultNumCells(RasterLayer demLayer) {
            if (!await this.setDimensions(demLayer)) {
                return false;
            }
            // set to default number of cells unless already set
            if (this._dlg.numCellsStText == "") {
                var numCells = this.demWidth * this.demHeight;
                var defaultNumCellsSt = Convert.ToInt32(numCells * 0.01);
                this._dlg.numCellsStText = defaultNumCellsSt.ToString();
            } else {
                // already have a setting: keep same area but change number of cells according to dem cell size
                this._dlg.setNumCellsSt();
            }
            if (!this._gv.useGridModel) {
                if (this._dlg.numCellsChText == "") {
                    var defaultNumCellsCh = Convert.ToInt32(this._dlg.numCellsStText) / 10;
                    this._dlg.numCellsChText = defaultNumCellsCh.ToString();
                } else {
                    this._dlg.setNumCellsCh();
                }
            }
            return true;
        }

        // 
        //         Set dimensions of DEM.
        //         
        //         Note demLayer is a map layer (not tree)
        //         Also sets DEM properties tab.  Return true if successful.
        //         
        public async Task<bool> setDimensions(RasterLayer demLayer) {
            string @string;
            int epsg;
            ArcGIS.Core.Geometry.Unit unit;
            double factor;
            // can fail if demLayer is None or not projected
            try {
                await this._gv.topo.setCrs(demLayer, this._gv);
                if (this._gv.crsProject.IsGeographic) {
                    Utils.error("DEM does not seem to be projected", this._gv.isBatch);
                    return false;
                }
                unit = this._gv.crsProject.Unit;
                this._dlg.enableThresholds();
            } catch (Exception ex) {
                Utils.loginfo(string.Format("Failure to read DEM units: {0}", ex.Message));
                return false;
            }
            ArcGIS.Core.Data.Raster.Raster raster = null;
            int height = 0;
            int width = 0;
            await QueuedTask.Run(() => {
                raster = demLayer.GetRaster();
                height = raster.GetHeight();
                width = raster.GetWidth();
                this._gv.topo.demExtent = raster.GetExtent();
            });
            string wkt = this._gv.crsProject.Wkt;
            this._gv.xBlockSize = height > 1000 ? 1000 : height;
            this._gv.yBlockSize = width > 1000 ? 1000 : width;
            Utils.loginfo(string.Format("DEM horizontal and vertical block sizes are {0} and {1}", this._gv.xBlockSize, this._gv.yBlockSize));
            var demFile = await Utils.layerFilename(demLayer);
            var demPrj = System.IO.Path.ChangeExtension(demFile, ".prj");
            var demPrjTxt = demPrj + ".txt";
            if (System.IO.File.Exists(demPrj) && !System.IO.File.Exists(demPrjTxt)) {
                using (var f = new StreamWriter(demPrjTxt, true)) {
                    f.Write(wkt);
                }
            }
            this._dlg.textBrowserText = wkt;
            try {
                epsg = this._gv.crsProject.LatestWkid;
                Utils.loginfo(epsg.ToString());
                this._dlg.crsLabelText = string.Format("Spatial reference: EPSG:{0}", epsg);
                if (this._gv.isBatch && epsg > 0) {
                    var demDataFile = Utils.join(this._gv.projDir, "dem_data.xml");
                    if (!System.IO.File.Exists(demDataFile)) {
                        using (var f = new StreamWriter(demDataFile, true)) {
                            f.WriteLine("<demdata>");
                            f.WriteLine(string.Format("<epsg>{0}</epsg>", epsg));
                            f.WriteLine(string.Format("<minx>{0}</minx>", this._gv.topo.demExtent.XMin));
                            f.WriteLine(string.Format("<maxx>{0}</maxx>", this._gv.topo.demExtent.XMax));
                            f.WriteLine(string.Format("<miny>{0}</miny>", this._gv.topo.demExtent.YMin));
                            f.WriteLine(string.Format("<maxy>{0}</maxy>", this._gv.topo.demExtent.YMax));
                            f.WriteLine("</demdata>");
                        }
                    }
                }
            }
            catch (Exception) {
                // fail gracefully
                epsg = -1;
            }
            int uCode = unit.FactoryCode;
            if (uCode == ArcGIS.Core.Geometry.LinearUnit.Meters.FactoryCode) {
                factor = 1.0;
                this._dlg.delinHorizontalCombo.SelectedIndex = this._dlg.delinHorizontalCombo.Items.IndexOf(Parameters._METRES);
                this._dlg.delinHorizontalCombo.Enabled = false;
            } else if (uCode == ArcGIS.Core.Geometry.LinearUnit.Feet.FactoryCode) {
                factor = 0.3048;
                this._dlg.delinHorizontalCombo.SelectedIndex = this._dlg.delinHorizontalCombo.Items.IndexOf(Parameters._FEET);
                this._dlg.delinHorizontalCombo.Enabled = false;
            } else {
                if (uCode == ArcGIS.Core.Geometry.AngularUnit.Degrees.FactoryCode) {
                    @string = "degrees";
                    this._dlg.delinHorizontalCombo.SelectedIndex = this._dlg.delinHorizontalCombo.Items.IndexOf(Parameters._DEGREES);
                    this._dlg.delinHorizontalCombo.Enabled = false;
                } else {
                    @string = "unknown";
                    this._dlg.delinHorizontalCombo.SelectedIndex = this._dlg.delinHorizontalCombo.Items.IndexOf(Parameters._DEGREES);
                    this._dlg.delinHorizontalCombo.Enabled = true;
                }
                Utils.information("WARNING: DEM does not seem to be projected: its units are " + @string, this._gv.isBatch);
                return false;
            }
            this.demWidth = width;
            this.demHeight = height;
            var XYSizes = await QueuedTask.Run<Tuple<double, double>>(() => {
                return raster.GetMeanCellSize();
            });
            this._gv.topo.dx = XYSizes.Item1 * factor;
            this._gv.topo.dy = XYSizes.Item2 * factor;
            this._dlg.delinSizeEdit.Text = string.Format("{0:G4} x {1:G4}", this._gv.topo.dx, this._gv.topo.dy);
            this._dlg.delinSizeEdit.ReadOnly = true;
            var areaM2 = this._gv.topo.dx * this._gv.topo.dy;
            this._dlg.setAreaOfCell(areaM2);
            this._dlg.delinAreaEdit.Text = string.Format("{0:G4}", areaM2);
            this._dlg.delinAreaEdit.ReadOnly = true;
            var north = this._gv.topo.demExtent.YMax;
            var south = this._gv.topo.demExtent.YMin;
            var east = this._gv.topo.demExtent.XMax;
            var west = this._gv.topo.demExtent.XMin;
            var topLeft = this._gv.topo.pointToLatLong(new double[3] { west, north, 0 });
            var bottomRight = this._gv.topo.pointToLatLong(new double[3] { east, south, 0 });
            var northll = topLeft[1];
            var southll = bottomRight[1];
            var eastll = bottomRight[0];
            var westll = topLeft[0];
            this._dlg.delinNorthEdit.Text = Delineation.degreeString(northll);
            this._dlg.delinSouthEdit.Text = Delineation.degreeString(southll);
            this._dlg.delinEastEdit.Text = Delineation.degreeString(eastll);
            this._dlg.delinWestEdit.Text = Delineation.degreeString(westll);
            return true;
        }

        // Generate string showing degrees as decimal and as degrees minutes seconds.
        public static string degreeString(double decDeg) {
            var deg = Convert.ToInt32(decDeg);
            var decMin = Math.Abs(decDeg - deg) * 60;
            var minn = Convert.ToInt32(decMin);
            var sec = Convert.ToInt32((decMin - minn) * 60);
            return string.Format("{0}{1} ({2}{1} {3}\' {4}\")", string.Format(".F2", decDeg), @"\u00B0", deg, minn, sec);
        }

        // Set area of cell and update channel and stream area thresholds.
        public void changeAreaOfCell() {
            this._dlg.setAreaOfCell(this._gv.topo.dx * this._gv.topo.dy);
            if (!this._gv.useGridModel) {
                this._dlg.setAreaCh();
            }
            this._dlg.setAreaSt();
        }

        // Allow user to create inlets/outlets in current shapefile 
        //         or a new one.
        //      
        public async Task drawOutlets() {
            DialogResult result;
            //SelectPointViewModel vm = new SelectPointViewModel(null, false);
            //this.mapTool = new PointTool();
            var outletLayer = await Utils.getLayerByFilenameOrLegend(this._gv.outletFile, FileTypes._OUTLETS, "", this._gv.isBatch);
            if (outletLayer is not null) {
                // we have a current outlet layer - give user a choice
                result = new ChoosePointsFile().ShowDialog();
                if (result == DialogResult.Cancel) {
                    return;
                }
                this.drawCurrent = result == DialogResult.OK;  // returns Yes for new file
            } else {
                this.drawCurrent = false;
            }
            // always make a drawoutlets file.
            // if adding points is cancelled we will just discard it.
            // if drawCurrent is true we will add its features to outletLayer
            // if drawCurrent is false we make it the outletlayer
            this.drawOutletFile = Utils.join(this._gv.shapesDir, "drawoutlets.shp");
            // our outlet file may already be called drawoutlets.shp
            if (Utils.samePath(this.drawOutletFile, this._gv.outletFile)) {
                this.drawOutletFile = Utils.join(this._gv.shapesDir, "drawoutlets1.shp");
            }
            if (Project.Current.HasEdits) {
                await Project.Current.DiscardEditsAsync();
            }
            if (!await Delineation.createOutletFile(this.drawOutletFile, this._gv.demFile, false, this._gv)) {
                return;
            }
            this.drawOutletLayer = (await Utils.getLayerByFilename(this.drawOutletFile, FileTypes._OUTLETS, this._gv, null, Utils._WATERSHED_GROUP_NAME)).Item1 as FeatureLayer;
            if (this.drawOutletLayer is null) {
                Utils.error(string.Format("Unable to load shapefile {0}", this.drawOutletFile), this._gv.isBatch);
                return;
            }
            var outletForm = new OutletForm(this.drawOutletLayer, this);
            await outletForm.setup();
            outletForm.Show();
        }

        // return function from OutletForm
        public async Task addOutlets(bool ok) {
            if (ok) {
                if (this.drawCurrent) {
                    // add features to current outlet layer
                    await Utils.removeLayer(this.drawOutletFile);
                    // remove outlets layer to enable copying to it
                    await Utils.removeLayer(this._gv.outletFile);
                    //if (Project.Current.HasEdits) { await Project.Current.DiscardEditsAsync(); }
                    //var outletLayer = (await Utils.getLayerByFilenameOrLegend(this._gv.outletFile, FileTypes._OUTLETS, "", this._gv.isBatch)) as FeatureLayer;
                    //await QueuedTask.Run(() => {
                    //    var fc = outletLayer.GetFeatureClass();
                    //    var parms = Geoprocessing.MakeValueArray(this.drawOutletFile, fc);
                    //    Utils.runPython("runCopyFeatures.py", parms, this._gv);
                    //});
                    //var tempOutlets = Path.Combine(this._gv.shapesDir, "tempoutlets.shp");
                    //Utils.copyShapefile(this.drawOutletFile, "tempoutlets", this._gv.shapesDir);
                    //var parms = Geoprocessing.MakeValueArray(this.drawOutletFile, this._gv.outletFile);
                    //Utils.runPython("runCopyFeatures.py", parms, this._gv);
                    Utils.copyPointFeatures(this.drawOutletFile, this._gv.outletFile);
                    // restore outlets layer
                    var outletLayer = (await Utils.getLayerByFilename(this._gv.outletFile, FileTypes._OUTLETS, this._gv, null, Utils._WATERSHED_GROUP_NAME)).Item1 as FeatureLayer;
                    //await MapView.Active.RedrawAsync(true);
                } else {
                    // replaces current outlet layer
                    await Utils.removeLayer(this._gv.outletFile);
                    this._gv.outletFile = this.drawOutletFile;
                }
                this._dlg.delinSelectOutlets.Text = this._gv.outletFile;
            } else {
                // discard drawoutlets layer and file
                await Utils.removeLayerAndFiles(this.drawOutletFile);
            }
        }

        //// Disable choice of point to be added to show users they must resume adding,
        ////         unless changing to self.mapTool.
        //public virtual void mapToolChanged(object tool) {
        //        // @UndefinedVariable
        //        this._odlg.widget.setEnabled(tool == this.mapTool);
        //    }

        //// Reset canvas' mapTool.
        //public virtual void resumeDrawing() {
        //    this._odlg.widget.Enabled = true;
        //    this._gv.iface.setActiveLayer(this.drawOutletLayer);
        //    var canvas = this._gv.iface.mapCanvas();
        //    canvas.setMapTool(this.mapTool);
        //}

        //// noinspection PyCallByClass,PyCallByClass
        //// Add point to drawOutletLayer.
        //public virtual void getPoint(object point, object button) {
        //    // @UnusedVariable button
        //    var isInlet = this._odlg.inletButton.Checked || this._odlg.ptsourceButton.Checked;
        //    // can't use feature count as they can't be counted until adding is confirmed
        //    // so set to -1 and fix them later
        //    var pid = -1;
        //    var inlet = isInlet ? 1 : 0;
        //    // use 2 for pond, 1 for reservoir, 0 for neither
        //    var res = this._odlg.pondButton.Checked ? 2 : this._odlg.reservoirButton.Checked ? 1 : 0;
        //    var ptsource = this._odlg.ptsourceButton.Checked ? 1 : 0;
        //    var idIndex = this._gv.topo.getIndex(this.drawOutletLayer, Topology._ID);
        //    var inletIndex = this._gv.topo.getIndex(this.drawOutletLayer, Topology._INLET);
        //    var resIndex = this._gv.topo.getIndex(this.drawOutletLayer, Topology._RES);
        //    var ptsourceIndex = this._gv.topo.getIndex(this.drawOutletLayer, Topology._PTSOURCE);
        //    var feature = QgsFeature();
        //    Debug.Assert(this.drawOutletLayer is not null);
        //    var fields = this.drawOutletLayer.dataProvider().fields();
        //    feature.setFields(fields);
        //    feature.setAttribute(idIndex, pid);
        //    feature.setAttribute(inletIndex, inlet);
        //    feature.setAttribute(resIndex, res);
        //    feature.setAttribute(ptsourceIndex, ptsource);
        //    // noinspection PyCallByClass
        //    feature.setGeometry(QgsGeometry.fromPointXY(point));
        //    this.drawOutletLayer.addFeature(feature);
        //    this.drawOutletLayer.triggerRepaint();
        //    // clicking on map may have hidden the dialog, so make it top
        //    this._odlg.raise_();
        //}

        //// Give suitable point ids to drawn points.
        //public virtual void fixPointIds() {
        //    Debug.Assert(this.drawOutletLayer is not null);
        //    // need to commit first or appear to be no features
        //    this.drawOutletLayer.commitChanges();
        //    var idIndex = this._gv.topo.getIndex(this.drawOutletLayer, Topology._ID);
        //    var ptIdIndex = this._gv.topo.getIndex(this.drawOutletLayer, Topology._POINTID, ignoreMissing: true);
        //    if (ptIdIndex < 0) {
        //        ptIdIndex = Topology.makePositiveOutletIds(this.drawOutletLayer);
        //        if (ptIdIndex < 0) {
        //            Utils.error(string.Format("Failed to Add PointId field to inlets/outlets file {0}", Utils.layerFilename(this.drawOutletLayer)), this._gv.isBatch);
        //            return;
        //        }
        //    }
        //    // start editing again
        //    this.drawOutletLayer.startEditing();
        //    // find maximum existing feature id
        //    var maxId = 0;
        //    foreach (var feature in this.drawOutletLayer.getFeatures()) {
        //        maxId = Math.Max(maxId, feature[idIndex]);
        //    }
        //    // replace negative feature ids
        //    foreach (var feature in this.drawOutletLayer.getFeatures()) {
        //        var pid = feature[idIndex];
        //        if (pid < 0) {
        //            maxId += 1;
        //            this.drawOutletLayer.changeAttributeValue(feature.GetFID(), idIndex, maxId);
        //            this.drawOutletLayer.changeAttributeValue(feature.GetFID(), ptIdIndex, maxId);
        //        }
        //    }
        //    this.drawOutletLayer.commitChanges();
        //}

        // Allow user to select points in inlets/outlets layer.
        public async Task selectOutlets() {
            if (ArcSWATPlus.mainMap != MapView.Active.Map) {
                await ArcSWATPlus.mainMap.OpenViewAsync();
            }
            this.selFromLayer = null;
            List<ArcGIS.Desktop.Mapping.Layer> layers = (List<ArcGIS.Desktop.Mapping.Layer>)MapView.Active.GetSelectedLayers().ToList();
            ArcGIS.Desktop.Mapping.Layer layer = layers.Count == 1 ? layers[0] : null;
            if (layer is not null) {
                if (layer.Name.Contains("inlets/outlets")) {
                    //if layer.name().startswith(Utils._SELECTEDLEGEND):
                    //    Utils.error('You cannot select from a selected inlets/outlets layer', self._gv.isBatch)
                    //    return
                    this.selFromLayer = layer as FeatureLayer;
                }
            }
            if (this.selFromLayer is null) {
                this.selFromLayer = (await Utils.getLayerByFilenameOrLegend(this._gv.outletFile, FileTypes._OUTLETS, "", this._gv.isBatch)) as FeatureLayer;
                if (this.selFromLayer is null) {
                    Utils.error("Cannot find inlets/outlets layer.  Please choose the layer you want to select from in the layers panel.", this._gv.isBatch);
                    return;
                } else {
                    layers = new List<ArcGIS.Desktop.Mapping.Layer>() { this.selFromLayer };
                    MapView.Active.SelectLayers(layers);
                }
            }
            // make only the inlets/outlets layer selectable (else clicking selects all features)
            await QueuedTask.Run(() => {
                foreach (var layer in ArcSWATPlus.mainMap.GetLayersAsFlattenedList().ToList()) {
                    if (layer is FeatureLayer) {
                        ((FeatureLayer)layer).SetSelectable(false);
                    }
                }
                var cimFeatureDefinition = this.selFromLayer.GetDefinition() as ArcGIS.Core.CIM.CIMBasicFeatureLayer;
                // this does not work.  Following line just stops selection, and colour change is ignored
                //cimFeatureDefinition.UseSelectionSymbol = false;
                cimFeatureDefinition.SelectionColor = ColorFactory.Instance.CreateRGBColor(255, 255, 0);  // yellow
                this.selFromLayer.SetDefinition(cimFeatureDefinition);
                this.selFromLayer.SetSelectable(true);
            });
            var selectPoint = new SelectPoint(this);
            selectPoint.Show();
        }

        public async Task selectPoints(bool saveSelected) {
            if (ArcSWATPlus.mainMap != MapView.Active.Map) {
                await ArcSWATPlus.mainMap.OpenViewAsync();
            }
            if (!saveSelected) {
            await MapView.Active.ClearSketchAsync();
            await QueuedTask.Run(() => {
                this.selFromLayer.ClearSelection();
            });
            return;
            }
            IReadOnlyList<long> selected = null;
            await QueuedTask.Run(async () => {
                using (Selection selection = this.selFromLayer.GetSelection()) {
                    selected = selection.GetObjectIDs();
                    await MapView.Active.ClearSketchAsync();
                    this.selFromLayer.ClearSelection();
                    if (selected.Count() == 0) {
                        Utils.error("No points selected", this._gv.isBatch);
                        return;
                    }
                }
            });
            var filename = await Utils.layerFilename(this.selFromLayer);
            string selFile = filename;
            // make a copy of selected layer's file, unless already a selection, then remove non-selected features from it
            if (!filename.EndsWith("_sel.shp")) {
                var basename = System.IO.Path.GetFileNameWithoutExtension(filename) + "_sel";
                var dir = System.IO.Path.GetDirectoryName(filename);
                Utils.copyShapefile(filename, basename, dir);
                selFile = System.IO.Path.Combine(dir, basename + ".shp");
            }
            // remove features not selected from selFile
            using (var selDs = Ogr.Open(selFile, 1)) {
                // get layer - should only be one
                OSGeo.OGR.Layer layer = selDs.GetLayerByIndex(0);
                var fidsToRemove = new List<long>();
                layer.ResetReading();
                OSGeo.OGR.Feature f;
                do {
                    f = layer.GetNextFeature();
                    if (f != null) {
                        var fid = f.GetFID();
                        if (!selected.Contains(fid)) {
                            fidsToRemove.Add(fid);
                        }
                    }
                } while (f != null);
                fidsToRemove.Sort();
                // done in reverse as FIDs may change as features are removed
                for (int i = fidsToRemove.Count - 1; i >= 0; i--) {
                    layer.DeleteFeature(fidsToRemove[i]);
                }
                var numDeleted = fidsToRemove.Count;
                if (numDeleted > 0) {
                    Utils.loginfo(string.Format("{0} points removed", numDeleted));
                }
            }
            this._gv.outletFile = selFile;
            // make old outlet layer invisible
            await Utils.setLayerVisibility(selFromLayer, false);
            // remove any existing selected layer
            await Utils.removeLayerByLegend(Utils._SELECTEDLEGEND);
            // load new outletFile
            ArcGIS.Desktop.Mapping.Layer selOutletLayer;
            bool loaded;
            (selOutletLayer, loaded) = await Utils.getLayerByFilename(this._gv.outletFile, FileTypes._OUTLETS, this._gv, null, Utils._WATERSHED_GROUP_NAME);
            if (selOutletLayer is null || !loaded) {
                Utils.error(string.Format("Could not load selected inlets/outlets shapefile {0}", this._gv.outletFile), this._gv.isBatch);
                return;
            }
            this._dlg.delinSelectOutlets.Text = this._gv.outletFile;
            this.thresholdChanged = true;
            this._dlg.selectOutletsInteractiveLabelText = string.Format("{0} selected", selected.Count);
            this.snapFile = "";
            this._dlg.snappedLabelText = "";
        }

        // Allow user to select lakes shapefile.
        public async Task addLakesMap() {
            ArcGIS.Desktop.Mapping.Layer subLayer;
            // place above channels layer
            var channelsLayer = Utils.getLayerByLegend(Utils._CHANNELSLEGEND);
            if (channelsLayer is not null) {
                subLayer = channelsLayer;
            } else {
                var streamsLayer = Utils.getLayerByLegend(Utils._STREAMSLEGEND);
                if (streamsLayer is not null) {
                    subLayer = streamsLayer;
                } else {
                    subLayer = null;
                }
            }
            string lakeFile;
            ArcGIS.Desktop.Mapping.Layer lakesLayer;
            (lakeFile, lakesLayer) = await Utils.openAndLoadFile(FileTypes._LAKES, this._dlg.delinSelectLakes, this._gv.shapesDir, this._gv, subLayer, Utils._WATERSHED_GROUP_NAME, runFix: true);
            if (!string.IsNullOrEmpty(lakeFile) && lakesLayer is not null) {
                //lakesLayer.setLabelsEnabled(false);
                this._gv.lakeFile = lakeFile;
                this.lakesDone = false;
                this.lakePointsAdded = false;
                if (this._gv.useGridModel) {
                    this.gridLakesAdded = false;
                    using (var lakeDs = Ogr.Open(lakeFile, 1)) // write access as may need to add lake id
                    using (var gridDs = Ogr.Open(this._gv.subbasinsFile, 1)) {
                        var lakesOgrLayer = lakeDs.GetLayerByIndex(0);
                        var gridOgrLayer = gridDs.GetLayerByIndex(0);
                        this.progress("Making grid lakes ...");
                        await this.makeGridLakes(lakesOgrLayer, gridOgrLayer);
                        this.progress("");
                    }
                }
            }
        }

        // Allow user to remove cells from lake.
        public async Task removeLakeCells() {
            var ft = FileTypes._GRID;
            this.gridLayer = await Utils.getLayerByFilenameOrLegend(this._gv.subbasinsFile, ft, FileTypes.legend(ft), this._gv.isBatch) as FeatureLayer;
            if (this.gridLayer is null) {
                Utils.error("Cannot find the grid layer", this._gv.isBatch);
                return;
            }
            var lakesLayer = (await Utils.getLayerByFilename(this._gv.lakeFile, FileTypes._LAKES, null, null, null)).Item1 as FeatureLayer;
            if (lakesLayer is null) {
                Utils.error("Lakes layer not found.", this._gv.isBatch);
                return;
            }
            if (!this.gridLakesAdded) {
                if (!System.IO.File.Exists(this._gv.lakeFile)) {
                    Utils.error(string.Format("Cannot find lakes file {0}", this._gv.lakeFile), this._gv.isBatch);
                    return;
                }
                using (var lakeDs = Ogr.Open(this._gv.lakeFile, 0))
                using (var gridDs = Ogr.Open(this._gv.subbasinsFile, 1)) {
                    var lakesOgrLayer = lakeDs.GetLayerByIndex(0);
                    var gridOgrLayer = gridDs.GetLayerByIndex(0);
                    this.progress("Making grid lakes ...");
                    await this.makeGridLakes(lakesOgrLayer, gridOgrLayer);
                    this.progress("");
                }
            }
            //this._gv.iface.setActiveLayer(gridLayer);
            //this._gv.iface.actionSelect().trigger();
            // make only the lakes layer selectable (else clicking selects all features)
            if (ArcSWATPlus.mainMap != MapView.Active.Map) {
                await ArcSWATPlus.mainMap.OpenViewAsync();
            }
            await QueuedTask.Run(() => {
                foreach (var layer in ArcSWATPlus.mainMap.GetLayersAsFlattenedList().ToList()) {
                    if (layer is FeatureLayer) {
                        ((FeatureLayer)layer).SetSelectable(false);
                    }
                }
                var cimFeatureDefinition = lakesLayer.GetDefinition() as ArcGIS.Core.CIM.CIMBasicFeatureLayer;
                // this does not work.  Following line just stops selection, and colour change is ignored
                //cimFeatureDefinition.UseSelectionSymbol = false;
                cimFeatureDefinition.SelectionColor = ColorFactory.Instance.CreateRGBColor(255, 255, 0);  // yellow
                lakesLayer.SetDefinition(cimFeatureDefinition);
                lakesLayer.SetSelectable(true);
            });
            var selectLake = new SelectLake(this, false);
            selectLake.Show();
        }

        // Allow user to Add cells to lake.
        public async Task addLakeCells() {
            var ft = FileTypes._GRID;
            this.gridLayer = await Utils.getLayerByFilenameOrLegend(this._gv.subbasinsFile, ft, FileTypes.legend(ft), this._gv.isBatch) as FeatureLayer;
            if (this.gridLayer is null) {
                Utils.error("Cannot find the grid layer", this._gv.isBatch);
                return;
            }
            if (!System.IO.File.Exists(this._gv.lakeFile)) {
                Utils.error(string.Format("Cannot find lakes file {0}", this._gv.lakeFile), this._gv.isBatch);
                return;
            }
            var lakesLayer = (await Utils.getLayerByFilename(this._gv.lakeFile, FileTypes._LAKES, null, null, null)).Item1 as FeatureLayer;
            if (lakesLayer is null) {
                Utils.error("Lakes layer not found.", this._gv.isBatch);
                return;
            }
            if (!this.gridLakesAdded) {
                using (var lakeDs = Ogr.Open(this._gv.lakeFile, 0))
                using (var gridDs = Ogr.Open(this._gv.subbasinsFile, 1)) {
                    var lakesOgrLayer = lakeDs.GetLayerByIndex(0);
                    var gridOgrLayer = gridDs.GetLayerByIndex(0);
                    this.progress("Making grid lakes ...");
                    await this.makeGridLakes(lakesOgrLayer, gridOgrLayer);
                    this.progress("");
                }
            }
            int lakeId = -1;
            try {
                lakeId = Convert.ToInt32(this._dlg.delinLakeIdCombo.Text);
            } catch {
                Utils.information("Please select the number of the lake to which you wish to add cells", this._gv.isBatch);
                return;
            }
            // make only the lakes layer selectable (else clicking selects all features)
            if (ArcSWATPlus.mainMap != MapView.Active.Map) {
                await ArcSWATPlus.mainMap.OpenViewAsync();
            }
            await QueuedTask.Run(() => {
                foreach (var layer in ArcSWATPlus.mainMap.GetLayersAsFlattenedList().ToList()) {
                    if (layer is FeatureLayer) {
                        ((FeatureLayer)layer).SetSelectable(false);
                    }
                }
                var cimFeatureDefinition = lakesLayer.GetDefinition() as ArcGIS.Core.CIM.CIMBasicFeatureLayer;
                // this does not work.  Following line just stops selection, and colour change is ignored
                //cimFeatureDefinition.UseSelectionSymbol = false;
                cimFeatureDefinition.SelectionColor = ColorFactory.Instance.CreateRGBColor(255, 255, 0);  // yellow
                lakesLayer.SetDefinition(cimFeatureDefinition);
                lakesLayer.SetSelectable(true);
            });
            var selectLake = new SelectLake(this, false);
            selectLake.Show();

        }

        // if selecting, set lakeId of selected cells if adding, else nullify the lakeId of selected cells
        public async Task selectLakes(bool selecting, bool adding) {
            IReadOnlyList<long> fids = null;
            var lakeIdIndex = await this._gv.topo.getIndex(this.gridLayer, Topology._LAKEID);
            int lakeId;
            using (var gridDs = Ogr.Open(this._gv.subbasinsFile, 1)) {
                var gridOgrLayer = gridDs.GetLayerByIndex(0);
                if (selecting) {
                    lakeId = Convert.ToInt32(this._dlg.delinLakeIdCombo.Text);
                    fids = await QueuedTask.Run(() => {
                        return this.gridLayer.GetSelection().GetObjectIDs();
                    });
                    gridOgrLayer.ResetReading();
                    OSGeo.OGR.Feature cell;
                    do {
                        cell = gridOgrLayer.GetNextFeature();
                        if (cell is null) { break; }
                        if (fids.Contains(cell.GetFID())) {
                            if (adding) {
                                cell.SetField(lakeIdIndex, lakeId);
                            } else {
                                cell.SetField(lakeIdIndex, null);
                            }
                            gridOgrLayer.SetFeature(cell);
                        }
                    } while (cell is not null);
                }
                if (ArcSWATPlus.mainMap != MapView.Active.Map) {
                    await ArcSWATPlus.mainMap.OpenViewAsync();
                }
                await QueuedTask.Run(() => {
                    this.gridLayer.ClearSelection();
                    MapView.Active.Redraw(false);
                });
                var demLayer = (await Utils.getLayerByFilename(this._gv.demFile, FileTypes._DEM, null, null, null)).Item1 as RasterLayer;
                if (demLayer is null) {
                    Utils.error("DEM layer not found.", this._gv.isBatch);
                    return;
                }
                using (var streamsDs = Ogr.Open(this._gv.streamFile, 0)) {
                    var streamsOgrLayer = streamsDs.GetLayerByIndex(0);
                    await this._gv.topo.addGridReservoirsPondsAndWetlands(gridOgrLayer, streamsOgrLayer, demLayer, this._gv);
                }
            }
        }

        //=======not used========================================================================
        //     def addPointSources(self):
        //         """Create extra inlets/outlets shapefile 
        //         with point sources at source of every channel.
        //         """
        //         li = self._gv.iface.legendInterface()
        // #         ft = FileTypes._EXISTINGSUBBASINS if self._gv.existingWshed else \
        // #         FileTypes._GRID if self._gv.useGridModel else FileTypes._SUBBASINS
        // #         subbasinsLayer = Utils.getLayerByFilenameOrLegend(li.layers(), self._gv.subbasinsFile, ft, '', self._gv.isBatch)
        // #         if not subbasinsLayer:
        // #             Utils.error(u'Cannot find subbasins layer', self._gv.isBatch)
        // #             return
        // #         subbasinsLayer.removeSelection()
        //         channelLayer = Utils.getLayerByFilenameOrLegend(li.layers(), self._gv.channelFile, FileTypes._CHANNELS, '', self._gv.isBatch)
        //         if not channelLayer:
        //             Utils.error(u'Cannot find channels layer', self._gv.isBatch)
        //             return
        //         linkIndex = self._gv.topo.getIndex(channelLayer, Topology._LINKNO)
        //         basinNoIndex = self._gv.topo.getIndex(channelLayer, Topology._BASINNO)
        // #         reservoirIds = self.getOutletIds(Topology._RES)
        // #         ptsourceIds = self.getOutletIds(Topology._PTSOURCE)
        //         # Utils.information('Point source ids are {0}', ptsourceIds), self._gv.isBatch)
        // #         extraReservoirLinks = set()
        // #         for channel in channelLayer.getFeatures():
        // #             if channel[wsnoIndex] in self.extraReservoirBasins:  # TODO:
        // #                 if nodeidIndex >= 0:
        // #                     nodeid = channel[nodeidIndex]
        // #                     if nodeid in reservoirIds:
        // #                         continue  # already has a reservoir
        // #                 extraReservoirLinks.Add(channel[linkIndex])
        //         extraOutletFile = Utils.join(self._gv.shapesDir, 'extra.shp')
        //         Utils.tryRemoveLayerAndFiles(extraOutletFile, li)
        //         if not self.createOutletFile(extraOutletFile, self._gv.demFile, True):
        //             return
        //         self._dlg.setCursor(Qt.WaitCursor)
        //         extraOutletLayer = QgsVectorLayer(extraOutletFile, 'Extra inlets/outlets ({0})', extraOutletFile), 'ogr')
        //         idIndex = self._gv.topo.getIndex(extraOutletLayer, Topology._ID)
        //         inletIndex = self._gv.topo.getIndex(extraOutletLayer, Topology._INLET)
        //         resIndex = self._gv.topo.getIndex(extraOutletLayer, Topology._RES)
        //         ptsourceIndex = self._gv.topo.getIndex(extraOutletLayer, Topology._PTSOURCE)
        //         basinIndex = self._gv.topo.getIndex(extraOutletLayer, Topology._SUBBASIN)
        //         self._gv.writeProjectConfig(0,0)
        //         pid = 0
        //         for reach in channelLayer.getFeatures():
        //             link = reach[linkIndex]
        //             data = self._gv.topo.channelsData[link]
        //             point = QgsPoint(data.upperX, data.upperY)  # point source placed at source of channel
        //             basin = reach[basinNoIndex]
        //             feature = QgsFeature()
        //             fields = extraOutletLayer.dataProvider().fields()
        //             feature.setFields(fields)
        //             feature.setAttribute(idIndex, pid)
        //             pid += 1
        //             feature.setAttribute(inletIndex, 1)
        //             feature.setAttribute(resIndex, 0)
        //             feature.setAttribute(ptsourceIndex, 1)
        //             feature.setAttribute(basinIndex, basin)
        //             feature.setGeometry(QgsGeometry.fromPoint(point))
        //             extraOutletLayer.dataProvider().addFeatures([feature])
        //         if pid > 0:
        //             extraOutletLayer, loaded = Utils.getLayerByFilename(li.layers(), extraOutletFile, FileTypes._OUTLETS, self._gv, True)
        //             if not (extraOutletLayer and loaded):
        //                 Utils.error(u'Could not load extra outlets/inlets file {0}', extraOutletFile), self._gv.isBatch)
        //                 return
        //             li.moveLayer(extraOutletLayer, self._gv.watershedGroupIndex)
        //             self._gv.extraOutletFile = extraOutletFile
        //         else:
        //             # no extra reservoirs, inlets or point sources - clean up
        //             # first release resources, else .dbf file in use stops deletion of files
        //             extraOutletLayer = None
        //             Utils.tryRemoveLayerAndFiles(extraOutletFile, li)
        //             self._gv.extraOutletFile = ''
        //         self._dlg.setCursor(Qt.ArrowCursor)
        //===============================================================================
        //======= not used ====================================================================
        // def removeUpstreamCells(self, reach, startBasin, subbasinsLayer, streamLayer, wsnoIndex):
        //     """Remove grid cells and stream reaches upstream from basin."""
        //     basinIndex = self._gv.topo.getIndex(subbasinsLayer, Topology._POLYGONID)
        //     downIndex = self._gv.topo.getIndex(subbasinsLayer, Topology._DOWNID)
        //     provider = streamLayer.dataProvider()
        //     gridIdsToRemove = []
        //     upstreamBasins = set([startBasin])
        //     downBasin = 0
        //     count = 0
        //     numFeatures = subbasinsLayer.featureCount()
        //     time1 = DateTime.Now
        //     changing = True
        //     firstLoop = True
        //     request = QgsFeatureRequest().setFlags(QgsFeatureRequest.NoGeometry).setSubsetOfAttributes([basinIndex, downIndex])
        //     while changing:
        //         changing = False
        //         for f in subbasinsLayer.getFeatures(request):
        //             basin = f[basinIndex]
        //             if firstLoop and basin == startBasin:
        //                 firstLoop = False
        //                 gridIdsToRemove.Add(f.GetFID())
        //                 downBasin = f[downIndex]
        //                 # mark the downlink's basin as -1 to show it has no corresponding basin
        //                 mmap = Dictionary<>
        //                 mmap2 = Dictionary<>
        //                 mmap2[wsnoIndex] = -1
        //                 mmap[f.GetFID()] = mmap2
        //                 OK = provider.changeAttributeValues(mmap)
        //                 if not OK:
        //                     Utils.error(u'Cannot edit values in grid streams layer', self.isBatch)
        //             elif f[downIndex] in upstreamBasins and basin not in upstreamBasins:
        //                 upstreamBasins.Add(basin)
        //                 gridIdsToRemove.Add(f.GetFID())
        //                 count += 1
        //                 assert count < numFeatures, u'Loop in calculating cells upstream from inlet' 
        //                 changing = True
        //     if count > 0:
        //         upstreamBasins.discard(startBasin)
        //         streamIdsToRemove = []
        //         request = QgsFeatureRequest().setFlags(QgsFeatureRequest.NoGeometry).setSubsetOfAttributes([wsnoIndex])
        //         for reach in streamLayer.getFeatures(request):
        //             if reach[wsnoIndex] in upstreamBasins:
        //                 streamIdsToRemove.Add(reach.GetFID())
        //         provider.deleteFeatures(streamIdsToRemove)
        //         provider = subbasinsLayer.dataProvider()
        //         provider.deleteFeatures(gridIdsToRemove)
        //     time2 = DateTime.Now
        //     Utils.loginfo('Removing upstream grid cells took {0} seconds', int(time2 - time1)))
        //     return downBasin        
        //===========================================================================

        // Load snapped inlets/outlets points.
        public async Task snapReview() {
            var outletLayer = await Utils.getLayerByFilenameOrLegend(this._gv.outletFile, FileTypes._OUTLETS, "", this._gv.isBatch) as FeatureLayer;
            if (outletLayer is null) {
                Utils.error("Cannot find inlets/outlets layer", this._gv.isBatch);
                return;
            }
            if (this._gv.snapFile == "" || this.snapErrors) {
                var streamLayer = await Utils.getLayerByFilenameOrLegend(this._gv.streamFile, FileTypes._STREAMS, "", this._gv.isBatch) as FeatureLayer;
                if (streamLayer is null) {
                    Utils.error("Cannot find stream reaches layer", this._gv.isBatch);
                    return;
                }
                ArcGIS.Desktop.Mapping.FeatureLayer channelLayer;
                if (this._gv.useGridModel) {
                    channelLayer = streamLayer;
                } else {
                    channelLayer = await Utils.getLayerByFilenameOrLegend(this._gv.channelFile, FileTypes._CHANNELS, "", this._gv.isBatch) as FeatureLayer;
                    if (channelLayer is null) {
                        Utils.error("Cannot find channels layer", this._gv.isBatch);
                        return;
                    }
                }
                var outletBase = System.IO.Path.ChangeExtension(this._gv.outletFile, null);
                var snapFile = outletBase + "_snap.shp";
                if (! await this.createSnapOutletFile(outletLayer, streamLayer, channelLayer, this._gv.outletFile, snapFile)) {
                    return;
                }
            }
            // make old outlet layer invisible
            await Utils.setLayerVisibility(outletLayer, false);
            // load snapped layer
            var outletSnapLayer = (await Utils.getLayerByFilename(this.snapFile, FileTypes._OUTLETS, this._gv, null, Utils._WATERSHED_GROUP_NAME)).Item1;
            if (outletSnapLayer is null) {
                // don't worry about loaded flag as may already have the layer loaded
                Utils.error(string.Format("Could not load snapped inlets/outlets shapefile {0}", this._gv.snapFile), this._gv.isBatch);
            }
        }

        // Allow user to select subbasins to be merged.
        public async Task selectMergeSubbasins() {
            var ft = this._gv.existingWshed ? FileTypes._EXISTINGSUBBASINS : FileTypes._SUBBASINS;
            var subbasinsLayer = await Utils.getLayerByFilenameOrLegend(this._gv.subbasinsFile, ft, "", this._gv.isBatch) as FeatureLayer;
            if (subbasinsLayer is null) {
                Utils.error("Cannot find subbasins layer", this._gv.isBatch);
                return;
            }
            // make only subbasins layer selectable
            if (ArcSWATPlus.mainMap != MapView.Active.Map) {
                await ArcSWATPlus.mainMap.OpenViewAsync();
            }
            await QueuedTask.Run(() => {
                foreach (ArcGIS.Desktop.Mapping.Layer layer in ArcSWATPlus.mainMap.GetLayersAsFlattenedList().ToList()) {
                    if (layer is FeatureLayer) {
                        ((FeatureLayer)layer).SetSelectable(false);
                    }
                }
                var cimFeatureDefinition = subbasinsLayer.GetDefinition() as ArcGIS.Core.CIM.CIMBasicFeatureLayer;
                // this does not work.  Following line just stops selection, and colour change is ignored
                //cimFeatureDefinition.UseSelectionSymbol = false;
                cimFeatureDefinition.SelectionColor = ColorFactory.Instance.CreateRGBColor(255, 255, 0);  // yellow
                subbasinsLayer.SetDefinition(cimFeatureDefinition);
                subbasinsLayer.SetSelectable(true);
            });
            ArcGIS.Desktop.Framework.FrameworkApplication.CurrentTool = "ArcGIS.Desktop.Internal.Mapping.Ribbon.SelectTool";
            var selSubs = new SelectSubbasin(subbasinsLayer, this, this._gv);
            await selSubs.setup();
            selSubs.Show();
        }

        // Merged selected subbasin with its parent.  Not used with grid models.
        public async Task mergeSubbasinsOgr() {
            ReachData dataD;
            ReachData dataA;
            double dropD = 0.0;
            double dropA = 0.0;
            double lengthD = 0.0;
            double lengthA = 0.0;
            ArcGIS.Core.Data.Feature pointFeature;
            this.delineationFinishedOK = false;
            var demLayer = (await Utils.getLayerByFilenameOrLegend(this._gv.demFile, FileTypes._DEM, "", this._gv.isBatch)) as RasterLayer;
            if (demLayer is null) {
                Utils.error("Cannot find DEM layer", this._gv.isBatch);
                return;
            }
            var ft = this._gv.existingWshed ? FileTypes._EXISTINGSUBBASINS : FileTypes._SUBBASINS;
            var subbasinsArcLayer = (await Utils.getLayerByFilenameOrLegend(this._gv.subbasinsFile, ft, "", this._gv.isBatch)) as FeatureLayer;
            if (subbasinsArcLayer is null) {
                Utils.error("Cannot find subbasins layer", this._gv.isBatch);
                return;
            }
            // fids of the features will change as we delete them, so use polygonids, which we know will be unique
            // also because fids of Arc layer will not be the same as fids of OGR layer which we use for editing
            Selection selection = null;
            FeatureClass subbasinArcFeatures = null;
            var pids = new List<int>(); // will be selected polygonids
            var fids = new List<long>();
            await QueuedTask.Run(() => {
                selection = subbasinsArcLayer.GetSelection();
                if (selection.GetCount() == 0) {
                    Utils.information("Please select at least one subbasin to be merged", this._gv.isBatch);
                    return;
                }
                subbasinArcFeatures = subbasinsArcLayer.GetFeatureClass();
                foreach (var fid in selection.GetObjectIDs()) {
                    fids.Add(fid);
                    ArcGIS.Core.Data.QueryFilter qf = new ArcGIS.Core.Data.QueryFilter {
                        WhereClause = string.Format("FID = {0}", fid)
                    };
                    using (RowCursor rowCursor = subbasinArcFeatures.Search(qf, false)) {
                        rowCursor.MoveNext();
                        using (Row row = rowCursor.Current as ArcGIS.Core.Data.Feature) {
                            pids.Add(Convert.ToInt32(row[Topology._POLYGONID]));
                        }
                    }
                }
                subbasinsArcLayer.ClearSelection();
            });
            if (ArcSWATPlus.mainMap != MapView.Active.Map) {
                await ArcSWATPlus.mainMap.OpenViewAsync();
            }
            await MapView.Active.ClearSketchAsync();
            // remove the layers we will edit
            await Utils.removeLayerByLegend(FileTypes.legend(ft));
            await Utils.removeLayerByLegend(FileTypes.legend(FileTypes._STREAMS));
            using (var subbasinsDs = Ogr.Open(this._gv.subbasinsFile, 1))
            using (var streamDs = Ogr.Open(this._gv.streamFile, 1))
            using (var channelDs = Ogr.Open(this._gv.channelFile, 1)) {
                var subbasinsLayer = subbasinsDs.GetLayerByIndex(0);
                var streamLayer = streamDs.GetLayerByIndex(0);
                var channelLayer = channelDs.GetLayerByIndex(0);

                var outletLayer = (await Utils.getLayerByFilenameOrLegend(this._gv.outletFile, FileTypes._OUTLETS, "", this._gv.isBatch)) as FeatureLayer;
                var polygonidField = subbasinsLayer.FindFieldIndex(Topology._POLYGONID, 1);
                if (polygonidField < 0) {
                    return;
                }
                var areaField = subbasinsLayer.FindFieldIndex(Topology._AREA, 1);
                var streamlinkField = subbasinsLayer.FindFieldIndex(Topology._STREAMLINK, 1);
                var streamlenField = subbasinsLayer.FindFieldIndex(Topology._STREAMLEN, 1);
                var dsnodeidwField = subbasinsLayer.FindFieldIndex(Topology._DSNODEIDW, 1);
                var dswsidField = subbasinsLayer.FindFieldIndex(Topology._DSWSID, 1);
                var us1wsidField = subbasinsLayer.FindFieldIndex(Topology._US1WSID, 1);
                var us2wsidField = subbasinsLayer.FindFieldIndex(Topology._US2WSID, 1);
                var subbasinField = subbasinsLayer.FindFieldIndex(Topology._SUBBASIN, 1);
                var linknoField = streamLayer.FindFieldIndex(Topology._LINKNO, 1);
                if (linknoField < 0) {
                    return;
                }
                var dslinknoField = streamLayer.FindFieldIndex(Topology._DSLINKNO, 1);
                if (dslinknoField < 0) {
                    return;
                }
                var uslinkno1Field = streamLayer.FindFieldIndex(Topology._USLINKNO1, 1);
                var uslinkno2Field = streamLayer.FindFieldIndex(Topology._USLINKNO2, 1);
                var dsnodeidnField = streamLayer.FindFieldIndex(Topology._DSNODEID, 1);
                var orderField = streamLayer.FindFieldIndex(Topology._ORDER, 1);
                if (orderField < 0) {
                    orderField = streamLayer.FindFieldIndex(Topology._ORDER2, 1);
                }
                var lengthField = streamLayer.FindFieldIndex(Topology._LENGTH, 1);
                var magnitudeField = streamLayer.FindFieldIndex(Topology._MAGNITUDE, 1);
                var ds_cont_arField = streamLayer.FindFieldIndex(Topology._DRAINAREA, 1);
                if (ds_cont_arField < 0) {
                    ds_cont_arField = streamLayer.FindFieldIndex(Topology._DRAINAREA2, 1);
                }
                var dropField = streamLayer.FindFieldIndex(Topology._DROP, 1);
                if (dropField < 0) {
                    dropField = streamLayer.FindFieldIndex(Topology._DROP2, 1);
                }
                var slopeField = streamLayer.FindFieldIndex(Topology._SLOPE, 1);
                var straight_lField = streamLayer.FindFieldIndex(Topology._STRAIGHTL, 1);
                if (straight_lField < 0) {
                    straight_lField = streamLayer.FindFieldIndex(Topology._STRAIGHTL2, 1);
                }
                var us_cont_arField = streamLayer.FindFieldIndex(Topology._USCONTAR, 1);
                if (us_cont_arField < 0) {
                    us_cont_arField = streamLayer.FindFieldIndex(Topology._USCONTAR2, 1);
                }
                var wsnoField = streamLayer.FindFieldIndex(Topology._WSNO, 1);
                if (wsnoField < 0) {
                    return;
                }
                var dout_endField = streamLayer.FindFieldIndex(Topology._DOUTEND, 1);
                if (dout_endField < 0) {
                    dout_endField = streamLayer.FindFieldIndex(Topology._DOUTEND2, 1);
                }
                var dout_startField = streamLayer.FindFieldIndex(Topology._DOUTSTART, 1);
                if (dout_startField < 0) {
                    dout_startField = streamLayer.FindFieldIndex(Topology._DOUTSTART2, 1);
                }
                var dout_midField = streamLayer.FindFieldIndex(Topology._DOUTMID, 1);
                if (dout_midField < 0) {
                    dout_midField = streamLayer.FindFieldIndex(Topology._DOUTMID2, 1);
                }
                var basinField = channelLayer.FindFieldIndex(Topology._BASINNO, 1);
                int nodeidField = 0;
                int srcField = 0;
                int resField = 0;
                int inletField = 0;
                if (outletLayer is not null) {
                    nodeidField = await this._gv.topo.getIndex(outletLayer, Topology._ID, ignoreMissing: true);
                    srcField = await this._gv.topo.getIndex(outletLayer, Topology._PTSOURCE, ignoreMissing: true);
                    resField = await this._gv.topo.getIndex(outletLayer, Topology._RES, ignoreMissing: true);
                    inletField = await this._gv.topo.getIndex(outletLayer, Topology._INLET, ignoreMissing: true);
                }
                // collect map of deleted basin number to replacement for updating channelLayer
                var changedBasins = new Dictionary<int, int>();
                //var pids = new List<int>();
                //var fids = new List<long>();
                //await QueuedTask.Run(() => {
                //    foreach (var fid in selection.GetObjectIDs()) {
                //        fids.Add(fid);
                //        ArcGIS.Core.Data.QueryFilter qf = new ArcGIS.Core.Data.QueryFilter {
                //                WhereClause = string.Format("ObjectId = {0}", fid)
                //            };
                //        using (RowCursor rowCursor = subbasinArcFeatures.Search(qf, false)) {
                //            rowCursor.MoveNext();
                //            using (Row row = rowCursor.Current) {
                //                pids.Add(Convert.ToInt32(row[Topology._POLYGONID]));
                //            }
                //        }
                //    }
                //});
                // in the following
                // suffix A refers to the subbasin being merged
                // suffix UAs refers to the subbasin(s) upstream from A
                // suffix D refers to the subbasin downstream from A
                // suffix B refers to the other subbasin(s) upstream from D
                // suffix M refers to the merged basin
                this._gv.writeProjectConfig(0, 0);
                foreach (var polygonidA in pids) {
                    var subbasinA = Utils.getOgrFeatureByValue(subbasinsLayer, Topology._POLYGONID, polygonidA);
                    var reachA = Utils.getOgrFeatureByValue(streamLayer, Topology._WSNO, polygonidA);
                    if (reachA is null) {
                        Utils.error(string.Format("Cannot find reach with {0} value {1}", Topology._WSNO, polygonidA), this._gv.isBatch);
                        continue;
                    }
                    Utils.loginfo(string.Format("A is reach {0} polygon {1}", reachA.GetFieldAsInteger(linknoField), polygonidA));
                    var AHasOutlet = false;
                    var AHasInlet = false;
                    if (dsnodeidnField >= 0) {
                        var dsnodeidA = reachA.GetFieldAsInteger(dsnodeidnField);
                        if (outletLayer is not null) {
                            pointFeature = await Utils.getFeatureByValue(outletLayer, Topology._ID, dsnodeidA);
                            if (pointFeature is not null) {
                                if (inletField >= 0 && Convert.ToInt32(pointFeature[inletField]) == 1) {
                                    AHasInlet = true;
                                } else if (resField >= 0 && Convert.ToInt32(pointFeature[resField]) == 0) {
                                    AHasOutlet = true;
                                }
                            }
                        }
                    }
                    if (AHasOutlet || AHasInlet) {
                        Utils.information(string.Format("You cannot merge a subbasin which has an outlet or inlet.  Not merging subbasin with {0} value {1}", Topology._POLYGONID, polygonidA), this._gv.isBatch);
                        continue;
                    }
                    var linknoA = reachA.GetFieldAsInteger(linknoField);
                    streamLayer.ResetReading();
                    var reachUAs = new List<OSGeo.OGR.Feature>();
                    OSGeo.OGR.Feature reachUA = null;
                    do {
                        reachUA = streamLayer.GetNextFeature();
                        if (reachUA != null && reachUA.GetFieldAsInteger(Topology._DSLINKNO) == linknoA) {
                            reachUAs.Add(reachUA);
                        }
                    } while (reachUA != null);
                    // check whether a reach immediately upstream from A has an inlet
                    var inletUpFromA = false;
                    if (dsnodeidnField >= 0 && outletLayer is not null) {
                        foreach (var rUA in reachUAs) {
                            var dsnodeidUA = rUA.GetFieldAsInteger(dsnodeidnField);
                            pointFeature = await Utils.getFeatureByValue(outletLayer, Topology._ID, dsnodeidUA);
                            if (pointFeature is not null) {
                                if (inletField >= 0 && Convert.ToInt32(pointFeature[inletField]) == 1 && (srcField < 0 || Convert.ToInt32(pointFeature[srcField]) == 0)) {
                                    inletUpFromA = true;
                                    break;
                                }
                            }
                        }
                    }
                    var linknoD = reachA.GetFieldAsInteger(dslinknoField);
                    var reachD = Utils.getOgrFeatureByValue(streamLayer, Topology._LINKNO, linknoD);
                    if (reachD is null) {
                        Utils.information(string.Format("No downstream subbasin from subbasin with {0} value {1}: nothing to merge", Topology._POLYGONID, polygonidA), this._gv.isBatch);
                        continue;
                    }
                    var polygonidD = reachD.GetFieldAsInteger(wsnoField);
                    Utils.loginfo(string.Format("D is reach {0} polygon {1}", linknoD, polygonidD));
                    // reachD may be zero length, with no corresponding subbasin, so search downstream if necessary to find subbasinD
                    // at the same time collect zero-length reaches for later disposal
                    OSGeo.OGR.Feature subbasinD = null;
                    var nextReach = reachD;
                    var zeroReaches = new List<OSGeo.OGR.Feature>();
                    while (subbasinD is null) {
                        polygonidD = nextReach.GetFieldAsInteger(wsnoField);
                        subbasinD = Utils.getOgrFeatureByValue(subbasinsLayer, Topology._POLYGONID, polygonidD);
                        if (subbasinD is not null) {
                            break;
                        }
                        // nextReach has no subbasin (it is a zero length link); step downstream and try again
                        // first make a check
                        if (lengthField >= 0 && nextReach.GetFieldAsDouble(lengthField) > 0) {
                            Utils.error(string.Format("Internal error: stream reach wsno {0} has positive length but no subbasin.  Not merging subbasin with {1} value {2}", polygonidD, Topology._POLYGONID, polygonidA), this._gv.isBatch);
                            continue;
                        }
                        if (zeroReaches is not null) {
                            zeroReaches.Add(nextReach);
                        } else {
                            zeroReaches = new List<OSGeo.OGR.Feature> {
                                nextReach
                            };
                        }
                        var nextLink = nextReach.GetFieldAsInteger(dslinknoField);
                        if (nextLink < 0) {
                            // reached main outlet
                            break;
                        }
                        nextReach = Utils.getOgrFeatureByValue(streamLayer, Topology._LINKNO, nextLink);
                    }
                    if (subbasinD is null) {
                        Utils.information(string.Format("No downstream subbasin from subbasin with {0} value {1}: nothing to merge", Topology._POLYGONID, polygonidA), this._gv.isBatch);
                        continue;
                    }
                    reachD = nextReach;
                    linknoD = reachD.GetFieldAsInteger(linknoField);
                    var zeroLinks = (from reach in zeroReaches
                                     select reach.GetFieldAsInteger(linknoField)).ToList();
                    if (inletUpFromA) {
                        var DLinks = new List<int> {
                            linknoD
                        };
                        if (zeroLinks is not null) {
                            DLinks.AddRange(zeroLinks);
                        }
                        var reachBs = new List<OSGeo.OGR.Feature>();
                        streamLayer.ResetReading();
                        OSGeo.OGR.Feature reach = null;
                        do {
                            reach = streamLayer.GetNextFeature();
                            if (reach != null && DLinks.Contains(reach.GetFieldAsInteger(dslinknoField)) && reach.GetFID() != reachA.GetFID()) {
                                reachBs.Add(reach);
                            }
                        } while (reach != null);
                        if (reachBs.Count > 0) {
                            Utils.information(string.Format("Subbasin with {0} value {1} has an upstream inlet and the downstream one has another upstream subbasin: cannot merge.", Topology._POLYGONID, polygonidA), this._gv.isBatch);
                            continue;
                        }
                    }
                    // have reaches and watersheds A, UAs, D
                    // we are ready to start editing the streamLayer
                    try {
                        // create new merged stream M from D and A and add it to streams
                        var fidA = reachA.GetFID();
                        var fidD = reachD.GetFID();
                        var geomA = reachA.GetGeometryRef();
                        var geomD = reachD.GetGeometryRef();
                        var def = streamLayer.GetLayerDefn();
                        var reachM = new OSGeo.OGR.Feature(def);
                        reachM.SetField(linknoField, linknoD);
                        reachM.SetField(dslinknoField, reachD.GetFieldAsInteger(dslinknoField));
                        if (uslinkno1Field >= 0) {
                            var Dup1 = reachD.GetFieldAsInteger(uslinkno1Field);
                            if (Dup1 == linknoA || zeroLinks.Contains(Dup1)) {
                                // in general these cannot be relied on, since as we remove zero length links 
                                // there may be more than two upstream links from M
                                // At least don't leave it referring to a soon to be non-existent reach
                                Dup1 = reachA.GetFieldAsInteger(uslinkno1Field);
                            }
                            reachM.SetField(uslinkno1Field, Dup1);
                        }
                        if (uslinkno2Field >= 0) {
                            var Dup2 = reachD.GetFieldAsInteger(uslinkno2Field);
                            if (Dup2 == linknoA || zeroLinks.Contains(Dup2)) {
                                // in general these cannot be relied on, since as we remove zero length links 
                                // there may be more than two upstream links from M
                                // At least don't leave it referring to a soon to be non-existent reach
                                Dup2 = reachA.GetFieldAsInteger(uslinkno2Field);
                            }
                            reachM.SetField(uslinkno2Field, Dup2);
                        }
                        if (dsnodeidnField >= 0) {
                            reachM.SetField(dsnodeidnField, reachD.GetFieldAsInteger(dsnodeidnField));
                        }
                        if (orderField >= 0) {
                            reachM.SetField(orderField, reachD.GetFieldAsInteger(orderField));
                        }
                        if (lengthField >= 0) {
                            lengthA = reachA.GetFieldAsDouble(lengthField);
                            lengthD = reachD.GetFieldAsDouble(lengthField);
                            reachM.SetField(lengthField, lengthA + lengthD);
                        } else if (slopeField >= 0 || straight_lField >= 0 || dout_endField >= 0 && dout_midField >= 0) {
                            // we will need these lengths
                            lengthA = geomA.Length();
                            lengthD = geomD.Length();
                        }
                        if (magnitudeField >= 0) {
                            reachM.SetField(magnitudeField, reachD.GetFieldAsInteger(magnitudeField));
                        }
                        if (ds_cont_arField >= 0) {
                            reachM.SetField(ds_cont_arField, reachD.GetFieldAsDouble(ds_cont_arField));
                        }
                        if (dropField >= 0) {
                            dropA = reachA.GetFieldAsDouble(dropField);
                            dropD = reachD.GetFieldAsDouble(dropField);
                            reachM.SetField(dropField, dropA + dropD);
                        } else if (slopeField >= 0) {
                            dataA = await this._gv.topo.getOgrReachData(reachA, demLayer);
                            dropA = dataA.upperZ - dataA.lowerZ;
                            dataD = await this._gv.topo.getOgrReachData(reachD, demLayer);
                            dropD = dataD.upperZ - dataD.lowerZ;
                        }
                        if (slopeField >= 0) {
                            reachM.SetField(slopeField, (dropA + dropD) / (lengthA + lengthD));
                        }
                        if (straight_lField >= 0) {
                            dataA = await this._gv.topo.getOgrReachData(reachA, demLayer);
                            dataD = await this._gv.topo.getOgrReachData(reachD, demLayer);
                            var dx = dataA.upperX - dataD.lowerX;
                            var dy = dataA.upperY - dataD.lowerY;
                            reachM.SetField(straight_lField, Math.Sqrt(dx * dx + dy * dy));
                        }
                        if (us_cont_arField >= 0) {
                            reachM.SetField(us_cont_arField, reachA.GetFieldAsDouble(us_cont_arField));
                        }
                        reachM.SetField(wsnoField, polygonidD);
                        if (dout_endField >= 0) {
                            reachM.SetField(dout_endField, reachD.GetFieldAsDouble(dout_endField));
                        }
                        if (dout_startField >= 0) {
                            reachM.SetField(dout_startField, reachA.GetFieldAsDouble(dout_startField));
                        }
                        if (dout_endField >= 0 && dout_midField >= 0) {
                            reachM.SetField(dout_midField, reachD.GetFieldAsDouble(dout_endField) + (lengthA + lengthD) / 2.0);
                        }
                        // executing mergeReaches may change fids, so delete D and A first
                        streamLayer.DeleteFeature(Math.Max(fidA, fidD));
                        streamLayer.DeleteFeature(Math.Min(fidA, fidD));
                        reachM.SetGeometry(geomD.Union(geomA));
                        var result = streamLayer.CreateFeature(reachM);
                        if (result != OSGeo.OGR.Ogr.OGRERR_NONE) {
                            Utils.error("Failed to merge streams", this._gv.isBatch);
                        }
                        // now FIDs have changed through deletion of A and D, but LINKNOs are now unique
                        var fidsToRemove = new List<long>();
                        var linksToRemove = (from reach in zeroReaches
                                             select reach.GetFieldAsInteger(linknoField)).ToList();
                        if (linksToRemove.Count > 0) {
                            OSGeo.OGR.Feature r = null;
                            streamLayer.ResetReading();
                            do {
                                r = streamLayer.GetNextFeature();
                                if (r != null && linksToRemove.Contains(r.GetFieldAsInteger(linknoField))) {
                                    fidsToRemove.Add(r.GetFID());
                                }
                            } while (r != null);
                            fidsToRemove.Sort();
                            fidsToRemove.Reverse();
                            foreach (var fid in fidsToRemove) {
                                streamLayer.DeleteFeature(fid);
                            }
                        }
                        // change dslinks in UAs to D (= M)
                        var linksToChange = (from reach in reachUAs
                                             select reach.GetFieldAsInteger(linknoField)).ToList();
                        if (linksToChange.Count > 0) {
                            OSGeo.OGR.Feature r = null;
                            streamLayer.ResetReading();
                            do {
                                r = streamLayer.GetNextFeature();
                                if (r != null && linksToChange.Contains(r.GetFieldAsInteger(linknoField))) {
                                    Utils.loginfo(string.Format("Changing link number {0} to have link {1} downstream", r.GetFieldAsInteger(linknoField), linknoD));
                                    r.SetField(dslinknoField, linknoD);
                                    streamLayer.SetFeature(r);
                                }
                            } while (r != null);
                        }
                        // change any dslinks to zeroLinks to D as the zeroReaches have been deleted
                        if (zeroLinks.Count > 0) {
                            OSGeo.OGR.Feature r = null;
                            streamLayer.ResetReading();
                            do {
                                r = streamLayer.GetNextFeature();
                                if (r != null && zeroLinks.Contains(r.GetFieldAsInteger(dslinknoField))) {
                                    r.SetField(dslinknoField, linknoD);
                                    streamLayer.SetFeature(r);
                                }
                            } while (r != null);
                        }
                    } catch (Exception ex) {
                        Utils.exceptionError("Exception while updating stream reach shapefile: " + ex.Message, this._gv.isBatch);
                        return;
                    }
                    // New watershed shapefile will be inconsistent with watershed grid, so remove grid to be recreated later.
                    // Do not do it immediately because the user may remove several subbasins, so we wait until the 
                    // delineation form is closed.
                    // clear name as flag that it needs to be recreated
                    this._gv.basinFile = "";
                    try {
                        // create new merged subbasin M from D and A and Add it to subbasins
                        // prepare reachM
                        long fidA = subbasinA.GetFID();
                        long fidD = subbasinD.GetFID();
                        var def = subbasinsLayer.GetLayerDefn();
                        var subbasinM = new OSGeo.OGR.Feature(def);
                        subbasinM.SetField(polygonidField, polygonidD);
                        if (areaField >= 0) {
                            var areaA = subbasinA.GetFieldAsDouble(areaField);
                            var areaD = subbasinD.GetFieldAsDouble(areaField);
                            subbasinM.SetField(areaField, areaA + areaD);
                        }
                        if (streamlinkField >= 0) {
                            subbasinM.SetField(streamlinkField, subbasinD.GetFieldAsInteger(streamlinkField));
                        }
                        if (streamlenField >= 0) {
                            var lenA = subbasinA.GetFieldAsDouble(streamlenField);
                            var lenD = subbasinD.GetFieldAsDouble(streamlenField);
                            subbasinM.SetField(streamlenField, lenA + lenD);
                        }
                        if (dsnodeidwField >= 0) {
                            subbasinM.SetField(dsnodeidwField, subbasinD.GetFieldAsInteger(dsnodeidwField));
                        }
                        if (dswsidField >= 0) {
                            subbasinM.SetField(dswsidField, subbasinD.GetFieldAsInteger(dswsidField));
                        }
                        if (us1wsidField >= 0) {
                            if (subbasinD.GetFieldAsInteger(us1wsidField) == polygonidA) {
                                subbasinM.SetField(us1wsidField, subbasinA.GetFieldAsInteger(us1wsidField));
                            } else {
                                subbasinM.SetField(us1wsidField, subbasinD.GetFieldAsInteger(us1wsidField));
                            }
                        }
                        if (us2wsidField >= 0) {
                            if (subbasinD.GetFieldAsInteger(us2wsidField) == polygonidA) {
                                subbasinM.SetField(us2wsidField, subbasinA.GetFieldAsInteger(us2wsidField));
                            } else {
                                subbasinM.SetField(us2wsidField, subbasinD.GetFieldAsInteger(us2wsidField));
                            }
                        }
                        if (subbasinField >= 0) {
                            subbasinM.SetField(subbasinField, subbasinD.GetFieldAsInteger(subbasinField));
                        }
                        var geomA = subbasinA.GetGeometryRef();
                        var geomD = subbasinD.GetGeometryRef();
                        // executing merge may change oids, so delete D and A first
                        subbasinsLayer.DeleteFeature(Math.Max(fidA, fidD));
                        subbasinsLayer.DeleteFeature(Math.Min(fidA, fidD));
                        var geomM = geomD.Union(geomA);
                        if (!geomM.IsValid()) {
                            //Utils.information("Need to make subbasin geometry valid", this._gv.isBatch);
                            geomM = geomM.MakeValid(null);
                            if (!geomM.IsValid()) {
                                Utils.error("Failed to make subbasin geometry valid", this._gv.isBatch);
                                return;
                            }
                        }
                        subbasinM.SetGeometry(geomM);
                        var result = subbasinsLayer.CreateFeature(subbasinM);
                        if (result != OSGeo.OGR.Ogr.OGRERR_NONE) {
                            Utils.error("Failed to merge subbasins", this._gv.isBatch);
                        }
                        // we will replace A with D in channelLayer basinno field
                        changedBasins[polygonidA] = polygonidD;
                        // change downlinks to A into downlinks to D(=M)
                        if (dswsidField >= 0) {
                            OSGeo.OGR.Feature w = null;
                            subbasinsLayer.ResetReading();
                            do {
                                w = subbasinsLayer.GetNextFeature();
                                if (w != null && w.GetFieldAsInteger(dswsidField) == polygonidA) {
                                    w.SetField(dswsidField, polygonidD);
                                    subbasinsLayer.SetFeature(w);
                                }
                            } while (w != null);
                        }
                    }
                    catch (Exception ex) {
                        Utils.error(string.Format("Exception while updating watershed shapefile: {0}", ex.Message), this._gv.isBatch);
                        return;
                    }
                }
                // guard against B -> C, A -> B problems in changedBasins
                changedBasins = MapFuns<int>.flattenMap(changedBasins);
                // update BASINNO field in channels shapefile
                if (basinField >= 0) {
                    channelLayer.ResetReading();
                    OSGeo.OGR.Feature channel;
                    do {
                        channel = channelLayer.GetNextFeature();
                        if (channel is null) { break; }
                        int polygonidD;
                        if (changedBasins.TryGetValue(channel.GetFieldAsInteger(basinField), out polygonidD)) {
                            channel.SetField(basinField, polygonidD);
                            channelLayer.SetFeature(channel);
                        }
                    } while (channel is not null);
                }
                // update chBasinToSubbasin 
                // will change dictionary so use list
                foreach (var (chBasin, subbasin) in this._gv.topo.chBasinToSubbasin) {
                    int newSubbasin;
                    if (changedBasins.TryGetValue(subbasin, out newSubbasin)) {
                        this._gv.topo.chBasinToSubbasin[chBasin] = newSubbasin;
                    }
                }
                // remove subbasin outlets
                foreach (var polygonIdA in changedBasins.Keys) {
                    this._gv.topo.outlets.Remove(polygonIdA);
                }
                Utils.loginfo(string.Format("{0} outlets after merging subbasins", this._gv.topo.outlets.Count));
                streamDs.Dispose();
                subbasinsDs.Dispose();
                channelDs.Dispose();
            }
            // restore subbasin and stream layers
            var (subLayer, ok) = await Utils.getLayerByFilename(this._gv.subbasinsFile, ft, this._gv, null, Utils._WATERSHED_GROUP_NAME, false);
            if (!ok) {
                Utils.error(string.Format("Failed to reload subbasins file {0}", this._gv.subbasinsFile), this._gv.isBatch);
            }
            (var streamsLayer, ok) = await Utils.getLayerByFilename(this._gv.streamFile, FileTypes._STREAMS, this._gv, null, Utils._WATERSHED_GROUP_NAME, false);
            if (!ok) {
                Utils.error(string.Format("Failed to reload streams file {0}", this._gv.streamFile), this._gv.isBatch);
            }
        }

        //==========no longer used=================================================================
        // @staticmethod      
        // def reassignStrahler(streamLayer, reach, upLink, upOrder, linknoField, dslinknoField, orderField):
        //     """Reassign Strahler numbers downstream in the network starting from reach.
        //     Stop when the new Strahler number is already stored, or the root of the tree is reached.
        //     If a link draining to reach is the same as upLink, use upOrder as its order (since it is not 
        //     yet stored in streamLayer).
        //     """
        //     if reach is None:
        //         return
        //     link = reach[linknoField]
        //     ups = [up for up in streamLayer.getFeatures() if up[dslinknoField] == link]
        //     def orderOfReach(r): return upOrder if r[linknoField] == upLink else r[orderField]
        //     orders = [orderOfReach(up) for up in ups]
        //     s = Delineation.strahlerOrder(orders)
        //     if s != reach[orderField]:
        //         streamLayer.changeAttributeValue(reach.GetFID(), orderField, s)
        //         downReach = Utils.getFeatureByValue(streamLayer, linknoField, reach[dslinknoField])
        //         Delineation.reassignStrahler(streamLayer, downReach, link, s, linknoField, dslinknoField, orderField)
        //         
        // @staticmethod
        // def calculateStrahler(streamLayer, upLinks, linknoField, orderField):
        //     """Calculate Strahler order from upstream links upLinks."""
        //     orders = [Utils.getFeatureByValue(streamLayer, linknoField, upLink)[orderField] for upLink in upLinks]
        //     return Delineation.strahlerOrder(orders)
        //     
        // @staticmethod
        // def strahlerOrder(orders):
        //     """Calculate Strahler order from a list or orders."""
        //     if len(orders) == 0:
        //         return 1
        //     else:
        //         omax = Math.Max(orders)
        //         count = len([o for o in orders if o == omax])
        //         return omax if count == 1 else omax+1
        //===========================================================================
        // Set cursor to Arrow, clear progress label, clear message bar, 
        //         and change tab index if not negative.
        //         
        public void cleanUp(int tabIndex) {
            if (tabIndex >= 0) {
                this._dlg.delinTabWidget.SelectedIndex = tabIndex;
            }
            this.progress("");
            return;
        }

        // 
        //         Create watershed shapefile subbasinsFile from watershed grid wFile, 
        //         and load it if ft is FileTypes._SUBBASINS.
        //         
        //         This function is used to create the subbasins shapefile (one basin per stream)
        //         and also the watershed shapefile (one basin per channel).
        //         The latter is intended for existing watershed use, and for the landscape units file when floodplains are not used
        //         (so that landscape units coincide with stream basins), and is not normally loaded.
        //         
        public async Task createWatershedShapefile(string wFile, string subbasinsFile, int ft) {
            ArcGIS.Desktop.Mapping.Layer subLayer;
            FeatureLayer subbasinsLayer;
            if (Utils.isUpToDate(wFile, subbasinsFile)) {
                return;
            }
            await Utils.removeLayerAndFiles(subbasinsFile);
            var tempFile = System.IO.Path.ChangeExtension(subbasinsFile, "_temp.shp");
            // in case old temp files left
            Utils.removeFiles(tempFile);
            var parms = Geoprocessing.MakeValueArray(wFile, subbasinsFile);
            Utils.runPython("runRasterToPolygon.py", parms, this._gv);
            // wFile may not have a .prj (being a .tif) so use DEM's
            Utils.copyPrj(this._gv.demFile, subbasinsFile);
            // remove temp files
            Utils.removeFiles(tempFile);
            // use GDAL to rename gridcode to PolygonId and add Area and Subbasin
            using (var subbasinsDs = Ogr.Open(subbasinsFile, 1)) {
                if (subbasinsDs is null) {
                    Utils.error($"Cannot open subbasins file {subbasinsFile}", this._gv.isBatch);
                    return;
                }
                OSGeo.OGR.Driver drv = subbasinsDs.GetDriver();
                if (drv is null) {
                    Utils.error("Cannot get GDAL driver for subbasins file", this._gv.isBatch);
                    return;
                }
                // get layer - should only be one
                OSGeo.OGR.Layer layer = null;
                for (int iLayer = 0; iLayer < subbasinsDs.GetLayerCount(); iLayer++) {
                    layer = subbasinsDs.GetLayerByIndex(iLayer);
                    if (!(layer is null)) { break; }
                }
                if (layer == null) {
                    Utils.error(string.Format("No layers found in subbasins file {0}", subbasinsFile), this._gv.isBatch);
                    return;
                }
                FeatureDefn def = layer.GetLayerDefn();
                // add PolygonId as a copy of Id field rather than trying to change field name
                var gridcodeIndex = def.GetFieldIndex("gridcode");
                var polyFieldDef = new OSGeo.OGR.FieldDefn(Topology._POLYGONID, OSGeo.OGR.FieldType.OFTInteger);
                layer.AlterFieldDefn(gridcodeIndex, polyFieldDef, 1);
                var areaFieldDef = new OSGeo.OGR.FieldDefn(Topology._AREA, OSGeo.OGR.FieldType.OFTReal);
                layer.CreateField(areaFieldDef, 1);
                // for the subbasins layer we will later Add the SWAT basin number
                // and for the watershed layer we will later Add the SWAT channel number
                if (ft == FileTypes._SUBBASINS) {
                    var subFieldDef = new OSGeo.OGR.FieldDefn(Topology._SUBBASIN, OSGeo.OGR.FieldType.OFTInteger);
                    layer.CreateField(subFieldDef, 1);
                } else {
                    var chanFieldDef = new OSGeo.OGR.FieldDefn(Topology._CHANNEL, OSGeo.OGR.FieldType.OFTInteger);
                    layer.CreateField(chanFieldDef, 1);
                }
                // set area field and fill in centroids table
                if (ft == FileTypes._SUBBASINS) {
                    this._gv.topo.basinCentroids.Clear();
                }
                layer.ResetReading();
                OSGeo.OGR.Feature f;
                do {
                    f = layer.GetNextFeature();
                    if (f != null) {
                        OSGeo.OGR.Geometry geom = f.GetGeometryRef();
                        var area = geom.GetArea() * this._gv.horizontalFactor * this._gv.horizontalFactor / 10000.0;
                        f.SetField(Topology._AREA, area);
                        if (ft == FileTypes._SUBBASINS) {
                            int basin = f.GetFieldAsInteger(Topology._POLYGONID);
                            var centroid = geom.Centroid();
                            double[] argout = new double[3];
                            centroid.GetPoint(0, argout);
                            this._gv.topo.basinCentroids[basin] = argout;
                        }
                        layer.SetFeature(f);
                    }
                } while (f != null);
                // load it if ft is FileTypes._SUBBASINS
                if (ft == FileTypes._SUBBASINS) {
                    // load above DEM and below streams
                    // (or use Full HRUs layer if there is one)
                    var fullHRUsLayer = Utils.getLayerByLegend(Utils._FULLHRUSLEGEND);
                    if (fullHRUsLayer is not null) {
                        subLayer = fullHRUsLayer;
                    } else {
                        var demLayer = await Utils.getLayerByFilenameOrLegend(this._gv.demFile, FileTypes._DEM, "", this._gv.isBatch);
                        if (demLayer is not null) {
                            subLayer = demLayer;
                        } else {
                            subLayer = null;
                        }
                    }
                    subbasinsLayer = (await Utils.getLayerByFilename(subbasinsFile, FileTypes._SUBBASINS, this._gv, subLayer, Utils._WATERSHED_GROUP_NAME)).Item1 as FeatureLayer;
                    if (subbasinsLayer is null) {
                        Utils.error(string.Format("Failed to load subbasins shapefile {0}", subbasinsFile), this._gv.isBatch);
                        return;
                    }
                    // TODO
                    //this._gv.iface.setActiveLayer(subbasinsLayer);
                    // labels should be turned off, as may persist from previous run
                    // we turn back on when SWAT basin numbers are calculated and stored
                    // in the Subbasin field
                    await QueuedTask.Run(() => subbasinsLayer.SetLabelVisibility(false));
                }
            }
        }

        // Create basin file from subbasins or watershed basins shapefile.
        public async Task<string> createBasinFile(string basinsShapefile, RasterLayer demLayer, string nameCode) {
            var demPath = await Utils.layerFilename(demLayer);
            var wFile = System.IO.Path.ChangeExtension(demPath, null) + nameCode + ".tif";
            await Utils.removeLayerAndFiles(wFile);
            var parms = Geoprocessing.MakeValueArray(basinsShapefile, Topology._POLYGONID, wFile, this._gv.demFile);
            var environment = Geoprocessing.MakeEnvironmentArray(overwriteoutput: true);
            await Geoprocessing.ExecuteToolAsync("conversion.FeatureToRaster", parms, environment, null, null, GPExecuteToolFlags.None);
            Debug.Assert(System.IO.File.Exists(wFile));
            Utils.copyPrj(basinsShapefile, wFile);
            return wFile;
        }

        // Create raster of Playa lakes.  Can assume there is at least one playa.
        public async Task<string> createPlayaFile(string lakeFile, RasterLayer demLayer) {
            var demPath = await Utils.layerFilename(demLayer);
            var playaBase = System.IO.Path.ChangeExtension(demPath, null) + "playa";
            var playaFile = playaBase + ".tif";
            var playaLakesFile = playaBase + ".shp";
            await Utils.removeLayerAndFiles(playaFile);
            await Utils.removeLayerAndFiles(playaLakesFile);
            // make copy of lakes file but including only playa lakes
            using (var lakeDs = Ogr.Open(lakeFile, 0)) {
                var driver = lakeDs.GetDriver();
                var playaDs = driver.CreateDataSource(playaLakesFile, null);
                var lakeLayer = lakeDs.GetLayerByIndex(0);
                var defn = lakeLayer.GetLayerDefn();
                var wkt = this._gv.topo.crsProject.Wkt;
                OSGeo.OSR.SpatialReference srs = new OSGeo.OSR.SpatialReference(wkt);
                var playaLayer = playaDs.CreateLayer("Playas", srs, wkbGeometryType.wkbMultiPolygon, null);
                for (var i = 0; i < defn.GetFieldCount(); i++) {
                    playaLayer.CreateField(defn.GetFieldDefn(i), 1);
                }
                for (var i = 0; i < defn.GetGeomFieldCount(); i++) {
                    playaLayer.CreateGeomField(defn.GetGeomFieldDefn(i), 1);
                }
                lakeLayer.ResetReading();
                OSGeo.OGR.Feature lake;
                var where = string.Format("RES = {0}", (int)WaterRole._PLAYA);
                lakeLayer.SetAttributeFilter(where);
                do {
                    lake = lakeLayer.GetNextFeature();
                    if (lake is null) { break; }
                    playaLayer.SetFeature(lake);
                } while (lake is not null);
            }
            var parms = Geoprocessing.MakeValueArray(playaLakesFile, Topology._POLYGONID, playaFile, this._gv.demFile);
            var environment = Geoprocessing.MakeEnvironmentArray(overwriteoutput: true);
            await Geoprocessing.ExecuteToolAsync("conversion.FeatureToRaster", parms, environment, null, null, GPExecuteToolFlags.None);
            Debug.Assert(System.IO.File.Exists(playaFile));
            Utils.copyPrj(lakeFile, playaFile);
            return playaFile;
        }

        // Create grid shapefile for watershed.
        public async Task createGridShapefile(string pFile, string ad8File, string wFile) {
            this.progress("Creating grid ...");
            this._gv.gridSize = this._dlg.gridSizeValue;
            var time2 = DateTime.Now;
            var (storeGrid, accTransform, minDrainArea, maxDrainArea) = await this.storeGridData(ad8File, wFile, this._gv.gridSize);
            var time3 = DateTime.Now;
            Utils.loginfo(string.Format("Storing grid data took {0} seconds", time3.Subtract(time2).TotalSeconds));
            if (storeGrid is not null) {
                Debug.Assert(accTransform is not null);
                if (this.addDownstreamData(storeGrid, pFile, this._gv.gridSize, accTransform)) {
                    var time4 = DateTime.Now;
                    Utils.loginfo(string.Format("Adding downstream data took {0} seconds", time4.Subtract(time3).TotalSeconds));
                    await this.writeGridShapefile(storeGrid, pFile, this._gv.gridSize, accTransform);
                    var time5 = DateTime.Now;
                    Utils.loginfo(string.Format("Writing grid shapefile took {0} seconds", time5.Subtract(time4).TotalSeconds));
                    var numOutlets = await this.writeGridStreamsShapefile(storeGrid, pFile, minDrainArea, maxDrainArea, accTransform);
                    var time6 = DateTime.Now;
                    Utils.loginfo(string.Format("Writing grid streams shapefile took {0} seconds", time6.Subtract(time5).TotalSeconds));
                    if (numOutlets >= 0) {
                        var msg = string.Format("Grid processing done with subbasin threshold {0} sq.km: {1} outlets", this._dlg.areaStText, numOutlets);
                        Utils.loginfo(msg);
                        //self._gv.iface.messageBar().pushMessage(msg, level=Qgis.Info, duration=10)
                        //if self._gv.isBatch:
                        //    print(msg)
                    }
                }
            }
            return;
        }

        // Create grid data in array and return it.
        public async Task<(Dictionary<int, Dictionary<int, GridData>>, double[], double, double)> storeGridData(string ad8File, string basinFile, int gridSize) {
            int lost;
            // if there are inlets, first make an ad8 (accumulation) file for area upstream from inlets
            var (inAsOutFile, hasInlet) = this.hasInlets();
            if (hasInlet) {
                Debug.Assert(inAsOutFile is not null);
                var ad8File1 = await this.upstreamAccumulationFile(ad8File, inAsOutFile);
                if (ad8File1 is null) {
                    return (null, null, 0, 0);
                }
                ad8File = ad8File1;
            }
            // mask ad8File with basinFile to exclude small outflowing watersheds
            var @base = System.IO.Path.ChangeExtension(ad8File, null);
            var accFile = @base + "clip.tif";
            Utils.removeFiles(accFile);
            var parms = Geoprocessing.MakeValueArray(basinFile, ad8File, accFile, "VALUE IS NULL");
            var result = await Geoprocessing.ExecuteToolAsync("sa.SetNull", parms);
            if (string.IsNullOrEmpty(result.ReturnValue)) {
                Geoprocessing.ShowMessageBox(result.Messages, "GP Messages", result.IsFailed ? GPMessageBoxStyle.Error : GPMessageBoxStyle.Default);
                Utils.error("Failed to create downstream accumulation file: " + $@"{result.ErrorMessages}", this._gv.isBatch);
                return (null, null, 0, 0);
            } else {
                Utils.loginfo("Created downstream accumulation file: " + accFile + $@" {result.ReturnValue}");
            }
            Debug.Assert(System.IO.File.Exists(accFile));
            await Utils.removeLayer(accFile);
            Utils.copyPrj(ad8File, accFile);
            var accDs = OSGeo.GDAL.Gdal.Open(accFile, OSGeo.GDAL.Access.GA_ReadOnly);
            if (accDs is null) {
                Utils.error(string.Format("Cannot open accumulation file {0}", accFile), this._gv.isBatch);
                return (null, null, 0, 0);
            }
            // for now read whole accumulation file into memory
            var accBand = accDs.GetRasterBand(1);
            var accTransform = new double[6];
            accDs.GetGeoTransform(accTransform);
            var accArray = new double[accBand.XSize * accBand.YSize];
            accBand.ReadRaster(0, 0, accBand.XSize, accBand.YSize, accArray, accBand.XSize, accBand.YSize, 0, 0);
            var unitArea = Math.Abs(accTransform[1] * accTransform[5]) * this._gv.horizontalFactor * this._gv.horizontalFactor / 1000000.0;
            Utils.loginfo(string.Format("Grid cell area {0} sq km", unitArea * this._gv.gridSize * this._gv.gridSize));
            // create polygons and Add to gridFile
            var polyId = 0;
            // grid cells will be gridSize x gridSize squares
            var numGridRows = accBand.YSize / gridSize + 1;
            var numGridCols = accBand.XSize / gridSize + 1;
            var storeGrid = new Dictionary<int, Dictionary<int, GridData>>();
            var maxDrainArea = 0.0;
            var minDrainArea = double.PositiveInfinity;
            foreach (var gridRow in Enumerable.Range(0, numGridRows)) {
                var startAccRow = gridRow * gridSize;
                foreach (var gridCol in Enumerable.Range(0, numGridCols)) {
                    var startAccCol = gridCol * gridSize;
                    var maxAcc = 0.0;
                    var maxRow = -1;
                    var maxCol = -1;
                    var valCount = 0;
                    foreach (var row in Enumerable.Range(0, gridSize)) {
                        var accRow = startAccRow + row;
                        foreach (var col in Enumerable.Range(0, gridSize)) {
                            var accCol = startAccCol + col;
                            if (accRow < accBand.YSize && accCol < accBand.XSize) {
                                var accVal = accArray[accRow * accBand.XSize + accCol];
                                if (accVal > 0) {
                                    valCount += 1;
                                    // can get points with same (rounded) accumulation when values are high.
                                    // prefer one on edge if possible
                                    if (accVal > maxAcc || accVal == maxAcc && Delineation.onEdge(row, col, gridSize)) {
                                        maxAcc = accVal;
                                        maxRow = accRow;
                                        maxCol = accCol;
                                    }
                                }
                            }
                        }
                    }
                    if (valCount == 0) {
                        // no data for this grid
                        continue;
                    }
                    polyId += 1;
                    //if polyId <= 5:
                    //    x, y = Topology.cellToProj(maxCol, maxRow, accTransform)
                    //    maxAccPoint = QgsPointXY(x, y)
                    //    Utils.loginfo('Grid ({0},{1}) id {6} Math.Max {4} at ({2},{3}) which is {5}', gridRow, gridCol, maxCol, maxRow, maxAcc, maxAccPoint.toString(), polyId))
                    var drainArea = maxAcc * unitArea;
                    if (drainArea < minDrainArea) {
                        minDrainArea = drainArea;
                    }
                    if (drainArea > maxDrainArea) {
                        maxDrainArea = drainArea;
                    }
                    var data = new GridData(polyId, valCount, drainArea, maxAcc, maxRow, maxCol);
                    if (!storeGrid.ContainsKey(gridRow)) {
                        storeGrid[gridRow] = new Dictionary<int, GridData>();
                    }
                    storeGrid[gridRow][gridCol] = data;
                }
            }
            // Add any outlet and inlet ids
            this._gv.topo.lostDsNodeIds = new HashSet<int>();
            foreach (var (ioId, (point, pt)) in this.inletOutletPoints) {
                var gridCol = Convert.ToInt32((pt[0] - accTransform[0]) / (accTransform[1] * this._gv.gridSize));
                var gridRow = Convert.ToInt32((pt[1] - accTransform[3]) / (accTransform[5] * this._gv.gridSize));
                // try to find an at most 'one off' grid cell: 
                // for inlets can't guarantee central one is not just outside the grid
                var found = false;
                foreach (var i in new List<int> {
                    0,
                    1,
                    -1
                }) {
                    foreach (var j in new List<int> {
                        0,
                        1,
                        -1
                    }) {
                        var row = gridRow + i;
                        Dictionary<int, GridData> cols;
                        if (storeGrid.TryGetValue(row, out cols)) {
                            var col = gridCol + j;
                            GridData storeData;
                            if (cols.TryGetValue(col, out storeData)) { 
                                if (storeData.dsNodeId >= 0) {
                                    // cell already has a point added, and there can only be one 
                                    // as only one stream link per cell
                                    // TODO: should this be an error?
                                    // prefer inlets as these shape the watershed
                                    if (point == Points._INLET) {
                                        lost = storeData.dsNodeId;
                                        storeData.dsNodeId = ioId;
                                        Utils.loginfo(string.Format("Could not retain point with id {0} in grid cell {1}:  {2} with id {3} placed there.", lost, storeData.num, Points.toString(point), storeData.dsNodeId));
                                    } else {
                                        lost = ioId;
                                        Utils.loginfo(string.Format("Could not place {0} with id {1} in grid cell {2}: point {3} already placed there.", Points.toString(point), lost, storeData.num, storeData.dsNodeId));
                                    }
                                    this._gv.topo.lostDsNodeIds.Add(lost);
                                } else {
                                    storeData.dsNodeId = ioId;
                                    Utils.loginfo(string.Format("Placed {0} with id {1} at ({2}, {3}) in grid cell {4}.", Points.toString(point), storeData.dsNodeId, (int)pt[0], (int)pt[1], storeData.num));
                                }
                                found = true;
                                break;
                            }
                        }
                    }
                    if (found) {
                        break;
                    }
                }
                if (!found) {
                    Utils.error(string.Format("Cannot place {0} with id {1} at {2} in grid row {3} column {4}", Points.toString(point), ioId, pt, gridRow, gridCol), this._gv.isBatch);
                }
            }
            accDs = null;
            accArray = null;
            return (storeGrid, accTransform, minDrainArea, maxDrainArea);
        }

        // Reduce inlets/outlets file to inlets and outlets only to delineate subbasins. Return reduced file.
        public async Task<string> reduceToInletsOutlets() {
            if (!this._dlg.useOutletsChecked) {
                return null;
            }
            if (this._gv.snapFile == "" || !System.IO.File.Exists(this._gv.snapFile)) {
                Utils.error(string.Format("Cannot find snapped inlets/outlets file {0}", this._gv.snapFile), this._gv.isBatch);
                return null;
            }
            // make copy of snapped inlets/outlets file
            var @base = System.IO.Path.ChangeExtension(this._gv.snapFile, null);
            var dir = System.IO.Path.GetDirectoryName(this._gv.snapFile);
            var ioBase = @base + "io";
            Utils.copyShapefile(this._gv.snapFile, ioBase, dir);
            var iOutlets = ioBase + ".shp";
            // remove layer if any
            await Utils.removeLayer(iOutlets);
            using (var ioDs = Ogr.Open(iOutlets, 1)) {
                var ioLayer = ioDs.GetLayerByIndex(0);
                var inletIndex = ioLayer.FindFieldIndex(Topology._INLET, 1);
                var resIndex = ioLayer.FindFieldIndex(Topology._RES, 1);
                var ptsourceIndex = ioLayer.FindFieldIndex(Topology._PTSOURCE, 1);
                if (inletIndex < 0 || resIndex < 0 || ptsourceIndex < 0) {
                    return null;
                }
                var others = new List<long>();
                ioLayer.ResetReading();
                OSGeo.OGR.Feature f;
                do {
                    f = ioLayer.GetNextFeature();
                    if (f is null) { break; }
                    if (f.GetFieldAsInteger(inletIndex) == 0) {
                        if (f.GetFieldAsInteger(resIndex) > 0) {
                            ListFuns.insertIntoSortedLongList(f.GetFID(), others, true);
                        }
                    } else if (f.GetFieldAsInteger(ptsourceIndex) > 0) {
                        ListFuns.insertIntoSortedLongList(f.GetFID(), others, true);
                    }
                } while (f is not null);
                var count = others.Count;
                if (count > 0) {
                    for (int i = count - 1; i >= 0; i--) {
                        ioLayer.DeleteFeature(others[i]);
                    }
                    return iOutlets;
                } else {
                    Utils.removeFiles(iOutlets);
                    return this._gv.snapFile;
                }
            }
        }

        // 
        //         If there is an inlets/outlets file with one or more inlets, returns a file with only those points
        //         and true, else returns None and false.
        //         
        //         Also stores ID, kind and locations of inlets and outlets.
        public virtual (string, bool) hasInlets() {
            this.inletOutletPoints.Clear();
            if (!this._dlg.useOutletsChecked) {
                return (null, false);
            }
            if (this._gv.snapFile == "" || !System.IO.File.Exists(this._gv.snapFile)) {
                Utils.error(string.Format("Cannot find snapped inlets/outlets file {0}", this._gv.snapFile), this._gv.isBatch);
                return (null, false);
            }
            // first check if there are any inlets
            using (var snapDs = Ogr.Open(this._gv.snapFile, 0)) {
                var snapLayer = snapDs.GetLayerByIndex(0);
                var exp = string.Format("{0} = 1 AND {1} = 0", Topology._INLET, Topology._PTSOURCE);
                snapLayer.SetAttributeFilter(exp);
                snapLayer.ResetReading();
                var f = snapLayer.GetNextFeature();
                if (f is null) { return (null, false); }
            }
            // make copy of inlets/outlets file
            Utils.copyShapefile(this._gv.snapFile, "snapInlets", this._gv.shapesDir);
            var inletsFile = Utils.join(this._gv.shapesDir, "snapInlets.shp");
            using (var snapDs = Ogr.Open(inletsFile, 1)) {
                var inletsLayer = snapDs.GetLayerByIndex(0);
                var idIndex = inletsLayer.FindFieldIndex(Topology._ID, 1);
                var inletIndex = inletsLayer.FindFieldIndex(Topology._INLET, 1);
                var resIndex = inletsLayer.FindFieldIndex(Topology._RES, 1);
                var ptsourceIndex = inletsLayer.FindFieldIndex(Topology._PTSOURCE, 1);
                if (idIndex < 0 || inletIndex < 0 || resIndex < 0 || ptsourceIndex < 0) {
                    return (null, false);
                }
                // Remove from copy all but inlet points.
                // We could change them to outlets, but TauDEM does not take account of differences 
                // between kinds of point.
                // Also store inlets/outlets points for adding their IDs to the grid streams file.
                var result = false;
                var others = new List<long>();
                inletsLayer.ResetReading();
                OSGeo.OGR.Feature f;
                int point;
                do {
                    f = inletsLayer.GetNextFeature();
                    if (f is null) { break; }
                    if (f.GetFieldAsInteger(inletIndex) == 0) {
                        if (f.GetFieldAsInteger(resIndex) == 0) {
                            point = Points._OUTLET;
                        } else if (f.GetFieldAsInteger(resIndex) == 1) {
                            point = Points._RESERVOIR;
                        } else {
                            point = Points._POND;
                        }
                        ListFuns.insertIntoSortedLongList(f.GetFID(), others, true);
                    } else if (f.GetFieldAsInteger(ptsourceIndex) == 0) {
                        point = Points._INLET;
                        result = true;
                    } else {
                        point = Points._POINTSOURCE;
                        ListFuns.insertIntoSortedLongList(f.GetFID(), others, true);
                    }
                    this.inletOutletPoints[f.GetFieldAsInteger(idIndex)] = (point, Topology.geometryToPointAsDouble(f.GetGeometryRef()));
                } while (f is not null);
                if (result) {
#pragma warning disable CS0162 // Unreachable code detected
                    for (int i = others.Count - 1; i >= 0; i--) {
                        inletsLayer.DeleteFeature(others[i]);
                        return (inletsFile, true);
                    }
#pragma warning restore CS0162 // Unreachable code detected
                } else {
                    Utils.removeFiles(inletsFile);
                }
            }
            return (null, false);
        }

        // Return a new accumulation file with values of 0 for points upstream from outlets.
        public async Task<string> upstreamAccumulationFile(string accFile, string outletFile) {
            var @base = System.IO.Path.ChangeExtension(accFile, null);
            var upAccFile = @base + "Up.tif";
            await Utils.removeLayerAndFiles(upAccFile);
            var OK = await this._gv.taudem.runAreaD8(this._gv.pFile, upAccFile, outletFile, null, this._dlg.numberOfProcesses, this._dlg.getTaudemOutput, contCheck: false, mustRun: true);
            if (!OK) {
                Utils.error("Failed to calculate accumulation upstream from inlets", this._gv.isBatch);
                return null;
            }
            // use ArcGIS set null tool to remove the upstream area from the ad8 file by making it null.
            // we use ad8Up as the condition raster
            // ad8 as the false raster
            // and "not null" as the condition, since this is true for the upstream locations covered by ad8up
            var downAccFile = @base + "Down.tif";
            var parms = Geoprocessing.MakeValueArray(upAccFile, accFile, downAccFile, "VALUE IS NOT NULL");
            var result = await Geoprocessing.ExecuteToolAsync("sa.SetNull", parms);
            if (string.IsNullOrEmpty(result.ReturnValue)) {
                Utils.error("Failed to create downstream accumulation file: " + $@"{result.ErrorMessages}", this._gv.isBatch);
                return null;
            } else {
                Utils.loginfo("Created downstream accumulation file: " + downAccFile + $@" {result.ReturnValue}");
            }
            Debug.Assert(System.IO.File.Exists(downAccFile));
            Utils.copyPrj(accFile, downAccFile);
            return downAccFile;
        }

            // Returns true of (row, col) is on the edge of the cell.
            public static bool onEdge(int row, int col, int gridSize) {
                return row == 0 || row == gridSize - 1 || col == 0 || col == gridSize - 1;
            }

            // Use flow direction flowFile to see to which grid cell a D8 step takes you from the max accumulation point and store in array.
            public virtual bool addDownstreamData(Dictionary<int, Dictionary<int, GridData>> storeGrid, string flowFile, int gridSize, double[] accTransform) {
                int accToPCol;
                int accToPRow;
                var pRaster = OSGeo.GDAL.Gdal.Open(flowFile, OSGeo.GDAL.Access.GA_ReadOnly);
                if (pRaster is null) {
                    Utils.error(string.Format("Cannot open flow direction file {0}", flowFile), this._gv.isBatch);
                    return false;
                }
                // for now read whole D8 flow direction file into memory
                var pBand = pRaster.GetRasterBand(1);
            var pTransform = new double[6];
                pRaster.GetGeoTransform(pTransform);
                if (pTransform[1] != accTransform[1] || pTransform[5] != accTransform[5]) {
                    // problem with comparing floating point numbers
                    // actually OK if the vertical/horizontal difference times the number of rows/columns
                    // is less than half the depth/width of a cell
                    if (Math.Abs(pTransform[1] - accTransform[1]) * pBand.XSize > pTransform[1] * 0.5 || Math.Abs(pTransform[5] - accTransform[5]) * pBand.YSize > Math.Abs(pTransform[5]) * 0.5) {
                        Utils.error("Flow direction and accumulation files must have same cell size", this._gv.isBatch);
                        pRaster = null;
                        return false;
                    }
                }
            var pArray = new byte[pBand.XSize * pBand.YSize];
            pBand.ReadRaster(0, 0, pBand.XSize, pBand.YSize, pArray, pBand.XSize, pBand.YSize, 0, 0);
                // we know the cell sizes are sufficiently close;
                // accept the origins as the same if they are within a tenth of the cell size
                var sameCoords = pTransform == accTransform || Math.Abs(pTransform[0] - accTransform[0]) < pTransform[1] * 0.1 && Math.Abs(pTransform[3] - accTransform[3]) < Math.Abs(pTransform[5]) * 0.1;
                foreach (var (gridRow, gridCols) in storeGrid) {
                    foreach (var (gridCol, gridData) in gridCols) {
                        if (gridData.dsNodeId >= 0) {
                            // check if it is an outlet
                            var point = this.inletOutletPoints[gridData.dsNodeId].Item1;
                            if (point == Points._OUTLET) {
                                // nothing to do for this grid cell
                                continue;
                            }
                        }
                        // since we have same cell sizes, can simplify conversion from accumulation row, col to direction row, col
                        if (sameCoords) {
                            accToPRow = 0;
                            accToPCol = 0;
                        } else {
                            accToPCol = Convert.ToInt32((accTransform[0] - pTransform[0]) / accTransform[1]);
                            accToPRow = Convert.ToInt32((accTransform[3] - pTransform[3]) / accTransform[5]);
                            //pRow = Topology.yToRow(Topology.rowToY(gridData.maxRow, accTransform), pTransform)
                            //pCol = Topology.xToCol(Topology.colToX(gridData.maxCol, accTransform), pTransform)
                        }
                        var currentPRow = gridData.maxRow + accToPRow;
                        var currentPCol = gridData.maxCol + accToPCol;
                        // try to find downstream grid cell.  If we fail downstram number left as -1, which means outlet
                        // rounding of large accumulation values means that the maximum accumulation point found
                        // may not be at the outflow point, so we need to move until we find a new grid cell, or hit a map edge
                        var maxSteps = 2 * gridSize;
                        var found = false;
                    int direction;
                    int currentAccRow = 0;
                    int currentAccCol = 0;
                    int currentGridRow = 0;
                    int currentGridCol = 0;
                    while (!found) {
                            if (0 <= currentPRow && currentPRow < pBand.YSize && (0 <= currentPCol && currentPCol < pBand.XSize)) {
                                direction = (int)pArray[currentPRow * pBand.XSize + currentPCol];
                            } else {
                                break;
                            }
                            // apply a step in direction
                            if (1 <= direction && direction <= 8) {
                                currentPRow = currentPRow + Utils._dY[direction - 1];
                                currentPCol = currentPCol + Utils._dX[direction - 1];
                            } else {
                                break;
                            }
                            currentAccRow = currentPRow - accToPRow;
                            currentAccCol = currentPCol - accToPCol;
                            currentGridRow = currentAccRow / gridSize;
                            currentGridCol = currentAccCol / gridSize;
                            found = currentGridRow != gridRow || currentGridCol != gridCol;
                            if (!found) {
                                maxSteps -= 1;
                                if (maxSteps <= 0) {
                                    var (x0, y0) = Topology.cellToProj(gridData.maxCol, gridData.maxRow, accTransform);
                                    var (x, y) = Topology.cellToProj(currentAccCol, currentAccRow, accTransform);
                                    Utils.error(string.Format("Loop in flow directions in grid id {4} starting from ({0},{1}) and so far reaching ({2},{3})", Convert.ToInt32(x0), Convert.ToInt32(y0), Convert.ToInt32(x), Convert.ToInt32(y), gridData.num), this._gv.isBatch);
                                    break;
                                }
                            }
                        }
                        if (found) {
                        var cols = new Dictionary<int, GridData>();
                        if (storeGrid.TryGetValue(currentGridRow, out cols)) {
                            GridData currentData;
                            if (cols.TryGetValue(currentGridCol, out currentData)) {
                                    if (currentData.maxAcc < gridData.maxAcc) {
                                        Utils.loginfo(string.Format("WARNING: while calculating stream drainage, target grid cell {0} has lower maximum accumulation {1} than source grid cell {2}'s accumulation {3}", currentData.num, currentData.maxAcc, gridData.num, gridData.maxAcc));
                                    }
                                    gridData.downNum = currentData.num;
                                    gridData.downRow = currentGridRow;
                                    gridData.downCol = currentGridCol;
                                    //if gridData.num <= 5:
                                    //    Utils.loginfo('Grid ({0},{1}) drains to acc ({2},{3}) in grid ({4},{5})', gridRow, gridCol, currentAccCol, currentAccRow, currentGridRow, currentGridCol))
                                    //    Utils.loginfo('{0} at {1},{2} given down id {3}', gridData.num, gridRow, gridCol, gridData.downNum))
                                    if (gridData.downNum == gridData.num) {
                                        var (x, y) = Topology.cellToProj(gridData.maxCol, gridData.maxRow, accTransform);
                                        Utils.loginfo(string.Format("Grid ({0},{1}) id {6} at ({2},{3}) which is ({4}, {5}) draining to ({7},{8})", gridCol, gridRow, gridData.maxCol, gridData.maxRow, x, y, gridData.num, currentAccCol, currentAccRow));
                                        gridData.downNum = -1;
                                    }
                                    //assert gridData.downNum != gridData.num
                                    storeGrid[gridRow][gridCol] = gridData;
                                }
                            }
                        }
                    }
                }
                pRaster = null;
                pArray = null;
                return true;
            }

        // noinspection PyCallByClass
        // Write grid data to grid shapefile.  Also writes centroids dictionary.
        public virtual async Task writeGridShapefile(Dictionary<int, Dictionary<int, GridData>> storeGrid, string flowFile, int gridSize, double[] accTransform) {
            OSGeo.OGR.Layer gridLayer;
            this.progress("Writing grid ...");
            var gridFile = Utils.join(this._gv.shapesDir, "grid.shp");
            await Utils.removeLayerAndFiles(gridFile);
            using (var driver = Ogr.GetDriverByName("ESRI Shapefile")) {
                DataSource ds = driver.CreateDataSource(gridFile, null);
                var srsWkt = this._gv.topo.crsProject.Wkt;
                OSGeo.OSR.SpatialReference srs = new OSGeo.OSR.SpatialReference(srsWkt);
                gridLayer = ds.CreateLayer("Grid", srs, wkbGeometryType.wkbMultiPolygon, null);
                gridLayer.CreateField(new FieldDefn(Topology._POLYGONID, OSGeo.OGR.FieldType.OFTInteger), 1);
                gridLayer.CreateField(new FieldDefn(Topology._DOWNID, OSGeo.OGR.FieldType.OFTInteger), 1);
                gridLayer.CreateField(new FieldDefn(Parameters._AREA, OSGeo.OGR.FieldType.OFTReal), 1);
                gridLayer.CreateField(new FieldDefn(Topology._SUBBASIN, OSGeo.OGR.FieldType.OFTInteger), 1);
                gridLayer.CreateField(new FieldDefn(Topology._LAKEID, OSGeo.OGR.FieldType.OFTInteger), 1);
                Utils.copyPrj(this._gv.demFile, gridFile);
                var idIndex = gridLayer.FindFieldIndex(Topology._POLYGONID, 1);
                var downIndex = gridLayer.FindFieldIndex(Topology._DOWNID, 1);
                var areaIndex = gridLayer.FindFieldIndex(Parameters._AREA, 1);
                var defn = gridLayer.GetLayerDefn();
                var ul_x = accTransform[0];
                var x_size = accTransform[1];
                var ul_y = accTransform[3];
                var y_size = accTransform[5];
                var xDiff = x_size * gridSize * 0.5;
                var yDiff = y_size * gridSize * 0.5;
                this._gv.topo.basinCentroids = new Dictionary<int, double[]>();
                foreach (var (gridRow, gridCols) in storeGrid) {
                    // grids can be big so we'll Add one row at a time
                    var centreY = (gridRow + 0.5) * this._gv.gridSize * y_size + ul_y;
                    foreach (var (gridCol, gridData) in gridCols) {
                        var centreX = (gridCol + 0.5) * this._gv.gridSize * x_size + ul_x;
                        // this is strictly not the centroid for incomplete grid squares on the edges,
                        // but will make little difference.  
                        // Needs to be centre of grid for correct identification of landuse, soil and slope rows
                        // when creating HRUs.
                        this._gv.topo.basinCentroids[gridData.num] = new double[3] { centreX, centreY, 0 };
                        var x1 = centreX - xDiff; // 0
                        var x2 = centreX + xDiff; // 2
                        var y1 = centreY - yDiff; // 1
                        var y2 = centreY + yDiff; // 3
                        //var ring = new List<double[]> {
                        //new double[3] { x1, y1, 0},
                        //new double[3] { x2, y1, 0},
                        //new double[3] { x2, y2, 0},
                        //new double[3] { x1, y2, 0},
                        //new double[3] { x1, y1, 0}
                        var wkt = string.Format("POLYGON (({0} {1}, {2} {1}, {2} {3}, {0} {3}, {0} {1}))", x1, y1, x2, y2);
                        var polygon = OSGeo.OGR.Geometry.CreateFromWkt(wkt);
                        var feature = new OSGeo.OGR.Feature(defn);
                        feature.SetField(idIndex, gridData.num);
                        feature.SetField(downIndex, gridData.downNum);
                        feature.SetField(areaIndex, Math.Abs(gridData.area * x_size * y_size * this._gv.horizontalFactor * this._gv.horizontalFactor) / 10000.0);
                        feature.SetGeometry(polygon);
                        var err = gridLayer.CreateFeature(feature);
                        if (err != Ogr.OGRERR_NONE) {
                            Utils.error(string.Format("Unable to add feature to gridstreams shapefile {0}", gridFile), this._gv.isBatch);
                        }
                        feature = null;
                    }
                }
                ds.Dispose();
                ds = null;
            }
            // load grid shapefile above subbasins layer
            var subbasinsLayer = Utils.getLayerByLegend(Utils._SUBBASINSLEGEND);
            if (subbasinsLayer is not null) {
                await QueuedTask.Run(() => subbasinsLayer.SetVisibility(false));
            }
            var gridArcLayer = (await Utils.getLayerByFilename(gridFile, FileTypes._GRID, this._gv, subbasinsLayer, Utils._WATERSHED_GROUP_NAME)).Item1 as FeatureLayer;
            if (gridArcLayer is null) {
                Utils.error(string.Format("Failed to load grid shapefile {0}", gridFile), this._gv.isBatch);
                return;
            }
            //gridLayer.setLabelsEnabled(false);
            // don't expand legend for grid layer
            await QueuedTask.Run(() => gridArcLayer.SetExpanded(false));
            //gridLayer.triggerRepaint();
            // preserve access to subbasinsFile so can use to clip when making valley depths later
            // the gridFile can be reduced if there are inlets, and its convex hull will not be appropriate
            // for flow-based analysis as we will run off the edge
            this.clipperFile = this._gv.subbasinsFile;
            this._gv.subbasinsFile = gridFile;
            // make sure gridFile is used for making aquifers shapefile
            this._gv.subsNoLakesFile = "";
        }

        // Write grid data to grid streams shapefile.
        public async Task<int> writeGridStreamsShapefile(
            Dictionary<int, Dictionary<int, GridData>> storeGrid,
            string flowFile,
            double minDrainArea,
            double maxDrainArea,
            double[] accTransform) {
            Func<double, double> areaToPenWidth;
            OSGeo.OGR.Layer gridStreamsLayer;
            this.progress("Writing grid streams ...");
            var gridStreamsFile = Utils.join(this._gv.shapesDir, "gridstreams.shp");
            var gridFile = Utils.join(this._gv.shapesDir, "grid.shp");
            await Utils.removeLayerAndFiles(gridStreamsFile);
            using (var driver = Ogr.GetDriverByName("ESRI Shapefile")) {
                DataSource ds = driver.CreateDataSource(gridStreamsFile, null);
                var srsWkt = this._gv.topo.crsProject.Wkt;
                OSGeo.OSR.SpatialReference srs = new OSGeo.OSR.SpatialReference(srsWkt);
                gridStreamsLayer = ds.CreateLayer("GridStreams", srs, wkbGeometryType.wkbLineString, null);
                gridStreamsLayer.CreateField(new FieldDefn(Topology._LINKNO, OSGeo.OGR.FieldType.OFTInteger), 1);
                gridStreamsLayer.CreateField(new FieldDefn(Topology._DSLINKNO, OSGeo.OGR.FieldType.OFTInteger), 1);
                gridStreamsLayer.CreateField(new FieldDefn(Topology._DSNODEID, OSGeo.OGR.FieldType.OFTInteger), 1);
                gridStreamsLayer.CreateField(new FieldDefn(Topology._WSNO, OSGeo.OGR.FieldType.OFTInteger), 1);
                gridStreamsLayer.CreateField(new FieldDefn(Topology._DRAINAGE, OSGeo.OGR.FieldType.OFTReal), 1);
                gridStreamsLayer.CreateField(new FieldDefn(Topology._PENWIDTH, OSGeo.OGR.FieldType.OFTReal), 1);
                Utils.copyPrj(flowFile, gridStreamsFile);
                var defn = gridStreamsLayer.GetLayerDefn();
                var linkIndex = gridStreamsLayer.FindFieldIndex(Topology._LINKNO, 1);
                var downIndex = gridStreamsLayer.FindFieldIndex(Topology._DSLINKNO, 1);
                var dsNodeIndex = gridStreamsLayer.FindFieldIndex(Topology._DSNODEID, 1);
                var wsnoIndex = gridStreamsLayer.FindFieldIndex(Topology._WSNO, 1);
                var drainIndex = gridStreamsLayer.FindFieldIndex(Topology._DRAINAGE, 1);
                var penIndex = gridStreamsLayer.FindFieldIndex(Topology._PENWIDTH, 1);
                if (maxDrainArea > minDrainArea) {
                    // guard against division by zero
                    var rng = maxDrainArea - minDrainArea;
                    var mult = Parameters._MAXPENWIDTH - 0.2;
                    areaToPenWidth = x => (x - minDrainArea) * mult / rng + 0.2;
                } else {
                    areaToPenWidth = _ => 1.0;
                }
                var numOutlets = 0;
                double sourceX, sourceY, targetX, targetY;
                foreach (var gridCols in storeGrid.Values) {
                    // grids can be big so we'll Add one row at a time
                    foreach (var gridData in gridCols.Values) {
                        var downNum = gridData.downNum;
                        (sourceX, sourceY) = Topology.cellToProj(gridData.maxCol, gridData.maxRow, accTransform);
                        if (downNum > 0) {
                            var downData = storeGrid[gridData.downRow][gridData.downCol];
                            (targetX, targetY) = Topology.cellToProj(downData.maxCol, downData.maxRow, accTransform);
                        } else {
                            targetX = sourceX;
                            targetY = sourceY;
                            numOutlets += 1;
                        }
                        // respect default 'start at outlet' of TauDEM
                        var wkt = string.Format("LINESTRING ({0} {1}, {2} {3})", targetX, targetY, sourceX, sourceY);
                        var line = OSGeo.OGR.Geometry.CreateFromWkt(wkt);
                        var feature = new OSGeo.OGR.Feature(defn);
                        feature.SetField(linkIndex, gridData.num);
                        feature.SetField(downIndex, downNum);
                        feature.SetField(dsNodeIndex, gridData.dsNodeId);
                        feature.SetField(wsnoIndex, gridData.num);
                        // area needs coercion to float or will not write
                        feature.SetField(drainIndex, gridData.drainArea);
                        // set pen width to value in range 0 .. 2
                        feature.SetField(penIndex, areaToPenWidth(gridData.drainArea));
                        feature.SetGeometry(line);
                        var err = gridStreamsLayer.CreateFeature(feature);
                        if (err != Ogr.OGRERR_NONE) {
                            Utils.error(string.Format("Unable to add feature to gridstreams shapefile {0}", gridStreamsFile), this._gv.isBatch);
                        }
                        feature = null;
                    }
                }
                ds.Dispose();
                ds = null;
                // load grid streams shapefile
                // try to load above grid layer
                var gridLayer = Utils.getLayerByLegend(Utils._GRIDLEGEND);
                var gridArcStreamsLayer = (await Utils.getLayerByFilename(gridStreamsFile, FileTypes._GRIDSTREAMS, this._gv, gridLayer, Utils._WATERSHED_GROUP_NAME)).Item1 as FeatureLayer;
                if (gridArcStreamsLayer is null) {
                    Utils.error(string.Format("Failed to load grid streams shapefile {0}", gridStreamsFile), this._gv.isBatch);
                    return -1;
                }
                // make stream width dependent on drainage values (drainage is accumulation, ie number of dem cells draining to start of stream)
                await FileTypes.colourStreams(gridArcStreamsLayer, Topology._PENWIDTH, Topology._DRAINAGE);
                this._gv.streamFile = gridStreamsFile;
                this.streamDrainage = true;
                // remove visibility from streams layer, if any
                var streamsLayer = Utils.getLayerByLegend(Utils._STREAMSLEGEND);
                if (streamsLayer is not null) {
                    await QueuedTask.Run(() => {
                        streamsLayer.SetVisibility(false);
                    });
                }
                // remove visibility from channels layer, if any
                var channelsLayer = Utils.getLayerByLegend(Utils._CHANNELSLEGEND);
                if (channelsLayer is not null) {
                    await QueuedTask.Run(() => {
                        channelsLayer.SetVisibility(false);
                    });
                }
                this.progress("");
                return numOutlets;
            }
        }

        // Write streams shapefile with drainage from subbasinsLayer if grid drainage, else
        //         from drainage defined in csv file.
        public async Task<string> writeDrainageStreamsShapefile(string subbasinsFile, FeatureLayer subbasinsLayer, FeatureLayer outletLayer) {
            Dictionary<int, (int, double[], (double, double), (double, double))> centroids;
            OSGeo.OGR.Layer drainStreamsLayer;
            int point;
            int inletIndex;
            int resIndex;
            int ptsrcIndex;
            int idIndex;
            var outlets = new List<MapPoint>();
            this.inletOutletPoints = new Dictionary<int, (int, double[])>();
            if (outletLayer is not null) {
                inletIndex = await this._gv.topo.getIndex(outletLayer, Topology._INLET, ignoreMissing: true);
                resIndex = await this._gv.topo.getIndex(outletLayer, Topology._RES, ignoreMissing: true);
                ptsrcIndex = await this._gv.topo.getIndex(outletLayer, Topology._PTSOURCE, ignoreMissing: true);
                idIndex = await this._gv.topo.getIndex(outletLayer, Topology._ID, ignoreMissing: true);
                await QueuedTask.Run(() => {
                    using (var rc = outletLayer.Search()) {
                        while (rc.MoveNext()) {
                            var feature = rc.Current as ArcGIS.Core.Data.Feature;
                            if (inletIndex >= 0 && (int)feature[inletIndex] == 1) {
                                if (ptsrcIndex >= 0 && (int)feature[ptsrcIndex] == 1) {
                                    point = Points._POINTSOURCE;
                                } else {
                                    point = Points._INLET;
                                }
                            } else if (resIndex >= 0) {
                                if ((int)feature[resIndex] == 1) {
                                    point = Points._RESERVOIR;
                                } else {
                                    point = Points._POND;
                                }
                            } else {
                                point = Points._OUTLET;
                                outlets.Add(feature.GetShape() as MapPoint);
                            }
                            if (idIndex >= 0) {
                                var mp = feature.GetShape() as MapPoint;
                                this.inletOutletPoints[(int)feature[idIndex]] = (point, new double[3] { mp.X, mp.Y, 0 });
                            }
                        }
                    }
                });
            }
            if (!this.gridDrainage) {
                this.drainageTable = this._dlg.delinSelectStreams.Text;
                if (!System.IO.File.Exists(this.drainageTable)) {
                    Utils.error("Please select a drainage csv file", this._gv.isBatch);
                    return null;
                }
            }
            this.progress("Writing drainage grid streams ...");
            var time1 = DateTime.Now;
            var drainStreamsFile = Utils.join(this._gv.shapesDir, "drainstreams.shp");
            var ft = FileTypes._DRAINSTREAMS;
            var legend = FileTypes.legend(ft);
            await Utils.removeLayerAndFiles(drainStreamsFile);
            var driver = Ogr.GetDriverByName("ESRI Shapefile");
            using (DataSource ds = driver.CreateDataSource(drainStreamsFile, null)) {
                var srsWkt = this._gv.topo.crsProject.Wkt;
                OSGeo.OSR.SpatialReference srs = new OSGeo.OSR.SpatialReference(srsWkt);
                drainStreamsLayer = ds.CreateLayer("Grid streams", srs, wkbGeometryType.wkbLineString, null);
                drainStreamsLayer.CreateField(new FieldDefn(Topology._LINKNO, OSGeo.OGR.FieldType.OFTInteger), 1);
                drainStreamsLayer.CreateField(new FieldDefn(Topology._DSLINKNO, OSGeo.OGR.FieldType.OFTInteger), 1);
                drainStreamsLayer.CreateField(new FieldDefn(Topology._DSNODEID, OSGeo.OGR.FieldType.OFTInteger), 1);
                drainStreamsLayer.CreateField(new FieldDefn(Topology._WSNO, OSGeo.OGR.FieldType.OFTInteger), 1);
                drainStreamsLayer.CreateField(new FieldDefn(Topology._DRAINAGE, OSGeo.OGR.FieldType.OFTReal), 1);
                drainStreamsLayer.CreateField(new FieldDefn(Topology._PENWIDTH, OSGeo.OGR.FieldType.OFTReal), 1);
                Utils.copyPrj(subbasinsFile, drainStreamsFile);
                var defn = drainStreamsLayer.GetLayerDefn();
                var linkIndex = drainStreamsLayer.FindFieldIndex(Topology._LINKNO, 1);
                var dslinkIndex = drainStreamsLayer.FindFieldIndex(Topology._DSLINKNO, 1);
                var dsnodeIndex = drainStreamsLayer.FindFieldIndex(Topology._DSNODEID, 1);
                var wsnoIndex = drainStreamsLayer.FindFieldIndex(Topology._WSNO, 1);
                var basinIndex = await this._gv.topo.getIndex(subbasinsLayer, Topology._POLYGONID);
                if (basinIndex < 0) {
                    return null;
                }
                var downBasinIndex = await this._gv.topo.getIndex(subbasinsLayer, Topology._DOWNID);
                if (downBasinIndex < 0) {
                    return null;
                }
                this._gv.topo.basinCentroids.Clear();
                var toRemove = new List<long>();
                var areaFactor = this._gv.horizontalFactor * this._gv.horizontalFactor;
                var featureCount = 0;
                if (this.gridDrainage) {
                    centroids = new Dictionary<int, (int, double[], (double, double), (double, double))>();
                    await QueuedTask.Run(() => {
                        using (var rc = subbasinsLayer.Search()) {
                            while (rc.MoveNext()) {
                                var cell = rc.Current as ArcGIS.Core.Data.Feature;

                                var basin = (int)cell[basinIndex];
                                var area = Convert.ToInt32((cell.GetShape() as ArcGIS.Core.Geometry.Polygon).Area * areaFactor);
                                if (area == 0) {
                                    // grid cells collected by clipping with aquifers can be degenerate ones at edge with close to zero area:
                                    // remove them
                                    ListFuns.insertIntoSortedLongList(cell.GetObjectID(), toRemove, true);
                                    Utils.loginfo(string.Format("Zero area grid cell {0} removed", basin));
                                    continue;
                                }
                                var downBasin = (int)cell[downBasinIndex];
                                var (centroid, (xmin, xmax), (ymin, ymax)) = Utils.centreGridCell(cell);
                                centroids[basin] = (downBasin, centroid, (xmin, xmax), (ymin, ymax));
                                this._gv.topo.basinCentroids[basin] = centroid;
                            }
                            int count = toRemove.Count;
                            if (count > 0) {
                                var fc = subbasinsLayer.GetFeatureClass();
                                for (int i = count - 1; i >= 0; i--) {
                                    ArcGIS.Core.Data.QueryFilter qf = new ArcGIS.Core.Data.QueryFilter {
                                        WhereClause = string.Format("ObjectId = {0}", toRemove[i])
                                    };
                                    fc.DeleteRows(qf);
                                }
                            }
                        }
                    });
                    double[] target;
                    foreach (var (basin, (downBasin, source, (xmin, xmax), (ymin, ymax))) in centroids) {
                        if (downBasin < 0) {
                            target = Utils.nearestPoint(source, outlets);
                        } else {
                            (int, double[], (double, double), (double, double)) down;
                            if (!centroids.TryGetValue(downBasin, out down)) {
                                Utils.error(string.Format("Failed to find target grid cell in {0} for drain from {1} to {2}", subbasinsFile, basin, downBasin), this._gv.isBatch);
                                return null;
                            } else {
                                target = down.Item2;
                            }
                        }
                        var dsnode = -1;
                        foreach (var (nodeId, (pointType, pt)) in this.inletOutletPoints) {
                            var _tmp_1 = pt[0];
                            var _tmp_2 = pt[1];
                            if (xmin <= _tmp_1 && _tmp_1 < xmax && (ymin <= _tmp_2 && _tmp_2 < ymax)) {
                                Utils.loginfo(string.Format("Placed {0} with id {1} at ({2}, {3}) in grid cell {4}", Points.toString(pointType), nodeId, (int)pt[0], (int)pt[1], basin));
                                dsnode = nodeId;
                                break;
                            }
                        }
                        if (dsnode >= 0) {
                            // avoid any danger of adding point twice (and speed up for future cells)
                            this.inletOutletPoints.Remove(dsnode);
                        }
                        var feature = new OSGeo.OGR.Feature(defn);
                        // respect default 'start at outlet' of TauDEM
                        var wkt = string.Format("LINESTRING ( {0} {1}, {2} {3} )", target[0], target[1], source[0], source[1]);
                        var stream = OSGeo.OGR.Geometry.CreateFromWkt(wkt);
                        feature.SetField(linkIndex, basin);
                        feature.SetField(dslinkIndex, downBasin);
                        feature.SetField(dsnodeIndex, dsnode);
                        feature.SetField(wsnoIndex, basin);
                        feature.SetGeometry(stream);
                        drainStreamsLayer.CreateFeature(feature);
                        featureCount++;
                    }
                } else {
                    if (basinIndex < 0) {
                        return null;
                    }
                    centroids = new Dictionary<int, (int, double[], (double, double), (double, double))>();
                    await QueuedTask.Run(() => {
                        using (var rc = subbasinsLayer.Search()) {
                            while (rc.MoveNext()) {
                                var cell = rc.Current as ArcGIS.Core.Data.Feature;
                                var basin = (int)cell[basinIndex];
                                var downBasin = (int)cell[downBasinIndex];
                                var (centroid, (xmin, xmax), (ymin, ymax)) = Utils.centreGridCell(cell);
                                centroids[basin] = (downBasin, centroid, (xmin, xmax), (ymin, ymax));
                                this._gv.topo.basinCentroids[basin] = centroid;
                            }
                        }
                    });
                    using (var csvReader = new StreamReader(this.drainageTable)) {
                        // skip header
                        var line = csvReader.ReadLine();
                        while ((line = csvReader.ReadLine()) != null) {
                            string[] row = line.Split(',');
                            var basin = Convert.ToInt32(row[0]);
                            var downBasin = Convert.ToInt32(row[1]);
                            Debug.Assert(basin != downBasin, string.Format("Drainage has {0} downstream from itself", basin));
                            // we ignore down basins data in centroids map since the drainage is determined by the table
                            (int, double[], (double, double), (double, double)) result;
                            if (!centroids.TryGetValue(basin, out result)) {
                                Utils.error(string.Format("Failed to find source grid cell in {2} for drain from {0} to {1}", basin, downBasin, subbasinsFile), this._gv.isBatch);
                                return null;
                            }
                            var (_, source, (xmin, xmax), (ymin, ymax)) = result;
                            double[] target;
                            (int, double[], (double, double), (double, double)) downResult = (0, null, (0, 0), (0, 0));
                            if (downBasin < 0) {
                                target = Utils.nearestPoint(source, outlets);
                            } else {
                                if (!centroids.TryGetValue(downBasin, out downResult)) {
                                    Utils.error(string.Format("Failed to find target grid cell in {2} for drain from {0} to {1}", basin, downBasin, subbasinsFile), this._gv.isBatch);
                                    return null;
                                }
                                target = downResult.Item2;
                            }
                            var dsnode = -1;
                            foreach (var (nodeId, (pointType, pt)) in this.inletOutletPoints) {
                                var _tmp_3 = pt[0];
                                var _tmp_4 = pt[1];
                                if (xmin <= _tmp_3 && _tmp_3 < xmax && (ymin <= _tmp_4 && _tmp_4 < ymax)) {
                                    Utils.loginfo(string.Format("Placed {0} with id {1} at ({2}, {3}) in grid cell {4}", Points.toString(pointType), nodeId, (int)pt[0], (int)pt[1], basin));
                                    dsnode = nodeId;
                                    break;
                                }
                            }
                            if (dsnode >= 0) {
                                // avoid any danger of adding point twice (and speed up for future cells)
                                this.inletOutletPoints.Remove(dsnode);
                            }
                            var feature = new OSGeo.OGR.Feature(defn);
                            // respect default 'start at outlet' of TauDEM
                            var wkt = string.Format("LINESTRING ( {0} {1}, {2} {3} )", target[0], target[1], source[0], source[1]);
                            var stream = OSGeo.OGR.Geometry.CreateFromWkt(wkt);
                            feature.SetField(linkIndex, basin);
                            feature.SetField(dslinkIndex, downBasin);
                            feature.SetField(dsnodeIndex, dsnode);
                            feature.SetField(wsnoIndex, basin);
                            feature.SetGeometry(stream);
                            drainStreamsLayer.CreateFeature(feature);
                            featureCount++;
                        }
                    }
                    var subsCount = await QueuedTask.Run(() => { return subbasinsLayer.GetFeatureClass().GetCount(); });
                    if (featureCount != subsCount) {
                        Utils.information(string.Format("WARNING: number of drainage links ({0}) different from number of grid cells ({1})", featureCount, subsCount), this._gv.isBatch);
                    }
                }
                var unplaced = this.inletOutletPoints.Keys;
                if (unplaced.Count > 0) {
                    Utils.information(string.Format("Warning: failed to place inlet/outlet points with IDs {0}", unplaced), this._gv.isBatch);
                }
                ds.Dispose();
            }
            var time2 = DateTime.Now;
            Utils.loginfo(string.Format("Writing drainage grid streams took {0} seconds", Convert.ToInt32(time2.Subtract(time1).TotalSeconds)));
            // load grid streams shapefile
            // try to load above subbasins layer layer
            var drainStreamsArcLayer = (await Utils.getLayerByFilename(drainStreamsFile, FileTypes._DRAINSTREAMS, this._gv, subbasinsLayer, Utils._WATERSHED_GROUP_NAME)).Item1 as FeatureLayer;
            if (drainStreamsArcLayer is null) {
                Utils.error(string.Format("Failed to load grid streams shapefile {0}", drainStreamsFile), this._gv.isBatch);
                return null;
            }
            this._gv.streamFile = drainStreamsFile;
            // remove visibility from streams layer, if any
            var streamsLayer = Utils.getLayerByLegend(Utils._STREAMSLEGEND);
            if (streamsLayer is not null) {
                streamsLayer.SetVisibility(false);
            }
            // remove visibility from channels layer, if any
            var channelsLayer = Utils.getLayerByLegend(Utils._CHANNELSLEGEND);
            if (channelsLayer is not null) {
                channelsLayer.SetVisibility(false);
            }
            this.progress("");
            return drainStreamsFile;
        }

            // Run the landscape dialog and create files as requested.
            public async Task runLandscape() {
                if (this._gv.existingWshed) {
                // will need a d8 flow direction and a channels raster
                var suffix = System.IO.Path.GetExtension(this._gv.demFile);
                    var @base = System.IO.Path.ChangeExtension(this._gv.demFile, null);
                // check if we have flow direction because we are running an existing ArcSWAT project
                var proj = this._gv.proj;
                    var title = proj.title;
                    var (arcPFile, found) = proj.readEntry(title, "delin/arcPFile", "");
                    if (found && System.IO.File.Exists(arcPFile)) {
                        this._gv.pFile = arcPFile;
                    } else {
                        var sd8File = @base + "sd8" + suffix;
                        var pFile = @base + "p" + suffix;
                        await Utils.removeLayer(sd8File);
                        await Utils.removeLayer(pFile);
                        this.progress("D8FlowDir ...");
                        var ok = await this._gv.taudem.runD8FlowDir(this._gv.felFile, sd8File, pFile, this._dlg.numberOfProcesses, this._dlg.getTaudemOutput);
                        if (!ok) {
                            this.cleanUp(3);
                            return;
                        }
                        this._gv.pFile = pFile;
                    }
                    var channelraster = @base + "srcChannel" + suffix;
                    var demLayer = (await Utils.getLayerByFilename(this._gv.demFile, FileTypes._DEM, this._gv, null, Utils._WATERSHED_GROUP_NAME)).Item1 as RasterLayer;
                    await this.streamToRaster(demLayer, this._gv.channelFile, channelraster);
                    this._gv.srcChannelFile = channelraster;
                }
                await this.makeDistancesToOutlets();
                var numCellsSt = Convert.ToInt32(this._dlg.numCellsStText);
                var channelThresh = Convert.ToInt32(this._dlg.numCellsChText);
                // set branch method threshold to twice sqare root of stream threshold in square metres
                var branchThresh = Convert.ToInt32(2 * Math.Sqrt(numCellsSt * this._gv.topo.dx * this._gv.topo.dy));
                var clipperFile = this.clipperFile == "" ? this._gv.subbasinsFile : this.clipperFile;
                this.L = new Landscape(this._gv, this._dlg.getTaudemOutput, this._dlg.numberOfProcesses, this.progress);
                this.L.run(numCellsSt, channelThresh, branchThresh, clipperFile, this.thresholdChanged);
            }

        // Delete any files (and their layers if any) matching *flood*.* in Flood directory.
        public async Task deleteFloodFiles() {
            Matcher matcher = new Matcher();
            matcher.AddInclude("*flood*.*");
            foreach (var f in matcher.GetResultsInFullPath(this._gv.floodDir)) {
                await Utils.removeLayerAndFiles(f);
            }
        }

        // Return row and column after 1 step in D8 direction.
        public static object moveD8(int row, int col, int direction) {
            if (1 <= direction && direction <= 8) {
                var dir0 = direction - 1;
                return (row + Utils._dY[dir0], col + Utils._dX[dir0]);
            } else {
                // we have run off the edge of the direction grid
                return (-1, -1);
            }
        }

        // Use rasterize to generate a raster for the streams or channels, with a fixed value of 1 along the streams, zero elsewhere.
        public async Task streamToRaster(RasterLayer demLayer, string streamFile, string rasterFile) {
            await Utils.removeLayerAndFiles(rasterFile);
            Debug.Assert(!System.IO.File.Exists(rasterFile));
            var parms = Geoprocessing.MakeValueArray(streamFile, Topology._POLYGONID, rasterFile, this._gv.demFile);
            var environment = Geoprocessing.MakeEnvironmentArray(overwriteoutput: true);
            await Geoprocessing.ExecuteToolAsync("conversion.FeatureToRaster", parms, environment, null, null, GPExecuteToolFlags.None);
            Debug.Assert(System.IO.File.Exists(rasterFile));
            Utils.copyPrj(streamFile, rasterFile);
        }

        // 
        //         Create inlets/outlets file with points snapped to streams or channels.
        //         
        //         Inlets and outlets are snapped to the coarser streamLayer and reservoirs and point sources to the finer channelLayer.
        //         
        public async Task<bool> createSnapOutletFile(
            FeatureLayer outletLayer,
            FeatureLayer streamLayer,
            FeatureLayer channelLayer,
            string outletFile,
            string snapFile) {
            int countPoints = 0;
            int errorCount = 0;
            int count = 0;
            int outletCount = 0;
            int idIndex = -1;
            int ptIdIndex = -1;
            await QueuedTask.Run(() => {
                using (var fc = outletLayer.GetFeatureClass()) {
                    countPoints = Convert.ToInt32(fc.GetCount());
                    var defn = fc.GetDefinition();
                    idIndex = defn.FindField(Topology._ID);
                    ptIdIndex = defn.FindField(Topology._POINTID);
                }
            });
            if (countPoints == 0) {
                Utils.error(string.Format("The outlet layer {0} has no points", outletLayer.Name), this._gv.isBatch);
                return false;
            }
            int snapThreshold = 0;
            try {
                snapThreshold = Convert.ToInt32(this._dlg.delinSnapThreshold.Text);
            }
            catch (Exception) {
                Utils.error(string.Format("Cannot parse snap threshold {0} as integer.", this._dlg.delinSnapThreshold.Text), this._gv.isBatch);
                return false;
            }
            if (ptIdIndex < 0) {
                var parms = Geoprocessing.MakeValueArray(outletLayer, Topology._POINTID, "SHORT");
                var result = await Geoprocessing.ExecuteToolAsync("management.AddField", parms);
                if (string.IsNullOrEmpty(result.ReturnValue)) {
                    Utils.error("Failed to add PointId field to inlets/outlets file: " + $@"{result.ErrorMessages}", this._gv.isBatch);
                    return false;
                } else {
                    Utils.loginfo("Added PointId field to inlets/outlets file: " + $@"{result.ReturnValue}");
                    // need to recalculate ptIdIndex, which was -1.  For safety recalculate idIndex as well.
                    await QueuedTask.Run(() => {
                        using (var fc = outletLayer.GetFeatureClass()) {
                            var defn = fc.GetDefinition();
                            idIndex = defn.FindField(Topology._ID);
                            ptIdIndex = defn.FindField(Topology._POINTID);
                        }
                    });
                }
            }
            if (!await Topology.makePositiveOutletIds(outletLayer, idIndex, ptIdIndex)) {
                Utils.error(string.Format("Failed to set PointId field in inlets/outlets file {0}", outletFile), this._gv.isBatch);
                return false;
            }
            if (!await Delineation.createOutletFile(snapFile, outletFile, false, this._gv)) {
                return false;
            }
            if (this._gv.isBatch) {
                Utils.information(string.Format("Snap threshold: {0} metres", snapThreshold), this._gv.isBatch);
            }
            using (DataSource snapDs = Ogr.Open(snapFile, 1)) {
                if (snapDs is null) {
                    Utils.error(string.Format("Cannot open snap file {0}", snapFile), this._gv.isBatch);
                    return false;
                }
                OSGeo.OGR.Driver drv = snapDs.GetDriver();
                if (drv is null) {
                    Utils.error("Cannot get GDAL driver for snap file", this._gv.isBatch);
                    return false;
                }
                OSGeo.OGR.Layer snapLayer = null;
                snapLayer = snapDs.GetLayerByIndex(0);
                if (snapLayer == null) {
                    Utils.error(string.Format("No layers found in snap file {0}", snapFile), this._gv.isBatch);
                    return false;
                }
                FeatureDefn def = snapLayer.GetLayerDefn();
                var idSnapIndex = def.GetFieldIndex(Topology._ID);
                var inletSnapIndex = def.GetFieldIndex(Topology._INLET);
                var resSnapIndex = def.GetFieldIndex(Topology._RES);
                var ptsourceSnapIndex = def.GetFieldIndex(Topology._PTSOURCE);
                var ptIdSnapIndex = def.GetFieldIndex(Topology._POINTID);
                var ds = Gdal.Open(this._gv.demFile, Access.GA_ReadOnly);
                double[] transform = new double[6]; 
                ds.GetGeoTransform(transform);
                count = 0;
                errorCount = 0;
                outletCount = 0;
                //outletLayer = (await Utils.getLayerByFilename(outletFile, FileTypes._OUTLETS, this._gv, null, Utils._WATERSHED_GROUP_NAME)).Item1 as FeatureLayer;
                await QueuedTask.Run(() => {
                    using (var rc = outletLayer.Search()) {
                        while (rc.MoveNext()) {
                            var feature = rc.Current as ArcGIS.Core.Data.Feature;
                            var point = feature.GetShape() as MapPoint;
                            var pid = Convert.ToInt32(feature[Topology._ID]);
                            var inlet = Convert.ToInt32(feature[Topology._INLET]);
                            var res = Convert.ToInt32(feature[Topology._RES]);
                            var ptsource = Convert.ToInt32(feature[Topology._PTSOURCE]);
                            var ptId = Convert.ToInt32(feature[Topology._POINTID]);
                            var inletOrOutlet = inlet == 0 ? res == 0 : ptsource == 0;
                            var reachLayer = inletOrOutlet ? streamLayer : channelLayer;
                            var point1 = Topology.snapPointToReach(reachLayer, point, snapThreshold, transform, this._gv.isBatch);
                            if (point1 is null) {
                                errorCount += 1;
                                continue;
                            }
                            if (inlet == 0 && res == 0) {
                                outletCount += 1;
                            }
                            OSGeo.OGR.Feature pt1 = new OSGeo.OGR.Feature(def);
                            pt1.SetField(idSnapIndex, pid);
                            pt1.SetField(inletSnapIndex, inlet);
                            pt1.SetField(resSnapIndex, res);
                            pt1.SetField(ptsourceSnapIndex, ptsource);
                            pt1.SetField(ptIdSnapIndex, ptId);
                            OSGeo.OGR.Geometry ogrPoint = new OSGeo.OGR.Geometry(wkbGeometryType.wkbPoint);
                            ogrPoint.SetPoint_2D(0, point1.X, point1.Y);
                            pt1.SetGeometry(ogrPoint);
                            var num = snapLayer.CreateFeature(pt1);
                            if (num < 0) { errorCount++; }
                            count++;
                            // Utils.information('Snap point at ({0:G0}, {1:G0})', point1.X, point1.Y), self._gv.isBatch)
                        }
                    }
                });
            }
            var failMessage = errorCount == 0 ? "" : string.Format(": {0} failed", errorCount);
            this._dlg.snappedLabelText = string.Format("{0} snapped{1}", count, failMessage);
            if (this._gv.isBatch) {
                Utils.information(string.Format("{0} snapped{1}", count, failMessage), true);
            }
            if (count == 0) {
                Utils.error("Could not snap any points to streams or channels", this._gv.isBatch);
                return false;
            }
            if (outletCount == 0) {
                Utils.error(string.Format("The outlet layer {0} contains no outlets", outletFile), this._gv.isBatch);
                return false;
            }
            // shows we have created a snap file
            this._gv.snapFile = snapFile;
            this.snapErrors = errorCount > 0;
            return true;
        }

        // Create filePath with fields needed for outlets file, 
        //         copying .prj from sourcePath, and adding Subbasin field if wanted.
        //         
        public static async Task<bool> createOutletFile(
            string filePath,
            string sourcePath,
            bool basinWanted,
            GlobalVars gv) {
            //if (Utils.shapefileExists(filePath)) {
            //    // safer to empty it than to try to create a new one
            //    var ft = FileTypes._OUTLETS;
            //    var outletLayer = Utils.getLayerByFilename(filePath, ft, null, null, null)[0];
            //    if (outletLayer is null) {
            //        outletLayer = QgsVectorLayer(filePath, FileTypes.legend(ft), "ogr");
            //    }
            //    var OK = Utils.removeAllFeatures(outletLayer);
            //    // POINTID may be missing: assume we have ID, INLET, RES and PTSOURCE
            //    if (OK) {
            //        var ptIdIndex = gv.topo.getIndex(outletLayer, Topology._POINTID, ignoreMissing: true);
            //        if (ptIdIndex < 0) {
            //            OK = outletLayer.startEditing();
            //            if (OK) {
            //                OK = outletLayer.addAttribute(QgsField(Topology._POINTID, QVariant.Int));
            //                if (OK) {
            //                    OK = outletLayer.commitChanges();
            //                    if (OK) {
            //                        ptIdIndex = gv.topo.getIndex(outletLayer, Topology._POINTID, ignoreMissing: true);
            //                        OK = ptIdIndex >= 0;
            //                    }
            //                }
            //            }
            //        }
            //    }
            //    if (OK && basinWanted) {
            //        var basinIndex = gv.topo.getIndex(outletLayer, Topology._SUBBASIN, ignoreMissing: true);
            //        if (basinIndex < 0) {
            //            OK = outletLayer.startEditing();
            //            if (OK) {
            //                OK = outletLayer.addAttribute(QgsField(Topology._SUBBASIN, QVariant.Int));
            //                if (OK) {
            //                    OK = outletLayer.commitChanges();
            //                    if (OK) {
            //                        basinIndex = gv.topo.getIndex(outletLayer, Topology._SUBBASIN, ignoreMissing: true);
            //                        OK = basinIndex >= 0;
            //                    }
            //                }
            //            }
            //        }
            //    }
            //    if (OK) {
            //        return true;
            //    }
            //}
            await Utils.removeLayerAndFiles(filePath);
            using (var driver = Ogr.GetDriverByName("ESRI Shapefile")) 
             using (var outletDs = driver.CreateDataSource(filePath, null)) { 
                var wkt = gv.topo.crsProject.Wkt;
                OSGeo.OSR.SpatialReference srs = new OSGeo.OSR.SpatialReference(wkt);
                var outletLayer = outletDs.CreateLayer("Outlets", srs, wkbGeometryType.wkbPoint, null);
                var err = outletLayer.CreateField(new FieldDefn(Topology._ID, OSGeo.OGR.FieldType.OFTInteger), 1);
                if (err != Ogr.OGRERR_NONE) {
                    Utils.error($"Failed to create field in outlet file {filePath}", false);
                    return false;
                }
                outletLayer.CreateField(new FieldDefn(Topology._INLET, OSGeo.OGR.FieldType.OFTInteger), 1);
                outletLayer.CreateField(new FieldDefn(Topology._RES, OSGeo.OGR.FieldType.OFTInteger), 1);
                outletLayer.CreateField(new FieldDefn(Topology._PTSOURCE, OSGeo.OGR.FieldType.OFTInteger), 1);
                outletLayer.CreateField(new FieldDefn(Topology._POINTID, OSGeo.OGR.FieldType.OFTInteger), 1);
                if (basinWanted) {
                    outletLayer.CreateField(new FieldDefn(Topology._SUBBASIN, OSGeo.OGR.FieldType.OFTInteger), 1);
                }
                err = outletLayer.SyncToDisk();
                if (err != Ogr.OGRERR_NONE) {
                    Utils.error($"Failed to save layer of outlet file {filePath}", false);
                    return false;
                }
                outletLayer = null;
                err = outletDs.SyncToDisk();
                if (err != Ogr.OGRERR_NONE) {
                    Utils.error($"Failed to save data source of outlet file {filePath}", false);
                    return false;
                }
                //outletDs.Dispose();
                //outletDs = null;
            }
            Utils.copyPrj(sourcePath, filePath);
            return true;
        }

            //===========no longer used================================================================
            // def getOutletIds(self, field: str, root: QgsLayerTree) -> Set[int]:
            //     """Get list of ID values from inlets/outlets layer 
            //     for which field has value 1.
            //     """
            //     result: Set[int] = set()
            //     if self._gv.outletFile == '':
            //         return result
            //     outletLayer = Utils.getLayerByFilenameOrLegend(self._gv.outletFile, FileTypes._OUTLETS, '', self._gv.isBatch)
            //     if not outletLayer:
            //         Utils.error('Cannot find inlets/outlets layer', self._gv.isBatch)
            //         return result
            //     idIndex = self._gv.topo.getIndex(outletLayer, Topology._ID)
            //     fieldIndex = self._gv.topo.getIndex(outletLayer, field)
            //     for f in outletLayer.getFeatures():
            //         if f[fieldIndex] == 1:
            //             result.Add(f[idIndex])
            //     return result
            //===========================================================================
            // Update progress label with message
            public virtual void progress(string msg) {
                this._dlg.progress(msg);
            }

        // Close form.
        public virtual void doClose() {
            // close any landscape dialog if visible
            if (this.L is not null && this.L._dlg.Visible) {
                this.L.cancel();
            }
            this._dlg.Close();
            if (this.delineationFinishedOK) {
                if (this.finishHasRun) {
                    this._gv.writeProjectConfig(1, 0);
                    this._parent.postDelineation(1);
                } else {
                    // nothing done
                    this._parent.postDelineation(2);
                }
            } else {
                this._gv.writeProjectConfig(0, 0);
                this._parent.postDelineation(0);
            }
        }

    //        // Read delineation data from project file.
    //        public virtual void readProj() {
    //            string outletFile;
    //        string channelFile;
    //        string streamFile;
    //        string burnFile;
    //        object lakeLayer;
    //            string lakeFile;
    //        string wshedFile;
    //        string subbasinsFile;
    //        string possFile;
    //            object layer;
    //            string demFile;
    //        var proj = this._gv.proj;
    //bool found;
    //            var title = proj.title;
    //            this._dlg.delinTabWidget.SelectedIndex = 0;
    //            (this._gv.existingWshed, found) = proj.readBoolEntry(title, "delin/existingWshed", false);
    //            if (found && this._gv.existingWshed) {
    //                this._dlg.delinTabWidget.SelectedIndex = 1;
    //            }
    //            Utils.loginfo(string.Format("Existing watershed is {0}", this._gv.existingWshed));
    //            (this._gv.useGridModel, found) = proj.readBoolEntry(title, "delin/useGridModel", false);
    //            this._dlg.useGrid.setChecked(this._gv.useGridModel);
    //            this._dlg.gridBox.setChecked(this._gv.useGridModel);
    //            Utils.loginfo(string.Format("Use grid model is {0}", this._gv.useGridModel));
    //            var gridSize = proj.readNumEntry(title, "delin/gridSize", 0).Item1;
    //            this._dlg.gridSize.Value = gridSize);
    //            this._gv.gridSize = gridSize;
    //            this._dlg.drainGridButton.setChecked(proj.readBoolEntry(title, "delin/gridDrainage", true)[0]);
    //            this._dlg.drainStreamsButton.setChecked(proj.readBoolEntry(title, "delin/streamDrainage", false)[0]);
    //            (drainageTable, found) = proj.readEntry(title, "delin/drainageTable", "");
    //            if (found && drainageTable != "") {
    //                this.drainageTable = proj.readPath(drainageTable);
    //            }
    //            this.setDrainage();
    //            if (this._gv.useGridModel && this._dlg.drainTableButton.Checked) {
    //                this._dlg.selectStreams.Text = this.drainageTable);
    //            }
    //            (demFile, found) = proj.readEntry(title, "delin/DEM", "");
    //            object demLayer = null;
    //            if (found && demFile != "") {
    //                demFile = proj.readPath(demFile);
    //                (demLayer, _) = Utils.getLayerByFilename(demFile, FileTypes._DEM, this._gv, null, Utils._WATERSHED_GROUP_NAME);
    //            } else {
    //                treeLayer = Utils.getLayerByLegend(FileTypes.legend(FileTypes._DEM));
    //                if (treeLayer is not null) {
    //                    layer = treeLayer.layer();
    //                    possFile = Utils.layerFileInfo(layer).absoluteFilePath();
    //                    if (Utils.question(string.Format("Use {0} as {1} file?", possFile, FileTypes.legend(FileTypes._DEM)), this._gv.isBatch, true) == QMessageBox.Yes) {
    //                        demLayer = layer;
    //                        demFile = possFile;
    //                    }
    //                }
    //            }
    //            if (demLayer is not null) {
    //                this._gv.demFile = demFile;
    //                this._dlg.selectDem.Text = this._gv.demFile);
    //                this.setDefaultNumCells(demLayer);
    //            } else {
    //                this._gv.demFile = "";
    //            }
    //            (verticalUnits, found) = proj.readEntry(title, "delin/verticalUnits", Parameters._METRES);
    //            if (found) {
    //                this._gv.verticalUnits = verticalUnits;
    //                this._gv.setVerticalFactor();
    //            }
    //            (threshold, found) = proj.readNumEntry(title, "delin/thresholdCh", 0);
    //            if (found && threshold > 0) {
    //                try {
    //                    this._dlg.numCellsCh.Text = threshold.ToString());
    //                } catch (Exception) {
    //                }
    //            }
    //            (threshold, found) = proj.readNumEntry(title, "delin/thresholdSt", 0);
    //            if (found && threshold > 0) {
    //                try {
    //                    this._dlg.numCellsSt.Text = threshold.ToString());
    //                } catch (Exception) {
    //                }
    //            }
    //            (snapThreshold, found) = proj.readNumEntry(title, "delin/snapThreshold", 300);
    //            this._dlg.snapThreshold.Text = snapThreshold.ToString());
    //            (subbasinsFile, found) = proj.readEntry(title, "delin/subbasins", "");
    //            object subbasinsLayer = null;
    //            var ft = this._gv.useGridModel ? FileTypes._GRID : this._gv.existingWshed ? FileTypes._EXISTINGSUBBASINS : FileTypes._SUBBASINS;
    //            if (found && subbasinsFile != "") {
    //                subbasinsFile = proj.readPath(subbasinsFile);
    //                (subbasinsLayer, _) = Utils.getLayerByFilename(subbasinsFile, ft, this._gv, null, Utils._WATERSHED_GROUP_NAME);
    //            } else {
    //                treeLayer = Utils.getLayerByLegend(FileTypes.legend(ft));
    //                if (treeLayer is not null) {
    //                    layer = treeLayer.layer();
    //                    possFile = Utils.layerFileInfo(layer).absoluteFilePath();
    //                    if (Utils.question(string.Format("Use {0} as {1} file?", possFile, FileTypes.legend(ft)), this._gv.isBatch, true) == QMessageBox.Yes) {
    //                        subbasinsLayer = layer;
    //                        subbasinsFile = possFile;
    //                    }
    //                }
    //            }
    //            if (subbasinsLayer is not null) {
    //                this._dlg.selectSubbasins.Text = subbasinsFile);
    //                this._gv.subbasinsFile = subbasinsFile;
    //            } else {
    //                this._gv.subbasinsFile = "";
    //            }
    //            (wshedFile, found) = proj.readEntry(title, "delin/wshed", "");
    //            ft = this._gv.existingWshed ? FileTypes._EXISTINGWATERSHED : FileTypes._WATERSHED;
    //            if (found && wshedFile != "") {
    //                wshedFile = proj.readPath(wshedFile);
    //                var groupName = this._gv.existingWshed ? Utils._WATERSHED_GROUP_NAME : null;
    //                (_, _) = Utils.getLayerByFilename(wshedFile, ft, this._gv, null, groupName);
    //            } else if (!this._gv.useGridModel) {
    //                treeLayer = Utils.getLayerByLegend(FileTypes.legend(ft));
    //                if (treeLayer is not null) {
    //                    layer = treeLayer.layer();
    //                    possFile = Utils.layerFileInfo(layer).absoluteFilePath();
    //                    if (Utils.question(string.Format("Use {0} as {1} file?", possFile, FileTypes.legend(ft)), this._gv.isBatch, true) == QMessageBox.Yes) {
    //                        wshedFile = possFile;
    //                    }
    //                }
    //            }
    //            if (System.IO.File.Exists(wshedFile)) {
    //                this._dlg.selectWshed.Text = wshedFile);
    //                this._gv.wshedFile = wshedFile;
    //                if (this._gv.isHAWQS && this._gv.subbasinsFile == "") {
    //                    // use wshed file for subbasins
    //                    this._dlg.selectSubbasins.Text = wshedFile);
    //                    this._gv.subbasinsFile = wshedFile;
    //                }
    //            } else {
    //                this._gv.wshedFile = "";
    //            }
    //            (lakeFile, found) = proj.readEntry(title, "delin/lakes", "");
    //            ft = FileTypes._LAKES;
    //            if (found && lakeFile != "") {
    //                lakeFile = proj.readPath(lakeFile);
    //                (layer, _) = Utils.getLayerByFilename(lakeFile, ft, this._gv, null, Utils._WATERSHED_GROUP_NAME);
    //            } else {
    //                treeLayer = Utils.getLayerByLegend(FileTypes.legend(ft));
    //                if (treeLayer is not null) {
    //                    layer = treeLayer.layer();
    //                    possFile = Utils.layerFileInfo(layer).absoluteFilePath();
    //                    if (Utils.question(string.Format("Use {0} as {1} file?", possFile, FileTypes.legend(ft)), this._gv.isBatch, true) == QMessageBox.Yes) {
    //                        lakeFile = possFile;
    //                    }
    //                }
    //            }
    //            if (System.IO.File.Exists(lakeFile)) {
    //                this._dlg.selectLakes.Text = lakeFile);
    //                this._gv.lakeFile = lakeFile;
    //                lakeLayer = layer;
    //            } else {
    //                this._gv.lakeFile = "";
    //                lakeLayer = null;
    //            }
    //            (this.lakesDone, _) = proj.readBoolEntry(title, "delin/lakesDone", true);
    //            if (this._gv.useGridModel) {
    //                (this.gridLakesAdded, _) = proj.readBoolEntry(title, "delin/gridLakesAdded", false);
    //                if (this.gridLakesAdded && lakeLayer is not null) {
    //                    this.populateLakeIdCombo(lakeLayer);
    //                }
    //            } else {
    //                (this.lakePointsAdded, _) = proj.readBoolEntry(title, "delin/lakePointsAdded", false);
    //            }
    //            (burnFile, found) = proj.readEntry(title, "delin/burn", "");
    //            object burnLayer = null;
    //            if (found && burnFile != "") {
    //                burnFile = proj.readPath(burnFile);
    //                (burnLayer, _) = Utils.getLayerByFilename(burnFile, FileTypes._BURN, this._gv, null, Utils._WATERSHED_GROUP_NAME);
    //            } else {
    //                treeLayer = Utils.getLayerByLegend(FileTypes.legend(FileTypes._BURN));
    //                if (treeLayer is not null) {
    //                    layer = treeLayer.layer();
    //                    possFile = Utils.layerFileInfo(layer).absoluteFilePath();
    //                    if (Utils.question(string.Format("Use {0} as {1} file?", possFile, FileTypes.legend(FileTypes._BURN)), this._gv.isBatch, true) == QMessageBox.Yes) {
    //                        burnLayer = layer;
    //                        burnFile = possFile;
    //                    }
    //                }
    //            }
    //            if (burnLayer is not null) {
    //                this._gv.burnFile = burnFile;
    //                this._dlg.checkBurn.Checked = true;
    //                this._dlg.selectBurn.Text = burnFile);
    //            } else {
    //                this._gv.burnFile = "";
    //            }
    //            (streamFile, found) = proj.readEntry(title, "delin/net", "");
    //            object streamLayer = null;
    //            if (this._gv.useGridModel) {
    //                ft = this._gv.existingWshed ? FileTypes._DRAINSTREAMS : FileTypes._GRIDSTREAMS;
    //            } else {
    //                ft = FileTypes._STREAMS;
    //            }
    //            if (found && streamFile != "") {
    //                streamFile = proj.readPath(streamFile);
    //                (streamLayer, _) = Utils.getLayerByFilename(streamFile, ft, this._gv, null, Utils._WATERSHED_GROUP_NAME);
    //            } else {
    //                treeLayer = Utils.getLayerByLegend(FileTypes.legend(ft));
    //                if (treeLayer is not null) {
    //                    layer = treeLayer.layer();
    //                    possFile = Utils.layerFileInfo(layer).absoluteFilePath();
    //                    if (Utils.question(string.Format("Use {0} as {1} file?", possFile, FileTypes.legend(ft)), this._gv.isBatch, true) == QMessageBox.Yes) {
    //                        streamLayer = layer;
    //                        streamFile = possFile;
    //                    }
    //                }
    //            }
    //            if (streamLayer is not null) {
    //                this._gv.streamFile = streamFile;
    //            } else {
    //                this._gv.streamFile = "";
    //            }
    //            (this._gv.delinStreamFile, _) = proj.readEntry(title, "delin/delinNet", "");
    //            if (!this._gv.useGridModel) {
    //                (channelFile, found) = proj.readEntry(title, "delin/channels", "");
    //                object channelLayer = null;
    //                ft = FileTypes._CHANNELS;
    //                if (found && channelFile != "") {
    //                    channelFile = proj.readPath(channelFile);
    //                    (channelLayer, _) = Utils.getLayerByFilename(channelFile, ft, this._gv, null, Utils._WATERSHED_GROUP_NAME);
    //                } else {
    //                    treeLayer = Utils.getLayerByLegend(FileTypes.legend(ft));
    //                    if (treeLayer is not null) {
    //                        layer = treeLayer.layer();
    //                        possFile = Utils.layerFileInfo(layer).absoluteFilePath();
    //                        if (Utils.question(string.Format("Use {0} as {1} file?", possFile, FileTypes.legend(ft)), this._gv.isBatch, true) == QMessageBox.Yes) {
    //                            channelLayer = layer;
    //                            channelFile = possFile;
    //                        }
    //                    }
    //                }
    //                if (channelLayer is not null) {
    //                    this._dlg.selectStreams.Text = channelFile);
    //                    this._gv.channelFile = channelFile;
    //                } else {
    //                    this._gv.channelFile = "";
    //                    if (this._gv.isHAWQS && this._gv.streamFile != "") {
    //                        // use streams file for channels
    //                        this._dlg.selectStreams.Text = this._gv.streamFile);
    //                        this._gv.channelFile = this._gv.streamFile;
    //                    }
    //                }
    //            }
    //            (useOutlets, found) = proj.readBoolEntry(title, "delin/useOutlets", true);
    //            if (found) {
    //                this._dlg.useOutlets.setChecked(useOutlets);
    //                this.changeUseOutlets();
    //            }
    //            (outletFile, found) = proj.readEntry(title, "delin/outlets", "");
    //            object outletLayer = null;
    //            if (found && outletFile != "") {
    //                outletFile = proj.readPath(outletFile);
    //                (outletLayer, _) = Utils.getLayerByFilename(outletFile, FileTypes._OUTLETS, this._gv, null, Utils._WATERSHED_GROUP_NAME);
    //            } else {
    //                treeLayer = Utils.getLayerByLegend(FileTypes.legend(FileTypes._OUTLETS));
    //                if (treeLayer is not null) {
    //                    layer = treeLayer.layer();
    //                    possFile = Utils.layerFileInfo(layer).absoluteFilePath();
    //                    if (Utils.question(string.Format("Use {0} as {1} file?", possFile, FileTypes.legend(FileTypes._OUTLETS)), this._gv.isBatch, true) == QMessageBox.Yes) {
    //                        outletLayer = layer;
    //                        outletFile = possFile;
    //                    }
    //                }
    //            }
    //            if (outletLayer is not null) {
    //                this._gv.outletFile = outletFile;
    //                if (useOutlets) {
    //                    this._dlg.selectExistOutlets.Text = this._gv.outletFile);
    //                    this._dlg.selectOutlets.Text = this._gv.outletFile);
    //                }
    //            } else {
    //                this._gv.outletFile = "";
    //            }
    //            (snapFile, found) = proj.readEntry(title, "delin/snapOutlets", "");
    //            if (found && snapFile != "") {
    //                var snapFile = proj.readPath(snapFile);
    //                if (System.IO.File.Exists(snapFile)) {
    //                    this._gv.snapFile = snapFile;
    //                } else {
    //                    this._gv.snapFile = "";
    //                }
    //            } else {
    //                this._gv.snapFile = "";
    //                // extra outlets and reservoirs not longer available
    //                //         extraOutletFile, found = proj.readEntry(title, 'delin/extraOutlets', '')
    //                //         extraOutletLayer = None
    //                //         if found and extraOutletFile != '':
    //                //             extraOutletFile = proj.readPath(extraOutletFile)
    //                //             extraOutletLayer, _ = Utils.getLayerByFilename(extraOutletFile, FileTypes._OUTLETS,
    //                //                                                                      self._gv, None, Utils._WATERSHED_GROUP_NAME)
    //                //         else:
    //                //             treeLayer = Utils.getLayerByLegend(Utils._EXTRALEGEND)
    //                //             if treeLayer is not None:
    //                //                 layer = treeLayer.layer()
    //                //                 possFile = Utils.layerFileInfo(layer).absoluteFilePath()
    //                //                 if Utils.question('Use {0} as {1} file?', possFile, Utils._EXTRALEGEND), self._gv.isBatch, True) == QMessageBox.Yes:
    //                //                     extraOutletLayer = layer
    //                //                     extraOutletFile = possFile 
    //                //         if extraOutletLayer is not None:
    //                //             self._gv.extraOutletFile = extraOutletFile
    //                //             basinIndex = self._gv.topo.getIndex(extraOutletLayer, Topology._SUBBASIN)
    //                //             resIndex = self._gv.topo.getIndex(extraOutletLayer, Topology._RES)
    //                //             ptsrcIndex = self._gv.topo.getIndex(extraOutletLayer, Topology._PTSOURCE)
    //                //             if basinIndex >= 0 and resIndex >= 0 and ptsrcIndex >= 0:
    //                //                 extraPointSources = False
    //                //                 for point in extraOutletLayer.getFeatures():
    //                //                     if point[resIndex] == 1:
    //                //                         self.extraReservoirBasins.Add(point[basinIndex])  # TODO:
    //                //                     elif point[ptsrcIndex] == 1:
    //                //                         extraPointSources = True
    //                //                 self._dlg.checkAddPoints.setChecked(extraPointSources)  # TODO:
    //                //         else:
    //                //             self._gv.extraOutletFile = ''
    //            }
    //        }


        //    // Write delineation data to project file.
        //    public virtual void saveProj() {
        //        object snapThreshold;
        //        object numCellsSt;
        //        object numCellsCh;
        //        var proj = QgsProject.instance();
        //        var title = proj.title();
        //        proj.writeEntry(title, "delin/existingWshed", this._gv.existingWshed);
        //        proj.writeEntry(title, "delin/useGridModel", this._gv.useGridModel);
        //        proj.writeEntry(title, "delin/gridSize", this._gv.gridSize);
        //        proj.writeEntry(title, "delin/gridDrainage", this.gridDrainage);
        //        proj.writeEntry(title, "delin/streamDrainage", this.streamDrainage);
        //        proj.writeEntry(title, "delin/drainageTable", proj.writePath(this.drainageTable));
        //        proj.writeEntry(title, "delin/net", proj.writePath(this._gv.streamFile));
        //        proj.writeEntry(title, "delin/delinNet", proj.writePath(this._gv.delinStreamFile));
        //        proj.writeEntry(title, "delin/subbasins", proj.writePath(this._gv.subbasinsFile));
        //        if (!this._gv.useGridModel) {
        //            proj.writeEntry(title, "delin/channels", proj.writePath(this._gv.channelFile));
        //            proj.writeEntry(title, "delin/wshed", proj.writePath(this._gv.wshedFile));
        //        }
        //        proj.writeEntry(title, "delin/subsNoLakes", proj.writePath(this._gv.subsNoLakesFile));
        //        proj.writeEntry(title, "delin/lakes", proj.writePath(this._gv.lakeFile));
        //        proj.writeEntry(title, "delin/lakesDone", this.lakesDone);
        //        if (this._gv.useGridModel) {
        //            proj.writeEntry(title, "delin/gridLakesAdded", this.gridLakesAdded);
        //        } else {
        //            proj.writeEntry(title, "delin/lakePointsAdded", this.lakePointsAdded);
        //        }
        //        proj.writeEntry(title, "delin/DEM", proj.writePath(this._gv.demFile));
        //        var hillshadeFile = os.path.split(this._gv.demFile)[0] + "/hillshade.tif";
        //        if (System.IO.File.Exists(hillshadeFile)) {
        //            proj.writeEntry(title, "delin/hillshade", proj.writePath(hillshadeFile));
        //        }
        //        proj.writeEntry(title, "delin/useOutlets", this._dlg.useOutletsChecked);
        //        proj.writeEntry(title, "delin/outlets", proj.writePath(this._gv.outletFile));
        //        proj.writeEntry(title, "delin/snapOutlets", proj.writePath(this._gv.snapFile));
        //        proj.writeEntry(title, "delin/extraOutlets", proj.writePath(this._gv.extraOutletFile));
        //        proj.writeEntry(title, "delin/burn", proj.writePath(this._gv.burnFile));
        //        proj.writeEntry(title, "delin/verticalUnits", this._gv.verticalUnits);
        //        try {
        //            numCellsCh = Convert.ToInt32(this._dlg.numCellsCh.Text);
        //        } catch (Exception) {
        //            numCellsCh = 0;
        //        }
        //        proj.writeEntry(title, "delin/thresholdCh", numCellsCh);
        //        try {
        //            numCellsSt = Convert.ToInt32(this._dlg.numCellsSt.Text);
        //        } catch (Exception) {
        //            numCellsSt = 0;
        //        }
        //        proj.writeEntry(title, "delin/thresholdSt", numCellsSt);
        //        try {
        //            snapThreshold = Convert.ToInt32(this._dlg.snapThreshold.Text);
        //        } catch (Exception) {
        //            snapThreshold = 300;
        //        }
        //        proj.writeEntry(title, "delin/snapThreshold", snapThreshold);
        //        proj.Write();
        //    }
        //}
    }
}
