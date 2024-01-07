



using System;

using System.Collections.Generic;

using System.Diagnostics;

using System.IO;

using System.Linq;

using System.Windows.Forms;

using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Geoprocessing;
//using ArcGIS.Desktop.Internal.Mapping.Events;
using System.Threading.Tasks;
//using ArcGIS.Desktop.Mapping;
using OSGeo.OGR;
using OSGeo.GDAL;
using ArcGIS.Desktop.Framework.Controls;
using System.Printing.IndexedProperties;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;
using System.Windows.Markup;
using static System.Net.WebRequestMethods;
using System.Runtime.CompilerServices;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Desktop.Core.Utilities;

namespace ArcSWATPlus { 
    public class Landscape { 
        
        public LandscapeForm _dlg;
        
        public GlobalVars _gv;
        
        public Action<string> _progress;
        
        public double areaOfCell;
        
        public int branchThresh;
        
        public bool changing;
        
        public int channelThresh;

        public Raster channelsRaster;
        
        public int chunkCount;
        
        public string clipperFile;

        public Raster demRaster;
        
        public float floodThresh;

        public Raster flowRaster;
        
        public Floodplain FP;
        
        public Dictionary<(int, int), (int, bool)> heads;
        
        public Raster hillslopeRaster;
        
        public bool mustRun;
        
        public int noData;
        
        public int numCols;
        
        public int numProcesses;
        
        public int numRows;
        
        public OSGeo.OSR.SpatialReference projection;
        
        public int reachIdentifier;
        
        public int ridgeThresh;
        
        public Dictionary<(int, int), (int, float)> sides;
        
        public TextBox taudemOutput;
        
        public double[] transform;
        
        public Raster valleyDepthsRaster;
        
        public Landscape(GlobalVars gv, TextBox taudemOutput, int numProcesses, Action<string> progress) {
            this._gv = gv;
            this._dlg = new LandscapeForm(gv, this);
            //this._dlg.setWindowFlags(this._dlg.windowFlags() & ~Qt.WindowContextHelpButtonHint & Qt.WindowMinimizeButtonHint);
            //this._dlg.move(this._gv.landscapePos);
            //# TauDEM output buffer
            this.taudemOutput = taudemOutput;
            //# number of MPI processes to use
            this.numProcesses = numProcesses;
            //# progress report function
            this._progress = progress;
            //# flag set to true if must run even if files seem to be up to date
            this.mustRun = false;
            //# channel accumulation threshold
            this.channelThresh = 0;
            //# ridge accumulation threshold
            this.ridgeThresh = 0;
            //# branch length threshold
            this.branchThresh = 0;
            //# Slope position threshold 
            this.floodThresh = 0;
            //# clipper file: file used for clipping
            this.clipperFile = "";
            //# number of chunks to use for rasters and their arrays; increased when memory fails
            this.chunkCount = 1;
            //# raster  transform
            this.transform = null;
            //# output raster  no data value 
            this.noData = -32768;
            //# raster  projection
            this.projection = null;
            //# number of rows in output raster 
            this.numRows = 0;
            //# number of columns in output rastert
            this.numCols = 0;
            //# identifier number for reaches
            this.reachIdentifier = 0;
            //# map of (row, col) to (ident, isHead) for points which are channel heads or starts of reaches
            this.heads = null;
            //# map of (row, col) to (6*ident, elev) for head point, (6*ident + 2, elev) on left, (6*ident + 4, elev) on right of points draining directly to channel reaches
            this.sides = null;
            //# valley depths raster 
            this.valleyDepthsRaster = null;
            //# hillslope raster 
            this.hillslopeRaster = null;
            //# FloodPlain object
            this.FP = null;
            //# area of DEM cell in currently chosen units
            this.areaOfCell = 0;
            //# flag to show area being changed to prevent loop between setting cells and setting area
            this.changing = false;
        }
        
        // Run landscape using filled dem, channels, d8Flow and either 
        //         ridgep and ridges rasters, or subbasins and distances rasters.
        public virtual int run(
            int numCellsSt,
            int channelThresh,
            int branchThresh,
            string clipperFile,
            bool mustRun) {
            this._dlg.init(numCellsSt, branchThresh);
            this.channelThresh = channelThresh;
            this.clipperFile = clipperFile;
            this.mustRun = mustRun;
            if (!this._gv.isBatch) {
                this._dlg.Show();
                //this._gv.landscapePos = this._dlg.pos();
            }
            return 0;
        }
        
