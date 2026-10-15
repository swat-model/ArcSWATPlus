using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using ComboBox = System.Windows.Forms.ComboBox;
using TextBox = System.Windows.Forms.TextBox;
using System.Windows.Media.Animation;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;

namespace ArcSWATPlus
{
    public partial class VisualiseForm : Form
    {
        private Visualise _parent;

        public VisualiseForm(Visualise parent) {
            InitializeComponent();
            this._parent = parent;
        }

        public void removeAnimationMode() {
            this.animationMode.Visible = false;
            this.composeOptions.Visible = false;
        }

        public void setCompare(bool enabled) {
            this.compareGroup.Visible = enabled;
        }

        public ComboBox PscenariosCombo => this.scenariosCombo;

        //public ComboBox.ObjectCollection PscenariosComboItems => this.scenariosCombo.Items;
        public object PscenariosComboItem => this.scenariosCombo.SelectedItem;
        public string PscenariosComboText => this.scenariosCombo.Text;
        public object getScenariosComboItem(int i) {
            return this.scenariosCombo.Items[i];
        }
        public void setScenariosComboItem(int i, string item) {
            this.scenariosCombo.Items[i] = item;
        }

        public ComboBox PoutputCombo => this.outputCombo;

        public  ComboBox PvariablePlot => this.variablePlot;

        //public TabControl PtabWidget => this.tabWidget;

        public int PtabWidgetIndex {
            get { return this.tabWidget.SelectedIndex; }
            set { this.tabWidget.SelectedIndex = value; }
        }

        //public TabControl PQtabWidget => this.QtabWidget;

        public Timer Ptimer => this.timer;

        public TextBox PobservedFileEdit => this.observedFileEdit;

        public ComboBox PstartDay => this.startDay;

        public ComboBox PfinishDay => this.finishDay;

        public ComboBox PstartMonth => this.startMonth;
        public ComboBox PfinishMonth => this.finishMonth;

        public TextBox PstartYear => this.startYear;

        public TextBox PfinishYear => this.finishYear;

        //public DataGridView PtableWidget => this.tableWidget;
        public DataTable PtableWidgetDataSource {
            get { return this.tableWidget.DataSource as DataTable; }
            set { this.tableWidget.DataSource = value; }
        }
        public void setDataGridColumnWidth(int col, int width) {
            this.tableWidget.Columns[col].Width = width; 
        }
        public bool hasSelectedDataRow() {
            return this.tableWidget.CurrentRow != null;
        }

        public DataGridViewRow PtableWidgetCurrentRow => this.tableWidget.CurrentRow;
        public int PtableWidgetCurrentColumn => this.tableWidget.CurrentCell.ColumnIndex;
        public DataGridViewRow PtableWidgetCurrentCellOwningRow => this.tableWidget.CurrentCell.OwningRow;
        public int PtableWidgetRowCount => this.tableWidget.Rows.Count;
        public DataGridViewCell PtableWidgetCurrentCell {
            set { this.tableWidget.CurrentCell = value; }
        }
        public DataGridViewCell PtableWidgetRowCell(int row, int cell) {
            return this.tableWidget.Rows[row].Cells[cell];
        }

        public ComboBox PplotType => this.plotType;

        public TextBox PresultsFileEdit => this.resultsFileEdit;

        public ComboBox PvariableCombo => this.variableCombo;

        public ComboBox PanimationVariableCombo => this.animationVariableCombo;

        //public ListBox PvariableList => this.variableList; 
        public Action PvariableListClear => this.variableList.Items.Clear;
        public int PvariableListCount => this.variableList.Items.Count;
        public int PvariableListSelectedCount => this.variableList.SelectedItems.Count;
        public Func<string, int> PvariableListAdd => this.variableList.Items.Add;
        public object PvariableListItem => this.variableList.SelectedItem;
        public Func<string, int> PvariableListIndexOf => this.variableList.Items.IndexOf;
        public Action<int> PvariableListRemoveAt => this.variableList.Items.RemoveAt;
        public object getVariableListItem(int i) {
            return this.variableList.Items[i];
        }
        public Func<string, int> PvariableListFindString => this.variableList.FindString;

