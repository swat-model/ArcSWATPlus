

using System.Collections;

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using ArcGIS.Desktop.Core;
using System.Runtime.InteropServices;

namespace ArcSWATPlus
{


    // Collect QSWAT parameters (location of SWATPlus directory and MPI) from user and save.
    public class Parameters
    {

        public ParamForm _dlg;

        public GlobalVars _gv;

        public bool isBatch;

        public string mpiexecDir;

        public string numProcesses;

        public string SWATPlusDir;

        public static bool _ISWIN = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public static string _HOMEDIR =
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
            Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%") :
            Environment.ExpandEnvironmentVariables("HOME");

        public static string _SWATPLUSDEFAULTDIR = _HOMEDIR + @"\SWATPlus";

        public static string _SWATEDITOR = "SWATPlusEditor.exe";

        public static string _SWATEDITORDIR = "SWATPlusEditor";

        public static string _MPIEXEC = "mpiexec.exe";

        public static string _MPIEXECDEFAULTDIR = @"C:\Program Files\Microsoft MPI\Bin";

        public static string _TAUDEM539DIR = "TauDEM539Bin";

        public static string _TAUDEMDIR = "TauDEM5Bin";

        public static string _TAUDEMHELP = "TauDEM_Tools.chm";

        public static string _TAUDEMDOCS = "http://hydrology.usu.edu/taudem/taudem5/documentation.html";

        public static string _SVGDIR = "apps/qgis/svg";

        public static string _DBDIR = "Databases";

        public static string _DBPROJ = "QSWATPlusProj.sqlite";

        public static string _DBREF = "swatplus_datasets.sqlite";

        public static string _DBPROJHAWQS = "QSWATPlusProjHAWQS.sqlite";

        public static string _DBREFHAWQS = "QSWATPlusRefHAWQS.sqlite";

        public static string _RESULTS = "Results";

        public static string _PLOTS = "Plots";

        public static string _TXTINOUT = "TxtInOut";

        public static string _SOILDB = "swatplus_soils.sqlite";

        public static string _SIM = "time.sim";

        public static string _PRT = "print.prt";

        public static string _OUTPUTDB = "swatplus_output.sqlite";

        public static string _SUBS = "subs";

        public static string _RIVS = "rivs";

        public static string _LSUS = "lsus";

        public static string _HRUS = "hrus";

        public static string _AQUIFERS = "aquifers";

        public static string _DEEPAQUIFERS = "deep_aquifers";

        public static string _SUBS1 = "subs1";

        public static string _RIVS1 = "rivs1";

        public static string _LSUS1 = "lsus1";

        public static string _LSUS2 = "lsus2";

        public static string _HRUS1 = "hrus1";

        public static string _HRUS2 = "hrus2";

        public static string _HRUSRASTER = "hrus.tif";

        public static string _HRUSCSV = "hrus.csv";

        public static string _ANIMATION = "Animation";

        public static string _PNG = "Png";

        public static string _STILLPNG = "still.png";

        public static string _SSURGODB_HUC = "SSURGO_Soils_HUC.sqlite";

        public static int _SSURGOWater = 377988;

        public static string _WATERBODIES = "WaterBodies.sqlite";

        public static string _TOPOREPORT = "TopoRep.txt";

        public static string _TOPOITEM = "Elevation";

        public static string _BASINREPORT = "LanduseSoilSlopeRepSwat.txt";

        public static string _BASINITEM = "Landuse and Soil";

        public static string _HRUSREPORT = "HruLanduseSoilSlopeRepSwat.txt";

        public static string _HRUSITEM = "HRUs";

        public static string _DRAINAGECSV = "drainage.csv";

        public static string _USECSV = "Use csv file";

        public static string _LANDUSE = "Landuse";

        public static string _SOIL = "Soil";

        public static string _SLOPEBAND = "SlopeBand";

        public static string _AREA = "Area";

        public static string _AREASQKM = "AREASQKM";

        public static string _PERCENTSUB = "%Subbasin";

        public static string _PERCENTLSU = "%Landscape";

        public static string _SQKM = "sq. km";

        public static string _HECTARES = "hectares";

        public static string _SQMETRES = "sq. metres";

        public static string _SQMILES = "sq. miles";

        public static string _ACRES = "acres";

        public static string _SQFEET = "sq. feet";

