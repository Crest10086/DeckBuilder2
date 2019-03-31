using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using MyTools;
using BaseCardLibrary.Search;

namespace DeckBuilder2
{


    public class DB2Config : Config
    {
        private static DB2Config instance = null;

        protected override string GetFileName()
        {
            Global.appPath = System.AppDomain.CurrentDomain.BaseDirectory;
            for (int i = Global.appPath.Length - 1; i >= 0; i--)
                if (Global.appPath[i] == '\\')
                {
                    Global.appPath = Global.appPath.Substring(0, i);
                    break;
                }
            return Global.appPath + "\\Settings.ini";
        }

        protected override string GetSection()
        {
            return "DeckBuilder2";
        }

        private string MapPath(string path)
        {
            return path;

            /*
            if (path.IndexOf(':') == -1)
                return Global.appPath + "\\" + path;
            else
                return path;
             */ 
        }

        protected override void SetDefaultValue()
        {
            Settings = new Hashtable();
            Settings.Add("NotShowIco","False");
            Settings.Add("NoVirtualMode", "False");
            Settings.Add("DeckEdit1", "Ico");
            Settings.Add("DeckEdit2", "Ico");
            Settings.Add("DeckEdit3", "Ico");
            Settings.Add("DeckView1", "Ico");
            Settings.Add("DeckView2", "Ico");
            Settings.Add("DeckView3", "Ico");
            Settings.Add("CardView", "List");
            Settings.Add("ImagePath", MapPath(".\\Image\\"));
            Settings.Add("IcoPath", MapPath(".\\LargeIco\\"));
            Settings.Add("NBXPath", MapPath(".\\NetBattleX2.exe"));
            Settings.Add("DeckPath", MapPath(".\\"));
            Settings.Add("FirstTimeRun", "True");
            Settings.Add("CloseButtonOnTab", "False");
            Settings.Add("SaveType", "0");
            Settings.Add("NoDrag", "False");
            Settings.Add("DIYImagePath", MapPath(".\\diyimg\\"));
            Settings.Add("SaveLayout", "True");
            Settings.Add("RefreshInterval", "200");
            Settings.Add("LoadPicOnceNum", "10");
            Settings.Add("LoadPicInterval", "20");
            Settings.Add("ReDrawTime", "250");
            Settings.Add("isMax", "false");
            Settings.Add("Width", "1031");
            Settings.Add("Height", "750");
            Keys = new string[Settings.Count];
            Settings.Keys.CopyTo(Keys, 0);
        }

        public static new DB2Config GetInstance()
        {
            if (instance == null)
                instance = new DB2Config();
            return instance;
        }
    }
}
