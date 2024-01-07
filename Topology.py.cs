
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Mapping;

using System;
using System.Diagnostics;
using System.IO;

using System.Linq;

using System.Collections;
using System.Collections.Generic;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System.Threading.Tasks;
using OSGeo.OGR;
using OSGeo.GDAL;
using System.Printing.IndexedProperties;
using ArcGIS.Core.Data.Raster;
using ArcGIS.Core.Internal.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using System.Windows.Forms;
using ArcGIS.Desktop.Internal.Mapping.Locate;
using ArcGIS.Desktop.Core.Geoprocessing;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;

namespace ArcSWATPlus {


    public class Topology {

        public Dictionary<int, double[]> basinCentroids;

        public int channelIndex;

        public Dictionary<int, double> channelLengths;

        public Dictionary<int, ReachData> channelsData;

        public Dictionary<int, double> channelSlopes;

        public Dictionary<int, int> channelToSWATChannel;

        public Dictionary<int, double> chBasinAreas;

        public Dictionary<int, int> chBasinToChLink;

        public Dictionary<int, int> chBasinToSubbasin;

        public Dictionary<int, int> chLinkFromLake;

        public Dictionary<int, int> chLinkInsideLake;

        public Dictionary<int, int> chLinkIntoLake;

        public Dictionary<int, int> chLinkToChBasin;

        public Dictionary<int, (int, double[])> chLinkToInlet;

        public Dictionary<int, (int, double[])> chLinkToPtSrc;

        public Dictionary<int, (int, double[], WaterRole)> chLinkToWater;

        public Dictionary<int, (int, double[])> chOutlets;

        public Dictionary<int, (int, double[])> chPointSources;

        public int chunkCount;

        public ArcGIS.Core.Geometry.SpatialReference crsLatLong;

        public ArcGIS.Core.Geometry.SpatialReference crsProject;

        public DBUtils db;

        public ArcGIS.Core.Geometry.Envelope demExtent;

        public double demNodata;

        public Dictionary<int, Dictionary<int, double>> distancesToJoins;

        public Dictionary<int, int> downChannels;

        public Dictionary<int, int> downStreams;

        public Dictionary<int, int> downSubbasins;

        public Dictionary<int, double> drainAreas;

        public int dsChannelIndex;

        public double dx;

        public double dy;

        public Dictionary<int, (int, int, double[])> foundReservoirs;

        public double horizontalFactor;

        public Dictionary<int, (int, double[])> inlets;

        public bool isBatch;

        public bool isHAWQS;

        public bool isHUC;

        public Dictionary<int, HashSet<int>> lakeInlets;

        public Dictionary<int, HashSet<int>> lakeOutlets;

        public Dictionary<int, LakeData> lakesData;

        public HashSet<int> lostDsNodeIds;

        public Dictionary<int, double> maxFlowLengths;

        public Dictionary<int, double[]> nearsources;

        public Dictionary<int, double[]> nearoutlets;

        public Dictionary<int, double> origChBasinAreas;

        public bool outletAtStart;

        public Dictionary<int, (int, double[], HashSet<int>)> outlets;

        public Dictionary<int, int> outletsInLake;

        public int pointId;

        public Dictionary<int, int> strahler;

        public Dictionary<int, double> streamLengths;

        public Dictionary<int, int> subbasinToStream;

        public Dictionary<int, int> subbasinToSWATBasin;

        public Dictionary<int, int> SWATBasinToSubbasin;

        public Dictionary<int, int> SWATChannelToChannel;

        public object transformToLatLong;

        public HashSet<int> upstreamFromInlets;

        public double verticalFactor;

        public int waterBodyId;

        public int wsnoIndex;

        public double xThreshold;

        public double yThreshold;

        public HashSet<int> zeroChannels;

        public static int _NOBASIN = -2;

        public static string _TOTAL = "tot";

        public static string _RECHARGE = "rhg";

        public static string _SURFACE = "sur";

        public static string _LATERAL = "lat";

        public static string _TILE = "til";

        public static string _LINKNO = "LINKNO";

        public static string _DSLINKNO = "DSLINKNO";

        public static string _DSLINKNO1 = "DSLINKNO1";

        public static string _DSLINKNO2 = "DSLINKNO2";

        public static string _USLINKNO1 = "USLINKNO1";

        public static string _USLINKNO2 = "USLINKNO2";

        public static string _DSNODEID = "DSNODEID";

        public static string _DSNODEID1 = "DSNODEID1";

        public static string _DSNODEID2 = "DSNODEID2";

        public static string _DRAINAREA = "DSContArea";

        public static string _DRAINAREA2 = "DS_Cont_Ar";

        public static string _DRAINAGE = "Drainage";

        public static string _ORDER = "strmOrder";

        public static string _ORDER2 = "Order";

        public static string _ORDERHUC = "streamorde";

        public static string _LENGTH = "Length";

        public static string _MAGNITUDE = "Magnitude";

        public static string _DROP = "strmDrop";

        public static string _DROP2 = "Drop";

        public static string _SLOPE = "Slope";

        public static string _STRAIGHTL = "StraightL";

        public static string _STRAIGHTL2 = "Straight_L";

        public static string _USCONTAR = "USContArea";

        public static string _USCONTAR2 = "US_Cont_Ar";

        public static string _WSNO = "WSNO";

        public static string _DOUTEND = "DOUTEND";

        public static string _DOUTEND2 = "DOUT_END";

        public static string _DOUTSTART = "DOUTSTART";

        public static string _DOUTSTART2 = "DOUT_START";

        public static string _DOUTMID = "DOUTMID";

        public static string _DOUTMID2 = "DOUT_MID";

        public static string _BASINNO = "BasinNo";

        public static string _ID = "ID";

        public static string _INLET = "INLET";

        public static string _RES = "RES";

        public static string _PTSOURCE = "PTSOURCE";

        public static string _ADDED = "ADDED";

        public static string _POLYGONID = "PolygonId";

        public static string _DOWNID = "DownId";

        public static string _AREA = "Area";

        public static string _STREAMLINK = "StreamLink";

        public static string _STREAMLEN = "StreamLen";

        public static string _DSNODEIDW = "DSNodeID";

        public static string _DSWSID = "DSWSID";

        public static string _US1WSID = "US1WSID";

        public static string _US2WSID = "US2WSID";

        public static string _SUBBASIN = "Subbasin";

        public static string _CHANNEL = "Channel";

        public static string _CHANNELR = "ChannelR";

        public static string _LANDSCAPE = "Landscape";

        public static string _AQUIFER = "Aquifer";

        public static string _LSU = "LSU";

        public static string _LSUID = "LSUID";

        public static string _PENWIDTH = "PenWidth";

        public static string _HRUS = "HRUS";

        public static string _HRUGIS = "HRUGIS";

        public static string _LAKEID = "LakeId";

        public static string _RESERVOIR = "Reservoir";

        public static string _POND = "Pond";

        public static string _AREAC = "AreaC";

        public static string _LEN2 = "Len2";

        public static string _SLO2 = "Slo2";

        public static string _WID2 = "Wid2";

        public static string _DEP2 = "Dep2";

        public static string _MINEL = "MinEl";

        public static string _MAXEL = "MaxEl";

        public static string _LAKEIN = "LakeIn";

        public static string _LAKEOUT = "LakeOut";

        public static string _LAKEWITHIN = "LakeWithin";

        public static string _LAKEMAIN = "LakeMain";

        public static string _POINTID = "PointId";

        public static string _SOURCEX = "SourceX";

        public static string _SOURCEY = "SourceY";

        public static string _OUTLETX = "OutletX";

        public static string _OUTLETY = "OutletY";

        public static string _TOTDASQKM = "TotDASqKm";

        public static int _HUCPointId = 100000;

        public Topology(bool isBatch, bool isHUC, bool isHAWQS) {
            //# Link to project database
            this.db = null;
            //# True if outlet end of reach is its first point, i.e. index zero."""
            this.outletAtStart = true;
            //# index to LINKNO in channel shapefile
            this.channelIndex = -1;
            //# index to DSLINKNO in channel shapefile
            this.dsChannelIndex = -1;
            //# index to WSNO in channel shapefile (value commonly called channel basin)
            this.wsnoIndex = -1;
            //# relation between channel basins and subbasins
            // not used with grid models (since would be 1-1)
            this.chBasinToSubbasin = new Dictionary<int, int>();
            //# WSNO does not obey SWAT rules for element numbers (> 0) 
            // so we invent and store SWATBasin numbers
            // also SWAT basins may not be empty
            // not complete: zero areas/stream lengths excluded
            this.subbasinToSWATBasin = new Dictionary<int, int>();
            //#inverse map to make it easy to output in numeric order of SWAT Basins
            this.SWATBasinToSubbasin = new Dictionary<int, int>();
            //# original channel links may have zero numbers and may have zero lengths
            //# so we generate SWATChannel ids
            // not complete
            this.channelToSWATChannel = new Dictionary<int, int>();
            //# inverse map
            this.SWATChannelToChannel = new Dictionary<int, int>();
            //# subbasin to stream mapping (wsno to link fields in streams shapefile)
            // complete
            this.subbasinToStream = new Dictionary<int, int>();
            //# stream to downstream (link to dslink in streams shapefile)
            // complete
            this.downStreams = new Dictionary<int, int>();
            //# stream lengths (link to length in streams shapefile).  Lengths are in metres
            // complete
            this.streamLengths = new Dictionary<int, double>();
            //# LINKNO to DSLINKNO in channel shapefile
            // complete = all channel links defined
            this.downChannels = new Dictionary<int, int>();
            //# zero length channels
            this.zeroChannels = new HashSet<int>();
            //# subbasin to downstream subbasin
            // incomplete: no empty basins (zero length streams or missing wsno value in subbasins layer)
            this.downSubbasins = new Dictionary<int, int>();
            //# map from channel to chBasin
            // incomplete - no zero length channels
            this.chLinkToChBasin = new Dictionary<int, int>();
            //# reverse of chLinkToChBasin
            this.chBasinToChLink = new Dictionary<int, int>();
            //# centroids of basins as (x, y) pairs in projected units
            this.basinCentroids = new Dictionary<int, double[]>();
            //# channel link to channel length in metres:
            // complete
            this.channelLengths = new Dictionary<int, double>();
            //# channel slopes in m/m
            // complete
            this.channelSlopes = new Dictionary<int, double>();
            //# array of total area draining to downstream end of channel in square metres
            this.drainAreas = new Dictionary<int, double>(); 
            //# array of Strahler order of channels
            this.strahler = null;
            //# map of lake id to ids of points added to split channels entering lakes
            this.lakeInlets = new Dictionary<int, HashSet<int>>();
            //# map of lake id to ids of points added to split channels leaving lakes
            this.lakeOutlets = new Dictionary<int, HashSet<int>>();
            //# map of channel to ReachData: points and elevations at ends of channels, plus basin
            // not complete: zero areas/channel lengths excluded
            this.channelsData = new Dictionary<int, ReachData>();
            //# map of lake id to LakeData for lakes defined by shapefile
            this.lakesData = new Dictionary<int, LakeData>();
            //# map of channel links to lake ids: channel flowing into lake
            this.chLinkIntoLake = new Dictionary<int, int>();
            //# map of channel links to lake ids: channel completely inside lake
            this.chLinkInsideLake = new Dictionary<int, int>();
            //# map of channel links to lake ids: channel flowing out of lake
            this.chLinkFromLake = new Dictionary<int, int>();
            //# map of subbasin to lake id for subbasins with their outlet inside a lake (non-grid models only)
            this.outletsInLake = new Dictionary<int, int>();
            //# channel basin to area in square metres.  Not used with grid model.
            this.chBasinAreas = new Dictionary<int, double>();
            // original of chBasinAreas for use after chBasinAreas affected by merging, in case merging repeated
            this.origChBasinAreas = new Dictionary<int, double>();
            //# current point id (for outlets, inlets and point sources)
            this.pointId = 0;
            //# current water body id (for lakes, reservoirs and ponds)
            this.waterBodyId = 0;
            //# channel links to reservoir or pond point ids plus water type: reservoir or pond discharges into channel
            this.chLinkToWater = new Dictionary<int, (int, double[], WaterRole)>();
            //# channel links with point sources flowing into them (defined by inlets/outlets file)
            this.chLinkToPtSrc = new Dictionary<int, (int, double[])>();
            //# channel links to watershed inlets (for grid models only)
            this.chLinkToInlet = new Dictionary<int, (int, double[])>();
            //# basins draining to inlets
            this.upstreamFromInlets = new HashSet<int>();
            //# width of DEM cell in metres
            this.dx = 0;
            //# depth of DEM cell in metres
            this.dy = 0;
            //# x direction threshold for points to be considered coincident in DEM units
            this.xThreshold = 0;
            //# y direction threshold for points to be considered coincident in DEM units
            this.yThreshold = 0;
            //# multiplier to turn DEM units to metres
            this.horizontalFactor = 1;
            //# multiplier to turn DEM elevations to metres
            this.verticalFactor = 1;
            //# DEM nodata value
            this.demNodata = 0;
            //# DEM extent
            this.demExtent = null;
            //# map from subbasin to outlet pointId, point, and channel(s) draining to it
            this.outlets = new Dictionary<int, (int, double[], HashSet<int>)>();
            //# map from basin to near outlet point (used for placing extra reservoirs)
            this.nearoutlets = new Dictionary<int, double[]>();
            //# map from basin to near source point (use for placing extra point sources)
            this.nearsources = new Dictionary<int, double[]>();
            //# map from subbasin to inlet pointId and point (not used with grid models)
            this.inlets = new Dictionary<int, (int, double[])>();
            //# map from channel links to outlet end
            this.chOutlets = new Dictionary<int, (int, double[])>();
            //# map from channel links to point sources
            this.chPointSources = new Dictionary<int, (int, double[])>();
            //# reservoirs found by converting water HRUs
            this.foundReservoirs = new Dictionary<int, (int, int, double[])>();
            //# lat-long coordinate reference system
            this.crsLatLong = SpatialReferences.WGS84;
            //# transform from project coordinates to lat-long
            this.transformToLatLong = null;
            //# Flag to show if batch run
            this.isBatch = isBatch;
            //# flag for HUC projects
            this.isHUC = isHUC;
            //# flag for HAWQS projects
            this.isHAWQS = isHAWQS;
            //# table for memorizing distances from basin to join in flowpath with other basin:
            // basin -> otherBasin -> join distance in metres
            this.distancesToJoins = new Dictionary<int, Dictionary<int, double>>();
            //# table for use in existing non-grid models of maximum channel flow lengths in metres to subbasin outlets
            // complete
            this.maxFlowLengths = new Dictionary<int, double>();
            //# number of chunks to use for rasters and their arrays; increased when memory fails
            this.chunkCount = 1;
            //# dsNodeIds that cannot be retained when making grids as they would be in same grid cell as another point
            this.lostDsNodeIds = new HashSet<int>();
        }

        // Set DEM size parameters and stream orientation, and store source and outlet points for stream reaches.
        public async Task<bool> setUp0(
            RasterLayer demLayer,
            string channelFile,
            string outletFile,
            ArcGIS.Core.Data.Raster.Raster ad8Raster,
            GlobalVars gv) {
            ArcGIS.Core.Geometry.Unit units = null;
            // can fail if demLayer is None or not projected
            try {
                await this.setCrs(demLayer, gv);
                Debug.Assert(gv.crsProject is not null);
                units = gv.crsProject.Unit;
            } catch (Exception ex) {
                Utils.loginfo(string.Format("Failure to read DEM units: {0}", ex.Message));
                return false;
            }
            if (units.Name == "Meter") {
                gv.horizontalFactor = 1;
            } else if (units.Name == "Foot") {
                gv.horizontalFactor = Parameters._FEETTOMETRES;
            } else {
                // unknown or degrees - will be reported in delineation - just quietly fail here
                Utils.loginfo(string.Format("Failure to read DEM units: {0}", units.ToString()));
                return false;
            }
            Tuple<double, double> XYSizes = null;
            await QueuedTask.Run(() => {
                XYSizes = demLayer.GetRaster().GetMeanCellSize();
                dynamic nodata = demLayer.GetRaster().GetNoDataValue();
                var typ = nodata.GetType();
                if (typ.IsArray) {
                    this.demNodata = Convert.ToDouble(nodata[0]);
                } else {
                    this.demNodata = Convert.ToDouble(nodata);
                }
            });
            this.dx = XYSizes.Item1 * gv.horizontalFactor;
            this.dy = XYSizes.Item1 * gv.horizontalFactor;
            this.xThreshold = this.dx / gv.horizontalFactor * Parameters._NEARNESSTHRESHOLD;
            this.yThreshold = this.dy / gv.horizontalFactor * Parameters._NEARNESSTHRESHOLD;
            Utils.loginfo(string.Format("Factor is {0}, cell width is {1}, cell depth is {2}", gv.horizontalFactor, this.dx, this.dy));
            this.demExtent = await QueuedTask.Run(() => demLayer.QueryExtent());
            this.horizontalFactor = gv.horizontalFactor;
            this.verticalFactor = gv.verticalFactor;
            using (var channelDs = Ogr.Open(channelFile, 0)) {
                var channelLayer = channelDs.GetLayerByIndex(0);
                this.outletAtStart = this.hasOutletAtStart(channelLayer, ad8Raster, channelFile);
                Utils.loginfo(string.Format("Outlet at start is {0}", this.outletAtStart));
                this.outletsInLake.Clear();
                if (string.IsNullOrEmpty(outletFile)) {
                    return this.saveOutletsAndSources(channelLayer, null, gv.useGridModel);
                } else {
                    using (var outletDs = Ogr.Open(outletFile, 1)) {
                        var outletLayer = outletDs.GetLayerByIndex(0);
                        return this.saveOutletsAndSources(channelLayer, outletLayer, gv.useGridModel);
                    }
                }
            }
        }

        // Set crsProject and transformToLatLong if necessary.
        public async Task setCrs(RasterLayer demLayer, GlobalVars gv) {
            if (gv.crsProject is null) {
                await QueuedTask.Run(() => { gv.crsProject = demLayer.GetSpatialReference(); });
            }
            this.crsProject = gv.crsProject;
            this.transformToLatLong = ProjectionTransformation.Create(gv.crsProject, this.crsLatLong, this.demExtent);
        }

        // Establish subbasinToStream, downStreams and streamLengths dictionaries.
        //         
        //         Used when calculating ridges by branch length method and setUp has not been run yet.
        public virtual bool setUp1(string streamFile) {
            this.subbasinToStream.Clear();
            this.downStreams.Clear();
            this.streamLengths.Clear();
            using (var streamDs = Ogr.Open(streamFile, 0)) {
                var streamLayer = streamDs.GetLayerByIndex(0);
                var streamIndex = this.getIndex(streamLayer, Topology._LINKNO);
                if (streamIndex < 0) {
                    Utils.loginfo("No LINKNO field in stream layer");
                    return false;
                }
                var dsStreamIndex = this.getIndex(streamLayer, Topology._DSLINKNO);
                if (dsStreamIndex < 0) {
                    Utils.loginfo("No DSLINKNO field in stream layer");
                    return false;
                }
                var lengthIndex = this.getIndex(streamLayer, Topology._LENGTH, ignoreMissing: true);
                var wsnoIndex = this.getIndex(streamLayer, Topology._WSNO);
                if (wsnoIndex < 0) {
                    Utils.loginfo("No WSNO field in stream layer");
                    return false;
                }
                streamLayer.ResetReading();
                OSGeo.OGR.Feature reach;
                do {
                    reach = streamLayer.GetNextFeature();
                    if (reach is null) {
                        break;
                    }
                    var link = reach.GetFieldAsInteger(streamIndex);
                    var dsLink = reach.GetFieldAsInteger(dsStreamIndex);
                    var basin = reach.GetFieldAsInteger(wsnoIndex);
                    double length;
                    if (lengthIndex < 0) {
                        length = reach.GetGeometryRef().Length() * this.horizontalFactor;
                    } else {
                        length = reach.GetFieldAsDouble(lengthIndex) * this.horizontalFactor;
                    }
                    this.subbasinToStream[basin] = link;
                    this.downStreams[link] = dsLink;
                    this.streamLengths[link] = length;
                } while (reach != null);
            }
            return true;
        }

