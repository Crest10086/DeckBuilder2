using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using MyTools;
using BaseCardLibrary.Search;

namespace BaseCardLibrary
{


    public class CLConfig : Config
    {
        private static CLConfig instance = null;

        protected override string GetFileName()
        {
            ;
            return System.AppDomain.CurrentDomain.BaseDirectory + "Settings.ini";
        }

        protected override string GetSection()
        {
            return "CardLibrary";
        }

        protected override void OnValueChanged(string key, string value)
        {
            if (key == "AllowDIY")
            {
                if (string.Equals(value, "True", StringComparison.OrdinalIgnoreCase))
                    CardLibrary.GetInstance().AllowDIY = true;
                else
                    CardLibrary.GetInstance().AllowDIY = false;
            }
        }

        private string MapPath(string path)
        {
            if (path.IndexOf(':') == -1)
                return System.AppDomain.CurrentDomain.BaseDirectory + path;
            else
                return path;
        }

        protected override void SetDefaultValue()
        {
            Settings = new Hashtable();
            Settings.Add("AllowForbiddenCard", "False");
            Settings.Add("AllowDIY", "True");
            Settings.Add("KeepInMemory", "True");
            Keys = new string[Settings.Count];
            Settings.Keys.CopyTo(Keys, 0);
        }

        public static new CLConfig GetInstance()
        {
            if (instance == null)
                instance = new CLConfig();
            return instance;
        }
    }
}
