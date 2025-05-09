using ArcGIS.Core.Internal.CIM;
using ArcGIS.Desktop.Internal.Mapping;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArcSWATPlus
{
    public partial class LandscapeForm : Form
    {
        GlobalVars _gv;
        Landscape _parent;
        double areaOfCell;
        bool changing;

        public LandscapeForm(GlobalVars gv, Landscape parent) {
            InitializeComponent();
            this._gv = gv;
            this._parent = parent;
        }

        public void init(int numCellsSt, int branchThresh) {
            // for now hillslopes not available, and form will assume floodplain
            this.hillslopesCheckBox.Visible = false;
            this.floodplainCheckBox.Visible = false;
            // self.hillslopesCheckBox.stateChanged.connect(self.hillslopesCheck)
            // self.floodplainCheckBox.stateChanged.connect(self.floodplainCheck)
            //this.ridgeThresholdCells.textChanged.connect(this.setRidgeArea);
            //this.ridgeThresholdArea.textChanged.connect(this.setRidgeCells);
            this.areaUnitsBox.Items.Add(Parameters._SQKM);
            this.areaUnitsBox.Items.Add(Parameters._HECTARES);
            this.areaUnitsBox.Items.Add(Parameters._SQMETRES);
            this.areaUnitsBox.Items.Add(Parameters._SQMILES);
            this.areaUnitsBox.Items.Add(Parameters._ACRES);
            this.areaUnitsBox.Items.Add(Parameters._SQFEET);
            //this.areaUnitsBox.activated.connect(this.changeRidgeArea);
            //this.methodTab.currentChanged.connect(this.methodTabCheck);
            //this.branchThreshold.textChanged.connect(this.parameterChange);
            //this.createButton.clicked.connect(this.generate);
            //this.doneButton.clicked.connect(this.cancel);
            // if distances to outlets not generated (eg existing watershed) then branch method is not available
            if (!System.IO.File.Exists(this._gv.distStFile)) {
                this.methodTab.TabPages.RemoveAt(2);
            }
            this.bufferMultiplier.Value = 10;
            this.areaUnitsBox.SelectedIndex = 0;
            this.ridgeThresholdCells.Text = numCellsSt.ToString();
            this.changeRidgeArea();
            this.branchThreshold.Text = branchThresh.ToString();
            this.slopePositionSpinBox.Value = (decimal)0.1;
            this.methodTab.SelectedIndex = 1;
            this.methodTabCheck();
        }

        private void ridgeThresholdCells_TextChanged(object sender, EventArgs e) {
            this.setRidgeArea();
        }

        private void ridgeThresholdArea_TextChanged(object sender, EventArgs e) {
            this.setRidgeCells();
        }

        private void areaUnitsBox_SelectedIndexChanged(object sender, EventArgs e) {
            this.changeRidgeArea();
        }

        private void methodTab_TabIndexChanged(object sender, EventArgs e) {
            this.methodTabCheck();
        }

        private void branchThreshold_TextChanged(object sender, EventArgs e) {
            this._parent.parameterChange();
        }

        private async void createButton_Click(object sender, EventArgs e) {
            await this._parent.generate();
        }

        private void doneButton_Click(object sender, EventArgs e) {
            this._parent.cancel();
        }

        // Set area of cell in currently selected area units.
        public virtual void setAreaOfCell() {
            var areaSqM = this._gv.topo.dx * this._gv.topo.dy;
            this.areaOfCell = areaSqM;
            if (this.areaUnitsBox.Text == Parameters._SQKM) {
                this.areaOfCell = areaSqM / 1000000.0;
            } else if (this.areaUnitsBox.Text == Parameters._HECTARES) {
                this.areaOfCell = areaSqM / 10000.0;
            } else if (this.areaUnitsBox.Text == Parameters._SQMETRES) {
                this.areaOfCell = areaSqM;
            } else if (this.areaUnitsBox.Text == Parameters._SQMILES) {
                this.areaOfCell = areaSqM / Parameters._SQMILESTOSQMETRES;
            } else if (this.areaUnitsBox.Text == Parameters._ACRES) {
                this.areaOfCell = areaSqM / Parameters._ACRESTOSQMETRES;
            } else if (this.areaUnitsBox.Text == Parameters._SQFEET) {
                this.areaOfCell = areaSqM * Parameters._SQMETRESTOSQFEET;
            }
        }

        // Update area threshold display.
        public virtual void setRidgeArea() {
            if (this.changing) {
                return;
            }
            int numCells;
            try {
                numCells = Convert.ToInt32(this.ridgeThresholdCells.Text);
            }
            catch (Exception) {
                // not currently parsable - ignore
                return;
            }
            var area = numCells * this.areaOfCell;
            this.changing = true;
            this.ridgeThresholdArea.Text = string.Format("{0:G4}", area);
            this.changing = false;
            this._parent.parameterChange();
        }

        // Update number of cells threshold display.
        public virtual void setRidgeCells() {
            if (this.changing) {
                return;
            }
            // prevent division by zero
            if (this.areaOfCell == 0) {
                return;
            }
            double area;
            try {
                area = Convert.ToDouble(this.ridgeThresholdArea.Text);
            }
            catch (Exception) {
                // not currently parsable - ignore
                return;
            }
            var numCells = Convert.ToInt32(area / this.areaOfCell);
            this.changing = true;
            this.ridgeThresholdCells.Text = numCells.ToString();
            this.changing = false;
            this._parent.parameterChange();
        }

        // Set area of cell and update area threshold display.
        public virtual void changeRidgeArea() {
            this.setAreaOfCell();
            this.setRidgeArea();
        }

        // Changes to reflect different method selections.
        public virtual void methodTabCheck() {
            if (this.methodTab.SelectedIndex == 0) {
                // buffer method
                this.slopePositionSpinBox.Visible = false;
                this.slopePositionLabel.Visible = false;
            } else {
                this.slopePositionSpinBox.Visible = true;
                this.slopePositionLabel.Visible = true;
            }
        }

        public int bufferMultiplierValue {
            get { return (int)this.bufferMultiplier.Value; }
        }

        public int methodTabSelectedIndex {
            get { return this.methodTab.SelectedIndex; }
        }

        public string ridgeThresholdCellsText {
            get { return this.ridgeThresholdCells.Text; }
        }

        public string branchThresholdText {
            get { return this.branchThreshold.Text; }
        }

        public float slopePositionSpinBoxValue {
            get { return (float)this.slopePositionSpinBox.Value; }
        }
    }
}
