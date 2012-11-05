using System;
using System.Collections.Generic;
using System.Text;
using IniParser;
namespace YesPos
{
    class Config
    {
        private IniParser.IniData ConfigHandler;
        private IniParser.FileIniDataParser FileHandler;
        private string FilePath;
        private static Config self;

        private Dictionary<string, string> ReplacingData = new Dictionary<string, string>();

        private Config(string path)
        {
            FilePath = path;
            FileHandler = new FileIniDataParser();
            ConfigHandler = FileHandler.LoadFile(path);
            //Fill Dictionary
            ReplacingData.Add("$(root)",Global.AppDir);
        }

        public static void save()
        {
            self = (self == null) ? new Config(Global.IniPath) : self;
            self.FileHandler.SaveFile(self.FilePath, self.ConfigHandler);
        }

        public static string get(string section,string key)
        {
            self = (self==null)?new Config(Global.IniPath):self;
            string temp = self.ConfigHandler[section][key];
            foreach (KeyValuePair<string,string> data in self.ReplacingData){
                temp = temp.Replace(data.Key,data.Value);
            }
            return temp;
        }
    }
}
