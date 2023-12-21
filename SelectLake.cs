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
    public partial class SelectLake : Form
    {
        private Delineation _parent;
        private bool _adding;
        public SelectLake(Delineation parent, bool adding) {
            InitializeComponent();
            this._parent = parent;
            this._adding = adding;
            if (!adding) {
                label1.Text = label1.Text.Replace("to add to the lake", "to remove from the lake");
            }
        }


        private void saveButton_Click(object sender, EventArgs e) {
            Close();
            _parent.selectLakes(true, this._adding);
        }

        private void cancelButton_Click(object sender, EventArgs e) {
            Close();
            _parent.selectLakes(false, this._adding);
        }
    }
}
