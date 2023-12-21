
using QSettings = qgis.PyQt.QtCore.QSettings;

using QVariant = qgis.PyQt.QtCore.QVariant;

using QgsCoordinateReferenceSystem = qgis.core.QgsCoordinateReferenceSystem;

using QgsUnitTypes = qgis.core.QgsUnitTypes;

using QgsCoordinateTransform = qgis.core.QgsCoordinateTransform;

using QgsProject = qgis.core.QgsProject;

using QgsFeatureRequest = qgis.core.QgsFeatureRequest;

using QgsField = qgis.core.QgsField;

using QgsFeature = qgis.core.QgsFeature;

using QgsVectorLayer = qgis.core.QgsVectorLayer;

using QgsPointXY = qgis.core.QgsPointXY;

using QgsRasterLayer = qgis.core.QgsRasterLayer;

using QgsExpression = qgis.core.QgsExpression;

using QgsGeometry = qgis.core.QgsGeometry;

using QgsVectorDataProvider = qgis.core.QgsVectorDataProvider;

using QgsRectangle = qgis.core.QgsRectangle;

using QgsLayerTreeGroup = qgis.core.QgsLayerTreeGroup;

using QgsLayerTreeLayer = qgis.core.QgsLayerTreeLayer;

using QgsJsonExporter = qgis.core.QgsJsonExporter;

using gdal = osgeo.gdal;

using math = numpy.math;

using array = numpy.array;

using ndarray = numpy.ndarray;

using zeros = numpy.zeros;

using os.path;

using time;

using csv;

using traceback;

using Set = typing.Set;

using List = typing.List;

using Dict = typing.Dict;

using Tuple = typing.Tuple;

using Iterable = typing.Iterable;

using Iterator = typing.Iterator;

using cast = typing.cast;

using Any = typing.Any;

using Optional = typing.Optional;

using Union = typing.Union;

using Callable = typing.Callable;

using TYPE_CHECKING = typing.TYPE_CHECKING;

using QSWATUtils = QSWATUtils.QSWATUtils;

using FileTypes = QSWATUtils.FileTypes;

using ListFuns = QSWATUtils.ListFuns;

using DBUtils = DBUtils.DBUtils;

using Parameters = parameters.Parameters;

using Raster = raster.Raster;

using ReachData = dataInC.ReachData;

using MergedChannelData = dataInC.MergedChannelData;

using LakeData = dataInC.LakeData;

using LSUData = dataInC.LSUData;

using BasinData = dataInC.BasinData;

using QSWATUtils = QSWATUtils.QSWATUtils;

using FileTypes = QSWATUtils.FileTypes;

using ListFuns = QSWATUtils.ListFuns;

using DBUtils = DBUtils.DBUtils;

using Parameters = parameters.Parameters;

using Raster = raster.Raster;

using ReachData = dataInC.ReachData;

using MergedChannelData = dataInC.MergedChannelData;

using LakeData = dataInC.LakeData;

using LSUData = dataInC.LSUData;

using BasinData = dataInC.BasinData;

using System.Collections.Generic;

using System.Diagnostics;

using System;

using System.Linq;

using System.Collections;

public static class QSWATTopology {
    
    static QSWATTopology() {
        @"
/***************************************************************************
 QSWAT
                                 A QGIS plugin
 Create SWAT inputs
                              -------------------
        begin                : 2014-07-18
        copyright            : (C) 2014 by Chris George
        email                : cgeorge@mcmaster.ca
 ***************************************************************************/

/***************************************************************************
 *                                                                         *
 *   This program is free software; you can redistribute it and/or modify  *
 *   it under the terms of the GNU General Public License as published by  *
 *   the Free Software Foundation; either version 2 of the License, or     *
 *   (at your option) any later version.                                   *
 *                                                                         *
 ***************************************************************************/
";
    }
    
    // Module for creating and storing topological data 
    //     derived from watershed delineation.
    //     
    //     Nomenclature:  From TauDEM we have channels and also streams, each with link and wsno values.
    //     We translate as follows:
    //     channel link: channel
    //     channel wsno: chBasin
    //     stream link: stream
    //     stream wsno: [sub]basin
    //     These all have numbers from zero.  To avoid zero ids in the output, a SWATchannel etc has an id
    //     of at least one.
    //     Unlike QSWAT, we do not guarantee SWAT identifiers will form a continuous range 1..n.
    //     
    public class QSWATTopology {
        
        public dict basinCentroids;
        
        public object channelIndex;
        
        public dict channelLengths;
        
        public dict channelsData;
        
        public dict channelSlopes;
        
        public dict channelToSWATChannel;
        
        public dict chBasinAreas;
        
        public dict chBasinToChLink;
        
        public dict chBasinToSubbasin;
        
        public dict chLinkFromLake;
        
        public dict chLinkInsideLake;
        
        public dict chLinkIntoLake;
        
        public dict chLinkToChBasin;
        
        public dict chLinkToInlet;
        
        public dict chLinkToPtSrc;
        
        public dict chLinkToWater;
        
        public dict chOutlets;
        
        public dict chPointSources;
        
        public int chunkCount;
        
        public object crsLatLong;
        
        public object db;
        
        public object demExtent;
        
        public object demNodata;
        
        public dict distancesToJoins;
        
        public dict downChannels;
        
        public dict downStreams;
        
        public dict downSubbasins;
        
        public object drainAreas;
        
        public object dsChannelIndex;
        
        public object dx;
        
        public object dy;
        
        public dict foundReservoirs;
        
        public object horizontalFactor;
        
        public dict inlets;
        
        public object isBatch;
        
        public object isHAWQS;
        
        public object isHUC;
        
        public dict lakeInlets;
        
        public dict lakeOutlets;
        
        public dict lakesData;
        
        public object lostDsNodeIds;
        
        public dict maxFlowLengths;
        
        public object origChBasinAreas;
        
        public object outletAtStart;
        
        public dict outlets;
        
        public dict outletsInLake;
        
        public int pointId;
        
        public object strahler;
        
        public dict streamLengths;
        
        public dict subbasinToStream;
        
        public dict subbasinToSWATBasin;
        
        public dict SWATBasinToSubbasin;
        
        public dict SWATChannelToChannel;
        
        public object transformToLatLong;
        
        public object upstreamFromInlets;
        
        public object verticalFactor;
        
        public int waterBodyId;
        
        public int wsnoIndex;
        
        public object xThreshold;
        
        public object yThreshold;
        
        public object zeroChannels;
        
        public object _NOBASIN = -2;
        
        public object _RESTYPE = 1;
        
        public object _PONDTYPE = 2;
        
        public object _WETLANDTYPE = 3;
        
        public object _PLAYATYPE = 4;
        
        public object _TOTAL = "tot";
        
        public object _RECHARGE = "rhg";
        
        public object _SURFACE = "sur";
        
        public object _LATERAL = "lat";
        
        public object _TILE = "til";
        
        public object _LINKNO = "LINKNO";
        
        public object _DSLINKNO = "DSLINKNO";
        
        public object _DSLINKNO1 = "DSLINKNO1";
        
        public object _DSLINKNO2 = "DSLINKNO2";
        
        public object _USLINKNO1 = "USLINKNO1";
        
        public object _USLINKNO2 = "USLINKNO2";
        
        public object _DSNODEID = "DSNODEID";
        
        public object _DSNODEID1 = "DSNODEID1";
        
        public object _DSNODEID2 = "DSNODEID2";
        
        public object _DRAINAREA = "DSContArea";
        
        public object _DRAINAREA2 = "DS_Cont_Ar";
        
        public object _DRAINAGE = "Drainage";
        
        public object _ORDER = "strmOrder";
        
        public object _ORDER2 = "Order";
        
        public object _ORDERHUC = "streamorde";
        
        public object _LENGTH = "Length";
        
        public object _MAGNITUDE = "Magnitude";
        
        public object _DROP = "strmDrop";
        
        public object _DROP2 = "Drop";
        
        public object _SLOPE = "Slope";
        
        public object _STRAIGHTL = "StraightL";
        
        public object _STRAIGHTL2 = "Straight_L";
        
        public object _USCONTAR = "USContArea";
        
        public object _USCONTAR2 = "US_Cont_Ar";
        
        public object _WSNO = "WSNO";
        
        public object _DOUTEND = "DOUTEND";
        
        public object _DOUTEND2 = "DOUT_END";
        
        public object _DOUTSTART = "DOUTSTART";
        
        public object _DOUTSTART2 = "DOUT_START";
        
        public object _DOUTMID = "DOUTMID";
        
        public object _DOUTMID2 = "DOUT_MID";
        
        public object _BASINNO = "BasinNo";
        
        public object _ID = "ID";
        
        public object _INLET = "INLET";
        
        public object _RES = "RES";
        
        public object _PTSOURCE = "PTSOURCE";
        
        public object _ADDED = "ADDED";
        
        public object _POLYGONID = "PolygonId";
        
        public object _DOWNID = "DownId";
        
        public object _STREAMLINK = "StreamLink";
        
        public object _STREAMLEN = "StreamLen";
        
        public object _DSNODEIDW = "DSNodeID";
        
        public object _DSWSID = "DSWSID";
        
        public object _US1WSID = "US1WSID";
        
        public object _US2WSID = "US2WSID";
        
        public object _SUBBASIN = "Subbasin";
        
        public object _CHANNEL = "Channel";
        
        public object _CHANNELR = "ChannelR";
        
        public object _LANDSCAPE = "Landscape";
        
        public object _AQUIFER = "Aquifer";
        
        public object _LSU = "LSU";
        
        public object _LSUID = "LSUID";
        
        public object _PENWIDTH = "PenWidth";
        
        public object _HRUS = "HRUS";
        
        public object _HRUGIS = "HRUGIS";
        
        public object _LAKEID = "LakeId";
        
        public object _RESERVOIR = "Reservoir";
        
        public object _POND = "Pond";
        
        public object _AREAC = "AreaC";
        
        public object _LEN2 = "Len2";
        
        public object _SLO2 = "Slo2";
        
        public object _WID2 = "Wid2";
        
        public object _DEP2 = "Dep2";
        
        public object _MINEL = "MinEl";
        
        public object _MAXEL = "MaxEl";
        
        public object _LAKEIN = "LakeIn";
        
        public object _LAKEOUT = "LakeOut";
        
        public object _LAKEWITHIN = "LakeWithin";
        
        public object _LAKEMAIN = "LakeMain";
        
        public object _POINTID = "PointId";
        
        public object _SOURCEX = "SourceX";
        
        public object _SOURCEY = "SourceY";
        
        public object _OUTLETX = "OutletX";
        
        public object _OUTLETY = "OutletY";
        
        public object _TOTDASQKM = "TotDASqKm";
        
        public object _HUCPointId = 100000;
        
        public QSWATTopology(bool isBatch, bool isHUC, bool isHAWQS) {
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
            this.chBasinToSubbasin = new dict();
            //# WSNO does not obey SWAT rules for element numbers (> 0) 
            // so we invent and store SWATBasin numbers
            // also SWAT basins may not be empty
            // not complete: zero areas/stream lengths excluded
            this.subbasinToSWATBasin = new dict();
            //#inverse map to make it easy to output in numeric order of SWAT Basins
            this.SWATBasinToSubbasin = new dict();
            //# original channel links may have zero numbers and may have zero lengths
            //# so we generate SWATChannel ids
            // not complete
            this.channelToSWATChannel = new dict();
            //# inverse map
            this.SWATChannelToChannel = new dict();
            //# subbasin to stream mapping (wsno to link fields in streams shapefile)
            // complete
            this.subbasinToStream = new dict();
            //# stream to downstream (link to dslink in streams shapefile)
            // complete
            this.downStreams = new dict();
            //# stream lengths (link to length in streams shapefile).  Lengths are in metres
            // complete
            this.streamLengths = new dict();
            //# LINKNO to DSLINKNO in channel shapefile
            // complete = all channel links defined
            this.downChannels = new dict();
            //# zero length channels
            this.zeroChannels = new HashSet<object>();
            //# subbasin to downstream subbasin
            // incomplete: no empty basins (zero length streams or missing wsno value in subbasins layer)
            this.downSubbasins = new dict();
            //# map from channel to chBasin
            // incomplete - no zero length channels
            this.chLinkToChBasin = new dict();
            //# reverse of chLinkToChBasin
            this.chBasinToChLink = new dict();
            //# centroids of basins as (x, y) pairs in projected units
            this.basinCentroids = new dict();
            //# channel link to channel length in metres:
            // complete
            this.channelLengths = new dict();
            //# channel slopes in m/m
            // complete
            this.channelSlopes = new dict();
            //# numpy array of total area draining to downstream end of channel in square metres
            this.drainAreas = null;
            //# numpy array of Strahler order of channels
            this.strahler = null;
            //# map of lake id to ids of points added to split channels entering lakes
            this.lakeInlets = new dict();
            //# map of lake id to ids of points added to split channels leaving lakes
            this.lakeOutlets = new dict();
            //# map of channel to ReachData: points and elevations at ends of channels, plus basin
            // not complete: zero areas/channel lengths excluded
            this.channelsData = new dict();
            //# map of lake id to LakeData for lakes defined by shapefile
            this.lakesData = new dict();
            //# map of channel links to lake ids: channel flowing into lake
            this.chLinkIntoLake = new dict();
            //# map of channel links to lake ids: channel completely inside lake
            this.chLinkInsideLake = new dict();
            //# map of channel links to lake ids: channel flowing out of lake
            this.chLinkFromLake = new dict();
            //# map of subbasin to lake id for subbasins with their outlet inside a lake (non-grid models only)
            this.outletsInLake = new dict();
            //# channel basin to area in square metres.  Not used with grid model.
            this.chBasinAreas = new dict();
            // original of chBasinAreas for use after chBasinAreas affected by merging, in case merging repeated
            this.origChBasinAreas = new dict();
            //# current point id (for outlets, inlets and point sources)
            this.pointId = 0;
            //# current water body id (for lakes, reservoirs and ponds)
            this.waterBodyId = 0;
            //# channel links to reservoir or pond point ids plus water type: reservoir or pond discharges into channel
            this.chLinkToWater = new dict();
            //# channel links with point sources flowing into them (defined by inlets/outlets file)
            this.chLinkToPtSrc = new dict();
            //# channel links to watershed inlets (for grid models only)
            this.chLinkToInlet = new dict();
            //# basins draining to inlets
            this.upstreamFromInlets = new HashSet<object>();
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
            this.outlets = new dict();
            //# map from subbasin to inlet pointId and point (not used with grid models)
            this.inlets = new dict();
            //# map from channel links to outlet end
            this.chOutlets = new dict();
            //# map from channel links to point sources
            this.chPointSources = new dict();
            //# reservoirs found by converting water HRUs
            this.foundReservoirs = new dict();
            //# lat-long coordinate reference system
            this.crsLatLong = QgsCoordinateReferenceSystem("EPSG:4326");
            //# transform from project corrdinates to lat-long
            this.transformToLatLong = null;
            //# Flag to show if batch run
            this.isBatch = isBatch;
            //# flag for HUC projects
            this.isHUC = isHUC;
            //# flag for HAWQS projects
            this.isHAWQS = isHAWQS;
            //# table for memorizing distances from basin to join in flowpath with other basin:
            // basin -> otherBasin -> join distance in metres
            this.distancesToJoins = new dict();
            //# table for use in existing non-grid models of maximum channel flow lengths in metres to subbasin outlets
            // complete
            this.maxFlowLengths = new dict();
            //# number of chunks to use for rasters and their arrays; increased when memory fails
            this.chunkCount = 1;
            //# dsNodeIds that cannot be retained when making grids as they would be in same grid cell as another point
            this.lostDsNodeIds = new HashSet<object>();
        }
        
        // Set DEM size parameters and stream orientation, and store source and outlet points for stream reaches.
        public virtual bool setUp0(
            object demLayer,
            object channelLayer,
            object outletLayer,
            object ad8Layer,
            object gv) {
            // can fail if demLayer is None or not projected
            try {
                this.setCrs(demLayer, gv);
                Debug.Assert(gv.crsProject is not null);
                var units = gv.crsProject.mapUnits();
            } catch (Exception) {
                QSWATUtils.loginfo("Failure to read DEM units: {0}".format(traceback.format_exc()));
                return false;
            }
            if (units == QgsUnitTypes.DistanceMeters) {
                gv.horizontalFactor = 1;
            } else if (units == QgsUnitTypes.DistanceFeet) {
                gv.horizontalFactor = Parameters._FEETTOMETRES;
            } else {
                // unknown or degrees - will be reported in delineation - just quietly fail here
                QSWATUtils.loginfo("Failure to read DEM units: {0}".format(units.ToString()));
                return false;
            }
            this.demNodata = demLayer.dataProvider().sourceNoDataValue(1);
            this.dx = demLayer.rasterUnitsPerPixelX() * gv.horizontalFactor;
            this.dy = demLayer.rasterUnitsPerPixelY() * gv.horizontalFactor;
            this.xThreshold = this.dx / gv.horizontalFactor * Parameters._NEARNESSTHRESHOLD;
            this.yThreshold = this.dy / gv.horizontalFactor * Parameters._NEARNESSTHRESHOLD;
            QSWATUtils.loginfo("Factor is {0}, cell width is {1}, cell depth is {2}".format(gv.horizontalFactor, this.dx, this.dy));
            this.demExtent = demLayer.extent();
            this.horizontalFactor = gv.horizontalFactor;
            this.verticalFactor = gv.verticalFactor;
            this.outletAtStart = this.hasOutletAtStart(channelLayer, ad8Layer);
            QSWATUtils.loginfo("Outlet at start is {0!s}".format(this.outletAtStart));
            this.outletsInLake.clear();
            return this.saveOutletsAndSources(channelLayer, outletLayer, gv.useGridModel);
        }
        
        // Set crsProject and transformToLatLong if necessary.
        public virtual object setCrs(object demLayer, object gv) {
            if (gv.crsProject is null) {
                gv.crsProject = demLayer.crs();
                this.transformToLatLong = QgsCoordinateTransform(gv.crsProject, this.crsLatLong, QgsProject.instance());
                QgsProject.instance().setCrs(gv.crsProject);
                var settings = QSettings();
                settings.setValue("Projections/defaultBehaviour", "useProject");
            }
        }
        
        // Establish subbasinToStream, downStreams and streamLengths dictionaries.
        //         
        //         Used when calculating ridges by branch length method and setUp has not been run yet.
        public virtual bool setUp1(object streamLayer) {
            object length;
            this.subbasinToStream.clear();
            this.downStreams.clear();
            this.streamLengths.clear();
            var streamIndex = this.getIndex(streamLayer, QSWATTopology._LINKNO);
            if (streamIndex < 0) {
                QSWATUtils.loginfo("No LINKNO field in stream layer");
                return false;
            }
            var dsStreamIndex = this.getIndex(streamLayer, QSWATTopology._DSLINKNO);
            if (dsStreamIndex < 0) {
                QSWATUtils.loginfo("No DSLINKNO field in stream layer");
                return false;
            }
            var lengthIndex = this.getIndex(streamLayer, QSWATTopology._LENGTH, ignoreMissing: true);
            var wsnoIndex = this.getIndex(streamLayer, QSWATTopology._WSNO);
            if (wsnoIndex < 0) {
                QSWATUtils.loginfo("No WSNO field in stream layer");
                return false;
            }
            foreach (var reach in streamLayer.getFeatures()) {
                var link = reach[streamIndex];
                var dsLink = reach[dsStreamIndex];
                var basin = reach[wsnoIndex];
                if (lengthIndex < 0) {
                    length = reach.geometry().length() * this.horizontalFactor;
                } else {
                    length = reach[lengthIndex] * this.horizontalFactor;
                }
                this.subbasinToStream[basin] = link;
                this.downStreams[link] = dsLink;
                this.streamLengths[link] = length;
            }
            return true;
        }
        