        // 
        //         Clip inFile with clipperFile.  Return clipped file, plus flag True if generated.
        //         
        //         This makes floodplain calculation much faster.
        //         Note there might seem little need to clip other rasters as they are only read where DEM has data
        //         but in fact is a bit faster - perhaps because of reduced storage.
        //         Also if we clip all files with same shapefile we can use common (row, col) indexes to arrays when we read them.
        //         
        public async Task<(string, bool)> clipRaster(string inFile, string clipperFile) {
            var isNewFile = false;
            var suffix = Path.GetExtension(inFile);
            var @base = Path.ChangeExtension(inFile, null);
            var clipFile = @base + "clip" + suffix;
            if (Landscape.mustClip(clipperFile, inFile, clipFile)) {
                if (System.IO.File.Exists(clipFile)) {
                    await Utils.removeLayerAndFiles(clipFile);
                }
                var parms = Geoprocessing.MakeValueArray(inFile, "", clipFile, clipperFile, "", "ClippingGeometry", "NO_MAINTAIN_EXTENT");
                var result = await Geoprocessing.ExecuteToolAsync("management.Clip", parms);
                if (string.IsNullOrEmpty(result.ReturnValue)) {
                    Utils.error("Failed to create clipped file " + clipFile + ": " + $@"{result.ErrorMessages}", this._gv.isBatch);
                    return ("", false);
                } else {
                    Utils.loginfo("Created " + clipFile + ": " + $@"{result.ReturnValue}");
                }
                Utils.copyPrj(inFile, clipFile);
                Debug.Assert(System.IO.File.Exists(clipFile), string.Format("Failed to create clipped raster {2} by clipping {0} with {1}", inFile, clipperFile, clipFile));
                isNewFile = true;
            }
            return (clipFile, isNewFile);
        }

        // Generate hillslopes and valley depths rasters.
        public async Task<bool> calcHillslopes(int channelThresh, string clipperFile) {
            // channelThresh needed in spite of being module variable since can be called directly from delineation
            // clipperFile similarly enables original subbasins shapefile to be used for grids
            if (!System.IO.File.Exists(this._gv.felFile)) {
                Utils.error(string.Format("Cannot find pit filled DEM {0}", this._gv.felFile), this._gv.isBatch);
                return false;
            }
            var suffix = Path.GetExtension(this._gv.felFile);
            var @base = Path.ChangeExtension(this._gv.felFile, null);
            if (this._gv.useGridModel && this._gv.srcChannelFile == "") {
                await this.makeSrcChannelRaster(channelThresh);
            }
            if (!System.IO.File.Exists(this._gv.srcChannelFile)) {
                Utils.error(string.Format("Cannot find channels raster {0}", this._gv.srcChannelFile), this._gv.isBatch);
                return false;
            }
            using (new CursorWait()) {
                var (dem, isNew) = await this.clipRaster(this._gv.felFile, clipperFile);
                this.mustRun = this.mustRun || isNew;
                (string channels, isNew) = await this.clipRaster(this._gv.srcChannelFile, clipperFile);
                this.mustRun = this.mustRun || isNew;
                (string d8Flow, isNew) = await this.clipRaster(this._gv.pFile, clipperFile);
                this.mustRun = this.mustRun || isNew;
                Gdal.AllRegister();
                this._gv.valleyDepthsFile = @base + "depths" + suffix;
                this._gv.clearOpenRasters();
                var completed = false;
                while (!completed) {
                    this.demRaster = new Raster(dem, this._gv, canWrite: false, isInt: false);
                    var res = demRaster.open(this.chunkCount);
                    if (!res) {
                        this._gv.closeOpenRasters();
                        return false;
                    }
                    var hillslopeFile = @base + "slopes" + suffix;
                    if (this.mustRun ||
                        !Utils.isUpToDate(dem, this._gv.valleyDepthsFile) ||
                        !Utils.isUpToDate(channels, this._gv.valleyDepthsFile) ||
                        !Utils.isUpToDate(d8Flow, this._gv.valleyDepthsFile) ||
                        !Utils.isUpToDate(dem, hillslopeFile) ||
                        !Utils.isUpToDate(channels, hillslopeFile) ||
                        !Utils.isUpToDate(d8Flow, hillslopeFile)) {
                        this.channelsRaster = new Raster(channels, this._gv, canWrite: false, isInt: true);
                        res = this.channelsRaster.open(this.chunkCount);
                        if (!res) {
                            this._gv.closeOpenRasters();
                            return false;
                        }
                        this.flowRaster = new Raster(d8Flow, this._gv, canWrite: false, isInt: true);
                        res = this.flowRaster.open(this.chunkCount);
                        if (!res) {
                            this._gv.closeOpenRasters();
                            return false;
                        }
                        // Require D8 flow raster  and channels raster  to have same geometry as dem,
                        // else, for example, 1 step downstream in flow raster  could stay in same channel pixel, or skip a channel pixel.
                        // Could probably live with just same pixel size, but they should both come from TauDEM using same DEM anyway.
                        var flowTransform = new double[6];
                        flowRaster.ds.GetGeoTransform(this.transform);
                        if (!Topology.sameTransform(this.transform, flowTransform, this.numRows, this.numCols)) {
                            Utils.error(string.Format("Clipped flow directions raster {0} and clipped DEM raster {1} must have same geometry", d8Flow, dem), this._gv.isBatch);
                            return false;
                        }
                        var chanTransform = new double[6];
                        channelsRaster.ds.GetGeoTransform(chanTransform);
                        if (!Topology.sameTransform(this.transform, chanTransform, this.numRows, this.numCols)) {
                            Utils.error(string.Format("Clipped channels raster {0} and clipped DEM raster {1} must have same geometry", channels, dem), this._gv.isBatch);
                            return false;
                        }
                        this._progress("Hillslopes ...");
                        await this.openResult(this._gv.valleyDepthsFile, isInt: false);
                        if (this.valleyDepthsRaster is null) {
                            this._gv.closeOpenRasters();
                            return false;
                        }
                        this.findHeads();
                        this.findSides();
                        await this.openResult(hillslopeFile, isInt: true);
                        if (this.hillslopeRaster is null) {
                            this._gv.closeOpenRasters();
                            return false;
                        }
                        if (!this.fillResult()) {
                            this._gv.closeOpenRasters();
                            return false;
                        }
                        this.channelsRaster.close();
                        this.flowRaster.close();
                        this.hillslopeRaster.close();
                        // close depths raster  to flush it, then reopen readonly later
                        this.valleyDepthsRaster.close();
                    }
                }
            }
            return true;
        }
        
