using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
//using System.Linq;
using System.Text;
using System.Windows.Forms;
using BaseCardLibrary.DataAccess;

namespace CardXDict
{
    public partial class frmFloat : Form
    {
        public bool IsMouseIn = false;
        bool IsLocked = false;
        CardDescription currentcard = null;
        CardDescription[] cards = null;
        string keyword = null;
        int currentindex = 0;
        Point mdcursorpoint = new Point();
        Point mdformpoint = new Point();
        Point location3 = new Point();
        Point location4 = new Point(168, 6);

        public frmFloat()
        {
            InitializeComponent();
        }

        private void ShowColoredCardName()
        {
            panelEx3.Text = currentcard.name;
            panelEx3.Visible = true;
            if (currentcard.limit < 3)
            {
                panelEx3.Location = new Point(48, 14);
                panelEx4.Text = panelEx1.Text.Substring(0, panelEx1.Text.IndexOf('\n'));
                panelEx4.Visible = true;
                switch (currentcard.limit)
                {
                    case 0:
                        panelEx4.Style.ForeColor.Color = Color.Red;
                        break;
                    case 1:
                        panelEx4.Style.ForeColor.Color = Color.Blue;
                        break;
                    case 2:
                        panelEx4.Style.ForeColor.Color = Color.LimeGreen;
                        break;
                }
            }
            else
            {
                panelEx3.Location = new Point(48, 0);
                panelEx4.Visible = false;
            }
        }

        private void HideColoredCardName()
        {
            panelEx3.Visible = false;
            panelEx4.Visible = false;
        }

        public CardDescription CurrentCard
        {
            get
            {
                return currentcard;
            }
            set
            {
                currentcard = value;
                if (currentcard != null)
                {
                    panelEx1.Text = currentcard.GetAllInfo();

                    ShowColoredCardName();

                    labelX10.Text = "复制说明";
                    labelX12.Text = "查看调整";
                    if (currentcard.adjust.Trim() == "")
                    {
                        labelX12.Visible = false;
                        labelX13.Location = location3;
                    }
                    else
                    {
                        labelX12.Visible = true;
                        labelX13.Location = new Point(168, 6);
                    }
                }
                else
                {
                    panelEx1.Text = "找不到相关信息！";
                    labelX12.Visible = false;
                    labelX13.Location = location3;
                    panelEx3.Visible = false;
                    panelEx4.Visible = false;
                }
            }
        }

        public CardDescription[] Cards
        {
            get
            {
                return cards;
            }
            set
            {
                cards = value;
                if (cards.Length > 0)
                    CurrentCard = cards[0];
                else
                    CurrentCard = null;

                if (cards.Length > 0)
                {
                    labelX1.Text = cards[0].name;
                    labelX1.Visible = true;
                }
                else
                {
                    labelX1.Visible = false;
                }

                if (cards.Length > 1)
                {
                    labelX2.Text = cards[1].name;
                    labelX2.Visible = true;
                }
                else
                {
                    labelX2.Visible = false;
                }

                if (cards.Length > 2)
                {
                    labelX3.Text = cards[2].name;
                    labelX3.Visible = true;
                }
                else
                {
                    labelX3.Visible = false;
                }

                if (cards.Length > 3)
                {
                    labelX4.Text = cards[3].name;
                    labelX4.Visible = true;
                }
                else
                {
                    labelX4.Visible = false;
                }

                if (cards.Length > 4)
                {
                    labelX5.Text = cards[4].name;
                    labelX5.Visible = true;
                }
                else
                {
                    labelX5.Visible = false;
                }

                if (cards.Length > 5)
                {
                    labelX6.Text = cards[5].name;
                    labelX6.Visible = true;
                }
                else
                {
                    labelX6.Visible = false;
                }

                if (cards.Length > 6)
                {
                    labelX7.Text = cards[6].name;
                    labelX7.Visible = true;
                }
                else
                {
                    labelX7.Visible = false;
                }

                if (cards.Length > 7)
                {
                    labelX8.Text = cards[7].name;
                    labelX8.Visible = true;
                }
                else
                {
                    labelX8.Visible = false;
                }
            }
        }

        public string KeyWord
        {
            get
            {
                return keyword;
            }
            set
            {
                keyword = value;
                pnlKeyword.Text = value;
            }
        }

        private void frmFloat_Load(object sender, EventArgs e)
        {
            Global.frmFloatHolder = this;

            location3 = labelX12.Location;
            //location4 = labelX13.Location;
        }

        private void frmFloat_Shown(object sender, EventArgs e)
        {
            if (Cursor.Position.X >= this.Location.X && Cursor.Position.X <= this.Location.X + this.Width
                && Cursor.Position.Y >= this.Location.Y && Cursor.Position.Y <= this.Location.Y + this.Height)
                IsMouseIn = true;
            else
                IsMouseIn = false;
        }

        private void frmFloat_MouseEnter(object sender, EventArgs e)
        {
            IsMouseIn = true;
        }

        private void frmFloat_MouseLeave(object sender, EventArgs e)
        {
            IsMouseIn = false;
        }

        public bool HideForm()
        {
            if (!(IsMouseIn || IsLocked))
            {
                this.Hide();
                return true;
            }
            else
            {
                return false;
            }
        }

        public void MoveTo(Point p)
        {
            if (!IsLocked)
            {
                this.Location = p;
            }
        }
        
        private void labelX8_Click(object sender, EventArgs e)
        {
            int index = 0;
            try
            {
                string name = ((DevComponents.DotNetBar.LabelX)sender).Name;
                index = int.Parse(name.Substring(name.Length - 1, 1)) - 1;
                currentindex = index;
            }
            catch
            {
                return;
            }
            if (index < cards.Length)
                CurrentCard = cards[index];
            else
                CurrentCard = null;
        }

        private void labelX9_Click(object sender, EventArgs e)
        {
            if (CurrentCard != null)
                Clipboard.SetText(CurrentCard.name);
        }

        private void labelX11_Click(object sender, EventArgs e)
        {
            if (IsLocked)
            {
                IsLocked = false;
                labelX11.Text = "锁定";
            }
            else
            {
                IsLocked = true;
                labelX11.Text = "解除";
            }
        }

        private void labelX10_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(panelEx1.Text);
        }

        private void labelX12_Click(object sender, EventArgs e)
        {
            if (CurrentCard == null)
                return;

            if (labelX12.Text == "查看调整")
            {
                panelEx1.Text = CurrentCard.adjust;
                labelX10.Text = "复制调整";
                labelX12.Text = "查看说明";
                HideColoredCardName();
            }
            else
            {
                panelEx1.Text = CurrentCard.GetAllInfo();
                labelX10.Text = "复制说明";
                labelX12.Text = "查看调整";
                ShowColoredCardName();
            }
        }

        private void frmFloat_MouseDown(object sender, MouseEventArgs e)
        {
            mdcursorpoint = Cursor.Position;
            mdformpoint = this.Location;
        }

        private void frmFloat_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.None)
            {
                this.Location = new Point(mdformpoint.X + Cursor.Position.X - mdcursorpoint.X, mdformpoint.Y + Cursor.Position.Y - mdcursorpoint.Y);
            }
        }

        private void labelX16_Click(object sender, EventArgs e)
        {
            this.Hide();
            if (Global.frmDictHolder.Visible == false || Global.frmDictHolder.WindowState == FormWindowState.Minimized)
                Global.frmDictHolder.RestoreWindow();
            Global.frmDictHolder.DoSearch(CurrentCard.name);
            Global.frmDictHolder.ShowCardInfo(CurrentCard);
        }
    }
}
