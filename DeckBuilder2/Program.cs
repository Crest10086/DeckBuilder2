using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using AppInterface;
using MyTools;

namespace DeckBuilder2
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string appPath = Application.ExecutablePath;
            for (int i = appPath.Length - 1; i >= 0; i--)
                if (appPath[i] == '\\')
                {
                    Global.appPath =appPath.Substring(0, i);
                    break;
                }

            Patch1();

            if (string.Equals(DB2Config.GetInstance().GetSetting("FirstTimeRun"), "True", StringComparison.OrdinalIgnoreCase))
            {
                string icodir = DB2Config.GetInstance().GetSetting("IcoPath");
                string testicofile = icodir + "1.jpg";
                if (!System.IO.File.Exists(testicofile))
                {
                    DB2Config.GetInstance().SetSetting("FirstTimeRun", "False");
                    if (MessageBox.Show("���ǵ�һ�����б�����Ϊ�����������鱾��������й��ܣ��������и���ת�����ߣ�ConveterTools.exe����������ͼ���Ƿ���������ͼ��", "ѯ��", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        try
                        {
                            System.Diagnostics.Process.Start(Application.StartupPath + "\\ConveterTools.exe");
                        }
                        catch
                        {
                        }
                        return;
                    }
                }
            }

            BaseCardLibrary.Search.CardLibrary cardLibrary = BaseCardLibrary.Search.CardLibrary.GetInstance();
            Global.DataVersion = Global.InternalVersion * 100000 + cardLibrary.GetNonDIYCount();

            //ΪSoftMgr���ó�ʼ��Ϣ
            mAppInterface.SetProgramInfo("DeckBuilder", Global.appPath, Global.DataVersion);

            //��ȡĿ¼��Ϣ
            mAppInterface.GetCardImgDirEx(ref PicLoader.commonImagePath);
            PicLoader.commonImagePath = FileTools.DirToPath(PicLoader.commonImagePath);
            PicLoader.imagePath = DB2Config.GetInstance().GetSetting("ImagePath");
            PicLoader.imagePath = FileTools.DirToPath(PicLoader.imagePath);

            mAppInterface.GetDIYCardImgDirEx(ref PicLoader.commonDiyImagePath);
            PicLoader.commonDiyImagePath = FileTools.DirToPath(PicLoader.commonDiyImagePath);
            PicLoader.diyImagePath = DB2Config.GetInstance().GetSetting("DIYImagePath");
            PicLoader.diyImagePath = FileTools.DirToPath(PicLoader.diyImagePath);

            PicLoader.icoPath = DB2Config.GetInstance().GetSetting("IcoPath");
            PicLoader.diyIcoPath = DB2Config.GetInstance().GetSetting("IcoPath");

            Application.Run(new frmMain());
            //Application.Run(new frmDeckView());
        }

        static void Patch1()
        {

        }
    }
}