        // Make channel raster.
        public async Task<bool> makeSrcChannelRaster(int channelThresh) {
            var suffix = Path.GetExtension(this._gv.demFile);
            var @base = Path.ChangeExtension(this._gv.demFile, null);
            var srcChannelFile = @base + "srcChannel" + suffix;
            if (this._gv.ad8File == "") {
                // can happen with existing watershed; try to find it
                var ad8File = @base + "ad8" + suffix;
                if (!Utils.isUpToDate(this._gv.demFile, ad8File)) {
                    Utils.error(string.Format("Cannot find flow accumulation file: looking for {0}", ad8File), this._gv.isBatch);
                    return false;
                } else {
                    this._gv.ad8File = ad8File;
                }
            }
            await Utils.removeLayer(srcChannelFile);
            if (! await this._gv.taudem.runThreshold(this._gv.ad8File, srcChannelFile, channelThresh.ToString(), this.numProcesses, this.taudemOutput, mustRun: true)) {
                Utils.error(string.Format("Failed to run TauDEM Threshold on {0}", this._gv.ad8File), this._gv.isBatch);
                return false;
            }
            this._gv.srcChannelFile = srcChannelFile;
            return true;
        }
        
        // Make channel shapefile.
        public async Task<bool> makeChannelShapefile(int channelThresh) {
            if (this._gv.useGridModel && this._gv.srcChannelFile == "") {
                if (! await this.makeSrcChannelRaster(channelThresh)) {
                    return false;
                }
            }
            var suffix = Path.GetExtension(this._gv.demFile);
            var @base = Path.ChangeExtension(this._gv.demFile, null);
            var baseName = Path.GetFileName(@base);
            var shapesBase = Utils.join(this._gv.shapesDir, baseName);
            var ordChannelFile = @base + "ordChannel" + suffix;
            var treeChannelFile = @base + "treeChannel.dat";
            var coordChannelFile = @base + "coordChannel.dat";
            var channelFile = shapesBase + "channel.shp";
            var wChannelFile = @base + "wChannel" + suffix;
            await Utils.removeLayer(ordChannelFile);
            await Utils.removeLayer(channelFile);
            await Utils.removeLayer(wChannelFile);
            if (! await this._gv.taudem.runStreamNet(this._gv.felFile, this._gv.pFile, this._gv.ad8File, this._gv.srcChannelFile, null, ordChannelFile, treeChannelFile, coordChannelFile, channelFile, wChannelFile, false, this.numProcesses, this.taudemOutput, mustRun: true)) {
                Utils.error(string.Format("Failed to run TauDEM StreamNet on {0}, {1} and {2}", this._gv.felFile, this._gv.pFile, this._gv.ad8File), this._gv.isBatch);
                return false;
            }
            Utils.copyPrj(this._gv.felFile, channelFile);
            Utils.copyPrj(this._gv.felFile, wChannelFile);
            this._gv.channelFile = channelFile;
            return true;
        }