        public ComboBox PsummaryCombo => this.summaryCombo;

        //public GroupBox PsummaryBox => this.summaryBox;
        public bool PsummaryBoxVisible {
            get { return this.summaryBox.Visible; }
            set { this.summaryBox.Visible = value; }
        }

        //public ComboBox PunitPlot => this.unitPlot;
        public string PunitPlotText => this.unitPlot.Text;

        public object PunitPlotItem => this.unitPlot.SelectedItem;
        public object getUnitPlotItem(int i) {
            return this.unitPlot.Items[i];
        }
        public Action PunitPlotClear => this.unitPlot.Items.Clear;  
        public Func<string, int> PunitPlotFindStringExact => this.unitPlot.FindStringExact;
        public int PunitPlotIndex {
            get { return this.unitPlot.SelectedIndex; }
            set { this.unitPlot.SelectedIndex = value; }
        }
        public Func<string, int> PunitPlotAdd => this.unitPlot.Items.Add;

        //public TextBox PunitEdit => this.unitEdit;
        public string PunitEditText {
            get { return this.unitEdit.Text; }
            set { this.unitEdit.Text = value; }
        }

        //public RadioButton PcurrentAnimation => this.currentAnimation;

        //public RadioButton PprintAnimation => this.printAnimation;
        public bool PprintAnimationChecked => this.printAnimation.Checked;

        //public Label PdateLabel=> this.dateLabel;
        public string PdateLabelText {
            get { return this.dateLabel.Text; }
            set { this.dateLabel.Text = value; }
        }
        public Action PdateLabelRefresh => this.dateLabel.Refresh;

        //public RadioButton PcanvasAnimation => this.canvasAnimation;

        //public RadioButton PlandscapeButton => this.landscapeButton;
        public bool PlandscapeButtonChecked => this.landscapeButton.Checked;

        //public NumericUpDown PprintCount => this.printCount;
        public decimal PprintCountValue => this.printCount.Value;

        //public GroupBox PcomposeOptions => this.composeOptions;
        public bool PcomposeOptionsVisible {
            set { this.composeOptions.Visible = value; }
        }

        //public NumericUpDown PcomposeCount => this.composeCount;
        public decimal PcomposeCountValue {
            set { this.composeCount.Value = value; }
        }

        //public TrackBar Pslider => this.slider;
        public int PsliderValue {
            get { return (int)this.slider.Value; }
            set { this.slider.Value = value; }
        }
        public int PsliderMinimum {
            get { return (int)this.slider.Minimum; }
            set { this.slider.Minimum = value; }
        }
        public int PsliderMaximum {
            get { return (int)this.slider.Maximum; }
            set { this.slider.Maximum = value; }
        }

        //public NumericUpDown PspinBox => this.spinBox;
        public decimal PspinBoxValue => this.spinBox.Value;

        //public Button PrecordButton => this.recordButton;
        public Color PrecordButtonBackColor {
            set { this.recordButton.BackColor = value; }
        }

        //public Button PplayButton => this.playButton;
        public bool PplayButtonEnabled {
            set { this.playButton.Enabled = value; }
        }

        //public Label PcompareLabel => this.compareLabel;
        public string PcompareLabelText {
            set { this.compareLabel.Text = value; }
        }

        //public Label PrecordLabel => this.recordLabel;
        public string PrecordLabelText {
            set { this.recordLabel.Text = value; }
        }

        //public RadioButton PcurrentResultsMap => this.currentResultsMap;
        public bool PcurrentResultsMapChecked => this.currentResultsMap.Checked;

        //public RadioButton PnewResultsMap => this.newResultsMap;

        //public ToolTip PtoolTip => this.toolTip;

        //public Label PunitLabel => this.unitLabel;
        public string PunitLabelText {
            set { this.unitLabel.Text = value; }
        }

        //public NumericUpDown PQqSpin => this.QqSpin;
        public decimal PQqSpinValue => this.QqSpin.Value;