        public static string _METRES = "metres";

        public static string _FEET = "feet";

        public static string _CM = "centimetres";

        public static string _MM = "millimetres";

        public static string _INCHES = "inches";

        public static string _YARDS = "yards";

        public static string _DEGREES = "degrees";

        public static string _UNKNOWN = "unknown";

        public static double _FEETTOMETRES = 0.3048;

        public static double _CMTOMETRES = 0.01;

        public static double _MMTOMETRES = 0.001;

        public static double _INCHESTOMETRES = 0.0254;

        public static double _YARDSTOMETRES = 0.91441;

        public static double _SQMILESTOSQMETRES = 2589988.1;

        public static double _ACRESTOSQMETRES = 4046.8564;

        public static double _SQMETRESTOSQFEET = 10.76391;

        public static int _DEFAULTFONTSIZE = 9;

        public static int _PLOTFONTSIZE = 9;

        public static int _RIVS1SUBS1MAX = 100000;

        public static int _GRIDCELLSMAX = 10000;

        public static double _NEARNESSTHRESHOLD = 0.51;

        public static double _DEFAULTSLOPE = 0.005;

        public static double _CHANNELWIDTHMULTIPLIER = 1.29;

        public static double _CHANNELWIDTHEXPONENT = 0.6;

        public static double _CHANNELDEPTHMULTIPLIER = 0.13;

        public static double _CHANNELDEPTHEXPONENT = 0.4;

        public static double _BURNINDEPTH = 50;

        public static double _UPSLOPEHRUDRAIN = 90;

        public static double _LAKEOUTLETCHANNELAREA = 1;

        public static double _LAKEOUTLETCHANNELLENGTH = 5;

        public static double _MULTIPLIER = 1.0;

        public static List<string> _WATERLANDUSES = new List<string> {
            "WATR",
            "WETN",
            "WETF",
            "WETW",
            "RIWF",
            "UPWF",
            "RIWN",
            "UPWN"
        };

        public static double _WATERMAXSLOPE = 0.001;

        public static double _MAXPENWIDTH = 4.0; // for QGIS this is 2

        //  percentage of flow for major when there is a bifurcation: HUC and HAWQS models only
        public static int _MAJORPERCENT = 60;

        public Parameters(GlobalVars gv) {
            this.SWATPlusDir = Parameters._SWATPLUSDEFAULTDIR;
            //# mpiexec directory
            this.mpiexecDir = Parameters._MPIEXECDEFAULTDIR;
            //# number of MPI processes
            this.numProcesses = "0";
            this._gv = gv;
            this._dlg = new ParamForm(this);
            if (this._gv is not null) {
                this.isBatch = this._gv.isBatch;
            } else {
                this.isBatch = false;
            }
        }

        // Run the form.
        public void run() {
            if (Directory.Exists(this.mpiexecDir)) {
                this._dlg.useMPI = true;
            } else {
                this._dlg.useMPI = false;
            }
            this._dlg.MPIText = this.mpiexecDir;
            this._dlg.SWATPlusText = this.SWATPlusDir;
            this.readProj();
            if (!this.isBatch) {
                this._dlg.ShowDialog();
            }
            if (this._gv is not null) {
                this._gv.parametersPos = this._dlg.Location;
            }
        }