        // Generate floodplain raster by buffering the channels.
        public async void calcFloodplainBuffer() {
            double drainAreaKm;
            double areaToSqKm = 0;
            if (!System.IO.File.Exists(this._gv.channelFile)) {
                if (this._gv.useGridModel) {
                    if (!await this.makeChannelShapefile(this.channelThresh)) {
                        return;
                    }
                }
                if (!System.IO.File.Exists(this._gv.channelFile)) {
                    Utils.error(string.Format("Cannot find channels shapefile {0}", this._gv.channelFile), this._gv.isBatch);
                    return;
                }
            }
            var channels = this._gv.channelFile;
            var bufferShapefile = Utils.join(this._gv.shapesDir, "bufferflood.shp");
            var ft = FileTypes._BUFFERFLOOD;
            var legend = FileTypes.legend(ft);
            // easiest to start from scratch
            await Utils.removeLayerAndFiles(bufferShapefile);
            Utils.copyShapefile(channels, this._gv.shapesDir, "bufferflood.shp");
            using (var buffDs = Ogr.Open(bufferShapefile, 1)) {
                var buffLayer = buffDs.GetLayerByIndex(0);
                // we need to add a layer with a fixed value of 1 to be used later for the flood raster
                buffLayer.CreateField(new FieldDefn("Flood", FieldType.OFTInteger), 1);
                var defn = buffLayer.GetLayerDefn();
                var drainAreaIndex = defn.GetFieldIndex(Topology._DRAINAREA);
                var floodIndex = defn.GetFieldIndex("Flood");
                if (drainAreaIndex < 0) {
                    // try for AreaC - used in ESRI stream files
                    drainAreaIndex = defn.GetFieldIndex("AreaC");
                    if (drainAreaIndex >= 0) {
                        areaToSqKm = 100.0;
                    }
                } else {
                    areaToSqKm = 1000000.0 / (this._gv.horizontalFactor * this._gv.horizontalFactor);
                }
                buffLayer.ResetReading();
                OSGeo.OGR.Feature reach;
                do {
                    reach = buffLayer.GetNextFeature();
                    if (reach is null) { break; }
                    if (drainAreaIndex < 0) {
                        drainAreaKm = 100;
                    } else {
                        drainAreaKm = reach.GetFieldAsDouble(drainAreaIndex) / areaToSqKm;
                    }
                    var bufferWidth = this._gv.channelWidthMultiplier * Math.Pow(drainAreaKm, this._gv.channelWidthExponent) * this._dlg.bufferMultiplierValue;
                    var bufferGeometry = reach.GetGeometryRef().Buffer(bufferWidth, quadsecs: 30);
                    reach.SetGeometry(bufferGeometry);
                    reach.SetField(floodIndex, 1);
                    buffLayer.SetFeature(reach);
                } while (reach is not null);
            }
            var bufferRasterFile = Utils.join(this._gv.floodDir, "bufferflood" + this._dlg.bufferMultiplierValue.ToString() + ".tif");
            await Utils.removeLayerAndFiles(bufferRasterFile);
            Debug.Assert(!System.IO.File.Exists(bufferRasterFile));
            var parms = Geoprocessing.MakeValueArray(bufferShapefile, "Flood", bufferRasterFile, this._gv.demFile);
            var environment = Geoprocessing.MakeEnvironmentArray(overwriteoutput: true);
            await Geoprocessing.ExecuteToolAsync("conversion.FeatureToRaster", parms, environment, null, null, GPExecuteToolFlags.None);
            Debug.Assert(System.IO.File.Exists(bufferRasterFile));
            Utils.copyPrj(bufferShapefile, bufferRasterFile);
            // load flood abaove DEM
            var demLayer = Utils.getLayerByLegend(FileTypes.legend(FileTypes._DEM));
            var floodLayer = (await Utils.getLayerByFilename(bufferRasterFile, FileTypes._BUFFERFLOOD, this._gv, demLayer, Utils._WATERSHED_GROUP_NAME)).Item1;
            if (floodLayer is null) {
                Utils.error(string.Format("Failed to load buffer floodplain {0}", bufferRasterFile), this._gv.isBatch);
            }
        }

        // Generate floodplain raster using DEM inversion or branch length method.
        public async void calcFloodplain(bool useInversion) {
            bool isNew;
            string ridgep = null;
            string ridges = null;
            string subbasins = null;
            string distances = null;
            string slopeDir = null;
            string flowAcc = null;
            using (new CursorWait()) {
                if (useInversion) {
                    // generate inverted flow accumulation to get ridge flow directions and slopes
                    var time1 = DateTime.Now;
                    var ridgesResult = await this.calcRidges();
                    if (ridgesResult.Item1 is null) {
                        return;
                    }
                    var time2 = DateTime.Now;
                    Utils.loginfo(string.Format("Inverted ridges slope and accumulation took {0} seconds", Convert.ToInt32(time2.Subtract(time1).TotalSeconds)));
                    var (d8Inv, accInv) = ridgesResult;
                    (ridgep, isNew) = await this.clipRaster(d8Inv, this.clipperFile);
                    this.mustRun = this.mustRun || isNew;
                    (ridges, isNew) = await this.clipRaster(accInv, this.clipperFile);
                    this.mustRun = this.mustRun || isNew;
                } else {
                    var suffix = Path.GetExtension(this._gv.demFile);
                    var @base = Path.ChangeExtension(this._gv.demFile, null);
                    (subbasins, isNew) = await this.clipRaster(this._gv.basinFile, this.clipperFile);
                    this.mustRun = this.mustRun || isNew;
                    (distances, isNew) = await this.clipRaster(this._gv.distStFile, this.clipperFile);
                    this.mustRun = this.mustRun || isNew;
                    (slopeDir, isNew) = await this.clipRaster(this._gv.pFile, this.clipperFile);
                    this.mustRun = this.mustRun || isNew;
                    var ad8File = @base + "ad8" + suffix;
                    (flowAcc, isNew) = await this.clipRaster(ad8File, this.clipperFile);
                    this.mustRun = this.mustRun || isNew;
                }
                this.FP = new Floodplain(this._gv, this._progress, this.chunkCount);
                this.FP.run(this.demRaster, this._gv.valleyDepthsFile, this.ridgeThresh, this.floodThresh, this.branchThresh, subbasins, distances, slopeDir, flowAcc, ridgep, ridges, this.noData, this.mustRun);
            }
        }
        
