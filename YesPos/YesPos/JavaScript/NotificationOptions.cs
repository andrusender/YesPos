using System;
using System.Collections.Generic;
using System.Text;

namespace YesPos.JavaScript
{
    class NotificationOptions
    {
        public int Delay = 2000;
        public int AnimationDuration = 500;
        public string CloseButtonColor = "#3e8af3";
        public string ButtonHoverColor = "#ffffff";
        public string ContentHoverColor = "#3e8af3";
        public string CloseButtonBorderColor = "#3e8af3";
        public string Title="";
        public string Content="";
        public string Image="";

        public int Width = 350;
        public int Height = 80;

        public string BorderColor = "#3e8af3";
        public string BackgroundColor = "#ffffff";
        public string TitleColor = "#3e8af3";
        public string ContentColor = "#000000";
        
        public int HeaderHeight = 5;
        public string HeaderColor = "#3e8af3";
        
        public int ImagePaddingsLeft = 10;
        public int ImagePaddingsRight = 0;
        public int ImagePaddingsTop = 10;
        public int ImagePaddingsBottom = 10;

        public int TitlePaddingsLeft = 5;
        public int TitlePaddingsRight = 10;
        public int TitlePaddingsTop = 10;
        public int TitlePaddingsBottom = 5;

        public int ContentPaddingsLeft = 5;
        public int ContentPaddingsRight = 10;
        public int ContentPaddingsTop = 0;
        public int ContentPaddingsBottom = 10;

        public string OnClick="";
    }
}
