using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Net;
using System.Drawing.Printing;
using Newtonsoft.Json;
namespace YesPos
{
    class JavaScriptBridge
    {
        private WebViewForm F;
        public JavaScriptBridge(WebViewForm f)
        {
            F = f;
        }

        #region Form
            //TODO MINIMUM FORM SIZE FROM JS
        public void set_window_minimum_size(string width, string height)
        {
            F.MinimumSize = new Size(int.Parse(width), int.Parse(height));
        }
        public void change_icon(string icon, string text,string text_size, string color)
        {
            DynamicIcon di = new DynamicIcon(F);
            di.Refresh(icon, text, int.Parse(text_size), System.Drawing.ColorTranslator.FromHtml(color));
        }
        #endregion;

        #region Printing
        public string[] get_printers()
        {
            var ls = System.Drawing.Printing.PrinterSettings.InstalledPrinters;
            var result = new string[ls.Count];
            var i = 0;
            foreach (string p in ls)
            {
                result[i] = p;
                i++;
            }
            return result;
        }

        public string get_paper_sizes()
        {
            PrinterSettings ps = new PrinterSettings();
            List<object> yps = new List<object>();
            foreach (PaperSize p in ps.PaperSizes)
            {
                yps.Add(new { PaperName = p.PaperName, Width = inch2mm(p.Width), Height = inch2mm(p.Height) });
            }
            return JsonConvert.SerializeObject(yps);
        }     

        private PrintOptions applyPrintOptionsFromString(string options,ref MyWebKitBrowser web)
        {
            PrintOptions po;
            //Get User Params
            try
            {
                po = Newtonsoft.Json.JsonConvert.DeserializeObject<PrintOptions>(options);
            }
            finally
            {
                po = new PrintOptions();
            }
            //Converting
            po.marginLeft = mm2inch(po.marginLeft);
            po.marginTop = mm2inch(po.marginTop);
            po.marginRight = mm2inch(po.marginRight);
            po.marginBottom = mm2inch(po.marginBottom);
            //Apply Options
            web.PageSettings.PaperSize = new System.Drawing.Printing.PaperSize("YesPos", mm2inch(po.paperWidth), mm2inch(po.paperHeight));
            web.PageSettings.Margins = new System.Drawing.Printing.Margins(po.marginLeft, po.marginRight, po.marginTop, po.marginBottom);
            web.PageSettings.PrinterSettings.PrinterName = po.printer;
            return po;
        }

        public void set_default_print_options(string options)
        {
            try
            {
                PrintOptions po = Newtonsoft.Json.JsonConvert.DeserializeObject<PrintOptions>(options);
                Properties.Settings.Default.printerOptions = Newtonsoft.Json.JsonConvert.SerializeObject(options);
                Properties.Settings.Default.Save();
            }
            finally
            {
                F.getWebBrowser().StringByEvaluatingJavaScriptFromString("alert('Incorrect PrintOptions!')");
            }
            
            return;
        }

        public string get_default_print_options()
        {
            if (Properties.Settings.Default.printerOptions != "")
            {
                return Properties.Settings.Default.printerOptions;
            }
            else
            {
                PrintOptions po = new PrintOptions();
                Properties.Settings.Default.printerOptions = Newtonsoft.Json.JsonConvert.SerializeObject(po);
                Properties.Settings.Default.Save();
                return Properties.Settings.Default.printerOptions;
            }
        }

        public void print(string options)
        {
            if (String.IsNullOrEmpty(options)) options = get_default_print_options();            
            var web = F.getWebBrowser();            
            applyPrintOptionsFromString(options, ref web);
            web.Print();
        }

        public void print_url(string url,string options)
        {
            if (String.IsNullOrEmpty(options)) options = get_default_print_options();
            var webTemp = new MyWebKitBrowser();            
            F.Controls.Add(webTemp);
            webTemp.Navigate(url);
            webTemp.DocumentCompleted += (sender, e) => {
                applyPrintOptionsFromString(options, ref webTemp);
                webTemp.Print();
                F.Controls.Remove(webTemp);
            };
            webTemp.Error += (sender, e) => { F.Controls.Remove(webTemp); };
        }    
            
