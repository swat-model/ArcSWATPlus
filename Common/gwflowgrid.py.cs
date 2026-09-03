
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.Mapping.Symbology;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using Microsoft.VisualBasic;
using OSGeo.GDAL;
using OSGeo.OGR;
using OSGeo.OSR;
using SuperClusterKDTree;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ArcSWATPlus
{


    //             
    //Background grid generation for gwflow.

    //Generates structured or unstructured (balanced quadtree) grids clipped
    //to the watershed + lake boundary and saved as a shapefile.

    //Unstructured algorithm adapted from process.py: uses KDTree for fast
    //distance queries, levels-based refinement (min = max / 2^levels), and
    //2:1 balancing so adjacent cells differ by at most one refinement level.


    // Manages background grid generation and layer updates.
    public class GridGenerator
    {

        public static string _LAYER_NAME = "GWFlow Cells";

        public static string _LAYER_ID = "gwflowcells";

        public static string _SUBBASINS_NAME = "Subbasins";

        public static string _LSUS_NAME = "LSUs";

        public static string _LOG_TAG = "gwflow grid";

        public static string _CELL_ID = "cell_id";

        public static string _CELL_AREA = "cell_area";



        public GlobalVars _gv;

        //public Dictionary<string, object> _params;

        //public object _task;

        //public object _timer;

        public OSGeo.OGR.Geometry basinGeometry;

        public OSGeo.OGR.Envelope basinExtent;

            public GridGenerator(GlobalVars gv) {
                this._gv = gv;
            //this._task = null;
            //this._timer = QTimer();
            //this._timer.setSingleShot(true);
            //this._timer.setInterval(600);
            //this._timer.timeout.connect(this._startGeneration);
            //this._params = new Dictionary<string, object> {
            //};
            this.basinGeometry = null;
            this.basinExtent = null;
            }

        // set basin geometry and extent
        public async Task init() {
            await this._getWatershedGeometry();
        }

        public async Task requestGeneration(HRUsForm hf) {
            //                this._params = new dict(@params);
            ///*                this._*/timer.start();
            ///
            using (new CursorWait()) {
                try {
                    hf.progress("Generating gwflow grid ...");
                    hf.enableCreateButton(false);
                    this._startGeneration(hf);
                }
                catch (Exception) {; }
                finally {
                    hf.progress("Generating gwflow grid done");
                    hf.enableCreateButton(true);
                }
            }
        }

        public async Task _startGeneration(HRUsForm hf) {
            //try {
            //    if (this._task is not null && this._task.status() == QgsTask.Running) {
            //        this._task.cancel();
            //    }
            //} catch (RuntimeError) {
            //}
            //this._task = null;
            var wsGeom = this.basinGeometry;
            var extent = this.basinExtent;
                if (extent is null) {
                    Utils.error("No basin geometry found. Aborting.", this._gv.isBatch);
                    return;
                }
                var outPath = Path.Join(this._gv.shapesDir, "gwflowcells.shp");
                Utils.loginfo(string.Format("Starting grid generation: type={0} extent={1} out={2}", 
                    hf.isStructured ? "structured" : "unstructured", extent.ToString(), outPath));
                // Collect refinement points for unstructured grids
                var refineCoords = new List<IReadOnlyList<double>>();
                if (!hf.isStructured) {
                    if (hf._refineStreams) {
                        refineCoords.AddRange(this._getStreamCoords());
                    }
                    if (hf._refineBoundary && wsGeom is not null) {
                        refineCoords.AddRange(GridGenerator._getBoundaryCoords(wsGeom));
                    }
                    if (hf._refineWells) {
                        refineCoords.AddRange(this._getWellCoords());
                    }
                }
                var demFile = "";
                if (hf._refineTopography && !hf.isStructured) {
                    demFile = this._gv.demFile;
                }
            //var numProcesses = 8; //proj.readNumEntry(title, Convert.ToInt32(QSettings().value("/QSWATPlus/NumProcesses", 8));
                var task = new _GridTask(extent, wsGeom, hf, refineCoords, demFile, outPath, this._gv);

                await task.run();
            }

            public OSGeo.OGR.Envelope _getWatershedExtent() {
                return this.basinExtent;
            }

        // Get the combined watershed + lakes geometry and extent.
        public async Task _getWatershedGeometry() {
            Envelope extent = new Envelope();
            // dissolve basin into a single shape
            var subsNoLakes = Path.Join(this._gv.shapesDir, "subsNoLakes.shp");
            if (!File.Exists(subsNoLakes)) {
                // no lakes in model: use subbasins.shp
                subsNoLakes = this._gv.subbasinsFile;
                if (!File.Exists(subsNoLakes)) {
                    Utils.error(string.Format("Cannot find subbasins shapefile {0} to make grid", subsNoLakes), this._gv.isBatch);
                    return;
                }
            }
            var basinShapefile = Path.Join(this._gv.shapesDir, "basin.shp");
            if (!Utils.isUpToDate(subsNoLakes, basinShapefile)) { // need to create
                var parameters = Geoprocessing.MakeValueArray(subsNoLakes, basinShapefile);
                var environments = new Dictionary<string, string> {
                    { "parallelProcessingFactor", "100" }   // Full parallel processing by default
                };
                var gpResult = await Geoprocessing.ExecuteToolAsync(
                    "Analysis.PairwiseDissolve",
                    parameters,
                    environments);
                // don't want this added as a map layer, so remove
                await Utils.removeLayer(basinShapefile);
            }
            using (var driver = Ogr.GetDriverByName("ESRI Shapefile")) {
                DataSource basinDs = driver.Open(basinShapefile, 0);
                var layer = basinDs.GetLayerByIndex(0);
                var feature = layer.GetNextFeature(); // there should only be one
                var geom = feature.GetGeometryRef();
                layer.GetExtent(extent, 0);
                this.basinGeometry = geom;
                this.basinExtent = extent;
            }
        }




        // Get stream vertex coordinates as array for KDTree.
        public virtual List<IReadOnlyList<double>> _getStreamCoords() {
            var channelFile = this._gv.channelFile;
            var coords = new List<IReadOnlyList<double>>();
            if (!File.Exists(channelFile)) {
                return coords;
            }
            using (var driver = Ogr.GetDriverByName("ESRI Shapefile")) {
                DataSource ds = driver.Open(channelFile, 0);
                var layer = ds.GetLayerByIndex(0);
                OSGeo.OGR.Feature feat = null;
                layer.ResetReading();
                while (true) {
                    feat = layer.GetNextFeature();
                    if (feat == null) { break; }
                    var geom = feat.GetGeometryRef();
                    if (geom is null) { continue; }
                    double[] pt = new double[3];
                    var numPoints = geom.GetPointCount();
                    for (int i = 0; i < numPoints; i++) {
                        geom.GetPoint(i, pt);
                        coords.Add(new List<double> { pt[0], pt[1] });
                    }
                }
                return coords;
            }
        }

        // Extract vertices from the outer boundary only, not internal subbasin edges.
        public static List<IReadOnlyList<double>> _getBoundaryCoords(Geometry wsGeom) {
            var coords = new List<IReadOnlyList<double>>();
            double[] pt = new double[3];
            var numPoints = wsGeom.GetPointCount();
            for (int i = 0; i < numPoints; i++) {
                wsGeom.GetPoint(i, pt);
                coords.Add(new List<double> { pt[0], pt[1] });
            }
            return coords;
        }

            // Get observation well locations from the outlets layer.
            public virtual List<IReadOnlyList<double>> _getWellCoords() {
                // Wells are deliberately excluded from the snap file, so read the source outlets file.
                var outletFile = this._gv.outletFile;
            var coords = new List<IReadOnlyList<double>>();
            if (!File.Exists(outletFile)) {
                    return coords;
            }
            using (var driver = Ogr.GetDriverByName("ESRI Shapefile")) {
                DataSource ds = driver.Open(outletFile, 0);
                var layer = ds.GetLayerByIndex(0);
                OSGeo.OGR.Feature feat = null;
                layer.ResetReading();
                while (true) {
                    feat = layer.GetNextFeature();
                    if (feat == null) { break; }
                    var geom = feat.GetGeometryRef();
                    if (geom is null) { continue; }
                    var isWell = (WaterRole)feat.GetFieldAsInteger(Topology._RES) == WaterRole._WELLTYPE;
                    var isPointSource = feat.GetFieldAsInteger(Topology._PTSOURCE) == 1;
                    if (isWell && !isPointSource) {
                        double[] pt = new double[3];
                        geom.GetPoint(0, pt);
                        coords.Add(new List<double> { pt[0], pt[1] });
                    }
                }
                return coords;
            }
        }

        //// Remove map layers whose source is outPath. On Windows the OGR provider
        ////         holds an exclusive file lock that blocks rewriting until the layer is gone.
        //[staticmethod]
        //public static object _releaseLayerLocks(GridGenerator outPath) {
        //    var project = QgsProject.instance();
        //    var targetNorm = os.path.normcase(os.path.normpath(outPath));
        //    var toRemove = new List<object>();
        //    foreach (var layer in project.mapLayers().values()) {
        //        try {
        //            var src = layer.dataProvider().dataSourceUri();
        //        } catch (Exception) {
        //            continue;
        //        }
        //        var srcPath = src.split("|", 1)[0];
        //        if (os.path.normcase(os.path.normpath(srcPath)) == targetNorm) {
        //            toRemove.append(layer.id());
        //        }
        //    }
        //    foreach (var lid in toRemove) {
        //        project.removeMapLayer(lid);
        //    }
        //    if (toRemove) {
        //        _log("Released {0} layer(s) holding {1}".format(toRemove.Count, outPath));
        //    }
        //}

        // TODO
        //public virtual void _onComplete(object shapePath) {
        //    if (!os.path.isfile(shapePath)) {
        //        return;
        //    }
        //    var project = QgsProject.instance();
        //    foreach (var layer in project.mapLayersByName(_LAYER_NAME)) {
        //        project.removeMapLayer(layer.id());
        //    }
        //    var newLayer = QgsVectorLayer(shapePath, _LAYER_NAME, "ogr");
        //    if (newLayer.isValid()) {
        //        newLayer.setCustomProperty("layerId", _LAYER_ID);
        //        // Apply style
        //        var qmlPath = os.path.join(os.path.dirname(os.path.abspath(@__file__)), "gwflowcells.qml");
        //        if (os.path.isfile(qmlPath)) {
        //            newLayer.loadNamedStyle(qmlPath);
        //        }
        //        project.addMapLayer(newLayer);
        //        GridGenerator.setGwflowMode(true);
        //    }
        //}


        // TODO
        //public static void removeGwflowLayer() {
        //    var project = QgsProject.instance();
        //    var root = project.layerTreeRoot();
        //    foreach (var layer in project.mapLayersByName(_LAYER_NAME)) {
        //        project.removeMapLayer(layer.id());
        //    }
        //    foreach (var treeLayer in root.findLayers()) {
        //        var layer = treeLayer.layer();
        //        if (layer && layer.name().startswith(_SUBBASINS_NAME)) {
        //            treeLayer.setItemVisibilityChecked(true);
        //        }
        //    }
        //}

        // TODO
        //public virtual void cleanup() {
        //    this._timer.stop();
        //    try {
        //        if (this._task && this._task.status() == QgsTask.Running) {
        //            this._task.cancel();
        //        }
        //    } catch (RuntimeError) {
        //    }
        //}
    }

        // Background task that builds grid cells clipped to the watershed.
        public class _GridTask
    {
//            : QgsTask {


            public string demFile;

            public Envelope extent;

            public int numProcesses;

            public string outPath;

            public HRUsForm hf;

            public List<IReadOnlyList<double>> refineCoords;

            public List<OSGeo.OGR.Feature> resultFeatures;

            public Geometry wsGeom;

        public GlobalVars _gv;

            public _GridTask(
                Envelope extent,
                Geometry wsGeom,
                HRUsForm hf,
                List<IReadOnlyList<double>> refineCoords,
                string demFile,
                string outPath,
                GlobalVars _gv)
                {
                this.extent = extent;
                this.wsGeom = wsGeom;
            this.hf = hf;
                this.refineCoords = refineCoords;
                this.demFile = demFile;
                this.outPath = outPath;
                this.resultFeatures = new List<OSGeo.OGR.Feature>();
            this._gv = _gv;
            }

        public async Task<bool> run() {
            int cellCount = 0;
            try {
                await Utils.removeLayerAndFiles(this.outPath);
                using (var driver = Ogr.GetDriverByName("ESRI Shapefile")) {
                    DataSource ds = driver.CreateDataSource(this.outPath, null);
                    var srsWkt = this._gv.topo.crsProject.Wkt;
                    OSGeo.OSR.SpatialReference srs = new OSGeo.OSR.SpatialReference(srsWkt);
                    OSGeo.OGR.Layer gridLayer = ds.CreateLayer("gwGrid", srs, wkbGeometryType.wkbMultiPolygon, null);
                    gridLayer.CreateField(new FieldDefn(GridGenerator._CELL_ID, OSGeo.OGR.FieldType.OFTInteger), 1);
                    gridLayer.CreateField(new FieldDefn(GridGenerator._CELL_AREA, OSGeo.OGR.FieldType.OFTReal), 1);
                    Utils.copyPrj(this._gv.demFile, this.outPath);
                    var defn = gridLayer.GetLayerDefn();
                    var idIndex = gridLayer.FindFieldIndex(GridGenerator._CELL_ID, 1);
                    var areaIndex = gridLayer.FindFieldIndex(GridGenerator._CELL_AREA, 1);
                    if (this.hf.isStructured) {
                        cellCount = this._buildStructured(gridLayer, defn, idIndex, areaIndex);
                    } else {
                        cellCount = this._buildUnstructured(gridLayer, defn, idIndex, areaIndex);
                    }
                    //Utils.loginfo(string.Format("Build phase complete: ok={0} features={1}", ok, this.resultFeatures.Count));
                    if (cellCount > 0) {
                        Utils.loginfo(string.Format("Grid construction: success={0}", true));
                        var cellsLayer = (await Utils.getLayerByFilename(this.outPath, FileTypes._GWFLOWCELLS, this._gv, null, Utils._WATERSHED_GROUP_NAME)).Item1 as FeatureLayer;
                        if (cellsLayer is null) {
                            Utils.error(string.Format("Failed to load gwflow cells shapefile {0}", this.outPath), this._gv.isBatch);
                            return false;
                        }
                        await setGwflowMode(true);
                        return true;
                    } else {
                        Utils.information("Grid construction produced no features.", this._gv.isBatch);
                        return false;
                    }
                }
            }
            catch (Exception exc) {
                Utils.error(string.Format("Exception in grid construction: {0}", exc.Message), this._gv.isBatch);
                return false;
            }
        }

        public virtual int _buildStructured(OSGeo.OGR.Layer gridLayer, FeatureDefn defn, int idIndex, int areaIndex) {
            var cellSize = hf._cellSize;
            if (cellSize <= 0) {
                return 0;
            }
            var minX = this.extent.MinX;
            var minY = this.extent.MinY;
            var maxX = this.extent.MaxX;
            var maxY = this.extent.MaxY;
            var cols = (int)Math.Ceiling((maxX - minX) / cellSize);
            var rows = (int)Math.Ceiling((maxY - minY) / cellSize);
            Envelope wsBbox = new Envelope();
            if (this.wsGeom is not null) wsGeom.GetEnvelope(wsBbox);
            var cellId = 1;
            foreach (var row in Enumerable.Range(0, rows)) {
                //if (this.isCanceled()) {
                //    return false;
                //}
                foreach (var col in Enumerable.Range(0, cols)) {
                    var x0 = minX + col * cellSize;
                    var y0 = minY + row * cellSize;
                    var x1 = x0 + cellSize;
                    var y1 = y0 + cellSize;
                    // no need to acually create this
                    //var cellRect = new Envelope();
                    //cellRect.MinX = x0;
                    //cellRect.MinY = y0;
                    //cellRect.MaxX = x1;
                    //cellRect.MaxY = y1;
                    var disjoint = x0 > maxX || x1 < minX || y0 > maxY || y1 < minY;
                    if (this.wsGeom is not null && disjoint) {
                        continue;
                    }
                    var wkt = string.Format("POLYGON (({0} {1}, {2} {1}, {2} {3}, {0} {3}, {0} {1}))", x0, y0, x1, y1);
                    var cellGeom = OSGeo.OGR.Geometry.CreateFromWkt(wkt);
                    if (this.wsGeom is not null && !cellGeom.Intersects(this.wsGeom)) {
                        continue;
                    }
                    var feature = new OSGeo.OGR.Feature(defn);
                    feature.SetField(idIndex, cellId);
                    feature.SetField(areaIndex, cellSize * cellSize);
                    feature.SetGeometry(cellGeom);
                    var err = gridLayer.CreateFeature(feature);
                    if (err != Ogr.OGRERR_NONE) {
                        Utils.error(string.Format("Unable to add feature to grid shapefile {0}", this.outPath), this._gv.isBatch);
                        return 0;
                    }
                    cellId += 1;
                }
            }
            return cellId - 1; // cell count
        }

        // Balanced quadtree grid using KDTree distance queries, DEM variance, and 2:1 balancing.
        public virtual int _buildUnstructured(OSGeo.OGR.Layer gridLayer, FeatureDefn defn, int idIndex, int areaIndex) {
            var maxSize = this.hf._maxCellSize;
            var levels = this.hf._levels;
            var rate = this.hf.transitionRate;
            if (maxSize <= 0 || levels < 1) {
                return 0;
            }
            var minSize = maxSize / Math.Pow(2, levels);
            var minX = this.extent.MinX;
            var minY = this.extent.MinY;
            var maxX = this.extent.MaxX;
            var maxY = this.extent.MaxY;
            int half;
            double halfDiag;
            // metric: square of Euclidean distance
            Func<double[], double[], double> L2Norm = (p0, p1) => {
                var xDiff = p0[0] - p1[0];
                var yDiff = p0[1] - p1[1];
                return xDiff * xDiff + yDiff * yDiff;
            };
            // Build KDTree from refinement points (streams + boundary)
            KDTree<double, double, int> refineTree = null;
            List<IReadOnlyList<double>> coords = this.refineCoords;
            int[] nodes = coords.Select(p => 0).ToArray(); // dummy.  Failed to find library not using nodes
            if (coords.Count > 0) {
                refineTree = KDTree.Create<double, int>(coords, nodes, DistanceMetrics.EuclideanDistance);
            }
            OSGeo.GDAL.Dataset demDs = null;
            Band demBand = null;
            double[] demTransform = new double[6];
            // Open DEM for topographic refinement
            if (Path.Exists(this.demFile)) {
                demDs = Gdal.Open(this.demFile, 0);
                if (demDs is not null) {
                    demBand = demDs.GetRasterBand(1);
                    demDs.GetGeoTransform(demTransform);
                }
            }
            // Phase 1: quadtree refinement
            var x0 = (int)Math.Floor(minX / maxSize) * maxSize;
            var y0 = (int)Math.Floor(minY / maxSize) * maxSize;
            var queue = new List<(int, int, int)>();
            for (int y = y0; y < maxY; y += maxSize) {
                for (int x = x0; x < maxX; x += maxSize) {
                    queue.Add((x, y, maxSize));
                }
            }
            //var xs = np.arange(x0, maxX + maxSize, maxSize);
            //var ys = np.arange(y0, maxY + maxSize, maxSize);
            //var queue = (from y in ys[: -1:]
            //from x in xs[: -1:]
            //select (x, y, maxSize)).ToList();
            var cellDict = new Dictionary<(int, int, int), bool>();
            Envelope wsBbox = new OSGeo.OGR.Envelope();
            wsGeom.GetEnvelope(wsBbox);
            while (queue.Count > 0) {
                //if (this.isCanceled()) {
                //    demDs = null;
                //    return false;
                //}
                var (cx, cy, sz) = queue[0];
                queue.RemoveAt(0);
                // no need to actually create this
                //var cellRect = new Envelope() {
                //    MinX = cx,
                //    MinY = cy,
                //    MaxX = cx + sz,
                //    MaxY = cy + sz
                //};
                var disjoint = cx > wsBbox.MaxX || cy > wsBbox.MaxY || cx + sz < wsBbox.MinX || cy + sz < wsBbox.MinY;
                if (disjoint) { continue; }
                var wkt = string.Format("POLYGON (({0} {1}, {2} {1}, {2} {3}, {0} {3}, {0} {1}))", cx, cy, cx + sz, cy + sz);
                var cellGeom = OSGeo.OGR.Geometry.CreateFromWkt(wkt);
                if (!cellGeom.Intersects(this.wsGeom)) {
                    continue;
                }
                half = sz / 2;
                halfDiag = sz * Math.Sqrt(2) / 2;
                var shouldRefine = false;
                // Distance-based refinement (streams, boundary, wells)
                if (refineTree is not null && half >= minSize) {
                    var cxMid = cx + half;
                    var cyMid = cy + half;
                    var nearest = refineTree.NearestNeighbors(new List<double> {
                        cxMid,
                        cyMid
                    }, 1).FirstOrDefault().Item1;
                    var xDiff = cxMid - nearest[0];
                    var yDiff = cyMid - nearest[1];
                    var dCentroid = Math.Sqrt(xDiff * xDiff + yDiff * yDiff);
                    var d = Math.Max(0.0, dCentroid - halfDiag);
                    var t = 1.0 - Math.Exp(-rate * d / maxSize);
                    var target = minSize + (maxSize - minSize) * t;
                    if (half >= target) {
                        shouldRefine = true;
                    }
                }
                // DEM-based refinement: subdivide where terrain is variable
                if (!shouldRefine && demBand is not null && half >= minSize) {
                    var elevRange = _demElevRange(demBand, demTransform, cx, cy, maxSize);
                    var elevTol = this.hf._elevTolerance;
                    if (elevRange > elevTol) {
                        shouldRefine = true;
                    }
                }
                if (shouldRefine && half >= minSize) {
                    queue.Add((cx, cy, half));
                    queue.Add((cx + half, cy, half));
                    queue.Add((cx, cy + half, half));
                    queue.Add((cx + half, cy + half, half));
                    continue;
                }
                cellDict[(cx, cy, sz)] = true;
            }
            demDs = null;
            // Phase 2: 2:1 balancing
            var eps = Convert.ToInt32(minSize * 0.1);
            (int, int, int) findCellAt(double px, double py) {
                int sz = Convert.ToInt32(minSize);
                while (sz <= maxSize) {
                    int cx = (int)Math.Floor(px / sz) * sz;
                    int cy = (int)Math.Floor(py / sz) * sz;
                    if (cellDict.ContainsKey((cx, cy, sz))) {
                        return (cx, cy, sz);
                    }
                    sz *= 2;
                }
                return (-1, -1, -1);
            }
            var changed = true;
            while (changed) {
                //if (this.isCanceled()) {
                //    return false;
                //}
                changed = false;
                var toSplit = new List<(int, int, int)>();
                foreach (var (cx, cy, sz) in cellDict.Keys) {
                    half = sz / 2;
                    if (half < minSize) {
                        continue;
                    }
                    var mid = sz / 2;
                    var probes = new List<(int, int)> {
                        (cx + mid, cy - eps),
                        (cx + mid, cy + sz + eps),
                        (cx - eps, cy + mid),
                        (cx + sz + eps, cy + mid)
                    };
                    foreach (var (px, py) in probes) {
                        var nb = findCellAt(px, py);
                        if (nb.Item1 >= 0 && sz > 2 * nb.Item3) {
                            toSplit.Add((cx, cy, sz));
                            break;
                        }
                    }
                }
                foreach (var (cx, cy, sz) in toSplit) {
                    if (!cellDict.ContainsKey((cx, cy, sz))) {
                        continue;
                    }
                    half = sz / 2;
                    if (half < minSize) {
                        continue;
                    }
                    cellDict.Remove((cx, cy, sz));
                    foreach (var (dx, dy) in new List<(int, int)> {
                        (0, 0),
                        (half, 0),
                        (0, half),
                        (half, half)
                    }) {

                        var wkt = string.Format("POLYGON (({0} {1}, {2} {1}, {2} {3}, {0} {3}, {0} {1}))", cx + dx, cy + dy, cx + dx + half, cy + dy + half);
                        var childRect = OSGeo.OGR.Geometry.CreateFromWkt(wkt);
                        if (!childRect.Intersects(this.wsGeom)) {
                            continue;
                        }
                        cellDict[(cx + dx, cy + dy, half)] = true;
                    }
                    changed = true;
                }
            }
            // Build features: keep full cells if they touch the watershed
            var cellId = 1;
            foreach (var (cx, cy, sz) in cellDict.Keys) {
                //if (this.isCanceled()) {
                //    return false;
                //}
                var feature = new OSGeo.OGR.Feature(defn);
                feature.SetField(idIndex, cellId);
                feature.SetField(areaIndex, sz * sz);
                var wkt = string.Format("POLYGON (({0} {1}, {2} {1}, {2} {3}, {0} {3}, {0} {1}))", cx, cy, cx + sz, cy + sz);
                var cellGeom = OSGeo.OGR.Geometry.CreateFromWkt(wkt);
                feature.SetGeometry(cellGeom);
                var err = gridLayer.CreateFeature(feature);
                if (err != Ogr.OGRERR_NONE) {
                    Utils.error(string.Format("Unable to add feature to grid shapefile {0}", this.outPath), this._gv.isBatch);
                    return 0;
                }
                cellId += 1;
            }
            return cellId - 1;  // cell count
        }

            // Sample DEM at cell corners and center, return elevation range or 0.
            public static double _demElevRange(
                Band band,
                double[] transform,
                double cx,
                double cy,
                double sz) {
            var originX = transform[0];
            var pixelW = transform[1];
            var originY = transform[3];
            var pixelH = transform[5];
                var cols = band.XSize;
                var rows = band.YSize;
                var points = new List<(double, double)> {
                (cx, cy),
                (cx + sz, cy),
                (cx, cy + sz),
                (cx + sz, cy + sz),
                (cx + sz / 2, cy + sz / 2)
            };
                var elevs = new List<double>();
                foreach (var (px, py) in points) {
                        try {

                    var val = Topology.valueAtPoint(new double[] { px, py, 0 }, transform, band);
                                if (val > -9999) {
                                    elevs.Add(val);
                                }
                        } catch (Exception) {
                        }
                    }
                if (elevs.Count >= 2) {
                    return Math.Abs(elevs.Max() - elevs.Min());
                }
                return 0;
        }

        public async Task setGwflowMode(bool gridGenActive) {
            var layers = ArcSWATPlus.mainMap.GetLayersAsFlattenedList().Where(l => l is FeatureLayer || l is RasterLayer);
            foreach (var layer in layers) {
                if (layer.Name.StartsWith(GridGenerator._SUBBASINS_NAME) || layer.Name.Contains(GridGenerator._LSUS_NAME)) {
                    await QueuedTask.Run(() => {
                        layer.SetVisibility(!gridGenActive);
                    });
                } else if (layer.Name.StartsWith(GridGenerator._LAYER_NAME)) {
                    await QueuedTask.Run(() => {
                        layer.SetVisibility(gridGenActive);
                    });
                }
            }
        }
    }
    }
