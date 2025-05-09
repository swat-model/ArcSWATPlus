// 
// /***************************************************************************
//  QSWATPlus
//                                  A QGIS plugin
//  Create SWATPlus inputs
//                               -------------------
//         begin                : 2014-07-18
//         copyright            : (C) 2014 by Chris George
//         email                : cgeorge@mcmaster.ca
//  ***************************************************************************/
// 
// /***************************************************************************
//  *                                                                         *
//  *   This program is free software; you can redistribute it and/or modify  *
//  *   it under the terms of the GNU General Public License as published by  *
//  *   the Free Software Foundation; either version 2 of the License, or     *
//  *   (at your option) any later version.                                   *
//  *                                                                         *
//  ***************************************************************************/
// 

using System.Data.SQLite;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using ArcGIS.Core.Data;
using static System.Net.Mime.MediaTypeNames;
using System.Globalization;
using System.Data.SqlClient;
using System.Data;
using System;
using ArcGIS.Desktop.Internal.Catalog;

namespace ArcSWATPlus
{


    // Choose an sqlite database and a table and export table as csv file.
    public class ExportTable
    {

        public ExportTableForm _dlg;

        public GlobalVars _gv;

        public SQLiteConnection conn;

        public string _db;

        public static string _PROJECTDB = "Project database";

        public static string _REFDB = "Project's reference database";

        public static string _OTHERDB = "Other sqlite database";

        public ExportTable(GlobalVars gv) {
            this._gv = gv;
            //# current database
            this._db = "";
            //# current connection
            this.conn = null;
            this._dlg = new ExportTableForm(this);
            this._dlg.databases.Items.Add(ExportTable._PROJECTDB);
            this._dlg.databases.Items.Add(ExportTable._REFDB);
            this._dlg.databases.Items.Add(ExportTable._OTHERDB);
            this._dlg.databases.SelectedIndex = -1;
        }

        // Run the form.
        public virtual void run() {
            this._dlg.ShowDialog();
        }

        // Set tables for currently selected database.
        public virtual void setTables() {
            string db;
            if (this._dlg.databases.SelectedIndex < 0) {
                Utils.information("Please select a database", this._gv.isBatch);
                return;
            }
            var item = this._dlg.databases.Text;
            if (item == ExportTable._PROJECTDB) {
                db = this._gv.db.dbFile;
            } else if (item == ExportTable._REFDB) {
                db = this._gv.db.dbRefFile;
            } else if (item == ExportTable._OTHERDB) {
                var path = Path.GetDirectoryName(this._gv.db.dbFile);
                var title = Utils.trans("Select sqlite database");
                using (var selectFileDialog = new OpenFileDialog() {
                    Title = title,
                    Filter = FileTypes.filter(FileTypes._SQLITE),
                    InitialDirectory = path
                }) {
                    if (selectFileDialog.ShowDialog() == DialogResult.OK) {
                        db = selectFileDialog.FileName;
                    } else { return; }
                }
            } else {
                return;
            }
            if (this._db != db) {
                if (this.conn is not null) {
                    this.conn.Close();
                }
                this._db = db;
                this.conn = new SQLiteConnection(db);
                if (this.conn is null) {
                    Utils.error(string.Format("Failed to connect to database {0}", db), this._gv.isBatch);
                    return;
                }
                this._dlg.tables.Items.Clear();
                var tables = new List<string>();
                var sql = "SELECT name FROM sqlite_master WHERE TYPE='table'";
                var cmd = new SQLiteCommand(sql, this.conn);
                using (var reader = cmd.ExecuteReader()) {
                    while (reader.Read()) {
                        tables.Add(reader.GetString(0));
                    }
                }
                tables.Sort();
                foreach (var table in tables) {
                    this._dlg.tables.Items.Add(table);
                }
            }
            this._dlg.tables.SelectedIndex = -1;
        }

        // Export selected table as csv file.
        public virtual void exportTable() {
            if (this.conn is null) {
                this.setTables();
            }
            if (this._dlg.tables.SelectedIndex < 0) {
                Utils.information("Please select a table", this._gv.isBatch);
                return;
            }
            var table = this._dlg.tables.Text;
            var sql = string.Format("PRAGMA TABLE_INFO({0})", table);
            var varz = new List<string>();
            var cmd = new SQLiteCommand(sql, this.conn);
            using (var reader = cmd.ExecuteReader()) {
                while (reader.Read()) {
                    varz.Add(reader.GetString(1));
                }
            }
            var path = Path.GetDirectoryName(this._gv.db.dbFile);
            var title = Utils.trans("Select csv file name");
            using (var saveFileDialog = new SaveFileDialog() {
                Title = title,
                Filter = FileTypes.filter(FileTypes._CSV),
                InitialDirectory = path
            }) {
                string csvFile;
                if (saveFileDialog.ShowDialog() == DialogResult.OK) {
                    csvFile = saveFileDialog.FileName;
                } else { return; }
                using (var fs = saveFileDialog.OpenFile()) {
                    var header = string.Join(",", varz) + "\r\n";
                    fs.Write(System.Text.Encoding.ASCII.GetBytes(header));
                    var vals = new List<string>();
                    sql = string.Format("SELECT * FROM {0}", table);
                    cmd = new SQLiteCommand(sql, this.conn);
                    using (var reader = cmd.ExecuteReader()) {
                        Object[] values = new Object[reader.FieldCount];
                        while (reader.Read()) {
                            var fields = reader.GetValues(values);
                            foreach (var val in values) {
                                if (DBNull.Value.Equals(val)) {
                                    vals.Add("");
                                } else {
                                    vals.Add(val.ToString());
                                }
                            }
                        }
                    }
                    var line = string.Join(",", vals) + "\r\n";
                    fs.Write(System.Text.Encoding.ASCII.GetBytes(line));
                }
                Utils.information(string.Format("Table {0} written to {1}", table, csvFile), this._gv.isBatch);
            }
        }

        // Close connection and form.
        public virtual void close() {
            if (this.conn is not null) {
                this.conn.Close();
                this.conn.Dispose();
            }
            this._dlg.Close();
        }
    }
}