        public void print_html(string html, string options)
        {
            if (String.IsNullOrEmpty(options)) options = get_default_print_options();
            var webTemp = new MyWebKitBrowser();
            F.Controls.Add(webTemp);
            webTemp.DocumentText = html;
            webTemp.DocumentCompleted += (sender, e) =>
            {
                applyPrintOptionsFromString(options, ref webTemp);
                webTemp.Print();
                F.Controls.Remove(webTemp);
            };
        }
        #endregion;

        #region Clipboard
        public void set_clipboard_text(string text)
        {
            Clipboard.SetText(text);
        }
        public string get_clipboard_text()
        {
            return Clipboard.GetText();
        }
        #endregion

        private Image getImageByAddress(string addr)
        {
            Image result = null;
            if (addr.StartsWith("http://"))
            {
                var request = WebRequest.Create(addr);
                using (var response = request.GetResponse())
                using (var stream = response.GetResponseStream())
                {
                    result = Bitmap.FromStream(stream);
                }
            }
            else
            {   //LocalFolder
                if (System.IO.File.Exists(Global.ImagePath + addr))
                {
                    result = new Bitmap(Global.ImagePath + addr);
                }
                else if (System.IO.File.Exists(addr))
                {//Global Computer 
                    result = new Bitmap(addr);
                }
                else
                {
                    F.getWebBrowser().StringByEvaluatingJavaScriptFromString("alert('Error!\\nImage not Found!\\n" + (Global.ImagePath + addr).Replace("\\", "/") + "');");
                }
            }
            return result;
        }
       
        private string escapeJs(string js)
        {
            return "(function(window){" + js + "})(window);";
        }
       
        #region Notification
        public void notification(string options_json_string)
        {
            var options = Newtonsoft.Json.JsonConvert.DeserializeObject<JavaScript.NotificationOptions>(options_json_string);
            
            NotificationWindow.PopupNotifier p = new NotificationWindow.PopupNotifier();
            
            p.ShowOptionsButton = false;
            p.BodyColor = System.Drawing.ColorTranslator.FromHtml(options.BackgroundColor);            
            p.ShowGrip = false;
            p.BorderColor = System.Drawing.ColorTranslator.FromHtml(options.BorderColor);
            
            p.HeaderHeight = options.HeaderHeight;
            p.HeaderColor = System.Drawing.ColorTranslator.FromHtml(options.HeaderColor);
            
            if (!String.IsNullOrEmpty(options.Image))
            {
                p.Image = getImageByAddress(options.Image);
            }
            p.ImagePadding = new Padding(options.ImagePaddingsLeft, options.ImagePaddingsTop, options.ImagePaddingsRight, options.ImagePaddingsBottom);
            
            p.TitleText = options.Title;
            p.TitleFont = new Font("Arial", 12, FontStyle.Bold);
            p.TitleColor = System.Drawing.ColorTranslator.FromHtml(options.TitleColor);
            p.TitlePadding = new Padding(options.TitlePaddingsLeft, options.TitlePaddingsTop, options.TitlePaddingsRight, options.TitlePaddingsBottom);

            p.ContentFont = new Font("Arial", 10, FontStyle.Regular);
            p.ContentPadding = new Padding(options.ContentPaddingsLeft, options.ContentPaddingsTop, options.ContentPaddingsRight, options.ContentPaddingsBottom);
            p.ContentText = options.Content;
            p.ContentHoverColor = System.Drawing.ColorTranslator.FromHtml(options.ContentHoverColor);
            p.CloseButtonColor = System.Drawing.ColorTranslator.FromHtml(options.CloseButtonColor);
            p.ButtonHoverColor = System.Drawing.ColorTranslator.FromHtml(options.ButtonHoverColor);
            p.ButtonBorderColor = System.Drawing.ColorTranslator.FromHtml(options.CloseButtonBorderColor);
            p.AnimationDuration = options.AnimationDuration;
            p.Delay = options.Delay;
            if (!String.IsNullOrEmpty(options.OnClick))
            {
                p.Click += (sender, e) => { F.getWebBrowser().StringByEvaluatingJavaScriptFromString(escapeJs(options.OnClick)); p.Hide(); };
            }
            p.Size = new Size(options.Width, options.Height);
            p.Popup();
        }
        #endregion

