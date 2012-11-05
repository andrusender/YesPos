using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
namespace YesPos
{
    public class DynamicIcon
    {
        private Form _Form;
        
        public DynamicIcon(Form f)
        {
            _Form = f;
        }
        public void Refresh(string icon_path, string text,int text_size, Color color){
            using (Bitmap bitmap = CreateBitmapImage(icon_path,text,text_size,color))
            {
                IntPtr sourceIcon = bitmap.GetHicon();
                _Form.Icon = Icon.FromHandle(sourceIcon);                
            }
        }
        
        private Bitmap CreateBitmapImage(string icon_file, string text,int text_size, Color color)
        {
            var file = Global.ImagePath+icon_file;
            Bitmap objBmpImage = Bitmap.FromFile(file) as Bitmap;
            Font objFont = new Font("Arial",text_size, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            Graphics objGraphics = Graphics.FromImage(objBmpImage);
            objGraphics = Graphics.FromImage(objBmpImage);
            objGraphics.SmoothingMode = SmoothingMode.AntiAlias;
            objGraphics.TextRenderingHint = TextRenderingHint.AntiAlias;
            int intWidth = (int)objGraphics.MeasureString(text, objFont).Width;
            int intHeight = (int)objGraphics.MeasureString(text, objFont).Height;
            objGraphics.DrawString(text, objFont, new SolidBrush(color), objBmpImage.Width - intWidth-0, 0);
            objGraphics.Flush();
            return (objBmpImage);
        }
    }
}
