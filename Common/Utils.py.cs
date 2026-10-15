



using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.Raster;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Internal.CIM;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.Catalog;
using ArcGIS.Desktop.Internal.Catalog.PropertyPages.ParcelDataset;
using ArcGIS.Desktop.Internal.Core.Conda;
using ArcGIS.Desktop.Internal.Mapping.CommonControls;
using ArcGIS.Desktop.Internal.Mapping.Ribbon;
using ArcGIS.Desktop.Internal.Mapping.Voxel.Controls.Transparency;
using ArcGIS.Desktop.Mapping;
using ArcSWATPlus;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.VisualBasic.Logging;
using Microsoft.Win32;
using OSGeo.OGR;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
//using System.Data.Entity.Migrations.Sql;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using static System.Net.Mime.MediaTypeNames;
using Envelope = ArcGIS.Core.Geometry.Envelope;
//using ArcGIS.Desktop.Framework.Utilities;
using EventLog = ArcGIS.Desktop.Framework.Utilities.EventLog;
using Feature = ArcGIS.Core.Data.Feature;
using Layer = ArcGIS.Desktop.Mapping.Layer;
using LinearUnit = ArcGIS.Core.Geometry.LinearUnit;
//using ArcGIS.Desktop.Framework.Dialogs;
using MessageBox = ArcGIS.Desktop.Framework.Dialogs.MessageBox;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using Path = System.IO.Path;
using Polygon = ArcGIS.Core.Geometry.Polygon;
using QueryFilter = ArcGIS.Core.Data.QueryFilter;
using SpatialReference = ArcGIS.Core.Geometry.SpatialReference;

namespace ArcSWATPlus
{
    public static class Utils
    {

        public static string _DATEFORMAT = "d MMMM yyyy";

        public static string _ARCSWATPLUSNAME = "ArcSWAT+";

        public static string _SLOPE_GROUP_NAME = "Slope";

        public static string _LANDUSE_GROUP_NAME = "Landuse";

        public static string _SOIL_GROUP_NAME = "Soil";

        public static string _WATERSHED_GROUP_NAME = "Watershed";

        //public static string _RESULTS_GROUP_NAME = "Results";

        //public static string _ANIMATION_GROUP_NAME = "Animations";

        public static string _DEMLEGEND = "DEM";

        public static string _SNAPPEDLEGEND = "Snapped inlets/outlets";

        public static string _SELECTEDLEGEND = "Selected inlets/outlets";

        public static string _DRAWNLEGEND = "Drawn inlets/outlets";

        public static string _EXTRALEGEND = "Extra inlets/outlets";

        public static string _LAKESLEGEND = "Lakes";

        public static string _RESANDPTSRCLEGEND = "Reservoirs and point sources";

        public static string _FULLHRUSLEGEND = "Full HRUs";

        public static string _ACTHRUSLEGEND = "Actual HRUs";

        public static string _HILLSHADELEGEND = "Hillshade";

        public static string _STREAMSLEGEND = "Streams";

        public static string _CHANNELSLEGEND = "Channels";

        public static string _CHANNELREACHESLEGEND = "Channel reaches";

        public static string _SUBBASINSLEGEND = "Subbasins";

        public static string _WATERSHEDLEGEND = "Watershed";

        public static string _GRIDLEGEND = "Watershed grid";

        public static string _GRIDSTREAMSLEGEND = "Grid streams";

        public static string _DRAINSTREAMSLEGEND = "Drainage streams";

        public static string _BUFFERFLOODLEGEND = "Floodplain by buffer";

        public static string _INVFLOODLEGEND = "Floodplain by inversion";

        public static string _BRANCHFLOODLEGEND = "Floodplain by branch";

        public static string _FULLLSUSLEGEND = "Full LSUs";

        public static string _ACTLSUSLEGEND = "Actual LSUs";

        public static string _EXTRAPTSRCANDRESLEGEND = "Pt sources and reservoirs";

        public static string _LSUSLEGEND = "LSUs";

        public static string _HRUSLEGEND = "HRUs";

        public static string _AQUIFERSLEGEND = "Aquifers";

        public static string _GWFLOWCELLSLEGEND = "GWFlow cells";

        public static int _NOLANDSCAPE = 0;

        public static int _FLOODPLAIN = 1;

        public static int _UPSLOPE = 2;

        public static List<int> _dX = new List<int> {
            1,
            1,
            0,
            -1,
            -1,
            -1,
            0,
            1
        };

        public static List<int> _dY = new List<int> {
            0,
            -1,
            -1,
            -1,
            0,
            1,
            1,
            1
        };

            // Report msg as an error.  If not reportErrors merely log the message.

        public static void error(string msg, bool isBatch, bool reportErrors = true, string logFile = null) {
            Utils.logerror(msg);
            if (!reportErrors) {
                return;
            }
            if (isBatch) {
                // in batch mode we generally only look at stdout 
                // (to avoid distracting messages from gdal about .shp files not being supported)
                // so report to stdout
                if (logFile is null) {
                    Console.Write(String.Format("ERROR: {0}\n", msg));
                } else {
                    using (FileStream log = new FileStream(logFile, FileMode.Append, FileAccess.Write))
                    using (StreamWriter sw = new StreamWriter(log)) {
                        sw.WriteLine(String.Format("ERROR: {0}", msg));
                    }
                }
#if TESTING
                throw new Exception(String.Format("ERROR: {0}", msg));
#else
#endif
            } else {
                MessageBox.Show("ERROR: " + msg, Utils._ARCSWATPLUSNAME);
            }
            return;
        }

        // non-modal message box returning OK or Cancel
        public static MessageBoxResult nonModalMessage(string msg, string title, bool isBatch) {
            MessageBoxResult result = MessageBoxResult.OK;
            if (!isBatch) {
                new Thread(() => result = MessageBox.Show(msg, title, MessageBoxButton.OKCancel)).Start();
            }
            return result;
        }

        // Ask msg as a question, returning Yes or No.

        public static MessageBoxResult question(string msg, bool isBatch, bool affirm, string logFile = null) {
            MessageBoxResult result;
            object res;
            // only ask question if interactive
            if (!isBatch) {
                result = MessageBox.Show(msg, Utils._ARCSWATPLUSNAME, MessageBoxButton.YesNo);
            } else {
                // batch: use affirm parameter
                if (affirm) {
                    result = MessageBoxResult.Yes;
                } else {
                    result = MessageBoxResult.No;
                }
            }
            if (result == MessageBoxResult.Yes) {
                res = " Yes";
            } else {
                res = " No";
            }
            Utils.loginfo(msg + res);
            if (isBatch) {
                if (logFile is null) {
                    Console.WriteLine(String.Format("{0}", msg + res));
                } else {
                    using (FileStream log = new FileStream(logFile, FileMode.Append, FileAccess.Write))
                    using (StreamWriter sw = new StreamWriter(log)) {
                        sw.WriteLine(String.Format("{0}", msg + res));
                    }
                }
            }
            return result;
        }

        // Report msg as information.

        public static void information(string msg, bool isBatch, bool reportErrors = true, string logFile = null) {
            Utils.loginfo(msg);
            if (!reportErrors) {
                return;
            }
            if (isBatch) {
                if (logFile is null) {
                    Console.WriteLine(String.Format("{0}", msg));
                } else {
                    using (FileStream log = new FileStream(logFile, FileMode.Append, FileAccess.Write))
                    using (StreamWriter sw = new StreamWriter(log)) {
                        sw.WriteLine(String.Format("{0}", msg));
                    }
                }
            } else {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(msg, Utils._ARCSWATPLUSNAME);
            }
            return;
        }

        public static string DisplaySet<T>(IEnumerable<T> collection) {
            var s = "{";
            foreach (T i in collection) {
                s += string.Format(" {0}", i);
            }
            s += " }";
            return s;
        }


        public static void exceptionError(string msg, bool isBatch) {
            Utils.error(String.Format("{0}", msg), isBatch);
            return;
        }

        public static string logFile {
            get {
                var logName = Path.ChangeExtension(Project.Current.Name, ".log");
                return Path.Combine(Project.Current.HomeFolderPath, logName);
            }
        }

        public static void openLog() {
            var log = logFile; 
            using (var w = new StreamWriter(log, false)) {
                w.WriteLine(DateTime.Now.ToString("f"));
            }
        }

        // Log message as information.

        public static void loginfo(string msg, EventLog.EventType typ = EventLog.EventType.Information) {
            try {
                switch (typ) {
                    case EventLog.EventType.Information:
                        break;
                    case EventLog.EventType.Error:
                        msg = "ERROR: " + msg; break;
                    case EventLog.EventType.Warning:
                        msg = "WARNING: " + msg; break;
                }
                using (var w = new StreamWriter(logFile, true)) {
                    w.WriteLine(msg);
                }
            } catch {
                Console.WriteLine(msg);
            }
        }

        // Log message as error.

        public static void logerror(string msg) {
            Utils.loginfo(msg, EventLog.EventType.Error);
        }

        // Translate msg according to current locale.

        public static string trans(string msg) {
            return msg;   // QApplication.translate("QSWATPlus", msg, null);
        }

        // If filePath exists, return filePath relative to path, else return empty string.

        public static string relativise(string filePath, string path) {
            if (File.Exists(filePath) || Directory.Exists(filePath)) {
                string rel = Path.GetRelativePath(path, filePath);
                return rel; // rel.Replace("\\", "/");
            } else {
                return "";
            }
        }

        // Use appropriate path separator.

        public static string join(string path, string fileName) {
            return Path.Combine(path, fileName);
        }

        // Return true if absolute paths of the two paths are the same.

        public static bool samePath(string p1, string p2) {
            try {
                return string.Equals(Path.GetFullPath(p1), Path.GetFullPath(p2));
            } catch (Exception) {
                return false;
            }
        }

        public static string AddinAssemblyLocation() {
            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            return System.IO.Path.GetDirectoryName(
                              Uri.UnescapeDataString(
                                      new Uri(asm.Location).LocalPath));
        }

        public static bool LoadAssembly(string assemblyName) {
            try {
                var asm = Assembly.LoadFrom(Utils.join(AddinAssemblyLocation(), assemblyName + ".dll"));
                return asm != null;
            }
            catch (Exception ex) {
                Utils.error("Loading assembly " + assemblyName + " threw exception " + ex.Message, false);
                return false;
            }
        }


        // 
        //         Copy .prj file, if it exists, from inFile to .prj file of outFile,
        //         unless outFile is .dat or outFile prj already exists
        //         

        public static void copyPrj(string inFile, string outFile) {
            string outSuffix = Path.GetExtension(outFile);
            if (outSuffix != ".dat") {
                var inPrj = Path.ChangeExtension(inFile, ".prj");
                var outPrj = Path.ChangeExtension(outFile, ".prj");
                if (File.Exists(inPrj) && !File.Exists(outPrj)) {
                    File.Copy(inPrj, outPrj, true);
                }
            }
        }

        // Return true (outFile is up to date) if inFile exists, outFile exists and is no younger than inFile.

        public static bool isUpToDate(string inFile, string outFile) {
            if (!File.Exists(inFile)) {
                return false;
            }
            if (File.Exists(outFile)) {
                if (File.GetLastWriteTime(outFile) >= File.GetLastWriteTime(inFile)) {
                    //Utils.loginfo(String.Format("{0} is up to date compared to {1}", outFile, inFile));
                    return true;
                }
            }
            //Utils.loginfo(String.Format("{0} is not up to date compared to {1}", outFile, inFile));
            return false;
        }

        // Set label text and repaint.

        public static void progress(string text, System.Windows.Forms.Label label) {
            label.Text = text;
            // shows on console if visible; more useful in testing when appears on standard output
            Console.WriteLine(text);
            // calling processEvents after label.clear can cause QGIS to hang
            label.Refresh();
        }

        // sub for testing
        public static void progress(string text, string label) { }

        // If path is P/X.shp and P/X is a directory, return P/X/X.shp,
        //         else return path.

        public static string dirToShapefile(string path) {
            var @base = Path.ChangeExtension(path, null);
            if (Directory.Exists(@base)) {
                var baseName = Path.GetFileName(@base);
                return Utils.join(@base, baseName + ".shp");
            }
            return path;
        }

        // Return FileInfo of raster or vector layer.

        public static async Task<FileInfo> layerFileInfo(Layer layer) {
            if (layer is null) {
                return null;
            }
            var path = await layerFilename(layer);
            return new FileInfo(path);
        }

        // Return path of raster layer.
        public static async Task<string> layerFilename(Layer layer) {
            if (layer is null) { 
                return null; 
            }
            CIMStandardDataConnection connection = null;
            await QueuedTask.Run(() => {
                connection = layer.GetDataConnection() as CIMStandardDataConnection;
            });
            if (connection is null) { return null; }  // eg group layers
            string fileName = connection.Dataset;
            string wcs = connection.WorkspaceConnectionString;
            // wcs takes the form "DATABASE=..." where ... is the directory
            int indx = wcs.IndexOf('=');
            string dir = wcs.Substring(indx + 1);
            return Path.Combine(dir, fileName);
        }

        // Remove any layers for fileName; delete files with same basename 
        //         regardless of suffix.
        //         

        public static async Task removeLayerAndFiles(string fileName) {
            await Utils.removeLayer(fileName);
            Utils.removeFiles(fileName);
        }

        // Remove any layers for fileName; delete files with same basename 
        //         regardless of suffix, but allow deletions to fail.
        //         

        //public static void tryRemoveLayerAndFiles(string fileName)
        //{
        //    Utils.removeLayer(fileName);
        //    Utils.removeFiles(fileName);
        //}

        // Remove directory containing shapefile and any layers.

        public static async Task tryRemoveShapefileLayerAndDir(string direc) {
            var @base = Path.GetDirectoryName(direc);
            var shapefile = Utils.join(direc, @base + ".shp");
            await Utils.removeLayer(shapefile);
            // need to ignore locks in ArcGIS
            try {
                Directory.Delete(direc, true);
            }
            catch (Exception) {; }
        }

        // Remove any MapView layers for fileName.
        public static async Task removeLayer(string fileName) {
            var layers = ArcSWATPlus.mainMap.GetLayersAsFlattenedList().Where(l => l is FeatureLayer || l is RasterLayer).ToList();
            for (int i = 0; i < layers.Count; i++) {
                Layer layer = layers[i];
                var path = await Utils.layerFilename(layer);
                // layerFilename sometimes lacks extension
                if (Utils.samePath(fileName, path) || Utils.samePath(Path.ChangeExtension(fileName, null), path)) {
                    await QueuedTask.Run(() => {
                        ArcSWATPlus.mainMap.RemoveLayer(layer);
                    });
                    if (Utils.hasLayer(layer)) {
                        Utils.error(string.Format("failed to remove layer for {0}", fileName), false);
                    }
                    layer = null;
                }
            }
        }

        // Remove MapView layer

        public static async Task removeLayer(Layer layer) {
            await QueuedTask.Run(() => {
                ArcSWATPlus.mainMap.RemoveLayer(layer);
            });
        }

        public static bool hasLayer(Layer layer) {
            return ArcSWATPlus.mainMap.GetLayersAsFlattenedList().Where(l => l is FeatureLayer || l is RasterLayer).Contains(layer);
        }


        // Remove any layers whose legend name starts with the legend.

        public static async Task removeLayerByLegend(string legend) {
            // empty legend would remove all layers
            if (legend == "") {
                return;
            }
            var layers = ArcSWATPlus.mainMap.GetLayersAsFlattenedList().Where(l => l is FeatureLayer || l is RasterLayer);
            foreach (var layer in layers) {
                if (layer.Name.StartsWith(legend)) {
                    await QueuedTask.Run(() => {
                        ArcSWATPlus.mainMap.RemoveLayer(layer);
                    });
                    if (Utils.hasLayer(layer))
                        Utils.error(string.Format("failed to remove layer for legend {0}", legend), false);
                }
            }
        }

        //        // Remove all features from layer.
        //        
        //        public static bool removeAllFeatures(object layer) {
        //        	if (layer is FeatureLayer) {
        //        		
        //            var provider = layer.dataProvider();
        //            var request = QgsFeatureRequest().setFlags(QgsFeatureRequest.NoGeometry);
        //            var ids = (from feature in provider.getFeatures(request)
        //                select feature.id()).ToList();
        //            return provider.deleteFeatures(ids);
        //        }

        // Set map layer visible or not according to visibility.

        public static async Task setLayerVisibility(Layer layer, bool visibility) {
            await QueuedTask.Run(() => {
                try {
                    layer.SetVisibility(visibility);
                }
                catch (Exception) {
                    // layer probably removed - just exit
                    return;
                }
            });
        }

        // Find a non-group layer if any whose legend name starts with the legend.

        public static Layer getLayerByLegend(string legend) {
            // avoid default of finding background layer
            if (string.IsNullOrEmpty(legend)) { return null; }
            List<Layer> layers = ArcSWATPlus.mainMap.GetLayersAsFlattenedList().Where(layer => layer.Name.StartsWith(legend)).ToList();
            if (layers.Count > 0) {
                foreach (Layer layer in layers) {
                    if (layer is GroupLayer) { continue; }  //beware of Watershed layer and Watershed group layer
                    return layer;
                }
                return null;
            }
            return null;
        }

        // Find a group layer by name.

        public static GroupLayer getGroupLayerByName(string name) {
            System.Collections.Generic.List<GroupLayer> groupLayers =
                ArcSWATPlus.mainMap.Layers.OfType<GroupLayer>().Where(layer => string.Equals(layer.Name, name)).ToList();
            if (groupLayers.Count > 0) {
                return groupLayers[0];
            }
            return null;
        }

        // Return list of layers in group, restricted to visible if visible is true.

        public static List<Layer> getLayersInGroup(string group, bool visible = false) {
            var groupLayer =
                ArcSWATPlus.mainMap.Layers.OfType<GroupLayer>().FirstOrDefault(layer => string.Equals(layer.Name, group));
            var result = new List<Layer>();
            if (groupLayer is null) {
                return result;
            } else {
                foreach (var layer in groupLayer.Layers) {
                    if (visible == false || layer.IsVisible) {
                        result.Add(layer);
                    }
                }
                return result;
            }
        }

