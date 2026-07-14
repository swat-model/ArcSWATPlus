

using ArcSWATPlus;

namespace ArcSWAT3 {
    
 // provide basic information about QSWAT, including version, and link to SWAT website.
    public class AboutArcSWAT {
        
        public AboutForm _dlg;
        
        public object _gv;
        
        public AboutArcSWAT(object gv) {
            this._gv = gv;
            this._dlg = aboutDialog();
            if (this._gv) {
                this._dlg.move(this._gv.aboutPos);
            }
        }
        
        // Run the form.
        public virtual void run(string version) {
            this._dlg.SWATHomeButton.clicked.connect(this.openSWATUrl);
            this._dlg.closeButton.clicked.connect(this._dlg.Close);
            var text = @$"
ArcSWAT3 version: {version}

Python version: 3.9

Current restrictions:
- runs only in Windows
        ";
            this._dlg.textBrowser.setText(text);
            this._dlg.exec_();
            if (this._gv is not null) {
                this._gv.aboutPos = this._dlg.pos();
            }
        }
        
        // Open SWAT website.
        public virtual void openSWATUrl() {
            webbrowser.open("http://swat.tamu.edu/");
        }
    }
}
