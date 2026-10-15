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
    public partial class CompareScenariosForm : Form
    {

        private Visualise _parent;
        public CompareScenariosForm(Visualise parent) {
            InitializeComponent();
            this._parent = parent;
        }

        private void okButton_Click(object sender, EventArgs e) {
            this._parent.setupCompareScenarios();
        }

        private void cancelButton_Click(object sender, EventArgs e) {
            this._parent.closeCompareScenarios();
        }

        //public ComboBox Pscenario1 => scenario1;
        public Func<object, int> Pscenario1Add => scenario1.Items.Add;
        public object Pscenario1Item => scenario1.SelectedItem;
        //public ComboBox Pscenario2 => scenario2;
        public Func<object, int> Pscenario2Add => scenario2.Items.Add;
        public object Pscenario2Item => scenario2.SelectedItem;
    }
}