        // Create topological data from layers.
        public virtual bool setUp(
            object demLayer,
            object channelLayer,
            object subbasinsLayer,
            object outletLayer,
            object lakesLayer,
            object gv,
            bool existing,
            bool recalculate,
            bool useGridModel,
            bool streamDrainage,
            bool reportErrors) {
            object subbasin;
            object request;
            object ok;
            object pt;
            object typ;
            object isInlet;
            object isPtSource;
            object features;
            object dsNode;
            object drop;
            object data;
            object length;
            object chBasin;
            object chLink;
            //QSWATUtils.loginfo('Channel layer {0}'.format(channelLayer.dataProvider().dataSourceUri()))
            //QSWATUtils.loginfo('Subbasins layer {0}'.format(subbasinsLayer.dataProvider().dataSourceUri()))
            this.db = gv.db;
            this.chLinkToChBasin.clear();
            this.chBasinToChLink.clear();
            this.subbasinToSWATBasin.clear();
            this.SWATBasinToSubbasin.clear();
            this.channelToSWATChannel.clear();
            this.SWATChannelToChannel.clear();
            this.downChannels.clear();
            this.zeroChannels.clear();
            // do not clear centroids unless existing and not using grid model: 
            if (existing && !useGridModel) {
                this.basinCentroids.clear();
            }
            this.channelLengths.clear();
            this.channelSlopes.clear();
            this.channelsData.clear();
            this.chLinkToWater.clear();
            this.chLinkToPtSrc.clear();
            this.chLinkToInlet.clear();
            this.distancesToJoins.clear();
            this.maxFlowLengths.clear();
            var dsNodeToLink = new dict();
            var ignoreError = !reportErrors;
            var ignoreWithExisting = existing || !reportErrors;
            var ignoreWithGrid = useGridModel || !reportErrors;
            var ignoreWithGridOrExisting = ignoreWithGrid || ignoreWithExisting;
            var ignoreWithGridOrExistingOrWin = ignoreWithGridOrExisting || Parameters._ISWIN;
            var ignoreTotDA = !(this.isHUC || this.isHAWQS);
            this.channelIndex = this.getIndex(channelLayer, QSWATTopology._LINKNO, ignoreMissing: ignoreError);
            if (this.channelIndex < 0) {
                QSWATUtils.loginfo("No LINKNO field in channels layer");
                return false;
            }
            var DSLINKNO = this.isHUC ? QSWATTopology._DSLINKNO1 : QSWATTopology._DSLINKNO;
            this.dsChannelIndex = this.getIndex(channelLayer, DSLINKNO, ignoreMissing: ignoreError);
            if (this.dsChannelIndex < 0) {
                QSWATUtils.loginfo("No DSLINKNO field in channels layer");
                return false;
            }
            var DSNODEID = this.isHUC ? QSWATTopology._DSNODEID1 : QSWATTopology._DSNODEID;
            var dsNodeIndex = this.getIndex(channelLayer, DSNODEID, ignoreMissing: ignoreWithExisting);
            var dsNode2Index = this.getIndex(channelLayer, QSWATTopology._DSNODEID2, ignoreMissing: !this.isHUC);
            this.wsnoIndex = this.getIndex(channelLayer, QSWATTopology._WSNO, ignoreMissing: ignoreError);
            if (this.wsnoIndex < 0) {
                QSWATUtils.loginfo("No WSNO field in channels layer");
                return false;
            }
            if (this.isHUC) {
                var orderIndex = this.getIndex(channelLayer, QSWATTopology._ORDERHUC, ignoreMissing: false);
            } else {
                orderIndex = this.getIndex(channelLayer, QSWATTopology._ORDER, ignoreMissing: ignoreWithGridOrExistingOrWin);
                if (orderIndex < 0 && ignoreWithGridOrExistingOrWin) {
                    orderIndex = this.getIndex(channelLayer, QSWATTopology._ORDER2, ignoreMissing: ignoreWithGridOrExisting);
                }
            }
            var drainAreaIndex = this.getIndex(channelLayer, QSWATTopology._DRAINAREA, ignoreMissing: ignoreWithGridOrExistingOrWin);
            if (drainAreaIndex < 0 && ignoreWithGridOrExistingOrWin) {
                drainAreaIndex = this.getIndex(channelLayer, QSWATTopology._DRAINAREA2, ignoreMissing: ignoreWithGridOrExisting);
            }
            var lengthIndex = this.getIndex(channelLayer, QSWATTopology._LENGTH, ignoreMissing: ignoreWithGridOrExisting);
            var dropIndex = this.getIndex(channelLayer, QSWATTopology._DROP, ignoreMissing: ignoreWithGridOrExistingOrWin);
            if (dropIndex < 0 && ignoreWithGridOrExistingOrWin) {
                dropIndex = this.getIndex(channelLayer, QSWATTopology._DROP2, ignoreMissing: ignoreWithGridOrExisting);
            }
            var totDAIndex = this.getIndex(channelLayer, QSWATTopology._TOTDASQKM, ignoreMissing: ignoreTotDA);
            var polyIndex = this.getIndex(subbasinsLayer, QSWATTopology._POLYGONID, ignoreMissing: ignoreError);
            if (polyIndex < 0) {
                QSWATUtils.loginfo("No POLYGONID field in subbasins layer");
                return false;
            }
            var subbasinIndex = this.getIndex(subbasinsLayer, QSWATTopology._SUBBASIN, ignoreMissing: ignoreWithGridOrExisting);
            if (outletLayer is not null) {
                if (dsNodeIndex < 0) {
                    QSWATUtils.information("Warning: streams layer has no {0} field, so points in inlets/outlets file will be ignored".format(QSWATTopology._DSNODEID), gv.isBatch);
                }
                var idIndex = this.getIndex(outletLayer, QSWATTopology._ID, ignoreMissing: ignoreError);
                if (idIndex < 0) {
                    QSWATUtils.loginfo("No ID field in outlets layer");
                    return false;
                }
                var inletIndex = this.getIndex(outletLayer, QSWATTopology._INLET, ignoreMissing: ignoreError);
                if (inletIndex < 0) {
                    QSWATUtils.loginfo("No INLET field in outlets layer");
                    return false;
                }
                var ptSourceIndex = this.getIndex(outletLayer, QSWATTopology._PTSOURCE, ignoreMissing: ignoreError);
                if (ptSourceIndex < 0) {
                    QSWATUtils.loginfo("No PTSOURCE field in outlets layer");
                    return false;
                }
                var resIndex = this.getIndex(outletLayer, QSWATTopology._RES, ignoreMissing: ignoreError);
                if (resIndex < 0) {
                    QSWATUtils.loginfo("No RES field in outlets layer");
                    return false;
                }
                var ptIdIndex = this.getIndex(outletLayer, QSWATTopology._POINTID, ignoreMissing: ignoreError);
                if (ptIdIndex < 0) {
                    QSWATUtils.loginfo("No PointId field in outlets layer");
                    return false;
                }
                // ADDED field only used with lakes: may be missing
                var addIndex = this.getIndex(outletLayer, QSWATTopology._ADDED, ignoreMissing: true);
            }
            // upstream links
            var us = new dict();
            var time1 = time.process_time();
            var maxChLink = 0;
            var SWATChannel = 0;
            // drainAreas is a mapping from link number (used as index to array) of grid cell areas in sq m
            if (this.isHUC || this.isHAWQS) {
                // make it a dictionary rather than a numpy array because there is a big gap
                // between most basin numbers and the nunbers for inlets (100000 +)
                // and we need to do no calculation
                // create it here for HUC and HAWQS models as we set it up from totDASqKm field in channelLayer
                this.drainAreas = new dict();
            }
            foreach (var channel in channelLayer.getFeatures()) {
                chLink = channel[this.channelIndex];
                var dsChLink = channel[this.dsChannelIndex];
                chBasin = channel[this.wsnoIndex];
                var geom = channel.geometry();
                if (lengthIndex < 0 || recalculate) {
                    length = geom.length() * gv.horizontalFactor;
                } else {
                    length = channel[lengthIndex] * gv.horizontalFactor;
                }
                if ((this.isHUC || this.isHAWQS) && length == 0) {
                    // allow for zero length inlet channels
                    data = null;
                } else {
                    data = this.getReachData(channel, demLayer);
                    this.channelsData[chLink] = data;
                }
                // don'use TauDEM drops: affected by burn-in
                // if data and (dropIndex < 0 or recalculate):
                //     drop = data.upperZ - data.lowerZ
                // elif dropIndex >= 0:
                //     drop = channel[dropIndex]
                // else:
                //     drop = 0
                if (data) {
                    drop = data.upperZ - data.lowerZ;
                } else {
                    drop = 0;
                }
                var slope = length <= 0 || drop < 0 ? 0 : float(drop) / length;
                dsNode = dsNodeIndex >= 0 ? channel[dsNodeIndex] : -1;
                var dsNode2 = dsNode2Index >= 0 ? channel[dsNode2Index] : -1;
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
                        this.zeroChannels.add(chLink);
                    }
                }
                maxChLink = max(maxChLink, chLink);
                this.downChannels[chLink] = dsChLink;
                this.channelLengths[chLink] = length;
                this.channelSlopes[chLink] = slope;
                if (dsNode >= 0) {
                    dsNodeToLink[dsNode] = chLink;
                    //QSWATUtils.loginfo('DSNode {0} mapped to channel link {1}'.format(dsNode, chLink))
                }
                if (dsNode2 >= 0) {
                    dsNodeToLink[dsNode2] = chLink;
                    //QSWATUtils.loginfo('Minor DSNode {0} mapped to channel link {1}'.format(dsNode2, chLink))
                }
                if (dsChLink >= 0 && !((this.isHUC || this.isHAWQS) && chLink >= QSWATTopology._HUCPointId)) {
                    // keep HUC links out of us map
                    var ups = us.setdefault(dsChLink, new List<object>());
                    ups.append(chLink);
                    if (!useGridModel) {
                        // try later to fix circularities in grid models
                        // check we haven't just made the us relation circular
                        if (QSWATTopology.reachable(dsChLink, new List<object> {
                            chLink
                        }, us)) {
                            QSWATUtils.error("Circular drainage network from channel link {0}".format(dsChLink), this.isBatch);
                            return false;
                        }
                    }
                }
                if (this.isHUC || this.isHAWQS) {
                    this.drainAreas[chLink] = channel[totDAIndex] * 1000000.0;
                }
            }
            var time2 = time.process_time();
            QSWATUtils.loginfo("Topology setup for channels took {0} seconds".format(Convert.ToInt32(time2 - time1)));
            if (!useGridModel) {
                this.setChannelBasinAreas(gv);
                this.origChBasinAreas = QSWATTopology.copyBasinAreas(this.chBasinAreas);
                if (existing) {
                    // need to set centroids
                    foreach (var polygon in subbasinsLayer.getFeatures()) {
                        var basin = polygon[polyIndex];
                        var centroid = polygon.geometry().centroid().asPoint();
                        this.basinCentroids[basin] = (centroid.x(), centroid.y());
                    }
                    // find maximum channel flow length for each subbasin
                    this.setMaxFlowLengths();
                }
            }
            var time3 = time.process_time();
            QSWATUtils.loginfo("Topology setup of subbasin areas and centroids took {0} seconds".format(Convert.ToInt32(time3 - time2)));
            if (outletLayer is not null) {
                features = outletLayer.getFeatures();
            } else {
                features = new List<object>();
            }
            if (dsNodeIndex >= 0) {
                var doneNodes = new HashSet<object>();
                foreach (var point in features) {
                    // ignore HUC reservoir and pond points: only for display
                    if (this.isHUC && point[resIndex] > 0) {
                        continue;
                    }
                    dsNode = point[idIndex];
                    if (doneNodes.Contains(dsNode)) {
                        if (reportErrors) {
                            QSWATUtils.error("ID value {0} is used more than once in inlets/outlets file {1}.  Occurrences after the first are ignored".format(dsNode, QSWATUtils.layerFilename(outletLayer)), this.isBatch);
                        }
                        chLink = -1;
                    } else if (this.lostDsNodeIds.Contains(dsNode)) {
                        chLink = -1;
                    } else if (!dsNodeToLink.Contains(dsNode)) {
                        if (this.isHUC || this.isHAWQS) {
                            // extra points can occur, typically duplicates: ignore
                            continue;
                        }
                        if (reportErrors) {
                            QSWATUtils.error(@"ID value {0} from inlets/outlets file {1} not found as DSNODEID in channels file {2}.  Will be ignored.
                        This can be caused by an occasional error in delineation: try rerunning 'Create watershed'.".format(dsNode, QSWATUtils.layerFilename(outletLayer), QSWATUtils.layerFileInfo(channelLayer).filePath()), this.isBatch);
                        }
                        chLink = -1;
                    } else {
                        chLink = dsNodeToLink[dsNode];
                    }
                    doneNodes.add(dsNode);
                    if (chLink >= 0) {
                        if (point[inletIndex] == 1) {
                            if (point[ptSourceIndex] == 1) {
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
                        var isReservoir = point[resIndex] == 1;
                        var isPond = point[resIndex] == 2;
                        var isAdded = addIndex >= 0 && point[addIndex] == 1;
                        if (!isAdded && lakesLayer is not null) {
                            // check if point is inside lake
                            foreach (var lake in lakesLayer.getFeatures()) {
                                var lakeGeom = lake.geometry();
                                var lakeRect = lakeGeom.boundingBox();
                                if (QSWATTopology.polyContains(point.geometry().asPoint(), lakeGeom, lakeRect)) {
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
                                    var lakeIdIndex = lakesLayer.dataProvider().fieldNameIndex(QSWATTopology._LAKEID);
                                    var lakeId = Convert.ToInt32(lake[lakeIdIndex]);
                                    QSWATUtils.information("{0} {1} is inside lake {2}.  Will be ignored.".format(typ, dsNode, lakeId), this.isBatch);
                                    break;
                                }
                            }
                        }
                        if (isInlet) {
                            if (isPtSource) {
                                pt = point.geometry().asPoint();
                                this.chLinkToPtSrc[chLink] = (point[ptIdIndex], pt);
                            } else if (useGridModel) {
                                // inlets collected in setUp0 for non-grids
                                pt = point.geometry().asPoint();
                                this.chLinkToInlet[chLink] = (point[ptIdIndex], pt);
                            }
                        } else if (isReservoir) {
                            pt = point.geometry().asPoint();
                            this.chLinkToWater[chLink] = (point[ptIdIndex], pt, QSWATTopology._RESTYPE);
                        } else if (isPond) {
                            pt = point.geometry().asPoint();
                            this.chLinkToWater[chLink] = (point[ptIdIndex], pt, QSWATTopology._PONDTYPE);
                        }
                        // else an outlet: nothing to do
                        // check for user-defined outlets coincident with stream junctions
                        if (this.zeroChannels.Contains(chLink) && !this.chLinkIntoLake.Contains(chLink) && !(isInlet && (this.isHUC || this.isHAWQS))) {
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
                                var msg = "{0} with id {1} has a zero length channel leading to it: please remove or move downstream".format(typ, dsNode);
                                if (reportErrors) {
                                    QSWATUtils.error(msg, this.isBatch);
                                } else {
                                    QSWATUtils.loginfo(msg);
                                }
                                return false;
                            }
                        }
                    }
                }
            }
            var time4 = time.process_time();
            QSWATUtils.loginfo("Topology setup for inlets/outlets took {0} seconds".format(Convert.ToInt32(time4 - time3)));
            // add any extra reservoirs and point sources
            // create drainAreas here for non-HUC models as we now have maxChLink value to size the numpy array
            if (!(this.isHUC || this.isHAWQS)) {
                this.drainAreas = zeros(maxChLink + 1, dtype: float);
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
            var time5 = time.process_time();
            QSWATUtils.loginfo("Topology drainage took {0} seconds".format(Convert.ToInt32(time5 - time4)));
            // Strahler order
            if (this.isHUC || this.isHAWQS) {
                // use dict because of range of channel links, as we did for drainAreas
                this.strahler = new dict();
            } else {
                this.strahler = zeros(maxChLink + 1, dtype: @int);
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
            if (!ok) {
                // failed attempt may have put data in these, so clear them
                this.subbasinToSWATBasin.clear();
                this.SWATBasinToSubbasin.clear();
                // create SWAT basin numbers
                var SWATBasin = 0;
                request = QgsFeatureRequest().setFlags(QgsFeatureRequest.NoGeometry).setSubsetOfAttributes(new List<int> {
                    polyIndex
                });
                foreach (var feature in subbasinsLayer.getFeatures(request)) {
                    subbasin = feature[polyIndex];
                    if (!this.upstreamFromInlets.Contains(subbasin) && (useGridModel || this.chBasinToSubbasin.values().Contains(subbasin))) {
                        // else was removed by chBasin being within a lake
                        SWATBasin += 1;
                        this.subbasinToSWATBasin[subbasin] = SWATBasin;
                        this.SWATBasinToSubbasin[SWATBasin] = subbasin;
                    }
                }
                // put SWAT Basin numbers in subbasin field of subbasins shapefile
                var subbasinsProvider = subbasinsLayer.dataProvider();
                if (subbasinIndex < 0) {
                    // need to add subbasin field
                    subbasinsProvider.addAttributes(new List<object> {
                        QgsField(QSWATTopology._SUBBASIN, QVariant.Int)
                    });
                    subbasinsLayer.updateFields();
                    subbasinIndex = subbasinsProvider.fieldNameIndex(QSWATTopology._SUBBASIN);
                }
                var mmap = new dict();
                request = QgsFeatureRequest().setFlags(QgsFeatureRequest.NoGeometry).setSubsetOfAttributes(new List<int> {
                    polyIndex
                });
                foreach (var feature in subbasinsProvider.getFeatures(request)) {
                    subbasin = feature[polyIndex];
                    SWATBasin = this.subbasinToSWATBasin.get(subbasin, 0);
                    mmap[feature.id()] = new Dictionary<object, object> {
                        {
                            subbasinIndex,
                            SWATBasin}};
                }
                subbasinsProvider.changeAttributeValues(mmap);
                // put SWAT Basin numbers in subbasin field of subsNoLakes shapefile if exists
                if (os.path.isfile(gv.subsNoLakesFile)) {
                    var subsNoLakesLayer = QgsVectorLayer(gv.subsNoLakesFile, "subsNoLakes", "ogr");
                    var subsNoLakesProvider = subsNoLakesLayer.dataProvider();
                    subbasinIndex = subsNoLakesProvider.fieldNameIndex(QSWATTopology._SUBBASIN);
                    if (subbasinIndex < 0) {
                        // need to add subbasin field
                        subsNoLakesProvider.addAttributes(new List<object> {
                            QgsField(QSWATTopology._SUBBASIN, QVariant.Int)
                        });
                        subsNoLakesLayer.updateFields();
                        subbasinIndex = subsNoLakesProvider.fieldNameIndex(QSWATTopology._SUBBASIN);
                    }
                    request = QgsFeatureRequest().setFlags(QgsFeatureRequest.NoGeometry).setSubsetOfAttributes(new List<int> {
                        polyIndex
                    });
                    mmap = new dict();
                    foreach (var feature in subsNoLakesProvider.getFeatures(request)) {
                        subbasin = feature[polyIndex];
                        SWATBasin = this.subbasinToSWATBasin.get(subbasin, 0);
                        mmap[feature.id()] = new Dictionary<object, object> {
                            {
                                subbasinIndex,
                                SWATBasin}};
                    }
                    subsNoLakesProvider.changeAttributeValues(mmap);
                }
            }
            var time6 = time.process_time();
            QSWATUtils.loginfo("Topology setting SWATBasin numbers took {0} seconds".format(Convert.ToInt32(time6 - time5)));
            subbasinsLayer.setLabelsEnabled(true);
            subbasinsLayer.triggerRepaint();
            if (!useGridModel) {
                // add SWAT channel numbers to watershed shapefile
                // in case loaded
                var root = QgsProject.instance().layerTreeRoot();
                (wshedLayer, _) = QSWATUtils.getLayerByFilename(root.findLayers(), gv.wshedFile, FileTypes._WATERSHED, null, null, null);
                if (wshedLayer is null) {
                    var wshedLayer = QgsVectorLayer(gv.wshedFile, FileTypes.legend(FileTypes._WATERSHED), "ogr");
                }
                var wshedPolyIndex = this.getIndex(wshedLayer, QSWATTopology._POLYGONID, ignoreMissing: ignoreError);
                var wshedChannelIndex = this.getIndex(wshedLayer, QSWATTopology._CHANNEL, ignoreMissing: ignoreWithGridOrExisting);
                wshedLayer.startEditing();
                if (wshedChannelIndex < 0) {
                    wshedLayer.dataProvider().addAttributes(new List<object> {
                        QgsField(QSWATTopology._CHANNEL, QVariant.Int)
                    });
                    wshedLayer.updateFields();
                    wshedChannelIndex = wshedLayer.fields().indexOf(QSWATTopology._CHANNEL);
                }
                request = QgsFeatureRequest().setFlags(QgsFeatureRequest.NoGeometry).setSubsetOfAttributes(new List<int> {
                    wshedPolyIndex
                });
                foreach (var feature in wshedLayer.getFeatures(request)) {
                    chBasin = feature[wshedPolyIndex];
                    chLink = this.chBasinToChLink.get(chBasin, -1);
                    SWATChannel = this.channelToSWATChannel.get(chLink, 0);
                    wshedLayer.changeAttributeValue(feature.id(), wshedChannelIndex, SWATChannel);
                }
                wshedLayer.commitChanges();
            }
            var drainageFile = QSWATUtils.join(gv.shapesDir, gv.projName + Parameters._DRAINAGECSV);
            this.writeDrainageFile(drainageFile);
            return useGridModel || lakesLayer is not null || this.checkAreas(subbasinsLayer, gv);
        }
        
        // Add lakes from lakes shapefile layer.
        //         
        //         Not used with grid models.
        public virtual bool addLakes(
            object lakesLayer,
            object subbasinsLayer,
            object chBasinsLayer,
            object streamsLayer,
            object channelsLayer,
            object demLayer,
            double snapThreshold,
            object gv,
            bool reportErrors = true) {
            object channel;
            object request;
            object exp;
            object newPointId;
            object channelId;
            object elev;
            object link;
            object subbasin;
            object area2;
            object newGeom;
            object area1;
            object waterRole;
            var errorCount = 0;
            var lakesProvider = lakesLayer.dataProvider();
            var lakeIdIndex = lakesProvider.fieldNameIndex(QSWATTopology._LAKEID);
            var lakeResIndex = lakesProvider.fieldNameIndex(QSWATTopology._RES);
            var lakeAreaIndex = lakesProvider.fieldNameIndex(Parameters._AREA);
            if (lakeResIndex < 0) {
                QSWATUtils.information("No RES field in lakes shapefile {0}: assuming lakes are reservoirs".format(QSWATUtils.layerFilename(lakesLayer)), this.isBatch, reportErrors: reportErrors);
            }
            var subsProvider = subbasinsLayer.dataProvider();
            var subsPolyIndex = subsProvider.fieldNameIndex(QSWATTopology._POLYGONID);
            var subsAreaIndex = subsProvider.fieldNameIndex(Parameters._AREA);
            if (subsAreaIndex < 0) {
                QSWATUtils.error("Cannot find {0} field in {1}".format(Parameters._AREA, gv.subbasinsFile), this.isBatch);
                return false;
            }
            var chBasinsProvider = chBasinsLayer.dataProvider();
            var chBasinsPolyIndex = chBasinsProvider.fieldNameIndex(QSWATTopology._POLYGONID);
            var chBasinsAreaIndex = chBasinsProvider.fieldNameIndex(Parameters._AREA);
            var channelsProvider = channelsLayer.dataProvider();
            var channelLinkIndex = channelsProvider.fieldNameIndex(QSWATTopology._LINKNO);
            var DSLINKNO = this.isHUC ? QSWATTopology._DSLINKNO1 : QSWATTopology._DSLINKNO;
            var channelDsLinkIndex = channelsProvider.fieldNameIndex(DSLINKNO);
            var DSNODEID = this.isHUC ? QSWATTopology._DSNODEID1 : QSWATTopology._DSNODEID;
            var channelDsNodeIndex = channelsProvider.fieldNameIndex(DSNODEID);
            var channelDrainAreaIndex = channelsProvider.fieldNameIndex(QSWATTopology._DRAINAREA);
            if (channelDrainAreaIndex < 0) {
                channelDrainAreaIndex = channelsProvider.fieldNameIndex(QSWATTopology._DRAINAREA2);
            }
            var channelWSNOIndex = channelsProvider.fieldNameIndex(QSWATTopology._WSNO);
            var channelBasinIndex = channelsProvider.fieldNameIndex(QSWATTopology._BASINNO);
            this.addLakeFieldsToChannels(channelsLayer);
            var channelLakeInIndex = channelsProvider.fieldNameIndex(QSWATTopology._LAKEIN);
            var channelLakeOutIndex = channelsProvider.fieldNameIndex(QSWATTopology._LAKEOUT);
            var channelLakeWithinIndex = channelsProvider.fieldNameIndex(QSWATTopology._LAKEWITHIN);
            var channelLakeMainIndex = this.getIndex(channelsLayer, QSWATTopology._LAKEMAIN);
            this.chLinkIntoLake = new dict();
            this.chLinkInsideLake = new dict();
            this.chLinkFromLake = new dict();
            var areaFactor = gv.horizontalFactor * gv.horizontalFactor;
            var lakeAttMap = new dict();
            foreach (var lake in lakesProvider.getFeatures()) {
                var lakeGeom = lake.geometry();
                var lakeRect = lakeGeom.boundingBox();
                var lakeId = Convert.ToInt32(lake[lakeIdIndex]);
                var lakeCentroid = lakeGeom.centroid().asPoint();
                var lakeArea = lakeGeom.area() * areaFactor;
                var lakeOverrideArea = lakeArea;
                if (lakeResIndex < 0) {
                    waterRole = QSWATTopology._RESTYPE;
                } else {
                    waterRole = lake[lakeResIndex];
                }
                if (lakeAreaIndex >= 0) {
                    try {
                        lakeOverrideArea = float(lake[lakeAreaIndex]) * 10000.0;
                    } catch {
                    }
                }
                var lakeData = LakeData(lakeArea, lakeOverrideArea, lakeCentroid, waterRole);
                // reservoirs, ponds and wetlands are removed from subbasins and LSUs maps; playas will make HRUs
                if (waterRole == QSWATTopology._PLAYATYPE) {
                    lakeData.elevation = QSWATTopology.valueAtPoint(lakeCentroid, demLayer);
                    this.lakesData[lakeId] = lakeData;
                    continue;
                }
                var totalElevation = 0.0;
                var elevPointCount = 0;
                // the area removed from channel basins that intersect wih the lake
                var chBasinWaterArea = 0.0;
                var attMap = new dict();
                var geomMap = new dict();
                var polysToRemove = new List<object>();
                var minArea = this.dx * this.dy;
                foreach (var sub in subsProvider.getFeatures()) {
                    var subGeom = sub.geometry();
                    if (QSWATTopology.intersectsPoly(subGeom, lakeGeom, lakeRect)) {
                        var subId = sub.id();
                        area1 = subGeom.area() * areaFactor;
                        newGeom = subGeom.difference(lakeGeom);
                        area2 = newGeom.area() * areaFactor;
                        if (area2 < area1) {
                            var subPoly = sub[subsPolyIndex];
                            QSWATUtils.loginfo("Lake {0} overlaps subbasin polygon {1}: area reduced from {2} to {3}".format(lakeId, subPoly, area1, area2));
                            if (area2 < minArea) {
                                polysToRemove.append(subId);
                                // remove subbasin from range of chBasinToSubbasin
                                var chBasinsToRemove = new HashSet<object>();
                                foreach (var (chBasin, subbasin) in this.chBasinToSubbasin.items()) {
                                    if (subbasin == subPoly) {
                                        chBasinsToRemove.add(chBasin);
                                    }
                                }
                                foreach (var chBasin in chBasinsToRemove) {
                                    this.chBasinToSubbasin.Remove(chBasin);
                                }
                                QSWATUtils.loginfo("Subbasin polygon {0} removed".format(subPoly));
                            } else {
                                geomMap[subId] = newGeom;
                                attMap[subId] = new Dictionary<object, object> {
                                    {
                                        subsAreaIndex,
                                        area2 / 10000.0}};
                            }
                        }
                    }
                }
                // change attributes and topology BEFORE removing the zero area ones, as removal changes ids of the others
                if (!subsProvider.changeAttributeValues(attMap)) {
                    QSWATUtils.error("Failed to update subbasins attributes in {0}".format(gv.subbasinsFile), this.isBatch);
                    foreach (var err in subsProvider.errors()) {
                        QSWATUtils.loginfo(err);
                    }
                    return false;
                }
                if (!subsProvider.changeGeometryValues(geomMap)) {
                    QSWATUtils.error("Failed to update subbasin geometries in {0}".format(gv.subbasinsFile), this.isBatch);
                    foreach (var err in subsProvider.errors()) {
                        QSWATUtils.loginfo(err);
                    }
                    return false;
                }
                // for some reason doing both changes at once fails   
                //             if not subsProvider.changeFeatures(attMap, geomMap): 
                //                 QSWATUtils.error(u'Failed to update {0}'.format(gv.subbasinsFile), self.isBatch)
                //                 for err in subsProvider.errors():
                //                     QSWATUtils.loginfo(err)
                //                 return 
                if (polysToRemove.Count > 0) {
                    if (!subsProvider.deleteFeatures(polysToRemove)) {
                        QSWATUtils.error("Failed to remove polygons from {0}".format(gv.subbasinsFile), this.isBatch);
                        foreach (var err in subsProvider.errors()) {
                            QSWATUtils.loginfo(err);
                        }
                    }
                }
                attMap = new dict();
                geomMap = new dict();
                // map of polygon id to area that is part of the lake
                var channelAreaChange = new dict();
                foreach (var chBasinFeature in chBasinsProvider.getFeatures()) {
                    var chBasinGeom = chBasinFeature.geometry();
                    var polyId = chBasinFeature[chBasinsPolyIndex];
                    // if area reduced to zero because inside another lake, geometry is None
                    if (chBasinGeom is not null && QSWATTopology.intersectsPoly(chBasinGeom, lakeGeom, lakeRect)) {
                        var chBasinId = chBasinFeature.id();
                        area1 = chBasinGeom.area() * areaFactor;
                        newGeom = chBasinGeom.difference(lakeGeom);
                        area2 = newGeom.area() * areaFactor;
                        if (area2 < area1) {
                            QSWATUtils.loginfo("Lake {0} overlaps channel basin {1}: area reduced from {2} to {3}".format(lakeId, polyId, area1, area2));
                            chBasinWaterArea += area1 - area2;
                            geomMap[chBasinId] = newGeom;
                            attMap[chBasinId] = new Dictionary<object, object> {
                                {
                                    chBasinsAreaIndex,
                                    area2 / 10000.0}};
                            channelAreaChange[polyId] = area1 - area2;
                            if (area2 < minArea) {
                                // channel basin disappears into lake: remove its mapping to subbasin
                                // may already have been removed because all subbasin in lake
                                subbasin = this.chBasinToSubbasin.get(polyId, -1);
                                if (subbasin >= 0) {
                                    this.chBasinToSubbasin.Remove(polyId);
                                    QSWATUtils.loginfo("Channel basin {0} (subbasin {1}) removed".format(polyId, subbasin));
                                }
                            }
                        }
                    }
                }
                if (!chBasinsProvider.changeAttributeValues(attMap)) {
                    QSWATUtils.error("Failed to update channel basin attributes in {0}".format(gv.wshedFile), this.isBatch);
                    foreach (var err in chBasinsProvider.errors()) {
                        QSWATUtils.loginfo(err);
                    }
                    return false;
                }
                if (!chBasinsProvider.changeGeometryValues(geomMap)) {
                    QSWATUtils.error("Failed to update channel basin geometries in {0}".format(gv.wshedFile), this.isBatch);
                    foreach (var err in chBasinsProvider.errors()) {
                        QSWATUtils.loginfo(err);
                    }
                    return false;
                }
                attMap = new dict();
                var currentDrainArea = 0;
                // first pass through channels: collect inflowing and outflowing channels from DsNodes in lakeInlets and lakeOutlets
                foreach (var channel in channelsProvider.getFeatures()) {
                    link = channel[channelLinkIndex];
                    var dsLink = channel[channelDsLinkIndex];
                    var dsNode = channel[channelDsNodeIndex];
                    if (dsNode > 0) {
                        if (this.lakeInlets[lakeId].Contains(dsNode)) {
                            var inflowData = this.getReachData(channel, demLayer);
                            Debug.Assert(inflowData is not null);
                            elev = inflowData.lowerZ;
                            lakeData.inChLinks[link] = (dsNode, QgsPointXY(inflowData.lowerX, inflowData.lowerY), elev);
                            if (dsLink >= 0) {
                                lakeData.lakeChLinks.add(dsLink);
                                this.chLinkInsideLake[dsLink] = lakeId;
                            }
                            this.chLinkIntoLake[link] = lakeId;
                            if (!math.isclose(elev, gv.elevationNoData, rel_tol: 1E-06)) {
                                // type: ignore
                                totalElevation += elev;
                                elevPointCount += 1;
                            }
                            channelId = channel.id();
                            var wsno = channel[channelWSNOIndex];
                            var areaChange = channelAreaChange.get(wsno, 0);
                            var drainArea = channel[channelDrainAreaIndex] * areaFactor - areaChange;
                            attMap[channelId] = new Dictionary<object, object> {
                                {
                                    channelDrainAreaIndex,
                                    drainArea}};
                        } else if (this.lakeOutlets[lakeId].Contains(dsNode)) {
                            var outflowData = this.getReachData(channel, demLayer);
                            Debug.Assert(outflowData is not null);
                            var outlet = QgsPointXY(outflowData.lowerX, outflowData.lowerY);
                            var replace = true;
                            if (dsLink >= 0) {
                                if (lakeData.outPoint[2] is not null) {
                                    // choose point with larger drain area 
                                    var newDrainArea = channel[channelDrainAreaIndex];
                                    if (newDrainArea > currentDrainArea) {
                                        currentDrainArea = newDrainArea;
                                        if (lakeData.outChLink >= 0) {
                                            lakeData.otherOutChLinks.add(lakeData.outChLink);
                                        }
                                    } else {
                                        replace = false;
                                    }
                                }
                                if (replace) {
                                    // don't use WSNO as route to subbasin as channel basin may have been deleted
                                    // wsno = channel[channelWSNOIndex]
                                    // subbasin = self.chBasinToSubbasin[wsno]
                                    subbasin = channel[channelBasinIndex];
                                    lakeData.outPoint = (subbasin, dsNode, outlet, outflowData.lowerZ);
                                    lakeData.outChLink = dsLink;
                                } else {
                                    lakeData.otherOutChLinks.add(dsLink);
                                }
                                this.chLinkFromLake[dsLink] = lakeId;
                            }
                            lakeData.lakeChLinks.add(link);
                            this.chLinkInsideLake[link] = lakeId;
                        }
                    }
                }
                // check to see if a watershed outlet was marked inside the lake
                // and if so try to move it to the lake perimeter.  Else leave it as an internal outlet. 
                // we don't need to exclude outlets created to split channels flowing into and out of lake
                // because the outlets map is made from the streams before lake inlets and outlets are added to the snap file
                // and the augmented snapfile is only used to make channels
                foreach (var (subbasin, (pointId, pt, chs)) in this.outlets.items()) {
                    if (QSWATTopology.polyContains(pt, lakeGeom, lakeRect) && this.isWatershedOutlet(pointId, channelsProvider)) {
                        if (!os.path.exists(gv.pFile)) {
                            QSWATUtils.error("Cannot find D8 flow directions file {0}".format(gv.pFile), this.isBatch);
                            break;
                        }
                        // need to give different id to outPoint, since this is used to make the reservoir point
                        // which will then route to the subbasin outlet
                        // can use outlet point id if already created
                        if (lakeData.outPoint[1] >= 0) {
                            newPointId = lakeData.outPoint[1];
                        } else {
                            this.pointId += 1;
                            newPointId = this.pointId;
                        }
                        elev = QSWATTopology.valueAtPoint(pt, demLayer);
                        if (elev is null) {
                            elev = 0;
                        }
                        lakeData.outPoint = (subbasin, newPointId, pt, elev);
                        // maximum number of steps approximates to the threshold for snapping points expressed as number of DEM cells
                        var maxSteps = this.dx == 0 ? 5 : Convert.ToInt32(snapThreshold / this.dx + 0.5);
                        (lakeOutlet, found) = QSWATTopology.movePointToPerimeter(pt, lakeGeom, gv.pFile, maxSteps);
                        if (found) {
                            if (lakeData.outPoint[2] is not null) {
                                QSWATUtils.information("User marked outlet {0} chosen as main outlet for lake {1}".format(pointId, lakeId), gv.isBatch, reportErrors: reportErrors);
                                if (lakeData.outChLink >= 0) {
                                    lakeData.otherOutChLinks.add(lakeData.outChLink);
                                }
                            }
                            elev = QSWATTopology.valueAtPoint(lakeOutlet, demLayer);
                            if (elev is null) {
                                elev = 0;
                            }
                            lakeData.outPoint = (subbasin, newPointId, lakeOutlet, elev);
                            QSWATUtils.loginfo("Outlet of lake {0} set to ({1}, {2})".format(lakeId, Convert.ToInt32(lakeOutlet.x()), Convert.ToInt32(lakeOutlet.y())));
                            // update outlets map 
                            this.outlets[subbasin] = (pointId, lakeOutlet, chs);
                        } else {
                            QSWATUtils.loginfo("Outlet of lake {0} set to internal point ({1}, {2})".format(lakeId, Convert.ToInt32(lakeOutlet.x()), Convert.ToInt32(lakeOutlet.y())));
                        }
                        lakeData.outChLink = -1;
                        break;
                    }
                }
                // second pass through channels: collect channels within lake: i.e. both ends in lake
                // and set LakeIn, LakeOut, LakeWithin fields
                foreach (var channel in channelsProvider.getFeatures()) {
                    link = channel[channelLinkIndex];
                    channelId = channel.id();
                    object channelData = null;
                    var lakeIn = this.chLinkIntoLake.get(link, 0);
                    var lakeOut = this.chLinkFromLake.get(link, 0);
                    var lakeWithin = this.chLinkInsideLake.get(link, 0);
                    if (!this.chLinkIntoLake.Contains(link) && !this.chLinkFromLake.Contains(link) && !this.chLinkInsideLake.Contains(link)) {
                        channelData = this.getReachData(channel, null);
                        Debug.Assert(channelData is not null);
                        var pt1 = QgsPointXY(channelData.lowerX, channelData.lowerY);
                        var pt2 = QgsPointXY(channelData.upperX, channelData.upperY);
                        if (QSWATTopology.polyContains(pt1, lakeGeom, lakeRect) && QSWATTopology.polyContains(pt2, lakeGeom, lakeRect)) {
                            lakeData.lakeChLinks.add(link);
                            this.chLinkInsideLake[link] = lakeId;
                            lakeWithin = lakeId;
                        }
                    }
                    lakeAttMap[channelId] = new Dictionary<object, object> {
                        {
                            channelLakeInIndex,
                            lakeIn},
                        {
                            channelLakeOutIndex,
                            lakeOut},
                        {
                            channelLakeWithinIndex,
                            lakeWithin}};
                    if (lakeData.lakeChLinks.Contains(link)) {
                        // remove the channel's point source
                        // other errors may cause the link to not exist and hence not to have a point source
                        if (this.chPointSources.Contains(link)) {
                            this.chPointSources.Remove(link);
                        }
                    }
                }
                // if the lake has an outlet channel with a drain area less than LAKEOUTLETCHANNELAREA percent of the lake area
                // and dsNode negative (to avoid pulling a point inside the lake) make its channel internal
                var outLinkId = -1;
                var outLink = lakeData.outChLink;
                var outBasin = -1;
                var dsOutLink = -1;
                var dsNodeOutLink = -1;
                if (outLink >= 0) {
                    exp = QgsExpression("\"{0}\" = {1}".format(QSWATTopology._LINKNO, outLink));
                    request = QgsFeatureRequest(exp);
                    channel = channelsProvider.getFeatures(request).ToList()[0];
                    outLinkId = channel.id();
                    outBasin = channel[channelWSNOIndex];
                    dsOutLink = channel[channelDsLinkIndex];
                    dsNodeOutLink = channel[channelDsNodeIndex];
                }
                if (dsNodeOutLink < 0 && outBasin >= 0) {
                    // threshold in ha: LAKEOUTLETCHANNELAREA of lake area
                    var threshold = lakeData.area / 1000000.0 * Parameters._LAKEOUTLETCHANNELAREA;
                    request = QgsFeatureRequest().setFlags(QgsFeatureRequest.NoGeometry).setSubsetOfAttributes(new List<object> {
                        chBasinsPolyIndex,
                        chBasinsAreaIndex
                    });
                    foreach (var chBasinFeature in chBasinsProvider.getFeatures(request)) {
                        if (chBasinFeature[chBasinsPolyIndex] == outBasin) {
                            var areaHa = chBasinFeature[chBasinsAreaIndex];
                            if (areaHa < threshold) {
                                // move outlet channel inside lake
                                lakeData.lakeChLinks.add(outLink);
                                lakeData.outChLink = dsOutLink;
                                this.chLinkFromLake.Remove(outLink);
                                this.chLinkInsideLake[outLink] = lakeId;
                                // mark it as within as well as being the outlet (already set)
                                lakeAttMap[outLinkId][channelLakeWithinIndex] = lakeId;
                                // check if this point now inside the lake is a subbasin outlet
                                subbasin = this.chBasinToSubbasin[outBasin];
                                (_, _, outChannels) = this.outlets[subbasin];
                                if (outChannels.Contains(outLink)) {
                                    // subbasin outlet has moved inside the lake
                                    this.outletsInLake[subbasin] = lakeId;
                                }
                                QSWATUtils.loginfo("Channel link {0} channel basin {1} moved inside lake {2}".format(outLink, outBasin, lakeId));
                                // remove the channel's point source
                                this.chPointSources.Remove(outLink);
                                if (dsOutLink >= 0) {
                                    this.chLinkFromLake[dsOutLink] = lakeId;
                                }
                                break;
                            }
                        }
                    }
                }
                if (lakeData.outPoint[2] is null) {
                    QSWATUtils.error("Failed to find outlet for lake {0}".format(lakeId), this.isBatch);
                    errorCount += 1;
                }
                if (lakeData.outChLink >= 0) {
                    var chId = -1;
                    // find the channel's id
                    exp = QgsExpression("\"{0}\" = {1}".format(QSWATTopology._LINKNO, lakeData.outChLink));
                    request = QgsFeatureRequest(exp);
                    channel = channelsProvider.getFeatures(request).ToList()[0];
                    chId = channel.id();
                    lakeAttMap[chId][channelLakeMainIndex] = lakeId;
                }
                var meanElevation = elevPointCount > 0 ? totalElevation / elevPointCount : lakeData.outPoint[3];
                lakeData.elevation = meanElevation;
                QSWATUtils.loginfo("Lake {0} has outlet on channel {1}, other outlets on channels {2}, inlets on channels {3} and contains channels {4}".format(lakeId, lakeData.outChLink, lakeData.otherOutChLinks, lakeData.inChLinks.keys().ToList(), lakeData.lakeChLinks));
                var OK = channelsProvider.changeAttributeValues(attMap);
                OK = OK && channelsProvider.changeAttributeValues(lakeAttMap);
                if (!OK) {
                    QSWATUtils.error("Failed to update channel attributes in {0}".format(gv.channelFile), this.isBatch);
                    foreach (var err in channelsProvider.errors()) {
                        QSWATUtils.loginfo(err);
                    }
                    return false;
                }
                this.lakesData[lakeId] = lakeData;
                lakeArea = lakeData.area;
                var percentChBasinWater = chBasinWaterArea / lakeArea * 100;
                QSWATUtils.loginfo("Lake {0} has area {1} and channel basin water area {2}: {3}%".format(lakeId, lakeArea, chBasinWaterArea, percentChBasinWater));
                var intPercent = Convert.ToInt32(percentChBasinWater + 0.5);
                if (percentChBasinWater < 99 || percentChBasinWater > 101) {
                    QSWATUtils.information(@"WARNING: Only {0}% of the area of lake {1} is accounted for in your watershed.
                You should carefully check the messages concerning this lake in the QSWAT+ log in the QGIS log messages panel.".format(intPercent, lakeId), this.isBatch, reportErrors: reportErrors);
                }
            }
            if (this.lakesData.Count == 0) {
                QSWATUtils.error("No lakes found in {0}".format(QSWATUtils.layerFilename(lakesLayer)), this.isBatch);
                return false;
            }
            chBasinsLayer.triggerRepaint();
            streamsLayer.triggerRepaint();
            channelsLayer.triggerRepaint();
            return errorCount == 0;
        }
        
        // Add LakeIn etc fields to channelsLayer if necessary.
        public virtual object addLakeFieldsToChannels(object channelsLayer) {
            var channelsProvider = channelsLayer.dataProvider();
            var channelLakeInIndex = channelsProvider.fieldNameIndex(QSWATTopology._LAKEIN);
            var channelLakeOutIndex = channelsProvider.fieldNameIndex(QSWATTopology._LAKEOUT);
            var channelLakeWithinIndex = channelsProvider.fieldNameIndex(QSWATTopology._LAKEWITHIN);
            var channelLakeMainIndex = channelsProvider.fieldNameIndex(QSWATTopology._LAKEMAIN);
            var fields = new List<object>();
            if (channelLakeInIndex < 0) {
                fields.append(QgsField(QSWATTopology._LAKEIN, QVariant.Int));
            }
            if (channelLakeOutIndex < 0) {
                fields.append(QgsField(QSWATTopology._LAKEOUT, QVariant.Int));
            }
            if (channelLakeWithinIndex < 0) {
                fields.append(QgsField(QSWATTopology._LAKEWITHIN, QVariant.Int));
            }
            if (channelLakeMainIndex < 0) {
                fields.append(QgsField(QSWATTopology._LAKEMAIN, QVariant.Int));
            }
            if (fields.Count > 0) {
                if (!channelsProvider.addAttributes(fields)) {
                    QSWATUtils.error("Cannot add lake fields to channels shapefile", this.isBatch);
                    return;
                }
                channelsLayer.updateFields();
            }
        }
        
        // Return true if there is a channel with dsNode equal to pointId and with dsLink -1.
        public virtual bool isWatershedOutlet(int pointId, object channelsProvider) {
            var DSLINKNO = this.isHUC ? QSWATTopology._DSLINKNO1 : QSWATTopology._DSLINKNO;
            var channelDsLinkIndex = channelsProvider.fieldNameIndex(DSLINKNO);
            var DSNODEID = this.isHUC ? QSWATTopology._DSNODEID1 : QSWATTopology._DSNODEID;
            var exp = QgsExpression("\"{0}\" = {1}".format(DSNODEID, pointId));
            var request = QgsFeatureRequest(exp).setFlags(QgsFeatureRequest.NoGeometry).setSubsetOfAttributes(new List<object> {
                channelDsLinkIndex
            });
            foreach (var link in channelsProvider.getFeatures(request)) {
                if (link[channelDsLinkIndex] == -1) {
                    return true;
                }
            }
            return false;
            //     def isOutlet(self, pointId: int, outletsLayer: QgsVectorLayer) -> bool:
            //         """Return true if outletsLayer contains an outlet point with id pointId."""
            //         idIndex = self.getIndex(outletsLayer, QSWATTopology._ID, ignoreMissing=True)
            //         inletIndex = self.getIndex(outletsLayer, QSWATTopology._INLET, ignoreMissing=True)
            //         resIndex = self.getIndex(outletsLayer, QSWATTopology._RES, ignoreMissing=True)
            //         if idIndex < 0 or inletIndex < 0 or resIndex < 0:
            //             return False
            //         exp = QgsExpression('"{0}" = {1}'.format(QSWATTopology._ID, pointId))
            //         request = QgsFeatureRequest(exp).setFlags(QgsFeatureRequest.NoGeometry).setSubsetOfAttributes([inletIndex, resIndex])
            //         for point in outletsLayer.dataProvider().getFeatures(request):
            //             if point[inletIndex] == 0 and point[resIndex] == 0:
            //                 return True
            //         return False
        }
        
        // Add reservoir, pond and wetland lakes when using grid model.  Return number of lakes (which may be zero) or -1 if error.
        public virtual int addGridReservoirsPondsAndWetlands(
            object gridLayer,
            object channelsLayer,
            object demLayer,
            object gv,
            bool reportErrors = true) {
            object drainage;
            var gridProvider = gridLayer.dataProvider();
            var gridPolyIndex = gridProvider.fieldNameIndex(QSWATTopology._POLYGONID);
            var gridDownIndex = gridProvider.fieldNameIndex(QSWATTopology._DOWNID);
            var gridAreaIndex = gridProvider.fieldNameIndex(Parameters._AREA);
            var gridLakeIdIndex = gridProvider.fieldNameIndex(QSWATTopology._LAKEID);
            if (gridLakeIdIndex < 0) {
                // can be no lakes
                return 0;
            }
            var gridResIndex = gridProvider.fieldNameIndex(QSWATTopology._RES);
            var channelsProvider = channelsLayer.dataProvider();
            var channelLinkIndex = channelsProvider.fieldNameIndex(QSWATTopology._LINKNO);
            var channelDsLinkIndex = channelsProvider.fieldNameIndex(QSWATTopology._DSLINKNO);
            var channelWSNOIndex = channelsProvider.fieldNameIndex(QSWATTopology._WSNO);
            // the drainage field may not exist if we are using grid or table drainage: deal with this later
            var streamDrainageIndex = channelsProvider.fieldNameIndex(QSWATTopology._DRAINAGE);
            var polysIntoLake = new dict();
            var polysInsidelake = new dict();
            var polysFromLake = new dict();
            this.chLinkIntoLake = new dict();
            this.chLinkInsideLake = new dict();
            this.chLinkFromLake = new dict();
            var request = QgsFeatureRequest().setFlags(QgsFeatureRequest.NoGeometry).setSubsetOfAttributes(new List<object> {
                gridPolyIndex,
                gridLakeIdIndex
            });
            // first make map poly -> lake id
            var polyToLake = new dict();
            foreach (var cell in gridProvider.getFeatures(request)) {
                try {
                    var lakeId = Convert.ToInt32(cell[gridLakeIdIndex]);
                    polyToLake[cell[gridPolyIndex]] = lakeId;
                    // make sure waterbody id is set to maximum lake id in case using existing grid
                    this.waterBodyId = max(this.waterBodyId, lakeId);
                } catch {
                    // lakeId was presumably null
                    // QSWATUtils.loginfo('At polygonId {0} lakeId has type {1} and value {2}'.format(cell[gridPolyIndex], type(cell[gridLakeIdIndex]), cell[gridLakeIdIndex]))
                }
            }
            if (polyToLake.Count == 0) {
                // no lakes
                return 0;
            }
            // data for calculating centroid
            // map of lake id to (area, x moment of area, y moment)
            var lakeAreaData = new dict();
            foreach (var cell in gridProvider.getFeatures()) {
                var waterRole = cell[gridResIndex];
                var poly = cell[gridPolyIndex];
                var downPoly = cell[gridDownIndex];
                var sourceLake = polyToLake.get(poly, null);
                var targetLake = polyToLake.get(downPoly, null);
                if (sourceLake is not null) {
                    if (!lakeAreaData.Contains(sourceLake)) {
                        lakeAreaData[sourceLake] = (waterRole, 0, 0, 0);
                    }
                    var area = cell[gridAreaIndex] * 10000.0;
                    (centre, _, _) = QSWATUtils.centreGridCell(cell);
                    (_, totalArea, xMoment, yMoment) = lakeAreaData[sourceLake];
                    lakeAreaData[sourceLake] = (waterRole, totalArea + area, xMoment + area * centre.x(), yMoment + area * centre.y());
                    if (targetLake == sourceLake) {
                        // channel links two lake cells within lake
                        polysInsidelake[poly] = sourceLake;
                    } else {
                        // exit channel
                        polysFromLake[poly] = sourceLake;
                        if (targetLake is not null) {
                            // also entry channel
                            polysIntoLake[poly] = targetLake;
                        }
                    }
                } else if (targetLake is not null) {
                    polysIntoLake[poly] = targetLake;
                }
            }
            var totalElevation = new dict();
            // map of lake id to possible exit channels
            // will choose one with largest drainage
            var exitData = new dict();
            foreach (var (lakeId, (waterRole, area, xMoment, yMoment)) in lakeAreaData.items()) {
                var centroid = QgsPointXY(float(xMoment) / area, float(yMoment) / area);
                this.lakesData[lakeId] = LakeData(area, area, centroid, waterRole);
                totalElevation[lakeId] = 0;
                exitData[lakeId] = new dict();
            }
            // convert wsnos to links and complete LakesData
            // get maximum chLink and create downChannels map and link to basin map in case drainage needs calculating
            this.downChannels = new dict();
            this.chLinkToChBasin = new dict();
            var maxChLink = 0;
            foreach (var channel in channelsProvider.getFeatures()) {
                var chLink = channel[channelLinkIndex];
                maxChLink = max(maxChLink, chLink);
                var dsChLink = channel[channelDsLinkIndex];
                this.downChannels[chLink] = dsChLink;
                var wsno = channel[channelWSNOIndex];
                this.chLinkToChBasin[chLink] = wsno;
                var lakeIdInto = polysIntoLake.get(wsno, 0);
                if (lakeIdInto > 0) {
                    this.chLinkIntoLake[chLink] = lakeIdInto;
                    // since this is a grid model the grid cells form different subbasins and there will be a suitable outlet
                    // point already stored in the outlets map
                    (pointId, point, _) = this.outlets[wsno];
                    var elev = QSWATTopology.valueAtPoint(point, demLayer);
                    if (elev is null) {
                        elev = 0;
                    }
                    this.lakesData[lakeIdInto].inChLinks[chLink] = (pointId, point, elev);
                    totalElevation[lakeIdInto] += elev;
                    // if different lake cells abut, channel can be into one and from the other 
                    // continue
                }
                var lakeIdFrom = polysFromLake.get(wsno, 0);
                if (lakeIdFrom > 0) {
                    // allow for no drainage field
                    drainage = streamDrainageIndex < 0 ? -1 : channel[streamDrainageIndex];
                    var data = this.getReachData(channel, demLayer);
                    if (data is null) {
                        QSWATUtils.loginfo("No reach data for channel link {0}".format(chLink));
                    } else {
                        exitData[lakeIdFrom][chLink] = (wsno, drainage, QgsPointXY(data.upperX, data.upperY), data.upperZ);
                    }
                    continue;
                }
                var lakeIdInside = polysInsidelake.get(wsno, 0);
                if (lakeIdInside > 0) {
                    this.chLinkInsideLake[chLink] = lakeIdInside;
                    this.lakesData[lakeIdInside].lakeChLinks.add(chLink);
                    continue;
                }
            }
            // check if we need to calculate drainage: no drainage field and more than one exit for at least one lake
            var needDrainage = false;
            if (streamDrainageIndex < 0) {
                foreach (var data in exitData.values()) {
                    if (data.Count > 1) {
                        needDrainage = true;
                        break;
                    }
                }
            }
            if (needDrainage) {
                this.drainAreas = zeros(maxChLink + 1, dtype: float);
                var gridCellArea = this.dx * this.dy * gv.gridSize * gv.gridSize;
                // us parameter is set to None here because it is not yet calculated
                // and if there are any circularities they will be removed from the channels shapefile
                // and us will be calculated from that shapefile.
                this.setGridDrainageAreas(channelsLayer, gridLayer, maxChLink, null, gridCellArea);
            }
            // count lakes with multiple outlets 
            var multipleOutletCount = 0;
            foreach (var data in exitData.values()) {
                if (data.Count > 1) {
                    multipleOutletCount += 1;
                }
            }
            var multipleOutletsInteractive = multipleOutletCount <= 5;
            if (!multipleOutletsInteractive) {
                QSWATUtils.information("For information on {0} lakes with multiple possible outlets see the QSWAT+ log".format(multipleOutletCount), gv.isBatch, reportErrors: reportErrors);
            }
            // find outlet with largest drainage and mark as THE outlet
            foreach (var (lakeId, data) in exitData.items()) {
                // set maxDrainage less than -1 value used for missing drainage so that first exit link registers
                // as if there is only one exit for each lake needDrainage will be false
                var maxDrainage = -2.0;
                var exLink = -1;
                var exWsno = -1;
                object exPoint = null;
                var exElev = 0.0;
                foreach (var (chLink, (wsno, drainage, pt, elev)) in data.items()) {
                    if (needDrainage) {
                        drainage = float(this.drainAreas[chLink]);
                    }
                    if (drainage > maxDrainage) {
                        maxDrainage = drainage;
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
                        QSWATUtils.error("Failed to make outlet for lake {0}".format(lakeId), gv.isBatch, reportErrors: reportErrors);
                        continue;
                    } else {
                        // remove chLink from internal links
                        this.lakesData[lakeId].lakeChLinks.remove(exLink);
                        // remove from map of internal channels
                        this.chLinkInsideLake.Remove(exLink);
                        // add as an exit
                        // first find the channel
                        var exp = QgsExpression("\"{0}\" = {1}".format(QSWATTopology._LINKNO, exLink));
                        var channel = channelsProvider.getFeatures(QgsFeatureRequest(exp)).ToList()[0];
                        var reachData = this.getReachData(channel, demLayer);
                        if (reachData is null) {
                            QSWATUtils.loginfo("No reach data for channel link {0}".format(exLink));
                        } else {
                            exWsno = channel[channelWSNOIndex];
                            drainage = streamDrainageIndex < 0 ? float(this.drainAreas[exLink]) : float(channel[streamDrainageIndex]);
                            exPoint = QgsPointXY(reachData.upperX, reachData.upperY);
                            exElev = reachData.upperZ;
                            if (math.isclose(exElev, gv.elevationNoData, rel_tol: 1E-06)) {
                                // type: ignore
                                exElev = 0;
                            }
                            data[exLink] = (exWsno, drainage, exPoint, exElev);
                        }
                    }
                }
                var others = data.keys().ToList();
                others.remove(exLink);
                if (others != new List<object>()) {
                    var msg = @"Warning: Stream link {0} chosen as main outlet for all of lake {1}.  
                        Other possible outlet stream links are {2}.".format(exLink, lakeId, (from link in others
                        select Convert.ToInt32(link)).ToList().ToString());
                    if (multipleOutletsInteractive) {
                        QSWATUtils.information(msg, gv.isBatch, reportErrors: reportErrors);
                    } else {
                        QSWATUtils.loginfo(msg);
                    }
                }
                this.chLinkFromLake[exLink] = lakeId;
                this.lakesData[lakeId].outChLink = exLink;
                foreach (var chLink in others) {
                    this.chLinkFromLake[chLink] = lakeId;
                    this.lakesData[lakeId].otherOutChLinks.add(chLink);
                }
                this.pointId += 1;
                this.lakesData[lakeId].outPoint = (exWsno, this.pointId, exPoint, exElev);
            }
            foreach (var (lakeId, totalElev) in totalElevation.items()) {
                var numInLinks = this.lakesData[lakeId].inChLinks.Count;
                if (numInLinks > 0) {
                    this.lakesData[lakeId].elevation = float(totalElev) / numInLinks;
                } else {
                    this.lakesData[lakeId].elevation = this.lakesData[lakeId].outPoint[3];
                }
            }
            return this.lakesData.Count;
        }
        
        // Add playas to grid model.
        public virtual object addGridPlayas(object lakesLayer, object demLayer, object gv) {
            var lakeIdIndex = this.getIndex(lakesLayer, QSWATTopology._LAKEID);
            var lakeResIndex = this.getIndex(lakesLayer, QSWATTopology._RES);
            if (lakeResIndex < 0) {
                // cannot be anything but reservoirs, so nothing to do
                return;
            }
            var lakeAreaIndex = this.getIndex(lakesLayer, Parameters._AREA, ignoreMissing: true);
            var areaFactor = gv.horizontalFactor * gv.horizontalFactor;
            foreach (var lake in lakesLayer.getFeatures()) {
                var waterRole = lake[lakeResIndex];
                if (waterRole != QSWATTopology._PLAYATYPE) {
                    continue;
                }
                var lakeGeom = lake.geometry();
                var lakeId = Convert.ToInt32(lake[lakeIdIndex]);
                var lakeCentroid = lakeGeom.centroid().asPoint();
                var lakeArea = lakeGeom.area() * areaFactor;
                var lakeOverrideArea = lakeArea;
                if (lakeAreaIndex >= 0) {
                    try {
                        lakeOverrideArea = float(lake[lakeAreaIndex]) * 10000.0;
                    } catch {
                    }
                }
                var lakeData = LakeData(lakeArea, lakeOverrideArea, lakeCentroid, waterRole);
                lakeData.elevation = QSWATTopology.valueAtPoint(lakeCentroid, demLayer);
                this.lakesData[lakeId] = lakeData;
            }
        }
        
        // Add lakes data to existing non-grid model.
        //         
        //         We ignore DsNodeIds for inflowing and outflowing channels since these were
        //         probably only added previously to the snapped inlets/outlets file
        //         and inlets/outlets are little use in any case with existing watersheds.
        public virtual bool addExistingLakes(
            object lakesLayer,
            object channelsLayer,
            object demLayer,
            object gv,
            bool reportErrors = true) {
            object subbasin;
            object point;
            object elev;
            object data;
            object waterRole;
            var lakeIdIndex = this.getIndex(lakesLayer, QSWATTopology._LAKEID);
            var lakeResIndex = this.getIndex(lakesLayer, QSWATTopology._RES, ignoreMissing: true);
            if (lakeResIndex < 0) {
                QSWATUtils.information("No RES field in lakes shapefile {0}: assuming lakes are reservoirs".format(QSWATUtils.layerFilename(lakesLayer)), this.isBatch);
            }
            var lakeAreaIndex = this.getIndex(lakesLayer, Parameters._AREA, ignoreMissing: true);
            var channelLinkIndex = this.getIndex(channelsLayer, QSWATTopology._LINKNO);
            var DSLINKNO = this.isHUC ? QSWATTopology._DSLINKNO1 : QSWATTopology._DSLINKNO;
            var channelDsLinkIndex = this.getIndex(channelsLayer, DSLINKNO);
            var channelBasinIndex = this.getIndex(channelsLayer, QSWATTopology._BASINNO);
            var channelLakeInIndex = this.getIndex(channelsLayer, QSWATTopology._LAKEIN);
            var channelLakeOutIndex = this.getIndex(channelsLayer, QSWATTopology._LAKEOUT);
            var channelLakeWithinIndex = this.getIndex(channelsLayer, QSWATTopology._LAKEWITHIN);
            var channelLakeMainIndex = this.getIndex(channelsLayer, QSWATTopology._LAKEMAIN);
            if (lakeIdIndex < 0 || channelLinkIndex < 0 || channelDsLinkIndex < 0 || channelBasinIndex < 0 || channelLakeInIndex < 0 || channelLakeOutIndex < 0 || channelLakeWithinIndex < 0 || channelLakeMainIndex < 0) {
                return false;
            }
            this.lakesData = new dict();
            var areaFactor = gv.horizontalFactor * gv.horizontalFactor;
            foreach (var lake in lakesLayer.getFeatures()) {
                var lakeId = Convert.ToInt32(lake[lakeIdIndex]);
                if (lakeResIndex < 0) {
                    waterRole = QSWATTopology._RESTYPE;
                } else {
                    waterRole = Convert.ToInt32(lake[lakeResIndex]);
                }
                if (this.lakesData.Contains(lakeId)) {
                    QSWATUtils.error("Lake identifier {0} occurs twice in {1}.  Lakes not added.".format(lakeId, QSWATUtils.layerFilename(lakesLayer)), gv.isBatch, reportErrors: reportErrors);
                    this.lakesData = new dict();
                    return false;
                }
                // to stop reuse of the same water body id
                this.waterBodyId = max(this.waterBodyId, lakeId);
                var geom = lake.geometry();
                var area = geom.area() * areaFactor;
                var centroid = geom.centroid().asPoint();
                var overrideArea = area;
                if (lakeAreaIndex >= 0) {
                    try {
                        overrideArea = float(lake[lakeAreaIndex]) * 10000.0;
                    } catch {
                    }
                }
                this.lakesData[lakeId] = LakeData(area, overrideArea, centroid, waterRole);
            }
            this.chLinkIntoLake = new dict();
            this.chLinkInsideLake = new dict();
            this.chLinkFromLake = new dict();
            foreach (var channel in channelsLayer.getFeatures()) {
                var chLink = channel[channelLinkIndex];
                var dsLink = channel[channelDsLinkIndex];
                var lakeIn = channel[channelLakeInIndex];
                var lakeOut = channel[channelLakeOutIndex];
                var lakeWithin = channel[channelLakeWithinIndex];
                var lakeMain = channel[channelLakeMainIndex];
                object reachData = null;
                if (lakeIn > 0) {
                    data = this.lakesData.get(lakeIn, null);
                    if (data is null) {
                        QSWATUtils.error("Channel with LINKNO {0} flows into lake {1} not defined in {2}.  Lakes not added.".format(chLink, lakeIn, QSWATUtils.layerFilename(lakesLayer)), gv.isBatch, reportErrors: reportErrors);
                        this.lakesData = new dict();
                        return false;
                    }
                    if (data.waterRole == QSWATTopology._PLAYATYPE) {
                        QSWATUtils.information("Channel with LINKNO {0} flows into {1} lake {2}.  This is ignored.".format(chLink, QSWATTopology.lakeCategory(data.waterRole), lakeIn), gv.isBatch, reportErrors: reportErrors);
                    } else {
                        reachData = this.getReachData(channel, demLayer);
                        Debug.Assert(reachData is not null);
                        elev = reachData.lowerZ;
                        data.elevation += elev;
                        // may already have point for channel outlet
                        (pointId, point) = this.chOutlets.get(chLink, (null, null));
                        if (point is null) {
                            // need to add a point
                            point = QgsPointXY(reachData.lowerX, reachData.lowerY);
                            this.pointId += 1;
                            var pointId = this.pointId;
                        }
                        data.inChLinks[chLink] = (pointId, point, elev);
                        this.chLinkIntoLake[chLink] = lakeIn;
                    }
                } else if (lakeWithin > 0) {
                    data = this.lakesData.get(lakeWithin, null);
                    if (data is null) {
                        QSWATUtils.error("Channel with LINKNO {0} inside lake {1} not defined in {2}.  Lakes not added.".format(chLink, lakeWithin, QSWATUtils.layerFilename(lakesLayer)), gv.isBatch, reportErrors: reportErrors);
                        this.lakesData = new dict();
                        return false;
                    }
                    if (data.waterRole == QSWATTopology._PLAYATYPE) {
                        QSWATUtils.information("Channel with LINKNO {0} inside {1} lake {2}.  This is ignored.".format(chLink, QSWATTopology.lakeCategory(data.waterRole), lakeIn), gv.isBatch, reportErrors: reportErrors);
                    } else {
                        data.lakeChLinks.add(chLink);
                        this.chLinkInsideLake[chLink] = lakeWithin;
                        if (dsLink < 0) {
                            // watershed outlet
                            reachData = this.getReachData(channel, demLayer);
                            Debug.Assert(reachData is not null);
                            subbasin = channel[channelBasinIndex];
                            data.outChLink = -1;
                            point = QgsPointXY(reachData.lowerX, reachData.lowerY);
                            elev = reachData.lowerZ;
                            this.pointId += 1;
                            data.outPoint = (subbasin, this.pointId, point, elev);
                            this.outletsInLake[subbasin] = lakeWithin;
                        }
                    }
                }
                if (lakeOut > 0) {
                    data = this.lakesData.get(lakeOut, null);
                    if (data is null) {
                        QSWATUtils.error("Channel with LINKNO {0} flows out of lake {1} not defined in {2}.  Lakes not added.".format(chLink, lakeOut, QSWATUtils.layerFilename(lakesLayer)), gv.isBatch, reportErrors: reportErrors);
                        this.lakesData = new dict();
                        return false;
                    }
                    if (data.waterRole == QSWATTopology._PLAYATYPE) {
                        QSWATUtils.information("Channel with LINKNO {0} flows out of {1} lake {2}.  This is ignored.".format(chLink, QSWATTopology.lakeCategory(data.waterRole), lakeIn), gv.isBatch, reportErrors: reportErrors);
                    } else if (lakeMain == lakeOut) {
                        // lake's main outlet
                        // channel leaves lake at upper end
                        reachData = this.getReachData(channel, demLayer);
                        Debug.Assert(reachData is not null);
                        subbasin = channel[channelBasinIndex];
                        data.outChLink = chLink;
                        elev = reachData.upperZ;
                        this.pointId += 1;
                        point = QgsPointXY(reachData.upperX, reachData.upperY);
                        QSWATUtils.loginfo("Added point {0} for channel link {1} flowing from lake {2}".format(this.pointId, chLink, lakeOut));
                        data.outPoint = (subbasin, this.pointId, point, elev);
                        this.chLinkFromLake[chLink] = lakeOut;
                    } else {
                        // other outlet
                        data.otherOutChLinks.add(chLink);
                    }
                }
            }
            // define lake elevation
            foreach (var data in this.lakesData.values()) {
                if (data.waterRole == QSWATTopology._PLAYATYPE) {
                    data.elevation = QSWATTopology.valueAtPoint(data.centroid, demLayer);
                } else {
                    var numInflows = data.inChLinks.Count;
                    data.elevation = numInflows == 0 ? data.outPoint[3] : float(data.elevation) / numInflows;
                }
            }
            return true;
        }
        
        public virtual bool isLakeInletOrOutlet(int chLink) {
            foreach (var lakeData in this.lakesData.values()) {
                if (lakeData.waterRole == QSWATTopology._PLAYATYPE) {
                    continue;
                }
                if (lakeData.inChLinks.Contains(chLink) || chLink == lakeData.outChLink || lakeData.otherOutChLinks.Contains(chLink)) {
                    return true;
                }
            }
            return false;
        }
        
        // Returns true if any part of geom intersects any part of polyGeom, or geom is within polyGeom. polyGeom has associated rectangle polyRect.
        [staticmethod]
        public static bool intersectsPoly(object geom, object polyGeom, object polyRect) {
            var geoRect = geom.boundingBox();
            if (QSWATTopology.disjointBoxes(geoRect, polyRect)) {
                return false;
            } else {
                return !geom.disjoint(polyGeom);
            }
        }
        
        // Return True if the boxes are disjoint.
        [staticmethod]
        public static bool disjointBoxes(object box1, object box2) {
            return box1.xMinimum() > box2.xMaximum() || box1.xMaximum() < box2.xMinimum() || box1.yMinimum() > box2.yMaximum() || box1.yMaximum() < box2.yMinimum();
        }
        
        // Return true if point within polyGeom, which has associated rectangle polyRect.
        [staticmethod]
        public static bool polyContains(object point, object polyGeom, object polyRect) {
            var _tmp_1 = point.x();
            var _tmp_2 = point.y();
            if (polyRect.xMinimum() < _tmp_1 && _tmp_1 < polyRect.xMaximum() && (polyRect.yMinimum() < _tmp_2 && _tmp_2 < polyRect.yMaximum())) {
                return polyGeom.contains(point);
            } else {
                return false;
            }
        }
        
        // Save lakes data in project database.
        public virtual object saveLakesData(object db) {
            using (var conn = db.conn) {
                if (!conn) {
                    return;
                }
                curs = conn.cursor();
                lakesTable = "LAKESDATA";
                clearSQL = "DROP TABLE IF EXISTS " + lakesTable;
                curs.execute(clearSQL);
                curs.execute(db._CREATELAKESDATA);
                linksTable = "LAKELINKS";
                clearSQL = "DROP TABLE IF EXISTS " + linksTable;
                curs.execute(clearSQL);
                curs.execute(db._CREATELAKELINKS);
                basinsTable = "LAKEBASINS";
                clearSQL = "DROP TABLE IF EXISTS " + basinsTable;
                curs.execute(clearSQL);
                curs.execute(db._CREATELAKEBASINS);
                foreach (var (lakeId, lakeData) in this.lakesData.items()) {
                    point = lakeData.outPoint[2];
                    if (point is null) {
                        if (new HashSet {
                            QSWATTopology._RESTYPE,
                            QSWATTopology._PONDTYPE,
                            QSWATTopology._WETLANDTYPE
                        }.Contains(lakeData.waterRole)) {
                            QSWATUtils.loginfo("No outpoint for lake {0}".format(lakeId));
                        }
                        x = 0;
                        y = 0;
                    } else {
                        x = lakeData.outPoint[2].x();
                        y = lakeData.outPoint[2].y();
                    }
                    curs.execute(db._INSERTLAKESDATA, (lakeId, lakeData.outPoint[0], lakeData.waterRole, lakeData.area, lakeData.overrideArea, lakeData.elevation, lakeData.outChLink, lakeData.outPoint[1], x, y, lakeData.outPoint[3], lakeData.centroid.x(), lakeData.centroid.y()));
                    // QSWATUtils.loginfo(str(lakeData.inChLinks.keys()))
                    // QSWATUtils.loginfo(str(lakeData.lakeChLinks))
                    foreach (var (chLink, (pointId, pt, elev)) in lakeData.inChLinks.items()) {
                        try {
                            curs.execute(db._INSERTLAKELINKS, (chLink, lakeId, true, false, pointId, pt.x(), pt.y(), elev));
                        } catch {
                            QSWATUtils.error("Failed to add in channel link {0}".format(chLink), this.isBatch);
                        }
                    }
                    foreach (var chLink in lakeData.lakeChLinks) {
                        try {
                            curs.execute(db._INSERTLAKELINKS, (chLink, lakeId, false, true, null, null, null, null));
                        } catch {
                            QSWATUtils.error("Failed to add inside channel link {0}".format(chLink), this.isBatch);
                        }
                    }
                    foreach (var chLink in lakeData.otherOutChLinks) {
                        try {
                            curs.execute(db._INSERTLAKELINKS, (chLink, lakeId, false, false, null, null, null, null));
                        } catch {
                            QSWATUtils.error("Failed to add other out channel link {0}".format(chLink), this.isBatch);
                        }
                    }
                }
                foreach (var (subbasin, lakeId) in this.outletsInLake.items()) {
                    curs.execute(db._INSERTLAKEBASINS, (subbasin, lakeId));
                }
                db.hashDbTable(conn, lakesTable);
                db.hashDbTable(conn, linksTable);
                db.hashDbTable(conn, basinsTable);
            }
        }
        
        // Read lakes data from project database.  Return true if data read OK, false if no data or error.
        public virtual bool readLakesData(object db) {
            using (var conn = db.conn) {
                if (!conn) {
                    return false;
                }
                this.lakesData.clear();
                this.chLinkIntoLake.clear();
                this.chLinkInsideLake.clear();
                this.chLinkFromLake.clear();
                this.outletsInLake.clear();
                curs = conn.cursor();
                lakesTable = "LAKESDATA";
                linksTable = "LAKELINKS";
                basinsTable = "LAKEBASINS";
                lakeSql = db.sqlSelect(lakesTable, "*", "", "");
                linksSql = db.sqlSelect(linksTable, "*", "", "lakeid=?");
                basinsSql = db.sqlSelect(basinsTable, "*", "", "");
                try {
                    // without fetchall this only reads first row.  Strange
                    foreach (var lakeRow in curs.execute(lakeSql).fetchall()) {
                        lakeId = Convert.ToInt32(lakeRow["id"]);
                        this.waterBodyId = max(this.waterBodyId, lakeId);
                        this.lakesData[lakeId] = LakeData(lakeRow["area"], lakeRow["overridearea"], QgsPointXY(lakeRow["centroidx"], lakeRow["centroidy"]), lakeRow["role"]);
                        outChLink = lakeRow["outlink"];
                        this.lakesData[lakeId].outChLink = outChLink;
                        this.chLinkFromLake[outChLink] = lakeId;
                        this.lakesData[lakeId].outPoint = (lakeRow["subbasin"], lakeRow["outletid"], QgsPointXY(lakeRow["outletx"], lakeRow["outlety"]), lakeRow["outletelev"]);
                        this.lakesData[lakeId].centroid = QgsPointXY(lakeRow["centroidx"], lakeRow["centroidy"]);
                        this.lakesData[lakeId].elevation = lakeRow["meanelev"];
                        foreach (var linkRow in curs.execute(linksSql, ValueTuple.Create(lakeId))) {
                            chLink = linkRow["linkno"];
                            if (linkRow["inside"]) {
                                this.lakesData[lakeId].lakeChLinks.add(chLink);
                                this.chLinkInsideLake[chLink] = lakeId;
                            } else if (linkRow["inlet"]) {
                                this.lakesData[lakeId].inChLinks[chLink] = (linkRow["inletid"], QgsPointXY(linkRow["inletx"], linkRow["inlety"]), linkRow["inletelev"]);
                                this.chLinkIntoLake[chLink] = lakeId;
                            } else {
                                this.lakesData[lakeId].otherOutChLinks.add(chLink);
                                this.chLinkFromLake[chLink] = lakeId;
                            }
                        }
                    }
                    foreach (var basinRow in curs.execute(basinsSql).fetchall()) {
                        this.outletsInLake[basinRow["subbasin"]] = basinRow["lakeid"];
                    }
                    return this.lakesData.Count > 0;
                } catch {
                    QSWATUtils.loginfo("Reading lakes data failed: {0}".format(traceback.format_exc()));
                    return false;
                }
            }
        }
        
        // Get downstream channel, skipping zero-length channels.
        //         
        //         Returns -1 if the channel flows into a lake.
        public virtual int getDownChannel(int channel) {
            if (this.chLinkIntoLake.Contains(channel)) {
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
        public virtual object setChannelBasinAreas(object gv) {
            this.chBasinAreas.clear();
            var unitArea = this.dx * this.dy;
            var completed = false;
            var raster = Raster(gv.channelBasinFile, gv);
            while (!completed) {
                try {
                    // safer to mark complete immediately to avoid danger of endless loop
                    // only way to loop is then the memory error exception being raised 
                    completed = true;
                    if (!raster.open(this.chunkCount)) {
                        QSWATUtils.error("Failed to open channel basins raster {0}".format(gv.channelBasinFile), gv.isBatch);
                        return;
                    }
                    foreach (var row in Enumerable.Range(0, raster.numRows)) {
                        foreach (var col in Enumerable.Range(0, raster.numCols)) {
                            var val = Convert.ToInt32(raster.read(row, col));
                            if (val == raster.noData) {
                                continue;
                            }
                            if (this.chBasinAreas.Contains(val)) {
                                this.chBasinAreas[val] += unitArea;
                            } else {
                                this.chBasinAreas[val] = unitArea;
                            }
                        }
                    }
                    raster.close();
                } catch (MemoryError) {
                    QSWATUtils.loginfo("Out of memory calculating channel basin areas with chunk count {0}".format(this.chunkCount));
                    try {
                        raster.close();
                    } catch (Exception) {
                    }
                    completed = false;
                    this.chunkCount += 1;
                }
            }
        }
        
        // Return copy of areas.
        [staticmethod]
        public static object copyBasinAreas(object areas) {
            var areas2 = new dict();
            foreach (var (i, val) in areas.items()) {
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
        public virtual bool checkAreas(object subbasinsLayer, object gv) {
            object basinArea;
            // TODO:  make work with lakes
            if (gv.projName.Contains("test")) {
                var unitArea = this.dx * this.dy;
                var polyIndex = this.getIndex(subbasinsLayer, QSWATTopology._POLYGONID);
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
                //                     return abs(x - y) < 0.01 * x
                var areaFactor = gv.horizontalFactor * gv.horizontalFactor;
                foreach (var poly in subbasinsLayer.getFeatures()) {
                    if (areaIndex < 0) {
                        basinArea = poly.geometry().area() * areaFactor;
                    } else {
                        basinArea = poly[areaIndex] * 10000.0;
                    }
                    // need to count areas of basins upstream from inlets because comparison for whole watershed
                    // by using all channels will not exclude them
                    totalBasinsArea += basinArea;
                    var basin = poly[polyIndex];
                    var chBasins = new HashSet<object>();
                    var chLinks = new HashSet<object>();
                    foreach (var (chLink, chBasin) in this.chLinkToChBasin.items()) {
                        if (basin == this.chBasinToSubbasin.get(chBasin, -1)) {
                            chBasins.add(chBasin);
                            chLinks.add(chLink);
                        }
                    }
                    var area = 0.0;
                    foreach (var (chBasin, chArea) in this.chBasinAreas.items()) {
                        if (chBasins.Contains(chBasin)) {
                            area += chArea;
                        }
                    }
                    if (abs(basinArea - area) >= unitArea) {
                        // not using compare(basinArea, area):
                        var SWATChannels = (from chLink in chLinks
                            select this.channelToSWATChannel[chLink]).ToHashSet();
                        var SWATBasin = this.subbasinToSWATBasin[basin];
                        QSWATUtils.error("Basin {0} with area {1} has channels {2} with total area {3}".format(SWATBasin, basinArea, SWATChannels, area), gv.isBatch);
                        // return true so run continue if user regards error as small
                        return true;
                    }
                }
                // now compare areas for whole watershed
                foreach (var (_, chArea) in this.chBasinAreas.items()) {
                    totalChannelBasinsArea += chArea;
                }
                if (abs(totalBasinsArea - totalChannelBasinsArea) >= unitArea) {
                    // not using compare(totalBasinsArea, totalChannelBasinsArea):
                    QSWATUtils.error("Watershed area is {0} by adding subbasin areas and {1} by adding channel basin areas".format(totalBasinsArea, totalChannelBasinsArea), gv.isBatch);
                    // return true so run continue if user regards error as small
                    return true;
                }
                QSWATUtils.loginfo("Total watershed area is {0}".format(totalBasinsArea));
            }
            return true;
        }
        
        // Return true if chLink is in chLinks or reachable from an item in chLinks via the one-many relation us.
        [staticmethod]
        public static bool reachable(int chLink, List<int> chLinks, object us) {
            if (chLinks.Contains(chLink)) {
                return true;
            }
            foreach (var nxt in chLinks) {
                if (QSWATTopology.reachable(chLink, us.get(nxt, new List<object>()), us)) {
                    return true;
                }
            }
            return false;
        }
        
        //===========================================================================
        // def addUpstreamLinks(self, link, us):
        //     """Add to upstreamFromInlets the links upstream from link."""
        //     ups = us.get(link, None)
        //     if ups is not None:
        //         for up in ups:
        //             self.upstreamFromInlets.add(up)
        //             self.addUpstreamLinks(up, us)
        //===========================================================================
        // Get drain areas from channelLayer file's DS_Cont_Ar attribute.
        public virtual object setDrainageFromChannels(object channelLayer, int drainAreaIndex) {
            var inds = new List<int> {
                this.channelIndex,
                drainAreaIndex
            };
            var areaFactor = this.horizontalFactor * this.horizontalFactor;
            var request = QgsFeatureRequest().setFlags(QgsFeatureRequest.NoGeometry).setSubsetOfAttributes(inds);
            foreach (var reach in channelLayer.getFeatures(request)) {
                var channelLink = reach[this.channelIndex];
                this.drainAreas[channelLink] = reach[drainAreaIndex] * areaFactor;
            }
        }
        
        // Get drain areas from channelLayer file's Drainage attribute.  Return True if successful.
        public virtual bool setGridDrainageFromChannels(object channelLayer, object subbasinsLayer, int maxChLink, object us) {
            var channelIndex = this.getIndex(channelLayer, QSWATTopology._LINKNO, ignoreMissing: true);
            var drainageIndex = this.getIndex(channelLayer, QSWATTopology._DRAINAGE, ignoreMissing: true);
            if (channelIndex < 0 || drainageIndex < 0) {
                return false;
            }
            var inds = new List<int> {
                channelIndex,
                drainageIndex
            };
            var request = QgsFeatureRequest().setFlags(QgsFeatureRequest.NoGeometry).setSubsetOfAttributes(inds);
            foreach (var reach in channelLayer.getFeatures(request)) {
                var channel = reach[channelIndex];
                this.drainAreas[channel] = reach[drainageIndex] * 1000000.0;
            }
            this.checkDrainage(channelLayer, subbasinsLayer, maxChLink, us, false);
            return true;
        }
        
        // Calculate and save grid drain areas in sq km.
        public virtual object setGridDrainageAreas(
            object channelLayer,
            object subbasinsLayer,
            int maxChLink,
            object us,
            double gridCellArea) {
            // check to see if drain areas already calculated - may have been done with lakes
            if (this.drainAreas is null) {
                return;
            }
            foreach (var link in this.downChannels.keys()) {
                if (this.drainAreas[link] > gridCellArea) {
                    return;
                }
            }
            this.drainAreas.fill(gridCellArea);
            this.checkDrainage(channelLayer, subbasinsLayer, maxChLink, us, true);
        }
        
        // Check drainage as defined by downChannels map is not circular: attempt to fix if so.  Also calculate drainage in 
        //         self.drainAreas if needDrainage is true.  If so, assume it is initialised with area of grid cell.
        //         
        //         Only used in grid models.
        //         Changes the channel and the grid shapefiles, plus the downChannels and downSubbasins mappings, plus the upchannel map us if not None.
        public virtual object checkDrainage(
            object channelLayer,
            object subbasinsLayer,
            int maxChLink,
            object us,
            bool needDrainage) {
            object lakeData;
            object dsLink;
            object link;
            // number of incoming links for each link
            var incount = zeros(maxChLink + 1, dtype: @int);
            foreach (var dsLink in this.downChannels.values()) {
                if (dsLink >= 0) {
                    incount[dsLink] += 1;
                }
            }
            // queue contains all links whose drainage areas have been calculated 
            // i.e. will not increase and can be propagated
            var queue = (from link in Enumerable.Range(0, maxChLink + 1)
                where incount[link] == 0
                select link).ToList();
            while (queue) {
                link = queue.pop(0);
                dsLink = this.downChannels.get(link, -1);
                if (dsLink >= 0) {
                    if (needDrainage) {
                        this.drainAreas[dsLink] += this.drainAreas[link];
                    }
                    incount[dsLink] -= 1;
                    if (incount[dsLink] == 0) {
                        queue.append(dsLink);
                    }
                }
            }
            // incount values should now all be zero
            var remainder = (from link in Enumerable.Range(0, maxChLink + 1)
                where incount[link] > 0
                select link).ToList();
            if (remainder) {
                QSWATUtils.loginfo("Drainage areas incomplete.  There is a circularity in links {0!s}".format(remainder));
                // remainder may contain a number of circles.
                var rings = new List<object>();
                var nextRing = new List<object>();
                link = remainder.pop(0);
                while (true) {
                    nextRing.append(link);
                    dsLink = this.downChannels[link];
                    if (nextRing.Contains(dsLink)) {
                        // complete the ring
                        nextRing.append(dsLink);
                        rings.append(nextRing);
                        if (remainder) {
                            nextRing = new List<object>();
                            link = remainder.pop(0);
                        } else {
                            break;
                        }
                    } else {
                        // continue
                        remainder.remove(dsLink);
                        link = dsLink;
                    }
                }
                var numRings = rings.Count;
                if (numRings > 0) {
                    var channelMap = new dict();
                    var polyMap = new dict();
                    var channelProvider = channelLayer.dataProvider();
                    var linkIndex = channelProvider.fieldNameIndex(QSWATTopology._LINKNO);
                    var DSLINKNO = this.isHUC ? QSWATTopology._DSLINKNO1 : QSWATTopology._DSLINKNO;
                    var dsLinkIndex = channelProvider.fieldNameIndex(DSLINKNO);
                    var subProvider = subbasinsLayer.dataProvider();
                    var dsPolyIndex = subProvider.fieldNameIndex(QSWATTopology._DOWNID);
                    QSWATUtils.information("Drainage areas incomplete.  There are {0} circularities.  Will try to remove them.  See the QSWAT+ log for details".format(numRings), this.isBatch);
                    foreach (var ring in rings) {
                        QSWATUtils.loginfo("Circularity in links {0!s}".format(ring));
                        // fix the circularity by making the largest drainage link an exit in the downChannels map
                        var maxDrainage = 0;
                        var maxLink = -1;
                        foreach (var link in ring) {
                            var drainage = this.drainAreas[link];
                            if (drainage > maxDrainage) {
                                maxLink = link;
                                maxDrainage = drainage;
                            }
                        }
                        if (maxLink < 0) {
                            QSWATUtils.error("Failed to find link with largest drainage in circle {0!s}".format(ring), this.isBatch);
                        } else {
                            this.downChannels[maxLink] = -1;
                            var linkExpr = QgsExpression("\"{0}\" = {1}".format(QSWATTopology._LINKNO, maxLink));
                            var linkRequest = QgsFeatureRequest(linkExpr).setFlags(QgsFeatureRequest.NoGeometry).setSubsetOfAttributes(new List<object> {
                                linkIndex,
                                dsLinkIndex
                            });
                            foreach (var feature in channelProvider.getFeatures(linkRequest)) {
                                channelMap[feature.id()] = new Dictionary<object, object> {
                                    {
                                        dsLinkIndex,
                                        -1}};
                                if (us is not null) {
                                    link = feature[linkIndex];
                                    dsLink = feature[dsLinkIndex];
                                    us[dsLink].remove(link);
                                }
                            }
                            // also need to fix downSubbasins map
                            // subbasin same as chBasin since grid model
                            var basin = this.chLinkToChBasin[maxLink];
                            this.downSubbasins[basin] = -1;
                            var polyExpr = QgsExpression("\"{0}\" = {1}".format(QSWATTopology._POLYGONID, basin));
                            var polyRequest = QgsFeatureRequest(polyExpr).setFlags(QgsFeatureRequest.NoGeometry).setSubsetOfAttributes(new List<object>());
                            foreach (var feature in subProvider.getFeatures(polyRequest)) {
                                polyMap[feature.id()] = new Dictionary<object, object> {
                                    {
                                        dsPolyIndex,
                                        -1}};
                            }
                            QSWATUtils.loginfo("Link {0} and polygon {1} made into an exit".format(maxLink, basin));
                            // if there is a lake with the exit link internal to it we should make it the lake outlet
                            var lakeId = this.chLinkInsideLake.get(maxLink, -1);
                            if (lakeId > 0) {
                                lakeData = this.lakesData[lakeId];
                                if (lakeData.outChLink >= 0) {
                                    // add old outlet to other outlets
                                    var oldOutLink = lakeData.outChLink;
                                    lakeData.otherOutChLinks.add(oldOutLink);
                                    // change maxLink from internal to outlet
                                    lakeData.lakeChLinks.remove(maxLink);
                                    lakeData.outChLink = maxLink;
                                    QSWATUtils.loginfo("Outlet of lake {0} changed from link {1} to link {2}".format(lakeId, oldOutLink, maxLink));
                                    this.chLinkFromLake[maxLink] = lakeId;
                                    this.chLinkInsideLake.Remove(maxLink);
                                }
                            }
                            // and if the channel flows into a lake we should remove it from the lake's inlets
                            lakeId = this.chLinkIntoLake.get(maxLink, -1);
                            if (lakeId > 0) {
                                lakeData = this.lakesData[lakeId];
                                lakeData.inChLinks.Remove(maxLink);
                                this.chLinkIntoLake.Remove(maxLink);
                            }
                        }
                    }
                    channelProvider.changeAttributeValues(channelMap);
                    subProvider.changeAttributeValues(polyMap);
                }
            }
        }
        
        // 
        //         Calculate and save drainAreas.
        //         
        //         Not used with grid models.
        //         
        public virtual object setDrainageAreas(object us) {
            foreach (var (chLink, chBasin) in this.chLinkToChBasin.items()) {
                this.setLinkDrainageArea(chLink, chBasin, us);
            }
        }
        
        // 
        //         Calculate and save drainArea for chLink.
        //         
        //         Not used with grid models.
        //         
        public virtual object setLinkDrainageArea(int chLink, int chBasin, object us) {
            if (this.drainAreas[chLink] > 0) {
                // already done in calculating one further downstream
                return;
            }
            var ownArea = this.chBasinAreas.get(chBasin, 0);
            var upsArea = 0;
            var ups = us.get(chLink, new List<object>());
            foreach (var up in ups) {
                this.setLinkDrainageArea(up, this.chLinkToChBasin[up], us);
                upsArea += this.drainAreas[up];
            }
            this.drainAreas[chLink] = ownArea + upsArea;
        }
        
        // Get Strahler order from channelLayer file's order attribute.
        public virtual object setStrahlerFromChannels(object channelLayer, int orderIndex) {
            var inds = new List<int> {
                this.channelIndex,
                orderIndex
            };
            var request = QgsFeatureRequest().setFlags(QgsFeatureRequest.NoGeometry).setSubsetOfAttributes(inds);
            foreach (var reach in channelLayer.getFeatures(request)) {
                var channelLink = reach[this.channelIndex];
                this.strahler[channelLink] = reach[orderIndex];
            }
        }
        
        // Set Strahler orders using upstream map from each outlet.
        //         
        //         Not used with grid models
        public virtual object setStrahler(object us) {
            object setStrahlerLink(int link) {
                ups = us.get(link, new List<object>());
                if (ups == new List<object>()) {
                    this.strahler[link] = 1;
                    return 1;
                }
                orders = (from up in ups
                    select setStrahlerLink(up)).ToList();
                omax = max(orders);
                count = (from o in orders
                    where o == omax
                    select o).ToList().Count;
                order = count == 1 ? omax : omax + 1;
                this.strahler[link] = order;
                return order;
            }
            foreach (var (_, _, links) in this.outlets.values()) {
                foreach (var link in links) {
                    setStrahlerLink(link);
                }
            }
        }
        
        // Set Strahler orders for channels using upstream map us.
        //         
        //         Non-recusive as used in grid models.
        public virtual object setStrahlerFromGrid(object us) {
            // continue looping as long as an order is calculated
            var changed = true;
            while (changed) {
                changed = false;
                foreach (var link in this.downChannels.keys()) {
                    if (this.strahler[link] > 0) {
                        // already calculated
                        continue;
                    } else {
                        var ups = us.get(link, new List<object>());
                        if (ups == new List<object>()) {
                            this.strahler[link] = 1;
                            changed = true;
                        } else {
                            var orders = (from up in ups
                                select this.strahler[up]).ToList();
                            if (orders.Contains(0)) {
                                // at least one yet to be calculated
                                continue;
                            } else {
                                var omax = max(orders);
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
            var distances = this.distancesToJoins.get(link, new dict());
            var distance = distances.get(otherLink, -1);
            if (distance < 0) {
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
            var result = new List<object>();
            var nxt = start;
            while (true) {
                nxt = this.downStreams.get(nxt, -1);
                if (nxt == -1) {
                    break;
                }
                result = new List<object> {
                    nxt
                } + result;
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
            return path1[i];
        }
        
        // 
        //         Add basinno field (if necessary) to channels shapefile and populate with values from wStreamFile.
        //         
        //         Not done with grid models.
        //         
        public virtual object addBasinsToChannelFile(object channelLayer, string wStreamFile) {
            object line;
            object length;
            object OK;
            var provider = channelLayer.dataProvider();
            var bsnIdx = this.getIndex(channelLayer, QSWATTopology._BASINNO, ignoreMissing: true);
            if (bsnIdx < 0) {
                var field = QgsField(QSWATTopology._BASINNO, QVariant.Int);
                OK = provider.addAttributes(new List<object> {
                    field
                });
                if (!OK) {
                    QSWATUtils.error("Cannot add {0} field to channels shapefile".format(QSWATTopology._BASINNO), this.isBatch);
                    return;
                }
                channelLayer.updateFields();
                bsnIdx = this.getIndex(channelLayer, QSWATTopology._BASINNO);
            }
            var wLayer = QgsRasterLayer(wStreamFile, "Basins");
            var wNoData = wLayer.dataProvider().sourceNoDataValue(1);
            var lenIdx = this.getIndex(channelLayer, QSWATTopology._LENGTH, ignoreMissing: true);
            var chsMap = new dict();
            foreach (var feature in provider.getFeatures()) {
                // find a point well into the channel to ensure we are not just outside the basin
                var geometry = feature.geometry();
                if (lenIdx < 0) {
                    length = geometry.length() * this.horizontalFactor;
                } else {
                    length = feature[lenIdx] * this.horizontalFactor;
                }
                if (length <= 0) {
                    var basin = QSWATTopology._NOBASIN;
                } else {
                    if (geometry.isMultipart()) {
                        var lines = geometry.asMultiPolyline();
                        var numLines = lines.Count;
                        if (numLines == 0) {
                            QSWATUtils.error("Link in channel with id {0} consists of 0 lines".format(feature.id()), this.isBatch);
                            return;
                        }
                        line = lines[numLines / 2];
                    } else {
                        line = geometry.asPolyline();
                    }
                    var numPoints = line.Count;
                    if (numPoints < 2) {
                        QSWATUtils.error("Link in channel with id {0} has less than two points".format(feature.id()), this.isBatch);
                        return;
                    }
                    var point = line[numPoints / 2];
                    var wVal = QSWATTopology.valueAtPoint(point, wLayer);
                    if (wVal is null || math.isclose(wVal, wNoData, rel_tol: 1E-06)) {
                        // type: ignore # outside watershed  
                        basin = -1;
                    } else {
                        basin = cast(@int, wVal);
                    }
                }
                var fid = feature.id();
                chsMap[fid] = new dict();
                chsMap[fid][bsnIdx] = basin;
            }
            OK = provider.changeAttributeValues(chsMap);
            if (!OK) {
                QSWATUtils.error("Cannot add basin values to channels shapefile", this.isBatch);
            }
            return;
        }
        
        // Write drainage csv file.
        public virtual object writeDrainageFile(string drainageFile) {
            if (os.path.exists(drainageFile)) {
                os.remove(drainageFile);
            }
            using (var connFile = open(drainageFile, "w", newline: "")) {
                writer = csv.writer(connFile);
                writer.writerow(new List<string> {
                    "PolygonId",
                    "DownId"
                });
                foreach (var (subbasin, downSubbasin) in this.downSubbasins.items()) {
                    writer.writerow(new List<string> {
                        subbasin.ToString(),
                        downSubbasin.ToString()
                    });
                }
            }
        }
        
        // 
        //         Generate ReachData record for reach geometry.  demLayer may be none, or point may give nodata for elevation, in which case elevations are set zero.
        //         
        public virtual object getReachData(object reach, object demLayer) {
            object minElev;
            object maxElev;
            object pFinish;
            object pStart;
            if (this.isHUC || this.isHAWQS) {
                var wsno = reach[this.wsnoIndex];
                pStart = this.chPointSources[wsno][1];
                pFinish = this.chOutlets[wsno][1];
            } else {
                var geom = reach.geometry();
                var firstLine = QSWATTopology.reachFirstLine(geom, this.xThreshold, this.yThreshold);
                if (firstLine is null || firstLine.Count < 1) {
                    QSWATUtils.error("It looks like your stream shapefile does not obey the single direction rule, that all reaches are either upstream or downstream.", this.isBatch);
                    return null;
                }
                var lastLine = QSWATTopology.reachLastLine(geom, this.xThreshold, this.yThreshold);
                if (lastLine is null || lastLine.Count < 1) {
                    QSWATUtils.error("It looks like your stream shapefile does not obey the single direction rule, that all reaches are either upstream or downstream.", this.isBatch);
                    return null;
                }
                pStart = firstLine[0];
                pFinish = lastLine[^1];
            }
            if (demLayer is null) {
                var startVal = 0.0;
                var finishVal = 0.0;
            } else {
                startVal = QSWATTopology.valueAtPoint(pStart, demLayer);
                finishVal = QSWATTopology.valueAtPoint(pFinish, demLayer);
                // with grid models centre of cell at edge can have no data for elevation
                if (startVal is null) {
                    startVal = 0.0;
                }
                if (finishVal is null) {
                    finishVal = 0.0;
                }
            }
            Debug.Assert(startVal is not null);
            Debug.Assert(finishVal is not null);
            if (this.outletAtStart) {
                maxElev = finishVal * this.verticalFactor;
                minElev = startVal * this.verticalFactor;
                return ReachData(pFinish.x(), pFinish.y(), maxElev, pStart.x(), pStart.y(), minElev);
            } else {
                minElev = finishVal * this.verticalFactor;
                maxElev = startVal * this.verticalFactor;
                return ReachData(pStart.x(), pStart.y(), maxElev, pFinish.x(), pFinish.y(), minElev);
            }
        }
        
        // Length of reach assuming it is a single straight line.
        [staticmethod]
        public static double gridReachLength(object data) {
            var dx = data.upperX - data.lowerX;
            var dy = data.upperY - data.lowerY;
            return math.sqrt(dx * dx + dy * dy);
        }
        
        // Return true if the subbasin field values can be used as SWAT basin numbers.
        //         
        //         The basin numbers, if any, can be used if they 
        //         are all positive and different.
        //         Also populate subbasinToSWATBasin and SWATBasinToSubbasin if successful, else these are undetermined.
        //         
        public virtual bool tryBasinAsSWATBasin(object subbasinsLayer, int polyIndex, int subbasinIndex, bool useGridModel) {
            Debug.Assert(polyIndex >= 0 && subbasinIndex >= 0 && this.subbasinToSWATBasin.Count == 0 && this.SWATBasinToSubbasin.Count == 0);
            var SWATBasins = new HashSet<object>();
            var request = QgsFeatureRequest().setFlags(QgsFeatureRequest.NoGeometry).setSubsetOfAttributes(new List<int> {
                polyIndex,
                subbasinIndex
            });
            foreach (var polygon in subbasinsLayer.getFeatures(request)) {
                var subbasin = polygon[polyIndex];
                if (this.upstreamFromInlets.Contains(subbasin) || !(useGridModel || this.chBasinToSubbasin.values().Contains(subbasin))) {
                    // removed by chBasin being within a lake
                    continue;
                }
                var SWATBasin = polygon[subbasinIndex];
                if (SWATBasin <= 0) {
                    return false;
                }
                if (SWATBasins.Contains(SWATBasin)) {
                    return false;
                }
                this.subbasinToSWATBasin[subbasin] = SWATBasin;
                this.SWATBasinToSubbasin[SWATBasin] = subbasin;
                SWATBasins.add(SWATBasin);
            }
            return true;
        }
        
        // Add PointId field if necessary and set to id or largest id + 1 if id is zero.  Return index of PointId.
        [staticmethod]
        public static int makePositiveOutletIds(object outletLayer) {
            var outletProvider = outletLayer.dataProvider();
            var ptIdIndex = outletProvider.fieldNameIndex(QSWATTopology._POINTID);
            if (ptIdIndex < 0) {
                outletProvider.addAttributes(new List<object> {
                    QgsField(QSWATTopology._POINTID, QVariant.Int)
                });
                outletLayer.updateFields();
                ptIdIndex = outletProvider.fieldNameIndex(QSWATTopology._POINTID);
            }
            var idIndex = outletProvider.fieldNameIndex(QSWATTopology._ID);
            var maxId = 0;
            var zeroId = -1;
            var request = QgsFeatureRequest().setFlags(QgsFeatureRequest.NoGeometry).setSubsetOfAttributes(new List<object> {
                idIndex,
                ptIdIndex
            });
            var mmap = new dict();
            foreach (var point in outletProvider.getFeatures(request)) {
                var pid = point.id();
                var pointId = point[idIndex];
                if (pointId == 0) {
                    zeroId = pid;
                } else {
                    mmap[pid] = new Dictionary<object, object> {
                        {
                            ptIdIndex,
                            pointId}};
                    maxId = max(maxId, pointId);
                }
            }
            if (zeroId >= 0) {
                mmap[zeroId] = new Dictionary<object, object> {
                    {
                        ptIdIndex,
                        maxId + 1}};
            }
            if (outletProvider.changeAttributeValues(mmap)) {
                return ptIdIndex;
            } else {
                return -1;
            }
        }
        
        // Return the nearest point on a stream segment to the input point.
        [staticmethod]
        public static object snapPointToReach(
            object streamLayer,
            object point,
            double threshold,
            object transform,
            bool isBatch) {
            (line, pointIndex) = QSWATTopology.nearestVertex(streamLayer, point);
            if (pointIndex < 0) {
                QSWATUtils.error("Cannot snap point ({0:.0f}, {1:.0f}) to stream network".format(point.x(), point.y()), isBatch);
                return null;
            }
            (p1, p2) = QSWATTopology.intercepts(line, pointIndex, point);
            var p = QSWATTopology.nearer(p1, p2, point);
            if (p is null) {
                p = line[pointIndex];
                return QSWATTopology.distanceMeasure(p, point) <= threshold * threshold ? p : null;
            }
            // check p is sufficiently near point
            if (QSWATTopology.distanceMeasure(p, point) <= threshold * threshold) {
                // before returning p, move it along the stream a little if it is on or close to a '4 corners' position
                // since TauDEM can fail to make a boundary or use its id as a DSNODEID if is so positioned
                if (p1 == p2) {
                    // a point on the line was chosen, which is safe (points on the line are centres of DEM cells)
                    return p;
                } else {
                    var floatCol = float(p.x() - transform[0]) / transform[1];
                    var intCol = Convert.ToInt32(floatCol + 0.5);
                    var floatRow = float(p.y() - transform[3]) / transform[5];
                    var intRow = Convert.ToInt32(floatRow + 0.5);
                    if (abs(floatCol - intCol) < 0.1 && abs(floatRow - intRow) < 0.1) {
                        // move the point towards line[pointIndex] by about half a cell
                        var p3 = QSWATTopology.shiftedPoint(p, line[pointIndex], transform, 0.4);
                        QSWATUtils.loginfo("({0:.0f},{1:.0f}) shifted to ({2:.0f},{3:.0f})".format(p.x(), p.y(), p3.x(), p3.y()));
                        return p3;
                    } else {
                        return p;
                    }
                }
            } else {
                QSWATUtils.error("Cannot snap point ({0:.0f}, {1:.0f}) to stream network within threshold {2!s}".format(point.x(), point.y(), threshold), isBatch);
                return null;
            }
        }
        
        // Return true if points not separated by more than numPixels pixels.
        [staticmethod]
        public static bool withinPixels(int numPixels, object p1, object p2, object transform) {
            (col1, row1) = QSWATTopology.projToCell(p1.x(), p1.y(), transform);
            (col2, row2) = QSWATTopology.projToCell(p2.x(), p2.y(), transform);
            return abs(col1 - col2) <= numPixels && abs(row1 - row2) <= numPixels;
        }
        
        // Return true if p is on a pixel boundary or very close.
        [staticmethod]
        public static bool onPixelBoundary(object p, object transform) {
            var floatCol = float(p.x() - transform[0]) / transform[1];
            var intCol = Convert.ToInt32(floatCol + 0.5);
            var floatRow = float(p.y() - transform[3]) / transform[5];
            var intRow = Convert.ToInt32(floatRow + 0.5);
            return abs(floatCol - intCol) < 0.1 || abs(floatRow - intRow) < 0.1;
        }
        
        // If p2 is in same cell as p1 return a point in the next cell in the direction of p1 to p2.
        //         Else return p2.
        [staticmethod]
        public static object separatePoints(object p1, object p2, object transform) {
            if (QSWATTopology.withinPixels(1, p1, p2, transform)) {
                // p1 and p2 are in adjacent or same cells.  
                if (QSWATTopology.withinPixels(0, p1, p2, transform) || QSWATTopology.onPixelBoundary(p2, transform)) {
                    return QSWATTopology.shiftedPoint(p1, p2, transform, 1.1);
                }
            }
            return p2;
        }
        
        // p is on channel.  Find a point downstream from p on the channel that is in a different pixel from the inlet.
        public virtual object shiftDownstream(object p, object channel, object inlet, object transform) {
            object newX;
            object newPoint;
            object newY;
            object j;
            object upper;
            object target;
            object parts;
            object isOnSegment(object p, object p1, object p2) {
                object between(double a, double b, double c) {
                    return min(b, c) <= a && a <= max(b, c);
                }
                return between(p.x(), p1.x(), p2.x()) && between(p.y(), p1.y(), p2.y());
            }
            // def posOnSegment(p: QgsPointXY, p1: QgsPointXY, p2: QgsPointXY) -> Tuple[float, float]:
            //     """Calculate ratios in x and y directions for p position between p1 and p2."""
            //     if p1.x() == p2.x():
            //         xRatio = p1.x() - p.x()
            //     else:
            //         xRatio = (p1.x() - p.x()) / (p1.x() - p2.x())
            //     if p1.y() == p2.y():
            //         yRatio = p1.y() - p.y()
            //     else:
            //         yRatio = (p1.y() - p.y()) / (p1.y() - p2.y())
            //     return (abs(xRatio), abs(yRatio))    
            //
            (col, row) = QSWATTopology.projToCell(inlet.x(), inlet.y(), transform);
            // first find channel segment that p1 is on
            var geometry = channel.geometry();
            if (geometry.isMultipart()) {
                parts = geometry.asMultiPolyline();
            } else {
                parts = new List<object> {
                    geometry.asPolyline()
                };
            }
            var found = false;
            var useOriginalPoint = true;
            foreach (var line in parts) {
                foreach (var j in Enumerable.Range(0, line.Count - 1)) {
                    //xRatio, yRatio = posOnSegment(p, line[j], line[j+1])
                    //if 0 <= xRatio <= 1 and 0 <= yRatio <= 1 and abs(xRatio - yRatio) < 0.1:
                    // point is on segement (with some margin for error)
                    if (isOnSegment(p, line[j], line[j + 1])) {
                        found = true;
                        if (this.outletAtStart) {
                            // need to move towards j from j+1
                            target = line[j];
                            upper = line[j + 1];
                        } else {
                            target = line[j + 1];
                            upper = line[j];
                        }
                        // target is best we can do on this segment, so check it is feasible
                        (colTarget, rowTarget) = QSWATTopology.projToCell(target.x(), target.y(), transform);
                        while (col == colTarget && row == rowTarget) {
                            useOriginalPoint = false;
                            if (this.outletAtStart) {
                                if (j > 0) {
                                    j = j - 1;
                                    target = line[j];
                                    upper = line[j + 1];
                                } else {
                                    return null;
                                }
                            } else if (j < line.Count - 2) {
                                j = j + 1;
                                target = line[j + 1];
                                upper = line[j];
                            } else {
                                return null;
                            }
                            (colTarget, rowTarget) = QSWATTopology.projToCell(target.x(), target.y(), transform);
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
            if (target.x() == upper.x()) {
                // vertical segment
                if (target.y() > upper.y()) {
                    newY = min(target.y(), p.y() + this.dy);
                } else {
                    newY = max(target.y(), p.y() - this.dy);
                }
                newPoint = QgsPointXY(target.x(), newY);
            } else {
                if (target.x() > upper.x()) {
                    newX = min(target.x(), p.x() + this.dx);
                } else {
                    newX = max(target.x(), p.x() - this.dx);
                }
                newY = p.y() + (newX - p.x()) * (target.y() - upper.y()) / (target.x() - upper.x());
                newPoint = QgsPointXY(newX, newY);
            }
            if (QSWATTopology.withinPixels(0, inlet, newPoint, transform)) {
                return null;
            } else {
                return newPoint;
            }
        }
        
        // Return point which is frac of a cell away from p1 in direction p1 to p2.
        [staticmethod]
        public static object shiftedPoint(object p1, object p2, object transform, double frac) {
            var x1 = p1.x();
            var y1 = p1.y();
            var x2 = p2.x();
            var y2 = p2.y();
            var dirx = x2 > x1 ? 1 : -1;
            var diry = y2 > y1 ? 1 : -1;
            var stepx = transform[1] * frac;
            var stepy = abs(transform[5]) * frac;
            if (x1 == x2) {
                // vertical
                var shiftx = 0.0;
                var shifty = stepy * diry;
            } else {
                var slope = abs(float(y1 - y2) / (x1 - x2));
                if (slope < 1) {
                    shiftx = stepx * dirx;
                    shifty = stepy * diry * slope;
                } else {
                    shiftx = stepx * dirx / slope;
                    shifty = stepy * diry;
                }
            }
            return QgsPointXY(x1 + shiftx, y1 + shifty);
        }
        
        // Find nearest vertex in streamLayer to point and 
        //         return the line (list of points) in the reach and 
        //         index of the vertex within the line.
        //         
        [staticmethod]
        public static object nearestVertex(object streamLayer, object point) {
            object parts;
            var bestPointIndex = -1;
            var bestLine = new List<object>();
            var minMeasure = double.PositiveInfinity;
            foreach (var reach in streamLayer.getFeatures()) {
                var geometry = reach.geometry();
                if (geometry.isMultipart()) {
                    parts = geometry.asMultiPolyline();
                } else {
                    parts = new List<object> {
                        geometry.asPolyline()
                    };
                }
                foreach (var line in parts) {
                    foreach (var j in Enumerable.Range(0, line.Count)) {
                        var measure = QSWATTopology.distanceMeasure(line[j], point);
                        if (measure < minMeasure) {
                            minMeasure = measure;
                            bestPointIndex = j;
                            bestLine = line;
                        }
                    }
                }
            }
            // distance = math.sqrt(minMeasure)
            // QSWATUtils.information(u'Nearest point at ({0:.2F}, {1:.2F}), distance {2:.2F}'.format(bestReach[bestPointIndex].x(), bestReach[bestPointIndex].y(), distance), False)
            return (bestLine, bestPointIndex);
        }
        
        // Get points on segments on either side of pointIndex where 
        //         vertical from point meets the segment.
        //         
        [staticmethod]
        public static object intercepts(List<object> line, int pointIndex, object point) {
            Debug.Assert(Enumerable.Range(0, line.Count).Contains(pointIndex));
            // first try above pointIndex
            if (pointIndex == line.Count - 1) {
                // We are at the upper end - no upper segment.  
                // Return just this point to avoid a tiny subbasin.
                return (line[pointIndex], line[pointIndex]);
            } else {
                var upper = QSWATTopology.getIntercept(line[pointIndex], line[pointIndex + 1], point);
            }
            if (pointIndex == 0) {
                // We are at the lower end - no lower segment.  
                // Return just this point to avoid a tiny subbasin.
                return (line[0], line[0]);
            } else {
                var lower = QSWATTopology.getIntercept(line[pointIndex], line[pointIndex - 1], point);
            }
            return (lower, upper);
        }
        
        // Return point on line from p1 to p2 where 
        //         vertical from p intercepts it, or None if there is no intercept.
        //         
        [staticmethod]
        public static object getIntercept(object p1, object p2, object p) {
            var x1 = p1.x();
            var x2 = p2.x();
            var xp = p.x();
            var y1 = p1.y();
            var y2 = p2.y();
            var yp = p.y();
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
                return QgsPointXY(x1 - prop * X, y1 - prop * Y);
            }
        }
        
        // Return the nearer of p1 and p2 to p.
        [staticmethod]
        public static object nearer(object p1, object p2, object p) {
            if (p1 is null) {
                return p2;
            }
            if (p2 is null) {
                return p1;
            }
            if (QSWATTopology.distanceMeasure(p1, p) < QSWATTopology.distanceMeasure(p2, p)) {
                return p1;
            } else {
                return p2;
            }
        }
        
        // Return square of distance between p1 and p2.
        [staticmethod]
        public static double distanceMeasure(object p1, object p2) {
            var dx = p1.x() - p2.x();
            var dy = p1.y() - p2.y();
            return dx * dx + dy * dy;
        }
        
        // 
        //         Write table of subbasin to maximum flow length along channels within the basin.
        //         
        //         Used for maximum flow path for existing non-grid models, and only defined for these.
        //         
        public virtual object setMaxFlowLengths() {
            var channelFlowLengths = new dict();
            foreach (var (chLink, length) in this.channelLengths.items()) {
                this.setChannelFlowLength(chLink, length, channelFlowLengths);
            }
        }
        
        // Add entry for chLink to channelFlowLengths map.  Also update maxFlowLengths for chLink's subbasin.
        //         
        //         post: chLink in channelFlowLengths
        //         
        public virtual object setChannelFlowLength(int chLink, double length, object channelFlowLengths) {
            object flowLength;
            if (channelFlowLengths.Contains(chLink)) {
                return;
            }
            if (this.zeroChannels.Contains(chLink)) {
                return;
            }
            var chBasin = this.chLinkToChBasin[chLink];
            var subbasin = this.chBasinToSubbasin[chBasin];
            var dsLink = this.downChannels[chLink];
            var dsChBasin = this.chLinkToChBasin.get(dsLink, -1);
            var dsBasin = this.chBasinToSubbasin.get(dsChBasin, -1);
            if (dsBasin == subbasin) {
                // still in same subbasin:
                // add this channel's length to downstream flow length
                var dsFlowLength = channelFlowLengths.get(dsLink, -1);
                if (dsFlowLength < 0) {
                    this.setChannelFlowLength(dsLink, this.channelLengths[dsLink], channelFlowLengths);
                    dsFlowLength = channelFlowLengths[dsLink];
                }
                flowLength = dsFlowLength + length;
            } else {
                // outlet channel for subbasin
                flowLength = length;
            }
            channelFlowLengths[chLink] = flowLength;
            var maxFlowLength = this.maxFlowLengths.get(subbasin, 0);
            if (flowLength > maxFlowLength) {
                this.maxFlowLengths[subbasin] = flowLength;
            }
        }
        
        // Write the gis_points table in the project database.
        public virtual object writePointsTable(object demLayer, List<int> mergees, bool useGridModel, bool existingWshed) {
            Debug.Assert(this.db is DBUtils);
            using (var conn = this.db.conn) {
                if (!conn) {
                    return;
                }
                curs = conn.cursor();
                table = "gis_points";
                clearSQL = "DROP TABLE IF EXISTS " + table;
                curs.execute(clearSQL);
                curs.execute(this.db._POINTSCREATESQL);
                waterAdded = new List<object>();
                // Add outlets from streams
                foreach (var (subbasin, (pointId, pt, _)) in this.outlets.items()) {
                    if (this.upstreamFromInlets.Contains(subbasin)) {
                        continue;
                    }
                    elev = QSWATTopology.valueAtPoint(pt, demLayer);
                    if (elev is null) {
                        elev = 0;
                    }
                    this.addPoint(curs, subbasin, pointId, pt, elev, "O");
                }
                // Add inlets
                if (useGridModel) {
                    foreach (var (chLink, (pointId, pt)) in this.chLinkToInlet.items()) {
                        if (this.chLinkInsideLake.Contains(chLink) || this.chLinkFromLake.Contains(chLink)) {
                            // shouldn't happen
                            continue;
                        }
                        subbasin = this.chLinkToChBasin[chLink];
                        elev = QSWATTopology.valueAtPoint(pt, demLayer);
                        if (elev is null) {
                            elev = 0;
                        }
                        this.addPoint(curs, subbasin, pointId, pt, elev, "I");
                    }
                } else {
                    foreach (var (subbasin, (pointId, pt)) in this.inlets.items()) {
                        if (this.upstreamFromInlets.Contains(subbasin)) {
                            // shouldn't happen, but users can be stupid
                            continue;
                        }
                        elev = QSWATTopology.valueAtPoint(pt, demLayer);
                        if (elev is null) {
                            elev = 0;
                        }
                        this.addPoint(curs, subbasin, pointId, pt, elev, "I");
                    }
                }
                // Add point sources at heads of channels
                foreach (var (chLink, (pointId, pt)) in this.chLinkToPtSrc.items()) {
                    if (this.chLinkInsideLake.Contains(chLink)) {
                        continue;
                    }
                    if (useGridModel) {
                        if (this.chLinkFromLake.Contains(chLink)) {
                            continue;
                        }
                        subbasin = this.chLinkToChBasin[chLink];
                    } else {
                        chBasin = this.chLinkToChBasin.get(chLink, -1);
                        subbasin = this.chBasinToSubbasin.get(chBasin, -1);
                        if (subbasin < 0 || this.upstreamFromInlets.Contains(subbasin)) {
                            continue;
                        }
                    }
                    elev = QSWATTopology.valueAtPoint(pt, demLayer);
                    if (elev is null) {
                        elev = 0;
                    }
                    this.addPoint(curs, subbasin, pointId, pt, elev, "P");
                }
                foreach (var (chLink, (pointId, pt)) in this.chPointSources.items()) {
                    if (this.chLinkToPtSrc.Contains(chLink) || mergees.Contains(chLink) || this.chLinkInsideLake.Contains(chLink)) {
                        continue;
                    }
                    if (useGridModel) {
                        if (this.chLinkFromLake.Contains(chLink)) {
                            continue;
                        }
                        subbasin = this.chLinkToChBasin[chLink];
                    } else {
                        chBasin = this.chLinkToChBasin.get(chLink, -1);
                        subbasin = this.chBasinToSubbasin.get(chBasin, -1);
                        if (subbasin < 0 || this.upstreamFromInlets.Contains(subbasin)) {
                            continue;
                        }
                    }
                    elev = QSWATTopology.valueAtPoint(pt, demLayer);
                    if (elev is null) {
                        elev = 0;
                    }
                    this.addPoint(curs, subbasin, pointId, pt, elev, "P");
                }
                // Add lakes
                foreach (var lake in this.lakesData.values()) {
                    if (new HashSet {
                        QSWATTopology._RESTYPE,
                        QSWATTopology._PONDTYPE,
                        QSWATTopology._WETLANDTYPE
                    }.Contains(lake.waterRole)) {
                        // outlet from lake
                        (subbasin, pointId, pt, elev) = lake.outPoint;
                        Debug.Assert(elev is not null);
                        chLink = lake.outChLink;
                        // unnecessary as will be in self.outlets
                        //if not useGridModel and chLink == -1:
                        //    # main outlet was moved inside lake, but reservoir point will still be routed to it
                        //    # so add its definition
                        //    (outletId, outPt, _) = self.outlets[subbasin]
                        //    self.addPoint(curs, subbasin, outletId, outPt, elev, 'O')
                        this.addPoint(curs, subbasin, pointId, pt, elev, "W");
                        waterAdded.append(pointId);
                        // inlets to lake.  These are outlets from streams in grid models, so not necessary
                        if (!useGridModel) {
                            foreach (var (chLink, (pointId, pt, elev)) in lake.inChLinks.items()) {
                                chBasin = this.chLinkToChBasin[chLink];
                                subbasin = this.chBasinToSubbasin[chBasin];
                                Debug.Assert(elev is not null);
                                this.addPoint(curs, subbasin, pointId, pt, elev, "O");
                            }
                        }
                    }
                }
                foreach (var (chLink, (pointId, pt, _)) in this.chLinkToWater.items()) {
                    // reservoir points at lake outlets can appear here 
                    // but already added from lakesdata
                    if (waterAdded.Contains(pointId)) {
                        continue;
                    }
                    if (useGridModel) {
                        subbasin = this.chLinkToChBasin[chLink];
                    } else {
                        chBasin = this.chLinkToChBasin.get(chLink, -1);
                        subbasin = this.chBasinToSubbasin.get(chBasin, -1);
                        if (this.upstreamFromInlets.Contains(subbasin)) {
                            continue;
                        }
                    }
                    elev = QSWATTopology.valueAtPoint(pt, demLayer);
                    if (elev is null) {
                        elev = 0;
                    }
                    this.addPoint(curs, subbasin, pointId, pt, elev, "W");
                }
                foreach (var (channel, (_, pointId, pt)) in this.foundReservoirs.items()) {
                    if (useGridModel) {
                        subbasin = this.chLinkToChBasin[channel];
                    } else {
                        chBasin = this.chLinkToChBasin.get(channel, -1);
                        subbasin = this.chBasinToSubbasin.get(chBasin, -1);
                        if (this.upstreamFromInlets.Contains(subbasin)) {
                            continue;
                        }
                    }
                    elev = QSWATTopology.valueAtPoint(pt, demLayer);
                    if (elev is null) {
                        elev = 0;
                    }
                    this.addPoint(curs, subbasin, pointId, pt, elev, "W");
                    //             for subbasin, (pointId, pt) in self.extraReservoirs.iteritems():
                    //                 if subbasin in self.upstreamFromInlets: 
                    //                 # shouldn't happen, but users can be stupid
                    //                     continue
                    //                 elev = QSWATTopology.valueAtPoint(pt, demLayer)
                    //                 self.addPoint(curs, subbasin, pointId, pt, elev, 'R')
                }
                conn.commit();
            }
        }
        
        // Add extra points needed to mark where channels drain into reservoirs.
        public virtual object addExtraPointsToPointsTable(List<object> extraPoints, bool useGridModel) {
            Debug.Assert(this.db is DBUtils);
            using (var conn = this.db.conn) {
                if (!conn) {
                    return;
                }
                curs = conn.cursor();
                foreach (var (channel, pointId) in extraPoints) {
                    if (useGridModel) {
                        subbasin = this.chLinkToChBasin[channel];
                    } else {
                        chBasin = this.chLinkToChBasin[channel];
                        subbasin = this.chBasinToSubbasin[chBasin];
                    }
                    data = this.channelsData[channel];
                    pt = QgsPointXY(data.lowerX, data.lowerY);
                    this.addPoint(curs, subbasin, pointId, pt, data.lowerZ, "O");
                }
                conn.commit();
            }
        }
        
        // Add point to gis_points table.
        public virtual object addPoint(
            object cursor,
            int subbasin,
            int pointId,
            object pt,
            double elev,
            string typ) {
            var table = "gis_points";
            var SWATBasin = this.subbasinToSWATBasin.get(subbasin, 0);
            // include points without SWAT basin since points within lakes may not have one
            //if SWATBasin == 0:
            //    return
            var ptll = this.pointToLatLong(pt);
            var sql = "INSERT INTO " + table + " VALUES(?,?,?,?,?,?,?,?)";
            try {
                cursor.execute(sql, (pointId, SWATBasin, typ, pt.x(), pt.y(), ptll.y(), ptll.x(), elev));
                Debug.Assert(this.db is DBUtils);
                this.db.addKey(table, pointId);
            } catch {
                QSWATUtils.exceptionError("Internal error: unable to add point {0} type {1}".format(pointId, typ), this.isBatch);
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
        //     SWATBasin = self.subbasinToSWATBasin.get(data.ru, 0)
        //     if SWATBasin == 0:
        //         return
        //     lsu = 0
        //     if typ == 'I': # inlet
        //         pt = QgsPointXY(data.upperX, data.upperY)
        //         elev = data.upperZ
        //         drainId = SWATBasin 
        //         drainCat = 'R'
        //     else:
        //         pt = QgsPointXY(data.lowerX, data.lowerY)
        //         elev = data.lowerZ
        //         if typ == 'P': # point source
        //             resId = self.linkToReservoir.get(link, 0)
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
        //                 dsSWATBasin = self.subbasinToSWATBasin.get(dsBasin, 0)
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
        //                    pt.x(), pt.y(), ptll.y(), ptll.x(), elev))
        //===========================================================================
        // 
        //         Write the channels table in the project database, make rivs1.shp in shapes directory, and copy as results template to TablesOut directory.
        //         
        //         Changes the channel layer, so if successful, returns the new one.
        //         
        public virtual object writeChannelsTable(object mergeChannels, object basins, object gv) {
            object ft;
            object downChannel;
            object subbasin;
            object channel;
            object rid;
            object OK;
            object strng;
            object channelFile;
            var root = QgsProject.instance().layerTreeRoot();
            if (gv.useGridModel) {
                // use streams as channels
                channelFile = gv.streamFile;
                strng = "streams";
            } else {
                channelFile = gv.channelFile;
                strng = "channel";
            }
            if (!os.path.exists(channelFile)) {
                QSWATUtils.error("Cannot find {0} file {1}".format(strng, channelFile), gv.isBatch);
                return null;
            }
            var channelLayer = QSWATUtils.getLayerByFilename(root.findLayers(), channelFile, FileTypes._CHANNELS, null, null, null)[0];
            if (channelLayer is null) {
                // perhaps removed by user
                channelLayer = QgsVectorLayer(channelFile, "Channels", "ogr");
            }
            QSWATUtils.copyShapefile(channelFile, Parameters._RIVS1, gv.shapesDir);
            var rivs1File = QSWATUtils.join(gv.shapesDir, Parameters._RIVS1 + ".shp");
            QSWATUtils.removeLayer(rivs1File, root);
            var rivs1Layer = QgsVectorLayer(rivs1File, "Channels ({0})".format(Parameters._RIVS1), "ogr");
            var provider1 = rivs1Layer.dataProvider();
            // add Channel, ChannelR, and Subbasin fields unless already has them
            var chIdx = this.getIndex(rivs1Layer, QSWATTopology._CHANNEL, ignoreMissing: true);
            var chRIdx = this.getIndex(rivs1Layer, QSWATTopology._CHANNELR, ignoreMissing: true);
            var subIdx = this.getIndex(rivs1Layer, QSWATTopology._SUBBASIN, ignoreMissing: true);
            if (chIdx < 0) {
                OK = provider1.addAttributes(new List<object> {
                    QgsField(QSWATTopology._CHANNEL, QVariant.Int)
                });
                if (!OK) {
                    QSWATUtils.error("Cannot add {0} field to channels shapefile {1}".format(QSWATTopology._CHANNEL, rivs1File), this.isBatch);
                    return null;
                }
            }
            if (chRIdx < 0) {
                OK = provider1.addAttributes(new List<object> {
                    QgsField(QSWATTopology._CHANNELR, QVariant.Int)
                });
                if (!OK) {
                    QSWATUtils.error("Cannot add {0} field to channels shapefile {1}".format(QSWATTopology._CHANNELR, rivs1File), this.isBatch);
                    return null;
                }
            }
            if (subIdx < 0) {
                OK = provider1.addAttributes(new List<object> {
                    QgsField(QSWATTopology._SUBBASIN, QVariant.Int)
                });
                if (!OK) {
                    QSWATUtils.error("Cannot add {0} field to channels shapefile {1}".format(QSWATTopology._SUBBASIN, rivs1File), this.isBatch);
                    return null;
                }
            }
            rivs1Layer.updateFields();
            chIdx = this.getIndex(rivs1Layer, QSWATTopology._CHANNEL);
            chRIdx = this.getIndex(rivs1Layer, QSWATTopology._CHANNELR);
            subIdx = this.getIndex(rivs1Layer, QSWATTopology._SUBBASIN);
            var chLinkIdx = this.getIndex(rivs1Layer, QSWATTopology._LINKNO);
            var request = QgsFeatureRequest().setSubsetOfAttributes(new List<int> {
                chLinkIdx
            });
            if (!gv.useGridModel) {
                var basinMerge = this.mergeChannelData(mergeChannels);
                // make map channel -> feature it is merged with for merged channels
                var merges = new dict();
                var targets = new List<object>();
                foreach (var reach in provider1.getFeatures(request)) {
                    foreach (var channel in mergeChannels.keys()) {
                        var target = this.finalTarget(channel, mergeChannels);
                        if (!targets.Contains(target)) {
                            targets.append(target);
                        }
                        if (reach[chLinkIdx] == target) {
                            merges[channel] = reach;
                            //QSWATUtils.loginfo('Channel {0} merging to target {1} with length {2}'.format(channel, target, reach.geometry().length()))
                            // create geometries for merged reaches
                        }
                    }
                }
                var merged = new List<object>();
                foreach (var reach in provider1.getFeatures(request)) {
                    rid = reach.id();
                    channel = reach[chLinkIdx];
                    if (targets.Contains(channel) && !merged.Contains(rid)) {
                        merged.append(rid);
                    }
                    var mergeReach = merges.get(channel, null);
                    if (mergeReach is not null) {
                        // add its geometry to its merger target
                        //length1 = mergeReach.geometry().length()
                        //length2 = reach.geometry().length()
                        mergeReach.setGeometry(mergeReach.geometry().combine(reach.geometry()));
                        //length3 = mergeReach.geometry().length()
                        //QSWATUtils.loginfo('Channel {0} merged to target with length {1} ({2} + {3})' \
                        //                   .format(channel, length3, length1, length2))
                        if (!merged.Contains(rid)) {
                            merged.append(rid);
                        }
                    }
                }
                // remove channels and targets involved in mergers
                provider1.deleteFeatures(merged);
                // add mergers
                var mergers = new List<object>();
                foreach (var (channel, reach) in merges.items()) {
                    if (!mergers.Contains(reach)) {
                        mergers.append(reach);
                    }
                }
                provider1.addFeatures(mergers);
            }
            var chsMap = new dict();
            var zeroRids = new List<object>();
            foreach (var reach in provider1.getFeatures(request)) {
                channel = reach[chLinkIdx];
                if (gv.useGridModel) {
                    // subbasin and chBasin are the same
                    subbasin = this.chLinkToChBasin.get(channel, -1);
                    downChannel = this.downChannels[channel];
                } else {
                    var chBasin = this.chLinkToChBasin.get(channel, -1);
                    subbasin = this.chBasinToSubbasin.get(chBasin, -1);
                    downChannel = this.finalDownstream(channel, mergeChannels);
                }
                var SWATBasin = this.subbasinToSWATBasin.get(subbasin, 0);
                var SWATChannel = SWATBasin == 0 ? 0 : this.channelToSWATChannel.get(channel, 0);
                var downSWATChannel = this.channelToSWATChannel.get(downChannel, 0);
                rid = reach.id();
                if (SWATChannel == 0) {
                    zeroRids.append(rid);
                }
                chsMap[rid] = new dict();
                chsMap[rid][chIdx] = SWATChannel;
                chsMap[rid][chRIdx] = downSWATChannel;
                chsMap[rid][subIdx] = SWATBasin;
            }
            OK = provider1.changeAttributeValues(chsMap);
            if (!OK) {
                QSWATUtils.error("Cannot add channel and subbasin values to channels shapefile {0}".format(rivs1File), this.isBatch);
                return null;
            }
            if (zeroRids.Count > 0) {
                OK = provider1.deleteFeatures(zeroRids);
                if (!OK) {
                    QSWATUtils.error("Cannot remove merged, zero length, or above inlet channels from channels shapefile {0}".format(rivs1File), this.isBatch);
                    return null;
                }
            }
            // Add fields from channels table to rivs1File if less than RIV1SUBS1MAX features; otherwise takes too long.
            var addToRiv1 = rivs1Layer.featureCount() <= Parameters._RIVS1SUBS1MAX;
            // remove fields apart from Channel, ChannelR and Subbasin
            if (addToRiv1) {
                this.removeFields(rivs1Layer, new List<object> {
                    QSWATTopology._LINKNO,
                    QSWATTopology._CHANNEL,
                    QSWATTopology._CHANNELR,
                    QSWATTopology._SUBBASIN
                }, rivs1File, this.isBatch);
            }
            if (addToRiv1) {
                var fields = new List<object>();
                fields.append(QgsField(QSWATTopology._AREAC, QVariant.Double, len: 20, prec: 0));
                fields.append(QgsField(QSWATTopology._ORDER, QVariant.Int));
                fields.append(QgsField(QSWATTopology._LEN2, QVariant.Double));
                fields.append(QgsField(QSWATTopology._SLO2, QVariant.Double));
                fields.append(QgsField(QSWATTopology._WID2, QVariant.Double));
                fields.append(QgsField(QSWATTopology._DEP2, QVariant.Double));
                fields.append(QgsField(QSWATTopology._MINEL, QVariant.Double));
                fields.append(QgsField(QSWATTopology._MAXEL, QVariant.Double));
                fields.append(QgsField(QSWATTopology._RESERVOIR, QVariant.Int));
                fields.append(QgsField(QSWATTopology._POND, QVariant.Int));
                fields.append(QgsField(QSWATTopology._LAKEIN, QVariant.Int));
                fields.append(QgsField(QSWATTopology._LAKEOUT, QVariant.Int));
                provider1.addAttributes(fields);
                rivs1Layer.updateFields();
                var linkIdx = this.getIndex(rivs1Layer, QSWATTopology._LINKNO);
                chIdx = this.getIndex(rivs1Layer, QSWATTopology._CHANNEL);
                var areaCIdx = this.getIndex(rivs1Layer, QSWATTopology._AREAC);
                var orderIdx = this.getIndex(rivs1Layer, QSWATTopology._ORDER);
                var len2Idx = this.getIndex(rivs1Layer, QSWATTopology._LEN2);
                var slo2Idx = this.getIndex(rivs1Layer, QSWATTopology._SLO2);
                var wid2Idx = this.getIndex(rivs1Layer, QSWATTopology._WID2);
                var dep2Idx = this.getIndex(rivs1Layer, QSWATTopology._DEP2);
                var minElIdx = this.getIndex(rivs1Layer, QSWATTopology._MINEL);
                var maxElIdx = this.getIndex(rivs1Layer, QSWATTopology._MAXEL);
                var resIdx = this.getIndex(rivs1Layer, QSWATTopology._RESERVOIR);
                var pndIdx = this.getIndex(rivs1Layer, QSWATTopology._POND);
                var lakeInIdx = this.getIndex(rivs1Layer, QSWATTopology._LAKEIN);
                var lakeOutIdx = this.getIndex(rivs1Layer, QSWATTopology._LAKEOUT);
                var mmap = new dict();
            }
            Debug.Assert(this.db is DBUtils);
            using (var conn = this.db.conn) {
                if (!conn) {
                    return null;
                }
                curs = conn.cursor();
                table = "gis_channels";
                clearSQL = "DROP TABLE IF EXISTS " + table;
                curs.execute(clearSQL);
                curs.execute(this.db._CHANNELSCREATESQL);
                time1 = time.process_time();
                wid2Data = new dict();
                floodscape = gv.useLandscapes ? QSWATUtils._FLOODPLAIN : QSWATUtils._NOLANDSCAPE;
                sql = "INSERT INTO " + table + " VALUES(?,?,?,?,?,?,?,?,?,?,?,?)";
                if (addToRiv1) {
                    // iterate over channels in rivs1 shapefile
                    request = QgsFeatureRequest().setFlags(QgsFeatureRequest.NoGeometry).setSubsetOfAttributes(new List<int> {
                        linkIdx,
                        chIdx
                    });
                    generator = this.generateChannelsFromShapefile(request, provider1, linkIdx, chIdx);
                } else {
                    generator = this.generateChannelsFromTable();
                }
                toDelete = new List<object>();
                foreach (var (fid, channel, SWATChannel) in generator) {
                    if (gv.useGridModel) {
                        // basin and chBasin are the same
                        subbasin = this.chLinkToChBasin[channel];
                    } else {
                        chBasin = this.chLinkToChBasin.get(channel, -1);
                        subbasin = this.chBasinToSubbasin.get(chBasin, -1);
                    }
                    SWATBasin = this.chLinkInsideLake.Contains(channel) ? 0 : this.subbasinToSWATBasin.get(subbasin, 0);
                    lakeOutId = this.chLinkFromLake.get(channel, 0);
                    if (SWATBasin == 0 && (lakeOutId == 0 || this.downChannels.get(channel, -1) < 0)) {
                        toDelete.append(fid);
                        continue;
                    }
                    if (gv.useGridModel) {
                        channelData = this.channelsData[channel];
                        // drain area is a numpy float, so need to coerce, or won't get written to attributes of rivs1
                        drainAreaHa = float(this.drainAreas[channel]) / 10000.0;
                        order = Convert.ToInt32(this.strahler[channel]);
                        length = float(this.channelLengths[channel] * gv.mainLengthMultiplier);
                        slopePercent = float(this.channelSlopes[channel] * 100 * gv.reachSlopeMultiplier / gv.mainLengthMultiplier);
                        minEl = float(channelData.lowerZ);
                        maxEl = float(channelData.upperZ);
                    } else {
                        mergeData = basinMerge.get(channel, null);
                        if (mergeData is null) {
                            continue;
                        }
                        drainAreaHa = float(mergeData.areaC / 10000.0);
                        order = Convert.ToInt32(mergeData.order);
                        length = float(mergeData.length * gv.mainLengthMultiplier);
                        slopePercent = float(mergeData.slope * 100 * gv.reachSlopeMultiplier) / gv.mainLengthMultiplier;
                        minEl = float(mergeData.minEl);
                        maxEl = float(mergeData.maxEl);
                    }
                    // possible for channel to be so short it has no pixels draining to it
                    // also no LSU data when channel is outlet from lake in grid model
                    basinData = basins.get(subbasin, null);
                    channelData = basinData is null ? null : basinData.getLsus().get(channel, null);
                    lsuData = channelData is null ? null : channelData.get(floodscape, null);
                    drainAreaKm = float(drainAreaHa) / 100;
                    channelWidth = float(gv.channelWidthMultiplier * Math.Pow(drainAreaKm, gv.channelWidthExponent));
                    wid2Data[SWATChannel] = channelWidth;
                    channelDepth = float(gv.channelDepthMultiplier * Math.Pow(drainAreaKm, gv.channelDepthExponent));
                    if (lsuData is null) {
                        rid = 0;
                        pid = 0;
                        channelData = this.channelsData[channel];
                        midPointX = (channelData.lowerX + channelData.upperX) / 2;
                        midPointY = (channelData.lowerY + channelData.upperY) / 2;
                    } else {
                        rid = this.getReservoirId(lsuData);
                        pid = this.getPondId(lsuData);
                        midPointX = lsuData.midPointX;
                        midPointY = lsuData.midPointY;
                    }
                    midll = this.pointToLatLong(QgsPointXY(midPointX, midPointY));
                    midLat = midll.y();
                    midLong = midll.x();
                    if (rid == 0 && pid == 0) {
                        // omit from gis_channels channels which have become reservoirs or ponds
                        curs.execute(sql, (SWATChannel, SWATBasin, drainAreaHa, order, length, slopePercent, channelWidth, channelDepth, minEl, maxEl, midLat, midLong));
                        this.db.addKey(table, SWATChannel);
                    }
                    if (addToRiv1) {
                        lakeInId = this.chLinkIntoLake.get(channel, 0);
                        mmap[fid] = new dict();
                        mmap[fid][areaCIdx] = drainAreaHa;
                        mmap[fid][orderIdx] = order;
                        mmap[fid][len2Idx] = length;
                        mmap[fid][slo2Idx] = slopePercent;
                        mmap[fid][wid2Idx] = channelWidth;
                        mmap[fid][dep2Idx] = channelDepth;
                        mmap[fid][minElIdx] = minEl;
                        mmap[fid][maxElIdx] = maxEl;
                        mmap[fid][resIdx] = rid;
                        mmap[fid][pndIdx] = pid;
                        mmap[fid][lakeInIdx] = lakeInId;
                        mmap[fid][lakeOutIdx] = lakeOutId;
                    }
                }
                time2 = time.process_time();
                QSWATUtils.loginfo("Writing gis_channels table took {0} seconds".format(Convert.ToInt32(time2 - time1)));
                conn.commit();
                this.db.hashDbTable(conn, table);
            }
            if (addToRiv1) {
                if (!provider1.changeAttributeValues(mmap)) {
                    QSWATUtils.error("Cannot edit values in channels shapefile {0}".format(rivs1File), this.isBatch);
                    return null;
                }
            }
            if (toDelete.Count > 0) {
                OK = provider1.deleteFeatures(toDelete);
                if (!OK) {
                    QSWATUtils.error("Cannot remove channels in lakes from channels shapefile {0}".format(rivs1File), this.isBatch);
                    return null;
                }
            }
            // make copy as template for stream results
            QSWATUtils.copyShapefile(rivs1File, Parameters._RIVS, gv.resultsDir);
            var rivFile = QSWATUtils.join(gv.resultsDir, Parameters._RIVS + ".shp");
            var rivJson = QSWATUtils.join(gv.resultsDir, Parameters._RIVS + ".json");
            var rivLayer = QgsVectorLayer(rivFile, "Channels", "ogr");
            var provider = rivLayer.dataProvider();
            var exporter = QgsJsonExporter(rivLayer);
            QSWATUtils.removeLayer(rivJson, root);
            using (var jsonFile = open(rivJson, "w")) {
                jsonFile.write(exporter.exportFeatures(provider.getFeatures()));
            }
            // remove channels that are reservoirs or ponds
            var exp = QgsExpression("\"{0}\" > 0 OR \"{1}\" > 0".format(QSWATTopology._RESERVOIR, QSWATTopology._POND));
            var idsToDelete = new List<object>();
            foreach (var feature in provider.getFeatures(QgsFeatureRequest(exp).setFlags(QgsFeatureRequest.NoGeometry))) {
                idsToDelete.append(feature.id());
            }
            OK = provider.deleteFeatures(idsToDelete);
            if (!OK) {
                QSWATUtils.error("Cannot edit streams results template {0}.".format(rivFile), this.isBatch);
                return null;
            }
            // leave only the Channel, ChannelR and Subbasin attributes
            this.removeFields(rivLayer, new List<object> {
                QSWATTopology._CHANNEL,
                QSWATTopology._CHANNELR,
                QSWATTopology._SUBBASIN
            }, rivFile, this.isBatch);
            // add PenWidth field to stream results template
            OK = provider.addAttributes(new List<object> {
                QgsField(QSWATTopology._PENWIDTH, QVariant.Double)
            });
            if (!OK) {
                QSWATUtils.error("Cannot add {0} field to streams results template {1}".format(QSWATTopology._PENWIDTH, rivFile), this.isBatch);
                return null;
            }
            this.setPenWidth(wid2Data, 1.0, 4.0, provider);
            if (gv.useGridModel) {
                provider1.addAttributes(new List<object> {
                    QgsField(QSWATTopology._PENWIDTH, QVariant.Double)
                });
                this.setPenWidth(wid2Data, 0.2, 2.0, provider1);
            }
            var layers = root.findLayers();
            var subLayer = gv.useGridModel ? QSWATUtils.getLayerByLegend(QSWATUtils._GRIDLEGEND, layers) : root.findLayer(channelLayer.id());
            if (gv.useGridModel) {
                if (gv.existingWshed) {
                    ft = FileTypes._DRAINSTREAMS;
                } else {
                    ft = FileTypes._GRIDSTREAMS;
                }
            } else {
                ft = FileTypes._CHANNELREACHES;
            }
            rivs1Layer = QSWATUtils.getLayerByFilename(layers, rivs1File, ft, gv, subLayer, QSWATUtils._WATERSHED_GROUP_NAME)[0];
            if (gv.useGridModel) {
                FileTypes.colourStreams(rivs1Layer, QSWATTopology._PENWIDTH, QSWATTopology._AREAC);
            } else {
                // hide channel layer
                if (channelLayer is not null) {
                    QSWATUtils.setLayerVisibility(channelLayer, false, root);
                }
                if (this.upstreamFromInlets.Count > 0) {
                    this.replaceStreamLayer(root, layers, gv);
                }
            }
            return rivs1Layer;
        }
        
        // Yield (feature id, channel, swatChammel) tupless from rivs1.shp.
        public virtual object generateChannelsFromShapefile(object request, object provider, int linkIdx, int chIdx) {
            foreach (var feature in provider.getFeatures(request)) {
                yield return (feature.id(), feature[linkIdx], feature[chIdx]);
            }
        }
        
        // Yield (feature id, channel, swatChammel) tuples from tables.
        public virtual object generateChannelsFromTable() {
            foreach (var (channel, SWATChannel) in this.channelToSWATChannel.items()) {
                yield return (0, channel, SWATChannel);
            }
        }
        
        // Copy stream layer, remove streams upstream from inlets, and replace stream layer.
        public virtual object replaceStreamLayer(object root, List<object> layers, object gv) {
            var streamLayer = QSWATUtils.getLayerByFilename(layers, gv.streamFile, FileTypes._STREAMREACHES, gv, null, null)[0];
            if (streamLayer is not null) {
                (@base, _) = os.path.splitext(os.path.split(gv.streamFile)[1]);
                QSWATUtils.copyShapefile(gv.streamFile, @base + "act", gv.shapesDir);
                var actStreamFile = QSWATUtils.join(gv.shapesDir, @base + "act.shp");
                var actstreamLayer = QgsVectorLayer(actStreamFile, FileTypes.legend(FileTypes._STREAMREACHES), "ogr");
                var basinIdx = this.getIndex(actstreamLayer, QSWATTopology._WSNO);
                if (basinIdx < 0) {
                    return;
                }
                var provider = actstreamLayer.dataProvider();
                var request = QgsFeatureRequest().setFlags(QgsFeatureRequest.NoGeometry).setSubsetOfAttributes(new List<int> {
                    basinIdx
                });
                var toDelete = new List<object>();
                foreach (var feature in provider.getFeatures(request)) {
                    if (this.upstreamFromInlets.Contains(feature[basinIdx])) {
                        toDelete.append(feature.id());
                    }
                }
                if (provider.deleteFeatures(toDelete)) {
                    var subLayer = root.findLayer(streamLayer.id());
                    actstreamLayer = QSWATUtils.getLayerByFilename(layers, actStreamFile, FileTypes._STREAMREACHES, gv, subLayer, QSWATUtils._WATERSHED_GROUP_NAME, true)[0];
                    QSWATUtils.setLayerVisibility(streamLayer, false, root);
                }
            }
        }
        
        // Return reservoir id, if any, else 0.
        public virtual int getReservoirId(object lsuData) {
            if (lsuData.waterBody is not null && lsuData.waterBody.isReservoir()) {
                return cast(@int, lsuData.waterBody.id);
            }
            return 0;
        }
        
        // Return pond id, if any, else 0.
        public virtual int getPondId(object lsuData) {
            if (lsuData.waterBody is not null && lsuData.waterBody.isPond()) {
                return cast(@int, lsuData.waterBody.id);
            }
            return 0;
        }
        
        // Generate and return map of channel to MergedChannelData.
        public virtual object mergeChannelData(object mergeChannels) {
            object channelData;
            // first pass: collect data for unmerged channels
            var mergedChannelData = new dict();
            foreach (var channel in this.channelToSWATChannel.keys()) {
                if (!mergeChannels.Contains(channel)) {
                    channelData = this.channelsData[channel];
                    mergedChannelData[channel] = MergedChannelData(this.drainAreas[channel], this.strahler[channel], this.channelLengths[channel], this.channelSlopes[channel], channelData.lowerZ, channelData.upperZ);
                }
            }
            // second pass: add data for merged channels
            foreach (var (source, target) in mergeChannels.items()) {
                channelData = this.channelsData[source];
                var final = this.finalTarget(target, mergeChannels);
                mergedChannelData[final].add(this.drainAreas[source], this.strahler[source], this.channelLengths[source], this.channelSlopes[source], channelData.lowerZ, channelData.upperZ);
            }
            return mergedChannelData;
        }
        
        // Find final target of merges.
        public virtual int finalTarget(int target, object mergeChannels) {
            var nxt = mergeChannels.get(target, -1);
            if (nxt < 0) {
                return target;
            } else {
                return this.finalTarget(nxt, mergeChannels);
            }
        }
        
        // Find downstream channel from start, skipping merged channels, and return it.
        public virtual int finalDownstream(int start, object mergeChannels) {
            var chLink1 = this.finalTarget(start, mergeChannels);
            return this.finalTarget(this.getDownChannel(chLink1), mergeChannels);
        }
        
        // Add channels, lakes, basins, aquifers, point sources, reservoirs, inlets and outlets to main gis_routing table.
        public virtual bool routeChannelsOutletsAndBasins(
            object basins,
            object mergedChannels,
            List<int> mergees,
            List<object> extraPoints,
            object gv) {
            object channelToPtSrc;
            object channelToInlet;
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
                channelToInlet = new dict();
                foreach (var (subbasin, inlet) in this.inlets.items()) {
                    // find an inlet channel for this subbasin
                    var found = false;
                    foreach (var (channel, data) in this.channelsData.items()) {
                        var chBasin = this.chLinkToChBasin.get(channel, -1);
                        if (subbasin == this.chBasinToSubbasin.get(chBasin, -1) && QSWATTopology.coincidentPoints(QgsPointXY(data.upperX, data.upperY), inlet[1], this.xThreshold, this.yThreshold)) {
                            channelToInlet[this.finalTarget(channel, mergedChannels)] = inlet;
                            found = true;
                            break;
                        }
                    }
                    if (!found) {
                        QSWATUtils.error("Failed to find channel for inlet to subbasin {0}".format(subbasin), gv.isBatch);
                    }
                }
                // map point sources to the unmerged channels they drain into
                channelToPtSrc = new dict();
                foreach (var (channel, ptsrcPair) in this.chLinkToPtSrc.items()) {
                    channelToPtSrc[channel] = ptsrcPair;
                    //QSWATUtils.loginfo('Channel {0} merged to {1} has point source {2}'.format(channel, self.finalTarget(channel, mergedChannels), ptsrc[0]))
                    // add point sources at stream sources
                }
            }
            foreach (var (channel, ptsrcPair) in this.chPointSources.items()) {
                if (!channelToPtSrc.Contains(channel) && !mergees.Contains(channel) && !this.chLinkInsideLake.Contains(channel) && !(gv.useGridModel && this.chLinkFromLake.Contains(channel))) {
                    // not already has a point, not merged, not inslide lake 
                    channelToPtSrc[channel] = ptsrcPair;
                }
            }
            // map channels to water bodies that replace them as drainage targets
            // and water bodies to channels they drain into
            var floodscape = gv.useLandscapes ? QSWATUtils._FLOODPLAIN : QSWATUtils._NOLANDSCAPE;
            var channelToWater = new dict();
            foreach (var basinData in basins.values()) {
                foreach (var (channel, channelData) in basinData.getLsus().items()) {
                    var lsuData = channelData.get(floodscape, null);
                    if (lsuData is not null && lsuData.waterBody is not null && !lsuData.waterBody.isUnknown()) {
                        channelToWater[channel] = (lsuData.waterBody.id, lsuData.waterBody.waterRole);
                    }
                }
            }
            try {
                Debug.Assert(this.db is DBUtils);
                using (var conn = this.db.conn) {
                    curs = conn.cursor();
                    routedPoints = new List<object>();
                    routedWater = new List<object>();
                    routedChannels = new List<object>();
                    //                 routedSubbasins = []
                    foreach (var (channel, SWATChannel) in this.channelToSWATChannel.items()) {
                        if (mergedChannels.Contains(channel)) {
                            // all that is needed is to map its point source to the merge target
                            (ptsrcId, ptsrc) = channelToPtSrc.get(channel, (-1, null));
                            if (ptsrc is not null) {
                                if (!routedPoints.Contains(ptsrcId)) {
                                    finalChannel = this.finalTarget(channel, mergedChannels);
                                    (wid, role) = channelToWater.get(finalChannel, (-1, -1));
                                    if (wid >= 0) {
                                        wCat = role == 1 ? resCat : role == 2 ? pondCat : wetlandCat;
                                        this.db.addToRouting(curs, ptsrcId, ptCat, wid, wCat, QSWATTopology._TOTAL, 100);
                                    } else {
                                        finalSWATChannel = this.channelToSWATChannel[finalChannel];
                                        this.db.addToRouting(curs, ptsrcId, ptCat, finalSWATChannel, chCat, QSWATTopology._TOTAL, 100);
                                    }
                                    routedPoints.append(ptsrcId);
                                }
                            }
                            continue;
                        }
                        // if channel is lake outflow
                        // if main outflow, route lake to outlet and outlet to channel
                        // else route 0% of lake to channel
                        outLakeId = this.chLinkFromLake.get(channel, -1);
                        if (outLakeId >= 0) {
                            lakeData = this.lakesData[outLakeId];
                            wCat = lakeData.waterRole == 1 ? resCat : lakeData.waterRole == 2 ? pondCat : wetlandCat;
                            if (channel == lakeData.outChLink) {
                                // main outlet
                                outletId = lakeData.outPoint[1];
                                this.db.addToRouting(curs, outLakeId, wCat, outletId, ptCat, QSWATTopology._TOTAL, 100);
                                if (!routedPoints.Contains(outletId)) {
                                    if (gv.useGridModel && this.downChannels.get(channel, -1) < 0) {
                                        // we have an internal lake exit: route outlet id to watershed exit
                                        this.db.addToRouting(curs, outletId, ptCat, 0, xCat, QSWATTopology._TOTAL, 100);
                                    } else {
                                        this.db.addToRouting(curs, outletId, ptCat, SWATChannel, chCat, QSWATTopology._TOTAL, 100);
                                    }
                                }
                                routedPoints.append(outletId);
                            } else {
                                // other outlet
                                this.db.addToRouting(curs, outLakeId, wCat, SWATChannel, chCat, QSWATTopology._TOTAL, 0);
                            }
                        }
                        // check if channel routes into lake
                        inLakeId = this.chLinkIntoLake.get(channel, -1);
                        if (inLakeId >= 0) {
                            // route its point source to the channel
                            (ptsrcId, ptsrc) = channelToPtSrc.get(channel, (-1, null));
                            if (ptsrc is not null) {
                                if (!routedPoints.Contains(ptsrcId)) {
                                    this.db.addToRouting(curs, ptsrcId, ptCat, SWATChannel, chCat, QSWATTopology._TOTAL, 100);
                                    routedPoints.append(ptsrcId);
                                }
                            }
                            // route the channel into its outlet, and the outlet into the lake
                            lakeData = this.lakesData[inLakeId];
                            outletId = lakeData.inChLinks[channel][0];
                            wCat = lakeData.waterRole == 1 ? resCat : lakeData.waterRole == 2 ? pondCat : wetlandCat;
                            if (!routedChannels.Contains(SWATChannel)) {
                                this.db.addToRouting(curs, SWATChannel, chCat, outletId, ptCat, QSWATTopology._TOTAL, 100);
                                routedChannels.append(SWATChannel);
                            }
                            if (!routedPoints.Contains(outletId)) {
                                this.db.addToRouting(curs, outletId, ptCat, inLakeId, wCat, QSWATTopology._TOTAL, 100);
                                routedPoints.append(outletId);
                            }
                            if (!gv.useGridModel) {
                                continue;
                            }
                        }
                        if (gv.useGridModel) {
                            subbasin = this.chLinkToChBasin[channel];
                        } else {
                            chBasin = this.chLinkToChBasin.get(channel, -1);
                            subbasin = this.chBasinToSubbasin.get(chBasin, -1);
                        }
                        SWATBasin = this.subbasinToSWATBasin.get(subbasin, 0);
                        if (SWATBasin == 0) {
                            continue;
                        }
                        // if channel is inside lake ignore it unless a lake outflow
                        if (this.chLinkInsideLake.Contains(channel) && outLakeId < 0) {
                            continue;
                        }
                        dsChannel = this.finalDownstream(channel, mergedChannels);
                        dsSWATChannel = this.channelToSWATChannel.get(dsChannel, 0);
                        (wid, role) = channelToWater.get(channel, (-1, -1));
                        wCat = role == 1 ? resCat : role == 2 ? pondCat : wetlandCat;
                        (inletId, inletPt) = channelToInlet.get(channel, (-1, null));
                        if (inletPt is not null) {
                            // route inlet to channel or water
                            if (wid >= 0) {
                                this.db.addToRouting(curs, inletId, ptCat, wid, wCat, QSWATTopology._TOTAL, 100);
                            } else {
                                this.db.addToRouting(curs, inletId, ptCat, SWATChannel, chCat, QSWATTopology._TOTAL, 100);
                            }
                        }
                        (pointId, _, outletChannels) = this.outlets[subbasin];
                        if (outletChannels.Contains(channel) || gv.useGridModel) {
                            // subbasin outlet: channel routes to outlet point of subbasin; outlet routes to downstream channel
                            // but with some exceptions:
                            // - if the channel is replaced by a reservoir, this is routed to the outlet instead
                            // - if subbasin has an extra point source, this is added to its outlet channel or reservoir
                            if (gv.useGridModel) {
                                (ptsrcId, ptsrc) = channelToPtSrc.get(channel, (-1, null));
                            } else {
                                ptsrcId = -1;
                                ptsrc = null;
                                //                         if ptsrc is None:
                                //                             ptsrc = self.extraPtSrcs.get(subbasin, None)
                            }
                            if (ptsrc is not null) {
                                // route it to the outlet channel, unless already routed
                                if (!routedPoints.Contains(ptsrcId)) {
                                    if (wid < 0) {
                                        this.db.addToRouting(curs, ptsrcId, ptCat, SWATChannel, chCat, QSWATTopology._TOTAL, 100);
                                    } else {
                                        this.db.addToRouting(curs, ptsrcId, ptCat, wid, wCat, QSWATTopology._TOTAL, 100);
                                    }
                                    routedPoints.append(ptsrcId);
                                }
                            }
                            if (wid >= 0) {
                                // need to check if this is a reservoir that is continued in the downstream subbasin
                                // to make sure we only route it once, and at its final downstream end
                                (widDown, _) = channelToWater.get(dsChannel, (-1, -1));
                                if (!routedWater.Contains(wid) && wid != widDown) {
                                    // route water to water point and water point to outlet
                                    (waterId, _, _) = this.chLinkToWater.get(channel, (-1, null, -1));
                                    if (waterId < 0) {
                                        (waterId, ptId, _) = this.foundReservoirs.get(channel, (-1, -1, null));
                                    } else {
                                        // it is safe to use same id for reservoir and reservoir outlet point when
                                        // using DSNODEID from inlets/outlets file
                                        ptId = waterId;
                                    }
                                    if (waterId < 0) {
                                        QSWATUtils.error("Cannot find water point for channel {0}".format(SWATChannel), gv.isBatch);
                                    } else {
                                        this.db.addToRouting(curs, wid, wCat, ptId, ptCat, QSWATTopology._TOTAL, 100);
                                        if (!routedPoints.Contains(ptId)) {
                                            this.db.addToRouting(curs, ptId, ptCat, pointId, ptCat, QSWATTopology._TOTAL, 100);
                                            routedPoints.append(ptId);
                                        }
                                        routedWater.append(wid);
                                    }
                                }
                            } else if (!routedChannels.Contains(SWATChannel)) {
                                this.db.addToRouting(curs, SWATChannel, chCat, pointId, ptCat, QSWATTopology._TOTAL, 100);
                                routedChannels.append(SWATChannel);
                            }
                            if (!routedPoints.Contains(pointId)) {
                                if (dsSWATChannel > 0) {
                                    (widDown, roleDown) = channelToWater.get(dsChannel, (-1, -1));
                                    if (widDown >= 0) {
                                        wCat = roleDown == 1 ? resCat : roleDown == 2 ? pondCat : wetlandCat;
                                        this.db.addToRouting(curs, pointId, ptCat, widDown, wCat, QSWATTopology._TOTAL, 100);
                                    } else {
                                        this.db.addToRouting(curs, pointId, ptCat, dsSWATChannel, chCat, QSWATTopology._TOTAL, 100);
                                    }
                                } else {
                                    // watershed outlet: mark point as category X
                                    this.db.addToRouting(curs, pointId, ptCat, 0, xCat, QSWATTopology._TOTAL, 100);
                                }
                                routedPoints.append(pointId);
                            }
                        } else {
                            // channel and downstream channel within a subbasin: 
                            // channel routes to downstream channel unless it is replaced by water
                            // and if it has a point source this is added
                            Debug.Assert(dsSWATChannel > 0);
                            Debug.Assert("Channel {0} has no downstream channel".format(channel));
                            (widDown, roleDown) = channelToWater.get(dsChannel, (-1, -1));
                            if (wid >= 0) {
                                if (!routedWater.Contains(wid)) {
                                    if (widDown >= 0) {
                                        // if wid == widDown wid is only a part water body
                                        // and we will eventually route the one downstream
                                        if (wid != widDown) {
                                            // route water to water point and water point to ridDown
                                            (waterId, _, _) = this.chLinkToWater.get(channel, (-1, null, -1));
                                            if (waterId < 0) {
                                                (waterId, ptId, _) = this.foundReservoirs.get(channel, (-1, -1, null));
                                            } else {
                                                // it is safe to use same id for reservoir and reservoir outlet point when
                                                // using DSNODEID from inlets/outlets file
                                                ptId = waterId;
                                            }
                                            if (waterId < 0) {
                                                QSWATUtils.error("Cannot find water point for channel {0}".format(SWATChannel), gv.isBatch);
                                            } else {
                                                this.db.addToRouting(curs, wid, wCat, ptId, ptCat, QSWATTopology._TOTAL, 100);
                                                if (!routedPoints.Contains(ptId)) {
                                                    wCat = roleDown == 1 ? resCat : roleDown == 2 ? pondCat : wetlandCat;
                                                    this.db.addToRouting(curs, ptId, ptCat, widDown, wCat, QSWATTopology._TOTAL, 100);
                                                    routedPoints.append(ptId);
                                                }
                                                routedWater.append(wid);
                                            }
                                        }
                                    } else {
                                        // route water to water point and water point to downstream channel
                                        (waterId, _, _) = this.chLinkToWater.get(channel, (-1, null, -1));
                                        if (waterId < 0) {
                                            (waterId, ptId, _) = this.foundReservoirs.get(channel, (-1, -1, null));
                                        } else {
                                            // it is safe to use same id for water and water outlet point when
                                            // using DSNODEID from inlets/outlets file
                                            ptId = waterId;
                                        }
                                        if (waterId < 0) {
                                            QSWATUtils.error("Cannot find water point for channel {0}".format(SWATChannel), gv.isBatch);
                                        } else {
                                            this.db.addToRouting(curs, wid, wCat, ptId, ptCat, QSWATTopology._TOTAL, 100);
                                            if (!routedPoints.Contains(ptId)) {
                                                this.db.addToRouting(curs, ptId, ptCat, dsSWATChannel, chCat, QSWATTopology._TOTAL, 100);
                                                routedPoints.append(ptId);
                                            }
                                            routedWater.append(wid);
                                        }
                                    }
                                }
                            } else if (!routedChannels.Contains(SWATChannel)) {
                                if (widDown >= 0) {
                                    // insert an outlet point so that channel's contribution to reservoir 
                                    // is included in outputs
                                    this.pointId += 1;
                                    extraPoints.append((channel, this.pointId));
                                    this.db.addToRouting(curs, SWATChannel, chCat, this.pointId, ptCat, QSWATTopology._TOTAL, 100);
                                    wCat = roleDown == 1 ? resCat : roleDown == 2 ? pondCat : wetlandCat;
                                    this.db.addToRouting(curs, this.pointId, ptCat, widDown, wCat, QSWATTopology._TOTAL, 100);
                                    routedPoints.append(this.pointId);
                                } else {
                                    this.db.addToRouting(curs, SWATChannel, chCat, dsSWATChannel, chCat, QSWATTopology._TOTAL, 100);
                                }
                                routedChannels.append(SWATChannel);
                            }
                        }
                        // also route point source, if any, to channel  or water
                        (ptsrcId, ptsrc) = channelToPtSrc.get(channel, (-1, null));
                        if (ptsrc is not null) {
                            if (!routedPoints.Contains(ptsrcId)) {
                                if (wid > 0) {
                                    this.db.addToRouting(curs, ptsrcId, ptCat, wid, wCat, QSWATTopology._TOTAL, 100);
                                } else {
                                    this.db.addToRouting(curs, ptsrcId, ptCat, SWATChannel, chCat, QSWATTopology._TOTAL, 100);
                                }
                                routedPoints.append(ptsrcId);
                            }
                        }
                    }
                    // route reservoir and pond lakes without outlet channels to main outlet points
                    foreach (var (lakeId, lakeData) in this.lakesData.items()) {
                        if (new HashSet {
                            QSWATTopology._RESTYPE,
                            QSWATTopology._PONDTYPE
                        }.Contains(lakeData.waterRole) && lakeData.outChLink == -1) {
                            (subbasin, lakeOutletId, _, _) = lakeData.outPoint;
                            (outletId, _, _) = this.outlets[subbasin];
                            wCat = lakeData.waterRole == 1 ? resCat : lakeData.waterRole == 2 ? pondCat : wetlandCat;
                            // route the lake to its lake outlet, the lake outlet to the main outlet, and mark main outlet as category X 
                            this.db.addToRouting(curs, lakeId, wCat, lakeOutletId, ptCat, QSWATTopology._TOTAL, 100);
                            if (!routedPoints.Contains(lakeOutletId)) {
                                this.db.addToRouting(curs, lakeOutletId, ptCat, outletId, ptCat, QSWATTopology._TOTAL, 100);
                                routedPoints.append(lakeOutletId);
                            }
                            if (!routedPoints.Contains(outletId)) {
                                this.db.addToRouting(curs, outletId, ptCat, 0, xCat, QSWATTopology._TOTAL, 100);
                                routedPoints.append(outletId);
                            }
                        }
                    }
                    // route subbasin to outlet points
                    // or to lake if outlet in lake
                    foreach (var (subbasin, (pointId, _, chLinks)) in this.outlets.items()) {
                        SWATBasin = this.subbasinToSWATBasin.get(subbasin, 0);
                        if (SWATBasin == 0) {
                            continue;
                        }
                        // with grid models chLinks is always a singleton list
                        if (gv.useGridModel) {
                            if (this.chLinkInsideLake.Contains(chLinks[0]) || this.chLinkFromLake.Contains(chLinks[0])) {
                                continue;
                            }
                        }
                        lakeId = this.outletsInLake.get(subbasin, -1);
                        // if one chLink is inside lake, all will be, since they share their outlet point
                        if (lakeId < 0) {
                            lakeId = this.chLinkInsideLake.get(chLinks[0], -1);
                        }
                        if (lakeId < 0) {
                            this.db.addToRouting(curs, SWATBasin, subbasinCat, pointId, ptCat, QSWATTopology._TOTAL, 100);
                        } else {
                            lakeData = this.lakesData[lakeId];
                            wCat = lakeData.waterRole == 1 ? resCat : lakeData.waterRole == 2 ? pondCat : wetlandCat;
                            this.db.addToRouting(curs, SWATBasin, subbasinCat, lakeId, wCat, QSWATTopology._TOTAL, 100);
                        }
                    }
                }
                return true;
            } catch (Exception) {
                QSWATUtils.loginfo("Routing channels, outlets and subbasins failed: {0}".format(traceback.format_exc()));
                return false;
            }
        }
        
        // Remove fields other than keepFieldNames from shapefile fileName with provider.
        [staticmethod]
        public static object removeFields(object layer, List<string> keepFieldNames, string fileName, bool isBatch) {
            var toDelete = new List<object>();
            var provider = layer.dataProvider();
            var fields = provider.fields();
            var keepLower = (from name in keepFieldNames
                select name.casefold()).ToList();
            foreach (var idx in Enumerable.Range(0, fields.count())) {
                var name = fields.field(idx).name();
                if (!keepLower.Contains(name.casefold())) {
                    toDelete.append(idx);
                }
            }
            if (toDelete.Count > 0) {
                var OK = provider.deleteAttributes(toDelete);
                layer.updateFields();
                if (!OK) {
                    QSWATUtils.error("Cannot remove fields from shapefile {0}".format(fileName), isBatch);
                }
            }
        }
        
        // Scale wid2 data to a .. b and write to layer.
        public virtual object setPenWidth(object data, double a, double b, object provider) {
            object fun;
            var @base = a;
            var mult = b - a;
            var minW = double.PositiveInfinity;
            var maxW = 0.0;
            foreach (var val in data.values()) {
                minW = min(minW, val);
                maxW = max(maxW, val);
            }
            if (maxW > minW) {
                // guard against division by zero
                var rng = maxW - minW;
                fun = x => (x - minW) * mult / rng + @base;
            } else {
                fun = _ => 1.0;
            }
            var chIdx = provider.fieldNameIndex(QSWATTopology._CHANNEL);
            if (chIdx < 0) {
                QSWATUtils.error("Cannot find {0} field in channels results template".format(QSWATTopology._CHANNEL), this.isBatch);
                return;
            }
            var penIdx = provider.fieldNameIndex(QSWATTopology._PENWIDTH);
            if (penIdx < 0) {
                QSWATUtils.error("Cannot find {0} field in channels results template".format(QSWATTopology._PENWIDTH), this.isBatch);
                return;
            }
            var request = QgsFeatureRequest().setFlags(QgsFeatureRequest.NoGeometry).setSubsetOfAttributes(new List<object> {
                chIdx,
                penIdx
            });
            var mmap = new dict();
            foreach (var f in provider.getFeatures(request)) {
                var ch = f[chIdx];
                var width = data.get(ch, minW);
                mmap[f.id()] = new Dictionary<object, object> {
                    {
                        penIdx,
                        fun(width)}};
            }
            var OK = provider.changeAttributeValues(mmap);
            if (!OK) {
                QSWATUtils.error("Cannot edit channels results template", this.isBatch);
            }
        }
        
        // 
        //         Make file like D8 contributing area but with heightened values at subbasin outlets.  
        //         
        //         Return -1 if cannot make the file.
        //         
        public virtual int makeOutletThresholds(object gv, object root) {
            Debug.Assert(os.path.exists(gv.demFile));
            var demBase = os.path.splitext(gv.demFile)[0];
            if (!QSWATUtils.isUpToDate(gv.demFile, gv.ad8File)) {
                // Probably using existing watershed but switched tabs in delineation form
                // At any rate, cannot calculate flow paths
                QSWATUtils.loginfo("ad8 file not found or out of date");
                return -1;
            }
            Debug.Assert(this.outlets.Count > 0);
            gv.hd8File = demBase + "hd8.tif";
            QSWATUtils.removeLayerAndFiles(gv.hd8File, root);
            Debug.Assert(!os.path.exists(gv.hd8File));
            var ad8Layer = QgsRasterLayer(gv.ad8File, "D8 contributing area");
            // calculate maximum contributing area at an outlet point
            var maxContrib = 0.0;
            foreach (var (_, pt, _) in this.outlets.values()) {
                var contrib = QSWATTopology.valueAtPoint(pt, ad8Layer);
                if (contrib is not null) {
                    maxContrib = max(maxContrib, contrib);
                }
            }
            var threshold = Convert.ToInt32(2 * maxContrib);
            WONKO_del(ad8Layer);
            // copy ad8 to hd8 and then set outlet point values to threshold
            var ad8Ds = gdal.Open(gv.ad8File, gdal.GA_ReadOnly);
            var driver = gdal.GetDriverByName("GTiff");
            var hd8Ds = driver.CreateCopy(gv.hd8File, ad8Ds, 0);
            if (!hd8Ds) {
                QSWATUtils.error("Failed to create hd8 file {0}".format(gv.hd8File), this.isBatch);
                return -1;
            }
            ad8Ds = null;
            QSWATUtils.copyPrj(gv.ad8File, gv.hd8File);
            var band = hd8Ds.GetRasterBand(1);
            var transform = hd8Ds.GetGeoTransform();
            var arr = array(new List<List<int>> {
                new List<int> {
                    threshold
                }
            });
            foreach (var (_, pt, _) in this.outlets.values()) {
                (x, y) = QSWATTopology.projToCell(pt.x(), pt.y(), transform);
                band.WriteArray(arr, x, y);
            }
            hd8Ds = null;
            return threshold;
        }
        
        // Use func as a function to calulate outFile from file1.
        //         
        //         Valid input data values have fun1 applied if it is not None
        public virtual bool runCalc1(
            string file1,
            object func,
            string outFile,
            object gv,
            bool isInt = false,
            object fun1 = null) {
            if (os.path.exists(outFile)) {
                QSWATUtils.removeLayerAndFiles(outFile, gv.iface.legendInterface());
            }
            var r1 = Raster(file1, gv);
            var rout = Raster(outFile, gv, canWrite: true, isInt: isInt);
            var completed = false;
            while (!completed) {
                try {
                    // safer to mark complete immediately to avoid danger of endless loop
                    // only way to loop is then the memory error exception being raised 
                    completed = true;
                    r1.open(this.chunkCount);
                    var noData = isInt ? -99999 : r1.noData;
                    rout.open(this.chunkCount, numRows: r1.numRows, numCols: r1.numCols, transform: r1.ds.GetGeoTransform(), projection: r1.ds.GetProjection(), noData: noData);
                    foreach (var row in Enumerable.Range(0, r1.numRows)) {
                        foreach (var col in Enumerable.Range(0, r1.numCols)) {
                            var v1 = r1.read(row, col);
                            if (fun1 is not null && v1 != r1.noData) {
                                v1 = fun1(v1);
                            }
                            var vout = func(v1, r1.noData, noData);
                            rout.write(row, col, vout);
                        }
                    }
                    r1.close();
                    rout.close();
                } catch (MemoryError) {
                    QSWATUtils.loginfo("runCalc1 out of memory with chunk count {0}".format(this.chunkCount));
                    try {
                        r1.close();
                        rout.close();
                    } catch (Exception) {
                    }
                    this.chunkCount += 1;
                    completed = false;
                }
            }
            if (os.path.exists(outFile)) {
                QSWATUtils.copyPrj(file1, outFile);
                return true;
            } else {
                // QSWATUtils.error(u'Calculator  failed', self._gv.isBatch)
                return false;
            }
        }
        
        // Use func as a function to calulate outFile from file1 and file2.
        //         
        //         Assumes file1 and file2 have same origina and pixel size.
        //         If file1/2 values are not nodata and fun1/2 are not None, they are applied before func is applied.
        public virtual bool runCalc2(
            string file1,
            string file2,
            object func,
            string outFile,
            object gv,
            bool isInt = false,
            object fun1 = null,
            object fun2 = null) {
            if (os.path.exists(outFile)) {
                QSWATUtils.removeLayerAndFiles(outFile, gv.iface.legendInterface());
            }
            var r1 = Raster(file1, gv);
            var r2 = Raster(file2, gv);
            var rout = Raster(outFile, gv, canWrite: true, isInt: isInt);
            var completed = false;
            while (!completed) {
                try {
                    // safer to mark complete immediately to avoid danger of endless loop
                    // only way to loop is then the memory error exception being raised 
                    completed = true;
                    r1.open(this.chunkCount);
                    r2.open(this.chunkCount);
                    var noData = isInt ? -1 : r1.noData;
                    rout.open(this.chunkCount, numRows: r1.numRows, numCols: r1.numCols, transform: r1.ds.GetGeoTransform(), projection: r1.ds.GetProjection(), noData: noData);
                    foreach (var row in Enumerable.Range(0, r1.numRows)) {
                        foreach (var col in Enumerable.Range(0, r1.numCols)) {
                            var v1 = r1.read(row, col);
                            if (fun1 is not null && v1 != r1.noData) {
                                v1 = fun1(v1);
                            }
                            var v2 = r2.read(row, col);
                            if (fun2 is not null && v2 != r2.noData) {
                                v2 = fun2(v2);
                            }
                            var vout = func(v1, r1.noData, v2, r2.noData, noData);
                            rout.write(row, col, vout);
                        }
                    }
                    r1.close();
                    r2.close();
                    rout.close();
                } catch (MemoryError) {
                    QSWATUtils.loginfo("runCalc2 out of memory with chunk count {0}".format(this.chunkCount));
                    try {
                        r1.close();
                        r2.close();
                        rout.close();
                    } catch {
                    }
                    this.chunkCount += 1;
                    completed = false;
                }
            }
            if (os.path.exists(outFile)) {
                QSWATUtils.copyPrj(file1, outFile);
                return true;
            } else {
                // QSWATUtils.error(u'Calculator  failed', self._gv.isBatch)
                return false;
            }
        }
        
        // Use func as a function to calulate outFile from file1 and file2, using numbers of rows and columns of baseFile.
        // 
        //         If file1/2 values are not nodata and fun1/2 are not None, they are applied before func is applied.
        public virtual bool runCalc2Trans(
            string file1,
            string file2,
            object func,
            string outFile,
            string baseFile,
            object gv,
            bool isInt = false,
            object fun1 = null,
            object fun2 = null) {
            if (os.path.exists(outFile)) {
                QSWATUtils.removeLayerAndFiles(outFile, gv.iface.legendInterface());
            }
            var r1 = Raster(file1, gv);
            var r2 = Raster(file2, gv);
            var rout = Raster(outFile, gv, canWrite: true, isInt: isInt);
            var ds = gdal.Open(baseFile, gdal.GA_ReadOnly);
            var transform = ds.GetGeoTransform();
            var numRows = ds.RasterYSize;
            var numCols = ds.RasterXSize;
            var projection = ds.GetProjection();
            ds = null;
            var completed = false;
            while (!completed) {
                try {
                    // safer to mark complete immediately to avoid danger of endless loop
                    // only way to loop is then the memory error exception being raised 
                    completed = true;
                    r1.open(this.chunkCount);
                    r2.open(this.chunkCount);
                    var transform1 = r1.ds.GetGeoTransform();
                    var transform2 = r2.ds.GetGeoTransform();
                    (rowFun1, colFun1) = QSWATTopology.translateCoords(transform, transform1, numRows, numCols);
                    (rowFun2, colFun2) = QSWATTopology.translateCoords(transform, transform2, numRows, numCols);
                    var noData = isInt ? -1 : r1.noData;
                    rout.open(this.chunkCount, numRows: numRows, numCols: numCols, transform: transform, projection: projection, noData: noData);
                    foreach (var row in Enumerable.Range(0, numRows)) {
                        var y = QSWATTopology.rowToY(row, transform);
                        var row1 = rowFun1(row, y);
                        var row2 = rowFun2(row, y);
                        foreach (var col in Enumerable.Range(0, numCols)) {
                            var x = QSWATTopology.colToX(col, transform);
                            var col1 = colFun1(col, x);
                            var col2 = colFun2(col, x);
                            var v1 = r1.read(row1, col1);
                            if (fun1 is not null && v1 != r1.noData) {
                                v1 = fun1(v1);
                            }
                            var v2 = r2.read(row2, col2);
                            if (fun2 is not null && v2 != r2.noData) {
                                v2 = fun2(v2);
                            }
                            var vout = func(v1, r1.noData, v2, r2.noData, noData);
                            rout.write(row, col, vout);
                        }
                    }
                    r1.close();
                    r2.close();
                    rout.close();
                } catch (MemoryError) {
                    QSWATUtils.loginfo("runCalc2Trans out of memory with chunk count {0}".format(this.chunkCount));
                    try {
                        r1.close();
                        r2.close();
                        rout.close();
                    } catch {
                    }
                    this.chunkCount += 1;
                    completed = false;
                }
            }
            if (os.path.exists(outFile)) {
                QSWATUtils.copyPrj(baseFile, outFile);
                return true;
            } else {
                // QSWATUtils.error(u'Calculator  failed', self._gv.isBatch)
                return false;
            }
        }
        
        // Create as burnFile a copy of demFile with points on lines streamFile reduced in height by depth metres.
        [staticmethod]
        public static object burnStream(
            string streamFile,
            string demFile,
            string burnFile,
            double depth,
            double verticalFactor,
            bool isBatch) {
            object y1;
            object x1;
            object y0;
            object x0;
            object lines;
            // use vertical factor to convert from metres to vertical units of DEM
            var demReduction = float(depth) / verticalFactor;
            Debug.Assert(!os.path.exists(burnFile));
            var demDs = gdal.Open(demFile, gdal.GA_ReadOnly);
            var driver = gdal.GetDriverByName("GTiff");
            var burnDs = driver.CreateCopy(burnFile, demDs, 0);
            if (burnDs is null) {
                QSWATUtils.error("Failed to create burned-in DEM {0}".format(burnFile), isBatch);
                return;
            }
            demDs = null;
            QSWATUtils.copyPrj(demFile, burnFile);
            var band = burnDs.GetRasterBand(1);
            var nodata = band.GetNoDataValue();
            var burnTransform = burnDs.GetGeoTransform();
            var streamLayer = QgsVectorLayer(streamFile, "Burn in streams", "ogr");
            var start = time.process_time();
            var countHits = 0;
            var countPoints = 0;
            var countChanges = 0;
            var changed = new dict();
            foreach (var reach in streamLayer.getFeatures()) {
                var geometry = reach.geometry();
                if (geometry.isMultipart()) {
                    lines = geometry.asMultiPolyline();
                } else {
                    lines = new List<object> {
                        geometry.asPolyline()
                    };
                }
                foreach (var line in lines) {
                    foreach (var i in Enumerable.Range(0, line.Count - 1)) {
                        countPoints += 1;
                        var p0 = line[i];
                        var px0 = p0.x();
                        var py0 = p0.y();
                        (x0, y0) = QSWATTopology.projToCell(px0, py0, burnTransform);
                        var p1 = line[i + 1];
                        var px1 = p1.x();
                        var py1 = p1.y();
                        (x1, y1) = QSWATTopology.projToCell(px1, py1, burnTransform);
                        var steep = abs(y1 - y0) > abs(x1 - x0);
                        if (steep) {
                            x0 = y0;
                            y0 = x0;
                            x1 = y1;
                            y1 = x1;
                        }
                        if (x0 > x1) {
                            x0 = x1;
                            x1 = x0;
                            y0 = y1;
                            y1 = y0;
                        }
                        var deltax = x1 - x0;
                        var deltay = abs(y1 - y0);
                        var err = 0;
                        var deltaerr = deltay;
                        var y = y0;
                        var ystep = y0 < y1 ? 1 : -1;
                        var arr = array(new List<List<double>> {
                            new List<double> {
                                0.0
                            }
                        });
                        foreach (var x in Enumerable.Range(x0, x1 + 1 - x0)) {
                            if (steep) {
                                if (QSWATTopology.addPointToChanged(changed, y, x)) {
                                    // read can raise exception if coordinates outside extent
                                    try {
                                        arr = band.ReadAsArray(y, x, 1, 1);
                                        // arr may be none if stream map extends outside DEM extent
                                        if (arr && arr[0,0] != nodata) {
                                            arr[0,0] = arr[0,0] - demReduction;
                                            band.WriteArray(arr, y, x);
                                            countChanges += 1;
                                        }
                                    } catch {
                                    }
                                } else {
                                    countHits += 1;
                                }
                            } else if (QSWATTopology.addPointToChanged(changed, x, y)) {
                                // read can raise exception if coordinates outside extent
                                try {
                                    arr = band.ReadAsArray(x, y, 1, 1);
                                    // arr may be none if stream map extends outside DEM extent
                                    if (arr && arr[0,0] != nodata) {
                                        arr[0,0] = arr[0,0] - demReduction;
                                        band.WriteArray(arr, x, y);
                                        countChanges += 1;
                                    }
                                } catch {
                                }
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
            }
            var finish = time.process_time();
            QSWATUtils.loginfo("Created burned-in DEM {0} in {1!s} milliseconds; {2!s} points; {3!s} hits; {4!s} changes".format(burnFile, Convert.ToInt32((finish - start) * 1000), countPoints, countHits, countChanges));
        }
        
        // Changed points held in dictionary column -> row-sortedlist, since it is like a sparse matrix.
        //         Add a point unless ready there.  Return true if added.
        //         
        [staticmethod]
        public static bool addPointToChanged(object changed, int col, int row) {
            var rows = changed.get(col, new List<object>());
            var inserted = ListFuns.insertIntoSortedList(row, rows, true);
            if (inserted) {
                changed[col] = rows;
                return true;
            } else {
                return false;
            }
        }
        
        // 
        //         Get the band 1 value at point in a grid layer.  Return None if no value, eg NoData
        //         
        //         
        [staticmethod]
        public static object valueAtPoint(object point, object layer) {
            (val, ok) = layer.dataProvider().sample(point, 1);
            if (!ok) {
                return null;
            } else {
                return val;
            }
        }
        
        // Return true if a subbasin is upstream from an inlet.
        public virtual bool isUpstreamSubbasin(int subbasin) {
            return this.upstreamFromInlets.Contains(subbasin);
        }
        
        // Convert a QgsPointXY to latlong coordinates and return it.
        public virtual object pointToLatLong(object point) {
            var geom = QgsGeometry.fromPointXY(point);
            Debug.Assert(this.transformToLatLong is not null);
            geom.transform(this.transformToLatLong);
            return geom.asPoint();
        }
        
        // Get the index of a shapefile layer attribute name, 
        //         reporting error if not found, unless ignoreMissing is true.
        //         
        public virtual int getIndex(object layer, string name, bool ignoreMissing = false) {
            // field names are truncated to 10 characters when created, so only search for up to 10 characters
            // also allow any case, since using lookupField rather than indexOf
            var index = layer.fields().lookupField(name[:10:]);
            if (!ignoreMissing && index < 0) {
                QSWATUtils.error("Cannot find field {0} in {1}".format(name, QSWATUtils.layerFileInfo(layer).filePath()), this.isBatch);
                throw new Exception();
            }
            return index;
        }
        
        // Get the index of a shapefile provider attribute name, 
        //         reporting error if not found, unless ignoreMissing is true.
        //         
        public virtual int getProviderIndex(object provider, string name, bool ignoreMissing = false) {
            // field names are truncated to 10 characters when created, so only search for up to 10 characters
            var index = provider.fieldNameIndex(name[:10:]);
            if (!ignoreMissing && index < 0) {
                QSWATUtils.error("Cannot find field {0} in provider".format(name), this.isBatch);
            }
            return index;
            //     def makePointInLine(self, reach: QgsFeature, percent: float) -> QgsPointXY:
            //         """Return a point percent along line from outlet end to next point."""
            //         if self.outletAtStart:
            //             line = QSWATTopology.reachFirstLine(reach.geometry(), self.xThreshold, self.yThreshold)
            //             pt1 = line[0]
            //             pt2 = line[1]
            //         else:
            //             line = QSWATTopology.reachLastLine(reach.geometry(), self.xThreshold, self.yThreshold)
            //             length = len(line)
            //             pt1 = line[length-1]
            //             pt2 = line[length-2]
            //         x = (pt1.x() * (100 - percent) + pt2.x() * percent) / 100.0
            //         y = (pt1.y() * (100 - percent) + pt2.y() * percent) / 100.0
            //         return QgsPointXY(x, y)
        }
        
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
        public virtual bool hasOutletAtStart(object streamLayer, object ad8Layer) {
            object lines;
            if (this.isHUC || this.isHAWQS) {
                return false;
            }
            var streamIndex = this.getIndex(streamLayer, QSWATTopology._LINKNO, ignoreMissing: false);
            if (streamIndex < 0) {
                QSWATUtils.error("No LINKNO field in stream layer", this.isBatch);
                return true;
            }
            var dsStreamIndex = this.getIndex(streamLayer, QSWATTopology._DSLINKNO, ignoreMissing: false);
            if (dsStreamIndex < 0) {
                QSWATUtils.error("No DSLINKNO field in stream layer", this.isBatch);
                return true;
            }
            if (ad8Layer is not null) {
                // only set to non-None if not an existing watershed
                // use accumulation difference at ends of reach (or line in reach) to decide
                foreach (var reach in streamLayer.getFeatures()) {
                    var geometry = reach.geometry();
                    if (geometry.isMultipart()) {
                        lines = geometry.asMultiPolyline();
                    } else {
                        lines = new List<object> {
                            geometry.asPolyline()
                        };
                    }
                    foreach (var line in lines) {
                        if (line.Count > 1) {
                            // make sure we haven't picked on an empty line
                            var p1 = line[0];
                            var p2 = line[^1];
                            var acc1 = QSWATTopology.valueAtPoint(p1, ad8Layer);
                            var acc2 = QSWATTopology.valueAtPoint(p2, ad8Layer);
                            if (acc1 is not null && acc2 is not null && acc1 != acc2) {
                                return acc1 > acc2;
                            }
                        }
                    }
                }
            }
            // find candidates: links with a down connection
            var candidates = new List<object>();
            foreach (var reach in streamLayer.getFeatures()) {
                var downLink = reach[dsStreamIndex];
                if (downLink >= 0) {
                    // find the down reach
                    var downReach = QSWATUtils.getFeatureByValue(streamLayer, streamIndex, downLink);
                    if (downReach is not null) {
                        candidates.append((reach, downReach));
                        if (candidates.Count < 10) {
                            continue;
                        } else {
                            break;
                        }
                    } else {
                        QSWATUtils.error("Cannot find link {0!s} in {1}".format(downLink, QSWATUtils.layerFileInfo(streamLayer).filePath()), this.isBatch);
                        return true;
                    }
                }
            }
            if (candidates == new List<object>()) {
                QSWATUtils.error("Cannot find link with a downstream link in {0}.  Do you only have one stream?".format(QSWATUtils.layerFileInfo(streamLayer).filePath()), this.isBatch);
                return true;
            }
            foreach (var (upReach, downReach) in candidates) {
                var downGeom = downReach.geometry();
                var downStart = QSWATTopology.reachFirstLine(downGeom, this.xThreshold, this.yThreshold);
                if (downStart is null) {
                    continue;
                }
                var downFinish = QSWATTopology.reachLastLine(downGeom, this.xThreshold, this.yThreshold);
                if (downFinish is null) {
                    continue;
                }
                var upGeom = upReach.geometry();
                var upStart = QSWATTopology.reachFirstLine(upGeom, this.xThreshold, this.yThreshold);
                if (upStart is null) {
                    continue;
                }
                var upFinish = QSWATTopology.reachLastLine(upGeom, this.xThreshold, this.yThreshold);
                if (upFinish is null) {
                    continue;
                }
                if (QSWATTopology.pointOnLine(upStart[0], downFinish, this.xThreshold, this.yThreshold)) {
                    return true;
                }
                if (QSWATTopology.pointOnLine(upFinish[^1], downStart, this.xThreshold, this.yThreshold)) {
                    return false;
                }
            }
            QSWATUtils.error("Cannot find physically connected reaches in streams shapefile {0}.  Try increasing nearness threshold".format(QSWATUtils.layerFileInfo(streamLayer).filePath()), this.isBatch);
            return true;
        }
        
        // Write outlets, downSubbasins, and (unless useGridModel)
        //          inlets, upstreamFromInlets, and outletChannels  tables.
        public virtual bool saveOutletsAndSources(object channelLayer, object outletLayer, bool useGridModel) {
            object last;
            object first;
            object pt;
            object subbasin;
            object dsChLink;
            object subbasinIndex;
            // in case called twice
            this.pointId = 0;
            this.waterBodyId = 0;
            this.outlets.clear();
            this.inlets.clear();
            this.chOutlets.clear();
            this.chPointSources.clear();
            this.upstreamFromInlets.clear();
            this.downSubbasins.clear();
            this.chBasinToSubbasin.clear();
            var chLinkToSubbasin = new dict();
            var downChannels = new dict();
            var chInlets = new dict();
            var chLinkIndex = this.getIndex(channelLayer, QSWATTopology._LINKNO);
            var DSLINKNO = this.isHUC ? QSWATTopology._DSLINKNO1 : QSWATTopology._DSLINKNO;
            var dsChLinkIndex = this.getIndex(channelLayer, DSLINKNO);
            var isHUCOrHAWQS = this.isHUC || this.isHAWQS;
            this.wsnoIndex = this.getIndex(channelLayer, QSWATTopology._WSNO, ignoreMissing: !useGridModel && !isHUCOrHAWQS);
            var sourceXIndex = this.getIndex(channelLayer, QSWATTopology._SOURCEX, ignoreMissing: !isHUCOrHAWQS);
            var sourceYIndex = this.getIndex(channelLayer, QSWATTopology._SOURCEY, ignoreMissing: !isHUCOrHAWQS);
            var outletXIndex = this.getIndex(channelLayer, QSWATTopology._OUTLETX, ignoreMissing: !isHUCOrHAWQS);
            var outletYIndex = this.getIndex(channelLayer, QSWATTopology._OUTLETY, ignoreMissing: !isHUCOrHAWQS);
            if (chLinkIndex < 0 || dsChLinkIndex < 0) {
                return false;
            }
            if (this.isHAWQS) {
                // channel file has a Subbasin field
                subbasinIndex = this.getIndex(channelLayer, QSWATTopology._SUBBASIN);
            } else {
                // ignoreMissing for subbasinIndex necessary when useGridModel, since channelLayer is then a streams layer
                subbasinIndex = this.getIndex(channelLayer, QSWATTopology._BASINNO, ignoreMissing: useGridModel);
            }
            if (useGridModel) {
                if (this.wsnoIndex < 0) {
                    return false;
                }
            } else if (subbasinIndex < 0) {
                return false;
            }
            var dsNodeIndex = this.getIndex(channelLayer, QSWATTopology._DSNODEID, ignoreMissing: true);
            if (outletLayer is not null) {
                var outletProvider = outletLayer.dataProvider();
                var idIndex = this.getProviderIndex(outletProvider, QSWATTopology._ID, ignoreMissing: false);
                var inletIndex = this.getProviderIndex(outletProvider, QSWATTopology._INLET, ignoreMissing: false);
                var srcIndex = this.getProviderIndex(outletProvider, QSWATTopology._PTSOURCE, ignoreMissing: false);
                var resIndex = this.getProviderIndex(outletProvider, QSWATTopology._RES, ignoreMissing: false);
                var ptIdIndex = this.getProviderIndex(outletProvider, QSWATTopology._POINTID, ignoreMissing: true);
                if (ptIdIndex < 0) {
                    outletProvider.addAttributes(new List<object> {
                        QgsField(QSWATTopology._POINTID, QVariant.Int)
                    });
                    outletLayer.updateFields();
                    ptIdIndex = outletProvider.fieldNameIndex(QSWATTopology._POINTID);
                }
                // set pointId to max id value in outletLayer
                // and waterBodyId to max reservoir or pond id
                // copy original id to PointId field, unless zero, when change to max id + 1
                var ptIdMap = new dict();
                var zeroIndex = -1;
                var request = QgsFeatureRequest().setFlags(QgsFeatureRequest.NoGeometry);
                foreach (var point in outletLayer.getFeatures(request)) {
                    var ptId = point[idIndex];
                    if (ptId == 0) {
                        zeroIndex = point.id();
                    } else {
                        ptIdMap[point.id()] = new Dictionary<object, object> {
                            {
                                ptIdIndex,
                                ptId}};
                    }
                    this.pointId = max(this.pointId, ptId);
                    if (point[inletIndex] == 0 && point[resIndex] > 0) {
                        this.waterBodyId = max(this.waterBodyId, ptId);
                    }
                }
                if (zeroIndex >= 0) {
                    this.pointId += 1;
                    ptIdMap[zeroIndex] = new Dictionary<object, object> {
                        {
                            ptIdIndex,
                            this.pointId}};
                }
                outletProvider.changeAttributeValues(ptIdMap);
            } else {
                dsNodeIndex = -1;
            }
            foreach (var reach in channelLayer.getFeatures()) {
                var chLink = reach[chLinkIndex];
                dsChLink = reach[dsChLinkIndex];
                var chBasin = reach[this.wsnoIndex];
                // for grids, channel basins and subbasins are the same
                subbasin = useGridModel ? chBasin : reach[subbasinIndex];
                chLinkToSubbasin[chLink] = subbasin;
                if (!useGridModel) {
                    this.chBasinToSubbasin[chBasin] = subbasin;
                }
                downChannels[chLink] = dsChLink;
                var dsNode = dsNodeIndex >= 0 ? reach[dsNodeIndex] : -1;
                if (dsNode >= 0 && idIndex >= 0 && inletIndex >= 0 && srcIndex >= 0 && resIndex >= 0 && ptIdIndex >= 0) {
                    object outletPoint = null;
                    object inletPoint = null;
                    Debug.Assert(outletLayer is not null);
                    foreach (var f in outletLayer.getFeatures()) {
                        if (f[idIndex] == dsNode) {
                            if (f[inletIndex] == 0) {
                                if (f[resIndex] == 0) {
                                    outletPoint = f;
                                    break;
                                }
                            } else if (f[srcIndex] == 0) {
                                inletPoint = f;
                                break;
                            }
                        }
                    }
                    if (outletPoint is not null) {
                        pt = outletPoint.geometry().asPoint();
                        this.chOutlets[chLink] = (outletPoint[ptIdIndex], pt);
                    } else if (inletPoint is not null) {
                        pt = inletPoint.geometry().asPoint();
                        chInlets[chLink] = (inletPoint[ptIdIndex], pt);
                    }
                }
                if (this.isHUC) {
                    first = new List<object> {
                        QgsPointXY(reach[sourceXIndex], reach[sourceYIndex])
                    };
                    last = new List<object> {
                        QgsPointXY(reach[outletXIndex], reach[outletYIndex])
                    };
                } else {
                    var geom = reach.geometry();
                    first = QSWATTopology.reachFirstLine(geom, this.xThreshold, this.yThreshold);
                    if (first is null || first.Count < 2) {
                        QSWATUtils.error("It looks like your channels shapefile does not obey the single direction rule, that all channels are either upstream or downstream.", this.isBatch);
                        return false;
                    }
                    last = QSWATTopology.reachLastLine(geom, this.xThreshold, this.yThreshold);
                    if (last is null || last.Count < 2) {
                        QSWATUtils.error("It looks like your channels shapefile does not obey the single direction rule, that all channels are either upstream or downstream.", this.isBatch);
                        return false;
                    }
                }
                (outId, outPt) = this.chOutlets.get(chLink, (-1, null));
                if (outPt is null) {
                    this.pointId += 1;
                    var outId = this.pointId;
                }
                this.pointId += 1;
                var srcId = this.pointId;
                Debug.Assert(first is not null);
                Debug.Assert(last is not null);
                if (this.outletAtStart) {
                    if (!useGridModel && outPt is not null && !QSWATTopology.coincidentPoints(first[0], outPt, this.xThreshold, this.yThreshold)) {
                        QSWATUtils.error("Outlet point {0} at ({1}, {2}) not coincident with start of channel link {3}".format(outId, outPt.x(), outPt.y(), chLink), this.isBatch);
                    }
                    this.chOutlets[chLink] = (outId, first[0]);
                    this.chPointSources[chLink] = (srcId, last[^1]);
                } else {
                    if (!useGridModel && outPt is not null && !QSWATTopology.coincidentPoints(last[^1], outPt, this.xThreshold, this.yThreshold)) {
                        QSWATUtils.error("Outlet point {0} at ({1}, {2}) not coincident with end of channel link {3}".format(outId, outPt.x(), outPt.y(), chLink), this.isBatch);
                    }
                    this.chOutlets[chLink] = (outId, last[^1]);
                    this.chPointSources[chLink] = (srcId, first[0]);
                }
            }
            // now find the channels which are on subbasin boundaries,
            // i.e. their downstream channels are in different basins
            var hasInlet = false;
            foreach (var (chLink, dsChLink) in downChannels.items()) {
                subbasin = chLinkToSubbasin[chLink];
                if (subbasin == QSWATTopology._NOBASIN) {
                    // from a zero-length channel
                    continue;
                }
                var dsSubbasin = dsChLink >= 0 ? chLinkToSubbasin[dsChLink] : -1;
                while (dsSubbasin == QSWATTopology._NOBASIN) {
                    // skip past zero-length channels
                    dsChLink = downChannels.get(dsChLink, -1);
                    dsSubbasin = chLinkToSubbasin.get(dsChLink, -1);
                }
                if (subbasin != dsSubbasin) {
                    this.downSubbasins[subbasin] = dsSubbasin;
                    // collect the basin's outlet location:
                    (outletId, outletPt) = this.chOutlets[chLink];
                    var existOutlets = this.outlets.get(subbasin, null);
                    if (existOutlets is null) {
                        this.outlets[subbasin] = (outletId, outletPt, new List<object> {
                            chLink
                        });
                    } else {
                        (outletid0, outletPt0, chLinks) = existOutlets;
                        if (!QSWATTopology.coincidentPoints(outletPt0, outletPt, this.xThreshold, this.yThreshold)) {
                            QSWATUtils.error("Polygon {0} has separate outlets at ({1}, {2}) and ({3}, {4}): ignoring second".format(subbasin, outletPt0.x(), outletPt0.y(), outletPt.x(), outletPt.y()), this.isBatch);
                        } else {
                            this.chOutlets[chLink] = (outletid0, outletPt0);
                            this.outlets[subbasin] = (outletid0, outletPt0, chLinks + new List<object> {
                                chLink
                            });
                        }
                    }
                    if (!useGridModel) {
                        //                     self.extraResPoints[subbasin] = chResPoints[chLink]
                        //                     self.extraPtSrcPoints[subbasin] = chSources[chLink]
                        (inletId, inletPt) = chInlets.get(chLink, (-1, null));
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
                foreach (var subbasin in this.inlets.keys()) {
                    this.addUpstreamSubbasins(subbasin);
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
        
        // Add basins upstream from start to upstreamFromInlets.
        public virtual object addUpstreamSubbasins(int start) {
            foreach (var (subbasin, downSubbasin) in this.downSubbasins.items()) {
                if (downSubbasin == start) {
                    this.upstreamFromInlets.add(subbasin);
                    this.addUpstreamSubbasins(subbasin);
                }
            }
        }
        
        // Return id of lake containing channel, if any, else -1.
        public virtual int surroundingLake(int SWATChannel, bool useGridModel) {
            var chLink = this.SWATChannelToChannel[SWATChannel];
            var lake1 = this.chLinkInsideLake.get(chLink, -1);
            if (useGridModel && lake1 < 0) {
                return this.chLinkFromLake.get(chLink, -1);
            } else {
                return lake1;
            }
        }
        
        // Result is val unless mask is nodata.
        [staticmethod]
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
        
        // Returns the line of a single polyline, 
        //         or a line in a multipolyline whose first point is not adjacent to a point 
        //         of another line in the multipolyline.
        //         
        [staticmethod]
        public static object reachFirstLine(object geometry, double xThreshold, double yThreshold) {
            if (!geometry.isMultipart()) {
                return geometry.asPolyline();
            }
            var mpl = geometry.asMultiPolyline();
            var numLines = mpl.Count;
            foreach (var i in Enumerable.Range(0, numLines)) {
                var linei = mpl[i];
                var connected = false;
                if (linei is null || linei.Count == 0) {
                    continue;
                } else {
                    var start = linei[0];
                    foreach (var j in Enumerable.Range(0, numLines)) {
                        if (i != j) {
                            var linej = mpl[j];
                            // linei is connected to linej (ie continues from linej) if the first point of linei (start) is on linej AND
                            // linej has preceding point(s).
                            // So exclude the start of linej in the search for a connection point
                            if (linej.Count > 0 && QSWATTopology.pointOnLine(start, linej[1], xThreshold, yThreshold)) {
                                connected = true;
                                break;
                            }
                        }
                    }
                }
                if (!connected) {
                    return linei;
                }
            }
            // should not get here
            return null;
        }
        
        // Returns the line of a single polyline, 
        //         or a line in a multipolyline whose last point is not adjacent to a point 
        //         of another line in the multipolyline.
        //         
        [staticmethod]
        public static object reachLastLine(object geometry, double xThreshold, double yThreshold) {
            if (!geometry.isMultipart()) {
                return geometry.asPolyline();
            }
            var mpl = geometry.asMultiPolyline();
            var numLines = mpl.Count;
            foreach (var i in Enumerable.Range(0, numLines)) {
                var linei = mpl[i];
                var connected = false;
                if (linei is null || linei.Count == 0) {
                    continue;
                } else {
                    var finish = linei[^1];
                    foreach (var j in Enumerable.Range(0, numLines)) {
                        if (i != j) {
                            var linej = mpl[j];
                            // linei is connected to linej if the last point of linei (finish) is on linej AND
                            // linej extends further.
                            // So exclude the finish of linej in the search for a connection point
                            if (linej.Count > 0 && QSWATTopology.pointOnLine(finish, linej[: - 1:], xThreshold, yThreshold)) {
                                connected = true;
                                break;
                            }
                        }
                    }
                }
                if (!connected) {
                    return linei;
                }
            }
            // should not get here
            return null;
        }
        
        // Return true if point is coincident with a point on the line. 
        //         
        //         Note this only checks if the point is close to a vertex.
        [staticmethod]
        public static bool pointOnLine(object point, List<object> line, double xThreshold, double yThreshold) {
            if (line is null || line.Count == 0) {
                return false;
            }
            foreach (var pt in line) {
                if (QSWATTopology.coincidentPoints(point, pt, xThreshold, yThreshold)) {
                    return true;
                }
            }
            return false;
        }
        
        // Return true if points are within xThreshold and yThreshold
        //         horizontally and vertically.
        [staticmethod]
        public static bool coincidentPoints(object pt1, object pt2, double xThreshold, double yThreshold) {
            return abs(pt1.x() - pt2.x()) < xThreshold && abs(pt1.y() - pt2.y()) < yThreshold;
        }
        
        // Convert column number to X-coordinate.
        [staticmethod]
        public static double colToX(int col, object transform) {
            return (col + 0.5) * transform[1] + transform[0];
        }
        
        // Convert row number to Y-coordinate.
        [staticmethod]
        public static double rowToY(int row, object transform) {
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
        [staticmethod]
        public static object cellToProj(int col, int row, object transform) {
            var x = (col + 0.5) * transform[1] + transform[0];
            var y = (row + 0.5) * transform[5] + transform[3];
            return (x, y);
        }
        
        // Convert (X,Y)-coordinates to column and row numbers.
        [staticmethod]
        public static object projToCell(double x, double y, object transform) {
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
        //     if (abs(transform1[0] - transform2[0]) > transform2[1] * 0.1 or \
        //         abs(transform1[3] - transform2[3]) > abs(transform2[5]) * 0.1):
        //         return False
        //     # then check if the vertical/horizontal difference in cell size times the number of rows/columns
        //     # in the first is less than half the depth/width of a cell in the second
        //     return abs(transform1[1] - transform2[1]) * band1.XSize < transform2[1] * 0.5 and \
        //             abs(transform1[5] - transform2[5]) * band1.YSize < abs(transform2[5]) * 0.5
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
        [staticmethod]
        public static object translateCoords(object transform1, object transform2, int numRows1, int numCols1) {
            object rowShift;
            object yFun;
            object colShift;
            object xFun;
            // may work, thuough we are comparing real values
            if (transform1 == transform2) {
                return ((row,_) => row, (col,_) => col);
            }
            (xOrigin1, xSize1, _, yOrigin1, _, ySize1) = transform1;
            (xOrigin2, xSize2, _, yOrigin2, _, ySize2) = transform2;
            // accept the origins as the same if they are within a tenth of the cell size
            var sameXOrigin = abs(xOrigin1 - xOrigin2) < xSize2 * 0.1;
            var sameYOrigin = abs(yOrigin1 - yOrigin2) < abs(ySize2) * 0.1;
            // accept cell sizes as equivalent if  vertical/horizontal difference 
            // in cell size times the number of rows/columns
            // in the first is less than half the depth/width of a cell in the second
            var sameXSize = abs(xSize1 - xSize2) * numCols1 < xSize2 * 0.5;
            var sameYSize = abs(ySize1 - ySize2) * numRows1 < abs(ySize2) * 0.5;
            if (sameXSize) {
                if (sameXOrigin) {
                    xFun = (col,_) => col;
                } else {
                    // just needs origin shift
                    // note that int truncates, i.e. rounds towards zero
                    if (xOrigin1 > xOrigin2) {
                        colShift = Convert.ToInt32((xOrigin1 - xOrigin2) / xSize1 + 0.5);
                        xFun = (col,_) => col + colShift;
                    } else {
                        colShift = Convert.ToInt32((xOrigin2 - xOrigin1) / xSize1 + 0.5);
                        xFun = (col,_) => col - colShift;
                    }
                }
            } else {
                // full transformation
                xFun = (_,x) => Convert.ToInt32((x - xOrigin2) / xSize2);
            }
            if (sameYSize) {
                if (sameYOrigin) {
                    yFun = (row,_) => row;
                } else {
                    // just needs origin shift
                    // note that int truncates, i.e. rounds towards zero, and y size will be negative
                    if (yOrigin1 > yOrigin2) {
                        rowShift = Convert.ToInt32((yOrigin2 - yOrigin1) / ySize1 + 0.5);
                        yFun = (row,_) => row - rowShift;
                    } else {
                        rowShift = Convert.ToInt32((yOrigin1 - yOrigin2) / ySize1 + 0.5);
                        yFun = (row,_) => row + rowShift;
                    }
                }
            } else {
                // full transformation
                yFun = (_,y) => Convert.ToInt32((y - yOrigin2) / ySize2);
            }
            // note row, column order of return (same as order of reading rasters)
            return (yFun, xFun);
        }
        
        // Return true if transforms are sufficiently close to be regarded as the same,
        //         i.e. row and column numbers for the first can be used without transformation to read the second.  
        //         Avoids relying on equality between real numbers.
        [staticmethod]
        public static bool sameTransform(object transform1, object transform2, int numRows1, int numCols1) {
            // may work, thuough we are comparing real values
            if (transform1 == transform2) {
                return true;
            }
            (xOrigin1, xSize1, _, yOrigin1, _, ySize1) = transform1;
            (xOrigin2, xSize2, _, yOrigin2, _, ySize2) = transform2;
            // accept the origins as the same if they are within a tenth of the cell size
            var sameXOrigin = abs(xOrigin1 - xOrigin2) < xSize2 * 0.1;
            if (sameXOrigin) {
                var sameYOrigin = abs(yOrigin1 - yOrigin2) < abs(ySize2) * 0.1;
                if (sameYOrigin) {
                    // accept cell sizes as equivalent if  vertical/horizontal difference 
                    // in cell size times the number of rows/columns
                    // in the first is less than half the depth/width of a cell in the second
                    var sameXSize = abs(xSize1 - xSize2) * numCols1 < xSize2 * 0.5;
                    if (sameXSize) {
                        var sameYSize = abs(ySize1 - ySize2) * numRows1 < abs(ySize2) * 0.5;
                        return sameYSize;
                    }
                }
            }
            return false;
        }
        
        // lakeGeom is a polygon representing a lake.  reach is known to intersect with the lake..
        //         Returns a pair of inflowing and outflowing reaches, either or both of which may be None.
        public virtual object splitReachByLake(object lakeGeom, object reachGeom, object reachData) {
            var sourcePt = QgsPointXY(reachData.upperX, reachData.upperY);
            var sourceToLake = QSWATTopology.toIntersection(reachGeom, lakeGeom, sourcePt, !this.outletAtStart, this.xThreshold, this.yThreshold);
            var outletPt = QgsPointXY(reachData.lowerX, reachData.lowerY);
            var outletToLake = QSWATTopology.toIntersection(reachGeom, lakeGeom, outletPt, this.outletAtStart, this.xThreshold, this.yThreshold);
            return (sourceToLake, outletToLake);
        }
        
        // Return geometry for sequence of points from start to one before first one that intersects with lakeGeom, 
        //         or None if this is empty or a singleton, or if start is within the lake.
        //         
        //         If isUp the search is from index 0 if the of the reach, else it is from the last index.
        [staticmethod]
        public static object toIntersection(
            object reachGeom,
            object lakeGeom,
            object start,
            bool isUp,
            double xThreshold,
            double yThreshold) {
            object length;
            object mpl;
            if (lakeGeom.contains(start)) {
                return null;
            }
            if (reachGeom.isMultipart()) {
                mpl = reachGeom.asMultiPolyline();
            } else {
                mpl = new List<object> {
                    reachGeom.asPolyline()
                };
            }
            var result = new List<object>();
            var done = new HashSet<object>();
            while (true) {
                var progress = false;
                foreach (var i in Enumerable.Range(0, mpl.Count)) {
                    if (!done.Contains(i)) {
                        var line = mpl[i];
                        if (line.Count <= 1) {
                            continue;
                        }
                        if (isUp) {
                            if (QSWATTopology.coincidentPoints(start, line[0], xThreshold, yThreshold)) {
                                foreach (var pt in line) {
                                    if (lakeGeom.contains(pt)) {
                                        length = result.Count;
                                        if (length < 1) {
                                            return null;
                                        } else if (length == 1) {
                                            // create zero-length line at result[0]
                                            return QgsGeometry.fromPolylineXY(new List<List<object>> {
                                                result[0],
                                                result[0]
                                            });
                                        }
                                        return QgsGeometry.fromPolylineXY(result);
                                    }
                                    result.append(pt);
                                }
                                start = line[^1];
                                done.add(i);
                                progress = true;
                            }
                        } else if (QSWATTopology.coincidentPoints(start, line[^1], xThreshold, yThreshold)) {
                            foreach (var pt in reversed(line)) {
                                if (lakeGeom.contains(pt)) {
                                    length = result.Count;
                                    if (length < 1) {
                                        return null;
                                    } else if (length == 1) {
                                        // create zero-length line at result[0]
                                        return QgsGeometry.fromPolylineXY(new List<List<object>> {
                                            result[0],
                                            result[0]
                                        });
                                    }
                                    return QgsGeometry.fromPolylineXY(result);
                                }
                                result.insert(0, pt);
                            }
                            start = line[0];
                            done.add(i);
                            progress = true;
                        }
                    }
                }
                if (!progress) {
                    throw new Exception("Looping trying to calculate reach");
                    //     @staticmethod
                    //     def splitReach(resGeom, reachGeom, source, outlet, outletAtStart, xThreshold, yThreshold):
                    //         """Split reachGeom into two parts, one from source to reservoir and one from reservoir to outlet.
                    //         
                    //         Assumes the reach has been split into at least two disjoint parts, one flowing from source, the other flowing to outlet.
                    //         Algorithm checks each line in reach geometry, moving up from source or down from outlet until reservoir is reached
                    //         in both cases."""
                    //         sourcePart = []
                    //         outletPart = []
                    //         mpl = reachGeom.asMultiPolyline()
                    //         done = set()
                    //         outletToLakeDone = False
                    //         sourceToLakeDone = False
                    //         while True:
                    //             reduced = False
                    //             for i in xrange(len(mpl)):
                    //                 if i not in done:
                    //                     line = mpl[i]
                    //                     start = line[0]
                    //                     finish = line[-1]
                    //                     if outletAtStart:
                    //                         if not outletToLakeDone and QSWATTopology.coincidentPoints(outlet, start, xThreshold, yThreshold):
                    //                             newLine = []
                    //                             for pt in line:
                    //                                 newLine.append(pt)
                    //                                 if resGeom.intersects(QgsGeometry.fromPointXY(pt)):
                    //                                     outletToLakeDone = True 
                    //                                     break
                    //                             outletPart.append(newLine)
                    //                             outlet = finish
                    //                             reduced = True
                    //                             done.add(i)
                    //                         elif not sourceToLakeDone and QSWATTopology.coincidentPoints(source, finish, xThreshold, yThreshold):
                    //                             newLine = []
                    //                             for pt in reversed(line):
                    //                                 newLine.insert(0, pt)
                    //                                 if resGeom.intersects(QgsGeometry.fromPointXY(pt)):
                    //                                     sourceToLakeDone = True 
                    //                                     break
                    //                             sourcePart.append(newLine)
                    //                             source = start
                    //                             done.add(i)
                    //                             reduced = True
                    //                     else:
                    //                         if not outletToLakeDone and QSWATTopology.coincidentPoints(outlet, finish, xThreshold, yThreshold):
                    //                             newLine = []
                    //                             for pt in reversed(line):
                    //                                 newLine.insert(0, pt)
                    //                                 if resGeom.intersects(QgsGeometry.fromPointXY(pt)):
                    //                                     outletToLakeDone = True 
                    //                                     break
                    //                             outletPart.append(line)
                    //                             outlet = start
                    //                             done.add(i)
                    //                             reduced = True
                    //                         elif QSWATTopology.coincidentPoints(source, start, xThreshold, yThreshold):
                    //                             newLine = []
                    //                             for pt in line:
                    //                                 newLine.append(pt)
                    //                                 if resGeom.intersects(QgsGeometry.fromPointXY(pt)):
                    //                                     sourceToLakeDone = True 
                    //                                     break
                    //                             sourcePart.append(line)
                    //                             source = finish
                    //                             done.add(i)
                    //                             reduced = True
                    //             if outletToLakeDone and sourceToLakeDone:
                    //                 break
                    //             if not reduced:
                    //                 raise Exception('Looping trying to split reach')
                    //         sourceGeom = QgsGeometry.fromPolyline(sourcePart[0]) if len(sourcePart) == 1 else QgsGeometry.fromMultiPolyline(sourcePart)
                    //         outletGeom = QgsGeometry.fromPolyline(outletPart[0]) if len(outletPart) == 1 else QgsGeometry.fromMultiPolyline(outletPart)
                    //         return sourceGeom, outletGeom
                }
            }
        }
        
        // Point pt is contained in lake.  Move it downstream at most maxSteps
        //         using D8 flow direction raster pFile until it is not inside the lake,
        //         returning new point and true.
        //         
        //         Return original point and false if failed to find perimeter.
        [staticmethod]
        public static object movePointToPerimeter(object pt, object lakeGeom, string pFile, int maxSteps) {
            var pLayer = QgsRasterLayer(pFile, "FlowDir");
            var ds = gdal.Open(pFile, gdal.GA_ReadOnly);
            var transform = ds.GetGeoTransform();
            var stepCount = 0;
            var pt1 = pt;
            while (stepCount < maxSteps) {
                if (!lakeGeom.contains(pt1)) {
                    return (pt1, true);
                }
                var dir1 = QSWATTopology.valueAtPoint(pt1, pLayer);
                if (dir1 is null) {
                    QSWATUtils.loginfo("Failed to reach lake perimeter: no flow direction available.");
                    return (pt, false);
                }
                // dir1 is read as a float.  Also subtract 1 to get range 0..7
                var dir0 = Convert.ToInt32(dir1) - 1;
                (col, row) = QSWATTopology.projToCell(pt1.x(), pt1.y(), transform);
                var col1 = col + QSWATUtils._dX[dir0];
                var row1 = row + QSWATUtils._dY[dir0];
                (x1, y1) = QSWATTopology.cellToProj(col1, row1, transform);
                pt1 = QgsPointXY(x1, y1);
                stepCount += 1;
            }
            QSWATUtils.loginfo("Failed to reach lake perimeter in {0} steps.".format(maxSteps));
            return (pt, false);
        }
        
        [staticmethod]
        public static string lakeCategory(int waterRole) {
            if (waterRole == QSWATTopology._RESTYPE) {
                return "reservoir";
            } else if (waterRole == QSWATTopology._PONDTYPE) {
                return "pond";
            } else if (waterRole == QSWATTopology._WETLANDTYPE) {
                return "wetland";
            } else {
                return "playa";
            }
        }
    }
    
    // -*- coding: utf-8 -*-
    // Import the PyQt and QGIS libraries
    //from qgis.PyQt.QtGui import *  # @UnusedWildImport type: ignore 
    // if TYPE_CHECKING:
    //     from globals import Any
    static QSWATTopology() {
        try {
        } catch {
            // used by convertFromArc
        }
    }
}
