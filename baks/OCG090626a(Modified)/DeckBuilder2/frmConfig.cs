using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using BaseCardLibrary;

namespace DeckBuilder2
{
    public partial class frmConfig : Form
    {
        public frmConfig()
        {
            InitializeComponent();
        }

        private void buttonX2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonX1_Click(object sender, EventArgs e)
        {
            DB2Config config = DB2Config.GetInstance();
            CLConfig clconfig = CLConfig.GetInstance();

            if (checkBoxX1.Checked)
            {
                config.SetSettingNext("NotShowIco", "True");
            }
            else
            {
                config.SetSettingNext("NotShowIco", "False");
            }

            if (checkBoxX2.Checked)
            {
                config.SetSettingNext("NoVirtualMode", "True");
            }
            else
            {
                config.SetSettingNext("NoVirtualMode", "False");
            }

            if (checkBoxX3.Checked)
            {
                clconfig.SetSetting("AllowForbiddenCard", "True");
            }
            else
            {
                clconfig.SetSetting("AllowForbiddenCard", "False");
            }

            if (checkBoxX4.Checked)
            {
                config.SetSetting("NoDrag", "True");
            }
            else
            {
                config.SetSetting("NoDrag", "False");
            }

            if (checkBoxX5.Checked)
            {
                clconfig.SetSetting("AllowDIY", "True");
            }
            else
            {
                clconfig.SetSetting("AllowDIY", "False");
            }

            if (checkBoxX6.Checked)
            {
                config.SetSetting("SaveLayout", "True");
            }
            else
            {
                config.SetSetting("SaveLayout", "False");
            }

            config.SetSetting("ImagePath", textBox1.Text);

            config.SetSetting("IcoPath", textBox2.Text);

            config.SetSetting("NBXPath", textBox3.Text);

            config.SetSetting("DeckPath", textBox4.Text);

            this.Close();
        }

        private void frmConfig_Load(object sender, EventArgs e)
        {
            DB2Config config = DB2Config.GetInstance();
            CLConfig clconfig = CLConfig.GetInstance();

            if (string.Equals(config.GetSettingNext("NotShowIco"), "True", StringComparison.OrdinalIgnoreCase))
                checkBoxX1.Checked = true;

            if (string.Equals(config.GetSettingNext("NoVirtualMode"), "True", StringComparison.OrdinalIgnoreCase))
                checkBoxX2.Checked = true;

            if (string.Equals(clconfig.GetSetting("AllowForbiddenCard"), "True", StringComparison.OrdinalIgnoreCase))
                checkBoxX3.Checked = true;

            if (string.Equals(config.GetSettingNext("NoDrag"), "True", StringComparison.OrdinalIgnoreCase))
                checkBoxX4.Checked = true;

            if (!string.Equals(clconfig.GetSettingNext("AllowDIY"), "True", StringComparison.OrdinalIgnoreCase))
                checkBoxX5.Checked = false;

            if (!string.Equals(config.GetSetting("SaveLayout"), "True", StringComparison.OrdinalIgnoreCase))
                checkBoxX6.Checked = false;

            textBox1.Text = config.GetSetting("ImagePath");

            textBox2.Text = config.GetSetting("IcoPath");

            textBox3.Text = config.GetSetting("NBXPath");

            textBox4.Text = config.GetSetting("DeckPath");
        }

        private void buttonX3_Click(object sender, EventArgs e)
        {
            string s = null;
            FolderBrowserDialog openFolderDialog1 = new FolderBrowserDialog();
            openFolderDialog1.RootFolder = Environment.SpecialFolder.MyComputer;
            if (openFolderDialog1.ShowDialog() == DialogResult.OK)
            {
                s = openFolderDialog1.SelectedPath;
                if (s[s.Length - 1] != '\\')
                    s = s + "\\";
                textBox1.Text = s;
            }
        }

        private void buttonX4_Click(object sender, EventArgs e)
        {
            string s = null;
            FolderBrowserDialog openFolderDialog1 = new FolderBrowserDialog();
            openFolderDialog1.RootFolder = Environment.SpecialFolder.MyComputer;
            if (openFolderDialog1.ShowDialog() == DialogResult.OK)
            {
                s = openFolderDialog1.SelectedPath;
                if (s[s.Length - 1] != '\\')
                    s = s + "\\";
                textBox2.Text = s;
            }
        }

        private void buttonX5_Click(object sender, EventArgs e)
        {
            string s = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            //openFileDialog1.InitialDirectory = "D:\\Patch";
            openFileDialog1.Filter = "NBX files (NetBattleX2.exe)|NetBattleX*.exe|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 0;
            openFileDialog1.RestoreDirectory = true;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                s = openFileDialog1.FileName;
                textBox3.Text = s;

                if (MessageBox.Show("�Ƿ�NBX�Ŀ�ͼ�Ϳ���Ŀ¼��ΪĬ�ϵĿ�ͼ�Ϳ���Ŀ¼��", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    System.IO.FileInfo fi = new System.IO.FileInfo(s);
                    string nbxdir = fi.DirectoryName;
                    if (fi.Exists)
                    {
                        textBox4.Text = nbxdir + "\\data\\deck\\";
                        textBox1.Text = nbxdir + "\\data\\image\\";
                    }
                }
            }
        }

        private void buttonX6_Click(object sender, EventArgs e)
        {
            string s = null;
            FolderBrowserDialog openFolderDialog1 = new FolderBrowserDialog();
            openFolderDialog1.RootFolder = Environment.SpecialFolder.MyComputer;
            if (openFolderDialog1.ShowDialog() == DialogResult.OK)
            {
                s = openFolderDialog1.SelectedPath;
                if (s[s.Length - 1] != '\\')
                    s = s + "\\";
                textBox4.Text = s;
            }
        }
    }
}