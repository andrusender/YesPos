using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using WebKit;
using CefSharp;
using CefSharp.WinForms;
namespace YesPos
{
    class _CefBrowser
    {
        private WebView webview;
        public WebView getWebView()
        {
            return webview;
        }
        public _CefBrowser(string start_url)
        {
            var cefSettings = new Settings();
                cefSettings.CachePath = (Global.AppDir + @"\cache");
            CEF.Initialize(cefSettings);
            CEF.SetCookiePath(Global.AppDir+@"\cookies");            
            var bsettings = new BrowserSettings();
            bool debug = false;
            bool.TryParse(Config.get("system", "debug"), out debug);
            bsettings.DeveloperToolsDisabled = !debug;
            webview = new WebView(start_url, bsettings);
            
            webview.PropertyChanged += (sender, e) => {
                if (e.PropertyName == "Title")
                {
                    if(DocumentTitleChanged!=null)
                        DocumentTitleChanged(this, new EventArgs());
                    webview.ExecuteScript("document.addEventListener('DOMContentLoaded', function(){window.external.dom_ready()}, false); ");
                }
            };
        }
        public void StringByEvaluatingJavaScriptFromString(string js)
        {
             webview.ExecuteScript(js);
        }
        public void Navigate(string url)
        {
            webview.Load(url);
        }

        public void call_dom_ready_event()
        {
            if (DocumentCompleted != null)
            {
                DocumentCompleted(this, new WebBrowserDocumentCompletedEventArgs(new Uri(webview.Address)));
            }
        }

        public event System.Windows.Forms.WebBrowserDocumentCompletedEventHandler DocumentCompleted;

        public event EventHandler DocumentTitleChanged;

        public event WebKitBrowserErrorEventHandler Error;

        public event System.Windows.Forms.WebBrowserNavigatedEventHandler Navigated;

        public event System.Windows.Forms.WebBrowserNavigatingEventHandler Navigating;        
    }
}
