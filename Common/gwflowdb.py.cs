
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.Analyst3D;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Core.Internal.CIM;
using ArcGIS.Desktop.Core.Utilities;
using ArcGIS.Desktop.Internal.KnowledgeGraph;
using ArcGIS.Desktop.Internal.TaskAssistant;
using ImageMagick;
using OSGeo.GDAL;
using OSGeo.OGR;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Transactions;
using Path = System.IO.Path;


namespace ArcSWATPlus {

    // Creates and populates gwflow database tables.
    public class GwflowDB {

        // Database table creation and population for the gwflow module.

        //Creates tables per gwflowDatabaseDesign.md and populates them from:
        //- Spatial intersections with HRUs, channels, LSUs, lakes
        //- DEM sampling for cell elevations
        //- Aquifer thickness/permeability rasters and shapefiles



        //public static bool _mpExecutableConfigured = false;

        //// Make multiprocessing workers spawn the bundled Python, not QGIS.
        //// 
        ////     Under QGIS on Windows sys.executable is qgis-bin.exe, so the default
        ////     'spawn' start method relaunches a whole new QGIS instance for every worker.
        ////     Pointing multiprocessing at pythonw.exe in the Python prefix
        ////     (sys.exec_prefix) makes it start plain Python processes instead.
        ////     Does nothing on non-Windows platforms (which use 'fork') and runs once.
        ////     
        //public static void _configureMultiprocessing() {
        //    if (_mpExecutableConfigured) {
        //        return;
        //    }
        //    if (sys.platform == "win32") {
        //        foreach (var exe in ("pythonw.exe", "python.exe")) {
        //            var candidate = os.path.join(sys.exec_prefix, exe);
        //            if (File.Exists(candidate)) {
        //                multiprocessing.set_executable(candidate);
        //                break;
        //            }
        //        }
        //    }
        //    _mpExecutableConfigured = true;
        //}

        //// Worker: find touching neighbors for a chunk of cells using WKB geometry.
        //public static List<object> _find_connections_chunk(object args) {
        //    (chunk_fids, all_wkbs, all_cellids, all_bboxes) = args;
        //    var results = new List<object>();
        //    // Build a simple spatial lookup: cells indexed by grid position
        //    foreach (var fid in chunk_fids) {
        //        var geom = wkb.loads(all_wkbs[fid]);
        //        prepare(geom);
        //        var cellId = all_cellids[fid];
        //        var bx = all_bboxes[fid];
        //        foreach (var (other_fid, other_bbox) in all_bboxes.items()) {
        //            if (other_fid == fid) {
        //                continue;
        //            }
        //            // Quick bbox rejection
        //            if (bx[2] < other_bbox[0] || bx[0] > other_bbox[2] || bx[3] < other_bbox[1] || bx[1] > other_bbox[3]) {
        //                continue;
        //            }
        //            var other_geom = wkb.loads(all_wkbs[other_fid]);
        //            if (geom.touches(other_geom)) {
        //                results.append((cellId, all_cellids[other_fid]));
        //            }
        //        }
        //    }
        //    return results;
        //}

        // Worker: compute intersection areas between grid cells and overlay features.
        //public static List<object> _intersect_cells_chunk(object args) {
        //    (cell_data, overlay_wkbs, overlay_ids) = args;
        //    var results = new List<object>();
        //    foreach (var (cellId, cell_wkb) in cell_data) {
        //        var cell_geom = wkb.loads(cell_wkb);
        //        var cell_bbox = cell_geom.bounds;
        //        foreach (var (oid, o_wkb) in zip(overlay_ids, overlay_wkbs)) {
        //            var o_geom = wkb.loads(o_wkb);
        //            var o_bbox = o_geom.bounds;
        //            if (cell_bbox[2] < o_bbox[0] || cell_bbox[0] > o_bbox[2] || cell_bbox[3] < o_bbox[1] || cell_bbox[1] > o_bbox[3]) {
        //                continue;
        //            }
        //            if (cell_geom.intersects(o_geom)) {
        //                var inter = cell_geom.intersection(o_geom);
        //                var area = inter.area;
        //                if (area > 0) {
        //                    results.append((cellId, oid, area));
        //                }
        //            }
        //        }
        //    }
        //    return results;
        //}