        // Return true if outFile does not exist, or is earlier than clipFile or inFile.
        public static bool mustClip(string clipFile, string inFile, string outFile) {
            return !(Utils.isUpToDate(clipFile, outFile) && Utils.isUpToDate(inFile, outFile));
        }
        
        //// Enable dialog items or not according to hillslopes check box state.
        //public virtual void hillslopesCheck() {
        //    if (this._dlg.hillslopesCheckBox.Checked || this._dlg.methodTab.currentIndex() == 0) {
        //        this._dlg.floodplainCheckBox.setEnabled(true);
        //    } else {
        //        this._dlg.floodplainCheckBox.setEnabled(false);
        //    }
        //    this.floodplainCheck();
        //}
        
        //// Enable dialog items or not according to floodplain check box state.
        //public virtual void floodplainCheck() {
        //    if (this._dlg.floodplainCheckBox.Checked) {
        //        this._dlg.methodTab.setEnabled(true);
        //        this._dlg.slopePositionSpinBox.setEnabled(true);
        //        this._dlg.slopePositionLabel.setEnabled(true);
        //    } else {
        //        this._dlg.methodTab.setEnabled(false);
        //        this._dlg.slopePositionSpinBox.setEnabled(false);
        //        this._dlg.slopePositionLabel.setEnabled(false);
        //    }
        //    this.methodTabCheck();
        //}

        // A parameter has changed, so must generate files.
        public virtual void parameterChange() {
            this.mustRun = true;
        }

        // Run landscape generation.
        public async void generate() {
            bool ok;
            var doHillslopes = false; //  this._dlg.hillslopesCheckBox.Checked;
            var doFloodplain = true; // this._dlg.floodplainCheckBox.Checked;
            //if (!doHillslopes && !doFloodplain) {
            //    this._dlg.close();
            //}
            var useBuffer = false;
            var useInversion = false;
            var index = this._dlg.methodTabSelectedIndex;
            if (doFloodplain) {
                if (index == 0) {
                    useBuffer = true;
                } else if (index == 1) {
                    try {
                        this.ridgeThresh = Convert.ToInt32(this._dlg.ridgeThresholdCellsText);
                        useInversion = true;
                        doHillslopes = true;
                    } catch (Exception) {
                        Utils.information("Please set a ridge threshold (an integer number of cells)", this._gv.isBatch);
                        return;
                    }
                } else {
                    try {
                        this.branchThresh = Convert.ToInt32(this._dlg.branchThresholdText);
                        doHillslopes = true;
                    } catch (Exception) {
                        Utils.information("Please set a branch length threshold (an integer number of metres)", this._gv.isBatch);
                        return;
                    }
                }
            }
            if (doHillslopes || doFloodplain && !useBuffer) {
                ok = await this.calcHillslopes(this.channelThresh, this.clipperFile);
            } else {
                ok = true;
            }
            if (ok && doFloodplain) {
                if (useBuffer) {
                    this.calcFloodplainBuffer();
                } else {
                    this.floodThresh = this._dlg.slopePositionSpinBoxValue;
                    this.calcFloodplain(useInversion);
                }
            }
        }
        
        // Close the dialog.
        public virtual void cancel() {
            this._gv.closeOpenRasters();
            if (!(this.valleyDepthsRaster is null)) {
                this.valleyDepthsRaster.close();
            }
            if (!(this.hillslopeRaster is null)) {
                this.hillslopeRaster.close();
            }
            if (this.FP is not null) {
                if (!(this.FP.valleyDepthsRaster is null)) {
                    this.FP.valleyDepthsRaster.close();
                }
                if (!(this.FP.ridgeHeightsRaster is null)) {
                    this.FP.ridgeHeightsRaster.close();
                }
                if (!(this.FP.ridgeDistancesRaster is null)) {
                    this.FP.ridgeDistancesRaster.close();
                }
                if (!(this.FP.floodplainRaster is null)) {
                    this.FP.floodplainRaster.close();
                }
            }
            this._dlg.Close();
        }
        
