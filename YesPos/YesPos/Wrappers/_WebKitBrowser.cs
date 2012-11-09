using System;
using System.Collections.Generic;
using System.Text;
using WebKit;
using System.IO;
using System.Windows.Forms;
using System.Net;
using Newtonsoft.Json;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO.Compression;
using WebKit.Interop;
using System.Diagnostics;
namespace YesPos
{
    class _WebKitBrowser:WebKitBrowser
    {
        private bool allowFireBug = true;
        private bool allowPlugins = true;
        public bool AllowFireBug
        {
            get { return allowFireBug; }
            set { allowFireBug = value; }
        }
        private string jsObjectName;
        public void RegisterJsObject(string name, object obj)
        {
            jsObjectName = name;
            ObjectForScripting = obj;
        }
        private bool allowCookies = false;
        public bool AllowCookies { 
            get { return allowCookies; } 
            set{ allowCookies = value; } 
        }
        public bool AllowPlugins { 
            get { return allowPlugins; }
            set { allowPlugins = value; }
        }

        public FireBugOptionsStruct FireBugOptions = new FireBugOptionsStruct() { overrideConsole = true, startInNewWindow = false, enableTrace = true, startOpened = false};
        public _WebKitBrowser() : base() 
        {
            this.Navigated += (sender, e) => {
                this.StringByEvaluatingJavaScriptFromString("window."+jsObjectName+" = window.external");
                this.StringByEvaluatingJavaScriptFromString(@"document.addEventListener('contextmenu', function(event) { event.preventDefault();}, false);");
            };            
            this.ShowJavaScriptAlertPanel += (sender, e) => { MessageBox.Show(this.ParentForm,e.Message, this.DocumentTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation); };
            this.ShowJavaScriptConfirmPanel += (sender, e) => { e.ReturnValue = (DialogResult.Yes == MessageBox.Show(this.ParentForm,e.Message, this.DocumentTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question)); };
            this.ShowJavaScriptPromptPanel += (sender, e) => { e.ReturnValue = Microsoft.VisualBasic.Interaction.InputBox(e.Message, this.DocumentTitle, e.DefaultValue,0,0); };
            this.DocumentCompleted += (sender, e) => {
                if (allowFireBug)
                {
                    string firebug_params = String.Format("overrideConsole: {0}, startInNewWindow: {1},startOpened: {2},enableTrace: {3}", 
                        FireBugOptions.overrideConsole.ToString().ToLower(),
                        FireBugOptions.startInNewWindow.ToString().ToLower(),
                        FireBugOptions.startOpened.ToString().ToLower(),
                        FireBugOptions.enableTrace.ToString().ToLower());
                    this.StringByEvaluatingJavaScriptFromString(@"var script = document.createElement('script');script.src = '" + Global.getSystemUrl(Global.JsPath + "firebug-lite.js").Replace("file:///", "file://") + "';script.innerHTML = '{" + firebug_params + "}';document.getElementsByTagName('head')[0].appendChild(script);");
                }
                if (this.allowPlugins)
                {
                    var plugins = getPlugins();
                    foreach (KeyValuePair<string, string> plugin in plugins)
                    {
                        var script = this.Document.CreateElement("script");
                        script.TextContent = "//"+plugin.Key+"\n"+plugin.Value;
                        this.Document.GetElementsByTagName("head")[0].AppendChild(script);
                    }
                }
            };
        }

        private Dictionary<string, string> getPlugins()
        {
            var result = new Dictionary<string, string>();
            var content = "";
            foreach (string plugin_file in Directory.GetFiles(Global.PluginsPath, "*.js"))
            {
                content = File.ReadAllText(plugin_file);
                result.Add(plugin_file, content);                
            }
            return result;
        }

        public static void WriteCookiesToDisk(string file, CookieContainer cookieJar)
        {
            using (Stream stream = File.Create(file))
            {
                try
                {
                    Console.Out.Write("Writing cookies to disk... ");
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, cookieJar);
                    Console.Out.WriteLine("Done.");
                }
                catch (Exception e)
                {
                    Console.Out.WriteLine("Problem writing cookies to disk: " + e.GetType());
                }
            }
        }

        public static CookieContainer ReadCookiesFromDisk(string file)
        {
            try
            {
                using (Stream stream = File.Open(file, FileMode.Open))
                {
                    Console.Out.Write("Reading cookies from disk... ");
                    BinaryFormatter formatter = new BinaryFormatter();
                    Console.Out.WriteLine("Done.");
                    return (CookieContainer)formatter.Deserialize(stream);
                }
            }
            catch (Exception e)
            {
                Console.Out.WriteLine("Problem reading cookies from disk: " + e.GetType());
                return new CookieContainer();
            }
        }

        /*
        public void Navigate(string url)
        {
            //base.Navigate(url);
            //return;
            allowCookies = true;

            //var c = new WebCacheModel();
            //p.ca
            base.Navigate(url);
        }*/

        public struct FireBugOptionsStruct{
            public bool overrideConsole;
            public bool startInNewWindow;
            public bool startOpened;
            public bool enableTrace;
        }
    }
}
