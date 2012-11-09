using System;
using System.Collections.Generic;
using System.Text;

namespace YesPos
{
    public class Global
    {        
        public static string AppDir = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
        public static string IniPath = Global.AppDir + @"\System\Configuration.ini";
        public static string HtmlPath = Global.AppDir + @"\System\Html\";
        public static string ImagePath = Global.AppDir + @"\System\Html\images\";
        public static string JsPath = Global.AppDir + @"\System\Html\js\";
        public static string PluginsPath = Global.AppDir + @"\System\Plugins\";
        public static string CookiesFile = Global.AppDir + @"\System\cookies.jar";
        
        public static string getSystemUrl(string path)
        {
            //var path = Global.HtmlPath + htmlFile+".html";
            string url = new Uri(path.Replace(@"\","/"), UriKind.Absolute).AbsoluteUri;
            return url;
        }
    }
}
