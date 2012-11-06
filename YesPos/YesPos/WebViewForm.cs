using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using WebKit;
using System.Drawing;
using System.IO;
namespace YesPos
{
    class WebViewForm:Form
    {
        private System.ComponentModel.IContainer components;
        private ContextMenuStrip contextMenuStrip;
        private ToolStripMenuItem copyToolStripMenuItem;
        private MyWebKitBrowser Web;
        private bool firstRun = true;
        private Panel splashPanel;
        public WebViewForm(string start_url)
            : base()
        {
            InitializeComponent();
            splashPanel= new Panel();
            splashPanel.BackColor = Color.White;
            splashPanel.BackgroundImage = Bitmap.FromFile(Global.ImagePath+"yespos_logo.png");
            splashPanel.Dock = DockStyle.Fill;
            splashPanel.BackgroundImageLayout = ImageLayout.Center;
            Controls.Add(splashPanel);            
            Timer t = new Timer();            
            t.Interval = 1;
            t.Start();
            t.Tick += (sender,e) => {
                InitializeWebBrowser(start_url);
                t.Stop();
            };            
        }
        
        private void InitializeWebBrowser(string start_url)
        {            
            //Initialization            
            Web = new MyWebKitBrowser();
            Web.AllowDownloads = false;
            Web.AllowNewWindows = true;
            Web.Dock = DockStyle.Fill;
            Web.ObjectForScripting = new JavaScriptBridge(this);
            Web.AllowFireBug = bool.Parse(Config.get("firebug", "enabled"));
            Web.FireBugOptions = new MyWebKitBrowser.FireBugOptionsStruct() {
                overrideConsole = bool.Parse(Config.get("firebug", "overrideConsole")), 
                startInNewWindow = bool.Parse(Config.get("firebug", "startInNewWindow")), 
                enableTrace = bool.Parse(Config.get("firebug", "enableTrace")), 
                startOpened = bool.Parse(Config.get("firebug", "startOpened")) 
            };
            Web.AllowPlugins = bool.Parse(Config.get("system","plugins"));
            Web.ContextMenuStrip = contextMenuStrip;
            
            //Events 
            Web.Error += (sender, e) => { Web.Navigate(Global.getSystemUrl(Global.HtmlPath + "NetworkError.html")); };
            Web.DocumentTitleChanged += (sender, e) => { Text = Web.DocumentTitle; };
            Web.DocumentCompleted += (sender, e) =>
            {
                Web.Focus();
                if (firstRun)
                {
                    firstRun = false;
                    splashPanel.Hide();
                    splashPanel.Dispose();
                }
                Text = Web.DocumentTitle;
            };            
            //Add To Controls
            Controls.Add(Web);            
            //Navigation            
            Web.Navigate(start_url);            
        }



        public MyWebKitBrowser getWebBrowser()
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WebViewForm));
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
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
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
            finally { }
        }
    }
}