        // Save parameters and close form.
        public virtual void save() {
            var SWATPlusDir = this._dlg.SWATPlusText;
            if (SWATPlusDir == "" || !Directory.Exists(SWATPlusDir)) {
                Utils.error("Please select the SWATPlus directory", this.isBatch);
                return;
            }
            //         dbProjTemplate =  Utils.join(Parameters._DBDIR, Parameters._DBPROJ)
            //         if not File.Exists(dbProjTemplate):
            //             Utils.error('Cannot find the default project database {0}', dbProjTemplate), self.isBatch)
            //             return
            //         dbRefTemplate =  Utils.join(Parameters._DBDIR, Parameters._DBREF)
            //         if not File.Exists(dbRefTemplate):
            //             Utils.error('Cannot find the SWAT parameter database {0}', dbRefTemplate), self.isBatch)
            //             return
            //         TauDEMDir = Parameters._TAUDEMDIR
            //         if not os.path.isdir(TauDEMDir):
            //             Utils.error('Cannot find the TauDEM directory {0}', TauDEMDir), self.isBatch)
            //             return
            string mpiexecPath = "";
            if (this._dlg.useMPI) {
                var mpiexecDir = this._dlg.MPIText;
                if (mpiexecDir == "" || !Directory.Exists(mpiexecDir)) {
                    Utils.error("Please select the MPI bin directory", this.isBatch);
                    return;
                }
                var mpiexec = Parameters._MPIEXEC;
                mpiexecPath = Utils.join(mpiexecDir, mpiexec);
                if (!Directory.Exists(mpiexecPath)) {
                    Utils.error(string.Format("Cannot find mpiexec program {0}", mpiexecPath), this.isBatch);
                    return;
                }
            }
            // no problems - save parameters
            if (this._gv is not null) {
                if (this._gv.SWATPlusDir == "") {
                    // failed earlier - need to restart project
                    Utils.information("The project needs to be restarted: please click the Existing Project button", this.isBatch);
                    this._dlg.Close();
                    return;
                }
                this._gv.mpiexecPath = this._dlg.useMPI ? mpiexecPath : "";
                this._gv.SWATPlusDir = SWATPlusDir;
            }
            this.saveProj();
            if (this._dlg.useMPI) {
                if (this.numProcesses == "") {
                    this.numProcesses = "8";
                }
            } else {
                if (this.numProcesses == "") {
                    this.numProcesses = "0";
                }
            }
            // careful about changing font size too much
            var pointSize = this._dlg.pointSizeValue;
            MessageBoxResult result = MessageBoxResult.OK;
            if (8 > pointSize || pointSize > 12) {
                result = Utils.question(string.Format("Are you sure you want to set the QSWATPlus point size to {0}?", pointSize), this.isBatch, false);
            }
            if (result == MessageBoxResult.OK) {
                this._gv.fontSize = pointSize;
                this._gv.SetFontSize(this._dlg);
            }
            this._dlg.Close();
        }

        // Choose SWATPlus directory.
        public void chooseSWATPlusDir() {
            var title = Utils.trans("Select SWATPlus directory");
            string startDir = null;
            if (this._dlg.SWATPlusText != "") {
                startDir = Path.GetDirectoryName(this._dlg.SWATPlusText);
            } else if (Directory.Exists(this.SWATPlusDir)) {
                startDir = Path.GetDirectoryName(this.SWATPlusDir);
            }
            using (var dlg = new FolderBrowserDialog()) {
                dlg.Description = title;
                dlg.UseDescriptionForTitle= true;
                if (startDir is not null) {
                    dlg.InitialDirectory = startDir;
                }
                var result = dlg.ShowDialog();
                if (result == DialogResult.OK) {
                    this.SWATPlusDir = dlg.SelectedPath;
                    this._dlg.SWATPlusText = this.SWATPlusDir;
                }
            }
        }

        // Choose MPI directory.
        public void chooseMPIDir() {
            var title = Utils.trans("Select MPI bin directory");
            string startDir = null;
            if (this._dlg.MPIText != "") {
                startDir = Path.GetDirectoryName(this._dlg.MPIText);
            } else if (Directory.Exists(this.mpiexecDir)) {
                startDir = Path.GetDirectoryName(mpiexecDir);
            }
            using (var dlg = new FolderBrowserDialog()) {
                dlg.Description = title;
                dlg.UseDescriptionForTitle = true;
                if (startDir is not null) {
                    dlg.InitialDirectory = startDir;
                }
                var result = dlg.ShowDialog();
                if (result == DialogResult.OK) {
                    this.mpiexecDir = dlg.SelectedPath;
                    this._dlg.MPIText = this.mpiexecDir;
                }
            }
        }

        // Change font size of this form.  
        public void changeFontSize() {
            this._gv.SetFontSize(this._dlg);
        }