        // Return number of layers in group.

        public static int countLayersInGroup(string group) {
            var groupLayer =
                ArcSWATPlus.mainMap.Layers.OfType<GroupLayer>().FirstOrDefault(layer => string.Equals(layer.Name, group));
            if (groupLayer is null) {
                return 0;
            } else {
                return groupLayer.Layers.Count;
            }
        }

        public static async Task clearAnimationMaps() {
            // remove animation maps
            var mapItems = Project.Current.GetItems<MapProjectItem>();
            foreach (var mapItem in mapItems) {
                if (mapItem.Name.StartsWith("Animate ")) {
                    await QueuedTask.Run(
                        () => Project.Current.RemoveItem(mapItem));
                }
            }
        }

        //        // Find layer by id, and raise exception if not found.
        //        
        //        public static void getLayerById(Utils lid, object layers) {
        //            foreach (var layer in layers) {
        //                if (layer.id() == lid) {
        //                    return layer;
        //                }
        //            }
        //            throw new ValueError("Cannot find layer with identifier {0}", lid));
        //        }

        // 
        //         Delete all files with same root as fileName, 
        //         i.e. regardless of suffix.
        //         

        public static void removeFiles(string fileName) {
            var direc = Path.GetDirectoryName(fileName);
            var pattern = Path.GetFileNameWithoutExtension(fileName) + ".*";
            tryRemoveFilePattern(direc, pattern);
            // some files can have double extension, eg attribute files .vat.dbf
            tryRemoveFilePattern(direc, pattern + ".*");
        }

        // 
        //         Try to delete all files with same root as fileName, 
        //         i.e. regardless of suffix unless provided, and including extension, eg numbers.
        //         
        //         If the filename supplied is X.Y, this will try to remove 
        //         all files matching X*.*.
        //         If the filename supplied is X.Y, and Z is the suffix argument this will try to remove 
        //         all files matching X*.Z.  Y can be the same as Z: it is ignored.
        //         

        public static void tryRemoveExtendedFiles(string fileName, string suffix = "") {
            var direc = Path.GetDirectoryName(fileName);
            var @base = Path.ChangeExtension(Path.GetFileName(fileName), null);
            if (suffix != "" && !suffix.StartsWith(".")) {
                suffix = "." + suffix;
            }
            var pattern = suffix == "" ? @base + "*.*" : @base + "*" + suffix;
            tryRemoveFilePattern(direc, pattern);
        }

        // 
        //         Delete all files with same root as fileName, 
        //         i.e. regardless of suffix, but allow deletions to fail.
        //         

        //public static void tryRemoveFiles(string fileName)
        //{
        //    var pattern = Path.ChangeExtension(fileName, null) + ".*";
        //    Utils.tryRemoveFilePattern(pattern);
        //}

        // Delete all files in direc matching pattern, allowing deletions to fail.
        public static void tryRemoveFilePattern(string direc, string pattern) {
            Matcher matcher = new Matcher();
            matcher.AddInclude(pattern);
            foreach (var f in matcher.GetResultsInFullPath(direc)) {
                try {
                    File.Delete(f);
                }
                catch (Exception) {
                    continue;
                }
            }
        }

        // Set access and modification time of file to now

        public static void setFileModifiedNow(string path) {
            File.SetLastWriteTime(path, DateTime.Now);
        }

        // 
        //         Copy files with same basename as file with info  inInfo to saveDir, 
        //         i.e. regardless of suffix.
        //         Set last modified time to now.
        //         
        public static void copyFiles(FileInfo inInfo, string saveDir) {
            var inFile = inInfo.Name;
            var inPath = inInfo.FullName;
            if (inFile == "sta.adf" || inFile == "hdr.adf") {
                // ESRI grid: need to copy top directory of inInfo to saveDir
                var inDir = inInfo.Directory.FullName;
                var inDirName = inInfo.Directory.Name;
                var savePath = Utils.join(saveDir, inDirName);
                // guard against trying to copy to itself
                if (!Utils.samePath(inDir, savePath)) {
                    if (Directory.Exists(savePath)) {
                        Directory.Delete(savePath, true);
                    }
                    Directory.CreateDirectory(savePath);
                    foreach (string s in Directory.GetFiles(inDir)) {
                        var fileName = Path.GetFileName(s);
                        var destFile = Path.Combine(savePath, fileName);
                        File.Copy(s, destFile, true);
                    }
                }
            } else if (!Utils.samePath(inPath, saveDir)) {
                string basename = inInfo.Name;
                string pattern = Path.ChangeExtension(inFile, null) + ".*";
                foreach (string s in Directory.GetFiles(Path.GetDirectoryName(inPath), pattern)) {
                    var fileName = Path.GetFileName(s);
                    var destFile = Path.Combine(saveDir, fileName);
                    File.Copy(s, destFile, true);
                    Utils.setFileModifiedNow(destFile);
                }
            }
        }

        // Copy files with same basename as infile to outdir, setting basename to outbase,
        //         and last modified time to now.

        public static void copyShapefile(string inFile, string outBase, string outDir) {
            string inFileName = Path.GetFileName(inFile);
            string inBase = Path.ChangeExtension(inFileName, null);
            string inDir = Path.GetDirectoryName(inFile);
            if (Utils.samePath(inDir, outDir) && inBase == outBase) {
                // avoid copying to same file
                return;
            }
            string pattern = inBase + ".*";
            foreach (string s in Directory.GetFiles(inDir, pattern)) {
                var suffix = Path.GetExtension(s);
                if (suffix != ".lock") {
                    var outfile = Utils.join(outDir, outBase + suffix);
                    File.Copy(s, outfile, true);
                    Utils.setFileModifiedNow(outfile);
                }
            }
        }

        //public static void removeLocks(string inFile) {
        //    // remove any inactive locks on infile
        //    string inFileName = Path.GetFileName(inFile);
        //    string inBase = Path.ChangeExtension(inFileName, null);
        //    string inDir = Path.GetDirectoryName(inFile);
        //    string pattern = inBase + ".*";  //use this rather than *.lock because can be .XXX.lock
        //    foreach (string s in Directory.GetFiles(inDir, pattern)) {
        //        if (Path.GetExtension(s) == ".lock") {
        //            try {
        //                File.Delete(s);
        //            }
        //            catch {; }
        //        }
        //    }
        //}

        // Assuming infile is .shp, check existence of .shp. .shx and .dbf.

        public static bool shapefileExists(string infile)
        {
            if (!File.Exists(infile))
            {
                return false;
            }
            var shxFile = Path.ChangeExtension(infile, ".shx");
            if (!File.Exists(shxFile))
            {
                return false;
            }
            var dbfFile = Path.ChangeExtension(infile, ".dbf");
            if (!File.Exists(dbfFile))
            {
                return false;
            }
            return true;
        }

        // If baseFile takes form X.Y, returns Xz.Y where z is the smallest integer >= n such that Xz.Y does not exist.
        // Also changes n to z.
        public static string nextFileName(string baseFile, ref int n)
        {
            string @base = Path.ChangeExtension(baseFile, null);
            string suffix = Path.GetExtension(baseFile);
            var nextFile = @base + n.ToString() + suffix;
            while (File.Exists(nextFile))
            {
                n += 1;
                nextFile = @base + n.ToString() + suffix;
            }
            return nextFile;
        }

        // 
        //         Look for file that should have a map layer and return it. 
        //         If not found by filename, try legend, either as given or by file type ft.
        //         
        public static async Task<Layer> getLayerByFilenameOrLegend(
            string fileName,
            int ft,
            string legend,
            bool isBatch)
        {
            var layer = (await Utils.getLayerByFilename(fileName, ft, null, null, null)).Item1;
            if (!(layer is null))
            {
                return layer;
            }
            var lgnd = legend == "" ? FileTypes.legend(ft) : legend;
            layer = Utils.getLayerByLegend(lgnd);
            if (!(layer is null))
            {
                var info = await Utils.layerFileInfo(layer);
                if (!(info is null))
                {
                    var possFile = info.FullName;
                    if (Utils.question(String.Format("Use {0} as {1} file?", possFile, lgnd), isBatch, true) == MessageBoxResult.Yes)
                    {
                        return layer;
                    }
                }
            }
            return null;
        }

        public static (double, bool) getHorizontalFactor(ArcGIS.Core.Geometry.Unit units) {
            //Get horizontal factor (multiplier to metres from units), plus OK flag

            double factor = 0.0;
            bool OK = false;
            if (units == ArcGIS.Core.Geometry.LinearUnit.Meters) {
                factor = 1.0;
                OK = true;
            } else if (units.UnitType == ArcGIS.Core.Geometry.UnitType.Linear) {
                factor = ((ArcGIS.Core.Geometry.LinearUnit)units).MetersPerUnit;
                OK = true;
            }
            return (factor, OK);
        }

        // Clip layer to just outside DEM if it is 10% or more bigger.

        public static async Task<string> clipLayerToDEM(RasterLayer rasterLayer, string fileName, string legend, GlobalVars gv)
        {
            var extent = await QueuedTask.Run(() => rasterLayer.QueryExtent());
            var crs = await QueuedTask.Run(() => rasterLayer.GetSpatialReference());
            var unit = crs.Unit;
            double factor;
            bool OK;
            (factor, OK) = Utils.getHorizontalFactor(unit);
            if (!OK)
            {
                // something odd has happened - probably in lat-long - reported elsewhere
                return fileName;
            }
            // take demExtent plus 1% as clip
            Envelope demExtent = gv.topo.demExtent;  // use EnvelopeBuilderEx.CreateEnvelope(0, 0, 0, 0); to stop clipping
            Envelope clipExtent = demExtent.Expand(1.01, 1.01, true);
            // check if no excess to clip
            if (extent.XMin > clipExtent.XMin || extent.XMax < clipExtent.XMax || extent.YMin > clipExtent.YMin || extent.YMax < clipExtent.YMax) {
                return fileName;
            }
            string baseName = Path.ChangeExtension(fileName, null);
            var clipName = baseName + "_clip.tif";
            await Utils.removeLayerByLegend(legend);
            Utils.removeFiles(clipName);
            var args = Geoprocessing.MakeValueArray(fileName, clipExtent, clipName, "", "", "NONE", "NO_MAINTAIN_EXTENT");
            await QueuedTask.Run(() => Geoprocessing.ExecuteToolAsync("management.Clip", args, null, null, null, GPExecuteToolFlags.None));
            //Debug.Assert(File.Exists(clipName), String.Format("Failed to clip raster {0} to make {1}", fileName, clipName));
            Utils.copyPrj(fileName, clipName);
            var attributeFile = clipName + ".vat.dbf";
            if (!File.Exists(attributeFile)) {
                // need to build attributes table before loading file or table will be ignored if loaded again
                var parms = Geoprocessing.MakeValueArray(clipName, "NONE");
                Utils.runPython("runBuildRasterAttributes.py", parms, gv);
            }
            return clipName;
        }

        // 
        //         Return map layer for this fileName and flag to indicate if new layer, 
        //         loading it if necessary if groupName is not None.
        //         
        //         If subLayer is not none, finds its index and inserts the new layer immediately above it.
        //         