        //======No longer used - results poor=====================================================================
        // def calcRidges(self):
        //     """
        //     Create the ridges raster as flow accumulations.
        //     
        //     This is done by reversing the D8 flow directions and recalculating accumulation.
        //     This is faster than inverting the DEM and running from PitFill, 
        //     and safe as there is a loop in the reversed directions iff there is a loop in the original.
        //     """
        //     base, ext = os.path.splitext(self._gv.demFile)
        //     assert System.IO.File.Exists(self._gv.pFile), u'Cannot find d8 flow directions file'
        //     dirInv = base + 'invp' + ext
        //     accInv = base + 'invad8' + ext
        //     if not Utils.isUpToDate(self._gv.pFile, dirInv) or not Utils.isUpToDate(self._gv.pFile, accInv):
        //         pLayer = QgsRasterLayer(self._gv.pFile, 'P')
        //         entry = QgsRasterCalculatorEntry()
        //         entry.bandNumber = 1
        //         entry.raster = pLayer
        //         entry.ref = 'P@1'
        //         # The formula is equivalent to 'if P@1 <= 4 then P@1 + 4 else P@1 - 4'
        //         # since Booleans expressions evaluate to 1 (true) or 0 (false).
        //         # It reverses the TauDEM D8 direction P@1
        //         formula = '((P@1 <= 4) * (P@1 + 4) + (P@1 > 4) * (P@1 - 4))'
        //         calc = QgsRasterCalculator(formula, dirInv, 'GTiff', pLayer.extent(), pLayer.width(), pLayer.height(), [entry])
        //         result = calc.processCalculation(feedback=None)
        //         if result == 0:
        //             assert System.IO.File.Exists(dirInv), u'QGIS calculator formula {0} failed to write output', formula)
        //             Utils.copyPrj(self._gv.pFile, dirInv)
        //         else:
        //             Utils.error(u'QGIS calculator formula {0} failed: returned {1}', formula, result), self._gv.isBatch)
        //             return None     
        //         if not TauDEMUtils.runAreaD8(dirInv, accInv, None, None, self.numProcesses, self.taudemOutput, contCheck=False, mustRun=False):
        //             Utils.error(u'Failed to run TauDEM AreaD8 on {0}', dirInv), self._gv.isBatch)
        //             return None
        //     return dirInv, accInv
        //===========================================================================
        // 
        //         Create the ridges raster as flow accumulations.
        //         
        //         This is done by negating the DEM and recalculating accumulation
        //         via PitFill, D8FlowDir and AreaD8.
        //         
        public async Task<(string, string)> calcRidges() {
            var suffix = Path.GetExtension(this._gv.demFile);
            var @base = Path.ChangeExtension(this._gv.demFile, null);
            var demInv = @base + "inv" + suffix;
            var dirInv = @base + "invp" + suffix;
            var accInv = @base + "invad8" + suffix;
            if (!Utils.isUpToDate(this._gv.demFile, dirInv) || !Utils.isUpToDate(this._gv.demFile, accInv)) {
                var demRaster = new Raster(this._gv.demFile, this._gv, canWrite: false, isInt: false);
                demRaster.open(this.chunkCount);
                int numRows = demRaster.numRows;
                int numCols = demRaster.numCols;
                var transform = new double[6];
                demRaster.ds.GetGeoTransform(transform);
                float noData = (float)demRaster.noData;
                var demInvRaster = new Raster(demInv, this._gv, canWrite: true, isInt: false);
                demInvRaster.open(this.chunkCount, numRows, numCols, transform, demRaster.ds.GetSpatialRef(), demRaster.noData);
                for (int row = 0; row < numRows; row++) {
                    for (int col = 0; col < numCols; col++) {
                        var val = (float)demRaster.read(row, col);
                        if (val != noData) {
                            val = 0 - val;
                        }
                        demInvRaster.write(row, col, val);
                    }
                }
                demInvRaster.close();
                demRaster.close();
                Utils.copyPrj(this._gv.demFile, demInv);
                var invFel = @base + "invFel" + suffix;
                if (!await this._gv.taudem.runPitFill(demInv, null, invFel, this.numProcesses, this.taudemOutput)) {
                    Utils.error(string.Format("Failed to run TauDEM PitFill on {0}", demInv), this._gv.isBatch);
                    return (null, null);
                }
                Utils.copyPrj(this._gv.demFile, invFel);
                var sd8Inv = @base + "invsd8" + suffix;
                if (!await this._gv.taudem.runD8FlowDir(invFel, sd8Inv, dirInv, this.numProcesses, this.taudemOutput)) {
                    Utils.error(string.Format("Failed to run TauDEM D8FlowDir on {0}", invFel), this._gv.isBatch);
                    return (null, null);
                }
                Utils.copyPrj(this._gv.demFile, sd8Inv);
                Utils.copyPrj(this._gv.demFile, dirInv);
                if (!await this._gv.taudem.runAreaD8(dirInv, accInv, null, null, this.numProcesses, this.taudemOutput, contCheck: false, mustRun: false)) {
                    Utils.error(string.Format("Failed to run TauDEM AreaD8 on {0}", dirInv), this._gv.isBatch);
                    return (null, null);
                }
                Utils.copyPrj(this._gv.demFile, accInv);
            }
            return (dirInv, accInv);
        }
        
        // Populate heads dictionary for starts of reaches.
        public virtual void findHeads() {
            this.heads = new Dictionary<(int, int), (int, bool)>();
            this.reachIdentifier = 0;
            foreach (var row in Enumerable.Range(0, this.numRows)) {
                foreach (var col in Enumerable.Range(0, this.numCols)) {
                    if ((int)this.channelsRaster.read(col, row) > 0) {
                        var count = this.upSlopeChannelPointsCount(row, col);
                        if (count == 0) {
                            // head of channel
                            this.reachIdentifier += 1;
                            this.heads[(row, col)] = (this.reachIdentifier, true);
                        } else if (count > 1) {
                            // junction - new reach starts
                            this.reachIdentifier += 1;
                            this.heads[(row, col)] = (this.reachIdentifier, false);
                        }
                    }
                }
            }
        }
        
