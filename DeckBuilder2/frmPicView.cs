using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DeckBuilder2
{
    public partial class frmPicView : Form
    {
        public frmPicView()
        {
            InitializeComponent();
        }

        private void frmPicView_Load(object sender, EventArgs e)
        {
            Global.frmPicViewHolder = this;
        }

        public void ShowPic(string filename)
        {
            try
            {
                this.pictureBox1.Load(filename);
            }
            catch
            {

            }
            this.BringToFront();
            this.Show();
        }

        private void frmPicView_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Visible = false;
            e.Cancel = true;
        }
    }
}