        //public NumericUpDown PdQpSpinP => this.dQpSpinP;
        public bool PdQpSpinPEnabled {
            set { this.dQpSpinP.Enabled = value; }
        }
        public decimal PdQpSpinPValue => this.dQpSpinP.Value;

        public NumericUpDown PdQpSpinD => this.dQpSpinD;
        public decimal PdQpSpinDValue => this.dQpSpinD.Value;

        public ComboBox PQqSubbasin => this.QqSubbasin;

        public ComboBox PdQpSubbasin => this.dQpSubbasin;

        public ComboBox PQbSubbasin => this.QbSubbasin;

        public ComboBox PdQpStartMonth => this.dQpStartMonth;

        public ComboBox PQbStartMonth => this.QbStartMonth;

        //public Button PQqCalculate => this.QqCalculate;

        //public Button PdQpCalculate => this.dQpCalculate;

        //public Button PQbCalculate => this.QbCalculate;

        //public Button PQqSave => this.QqSave;

        //public Button PdQpSave => this.dQpSave;

        //public Button PQbSave => this.QbSave;

        //public CheckBox PQqAppend => this.QqAppend;
        public bool PQqAppendChecked => this.QqAppend.Checked;

        //public CheckBox PdQpAppend => this.dQpAppend;
        public bool PdQpAppendChecked => this.dQpAppend.Checked;

        //public CheckBox PQbAppend => this.QbAppend;
        public bool PQbAppendChecked => this.dQpAppend.Checked;

        //public DataGridView PQqResults => this.QqResults;
        public object PQqResultsDataSource {
            set { this.QqResults.DataSource = value; }
        }
        public void PQqResultsColumnsWidth(int col, int width) {
            this.QqResults.Columns[col].Width = width;
        }

        //public DataGridView PQbResults => this.QbResults;
        public object PQbResultsDataSource {
            set { this.QbResults.DataSource = value; }
        }
        public void PQbResultsColumnsWidth(int col, int width) {
            this.QbResults.Columns[col].Width = width;
        }

        //public Label PdQpResult => this.dQpResult;
        public string PdQpResultText {
            set { this.dQpResult.Text = value; }
        }

        //public Label PQbAnnualResult => this.QbAnnualResult;
        public string PQbAnnualResultText {
            set { this.QbAnnualResult.Text = value; }
        }

        //public RadioButton PdQpPercentile => this.dQpPercentile;
        public bool PdQpPercentileChecked => this.dQpPercentile.Checked;

        //public RadioButton PdQpMean => this.dQpMean;

        private void scenariosCombo_SelectionChangeCommitted(object sender, EventArgs e) {
            this._parent.setupDb();
        }

        private void outputCombo_SelectedChangeCommitted(object sender, EventArgs e) {
            this._parent.setVariables();
        }

        private void PsummaryCombo_SelectionChangeCommitted(object sender, EventArgs e) {
            this._parent.changeSummary();
        }

        private void addButton_Click(object sender, EventArgs e) {
            this._parent.addClick();
        }

        private void allButton_Click(object sender, EventArgs e) {
            this._parent.allClick();
        }

        private void delButton_Click(object sender, EventArgs e) {
            this._parent.delClick();
        }

        private void clearbutton_Click(object sender, EventArgs e) {
            this._parent.clearClick();
        }

        private void resultsFileButton_Click(object sender, EventArgs e) {
            this._parent.setResultsFile();
        }

        //private async void tabWidget_SelectedIndexChanged(object sender, EventArgs e) {
        //    await this._parent.modeChange();
        //}

        private async void saveButton_Click(object sender, EventArgs e) {
            await this._parent.makeResults();
        }

        private async void printButton_Click(object sender, EventArgs e) {
            await this._parent.printResults();
        }

        private void canvasAnimation_CheckedChanged(object sender, EventArgs e) {
            this._parent.changeAnimationMode();
        }

        private void printAnimation_CheckedChanged(object sender, EventArgs e) {
            this._parent.changeAnimationMode();
        }