        public static async Task<(Layer, bool)> getLayerByFilename(
            string fileName,
            int ft,
            GlobalVars gv,
            Layer subLayer,
            string groupName,
            bool clipToDEM = false) {
            if (FileTypes.isRaster(ft) && Directory.Exists(fileName)) {  // ESRI raster
                fileName = Path.Combine(fileName, "hdr.adf");
            }
            if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName)) {
                return (null, false);
            }
            string layerFile;
            // make sure any layer changes have completed by clearing the MCT task queue
            //await QueuedTask.Run(() => {; });
            foreach (var layer in ArcSWATPlus.mainMap.GetLayersAsFlattenedList().Where (l => (l is RasterLayer) || (l is FeatureLayer)))
            {
                layerFile = await Utils.layerFilename(layer);
                if (Utils.samePath(layerFile, fileName))
                {
                    if (layer is FeatureLayer) {
                        await setMapTip((FeatureLayer)layer, ft, gv);
                    }
                    return (layer, false);
                }
            }
            // not found: load layer if requested
            if (groupName is not null)
            {
                var legend = FileTypes.legend(ft);
                var styleFile = FileTypes.styleFile(ft);
                var baseName = Path.ChangeExtension(Path.GetFileName(fileName), null);
                if (Path.GetExtension(fileName) == ".adf")
                {
                    // ESRI grid: use directory name as baseName
                    baseName = Path.GetFileName(Path.GetDirectoryName(fileName));
                }
                if (ft == FileTypes._OUTLETS)
                {
                    if (baseName.EndsWith("_snap"))
                    {
                        legend = Utils._SNAPPEDLEGEND;
                    }
                    else if (baseName.EndsWith("_sel"))
                    {
                        legend = Utils._SELECTEDLEGEND;
                    }
                    else if (baseName.EndsWith("extra"))
                    {
                        // note includes arcextra
                        legend = Utils._EXTRALEGEND;
                    }
                    else if (baseName == "drawoutlets")
                    {
                        legend = Utils._DRAWNLEGEND;
                    }
                }

                // multiple layers not allowed
                await Utils.removeLayerByLegend(legend);
                int index = 0;
                GroupLayer groupLayer = null;
                groupLayer =
                    ArcSWATPlus.mainMap.Layers.OfType<GroupLayer>().FirstOrDefault(layer => string.Equals(layer.Name, groupName));
                if (groupLayer is null)
                {
                    Utils.information(String.Format("Internal error: cannot find group {0}.", groupName), false /* gv.isBatch */);
                    return (null, false);
                }
#if TESTING
#else
                await QueuedTask.Run(() => {
                    groupLayer.SetExpanded(true);
                });
#endif
                if (subLayer is null)
                {
                    index = 0;
                }
                else
                {
                    index = Utils.groupIndex(groupLayer, subLayer);
                }
                Layer mapLayer = null;
                if (FileTypes.isRaster(ft)) {
                    if (clipToDEM || ft == FileTypes._SLOPEBANDS) {  // landuse or soil raster, or slope bands
                        var attributeFile = fileName + ".vat.dbf";
                        // replace it case slope limits changed or landuse or soil changed
                        if (File.Exists(attributeFile)) {
                            Utils.removeFiles(attributeFile);
                        }
                        // need to build attributes table before loading file or table will be ignored if loaded again
                        var parms = Geoprocessing.MakeValueArray(fileName, "NONE");
                        Utils.runPython("runBuildRasterAttributes.py", parms, gv);
                    }
                    // for ESRI rasters, QGIS loads hdr.adf, but ArcGIS needs path of directory
                    if (Path.GetFileName(fileName) == "hdr.adf") {
                        fileName = Path.GetDirectoryName(fileName);
                        //var dsName = Path.GetFileName(fileName);
                        //var connDir = Path.GetDirectoryName(fileName);
                        await QueuedTask.Run(() => {
                            //// Create a FileSystemConnectionPath using the folder path
                            //FileSystemConnectionPath connectionPath = new FileSystemConnectionPath(new Uri(connDir), FileSystemDatastoreType.Raster);
                            //// Create a new FileSystemDatastore using the FileSystemConnectionPath.
                            //var dataStore = new FileSystemDatastore(connectionPath);
                            //// Open the raster dataset.
                            //RasterDataset fileRasterDataset = dataStore.OpenDataset<RasterDataset>(dsName);
                            //var layerParams = new RasterLayerCreationParams(fileRasterDataset) {
                            //    MapMemberPosition = MapMemberPosition.Index,
                            //    Name = String.Format("{0} ({1})", legend, baseName),
                            //    MapMemberIndex = index
                            //};
                            mapLayer = LayerFactory.Instance.CreateLayer(new Uri(fileName), groupLayer, index, String.Format("{0} ({1})", legend, baseName));
                        });
                    } else {
                        await QueuedTask.Run(() => {
                            var uri = new Uri(fileName);
                            string txt = String.Format("{0} ({1})", legend, baseName);
                            mapLayer = LayerFactory.Instance.CreateLayer(uri, groupLayer, index, txt);
                        });
                        //var dsName = Path.GetFileNameWithoutExtension(fileName);
                        //var parms = Geoprocessing.MakeValueArray(fileName, gv.gdb, dsName);
                        //Utils.runPython("runCopyRaster.py", parms, gv);
                        //var uri = new Uri(Utils.join(gv.gdb, dsName));
                        //mapLayer = await QueuedTask.Run(() => LayerFactory.Instance.CreateLayer(uri, groupLayer, index, String.Format("{0} ({1})", legend, baseName)));
                    }
                    if (clipToDEM) {
                        var currentName = fileName;
                        fileName = await Utils.clipLayerToDEM((RasterLayer)mapLayer, fileName, legend, gv);
                        if (fileName != currentName) {
                            // mapLayer needs to be replaced
                            mapLayer = await QueuedTask.Run(() => LayerFactory.Instance.CreateLayer(new Uri(fileName), groupLayer, index, String.Format("{0} ({1})", legend, baseName)));
                        }
                    }
                    Utils.copyPrj(gv.demFile, fileName);
                } else {
                    //var parms = Geoprocessing.MakeValueArray(fileName, Path.ChangeExtension(gv.demFile, ".prj"));
                    //try { // often fails because filename already locked
                    //    Utils.runPython("runDefineProjection.py", parms, gv);
                    //} catch {; }
                    var uri = new Uri(fileName);
                    var legend2 = String.Format("{0} ({1})", legend, baseName);
                    await QueuedTask.Run(() => {
                        mapLayer = LayerFactory.Instance.CreateLayer(uri, groupLayer, index, legend2);
                    });
                    await FileTypes.ApplySymbolToFeatureLayerAsync((FeatureLayer)mapLayer, ft, gv);
                    await setMapTip((FeatureLayer)mapLayer, ft, gv);
                }
#if TESTING
#else
                var fun = FileTypes.colourFun(ft);
                if (fun is not null) {
                    fun((RasterLayer)mapLayer, gv);
                }
#endif
                //if (!(styleFile is null || styleFile == "")) {
                //    mapLayer.loadNamedStyle(Utils.join(gv.plugin_dir, styleFile));
                //}
                //// save qml form of DEM style file if batch (no support for sld form for rasters)
                //if (gv.isBatch && ft == FileTypes._DEM) {
                //    var qmlFile = Utils.join(gv.projDir, "dem.qml");
                //    (msg, OK) = mapLayer.saveNamedStyle(qmlFile);
                //    if (!OK) {
                //        Utils.error("Failed to create dem.qml: {0}", msg), gv.isBatch);
                //    }
                //}
                return (mapLayer, true);
            }
            else
            {
                return (null, false);
            }
        }

        public static async Task setMapTip(FeatureLayer layer, int ft, GlobalVars gv) {
            bool useOBJECTID = ft == FileTypes._LAKES && gv is not null && (gv.isHUC || gv.isHAWQS);
            var (vari, tip) = FileTypes.mapTip(ft, useOBJECTID);
            if (!string.IsNullOrEmpty(tip)) {
                await setMapTip(layer, vari, tip);
            }
        }

        public static async Task setMapTip(FeatureLayer layer, string vari, string tip) {
            var defn = await QueuedTask.Run(() => layer.GetDefinition()) as CIMBasicFeatureLayer;
            var tabl = defn.FeatureTable;
            tabl.DisplayField = vari;
            var info = new CIMExpressionInfo() {
                Expression = tip
            };
            tabl.DisplayExpressionInfo = info;
            defn.ShowMapTips = true;
            await QueuedTask.Run(() => layer.SetDefinition(defn));
        }

        // Find index of layer in groupLayer, defaulting to 0.

        public static int groupIndex(GroupLayer group, Layer layer)
        {
            var index = 0;
            if (group is null || layer is null)
            {
                return index;
            }
            foreach (var layer2 in ArcSWATPlus.mainMap.Layers.OfType<GroupLayer>())
            {
                if (layer2 == layer)
                {
                    return index;
                }
                index += 1;
            }
            return 0;
        }

        //// Move map layer to start of group unless already in it, and return it.
        //
        //public static object moveLayerToGroup(object layer, string groupName, object root, object gv) {
        //    Utils.loginfo(String.Format("Moving {0} to group {1}", layer.name(), groupName));
        //    var group = root.findGroup(groupName);
        //    if (group is null) {
        //        Utils.information("Internal error: cannot find group {0}.", groupName), gv.isBatch);
        //        return layer;
        //    }
        //    var layerId = layer.id();
        //    // redundant code, but here to make a fast exit from usual situation of already in right group
        //    if (group.findLayer(layerId) is not null) {
        //        Utils.loginfo(String.Format("Found layer in group {}", groupName));
        //        return layer;
        //    }
        //    // find the group the layer is in
        //    var currentGroup = root;
        //    var currentLayer = root.findLayer(layerId);
        //    if (currentLayer is null) {
        //        // not at the top level: check top level groups
        //        currentGroup = null;
        //        foreach (var child in root.children()) {
        //            var node = cast(QgsLayerTreeNode, child);
        //            if (QgsLayerTree.isGroup(node)) {
        //                currentLayer = cast(QgsLayerTreeGroup, node).findLayer(layerId);
        //                if (currentLayer is not null) {
        //                    if (node == group) {
        //                        // already in required group
        //                        return layer;
        //                    } else {
        //                        currentGroup = cast(QgsLayerTreeGroup, node);
        //                        break;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    if (currentGroup is null) {
        //        // failed to find layer
        //        Utils.information($"Trying to move layer {layer.name()} to group {groupName} but failed to find layer", gv.isBatch);
        //        return layer;
        //    }
        //    Utils.loginfo(String.Format("Found layer in group {0}", currentGroup.name()));
        //    // need to move from currentGroup to group
        //    Utils.loginfo(String.Format("Layer to be cloned is {0}", repr(layer)));
        //    var cloneLayer = layer.clone();
        //    Utils.loginfo(String.Format("Cloned map layer is {0}", repr(cloneLayer)));
        //    var movedLayer = group.insertLayer(0, cloneLayer);
        //    currentGroup.removeLayer(layer);
        //    Utils.loginfo(String.Format("Moved tree layer is {0}", repr(movedLayer)));
        //    var newMapLayer = movedLayer.layer();
        //    if (newMapLayer is null) {
        //        return layer;
        //    }
        //    Utils.loginfo(String.Format("Moved map layer is {0}", repr(newMapLayer)));
        //    return newMapLayer;
        //}

        //// Debug function for displaying tree and map laers.
        //
        //public static object printLayers(object root, int n) {
        //    var layers = (from layer in root.findLayers()
        //        select (layer.name(), layer)).ToList();
        //    var mapLayers = (from layer in root.findLayers()
        //        select (layer.layer().name(), layer.layer())).ToList();
        //    Utils.loginfo(String.Format("{0}: layers: {1}", n, repr(layers)));
        //    Utils.loginfo(String.Format("{0}: map layers: {1}", n, repr(mapLayers)));
        //}

        public static string runPython(string script, IReadOnlyList<string> parms, GlobalVars gv) {
            string pathProExe = null;
            // python environment with pyqt added no longer used: too big
            // need Python envirmnment with PyQt included.  Try local copy first
            //string pathProExeArcSWAT = System.IO.Path.Combine(Parameters._HOMEDIR, @"SWATPlus\ArcGIS\arcgispro-py3-ArcSWAT");
            //if (!Directory.Exists(pathProExeArcSWAT)) {
            //    pathProExe = System.IO.Path.GetDirectoryName(new System.Uri(Assembly.GetEntryAssembly().Location).AbsolutePath);
            //    if (pathProExe == null) return "";
            //    pathProExe = Uri.UnescapeDataString(pathProExe);
            //    pathProExeArcSWAT = System.IO.Path.Combine(pathProExe, @"Python\envs\arcgispro-py3-ArcSWAT");
            //}
            //if (!Directory.Exists(pathProExeArcSWAT)) {
            //    Utils.loginfo("ArcSWAT python environment does not seem to be installed.  Using default, which will not work for some scripts, including visualisation plotting.", EventLog.EventType.Warning);
            //    pathProExeArcSWAT = System.IO.Path.Combine(pathProExe, @"Python\envs\arcgispro-py3");
            //}
#if TESTING
            pathProExe = @"C:\Program Files\ArcGIS\Pro\bin";
#else
            pathProExe = System.IO.Path.GetDirectoryName(new System.Uri(Assembly.GetEntryAssembly().Location).AbsolutePath);
            if (pathProExe == null) return "";
            pathProExe = Uri.UnescapeDataString(pathProExe);
#endif
            string pathProExeArcSWAT = System.IO.Path.Combine(pathProExe, @"Python\envs\arcgispro-py3");
            // Create and format process command string.
            // Create process start info, with instruction settings
            var procStartInfo = new System.Diagnostics.ProcessStartInfo("cmd.exe") { 
                ArgumentList = {
                    "/c",
                    System.IO.Path.Combine(pathProExeArcSWAT, "python.exe"),
                    System.IO.Path.Combine(gv.addinPath, script)
                }
            };
            foreach (string s in parms) {
                procStartInfo.ArgumentList.Add(s);
            }
            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.RedirectStandardError = true;
            procStartInfo.UseShellExecute = false;
            procStartInfo.CreateNoWindow = true;
            // Create process and start it
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo = procStartInfo;
            string result;
            using (new CursorWait())
            {
                proc.Start();
                // Create and format result string
                result = proc.StandardOutput.ReadToEnd();
                string error = proc.StandardError.ReadToEnd();
                if (!string.IsNullOrEmpty(error)) {
                    result += String.Format(" Error: {0}", error);
                    Utils.error(result, gv.isBatch);
                } else {
                    Utils.loginfo(result);
                }
            }
            return result;
        }

        //Use dialog to open file of FileType ft chosen by user(or get from box if is batch, intended for testing), 
        //         add a layer for it if necessary, 
        //         clip if clipToDEM is true and substantially larger than DEM,
        //         run geometry fix if runFix is true,
        //         copy files to saveDir, write path to box,
        //         and return file path and layer.

        public static async Task<(string, Layer)> openAndLoadFile(
            int ft,
#if TESTING
            mimicTextBox box,
#else
            TextBox box,
#endif
            string saveDir,
            GlobalVars gv,
            Layer subLayer,
            string groupName,
            bool clipToDEM = false,
            bool runFix = false)
        {
            string inFileName = "";
            string outFileName;
            string itemName = "";
            string path = "";
            bool isGdbItem = false;
            //var settings = QSettings();
            //if (settings.contains("/QSWATPlus/LastInputPath"))
            //{
            //    path = settings.value("/QSWATPlus/LastInputPath").ToString();
            //}
            //else
            //{
            //    path = "";
            //}
            var title = Utils.trans(FileTypes.title(ft));
            if (gv.isBatch) {
                // filename in box
                inFileName = box.Text;
            }
            else
            {
                OpenItemDialog dlg = new OpenItemDialog() {
                    InitialLocation = path,
                    Title = title,
                    Filter = FileTypes.filter(ft),
                    AlwaysUseInitialLocation = false,
                    MultiSelect = false
                };
                bool? ok = dlg.ShowDialog();
                if (ok == true) {
                    await QueuedTask.Run(() => {
                        Item item = dlg.Items.First();
                        itemName = Path.ChangeExtension(item.Name, null);
                        inFileName = item.Path;
                        var typeID = item.TypeID;
                        isGdbItem = typeID.StartsWith("fgdb_") ||
                            typeID.EndsWith("_fgdb") ||
                            typeID.StartsWith("egdb_") ||
                            typeID.EndsWith("_egdb");
                    });
                }

                //using (OpenFileDialog dlg = new OpenFileDialog()) {
                //    dlg.InitialDirectory = path;
                //    dlg.Title = title;
                //    dlg.Filter = FileTypes.filter(ft);
                //    dlg.RestoreDirectory = false;
                //    if (dlg.ShowDialog() == DialogResult.OK) {
                //        inFileName = dlg.FileName;
                //    }
                //}
            }
            //Utils.information('File is |{0}|', inFileName), False)
            if (inFileName is not null && inFileName != "")
            {
                //settings.Value = "/QSWATPlus/LastInputPath", os.path.dirname(inFileName.ToString()));
                // copy to saveDir if necessary
                bool isESRIGrid = false;
                string inDir;
                string outFileBase;
                var outDir = saveDir;
                // check for ESRI rasters
                if (!isGdbItem & FileTypes.isRaster(ft)) {
                    if (Directory.Exists(inFileName)) {
                        isESRIGrid = true;
                        var inInfo = new DirectoryInfo(inFileName);
                        outFileBase = inInfo.Name;
                    } else {
                var inInfo = new FileInfo(inFileName);
                    var inFile = inInfo.Name;
                        outFileBase = inInfo.Directory.Name;
                        isESRIGrid = inFile == "sta.adf" || inFile == "hdr.adf";
                    }
                } else {
                    if (isGdbItem) {
                        outFileBase = itemName;
                    } else {
                        outFileBase = Path.GetFileNameWithoutExtension(inFileName);
                    }
                }
                if (isESRIGrid || isGdbItem) {
                    if (FileTypes.isRaster(ft)) {
                            // will be converted to .tif, so make a .tif name
                        outFileName = Utils.join(saveDir, outFileBase) + ".tif";
                            await Utils.removeLayerByLegend(FileTypes.legend(ft));
                            Utils.removeFiles(outFileName);
                        var parms = Geoprocessing.MakeValueArray(inFileName, saveDir);
                        Utils.runPython("runConversion.py", parms, gv);
                    } else {
                        // must be gdb feature class.  Save as shapefile
                        outFileName = Utils.join(saveDir, outFileBase) + ".shp";
                        await Utils.removeLayerByLegend(FileTypes.legend(ft));
                        Utils.removeFiles(outFileName);
                        var parms = Geoprocessing.MakeValueArray(Path.GetDirectoryName(inFileName), itemName, saveDir);
                        Utils.runPython("runFeaturesToShapefile.py", parms, gv);
                        }
                    // may not have got projection information, so if any copy it
                    Utils.copyPrj(inFileName, outFileName);
                } else { // we are just dealing with files
                    var inInfo = new FileInfo(inFileName);
                    inDir = inInfo.DirectoryName;
                    var inFile = inInfo.Name;
                    if (inDir != outDir) {
                        outFileName = Utils.join(saveDir, inFile);
                        if (ft == FileTypes._DEM) {
                            // will be converted to .tif, so convert to .tif name
                            outFileName = Path.ChangeExtension(outFileName, ".tif");
                        }
                    // remove any existing layer for this file, else cannot copy to it
                    await Utils.removeLayerByLegend(FileTypes.legend(ft));
                        if (ft == FileTypes._DEM && Path.GetExtension(inFileName) != ".tif") {
                        Utils.removeFiles(outFileName);
                    }
                    //else if (runFix)
                    //{
                    //    Utils.fixGeometry(inFileName, saveDir);
                    //}
                    //else {
                    Utils.copyFiles(inInfo, saveDir);
                    //}
                    } else {
                    // ignore runFix: assume already fixed as inside project
                    outFileName = inFileName;
                	}
                }
                if (ft == FileTypes._CSV || ft == FileTypes._CHANNELBASINSRASTER || ft == FileTypes._AQUIFERTHICKNESS) {
                    // not to be loaded into ArcGIS
                    return (outFileName, null);
                }
                // this function will add layer if necessary
                Layer layer = null;
                bool ok;
                (layer, ok) = await Utils.getLayerByFilename(outFileName, ft, gv, subLayer, groupName, clipToDEM);
                if (layer is null)
                {
                    return (null, null);
                }
                // layer name will have changed if clipped
                outFileName = await Utils.layerFilename(layer);

                if (!(box is null))
                {
                    box.Text = outFileName;
                }
                // if no .prj file, try to create one
                // this is needed, for example, when DEM is created from ESRI grid
                // or if DEM is made by clipping
                await Utils.writePrj(outFileName, layer);
                //// check projection is same as project's
                var crsProject = gv.topo.crsProject;
                if (crsProject is not null) {
                    // it is None before DEM is loaded
                    var crsLayer = await QueuedTask.Run(() => layer.GetSpatialReference());
                    if (!SpatialReference.AreEqual(crsProject, crsLayer)) {
                        Utils.information(String.Format("WARNING: File {0} has a projection {1} (WKID {3}) which is different from the project projection {2} (WKID {4}).  You may need to reproject and reload.", outFileName, crsLayer.Name, crsProject.Name, crsLayer.LatestWkid, crsProject.LatestWkid), gv.isBatch);
                        //QgsProject.instance().removeMapLayer(layer.id())
                        //del layer
                        //gv.iface.mapCanvas().refresh()
                        //return (None, None)
                    }
                }
                return (outFileName, layer);
            }
            else
            {
                return (null, null);
            }
        }

        //// Fix geometries in shapefile.  Assumes saveDir is not the folder of inFile.
        //
        //public static object fixGeometry(string inFile, string saveDir) {
        //    var filename = os.path.split(inFile)[1];
        //    var outFile = Utils.join(saveDir, filename);
        //    var @params = new Dictionary<object, object> {
        //        {
        //            "INPUT",
        //            inFile},
        //        {
        //            "OUTPUT",
        //            outFile}};
        //    processing.run("native:fixgeometries", @params);
        //}

        // Combines two polygon or multipolygon geometries by simply appending one list to the other.
        //         
        //         Not ideal, as polygons may abut and this leaves a line between them, 
        //         but useful when QgsGeometry.combine fails.
        //         Assumes both geomtries are polygon or multipolygon type
        //
        //public static object polyCombine(object geom1, object geom2) {
        //    object list2;
        //    object list1;
        //    if (geom1.wkbType() == QgsWkbTypes.Polygon) {
        //        list1 = new List<object> {
        //            geom1.asPolygon()
        //        };
        //    } else {
        //        list1 = geom1.asMultiPolygon();
        //    }
        //    if (geom2.wkbType() == QgsWkbTypes.Polygon) {
        //        list2 = new List<object> {
        //            geom2.asPolygon()
        //        };
        //    } else {
        //        list2 = geom2.asMultiPolygon();
        //    }
        //    return QgsGeometry.fromMultiPolygonXY(list1 + list2);
        //}

        // Return feature in OGR layer whose attribute with field name has value val.

        public static OSGeo.OGR.Feature getOgrFeatureByValue(OSGeo.OGR.Layer layer, string name, object val)
        {
            if (val is String) {
                val = string.Format("'{0}'", val);
            }
            layer.SetAttributeFilter(string.Format("{0}={1}", name, val));
            layer.ResetReading();
            var feature = layer.GetNextFeature();
            layer.SetAttributeFilter(null);
            return feature;
        }

        public static async Task<Feature> getFeatureByValue(FeatureLayer layer, string name, object val) {
            var qf = new QueryFilter() {
                WhereClause = (val is String) ? string.Format("{0} = '{1}'", name, val) : string.Format("{0} = {1}", name, val)
            };
            return await QueuedTask.Run<Feature>(() => {
                using (var rc = layer.Search(qf)) {
                    rc.MoveNext();
                    return rc.Current as Feature;
                }
            });
        }

        // copy all point features from inFile to outFile
        public static void copyPointFeatures(string inFile, string outFile) {
            var inDs = OSGeo.OGR.Ogr.Open(inFile, 0);
            var outDs = OSGeo.OGR.Ogr.Open(outFile, 1);
            var inLayer = inDs.GetLayerByIndex(0);
            var outLayer = outDs.GetLayerByIndex(0);
            var inDef = inLayer.GetLayerDefn();
            var inIdIndex = inDef.GetFieldIndex("ID");
            var inResIndex = inDef.GetFieldIndex("RES");
            var inInletIndex = inDef.GetFieldIndex("INLET");
            var inPtsourceIndex = inDef.GetFieldIndex("PTSOURCE");
            var outDef = outLayer.GetLayerDefn();
            var outIdIndex = outDef.GetFieldIndex("ID");
            var outResIndex = outDef.GetFieldIndex("RES");
            var outInletIndex = outDef.GetFieldIndex("INLET");
            var outPtsourceIndex = outDef.GetFieldIndex("PTSOURCE");
            inLayer.ResetReading();
            OSGeo.OGR.Feature f = null; 
            do {
                f = inLayer.GetNextFeature();
                if (f != null) {
                    var outF = new OSGeo.OGR.Feature(outDef);
                    var id = f.GetFieldAsInteger(inIdIndex);
                    outF.SetField(outIdIndex, id);
                    var res = f.GetFieldAsInteger(inResIndex);
                    outF.SetField(outResIndex, res);
                    outF.SetField(outInletIndex, f.GetFieldAsInteger(inInletIndex));
                    outF.SetField(outPtsourceIndex, f.GetFieldAsInteger(inPtsourceIndex));
                    var geom = f.GetGeometryRef();
                    double[] pt = new double[3];
                    geom.GetPoint(0, pt);
                    outF.SetGeometry(geom); 
                    outLayer.CreateFeature(outF);
                }
            } while (f != null);
        }

        // If no .prj file exists for fileName, try to create one from the layer's crs.

        public static async Task writePrj(string fileName, Layer layer)
        {
            var prjFile = Path.ChangeExtension(fileName, ".prj");
            if (File.Exists(prjFile))
            {
                return;
            }
            try
            {
                SpatialReference srs = null;
                srs = await QueuedTask.Run< SpatialReference>(() => { 
                    var srs1 = layer.GetSpatialReference();
                    return srs1;
                });
                // if layer not available srs can be null
                if (srs is null) { return; }
                var wkt = srs.Wkt;
                if (wkt is null)
                {
                    throw new Exception("Could not make WKT from CRS.");
                }
                using (FileStream log = new FileStream(prjFile, FileMode.Append, FileAccess.Write))
                using (StreamWriter sw = new StreamWriter(log))
                {
                    sw.WriteLine(wkt);
                }
            }
            catch (Exception)
            {
                Utils.information(@"Unable to make .prj file for {fileName}.  
            You may need to set this map's projection manually", false);
            }
        }

        // Make temporary QSWAT folder and return its absolute path.

        public static string tempFolder(bool create = true)
        {
            var tempDir = Path.GetTempPath() + "SWAT";
            if (create && !Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }
            return tempDir;
        }

        // Make a new temporary file in tempFolder with suffix.

        public static string tempFile(string suffix)
        {
            DateTime date_time = DateTime.Now;
            var @base = "tmp" + date_time.Millisecond.ToString();
            var folder = Utils.tempFolder(true);
            var fil = Utils.join(folder, @base + suffix);
            if (File.Exists(fil))
            {
                Thread.Sleep(1000);
                return Utils.tempFile(suffix);
            }
            return fil;
        }

        // Delete the temporary folder and its contents.

        public static void deleteTempFolder()
        {
            var folder = Utils.tempFolder(create: false);
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
            }
        }

        // Add string to combo box if not already present, and make it current.

        public static void makeCurrent(string strng, ComboBox combo)
        {
            int index = combo.FindString(strng);
            if (index < 0)
            {
                combo.Items.Add(strng);
                combo.SelectedIndex = combo.Items.Count - 1;
            }
            else
            {
                combo.SelectedIndex = index;
            }
        }

        // 
        //         Parse a slope limits string to list of intermediate limits.
        //         For example '[min, a, b, max]' would be returned as [a, b].
        //         

        public static List<double> parseSlopes(string strng)
        {
            // remove [ and ]
            strng = strng.Trim();
            strng = strng.Substring(1, strng.Length - 2);
            var slopeLimits = new List<double>();
            var nums = strng.Split(",");
            // ignore first and last
            foreach (var i in Enumerable.Range(1, nums.Count() - 2))
            {
                slopeLimits.Add(Double.Parse(nums[i]));
            }
            return slopeLimits;
        }

        // 
        //         Return a slope limits string made from a string of intermediate limits.
        //         For example [a, b] would be returned as '[0, a, b, 9999]'.
        //         

        public static string slopesToString(List<double> slopeLimits)
        {
            var str1 = "[0, ";
            foreach (var i in slopeLimits)
            {
                // lose the decimal point if possible
                var d = Convert.ToInt32(i);
                if (i == d)
                {
                    str1 += String.Format("{0}, ", d);
                }
                else
                {
                    str1 += String.Format("{0}, ", i);
                }
            }
            return str1 + "9999]";
        }

        // Return true if point is within (assumed rectangular) cell.
        // Assume cell is a polygon with just one outer rectangular ring with five points, so points 0 and 2 are opposite corners.

        public static Task<bool> pointInGridCell(double[] point, Feature cell)
        {
            bool inX;
            return QueuedTask.Run(() => {
                var poly = cell.GetShape() as ArcGIS.Core.Geometry.Polygon;
                ReadOnlyPointCollection points = poly.Points;
                var corner1 = points[0];
                var corner2 = points[2];
                var x1 = corner1.X;
                var x2 = corner2.X;
                var y1 = corner1.Y;
                var y2 = corner2.Y;
                if (x1 < x2) {
                    var _tmp_1 = point[0];
                    inX = x1 <= _tmp_1 && _tmp_1 <= x2;
                } else {
                    var _tmp_2 = point[0];
                    inX = x2 <= _tmp_2 && _tmp_2 <= x1;
                }
                if (inX) {
                    if (y1 < y2) {
                        var _tmp_3 = point[1];
                        return y1 <= _tmp_3 && _tmp_3 <= y2;
                    } else {
                        var _tmp_4 = point[1];
                        return y2 <= _tmp_4 && _tmp_4 <= y1;
                    }
                } else {
                    return false;
                }
            });
        }

        // Return centre point of (assumed rectangular) cell.
        //         plus extent as minimum and maximum x and y values.
        // Assume cell is a polygon with just one outer rectangular ring with five points, so points 0 and 2 are opposite corners.

        public static (double[], (double, double), (double, double)) centreGridCell(OSGeo.OGR.Feature cell)
        {
            var poly = cell.GetGeometryRef();
            // need to get subgeometry, a linestring, in order to get points
            var perim = poly.GetGeometryRef(0);
            double[] corner1 = new double[3];
            perim.GetPoint(0, corner1);
            double[] corner2 = new double[3];
            perim.GetPoint(2, corner2);
            var x1 = corner1[0];
            var x2 = corner2[0];
            var y1 = corner1[1];
            var y2 = corner2[1];
            return (new double[3] { (x1 + x2) / 2.0, (y1 + y2) / 2.0, 0 },
                    x1 <= x2 ? (x1, x2) : (x2, x1), y1 <= y2 ? (y1, y2) : (y2, y1));
        }

        // Return centre point of (assumed rectangular) cell, 
        //         plus extent as minimum and maximum x and y values.
        // Assume cell is a polygon with just one outer rectangular ring with five points, so points 0 and 2 are opposite corners.

        public static (double[], (double, double), (double, double)) centreGridCell(Feature cell) {
            var poly = cell.GetShape() as ArcGIS.Core.Geometry.Polygon;
            var points = poly.Points;
            var corner1 = points[0];
            var corner2 = points[2];
            var x1 = corner1.X;
            var x2 = corner2.X;
            var y1 = corner1.Y;
            var y2 = corner2.Y;
            return (new double[3] { (x1 + x2) / 2.0, (y1 + y2) / 2.0, 0 },
                    x1 <= x2 ? (x1, x2) : (x2, x1), y1 <= y2 ? (y1, y2) : (y2, y1));
        }

        // Retun today's date as day month year.

        public static string date()
        {
            return DateTime.Now.ToString(Utils._DATEFORMAT);
        }

        // Return the time now as hours.minutes.

        public static string time()
        {
            return DateTime.Now.ToString("HH:mm");
        }

        // 
        //         Return the string used to name SWAT input files 
        //         from basin and relative HRU number.
        //         

        public static string fileBase(int SWATBasin, int relhru, bool forTNC = false)
        {
            if (forTNC)
            {
                return String.Format("{0:D7}{1:D2}", SWATBasin, relhru);
            }
            return String.Format("{0:D5}{1:D4}", SWATBasin, relhru);
        }

        // Estimate the average slope length in metres from the mean slope.

        public static int getSlsubbsn(double meanSlope)
        {
            if (meanSlope < 0.01)
            {
                return 120;
            }
            else if (meanSlope < 0.02)
            {
                return 100;
            }
            else if (meanSlope < 0.03)
            {
                return 90;
            }
            else if (meanSlope < 0.05)
            {
                return 60;
            }
            else
            {
                return 30;
            }
        }

        // 
        //         In xmlFile, sets the value of node tag with attName equal to attVal to tagVal.
        //         
        //         Return true and empty string if ok, else false and an error string.
        //         
        //
        //public static object setXMLValue(
        //    string xmlFile,
        //    string tag,
        //    string attName,
        //    string attVal,
        //    string tagVal) {
        //    var doc = QDomDocument();
        //    var f = QFile(xmlFile);
        //    var done = false;
        //    if (f.open(QIODevice.ReadWrite)) {
        //        if (doc.setContent(f)) {
        //            var tagNodes = doc.elementsByTagName(tag);
        //            foreach (var i in Enumerable.Range(0, tagNodes.length())) {
        //                var tagNode = tagNodes.item(i);
        //                var atts = tagNode.attributes();
        //                var key = atts.namedItem(attName);
        //                if (key is not null) {
        //                    var att = key.toAttr();
        //                    var val = att.Value;
        //                    if (val == attVal) {
        //                        var textNode = tagNode.firstChild().toText();
        //                        textNode.setNodeValue(tagVal);
        //                        var newVal = tagNode.firstChild().toText().nodeValue();
        //                        if (newVal != tagVal) {
        //                            return (false, "found new XML value of {0} instead of {1}", newVal, val));
        //                        }
        //                        done = true;
        //                        break;
        //                    }
        //                }
        //            }
        //            if (!done) {
        //                return (false, "Failed to find {0} node with {1}={2} in {3}", tag, attName, attVal, xmlFile));
        //            }
        //        } else {
        //            return (false, "Failed to read XML file {0}", xmlFile));
        //        }
        //    } else {
        //        return (false, "Failed to open XML file {0}", xmlFile));
        //    }
        //    f.resize(0);
        //    var strm = QTextStream(f);
        //    doc.save(strm, 4);
        //    f.close();
        //    return (true, "");
        //}

        // Return point in points nearest to point, or point itself if points is empty.

        public static double[] nearestPoint(double[] point, List<double[]> points)
        {
            if (points is null || points.Count == 0)
            {
                return point;
            }
            if (points.Count == 1)
            {
                return points[0];
            }
            var minm = double.PositiveInfinity;
            var nearest = point;
            var pointx = point[0];
            var pointy = point[1];
            foreach (var pt in points)
            {
                var dx = pt[0] - pointx;
                var dy = pt[1] - pointy;
                var measure = dx * dx + dy * dy;
                if (measure < minm)
                {
                    minm = measure;
                    nearest = pt;
                }
            }
            return nearest;
        }

        // Return point in points nearest to point, or point itself if points is empty.

        public static double[] nearestPoint(double[] point, List<MapPoint> points) {
            if (points is null || points.Count == 0) {
                return point;
            }
            if (points.Count == 1) {
                return new double[3] { points[0].X, points[0].Y, 0 };
            }
            var minm = double.PositiveInfinity;
            var nearest = point;
            var pointx = point[0];
            var pointy = point[1];
            foreach (var pt in points) {
                var dx = pt.X - pointx;
                var dy = pt.Y - pointy;
                var measure = dx * dx + dy * dy;
                if (measure < minm) {
                    minm = measure;
                    nearest = new double[3] { pt.X, pt.Y, 0 };
                }
            }
            return nearest;
        }


        // Calculate landscape unit id from SWAT channel and landscape.
        public static int landscapeUnitId(int SWATChannel, int landscape) {
            // SWATChannel is alreay unique across the watershed
            // Currently landscape ia zero (no landscape division)
            // else 1 (floodplain) or 2 (upslope)
            // This method allows for further landscape divisions in future, e.g.left/right
            // If this method is changed landscapeIdIsUpslope needs to be changed
            return SWATChannel * 10 + landscape;
        }

        // Return true if landscape id is for an upslope landscape.
        public static bool landscapeUnitIdIsUpslope(int lsuId) {
            // note this depends on the encoding used in landscapeUnitId and landscapeName
            var lscape = lsuId % 10;
            return lscape != 0 && lscape % 2 == 0;
        }

        // Return landscape unit name.
        public static string landscapeName(int lscape, bool useLeftRight, bool notEmpty = false) {
            string result = null;
            if (lscape == Utils._NOLANDSCAPE) {
                result = notEmpty ? "NA" : "";
            } else if (lscape == Utils._FLOODPLAIN) {
                result = useLeftRight ? "Left floodplain" : "Floodplain";
            } else if (lscape == Utils._UPSLOPE) {
                result = useLeftRight ? "Left upslope" : "Upslope";
            } else if (lscape == 3) {
                result = "Right floodplain";
            } else if (lscape == 4) {
                result = "Right upslope";
            } else if (lscape == 5) {
                result = "Head floodplain";
            } else if (lscape == 6) {
                result = "Head upslope";
            } else {
                throw new Exception(string.Format("Unknown landscape unit {0}", lscape));
            }
            //return result + ' ({0})', Utils.landscapeAbbreviation(lscape, useLeftRight))
            return result;
        }

        // Return landscape code
        public static int landscapeFromName(string name) {
            if (name is null || name == "" || name == "NA") {
                return Utils._NOLANDSCAPE;
            } else if (name == "Floodplain") {
                return Utils._FLOODPLAIN;
            } else if (name == "Upslope") {
                return Utils._UPSLOPE;
            } else {
                throw new Exception(string.Format("Unknown landscape name {0}", name));
            }
        }

        // currently not used    
        // Return landscape unit abbreviated name.
        public static string landscapeAbbreviation(int lsu, bool useLeftRight) {
            if (lsu == Utils._NOLANDSCAPE) {
                return "";
            } else if (lsu == Utils._FLOODPLAIN) {
                return useLeftRight ? "LFlood" : "Flood";
            } else if (lsu == Utils._UPSLOPE) {
                return useLeftRight ? "LUp" : "Up";
            } else if (lsu == 3) {
                return "RFlood";
            } else if (lsu == 4) {
                return "RUp";
            } else if (lsu == 5) {
                return "HFlood";
            } else {
                return "HUp";
            }
        }

        // TODO
        // not clear how to do this in GDAL
        //// Combines two polygon or multipolygon geometries by simply appending one list to the other.
        ////         
        ////         Not ideal, as polygons may abut and this leaves a line between them, 
        ////         but usful when sGeometry.union fails.
        ////         Assumes both geomtries are polygon or multipolygon type
        //public static object polyCombine(OSGeo.OGR.Geometry geom1, OSGeo.OGR.Geometry geom2) {
        //    object list2;
        //    object list1;
        //    if (geom1.GetGeometryType() == OSGeo.OGR.wkbGeometryType.wkbPolygon) {
        //        list1 = new List<OSGeo.OGR.Geometry> {
        //                geom1.Get
        //            };
        //    } else {
        //        list1 = geom1.asMultiPolygon();
        //    }
        //    if (geom2.wkbType() == QgsWkbTypes.Polygon) {
        //        list2 = new List<object> {
        //                geom2.asPolygon()
        //            };
        //    } else {
        //        list2 = geom2.asMultiPolygon();
        //    }
        //    return QgsGeometry.fromMultiPolygonXY(list1 + list2);
        //}

        public static bool areClose(double v1, double v2, double rel_tol = 1E-06) {
            // return true if numbers less than rel_tol apart
            return Math.Abs(v1 - v2) < rel_tol;
        }

        public static void CopyAll(DirectoryInfo source, DirectoryInfo target) {
            if (source.FullName.ToLower() == target.FullName.ToLower()) {
                return;
            }

            // Check if the target directory exists, if not, create it.
            if (Directory.Exists(target.FullName) == false) {
                Directory.CreateDirectory(target.FullName);
            }

            // Copy each file into it's new directory.
            foreach (FileInfo fi in source.GetFiles()) {
                fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories()) {
                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }
    }

    // Inserted list function.
    public class ListFuns
    {
        // int list to string
        public static string listToString(List<int> l) {
            string Result = "[";
            for (int i = 0; i < l.Count; i++) {
                if (i > 0) { Result += ", "; }
                Result += l[i].ToString();
            }
            Result += "]";
            return Result;
        }

        // 
        //         Insert val into assumed sorted list vals.  
        //         If unique is true and val already in vals do nothing.
        //         Return true if insertion made.
        //          

        public static bool insertIntoSortedList(double val, List<double> vals, bool unique)
        {
            foreach (var index in Enumerable.Range(0, vals.Count))
            {
                var nxt = vals[index];
                if (nxt == val)
                {
                    if (unique)
                    {
                        return false;
                    }
                    else
                    {
                        vals.Insert(index, val);
                        return true;
                    }
                }
                if (nxt > val)
                {
                    vals.Insert(index, val);
                    return true;
                }
            }
            vals.Add(val);
            return true;
        }

        public static bool insertIntoSortedIntList(int val, List<int> vals, bool unique) {
            foreach (var index in Enumerable.Range(0, vals.Count)) {
                var nxt = vals[index];
                if (nxt == val) {
                    if (unique) {
                        return false;
                    } else {
                        vals.Insert(index, val);
                        return true;
                    }
                }
                if (nxt > val) {
                    vals.Insert(index, val);
                    return true;
                }
            }
            vals.Add(val);
            return true;
        }

        public static bool insertIntoSortedLongList(long val, List<long> vals, bool unique) {
            foreach (var index in Enumerable.Range(0, vals.Count)) {
                var nxt = vals[index];
                if (nxt == val) {
                    if (unique) {
                        return false;
                    } else {
                        vals.Insert(index, val);
                        return true;
                    }
                }
                if (nxt > val) {
                    vals.Insert(index, val);
                    return true;
                }
            }
            vals.Add(val);
            return true;
        }

        public static bool insertIntoSortedStringList(string val, List<string> vals, bool unique) {
            foreach (var index in Enumerable.Range(0, vals.Count)) {
                var nxt = vals[index];
                if (nxt == val) {
                    if (unique) {
                        return false;
                    } else {
                        vals.Insert(index, val);
                        return true;
                    }
                }
                var comparison = String.Compare(nxt, val, comparisonType: StringComparison.OrdinalIgnoreCase);
                if (comparison < 0) {  // (nxt > val) 
                    vals.Insert(index, val);
                    return true;
                }
            }
            vals.Add(val);
            return true;
        }
    }

    public static class MapFuns<T> {

        public static Dictionary<T, T> flattenMap(Dictionary<T, T> mmap) {
            // Map each domain element to its final target.
            // If for example we have [A->B, B->C] then return [A->C, B->C].

            var result = new Dictionary<T, T>();
            foreach (var (a, b) in mmap) {
                result[a] = MapFuns<T>.transApply(b, mmap);
            }
            return result;
        }

        public static T transApply(T b, Dictionary<T, T> mmap) {
            // b is a range element.  If it is not a domain element, return it.
            // Else continue with mmap[b] as the next range element.

            //mmap is assumed not to be circular.
            T nxt;
            if (mmap.TryGetValue(b, out nxt)) {
                return transApply(nxt, mmap);
            } else {
                return b;
            }
        }
    }

    //// 
    ////     Class effectively extending writer
    ////     to support changing the end-of-line character for Windows/Linux.
    ////     Also supplies a WriteLine method
    ////     
    //public class fileWriter {

    //    public Action<object> close;

    //    public Action<object> write;

    //    public file writer;

    //    public object _END_LINE = os.linesep;

    //    public fileWriter(string path) {
    //        //# writer
    //        this.writer = open(path, "w");
    //        //# write method
    //        this.Write = this.writer.Write;
    //        //# close
    //        this.close = this.writer.close;
    //    }

    //    // Write string plus end-of-line.
    //    public virtual void WriteLine(string @string) {
    //        this.writer.Write(@string + "\n");
    //    }

    //    // Return self.
    //    public virtual fileWriter @__enter__() {
    //        // type: ignore
    //        return this;
    //    }

    //    // Close.
    //    public virtual void @__exit__(object typ, object value, object traceback) {
    //        // @UnusedVariable
    //        this.writer.close();
    //    }
    //}

    // File types for various kinds of file that will be loaded, 
    //     and utility functions.
    //     
    public static class FileTypes
    {

        public static int _DEM = 0;

        public static int _MASK = 1;

        public static int _BURN = 2;

        public static int _OUTLETS = 3;

        public static int _STREAMS = 4;

        public static int _SUBBASINS = 5;

        public static int _LANDUSES = 6;

        public static int _SOILS = 7;

        public static int _SLOPEBANDS = 8;

        public static int _CHANNELREACHES = 9;

        public static int _WATERSHED = 10;

        public static int _EXISTINGSUBBASINS = 11;

        public static int _EXISTINGWATERSHED = 12;

        public static int _HILLSHADE = 13;

        public static int _GRID = 14;

        public static int _GRIDSTREAMS = 15;
        public static int _DRAINSTREAMS = 16;
        public static int _BUFFERSHAPE = 17;
        public static int _BUFFERFLOOD = 18;
        public static int _INVFLOOD = 19;
        public static int _BRANCHFLOOD = 20;
        public static int _CSV = 21;
        public static int _SQLITE = 22;
        public static int _CHANNELS = 23;
        public static int _CHANNELBASINSRASTER = 24;
        public static int _LSUS = 25;
        public static int _STREAMREACHES = 26;
        public static int _LAKES = 27;
        public static int _EXTRAPTSRCANDRES = 28;
        public static int _HRUS = 29;
        public static int _AQUIFERS = 30;
        public static int _AQUIFERTHICKNESS = 31;
        public static int _PERMEABILITY = 32;
        public static int _INITIALIZATION = 33;
        public static int _OBSERVATIONLOCATIONS = 34;
        public static int _TILEDRAINS = 35;
        public static int _OUTPUTTIMES = 36;
        public static int _OUTLETSHUC = 37;
        public static int _ACTHRUS = 38;
        public static int _ACTLSUS = 39;
        public static int _GWFLOWCELLS = 40;
        public static int _OTHER = 99;

        // Return filter for open file dialog according to file type.

        public static string filter(int ft) {
            string filtr = "";
            if (isRaster(ft)) {
                filtr = "esri_browseDialogFilters_rasters";
            } else {
                if (ft == FileTypes._OUTLETS || ft == FileTypes._OUTLETSHUC || ft == FileTypes._EXTRAPTSRCANDRES ||
                    ft == FileTypes._OBSERVATIONLOCATIONS) {
                    filtr = "esri_browseDialogFilters_featureClasses_point";
                } else if (ft == FileTypes._STREAMS || ft == FileTypes._STREAMREACHES || ft == FileTypes._BURN ||
                     ft == FileTypes._GRIDSTREAMS || ft == FileTypes._DRAINSTREAMS || ft == FileTypes._CHANNELS) {
                    filtr = "esri_browseDialogFilters_featureClasses_line";
                } else if (ft == FileTypes._SUBBASINS || ft == FileTypes._EXISTINGSUBBASINS || ft == FileTypes._AQUIFERS ||
                        ft == FileTypes._WATERSHED || ft == FileTypes._EXISTINGWATERSHED || ft == FileTypes._GRID ||
                        ft == FileTypes._LSUS || ft == FileTypes._LAKES || ft == FileTypes._BUFFERSHAPE ||
                        ft == FileTypes._HRUS || ft == FileTypes._ACTHRUS || ft == FileTypes._ACTLSUS ||
                        ft == FileTypes._PERMEABILITY || ft == FileTypes._TILEDRAINS || ft == FileTypes._GWFLOWCELLS) {
                    filtr = "esri_browseDialogFilters_featureClasses_polygon";
                } else if (ft == FileTypes._CSV) {
                    filtr = "CSV files|*.csv|All files|*.*";
                } else if (ft == FileTypes._SQLITE) {
                    filtr = "SQLite files|*.sqlite; *.db; *.sdb; *.db3; *.s3db; *.sqlite3; *.sl3|All files|*.*";
                } else if (ft == FileTypes._INITIALIZATION) {
                    filtr = "Initialization files|*.ini; *.cfg; *.conf; *.config|All files|*.*";
                } else if (ft == FileTypes._OUTPUTTIMES) {
                    filtr = "Text files|*.txt|CSV files|*.csv|Excel files|*.xls; *.xlsx|All files|*.*";
                }
            }
            return filtr;
        }

        //public static string filter(int ft) {
        //    if (ft == FileTypes._DEM || ft == FileTypes._LANDUSES || ft == FileTypes._SOILS || ft == FileTypes._HILLSHADE ||
        //        ft == FileTypes._BUFFERFLOOD || ft == FileTypes._INVFLOOD || ft == FileTypes._BRANCHFLOOD || ft == FileTypes._CHANNELBASINSRASTER ||
        //        ft == FileTypes._AQUIFERTHICKNESS) {
        //        return "All files|*.*"; // "All files (*)|*|All supported files (*.vrt *.VRT *.ovr *.OVR *.tif *.TIF *.tiff *.TIFF *.ntf *.NTF *.toc *.TOC *.xml *.XML *.img *.IMG *.gff *.GFF *.asc *.ASC *.isg *.ISG *.ddf *.DDF *.dt0 *.DT0 *.dt1 *.DT1 *.dt2 *.DT2 *.png *.PNG *.jpg *.JPG *.jpeg *.JPEG *.mem *.MEM *.gif *.GIF *.n1 *.N1 *.kap *.KAP *.xpm *.XPM *.bmp *.BMP *.pix *.PIX *.map *.MAP *.mpr *.MPR *.mpl *.MPL *.rgb *.RGB *.hgt *.HGT *.ter *.TER *.ter *.TER *.nc *.NC *.hdf *.HDF *.lbl *.LBL *.cub *.CUB *.xml *.XML *.ers *.ERS *.ecw *.ECW *.jp2 *.JP2 *.j2k *.J2K *.jp2 *.JP2 *.j2k *.J2K *.grb *.GRB *.grb2 *.GRB2 *.grib2 *.GRIB2 *.sid *.SID *.jp2 *.JP2 *.rsw *.RSW *.nat *.NAT *.rst *.RST *.grd *.GRD *.grd *.GRD *.grd *.GRD *.hdr *.HDR *.rda *.RDA *.kml *.KML *.kmz *.KMZ *.webp *.WEBP *.pdf *.PDF *.sqlite *.SQLITE *.mbtiles *.MBTILES *.cal *.CAL *.ct1 *.CT1 *.mrf *.MRF *.pgm *.PGM *.ppm *.PPM *.pnm *.PNM *.hdr *.HDR *.bt *.BT *.lcp *.LCP *.gtx *.GTX *.gsb *.GSB *.gvb *.GVB *.ACE2 *.ACE2 *.hdr *.HDR *.kro *.KRO *.grd *.GRD *.byn *.BYN *.err *.ERR *.rik *.RIK *.dem *.DEM *.gxf *.GXF *.bag *.BAG *.h5 *.H5 *.hdf5 *.HDF5 *.grd *.GRD *.grc *.GRC *.gen *.GEN *.img *.IMG *.blx *.BLX *.sdat *.SDAT *.sg-grd-z *.SG-GRD-Z *.xyz *.XYZ *.hf2 *.HF2 *.dat *.DAT *.bin *.BIN *.ppi *.PPI *.prf *.PRF *.sigdem *.SIGDEM *.tga *.TGA *.json *.JSON *.gpkg *.GPKG *.dwg *.DWG *.bil *.BIL *.zip *.ZIP *.gz *.GZ *.tar *.TAR *.tar.gz *.TAR.GZ *.tgz *.TGZ) |GDAL/OGR VSIFileHandler (*.zip *.gz *.tar *.tar.gz *.tgz *.ZIP *.GZ *.TAR *.TAR.GZ *.TGZ);;ACE2 (*.ace2 *.ACE2);;ARC Digitized Raster Graphics (*.gen *.GEN);;ASCII Gridded XYZ (*.xyz *.XYZ);;Arc/Info ASCII Grid (*.asc *.ASC);;Arc/Info Binary Grid (hdr.adf HDR.ADF);;AutoCAD Driver (*.dwg *.DWG);;Bathymetry Attributed Grid (*.bag *.BAG);;CALS  (*.cal *.ct1 *.CAL *.CT1);;DRDC COASP SAR Processor Raster (*.hdr *.HDR);;DTED Elevation Raster (*.dt0 *.dt1 *.dt2 *.DT0 *.DT1 *.DT2);;ECRG TOC format (*.xml *.XML);;ERDAS Compressed Wavelets  (*.ecw *.ECW);;ERDAS JPEG2000  (*.jp2 *.j2k *.JP2 *.J2K);;ERMapper .ers Labelled (*.ers *.ERS);;ESRI .hdr Labelled (*.bil *.BIL);;EUMETSAT Archive native  (*.nat *.NAT);;Envisat Image Format (*.n1 *.N1);;Erdas Imagine Images  (*.img *.IMG);;FARSITE v.4 Landscape File  (*.lcp *.LCP);;GRIdded Binary  (*.grb *.grb2 *.grib2 *.GRB *.GRB2 *.GRIB2);;GeoPackage (*.gpkg *.GPKG);;GeoSoft Grid Exchange Format (*.gxf *.GXF);;GeoTIFF (*.tif *.tiff *.TIF *.TIFF);;Geospatial PDF (*.pdf *.PDF);;Golden Software 7 Binary Grid  (*.grd *.GRD);;Golden Software ASCII Grid  (*.grd *.GRD);;Golden Software Binary Grid  (*.grd *.GRD);;Graphics Interchange Format  (*.gif *.GIF);;Ground-based SAR Applications Testbed File Format  (*.gff *.GFF);;HF2/HFZ heightfield raster (*.hf2 *.HF2);;Hierarchical Data Format Release 4 (*.hdf *.HDF);;Hierarchical Data Format Release 5 (*.h5 *.hdf5 *.H5 *.HDF5);;ILWIS Raster Map (*.mpr *.mpl *.MPR *.MPL);;IRIS data  (*.ppi *.PPI);;Idrisi Raster A.1 (*.rst *.RST);;International Service for the Geoid (*.isg *.ISG);;JPEG JFIF (*.jpg *.jpeg *.JPG *.JPEG);;Japanese DEM  (*.mem *.MEM);;KOLOR Raw (*.kro *.KRO);;Kml Super Overlay (*.kml *.kmz *.KML *.KMZ);;Leveller heightfield (*.ter *.TER);;MBTiles (*.mbtiles *.MBTILES);;MS Windows Device Independent Bitmap (*.bmp *.BMP);;Magellan topo  (*.blx *.BLX);;Maptech BSB Nautical Charts (*.kap *.KAP);;Meta Raster Format (*.mrf *.MRF);;Multi-resolution Seamless Image Database  (*.sid *.SID);;NASA Planetary Data System 4 (*.xml *.XML);;NOAA NGS Geoid Height Grids (*.bin *.BIN);;NOAA Vertical Datum .GTX (*.gtx *.GTX);;NTv2 Datum Grid Shift (*.gsb *.gvb *.GSB *.GVB);;National Imagery Transmission Format (*.ntf *.NTF);;Natural Resources Canada's Geoid (*.byn *.err *.BYN *.ERR);;Network Common Data Format (*.nc *.NC);;Northwood Classified Grid Format .grc/.tab (*.grc *.GRC);;Northwood Numeric Grid Format .grd/.tab (*.grd *.GRD);;PCIDSK Database File (*.pix *.PIX);;PCRaster Raster File (*.map *.MAP);;Portable Network Graphics (*.png *.PNG);;Portable Pixmap Format  (*.pgm *.ppm *.pnm *.PGM *.PPM *.PNM);;R Object Data Store (*.rda *.RDA);;R Raster (*.grd *.GRD);;Racurs PHOTOMOD PRF (*.prf *.PRF);;Raster Matrix Format (*.rsw *.RSW);;Raster Product Format TOC format (*.toc *.TOC);;Rasterlite (*.sqlite *.SQLITE);;SAGA GIS Binary Grid  (*.sdat *.sg-grd-z *.SDAT *.SG-GRD-Z);;SDTS Raster (*.ddf *.DDF);;SGI Image File Format 1.0 (*.rgb *.RGB);;SRTMHGT File Format (*.hgt *.HGT);;Scaled Integer Gridded DEM .sigdem (*.sigdem *.SIGDEM);;Snow Data Assimilation System (*.hdr *.HDR);;Spatio-Temporal Asset Catalog Tiled Assets (*.json *.JSON);;Standard Raster Product  (*.img *.IMG);;Swedish Grid RIK  (*.rik *.RIK);;TGA/TARGA Image File Format (*.tga *.TGA);;Terragen heightfield (*.ter *.TER);;USGS Astrogeology ISIS cube  (*.lbl *.cub *.LBL *.CUB);;USGS Optional ASCII DEM  (*.dem *.DEM);;VTP .bt (Binary Terrain) 1.3 Format (*.bt *.BT);;Vexcel MFF Raster (*.hdr *.HDR);;Virtual Raster (*.vrt *.ovr *.VRT *.OVR);;WEBP (*.webp *.WEBP);;X11 PixMap Format (*.xpm *.XPM);;ZMap Plus Grid (*.dat *.DAT)";
        //    } else if (ft == FileTypes._MASK) {
        //        return "All files|*.*";
        //    } else if (ft == FileTypes._BURN || ft == FileTypes._OUTLETS || ft == FileTypes._STREAMS || ft == FileTypes._SUBBASINS || ft == FileTypes._CHANNELREACHES || ft == FileTypes._WATERSHED || ft == FileTypes._EXISTINGSUBBASINS || ft == FileTypes._EXISTINGWATERSHED || ft == FileTypes._GRID || ft == FileTypes._GRIDSTREAMS ||
        //        ft == FileTypes._HRUS || ft == FileTypes._ACTHRUS || ft == FileTypes._ACTLSUS ||
        //        ft == FileTypes._DRAINSTREAMS || ft == FileTypes._BUFFERSHAPE || ft == FileTypes._CHANNELS || ft == FileTypes._LSUS || ft == FileTypes._LAKES || ft == FileTypes._EXTRAPTSRCANDRES || ft == FileTypes._AQUIFERS || ft == FileTypes._PERMEABILITY || ft == FileTypes._OBSERVATIONLOCATIONS || ft == FileTypes._TILEDRAINS) {
        //        return "All files|*.*"; // All supported files(*.pix *.PIX *.nc *.NC *.xml *.XML *.jp2 *.JP2 *.jp2 *.JP2 *.j2k *.J2K *.pdf *.PDF *.mbtiles *.MBTILES *.bag *.BAG *.shp *.SHP *.dbf *.DBF *.shz *.SHZ *.shp.zip *.SHP.ZIP *.mif *.MIF *.tab *.TAB *.xml *.XML * .000 * .000 *.dgn *.DGN *.vrt *.VRT *.ovf *.OVF *.csv *.CSV *.xml *.XML *.gml *.GML *.gpx *.GPX *.kml *.KML *.kmz *.KMZ *.geojson *.GEOJSON *.geojsonl *.GEOJSONL *.geojsons *.GEOJSONS *.nlgeojson *.NLGEOJSON *.json *.JSON *.json *.JSON *.json *.JSON *.topojson *.TOPOJSON *.itf *.ITF *.xml *.XML *.ili *.ILI *.xtf *.XTF *.xml *.XML *.ili *.ILI *.gmt *.GMT *.gpkg *.GPKG *.sqlite *.SQLITE *.db *.DB *.sqlite3 *.SQLITE3 *.db3 *.DB3 *.s3db *.S3DB *.sl3 *.SL3 *.map *.MAP *.mdb *.MDB *.dxf *.DXF *.dwg *.DWG *.fgb *.FGB *.gxt *.GXT *.txt *.TXT *.xml *.XML *.vfk *.VFK *.sql *.SQL *.osm *.OSM *.pbf *.PBF *.mps *.MPS *.gdb *.GDB *.osm *.OSM *.tcx *.TCX *.igc *.IGC *.sos *.SOS *.thf *.THF *.svg *.SVG *.vct *.VCT *.xls *.XLS *.ods *.ODS *.xlsx *.XLSX *.sxf *.SXF *.jml *.JML *.txt *.TXT *.x10 *.X10 *.mvt *.MVT *.mvt.gz *.MVT.GZ *.pbf *.PBF *.parquet *.PARQUET *.arrow *.ARROW *.feather *.FEATHER *.arrows *.ARROWS *.ipc *.IPC *.e00 *.E00 *.zip *.ZIP *.gz *.GZ *.tar *.TAR *.tar.gz *.TAR.GZ *.tgz *.TGZ); ; GDAL / OGR VSIFileHandler(*.zip *.gz *.tar *.tar.gz *.tgz *.ZIP *.GZ *.TAR *.TAR.GZ *.TGZ); ; (Geo)Arrow IPC File Format / Stream(*.arrow *.feather *.arrows *.ipc *.ARROW *.FEATHER *.ARROWS *.IPC); ; (Geo)Parquet(*.parquet *.PARQUET); ; Arc / Info ASCII Coverage(*.e00 *.E00); ; AutoCAD DXF(*.dxf *.DXF); ; AutoCAD Driver(*.dwg *.DWG); ; Bathymetry Attributed Grid(*.bag *.BAG); ; Comma Separated Value(*.csv *.CSV); ; Czech Cadastral Exchange Data Format(*.vfk *.VFK); ; EDIGEO(*.thf *.THF); ; ESRI Personal GeoDatabase(*.mdb *.MDB); ; ESRI Shapefiles(*.shp *.shz *.shp.zip *.SHP *.SHZ *.SHP.ZIP); ; ESRIJSON(*.json *.JSON); ; FlatGeobuf(*.fgb *.FGB); ; GMT ASCII Vectors(.gmt)(*.gmt *.GMT); ; GPS eXchange Format[GPX](*.gpx *.GPX); ; GPSBabel(*.mps *.gdb *.osm *.tcx *.igc *.MPS *.GDB *.OSM *.TCX *.IGC); ; GeoJSON(*.geojson *.GEOJSON); ; GeoJSON Newline Delimited JSON(*.geojsonl *.geojsons *.nlgeojson *.json *.GEOJSONL *.GEOJSONS *.NLGEOJSON *.JSON); ; GeoPackage(*.gpkg *.GPKG); ; GeoRSS(*.xml *.XML); ; Geoconcept(*.gxt *.txt *.GXT *.TXT); ; Geography Markup Language[GML](*.gml *.GML); ; Geospatial PDF(*.pdf *.PDF); ; INTERLIS 1(*.itf *.xml *.ili *.ITF *.XML *.ILI); ; INTERLIS 2(*.xtf *.xml *.ili *.XTF *.XML *.ILI); ; Idrisi Vector(.vct) (*.vct *.VCT); ; Kadaster LV BAG Extract 2.0(*.xml *.XML); ; Keyhole Markup Language[KML](*.kml *.kmz *.KML *.KMZ); ; MBTiles(*.mbtiles *.MBTILES); ; MS Excel format(*.xls *.XLS); ; MS Office Open XML spreadsheet(*.xlsx *.XLSX); ; Mapbox Vector Tiles(*.mvt *.mvt.gz *.pbf *.MVT *.MVT.GZ *.PBF); ; Mapinfo File(*.mif *.tab *.MIF *.TAB); ; Microstation DGN(*.dgn *.DGN); ; NAS - ALKIS(*.xml *.XML); ; Network Common Data Format(*.nc *.NC); ; Open Document Spreadsheet(*.ods *.ODS); ; OpenJUMP JML(*.jml *.JML); ; OpenStreetMap(*.osm *.pbf *.OSM *.PBF); ; PCI Geomatics Database File(*.pix *.PIX); ; Planetary Data Systems TABLE(*.xml *.XML); ; PostgreSQL SQL dump(*.sql *.SQL); ; S - 57 Base file(*.000 * .000); ; SQLite / SpatiaLite(*.sqlite *.db *.sqlite3 *.db3 *.s3db *.sl3 *.SQLITE *.DB *.SQLITE3 *.DB3 *.S3DB *.SL3); ; Scalable Vector Graphics(*.svg *.SVG); ; Storage and eXchange Format(*.sxf *.SXF); ; Systematic Organization of Spatial Information[SOSI](*.sos *.SOS); ; TopoJSON(*.json *.topojson *.JSON *.TOPOJSON); ; VDV - 451 / VDV - 452 / INTREST Data Format(*.txt *.x10 *.TXT *.X10); ; VRT - Virtual Datasource(*.vrt *.ovf *.VRT *.OVF); ; WAsP(*.map *.MAP)";
        //    } else if (ft == FileTypes._CSV) {
        //        return "CSV files|*.csv|All files|*.*";
        //    } else if (ft == FileTypes._SQLITE) {
        //        return "SQLite files|*.sqlite; *.db; *.sdb; *.db3; *.s3db; *.sqlite3; *.sl3|All files|*.*";
        //    } else if (ft == FileTypes._INITIALIZATION) {
        //        return "Initialization files|*.ini; *.cfg; *.conf; *.config|All files|*.*";
        //    } else if (ft == FileTypes._OUTPUTTIMES) {
        //        return "Text files|*.txt|CSV files|*.csv|Excel files|*.xls; *.xlsx|All files|*.*";
        //    }
        //    return "";
        //}


        public static bool isRaster(int ft) {
            if (ft == FileTypes._DEM || 
                ft == FileTypes._LANDUSES || 
                ft == FileTypes._SOILS || 
                ft == FileTypes._SLOPEBANDS || 
                ft == FileTypes._HILLSHADE || 
                ft == FileTypes._CHANNELBASINSRASTER || 
                ft == FileTypes._BUFFERFLOOD ||
                ft == FileTypes._INVFLOOD ||
                ft == FileTypes._BRANCHFLOOD ||
                ft == FileTypes._AQUIFERTHICKNESS) {
                return true;
            } else {
                return false;
            }
        }

        // Legend entry string for file type ft.

        public static string legend(int ft) {
            if (ft == FileTypes._DEM) {
                return Utils._DEMLEGEND;
            } else if (ft == FileTypes._MASK) {
                return "Mask";
            } else if (ft == FileTypes._BURN) {
                return "Stream burn-in";
            } else if (ft == FileTypes._OUTLETS || ft == FileTypes._OUTLETSHUC) {
                return "Inlets/outlets";
            } else if (ft == FileTypes._STREAMS) {
                return Utils._STREAMSLEGEND;
            } else if (ft == FileTypes._CHANNELS) {
                return Utils._CHANNELSLEGEND;
            } else if (ft == FileTypes._LAKES) {
                return Utils._LAKESLEGEND;
            } else if (ft == FileTypes._SUBBASINS || ft == FileTypes._EXISTINGSUBBASINS) {
                return Utils._SUBBASINSLEGEND;
            } else if (ft == FileTypes._LANDUSES) {
                return "Landuses";
            } else if (ft == FileTypes._SOILS) {
                return "Soils";
            } else if (ft == FileTypes._SLOPEBANDS) {
                return "Slope bands";
            } else if (ft == FileTypes._WATERSHED || ft == FileTypes._EXISTINGWATERSHED) {
                return Utils._WATERSHEDLEGEND;
            } else if (ft == FileTypes._CHANNELREACHES) {
                return Utils._CHANNELREACHESLEGEND;
            } else if (ft == FileTypes._STREAMREACHES) {
                return "Stream reaches";
            } else if (ft == FileTypes._HILLSHADE) {
                return Utils._HILLSHADELEGEND;
            } else if (ft == FileTypes._GRID) {
                return Utils._GRIDLEGEND;
            } else if (ft == FileTypes._GRIDSTREAMS) {
                return Utils._GRIDSTREAMSLEGEND;
            } else if (ft == FileTypes._DRAINSTREAMS) {
                return Utils._DRAINSTREAMSLEGEND;
            } else if (ft == FileTypes._BUFFERFLOOD || ft == FileTypes._BUFFERSHAPE) {
                return Utils._BUFFERFLOODLEGEND;
            } else if (ft == FileTypes._INVFLOOD) {
                return Utils._INVFLOODLEGEND;
            } else if (ft == FileTypes._BRANCHFLOOD) {
                return Utils._BRANCHFLOODLEGEND;
            } else if (ft == FileTypes._HRUS) {
                return Utils._FULLHRUSLEGEND;
            } else if (ft == FileTypes._ACTHRUS) {
                return Utils._ACTHRUSLEGEND;
            } else if (ft == FileTypes._LSUS) {
                return Utils._FULLLSUSLEGEND;
            } else if (ft == FileTypes._ACTLSUS) {
                return Utils._ACTLSUSLEGEND;
            } else if (ft == FileTypes._AQUIFERS) {
                return Utils._AQUIFERSLEGEND;
            } else if (ft == FileTypes._GWFLOWCELLS) {
                return Utils._GWFLOWCELLSLEGEND;
            }
            return "";
        }

        // .qml file, if any, for file type ft.

        public static object styleFile(int ft) {
            if (ft == FileTypes._DEM) {
                return null;
            } else if (ft == FileTypes._MASK) {
                return null;
            } else if (ft == FileTypes._BURN) {
                return null;
            } else if (ft == FileTypes._OUTLETS) {
                return "outlets.qml";
            } else if (ft == FileTypes._STREAMS || ft == FileTypes._CHANNELREACHES) {
                return "stream.qml";
            } else if (ft == FileTypes._SUBBASINS || ft == FileTypes._EXISTINGSUBBASINS) {
                return "subbasins.qml";
            } else if (ft == FileTypes._WATERSHED) {
                return "wshed.qml";
            } else if (ft == FileTypes._EXISTINGWATERSHED) {
                return "existingwshed.qml";
            } else if (ft == FileTypes._GRID) {
                return "grid.qml";
            } else if (ft == FileTypes._GRIDSTREAMS) {
                return "gridstreams.qml";
            } else if (ft == FileTypes._LANDUSES) {
                return null;
            } else if (ft == FileTypes._SOILS) {
                return null;
            } else if (ft == FileTypes._SLOPEBANDS) {
                return null;
            } else if (ft == FileTypes._HILLSHADE) {
                return null;
            } else if (ft == FileTypes._OUTLETSHUC) {
                return "outletsHUC.qml";
            }
            return null;
        }


        public static Tuple<StyleItemType, string> getTypeAndName(int ft) {
            if (ft == FileTypes._STREAMS || ft == FileTypes._STREAMREACHES || ft == FileTypes._BURN) {
                return new Tuple<StyleItemType, string>(StyleItemType.LineSymbol, "Stream");
            } else if (ft == FileTypes._CHANNELS || ft == FileTypes._CHANNELREACHES) {
                return new Tuple<StyleItemType, string>(StyleItemType.LineSymbol, "Channel");
            } else if (ft == FileTypes._SUBBASINS || ft == FileTypes._EXISTINGSUBBASINS) {
                return new Tuple<StyleItemType, string>(StyleItemType.PolygonSymbol, "Subbasin");
            } else if (ft == FileTypes._WATERSHED) {
                return new Tuple<StyleItemType, string>(StyleItemType.PolygonSymbol, "Subbasin");
            } else if (ft == FileTypes._EXISTINGWATERSHED) {
                return new Tuple<StyleItemType, string>(StyleItemType.PolygonSymbol, "Subbasin");
            } else if (ft == FileTypes._HRUS || ft == FileTypes._ACTHRUS) {
                return new Tuple<StyleItemType, string>(StyleItemType.PolygonSymbol, "HRU");
            } else if (ft == FileTypes._LSUS || ft == FileTypes._ACTLSUS) {
                return new Tuple<StyleItemType, string>(StyleItemType.PolygonSymbol, "LSU");
            } else if (ft == FileTypes._LAKES) {
                return new Tuple<StyleItemType, string>(StyleItemType.PolygonSymbol, "Lake");
            } else if (ft == FileTypes._AQUIFERS) {
                return new Tuple<StyleItemType, string>(StyleItemType.PolygonSymbol, "Aquifer");
            }
                //} else if (ft == FileTypes._GRID) {
                //    return "grid.qml";
                //} else if (ft == FileTypes._GRIDSTREAMS) {
                //    return "gridstreams.qml";
                //}
            return null;
        }

        // Title for open file dialog for file type ft.

        public static string title(int ft) {
            if (ft == FileTypes._DEM) {
                return "Select DEM";
            } else if (ft == FileTypes._MASK) {
                return "Select mask";
            } else if (ft == FileTypes._BURN) {
                return "Select stream reaches shapefile to burn-in";
            } else if (ft == FileTypes._OUTLETS || ft == FileTypes._OUTLETSHUC) {
                return "Select inlets/outlets shapefile";
            } else if (ft == FileTypes._STREAMS) {
                return "Select stream reaches shapefile";
            } else if (ft == FileTypes._SUBBASINS || ft == FileTypes._EXISTINGSUBBASINS) {
                return "Select watershed shapefile";
            } else if (ft == FileTypes._LANDUSES) {
                return "Select landuses raster";
            } else if (ft == FileTypes._SOILS) {
                return "Select soils raster";
            } else if (ft == FileTypes._AQUIFERTHICKNESS) {
                return "Select aquifer thickness raster";
            } else if (ft == FileTypes._SLOPEBANDS || ft == FileTypes._CHANNELREACHES || ft == FileTypes._WATERSHED || ft == FileTypes._EXISTINGWATERSHED || ft == FileTypes._HILLSHADE || ft == FileTypes._GRID) {
                return "";
            }
            return "";
        }


        public static (string, string) mapTip(int ft, bool useOBJECTID=false) {
            if (ft == FileTypes._OUTLETS) {
                return ("ID", "\"Point id: \" + Text($feature.ID);");
            } else if (ft == FileTypes._OUTLETSHUC) {
                return ("ID", "\"Point id: \" + Text($feature.ID) + \" \" + Text($feature.Name);");
            } else if (ft == FileTypes._CHANNELREACHES) {
                return ("Channel", "\"Channel: \" + Text($feature.Channel);");
            } else if (ft == FileTypes._DRAINSTREAMS || ft == FileTypes._GRIDSTREAMS) {
                return ("LINKNO", "\"Stream link: \" + Text($feature.LINKNO);");
            } else if (ft == FileTypes._LSUS || ft == FileTypes._ACTLSUS) {
                return ("LSUID", "\"LSU: \" + Text($feature.LSUID);");
            } else if (ft == FileTypes._SUBBASINS || ft == FileTypes._EXISTINGSUBBASINS || ft == FileTypes._WATERSHED || ft == FileTypes._EXISTINGWATERSHED) {
                return ("Subbasin", "\"Subbasin: \" + Text($feature.Subbasin);");
            } else if (ft == FileTypes._HRUS || ft == FileTypes._ACTHRUS) {
                return ("HRUGIS", "\"HRU(s): \" + Text($feature.HRUGIS);");
            } else if (ft == FileTypes._LAKES) {
                if (useOBJECTID) {
                    return ("OBJECTID", "\"Lake: \" + Text($feature.OBJECTID);");
                } else {
                    return ("LakeId", "\"Lake: \" + Text($feature.LakeId);");
                }
            } else if (ft == FileTypes._AQUIFERS) {
                return ("Aquifer", "\"Aquifer: \" + Text($feature.Aquifer);");
            } else if (ft == FileTypes._GWFLOWCELLS) {
                return ("Cell", "\"Cell: \" + Text($feature.cell_id);");
            } else {
                return ("", "");
            }
        }

        // Layer colouring function for raster layer of file type ft.

        public static Action<RasterLayer, GlobalVars> colourFun(int ft) {
            if (ft == FileTypes._DEM) {
                return async (layer, gv) => await FileTypes.colourDEM(layer, gv);
            } else if (ft == FileTypes._LANDUSES) {
                return async (layer, gv) => await FileTypes.colourLanduses(layer, gv);
            } else if (ft == FileTypes._SOILS) {
                return async (layer, gv) => await FileTypes.colourSoils(layer, gv);
            } else if (ft == FileTypes._SLOPEBANDS) {
                return async (layer, gv) => await FileTypes.colourSlopes(layer, gv);
            } else if (ft == FileTypes._BUFFERFLOOD || ft == FileTypes._INVFLOOD || ft == FileTypes._BRANCHFLOOD) {
                return async (layer, gv) => await FileTypes.colourFlood(layer, gv);
            } else {
                return null;
            }
        }

        // example for subbasins
        // CIMPolygonSymbol polygonSymbol = SymbolFactory.Instance.ConstructPolygonSymbol(ColorFactory.Instance.RedRGB, SimpleFillStyle.Null);

        // Layer colouring function for DEM.

        public static async Task colourDEM(RasterLayer layer, object gv) {
            // calculate statistics (in particular max and min values)
            // by first generating a default colorizer
            // Create a new Stretch Colorizer Definition using the default constructor.
            CIMRasterStretchColorizer newStretchColorizer_default = null;
            //StatsHistogram stats = null;
            await QueuedTask.Run(() => {
                StretchColorizerDefinition stretchColorizerDef_default = new StretchColorizerDefinition();
                // Create a new Stretch colorizer using the colorizer definition created above.
                newStretchColorizer_default =
                    layer.CreateColorizer(stretchColorizerDef_default) as CIMRasterStretchColorizer;
                //    //layer.SetColorizer(newStretchColorizer_default);
            });
            //CIMRasterShadedReliefColorizer colorizer = null;
            //await QueuedTask.Run(() => {
            //    colorizer = layer.CreateColorizer(new ShadedReliefColorizerDefinition()) as CIMRasterShadedReliefColorizer;
            //});
            //StyleProjectItem style =
            //    Project.Current.GetItems<StyleProjectItem>()
            //        .FirstOrDefault(s => s.Name == "ArcGIS Colors");
            //if (style == null) return;
            //var colorRampList = await QueuedTask.Run(() =>
            //            style.SearchColorRamps("Elevation #10"));
            //if (colorRampList == null || colorRampList.Count == 0) return;
            //CIMColorRamp cimColorRamp = null;
            //await QueuedTask.Run(() => {
            //    cimColorRamp = colorRampList[0].ColorRamp;
            //    newStretchColorizer_default.ColorRamp = cimColorRamp;
            //    layer.SetColorizer(newStretchColorizer_default);
            //});

            CIMICCColorSpace colorSpace = new CIMICCColorSpace();
            colorSpace.URL = "Default RGB";
            CIMMultipartColorRamp ramp = new CIMMultipartColorRamp();
            CIMColor colour1 = CIMColor.CreateRGBColor(10, 100, 10, 25);
            CIMColor colour2 = CIMColor.CreateRGBColor(153, 125, 25, 25);
            CIMColor colour3 = CIMColor.CreateRGBColor(255, 255, 255, 25);
            CIMPolarContinuousColorRamp ramp1 = new CIMPolarContinuousColorRamp();
            ramp1.ColorSpace = colorSpace;
            ramp1.FromColor = colour1;
            ramp1.PrimitiveName = "ElevRamp1";
            ramp1.ToColor = colour2;
            CIMPolarContinuousColorRamp ramp2 = new CIMPolarContinuousColorRamp();
            ramp.ColorSpace = colorSpace;
            ramp2.FromColor = colour2;
            ramp2.PrimitiveName = "ElevRamp2";
            ramp2.ToColor = colour3;
            CIMColorRamp[] ramps = new CIMColorRamp[] { ramp1, ramp2 };
            ramp.ColorRamps = ramps;
            ramp.ColorSpace = colorSpace;
            double[] weights = new double[] { 1, 1 };
            ramp.Weights = weights;
            await QueuedTask.Run(() => {
                newStretchColorizer_default.ColorRamp = ramp;
                layer.SetColorizer(newStretchColorizer_default);
            });

            ////Accessing the statistics
            //stats = newStretchColorizer_default.StretchStats;
            //var minVal = Convert.ToInt32(stats.min);
            //var maxVal = Convert.ToInt32(stats.max);
            //// var mean = (minVal + maxVal) / 2;
            //int v1 = Convert.ToInt32(((minVal * 2 + maxVal) / 3);
            //int v2 = Convert.ToInt32(((minVal + maxVal * 2) / 3);
            //var s1 = v1.ToString();
            //var s2 = v2.ToString();
            //List<CIMColor> colours = new List<CIMColor>() { CIMColor.CreateRGBColor(10, 100, 10, 255), CIMColor.CreateRGBColor(153, 125, 25, 255), CIMColor.CreateRGBColor(255, 255, 255, 255) };
            //List<int> vals = new List<int>() { minVal, v1, v2, maxVal };
            //List<String> labels = new List<String>() { minVal.ToString() + " - " + s1, s1 + " - " + s2, s2 + " - " + maxVal.ToString() };
            //ColormapColorizerDefinition defn = new ColormapColorizerDefinition(colours, vals, labels);
            //defn.NoDataColor = CIMColor.CreateGrayColor(0);
            //await QueuedTask.Run(() => {
            //    var lst = layer.GetApplicableColorizers().ToString();
            //    //Utils.information(String.Format("Applicable colorizers: {0}", layer.GetApplicableColorizers().ToString()), false);
            //    if (layer.CanCreateColorizer(defn)) {
            //        var colorizer = layer.CreateColorizer(defn) as CIMRasterColorMapColorizer;
            //        layer.SetColorizer(colorizer);
            //    } else {
            //        Utils.error(String.Format("Cannot create colorizer.  Applicable: {0}", layer.GetApplicableColorizers().ToString()), false);
            //    }
            //});
            //var item0 = QgsColorRampShader.ColorRampItem(minVal, CreateRGBColor(10, 100, 10, 255), minVal.ToString() + " - " + s1);
            //var item1 = QgsColorRampShader.ColorRampItem(mean, CreateRGBColor(153, 125, 25, 255), s1 + " - " + s2);
            //var item2 = QgsColorRampShader.ColorRampItem(maxVal, CreateRGBColor(255, 255, 255, 255), s2 + " - " + maxVal.ToString());
            //var fcn = QgsColorRampShader(minVal, maxVal);
            //fcn.setColorRampType(QgsColorRampShader.Interpolated);
            //fcn.setColorRampItemList(new List<object> {
            //    item0,
            //    item1,
            //    item2
            //});
            //if (gv.QGISSubVersion >= 18) {
            //    //legend settings is QGIS 3.18 or later
            //    // from qgis.core import QgsColorRampLegendNodeSettings  # @UnresolvedImport
            //    var legendSettings = fcn.legendSettings();
            //    legendSettings.setUseContinuousLegend(false);
            //    fcn.setLegendSettings(legendSettings);
            //}
            //shader.setRasterShaderFunction(fcn);
            //var renderer = QgsSingleBandPseudoColorRenderer(layer.dataProvider(), 1, shader);
            //layer.setRenderer(renderer);
            //layer.triggerRepaint();
        }

        // symbology for inlets/outlets file
        public static async Task colourPoints(FeatureLayer featureLayer, GlobalVars gv) {
            // collect symbols from arcSWATPlusStyle
            CIMSymbolReference outletRef = null;
            CIMSymbolReference reservoirRef = null;
            CIMSymbolReference pondRef = null;
            CIMSymbolReference inletRef = null;
            CIMSymbolReference ptsourceRef = null;
            await QueuedTask.Run(() => {
                try { 
                    var outlet = ((SymbolStyleItem)gv.arcSWATPlusStyle.LookupItem(StyleItemType.PointSymbol, "Outlet")).Symbol as CIMPointSymbol;
                    var reservoir = ((SymbolStyleItem)gv.arcSWATPlusStyle.LookupItem(StyleItemType.PointSymbol, "Reservoir")).Symbol as CIMPointSymbol;
                    var pond = ((SymbolStyleItem)gv.arcSWATPlusStyle.LookupItem(StyleItemType.PointSymbol, "Pond")).Symbol as CIMPointSymbol;
                    var inlet = ((SymbolStyleItem)gv.arcSWATPlusStyle.LookupItem(StyleItemType.PointSymbol, "Inlet")).Symbol as CIMPointSymbol;
                    var ptsource = ((SymbolStyleItem)gv.arcSWATPlusStyle.LookupItem(StyleItemType.PointSymbol, "Point_source")).Symbol as CIMPointSymbol;
                    outletRef = outlet.MakeSymbolReference();
                    reservoirRef = reservoir.MakeSymbolReference();
                    pondRef = pond.MakeSymbolReference();
                    inletRef = inlet.MakeSymbolReference();
                    ptsourceRef = ptsource.MakeSymbolReference();
                    var outletValues = new List<CIMUniqueValue> { new CIMUniqueValue { FieldValues = new string[] { "0", "0", "0" } } };
                    CIMUniqueValueClass outletValueClass = new CIMUniqueValueClass {
                        Editable = true,
                        Label = "Outlet",
                        Symbol = outletRef,
                        Visible = true,
                        Values = outletValues.ToArray()
                    };
                    var reservoirValues = new List<CIMUniqueValue> { new CIMUniqueValue { FieldValues = new string[] { "1", "0", "0" } } };
                    CIMUniqueValueClass reservoirValueClass = new CIMUniqueValueClass {
                        Editable = true,
                        Label = "Reservoir",
                        Symbol = reservoirRef,
                        Visible = true,
                        Values = reservoirValues.ToArray()
                    };
                    var pondValues = new List<CIMUniqueValue> { new CIMUniqueValue { FieldValues = new string[] { "2", "0", "0" } } };
                    CIMUniqueValueClass pondValueClass = new CIMUniqueValueClass {
                        Editable = true,
                        Label = "Pond",
                        Symbol = pondRef,
                        Visible = true,
                        Values = pondValues.ToArray()
                    };
                    var inletValues = new List<CIMUniqueValue> { new CIMUniqueValue { FieldValues = new string[] { "0", "1", "0" } } };
                    CIMUniqueValueClass inletValueClass = new CIMUniqueValueClass {
                        Editable = true,
                        Label = "Inlet",
                        Symbol = inletRef,
                        Visible = true,
                        Values = inletValues.ToArray()
                    };
                    var ptsourceValues = new List<CIMUniqueValue> { new CIMUniqueValue { FieldValues = new string[] { "0", "1", "1" } } };
                    CIMUniqueValueClass ptsourceValueClass = new CIMUniqueValueClass {
                        Editable = true,
                        Label = "Point source",
                        Symbol = ptsourceRef,
                        Visible = true,
                        Values = ptsourceValues.ToArray()
                    };
                    var listClasses = new List<CIMUniqueValueClass>() { outletValueClass, reservoirValueClass, pondValueClass, inletValueClass, ptsourceValueClass };
                    var uvg = new CIMUniqueValueGroup {
                        Classes = listClasses.ToArray()
                    };
                    var listUniqueValueGroups = new List<CIMUniqueValueGroup> { uvg };
                    var uvr = new CIMUniqueValueRenderer {
                        Fields = new string[] { "RES", "INLET", "PTSOURCE" },
                        UseDefaultSymbol = true,
                        Groups = listUniqueValueGroups.ToArray()
                    };
                    featureLayer.SetRenderer(uvr);
                } catch (Exception ex) {
                    Utils.error("Failed to colour inlets/outlets file: " + ex.Message, gv.isBatch);
                }
            });
        }

        // symbology for subbasins file
        public static async Task colourSubbasins(FeatureLayer featureLayer, GlobalVars gv) {
            // check if it has a subbasin field yet
            if (await gv.topo.getIndex(featureLayer, "Subbasin", true) < 0) {
                await ApplySymbolToFeatureLayerAsync(featureLayer, FileTypes._SUBBASINS, gv);
                return;
            }
            // collect symbols from arcSWAT3Style
            CIMSymbolReference subbasinRef = null;
            CIMSymbolReference upstreamRef = null;
            await QueuedTask.Run(() => {
                var subbasin = ((SymbolStyleItem)gv.arcSWATPlusStyle.LookupItem(StyleItemType.PolygonSymbol, "Subbasin")).Symbol as CIMPolygonSymbol;
                var upstream = ((SymbolStyleItem)gv.arcSWATPlusStyle.LookupItem(StyleItemType.PolygonSymbol, "Upstream")).Symbol as CIMPolygonSymbol;
                subbasinRef = subbasin.MakeSymbolReference();
                upstreamRef = upstream.MakeSymbolReference();
                var subbasinValues = new List<CIMUniqueValue> { new CIMUniqueValue { FieldValues = new string[] { "true" } } };
                CIMUniqueValueClass subbasinValueClass = new CIMUniqueValueClass {
                    Editable = true,
                    Label = "Subbasin",
                    Symbol = subbasinRef,
                    Visible = true,
                    Values = subbasinValues.ToArray()
                };
                var upstreamValues = new List<CIMUniqueValue> { new CIMUniqueValue { FieldValues = new string[] { "false" } } };
                CIMUniqueValueClass upstreamValueClass = new CIMUniqueValueClass {
                    Editable = true,
                    Label = "Upstream",
                    Symbol = upstreamRef,
                    Visible = true,
                    Values = upstreamValues.ToArray()
                };
                var listClasses = new List<CIMUniqueValueClass>() { subbasinValueClass, upstreamValueClass };
                var uvg = new CIMUniqueValueGroup {
                    Classes = listClasses.ToArray()
                };
                var listUniqueValueGroups = new List<CIMUniqueValueGroup> { uvg };
                var expressionInfo = new CIMExpressionInfo {
                    Expression = "$feature.Subbasin == null || $feature.Subbasin > 0",
                    Title = "Subbasin"
                };
                var uvr = new CIMUniqueValueRenderer {
                    ValueExpressionInfo = expressionInfo,
                    UseDefaultSymbol = true,
                    Groups = listUniqueValueGroups.ToArray()
                };
                featureLayer.SetRenderer(uvr);
            });
        }

        //Colour streams layer classifying drainage with symbol based on penWidth.
        public static async Task colourStreams(FeatureLayer layer, string penWidth, string drainage) {
            // ramp from light to darkish blue
            CIMICCColorSpace colorSpace = new CIMICCColorSpace();
            colorSpace.URL = "Default RGB";
            CIMColor colour1 = CIMColor.CreateRGBColor(166, 206, 227, 255);
            CIMColor colour2 = CIMColor.CreateRGBColor(0, 0, 255, 255);
            CIMPolarContinuousColorRamp ramp = new CIMPolarContinuousColorRamp();
            ramp.ColorSpace = colorSpace;
            ramp.FromColor = colour1;
            ramp.PrimitiveName = "MyBlues";
            ramp.ToColor = colour2;
            int count = 5;
            double transparency = 0;
            CIMSymbol symbol = SymbolFactory.Instance.ConstructLineSymbol();
            GraduatedColorsRendererDefinition rendererDefn = new GraduatedColorsRendererDefinition() {
                ClassificationField = penWidth,
                ClassificationMethod = ArcGIS.Core.CIM.ClassificationMethod.NaturalBreaks,
                BreakCount = count,
                ColorRamp = ramp,
                SymbolTemplate = symbol.MakeSymbolReference()
            };
            CIMClassBreaksRenderer renderer;
            await QueuedTask.Run(() => {
                renderer = layer.CreateRenderer(rendererDefn) as CIMClassBreaksRenderer;
                CIMVisualVariable[] visualVars = new CIMVisualVariable[1];
                var expressionInfo = new CIMExpressionInfo() {
                    Expression = "$feature.PenWidth",
                    Title = "Custom"
                };
                var sizeVar = new CIMSizeVisualVariable() {
                    ValueExpressionInfo = expressionInfo,
                    VariableType = SizeVisualVariableType.Expression,
                    MaxSize = 0,
                    MinValue = 1,
                    MaxValue = 8
                };
                visualVars[0] = sizeVar;
                renderer.VisualVariables = visualVars;
                layer.SetRenderer(renderer);
                layer.SetTransparency(transparency);
                //MapView.Active.Redraw(false);
            });
        }

        // symbology for Lakes file
        public static async Task colourLakes(FeatureLayer featureLayer, GlobalVars gv) {
            bool alwaysRes = false;
            // check if it has a reservoir field yet
            if (await gv.topo.getIndex(featureLayer, Topology._RES, true) < 0) {
                alwaysRes = true;
            }
            // collect symbols from arcSWAT3Style
            CIMSymbolReference reservoirRef = null;
            CIMSymbolReference pondRef = null;
            CIMSymbolReference wetlandRef = null;
            CIMSymbolReference playaRef = null;
            CIMUniqueValueClass reservoirValueClass = null;
            CIMUniqueValueClass pondValueClass = null;
            CIMUniqueValueClass wetlandValueClass = null; 
            CIMUniqueValueClass playaValueClass = null;
            await QueuedTask.Run(() => {
                if (alwaysRes) {
                    var reservoir = ((SymbolStyleItem)gv.arcSWATPlusStyle.LookupItem(StyleItemType.PolygonSymbol, "Reservoir2")).Symbol as CIMPolygonSymbol;
                    reservoirRef = reservoir.MakeSymbolReference();
                    var reservoirValues = new List<CIMUniqueValue> { new CIMUniqueValue { FieldValues = new string[] { "true" } } };
                    reservoirValueClass = new CIMUniqueValueClass {
                        Editable = true,
                        Label = "Reservoir",
                        Symbol = reservoirRef,
                        Visible = true,
                        Values = reservoirValues.ToArray()
                    };
                } else {
                    var reservoir = ((SymbolStyleItem)gv.arcSWATPlusStyle.LookupItem(StyleItemType.PolygonSymbol, "Reservoir2")).Symbol as CIMPolygonSymbol;
                    reservoirRef = reservoir.MakeSymbolReference();
                    var reservoirValues = new List<CIMUniqueValue> { new CIMUniqueValue { FieldValues = new string[] { "1" } } };
                    reservoirValueClass = new CIMUniqueValueClass {
                        Editable = true,
                        Label = "Reservoir",
                        Symbol = reservoirRef,
                        Visible = true,
                        Values = reservoirValues.ToArray()
                    };
                    var pond = ((SymbolStyleItem)gv.arcSWATPlusStyle.LookupItem(StyleItemType.PolygonSymbol, "Pond2")).Symbol as CIMPolygonSymbol;
                    var wetland = ((SymbolStyleItem)gv.arcSWATPlusStyle.LookupItem(StyleItemType.PolygonSymbol, "Wetland")).Symbol as CIMPolygonSymbol;
                    var playa = ((SymbolStyleItem)gv.arcSWATPlusStyle.LookupItem(StyleItemType.PolygonSymbol, "Playa")).Symbol as CIMPolygonSymbol;
                    pondRef = pond.MakeSymbolReference();
                    wetlandRef = wetland.MakeSymbolReference();
                    playaRef = playa.MakeSymbolReference();
                    var pondValues = new List<CIMUniqueValue> { new CIMUniqueValue { FieldValues = new string[] { "2" } } };
                    pondValueClass = new CIMUniqueValueClass {
                        Editable = true,
                        Label = "Pond",
                        Symbol = pondRef,
                        Visible = true,
                        Values = pondValues.ToArray()
                    };
                    var wetlandValues = new List<CIMUniqueValue> { new CIMUniqueValue { FieldValues = new string[] { "3" } } };
                    wetlandValueClass = new CIMUniqueValueClass {
                        Editable = true,
                        Label = "Wetland",
                        Symbol = wetlandRef,
                        Visible = true,
                        Values = wetlandValues.ToArray()
                    };
                    var playaValues = new List<CIMUniqueValue> { new CIMUniqueValue { FieldValues = new string[] { "4" } } };
                    playaValueClass = new CIMUniqueValueClass {
                        Editable = true,
                        Label = "playa",
                        Symbol = playaRef,
                        Visible = true,
                        Values = playaValues.ToArray()
                    };
                }
                List<CIMUniqueValueClass> listClasses = null;
                if (alwaysRes) {
                    listClasses = new List<CIMUniqueValueClass>() { reservoirValueClass };
                } else {
                    listClasses = new List<CIMUniqueValueClass>() { reservoirValueClass, pondValueClass, wetlandValueClass, playaValueClass };
                }
                var uvg = new CIMUniqueValueGroup {
                    Classes = listClasses.ToArray()
                };
                List<CIMUniqueValueGroup> listUniqueValueGroups = null;
                CIMUniqueValueRenderer uvr = null;
                if (alwaysRes) {
                    listUniqueValueGroups = new List<CIMUniqueValueGroup> { uvg };
                    var expressionInfo = new CIMExpressionInfo {
                        Expression = "true",
                        Title = "Lake"
                    };
                    uvr = new CIMUniqueValueRenderer {
                        ValueExpressionInfo = expressionInfo,
                        UseDefaultSymbol = true,
                        Groups = listUniqueValueGroups.ToArray()
                    };
                } else {
                    listUniqueValueGroups = new List<CIMUniqueValueGroup> { uvg };
                    uvr = new CIMUniqueValueRenderer {
                        Fields = new string[] { "RES" },
                        UseDefaultSymbol = true,
                        Groups = listUniqueValueGroups.ToArray()
                    };
                }
                featureLayer.SetRenderer(uvr);
            });
        }

        // symbology for grid file
        public static async Task colourGrid(FeatureLayer featureLayer, GlobalVars gv) {
            // collect symbols from arcSWAT3Style
            CIMSymbolReference gridRef = null;
            CIMSymbolReference lakeRef = null;
            await QueuedTask.Run(() => {
                var grid = ((SymbolStyleItem)gv.arcSWATPlusStyle.LookupItem(StyleItemType.PolygonSymbol, "Grid")).Symbol as CIMPolygonSymbol;
                var lake = ((SymbolStyleItem)gv.arcSWATPlusStyle.LookupItem(StyleItemType.PolygonSymbol, "Gridlake")).Symbol as CIMPolygonSymbol;
                gridRef = grid.MakeSymbolReference();
                lakeRef = lake.MakeSymbolReference();
                var gridValues = new List<CIMUniqueValue> { new CIMUniqueValue { FieldValues = new string[] { "true" } } };
                CIMUniqueValueClass gridValueClass = new CIMUniqueValueClass {
                    Editable = true,
                    Label = "Grid",
                    Symbol = gridRef,
                    Visible = true,
                    Values = gridValues.ToArray()
                };
                var lakeValues = new List<CIMUniqueValue> { new CIMUniqueValue { FieldValues = new string[] { "false" } } };
                CIMUniqueValueClass lakeValueClass = new CIMUniqueValueClass {
                    Editable = true,
                    Label = "Lake",
                    Symbol = lakeRef,
                    Visible = true,
                    Values = lakeValues.ToArray()
                };
                var listClasses = new List<CIMUniqueValueClass>() { gridValueClass, lakeValueClass };
                var uvg = new CIMUniqueValueGroup {
                    Classes = listClasses.ToArray()
                };
                var listUniqueValueGroups = new List<CIMUniqueValueGroup> { uvg };
                var expressionInfo = new CIMExpressionInfo {
                    Expression = "$feature.LakeId == null",
                    Title = "Grid"
                };
                var uvr = new CIMUniqueValueRenderer {
                    ValueExpressionInfo = expressionInfo,
                    UseDefaultSymbol = true,
                    Groups = listUniqueValueGroups.ToArray()
                };
                featureLayer.SetRenderer(uvr);
            });
        }

        // symbology for gwflow grid file
        public static async Task colourCells(FeatureLayer featureLayer, GlobalVars gv) {
            // collect symbols from arcSWAT3Style
            CIMSymbolReference cellRef = null;
            await QueuedTask.Run(() => {
                var cell = ((SymbolStyleItem)gv.arcSWATPlusStyle.LookupItem(StyleItemType.PolygonSymbol, "Cell")).Symbol as CIMPolygonSymbol;
                cellRef = cell.MakeSymbolReference();
                CIMSimpleRenderer renderer = featureLayer.GetRenderer() as CIMSimpleRenderer;
                renderer.Symbol = cellRef;
                featureLayer.SetRenderer(renderer);
            });
        }

        // Layer colouring function for landuse grid.

        public static async Task colourLanduses(RasterLayer layer, GlobalVars gv) {
            await QueuedTask.Run(async () => {
                var raster = layer.GetRaster();
                if (raster is null) { return; }
                var ramp = await SWATRamp("Random");
                var defn = new UniqueValueColorizerDefinition("Value", ramp);
                bool ok1 = layer.CanCreateColorizer(defn);
                if (!ok1) {
                    Utils.loginfo("Cannot create colorizer for landuses");
                    return;
                }
                try {
                    var colorizer = await layer.CreateColorizerAsync(defn) as CIMRasterUniqueValueColorizer;
                    // should only be one group
                    // get the number of colours needed
                    var grps = colorizer.Groups;
                    var classes = grps[0].Classes;
                    var total_colours = classes.Length;
                    var colours = ColorFactory.Instance.GenerateColorsFromColorRamp(ramp, total_colours);
                    foreach (int i in Enumerable.Range(0, total_colours)) {
                        classes[i].Color = colours[i];
                        var value = Convert.ToInt32(classes[i].Values[0]);
                        string label = value.ToString();
                        // use codes if landuse lookup table has been loaded
                        if (gv.db.landuseCodes.Count > 0) {
                            label = gv.db.getLanduseCode(value);
                        }
                        classes[i].Label = label;
                    }
                    grps[0].Classes = classes;
                    colorizer.Groups = grps;
                    bool ok2 = layer.CanSetColorizer(colorizer);
                    if (!ok2) {
                        Utils.loginfo("Cannot set colorizer for landuses");
                        return;
                    }
                    layer.SetColorizer(colorizer);
                    //MapView.Active.Redraw(false);
                }
                catch (Exception ex) {
                    Utils.loginfo("Cannot create landuse colorizer: " + ex.Message);
                    return;
                }
            });
        }

        // Layer colouring function for soil grid.

        public static async Task colourSoils(RasterLayer layer, GlobalVars gv) {
            await QueuedTask.Run(async () => {
                var raster = layer.GetRaster();
                if (raster is null) { return; }
                var ramp = await SWATRamp("Random");
                var defn = new UniqueValueColorizerDefinition("Value", ramp);
                bool ok1 = layer.CanCreateColorizer(defn);
                if (!ok1) {
                    Utils.loginfo("Cannot create colorizer for soils");
                    return;
                }
                try {
                    var colorizer = await layer.CreateColorizerAsync(defn) as CIMRasterUniqueValueColorizer;
                    // should only be one group
                    // get the number of colours needed
                    var grps = colorizer.Groups;
                    if (grps is not null) {  // colorizer can fail to have any groups
                        var classes = grps[0].Classes;
                        var total_colours = classes.Length;
                        var colours = ColorFactory.Instance.GenerateColorsFromColorRamp(ramp, total_colours);
                        foreach (int i in Enumerable.Range(0, total_colours)) {
                            classes[i].Color = colours[i];
                            var value = Convert.ToInt32(classes[i].Values[0]);
                            string label = value.ToString();
                            // use names if soil lookup table has been loaded
                            if (gv.db.soilNames.Count > 0) {
                                // only make name label if actually used 
                                // (otherwise getSoilName will add it to usedSoils)
                                var value1 = gv.db.translateSoil(value).Item1;
                                if (gv.db.usedSoilNames.Keys.Contains(value1))
                                {
                                    label = gv.db.getSoilName(value).Item1;
                                }
                            }
                            classes[i].Label = label;
                        }
                        grps[0].Classes = classes;
                        colorizer.Groups = grps;
                        bool ok2 = layer.CanSetColorizer(colorizer);
                        if (!ok2) {
                            Utils.loginfo("Cannot set colorizer for soils");
                            return;
                        }
                        layer.SetColorizer(colorizer);
                        //MapView.Active.Redraw(false);
                    }
                    else {
                        Utils.loginfo("Cannot create soil colorizer");
                    }
                }
                catch (Exception ex) {
                    Utils.loginfo("Cannot create soil colorizer: " + ex.Message);
                    return;
                }
            });
        }

        // Layer colouring for slope bands grid.

        public static async Task colourSlopes(RasterLayer layer, GlobalVars gv) {
            await QueuedTask.Run(async () => {
                var raster = layer.GetRaster();
                if (raster is null) { return; }
                var ramp = await SWATRamp("Grays");
                var count = gv.db.slopeLimits.Count + 1;
                var defn = new ClassifyColorizerDefinition("Value", count, ClassificationMethod.Manual, ramp);
                bool ok1 = layer.CanCreateColorizer(defn);
                if (!ok1) {
                    Utils.loginfo("Cannot create colorizer for slopes");
                    return;
                }
                try {
                    var colorizer = await layer.CreateColorizerAsync(defn) as CIMRasterClassifyColorizer;
                    var colours = ColorFactory.Instance.GenerateColorsFromColorRamp(ramp, 3);
                    CIMRasterClassBreak[] breaks = new CIMRasterClassBreak[3];
                    (double, double)[] bounds = new (double, double)[3];
                    foreach (int i in Enumerable.Range(0, 3)) {
                        breaks[i] = new CIMRasterClassBreak();
                        breaks[i].Color = colours[i];
                        bounds[i].Item1 = i == 0 ? 0 : gv.db.slopeLimits[i - 1];
                        bounds[i].Item2 = i == 2 ? 9999 : gv.db.slopeLimits[i];
                        breaks[i].Label = gv.db.slopeRange(i); // string.Format("{0:F1} - {1:F1}", bounds[i].Item1, bounds[i].Item2);
                        breaks[i].UpperBound = i == 2 ? 9999 : gv.db.slopeLimits[i];
                    }
                    colorizer.ClassBreaks = breaks;
                    bool ok2 = layer.CanSetColorizer(colorizer);
                    if (!ok2) {
                        Utils.loginfo("Cannot set colorizer for slopes");
                        return;
                    }
                    layer.SetColorizer(colorizer);
                    //MapView.Active.Redraw(false);
                    layer.SetVisibility(false);
                }
                catch (Exception ex) {
                    Utils.loginfo("Cannot create slope colorizer: " + ex.Message);
                    return;
                }
            });
        }

        // Layer colouring for floodplain grid.

        public static async Task colourFlood(RasterLayer layer, GlobalVars gv) {
            await QueuedTask.Run(async () => {
                var raster = layer.GetRaster();
                if (raster is null) { return; }
                var defn = new UniqueValueColorizerDefinition("Value");
                bool ok1 = layer.CanCreateColorizer(defn);
                if (!ok1) {
                    Utils.loginfo("Cannot create colorizer for slopes");
                    return;
                }
                try {
                    var colorizer = await layer.CreateColorizerAsync(defn) as CIMRasterUniqueValueColorizer;
                    // only one colour required: white
                    CIMColor white = CIMColor.CreateRGBColor(255,255,255);
                    // set up a single group with a single class
                    var grps = new CIMRasterUniqueValueGroup[1] { new CIMRasterUniqueValueGroup() };
                    var classes = new CIMRasterUniqueValueClass[1] { new CIMRasterUniqueValueClass() };
                    classes[0].Color = white;
                    string[] values = new string[1] { "0" };
                    classes[0].Values = values;
                    classes[0].Label = "Floodplain";
                    grps[0].Classes = classes;
                    colorizer.Groups = grps;
                    colorizer.NoDataColor = CIMColor.NoColor();
                    bool ok2 = layer.CanSetColorizer(colorizer);
                    if (!ok2) {
                        Utils.loginfo("Cannot set colorizer for floodplain");
                        return;
                    }
                    layer.SetColorizer(colorizer);
                    layer.SetExpanded(true);
                    //MapView.Active.Redraw(false);
                }
                catch (Exception ex) {
                    Utils.loginfo("Cannot create floodplain colorizer: " + ex.Message);
                    return;
                }
            });
        }

        public static async Task ApplySymbolToFeatureLayerAsync(FeatureLayer featureLayer, int ft, GlobalVars gv) {
            if (ft == FileTypes._OUTLETS || ft == FileTypes._EXTRAPTSRCANDRES) {
                await colourPoints(featureLayer, gv);
                return;
            } else if (ft == FileTypes._SUBBASINS || ft == FileTypes._EXISTINGSUBBASINS) {
                await colourSubbasins(featureLayer, gv);
                return;
            } else if (ft == FileTypes._LAKES) {
                await colourLakes(featureLayer, gv);
                return;
            } else if (ft == FileTypes._GRID) {
                await colourGrid(featureLayer, gv);
                return;
            } else if (ft == FileTypes._GWFLOWCELLS) {
                await colourCells(featureLayer, gv);
            }

            Tuple<StyleItemType, string> pair = getTypeAndName(ft);
            if (pair is null) { return; }
            var symbolType = pair.Item1;
            var symbolName = pair.Item2;
            await QueuedTask.Run(() => {

                //Search for the symbolName style items within the style project item.
                var items = gv.arcSWATPlusStyle.SearchSymbols(symbolType, symbolName);
                if (items.Count == 0) { return; }

                //Gets the CIMSymbol
                CIMSymbol symbol = items.FirstOrDefault().Symbol;

                //Get the renderer of the feature layer
                // seems to fail often
                try {
                    CIMSimpleRenderer renderer = featureLayer.GetRenderer() as CIMSimpleRenderer;

                    //Set symbol's real world setting to be the same as that of the feature layer
                    symbol.SetRealWorldUnits(featureLayer.UsesRealWorldSymbolSizes);

                    //Apply the symbol to the feature layer's current renderer
                    renderer.Symbol = symbol.MakeSymbolReference();

                    //Appy the renderer to the feature layer
                    featureLayer.SetRenderer(renderer);
                    //MapView.Active.Redraw(false);
                } catch { }
            });
        }

        public static async Task<CIMColorRamp> SWATRamp(string name) {
            StyleProjectItem style = null;
            var items = Project.Current.GetItems<StyleProjectItem>();
            StyleProjectItem style1 = items.FirstOrDefault(s => s.Name == "ColorBrewer Schemes (RGB)");
            StyleProjectItem style2 = items.FirstOrDefault(s => s.Name == "ArcGIS Colors");
            //StyleProjectItem style3 = items.FirstOrDefault(s => s.Name == "ArcGIS 2D"); // empty
            string arcName = "";
            switch (name) {
                case "RdYlGn":
                    style = style2;
                    if (style == null) return null;
                    // Condition Number is the reversal of Red-Yellow-Green
                    arcName = "Condition Number";
                    break;
                case "YlGnBu":
                    style = style1;
                    if (style == null) return null;
                    arcName = "Yellow-Green-Blue (Continuous)";
                    break;
                case "GnBu":
                    style = style1;
                    if (style == null) return null;
                    arcName = "Blue-Green (Continuous)";
                    break;
                case "YlOrRd":
                    style = style1;
                    if (style == null) return null;
                    arcName = "Yellow-Orange-Red (Continuous)";
                    break;
                case "Grays":
                    style = style1;
                    if (style == null) return null;
                    arcName = "Grays (Continuous)";
                    break;
                case "Random":
                    style = style2;
                    if (style == null) return null;
                    arcName = "Basic Random";
                    break;
            }
            //debugging code for listing available styles
            //var ramps1 = new List<string>();
            //var crl1 = await QueuedTask.Run(() =>
            //        style1.SearchColorRamps(""));
            //for (int i = 0; i < crl1.Count; i++) {
            //    ramps1.Add(string.Format("({0}, {1})", i.ToString(), crl1[i].Name));
            //}
            //var ramps2 = new List<string>();
            //var crl2 = await QueuedTask.Run(() =>
            //        style2.SearchColorRamps(""));
            //for (int i = 0; i < crl2.Count; i++) {
            //    ramps2.Add(string.Format("({0}, {1})", i.ToString(), crl2[i].Name));
            //}
            IList<ColorRampStyleItem> colorRampList = await QueuedTask.Run(() =>
                        style.SearchColorRamps(arcName));
            if (colorRampList == null || colorRampList.Count == 0) return null;
            // search returns a 'contains' match, so look for a precise one
            int j = 0; // default to first if precise match fails
            for (int i = 0; i < colorRampList.Count; i++) {
                if (arcName == colorRampList[i].Name) {
                    j = i;
                    break;
                }
            }
            CIMColorRamp cimColorRamp = null;
            await QueuedTask.Run(() => {
                cimColorRamp = colorRampList[j].ColorRamp;
            });
            return cimColorRamp;
        }
    }
    public class CursorWait : IDisposable
    {
        public CursorWait(bool appStarting = false, bool applicationCursor = true)
        {
            // Wait
            Cursor.Current = appStarting ? Cursors.AppStarting : Cursors.WaitCursor;
            if (applicationCursor) System.Windows.Forms.Application.UseWaitCursor = true;
        }

        public void Dispose()
        {
            // Reset
            Cursor.Current = Cursors.Default;
            System.Windows.Forms.Application.UseWaitCursor = false;
        }
    }
}