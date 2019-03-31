using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

namespace Tools
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
        protected Hashtable Settings = null;
        protected string[] Keys = null;
        private string FileName = null;
        private string Section = null;

        protected virtual string GetFileName()
        {
            return "";
        }

        protected virtual string GetSection()
        {
            return "";
        }

        protected virtual void SetDefaultValue()
        {
            Settings = new Hashtable();
        }

        protected virtual void OnValueChanged(string key, string value)
        {
        }

        public Config()
        {
            FileName = GetFileName();
            Section = GetSection();
            SetDefaultValue();

            StringBuilder s = new StringBuilder(255);

            try
            {
                foreach (string key in Keys)
                {
                    GetPrivateProfileString(Section, key, "", s, 255, FileName);
                    if (s.Length > 0)
                        Settings[key] = s.ToString();
                    else
                        WritePrivateProfileString(Section, key, (string)Settings[key], FileName);
                }
            }
            catch
            {
                ////MessageBox.Show("读取配置文件出错！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static Config GetInstance()
        {
            if (instance == null)
                instance = new Config();
            return instance;
        }

        public static int GetIntValue(string value)
        {
            return GetIntValue(value, 0);
        }

        public static int GetIntValue(string value, int defaultvalue)
        {
            try
            {
                return int.Parse(value);
            }
            catch
            {
                return defaultvalue;
            }
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
                GetPrivateProfileString(Section, key, (string)Settings[key], s, 255, FileName);
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
                WritePrivateProfileString(Section, key, value, FileName);
                OnValueChanged(key, value);
            }
            catch
            {
            }
        }

        public void SetSettingNext(string key, string value)
        {
            try
            {
                WritePrivateProfileString(Section, key, value, FileName);
            }
            catch
            {
            }
        }
    }
}
