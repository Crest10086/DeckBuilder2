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
            labelX1.Text = string.Format(@"��Ϸ��GAMEͼ���鿨��2
V{0} build {1} ({3})

���ߣ�AI_Player
Email��ai.player@gmail.com
QQȺ��86204100
��Ȩ���й�OCG������
��ַ��http://www.ocgsoft.cn

������ԣ�{2}

��л������С�ס���桢������顢DK", Global.Version, Global.Build, Global.ProgramDebug, Global.DataVersion);
        }
    }
}