        private async void animationVariableCombo_SelectionChangeCommitted(object sender, EventArgs e) {
            await this._parent.setupAnimateLayer();
        }

        private async void trackBar1_Scroll(object sender, EventArgs e) {
            await this._parent.changeAnimate();
        }

        private void trackBar1_MouseDown(object sender, MouseEventArgs e) {
            this._parent.pressSlider();
        }

        private void playCommand_Click(object sender, EventArgs e) {
            this._parent.doPlay();
        }

        private void pauseCommand_Click(object sender, EventArgs e) {
            this._parent.doPause();
        }

        private void rewindCommand_Click(object sender, EventArgs e) {
            this._parent.doRewind();
        }

        private void recordButton_Click(object sender, EventArgs e) {
            this._parent.record();
        }

        private void playButton_Click(object sender, EventArgs e) {
            this._parent.playRecording();
        }

        private void spinBox_ValueChanged(object sender, EventArgs e) {
            this._parent.changeSpeed((int)spinBox.Value);
        }

        private void timer1_Tick(object sender, EventArgs e) {
            this._parent.doStep();
        }

        private void variablePlot_SelectionChangeCommitted(object sender, EventArgs e) {
            this._parent.plotSetVar();
        }

        private void addPlot_Click(object sender, EventArgs e) {
            this._parent.doAddPlot();
        }

        private void deletePlot_Click(object sender, EventArgs e) {
            this._parent.doDelPlot();
        }

        private void copyPlot_Click(object sender, EventArgs e) {
            this._parent.doCopyPlot();
        }

        private void upPlot_Click(object sender, EventArgs e) {
            this._parent.doUpPlot();
        }

        private void downPlot_Click(object sender, EventArgs e) {
            this._parent.doDownPlot();
        }

        private void addObserved_Click(object sender, EventArgs e) {
            this._parent.addObservedPlot();
        }

        private void observedFileButton_Click(object sender, EventArgs e) {
            this._parent.setObservedFile(observedFileEdit.Text);
        }

        private void plotButton_Click(object sender, EventArgs e) {
            this._parent.writePlotData();
        }

        private async void closeButton_Click(object sender, EventArgs e) {
            await this._parent.doClose();
        }

        private void compareButton_Click(object sender, EventArgs e) {
            this._parent.startCompareScenarios();
        }

        private void VisualiseForm_KeyUp(object sender, KeyEventArgs e) {
            if (e.KeyValue == 39) { // right arrow
                this._parent.animateStepRight();
            } else if (e.KeyValue == 37) {  // left arrow
                this._parent.animateStepLeft();
            }
        }

        private async void slider_ValueChanged(object sender, EventArgs e) {
            await this._parent.changeAnimate();
        }

        private void unitPlot_SelectedIndexChanged(object sender, EventArgs e) {
            this._parent.plotSetUnit();        }

        private void unitEdit_TextChanged(object sender, EventArgs e) {
            this._parent.plotEditUnit();
        }

        private void QqCalculate_Click(object sender, EventArgs e) {
            this._parent.calculateQq();
        }

        private void QqSave_Click(object sender, EventArgs e) {
            this._parent.saveQq();
        }

        private void dQpCalculate_Click(object sender, EventArgs e) {
            this._parent.calculatedQp();
        }

        private void dQpSave_Click(object sender, EventArgs e) {
            this._parent.savedQp();
        }

        private void QbCalculate_Click(object sender, EventArgs e) {
            this._parent.calculateQb();
        }

        private void QbSave_Click(object sender, EventArgs e) {
            this._parent.saveQb();
        }

        private void tableWidget_CurrentCellChanged(object sender, EventArgs e) {
            this._parent.setVariablesForRow();
        }

