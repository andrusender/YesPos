using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using WebKit;
using System.Drawing;
namespace YesPos
{
    class WebViewForm:Form
    {
        private System.ComponentModel.IContainer components;
        private ContextMenuStrip contextMenuStrip;
        private ToolStripMenuItem copyToolStripMenuItem;
        private WebKitBrowser Web;
        public WebViewForm()
            : base()
        {
            InitializeComponent();

            //Initialization
            Web = new WebKitBrowser();            
            Web.AllowDownloads = false;
            Web.AllowNewWindows = true;
            Web.Dock = DockStyle.Fill;
            Web.ObjectForScripting = new JavaScriptBridge(this);
            Web.ContextMenuStrip = contextMenuStrip;
            Web.Click += (sender, e) => { this.Close(); };
            Controls.Add(Web);

            //Events 
            Web.Error += (sender, e) => { Web.Navigate(Global.getSystemUrl(Global.HtmlPath+"NetworkError.html")); };            
            Web.DocumentTitleChanged += (sender, e) => { Text = Web.DocumentTitle; };
            Web.DocumentCompleted += (sender, e) => { 
                Text = Web.DocumentTitle;
                if (bool.Parse(Ini.get("firebug", "enabled")))
                {
                    string firebug_params = String.Format("overrideConsole: {0}, startInNewWindow: {1},startOpened: {2},enableTrace: {3}", Ini.get("firebug", "overrideConsole"), Ini.get("firebug", "startInNewWindow"), Ini.get("firebug", "startOpened"), Ini.get("firebug", "enableTrace"));
                    Web.StringByEvaluatingJavaScriptFromString(@"var script = document.createElement('script');script.src = '" + Global.getSystemUrl(Global.JsPath + "firebug-lite.js").Replace("file:///", "file://") + "';script.innerHTML = '{"+firebug_params+"}';document.getElementsByTagName('head')[0].appendChild(script);");
                }
            };
            Web.Navigated += (sender, e) => { 
                Web.StringByEvaluatingJavaScriptFromString("window.system = window.external");
                Web.StringByEvaluatingJavaScriptFromString(@"document.addEventListener('contextmenu', function(event) { event.preventDefault();}, false);");
                Web.Show();
            };
            Web.ShowJavaScriptAlertPanel += (sender, e) => { MessageBox.Show(e.Message, Web.DocumentTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation); };
            Web.ShowJavaScriptConfirmPanel += (sender, e) => { e.ReturnValue = (DialogResult.Yes == MessageBox.Show(e.Message, Web.DocumentTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question));  };
            Web.ShowJavaScriptPromptPanel += (sender, e) => { e.ReturnValue = Microsoft.VisualBasic.Interaction.InputBox(e.Message, Web.DocumentTitle, e.DefaultValue); };
            
            //Navigation            
            Web.Navigate(Global.getSystemUrl(Global.HtmlPath+"test.html"));                        
        }

        public WebKitBrowser getWebBrowser()
        {
            return Web;
        }

        public ContextMenuStrip getContextMenuStrip()
        {
            return contextMenuStrip;
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(103, 26);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(102, 22);
            this.copyToolStripMenuItem.Text = "Copy";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
            // 
            // WebViewForm
            // 
            this.ClientSize = new System.Drawing.Size(292, 270);
            this.Name = "WebViewForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.contextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetText(Web.SelectedText);
            }
            catch { }
        }
    }
}
