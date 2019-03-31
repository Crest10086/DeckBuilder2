using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;
using Tools;
using BaseCardLibrary;

namespace CardXDict
{
    public class CXDConfig : Config
    {
        private static CXDConfig instance = null;

        protected override string GetFileName()
        {
            Global.appPath = Application.ExecutablePath;
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
            return "CardXDict";
        }

        protected override void OnValueChanged(string key, string value)
        {
            if (string.Equals(key, "AutoRun", StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(value, "True", StringComparison.OrdinalIgnoreCase)) //设置开机自启动
                {
                    //MessageBox.Show ("设置开机自启动需要修改注册表","提示");
                    string path = Application.ExecutablePath;
                    RegistryKey rk = Registry.LocalMachine;
                    RegistryKey rk2 = rk.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
                    rk2.SetValue("CardXDict", path);
                    rk2.Close();
                    rk.Close();
                }
                else //取消开机自启动
                {
                    //MessageBox.Show ("取消开机自启动需要修改注册表","提示");
                    string path = Application.ExecutablePath;
                    RegistryKey rk = Registry.LocalMachine;
                    RegistryKey rk2 = rk.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
                    rk2.DeleteValue("CardXDict", false);
                    rk2.Close();
                    rk.Close();
                }
                return;
            }

            if (string.Equals(key, "GetWordMode", StringComparison.OrdinalIgnoreCase))
            {
                switch (value)
                {
                    case "0":
                        Global.frmDictHolder.gp.GrabMode = XDICTGRB.XDictGrabModeEnum.XDictGrabMouse;
                        break;
                    case "1":
                        Global.frmDictHolder.gp.GrabMode = XDICTGRB.XDictGrabModeEnum.XDictGrabMouseWithCtrl;
                        break;
                    case "2":
                        Global.frmDictHolder.gp.GrabMode = XDICTGRB.XDictGrabModeEnum.XDictGrabMouseWithShift;
                        break;
                    case "3":
                        Global.frmDictHolder.gp.GrabMode = XDICTGRB.XDictGrabModeEnum.XDictGrabMouseWithMiddleButton;
                        break;
                    default:
                        Global.frmDictHolder.gp.GrabMode = XDICTGRB.XDictGrabModeEnum.XDictGrabMouse;
                        break;
                }
                return;
            }

            if (string.Equals(key, "Interval", StringComparison.OrdinalIgnoreCase))
            {
                int i = 30;
                if (!int.TryParse(value, out i))
                    i = 30;
                Global.frmDictHolder.gp.GrabInterval = i;
                return;
            }

            if (string.Equals(key, "Opacity", StringComparison.OrdinalIgnoreCase))
            {
                int i = 90;
                if (!int.TryParse(value, out i))
                    i = 90;
                Global.frmFloatHolder.Opacity = i;
                return;
            }
        }

        private string MapPath(string path)
        {
            if (path.IndexOf(':') == -1)
                return Global.appPath + "\\" + path;
            else
                return path;
        }

        protected override void SetDefaultValue()
        {
            Settings = new Hashtable();
            Settings.Add("GetWordMode", "0");
            Settings.Add("Opacity", "90");
            Settings.Add("Interval", "30");
            Settings.Add("MinimizeOnLoad", "True");
            Settings.Add("HideToTrayOnMinimized", "True");
            Settings.Add("HideToTrayOnClose", "True");
            Settings.Add("ChooseByClick", "False");
            Settings.Add("AutoRun", "False");
            Keys = new string[Settings.Count];
            Settings.Keys.CopyTo(Keys, 0);
        }

        public static new CXDConfig GetInstance()
        {
            if (instance == null)
                instance = new CXDConfig();
            return instance;
        }
    }
}