        #region Converting mm<=>inch
        public int mm2inch(double mm)
        {
            return Convert.ToInt16(Math.Round(System.Drawing.Printing.PrinterUnitConvert.Convert(mm, System.Drawing.Printing.PrinterUnit.TenthsOfAMillimeter, System.Drawing.Printing.PrinterUnit.ThousandthsOfAnInch)));
        }
        public int inch2mm(double inch)
        {
            return Convert.ToInt16(Math.Round(System.Drawing.Printing.PrinterUnitConvert.Convert(inch, System.Drawing.Printing.PrinterUnit.ThousandthsOfAnInch, System.Drawing.Printing.PrinterUnit.TenthsOfAMillimeter)));
        }
        #endregion;

        #region ContextMenu
        public string context_menu_add_separator(string ids_path_string)
        {
            return context_menu_add_item(ids_path_string, "", "", "");
        }
        public string context_menu_add_item(string ids_path_string, string text, string image, string OnClick)
        {
            string result="";
            var ids_path = String.IsNullOrEmpty(ids_path_string)?null:ids_path_string.Split(',');
            var menu = F.getContextMenuStrip();
            ToolStripItem new_item = null;
            if (String.IsNullOrEmpty(text) && String.IsNullOrEmpty(image) && String.IsNullOrEmpty(OnClick))
            {
                new_item = new ToolStripSeparator();
            }
            else
            {
                new_item = String.IsNullOrEmpty(image) ? new ToolStripMenuItem(text) : new ToolStripMenuItem(text, getImageByAddress(image));
            }
            
            if (!String.IsNullOrEmpty(OnClick))
            {
                new_item.Click += (sender, e) => { F.getWebBrowser().StringByEvaluatingJavaScriptFromString(escapeJs(OnClick)); };
            }
            if (ids_path==null)
            {
                return menu.Items.Add(new_item).ToString();
            }
            else
            {
                ToolStripMenuItem item = null;                
                for (int i = 0; i < ids_path.Length; i++)
                {
                    result += ids_path[i] + ",";
                    item = (item == null) ? (ToolStripMenuItem)menu.Items[int.Parse(ids_path[i])] : (ToolStripMenuItem)item.DropDownItems[int.Parse(ids_path[i])];                    
                }
                result += item.DropDownItems.Add(new_item);                
            }            
            return result;
        }
        public void context_menu_remove_item(string ids_path_string)
        {
            var ids_path = String.IsNullOrEmpty(ids_path_string) ? null : ids_path_string.Split(',');
            var menu = F.getContextMenuStrip();
            if (ids_path.Length == 1)
            {
                menu.Items.Remove(menu.Items[int.Parse(ids_path[0])]);
            }
            else
            {
                ToolStripMenuItem parent_item = null;
                ToolStripMenuItem remove_item = null;
                for (int i = 0; i < ids_path.Length; i++)
                {
                    if (i < ids_path.Length - 1)
                    {
                        parent_item = (parent_item == null) ? (ToolStripMenuItem)menu.Items[int.Parse(ids_path[i])] : (ToolStripMenuItem)parent_item.DropDownItems[int.Parse(ids_path[i])];
                    }
                    else
                    {
                        remove_item = (ToolStripMenuItem) parent_item.DropDownItems[int.Parse(ids_path[i])];
                    }
                }               
                parent_item.DropDownItems.Remove(remove_item);
            }
        }
        #endregion
    }
}
