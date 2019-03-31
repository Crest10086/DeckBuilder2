using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DeckBuilder2
{


    public class Config
    {
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section,
         string key, string def, StringBuilder retVal,
         int size, string filePath);

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section,
         string key, string val, string filePath);


        private static Config instance = null;
        private Hashtable Settings = null;
        private string filename = null;

        private string MapPath(string path)
        {
            if (path.IndexOf(':') == -1)
                return Global.appPath + "\\" + path;
            else
                return path;
        }

        public Config()
        {
            Global.appPath = Application.ExecutablePath;
            for (int i = Global.appPath.Length - 1; i >= 0; i--)
                if (Global.appPath[i] == '\\')
                {
                    Global.appPath = Global.appPath.Substring(0, i);
                    break;
                }
            filename =  Global.appPath + "\\Settings.ini";
            Settings = new Hashtable();

            if (!System.IO.File.Exists(filename))
            {
                WritePrivateProfileString("Settings", "NotShowIco", "False", filename);
                WritePrivateProfileString("Settings", "NoVirtualMode", "False", filename);
                WritePrivateProfileString("Settings", "AllowForbiddenCard", "False", filename);
                WritePrivateProfileString("Settings", "DeckEdit", "Ico", filename);
                WritePrivateProfileString("Settings", "DeckView", "Ico", filename);
                WritePrivateProfileString("Settings", "CardView", "List", filename);
                WritePrivateProfileString("Settings", "ImagePath", ".\\Image\\", filename);
                WritePrivateProfileString("Settings", "IcoPath", ".\\LargeIco\\", filename);
                WritePrivateProfileString("Settings", "NBXPath", ".\\NetBattleX2.exe", filename);
                WritePrivateProfileString("Settings", "DeckPath", ".\\", filename);
                WritePrivateProfileString("Settings", "FirstTimeRun", "True", filename);
                WritePrivateProfileString("Settings", "CloseButtonOnTab", "False", filename);
                WritePrivateProfileString("Settings", "SaveType", "0", filename);
                WritePrivateProfileString("Settings", "NoDrag", "False", filename);
            }

            StringBuilder s = new StringBuilder(255);
            GetPrivateProfileString("Settings", "NotShowIco", "False", s, 255, filename);
            Settings.Add("NotShowIco", s.ToString());

            GetPrivateProfileString("Settings", "NoVirtualMode", "False", s, 255, filename);
            Settings.Add("NoVirtualMode", s.ToString());

            GetPrivateProfileString("Settings", "AllowForbiddenCard", "False", s, 255, filename);
            Settings.Add("AllowForbiddenCard", s.ToString());

            GetPrivateProfileString("Settings", "DeckEdit", "Ico", s, 255, filename);
            Settings.Add("DeckEdit", s.ToString());

            GetPrivateProfileString("Settings", "DeckView", "Ico", s, 255, filename);
            Settings.Add("DeckView", s.ToString());

            GetPrivateProfileString("Settings", "CardView", "List", s, 255, filename);
            Settings.Add("CardView", s.ToString());

            GetPrivateProfileString("Settings", "ImagePath", ".\\Image\\", s, 255, filename);
            Settings.Add("ImagePath", MapPath(s.ToString()));

            GetPrivateProfileString("Settings", "IcoPath", ".\\LargeIco\\", s, 255, filename);
            Settings.Add("IcoPath", MapPath(s.ToString()));

            GetPrivateProfileString("Settings", "NBXPath", ".\\NetBattleX2.exe", s, 255, filename);
            Settings.Add("NBXPath", MapPath(s.ToString()));

            GetPrivateProfileString("Settings", "DeckPath", ".\\", s, 255, filename);
            Settings.Add("DeckPath", MapPath(s.ToString()));

            GetPrivateProfileString("Settings", "FirstTimeRun", "True", s, 255, filename);
            Settings.Add("FirstTimeRun", s.ToString());

            GetPrivateProfileString("Settings", "CloseButtonOnTab", "False", s, 255, filename);
            Settings.Add("CloseButtonOnTab", s.ToString());

            GetPrivateProfileString("Settings", "SaveType", "0", s, 255, filename);
            Settings.Add("SaveType", s.ToString());

            GetPrivateProfileString("Settings", "NoDrag", "False", s, 255, filename);
            Settings.Add("NoDrag", s.ToString());
        }

        public static Config GetInstance()
        {
            if (instance == null)
                instance = new Config();
            return instance;
        }

        public string GetSetting(string key)
        {
            if (Settings[key] == null)
                return "";
            else
                return ((String)Settings[key]);
        }

        public string GetSettingNext(string key)
        {
            StringBuilder s = new StringBuilder(255);
            try
            {
                string filename = Global.appPath + "\\Settings.ini";
                GetPrivateProfileString("Settings", key, "False", s, 255, filename);
            }
            catch
            {
            }

            return s.ToString();
        }

        public void SetSetting(string key, string value)
        {
            try
            {
                Settings[key] = value;
                WritePrivateProfileString("Settings", key, value, filename);
            }
            catch
            {
            }
        }

        public void SetSettingNext(string key, string value)
        {
            try
            {
                WritePrivateProfileString("Settings", key, value, filename);
            }
            catch
            {
            }
        }
    }
}