        // 
        //         Add to sides dictionary map of (row, col) -> (val, elevation) for head points, right and left side points of channels.
        //         
        //         val is 6 * ident for head points, 6 * ident + 2 for left side points, 6 * ident + 4 for right side points,
        //         where ident is the value in the heads map.
        //         
        public virtual void findSides() {
            int pointsToIgnore;
            int y;
            int x;
            float elev;
            int n;
            int dir0;
            this.sides = new Dictionary<(int, int), (int, float)>();
            foreach (var ((r, c), (ident, isHead)) in this.heads) {
                var row = r;
                var col = c;
                if (isHead) {
                    // find a point draining in to here to use for elevation (hoping to avoid burned-in points)
                    dir0 = (int)this.flowRaster.read(row, col) - 1;
                    if (!(0 <= dir0 && dir0 < 8)) {
                        break;
                    }
                    n = dir0;
                    // loop clockwise (arbitrary - could go other way)
                    while (true) {
                        n = (n - 1) % 8;
                        if (n == dir0) {
                            // no point not on some channel drains into here - can use own elevation
                            elev = (float)demRaster.read(row, col);
                            break;
                        }
                        x = col + Utils._dX[n];
                        y = row + Utils._dY[n];
                        if (!this.pointInMap(y, x)) {
                            continue;
                        }
                        if (this.downSlopePoint(y, x) == (row, col)) {
                            // (y, x) drains directly to (row, col)
                            if ((int)this.channelsRaster.read(row, col) > 0) {
                                // found an upslope channel point
                                continue;
                            }
                            elev = (float)demRaster.read(row, col);
                            break;
                        }
                    }
                    this.sides[(row, col)] = (6 * ident, elev);
                    // start one step down
                    (row, col) = this.downSlopePoint(row, col);
                }
                // If we are starting from a junction we will keep looking on left until meet last incoming channel
                // else may miss points between incoming channels.
                // This arbitrarily puts such points on the left side; could have chosen the right
                if (isHead) {
                    pointsToIgnore = 0;
                } else {
                    pointsToIgnore = this.upSlopeChannelPointsCount(row, col) - 1;
                }
                // loop through points on reach
                while (true) {
                    if (!this.pointInMap(row, col)) {
                        break;
                    }
                    dir0 = (int)flowRaster.read(row, col) - 1;
                    if (!(0 <= dir0 && dir0 < 8)) {
                        break;
                    }
                    n = dir0;
                    // loop clockwise to find left side points
                    while (true) {
                        n = (n - 1) % 8;
                        if (n == dir0) {
                            break;
                        }
                        x = col + Utils._dX[n];
                        y = row + Utils._dY[n];
                        if (!this.pointInMap(y, x)) {
                            continue;
                        }
                        if (this.downSlopePoint(y, x) == (row, col)) {
                            // (y, x) drains directly to (row, col)
                            if ((int)this.channelsRaster.read(y, x) > 0) {
                                // found upslope channel point
                                if (pointsToIgnore == 0) {
                                    break;
                                } else {
                                    pointsToIgnore -= 1;
                                }
                            } else {
                                this.sides[(y, x)] = (6 * ident + 2, (float)this.demRaster.read(y, x));
                            }
                        }
                    }
                    // loop anticlockwise to find right side points
                    n = dir0;
                    while (true) {
                        n = (n + 1) % 8;
                        if (n == dir0) {
                            break;
                        }
                        x = col + Utils._dX[n];
                        y = row + Utils._dY[n];
                        if (!this.pointInMap(y, x)) {
                            continue;
                        }
                        if (this.downSlopePoint(y, x) == (row, col)) {
                            // (y, x) drains directly to (row, col)
                            if ((int)this.channelsRaster.read(row, col) > 0) {
                                // found upslope channel point
                                break;
                            } else {
                                this.sides[(y, x)] = (6 * ident + 4, (float)this.demRaster.read(row, col));
                            }
                        }
                    }
                    // move to next point downstream
                    var downChannelPoint = this.downSlopePoint(row, col);
                    if (downChannelPoint.Item1 < 0 || this.heads.ContainsKey(downChannelPoint)) {
                        // off map or next reach
                        break;
                    }
                    (row, col) = downChannelPoint;
                    if ((int)this.channelsRaster.read(row, col) <= 0) {
                        // off end of reach: outlet
                        break;
                    }
                }
            }
        }
        
        // Fill hillslope raster with values of points in sides that each point drains to,
        //         and valleyDepthsRaster with point elevation - elevation of point in side it drains to.
        public virtual bool fillResult() {
            int hasVal;
            double channelsNoData;
            this.channelsRaster.band.GetNoDataValue(out channelsNoData, out hasVal);
            foreach (var row in Enumerable.Range(0, this.numRows)) {
                foreach (var col in Enumerable.Range(0, this.numCols)) {
                    if ((int)channelsRaster.read(row, col) != channelsNoData) {
                        var (val, elev, path, ok) = this.drainsTo(row, col);
                        if (!ok) {
                            return false;
                        }
                        this.propagate(val, elev, path);
                    }
                }
            }
            return true;
        }
        
