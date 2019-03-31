using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace AppInterface {

    public class mAppInterface {

        [DllImport("OcgsoftIntf.dll")]
        public static extern void DoUploadFile(string AFileName, ref string AServerFile, ref string err);

        [DllImport("OcgsoftIntf.dll")]
        public static extern void DoDownloadFile(string AFileName, string ASavePath, ref string err);

        [DllImport("OcgsoftIntf.dll")]
        public static extern void SetProgramInfo(string AppName, string APath, int AVersion);

        [DllImport("OcgsoftIntf.dll")]
        public static extern void SetCardImgDir(string ADir);

        [DllImport("OcgsoftIntf.dll")]
        public static extern void GetCardImgDir(ref string ADir);

        [DllImport("OcgsoftIntf.dll")]
        public static extern void SetDIYCardImgDir(string ADir);

        [DllImport("OcgsoftIntf.dll")]
        public static extern void GetDIYCardImgDir(ref string ADir);
    
    }
}
