
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.Exceptions;
using ArcGIS.Core.Data.Raster;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Core.Internal.CIM;
using ArcGIS.Desktop.Layouts;
using CsvHelper.Configuration.Attributes;
using Microsoft.Win32;
using OSGeo.OGR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Resources;
//using System.Windows.Controls;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using static System.Net.WebRequestMethods;
using File = System.IO.File;
using Path = System.IO.Path;

namespace ArcSWATPlus {

    // Functions for interacting with project and reference databases.
    public class DBUtils {

        public List<string> _allTableNames;

        public Dictionary<int, int> _landuseTranslate;

        public List<int> _undefinedLanduseIds;

        public List<int> _undefinedSoilIds;

        public bool addName;

        public bool addSeqn;

        public SQLiteConnection conn;

        public SQLiteConnection connRef;

        public string dbFile;

        public string dbRefFile;

        public int defaultLanduse;

        public int defaultSoil;

        public int floodWetlandCrop;

        public Dictionary<string, HashSet<int>> gis_keys;

        public bool isBatch;

        public bool isHAWQS;

        public bool isHUC;

        public Dictionary<int, string> landuseCodes;

        public Dictionary<int, int> landuseIds;

        public List<string> landuseTableNames;

        public HashSet<int> landuseVals;

        public string logFile;

        public string plantSoilDatabase;

        public bool plantSoilDatabaseSelected;

        public string plantTable;

        public List<string> plantTableNames;

        public int playaCrop;

        public string projDir;

        public string projName;

        public HashSet<(int, string)> routingSinks;

        public HashSet<(int, string)> routingSources;

        public List<double> slopeLimits;

        public string soilDatabase;

        public Dictionary<int, string> soilNames;

        public List<string> soilTableNames;

        public Dictionary<int, int> soilTranslate;

        public HashSet<int> soilVals;

        public SQLiteConnection SSURGOConn;

        public string SSURGODbFile;

        public HashSet<int> usedSSURGOSoils;

        public Dictionary<int, int> SSURGOsoils;

        public int SSURGOUndefined;

        public bool SSURGOUndefinedMapValueReported;

        public int upslopeWetlandCrop;

        public Dictionary<int, int> urbanIds;

        public string urbanTable;

        public List<string> urbanTableNames;

        public Dictionary<int, string> usedSoilNames;

        public string usersoilTable;

        public List<string> usersoilTableNames;

        public bool useS5id;

        public bool useSSURGO;

        public bool useSTATSGO;

        public int waterLanduse;

        public string waterPattern;

        public Regex _MUIDPATTERN = new Regex(@"^[A-Z]{2}[0-9]{3}$");

        public Regex _MUIDPLUSSEQPATTERN = new Regex(@"^[A-Z]{2}[0-9]{3}\+[0-9]+$");

        public Regex _MUIDPLUSNAMEPATTERN = new Regex(@"^[A-Z]{2}[0-9]{3}\+[A-Z]+$");

        public Regex _S5IDPATTERN = new Regex(@"^[A-Z]{2}[0-9]{4}$");

