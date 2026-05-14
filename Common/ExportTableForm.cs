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
    public partial class ExportTableForm : Form
    {
        private ExportTable _parent;


        public ExportTableForm(ExportTable parent) {
            InitializeComponent();
            this._parent = parent;
            parent._gv.SetFontSize(this);
        }

        private void closeButton_Click(object sender, EventArgs e) {
            this._parent.close();
        }

        private void exportButton_Click(object sender, EventArgs e) {
            this._parent.exportTable();
        }

        private void databaseBox_SelectedIndexChanged(object sender, EventArgs e) {
            this._parent.setTables();
        }

        public ComboBox databases {
           get { return databaseBox; }
        }

        public ComboBox tables {
            get { return tableBox; }
        }
    }
}
