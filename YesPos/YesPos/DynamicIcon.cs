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
        public void Refresh(Bitmap icon, string text,int text_size, Color color){
            using (Bitmap bitmap = getBadgedBitmapImage(icon,text,text_size,color,""))
            {
                IntPtr sourceIcon = bitmap.GetHicon();
                _Form.Icon = Icon.FromHandle(sourceIcon);                
            }
        }

        public static Bitmap getBadgedBitmapImage(Bitmap objBmpImage, string text, int text_size, Color color, string position)
        {            
            Font objFont = new Font("Arial",text_size, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            Graphics objGraphics = Graphics.FromImage(objBmpImage);
            objGraphics = Graphics.FromImage(objBmpImage);
            objGraphics.SmoothingMode = SmoothingMode.AntiAlias;
            objGraphics.TextRenderingHint = TextRenderingHint.AntiAlias;
            int intWidth = (int)objGraphics.MeasureString(text, objFont).Width;
            int intHeight = (int)objGraphics.MeasureString(text, objFont).Height;
            int x=0, y=0;
            switch (position)
            {
                case "topRight":
                    x = objBmpImage.Width - intWidth;
                    break;
                case "bottomLeft":
                    y = objBmpImage.Height - intHeight;
                    break;
                case "bottomRight":
                    x = objBmpImage.Width - intWidth;
                    y = objBmpImage.Height - intHeight;
                    break;
                case "center":
                    x = objBmpImage.Width/2 - intWidth/2;
                    y = objBmpImage.Height/2 - intHeight/2;
                    break;
            }
            //objGraphics.DrawString(text, objFont, new SolidBrush(color), objBmpImage.Width - intWidth-0, 0);
            objGraphics.DrawString(text, objFont, new SolidBrush(color), x,y);
            objGraphics.Flush();
            return (objBmpImage);
        }
    }
}
