



using System;

using System.IO;

using System.Linq;

using System.Collections.Generic;

using System.Diagnostics;
using System.Threading.Tasks;

using OSGeo.OSR;
using OSGeo.GDAL;
using System.Printing.IndexedProperties;
using ArcGIS.Desktop.Internal.Catalog;

namespace ArcSWATPlus { 


    
    // 
    //     Calculate floodplain raster  from DEM, stream threshold, ridge threshold, floodplain threshold,
    //     (optionally) D8 flow direction grid, flow accumulation grid, outlets shapefile.
    //      
    //     The slope position value is, for each point, (e - v) / (r - v) where e is the elevation, 
    //     v the nearest valley floor elevation, and r the nearest ridge elevation.  Nearest means along D8 flow path.
    //     Valley floor is points with flow accumulation above stream generation threshold.
    //     Ridge is similar, obtained by inverting DEM, then calculating D8 flow direction and accumulation.
    //     The floodplain raster  is zero for points with slope position value at most the floodplain threshold, else 1.
    //     
    public class Floodplain {
        
        public GlobalVars _gv;
        
        public Action<string> _progress;
        
        public int branchThresh;
        
        public int chunkCount;
        
        public double demNoData;
        
        public SpatialReference demProjection;
        
        public Raster demRaster;
        
        public double[] demTransform;
        
        public Raster floodplainRaster;
        
        public float floodThresh;
        
        public double noData;
        
        public int numCols;
        
        public int numRows;
        
        public Raster ridgeDistancesRaster;
        
        public string ridgeHeightsFile;
        
        public Raster ridgeHeightsRaster;
        
        public Dictionary<(int, int), (float, double)> ridgePoints;
        
        public int ridgeThresh;
        
        public bool useInversion;
        
        public Raster valleyDepthsRaster;
        
        public Floodplain(GlobalVars gv, Action<string> progress, int chunkCount) {
            this._gv = gv;
            this._progress = progress;
            //# raster  chunk count
            this.chunkCount = chunkCount;
            //# ridge accumulation threshold
            this.ridgeThresh = 0;
            //# floodplain accumulation threshold
            this.floodThresh = 0;
            //# branch length threshold
            this.branchThresh = 0;
            //# dem raster  transform
            this.demTransform = new double[6];
            //# dem raster 
            this.demRaster = null;
            //# dem raster  no data value
            this.demNoData = -1;
            //# dem raster  projection
            this.demProjection = null;
            //# no data value for output rasters; must be negative
            this.noData = -1;
            //# number of rows in input clipped DEM raster 
            this.numRows = 0;
            //# number of columns in input clipped DEM raster 
            this.numCols = 0;
            //# depths of valley floor below points
            this.valleyDepthsRaster = null;
            //# file path for ridgeHeightsRaster
            this.ridgeHeightsFile = "";
            //# heights of ridge above points
            this.ridgeHeightsRaster = null;
            //# distances of ridge from points
            this.ridgeDistancesRaster = null;
            //# floodplain raster 
            this.floodplainRaster = null;
            //# flag to indicate if using inversion or branch length method
            this.useInversion = true;
            //# map of (row, col) to (elevation, branch length) for ridge points
            this.ridgePoints = null;
        }
        
