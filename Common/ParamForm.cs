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
    public partial class ParamForm : Form
    {
        private Parameters _parent;

        public ParamForm(Parameters parent) {
            InitializeComponent();
            this._parent = parent;
            parent._gv.SetFontSize(this);
        }

        public String SWATPlusText {
            get { return this.SWATPlusBox.Text; }
            set { this.SWATPlusBox.Text = value; }
        }

        public String MPIText {
            get { return this.MPIBox.Text; }
            set { this.MPIBox.Text = value; }
        }

        public bool useMPI {
            get { return this.checkUseMPI.Checked; }
            set { checkUseMPI.Checked = value; }
        }

        public int pointSizeValue {
            get { return Convert.ToInt32(this.pointSizeBox.Value); }
            set { this.pointSizeBox.Value = value; }
        }

        public int decimalPlacesValue {
            get { return Convert.ToInt32(this.decimalPlaces.Value); }
            set { this.decimalPlaces.Value = value; }
        }

        public double burninDepthValue {
            get { return Convert.ToDouble(this.burninDepth.Text); }
            set { this.burninDepth.Text = value.ToString(); }
        }

        public double widthMultValue {
            get { return Convert.ToDouble(this.widthMult.Text); }
            set { this.widthMult.Text = value.ToString(); }
        }

        public double widthExpValue {
            get { return Convert.ToDouble(this.widthExp.Text); }
            set { this.widthExp.Text = value.ToString(); }
        }

        public double depthMultValue {
            get { return Convert.ToDouble(this.depthMult.Text); }
            set { this.depthMult.Text = value.ToString(); }
        }

        public double depthExpValue {
            get { return Convert.ToDouble(this.depthExp.Text); }
            set { this.depthExp.Text = value.ToString(); }
        }

        public double reachSlope {
            get { return Convert.ToDouble(this.reachSlopeMultiplier.Text); }
            set { this.reachSlopeMultiplier.Text = value.ToString(); }
        }

        public double tributarySlope {
            get { return Convert.ToDouble(this.tributarySlopeMultiplier.Text); }
            set { this.tributarySlopeMultiplier.Text = value.ToString(); }
        }

        public double meanSlope {
            get { return Convert.ToDouble(this.meanSlopeMultiplier.Text); }
            set { this.meanSlopeMultiplier.Text = value.ToString(); }
        }

        public double mainLength {
            get { return Convert.ToDouble(this.mainLengthMultiplier.Text); }
            set { this.mainLengthMultiplier.Text = value.ToString(); }
        }

        public double tributaryLength {
            get { return Convert.ToDouble(this.tributaryLengthMultiplier.Text); }
            set { this.tributaryLengthMultiplier.Text = value.ToString(); }
        }

        public double upslopeHRUDrainValue {
            get { return Convert.ToDouble(this.upslopeHRUDrain.Text); }
            set { this.upslopeHRUDrain.Text = value.ToString(); }
        }

        private void checkUseMPI_CheckedChanged(object sender, EventArgs e) {
            if (this.checkUseMPI.Checked) {
                this.MPIBox.Enabled = true;
                this.MPIButton.Enabled = true;
                this.MPILabel.Enabled = true;
            } else {
                this.MPIBox.Enabled = false;
                this.MPIButton.Enabled = false;
                this.MPILabel.Enabled = false;
            }
        }

        private void SWATPlusButton_Click(object sender, EventArgs e) {
            this._parent.chooseSWATPlusDir();
        }

        private void MPIButton_Click(object sender, EventArgs e) {
            this._parent.chooseMPIDir();
        }

        private void cancelButton_Click(object sender, EventArgs e) {
            this.Close();
        }

        private void saveButton_Click(object sender, EventArgs e) {
            this._parent.save();
        }

        private void pointSizeBox_ValueChanged(object sender, EventArgs e) {
            this._parent.changeFontSize();
        }
    }
}
