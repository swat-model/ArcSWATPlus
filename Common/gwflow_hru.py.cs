



using ArcGIS.Desktop.Internal.Framework.Controls;
using System;


using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using static System.Net.Mime.MediaTypeNames;

namespace ArcSWATPlus
{

    public class GwflowHru
    {
#if TESTING
        public HRUsFormStub _parent;
#else
        public HRUsForm _parent;
#endif

#if TESTING
        public GwflowHru(HRUsFormStub parent) {
#else
        public GwflowHru(HRUsForm parent) {
#endif
            this._parent = parent;
            //this.setupUi(this);
            //this._connect();
        }

        //public virtual void _connect() {
        //    this.aquiferThicknessButton.clicked.connect(this._browseThickness);
        //    this.aquiferPermeabilityButton.clicked.connect(this._browsePermeability);
        //    this.tileDrainsButton.clicked.connect(this._browseTileDrains);
        //    this.useTileDrains.toggled.connect(this._toggleTile);
        //    this.structuredRadio.toggled.connect(this._toggleGridType);
        //    this.refineTopography.toggled.connect(this._toggleElevTolerance);
        //    this.transitionSlider.valueChanged.connect(v => this.transitionSpin.setValue(v / 10.0));
        //    this.transitionSpin.valueChanged.connect(v => this.transitionSlider.setValue(Convert.ToInt32(v * 10)));
        //    this.cellSize.valueChanged.connect(this._onCellSizeChanged);
        //    this.maxCellSize.valueChanged.connect(this._updateMinSizeLabel);
        //    this.refineLevels.valueChanged.connect(this._updateMinSizeLabel);
        //    this.generateGridButton.clicked.connect(this._requestGrid);
        //    this.aquiferThickness.textChanged.connect(this._checkValidity);
        //    this.aquiferPermeability.textChanged.connect(this._checkValidity);
        //}

        public virtual void saveToProject() {
            var proj = this._parent._gv.proj;
            var title = this._parent._gv.projName;
            proj.writeEntry(title, "gwflow/aquiferThickness", this._parent._aquiferThickness);
            proj.writeEntry(title, "gwflow/aquiferPermeability", this._parent._aquiferPermeability);
            proj.writeEntry(title, "gwflow/tileDrains", this._parent._tileDrains);
            proj.writeEntry("", "gwflow/gridType", this._parent.isStructured ? "structured" : "unstructured");
            proj.writeNumEntry(title, "gwflow/cellSize", this._parent._cellSize);
            proj.writeNumEntry(title, "gwflow/maxSize", this._parent._maxCellSize);
            proj.writeNumEntry(title, "gwflow/levels", this._parent._levels);
            proj.writeEntryDouble(title, "gwflow/transitionRate", this._parent.transitionRate);
            proj.writeEntryBool(title, "gwflow/refineStreams", this._parent._refineStreams);
            proj.writeEntryBool(title, "gwflow/refineTopography", this._parent._refineTopography);
            proj.writeEntryBool(title, "gwflow/refineBoundary", this._parent._refineBoundary);
            proj.writeEntryBool(title, "gwflow/refineWells", this._parent._refineWells);
            proj.writeEntryBool(title, "gwflow/useTileDrains", this._parent._useTileDrains);
            proj.writeEntryBool(title, "gwflow/alignGrids", this._parent.alignToGrid);
            proj.writeNumEntry(title, "gwflow/elevTolerance", this._parent._elevTolerance);
        }

        public virtual void loadFromProject() {
            var proj = this._parent._gv.proj;
            var title = this._parent._gv.projName;
            string strVal;
            bool found;
            int intVal;
            double doubleVal;
            bool val;
            (strVal, found) = proj.readEntry(title, "gwflow/aquiferThickness", "");
            if (found && strVal is not null) {
                this._parent._aquiferThickness = proj.readPath(strVal);
            }
            (strVal, found) = proj.readEntry(title, "gwflow/aquiferPermeability", "");
            if (found && strVal is not null) {
                this._parent._aquiferPermeability = proj.readPath(strVal);
            }
            (strVal, found) = proj.readEntry(title, "gwflow/tileDrains", "");
            if (found && strVal is not null) {
                this._parent._tileDrains = proj.readPath(strVal);
                this._parent._useTileDrains = true;
            }
            (strVal, _) = proj.readEntry("", "gwflow/gridType", "structured");
            this._parent.isStructured = strVal == "structured";
            this._parent._toggleGridType(this._parent.isStructured);
            (intVal, found) = proj.readNumEntry(title, "gwflow/cellSize", 0);
            if (found && intVal > 0) {
                this._parent._cellSize = intVal;
            }
            (intVal, found) = proj.readNumEntry(title, "gwflow/maxSize", 0);
            if (found && intVal > 0) {
                this._parent._maxCellSize = intVal;
            }
            (intVal, found) = proj.readNumEntry(title, "gwflow/levels", 0);
            if (found && intVal > 0) {
                this._parent._levels = intVal;
            }
            (doubleVal, found) = proj.readDoubleEntry(title, "gwflow/transitionRate", 0);
            if (found && doubleVal > 0) {
                this._parent.transitionRate = doubleVal;
            }
            (val, found) = proj.readBoolEntry(title, "gwflow/refineStreams", true);
            if (found) {
                this._parent._refineStreams = val;
            }
            (val, found) = proj.readBoolEntry(title, "gwflow/refineTopography", true);
            if (found) {
                this._parent._refineTopography = val;
            }
            (val, found) = proj.readBoolEntry(title, "gwflow/refineBoundary", true);
            if (found) {
                this._parent._refineBoundary = val;
            }
            (val, found) = proj.readBoolEntry(title, "gwflow/refineWells", false);
            if (found) {
                this._parent._refineWells = val;
            }
            (val, found) = proj.readBoolEntry(title, "gwflow/alignGrids", true);
            if (found) {
                this._parent.alignToGrid = val;
            }
            (intVal, found) = proj.readNumEntry(title, "gwflow/elevTolerance", 0);
            if (found && intVal > 0) {
                this._parent._elevTolerance = intVal;
            }
            this._parent._updateMinSizeLabel();
        }


        // TODO
        //public virtual bool isValid() {
        //    var t = this._parent._aquiferThickness;
        //    var p = this._parent._aquiferPermeability;
        //    return File.Exists(t) && File.Exists(p);
        //}

        // TODO
        //public virtual void cleanup() {
        //    if (this._parent._gridGen is not null) {
        //        this._parent._gridGen.cleanup();
        //    }
        //}
    }
}
