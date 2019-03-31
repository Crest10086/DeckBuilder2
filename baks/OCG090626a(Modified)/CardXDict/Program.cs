using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CardXDict
{
    static class Program
    {
        private static System.Threading.Mutex Mutex = null;


        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {

            Mutex = new System.Threading.Mutex(false, Application.ProductName);
            if (Mutex.WaitOne(0, false) == false)
            {
                Mutex.Close();
                //   提示“程序已运行！”   
                MessageBox.Show("卡片词霸已经运行！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }   

            string err = Register("xdictGrb.dll");
            if (err == null)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new frmDict());
            }
            else
            {
                MessageBox.Show(err, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static string Register(string filename)
        {
            if (System.IO.File.Exists(filename))
            {
                System.Diagnostics.Process.Start("regsvr32.exe", "/s " + filename);
                return null;
            }
            else
            {
                return filename + "文件未找到，请重新安装本软件！";
            }
        }
    }
}
