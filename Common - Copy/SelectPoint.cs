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
    public partial class SelectPoint : Form
    {
        private Delineation _parent;
        public SelectPoint(Delineation parent) {
            InitializeComponent();
            this._parent = parent;
        }

        private async void saveButton_Click(object sender, EventArgs e) {
            Close();
            await this._parent.selectPoints(true);
        }

        private async void cancelButton_Click(object sender, EventArgs e) {
            Close();
            await this._parent.selectPoints(false);
        }
    }
}