        public static Dictionary<string, string> _TABLES = new Dictionary<string, string> {
        {
            "codes_gw",
            @"CREATE TABLE IF NOT EXISTS codes_gw (
        id INTEGER PRIMARY KEY DEFAULT 1,
        grid_type TEXT DEFAULT 'structured',
        cell_size REAL DEFAULT 200,
        num_rows INTEGER DEFAULT 0,
        num_cols INTEGER DEFAULT 0,
        num_cells INTEGER DEFAULT 0,
        boundary_condition INTEGER DEFAULT 2,
        recharge_type INTEGER DEFAULT 2,
        gw_soil_transfer INTEGER DEFAULT 1,
        saturation_excess INTEGER DEFAULT 1,
        external_pumping INTEGER DEFAULT 0,
        tile_drainage INTEGER DEFAULT 0,
        reservoir_exchange INTEGER DEFAULT 1,
        wetland_exchange INTEGER DEFAULT 1,
        floodplain_exchange INTEGER DEFAULT 1,
        canal_seepage INTEGER DEFAULT 0,
        solute_transport INTEGER DEFAULT 0,
        timestep_days REAL DEFAULT 1.0,
        daily_output INTEGER DEFAULT 1,
        monthly_output INTEGER DEFAULT 0,
        annual_output INTEGER DEFAULT 1,
        aa_output INTEGER DEFAULT 1,
        river_depth REAL DEFAULT 5.0,
        tile_depth REAL DEFAULT 1.22,
        tile_area REAL DEFAULT 50,
        tile_k REAL DEFAULT 5.0,
        resbed_thickness REAL DEFAULT 2.0,
        resbed_k REAL DEFAULT 9.99e-6,
        wet_thickness REAL DEFAULT 0.25,
        transport_steps INTEGER DEFAULT 1,
        disp_coef REAL DEFAULT 5.0,
        detail_row INTEGER DEFAULT 0,
        detail_col INTEGER DEFAULT 0,
        heat_transport INTEGER DEFAULT 0
    )"},
        {
            "zones_gw",
            @"CREATE TABLE IF NOT EXISTS zones_gw (
        zone_id INTEGER PRIMARY KEY,
        aquifer_k REAL,
        specific_yield REAL DEFAULT 0.2,
        streambed_k REAL DEFAULT 0.005,
        streambed_thickness REAL DEFAULT 0.5,
        thermal_K REAL DEFAULT 0
    )"},
        {
            "cells_gw",
            @"CREATE TABLE IF NOT EXISTS cells_gw (
        cell_id INTEGER PRIMARY KEY,
        status INTEGER NOT NULL DEFAULT 1,
        row INTEGER,
        col INTEGER,
        x_centroid REAL NOT NULL,
        y_centroid REAL NOT NULL,
        area REAL NOT NULL,
        elevation REAL NOT NULL DEFAULT 0,
        aquifer_thickness REAL NOT NULL DEFAULT 50,
        zone INTEGER NOT NULL DEFAULT 1 REFERENCES zones_gw (zone_id),
        extinction_depth REAL DEFAULT 1.0,
        initial_head REAL,
        tile INTEGER DEFAULT 0,
        streambed_k REAL,
        streambed_thickness REAL,
        bc_type INTEGER,
        tile_depth REAL,
        tile_area REAL,
        tile_k REAL,
        init_temp REAL,
        gis_id INTEGER
    )"},
        {
            "cellcon_gw",
            @"CREATE TABLE IF NOT EXISTS cellcon_gw (
        cell_id INTEGER NOT NULL REFERENCES cells_gw (cell_id),
        connected_cell_id INTEGER NOT NULL REFERENCES cells_gw (cell_id),
        PRIMARY KEY (cell_id, connected_cell_id)
    )"},
        {
            "hrucell_gw",
            @"CREATE TABLE IF NOT EXISTS hrucell_gw (
        cell_id INTEGER NOT NULL REFERENCES cells_gw (cell_id),
        hru_id INTEGER NOT NULL,
        area_m2 REAL NOT NULL,
        PRIMARY KEY (cell_id, hru_id)
    )"},
        {
            "lsucell_gw",
            @"CREATE TABLE IF NOT EXISTS lsucell_gw (
        cell_id INTEGER NOT NULL REFERENCES cells_gw (cell_id),
        lsu_id INTEGER NOT NULL,
        area_m2 REAL NOT NULL,
        PRIMARY KEY (cell_id, lsu_id)
    )"},
        {
            "chancell_gw",
            @"CREATE TABLE IF NOT EXISTS chancell_gw (
        id INTEGER PRIMARY KEY,
        cell_id INTEGER NOT NULL REFERENCES cells_gw (cell_id),
        channel_id INTEGER NOT NULL,
        bed_elevation REAL NOT NULL,
        length_m REAL NOT NULL,
        zone_id INTEGER NOT NULL DEFAULT 1 REFERENCES zones_gw (zone_id),
        obs INTEGER NOT NULL DEFAULT 0,
        dep_zone INTEGER
    )"},
        {
            "floodplain_gw",
            @"CREATE TABLE IF NOT EXISTS floodplain_gw (
        cell_id INTEGER NOT NULL REFERENCES cells_gw (cell_id),
        channel_id INTEGER NOT NULL,
        area_m2 REAL NOT NULL,
        conductivity REAL NOT NULL DEFAULT 0,
        PRIMARY KEY (cell_id, channel_id)
    )"},
        {
            "rescell_gw",
            @"CREATE TABLE IF NOT EXISTS rescell_gw (
        cell_id INTEGER NOT NULL REFERENCES cells_gw (cell_id),
        reservoir_id INTEGER NOT NULL,
        stage REAL DEFAULT 0,
        PRIMARY KEY (cell_id, reservoir_id)
    )"},
        {
            "pumpex_gw",
            @"CREATE TABLE IF NOT EXISTS pumpex_gw (
        id INTEGER PRIMARY KEY,
        cell_id INTEGER NOT NULL REFERENCES cells_gw (cell_id),
        start_year INTEGER NOT NULL,
        start_day INTEGER NOT NULL,
        end_year INTEGER NOT NULL,
        end_day INTEGER NOT NULL,
        rate_m3day REAL NOT NULL
    )"},
        {
            "obs_gw",
            @"CREATE TABLE IF NOT EXISTS obs_gw (
        cell_id INTEGER NOT NULL REFERENCES cells_gw (cell_id),
        name TEXT,
        PRIMARY KEY (cell_id)
    )"},
        {
            "hru_pump_gw",
            @"CREATE TABLE IF NOT EXISTS hru_pump_gw (
        hru_id INTEGER PRIMARY KEY
    )"},
        {
            "tvheads_gw",
            @"CREATE TABLE IF NOT EXISTS tvheads_gw (
        cell_id INTEGER NOT NULL REFERENCES cells_gw (cell_id),
        year INTEGER NOT NULL,
        head REAL NOT NULL,
        PRIMARY KEY (cell_id, year)
    )"},
        {
            "out_times_gw",
            @"CREATE TABLE IF NOT EXISTS out_times_gw (
        year INTEGER NOT NULL,
        jday INTEGER NOT NULL,
        PRIMARY KEY (year, jday)
    )"},
        {
            "solute_gw",
            @"CREATE TABLE IF NOT EXISTS solute_gw (
        id INTEGER PRIMARY KEY,
        name TEXT NOT NULL,
        sorption_coef REAL DEFAULT 1,
        rate_const REAL DEFAULT 0,
        canal_irr REAL DEFAULT 0,
        init_conc REAL DEFAULT 0
    )"},
        {
            "cell_sol_gw",
            @"CREATE TABLE IF NOT EXISTS cell_sol_gw (
        cell_id INTEGER NOT NULL REFERENCES cells_gw (cell_id),
        solute_id INTEGER NOT NULL REFERENCES solute_gw (id),
        init_conc REAL DEFAULT 0,
        PRIMARY KEY (cell_id, solute_id)
    )"},
        {
            "wetland_gw",
            @"CREATE TABLE IF NOT EXISTS wetland_gw (
        wet_id INTEGER PRIMARY KEY,
        thickness REAL
    )"},
        {
            "sw_group_gw",
            @"CREATE TABLE IF NOT EXISTS sw_group_gw (
        group_id INTEGER NOT NULL,
        cell_id INTEGER NOT NULL REFERENCES cells_gw (cell_id),
        PRIMARY KEY (group_id, cell_id)
    )"},
        {
            "ponds_gw",
            @"CREATE TABLE IF NOT EXISTS ponds_gw (
        id INTEGER PRIMARY KEY,
        area REAL,
        chan INTEGER,
        canal INTEGER,
        unl INTEGER,
        bed_k REAL,
        wsta INTEGER,
        evap_co REAL,
        start_yr INTEGER,
        start_mo INTEGER,
        start_day INTEGER
    )"},
        {
            "pond_solute_gw",
            @"CREATE TABLE IF NOT EXISTS pond_solute_gw (
        pond_id INTEGER NOT NULL REFERENCES ponds_gw (id),
        solute_idx INTEGER NOT NULL,
        unl_conc REAL DEFAULT 0,
        PRIMARY KEY (pond_id, solute_idx)
    )"},
        {
            "pond_cell_gw",
            @"CREATE TABLE IF NOT EXISTS pond_cell_gw (
        pond_id INTEGER NOT NULL REFERENCES ponds_gw (id),
        cell_id INTEGER NOT NULL REFERENCES cells_gw (cell_id),
        conn_area REAL,
        PRIMARY KEY (pond_id, cell_id)
    )"},
        {
            "pond_div_gw",
            @"CREATE TABLE IF NOT EXISTS pond_div_gw (
        pond_id INTEGER NOT NULL REFERENCES ponds_gw (id),
        year INTEGER NOT NULL,
        month INTEGER NOT NULL,
        day INTEGER NOT NULL,
        div REAL NOT NULL DEFAULT 0,
        PRIMARY KEY (pond_id, year, month, day)
    )"},
        {
            "phreato_gw",
            @"CREATE TABLE IF NOT EXISTS phreato_gw (
        id INTEGER PRIMARY KEY,
        depth REAL NOT NULL,
        et_rate REAL NOT NULL
    )"},
        {
            "phreato_cell_gw",
            @"CREATE TABLE IF NOT EXISTS phreato_cell_gw (
        cell_id INTEGER PRIMARY KEY REFERENCES cells_gw (cell_id),
        area REAL
    )"},
        {
            "chan_depth_gw",
            @"CREATE TABLE IF NOT EXISTS chan_depth_gw (
        year INTEGER NOT NULL,
        jday INTEGER NOT NULL,
        zone_idx INTEGER NOT NULL,
        depth REAL NOT NULL,
        PRIMARY KEY (year, jday, zone_idx)
    )"}};

        public static List<string> _DROP_ORDER = new List<string> {
        "pond_div_gw",
        "pond_solute_gw",
        "pond_cell_gw",
        "ponds_gw",
        "phreato_cell_gw",
        "phreato_gw",
        "sw_group_gw",
        "chan_depth_gw",
        "cell_sol_gw",
        "solute_gw",
        "out_times_gw",
        "gwflow_init_conc",
        "gwflow_solutes",
        "gwflow_out_days",
        "tvheads_gw",
        "hru_pump_gw",
        "obs_gw",
        "gwflow_obs_locs",
        "pumpex_gw",
        "rescell_gw",
        "floodplain_gw",
        "chancell_gw",
        "lsucell_gw",
        "hrucell_gw",
        "cellcon_gw",
        "cells_gw",
        "zones_gw",
        "codes_gw",
        "wetland_gw",
        "gwflow_chancell_obs",
        "gwflow_chancell",
        "gwflow_rivcell",
        "gwflow_grid",
        "gwflow_cell",
        "gwflow_cell_connection",
        "gwflow_hrucell",
        "gwflow_lsucell",
        "gwflow_fpcell",
        "gwflow_rescell",
        "gwflow_pump",
        "gwflow_hru_pump_obs",
        "gwflow_tvhead",
        "gwflow_zone",
        "gwflow_config",
        "gwflow_base",
        "gwflow_wetland"
    };


        public GlobalVars _gv;

        public HRUsForm _panel;

        public OSGeo.OGR.Geometry basinGeometry;

        public Action<string> _progress;

        public System.Windows.Forms.ProgressBar _progressBar;

        public GwflowDB(GlobalVars gv, HRUsForm panel) {
            this._gv = gv;
            this._panel = panel;
            this.basinGeometry = this._panel._gridGen.basinGeometry;
            this._progress = panel is not null ? panel.progress : static msg => { };
            this._progressBar = panel is not null ? panel.getProgressBar : null;
        }

        public class GridData {
            public Dictionary<int, OSGeo.OGR.Geometry> geoms;
            public Dictionary<int, int> cellids;
            public Dictionary<int, OSGeo.OGR.Envelope> bboxes;
            public OSGeo.OGR.Envelope extent;
        }

        // Create tables, populate cells and intersections.
        public async Task<bool> run() {
            //_configureMultiprocessing();
            var gridPath = Path.Join(this._gv.shapesDir, "gwflowcells.shp");
            if (!Path.Exists(gridPath)) {
                Utils.error("gwflow grid not found. Click Generate Grid first.", this._gv.isBatch, logFile: this._gv.logFile);
                return false;
            }
            Layer gridLayer = null;
            using (var driver = Ogr.GetDriverByName("ESRI Shapefile")) {
                try {
                    DataSource ds = driver.Open(gridPath, 0);
                    gridLayer = ds.GetLayerByIndex(0);
                }
                catch (Exception ex) {
                    Utils.error(string.Format("Failed to load gwflow grid {0}: {1}", gridPath, ex.Message), this._gv.isBatch, logFile: this._gv.logFile);
                    return false;
                }
                // collect data from gidLayer for use in several functions below
                GridData gridData = new();
                gridData.geoms = new Dictionary<int, OSGeo.OGR.Geometry> { };
                gridData.cellids = new Dictionary<int, int> { };
                gridData.bboxes = new Dictionary<int, OSGeo.OGR.Envelope> { };
                if (this._panel.isStructured) {
                    gridData.extent = new OSGeo.OGR.Envelope();
                    gridLayer.GetExtent(gridData.extent, 1);
                } else {
                    gridData.extent = null;
                }
                gridLayer.ResetReading();
                OSGeo.OGR.Feature feat = null;
                // create index running from 0 as index for task arrays later
                var id = 0;
                while (true) {
                    feat = gridLayer.GetNextFeature();
                    if (feat is null) { break; }
                    var geom = feat.GetGeometryRef();
                    //geom.ExportToWkb(all_wkbs[(fid], wkbByteOrder.wkbNDR);
                    gridData.geoms[id] = geom;
                    gridData.cellids[id] = feat.GetFieldAsInteger(GridGenerator._CELL_ID);
                    OSGeo.OGR.Envelope bb = new OSGeo.OGR.Envelope();
                    geom.GetEnvelope(bb);
                    gridData.bboxes[id] = bb;
                    id++;
                }
                var steps = new List<string> {
                "Creating tables",
                "Writing configuration",
                "Populating cells",
                "Building connections",
                "HRU intersections",
                "LSU intersections",
                "Channel intersections",
                "Reservoir intersections",
                "Floodplain intersections",
                "Mapping wells",
                "Writing solutes",
                "Done"
                };
                if (this._progressBar is not null) {
                    this._progressBar.Visible = true;
                    this._progressBar.Maximum = steps.Count;
                    this._progressBar.Value = 0;
                }
                void step(int i) {
                    if (this._progressBar is not null) {
                        this._progressBar.Value = i;
                        this._progress(steps[i]);
                    }
                }
                step(0);
                this._createTables();
                step(1);
                this._populateConfig(gridData);
                this._populateZones();
                step(2);
                this._populateCells(gridData);
                // num_cells = active cell count (ncell) = rows in cells_gw. num_rows/
                // num_cols are full-rectangle metadata; backfill from active cells only
                // if _populateConfig left them 0 (e.g. unstructured).
                var reader = this._gv.db.getReader("SELECT COUNT(*) FROM cells_gw");
                reader.Read();
                int numCells = reader.GetInt32(0);
                reader = this._gv.db.getReader("SELECT MAX(row) FROM cells_gw");
                reader.Read();
                int maxRow = reader.GetInt32(0);
                reader = this._gv.db.getReader("SELECT MAX(col) FROM cells_gw");
                reader.Read();
                int maxCol = reader.GetInt32(0);

                this._gv.db.execNonQuery("UPDATE codes_gw SET num_cells=? WHERE id=1", [numCells]);
                this._gv.db.execNonQuery("UPDATE codes_gw SET num_rows=? WHERE id=1 AND (num_rows IS NULL OR num_rows=0)", [maxRow]);
                this._gv.db.execNonQuery("UPDATE codes_gw SET num_cols=? WHERE id=1 AND (num_cols IS NULL OR num_cols=0)", [maxCol]);
                step(3);
                this._populateCellConnections(gridData);
                step(4);
                this._populateHRUCells(gridData);
                step(5);
                this._populateLSUCells(gridData);
                step(6);
                this._populateChannelCells(gridData);
                step(7);
                this._populateReservoirCells(gridData);
                step(8);
                this._populateFloodplainCells(gridData);
                step(9);
                this._populateObservationWells(gridData);
                step(10);
                this._populateSolutes();
                step(11);
            }
            return true;
        }

        public virtual void _createTables() {
            using (var transaction = this._gv.db.conn.BeginTransaction()) {
                this._progress("Creating gwflow tables");
                foreach (var table in _DROP_ORDER) {
                    this._gv.db.execNonQuery(string.Format("DROP TABLE IF EXISTS {0}", table), transaction: transaction);
                }
                foreach (var name in new List<string> {
                "codes_gw",
                "zones_gw",
                "cells_gw",
                "cellcon_gw",
                "hrucell_gw",
                "lsucell_gw",
                "chancell_gw",
                "floodplain_gw",
                "rescell_gw",
                "pumpex_gw",
                "obs_gw",
                "hru_pump_gw",
                "tvheads_gw",
                "out_times_gw",
                "solute_gw",
                "cell_sol_gw",
                "wetland_gw",
                "sw_group_gw",
                "ponds_gw",
                "pond_solute_gw",
                "pond_cell_gw",
                "pond_div_gw",
                "phreato_gw",
                "phreato_cell_gw",
                "chan_depth_gw"
            }) {
                    this._gv.db.execNonQuery(_TABLES[name], transaction: transaction);
                }
                this._gv.db.execNonQuery("CREATE INDEX IF NOT EXISTS idx_cells_gw_zone ON cells_gw (zone)", transaction: transaction);
                this._gv.db.execNonQuery("CREATE INDEX IF NOT EXISTS idx_hrucell_gw_hru ON hrucell_gw (hru_id)", transaction: transaction);
                this._gv.db.execNonQuery("CREATE INDEX IF NOT EXISTS idx_lsucell_gw_lsu ON lsucell_gw (lsu_id)", transaction: transaction);
                this._gv.db.execNonQuery("CREATE INDEX IF NOT EXISTS idx_floodplain_gw_channel ON floodplain_gw (channel_id)", transaction: transaction);
                this._gv.db.execNonQuery("CREATE INDEX IF NOT EXISTS idx_rescell_gw_res ON rescell_gw (reservoir_id)", transaction: transaction);
                this._gv.db.execNonQuery("CREATE INDEX IF NOT EXISTS idx_chancell_gw_cell ON chancell_gw (cell_id)", transaction: transaction);
                this._gv.db.execNonQuery("CREATE INDEX IF NOT EXISTS idx_chancell_gw_channel ON chancell_gw (channel_id)", transaction: transaction);
            transaction.Commit();
            }
        }


        // Write single config row with defaults from panel.
        public virtual void _populateConfig(GridData gridData) {
            using (var transaction = this._gv.db.conn.BeginTransaction()) {
                this._progress("Writing gwflow configuration");
                var panel = this._panel;
                var gridType = panel.isStructured ? "structured" : "unstructured";
                var cellSize = gridType == "structured" ? panel._cellSize : panel._maxCellSize;
                var useTile = panel._useTileDrains ? 1 : 0;
                var numRows = 0;
                var numCols = 0;
                var extent = gridData.extent;
                if (gridType == "structured") {
                    var cs = panel._cellSize;
                    if (cs > 0) {
                        numRows = Convert.ToInt32(Math.Ceiling((extent.MaxY - extent.MinY) / cs));
                        numCols = Convert.ToInt32(Math.Ceiling((extent.MaxX - extent.MinX) / cs));
                    }
                }
                this._gv.db.execNonQuery(@"INSERT INTO codes_gw
                        (id, grid_type, cell_size, num_rows, num_cols, tile_drainage, recharge_type)
                        VALUES (1, ?, ?, ?, ?, ?, 1)", [gridType, cellSize, numRows, numCols, useTile], transaction: transaction);
                transaction.Commit();
            }
        }

        // Create zones from aquifer permeability shapefile.
        public virtual void _populateZones() {
            using (var transaction = this._gv.db.conn.BeginTransaction()) {
                this._progress("Processing aquifer zones");
                var permFile = this._panel._aquiferPermeability;
            if (!File.Exists(permFile)) {
                this._gv.db.execNonQuery("INSERT INTO zones_gw (zone_id, aquifer_k) VALUES (1, 1.0)", transaction: transaction);
                return;
            }

                using (var driver = Ogr.GetDriverByName("ESRI Shapefile")) {
                    var names = new List<string> { "k", "k_mday", "k_m_day", "hydc", "conductivity", "perm" };
                    var ds = driver.Open(permFile, 0);
                    var permLayer = ds.GetLayerByIndex(0);
                    var defn = permLayer.GetLayerDefn();
                    // Find K field
                    var kFieldIdx = -1;
                    for (int i = 0; i < defn.GetFieldCount(); i++) {
                        var field = defn.GetFieldDefn(i);
                        var name = field.GetName().ToLower();
                        if (names.Contains(name)) {
                            kFieldIdx = i;
                            break;
                        }
                    }
                    var zoneId = 0;
                    OSGeo.OGR.Feature feat = null;
                    permLayer.ResetReading();
                    while (true) {
                        feat = permLayer.GetNextFeature();
                        if (feat is null) { break; }
                        zoneId += 1;
                        var kVal = kFieldIdx >= 0 ? feat.GetFieldAsDouble(kFieldIdx) : 1.0;
                        if (kVal == 0) {
                            kVal = 1.0;
                        }
                        this._gv.db.execNonQuery("INSERT INTO zones_gw (zone_id, aquifer_k) VALUES (?, ?)", [zoneId, kVal], transaction: transaction);
                    }
                    if (zoneId == 0) {
                        this._gv.db.execNonQuery("INSERT INTO zones_gw (zone_id, aquifer_k) VALUES (1, 1.0)", transaction: transaction);
                    }
                    transaction.Commit();
                }
            }
        }

        // Populate cell table from grid shapefile, sampling DEM and thickness.
        public virtual void _populateCells(GridData gridData) {
            using (var transaction = this._gv.db.conn.BeginTransaction()) {
                this._progress("Populating gwflow cells");
                OSGeo.GDAL.Dataset demDs = null;
                OSGeo.GDAL.Band demBand = null;
                double[] demTransform = new double[6];
                if (File.Exists(this._gv.demFile)) {
                    demDs = Gdal.Open(this._gv.demFile, Access.GA_ReadOnly);
                    if (demDs is not null) {
                        demBand = demDs.GetRasterBand(1);
                        demDs.GetGeoTransform(demTransform);
                    }
                }
                OSGeo.GDAL.Dataset thickDs = null;
                Band thickBand = null;
                double[] thickTransform = new double[6];
                var thickFile = this._panel._aquiferThickness;
                if (File.Exists(thickFile)) {
                    thickDs = Gdal.Open(thickFile, Access.GA_ReadOnly);
                    if (thickDs is not null) {
                        thickBand = thickDs.GetRasterBand(1);
                        thickDs.GetGeoTransform(thickTransform);
                    }
                }
                var permZones = this._loadPermZones();
                var wsGeom = this.basinGeometry;
                // For structured grids, compute row/col from cell centroid position
                var isStructured = this._panel.isStructured;
                var cellSize = isStructured ? this._panel._cellSize : 0;
                // gis_id = gwflowcells.shp cell id (== cell_id); for GIS join-back; stored
                // so editor/reader doesn't recompute it from row/col.
                var sql = @"INSERT INTO cells_gw
                 (cell_id, status, row, col, x_centroid, y_centroid, area, elevation,
                  aquifer_thickness, zone, initial_head, gis_id)
                 VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
                //var count = 0;
                foreach (var (id, geom) in gridData.geoms) {
                    //count += 1;
                    //if (count % 50 == 0) {
                    //    QCoreApplication.processEvents(QEventLoop.ProcessEventsFlag.ExcludeUserInputEvents);
                    //}
                    var cellId = gridData.cellids[id];
                    var centroidGeom = geom.Centroid();
                    double[] centroid = new double[3];
                    centroidGeom.GetPoint(0, centroid);
                    var area = geom.Area();
                    // Compute row/col for structured grids
                    int row = -1;
                    int col = -1;
                    if (isStructured && gridData.extent is not null && cellSize > 0) {
                        col = Convert.ToInt32((centroid[0] - gridData.extent.MinX) / cellSize) + 1;
                        row = Convert.ToInt32((gridData.extent.MaxY - centroid[1]) / cellSize) + 1;
                    }
                    // Cell elevation = mean of DEM pixels over the cell footprint, not a
                    // single centroid pixel. gwflow cells span many DEM pixels, so a
                    // point sample picks up local spikes and produces a rough elevation
                    // field (and spiky water-table-depth = elev - head); the cell mean is
                    // the representative value.
                    var elev = _cellMeanRaster(demBand, demTransform, geom);
                    if (elev < -9000) {
                        elev = 0.0;
                    }
                    // Aquifer thickness: same, mean over the cell footprint.
                    var thick = _cellMeanRaster(thickBand, thickTransform, geom);
                    if (thick <= 0) {
                        thick = 50.0;
                    }
                    // Assign zone
                    var zoneId = _findZone(permZones, centroidGeom);
                    // Determine status: 2=boundary (touches watershed edge); 1=interior
                    var status = 1;
                    if (wsGeom is not null) {
                        try {
                            if (geom.Distance(wsGeom) < Math.Sqrt(area) * 0.1) {
                                status = 2;
                            }
                        }
                        catch (Exception) {
                        }
                    }
                    var initialHead = elev - 5.0;
                    this._gv.db.execNonQuery(sql, [cellId, status, row, col, centroid[0], centroid[1], area, elev, thick, zoneId, initialHead, cellId], transaction: transaction);
                }
                demDs = null;
                thickDs = null;
                transaction.Commit();
            }
        }


        private class ConnectionResult {
            public int hub;
            public List<int> connections;
        }

        // Build cell adjacency using parallel workers.
        public virtual void _populateCellConnections(GridData gridData) {
            using (var transaction = this._gv.db.conn.BeginTransaction()) {
                this._progress("Building cell connections");
                //var all_wkbs = new Dictionary<int, byte[]> {
                //};
                //var nWorkers = Math.Min(Math.Min(8, fids.Count); Environment.ProcessorCount);
                //nWorkers = Math.Max(1, nWorkers);
                //var chunks = (from _ in Enumerable.Range(0, nWorkers)
                //              select new List<object>()).ToList();
                //foreach (var (i, fid) in fids.Select((_p_1, _p_2) => Tuple.Create(_p_2, _p_1))) {
                //    chunks[i % nWorkers].append(fid);
                //}
                //var worker_args = (from chunk in chunks
                //                   where chunk
                //                   select (chunk, all_wkbs, all_cellids, all_bboxes)).ToList();

                Task[] taskArray = new Task[gridData.geoms.Count];
                foreach (var (id, geom) in gridData.geoms) {
                    taskArray[id] = Task.Factory.StartNew((Object obj) => {
                        ConnectionResult result = obj as ConnectionResult;

                        var bx = gridData.bboxes[id];
                        foreach (var (other_id, other_bbox) in gridData.bboxes) {
                            if (other_id == id) {
                                continue;
                            }
                            // Quick bbox rejection
                            if (bx.MaxX < other_bbox.MinX || bx.MinX > other_bbox.MaxX || bx.MaxY < other_bbox.MinY || bx.MinY > other_bbox.MaxY) {
                                continue;
                            }
                            var other_geom = gridData.geoms[other_id];
                            if (geom.Touches(other_geom)) {
                                result.connections.Add(gridData.cellids[other_id]);
                            }
                        }
                    },
                    new ConnectionResult() { hub = gridData.cellids[id], connections = new List<int>() });
                }
                var sql = "INSERT OR IGNORE INTO cellcon_gw (cell_id, connected_cell_id) VALUES (?, ?)";
                Task.WaitAll(taskArray);
                foreach (var task in taskArray) {
                    var data = task.AsyncState as ConnectionResult;
                    var cellId = data.hub;
                    foreach (var cid in data.connections) {
                        this._gv.db.execNonQuery(sql, [cellId, cid], transaction: transaction);
                    }

                    //    var sql = "INSERT OR IGNORE INTO cellcon_gw (cell_id, connected_cell_id) VALUES (?, ?)";
                    //if (nWorkers > 1 && fids.Count > 50) {
                    //    using (var pool = Pool(nWorkers)) {
                    //        foreach (var result in pool.imap_unordered(_find_connections_chunk, worker_args)) {
                    //            foreach (var pair in result) {
                    //                this._gv.db.execNonQuery(sql, pair);
                    //            }
                    //            QCoreApplication.processEvents(QEventLoop.ProcessEventsFlag.ExcludeUserInputEvents);
                    //        }
                    //    }
                    //} else {
                    //    foreach (var args in worker_args) {
                    //        foreach (var pair in _find_connections_chunk(args)) {
                    //            this._gv.db.execNonQuery(sql, pair);
                    //        }
                    //    }
                    //}
                }
                transaction.Commit();
            }
        }

        private class IntersectionResult {
            public int cellId;
            public int unitId;
            public double area;
        }

        // Intersect gwflow grid with HRU shapefile using parallel workers.
        public virtual void _populateHRUCells(GridData gridData) {
            using (var transaction = this._gv.db.conn.BeginTransaction()) {
                this._progress("Computing HRU-cell intersections");
            var hruFile = this._gv.actHRUsFile;
            if (!File.Exists(hruFile)) {
                Utils.error("You cannot use gwflow if you have not generated a fullHRUs shapefile", this._gv.isBatch, logFile: this._gv.logFile);
                return;
            }
                using (var driver = Ogr.GetDriverByName("ESRI Shapefile")) {
                    var ds = driver.Open(hruFile, 0);
                    var hruLayer = ds.GetLayerByIndex(0);
                    var hruIdIdx = hruLayer.FindFieldIndex("HRUS", 1);
                    if (hruIdIdx < 0) {
                        Utils.error(string.Format("No HRUS field in {0}; cannot link gwflow cells to HRUs.", hruFile), this._gv.isBatch, logFile: this._gv.logFile);
                        return;
                    }
                    var overlay_hrus = new Dictionary<int, (OSGeo.OGR.Geometry, OSGeo.OGR.Envelope)>();
                    hruLayer.ResetReading();
                    OSGeo.OGR.Feature hru = null;
                    while (true) {
                        hru = hruLayer.GetNextFeature();
                        if (hru is null) { break; }
                        var hruVal = hru.GetFieldAsString(hruIdIdx);
                        if (hruVal is null || hruVal == "NA") {
                            continue;
                        }
                        try {
                            var unitId = Convert.ToInt32(hruVal.ToString().Split(",")[0]);
                            if (unitId > 0) {
                                var geom = hru.GetGeometryRef();
                                OSGeo.OGR.Envelope bx = new OSGeo.OGR.Envelope();
                                geom.GetEnvelope(bx);
                                overlay_hrus[unitId] = (geom, bx);
                            }
                        }
                        catch {
                        }
                    }
                    if (overlay_hrus.Count == 0) {
                        return;
                    }

                    Task[] taskArray = new Task[gridData.geoms.Count];
                    foreach (var (id, cellGeom) in gridData.geoms) {
                        taskArray[id] = Task.Factory.StartNew((Object obj) => {
                            IntersectionResult result = obj as IntersectionResult;
                            if (result == null) { return; }
                            var cellBx = gridData.bboxes[id];
                            foreach (var (oId, (oGeom, oBx)) in overlay_hrus) {
                                if (cellBx.MaxX < oBx.MinX || cellBx.MinX > oBx.MaxX || cellBx.MaxY < oBx.MinY || cellBx.MinY > oBx.MaxY) {
                                    continue;
                                }
                                if (cellGeom.Intersects(oGeom)) {
                                    var inter = cellGeom.Intersection(oGeom);
                                    var area = inter.Area();
                                    result.unitId = oId;
                                    result.area = area;
                                }
                            }
                        },
                        new IntersectionResult() { cellId = gridData.cellids[id] });
                    }
                    //var nWorkers = min(Convert.ToInt32(QSettings().value("/QSWATPlus/NumProcesses", 8)); cell_data.Count, cpu_count());
                    //nWorkers = max(1, nWorkers);
                    //// Split cells into chunks
                    //var chunks = (from _ in Enumerable.Range(0, nWorkers)
                    //              select new List<object>()).ToList();
                    //foreach (var (i, cd) in cell_data.Select((_p_3, _p_4) => Tuple.Create(_p_4, _p_3))) {
                    //    chunks[i % nWorkers].append(cd);
                    //}
                    var sql = "INSERT OR IGNORE INTO hrucell_gw (cell_id, hru_id, area_m2) VALUES (?, ?, ?)";
                    Task.WaitAll(taskArray);
                    foreach (var task in taskArray) {
                        var data = task.AsyncState as IntersectionResult;
                        if (data != null) {
                            this._gv.db.execNonQuery(sql, [data.cellId, data.unitId, data.area], transaction: transaction);
                        }
                    }
                    //var worker_args = (from chunk in chunks
                    //                   where chunk
                    //                   select (chunk, overlay_wkbs, overlay_ids)).ToList();
                    ////if (nWorkers > 1 && cell_data.Count > 20) {
                    //    using (var pool = Pool(nWorkers)) {
                    //        foreach (var result in pool.imap_unordered(_intersect_cells_chunk, worker_args)) {
                    //            foreach (var (cellId, hruId, area) in result) {
                    //                this._gv.db.execNonQuery(sql, (cellId, unitId, area));
                    //            }
                    //            QCoreApplication.processEvents(QEventLoop.ProcessEventsFlag.ExcludeUserInputEvents);
                    //        }
                    //    }
                    //} else {
                    //    foreach (var args in worker_args) {
                    //        foreach (var (cellId, unitId, area) in _intersect_cells_chunk(args)) {
                    //            this._gv.db.execNonQuery(sql, (cellId, hruId, area));
                    //        }
                    //    }
                    //}
                    transaction.Commit();
                }
            }
        }

        // Intersect gwflow grid with LSU shapefile using parallel workers.
        public virtual void _populateLSUCells(GridData gridData) {
            using (var transaction = this._gv.db.conn.BeginTransaction()) {
                this._progress("Computing LSU-cell intersections");
            var lsuFile = this._gv.actLSUsFile;
            if (!File.Exists(lsuFile)) {
                return;
            }
            using (var driver = Ogr.GetDriverByName("ESRI Shapefile")) {
                var ds = driver.Open(lsuFile, 0);
                var lsuLayer = ds.GetLayerByIndex(0);
                var lsuIdIdx = lsuLayer.FindFieldIndex("LSUID", 1);
                if (lsuIdIdx < 0) {
                    Utils.error(string.Format("No LSUID field in {0}; cannot link gwflow cells to LSUs.", lsuFile), this._gv.isBatch, logFile: this._gv.logFile);
                    return;
                }
                var overlay_lsus = new Dictionary<int, (OSGeo.OGR.Geometry, OSGeo.OGR.Envelope)>();
                lsuLayer.ResetReading();
                OSGeo.OGR.Feature lsu = null;
                while (true) {
                    lsu = lsuLayer.GetNextFeature();
                    if (lsu is null) { break; }
                    var lsuVal = lsu.GetFieldAsString(lsuIdIdx);
                    if (lsuVal is null) {
                        continue;
                    }
                    try {
                        var unitId = Convert.ToInt32(lsuVal.ToString().Split(",")[0]);
                        if (unitId > 0) {
                            var geom = lsu.GetGeometryRef();
                            OSGeo.OGR.Envelope bx = new OSGeo.OGR.Envelope();
                            geom.GetEnvelope(bx);
                            overlay_lsus[unitId] = (geom, bx);
                        }
                    }
                    catch {
                    }
                }
                if (overlay_lsus.Count == 0) {
                    return;
                }

                    Task[] taskArray = new Task[gridData.geoms.Count];
                    foreach (var (id, cellGeom) in gridData.geoms) {
                        taskArray[id] = Task.Factory.StartNew((Object obj) => {
                        IntersectionResult result = obj as IntersectionResult;
                        if (result == null) { return; }

                        var cellBx = gridData.bboxes[id];
                        foreach (var (oId, (oGeom, oBx)) in overlay_lsus) {
                            if (cellBx.MaxX < oBx.MinX || cellBx.MinX > oBx.MaxX || cellBx.MaxY < oBx.MinY || cellBx.MinY > oBx.MaxY) {
                                continue;
                            }
                            if (cellGeom.Intersects(oGeom)) {
                                var inter = cellGeom.Intersection(oGeom);
                                var area = inter.Area();
                                result.unitId = oId;
                                result.area = area;
                            }
                        }
                    },
                    new IntersectionResult() { cellId = gridData.cellids[id] });
                }
                var sql = "INSERT OR IGNORE INTO lsucell_gw (cell_id, lsu_id, area_m2) VALUES (?, ?, ?)";
                Task.WaitAll(taskArray);
                    foreach (var task in taskArray) {
                        var data = task.AsyncState as IntersectionResult;
                        if (data != null) {
                            this._gv.db.execNonQuery(sql, [data.cellId, data.unitId, data.area], transaction: transaction);
                        }
                    }
                    transaction.Commit();
                }
            }
        }

        // Intersect gwflow grid with channel lines.
        public virtual void _populateChannelCells(GridData gridData) {
            using (var transaction = this._gv.db.conn.BeginTransaction()) {
                this._progress("Computing channel-cell intersections");
                // Use rivs1.shp: it carries the SWAT+ Channel number (== gis_channels.id /
                // chandeg.con / chancell.gw). self._gv.channelFile is the raw TauDEM
                // channel shapefile, which has only LINKNO (a different numbering) and no
                // Channel field, so it must not be used for the channel-cell links.
                var channelFile = Utils.join(this._gv.shapesDir, Parameters._RIVS1 + ".shp");
                if (!File.Exists(channelFile)) {
                    return;
                }
                using (var driver = Ogr.GetDriverByName("ESRI Shapefile")) {
                    var ds = driver.Open(channelFile, 0);
                    var chanLayer = ds.GetLayerByIndex(0);
                    var chanIdIdx = chanLayer.FindFieldIndex(Topology._CHANNEL, 1);
                    if (chanIdIdx < 0) {
                        Utils.error(string.Format("No {0} field in {1}; cannot link gwflow cells to channels.", Topology._CHANNEL, channelFile), this._gv.isBatch, logFile: this._gv.logFile);
                        return;
                    }
                    var chanDepthIdx = chanLayer.FindFieldIndex(Topology._DEP2, 1);
                    var validChannels = new HashSet<int>();
                    var reader = this._gv.db.getReader("SELECT id FROM gis_channels");
                    while (reader.Read()) {
                        validChannels.Add(reader.GetInt32(0));
                    }
                    OSGeo.GDAL.Dataset demDs = null;
                    Band demBand = null;
                    double[] demTransform = new double[6];
                    var riverDepth = 5.0;  // default: changed later to DEP2 field
                    if (File.Exists(this._gv.demFile)) {
                        demDs = Gdal.Open(this._gv.demFile, Access.GA_ReadOnly);
                        if (demDs is not null) {
                            demBand = demDs.GetRasterBand(1);
                            demDs.GetGeoTransform(demTransform);
                        }
                    }
                    var sql = @"INSERT INTO chancell_gw (cell_id, channel_id, bed_elevation, length_m, zone_id)
                 VALUES (?, ?, ?, ?, 1)";
                    foreach (var (id, cellGeom) in gridData.geoms) {
                        var cellId = gridData.cellids[id];
                        OSGeo.OGR.Envelope cellBbox = gridData.bboxes[id];
                        OSGeo.OGR.Feature chanFeat = null;
                        chanLayer.ResetReading();
                        while (true) {
                            chanFeat = chanLayer.GetNextFeature();
                            if (chanFeat == null) { break; }
                            var chanId = chanFeat.GetFieldAsInteger(chanIdIdx);
                            if (!validChannels.Contains(chanId)) {
                                continue;
                            }
                            if (chanDepthIdx >= 0) {
                                riverDepth = chanFeat.GetFieldAsDouble(chanDepthIdx);
                            }
                            var chanGeom = chanFeat.GetGeometryRef();
                            OSGeo.OGR.Envelope chanBbox = new OSGeo.OGR.Envelope();
                            chanGeom.GetEnvelope(chanBbox);
                            if (chanBbox.MinX > cellBbox.MaxX || cellBbox.MinX > chanBbox.MaxX || chanBbox.MinY > cellBbox.MaxY || cellBbox.MinY > chanBbox.MaxY) { continue; }
                            if (cellGeom.Intersects(chanGeom)) {
                                var intersection = cellGeom.Intersection(chanGeom);
                                var length = intersection.Length();
                                if (length > 0) {
                                    double[] centroid = new double[3];
                                    intersection.Centroid().GetPoint(0, centroid);
                                    var elev = Topology.valueAtPoint(centroid, demTransform, demBand);
                                    var bedElev = elev > -9000 ? elev - riverDepth : 0.0;
                                    this._gv.db.execNonQuery(sql, [cellId, chanId, bedElev, length], transaction: transaction);
                                }
                            }
                        }
                    }
                    demDs = null;
                }
                transaction.Commit();
            }
        }

        // Find cells that overlap reservoir/lake polygons.
        public virtual void _populateReservoirCells(GridData gridData) {
            using (var transaction = this._gv.db.conn.BeginTransaction()) {
                this._progress("Computing reservoir-cell intersections");
                var lakeFile = this._gv.lakeFile;
                if (!File.Exists(lakeFile)) {
                    return;
                }
                using (var driver = Ogr.GetDriverByName("ESRI Shapefile")) {
                    var ds = driver.Open(lakeFile, 0);
                    var lakeLayer = ds.GetLayerByIndex(0);
                    var lakeIdIdx = lakeLayer.FindFieldIndex("LakeId", 1);
                    if (lakeIdIdx < 0) {
                        Utils.error(string.Format("No LakeId field in {0}; cannot link gwflow cells to reservoirs", lakeFile), this._gv.isBatch, logFile: this._gv.logFile);
                        return;
                    }
                    var sql = "INSERT OR IGNORE INTO rescell_gw (cell_id, reservoir_id, stage) VALUES (?, ?, 0)";
                    foreach (var (id, cellGeom) in gridData.geoms) {
                        var cellId = gridData.cellids[id];
                        OSGeo.OGR.Envelope cellBbox = gridData.bboxes[id];
                        OSGeo.OGR.Feature lakeFeat = null;
                        lakeLayer.ResetReading();
                        while (true) {
                            lakeFeat = lakeLayer.GetNextFeature();
                            if (lakeFeat == null) { break; }
                            var lakeGeom = lakeFeat.GetGeometryRef();
                            OSGeo.OGR.Envelope lakeBbox = new OSGeo.OGR.Envelope();
                            lakeGeom.GetEnvelope(lakeBbox);
                            if (lakeBbox.MinX > cellBbox.MaxX || cellBbox.MinX > lakeBbox.MaxX || lakeBbox.MinY > cellBbox.MaxY || cellBbox.MinY > lakeBbox.MaxY) { continue; }
                            if (cellGeom.Intersects(lakeGeom)) {
                                var lakeId = lakeFeat.GetFieldAsInteger(lakeIdIdx);
                                this._gv.db.execNonQuery(sql, [cellId, lakeId], transaction: transaction);
                            }
                        }
                    }
                }
                transaction.Commit();
            }
        }

        // Find cells that overlap floodplain LSUs, only for valid channels.
        public virtual void _populateFloodplainCells(GridData gridData) {
            using (var transaction = this._gv.db.conn.BeginTransaction()) {
                this._progress("Computing floodplain-cell intersections");
            var lsuFile = this._gv.actLSUsFile;
            if (!File.Exists(lsuFile)) {
                return;
            }
                using (var driver = Ogr.GetDriverByName("ESRI Shapefile")) {
                    var ds = driver.Open(lsuFile, 0);
                    var lsuLayer = ds.GetLayerByIndex(0);
                    var lsuIdIdx = lsuLayer.FindFieldIndex("LSUID", 1);
                    if (lsuIdIdx < 0) {
                        Utils.error(string.Format("No LSUID field in {0}; cannot link gwflow cells to LSUs.", lsuFile), this._gv.isBatch, logFile: this._gv.logFile);
                        return;
                    }
                    var validChannels = new HashSet<int>();
                    var reader = this._gv.db.getReader("SELECT id FROM gis_channels");
                    while (reader.Read()) {
                        validChannels.Add(reader.GetInt32(0));
                    }
                    var sql = "INSERT OR IGNORE INTO floodplain_gw (cell_id, channel_id, area_m2, conductivity) VALUES (?, ?, ?, 0)";
                    foreach (var (id, cellGeom) in gridData.geoms) {
                        var cellId = gridData.cellids[id];
                        OSGeo.OGR.Envelope cellBbox = gridData.bboxes[id];
                        lsuLayer.ResetReading();
                        OSGeo.OGR.Feature lsu = null;
                        while (true) {
                            lsu = lsuLayer.GetNextFeature();
                            if (lsu is null) { break; }
                            var lsuId = lsu.GetFieldAsInteger(lsuIdIdx);
                            if (lsuId % 10 != 1) {
                                continue;
                            }
                            int channelId = lsuId / 10;
                            if (!validChannels.Contains(channelId)) {
                                continue;
                            }
                            var lsuGeom = lsu.GetGeometryRef();
                            OSGeo.OGR.Envelope lsuBbox = new OSGeo.OGR.Envelope();
                            lsuGeom.GetEnvelope(lsuBbox);
                            if (lsuBbox.MinX > cellBbox.MaxX || cellBbox.MinX > lsuBbox.MaxX || lsuBbox.MinY > cellBbox.MaxY || cellBbox.MinY > lsuBbox.MaxY) { continue; }
                            if (cellGeom.Intersects(lsuGeom)) {
                                var intersection = cellGeom.Intersection(lsuGeom);
                                var interArea = intersection.Area();
                                if (interArea > 0) {
                                    this._gv.db.execNonQuery(sql, [cellId, channelId, interArea], transaction: transaction);
                                }
                            }
                        }
                    }
                }
                transaction.Commit();
            }
        }

        // Insert default solute species.
        public virtual void _populateSolutes() {
            using (var transaction = this._gv.db.conn.BeginTransaction()) {
                this._progress("Writing default solutes");
                // columns: name, sorption_coef, rate_const, canal_irr, init_conc
                var solutes = new Dictionary<string, (double, double, double, double)>();
                solutes["no3-n"] = (1.0, -0.0001, 3.0, 3.0);
                solutes["p"] = (2.0, 0, 0.05, 0.05);
                solutes["so4"] = (1.0, 0, 0, 100.0);
                solutes["ca"] = (1.0, 0, 0, 50.0);
                solutes["mg"] = (1.0, 0, 0, 30.0);
                solutes["na"] = (1.0, 0, 0, 40.0);
                solutes["k"] = (1.0, 0, 0, 1.0);
                solutes["cl"] = (1.0, 0, 0, 25.0);
                solutes["co3"] = (1.0, 0, 0, 1.0);
                solutes["hco3"] = (1.0, 0, 0, 80.0);
                var sql = "INSERT INTO solute_gw (name, sorption_coef, rate_const, canal_irr, init_conc) VALUES (?, ?, ?, ?, ?)";
                foreach (var (name, (sorption_coef, rate_const, canal_irr, init_conc)) in solutes) {
                    this._gv.db.execNonQuery(sql, [name, sorption_coef, rate_const, canal_irr, init_conc], transaction: transaction);
                }
                transaction.Commit();
            }
        }

        // Map observation well points (RES=3 in outlets) to nearest grid cells.
        public virtual void _populateObservationWells(GridData gridData) {
            using (var transaction = this._gv.db.conn.BeginTransaction()) {
                this._progress("Mapping observation wells to cells");
            // Source wells as gwflowgrid._getWellCoords does (outlets file; inlet==0,
            // RES==well type, ptsource==0) so obs_gw holds the same wells the grid was
            // refined around.
            var wellCoords = new Dictionary<int, double[]>();
            var outletFile = this._gv.outletFile;
            if (!File.Exists(outletFile)) {
                return;
            }
            using (var driver = Ogr.GetDriverByName("ESRI Shapefile")) {
                var ds = driver.Open(outletFile, 0);
                var layer = ds.GetLayerByIndex(0);
                var resIdx = layer.FindFieldIndex(Topology._RES, 1);
                var inletIdx = layer.FindFieldIndex(Topology._INLET, 1);
                var ptsourceIdx = layer.FindFieldIndex(Topology._PTSOURCE, 1);
                var ptIdIdx = layer.FindFieldIndex(Topology._POINTID, 1);
                if (resIdx >= 0 && inletIdx >= 0 && ptsourceIdx >= 0 && ptIdIdx >= 0) {
                    OSGeo.OGR.Feature feat = null;
                    layer.ResetReading();
                    while (true) {
                        feat = layer.GetNextFeature();
                        if (feat is null) { break; }
                        if (feat.GetFieldAsInteger(inletIdx) == 0 &&
                            (WaterRole)feat.GetFieldAsInteger(resIdx) == WaterRole._WELLTYPE &&
                            feat.GetFieldAsInteger(ptsourceIdx) == 0) {
                            var geom = feat.GetGeometryRef();
                            if (geom is not null) {
                                double[] point = new double[3];
                                geom.GetPoint(0, point);
                                var ptId = feat.GetFieldAsInteger(ptIdIdx);
                                wellCoords[ptId] = point;
                            }
                        }
                    }
                }
            }
            if (wellCoords.Count == 0) {
                return;
            }
            // Find nearest cell for each well
            var sql = "INSERT OR IGNORE INTO obs_gw (cell_id, name) VALUES (?, ?)";
                foreach (var (ptId, wellPt) in wellCoords) {
                    var wkt = string.Format("POINT ({0} {1})", wellPt[0], wellPt[1]);
                    var wellGeom = OSGeo.OGR.Geometry.CreateFromWkt(wkt);
                    int bestCellId = -1;
                    var bestDist = double.PositiveInfinity;
                    foreach (var (id, cellGeom) in gridData.geoms) {
                        var cellId = gridData.cellids[id];
                        var dist = wellGeom.Distance(cellGeom);
                        if (dist < bestDist) {
                            bestDist = dist;
                            bestCellId = cellId;
                        }
                    }
                    if (bestCellId >= 0) {
                        this._gv.db.execNonQuery(sql, [bestCellId, string.Format("well_{0}", ptId)], transaction: transaction);
                    }
                }
                transaction.Commit();
            }
        }

        // Mean of valid raster pixels over a cell's footprint (its bounding-box
        //         window); ignoring nodata. For axis-aligned grid cells the window is the
        //         cell, so this is the per-cell mean. Returns 0 if no valid pixels.
        public static double _cellMeanRaster(Band band, double[] transform, OSGeo.OGR.Geometry geom) {
            if (band is null || transform is null) {
                return 0;
            }
            var px = transform[1];
            var py = transform[5];
            if (px == 0 || py == 0) {
                return 0;
            }
            OSGeo.OGR.Envelope bb = new OSGeo.OGR.Envelope();
            geom.GetEnvelope(bb);
            var c0 = Convert.ToInt32((bb.MinX - transform[0]) / px);
            var c1 = Convert.ToInt32((bb.MaxX - transform[0]) / px);
            var r0 = Convert.ToInt32((bb.MaxY - transform[3]) / py);
            var r1 = Convert.ToInt32((bb.MinY - transform[3]) / py);
            var col0 = Math.Max(0, Math.Min(c0, c1));
            var col1 = Math.Min(band.XSize - 1, Math.Max(c0, c1));
            var row0 = Math.Max(0, Math.Min(r0, r1));
            var row1 = Math.Min(band.YSize - 1, Math.Max(r0, r1));
            if (col1 < col0 || row1 < row0) {
                return 0;
            }
            int width = col1 - col0 + 1;
            int height = row1 - row0 + 1;
            double[] data = new double[width * height];

            try {
                var arr = band.ReadRaster(col0, row0, width, height, data, width, height, 0, 0);
            } catch (Exception) {
                return 0;
            }
            int hasNodata;
            double nodata;
            band.GetNoDataValue(out nodata, out hasNodata);
            if (hasNodata != 1) { nodata = -9999; }
            double sum = 0;
            var count = 0;
            for (int i = 0; i < width * height; i++) {
                var val = data[i];
                if (val > nodata) {
                    sum += val;
                    count++;
                }
            }
            if (count > 0) {
                return sum / count;
            } else {
                return 0;
            }
        }

        // Load permeability shapefile geometries with zone IDs.
        public virtual List<(int, OSGeo.OGR.Geometry)> _loadPermZones() {
            var zones = new List<(int, OSGeo.OGR.Geometry)>();
            var permFile = this._panel._aquiferPermeability;
            if (!File.Exists(permFile)) {
                return zones;
            }
            using (var driver = Ogr.GetDriverByName("ESRI Shapefile")) {
                var ds = driver.Open(permFile, 0);
                var layer = ds.GetLayerByIndex(0);
                var zoneId = 0;
                OSGeo.OGR.Feature feat = null;
                layer.ResetReading();
                while (true) {
                    feat = layer.GetNextFeature();
                    if (feat is null) { break; }
                    zoneId += 1;
                    zones.Add((zoneId, feat.GetGeometryRef()));
                }
                return zones;
            }
        }

        // Find which permeability zone contains a point. Returns zone_id or 1.
        public static int _findZone(List<(int, OSGeo.OGR.Geometry)> permZones, OSGeo.OGR.Geometry point) {
            if (permZones is null) {
                return 1;
            };
            foreach (var (zoneId, geom) in permZones) {
                if (geom.Contains(point)) {
                    return zoneId;
                }
            }
            return 1;
        }
    }
}
