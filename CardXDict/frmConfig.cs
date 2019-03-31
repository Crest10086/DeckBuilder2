using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CardXDict
{
    public partial class frmConfig : DevComponents.DotNetBar.Office2007Form//Form
    {
        private bool configchanged = false;
        private bool ConfigChanged
        {
            get
            {
                return configchanged;
            }
            set
            {
                configchanged = value;
                buttonX3.Enabled = value;
            }
        }

        public frmConfig()
        {
            InitializeComponent();
        }

        private void SaveConfig()
        {
            CXDConfig config = CXDConfig.GetInstance();
            config.SetSetting("AutoRun", checkBox1.Checked.ToString());
            config.SetSetting("MinimizeOnLoad", checkBox2.Checked.ToString());
            config.SetSetting("HideToTrayOnClose", checkBox3.Checked.ToString());
            config.SetSetting("HideToTrayOnMinimized", checkBox4.Checked.ToString());
            config.SetSetting("ChooseByClick", checkBox5.Checked.ToString());

            config.SetSetting("GetWordMode", comboBoxEx1.SelectedIndex.ToString());
            config.SetSetting("Interval", comboBoxEx2.Text);
            config.SetSetting("Opacity", slider1.Value.ToString());
        }

        private void frmConfig_Load(object sender, EventArgs e)
        {
            CXDConfig config = CXDConfig.GetInstance();

            //常规
            checkBox1.Checked = string.Equals(config.GetSetting("AutoRun"), "True", StringComparison.OrdinalIgnoreCase);
            checkBox2.Checked = string.Equals(config.GetSetting("MinimizeOnLoad"), "True", StringComparison.OrdinalIgnoreCase);
            checkBox3.Checked = string.Equals(config.GetSetting("HideToTrayOnClose"), "True", StringComparison.OrdinalIgnoreCase);
            checkBox4.Checked = string.Equals(config.GetSetting("HideToTrayOnMinimized"), "True", StringComparison.OrdinalIgnoreCase);
            checkBox5.Checked = string.Equals(config.GetSetting("ChooseByClick"), "True", StringComparison.OrdinalIgnoreCase);
                
            //取词
            comboBoxEx1.SelectedIndex = MyTools.Config.GetIntValue(config.GetSetting("GetWordMode"), 0);
            comboBoxEx2.Text = MyTools.Config.GetIntValue(config.GetSetting("Interval"), 30).ToString();
            slider1.Value = MyTools.Config.GetIntValue(config.GetSetting("Opacity"), 90);

            ConfigChanged = false;
        }

        private void slider1_ValueChanged(object sender, EventArgs e)
        {
            slider1.Text = slider1.Value.ToString() + "%";
            ConfigChanged = true;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            ConfigChanged = true;
        }

        private void buttonX2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonX3_Click(object sender, EventArgs e)
        {
            SaveConfig();
            ConfigChanged = false;
        }

        private void buttonX1_Click(object sender, EventArgs e)
        {
            SaveConfig();
            this.Close();
        }
    }
}