        // code to get tooltips on outputCombo items to work
        // needs also:
        // DropDownStyle = DropDownList
        // DrawMode = OwnerDrawFixed
        // 4 event handlers below
        private void outputCombo_DrawItem(object sender, DrawItemEventArgs e) {
            if (e.Index < 0) { return; }
            string text = outputCombo.GetItemText(outputCombo.Items[e.Index]);
            string tip = "";
            if (!string.IsNullOrEmpty(text)) {
                tip = this._parent.getTableTip(text);
                e.DrawBackground();
            }
            using (SolidBrush br = new SolidBrush(e.ForeColor)) {
                e.Graphics.DrawString(text, e.Font, br, e.Bounds);
            }
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected && outputCombo.DroppedDown) {
                if (!string.IsNullOrEmpty(tip)) {
                    toolTip.Show(tip, outputCombo, e.Bounds.Right, e.Bounds.Bottom);
                } else {
                    toolTip.Hide(outputCombo);
                }
            }
            e.DrawFocusRectangle();
        }

        private void outputCombo_DropDownClosed(object sender, EventArgs e) {
            toolTip.Hide(outputCombo);
        }

        private void outputCombo_MouseLeave(object sender, EventArgs e) {
            toolTip.Hide(outputCombo);
        }

        private void outputCombo_MouseHover(object sender, EventArgs e) {
            if (!outputCombo.DroppedDown) {
                try {
                    string text = outputCombo.Text;
                    string tip = "";
                    if (!string.IsNullOrEmpty(text)) {
                        tip = this._parent.getTableTip(text);
                        if (!string.IsNullOrEmpty(tip)) {
                            toolTip.Show(tip, outputCombo, outputCombo.Location.X, outputCombo.Location.Y);
                        }
                    }
                } catch {
                    return;
                }
            }
        }