        // Open raster  for writing and return it if OK.
        public async Task<Raster> openResult(string fileName, bool isInt = true) {
            if (System.IO.File.Exists(fileName)) {
                await Utils.removeLayerAndFiles(fileName);
            }
            var raster = new Raster(fileName, this._gv, canWrite: true, isInt: isInt);
            var res = raster.open(this.chunkCount, numRows: this.numRows, numCols: this.numCols, transform: this.transform, projection: this.projection, noData: this.noData);
            if (res) {
                return raster;
            } else {
                return null;
            }
        }
        
        // Return drainage value, valley floor elevation and drainage path for (row, col), plus boolean flag indicating OK or error.
        public virtual (int,  float, List<(int, int)>, bool) drainsTo(int row, int col) {
            var path = new List<(int, int)>();
            while (true) {
                int val = (int)this.hillslopeRaster.read(row, col);
                if (val >= 0) {
                    // already done this point
                    // if, eg, this point is 100m in elevation, and its valley depth is 60m, its valley floor point is at 40m
                    return (val, (float)this.demRaster.read(row, col) - (float)this.valleyDepthsRaster.read(row, col), path, true);
                }
                float elev;
                (int, float) res;
                if (!this.sides.TryGetValue((row, col), out res)) {
                    val = -1;
                    elev = 0;
                } else {
                    (val, elev) = res;
                }
                if (val > 0) {
                    // found the headwater or side drainage point
                    path.Add((row, col));
                    return (val, elev, path, true);
                }
                if ((int)this.channelsRaster.read(row, col) > 0) {
                    if (path.Count > 0) {
                        var (startRow, startCol) = path[0];
                        var (startX, startY) = Topology.cellToProj(startCol, startRow, this.transform);
                        var (x, y) = Topology.cellToProj(col, row, this.transform);
                        Utils.error(string.Format("Hit channel at ({0}, {1}) from ({2}, {3}). Landscape not created", x, y, startX, startY), this._gv.isBatch);
                        return (-1, 0, new List<(int, int)>(), false);
                    } else {
                        return (0, (float)this.demRaster.read(row, col), new List<(int, int)> {
                            (row, col)
                        }, true);
                    }
                }
                if (path.Contains((row, col))) {
                    var (startRow, startCol) = path[0];
                    var (startX, startY) = Topology.cellToProj(startCol, startRow, this.transform);
                    var (x, y) = Topology.cellToProj(col, row, this.transform);
                    Utils.error(string.Format("Loop to ({0}, {1}) from ({2}, {3}). Landscape not created.", x, y, startX, startY), this._gv.isBatch);
                    return (-1, 0, new List<(int, int)>(), false);
                }
                path.Add((row, col));
                var pt = this.downSlopePoint(row, col);
                if (pt.Item1 < 0) {
                    var (startRow, startCol) = path[0];
                    var (startX, startY) = Topology.cellToProj(startCol, startRow, this.transform);
                    var (x, y) = Topology.cellToProj(col, row, this.transform);
                    Utils.error(string.Format("Dead end at ({0}, {1}) from ({2}, {3}). Landscape not created.", x, y, startX, startY), this._gv.isBatch);
                    return (-1, 0, new List<(int, int)>(), false);
                }
                (row, col) = pt;
            }
        }
        
        // Set points on path in hillslopeRaster to val, and in valleyDepthsRaster to point elevation - elev.
        public virtual void propagate(int val, float elev, List<(int, int)> path) {
            if (path is not null) {
                foreach (var (row, col) in path) {
                    this.hillslopeRaster.write(row, col, val);
                    this.valleyDepthsRaster.write(row, col, (float)this.demRaster.read(row, col) - elev);
                }
            }
        }
        
        // Return number of points flowing into (row, col) that are on the channel.
        public virtual int upSlopeChannelPointsCount(int row, int col) {
            var count = 0;
            foreach (var n in Enumerable.Range(0, 8)) {
                var rown = row + Utils._dY[n];
                var coln = col + Utils._dX[n];
                if (this.pointInMap(rown, coln)) {
                    if (this.downSlopePoint(rown, coln) == (row, col) && (int)this.channelsRaster.read(row, col) > 0) {
                        count += 1;
                    }
                }
            }
            return count;
        }
        
        // Return point down slope from (row, col).
        public virtual (int, int) downSlopePoint(int row, int col) {
            var dir0 = (int)this.flowRaster.read(row, col) - 1;
            if (0 <= dir0 && dir0 < 8) {
                var row1 = row + Utils._dY[dir0];
                var col1 = col + Utils._dX[dir0];
                if (this.pointInMap(row1, col1)) {
                    return (row1, col1);
                } else {
                    return (-1, -1);
                }
            } else {
                return (-1, -1);
            }
        }
        
        // Return true if row and col are in the limits for the channel raster  array.
        public virtual bool pointInMap(int row, int col) {
            return 0 <= row && row < this.numRows && (0 <= col && col < this.numCols);
        }
    }
}
