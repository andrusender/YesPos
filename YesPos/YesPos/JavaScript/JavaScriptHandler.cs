using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Net;
using System.Drawing.Printing;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.ComponentModel;
namespace YesPos
{
    class JavaScriptHandler
    {
        private WebViewForm F;
        private Icon tempFicon;

        public JavaScriptHandler(WebViewForm f)
        {
            F = f;
        }

        #region Private Helpers
        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetDefaultPrinter(string Name);
        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GetDefaultPrinter(StringBuilder pszBuffer, ref int pcchBuffer);
        private const int ERROR_FILE_NOT_FOUND = 2;
        private const int ERROR_INSUFFICIENT_BUFFER = 122;
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
        #endregion

        #region System
        public void refresh()
        {
            var b = F.getWebBrowser();
            b.Navigate(b.Url.OriginalString);
        }
        public void error(string title, string text)
        {
            MessageBox.Show((Form)F, text, "System Error [" + title + "]", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        public bool set_system_default_printer(string printer_name)
        {
            return SetDefaultPrinter(printer_name);
        }
        public string get_system_default_printer()
        {

            int pcchBuffer = 0;
            if (GetDefaultPrinter(null, ref pcchBuffer))
            {
                return null;
            }
            int lastWin32Error = Marshal.GetLastWin32Error();
            if (lastWin32Error == ERROR_INSUFFICIENT_BUFFER)
            {
                StringBuilder pszBuffer = new StringBuilder(pcchBuffer);
                if (GetDefaultPrinter(pszBuffer, ref pcchBuffer))
                {
                    return pszBuffer.ToString();
                }
                lastWin32Error = Marshal.GetLastWin32Error();
            }
            if (lastWin32Error == ERROR_FILE_NOT_FOUND)
            {
                return null;
            }
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }
        #endregion

        #region Storage
        public void set_storage_object(string obj)
        {
            Properties.Settings.Default.storageObject = obj;
            Properties.Settings.Default.Save();
        }
        public string get_storage_object()
        {
            return Properties.Settings.Default.storageObject;
        }
        #endregion

        #region Form
        public void on_window_minimized_event(string js)
        {
            EventHandler eh = null;
            eh = (sender, e) => {
                if (FormWindowState.Minimized == F.WindowState)
                {
                    F.getWebBrowser().StringByEvaluatingJavaScriptFromString(escapeJs(js));
                }
                //F.Resize -= eh;
            };
            F.Resize += eh;
        }
        public void on_window_close_event(string js)
        {
            F.FormClosing += (sender, e) => { F.getWebBrowser().StringByEvaluatingJavaScriptFromString(escapeJs(js)); };
        }
        public void set_window_minimum_size(string w, string h)
        {
            try
            {
                F.MinimumSize = new Size(int.Parse(w), int.Parse(h));
            }
            catch (Exception e)
            {
                error("set_window_minimum_size", "set_window_minimum_size('" + w + "','" + h + "')\nInvalid Arguments");
            }
        }
        public void show_window_from_tray()
        {
            F.fromTray();
        }
        public void hide_window_to_tray()
        {
            F.toTray();
        }
        public void set_icon(string icon_path)
        {
            var icon_bitmap = getImageByAddress(icon_path) as Bitmap;
            IntPtr icon_handler = icon_bitmap.GetHicon();
            F.Icon = tempFicon = Icon.FromHandle(icon_handler);
        }
        public void set_badge_text(string text, string font_size, string color, string position)
        {
            if (tempFicon == null) tempFicon = F.Icon;
            int _font_size = 19;
            int.TryParse(font_size, out _font_size);
            Bitmap badgetBitmap = DynamicIcon.getBadgedBitmapImage(tempFicon.ToBitmap(), text, _font_size, System.Drawing.ColorTranslator.FromHtml(color),position);
            IntPtr iconHandler = badgetBitmap.GetHicon();
            F.Icon = Icon.FromHandle(iconHandler);
        }
        public void set_tray_icon(string icon_path)
        {
            var notify_icon = F.getNotifyIcon();
            var icon_handler = ((Bitmap)getImageByAddress(icon_path)).GetHicon();
            notify_icon.Icon = Icon.FromHandle(icon_handler);
        }
        public void show_tray_baloon(string title, string text, string icon_type, string delay, string onClick)
        {
            ToolTipIcon icon;
            switch (icon_type)
            {
                case "error":
                    icon = ToolTipIcon.Error;
                    break;
                case "info":
                    icon = ToolTipIcon.Info;
                    break;
                case "warning":
                    icon = ToolTipIcon.Warning;
                    break;
                default:
                    icon = ToolTipIcon.None;
                    break;
            }
            var web = F.getWebBrowser();
            var notify_icon = F.getNotifyIcon();
            EventHandler eh = null;
            eh = (s, e) =>
            {
                web.StringByEvaluatingJavaScriptFromString(escapeJs(onClick));
                notify_icon.BalloonTipClicked -= eh;
            };
            notify_icon.BalloonTipClicked += eh;
            notify_icon.BalloonTipText = text;
            notify_icon.BalloonTipIcon = icon;
            notify_icon.BalloonTipTitle = title;
            notify_icon.Visible = true;
            int _delay = 1000;
            int.TryParse(delay, out _delay);
            notify_icon.ShowBalloonTip(_delay);
        }
        public void set_tray_icon_visible(string v)
        {
            bool _v = false;
            bool.TryParse(v, out _v);
            F.getNotifyIcon().Visible = _v;
        }
        #endregion;

        #region Printing
        public string[] get_system_printers()
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

        private void applyPrintOptionsFromString(string options, ref _WebKitBrowser web)
        {
            PrintOptions po;
            //Get User Params            
            po = Newtonsoft.Json.JsonConvert.DeserializeObject<PrintOptions>(options);
            //Converting
            po.marginLeft = mm2inch(po.marginLeft);
            po.marginTop = mm2inch(po.marginTop);
            po.marginRight = mm2inch(po.marginRight);
            po.marginBottom = mm2inch(po.marginBottom);
            //Apply Options
            web.PageSettings.PaperSize = new System.Drawing.Printing.PaperSize("YesPos", mm2inch(po.paperWidth), mm2inch(po.paperHeight));
            web.PageSettings.Margins = new System.Drawing.Printing.Margins(po.marginLeft, po.marginRight, po.marginTop, po.marginBottom);
            web.PageSettings.PrinterSettings.PrinterName = po.printer;
            //Set SystemDefaultPrinter
            var tempDefPinter = get_system_default_printer();
            if (tempDefPinter != po.printer)
            {
                SetDefaultPrinter(po.printer);
            }
        }

        public void set_default_print_options(string options)
        {
            try
            {
                PrintOptions po = Newtonsoft.Json.JsonConvert.DeserializeObject<PrintOptions>(options);
                Properties.Settings.Default.printerOptions = Newtonsoft.Json.JsonConvert.SerializeObject(options);
                Properties.Settings.Default.Save();
            }
            catch (Exception e)
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

        public void print_url(string url, string options)
        {
            if (String.IsNullOrEmpty(options)) options = get_default_print_options();
            var webTemp = new _WebKitBrowser();
            F.Controls.Add(webTemp);
            webTemp.Navigate(url);
            webTemp.DocumentCompleted += (sender, e) =>
            {
                applyPrintOptionsFromString(options, ref webTemp);
                webTemp.Print();
                F.Controls.Remove(webTemp);
            };
            webTemp.Error += (sender, e) => { F.Controls.Remove(webTemp); };
        }

        public void print_html(string html, string options)
        {
            if (String.IsNullOrEmpty(options)) options = get_default_print_options();
            /*var tempPath = System.IO.Path.GetTempPath();
            var uniq = DateTime.Now.Ticks;
            var tempFile = tempPath + "\\__yes_pos_print_" + uniq + ".html";
            System.IO.File.WriteAllText(tempFile, html);
            print_url(Global.getSystemUrl(tempFile), options);*/
            var webTemp = new _WebKitBrowser();
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

        #region Notification
        public void show_notification(string options_json_string)
        {
            JavaScript.NotificationOptions options = null;
            try
            {
                options = Newtonsoft.Json.JsonConvert.DeserializeObject<JavaScript.NotificationOptions>(options_json_string);
            }
            catch (Exception e)
            {
                error("show_notification", "Invalid Options Format");
                return;
            }

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
                EventHandler eh = null;
                eh = (sender, e) => { F.getWebBrowser().StringByEvaluatingJavaScriptFromString(escapeJs(options.OnClick)); p.Hide(); p.Click -= eh; };
                p.Click += eh;
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
        public string context_menu_add_separator(string type, string ids_path_string)
        {
            return context_menu_add_item(type, ids_path_string, "", "", "");
        }
        public string context_menu_add_item(string type, string ids_path_string, string text, string image, string OnClick)
        {
            string result = "";
            if(ids_path_string!=null && ids_path_string.StartsWith("/"))
            {
                ids_path_string = ids_path_string.Substring(1);
            }
            var ids_path = (String.IsNullOrEmpty(ids_path_string))? null : ids_path_string.Split('/');
            var menu = (type == "tray") ? F.getTrayContextMenuStrip() : F.getContextMenuStrip();
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
            try
            {
                if (ids_path == null)
                {
                    return menu.Items.Add(new_item).ToString();
                }
                else
                {
                    ToolStripMenuItem item = null;
                    for (int i = 0; i < ids_path.Length; i++)
                    {
                        result += ids_path[i] + "/";
                        item = (item == null) ? (ToolStripMenuItem)menu.Items[int.Parse(ids_path[i])] : (ToolStripMenuItem)item.DropDownItems[int.Parse(ids_path[i])];
                    }
                    result += item.DropDownItems.Add(new_item);
                }
            }
            catch (Exception e)
            {
                error("context_menu_add_item", "Ivalid Arguments\n context_menu_add_item('"+type+"', '"+ids_path_string+"', '"+text+"', '"+image+"', '"+OnClick+"')");
            }
            return result;
        }
        public void context_menu_remove_item(string type, string ids_path_string)
        {
            if (ids_path_string != null && ids_path_string.StartsWith("/"))
            {
                ids_path_string = ids_path_string.Substring(1);
            }
            var ids_path = String.IsNullOrEmpty(ids_path_string) ? null : ids_path_string.Split('/');
            var menu = (type == "tray") ? F.getTrayContextMenuStrip() : F.getContextMenuStrip();
            try
            {
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
                            remove_item = (ToolStripMenuItem)parent_item.DropDownItems[int.Parse(ids_path[i])];
                        }
                    }
                    parent_item.DropDownItems.Remove(remove_item);
                }
            }
            catch (Exception e)
            {
                error("context_menu_remove_item", "context_menu_remove_item('" + type + "','" + ids_path_string + "')\nIvalid Arguments");
            }
        }
        #endregion
    }
}