        // Read parameter data from project file.
        public void readProj() {
            var proj = this._gv.proj;
            var title = proj.Name;
            var burninDepth = proj.readDoubleEntry(title, "params/burninDepth", Parameters._BURNINDEPTH).Item1;
            this._dlg.burninDepthValue = burninDepth;
            var channelWidthMultiplier = proj.readDoubleEntry(title, "params/channelWidthMultiplier", Parameters._CHANNELWIDTHMULTIPLIER).Item1;
            this._dlg.widthMultValue = channelWidthMultiplier;
            var channelWidthExponent = proj.readDoubleEntry(title, "params/channelWidthExponent", Parameters._CHANNELWIDTHEXPONENT).Item1;
            this._dlg.widthExpValue = channelWidthExponent;
            var channelDepthMultiplier = proj.readDoubleEntry(title, "params/channelDepthMultiplier", Parameters._CHANNELDEPTHMULTIPLIER).Item1;
            this._dlg.depthMultValue = channelDepthMultiplier;
            var channelDepthExponent = proj.readDoubleEntry(title, "params/channelDepthExponent", Parameters._CHANNELDEPTHEXPONENT).Item1;
            this._dlg.depthExpValue = channelDepthExponent;
            var reachSlopeMultiplier = proj.readDoubleEntry(title, "params/reachSlopeMultiplier", Parameters._MULTIPLIER).Item1;
            this._dlg.reachSlope = reachSlopeMultiplier;
            var tributarySlopeMultiplier = proj.readDoubleEntry(title, "params/tributarySlopeMultiplier", Parameters._MULTIPLIER).Item1;
            this._dlg.tributarySlope = tributarySlopeMultiplier;
            var meanSlopeMultiplier = proj.readDoubleEntry(title, "params/meanSlopeMultiplier", Parameters._MULTIPLIER).Item1;
            this._dlg.meanSlope = meanSlopeMultiplier;
            var mainLengthMultiplier = proj.readDoubleEntry(title, "params/mainLengthMultiplier", Parameters._MULTIPLIER).Item1;
            this._dlg.mainLength = mainLengthMultiplier;
            var tributaryLengthMultiplier = proj.readDoubleEntry(title, "params/tributaryLengthMultiplier", Parameters._MULTIPLIER).Item1;
            this._dlg.tributaryLength = tributaryLengthMultiplier;
            var upslopeHRUDrain = proj.readDoubleEntry(title, "params/upslopeHRUDrain", Parameters._UPSLOPEHRUDRAIN).Item1;
            this._dlg.upslopeHRUDrainValue = upslopeHRUDrain;
        }

