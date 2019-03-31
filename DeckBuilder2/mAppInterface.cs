using System;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace AppInterface {

    public class mAppInterface {
        public const string AppName_DeckBuilder = "DeckBuilder";
        public const string AppName_NBX = "NBX";
        public const string AppName_NBXE = "NBXE";
        public const string AppName_YFCC = "YFCC";
        public const string AppName_DIYTool = "DIYTool";
        public const string AppName_IMAGE = "Image";

        private static char[] slash = new char[] { '\\' };

        private static string PathToDir(string path)
        {
            if (path == null)
                return null;
            return path.TrimEnd(slash);
        }

        private static string GetAppKey(string AppName)
        {
            string subkey = null;
            string appname = AppName.ToLower();

            switch (appname)
            {
                case "nbx":
                    subkey = "NBX";
                    break;
                case "nbxe":
                    subkey = "NBXE";
                    break;
                case "deckbuilder":
                    subkey = "DeckBuilder";
                    break;
                case "yfcc":
                    subkey = "YFCC";
                    break;
                case "appstore":
                    subkey = "AppStore";
                    break;
                case "image":
                    subkey = "Cards";
                    break;
            }

            subkey = string.Format("Software\\OCGSOFT\\{0}", new Object[] { subkey });

            return subkey;
        }

        /*
        [DllImport("OcgsoftIntf.dll")]
        public static extern void DoUploadFile(string AFileName, ref string AServerFile, ref string err);

        [DllImport("OcgsoftIntf.dll")]
        public static extern void DoDownloadFile(string AFileName, string ASavePath, ref string err);

        [DllImport("OcgsoftIntf.dll")]
        public static extern void SetProgramInfo(string AppName, string APath, int AVersion);

        [DllImport("OcgsoftIntf.dll")]
        public static extern void SetCardImgDir(string ADir);

        [DllImport("OcgsoftIntf.dll")]
        private static extern void GetCardImgDir(ref string ADir);
        
        [DllImport("OcgsoftIntf.dll")]
        public static extern void SetDIYCardImgDir(string ADir);

        [DllImport("OcgsoftIntf.dll")]
        private static extern void GetDIYCardImgDir(ref string ADir);
        
        [DllImport("OcgsoftIntf.dll", EntryPoint = "GetAppDir", CallingConvention = CallingConvention.StdCall)]
        private static extern void GetAppDir(string AppInnerName, ref string ADir); 
         
        [DllImport("OcgsoftIntf.dll")]
        public static extern int GetAppVersion(string AppInnerName);

        //0:未安装  1:已安装  2:可更新
        [DllImport("OcgsoftIntf.dll")]
        public static extern int QueryAppStatus(string AppInnerName, int AVersion);
        */

        /*
        public static void GetCardImgDirEx(ref string ADir)
        {
            string dir = null;
            GetCardImgDir(ref dir);
            ADir = PathToDir(dir);
        }
        
        public static void GetDIYCardImgDirEx(ref string ADir)
        {
            string dir = null;
            GetDIYCardImgDir(ref dir);
            ADir = PathToDir(dir);
        }

        public static void GetAppDirEx(string AppInnerName, ref string ADir)
        {
            string dir = null;
            GetAppDir(AppInnerName, ref dir);
            ADir = PathToDir(dir);
        }
         */

        public static void GetCardImgDirEx(ref string ADir)
        {
            RegistryKey key = Registry.CurrentUser;
            key = key.OpenSubKey(@"Software\OCGSOFT\Cards");
            if (key == null)
            {
                ADir = "";
                return;
            }
           
            ADir =  Convert.ToString(key.GetValue("Path"));
            key.Close();
        }

        public static void GetDIYCardImgDirEx(ref string ADir)
        {
            RegistryKey key = Registry.CurrentUser;
            key = key.OpenSubKey(@"Software\OCGSOFT\Cards");
            if (key == null)
            {
                ADir = "";
                return;
            }

            ADir = Convert.ToString(key.GetValue("DIYPath"));
            key.Close();
        }

        public static void GetAppDirEx(string AppInnerName, ref string ADir)
        {
            RegistryKey key = Registry.CurrentUser;
            key = key.OpenSubKey(GetAppKey(AppInnerName));
            if (key == null)
            {
                ADir = "";
                return;
            }

            ADir = Convert.ToString(key.GetValue("Path"));
            key.Close();
        }

        public static void SetProgramInfo(string AppName, string APath, int AVersion)
        {
            RegistryKey key = Registry.CurrentUser;
            key = key.OpenSubKey(GetAppKey(AppName), true);
            if (key == null)
            {
                return;
            }

            try
            {
                key.SetValue("Path", APath);
                key.SetValue("Ver", AVersion);
            }
            finally
            {
                key.Close();
            }
        }
    }
}
