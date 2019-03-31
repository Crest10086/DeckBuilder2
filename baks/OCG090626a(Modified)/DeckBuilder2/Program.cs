using System;
using System.Collections.Generic;
using System.Windows.Forms;

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

            if (string.Equals(DB2Config.GetInstance().GetSetting("FirstTimeRun"), "True", StringComparison.OrdinalIgnoreCase))
            {
                string icodir = DB2Config.GetInstance().GetSetting("IcoPath");
                string testicofile = icodir + "1.jpg";
                if (!System.IO.File.Exists(testicofile))
                {
                    DB2Config.GetInstance().SetSetting("FirstTimeRun", "False");
                    if (MessageBox.Show("���ǵ�һ�����б�����Ϊ�����������鱾��������й��ܣ��������и���ת�����ߣ�ConveterTools.exe����������ͼ���Ƿ���������ͼ��", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
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

            Application.Run(new frmMain());
            //Application.Run(new frmDeckView());
        }
    }
}