        // 
        //         Generate floodplain raster , with floodplain value 0 and upland value 1.
        //         
        //         valleyDepthsRaster is already calculated and stored in valleyDepthsFile
        //         ridgeThresh is the threshold in pixel counts for ridges (inverse accumulation).
        //         floodThresh is the critical ratio of relative depth of valley floor against height of ridge above valley floor.
        //         branchThresh is the threshold in metres for branch lengths.
        //         dem is clipped filled DEM
        //         The groups {subbasins, distances, slopeDir, flowAcc} and {ridgep, ridges} are either all None or all existing files,
        //         assumed to be clipped and with same pixel size as DEM (so (row, col|) coordinates may be shared across all of them).
        //         The second group is used to calculate ridges by DEM inversion if subbasins is None, else the first is used to 
        //         calculate ridges by branch lengths.
        //         
        public async Task run(
            Raster demRaster,
            string valleyDepthsFile,
            int ridgeThresh,
            float floodThresh,
            int branchThresh,
            string subbasins,
            string  distances,
            string slopeDir,
            string flowAcc,
            string ridgep,
            string ridges,
            int noData,
            bool mustRun) {
            DateTime time2;
            DateTime time1;
            this.demRaster = demRaster;
            this.ridgeThresh = ridgeThresh;
            this.floodThresh = floodThresh;
            this.branchThresh = branchThresh;
            Gdal.AllRegister();
            var ds = this.demRaster.ds;
            this.numRows = demRaster.numRows;
            this.numCols = demRaster.numCols;
            ds.GetGeoTransform(this.demTransform);
            int hasval;
            this.demRaster.band.GetNoDataValue(out this.demNoData, out hasval);
            this.demProjection = ds.GetSpatialRef();
            this.noData = noData;
            this._progress("Ridge heights ...");
            if (subbasins is null) {
                // use inverted DEM method for ridges
                this.useInversion = true;
                time1 = DateTime.Now;
                if (!await this.calcRidgeHeightsByInversion(ridgep, ridges, mustRun)) {
                    return;
                }
                time2 = DateTime.Now;
                Utils.loginfo(string.Format("Ridge heights by inversion took {0} seconds", time2.Subtract(time1).TotalSeconds));
            } else {
                this.useInversion = false;
                time1 = DateTime.Now;
                if (!await this.calRidgeHeghtsByBranchLength(subbasins, distances, slopeDir, flowAcc, valleyDepthsFile, mustRun)) {
                    return;
                }
                time2 = DateTime.Now;
                Utils.loginfo(string.Format("Ridge heights by branch length took {0} seconds", time2.Subtract(time1).TotalSeconds));
            }
            time1 = DateTime.Now;
            await this.writeFloodPlain(valleyDepthsFile, mustRun);
            time2 = DateTime.Now;
            Utils.loginfo(string.Format("Floodplain creation took {0} seconds", time2.Subtract(time1).TotalSeconds));
            this._progress("");
        }
        
