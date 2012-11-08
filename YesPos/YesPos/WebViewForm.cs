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
        private _WebKitBrowser Web;
        private bool firstRun = true;
        private NotifyIcon mynotifyicon;
        private ContextMenuStrip trayContextMenuStrip;
        private ToolStripMenuItem exitToolStripMenuItem;
        private Panel splashPanel;
        public WebViewForm(string start_url)
            : base()
        {            
            InitializeComponent();
            this.MinimumSize = new Size(int.Parse(Config.get("system", "minWidth")), int.Parse(Config.get("system", "minHeight")));
            
            splashPanel= new Panel();
            splashPanel.BackColor = Color.White;
            splashPanel.BackgroundImage = Bitmap.FromFile(Config.get("system","splashImage"));
            splashPanel.Dock = DockStyle.Fill;
            splashPanel.BackgroundImageLayout = ImageLayout.Center;
            Controls.Add(splashPanel);            
            Timer t = new Timer();            
            t.Interval = 1;
            t.Start();
            t.Tick += (sender,e) => {
                Dispatcher.Invoke(this, () => { 
                    InitializeWebBrowser(start_url);
                });
                t.Stop();
            };
            this.FormClosed += (sender, e) => { mynotifyicon.Visible = false; };
        }
        
        private void InitializeWebBrowser(string start_url)
        {            
            //Initialization            
            Web = new _WebKitBrowser();
            Web.AllowDownloads = false;
            Web.AllowNewWindows = true;
            Web.Dock = DockStyle.Fill;
            Web.RegisterJsObject("system",new JavaScriptHandler(this));
            Web.AllowFireBug = bool.Parse(Config.get("firebug", "enabled"));
            Web.FireBugOptions = new _WebKitBrowser.FireBugOptionsStruct() {
                overrideConsole = bool.Parse(Config.get("firebug", "overrideConsole")), 
                startInNewWindow = bool.Parse(Config.get("firebug", "startInNewWindow")), 
                enableTrace = bool.Parse(Config.get("firebug", "enableTrace")), 
                startOpened = bool.Parse(Config.get("firebug", "startOpened")) 
            };
            Web.AllowPlugins = bool.Parse(Config.get("system","plugins"));
            Web.ContextMenuStrip = contextMenuStrip;
            
            //Events 
            Web.Error += (sender, e) => { 
                Web.Navigate(Global.getSystemUrl(Global.HtmlPath + "NetworkError.html")); 
            };
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



        public _WebKitBrowser getWebBrowser()
        {
            return Web;
        }

        public ContextMenuStrip getContextMenuStrip()
        {
            return contextMenuStrip;
        }

        public ContextMenuStrip getTrayContextMenuStrip()
        {
            return trayContextMenuStrip;
        }

        public NotifyIcon getNotifyIcon()
        {
            return mynotifyicon;
        }
        
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WebViewForm));
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mynotifyicon = new System.Windows.Forms.NotifyIcon(this.components);
            this.trayContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip.SuspendLayout();
            this.trayContextMenuStrip.SuspendLayout();
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
            // mynotifyicon
            // 
            this.mynotifyicon.BalloonTipText = "We are Still Working";
            this.mynotifyicon.Icon = ((System.Drawing.Icon)(resources.GetObject("mynotifyicon.Icon")));
            this.mynotifyicon.Visible = true;
            this.mynotifyicon.MouseClick += new System.Windows.Forms.MouseEventHandler(this.mynotifyicon_MouseClick);
            // 
            // trayContextMenuStrip
            // 
            this.trayContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.trayContextMenuStrip.Name = "trayContextMenuStrip";
            this.trayContextMenuStrip.Size = new System.Drawing.Size(93, 26);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(92, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // WebViewForm
            // 
            this.ClientSize = new System.Drawing.Size(292, 270);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "WebViewForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Resize += new System.EventHandler(this.WebViewForm_Resize);
            this.contextMenuStrip.ResumeLayout(false);
            this.trayContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Dispatcher.Invoke(this, () => {
                    Clipboard.SetText(Web.SelectedText);
                });
            }
            finally { }
        }

        private void WebViewForm_Resize(object sender, EventArgs e)
        {
            /*
            if (FormWindowState.Minimized == this.WindowState)
            {
                toTray();
            }
            else
            {
                mynotifyicon.Visible = false;
            }*/
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mynotifyicon.Visible = false;
            this.Close();
        }

        private void mynotifyicon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                fromTray();
            }
        }

        public void fromTray()
        {
            mynotifyicon.Visible = false;
            this.WindowState = FormWindowState.Maximized;
            this.Show();
            this.TopMost = true;
            this.TopMost = false;
        }

        public void toTray()
        {
            mynotifyicon.Visible = true;
            /*mynotifyicon.ContextMenuStrip = trayContextMenuStrip;
            mynotifyicon.BalloonTipTitle = "YesPos still working";
            mynotifyicon.BalloonTipText = "Some text going here....";
            mynotifyicon.BalloonTipIcon = ToolTipIcon.Info;
            mynotifyicon.ShowBalloonTip(1000);*/
            this.Hide();
        }        
    }
}