        // code to get tooltips on variablePlot items to work
        // needs also:
        // DropDownStyle = DropDownList
        // DrawMode = OwnerDrawFixed
        // 4 event handlers below
        private void variablePlot_DrawItem(object sender, DrawItemEventArgs e) {
            if (e.Index < 0) { return; }
            string text = variablePlot.GetItemText(variablePlot.Items[e.Index]);
            string tip = "";
            if (!string.IsNullOrEmpty(text)) {
                tip = this._parent.getVarTip(text);
                e.DrawBackground();
            }
            using (SolidBrush br = new SolidBrush(e.ForeColor)) {
                e.Graphics.DrawString(text, e.Font, br, e.Bounds);
            }
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected && variablePlot.DroppedDown) {
                if (!string.IsNullOrEmpty(tip)) {
                    toolTip.Show(tip, variablePlot, e.Bounds.Right, e.Bounds.Bottom);
                } else {
                    toolTip.Hide(variablePlot);
                }
            }
            e.DrawFocusRectangle();
        }

        private void variablePlot_DropDownClosed(object sender, EventArgs e) {
            toolTip.Hide(variablePlot);
        }

        private void variablePlot_MouseLeave(object sender, EventArgs e) {
            toolTip.Hide(variablePlot);
        }

        private void variablePlot_MouseHover(object sender, EventArgs e) {
            if (!variablePlot.DroppedDown) {
                try {
                    string text = variablePlot.Text;
                    string tip = "";
                    if (!string.IsNullOrEmpty(text)) {
                        tip = this._parent.getVarTip(text);
                        if (!string.IsNullOrEmpty(tip)) {
                            toolTip.Show(tip, variablePlot, variablePlot.Location.X, variablePlot.Location.Y);
                        }
                    } else {
                        toolTip.Show("Select the current plot's variable", animationVariableCombo, animationVariableCombo.Location.X, animationVariableCombo.Location.Y);
                    }
                } catch {
                    return;
                }
            }
        }

        private void animationVariableCombo_DrawItem(object sender, DrawItemEventArgs e) {
            if (e.Index < 0) { return; }
            string text = animationVariableCombo.GetItemText(animationVariableCombo.Items[e.Index]);
            string tip = "";
            if (!string.IsNullOrEmpty(text)) {
                tip = this._parent.getVarTip(text);
                e.DrawBackground();
            }
            using (SolidBrush br = new SolidBrush(e.ForeColor)) {
                e.Graphics.DrawString(text, e.Font, br, e.Bounds);
            }
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected && animationVariableCombo.DroppedDown) {
                if (!string.IsNullOrEmpty(tip)) {
                    toolTip.Show(tip, animationVariableCombo, e.Bounds.Right, e.Bounds.Bottom);
                } else {
                    toolTip.Hide(animationVariableCombo);
                }
            }
            e.DrawFocusRectangle();
        }

        private void animationVariableCombo_DropDownClosed(object sender, EventArgs e) {
            toolTip.Hide(animationVariableCombo);
        }

        private void animationVariableCombo_MouseLeave(object sender, EventArgs e) {
            toolTip.Hide(animationVariableCombo);
        }

        private void animationVariableCombo_MouseHover(object sender, EventArgs e) {
                    if (!animationVariableCombo.DroppedDown) {
                        try {
                            string text = animationVariableCombo.Text;
                            string tip = "";
                            if (!string.IsNullOrEmpty(text)) {
                                tip = this._parent.getVarTip(text);
                                if (!string.IsNullOrEmpty(tip)) {
                                    toolTip.Show(tip, animationVariableCombo, animationVariableCombo.Location.X, animationVariableCombo.Location.Y);
                                }
                            } else {
                                toolTip.Show("Select a variable for animation", animationVariableCombo, animationVariableCombo.Location.X, animationVariableCombo.Location.Y);
                            }
                        } catch {
                            return;
                        }
                    }
                }

        private void variableCombo_DrawItem(object sender, DrawItemEventArgs e) {
            if (e.Index < 0) { return; }
            string text = variableCombo.GetItemText(variableCombo.Items[e.Index]);
            string tip = "";
            if (!string.IsNullOrEmpty(text)) {
                tip = this._parent.getVarTip(text);
                e.DrawBackground();
            }
            using (SolidBrush br = new SolidBrush(e.ForeColor)) {
                e.Graphics.DrawString(text, e.Font, br, e.Bounds);
            }
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected && variableCombo.DroppedDown) {
                if (!string.IsNullOrEmpty(tip)) {
                    toolTip.Show(tip, variableCombo, e.Bounds.Right, e.Bounds.Bottom);
                } else {
                    toolTip.Hide(variableCombo);
                }
            }
            e.DrawFocusRectangle();
        }

        private void variableCombo_DropDownClosed(object sender, EventArgs e) {
            toolTip.Hide(variableCombo);
        }

        private void variableCombo_MouseLeave(object sender, EventArgs e) {
            toolTip.Hide(variableCombo);
        }

        private void variableCombo_MouseHover(object sender, EventArgs e) {
            if (!variableCombo.DroppedDown) {
                try { 
                    string text = variableCombo.Text;
                    string tip = "";
                    if (!string.IsNullOrEmpty(text)) {
                        tip = this._parent.getVarTip(text);
                        if (!string.IsNullOrEmpty(tip)) {
                            toolTip.Show(tip, variableCombo, variableCombo.Location.X, variableCombo.Location.Y);
                        }
                    } else {
                        toolTip.Show("Select variable to be added to list", variableCombo, variableCombo.Location.X, variableCombo.Location.Y);
                    }
                } catch {
                    return;
                }
            }
        }

        // failed attempt to get tool tips for outputCombo items
        //private void outputCombo_MouseHover(object sender, EventArgs e) {
        //    this._parent.outputComboToolTip();
        //}

        //private void outputCombo_DropDown(object sender, EventArgs e) {
        //    this.outputCombo.MouseHover += new System.EventHandler(this.outputCombo_MouseHover);
        //}

        //private void outputCombo_DropDownClosed(object sender, EventArgs e) {
        //    this.outputCombo.MouseHover -= this.outputCombo_MouseHover;
        //}

        //private void outputCombo_MouseEnter(object sender, EventArgs e) {
        //    if (this.outputCombo.DroppedDown) {
        //        this._parent.outputComboToolTip();
        //    }
        //}

        //private void outputCombo_MouseLeave(object sender, EventArgs e) {
        //    this.toolTip.SetToolTip(this.outputCombo, null);
        //}
    }
}