        // Write parameter data to project file.
        public void saveProj() {
            var burninDepth = this._dlg.burninDepthValue;
            if (burninDepth == 0) {
                Utils.error("Stream burn-in depth may not be set to zero", this.isBatch);
                return;
            }
            var widthMult = this._dlg.widthMultValue;
            if (widthMult == 0) {
                Utils.error("Channel width multiplier may not be set to zero", this.isBatch);
                return;
            }
            var depthMult = this._dlg.depthMultValue;
            if (depthMult == 0) {
                Utils.error("Channel depth multiplier may not be set to zero", this.isBatch);
                return;
            }
            var widthExp = this._dlg.widthExpValue;
            if (widthExp == 0) {
                Utils.error("Channel width exponent may not be set to zero", this.isBatch);
                return;
            }
            var depthExp = this._dlg.depthExpValue;
            if (depthExp == 0) {
                Utils.error("Channel depth exponent may not be set to zero", this.isBatch);
                return;
            }
            var reachSlopeMultiplier = this._dlg.reachSlope;
            if (reachSlopeMultiplier == 0) {
                Utils.error("Reach slopes multiplier may not be set to zero", this.isBatch);
                return;
            }
            var tributarySlopeMultiplier = this._dlg.tributarySlope;
            if (tributarySlopeMultiplier == 0) {
                Utils.error("Tributary slopes multiplier may not be set to zero", this.isBatch);
                return;
            }
            var meanSlopeMultiplier = this._dlg.meanSlope;
            if (meanSlopeMultiplier == 0) {
                Utils.error("Mean slopes multiplier may not be set to zero", this.isBatch);
                return;
            }
            var mainLengthMultiplier = this._dlg.mainLength;
            if (mainLengthMultiplier == 0) {
                Utils.error("Main channel length multiplier may not be set to zero", this.isBatch);
                return;
            }
            var tributaryLengthMultiplier = this._dlg.tributaryLength;
            if (tributaryLengthMultiplier == 0) {
                Utils.error("Tributary channel length multiplier may not be set to zero", this.isBatch);
                return;
            }
            var upslopeHRUDrain = this._dlg.upslopeHRUDrainValue;
            if (0 > upslopeHRUDrain || upslopeHRUDrain > 100) {
                Utils.error("Upslope HRU drain must be in the range 0 to 100", this.isBatch);
                return;
            }
            var proj = this._gv.proj;
            var title = proj.title;
            proj.writeEntryDouble(title, "params/burninDepth", burninDepth);
            proj.writeEntryDouble(title, "params/channelWidthMultiplier", widthMult);
            proj.writeEntryDouble(title, "params/channelWidthExponent", widthExp);
            proj.writeEntryDouble(title, "params/channelDepthMultiplier", depthMult);
            proj.writeEntryDouble(title, "params/channelDepthExponent", depthExp);
            proj.writeEntryDouble(title, "params/reachSlopeMultiplier", reachSlopeMultiplier);
            proj.writeEntryDouble(title, "params/tributarySlopeMultiplier", tributarySlopeMultiplier);
            proj.writeEntryDouble(title, "params/meanSlopeMultiplier", meanSlopeMultiplier);
            proj.writeEntryDouble(title, "params/mainLengthMultiplier", mainLengthMultiplier);
            proj.writeEntryDouble(title, "params/tributaryLengthMultiplier", tributaryLengthMultiplier);
            proj.writeEntryDouble(title, "params/upslopeHRUDrain", upslopeHRUDrain);
            if (this._gv is not null) {
                this._gv.burninDepth = burninDepth;
                // update channel widths and depths in affected tables
                if (Math.Abs(this._gv.channelWidthMultiplier - widthMult) > 0.005 || Math.Abs(this._gv.channelWidthExponent - widthExp) > 0.005 || Math.Abs(this._gv.channelDepthMultiplier - depthMult) > 0.005 || Math.Abs(this._gv.channelDepthExponent - depthExp) > 0.005) {
                    this._gv.db.changeChannelWidthAndDepth(widthMult, widthExp, depthMult, depthExp, this._gv.shapesDir);
                }
                this._gv.channelWidthMultiplier = widthMult;
                this._gv.channelWidthExponent = widthExp;
                this._gv.channelDepthMultiplier = depthMult;
                this._gv.channelDepthExponent = depthExp;
                // update slope values in affected tables
                // avoid real equality test
                if (Math.Abs(this._gv.reachSlopeMultiplier - reachSlopeMultiplier) > 0.05) {
                    this._gv.db.changeReachSlopes(reachSlopeMultiplier, this._gv.reachSlopeMultiplier, this._gv.shapesDir);
                }
                if (Math.Abs(this._gv.tributarySlopeMultiplier - tributarySlopeMultiplier) > 0.05) {
                    this._gv.db.changeTributarySlopes(tributarySlopeMultiplier, this._gv.tributarySlopeMultiplier, this._gv.shapesDir);
                }
                if (Math.Abs(this._gv.meanSlopeMultiplier - meanSlopeMultiplier) > 0.05) {
                    this._gv.db.changeMeanSlopes(meanSlopeMultiplier, this._gv.meanSlopeMultiplier, this._gv.shapesDir);
                }
                if (Math.Abs(this._gv.mainLengthMultiplier - mainLengthMultiplier) > 0.05) {
                    this._gv.db.changeMainLengths(mainLengthMultiplier, this._gv.mainLengthMultiplier, this._gv.shapesDir);
                }
                if (Math.Abs(this._gv.tributaryLengthMultiplier - tributaryLengthMultiplier) > 0.05) {
                    this._gv.db.changeTributaryLengths(tributaryLengthMultiplier, this._gv.tributaryLengthMultiplier, this._gv.shapesDir);
                }
                this._gv.reachSlopeMultiplier = reachSlopeMultiplier;
                this._gv.tributarySlopeMultiplier = tributarySlopeMultiplier;
                this._gv.meanSlopeMultiplier = meanSlopeMultiplier;
                this._gv.mainLengthMultiplier = mainLengthMultiplier;
                this._gv.tributaryLengthMultiplier = tributaryLengthMultiplier;
                // update upslopeHRUDrain       
                if (upslopeHRUDrain != this._gv.upslopeHRUDrain) {
                    this._gv.upslopeHRUDrain = upslopeHRUDrain;
                    if (this._gv.useLandscapes) {
                        this._gv.db.changeUpslopeHRUDrain(upslopeHRUDrain);
                    }
                }
            }
        }
    }
}
