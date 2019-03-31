using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DeckBuilder2
{
    public partial class frmAbout : Form
    {
        public frmAbout()
        {
            InitializeComponent();
        }

        private void frmAbout_Load(object sender, EventArgs e)
        {
            labelX1.Text = string.Format(@"游戏王GAME图形组卡器2
V{0} build {1} ({3})

作者：AI_Player
Email：ai.player@gmail.com
QQ群：86204100
版权：中国OCG工作组
网址：http://www.ocgsoft.cn

程序测试：{2}

感谢：青眼小白、虫虫、逆卷炎灵、DK", Global.Version, Global.Build, Global.ProgramDebug, Global.DataVersion);
        }
    }
}