        // Create topological data from layers.
        public async Task<bool> setUp(
            RasterLayer demLayer,
            string channelFile,
            string subbasinsFile,
            string outletFile,
            string lakesFile,
            GlobalVars gv,
            bool existing,
            bool recalculate,
            bool useGridModel,
            bool streamDrainage,
            bool reportErrors) {
            int subbasin;
            bool ok;
            double[] pt;
            string typ;
            bool isInlet;
            bool isPtSource;
            int dsNode;
            double drop;
            ReachData data;
            double length;
            int chBasin;
            int chLink;
            //Utils.loginfo('Channel layer {0}', channelLayer.dataProvider().dataSourceUri()))
            //Utils.loginfo('Subbasins layer {0}', subbasinsLayer.dataProvider().dataSourceUri()))
            this.db = gv.db;
            this.chLinkToChBasin.Clear();
            this.chBasinToChLink.Clear();
            this.subbasinToSWATBasin.Clear();
            this.SWATBasinToSubbasin.Clear();
            this.channelToSWATChannel.Clear();
            this.SWATChannelToChannel.Clear();
            this.downChannels.Clear();
            this.zeroChannels.Clear();
            // do not clear centroids unless existing and not using grid model: 
            if (existing && !useGridModel) {
                this.basinCentroids.Clear();
            }
            this.channelLengths.Clear();
            this.channelSlopes.Clear();
            this.channelsData.Clear();
            this.chLinkToWater.Clear();
            this.chLinkToPtSrc.Clear();
            this.chLinkToInlet.Clear();
            this.distancesToJoins.Clear();
            this.maxFlowLengths.Clear();
            var dsNodeToLink = new Dictionary<int, int>();
            var ignoreError = !reportErrors;
            var ignoreWithExisting = existing || !reportErrors;
            var ignoreWithGrid = useGridModel || !reportErrors;
            var ignoreWithGridOrExisting = ignoreWithGrid || ignoreWithExisting;
            var ignoreTotDA = !(this.isHUC || this.isHAWQS);
            using (var channelDs = Ogr.Open(channelFile, 0))
            using (var subbasinsDs = Ogr.Open(subbasinsFile, 1)) {
                var channelLayer = channelDs.GetLayerByIndex(0);
                var channelDefn = channelLayer.GetLayerDefn();
                var subbasinsLayer = subbasinsDs.GetLayerByIndex(0);
                OSGeo.OGR.Layer outletLayer = null;
                if (File.Exists(outletFile)) {
                    var outletDs = Ogr.Open(outletFile, 0);
                    outletLayer = outletDs.GetLayerByIndex(0);
                }
                OSGeo.OGR.Layer lakesLayer = null;
                if (File.Exists(lakesFile)) {
                    var lakesDs = Ogr.Open(lakesFile, 0);
                    lakesLayer = lakesDs.GetLayerByIndex(0);
                }
                this.channelIndex = this.getIndex(channelDefn, Topology._LINKNO, channelFile, ignoreMissing: ignoreError);
                if (this.channelIndex < 0) {
                    Utils.loginfo("No LINKNO field in channels layer");
                    return false;
                }
                var DSLINKNO = this.isHUC ? Topology._DSLINKNO1 : Topology._DSLINKNO;
                this.dsChannelIndex = this.getIndex(channelDefn, DSLINKNO, channelFile, ignoreMissing: ignoreError);
                if (this.dsChannelIndex < 0) {
                    Utils.loginfo("No DSLINKNO field in channels layer");
                    return false;
                }
                var DSNODEID = this.isHUC ? Topology._DSNODEID1 : Topology._DSNODEID;
                var dsNodeIndex = this.getIndex(channelDefn, DSNODEID, channelFile, ignoreMissing: ignoreWithExisting);
                var dsNode2Index = this.getIndex(channelDefn, Topology._DSNODEID2, channelFile, ignoreMissing: !this.isHUC);
                this.wsnoIndex = this.getIndex(channelDefn, Topology._WSNO, channelFile, ignoreMissing: ignoreError);
                if (this.wsnoIndex < 0) {
                    Utils.loginfo("No WSNO field in channels layer");
                    return false;
                }
                int orderIndex;
                if (this.isHUC) {
                    orderIndex = this.getIndex(channelDefn, Topology._ORDERHUC, channelFile, ignoreMissing: false);
                } else {
                    orderIndex = this.getIndex(channelDefn, Topology._ORDER, channelFile, ignoreMissing: ignoreWithGridOrExisting);
                    if (orderIndex < 0 && ignoreWithGridOrExisting) {
                        orderIndex = this.getIndex(channelDefn, Topology._ORDER2, channelFile, ignoreMissing: ignoreWithGridOrExisting);
                    }
                }
                var drainAreaIndex = this.getIndex(channelDefn, Topology._DRAINAREA, channelFile, ignoreMissing: ignoreWithGridOrExisting);
                if (drainAreaIndex < 0 && ignoreWithGridOrExisting) {
                    drainAreaIndex = this.getIndex(channelDefn, Topology._DRAINAREA2, channelFile, ignoreMissing: ignoreWithGridOrExisting);
                }
                var lengthIndex = this.getIndex(channelDefn, Topology._LENGTH, channelFile, ignoreMissing: ignoreWithGridOrExisting);
                var dropIndex = this.getIndex(channelDefn, Topology._DROP, channelFile, ignoreMissing: ignoreWithGridOrExisting);
                if (dropIndex < 0 && ignoreWithGridOrExisting) {
                    dropIndex = this.getIndex(channelDefn, Topology._DROP2, channelFile, ignoreMissing: ignoreWithGridOrExisting);
                }
                var totDAIndex = this.getIndex(channelDefn, Topology._TOTDASQKM, channelFile, ignoreMissing: ignoreTotDA);
                var polyIndex = this.getIndex(subbasinsLayer, Topology._POLYGONID, ignoreMissing: ignoreError);
                if (polyIndex < 0) {
                    Utils.loginfo("No POLYGONID field in subbasins layer");
                    return false;
                }
                int idIndex = -1;
                int inletIndex = -1;
                int ptSourceIndex = -1;
                int resIndex = -1;
                int ptIdIndex = -1;
                int addIndex = -1;
                var subbasinIndex = this.getIndex(subbasinsLayer, Topology._SUBBASIN, ignoreMissing: ignoreWithGridOrExisting);
                if (outletLayer is not null) {
                    if (dsNodeIndex < 0) {
                        Utils.information(string.Format("Warning: streams layer has no {0} field, so points in inlets/outlets file will be ignored", Topology._DSNODEID), gv.isBatch);
                    }
                    idIndex = this.getIndex(outletLayer, Topology._ID, ignoreMissing: ignoreError);
                    if (idIndex < 0) {
                        Utils.loginfo("No ID field in outlets layer");
                        return false;
                    }
                    inletIndex = this.getIndex(outletLayer, Topology._INLET, ignoreMissing: ignoreError);
                    if (inletIndex < 0) {
                        Utils.loginfo("No INLET field in outlets layer");
                        return false;
                    }
                    ptSourceIndex = this.getIndex(outletLayer, Topology._PTSOURCE, ignoreMissing: ignoreError);
                    if (ptSourceIndex < 0) {
                        Utils.loginfo("No PTSOURCE field in outlets layer");
                        return false;
                    }
                    resIndex = this.getIndex(outletLayer, Topology._RES, ignoreMissing: ignoreError);
                    if (resIndex < 0) {
                        Utils.loginfo("No RES field in outlets layer");
                        return false;
                    }
                    ptIdIndex = this.getIndex(outletLayer, Topology._POINTID, ignoreMissing: ignoreError);
                    if (ptIdIndex < 0) {
                        Utils.loginfo("No PointId field in outlets layer");
                        return false;
                    }
                    // ADDED field only used with lakes: may be missing
                    addIndex = this.getIndex(outletLayer, Topology._ADDED, ignoreMissing: true);
                }
                // upstream links
                var us = new Dictionary<int, List<int>>();
                var time1 = DateTime.Now;
                var maxChLink = 0;
                var SWATChannel = 0;
                // drainAreas is a mapping from link number (used as index to array) of grid cell areas in sq m
                if (this.isHUC || this.isHAWQS) {
                    // make it a dictionary rather than a numpy array because there is a big gap
                    // between most basin numbers and the nunbers for inlets (100000 +)
                    // and we need to do no calculation
                    // create it here for HUC and HAWQS models as we set it up from totDASqKm field in channelLayer
                    this.drainAreas = new Dictionary<int, double>();
                }
                channelLayer.ResetReading();
                OSGeo.OGR.Feature channel;
                do {
                    channel = channelLayer.GetNextFeature();
                    if (channel is null) {
                        break;
                    }
                    chLink = channel.GetFieldAsInteger(this.channelIndex);
                    var dsChLink = channel.GetFieldAsInteger(this.dsChannelIndex);
                    chBasin = channel.GetFieldAsInteger(this.wsnoIndex);
                    var geom = channel.GetGeometryRef();
                    if (lengthIndex < 0 || recalculate) {
                        length = geom.Length() * gv.horizontalFactor;
                    } else {
                        length = channel.GetFieldAsDouble(lengthIndex) * gv.horizontalFactor;
                    }
                    if ((this.isHUC || this.isHAWQS) && length == 0) {
                        // allow for zero length inlet channels
                        data = null;
                    } else {
                        data = await this.getOgrReachData(channel, demLayer);
                        this.channelsData[chLink] = data;
                    }
                    // don'use TauDEM drops: affected by burn-in
                    // if data and (dropIndex < 0 or recalculate):
                    //     drop = data.upperZ - data.lowerZ
                    // elif dropIndex >= 0:
                    //     drop = channel[dropIndex]
                    // else:
                    //     drop = 0
                    if (data is not null) {
                        drop = data.upperZ - data.lowerZ;
                    } else {
                        drop = 0;
                    }
                    var slope = length <= 0 || drop < 0 ? 0 : drop / length;
                    dsNode = dsNodeIndex >= 0 ? channel.GetFieldAsInteger(dsNodeIndex) : -1;
                    var dsNode2 = dsNode2Index >= 0 ? channel.GetFieldAsInteger(dsNode2Index) : -1;
                    if (useGridModel && chBasin < 0) {
                        // it is the downstream channel link from an inlet, and has no basin
                    } else {
                        this.chLinkToChBasin[chLink] = chBasin;
                        this.chBasinToChLink[chBasin] = chLink;
                        // exit channels in grid model can have zero length
                        if (length > 0 || useGridModel) {
                            SWATChannel += 1;
                            this.channelToSWATChannel[chLink] = SWATChannel;
                            this.SWATChannelToChannel[SWATChannel] = chLink;
                        } else if (dsNode < 0 || this.isHUC || this.isHAWQS) {
                            // inlets in HUC and HAWQS models are zero length channels, and these have positive DSNODEIDs
                            this.zeroChannels.Add(chLink);
                        }
                    }
                    maxChLink = Math.Max(maxChLink, chLink);
                    this.downChannels[chLink] = dsChLink;
                    this.channelLengths[chLink] = length;
                    this.channelSlopes[chLink] = slope;
                    if (dsNode >= 0) {
                        dsNodeToLink[dsNode] = chLink;
                        //Utils.loginfo('DSNode {0} mapped to channel link {1}', dsNode, chLink))
                    }
                    if (dsNode2 >= 0) {
                        dsNodeToLink[dsNode2] = chLink;
                        //Utils.loginfo('Minor DSNode {0} mapped to channel link {1}', dsNode2, chLink))
                    }
                    if (dsChLink >= 0 && !((this.isHUC || this.isHAWQS) && chLink >= Topology._HUCPointId)) {
                        // keep HUC links out of us map
                        us.TryAdd(dsChLink, new List<int>());
                        us[dsChLink].Add(chLink);
                        if (!useGridModel) {
                            // try later to fix circularities in grid models
                            // check we haven't just made the us relation circular
                            if (Topology.reachable(dsChLink, new List<int> {
                                chLink
                            }, us)) {
                                Utils.error(string.Format("Circular drainage network from channel link {0}", dsChLink), this.isBatch);
                                return false;
                            }
                        }
                    }
                    if (this.isHUC || this.isHAWQS) {
                        this.drainAreas[chLink] = channel.GetFieldAsDouble(totDAIndex) * 1000000.0;
                    }
                } while (channel != null);
                var time2 = DateTime.Now;
                Utils.loginfo(string.Format("Topology setup for channels took {0} seconds", Convert.ToInt32(time2.Subtract(time1).TotalSeconds)));
                if (!useGridModel) {
                    this.setChannelBasinAreas(gv);
                    this.origChBasinAreas = Topology.copyBasinAreas(this.chBasinAreas);
                    if (existing) {
                        // need to set centroids
                        subbasinsLayer.ResetReading();
                        OSGeo.OGR.Feature polygon = null;
                        do {
                            polygon = subbasinsLayer.GetNextFeature();
                            if (polygon == null) { break; }
                            var basin = polygon.GetFieldAsInteger(polyIndex);
                            var centroid = polygon.GetGeometryRef().Centroid();
                            double[] centre = new double[3];
                            centroid.GetPoint(0, centre);
                            this.basinCentroids[basin] = centre;
                        } while (polygon != null);
                        // find maximum channel flow length for each subbasin
                        this.setMaxFlowLengths();
                    }
                }
                var time3 = DateTime.Now;
                Utils.loginfo(string.Format("Topology setup of subbasin areas and centroids took {0} seconds", Convert.ToInt32(time3.Subtract(time2).TotalSeconds)));
                if (dsNodeIndex >= 0) {
                    var doneNodes = new HashSet<int>();
                    outletLayer.ResetReading();
                    OSGeo.OGR.Feature point = null;
                    do {
                        point = outletLayer.GetNextFeature();
                        if (point is null) { break; }
                        // ignore HUC reservoir and pond points: only for display
                        if (this.isHUC && point.GetFieldAsInteger(resIndex) > 0) {
                            continue;
                        }
                        dsNode = point.GetFieldAsInteger(idIndex);
                        if (doneNodes.Contains(dsNode)) {
                            if (reportErrors) {
                                Utils.error(string.Format("ID value {0} is used more than once in inlets/outlets file {1}.  Occurrences after the first are ignored", dsNode, outletFile), this.isBatch);
                            }
                            chLink = -1;
                        } else if (this.lostDsNodeIds.Contains(dsNode)) {
                            chLink = -1;
                        } else if (!dsNodeToLink.ContainsKey(dsNode)) {
                            if (this.isHUC || this.isHAWQS) {
                                // extra points can occur, typically duplicates: ignore
                                continue;
                            }
                            if (reportErrors) {
                                Utils.error(string.Format(@"ID value {0} from inlets/outlets file {1} not found as DSNODEID in channels file {2}.  Will be ignored.
                        This can be caused by an occasional error in delineation: try rerunning 'Create watershed'.", dsNode, outletFile, channelFile), this.isBatch);
                            }
                            chLink = -1;
                        } else {
                            chLink = dsNodeToLink[dsNode];
                        }
                        doneNodes.Add(dsNode);
                        if (chLink >= 0) {
                            if (point.GetFieldAsInteger(inletIndex) == 1) {
                                if (point.GetFieldAsInteger(ptSourceIndex) == 1) {
                                    isPtSource = true;
                                    isInlet = false;
                                } else {
                                    isPtSource = false;
                                    isInlet = true;
                                }
                            } else {
                                isPtSource = false;
                                isInlet = false;
                            }
                            var isReservoir = point.GetFieldAsInteger(resIndex) == 1;
                            var isPond = point.GetFieldAsInteger(resIndex) == 2;
                            var isAdded = addIndex >= 0 && point.GetFieldAsInteger(addIndex) == 1;
                            pt = new double[3];
                            point.GetGeometryRef().GetPoint(0, pt);
                            if (!isAdded && lakesLayer is not null) {
                                OSGeo.OGR.Feature lake = null;
                                do {
                                    lake = lakesLayer.GetNextFeature();
                                    if (lake is null) { break; }
                                    var lakeGeom = lake.GetGeometryRef();
                                    var lakeRect = new OSGeo.OGR.Envelope();
                                    lakeGeom.GetEnvelope(lakeRect);
                                    if (Topology.polyContains(pt, lakeGeom, lakeRect)) {
                                        if (isInlet) {
                                            typ = "Inlet";
                                        } else if (isPtSource) {
                                            typ = "Point source";
                                        } else if (isReservoir) {
                                            typ = "Reservoir";
                                        } else if (isPond) {
                                            typ = "Pond";
                                        } else {
                                            // main outlets allowed within lakes
                                            break;
                                        }
                                        var lakeIdIndex = this.getIndex(lakesLayer, Topology._LAKEID);
                                        var lakeId = lake.GetFieldAsInteger(lakeIdIndex);
                                        Utils.information(string.Format("{0} {1} is inside lake {2}.  Will be ignored.", typ, dsNode, lakeId), this.isBatch);
                                        break;
                                    }
                                } while (lake != null);
                            }
                            if (isInlet) {
                                if (isPtSource) {
                                    this.chLinkToPtSrc[chLink] = (point.GetFieldAsInteger(ptIdIndex), pt);
                                } else if (useGridModel) {
                                    // inlets collected in setUp0 for non-grids
                                    this.chLinkToInlet[chLink] = (point.GetFieldAsInteger(ptIdIndex), pt);
                                }
                            } else if (isReservoir) {
                                this.chLinkToWater[chLink] = (point.GetFieldAsInteger(ptIdIndex), pt, WaterRole._RESERVOIR);
                            } else if (isPond) {
                                this.chLinkToWater[chLink] = (point.GetFieldAsInteger(ptIdIndex), pt, WaterRole._POND);
                            }
                            // else an outlet: nothing to do
                            // check for user-defined outlets coincident with stream junctions
                            if (this.zeroChannels.Contains(chLink) && !this.chLinkIntoLake.ContainsKey(chLink) && !(isInlet && (this.isHUC || this.isHAWQS))) {
                                if (isInlet) {
                                    typ = "Inlet";
                                } else if (isPtSource) {
                                    typ = "Point source";
                                } else if (isReservoir) {
                                    typ = "Reservoir";
                                } else if (isPond) {
                                    typ = "Pond";
                                } else {
                                    typ = "Outlet";
                                }
                                if (typ != "Outlet") {
                                    var msg = String.Format("{0} with id {1} has a zero length channel leading to it: please remove or move downstream", typ, dsNode);
                                    if (reportErrors) {
                                        Utils.error(msg, this.isBatch);
                                    } else {
                                        Utils.loginfo(msg);
                                    }
                                    return false;
                                }
                            }
                        }
                    } while (point != null);
                }
                var time4 = DateTime.Now;
                Utils.loginfo(string.Format("Topology setup for inlets/outlets took {0} seconds", Convert.ToInt32(time4.Subtract(time3).TotalSeconds)));
                // Add any extra reservoirs and point sources
                // create drainAreas here for non-HUC models as we now have maxChLink value to size the array
                if (!(this.isHUC || this.isHAWQS)) {
                    if (totDAIndex < 0) {
                        for (int i = 0; i <= maxChLink; i++) {
                            this.drainAreas[i] = 0;
                        }
                        if (useGridModel) {
                            var gridCellArea = this.dx * this.dy * gv.gridSize * gv.gridSize;
                            // try to use Drainage field from grid channels shapefile
                            if (streamDrainage) {
                                ok = this.setGridDrainageFromChannels(channelLayer, subbasinsLayer, maxChLink, us);
                            } else {
                                ok = false;
                            }
                            if (!ok) {
                                this.setGridDrainageAreas(channelLayer, subbasinsLayer, maxChLink, us, gridCellArea);
                            }
                        } else {
                            // can use drain areas from TauDEM if we have them
                            if (drainAreaIndex >= 0) {
                                this.setDrainageFromChannels(channelLayer, drainAreaIndex);
                            } else {
                                this.setDrainageAreas(us);
                            }
                        }
                    }
                }
                var time5 = DateTime.Now;
                Utils.loginfo(string.Format("Topology drainage took {0} seconds", Convert.ToInt32(time5.Subtract(time4).TotalSeconds)));
                // Strahler order
                this.strahler = new Dictionary<int, int>();
                for (int i = 0; i <= maxChLink; i++) {
                    this.strahler[i] = 0;
                }
                if (orderIndex >= 0) {
                    this.setStrahlerFromChannels(channelLayer, orderIndex);
                } else if (useGridModel) {
                    this.setStrahlerFromGrid(us);
                } else {
                    this.setStrahler(us);
                }
                //try existing subbasin numbers as SWAT basin numbers
                ok = polyIndex >= 0 && subbasinIndex >= 0 && this.tryBasinAsSWATBasin(subbasinsLayer, polyIndex, subbasinIndex, useGridModel);
                int SWATBasin = 0;
                if (!ok) {
                    // failed attempt may have put data in these, so clear them
                    this.subbasinToSWATBasin.Clear();
                    this.SWATBasinToSubbasin.Clear();
                    // create SWAT basin numbers
                    subbasinsLayer.ResetReading();
                    OSGeo.OGR.Feature feature = null;
                    do {
                        feature = subbasinsLayer.GetNextFeature();
                        if (feature is null) { break; }
                        subbasin = feature.GetFieldAsInteger(polyIndex);
                        if (!this.upstreamFromInlets.Contains(subbasin) && (useGridModel || this.chBasinToSubbasin.Values.Contains(subbasin))) {
                            // else was removed by chBasin being within a lake
                            SWATBasin += 1;
                            this.subbasinToSWATBasin[subbasin] = SWATBasin;
                            this.SWATBasinToSubbasin[SWATBasin] = subbasin;
                        }
                    } while (feature != null);
                }

                // put SWAT Basin numbers in subbasin field of subbasins shapefile
                if (subbasinIndex < 0) {
                    // need to Add subbasin field
                    var subField = new FieldDefn(Topology._SUBBASIN, OSGeo.OGR.FieldType.OFTInteger);
                    subbasinsLayer.CreateField(subField, 1);
                    subbasinIndex = subbasinsLayer.GetLayerDefn().GetFieldIndex(Topology._SUBBASIN);
                }
                subbasinsLayer.ResetReading();
                OSGeo.OGR.Feature poly = null;
                do {
                    poly = subbasinsLayer.GetNextFeature();
                    if (poly is null) { break; }
                    subbasin = poly.GetFieldAsInteger(polyIndex);
                    if (!this.subbasinToSWATBasin.TryGetValue(subbasin, out SWATBasin)) {
                        SWATBasin = 0;
                    }
                    poly.SetField(subbasinIndex, SWATBasin);
                    subbasinsLayer.SetFeature(poly);
                } while (poly != null);
                // put SWAT Basin numbers in subbasin field of subsNoLakes shapefile if exists
                if (File.Exists(gv.subsNoLakesFile)) {
                    using (var ds = Ogr.Open(gv.subsNoLakesFile, 1)) {
                        var subsNoLakesLayer = ds.GetLayerByIndex(0);
                        subbasinIndex = this.getIndex(subsNoLakesLayer, Topology._SUBBASIN, ignoreMissing: true);
                        if (subbasinIndex < 0) {
                            // need to Add subbasin field
                            var subField = new FieldDefn(Topology._SUBBASIN, OSGeo.OGR.FieldType.OFTInteger);
                            subsNoLakesLayer.CreateField(subField, 1);
                            subbasinIndex = subsNoLakesLayer.GetLayerDefn().GetFieldIndex(Topology._SUBBASIN);
                        }
                        OSGeo.OGR.Feature subNoLake = null;
                        do {
                            subNoLake = subsNoLakesLayer.GetNextFeature();
                            if (subNoLake is null) { break; }
                            subbasin = subNoLake.GetFieldAsInteger(polyIndex);
                            if (!this.subbasinToSWATBasin.TryGetValue(subbasin, out SWATBasin)) {
                                SWATBasin = 0;
                            }
                            subNoLake.SetField(subbasinIndex, SWATBasin);
                            subbasinsLayer.SetFeature(subNoLake);
                        } while (subNoLake != null);
                    }
                }
                var time6 = DateTime.Now;
                Utils.loginfo(string.Format("Topology setting SWATBasin numbers took {0} seconds", Convert.ToInt32(time6.Subtract(time5).TotalSeconds)));
                //subbasinsLayer.setLabelsEnabled(true);
                //subbasinsLayer.triggerRepaint();
                if (!useGridModel) {
                    // Add SWAT channel numbers to watershed shapefile
                    using (var wshedDs = Ogr.Open(gv.wshedFile, 1)) {
                        var wshedLayer = wshedDs.GetLayerByIndex(0);
                        var wshedPolyIndex = this.getIndex(wshedLayer, Topology._POLYGONID, ignoreMissing: ignoreError);
                        var wshedChannelIndex = this.getIndex(wshedLayer, Topology._CHANNEL, ignoreMissing: ignoreWithGridOrExisting);
                        if (wshedChannelIndex < 0) {
                            var channelField = new OSGeo.OGR.FieldDefn(Topology._CHANNEL, OSGeo.OGR.FieldType.OFTInteger);
                            wshedLayer.CreateField(channelField, 1);
                            wshedChannelIndex = wshedLayer.GetLayerDefn().GetFieldIndex(Topology._CHANNEL);
                        }
                        wshedLayer.ResetReading();
                        OSGeo.OGR.Feature feature;
                        do {
                            feature = wshedLayer.GetNextFeature();
                            if (feature is null) { break; }
                            chBasin = feature.GetFieldAsInteger(wshedPolyIndex);
                            if (this.chBasinToChLink.TryGetValue(chBasin, out chLink)) {
                                if (!this.channelToSWATChannel.TryGetValue(chLink, out SWATChannel)) {
                                    SWATChannel = 0;
                                }
                            } else {
                                SWATChannel = 0;
                            }
                            feature.SetField(wshedChannelIndex, SWATChannel);
                            wshedLayer.SetFeature(feature);
                        } while (feature != null);
                    }
                }
                var drainageFile = Utils.join(gv.shapesDir, gv.projName + Parameters._DRAINAGECSV);
                this.writeDrainageFile(drainageFile);
                return useGridModel || lakesLayer is not null || this.checkAreas(subbasinsLayer, gv);
            }
        }

        // Add lakes from lakes shapefile layer.
        //         
        //         Not used with grid models.
        public async Task<bool> addLakes(
            OSGeo.OGR.Layer lakesLayer,
            OSGeo.OGR.Layer subbasinsLayer,
            OSGeo.OGR.Layer chBasinsLayer,
            OSGeo.OGR.Layer streamsLayer,
            OSGeo.OGR.Layer channelsLayer,
            RasterLayer demLayer,
            double snapThreshold,
            GlobalVars gv,
            bool reportErrors = true) {
            int newPointId;
            long channelId;
            double elev;
            int link;
            int subbasin;
            double area2;
            OSGeo.OGR.Geometry newGeom;
            double area1;
            WaterRole waterRole;
            var errorCount = 0;
            //using (var lakesDs = Ogr.Open(lakesFile, 1))
            //using (var subbasinsDs = Ogr.Open(subbasinsFile, 1))
            //using (var chBasinsDs = Ogr.Open(chBasinsFile, 1))
            //using (var streamsDs = Ogr.Open(streamsFile, 1))
            //using (var channelsDs = Ogr.Open(channelsFile, 1)) {
            //    var lakesLayer = lakesDs.GetLayerByIndex(0);
            //    var subbasinsLayer = subbasinsDs.GetLayerByIndex(0);
            //    var chBasinsLayer = chBasinsDs.GetLayerByIndex(0);
            //    var streamsLayer = streamsDs.GetLayerByIndex(0);
            //    var channelsLayer = channelsDs.GetLayerByIndex(0);
            var lakesDefn = lakesLayer.GetLayerDefn();
            var lakeIdIndex = lakesDefn.GetFieldIndex(Topology._LAKEID);
            var lakeResIndex = lakesDefn.GetFieldIndex(Topology._RES);
            var lakeAreaIndex = lakesDefn.GetFieldIndex(Parameters._AREA);
            if (lakeResIndex < 0) {
                Utils.information(string.Format("No RES field in lakes shapefile {0}: assuming lakes are reservoirs", gv.lakeFile), this.isBatch, reportErrors: reportErrors);
            }
            var subbasinsDefn = subbasinsLayer.GetLayerDefn();
            var subbasinsPolyIndex = subbasinsDefn.GetFieldIndex(Topology._POLYGONID);
            var subbasinsAreaIndex = subbasinsDefn.GetFieldIndex(Parameters._AREA);
            if (subbasinsAreaIndex < 0) {
                Utils.error(string.Format("Cannot find {0} field in {1}", Parameters._AREA, gv.subbasinsFile), this.isBatch);
                return false;
            }
            var chBasinsDefn = chBasinsLayer.GetLayerDefn();
            var chBasinsPolyIndex = chBasinsDefn.GetFieldIndex(Topology._POLYGONID);
            var chBasinsAreaIndex = chBasinsDefn.GetFieldIndex(Parameters._AREA);
            var channelsDefn = channelsLayer.GetLayerDefn();
            var channelLinkIndex = channelsDefn.GetFieldIndex(Topology._LINKNO);
            var DSLINKNO = this.isHUC ? Topology._DSLINKNO1 : Topology._DSLINKNO;
            var channelDsLinkIndex = channelsDefn.GetFieldIndex(DSLINKNO);
            var DSNODEID = this.isHUC ? Topology._DSNODEID1 : Topology._DSNODEID;
            var channelDsNodeIndex = channelsDefn.GetFieldIndex(DSNODEID);
            var channelDrainAreaIndex = channelsDefn.GetFieldIndex(Topology._DRAINAREA);
            if (channelDrainAreaIndex < 0) {
                channelDrainAreaIndex = channelsDefn.GetFieldIndex(Topology._DRAINAREA2);
            }
            var channelWSNOIndex = channelsDefn.GetFieldIndex(Topology._WSNO);
            var channelBasinIndex = channelsDefn.GetFieldIndex(Topology._BASINNO);
            this.addLakeFieldsToChannels(channelsLayer);
            var channelLakeInIndex = channelsDefn.GetFieldIndex(Topology._LAKEIN);
            var channelLakeOutIndex = channelsDefn.GetFieldIndex(Topology._LAKEOUT);
            var channelLakeWithinIndex = channelsDefn.GetFieldIndex(Topology._LAKEWITHIN);
            var channelLakeMainIndex = this.getIndex(channelsLayer, Topology._LAKEMAIN);
            this.chLinkIntoLake = new Dictionary<int, int>();
            this.chLinkInsideLake = new Dictionary<int, int>();
            this.chLinkFromLake = new Dictionary<int, int>();
            var areaFactor = gv.horizontalFactor * gv.horizontalFactor;
            OSGeo.OGR.Feature lake = null;
            lakesLayer.ResetReading();
            do {
                lake = lakesLayer.GetNextFeature();
                if (lake is null) { break; }
                var lakeGeom = lake.GetGeometryRef();
                var lakeRect = new OSGeo.OGR.Envelope();
                lakeGeom.GetEnvelope(lakeRect);
                var lakeId = lake.GetFieldAsInteger(lakeIdIndex);
                var lakeCentroid = lakeGeom.Centroid();
                var lakeArea = lakeGeom.Area() * areaFactor;
                var lakeOverrideArea = lakeArea;
                if (lakeResIndex < 0) {
                    waterRole = WaterRole._RESERVOIR;
                } else {
                    waterRole = (WaterRole)lake.GetFieldAsInteger(lakeResIndex);
                }
                if (lakeAreaIndex >= 0) {
                    try {
                        lakeOverrideArea = lake.GetFieldAsDouble(lakeAreaIndex) * 10000.0;
                    }
                    catch {
                    }
                }
                double[] centroid = new double[3];
                lakeCentroid.GetPoint(0, centroid);
                var lakeData = new LakeData(lakeArea, lakeOverrideArea, centroid, waterRole);
                // reservoirs, ponds and wetlands are removed from subbasins and LSUs maps; playas will make HRUs
                if (waterRole == WaterRole._PLAYA) {
                    lakeData.elevation = Topology.valueAtPoint(centroid, demLayer.GetRaster());
                    this.lakesData[lakeId] = lakeData;
                    continue;
                }
                var totalElevation = 0.0;
                var elevPointCount = 0;
                var attMap = new Dictionary<long, Dictionary<int, double>>();
                var geomMap = new Dictionary<long, OSGeo.OGR.Geometry>();
                // the area removed from channel basins that intersect wih the lake
                var chBasinWaterArea = 0.0;
                var minArea = this.dx * this.dy;
                subbasinsLayer.ResetReading();
                OSGeo.OGR.Feature sub;
                do {
                    sub = subbasinsLayer.GetNextFeature();
                    if (sub is null) { break; }
                    var subGeom = sub.GetGeometryRef();
                    if (Topology.intersectsPoly(subGeom, lakeGeom, lakeRect)) {
                        var subId = sub.GetFID();
                        area1 = subGeom.Area() * areaFactor;
                        newGeom = subGeom.Difference(lakeGeom);
                        area2 = newGeom.Area() * areaFactor;
                        if (area2 < area1) {
                            var subPoly = sub.GetFieldAsInteger(subbasinsPolyIndex);
                            Utils.loginfo(string.Format("Lake {0} overlaps subbasin polygon {1}: area reduced from {2} to {3}", lakeId, subPoly, area1, area2));
                            if (area2 < minArea) {
                                subbasinsLayer.DeleteFeature(subId);
                                // remove subbasin from range of chBasinToSubbasin
                                var chBasinsToRemove = new HashSet<int>();
                                foreach (var (chBasin, subbsn) in this.chBasinToSubbasin) {
                                    if (subbsn == subPoly) {
                                        chBasinsToRemove.Add(chBasin);
                                    }
                                }
                                foreach (var chBasin in chBasinsToRemove) {
                                    this.chBasinToSubbasin.Remove(chBasin);
                                }
                                Utils.loginfo(string.Format("Subbasin polygon {0} removed", subPoly));
                            } else {
                                sub.SetGeometry(newGeom);
                                sub.SetField(subbasinsAreaIndex, area2 / 10000.0);
                                subbasinsLayer.SetFeature(sub);
                            }
                        }
                    }
                } while (sub != null);
                // map of polygon id to area that is part of the lake
                var channelAreaChange = new Dictionary<int, double>();
                chBasinsLayer.ResetReading();
                OSGeo.OGR.Feature chBasinFeature;
                do {
                    chBasinFeature = chBasinsLayer.GetNextFeature();
                    if (chBasinFeature is null) { break; }
                    var chBasinGeom = chBasinFeature.GetGeometryRef();
                    var polyId = chBasinFeature.GetFieldAsInteger(chBasinsPolyIndex);
                    // if area reduced to zero because inside another lake, geometry is None
                    if (chBasinGeom is not null && Topology.intersectsPoly(chBasinGeom, lakeGeom, lakeRect)) {
                        var chBasinId = chBasinFeature.GetFID();
                        area1 = chBasinGeom.Area() * areaFactor;
                        newGeom = chBasinGeom.Difference(lakeGeom);
                        area2 = newGeom.Area() * areaFactor;
                        if (area2 < area1) {
                            Utils.loginfo(string.Format("Lake {0} overlaps channel basin {1}: area reduced from {2} to {3}", lakeId, polyId, area1, area2));
                            chBasinWaterArea += area1 - area2;
                            chBasinFeature.SetGeometry(newGeom);
                            chBasinFeature.SetField(chBasinsAreaIndex, area2 / 10000.0);
                            chBasinsLayer.SetFeature(chBasinFeature);
                            channelAreaChange[polyId] = area1 - area2;
                            if (area2 < minArea) {
                                // channel basin disappears into lake: remove its mapping to subbasin
                                // may already have been removed because all subbasin in lake
                                if (this.chBasinToSubbasin.TryGetValue(polyId, out subbasin)) {
                                    if (subbasin >= 0) {
                                        this.chBasinToSubbasin.Remove(polyId);
                                        Utils.loginfo(string.Format("Channel basin {0} (subbasin {1}) removed", polyId, subbasin));
                                    }
                                }
                            }
                        }
                    }
                } while (chBasinFeature != null);
                var currentDrainArea = 0.0;
                // first pass through channels: collect inflowing and outflowing channels from DsNodes in lakeInlets and lakeOutlets
                channelsLayer.ResetReading();
                OSGeo.OGR.Feature channel;
                do {
                    channel = channelsLayer.GetNextFeature();
                    if (channel is null) { break; }
                    link = channel.GetFieldAsInteger(channelLinkIndex);
                    var dsLink = channel.GetFieldAsInteger(channelDsLinkIndex);
                    var dsNode = channel.GetFieldAsInteger(channelDsNodeIndex);
                    if (dsNode > 0) {
                        if (this.lakeInlets[lakeId].Contains(dsNode)) {
                            var inflowData = await this.getOgrReachData(channel, demLayer);
                            Debug.Assert(inflowData is not null);
                            elev = inflowData.lowerZ;
                            lakeData.inChLinks[link] = (dsNode, new double[3] { inflowData.lowerX, inflowData.lowerY, 0 }, (double)elev);
                            if (dsLink >= 0) {
                                lakeData.lakeChLinks.Add(dsLink);
                                this.chLinkInsideLake[dsLink] = lakeId;
                            }
                            this.chLinkIntoLake[link] = lakeId;
                            if (!Utils.areClose((double)elev, gv.elevationNoData, rel_tol: 1E-06)) {
                                // type: ignore
                                totalElevation += (double)elev;
                                elevPointCount += 1;
                            }
                            var wsno = channel.GetFieldAsInteger(channelWSNOIndex);
                            double areaChange;
                            if (!channelAreaChange.TryGetValue(wsno, out areaChange)) {
                                areaChange = 0;
                            }
                            var drainArea = channel.GetFieldAsDouble(channelDrainAreaIndex) * areaFactor - areaChange;
                            channel.SetField(channelDrainAreaIndex, drainArea);
                        } else if (this.lakeOutlets[lakeId].Contains(dsNode)) {
                            var outflowData = await this.getOgrReachData(channel, demLayer);
                            Debug.Assert(outflowData is not null);
                            var outlet = new double[3] { outflowData.lowerX, outflowData.lowerY, 0 };
                            var replace = true;
                            if (dsLink >= 0) {
                                if (lakeData.outPoint.Item3 is not null) {
                                    // choose point with larger drain area 
                                    var newDrainArea = channel.GetFieldAsDouble(channelDrainAreaIndex);
                                    if (newDrainArea > currentDrainArea) {
                                        currentDrainArea = newDrainArea;
                                        if (lakeData.outChLink >= 0) {
                                            lakeData.otherOutChLinks.Add(lakeData.outChLink);
                                        }
                                    } else {
                                        replace = false;
                                    }
                                }
                                if (replace) {
                                    // don't use WSNO as route to subbasin as channel basin may have been deleted
                                    // wsno = channel[channelWSNOIndex]
                                    // subbasin = self.chBasinToSubbasin[wsno]
                                    subbasin = channel.GetFieldAsInteger(channelBasinIndex);
                                    lakeData.outPoint = (subbasin, dsNode, outlet, outflowData.lowerZ);
                                    lakeData.outChLink = dsLink;
                                } else {
                                    lakeData.otherOutChLinks.Add(dsLink);
                                }
                                this.chLinkFromLake[dsLink] = lakeId;
                            }
                            lakeData.lakeChLinks.Add(link);
                            this.chLinkInsideLake[link] = lakeId;
                        }
                    }
                } while (channel is not null);
                // check to see if a watershed outlet was marked inside the lake
                // and if so try to move it to the lake perimeter.  Else leave it as an internal outlet. 
                // we don't need to exclude outlets created to split channels flowing into and out of lake
                // because the outlets map is made from the streams before lake inlets and outlets are added to the snap file
                // and the augmented snapfile is only used to make channels
                foreach (var (subbsn, (pointId, pt, chs)) in this.outlets) {
                    if (Topology.polyContains(pt, lakeGeom, lakeRect) && this.isWatershedOutlet(pointId, channelsLayer, channelDsLinkIndex, channelDsNodeIndex)) {
                        if (!File.Exists(gv.pFile)) {
                            Utils.error(string.Format("Cannot find D8 flow directions file {0}", gv.pFile), this.isBatch);
                            break;
                        }
                        // need to give different id to outPoint, since this is used to make the reservoir point
                        // which will then route to the subbasin outlet
                        // can use outlet point id if already created
                        if (lakeData.outPoint.Item2 >= 0) {
                            newPointId = lakeData.outPoint.Item2;
                        } else {
                            this.pointId += 1;
                            newPointId = this.pointId;
                        }
                        elev = Topology.valueAtPoint(pt, demLayer.GetRaster());
                        if (double.IsNaN(elev)) {
                            elev = 0;
                        }
                        lakeData.outPoint = (subbsn, newPointId, pt, (double)elev);
                        // maximum number of steps approximates to the threshold for snapping points expressed as number of DEM cells
                        var maxSteps = this.dx == 0 ? 5 : Convert.ToInt32(snapThreshold / this.dx);
                        var (lakeOutlet, found) = Topology.movePointToPerimeter(pt, lakeGeom, gv.pFile, maxSteps);
                        if (found) {
                            if (lakeData.outPoint.Item3 is not null) {
                                Utils.information(string.Format("User marked outlet {0} chosen as main outlet for lake {1}", pointId, lakeId), gv.isBatch, reportErrors: reportErrors);
                                if (lakeData.outChLink >= 0) {
                                    lakeData.otherOutChLinks.Add(lakeData.outChLink);
                                }
                            }
                            elev = Topology.valueAtPoint(lakeOutlet, demLayer.GetRaster());
                            if (double.IsNaN(elev)) {
                                elev = 0;
                            }
                            lakeData.outPoint = (subbsn, newPointId, lakeOutlet, (double)elev);
                            Utils.loginfo(string.Format("Outlet of lake {0} set to ({1}, {2})", lakeId, Convert.ToInt32(lakeOutlet[0]), Convert.ToInt32(lakeOutlet[1])));
                            // update outlets map 
                            this.outlets[subbsn] = (pointId, lakeOutlet, chs);
                        } else {
                            Utils.loginfo(string.Format("Outlet of lake {0} set to internal point ({1}, {2})", lakeId, Convert.ToInt32(lakeOutlet[0]), Convert.ToInt32(lakeOutlet[1])));
                        }
                        lakeData.outChLink = -1;
                        break;
                    }
                }
                // second pass through channels: collect channels within lake: i.e. both ends in lake
                // and set LakeIn, LakeOut, LakeWithin fields
                channelsLayer.ResetReading();
                do {
                    channel = channelsLayer.GetNextFeature();
                    if (channel is null) { break; }
                    link = channel.GetFieldAsInteger(channelLinkIndex);
                    channelId = channel.GetFID();
                    ReachData channelData = null;
                    int lakeIn;
                    if (!this.chLinkIntoLake.TryGetValue(link, out lakeIn)) {
                        lakeIn = 0;
                    }
                    int lakeOut;
                    if (!this.chLinkFromLake.TryGetValue(link, out lakeOut)) {
                        lakeOut = 0;
                    }
                    int lakeWithin;
                    if (!this.chLinkInsideLake.TryGetValue(link, out lakeWithin)) {
                        lakeWithin = 0;
                    }
                    if (!this.chLinkIntoLake.ContainsKey(link) && !this.chLinkFromLake.ContainsKey(link) && !this.chLinkInsideLake.ContainsKey(link)) {
                        channelData = await this.getOgrReachData(channel, null);
                        Debug.Assert(channelData is not null);
                        var pt1 = new double[3] { channelData.lowerX, channelData.lowerY, 0 };
                        var pt2 = new double[3] { channelData.upperX, channelData.upperY, 0 };
                        if (Topology.polyContains(pt1, lakeGeom, lakeRect) && Topology.polyContains(pt2, lakeGeom, lakeRect)) {
                            lakeData.lakeChLinks.Add(link);
                            this.chLinkInsideLake[link] = lakeId;
                            lakeWithin = lakeId;
                        }
                    }
                    channel.SetField(channelLakeInIndex, lakeIn);
                    channel.SetField(channelLakeOutIndex, lakeOut);
                    channel.SetField(channelLakeWithinIndex, lakeWithin);
                    channelsLayer.SetFeature(channel);
                    if (lakeData.lakeChLinks.Contains(link)) {
                        // remove the channel's point source
                        // other errors may cause the link to not exist and hence not to have a point source
                        if (this.chPointSources.ContainsKey(link)) {
                            this.chPointSources.Remove(link);
                        }
                    }
                } while (channel is not null);
                // if the lake has an outlet channel with a drain area less than LAKEOUTLETCHANNELAREA percent of the lake area
                // and dsNode negative (to avoid pulling a point inside the lake) make its channel internal
                long outLinkId = -1;
                var outLink = lakeData.outChLink;
                var outBasin = -1;
                var dsOutLink = -1;
                var dsNodeOutLink = -1;
                if (outLink >= 0) {
                    channel = Utils.getOgrFeatureByValue(channelsLayer, Topology._LINKNO, outLink);
                    outLinkId = channel.GetFID();
                    outBasin = channel.GetFieldAsInteger(channelWSNOIndex);
                    dsOutLink = channel.GetFieldAsInteger(channelDsLinkIndex);
                    dsNodeOutLink = channel.GetFieldAsInteger(channelDsNodeIndex);
                }
                if (dsNodeOutLink < 0 && outBasin >= 0) {
                    // threshold in ha: LAKEOUTLETCHANNELAREA of lake area
                    var threshold = lakeData.area / 1000000.0 * Parameters._LAKEOUTLETCHANNELAREA;
                    chBasinsLayer.ResetReading();
                    do {
                        chBasinFeature = chBasinsLayer.GetNextFeature();
                        if (chBasinFeature is null) { break; }
                        if (chBasinFeature.GetFieldAsInteger(chBasinsPolyIndex) == outBasin) {
                            var areaHa = chBasinFeature.GetFieldAsDouble(chBasinsAreaIndex);
                            if (areaHa < threshold) {
                                // move outlet channel inside lake
                                lakeData.lakeChLinks.Add(outLink);
                                lakeData.outChLink = dsOutLink;
                                this.chLinkFromLake.Remove(outLink);
                                this.chLinkInsideLake[outLink] = lakeId;
                                // mark it as within as well as being the outlet (already set)
                                chBasinFeature.SetField(channelLakeWithinIndex, lakeId);
                                chBasinsLayer.SetFeature(chBasinFeature);
                                // check if this point now inside the lake is a subbasin outlet
                                subbasin = this.chBasinToSubbasin[outBasin];
                                (_, _, var outChannels) = this.outlets[subbasin];
                                if (outChannels.Contains(outLink)) {
                                    // subbasin outlet has moved inside the lake
                                    this.outletsInLake[subbasin] = lakeId;
                                }
                                Utils.loginfo(string.Format("Channel link {0} channel basin {1} moved inside lake {2}", outLink, outBasin, lakeId));
                                // remove the channel's point source
                                this.chPointSources.Remove(outLink);
                                if (dsOutLink >= 0) {
                                    this.chLinkFromLake[dsOutLink] = lakeId;
                                }
                                break;
                            }
                        }
                    } while (chBasinFeature is not null);
                }
                if (lakeData.outPoint.Item3 is null) {
                    Utils.error(string.Format("Failed to find outlet for lake {0}", lakeId), this.isBatch);
                    errorCount += 1;
                }
                if (lakeData.outChLink >= 0) {
                    long chId = -1;
                    // find the channel's id
                    channel = Utils.getOgrFeatureByValue(channelsLayer, Topology._LINKNO, lakeData.outChLink);
                    chId = channel.GetFID();
                    channel.SetField(channelLakeMainIndex, lakeId);
                    channelsLayer.SetFeature(channel);
                }
                var meanElevation = elevPointCount > 0 ? totalElevation / elevPointCount : lakeData.outPoint.Item4;
                lakeData.elevation = meanElevation;
                Utils.loginfo(string.Format("Lake {0} has outlet on channel {1}, other outlets on channels {2}, inlets on channels {3} and contains channels {4}", lakeId, lakeData.outChLink, lakeData.otherOutChLinks, lakeData.inChLinks.Keys.ToList(), lakeData.lakeChLinks));
                this.lakesData[lakeId] = lakeData;
                lakeArea = lakeData.area;
                var percentChBasinWater = chBasinWaterArea / lakeArea * 100;
                Utils.loginfo(string.Format("Lake {0} has area {1} and channel basin water area {2}: {3}%", lakeId, lakeArea, chBasinWaterArea, percentChBasinWater));
                var intPercent = Convert.ToInt32(percentChBasinWater);
                if (percentChBasinWater < 99 || percentChBasinWater > 101) {
                    Utils.information(string.Format(@"WARNING: Only {0}% of the area of lake {1} is accounted for in your watershed.
                You should carefully check the messages concerning this lake in the QSWAT+ log in the QGIS log messages panel.", intPercent, lakeId), this.isBatch, reportErrors: reportErrors);
                }
            } while (lake != null);
            if (this.lakesData.Count == 0) {
                Utils.error(string.Format("No lakes found in {0}", gv.lakeFile), this.isBatch);
                return false;
            }
            //chBasinsLayer.triggerRepaint();
            //streamsLayer.triggerRepaint();
            //channelsLayer.triggerRepaint();
            return errorCount == 0;
        }
        //}

        // Add LakeIn etc fields to channelsLayer if necessary.
        public virtual void addLakeFieldsToChannels(OSGeo.OGR.Layer channelsLayer) {
            var channelsDefn = channelsLayer.GetLayerDefn();
            var channelLakeInIndex = channelsDefn.GetFieldIndex(Topology._LAKEIN);
            var channelLakeOutIndex = channelsDefn.GetFieldIndex(Topology._LAKEOUT);
            var channelLakeWithinIndex = channelsDefn.GetFieldIndex(Topology._LAKEWITHIN);
            var channelLakeMainIndex = channelsDefn.GetFieldIndex(Topology._LAKEMAIN);
            if (channelLakeInIndex < 0) {
                channelsLayer.CreateField(new FieldDefn(Topology._LAKEIN, OSGeo.OGR.FieldType.OFTInteger), 1);
            }
            if (channelLakeOutIndex < 0) {
                channelsLayer.CreateField(new FieldDefn(Topology._LAKEOUT, OSGeo.OGR.FieldType.OFTInteger), 1);
            }
            if (channelLakeWithinIndex < 0) {
                channelsLayer.CreateField(new FieldDefn(Topology._LAKEWITHIN, OSGeo.OGR.FieldType.OFTInteger), 1);
            }
            if (channelLakeMainIndex < 0) {
                channelsLayer.CreateField(new FieldDefn(Topology._LAKEMAIN, OSGeo.OGR.FieldType.OFTInteger), 1);
            }
        }

        // Return true if there is a channel with dsNode equal to pointId and with dsLink -1.
        public virtual bool isWatershedOutlet(int pointId, OSGeo.OGR.Layer channelslayer, int channelDsLinkIndex, int channelDsNodeIndex) {
            channelslayer.ResetReading();
            OSGeo.OGR.Feature link;
            do {
                link = channelslayer.GetNextFeature();
                if (link is null) { break; }
                if (link.GetFieldAsInteger(channelDsNodeIndex) == pointId && link.GetFieldAsInteger(channelDsLinkIndex) == -1) {
                    return true;
                }
            } while (link is not null);
            return false;

            //     def isOutlet(self, pointId: int, outletsLayer: QgsVectorLayer) -> bool:
            //         """Return true if outletsLayer contains an outlet point with id pointId."""
            //         idIndex = self.getIndex(outletsLayer, Topology._ID, ignoreMissing=True)
            //         inletIndex = self.getIndex(outletsLayer, Topology._INLET, ignoreMissing=True)
            //         resIndex = self.getIndex(outletsLayer, Topology._RES, ignoreMissing=True)
            //         if idIndex < 0 or inletIndex < 0 or resIndex < 0:
            //             return False
            //         exp = QgsExpression('"{0}" = {1}', Topology._ID, pointId))
            //         request = QgsFeatureRequest(exp).setFlags(QgsFeatureRequest.NoGeometry).setSubsetOfAttributes([inletIndex, resIndex])
            //         for point in outletsLayer.dataProvider().getFeatures(request):
            //             if point[inletIndex] == 0 and point[resIndex] == 0:
            //                 return True
            //         return False
        }

        // Add reservoir, pond and wetland lakes when using grid model.  Return number of lakes (which may be zero) or -1 if error.
        public async Task<int> addGridReservoirsPondsAndWetlands(
            OSGeo.OGR.Layer gridLayer,
            OSGeo.OGR.Layer channelsLayer,
            RasterLayer demLayer,
            GlobalVars gv,
            bool reportErrors = true) {
            double drainage;
            var gridDefn = gridLayer.GetLayerDefn();
            var gridPolyIndex = gridDefn.GetFieldIndex(Topology._POLYGONID);
            var gridDownIndex = gridDefn.GetFieldIndex(Topology._DOWNID);
            var gridAreaIndex = gridDefn.GetFieldIndex(Parameters._AREA);
            var gridLakeIdIndex = gridDefn.GetFieldIndex(Topology._LAKEID);
            if (gridLakeIdIndex < 0) {
                // can be no lakes
                return 0;
            }
            var gridResIndex = gridDefn.GetFieldIndex(Topology._RES);
            var channelsDefn = channelsLayer.GetLayerDefn();
            var channelLinkIndex = channelsDefn.GetFieldIndex(Topology._LINKNO);
            var channelDsLinkIndex = channelsDefn.GetFieldIndex(Topology._DSLINKNO);
            var channelWSNOIndex = channelsDefn.GetFieldIndex(Topology._WSNO);
            // the drainage field may not exist if we are using grid or table drainage: deal with this later
            var streamDrainageIndex = channelsDefn.GetFieldIndex(Topology._DRAINAGE);
            var polysIntoLake = new Dictionary<int, int>();
            var polysInsidelake = new Dictionary<int, int>();
            var polysFromLake = new Dictionary<int, int>();
            this.chLinkIntoLake = new Dictionary<int, int>();
            this.chLinkInsideLake = new Dictionary<int, int>();
            this.chLinkFromLake = new Dictionary<int, int>();
            // first make map poly -> lake id
            var polyToLake = new Dictionary<int, int>();
            gridLayer.ResetReading();
            OSGeo.OGR.Feature cell;
            do {
                cell = gridLayer.GetNextFeature();
                if (cell is null) { break; }
                try {
                    var lakeId = cell.GetFieldAsInteger(gridLakeIdIndex);
                    polyToLake[cell.GetFieldAsInteger(gridPolyIndex)] = lakeId;
                    // make sure waterbody id is set to maximum lake id in case using existing grid
                    this.waterBodyId = Math.Max(this.waterBodyId, lakeId);
                } catch {
                    // lakeId was presumably null
                    // Utils.loginfo('At polygonId {0} lakeId has type {1} and value {2}', cell[gridPolyIndex], type(cell[gridLakeIdIndex]), cell[gridLakeIdIndex]))
                }
            } while (cell is not null);
            if (polyToLake.Count == 0) {
                // no lakes
                return 0;
            }
            // data for calculating centroid
            // map of lake id to (area, x moment of area, y moment)
            var lakeAreaData = new Dictionary<int, (WaterRole, double, double, double)>();
            gridLayer.ResetReading();
            do {
                cell = gridLayer.GetNextFeature();
                if (cell is null) { break; }
                WaterRole waterRole = (WaterRole)cell.GetFieldAsInteger(gridResIndex);
                var poly = cell.GetFieldAsInteger(gridPolyIndex);
                var downPoly = cell.GetFieldAsInteger(gridDownIndex);
                int sourceLake;
                int targetLake;
                if (!polyToLake.TryGetValue(downPoly, out targetLake)) {
                    targetLake = -1;
                }
                if (polyToLake.TryGetValue(poly, out sourceLake)) {
                    if (!lakeAreaData.ContainsKey(sourceLake)) {
                        lakeAreaData[sourceLake] = (waterRole, 0, 0, 0);
                    }
                    var area = cell.GetFieldAsDouble(gridAreaIndex) * 10000.0;
                    var (centre, _, _) = Utils.centreGridCell(cell);
                    var (_, totalArea, xMoment, yMoment) = lakeAreaData[sourceLake];
                    lakeAreaData[sourceLake] = (waterRole, totalArea + area, xMoment + area * centre[0], yMoment + area * centre[1]);
                    if (targetLake == sourceLake) {
                        // channel links two lake cells within lake
                        polysInsidelake[poly] = sourceLake;
                    } else {
                        // exit channel
                        polysFromLake[poly] = sourceLake;
                        if (targetLake >= 0) {
                            // also entry channel
                            polysIntoLake[poly] = targetLake;
                        }
                    }
                } else if (targetLake >= 0) {
                    polysIntoLake[poly] = targetLake;
                }
            } while (cell is not null);
            var totalElevation = new Dictionary<int, double>();
            // map of lake id to possible exit channels
            // will choose one with largest drainage
            var exitData = new Dictionary<int, Dictionary<int, (int, double, double[], double)>>();
            foreach (var (lakeId, (waterRole, area, xMoment, yMoment)) in lakeAreaData) {
                var centroid = new double[3] { xMoment / area, yMoment / area, 0 };
                this.lakesData[lakeId] = new LakeData(area, area, centroid, waterRole);
                totalElevation[lakeId] = 0;
                exitData[lakeId] = new Dictionary<int, (int, double, double[], double)>();
            }
            // convert wsnos to links and complete LakesData
            // get maximum chLink and create downChannels map and link to basin map in case drainage needs calculating
            this.downChannels = new Dictionary<int, int>();
            this.chLinkToChBasin = new Dictionary<int, int>();
            var maxChLink = 0;
            channelsLayer.ResetReading();
            OSGeo.OGR.Feature channel;
            do {
                channel = channelsLayer.GetNextFeature();
                if (channel is null) { break; }
                var chLink = channel.GetFieldAsInteger(channelLinkIndex);
                maxChLink = Math.Max(maxChLink, chLink);
                var dsChLink = channel.GetFieldAsInteger(channelDsLinkIndex);
                this.downChannels[chLink] = dsChLink;
                var wsno = channel.GetFieldAsInteger(channelWSNOIndex);
                this.chLinkToChBasin[chLink] = wsno;
                int lakeIdInto;
                if (polysIntoLake.TryGetValue(wsno, out lakeIdInto)) {
                    this.chLinkIntoLake[chLink] = lakeIdInto;
                    // since this is a grid model the grid cells form different subbasins and there will be a suitable outlet
                    // point already stored in the outlets map
                    var (pointId, point, _) = this.outlets[wsno];
                    var elev = Topology.valueAtPoint(point, demLayer.GetRaster());
                    if (double.IsNaN(elev)) {
                        elev = 0;
                    }
                    this.lakesData[lakeIdInto].inChLinks[chLink] = (pointId, point, (double)elev);
                    totalElevation[lakeIdInto] += (double)elev;
                    // if different lake cells abut, channel can be into one and from the other 
                    // continue
                }
                int lakeIdFrom;
                if (polysFromLake.TryGetValue(wsno, out lakeIdFrom)) {
                    // allow for no drainage field
                    drainage = streamDrainageIndex < 0 ? -1 : channel.GetFieldAsDouble(streamDrainageIndex);
                    var data = await this.getOgrReachData(channel, demLayer);
                    if (data is null) {
                        Utils.loginfo(string.Format("No reach data for channel link {0}", chLink));
                    } else {
                        exitData[lakeIdFrom][chLink] = (wsno, drainage, new double[3] { data.upperX, data.upperY, 0 }, data.upperZ);
                    }
                    continue;
                }
                int lakeIdInside;
                if (polysInsidelake.TryGetValue(wsno, out lakeIdInside)) {
                    this.chLinkInsideLake[chLink] = lakeIdInside;
                    this.lakesData[lakeIdInside].lakeChLinks.Add(chLink);
                    continue;
                }
            } while (channel is not null);
            // check if we need to calculate drainage: no drainage field and more than one exit for at least one lake
            var needDrainage = false;
            if (streamDrainageIndex < 0) {
                foreach (var data in exitData.Values) {
                    if (data.Count > 1) {
                        needDrainage = true;
                        break;
                    }
                }
            }
            if (needDrainage) {
                for (int i = 0; i < this.drainAreas.Count; i++) {
                    this.drainAreas[i] = 0;
                }
                var gridCellArea = this.dx * this.dy * gv.gridSize * gv.gridSize;
                // us parameter is set to None here because it is not yet calculated
                // and if there are any circularities they will be removed from the channels shapefile
                // and us will be calculated from that shapefile.
                this.setGridDrainageAreas(channelsLayer, gridLayer, maxChLink, null, gridCellArea);
            }
            // count lakes with multiple outlets 
            var multipleOutletCount = 0;
            foreach (var data in exitData.Values) {
                if (data.Count > 1) {
                    multipleOutletCount += 1;
                }
            }
            var multipleOutletsInteractive = multipleOutletCount <= 5;
            if (!multipleOutletsInteractive) {
                Utils.information(string.Format("For information on {0} lakes with multiple possible outlets see the ArcSWAT+ log", multipleOutletCount), gv.isBatch, reportErrors: reportErrors);
            }
            // find outlet with largest drainage and mark as THE outlet
            foreach (var (lakeId, data) in exitData) {
                // set maxDrainage less than -1 value used for missing drainage so that first exit link registers
                // as if there is only one exit for each lake needDrainage will be false
                var maxDrainage = -2.0;
                var exLink = -1;
                var exWsno = -1;
                double[] exPoint = null;
                var exElev = 0.0;
                foreach (var (chLink, (wsno, drainVal, pt, elev)) in data) {
                    double drainArea;
                    if (needDrainage) {
                        drainArea = this.drainAreas[chLink];
                    } else { drainArea = drainVal; }
                    if (drainArea > maxDrainage) {
                        maxDrainage = drainArea;
                        exLink = chLink;
                        exWsno = wsno;
                        exPoint = pt;
                        exElev = elev;
                    }
                }
                if (exLink < 0) {
                    // should be at least one internal channel.  Select an arbitrary one, since linked anyway.
                    exLink = -1;
                    foreach (var chLink in this.lakesData[lakeId].lakeChLinks) {
                        exLink = chLink;
                        break;
                    }
                    if (exLink < 0) {
                        Utils.error(string.Format("Failed to make outlet for lake {0}", lakeId), gv.isBatch, reportErrors: reportErrors);
                        continue;
                    } else {
                        // remove chLink from internal links
                        this.lakesData[lakeId].lakeChLinks.Remove(exLink);
                        // remove from map of internal channels
                        this.chLinkInsideLake.Remove(exLink);
                        // Add as an exit
                        // first find the channel
                        channel = Utils.getOgrFeatureByValue(channelsLayer, Topology._LINKNO, exLink);
                        var reachData = await this.getOgrReachData(channel, demLayer);
                        if (reachData is null) {
                            Utils.loginfo(string.Format("No reach data for channel link {0}", exLink));
                        } else {
                            exWsno = channel.GetFieldAsInteger(channelWSNOIndex);
                            drainage = streamDrainageIndex < 0 ? this.drainAreas[exLink] : channel.GetFieldAsDouble(streamDrainageIndex);
                            exPoint = new double[3] { reachData.upperX, reachData.upperY, 0 };
                            exElev = reachData.upperZ;
                            if (Utils.areClose(exElev, gv.elevationNoData, rel_tol: 1E-06)) {
                                exElev = 0;
                            }
                            data[exLink] = (exWsno, drainage, exPoint, exElev);
                        }
                    }
                }
                var others = data.Keys.ToList();
                others.Remove(exLink);
                if (others.Count == 0) {
                    var msg = string.Format(@"Warning: Stream link {0} chosen as main outlet for all of lake {1}.  
                        Other possible outlet stream links are {2}.", exLink, lakeId, (from link in others
                                                                                       select Convert.ToInt32(link)).ToList().ToString());
                    if (multipleOutletsInteractive) {
                        Utils.information(msg, gv.isBatch, reportErrors: reportErrors);
                    } else {
                        Utils.loginfo(msg);
                    }
                }
                this.chLinkFromLake[exLink] = lakeId;
                this.lakesData[lakeId].outChLink = exLink;
                foreach (var chLink in others) {
                    this.chLinkFromLake[chLink] = lakeId;
                    this.lakesData[lakeId].otherOutChLinks.Add(chLink);
                }
                this.pointId += 1;
                this.lakesData[lakeId].outPoint = (exWsno, this.pointId, exPoint, exElev);
            }
            foreach (var (lakeId, totalElev) in totalElevation) {
                var numInLinks = this.lakesData[lakeId].inChLinks.Count;
                if (numInLinks > 0) {
                    this.lakesData[lakeId].elevation = totalElev / numInLinks;
                } else {
                    this.lakesData[lakeId].elevation = this.lakesData[lakeId].outPoint.Item4;
                }
            }
            return this.lakesData.Count;
        }

        // Add playas to grid model.
        public virtual void addGridPlayas(OSGeo.OGR.Layer lakesLayer, RasterLayer demLayer, GlobalVars gv) {
            var lakeIdIndex = this.getIndex(lakesLayer, Topology._LAKEID);
            var lakeResIndex = this.getIndex(lakesLayer, Topology._RES);
            if (lakeResIndex < 0) {
                // cannot be anything but reservoirs, so nothing to do
                return;
            }
            var lakeAreaIndex = this.getIndex(lakesLayer, Parameters._AREA, ignoreMissing: true);
            var areaFactor = gv.horizontalFactor * gv.horizontalFactor;
            lakesLayer.ResetReading();
            OSGeo.OGR.Feature lake;
            do {
                lake = lakesLayer.GetNextFeature();
                if (lake is null) { break; }
                var waterRole = (WaterRole)lake.GetFieldAsInteger(lakeResIndex);
                if (waterRole != WaterRole._PLAYA) {
                    continue;
                }
                var lakeGeom = lake.GetGeometryRef();
                var lakeId = lake.GetFieldAsInteger(lakeIdIndex);
                double[] lakeCentroid = new double[3];
                lakeGeom.Centroid().GetPoint(0, lakeCentroid);
                var lakeArea = lakeGeom.Area() * areaFactor;
                var lakeOverrideArea = lakeArea;
                if (lakeAreaIndex >= 0) {
                    try {
                        lakeOverrideArea = lake.GetFieldAsDouble(lakeAreaIndex) * 10000.0;
                    } catch {
                    }
                }
                var lakeData = new LakeData(lakeArea, lakeOverrideArea, lakeCentroid, waterRole);
                lakeData.elevation = Topology.valueAtPoint(lakeCentroid, demLayer.GetRaster());
                this.lakesData[lakeId] = lakeData;
            } while (lake is not null);
        }

        // Add lakes data to existing non-grid model.
        //         
        //         We ignore DsNodeIds for inflowing and outflowing channels since these were
        //         probably only added previously to the snapped inlets/outlets file
        //         and inlets/outlets are little use in any case with existing watersheds.
        public async Task<bool> addExistingLakes(
            OSGeo.OGR.Layer lakesLayer,
            OSGeo.OGR.Layer channelsLayer,
            RasterLayer demLayer,
            GlobalVars gv,
            bool reportErrors = true) {
            int subbasin;
            double[] point;
            double elev;
            LakeData data;
            WaterRole waterRole;
            var lakeIdIndex = this.getIndex(lakesLayer, Topology._LAKEID);
            var lakeResIndex = this.getIndex(lakesLayer, Topology._RES, ignoreMissing: true);
            if (lakeResIndex < 0) {
                Utils.information(string.Format("No RES field in lakes shapefile {0}: assuming lakes are reservoirs", gv.lakeFile), this.isBatch);
            }
            var lakeAreaIndex = this.getIndex(lakesLayer, Parameters._AREA, ignoreMissing: true);
            var channelLinkIndex = this.getIndex(channelsLayer, Topology._LINKNO);
            var DSLINKNO = this.isHUC ? Topology._DSLINKNO1 : Topology._DSLINKNO;
            var channelDsLinkIndex = this.getIndex(channelsLayer, DSLINKNO);
            var channelBasinIndex = this.getIndex(channelsLayer, Topology._BASINNO);
            var channelLakeInIndex = this.getIndex(channelsLayer, Topology._LAKEIN);
            var channelLakeOutIndex = this.getIndex(channelsLayer, Topology._LAKEOUT);
            var channelLakeWithinIndex = this.getIndex(channelsLayer, Topology._LAKEWITHIN);
            var channelLakeMainIndex = this.getIndex(channelsLayer, Topology._LAKEMAIN);
            if (lakeIdIndex < 0 || channelLinkIndex < 0 || channelDsLinkIndex < 0 || channelBasinIndex < 0 || channelLakeInIndex < 0 || channelLakeOutIndex < 0 || channelLakeWithinIndex < 0 || channelLakeMainIndex < 0) {
                return false;
            }
            this.lakesData = new Dictionary<int, LakeData>();
            var areaFactor = gv.horizontalFactor * gv.horizontalFactor;
            lakesLayer.ResetReading();
            OSGeo.OGR.Feature lake;
            do {
                lake = lakesLayer.GetNextFeature();
                if (lake is null) { break; }
                var lakeId = lake.GetFieldAsInteger(lakeIdIndex);
                if (lakeResIndex < 0) {
                    waterRole = WaterRole._RESERVOIR;
                } else {
                    waterRole = (WaterRole)lake.GetFieldAsInteger(lakeResIndex);
                }
                if (this.lakesData.ContainsKey(lakeId)) {
                    Utils.error(string.Format("Lake identifier {0} occurs twice in {1}.  Lakes not added.", lakeId, gv.lakeFile), gv.isBatch, reportErrors: reportErrors);
                    this.lakesData.Clear(); ;
                    return false;
                }
                // to stop reuse of the same water body id
                this.waterBodyId = Math.Max(this.waterBodyId, lakeId);
                var geom = lake.GetGeometryRef();
                var area = geom.Area() * areaFactor;
                var centroid = new double[3];
                geom.Centroid().GetPoint(0, centroid);
                var overrideArea = area;
                if (lakeAreaIndex >= 0) {
                    try {
                        overrideArea = lake.GetFieldAsDouble(lakeAreaIndex) * 10000.0;
                    } catch {
                    }
                }
                this.lakesData[lakeId] = new LakeData(area, overrideArea, centroid, waterRole);
            } while (lake is not null);
            this.chLinkIntoLake = new Dictionary<int, int>();
            this.chLinkInsideLake = new Dictionary<int, int>();
            this.chLinkFromLake = new Dictionary<int, int>();
            channelsLayer.ResetReading();
            OSGeo.OGR.Feature channel;
            do {
                channel = channelsLayer.GetNextFeature();
                if (channel is null) { break; }
                var chLink = channel.GetFieldAsInteger(channelLinkIndex);
                var dsLink = channel.GetFieldAsInteger(channelDsLinkIndex);
                var lakeIn = channel.GetFieldAsInteger(channelLakeInIndex);
                var lakeOut = channel.GetFieldAsInteger(channelLakeOutIndex);
                var lakeWithin = channel.GetFieldAsInteger(channelLakeWithinIndex);
                var lakeMain = channel.GetFieldAsInteger(channelLakeMainIndex);
                ReachData reachData = null;
                if (lakeIn > 0) {
                    if (!this.lakesData.TryGetValue(lakeIn, out data)) {
                        Utils.error(string.Format("Channel with LINKNO {0} flows into lake {1} not defined in {2}.  Lakes not added.", chLink, lakeIn, gv.lakeFile), gv.isBatch, reportErrors: reportErrors);
                        this.lakesData.Clear();
                        return false;
                    }
                    if (data.waterRole == WaterRole._PLAYA) {
                        Utils.information(string.Format("Channel with LINKNO {0} flows into {1} lake {2}.  This is ignored.", chLink, Topology.lakeCategory(data.waterRole), lakeIn), gv.isBatch, reportErrors: reportErrors);
                    } else {
                        reachData = await this.getOgrReachData(channel, demLayer);
                        Debug.Assert(reachData is not null);
                        elev = reachData.lowerZ;
                        data.elevation += elev;
                        // may already have point for channel outlet
                        int pointId;
                        if (!this.chOutlets.ContainsKey(chLink)) {
                            // need to Add a point
                            point = new double[3] { reachData.lowerX, reachData.lowerY, 0 };
                            this.pointId += 1;
                            pointId = this.pointId;
                        } else {
                            (pointId, point) = this.chOutlets[chLink];
                        }
                        data.inChLinks[chLink] = (pointId, point, elev);
                        this.chLinkIntoLake[chLink] = lakeIn;
                    }
                } else if (lakeWithin > 0) {
                    if (!this.lakesData.TryGetValue(lakeWithin, out data)) {
                        Utils.error(string.Format("Channel with LINKNO {0} inside lake {1} not defined in {2}.  Lakes not added.", chLink, lakeWithin, gv.lakeFile), gv.isBatch, reportErrors: reportErrors);
                        this.lakesData.Clear();
                        return false;
                    }
                    if (data.waterRole == WaterRole._PLAYA) {
                        Utils.information(string.Format("Channel with LINKNO {0} inside {1} lake {2}.  This is ignored.", chLink, Topology.lakeCategory(data.waterRole), lakeIn), gv.isBatch, reportErrors: reportErrors);
                    } else {
                        data.lakeChLinks.Add(chLink);
                        this.chLinkInsideLake[chLink] = lakeWithin;
                        if (dsLink < 0) {
                            // watershed outlet
                            reachData = await this.getOgrReachData(channel, demLayer);
                            Debug.Assert(reachData is not null);
                            subbasin = channel.GetFieldAsInteger(channelBasinIndex);
                            data.outChLink = -1;
                            point = new double[3] { reachData.lowerX, reachData.lowerY, 0 };
                            elev = reachData.lowerZ;
                            this.pointId += 1;
                            data.outPoint = (subbasin, this.pointId, point, elev);
                            this.outletsInLake[subbasin] = lakeWithin;
                        }
                    }
                }
                if (lakeOut > 0) {
                    if (!this.lakesData.TryGetValue(lakeOut, out data)) {
                        Utils.error(string.Format("Channel with LINKNO {0} flows out of lake {1} not defined in {2}.  Lakes not added.", chLink, lakeOut, gv.lakeFile), gv.isBatch, reportErrors: reportErrors);
                        this.lakesData.Clear();
                        return false;
                    }
                    if (data.waterRole == WaterRole._PLAYA) {
                        Utils.information(string.Format("Channel with LINKNO {0} flows out of {1} lake {2}.  This is ignored.", chLink, Topology.lakeCategory(data.waterRole), lakeIn), gv.isBatch, reportErrors: reportErrors);
                    } else if (lakeMain == lakeOut) {
                        // lake's main outlet
                        // channel leaves lake at upper end
                        reachData = await this.getOgrReachData(channel, demLayer);
                        Debug.Assert(reachData is not null);
                        subbasin = channel.GetFieldAsInteger(channelBasinIndex);
                        data.outChLink = chLink;
                        elev = reachData.upperZ;
                        this.pointId += 1;
                        point = new double[3] { reachData.upperX, reachData.upperY, 0 };
                        Utils.loginfo(string.Format("Added point {0} for channel link {1} flowing from lake {2}", this.pointId, chLink, lakeOut));
                        data.outPoint = (subbasin, this.pointId, point, elev);
                        this.chLinkFromLake[chLink] = lakeOut;
                    } else {
                        // other outlet
                        data.otherOutChLinks.Add(chLink);
                    }
                }
            } while (channel is not null);
            // define lake elevation
            foreach (var datal in this.lakesData.Values) {
                if (datal.waterRole == WaterRole._PLAYA) {
                    datal.elevation = Topology.valueAtPoint(datal.centroid, demLayer.GetRaster());
                } else {
                    var numInflows = datal.inChLinks.Count;
                    datal.elevation = numInflows == 0 ? datal.outPoint.Item4 : datal.elevation / numInflows;
                }
            }
            return true;
        }

        public virtual bool isLakeInletOrOutlet(int chLink) {
            foreach (var lakeData in this.lakesData.Values) {
                if (lakeData.waterRole == WaterRole._PLAYA) {
                    continue;
                }
                if (lakeData.inChLinks.ContainsKey(chLink) || chLink == lakeData.outChLink || lakeData.otherOutChLinks.Contains(chLink)) {
                    return true;
                }
            }
            return false;
        }

        // Returns true if any part of geom intersects any part of polyGeom, or geom is within polyGeom. polyGeom has associated rectangle polyRect.

        public static bool intersectsPoly(OSGeo.OGR.Geometry geom, OSGeo.OGR.Geometry polyGeom, OSGeo.OGR.Envelope polyRect) {
            var geoRect = new OSGeo.OGR.Envelope();
            geom.GetEnvelope(geoRect);
            if (Topology.disjointBoxes(geoRect, polyRect)) {
                return false;
            } else {
                return !geom.Disjoint(polyGeom);
            }
        }

        // Return True if the boxes are disjoint.

        public static bool disjointBoxes(OSGeo.OGR.Envelope box1, OSGeo.OGR.Envelope box2) {
            return box1.MinX > box2.MaxX || box1.MaxX < box2.MinX || box1.MinY > box2.MaxY || box1.MaxY < box2.MinY;
        }

        // Return true if point within polyGeom, which has associated rectangle polyRect.

        public static bool polyContains(double[] point, OSGeo.OGR.Geometry polyGeom, OSGeo.OGR.Envelope polyRect) {

            if (polyRect.MinX < point[0] && point[0] < polyRect.MaxX && (polyRect.MinY < point[1] && point[1] < polyRect.MaxY)) {
                var geom = new OSGeo.OGR.Geometry(OSGeo.OGR.wkbGeometryType.wkbPoint);
                geom.AddPoint(point[0], point[1], 0);
                return polyGeom.Contains(geom);
            } else {
                return false;
            }
        }

        // Save lakes data in project database.
        public virtual void saveLakesData(DBUtils db) {
            double x, y;
            var lakesTable = "LAKESDATA";
            var clearSQL = "DROP TABLE IF EXISTS " + lakesTable;
            db.execNonQuery(clearSQL);
            db.execNonQuery(DBUtils._CREATELAKESDATA);
            var linksTable = "LAKELINKS";
            clearSQL = "DROP TABLE IF EXISTS " + linksTable;
            db.execNonQuery(clearSQL);
            db.execNonQuery(DBUtils._CREATELAKELINKS);
            var basinsTable = "LAKEBASINS";
            clearSQL = "DROP TABLE IF EXISTS " + basinsTable;
            db.execNonQuery(clearSQL);
            db.execNonQuery(DBUtils._CREATELAKEBASINS);
            foreach (var (lakeId, lakeData) in this.lakesData) {
                var point = lakeData.outPoint.Item3;
                if (point is null) {
                    if (new HashSet<WaterRole> {
                            WaterRole._RESERVOIR,
                            WaterRole._POND,
                            WaterRole._WETLAND
                        }.Contains(lakeData.waterRole)) {
                        Utils.loginfo(string.Format("No outpoint for lake {0}", lakeId));
                    }
                    x = 0;
                    y = 0;
                } else {
                    x = lakeData.outPoint.Item3[0];
                    y = lakeData.outPoint.Item3[1];
                }
                db.execNonQuery(DBUtils._INSERTLAKESDATA, new object[] { lakeId, lakeData.outPoint.Item1, lakeData.waterRole, lakeData.area, lakeData.overrideArea, lakeData.elevation, lakeData.outChLink, lakeData.outPoint.Item2, x, y, lakeData.outPoint.Item4, lakeData.centroid[0], lakeData.centroid[1] });
                // Utils.loginfo(str(lakeData.inChLinks.keys()))
                // Utils.loginfo(str(lakeData.lakeChLinks))
                foreach (var (chLink, (pointId, pt, elev)) in lakeData.inChLinks) {
                    try {
                        db.execNonQuery(DBUtils._INSERTLAKELINKS, new object[] { chLink, lakeId, true, false, pointId, pt[0], pt[1], elev });
                    } catch {
                        Utils.error(string.Format("Failed to Add in channel link {0}", chLink), this.isBatch);
                    }
                }
                foreach (var chLink in lakeData.lakeChLinks) {
                    try {
                        db.execNonQuery(DBUtils._INSERTLAKELINKS, new object[] { chLink, lakeId, false, true, null, null, null, null });
                    } catch {
                        Utils.error(string.Format("Failed to Add inside channel link {0}", chLink), this.isBatch);
                    }
                }
                foreach (var chLink in lakeData.otherOutChLinks) {
                    try {
                        db.execNonQuery(DBUtils._INSERTLAKELINKS, new object[] { chLink, lakeId, false, false, null, null, null, null });
                    } catch {
                        Utils.error(string.Format("Failed to Add other out channel link {0}", chLink), this.isBatch);
                    }
                }
            }
            foreach (var (subbasin, lakeId) in this.outletsInLake) {
                db.execNonQuery(DBUtils._INSERTLAKEBASINS, new object[] { subbasin, lakeId });
            }
            //db.hashDbTable(conn, lakesTable);
            //db.hashDbTable(conn, linksTable);
            //db.hashDbTable(conn, basinsTable);
        }

        // Read lakes data from project database.  Return true if data read OK, false if no data or error.
        public virtual bool readLakesData(DBUtils db) {

            this.lakesData.Clear();
            this.chLinkIntoLake.Clear();
            this.chLinkInsideLake.Clear();
            this.chLinkFromLake.Clear();
            this.outletsInLake.Clear();
            var lakesTable = "LAKESDATA";
            var linksTable = "LAKELINKS";
            var basinsTable = "LAKEBASINS";
            var lakeSql = DBUtils.sqlSelect(lakesTable, "*", "", "");
            var linksSql = DBUtils.sqlSelect(linksTable, "*", "", "lakeid=@p1");
            var basinsSql = DBUtils.sqlSelect(basinsTable, "*", "", "");
            try {
                using (var reader = db.getReader(lakeSql)) {
                    while (reader.Read()) {
                        int lakeId = (int)(reader["id"]);
                        this.waterBodyId = Math.Max(this.waterBodyId, lakeId);
                        this.lakesData[lakeId] = new LakeData((double)reader["area"], (double)reader["overridearea"], new double[3] { (double)reader["centroidx"], (double)reader["centroidy"], 0 }, (WaterRole)reader["role"]);
                        int outChLink = (int)reader["outlink"];
                        this.lakesData[lakeId].outChLink = outChLink;
                        this.chLinkFromLake[outChLink] = lakeId;
                        this.lakesData[lakeId].outPoint = ((int)reader["subbasin"], (int)reader["outletid"], new double[3] { (double)reader["outletx"], (double)reader["outlety"], 0 }, (double)reader["outletelev"]);
                        this.lakesData[lakeId].elevation = (double)reader["meanelev"];
                        using (var linksReader = db.getReader(linksSql, new object[] { lakeId })) {
                            while (linksReader.Read()) {
                                int chLink = Convert.ToInt32(linksReader["linkno"]);
                                if ((bool)(linksReader["inside"])) {
                                    this.lakesData[lakeId].lakeChLinks.Add(chLink);
                                    this.chLinkInsideLake[chLink] = lakeId;
                                } else if ((bool)linksReader["inlet"]) {
                                    this.lakesData[lakeId].inChLinks[chLink] = ((int)linksReader["inletid"], new double[3] { (double)linksReader["inletx"], (double)linksReader["inlety"], 0 }, (double)linksReader["inletelev"]);
                                    this.chLinkIntoLake[chLink] = lakeId;
                                } else {
                                    this.lakesData[lakeId].otherOutChLinks.Add(chLink);
                                    this.chLinkFromLake[chLink] = lakeId;
                                }
                            }
                        }
                    }
                }
                using (var basinsReader = db.getReader(basinsSql)) {
                    while (basinsReader.Read()) {
                        this.outletsInLake[(int)basinsReader["subbasin"]] = (int)basinsReader["lakeid"];
                    }
                }
                return this.lakesData.Count > 0;
            } catch (Exception ex) {
                Utils.loginfo(string.Format("Reading lakes data failed: {0}", ex.Message));
                return false;
            }
        }

        // Get downstream channel, skipping zero-length channels.
        //         
        //         Returns -1 if the channel flows into a lake.
        public virtual int getDownChannel(int channel) {
            if (this.chLinkIntoLake.ContainsKey(channel)) {
                return -1;
            }
            while (true) {
                var dsChannel = this.downChannels[channel];
                if (this.zeroChannels.Contains(dsChannel)) {
                    channel = dsChannel;
                } else {
                    return dsChannel;
                }
            }
        }

        // 
        //         Define map chBasinAreas from channel basin to basin area in sq m.
        //         
        //         Done by counting pixels in the wChannel file (as an alternative to creating a shapefile from it).
        //         Not used with grid models.
        //         
        public virtual void setChannelBasinAreas(GlobalVars gv) {
            this.chBasinAreas.Clear();
            var unitArea = this.dx * this.dy;
            var ds = Gdal.Open(gv.channelBasinFile, Access.GA_ReadOnly);
            var band = ds.GetRasterBand(1);
            int numRows = ds.RasterYSize;
            int numCols = ds.RasterXSize;
            double noData;
            int hasNoData;
            band.GetNoDataValue(out noData, out hasNoData);
            var buffer = new int[numCols];
            for (int row = 0; row < numRows; row++) {
                band.ReadRaster(0, row, numCols, 1, buffer, numCols, 1, 0, 0);
                for (int col = 0; col < numCols; col++) {
                    int val = buffer[col];
                    if (val == noData) { continue; }
                    if (this.chBasinAreas.ContainsKey(val)) {
                        this.chBasinAreas[val] += unitArea;
                    } else {
                        this.chBasinAreas[val] = unitArea;
                    }
                }
            }
        }

        // Return copy of areas.
        public static Dictionary<int, double> copyBasinAreas(Dictionary<int, double> areas) {
            var areas2 = new Dictionary<int, double>();
            foreach (var (i, val) in areas) {
                areas2[i] = val;
            }
            return areas2;
        }

        // 
        //         Check total channel basin areas in each subbasin tally with subbasin areas, 
        //         and total watershed areaa for both tally, i.e. are the same within area of one DEM pixel.
        //         
        //         This is only done in testing ('test' in project name) and is mostly a check that
        //         channels are correctly assigned to subbasins.
        //         Not used with grid models (since channel-subbasin is one-one for grid models).
        //         
        public virtual bool checkAreas(OSGeo.OGR.Layer subbasinsLayer, GlobalVars gv) {
            double basinArea;
            // TODO:  make work with lakes
            if (gv.projName.Contains("test")) {
                var unitArea = this.dx * this.dy;
                var polyIndex = this.getIndex(subbasinsLayer, Topology._POLYGONID);
                if (polyIndex < 0) {
                    return false;
                }
                var areaIndex = this.getIndex(subbasinsLayer, Parameters._AREA, ignoreMissing: true);
                var totalBasinsArea = 0.0;
                var totalChannelBasinsArea = 0.0;
                // one percent test: using 1 pixel test instead
                //             def compare(x, y): # return true if both zero or difference < 1% of x
                //                 if x == 0:
                //                     return y == 0
                //                 else:
                //                     return Math.Abs(x - y) < 0.01 * x
                var areaFactor = gv.horizontalFactor * gv.horizontalFactor;
                subbasinsLayer.ResetReading();
                OSGeo.OGR.Feature poly;
                do {
                    poly = subbasinsLayer.GetNextFeature();
                    if (poly is null) { break; }
                    if (areaIndex < 0) {
                        basinArea = poly.GetGeometryRef().Area() * areaFactor;
                    } else {
                        basinArea = poly.GetFieldAsDouble(areaIndex) * 10000.0;
                    }
                    // need to count areas of basins upstream from inlets because comparison for whole watershed
                    // by using all channels will not exclude them
                    totalBasinsArea += basinArea;
                    var basin = poly.GetFieldAsInteger(polyIndex);
                    var chBasins = new HashSet<int>();
                    var chLinks = new HashSet<int>();
                    foreach (var (chLink, chBasin) in this.chLinkToChBasin) {
                        int nextBasin;
                        if (this.chBasinToSubbasin.TryGetValue(chBasin, out nextBasin) && nextBasin == basin) {
                            chBasins.Add(chBasin);
                            chLinks.Add(chLink);
                        }
                    }
                    var area = 0.0;
                    foreach (var (chBasin, chArea) in this.chBasinAreas) {
                        if (chBasins.Contains(chBasin)) {
                            area += chArea;
                        }
                    }
                    if (Math.Abs(basinArea - area) >= unitArea) {
                        // not using compare(basinArea, area):
                        var SWATChannels = (from chLink in chLinks
                                            select this.channelToSWATChannel[chLink]).ToHashSet();
                        var SWATBasin = this.subbasinToSWATBasin[basin];
                        Utils.error(string.Format("Basin {0} with area {1} has channels {2} with total area {3}", SWATBasin, basinArea, SWATChannels, area), gv.isBatch);
                        // return true so run can continue if user regards error as small
                        return true;
                    }
                } while (poly is not null);
                // now compare areas for whole watershed
                foreach (var (_, chArea) in this.chBasinAreas) {
                    totalChannelBasinsArea += chArea;
                }
                if (Math.Abs(totalBasinsArea - totalChannelBasinsArea) >= unitArea) {
                    // not using compare(totalBasinsArea, totalChannelBasinsArea):
                    Utils.error(string.Format("Watershed area is {0} by adding subbasin areas and {1} by adding channel basin areas", totalBasinsArea, totalChannelBasinsArea), gv.isBatch);
                    // return true so run can continue if user regards error as small
                    return true;
                }
                Utils.loginfo(string.Format("Total watershed area is {0}", totalBasinsArea));
            }
            return true;
        }

        // Return true if chLink is in chLinks or reachable from an item in chLinks via the one-many relation us.
        public static bool reachable(int chLink, List<int> chLinks, Dictionary<int, List<int>> us) {
            if (chLinks.Contains(chLink)) {
                return true;
            }
            foreach (var nxt in chLinks) {
                List<int> temp;
                if (us.TryGetValue(nxt, out temp)) {
                    if (Topology.reachable(chLink, temp, us)) {
                        return true;
                    }
                }
            }
            return false;
        }

        //===========================================================================
        // def addUpstreamLinks(self, link, us):
        //     """Add to upstreamFromInlets the links upstream from link."""
        //     ups = us.TryGetValue(link, None)
        //     if ups is not None:
        //         for up in ups:
        //             self.upstreamFromInlets.Add(up)
        //             self.addUpstreamLinks(up, us)
        //===========================================================================
        // Get drain areas from channelLayer file's DS_Cont_Ar attribute.
        public virtual void setDrainageFromChannels(OSGeo.OGR.Layer channelLayer, int drainAreaIndex) {
            var areaFactor = this.horizontalFactor * this.horizontalFactor;
            channelLayer.ResetReading();
            OSGeo.OGR.Feature reach;
            do {
                reach = channelLayer.GetNextFeature();
                if (reach is null) { break; }
                var channelLink = reach.GetFieldAsInteger(this.channelIndex);
                this.drainAreas[channelLink] = reach.GetFieldAsDouble(drainAreaIndex) * areaFactor;
            } while (reach is not null);
        }

        // Get drain areas from channelLayer file's Drainage attribute.  Return True if successful.
        public virtual bool setGridDrainageFromChannels(OSGeo.OGR.Layer channelLayer, OSGeo.OGR.Layer subbasinsLayer, int maxChLink, Dictionary<int, List<int>> us) {
            var channelDefn = channelLayer.GetLayerDefn();
            var channelIndex = channelDefn.GetFieldIndex(Topology._LINKNO);
            var drainageIndex = channelDefn.GetFieldIndex(Topology._DRAINAGE);
            if (channelIndex < 0 || drainageIndex < 0) {
                return false;
            }
            channelLayer.ResetReading();
            OSGeo.OGR.Feature reach;
            do {
                reach = channelLayer.GetNextFeature();
                if (reach is null) { break; }
                var channel = reach.GetFieldAsInteger(channelIndex);
                this.drainAreas[channel] = reach.GetFieldAsDouble(drainageIndex) * 1000000.0;
            } while (reach is not null);
            this.checkDrainage(channelLayer, subbasinsLayer, maxChLink, us, false);
            return true;
        }

        // Calculate and save grid drain areas in sq km.
        public virtual void setGridDrainageAreas(
            OSGeo.OGR.Layer channelLayer,
            OSGeo.OGR.Layer subbasinsLayer,
            int maxChLink,
            Dictionary<int, List<int>> us,
            double gridCellArea) {
            // check to see if drain areas already calculated - may have been done with lakes
            if (this.drainAreas is null) {
                return;
            }
            foreach (var link in this.downChannels.Keys) {
                if (this.drainAreas[link] > gridCellArea) {
                    return;
                }
            }
            foreach (var link in this.downChannels.Keys) {
                this.drainAreas[link] = gridCellArea;
            }
            this.checkDrainage(channelLayer, subbasinsLayer, maxChLink, us, true);
        }

        // Check drainage as defined by downChannels map is not circular: attempt to fix if so.  Also calculate drainage in 
        //         self.drainAreas if needDrainage is true.  If so, assume it is initialised with area of grid cell.
        //         
        //         Only used in grid models.
        //         Changes the channel and the grid shapefiles, plus the downChannels and downSubbasins mappings, plus the upchannel map us if not None.
        public virtual void checkDrainage(
            OSGeo.OGR.Layer channelLayer,
            OSGeo.OGR.Layer subbasinsLayer,
            int maxChLink,
            Dictionary<int, List<int>> us,
            bool needDrainage) {
            LakeData lakeData;
            // number of incoming links for each link
            int[] incount = new int[maxChLink + 1];
            for (int i = 0; i <= maxChLink; i++) {
                incount[i] = 0;
            }
            foreach (var dsLink in this.downChannels.Values) {
                if (dsLink >= 0) {
                    incount[dsLink] += 1;
                }
            }
            // queue contains all links whose drainage areas have been calculated 
            // i.e. will not increase and can be propagated
            var queue = (from link in Enumerable.Range(0, maxChLink + 1)
                         where incount[link] == 0
                         select link).ToList();
            while (queue.Count > 0) {
                var link = queue[0];
                queue.RemoveAt(0);
                int dsLink;
                if (this.downChannels.TryGetValue(link, out dsLink)) {
                    if (needDrainage) {
                        this.drainAreas[dsLink] += this.drainAreas[link];
                    }
                    incount[dsLink] -= 1;
                    if (incount[dsLink] == 0) {
                        queue.Add(dsLink);
                    }
                }
            }
            // incount values should now all be zero
            var remainder = (from link in Enumerable.Range(0, maxChLink + 1)
                             where incount[link] > 0
                             select link).ToList();
            if (remainder.Count > 0) {
                Utils.loginfo(string.Format("Drainage areas incomplete.  There is a circularity in links {0}", remainder));
                // remainder may contain a number of circles.
                var rings = new List<List<int>>();
                var nextRing = new List<int>();
                var link = remainder[0];
                remainder.RemoveAt(0);
                while (true) {
                    nextRing.Add(link);
                    var dsLink = this.downChannels[link];
                    if (nextRing.Contains(dsLink)) {
                        // complete the ring
                        nextRing.Add(dsLink);
                        rings.Add(nextRing);
                        if (remainder.Count > 0) {
                            nextRing = new List<int>();
                            link = remainder[0];
                            remainder.RemoveAt(0);
                        } else {
                            break;
                        }
                    } else {
                        // continue
                        remainder.Remove(dsLink);
                        link = dsLink;
                    }
                }
                var numRings = rings.Count;
                if (numRings > 0) {
                    var channelDefn = channelLayer.GetLayerDefn();
                    var linkIndex = channelDefn.GetFieldIndex(Topology._LINKNO);
                    var DSLINKNO = this.isHUC ? Topology._DSLINKNO1 : Topology._DSLINKNO;
                    var dsLinkIndex = channelDefn.GetFieldIndex(DSLINKNO);
                    var subDefn = subbasinsLayer.GetLayerDefn();
                    var dsPolyIndex = subDefn.GetFieldIndex(Topology._DOWNID);
                    Utils.information(string.Format("Drainage areas incomplete.  There are {0} circularities.  Will try to remove them.  See the ArcSWAT+ log for details", numRings), this.isBatch);
                    foreach (var ring in rings) {
                        Utils.loginfo(string.Format("Circularity in links {0}", ring));
                        // fix the circularity by making the largest drainage link an exit in the downChannels map
                        var maxDrainage = 0.0;
                        var maxLink = -1;
                        foreach (var lnk in ring) {
                            var drainage = this.drainAreas[lnk];
                            if (drainage > maxDrainage) {
                                maxLink = lnk;
                                maxDrainage = drainage;
                            }
                        }
                        if (maxLink < 0) {
                            Utils.error(string.Format("Failed to find link with largest drainage in circle {0}", ring), this.isBatch);
                        } else {
                            this.downChannels[maxLink] = -1;
                            OSGeo.OGR.Feature feature = Utils.getOgrFeatureByValue(channelLayer, Topology._LINKNO, maxLink);
                            feature.SetField(dsLinkIndex, -1);
                            channelLayer.SetFeature(feature);
                            if (us is not null) {
                                link = feature.GetFieldAsInteger(linkIndex);
                                var dsLink = feature.GetFieldAsInteger(dsLinkIndex);
                                us[dsLink].Remove(link);
                            }
                            // also need to fix downSubbasins map
                            // subbasin same as chBasin since grid model
                            var basin = this.chLinkToChBasin[maxLink];
                            this.downSubbasins[basin] = -1;
                            feature = Utils.getOgrFeatureByValue(subbasinsLayer, Topology._POLYGONID, basin);
                            feature.SetField(dsPolyIndex, -1);
                            Utils.loginfo(string.Format("Link {0} and polygon {1} made into an exit", maxLink, basin));
                            // if there is a lake with the exit link internal to it we should make it the lake outlet
                            int lakeId;
                            if (this.chLinkInsideLake.TryGetValue(maxLink, out lakeId)) {
                                lakeData = this.lakesData[lakeId];
                                if (lakeData.outChLink >= 0) {
                                    // Add old outlet to other outlets
                                    var oldOutLink = lakeData.outChLink;
                                    lakeData.otherOutChLinks.Add(oldOutLink);
                                    // change maxLink from internal to outlet
                                    lakeData.lakeChLinks.Remove(maxLink);
                                    lakeData.outChLink = maxLink;
                                    Utils.loginfo(string.Format("Outlet of lake {0} changed from link {1} to link {2}", lakeId, oldOutLink, maxLink));
                                    this.chLinkFromLake[maxLink] = lakeId;
                                    this.chLinkInsideLake.Remove(maxLink);
                                }
                            }
                            // and if the channel flows into a lake we should remove it from the lake's inlets
                            if (this.chLinkIntoLake.TryGetValue(maxLink, out lakeId)) {
                                lakeData = this.lakesData[lakeId];
                                lakeData.inChLinks.Remove(maxLink);
                                this.chLinkIntoLake.Remove(maxLink);
                            }
                        }
                    }
                }
            }
        }

        // 
        //         Calculate and save drainAreas.
        //         
        //         Not used with grid models.
        //         
        public virtual void setDrainageAreas(Dictionary<int, List<int>> us) {
            foreach (var (chLink, chBasin) in this.chLinkToChBasin) {
                this.setLinkDrainageArea(chLink, chBasin, us);
            }
        }

        // 
        //         Calculate and save drainArea for chLink.
        //         
        //         Not used with grid models.
        //         
        public virtual void setLinkDrainageArea(int chLink, int chBasin, Dictionary<int, List<int>> us) {
            if (this.drainAreas[chLink] > 0) {
                // already done in calculating one further downstream
                return;
            }
            double ownArea;
            if (!this.chBasinAreas.TryGetValue(chBasin, out ownArea)) {
                ownArea = 0;
            }
            var upsArea = 0.0;
            List<int> ups;
            if (!us.TryGetValue(chLink, out ups)) {
                ups = new List<int>();
            }
            foreach (var up in ups) {
                this.setLinkDrainageArea(up, this.chLinkToChBasin[up], us);
                upsArea += this.drainAreas[up];
            }
            this.drainAreas[chLink] = ownArea + upsArea;
        }

        // Get Strahler order from channelLayer file's order attribute.
        public virtual void setStrahlerFromChannels(OSGeo.OGR.Layer channelLayer, int orderIndex) {
            channelLayer.ResetReading();
            OSGeo.OGR.Feature reach;
            do {
                reach = channelLayer.GetNextFeature();
                if (reach is null) { break; }
                var channelLink = reach.GetFieldAsInteger(this.channelIndex);
                this.strahler[channelLink] = reach.GetFieldAsInteger(orderIndex);
            } while (reach is not null);
        }

        // Set Strahler orders using upstream map from each outlet.
        //         
        //         Not used with grid models
        public virtual void setStrahler(Dictionary<int, List<int>> us) {

            int setStrahlerLink(int link) {
                List<int> ups;
                if (!us.TryGetValue(link, out ups)) {
                    ups = new List<int>();
                }
                if (ups.Count == 0) {
                    this.strahler[link] = 1;
                    return 1;
                }
                var orders = (from up in ups
                              select setStrahlerLink(up)).ToList();
                int omax = Enumerable.Max(orders);
                int count = (from o in orders
                             where o == omax
                             select o).ToList().Count;
                int order = count == 1 ? omax : omax + 1;
                this.strahler[link] = order;
                return order;
            }

            foreach (var (_, _, links) in this.outlets.Values) {
                foreach (var link in links) {
                    setStrahlerLink(link);
                }
            }
        }

        // Set Strahler orders for channels using upstream map us.
        //         
        //         Non-recursive as used in grid models.
        public virtual void setStrahlerFromGrid(Dictionary<int, List<int>> us) {
            // continue looping as long as an order is calculated
            var changed = true;
            while (changed) {
                changed = false;
                foreach (var link in this.downChannels.Keys) {
                    if (this.strahler[link] > 0) {
                        // already calculated
                        continue;
                    } else {
                        List<int> ups;
                        if (!us.TryGetValue(link, out ups)) {
                            ups = new List<int>();
                        }
                        if (ups.Count == 0) {
                            this.strahler[link] = 1;
                            changed = true;
                        } else {
                            var orders = (from up in ups
                                          select this.strahler[up]).ToList();
                            if (orders.Contains(0)) {
                                // at least one yet to be calculated
                                continue;
                            } else {
                                var omax = Enumerable.Max(orders);
                                var count = (from o in orders
                                             where o == omax
                                             select o).ToList().Count;
                                var order = count == 1 ? omax : omax + 1;
                                this.strahler[link] = order;
                                changed = true;
                            }
                        }
                    }
                }
            }
        }

        // Get distance in metres to join with otherBasin from outlet of basin.  Add to distancesToJoins if necessary.
        public virtual double getDistanceToJoin(int basin, int otherBasin) {
            var link = this.subbasinToStream[basin];
            var otherLink = this.subbasinToStream[otherBasin];
            Dictionary<int, double> distances;
            if (!this.distancesToJoins.TryGetValue(link, out distances)) {
                distances = new Dictionary<int, double>();
            }
            double distance;
            if (!distances.TryGetValue(otherLink, out distance)) {
                distance = this.distanceToJoin(link, otherLink);
                distances[otherLink] = distance;
                this.distancesToJoins[link] = distances;
            }
            return distance;
        }

        // 
        //         Return distance in metres from outlet of link start to point of confluence with
        //         flow from otherLink, or to Outlet if no confluence.
        //         
        public virtual double distanceToJoin(int start, int otherLink) {
            return (from link in this.pathFromJoin(start, otherLink)
                    select this.streamLengths[link]).ToList().Sum();
        }

        // 
        //         Return list of downstream links starting with confluence with downstream path from otherLink, 
        //         and finishing with link immediately downstream from start.
        //         
        //         If otherLink is immediately downstream from start, list will be [otherLink].
        //         If start and otherLink both flow immediately into x, list will be empty.
        //         If there is no confluence, list will be path from outlet to immediately downstream from start.
        //         
        public virtual List<int> pathFromJoin(int start, int otherLink) {
            var startPath = this.pathFromOutlet(start);
            var otherPath = this.pathFromOutlet(otherLink);
            return this.removeCommonPrefix(startPath, otherPath);
        }

        // List of links downstream of start, in upstream order.
        public virtual List<int> pathFromOutlet(int start) {
            List<int> result = new List<int>();
            int nxt = start;
            while (true) {
                if (!this.downStreams.TryGetValue(nxt, out nxt)) {
                    break;
                }
                List<int> r = new List<int> { nxt };
                r.AddRange(result);
                result = r;
            }
            return result;
        }

        // Remove from the beginning of path1 the longest sequence that starts path2.
        public virtual List<int> removeCommonPrefix(List<int> path1, List<int> path2) {
            var i = 0;
            while (i < path1.Count && i < path2.Count) {
                if (path1[i] == path2[i]) {
                    i += 1;
                } else {
                    break;
                }
            }
            return path1.Skip(i).ToList();
        }

        // 
        //         Add basinno field (if necessary) to channels shapefile and populate with values from wStreamFile.
        //         
        //         Not done with grid models.
        //         
        public void addBasinsToChannelFile(OSGeo.OGR.Layer channelLayer, string channelFile, string wStreamFile) {
            OSGeo.OGR.Geometry line = null; ;
            double length;
            var channelDefn = channelLayer.GetLayerDefn();
            var bsnIdx = this.getIndex(channelDefn, Topology._BASINNO, channelFile, ignoreMissing: true);
            if (bsnIdx < 0) {
                var field = new FieldDefn(Topology._BASINNO, OSGeo.OGR.FieldType.OFTInteger);
                channelLayer.CreateField(field, 1);
                bsnIdx = this.getIndex(channelDefn, Topology._BASINNO, channelFile);
            }
            using (var ds = Gdal.Open(wStreamFile, Access.GA_ReadOnly)) {
                Band band = ds.GetRasterBand(1);
                var transform = new double[6];
                ds.GetGeoTransform(transform);
                double noData;
                int hasData;
                band.GetNoDataValue(out noData, out hasData);
                var lenIdx = this.getIndex(channelDefn, Topology._LENGTH, channelFile, ignoreMissing: true);
                channelLayer.ResetReading();
                OSGeo.OGR.Feature feature;
                do {
                    feature = channelLayer.GetNextFeature();
                    if (feature is null) { break; }
                    // find a point well into the channel to ensure we are not just outside the basin
                    var geometry = feature.GetGeometryRef();
                    if (lenIdx < 0) {
                        length = geometry.Length() * this.horizontalFactor;
                    } else {
                        length = feature.GetFieldAsDouble(lenIdx) * this.horizontalFactor;
                    }
                    if (length <= 0) {
                        var basin = Topology._NOBASIN;
                    } else {
                        if (geometry.GetGeometryType() == wkbGeometryType.wkbLineString) {
                            line = geometry;
                        } else {
                            int count = geometry.GetGeometryCount();
                            for (int i = 0; i < count; i++) {
                                var linei = geometry.GetGeometryRef(i);
                                if (linei is null || linei.GetPointCount() == 0) { continue; }
                                line = linei;
                                break;
                            }
                        }
                        var numPoints = line.GetPointCount();
                        if (numPoints < 2) {
                            Utils.error(string.Format("Link in channel with id {0} has less than two points", feature.GetFID()), this.isBatch);
                            return;
                        }
                        int basin;
                        double[] point = new double[3];
                        line.GetPoint(numPoints / 2, point);
                        var wVal = Topology.valueAtPoint(point, transform, band);
                        if (double.IsNaN(wVal) || Utils.areClose(wVal, noData, rel_tol: 1E-06)) {
                            // outside watershed  
                            basin = -1;
                        } else {
                            basin = Convert.ToInt32(wVal);
                        }
                        feature.SetField(bsnIdx, basin);
                        channelLayer.SetFeature(feature);
                    }
                } while (feature is not null);
            }
        }

        // Write drainage csv file.
        public virtual void writeDrainageFile(string drainageFile) {
            if (File.Exists(drainageFile)) {
                File.Delete(drainageFile);
            }
            using (var writer = new StreamWriter(drainageFile)) {
                writer.WriteLine("PolygonId,DownId");
                foreach (var (subbasin, downSubbasin) in this.downSubbasins) {
                    writer.WriteLine(string.Format("{0},{1}", subbasin, downSubbasin));
                }
            }
        }

        //// 
        ////         Generate ReachData record for reach geometry.  demLayer may be none, or point may give nodata for elevation, in which case elevations are set zero.
        ////         
        //public virtual ReachData getReachData(Feature reach, RasterLayer demLayer) {
        //    object minElev;
        //    object maxElev;
        //    object pFinish;
        //    object pStart;
        //    if (this.isHUC || this.isHAWQS) {
        //        var wsno = reach.GetFieldAsInteger(this.wsnoIndex);
        //        pStart = this.chPointSources[wsno][1];
        //        pFinish = this.chOutlets[wsno][1];
        //    } else {
        //        var geom = reach.geometry();
        //        var firstLine = Topology.reachFirstLine(geom, this.xThreshold, this.yThreshold);
        //        if (firstLine is null || firstLine.Count < 1) {
        //            Utils.error("It looks like your stream shapefile does not obey the single direction rule, that all reaches are either upstream or downstream.", this.isBatch);
        //            return null;
        //        }
        //        var lastLine = Topology.reachLastLine(geom, this.xThreshold, this.yThreshold);
        //        if (lastLine is null || lastLine.Count < 1) {
        //            Utils.error("It looks like your stream shapefile does not obey the single direction rule, that all reaches are either upstream or downstream.", this.isBatch);
        //            return null;
        //        }
        //        pStart = firstLine[0];
        //        pFinish = lastLine[^1];
        //    }
        //    if (demLayer is null) {
        //        var startVal = 0.0;
        //        var finishVal = 0.0;
        //    } else {
        //        startVal = Topology.valueAtPoint(pStart, demLayer);
        //        finishVal = Topology.valueAtPoint(pFinish, demLayer);
        //        // with grid models centre of cell at edge can have no data for elevation
        //        if (startVal is null) {
        //            startVal = 0.0;
        //        }
        //        if (finishVal is null) {
        //            finishVal = 0.0;
        //        }
        //    }
        //    Debug.Assert(startVal is not null);
        //    Debug.Assert(finishVal is not null);
        //    if (this.outletAtStart) {
        //        maxElev = finishVal * this.verticalFactor;
        //        minElev = startVal * this.verticalFactor;
        //        return ReachData(pFinish.X, pFinish.Y, maxElev, pStart.X, pStart.Y, minElev);
        //    } else {
        //        minElev = finishVal * this.verticalFactor;
        //        maxElev = startVal * this.verticalFactor;
        //        return ReachData(pStart.X, pStart.Y, maxElev, pFinish.X, pFinish.Y, minElev);
        //    }
        //}

        // Generate ReachData record for OGR reach.  If demLayer is null returns source and outlet points with zero elevations.
        public async Task<ReachData> getOgrReachData(OSGeo.OGR.Feature reach, RasterLayer demLayer) {
            double minElev;
            double maxElev;
            double[] pFinish = new double[3];
            double[] pStart = new double[3];
            if (this.isHUC || this.isHAWQS) {
                int wsno = reach.GetFieldAsInteger(this.wsnoIndex);
                pStart[0] = this.nearsources[wsno][0];
                pStart[1] = this.nearsources[wsno][1];
                pFinish[0] = this.outlets[wsno].Item2[0];
                pFinish[1] = this.outlets[wsno].Item2[1];
            } else {
                var firstLine = Topology.reachOgrFirstLine(reach, this.dx, this.dy);
                if (firstLine is null || firstLine.GetPointCount() < 1) {
                    Utils.error("It looks like your stream shapefile does not obey the single direction rule, that all reaches are either upstream or downstream.", this.isBatch);
                    return null;
                }
                var lastLine = Topology.reachOgrLastLine(reach, this.dx, this.dy);
                if (lastLine is null || lastLine.GetPointCount() < 1) {
                    Utils.error("It looks like your stream shapefile does not obey the single direction rule, that all reaches are either upstream or downstream.", this.isBatch);
                    return null;
                }
                firstLine.GetPoint(0, pStart);
                lastLine.GetPoint(lastLine.GetPointCount() - 1, pFinish);
            }
            double startVal = 0;
            double finishVal = 0;
            if (demLayer is not null) {
                ArcGIS.Core.Data.Raster.Raster demRas;
                await QueuedTask.Run(() => {
                    demRas = demLayer.GetRaster();
                    startVal = Topology.valueAtPoint(pStart, demRas);
                    finishVal = Topology.valueAtPoint(pFinish, demRas);
                });
                if (startVal == this.demNodata) {
                    if (finishVal == this.demNodata) {
                        if (this.isHUC || this.isHAWQS) {
                            // allow for streams outside DEM
                            startVal = 0;
                            finishVal = 0;
                        } else {
                            Utils.error(String.Format("Stream link {4} ({0},{1}) to ({2},{3}) seems to be outside DEM", pStart[0], pStart[1], pFinish[0], pFinish[1], reach.GetFieldAsInteger(this.channelIndex)), this.isBatch);
                            return null;
                        }
                    } else {
                        startVal = finishVal;
                    }
                } else if (finishVal == this.demNodata) {
                    finishVal = startVal;
                }
            }
            if (this.outletAtStart) {
                maxElev = finishVal * this.verticalFactor;
                minElev = startVal * this.verticalFactor;
                return new ReachData(pFinish[0], pFinish[1], maxElev, pStart[0], pStart[1], minElev);
            } else {
                minElev = finishVal * this.verticalFactor;
                maxElev = startVal * this.verticalFactor;
                return new ReachData(pStart[0], pStart[1], maxElev, pFinish[0], pFinish[1], minElev);
            }
        }

        // Length of reach assuming it is a single straight line.

        public static double gridReachLength(ReachData data) {
            var dx = data.upperX - data.lowerX;
            var dy = data.upperY - data.lowerY;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        // Return true if the subbasin field values can be used as SWAT basin numbers.
        //         
        //         The basin numbers, if any, can be used if they 
        //         are all positive and different.
        //         Also populate subbasinToSWATBasin and SWATBasinToSubbasin if successful, else these are undetermined.
        //         
        public virtual bool tryBasinAsSWATBasin(OSGeo.OGR.Layer subbasinsLayer, int polyIndex, int subbasinIndex, bool useGridModel) {
            Debug.Assert(polyIndex >= 0 && subbasinIndex >= 0 && this.subbasinToSWATBasin.Count == 0 && this.SWATBasinToSubbasin.Count == 0);
            var SWATBasins = new HashSet<int>();
            subbasinsLayer.ResetReading();
            OSGeo.OGR.Feature polygon;
            do {
                polygon = subbasinsLayer.GetNextFeature();
                if (polygon is null) { break; }
                var subbasin = polygon.GetFieldAsInteger(polyIndex);
                if (this.upstreamFromInlets.Contains(subbasin) || !(useGridModel || this.chBasinToSubbasin.Values.Contains(subbasin))) {
                    // removed by chBasin being within a lake
                    continue;
                }
                var SWATBasin = polygon.GetFieldAsInteger(subbasinIndex);
                if (SWATBasin <= 0) {
                    return false;
                }
                if (SWATBasins.Contains(SWATBasin)) {
                    return false;
                }
                this.subbasinToSWATBasin[subbasin] = SWATBasin;
                this.SWATBasinToSubbasin[SWATBasin] = subbasin;
                SWATBasins.Add(SWATBasin);
            } while (polygon is not null);
            return true;
        }

        // Add PointId field if necessary and set to id or largest id + 1 if id is zero.  Return index of PointId.

        public static int makePositiveOutletIds(OSGeo.OGR.Layer outletLayer) {
            var outletDefn = outletLayer.GetLayerDefn();
            var ptIdIndex = outletDefn.GetFieldIndex(Topology._POINTID);
            if (ptIdIndex < 0) {
                var field = new OSGeo.OGR.FieldDefn(Topology._POINTID, OSGeo.OGR.FieldType.OFTInteger);
                outletLayer.CreateField(field, 1);
                ptIdIndex = outletLayer.GetLayerDefn().GetFieldIndex(Topology._POINTID);
            }
            var idIndex = outletDefn.GetFieldIndex(Topology._ID);
            var maxId = 0;
            long zeroId = -1;
            outletLayer.ResetReading();
            OSGeo.OGR.Feature point;
            do {
                point = outletLayer.GetNextFeature();
                if (point is null) { break; }
                var pointId = point.GetFieldAsInteger(idIndex);
                if (pointId == 0) {
                    zeroId = point.GetFID();
                } else {
                    point.SetField(ptIdIndex, pointId);
                    outletLayer.SetFeature(point);
                    maxId = Math.Max(maxId, pointId);
                }
            } while (point is not null);
            if (zeroId >= 0) {
                point = outletLayer.GetFeature(zeroId);
                point.SetField(ptIdIndex, maxId + 1);
                outletLayer.SetFeature(point);
            }
            return ptIdIndex;
        }

        // Set to id or largest id + 1 if id is zero.  Return true if successful.

        public static async Task<bool> makePositiveOutletIds(FeatureLayer outletLayer, int idIndex, int ptIdIndex) {
            var maxId = 0;
            long zeroId = -1;
            bool result = await QueuedTask.Run<bool>(() => {
                var op = new ArcGIS.Desktop.Editing.EditOperation();
                op.Name = "Set PointId";
                using (var rc = outletLayer.Search()) {
                    //if (rc is null) {
                    //    Utils.error("rc is null", false);
                    //    return false;
                    //}
                    while (rc.MoveNext()) {
                        //if (rc.Current is null) {
                        //    Utils.error("rc.Current is null", false);
                        //    return false;
                        //}
                        using (var f = rc.Current as ArcGIS.Core.Data.Feature) { 
                            var pointId = Convert.ToInt32(f[idIndex]);
                            if (pointId == 0) {
                                zeroId = f.GetObjectID();
                            } else {
                                op.Modify(f, ptIdIndex, pointId);
                                if (!op.IsEmpty) {
                                    if (!op.Execute()) { // returns true if the operation was successful and false if not
                                        return false;
                                    }
                                } else { return false; }
                                maxId = Math.Max(maxId, pointId);
                            }
                        }
                    }
                }
                if (zeroId >= 0) {
                    var qf = new ArcGIS.Core.Data.QueryFilter() {
                        WhereClause = "FID = " + zeroId.ToString() 
                    };
                    using (var rc = outletLayer.Search(qf)) {
                        rc.MoveNext();
                        var f = rc.Current as ArcGIS.Core.Data.Feature;
                        op.Modify(f, ptIdIndex, maxId + 1);
                        if (!op.IsEmpty) {
                            if (!op.Execute()) {
                                return false;
                            }
                        } else { return false; }
                    }
                }
                return true;
            });
            return result;
        }

        // Return the nearest point on a stream segment to the input point.

        public static MapPoint snapPointToReach(
            FeatureLayer streamLayer,
            MapPoint point,
            double threshold,
            double[] transform,
            bool isBatch) {
            var p = Topology.nearestStreamPoint(streamLayer, point, threshold, transform);
            if (p is null) {
                Utils.error(string.Format("Cannot snap point ({0:G0}, {1:G0}) to stream network", point.X, point.Y), isBatch);
                return null;
            }
            return p;
        }

        // Return true if points not separated by more than numPixels pixels.

        public static bool withinPixels(int numPixels, double[] p1, double[] p2, double[] transform) {
            var (col1, row1) = Topology.projToCell(p1[0], p1[1], transform);
            var (col2, row2) = Topology.projToCell(p2[0], p2[1], transform);
            return Math.Abs(col1 - col2) <= numPixels && Math.Abs(row1 - row2) <= numPixels;
        }

        // Return true if p is on a pixel boundary or very close.

        public static bool onPixelBoundary(double[] p, double[] transform) {
            var floatCol = p[0] - transform[0] / transform[1];
            var intCol = Convert.ToInt32(floatCol);
            var floatRow = p[1] - transform[3] / transform[5];
            var intRow = Convert.ToInt32(floatRow);
            return Math.Abs(floatCol - intCol) < 0.1 || Math.Abs(floatRow - intRow) < 0.1;
        }

        // If p2 is in same cell as p1 return a point in the next cell in the direction of p1 to p2.
        //         Else return p2.

        public static double[] separatePoints(double[] p1, double[] p2, double[] transform) {
            if (Topology.withinPixels(1, p1, p2, transform)) {
                // p1 and p2 are in adjacent or same cells.  
                if (Topology.withinPixels(0, p1, p2, transform) || Topology.onPixelBoundary(p2, transform)) {
                    return Topology.shiftedPoint(p1, p2, transform, 1.1, false);
                }
            }
            return p2;
        }

        // p is on channel.  Find a point downstream from p on the channel that is in a different pixel from the inlet.
        public virtual double[] shiftDownstream(double[] p, OSGeo.OGR.Feature channel, double[] inlet, double[] transform) {
            double newX;
            double[] newPoint;
            double newY;
            double[] upper = new double[3];
            double[] target = new double[3];
            List<OSGeo.OGR.Geometry> parts;

            bool isOnSegment(double[] p, double[] p1, double[] p2) {

                bool between(double a, double b, double c) {
                    return Math.Min(b, c) <= a && a <= Math.Max(b, c);
                }

                return between(p[0], p1[0], p2[0]) && between(p[1], p1[1], p2[1]);
            }
            // def posOnSegment(p: MapPointBuilderEx.CreateMapPoint, p1: MapPointBuilderEx.CreateMapPoint, p2: MapPointBuilderEx.CreateMapPoint) -> Tuple[float, float]:
            //     """Calculate ratios in x and y directions for p position between p1 and p2."""
            //     if p1.X == p2.X:
            //         xRatio = p1.X - p.X
            //     else:
            //         xRatio = (p1.X - p.X) / (p1.X - p2.X)
            //     if p1.Y == p2.Y:
            //         yRatio = p1.Y - p.Y
            //     else:
            //         yRatio = (p1.Y - p.Y) / (p1.Y - p2.Y)
            //     return (Math.Abs(xRatio), Math.Abs(yRatio))    
            //
            var (col, row) = Topology.projToCell(inlet[0], inlet[1], transform);
            // first find channel segment that p1 is on
            var geometry = channel.GetGeometryRef();
            if (geometry.GetGeometryType() == wkbGeometryType.wkbMultiLineString) {
                parts = Topology.geometryToPartsList(geometry);
            } else {
                parts = new List<OSGeo.OGR.Geometry> {
                    geometry
                };
            }
            var found = false;
            var useOriginalPoint = true;
            foreach (var line in parts) {
                int count = line.GetPointCount();
                for (int j = 0; j < count - 1; j++) {
                    //xRatio, yRatio = posOnSegment(p, line[j], line[j+1])
                    //if 0 <= xRatio <= 1 and 0 <= yRatio <= 1 and Math.Abs(xRatio - yRatio) < 0.1:
                    // point is on segement (with some margin for error)
                    double[] p1 = new double[3];
                    double[] p2 = new double[3];
                    line.GetPoint(j, p1);
                    line.GetPoint(j + 1, p1);
                    if (isOnSegment(p, p1, p2)) {
                        found = true;
                        if (this.outletAtStart) {
                            // need to move towards p1 from p2
                            target = p1;
                            upper = p2;
                        } else {
                            target = p2;
                            upper = p1;
                        }
                        // target is best we can do on this segment, so check it is feasible
                        var (colTarget, rowTarget) = Topology.projToCell(target[0], target[1], transform);
                        while (col == colTarget && row == rowTarget) {
                            useOriginalPoint = false;
                            if (this.outletAtStart) {
                                if (j > 0) {
                                    j = j - 1;
                                    line.GetPoint(j, target);
                                    line.GetPoint(j + 1, upper);
                                } else {
                                    return null;
                                }
                            } else if (j < count - 2) {
                                j = j + 1;
                                line.GetPoint(j + 1, target);
                                line.GetPoint(j, upper);
                            } else {
                                return null;
                            }
                            (colTarget, rowTarget) = Topology.projToCell(target[0], target[1], transform);
                        }
                        // found possible segment
                        if (!useOriginalPoint) {
                            // have had to shift from segment conatining p
                            // start from upper, as it is previous target and is too close to inlet
                            p = upper;
                        }
                        break;
                    }
                }
                if (found) {
                    break;
                }
            }
            if (!found) {
                return null;
            }
            if (target[0] == upper[0]) {
                // vertical segment
                if (target[1] > upper[1]) {
                    newY = Math.Min(target[1], p[1] + this.dy);
                } else {
                    newY = Math.Max(target[1], p[1] - this.dy);
                }
                newPoint = new double[3] { target[0], newY, 0 };
            } else {
                if (target[0] > upper[0]) {
                    newX = Math.Min(target[0], p[0] + this.dx);
                } else {
                    newX = Math.Max(target[0], p[0] - this.dx);
                }
                newY = p[1] + (newX - p[0]) * (target[1] - upper[1]) / (target[0] - upper[0]);
                newPoint = new double[3] { newX, newY, 0 };
            }
            if (Topology.withinPixels(0, inlet, newPoint, transform)) {
                return null;
            } else {
                return newPoint;
            }
        }

        // Return point which is frac of a cell away from p1 in direction p1 to p2,
        // If stayWithin move no further than p2, so it stays on the segment ending at p2.

        public static MapPoint shiftedPoint(MapPoint p1, MapPoint p2, double[] transform, double frac, bool stayWithin) {
            var p1_2D = new Coordinate2D(p1.X, p1.Y);
            var p2_2D = new Coordinate2D(p2.X, p2.Y);
            var p3_2D = shiftedPoint(p1_2D, p2_2D, transform, frac, stayWithin);
            return MapPointBuilderEx.CreateMapPoint(p3_2D.X, p3_2D.Y);
        }

        public static double[] shiftedPoint(double[] p1, double[] p2, double[] transform, double frac, bool stayWithin) {
            var p1_2D = new Coordinate2D(p1[0], p1[1]);
            var p2_2D = new Coordinate2D(p2[0], p2[1]);
            var p3_2D = shiftedPoint(p1_2D, p2_2D, transform, frac, stayWithin);
            return new double[3] { p3_2D.X, p3_2D.Y, 0 };
        }

        public static Coordinate2D shiftedPoint(Coordinate2D p1, Coordinate2D p2, double[] transform, double frac, bool stayWithin) {
            var x1 = p1.X;
            var y1 = p1.Y;
            var x2 = p2.X;
            var y2 = p2.Y;
            var dirx = x2 > x1 ? 1 : -1;
            var diry = y2 > y1 ? 1 : -1;
            var stepx = transform[1] * frac;
            var stepy = Math.Abs(transform[5]) * frac;
            double shiftx;
            double shifty;
            if (x1 == x2) {
                // vertical
                shiftx = 0.0;
                shifty = stepy * diry;
                if (stayWithin && Math.Abs(shifty) > Math.Abs(y1 - y2)) {
                    // moving beyond p2
                    return p2;
                }
            } else {
                var slope = Math.Abs((y1 - y2) / (x1 - x2));
                if (slope < 1) {
                    shiftx = stepx * dirx;
                    shifty = stepy * diry * slope;
                } else {
                    shiftx = stepx * dirx / slope;
                    shifty = stepy * diry;
                }
                if (stayWithin && Math.Abs(shiftx) > Math.Abs(x1 - x2)) {
                    // moving beyond p2
                    return p2;
                }
            }
            return new Coordinate2D(x1 + shiftx, y1 + shifty);
        }

            // Find nearest vertex in streamLayer to point and 
            //         return the line (line geometry) in the reach and 
            //         index of the vertex within the line.
            //         

            public static MapPoint nearestStreamPoint(FeatureLayer streamLayer, MapPoint point, double threshold, double[] transform) {
            var bestSegmentIndex = -1;
            ReadOnlySegmentCollection bestLine = null;
            bool isStart = true;
            var minMeasure = double.PositiveInfinity;
            double measure;
            MapPoint p = null;
            MapPoint p1 = null;
            MapPoint p2 = null;
            bool firstChosen;
            ArcGIS.Core.Geometry.Segment chosenSegment = null;
            using (RowCursor cursor = streamLayer.Search(null)) {
                while (cursor.MoveNext()) {
                    var reach = cursor.Current as ArcGIS.Core.Data.Feature;
                    var geometry = reach.GetShape() as ArcGIS.Core.Geometry.Polyline;
                    var parts = geometry.Parts;
                    foreach (var line in parts) {
                        foreach (var j in Enumerable.Range(0, line.Count)) {
                            var linej = line[j];
                            measure = Topology.distanceMeasure(linej.StartPoint, point);
                            if (measure < minMeasure) {
                                minMeasure = measure;
                                bestSegmentIndex = j;
                                bestLine = line;
                                isStart = true;
                            }
                            measure = Topology.distanceMeasure(linej.EndPoint, point);
                            if (measure < minMeasure) {
                                minMeasure = measure;
                                bestSegmentIndex = j;
                                bestLine = line;
                                isStart = false;
                            }
                        }
                    }
                }
            }
            // have best line and best segment tip.  There are two candidates: the intercepts on this segment and the next (if any).
            if (isStart) {
                if (bestSegmentIndex == 0) {
                    p = getIntercept(bestLine[0].StartPoint, bestLine[0].EndPoint, point);
                } else {
                    p1 = getIntercept(bestLine[bestSegmentIndex].StartPoint, bestLine[bestSegmentIndex].EndPoint, point);
                    p2 = getIntercept(bestLine[bestSegmentIndex - 1].EndPoint, bestLine[bestSegmentIndex - 1].StartPoint, point);
                    (p, firstChosen) = nearer(p1, p2, point);
                    if (firstChosen) {
                        chosenSegment = bestLine[bestSegmentIndex];
                        isStart = true;
                    } else {
                        chosenSegment = bestLine[bestSegmentIndex - 1];
                        isStart = false;
                    }
                }
            } else {
                if (bestSegmentIndex == bestLine.Count - 1) {
                    p = getIntercept(bestLine[bestSegmentIndex].EndPoint, bestLine[bestSegmentIndex].StartPoint, point);
                } else {
                    p1 = getIntercept(bestLine[bestSegmentIndex].EndPoint, bestLine[bestSegmentIndex].StartPoint, point);
                    p2 = getIntercept(bestLine[bestSegmentIndex + 1].StartPoint, bestLine[bestSegmentIndex + 1].EndPoint, point);
                    (p, firstChosen) = nearer(p1, p2, point);
                    if (firstChosen) {
                        chosenSegment = bestLine[bestSegmentIndex];
                        isStart = false;
                    } else {
                        chosenSegment = bestLine[bestSegmentIndex + 1];
                        isStart = true;
                    }
                    chosenSegment = firstChosen ? bestLine[bestSegmentIndex] : bestLine[bestSegmentIndex + 1];
                }
            }
            // check p is sufficiently near point
            if (Topology.distanceMeasure(p, point) <= threshold * threshold) {
                // before returning p, move it along the stream a little if it is on or close to a corner of a DEM cell
                // since TauDEM can fail to make a boundary or use its id as a DSNODEID if is so positioned
                if ((p1 is null) || (p1 == p2)) {
                    // an endpoint on the segment was chosen, which is safe (points on a stream are centres of DEM cells)
                    return p;
                } else {
                    var floatCol = p.X - transform[0] / transform[1];
                    var intCol = Convert.ToInt32(floatCol);
                    var floatRow = p.Y - transform[3] / transform[5];
                    var intRow = Convert.ToInt32(floatRow);
                    if (Math.Abs(floatCol - intCol) < 0.1 && Math.Abs(floatRow - intRow) < 0.1) {
                        // move the point towards nearest end point by about half a cell
                        var nearestPoint = isStart ? chosenSegment.StartPoint : chosenSegment.EndPoint;
                        var p3 = Topology.shiftedPoint(p, nearestPoint, transform, 0.4, true);
                        Utils.loginfo(string.Format("({0:G0},{1:G0}) shifted to ({2:G0},{3:G0})", p.X, p.Y, p3.X, p3.Y));
                        return p3;
                    } else {
                        return p;
                    }
                }
            } else {
                return null;
            }
        }

        //// Find nearest point in the stream layer, i.e. one segment tip is the nearest segment tip and the vertical distance
        //// to the next segment is greater than the vertical distance to this one.
        //// The next segment is one with an increased index if the nearest point is an end,
        //// and the one with a decreased index if the nearest point is a start.
        //// Return the segment plus true if the nearest point is the segment start
        ////         
        //public static Coordinate2D nearestVertex(FeatureLayer streamLayer, Coordinate2D point) {
        //        int bestSegmentIndex = -1;
        //        ReadOnlySegmentCollection bestLine = null;
        //        bool isStart = true;
        //        var minMeasure = double.PositiveInfinity;
        //        double measure;
        //        using (RowCursor cursor = streamLayer.Search(null)) {
        //            while (cursor.MoveNext()) {
        //                var reach = cursor.Current as ArcGIS.Core.Data.Feature;
        //                var geometry = reach.GetShape() as ArcGIS.Core.Geometry.Polyline;
        //                var parts = geometry.Parts;
        //                foreach (var line in parts) {
        //                    foreach (var j in Enumerable.Range(0, line.Count)) {
        //                        var linej = line[j];
        //                        measure = Topology.distanceMeasure(linej.StartCoordinate, point);
        //                        if (measure < minMeasure) {
        //                            minMeasure = measure;
        //                            bestSegmentIndex = j;
        //                            bestLine = line;
        //                            isStart = true;
        //                        }
        //                        measure = Topology.distanceMeasure(linej.EndCoordinate, point);
        //                        if (measure < minMeasure) {
        //                            minMeasure = measure;
        //                            bestSegmentIndex = j;
        //                            bestLine = line;
        //                            isStart = false;
        //                        }
        //                    }
        //                }
        //            }
        //            // have best line and best segment tip.  There are two candidates: the intercepts on this segment and the next (if any).
        //            if (isStart) {
        //                if (bestSegmentIndex == 0) return getIntercept(bestLine[0].StartCoordinate, bestLine[0].EndCoordinate, point);
        //                var p1 = getIntercept(bestLine[bestSegmentIndex].StartCoordinate, bestLine[bestSegmentIndex].EndCoordinate, point);
        //                var p2 = getIntercept(bestLine[bestSegmentIndex - 1].EndCoordinate, bestLine[bestSegmentIndex - 1].StartCoordinate, point);
        //                return nearer(p1, p2, point);
        //            } else {
        //                if (bestSegmentIndex == bestLine.Count - 1) return getIntercept(bestLine[bestSegmentIndex].EndCoordinate, bestLine[bestSegmentIndex].StartCoordinate, point);
        //                var p1 = getIntercept(bestLine[bestSegmentIndex].EndCoordinate, bestLine[bestSegmentIndex].StartCoordinate, point);
        //                var p2 = getIntercept(bestLine[bestSegmentIndex + 1].StartCoordinate, bestLine[bestSegmentIndex + 1].EndCoordinate, point);
        //                return nearer(p1, p2, point);
        //            }
        //        }
        //    }

        //// Get points on segments on either side of pt at pointIndex where 
        ////         vertical from point meets the segment.
        ////         

        //public static (double[], double[]) intercepts(OSGeo.OGR.Geometry line, int pointIndex, double[] pt, double[] point) {
        //    int pointCount = line.GetPointCount();
        //    Debug.Assert(Enumerable.Range(0, line.GetPointCount()).Contains(pointIndex));
        //    double[] lower, upper;
        //    // first try above pointIndex
        //    if (pointIndex == pointCount - 1) {
        //        // We are at the upper end - no upper segment.  
        //        // Return just this point to avoid a tiny subbasin.
        //        return (pt, pt);
        //    } else {
        //        double[] pt2 = new double[3];
        //        line.GetPoint(pointIndex + 1, pt2);
        //        upper = Topology.getIntercept(pt, pt2, point);
        //    }
        //    if (pointIndex == 0) {
        //        // We are at the lower end - no lower segment.  
        //        // Return just this point to avoid a tiny subbasin.
        //        return (pt, pt);
        //    } else {
        //        double[] pt0 = new double[3];
        //        line.GetPoint(pointIndex - 1, pt0);
        //        lower = Topology.getIntercept(pt, pt0, point);
        //    }
        //    return (lower, upper);
        //}

        // Return point on line from p1 to p2 where 
        //         vertical from p intercepts it, or None if there is no intercept.
        //         

        public static double[] getIntercept(double[] p1, double[] p2, double[] p) {
            var x1 = p1[0];
            var x2 = p2[0];
            var xp = p[0];
            var y1 = p1[1];
            var y2 = p2[1];
            var yp = p[1];
            var X = x1 - x2;
            var Y = y1 - y2;
            Debug.Assert(!(X == 0 && Y == 0));
            var prop = (X * (x1 - xp) + Y * (y1 - yp)) / (X * X + Y * Y);
            if (prop < 0) {
                // intercept is off the line beyond p1
                // technically we should check for prop > 1, which means 
                // intercept is off the line beyond p2, but we can assume p is nearer to p1
                return null;
            } else {
                Debug.Assert(0 <= prop && prop < 1);
                return new double[3] { x1 - prop * X, y1 - prop * Y, 0 };
            }
        }

        // Return point in segment with tips p1 and p2 where 
        // vertical from p intercepts it, or p1 if there is no intercept.
        // p1 is known to be nearer to P than p2.
        //         
        public static MapPoint getIntercept(MapPoint p1, MapPoint p2, MapPoint p) {
            var x1 = p1.X;
            var x2 = p2.X;
            var xp = p.X;
            var y1 = p1.Y;
            var y2 = p2.Y;
            var yp = p.Y;
            var X = x1 - x2;
            var Y = y1 - y2;
            //Debug.Assert(!(X == 0 && Y == 0));
            var prop = (X * (x1 - xp) + Y * (y1 - yp)) / (X * X + Y * Y);
            if (prop < 0) {
                // intercept is off the line beyond p1.
                // Could check for prop > 1, which means 
                // intercept is off the line beyond p2, but we know p is nearer to p1
                // so return the nearest
                return p1;
            } else {
                //Debug.Assert(0 <= prop && prop < 1);
                return new MapPointBuilderEx(x1 - prop * X, y1 - prop * Y).ToGeometry();
            }
        }

        // Return the nearer of p1 and p2 to p.

        public static double[] nearer(double[] p1, double[] p2, double[] p) {
            if (p1 is null) {
                return p2;
            }
            if (p2 is null) {
                return p1;
            }
            if (Topology.distanceMeasure(p1, p) < Topology.distanceMeasure(p2, p)) {
                return p1;
            } else {
                return p2;
            }
        }

        // Return the nearer of p1 and p2 to p.
        public static Coordinate2D nearer(Coordinate2D p1, Coordinate2D p2, Coordinate2D p) {
                if (Topology.distanceMeasure(p1, p) < Topology.distanceMeasure(p2, p)) {
                    return p1;
                } else {
                    return p2;
                }
            }

            // Return the nearer of p1 and p2 to p.
            public static (MapPoint, bool) nearer(MapPoint p1, MapPoint p2, MapPoint p) {
                    if (Topology.distanceMeasure(p1, p) < Topology.distanceMeasure(p2, p)) {
                        return (p1, true);
                    } else {
                        return (p2, false);
                    }
                }

            // Return square of distance between p1 and p2.

            public static double distanceMeasure(double[] p1, double[] p2) {
            var dx = p1[0] - p2[0];
            var dy = p1[1] - p2[1];
            return dx * dx + dy * dy;
        }

        // Return square of distance between p1 and p2.

        public static double distanceMeasure(Coordinate2D p1, Coordinate2D p2) {
            var dx = p1.X - p2.X;
            var dy = p1.Y - p2.Y;
            return dx * dx + dy * dy;
            }

            // Return square of distance between p1 and p2.

            public static double distanceMeasure(MapPoint p1, MapPoint p2) {
                var dx = p1.X - p2.X;
                var dy = p1.Y - p2.Y;
                return dx * dx + dy * dy;
            }

        // 
        //         Write table of subbasin to maximum flow length along channels within the basin.
        //         
        //         Used for maximum flow path for existing non-grid models, and only defined for these.
        //         
        public virtual void setMaxFlowLengths() {
            var channelFlowLengths = new Dictionary<int, double>();
            foreach (var (chLink, length) in this.channelLengths) {
                this.setChannelFlowLength(chLink, length, channelFlowLengths);
            }
        }

        // Add entry for chLink to channelFlowLengths map.  Also update maxFlowLengths for chLink's subbasin.
        //         
        //         post: chLink in channelFlowLengths
        //         
        public virtual void setChannelFlowLength(int chLink, double length, Dictionary<int, double> channelFlowLengths) {
            double flowLength;
            if (channelFlowLengths.ContainsKey(chLink)) {
                return;
            }
            if (this.zeroChannels.Contains(chLink)) {
                return;
            }
            var chBasin = this.chLinkToChBasin[chLink];
            var subbasin = this.chBasinToSubbasin[chBasin];
            var dsLink = this.downChannels[chLink];
            int dsChBasin;
            if (this.chLinkToChBasin.TryGetValue(dsLink, out dsChBasin)) {
                int dsBasin;
                if (this.chBasinToSubbasin.TryGetValue(dsChBasin, out dsBasin)) {
                    if (dsBasin == subbasin) {
                        // still in same subbasin:
                        // Add this channel's length to downstream flow length
                        double dsFlowLength;
                        if (!channelFlowLengths.TryGetValue(dsLink, out dsFlowLength)) {
                            this.setChannelFlowLength(dsLink, this.channelLengths[dsLink], channelFlowLengths);
                            dsFlowLength = channelFlowLengths[dsLink];
                        }
                        flowLength = dsFlowLength + length;
                    } else {
                        // outlet channel for subbasin
                        flowLength = length;
                    }
                } else {
                    flowLength = length;
                }
            } else {
                flowLength = length;
            }
            channelFlowLengths[chLink] = flowLength;
            double maxFlowLength;
            if (!this.maxFlowLengths.TryGetValue(subbasin, out maxFlowLength)) {
                maxFlowLength = 0;
            }
            if (flowLength > maxFlowLength) {
                this.maxFlowLengths[subbasin] = flowLength;
            }
        }

        // Write the gis_points table in the project database.
        public virtual async void writePointsTable(RasterLayer demLayer, IEnumerable<int> mergees, bool useGridModel, bool existingWshed) {

            var table = "gis_points";
            var clearSQL = "DROP TABLE IF EXISTS " + table;
            db.execNonQuery(clearSQL);
            db.execNonQuery(DBUtils._POINTSCREATESQL);
            var waterAdded = new List<int>();
            ArcGIS.Core.Data.Raster.Raster demRaster = null;
            await QueuedTask.Run(() => { demRaster = demLayer.GetRaster(); });
            // Add outlets from streams
            foreach (var (subbasin, (pointId, pt, _)) in this.outlets) {
                if (this.upstreamFromInlets.Contains(subbasin)) {
                    continue;
                }
                var elev = Topology.valueAtPoint(pt, demRaster);
                if (double.IsNaN(elev)) {
                    elev = 0;
                }
                this.addPoint(subbasin, pointId, pt, (double)elev, "O");
            }
            // Add inlets
            if (useGridModel) {
                foreach (var (chLink, (pointId, pt)) in this.chLinkToInlet) {
                    if (this.chLinkInsideLake.ContainsKey(chLink) || this.chLinkFromLake.ContainsKey(chLink)) {
                        // shouldn't happen
                        continue;
                    }
                    int subbasin = this.chLinkToChBasin[chLink];
                    var elev = Topology.valueAtPoint(pt, demRaster);
                    if (double.IsNaN(elev)) {
                        elev = 0;
                    }
                    this.addPoint(subbasin, pointId, pt, (double)elev, "I");
                }
            } else {
                foreach (var (subbasin, (pointId, pt)) in this.inlets) {
                    if (this.upstreamFromInlets.Contains(subbasin)) {
                        // shouldn't happen, but users can be stupid
                        continue;
                    }
                    var elev = Topology.valueAtPoint(pt, demRaster);
                    if (double.IsNaN(elev)) {
                        elev = 0;
                    }
                    this.addPoint(subbasin, pointId, pt, (double)elev, "I");
                }
            }
            // Add point sources at heads of channels
            foreach (var (chLink, (pointId, pt)) in this.chLinkToPtSrc) {
                int subbasin;
                if (this.chLinkInsideLake.ContainsKey(chLink)) {
                    continue;
                }
                if (useGridModel) {
                    if (this.chLinkFromLake.ContainsKey(chLink)) {
                        continue;
                    }
                    subbasin = this.chLinkToChBasin[chLink];
                } else {
                    int chBasin;
                    if (!this.chLinkToChBasin.TryGetValue(chLink, out chBasin)) {
                        continue;
                    }
                    if (!this.chBasinToSubbasin.TryGetValue(chBasin, out subbasin)) {
                        continue;
                    }
                    if (subbasin < 0 || this.upstreamFromInlets.Contains(subbasin)) {
                        continue;
                    }
                }
                var elev = Topology.valueAtPoint(pt, demRaster);
                if (double.IsNaN(elev)) {
                    elev = 0;
                }
                this.addPoint(subbasin, pointId, pt, (double)elev, "P");
            }
            foreach (var (chLink, (pointId, pt)) in this.chPointSources) {
                int subbasin;
                if (this.chLinkToPtSrc.ContainsKey(chLink) || mergees.Contains(chLink) || this.chLinkInsideLake.ContainsKey(chLink)) {
                    continue;
                }
                if (useGridModel) {
                    if (this.chLinkFromLake.ContainsKey(chLink)) {
                        continue;
                    }
                    subbasin = this.chLinkToChBasin[chLink];
                } else {
                    int chBasin;
                    if (!this.chLinkToChBasin.TryGetValue(chLink, out chBasin)) {
                        continue;
                    }
                    if (!this.chBasinToSubbasin.TryGetValue(chBasin, out subbasin)) {
                        continue;
                    }
                    if (subbasin < 0 || this.upstreamFromInlets.Contains(subbasin)) {
                        continue;
                    }
                }
                var elev = Topology.valueAtPoint(pt, demRaster);
                if (double.IsNaN(elev)) {
                    elev = 0;
                }
                this.addPoint(subbasin, pointId, pt, (double)elev, "P");
            }
            // Add lakes
            foreach (var lake in this.lakesData.Values) {
                if (new HashSet<WaterRole> {
                        WaterRole._RESERVOIR,
                        WaterRole._POND,
                        WaterRole._WETLAND
                    }.Contains(lake.waterRole)) {
                    // outlet from lake
                    var (subbasin, pointId, pt, elev) = lake.outPoint;
                    int chLink = lake.outChLink;
                    // unnecessary as will be in self.outlets
                    //if not useGridModel and chLink == -1:
                    //    # main outlet was moved inside lake, but reservoir point will still be routed to it
                    //    # so Add its definition
                    //    (outletId, outPt, _) = self.outlets[subbasin]
                    //    self.addPoint(curs, subbasin, outletId, outPt, elev, 'O')
                    this.addPoint(subbasin, pointId, pt, elev, "W");
                    waterAdded.Add(pointId);
                    // inlets to lake.  These are outlets from streams in grid models, so not necessary
                    if (!useGridModel) {
                        foreach (var (inchLink, (inpointId, inpt, inelev)) in lake.inChLinks) {
                            var inchBasin = this.chLinkToChBasin[inchLink];
                            var insubbasin = this.chBasinToSubbasin[inchBasin];
                            this.addPoint(insubbasin, inpointId, inpt, inelev, "O");
                        }
                    }
                }
            }
            foreach (var (chLink, (pointId, pt, _)) in this.chLinkToWater) {
                int subbasin;
                // reservoir points at lake outlets can appear here 
                // but already added from lakesdata
                if (waterAdded.Contains(pointId)) {
                    continue;
                }
                if (useGridModel) {
                    subbasin = this.chLinkToChBasin[chLink];
                } else {
                    int chBasin;
                    if (!this.chLinkToChBasin.TryGetValue(chLink, out chBasin)) {
                        continue;
                    }
                    if (!this.chBasinToSubbasin.TryGetValue(chBasin, out subbasin)) {
                        continue;
                    }
                    if (this.upstreamFromInlets.Contains(subbasin)) {
                        continue;
                    }
                }
                var elev = Topology.valueAtPoint(pt, demRaster);
                if (double.IsNaN(elev)) {
                    elev = 0;
                }
                this.addPoint(subbasin, pointId, pt, (double)elev, "W");
            }
            foreach (var (channel, (_, pointId, pt)) in this.foundReservoirs) {
                int subbasin;
                if (useGridModel) {
                    subbasin = this.chLinkToChBasin[channel];
                } else {
                    int chBasin;
                    if (!this.chLinkToChBasin.TryGetValue(channel, out chBasin)) {
                        continue;
                    }
                    if (!this.chBasinToSubbasin.TryGetValue(chBasin, out subbasin)) {
                        continue;
                    }
                    if (this.upstreamFromInlets.Contains(subbasin)) {
                        continue;
                    }
                }
                var elev = Topology.valueAtPoint(pt, demLayer.GetRaster());
                if (double.IsNaN(elev)) {
                    elev = 0;
                }
                this.addPoint(subbasin, pointId, pt, elev, "W");
                //             for subbasin, (pointId, pt) in self.extraReservoirs.iteritems():
                //                 if subbasin in self.upstreamFromInlets: 
                //                 # shouldn't happen, but users can be stupid
                //                     continue
                //                 elev = Topology.valueAtPoint(pt, demLayer)
                //                 self.addPoint(curs, subbasin, pointId, pt, elev, 'R')
            }
        }

        // Add extra points needed to mark where channels drain into reservoirs.
        public virtual void addExtraPointsToPointsTable(List<(int, int)> extraPoints, bool useGridModel) {
            foreach (var (channel, pointId) in extraPoints) {
                int subbasin;
                if (useGridModel) {
                    subbasin = this.chLinkToChBasin[channel];
                } else {
                    int chBasin = this.chLinkToChBasin[channel];
                    subbasin = this.chBasinToSubbasin[chBasin];
                }
                var data = this.channelsData[channel];
                var pt = new double[3] { data.lowerX, data.lowerY, 0 };
                this.addPoint(subbasin, pointId, pt, data.lowerZ, "O");
            }
        }

        // Add point to gis_points table.
        public virtual void addPoint(
            int subbasin,
            int pointId,
            double[] pt,
            double elev,
            string typ) {
            var table = "gis_points";
            int SWATBasin;
            if (!this.subbasinToSWATBasin.TryGetValue(subbasin, out SWATBasin)) {
                SWATBasin = 0;
            }
            // include points without SWAT basin since points within lakes may not have one
            //if SWATBasin == 0:
            //    return
            var ptll = this.pointToLatLong(pt);
            var sql = "INSERT INTO " + table + " VALUES(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8)";
            try {
                this.db.execNonQuery(sql, new object[] { pointId, SWATBasin, typ, pt[0], pt[1], ptll[1], ptll[0], elev });
                this.db.addKey(table, pointId);
            } catch {
                Utils.exceptionError(string.Format("Internal error: unable to Add point {0} type {1}", pointId, typ), this.isBatch);
            }
        }

        //===========================================================================
        // def addPoint(self, cursor, link, data, pointId, typ):
        //     """Add a point to the points table."""
        //     table = 'gis_points'
        //     # inlets will be located at the upstream ends of their links
        //     # since they are attached to their downstream basins
        //     if not data:
        //         return
        //     SWATBasin = self.subbasinToSWATBasin.TryGetValue(data.ru, 0)
        //     if SWATBasin == 0:
        //         return
        //     lsu = 0
        //     if typ == 'I': # inlet
        //         pt = MapPointBuilderEx.CreateMapPoint(data.upperX, data.upperY)
        //         elev = data.upperZ
        //         drainId = SWATBasin 
        //         drainCat = 'R'
        //     else:
        //         pt = MapPointBuilderEx.CreateMapPoint(data.lowerX, data.lowerY)
        //         elev = data.lowerZ
        //         if typ == 'P': # point source
        //             resId = self.linkToReservoir.TryGetValue(link, 0)
        //             if resId > 0: 
        //                 # point source drains to reservoir
        //                 drainId = resId
        //                 drainCat = 'P'
        //             else:
        //                 # point source drains to link outlet
        //                 drainId = self.linkToOutlet[link]
        //                 drainCat = 'P'
        //         elif typ == 'R': # reservoir: drains to link outlet
        //             drainId = self.linkToOutlet[link]
        //             drainCat = 'P'
        //         else:
        //             assert typ == 'O', u'Unknown point type: ' + typ
        //             # outlet: drains to reach of downstream basin (if any)
        //             dsLink = self.downLinks[link]
        //             dsSWATBasin = 0
        //             while dsLink >= 0 and dsSWATBasin == 0:
        //                 dsBasin = self.linkToBasin[dsLink]
        //                 dsSWATBasin = self.subbasinToSWATBasin.TryGetValue(dsBasin, 0)
        //                 if dsSWATBasin == 0:
        //                     dsLink = self.downLinks[dsLink]
        //             if dsSWATBasin > 0:
        //                 drainId = dsSWATBasin
        //                 drainCat = 'R'
        //             else:
        //                 drainId = -1
        //                 drainCat = 'X'
        //     ptll = self.pointToLatLong(pt)
        //     sql = "INSERT INTO " + table + " VALUES(?,?,?,?,?,?,?,?,?)"
        //     cursor.execute(sql, (pointId, SWATBasin, lsu, typ, \
        //                    pt.X, pt.Y, ptll.Y, ptll.X, elev))
        //===========================================================================
        // 
        //         Write the channels table in the project database, make rivs1.shp in shapes directory, and copy as results template to TablesOut directory.
        //         
        //         Changes the channel layer, so if successful, returns the new one.
        //         
        public async Task<OSGeo.OGR.Layer> writeChannelsTable(Dictionary<int, int> mergeChannels, Dictionary<int, BasinData> basins, GlobalVars gv) {
            int ft;
            string strng;
            string channelFile;
            OSGeo.OGR.Feature reach;
            int subbasin;
            int downChannel;
            int linkIdx = 0;
            var wid2Data = new Dictionary<int, double>();
            if (gv.useGridModel) {
                // use streams as channels
                channelFile = gv.streamFile;
                strng = "streams";
            } else {
                channelFile = gv.channelFile;
                strng = "channel";
            }
            if (!File.Exists(channelFile)) {
                Utils.error(string.Format("Cannot find {0} file {1}", strng, channelFile), gv.isBatch);
                return null;
            }
            Utils.copyShapefile(channelFile, Parameters._RIVS1, gv.shapesDir);
            var rivs1File = Utils.join(gv.shapesDir, Parameters._RIVS1 + ".shp");
            using (var channelDs = Ogr.Open(channelFile, 0))
            using (var rivs1Ds = Ogr.Open(rivs1File, 1)) {
                var channelLayer = channelDs.GetLayerByIndex(0);
                var rivs1Layer = rivs1Ds.GetLayerByIndex(0);
                var rivs1Defn = rivs1Layer.GetLayerDefn();
                // Add Channel, ChannelR, and Subbasin fields unless already has them
                var chIdx = rivs1Defn.GetFieldIndex(Topology._CHANNEL);
                var chRIdx = rivs1Defn.GetFieldIndex(Topology._CHANNELR);
                var subIdx = rivs1Defn.GetFieldIndex(Topology._SUBBASIN);
                if (chIdx < 0) {
                    rivs1Layer.CreateField(new FieldDefn(Topology._CHANNEL, OSGeo.OGR.FieldType.OFTInteger), 1);
                }
                if (chRIdx < 0) {
                    rivs1Layer.CreateField(new FieldDefn(Topology._CHANNELR, OSGeo.OGR.FieldType.OFTInteger), 1);
                }
                if (subIdx < 0) {
                    rivs1Layer.CreateField(new FieldDefn(Topology._SUBBASIN, OSGeo.OGR.FieldType.OFTInteger), 1);
                }
                rivs1Defn = rivs1Layer.GetLayerDefn();
                chIdx = rivs1Defn.GetFieldIndex(Topology._CHANNEL);
                chRIdx = rivs1Defn.GetFieldIndex(Topology._CHANNELR);
                subIdx = rivs1Defn.GetFieldIndex(Topology._SUBBASIN);
                var chLinkIdx = rivs1Defn.GetFieldIndex(Topology._LINKNO);
                Dictionary<int, MergedChannelData> basinMerge = null;
                if (!gv.useGridModel) {
                    basinMerge = this.mergeChannelData(mergeChannels);
                    // make map channel -> feature it is merged with for merged channels
                    var merges = new Dictionary<int, OSGeo.OGR.Feature>();
                    var targets = new List<int>();
                    rivs1Layer.ResetReading();
                    do {
                        reach = rivs1Layer.GetNextFeature();
                        if (reach is null) { break; }
                        foreach (var channel in mergeChannels.Keys) {
                            var target = this.finalTarget(channel, mergeChannels);
                            if (!targets.Contains(target)) {
                                targets.Add(target);
                            }
                            if (reach.GetFieldAsInteger(chLinkIdx) == target) {
                                merges[channel] = reach;
                                //Utils.loginfo('Channel {0} merging to target {1} with length {2}', channel, target, reach.GetGeometryRef().length()))
                                // create geometries for merged reaches
                            }
                        }
                    } while (reach is not null);
                    var merged = new List<long>();
                    rivs1Layer.ResetReading();
                    do {
                        reach = rivs1Layer.GetNextFeature();
                        if (reach is null) { break; }
                        long rid = reach.GetFID();
                        int channel = reach.GetFieldAsInteger(chLinkIdx);
                        if (targets.Contains(channel) && !merged.Contains(rid)) {
                            merged.Add(rid);
                        }
                        OSGeo.OGR.Feature mergeReach;
                        if (merges.TryGetValue(channel, out mergeReach)) {
                            // Add its geometry to its merger target
                            //length1 = mergeReach.GetGeometryRef().length()
                            //length2 = reach.GetGeometryRef().length()
                            mergeReach.GetGeometryRef().AddGeometry(reach.GetGeometryRef());
                            //length3 = mergeReach.GetGeometryRef().length()
                            //Utils.loginfo('Channel {0} merged to target with length {1} ({2} + {3})' \
                            //                   , channel, length3, length1, length2))
                            if (!merged.Contains(rid)) {
                                merged.Add(rid);
                            }
                        }
                    } while (reach is not null);
                    // remove channels and targets involved in mergers
                    foreach (var rid in merged) {
                        rivs1Layer.DeleteFeature(rid);
                    }
                    // Add mergers
                    var mergers = new List<(int, OSGeo.OGR.Feature)>();
                    foreach (var (channel, reachM) in merges) {
                        rivs1Layer.SetFeature(reachM);
                    }
                }
                var zeroRids = new List<long>();
                rivs1Layer.ResetReading();
                do {
                    reach = rivs1Layer.GetNextFeature();
                    if (reach is null) { break; }
                    int channel = reach.GetFieldAsInteger(chLinkIdx);
                    if (gv.useGridModel) {
                        // subbasin and chBasin are the same
                        subbasin = this.chLinkToChBasin[channel];
                        downChannel = this.downChannels[channel];
                    } else {
                        var chBasin = this.chLinkToChBasin[channel];
                        subbasin = this.chBasinToSubbasin[chBasin];
                        downChannel = this.finalDownstream(channel, mergeChannels);
                    }
                    int SWATBasin;
                    if (!this.subbasinToSWATBasin.TryGetValue(subbasin, out SWATBasin)) {
                        SWATBasin = 0;
                    }
                    int SWATChannel;
                    if (SWATBasin == 0) {
                        SWATChannel = 0;
                    } else if (!this.channelToSWATChannel.TryGetValue(channel, out SWATChannel)) {
                        SWATChannel = 0;
                    }
                    int downSWATChannel;
                    if (!this.channelToSWATChannel.TryGetValue(downChannel, out downSWATChannel)) {
                        downSWATChannel = 0;
                    }
                    long rid = reach.GetFID();
                    if (SWATChannel == 0) {
                        zeroRids.Add(rid);
                    }
                    reach.SetField(chIdx, SWATChannel);
                    reach.SetField(chRIdx, downSWATChannel);
                    reach.SetField(subIdx, SWATBasin);
                    rivs1Layer.SetFeature(reach);
                } while (reach is not null);
                foreach (var rid in zeroRids) {
                    rivs1Layer.DeleteFeature(rid);
                }
                // Add fields from channels table to rivs1File if less than RIV1SUBS1MAX features; otherwise takes too long.
                var addToRiv1 = rivs1Layer.GetFeatureCount(1) <= Parameters._RIVS1SUBS1MAX;
                // remove fields apart from Channel, ChannelR and Subbasin
                if (addToRiv1) {
                    Topology.removeFields(rivs1Layer, new List<string> {
                    Topology._LINKNO,
                    Topology._CHANNEL,
                    Topology._CHANNELR,
                    Topology._SUBBASIN
                }, rivs1File, this.isBatch);
                }
                int areaCIdx = 0;
                int orderIdx = 0;
                int len2Idx = 0;
                int slo2Idx = 0;
                int wid2Idx = 0;
                int dep2Idx = 0;
                int minElIdx = 0;
                int maxElIdx = 0;
                int resIdx = 0;
                int pndIdx = 0;
                int lakeInIdx = 0;
                int lakeOutIdx = 0;
                if (addToRiv1) {
                    rivs1Layer.CreateField(new FieldDefn(Topology._AREAC, OSGeo.OGR.FieldType.OFTReal), 1);
                    rivs1Layer.CreateField(new FieldDefn(Topology._ORDER, OSGeo.OGR.FieldType.OFTInteger), 1);
                    rivs1Layer.CreateField(new FieldDefn(Topology._LEN2, OSGeo.OGR.FieldType.OFTReal), 1);
                    rivs1Layer.CreateField(new FieldDefn(Topology._SLO2, OSGeo.OGR.FieldType.OFTReal), 1);
                    rivs1Layer.CreateField(new FieldDefn(Topology._WID2, OSGeo.OGR.FieldType.OFTReal), 1);
                    rivs1Layer.CreateField(new FieldDefn(Topology._DEP2, OSGeo.OGR.FieldType.OFTReal), 1);
                    rivs1Layer.CreateField(new FieldDefn(Topology._MINEL, OSGeo.OGR.FieldType.OFTReal), 1);
                    rivs1Layer.CreateField(new FieldDefn(Topology._MAXEL, OSGeo.OGR.FieldType.OFTReal), 1);
                    rivs1Layer.CreateField(new FieldDefn(Topology._RESERVOIR, OSGeo.OGR.FieldType.OFTInteger), 1);
                    rivs1Layer.CreateField(new FieldDefn(Topology._POND, OSGeo.OGR.FieldType.OFTInteger), 1);
                    rivs1Layer.CreateField(new FieldDefn(Topology._LAKEIN, OSGeo.OGR.FieldType.OFTInteger), 1);
                    rivs1Layer.CreateField(new FieldDefn(Topology._LAKEOUT, OSGeo.OGR.FieldType.OFTInteger), 1);
                    rivs1Defn = rivs1Layer.GetLayerDefn();
                    linkIdx = rivs1Defn.GetFieldIndex(Topology._LINKNO);
                    chIdx = rivs1Defn.GetFieldIndex(Topology._CHANNEL);
                    areaCIdx = rivs1Defn.GetFieldIndex(Topology._AREAC);
                    orderIdx = rivs1Defn.GetFieldIndex(Topology._ORDER);
                    len2Idx = rivs1Defn.GetFieldIndex(Topology._LEN2);
                    slo2Idx = rivs1Defn.GetFieldIndex(Topology._SLO2);
                    wid2Idx = rivs1Defn.GetFieldIndex(Topology._WID2);
                    dep2Idx = rivs1Defn.GetFieldIndex(Topology._DEP2);
                    minElIdx = rivs1Defn.GetFieldIndex(Topology._MINEL);
                    maxElIdx = rivs1Defn.GetFieldIndex(Topology._MAXEL);
                    resIdx = rivs1Defn.GetFieldIndex(Topology._RESERVOIR);
                    pndIdx = rivs1Defn.GetFieldIndex(Topology._POND);
                    lakeInIdx = rivs1Defn.GetFieldIndex(Topology._LAKEIN);
                    lakeOutIdx = rivs1Defn.GetFieldIndex(Topology._LAKEOUT);
                }
                var table = "gis_channels";
                var clearSQL = "DROP TABLE IF EXISTS " + table;
                db.execNonQuery(clearSQL);
                db.execNonQuery(DBUtils._CHANNELSCREATESQL);
                var time1 = DateTime.Now;
                var floodscape = gv.useLandscapes ? Utils._FLOODPLAIN : Utils._NOLANDSCAPE;
                string sql = "INSERT INTO " + table + " VALUES(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12)";
                IEnumerable<(long, int, int)> generator;
                if (addToRiv1) {
                    // iterate over channels in rivs1 shapefile
                    generator = this.generateChannelsFromShapefile(rivs1Layer, linkIdx, chIdx);
                } else {
                    generator = this.generateChannelsFromTable();
                }
                var toDelete = new List<long>();
                foreach (var (fid, channel, SWATChannel) in generator) {
                    if (gv.useGridModel) {
                        // basin and chBasin are the same
                        subbasin = this.chLinkToChBasin[channel];
                    } else {
                        int chBasin = this.chLinkToChBasin[channel];
                        subbasin = this.chBasinToSubbasin[chBasin];
                    }
                    int SWATBasin;
                    if (this.chLinkInsideLake.ContainsKey(channel)) {
                        SWATBasin = 0;
                    } else if (!this.subbasinToSWATBasin.TryGetValue(subbasin, out SWATBasin)) {
                        SWATBasin = 0;
                    }
                    int lakeOutId;
                    if (!this.chLinkFromLake.TryGetValue(channel, out lakeOutId)) {
                        lakeOutId = 0;
                    }
                    if (!this.downChannels.TryGetValue(channel, out downChannel)) {
                        downChannel = -1;
                    }
                    if (SWATBasin == 0 && (lakeOutId == 0 || downChannel < 0)) {
                        toDelete.Add(fid);
                        continue;
                    }
                    ReachData reachData;
                    double drainAreaHa, length, slopePercent, minEl, maxEl;
                    int order;
                    if (gv.useGridModel) {
                        reachData = this.channelsData[channel];
                        // drain area is a numpy float, so need to coerce, or won't get written to attributes of rivs1
                        drainAreaHa = this.drainAreas[channel] / 10000.0;
                        order = this.strahler[channel];
                        length = this.channelLengths[channel] * gv.mainLengthMultiplier;
                        slopePercent = this.channelSlopes[channel] * 100 * gv.reachSlopeMultiplier / gv.mainLengthMultiplier;
                        minEl = reachData.lowerZ;
                        maxEl = reachData.upperZ;
                    } else {
                        MergedChannelData mergeData;
                        if (!basinMerge.TryGetValue(channel, out mergeData)) {
                            continue;
                        }
                        drainAreaHa = (mergeData.areaC / 10000.0);
                        order = mergeData.order;
                        length = mergeData.length * gv.mainLengthMultiplier;
                        slopePercent = mergeData.slope * 100 * gv.reachSlopeMultiplier / gv.mainLengthMultiplier;
                        minEl = mergeData.minEl;
                        maxEl = mergeData.maxEl;
                    }
                    // possible for channel to be so short it has no pixels draining to it
                    // also no LSU data when channel is outlet from lake in grid model
                    BasinData basinData;
                    if (!basins.TryGetValue(subbasin, out basinData)) { basinData = null; }
                    Dictionary<int, LSUData> channelData;
                    if (basinData is null) {
                        channelData = null;
                    } else if (!basinData.getLsus().TryGetValue(channel, out channelData)) {
                        channelData = null;
                    }
                    LSUData lsuData;
                    if (channelData is null) { lsuData = null; } else if (!channelData.TryGetValue(floodscape, out lsuData)) { lsuData = null; }
                    var drainAreaKm = drainAreaHa / 100;
                    var channelWidth = gv.channelWidthMultiplier * Math.Pow(drainAreaKm, gv.channelWidthExponent);
                    wid2Data[SWATChannel] = channelWidth;
                    var channelDepth = gv.channelDepthMultiplier * Math.Pow(drainAreaKm, gv.channelDepthExponent);
                    int rid, pid;
                    double midPointX, midPointY;
                    if (lsuData is null) {
                        rid = 0;
                        pid = 0;
                        reachData = this.channelsData[channel];
                        midPointX = (reachData.lowerX + reachData.upperX) / 2;
                        midPointY = (reachData.lowerY + reachData.upperY) / 2;
                    } else {
                        rid = this.getReservoirId(lsuData);
                        pid = this.getPondId(lsuData);
                        midPointX = lsuData.midPointX;
                        midPointY = lsuData.midPointY;
                    }
                    var midll = this.pointToLatLong(new double[3] { midPointX, midPointY, 0 });
                    var midLat = midll[1];
                    var midLong = midll[0];
                    if (rid == 0 && pid == 0) {
                        // omit from gis_channels channels which have become reservoirs or ponds
                        db.execNonQuery(sql, new object[] { SWATChannel, SWATBasin, drainAreaHa, order, length, slopePercent, channelWidth, channelDepth, minEl, maxEl, midLat, midLong });
                        this.db.addKey(table, SWATChannel);
                    }
                    if (addToRiv1) {
                        int lakeInId;
                        if (!this.chLinkIntoLake.TryGetValue(channel, out lakeInId)) {
                            lakeInId = 0;
                        }
                        reach = rivs1Layer.GetFeature(fid);
                        reach.SetField(areaCIdx, drainAreaHa);
                        reach.SetField(orderIdx, order);
                        reach.SetField(len2Idx, length);
                        reach.SetField(slo2Idx, slopePercent);
                        reach.SetField(wid2Idx, channelWidth);
                        reach.SetField(dep2Idx, channelDepth);
                        reach.SetField(minElIdx, minEl);
                        reach.SetField(maxElIdx, maxEl);
                        reach.SetField(resIdx, rid);
                        reach.SetField(pndIdx, pid);
                        reach.SetField(lakeInIdx, lakeInId);
                        reach.SetField(lakeOutIdx, lakeOutId);
                        rivs1Layer.SetFeature(reach);
                    }
                }
                var time2 = DateTime.Now;
                Utils.loginfo(string.Format("Writing gis_channels table took {0} seconds", Convert.ToInt32(time2.Subtract(time1).TotalSeconds)));
                //this.db.hashDbTable(conn, table);
                foreach (long fid in toDelete) {
                    rivs1Layer.DeleteFeature(fid);
                }
                // make copy as template for stream results
                Utils.copyShapefile(rivs1File, Parameters._RIVS, gv.resultsDir);
                var rivFile = Utils.join(gv.resultsDir, Parameters._RIVS + ".shp");
                var rivJson = Utils.join(gv.resultsDir, Parameters._RIVS + ".json");
                await Utils.removeLayer(rivJson);
                var parms = Geoprocessing.MakeValueArray(rivFile, rivJson);
                var environment = Geoprocessing.MakeEnvironmentArray(overwriteoutput: true);
                await Geoprocessing.ExecuteToolAsync("conversion.FeaturesToJSON", parms, environment, null, null, GPExecuteToolFlags.None);

                // remove channels that are reservoirs or ponds
                var exp = string.Format("{0} > 0 OR {1} > 0", Topology._RESERVOIR, Topology._POND);
                using (var rivDs = Ogr.Open(rivFile, 1)) {
                    var rivLayer = rivDs.GetLayerByIndex(0);
                    rivLayer.SetAttributeFilter(exp);
                    rivLayer.ResetReading();
                    do {
                        reach = rivLayer.GetNextFeature();
                        if (reach is null) { break; }
                        rivLayer.DeleteFeature(reach.GetFID());
                    } while (reach is not null);
                    rivLayer.SetAttributeFilter(null);
                    // leave only the Channel, ChannelR and Subbasin attributes
                    Topology.removeFields(rivLayer, new List<string> {
                Topology._CHANNEL,
                Topology._CHANNELR,
                Topology._SUBBASIN
            }, rivFile, this.isBatch);
                    // Add PenWidth field to stream results template
                    rivLayer.CreateField(new FieldDefn(Topology._PENWIDTH, OSGeo.OGR.FieldType.OFTReal), 1);
                    this.setPenWidth(wid2Data, 1.0, 4.0, rivLayer);
                    if (gv.useGridModel) {
                        rivs1Layer.CreateField(new FieldDefn(Topology._PENWIDTH, OSGeo.OGR.FieldType.OFTReal), 1);
                        this.setPenWidth(wid2Data, 0.2, 2.0, rivs1Layer);
                    }
                    var subLayer = gv.useGridModel ? Utils.getLayerByLegend(Utils._GRIDLEGEND) : Utils.getLayerByLegend(FileTypes.legend(FileTypes._SUBBASINS));
                    if (gv.useGridModel) {
                        if (gv.existingWshed) {
                            ft = FileTypes._DRAINSTREAMS;
                        } else {
                            ft = FileTypes._GRIDSTREAMS;
                        }
                    } else {
                        ft = FileTypes._CHANNELREACHES;
                    }
                    var rivs1ArcLayer = (await Utils.getLayerByFilename(rivs1File, ft, gv, subLayer, Utils._WATERSHED_GROUP_NAME)).Item1 as FeatureLayer;
                    if (gv.useGridModel) {
                        FileTypes.colourStreams(rivs1ArcLayer, Topology._PENWIDTH, Topology._AREAC);
                    } else {
                        // hide channel layer
                        var channelArcLayer = (await Utils.getLayerByFilename(channelFile, ft, gv, subLayer, Utils._WATERSHED_GROUP_NAME)).Item1;
                        if (channelArcLayer is not null) {
                            Utils.setLayerVisibility(channelArcLayer, false);
                        }
                        if (this.upstreamFromInlets.Count > 0) {
                            this.replaceStreamLayer(gv);
                        }
                    }
                    return rivs1Layer;
                }
            }
        }


        // Yield (feature id, channel, swatChammel) tuples from rivs1.shp.
        public IEnumerable<(long, int, int)> generateChannelsFromShapefile(OSGeo.OGR.Layer layer, int linkIdx, int chIdx) {
            long count = layer.GetFeatureCount(1);
            for (long i = 0; i < count; i++) {
                var feature = layer.GetFeature(i);
                yield return (i, feature.GetFieldAsInteger(linkIdx), feature.GetFieldAsInteger(chIdx));
            }
        }

        // Yield (feature id, channel, swatChammel) tuples from tables.
        public IEnumerable<(long, int, int)> generateChannelsFromTable() {
            foreach (var (channel, SWATChannel) in this.channelToSWATChannel) {
                yield return (0, channel, SWATChannel);
            }
        }

        // Copy stream layer, remove streams upstream from inlets, and replace stream layer.
        public async void replaceStreamLayer(GlobalVars gv) {
            var streamLayer = (await Utils.getLayerByFilename(gv.streamFile, FileTypes._STREAMREACHES, gv, null, null)).Item1;
            if (streamLayer is not null) {
                var @base = System.IO.Path.ChangeExtension(System.IO.Path.GetFileName(gv.streamFile), null);
                Utils.copyShapefile(gv.streamFile, @base + "act", gv.shapesDir);
                var actStreamFile = Utils.join(gv.shapesDir, @base + "act.shp");
                using (var actStreamDs = Ogr.Open(actStreamFile, 1)) {
                    var actStreamLayer = actStreamDs.GetLayerByIndex(0);
                    var basinIdx = this.getIndex(actStreamLayer, Topology._WSNO);
                    if (basinIdx < 0) {
                        return;
                    }
                    actStreamLayer.ResetReading();
                    OSGeo.OGR.Feature reach;
                    do {
                        reach = actStreamLayer.GetNextFeature();
                        if (reach is null) { break; }
                        if (this.upstreamFromInlets.Contains(reach.GetFieldAsInteger(basinIdx))) {
                            actStreamLayer.DeleteFeature(reach.GetFID());
                        }
                    } while (reach != null);
                    var subLayer = streamLayer;
                    var actStreamArcLayer = (await Utils.getLayerByFilename(actStreamFile, FileTypes._STREAMREACHES, gv, subLayer, Utils._WATERSHED_GROUP_NAME, true)).Item1;
                    Utils.setLayerVisibility(streamLayer, false);
                }
            }
        }

        // Return reservoir id, if any, else 0.
        public virtual int getReservoirId(LSUData lsuData) {
            if (lsuData.waterBody is not null && lsuData.waterBody.isReservoir()) {
                return lsuData.waterBody.id;
            }
            return 0;
        }

        // Return pond id, if any, else 0.
        public virtual int getPondId(LSUData lsuData) {
            if (lsuData.waterBody is not null && lsuData.waterBody.isPond()) {
                return lsuData.waterBody.id;
            }
            return 0;
        }

        // Generate and return map of channel to MergedChannelData.
        public virtual Dictionary<int, MergedChannelData> mergeChannelData(Dictionary<int, int> mergeChannels) {
            ReachData channelData;
            // first pass: collect data for unmerged channels
            var mergedChannelData = new Dictionary<int, MergedChannelData>();
            foreach (var channel in this.channelToSWATChannel.Keys) {
                if (!mergeChannels.ContainsKey(channel)) {
                    channelData = this.channelsData[channel];
                    mergedChannelData[channel] = new MergedChannelData(this.drainAreas[channel], this.strahler[channel], this.channelLengths[channel], this.channelSlopes[channel], channelData.lowerZ, channelData.upperZ);
                }
            }
            // second pass: Add data for merged channels
            foreach (var (source, target) in mergeChannels) {
                channelData = this.channelsData[source];
                var final = this.finalTarget(target, mergeChannels);
                mergedChannelData[final].add(this.drainAreas[source], this.strahler[source], this.channelLengths[source], this.channelSlopes[source], channelData.lowerZ, channelData.upperZ);
            }
            return mergedChannelData;
        }

        // Find final target of merges.
        public virtual int finalTarget(int target, Dictionary<int, int> mergeChannels) {
            int nxt;
            if (!mergeChannels.TryGetValue(target, out nxt)) {
                return target;
            } else {
                return this.finalTarget(nxt, mergeChannels);
            }
        }

        // Find downstream channel from start, skipping merged channels, and return it.
        public virtual int finalDownstream(int start, Dictionary<int, int> mergeChannels) {
            var chLink1 = this.finalTarget(start, mergeChannels);
            return this.finalTarget(this.getDownChannel(chLink1), mergeChannels);
        }

        // Add channels, lakes, basins, aquifers, point sources, reservoirs, inlets and outlets to main gis_routing table.
        public virtual bool routeChannelsOutletsAndBasins(
            Dictionary<int, BasinData> basins,
            Dictionary<int, int> mergedChannels,
            IEnumerable<int> mergees,
            List<(int, int)> extraPoints,
            GlobalVars gv) {
            Dictionary<int, (int, double[])> channelToPtSrc = null;
            Dictionary<int, (int, double[])> channelToInlet;
            var chCat = "CH";
            var subbasinCat = "SUB";
            var ptCat = "PT";
            var resCat = "RES";
            var pondCat = "PND";
            var wetlandCat = "WETL";
            var xCat = "X";
            // first associate any inlets, point sources and reservoirs with appropriate channels
            if (gv.useGridModel) {
                // no merging
                channelToInlet = this.chLinkToInlet;
                channelToPtSrc = this.chLinkToPtSrc;
            } else {
                channelToInlet = new Dictionary<int, (int, double[])>();
                foreach (var (subbasin, inlet) in this.inlets) {
                    // find an inlet channel for this subbasin
                    var found = false;
                    foreach (var (channel, data) in this.channelsData) {
                        int chBasin;
                        if (this.chLinkToChBasin.TryGetValue(channel, out chBasin)) {
                            int subbsn;
                            if (this.chBasinToSubbasin.TryGetValue(chBasin, out subbsn)) {
                                double[] source = new double[3] { data.upperX, data.upperY, 0 };
                                if (subbasin == subbsn && Topology.coincidentPoints(source, inlet.Item2, this.xThreshold, this.yThreshold)) {
                                    channelToInlet[this.finalTarget(channel, mergedChannels)] = inlet;
                                    found = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (!found) {
                        Utils.error(string.Format("Failed to find channel for inlet to subbasin {0}", subbasin), gv.isBatch);
                    }
                }
                // map point sources to the unmerged channels they drain into
                channelToPtSrc = new Dictionary<int, (int, double[])>();
                foreach (var (channel0, ptsrcPair) in this.chLinkToPtSrc) {
                    channelToPtSrc[channel0] = ptsrcPair;
                    //Utils.loginfo('Channel {0} merged to {1} has point source {2}', channel, self.finalTarget(channel, mergedChannels), ptsrc.Item1))
                    // Add point sources at stream sources
                }
            }
            foreach (var (channel, ptsrcPair) in this.chPointSources) {
                if (!channelToPtSrc.ContainsKey(channel) && !mergees.Contains(channel) && !this.chLinkInsideLake.ContainsKey(channel) && !(gv.useGridModel && this.chLinkFromLake.ContainsKey(channel))) {
                    // not already has a point, not merged, not inslide lake 
                    channelToPtSrc[channel] = ptsrcPair;
                }
            }
            // map channels to water bodies that replace them as drainage targets
            // and water bodies to channels they drain into
            var floodscape = gv.useLandscapes ? Utils._FLOODPLAIN : Utils._NOLANDSCAPE;
            var channelToWater = new Dictionary<int, (int, WaterRole)>();
            foreach (var basinData in basins.Values) {
                foreach (var (channel, channelData) in basinData.getLsus()) {
                    LSUData lsuData;
                    if (channelData.TryGetValue(floodscape, out lsuData)) {
                        if (lsuData is not null && lsuData.waterBody is not null && !lsuData.waterBody.isUnknown()) {
                            channelToWater[channel] = (lsuData.waterBody.id, lsuData.waterBody.waterRole);
                        }
                    }
                }
            }
            try {
                var routedPoints = new List<int>();
                var routedWater = new List<int>();
                var routedChannels = new List<int>();
                //                 routedSubbasins = []
                int wid;
                WaterRole role;
                string wCat;
                int ptsrcId;
                double[] ptsrc;
                foreach (var (channel, SWATChannel) in this.channelToSWATChannel) {
                    if (mergedChannels.ContainsKey(channel)) {
                        // all that is needed is to map its point source to the merge target
                        ptsrcId = 0;

                        (int, double[]) pair;
                        if (channelToPtSrc.TryGetValue(channel, out pair))
                            ptsrcId = pair.Item1;
                        ptsrc = pair.Item2;
                        if (ptsrc is not null) {
                            if (!routedPoints.Contains(ptsrcId)) {
                                int finalChannel = this.finalTarget(channel, mergedChannels);
                                (int, WaterRole) val;
                                if (channelToWater.TryGetValue(finalChannel, out val)) {
                                    wid = val.Item1;
                                    role = val.Item2;
                                    if (wid >= 0) {
                                        wCat = role == WaterRole._RESERVOIR ? resCat : role == WaterRole._POND ? pondCat : wetlandCat;
                                        this.db.addToRouting(ptsrcId, ptCat, wid, wCat, Topology._TOTAL, 100);
                                    } else {
                                        var finalSWATChannel = this.channelToSWATChannel[finalChannel];
                                        this.db.addToRouting(ptsrcId, ptCat, finalSWATChannel, chCat, Topology._TOTAL, 100);
                                    }
                                    routedPoints.Add(ptsrcId);
                                }
                            }
                        }
                        continue;
                    }
                    // if channel is lake outflow
                    // if main outflow, route lake to outlet and outlet to channel
                    // else route 0% of lake to channel
                    int outLakeId;
                    if (this.chLinkFromLake.TryGetValue(channel, out outLakeId)) {
                        if (outLakeId >= 0) {
                            var lakeData = this.lakesData[outLakeId];
                            wCat = lakeData.waterRole == WaterRole._RESERVOIR ? resCat : lakeData.waterRole == WaterRole._POND ? pondCat : wetlandCat;
                            if (channel == lakeData.outChLink) {
                                // main outlet
                                int outletId = lakeData.outPoint.Item2;
                                this.db.addToRouting(outLakeId, wCat, outletId, ptCat, Topology._TOTAL, 100);
                                if (!routedPoints.Contains(outletId)) {
                                    if (gv.useGridModel) {
                                        int downChannel;
                                        if (!this.downChannels.TryGetValue(channel, out downChannel)) {
                                            downChannel = -1;
                                        }
                                        if (downChannel < 0) {
                                            // we have an internal lake exit: route outlet id to watershed exit
                                            this.db.addToRouting(outletId, ptCat, 0, xCat, Topology._TOTAL, 100);
                                        } else {
                                            this.db.addToRouting(outletId, ptCat, SWATChannel, chCat, Topology._TOTAL, 100);
                                        }
                                    }
                                }
                                routedPoints.Add(outletId);
                            } else {
                                // other outlet
                                this.db.addToRouting(outLakeId, wCat, SWATChannel, chCat, Topology._TOTAL, 0);
                            }
                        }
                    }
                    // check if channel routes into lake
                    int inLakeId;
                    if (this.chLinkIntoLake.TryGetValue(channel, out inLakeId)) {
                        if (inLakeId >= 0) {
                            // route its point source to the channel
                            (int, double[]) pair;
                            if (channelToPtSrc.TryGetValue(channel, out pair)) {
                                ptsrcId = pair.Item1;
                                ptsrc = pair.Item2;
                                if (ptsrc is not null) {
                                    if (!routedPoints.Contains(ptsrcId)) {
                                        this.db.addToRouting(ptsrcId, ptCat, SWATChannel, chCat, Topology._TOTAL, 100);
                                        routedPoints.Add(ptsrcId);
                                    }
                                }
                            }
                            // route the channel into its outlet, and the outlet into the lake
                            var lakeData = this.lakesData[inLakeId];
                            int outletId = lakeData.inChLinks[channel].Item1;
                            wCat = lakeData.waterRole == WaterRole._RESERVOIR ? resCat : lakeData.waterRole == WaterRole._POND ? pondCat : wetlandCat;
                            if (!routedChannels.Contains(SWATChannel)) {
                                this.db.addToRouting(SWATChannel, chCat, outletId, ptCat, Topology._TOTAL, 100);
                                routedChannels.Add(SWATChannel);
                            }
                            if (!routedPoints.Contains(outletId)) {
                                this.db.addToRouting(outletId, ptCat, inLakeId, wCat, Topology._TOTAL, 100);
                                routedPoints.Add(outletId);
                            }
                            if (!gv.useGridModel) {
                                continue;
                            }
                        }
                    }
                    int subbasin = -1;
                    int chBasin = -1;
                    if (gv.useGridModel) {
                        subbasin = this.chLinkToChBasin[channel];
                    } else {
                        if (!this.chLinkToChBasin.TryGetValue(channel, out chBasin)) {
                            chBasin = -1;
                        }
                        if (!this.chBasinToSubbasin.TryGetValue(chBasin, out subbasin)) {
                            subbasin = -1;
                        }
                    }
                    int SWATBasin;
                    if (!this.subbasinToSWATBasin.TryGetValue(subbasin, out SWATBasin)) {
                        SWATBasin = 0;
                    }
                    if (SWATBasin == 0) {
                        continue;
                    }
                    // if channel is inside lake ignore it unless a lake outflow
                    if (this.chLinkInsideLake.ContainsKey(channel) && outLakeId < 0) {
                        continue;
                    }
                    int dsChannel = this.finalDownstream(channel, mergedChannels);
                    int dsSWATChannel;
                    if (!this.channelToSWATChannel.TryGetValue(dsChannel, out dsSWATChannel)) {
                        dsSWATChannel = 0;
                    }
                    (int, WaterRole) result;
                    if (!channelToWater.TryGetValue(channel, out result)) {
                        wid = -1;
                        role = WaterRole._NONE;
                    } else {
                        wid = result.Item1;
                        role = result.Item2;
                    }
                    wCat = role == WaterRole._RESERVOIR ? resCat : role == WaterRole._POND ? pondCat : wetlandCat;
                    int inletId;
                    double[] inletPt;
                    (int, double[]) reslt;
                    if (!channelToInlet.TryGetValue(channel, out reslt)) {
                        inletId = -1;
                        inletPt = null;
                    } else {
                        inletId = reslt.Item1;
                        inletPt = reslt.Item2;
                    }
                    if (inletPt is not null) {
                        // route inlet to channel or water
                        if (wid >= 0) {
                            this.db.addToRouting(inletId, ptCat, wid, wCat, Topology._TOTAL, 100);
                        } else {
                            this.db.addToRouting(inletId, ptCat, SWATChannel, chCat, Topology._TOTAL, 100);
                        }
                    }
                    (pointId, _, var outletChannels) = this.outlets[subbasin];
                    if (outletChannels.Contains(channel) || gv.useGridModel) {
                        // subbasin outlet: channel routes to outlet point of subbasin; outlet routes to downstream channel
                        // but with some exceptions:
                        // - if the channel is replaced by a reservoir, this is routed to the outlet instead
                        // - if subbasin has an extra point source, this is added to its outlet channel or reservoir
                        if (gv.useGridModel) {
                            (int, double[]) tmp;
                            if (!channelToPtSrc.TryGetValue(channel, out tmp)) {
                                ptsrcId = -1;
                                ptsrc = null;
                            } else {
                                ptsrcId = tmp.Item1;
                                ptsrc = tmp.Item2;
                            }
                        } else {
                            ptsrcId = -1;
                            ptsrc = null;
                            //                         if ptsrc is None:
                            //                             ptsrc = self.extraPtSrcs.TryGetValue(subbasin, None)
                        }
                        if (ptsrc is not null) {
                            // route it to the outlet channel, unless already routed
                            if (!routedPoints.Contains(ptsrcId)) {
                                if (wid < 0) {
                                    this.db.addToRouting(ptsrcId, ptCat, SWATChannel, chCat, Topology._TOTAL, 100);
                                } else {
                                    this.db.addToRouting(ptsrcId, ptCat, wid, wCat, Topology._TOTAL, 100);
                                }
                                routedPoints.Add(ptsrcId);
                            }
                        }
                        if (wid >= 0) {
                            // need to check if this is a reservoir that is continued in the downstream subbasin
                            // to make sure we only route it once, and at its final downstream end
                            int widDown;
                            (int, WaterRole) temp1;
                            if (!channelToWater.TryGetValue(dsChannel, out temp1)) {
                                widDown = -1;
                            } else {
                                widDown = temp1.Item1;
                            }
                            if (!routedWater.Contains(wid) && wid != widDown) {
                                int waterId;
                                (int, double[], WaterRole) temp2;
                                if (!this.chLinkToWater.TryGetValue(channel, out temp2)) {
                                    waterId = -1;
                                } else {
                                    waterId = temp2.Item1;
                                }
                                // route water to water point and water point to outlet
                                int ptId;
                                if (waterId < 0) {
                                    (int, int, double[]) temp3;
                                    if (!this.foundReservoirs.TryGetValue(channel, out temp3)) {
                                        waterId = -1;
                                        ptId = -1;
                                    } else {
                                        waterId = temp3.Item1;
                                        ptId = temp3.Item2;
                                    }
                                } else {
                                    // it is safe to use same id for reservoir and reservoir outlet point when
                                    // using DSNODEID from inlets/outlets file
                                    ptId = waterId;
                                }
                                if (waterId < 0) {
                                    Utils.error(string.Format("Cannot find water point for channel {0}", SWATChannel), gv.isBatch);
                                } else {
                                    this.db.addToRouting(wid, wCat, ptId, ptCat, Topology._TOTAL, 100);
                                    if (!routedPoints.Contains(ptId)) {
                                        this.db.addToRouting(ptId, ptCat, pointId, ptCat, Topology._TOTAL, 100);
                                        routedPoints.Add(ptId);
                                    }
                                    routedWater.Add(wid);
                                }
                            }
                        } else if (!routedChannels.Contains(SWATChannel)) {
                            this.db.addToRouting(SWATChannel, chCat, pointId, ptCat, Topology._TOTAL, 100);
                            routedChannels.Add(SWATChannel);
                        }
                        if (!routedPoints.Contains(pointId)) {
                            if (dsSWATChannel > 0) {
                                int widDown;
                                WaterRole roleDown;
                                (int, WaterRole) temp4;
                                if (!channelToWater.TryGetValue(dsChannel, out temp4)) {
                                    widDown = -1;
                                    roleDown = WaterRole._NONE;
                                } else {
                                    widDown = temp4.Item1;
                                    roleDown = temp4.Item2;
                                }
                                if (widDown >= 0) {
                                    wCat = roleDown == WaterRole._RESERVOIR ? resCat : roleDown == WaterRole._POND ? pondCat : wetlandCat;
                                    this.db.addToRouting(pointId, ptCat, widDown, wCat, Topology._TOTAL, 100);
                                } else {
                                    this.db.addToRouting(pointId, ptCat, dsSWATChannel, chCat, Topology._TOTAL, 100);
                                }
                            } else {
                                // watershed outlet: mark point as category X
                                this.db.addToRouting(pointId, ptCat, 0, xCat, Topology._TOTAL, 100);
                            }
                            routedPoints.Add(pointId);
                        }
                    } else {
                        // channel and downstream channel within a subbasin: 
                        // channel routes to downstream channel unless it is replaced by water
                        // and if it has a point source this is added
                        Debug.Assert(dsSWATChannel > 0, string.Format("Channel {0} has no downstream channel", channel));
                        int widDown;
                        WaterRole roleDown;
                        (int, WaterRole) temp4;
                        if (!channelToWater.TryGetValue(dsChannel, out temp4)) {
                            widDown = -1;
                            roleDown = WaterRole._NONE;
                        } else {
                            widDown = temp4.Item1;
                            roleDown = temp4.Item2;
                        }
                        if (wid >= 0) {
                            if (!routedWater.Contains(wid)) {
                                int waterId;
                                int ptId;
                                if (widDown >= 0) {
                                    // if wid == widDown wid is only a part water body
                                    // and we will eventually route the one downstream
                                    if (wid != widDown) {
                                        // route water to water point and water point to ridDown
                                        (int, double[], WaterRole) temp5;
                                        if (!this.chLinkToWater.TryGetValue(channel, out temp5)) {
                                            waterId = -1;
                                        } else {
                                            waterId = temp5.Item1;
                                        }
                                        if (waterId < 0) {
                                            (int, int, double[]) temp6;
                                            if (!this.foundReservoirs.TryGetValue(channel, out temp6)) {
                                                waterId = -1;
                                                ptId = -1;
                                            } else {
                                                waterId = temp6.Item1;
                                                ptId = temp6.Item2;
                                            }
                                        } else {
                                            // it is safe to use same id for reservoir and reservoir outlet point when
                                            // using DSNODEID from inlets/outlets file
                                            ptId = waterId;
                                        }
                                        if (waterId < 0) {
                                            Utils.error(string.Format("Cannot find water point for channel {0}", SWATChannel), gv.isBatch);
                                        } else {
                                            this.db.addToRouting(wid, wCat, ptId, ptCat, Topology._TOTAL, 100);
                                            if (!routedPoints.Contains(ptId)) {
                                                wCat = roleDown == WaterRole._RESERVOIR ? resCat : roleDown == WaterRole._POND ? pondCat : wetlandCat;
                                                this.db.addToRouting(ptId, ptCat, widDown, wCat, Topology._TOTAL, 100);
                                                routedPoints.Add(ptId);
                                            }
                                            routedWater.Add(wid);
                                        }
                                    }
                                } else {
                                    // route water to water point and water point to downstream channel
                                    (int, double[], WaterRole) temp5;
                                    if (!this.chLinkToWater.TryGetValue(channel, out temp5)) {
                                        waterId = -1;
                                    } else {
                                        waterId = temp5.Item1;
                                    }
                                    if (waterId < 0) {
                                        (int, int, double[]) temp6;
                                        if (!this.foundReservoirs.TryGetValue(channel, out temp6)) {
                                            waterId = -1;
                                            ptId = -1;
                                        } else {
                                            waterId = temp6.Item1;
                                            ptId = temp6.Item2;
                                        }
                                    } else {
                                        // it is safe to use same id for water and water outlet point when
                                        // using DSNODEID from inlets/outlets file
                                        ptId = waterId;
                                    }
                                    if (waterId < 0) {
                                        Utils.error(string.Format("Cannot find water point for channel {0}", SWATChannel), gv.isBatch);
                                    } else {
                                        this.db.addToRouting(wid, wCat, ptId, ptCat, Topology._TOTAL, 100);
                                        if (!routedPoints.Contains(ptId)) {
                                            this.db.addToRouting(ptId, ptCat, dsSWATChannel, chCat, Topology._TOTAL, 100);
                                            routedPoints.Add(ptId);
                                        }
                                        routedWater.Add(wid);
                                    }
                                }
                            }
                        } else if (!routedChannels.Contains(SWATChannel)) {
                            if (widDown >= 0) {
                                // insert an outlet point so that channel's contribution to reservoir 
                                // is included in outputs
                                this.pointId += 1;
                                extraPoints.Add((channel, this.pointId));
                                this.db.addToRouting(SWATChannel, chCat, this.pointId, ptCat, Topology._TOTAL, 100);
                                wCat = roleDown == WaterRole._RESERVOIR ? resCat : roleDown == WaterRole._POND ? pondCat : wetlandCat;
                                this.db.addToRouting(this.pointId, ptCat, widDown, wCat, Topology._TOTAL, 100);
                                routedPoints.Add(this.pointId);
                            } else {
                                this.db.addToRouting(SWATChannel, chCat, dsSWATChannel, chCat, Topology._TOTAL, 100);
                            }
                            routedChannels.Add(SWATChannel);
                        }
                    }
                    // also route point source, if any, to channel  or water
                    (int, double[]) temp;
                    if (!channelToPtSrc.TryGetValue(channel, out temp)) {
                        ptsrcId = -1;
                        ptsrc = null;
                    } else {
                        ptsrcId = temp.Item1;
                        ptsrc = temp.Item2;
                    }
                    if (ptsrc is not null) {
                        if (!routedPoints.Contains(ptsrcId)) {
                            if (wid > 0) {
                                this.db.addToRouting(ptsrcId, ptCat, wid, wCat, Topology._TOTAL, 100);
                            } else {
                                this.db.addToRouting(ptsrcId, ptCat, SWATChannel, chCat, Topology._TOTAL, 100);
                            }
                            routedPoints.Add(ptsrcId);
                        }
                    }
                }
                // route reservoir and pond lakes without outlet channels to main outlet points
                foreach (var (lakeId, lakeData) in this.lakesData) {
                    if (new HashSet<WaterRole> {
                            WaterRole._RESERVOIR,
                            WaterRole._POND
                        }.Contains(lakeData.waterRole) && lakeData.outChLink == -1) {
                        (var subbasin, var lakeOutletId, _, _) = lakeData.outPoint;
                        (var outletId, _, _) = this.outlets[subbasin];
                        wCat = lakeData.waterRole == WaterRole._RESERVOIR ? resCat : lakeData.waterRole == WaterRole._POND ? pondCat : wetlandCat;
                        // route the lake to its lake outlet, the lake outlet to the main outlet, and mark main outlet as category X 
                        this.db.addToRouting(lakeId, wCat, lakeOutletId, ptCat, Topology._TOTAL, 100);
                        if (!routedPoints.Contains(lakeOutletId)) {
                            this.db.addToRouting(lakeOutletId, ptCat, outletId, ptCat, Topology._TOTAL, 100);
                            routedPoints.Add(lakeOutletId);
                        }
                        if (!routedPoints.Contains(outletId)) {
                            this.db.addToRouting(outletId, ptCat, 0, xCat, Topology._TOTAL, 100);
                            routedPoints.Add(outletId);
                        }
                    }
                }
                // route subbasin to outlet points
                // or to lake if outlet in lake
                foreach (var (subbasin, (pointId, _, chLinks)) in this.outlets) {
                    int SWATBasin;
                    if (!this.subbasinToSWATBasin.TryGetValue(subbasin, out SWATBasin)) {
                        SWATBasin = 0;
                    }
                    if (SWATBasin == 0) {
                        continue;
                    }
                    // with grid models chLinks is always a singleton list
                    if (gv.useGridModel) {
                        if (this.chLinkInsideLake.ContainsKey(chLinks.First()) || this.chLinkFromLake.ContainsKey(chLinks.First())) {
                            continue;
                        }
                    }
                    int lakeId;
                    if (!this.outletsInLake.TryGetValue(subbasin, out lakeId)) {
                        lakeId = -1;
                    }
                    // if one chLink is inside lake, all will be, since they share their outlet point
                    if (lakeId < 0) {
                        if (!this.chLinkInsideLake.TryGetValue(chLinks.First(), out lakeId)) {
                            lakeId = -1;
                        }
                    }
                    if (lakeId < 0) {
                        this.db.addToRouting(SWATBasin, subbasinCat, pointId, ptCat, Topology._TOTAL, 100);
                    } else {
                        var lakeData = this.lakesData[lakeId];
                        wCat = lakeData.waterRole == WaterRole._RESERVOIR ? resCat : lakeData.waterRole == WaterRole._POND ? pondCat : wetlandCat;
                        this.db.addToRouting(SWATBasin, subbasinCat, lakeId, wCat, Topology._TOTAL, 100);
                    }
                }
                return true;
            } catch (Exception ex) {
                Utils.loginfo(string.Format("Routing channels, outlets and subbasins failed: {0}", ex.Message));
                return false;
            }
        }

        // Remove fields other than keepFieldNames from shapefile fileName with layer.

        public static void removeFields(OSGeo.OGR.Layer layer, List<string> keepFieldNames, string fileName, bool isBatch) {
            var toDelete = new List<object>();
            var defn = layer.GetLayerDefn();
            int fieldCount = defn.GetFieldCount();
            var keepLower = (from name in keepFieldNames
                             select name.ToLower()).ToList();
            for (int idx = 0; idx < fieldCount; idx++) {
                var field = layer.GetLayerDefn().GetFieldDefn(idx);
                if (field is null) {
                    continue;
                }
                var fieldName = field.GetName().ToLower();
                if (!keepLower.Contains(fieldName)) { 
                    layer.DeleteField(idx);
                    // don't skip next field, which may have a reduced index
                    idx--;
                    fieldCount--;
                }
            }
        }

        // Scale wid2 data to a .. b and write to layer.
        public virtual void setPenWidth(Dictionary<int, double> data, double a, double b, OSGeo.OGR.Layer layer) {
            Func<double, double> fun;
            var @base = a;
            var mult = b - a;
            var minW = double.PositiveInfinity;
            var maxW = 0.0;
            foreach (var val in data.Values) {
                minW = Math.Min(minW, val);
                maxW = Math.Max(maxW, val);
            }
            if (maxW > minW) {
                // guard against division by zero
                var rng = maxW - minW;
                fun = x => (x - minW) * mult / rng + @base;
            } else {
                fun = _ => 1.0;
            }
            var chIdx = layer.FindFieldIndex(Topology._CHANNEL, 1);
            if (chIdx < 0) {
                Utils.error(string.Format("Cannot find {0} field in channels results template", Topology._CHANNEL), this.isBatch);
                return;
            }
            var penIdx = layer.FindFieldIndex(Topology._PENWIDTH, 1);
            if (penIdx < 0) {
                Utils.error(string.Format("Cannot find {0} field in channels results template", Topology._PENWIDTH), this.isBatch);
                return;
            }
            layer.ResetReading();
            OSGeo.OGR.Feature f;
            do {
                f = layer.GetNextFeature();
                if (f is null) { break; }
                var ch = f.GetFieldAsInteger(chIdx);
                double width; 
                if (!data.TryGetValue(ch, out width)) {
                    width = minW;
                }
                f.SetField(penIdx, fun(width));
                layer.SetFeature(f);
            } while (f is not null);
        }

        // 
        //         Make file like D8 contributing area but with heightened values at subbasin outlets.  
        //         
        //         Return -1 if cannot make the file.
        //         
        public async Task<int> makeOutletThresholds(GlobalVars gv) {
            Debug.Assert(File.Exists(gv.demFile));
            var demBase = System.IO.Path.ChangeExtension(gv.demFile, null);
            if (!Utils.isUpToDate(gv.demFile, gv.ad8File)) {
                // Probably using existing watershed but switched tabs in delineation form
                // At any rate, cannot calculate flow paths
                Utils.loginfo("ad8 file not found or out of date");
                return -1;
            }
            Debug.Assert(this.outlets.Count > 0);
            int threshold;
            gv.hd8File = demBase + "hd8.tif";
            await Utils.removeLayerAndFiles(gv.hd8File);
            Debug.Assert(!File.Exists(gv.hd8File));
            using (var ad8Ds = Gdal.Open(gv.ad8File, Access.GA_ReadOnly)) {
                Band ad8Band = ad8Ds.GetRasterBand(1);
                double[] transform = new double[6];
                ad8Ds.GetGeoTransform(transform);
                // calculate maximum contributing area at an outlet point
                var maxContrib = 0.0;
                foreach (var (_, pt, _) in this.outlets.Values) {
                    var contrib = Topology.valueAtPoint(pt, transform, ad8Band);
                    if (!double.IsNaN(contrib)) {
                        maxContrib = Math.Max(maxContrib, contrib);
                    }
                }
                threshold = Convert.ToInt32(2 * maxContrib);
                // copy ad8 to hd8 and then set outlet point values to threshold
                var driver = Gdal.GetDriverByName("GTiff");
                using (var hd8Ds = driver.CreateCopy(gv.hd8File, ad8Ds, 0, null, null, "")) {
                    if (hd8Ds is null) {
                        Utils.error(string.Format("Failed to create hd8 file {0}", gv.hd8File), this.isBatch);
                        return -1;
                    }
                    Utils.copyPrj(gv.ad8File, gv.hd8File);
                    var hd8Band = hd8Ds.GetRasterBand(1);
                    int[] arr = new int[1];
                    arr[0] = threshold;
                    foreach (var (_, pt, _) in this.outlets.Values) {
                        (int x, int y) = Topology.projToCell(pt[0], pt[1], transform);
                        var err = hd8Band.WriteRaster(x, y, 1, 1, arr, 1, 1, 0, 0);
                        if (err != CPLErr.CE_None) {
                            Utils.error($"Failed to write value {threshold} to hd8 file at ({y}, {x})", gv.isBatch);
                            return -1;
                        }
                    }
                    //hd8Ds = null;
                }
            }
            return threshold;
        }

        //// Use func as a function to calulate outFile from file1.
        ////         
        ////         Valid input data values have fun1 applied if it is not None
        //public async bool runCalc1(
        //    string file1,
        //    object func,
        //    string outFile,
        //    object gv,
        //    bool isInt = false,
        //    object fun1 = null) {
        //    if (File.Exists(outFile)) {
        //        await Utils.removeLayerAndFiles(outFile);
        //    }
        //    var r1 = Raster(file1, gv);
        //    var rout = Raster(outFile, gv, canWrite: true, isInt: isInt);
        //    var completed = false;
        //    while (!completed) {
        //        try {
        //            // safer to mark complete immediately to avoid danger of endless loop
        //            // only way to loop is then the memory error exception being raised 
        //            completed = true;
        //            r1.open(this.chunkCount);
        //            var noData = isInt ? -99999 : r1.noData;
        //            rout.open(this.chunkCount, numRows: r1.numRows, numCols: r1.numCols, transform: r1.ds.GetGeoTransform(), projection: r1.ds.GetProjection(), noData: noData);
        //            foreach (var row in Enumerable.Range(0, r1.numRows)) {
        //                foreach (var col in Enumerable.Range(0, r1.numCols)) {
        //                    var v1 = r1.read(row, col);
        //                    if (fun1 is not null && v1 != r1.noData) {
        //                        v1 = fun1(v1);
        //                    }
        //                    var vout = func(v1, r1.noData, noData);
        //                    rout.Write(row, col, vout);
        //                }
        //            }
        //            r1.close();
        //            rout.close();
        //        } catch (OutOfMemoryException) {
        //            Utils.loginfo(string.Format("runCalc1 out of memory with chunk count {0}", this.chunkCount));
        //            try {
        //                r1.close();
        //                rout.close();
        //            } catch (Exception ex) {
        //            }
        //            this.chunkCount += 1;
        //            completed = false;
        //        }
        //    }
        //    if (File.Exists(outFile)) {
        //        Utils.copyPrj(file1, outFile);
        //        return true;
        //    } else {
        //        // Utils.error(u'Calculator  failed', self._gv.isBatch)
        //        return false;
        //    }
        //}

        //// Use func as a function to calulate outFile from file1 and file2.
        ////         
        ////         Assumes file1 and file2 have same origina and pixel size.
        ////         If file1/2 values are not nodata and fun1/2 are not None, they are applied before func is applied.
        //public virtual bool runCalc2(
        //    string file1,
        //    string file2,
        //    object func,
        //    string outFile,
        //    object gv,
        //    bool isInt = false,
        //    object fun1 = null,
        //    object fun2 = null) {
        //    if (File.Exists(outFile)) {
        //        Utils.removeLayerAndFiles(outFile, gv.iface.legendInterface());
        //    }
        //    var r1 = Raster(file1, gv);
        //    var r2 = Raster(file2, gv);
        //    var rout = Raster(outFile, gv, canWrite: true, isInt: isInt);
        //    var completed = false;
        //    while (!completed) {
        //        try {
        //            // safer to mark complete immediately to avoid danger of endless loop
        //            // only way to loop is then the memory error exception being raised 
        //            completed = true;
        //            r1.open(this.chunkCount);
        //            r2.open(this.chunkCount);
        //            var noData = isInt ? -1 : r1.noData;
        //            rout.open(this.chunkCount, numRows: r1.numRows, numCols: r1.numCols, transform: r1.ds.GetGeoTransform(), projection: r1.ds.GetProjection(), noData: noData);
        //            foreach (var row in Enumerable.Range(0, r1.numRows)) {
        //                foreach (var col in Enumerable.Range(0, r1.numCols)) {
        //                    var v1 = r1.read(row, col);
        //                    if (fun1 is not null && v1 != r1.noData) {
        //                        v1 = fun1(v1);
        //                    }
        //                    var v2 = r2.read(row, col);
        //                    if (fun2 is not null && v2 != r2.noData) {
        //                        v2 = fun2(v2);
        //                    }
        //                    var vout = func(v1, r1.noData, v2, r2.noData, noData);
        //                    rout.Write(row, col, vout);
        //                }
        //            }
        //            r1.close();
        //            r2.close();
        //            rout.close();
        //        } catch (OutOfMemoryException) {
        //            Utils.loginfo(string.Format("runCalc2 out of memory with chunk count {0}", this.chunkCount));
        //            try {
        //                r1.close();
        //                r2.close();
        //                rout.close();
        //            } catch {
        //            }
        //            this.chunkCount += 1;
        //            completed = false;
        //        }
        //    }
        //    if (File.Exists(outFile)) {
        //        Utils.copyPrj(file1, outFile);
        //        return true;
        //    } else {
        //        // Utils.error(u'Calculator  failed', self._gv.isBatch)
        //        return false;
        //    }
        //}

        //// Use func as a function to calulate outFile from file1 and file2, using numbers of rows and columns of baseFile.
        //// 
        ////         If file1/2 values are not nodata and fun1/2 are not None, they are applied before func is applied.
        //public virtual bool runCalc2Trans(
        //    string file1,
        //    string file2,
        //    object func,
        //    string outFile,
        //    string baseFile,
        //    object gv,
        //    bool isInt = false,
        //    object fun1 = null,
        //    object fun2 = null) {
        //    if (File.Exists(outFile)) {
        //        Utils.removeLayerAndFiles(outFile, gv.iface.legendInterface());
        //    }
        //    var r1 = Raster(file1, gv);
        //    var r2 = Raster(file2, gv);
        //    var rout = Raster(outFile, gv, canWrite: true, isInt: isInt);
        //    var ds = gdal.Open(baseFile, gdal.GA_ReadOnly);
        //    var transform = ds.GetGeoTransform();
        //    var numRows = ds.RasterYSize;
        //    var numCols = ds.RasterXSize;
        //    var projection = ds.GetProjection();
        //    ds = null;
        //    var completed = false;
        //    while (!completed) {
        //        try {
        //            // safer to mark complete immediately to avoid danger of endless loop
        //            // only way to loop is then the memory error exception being raised 
        //            completed = true;
        //            r1.open(this.chunkCount);
        //            r2.open(this.chunkCount);
        //            var transform1 = r1.ds.GetGeoTransform();
        //            var transform2 = r2.ds.GetGeoTransform();
        //            (rowFun1, colFun1) = Topology.translateCoords(transform, transform1, numRows, numCols);
        //            (rowFun2, colFun2) = Topology.translateCoords(transform, transform2, numRows, numCols);
        //            var noData = isInt ? -1 : r1.noData;
        //            rout.open(this.chunkCount, numRows: numRows, numCols: numCols, transform: transform, projection: projection, noData: noData);
        //            foreach (var row in Enumerable.Range(0, numRows)) {
        //                var y = Topology.rowToY(row, transform);
        //                var row1 = rowFun1(row, y);
        //                var row2 = rowFun2(row, y);
        //                foreach (var col in Enumerable.Range(0, numCols)) {
        //                    var x = Topology.colToX(col, transform);
        //                    var col1 = colFun1(col, x);
        //                    var col2 = colFun2(col, x);
        //                    var v1 = r1.read(row1, col1);
        //                    if (fun1 is not null && v1 != r1.noData) {
        //                        v1 = fun1(v1);
        //                    }
        //                    var v2 = r2.read(row2, col2);
        //                    if (fun2 is not null && v2 != r2.noData) {
        //                        v2 = fun2(v2);
        //                    }
        //                    var vout = func(v1, r1.noData, v2, r2.noData, noData);
        //                    rout.Write(row, col, vout);
        //                }
        //            }
        //            r1.close();
        //            r2.close();
        //            rout.close();
        //        } catch (OutOfMemoryException) {
        //            Utils.loginfo(string.Format("runCalc2Trans out of memory with chunk count {0}", this.chunkCount));
        //            try {
        //                r1.close();
        //                r2.close();
        //                rout.close();
        //            } catch {
        //            }
        //            this.chunkCount += 1;
        //            completed = false;
        //        }
        //    }
        //    if (File.Exists(outFile)) {
        //        Utils.copyPrj(baseFile, outFile);
        //        return true;
        //    } else {
        //        // Utils.error(u'Calculator  failed', self._gv.isBatch)
        //        return false;
        //    }
        //}

        // Create as burnFile a copy of demFile with points on lines streamFile reduced in height by depth metres.
       
        public static void burnStream(
            string streamFile,
            string demFile,
            string burnFile,
            double depth,
            double verticalFactor,
            bool isBatch) {
            int y1;
            int x1;
            int y0;
            int x0;
            int temp;
            // use vertical factor to convert from metres to vertical units of DEM
            var demReduction = depth / verticalFactor;
            Debug.Assert(!File.Exists(burnFile));
            var demDs = Gdal.Open(demFile, Access.GA_ReadOnly);
            var driver = Gdal.GetDriverByName("GTiff");
            var burnDs = driver.CreateCopy(burnFile, demDs, 0, null, null, null);
            if (burnDs is null) {
                Utils.error(string.Format("Failed to create burned-in DEM {0}", burnFile), isBatch);
                return;
            }
            demDs = null;
            Utils.copyPrj(demFile, burnFile);
            var band = burnDs.GetRasterBand(1);
            double nodata;
            int hasval;
            band.GetNoDataValue(out nodata, out hasval);
            double[] burnTransform = new double[6];
            burnDs.GetGeoTransform(burnTransform);
            var streamDs = Ogr.Open(streamFile, 0);
            var streamLayer = streamDs.GetLayerByIndex(0);
            var start = DateTime.Now;
            var countHits = 0;
            var countPoints = 0;
            var countChanges = 0;
            var changed = new Dictionary<int, List<int>>();
            bool single;
            int lineCount;
            OSGeo.OGR.Geometry line;
            streamLayer.ResetReading();
            OSGeo.OGR.Feature reach;
            do {
                reach = streamLayer.GetNextFeature();
                if (reach is null) { break; }
                var geometry = reach.GetGeometryRef();
                if (geometry.GetGeometryType() == wkbGeometryType.wkbMultiLineString) {
                    single = false;
                    lineCount = geometry.GetGeometryCount();
                } else {
                    single = true;
                    lineCount = 1;
                }
                for (int i = 0; i < lineCount; i++) {
                    if (single && i == 0) {
                        line = geometry;
                    } else {
                        line = geometry.GetGeometryRef(i);
                    }
                    int count = line.GetPointCount();
                    foreach (var j in Enumerable.Range(0, count - 1)) {
                        countPoints += 1;
                        var p0 = new double[3];
                        line.GetPoint(j, p0);
                        var px0 = p0[0];
                        var py0 = p0[1];
                        (x0, y0) = Topology.projToCell(px0, py0, burnTransform);
                        var p1 = new double[3];
                        line.GetPoint(j + 1, p1);
                        var px1 = p1[0];
                        var py1 = p1[1];
                        (x1, y1) = Topology.projToCell(px1, py1, burnTransform);
                        var steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
                        if (steep) {
                            temp = x0;
                            x0 = y0;
                            y0 = temp;
                            temp = x1;
                            x1 = y1;
                            y1 = temp;
                        }
                        if (x0 > x1) {
                            temp = x0;
                            x0 = x1;
                            x1 = temp;
                            temp = y0;
                            y0 = y1;
                            y1 = temp;
                        }
                        var deltax = x1 - x0;
                        var deltay = Math.Abs(y1 - y0);
                        var err = 0;
                        var deltaerr = deltay;
                        var y = y0;
                        var ystep = y0 < y1 ? 1 : -1;
                        double[] data = new double[1];
                        foreach (var x in Enumerable.Range(x0, x1 + 1 - x0)) {
                            if (steep) {
                                if (Topology.addPointToChanged(changed, y, x)) {
                                    // read can raise exception if coordinates outside extent
                                    try {
                                        band.ReadRaster(y, x, 1, 1, data, 1, 1, 0, 0);
                                        if (data[0] != nodata) {
                                            data[0] -= demReduction;
                                            band.WriteRaster(y, x, 1, 1, data, 1, 1, 0, 0);
                                            countChanges += 1;
                                        }
                                    } catch {; }
                                } else {
                                    countHits += 1;
                                }
                            } else if (Topology.addPointToChanged(changed, x, y)) {
                                // read can raise exception if coordinates outside extent
                                try {
                                    band.ReadRaster(x, y, 1, 1, data, 1, 1, 0, 0);
                                    if (data[0] != nodata) {
                                        data[0] -= demReduction;
                                        band.WriteRaster(x, y, 1, 1, data, 1, 1, 0, 0);
                                        countChanges += 1;
                                    }
                                } catch {; }
                            } else {
                                countHits += 1;
                            }
                            err += deltaerr;
                            if (2 * err < deltax) {
                                continue;
                            }
                            y += ystep;
                            err -= deltax;
                        }
                    }
                }
            } while (reach is not null);
            var finish = DateTime.Now;
            Utils.loginfo(string.Format("Created burned-in DEM {0} in {1} milliseconds; {2} points; {3} hits; {4} changes", burnFile, Convert.ToInt32(finish.Subtract(start).TotalMilliseconds), countPoints, countHits, countChanges));
        }

        // Changed points held in dictionary column -> row-sortedlist, since it is like a sparse matrix.
        //         Add a point unless ready there.  Return true if added.
        //         
       
        public static bool addPointToChanged(Dictionary<int, List<int>> changed, int col, int row) {
            List<int> rows;
            if (!changed.TryGetValue(col, out rows)) {
                rows = new List<int>();
            }
            var inserted = ListFuns.insertIntoSortedIntList(row, rows, true);
            if (inserted) {
                changed[col] = rows;
                return true;
            } else {
                return false;
            }
        }
        //         Get the band 1 value at point (double[] verion) in a raster.
        //         
        //         Must be run inside QueuedTask.Run
        //         
        public static double valueAtPoint(double[] point, ArcGIS.Core.Data.Raster.Raster raster) {
            try {
                // returns (column, row)
                Tuple<int, int> pixel = raster.MapToPixel(point[0], point[1]);
                // uses band, column, row
                // use band 1, which seems to have index 0 in ArcGIS Pro version 3.1
                //var x1 = raster.GetPixelValue(1, pixel.Item1, pixel.Item2);
                var x0 = raster.GetPixelValue(0, pixel.Item1, pixel.Item2);
                double val = Convert.ToDouble(x0);
                return val;
            }
            catch (Exception) { return double.NaN; }
        }

        // 
        //         Get the band value at point (double[]) in a GDAL raster band
        //         
        public static double valueAtPoint(double[] point, double[] transform, Band band) {
            try {
                // returns (column, row)
                (int col, int row) = Topology.projToCell(point[0], point[1], transform);
                double[] data = new double[1];
                band.ReadRaster(col, row, 1, 1, data, 1, 1, 0, 0);
                return data[0];
            }
            catch (Exception) { return double.NaN; }
        }

        public static double[] geometryToPointAsDouble(OSGeo.OGR.Geometry geom) {
            // convert the first point in a geometry to double[]
            double[] argout = new double[3];
            geom.GetPoint(0, argout);
            return argout;
        }
        // make list of geometry points 
        public static List<double[]> geometryPointsToList(OSGeo.OGR.Geometry geom) {
            var result = new List<double[]>();
            int count = geom.GetPointCount();
            if (count > 0) {
                for (int i = 0; i < count; i++) {
                    double[] argout = new double[3];
                    geom.GetPoint(i, argout);
                    result.Add(argout);
                }
            }
            return result;
        }
        // lake list of multi geometry parts
        public static List<OSGeo.OGR.Geometry> geometryToPartsList(OSGeo.OGR.Geometry geom) {
            var result = new List<OSGeo.OGR.Geometry>();
            int count = geom.GetGeometryCount();
            if (count > 0) {
                for (int i = 0; i < count; i++) {
                    result.Add(geom.GetGeometryRef(i));
                }
            }
            return result;
        }

        //// 
        ////         Get the band 1 value at point in a grid layer.  Return None if no value, eg NoData
        ////         
        ////         

                    //public static object valueAtPoint(object point, object layer) {
                    //    (val, ok) = layer.dataProvider().sample(point, 1);
                    //    if (!ok) {
                    //        return null;
                    //    } else {
                    //        return val;
                    //    }
                    //}

                    // Return true if a subbasin is upstream from an inlet.
        public virtual bool isUpstreamSubbasin(int subbasin) {
            return this.upstreamFromInlets.Contains(subbasin);
        }

        // Convert a double[] to latlong coordinates and return it.
        public double[] pointToLatLong(double[] point) {
            var crsTransform = ProjectionTransformation.Create(this.crsProject, this.crsLatLong, this.demExtent);
            var inGeom = new MapPointBuilderEx(point[0], point[1]).ToGeometry();
            var outGeom = GeometryEngine.Instance.ProjectEx(inGeom, crsTransform) as MapPoint;
            return new double[3] { outGeom.X, outGeom.Y, 0 }; 
        }

        // Get the index of a shapefile layer attribute name, 
        //         reporting error if not found, unless ignoreMissing is true.
        //         
        public async Task<int> getIndex(ArcGIS.Desktop.Mapping.FeatureLayer layer, string name, bool ignoreMissing = false) {

            bool eqName(FieldDescription fd) {
                var len = Math.Min(name.Length, 10);
                return fd.Name.ToUpper() == name.Substring(0, len).Trim().ToUpper(); 
            }
            var predicate = new Predicate<FieldDescription>(eqName);
            var index = await QueuedTask.Run(() => { return layer.GetFieldDescriptions().FindIndex(0, predicate); });
            if (!ignoreMissing && index < 0) {
                var filename = await Utils.layerFilename(layer);
                Utils.error(string.Format("Cannot find field {0} in {1}", name, filename), this.isBatch);
                throw new Exception();
            }
            return index;
        }

        // Get the index of a shapefile layer attribute name, 
        //         reporting error if not found, unless ignoreMissing is true.
        // OGR version 
        //         
        public int getIndex(OSGeo.OGR.Layer layer, string name, bool ignoreMissing = false) {
            var defn = layer.GetLayerDefn();
            var index = defn.GetFieldIndex(name);
            if (index < 0 && !ignoreMissing) {
                Utils.error(String.Format("Cannot find field {0} in layer {1}", name, layer.GetName()), this.isBatch);
            }
            return index;
        }

        // Get the index of a shapefile definition attribute name, 
        //         reporting error if not found, unless ignoreMissing is true.
        // OGR version 
        //         
        public int getIndex(OSGeo.OGR.FeatureDefn defn, string name, string filename, bool ignoreMissing = false) {
            var index = defn.GetFieldIndex(name);
            if (index < 0 && !ignoreMissing) {
                Utils.error(String.Format("Cannot find field {0} in {1}", name, filename), this.isBatch);
            }
            return index;
        }

        //// Get the index of a shapefile provider attribute name, 
        ////         reporting error if not found, unless ignoreMissing is true.
        ////         
        //public virtual int getProviderIndex(object provider, string name, bool ignoreMissing = false) {
        //    // field names are truncated to 10 characters when created, so only search for up to 10 characters
        //    var index = provider.fieldNameIndex(name[:10:]);
        //    if (!ignoreMissing && index < 0) {
        //        Utils.error(string.Format("Cannot find field {0} in provider", name), this.isBatch);
        //    }
        //    return index;
            //     def makePointInLine(self, reach: QgsFeature, percent: float) -> MapPointBuilderEx.CreateMapPoint:
            //         """Return a point percent along line from outlet end to next point."""
            //         if self.outletAtStart:
            //             line = Topology.reachFirstLine(reach.GetGeometryRef(), self.xThreshold, self.yThreshold)
            //             pt1 = line[0]
            //             pt2 = line[1]
            //         else:
            //             line = Topology.reachLastLine(reach.GetGeometryRef(), self.xThreshold, self.yThreshold)
            //             length = len(line)
            //             pt1 = line[length-1]
            //             pt2 = line[length-2]
            //         x = (pt1.X * (100 - percent) + pt2.X * percent) / 100.0
            //         y = (pt1.Y * (100 - percent) + pt2.Y * percent) / 100.0
            //         return MapPointBuilderEx.CreateMapPoint(x, y)
        //}

        // Returns true iff streamLayer lines have their outlet points at their start points.
        //          
        //         If ad8Layer is not None, we are not in an existing watershed, and can rely on accumulations.
        //         Accumulation will be higher at the outlet end.
        //         Finds shapes with a downstream connections, and 
        //         determines the orientation by seeing how such a shape is connected to the downstream shape.
        //         If they don't seem to be connected (as my happen after merging subbasins) 
        //         tries other shapes with downstream connections, up to 10.
        //         A line is connected to another if their ends are less than dx and dy apart horizontally and vertically.
        //         Assumes the orientation found for this shape can be used generally for the layer.
        //         For HUC models just returns False immediately as NHD flowlines start from source end.
        //
        // TODO remove ad8Raster parameter.  Always null except in main module checking delineation is run, and not worth the computation
        public virtual bool hasOutletAtStart(OSGeo.OGR.Layer streamLayer, ArcGIS.Core.Data.Raster.Raster ad8Raster, string channelFile) {
            OSGeo.OGR.Geometry line;
            bool single;
            int lineCount;
            OSGeo.OGR.Feature reach;
            if (this.isHUC || this.isHAWQS) {
                return false;
            }
            var dsStreamIndex = this.getIndex(streamLayer, Topology._DSLINKNO, ignoreMissing: false);
            if (dsStreamIndex < 0) {
                Utils.error(string.Format("No DSLINKNO field in channel file {0}", channelFile), this.isBatch);
                return true;
            }
            if (ad8Raster is not null) {
                // only set to non-None if not an existing watershed
                // use accumulation difference at ends of reach (or line in reach) to decide
                streamLayer.ResetReading();
                do {
                    reach = streamLayer.GetNextFeature();
                    if (reach is null) { break; }
                    var geometry = reach.GetGeometryRef();
                    if (geometry.GetGeometryType() == wkbGeometryType.wkbLineString) { 
                        single = true;
                        lineCount = 1;
                    } else {
                        lineCount = geometry.GetGeometryCount();
                        single = false;
                    }
                    for (int i = 0; i < lineCount; i++) {
                        if (single && i == 0) {
                            line = geometry;
                        } else {
                            line = geometry.GetGeometryRef(i);
                        }
                        int count = line.GetPointCount();
                        if (count > 1) {
                            // make sure we haven't picked on an empty line
                            double[] p1 = new double[3];
                            line.GetPoint(0, p1);
                            double[] p2 = new double[3];
                            line.GetPoint(count - 1, p2);
                            var acc1 = Topology.valueAtPoint(p1, ad8Raster);
                            var acc2 = Topology.valueAtPoint(p2, ad8Raster);
                            if (!(double.IsNaN(acc1) || double.IsNaN(acc2) || acc1 == acc2)) {
                                return acc1 > acc2;
                            }
                        }
                    }
                } while (reach is not null) ;
            }
            // find candidates: links with a down connection
            var candidates = new List<(OSGeo.OGR.Feature, OSGeo.OGR.Feature)>();
            streamLayer.ResetReading();
            do {
                reach = streamLayer.GetNextFeature();
                if (reach is null) { break; }
                var downLink = reach.GetFieldAsInteger(dsStreamIndex);
                if (downLink >= 0) {
                    // find the down reach
                    var downReach = Utils.getOgrFeatureByValue(streamLayer, Topology._LINKNO, downLink);
                    if (downReach is not null) {
                        candidates.Add((reach, downReach));
                        if (candidates.Count < 10) {
                            continue;
                        } else {
                            break;
                        }
                    } else {
                        Utils.error(string.Format("Cannot find link {0} in streams file", downLink), this.isBatch);
                        return true;
                    }
                }
            } while (reach is not null);
            if (candidates == new List<(OSGeo.OGR.Feature, OSGeo.OGR.Feature)>()) {
                Utils.error(string.Format("Cannot find link with a downstream link in {0}.  Do you only have one channel?", channelFile), this.isBatch);
                return true;
            }
            foreach (var (upReach, downReach) in candidates) {
                var downStart = Topology.reachOgrFirstLine(downReach, this.xThreshold, this.yThreshold);
                if (downStart is null) {
                    continue;
                }
                var downFinish = Topology.reachOgrLastLine(downReach, this.xThreshold, this.yThreshold);
                if (downFinish is null) {
                    continue;
                }
                var upStart = Topology.reachOgrFirstLine(upReach, this.xThreshold, this.yThreshold);
                if (upStart is null) {
                    continue;
                }
                var upFinish = Topology.reachOgrLastLine(upReach, this.xThreshold, this.yThreshold);
                if (upFinish is null) {
                    continue;
                }
                double[] pt = new double[3];
                upStart.GetPoint(0, pt);
                if (Topology.pointOnLine(pt, downFinish, this.xThreshold, this.yThreshold)) {
                    return true;
                }
                upFinish.GetPoint(upFinish.GetPointCount() - 1, pt);
                if (Topology.pointOnLine(pt, downStart, this.xThreshold, this.yThreshold)) {
                    return false;
                }
            }
            Utils.error(string.Format("Cannot find physically connected reaches in channel shapefile {0}.  Try increasing nearness threshold", channelFile), this.isBatch);
            return true;
        }

        // Write outlets, downSubbasins, and (unless useGridModel)
        //          inlets, upstreamFromInlets, and outletChannels  tables.
        public virtual bool saveOutletsAndSources(OSGeo.OGR.Layer channelLayer, OSGeo.OGR.Layer outletLayer, bool useGridModel) {
            double[] last;
            double[] first;
            double[] pt = new double[3];
            int subbasin;
            int dsChLink;
            int subbasinIndex;
            int idIndex = 0;
            int inletIndex = 0;
            int srcIndex = 0;
            int resIndex = 0;
            int ptIdIndex = 0;
            // in case called twice
            this.pointId = 0;
            this.waterBodyId = 0;
            this.outlets.Clear();
            this.nearoutlets.Clear();
            this.nearsources.Clear();
            this.inlets.Clear();
            this.chOutlets.Clear();
            this.chPointSources.Clear();
            this.upstreamFromInlets.Clear();
            this.downSubbasins.Clear();
            this.chBasinToSubbasin.Clear();
            var chLinkToSubbasin = new Dictionary<int, int>();
            var downChannels = new Dictionary<int, int>();
            var chInlets = new Dictionary<int, (int, double[])>();
            var channelDefn = channelLayer.GetLayerDefn();
            var chLinkIndex = channelDefn.GetFieldIndex(Topology._LINKNO);
            var DSLINKNO = this.isHUC ? Topology._DSLINKNO1 : Topology._DSLINKNO;
            var dsChLinkIndex = channelDefn.GetFieldIndex(DSLINKNO);
            var isHUCOrHAWQS = this.isHUC || this.isHAWQS;
            this.wsnoIndex = channelDefn.GetFieldIndex(Topology._WSNO);
            if (this.wsnoIndex < 0 && (useGridModel || isHUCOrHAWQS)) {
                Utils.error(String.Format("Cannot find field {0} in layer {1}", Topology._WSNO, channelLayer.GetName()), this.isBatch);
                return false;
            }
                var sourceXIndex = channelDefn.GetFieldIndex(Topology._SOURCEX);
            if (sourceXIndex < 0 && isHUCOrHAWQS) {
                Utils.error(String.Format("Cannot find field {0} in layer {1}", Topology._SOURCEX, channelLayer.GetName()), this.isBatch);
                return false;
            }
            var sourceYIndex = channelDefn.GetFieldIndex(Topology._SOURCEY);
            if (sourceYIndex < 0 && isHUCOrHAWQS) {
                Utils.error(String.Format("Cannot find field {0} in layer {1}", Topology._SOURCEY, channelLayer.GetName()), this.isBatch);
                return false;
            }
            var outletXIndex = channelDefn.GetFieldIndex(Topology._OUTLETX);
            if (outletXIndex < 0 && isHUCOrHAWQS) {
                Utils.error(String.Format("Cannot find field {0} in layer {1}", Topology._OUTLETX, channelLayer.GetName()), this.isBatch);
                return false;
            }
            var outletYIndex = channelDefn.GetFieldIndex(Topology._OUTLETY);
            if (outletYIndex < 0 && isHUCOrHAWQS) {
                Utils.error(String.Format("Cannot find field {0} in layer {1}", Topology._OUTLETY, channelLayer.GetName()), this.isBatch);
                return false;
            }
            if (chLinkIndex < 0 || dsChLinkIndex < 0) {
                return false;
            }
            if (this.isHAWQS) {
                // channel file has a Subbasin field
                subbasinIndex = channelDefn.GetFieldIndex(Topology._SUBBASIN);
            } else {
                // ignoreMissing for subbasinIndex necessary when useGridModel, since channelLayer is then a streams layer
                subbasinIndex = channelDefn.GetFieldIndex(Topology._BASINNO);
                if (subbasinIndex < 0 && !useGridModel) {
                    Utils.error(String.Format("Cannot find field {0} in layer {1}", Topology._BASINNO, channelLayer.GetName()), this.isBatch);
                    return false;
                }
            }
            if (useGridModel) {
                if (this.wsnoIndex < 0) {
                    return false;
                }
            } else if (subbasinIndex < 0) {
                return false;
            }
            var dsNodeIndex = channelDefn.GetFieldIndex(Topology._DSNODEID);
            if (outletLayer is not null) {
                var outletDefn = outletLayer.GetLayerDefn();
                idIndex = outletDefn.GetFieldIndex(Topology._ID);
                if (idIndex < 0) {
                    Utils.error(String.Format("Cannot find field {0} in layer {1}", Topology._ID, outletLayer.GetName()), this.isBatch);
                    return false;
                }
                inletIndex = outletDefn.GetFieldIndex(Topology._INLET);
                if (inletIndex < 0) {
                    Utils.error(String.Format("Cannot find field {0} in layer {1}", Topology._INLET, outletLayer.GetName()), this.isBatch);
                    return false;
                }
                srcIndex = outletDefn.GetFieldIndex(Topology._PTSOURCE);
                if (srcIndex < 0) {
                    Utils.error(String.Format("Cannot find field {0} in layer {1}", Topology._PTSOURCE, outletLayer.GetName()), this.isBatch);
                    return false;
                }
                resIndex = outletDefn.GetFieldIndex(Topology._RES);
                if (resIndex < 0) {
                    Utils.error(String.Format("Cannot find field {0} in layer {1}", Topology._RES, outletLayer.GetName()), this.isBatch);
                    return false;
                }
                ptIdIndex = outletDefn.GetFieldIndex(Topology._POINTID);
                if (ptIdIndex < 0) {
                    outletLayer.CreateField(new FieldDefn(Topology._POINTID, OSGeo.OGR.FieldType.OFTInteger), 1);
                    outletDefn = outletLayer.GetLayerDefn();
                    ptIdIndex = outletDefn.GetFieldIndex(Topology._POINTID);
                }
                // set pointId to Math.Max id value in outletLayer
                // and waterBodyId to Math.Max reservoir or pond id
                // copy original id to PointId field, unless zero, when change to Math.Max id + 1
                long zeroIndex = -1;
                outletLayer.ResetReading();
                OSGeo.OGR.Feature point;
                do {
                    point = outletLayer.GetNextFeature();
                    if (point is null) { break; }
                    var ptId = point.GetFieldAsInteger(idIndex);
                    if (ptId == 0) {
                        zeroIndex = point.GetFID();
                    } else {
                        point.SetField(ptIdIndex, ptId);
                        outletLayer.SetFeature(point);
                    }
                    this.pointId = Math.Max(this.pointId, ptId);
                    if (point.GetFieldAsInteger(inletIndex) == 0 && point.GetFieldAsInteger(resIndex) > 0) {
                        this.waterBodyId = Math.Max(this.waterBodyId, ptId);
                    }
                } while (point is not null);
                if (zeroIndex >= 0) {
                    this.pointId += 1;
                    point = outletLayer.GetFeature(zeroIndex);
                    point.SetField(ptIdIndex, this.pointId);
                }
            } else {
                dsNodeIndex = -1;
            }
                channelLayer.ResetReading();
                OSGeo.OGR.Feature reach;
                do {
                    reach = channelLayer.GetNextFeature();
                    if (reach is null) { break; }
                    var chLink = reach.GetFieldAsInteger(chLinkIndex);
                    dsChLink = reach.GetFieldAsInteger(dsChLinkIndex);
                    var chBasin = reach.GetFieldAsInteger(this.wsnoIndex);
                    // for grids, channel basins and subbasins are the same
                    subbasin = useGridModel ? chBasin : reach.GetFieldAsInteger(subbasinIndex);
                    chLinkToSubbasin[chLink] = subbasin;
                    if (!useGridModel) {
                        this.chBasinToSubbasin[chBasin] = subbasin;
                    }
                    downChannels[chLink] = dsChLink;
                    var dsNode = dsNodeIndex >= 0 ? reach.GetFieldAsInteger(dsNodeIndex) : -1;
                    if (dsNode >= 0 && idIndex >= 0 && inletIndex >= 0 && srcIndex >= 0 && resIndex >= 0 && ptIdIndex >= 0) {
                        Debug.Assert(outletLayer != null); // else dsNodeIndex = -1, so dsNode = -1
                        OSGeo.OGR.Feature outletPoint = null;
                        OSGeo.OGR.Feature inletPoint = null;
                        OSGeo.OGR.Feature f;
                        do {
                            f = outletLayer.GetNextFeature();
                            if (f is null) { break; }
                            if (f.GetFieldAsInteger(idIndex) == dsNode) {
                                if (f.GetFieldAsInteger(inletIndex) == 0) {
                                    if (f.GetFieldAsInteger(resIndex) == 0) {
                                        outletPoint = f;
                                        break;
                                    }
                                } else if (f.GetFieldAsInteger(srcIndex) == 0) {
                                    inletPoint = f;
                                    break;
                                }
                            }
                        } while (f is not null);
                        if (outletPoint is not null) {
                            outletPoint.GetGeometryRef().GetPoint(0, pt);
                            this.chOutlets[chLink] = (outletPoint.GetFieldAsInteger(ptIdIndex), pt);
                        } else if (inletPoint is not null) {
                            inletPoint.GetGeometryRef().GetPoint(0, pt);
                            chInlets[chLink] = (inletPoint.GetFieldAsInteger(ptIdIndex), pt);
                        }
                    }
                    if (this.isHUC) {
                        first = new double[3] { reach.GetFieldAsDouble(sourceXIndex), reach.GetFieldAsDouble(sourceYIndex), 0 };
                        last = new double[3] { reach.GetFieldAsDouble(outletXIndex), reach.GetFieldAsDouble(outletYIndex), 0 };
                    } else {
                        var geom = Topology.reachOgrFirstLine(reach, this.xThreshold, this.yThreshold);
                        if (geom is null || geom.GetPointCount() < 2) {
                            Utils.error("It looks like your channels shapefile does not obey the single direction rule, that all channels are either upstream or downstream.", this.isBatch);
                            return false;
                        }
                        first = new double[3];
                        geom.GetPoint(0, first);
                        geom = Topology.reachOgrLastLine(reach, this.xThreshold, this.yThreshold);
                        int count = geom.GetPointCount();
                        if (geom is null || count < 2) {
                            Utils.error("It looks like your channels shapefile does not obey the single direction rule, that all channels are either upstream or downstream.", this.isBatch);
                            return false;
                        }
                        last = new double[3];
                        geom.GetPoint(count - 1, last);
                    }
                    int outId;
                    double[] outPt;
                    if (this.chOutlets.ContainsKey(chLink)) {
                        (outId, outPt) = this.chOutlets[chLink];
                    } else {
                        outId = -1;
                        outPt = null;
                    }
                    if (outPt is null) {
                        this.pointId += 1;
                        outId = this.pointId;
                    }
                    this.pointId += 1;
                    var srcId = this.pointId;
                    Debug.Assert(first is not null);
                    Debug.Assert(last is not null);
                    if (this.outletAtStart) {
                        if (!useGridModel && outPt is not null && !Topology.coincidentPoints(first, outPt, this.xThreshold, this.yThreshold)) {
                            Utils.error(string.Format("Outlet point {0} at ({1}, {2}) not coincident with start of channel link {3}", outId, outPt[0], outPt[1], chLink), this.isBatch);
                        }
                        this.chOutlets[chLink] = (outId, first);
                        this.chPointSources[chLink] = (srcId, last);
                    } else {
                        if (!useGridModel && outPt is not null && !Topology.coincidentPoints(last, outPt, this.xThreshold, this.yThreshold)) {
                            Utils.error(string.Format("Outlet point {0} at ({1}, {2}) not coincident with end of channel link {3}", outId, outPt[0], outPt[1], chLink), this.isBatch);
                        }
                        this.chOutlets[chLink] = (outId, last);
                        this.chPointSources[chLink] = (srcId, first);
                    }
                } while (reach is not null);
                // now find the channels which are on subbasin boundaries,
                // i.e. their downstream channels are in different basins
                var hasInlet = false;
            foreach (var (chLink, dsChLnk) in downChannels) {
                subbasin = chLinkToSubbasin[chLink];
                if (subbasin == Topology._NOBASIN) {
                    // from a zero-length channel
                    continue;
                }
                var dsSubbasin = dsChLnk >= 0 ? chLinkToSubbasin[dsChLnk] : -1;
                while (dsSubbasin == Topology._NOBASIN) {
                    // skip past zero-length channels
                    int nextLink;
                    if (!downChannels.TryGetValue(dsChLnk, out nextLink)) {
                        nextLink = -1;
                    }
                    if (!chLinkToSubbasin.TryGetValue(nextLink, out dsSubbasin)) {
                        dsSubbasin = -1;
                    }
                }
                if (subbasin != dsSubbasin) {
                    this.downSubbasins[subbasin] = dsSubbasin;
                    // collect the basin's outlet location:
                    var (outletId, outletPt) = this.chOutlets[chLink];
                    (int, double[], HashSet<int>) existOutlets;
                    if (!this.outlets.TryGetValue(subbasin, out existOutlets)) {
                        this.outlets[subbasin] = (outletId, outletPt, new HashSet<int> {
                            chLink
                        });
                    } else {
                        var (outletid0, outletPt0, chLinks) = existOutlets;
                        if (!Topology.coincidentPoints(outletPt0, outletPt, this.xThreshold, this.yThreshold)) {
                            Utils.error(string.Format("Polygon {0} has separate outlets at ({1}, {2}) and ({3}, {4}): ignoring second", subbasin, outletPt0[0], outletPt0[1], outletPt[0], outletPt[1]), this.isBatch);
                        } else {
                            this.chOutlets[chLink] = (outletid0, outletPt0);
                            chLinks.Add(chLink);
                            this.outlets[subbasin] = (outletid0, outletPt0, chLinks);
                        }
                    }
                    if (!useGridModel) {
                        //                     self.extraResPoints[subbasin] = chResPoints[chLink]
                        //                     self.extraPtSrcPoints[subbasin] = chSources[chLink]
                        int inletId;
                        double[] inletPt;
                        (int, double[]) temp;
                        if (!chInlets.TryGetValue(chLink, out temp)) {
                            inletId = -1;
                            inletPt = null;
                        } else {
                            inletId = temp.Item1;
                            inletPt = temp.Item2;
                        }
                        if (inletPt is not null && dsSubbasin >= 0) {
                            // inlets are associated with downstream basin
                            this.inlets[dsSubbasin] = (inletId, inletPt);
                            hasInlet = true;
                        }
                    }
                }
            }
            // collect subbasins upstream from inlets
            // this looks inefficient, repeatedly going through all basins, but probably few projects have inlets:
            if (!useGridModel && hasInlet) {
                foreach (var subbsn in this.inlets.Keys) {
                    this.addUpstreamSubbasins(subbsn);
                }
            }
            return true;
        }

        // Return dsNode, or next pointId if dsNode is zero.  Used to prevent a zero point id.
        public virtual int nonzeroPointId(int dsNode) {
            if (dsNode == 0) {
                this.pointId += 1;
                return this.pointId;
            }
            return dsNode;
        }

        // Returns the line of a single polyline, 
        //         or a line in a multipolyline whose first point is not adjacent to a point 
        //         of another line in the multipolyline.
        //         
        public static OSGeo.OGR.Geometry reachOgrFirstLine(OSGeo.OGR.Feature reach, double dx, double dy) {
            OSGeo.OGR.Geometry geometry = reach.GetGeometryRef();
            if (geometry.GetGeometryType() == wkbGeometryType.wkbLineString) {
                return geometry;
            } else {
                int count = geometry.GetGeometryCount();
                for (int i = 0; i < count; i++) {
                    var linei = geometry.GetGeometryRef(i);
                    bool connected = false;
                    if (linei is null || linei.GetPointCount() == 0) { continue; }
                    double[] startPoint = new double[3];
                    linei.GetPoint(0, startPoint);
                    for (int j = 0; j < count; j++) {
                        if (i != j) {
                            var linej = geometry.GetGeometryRef(j);
                            // linei is connected to (ie continues) linej if linei's start point is on linej and 
                            // linej has preceding points, so exclude start point of linej from the search
                            if (Topology.pointOgrOnLine(startPoint, linej, 1, linej.GetPointCount() - 1, dx, dy)) {
                                connected = true;
                                break;
                            }
                        }
                    }
                    if (!connected) {
                        return linei;
                    }
                }
            }
            // should not get here
            return null;
        }

        // Returns the line of a single polyline, 
        //         or a line in a multipolyline whose last point is not adjacent to a point 
        //         of another line in the multipolyline.
        //         
        public static OSGeo.OGR.Geometry reachOgrLastLine(OSGeo.OGR.Feature reach, double dx, double dy) {
            OSGeo.OGR.Geometry geometry = reach.GetGeometryRef();
            if (geometry.GetGeometryType() == wkbGeometryType.wkbLineString) {
                return geometry;
            } else {
                int count = geometry.GetGeometryCount();
                for (int i = 0; i < count; i++) {
                    var linei = geometry.GetGeometryRef(i);
                    bool connected = false;
                    if (linei is null || linei.GetPointCount() == 0) { continue; }
                    double[] lastPoint = new double[3];
                    linei.GetPoint(linei.GetPointCount() - 1, lastPoint);
                    for (int j = 0; j < count; j++) {
                        if (i != j) {
                            var linej = geometry.GetGeometryRef(j);
                            // linei connects to linej if linei's last point is on linej AND
                            // linej continues further, so exclude linej's last point from search
                            if (Topology.pointOgrOnLine(lastPoint, linej, 0, linej.GetPointCount() - 2, dx, dy)) {
                                connected = true;
                                break;
                            }
                        }
                    }
                    if (!connected) {
                        return linei;
                    }
                }
            }
            // should not get here
            return null;
        }

        // Return True if p1 and p2 are within dx and dy horizontally and vertically.
        public static bool samePoint(double[] p1, double[] p2, double dx, double dy) {
            var xThreshold = dx * Parameters._NEARNESSTHRESHOLD;
            var yThreshold = dy * Parameters._NEARNESSTHRESHOLD;
            return Math.Abs(p1[0] - p2[0]) < xThreshold && Math.Abs(p1[1] - p2[1]) < yThreshold;
        }

        // Return true if point is within dx and dy horizontally and vertically
        //         of a point on the line between startIndex and finishIndex inclusive
        //         
        //         Note this only checks if the point is close to a vertex.
        public static bool pointOgrOnLine(double[] point, OSGeo.OGR.Geometry line, int startIndex, int finishIndex, double dx, double dy) {
            if (line is null || line.GetPointCount() == 0) {
                return false;
            }
            var x = point[0];
            var y = point[1];
            var xThreshold = dx * Parameters._NEARNESSTHRESHOLD;
            var yThreshold = dy * Parameters._NEARNESSTHRESHOLD;
            for (int i = startIndex; i <= finishIndex; i++) {
                double[] argout = new double[3];
                line.GetPoint(i, argout);
                if (Math.Abs(x - argout[0]) < xThreshold && Math.Abs(y - argout[1]) < yThreshold) {
                    return true;
                }
            }
            return false;
        }

        // Add basins upstream from start to upstreamFromInlets.
        public virtual void addUpstreamSubbasins(int start) {
            foreach (var (subbasin, downSubbasin) in this.downSubbasins) {
                if (downSubbasin == start) {
                    this.upstreamFromInlets.Add(subbasin);
                    this.addUpstreamSubbasins(subbasin);
                }
            }
        }

        // Return id of lake containing channel, if any, else -1.
        public virtual int surroundingLake(int SWATChannel, bool useGridModel) {
            var chLink = this.SWATChannelToChannel[SWATChannel];
            int lake1;
            if (!this.chLinkInsideLake.TryGetValue(chLink, out lake1)) {
                lake1 = -1;
            }
            if (useGridModel && lake1 < 0) {
                if (!this.chLinkFromLake.TryGetValue(chLink, out lake1)) {
                    return -1;
                } else {
                    return lake1;
                }
            } else {
                return lake1;
            }
        }

        // Result is val unless mask is nodata.
       
        public static double maskFun(
            double val,
            double valNoData,
            double mask,
            double maskNoData,
            double resNoData) {
            if (val == valNoData || mask == maskNoData) {
                return resNoData;
            } else {
                return val;
            }
        }

        //// Returns the line of a single polyline, 
        ////         or a line in a multipolyline whose first point is not adjacent to a point 
        ////         of another line in the multipolyline.
        ////         
       
        //public static object reachFirstLine(OSGeo.OGR.Geometry geometry, double xThreshold, double yThreshold) {

        //    if (!geometry.isMultipart()) {
        //        return geometry.asPolyline();
        //    }
        //    var mpl = geometry.asMultiPolyline();
        //    var numLines = mpl.Count;
        //    foreach (var i in Enumerable.Range(0, numLines)) {
        //        var linei = mpl[i];
        //        var connected = false;
        //        if (linei is null || linei.Count == 0) {
        //            continue;
        //        } else {
        //            var start = linei[0];
        //            foreach (var j in Enumerable.Range(0, numLines)) {
        //                if (i != j) {
        //                    var linej = mpl[j];
        //                    // linei is connected to linej (ie continues from linej) if the first point of linei (start) is on linej AND
        //                    // linej has preceding point(s).
        //                    // So exclude the start of linej in the search for a connection point
        //                    if (linej.Count > 0 && Topology.pointOnLine(start, linej[1], xThreshold, yThreshold)) {
        //                        connected = true;
        //                        break;
        //                    }
        //                }
        //            }
        //        }
        //        if (!connected) {
        //            return linei;
        //        }
        //    }
        //    // should not get here
        //    return null;
        //}

        //// Returns the line of a single polyline, 
        ////         or a line in a multipolyline whose last point is not adjacent to a point 
        ////         of another line in the multipolyline.
        ////         
       
        //public static object reachLastLine(object geometry, double xThreshold, double yThreshold) {
        //    if (!geometry.isMultipart()) {
        //        return geometry.asPolyline();
        //    }
        //    var mpl = geometry.asMultiPolyline();
        //    var numLines = mpl.Count;
        //    foreach (var i in Enumerable.Range(0, numLines)) {
        //        var linei = mpl[i];
        //        var connected = false;
        //        if (linei is null || linei.Count == 0) {
        //            continue;
        //        } else {
        //            var finish = linei[^1];
        //            foreach (var j in Enumerable.Range(0, numLines)) {
        //                if (i != j) {
        //                    var linej = mpl[j];
        //                    // linei is connected to linej if the last point of linei (finish) is on linej AND
        //                    // linej extends further.
        //                    // So exclude the finish of linej in the search for a connection point
        //                    if (linej.Count > 0 && Topology.pointOnLine(finish, linej[: -1:], xThreshold, yThreshold)) {
        //                        connected = true;
        //                        break;
        //                    }
        //                }
        //            }
        //        }
        //        if (!connected) {
        //            return linei;
        //        }
        //    }
        //    // should not get here
        //    return null;
        //}

        // Return true if point is coincident with a point on the line. 
        //         
        //         Note this only checks if the point is close to a vertex.
       
        public static bool pointOnLine(double[] point, OSGeo.OGR.Geometry line, double xThreshold, double yThreshold) {
            if (line is null) { return false; }
            int count = line.GetPointCount();
            if (count == 0) {
                return false;
            }
            double[] pt = new double[3];
            foreach (int i in Enumerable.Range(0, count)) {
                line.GetPoint(i, pt);
                if (Topology.coincidentPoints(point, pt, xThreshold, yThreshold)) {
                    return true;
                }
            }
            return false;
        }

        // Return true if points are within xThreshold and yThreshold
        //         horizontally and vertically.
       
        public static bool coincidentPoints(double[] pt1, double[] pt2, double xThreshold, double yThreshold) {
            return Math.Abs(pt1[0] - pt2[0]) < xThreshold && Math.Abs(pt1[1] - pt2[1]) < yThreshold;
        }

        // Convert column number to X-coordinate.
       
        public static double colToX(int col, double[] transform) {
            return (col + 0.5) * transform[1] + transform[0];
        }

        // Convert row number to Y-coordinate.
       
        public static double rowToY(int row, double[] transform) {
            return (row + 0.5) * transform[5] + transform[3];
        }

        //=========currently not used==================================================================
        // @staticmethod
        // def xToCol(x, transform):
        //     """Convert X-coordinate to column number."""
        //     return int((x - transform[0]) / transform[1])
        //===========================================================================
        //=========currently not used==================================================================
        // @staticmethod
        // def yToRow(y, transform):
        //     """Convert Y-coordinate to row number."""
        //     return int((y - transform[3]) / transform[5])
        //===========================================================================
        // Convert column and row numbers to (X,Y)-coordinates.
       
        public static (double, double) cellToProj(int col, int row, double[] transform) {
            var x = (col + 0.5) * transform[1] + transform[0];
            var y = (row + 0.5) * transform[5] + transform[3];
            return (x, y);
        }

        // Convert (X,Y)-coordinates to column and row numbers.
       
        public static (int, int) projToCell(double x, double y, double[] transform) {
            var col = Convert.ToInt32((x - transform[0]) / transform[1]);
            var row = Convert.ToInt32((y - transform[3]) / transform[5]);
            return (col, row);
        }

        //==========not currently used=================================================================
        // @staticmethod
        // def haveSameCoords(band1, transform1, transform2):
        //     """
        //     Return true if raster transform1 and transform2 are the same or sufficiently
        //     close for row/col coordinates of first to be used without reprojection
        //     as row/col coordinates of the second.
        //     
        //     Assumes second raster has sufficient extent.
        //     We could demand this, but in practice often read rasters well within their extents,
        //     because only looking at points within a watershed.
        //     """
        //     # may work, though may also fail - we are comparing float values
        //     if transform1 == transform2:
        //         return True
        //     # accept the origins as the same if they are within a tenth of the cell size
        //     # otherwise return false
        //     if (Math.Abs(transform1[0] - transform2[0]) > transform2[1] * 0.1 or \
        //         Math.Abs(transform1[3] - transform2[3]) > Math.Abs(transform2[5]) * 0.1):
        //         return False
        //     # then check if the vertical/horizontal difference in cell size times the number of rows/columns
        //     # in the first is less than half the depth/width of a cell in the second
        //     return Math.Abs(transform1[1] - transform2[1]) * band1.XSize < transform2[1] * 0.5 and \
        //             Math.Abs(transform1[5] - transform2[5]) * band1.YSize < Math.Abs(transform2[5]) * 0.5
        //===========================================================================
        // 
        //         Return a pair of functions:
        //         row, latitude -> row and column, longitude -> column
        //         for transforming positions in raster1 to row and column of raster2.
        //         
        //         The functions are:
        //         identities on the first argument if the rasters have (sufficiently) 
        //         the same origins and cell sizes;
        //         a simple shift on the first argument if the rasters have 
        //         the same cell sizes but different origins;
        //         otherwise a full transformation on the second argument.
        //         It is assumed that the first and second arguments are consistent, 
        //         ie they identify the same cell in raster1.
        //         
       
        public static (Func<int, double, int>, Func<int, double, int>) translateCoords(double[] transform1, double[] transform2, int numRows1, int numCols1) {
            int rowShift;
            Func<int, double, int> yFun;
            int colShift;
            Func<int, double, int> xFun;
            // may work, thuough we are comparing real values
            if (transform1 == transform2) {
                return ((row, _) => row, (col, _) => col);
            }
            var xOrigin1 = transform1[0];
            var xSize1 = transform1[1];
            var yOrigin1 = transform1[3]; 
            var ySize1 = transform1[5];
            var xOrigin2 = transform2[0];
            var xSize2 = transform2[1];
            var yOrigin2 = transform2[3];
            var ySize2 = transform2[5];
            // accept the origins as the same if they are within a tenth of the cell size
            var sameXOrigin = Math.Abs(xOrigin1 - xOrigin2) < xSize2 * 0.1;
            var sameYOrigin = Math.Abs(yOrigin1 - yOrigin2) < Math.Abs(ySize2) * 0.1;
            // accept cell sizes as equivalent if  vertical/horizontal difference 
            // in cell size times the number of rows/columns
            // in the first is less than half the depth/width of a cell in the second
            var sameXSize = Math.Abs(xSize1 - xSize2) * numCols1 < xSize2 * 0.5;
            var sameYSize = Math.Abs(ySize1 - ySize2) * numRows1 < Math.Abs(ySize2) * 0.5;
            if (sameXSize) {
                if (sameXOrigin) {
                    xFun = (col, _) => col;
                } else {
                    // just needs origin shift
                    if (xOrigin1 > xOrigin2) {
                        colShift = Convert.ToInt32((xOrigin1 - xOrigin2) / xSize1);
                        xFun = (col, _) => col + colShift;
                    } else {
                        colShift = Convert.ToInt32((xOrigin2 - xOrigin1) / xSize1);
                        xFun = (col, _) => col - colShift;
                    }
                }
            } else {
                // full transformation
                xFun = (_, x) => Convert.ToInt32((x - xOrigin2) / xSize2);
            }
            if (sameYSize) {
                if (sameYOrigin) {
                    yFun = (row, _) => row;
                } else {
                    // just needs origin shift
                    // note y size will be negative
                    if (yOrigin1 > yOrigin2) {
                        rowShift = Convert.ToInt32((yOrigin2 - yOrigin1) / ySize1);
                        yFun = (row, _) => row - rowShift;
                    } else {
                        rowShift = Convert.ToInt32((yOrigin1 - yOrigin2) / ySize1);
                        yFun = (row, _) => row + rowShift;
                    }
                }
            } else {
                // full transformation
                yFun = (_, y) => Convert.ToInt32((y - yOrigin2) / ySize2);
            }
            // note row, column order of return (same as order of reading rasters)
            return (yFun, xFun);
        }

        // Return true if transforms are sufficiently close to be regarded as the same,
        //         i.e. row and column numbers for the first can be used without transformation to read the second.  
        //         Avoids relying on equality between real numbers.
       
        public static bool sameTransform(double[] transform1, double[] transform2, int numRows1, int numCols1) {
            // may work, thuough we are comparing real values
            if (transform1 == transform2) {
                return true;
            }
            var xOrigin1 = transform1[0];
            var xSize1 = transform1[1];
            var yOrigin1 = transform1[3];
            var ySize1 = transform1[5];
            var xOrigin2 = transform2[0];
            var xSize2 = transform2[1];
            var yOrigin2 = transform2[3];
            var ySize2 = transform2[5];
            // accept the origins as the same if they are within a tenth of the cell size
            var sameXOrigin = Math.Abs(xOrigin1 - xOrigin2) < xSize2 * 0.1;
            if (sameXOrigin) {
                var sameYOrigin = Math.Abs(yOrigin1 - yOrigin2) < Math.Abs(ySize2) * 0.1;
                if (sameYOrigin) {
                    // accept cell sizes as equivalent if  vertical/horizontal difference 
                    // in cell size times the number of rows/columns
                    // in the first is less than half the depth/width of a cell in the second
                    var sameXSize = Math.Abs(xSize1 - xSize2) * numCols1 < xSize2 * 0.5;
                    if (sameXSize) {
                        var sameYSize = Math.Abs(ySize1 - ySize2) * numRows1 < Math.Abs(ySize2) * 0.5;
                        return sameYSize;
                    }
                }
            }
            return false;
        }

        //// lakeGeom is a polygon representing a lake.  reach is known to intersect with the lake..
        ////         Returns a pair of inflowing and outflowing reaches, either or both of which may be None.
        //public virtual void splitReachByLake(object lakeGeom, object reachGeom, ReachData reachData) {
        //    var sourcePt = MapPointBuilderEx.CreateMapPoint(reachData.upperX, reachData.upperY);
        //    var sourceToLake = Topology.toIntersection(reachGeom, lakeGeom, sourcePt, !this.outletAtStart, this.xThreshold, this.yThreshold);
        //    var outletPt = MapPointBuilderEx.CreateMapPoint(reachData.lowerX, reachData.lowerY);
        //    var outletToLake = Topology.toIntersection(reachGeom, lakeGeom, outletPt, this.outletAtStart, this.xThreshold, this.yThreshold);
        //    return (sourceToLake, outletToLake);
        //}

        //// Return geometry for sequence of points from start to one before first one that intersects with lakeGeom, 
        ////         or None if this is empty or a singleton, or if start is within the lake.
        ////         
        ////         If isUp the search is from index 0 if the of the reach, else it is from the last index.
       
        //public static object toIntersection(
        //    OSGeo.OGR.Geometry reachGeom,
        //    OSGeo.OGR.Geometry lakeGeom,
        //    object start,
        //    bool isUp,
        //    double xThreshold,
        //    double yThreshold) {
        //    object length;
        //    object mpl;
        //    if (lakeGeom.Contains(start)) {
        //        return null;
        //    }
        //    if (reachGeom.isMultipart()) {
        //        mpl = reachGeom.asMultiPolyline();
        //    } else {
        //        mpl = new List<object> {
        //            reachGeom.asPolyline()
        //        };
        //    }
        //    var result = new List<object>();
        //    var done = new HashSet<object>();
        //    while (true) {
        //        var progress = false;
        //        foreach (var i in Enumerable.Range(0, mpl.Count)) {
        //            if (!done.Contains(i)) {
        //                var line = mpl[i];
        //                if (line.Count <= 1) {
        //                    continue;
        //                }
        //                if (isUp) {
        //                    if (Topology.coincidentPoints(start, line[0], xThreshold, yThreshold)) {
        //                        foreach (var pt in line) {
        //                            if (lakeGeom.contains(pt)) {
        //                                length = result.Count;
        //                                if (length < 1) {
        //                                    return null;
        //                                } else if (length == 1) {
        //                                    // create zero-length line at result[0]
        //                                    return QgsGeometry.fromPolylineXY(new List<List<object>> {
        //                                        result[0],
        //                                        result[0]
        //                                    });
        //                                }
        //                                return QgsGeometry.fromPolylineXY(result);
        //                            }
        //                            result.Add(pt);
        //                        }
        //                        start = line[^1];
        //                        done.Add(i);
        //                        progress = true;
        //                    }
        //                } else if (Topology.coincidentPoints(start, line[^1], xThreshold, yThreshold)) {
        //                    foreach (var pt in reversed(line)) {
        //                        if (lakeGeom.contains(pt)) {
        //                            length = result.Count;
        //                            if (length < 1) {
        //                                return null;
        //                            } else if (length == 1) {
        //                                // create zero-length line at result[0]
        //                                return QgsGeometry.fromPolylineXY(new List<List<object>> {
        //                                    result[0],
        //                                    result[0]
        //                                });
        //                            }
        //                            return QgsGeometry.fromPolylineXY(result);
        //                        }
        //                        result.insert(0, pt);
        //                    }
        //                    start = line[0];
        //                    done.Add(i);
        //                    progress = true;
        //                }
        //            }
        //        }
        //        if (!progress) {
        //            throw new Exception("Looping trying to calculate reach");
        //            //     @staticmethod
        //            //     def splitReach(resGeom, reachGeom, source, outlet, outletAtStart, xThreshold, yThreshold):
        //            //         """Split reachGeom into two parts, one from source to reservoir and one from reservoir to outlet.
        //            //         
        //            //         Assumes the reach has been split into at least two disjoint parts, one flowing from source, the other flowing to outlet.
        //            //         Algorithm checks each line in reach geometry, moving up from source or down from outlet until reservoir is reached
        //            //         in both cases."""
        //            //         sourcePart = []
        //            //         outletPart = []
        //            //         mpl = reachGeom.asMultiPolyline()
        //            //         done = set()
        //            //         outletToLakeDone = False
        //            //         sourceToLakeDone = False
        //            //         while True:
        //            //             reduced = False
        //            //             for i in xrange(len(mpl)):
        //            //                 if i not in done:
        //            //                     line = mpl[i]
        //            //                     start = line[0]
        //            //                     finish = line[-1]
        //            //                     if outletAtStart:
        //            //                         if not outletToLakeDone and Topology.coincidentPoints(outlet, start, xThreshold, yThreshold):
        //            //                             newLine = []
        //            //                             for pt in line:
        //            //                                 newLine.Add(pt)
        //            //                                 if resGeom.intersects(QgsGeometry.fromPointXY(pt)):
        //            //                                     outletToLakeDone = True 
        //            //                                     break
        //            //                             outletPart.Add(newLine)
        //            //                             outlet = finish
        //            //                             reduced = True
        //            //                             done.Add(i)
        //            //                         elif not sourceToLakeDone and Topology.coincidentPoints(source, finish, xThreshold, yThreshold):
        //            //                             newLine = []
        //            //                             for pt in reversed(line):
        //            //                                 newLine.insert(0, pt)
        //            //                                 if resGeom.intersects(QgsGeometry.fromPointXY(pt)):
        //            //                                     sourceToLakeDone = True 
        //            //                                     break
        //            //                             sourcePart.Add(newLine)
        //            //                             source = start
        //            //                             done.Add(i)
        //            //                             reduced = True
        //            //                     else:
        //            //                         if not outletToLakeDone and Topology.coincidentPoints(outlet, finish, xThreshold, yThreshold):
        //            //                             newLine = []
        //            //                             for pt in reversed(line):
        //            //                                 newLine.insert(0, pt)
        //            //                                 if resGeom.intersects(QgsGeometry.fromPointXY(pt)):
        //            //                                     outletToLakeDone = True 
        //            //                                     break
        //            //                             outletPart.Add(line)
        //            //                             outlet = start
        //            //                             done.Add(i)
        //            //                             reduced = True
        //            //                         elif Topology.coincidentPoints(source, start, xThreshold, yThreshold):
        //            //                             newLine = []
        //            //                             for pt in line:
        //            //                                 newLine.Add(pt)
        //            //                                 if resGeom.intersects(QgsGeometry.fromPointXY(pt)):
        //            //                                     sourceToLakeDone = True 
        //            //                                     break
        //            //                             sourcePart.Add(line)
        //            //                             source = finish
        //            //                             done.Add(i)
        //            //                             reduced = True
        //            //             if outletToLakeDone and sourceToLakeDone:
        //            //                 break
        //            //             if not reduced:
        //            //                 raise Exception('Looping trying to split reach')
        //            //         sourceGeom = QgsGeometry.fromPolyline(sourcePart[0]) if len(sourcePart) == 1 else QgsGeometry.fromMultiPolyline(sourcePart)
        //            //         outletGeom = QgsGeometry.fromPolyline(outletPart[0]) if len(outletPart) == 1 else QgsGeometry.fromMultiPolyline(outletPart)
        //            //         return sourceGeom, outletGeom
        //        }
        //    }
        //}

        // Point pt is contained in lake.  Move it downstream at most maxSteps
        //         using D8 flow direction raster pFile until it is not inside the lake,
        //         returning new point and true.
        //         
        //         Return original point and false if failed to find perimeter.
       
        public static (double[], bool) movePointToPerimeter(double[] pt, OSGeo.OGR.Geometry lakeGeom, string pFile, int maxSteps) {
            using (var ds = Gdal.Open(pFile, 0)) {
                Band band = ds.GetRasterBand(1);
                double[] transform = new double[6];
                ds.GetGeoTransform(transform);
                var stepCount = 0;
                double[] pt1 = new double[3] { pt[0], pt[1], 0 };
                while (stepCount < maxSteps) {
                    var pt1Geom = Topology.geometryFromPoint(pt1); 
                    if (!lakeGeom.Contains(pt1Geom)) {
                        return (pt1, true);
                    }
                    var dir1 = Topology.valueAtPoint(pt1, transform, band);
                    if (double.IsNaN(dir1)) {
                        Utils.loginfo("Failed to reach lake perimeter: no flow direction available.");
                        return (pt, false);
                    }
                    // dir1 is read as a float.  Also subtract 1 to get range 0..7
                    var dir0 = Convert.ToInt32(dir1) - 1;
                    var (col, row) = Topology.projToCell(pt1[0], pt1[1], transform);
                    var col1 = col + Utils._dX[dir0];
                    var row1 = row + Utils._dY[dir0];
                    var (x1, y1) = Topology.cellToProj(col1, row1, transform);
                    pt1[0] = x1;
                    pt1[1] = y1;
                    stepCount += 1;
                }
                Utils.loginfo(string.Format("Failed to reach lake perimeter in {0} steps.", maxSteps));
                return (pt, false);
            }
        }

        public static OSGeo.OGR.Geometry geometryFromPoint(double[] pt) {
            return OSGeo.OGR.Geometry.CreateFromWkt(string.Format("POINT ({0} {1})", pt[0], pt[1]));
        }

        public static OSGeo.OGR.Geometry geometryFromXY(double x, double y) {
            return OSGeo.OGR.Geometry.CreateFromWkt(string.Format("POINT ({0} {1})", x, y));
        }

       
        public static string lakeCategory(WaterRole waterRole) {
            if (waterRole == WaterRole._UNKNOWN) {
                return "unknown";
            } else if (waterRole == WaterRole._RESERVOIR) {
                return "reservoir";
            } else if (waterRole == WaterRole._POND) {
                return "pond";
            } else if (waterRole == WaterRole._WETLAND) {
                return "wetland";
            } else {
                return "playa";
            }
        }
    }
}