        public DBUtils(
            string projDir,
            string projName,
            string dbProjTemplate,
            string dbRefTemplate,
            bool isHUC,
            bool isHAWQS,
            string logFile,
            bool isBatch) {
            //# Flag showing if batch run
            this.isBatch = isBatch;
            //# flag for HUC projects
            this.isHUC = isHUC;
            //# flag for HAWQS projects
            this.isHAWQS = isHAWQS;
            //# message logging file for HUC projects
            this.logFile = logFile;
            //# project directory
            this.projDir = projDir;
            //# project name
            this.projName = projName;
            //# project database
            var dbSuffix = Path.GetExtension(dbProjTemplate);
            this.dbFile = Utils.join(projDir, projName + dbSuffix);
            //# reference database
            // for HUC models, reference database is shared one level up
            if (this.isHUC) {
                this.dbRefFile = Utils.join(projDir + "/..", Parameters._DBREF);
            } else {
                this.dbRefFile = Utils.join(projDir, Parameters._DBREF);
            }
            //self._connStr = Parameters._ACCESSSTRING + self.dbFile
            //self._connRefStr = Parameters._ACCESSSTRING + self.dbRefFile
            // copy template project database to project folder if not already there
            if (!File.Exists(this.dbFile)) {
                if (!File.Exists(dbProjTemplate)) {
                    Utils.error(string.Format(@"Cannot find project database template {0}.
Have you installed SWAT+ as a different directory from C:\SWAT\SWATPlus?
If so use the QSWAT+ Parameters form to set the correct location.", dbProjTemplate), this.isBatch);

                } else {
                    File.Copy(dbProjTemplate, this.dbFile);
                    Utils.loginfo(string.Format("Made project database from {0}", dbProjTemplate));
                }
            }
            // copy template reference database to project folder if not already there
            if (!File.Exists(this.dbRefFile)) {
                if (!File.Exists(dbRefTemplate)) {
                    Utils.error(string.Format(@"Cannot find reference database template {0}.
Have you installed SWAT+ as a different directory from C:\SWAT\SWATPlus?
If so use the QSWAT+ Parameters form to set the correct location.", dbRefTemplate), this.isBatch);

                } else {
                    File.Copy(dbRefTemplate, this.dbRefFile);
                    Utils.loginfo(string.Format("Made reference database from {0}", dbRefTemplate));
                }
            }
            this.conn = null;
            this.connectToProjectDatabase();
            //# sqlite3 connection to reference database
            this.connRef = new SQLiteConnection(string.Format("Data Source={0}", this.dbRefFile));
            if (this.connRef is null) {
                Utils.error(string.Format("Failed to connect to reference database {0}", this.dbRefFile), this.isBatch);
            }
            this.connRef.Open();
            //# Tables in project database containing 'landuse'
            this.landuseTableNames = new List<string>();
            //# Tables in project database containing 'soil'
            this.soilTableNames = new List<string>();
            //# all tables names in project database
            this._allTableNames = new List<string>();
            //# map of landuse category to SWAT landuse code
            this.landuseCodes = new Dictionary<int, string>();
            //# Landuse categories may not translate 1-1 into SWAT codes.
            //
            // This map is used to map category ids into equivalent ids.
            // Eg if we have [0 +> XXXX, 1 +> YYYY, 2 +> XXXX, 3 +> XXXX] then _landuseTranslate will be
            // [2 +> 0, 3 +> 0] showing that 2 and 3 map to 0, and other categories are not changed.
            // Only landuse categories 0 and 1 are then used to calculate HRUs, i.e. landuses 0, 2 and 3 will 
            // contribute to the same HRUs.
            // There is an invariant that the domains of landuseCodes and _landuseTranslate are disjoint,
            // and that the range of _landuseTranslate is a subset of the domain of landuseCodes.
            this._landuseTranslate = new Dictionary<int, int>();
            //# Map of landuse category to SWAT plant ids (as found in plant table,
            // or 0 for urban)
            //
            // There is an invariant that the domains of landuseCodes and landuseIds are identical.
            this.landuseIds = new Dictionary<int, int>();
            //# List of undefined landuse categories.  Retained so each is only reported once as an error in each run.
            this._undefinedLanduseIds = new List<int>();
            //# Map of landuse category to SWAT urban ids 
            // There is an invariant that the domain of urbanIds is a subset of 
            // the domain of landuseIds, corresponding to those whose plant id is 0
            this.urbanIds = new Dictionary<int, int>();
            //# Set of values occurring in landuse map
            this.landuseVals = new HashSet<int>();
            //# Default landuse
            //# Set to 0 landuse value (if any) else first landuse in lookup table and used to replace landuse nodata when using grid model
            this.defaultLanduse = -1;
            //# code used for WATR, or -1 if WATR not in landuse table
            this.waterLanduse = -1;
            //# crop value that will translate to WETM
            this.floodWetlandCrop = -1;
            //# crop value that will translate to WETN
            this.upslopeWetlandCrop = -1;
            //# crop value that will translate to WETW
            this.playaCrop = -1;
            //# Map of soil id  to soil name
            this.soilNames = new Dictionary<int, string>();
            //# Soil categories may not translate 1-1 into soils.
            //
            // This map is used to map category ids into equivalent ids.
            // Eg if we have [0 +> XXXX, 1 +> YYYY, 2 +> XXXX, 3 +> XXXX] then soilTranslate will be
            // [2 +> 0, 3 +> 0] showing that 2 and 3 map to 0, and other categories are not changed.
            // Only soil ids 0 and 1 are then used to calculate HRUs, i.e. soils 0, 2 and 3 will 
            // contribute to the same HRUs.
            // There is an invariant that the domains of soilNames and soilTranslate are disjoint,
            // and that the range of soilTranslate is a subset of the domain of soilNames.
            this.soilTranslate = new Dictionary<int, int>();
            //# List of undefined soil identifiers.  Retained so each is only reported once as an error in each run.
            this._undefinedSoilIds = new List<int>();
            //# Copy of soilNames for those soils actually found
            this.usedSoilNames = new Dictionary<int, string>();
            //# Set of values occurring in soil map
            this.soilVals = new HashSet<int>();
            //# ssurgo soil numbers actually found
            this.usedSSURGOSoils = new HashSet<int>();
            //# Default soil
            //# Set to 0 soild value (if any) else first soil in lookup table and used to replace soil nodata when using grid model
            this.defaultSoil = -1;
            //# List of limits for slopes.
            //
            // A list [a,b] means slopes are in ranges [slopeMin,a), [a,b), [b,slopeMax] 
            // and these ranges would be indexed by slopes 0, 1 and 2.
            this.slopeLimits = new List<double>();
            //# flag indicating STATSGO soil data is being used
            this.useSTATSGO = false;
            //# flag indicating SSURGO or STATSGO2 soil data is being used
            this.useSSURGO = false;
            //# map of SSURGO map values to SSURGO MUID (only used with HUC)
            this.SSURGOsoils = new Dictionary<int, int>();
            if (isHUC || isHAWQS) {
                //# SSURGO soil database (only used with HUC and HAWQS)
                if (isHUC) {
                    // changed to use copy one up frpm projDir
                    this.SSURGODbFile = Utils.join(projDir + "/..", Parameters._SOILDB);
                } else {
                    // HAWQS
                    this.SSURGODbFile = Utils.join(Utils.join(Parameters._SWATPLUSDEFAULTDIR, Parameters._DBDIR), Parameters._SOILDB);
                }
                this.SSURGOConn = new SQLiteConnection("Data Source=" + this.SSURGODbFile);
                this.SSURGOConn.Open();
            }
            //# nodata value from soil map to replace undefined SSURGO soils (only used with HUC and HAWQS)
            this.SSURGOUndefined = -1;
            // flag to reduce undefined SSURGO map values (only used with HUC and HAWQS)
            this.SSURGOUndefinedMapValueReported = false;
            //# regular expression for checking if SSURGO soils are water (only used with HUC and HAWQS)
            this.waterPattern = @"\bwaters?\b";
            //# flag indicating, if useSTATSGO is true, that muid+seqn is being used
            this.addSeqn = false;
            //# flag indicating, if useSTATSGO is true, that muid+name is being used
            this.addName = false;
            //# flag indicating, if useSTATSGO is true, that s5id is being used
            this.useS5id = false;
            //# database containing landuse and soil properties tables
            this.plantSoilDatabase = "";
            //# database to use for STATSGO and SSURGO soils
            this.soilDatabase = "";
            //# plant landuse properties table
            this.plantTable = "";
            //# urban landuse properties table
            this.urbanTable = "";
            //# soil properties (usersoil) table; overridden by 'statsgo' or 'ssurgo' if their flags set
            this.usersoilTable = "";
            //# flag to show if landuse and soil database has been selected by user, or defined in project file
            this.plantSoilDatabaseSelected = false;
            //# table names in landuse and soil database containing 'plant'
            this.plantTableNames = new List<string>();
            //# table names in landuse and soil database containing 'urban'
            this.urbanTableNames = new List<string>();
            //# table names in landuse and soil database containing 'usersoil'
            this.usersoilTableNames = new List<string>();
            //# collections of keys for each gis_ table: used for checking routing entries exist
            this.gis_keys = new Dictionary<string, HashSet<int>>();
            //# routing sources
            this.routingSources = new HashSet<(int, string)>();
            //# routing targets
            this.routingSinks = new HashSet<(int, string)>();
        }

        //  sqlite3 connection to project database and reference database
        public virtual void connectToProjectDatabase() {
            if (this.conn is null) {
                this.conn = new SQLiteConnection("Data Source=" + this.dbFile);
                if (this.conn is null) {
                    Utils.error(string.Format("Failed to connect to project database {0}", this.dbFile), this.isBatch);
                    return;
                }
            }
            if (this.conn.State != ConnectionState.Open) {
                this.conn.Open();
            }
            if (this.conn.State != ConnectionState.Open) {
                Utils.error(string.Format("Failed to open project database {0}", this.dbFile), this.isBatch);
                return;
            }
            var sql = "PRAGMA journal_mode=OFF";
            this.execNonQuery(sql);
            if (this.connRef is null) {
                this.connRef = new SQLiteConnection("Data Source=" + this.dbRefFile);
                if (this.connRef is null) {
                    Utils.error(string.Format("Failed to connect to reference database {0}", this.dbRefFile), this.isBatch);
                    return;
                }
            }
            if (this.connRef.State != ConnectionState.Open) {
                this.connRef.Open();
            }
            if (this.connRef.State != ConnectionState.Open) {
                Utils.error(string.Format("Failed to open reference database {0}", this.dbRefFile), this.isBatch);
                return;
            }
        }

        // Create gis_routing table.
        //         
        public virtual bool createRoutingTable() {
            try {
                var sql = "DROP TABLE IF EXISTS gis_routing";
                this.execNonQuery(sql);
                this.execNonQuery(DBUtils._ROUTINGCREATESQL);
                this.execNonQuery(DBUtils._ROUTINGINDEXSQL);
                return true;
            } catch (Exception) {
                return false;
            }
        }

        // Return true if table exists in project database and has data.
        public virtual bool hasData(string table) {
            try {
                var sql = sqlSelect(table, "*", "", "");
                using (System.Data.SQLite.SQLiteDataReader reader = getReader(sql)) {
                    return reader.HasRows;
                }
            } catch (Exception) {
                return false;
            }
        }

        // Returns true if db has table.
        public virtual bool hasTable(string db, string table) {
            // avoid opening proj or ref db again (and closing it!)
            var isProjDb = File.Equals(db, this.dbFile);
            var isRefDb = File.Equals(db, this.dbRefFile);
            SQLiteConnection localConn = null;
            if (isProjDb) {
                localConn = this.conn;
            } else if (isRefDb) {
                localConn = this.connRef;
            } else {
                localConn = new SQLiteConnection("Data Source=" + db);
                localConn.Open();
            }
            if (localConn is null) {
                return false;
            }
            try {
                return this.hasTableConn(localConn, table);
            } catch (Exception) {
                return false;
            } finally {
                if (!(isProjDb || isRefDb)) {
                    localConn.Close();
                    localConn.Dispose();
                }
            }
        }

        // Uses existing connection to return true if table exists.
        public virtual bool hasTableConn(SQLiteConnection localConn, string table) {
            try {
                var sql = "SELECT name FROM sqlite_master WHERE TYPE='table'";
                using (var cmd = new SQLiteCommand(sql, localConn)) {
                    using (var reader = cmd.ExecuteReader()) {
                        return reader.HasRows;
                    }
                }
            } catch (Exception) {
                return false;
            }
        }

        // Return true if table exists in existing connection and has data.
        public virtual bool hasDataConn(string table, SQLiteConnection localConn) {
            try {
                var sql = sqlSelect(table, "*", "", "");
                using (var cmd = new SQLiteCommand(sql, localConn)) {
                    using (var reader = cmd.ExecuteReader()) {
                        return reader.HasRows;
                    }
                }
            } catch (Exception) {
                return false;
            }
        }

        // Clear table of data.
        public virtual void clearTable(string table) {
            try {
                this.execNonQuery("DELETE FROM " + table);
            } catch (Exception) {
                // since purpose is to make sure any data in table is not accessible
                // ignore problems such as table not existing
            }
        }

        public void execCmdQuery(SQLiteCommand cmd, object[] parms=null) {
            if (parms != null) {
                for (int i = 1; i <= parms.Length; i++) {
                    var val = parms[i - 1];
                    if (val == null) { val = DBNull.Value; } 
                    cmd.Parameters.AddWithValue(string.Format("@p{0}", i), val);
                }
            }
            cmd.ExecuteNonQuery();
        }

        public void execNonQuery(string sql, object[] parms = null, SQLiteTransaction transaction = null) {
            if (transaction != null) {
                using (var cmd = new SQLiteCommand(sql, this.conn, transaction)) {
                    if (parms != null) {
                        for (int i = 1; i <= parms.Length; i++) {
                            var val = parms[i - 1];
                            if (val == null) { val = DBNull.Value; }
                            cmd.Parameters.AddWithValue(string.Format("@p{0}", i), val);
                        }
                    }
                    try {
                        cmd.ExecuteNonQuery();
                    } catch (Exception ex) {
                        Utils.error(string.Format("{0} failed: {1}", cmd.CommandText, ex.Message), this.isBatch);
                    }
                }
            } else {
                using (var cmd = new SQLiteCommand(sql, this.conn)) {
                    if (parms != null) {
                        for (int i = 1; i <= parms.Length; i++) {
                            var val = parms[i - 1];
                            if (val == null) { val = DBNull.Value; }
                            cmd.Parameters.AddWithValue(string.Format("@p{0}", i), val);
                        }
                    }
                    try {
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex) {
                        Utils.error(string.Format("{0} failed: {1}", cmd.CommandText, ex.Message), this.isBatch);
                    }
                }
            }
        }

        public bool hasColumn (string table, string columnName) {
            var sql = string.Format("PRAGMA TABLE_INFO('{0}')", table);
            using (var cmd = new SQLiteCommand(sql, this.conn)) {
                using (var reader = cmd.ExecuteReader()) {
                    if (reader.HasRows) {
                        while (reader.Read()) {
                            if (reader.GetString(1).Trim().ToLower().Equals(columnName.ToLower())) {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public SQLiteDataReader getReader(string sql, object[] parms = null) {
            using (var cmd = new SQLiteCommand(sql, this.conn)) {
                if (parms != null) {
                    for (int i = 1; i <= parms.Length; i++) {
                        var val = parms[i - 1];
                        if (val == null) { val = DBNull.Value; }
                        cmd.Parameters.AddWithValue(string.Format("@p{0}", i), val);
                    }
                }
                return cmd.ExecuteReader();
            }
        }

        // remove all keys for table
        public virtual void clearKeys(string table) {
            this.gis_keys[table] = new HashSet<int>();
        }

        // Add key to table entry in gis_keys.
        public virtual void addKey(string table, int key) {
            HashSet<int> keys = new HashSet<int>();
            if (this.gis_keys.TryGetValue(table, out keys)) {
                keys.Add(key);
            } else {
                keys = new HashSet<int> { key };
            }
            this.gis_keys[table] = keys;
        }

        // Return true if key in gis_keys for this table.
        public virtual bool hasKey(string table, int key) {
            HashSet<int> keys = new HashSet<int>();
            if (this.gis_keys.TryGetValue(table, out keys)) {
                return keys.Contains(key);
            }
            return false;
        }

        // Generate error message if key not in gis_keys for this table.
        public virtual void checkKeyInTable(string table, int key) {
            //// ignore for HUC and HAWQS projects
            //if (this.isHUC || this.isHAWQS) {
            //    return;
            //}
            HashSet<int> keys = new HashSet<int>();
            if (this.gis_keys.TryGetValue(table, out keys)) {
                if (!keys.Contains(key)) {
                    Utils.error(string.Format("Internal error: Id {0} has not been added to table {1}", key, table), this.isBatch);
                }
            } else {
                Utils.error(string.Format("Internal error: Id {0} has not been added to table {1}", key, table), this.isBatch);
            }
        }

        // Check that source is defined in the relevant table, and add to routing table.
        //         
        //         Does not check sink since there is a separate check that all non-exit sinks are sources.
        public virtual void addToRouting(
            int sourceId,
            string sourceCategory,
            int sinkId,
            string sinkCategory,
            string hydTyp,
            double percent,
            SQLiteCommand cmd) {
            string table = null;
            if (sourceCategory == "PT") {
                table = null;
            } else if (sourceCategory == "CH") {
                table = "gis_channels";
            } else if (sourceCategory == "HRU") {
                table = null;
            } else if (sourceCategory == "LSU") {
                table = null;
            } else if (sourceCategory == "SUB" || sourceCategory == "SBR") {
                table = null;
            } else if (sourceCategory == "AQU") {
                table = null;
            } else if (sourceCategory == "DAQ") {
                table = null;
            } else if (new HashSet<string> {
                "RES",
                "PND",
                "WETL"
            }.Contains(sourceCategory)) {
                table = "gis_water";
            } else {
                table = null;
            }
            if (table is not null) {
                this.checkKeyInTable(table, sourceId);
            }
            this.routingSources.Add((sourceId, sourceCategory));
            this.routingSinks.Add((sinkId, sinkCategory));
            cmd.CommandText = DBUtils._ROUTINGINSERTSQL;
            this.execCmdQuery(cmd, new object[] { sourceId, sourceCategory, hydTyp, sinkId, sinkCategory, percent });
        }

        // Change routing of existing (not checked) entry.
        public virtual void updateRouting(
            int sourceId,
            string sourceCategory,
            int sinkId,
            string sinkCategory,
            string hydTyp,
            double percent,
            SQLiteCommand cmd) {
            this.routingSinks.Add((sinkId, sinkCategory));
            cmd.CommandText = DBUtils._ROUTINGUPDATESQL;
            this.execCmdQuery(cmd, new object[] { sinkId, sinkCategory, percent, sourceId, sourceCategory, hydTyp });
        }

        // Checks categories PT, LSU, SUB, SBR sources are defined in gis_table.
        public virtual void checkRoutedPointsSubbasinsAndLSUsDefined() {
            foreach (var (sourceId, sourceCategory) in this.routingSources) {
                if (sourceCategory == "PT") {
                    this.checkKeyInTable("gis_points", sourceId);
                } else if (sourceCategory == "LSU") {
                    this.checkKeyInTable("gis_lsus", sourceId);
                } else if (sourceCategory == "SUB" || sourceCategory == "SBR") {
                    this.checkKeyInTable("gis_subbasins", sourceId);
                }
            }
        }

        // Checks that all non-exit sinks are sources.  Also checks all LSU and subbasin sources defined in gis_ table.
        //         Finally checks for circularities in gis_routing. 
        public virtual void checkRoutingTable() {
            Utils.loginfo("Checking the routing table gis_routing");
            foreach (var sink in this.routingSinks) {
                if (sink.Item2 != "X" && !this.routingSources.Contains(sink)) {
                    Utils.error(string.Format("Internal error: routing sink category {1} id {0} not found as a source", sink.Item1, sink.Item2), this.isBatch);
                    return;
                }
            }
            this.checkRoutedPointsSubbasinsAndLSUsDefined();
            // check routing table for circularities
            var (errors, warnings) = DBUtils.checkRouting(this.conn);
            foreach (var error in errors) {
                Utils.error(error, this.isBatch);
            }
            if (warnings.Count > 5) {
                Utils.information(string.Format("See QSWAT+ log for {0} warnings about circularities in routing", warnings.Count), this.isBatch);
                foreach (var warning in warnings) {
                    Utils.loginfo(warning);
                }
            } else {
                foreach (var warning in warnings) {
                    Utils.information(warning, this.isBatch);
                }
            }
        }

        // Return usersoil table.
        public virtual string getUsersoilTable() {
            if (this.useSSURGO) {
                return "ssurgo";
            } else if (this.useSTATSGO) {
                return "statsgo";
            } else {
                // avoid confusion with separate layer table
                if (this.usersoilTable.EndsWith("_layer"))
                {
                    this.usersoilTable = this.usersoilTable.Substring(0, this.usersoilTable.Length - 6);
                }
                return this.usersoilTable;

            }
        }

        // Create SQL select statement.
        public static string sqlSelect(string table, string selection, string order, string where) {
            where = where == "" ? "" : " WHERE " + where;
            var select = "SELECT " + selection + " FROM " + table + where;
            return order == "" ? select : select + " ORDER BY " + order;
        }

        // Collect table names from project database.
        public virtual void populateTableNames() {
            this.landuseTableNames = new List<string>();
            this.soilTableNames = new List<string>();
            this._allTableNames = new List<string>();
            var sql = "SELECT name FROM sqlite_master WHERE TYPE='table'";
            var cmd = new SQLiteCommand(sql, this.conn);
            using (var reader = cmd.ExecuteReader()) {
                try {
                    while (reader.Read()) {
                        var table = reader.GetString(0);
                        if (table.StartsWith("config_")) { continue; }
                        if (table.Contains("landuse")) {
                            this.landuseTableNames.Add(table);
                        } else if (table.Contains("soil") && !table.Contains("usersoil") && !table.Contains(DBUtils._SOILS_SOL_NAME)) {
                            this.soilTableNames.Add(table);
                        }
                        this._allTableNames.Add(table);
                    }
                }
                catch (Exception) {
                    Utils.exceptionError(string.Format("Could not read tables in project database {0}", this.dbFile), this.isBatch);
                    return;
                }
            }
        }

        // 
        //         Collect table names containing tableName from landuse and soil database, place in comboBox and return.
        //         
        public virtual List<string> collectPlantSoilTableNames(string tableName, System.Windows.Forms.ComboBox comboBox) {
            // avoid opening proj or ref db again (and closing it!)
            if (!File.Exists(this.plantSoilDatabase)) {
                Utils.error(string.Format("Cannot find landuse and soil database {0}", this.plantSoilDatabase), this.isBatch);
                return new List<string>();
            }
            var isProjDb = Path.Equals(this.plantSoilDatabase, this.dbFile);
            var isRefDb = Path.Equals(this.plantSoilDatabase, this.dbRefFile);
            var ignoreList = new List<string>();
            SQLiteConnection localConn = null;
            if (isProjDb) {
                localConn = this.conn;
            } else if (isRefDb) {
                localConn = this.connRef;
                if (tableName == "plant") {
                    ignoreList.Add("plant_ini");
                    ignoreList.Add("plant_ini_item");
                }
            } else {
                localConn = new SQLiteConnection(string.Format("Data Source={0}", this.plantSoilDatabase));
                localConn.Open();
            }
            if (localConn is null) {
                return new List<string>();
            }
            comboBox.Items.Clear();
            var tableNames = new List<string>();
            var sql = "SELECT name FROM sqlite_master WHERE TYPE='table'";
            var cmd = new SQLiteCommand(sql, localConn);
            using (var reader = cmd.ExecuteReader()) {
                try {
                    while (reader.Read()) {
                        var table = reader.GetString(0);
                        if (!ignoreList.Contains(table) && table.Contains(tableName)) {
                            comboBox.Items.Add(table);
                            tableNames.Add(table);
                        }
                    }
                    comboBox.Items.Add(Parameters._USECSV);
                    return tableNames;
                } catch (Exception) {
                    Utils.exceptionError(string.Format("Could not read tables in landuse and soil database {0}", this.plantSoilDatabase), this.isBatch);
                    return new List<string>();
                } finally {
                    if (!(isProjDb || isRefDb)) {
                        localConn.Close();
                        localConn.Dispose();
                    }
                }
            }
        }

        // Collect landuse codes from landuseTable and create lookup tables.
        public virtual bool populateLanduseCodes(string landuseTable, bool useGridModel) {
            var OK = true;
            this.landuseCodes.Clear();
            this._landuseTranslate.Clear();
            this.landuseIds.Clear();
            this.urbanIds.Clear();
            this.floodWetlandCrop = -1;
            this.upslopeWetlandCrop = -1;
            this.playaCrop = -1;
            var maxId = 0;
            var sql = DBUtils.sqlSelect(landuseTable, "LANDUSE_ID, SWAT_CODE", "", "");
            var cmd = new SQLiteCommand(sql, this.conn);
            using (var reader = cmd.ExecuteReader()) {
                try {
                    while (reader.Read()) {
                        var nxt = Convert.ToInt32(reader.GetValue(0));
                        maxId = Math.Max(maxId, nxt);
                        var landuseCode = reader.GetString(1).Trim();
                        // replace WETW with WATR.
                        // WATR is converted to WETW later, but for now it is needed as it may be added to pond, reservoir or lake
                        // if this is not done it is possible for WETW to appear twice in the hrus table for what should be a single HRU.
                        if (landuseCode.ToUpper() == "WETW") {
                            landuseCode = "WATR";
                        }
                        if (nxt == 0) {
                            this.defaultLanduse = nxt;
                            if (useGridModel) {
                                Utils.loginfo(string.Format("Default landuse set to {0}", landuseCode));
                            }
                        } else if (this.defaultLanduse < 0) {
                            this.defaultLanduse = nxt;
                            if (useGridModel) {
                                Utils.loginfo(string.Format("Default landuse set to {0}", landuseCode));
                            }
                        }
                        // check if code already defined
                        var equiv = nxt;
                        foreach (var (key, code) in this.landuseCodes) {
                            if (code.ToUpper() == landuseCode.ToUpper()) {
                                equiv = key;
                                break;
                            }
                        }
                        if (equiv == nxt) {
                            // landuseCode was not already defined
                            if (landuseCode.ToUpper() == "WETM") {
                                this.floodWetlandCrop = nxt;
                            }
                            if (landuseCode.ToUpper() == "WETN") {
                                this.upslopeWetlandCrop = nxt;
                            } else if (landuseCode.ToUpper() == "WATR") {
                                // could have been 'WETW' originally
                                this.playaCrop = nxt;
                            }
                            if (!this.storeLanduseCode(nxt, landuseCode)) {
                                OK = false;
                            }
                        } else {
                            this.storeLanduseTranslate(nxt, equiv);
                        }
                    }
                    if (this.floodWetlandCrop < 0) {
                        maxId += 1;
                        this.floodWetlandCrop = maxId;
                        if (!this.storeLanduseCode(maxId, "WETM")) {
                            OK = false;
                        }
                    }
                    if (this.upslopeWetlandCrop < 0) {
                        maxId += 1;
                        this.upslopeWetlandCrop = maxId;
                        if (!this.storeLanduseCode(maxId, "WETN")) {
                            OK = false;
                        }
                    }
                    if (this.playaCrop < 0) {
                        maxId += 1;
                        this.playaCrop = maxId;
                        if (!this.storeLanduseCode(maxId, "WETW")) {
                            OK = false;
                        }
                    }
                } catch (Exception) {
                    Utils.exceptionError(string.Format("Could not read table {0} in project database {1}", landuseTable, this.dbFile), this.isBatch);
                    return false;
                }
            }
            return OK;
        }

        // Make key lid equivalent to key equiv, 
        //         where equiv is a key in landuseCodes.
        //         
        public virtual void storeLanduseTranslate(int lid, int equiv) {
            if (!this._landuseTranslate.ContainsKey(lid)) {
                this._landuseTranslate[lid] = equiv;
            }
        }

        // Translate a landuse id to its equivalent lid 
        //         in landuseCodes, if any.
        //         
        public virtual int translateLanduse(int lid) {
            this.landuseVals.Add(lid);
            int temp = lid;
            if (this._landuseTranslate.TryGetValue(lid, out temp)) {
                return temp;
            } else {
                return lid;
            }
        }

        // Store landuse codes in lookup tables.
        public virtual bool storeLanduseCode(int landuseCat, string landuseCode) {
            var landuseId = 0;
            var urbanId = 0;
            string table = null;
            var OK = true;
            // make sure no extra spaces and use lower case
            landuseCode = landuseCode.Trim().ToLower();
            if (landuseCode == "barr") {
                // replaced by NULL later
                // use -1 as the landuseId
                this.landuseCodes[landuseCat] = landuseCode;
                this.landuseIds[landuseCat] = -1;
                return true;
            } else if (landuseCode == "watr") {
                // no longer in plant table; replaced by WETW later
                // but needed for now as may form pond, reservoir or be added to lake
                this.landuseCodes[landuseCat] = landuseCode;
                this.landuseIds[landuseCat] = 0;
                this.waterLanduse = landuseCat;
                return true;
            }
            var database = this.plantSoilDatabase;
            var isProjDb = File.Equals(database, this.dbFile);
            var isRefDb = File.Equals(database, this.dbRefFile);
            SQLiteConnection localConn = null;
            if (isProjDb) {
                localConn = this.conn;
            } else if (isRefDb) {
                localConn = this.connRef;
            } else {
                localConn = new SQLiteConnection(string.Format("Data Source={0}", database));
                localConn.Open();
            }
            if (localConn is null) {
                Utils.error(string.Format("Failed to connect to database {0} to read landuse tables", database), this.isBatch);
                return false;
            }
            try {
                // look in plant database first
                table = this.plantTable;
                var sqlp = DBUtils.sqlSelect(table, "id", "", "name=@p1 COLLATE NOCASE");
                var cmdp = new SQLiteCommand(sqlp, localConn);
                cmdp.Parameters.Clear();
                cmdp.Parameters.AddWithValue("@p1", landuseCode);
                using (SQLiteDataReader readerp = cmdp.ExecuteReader()) {
                    if (!readerp.HasRows) {
                        table = this.urbanTable;
                        var sqlu = DBUtils.sqlSelect(table, "id", "", "name=@p1 COLLATE NOCASE");
                        var cmdu = new SQLiteCommand(sqlu, localConn);
                        cmdp.Parameters.Clear();
                        cmdu.Parameters.AddWithValue("@p1", landuseCode);
                        using (SQLiteDataReader readeru = cmdu.ExecuteReader()) {
                            if (!readeru.HasRows) {
                                if (this.isHUC || this.isHAWQS) {
                                    //// TODO: plant table needs updating: don't generate lots of error messages: just use AGRL for now
                                    //this.landuseCodes[landuseCat] = "agrl";
                                    //this.landuseIds[landuseCat] = 2;
                                    this.landuseCodes[landuseCat] = landuseCode;
                                    return true;
                                }
                                Utils.error(string.Format("No data for landuse {0}", landuseCode), this.isBatch);
                                OK = false;
                            } else {
                                readeru.Read();
                                urbanId = Convert.ToInt32(readeru.GetValue(0));
                                this.urbanIds[landuseCat] = urbanId;
                            }
                        }
                    } else {
                        readerp.Read();
                        landuseId = Convert.ToInt32(readerp.GetValue(0));
                    }
                    this.landuseCodes[landuseCat] = landuseCode;
                    this.landuseIds[landuseCat] = landuseId;
                    return OK;
                }
            }
            catch (Exception ex) {
                Utils.exceptionError(string.Format("Could not read table {0} in database {1}: {2}", table, database, ex.Message), this.isBatch);
                return false;
            }
            finally {
                if (!(isProjDb || isRefDb)) {
                    localConn.Close();
                    localConn.Dispose();
                }
            }
        }

        // Return landuse code of landuse category lid.
        public virtual string getLanduseCode(int lid) {
            var lid1 = this.translateLanduse(lid);
            string code = null;
            if (this.landuseCodes.TryGetValue(lid1, out code)) {
                return code;
            }
            if (this._undefinedLanduseIds.Contains(lid)) {
                return lid.ToString();
            } else {
                this._undefinedLanduseIds.Add(lid);
                var @string = lid.ToString();
                Utils.error(string.Format("Unknown landuse value {0}", @string), this.isBatch);
                return @string;
            }
        }

        // Return landuse category (value in landuse map) for code, 
        //         adding to lookup tables if necessary.
        //         
        public virtual int getLanduseCat(string landuseCode) {
            foreach (var (category, code) in this.landuseCodes) {
                if (code.ToLower() == landuseCode.ToLower()) {
                    return category;
                }
            }
            // we have a new landuse from splitting
            // first find a new category: maximum existing ones plus 1
            var max1 = Enumerable.Max(this.landuseCodes.Keys);
            var max2 = this._landuseTranslate.Count > 0 ? Enumerable.Max(this._landuseTranslate.Keys) : 0;
            var cat = Math.Max(max1, max2) + 1;
            this.landuseCodes[cat] = landuseCode;
            // now add to landuseIds or urbanIds table
            this.storeLanduseCode(cat, landuseCode);
            return cat;
        }

        // HUC only.  Return True if landuse counts as agriculture.
        public virtual bool isAgriculture(int landuse) {
            return 81 < landuse && landuse < 91 || 99 < landuse && landuse < 567;
        }

        // Store names for soil categories.
        public virtual bool populateSoilNames(string soilTable, bool useGridModel) {
            this.soilNames.Clear();
            this.soilTranslate.Clear();
            var columns = new List<string>();
            // since several forms of soil name are allowed, look for SOIL_ID as one of them and 
            // take the other as SNAM
            var cmdr = new SQLiteCommand($"PRAGMA table_info({soilTable})", this.conn);
            using (var dr = cmdr.ExecuteReader()) {
                while (dr.Read()) { //loop through the various columns and their info
                    columns.Add(dr.GetString(1)); // name field has index 1
                }
            }
            var idIndex = columns.IndexOf("SOIL_ID");
            if (idIndex < 0) {
                Utils.error(string.Format("No column SOIL_ID in soil lookup table {0}", soilTable), this.isBatch);
                return false;
            }
            string nameHead = null;
            if (idIndex == 0) {
                nameHead = columns[1];
            } else {
                nameHead = columns[0];
            }
            var sql = DBUtils.sqlSelect(soilTable, "*", "", "");
            var cmd = new SQLiteCommand(sql, this.conn);
            using (var reader = cmd.ExecuteReader()) {
                if (reader.HasRows) {
                    var firstRow = true;
                    var nameIndex = reader.GetOrdinal(nameHead);
                    while (reader.Read()) {
                        var nxt = Convert.ToInt32(reader.GetValue(idIndex));
                        var soilName = reader.GetString(nameIndex).Trim();
                        if (nxt == 0) {
                            this.defaultSoil = nxt;
                            if (useGridModel) {
                                Utils.loginfo(string.Format("Default soil set to {0}", soilName));
                            }
                        } else if (this.defaultSoil < 0) {
                            this.defaultSoil = nxt;
                            if (useGridModel) {
                                Utils.loginfo(string.Format("Default soil set to {0}", soilName));
                            }
                        }
                        if (firstRow && this.useSTATSGO) {
                            firstRow = false;
                            // determine what kind of name is being used
                            this.addSeqn = false;
                            this.addName = false;
                            this.useS5id = false;
                            if (this._MUIDPATTERN.IsMatch(soilName)) {
                            } else if (this._MUIDPLUSSEQPATTERN.IsMatch(soilName)) {
                                this.addSeqn = true;
                            } else if (this._MUIDPLUSNAMEPATTERN.IsMatch(soilName)) {
                                this.addName = true;
                            } else if (this._S5IDPATTERN.IsMatch(soilName)) {
                                this.useS5id = true;
                            } else {
                                Utils.error(string.Format("Cannot recognise {0} as an muid, muid+seqn, or s5id", soilName), this.isBatch);
                                return false;
                            }
                        }
                        // check if code already defined
                        var equiv = nxt;
                        foreach (var (key, name) in this.soilNames) {
                            if (name == soilName) {
                                equiv = key;
                                break;
                            }
                        }
                        if (equiv == nxt) {
                            // soilName not found
                            this.soilNames[nxt] = soilName;
                        } else {
                            this.storeSoilTranslate(nxt, equiv);
                        }
                    }
                } else {
                    Utils.error(string.Format("Could not read table {0} in project database {1}", soilTable, this.dbFile), this.isBatch);
                    return false;
                }
            }
            return true;
        }


        // Return name for soil id sid.
        public virtual (string, bool) getSoilName(int sid) {
            if (this.useSSURGO) {
                this.usedSSURGOSoils.Add(sid);
                return (sid.ToString(), true);
            }
            var (sid1, OK) = this.translateSoil(sid);
            // first try used soil names
            string name = null;
            if (this.usedSoilNames.TryGetValue(sid1, out name)) {
                return (name, OK);
            } else if (this.soilNames.TryGetValue(sid1, out name)) {
                this.usedSoilNames[sid1] = name;
                return (name, true);
            }
            if (this._undefinedSoilIds.Contains(sid)) {
                return (sid.ToString(), false);
            } else {
                var @string = sid.ToString();
                this._undefinedSoilIds.Add(sid);
                Utils.error(string.Format("Unknown soil value {0}", @string), this.isBatch);
                return (@string, false);
            }
        }

        public static string _SOILS_SOL_TABLE = @"
    (id        INTEGER NOT NULL PRIMARY KEY,
    name       TEXT NOT NULL,
    hyd_grp    TEXT NOT NULL,
    dp_tot     REAL NOT NULL,
    anion_excl REAL NOT NULL,
    perc_crk   REAL NOT NULL,
    texture    TEXT,
    description     TEXT)
    ";

        public static string _SOILS_SOL_NAME = "soils_sol";

        public static string _SOILS_SOL_LAYER_TABLE = @"
    (id       INTEGER NOT NULL PRIMARY KEY,
    soil_id   INTEGER NOT NULL,
    layer_num INTEGER NOT NULL,
    dp        REAL    NOT NULL,
    bd        REAL    NOT NULL,
    awc       REAL    NOT NULL,
    soil_k    REAL    NOT NULL,
    carbon    REAL    NOT NULL,
    clay      REAL    NOT NULL,
    silt      REAL    NOT NULL,
    sand      REAL    NOT NULL,
    rock      REAL    NOT NULL,
    alb       REAL    NOT NULL,
    usle_k    REAL    NOT NULL,
    ec        REAL    NOT NULL,
    caco3     REAL,
    ph        REAL)
    ";

        public static string _SOILS_SOL_LAYER_NAME = "soils_sol_layer";

        public static string _PLANTS_PLT_TABLE = @"
    (id         INTEGER       NOT NULL PRIMARY KEY,
    name        TEXT          NOT NULL,
    plnt_typ    TEXT          NOT NULL,
    gro_trig    TEXT          NOT NULL,
    nfix_co     REAL          NOT NULL,
    days_mat    REAL          NOT NULL,
    bm_e        REAL          NOT NULL,
    harv_idx    REAL          NOT NULL,
    lai_pot     REAL          NOT NULL,
    frac_hu1    REAL          NOT NULL,
    lai_max1    REAL          NOT NULL,
    frac_hu2    REAL          NOT NULL,
    lai_max2    REAL          NOT NULL,
    hu_lai_decl REAL          NOT NULL,
    dlai_rate   REAL          NOT NULL,
    can_ht_max  REAL          NOT NULL,
    rt_dp_max   REAL          NOT NULL,
    tmp_opt     REAL          NOT NULL,
    tmp_base    REAL          NOT NULL,
    frac_n_yld  REAL          NOT NULL,
    frac_p_yld  REAL          NOT NULL,
    frac_n_em   REAL          NOT NULL,
    frac_n_50   REAL          NOT NULL,
    frac_n_mat  REAL          NOT NULL,
    frac_p_em   REAL          NOT NULL,
    frac_p_50   REAL          NOT NULL,
    frac_p_mat  REAL          NOT NULL,
    harv_idx_ws REAL          NOT NULL,
    usle_c_min  REAL          NOT NULL,
    stcon_max   REAL          NOT NULL,
    vpd         REAL          NOT NULL,
    frac_stcon  REAL          NOT NULL,
    ru_vpd      REAL          NOT NULL,
    co2_hi      REAL          NOT NULL,
    bm_e_hi     REAL          NOT NULL,
    plnt_decomp REAL          NOT NULL,
    lai_min     REAL          NOT NULL,
    bm_tree_acc REAL          NOT NULL,
    yrs_mat     REAL          NOT NULL,
    bm_tree_max REAL          NOT NULL,
    ext_co      REAL          NOT NULL,
    leaf_tov_mn REAL          NOT NULL,
    leaf_tov_mx REAL          NOT NULL,
    bm_dieoff   REAL          NOT NULL,
    rt_st_beg   REAL          NOT NULL,
    rt_st_end   REAL          NOT NULL,
    plnt_pop1   REAL          NOT NULL,
    frac_lai1   REAL          NOT NULL,
    plnt_pop2   REAL          NOT NULL,
    frac_lai2   REAL          NOT NULL,
    frac_sw_gro REAL          NOT NULL,
    aeration    REAL          NOT NULL,
    rsd_pctcov  REAL          NOT NULL,
    rsd_covfac  REAL          NOT NULL,
    description TEXT)
    ";

        public static string _PLANTS_PLT_NAME = "plants_plt";

        public static string _URBAN_URB_TABLE = @"
    (id          INTEGER       NOT NULL PRIMARY KEY,
    name        TEXT          NOT NULL,
    frac_imp    REAL          NOT NULL,
    frac_dc_imp REAL          NOT NULL,
    curb_den    REAL          NOT NULL,
    urb_wash    REAL          NOT NULL,
    dirt_max    REAL          NOT NULL,
    t_halfmax   REAL          NOT NULL,
    conc_totn   REAL          NOT NULL,
    conc_totp   REAL          NOT NULL,
    conc_no3n   REAL          NOT NULL,
    urb_cn      REAL          NOT NULL,
    description TEXT)
    ";

        public static string _URBAN_URB_NAME = "urban_urb";

        // Count the columns in table in database currently connected to localConn.
        public static int countCols(string table, SQLiteConnection localConn) {
            try {
                var sql = string.Format("SELECT COUNT(*) FROM pragma_table_info('{0}')", table);
                using (var cmd = new SQLiteCommand(sql, localConn)) {
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch (Exception) {
                return -1;
            }
        }

        // Write the soils_sol and soils_sol_layer tables in the project database.
        public virtual bool writeSoilsTable() {
            string database;
            if (this.useSTATSGO || this.useSSURGO) {
                database = this.soilDatabase;
            } else {
                database = this.plantSoilDatabase;
            }
            var isProjDb = File.Equals(database, this.dbFile);
            var isRefDb = File.Equals(database, this.dbRefFile);
            SQLiteConnection localConn = null;
            if (isProjDb) {
                localConn = this.conn;
            } else if (isRefDb) {
                localConn = this.connRef;
            } else {
                localConn = new SQLiteConnection(string.Format("DataSource={0}", database));
                localConn.Open();
            }
            var usersoilTable = this.getUsersoilTable();
            var errorReported = false;
            try {
                // single usersoil table has 152 columns, one with separate layer table has 11: 
                // we leave room for expansion by using 20 as limit
                var columnCount = DBUtils.countCols(usersoilTable, localConn);
                if (columnCount < 0) {
                    Utils.error(string.Format("Could not find table {0} in soil database {1}", usersoilTable, database), this.isBatch);
                    return false;
                }
                var hasSeparateLayerTable = columnCount < 20;
                string sqlLayer = null;
                if (hasSeparateLayerTable) {
                    var layerTable = usersoilTable + "_layer";
                    if (!this.hasTableConn(localConn, layerTable)) {
                        Utils.error(string.Format("Table {0} has {1} columns but cannot find layer table {2} in soil database {3}", usersoilTable, columnCount, layerTable, database), this.isBatch);
                        return false;
                    }
                    sqlLayer = DBUtils.sqlSelect(layerTable, "*", "layer_num", "soil_id=@p1");
                }

                using (var transaction = this.conn.BeginTransaction()) {
                    using (SQLiteCommand writeCmd = new SQLiteCommand("", this.conn, transaction), readCmd = new SQLiteCommand("", localConn)) {
                        writeCmd.CommandText = string.Format("DROP TABLE IF EXISTS {0}", DBUtils._SOILS_SOL_NAME);
                        writeCmd.ExecuteNonQuery();
                        writeCmd.CommandText = string.Format("CREATE TABLE {0} {1}", DBUtils._SOILS_SOL_NAME, DBUtils._SOILS_SOL_TABLE);
                        writeCmd.ExecuteNonQuery();
                        writeCmd.CommandText = string.Format("DROP TABLE IF EXISTS {0}", DBUtils._SOILS_SOL_LAYER_NAME);
                        writeCmd.ExecuteNonQuery();
                        writeCmd.CommandText = string.Format("CREATE TABLE {0} {1}", DBUtils._SOILS_SOL_LAYER_NAME, DBUtils._SOILS_SOL_LAYER_TABLE);
                        writeCmd.ExecuteNonQuery();
                        var insert = string.Format("INSERT INTO {0} VALUES(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8)", DBUtils._SOILS_SOL_NAME);
                        var insertLayer = string.Format("INSERT INTO {0} VALUES(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15,@p16,@p17)", DBUtils._SOILS_SOL_LAYER_NAME);
                        string sql = null;
                        if (this.useSTATSGO) {
                            if (this.addSeqn) {
                                sql = DBUtils.sqlSelect(usersoilTable, "*", "id ASC", "muid=@p1 AND seqn=@p2");
                            } else if (this.addName) {
                                sql = DBUtils.sqlSelect(usersoilTable, "*", "id ASC", "muid=@p1 AND name=@p2");
                            } else if (this.useS5id) {
                                sql = DBUtils.sqlSelect(usersoilTable, "*", "id ASC", "s5id=@p1");
                            } else {
                                sql = DBUtils.sqlSelect(usersoilTable, "*", "id ASC", "muid=@p1");
                            }
                        } else if (this.useSSURGO) {
                            sql = DBUtils.sqlSelect(usersoilTable, "*", "", "muid=@p1");
                        } else if (hasSeparateLayerTable) {
                            // main and layer tables can get confused
                            if (usersoilTable.EndsWith("_layer")) {
                                usersoilTable = usersoilTable.Substring(0, usersoilTable.Length - 6);
                                Utils.loginfo("usersoil table reduced to " + usersoilTable);
                            }
                            sql = DBUtils.sqlSelect(usersoilTable, "*", "", "name=@p1");
                        } else {
                            sql = DBUtils.sqlSelect(usersoilTable, "*", "", "SNAM=@p1");
                        }
                        var sid = 0;
                        var lid = 0;
                        readCmd.CommandText = sql;
                        if (this.useSSURGO) {
                            foreach (var ssurgoId in this.usedSSURGOSoils) {
                                readCmd.Parameters.Clear();
                                readCmd.Parameters.AddWithValue("@p1", ssurgoId);
                                using (var readerSSURGO = readCmd.ExecuteReader()) {
                                    if (!readerSSURGO.HasRows) {
                                        if (this.isHUC || this.isHAWQS) {
                                            continue;
                                        }
                                        if (!errorReported) {
                                            Utils.error(string.Format("SSURGO soil {0} (and perhaps others) not defined in {1} table in database {2}.  {3} table incomplete.  You might like to try using the soil raster https://swat.tamu.edu/docs/swat/SSURGO-2023.tif.", ssurgoId, usersoilTable, database, DBUtils._SOILS_SOL_NAME), this.isBatch);
                                            errorReported = !this.isBatch;
                                        }
                                        Utils.logerror(string.Format("SSURGO soil {0} not defined", ssurgoId));
                                    }
                                    sid += 1;
                                    if (hasSeparateLayerTable) {
                                        lid = this.writeUsedSoilRowSeparate(sid, lid, ssurgoId.ToString(), writeCmd, readerSSURGO, insert, insertLayer, localConn, sqlLayer);
                                    } else {
                                        lid = this.writeUsedSoilRow(sid, lid, ssurgoId.ToString(), writeCmd, readerSSURGO, insert, insertLayer);
                                    }
                                }
                            }
                        } else {
                            foreach (string name in this.usedSoilNames.Values) {
                                // Utils.loginfo("Used soil " + name);
                                readCmd.Parameters.Clear();
                                if (this.useSTATSGO) {
                                    if (this.useS5id) {
                                        readCmd.Parameters.AddWithValue("@p1", name);
                                    } else if (this.addSeqn || this.addName) {
                                        readCmd.Parameters.AddWithValue("@p1", name.Substring(0, 5));
                                        readCmd.Parameters.AddWithValue("@p2", name.Substring(6));
                                    } else {
                                        readCmd.Parameters.AddWithValue("@p1", name);
                                    }
                                } else {
                                    readCmd.Parameters.AddWithValue("@p1", name);
                                }
                                using (var reader = readCmd.ExecuteReader()) {
                                    if (!reader.HasRows) {
                                        Utils.error(string.Format("Soil name {0} (and perhaps others) not defined in {1} table in database {2}.  {3} table not written.", name, usersoilTable, database, DBUtils._SOILS_SOL_NAME), this.isBatch);
                                        return false;
                                    }
                                    sid += 1;
                                    if (hasSeparateLayerTable) {
                                        lid = this.writeUsedSoilRowSeparate(sid, lid, name, writeCmd, reader, insert, insertLayer, localConn, sqlLayer);
                                    } else {
                                        lid = this.writeUsedSoilRow(sid, lid, name, writeCmd, reader, insert, insertLayer);
                                    }
                                }
                            }
                        }
                    }
                    transaction.Commit();
                }
                return true;
            } catch (Exception ex) {
                Utils.exceptionError(string.Format("Could not create {2} and {3} tables from {0} table in soil database {1}: {4}", usersoilTable, database, DBUtils._SOILS_SOL_NAME, DBUtils._SOILS_SOL_LAYER_NAME, ex.Message), this.isBatch);
                return false;
            } finally {
                //this.hashDbTable(this.conn, DBUtils._SOILS_SOL_NAME);
                //this.hashDbTable(this.conn, DBUtils._SOILS_SOL_LAYER_NAME);
                if (!(isProjDb || isRefDb)) {
                    localConn.Close();
                    localConn.Dispose();
                }
            }
        }

        // Write data from one row of usersoil table to soils_sol and soils_sol_layer tables in project database.
        public virtual int writeUsedSoilRow(
            int sid,
            int lid,
            string name,
            SQLiteCommand writeCmd,
            SQLiteDataReader reader,
            string insert,
            string insertLayer) {
            reader.Read();  
            writeCmd.CommandText = insert;
            writeCmd.Parameters.Clear();
            writeCmd.Parameters.AddWithValue("@p1", sid);
            writeCmd.Parameters.AddWithValue("@p2", name);
            writeCmd.Parameters.AddWithValue("@p3", reader.GetValue(7));
            writeCmd.Parameters.AddWithValue("@p4", reader.GetValue(8));
            writeCmd.Parameters.AddWithValue("@p5", reader.GetValue(9));
            writeCmd.Parameters.AddWithValue("@p6", reader.GetValue(10));
            writeCmd.Parameters.AddWithValue("@p7", reader.GetValue(11));
            writeCmd.Parameters.AddWithValue("@p8", null);
            writeCmd.ExecuteNonQuery();
            var startLayer1 = 12; // index of SOL_Z1
            var layerWidth = 12;  // number of entries per layer
            var startCal = 132;   // index of SOL_CAL1
            var startPh = 142;    // index of SOL_PH1
            writeCmd.CommandText = insertLayer;
            var nLayers = Convert.ToInt32(reader.GetValue(6));
            foreach (var i in Enumerable.Range(0, nLayers)) {
                lid += 1;
                var startLayer = startLayer1 + i * layerWidth;
                writeCmd.Parameters.AddWithValue("@p1", lid);
                writeCmd.Parameters.AddWithValue("@p2", sid);
                writeCmd.Parameters.AddWithValue("@p3", i + 1);
                for (int j = 0; j < layerWidth; j++) {
                    writeCmd.Parameters.AddWithValue(string.Format("@p{0}", j + 4), reader.GetValue(startLayer + j));
                }
                writeCmd.Parameters.AddWithValue("@p16", reader.GetValue(startCal + i));
                writeCmd.Parameters.AddWithValue("@p17", reader.GetValue(startPh + i));
                writeCmd.ExecuteNonQuery();
            }
            return lid;
        }

        // Write data from one row of usersoil table plus separate layer table 
        //         to soils_sol and soils_sol_layer tables.
        //         
        public virtual int writeUsedSoilRowSeparate(
            int sid,
            int lid,
            string name,
            SQLiteCommand writeCmd,
            SQLiteDataReader reader,
            string insert,
            string insertLayer,
            SQLiteConnection localConn,
            string sqlLayer)
        {
            reader.Read();
            writeCmd.CommandText = insert;
            writeCmd.Parameters.Clear();
            writeCmd.Parameters.AddWithValue("@p1", sid);
            writeCmd.Parameters.AddWithValue("@p2", name);
            //Utils.loginfo(String.Format("Value |{0}|", reader.GetValue(6)));
            writeCmd.Parameters.AddWithValue("@p3", reader.GetValue(6));
            //Utils.loginfo(String.Format("Value |{0}|", reader.GetValue(7)));
            writeCmd.Parameters.AddWithValue("@p4", reader.GetValue(7));
            //Utils.loginfo(String.Format("Value |{0}|", reader.GetValue(8)));
            writeCmd.Parameters.AddWithValue("@p5", reader.GetValue(8));
            //Utils.loginfo(String.Format("Value |{0}|", reader.GetValue(9)));
            writeCmd.Parameters.AddWithValue("@p6", reader.GetValue(9));
            //Utils.loginfo(String.Format("Value |{0}|", reader.GetValue(10)));
            writeCmd.Parameters.AddWithValue("@p7", reader.GetValue(10));
            // check whether there is a non-null description item
            if (reader.VisibleFieldCount == 11) {
                writeCmd.Parameters.AddWithValue("@p8", null);
            } else {
                writeCmd.Parameters.AddWithValue("@p8", reader.GetValue(11));
            }
            writeCmd.ExecuteNonQuery(); 
            SQLiteCommand readCmd = new SQLiteCommand(sqlLayer, localConn);
            readCmd.Parameters.Clear();
            var soilId = reader.GetValue(0);
            readCmd.Parameters.AddWithValue("@p1", soilId);
            using (var readLayer = readCmd.ExecuteReader()) {
                if (!readLayer.HasRows) {
                    Utils.error(string.Format("Failed to find soil layers in table {0} with soil_id {1}", this.getUsersoilTable() + "_layer", soilId), this.isBatch);
                    return lid;
                }
                writeCmd.CommandText = insertLayer;
                while (readLayer.Read()) {
                    writeCmd.Parameters.Clear();
                    lid += 1;
                    writeCmd.Parameters.AddWithValue("@p1", lid);
                    writeCmd.Parameters.AddWithValue("@p2", sid);
                    // layer table has 17 fields
                    for (int i = 2; i < 17; i++) {
                        writeCmd.Parameters.AddWithValue(string.Format("@p{0}", i + 1), readLayer.GetValue(i));
                    }
                    writeCmd.ExecuteNonQuery();
                }
            }
            return lid;
        }

        // Make key sid equivalent to key equiv, 
        //         where equiv is a key in soilNames.
        //         
        public virtual void storeSoilTranslate(int sid, int equiv) {
            if (!this.soilTranslate.ContainsKey(sid)) {
                this.soilTranslate[sid] = equiv;
            }
        }

        // Translate a soil id to its equivalent id in soilNames, plus flag indicating lookup success.
        public virtual (int, bool) translateSoil(int sid) {
            this.soilVals.Add(sid);
            if (this.useSSURGO) {
                if (this.isHUC || this.isHAWQS) {
                    return this.translateSSURGOSoil(sid);
                } else {
                    return (sid, true);
                }
            }
            int temp = sid;
            if (this.soilTranslate.TryGetValue(sid, out temp)) {
                return (temp, true);
            } else {
                return (sid, true);
            }
        }

        // Return soil id, possibly changed to Parameters._SSURGOWater,
        // plus flag to indicate success or failure in lookup.  
        //         Report undefined SSURGO soils.  Only used with HUC and HAWQS.
        public virtual (int, bool) translateSSURGOSoil(int sid) {
            if (this._undefinedSoilIds.Contains(sid)) {
                return (this.SSURGOUndefined, false);
            }
            var muid = -1;
            if (this.SSURGOsoils.TryGetValue(sid, out muid) && muid > 0) { return (muid, true); }
            //var sql = DBUtils.sqlSelect("statsgo_ssurgo_lkey", "Source, MUKEY", "", "LKEY=@p1");
            //var lookup_cmd = new SQLiteCommand(sql, this.conn);
            //lookup_cmd.Parameters.AddWithValue("@p1", sid);
            //using (var lookup_reader = lookup_cmd.ExecuteReader()) {
            //    if (!lookup_reader.HasRows) {
            //        Utils.information(string.Format("WARNING: SSURGO soil map value {0} not defined as lkey in statsgo_ssurgo_lkey in project {1}", sid, this.projName), this.isBatch, logFile: this.logFile);
            //        this._undefinedSoilIds.Add(sid);
            //        return (sid, false);
            //    }
            //    lookup_reader.Read();
            //    // only an information issue, not an error for now 
            //    if (lookup_reader.GetString(0).ToUpper().Trim() == "STATSGO") {
            //        Utils.information(string.Format("SSURGO soil map value {0} is a STATSGO soil according to statsgo_ssurgo_lkey", sid), this.isBatch, logFile: this.logFile);
            //        // self._undefinedSoilIds.append(sid)
            //        // return sid
            //    }
            var msg2 = this.isBatch ? "" : " (and perhaps others)";
            var msg3 = this.isBatch ? "" : "See QSWAT+ log messages.";
            var sql = DBUtils.sqlSelect("ssurgo", "texture", "", "muid=@p1");
            var cmd = new SQLiteCommand(sql, this.SSURGOConn);
            cmd.Parameters.AddWithValue("@p1", sid);
            using (var reader = cmd.ExecuteReader()) {
                if (!reader.HasRows) {
                    Utils.loginfo(string.Format("SSURGO MUID {0} not defined", sid));
                    if (!this.SSURGOUndefinedMapValueReported) {
                        if (!this.isHAWQS) { // minimise interruptions to HAWQS runs
                            Utils.information(string.Format("WARNING: SSURGO MUID {0}{1} not defined in project {2}.  {3} You might like to try using the soil raster https://swat.tamu.edu/docs/swat/SSURGO-2023.tif.",
                                sid, msg2, this.projName, msg3), this.isBatch, logFile: this.logFile);
                        }
                        this.SSURGOUndefinedMapValueReported = !this.isBatch;
                    }
                    this._undefinedSoilIds.Add(sid);
                    return (this.SSURGOUndefined, false);
                }
                //if row[0].lower().Trim() == 'water':
                Regex patt = new Regex(this.waterPattern);
                if (patt.IsMatch(reader.GetString(0))) {
                    this.SSURGOsoils[sid] = Parameters._SSURGOWater;
                    return (Parameters._SSURGOWater, true);
                } else {
                    this.SSURGOsoils[sid] = sid;
                    return (sid, true);
                }
            }
        }


        // Write the plants_plt and urban_urb tables in the project database.
        public virtual bool writeLanduseTables() {
            string sql;
            var isProjDb = File.Equals(this.plantSoilDatabase, this.dbFile);
            var isRefDb = File.Equals(this.plantSoilDatabase, this.dbRefFile);
            var copyPlant = true;
            var copyUrban = true;
            SQLiteConnection localConn;
            if (isProjDb) {
                localConn = this.conn;
                // avoid trying to copy table to itself
                copyPlant = this.plantTable != DBUtils._PLANTS_PLT_NAME;
                copyUrban = this.urbanTable != DBUtils._URBAN_URB_NAME;
                if (!(copyPlant || copyUrban)) {
                    return true;
                }
            } else if (isRefDb) {
                localConn = this.connRef;
            } else {
                localConn = new SQLiteConnection(string.Format("DataSource={0}", this.plantSoilDatabase));
                localConn.Open();
            }
            try {
                using (var transaction = this.conn.BeginTransaction()) {
                    using (SQLiteCommand writeCmd = new SQLiteCommand("", this.conn, transaction)) {
                        if (copyPlant) {
                            sql = string.Format("DROP TABLE IF EXISTS {0}", DBUtils._PLANTS_PLT_NAME);
                            writeCmd.CommandText = sql;
                            writeCmd.ExecuteNonQuery();
                            sql = string.Format("CREATE TABLE {0} {1}", DBUtils._PLANTS_PLT_NAME, DBUtils._PLANTS_PLT_TABLE);
                            writeCmd.CommandText = sql;
                            writeCmd.ExecuteNonQuery();
                        }
                        if (copyUrban) {
                            sql = string.Format("DROP TABLE IF EXISTS {0}", DBUtils._URBAN_URB_NAME);
                            writeCmd.CommandText = sql;
                            writeCmd.ExecuteNonQuery();
                            sql = string.Format("CREATE TABLE {0} {1}", DBUtils._URBAN_URB_NAME, DBUtils._URBAN_URB_TABLE);
                            writeCmd.CommandText = sql;
                            writeCmd.ExecuteNonQuery();
                        }
                        var plantSelect = DBUtils.sqlSelect(this.plantTable, "*", "", "name=@p1 COLLATE NOCASE");
                        var urbanSelect = DBUtils.sqlSelect(this.urbanTable, "*", "", "name=@p1 COLLATE NOCASE");
                        // changed to copy all of plant and urban tables
                        // pid = 0
                        // uid = 0
                        using (var readCmd = new SQLiteCommand(localConn)) {
                            foreach (var crop in this.landuseVals) {
                                var name = this.getLanduseCode(crop);
                                if (name.ToUpper() == "BARR" || name.ToUpper() == "WATR") {
                                    // BARR will become NULL later, WATR will become WETW; neither in plant table
                                    continue;
                                }
                                var args = ValueTuple.Create(name);
                                // look first in plantTable
                                readCmd.Parameters.Clear();
                                readCmd.Parameters.AddWithValue("@p1", name);
                                readCmd.CommandText = plantSelect;
                                var found = false;
                                using (var readPlant = readCmd.ExecuteReader()) {
                                    found = readPlant.HasRows;
                                }
                                if (!found) {
                                    readCmd.CommandText = urbanSelect;
                                    using (var readUrban = readCmd.ExecuteReader()) {
                                        if (!readUrban.HasRows) {
                                            Utils.error(string.Format("Landuse name {0} not defined in {1} or {2} tables in database {3}.", name, this.plantTable, this.urbanTable, this.plantSoilDatabase), this.isBatch);
                                        }
                                    }
                                }
                            }
                            // copy all of plant and urban tables
                            var insertPlant = string.Format("INSERT INTO {0} VALUES(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15,@p16,@p17,@p18,@p19,@p20,@p21,@p22,@p23,@p24,@p25,@p26,@p27,@p28,@p29,@p30,@p31,@p32,@p33,@p34,@p35,@p36,@p37,@p38,@p39,@p40,@p41,@p42,@p43,@p44,@p45,@p46,@p47,@p48,@p49,@p50,@p51,@p52,@p53,@p54,@p55)", DBUtils._PLANTS_PLT_NAME);
                            var insertUrban = string.Format("INSERT INTO {0} VALUES(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13)", DBUtils._URBAN_URB_NAME);
                            if (copyPlant) {
                                var plantAll = DBUtils.sqlSelect(this.plantTable, "*", "", "");
                                readCmd.Parameters.Clear();
                                readCmd.CommandText = plantAll;
                                writeCmd.Parameters.Clear();
                                writeCmd.CommandText = insertPlant;
                                using (var reader = readCmd.ExecuteReader()) {
                                    while (reader.Read()) {
                                        for (int i = 0; i < 55; i++) {
                                            writeCmd.Parameters.AddWithValue(string.Format("@p{0}", i + 1), reader.GetValue(i));
                                        }
                                        writeCmd.ExecuteNonQuery();
                                    }
                                }
                            }
                            if (copyUrban) {
                                var urbanAll = DBUtils.sqlSelect(this.urbanTable, "*", "", "");
                                readCmd.Parameters.Clear();
                                readCmd.CommandText = urbanAll;
                                writeCmd.Parameters.Clear();
                                writeCmd.CommandText = insertUrban;
                                using (var reader = readCmd.ExecuteReader()) {
                                    while (reader.Read()) {
                                        for (int i = 0; i < 13; i++) {
                                            writeCmd.Parameters.AddWithValue(string.Format("@p{0}", i + 1), reader.GetValue(i));
                                        }
                                        writeCmd.ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                    }
                    transaction.Commit();
                }
                return true;
            }
            catch (Exception) {
                Utils.exceptionError(string.Format("Could not create {3} and {4} tables from {0} and {1} tables in landuse and soil database {2}", this.plantTable, this.urbanTable, this.plantSoilDatabase, DBUtils._PLANTS_PLT_NAME, DBUtils._URBAN_URB_NAME), this.isBatch);
                return false;
            }
            finally {
                // this.hashDbTable(this.conn, DBUtils._PLANTS_PLT_NAME);
                // this.hashDbTable(this.conn, DBUtils._URBAN_URB_NAME);
                if (!(isProjDb || isRefDb)) {
                    localConn.Close();
                    localConn.Dispose();
                }
            }
        }

        // Make list of all landuses in listBox.
        public List<string> populateAllLanduses(bool includeWATR = true) {
            var luses = new List<string>();
            string strng;
            string code;
            var isProjDb = File.Equals(this.plantSoilDatabase, this.dbFile);
            var isRefDb = File.Equals(this.plantSoilDatabase, this.dbRefFile);
            SQLiteConnection localConn;
            if (isProjDb) {
                localConn = this.conn;
            } else if (isRefDb) {
                localConn = this.connRef;
            } else {
                localConn = new SQLiteConnection(string.Format("DataSource={0}", this.plantSoilDatabase));
                localConn.Open();
            }
            var landuseSql = DBUtils.sqlSelect(this.plantTable, "name, description", "", "");
            var urbanSql = DBUtils.sqlSelect(this.urbanTable, "name, description", "", "");
            var cmd = new SQLiteCommand(localConn);
            try {
                cmd.CommandText = landuseSql;
                using (var reader = cmd.ExecuteReader()) {
                    while (reader.Read()) {
                        code = reader.GetString(0).ToUpper();
                        var include = code == "WATR" ? includeWATR : true;
                        if (include) {
                            if (reader.IsDBNull(1)) {
                                strng = code;
                            } else {
                                strng = code + " (" + reader.GetString(1) + ")";
                            }
                            luses.Add(strng);
                        }
                    }
                }
            } catch (Exception) {
                Utils.exceptionError(string.Format("Could not read table {0} in landuse and soil database {1}", this.plantTable, this.plantSoilDatabase), this.isBatch);
                if (!(isProjDb || isRefDb) ) {
                    localConn.Close();
                    localConn.Dispose();
                }
                return luses;
            }
            try {
                cmd.CommandText = urbanSql;
                using (var reader = cmd.ExecuteReader()) {
                    while (reader.Read()) {
                        code = reader.GetString(0).ToUpper();
                        var include = code == "WATR" ? includeWATR : true;
                        if (include) {
                            if (reader.IsDBNull(1)) {
                                strng = code;
                            } else {
                                strng = code + " (" + reader.GetString(1) + ")";
                            }
                            luses.Add(strng);
                        }
                    }
                }
            } catch (Exception) {
                Utils.exceptionError(string.Format("Could not read table {0} in landuse and soil database {1}", this.urbanTable, this.plantSoilDatabase), this.isBatch);
                return luses;
            } finally {
                if (!(isProjDb || isRefDb)) {
                    localConn.Close();
                    localConn.Dispose();
                }
            }
            luses.Sort();
            return luses;
        }

        // Return map of crop -> (code, description) for list or iterable of crop values.
        public Dictionary<int, (string, string)> getLanduseDescriptions(IEnumerable<int> crops) {
            var isProjDb = File.Equals(this.plantSoilDatabase, this.dbFile);
            var isRefDb = File.Equals(this.plantSoilDatabase, this.dbRefFile);
            SQLiteConnection localConn;
            if (isProjDb) {
                localConn = this.conn;
            } else if (isRefDb) {
                localConn = this.connRef;
            } else {
                localConn = new SQLiteConnection(string.Format("DataSource={0}", this.plantSoilDatabase));
                localConn.Open();
            }
            var cmd = new SQLiteCommand(localConn);
            var plantSql = DBUtils.sqlSelect(this.plantTable, "description", "", "name=@p1 COLLATE NOCASE");
            var urbanSql = DBUtils.sqlSelect(this.urbanTable, "description", "", "name=@p1 COLLATE NOCASE");
            var result = new Dictionary<int, (string, string)>();
            try {
                foreach (var crop in crops) {
                    var landuseCode = this.getLanduseCode(crop);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@p1", landuseCode);
                    // try plant table first
                    cmd.CommandText = plantSql;
                    using (var reader1 = cmd.ExecuteReader()) {
                        if (!reader1.HasRows) {
                            cmd.CommandText = urbanSql;
                            using (var reader2 = cmd.ExecuteReader()) {
                                if (!reader2.HasRows) {
                                    // avoid repeating error report
                                    result[crop] = (crop.ToString(), "");
                                } else {
                                    result[crop] = (landuseCode, reader2.GetString(1));
                                }
                            }
                        } else {
                            result[crop] = (landuseCode, reader1.GetString(1));
                        }
                    }
                }
            }
            finally {
                if (!(isProjDb || isRefDb)) {
                    localConn.Close();
                    localConn.Dispose();
                }
            }
            return result;
        }

        // Put all landuse codes except WATR from landuse values vals in combo box.
        public virtual void populateMapLanduses(IEnumerable<int> vals, ComboBox combo, bool includeWATR = true) {
            foreach (var i in vals) {
                var code = this.getLanduseCode(i);
                var include = code.ToUpper() == "WATR" ? includeWATR : true;
                if (include) {
                    // avoid duplicates
                    if (combo.Items.IndexOf(code) < 0) {
                        combo.Items.Add(code);
                    }
                }
            }
        }

        // Return index of slopePerecent from slope limits list.
        public virtual int slopeIndex(double slopePercent) {
            var n = this.slopeLimits.Count;
            foreach (var index in Enumerable.Range(0, n)) {
                if (slopePercent < this.slopeLimits[index]) {
                    return index;
                }
            }
            return n;
        }

        // Return the slope range for an index.
        public virtual string slopeRange(int slopeIndex) {
            Debug.Assert(0 <= slopeIndex && slopeIndex <= this.slopeLimits.Count, string.Format("Slope index {0} out of range", slopeIndex));
            var minimum = slopeIndex == 0 ? 0 : this.slopeLimits[slopeIndex - 1];
            var maximum = slopeIndex == this.slopeLimits.Count ? 9999 : this.slopeLimits[slopeIndex];
            return string.Format("{0}-{1}", minimum, maximum);
        }

        public static string _BASINSDATA = "BASINSDATA";

        public static string _BASINSDATATABLE = @"
    (basin INTEGER PRIMARY KEY UNIQUE NOT NULL, 
    farDistance REAL, 
    minElevation REAL, 
    maxElevation REAL,
    waterId INTEGER)
    ";

        public static string _BASINSDATAINSERTSQL = "INSERT INTO BASINSDATA VALUES(@p1,@p2,@p3,@p4,@p5)";

        public static string _LSUSDATA = "LSUSDATA";

        public static string _LSUSDATATABLE = @"
    (lsu INTEGER PRIMARY KEY UNIQUE NOT NULL, 
    basin INTEGER, 
    category INTEGER,
    channel INTEGER,
    cellCount INTEGER, 
    area REAL, 
    outletElevation REAL, 
    sourceElevation REAL,
    channelLength REAL,  
    channelOrder INTEGER,
    farElevation REAL, 
    farDistance REAL, 
    farPointX REAL, 
    farPointY REAL, 
    midPointX REAL, 
    midPointY REAL,  
    totalElevation REAL, 
    totalSlope REAL, 
    totalLatitude REAL, 
    totalLongitude REAL, 
    cropSoilSlopeArea REAL,
    lastHru INTEGER)
    ";

        public static string _LSUSDATAINSERTSQL = "INSERT INTO LSUSDATA VALUES(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15,@p16,@p17,@p18,@p19,@p20,@p21,@p22)";

        public static string _HRUSDATA = "HRUSDATA";

        public static string _HRUSDATATABLE = @"
    (hru INTEGER,  
    lsu INTEGER,  
    crop INTEGER,  
    soil INTEGER,  
    slope INTEGER,  
    cellCount INTEGER,  
    area REAL,  
    totalElevation REAL,  
    totalSlope REAL,  
    totalLatitude REAL,  
    totalLongitude REAL)
    ";

        public static string _HRUSDATAINSERTSQL = "INSERT INTO HRUSDATA VALUES(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11)";

        public static string _WATERDATA = "WATERDATA";

        public static string _WATERDATATABLE = @"
    (lsu INTEGER, 
    cellCount INTEGER,  
    area REAL, 
    originalArea REAL, 
    totalElevation REAL,
    totalLatitude REAL,  
    totalLongitude REAL,
    id INTEGER,
    channelRole INTEGER,
    waterRole INTEGER)
    ";

        public static string _WATERDATAINSERTSQL = "INSERT INTO WATERDATA VALUES(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10)";

        public static string _ELEVATIONBANDSTABLE = @"
    (subbasin INTEGER PRIMARY KEY UNIQUE NOT NULL,  
    elevb1 REAL,  
    elevb2 REAL,  
    elevb3 REAL,  
    elevb4 REAL,  
    elevb5 REAL,  
    elevb6 REAL,  
    elevb7 REAL,  
    elevb8 REAL,  
    elevb9 REAL,  
    elevb10 REAL,  
    elevb_fr1 REAL,  
    elevb_fr2 REAL,  
    elevb_fr3 REAL,  
    elevb_fr4 REAL,  
    elevb_fr5 REAL,  
    elevb_fr6 REAL,  
    elevb_fr7 REAL,  
    elevb_fr8 REAL,  
    elevb_fr9 REAL,  
    elevb_fr10 REAL)
    ";

        public static string _USERSOILTABLE = @"
    (OBJECTID   INTEGER,
    MUID       TEXT,
    SEQN       TEXT,
    SNAM       TEXT    PRIMARY KEY UNIQUE NOT NULL,
    S5ID       TEXT,
    CMPPCT     TEXT,
    NLAYERS    REAL,
    HYDGRP     TEXT,
    SOL_ZMX    REAL,
    ANION_EXCL REAL,
    SOL_CRK    REAL,
    TEXTURE    TEXT,
    SOL_Z1     REAL,
    SOL_BD1    REAL,
    SOL_AWC1   REAL,
    SOL_K1     REAL,
    SOL_CBN1   REAL,
    CLAY1      REAL,
    SILT1      REAL,
    SAND1      REAL,
    ROCK1      REAL,
    SOL_ALB1   REAL,
    USLE_K1    REAL,
    SOL_EC1    REAL,
    SOL_Z2     REAL,
    SOL_BD2    REAL,
    SOL_AWC2   REAL,
    SOL_K2     REAL,
    SOL_CBN2   REAL,
    CLAY2      REAL,
    SILT2      REAL,
    SAND2      REAL,
    ROCK2      REAL,
    SOL_ALB2   REAL,
    USLE_K2    REAL,
    SOL_EC2    REAL,
    SOL_Z3     REAL,
    SOL_BD3    REAL,
    SOL_AWC3   REAL,
    SOL_K3     REAL,
    SOL_CBN3   REAL,
    CLAY3      REAL,
    SILT3      REAL,
    SAND3      REAL,
    ROCK3      REAL,
    SOL_ALB3   REAL,
    USLE_K3    REAL,
    SOL_EC3    REAL,
    SOL_Z4     REAL,
    SOL_BD4    REAL,
    SOL_AWC4   REAL,
    SOL_K4     REAL,
    SOL_CBN4   REAL,
    CLAY4      REAL,
    SILT4      REAL,
    SAND4      REAL,
    ROCK4      REAL,
    SOL_ALB4   REAL,
    USLE_K4    REAL,
    SOL_EC4    REAL,
    SOL_Z5     REAL,
    SOL_BD5    REAL,
    SOL_AWC5   REAL,
    SOL_K5     REAL,
    SOL_CBN5   REAL,
    CLAY5      REAL,
    SILT5      REAL,
    SAND5      REAL,
    ROCK5      REAL,
    SOL_ALB5   REAL,
    USLE_K5    REAL,
    SOL_EC5    REAL,
    SOL_Z6     REAL,
    SOL_BD6    REAL,
    SOL_AWC6   REAL,
    SOL_K6     REAL,
    SOL_CBN6   REAL,
    CLAY6      REAL,
    SILT6      REAL,
    SAND6      REAL,
    ROCK6      REAL,
    SOL_ALB6   REAL,
    USLE_K6    REAL,
    SOL_EC6    REAL,
    SOL_Z7     REAL,
    SOL_BD7    REAL,
    SOL_AWC7   REAL,
    SOL_K7     REAL,
    SOL_CBN7   REAL,
    CLAY7      REAL,
    SILT7      REAL,
    SAND7      REAL,
    ROCK7      REAL,
    SOL_ALB7   REAL,
    USLE_K7    REAL,
    SOL_EC7    REAL,
    SOL_Z8     REAL,
    SOL_BD8    REAL,
    SOL_AWC8   REAL,
    SOL_K8     REAL,
    SOL_CBN8   REAL,
    CLAY8      REAL,
    SILT8      REAL,
    SAND8      REAL,
    ROCK8      REAL,
    SOL_ALB8   REAL,
    USLE_K8    REAL,
    SOL_EC8    REAL,
    SOL_Z9     REAL,
    SOL_BD9    REAL,
    SOL_AWC9   REAL,
    SOL_K9     REAL,
    SOL_CBN9   REAL,
    CLAY9      REAL,
    SILT9      REAL,
    SAND9      REAL,
    ROCK9      REAL,
    SOL_ALB9   REAL,
    USLE_K9    REAL,
    SOL_EC9    REAL,
    SOL_Z10    REAL,
    SOL_BD10   REAL,
    SOL_AWC10  REAL,
    SOL_K10    REAL,
    SOL_CBN10  REAL,
    CLAY10     REAL,
    SILT10     REAL,
    SAND10     REAL,
    ROCK10     REAL,
    SOL_ALB10  REAL,
    USLE_K10   REAL,
    SOL_EC10   REAL,
    SOL_CAL1   REAL,
    SOL_CAL2   REAL,
    SOL_CAL3   REAL,
    SOL_CAL4   REAL,
    SOL_CAL5   REAL,
    SOL_CAL6   REAL,
    SOL_CAL7   REAL,
    SOL_CAL8   REAL,
    SOL_CAL9   REAL,
    SOL_CAL10  REAL,
    SOL_PH1    REAL,
    SOL_PH2    REAL,
    SOL_PH3    REAL,
    SOL_PH4    REAL,
    SOL_PH5    REAL,
    SOL_PH6    REAL,
    SOL_PH7    REAL,
    SOL_PH8    REAL,
    SOL_PH9    REAL,
    SOL_PH10   REAL)
    ";

        public static string _SOILTABLE = @"
    (id         INTEGER NOT NULL
                    PRIMARY KEY,
    name       TEXT NOT NULL,
    muid       TEXT,
    seqn       INTEGER,
    s5id       TEXT,
    cmppct     INTEGER,
    hydgrp     TEXT NOT NULL,
    zmx        REAL NOT NULL,
    anion_excl REAL NOT NULL,
    crk        REAL NOT NULL,
    texture    TEXT NOT NULL)
    ";

        public static string _SOILLAYERTABLE = @"
    (id        INTEGER NOT NULL
                      PRIMARY KEY,
    soil_id   INTEGER NOT NULL,
    layer_num INTEGER NOT NULL,
    z         REAL    NOT NULL,
    bd        REAL    NOT NULL,
    awc       REAL    NOT NULL,
    k         REAL    NOT NULL,
    cbn       REAL    NOT NULL,
    clay      REAL    NOT NULL,
    silt      REAL    NOT NULL,
    sand      REAL    NOT NULL,
    rock      REAL    NOT NULL,
    alb       REAL    NOT NULL,
    usle_k    REAL    NOT NULL,
    ec        REAL    NOT NULL,
    cal       REAL,
    ph        REAL)
    ";

        public static string _PLANTTABLE = _PLANTS_PLT_TABLE;

        public static string _URBANTABLE = _URBAN_URB_TABLE;

        // Create BASINSDATA, LUSDATA, WATERDATA and HRUSDATA in project database.
        public virtual bool createBasinsDataTables() {
            using (var cmd = new SQLiteCommand(this.conn)) {
                // remove old table completely, for backward compatibility, in case structure changed
                var tableHrus = DBUtils._HRUSDATA;
                var dropSQL = "DROP TABLE IF EXISTS " + tableHrus;
                try {
                    cmd.CommandText = dropSQL;
                    cmd.ExecuteNonQuery();
                }
                catch (Exception) {
                    Utils.exceptionError(string.Format("Could not drop table1 {0} from project database {1}", tableHrus, this.dbFile), this.isBatch);
                    return false;
                }
                var tableLus = DBUtils._LSUSDATA;
                dropSQL = "DROP TABLE IF EXISTS " + tableLus;
                try {
                    cmd.CommandText = dropSQL;
                    cmd.ExecuteNonQuery();
                }
                catch (Exception) {
                    Utils.exceptionError(string.Format("Could not drop table {0} from project database {1}", tableLus, this.dbFile), this.isBatch);
                    return false;
                }
                var tableWater = DBUtils._WATERDATA;
                dropSQL = "DROP TABLE IF EXISTS " + tableWater;
                try {
                    cmd.CommandText = dropSQL;
                    cmd.ExecuteNonQuery();
                }
                catch (Exception) {
                    Utils.exceptionError(string.Format("Could not drop table1 {0} from project database {1}", tableWater, this.dbFile), this.isBatch);
                    return false;
                }
                var tableBasins = DBUtils._BASINSDATA;
                //Utils.loginfo("Dropping BASINSDATA");
                dropSQL = "DROP TABLE IF EXISTS " + tableBasins;
                try {
                    cmd.CommandText = dropSQL;
                    cmd.ExecuteNonQuery();
                }
                catch (Exception) {
                    Utils.exceptionError(string.Format("Could not drop table {0} from project database {1}", tableBasins, this.dbFile), this.isBatch);
                    return false;
                }
                var createSQL = "CREATE TABLE " + tableBasins + DBUtils._BASINSDATATABLE;
                try {
                    cmd.CommandText = createSQL;
                    cmd.ExecuteNonQuery();
                }
                catch (Exception) {
                    Utils.exceptionError(string.Format("Could not create table {0} in project database {1}", tableBasins, this.dbFile), this.isBatch);
                    return false;
                }
                createSQL = "CREATE TABLE " + tableLus + " " + DBUtils._LSUSDATATABLE;
                try {
                    cmd.CommandText = createSQL;
                    cmd.ExecuteNonQuery();
                }
                catch (Exception) {
                    Utils.exceptionError(string.Format("Could not create table {0} in project database {1}", tableLus, this.dbFile), this.isBatch);
                    return false;
                }
                var indexSQL = string.Format("CREATE INDEX IF NOT EXISTS basin_index on {0} (basin)", tableLus);
                try {
                    cmd.CommandText = indexSQL;
                    cmd.ExecuteNonQuery();
                }
                catch (Exception) {
                    Utils.exceptionError(string.Format("Could not create index for basin in table {0} in project database {1}", tableLus, this.dbFile), this.isBatch);
                }
                createSQL = "CREATE TABLE " + tableWater + " " + DBUtils._WATERDATATABLE;
                try {
                    cmd.CommandText = createSQL;
                    cmd.ExecuteNonQuery();
                }
                catch (Exception) {
                    Utils.exceptionError(string.Format("Could not create table {0} in project database {1}", tableWater, this.dbFile), this.isBatch);
                    return false;
                }
                indexSQL = string.Format("CREATE INDEX IF NOT EXISTS lsu_index on {0} (lsu)", tableWater);
                try {
                    cmd.CommandText = indexSQL;
                    cmd.ExecuteNonQuery();
                }
                catch (Exception) {
                    Utils.exceptionError(string.Format("Could not create index for lsu in table {0} in project database {1}", tableWater, this.dbFile), this.isBatch);
                    return false;
                }
                createSQL = "CREATE TABLE " + tableHrus + " " + DBUtils._HRUSDATATABLE;
                try {
                    cmd.CommandText = createSQL;
                    cmd.ExecuteNonQuery();
                }
                catch (Exception) {
                    Utils.exceptionError(string.Format("Could not create table {0} in project database {1}", tableHrus, this.dbFile), this.isBatch);
                    return false;
                }
                indexSQL = string.Format("CREATE INDEX IF NOT EXISTS lsu_index on {0} (lsu)", tableHrus);
                try {
                    cmd.CommandText = indexSQL;
                    cmd.ExecuteNonQuery();
                }
                catch (Exception) {
                    Utils.exceptionError(string.Format("Could not create index for lsu in table {0} in project database {1}", tableHrus, this.dbFile), this.isBatch);
                    return false;
                }
            }
            return true;
        }

        // Write BASINSDATA, LSUSDATA, HRUSDATA and WATERDATA tables in project database.
        public virtual void writeBasinsData(Dictionary<int, BasinData> basins) {
            using (var transaction = this.conn.BeginTransaction()) {
                using (var cmd = new SQLiteCommand("", this.conn, transaction)) {
                    foreach (var (basin, data) in basins) {
                        var OK = this.writeBasinsDataItem(basin, data, cmd);
                        if (!OK) {
                            // error occurred - no point in repeating the failure
                            break;
                        }
                    }
                }
                transaction.Commit();
            }
            //this.hashDbTable(this.conn, this._BASINSDATA);
            //this.hashDbTable(this.conn, this._LSUSDATA);
            //this.hashDbTable(this.conn, this._HRUSDATA);
            //this.hashDbTable(this.conn, this._WATERDATA);
        }

        // Write data for one basin in BASINSDATA, LSUSDATA, HRUSDATA and WATERDATA tables in project database.
        public virtual bool writeBasinsDataItem(int basin, BasinData data, SQLiteCommand cmd) {
            try {
                cmd.CommandText = DBUtils._BASINSDATAINSERTSQL;
                cmd.Parameters.AddWithValue("@p1", basin);
                cmd.Parameters.AddWithValue("@p2", data.farDistance);
                cmd.Parameters.AddWithValue("@p3", data.minElevation);
                cmd.Parameters.AddWithValue("@p4", data.maxElevation);
                cmd.Parameters.AddWithValue("@p5", data.waterId);
                //Utils.loginfo("Writing BASINSDATA item");
                cmd.ExecuteNonQuery();
                //Utils.loginfo("Finished writing BASINSDATA item");
            }
            catch (Exception) {
                Utils.exceptionError(string.Format("Could not write to table {0} in project database {1}", DBUtils._BASINSDATA, this.dbFile), this.isBatch);
                return false;
            }
            foreach (var (channel, channeldata) in data.lsus) {
                foreach (var (landscape, lsuData) in channeldata) {
                    var lsuId = Utils.landscapeUnitId(channel, landscape);
                    try {
                        cmd.CommandText = DBUtils._LSUSDATAINSERTSQL;
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@p1", lsuId);
                        cmd.Parameters.AddWithValue("@p2", basin);
                        cmd.Parameters.AddWithValue("@p3", landscape);
                        cmd.Parameters.AddWithValue("@p4", channel);
                        cmd.Parameters.AddWithValue("@p5", lsuData.cellCount);
                        cmd.Parameters.AddWithValue("@p6", lsuData.area);
                        cmd.Parameters.AddWithValue("@p7", lsuData.outletElevation);
                        cmd.Parameters.AddWithValue("@p8", lsuData.sourceElevation);
                        cmd.Parameters.AddWithValue("@p9", lsuData.channelLength);
                        cmd.Parameters.AddWithValue("@p10", lsuData.channelOrder);
                        cmd.Parameters.AddWithValue("@p11", lsuData.farElevation);
                        cmd.Parameters.AddWithValue("@p12", lsuData.farDistance);
                        cmd.Parameters.AddWithValue("@p13", lsuData.farPointX);
                        cmd.Parameters.AddWithValue("@p14", lsuData.farPointY);
                        cmd.Parameters.AddWithValue("@p15", lsuData.midPointX);
                        cmd.Parameters.AddWithValue("@p16", lsuData.midPointY);
                        cmd.Parameters.AddWithValue("@p17", lsuData.totalElevation);
                        cmd.Parameters.AddWithValue("@p18", lsuData.totalSlope);
                        cmd.Parameters.AddWithValue("@p19", lsuData.totalLatitude);
                        cmd.Parameters.AddWithValue("@p20", lsuData.totalLongitude);
                        cmd.Parameters.AddWithValue("@p21", lsuData.cropSoilSlopeArea);
                        cmd.Parameters.AddWithValue("@p22", lsuData.lastHru);
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception) {
                        Utils.exceptionError(string.Format("Could not write to table {0} in project database {1}", DBUtils._LSUSDATA, this.dbFile), this.isBatch);
                        return false;
                    }
                    var waterBody = lsuData.waterBody;
                    // note that empty reservoirs created by addReservoirs will not be included in the WATERDATA table
                    // because they have been added to the mergedLsus, and DATA tables are written from (unmerged) lsus.
                    // But after regenerating from the DATA tables the reservoirs will be recreated, so there is no harm.
                    if (waterBody is not null) {
                        try {
                            cmd.CommandText = DBUtils._WATERDATAINSERTSQL;
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@p1", lsuId);
                            cmd.Parameters.AddWithValue("@p2", waterBody.cellCount);
                            cmd.Parameters.AddWithValue("@p3", waterBody.area);
                            cmd.Parameters.AddWithValue("@p4", waterBody.originalArea);
                            cmd.Parameters.AddWithValue("@p5", waterBody.totalElevation);
                            cmd.Parameters.AddWithValue("@p6", waterBody.totalLatitude);
                            cmd.Parameters.AddWithValue("@p7", waterBody.totalLongitude);
                            cmd.Parameters.AddWithValue("@p8", waterBody.id);
                            cmd.Parameters.AddWithValue("@p9", waterBody.channelRole);
                            cmd.Parameters.AddWithValue("@p10", waterBody.waterRole);
                            cmd.ExecuteNonQuery();
                        }
                        catch (Exception) {
                            Utils.exceptionError(string.Format("Could not write to table {0} in project database {1}", DBUtils._WATERDATA, this.dbFile), this.isBatch);
                            return false;
                        }
                    }
                    foreach (var (crop, soilSlopeNumbers) in lsuData.cropSoilSlopeNumbers) {
                        foreach (var (soil, slopeNumbers) in soilSlopeNumbers) {
                            foreach (var (slope, hru) in slopeNumbers) {
                                var cd = lsuData.hruMap[hru];
                                try {
                                    cmd.CommandText = DBUtils._HRUSDATAINSERTSQL;
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("@p1", hru);
                                    cmd.Parameters.AddWithValue("@p2", lsuId);
                                    cmd.Parameters.AddWithValue("@p3", crop);
                                    cmd.Parameters.AddWithValue("@p4", soil);
                                    cmd.Parameters.AddWithValue("@p5", slope);
                                    cmd.Parameters.AddWithValue("@p6", cd.cellCount);
                                    cmd.Parameters.AddWithValue("@p7", cd.area);
                                    cmd.Parameters.AddWithValue("@p8", cd.totalElevation);
                                    cmd.Parameters.AddWithValue("@p9", cd.totalSlope);
                                    cmd.Parameters.AddWithValue("@p10", cd.totalLatitude);
                                    cmd.Parameters.AddWithValue("@p11", cd.totalLongitude);
                                    cmd.ExecuteNonQuery();
                                }
                                catch (Exception) {
                                    Utils.exceptionError(string.Format("Could not write to table {0} in project database {1}", DBUtils._HRUSDATA, this.dbFile), this.isBatch);
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }

        // Recreate basins data from BASINSDATA, LSUSDATA, WATERDATA and HRUSDATA tables in project database.
        //         
        //         Also recreates original channel basin areas.
        public (Dictionary<int, BasinData>, bool) regenerateBasins(GlobalVars gv, bool ignoreerrors = false) {
            try {
                gv.topo.origChBasinAreas = new Dictionary<int, double>();
                var basins = new Dictionary<int, BasinData>();
                // map lsuid -> (basin, channel, landscape) to enable non-search reads of all tables
                var lsuMap = new Dictionary<int, (int, int, int)>();
                var cmd = new SQLiteCommand(this.conn);
                try {
                    cmd.CommandText = DBUtils.sqlSelect(DBUtils._BASINSDATA, "*", "", "");
                    using (var reader = cmd.ExecuteReader()) {
                        //Utils.loginfo("Reading BASINSDATA");
                        // first read basins to get domain of basins map fixed
                        while (reader.Read()) {
                            var basin = Convert.ToInt32(reader.GetValue(0));
                            var bd = new BasinData(this.waterLanduse, reader.GetDouble(1), reader.GetInt32(4));
                            bd.minElevation = reader.GetDouble(2);
                            bd.maxElevation = reader.GetDouble(3);
                            basins[basin] = bd;
                        }
                    }
                    Utils.loginfo("Finished reading BASINSDATA");
                    cmd.CommandText = DBUtils.sqlSelect(DBUtils._LSUSDATA, "*", "", "");
                    using (var reader = cmd.ExecuteReader()) {
                        // next read lsus to set up lsusData in each basin and to populate lsuMap
                        while (reader.Read()) {
                            var basin = Convert.ToInt32(reader.GetValue(1));
                            var lsuId = Convert.ToInt32(reader.GetValue(0));
                            var landscape = Convert.ToInt32(reader.GetValue(2));
                            var channel = Convert.ToInt32(reader.GetValue(3));
                            var lsuData = new LSUData();
                            lsuData.cellCount = Convert.ToInt32(reader.GetValue(4));
                            lsuData.area = Convert.ToDouble(reader.GetValue(5));
                            lsuData.outletElevation = Convert.ToDouble(reader.GetValue(6));
                            lsuData.sourceElevation = Convert.ToDouble(reader.GetValue(7));
                            lsuData.channelLength = Convert.ToDouble(reader.GetValue(8));
                            lsuData.channelOrder = Convert.ToInt32(reader.GetValue(9));
                            lsuData.farElevation = Convert.ToDouble(reader.GetValue(10));
                            lsuData.farDistance = Convert.ToDouble(reader.GetValue(11));
                            lsuData.farPointX = Convert.ToDouble(reader.GetValue(12));
                            lsuData.farPointY = Convert.ToDouble(reader.GetValue(13));
                            lsuData.midPointX = Convert.ToDouble(reader.GetValue(14));
                            lsuData.midPointY = Convert.ToDouble(reader.GetValue(15));
                            lsuData.totalElevation = Convert.ToDouble(reader.GetValue(16));
                            lsuData.totalSlope = Convert.ToDouble(reader.GetValue(17));
                            lsuData.totalLatitude = Convert.ToDouble(reader.GetValue(18));
                            lsuData.totalLongitude = Convert.ToDouble(reader.GetValue(19));
                            lsuData.cropSoilSlopeArea = Convert.ToDouble(reader.GetValue(20));
                            lsuData.lastHru = Convert.ToInt32(reader.GetValue(21));
                            lsuMap[lsuId] = (basin, channel, landscape);
                            var basinData = basins[basin];
                            Dictionary<int, LSUData> channelData = null;
                            if (!basinData.lsus.TryGetValue(channel, out channelData)) {
                                channelData = new Dictionary<int, LSUData>();
                            }
                            channelData[landscape] = lsuData;
                            basinData.lsus[channel] = channelData;
                            int chBasin = gv.topo.chLinkToChBasin[channel];
                            var temp = 0.0;
                            if (!gv.topo.origChBasinAreas.TryGetValue(chBasin, out temp)) {
                                gv.topo.origChBasinAreas[chBasin] = lsuData.area;
                            } else {
                                gv.topo.origChBasinAreas[chBasin] += lsuData.area;
                            }
                        }
                    }
                    cmd.CommandText = DBUtils.sqlSelect(DBUtils._WATERDATA, "*", "", "");
                    using (var reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {
                            // add water bodies
                            var lsuid = Convert.ToInt32(reader.GetValue(0));
                            var (basin, channel, landscape) = lsuMap[lsuid];
                            var lsuData = basins[basin].lsus[channel][landscape];
                            lsuData.waterBody = new WaterBody(Convert.ToInt32(reader.GetValue(1)), Convert.ToDouble(reader.GetValue(2)), Convert.ToDouble(reader.GetValue(4)), Convert.ToDouble(reader.GetValue(6)), Convert.ToDouble(reader.GetValue(5)));
                            lsuData.waterBody.originalArea = Convert.ToDouble(reader.GetValue(3));
                            lsuData.waterBody.id = Convert.ToInt32(reader.GetValue(7));
                            lsuData.waterBody.channelRole = (ChannelRole)Convert.ToInt32(reader.GetValue(8));
                            lsuData.waterBody.waterRole = (WaterRole)Convert.ToInt32(reader.GetValue(9));
                        }
                    }
                    cmd.CommandText = DBUtils.sqlSelect(DBUtils._HRUSDATA, "*", "", "");
                    using (var reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {
                            // add hrus
                            var hru = Convert.ToInt32(reader.GetValue(0));
                            var lsuid = Convert.ToInt32(reader.GetValue(1));
                            var (basin, channel, landscape) = lsuMap[lsuid];
                            var lsuData = basins[basin].lsus[channel][landscape];
                            var crop = Convert.ToInt32(reader.GetValue(2));
                            var soil = Convert.ToInt32(reader.GetValue(3));
                            var slope = Convert.ToInt32(reader.GetValue(4));
                            if (!lsuData.cropSoilSlopeNumbers.ContainsKey(crop)) {
                                lsuData.cropSoilSlopeNumbers[crop] = new Dictionary<int, Dictionary<int, int>>();
                                this.landuseVals.Add(crop);
                            }
                            if (!lsuData.cropSoilSlopeNumbers[crop].ContainsKey(soil)) {
                                lsuData.cropSoilSlopeNumbers[crop][soil] = new Dictionary<int, int>();
                                this.soilVals.Add(soil);
                            }
                            lsuData.cropSoilSlopeNumbers[crop][soil][slope] = hru;
                            var cellData = new CellData(Convert.ToInt32(reader.GetValue(5)), Convert.ToDouble(reader.GetValue(6)), Convert.ToDouble(reader.GetValue(7)), Convert.ToDouble(reader.GetValue(8)), Convert.ToDouble(reader.GetValue(10)), Convert.ToDouble(reader.GetValue(9)), crop);
                            lsuData.hruMap[hru] = cellData;
                        }
                    }
                } catch (Exception) {
                    if (!ignoreerrors) {
                        Utils.exceptionError(string.Format("Could not read basins data from project database {0}", this.dbFile), this.isBatch);
                    }
                    return (null, false);
                }
                return (basins, true);
            } catch (Exception) {
                if (!ignoreerrors) {
                    Utils.exceptionError("Failed to reconstruct basin data from database", this.isBatch);
                }
                return (null, false);
            }
        }

        // 
        //         Apply multiplier in place of oldMultiplier to all reach slopes 
        //         in gis_channels table (if any) in project database.
        //         
        public virtual void changeReachSlopes(double multiplier, double oldMultiplier, string shapesDir) {
            try {
                var table = "gis_channels";
                var factor = multiplier / oldMultiplier;
                // need to change SLO2 value
                var sql = string.Format("UPDATE {0} SET slo2 = slo2 * {1}", table, factor);
                this.execNonQuery(sql);
                // update rivs1 file
                var rivs1File = Utils.join(shapesDir, Parameters._RIVS1 + ".shp");
                if (!File.Exists(rivs1File)) {
                    Utils.loginfo(string.Format("{0} not found", rivs1File));
                    return;
                }
                var ds = Ogr.Open(rivs1File, 1);
                var rivs1Layer = ds.GetLayerByIndex(0);
                var slo2Idx = rivs1Layer.FindFieldIndex("Slo2", 1);
                if (slo2Idx < 0) {
                    Utils.loginfo(string.Format("Slo2 field in {0} not found", rivs1File));
                    return;
                }
                rivs1Layer.ResetReading();
                OSGeo.OGR.Feature feature = null;
                do {
                    feature = rivs1Layer.GetNextFeature();
                    if (feature != null) {
                        feature.SetField(slo2Idx, feature.GetFieldAsDouble(slo2Idx) * factor);
                        rivs1Layer.SetFeature(feature);
                    }
                }
                while (feature != null);
            } catch (Exception ex) {
                Utils.loginfo(string.Format("Failed to change reach slopes: {0}", ex.Message));
                // table may not exist yet
                return;
            }
        }

        // 
        //         Apply multiplier in place of oldMultiplier to all tributary slopes 
        //         in subbasins and lsus tables (if any) in project database.
        //         
        public virtual void changeTributarySlopes(double multiplier, double oldMultiplier, string shapesDir) {
            try {
                var table = "gis_lsus";
                var factor = multiplier / oldMultiplier;
                // need to change CSL value
                var sqlBase = "UPDATE {0} SET csl = csl * {1}";
                var sql = string.Format(sqlBase, table, factor);
                this.execNonQuery(sql);
                // update lsus2 file
                var lsus2File = Utils.join(shapesDir, Parameters._LSUS2 + ".shp");
                if (!File.Exists(lsus2File)) {
                    Utils.loginfo(string.Format("{0} not found", lsus2File));
                    return;
                }
                var ds = Ogr.Open(lsus2File, 1);
                var lsus2Layer = ds.GetLayerByIndex(0);
                var cslIdx = lsus2Layer.FindFieldIndex("Csl", 1);
                if (cslIdx < 0) {
                    Utils.loginfo(string.Format("Csl field in {0} not found", lsus2File));
                    return;
                }
                lsus2Layer.ResetReading();
                OSGeo.OGR.Feature feature = null;
                do {
                    feature = lsus2Layer.GetNextFeature();
                    if (feature != null) {
                        feature.SetField(cslIdx, feature.GetFieldAsDouble(cslIdx) * factor);
                        lsus2Layer.SetFeature(feature);
                    }
                }
                while (feature != null);
            } catch (Exception ex) {
                Utils.loginfo(string.Format("Failed to change tributary slopes: {0}", ex.Message));
                // table may not exist yet
                return;
            }
        }

        // 
        //         Apply multiplier in place of oldMultiplier to all mean slopes 
        //         in gis_subbasins, gis_lsus and gis_hrus tables (if any) in project database.
        //         
        public virtual void changeMeanSlopes(double multiplier, double oldMultiplier, string shapesDir) {
            try {

                var table1 = "gis_subbasins";
                var table2 = "gis_lsus";
                var table3 = "gis_hrus";
                var factor = multiplier / oldMultiplier;
                // need to change slope value
                var sqlBase = "UPDATE {0} SET slope = slope * {1}";
                var sql = string.Format(sqlBase, table2, factor);
                this.execNonQuery(sql);
                sql = string.Format(sqlBase, table3, factor);
                this.execNonQuery(sql);
                // for subbasins table we need to update slo1 field as above, but also recalculate
                // the sll field
                sql = DBUtils.sqlSelect(table1, "id, slo1", "", "");
                var update = string.Format("UPDATE {0} SET sll=@p1, slo1=@p2 WHERE id=@p3", table1);
                var cmd = new SQLiteCommand(sql, this.conn);
                using (var reader = cmd.ExecuteReader()) {
                    var idIdx = reader.GetOrdinal("id");
                    var slo1Idx1 = reader.GetOrdinal("slo1");
                    while (reader.Read()) {
                        var meanSlopePercent = reader.GetDouble(slo1Idx1) * factor;
                        var sll = Utils.getSlsubbsn(meanSlopePercent / 100);
                        var ident = reader.GetInt32(idIdx);
                        this.execNonQuery(update, new object[] { sll, meanSlopePercent, ident });
                    }
                }
                // update Slope in lsus2 file
                var lsus2File = Utils.join(shapesDir, Parameters._LSUS2 + ".shp");
                if (!File.Exists(lsus2File)) {
                    Utils.loginfo(string.Format("{0} not found", lsus2File));
                    return;
                }
                var ds = Ogr.Open(lsus2File, 1);
                var lsus2Layer = ds.GetLayerByIndex(0);
                var slopeIdx = lsus2Layer.FindFieldIndex("Slope", 1);
                if (slopeIdx < 0) {
                    Utils.loginfo(string.Format("Slope field in {0} not found", lsus2File));
                    return;
                }
                lsus2Layer.ResetReading();
                OSGeo.OGR.Feature feature = null;
                do {
                    feature = lsus2Layer.GetNextFeature();
                    if (feature != null) {
                        feature.SetField(slopeIdx, feature.GetFieldAsDouble(slopeIdx) * factor);
                        lsus2Layer.SetFeature(feature);
                    }
                } while (feature != null);
                // update Sll and Slo1 in in subs1 file
                var subs1File = Utils.join(shapesDir, Parameters._SUBS1 + ".shp");
                if (!File.Exists(subs1File)) {
                    Utils.loginfo(string.Format("{0} not found", subs1File));
                    return;
                }
                ds = Ogr.Open(subs1File, 1);
                var subs1Layer = ds.GetLayerByIndex(0);
                var sllIdx = subs1Layer.FindFieldIndex("Sll", 1);
                if (sllIdx < 0) {
                    Utils.loginfo(string.Format("Sll field in {0} not found", subs1File));
                    return;
                }
                var slo1Idx = subs1Layer.FindFieldIndex("Slo1", 1);
                if (slo1Idx < 0) {
                    Utils.loginfo(string.Format("Slo1 field in {0} not found", subs1File));
                    return;
                }
                subs1Layer.ResetReading();
                do {
                    feature = subs1Layer.GetNextFeature();
                    if (feature != null) {
                        var meanSlopePercent = feature.GetFieldAsDouble(slo1Idx) * factor;
                        feature.SetField(sllIdx, Utils.getSlsubbsn(meanSlopePercent / 100));
                        feature.SetField(slo1Idx, meanSlopePercent);
                        subs1Layer.SetFeature(feature);
                    }
                } while (feature != null);
            } catch (Exception ex) {
                Utils.loginfo(string.Format("Failed to change mean slopes: {0}", ex.Message));
                // table may not exist yet
                return;
            }
        }

        // 
        //         Apply multiplier in place of oldMultiplier to all main channel lengths
        //         in gis_channels table (if any) in project database, and consequently also change
        //         main channel slope.
        //         
        public virtual void changeMainLengths(double multiplier, double oldMultiplier, string shapesDir) {
            try {
                var table = "gis_channels";
                var factor = multiplier / oldMultiplier;
                // need to change len2 and slo2 values
                var sql = string.Format("UPDATE {0} SET len2 = len2 * {1}, slo2 = slo2 / {1}", table, factor.ToString());
                this.execNonQuery(sql);
                // update rivs1 file
                var rivs1File = Utils.join(shapesDir, Parameters._RIVS1 + ".shp");
                if (!File.Exists(rivs1File)) {
                    Utils.loginfo(string.Format("{0} not found", rivs1File));
                    return;
                }
                var ds = Ogr.Open(rivs1File, 1);
                var rivs1Layer = ds.GetLayerByIndex(0);
                var len2Idx = rivs1Layer.FindFieldIndex("Len2", 1);
                if (len2Idx < 0) {
                    Utils.loginfo(string.Format("Len2 field in {0} not found", rivs1File));
                    return;
                }
                var slo2Idx = rivs1Layer.FindFieldIndex("Slo2", 1);
                if (slo2Idx < 0) {
                    Utils.loginfo(string.Format("Slo2 field in {0} not found", rivs1File));
                    return;
                }
                rivs1Layer.ResetReading();
                OSGeo.OGR.Feature feature = null;
                do {
                    feature = rivs1Layer.GetNextFeature();
                    if (feature != null) {
                        feature.SetField(len2Idx, feature.GetFieldAsDouble(len2Idx) * factor);
                        feature.SetField(slo2Idx, feature.GetFieldAsDouble(slo2Idx) / factor);
                        rivs1Layer.SetFeature(feature);
                    }
                }
                while (feature != null);
            } catch (Exception ex) {
                Utils.loginfo(string.Format("Failed to change main lengths: {0}", ex.Message));
                // table may not exist yet
                return;
            }
        }

        // 
        //         Apply multiplier in place of oldMultiplier to all tributary lengths 
        //         (longest flow path) in subbasins tables (if any) in project database, 
        //         and consequently change tributary slope in gis_lsus tables
        //         
        public virtual void changeTributaryLengths(double multiplier, double oldMultiplier, string shapesDir) {
            try {

                var table1 = "gis_subbasins";
                var factor = multiplier / oldMultiplier;
                var sql1 = string.Format("UPDATE {0} SET len1 = len1 * {1}", table1, factor.ToString());
                this.execNonQuery(sql1);
                var table2 = "gis_lsus";
                // slope is csl
                // slopes are affected inversely
                var sql2 = string.Format("UPDATE {0} SET len1 = len1 * {1}, csl = csl / {1}", table2, factor.ToString());
                this.execNonQuery(sql2);
                // update Len1 in in subs1 file
                var subs1File = Utils.join(shapesDir, Parameters._SUBS1 + ".shp");
                if (!File.Exists(subs1File)) {
                    Utils.loginfo(string.Format("{0} not found", subs1File));
                    return;
                }
                var ds = Ogr.Open(subs1File, 1);
                var subs1Layer = ds.GetLayerByIndex(0);
                var len1Idx = subs1Layer.FindFieldIndex("Len1", 1);
                if (len1Idx < 0) {
                    Utils.loginfo(string.Format("Len1 field in {0} not found", subs1File));
                    return;
                }
                subs1Layer.ResetReading();
                OSGeo.OGR.Feature feature = null;
                do {
                    feature = subs1Layer.GetNextFeature();
                    if (feature != null) {
                        feature.SetField(len1Idx, feature.GetFieldAsDouble(len1Idx) * factor);
                        subs1Layer.SetFeature(feature);
                    }
                } while (feature != null);
                // update lsus2 file
                var lsus2File = Utils.join(shapesDir, Parameters._LSUS2 + ".shp");
                if (!File.Exists(lsus2File)) {
                    Utils.loginfo(string.Format("{0} not found", lsus2File));
                    return;
                }
                ds = Ogr.Open(lsus2File, 1);
                var lsus2Layer = ds.GetLayerByIndex(0);
                len1Idx = lsus2Layer.FindFieldIndex("Len1", 1);
                if (len1Idx < 0) {
                    Utils.loginfo(string.Format("Len1 field in {0} not found", lsus2File));
                    return;
                }
                var cslIdx = lsus2Layer.FindFieldIndex("Csl1", 1);
                if (cslIdx < 0) {
                    Utils.loginfo(string.Format("Csl field in {0} not found", lsus2File));
                    return;
                }
                lsus2Layer.ResetReading();
                do {
                    feature = lsus2Layer.GetNextFeature();
                    if (feature != null) {
                        feature.SetField(len1Idx, feature.GetFieldAsDouble(len1Idx) * factor);
                        feature.SetField(cslIdx, feature.GetFieldAsDouble(cslIdx) / factor);
                        lsus2Layer.SetFeature(feature);
                    }
                } while (feature != null);
            } catch (Exception ex) {
                Utils.loginfo(string.Format("Failed to change tributary slopes: {0}", ex.Message));
                // table may not exist yet
                return;
            }
        }

        // Change main and tributary channel widths and depths in gis_channels and gis_lsus tables.
        public virtual void changeChannelWidthAndDepth(
            double widthMult,
            double widthExp,
            double depthMult,
            double depthExp,
            string shapesDir) {
            try {
                // do gis_channels first as generated in delineation, before subbasins and lsus
                var table = "gis_channels";
                var sql = DBUtils.sqlSelect(table, "id, areac, wid2, dep2", "", "");
                var adapter = new SQLiteDataAdapter(sql, this.conn);
                var update = string.Format("UPDATE {0} SET wid2=@p1, dep2=@p2 WHERE id=@p3", table);
                adapter.UpdateCommand = new SQLiteCommand(update, this.conn);
                adapter.UpdateCommand.Parameters.Add("@p1", DbType.Double, 15, "wid2");
                adapter.UpdateCommand.Parameters.Add("@p2", DbType.Double, 15, "dep2");
                var parameter = adapter.UpdateCommand.Parameters.Add("@p3", DbType.Int32);
                parameter.SourceColumn = "id";
                parameter.SourceVersion = DataRowVersion.Original;
                DataTable channelData = new DataTable();
                adapter.Fill(channelData);
                foreach (DataRow row in channelData.Rows) {
                    var drainAreaKm = Convert.ToDouble(row["areac"]) / 100;
                    var channelWidth = widthMult * Math.Pow(drainAreaKm, widthExp);
                    var channelDepth = depthMult * Math.Pow(drainAreaKm, depthExp);
                    row["wid2"] = channelWidth;
                    row["dep2"] = channelDepth;
                }
                adapter.Update(channelData);
                // update rivs1 file
                var rivs1File = Utils.join(shapesDir, Parameters._RIVS1 + ".shp");
                if (!File.Exists(rivs1File)) {
                    Utils.loginfo(string.Format("{0} not found", rivs1File));
                    return;
                }
                var ds = Ogr.Open(rivs1File, 1);
                var rivs1Layer = ds.GetLayerByIndex(0);
                var areaIdx = rivs1Layer.FindFieldIndex("AreaC", 1);
                if (areaIdx < 0) {
                    Utils.loginfo(string.Format("AreaC field in {0} not found", rivs1File));
                    return;
                }
                var wid2Idx = rivs1Layer.FindFieldIndex("Wid2", 1);
                if (wid2Idx < 0) {
                    Utils.loginfo(string.Format("Wid2 field in {0} not found", rivs1File));
                    return;
                }
                var dep2Idx = rivs1Layer.FindFieldIndex("Dep2", 1);
                if (dep2Idx < 0) {
                    Utils.loginfo(string.Format("Dep2 field in {0} not found", rivs1File));
                    return;
                }
                rivs1Layer.ResetReading();
                OSGeo.OGR.Feature feature = null;
                do {
                    feature = rivs1Layer.GetNextFeature();
                    if (feature != null) {
                        var drainAreaKm = feature.GetFieldAsDouble(areaIdx) / 100;
                        feature.SetField(wid2Idx, widthMult * Math.Pow(drainAreaKm, widthExp));
                        feature.SetField(dep2Idx, depthMult * Math.Pow(drainAreaKm, depthExp));
                        rivs1Layer.SetFeature(feature);
                    }
                } while (feature != null);
            }
            catch (Exception ex) {
                Utils.loginfo(string.Format("Failed to change main channel widths and depths: {0}", ex.Message));
                // gis_channels tables may not exist
            }
            try {
                var table = "gis_lsus";
                var sql = DBUtils.sqlSelect(table, "id, area, wid1, dep1", "", "");
                var adapter = new SQLiteDataAdapter(sql, this.conn);
                var update = string.Format("UPDATE {0} SET wid1=@p1, dep1=@p2 WHERE id=@p3", table);
                adapter.UpdateCommand = new SQLiteCommand(update, this.conn);
                adapter.UpdateCommand.Parameters.Add("@p1", DbType.Double, 15, "wid1");
                adapter.UpdateCommand.Parameters.Add("@p2", DbType.Double, 15, "dep1");
                var parameter = adapter.UpdateCommand.Parameters.Add("@p3", DbType.Int32);
                parameter.SourceColumn = "id";
                parameter.SourceVersion = DataRowVersion.Original;
                DataTable lsusData = new DataTable();
                adapter.Fill(lsusData);
                foreach (DataRow row in lsusData.Rows) {
                    var drainAreaKm = Convert.ToDouble(row["area"]) / 100;
                    var channelWidth = widthMult * Math.Pow(drainAreaKm, widthExp);
                    var channelDepth = depthMult * Math.Pow(drainAreaKm, depthExp);
                    row["wid1"] = channelWidth;
                    row["dep1"] = channelDepth;
                }
                adapter.Update(lsusData);
                // update lsus2 file
                var lsus2File = Utils.join(shapesDir, Parameters._LSUS2 + ".shp");
                if (!File.Exists(lsus2File)) {
                    Utils.loginfo(string.Format("{0} not found", lsus2File));
                    return;
                }
                var ds = Ogr.Open(lsus2File, 1);
                var lsus2Layer = ds.GetLayerByIndex(0);
                var areaIdx = lsus2Layer.FindFieldIndex(Parameters._AREA, 1);
                if (areaIdx < 0) {
                    Utils.loginfo(string.Format("{0} field in {1} not found", Parameters._AREA, lsus2File));
                    return;
                }
                var wid1Idx = lsus2Layer.FindFieldIndex("Wid1", 1);
                if (wid1Idx < 0) {
                    Utils.loginfo(string.Format("Wid1 field in {0} not found", lsus2File));
                    return;
                }
                var dep1Idx = lsus2Layer.FindFieldIndex("Dep1", 1);
                if (dep1Idx < 0) {
                    Utils.loginfo(string.Format("Dep1 field in {0} not found", lsus2File));
                    return;
                }
                lsus2Layer.ResetReading();
                OSGeo.OGR.Feature feature = null;
                do {
                    feature = lsus2Layer.GetNextFeature();
                    if (feature != null) {
                        var drainAreaKm = feature.GetFieldAsDouble(areaIdx) / 100;
                        feature.SetField(wid1Idx, widthMult * Math.Pow(drainAreaKm, widthExp));
                        feature.SetField(dep1Idx, depthMult * Math.Pow(drainAreaKm, depthExp));
                        lsus2Layer.SetFeature(feature);
                    }
                }
                while (feature != null);
            } catch (Exception ex) {
                Utils.loginfo(string.Format("Failed to change tributary channel widths and depths: {0}", ex.Message));
                // table may not exist
                return;
            }
        }

        // Change upslope HRU drainage to percent into channel or water body, rest to downslope LSU.
        public virtual void changeUpslopeHRUDrain(double percent) {
            try {
                var table1 = "gis_hrus";
                var table2 = "gis_routing";
                var sql1 = DBUtils.sqlSelect(table1, "id, lsu", "", "");
                var sql2 = DBUtils.sqlSelect(table2, "rowid, sinkcat, percent", "", "sourceid=@p1 AND sourcecat=@p2");
                var update = string.Format("UPDATE {0} SET percent=@p1 WHERE rowid=@p2", table2);
                // first find upslope HRUs
                var cmd1 = new SQLiteCommand(sql1, this.conn);
                using (var reader1 = cmd1.ExecuteReader()) {
                    while (reader1.Read()) {
                        var lsuId = reader1.GetInt32(1);
                        if (Utils.landscapeUnitIdIsUpslope(lsuId)) {
                            var hru = reader1.GetInt32(0);
                            var cmd2 = new SQLiteCommand(sql2, this.conn);
                            cmd2.Parameters.AddWithValue("@p1", hru);
                            cmd2.Parameters.AddWithValue("@p2", "HRU");
                            using (var reader2 = cmd2.ExecuteReader()) {
                                while (reader2.Read()) {
                                    var sinkCat = reader2.GetString(1);
                                    var rowId = reader2.GetInt32(0);
                                    var current_percent = reader2.GetInt32(2);
                                    if (current_percent == 100) {
                                        continue; // lower LSU is all water; nothing to change
                                    }
                                    // set drain to channel or water body to percent
                                    if (sinkCat == "CH" || sinkCat == "RES" || sinkCat == "PND" || sinkCat == "WETL") {
                                        this.execNonQuery(update, new object[] { percent, rowId });
                                        // set drain to LSU to 100 - percent
                                    } else if (sinkCat == "LSU") {
                                        this.execNonQuery(update, new object[] { 100 - percent, rowId });
                                    }
                                }
                            }
                        }
                    }
                }
            } catch (Exception) {
                // table may not exist
                return;
            }
        }

        // Write gis_elevationbands table.
        public virtual void writeElevationBands(Dictionary<int, List<(double, double, double)>> basinElevBands, int numElevBands) {
            using (var transaction = this.conn.BeginTransaction()) {
                using (var cmd = new SQLiteCommand("", this.conn, transaction)) {
                    var table = "gis_elevationbands";
                    cmd.CommandText = "DROP TABLE IF EXISTS " + table;
                    this.execCmdQuery(cmd);
                    cmd.CommandText = "CREATE TABLE " + table + " " + DBUtils._ELEVATIONBANDSTABLE;
                    this.execCmdQuery(cmd);
                    foreach (var (SWATBasin, bands) in basinElevBands) {
                        string row = null;
                        if (bands is not null) {
                            // need mid-points of bands, but list has start values
                            double el1 = 0;
                            double el2 = 0;
                            double mid = 0;
                            double semiWidth = 0;
                            if (bands.Count == 1) {
                                el1 = bands[0].Item1;
                                mid = bands[0].Item2;
                                semiWidth = mid - el1;
                            } else {
                                el1 = bands[0].Item1;
                                el2 = bands[1].Item1;
                                semiWidth = (el2 - el1) / 2.0;
                            }
                            // with very small range of elevations in a subbasin can have fewer bands than numElevBands:
                            // len(bands) is at most number of enteger elevations in subbasin
                            // Conversely, with rounding errors can have more bands than numElevBands
                            var indexBound = Math.Min(numElevBands, bands.Count);
                            row = string.Format("({0},", SWATBasin);
                            foreach (var i in Enumerable.Range(0, 10)) {
                                var el = 0.0;
                                if (i < indexBound) {
                                    el = bands[i].Item1 + semiWidth;
                                }
                                row += string.Format("{:.2F},", el);
                            }
                            foreach (var i in Enumerable.Range(0, 10)) {
                                var frac = 0.0;
                                if (i < indexBound) {
                                    frac = bands[i].Item2;
                                }
                                row += string.Format("{:.4F}", frac / 100.0);
                                var sep = i < 9 ? "," : ")";
                                row += sep;
                            }
                        } else {
                            row = string.Format("({0},0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0)", SWATBasin);
                        }
                        cmd.CommandText = "INSERT INTO " + table + " VALUES " + row;
                        try {
                            this.execCmdQuery(cmd);
                        }
                        catch (Exception) {
                            Utils.exceptionError(string.Format("Could not write to table {0} in project database {1}", table, this.dbFile), this.isBatch);
                            return;
                        }
                    }
                    transaction.Commit();
                }
                //this.hashDbTable(this.conn, table);
            }
        }

        public static string _LANDUSELOOKUPTABLE2 = "(LANDUSE_ID INTEGER, SWAT_CODE TEXT)";

        public static string _LANDUSELOOKUPTABLE3 = "(LANDUSE_ID INTEGER, SWAT_CODE TEXT, DESCRIPTION TEXT)";

        public static string _SOILLOOKUPTABLE = "(SOIL_ID INTEGER, NAME TEXT)";

        // Read landuse csv file.
        public virtual string readLanduseCsv() {
            return this.readCsv("landuse", this.landuseTableNames);
        }

        // Read soil csv file.
        public virtual string readSoilCsv() {
            return this.readCsv("soil", this.soilTableNames);
        }

        // Read plant csv file.
        public virtual string readPlantCsv() {
            return this.readCsv("plant", this.plantTableNames);
        }

        // Read urban csv file.
        public virtual string readUrbanCsv() {
            return this.readCsv("urban", this.urbanTableNames);
        }

        // Read usersoil csv file.
        public virtual string readUsersoilCsv() {
            return this.readCsv("usersoil", this.usersoilTableNames);
        }

        // Invite reader to choose csv file and read it.
        public virtual string readCsv(string typ, List<string> names) {
            string caption;
            if (typ == "plant" || typ == "urban" || typ == "usersoil" || typ == "usersoil_layer") {
                caption = string.Format("Choose {0} csv file", typ);
            } else {
                caption = string.Format("Choose {0} lookup csv file", typ);
            }
            caption = Utils.trans(caption);
            var filtr = FileTypes.filter(FileTypes._CSV);
            string csvFile = null;
            using (var dlg = new System.Windows.Forms.OpenFileDialog() {
                Title = caption,
                Filter = filtr
            }) {
                if (dlg.ShowDialog() == DialogResult.OK) {
                    csvFile = dlg.FileName;
                }
            }
            if (csvFile is not null && File.Exists(csvFile)) {
                return this.readCsvFile(csvFile, typ, names);
            } else {
                return "";
            }
        }

        // Read csv file into table.
        //         
        //         The database used is the landuse and soil database if typ is usersoil, plant, or urban
        //         else the project database.
        //         The table name is the csv file name (without extension) if that contains typ as a substring, 
        //         else is typ if that is usersoil, plant, or urban
        //         else is typ_lookup.
        //         The table name is then extended with 0, 1, etc until a new name is found
        //         that is not in names.
        //         
        public virtual string readCsvFile(string csvFile, string typ, List<string> names) {
            if (typ == "usersoil_layer") {
                // use singlaton usersoil table name in names as basis for layer name, 
                // so they necessarily match
                var table = names[0] + "_layer";
                return this.importCsv(table, typ, csvFile);
            } else {
                var table = Path.ChangeExtension(Path.GetFileName(csvFile), null);
                if (!table.Contains(typ)) {
                    if (typ == "usersoil" || typ == "plant" || typ == "urban") {
                        table = typ;
                    } else {
                        table = string.Format("{0}_lookup", typ);
                    }
                }
                var @base = table;
                var i = 0;
                while (names.Contains(table)) {
                    table = @base + i.ToString();
                    i = i + 1;
                }
                return this.importCsv(table, typ, csvFile);
            }
        }

        // 
        //         Write table of typ either soil or landuse in project database, 
        //         or usersoil in landuse and soil database, using csv file fil.
        //         
        public virtual string importCsv(string table, string typ, string fil) {
            string design;
            string values = "";
            SQLiteConnection localConn = null;
            bool isRefDb = false;
            bool isProjDb = true;
            string db;
            var needsSeparateLayerTable = false;
            var isLanduse = false;

            string makeValues(int count) {
                string result = " VALUES(";
                for (int i = 1; i <= count; i++) {
                    string terminator = i < count ? "," : ")";
                    result += (string.Format("@p{0}", i.ToString()) + terminator);
                }
                return result;
            }

            try {
                if (typ == "usersoil" || typ == "usersoil_layer" || typ == "plant" || typ == "urban") {
                    db = this.plantSoilDatabase;
                    isProjDb = File.Equals(this.plantSoilDatabase, this.dbFile);
                    isRefDb = File.Equals(this.plantSoilDatabase, this.dbRefFile);
                    if (isProjDb) {
                        localConn = this.conn;
                    } else if (isRefDb) {
                        localConn = this.connRef;
                    } else {
                        localConn = new SQLiteConnection("DataSource=" + this.plantSoilDatabase);
                    }
                    if (typ == "usersoil") {
                        // we should have have either 152 columns
                        // or 11 plus a layers file
                        // read first line to find which
                        using (var reader = new StreamReader(fil)) {
                            var line = reader.ReadLine();
                            var cols = line.Split(',');
                            needsSeparateLayerTable = cols.Length < 20;
                        }
                        if (needsSeparateLayerTable) {
                            var layerTable = this.readCsv("usersoil_layer", new List<string> {
                            table
                        });
                            Utils.loginfo(string.Format("Making separate usersoil tables {0} and {1}", table, layerTable));
                            values = makeValues(11); // " VALUES(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11)";
                        } else {
                            values = makeValues(152);
                        }
                    } else if (typ == "usersoil_layer") {
                        values = makeValues(17);
                    } else if (typ == "plant") {
                        values = makeValues(55);
                    } else if (typ == "urban") {
                        values = makeValues(13);
                    }
                } else {
                    localConn = this.conn;
                    isProjDb = true;
                    isRefDb = false;
                    db = this.dbFile;
                    values = " VALUES(@p1,@p2)";
                    isLanduse = typ == "landuse";
                }
                var sql = "INSERT INTO " + table + values;
                if (localConn is null) {
                    return "";
                }
                var dropSQL = "DROP TABLE IF EXISTS " + table;
                var createSQL = "";
                using (var transaction = localConn.BeginTransaction()) {
                    // should not happen, but safety first
                    using (var cmd = new SQLiteCommand(dropSQL, localConn)) {
                        try {
                            cmd.ExecuteNonQuery();
                        }
                        catch (Exception) {
                            Utils.exceptionError(string.Format("Could not drop table {0} from database {1}", table, db), this.isBatch);
                            return "";
                        }
                    }
                    if (typ == "usersoil") {
                        if (needsSeparateLayerTable) {
                            design = DBUtils._SOILTABLE;
                        } else {
                            design = DBUtils._USERSOILTABLE;
                        }
                    } else if (typ == "usersoil_layer") {
                        design = DBUtils._SOILLAYERTABLE;
                    } else if (typ == "plant") {
                        design = DBUtils._PLANTTABLE;
                    } else if (typ == "urban") {
                        design = DBUtils._URBANTABLE;
                    } else if (isLanduse) {
                        design = DBUtils._LANDUSELOOKUPTABLE2;
                    } else {
                        design = DBUtils._SOILLOOKUPTABLE;
                    }
                    createSQL = "CREATE TABLE " + table + design;
                    using (var cmd = new SQLiteCommand(createSQL, localConn, transaction)) {
                        try {
                            cmd.ExecuteNonQuery();
                        }
                        catch (Exception) {
                            Utils.error(string.Format("Could not create table {0} in database {1}", table, db), this.isBatch);
                            if (!(isProjDb || isRefDb)) {
                                localConn.Close();
                                localConn.Dispose();
                            }
                            return "";
                        }
                    }
                    transaction.Commit();
                }
                using (var transaction = this.conn.BeginTransaction()) {
                    using (var cmd = new SQLiteCommand(sql, this.conn, transaction)) {
                        using (var reader = new StreamReader(fil)) {
                            var headers = reader.ReadLine().Split(',');
                            if (typ == "landuse") {
                                // check if we have the optional third description field
                                if (headers.Length == 3) {
                                    // recreate table with 3 columns
                                    cmd.CommandText = dropSQL;
                                    this.execCmdQuery(cmd);
                                    cmd.CommandText = "CREATE TABLE " + table + DBUtils._LANDUSELOOKUPTABLE3;
                                    this.execCmdQuery(cmd);
                                    cmd.CommandText = "INSERT INTO " + table + " VALUES(@p1,@p2,@p3)";
                                }
                            }
                            string line;
                            while ((line = reader.ReadLine()) != null) {
                                try {
                                    this.execCmdQuery(cmd, line.Split(','));
                                }
                                catch (Exception) {
                                    Utils.exceptionError(string.Format("Could not write to table {0} in database {1} from file {2}", table, db, fil), this.isBatch);
                                    if (!(isProjDb || isRefDb)) {
                                        localConn.Close();
                                        localConn.Dispose();
                                    }
                                    return "";
                                }
                            }
                        }
                    }
                    transaction.Commit();
                }
                if (typ == "usersoil" || typ == "plant" || typ == "urban") {
                    if (typ == "usersoil") {
                        this.usersoilTableNames.Add(table);
                    } else if (typ == "plant") {
                        this.plantTableNames.Add(table);
                    } else {
                        this.urbanTableNames.Add(table);
                    }
                } else if (typ == "usersoil_layer") {
                } else if (isLanduse) {
                    this.landuseTableNames.Add(table);
                } else {
                    this.soilTableNames.Add(table);
                }
                return table;
            }
            finally {
                if (!(isProjDb || isRefDb)) {
                    localConn.Close();
                    localConn.Dispose();
                }
            }
        }

        //using HRUMap = Dictionary<int,Dictionary<int,Dictionary<int,HashSet<int>>>>;
        
        //using WaterMap = Dictionary<int,Dictionary<int,Dictionary<int,int>>>;
        
        // Create HRU map SWATBasin -> SWATChannel -> LSUId -> hruNum-set,
        //         reservoir map SWATBasin -> SWATChannel -> LSUId -> reservoir number,
        //         and pond map SWATBasin -> SWATChannel -> LSUId -> pond number 
        //         from gis_channels, gis_lsus, gis_hrus and gis_water.
        public (Dictionary<int, Dictionary<int, Dictionary<int, HashSet<int>>>>,
        Dictionary<int, Dictionary<int, Dictionary<int, int>>>,
        Dictionary<int, Dictionary<int, Dictionary<int, int>>>) 
            createTopology() {
            var resLSUs = new Dictionary<int, int>();
            var pndLSUs = new Dictionary<int, int>();
            var sql0 = DBUtils.sqlSelect("gis_water", "id, wtype, lsu", "", "");
            using (var reader0 = this.getReader(sql0)) {
                while (reader0.Read()) {
                    var lsuId = reader0.GetInt32(2);
                    if (reader0.GetString(1) == "RES") {
                        resLSUs[lsuId] = reader0.GetInt32(0);
                    } else { 
                        pndLSUs[lsuId] = reader0.GetInt32(0);
                    }
                }
            }
            var lsus = new Dictionary<int, HashSet<int>>();
            var sql1 = DBUtils.sqlSelect("gis_hrus", "id, lsu", "", "");
            using (var reader1 = this.getReader(sql1)) {
                while (reader1.Read()) {
                    var hruNum = reader1.GetInt32(0);
                    var lsuId = reader1.GetInt32(1);
                    HashSet<int> hruNums;
                    if (lsus.TryGetValue(lsuId, out hruNums)) {
                        hruNums.Add(hruNum);
                    } else {
                        hruNums = new HashSet<int> { hruNum };
                    }
                    lsus[lsuId] = hruNums;
                }
            }
            var channels = new Dictionary<int, Dictionary<int, HashSet<int>>>();
            var resChannels = new Dictionary<int, Dictionary<int, int>>();
            var pndChannels = new Dictionary<int, Dictionary<int, int>>();
            var sql2 = DBUtils.sqlSelect("gis_lsus", "id, channel", "", "");
            using (var reader2 = this.getReader(sql1)) {
                while (reader2.Read()) {
                    var lsuId = reader2.GetInt32(0);
                    var SWATChannel = reader2.GetInt32(1);
                    Dictionary<int, HashSet<int>> lsus2;
                    if (!channels.TryGetValue(SWATChannel, out lsus2)) {
                        lsus2 = new Dictionary<int, HashSet<int>>();
                    }
                    HashSet<int> hruNums;
                    if (!lsus.Remove(lsuId, out hruNums)) {
                        hruNums = new HashSet<int>();
                    }
                    lsus2[lsuId] = hruNums;
                    channels[SWATChannel] = lsus2;
                    if (resLSUs.ContainsKey(lsuId)) {
                        Dictionary<int, int> resLSUs2;
                        if (!resChannels.TryGetValue(SWATChannel, out resLSUs2)) {
                            resLSUs2 = new Dictionary<int, int>();
                        }
                        int res = resLSUs[lsuId];
                        resLSUs.Remove(lsuId);
                        resLSUs2[lsuId] = res;
                        resChannels[SWATChannel] = resLSUs2;
                    }
                    if (pndLSUs.ContainsKey(lsuId)) {
                        Dictionary<int, int> pndLSUs2;
                        if (!pndChannels.TryGetValue(SWATChannel, out pndLSUs2)) {
                            pndLSUs2 = new Dictionary<int, int>();
                        }
                        int pnd = pndLSUs[lsuId];
                        pndLSUs2.Remove(lsuId);
                        pndLSUs2[lsuId] = pnd;
                        pndChannels[SWATChannel] = pndLSUs2;
                    }
                }
            }
            var topology = new Dictionary<int, Dictionary<int, Dictionary<int, HashSet<int>>>>();
            var reservoirs = new Dictionary<int, Dictionary<int, Dictionary<int, int>>>();
            var ponds = new Dictionary<int, Dictionary<int, Dictionary<int, int>>>();
            var sql3 = DBUtils.sqlSelect("gis_channels", "id, subbasin", "", "");
            using (var reader3 = this.getReader(sql3)) {
                while (reader3.Read()) {
                    var SWATChannel = reader3.GetInt32(0);
                    var SWATBasin = reader3.GetInt32(1);
                    Dictionary<int, Dictionary<int, HashSet<int>>> channels3;
                    if (!topology.TryGetValue(SWATBasin, out channels3)) {
                        channels3 = new Dictionary<int, Dictionary<int, HashSet<int>>>();
                    }
                    Dictionary<int, HashSet<int>> data;
                    if (!channels.Remove(SWATChannel, out data)) {
                        data = new Dictionary<int, HashSet<int>>();
                    }
                    channels3[SWATChannel] = data;
                    topology[SWATBasin] = channels3;
                    if (resChannels.ContainsKey(SWATChannel)) {
                        Dictionary<int, Dictionary<int, int>> resChannels2;
                        if (!reservoirs.TryGetValue(SWATBasin, out resChannels2)) {
                            resChannels2 = new Dictionary<int, Dictionary<int, int>>();
                        }
                        Dictionary<int, int> resData;
                        resChannels.Remove(SWATChannel, out resData);
                        resChannels2[SWATChannel] = resData;
                        reservoirs[SWATBasin] = resChannels2;
                    }
                    if (pndChannels.ContainsKey(SWATChannel)) {
                        Dictionary<int, Dictionary<int, int>> pndChannels2;
                        if (!ponds.TryGetValue(SWATBasin, out pndChannels2)) {
                            pndChannels2 = new Dictionary<int, Dictionary<int, int>>();
                        }
                        Dictionary<int, int> pndData;
                        pndChannels.Remove(SWATChannel, out pndData);
                        pndChannels2[SWATChannel] = pndData;
                        ponds[SWATBasin] = pndChannels2;
                        pndChannels2[SWATChannel] = pndData;
                        ponds[SWATBasin] = pndChannels2;
                    }
                }
            }
            return (topology, reservoirs, ponds);
        }
        
        ////# Return an md5 hash value for a database table.  Used in testing.
        //public virtual string hashDbTable(object conn, string table) {
        //    // Only calculate and store table hashes when testing, as this is their purpose
        //    if (this.projName.Contains("test")) {
        //        // suspend row_factory setting
        //        // normal setting to sqlite3.Row produces dictionaries for rows, which have random order 
        //        // and hence produce random hashes
        //        // row_factory needs to be a function taking a cursor and row as tuple and returning a row
        //        // so we use an identity
        //        conn.row_factory = (_,row) => row;
        //        var m = hashlib.md5();
        //        var sql = DBUtils.sqlSelect(table, "*", "", "");
        //        foreach (var row in this._gv.db.execNonQuery(sql)) {
        //            m.update(row.@__repr__().encode());
        //        }
        //        var result = m.hexdigest();
        //        Utils.loginfo(string.Format("Hash for table {0}: {1}", table, result));
        //        // restore row_factory
        //        conn.row_factory = sqlite3.Row;
        //        return result;
        //    }
        //    return "";
        //}

        // Log channels involved in loop, in reverse order.
        public static void outputLoop(List<int> channels) {
            var done = new HashSet<int>();
            var maxCount = channels.Count;
            Utils.logerror("Reverse list of channels in loop");
            var index = -1;
            while (maxCount + index >= 0) {
                var channel = channels[index];
                if (done.Contains(channel)) { break; }
                Utils.loginfo(channel.ToString());
                done.Add(channel);
                index--;
            }
        }
        
        // Check every source in gis_routing table drains ultimately to an outlet, i.e. there are no loops
        //         and everything not a final outlet has a sink appearing as a source in gis_routing.  Also check routing for each
        //         source totals 100%.  Return lists of error messages (loops; percents not totalling 100) and 
        //         warning messages (loops but with a zero percent somwehere)
        public static (List<string>, List<string>) checkRouting(SQLiteConnection localConn) {
            var errors = new List<string>();
            var warnings = new List<string>();
            var table = "gis_routing";
            // mapping category -> id-set showing sources that are known to drain to an outlet
            // to save tracking every source all the way
            var done = new Dictionary<string, HashSet<int>>();
            // mapping category -> id-list showing sources currently under investigation
            // (so all can be marked as done if the chain terminates)
            var pending = new Dictionary<string, List<int>>();
            // mapping category -> id -> hyd_typ -> percent used to check sources with multiple sinks have 100% accounted for
            var percentages = new Dictionary<string, Dictionary<int, Dictionary<string, double>>>();
            var nextSql = DBUtils.sqlSelect(table, "*", "", "");
            var findSql = DBUtils.sqlSelect(table, "hyd_typ, sinkid, sinkcat, percent", "", "sourceid=@p1 AND sourcecat=@p2");
            // Might be tempted to use pending to detect loops, but note below we don't
            // add to pending for multiple sinks, so then e.g. A -> A 90% would just loop.
            // So we prefer a count.
            var sql = "SELECT COUNT(*) FROM " + table;
            var cmd = new SQLiteCommand(sql, localConn);
            var maxSteps = Convert.ToInt32(cmd.ExecuteScalar());
            cmd = new SQLiteCommand(nextSql, localConn);
            using (var reader = cmd.ExecuteReader()) {
                while (reader.Read()) {
                    var sid = Convert.ToInt32(reader.GetValue(0)); // "sourceid"
                    var scat = reader.GetString(1); // "sourcecat"
                    var hydTyp = reader.GetString(2); // "hyd_typ"];
                    var tid = Convert.ToInt32(reader.GetValue(3));  // "sinkid"
                    var tcat = reader.GetString(4); // "sinkcat"];
                    var percent = Convert.ToDouble(reader.GetValue(5));  // "percent"]);
                    var hasZero = percent == 0;
                    if (percent < 100) {
                        // add percent to percentages for this source
                        Dictionary<string, double> hydTypPercentages;
                        Dictionary<int, Dictionary<string, double>> sourcePercentages;
                        if (!percentages.TryGetValue(scat, out sourcePercentages)) {
                            hydTypPercentages = new() { [hydTyp] = percent };
                            sourcePercentages = new() { [sid] = hydTypPercentages };
                            percentages[scat] = sourcePercentages;
                        } else {
                            if (!sourcePercentages.TryGetValue(sid, out hydTypPercentages)) {
                                hydTypPercentages = new() { [hydTyp] = percent };
                                sourcePercentages[sid] = hydTypPercentages;
                            } else {
                                hydTypPercentages[hydTyp] += percent;
                            }
                        }
                    }
                    var count = maxSteps;
                    while (count > 0) {
                        if (done.ContainsKey(scat) && done[scat].Contains(sid)) {
                            // move pending to done
                            foreach (var (pcat, pids) in pending) {
                                if (done.ContainsKey(pcat)) {
                                    done[pcat].UnionWith(pids);
                                } else {
                                    done[pcat] = new HashSet<int>(pids);
                                }
                            }
                            pending = new Dictionary<string, List<int>>();
                            break;
                        }
                        if (percent == 100) {
                            // Only include source in pending (which should become done) 
                            // if there is only one sink for it, else other sinks will be ignored.
                            // Pending is only used for efficiency, to avoid tracking every source to a final outlet,
                            // so little harm in omitting some sources from it.
                            if (pending.ContainsKey(scat)) {
                                pending[scat].Add(sid);
                            } else {
                                pending[scat] = new List<int> { sid };
                            }
                        }
                        if (tcat == "X") {
                            // move pending to done
                            foreach (var (pcat, pids) in pending) {
                                if (done.ContainsKey(pcat)) {
                                    done[pcat].UnionWith(pids);
                                } else {
                                    done[pcat] = new HashSet<int>(pids);
                                }
                            }
                            pending = new Dictionary<string, List<int>>();
                            break;
                        }
                        count -= 1;
                        if (count == 0) {
                            if (hasZero) {
                                warnings.Add(string.Format("WARNING: There is a loop in the {0} table involving id {1}, category {2}, flow type {3} but with a zero percentage", table, sid, scat, hydTyp));
                            } else {
                                errors.Add(string.Format("There is a loop in the {0} table involving id {1}, category {2}, flow type {3}", table, sid, scat, hydTyp));
                                List<int> channels;
                                if (pending.TryGetValue("CH", out channels)) {
                                    outputLoop(channels);
                                }
                                // move pending to done
                                foreach (var (pcat, pids) in pending) {
                                    if (done.ContainsKey(pcat)) {
                                        done[pcat].UnionWith(pids);
                                    } else {
                                        done[pcat] = new HashSet<int>(pids);
                                    }
                                }
                                pending = new Dictionary<string, List<int>>();
                                break;
                            }
                        }
                        var cmd2 = new SQLiteCommand(findSql, localConn);
                        cmd2.Parameters.Clear();
                        cmd2.Parameters.AddWithValue("@p1", tid);
                        cmd2.Parameters.AddWithValue("@p2", tcat);
                        using (var reader2 = cmd2.ExecuteReader()) {
                            if (!reader2.HasRows) {
                                // unnecessary to report as done in checkRoutingTable
                                //                     report = False
                                //                     if tcat not in erroneous:
                                //                         report = True
                                //                         erroneous[tcat] = {tid}
                                //                     elif tid not in erroneous[tcat]:
                                //                         erroneous[tcat].add(tid)
                                //                         report = True
                                //                     if report:
                                //                         errors.append('Cannot find id {1} category {2} as a source in the {0} table', table, tid, tcat))
                                break;  // from while loop
                            }
                            reader2.Read();
                            sid = tid;
                            scat = tcat;
                            hydTyp = reader2.GetString(0); // "hyd_typ"
                            tid = Convert.ToInt32(reader2.GetValue(1)); // "sinkid"
                            tcat = reader2.GetString(2); // "sinkcat"
                            percent = Convert.ToDouble(reader2.GetValue(3)); // "percent"
                            hasZero = hasZero || percent == 0;
                            // check all percentages are approximately 100
                        }
                    } //end of while loop
                } // end of for loop
            }
            foreach (var (scat, sourceDdata) in percentages) {
                foreach (var (sid, data) in sourceDdata) {
                    foreach (var (hydTyp, percent) in data) {
                        if (Convert.ToInt32(percent) != 100) {
                            if (percent == 0 && new HashSet<string> {
                                "RES",
                                "PND",
                                "WETL"
                            }.Contains(scat)) {
                                // an unused lake exit: ignore
                            } else {
                                errors.Add(string.Format("The percentages for id {1}, category {2}, flow type {3} in the {0} table sum to {4} rather than 100", table, sid, scat, hydTyp, Convert.ToInt32(percent)));
                            }
                        }
                    }
                }
            }
            return (errors, warnings);
        }
        
        public static string _HRUSCREATESQL = @"
        CREATE TABLE gis_hrus (
            id       INTEGER PRIMARY KEY
                             UNIQUE
                             NOT NULL,
            lsu      INTEGER,
            arsub    REAL,
            arlsu    REAL,
            landuse  TEXT,
            arland   REAL,
            soil     TEXT,
            arso     REAL,
            slp      TEXT,
            arslp    REAL,
            slope    REAL,
            lat      REAL,
            lon      REAL,
            elev     REAL
        );
        ";
        
        public static string _HRUSINSERTSQL = "INSERT INTO gis_hrus VALUES(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14)";
        
        public static string _LSUSCREATESQL = @"
        CREATE TABLE gis_lsus (
            id       INTEGER PRIMARY KEY
                             UNIQUE
                             NOT NULL,
            category INTEGER,
            channel  INTEGER,
            subbasin INTEGER,
            area     REAL,
            slope    REAL,
            len1     REAL,
            csl      REAL,
            wid1     REAL,
            dep1     REAL,
            lat      REAL,
            lon      REAL,
            elev     REAL
        );
        ";
        
        public static string _LSUSINSERTSQL = "INSERT INTO gis_lsus VALUES(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13)";
        
        public static string _POINTSCREATESQL = @"
        CREATE TABLE gis_points (
            id       INTEGER
                             NOT NULL,
            subbasin INTEGER,
            ptype    TEXT,
            xpr      REAL,
            ypr      REAL,
            lat      REAL,
            lon      REAL,
            elev     REAL
        );
        ";
        
        public static string _CHANNELSCREATESQL = @"
        CREATE TABLE gis_channels (
            id       INTEGER PRIMARY KEY
                             UNIQUE
                             NOT NULL,
            subbasin INTEGER,
            areac    REAL,
            strahler INTEGER,
            len2     REAL,
            slo2     REAL,
            wid2     REAL,
            dep2     REAL,
            elevmin  REAL,
            elevmax  REAL,
            midlat   REAL,
            midlon   REAL
        );
        ";
        
        public static string _SUBBASINSCREATESQL = @"
        CREATE TABLE gis_subbasins (
            id       INTEGER PRIMARY KEY
                             UNIQUE
                             NOT NULL,
            area     REAL,
            slo1     REAL,
            len1     REAL,
            sll      REAL,
            lat      REAL,
            lon      REAL,
            elev     REAL,
            elevmin  REAL,
            elevmax  REAL,
            waterid INTEGER
        );
        ";
        
        public static string _SUBBASINSINSERTSQL = "INSERT INTO gis_subbasins VALUES(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11)";
        
        public static string _WATERCREATESQL = @"
        CREATE TABLE gis_water (
            id    INTEGER,
            wtype TEXT,
            lsu   INTEGER,
            subbasin INTEGER,
            area  REAL,
            xpr   REAL,
            ypr   REAL,
            lat   REAL,
            lon   REAL,
            elev  REAL
        );
        ";
        
        public static string _WATERINSERTSQL = "INSERT INTO gis_water VALUES(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10)";
        
        public static string _ROUTINGCREATESQL = @"
        CREATE TABLE gis_routing (
            sourceid  INTEGER,
            sourcecat TEXT,
            hyd_typ   TEXT,
            sinkid    INTEGER,
            sinkcat   TEXT,
            percent   REAL
        );
        ";
        
        public static string _ROUTINGINDEXSQL = @"
    CREATE INDEX source ON gis_routing (
    sourceid,
    sourcecat
    );
    ";
        
        public static string _ROUTINGINSERTSQL = "INSERT INTO gis_routing VALUES(@p1,@p2,@p3,@p4,@p5,@p6)";

        public static string _ROUTINGUPDATESQL = "UPDATE gis_routing SET sinkid=@p1, sinkcat=@p2, percent=@p3 WHERE sourceid=@p4 AND sourcecat=@p5 AND hyd_typ=@p6";
        
        public static string _LANDEXEMPTCREATESQL = @"
    CREATE TABLE gis_landexempt (
    landuse TEXT
    );
    ";
        
        public static string _LANDEXEMPTINSERTSQL = "INSERT INTO gis_landexempt VALUES(@p1)";
        
        public static string _SPLITHRUSCREATESQL = @"
    CREATE TABLE gis_splithrus (
    landuse    TEXT,
    sublanduse TEXT,
    percent    REAL
    );
    ";
        
        public static string _SPLITHRUSINSERTSQL = "INSERT INTO gis_splithrus VALUES(@p1,@p2,@p3)";
        
        public static string _CREATEPROJECTCONFIG = @"
    CREATE TABLE project_config (
    id                       INTEGER  PRIMARY KEY
                                      NOT NULL
                                      DEFAULT (1),
    project_name             TEXT,
    project_directory        TEXT,
    editor_version           TEXT,
    gis_type                 TEXT,
    gis_version              TEXT,
    project_db               TEXT,
    reference_db             TEXT,
    wgn_db                   TEXT,
    wgn_table_name           TEXT,
    weather_data_dir         TEXT,
    weather_data_format      TEXT,
    input_files_dir          TEXT,
    input_files_last_written DATETIME,
    swat_last_run            DATETIME,
    delineation_done         BOOLEAN  DEFAULT (0) 
                                      NOT NULL,
    hrus_done                BOOLEAN  DEFAULT (0) 
                                      NOT NULL,
    soil_table               TEXT,
    soil_layer_table         TEXT,
    output_last_imported     DATETIME,
    imported_gis             BOOLEAN  DEFAULT (0) 
                                      NOT NULL,
    is_lte                   BOOLEAN  DEFAULT (0) 
                                      NOT NULL,
    use_gwflow               BOOLEAN  DEFAULT (0)
                                      NOT NULL
    )
    ";
        
        public static string _INSERTPROJECTCONFIG = "INSERT INTO project_config VALUES(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15,@p16,@p17,@p18,@p19,@p20,@p21,@p22,@p23)";
        
        public static string _CREATELAKESDATA = @"
    CREATE TABLE LAKESDATA (
    id         INTEGER PRIMARY KEY,
    subbasin   INTEGER,
    role       INTEGER,
    area       REAL,
    overridearea REAL,
    meanelev   REAL,
    outlink    INTEGER,
    outletid   INTEGER,
    outletx    REAL,
    outlety    REAL,
    outletelev REAL,
    centroidx  REAL,
    centroidy  REAL
    )
    ";
        
        public static string _INSERTLAKESDATA = "INSERT INTO LAKESDATA VALUES(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13)";
        
        public static string _CREATELAKELINKS = @"
    CREATE TABLE LAKELINKS (
    linkno    INTEGER,
    lakeid    INTEGER,
    inlet     BOOLEAN,
    inside    BOOLEAN,
    inletid   INTEGER,
    inletx    REAL,
    inlety    REAL,
    inletelev REAL
    )
    ";
        
        public static string _INSERTLAKELINKS = "INSERT INTO LAKELINKS VALUES(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8)";
        
        public static string _CREATELAKEBASINS = @"
    CREATE TABLE LAKEBASINS (
    subbasin  INTEGER PRIMARY KEY,
    lakeid    INTEGER
    )
    ";
        
        public static string _INSERTLAKEBASINS = "INSERT INTO LAKEBASINS VALUES(@p1,@p2)";
        
        public static string _CREATEAQUIFERS = @"
    CREATE TABLE gis_aquifers (
    id         INTEGER PRIMARY KEY,
    category   INTEGER,
    subbasin   INTEGER,
    deep_aquifer INTEGER,
    area       REAK,
    lat        REAL,
    lon        REAL,
    elev       REAL
    );
    ";
        
        public static string _INSERTAQUIFERS = "INSERT INTO gis_aquifers VALUES(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8)";
        
        public static string _CREATEADEEPQUIFERS = @"
    CREATE TABLE gis_deep_aquifers (
    id         INTEGER PRIMARY KEY,
    subbasin   INTEGER,
    area       REAK,
    lat        REAL,
    lon        REAL,
    elev       REAL
    );
    ";
        
        public static string _INSERTDEEPAQUIFERS = "INSERT INTO gis_deep_aquifers VALUES(@p1,@p2,@p3,@p4,@p5,@p6)";
        
    }

    public class Proj
    {

        private GlobalVars gv;

        public Proj(GlobalVars gv) { 
            this.gv = gv;
        }

        // if any parameter tables are empty initialise them with default values
        // this enables UPDATE commands to work.
        public void init() {
            // older projects may not have config_gwflow table
            this.makeConfigGwflow();
            var paramTables = new List<string> { "delin", "hru", "landuse", "lsu", "observed", "params", "soil", "gwflow" };
            foreach (var table in paramTables) {
                var projTable = "config_" + table;
                if (this.gv.db.hasData(projTable)) { continue; }
                var sql = $"INSERT INTO {projTable} DEFAULT VALUES;";
                this.gv.db.execNonQuery(sql);
            }
        }
        public void makeConfigGwflow() {
            var sql = @"CREATE TABLE IF NOT EXISTS config_gwflow(
    aquiferThickness    TEXT    DEFAULT(''),
    aquiferPermeability TEXT    DEFAULT(''),
    tileDrains          TEXT    DEFAULT(''),
    gridType            TEXT    DEFAULT('structured'),
    cellSize            INTEGER DEFAULT(1000),
    maxSize             INTEGER DEFAULT(1000),
    levels              INTEGER DEFAULT(0),
    transitionRate      REAL    DEFAULT(0),
    refineStreams       INTEGER DEFAULT(1),
    refineTopography    INTEGER DEFAULT(1),
    refineBoundary      INTEGER DEFAULT(1),
    refineWells         INTEGER DEFAULT(0),
    useTileDrains       INTEGER DEFAULT(0),
    alignGrids          INTEGER DEFAULT(1),
    elevTolerance       INTEGER DEFAULT(0)
)";
            this.gv.db.execNonQuery(sql);
        }


        // imitation of QGIS readEntry
        // assume result is a relative path and prefix with projDir if title is not empty
        // (a kludge exploiting fact title is not needed)
        public Tuple<string, bool> readEntry(string title, string path, string deflt) {
            gv.db.connectToProjectDatabase();
            var parts = path.Split('/');
            var table = "config_" + parts[0];
            var field = parts[1];
            if (field == "table") { field = "tabl"; } // table is an SQL reserved word
            var sql = "SELECT " + field + " FROM " + table;
            // code used for debugging
            //var sql0 = "SELECT * FROM " + table;
            //var adapter = new OleDbDataAdapter(sql0, gv.db.conn as OleDbConnection);
            //var values = new DataSet();
            //adapter.Fill(values, "Data");
            //object[] vals = new object[30];
            //foreach (DataRow r in values.Tables["Data"].Rows) {
            //    for (int i = 0; i < r.ItemArray.Length; i++) {
            //        vals[i] = r.ItemArray[i];
            //    }
            //}
            try {
                using (var reader = gv.db.getReader(sql)) {
                    if (reader.HasRows) {
                        reader.Read();
                        string result = null;
                        try {
                            var value = reader.GetValue(0);
                            if (value != null) {
                                var typ = value.GetType();
                                if (typ == typeof(string)) {
                                    result = value.ToString();
                                }
                            }
                        }
                        catch {
                            var errRes = reader.GetValue(0);
                            var typ = errRes.GetType();
                        }
                        reader.Close();
                        if (string.IsNullOrEmpty(result)) {
                            return Tuple.Create(deflt, false);
                        }
                        if (!string.IsNullOrEmpty(title)) {
                            result = Uri.UnescapeDataString(Path.Combine(gv.projDir, result));
                        }
                        return Tuple.Create(result, true);
                    } else {
                        reader.Close();
                        return Tuple.Create(deflt, false);
                    }
                }
            }
            catch {
                return Tuple.Create(deflt, false);
            }
        }

        // imitation of QGIS readBoolEntry
        public Tuple<bool, bool> readBoolEntry(string title, string path, bool deflt) {
            gv.db.connectToProjectDatabase();
            var parts = path.Split('/');
            var table = "config_" + parts[0];
            var field = parts[1];
            var sql = "SELECT " + field + " FROM " + table + ";";
            try {
                using (var reader = gv.db.getReader(sql)) {
                    if (reader.HasRows) {
                        reader.Read();
                        return Tuple.Create(reader.GetBoolean(0), true);
                    } else {
                        return Tuple.Create(deflt, false);
                    }
                }
            }
            catch {
                return Tuple.Create(deflt, false);
            }
        }

        // imitation of QGIS readNumEntry
        public Tuple<int, bool> readNumEntry(string title, string path, int deflt) {
            gv.db.connectToProjectDatabase();
            var parts = path.Split('/');
            var table = "config_" + parts[0];
            var field = parts[1];
            var sql = "SELECT " + field + " FROM " + table + ";";
            try {
                using (var reader = gv.db.getReader(sql)) {
                    if (reader.HasRows) {
                        reader.Read();
                        return Tuple.Create(Convert.ToInt32(reader.GetValue(0)), true);
                    } else {
                        return Tuple.Create(deflt, false);
                    }
                }
            }
            catch {
                return Tuple.Create(deflt, false);
            }
        }

        public Tuple<double, bool> readDoubleEntry(string title, string path, double deflt) {
            gv.db.connectToProjectDatabase();
            var parts = path.Split('/');
            var table = "config_" + parts[0];
            var field = parts[1];
            var sql = "SELECT " + field + " FROM " + table + ";";
            try {
                using (var reader = gv.db.getReader(sql)) {
                    if (reader.HasRows) {
                        reader.Read();
                        return Tuple.Create(Convert.ToDouble(reader.GetValue(0)), true);
                    } else {
                        return Tuple.Create(deflt, false);
                    }
                }
            }
            catch {
                return Tuple.Create(deflt, false);
            }
        }

        // imitation of QGIS writeEntry
        // if title is not empty relativise path before writing it
        public void writeEntry(string title, string path, string val) {
            this.gv.db.connectToProjectDatabase();
            var parts = path.Split('/');
            var table = "config_" + parts[0];
            var field = parts[1];
            if (field == "table") { field = "tabl"; } // table is an SQL reserved word
            if (!string.IsNullOrEmpty(title)) {
                val = writePath(val);
            }
            var sql = "UPDATE " + table + " SET " + field + String.Format("='{0}';", val);
            try {
                this.gv.db.execNonQuery(sql);
            }
            catch {; }
        }

        // imitation of QGIS writeNumEntry
        public void writeNumEntry(string title, string path, int val) {
            gv.db.connectToProjectDatabase();
            var parts = path.Split('/');
            var table = "config_" + parts[0];
            var field = parts[1];
            var sql = "UPDATE " + table + " SET " + field + String.Format("={0};", val);
            try {
                gv.db.execNonQuery(sql);
            }
            catch {; }
        }

        // imitation of QGIS writeEntryBool
        public void writeEntryBool(string title, string path, bool val) {
            gv.db.connectToProjectDatabase();
            var parts = path.Split('/');
            var table = "config_" + parts[0];
            var field = parts[1];
            var sql = "UPDATE " + table + " SET " + field + String.Format("={0};", val);
            try {
                gv.db.execNonQuery(sql);
            }
            catch {; }
        }

        // imitation of QGIS writeEntryDouble
        public void writeEntryDouble(string title, string path, double val) {
            gv.db.connectToProjectDatabase();
            var parts = path.Split('/');
            var table = "config_" + parts[0];
            var field = parts[1];
            var sql = "UPDATE " + table + " SET " + field + String.Format("={0};", val);
            try {
                gv.db.execNonQuery(sql);
            }
            catch {; }
        }

        public string writePath(string path) {
            // imitation of QGSProject writePath to relativise path to project directory
            if (string.IsNullOrEmpty(path)) {
                return "";
            }
            return Path.GetRelativePath(gv.projDir, path);
        }

        public string readPath(string path) {
            // imitation of QGSProject readPath to get full path
            return Utils.join(gv.projDir, path);
        }

        public string Name {
            get { return gv.projName; }
        }

        public string title {
            get { return gv.projName; }
        }
    }

}
