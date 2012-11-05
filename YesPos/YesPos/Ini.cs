using System;
using System.Collections.Generic;
using System.Text;
using IniParser;
namespace YesPos
{
    class Ini
    {
        private IniParser.IniData ConfigHandler;
        private IniParser.FileIniDataParser FileHandler;
        private string FilePath;
        private static Ini self;
        
        private Ini(string path)
        {
            FilePath = path;
            FileHandler = new FileIniDataParser();
            ConfigHandler = FileHandler.LoadFile(path);            
        }

        public static void save()
        {
            self = (self == null) ? new Ini(Global.IniPath) : self;
            self.FileHandler.SaveFile(self.FilePath, self.ConfigHandler);
        }

        public static string get(string section,string key)
        {
            self = (self==null)?new Ini(Global.IniPath):self;
            return self.ConfigHandler[section][key];
        }
    }
}