        // 
        //         Create the ridgeHeightsRaster with differences between the elevation at the point and 
        //         the elevation of the nearest ridge cell.
        //         
        //         ridgep is the D8 flow directions and ridges the flow accumulation raster , both calculated from an inverted DEM.
        //         
        public async Task<bool> calcRidgeHeightsByInversion(string ridgep, string ridges, bool mustRun) {
            this.ridgeHeightsFile = Utils.join(this._gv.demDir, "invheights.tif");
            if (mustRun || !Utils.isUpToDate(ridgep, this.ridgeHeightsFile) || !Utils.isUpToDate(ridges, this.ridgeHeightsFile)) {
                this._gv.clearOpenRasters();
                var completed = false;
                while (!completed) {
                    try {
                        completed = true;
                        if (File.Exists(this.ridgeHeightsFile)) {
                            await Utils.removeLayerAndFiles(this.ridgeHeightsFile);
                        }
                        this.ridgeHeightsRaster = new Raster(this.ridgeHeightsFile, this._gv, canWrite: true, isInt: false);
                        var res = this.ridgeHeightsRaster.open(this.chunkCount, numRows: this.numRows, numCols: this.numCols, transform: this.demTransform, projection: this.demProjection, noData: this.noData);
                        if (!res) {
                            return false;
                        }
                        // ridgep obtained by inversion has float values, although these are in the range 1 to 8
                        var ridgepRaster = new Raster(ridgep, this._gv, canWrite: false, isInt: false);
                        res = ridgepRaster.open(this.chunkCount);
                        if (!res) {
                            this._gv.closeOpenRasters();
                            return false;
                        }
                        var ridgesRaster = new Raster(ridges, this._gv, canWrite: false, isInt: true);
                        res = ridgesRaster.open(this.chunkCount);
                        if (!res) {
                            this._gv.closeOpenRasters();
                            return false;
                        }
                        foreach (var row in Enumerable.Range(0, this.numRows)) {
                            foreach (var col in Enumerable.Range(0, this.numCols)) {
                                if (this.demRaster.read(row, col) != this.demNoData) {
                                    var (val, path) = this.valueAtNearest(row, col, ridgepRaster, ridgesRaster, this.ridgeThresh);
                                    if (path is not null) {
                                        this.propagate(val, path);
                                    }
                                }
                            }
                        }
                        ridgepRaster.close();
                        ridgesRaster.close();
                        this.ridgeHeightsRaster.close();
                        return true;
                    } catch (OutOfMemoryException) {
                        Utils.loginfo(string.Format("Out of memory for ridge heights by inversion with chunk count {0}", this.chunkCount));
                        this._gv.closeOpenRasters();
                        completed = false;
                        this.chunkCount += 1;
                        if (!this.demRaster.open(this.chunkCount)) {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        
        // 
        //         Create the ridgeHeightsRaster with differences between the elevation at the point and 
        //         the elevation of the nearest ridge cell.
        //         
        //         subbasins is the subbasins raster, distances is the distances to outlet raster, 
        //         slopeDir is the D8 slope directions raster, flowAcc the flow accumulation raster, all four clipped 
        //         the same as the DEM.
        //         valleyDepthsFile is the existing raster  giving the depth of the valley flow below each point (as a positive number of metres).
        //         
        public async Task<bool> calRidgeHeghtsByBranchLength(
            string subbasins,
            string distances,
            string slopeDir,
            string flowAcc,
            string valleyDepthsFile,
            bool mustRun) {
            this.ridgeHeightsFile = Utils.join(this._gv.demDir, "branchheights.tif");
            var distancesFile = Utils.join(this._gv.demDir, "branchdistancess.tif");
            if (mustRun || !Utils.isUpToDate(subbasins, this.ridgeHeightsFile) || !Utils.isUpToDate(distances, this.ridgeHeightsFile) || !Utils.isUpToDate(slopeDir, this.ridgeHeightsFile) || !Utils.isUpToDate(flowAcc, this.ridgeHeightsFile)) {
                this._gv.clearOpenRasters();
                var completed = false;
                while (!completed) {
                    try {
                        completed = true;
                        if (File.Exists(this.ridgeHeightsFile)) {
                            await Utils.removeLayerAndFiles(this.ridgeHeightsFile);
                        }
                        if (File.Exists(distancesFile)) {
                            await Utils.removeLayerAndFiles(distancesFile);
                        }
                        this.ridgeHeightsRaster = new Raster(this.ridgeHeightsFile, this._gv, canWrite: true, isInt: false);
                        var res = this.ridgeHeightsRaster.open(this.chunkCount, numRows: this.numRows, numCols: this.numCols, transform: this.demTransform, projection: this.demProjection, noData: this.noData);
                        if (!res) {
                            return false;
                        }
                        this.ridgeDistancesRaster = new Raster(distancesFile, this._gv, canWrite: true, isInt: false);
                        res = this.ridgeDistancesRaster.open(this.chunkCount, numRows: this.numRows, numCols: this.numCols, transform: this.demTransform, projection: this.demProjection, noData: this.noData);
                        if (!res) {
                            this._gv.closeOpenRasters();
                            return false;
                        }
                        var subbasinsRaster = new Raster(subbasins, this._gv, canWrite: false, isInt: true);
                        res = subbasinsRaster.open(this.chunkCount);
                        if (!res) {
                            this._gv.closeOpenRasters();
                            return false;
                        }
                        var distRaster = new Raster(distances, this._gv, canWrite: false, isInt: false);
                        res = distRaster.open(this.chunkCount);
                        if (!res) {
                            this._gv.closeOpenRasters();
                            return false;
                        }
                        this.findRidges(subbasinsRaster, distRaster);
                        subbasinsRaster.close();
                        distRaster.close();
                        var accRaster = new Raster(flowAcc, this._gv, canWrite: false, isInt: true);
                        res = accRaster.open(this.chunkCount);
                        if (!res) {
                            this._gv.closeOpenRasters();
                            return false;
                        }
                        var dirRaster = new Raster(slopeDir, this._gv, canWrite: false, isInt: true);
                        res = dirRaster.open(this.chunkCount);
                        if (!res) {
                            this._gv.closeOpenRasters();
                            return false;
                        }
                        this.propagateFromRidges(valleyDepthsFile, dirRaster, accRaster);
                        dirRaster.close();
                        accRaster.close();
                        this.ridgeHeightsRaster.close();
                        this.ridgeDistancesRaster.close();
                        return true;
                    } catch (OutOfMemoryException) {
                        Utils.loginfo(string.Format("Out of memory for ridge heights by branch length with chunk count {0}", this.chunkCount));
                        this._gv.closeOpenRasters();
                        completed = false;
                        this.chunkCount += 1;
                        if (!this.demRaster.open(this.chunkCount)) {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        
        // 
        //         Return the positive elevation of the nearest point in accRaster where it is at least threshold,
        //         plus path along d8Raster to that point.
        //         
        //         If a value is found in result, point already processed, return result - elevation for that point.
        //         We are working upslope, final height is ridge height,
        //         ridgeHeights contains ridge - elev, i.e. positive height of ridge above point, 
        //         so if result already exists, ridge is recovered as result + elev
        //         
        public virtual (float, List<(int, int)>) valueAtNearest(
            int row,
            int col,
            Raster d8Raster,
            Raster accRaster,
            int threshold) {
            var path = new List<(int, int)>();
            double accNoData;
            int hasval;
            accRaster.band.GetNoDataValue(out accNoData, out hasval);
            while (true) {
                var res = (float)this.ridgeHeightsRaster.read(row, col);
                if (res >= 0) {
                    // already done this point
                    // eg result is 60, this point's elevation is 100, return value (ridge elevation) is 160
                    return ((float)this.demRaster.read(row, col) + res, path);
                }
                var acc = (int)accRaster.read(row, col);
                if (acc >= threshold || acc == accNoData) {
                    // found nearest stopping point
                    path.Add((row, col));
                    return ((float)this.demRaster.read(row, col), path);
                }
                if (path.Contains((row, col))) {
                    var (startRow, startCol) = path[0];
                    var (startX, startY) = Topology.cellToProj(startCol, startRow, this.demTransform);
                    var (x, y) = Topology.cellToProj(col, row, this.demTransform);
                    Utils.error(string.Format("Loop to ({0}, {1}) from ({2}, {3})", x, y, startX, startY), this._gv.isBatch);
                    return (0, null);
                }
                path.Add((row, col));
                var pt = this.alongDirPoint(row, col, d8Raster).Item1;
                if (pt.Item1 < 0) {
                    // hit edge of clipped area = must be a ridge
                    return ((float)this.demRaster.read(row, col), path);
                }
                (row, col) = pt;
            }
        }
        
        // 
        //         For each point on path, set result to val (ridge elevation) - point elevation.
        public virtual void propagate(float val, List<(int, int)> path) {
            foreach (var (row, col) in path) {
                this.ridgeHeightsRaster.write(row, col, val - (float)this.demRaster.read(row, col));
            }
        }
        
        // Calculate slope positions from valleyDepths and ridgeHeights, and write floodplain raster .
        public async Task writeFloodPlain(string valleyDepthsFile, bool mustRun) {
            int ft;
            ArcGIS.Desktop.Mapping.RasterLayer demLayer;
            var method = this.useInversion ? "inv" : "branch";
            var thresh = string.Format("{0:F2}", this.floodThresh).Replace(".", "_");
            var flood = Utils.join(this._gv.floodDir, method + "flood" + thresh + ".tif");
            if (mustRun || !Utils.isUpToDate(valleyDepthsFile, flood) || !Utils.isUpToDate(this.ridgeHeightsFile, flood)) {
                if (File.Exists(flood)) {
                    await Utils.removeLayerAndFiles(flood);
                }
                this._gv.clearOpenRasters();
                var completed = false;
                while (!completed) {
                    try {
                        completed = true;
                        this.ridgeHeightsRaster = new Raster(this.ridgeHeightsFile, this._gv, canWrite: false, isInt: false);
                        var res = this.ridgeHeightsRaster.open(this.chunkCount);
                        if (!res) {
                            this._gv.closeOpenRasters();
                            return;
                        }
                        if (this.valleyDepthsRaster is null) {
                            // may have been opened when calculating ridge heights by branch length
                            this.valleyDepthsRaster = new Raster(valleyDepthsFile, this._gv, canWrite: false, isInt: false);
                            res = this.valleyDepthsRaster.open(this.chunkCount);
                            if (!res) {
                                this._gv.closeOpenRasters();
                                return;
                            }
                        }
                        this._progress("Flood plain...");
                        this.floodplainRaster = new Raster(flood, this._gv, canWrite: true, isInt: true);
                        var OK = this.floodplainRaster.open(this.chunkCount, numRows: this.numRows, numCols: this.numCols, transform: this.demTransform, projection: this.demProjection, noData: this.noData);
                        if (OK) {
                            this.calcFloodPlain1();
                            this.floodplainRaster.close();
                            Utils.copyPrj(this.demRaster.fileName, this.floodplainRaster.fileName);
                            // load flood above DEM
                            demLayer = Utils.getLayerByLegend(FileTypes.legend(FileTypes._DEM)) as ArcGIS.Desktop.Mapping.RasterLayer;
                            ft = this.useInversion ? FileTypes._INVFLOOD : FileTypes._BRANCHFLOOD;
                            var floodLayer = (await Utils.getLayerByFilename(this.floodplainRaster.fileName, ft, this._gv, demLayer, Utils._WATERSHED_GROUP_NAME)).Item1 as ArcGIS.Desktop.Mapping.RasterLayer;
                            if (floodLayer is null) {
                                Utils.error(string.Format("Failed to load floodplain raster {0}", this.floodplainRaster.fileName), this._gv.isBatch);
                            }
                        }
                        this.valleyDepthsRaster.close();
                        this.ridgeHeightsRaster.close();
                        this._progress("Flood plain done");
                    } catch (OutOfMemoryException) {
                        Utils.loginfo(string.Format("Out of memory for flood plain with chunk count {0}", this.chunkCount));
                        this._gv.closeOpenRasters();
                        completed = false;
                        this.chunkCount += 1;
                    }
                }
            } else {
                // already have uptodate flood file: make sure it is loaded
                // load flood above DEM
                demLayer = Utils.getLayerByLegend(FileTypes.legend(FileTypes._DEM)) as ArcGIS.Desktop.Mapping.RasterLayer;
                ft = this.useInversion ? FileTypes._INVFLOOD : FileTypes._BRANCHFLOOD;
                var floodLayer = (await Utils.getLayerByFilename(flood, ft, this._gv, demLayer, Utils._WATERSHED_GROUP_NAME)).Item1 as ArcGIS.Desktop.Mapping.RasterLayer;
                if (floodLayer is null) {
                    Utils.error(string.Format("Failed to load floodplain raster {0}", this.floodplainRaster.fileName), this._gv.isBatch);
                }
            }
        }
        
        // Calculate the floodplain array elementwise.
        public virtual void calcFloodPlain1() {
            float sp;
            var valleyTransform = new double[6];
            this.valleyDepthsRaster.ds.GetGeoTransform(valleyTransform);
            foreach (var row in Enumerable.Range(0, this.numRows)) {
                foreach (var col in Enumerable.Range(0, this.numCols)) {
                    var rh = (float)this.ridgeHeightsRaster.read(row, col);
                    if (rh != this.noData) {
                        var vd = (float)this.valleyDepthsRaster.read(row, col);
                        if (vd != this.noData) {
                            try {
                                sp = Convert.ToInt32(rh + vd) == 0 ? 0 : vd / (rh + vd);
                            } catch (Exception ex) {
                                Utils.information(string.Format("Problem in calculating slope position at {0}: numerator {1}; denominator {2}: {3}. Set to 0", Topology.cellToProj(col, row, valleyTransform), vd, rh + vd, ex.Message), this._gv.isBatch);
                                sp = 0;
                            }
                            this.floodplainRaster.write(row, col, sp <= this.floodThresh ? 1 : (int)this.noData);
                        }
                    }
                }
            }
        }
        
        // TODO: deal with array access
        //===========================================================================
        // def calcFloodPlain2(self):
        //     """Calculate the floodplain array using numpy array methods."""
        //     for i in xrange(self.chunkCount):
        //         # if there is only one chunk we can avoid reading it again
        //         if i != self.ridgeHeightsRaster.currentIndex:
        //             heightsChunk = self.ridgeHeightsRaster.chunks[i]
        //             self.ridgeHeightsRaster.array = self.ridgeHeightsRaster.band.ReadAsArray(0, heightsChunk.rowOffset, self.ridgeHeightsRaster.numCols, heightsChunk.numRows)
        //             self.ridgeHeightsRaster.currentIndex = i
        //         if i != self.valleyDepthsRaster.currentIndex:
        //             depthsChunk = self.valleyDepthsRaster.chunks[i]
        //             self.valleyDepthsRaster.array = self.valleyDepthsRaster.band.ReadAsArray(0, depthsChunk.rowOffset, self.valleyDepthsRaster.numCols, depthsChunk.numRows)
        //             self.valleyDepthsRaster.currentIndex = i
        //         msk = (self.ridgeHeightsRaster.array != self.noData) & (self.valleyDepthsRaster.array() != self.noData)
        //         # start with array of right size for floodplain (since current size will be last initial chunk which may be small)
        //         # note also we have to initialize it to type float and correct to int later
        //         floodplainChunk = self.floodplainRaster.chunks[i]
        //         self.floodplainRaster.array = np.core.full((floodplainChunk.numRows, self.numCols), self.noData, np.float_)
        //         # the next line uses just one write but does not work
        //         #self.floodplainRaster.array[msk] = 1 if (self.valleyDepthsRaster.array[msk] / (self.valleyDepthsRaster.array[msk] + self.ridgeHeightsRaster.array[msk])) <= self.floodThresh else 0
        //         self.floodplainRaster.array[msk] = self.valleyDepthsRaster.array[msk] / (self.valleyDepthsRaster.array[msk] + self.ridgeHeightsRaster.array[msk])
        //         self.floodplainRaster.array[(self.floodplainRaster.array > self.floodThresh) & (self.ridgeHeightsRaster.array != self.noData) & (self.valleyDepthsRaster.array != self.noData)] = 1
        //         self.floodplainRaster.array[(self.floodplainRaster.array <= self.floodThresh) & (self.ridgeHeightsRaster.array != self.noData) & (self.valleyDepthsRaster.array != self.noData)] = 2
        //         self.floodplainRaster.array[msk] = self.floodplainRaster.array[msk] - 1
        //         self.floodplainRaster.array = self.floodplainRaster.array.astype(np.int_)
        //         self.floodplainRaster.array[self.floodplainRaster.array < 0] = self.noData
        //         self.floodplainRaster.band.WriteArray(self.floodplainRaster.array, 0, floodplainChunk.rowOffset)
        //===========================================================================
        // 
        //         Create ridge points dictionary (row, col) -> (elevation, maximum branch length) for all cells on 
        //         subbasin boundaries.
        //         
        //         A cell is on a boundary if it has one or more adjacent cells in a different subbasin,
        //         or if it has an adjacent cell with nodata for its subbasin, ie it is on watershed boundary,
        //         when its branch length is set to the branch threshold.
        //         The maximum branch length is the maximum of the branch lengths for adjacent cells 
        //         that are in a different subbasin or outside the watershed.
        //         Assumes subbasins, outlet distance and dem rasters have same extents and cell sizes.
        //         
        public virtual void findRidges(Raster subbasinsRaster, Raster distRaster) {
            // check assumption about rasters:
            var subTransform = new double[6];
            subbasinsRaster.ds.GetGeoTransform(subTransform);
            Debug.Assert(this.demTransform == subTransform, "DEM and subbasins rasters not compatible");
            var distTransform = new double[6];
            distRaster.ds.GetGeoTransform(distTransform);
            Debug.Assert(this.demTransform == distTransform, "DEM and distance to outlet rasters not compatible");
            if (this._gv.topo.subbasinToStream.Count == 0) {
                // need to calculate some topology
                string streamFile;
                if (this._gv.useGridModel) {
                    if (!File.Exists(this._gv.delinStreamFile)) {
                        Utils.error("Cannot use branch length algorithm without a delineated stream shapefile", this._gv.isBatch);
                        return;
                    }
                    streamFile = this._gv.delinStreamFile;
                } else {
                    streamFile = this._gv.streamFile;
                }
                if (!this._gv.topo.setUp1(streamFile)) {
                    return;
                }
            }
            this.makeRidges(subbasinsRaster, distRaster);
            var time1 = DateTime.Now;
            this.makeRidgeRaster();
            var time2 = DateTime.Now;
            Utils.loginfo(string.Format("Writing ridge raster  took {0} seconds", time2.Subtract(time1).TotalSeconds));
        }
        
        // 
        //         Set values of ridge points.
        //         
        public virtual void makeRidges(Raster subbasinsRaster, Raster distRaster) {
            int subbasin1;
            var time1 = DateTime.Now;
            this.ridgePoints = new Dictionary<(int, int), (float, double)>();
            double subbasinsNoData;
            int hasval;
            subbasinsRaster.band.GetNoDataValue(out subbasinsNoData, out hasval);
            foreach (var row in Enumerable.Range(0, this.numRows)) {
                foreach (var col in Enumerable.Range(0, this.numCols)) {
                    var subbasin = (int)subbasinsRaster.read(row, col);
                    if (subbasin == subbasinsNoData) {
                        continue;
                    }
                    var maxBranchLength = 0.0;
                    var finished = false;
                    foreach (var dy in new List<int> {
                        -1,
                        0,
                        1
                    }) {
                        var row1 = row + dy;
                        var dxs = dy == 0 ? new List<int> {
                            -1,
                            1
                        } : new List<int> {
                            -1,
                            0,
                            1
                        };
                        foreach (var dx in dxs) {
                            var col1 = col + dx;
                            if (this.pointInMap(row1, col1)) {
                                subbasin1 = (int)subbasinsRaster.read(row + dy, col + dx);
                            } else {
                                subbasin1 = (int)subbasinsNoData;
                            }
                            if (subbasin1 == subbasinsNoData) {
                                maxBranchLength = this.branchThresh;
                                finished = true;
                                break;
                            } else if (subbasin1 != subbasin) {
                                var distanceToJoin = this._gv.topo.getDistanceToJoin(subbasin1, subbasin);
                                var branchLength = (float)distRaster.read(row1, col1) + distanceToJoin;
                                maxBranchLength = Math.Max(branchLength, maxBranchLength);
                            }
                        }
                        if (finished) {
                            break;
                        }
                    }
                    if (maxBranchLength >= this.branchThresh) {
                        var elevation = (float)this.demRaster.read(row, col);
                        if (elevation != this.demNoData) {
                            this.ridgePoints[(row, col)] = (elevation, maxBranchLength);
                        }
                    }
                }
            }
            var time2 = DateTime.Now;
            Utils.loginfo(string.Format("Making ridge points took {0} seconds", time2.Subtract(time1).TotalSeconds));
        }
        
        // 
        //         Find nearest ridge point to (row, col) and return its elevation and distance from (row, col).
        //         
        //         Distance is cartesian distance, in number of pixels (assumed square).
        //         Algorithm looks for first acceptable ridge  point on increasing square perimeter around original point,
        //         as an approximation to the nearest.
        //         Slope position algorithm for floodplain identification only depends on elevations, not distances, 
        //         so we don't worry too much about distances to ridge points, and interpret 'nearest' loosely for efficient execution.
        //         
        public virtual (float, double) getRidgeElevation(int row, int col, bool reportFailure) {
            // use view so that changes may be used later in iteration, and avoid repeated searches for adjacent unacceptable points
            var n = 0;
            while (true) {
                //=============version where square used has N-S and E-W sides=======
                // for dy in xrange(0-n, n+1):
                //     for dx in xrange(0-n, n+1):
                //         if abs(dy) == n or abs(dx) == n: # on boundary of square
                //             (e, l) = self.ridgePoints.get((row+dy, col+dx), (-1, -1))
                //             if l >= 0:
                //                 return e, math.sqrt(dx * dx + dy * dy)
                //===================================================================
                // version with diagonal sides to square - a little closer to circular maybe?
                (float, double) pair;
                float e;
                double l;
                if (n == 0) {
                    if (!this.ridgePoints.TryGetValue((row, col), out pair)) {
                        e = -1;
                        l = -1;
                    } else {
                        (e, l) = pair;
                    }
                    if (l >= 0) {
                        return (e, 0);
                    }
                } else {
                    foreach (var dx in Enumerable.Range(0, n + 1)) {
                        var dy = n - dx;
                        if (!this.ridgePoints.TryGetValue((row + dy, col + dx), out pair)) {
                            e = -1;
                            l = -1;
                        } else {
                            (e, l) = pair;
                        }
                        if (l >= 0) {
                            return (e, Math.Sqrt(dx * dx + dy * dy));
                        }
                        if (dx != 0) {
                            if (!this.ridgePoints.TryGetValue((row + dy, col - dx), out pair)) {
                                e = -1;
                                l = -1;
                            } else {
                                (e, l) = pair;
                            }
                            if (l >= 0) {
                                return (e, Math.Sqrt(dx * dx + dy * dy));
                            }
                        }
                        if (dy != 0) {
                            if (!this.ridgePoints.TryGetValue((row - dy, col + dx), out pair)) {
                                e = -1;
                                l = -1;
                            } else {
                                (e, l) = pair;
                            }
                            if (l >= 0) {
                                return (e, Math.Sqrt(dx * dx + dy * dy));
                            }
                            if (dx != 0) {
                                if (!this.ridgePoints.TryGetValue((row - dy, col - dx), out pair)) {
                                    e = -1;
                                    l = -1;
                                } else {
                                    (e, l) = pair;
                                }
                                if (l >= 0) {
                                    return (e, Math.Sqrt(dx * dx + dy * dy));
                                }
                            }
                        }
                    }
                }
                n += 1;
                if (n > 1000) {
                    if (reportFailure) {
                        var (x, y) = Topology.cellToProj(row, col, this.demTransform);
                        Utils.information(string.Format("No ridge point found within 1000 pixels of ({0}, {1}).  Is the branch threshold too high?", x, y), this._gv.isBatch);
                    }
                    return (0, -1);
                }
            }
        }
        
        // 
        //         Propagate relative heights of nearest ridge points along flow paths, starting from points with accumulation 1.
        //         
        //         Heights are put into ridgeHeightsRaster.  
        //         Distances from ridge are put into ridgeDistancesRaster  If a point already has a height, 
        //         but the new path is shorter, the new height overwrites the old.
        //         
        public virtual void propagateFromRidges(string valleyDepthsFile, Raster dirRaster, Raster accRaster) {
            var time1 = DateTime.Now;
            this.valleyDepthsRaster = new Raster(valleyDepthsFile, this._gv, canWrite: false, isInt: false);
            var res = this.valleyDepthsRaster.open(this.chunkCount);
            if (!res) {
                return;
            }
            var reportFailure = true;
            var pathLength = 0.0;
            var elevation = 0.0;
            var diag = Math.Sqrt(2);
            foreach (var row in Enumerable.Range(0, this.numRows)) {
                foreach (var col in Enumerable.Range(0, this.numCols)) {
                    if ((int)accRaster.read(row, col) == 1) {
                        (elevation, pathLength) = this.getRidgeElevation(row, col, reportFailure);
                        if (pathLength < 0) {
                            if (reportFailure) {
                                reportFailure = false;
                            }
                            elevation = (float)this.demRaster.read(row, col);
                            pathLength = 0;
                        }
                        var nextRow = row;
                        var nextCol = col;
                        while (true) {
                            var nextElev = (float)this.demRaster.read(nextRow, nextCol);
                            if (nextElev == this.demNoData) {
                                break;
                            }
                            var currentPathLength = (float)this.ridgeDistancesRaster.read(nextRow, nextCol);
                            if (currentPathLength >= 0) {
                                // have a previously stored value
                                if (pathLength < currentPathLength) {
                                    // new path length from ridge is shorter: update heights raster 
                                    this.ridgeHeightsRaster.write(nextRow, nextCol, elevation - nextElev);
                                    this.ridgeDistancesRaster.write(nextRow, nextCol, pathLength);
                                } else {
                                    // already had shorter path from ridge - no point in continuing down flow path
                                    break;
                                }
                            } else {
                                // no value stored yet
                                this.ridgeHeightsRaster.write(nextRow, nextCol, elevation - nextElev);
                                this.ridgeDistancesRaster.write(nextRow, nextCol, pathLength);
                            }
                            var (pt, isDiag) = this.alongDirPoint(nextRow, nextCol, dirRaster);
                            if (pt.Item1 < 0) {
                                break;
                            }
                            pathLength += isDiag ? diag : 1;
                            (nextRow, nextCol) = pt;
                        }
                    }
                }
            }
            var time2 = DateTime.Now;
            Utils.loginfo(string.Format("Propagating ridge points took {0} seconds", time2.Subtract(time1).TotalSeconds));
        }
        
        // Make raster  to show ridges.
        public virtual void makeRidgeRaster() {
            var ridgeFile = Utils.join(this._gv.demDir, "ridge" + this.branchThresh.ToString() + ".tif");
            var ridgeRaster = new Raster(ridgeFile, this._gv, canWrite: true, isInt: true);
            var res = ridgeRaster.open(this.chunkCount, numRows: this.numRows, numCols: this.numCols, transform: this.demTransform, projection: this.demProjection, noData: this.noData);
            if (!res) {
                return;
            }
            foreach (var row in Enumerable.Range(0, this.numRows)) {
                foreach (var col in Enumerable.Range(0, this.numCols)) {
                    if ((float)this.demRaster.read(row, col) != this.demNoData) {
                        (float, double) pair;
                        float val;
                        if (!this.ridgePoints.TryGetValue((row, col), out pair)) {
                            val = -1;
                        } else {
                            val = pair.Item1;
                        }
                        ridgeRaster.write(row, col, val == -1 ? 0 : 1);
                    }
                }
            }
            ridgeRaster.close();
            //Utils.getLayerByFilename(self._iface.legendInterface().layers(), ridgeFile, FileTypes._OTHER, 
            //                                self._gv, None, Utils._WATERSHED_GROUP_NAME)
        }
        
        // Return next point (as pair or (-1, -1)) along d8Raster direction from (row, col) that has a data value in demRaster.
        public virtual ((int, int), bool) alongDirPoint(int row, int col, Raster d8Raster) {
            // d8Raster obtained by inverting the d8 flow directions raster has float values, so must cooerce to int
            var dir0 = (int)d8Raster.read(row, col) - 1;
            if (0 <= dir0 && dir0 < 8) {
                var row1 = row + Utils._dY[dir0];
                var col1 = col + Utils._dX[dir0];
                if (this.pointInMap(row1, col1) && (float)this.demRaster.read(row1, col1) != this.demNoData) {
                    var isDiag = dir0 / 2 == 1;
                    return ((row1, col1), isDiag);
                } else {
                    return ((-1, -1), false);
                }
            } else {
                return ((-1, -1), false);
            }
        }
        
        // Return true if row and col are in the limits for the DEM array.
        public virtual bool pointInMap(int row, int col) {
            return 0 <= row && row < this.numRows && (0 <= col && col < this.numCols);
        }
    }
}
