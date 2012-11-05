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
        
        public static string getSystemUrl(string htmlFile)
        {
            var path = Global.HtmlPath + htmlFile+".html";
            string url = new Uri(path, UriKind.Absolute).AbsoluteUri;
            return url;
        }
    }
}
