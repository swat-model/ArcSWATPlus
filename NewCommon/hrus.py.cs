


using ArcGIS.Core.Arcade;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data.Analyst3D;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Internal.CIM;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.GeoProcessing;
using ArcGIS.Desktop.Internal.Mapping;
using ArcGIS.Desktop.Internal.Mapping.CommonControls;
using ArcGIS.Desktop.Internal.Mapping.Labeling.Ribbon;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using OSGeo.GDAL;
using OSGeo.OGR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
//using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Printing.IndexedProperties;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml.Linq;
using Path = System.IO.Path;

namespace ArcSWATPlus
{
    public class CreateHRUs
    {

        public DBUtils _db;

        public HRUsForm _dlg;

        public GlobalVars _gv;

        public ComboBox _reportsCombo;

        public int areaVal;

        public Dictionary<int, List<(double, double, double)>> basinElevBands;

        public Dictionary<int, List<int>> basinElevMap;

        public Dictionary<int, BasinData> basins;

        public bool channelMergeByPercent;

        public int channelMergeThreshold;

        public int defaultNoData;

        public List<int> elevMap;

        public bool fullHRUsWanted;

        public int HRUNum;

        public bool isArea;

        public bool isDominantHRU;

        public bool isMultiple;

        public bool isTarget;

        public int landuseVal;

        public Dictionary<int, int> mergedChannels;

        public HashSet<int> mergees;

        public double minElev;

        public int reservoirThreshold;

        public int slopeVal;

        public int soilVal;

        public int targetVal;

        public bool useArea;


        public CreateHRUs(GlobalVars gv, ComboBox reportsCombo, HRUsForm dialog) {
            //# Map of basin number to basin data
            this.basins = new Dictionary<int, BasinData>();
            this._gv = gv;
            this._db = gv.db;
            this._reportsCombo = reportsCombo;
            this._dlg = dialog;
            dialog.gwGroup.Visible = false;
            //# number of HRUs created in watershed
            this.HRUNum = 0;
            //# Minimum elevation in watershed
            this.minElev = 0;
            //# Array of elevation frequencies for whole watershed
            // Index i in array corresponds to elevation elevationGrid.Minimum + i.
            // Used to generate elevation report.
            this.elevMap = new List<int>();
            //# Map from basin number to array of elevation frequencies.
            // Index i in array corresponds to elevation minElev + i.
            // Used to generate elevation report.
            // not used for grid model
            this.basinElevMap = new Dictionary<int, List<int>>();
            //# Map from SWAT basin number to list of (start of band elevation, percent of subbasin area) pairs.
            // List is None if bands not wanted or maximum elevation of subbasin below threshold.
            this.basinElevBands = new Dictionary<int, List<(double, double, double)>>();
            // LSU parameters
            //# Flag indicating short channels to be merged by percent or area
            this.channelMergeByPercent = true;
            //# channel merge threshold
            this.channelMergeThreshold = 0;
            // HRU parameters
            //# Flag indicating multiple/single HRUs per subbasin
            this.isMultiple = false;
            //# For single HRU, flag indicating if dominant
            this.isDominantHRU = true;
            //# Flag indicationg if filtering by area
            this.isArea = false;
            //# Flag indicating if filtering  by target number of HRUs
            this.isTarget = false;
            //# Flag indicating selection by area (else by percentage)
            this.useArea = false;
            //# Current value of area slider
            this.areaVal = 0;
            //# Current value of landuse slider
            this.landuseVal = 0;
            //# Current value of soil slider
            this.soilVal = 0;
            //# Current value of slope slider
            this.slopeVal = 0;
            //# Current value of target slider
            this.targetVal = 0;
            //# Flag indicating if full HRUs file to be generated
            this.fullHRUsWanted = false;
            //# merged channels mapped to channel they were merged with
            this.mergedChannels = new Dictionary<int, int>();
            //# channels merged into (range of self.mergedChannels)
            this.mergees = new HashSet<int>();
            //# threshold (% of lsu area) for water landuse to form a reservoir
            this.reservoirThreshold = 0;
            //# default nodata value used for crop and soil maps with no defined noData value (which would otherwise be None)
            // and for crop and soil maps with nodata values larger in absolute size
            // this value is assumed in the code to be negative
            this.defaultNoData = 1 - (int)Math.Pow(2, 31);
        }

        // Update progress label with message; emit message for display in testing.
        public virtual void progress(string msg) {
            var progressLabel = this._dlg.progressLabel;
            Utils.progress(msg, progressLabel);
        }

        // Generate basin data from watershed, landuse, soil and slope grids.
        //         
        //         Also generate lsus shapefile, and fullHRUs shapefile if requested.
        public async Task<bool> generateBasins(ProgressBar progressBar) {
            double[] transform = new double[6];
            int[] floodData = null;
            int floodCurrentRow = 0;
            int floodReadRows = 0;
            double soilDefinedPercent = 0;
            double landusePercent;
            int hru;
            int slope;
            double slopeValue = 0;
            int slopeCol;
            string cropCode = "";
            bool soilIsNoData;
            int soil;
            int soilCol;
            int crop = 0;
            int cropCol;
            int distStCol;
            int distChCol;
            int floodCol;
            double x;
            int index;
            double elevation;
            int slopeRow;
            int soilRow;
            int cropRow;
            int distStRow = 0;
            int distChRow = 0;
            int floodRow = 0; 
            double y;
            BasinData data;
            //Range colRange;
            //Range rowRange;
            //int elevationTopRow;
            int lastHru = 0;
            int basinReadRows = 1;
            int slopeActReadRows = 0;
            int slopeReadRows;
            int soilActReadRows = 0;
            int soilReadRows;
            int cropActReadRows = 0;
            int cropReadRows;
            int elevationReadRows;
            int distStReadRows = 1;
            int distChReadRows = 1;
            OSGeo.GDAL.Dataset slopeBandsDs = null;
            OSGeo.GDAL.Driver driver;
            string proj;
            bool usePlaya;
            // in case this is a rerun
            this.basins.Clear();
            var elevationDs = Gdal.Open(this._gv.demFile, Access.GA_ReadOnly);
            if (elevationDs is null) {
                Utils.error(string.Format("Cannot open DEM {0}", this._gv.demFile), this._gv.isBatch);
                return false;
            }
            Dataset basinDs = null;
            int basinNumberRows = 0;
            int basinNumberCols = 0;
            int fivePercent = 0;
            double[] basinTransform = new double[6];
            Band basinBand = null;
            double basinNoData = 0;
            int basinHasNoData;
            if (!this._gv.useGridModel) {
                var basinFile = File.Exists(this._gv.chBasinNoLakeFile) ? this._gv.chBasinNoLakeFile : this._gv.channelBasinFile;
                basinDs = Gdal.Open(basinFile, Access.GA_ReadOnly);
                if (basinDs is null) {
                    Utils.error(string.Format("Cannot open watershed raster {0}", basinFile), this._gv.isBatch);
                    return false;
                }
                basinNumberRows = basinDs.RasterYSize;
                basinNumberCols = basinDs.RasterXSize;
                fivePercent = (int)Math.Ceiling(basinNumberRows / 20.0);
                basinDs.GetGeoTransform(basinTransform);
                basinBand = basinDs.GetRasterBand(1);
                basinBand.GetNoDataValue(out basinNoData, out basinHasNoData);
                // TODO is this necessary? appropriate?
                //// can have -128 as the nodata value while map read returns 128
                //basinNoData = Math.Abs(basinNoData);
            }
            Dataset distStDs = null;
            Dataset distChDs = null;
            if (!this._gv.existingWshed) {
                distStDs = Gdal.Open(this._gv.distStFile, Access.GA_ReadOnly);
                if (distStDs is null) {
                    Utils.error(string.Format("Cannot open distance to outlets file {0}", this._gv.distStFile), this._gv.isBatch);
                    return false;
                }
                if (!this._gv.useGridModel) {
                    distChDs = Gdal.Open(this._gv.distChFile, Access.GA_ReadOnly);
                    if (distChDs is null) {
                        Utils.error(string.Format("Cannot open distance to channel file {0}", this._gv.distChFile), this._gv.isBatch);
                        return false;
                    }
                }
            }
            var cropDs = Gdal.Open(this._gv.landuseFile, Access.GA_ReadOnly);
            if (cropDs is null) {
                Utils.error(string.Format("Cannot open landuse file {0}", this._gv.landuseFile), this._gv.isBatch);
                return false;
            }
            var soilDs = Gdal.Open(this._gv.soilFile, Access.GA_ReadOnly);
            if (soilDs is null) {
                Utils.error(string.Format("Cannot open soil file {0}", this._gv.soilFile), this._gv.isBatch);
                return false;
            }
            var slopeDs = Gdal.Open(this._gv.slopeFile, Access.GA_ReadOnly);
            if (slopeDs is null) {
                Utils.error(string.Format("Cannot open slope file {0}", this._gv.slopeFile), this._gv.isBatch);
                return false;
            }
            Dataset floodDs = null;
            if (this._gv.useLandscapes) {
                floodDs = Gdal.Open(this._gv.floodFile, Access.GA_ReadOnly);
                if (floodDs is null) {
                    Utils.error(string.Format("Cannot open floodplain file {0}", this._gv.floodFile), this._gv.isBatch);
                    return false;
                }
            }
            Dataset wpDs = null;
            if (this._gv.playaFile != "") {
                wpDs = Gdal.Open(this._gv.playaFile, Access.GA_ReadOnly);
                if (wpDs is null) {
                    Utils.error(string.Format("Cannot open wetlandPlaya file {0}", this._gv.playaFile), this._gv.isBatch);
                    return false;
                }
                usePlaya = true;
            } else {
                usePlaya = false;
            }
            // Loop reading grids is MUCH slower if these are not stored locally
            int distStNumberRows = 0;
            int distStNumberCols = 0;
            int distChNumberRows = 0;
            int distChNumberCols = 0;
            if (!this._gv.existingWshed) {
                distStNumberRows = distStDs.RasterYSize;
                distStNumberCols = distStDs.RasterXSize;
                if (!this._gv.useGridModel) {
                    distChNumberRows = distChDs.RasterYSize;
                    distChNumberCols = distChDs.RasterXSize;
                }
            }
            var cropNumberRows = cropDs.RasterYSize;
            var cropNumberCols = cropDs.RasterXSize;
            var soilNumberRows = soilDs.RasterYSize;
            var soilNumberCols = soilDs.RasterXSize;
            var slopeNumberRows = slopeDs.RasterYSize;
            var slopeNumberCols = slopeDs.RasterXSize;
            var elevationNumberRows = elevationDs.RasterYSize;
            var elevationNumberCols = elevationDs.RasterXSize;
            int floodNumberRows = 0;
            int floodNumberCols = 0;
            if (this._gv.useLandscapes) {
                floodNumberRows = floodDs.RasterYSize;
                floodNumberCols = floodDs.RasterXSize;
            }
            double[] distStTransform = new double[6];
            double[] distChTransform = new double[6];
            if (!this._gv.existingWshed) {
                distStDs.GetGeoTransform(distStTransform);
                if (!this._gv.useGridModel) {
                    distChDs.GetGeoTransform(distChTransform);
                }
            }
            double[] cropTransform = new double[6];
            double[] soilTransform = new double[6];
            double[] slopeTransform = new double[6];
            double[] elevationTransform = new double[6];
            double[] floodTransform = new double[6];
            cropDs.GetGeoTransform(cropTransform);
            soilDs.GetGeoTransform(soilTransform);
            slopeDs.GetGeoTransform(slopeTransform);
            elevationDs.GetGeoTransform(elevationTransform);
            if (this._gv.useLandscapes) {
                floodDs.GetGeoTransform(floodTransform);
            }
            // if grids have same coords we can use (col, row) from one in another
            Func<int, double, int> cropRowFun;
            Func<int, double, int> cropColFun;
            Func<int, double, int> soilRowFun;
            Func<int, double, int> soilColFun;
            Func<int, double, int> slopeRowFun;
            Func<int, double, int> slopeColFun;
            Func<int, double, int> floodRowFun = null;
            Func<int, double, int> floodColFun = null;
            Func<int, double, int> distStRowFun = null;
            Func<int, double, int> distStColFun = null;
            Func<int, double, int> distChRowFun = null;
            Func<int, double, int> distChColFun = null;
            Func<int, double, int> elevationRowFun = null;
            Func<int, double, int> elevationColFun = null;
            if (this._gv.useGridModel) {
                (cropRowFun, cropColFun) = Topology.translateCoords(elevationTransform, cropTransform, elevationNumberRows, elevationNumberCols);
                (soilRowFun, soilColFun) = Topology.translateCoords(elevationTransform, soilTransform, elevationNumberRows, elevationNumberCols);
                (slopeRowFun, slopeColFun) = Topology.translateCoords(elevationTransform, slopeTransform, elevationNumberRows, elevationNumberCols);
                if (this._gv.useLandscapes) {
                    (floodRowFun, floodColFun) = Topology.translateCoords(elevationTransform, floodTransform, elevationNumberRows, elevationNumberCols);
                }
                if (!this._gv.existingWshed) {
                    (distStRowFun, distStColFun) = Topology.translateCoords(elevationTransform, distStTransform, elevationNumberRows, elevationNumberCols);
                }
            } else {
                if (!this._gv.existingWshed) {
                    (distStRowFun, distStColFun) = Topology.translateCoords(basinTransform, distStTransform, basinNumberRows, basinNumberCols);
                    (distChRowFun, distChColFun) = Topology.translateCoords(basinTransform, distChTransform, basinNumberRows, basinNumberCols);
                }
                (cropRowFun, cropColFun) = Topology.translateCoords(basinTransform, cropTransform, basinNumberRows, basinNumberCols);
                (soilRowFun, soilColFun) = Topology.translateCoords(basinTransform, soilTransform, basinNumberRows, basinNumberCols);
                (slopeRowFun, slopeColFun) = Topology.translateCoords(basinTransform, slopeTransform, basinNumberRows, basinNumberCols);
                (elevationRowFun, elevationColFun) = Topology.translateCoords(basinTransform, elevationTransform, basinNumberRows, basinNumberCols);
                if (this._gv.useLandscapes) {
                    (floodRowFun, floodColFun) = Topology.translateCoords(basinTransform, floodTransform, basinNumberRows, basinNumberCols);
                }
            }
            Band distStBand = null;
            Band distChBand = null;
            if (!this._gv.existingWshed) {
                distStBand = distStDs.GetRasterBand(1);
                if (!this._gv.useGridModel) {
                    distChBand = distChDs.GetRasterBand(1);
                }
            }
            var cropBand = cropDs.GetRasterBand(1);
            var soilBand = soilDs.GetRasterBand(1);
            var slopeBand = slopeDs.GetRasterBand(1);
            var elevationBand = elevationDs.GetRasterBand(1);
            Band floodBand = null;
            Band wpBand = null;
            if (this._gv.useLandscapes) {
                floodBand = floodDs.GetRasterBand(1);
            }
            if (usePlaya) {
                wpBand = wpDs.GetRasterBand(1);
            }
            int hasval;
            double elevationNoData;
            elevationBand.GetNoDataValue(out elevationNoData, out hasval);
            // guard against DEM with no nodata value
            if (hasval == 0) {
                elevationNoData = this.defaultNoData;
            }
            var distStNoData = elevationNoData;
            var distChNoData = elevationNoData;
            if (!this._gv.existingWshed) {
                distStBand.GetNoDataValue(out distStNoData, out hasval);
                if (!this._gv.useGridModel) {
                    distChBand.GetNoDataValue(out distChNoData, out hasval);
                }
            }
            double cropNoData;
            Func<double?, bool> cropIsNoDataFun = null;
            cropBand.GetNoDataValue(out cropNoData, out hasval);
            if (hasval == 0) {
                cropNoData = this.defaultNoData;
            // guard against floats that cannot be converted to int
            } else if (cropNoData < this.defaultNoData) {   // large negative
                cropNoData = this.defaultNoData;
                cropIsNoDataFun = x => x is null || Double.IsNaN((double)x) || x <= this.defaultNoData;
            } else if (cropNoData > 0 - this.defaultNoData) { // large positive
                cropNoData = this.defaultNoData;
                cropIsNoDataFun = x => x is null || Double.IsNaN((double)x) || x >= 0 - this.defaultNoData;
            } else {
                cropIsNoDataFun = x => x is null || Double.IsNaN((double)x) || x == cropNoData;
            }
            double soilNoData;
            Func<double?, bool> soilIsNoDataFun = null;
            soilBand.GetNoDataValue(out soilNoData, out hasval);
            if (hasval == 0) {
                soilNoData = this.defaultNoData;
            // guard against floats that cannot be converted to int
            } else if (soilNoData < this.defaultNoData) {   // large negative
                soilNoData = this.defaultNoData;
                soilIsNoDataFun = x => x is null || Double.IsNaN((double)x) || x <= this.defaultNoData;
            } else if (soilNoData > 0 - this.defaultNoData) { // large positive
                soilNoData = this.defaultNoData;
                soilIsNoDataFun = x => x is null || Double.IsNaN((double)x) || x >= 0 - this.defaultNoData;
            } else {
                soilIsNoDataFun = x => x is null || Double.IsNaN((double)x) || x == soilNoData;
            }
            if (this._gv.isHUC || this._gv.isHAWQS) {
                this._gv.db.SSURGOUndefined = (int)soilNoData;
            }
            double slopeNoData;
            slopeBand.GetNoDataValue(out slopeNoData, out hasval);
            if (!this._gv.useGridModel) {
                this._gv.basinNoData = (int)basinNoData;
            }
            this._gv.distStNoData = distStNoData;
            this._gv.distChNoData = distChNoData;
            this._gv.cropNoData = (int)cropNoData;
            this._gv.soilNoData = (int)soilNoData;
            this._gv.slopeNoData = (int)slopeNoData;
            this._gv.elevationNoData = elevationNoData;
            double floodNoData = -1;
            if (this._gv.useLandscapes) {
                floodBand.GetNoDataValue(out floodNoData, out hasval);
                this._gv.floodNoData = (int)floodNoData;
            }
            double wpNoData = -1;
            if (usePlaya) {
                wpBand.GetNoDataValue(out wpNoData, out hasval);
            }
            var streamGeoms = new Dictionary<int, OSGeo.OGR.Geometry>();
            var basinStreamWaterData = new Dictionary<int, (OSGeo.OGR.Geometry, double, double)>();
            // TODO
            //if (this._gv.isHUC || this._gv.isHAWQS) {
            //    this._gv.db.SSURGOUndefined = (int)soilNoData;
            //    // collect data basin -> stream buffer shape, buffer area, WATR area 
            //    // map basin -> stream geometry
            //    var streamDs = Ogr.Open(this._gv.streamFile, 0);
            //    var streamLayer = streamDs.GetLayerByIndex(0);
            //    var linkIndex = this._gv.topo.getIndex(streamLayer, Topology._LINKNO);
            //    for (int i = 0; i < streamLayer.GetFeatureCount(1); i++) {
            //        var stream = streamLayer.GetFeature(i);
            //        link = stream.GetFieldAsInteger(linkIndex);
            //        basin = this._gv.topo.linkToBasin[link];
            //        streamGeoms[basin] = stream.GetGeometryRef();
            //    }
            //}
            // counts to calculate landuse and soil overlaps
            var landuseCount = 0.0;
            var landuseNoDataCount = 0.0;
            var soilDefinedCount = 0.0;
            var soilUndefinedCount = 0.0;
            var soilNoDataCount = 0.0;
            var slopeBandsNoData = -1;
            Band slopeBandsBand = null;
            // prepare slope bands grid
            // remove old one since may not be wanted: will be recalculated if it is needed
            this._gv.slopeBandsFile = System.IO.Path.ChangeExtension(this._gv.demFile, null) + "slope_bands.tif";
            await Utils.removeLayerAndFiles(this._gv.slopeBandsFile);
            if (this._gv.db.slopeLimits.Count > 0) {
                proj = slopeDs.GetProjection();
                driver = Gdal.GetDriverByName("GTiff");
                if (File.Exists(this._gv.slopeBandsFile)) {
                    // failed to remove it - already open in ArcGIS
                    slopeBandsDs = Gdal.Open(this._gv.slopeBandsFile, Access.GA_Update);
                } else {
                    slopeBandsDs = driver.Create(this._gv.slopeBandsFile, slopeNumberCols, slopeNumberRows, 1, DataType.GDT_Byte, null);
                }
                slopeBandsBand = slopeBandsDs.GetRasterBand(1);
                slopeBandsBand.SetNoDataValue(slopeBandsNoData);
                slopeBandsDs.SetGeoTransform(slopeTransform);
                slopeBandsDs.SetProjection(proj);
                Utils.copyPrj(this._gv.slopeFile, this._gv.slopeBandsFile);
            }
            int hasMinVal;
            elevationBand.GetMinimum(out this.minElev, out hasMinVal);
            double maxElev;
            int hasMaxVal;
            elevationBand.GetMaximum(out maxElev, out hasMaxVal);
            if (hasMinVal == 0 || hasMaxVal == 0) {
                try {
                    double[] extrema = new double[2];
                    elevationBand.ComputeRasterMinMax(extrema, 1);
                    this.minElev = extrema[0];
                    maxElev = extrema[1];
                }
                catch {
                    Utils.error("Failed to calculate Math.Min/Math.Max values of your DEM.  Is it too small?", this._gv.isBatch);
                    return false;
                }
            }
            // convert to metres
            this.minElev *= this._gv.verticalFactor;
            // avoid rounding errors
            this.minElev -= 10;
            maxElev *= this._gv.verticalFactor;
            // have seen minInt for minElev, so let's play safe
            // else can get absurdly large list of elevations
            var globalMinElev = -419;
            var globalMaxElev = 8849;
            this.minElev = Math.Max(Convert.ToInt32(this.minElev), globalMinElev);
            maxElev = Math.Min(Convert.ToInt32(maxElev), globalMaxElev);
            var elevMapSize = Convert.ToInt32(1 + maxElev - this.minElev);
            this.elevMap = new List<int>();
            for (var i = 0; i < elevMapSize; i++) {
                this.elevMap.Add(0);
            }
            int distStActReadRows = 0;
            int floodActReadRows = 0;
            // We read raster data in complete rows, using several rows for the grid model if necessary.
            // Complete rows should be reasonably efficient, and for the grid model
            // reading all rows necessary for each row of grid cells avoids rereading any row
            // create empty arrays to hold raster data when read
            // to avoid danger of allocating and deallocating with main loop
            // currentRow is the top row when using grid model
            var basinCurrentRow = -1;
            int[] basinData = new int[basinReadRows * basinNumberCols];
            var distChCurrentRow = -1;
            double[] distChData = null;
            if (!this._gv.existingWshed) {
                distChData = new double[distChReadRows * distChNumberCols];
            }
            if (this._gv.useLandscapes) {
                floodCurrentRow = -1;
                floodData = new int[floodReadRows * floodNumberCols];
            }
            double diag = 0;
            double halfDiag = 0;
            if (this._gv.useGridModel) {
                elevationReadRows = this._gv.gridSize;
                // estimate of maximum distance to outlet
                diag = Math.Sqrt(this._gv.topo.dx * this._gv.topo.dx + this._gv.topo.dy * this._gv.topo.dy) * this._gv.gridSize;
                // estimate of maximum distance to channel
                halfDiag = diag / 2.0;
                var elevationRowDepth = elevationReadRows * elevationTransform[5];
                // we add an extra 2 rows since edges of rows may not
                // line up with elevation map.
                cropReadRows = Math.Max(1, Convert.ToInt32(elevationRowDepth / cropTransform[5] + 2));
                cropActReadRows = cropReadRows;
                soilReadRows = Math.Max(1, Convert.ToInt32(elevationRowDepth / soilTransform[5] + 2));
                soilActReadRows = soilReadRows;
                slopeReadRows = Math.Max(1, Convert.ToInt32(elevationRowDepth / slopeTransform[5] + 2));
                slopeActReadRows = slopeReadRows;
                if (!this._gv.existingWshed) {
                    distStReadRows = Math.Max(1, Convert.ToInt32(elevationRowDepth / distStTransform[5] + 2));
                } else {
                    distStReadRows = 0;
                }
                distStActReadRows = distStReadRows;
                if (this._gv.useLandscapes) {
                    floodReadRows = Math.Max(1, Convert.ToInt32(elevationRowDepth / floodTransform[5] + 2));
                } else {
                    floodReadRows = 0;
                }
                floodActReadRows = floodReadRows;
                basinReadRows = 1;
                Utils.loginfo(string.Format("{0}, {1}, {2}, {3}, {4} rows of landuse, soil, slope, distance to stream and flood for each grid cell", cropReadRows, soilReadRows, slopeReadRows, distStReadRows, floodReadRows));
            } else {
                elevationReadRows = 1;
                cropReadRows = 1;
                soilReadRows = 1;
                slopeReadRows = 1;
                basinReadRows = 1;
                if (!this._gv.existingWshed) {
                    distStReadRows = 1;
                    distChReadRows = 1;
                }
                if (this._gv.useLandscapes) {
                    floodReadRows = 1;
                }
            }
            // last HRU number used
            lastHru = 0;
            int[] lsuRows = null;
            int[] lsuRow = null;
            Polygonize lsuShapes = null;
            int[] hruRows = null;
            int[] hruRow = null;
            Polygonize hruShapes = null;
            Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, int>>>>>> subbasinChannelLandscapeCropSoilSlopeNumbers = null;
            if (this._gv.useLandscapes || this.fullHRUsWanted) {
                subbasinChannelLandscapeCropSoilSlopeNumbers =
                    new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, int>>>>>>();
                if (this._gv.useLandscapes) {
                    // use LSU ids to make LSU polygon shapefile (otherwise can use subbasins)
                    // grid models are based on the DEM raster, and non-grid models on the basins grid
                    if (this._gv.useGridModel) {
                        transform = elevationTransform;
                        lsuRows = new int[elevationReadRows * elevationNumberCols];
                    } else {
                        transform = basinTransform;
                        lsuRow = new int[basinNumberCols];
                    }
                    MapPoint p = MapPointBuilderEx.CreateMapPoint(transform[0], transform[3]);
                    lsuShapes = new Polygonize(true, this._gv.useGridModel ? elevationNumberCols : basinNumberCols, -1, p, transform[1], Math.Abs(transform[5]));
                }
                if (this.fullHRUsWanted) {
                    // grid models are based on the DEM raster, and non-grid models on the basins grid
                    if (this._gv.useGridModel) {
                        transform = elevationTransform;
                        hruRows = new int[elevationReadRows * elevationNumberCols];
                    } else {
                        transform = basinTransform;
                        hruRow = new int[basinNumberCols];
                    }
                    MapPoint p = MapPointBuilderEx.CreateMapPoint(transform[0], transform[3]);
                    hruShapes = new Polygonize(true, this._gv.useGridModel ? elevationNumberCols : basinNumberCols, -1, p, transform[1], Math.Abs(transform[5]));
                }
            }
            var cropCurrentRow = -1;
            var cropData = new int[cropReadRows * cropNumberCols];
            var soilCurrentRow = -1;
            var soilData = new int[soilNumberCols * soilReadRows];
            var slopeCurrentRow = -1;
            var slopeData = new double[slopeNumberCols * slopeReadRows];
            var elevationCurrentRow = -1;
            var elevationData = new double[elevationNumberCols * elevationReadRows];
            var distStCurrentRow = -1;
            float[] distStData = null;
            if (!this._gv.existingWshed) {
                distStData = new float[distStReadRows * distStNumberCols];
            }
            if (this._gv.useLandscapes) {
                floodCurrentRow = -1;
                floodData = new int[floodReadRows * floodNumberCols];
            }
            int[] wpData = null;
            // wetlandPlaya raster has same size and origin as elevation raster
            if (usePlaya) {
                wpData = new int[elevationReadRows * elevationNumberCols];
            }
            var progressCount = 0;
            if (this._gv.useGridModel) {
                var time1 = DateTime.Now;
                fivePercent = (int)Math.Ceiling(this._gv.topo.subbasinToSWATBasin.Count / 20.0);
                var gridCount = 0;
                foreach (var (chLink, chBasin) in this._gv.topo.chLinkToChBasin) {
                    if (this._gv.topo.chLinkInsideLake.ContainsKey(chLink)) {
                        continue;
                    }
                    // since this is for grid models
                    int subbasin = chBasin;
                    // Set waterId to lake id if this basin is an exit from a lake.
                    // We need to preserve such cells in the basins map even though they are part of lakes
                    // since their subbasin references appear as the subbasins of the outflowing channels
                    // and, for the main outlet, as the subbasin of the water point.
                    // The positive waterId field identifies them as part of the lake.
                    int waterId;
                    if (!this._gv.topo.chLinkFromLake.TryGetValue(chLink, out waterId)) {
                        waterId = 0;
                    }
                    if (waterId > 0) {
                        data = new BasinData(this._db.waterLanduse, 0, waterId);
                        this.basins[subbasin] = data;
                        continue;
                    }
                    int SWATBasin;
                    if (!this._gv.topo.subbasinToSWATBasin.TryGetValue(chBasin, out SWATBasin)) {
                        continue;
                    }
                    if (progressCount == fivePercent) {
                        progressBar.Value = progressBar.Value + 5;
                        progressCount = 1;
                    } else {
                        progressCount += 1;
                    }
                    gridCount += 1;
                    // centroid was taken from accumulation grid, but does not matter since in projected units
                    var centre = this._gv.topo.basinCentroids[chBasin];
                    var centreX = centre[0];
                    var centreY = centre[1];
                    var n = elevationReadRows;
                    int centreCol;
                    int centreRow;
                    int elevationTopRow;
                    IEnumerable<int> rowRange;
                    IEnumerable<int> colRange;
                    // each grid subbasin contains n x n DEM cells
                    if (n % 2 == 0) {
                        // even number of rows and columns - start half a row and column NW of centre
                        (centreCol, centreRow) = Topology.projToCell(centreX - elevationTransform[1] / 2.0, centreY - elevationTransform[5] / 2.0, elevationTransform);
                        elevationTopRow = centreRow - (n - 2) / 2;
                        // beware of rows or columns not dividing by n:
                        // last grid row or column may be short
                        rowRange = Enumerable.Range(elevationTopRow, Math.Min(centreRow + (n + 2) / 2, elevationNumberRows) - elevationTopRow);
                        colRange = Enumerable.Range(centreCol - (n - 2) / 2, Math.Min(centreCol + (n + 2) / 2, elevationNumberCols) - (centreCol - (n - 2) / 2));
                    } else {
                        // odd number of rows and columns
                        (centreCol, centreRow) = Topology.projToCell(centreX, centreY, elevationTransform);
                        elevationTopRow = centreRow - (n - 1) / 2;
                        // beware of rows or columns not dividing by n:
                        // last grid row or column may be short
                        rowRange = Enumerable.Range(elevationTopRow, Math.Min(centreRow + (n + 1) / 2, elevationNumberRows) - elevationTopRow);
                        colRange = Enumerable.Range(centreCol - (n - 1) / 2, Math.Min(centreCol + (n + 1) / 2, elevationNumberCols) - (centreCol - (n - 1) / 2));
                    }
                    data = new BasinData(this._db.waterLanduse, -1, 0);
                    // read data if necessary
                    if (elevationTopRow != elevationCurrentRow) {
                        if (this._gv.useLandscapes) {
                            foreach (var rowNum in Enumerable.Range(0, n)) {
                                int start = rowNum * elevationNumberCols;
                                lsuShapes.addRow(lsuRows, start, elevationCurrentRow + rowNum);
                                Array.Fill(lsuRows, -1, start, elevationNumberCols);
                            }
                            //lsuRows.fill(-1);
                        }
                        if (this.fullHRUsWanted && lastHru > 0) {
                            // something has been written to hruRows
                            foreach (var rowNum in Enumerable.Range(0, n)) {
                                int start = rowNum * elevationNumberCols;
                                hruShapes.addRow(hruRows, start, elevationCurrentRow + rowNum);
                                Array.Fill(hruRows, -1, start, elevationNumberCols);
                            }
                            //hruRows.fill(-1);
                        }
                        elevationBand.ReadRaster(0, elevationTopRow, elevationNumberCols, Math.Min(elevationReadRows, elevationNumberRows - elevationTopRow), elevationData, elevationNumberCols, elevationReadRows, 0, 0);
                        if (usePlaya) {
                            wpBand.ReadRaster(0, elevationTopRow, elevationNumberCols, Math.Min(elevationReadRows, elevationNumberRows - elevationTopRow), wpData, elevationNumberCols, elevationReadRows, 0, 0);
                        }
                        elevationCurrentRow = elevationTopRow;
                    }
                    var topY = Topology.rowToY(elevationTopRow, elevationTransform);
                    var cropTopRow = cropRowFun(elevationTopRow, topY);
                    if (cropTopRow != cropCurrentRow) {
                        if (0 <= cropTopRow && cropTopRow <= cropNumberRows - cropReadRows) {
                            cropBand.ReadRaster(0, cropTopRow, cropNumberCols, cropReadRows, cropData, cropNumberCols, cropReadRows, 0, 0);
                            cropActReadRows = cropReadRows;
                            cropCurrentRow = cropTopRow;
                        } else if (cropNumberRows - cropTopRow < cropReadRows) {
                            // runnning off the bottom of crop map
                            cropActReadRows = cropNumberRows - cropTopRow;
                            if (cropActReadRows >= 1) {
                                cropBand.ReadRaster(0, cropTopRow, cropNumberCols, cropActReadRows, cropData, cropNumberCols, cropReadRows, 0, 0);
                                cropCurrentRow = cropTopRow;
                            }
                        } else {
                            cropActReadRows = 0;
                        }
                    }
                    var soilTopRow = soilRowFun(elevationTopRow, topY);
                    if (soilTopRow != soilCurrentRow) {
                        if (0 <= soilTopRow && soilTopRow <= soilNumberRows - soilReadRows) {
                            soilBand.ReadRaster(0, soilTopRow, soilNumberCols, soilReadRows, soilData, soilNumberCols, soilReadRows, 0, 0);
                            soilActReadRows = soilReadRows;
                            soilCurrentRow = soilTopRow;
                        } else if (soilNumberRows - soilTopRow < soilReadRows) {
                            // runnning off the bottom of soil map
                            soilActReadRows = soilNumberRows - soilTopRow;
                            if (soilActReadRows >= 1) {
                                soilBand.ReadRaster(0, soilTopRow, soilNumberCols, soilActReadRows, soilData, soilNumberCols, soilReadRows, 0, 0);
                                soilCurrentRow = soilTopRow;
                            }
                        } else {
                            soilActReadRows = 0;
                        }
                    }
                    var slopeTopRow = slopeRowFun(elevationTopRow, topY);
                    if (slopeTopRow != slopeCurrentRow) {
                        if (0 <= slopeTopRow && slopeTopRow <= slopeNumberRows - slopeReadRows) {
                            slopeBand.ReadRaster(0, slopeTopRow, slopeNumberCols, slopeReadRows, slopeData, slopeNumberCols, slopeReadRows, 0, 0);
                            slopeActReadRows = slopeReadRows;
                            slopeCurrentRow = slopeTopRow;
                        } else if (slopeNumberRows - slopeTopRow < slopeReadRows) {
                            // runnning off the bottom of slope map
                            slopeActReadRows = slopeNumberRows - slopeTopRow;
                            if (slopeActReadRows >= 1) {
                                slopeBand.ReadRaster(0, slopeTopRow, slopeNumberCols, slopeActReadRows, slopeData, slopeNumberCols, slopeReadRows, 0, 0);
                                slopeCurrentRow = slopeTopRow;
                            }
                        } else {
                            slopeActReadRows = 0;
                        }
                    }
                    var distStTopRow = 0;
                    if (!this._gv.existingWshed) {
                        distStTopRow = distStRowFun(elevationTopRow, topY);
                        if (distStTopRow != distStCurrentRow) {
                            if (0 <= distStTopRow && distStTopRow <= distStNumberRows - distStReadRows) {
                                distStBand.ReadRaster(0, distStTopRow, distStNumberCols, distStReadRows, distStData, distStNumberCols, distStReadRows, 0, 0);
                                distStActReadRows = distStReadRows;
                                distStCurrentRow = distStTopRow;
                            } else if (distStNumberRows - distStTopRow < distStReadRows) {
                                // runnning off the bottom of distSt map
                                distStActReadRows = distStNumberRows - distStTopRow;
                                if (distStActReadRows >= 1) {
                                    distStBand.ReadRaster(0, distStTopRow, distStNumberCols, distStActReadRows, distStData, distStNumberCols, distStReadRows, 0, 0);
                                    distStCurrentRow = distStTopRow;
                                }
                            } else {
                                distStActReadRows = 0;
                            }
                        }
                    }
                    var floodTopRow = 0;
                    if (this._gv.useLandscapes) {
                        floodTopRow = floodRowFun(elevationTopRow, topY);
                        if (floodTopRow != floodCurrentRow) {
                            if (0 <= floodTopRow && floodTopRow <= floodNumberRows - floodReadRows) {
                                floodBand.ReadRaster(0, floodTopRow, floodNumberCols, floodReadRows, floodData, floodNumberCols, floodReadRows, 0, 0);
                                floodActReadRows = floodReadRows;
                                floodCurrentRow = floodTopRow;
                            } else if (floodNumberRows - floodTopRow < floodReadRows) {
                                // runnning off the bottom of flood map
                                floodActReadRows = floodNumberRows - floodTopRow;
                                if (floodActReadRows >= 1) {
                                    floodBand.ReadRaster(0, floodTopRow, floodNumberCols, floodActReadRows, floodData, floodNumberCols, floodReadRows, 0, 0);
                                    floodCurrentRow = floodTopRow;
                                }
                            } else {
                                floodActReadRows = 0;
                            }
                        }
                    }
                    foreach (var row in rowRange) {
                        y = Topology.rowToY(row, elevationTransform);
                        cropRow = cropRowFun(row, y);
                        soilRow = soilRowFun(row, y);
                        slopeRow = slopeRowFun(row, y);
                        distStRow = 0;
                        if (!this._gv.existingWshed) {
                            distStRow = distStRowFun(row, y);
                        }
                        floodRow = 0;
                        if (this._gv.useLandscapes) {
                            floodRow = floodRowFun(row, y);
                        }
                        foreach (var col in colRange) {
                            elevation = (float)elevationData[(row - elevationTopRow) * elevationNumberCols + col];
                            int elev;
                            if (Utils.areClose(elevation, elevationNoData, rel_tol: 1E-06)) {
                                elev = (int)elevationNoData;
                            } else {
                                elev = Convert.ToInt32(elevation * this._gv.verticalFactor);
                                index = elev - (int)this.minElev;
                                // can have index too large because Math.Max not calculated properly by gdal
                                if (index >= elevMapSize) {
                                    for (int i = elevMapSize; i <= index; i++) {
                                        this.elevMap.Add(0);
                                    }
                                    elevMapSize = index + 1;
                                }
                                this.elevMap[index] += 1;
                            }
                            Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, int>>>>> channelLandscapeCropSoilSlopeNumbers = null;
                            if (this._gv.useLandscapes || this.fullHRUsWanted) {
                                if (!subbasinChannelLandscapeCropSoilSlopeNumbers.TryGetValue(subbasin, out channelLandscapeCropSoilSlopeNumbers)) {
                                    channelLandscapeCropSoilSlopeNumbers = new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, int>>>>>();
                                }
                                subbasinChannelLandscapeCropSoilSlopeNumbers[subbasin] = channelLandscapeCropSoilSlopeNumbers;
                            }
                            x = Topology.colToX(col, elevationTransform);
                            crop = -1;
                            if (usePlaya) {
                                var wpVal = (int)wpData[(row - elevationTopRow) * elevationNumberCols + col];
                                if (wpVal == (int)WaterRole._PLAYA) {
                                    crop = this._gv.db.playaCrop;
                                }
                            }
                            if (crop < 0) {
                                var _tmp_1 = cropRow - cropTopRow;
                                if (0 <= _tmp_1 && _tmp_1 < cropActReadRows) {
                                    cropCol = cropColFun(col, x);
                                    if (0 <= cropCol && cropCol < cropNumberCols) {
                                        crop = (int)cropData[(cropRow - cropTopRow) * cropNumberCols + cropCol];
                                    } else {
                                        crop = (int)cropNoData;
                                    }
                                } else {
                                    crop = (int)cropNoData;
                                }
                            }
                            // no data read from map is Nan if no noData value defined 
                            if (cropIsNoDataFun(crop)) {
                                // use <= since may have been set to defaultNoData
                                landuseNoDataCount += 1;
                                // when using grid model small amounts of
                                // no data for crop, soil or slope could lose subbasin
                                crop = this._db.defaultLanduse;
                            } else {
                                landuseCount += 1;
                            }
                            // use an equivalent landuse if any
                            crop = this._db.translateLanduse(Convert.ToInt32(crop));
                            var _tmp_2 = soilRow - soilTopRow;
                            if (0 <= _tmp_2 && _tmp_2 < soilActReadRows) {
                                soilCol = soilColFun(col, x);
                                if (0 <= soilCol && soilCol < soilNumberCols) {
                                    soil = (int)soilData[(soilRow - soilTopRow) * soilNumberCols + soilCol];
                                } else {
                                    soil = (int)soilNoData;
                                }
                            } else {
                                soil = (int)soilNoData;
                            }
                            if (soilIsNoDataFun(soil)) {
                                soilIsNoData = true;
                                // when using grid model small amounts of
                                // no data for crop, soil or slope could lose subbasin
                                soil = this._db.defaultSoil;
                            } else {
                                soilIsNoData = false;
                            }
                            // use an equivalent soil if any
                            bool OK = false;
                            (soil, OK) = this._db.translateSoil(Convert.ToInt32(soil));
                            if (soilIsNoData) {
                                soilNoDataCount += 1;
                            } else if (OK) {
                                soilDefinedCount += 1;
                            } else {
                                soilUndefinedCount += 1;
                            }
                            var _tmp_3 = slopeRow - slopeTopRow;
                            if (0 <= _tmp_3 && _tmp_3 < slopeActReadRows) {
                                slopeCol = slopeColFun(col, x);
                                if (0 <= slopeCol && slopeCol < slopeNumberCols) {
                                    slopeValue = (float)slopeData[(slopeRow - slopeTopRow) * slopeNumberCols + slopeCol];
                                } else {
                                    slopeValue = slopeNoData;
                                }
                            } else {
                                slopeValue = slopeNoData;
                            }
                            // slopes will be nodata in pits
                            if (slopeValue == slopeNoData) {
                                slopeValue = Parameters._DEFAULTSLOPE;
                            }
                            if (crop != cropNoData) {
                                cropCode = this._gv.db.getLanduseCode(crop);
                                if (Parameters._WATERLANDUSES.Contains(cropCode)) {
                                    slopeValue = Math.Min(slopeValue, Parameters._WATERMAXSLOPE);
                                }
                            }
                            slope = this._db.slopeIndex(slopeValue * 100);
                            var distSt = diag;
                            var distCh = halfDiag;
                            if (!this._gv.existingWshed) {
                                var _tmp_4 = distStRow - distStTopRow;
                                if (0 <= _tmp_4 && _tmp_4 < distStActReadRows) {
                                    distStCol = distStColFun(col, x);
                                    if (0 <= distStCol && distStCol < distStNumberCols) {
                                        distSt = distStData[(distStRow - distStTopRow) * distStNumberCols + distStCol];
                                    }
                                }
                            }
                            int flood = (int)floodNoData;
                            var landscape = Utils._NOLANDSCAPE;
                            if (this._gv.useLandscapes) {
                                var _tmp_5 = floodRow - floodTopRow;
                                if (0 <= _tmp_5 && _tmp_5 < floodActReadRows) {
                                    floodCol = floodColFun(col, x);
                                    if (0 <= floodCol && floodCol < floodNumberCols) {
                                        flood = floodData[(floodRow - floodTopRow) * floodNumberCols + floodCol];
                                    } else {
                                        flood = (int)floodNoData;
                                    }
                                } else {
                                }
                                // floodplain maps use 1 for the floodplain and nodata elsewhere (for display purposes)
                                // We reset this to a landscape value
                                landscape = flood == 1 ? Utils._FLOODPLAIN : Utils._UPSLOPE;
                                int SWATChannel;
                                if (!this._gv.topo.channelToSWATChannel.TryGetValue(chLink, out SWATChannel)) {
                                    SWATChannel = 0;
                                }
                                if (SWATChannel > 0) {
                                    var lsuId = Utils.landscapeUnitId(SWATChannel, landscape);
                                    lsuRows[(row - elevationTopRow) * elevationNumberCols + col] = lsuId;
                                }
                            }
                            data.addCell(chLink, landscape, crop, soil, slope, this._gv.cellArea, elev, slopeValue, distSt, distCh, x, y, this._gv);
                            this.basins[subbasin] = data;
                            if (this._gv.useLandscapes || this.fullHRUsWanted) {
                                if (crop != cropNoData && soil != soilNoData && slope != slopeNoData) {
                                    hru = BasinData.getHruNumber(channelLandscapeCropSoilSlopeNumbers, lastHru, chLink, landscape, crop, soil, slope);
                                    if (hru > lastHru) {
                                        // new HRU number: store it
                                        lastHru = hru;
                                    }
                                    if (this.fullHRUsWanted) {
                                        hruRows[(row - elevationTopRow) * elevationNumberCols + col] = hru;
                                    }
                                }
                            }
                        }
                    }
                }
                if (this._gv.useLandscapes) {
                    // write final rows
                    foreach (var rowNum in Enumerable.Range(0, elevationReadRows)) {
                        int start = rowNum * elevationNumberCols;
                        lsuShapes.addRow(lsuRows, start, elevationCurrentRow + rowNum);
                    }
                }
                if (this.fullHRUsWanted) {
                    // write final rows
                    foreach (var rowNum in Enumerable.Range(0, elevationReadRows)) {
                        int start = rowNum * elevationNumberCols;
                        hruShapes.addRow(hruRows, start, elevationCurrentRow + rowNum);
                    }
                }
                var time2 = DateTime.Now;
                Utils.loginfo(string.Format("Reading grid model files took {0} seconds", Convert.ToInt32(time2.Subtract(time1).TotalSeconds)));
            } else {
                // not grid model          
                foreach (var row in Enumerable.Range(0, basinNumberRows)) {
                    if (progressCount == fivePercent) {
                        progressBar.Value = progressBar.Value + 5;
                        progressCount = 1;
                    } else {
                        progressCount += 1;
                    }
                    if (row != basinCurrentRow) {
                        basinBand.ReadRaster(0, row, basinNumberCols, 1, basinData, basinNumberCols, 1, 0, 0);
                    }
                    y = Topology.rowToY(row, basinTransform);
                    if (!this._gv.existingWshed) {
                        distStRow = distStRowFun(row, y);
                        if (0 <= distStRow && distStRow < distStNumberRows && distStRow != distStCurrentRow) {
                            distStCurrentRow = distStRow;
                            distStBand.ReadRaster(0, distStRow, distStNumberCols, 1, distStData, distStNumberCols, 1, 0, 0);
                        }
                        distChRow = distChRowFun(row, y);
                        if (0 <= distChRow && distChRow < distChNumberRows && distChRow != distChCurrentRow) {
                            distChCurrentRow = distChRow;
                            distChBand.ReadRaster(0, distChRow, distChNumberCols, 1, distChData, distChNumberCols, 1, 0, 0);
                        }
                    }
                    cropRow = cropRowFun(row, y);
                    if (0 <= cropRow && cropRow < cropNumberRows && cropRow != cropCurrentRow) {
                        cropCurrentRow = cropRow;
                        cropBand.ReadRaster(0, cropRow, cropNumberCols, 1, cropData, cropNumberCols, 1, 0, 0);
                    }
                    soilRow = soilRowFun(row, y);
                    if (0 <= soilRow && soilRow < soilNumberRows && soilRow != soilCurrentRow) {
                        soilCurrentRow = soilRow;
                        soilBand.ReadRaster(0, soilRow, soilNumberCols, 1, soilData, soilNumberCols, 1, 0, 0);
                    }
                    slopeRow = slopeRowFun(row, y);
                    if (0 <= slopeRow && slopeRow < slopeNumberRows && slopeRow != slopeCurrentRow) {
                        if (this._db.slopeLimits.Count > 0 && (0 <= slopeCurrentRow && slopeCurrentRow < slopeNumberRows)) {
                            // generate slope bands data and write it before reading next row
                            foreach (var i in Enumerable.Range(0, slopeNumberCols)) {
                                slopeValue = slopeData[i];
                                slopeData[i] = slopeValue != slopeNoData ? this._db.slopeIndex(slopeValue * 100) : slopeBandsNoData;
                            }
                            slopeBandsBand.WriteRaster(0, slopeCurrentRow, slopeNumberCols, 1, slopeData, slopeNumberCols, 1, 0, 0);
                        }
                        slopeCurrentRow = slopeRow;
                        slopeBand.ReadRaster(0, slopeRow, slopeNumberCols, 1, slopeData, slopeNumberCols, 1, 0, 0);
                    }
                    var elevationRow = elevationRowFun(row, y);
                    if (0 <= elevationRow && elevationRow < elevationNumberRows && elevationRow != elevationCurrentRow) {
                        elevationCurrentRow = elevationRow;
                        elevationBand.ReadRaster(0, elevationRow, elevationNumberCols, 1, elevationData, elevationNumberCols, 1, 0, 0);
                        if (usePlaya) {
                            wpBand.ReadRaster(0, elevationRow, elevationNumberCols, 1, wpData, elevationNumberCols, 1, 0, 0);
                        }
                    }
                    if (this._gv.useLandscapes) {
                        floodRow = floodRowFun(row, y);
                        if (0 <= floodRow && floodRow < floodNumberRows && floodRow != floodCurrentRow) {
                            floodCurrentRow = floodRow;
                            floodBand.ReadRaster(0, floodRow, floodNumberCols, 1, floodData, floodNumberCols, 1, 0, 0);
                        }
                    }
                    foreach (var col in Enumerable.Range(0, basinNumberCols)) {
                        var chBasin = basinData[col];
                        if (chBasin == basinNoData) {
                            if (this._gv.useLandscapes) {
                                lsuRow[col] = -1;
                            }
                            if (this.fullHRUsWanted) {
                                hruRow[col] = -1;
                            }
                            continue;
                        }
                        chBasin = Convert.ToInt32(chBasin);
                        int chLink;
                        if (!this._gv.topo.chBasinToChLink.TryGetValue(chBasin, out chLink)) {
                            chLink = -1;
                        }
                        int SWATChannel;
                        if (!this._gv.topo.channelToSWATChannel.TryGetValue(chLink, out SWATChannel)) {
                            SWATChannel = 0;
                        }
                        if (SWATChannel == 0) {
                            if (this._gv.useLandscapes) {
                                lsuRow[col] = -1;
                            }
                            if (this.fullHRUsWanted) {
                                hruRow[col] = -1;
                            }
                            continue;
                        }
                        int subbasin;
                        if (!this._gv.topo.chBasinToSubbasin.TryGetValue(chBasin, out subbasin)) {
                            subbasin = -1;
                        }
                        if (subbasin < 0) { // allow for NOBASIN (-2) as well as no such chBasin
                            if (this._gv.useLandscapes) {
                                lsuRow[col] = -1;
                            }
                            if (this.fullHRUsWanted) {
                                hruRow[col] = -1;
                            }
                            continue;
                        }
                        Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, int>>>>> channelLandscapeCropSoilSlopeNumbers = null;
                        if (!this._gv.topo.isUpstreamSubbasin(subbasin)) {
                            if (this._gv.useLandscapes || this.fullHRUsWanted) {
                                if (!subbasinChannelLandscapeCropSoilSlopeNumbers.TryGetValue(subbasin, out channelLandscapeCropSoilSlopeNumbers)) {
                                    channelLandscapeCropSoilSlopeNumbers = new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, int>>>>>();
                                    subbasinChannelLandscapeCropSoilSlopeNumbers[subbasin] = channelLandscapeCropSoilSlopeNumbers;
                                }
                            }
                            x = Topology.colToX(col, basinTransform);
                            var distSt = (float)distStNoData;
                            var distCh = (float)distChNoData;
                            if (!this._gv.existingWshed) {
                                distStCol = distStColFun(col, x);
                                if (0 <= distStCol && distStCol < distStNumberCols && (0 <= distStRow && distStRow < distStNumberRows)) {
                                    distSt = distStData[distStCol];
                                }
                                distChCol = distChColFun(col, x);
                                if (0 <= distChCol && distChCol < distChNumberCols && (0 <= distChRow && distChRow < distChNumberRows)) {
                                    // coerce dist to float else considered to be a numpy float
                                    distCh = (float)distChData[distChCol];
                                }
                            }
                            var elevationCol = elevationColFun(col, x);
                            int wpVal = 0;
                            if (0 <= elevationCol && elevationCol < elevationNumberCols && (0 <= elevationRow && elevationRow < elevationNumberRows)) {
                                elevation = elevationData[elevationCol];
                                if (usePlaya) {
                                    wpVal = wpData[elevationCol];
                                }
                            } else {
                                elevation = elevationNoData;
                                wpVal = (int)wpNoData;
                            }
                            int elev;
                            if (Utils.areClose(elevation, elevationNoData, rel_tol: 1E-06)) {
                                elev = (int)elevationNoData;
                            } else {
                                elev = Convert.ToInt32(elevation * this._gv.verticalFactor);
                            }
                            crop = -1;
                            if (usePlaya) {
                                if (wpVal == (int)WaterRole._PLAYA) {
                                    crop = this._gv.db.playaCrop;
                                }
                            }
                            if (crop < 0) {
                                cropCol = cropColFun(col, x);
                                if (0 <= cropCol && cropCol < cropNumberCols && (0 <= cropRow && cropRow < cropNumberRows)) {
                                    crop = cropData[cropCol];
                                    if (cropIsNoDataFun(crop)) {
                                        crop = (int)cropNoData;
                                    }
                                } else {
                                    crop = (int)cropNoData;
                                }
                            }
                            // landuse maps used for HUC models have 0 in Canada
                            // so to prevent messages about 0 not recognised as a landuse
                            if ((this._gv.isHUC || this._gv.isHAWQS) && crop == 0) {
                                crop = (int)cropNoData;
                            }
                            if (crop == cropNoData) {
                                landuseNoDataCount += 1;
                            } else {
                                landuseCount += 1;
                                // use an equivalent landuse if any
                                crop = this._db.translateLanduse(Convert.ToInt32(crop));
                            }
                            bool isWet = false;
                            if (crop != cropNoData) {
                                cropCode = this._gv.db.getLanduseCode(crop);
                                isWet = Parameters._WATERLANDUSES.Contains(cropCode);
                            }
                            soilCol = soilColFun(col, x);
                            if (0 <= soilCol && soilCol < soilNumberCols && (0 <= soilRow && soilRow < soilNumberRows)) {
                                soil = soilData[soilCol];
                                if (soilIsNoDataFun(soil)) {
                                    soil = (int)soilNoData;
                                }
                            } else {
                                soil = (int)soilNoData;
                            }
                            // make sure crop and soil do not conflict about water
                            if (this._gv.db.useSSURGO) {
                                if (isWet) {
                                    soil = Parameters._SSURGOWater;
                                } else if (soil == Parameters._SSURGOWater) {
                                    isWet = true;
                                    if (crop == cropNoData || !Parameters._WATERLANDUSES.Contains(cropCode)) {
                                        crop = this._gv.db.getLanduseCat("WATR");
                                    }
                                }
                            }
                            bool OK = false;
                            if (soil == soilNoData) {
                                soilIsNoData = true;
                            } else {
                                soilIsNoData = false;
                                if (soil == Parameters._SSURGOWater) {
                                    OK = true;
                                } else {
                                    // use an equivalent soil if any
                                    (soil, OK) = this._db.translateSoil(Convert.ToInt32(soil));
                                }
                            }
                            if (soilIsNoData) {
                                soilNoDataCount += 1;
                            } else if (OK) {
                                soilDefinedCount += 1;
                            } else {
                                soilUndefinedCount += 1;
                            }
                            slopeCol = slopeColFun(col, x);
                            if (0 <= slopeCol && slopeCol < slopeNumberCols && (0 <= slopeRow && slopeRow < slopeNumberRows)) {
                                slopeValue = slopeData[slopeCol];
                            } else {
                                slopeValue = slopeNoData;
                            }
                            // slopes will be nodata in pits
                            if (slopeValue == slopeNoData) {
                                slopeValue = Parameters._DEFAULTSLOPE;
                            }
                            // set water or wetland pixels to have slope at most WATERMAXSLOPE
                            if (isWet) {
                                slopeValue = Math.Min(slopeValue, Parameters._WATERMAXSLOPE);
                            }
                            if (slopeValue != slopeNoData) {
                                // for HUC, slope bands only used for agriculture
                                if (!(this._gv.isHUC || this._gv.isHAWQS) || this._gv.db.isAgriculture(crop)) {
                                    slope = this._gv.db.slopeIndex(slopeValue * 100);
                                } else {
                                    slope = 0;
                                }
                            } else {
                                slope = slopeBandsNoData;
                            }
                            int flood = (int)floodNoData;
                            int landscape = Utils._NOLANDSCAPE;
                            if (this._gv.useLandscapes) {
                                floodCol = floodColFun(col, x);
                                if (0 <= floodCol && floodCol < floodNumberCols && (0 <= floodRow && floodRow < floodNumberRows)) {
                                    flood = floodData[floodCol];
                                }
                                // floodplain maps in ArcGIS use 0 for the floodplain and nodata elsewhere (for display purposes)
                                // We reset this to a landscape value
                                landscape = flood == 0 ? Utils._FLOODPLAIN : Utils._UPSLOPE;
                                int lsuId = Utils.landscapeUnitId(SWATChannel, landscape);
                                lsuRow[col] = lsuId;
                            }
                            if (!this.basins.TryGetValue(subbasin, out data)) {
                                // new basin
                                var elevs = new List<int>();
                                for (int i = 0; i < elevMapSize; i++) { elevs.Add(0); }
                                this.basinElevMap[subbasin] = elevs;
                                // set initially negative so even the smallest actual distance will exceed it
                                double farDistance = -1;
                                if (this._gv.existingWshed) {
                                    farDistance = this._gv.topo.maxFlowLengths[subbasin];
                                }
                                data = new BasinData(this._db.waterLanduse, farDistance, 0);
                                this.basins[subbasin] = data;
                            }
                            try {
                                data.addCell(chLink, landscape, crop, soil, slope, this._gv.cellArea, elev, slopeValue, distSt, distCh, x, y, this._gv);
                            }
                            catch {
                                Utils.error(string.Format("Cannot add cell for link {0} landscape {1} crop {2} soil {3} slope {4}", chLink, landscape, crop, soil, slope), this._gv.isBatch);
                            }
                            this.basins[subbasin] = data;
                            if (!Utils.areClose(elevation, elevationNoData, rel_tol: 1E-06)) {
                                index = Convert.ToInt32(elevation - this.minElev);
                                // can have index too large because Math.Max not calculated properly by gdal
                                if (index >= elevMapSize) {
                                    foreach (var b in this.basinElevMap.Keys) {
                                        for (int i = elevMapSize; i <= index; i++) {
                                            this.basinElevMap[b].Add(0);
                                        }
                                    }
                                    for (int i = elevMapSize; i <= index; i++) {
                                        this.elevMap.Add(0);
                                    }
                                    elevMapSize = index + 1;
                                }
                                //var count = this.basinElevMap[subbasin].Count;
                                //if (index < 0 || index >= count) {
                                //    var z = 0;
                                //}
                                this.basinElevMap[subbasin][index] += 1;
                                this.elevMap[index] += 1;
                            }
                            if (this._gv.useLandscapes || this.fullHRUsWanted) {
                                if (crop != cropNoData && soil != soilNoData && slope != slopeNoData) {
                                    hru = BasinData.getHruNumber(channelLandscapeCropSoilSlopeNumbers, lastHru, chLink, landscape, crop, soil, slope);
                                    if (hru > lastHru) {
                                        // new HRU number: store it
                                        lastHru = hru;
                                    }
                                    if (this.fullHRUsWanted) {
                                        hruRow[col] = hru;
                                    }
                                } else if (this.fullHRUsWanted) {
                                    hruRow[col] = -1;
                                }
                            }
                        } else {
                            if (this._gv.useLandscapes) {
                                lsuRow[col] = -1;
                            }
                            if (this.fullHRUsWanted) {
                                hruRow[col] = -1;
                            }
                        }
                    }
                    if (this._gv.useLandscapes) {
                        lsuShapes.addRow(lsuRow, 0, row);
                    }
                    if (this.fullHRUsWanted) {
                        hruShapes.addRow(hruRow, 0, row);
                    }
                }
                if (this._db.slopeLimits.Count > 0 && (0 <= slopeCurrentRow && slopeCurrentRow < slopeNumberRows)) {
                    // write final slope bands row
                    foreach (var i in Enumerable.Range(0, slopeNumberCols)) {
                        slopeValue = slopeData[i];
                        slopeData[i] = slopeValue != slopeNoData ? this._db.slopeIndex(slopeValue * 100) : slopeBandsNoData;
                    }
                    slopeBandsBand.WriteRaster(0, slopeCurrentRow, slopeNumberCols, 1, slopeData, slopeNumberCols, 1, 0, 0);
                }
            }
            // clear some memory
            elevationDs = null;
            if (!this._gv.existingWshed && !this._gv.useGridModel) {
                distStDs = null;
                distChDs = null;
            }
            slopeDs = null;
            soilDs = null;
            cropDs = null;
            if (this._gv.useLandscapes) {
                floodDs = null;
            }
            if (this._db.slopeLimits.Count > 0) {
                // will also flush
                slopeBandsDs.Dispose();
                slopeBandsDs = null;
            }
            // check landuse and soil overlaps
            if (landuseCount + landuseNoDataCount == 0) {
                landusePercent = 0.0;
            } else {
                landusePercent = landuseCount / (landuseCount + landuseNoDataCount) * 100;
            }
            Utils.loginfo(string.Format("Landuse cover percent: {0:F1}", landusePercent));
            if (landusePercent < 95) {
                Utils.information(string.Format("WARNING: only {0:F1} percent of the watershed has defined landuse values.\n If this percentage is zero check your landuse map has the same projection as your DEM.", landusePercent), this._gv.isBatch);
            }
            double soilMapPercent = 0;
            if (soilDefinedCount + soilUndefinedCount + soilNoDataCount > 0) {
                soilMapPercent = 100.0 * (soilDefinedCount + soilUndefinedCount) / (soilDefinedCount + soilUndefinedCount + soilNoDataCount);
            }
            Utils.loginfo(string.Format("Soil cover percent: {0:F1}", soilMapPercent));
            if (soilDefinedCount + soilUndefinedCount > 0) {
                soilDefinedPercent = 100.0 * (soilDefinedCount) / (soilDefinedCount + soilUndefinedCount);
            } else {
                soilDefinedPercent = 0;
            }
            Utils.loginfo(string.Format("Soil defined percent: {0:F1}", soilDefinedPercent));
            bool under95 = false;
            if (this._gv.isHUC) {
                var huc12 = this._gv.projName.Substring(3);
                var logFile = this._gv.logFile;
                if (soilMapPercent < 1) {
                    // start of message is key phrase for HUC12Models
                    Utils.information(string.Format("EMPTY PROJECT: only {0:F1} percent of the watershed in project huc{1} is inside the soil map", soilMapPercent, huc12), this._gv.isBatch, logFile: logFile);
                    return false;
                } else if (soilMapPercent < 95) {
                    // start of message is key phrase for HUC12Models
                    Utils.information(string.Format("UNDER95 WARNING: only {0:F1} percent of the watershed in project huc{1} is inside the soil map.", soilMapPercent, huc12), this._gv.isBatch, logFile: logFile);
                    under95 = true;
                } else if (soilMapPercent < 99.95) {
                    // always give statistic for HUC models; avoid saying 100.0 when rounded to 1dp
                    // start of message is key word for HUC12Models
                    Utils.information(string.Format("WARNING: only {0:F1} percent of the watershed in project huc{1} is inside the soil map.", soilMapPercent, huc12), this._gv.isBatch, logFile: logFile);
                }
                if (soilDefinedPercent < 95) {
                    // start of message is key word for HUC12Models
                    Utils.information(string.Format("WARNING: only {0:F1} percent of the watershed in project huc{1} has defined soils.", soilDefinedPercent, huc12), this._gv.isBatch, logFile: logFile);
                }
            } else {
                if (soilMapPercent < 95) {
                    Utils.information(string.Format("WARNING: only {0:F1} percent of the watershed has defined soil values.\n If this percentage is zero check your soil map has the same projection as your DEM.", soilMapPercent), this._gv.isBatch);
                    under95 = true;
                    // only run this for non-HUC models: previous warnings sufficient
                }
                if (!this.noCropOrSoilLSUs()) {
                    return false;
                }
            }
            string lsusFile = null;
            if (this._gv.useLandscapes) {
                this._dlg.progress("Creating landscape units shapes ...");
                lsuShapes.finish();
                this._dlg.progress("Writing landscape units shapes ...");
                lsusFile = await this.createLSUShapefileFromShapes(lsuShapes, subbasinChannelLandscapeCropSoilSlopeNumbers);
            } else {
                this._dlg.progress("Writing landscape units shapes ...");
                lsusFile = this.createLSUShapefileFromWshed();
            }
            if (lsusFile is not null && !this._gv.useGridModel) {
                // insert above dem (or hillshade if exists) in legend, so subbasin still visible
                var legend = Utils._FULLLSUSLEGEND;
                var group = Utils.getGroupLayerByName(Utils._WATERSHED_GROUP_NAME);
                var demLayer = (await Utils.getLayerByFilename(this._gv.demFile, FileTypes._DEM, null, null, null)).Item1 as RasterLayer;
                RasterLayer subLayer = null;
                if (demLayer is not null) {
                    subLayer = demLayer;
                }
                await Utils.removeLayerByLegend(legend);
                var fullLSUsLayer = (await Utils.getLayerByFilename(lsusFile, FileTypes._LSUS, this._gv, subLayer, Utils._WATERSHED_GROUP_NAME)).Item1 as FeatureLayer;
            }
            this._dlg.progress("");
            if (this.fullHRUsWanted) {
                this._dlg.progress("Creating FullHRUs shapes ...");
                hruShapes.finish();
                //Utils.loginfo(hruShapes.makeString())
                this._dlg.progress("Writing FullHRUs shapes ...");
                if (!await this.createFullHRUsShapefile(hruShapes, subbasinChannelLandscapeCropSoilSlopeNumbers, progressBar, lastHru)) {
                    Utils.error("Unable to create FullHRUs shapefile", this._gv.isBatch);
                    this._dlg.progress("");
                } else {
                    this._dlg.progress("FullHRUs shapefile finished");
                }
            }
            // clear memory
            if (!this._gv.useGridModel) {
                basinDs = null;
            }
            this.saveAreas(true, redistributeNodata: true));
            this._dlg.progress("Writing HRU data to database ...");
            if (!this._db.createBasinsDataTables()) {
                return false;
            }
            this._db.writeBasinsData(this.basins);
            this._dlg.progress("Writing topographic report ...");
            this.writeTopoReport();
            this._dlg.progress("");
            return true;
        }

        // Give an error message and return false (unless batch/HUC/HAWQS) if any LSUs have no crop and soil data, nor a waterbody.
        public virtual bool noCropOrSoilLSUs() {
            foreach (var (basin, basinData) in this.basins) {
                foreach (var (channel, channelData) in basinData.lsus) {
                    foreach (var (lscape, lsuData) in channelData) {
                        if (lsuData.cropSoilSlopeNumbers.Count == 0 && lsuData.waterBody is null) {
                            var SWATBasin = this._gv.topo.subbasinToSWATBasin[basin];
                            var SWATChannel = this._gv.topo.channelToSWATChannel[channel];
                            var msg1 = "There is no crop or soil data for ";
                            var msg2 = "";
                            if (lscape == Utils._FLOODPLAIN) {
                                msg2 = "the floodplain of ";
                            } else if (lscape == Utils._UPSLOPE) {
                                msg2 = "the upslope of ";
                            }
                            var msg3 = string.Format("channel {0} (LINKNO {2}) in subbasin {1} (and perhaps others)", SWATChannel, SWATBasin, channel);
                            if (this._gv.isHUC || this._gv.isHAWQS) {
                                Utils.loginfo(msg1 + msg2 + msg3);
                            } else {
                                Utils.error(msg1 + msg2 + msg3, this._gv.isBatch);
                            }
                            // allow batch, HUC and HAWQS runs to continue
                            return this._gv.isBatch || this._gv.isHUC || this._gv.isHAWQS;
                        }
                    }
                }
            }
            return true;
        }

        // Merge channels in each basin until none are below threshold or no more can be merged.
        //         
        //         Note merging is always downstream, and this is assumed in a number of places,
        //         e.g. the outlet of a merged channel is the same as the outlet of the merge target,
        //         which has the same channel number as the target.
        public virtual void mergeChannels() {
            double threshold;

            int mergeCandidate(
                int basin,
                Dictionary<int, Dictionary<int, LSUData>> lsus,
                int channel,
                List<int> mergedToWaterOrPtSrc,
                bool isSmall,
                double threshold) {

                bool isLakeInlet(int channel) {
                    foreach (var lakeData in this._gv.topo.lakesData.Values) {
                        if (lakeData.inChLinks.ContainsKey(channel)) {
                            return true;
                        }
                    }
                    return false;
                }

                bool isLakeOutlet(int channel) {
                    return this._gv.topo.chLinkFromLake.ContainsKey(channel);
                }

                bool isWaterOutlet(int channel) {
                    foreach (var chLink in this._gv.topo.chLinkToWater.Keys) {
                        if (this._gv.topo.getDownChannel(chLink) == channel) {
                            return true;
                        }
                    }
                    return false;
                }

                if (isWaterOutlet(channel)) {
                    // cannot merge down because could pull outlets of channels joining downstream up into water body
                    return -1;
                }
                if (isLakeInlet(channel)) {
                    // downstream channel would be in lake
                    return -1;
                }
                // try downstream channel
                var dsChannel = mergedToWaterOrPtSrc.Contains(channel) ? -1 : this._gv.topo.getDownChannel(channel);
                if (dsChannel < 0) {
                    return -1;
                } else {
                    int nextChannel;
                    if (!this.mergedChannels.TryGetValue(dsChannel, out nextChannel)) {
                        nextChannel = -1;
                    }
                    if (nextChannel >= 0) {
                        // downstream channel dsChannel already merged with nextChannel further downstream: 
                        // try starting with dsChannel
                        return mergeCandidate(basin, lsus, dsChannel, mergedToWaterOrPtSrc, isSmall, threshold);
                    }
                }
                if (dsChannel >= 0) {
                    if (isLakeOutlet(dsChannel)) {
                        // reject as if channel inside or flowing into lake merged with one leaving
                        // its flow would no longer be into lake
                        return -1;
                    }
                    Dictionary<int, LSUData> dsChannelData;
                    if (!lsus.TryGetValue(dsChannel, out dsChannelData)) {
                        // different basin
                        return -1;
                    }
                    if (!isSmall) {
                        // candidate must be small:
                        if (BasinData.channelArea(dsChannelData) >= threshold) {
                            return -1;
                        }
                    }
                    return dsChannel;
                }
                return -1;
            }

            this.mergedChannels.Clear();
            this.mergees.Clear();
            // restore channel basin areas to original
            this._gv.topo.chBasinAreas = Topology.copyBasinAreas(this._gv.topo.origChBasinAreas);
            // keep list of channels with water bodies or point sources,
            // and extend this list as merges are made
            var mergedToWaterOrPtSrc = new List<int>();
            foreach (var channel in this._gv.topo.chLinkToWater.Keys) {
                mergedToWaterOrPtSrc.Add(channel);
            }
            foreach (var channel in this._gv.topo.chLinkToPtSrc.Keys) {
                mergedToWaterOrPtSrc.Add(channel);
            }
            foreach (var (basin, basinData) in this.basins) {
                var lsus = basinData.getLsus();
                if (lsus.Count == 1) {
                    // only one channel: no merge possible
                    continue;
                }
                // calculate threshold for channel area in square metres
                if (this.channelMergeByPercent) {
                    threshold = basinData.subbasinArea() * this.channelMergeThreshold / 100.0;
                } else {
                    threshold = this.channelMergeThreshold * 10000.0;
                }
                var merging = true;
                // replace any previous merge
                basinData.copyLsus();
                var hasMerge = false;
                var SWATBasin = this._gv.topo.subbasinToSWATBasin[basin];
                while (merging) {
                    merging = false;
                    foreach (var (channel, channelData) in basinData.mergedLsus) {
                        if (!this.mergedChannels.ContainsKey(channel)) {
                            var isSmall = BasinData.channelArea(channelData) < threshold;
                            var candidate = mergeCandidate(basin, basinData.mergedLsus, channel, mergedToWaterOrPtSrc, isSmall, threshold);
                            if (candidate < 0) {
                                continue;
                            } else {
                                var SWATChannel = this._gv.topo.channelToSWATChannel[channel];
                                var SWATTarget = this._gv.topo.channelToSWATChannel[candidate];
                                Utils.loginfo(string.Format("Merging channel {0} (link {1}) into {2} (link {3}) in basin {4}", SWATChannel, channel, SWATTarget, candidate, SWATBasin));
                                this.mergedChannels[channel] = candidate;
                                this.mergees.Add(candidate);
                                //                             for landscape, lsuData in channelData.iteritems():
                                //                                 CreateHRUs.checkConsistent(lsuData, basin, channel, landscape, 1)
                                //                             for landscape, lsuData in basinData.mergedLsus[candidate].iteritems():
                                //                                 CreateHRUs.checkConsistent(lsuData, basin, candidate, landscape, 2)
                                basinData.merge(channel, candidate);
                                // fix chBasinAreas map
                                // errors can cause this code to be executed twice
                                // and second time channel has already been removed
                                if (this._gv.topo.chBasinAreas.ContainsKey(channel)) {
                                    this._gv.topo.chBasinAreas[candidate] += this._gv.topo.chBasinAreas[channel];
                                    this._gv.topo.chBasinAreas.Remove(channel);
                                }
                                //TODO: merge FullHRUs and LSUs shapefiles
                                //                             for landscape, lsuData in basinData.mergedLsus[candidate].iteritems():
                                //                                 CreateHRUs.checkConsistent(lsuData, basin, candidate, landscape, 3)
                                // if channel is a lake outlet, merged one will become the lake outlet
                                int lakeId;
                                if (this._gv.topo.chLinkFromLake.TryGetValue(channel, out lakeId)) {
                                    var lakeData = this._gv.topo.lakesData[lakeId];
                                    lakeData.outChLink = candidate;
                                    this._gv.topo.chLinkFromLake.Remove(channel);
                                    this._gv.topo.chLinkFromLake[candidate] = lakeId;
                                }
                                // if channel has a point source or reservoir, mark merged one similarly
                                if (mergedToWaterOrPtSrc.Contains(channel)) {
                                    mergedToWaterOrPtSrc.Add(candidate);
                                }
                                // continue looking for merges
                                merging = true;
                                // remember to retain merge
                                hasMerge = true;
                                break;
                            }
                        }
                    }
                }
                if (!hasMerge) {
                    // no merges: remove merged data
                    basinData.mergedLsus = null;
                }
            }
        }

        // Assert consistency of hru numbers between cropSoilSlopeNumbers and hru map.
        public static void checkConsistent(
            LSUData lsuData,
            int basin,
            int channel,
            int landscape,
            int posn) {
            var hruList1 = new List<int>();
            foreach (var soilSlopeNumbers in lsuData.cropSoilSlopeNumbers.Values) {
                foreach (var slopeNumbers in soilSlopeNumbers.Values) {
                    foreach (var hru in slopeNumbers.Values) {
                        hruList1.Add(hru);
                    }
                }
            }
            var hruSet1 = new HashSet<int>(hruList1);
            var hruList2 = lsuData.hruMap.Keys.ToList();
            Debug.Assert(hruSet1 == new HashSet<int>(hruList2) && hruSet1.Count == hruList1.Count, string.Format("{5}: numbers {0} not consistent with hru map keys {1} for basin {2} channel {3} landscape {4}", lsuData.cropSoilSlopeNumbers, hruList2, basin, channel, landscape, posn));
        }

        // Add reservoirs, and remove WATR HRUs
        public virtual void addReservoirs() {

            void mergeDownstream(int floodscape) {

                WaterBody findDownstreamReservoir(int channel, int floodscape) {
                    var dsChannel = this._gv.topo.finalDownstream(channel, this.mergedChannels);
                    int chBasin;
                    if (!this._gv.topo.chLinkToChBasin.TryGetValue(dsChannel, out chBasin)) {
                        chBasin = -1;
                    }
                    int basin;
                    if (!this._gv.topo.chBasinToSubbasin.TryGetValue(chBasin, out basin)) {
                        basin = -1;
                    }
                    BasinData basinData;
                    if (this.basins.TryGetValue(basin, out basinData)) {
                        Dictionary<int, LSUData> dsChannelData;
                        if (basinData.getLsus().TryGetValue(dsChannel, out dsChannelData)) {
                            LSUData dsLsuData;
                            if (dsChannelData.TryGetValue(floodscape, out dsLsuData)) {
                                var dsWaterBody = dsLsuData.waterBody;
                                if (dsWaterBody is not null && dsWaterBody.isReservoir()) {
                                    if (dsWaterBody.isOutlet()) {
                                        return null;
                                    }
                                    if (dsWaterBody.cellCount > 0) {
                                        return dsWaterBody;
                                    } else {
                                        // continue downstream: dsChannel's reservoir has already been merged
                                        return findDownstreamReservoir(dsChannel, floodscape);
                                    }
                                }
                            }
                        }
                    }
                    return null;
                }

                foreach (var basinData in this.basins.Values) {
                    foreach (var (channel, channelData) in basinData.getLsus()) {
                        LSUData lsuData;
                        WaterBody waterBody;
                        if (!channelData.TryGetValue(floodscape, out lsuData)) {
                            lsuData = null;
                        }
                        waterBody = lsuData is null ? null : lsuData.waterBody;
                        if (waterBody is not null && waterBody.isReservoir() && !waterBody.isInlet() && waterBody.cellCount > 0) {
                            var dsWaterBody = findDownstreamReservoir(channel, floodscape);
                            if (dsWaterBody is not null) {
                                dsWaterBody.addWater(waterBody, true);
                                // leave empty water body rather than make None, so later downstream search can pass through
                                waterBody.cellCount = 0;
                            }
                        }
                    }
                }
            }

            var reservoirChannels = new HashSet<int>();
            var floodscape = this._gv.useLandscapes ? Utils._FLOODPLAIN : Utils._NOLANDSCAPE;
            // mark water bodies, either by being user-defined or by exceeding reservoir threshold
            // then merge reservoirs
            foreach (var basinData in this.basins.Values) {
                foreach (var (channel, channelData) in basinData.getLsus()) {
                    if (this._gv.topo.chLinkIntoLake.ContainsKey(channel) || this._gv.topo.chLinkInsideLake.ContainsKey(channel) || this._gv.topo.chLinkFromLake.ContainsKey(channel)) {
                        continue;
                    }
                    LSUData lsuData;
                    if (!channelData.TryGetValue(floodscape, out lsuData)) {
                        // eg only upslope lsu, which happens with grid model
                        continue;
                    }
                    var waterBody = lsuData.waterBody;
                    (int, double[], WaterRole) temp;
                    int wid;
                    if (!this._gv.topo.chLinkToWater.TryGetValue(channel, out temp)) {
                        wid = -1;
                    } else {
                        wid = temp.Item1;
                        if (wid >= 0) {
                            if (waterBody is null) {
                                // Make an empty water body so upstream waterbodies can be merged with it.
                                // channelsData is not up to date after merging
                                // (and should not be changed by merging to allow re-merging)
                                // but the channel a water body is on can only be merged with channels upstream
                                // so its outlet data is still correct
                                var channelData0 = this._gv.topo.channelsData[channel];
                                waterBody = new WaterBody(0, 0, channelData0.lowerZ, channelData0.lowerX, channelData0.lowerY);
                                lsuData.waterBody = waterBody;
                            }
                            waterBody.id = wid;
                            waterBody.setInlet();
                            if (temp.Item3 == WaterRole._RESERVOIR) {
                                waterBody.setReservoir();
                            } else {
                                waterBody.setPond();
                            }
                        }
                        if (waterBody is not null && waterBody.isUnknown()) {
                            // make reservoir if exceeds threshold
                            lsuData.makeReservoir(this.reservoirThreshold);
                            if (waterBody.isReservoir()) {
                                reservoirChannels.Add(channel);
                            }
                        }
                        if (waterBody is not null && !waterBody.isUnknown()) {
                            // is reservoir, pond or wetland
                            // need to remove WATR HRUs
                            // SWATChannel = self._gv.topo.channelToSWATChannel[channel]
                            // if SWATChannel == 761:
                            //     Utils.loginfo('Waterbody area = {0}', lsuData.waterBody.area))
                            //     Utils.loginfo('cropsoilslope area = {0}', lsuData.cropSoilSlopeArea))
                            //     hrusArea = 0
                            //     for hru, celldata in lsuData.hruMap:
                            //         Utils.loginfo('HRU {0} has area {1}', hru, celldata.area))
                            //         hrusArea += celldata.area
                            //     Utils.loginfo('Total HRUs area = {0}', hrusArea))
                            // lsuData.removeWaterHRUs(self._gv.db.waterLanduse)
                            // if SWATChannel == 761:
                            //     Utils.loginfo('After removing water HRUs:')
                            //     Utils.loginfo('Waterbody area = {0}', lsuData.waterBody.area))
                            //     Utils.loginfo('cropsoilslope area = {0}', lsuData.cropSoilSlopeArea))
                            //     hrusArea = 0
                            //     for hru, celldata in lsuData.hruMap:
                            //         Utils.loginfo('HRU {0} has area {1}', hru, celldata.area))
                            //         hrusArea += celldata.area
                            //     Utils.loginfo('Total HRUs area = {0}', hrusArea))
                            Dictionary<int, Dictionary<int, int>> soilSlopeNumbers;
                            if (lsuData.cropSoilSlopeNumbers.TryGetValue(this._gv.db.waterLanduse, out soilSlopeNumbers)) {
                                foreach (var (soil, slopeNumbers) in soilSlopeNumbers.ToList()) {
                                    foreach (var (slope, hru) in slopeNumbers.ToList()) {
                                        lsuData.removeHRU(hru, this._gv.db.waterLanduse, soil, slope);
                                    }
                                }
                            }
                        }
                    }
                }
                foreach (var channel in reservoirChannels) {
                    Utils.loginfo(string.Format("Reservoir water body in channel {0} ({1})", this._gv.topo.channelToSWATChannel[channel], channel));
                }
                mergeDownstream(floodscape);
            }
        }

        //  Create and add features to lsus shapefile layer.  Return True if OK.
        public virtual bool insertLSUFeatures(OSGeo.OGR.Layer layer, OSGeo.OGR.FeatureDefn fields, Polygonize shapes,
            Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, int>>>>>> subbasinChannelLandscapeCropSoilSlopeNumbers) {
            double percentSub;
            var lsuIndx = fields.GetFieldIndex(Topology._LSUID);
            if (lsuIndx < 0) {
                Utils.error(string.Format("Cannot find field {0} in {1}", Topology._LSUID, this._gv.fullLSUsFile), this._gv.isBatch);
                return false;
            }
            var subIndx = fields.GetFieldIndex(Topology._SUBBASIN);
            if (subIndx < 0) {
                Utils.error(string.Format("Cannot find field {0} in {1}", Topology._SUBBASIN, this._gv.fullLSUsFile), this._gv.isBatch);
                return false;
            }
            var chIndx = fields.GetFieldIndex(Topology._CHANNEL);
            if (chIndx < 0) {
                Utils.error(string.Format("Cannot find field {0} in {1}", Topology._CHANNEL, this._gv.fullLSUsFile), this._gv.isBatch);
                return false;
            }
            var catIndx = fields.GetFieldIndex(Topology._LANDSCAPE);
            if (catIndx < 0) {
                Utils.error(string.Format("Cannot find field {0} in {1}", Topology._LANDSCAPE, this._gv.fullLSUsFile), this._gv.isBatch);
                return false;
            }
            var areaIndx = fields.GetFieldIndex(Parameters._AREA);
            if (areaIndx < 0) {
                Utils.error(string.Format("Cannot find field {0} in {1}", Parameters._AREA, this._gv.fullLSUsFile), this._gv.isBatch);
                return false;
            }
            var percentSubIndx = fields.GetFieldIndex(Parameters._PERCENTSUB);
            if (percentSubIndx < 0) {
                Utils.error(string.Format("Cannot find field {0} in {1}", Parameters._PERCENTSUB, this._gv.fullLSUsFile), this._gv.isBatch);
                return false;
            }
            foreach (var (subbasin, channelLandscapeCropSoilSlopeNumbers) in subbasinChannelLandscapeCropSoilSlopeNumbers) {
                int SWATBasin;
                if (!this._gv.topo.subbasinToSWATBasin.TryGetValue(subbasin, out SWATBasin)) {
                    SWATBasin = 0;
                }
                if (SWATBasin > 0) {
                    var basinData = this.basins[subbasin];
                    var subbasinArea = basinData.subbasinArea();
                    foreach (var (channel, landscapeCropSoilSlopeNumbers) in channelLandscapeCropSoilSlopeNumbers) {
                        int SWATChannel;
                        if (!this._gv.topo.channelToSWATChannel.TryGetValue(channel, out SWATChannel)) {
                            SWATChannel = 0;
                        }
                        if (SWATChannel > 0) {
                            foreach (var landscape in landscapeCropSoilSlopeNumbers.Keys) {
                                var lsuId = Utils.landscapeUnitId(SWATChannel, landscape);
                                var geometry = shapes.getGeometry(lsuId);
                                if (geometry is null) {
                                    Utils.error(string.Format("No geometry for lsuid {0} in {1}", lsuId, this._gv.fullLSUsFile), this._gv.isBatch);
                                    return false;
                                }
                                var feature = new OSGeo.OGR.Feature(fields);
                                feature.SetField(lsuIndx, lsuId);
                                feature.SetField(subIndx, SWATBasin);
                                feature.SetField(chIndx, SWATChannel);
                                feature.SetField(catIndx, Utils.landscapeName(landscape, this._gv.useLeftRight, notEmpty: true));
                                var shapeArea = shapes.area(lsuId) * this._gv.horizontalFactor * this._gv.horizontalFactor;
                                feature.SetField(areaIndx, shapeArea / 10000.0);
                                if (subbasinArea == 0) {
                                    Utils.error(string.Format("SWAT basin {0} seems to be empty", SWATBasin), this._gv.isBatch);
                                    percentSub = 0;
                                } else {
                                    percentSub = shapeArea / subbasinArea * 100;
                                }
                                feature.SetField(percentSubIndx, percentSub);
                                feature.SetGeometry(geometry);
                                var err = layer.CreateFeature(feature);
                                if (err != Ogr.OGRERR_NONE) {
                                    Utils.error(string.Format("Unable to add feature to LSUs shapefile {0}", this._gv.fullLSUsFile), this._gv.isBatch);
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }

        //  Create and add features to FullHRUs shapefile.  Return True if OK.
        public virtual bool insertHRUFeatures(OSGeo.OGR.Layer layer, OSGeo.OGR.FeatureDefn fields, Polygonize shapes,
            Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, int>>>>>> subbasinChannelLandscapeCropSoilSlopeNumbers,
            ProgressBar progressBar,
            int lastHru) {
            double percentLsu;
            double percentSub;
            var subIndx = fields.GetFieldIndex(Topology._SUBBASIN);
            if (subIndx < 0) {
                return false;
            }
            var chIndx = fields.GetFieldIndex(Topology._CHANNEL);
            if (chIndx < 0) {
                return false;
            }
            var catIndx = fields.GetFieldIndex(Topology._LANDSCAPE);
            if (catIndx < 0) {
                return false;
            }
            var luseIndx = fields.GetFieldIndex(Parameters._LANDUSE);
            if (luseIndx < 0) {
                return false;
            }
            var soilIndx = fields.GetFieldIndex(Parameters._SOIL);
            if (soilIndx < 0) {
                return false;
            }
            var slopeIndx = fields.GetFieldIndex(Parameters._SLOPEBAND);
            if (slopeIndx < 0) {
                return false;
            }
            var areaIndx = fields.GetFieldIndex(Parameters._AREA);
            if (areaIndx < 0) {
                return false;
            }
            var percentSubIndx = fields.GetFieldIndex(Parameters._PERCENTSUB);
            if (percentSubIndx < 0) {
                return false;
            }
            var percentLsuIndx = fields.GetFieldIndex(Parameters._PERCENTLSU);
            if (percentLsuIndx < 0) {
                return false;
            }
            var hrusIndx = fields.GetFieldIndex(Topology._HRUS);
            if (hrusIndx < 0) {
                return false;
            }
            var linkIndx = fields.GetFieldIndex(Topology._LINKNO);
            if (linkIndx < 0) {
                return false;
            }
            var fivePercent = Math.Ceiling(lastHru / 20.0);
            var progressCount = 0;
            progressBar.Visible = true;
            progressBar.Value = 0;
            foreach (var (subbasin, channelLandscapeCropSoilSlopeNumbers) in subbasinChannelLandscapeCropSoilSlopeNumbers) {
                int SWATBasin;
                if (!this._gv.topo.subbasinToSWATBasin.TryGetValue(subbasin, out SWATBasin)) {
                    SWATBasin = 0;
                }
                if (SWATBasin > 0) {
                    var basinData = this.basins[subbasin];
                    // use original lsus
                    var lsus = basinData.lsus;
                    var subbasinArea = basinData.subbasinArea();
                    foreach (var (channel, landscapeCropSoilSlopeNumbers) in channelLandscapeCropSoilSlopeNumbers) {
                        int SWATChannel;
                        if (!this._gv.topo.channelToSWATChannel.TryGetValue(channel, out SWATChannel)) {
                            SWATChannel = 0;
                        }
                        foreach (var (landscape, cropSoilSlopeNumbers) in landscapeCropSoilSlopeNumbers) {
                            var lsuArea = lsus[channel][landscape].area;
                            foreach (var (crop, soilSlopeNumbers) in cropSoilSlopeNumbers) {
                                foreach (var (soil, slopeNumbers) in soilSlopeNumbers) {
                                    foreach (var (slope, hru) in slopeNumbers) {
                                        var geometry = shapes.getGeometry(hru);
                                        if (geometry is null) {
                                            return false;
                                        }
                                        var feature = new OSGeo.OGR.Feature(fields);
                                        feature.SetField(subIndx, SWATBasin);
                                        feature.SetField(chIndx, SWATChannel);
                                        feature.SetField(catIndx, Utils.landscapeName(landscape, this._gv.useLeftRight, notEmpty: true));
                                        feature.SetField(luseIndx, this._db.getLanduseCode(crop));
                                        feature.SetField(soilIndx, this._db.getSoilName(soil).Item1);
                                        feature.SetField(slopeIndx, this._db.slopeRange(slope));
                                        var shapeArea = shapes.area(hru) * this._gv.horizontalFactor * this._gv.horizontalFactor;
                                        feature.SetField(areaIndx, shapeArea / 10000.0);
                                        if (subbasinArea == 0) {
                                            Utils.error(string.Format("SWAT basin {0} seems to be empty", SWATBasin), this._gv.isBatch);
                                            percentSub = 0;
                                        } else {
                                            percentSub = shapeArea / subbasinArea * 100;
                                        }
                                        feature.SetField(percentSubIndx, percentSub);
                                        if (lsuArea == 0) {
                                            Utils.error(string.Format("LSU {0} seems to be empty", Utils.landscapeUnitId(SWATChannel, landscape)), this._gv.isBatch);
                                            percentLsu = 0;
                                        } else {
                                            percentLsu = shapeArea / lsuArea * 100;
                                        }
                                        feature.SetField(percentLsuIndx, percentLsu);
                                        feature.SetField(hrusIndx, "NA");
                                        feature.SetField(linkIndx, channel);
                                        feature.SetGeometry(geometry);
                                        var err = layer.CreateFeature(feature);
                                        if (err != Ogr.OGRERR_NONE) {
                                            Utils.error(string.Format("Unable to add feature to FullHRUs shapefile {0}", this._gv.fullHRUsFile), this._gv.isBatch);
                                            progressBar.Visible = false;
                                            return false;
                                        }
                                        if (progressCount == fivePercent) {
                                            progressBar.Value = progressBar.Value + 5;
                                            progressCount = 1;
                                        } else {
                                            progressCount += 1;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            progressBar.Visible = false;
            return true;
        }

        // Create lsus shapefile when landscapes are not in use, copying the 
        //         geometry from the wshed shapefile or from subbasins shapefile if using grid model.  Return lsus shapefile layer
        public virtual string createLSUShapefileFromWshed() {
            double percentSub;
            double area = 0;
            int SWATChannel;
            int channel;
            int SWATBasin;
            int basin;
            // copy wshed or subbasins shapefile 
            var direc = Path.GetDirectoryName(this._gv.fullLSUsFile);
            var fileName = Path.GetFileName(this._gv.fullLSUsFile);
            var baseName = Path.ChangeExtension(fileName, null);
            var sourceShapefile = this._gv.useGridModel ? this._gv.subbasinsFile : this._gv.wshedFile;
            OSGeo.OGR.Layer lsusLayer;
            Utils.copyShapefile(sourceShapefile, baseName, direc);
            using (var lsusDs = Ogr.Open(this._gv.fullLSUsFile, 1)) {
                lsusLayer = lsusDs.GetLayerByIndex(0);
                if (!(this._gv.isHUC || this._gv.isHAWQS)) { // HUC wshed files have LSUID field
                    lsusLayer.CreateField(new FieldDefn(Topology._LSUID, FieldType.OFTInteger), 1);
                }
                var lsusFields = lsusLayer.GetLayerDefn();
                // If grid model we should have subbasin, if not grid we should have channel
                var subIndx = lsusFields.GetFieldIndex(Topology._SUBBASIN);
                if (subIndx < 0) {
                    if (this._gv.useGridModel) {
                        Utils.error(string.Format("Cannot find field {0} in {1}", Topology._SUBBASIN, sourceShapefile), this._gv.isBatch);
                        return null;
                    }
                    lsusLayer.CreateField(new FieldDefn(Topology._SUBBASIN, FieldType.OFTInteger), 1);
                }
                var chIndx = lsusFields.GetFieldIndex(Topology._CHANNEL);
                if (chIndx < 0) {
                    if (!this._gv.useGridModel) {
                        Utils.error(string.Format("Cannot find field {0} in {1}", Topology._CHANNEL, sourceShapefile), this._gv.isBatch);
                        return null;
                    }
                    lsusLayer.CreateField(new FieldDefn(Topology._CHANNEL, FieldType.OFTInteger), 1);
                }
                lsusLayer.CreateField(new FieldDefn(Topology._LANDSCAPE, FieldType.OFTString), 1);
                var areaIndx = lsusFields.GetFieldIndex(Parameters._AREA);
                if (areaIndx < 0) {
                    lsusLayer.CreateField(new FieldDefn(Parameters._AREA, FieldType.OFTReal), 1);
                }
                lsusLayer.CreateField(new FieldDefn(Parameters._PERCENTSUB, FieldType.OFTReal), 1);
                var lsuIndx = lsusFields.GetFieldIndex(Topology._LSUID);
                if (lsuIndx < 0) {
                    Utils.error(string.Format("Cannot find field {0} in {1}", Topology._LSUID, this._gv.fullLSUsFile), this._gv.isBatch);
                    return null;
                }
                subIndx = lsusFields.GetFieldIndex(Topology._SUBBASIN);
                if (subIndx < 0) {
                    Utils.error(string.Format("Cannot find field {0} in {1}", Topology._SUBBASIN, this._gv.fullLSUsFile), this._gv.isBatch);
                    return null;
                }
                chIndx = lsusFields.GetFieldIndex(Topology._CHANNEL);
                if (chIndx < 0) {
                    Utils.error(string.Format("Cannot find field {0} in {1}", Topology._CHANNEL, this._gv.fullLSUsFile), this._gv.isBatch);
                    return null;
                }
                var lscapeIndx = lsusFields.GetFieldIndex(Topology._LANDSCAPE);
                if (lscapeIndx < 0) {
                    Utils.error(string.Format("Cannot find field {0} in {1}", Topology._LANDSCAPE, this._gv.fullLSUsFile), this._gv.isBatch);
                    return null;
                }
                areaIndx = lsusFields.GetFieldIndex(Parameters._AREA);
                if (areaIndx < 0) {
                    Utils.error(string.Format("Cannot find field {0} in {1}", Parameters._AREA, this._gv.fullLSUsFile), this._gv.isBatch);
                    return null;
                }
                var percentIndx = lsusFields.GetFieldIndex(Parameters._PERCENTSUB);
                if (percentIndx < 0) {
                    Utils.error(string.Format("Cannot find field {0} in {1}", Parameters._PERCENTSUB, this._gv.fullLSUsFile), this._gv.isBatch);
                    return null;
                }
                var polyIndx = lsusFields.GetFieldIndex(Topology._POLYGONID);
                if (polyIndx < 0) {
                    Utils.error(string.Format("Cannot find field {0} in {1}", Topology._POLYGONID, this._gv.fullLSUsFile), this._gv.isBatch);
                    return null;
                }
                int downIndx = 0;
                if (this._gv.useGridModel) {
                    downIndx = lsusFields.GetFieldIndex(Topology._DOWNID);
                    if (downIndx < 0) {
                        Utils.error(string.Format("Cannot find field {0} in {1}", Topology._DOWNID, this._gv.fullLSUsFile), this._gv.isBatch);
                        return null;
                    }
                }
                var areaFactor = this._gv.horizontalFactor * this._gv.horizontalFactor;
                var toDelete = new List<long>();
                lsusLayer.ResetReading();
                OSGeo.OGR.Feature f;
                do {
                    f = lsusLayer.GetNextFeature();
                    if (f is null) { break; }
                    var geom = f.GetGeometryRef();
                    // geometry can be null if LSU within lake
                    if (geom is null) {
                        area = 0;
                    }
                    else if (!geom.IsValid()) {
                        var geom1 = geom.MakeValid(null);
                        f.SetGeometry(geom1);
                        area = geom1.Area() * areaFactor / 1E4;
                    } else {
                        area = geom.Area() * areaFactor / 1E4;
                    }
                    if (this._gv.useGridModel) {
                        basin = f.GetFieldAsInteger(polyIndx);
                        var chLink = this._gv.topo.chBasinToChLink[basin];
                        if (this._gv.topo.chLinkInsideLake.ContainsKey(chLink) || this._gv.topo.chLinkFromLake.ContainsKey(chLink)) {
                            ListFuns.insertIntoSortedLongList(f.GetFID(), toDelete, true);
                            continue;
                        }
                        SWATBasin = f.GetFieldAsInteger(subIndx);
                        if (SWATBasin == 0) {
                            ListFuns.insertIntoSortedLongList(f.GetFID(), toDelete, true);
                            continue;
                        }
                        channel = this._gv.topo.chBasinToChLink[basin];
                        if (!this._gv.topo.channelToSWATChannel.TryGetValue(channel, out SWATChannel)) {
                            SWATChannel = 0;
                        }
                        if (SWATChannel == 0) {
                            ListFuns.insertIntoSortedLongList(f.GetFID(), toDelete, true);
                            continue;
                        }
                    } else {
                        var chBasin = f.GetFieldAsInteger(polyIndx);
                        SWATChannel = f.GetFieldAsInteger(chIndx);
                        if (SWATChannel == 0) {
                            ListFuns.insertIntoSortedLongList(f.GetFID(), toDelete, true);
                            continue;
                        }
                        channel = this._gv.topo.SWATChannelToChannel[SWATChannel];
                        if (!this._gv.topo.chBasinToSubbasin.TryGetValue(chBasin, out basin)) {
                            basin = -1;
                        }
                        if (!this._gv.topo.subbasinToSWATBasin.TryGetValue(basin, out SWATBasin)) {
                            SWATBasin = 0;
                        }
                        if (SWATBasin == 0) {
                            ListFuns.insertIntoSortedLongList(f.GetFID(), toDelete, true);
                            continue;
                        }
                    }
                    BasinData basinData;
                    if (this.basins.TryGetValue(basin, out basinData)) {
                        var subbasinCells = basinData.subbasinCellCount();
                        // use original lsus
                        var lsus = basinData.lsus;
                        if (this._gv.useGridModel) {
                            area = f.GetGeometryRef().Area() * areaFactor / 10000.0;
                        } else {
                            Dictionary<int, LSUData> channelData;
                            if (!basinData.getLsus().TryGetValue(channel, out channelData)) {
                                channelData = null;
                            }
                            area = 0;
                            if (channelData is not null) {
                                foreach ((int landscape, LSUData data) in channelData) {
                                    area += data.area;
                                }
                            }
                        }

                        if (!lsus.ContainsKey(channel) || area == 0) {
                            // too small?  within lake?
                            Utils.loginfo(string.Format("Ignoring LSU for link {0} with area {1}", channel, area));
                            ListFuns.insertIntoSortedLongList(f.GetFID(), toDelete, true);
                            continue;
                        }
                        var lsuData = lsus[channel][Utils._NOLANDSCAPE];
                        var lsuCells = lsuData.cellCount;
                        var lsuAreaHa = lsuData.area / 10000.0;
                        if (subbasinCells == 0) {
                            Utils.error(string.Format("SWAT basin {0} seems to be empty", SWATBasin), this._gv.isBatch);
                            percentSub = 0.0;
                        } else {
                            percentSub = (double)lsuCells / subbasinCells * 100;
                        }
                        var lsuId = Utils.landscapeUnitId(SWATChannel, Utils._NOLANDSCAPE);
                        f.SetField(lsuIndx, lsuId);
                        f.SetField(subIndx, SWATBasin);
                        f.SetField(chIndx, SWATChannel);
                        f.SetField(lscapeIndx, Utils.landscapeName(Utils._NOLANDSCAPE, this._gv.useLeftRight, notEmpty: true));
                        f.SetField(areaIndx, lsuAreaHa);
                        f.SetField(percentIndx, percentSub);
                        var err = lsusLayer.SetFeature(f);
                        if (err != Ogr.OGRERR_NONE) {
                            Utils.error(string.Format("Cannot change attributes of LSUs shapefile {0}", this._gv.fullLSUsFile), this._gv.isBatch);
                            return null;
                        }
                    }
                } while (f is not null);
                toDelete.Reverse();
                foreach (var fid in toDelete) {
                    var err = lsusLayer.DeleteFeature(fid);
                    if (err != Ogr.OGRERR_NONE) {
                        Utils.error(string.Format("Failed to delete redundant features from LSUs shapefile {0}", this._gv.fullLSUsFile), this._gv.isBatch);
                        return null;
                    }
                }
                //TODO
                //// delete PolygonId field, plus DownId if using grid model
                //lsusLayer.DeleteField(polyIndx);
                //if (this._gv.useGridModel) {
                //    lsusLayer.DeleteField(downIndx);
                //}
            }
            return this._gv.fullLSUsFile;
        }

        // Create LSU shapefile when landscapes in use.  Return LSU shapefile layer if successful, else None
        public async Task<string> createLSUShapefileFromShapes(Polygonize lsuShapes,
            Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, int>>>>>> subbasinChannelLandscapeCropSoilSlopeNumbers) {
            await Utils.removeLayerAndFiles(this._gv.fullLSUsFile);
            var driver = Ogr.GetDriverByName("ESRI Shapefile");
            DataSource ds = driver.CreateDataSource(this._gv.fullLSUsFile, null);
            var wkt = this._gv.topo.crsProject.Wkt;
            OSGeo.OSR.SpatialReference srs = new OSGeo.OSR.SpatialReference(wkt);
            var lsusLayer = ds.CreateLayer("LSUs", srs, wkbGeometryType.wkbPolygon, null);
            lsusLayer.CreateField(new FieldDefn(Topology._LSUID, FieldType.OFTInteger), 1);
            lsusLayer.CreateField(new FieldDefn(Topology._LSUID, FieldType.OFTInteger), 1);
            lsusLayer.CreateField(new FieldDefn(Topology._SUBBASIN, FieldType.OFTInteger), 1);
            lsusLayer.CreateField(new FieldDefn(Topology._CHANNEL, FieldType.OFTInteger), 1);
            lsusLayer.CreateField(new FieldDefn(Topology._LANDSCAPE, FieldType.OFTString), 1);
            lsusLayer.CreateField(new FieldDefn(Parameters._AREA, FieldType.OFTReal), 1);
            lsusLayer.CreateField(new FieldDefn(Parameters._PERCENTSUB, FieldType.OFTReal), 1);
            Utils.copyPrj(this._gv.basinFile, this._gv.fullLSUsFile);
            if (this.insertLSUFeatures(lsusLayer, lsusLayer.GetLayerDefn(), lsuShapes, subbasinChannelLandscapeCropSoilSlopeNumbers)) {
                return this._gv.fullLSUsFile;
            } else {
                return null;
            }
        }

        // Create FullHRUs shapefile.
        public async Task<bool> createFullHRUsShapefile(Polygonize hruShapes,
            Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, int>>>>>> subbasinChannelLandscapeCropSoilSlopeNumbers,
            ProgressBar progressBar, int lastHru) {
            await Utils.removeLayerAndFiles(this._gv.fullHRUsFile);
            var driver = Ogr.GetDriverByName("ESRI Shapefile");
            using (DataSource ds = driver.CreateDataSource(this._gv.fullHRUsFile, null)) {
                var wkt = this._gv.topo.crsProject.Wkt;
                OSGeo.OSR.SpatialReference srs = new OSGeo.OSR.SpatialReference(wkt);
                var hrusLayer = ds.CreateLayer("HRUs", srs, wkbGeometryType.wkbPolygon, null);
                hrusLayer.CreateField(new FieldDefn(Topology._SUBBASIN, FieldType.OFTInteger), 1);
                hrusLayer.CreateField(new FieldDefn(Topology._CHANNEL, FieldType.OFTInteger), 1);
                hrusLayer.CreateField(new FieldDefn(Topology._LANDSCAPE, FieldType.OFTString), 1);
                hrusLayer.CreateField(new FieldDefn(Parameters._LANDUSE, FieldType.OFTString), 1);
                hrusLayer.CreateField(new FieldDefn(Parameters._SOIL, FieldType.OFTString), 1);
                hrusLayer.CreateField(new FieldDefn(Parameters._SLOPEBAND, FieldType.OFTString), 1);
                hrusLayer.CreateField(new FieldDefn(Parameters._AREA, FieldType.OFTReal), 1);
                hrusLayer.CreateField(new FieldDefn(Parameters._PERCENTSUB, FieldType.OFTReal), 1);
                hrusLayer.CreateField(new FieldDefn(Parameters._PERCENTLSU, FieldType.OFTReal), 1);
                hrusLayer.CreateField(new FieldDefn(Topology._HRUS, FieldType.OFTString), 1);
                hrusLayer.CreateField(new FieldDefn(Topology._LINKNO, FieldType.OFTInteger), 1);
                Debug.Assert(this._gv.crsProject is not null);
                Utils.copyPrj(this._gv.demFile, this._gv.fullHRUsFile);
                if (!this.insertHRUFeatures(hrusLayer, hrusLayer.GetLayerDefn(), hruShapes, subbasinChannelLandscapeCropSoilSlopeNumbers, progressBar, lastHru)) {
                    return false;
                }
                hrusLayer.SyncToDisk();
            }
            // insert above dem in legend, so streams and watershed still visible
            await Utils.removeLayerByLegend(FileTypes.legend(FileTypes._HRUS));
            var demLayer = (await Utils.getLayerByFilename(this._gv.demFile, FileTypes._DEM, null, null, null)).Item1;
            var fullHRUSLayer = (await Utils.getLayerByFilename(this._gv.fullHRUsFile, FileTypes._HRUS, this._gv, demLayer, Utils._WATERSHED_GROUP_NAME)).Item1;
            return true;
        }

        // Count HRUs in watershed.
        public virtual int countHRUs() {
            var count = 0;
            foreach (var data in this.basins.Values) {
                foreach (var channelData in data.getLsus().Values) {
                    foreach (var lsuData in channelData.Values) {
                        count += lsuData.hruMap.Count;
                    }
                }
            }
            return count;
        }

        // Count channels in watershed.
        //         
        //         Note this includes channels in lakes.
        public virtual int countChannels() {
            var count = 0;
            foreach (var data in this.basins.Values) {
                count += data.getLsus().Count;
            }
            return count;
        }

        // Count channels inside lakes that have not been merged.
        public virtual int countChannelsInLake() {
            var count = 0;
            foreach (var ch in this._gv.topo.chLinkInsideLake.Keys) {
                if (!this.mergedChannels.ContainsKey(ch)) {
                    count += 1;
                }
            }
            return count;
        }

        // Count landscape units in watershed.
        public virtual int countLsus() {
            var count = 0;
            foreach (var data in this.basins.Values) {
                foreach (var channelData in data.getLsus().Values) {
                    count += channelData.Count;
                }
            }
            return count;
        }

        // Create area maps for each subbasin.
        public virtual void saveAreas(bool isOriginal, bool redistributeNodata = true) {
            IEnumerable<int> chLinksByLakes;
            if (this._gv.playaFile != "") {
                // if using playas do not want to absorb water into nearby lakes, as some playas will disappear
                chLinksByLakes = new List<int>();
            } else if (this._gv.useGridModel) {
                chLinksByLakes = this._gv.topo.chLinkIntoLake.Keys.Concat(this._gv.topo.chLinkInsideLake.Keys).Concat(this._gv.topo.chLinkFromLake.Keys);
            } else {
                chLinksByLakes = this._gv.topo.chLinkIntoLake.Keys.Concat(this._gv.topo.chLinkInsideLake.Keys);
            }
            double maxRedistributeFactor = 4 / 3.0  // for HUC and HAWQS. Corresponds to cropSoilSlopeArea is 75% of area
            foreach (var (basin, data) in this.basins) {
                if (isOriginal && (this._gv.isHUC || this._gv.isHAWQS)) {  // don't redistribute water in wetlands for HUC or HAWQS
                    foreach (var (chLink, channelData) in data.getLsus()) {
                        foreach (var (lscape, lsuData) in channelData) {
                            if (redistributeNodata) {
                                var SWATChannel = self._gv.topo.channelToSWATChannel[chLink];
                                var lsuId = Utils.landscapeUnitId(SWATChannel, lscape);
                                var doRedistribute = true;
                                var areaToRedistribute = lsuData.area - lsuData.cropSoilSlopeArea;
                                if (lsuData.area > areaToRedistribute && areaToRedistribute > 0) {
                                    var redistributeFactor = lsuData.area / (lsuData.area - areaToRedistribute);
                                    if (this._gv.isHUC || this._gv.isHAWQS) {
                                        if (redistributeFactor > maxRedistributeFactor) {
                                            // report limit also requires at least 50% defined soil and 1 sq km total LSU area
                                            if (redistributeFactor <= 2 and lsuData.area >= 1E6) {
                                                Utils.loginfo(string.Format("LSU {0} with area {1:F1} ha has less than {2:F1}% area with defined soil and landuse: not redistributing", lsuId, lsuData.area / 1E4, 100 / redistributeFactor))
                                            }
                                            lsuData.area = lsuData.cropSoilSlopeArea;
                                            doRedistribute = false;
                                        }
                                    } else if (redistributeFactor > 2) {
                                        Utils.loginfo(string.Format("Channel {0} has redistribute factor of {1:F1}", SWATChannel, redistributeFactor));
                                    }
                                    if (doRedistribute) {
                                        lsuData.redistribute(redistributeFactor);
                                    }
                                }
                            }
                            lsuData.setCropAreas(isOriginal);
                            lsuData.setSoilAreas(isOriginal);
                            lsuData.setSlopeAreas(isOriginal);
                        }
                    }
                } else {
                    data.setAreas(isOriginal, chLinksByLakes, this._gv.db.waterLanduse, redistributeNodata: redistributeNodata);
                }
            }
        }

        //===========================================================================
        //     if not redistributeNodata:
        //         # need to correct the drain areas of the basins, using the defined area of each
        //         self.defineDrainAreas()
        //         
        // def defineDrainAreas(self) -> None:
        //     """Reset drain areas map according to defined area value of each basin and update Reach table.  
        //     For use with HUC models, so we can assume number of basins is small and use recursion.
        //     Also assumes (because HUC model) there are no basins above inlets."""
        //     
        //     def drainArea(us: Dict[int, List[int]], chLink: int) -> float:
        //         """Return drain area for link."""
        //         
        //         drainAreaDone = self._gv.topo.drainAreas.TryGetValue(chLink, -1)
        //         if drainAreaDone >= 0:
        //             return drainAreaDone
        //         chBasin = self._gv.topo.chLinkToChBasin.TryGetValue(chLink, -1)
        //         basin = self._gv.topo.chBasinToSubbasin.TryGetValue(chBasin, -1)
        //         if basin < 0:
        //             result = 0.0
        //         else:
        //             basinData = self.basins.TryGetValue(basin, None)
        //             if basinData is None:
        //                 result = 0.0
        //             else:
        //                 ups = us.TryGetValue(link, [])
        //                 result = basinData.cropSoilSlopeArea + sum([drainArea(us, l) for l in ups])
        //         self._gv.topo.drainAreas[link] = result
        //         return result
        //         
        //     # build us relation from downChannels map
        //     assert self._gv.isHUC, "Internal error: definedDrainAreas called for non-HUC model"
        //     us: Dict[int, List[int]] = dict()
        //     for link, dsLink in self._gv.topo.downChannels:
        //         if dsLink >= 0:
        //             ups = us.setdefault(dsLink, [])
        //             ups.Add(link)
        //     # redefine link drain areas and update Reach table
        //     self._gv.topo.drainAreas = dict()
        //     sql = 'UPDATE Reach SET AreaC=?, Wid2=?, Dep2=? WHERE Subbasin=?'
        //     for chLink, chBasin in self._gv.topo.chLinkToChBasin:
        //         basin = self._gv.topo.chBasinToSubbasin.TryGetValue(chBasin, -1)
        //         basinData = self.basins.TryGetValue(basin, None)
        //         if basinData is not None:
        //             basinData.drainArea = drainArea(us, chLink)
        //             drainAreaHa = basinData.drainArea / 1E4
        //             drainAreaKm = drainAreaHa / 1E2
        //             SWATBasin = self._gv.topo.subbasinToSWATBasin[basin]
        //             channelWidth = float(self._gv.channelWidthMultiplier * drainAreaKm ** self._gv.channelWidthExponent)
        //             channelDepth = floatself._gv.channelDepthMultiplier * drainAreaKm ** self._gv.channelDepthExponent)
        //             conn.execute(sql, (basinData.drainArea / 1E4, channelWidth, channelDepth, SWATBasin))
        //===========================================================================
        // 
        //         Return the maximum landscape area in hectares.
        //         
        public virtual double maxLandscapeArea() {
            var maximum = 0.0;
            foreach (var basinData in this.basins.Values) {
                foreach (var channelData in basinData.getLsus().Values) {
                    foreach (var lsuData in channelData.Values) {
                        var area = lsuData.area;
                        if (area > maximum) {
                            maximum = area;
                        }
                    }
                }
            }
            return maximum / 10000.0;
        }

        // 
        //         Return the minimum across the watershed of the largest percentage (or area in hectares)
        //         of a crop within each landscape unit.
        //         
        //         Finds the least percentage (or area) across the landscape units of the percentages 
        //         (or areas) of the dominant crop in the landscape units.  This is the maximum percentage (or area)
        //         acceptable for the minuimum crop percentage (or area) to be used to form multiple HRUs.  
        //         If the user could choose a percentage (or area) above this figure then at
        //         least one landscape unit would have no HRU.
        //         This figure is only advisory since limits are checked during removal.
        //         
        public virtual double minMaxCropVal(bool useArea) {
            var minMax = useArea ? Int32.MaxValue : 100.0;
            foreach (var basinData in this.basins.Values) {
                foreach (var (channel, channelData) in basinData.getLsus()) {
                    foreach (var (landscape, lsuData) in channelData) {
                        var cropAreas = lsuData.originalCropAreas;
                        var (crop, maxArea) = BasinData.dominantKey(cropAreas);
                        if (crop < 0) {
                            // empty map - perhaps all water
                            continue;
                        }
                        double val;
                        if (useArea) {
                            val = maxArea / 10000.0;
                        } else if (lsuData.cropSoilSlopeArea == 0) {
                            var SWATChannel = this._gv.topo.channelToSWATChannel[channel];
                            Utils.error(string.Format("LSU {0} seems to have no land area", Utils.landscapeUnitId(SWATChannel, landscape)), this._gv.isBatch);
                            val = 0;
                        } else {
                            val = maxArea / lsuData.cropSoilSlopeArea * 100;
                        }
                        // Utils.loginfo('Max crop value {0} for landscape {2} in basin {1}', int(val), self._gv.topo.subbasinToSWATBasin[basin], landscape))
                        if (val < minMax) {
                            minMax = val;
                        }
                    }
                }
            }
            return minMax;
        }

        // 
        //         Return the minimum across the watershed of the largest area in hectares
        //         of a soil within each landscape unit.
        //         
        //         Finds the least area across the landscape units of the areas of the dominant soil
        //         in the landscape units.  This is the maximum area
        //         acceptable for the minuimum soil area to be used to form multiple HRUs.  
        //         If the user could choose an area above this figure then at
        //         least one landscape unit would have no HRU.
        //         This figure is only advisory since limits are checked during removal.
        //         
        public virtual double minMaxSoilArea() {
            var minMax = double.PositiveInfinity;
            foreach (var basinData in this.basins.Values) {
                foreach (var channelData in basinData.getLsus().Values) {
                    foreach (var lsuData in channelData.Values) {
                        var soilAreas = lsuData.originalSoilAreas;
                        var (soil, maxArea) = BasinData.dominantKey(soilAreas);
                        if (soil < 0) {
                            // empty map - perhaps all water
                            continue;
                        }
                        var val = maxArea / 10000;
                        // Utils.loginfo('Max soil area {0} for landscape {2} in basin {1}', int(val), self._gv.topo.subbasinToSWATBasin[basin], landscape))
                        if (val < minMax) {
                            minMax = val;
                        }
                    }
                }
            }
            return minMax;
        }

        // 
        //         Return the minimum across the watershed of the largest area in hectares
        //         of a slope within each landscape unit.
        //         
        //         Finds the least area across the landscape units of the areas of the dominant slope
        //         in the subbasins.  This is the maximum area
        //         acceptable for the minuimum slope area to be used to form multiple HRUs.  
        //         If the user could choose an area above this figure then at
        //         least one landscape unit would have no HRU.
        //         This figure is only advisory since limits are checked during removal.
        //         
        public virtual double minMaxSlopeArea() {
            var minMax = double.PositiveInfinity;
            foreach (var basinData in this.basins.Values) {
                foreach (var channelData in basinData.getLsus().Values) {
                    foreach (var lsuData in channelData.Values) {
                        var slopeAreas = lsuData.originalSlopeAreas;
                        var (slope, slopeArea) = BasinData.dominantKey(slopeAreas);
                        if (slope < 0) {
                            // empty map - perhaps all water
                            continue;
                        }
                        var val = slopeArea / 10000;
                        // Utils.loginfo('Max slope area {0} for landsacape {2} in basin {1}', int(val), self._gv.topo.subbasinToSWATBasin[basin], landscape))
                        if (val < minMax) {
                            minMax = val;
                        }
                    }
                }
            }
            return minMax;
        }

        // 
        //         Return the minimum across the watershed of the percentages
        //         of the dominant soil in the crops included by minCropVal.
        // 
        //         Finds the least percentage across the watershed of the percentages 
        //         of the dominant soil in the crops included by minCropVal.  
        //         This is the maximum percentage acceptable for the minimum soil
        //         percentage to be used to form multiple HRUs.  
        //         If the user could choose a percentage above this figure then
        //         at least one soil in one landscape unit would have no HRU.
        //         This figure is only advisory since limits are checked during removal.
        //         
        public virtual double minMaxSoilPercent(double minCropVal) {
            double soilVal;
            double cropVal;
            int SWATChannel;
            var minMax = 100.0;
            foreach (var basinData in this.basins.Values) {
                foreach (var (channel, channelData) in basinData.getLsus()) {
                    foreach (var (landscape, lsuData) in channelData) {
                        var cropAreas = lsuData.originalCropAreas;
                        foreach (var (crop, cropArea) in cropAreas) {
                            if (lsuData.cropSoilSlopeArea == 0) {
                                SWATChannel = this._gv.topo.channelToSWATChannel[channel];
                                Utils.error(string.Format("LSU {0} seems to have no land area", Utils.landscapeUnitId(SWATChannel, landscape)), this._gv.isBatch);
                                cropVal = 0.0;
                            } else {
                                cropVal = cropArea / lsuData.cropSoilSlopeArea * 100;
                            }
                            if (cropVal >= minCropVal) {
                                // crop will be included.  Find the maximum area or percentage for soils for this crop.
                                var maximum = 0.0;
                                var soilSlopeNumbers = lsuData.cropSoilSlopeNumbers[crop];
                                foreach (var slopeNumbers in soilSlopeNumbers.Values) {
                                    var area = 0.0;
                                    foreach (var hru in slopeNumbers.Values) {
                                        var cellData = lsuData.hruMap[hru];
                                        area += cellData.area;
                                    }
                                    if (cropArea == 0) {
                                        SWATChannel = this._gv.topo.channelToSWATChannel[channel];
                                        var cropName = this._db.getLanduseCode(crop);
                                        Utils.error(string.Format("crop {0} in LSU {1} seems to have no area", cropName, Utils.landscapeUnitId(SWATChannel, landscape)), this._gv.isBatch);
                                        soilVal = 0.0;
                                    } else {
                                        soilVal = area / cropArea * 100;
                                    }
                                    if (soilVal > maximum) {
                                        maximum = soilVal;
                                    }
                                }
                                if (maximum < minMax) {
                                    minMax = maximum;
                                }
                            }
                        }
                    }
                }
            }
            return minMax;
        }

        // 
        //         Return the minimum across the watershed of the percentages 
        //         of the dominant slope in the crops and soils included by 
        //         minCropPercent and minSoilPercent.
        //         
        //         Finds the least percentage across the watershed of the percentages 
        //         of the dominant slope in the crops and soils included by 
        //         minCropVal and minSoilVal.
        //         This is the maximum percentage  acceptable for the minimum slope
        //         percentage to be used to form multiple HRUs.  
        //         If the user could choose a percentage above this figure then
        //         at least one slope in one landscape unit would have no HRU.
        //         This figure is only advisory since limits are checked during removal.
        //         
        public virtual double minMaxSlopePercent(double minCropVal, double minSoilVal) {
            double slopeVal;
            double soilVal;
            CellData cellData;
            double cropVal;
            int SWATChannel;
            var minMax = 100.0;
            foreach (var basinData in this.basins.Values) {
                foreach (var (channel, channelData) in basinData.getLsus()) {
                    foreach (var (landscape, lsuData) in channelData) {
                        var cropAreas = lsuData.originalCropAreas;
                        foreach (var (crop, cropArea) in cropAreas) {
                            if (lsuData.cropSoilSlopeArea == 0) {
                                SWATChannel = this._gv.topo.channelToSWATChannel[channel];
                                Utils.error(string.Format("LSU {0} seems to have no land area", Utils.landscapeUnitId(SWATChannel, landscape)), this._gv.isBatch);
                                cropVal = 0.0;
                            } else {
                                cropVal = cropArea / lsuData.cropSoilSlopeArea * 100;
                            }
                            if (cropVal >= minCropVal) {
                                // crop will be included.
                                var soilSlopeNumbers = lsuData.cropSoilSlopeNumbers[crop];
                                foreach (var (soil, slopeNumbers) in soilSlopeNumbers) {
                                    // first find if this soil is to be included
                                    var soilArea = 0.0;
                                    foreach (var hru in slopeNumbers.Values) {
                                        cellData = lsuData.hruMap[hru];
                                        soilArea += cellData.area;
                                    }
                                    if (cropArea == 0) {
                                        SWATChannel = this._gv.topo.channelToSWATChannel[channel];
                                        var cropName = this._db.getLanduseCode(crop);
                                        Utils.error(string.Format("crop {0} in LSU {1} seems to have no area", cropName, Utils.landscapeUnitId(SWATChannel, landscape)), this._gv.isBatch);
                                        soilVal = 0.0;
                                    } else {
                                        soilVal = soilArea / cropArea * 100;
                                    }
                                    if (soilVal >= minSoilVal) {
                                        // soil will be included.
                                        // Find the maximum percentage area for slopes for this soil.
                                        var maximum = 0.0;
                                        foreach (var hru in slopeNumbers.Values) {
                                            cellData = lsuData.hruMap[hru];
                                            if (soilArea == 0) {
                                                SWATChannel = this._gv.topo.channelToSWATChannel[channel];
                                                var soilName = this._db.getSoilName(soil).Item1;
                                                Utils.error(string.Format("soil {0} in LSU {1} seems to have no area", soilName, Utils.landscapeUnitId(SWATChannel, landscape)), this._gv.isBatch);
                                                slopeVal = 0;
                                            } else {
                                                slopeVal = cellData.area / soilArea * 100;
                                                if (slopeVal > maximum) {
                                                    maximum = slopeVal;
                                                }
                                            }
                                        }
                                        if (maximum < minMax) {
                                            minMax = maximum;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return minMax;

            // beware = this function is no longer used and is also out of date because 
            // it has not been revised to allow for both area and percentages as thresholds
            // nor does it include landscapes
            //===============================================================================
            //     def cropSoilAndSlopeThresholdsAreOK(self):
            //         """
            //         Check that at least one hru will be left in each subbasin 
            //         after applying thresholds.
            //         
            //         This is really a precondition for removeSmallHRUsByThreshold.
            //         It checks that at least one crop will be left
            //         in each subbasin, that at least one soil will be left for each crop,
            //         and that at least one slope will be left for each included crop and 
            //         soil combination.
            //         """
            //         minCropVal = self.landuseVal
            //         minSoilVal = self.soilVal
            //         minSlopeVal = self.slopeVal
            // 
            //         for basinData in self.basins.itervalues():
            //             cropAreas = basinData.originalCropAreas
            //             cropFound = False
            //             minCropArea = minCropVal * 10000 if self.useArea else (float(basinData.landscapeCropSoilSlopeArea) * minCropVal) / 100
            //             for (crop, area) in cropAreas.iteritems():
            //                 cropFound = cropFound or (area >= minCropArea)
            //                 if area >= minCropArea:
            //                     # now check soils for this crop
            //                     soilFound = False
            //                     minSoilArea = minSoilVal * 10000 if self.useArea else (float(area) * minSoilVal) / 100
            //                     soilSlopeNumbers = basinData.cropSoilSlopeNumbers[crop]
            //                     for slopeNumbers in soilSlopeNumbers.itervalues():
            //                         soilArea = 0
            //                         for hru in slopeNumbers.itervalues():
            //                             cellData = basinData.hruMap[hru]
            //                             soilArea += cellData.area
            //                         soilFound = soilFound or (soilArea >= minSoilArea)
            //                         if soilArea >= minSoilArea:
            //                             # now sheck for slopes for this soil
            //                             slopeFound = False
            //                             minSlopeArea = minSlopeVal * 10000 if self.useArea else (float(soilArea) * minSlopeVal) / 100
            //                             for hru in slopeNumbers.itervalues():
            //                                 cellData = basinData.hruMap[hru]
            //                                 slopeFound = (cellData.area >= minSlopeArea)
            //                                 if slopeFound: break
            //                             if not slopeFound: return False
            //                     if not soilFound: return False
            //             if not cropFound: return False
            //         return True
            //===============================================================================
        }

        // Create in each landscape unit a single HRU with the crop, soil and slope of the largest HRU.
        public virtual void selectDominantHRUs() {
            double frac;
            double area;
            int cellCount;
            foreach (var (basin, basinData) in this.basins) {
                foreach (var (channel, channelData) in basinData.getLsus()) {
                    foreach (var (landscape, lsuData) in channelData) {
                        var allowWaterHRU = lsuData.waterBody is null || lsuData.waterBody.isUnknown();
                        var (crop, soil, slope) = lsuData.getDominantHRU(this._gv.db.waterLanduse, allowWaterHRU);
                        if (crop < 0) {
                            if (!allowWaterHRU) {
                                // have just reservoir or pond
                                lsuData.hruMap = new Dictionary<int, CellData>();
                                lsuData.cropSoilSlopeNumbers = new Dictionary<int, Dictionary<int, Dictionary<int, int>>>();
                                return;
                            }
                            throw new InvalidDataException(string.Format("No crop data for channel {2} landscape unit {1} in basin {0}", basin, landscape, channel));
                        }
                        if (allowWaterHRU) {
                            cellCount = lsuData.cellCount;
                            area = lsuData.area;
                            frac = 1.0;
                        } else {
                            // fraction of lsu that is HRU
                            frac = 1.0 - lsuData.waterBody.originalArea / lsuData.area;
                            // water body cellCount may have been made zero, so reconstruct
                            cellCount = Convert.ToInt32(lsuData.cellCount * frac);
                            area = lsuData.area - lsuData.waterBody.originalArea;
                        }
                        var cellData = new CellData(cellCount, area, lsuData.totalElevation * frac, lsuData.totalSlope * frac, lsuData.totalLongitude * frac, lsuData.totalLatitude * frac, crop);
                        lsuData.hruMap = new Dictionary<int, CellData>();
                        lsuData.hruMap[1] = cellData;
                        lsuData.cropSoilSlopeNumbers = new Dictionary<int, Dictionary<int, Dictionary<int, int>>>();
                        lsuData.cropSoilSlopeNumbers[crop] = new Dictionary<int, Dictionary<int, int>>();
                        lsuData.cropSoilSlopeNumbers[crop][soil] = new Dictionary<int, int>();
                        lsuData.cropSoilSlopeNumbers[crop][soil][slope] = 1;
                    }
                }
            }
        }

        // Create in each landscape unit a single HRU with the dominant landuse, soil and slope.
        public virtual void selectDominantLanduseSoilSlope() {
            double frac;
            double area;
            int cellCount;
            foreach (var (basin, basinData) in this.basins) {
                foreach (var (channel, channelData) in basinData.getLsus()) {
                    foreach (var (landscape, lsuData) in channelData) {
                        var allowWaterHRU = lsuData.waterBody is null || lsuData.waterBody.isUnknown();
                        var cropAreas = lsuData.originalCropAreas;
                        if (!allowWaterHRU) {
                            cropAreas.Remove(this._gv.db.waterLanduse);
                        }
                        if (cropAreas.Count == 0) {
                            // lsu is all reservoir or pond
                            lsuData.hruMap = new Dictionary<int, CellData>();
                            lsuData.cropSoilSlopeNumbers = new Dictionary<int, Dictionary<int, Dictionary<int, int>>>();
                        } else {
                            var crop = BasinData.dominantKey(cropAreas).Item1;
                            if (crop < 0) {
                                throw new InvalidDataException(string.Format("No crop data for channel {2} landscape unit {1} in basin {0}", basin, landscape, channel));
                            }
                            var soil = BasinData.dominantKey(lsuData.originalSoilAreas).Item1;
                            if (soil < 0) {
                                throw new InvalidDataException(string.Format("No soil data for channel {2} landscape unit {1} in basin {0}", basin, landscape, channel));
                            }
                            var slope = BasinData.dominantKey(lsuData.originalSlopeAreas).Item1;
                            if (slope < 0) {
                                throw new InvalidDataException(string.Format("No slope data for channel {2} landscape unit {1} in basin {0}", basin, landscape, channel));
                            }
                            if (allowWaterHRU) {
                                cellCount = lsuData.cellCount;
                                area = lsuData.area;
                                frac = 1.0;
                            } else {
                                // fraction of lsu that is HRU
                                frac = 1.0 - lsuData.waterBody.originalArea / lsuData.area;
                                // water body cellCount may have been made zero, so reconstruct
                                cellCount = Convert.ToInt32(lsuData.cellCount * frac);
                                area = lsuData.area - lsuData.waterBody.originalArea;
                            }
                            var cellData = new CellData(cellCount, area, lsuData.totalElevation * frac, lsuData.totalSlope * frac, lsuData.totalLongitude * frac, lsuData.totalLatitude * frac, crop);
                            lsuData.hruMap = new Dictionary<int, CellData>();
                            lsuData.hruMap[1] = cellData;
                            lsuData.cropSoilSlopeNumbers = new Dictionary<int, Dictionary<int, Dictionary<int, int>>>();
                            lsuData.cropSoilSlopeNumbers[crop] = new Dictionary<int, Dictionary<int, int>>();
                            lsuData.cropSoilSlopeNumbers[crop][soil] = new Dictionary<int, int>();
                            lsuData.cropSoilSlopeNumbers[crop][soil][slope] = 1;
                        }
                    }
                }
            }
        }

        // 
        //         Remove from basins data HRUs that are below the minimum area or minumum percent.
        //         
        //         Removes from basins data HRUs that are below areaVal 
        //         (which is in hectares if useArea is true, else is a percentage) 
        //         and redistributes their areas and slope 
        //         totals in proportion to the other HRUs.
        //         Crop, soil, and slope nodata cells are also redistributed, 
        //         so the total area of the retained HRUs should eventually be the 
        //         total area of the subbasin's landscape units.
        //         
        //         The algorithm removes one HRU at a time, the smallest, 
        //         redistributing its area to the others, until all are above the 
        //         threshold or only one is left.  So an HRU that was initially below the
        //         threshold may be retained because redistribution from smaller 
        //         ones lifts its area above the threshold.
        //         
        //         The area of a landscape unit can be below the minimum area, 
        //         in which case the dominant HRU will finally be left.
        //         
        public virtual void removeSmallHRUsByArea() {
            foreach (var (basin, basinData) in this.basins) {
                foreach (var (channel, channelData) in basinData.getLsus()) {
                    foreach (var (landscape, lsuData) in channelData) {
                        var count = lsuData.hruMap.Count;
                        // self.areaVal is either an area in hectares or a percentage of the subbasin
                        // in either case convert to square metres
                        var threshold = this.useArea ? this.areaVal * 10000 : lsuData.cropSoilSlopeArea * this.areaVal / 100;
                        var exemptWater = lsuData.waterBody is not null && !lsuData.waterBody.isUnknown();
                        var hrusArea = lsuData.area;
                        if (exemptWater) {
                            hrusArea -= lsuData.waterBody.originalArea;
                            if (Math.Abs(lsuData.cropSoilSlopeArea - lsuData.waterBody.originalArea) < this._gv.cellArea) {
                                // all reservoir or pond
                                lsuData.hruMap = new Dictionary<int, CellData>();
                                lsuData.cropSoilSlopeNumbers = new Dictionary<int, Dictionary<int, Dictionary<int, int>>>();
                                continue;
                            }
                        }
                        var areaToRedistribute = 0.0;
                        var unfinished = true;
                        while (unfinished) {
                            // find smallest non-exempt HRU
                            var minCrop = 0;
                            var minSoil = 0;
                            var minSlope = 0;
                            var minHru = 0;
                            var minArea = threshold;
                            foreach (var (crop, soilSlopeNumbers) in lsuData.cropSoilSlopeNumbers) {
                                if (!this._gv.isExempt(crop) && !(crop == this._gv.db.waterLanduse && exemptWater)) {
                                    foreach (var (soil, slopeNumbers) in soilSlopeNumbers) {
                                        foreach (var (slope, hru) in slopeNumbers) {
                                            var cellData = lsuData.hruMap[hru];
                                            if (cellData.area < minArea) {
                                                minArea = cellData.area;
                                                minHru = hru;
                                                minCrop = crop;
                                                minSoil = soil;
                                                minSlope = slope;
                                            }
                                        }
                                    }
                                }
                            }
                            if (minArea < threshold) {
                                // Don't remove last hru.
                                // This happens when the subbasin area is below the area threshold
                                if (count > 1) {
                                    lsuData.removeHRU(minHru, minCrop, minSoil, minSlope);
                                    count -= 1;
                                    areaToRedistribute += minArea;
                                } else {
                                    // count is 1; ensure termination after redistributing
                                    unfinished = false;
                                }
                                if (areaToRedistribute > 0) {
                                    // make sure we don't divide by zero (or close to it)
                                    if (Math.Abs(hrusArea - areaToRedistribute) < this._gv.cellArea) {
                                        if (exemptWater || (this._gv.isHUC || this._gv.isHAWQS)) {
                                            // all rest is reservoir or pond, or with HUC or HAWQS may be water being added to lake
                                            lsuData.hruMap = new Dictionary<int, CellData>();
                                            lsuData.cropSoilSlopeNumbers = new Dictionary<int, Dictionary<int, Dictionary<int, int>>>();
                                            continue;
                                        }
                                        throw new InvalidDataException(string.Format("No HRUs for channel {2} landscape {1} in basin {0}", basin, landscape, channel));
                                    }
                                    var redistributeFactor = hrusArea / (hrusArea - areaToRedistribute);
                                    lsuData.redistribute(redistributeFactor);
                                    areaToRedistribute = 0;
                                }
                            } else {
                                unfinished = false;
                            }
                        }
                    }
                }
            }
        }

        // 
        //         Remove HRUs that are below the minCropVal, minSoilVal, 
        //         or minSlopeVal, where the values are percentages.
        // 
        //         Remove from basins data HRUs that are below the minCropVal,
        //         minSoilVal, or minSlopeVal, where the values are percentages, and 
        //         redistribute their areas in proportion to the other HRUs in the same landscape unit.
        //         Do not remove the last HRU.  
        //         Crop, soil, and slope nodata cells are also redistributed, 
        //         so the total area of the retained HRUs in all landscape units should eventually be the total
        //         area of the subbasin, provided landscape units don't have landscape no data values.
        //         
        public virtual void removeSmallHRUsByThresholdPercent() {
            CellData cellData;
            double soilArea;
            double minArea;
            var minCropPercent = this.landuseVal;
            var minSoilPercent = this.soilVal;
            var minSlopePercent = this.slopeVal;
            foreach (var (basin, basinData) in this.basins) {
                foreach (var (channel, channelData) in basinData.getLsus()) {
                    foreach (var (landscape, lsuData) in channelData) {
                        var cropAreas = lsuData.originalCropAreas;
                        var exemptWater = lsuData.waterBody is not null && !lsuData.waterBody.isUnknown();
                        var hrusArea = lsuData.area;
                        if (exemptWater) {
                            hrusArea -= lsuData.waterBody.originalArea;
                            if (Math.Abs(lsuData.cropSoilSlopeArea - lsuData.waterBody.originalArea) < this._gv.cellArea) {
                                // all reservoir or pond
                                lsuData.hruMap = new Dictionary<int, CellData>();
                                lsuData.cropSoilSlopeNumbers = new Dictionary<int, Dictionary<int, Dictionary<int, int>>>();
                                continue;
                            }
                        }
                        var areaToRedistribute = 0.0;
                        var minCropArea = lsuData.cropSoilSlopeArea * minCropPercent / 100;
                        // reduce area if necessary to avoid removing all crops
                        if (!this.hasExemptCrop(lsuData)) {
                            minCropArea = Math.Min(minCropArea, CreateHRUs.maxValue(cropAreas));
                        }
                        foreach (var (crop, area) in cropAreas) {
                            if (!this._gv.isExempt(crop) && !(crop == this._gv.db.waterLanduse && exemptWater)) {
                                if (area < minCropArea) {
                                    areaToRedistribute += area;
                                    // remove this crop
                                    // going to change maps so use lists
                                    var soilSlopeNumbers = lsuData.cropSoilSlopeNumbers[crop];
                                    foreach (var (soil, slopeNumbers) in soilSlopeNumbers.ToList()) {
                                        foreach (var (slope, hru) in slopeNumbers.ToList()) {
                                            lsuData.removeHRU(hru, crop, soil, slope);
                                        }
                                    }
                                }
                            }
                        }
                        if (areaToRedistribute > 0) {
                            // just to make sure we don't divide by zero or close to it
                            if (Math.Abs(hrusArea - areaToRedistribute) < this._gv.cellArea) {
                                if (exemptWater) {
                                    // all rest is reservoir or pond
                                    lsuData.hruMap = new Dictionary<int, CellData>();
                                    lsuData.cropSoilSlopeNumbers = new Dictionary<int, Dictionary<int, Dictionary<int, int>>>();
                                    continue;
                                }
                                throw new InvalidDataException(string.Format("No landuse data for channel {2} landscape {1} in basin {0}", basin, landscape, channel));
                            }
                            var redistributeFactor = hrusArea / (hrusArea - areaToRedistribute);
                            lsuData.redistribute(redistributeFactor);
                        }
                        // Now have to remove soil areas within each crop area that are
                        // less than minSoilVal for that crop.
                        // First create crop areas map (not overwriting the original)
                        lsuData.setCropAreas(false);
                        foreach (var (crop, soilSlopeNumbers) in lsuData.cropSoilSlopeNumbers) {
                            var cropArea = lsuData.cropAreas[crop];
                            minArea = cropArea * minSoilPercent / 100;
                            var soilAreas = lsuData.cropSoilAreas(crop);
                            // reduce area if necessary to avoid removing all soils for this crop
                            minArea = Math.Min(minArea, CreateHRUs.maxValue(soilAreas));
                            var soilAreaToRedistribute = 0.0;
                            // Cannot use original soilSlopeNumbers as we will remove domain elements, so make list
                            foreach (var (soil, slopeNumbersCopy) in soilSlopeNumbers.ToList()) {
                                // first calculate area for this soil
                                soilArea = soilAreas[soil];
                                if (soilArea < minArea) {
                                    // add to area to redistribute
                                    soilAreaToRedistribute += soilArea;
                                    // remove hrus
                                    foreach (var (slope, hru) in slopeNumbersCopy.ToList()) {
                                        lsuData.removeHRU(hru, crop, soil, slope);
                                    }
                                }
                            }
                            if (soilAreaToRedistribute > 0) {
                                // now redistribute
                                // just to make sure we don't divide by zero
                                if (Math.Abs(cropArea - soilAreaToRedistribute) < this._gv.cellArea) {
                                    if (exemptWater) {
                                        // all rest is reservoir or pond
                                        lsuData.hruMap = new Dictionary<int, CellData>();
                                        lsuData.cropSoilSlopeNumbers = new Dictionary<int, Dictionary<int, Dictionary<int, int>>>();
                                        continue;
                                    }
                                    throw new InvalidDataException(string.Format("No soil data for landuse {1} in channel {3} landscape {2} in basin {0}", basin, crop, landscape, channel));
                                }
                                var soilRedistributeFactor = cropArea / (cropArea - soilAreaToRedistribute);
                                foreach (var slopeNumbers in soilSlopeNumbers.Values) {
                                    foreach (var hru in slopeNumbers.Values) {
                                        cellData = lsuData.hruMap[hru];
                                        cellData.multiply(soilRedistributeFactor);
                                        lsuData.hruMap[hru] = cellData;
                                    }
                                }
                            }
                        }
                        // Now we remove the slopes for each remaining crop/soil combination
                        // that fall below minSlopePercent.
                        foreach (var (crop, soilSlopeNumbers) in lsuData.cropSoilSlopeNumbers) {
                            foreach (var (soil, slopeNumbers) in soilSlopeNumbers) {
                                // first calculate area for the soil
                                soilArea = 0.0;
                                foreach (var hru in slopeNumbers.Values) {
                                    cellData = lsuData.hruMap[hru];
                                    soilArea += cellData.area;
                                }
                                minArea = soilArea * minSlopePercent / 100;
                                var slopeAreas = lsuData.cropSoilSlopeAreas(crop, soil);
                                // reduce minArea if necessary to avoid removing all slopes for this crop and soil
                                minArea = Math.Min(minArea, CreateHRUs.maxValue(slopeAreas));
                                var slopeAreaToRedistribute = 0.0;
                                // Make list as we will remove domain elements from original
                                foreach (var (slope, hru) in slopeNumbers.ToList()) {
                                    // first calculate the area for this slope
                                    var slopeArea = slopeAreas[slope];
                                    if (slopeArea < minArea) {
                                        // add to area to redistribute
                                        slopeAreaToRedistribute += slopeArea;
                                        // remove hru
                                        lsuData.removeHRU(hru, crop, soil, slope);
                                    }
                                }
                                if (slopeAreaToRedistribute > 0) {
                                    // Now redistribute removed slope areas
                                    // just to make sure we don't divide by zero
                                    if (Math.Abs(soilArea - slopeAreaToRedistribute) < this._gv.cellArea) {
                                        if (exemptWater) {
                                            // all rest is reservoir or pond
                                            lsuData.hruMap = new Dictionary<int, CellData>();
                                            lsuData.cropSoilSlopeNumbers = new Dictionary<int, Dictionary<int, Dictionary<int, int>>>();
                                            continue;
                                        }
                                        throw new InvalidDataException(string.Format("No slope data for landuse {1} and soil {2} in channel {3} landscape {4} in basin {0}", basin, crop, soil, channel, landscape));
                                    }
                                    var slopeRedistributeFactor = soilArea / (soilArea - slopeAreaToRedistribute);
                                    foreach (var hru in slopeNumbers.Values) {
                                        cellData = lsuData.hruMap[hru];
                                        cellData.multiply(slopeRedistributeFactor);
                                        lsuData.hruMap[hru] = cellData;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // 
        //         Remove HRUs that are below the minCropVal, minSoilVal, 
        //         or minSlopeVal, where the values are areas in hectares.
        // 
        //         Remove from basins data HRUs that are below the minCropVal,
        //         minSoilVal, or minSlopeVal, where the values are areas, and 
        //         redistribute their areas in proportion to the other HRUs.  
        //         Do not remove the last HRU.  
        //         Crop, soil, and slope nodata cells are also redistributed, 
        //         so the total area of the retained HRUs should eventually be the total
        //         area of the subbasin.
        //         
        public virtual void removeSmallHRUsByThresholdArea() {
            double minCropArea;
            // convert threshold areas to square metres
            var minCropAreaBasin = this.landuseVal * 10000;
            var minSoilAreaBasin = this.soilVal * 10000;
            var minSlopeAreaBasin = this.slopeVal * 10000;
            foreach (var (basin, basinData) in this.basins) {
                foreach (var (channel, channelData) in basinData.getLsus()) {
                    foreach (var (landscape, lsuData) in channelData) {
                        var exemptWater = lsuData.waterBody is not null && !lsuData.waterBody.isUnknown();
                        var cropAreas = lsuData.originalCropAreas;
                        if (exemptWater && cropAreas.ContainsKey(this._gv.db.waterLanduse)) {
                            cropAreas.Remove(this._gv.db.waterLanduse);
                            if (cropAreas.Count == 0) {
                                // all reservoir or pond
                                lsuData.hruMap = new Dictionary<int, CellData>();
                                lsuData.cropSoilSlopeNumbers = new Dictionary<int, Dictionary<int, Dictionary<int, int>>>();
                                continue;
                            }
                        }
                        // reduce area if necessary to avoid removing all crops
                        if (!this.hasExemptCrop(lsuData)) {
                            minCropArea = Math.Min(minCropAreaBasin, CreateHRUs.maxValue(cropAreas));
                        } else {
                            minCropArea = minCropAreaBasin;
                        }
                        var hrusArea = lsuData.area;
                        if (exemptWater) {
                            hrusArea -= lsuData.waterBody.originalArea;
                        }
                        var areaToRedistribute = 0.0;
                        foreach (var (crop, area) in cropAreas) {
                            if (!this._gv.isExempt(crop)) {
                                if (area < minCropArea) {
                                    // remove this crop
                                    // going to change maps so make lists
                                    Dictionary<int, Dictionary<int, int>> soilSlopeNumbers;
                                    if (lsuData.cropSoilSlopeNumbers.TryGetValue(crop, out soilSlopeNumbers)) {
                                        foreach (var (soil, slopeNumbers) in soilSlopeNumbers.ToList()) {
                                            foreach (var (slope, hru) in slopeNumbers.ToList()) {
                                                if (lsuData.hruMap.Count == 1) {
                                                    break;
                                                }
                                                areaToRedistribute += lsuData.hruMap[hru].area;
                                                lsuData.removeHRU(hru, crop, soil, slope);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        // Now have to remove soil areas that are
                        // less than minSoilArea
                        var soilAreas = lsuData.originalSoilAreas;
                        // reduce area if necessary to avoid removing all soils
                        var minSoilArea = Math.Min(minSoilAreaBasin, CreateHRUs.maxValue(soilAreas));
                        foreach (var (soil, area) in soilAreas) {
                            if (area < minSoilArea) {
                                // remove this soil
                                // going to change maps so make lists
                                foreach (var (crop, soilSlopeNumbers) in lsuData.cropSoilSlopeNumbers.ToList()) {
                                    // guard against soil having gone by crop deletions earlier
                                    Dictionary<int, int> slopeNumbers;
                                    if (soilSlopeNumbers.TryGetValue(soil, out slopeNumbers)) {
                                        foreach (var (slope, hru) in slopeNumbers.ToList()) {
                                            if (lsuData.hruMap.Count == 1) {
                                                break;
                                            }
                                            areaToRedistribute += lsuData.hruMap[hru].area;
                                            lsuData.removeHRU(hru, crop, soil, slope);
                                        }
                                    }
                                }
                            }
                        }
                        // Now we remove the slopes that are less than minSlopeArea
                        var slopeAreas = lsuData.originalSlopeAreas;
                        // reduce area if necessary to avoid removing all slopes
                        var minSlopeArea = Math.Min(minSlopeAreaBasin, CreateHRUs.maxValue(slopeAreas));
                        foreach (var (slope, area) in slopeAreas) {
                            if (area < minSlopeArea) {
                                // remove this slope
                                // going to change maps so make lists
                                foreach (var (crop, soilSlopeNumbers) in lsuData.cropSoilSlopeNumbers.ToList()) {
                                    foreach (var (soil, slopeNumbers) in soilSlopeNumbers.ToList()) {
                                        // guard against slope having gone by crop and soil deletions earlier
                                        int hru;
                                        if (slopeNumbers.TryGetValue(slope, out hru)) {
                                            if (lsuData.hruMap.Count == 1) {
                                                break;
                                            }
                                            areaToRedistribute += lsuData.hruMap[hru].area;
                                            lsuData.removeHRU(hru, crop, soil, slope);
                                        }
                                    }
                                }
                            }
                        }
                        if (areaToRedistribute > 0) {
                            // Now redistribute removed areas
                            // just to make sure we don't divide by zero
                            if (Math.Abs(hrusArea - areaToRedistribute) < this._gv.cellArea) {
                                if (exemptWater) {
                                    // all rest is reservoir or pond
                                    lsuData.hruMap = new Dictionary<int, CellData>();
                                    lsuData.cropSoilSlopeNumbers = new Dictionary<int, Dictionary<int, Dictionary<int, int>>>();
                                    continue;
                                }
                                throw new InvalidDataException(string.Format("Cannot redistribute area of {3} ha for channel {2} landscape {1} in basin {0}", basin, landscape, channel, areaToRedistribute / 10000));
                            }
                            var redistributeFactor = hrusArea / (hrusArea - areaToRedistribute);
                            lsuData.redistribute(redistributeFactor);
                        }
                    }
                }
            }
        }

        // Return true if landscape unit has an exempt crop.
        public virtual bool hasExemptCrop(LSUData lsuData) {
            if (lsuData.waterBody is not null && !lsuData.waterBody.isUnknown() && lsuData.cropSoilSlopeNumbers.ContainsKey(this._gv.db.waterLanduse)) {
                return true;
            }
            foreach (var crop in lsuData.cropSoilSlopeNumbers.Keys) {
                if (this._gv.isExempt(crop)) {
                    return true;
                }
            }
            return false;
        }

        // Return maximum value in map.
        public static double maxValue(Dictionary<int, double> mapv) {
            var maxm = 0.0;
            foreach (var val in mapv.Values) {
                if (val > maxm) {
                    maxm = val;
                }
            }
            return maxm;
        }

        // Return sum of double values in a map.
        public static double mapSum(Dictionary<int, double> mapv) {
            var total = 0.0;
            foreach (var val in mapv.Values) {
                total += val;
            }
            return total;
        }

        // Try to reduce the number of HRUs to targetVal, 
        //         removing them in increasing order of size.
        //         
        //         Size is measured by area (if useArea is true) or by fraction
        //         of subbasin.
        //         The target may not be met if the order is by area and it would cause
        //         one or more landscape units to have no HRUs.
        //         The strategy is to make a list of all potential HRUs and their sizes 
        //         for which the landuses are not exempt, sort this list by increasing size, 
        //         and remove HRUs according to this list until the target is met.
        //         
        public virtual void removeSmallHRUsbyTarget() {
            // first make a list of (basin, hru, channel, landscape, crop, soil, slope, size) tuples
            var removals = new List<(int, int, int, int, int, int, int, double)>();
            foreach (var (basin, basinData) in this.basins) {
                foreach (var (channel, channelData) in basinData.getLsus()) {
                    foreach (var (landscape, lsuData) in channelData) {
                        var exemptWater = lsuData.waterBody is not null && !lsuData.waterBody.isUnknown();
                        if (exemptWater && Math.Round(Math.Abs(lsuData.cropSoilSlopeArea - lsuData.waterBody.originalArea)) < Math.Round(this._gv.cellArea)) {
                            // all reservoir or pond
                            lsuData.hruMap = new Dictionary<int, CellData>();
                            lsuData.cropSoilSlopeNumbers = new Dictionary<int, Dictionary<int, Dictionary<int, int>>>();
                            continue;
                        }
                        foreach (var (crop, soilSlopeNumbers) in lsuData.cropSoilSlopeNumbers) {
                            if (!this._gv.isExempt(crop) && !(crop == this._gv.db.waterLanduse && exemptWater)) {
                                foreach (var (soil, slopeNumbers) in soilSlopeNumbers) {
                                    foreach (var (slope, hru) in slopeNumbers) {
                                        var hruArea = lsuData.hruMap[hru].area;
                                        double size = 0;
                                        if (this.useArea) {
                                            size = hruArea;
                                        } else if (lsuData.cropSoilSlopeArea == 0) {
                                            var SWATChannel = this._gv.topo.channelToSWATChannel[channel];
                                            Utils.error(string.Format("LSU {0} seems to have no land area", Utils.landscapeUnitId(SWATChannel, landscape)), this._gv.isBatch);
                                            size = 0.0;
                                        } else {
                                            size = hruArea / lsuData.cropSoilSlopeArea;
                                        }
                                        removals.Add((basin, hru, channel, landscape, crop, soil, slope, size));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            // sort by increasing size
            Func<(int, int, int, int, int, int, int, double), double> sortFun = item => item.Item8;
            removals = removals.OrderBy(sortFun).ToList();
            // remove HRUs
            // if some are exempt and target is small, can try to remove more than all in removals, so check for this
            // also make sure not negative since HRU count may have been reduced by addReservoirs
            var numToRemove = Math.Max(0, Math.Min(this.countHRUs() - this.targetVal, removals.Count));
            Utils.loginfo(string.Format("targetVal1: {0}", this.targetVal));
            Utils.loginfo(string.Format("HRUs count: {0}", this.countHRUs()));
            Utils.loginfo(string.Format("Possible removals: {0}", removals.Count));
            Utils.loginfo(string.Format("Num to remove: {0}", numToRemove));
            // print('targetVal1: {0}', self.targetVal))
            // print('HRUs count: {0}', self.countHRUs()))
            // print('Possible removals: {0}', len(removals)))
            // print('Num to remove: {0}', numToRemove))
            // numToRemoveStart = numToRemove
            foreach (var nextItem in removals) {
                if (numToRemove == 0) {
                    break;
                }
                if (this.removeHru(nextItem.Item1, nextItem.Item2, nextItem.Item3, nextItem.Item4, nextItem.Item5, nextItem.Item6, nextItem.Item7)) {
                    numToRemove -= 1;
                    // print('Number removed: {0}', numToRemoveStart - numToRemove))
                }
            }
        }

        // Remove an HRU and redistribute its area within its landscape unit.
        public virtual bool removeHru(
            int basin,
            int hru,
            int channel,
            int landscape,
            int crop,
            int soil,
            int slope) {
            var basinData = this.basins[basin];
            var lsuData = basinData.getLsus()[channel][landscape];
            // if channel in self._gv.topo.chLinkToWater and lsuData.waterBody is None and (landscape == Utils._NOLANDSCAPE or landscape == Utils._FLOODPLAIN):
            //     # need to create an empty water body since identified as pond or reservoir in inlets/outlets file
            //     # but no WATR pixels found.  May be reported later as zero area
            //     wid, _, role = self._gv.topo.chLinkToWater[channel]
            //     channelData = self._gv.topo.channelsData[channel]
            //     waterBody = WaterBody(0, 0, channelData.lowerZ, channelData.lowerX, channelData.lowerY)
            //     lsuData.waterBody = waterBody
            //     waterBody.id = wid
            //     waterBody.setInlet()  # prevent merging downstream
            //     if role == WaterRole._RESERVOIR:
            //         waterBody.setReservoir()
            //     else:
            //         waterBody.setPond()
            // water body needs to be preserved if it exists and has an identified role
            var exemptWater = lsuData.waterBody is not null && !lsuData.waterBody.isUnknown();
            var hrusArea = lsuData.area;
            if (exemptWater) {
                hrusArea -= lsuData.waterBody.originalArea;
            }
            //CreateHRUs.checkConsistent(lsuData, basin, channel, landscape, 4);  TODO
            var hrus = lsuData.hruMap;
            if (hrus.Count == 1) {
                // and not exemptWater:
                // last HRU - do not remove
                return false;
            }
            if (!hrus.ContainsKey(hru)) {
                var SWATBasin = this._gv.topo.subbasinToSWATBasin[basin];
                var SWATChannel = this._gv.topo.channelToSWATChannel[channel];
                var cropStr = this._gv.db.landuseCodes[crop];
                var soilStr = this._gv.db.soilNames[soil];
                var slopeStr = this._gv.db.slopeRange(slope);
                Utils.loginfo(string.Format("HRU {1} not found in basin {0} channel {2} landscape {3} crop {4} soil {5} slope {6}", SWATBasin, hru, SWATChannel, landscape, cropStr, soilStr, slopeStr));
                return false;
            }
            var areaToRedistribute = hrus[hru].area;
            lsuData.removeHRU(hru, crop, soil, slope);
            // if len(lsuData.hruMap) == 0:
            //     Utils.loginfo('LSU area for channel {0} is {1}', self._gv.topo.channelToSWATChannel.TryGetValue(channel, -1), lsuData.area))
            //     Utils.loginfo('HRUs area {0}', hrusArea))
            //     Utils.loginfo('Area to redistribute {0}', areaToRedistribute))
            if (areaToRedistribute > 0) {
                // make sure we don't divide by zero
                if (Math.Abs(hrusArea - areaToRedistribute) < this._gv.cellArea) {
                    if (exemptWater) {
                        // all rest is reservoir or pond
                        lsuData.hruMap = new Dictionary<int, CellData>();
                        lsuData.cropSoilSlopeNumbers = new Dictionary<int, Dictionary<int, Dictionary<int, int>>>();
                        return false;
                    }
                    int SWATBasin;
                    if (!this._gv.topo.subbasinToSWATBasin.TryGetValue(basin, out SWATBasin)) { SWATBasin = 0; }
                    int SWATChannel;
                    if (!this._gv.topo.channelToSWATChannel.TryGetValue(channel, out SWATChannel)) { SWATChannel = 0; }
                    throw new InvalidDataException(string.Format("No HRUs for channel {2} landscape {1} in subbasin {0}", SWATBasin, landscape, SWATChannel));
                }
                var redistributeFactor = hrusArea / (hrusArea - areaToRedistribute);
                lsuData.redistribute(redistributeFactor);
            }
            // CreateHRUs.checkConsistent(lsuData, basin, channel, landscape, 5); TODO
            return true;
        }

        // Split HRUs according to split landuses.
        public virtual bool splitHRUs() {
            int newhru;
            CellData cd1;
            Dictionary<int, int> newsn;
            Dictionary<int, Dictionary<int, int>> newssn;
            foreach (var (landuse, split) in this._gv.splitLanduses) {
                var crop = this._db.getLanduseCat(landuse);
                if (crop < 0) {
                    // error already reported
                    return false;
                }
                foreach (var basinData in this.basins.Values) {
                    foreach (var channelData in basinData.getLsus().Values) {
                        foreach (var lsuData in channelData.Values) {
                            Dictionary<int, Dictionary<int, int>> soilSlopeNumbers;
                            if (lsuData.cropSoilSlopeNumbers.TryGetValue(crop, out soilSlopeNumbers)) {
                                // have some hrus to split
                                // Make a new cropSoilSlopeNumbers map for the new crops
                                var newcssn = new Dictionary<int, Dictionary<int, Dictionary<int, int>>>();
                                foreach (var luse in split.Keys) {
                                    newssn = new Dictionary<int, Dictionary<int, int>>();
                                    var crop1 = this._db.getLanduseCat(luse);
                                    if (crop1 < 0) {
                                        // error already reported
                                        return false;
                                    }
                                    newcssn[crop1] = newssn;
                                }
                                // generally we will remove crop from cropSoilSlopeNumbers
                                // exception is when crop is named as a sublanduse of itself
                                var retainCrop = false;
                                foreach (var (soil, slopeNumbers) in soilSlopeNumbers) {
                                    // add soils to new dictionary
                                    foreach (var newssn0 in newcssn.Values) {
                                        newsn = new Dictionary<int, int>();
                                        newssn0[soil] = newsn;
                                    }
                                    foreach (var (slope, hru) in slopeNumbers) {
                                        var cd = lsuData.hruMap[hru];
                                        // remove hru from hruMap
                                        lsuData.hruMap.Remove(hru);
                                        foreach (var (subluse, percent) in split) {
                                            var subcrop = this._db.getLanduseCat(subluse);
                                            Dictionary<int, Dictionary<int, int>> oldssn;
                                            if (lsuData.cropSoilSlopeNumbers.TryGetValue(subcrop, out oldssn)) {
                                                // existing crop
                                                if (subcrop != crop) {
                                                    // add to an existing crop
                                                    // if have HRU with same soil and slope, add to it
                                                    var oldhru = -1;
                                                    Dictionary<int, int> sn;
                                                    if (oldssn.TryGetValue(soil, out sn)) {
                                                        if (!sn.TryGetValue(slope, out oldhru)) {
                                                            oldhru = -1;
                                                        }
                                                        if (oldhru >= 0) {
                                                            var oldcd = lsuData.hruMap[oldhru];
                                                            cd1 = new CellData(cd.cellCount, cd.area, cd.totalElevation, cd.totalSlope, cd.totalLongitude, cd.totalLatitude, crop);
                                                            cd1.multiply(percent / 100);
                                                            oldcd.addCells(cd1);
                                                        }
                                                    }
                                                    if (oldhru < 0) {
                                                        // have to add new HRU to existing crop
                                                        // keep original crop number in cell data
                                                        cd1 = new CellData(cd.cellCount, cd.area, cd.totalElevation, cd.totalSlope, cd.totalLongitude, cd.totalLatitude, crop);
                                                        cd1.multiply(percent / 100);
                                                        newhru = lsuData.nextHruNumber();
                                                        // add the new hru to hruMap
                                                        lsuData.hruMap[newhru] = cd1;
                                                        // add hru to existing data for this crop
                                                        Dictionary<int, int> oldsn;
                                                        if (!oldssn.TryGetValue(soil, out oldsn)) {
                                                            oldsn = new Dictionary<int, int>();
                                                            oldssn[soil] = oldsn;
                                                        }
                                                        oldsn[slope] = newhru;
                                                    }
                                                } else {
                                                    // reducing an existing crop
                                                    // replace with same hru number
                                                    // but don't change cd as being used in other code branches
                                                    cd1 = new CellData(cd.cellCount, cd.area, cd.totalElevation, cd.totalSlope, cd.totalLongitude, cd.totalLatitude, crop);
                                                    cd1.multiply(percent / 100);
                                                    lsuData.hruMap[hru] = cd1;
                                                    retainCrop = true;
                                                }
                                            } else {
                                                // the subcrop is new to the basin
                                                // keep original crop number in cell data
                                                cd1 = new CellData(cd.cellCount, cd.area, cd.totalElevation, cd.totalSlope, cd.totalLongitude, cd.totalLatitude, crop);
                                                cd1.multiply(percent / 100);
                                                newhru = lsuData.nextHruNumber();
                                                // add the new hru to hruMap
                                                lsuData.hruMap[newhru] = cd1;
                                                // add slope and hru number to new dictionary
                                                newssn = newcssn[subcrop];
                                                newsn = newssn[soil];
                                                newsn[slope] = newhru;
                                            }
                                        }
                                    }
                                }
                                // remove crop from cropSoilSlopeNumbers unless to be retained
                                if (!retainCrop) {
                                    lsuData.cropSoilSlopeNumbers.Remove(crop);
                                }
                                // add new cropSoilSlopeNumbers to original
                                foreach (var (newcrop, newssn0) in newcssn) {
                                    // existing subcrops already dealt with
                                    if (!lsuData.cropSoilSlopeNumbers.ContainsKey(newcrop)) {
                                        lsuData.cropSoilSlopeNumbers[newcrop] = newssn0;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }

        // Write topographic report file.
        public virtual void writeTopoReport() {
            var topoPath = Utils.join(this._gv.textDir, Parameters._TOPOREPORT);
            var line = "------------------------------------------------------------------------------";
            using (var fw = new StreamWriter(topoPath)) {
                fw.WriteLine("");
                fw.WriteLine(line);
                fw.WriteLine(Utils.trans("Elevation report for the watershed".PadRight(40) + Utils.date() + " " + Utils.time()));
                fw.WriteLine("");
                fw.WriteLine(line);
                this.writeTopoReportSection(this.elevMap, fw, "watershed");
                fw.WriteLine(line);
                if (!this._gv.useGridModel) {
                    foreach (var SWATBasin in this._gv.topo.SWATBasinToSubbasin.Keys.OrderBy(_p_1 => _p_1)) {
                        fw.WriteLine(string.Format("Subbasin {0}", SWATBasin));
                        int basin = this._gv.topo.SWATBasinToSubbasin[SWATBasin];
                        List<int> mapp;
                        List<(double, double, double)> bands;
                        if (this.basinElevMap.TryGetValue(basin, out mapp)) {
                            try {
                                bands = this.writeTopoReportSection(mapp, fw, "subbasin");
                            }
                            catch (Exception) {
                                Utils.exceptionError(string.Format("Internal error: cannot write topo report for SWAT basin {0} (basin {1})", SWATBasin, basin), this._gv.isBatch);
                                bands = null;
                            }
                        } else {
                            bands = null;
                        }
                        fw.WriteLine(line);
                        this.basinElevBands[SWATBasin] = bands;
                    }
                }
            }
            this._reportsCombo.Visible = true;
            if (this._reportsCombo.FindString(Parameters._TOPOITEM) < 0) {
                this._reportsCombo.Items.Add(Parameters._TOPOITEM);
            }
            this._db.writeElevationBands(this.basinElevBands, this._gv.numElevBands);
        }

        // Write topographic report file section for 1 subbasin.
        public virtual List<(double, double, double)> writeTopoReportSection(List<int> mapp, StreamWriter fw, string @string) {
            double nextBand = 0;
            List<(double, double, double)> bands;
            double bandWidth = 0;
            fw.WriteLine("");
            fw.WriteLine(Utils.trans("Statistics: All elevations reported in meters"));
            fw.WriteLine("-----------");
            fw.WriteLine("");
            var (minimum, maximum, totalFreq, mean, stdDev) = this.analyseElevMap(mapp);
            fw.WriteLine(Utils.trans("Minimum elevation: ").PadLeft(21) + (minimum + this.minElev).ToString());
            fw.WriteLine(Utils.trans("Maximum elevation: ").PadLeft(21) + (maximum + this.minElev).ToString());
            fw.WriteLine(Utils.trans("Mean elevation: ").PadLeft(21) + string.Format("{0:F2}", mean));
            fw.WriteLine(Utils.trans("Standard deviation: ").PadLeft(21) + string.Format("{0:F2}", stdDev));
            fw.WriteLine("");
            fw.Write(Utils.trans("Elevation").PadLeft(23));
            fw.Write(Utils.trans("% area up to elevation").PadLeft(32));
            fw.Write(Utils.trans("% area of ").PadLeft(14) + @string);
            fw.WriteLine("");
            var summ = 0.0;
            if (@string == "subbasin" && this._gv.elevBandsThreshold > 0 && this._gv.numElevBands > 0 && maximum + this.minElev > this._gv.elevBandsThreshold) {
                if (this._gv.isHUC || this._gv.isHAWQS) {
                    // HUC models use fixed bandwidth of 500m
                    bandWidth = 500;
                    this._gv.numElevBands = Convert.ToInt32(1 + (maximum - minimum) / bandWidth);
                    var thisWidth = Math.Min(maximum + 1 - minimum, bandWidth);
                    var midPoint = minimum + this.minElev + (thisWidth - 1) / 2.0;
                    bands = new List<(double, double, double)> {
                        (minimum + this.minElev, midPoint, 0.0)
                    };
                    nextBand = minimum + this.minElev + thisWidth;
                } else {
                    bandWidth = (maximum - minimum) / this._gv.numElevBands;
                    double midPoint = minimum + this.minElev + (bandWidth - 1) / 2.0;
                    bands = new List<(double, double, double)> {
                        (minimum + this.minElev, midPoint, 0)
                    };
                    nextBand = minimum + this.minElev + bandWidth;
                }
            } else {
                bands = null;
            }
            foreach (var i in Enumerable.Range(minimum, maximum + 1 - minimum)) {
                var freq = mapp[i];
                summ += freq;
                var elev = i + this.minElev;
                if (totalFreq == 0) {
                    throw new InvalidDataException(string.Format("Total frequency for {0} is zero", @string));
                }
                var upto = summ / totalFreq * 100.0;
                var percent = freq / totalFreq * 100.0;
                if (bands is not null) {
                    if (elev > nextBand) {
                        // start a new band
                        var nextWidth = Math.Min(maximum + 1 + this.minElev - elev, bandWidth);
                        var nextMidPoint = elev + (nextWidth - 1) / 2.0;
                        bands.Add((nextBand, nextMidPoint, percent));
                        nextBand = Math.Min(nextBand + bandWidth, maximum + 1 + this.minElev);
                    } else {
                        var (el, mid, frac) = bands[^1];
                        bands[^1] = (el, mid, frac + percent);
                    }
                }
                fw.Write(string.Format("{0:G}", elev).PadLeft(20));
                fw.Write(string.Format("{0:F2}", upto).PadLeft(25));
                fw.WriteLine(string.Format("{0:F2}", percent).PadLeft(25));
            }
            fw.WriteLine("");
            return bands;
        }

        // Calculate statistics from map elevation -> frequency.
        public virtual (int, int, int, double, double) analyseElevMap(List<int> mapp) {
            // find index of first non-zero frequency
            var minimum = 0;
            if (mapp is null || mapp.Count == 0) {
                return (0, 0, 0, 0, 0);
            }
            while (minimum < mapp.Count && mapp[minimum] == 0) {
                minimum += 1;
                }
            }
            // find index of last non-zero frequency
            var maximum = mapp.Count - 1;
            while (maximum >= 0 and mapp[maximum] == 0) {
                maximum -= 1;
                }
            }
            // calculate mean elevation and total of frequencies
            var summ = 0.0;
            var totalFreq = 0;
            foreach (var i in Enumerable.Range(minimum, maximum + 1 - minimum)) {
                var freq = mapp[i];
                summ += i * freq;
                totalFreq += freq;
            }
            // just to avoid dvision by zero
            if (totalFreq == 0) {
                return (minimum, maximum, 0, 0, 0);
            }
            var mapMean = summ / totalFreq;
            var mean = mapMean + this.minElev;
            var variance = 0.0;
            foreach (var i in Enumerable.Range(minimum, maximum + 1 - minimum)) {
                var diff = i - mapMean;
                variance += diff * diff * mapp[i];
            }
            var stdDev = Math.Sqrt(variance / totalFreq);
            return (minimum, maximum, totalFreq, mean, stdDev);
        }

        // 
        //         Print report on crops, soils, and slopes for watershed.
        //         
        //         Also assigns HRU numbers and writes gis_hrus table if withHRUs.
        //         
        public async Task printBasins(bool withHRUs) {
            var fileName = withHRUs ? Parameters._HRUSREPORT : Parameters._BASINREPORT;
            var path = Utils.join(this._gv.textDir, fileName);
            using (var fw = new StreamWriter(path)) {
                string horizLine = "------------------------------------------------------------------------------";
                if (withHRUs) {
                    horizLine = "-----------------------------------------------------------------------------------------------------------";
                }
                var landscapeString = this._gv.useLandscapes ? "Landscape/" : "";
                var HRUString = withHRUs ? "and HRU " : "";
                fw.WriteLine(string.Format("{0}Landuse/Soil/Slope {1}Distribution", landscapeString, HRUString).PadRight(57) + Utils.date() + " " + Utils.time());
                fw.WriteLine("");
                string units = "";
                if (this._gv.useGridModel) {
                    fw.WriteLine("Grid model");
                } else if (this.channelMergeThreshold > 0) {
                    units = this.channelMergeByPercent ? "%" : " ha";
                    fw.WriteLine(string.Format("Short channel merge threshold {0:d}{1}", this.channelMergeThreshold, units));
                } else {
                    fw.WriteLine("No short channel merge");
                }
                if (withHRUs) {
                    if (this.isDominantHRU) {
                        fw.WriteLine("Dominant HRU option");
                        if (this._gv.isBatch) {
                            Utils.information("Dominant HRU option", true);
                        }
                    } else if (!this.isMultiple) {
                        fw.WriteLine("Dominant Landuse/Soil/Slope option");
                        if (this._gv.isBatch) {
                            Utils.information("Dominant Landuse/Soil/Slope option", true);
                        }
                    } else {
                        // multiple
                        string method = "";
                        if (this.useArea) {
                            method = "Using area in hectares";
                            units = "ha";
                        } else {
                            method = "Using percentage of subbasin";
                            units = "%";
                        }
                        var line1 = "";
                        var line2 = "";
                        if (this.isTarget) {
                            line1 = method + " as a measure of size";
                            line2 = "Target number of HRUs option".PadRight(47) + string.Format("Target {0}", this.targetVal);
                        } else if (this.isArea) {
                            line1 = method + " as threshold";
                            line2 = "Multiple HRUs Area option".PadRight(47) + string.Format("Threshold: {0:d} {1}", this.areaVal, units);
                        } else {
                            line1 = method + " as a threshold";
                            line2 = "Multiple HRUs Landuse/Soil/Slope option".PadRight(47) + string.Format("Thresholds: {0:d}/{1:d}/{2:d} [{3}]", this.landuseVal, this.soilVal, this.slopeVal, units);
                        }
                        fw.WriteLine(line1);
                        if (this._gv.isBatch) {
                            Utils.information(line1, true);
                        }
                        fw.WriteLine(line2);
                        if (this._gv.isBatch) {
                            Utils.information(line2, true);
                        }
                    }
                }
                fw.WriteLine(string.Format("Number of subbasins: {0}", this.basins.Count));
                fw.WriteLine(string.Format("Number of channels: {0}", this.countChannels()));
                fw.WriteLine(string.Format("Number of LSUs: {0}", this.countLsus()));
                var (numRes, numPond, numWetland, numPlaya) = this.countLakes();
                var numLakes = numRes + numPond + numWetland + numPlaya;
                if (numLakes > 0) {
                    fw.WriteLine(string.Format("Number of lakes: {0}: {1} reservoirs, {2} ponds, {3} wetlands, {4} playas", numLakes, numRes, numPond, numWetland, numPlaya));
                }
                if (withHRUs) {
                    fw.WriteLine(string.Format("Number of HRUs: {0}", this.countHRUs()));
                }
                if (withHRUs && this.isMultiple) {
                    if (this._gv.exemptLanduses.Count > 0) {
                        fw.Write("Landuses exempt from thresholds: ");
                        foreach (var landuse in this._gv.exemptLanduses) {
                            fw.Write(landuse.PadLeft(6));
                        }
                        fw.WriteLine("");
                    }
                    if (this._gv.splitLanduses.Count > 0) {
                        fw.WriteLine("Split landuses: ");
                        foreach (var (landuse, splits) in this._gv.splitLanduses) {
                            fw.Write(landuse.PadLeft(6));
                            fw.Write(" split into ");
                            foreach (var (use, percent) in splits) {
                                fw.Write(string.Format("{0} : {1}%  ", use, percent));
                            }
                            fw.WriteLine("");
                        }
                    }
                }
                var lakesArea = this.totalLakesArea();
                var waterBodiesArea = this.totalWaterBodiesArea();
                if (lakesArea > 0) {
                    fw.WriteLine("");
                    if (lakesArea > 0) {
                        fw.WriteLine("Area of watershed includes lakes that are reservoirs, ponds or wetlands; other areas do not include such lakes.");
                        //                     subsString = ''
                        //                 else:
                        //                     subsString = 'watershed, '
                        //                 if waterBodiesArea > 0:
                        //                     fw.WriteLine('Areas of {0}subbasins, landscape units and channels include ponds and reservoirs.', subsString))
                        //                     hruString = ', slope ranges and HRUs' if withHRUs else ' and slope ranges'
                        //                     fw.WriteLine('Areas of landuses, soils{0} exclude ponds and reservoirs.', hruString))
                    }
                }
                if (withHRUs) {
                    fw.WriteLine("");
                    fw.WriteLine("Numbers in parentheses are corresponding values before HRU creation");
                }
                fw.WriteLine("");
                fw.WriteLine(horizLine);
                var st1 = "Area [ha]";
                var st2 = "%Watershed";
                int col2just = withHRUs ? 33 : 18;
                fw.WriteLine(st1.PadLeft(45));
                var basinHa = (this.totalBasinsArea() + lakesArea) / 10000.0;
                Debug.Assert(basinHa > 0, "Watershed seems to have no area");
                fw.WriteLine("Watershed" + string.Format("{0:F2}", basinHa).PadLeft(36));
                fw.WriteLine(horizLine);
                fw.WriteLine(st1.PadLeft(45) + st2.PadLeft(col2just));
                fw.WriteLine("");
                if (this._gv.useLandscapes) {
                    fw.WriteLine("Landscape units");
                    var landscapeAreas = this.totalLandscapeAreas();
                    this.printLandscapeAreas(landscapeAreas, withHRUs, basinHa, 0, fw);
                    fw.WriteLine("");
                }
                fw.WriteLine("Landuse");
                var (cropAreas, originalCropAreas) = this.totalCropAreas(withHRUs);
                this.printCropAreas(cropAreas, originalCropAreas, basinHa, 0, fw);
                fw.WriteLine("");
                fw.WriteLine("Soil");
                var (soilAreas, originalSoilAreas) = this.totalSoilAreas(withHRUs);
                this.printSoilAreas(soilAreas, originalSoilAreas, basinHa, 0, fw);
                fw.WriteLine("");
                fw.WriteLine("Slope");
                var (slopeAreas, originalSlopeAreas) = this.totalSlopeAreas(withHRUs);
                this.printSlopeAreas(slopeAreas, originalSlopeAreas, basinHa, 0, fw);
                if (lakesArea > 0) {
                    fw.WriteLine("");
                    this.printLakeArea((lakesArea, ""), basinHa, 0, withHRUs, fw);
                }
                if (waterBodiesArea > 0) {
                    fw.WriteLine("");
                    this.printWaterArea(waterBodiesArea, basinHa, 0, withHRUs, fw);
                }
                fw.WriteLine(horizLine);
                fw.WriteLine(horizLine);
                if (lakesArea > 0) {
                    st1 = "Area [ha]";
                    st2 = "%Watershed";
                    int just2 = withHRUs ? 33 : 18;
                    fw.WriteLine(st1.PadLeft(45) + st2.PadLeft(just2));
                    var lakeAreas = this.getLakeAreas();
                    foreach (var n in lakeAreas.Keys.OrderBy(_p_1 => _p_1)) {
                        this.printLakeArea(lakeAreas[n], basinHa, n, withHRUs, fw);
                    }
                    fw.WriteLine(horizLine);
                }
                if (withHRUs) {
                    this._dlg.progress("Writing channels table ...");
                    await this._gv.topo.writeChannelsTable(this.mergedChannels, this.basins, this._gv);
                    var demLayer = Utils.getLayerByLegend(FileTypes.legend(FileTypes._DEM)) as RasterLayer;
                    this._dlg.progress("Writing points, routing and hrus tables ...");
                    await this._gv.topo.writePointsTable(demLayer, this.mergees, this._gv.useGridModel, this._gv.existingWshed);
                    if (!this._db.createRoutingTable()) {
                        Utils.error("Failed to create table gis_routing in project database", this._gv.isBatch);
                        return;
                    }
                    var time1 = DateTime.Now;
                    // list of channel, pointId pairs for extra points at outlet of channel between channel
                    // and reservoir it flows into
                    var extraPoints = new List<(int, int)>();
                    if (!this._gv.topo.routeChannelsOutletsAndBasins(this.basins, this.mergedChannels, this.mergees, extraPoints, this._gv)) {
                        Utils.error("Failed to route channels and subbasins in table gis_routing in project database", this._gv.isBatch);
                        return;
                    }
                    this._gv.topo.addExtraPointsToPointsTable(extraPoints, this._gv.useGridModel);
                    using (var transaction = this._db.conn.BeginTransaction()) {
                        using (var cmd = new SQLiteCommand("", this._db.conn, transaction)) {
                            var table = "gis_hrus";
                            cmd.CommandText = "DROP TABLE IF EXISTS " + table;
                            this._db.execCmdQuery(cmd);
                            cmd.CommandText = DBUtils._HRUSCREATESQL;
                            this._db.execCmdQuery(cmd);
                            await this.printBasinsDetails(basinHa, true, fw, horizLine, cmd);
                            this._dlg.progress("");
                            var time2 = DateTime.Now;
                            Utils.loginfo(string.Format("Writing gis_points, gis_hrus and gis_routing tables took {0} seconds", Convert.ToInt32(time2.Subtract(time1).TotalSeconds)));
                            //this._db.hashDbTable(conn, "gis_points");
                            //this._db.hashDbTable(conn, "gis_hrus");
                            //this._db.hashDbTable(conn, "gis_routing");
                            transaction.Commit();
                        }
                    }
                } else {
                    await this.printBasinsDetails(basinHa, false, fw, horizLine, null);
                }
            }
            this._reportsCombo.Visible = true;
            if (withHRUs) {
                if (this._reportsCombo.FindString(Parameters._HRUSITEM) < 0) {
                    this._reportsCombo.Items.Add(Parameters._HRUSITEM);
                }
            } else if (this._reportsCombo.FindString(Parameters._BASINITEM) < 0) {
                this._reportsCombo.Items.Add(Parameters._BASINITEM);
            }
        }

        // Print report on crops, soils, and slopes for subbasin.
        public async Task printBasinsDetails(
            double basinHa,
            bool withHRUs,
            StreamWriter fw,
            string horizLine,
            SQLiteCommand cmd = null) {
            string msg;
            string subMsg;
            bool hasCrop;
            OSGeo.OGR.DataSource fullHRUsDs = null;
            OSGeo.OGR.Layer fullHRUsLayer = null;
            if (withHRUs && this.fullHRUsWanted) {
                if (File.Exists(this._gv.fullHRUsFile)) {
                    fullHRUsDs = Ogr.Open(this._gv.fullHRUsFile, 1);
                    fullHRUsLayer = fullHRUsDs.GetLayerByIndex(0);
                }
            }
            var setHRUS = withHRUs && fullHRUsLayer is not null;
            int subIndx = 0;
            int chIndx = 0;
            int catIndx = 0;
            int luseIndx = 0;
            int soilIndx = 0;
            int slopeIndx = 0;
            int areaIndx = 0;
            int linkIndx = 0;
            int hrusIndx = 0;
            if (setHRUS) {
                Debug.Assert(fullHRUsLayer is not null);
                subIndx = fullHRUsLayer.FindFieldIndex(Topology._SUBBASIN, 1);
                if (subIndx < 0) {
                    setHRUS = false;
                }
                chIndx = fullHRUsLayer.FindFieldIndex(Topology._CHANNEL, 1);
                if (chIndx < 0) {
                    setHRUS = false;
                }
                catIndx = fullHRUsLayer.FindFieldIndex(Topology._LANDSCAPE, 1);
                if (catIndx < 0) {
                    setHRUS = false;
                }
                luseIndx = fullHRUsLayer.FindFieldIndex(Parameters._LANDUSE, 1);
                if (luseIndx < 0) {
                    setHRUS = false;
                }
                soilIndx = fullHRUsLayer.FindFieldIndex(Parameters._SOIL, 1);
                if (soilIndx < 0) {
                    setHRUS = false;
                }
                slopeIndx = fullHRUsLayer.FindFieldIndex(Parameters._SLOPEBAND, 1);
                if (slopeIndx < 0) {
                    setHRUS = false;
                }
                areaIndx = fullHRUsLayer.FindFieldIndex(Parameters._AREA, 1);
                if (areaIndx < 0) {
                    setHRUS = false;
                }
                hrusIndx = fullHRUsLayer.FindFieldIndex(Topology._HRUS, 1);
                if (hrusIndx < 0) {
                    setHRUS = false;
                }
                linkIndx = fullHRUsLayer.FindFieldIndex(Topology._LINKNO, 1);
                if (linkIndx < 0) {
                    setHRUS = false;
                }
            }
            //if (setHRUS) {
            //    Debug.Assert(fullHRUsLayer is not null);
            //    OK = fullHRUsLayer.startEditing();
            //    if (!OK) {
            //        Utils.error("Cannot edit FullHRUs shapefile", this._gv.isBatch);
            //        setHRUS = false;
            //    }
            //}

            // set HRUS field for all shapes for this basin to NA
            // and reset channels to originals
            // (in case rerun with different HRU settings or different merges)
            if (setHRUS) {
                Debug.Assert(fullHRUsLayer is not null);
                this.clearHRUSNums(fullHRUsLayer, hrusIndx, chIndx, linkIndx);
            }
            var noLanduseReported = false;
            var noSoilReported = false;
            foreach (var SWATBasin in this._gv.topo.SWATBasinToSubbasin.Keys.OrderBy(_p_1 => _p_1)) {
                var basin = this._gv.topo.SWATBasinToSubbasin[SWATBasin];
                if (this._gv.useGridModel) {
                    var chLink = this._gv.topo.chBasinToChLink[basin];
                    if (this._gv.topo.chLinkInsideLake.ContainsKey(chLink) || this._gv.topo.chLinkFromLake.ContainsKey(chLink)) {
                        continue;
                    }
                }
                BasinData basinData;
                if (!this.basins.TryGetValue(basin, out basinData)) {
                    basinData = null;
                }
                if (basinData is null) {
                    if (this._gv.isHUC || this._gv.isHAWQS) {
                        Utils.information(string.Format("WARNING: Subbasin {0} has no data", SWATBasin), this._gv.isBatch);
                    } else {
                        Utils.error(string.Format("Subbasin {0} has no data", SWATBasin), this._gv.isBatch);
                    }
                    break;
                }
                var subHa = basinData.subbasinArea() / 10000.0;
                //assert subHa > 0, 'SWAT basin {0} seems to be empty', SWATBasin)
                // basinHa earlier asserted to be positive
                var percent = subHa / basinHa * 100;
                var st1 = "Area [ha]";
                var st2 = "%Watershed";
                var st3 = "%Subbasin";
                var col2just = withHRUs ? 33 : 18;
                var col3just = withHRUs ? 23 : 15;
                if (withHRUs) {
                    var st4 = "%Landscape unit";
                    var col4just = 23;
                    fw.WriteLine(st1.PadLeft(45) + st2.PadLeft(col2just) + st3.PadLeft(col3just) + st4.PadLeft(col4just));
                } else {
                    fw.WriteLine(st1.PadLeft(45) + st2.PadLeft(col2just) + st3.PadLeft(col3just));
                }
                fw.WriteLine("");
                fw.WriteLine(string.Format("Subbasin {0}", SWATBasin).PadRight(30) + string.Format("{0:F2}", subHa).PadLeft(15) + string.Format("{0:F2}", percent).PadLeft(col2just - 3));
                fw.WriteLine("");
                fw.WriteLine("Landuse");
                var originalCropAreas = basinData.cropAreas(true);
                var basinWaterBodiesArea = this.basinWaterBodiesArea(basinData);
                if (this._gv.isHUC || this._gv.isHAWQS) {
                    // avoid empty subbasin errors
                    noLanduseReported = true;
                    noSoilReported = true;
                }
                if (!withHRUs && originalCropAreas.Count == 0) {
                    hasCrop = false;
                    if (basinWaterBodiesArea == 0) {
                        subMsg = this._gv.useGridModel ? " (and perhaps others)" : "";
                        msg = string.Format("There is no landuse data for subbasin {0}{1}.  Check your landuse map.", SWATBasin, subMsg);
                        if (noLanduseReported) {
                            Utils.loginfo(msg);
                        } else {
                            Utils.error(msg, this._gv.isBatch);
                            noLanduseReported = !this._gv.isBatch;
                        }
                    }
                } else {
                    hasCrop = true;
                }
                this.printCropAreas(basinData.cropAreas(false), originalCropAreas, basinHa, subHa, fw);
                fw.WriteLine("");
                fw.WriteLine("Soil");
                var originalSoilAreas = basinData.soilAreas(true);
                // soilAreas will be empty if cropAreas is, so avoid knock-on message
                if (!withHRUs && hasCrop && originalSoilAreas.Count == 0) {
                    subMsg = this._gv.useGridModel ? " (and perhaps others)" : "";
                    msg = string.Format("There is no soil data for subbasin {0}{1}.  Check your soil map.", SWATBasin, subMsg);
                    if (noSoilReported) {
                        Utils.loginfo(msg);
                    } else {
                        Utils.error(msg, this._gv.isBatch);
                        noSoilReported = !this._gv.isBatch;
                    }
                }
                this.printSoilAreas(basinData.soilAreas(false), originalSoilAreas, basinHa, subHa, fw);
                fw.WriteLine("");
                fw.WriteLine("Slope");
                this.printSlopeAreas(basinData.slopeAreas(false), basinData.slopeAreas(true), basinHa, subHa, fw);
                fw.WriteLine("");
                if (basinWaterBodiesArea > 0) {
                    this.printWaterArea(basinWaterBodiesArea, basinHa, subHa, withHRUs, fw);
                    fw.WriteLine("");
                }
                if (withHRUs) {
                    fw.WriteLine("HRUs:");
                }
                this.printChannelHRUs(SWATBasin, basinData, withHRUs, basinHa, subHa, fw, cmd);
                fw.WriteLine(horizLine);
            }
            if (setHRUS) {
                Debug.Assert(fullHRUsLayer is not null);
                this.addHRUSNums(fullHRUsLayer, subIndx, chIndx, catIndx, luseIndx, soilIndx, slopeIndx, hrusIndx, linkIndx);
                fullHRUsLayer.SyncToDisk();
                fullHRUsDs.FlushCache();
                await this.writeActHRUs(subIndx, chIndx, catIndx, areaIndx, hrusIndx);
            }
        }

        // Print channel number, landscape and (if with HRUs) HRUs for each channel.
        public virtual void printChannelHRUs(
            int SWATBasin,
            BasinData basinData,
            bool withHRUs,
            double basinHa,
            double subHa,
            StreamWriter fw,
            SQLiteCommand cmd) {
            foreach (var (channel, channelData) in basinData.getLsus()) {
                var SWATChannel = this._gv.topo.channelToSWATChannel[channel];
                var landscapeAreas = channelData.ToDictionary(landscape => landscape.Key, landscape => channelData[landscape.Key].area);
                this.printChannelArea(SWATChannel, CreateHRUs.mapSum(landscapeAreas), withHRUs, basinHa, subHa, fw);
                this.printLandscapeAreas(landscapeAreas, withHRUs, basinHa, subHa, fw);
                if (withHRUs) {
                    this.printLandscapeHRUs(SWATBasin, SWATChannel, channelData, basinHa, subHa, fw, cmd);
                    fw.WriteLine("");
                }
            }
        }

        // Print HRUs for each landscape.
        public virtual void printLandscapeHRUs(
            int SWATBasin,
            int SWATChannel,
            Dictionary<int, LSUData> channelData,
            double basinHa,
            double subHa,
            StreamWriter fw,
            SQLiteCommand cmd) {
            string lCat;
            LakeData lakeData;
            int lake;
            var aquifer = 10 * SWATBasin;
            var downAquifer = 10 * SWATBasin + Utils._FLOODPLAIN;
            var upAquifer = 10 * SWATBasin + Utils._UPSLOPE;
            foreach (var (landscape, lsuData) in channelData) {
                var lsuId = Utils.landscapeUnitId(SWATChannel, landscape);
                // identity of the floodplain water body if any
                var wid = 0;
                var wCat = "unknown";
                // treat as upslope if is upslope and floodplain LSU exists
                // as in grids may all be upslope
                var treatAsUpslope = landscape == Utils._UPSLOPE && channelData.ContainsKey(Utils._FLOODPLAIN);
                // true if this is upslope and the floodplain lsu is all water body
                var downAllWater = false;
                // Route LSU
                if (treatAsUpslope) {
                    var downLsuId = Utils.landscapeUnitId(SWATChannel, Utils._FLOODPLAIN);
                    var downLsuData = channelData[Utils._FLOODPLAIN];
                    // if floodplain LSU is all water, route into the water body
                    if (downLsuData.waterBody is not null && !downLsuData.waterBody.isUnknown() && downLsuData.cropSoilSlopeArea == downLsuData.waterBody.originalArea) {
                        downAllWater = true;
                        wid = downLsuData.waterBody.id;
                        wCat = downLsuData.waterBody.isReservoir() ? "RES" : "PND";
                        this._db.addToRouting(lsuId, "LSU", wid, wCat, Topology._TOTAL, 100, cmd);
                    } else {
                        // use Katrin's formula
                        var upslopePercent = Convert.ToInt32(100 * lsuData.cropSoilSlopeArea / (lsuData.cropSoilSlopeArea + downLsuData.cropSoilSlopeArea));
                        // make upslopePercent at least 1%, or percentage sum check algorithm fails with numbers of 0 and 100
                        upslopePercent = upslopePercent == 0 ? 1 : upslopePercent;
                        if (downLsuData.waterBody is not null && !downLsuData.waterBody.isUnknown()) {
                            wid = downLsuData.waterBody.id;
                            wCat = downLsuData.waterBody.isReservoir() ? "RES" : "PND";
                            if (downLsuData.cropSoilSlopeArea > downLsuData.waterBody.originalArea) {
                                // else all water
                                this._db.addToRouting(lsuId, "LSU", wid, wCat, Topology._SURFACE, upslopePercent, cmd);
                            }
                        } else {
                            lake = this._gv.topo.surroundingLake(SWATChannel, this._gv.useGridModel);
                            if (lake > 0) {
                                lakeData = this._gv.topo.lakesData[lake];
                                lCat = lakeData.waterRole == WaterRole._RESERVOIR ? "RES" : lakeData.waterRole == WaterRole._POND ? "PND" : "WETL";
                                this._db.addToRouting(lsuId, "LSU", lake, lCat, Topology._SURFACE, upslopePercent, cmd);
                            } else {
                                this._db.addToRouting(lsuId, "LSU", SWATChannel, "CH", Topology._SURFACE, upslopePercent, cmd);
                            }
                        }
                        this._db.addToRouting(lsuId, "LSU", downLsuId, "LSU", Topology._SURFACE, 100 - upslopePercent, cmd);
                        this._db.addToRouting(lsuId, "LSU", downLsuId, "LSU", Topology._LATERAL, 100, cmd);
                    }
                } else if (lsuData.waterBody is not null && !lsuData.waterBody.isUnknown()) {
                    wid = lsuData.waterBody.id;
                    wCat = lsuData.waterBody.isReservoir() ? "RES" : "PND";
                    if (lsuData.cropSoilSlopeArea > lsuData.waterBody.originalArea) {
                        // else all water
                        this._gv.db.addToRouting(lsuId, "LSU", wid, wCat, Topology._TOTAL, 100, cmd);
                    }
                } else {
                    lake = this._gv.topo.surroundingLake(SWATChannel, this._gv.useGridModel);
                    if (lake > 0) {
                        lakeData = this._gv.topo.lakesData[lake];
                        lCat = lakeData.waterRole == WaterRole._RESERVOIR ? "RES" : lakeData.waterRole == WaterRole._POND ? "PND" : "WETL";
                        this._db.addToRouting(lsuId, "LSU", lake, lCat, Topology._TOTAL, 100, cmd);
                    } else {
                        this._db.addToRouting(lsuId, "LSU", SWATChannel, "CH", Topology._TOTAL, 100, cmd);
                    }
                }
                // route recharge from LSU
                if (lsuData.waterBody is null || lsuData.waterBody.isUnknown() || lsuData.cropSoilSlopeArea > lsuData.waterBody.originalArea) {
                    // else all water
                    var aqu = landscape == Utils._UPSLOPE ? upAquifer : landscape == Utils._FLOODPLAIN ? downAquifer : aquifer;
                    this._db.addToRouting(lsuId, "LSU", aqu, "AQU", Topology._RECHARGE, 100, cmd);
                }
                if (this._gv.useLandscapes) {
                    var landscapeName = Utils.landscapeName(landscape, this._gv.useLeftRight);
                    fw.WriteLine(string.Format("{0} (LSU {1}):", landscapeName, lsuId));
                }
                this.printLSUHRUs(lsuId, SWATChannel, landscape, lsuData, channelData, wid, wCat, downAllWater, basinHa, subHa, fw, cmd);
            }
        }

        // Print and route HRUs and water bodies for an LSU.
        public virtual void printLSUHRUs(
            int lsuId,
            int SWATChannel,
            int landscape,
            LSUData lsuData,
            Dictionary<int, LSUData> channelData,
            int wid,
            string wCat,
            bool downAllWater,
            double basinHa,
            double subHa,
            StreamWriter fw,
            SQLiteCommand cmd) {
            string string1;
            double percent3;
            double percent2;
            double percent1;
            double meanMultiplier;
            LakeData lakeData;
            string lCat = "";
            var lake = this._gv.topo.surroundingLake(SWATChannel, this._gv.useGridModel);
            if (lake > 0) {
                lakeData = this._gv.topo.lakesData[lake];
                lCat = lakeData.waterRole == WaterRole._RESERVOIR ? "RES" : lakeData.waterRole == WaterRole._POND ? "PND" : "WETL";
            }
            var (floodTarget, floodCat) = lake > 0 ? (lake, lCat) : wid > 0 ? (wid, wCat) : (SWATChannel, "CH");
            var arlsuHa = lsuData.area / 10000.0;
            var routeWaterAsWaterBody = lsuData.waterBody is not null && !lsuData.waterBody.isUnknown();
            var zeroAreaReportCount = 0;
            foreach (var (crop, soilSlopeNumbers) in lsuData.cropSoilSlopeNumbers) {
                if (crop != this._gv.db.waterLanduse || !routeWaterAsWaterBody) {
                    var arluse = lsuData.cropAreas[crop] / 10000;
                    foreach (var (soil, slopeNumbers) in soilSlopeNumbers) {
                        var arso = lsuData.cropSoilArea(crop, soil) / 10000;
                        foreach (var (slope, landscape_hru) in slopeNumbers) {
                            this.HRUNum += 1;
                            var cellData = lsuData.hruMap[landscape_hru];
                            cellData.actHRUNum = this.HRUNum;
                            var luse = this._db.getLanduseCode(crop);
                            // replace WATR with WETW
                            if (luse.ToUpper() == "WATR") {
                                luse = "WETW";
                            }
                            var snam = this._db.getSoilName(soil).Item1;
                            var slp = this._db.slopeRange(slope);
                            var cropSoilSlope = luse + "/" + snam + "/" + slp;
                            // replace BARR with null
                            if (luse.ToUpper() == "BARR") {
                                luse = null;
                            }
                            // use cellArea rather than cellCount to calculate means
                            // cellCounts are integer and inaccurate (even zero) for small HRUs
                            if (cellData.area == 0) {
                                //Don't report more than 4
                                zeroAreaReportCount += 1;
                                if (zeroAreaReportCount > 4) { 
                                    Utils.error(string.Format("HRU {0} in LSU {1} (and perhaps others) has zero area", this.HRUNum, lsuId), this._gv.isBatch);
                                    return;
                                } else {
                                    Utils.error(string.Format("HRU {0} in LSU {1} has zero area", this.HRUNum, lsuId), this._gv.isBatch);
                                }
                                meanMultiplier = 1;
                            } else {
                                meanMultiplier = this._gv.cellArea / cellData.area;
                            }
                            var meanSlopePercent = cellData.totalSlope * meanMultiplier * 100;
                            var hruha = cellData.area / 10000;
                            var arslp = hruha;
                            fw.Write(this.HRUNum.ToString().PadRight(5) + cropSoilSlope.PadLeft(25) + string.Format("{0:F2}", hruha).PadLeft(15));
                            if (basinHa > 0) {
                                percent1 = hruha / basinHa * 100;
                                fw.Write(string.Format("{0:F2}", percent1).PadLeft(30));
                            }
                            if (subHa > 0) {
                                percent2 = hruha / subHa * 100;
                                fw.Write(string.Format("{0:F2}", percent2).PadLeft(23));
                            }
                            if (arlsuHa > 0) {
                                percent3 = hruha / arlsuHa * 100;
                                fw.Write(string.Format("{0:F2}", percent3).PadLeft(23));
                            }
                            fw.WriteLine("");
                            var meanElevation = cellData.totalElevation * meanMultiplier;
                            var centroid = new double[3] { cellData.totalLongitude * meanMultiplier, cellData.totalLatitude * meanMultiplier, 0 };
                            var centroidll = this._gv.topo.pointToLatLong(centroid);
                            var lat = centroidll[1];
                            if (double.IsNaN(lat)) {
                                Utils.error(string.Format("Cannot compute latitude for {0} on channel {1}", cellData.totalLatitude * meanMultiplier, SWATChannel), this._gv.isBatch);
                                lat = 0;
                            }
                            var lon = centroidll[0];
                            if (double.IsNaN(lon)) {
                                Utils.error(string.Format("Cannot compute longitude for {0} on channel {1}", cellData.totalLongitude * meanMultiplier, SWATChannel), this._gv.isBatch);
                                lon = 0;
                            }
                            this._db.execNonQuery(DBUtils._HRUSINSERTSQL, new object[] { this.HRUNum, lsuId, subHa, arlsuHa, luse, arluse, snam, arso, slp, arslp, meanSlopePercent, lat, lon, meanElevation });
                            // route HRU
                            if (landscape == Utils._NOLANDSCAPE || landscape == Utils._FLOODPLAIN || !channelData.ContainsKey(Utils._FLOODPLAIN)) {
                                // upslope with no floodplain; can happen in grid models
                                this._db.addToRouting(this.HRUNum, "HRU", floodTarget, floodCat, Topology._TOTAL, 100, cmd);
                            } else if (downAllWater) {
                                this._db.addToRouting(this.HRUNum, "HRU", floodTarget, floodCat, Topology._TOTAL, 100, cmd);
                            } else {
                                this._db.addToRouting(this.HRUNum, "HRU", floodTarget, floodCat, Topology._TOTAL, this._gv.upslopeHRUDrain, cmd);
                                var floodLsuId = Utils.landscapeUnitId(SWATChannel, Utils._FLOODPLAIN);
                                this._db.addToRouting(this.HRUNum, "HRU", floodLsuId, "LSU", Topology._TOTAL, 100 - this._gv.upslopeHRUDrain, cmd);
                            }
                        }
                    }
                }
            }
            var waterBody = lsuData.waterBody;
            if (waterBody is not null && routeWaterAsWaterBody) {
                // water area is original - prior to merging of reservoirs
                var waterHa = waterBody.originalArea / 10000;
                if (waterBody.isReservoir()) {
                    // reservoir cellCount is zero if it has been merged
                    if (waterBody.cellCount > 0) {
                        string1 = string.Format("Reservoir {0}", waterBody.id);
                    } else {
                        string1 = string.Format("Reservoir {0} (part)", waterBody.id);
                    }
                } else {
                    string1 = string.Format("Pond {0}", waterBody.id);
                }
                var string2 = string.Format("{0:F2}", waterHa);
                fw.Write(string1.PadRight(30) + string2.PadLeft(15));
                if (basinHa > 0) {
                    percent1 = waterHa / basinHa * 100;
                    fw.Write(string.Format("{0:F2}", percent1).PadLeft(30));
                }
                if (subHa > 0) {
                    percent2 = waterHa / subHa * 100;
                    fw.Write(string.Format("{0:F2}", percent2).PadLeft(23));
                }
                if (arlsuHa > 0) {
                    percent3 = waterHa / arlsuHa * 100;
                    fw.Write(string.Format("{0:F2}", percent3).PadLeft(23));
                }
                fw.WriteLine("");
                // for actual reservoir print total area (using area rather than originalArea)
                if (waterBody.isReservoir()) {
                    if (waterBody.cellCount > 0) {
                        var areaHa = waterBody.area / 10000;
                        fw.WriteLine("Total reservoir area:".PadLeft(30) + string.Format("{0:F2}", areaHa).PadLeft(15));
                        // reservoirs and ponds were routed with channels
                    }
                }
            }
            return;
        }

        // Set HRUS values to NA, and reset SWAT channel numbers to originals (before any channel merges).
        public void clearHRUSNums(OSGeo.OGR.Layer fullHRUsLayer, int hrusIndx, int chIndx, int linkIndx) {
            fullHRUsLayer.ResetReading();
            OSGeo.OGR.Feature feature = null;
            do {
                feature = fullHRUsLayer.GetNextFeature();
                if (feature != null) {
                    var chLink = feature.GetFieldAsInteger(linkIndx);
                    int SWATChannel;
                    if (!this._gv.topo.channelToSWATChannel.TryGetValue(linkIndx, out SWATChannel)) { SWATChannel = 0; }
                    feature.SetField(chIndx, SWATChannel);
                    feature.SetField(hrusIndx, "NA");
                }
            } while (feature != null);
        }


        // Add HRUS values to fullHRUsLayer for actual HRUs, and change channel field to reflect channel merges.
        //         
        //         Do not merge HRUs according to channel merges, so that the merges can be changed without having
        //         to recompute the geometries.  ActHRUs wil be given merged geometries for merged channels.
        public void addHRUSNums(
            OSGeo.OGR.Layer fullHRUsLayer,
            int subIndx,
            int chIndx,
            int catIndx,
            int luseIndx,
            int soilIndx,
            int slopeIndx,
            int hrusIndx,
            int linkIndx) {
            string hruNum;
            fullHRUsLayer.ResetReading();
            OSGeo.OGR.Feature feature = null;
            do {
                feature = fullHRUsLayer.GetNextFeature();
                if (feature != null) {
                    var fid = feature.GetFID();
                    var SWATBasin = feature.GetFieldAsInteger(subIndx);
                    var category = Utils.landscapeFromName(feature.GetFieldAsString(catIndx));
                    var origCropCode = feature.GetFieldAsString(luseIndx);
                    var soilName = feature.GetFieldAsString(soilIndx);
                    var slopeRange = feature.GetFieldAsString(slopeIndx);
                    var basin = this._gv.topo.SWATBasinToSubbasin[SWATBasin];
                    var basinData = this.basins[basin];
                    var channel = (int)feature.GetFieldAsInteger(linkIndx);
                    // FullHRUS file created before channel merging, so based on original channels, which might have been merged
                    var targetChannel = this._gv.topo.finalTarget(channel, this.mergedChannels);
                    var lsuData = basinData.getLsus()[targetChannel][category];
                    if (lsuData.waterBody is not null && origCropCode.ToUpper() == "WATR" && !lsuData.waterBody.isUnknown()) {
                        if (lsuData.waterBody.isReservoir()) {
                            hruNum = string.Format("RES {0}", lsuData.waterBody.id);
                        } else {
                            hruNum = string.Format("PND {0}", lsuData.waterBody.id);
                        }
                    } else {
                        hruNum = null;
                        foreach (var soilSlopeNumbers in lsuData.cropSoilSlopeNumbers.Values) {
                            foreach (var (soil, slopeNumbers) in soilSlopeNumbers) {
                                if (this._db.getSoilName(soil).Item1 == soilName) {
                                    foreach (var (slope, hru) in slopeNumbers) {
                                        var cellData = lsuData.hruMap[hru];
                                        if (this._db.getLanduseCode(cellData.crop) == origCropCode && this._db.slopeRange(slope) == slopeRange) {
                                            if (hruNum is null) {
                                                hruNum = cellData.actHRUNum.ToString();
                                            } else {
                                                hruNum = hruNum + string.Format(", {0}", cellData.actHRUNum);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (hruNum != null) {
                        feature.SetField(hrusIndx, hruNum);
                        if (targetChannel != channel) {
                            var targetSWATChannel = this._gv.topo.channelToSWATChannel[targetChannel];
                            feature.SetField(chIndx, targetSWATChannel);
                        }
                        if (fullHRUsLayer.SetFeature(feature) != Ogr.OGRERR_NONE) {
                            Utils.error("Cannot write to FullHRUs attribute table", this._gv.isBatch);
                            return;
                        }
                    }
                }
            } while (feature != null);
        }

        // Create and load the actual HRUs file, with deselected HRUs removed and HRUs merged according to channel merges.
        public virtual async Task writeActHRUs(
            int subIndx,
            int chIndx,
            int catIndx,
            int areaIndx,
            int hrusIndx) {
            var actHRUsBasename = Parameters._HRUS2;
            var actHRUsFilename = actHRUsBasename + ".shp";
            Utils.copyShapefile(this._gv.fullHRUsFile, actHRUsBasename, this._gv.shapesDir);
            var actHRUsFile = Utils.join(this._gv.shapesDir, actHRUsFilename);
            var fidsToBeMerged = new Dictionary<long, long>();
            var fidsToMerge = new HashSet<long>();
            using (var ds = Ogr.Open(actHRUsFile, 1)) {
                var actLayer = ds.GetLayerByIndex(0);
                this.findMerges(actLayer, hrusIndx, ref fidsToMerge, ref fidsToBeMerged);
                // now merge any merged channels, plus ponds and reservoirs
                if (!this.mergeActHRUs(actLayer, subIndx, chIndx, catIndx, areaIndx, hrusIndx, fidsToMerge, fidsToBeMerged)) {
                    Utils.error("Failed to complete actual HRUs shapefile", this._gv.isBatch);
                    return;
                }
            }
            // insert above FullHRUs in legend
            var fullLayer = Utils.getLayerByLegend(Utils._FULLHRUSLEGEND);
            await Utils.removeLayerByLegend(Utils._ACTHRUSLEGEND);
            var actHRUsLayer = (await Utils.getLayerByFilename(actHRUsFile, FileTypes._ACTHRUS, this._gv, fullLayer, Utils._WATERSHED_GROUP_NAME)).Item1 as FeatureLayer;
            // remove visibility from FullHRUs layer
            await Utils.setLayerVisibility(fullLayer, false);
            // copy actual HRUs file as template for visualisation
            this.createHrusResultsFile(actHRUsFile, actHRUsLayer);
        }

        // Build table of HRU fids to merge.
        public virtual void findMerges(OSGeo.OGR.Layer layer, int hrusIndx, ref HashSet<long> fidsToMerge, ref Dictionary<long, long> fidsToBeMerged) {
            var hrusToFid = new Dictionary<string, long>();
            layer.ResetReading();
            OSGeo.OGR.Feature feature;
            do {
                feature = layer.GetNextFeature();
                if (feature is null) { break; }
                var fid = feature.GetFID();
                var hrus = feature.GetFieldAsString(hrusIndx);
                if (hrus == "NA") {
                    continue;
                } else {
                    // check if this hrus field has already occurred
                    long fid1;
                    if (!hrusToFid.TryGetValue(hrus, out fid1)) {
                        hrusToFid[hrus] = fid;
                    } else {
                        // fid will be merged into fid1
                        fidsToMerge.Add(fid1);
                        fidsToBeMerged[fid] = fid1;
                    }
                }
            } while (feature is not null);
        }

        // Merge HRUs in ActHRUs file according to merged fids.  Also sets area and percent fields to reflect
        //         changes in these because of HRU removal and area reallocation.
        //         
        //         Must be run after addHRUSNums as depends on merging HRUs with same HRUS values,
        //         and after findMerges which makes fidsToMerge and fidsToBeMerged.
        public bool mergeActHRUs(
            OSGeo.OGR.Layer layer,
            int subIndx,
            int chIndx,
            int catIndx,
            int areaIndx,
            int hrusIndx,
            HashSet<long> fidsToMerge,
            Dictionary<long, long> fidsToBeMerged) {
            double percentLSU;
            double percentSub;
            var defn = layer.GetLayerDefn();
            var percentSubIndx = defn.GetFieldIndex(Parameters._PERCENTSUB);
            if (percentSubIndx < 0) {
                return false;
            }
            var percentLSUIndx = defn.GetFieldIndex(Parameters._PERCENTLSU);
            if (percentLSUIndx < 0) {
                return false;
            }
            // map featureId to attributes to be used for updating non-merged features

            var fidsToDelete = new List<long>();
            var mergers = new Dictionary<long, (OSGeo.OGR.Feature, List<OSGeo.OGR.Feature>)>();
            layer.ResetReading();
            OSGeo.OGR.Feature feature;
            do {
                feature = layer.GetNextFeature();
                if (feature is null) { break; }
                var fid = feature.GetFID();
                var hrus = feature.GetFieldAsString(hrusIndx);
                if (hrus == "NA") {
                    ListFuns.insertIntoSortedLongList(fid, fidsToDelete, true);
                    continue;
                }
                // updates for area, percent of subbasin and percent of LSU stored in amap
                var basin = this._gv.topo.SWATBasinToSubbasin[feature.GetFieldAsInteger(subIndx)];
                var basinData = this.basins[basin];
                var subArea = basinData.subbasinArea();
                var SWATChannel = feature.GetFieldAsInteger(chIndx);
                var channel = this._gv.topo.SWATChannelToChannel[SWATChannel];
                var lscape = Utils.landscapeFromName(feature.GetFieldAsString(catIndx));
                // use final lsus
                var lsuData = basinData.getLsus()[channel][lscape];
                var lsuArea = lsuData.area;
                var area = this.hrusArea(hrus, lsuData);
                var areaHa = area / 10000.0;
                if (subArea == 0) {
                    Utils.error(string.Format("SWAT basin {0} seems to be empty", feature.GetFieldAsInteger(subIndx)), this._gv.isBatch);
                    percentSub = 0.0;
                } else {
                    percentSub = area / subArea * 100;
                }
                if (lsuArea == 0) {
                    Utils.error(string.Format("LSU {0} seems to be empty", Utils.landscapeUnitId(SWATChannel, lscape)), this._gv.isBatch);
                    percentLSU = 0.0;
                } else {
                    percentLSU = area / lsuArea * 100;
                }
                if (fidsToMerge.Contains(fid)) {
                    // fid will have other geometries added to it.  Define a new feature for it, to be added later
                    List<OSGeo.OGR.Feature> feats;
                    if (mergers.ContainsKey(fid)) {
                        feats = mergers[fid].Item2;
                    } else {
                        feats = new List<OSGeo.OGR.Feature>();
                    }
                    var geom = feature.GetGeometryRef();
                    var newFeature = feature.Clone();
                    newFeature.SetField(areaIndx, areaHa);
                    newFeature.SetField(percentSubIndx, percentSub);
                    newFeature.SetField(percentLSUIndx, percentLSU);
                    newFeature.SetGeometry(geom.Clone());
                    mergers[fid] = (newFeature, feats);
                    //ListFuns.insertIntoSortedLongList(fid, fidsToDelete, true);
                } else {
                    long fid1;
                    if (!fidsToBeMerged.TryGetValue(fid, out fid1)) {
                        // not involved in any mergers: update
                        feature.SetField(areaIndx, areaHa);
                        feature.SetField(percentSubIndx, percentSub);
                        feature.SetField(percentLSUIndx, percentLSU);
                        if (layer.SetFeature(feature) != Ogr.OGRERR_NONE) {
                            Utils.error("Failed to update actual HRUs shapefile", this._gv.isBatch);
                            return false;
                        }
                    } else {
                        // add feature to list of features to be merged with fid1's new feature
                        (OSGeo.OGR.Feature, List<OSGeo.OGR.Feature>) temp;
                        OSGeo.OGR.Feature feat;
                        List<OSGeo.OGR.Feature> feats;
                        if (!mergers.TryGetValue(fid1, out temp)) {
                            feat = null;
                            feats = new List<OSGeo.OGR.Feature>();
                        } else {
                            (feat, feats) = temp;
                        }
                        feats.Add(feature);
                        mergers[fid1] = (feat, feats);
                        ListFuns.insertIntoSortedLongList(fid, fidsToDelete, true);
                    }
                }
            } while (feature is not null);
            var newFeatures = this.mergeGeometries(mergers);
            foreach (var feat in newFeatures) {
                if (layer.SetFeature(feat) != Ogr.OGRERR_NONE) {
                    Utils.error("Failed to save merged HRU in actual HRUs shapefile", this._gv.isBatch);
                    return false;
                }
            }
            for (int i = fidsToDelete.Count - 1; i >= 0; i--) {
                var fid = fidsToDelete[i];
                if (layer.DeleteFeature(fid) != Ogr.OGRERR_NONE) {
                    Utils.error($"Failed to remove HRU with FID {fid} from actual HRUs shapefile", this._gv.isBatch);
                    return false;
                }
            }
            return true;
        }

        // Mergers is a map fid to pair (feature, feature list).  Merge geometries in list into feature, and finally return list of features.
        public virtual List<OSGeo.OGR.Feature> mergeGeometries(Dictionary<long, (OSGeo.OGR.Feature, List<OSGeo.OGR.Feature>)> mergers) {
            var result = new List<OSGeo.OGR.Feature>();
            foreach (var (feature, feats) in mergers.Values) {
                Debug.Assert(feature is not null);
                var geom = feature.GetGeometryRef().MakeValid(null);
                foreach (var feat in feats) {
                    var geom1 = feat.GetGeometryRef().MakeValid(null);
                    var geom2 = geom.Union(geom1);
                    if (geom2 is null) {
                        // hru features can contain one polygon inside another
                        // and this is regarded as invalid by ogr: combine returns None
                        // fix this by simply extending the first list of polygons with the second
                        // TODO
                        //geom2 = Utils.polyCombine(geom, geom1);
                        Utils.error("Failed to merge HRU geometries", this._gv.isBatch);
                    } else {
                        geom = geom2;
                    }
                }
                feature.SetGeometry(geom);
                result.Add(feature);
            }
            return result;
        }

        // Return total area of hrus in square metres.
        public virtual double hrusArea(string hrus, LSUData lsuData) {
            var area = 0.0;
            foreach (var hru in hrus.Split(",")) {
                area += this.hruArea(hru.Trim(), lsuData);
            }
            return area;
        }

        // Return area of hru in square metres.
        public virtual double hruArea(string hru, LSUData lsuData) {
            try {
                if (hru.StartsWith("RES") || hru.StartsWith("PND")) {
                    return lsuData.waterBody.originalArea;
                } else {
                    var hruNum = Convert.ToInt32(hru);
                    // find hru with this number
                    foreach (var cellData in lsuData.hruMap.Values) {
                        if (cellData.actHRUNum == hruNum) {
                            return cellData.area;
                        }
                    }
                    Utils.error(string.Format("Cannot find hru {0}", hru), this._gv.isBatch);
                    return 0;
                }
            }
            catch (Exception) {
                Utils.error(string.Format("Cannot parse {0} as an hru reference", hru), this._gv.isBatch);
                return 0;
            }
        }

        // Copy actual hrus file to results folder; remove fields except HRUS.
        public void createHrusResultsFile(string actHRUsFile, FeatureLayer arcLayer) {
            Utils.copyShapefile(actHRUsFile, Parameters._HRUS, this._gv.resultsDir);
            var hrusFile = Utils.join(this._gv.resultsDir, Parameters._HRUS + ".shp");
            using (var ds = Ogr.Open(hrusFile, 1)) {
                var hrusLayer = ds.GetLayerByIndex(0);
                Topology.removeFields(hrusLayer, new List<string> { Topology._HRUS }, hrusFile, this._gv.isBatch);
            }
        }

        // Merge LSUs in lsus shapefile according to channel merges, making lsus2.  Return true if successful.
        public async Task<bool> mergeLSUs() {
            string lscape;
            OSGeo.OGR.Feature newFeature;
            int SWATChannel;
            if (!File.Exists(this._gv.fullLSUsFile)) {
                return false;
            }
            var legend = Utils._ACTLSUSLEGEND;
            await Utils.removeLayerByLegend(legend);
            Utils.copyShapefile(this._gv.fullLSUsFile, Parameters._LSUS2, this._gv.shapesDir);
            if (this._gv.useGridModel || this.mergedChannels.Count == 0) {
                // no merging to do
                return true;
            }
            bool OK = true;
            var actLSUsFile = Utils.join(this._gv.shapesDir, Parameters._LSUS2 + ".shp");
            using (var ds = Ogr.Open(actLSUsFile, 1)) {
                var lsusLayer = ds.GetLayerByIndex(0);
                var lsuIndx = lsusLayer.FindFieldIndex(Topology._LSUID, 1);
                if (lsuIndx < 0) {
                    return false;
                }
                var chIndx = lsusLayer.FindFieldIndex(Topology._CHANNEL, 1);
                if (chIndx < 0) {
                    return false;
                }
                var catIndx = lsusLayer.FindFieldIndex(Topology._LANDSCAPE, 1);
                if (catIndx < 0) {
                    return false;
                }
                var areaIndx = lsusLayer.FindFieldIndex(Parameters._AREA, 1);
                if (areaIndx < 0) {
                    return false;
                }
                var percentSubIndx = lsusLayer.FindFieldIndex(Parameters._PERCENTSUB, 1);
                if (percentSubIndx < 0) {
                    return false;
                }
                // first make a set of final target channels
                var finalTargets = new HashSet<int>();
                foreach (var channel in this.mergedChannels.Keys) {
                    finalTargets.Add(this._gv.topo.finalTarget(channel, this.mergedChannels));
                }
                // map of target channel -> landscape -> newFeature
                var mergeMap = new Dictionary<int, Dictionary<string, OSGeo.OGR.Feature>>();
                var toDelete = new List<long>();
                // populate mergeMap
                foreach (var target in finalTargets) {
                    if (!this._gv.topo.channelToSWATChannel.TryGetValue(target, out SWATChannel)) { SWATChannel = 0; }
                    if (SWATChannel > 0) {
                        // defensive programming
                        lsusLayer.SetAttributeFilter(string.Format("{0} = {1}", Topology._CHANNEL, SWATChannel));
                        OSGeo.OGR.Feature targetFeature;
                        do {
                            targetFeature = lsusLayer.GetNextFeature();
                            if (targetFeature is null) { break; }
                            newFeature = targetFeature.Clone();
                            var geom = targetFeature.GetGeometryRef().MakeValid(null);
                            newFeature.SetGeometry(geom);
                            lscape = targetFeature.GetFieldAsString(catIndx);
                            if (mergeMap.ContainsKey(target)) {
                                mergeMap[target][lscape] = newFeature;
                            } else {
                                mergeMap[target] = new Dictionary<string, Feature> { { lscape, newFeature } };
                            }
                            ListFuns.insertIntoSortedLongList(targetFeature.GetFID(), toDelete, true);
                        } while (targetFeature is not null);
                    }
                }
                // find merged channels, add merge details to newFeature, and list them for deletion
                foreach (var channel in this.mergedChannels.Keys) {
                    if (!this._gv.topo.channelToSWATChannel.TryGetValue(channel, out SWATChannel)) {
                        SWATChannel = 0;
                    }
                    if (SWATChannel > 0) {
                        // defensive programming
                        lsusLayer.SetAttributeFilter(string.Format("{0} = {1}", Topology._CHANNEL, SWATChannel));
                        OSGeo.OGR.Feature feature;
                        do {
                            feature = lsusLayer.GetNextFeature();
                            if (feature is null) { break; }
                            var fid = feature.GetFID();
                            var target = this._gv.topo.finalTarget(channel, this.mergedChannels);
                            lscape = feature.GetFieldAsString(catIndx);
                            if (mergeMap.ContainsKey(target)) {
                                if (mergeMap[target].ContainsKey(lscape)) {
                                    newFeature = mergeMap[target][lscape];
                                    var area = newFeature.GetFieldAsDouble(areaIndx);
                                    newFeature.SetField(areaIndx, area += feature.GetFieldAsDouble(areaIndx));
                                    var percentSub = newFeature.GetFieldAsDouble(percentSubIndx);
                                    newFeature.SetField(percentSubIndx, percentSub + feature.GetFieldAsDouble(percentSubIndx));
                                    var geom1 = newFeature.GetGeometryRef().MakeValid(null);
                                    var geom2 = feature.GetGeometryRef().MakeValid(null);
                                    var geom3 = geom1.Union(geom2);
                                    if (geom3 is null) {
                                        // LSU geometries can fail to combine
                                        // fix this by simply extending the first list of polygons with the second
                                        // TODO
                                        //geom3 = Utils.polyCombine(geom1, geom2);
                                        Utils.error("Failed to make combined LSU geometry", this._gv.isBatch);
                                        OK = false;
                                    } else {
                                        newFeature.SetGeometry(geom3);
                                    }
                                    ListFuns.insertIntoSortedLongList(fid, toDelete, true);
                                    lsusLayer.CreateFeature(newFeature);
                                    // delete entry from mergeMap so we can check later all used
                                    mergeMap[target].Remove(lscape);
                                } else {
                                    // there is no lsu to merge with (eg this is upslope but merge target is all floodplain)
                                    // just need to update this feature's lsuid and channel
                                    var tSWATChannel = this._gv.topo.channelToSWATChannel[target];
                                    var lsuid = Utils.landscapeUnitId(tSWATChannel, Utils.landscapeFromName(lscape));
                                    feature.SetField(lsuIndx, lsuid);
                                    feature.SetField(chIndx, tSWATChannel);
                                    lsusLayer.SetFeature(feature);
                                }

                            }
                        } while (feature is not null);
                    }
                }
                // can get newFeatures left in mergeMap because, e.g, the upper LSU has no upslope but the lower one does
                foreach (var mapp in mergeMap.Values) {
                    foreach (var feature in mapp.Values) {
                        lsusLayer.CreateFeature(feature);
                    }
                }
                // remove features to be deleted
                for (int i = toDelete.Count - 1; i >= 0; i--) {
                    var fid = toDelete[i];
                    if (lsusLayer.DeleteFeature(fid) != Ogr.OGRERR_NONE) {
                        Utils.error($"Failed to delete LSU with FID {fid} from actual LSUs shapefile", this._gv.isBatch);
                        OK = false;
                    }
                }
                if (OK && !this._gv.useGridModel) {
                    // load in place of full lsus
                    var fullLSUsLayer = Utils.getLayerByLegend(Utils._FULLLSUSLEGEND);
                    var group = Utils.getGroupLayerByName(Utils._WATERSHED_GROUP_NAME);
                    ArcGIS.Desktop.Mapping.Layer subLayer;
                    if (fullLSUsLayer is null) {
                        subLayer = Utils.getLayerByLegend(Utils._SUBBASINSLEGEND);
                    } else {
                        subLayer = fullLSUsLayer;
                    }
                    var actLSUsLayer = (await Utils.getLayerByFilename(actLSUsFile, FileTypes._ACTLSUS, this._gv, subLayer, Utils._WATERSHED_GROUP_NAME)).Item1;
                    if (fullLSUsLayer is not null) {
                        await Utils.setLayerVisibility(fullLSUsLayer, false);
                    }
                }
            }
            return OK;
        }

        // Write gis_subbasins and gis_lsus tables in project database, 
        //         make subs1.shp in shapes directory, 
        //         copy subs1.shp and lsus2.shp as results templates to Results directory,
        //         and add fields to subs1.shp and lsus2.shp.
        public async Task<bool> writeSubbasinsAndLandscapeTables() {
            Utils.copyShapefile(this._gv.subbasinsFile, Parameters._SUBS1, this._gv.shapesDir);
            var subs1File = Utils.join(this._gv.shapesDir, Parameters._SUBS1 + ".shp");
            var lsus2File = Utils.join(this._gv.shapesDir, Parameters._LSUS2 + ".shp");
            if (!File.Exists(lsus2File)) {
                Utils.error("No actual LSUs file, so gis_subbasins and gis_lsus tables not written", this._gv.isBatch);
                return false;
            }
            this._dlg.progress("Writing subbasins and lsus tables ...");
            // remove features with 0 subbasin value
            var idsToDelete = new List<long>();
            using (var subsDs = Ogr.Open(subs1File, 1)) {
                var subs1Layer = subsDs.GetLayerByIndex(0);
                subs1Layer.ResetReading();
                subs1Layer.SetAttributeFilter(string.Format("{0} = 0", Topology._SUBBASIN));
                OSGeo.OGR.Feature feature;
                do {
                    feature = subs1Layer.GetNextFeature();
                    if (feature is null) { break; }
                    ListFuns.insertIntoSortedLongList(feature.GetFID(), idsToDelete, true);
                } while (feature is not null);
                for (int i = idsToDelete.Count - 1; i >= 0; i--) {
                    var fid = idsToDelete[i];
                    if (subs1Layer.DeleteFeature(fid) != Ogr.OGRERR_NONE) {
                        Utils.error($"Failed to remove subbasin with FID {fid} from subs1.shp", this._gv.isBatch);
                    }
                }
                subs1Layer.SetAttributeFilter(null);
                //OSGeo.OGR.Layer lsus2Layer = null; // defined here for scope reasons.  Assigned later.
                // Add fields from Watershed table to subs1File if less than RIVS1SUBS1MAX features; otherwise takes too long.
                var addToSubs1 = subs1Layer.GetFeatureCount(1) <= Parameters._RIVS1SUBS1MAX;
                // If we are adding fields to subs1 we need to
                // 1. remove fields apart from Subbasin from subs1
                // 2. copy to make results template
                // and if not we need to 
                // 1. copy to make results template
                // 2. remove fields apart from Subbasin from template
                // In the case of lsus1 we have demwshed but that differs from lsus1 in not reflecting stream mergers,
                // so we 
                // 1. copy lsus1 to results directory
                // 2. remove fields from copy except LSUID
                // 3. add fields from gis_lsus to lsus1, using same condition addToSubs1 to decide if this should be done
                if (addToSubs1) {
                    Topology.removeFields(subs1Layer, new List<string> { Topology._POLYGONID, Topology._SUBBASIN }, subs1File, this._gv.isBatch);
                }
                // make copy as template for stream results
                Utils.copyShapefile(subs1File, Parameters._SUBS, this._gv.resultsDir);
                // Utils.loginfo('subs1 copied to subs')
                var subsFile = Utils.join(this._gv.resultsDir, Parameters._SUBS + ".shp");
                if (!addToSubs1) {
                    Topology.removeFields(subs1Layer, new List<string> { Topology._SUBBASIN }, subsFile, this._gv.isBatch);
                }
                // copy lsus2 to results directory and remove fields other than LSUID
                Utils.copyShapefile(lsus2File, Parameters._LSUS, this._gv.resultsDir);
                var lsusFile = Utils.join(this._gv.resultsDir, Parameters._LSUS + ".shp");
                using (var lsusDs = Ogr.Open(lsusFile, 1)) {
                    var lsusLayer = lsusDs.GetLayerByIndex(0);
                    Topology.removeFields(lsusLayer, new List<string> { Topology._LSUID }, lsusFile, this._gv.isBatch);
                }
                int polyIdx = 0;
                int subIdx = 0;
                int areasubIdx = 0;
                int slo1subIdx = 0;
                int len1subIdx = 0;
                int sllsubIdx = 0;
                int latsubIdx = 0;
                int lonsubIdx = 0;
                int elevsubIdx = 0;
                int elevMinsubIdx = 0;
                int elevMaxsubIdx = 0;
                int waterIdIdx = 0;
                int lsuIdx = 0;
                int catlsuIdx = 0;
                int slopelsuIdx = 0;
                int len1lsuIdx = 0;
                int csllsuIdx = 0;
                int wid1lsuIdx = 0;
                int dep1lsuIdx = 0;
                int latlsuIdx = 0;
                int lonlsuIdx = 0;
                int elevlsuIdx = 0;
                var allWaterLSUs = new List<int>();
                if (addToSubs1) {
                    // add fields from gis_subbasins table to subs1
                    subs1Layer.CreateField(new FieldDefn(Parameters._AREA, FieldType.OFTReal), 1);
                    subs1Layer.CreateField(new FieldDefn("Slo1", FieldType.OFTReal), 1);
                    subs1Layer.CreateField(new FieldDefn("Len1", FieldType.OFTReal), 1);
                    subs1Layer.CreateField(new FieldDefn("Sll", FieldType.OFTReal), 1);
                    subs1Layer.CreateField(new FieldDefn("Lat", FieldType.OFTReal), 1);
                    subs1Layer.CreateField(new FieldDefn("Lon", FieldType.OFTReal), 1);
                    subs1Layer.CreateField(new FieldDefn("Elev", FieldType.OFTReal), 1);
                    subs1Layer.CreateField(new FieldDefn("ElevMin", FieldType.OFTReal), 1);
                    subs1Layer.CreateField(new FieldDefn("ElevMax", FieldType.OFTReal), 1);
                    subs1Layer.CreateField(new FieldDefn("WaterId", FieldType.OFTInteger), 1);
                    var defn = subs1Layer.GetLayerDefn();
                    polyIdx = defn.GetFieldIndex(Topology._POLYGONID);
                    subIdx = defn.GetFieldIndex(Topology._SUBBASIN);
                    areasubIdx = defn.GetFieldIndex(Parameters._AREA);
                    slo1subIdx = defn.GetFieldIndex("Slo1");
                    len1subIdx = defn.GetFieldIndex("Len1");
                    sllsubIdx = defn.GetFieldIndex("Sll");
                    latsubIdx = defn.GetFieldIndex("Lat");
                    lonsubIdx = defn.GetFieldIndex("Lon");
                    elevsubIdx = defn.GetFieldIndex("Elev");
                    elevMinsubIdx = defn.GetFieldIndex("ElevMin");
                    elevMaxsubIdx = defn.GetFieldIndex("ElevMax");
                    waterIdIdx = defn.GetFieldIndex("WaterId");
                    using (var lsus2Ds = Ogr.Open(lsus2File, 1)) {
                        var lsus2Layer = lsus2Ds.GetLayerByIndex(0);
                        // add fields from gis_lsus table to lsus2
                        lsus2Layer.CreateField(new FieldDefn("Category", FieldType.OFTInteger), 1);
                        lsus2Layer.CreateField(new FieldDefn("Slope", FieldType.OFTReal), 1);
                        lsus2Layer.CreateField(new FieldDefn("Len1", FieldType.OFTReal), 1);
                        lsus2Layer.CreateField(new FieldDefn("Csl", FieldType.OFTReal), 1);
                        lsus2Layer.CreateField(new FieldDefn("Wid1", FieldType.OFTReal), 1);
                        lsus2Layer.CreateField(new FieldDefn("Dep1", FieldType.OFTReal), 1);
                        lsus2Layer.CreateField(new FieldDefn("Lat", FieldType.OFTReal), 1);
                        lsus2Layer.CreateField(new FieldDefn("Lon", FieldType.OFTReal), 1);
                        lsus2Layer.CreateField(new FieldDefn("Elev", FieldType.OFTReal), 1);
                        var defn2 = lsus2Layer.GetLayerDefn();
                        lsuIdx = defn2.GetFieldIndex(Topology._LSUID);
                        catlsuIdx = defn2.GetFieldIndex("Category");
                        slopelsuIdx = defn2.GetFieldIndex("Slope");
                        len1lsuIdx = defn2.GetFieldIndex("Len1");
                        csllsuIdx = defn2.GetFieldIndex("Csl");
                        wid1lsuIdx = defn2.GetFieldIndex("Wid1");
                        dep1lsuIdx = defn2.GetFieldIndex("Dep1");
                        latlsuIdx = defn2.GetFieldIndex("Lat");
                        lonlsuIdx = defn2.GetFieldIndex("Lon");
                        elevlsuIdx = defn2.GetFieldIndex("Elev");
                    }
                }
                if (this._gv.existingWshed) {
                    // allow for no valley depths file, but use if found
                    if (this._gv.valleyDepthsFile == "") {
                        var @base = System.IO.Path.ChangeExtension(this._gv.demFile, null);
                        var valleyDepthsFile = @base + "depths.tif";
                        if (File.Exists(valleyDepthsFile)) {
                            Utils.loginfo(string.Format("Using {0} as valley depths file", valleyDepthsFile));
                            this._gv.valleyDepthsFile = valleyDepthsFile;
                        }
                    }
                } else if (!File.Exists(this._gv.valleyDepthsFile)) {
                    if (this._gv.useLandscapes) {
                        // should be a valley depths file unless using a buffer for floodplain
                        // or imported floodplain file, which will not contain 'flood'
                        string floodFilename = Path.GetFileName(this._gv.floodFile);
                        if (!floodFilename.StartsWith("bufferflood") && floodFilename.Contains("flood")) {
                            Utils.error(string.Format("Cannot find valley depths raster {0}", this._gv.valleyDepthsFile), this._gv.isBatch);
                            return false;
                        }
                    }
                }
                var transform = new double[6];
                Band valleyDepthsBand = null;
                if (File.Exists(this._gv.valleyDepthsFile)) {
                    var ds = Gdal.Open(this._gv.valleyDepthsFile, Access.GA_ReadOnly);
                    valleyDepthsBand = ds.GetRasterBand(1);
                    ds.GetGeoTransform(transform);
                }
                using (var transaction = this._db.conn.BeginTransaction()) {
                    using (var cmd = new SQLiteCommand("", this._db.conn, transaction)) {
                        var subtable = "gis_subbasins";
                        var clearSQL = "DROP TABLE IF EXISTS " + subtable;
                        this._db.execNonQuery(clearSQL, null, transaction);
                        this._db.execNonQuery(DBUtils._SUBBASINSCREATESQL, null, transaction);
                        var lsutable = "gis_lsus";
                        clearSQL = "DROP TABLE IF EXISTS " + lsutable;
                        this._db.execNonQuery(clearSQL, null, transaction);
                        this._db.execNonQuery(DBUtils._LSUSCREATESQL, null, transaction);
                        IEnumerable<(long, int, int, BasinData)> generator;
                        if (addToSubs1) {
                            generator = this.generateBasinsFromShapefile(subs1Layer, polyIdx, subIdx);
                        } else {
                            generator = this.generateBasinsFromTable();
                        }
                        foreach (var (fid, basin, SWATBasin, basinData) in generator) {
                            if (SWATBasin == 0) {
                                continue;
                            }
                            int waterId = 0;
                            double areaHa = 0;
                            double meanSlopePercent = 0;
                            double farDistance = 0;
                            double slsubbsn = 120;
                            double meanElevation = 0;
                            double elevMin = 0;
                            double elevMax = 0;
                            if (basinData is null) {
                                // subbasin may be inside lake
                                // include anyways as SBR and SUB entries in gis_routing will otherwise cause errors
                                // look for lakeId to use as waterId
                                int lakeId = 0
                                foreach (var (subbasin, (_, _, chLinks)) in this._gv.topo.outlets) {
                                    if (basin == subbasin) {
                                        // with grid models chLinks is always a singleton list
                                        if (this._gv.useGridModel) {
                                            var chLink = chLinks[0];
                                            if (this._gv.topo.chLinkInsideLake.Contains(chLink) || this._gv.topo.chLinkFromLake.Contains(chLink)) {
                                                continue;
                                            }
                                        }
                                        int lakeId;
                                        if (!this._gv.topo.outletsInLake.TryGetValue(subbasin, out lakeId)) { lakeId = -1; }
                                        // if one chLink is inside lake, all will be, since they share their outlet point
                                        // does not seem to be true for HUC models, where right link (coincident with stream exit) must be first
                                        if (lakeId < 0) {
                                            if (!this._gv.topo.chLinkInsideLake.get(chLink, out lakeId)) { break; }
                                        }
                                    }
                                }
                                waterId = lakeId;
                            } else {
                                var areaKm = basinData.subbasinArea() / 1000000.0;
                                areaHa = areaKm * 100;
                                var cellCount = basinData.subbasinCellCount();
                                waterId = basinData.waterId;
                                //  cell counts may be zero in HUC and HAWQS models because of restricted landuse and soil maps
                                Debug.Assert(waterId > 0 || cellCount > 0 || this._gv.isHUC || this._gv.isHAWQS, string.Format("Basin {0} has zero cell count", SWATBasin));
                                var meanSlope = waterId > 0 || cellCount == 0 ? 0 : basinData.totalSlope() / cellCount * this._gv.meanSlopeMultiplier;
                                meanSlopePercent = meanSlope * 100;
                                farDistance = basinData.farDistance * this._gv.tributaryLengthMultiplier;
                                slsubbsn = Utils.getSlsubbsn(meanSlope);
                                meanElevation = waterId > 0 || cellCount == 0 ? 0.0 : basinData.totalElevation() / cellCount;
                                elevMin = waterId > 0 ? 0.0 : basinData.minElevation;
                                elevMax = waterId > 0 ? 0.0 : basinData.maxElevation;
                            }
                            var centroid = this._gv.topo.basinCentroids[basin];
                            var centreX = centroid[0];
                            var centreY = centroid[1];
                            var centroidll = this._gv.topo.pointToLatLong(centroid);
                            var lat = centroidll[1];
                            var lon = centroidll[0];
                            if (addToSubs1) {
                                var f = subs1Layer.GetFeature(fid);
                                f.SetField(areasubIdx, areaHa);
                                f.SetField(slo1subIdx, meanSlopePercent);
                                f.SetField(len1subIdx, farDistance);
                                f.SetField(sllsubIdx, slsubbsn);
                                f.SetField(elevsubIdx, meanElevation);
                                f.SetField(latsubIdx, lat);
                                f.SetField(lonsubIdx, lon);
                                f.SetField(elevMinsubIdx, elevMin);
                                f.SetField(elevMaxsubIdx, elevMax);
                                f.SetField(waterIdIdx, waterId);
                                if (subs1Layer.SetFeature(f) != Ogr.OGRERR_NONE) {
                                    Utils.error($"Failed to update subbasin with FID {fid} in " + subs1File, this._gv.isBatch);
                                }
                            }
                            this._db.execNonQuery(DBUtils._SUBBASINSINSERTSQL,
                                new object[] { SWATBasin, areaHa, meanSlopePercent, farDistance, slsubbsn, lat, lon, meanElevation, elevMin, elevMax, waterId },
                                transaction);
                            this._db.addKey(subtable, SWATBasin);
                            if (basinData is not null) {
                                foreach (var (channel, channelData) in basinData.getLsus()) {
                                    int SWATChannel = this._gv.topo.channelToSWATChannel[channel];
                                    var floodDrop = 0.0;
                                    var floodLen = 0.0;
                                    LSUData floodLsuData;
                                    if (this._gv.useLandscapes) {
                                        if (!channelData.TryGetValue(Utils._FLOODPLAIN, out floodLsuData)) {
                                            floodLsuData = null;
                                        }
                                    } else {
                                        if (!channelData.TryGetValue(Utils._NOLANDSCAPE, out floodLsuData)) {
                                            floodLsuData = null;
                                        }
                                    }

                                    if (floodLsuData is not null) {
                                        var dropToSource = floodLsuData.farElevation - floodLsuData.sourceElevation;
                                        if (valleyDepthsBand is null) {
                                            floodDrop = double.NaN;
                                        } else {
                                            floodDrop = Topology.valueAtPoint(new double[3] { floodLsuData.farPointX, floodLsuData.farPointY, 0 }, transform, valleyDepthsBand);
                                        }
                                        if (double.IsNaN(floodDrop)) {
                                            // estimate it
                                            floodDrop = Math.Abs(this._gv.useLandscapes ? dropToSource * 0.25 : dropToSource);
                                        }
                                        floodLen = floodLsuData.farDistance;
                                    }
                                    foreach (var (landscape, lsuData) in channelData) {
                                        var lsuId = Utils.landscapeUnitId(SWATChannel, landscape);
                                        if (lsuData.cropSoilSlopeArea == 0) {
                                            if (!(this._gv.isHUC || this._gv.isHAWQS)) {  //  outside soil/landuse: leave HUC and HAWQS LSUs to avoid complications with channel
                                                continue;
                                            }
                                        } else if (lsuData.waterBody is not null && !lsuData.waterBody.isUnknown() && lsuData.cropSoilSlopeArea == lsuData.waterBody.originalArea) {
                                            // was all water
                                            allWaterLSUs.Add(lsuId);
                                            continue;
                                        }
                                        double slopeDrop;
                                        double tribDrop;
                                        double tribDistance;
                                        if (landscape == Utils._UPSLOPE) {
                                            tribDistance = (lsuData.farDistance - floodLen) * this._gv.tributaryLengthMultiplier;
                                            if (valleyDepthsBand is null) {
                                                slopeDrop = double.NaN;
                                            } else {
                                                slopeDrop = Topology.valueAtPoint(new double[3] { lsuData.farPointX, lsuData.farPointY, 0 }, transform, valleyDepthsBand);
                                            }
                                            if (double.IsNaN(slopeDrop)) {
                                                // estimate it
                                                slopeDrop = Math.Abs(lsuData.farElevation - lsuData.sourceElevation);
                                            }
                                            tribDrop = Math.Abs(slopeDrop - floodDrop);
                                        } else {
                                            tribDistance = floodLen * this._gv.tributaryLengthMultiplier;
                                            tribDrop = Math.Abs(floodDrop);
                                        }
                                        areaHa = lsuData.area / 10000.0;
                                        if (areaHa == 0) {
                                            Utils.error(string.Format("LSU {0} in subbasin {1} has zero area", lsuId, SWATBasin), this._gv.isBatch);
                                        }
                                        areaKm = areaHa / 100;
                                        if (!this._gv.isHUC) {
                                            Debug.Assert(lsuData.cellCount > 0, string.Format("LSU {0} has zero cell count", lsuId));
                                        }
                                        meanSlopePercent = lsuData.cellCount == 0 ? 0 : lsuData.totalSlope / lsuData.cellCount * 100 * this._gv.meanSlopeMultiplier;
                                        var tribSlopePercent = tribDistance < 1 ? 0 : tribDrop / tribDistance * 100 * this._gv.tributarySlopeMultiplier;
                                        var tribWidth = this._gv.channelWidthMultiplier * Math.Pow(areaKm, this._gv.channelWidthExponent);
                                        var tribDepth = this._gv.channelDepthMultiplier * Math.Pow(areaKm, this._gv.channelDepthExponent);
                                        double meanElev;
                                        if (lsuData.cellCount == 0) {
                                            lat = 0;
                                            lon = 0;
                                            meanElev = 0;
                                        } else {
                                            centroid = this._gv.topo.pointToLatLong(new double[3] { lsuData.totalLongitude / lsuData.cellCount, lsuData.totalLatitude / lsuData.cellCount, 0 });
                                            lat = centroid[1];
                                            lon = centroid[0];
                                            meanElev = lsuData.totalElevation / lsuData.cellCount;
                                        }
                                        this._db.execNonQuery(DBUtils._LSUSINSERTSQL,
                                            new object[] { lsuId, landscape, SWATChannel, SWATBasin, areaHa, meanSlopePercent, tribDistance, tribSlopePercent, tribWidth, tribDepth, lat, lon, meanElev },
                                            transaction);
                                        this._db.addKey(lsutable, lsuId);
                                        if (addToSubs1) {
                                            using (var lsus2Ds = Ogr.Open(lsus2File, 1)) {
                                                var lsus2Layer = lsus2Ds.GetLayerByIndex(0);
                                                lsus2Layer.SetAttributeFilter(string.Format("{0} = {1}", Topology._LSUID, lsuId));
                                                lsus2Layer.ResetReading();
                                                var f = lsus2Layer.GetNextFeature();
                                                if (f is null) {
                                                    Utils.error(string.Format("Failed to find LSU with LSUId {0} in {1}", lsuId, lsus2File), this._gv.isBatch);
                                                    continue;
                                                }
                                                f.SetField(catlsuIdx, landscape);
                                                f.SetField(slopelsuIdx, meanSlopePercent);
                                                f.SetField(len1lsuIdx, tribDistance);
                                                f.SetField(csllsuIdx, tribSlopePercent);
                                                f.SetField(wid1lsuIdx, tribWidth);
                                                f.SetField(dep1lsuIdx, tribDepth);
                                                f.SetField(latlsuIdx, lat);
                                                f.SetField(lonlsuIdx, lon);
                                                f.SetField(elevlsuIdx, meanElev);
                                                if (lsus2Layer.SetFeature(f) != Ogr.OGRERR_NONE) {
                                                    Utils.error($"Failed to update LSU with LSUId {lsuId} in " + lsus2File, this._gv.isBatch);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    transaction.Commit();
                }
                //this._db.hashDbTable(conn, lsutable);
                //this._db.hashDbTable(conn, subtable);
                if (addToSubs1) {
                    var toDelete = new List<long>();
                    using (var lsus2Ds = Ogr.Open(lsus2File, 1)) {
                        var lsus2Layer = lsus2Ds.GetLayerByIndex(0);
                        lsus2Layer.ResetReading();
                        do {
                            feature = lsus2Layer.GetNextFeature();
                            if (feature is null) { break; }
                            var lsuId = feature.GetFieldAsInteger(lsuIdx);
                            if (allWaterLSUs.Contains(lsuId)) {
                                ListFuns.insertIntoSortedLongList(feature.GetFID(), toDelete, true);
                            }
                        } while (feature is not null);
                        for (int i = toDelete.Count - 1; i >= 0; i--) {
                            var fid = toDelete[i];
                            if (lsus2Layer.DeleteFeature(fid) != Ogr.OGRERR_NONE) {
                                Utils.error($"Failed to delete all water LSU with FID {fid} from " + lsus2File, this._gv.isBatch);
                            }
                        }
                    }
                }
                // add subs1 layer in place of watershed layer, unless using grid model
                if (!this._gv.useGridModel) {
                    var ft = this._gv.existingWshed ? FileTypes._EXISTINGSUBBASINS : FileTypes._SUBBASINS;
                    var subbasinsLayer = (await Utils.getLayerByFilename(this._gv.subbasinsFile, ft, null, null, null)).Item1;
                    var subs1Layer = (await Utils.getLayerByFilename(subs1File, ft, this._gv, subbasinsLayer, Utils._WATERSHED_GROUP_NAME)).Item1 as FeatureLayer; ;
                    //subs1Layer.setLabelsEnabled(true);
                    // no need to expand legend since any subbasins upstream from inlets have been removed
                    await QueuedTask.Run(() => { subs1Layer.SetExpanded(false); });
                    if (subbasinsLayer is not null) {
                        await Utils.setLayerVisibility(subbasinsLayer, false);
                    }
                }
            }
            await this.createAquifers();
            this._dlg.progress("");
            return true;
        }

                // Create aquifers and deep aquifers shapefiles, and write gis_aquifers and gis_deep_aquifers tables.
        public async Task createAquifers() {
            int SWATBasin;
            OSGeo.OGR.Layer aqLayer;
            OSGeo.OGR.Layer subsLayer;
            string subsFile;

            // Add new integer 64 field to shapefile; set field to fun applied to value in old integer field.
            // Return the new field's index.  Needs to be 64 bit for HAWQS projects."""
            int addNewField(
                OSGeo.OGR.Layer layer,
                string fileName,
                string newFieldname,
                string oldFieldname,
                Func<int, int> fun) {
                // new field may already be present
                var defn = layer.GetLayerDefn();
                var newIndex = defn.GetFieldIndex(newFieldname);
                if (newIndex < 0) {
                    layer.CreateField(new FieldDefn(newFieldname, FieldType.OFTInteger64), 1);
                    defn = layer.GetLayerDefn();
                    newIndex = defn.GetFieldIndex(newFieldname);
                }
                var oldIndex = defn.GetFieldIndex(oldFieldname);
                layer.ResetReading();
                OSGeo.OGR.Feature f;
                do {
                    f = layer.GetNextFeature();
                    if (f is null) { break; }
                    var oldVal = f.GetFieldAsInteger(oldIndex);
                    f.SetField(newIndex, fun(oldVal));
                    if (layer.SetFeature(f) != Ogr.OGRERR_NONE) {
                        Utils.error(string.Format("Cannot write data to {0}", fileName), this._gv.isBatch);
                    }
                } while (f is not null);
                return newIndex;
            }

            //// downBasins maps basin to downstream basin or -1 if none.  Return final basin starting from basin
            //(int, int) findOutlet(int basin, Dictionary<int, int> downBasins) {
            //    int steps = 0;
            //    while (basin >= 0 && steps < 1E6) {
            //        int downBasin;
            //        if (!downBasins.TryGetValue(basin, out downBasin)) { downBasin = -1; }
            //        if (downBasin < 0) {
            //            return (basin, steps);
            //        } else {
            //            basin = downBasin;
            //            steps += 1;
            //        }
            //    }
            //    return (-1, steps);
            //}

            void makeOutbasins(Dictionary<int, int> downSubbasins, Dictionary<int, int> outbasins) {
                var path = new List<int>();
                int maxx = downSubbasins.Count;
                // Utils.loginfo(string.Format("Downsubbasins: {0}", downSubbasins)
                foreach (var basin in downSubbasins.Keys) {
                    var nxt = basin;
                    while (path.Count <= maxx) {
                        int outBasin;
                        if (!outbasins.TryGetValue(nxt, out outBasin)) { outBasin = -1; }
                        if (outBasin >= 0) {
                            foreach (var p in path) {
                                outbasins[p] = outBasin;
                            }
                            path = new List<int>();
                            break;
                        }
                        path.Add(nxt);
                        int dsBasin;
                        if (!downSubbasins.TryGetValue(nxt, out dsBasin)) { dsBasin = -1; }
                        if (dsBasin < 0) {
                            foreach (var p in path) {
                                outbasins[p] = nxt;
                            }
                            path = new List<int>();
                            break;
                        }
                        nxt = dsBasin;
                    }
                    if (path.Count > maxx) {
                        Utils.error(string.Format("Failed to find outlet for basin {0} on path {1}", basin, path), this._gv.isBatch);
                        var l = path.Count;
                        var last = path[l - 1];
                        var i = l - 2;
                        while (i >= 0) {
                            if (last == path[i]) {
                                Utils.error(string.Format("Path has loop length {0}", l - i), this._gv.isBatch);
                                return;
                            }
                            i -= 1;
                        }
                        Utils.error("Path has no loop", this._gv.isBatch);
                        return;
                    }
                }
            }

            int addOutletBasin(HashSet<int> outlets, int outBasin) {
                // If outBasin has the same outlet point as an existing outlet basin in outlets, return that outlet basin.
                // Else add outBasin to outlets and return outBasin.
                (int, double[], HashSet<int>) triple;
                int outPoint = -1;
                if (!this._gv.topo.outlets.TryGetValue(outBasin, out triple)) {
                    outPoint = -1;
                } else {
                    outPoint = triple.Item1;
                }
                if (outPoint < 0) { // should not happen but safety first
                    outlets.Add(outBasin);
                    return outBasin;
                }
                foreach (var outletBasin in outlets) {
                    int outPoint1 = -1;
                    if (!this._gv.topo.outlets.TryGetValue(outletBasin, out triple) {
                        outPoint1 = -1;
                    } else {
                        outPoint1 = triple.Item1;
                    }
                    if (outPoint1 == outPoint) {
                        return outletBasin;
                    }
                }
                // no matching existing outlet found
                outlets.Add(outBasin);
                return outBasin;
            }

            void writeAquifersTables(Dictionary<int, int> outletSubbasins, Dictionary<int, int> outletLakes) {
                using (var transaction = this._db.conn.BeginTransaction()) {
                    using (var cmd = new SQLiteCommand("", this._db.conn, transaction)) {
                        cmd.CommandText = "DROP TABLE IF EXISTS gis_aquifers";
                        this._db.execCmdQuery(cmd);
                        cmd.CommandText = DBUtils._CREATEAQUIFERS;
                        this._db.execCmdQuery(cmd);
                        using (var demDs = Gdal.Open(this._gv.demFile, Access.GA_ReadOnly)) {
                            var demBand = demDs.GetRasterBand(1);
                            var transform = new double[6];
                            demDs.GetGeoTransform(transform);
                            // map of outlet deep aquifer id to SWATBasin, area, elevation * area, x * area, y * area, lakeId or zero
                            var deepData = new Dictionary<int, (int, double, double, double, double, int)>();
                            foreach (var (basin, basinData) in this.basins) {
                                SWATBasin = this._gv.topo.subbasinToSWATBasin[basin];
                                var aquiferId = SWATBasin * 10;
                                var floodAquiferId = SWATBasin * 10 + Utils._FLOODPLAIN;
                                var upAquiferId = SWATBasin * 10 + Utils._UPSLOPE;
                                var deepAquiferId = outletSubbasins[SWATBasin];
                                // map of aquifer id to (area, centroid)
                                var aquiferData = new Dictionary<int, (double, double, double, double)>();
                                var centroid = this._gv.topo.basinCentroids[basin];
                                var x = centroid[0];
                                var y = centroid[1];
                                foreach (var (channel, channelData) in basinData.getLsus()) {
                                    int chBasin;
                                    if (!this._gv.topo.chLinkToChBasin.TryGetValue(channel, out chBasin)) {
                                        continue;
                                    }
                                    var chBasinArea = this._gv.useGridModel ? this._gv.topo.dx * this._gv.topo.dy * this._gv.gridSize * this._gv.gridSize : this._gv.topo.chBasinAreas[chBasin];
                                    var upArea = 0.0;
                                    var upTotalElevation = 0.0;
                                    var upCellCount = 0;
                                    var hasFlood = false;
                                    var hasUp = false;
                                    // only count upslope areas as floodplain may be partly water or lake
                                    foreach (var (landscape, lsuData) in channelData) {
                                        if (landscape == Utils._UPSLOPE) {
                                            hasUp = true;
                                            upArea += lsuData.area;
                                            upTotalElevation += lsuData.totalElevation;
                                            upCellCount += lsuData.cellCount;
                                        } else if (landscape == Utils._FLOODPLAIN) {
                                            hasFlood = true;
                                        }
                                    }
                                    var floodArea = chBasinArea - upArea;
                                    // Utils.loginfo('Channel {0}: Area: {1}: Up: {2}: Flood: {3}', SWATChannel, chBasinArea, upArea, floodArea))
                                    int lakeId;
                                    if (!outletLakes.TryGetValue(SWATBasin, out lakeId)) {
                                        lakeId = 0;
                                    }
                                    double deepArea = 0;
                                    double deepElevMoment = 0;
                                    double deepXMoment = 0;
                                    double deepYMoment = 0;
                                    double aqArea = 0;
                                    double aqElevMoment = 0;
                                    double aqXMoment = 0;
                                    double aqYMoment = 0;
                                    //  take care not to go back to old SWATBasin or lakeId
                                    if (deepData.ContainsKey(deepAquiferId)) {
                                        (_, deepArea, deepElevMoment, deepXMoment, deepYMoment, _) = deepData[deepAquiferId];
                                    } else {
                                        deepData.Add(deepAquiferId, (SWATBasin, deepArea, deepElevMoment, deepXMoment, deepYMoment, lakeId));
                                    }
                                    double elev;
                                    if (hasUp) {
                                        elev = upCellCount == 0 ? 0 : upTotalElevation / upCellCount;
                                        if (aquiferData.ContainsKey(upAquiferId)) {
                                            (aqArea, aqElevMoment, aqXMoment, aqYMoment) = aquiferData[upAquiferId];
                                        } else {
                                            aquiferData.Add(upAquiferId, (aqArea, aqElevMoment, aqXMoment, aqYMoment));
                                        }
                                        aqArea += upArea;
                                        aqElevMoment += elev * upArea;
                                        aqXMoment += x * upArea;
                                        aqYMoment += y * upArea;
                                        aquiferData[upAquiferId] = (aqArea, aqElevMoment, aqXMoment, aqYMoment);
                                        deepArea += upArea;
                                        deepElevMoment += elev * upArea;
                                        deepXMoment += x * upArea;
                                        deepYMoment += y * upArea;
                                    }
                                    if (hasFlood) {
                                        elev = Topology.valueAtPoint(centroid, transform, demBand);
                                        if (double.IsNaN(elev)) {
                                            elev = 0;
                                            // Utils.loginfo('No elevation for basin {0}', chBasin))
                                        }
                                        if (aquiferData.ContainsKey(floodAquiferId)) {
                                            (aqArea, aqElevMoment, _, _) = aquiferData[floodAquiferId];
                                        } else {
                                            aquiferData.Add(floodAquiferId, (0.0, 0.0, 0.0, 0.0));
                                        }
                                        aqArea += floodArea;
                                        aqElevMoment += elev * floodArea;
                                        aquiferData[floodAquiferId] = (aqArea, aqElevMoment, 0, 0);
                                        deepArea += floodArea;
                                        deepElevMoment += elev * floodArea;
                                        deepXMoment += x * floodArea;
                                        deepYMoment += y * floodArea;
                                    } else if (!hasUp) {
                                        elev = Topology.valueAtPoint(centroid, transform, demBand);
                                        if (double.IsNaN(elev)) {
                                            elev = 0;
                                            // Utils.loginfo('No elevation for basin {0}', chBasin))
                                        }
                                        if (aquiferData.ContainsKey(aquiferId)) {
                                            (aqArea, aqElevMoment, _, _) = aquiferData[aquiferId];
                                        } else {
                                            aquiferData.Add(aquiferId, (0.0, 0.0, 0.0, 0.0));
                                        }
                                        aqArea += chBasinArea;
                                        aqElevMoment += elev * chBasinArea;
                                        aquiferData[aquiferId] = (aqArea, aqElevMoment, 0, 0);
                                        deepArea += chBasinArea;
                                        deepElevMoment += elev * chBasinArea;
                                        deepXMoment += x * chBasinArea;
                                        deepYMoment += y * chBasinArea;
                                    }
                                    // upAreaSoFar = aquiferData[upAquiferId][0]
                                    // floodAreaSoFar = aquiferData[floodAquiferId][0]
                                    // Utils.loginfo('So far: Up: {0}: Flood: {1}', upAreaSoFar, floodAreaSoFar))
                                    deepData[deepAquiferId] = (SWATBasin, deepArea, deepElevMoment, deepXMoment, deepYMoment, lakeId);
                                    // Utils.loginfo('Deep area so far: {0}', deepArea))
                                }
                                // write gis_aquifers and route aquifers
                                var centroidll = this._gv.topo.pointToLatLong(centroid);
                                foreach (var (aqId, (area, elevMoment, xMoment, yMoment)) in aquiferData) {
                                    var landscape = aqId % 10;
                                    var elev = elevMoment / area;
                                    if (landscape == Utils._UPSLOPE) {
                                        var upCentroid = new double[3] { xMoment / area, yMoment / area, 0 };
                                        var upCentroidll = this._gv.topo.pointToLatLong(upCentroid);
                                        this._db.execNonQuery(DBUtils._INSERTAQUIFERS, new object[] { aqId, landscape, SWATBasin, deepAquiferId, area / 10000.0, upCentroidll[1], upCentroidll[0], elev });
                                        // route total flow to floodplain aquifer if any, else to subbasin outlet
                                        var downAqId = SWATBasin * 10 + Utils._FLOODPLAIN;
                                        if (aquiferData.ContainsKey(downAqId)) {
                                            this._db.addToRouting(aqId, "AQU", downAqId, "AQU", Topology._TOTAL, 100, cmd);
                                        } else {
                                            var (pointId, _, chLinks) = this._gv.topo.outlets[basin];
                                            int lakeId;
                                            if (!this._gv.topo.outletsInLake.TryGetValue(basin, out lakeId)) {
                                                // all chLinks will share a common outlet, so sufficient to check one
                                                if (!this._gv.topo.chLinkInsideLake.TryGetValue(chLinks.First(), out lakeId)) { lakeId = -1; }
                                            }
                                            if (lakeId < 0) {
                                                this._gv.db.addToRouting(aqId, "AQU", pointId, "PT", Topology._TOTAL, 100, cmd);
                                            } else {
                                                var lakeData = this._gv.topo.lakesData[lakeId];
                                                var wCat = lakeData.waterRole == WaterRole._RESERVOIR ? "RES" : lakeData.waterRole == WaterRole._POND ? "PND" : "WETL";
                                                this._gv.db.addToRouting(aqId, "AQU", lakeId, wCat, Topology._TOTAL, 100, cmd);
                                            }
                                        }
                                    } else {
                                        this._db.execNonQuery(DBUtils._INSERTAQUIFERS, new object[] { aqId, landscape, SWATBasin, deepAquiferId, area / 10000.0, centroidll[1], centroidll[0], elev }, transaction);
                                        // route total flow to subbasin outlet
                                        var (pointId, _, chLinks) = this._gv.topo.outlets[basin];
                                        int lakeId;
                                        if (!this._gv.topo.outletsInLake.TryGetValue(basin, out lakeId)) {
                                            // all chLinks will share a common outlet, so sufficient to check one
                                            if (!this._gv.topo.chLinkInsideLake.TryGetValue(chLinks.First(), out lakeId)) { lakeId = -1; }
                                        }
                                        if (lakeId < 0) {
                                            this._gv.db.addToRouting(aqId, "AQU", pointId, "PT", Topology._TOTAL, 100, cmd);
                                        } else {
                                            var lakeData = this._gv.topo.lakesData[lakeId];
                                            var wCat = lakeData.waterRole == WaterRole._RESERVOIR ? "RES" : lakeData.waterRole == WaterRole._POND ? "PND" : "WETL";
                                            this._gv.db.addToRouting(aqId, "AQU", lakeId, wCat, Topology._TOTAL, 100, cmd);
                                        }
                                    }
                                    // route recharge to deep aquifer
                                    this._gv.db.addToRouting(aqId, "AQU", deepAquiferId, "DAQ", Topology._RECHARGE, 100, cmd);
                                }
                            }
                            // write deep aquifers
                            cmd.CommandText = "DROP TABLE IF EXISTS gis_deep_aquifers";
                            this._db.execCmdQuery(cmd);
                            cmd.CommandText = DBUtils._CREATEADEEPQUIFERS;
                            this._db.execCmdQuery(cmd);
                            foreach (var (SWATBasin, deepAqId, (deepArea, deepElevMoment, deepXMoment, deepYMoment, lakeId)) in deepData) {
                                var elev = deepElevMoment / deepArea;
                                var centroid = new double[3] { deepXMoment / deepArea, deepYMoment / deepArea, 0 };
                                var centroidll = this._gv.topo.pointToLatLong(centroid);
                                cmd.CommandText = DBUtils._INSERTDEEPAQUIFERS;
                                this._db.execCmdQuery(cmd, new object[] { deepAqId, SWATBasin, deepArea / 10000.0, centroidll[1], centroidll[0], elev });
                                // route total flow from deep aquifer to watershed outlet
                                int pointId;
                                if (lakeId > 0) {
                                    (_, pointId, _, _) = this._gv.topo.lakesData[lakeId].outPoint;
                                } else {
                                    int basin = this._gv.topo.SWATBasinToSubbasin[deepAqId];
                                    (pointId, _, _) = this._gv.topo.outlets[basin];
                                }
                                this._gv.db.addToRouting(deepAqId, "DAQ", pointId, "PT", Topology._TOTAL, 100, cmd);
                                // route point in case not done
                                if (!this._gv.topo.routedPoints.Contains(pointId)) {
                                    this._gv.db.addToRouting(pointId, "PT", 0, "X", Topology._TOTAL, 100, cmd);
                                } else {
                                    this._gv.db.updatRouting(pointId, "PT", 0, "X", Topology._TOTAL, 100, cmd);
                                }
                            }
                        }
                    }
                    transaction.Commit();
                }
            }

            // start of createAquifers
            this._dlg.progress("Writing aquifers and deep aquifers tables ...");
            DataSource subsDs;
            if (File.Exists(this._gv.subsNoLakesFile)) {
                subsFile = this._gv.subsNoLakesFile;
                subsDs = Ogr.Open(subsFile, 0);
                subsLayer = subsDs.GetLayerByIndex(0);
                subsLayer.SetAttributeFilter(string.Format("{0} = 0", Topology._SUBBASIN));
                subsLayer.ResetReading();
                OSGeo.OGR.Feature feature;
                var idsToDelete = new List<long>();
                do {
                    feature = subsLayer.GetNextFeature();
                    if (feature is null) { break; }
                    ListFuns.insertIntoSortedLongList(feature.GetFID(), idsToDelete, true);
                } while (feature is not null);
                for (int i = idsToDelete.Count - 1; i >= 0; i--) {
                    if (subsLayer.DeleteFeature(idsToDelete[i]) != Ogr.OGRERR_NONE) {
                        Utils.error(string.Format("Cannot edit aquifers shapefile {0}, so cannot create aquifer and deep aquifer shapefiles for visualisation.", subsFile), this._gv.isBatch);
                        return;
                    }
                }
            } else {
                // use subs1 as already has basins upstream from inlets removed
                subsFile = Utils.join(this._gv.shapesDir, Parameters._SUBS1 + ".shp");
                subsDs = Ogr.Open(subsFile, 1);
                subsLayer = subsDs.GetLayerByIndex(0);
            }
            var aqFile = Utils.join(this._gv.resultsDir, Parameters._AQUIFERS + ".shp");
            await Utils.removeLayer(aqFile);
            if (this._dlg.floodplainComboBox.SelectedIndex == 0) {
                // use subbasins shapefile as aquifers shapefile
                Utils.copyShapefile(subsFile, Parameters._AQUIFERS, this._gv.resultsDir);
                using (var aqDs = Ogr.Open(aqFile, 1)) {
                    aqLayer = aqDs.GetLayerByIndex(0);
                    addNewField(aqLayer, aqFile, Topology._AQUIFER, Topology._SUBBASIN, x => 10 * x);
                    Topology.removeFields(aqLayer, new List<string> { Topology._AQUIFER }, aqFile, this._gv.isBatch);
                }
            } else {
                try {
                    // merge shapes in actual LSUs file that have same subbasin and same landscape
                    using (var lsusDs = Ogr.Open(this._gv.actLSUsFile, 0))
                    using (var aqDrv = Ogr.GetDriverByName("ESRI Shapefile")) {
                        var actLSUsLayer = lsusDs.GetLayerByIndex(0);
                        var aqDs = aqDrv.CreateDataSource(aqFile, null);
                        var wkt = this._gv.topo.crsProject.Wkt;
                        OSGeo.OSR.SpatialReference srs = new OSGeo.OSR.SpatialReference(wkt);
                        aqLayer = aqDs.CreateLayer("Aquifers", srs, wkbGeometryType.wkbMultiPolygon, null);
                        if (aqLayer.CreateField(new FieldDefn(Topology._AQUIFER, FieldType.OFTInteger), 1) != Ogr.OGRERR_NONE) {
                            throw new Exception(string.Format("Cannot create aquifers shapefile {0}", aqFile));
                        }
                        foreach (var basin in this.basins.Keys) {
                            if (!this._gv.topo.subbasinToSWATBasin.TryGetValue(basin, out SWATBasin)) { SWATBasin = 0; }
                            if (SWATBasin > 0) {
                                var subExp = $"Subbasin = {SWATBasin}";
                                //var floodExp = string.Format("Subbasin = {0} AND Landscape = \'Floodplain\'", SWATBasin);
                                //var upExp = string.Format("Subbasin = {0} AND Landscape = \'Upslope\'", SWATBasin);
                                actLSUsLayer.SetAttributeFilter(subExp);
                                actLSUsLayer.ResetReading();
                                OSGeo.OGR.Feature feature;
                                OSGeo.OGR.Geometry floodAqGeom = null;
                                OSGeo.OGR.Geometry upAqGeom = null;
                                do {
                                    feature = actLSUsLayer.GetNextFeature();
                                    if (feature is null) { break; }
                                    var lscape = feature.GetFieldAsString("Landscape");
                                    if (lscape == "Floodplain") {
                                        if (floodAqGeom is null) {
                                            floodAqGeom = feature.GetGeometryRef();
                                        } else {
                                            floodAqGeom = floodAqGeom.Union(feature.GetGeometryRef());
                                        }
                                    } else if (lscape == "Upslope") {
                                        if (floodAqGeom is null) {
                                            floodAqGeom = feature.GetGeometryRef();
                                        } else {
                                            floodAqGeom = floodAqGeom.Union(feature.GetGeometryRef());
                                        }
                                    }
                                } while (feature is not null);
                                actLSUsLayer.SetAttributeFilter(null);
                                var defn = aqLayer.GetLayerDefn();
                                var floodAq = new OSGeo.OGR.Feature(defn);
                                floodAq.SetGeometry(floodAqGeom);
                                floodAq.SetField(Topology._AQUIFER, SWATBasin * 10 + Utils._FLOODPLAIN);
                                if (aqLayer.CreateFeature(floodAq) != Ogr.OGRERR_NONE) {
                                    throw new Exception(string.Format("Failed to write aquifers shapefile {0}", aqFile));
                                }
                                var upAq = new OSGeo.OGR.Feature(defn);
                                upAq.SetGeometry(upAqGeom);
                                upAq.SetField(Topology._AQUIFER, SWATBasin * 10 + Utils._UPSLOPE);
                                if (aqLayer.CreateFeature(upAq) != Ogr.OGRERR_NONE) {
                                    throw new Exception(string.Format("Failed to write aquifers shapefile {0}", aqFile));
                                }
                            }
                        }
                        aqDs.SyncToDisk();
                        aqDs.Dispose();
                    }
                }
                catch (Exception ex) {
                    Utils.information(string.Format("Failed to generate aquifers shapefile: aquifer result visualisation will not be possible: {0}", ex.Message), this._gv.isBatch);
                }
            };
            // make map of subbasin to outlet subbasin in each watershed:
            var outletSubbasins = new Dictionary<int, int>();
            // lakes with watershed outlets within them
            var outletLakes = new Dictionary<int, int>();
            // to make deepAquiferId when watershed outlet is in lake and there is no outlet SWATBasin
            var baseDeepAquiferId = Enumerable.Max(this._gv.topo.SWATBasinToSubbasin.Keys);
            var outlets = new HashSet<int>();
            // make map basin to outbasin
            var outbasins = new Dictionary<int, int>();
            makeOutbasins(this._gv.topo.downSubbasins, outbasins);
            foreach (var basin in this._gv.topo.downSubbasins.Keys) {
                if (!this._gv.topo.subbasinToSWATBasin.TryGetValue(basin, out SWATBasin)) { SWATBasin = 0; }
                if (SWATBasin > 0) {
                    int outBasin;
                    if (!outbasins.TryGetValue(basin, out outBasin)) { outBasin = -1; }
                    if (outBasin < 0) {
                        Utils.error(string.Format("Cannot find watershed outlet for subbasin {0}", SWATBasin), this._gv.isBatch);
                        continue;
                    }
                    outBasin = addOutletBasin(outlets, outBasin);
                    // check if outlet in lake
                    int lakeId;
                    if (!this._gv.topo.outletsInLake.TryGetValue(outBasin, out lakeId)) {
                        lakeId = -1;
                    }
                    if (lakeId > 0) {
                        // deepAquiferId chosen so that basins draining to same lake will share it
                        var deepAquiferId = baseDeepAquiferId + lakeId;
                        outletSubbasins[SWATBasin] = deepAquiferId;
                        outletLakes[SWATBasin] = lakeId;
                        // store mapping of deepAquiferId to its basin in SWATBasinToSubbasin so it can be recovered later
                        // when routing DAQ to its outlet point
                        this._gv.topo.SWATBasinToSubbasin[deepAquiferId] = outBasin
                    } else {
                        int outlet;
                        if (!this._gv.topo.subbasinToSWATBasin.TryGetValue(outBasin, out outlet)) {
                            outlet = 0;
                        }
                        outletSubbasins[SWATBasin] = outlet;
                        if (outletSubbasins[SWATBasin] == 0) {
                            Utils.error(string.Format("Cannot find watershed outlet for subbasin {0}", SWATBasin), this._gv.isBatch);
                            continue;
                        }
                    }
                }
            }
            // make all subbasins with outletsubbasin in lake have outletsubbasin deepAquiferId
            foreach (var (outSWATBasin, lakeId) in outletLakes) {
                deepAquiferId = baseDeepAquiferId + lakeId;
                foreach (var SWATBasin in outletSubbasins.Keys) {
                    if (outletSubbasins[SWATBasin] == outSWATBasin) {
                        outletSubbasins[SWATBasin] = deepAquiferId;
                    }
                }
            }
            //Utils.loginfo('Outlet subbasins: {0}', outletSubbasins))
            //Utils.loginfo('Down subbasins: {0}', self._gv.topo.downSubbasins))
            string deepAqFile = Utils.join(this._gv.resultsDir, Parameters._DEEPAQUIFERS + ".shp");
            try {
                // create deep aquifer file by dissolving subbasins file
                await Utils.removeLayer(deepAqFile);
                Utils.copyShapefile(subsFile, "deep_temp", this._gv.resultsDir);
                var tempDeepAqFile = Utils.join(this._gv.resultsDir, "deep_temp.shp");
                using (var tempDs = Ogr.Open(tempDeepAqFile, 1)) {
                    var tempLayer = tempDs.GetLayerByIndex(0);
                    // add Aquifer field, set to number of outlet subbasin

                    int getOutletSubbasin(int bsn) {
                        int outletBasin;
                        if (outletSubbasins.TryGetValue(bsn, out outletbasin)) {
                            return outletBasin;
                        } else {
                            return 0;
                        }
                    }

                    addNewField(tempLayer, tempDeepAqFile, Topology._AQUIFER, Topology._SUBBASIN, x => getOutletSubbasin(x));
                    Topology.removeFields(tempLayer, new List<string> { Topology._AQUIFER }, tempDeepAqFile, this._gv.isBatch);
                    // dissolve on Aquifer field
                    var parms = Geoprocessing.MakeValueArray(tempDeepAqFile, deepAqFile, Topology._AQUIFER);
                    var result = await Geoprocessing.ExecuteToolAsync("analysis.PairwiseDissolve", parms);
                    if (string.IsNullOrEmpty(result.ReturnValue)) {
                        Utils.error("Failed to dissolve deep aquifer shapefile: " + $@"{result.ErrorMessages}", this._gv.isBatch);
                    } else {
                        //Utils.loginfo("Created deep aquifer shapefile: " + deepAqFile + $@" {result.ReturnValue}");
                        // this seems to create a layer automatically.  Remove it
                        await Utils.removeLayer(deepAqFile);
                    }
                }
                Utils.removeFiles(tempDeepAqFile);
            }
            catch (Exception ex) {
                Utils.information(string.Format("Failed to generate deep aquifer shapefile: deep aquifer result visualisation will not be possible: {0}", ex.Message), this._gv.isBatch);
            }
            writeAquifersTables(outletSubbasins, outletLakes);
            // if we have a multiple catchment grid model with over _GRIDCELLSMAX cells, create partition into catchments
            // only do this if not an existing watershed, when user presumed to know what they want.
            if (this._gv.useGridModel && !this._gv.existingWshed && outlets.Count > 1 && subsLayer.GetFeatureCount(1) > Parameters._GRIDCELLSMAX) {
                // add Aquifer field to grid shapefile
                int aqIndex = 0; // Aquifer is only field
                var gridAqIndex = addNewField(subsLayer, subsFile, Topology._AQUIFER, Topology._SUBBASIN, x => outletSubbasins[x]);
                this.partition(deepAqFile, aqIndex, subsLayer, gridAqIndex, subsFile);
            }
        }

        //  Create a partition of the grid shapefile into a collection of files
        //             the first containing the grid cells in the largest catchment and subsequent ones 
        //             the grid cells forming a collection of catchments with total area at most the largest catchment.
        public virtual void partition(
            string catchmentsFile,
            int catchmentIndex,
            OSGeo.OGR.Layer gridLayer,
            int gridCatchmentIndex,
            string gridFile) {
            // create a list of catchment ids and areas sorted by area
            var gridBase = System.IO.Path.ChangeExtension(gridFile, null);
            // remove existing partitions
            var pattern = gridBase + "[1-9]*.*";
            Utils.tryRemoveFilePattern(System.IO.Path.GetDirectoryName(gridBase), pattern);
            var gridFields = gridLayer.GetLayerDefn();
            var catchmentAreasList = new List<(int, double)>();
            using (var catchmentsDs = Ogr.Open(catchmentsFile, 1)) {
                var catchmentsLayer = catchmentsDs.GetLayerByIndex(0);
                catchmentsLayer.ResetReading();
                OSGeo.OGR.Feature catchment;
                do {
                    catchment = catchmentsLayer.GetNextFeature();
                    if (catchment is null) { break; }
                    var area = catchment.GetGeometryRef().Area();
                    var catchmentNum = catchment.GetFieldAsInteger(catchmentIndex);
                    catchmentAreasList.Add((catchmentNum, area));
                } while (catchment is not null);
            }
            // sort in order of decreaing area
            var catchmentsDecreasing = catchmentAreasList.OrderByDescending(x => x.Item2).ToList();
            var minArea = Parameters._GRIDCELLSMAX * this._gv.topo.dx * this._gv.topo.dy * this._gv.gridSize * this._gv.gridSize;
            // aim at files size of largest aquifer, or minimum area, whichever is larger
            // minimum area condition prevents splitting more than we need to
            var targetArea = Math.Max(catchmentsDecreasing.First().Item2, minArea);
            var partNum = 0;
            var driver = Ogr.GetDriverByName("ESRI Shapefile");
            while (catchmentsDecreasing.Count > 0) {
                // prepare next partition
                // add as many catchments as possible, searching largest to smallest
                // storing catchment numbers in nextPart
                var remainingArea = targetArea;
                var i = 0;
                var nextPart = new List<int>();
                while (i < catchmentsDecreasing.Count) {
                    var (catchmentNum, area) = catchmentsDecreasing[i];
                    if (area <= remainingArea) {
                        remainingArea -= area;
                        nextPart.Add(catchmentNum);
                        catchmentsDecreasing.RemoveAt(i);
                    } else {
                        i += 1;
                    }
                }
                partNum += 1;
                var nextPartFile = gridBase + partNum.ToString() + ".shp";
                Debug.Assert(this._gv.crsProject is not null);
                var nextDs = driver.CreateDataSource(nextPartFile, null);
                var wkt = this._gv.topo.crsProject.Wkt;
                OSGeo.OSR.SpatialReference srs = new OSGeo.OSR.SpatialReference(wkt);
                var nextPartLayer = nextDs.CreateLayer($"partition {partNum}", srs, wkbGeometryType.wkbMultiPolygon, null);
                for (var j = 0; j < gridFields.GetFieldCount(); j++) {
                    nextPartLayer.CreateField(gridFields.GetFieldDefn(j), 1);
                }
                for (var j = 0; j < gridFields.GetGeomFieldCount(); j++) {
                    nextPartLayer.CreateGeomField(gridFields.GetGeomFieldDefn(j), 1);
                }
                Utils.copyPrj(gridFile, nextPartFile);
                gridLayer.ResetReading();
                OSGeo.OGR.Feature cell;
                do {
                    cell = gridLayer.GetNextFeature();
                    if (cell is null) { break; }
                    var num = cell.GetFieldAsInteger(gridCatchmentIndex);
                    if (nextPart.Contains(num)) {
                        nextPartLayer.SetFeature(cell.Clone());
                    }
                } while (cell is not null);
                nextPartLayer.SyncToDisk();
                nextDs = null;
            }
        }

        // Yield (feature id, basin, SWATBasin, basinData) tuples from subs1.shp.
        public virtual IEnumerable<(long, int, int, BasinData)> generateBasinsFromShapefile(OSGeo.OGR.Layer layer, int polyIdx, int subIdx) {
            layer.ResetReading();
            OSGeo.OGR.Feature feature;
            do {
                feature = layer.GetNextFeature();
                if (feature is null) { break; }
                var basin = feature.GetFieldAsInteger(polyIdx);
                BasinData basinData;
                if (!this.basins.TryGetValue(basin, out basinData)) { basinData = null; }
                yield return (feature.GetFID(), basin, feature.GetFieldAsInteger(subIdx), basinData);
            } while (feature is not null);
        }

        // Yield (feature id, basin, SWATBasin, basinData) tuples from tables.
        public virtual IEnumerable<(long, int, int, BasinData)> generateBasinsFromTable() {
            foreach (var (basin, basinData) in this.basins) {
                int SWATBasin;
                if (!this._gv.topo.subbasinToSWATBasin.TryGetValue(basin, out SWATBasin)) { SWATBasin = 0; }
                yield return (0, basin, SWATBasin, basinData);
            }
        }

        // Write gis_water table.  Also creates new reservoirs as needed.
        public async Task writeWaterBodiesTable() {
            this._gv.topo.foundReservoirs = new Dictionary<int, (int, int, double[])>();
            var channelLayer = (await Utils.getLayerByFilename(this._gv.channelFile, FileTypes._CHANNELS, null, null, null)).Item1 as FeatureLayer;
            //if (channelLayer is null) {
            //    // perhaps user removed it
            //    var channelLayer = QgsVectorLayer(this._gv.channelFile, "Channels", "ogr");
            //}
            using (var transaction = this._db.conn.BeginTransaction()) {
                using (var cmd = new SQLiteCommand("", this._db.conn, transaction)) {
                    cmd.CommandText = "DROP TABLE IF EXISTS gis_water";
                    this._db.execCmdQuery(cmd);
                    cmd.CommandText = DBUtils._WATERCREATESQL;
                    this._db.execCmdQuery(cmd);
                    // add lakes 
                    foreach (var (lakeId, lakeData) in this._gv.topo.lakesData) {
                        var lCat = lakeData.waterRole == WaterRole._RESERVOIR ? "RES" : lakeData.waterRole == WaterRole._POND ? "PND" : lakeData.waterRole == WaterRole._WETLAND ? "WETL" : null;
                        if (lCat is null) {
                            continue; // don't include playas
                        }
                        int lsuId = 0; // no LSU for a reservoir, pond or wetland lake
                        var subbasin = lakeData.outPoint.Item1;
                        if ((this._gv.isHUC || this._gv.isHAWQS) && subbasin < 0) {
                            continue; // lakes file is larger than watershed for HUC12 models
                        }
                        // out point may have no subbasin if internal to lake
                        int SWATBasin;
                        if (!this._gv.topo.subbasinToSWATBasin.TryGetValue(subbasin, out SWATBasin)) { SWATBasin = 0; }
                        // out point may have no subbasin if internal to lake
                        if (SWATBasin == 0) {
                            if (!this._gv.topo.subbasinsInLakes.TryGetValue(subbasin, out SWATBasin) { SWATBasin = 0; }
                            if (SWATBasin == 0) {
                                QSWATUtils.error(string.Format("Cannot find subbasin for outlet of lake {0}", lakeId), this._gv.isBatch);
                            }
                        }
                        var areaHa = lakeData.overrideArea / 10000.0;
                        var centroid = lakeData.centroid;
                        var centroidll = this._gv.topo.pointToLatLong(centroid);
                        var elev = lakeData.elevation;
                        cmd.CommandText = DBUtils._WATERINSERTSQL;
                        this._db.execCmdQuery(cmd, new object[] { lakeId, lCat, lsuId, SWATBasin, areaHa, centroid[0], centroid[1], centroidll[1], centroidll[0], elev });
                        this._gv.db.addKey("gis_water", lakeId);
                    }
                    // add reservoirs and ponds
                    var floodscape = this._gv.useLandscapes ? Utils._FLOODPLAIN : Utils._NOLANDSCAPE;
                    foreach (var basinData in this.basins.Values) {
                        foreach (var (channel, channelData) in basinData.getLsus()) {
                            int SWATChannel;
                            if (!this._gv.topo.channelToSWATChannel.TryGetValue(channel, out SWATChannel)) { SWATChannel = -1; }
                            foreach (var (landscape, lsuData) in channelData) {
                                var waterBody = lsuData.waterBody;
                                if (waterBody is not null && !waterBody.isUnknown()) {
                                    if (waterBody.cellCount == 0) {
                                        if (waterBody.isInlet()) {
                                            // user-defined water body
                                            var cat = waterBody.isReservoir() ? "reservoir" : "pond";
                                            Utils.information(string.Format("WARNING: {0} on channel {1} has zero area", cat, SWATChannel), this._gv.isBatch);
                                        } else {
                                            continue;
                                        }
                                    }
                                    // set id if necessary:
                                    // point ids from inlets/outlets file may be sme as lake ids
                                    if (waterBody.id == 0 || this._gv.db.hasKey("gis_water", waterBody.id)) {
                                        this._gv.topo.waterBodyId += 1;
                                        waterBody.id = this._gv.topo.waterBodyId;
                                    }
                                    string wCat = "";
                                    if (waterBody.isReservoir()) {
                                        this.propagateReservoirId(channel, waterBody.id, floodscape);
                                        wCat = "RES";
                                    } else {
                                        wCat = "PND";
                                    }
                                    var channel0 = this._gv.topo.finalTarget(channel, this.mergedChannels);
                                    var lsuId = Utils.landscapeUnitId(this._gv.topo.channelToSWATChannel[channel0], landscape);
                                    int basin;
                                    int chBasin;
                                    if (this._gv.useGridModel) {
                                        basin = this._gv.topo.chLinkToChBasin[channel0];
                                    } else {
                                        chBasin = this._gv.topo.chLinkToChBasin[channel0];
                                        basin = this._gv.topo.chBasinToSubbasin[chBasin];
                                    }
                                    var SWATBasin = this._gv.topo.subbasinToSWATBasin[basin];
                                    var area = waterBody.area / 10000.0;
                                    // avoid division by zero for zero-area reservoirs
                                    double x;
                                    double y;
                                    double meanElev;
                                    if (waterBody.cellCount == 0) {
                                        x = waterBody.totalLongitude;
                                        y = waterBody.totalLatitude;
                                        meanElev = waterBody.totalElevation;
                                    } else {
                                        x = waterBody.totalLongitude / waterBody.cellCount;
                                        y = waterBody.totalLatitude / waterBody.cellCount;
                                        meanElev = waterBody.totalElevation / waterBody.cellCount;
                                    }
                                    var pt = new double[3] { x, y, 0 };
                                    var centroidll = this._gv.topo.pointToLatLong(pt);
                                    cmd.CommandText = DBUtils._WATERINSERTSQL;
                                    this._db.execCmdQuery(cmd, new object[] { waterBody.id, wCat, lsuId, SWATBasin, area, x, y, centroidll[1], centroidll[0], meanElev });
                                    this._db.addKey("gis_water", waterBody.id);
                                    if (waterBody.isReservoir() && !waterBody.isInlet()) {
                                        // place reservoir point at channel outlet
                                        var channelData0 = this._gv.topo.channelsData[channel0];
                                        this._gv.topo.pointId += 1;
                                        var resPt = new double[3] { channelData0.lowerX, channelData0.lowerY, 0 };
                                        this._gv.topo.foundReservoirs[channel0] = (waterBody.id, this._gv.topo.pointId, resPt);
                                    }
                                }
                            }
                        }
                    }
                    //his._db.hashDbTable(conn, "gis_water");
                    var reservoirsFile = Utils.join(this._gv.shapesDir, "reservoirs.shp");
                    await Utils.removeLayer(reservoirsFile);
                    if (await Delineation.createOutletFile(reservoirsFile, this._gv.demFile, false, this._gv)) {
                        await this.addReservoirsToFile(this._gv.topo.chPointSources, this.mergees, this._gv.topo.foundReservoirs, reservoirsFile);
                    }
                }
                transaction.Commit();
            }
        }

        // Add reservoirs and point sources to outletFile.  Load as layer if not already loaded.
        public async Task addReservoirsToFile(
            Dictionary<int, (int, double[])> ptSources,
            HashSet<int> mergees,
            Dictionary<int, (int, int, double[])> reservoirs,
            string outletFile) {
            OSGeo.OGR.Feature feature;
            using (var outDs = Ogr.Open(outletFile, 1)) {
                var layer = outDs.GetLayerByIndex(0);
                var fields = layer.GetLayerDefn();
                var idIndex = fields.GetFieldIndex(Topology._ID);
                var inletIndex = fields.GetFieldIndex(Topology._INLET);
                var resIndex = fields.GetFieldIndex(Topology._RES);
                var ptsourceIndex = fields.GetFieldIndex(Topology._PTSOURCE);
                var ptIdIndex = fields.GetFieldIndex(Topology._POINTID);
                if (ptIdIndex < 0) {
                    layer.CreateField(new FieldDefn(Topology._POINTID, FieldType.OFTInteger), 1);
                    fields = layer.GetLayerDefn();
                    ptIdIndex = fields.GetFieldIndex(Topology._POINTID);
                }
                foreach (var (_, pointId, point) in reservoirs.Values) {
                    feature = new OSGeo.OGR.Feature(fields);
                    feature.SetField(idIndex, pointId);
                    feature.SetField(inletIndex, 0);
                    feature.SetField(resIndex, 1);
                    feature.SetField(ptsourceIndex, 0);
                    feature.SetField(ptIdIndex, pointId);
                    feature.SetGeometry(Topology.geometryFromPoint(point));
                    layer.CreateFeature(feature);
                }
                foreach (var (channel, (pointId, point)) in ptSources) {
                    // check channel not merged with one upstream
                    // and not inside lake
                    if (!mergees.Contains(channel) && !this._gv.topo.chLinkInsideLake.ContainsKey(channel) && !(this._gv.useGridModel && this._gv.topo.chLinkFromLake.ContainsKey(channel))) {
                        int chBasin;
                        if (!this._gv.topo.chLinkToChBasin.TryGetValue(channel, out chBasin)) { chBasin = -1; }
                        int subbasin;
                        if (!this._gv.topo.chBasinToSubbasin.TryGetValue(chBasin, out subbasin)) { subbasin = -1; }
                        if (subbasin < 0 || this._gv.topo.upstreamFromInlets.Contains(subbasin)) {
                            continue;
                        }
                        feature = new OSGeo.OGR.Feature(fields);
                        feature.SetField(idIndex, pointId);
                        feature.SetField(inletIndex, 1);
                        feature.SetField(resIndex, 0);
                        feature.SetField(ptsourceIndex, 1);
                        feature.SetField(ptIdIndex, pointId);
                        feature.SetGeometry(Topology.geometryFromPoint(point));
                        layer.CreateFeature(feature);
                    }
                }
            }
            var addReservoirLayer = (await Utils.getLayerByFilename(outletFile, FileTypes._EXTRAPTSRCANDRES, this._gv, null, Utils._WATERSHED_GROUP_NAME)).Item1 as FeatureLayer;
            if (addReservoirLayer is null) {
                Utils.error(string.Format("Could not load reservoir file {0}", outletFile), this._gv.isBatch);
                return;
            }
            await QueuedTask.Run(() => addReservoirLayer.SetExpanded(false));
        }

        // Set water body id to rid for upstream components of this reservoir.
        public virtual void propagateReservoirId(int start, int rid, int floodscape) {
            foreach (var channel in this._gv.topo.channelToSWATChannel.Keys) {
                var dsChannel = this._gv.topo.finalDownstream(channel, this.mergedChannels);
                if (dsChannel == start) {
                    int chBasin;
                    if (!this._gv.topo.chLinkToChBasin.TryGetValue(channel, out chBasin)) { chBasin = -1; }
                    int subbasin;
                    if (!this._gv.topo.chBasinToSubbasin.TryGetValue(chBasin, out subbasin)) { subbasin = -1; }
                    BasinData basinData;
                    if (this.basins.TryGetValue(subbasin, out basinData)) {
                        Dictionary<int, LSUData> channelData;
                        if (basinData.getLsus().TryGetValue(channel, out channelData)) {
                            LSUData lsuData;
                            if (channelData.TryGetValue(floodscape, out lsuData)) {
                                var waterBody = lsuData.waterBody;
                                if (waterBody is not null && waterBody.isReservoir() && waterBody.cellCount == 0 && waterBody.id == 0) {
                                    waterBody.id = rid;
                                    this.propagateReservoirId(channel, rid, floodscape);
                                }
                            }
                        }
                    }
                }
            }
        }

        // Return sum of areas of subbasins (land and water) in square metres.
        public virtual double totalBasinsArea() {
            var total = 0.0;
            foreach (var bd in this.basins.Values) {
                foreach (var channelData in bd.getLsus().Values) {
                    foreach (var lsuData in channelData.Values) {
                        total += lsuData.area;
                    }
                }
            }
            return total;
        }

        // Return a map landscape unit -> area (land and water) in square metres across all subbasins.
        public virtual Dictionary<int, double> totalLandscapeAreas() {
            var result = new Dictionary<int, double>();
            foreach (var bd in this.basins.Values) {
                foreach (var channelData in bd.getLsus().Values) {
                    foreach (var (landscape, lsuData) in channelData) {
                        double area;
                        if (!result.TryGetValue(landscape, out area)) { area = 0; }
                        result[landscape] = area + lsuData.area;
                    }
                }
            }
            return result;
        }

        // 
        //         Return maps of crop -> area in square metres across all subbasins.
        //         
        //         If withHRUs, return updated and original values.
        //         Otherise return the original values, and first map is None
        //         
        //         
        public virtual (Dictionary<int, double>, Dictionary<int, double>) totalCropAreas(bool withHRUs) {
            var result1 = withHRUs ? new Dictionary<int, double>() : null;
            var result2 = new Dictionary<int, double>();
            foreach (var bd in this.basins.Values) {
                foreach (var channelData in bd.getLsus().Values) {
                    foreach (var lsuData in channelData.Values) {
                        var map1 = withHRUs ? lsuData.cropAreas : null;
                        var map2 = lsuData.originalCropAreas;
                        if (map1 is not null) {
                            Debug.Assert(result1 is not null);
                            foreach (var (crop, area) in map1) {
                                if (result1.ContainsKey(crop)) {
                                    result1[crop] += area;
                                } else {
                                    result1[crop] = area;
                                }
                            }
                        }
                        foreach (var (crop, area) in map2) {
                            if (result2.ContainsKey(crop)) {
                                result2[crop] += area;
                            } else {
                                result2[crop] = area;
                            }
                        }
                    }
                }
            }
            return (result1, result2);
        }

        // Return map of soil -> area in square metres across all subbasins.
        //         
        //         If withHRUs, return updated and original values.
        //         Otherise return the original values, and first map is None
        //         
        //         
        public virtual (Dictionary<int, double>, Dictionary<int, double>) totalSoilAreas(bool withHRUs) {
            var result1 = withHRUs ? new Dictionary<int, double>() : null;
            var result2 = new Dictionary<int, double>();
            foreach (var bd in this.basins.Values) {
                foreach (var channelData in bd.getLsus().Values) {
                    foreach (var lsuData in channelData.Values) {
                        var map1 = withHRUs ? lsuData.soilAreas : null;
                        var map2 = lsuData.originalSoilAreas;
                        if (map1 is not null) {
                            Debug.Assert(result1 is not null);
                            foreach (var (soil, area) in map1) {
                                if (result1.ContainsKey(soil)) {
                                    result1[soil] += area;
                                } else {
                                    result1[soil] = area;
                                }
                            }
                        }
                        foreach (var (soil, area) in map2) {
                            if (result2.ContainsKey(soil)) {
                                result2[soil] += area;
                            } else {
                                result2[soil] = area;
                            }
                        }
                    }
                }
            }
            return (result1, result2);
        }

        // Return map of slope -> area in square metres across all subbasins.
        //         
        //         If withHRUs, return updated and original values.
        //         Otherise return the original values, and first map is None
        //         
        //         
        public virtual (Dictionary<int, double>, Dictionary<int, double>) totalSlopeAreas(bool withHRUs) {
            var result1 = withHRUs ? new Dictionary<int, double>() : null;
            var result2 = new Dictionary<int, double>();
            foreach (var bd in this.basins.Values) {
                foreach (var channelData in bd.getLsus().Values) {
                    foreach (var lsuData in channelData.Values) {
                        var map1 = withHRUs ? lsuData.slopeAreas : null;
                        var map2 = lsuData.originalSlopeAreas;
                        if (map1 is not null) {
                            Debug.Assert(result1 is not null);
                            foreach (var (slope, area) in map1) {
                                if (result1.ContainsKey(slope)) {
                                    result1[slope] += area;
                                } else {
                                    result1[slope] = area;
                                }
                            }
                        }
                        foreach (var (slope, area) in map2) {
                            if (result2.ContainsKey(slope)) {
                                result2[slope] += area;
                            } else {
                                result2[slope] = area;
                            }
                        }
                    }
                }
            }
            return (result1, result2);
        }

        // Return total reservoir, pond and wetland lakes area in square metres.
        public virtual double totalLakesArea() {
            var result = 0.0;
            foreach (var lakeData in this._gv.topo.lakesData.Values) {
                int subbasin = lakeData.outPoint.Item1;
                if ((this._gv.isHUC || this._gv.isHAWQS) && subbasin < 0) {
                    continue;  // lakes file is larger than watershed for HUC12 models
                }
                if (new HashSet<WaterRole> {
                    WaterRole._RESERVOIR,
                    WaterRole._POND,
                    WaterRole._WETLAND
                }.Contains(lakeData.waterRole)) {
                    result += lakeData.area;
                }
            }
            return result;
        }

        // Return map of reservoir, pond and wetland lake id to lake area in square metres and category.
        public virtual Dictionary<int, (double, string)> getLakeAreas() {
            var result = new Dictionary<int, (double, string)>();
            foreach (var (lakeId, lakeData) in this._gv.topo.lakesData) {
                int subbasin = lakeData.outPoint.Item1;
                if ((this._gv.isHUC || this._gv.isHAWQS) && subbasin < 0) {
                    continue;  // lakes file is larger than watershed for HUC12 models
                }
                if (lakeData.waterRole == WaterRole._RESERVOIR) {
                    result[lakeId] = (lakeData.area, "reservoir");
                } else if (lakeData.waterRole == WaterRole._POND) {
                    result[lakeId] = (lakeData.area, "pond");
                } else if (lakeData.waterRole == WaterRole._WETLAND) {
                    result[lakeId] = (lakeData.area, "wetland");
                }
            }
            return result;
        }

        // Return number of reservoirs, ponds, wetlands and playas in lakes shapefile.
        public virtual (int, int, int, int) countLakes() {
            var numRes = 0;
            var numPond = 0;
            var numWetland = 0;
            var numPlaya = 0;
            foreach (var lakedata in this._gv.topo.lakesData.Values) {
                int subbasin = lakedata.outPoint.Item1;
                if ((this._gv.isHUC || this._gv.isHAWQS) && subbasin < 0) {
                    continue;  // lakes file is larger than watershed for HUC12 models
                }
                if (lakedata.waterRole == WaterRole._RESERVOIR) {
                    numRes += 1;
                } else if (lakedata.waterRole == WaterRole._POND) {
                    numPond += 1;
                } else if (lakedata.waterRole == WaterRole._WETLAND) {
                    numWetland += 1;
                } else {
                    numPlaya += 1;
                }
            }
            if (this._gv.isHUC || this._gv.isHAWQS) {
                //  playas not included in lakesData as generated from unconnected lakes
                numPlaya += this._gv.playaCount;
            }
            return (numRes, numPond, numWetland, numPlaya);
        }

        // Return total water bodies (reservoirs and ponds) area in square metres across all subbasins.
        public virtual double totalWaterBodiesArea() {
            var result = 0.0;
            foreach (var basinData in this.basins.Values) {
                foreach (var channelData in basinData.getLsus().Values) {
                    foreach (var lsuData in channelData.Values) {
                        if (lsuData.waterBody is not null && !lsuData.waterBody.isUnknown()) {
                            result += lsuData.waterBody.originalArea;
                        }
                    }
                }
            }
            return result;
        }

        // Return water bodies (reservoirs and ponds) area for basinData.
        public virtual double basinWaterBodiesArea(BasinData basinData) {
            var result = 0.0;
            foreach (var channelData in basinData.getLsus().Values) {
                // channel, channelData in basinData.getLsus():
                foreach (var lsuData in channelData.Values) {
                    if (lsuData.waterBody is not null && !lsuData.waterBody.isUnknown()) {
                        result += lsuData.waterBody.originalArea;
                        // SWATChannel = self._gv.topo.channelToSWATChannel[channel]
                        // Utils.loginfo('Channel {0} has reservoir or pond area {1}, originally {2}'.
                        //                    format(SWATChannel, result / 10000, lsuData.waterBody.originalArea / 10000))
                    }
                }
            }
            return result;
        }

        // Print a line containing the channel identifier, area in hectares.
        //         percent of total1, , percent of total2 (unless zero)
        public virtual void printChannelArea(
            int SWATChannel,
            double areaSqM,
            bool withHRUs,
            double total1,
            double total2,
            StreamWriter fw) {
            var width1 = withHRUs ? 30 : 15;
            var width2 = withHRUs ? 23 : 15;
            var area = areaSqM / 10000.0;
            var channelId = $"Channel {SWATChannel}";
            var string0 = string.Format("{0:F2}", area).PadLeft(15);
            if (this._gv.useLandscapes) {
                fw.Write(channelId.PadRight(30) + string0);
            } else {
                var lsuId = Utils.landscapeUnitId(SWATChannel, Utils._NOLANDSCAPE);
                fw.Write(string.Format("{0} (LSU {1})", channelId, lsuId).PadRight(30) + string0);
            }
            if (total1 > 0) {
                var percent1 = area / total1 * 100;
                var string1 = string.Format("{0:F2}", percent1).PadLeft(width1);
                fw.Write(string1);
                if (total2 > 0) {
                    var percent2 = area / total2 * 100;
                    var string2 = string.Format("{0:F2}", percent2).PadLeft(width2);
                    fw.Write(string2);
                }
            }
            fw.WriteLine("");
        }

        //  Print a line containing landscape name, area in hectares, 
        //         percent of total1, percent of total2 (unless zero).
        public virtual void printLandscapeAreas(
            Dictionary<int, double> landscapeAreas,
            bool withHRUs,
            double total1,
            double total2,
            StreamWriter fw) {
            var width1 = withHRUs ? 30 : 15;
            var width2 = withHRUs ? 23 : 15;
            // seems natural to list these in numeric order
            foreach (var landscape in new List<int> {
                Utils._FLOODPLAIN,
                Utils._UPSLOPE
            }) {
                double areaM;
                if (landscapeAreas.TryGetValue(landscape, out areaM)) {
                    var lsuName = Utils.landscapeName(landscape, this._gv.useLeftRight);
                    var area = areaM / 10000.0;
                    var string0 = string.Format("{0:F2}", area).PadLeft(15);
                    fw.Write(lsuName.PadLeft(30) + string0);
                    if (total1 > 0) {
                        var percent1 = area / total1 * 100;
                        var string1 = string.Format("{0:F2}", percent1).PadLeft(width1);
                        fw.Write(string1);
                        if (total2 > 0) {
                            var percent2 = area / total2 * 100;
                            var string2 = string.Format("{0:F2}", percent2).PadLeft(width2);
                            fw.Write(string2);
                        }
                    }
                    fw.WriteLine("");
                }
            }
        }

        //  Print a line containing crop, area in hectares, 
        //         percent of total1, percent of total2.
        //         
        //         If cropAreas is not None, use its figures and add original figures in bracket for comparison.
        //         
        public virtual void printCropAreas(
            Dictionary<int, double> cropAreas,
            Dictionary<int, double> originalCropAreas,
            double total1,
            double total2,
            StreamWriter fw) {
            double opercent2;
            double opercent1;
            double originalArea = 0;
            string landuseCode;
            Dictionary<int, double> original;
            Dictionary<int, double> main;
            if (cropAreas is null || cropAreas.Count == 0) {
                main = originalCropAreas;
                original = null;
            } else {
                main = cropAreas;
                original = originalCropAreas;
            }
            foreach (var (crop, areaM) in main) {
                landuseCode = this._db.getLanduseCode(crop);
                if (landuseCode.ToUpper() == "WATR") {
                    landuseCode = "WETW";
                }
                var area = areaM / 10000;
                var string0 = string.Format("{0:F2}", area).PadLeft(15);
                if (original is not null) {
                    // crop may not have been in original because of splitting
                    if (!original.TryGetValue(crop, out originalArea)) { originalArea = 0; }
                    originalArea /= 10000;
                    string0 += string.Format("({0:F2})", originalArea).PadLeft(15);
                }
                fw.Write(landuseCode.PadLeft(30) + string0);
                if (total1 > 0) {
                    var percent1 = area / total1 * 100;
                    var string1 = string.Format("{0:F2}", percent1).PadLeft(15);
                    if (original is not null) {
                        opercent1 = originalArea / total1 * 100;
                        string1 += string.Format("({0:F2})", opercent1).PadLeft(8);
                    }
                    fw.Write(string1);
                    if (total2 > 0) {
                        var percent2 = area / total2 * 100;
                        var string2 = string.Format("{0:F2}", percent2).PadLeft(15);
                        if (original is not null) {
                            opercent2 = originalArea / total2 * 100;
                            string2 += string.Format("({0:F2})", opercent2).PadLeft(8);
                        }
                        fw.Write(string2);
                    }
                }
                fw.WriteLine("");
            }
            // if have original, add entries for originals that have been removed
            if (original is not null) {
                foreach (var (crop, areaM) in original) {
                    if (!main.ContainsKey(crop)) {
                        landuseCode = this._db.getLanduseCode(crop);
                        if (landuseCode.ToUpper() == "WATR") {
                            landuseCode = "WETW";
                        }
                        originalArea = areaM / 10000;
                        fw.Write(landuseCode.PadLeft(30) + string.Format("({0:F2})", originalArea).PadLeft(30));
                        if (total1 > 0) {
                            opercent1 = originalArea / total1 * 100;
                            fw.Write(string.Format("({0:F2})", opercent1).PadLeft(23));
                        }
                        if (total2 > 0) {
                            opercent2 = originalArea / total2 * 100;
                            fw.Write(string.Format("({0:F2})", opercent2).PadLeft(23));
                        }
                        fw.WriteLine("");
                    }
                }
            }
        }

        //  Print a line containing soil, area in hectares, 
        //         percent of total1, percent of total2.
        //         
        //         If soilAreas is not None, use its figures and add original figures in bracket for comparison.
        //         
        public virtual void printSoilAreas(
            Dictionary<int, double> soilAreas,
            Dictionary<int, double> originalSoilAreas,
            double total1,
            double total2,
            StreamWriter fw) {
            double opercent2;
            double opercent1;
            double originalArea = 0;
            string soilName;
            Dictionary<int, double> original;
            Dictionary<int, double> main;
            if (soilAreas is null || soilAreas.Count == 0) {
                main = originalSoilAreas;
                original = null;
            } else {
                main = soilAreas;
                original = originalSoilAreas;
            }
            foreach (var (soil, areaM) in main) {
                soilName = this._db.getSoilName(soil).Item1;
                var area = areaM / 10000;
                var string0 = string.Format("{0:F2}", area).PadLeft(15);
                if (original is not null) {
                    originalArea = original[soil] / 10000;
                    string0 += string.Format("({0:F2})", originalArea).PadLeft(15);
                }
                fw.Write(soilName.PadLeft(30) + string0);
                if (total1 > 0) {
                    var percent1 = area / total1 * 100;
                    var string1 = string.Format("{0:F2}", percent1).PadLeft(15);
                    if (original is not null) {
                        opercent1 = originalArea / total1 * 100;
                        string1 += string.Format("({0:F2})", opercent1).PadLeft(8);
                    }
                    fw.Write(string1);
                    if (total2 > 0) {
                        var percent2 = area / total2 * 100;
                        var string2 = string.Format("{0:F2}", percent2).PadLeft(15);
                        if (original is not null) {
                            opercent2 = originalArea / total2 * 100;
                            string2 += string.Format("({0:F2})", opercent2).PadLeft(8);
                        }
                        fw.Write(string2);
                    }
                }
                fw.WriteLine("");
            }
            // if have original, add entries for originals that have been removed
            if (original is not null) {
                foreach (var (soil, areaM) in original) {
                    if (!main.ContainsKey(soil)) {
                        soilName = this._db.getSoilName(soil).Item1;
                        originalArea = areaM / 10000;
                        fw.Write(soilName.PadLeft(30) + string.Format("({0:F2})", originalArea).PadLeft(30));
                        if (total1 > 0) {
                            opercent1 = originalArea / total1 * 100;
                            fw.Write(string.Format("({0:F2})", opercent1).PadLeft(23));
                        }
                        if (total2 > 0) {
                            opercent2 = originalArea / total2 * 100;
                            fw.Write(string.Format("({0:F2})", opercent2).PadLeft(23));
                        }
                        fw.WriteLine("");
                    }
                }
            }
        }

        //  Print a line containing slope, area in hectares, 
        //         percent of total1, percent of total2.
        //         
        //         If slopeAreas is not None, use its figures and add original figures in bracket for comparison.
        //         
        public virtual void printSlopeAreas(
            Dictionary<int, double> slopeAreas,
            Dictionary<int, double> originalSlopeAreas,
            double total1,
            double total2,
            StreamWriter fw) {
            double opercent2;
            double opercent1;
            double originalArea = 0;
            string slopeRange;
            Dictionary<int, double> original;
            Dictionary<int, double> main;
            if (slopeAreas is null || slopeAreas.Count == 0) {
                main = originalSlopeAreas;
                original = null;
            } else {
                main = slopeAreas;
                original = originalSlopeAreas;
            }
            // seems natural to list these in increasing order
            foreach (var i in Enumerable.Range(0, this._db.slopeLimits.Count + 1)) {
                if (main.ContainsKey(i)) {
                    slopeRange = this._db.slopeRange(i);
                    var area = main[i] / 10000;
                    var string0 = string.Format("{0:F2}", area).PadLeft(15);
                    if (original is not null) {
                        originalArea = original[i] / 10000;
                        string0 += string.Format("({0:F2})", originalArea).PadLeft(15);
                    }
                    fw.Write(slopeRange.PadLeft(30) + string0);
                    if (total1 > 0) {
                        var percent1 = area / total1 * 100;
                        var string1 = string.Format("{0:F2}", percent1).PadLeft(15);
                        if (original is not null) {
                            opercent1 = originalArea / total1 * 100;
                            string1 += string.Format("({0:F2})", opercent1).PadLeft(8);
                        }
                        fw.Write(string1);
                        if (total2 > 0) {
                            var percent2 = area / total2 * 100;
                            var string2 = string.Format("{0:F2}", percent2).PadLeft(15);
                            if (original is not null) {
                                opercent2 = originalArea / total2 * 100;
                                string2 += string.Format("({0:F2})", opercent2).PadLeft(8);
                            }
                            fw.Write(string2);
                        }
                    }
                    fw.WriteLine("");
                }
            }
            // if have original, add entries for originals that have been removed
            if (original is not null) {
                foreach (var i in Enumerable.Range(0, this._db.slopeLimits.Count + 1)) {
                    if (original.ContainsKey(i) && !main.ContainsKey(i)) {
                        slopeRange = this._db.slopeRange(i);
                        originalArea = original[i] / 10000;
                        fw.Write(slopeRange.PadLeft(30) + string.Format("({0:F2})", originalArea).PadLeft(30));
                        if (total1 > 0) {
                            opercent1 = originalArea / total1 * 100;
                            fw.Write(string.Format("({0:F2})", opercent1).PadLeft(23));
                        }
                        if (total2 > 0) {
                            opercent2 = originalArea / total2 * 100;
                            fw.Write(string.Format("({0:F2})", opercent2).PadLeft(23));
                        }
                        fw.WriteLine("");
                    }
                }
            }
        }

        // Print a line for lake num (or lakes if num is zero) with area in hectares and percent of total.
        public virtual void printLakeArea(
            (double, string) areaCat,
            double total,
            int num,
            bool withHRUs,
            StreamWriter fw) {
            var (area, category) = areaCat;
            var areaHa = area / 10000.0;
            if (total > 0) {
                var string0 = num > 0 ? string.Format("Lake {0} ({1})", num, category) : "Lakes (reservoirs, ponds and wetlands)";
                var string1 = string.Format("{0:F2}", areaHa).PadLeft(15);
                var percent = areaHa / total * 100;
                var just2 = withHRUs ? 30 : 15;
                var string2 = string.Format("{0:F2}", percent).PadLeft(just2);
                fw.WriteLine(string0.PadRight(30) + string1 + string2);
            }
        }

        // Print a line containing area in hectares,  
        //         percent of total1, percent of total2.
        //         
        //         Bracketed figures are unnecessary because we know water area does not change when HRUs are formed.
        public virtual void printWaterArea(
            double area,
            double total1,
            double total2,
            bool withHRUs,
            StreamWriter fw) {
            var areaHa = area / 10000;
            if (total1 > 0) {
                var string0 = string.Format("{0:F2}", areaHa).PadLeft(15);
                fw.Write("Reservoirs and ponds".PadRight(30) + string0);
                var percent1 = areaHa / total1 * 100;
                var just1 = withHRUs ? 30 : 15;
                var string1 = string.Format("{0:F2}", percent1).PadLeft(just1);
                fw.Write(string1);
                if (total2 > 0) {
                    var percent2 = areaHa / total2 * 100;
                    var just2 = withHRUs ? 23 : 15;
                    var string2 = string.Format("{0:F2}", percent2).PadLeft(just2);
                    fw.Write(string2);
                }
            }
            fw.WriteLine("");
        }
    }

    // Data and functions for creating HRUs.
    public class HRUs
    {
        public ArcSWATPlus _parent;

        public DBUtils _db;

        public HRUsForm _dlg;

        public GlobalVars _gv;

        public ComboBox _reportsCombo;

        public bool completed;

        public CreateHRUs CreateHRUs;

        public string landuseFile;

        public RasterLayer landuseLayer;

        public string landuseTable;

        public string soilFile;

        public RasterLayer soilLayer;

        public string soilTable;

        public HRUs(GlobalVars gv, ComboBox reportsCombo, ArcSWATPlus parent) {
            this._gv = gv;
            this._db = gv.db;
            this._parent = parent;
            this._dlg = new HRUsForm(gv, this);
            //this._dlg.setWindowFlags(this._dlg.windowFlags() & ~Qt.WindowContextHelpButtonHint & Qt.WindowMinimizeButtonHint);
            //this._dlg.move(this._gv.hrusPos);
            this._reportsCombo = reportsCombo;
            //# start in landuse and soil tabs
            this._dlg.mainTab.SelectedIndex = 0;
            this._dlg.tab2.Enabled = false;
            //# Landuse grid
            this.landuseFile = "";
            //# Soil grid
            this.soilFile = "";
            //# Landuse lookup table
            this.landuseTable = "";
            //# Soil lookup table
            this.soilTable = "";
            //# Landuse grid layer
            this.landuseLayer = null;
            //# Soil grid layer
            this.soilLayer = null;
            //# CreateHRUs object
            this.CreateHRUs = new CreateHRUs(gv, reportsCombo, this._dlg);
            //# Flag to indicate completion
            this.completed = false;
        }

        //// Set up HRUs dialog.
        //public virtual void init() {
        //    this._db.populateTableNames();
        //    this._dlg.selectLanduseTable.Items.AddRange(this._db.landuseTableNames.ToArray());
        //    this._dlg.selectLanduseTable.Items.Add(Parameters._USECSV);
        //    this._dlg.selectSoilTable.Items.AddRange(this._db.soilTableNames.ToArray());
        //    this._dlg.selectSoilTable.Items.Add(Parameters._USECSV);
        //    this.readProj();
        //    this.setLanduseData();
        //    this.setSoilData();
        //    //this._dlg.usersoilButton.toggled.connect(this.setSoilData);
        //    //this._dlg.STATSGOButton.toggled.connect(this.setSoilData);
        //    //this._dlg.SSURGOButton.toggled.connect(this.setSoilData);
        //    //this._dlg.selectPlantSoilDatabaseButton.clicked.connect(this.selectPlantSoilDatabase);
        //    this._gv.getExemptSplit();
        //    this._dlg.subbasinsLabel.Text = "";
        //    this._dlg.channelsLabel.Text = "";
        //    this._dlg.fullHRUsLabel.Text = "";
        //    this._dlg.optionGroup.Enabled = true;
        //    this._dlg.landuseSoilSlopeGroup.Enabled = false;
        //    this._dlg.areaGroup.Enabled = false;
        //    this._dlg.targetGroup.Enabled = false;
        //    this._dlg.createButton.Enabled = false;
        //    this._dlg.getProgressBar.Visible = false;
        //    this._dlg.shortChannelGroup.Visible = !this._gv.useGridModel);
        //    this._dlg.remerge.Visible = !this._gv.useGridModel);
        //    /this._dlg.channelPercentButton.toggled.connect(this.setChannelChoice);
        //    //this._dlg.channelAreaButton.toggled.connect(this.setChannelChoice);
        //    //this._dlg.channelMergeVal.setValidator(QIntValidator());
        //    //this._dlg.channelMergeVal.textChanged.connect(this.readChannelThreshold);
        //    //this._dlg.channelMergeSlider.valueChanged.connect(this.changeChannelThreshold);
        //    //this._dlg.reservoirThreshold.valueChanged.connect(this.setRead);
        //    this.setRead();
        //    this._dlg.floodplainCombo.activated.connect(this.setToReadFromMaps);
        //    this._dlg.readFromMaps.toggled.connect(this.setReadChoice);
        //    this._dlg.readFromPrevious.toggled.connect(this.setReadChoice);
        //    this._dlg.dominantHRUButton.toggled.connect(this.setHRUChoice);
        //    this._dlg.dominantLanduseButton.toggled.connect(this.setHRUChoice);
        //    this._dlg.filterLanduseButton.toggled.connect(this.setHRUChoice);
        //    this._dlg.filterAreaButton.toggled.connect(this.setHRUChoice);
        //    this._dlg.targetButton.toggled.connect(this.setHRUChoice);
        //    this._dlg.percentButton.toggled.connect(this.setHRUChoice);
        //    this._dlg.areaButton.toggled.connect(this.setHRUChoice);
        //    this._dlg.slopeBrowser.Text = Utils.slopesToString(this._db.slopeLimits));
        //    this._dlg.selectLanduseButton.clicked.connect(this.getLanduseFile);
        //    this._dlg.selectSoilButton.clicked.connect(this.getSoilFile);
        //    this._dlg.selectLanduseTable.activated.connect(this.setLanduseTable);
        //    this._dlg.selectSoilTable.activated.connect(this.setSoilTable);
        //    this._dlg.selectPlantTable.activated.connect(this.setPlantTable);
        //    this._dlg.selectUrbanTable.activated.connect(this.setUrbanTable);
        //    this._dlg.selectUsersoilTable.activated.connect(this.setUsersoilTable);
        //    this._dlg.insertButton.clicked.connect(this.insertSlope);
        //    this._dlg.landuseVal.setValidator(QIntValidator());
        //    this._dlg.soilVal.setValidator(QIntValidator());
        //    this._dlg.slopeVal.setValidator(QIntValidator());
        //    this._dlg.areaVal.setValidator(QIntValidator());
        //    this._dlg.targetVal.setValidator(QIntValidator());
        //    this._dlg.slopeBand.setValidator(QDoubleValidator());
        //    this._dlg.floodplainCombo.Items.Add("Select floodplain map (optional)");
        //    this._dlg.floodplainCombo.setCurrentIndex(0);
        //    this.initFloodplain();
        //    // if as here you use returnPressed setting in a LineEdit box make sure all buttons in the 
        //    // dialog have default and autoDefault set to False (use QT designer, which 
        //    // by default sets autoDefault to True)
        //    this._dlg.slopeBand.returnPressed.connect(this.insertSlope);
        //    this._dlg.clearButton.clicked.connect(this.clearSlopes);
        //    this._dlg.readButton.clicked.connect(this.readFiles);
        //    this._dlg.createButton.clicked.connect(this.calcHRUs);
        //    this._dlg.cancelButton.clicked.connect(this._dlg.close);
        //    this._dlg.areaVal.textChanged.connect(this.readAreaThreshold);
        //    this._dlg.areaSlider.valueChanged.connect(this.changeAreaThreshold);
        //    this._dlg.landuseVal.textChanged.connect(this.readLanduseThreshold);
        //    this._dlg.landuseSlider.valueChanged.connect(this.changeLanduseThreshold);
        //    this._dlg.soilVal.textChanged.connect(this.readSoilThreshold);
        //    this._dlg.soilSlider.valueChanged.connect(this.changeSoilThreshold);
        //    this._dlg.targetVal.textChanged.connect(this.readTargetThreshold);
        //    this._dlg.targetSlider.valueChanged.connect(this.changeTargetThreshold);
        //    this._dlg.slopeVal.textChanged.connect(this.readSlopeThreshold);
        //    this._dlg.slopeSlider.valueChanged.connect(this.changeSlopeThreshold);
        //    this._dlg.landuseButton.clicked.connect(this.setLanduseThreshold);
        //    this._dlg.soilButton.clicked.connect(this.setSoilThreshold);
        //    this._dlg.exemptButton.clicked.connect(this.doExempt);
        //    this._dlg.splitButton.clicked.connect(this.doSplit);
        //    this._dlg.elevBandsButton.clicked.connect(this.doElevBands);
        //    this._dlg.HRUsTab.currentChanged.connect(this.switchTabs);
        //}

        //// Debugging information when tab switched in HRUs form.
        //public virtual void switchTabs(int i) {
        //    // @UnusedVariable
        //    //Utils.information('Switched to tab {}', i), False)
        //    this._dlg.progress("");
        //}

        // 
        //         Add floodplain rasters to floodplainCombo.  Disable the combo if there are none.
        //         
        //         Potential floodplain rasters are of the form *flood*.tif in the flood directory.
        //         Also sets combo current item according to (previously read) project file.
        //         
        //public virtual void initFloodplain() {
        //    var pattern = Utils.join(this._gv.floodDir, "*.tif");
        //    var fs = glob.glob(pattern);
        //    if (fs.Count == 0) {
        //        this._dlg.floodplainCombo.Enabled = false;
        //        this._gv.useLandscapes = false;
        //    }
        //    foreach (var f in fs) {
        //        this._dlg.floodplainCombo.Items.Add(os.path.split(f)[1]);
        //    }
        //    if (this._gv.useLandscapes) {
        //        var index = this._dlg.floodplainCombo.FindString(os.path.split(this._gv.floodFile)[1]);
        //        if (index > 0) {
        //            this._dlg.floodplainCombo.setCurrentIndex(index);
        //        }
        //    }
        //}

        // Run HRUs dialog.
        public async Task<int> run() {
            await this._dlg.init();
            this._dlg.Show();
            this._dlg.progress("");
            //var result = this._dlg.exec_();
            // TODO: result is always zero. Need to reset to discover if CreateHRUs was run successfully
            //this._gv.hrusPos = this._dlg.pos();
            if (this.completed) {
                return 1;
            } else {
                return 0;
            }
        }

        // Return true if HRUs are up to date, else false.
        //         
        //         Requires:
        //         - subs.shp used by visualize no earlier than watershed shapefile
        //         - gis_hrus table in project database no earlier than watershed shapefile
        //         - project_config table says hrus done
        //         
        public virtual bool HRUsAreCreated() {
            object local_time;
            try {
                if (!this._gv.useGridModel) {
                    // TODO: currently grid model does not write the subs.shp file
                    // first check subsFile is up to date
                    var subsFile = Utils.join(this._gv.resultsDir, Parameters._SUBS + ".shp");
                    if (File.Exists(subsFile)) {
                        local_time = File.GetLastWriteTime(subsFile);
                        Utils.loginfo(string.Format("subs.shp last modified {0}", local_time));
                    }
                    if (File.Exists(this._gv.basinFile)) {
                        local_time = File.GetLastWriteTime(this._gv.basinFile);
                        Utils.loginfo(string.Format("Basin file last modified {0}", local_time));
                    }
                    //===================================================================
                    // return Utils.isUpToDate(self._gv.wshedFile, subsFile) and \
                    //         self._db.tableIsUpToDate(self._gv.wshedFile, 'Watershed') and \
                    //         self._db.tableIsUpToDate(self._gv.wshedFile, 'hrus')
                    //===================================================================
                    if (!Utils.isUpToDate(this._gv.basinFile, subsFile)) {
                        Utils.loginfo("HRUSAreCreated failed: subs.shp not up to date");
                        return false;
                    }
                }
                if (!this._db.hasData("gis_hrus")) {
                    Utils.loginfo("HRUSAreCreated failed: hrus table missing or empty");
                    return false;
                }
                return this._gv.isHRUsDone();
            }
            catch (Exception) {
                return false;
            }
        }

        // no longer used - too slow for large BASINSDATA files   
        //===============================================================================
        //     def tryRun(self):
        //         """Try rerunning with existing data and choices.  Fail quietly and return false if necessary, else return true."""
        //         try:
        //             self.init()
        //             if not self._db.hasData('BASINSDATA'): 
        //                 Utils.loginfo('HRUs tryRun failed: no basins data')
        //                 return False
        //             if not self.initLanduses(self.landuseTable):
        //                 Utils.loginfo('HRUs tryRun failed: cannot initialise landuses')
        //                 return False
        //             if not self.initSoils(self.soilTable):
        //                 Utils.loginfo('HRUs tryRun failed: cannot initialise soils')
        //                 return False
        //             time1 = time.process_time()
        //             self.CreateHRUs.basins, OK = self._db.regenerateBasins(True) 
        //             if not OK:
        //                 Utils.loginfo('HRUs tryRun failed: could not regenerate basins')
        //                 return False
        //             time2 = time.process_time()
        //             Utils.loginfo('Reading from database took {0} seconds', int(time2 - time1)))
        //             self.CreateHRUs.saveAreas(True)
        //             if self._gv.useGridModel:
        //                 self.rewriteWHUTables()
        //             else:
        //                 self._reportsCombo.Visible = True)
        //                 if self._reportsCombo.FindString(Parameters._TOPOITEM) < 0:
        //                     self._reportsCombo.Items.Add(Parameters._TOPOITEM)
        //                 if self._reportsCombo.FindString(Parameters._BASINITEM) < 0:
        //                     self._reportsCombo.Items.Add(Parameters._BASINITEM)
        //                 if self.CreateHRUs.isMultiple:
        //                     if self.CreateHRUs.isArea:
        //                         self.CreateHRUs.removeSmallHRUsByArea()
        //                     elif self.CreateHRUs.isTarget:
        //                         self.CreateHRUs.removeSmallHRUsbyTarget()
        //                     else:
        //                         if len(self._db.slopeLimits) == 0: self.CreateHRUs.slopeVal = 0
        //                         # allow too tight thresholds, since we guard against removing all HRUs from a subbasin
        //                         # if not self.CreateHRUs.cropSoilAndSlopeThresholdsAreOK():
        //                         #     Utils.error('Internal error: problem with tight thresholds', self._gv.isBatch)
        //                         #     return
        //                         if self.CreateHRUs.useArea:
        //                             self.CreateHRUs.removeSmallHRUsByThresholdArea()
        //                         else:
        //                             self.CreateHRUs.removeSmallHRUsByThresholdPercent()
        //                     if not self.CreateHRUs.splitHRUs():
        //                         return False
        //                 self.CreateHRUs.saveAreas(False)
        //                 self.CreateHRUs.basinsToHRUs()
        //                 if self._reportsCombo.FindString(Parameters._HRUSITEM) < 0:
        //                     self._reportsCombo.Items.Add(Parameters._HRUSITEM)
        //                 time1 = time.process_time()
        //                 self.CreateHRUs.writeSubbasinsAndLandscapeTables(root)
        //                 time2 = time.process_time()
        //                 Utils.loginfo('Writing subbasins and lsus tables took {0} seconds', int(time2 - time1)))
        //             self._gv.writeProjectConfig(-1, 1)
        //             return True
        //         except Exception as e:
        //             Utils.loginfo('HRUs tryRun failed: {0}', repr(e)))
        //             return False
        //===============================================================================
        // Read landuse and soil data from files 
        //         or from previous run stored in project database
        //         or redo merge of short channels.
        //         
        public async Task<bool> readFiles() {
            DateTime time2;
            DateTime time1;
            string soil;
            string luse;
            bool OK = false;
            MessageBoxResult res;
            if (this._dlg.remergeChecked) {
                this._dlg.setChannelMergeChoice();
                if (this._dlg.channelMergeSliderValue > 0) {
                    this.CreateHRUs.mergeChannels();
                } else {
                    // restore channel basin areas to original
                    this._gv.topo.chBasinAreas = Topology.copyBasinAreas(this._gv.topo.origChBasinAreas);
                    // remove all channel merges
                    foreach (var basinData in this.CreateHRUs.basins.Values) {
                        basinData.mergedLsus = null;
                    }
                }
                await this.CreateHRUs.printBasins(false);
                this._dlg.channelsLabelText = string.Format("Channels count: {0}", this.CreateHRUs.countChannels());
                this._dlg.fullHRUsLabelText = string.Format("Full HRUs count: {0}", this.CreateHRUs.countHRUs());
                // write landuse, soil, landscape choices in case of failure later
                this._dlg.saveProjPart1();
                this._dlg.tab2.Enabled = true;
                this._dlg.hruChoiceGroupEnabled = true;
                this._dlg.areaPercentChoiceGroupEnabled = true;
                this._dlg.splitButtonEnabled = true;
                this._dlg.exemptButtonEnabled = true;
                this._dlg.setHRUChoice();
                return true;
            }
            // check with user if there is a visible floodplain layer but no floodplain raster selected
            if (!this._gv.isBatch && this._dlg.readFromMapsChecked && this._dlg.floodplainComboBox.SelectedIndex == 0 && this.hasVisibleFloodplainLayer()) {
                res = Utils.question("You have at least one floodplain map available.  Would you like to select one?", false, false);
                if (res == MessageBoxResult.Yes) {
                    return false;
                }
            }
            // check if there is a slope limit not yet inserted
            if (!this._gv.isBatch && this._dlg.readFromMapsChecked && this._dlg.slopeBandText != "") {
                res = Utils.question("You seem to be about to insert a slope limit.  Would you like to complete that?", false, false);
                if (res == MessageBoxResult.Yes) {
                    return false;
                }
            }
            this._gv.writeProjectConfig(-1, 0);
            this._dlg.tab2.Enabled = false;
            // don't hide undefined soil and landuse errors from previous run
            this._db._undefinedLanduseIds = new List<int>();
            this._db._undefinedSoilIds = new List<int>();
            this._dlg.slopeGroupEnabled = false;
            this._dlg.generateFullHRUsEnabled = false;
            this._dlg.elevBandsButtonEnabled = false;
            if (!File.Exists(this.landuseFile)) {
                Utils.error("Please select a landuse file", this._gv.isBatch);
                return false;
            }
            if (!File.Exists(this.soilFile)) {
                Utils.error("Please select a soil file", this._gv.isBatch);
                return false;
            }
            this._gv.landuseFile = this.landuseFile;
            this._gv.soilFile = this.soilFile;
            if (this._gv.isBatch) {
                // use names from project file settings
                luse = this.landuseTable;
                soil = this.soilTable;
            } else {
                // allow user to choose
                luse = "";
                soil = "";
            }
            this._dlg.progress("Checking landuses ...");
            using (new CursorWait()) {
                if (!this.initLanduses(luse)) {
                    this._dlg.progress("");
                    return false;
                }
                //Utils.information('Using {0} as landuse table', self.landuseTable), self._gv.isBatch)
                this._dlg.progress("Checking soils ...");
                if (!this.initSoils(soil)) {
                    this._dlg.progress("");
                    return false;
                }
                // write landuse, soil, landscape choices in case of failure later
                this._dlg.saveProjPart1();
                //Utils.information('Using {0} as soil table', self.soilTable), self._gv.isBatch)
                if (this._gv.isBatch) {
                    Utils.information(string.Format("Landuse file: {0}", Path.GetFileName(this.landuseFile)), true);
                    Utils.information(string.Format("Landuse lookup table: {0}", this.landuseTable), true);
                    Utils.information(string.Format("Soil file: {0}", Path.GetFileName(this.soilFile)), true);
                    Utils.information(string.Format("Soil lookup table: {0}", this.soilTable), true);
                }
                if (this._dlg.readFromPreviousChecked) {
                    // read from database
                    this._dlg.progress("Reading basin data from database ...");
                    time1 = DateTime.Now;
                    (this.CreateHRUs.basins, OK) = this._db.regenerateBasins(this._gv);
                    //if (OK) {
                    //    OK = this._gv.topo.readLakesData(this._gv.db);
                    //}
                    time2 = DateTime.Now;
                    Utils.loginfo(string.Format("Reading from previous took {0} seconds", Convert.ToInt32(time2.Subtract(time1).TotalSeconds)));
                    this._dlg.progress("");
                    if (OK) {
                        this.CreateHRUs.saveAreas(true);
                        if (!this._gv.useGridModel) {
                            if (this._dlg.channelMergeSliderValue > 0) {
                                this._dlg.setChannelMergeChoice();
                                this.CreateHRUs.mergeChannels();
                            }
                        }
                        await this.CreateHRUs.printBasins(false);
                        this._reportsCombo.Visible = true;
                        if (this._reportsCombo.FindString(Parameters._TOPOITEM) < 0) {
                            this._reportsCombo.Items.Add(Parameters._TOPOITEM);
                        }
                        if (this._reportsCombo.FindString(Parameters._BASINITEM) < 0) {
                            this._reportsCombo.Items.Add(Parameters._BASINITEM);
                        }
                        this._dlg.tab2.Enabled = true;
                    }
                } else {
                    this._dlg.progress("Reading rasters ...");
                    this._gv.useLandscapes = false;
                    if (this._dlg.floodplainComboBox.Enabled && this._dlg.floodplainComboBox.SelectedIndex > 0) {
                        var floodFile = this._dlg.floodplainComboBox.Text;
                        var floodPath = Utils.join(this._gv.floodDir, floodFile);
                        if (File.Exists(floodPath)) {
                            this._gv.useLandscapes = true;
                            this._gv.floodFile = floodPath;
                        }
                    }
                    this._dlg.getProgressBar.Value = 0;
                    this._dlg.getProgressBar.Visible = true;
                    await Utils.removeLayer(this._gv.fullLSUsFile);
                    await Utils.removeLayer(this._gv.actLSUsFile);
                    if (this._dlg.generateFullHRUsChecked) {
                        this.CreateHRUs.fullHRUsWanted = true;
                        await Utils.removeLayer(this._gv.fullHRUsFile);
                        await Utils.removeLayer(this._gv.actHRUsFile);
                    } else {
                        // remove any full and actual HRUs layers and files
                        this.CreateHRUs.fullHRUsWanted = false;
                        var fullHRUsLayer = Utils.getLayerByLegend(Utils._FULLHRUSLEGEND);
                        if (fullHRUsLayer is not null) {
                            var fullHRUsFile = await Utils.layerFilename(fullHRUsLayer);
                            await Utils.removeLayer(fullHRUsFile);
                            ArcGIS.Desktop.Mapping.Layer actHRUsLayer = Utils.getLayerByLegend(Utils._ACTHRUSLEGEND);
                            if (actHRUsLayer is not null) {
                                var actHRUsFile = await Utils.layerFilename(actHRUsLayer);
                                await Utils.removeLayer(actHRUsFile);
                            }
                        }
                    }
                    time1 = DateTime.Now;
                    OK = await this.CreateHRUs.generateBasins(this._dlg.getProgressBar);
                    time2 = DateTime.Now;
                    Utils.loginfo(string.Format("Reading from files took {0} seconds", Convert.ToInt32(time2.Subtract(time1).TotalSeconds)));
                    this._dlg.progress("");
                    // now have occurrences of landuses, so can make proper colour scheme and legend entry
                    // soils will be done after HRU creation, since only uses occurring soils
                    await FileTypes.colourLanduses(this.landuseLayer, this._gv);
                    if (!this._gv.isBatch) {
                        Debug.Assert(this.landuseLayer is not null);
                        if (this._db.slopeLimits.Count > 0) {
                            var slopeBandsLayer = (await Utils.getLayerByFilename(this._gv.slopeBandsFile, FileTypes._SLOPEBANDS, this._gv, null, Utils._SLOPE_GROUP_NAME)).Item1 as RasterLayer;
                            if (slopeBandsLayer is not null) {
                                await QueuedTask.Run(() => slopeBandsLayer.SetVisibility(false));
                            }
                        //        var slopeBandsTreeLayer = root.findLayer(slopeBandsLayer.GetFID());
                        //        Debug.Assert(slopeBandsTreeLayer is not null);
                        //        treeModel.refreshLayerLegend(slopeBandsTreeLayer);
                        //    }
                        }
                    }
                    if (OK) {
                        if (!this._gv.useGridModel) {
                            if (this._dlg.channelMergeSliderValue > 0) {
                                this._dlg.setChannelMergeChoice();
                                this.CreateHRUs.mergeChannels();
                            }
                        }
                        if (this._gv.isBatch) {
                            Utils.information("Writing landuse and soil report ...", true);
                        }
                        await this.CreateHRUs.printBasins(false);
                    }
                    this._dlg.getProgressBar.Visible = false;
                }
                if (OK) {
                    this._dlg.readFromPreviousEnabled = true;
                    this._dlg.subbasinsLabelText = string.Format("Subbasins count: {0}", this.CreateHRUs.basins.Count);
                    this._dlg.channelsLabelText = string.Format("Channels count: {0}", this.CreateHRUs.countChannels());
                    this._dlg.fullHRUsLabelText = string.Format("Full HRUs count: {0}", this.CreateHRUs.countHRUs());
                    if (!this._gv.useGridModel) {
                        this._dlg.remergeEnabled = true;
                        this._dlg.remergeChecked = true;
                    }
                    this._dlg.hruChoiceGroupEnabled = true;
                    this._dlg.areaPercentChoiceGroupEnabled = true;
                    this._dlg.splitButtonEnabled = true;
                    this._dlg.exemptButtonEnabled = true;
                    this._dlg.tab2.Enabled = true;
                    this._dlg.setHRUChoice();
                    this._dlg.saveProj();
                }
            }
            return OK;
        }

        // Return true if there is a visible layer with a name starting 'Floodplain'.
        public virtual bool hasVisibleFloodplainLayer() {
            var layer = Utils.getLayerByLegend("Floodplain");
            return layer is not null && layer.IsVisible;
        }

        // Set up landuse lookup tables.
        public virtual bool initLanduses(string table) {
            this._db.landuseVals = new HashSet<int>();
            if (table == "") {
                this.landuseTable = this._dlg.selectLanduseTableText;
                if (!this._db.landuseTableNames.Contains(this.landuseTable)) {
                    Utils.error("Please select a landuse table", this._gv.isBatch);
                    return false;
                }
            } else {
                // doing tryRun and table already read from project file
                this.landuseTable = table;
            }
            return this._db.populateLanduseCodes(this.landuseTable, this._gv.useGridModel);
        }

        // Set up soil lookup tables.
        public virtual bool initSoils(string table) {
            if (!File.Exists(this._db.plantSoilDatabase)) {
                Utils.information(string.Format("Warning: landuse and soil database {0} does not seeme to exist", this._db.plantSoilDatabase), this._gv.isBatch);
            } else if (!this._db.useSTATSGO && !this._db.useSSURGO) {
                if (!this._db.hasTable(this._db.plantSoilDatabase, this._db.usersoilTable)) {
                    Utils.information(string.Format("Warning: table {0} not found in landuse and soil database {1}", this._db.usersoilTable, this._db.plantSoilDatabase), this._gv.isBatch);
                }
            }
            if (this._db.useSSURGO) {
                // no lookup table needed
                this._db.usedSSURGOSoils = new HashSet<int>();
                return true;
            } else {
                this._db.usedSoilNames = new Dictionary<int, string>();
            }
            if (table == "") {
                this.soilTable = this._dlg.selectSoilTableText;
                if (!this._db.soilTableNames.Contains(this.soilTable)) {
                    Utils.error("Please select a soil table", this._gv.isBatch);
                    return false;
                }
            } else {
                // doing tryRun and table already read from project file
                this.soilTable = table;
            }
            return this._db.populateSoilNames(this.soilTable, this._gv.useGridModel);
        }

        // Create HRUs.
        public async Task calcHRUs() {
            this._gv.writeProjectConfig(-1, 0);
            var time1 = DateTime.Now;
            try {
                using (new CursorWait()) {
                    this._dlg.slopeSliderEnabled = false;
                    this._dlg.slopeValEnabled = false;
                    this._dlg.areaGroupEnabled = false;
                    this._dlg.targetGroupEnabled = false;
                    this._dlg.landuseSoilSlopeGroupEnabled = false;
                    this.CreateHRUs.HRUNum = 0;
                    this.CreateHRUs.isDominantHRU = this._dlg.dominantHRUButtonChecked;
                    this.CreateHRUs.isMultiple = !this._dlg.dominantHRUButtonChecked && !this._dlg.dominantLanduseButtonChecked;
                    this.CreateHRUs.useArea = this._dlg.areaButtonChecked;
                    this.CreateHRUs.reservoirThreshold = this._dlg.reservoirThresholdValue;
                    this.CreateHRUs.addReservoirs();
                    if (!this._gv.saveExemptSplit()) {
                        return;
                    }
                    if (this.CreateHRUs.isMultiple) {
                        if (this.CreateHRUs.isArea) {
                            this.CreateHRUs.removeSmallHRUsByArea();
                        } else if (this.CreateHRUs.isTarget) {
                            this.CreateHRUs.removeSmallHRUsbyTarget();
                        } else {
                            if (this._db.slopeLimits.Count == 0) {
                                this.CreateHRUs.slopeVal = 0;
                            }
                            // allow too tight thresholds, since we guard against removing all HRUs from a subbasin
                            // if not self.CreateHRUs.cropSoilAndSlopeThresholdsAreOK():
                            //     Utils.error('Internal error: problem with tight thresholds', self._gv.isBatch)
                            //     return
                            if (this.CreateHRUs.useArea) {
                                this.CreateHRUs.removeSmallHRUsByThresholdArea();
                            } else {
                                this.CreateHRUs.removeSmallHRUsByThresholdPercent();
                            }
                        }
                        if (!this.CreateHRUs.splitHRUs()) {
                            return;
                        }
                    } else if (this.CreateHRUs.isDominantHRU) {
                        this.CreateHRUs.selectDominantHRUs();
                    } else {
                        this.CreateHRUs.selectDominantLanduseSoilSlope();
                    }
                    this.CreateHRUs.saveAreas(false);
                    //var fullHRUsLayer = (await Utils.getLayerByFilename(this._gv.fullHRUsFile, FileTypes._HRUS, null, null, null)).Item1 as FeatureLayer;
                    if (this._gv.isBatch) {
                        Utils.information("Writing HRUs report ...", true);
                    }
                    await this.CreateHRUs.writeWaterBodiesTable();
                    await this.CreateHRUs.printBasins(true);
                    if (!await this.CreateHRUs.mergeLSUs()) {
                        Utils.error(string.Format("Failed to create actual LSUs shapefile {0}", this._gv.actLSUsFile), this._gv.isBatch);
                    }
                    if (await this.CreateHRUs.writeSubbasinsAndLandscapeTables()) {
                        // can now provide soil layer legend
                        var soilLayer = (await Utils.getLayerByFilename(this._gv.soilFile, FileTypes._SOILS, null, null, null)).Item1 as RasterLayer;
                        if (soilLayer is not null) {
                            await FileTypes.colourSoils(soilLayer, this._gv);
                            //var treeModel = QgsLayerTreeModel(root);
                            //var soilTreeLayer = root.findLayer(soilLayer.GetFID());
                            //Debug.Assert(soilTreeLayer is not null);
                            //treeModel.refreshLayerLegend(soilTreeLayer);
                        }
                        if (this._db.writeSoilsTable() && this._db.writeLanduseTables()) {
                            this.completed = true;
                            this._gv.writeProjectConfig(-1, 1);
                            this._dlg.readFromPreviousEnabled = true;
                            var msg = string.Format("HRUs done: {0} HRUs formed with {1} channels in {2} subbasins.", this.CreateHRUs.countHRUs(), this.CreateHRUs.countChannels(), this.CreateHRUs.basins.Count);
                            Utils.loginfo(msg);
                            if (this._gv.isBatch) {
                                Console.WriteLine(msg);
                            }
                        }
                    }
                    this._gv.db.checkRoutingTable();
                }
            }
            catch (Exception ex) {
                Utils.exceptionError("Failed to create HRUs: " + ex.Message, this._gv.isBatch);
            }
            finally {
                this._dlg.saveProj();
                var time2 = DateTime.Now;
                Utils.loginfo(string.Format("Calculating HRUs took {0} seconds", Convert.ToInt32(time2.Subtract(time1).TotalSeconds)));
                if (this._dlg.gwflowButtonChecked) {
                    // TODO
                    //var gwf = GWFlow(this._gv, this._dlg.progress);
                    //gwf.run();
                    this._gv.writeProjectConfig(-1, -1, use_gwflow: 1);
                } else {
                    this._gv.writeProjectConfig(-1, -1, use_gwflow: 0);
                }
                if (this.completed) {
                    this._dlg.Close();
                    this._parent.postCreateHRUs(1);
                }
            }
        }

        public void cancel() {
            this._dlg.Close();
            this._parent.postCreateHRUs(0);
        }

        // Run the exempt dialog.
        public virtual void doExempt() {
            var dlg = new ExemptForm(this._gv);
            dlg.run();
        }

        // Run the split dialog.
        public virtual void doSplit() {
            var dlg = new SplitForm(this._gv);
            dlg.run();
        }

        // Run the elevation bands dialog.
        public virtual void doElevBands() {
            var dlg = new ElevationBandsForm(this._gv);
            dlg.run();
        }
    }
}

