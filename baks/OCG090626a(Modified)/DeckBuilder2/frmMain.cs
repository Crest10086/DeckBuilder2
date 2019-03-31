using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Resources;
using BaseCardLibrary;
using BaseCardLibrary.Search;
using BaseCardLibrary.DataAccess;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Tools;
using DevComponents.DotNetBar;

namespace DeckBuilder2
{
    public partial class frmMain : Form
    {
        int CurrentCardID = 0;
        Hashtable FormByTab = new Hashtable();
        Hashtable TabByForm = new Hashtable();
        DevComponents.DotNetBar.TabItem CurrentClickTab = null;
        bool IsMaximized = false;

        public frmMain()
        {
            InitializeComponent();
        }

        private void buttonItem16_Click(object sender, EventArgs e)
        {
            Form mdifrom = new frmCardView();
            mdifrom.MdiParent = this;
            //mdifrom.Dock = DockStyle.Fill;
            mdifrom.WindowState = FormWindowState.Maximized;
            mdifrom.Show();
        }

        public void ShowCard(int id)
        {
            CardLibrary cardLibrary = CardLibrary.GetInstance();
            CardDescription card = cardLibrary.GetCardByID(id);
            richTextBox1.Text = "\r\n" + card.GetAllInfo();
            if (card.limit < 3 || card.cardCamp != CardCamp.BothOT)
            {
                int start = 1;
                int end = richTextBox1.Text.IndexOf("中文名");
                richTextBox1.Select(start, end - start);

                switch (card.limit)
                {
                    case 0:
                        richTextBox1.SelectionColor = Color.Red;
                        break;
                    case 1:
                        richTextBox1.SelectionColor = Color.Blue;
                        break;
                    case 2:
                        richTextBox1.SelectionColor = Color.LimeGreen;
                        break;
                    case -4:
                        richTextBox1.SelectionColor = Color.Fuchsia;
                        break;
                }

                if (card.cardCamp != CardCamp.BothOT)
                    richTextBox1.SelectionColor = Color.OrangeRed;
            }
            API.SendMessage(richTextBox1.Handle, APIConst.WM_VSCROLL, APIConst.SB_TOP, IntPtr.Zero);

            if (string.Equals(card.adjust, ""))
            {
                dockContainerItem4.Text = "调整（无）";
                richTextBox2.Text = string.Format("[{0}]<{1}>", card.name, card.japName);
            }
            else
            {
                dockContainerItem4.Text = "调整（有）";
                richTextBox2.Text = card.adjust;
                API.SendMessage(richTextBox2.Handle, APIConst.WM_VSCROLL, APIConst.SB_TOP, IntPtr.Zero);
            }
            try
            {
                if (card.cardCamp == CardCamp.DIY)
                    pictureBox1.Load(DB2Config.GetInstance().GetSetting("DIYImagePath") + card.name + ".jpg");
                else
                    pictureBox1.Load(DB2Config.GetInstance().GetSetting("ImagePath") + id.ToString() + ".jpg");
            }
            catch
            {
                pictureBox1.Image = pictureBox2.Image;
            }
        }

        public void ShowCardsCount(int[] counts)
        {
            labelX1.Text = string.Format("结果总数：{10,-4}  通常怪：{0,-4}  效果怪：{1,-4}  仪式怪：{2,-4}  额外怪：{3,-4}  1-4星：{4,-4}  5-6星：{5,-4}  7星以上：{6,-4}  怪兽：{9,-4}  魔法：{7,-4}  陷阱：{8,-4}", counts[0], counts[1], counts[2], counts[3], counts[4], counts[5], counts[6], counts[7], counts[8], counts[4] + counts[5] + counts[6], counts[9]);
        }


        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            Form mdifrom = new frmCardView();
            mdifrom.MdiParent = this;
            mdifrom.WindowState = FormWindowState.Maximized;
            mdifrom.Show();
        }

        private void CancelVirtualMode(Form form)
        {
            if (form == null)
                return;
            if (form.GetType().ToString().Equals("DeckBuilder2.frmCardView"))
            {
                if (!string.Equals(DB2Config.GetInstance().GetSetting("NoVirtualMode"), "True", StringComparison.OrdinalIgnoreCase))
                    return;

                frmCardView f = (frmCardView)form;
                VirtualListView listView1 = f.listViewEx1;
                if (!listView1.VirtualMode)
                    return;

                int index = -1;
                 
                if (listView1.SelectedIndices != null && listView1.SelectedIndices.Count > 0)
                    index = listView1.SelectedIndices[0];

                listView1.BeginUpdate();
                View view = listView1.View;
                listView1.View = View.List;
                listView1.VirtualListSize = 0;
                listView1.VirtualMode = false;
                listView1.View = view;
                f.ReSearch();
                if (index >= 0)
                {
                    listView1.EnsureVisible(index);
                    listView1.Items[index].Selected = true;
                }
                listView1.EndUpdate();
            }
            else if (form.GetType().ToString().Equals("DeckBuilder2.frmDeckView"))
            {
                frmDeckView f = (frmDeckView)form;
                if (!f.listViewEx1.VirtualMode)
                    return;

                VirtualListView[] listViews = new VirtualListView[] { f.listViewEx1, f.listViewEx2, f.listViewEx3 };

                foreach (VirtualListView listView1 in listViews)
                {
                    listView1.BeginUpdate();
                    View view = listView1.View;
                    listView1.View = View.List;
                    listView1.VirtualListSize = 0;
                    listView1.VirtualMode = false;
                    listView1.View = view;
                }

                f.ShowDeck();
            }
            else
            {
                frmDeckEdit f = (frmDeckEdit)form;
                if (!f.listViewEx1.VirtualMode)
                    return;

                VirtualListView[] listViews = new VirtualListView[] { f.listViewEx1, f.listViewEx2, f.listViewEx3 };

                foreach (VirtualListView listView1 in listViews)
                {
                    listView1.BeginUpdate();
                    View view = listView1.View;
                    listView1.View = View.List;
                    listView1.VirtualListSize = 0;
                    listView1.VirtualMode = false;
                    listView1.View = view;
                    listView1.EndUpdate();
                }

                f.ShowDeck();
            }
        }

        public void LoadPicEnd()
        {
            Global.ReDrawTime = 30;

            Global.loadPicEnd = true;
            CancelVirtualMode(this.ActiveMdiChild);
            foreach (Form form in this.MdiChildren)
            {
                CancelVirtualMode(form);
            }
        }

        private void RefreshListView(ListView listview)
        {
            if (listview != null)
                listview.Refresh();
        }

        public void PicsLoaded()
        {
            //Global.fr

            Form form = this.ActiveMdiChild;
            switch (form.GetType().ToString())
            {
                case "DeckBuilder2.frmCardView":
                    RefreshListView(((frmCardView)form).listViewEx1);
                    break;
                case "DeckBuilder2.frmDeckEdit":
                    RefreshListView(((frmDeckEdit)form).listViewEx1);
                    RefreshListView(((frmDeckEdit)form).listViewEx2);
                    RefreshListView(((frmDeckEdit)form).listViewEx3);
                    break;
                case "DeckBuilder2.frmDeckView":
                    RefreshListView(((frmDeckView)form).listViewEx1);
                    RefreshListView(((frmDeckView)form).listViewEx2);
                    RefreshListView(((frmDeckView)form).listViewEx3);
                    break;
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            this.Text = "游戏王GAME图形组卡器2 v" + Global.Version;
            richTextBox1.Text = string.Format(@"
游戏王GAME图形组卡器2
v{0} build {1}

作者：AI_Player
Email：ai.player@gmail.com
QQ群：86204100
版权：中国OCG工作组
网址：http://www.ocgsoft.cn

程序测试：{2}

感谢：青眼小白、虫虫、逆卷炎灵、DK", Global.Version, Global.Build, Global.ProgramDebug);

            DB2Config config = DB2Config.GetInstance();
            CLConfig clconfig = CLConfig.GetInstance();

            Global.ReDrawTime = Tools.Config.GetIntValue(DB2Config.GetInstance().GetSetting("ReDrawTime"), 250); 

            SaveLayout("DefaultMainLayout.xml");
            if (string.Equals(config.GetSetting("SaveLayout"), "True", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    this.Width = int.Parse(config.GetSetting("Width"));
                    this.Height = int.Parse(config.GetSetting("Height"));
                }
                catch
                {
                }
                try
                {
                    int x = int.Parse(config.GetSetting("X"));
                    int y = int.Parse(config.GetSetting("Y"));
                    this.Location = new Point(x, y);
                }
                catch
                {
                }
                if (string.Equals(config.GetSetting("isMax"), "true", StringComparison.OrdinalIgnoreCase))
                    this.WindowState = FormWindowState.Maximized;
                LoadLayout("MainLayout.xml");
            }

            if (string.Equals(clconfig.GetSetting("AllowDIY"), "true", StringComparison.OrdinalIgnoreCase))
                CardLibrary.AllowDIY = true;
            else
                CardLibrary.AllowDIY = false;

            if (string.Equals(config.GetSetting("CloseButtonOnTab"), "true", StringComparison.OrdinalIgnoreCase))
                tabControl1.CloseButtonOnTabsVisible = true;
            else
                tabControl1.CloseButtonOnTabsVisible = false;

            tabControl1.Tabs.Clear();
            tabControl1.TabStrip.MouseDown += new MouseEventHandler(tabControl1_MouseDown);
            tabControl1.TabStrip.DoubleClick += new EventHandler(tabControl1_DouClick);
            this.contextMenuBar1.SetContextMenuEx(this.tabControl1, this.buttonItem16);
            Global.frmMainHolder = this;

            //Form frmdict = new CardXDict.frmDict();
            //frmdict.Show();
        }

        private void AdvSearch()
        {
            try
            {
                BooleanQuery query = new BooleanQuery();

                if (!textBoxX1.Text.Equals(""))
                {
                    BooleanQuery query2 = new BooleanQuery();

                    QueryParser parser = new QueryParser("中文名", AnalyzerFactory.GetAnalyzer());
                    Query q = parser.Parse(textBoxX1.Text);
                    query2.Add(q, BooleanClause.Occur.SHOULD);

                    parser = new QueryParser("旧卡名", AnalyzerFactory.GetAnalyzer());
                    q = parser.Parse(textBoxX1.Text);
                    query2.Add(q, BooleanClause.Occur.SHOULD);

                    parser = new QueryParser("简称", AnalyzerFactory.GetAnalyzer());
                    q = parser.Parse(textBoxX1.Text);
                    query2.Add(q, BooleanClause.Occur.SHOULD);

                    query.Add(query2, BooleanClause.Occur.MUST);
                }

                if (!textBoxX2.Text.Equals(""))
                {
                    QueryParser parser = new QueryParser("日文名", AnalyzerFactory.GetAnalyzer());
                    Query q = parser.Parse(textBoxX2.Text);
                    query.Add(q, BooleanClause.Occur.MUST);
                }

                if (!textBoxX3.Text.Equals(""))
                {
                    QueryParser parser = new QueryParser("效果", AnalyzerFactory.GetAnalyzer());
                    Query q = parser.Parse(textBoxX3.Text);
                    query.Add(q, BooleanClause.Occur.MUST);
                }

                if (!comboBoxEx1.Text.Equals(""))
                {
                    QueryParser parser = new QueryParser("种族", AnalyzerFactory.GetAnalyzer());
                    Query q = parser.Parse(comboBoxEx1.Text);
                    query.Add(q, BooleanClause.Occur.MUST);
                }

                if (!comboBoxEx2.Text.Equals(""))
                {
                    QueryParser parser = new QueryParser("属性", AnalyzerFactory.GetAnalyzer());
                    Query q = parser.Parse(comboBoxEx2.Text);
                    query.Add(q, BooleanClause.Occur.MUST);
                }

                if (!comboBoxEx3.Text.Equals(""))
                {
                    QueryParser parser = new QueryParser("卡种", AnalyzerFactory.GetAnalyzer());
                    Query q = parser.Parse(comboBoxEx3.Text);
                    query.Add(q, BooleanClause.Occur.MUST);
                }

                if (comboBoxEx4.SelectedItem != null && comboBoxEx4.SelectedIndex != 0)
                {
                    string limit = (comboBoxEx4.SelectedIndex - 1).ToString();
                    QueryParser parser = new QueryParser("禁限数", AnalyzerFactory.GetAnalyzer());
                    Query q = parser.Parse(limit);
                    query.Add(q, BooleanClause.Occur.MUST);
                }

                if (!comboBoxEx6.Text.Equals(""))
                {
                    QueryParser parser = new QueryParser("卡包", AnalyzerFactory.GetAnalyzer());
                    Query q = parser.Parse(comboBoxEx6.Text);
                    query.Add(q, BooleanClause.Occur.MUST);
                }

                if (!comboBoxEx5.Text.Equals(""))
                {
                    QueryParser parser = new QueryParser("罕见度", AnalyzerFactory.GetAnalyzer());
                    Query q = parser.Parse(comboBoxEx5.Text);
                    query.Add(q, BooleanClause.Occur.MUST);
                }

                if (!(textBoxX4.Text.Equals("") && textBoxX5.Text.Equals("")))
                {
                    string s = null;
                    if (textBoxX4.Text.Equals(""))
                        s = string.Format("[0000 TO {0:D4}]", int.Parse(textBoxX5.Text));
                    else if (textBoxX5.Text.Equals(""))
                        s = string.Format("[{0:D4} TO 9999]", int.Parse(textBoxX4.Text));
                    else
                        s = string.Format("[{0:D4} TO {1:D4}]", int.Parse(textBoxX4.Text), int.Parse(textBoxX5.Text));
                    QueryParser parser = new QueryParser("攻击", AnalyzerFactory.GetAnalyzer());
                    Query q = parser.Parse(s);
                    query.Add(q, BooleanClause.Occur.MUST);
                }

                if (!(textBoxX6.Text.Equals("") && textBoxX7.Text.Equals("")))
                {
                    string s = null;
                    if (textBoxX6.Text.Equals(""))
                        s = string.Format("[0000 TO {0:D4}]", int.Parse(textBoxX7.Text));
                    else if (textBoxX7.Text.Equals(""))
                        s = string.Format("[{0:D4} TO 9999]", int.Parse(textBoxX6.Text));
                    else
                        s = string.Format("[{0:D4} TO {1:D4}]", int.Parse(textBoxX6.Text), int.Parse(textBoxX7.Text));
                    QueryParser parser = new QueryParser("防御", AnalyzerFactory.GetAnalyzer());
                    Query q = parser.Parse(s);
                    query.Add(q, BooleanClause.Occur.MUST);
                }

                if (!(textBoxX8.Text.Equals("") && textBoxX9.Text.Equals("")))
                {
                    string s = null;
                    if (textBoxX8.Text.Equals(""))
                        s = string.Format("[00 TO {0:D2}]", int.Parse(textBoxX9.Text));
                    else if (textBoxX9.Text.Equals(""))
                        s = string.Format("[{0:D2} TO 12]", int.Parse(textBoxX8.Text));
                    else
                        s = string.Format("[{0:D2} TO {1:D2}]", int.Parse(textBoxX8.Text), int.Parse(textBoxX9.Text));
                    QueryParser parser = new QueryParser("星数", AnalyzerFactory.GetAnalyzer());
                    Query q = parser.Parse(s);
                    query.Add(q, BooleanClause.Occur.MUST);
                }

                frmCardView mdifrom = new frmCardView();
                mdifrom.MdiParent = this;
                mdifrom.WindowState = FormWindowState.Maximized;
                mdifrom.textBoxX1.Text = query.ToString();
                mdifrom.Show();
                mdifrom.DoSearch();
            }
            catch
            {
                MessageBox.Show("错误的语法格式！");
            }
        }

        private void buttonX1_Click(object sender, EventArgs e)
        {
            AdvSearch();
        }

        private void textBoxX1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                AdvSearch();
            }
        }

        private void buttonX2_Click(object sender, EventArgs e)
        {
            textBoxX1.Text = "";
            textBoxX2.Text = "";
            textBoxX3.Text = "";
            textBoxX4.Text = "";
            textBoxX5.Text = "";
            textBoxX6.Text = "";
            textBoxX7.Text = "";
            textBoxX8.Text = "";
            textBoxX9.Text = "";
            comboBoxEx1.Text = "";
            comboBoxEx2.Text = "";
            comboBoxEx3.Text = "";
            comboBoxEx5.Text = "";
            comboBoxEx6.Text = "";
            comboBoxEx4.SelectedItem = null;

        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Form mdifrom = new frmDeckView();
            mdifrom.MdiParent = this;
            mdifrom.WindowState = FormWindowState.Maximized;
            mdifrom.Show();
        }

        private void buttonItem3_Click(object sender, EventArgs e)
        {
            richTextBox1.Focus();
            richTextBox1.SelectAll();
        }

        private void buttonItem4_Click(object sender, EventArgs e)
        {
            richTextBox1.Copy();
        }

        private void buttonItem5_Click(object sender, EventArgs e)
        {
            if (!richTextBox1.SelectedText.Equals(""))
            {
                frmCardView mdifrom = new frmCardView();
                mdifrom.MdiParent = this;
                mdifrom.textBoxX1.Text = richTextBox1.SelectedText;
                mdifrom.WindowState = FormWindowState.Maximized;
                mdifrom.Show();
                mdifrom.DoSearch();
            }
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            if (this.ActiveMdiChild != null)
                this.ActiveMdiChild.Close();
        }

        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            foreach (Form form in this.MdiChildren)
            {
                form.Close();
            }
        }

        private void toolStripMenuItem9_Click(object sender, EventArgs e)
        {
            this.LayoutMdi(MdiLayout.Cascade);
        }

        private void toolStripMenuItem10_Click(object sender, EventArgs e)
        {
            this.LayoutMdi(MdiLayout.TileHorizontal);
        }

        private void toolStripMenuItem11_Click(object sender, EventArgs e)
        {
            this.LayoutMdi(MdiLayout.TileVertical);
        }

        private void toolStripMenuItem12_Click(object sender, EventArgs e)
        {
            this.LayoutMdi(MdiLayout.ArrangeIcons);
        }

        private void toolStripMenuItem13_Click(object sender, EventArgs e)
        {
            foreach (Form form in this.MdiChildren)
            {
                form.WindowState = FormWindowState.Minimized;
            }
        }

        private void toolStripMenuItem14_Click(object sender, EventArgs e)
        {
            foreach (Form form in this.MdiChildren)
            {
                form.WindowState = FormWindowState.Maximized;
            }
        }

        private void toolStripMenuItem15_Click(object sender, EventArgs e)
        {
            foreach (Form form in this.MdiChildren)
            {
                form.WindowState = FormWindowState.Normal;
            }
        }

        private void buttonItem6_Click(object sender, EventArgs e)
        {
            richTextBox2.Focus();
            richTextBox2.SelectAll();
        }

        private void buttonItem7_Click(object sender, EventArgs e)
        {
            richTextBox2.Copy();
        }

        private void buttonItem8_Click(object sender, EventArgs e)
        {
            if (!richTextBox2.SelectedText.Equals(""))
            {
                frmCardView mdifrom = new frmCardView();
                mdifrom.MdiParent = this;
                mdifrom.textBoxX1.Text = richTextBox2.SelectedText;
                mdifrom.WindowState = FormWindowState.Maximized;
                mdifrom.Show();
                mdifrom.DoSearch();
            }
        }

        private void toolStripMenuItem16_Click(object sender, EventArgs e)
        {
            Global.frmDeckEditHolder.Activate();
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!Global.appCloing)
            {
                DB2Config config = DB2Config.GetInstance();

                if (Global.frmDeckEditHolder.isChange() && !Global.frmDeckEditHolder.isEmpty())
                {
                    switch (MessageBox.Show("卡组尚未保存，是否在退出前保存？", "", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
                    {
                        case DialogResult.Yes:
                            if (Global.frmDeckEditHolder.SaveDeck())
                                Global.appCloing = true;
                            else
                                e.Cancel = true;
                            break;
                        case DialogResult.No:
                            Global.appCloing = true;
                            break;
                        case DialogResult.Cancel:
                            e.Cancel = true;
                            break;
                    }

                    if (Global.appCloing)
                    {
                        if (string.Equals(config.GetSetting("SaveLayout"), "True", StringComparison.OrdinalIgnoreCase))
                        {
                            SaveLayout("MainLayout.xml");
                            config.SetSetting("isMax", IsMaximized.ToString());
                            this.WindowState = FormWindowState.Normal;
                            config.SetSetting("X", this.Location.X.ToString());
                            config.SetSetting("Y", this.Location.Y.ToString());
                            config.SetSetting("Width", this.Width.ToString());
                            config.SetSetting("Height", this.Height.ToString());
                        }
                        Global.frmDeckEditHolder.Close();
                        this.Close();
                    }
                }
                else
                {
                    Global.appCloing = true;
                    if (string.Equals(config.GetSetting("SaveLayout"), "True", StringComparison.OrdinalIgnoreCase))
                    {
                        config.SetSetting("isMax", IsMaximized.ToString());
                        SaveLayout("MainLayout.xml");
                        this.WindowState = FormWindowState.Normal;
                        config.SetSetting("X", this.Location.X.ToString());
                        config.SetSetting("Y", this.Location.Y.ToString());
                        config.SetSetting("Width", this.Width.ToString());
                        config.SetSetting("Height", this.Height.ToString());
                    }
                    Global.frmDeckEditHolder.Close();
                    this.Close();
                }
            }
        }

        private void buttonX4_Click(object sender, EventArgs e)
        {
            if (Global.frmDeckEditHolder.isChange())
            {
                if (MessageBox.Show("卡组尚未保存，是否读入新卡组？", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }
            }
            Global.frmDeckEditHolder.LoadDeck();
        }

        private void buttonItem9_PopupOpen(object sender, DevComponents.DotNetBar.PopupOpenEventArgs e)
        {
            if (virtualListView1.SelectedIndices == null || virtualListView1.SelectedIndices.Count == 0)
            {
                buttonItem10.Enabled = false;
                buttonItem11.Enabled = false;
                buttonItem12.Enabled = false;
                buttonItem13.Enabled = false;
                buttonItem14.Enabled = false;
                buttonItem15.Enabled = false;
                return;
            }

            ListViewItem item = virtualListView1.SelectedItems[0];
            if (item.Group == virtualListView1.Groups[0])
            {
                buttonItem10.Enabled = false;
            }
            else if (item.Group == virtualListView1.Groups[1])
            {
                CardDescription card = CardLibrary.GetInstance().GetCardByName(virtualListView1.SelectedItems[0].Text);
                if (card != null && (card.iCardtype == 2 || card.iCardtype == 6))
                    buttonItem10.Text = "至额外卡组";
                buttonItem11.Enabled = false;
            }
            else
            {
                buttonItem10.Enabled = false;
            }
        }

        private void buttonItem9_PopupFinalized(object sender, EventArgs e)
        {
            buttonItem10.Text = "至主卡组";
            buttonItem10.Enabled = true;
            buttonItem11.Enabled = true;
            buttonItem12.Enabled = true;
            buttonItem13.Enabled = true;
            buttonItem14.Enabled = true;
            buttonItem15.Enabled = true;
        }

        private void ShowCurrentCard()
        {
            if (CurrentCardID > 0)
            {
                this.ShowCard(CurrentCardID);
            }
        }

        private void virtualListView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (virtualListView1.SelectedIndices != null)
                if (virtualListView1.SelectedIndices.Count == 1)
                {
                    CurrentCardID = CardLibrary.GetInstance().GetCardByName(virtualListView1.SelectedItems[0].Text).ID;
                }
            ShowCurrentCard();
        }

        private void buttonItem10_Click(object sender, EventArgs e)
        {
            if (virtualListView1.SelectedIndices == null || virtualListView1.SelectedIndices.Count == 0)
            {
                return;
            }
            ListViewItem item = virtualListView1.SelectedItems[0];
            if (item.Group == virtualListView1.Groups[1])
            {
                CardDescription card = CardLibrary.GetInstance().GetCardByName(virtualListView1.SelectedItems[0].Text);
                int id = card.ID;
                Global.frmDeckEditHolder.RemoveSideCard(id);
                if (card.iCardtype == 2 || card.iCardtype == 6)
                    Global.frmDeckEditHolder.AddToFusionDeck(id);
                else
                    Global.frmDeckEditHolder.AddToMainDeck(id);
            }
            Global.frmDeckEditHolder.ShowDeck();
        }

        private void buttonItem11_Click(object sender, EventArgs e)
        {
            if (virtualListView1.SelectedIndices == null || virtualListView1.SelectedIndices.Count == 0)
            {
                return;
            }
            ListViewItem item = virtualListView1.SelectedItems[0];
            if (item.Group == virtualListView1.Groups[0])
            {
                int id = CardLibrary.GetInstance().GetCardByName(virtualListView1.SelectedItems[0].Text).ID;
                Global.frmDeckEditHolder.RemoveMainCard(id);
                Global.frmDeckEditHolder.AddToSideDeck(id);
            }
            else if (item.Group == virtualListView1.Groups[2])
            {
                int id = CardLibrary.GetInstance().GetCardByName(virtualListView1.SelectedItems[0].Text).ID;
                Global.frmDeckEditHolder.RemoveFusionCard(id);
                Global.frmDeckEditHolder.AddToSideDeck(id);
            }
            Global.frmDeckEditHolder.ShowDeck();
        }

        private void buttonItem12_Click(object sender, EventArgs e)
        {
            if (virtualListView1.SelectedIndices == null || virtualListView1.SelectedIndices.Count == 0)
            {
                return;
            }
            ListViewItem item = virtualListView1.SelectedItems[0];
            int id = CardLibrary.GetInstance().GetCardByName(virtualListView1.SelectedItems[0].Text).ID;
            int count = 0;
            if (item.Group == virtualListView1.Groups[0])
            {
                Global.frmDeckEditHolder.AddMainCard(id);
                count = Global.frmDeckEditHolder.GetCurrentDeck().MainDeck.GetCount(id);
            }
            else if (item.Group == virtualListView1.Groups[1])
            {
                Global.frmDeckEditHolder.AddSideCard(id);
                count = Global.frmDeckEditHolder.GetCurrentDeck().SideDeck.GetCount(id);
            }
            else
            {
                Global.frmDeckEditHolder.AddFusionCard(id);
                count = Global.frmDeckEditHolder.GetCurrentDeck().FusionDeck.GetCount(id);
            }
            item.SubItems[1].Text = count.ToString();
            Global.frmDeckEditHolder.ShowQuickViewCount();
            Global.frmDeckEditHolder.CountCards();
        }

        private void buttonItem13_Click(object sender, EventArgs e)
        {
            if (virtualListView1.SelectedIndices == null || virtualListView1.SelectedIndices.Count == 0)
            {
                return;
            }
            ListViewItem item = virtualListView1.SelectedItems[0];
            int id = CardLibrary.GetInstance().GetCardByName(virtualListView1.SelectedItems[0].Text).ID;
            if (item.Group == virtualListView1.Groups[0])
            {
                Global.frmDeckEditHolder.RemoveMainCard(id);
            }
            else if (item.Group == virtualListView1.Groups[1])
            {
                Global.frmDeckEditHolder.RemoveSideCard(id);
            }
            else
            {
                Global.frmDeckEditHolder.RemoveFusionCard(id);
            }
            int count = int.Parse(item.SubItems[1].Text) - 1;
            if (count == 0)
            {
                virtualListView1.Items.Remove(item);
            }
            else
            {
                item.SubItems[1].Text = count.ToString();
            }
            Global.frmDeckEditHolder.ShowQuickViewCount();
            Global.frmDeckEditHolder.CountCards();
        }

        private void buttonItem14_Click(object sender, EventArgs e)
        {
            if (virtualListView1.SelectedIndices == null || virtualListView1.SelectedIndices.Count == 0)
            {
                return;
            }
            ListViewItem item = virtualListView1.SelectedItems[0];
            int id = CardLibrary.GetInstance().GetCardByName(virtualListView1.SelectedItems[0].Text).ID;
            int count = int.Parse(virtualListView1.SelectedItems[0].SubItems[1].Text);
            if (item.Group == virtualListView1.Groups[0])
            {
                for (int i=0; i<count; i++)
                    Global.frmDeckEditHolder.RemoveMainCard(id);
            }
            else if (item.Group == virtualListView1.Groups[1])
            {
                for (int i = 0; i < count; i++)
                    Global.frmDeckEditHolder.RemoveSideCard(id);
            }
            else
            {
                for (int i = 0; i < count; i++)
                    Global.frmDeckEditHolder.RemoveFusionCard(id);
            }
            virtualListView1.Items.Remove(item);
            Global.frmDeckEditHolder.ShowQuickViewCount();
            Global.frmDeckEditHolder.CountCards();
        }

        private void buttonX3_Click(object sender, EventArgs e)
        {
            Global.frmDeckEditHolder.SaveDeck();
        }

        private void frmMain_Shown(object sender, EventArgs e)
        {
            Global.largePicLoader = new PicLoader(CardLibrary.GetInstance().GetCount(), DB2Config.GetInstance().GetSetting("IcoPath"), DB2Config.GetInstance().GetSetting("IcoPath"));
            frmDeckEdit mdifrom = new frmDeckEdit();
            mdifrom.MdiParent = this;
            mdifrom.WindowState = FormWindowState.Maximized;
            mdifrom.Show();
            Global.frmDeckEditHolder = mdifrom;
            frmCardView mdifrom2 = new frmCardView();
            mdifrom2.MdiParent = this;
            mdifrom2.WindowState = FormWindowState.Maximized;
            mdifrom2.Show();
            mdifrom2.DoSearch();
        }

        private void virtualListView1_Enter(object sender, EventArgs e)
        {
            Global.frmDeckEditHolder.CountCards();
            if (virtualListView1.SelectedIndices != null)
                if (virtualListView1.SelectedIndices.Count == 1)
                {
                    CurrentCardID = CardLibrary.GetInstance().GetCardByName(virtualListView1.SelectedItems[0].Text).ID;
                }
            ShowCurrentCard();
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Form mdifrom = new frmConfig();
            mdifrom.Location = new Point(this.Location.X + (this.Size.Width - mdifrom.Size.Width) / 2, this.Location.Y + (this.Size.Height - mdifrom.Size.Height) / 2);
            mdifrom.ShowDialog();
        }

        private void buttonX5_Click(object sender, EventArgs e)
        {
            Global.frmDeckEditHolder.ClearDeck();
        }

        private void toolStripMenuItem17_Click(object sender, EventArgs e)
        {
            string nbxdir = DB2Config.GetInstance().GetSetting("NBXPath");
            if (System.IO.File.Exists(nbxdir))
            {
                try
                {
                    System.Diagnostics.Process.Start(nbxdir);
                }
                catch
                {
                }
            }
            else
            {
                MessageBox.Show("找不到NBX可执行文件，请检查设置中的NBX路径是否正确！");
            }
        }

        private void richTextBox1_DoubleClick(object sender, EventArgs e)
        {
            richTextBox1.SelectAll();
        }

        private void richTextBox1_SelectionChanged(object sender, EventArgs e)
        {
            if (richTextBox1.SelectionStart == 0)
                richTextBox1.SelectionStart++;
        }

        private void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            if (e.LinkText == null)
                return;

            if (e.LinkText.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                System.Diagnostics.Process.Start(e.LinkText);
        }

        private void toolStripMenuItem19_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.ocgsoft.cn");
        }

        private void toolStripMenuItem20_Click(object sender, EventArgs e)
        {
            Form mdifrom = new frmAbout();
            mdifrom.Location = new Point(this.Location.X + (this.Size.Width - mdifrom.Size.Width) / 2, this.Location.Y + (this.Size.Height - mdifrom.Size.Height) / 2);
            mdifrom.ShowDialog();
        }

        private void buttonItem1_PopupShowing(object sender, EventArgs e)
        {
            richTextBox1.Focus();
            if (richTextBox1.SelectionLength > 0)
            {
                buttonItem4.Enabled = true;
                buttonItem5.Enabled = true;
            }
            else
            {
                buttonItem4.Enabled = false;
                buttonItem5.Enabled = false;
            }
        }

        private void richTextBox1_Enter(object sender, EventArgs e)
        {
            buttonItem3.Enabled = true;
            buttonItem4.Enabled = true;
        }

        private void richTextBox1_Leave(object sender, EventArgs e)
        {
            buttonItem3.Enabled = false;
            buttonItem4.Enabled = false;
        }

        private void buttonItem2_PopupOpen(object sender, DevComponents.DotNetBar.PopupOpenEventArgs e)
        {
            richTextBox2.Focus();
            if (richTextBox2.SelectionLength > 0)
            {
                buttonItem6.Enabled = true;
                buttonItem7.Enabled = true;
            }
            else
            {
                buttonItem6.Enabled = false;
                buttonItem7.Enabled = false;
            }
        }

        private void richTextBox2_Enter(object sender, EventArgs e)
        {
            buttonItem6.Enabled = true;
            buttonItem7.Enabled = true;
        }

        private void richTextBox2_Leave(object sender, EventArgs e)
        {
            buttonItem6.Enabled = false;
            buttonItem7.Enabled = false;
        }

        private void buttonItem15_Click(object sender, EventArgs e)
        {
            if (virtualListView1.SelectedIndices == null || virtualListView1.SelectedIndices.Count == 0)
            {
                return;
            }
            ListViewItem item = virtualListView1.SelectedItems[0];
            int id = CardLibrary.GetInstance().GetCardByName(virtualListView1.SelectedItems[0].Text).ID;
            Global.frmDeckEditHolder.AddToTempDeck(id);
            Global.frmDeckEditHolder.ShowDeck();
        }

        public void MDIChildAdded(Form mdichild)
        {
            DevComponents.DotNetBar.TabItem tab = new DevComponents.DotNetBar.TabItem();
            tab.Text = mdichild.Text;
            tab.Icon = mdichild.Icon;
            //tab.MouseDown += new MouseEventHandler(this.Tab_MouseDown);
            FormByTab[tab] = mdichild;
            TabByForm[mdichild] = tab;
            tabControl1.Tabs.Add(tab);
            CurrentTabChanged();
        }

        public void MDIChildTextChanged(Form mdichild)
        {
            DevComponents.DotNetBar.TabItem tab = (DevComponents.DotNetBar.TabItem)TabByForm[mdichild];
            if (tab != null)
            {
                tab.Text = mdichild.Text;
            }
        }

        private void tabControl1_SelectedTabChanged(object sender, DevComponents.DotNetBar.TabStripTabChangedEventArgs e)
        {
            if (tabControl1.SelectedTab == null)
                return;

            Form form = (Form)FormByTab[tabControl1.SelectedTab];
            if (form != null && this.ActiveMdiChild != form)
            {
                CurrentTabChanged();
                form.Activate();
            }
        }

        private void frmMain_MdiChildActivate(object sender, EventArgs e)
        {
            if (this.ActiveMdiChild == null)
                return;

            DevComponents.DotNetBar.TabItem tab = (DevComponents.DotNetBar.TabItem)TabByForm[this.ActiveMdiChild];
            if (tab != null)
            {
                tabControl1.SelectedTab = tab;
                CurrentTabChanged();
            }
        }

        private void tabControl1_TabRemoved(object sender, EventArgs e)
        {
            //foreach (Form form in TabByForm.Keys)
            //{
            //    if (TabByForm[form] == null)
            //        form.Close();
            //}
        }

        public void MDIChildClosed(Form mdichild)
        {
            DevComponents.DotNetBar.TabItem tab = (DevComponents.DotNetBar.TabItem)TabByForm[mdichild];
            if (tab != null)
            {
                try
                {
                    tabControl1.Tabs.Remove(tab);
                }
                catch
                {
                }
            }
        }

        private void Tab_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Clicks == 2 && e.Button == MouseButtons.Left)
            {
                CloseFormByTab((DevComponents.DotNetBar.TabItem)sender);
            }
        }

        private void tabControl1_MouseDown(object sender, MouseEventArgs e)
        {
            Point pt = new Point(e.X, e.Y);
            Rectangle recTab = new Rectangle();
            CurrentClickTab = null;
            for (int i = 0; i < tabControl1.Tabs.Count; i++)
            {
                recTab = tabControl1.Tabs[i].DisplayRectangle;
                if (recTab.Contains(pt))
                {
                    CurrentClickTab = tabControl1.Tabs[i];
                    break;
                }
            }
            
            if (e.Clicks == 2 && e.Button == MouseButtons.Left)
            {
                if (CurrentClickTab == null)
                {
                    toolStripMenuItem3_Click(null, null);
                }
                else
                {
                    CloseFormByTab(CurrentClickTab);
                }
            }

            /*
            if (e.Button == MouseButtons.Right)
            {
                if (CurrentClickTab != null)
                {
                    //toolStripMenuItem21.Enabled = true;
                    tabControl1.SelectedTab = CurrentClickTab;
                    Form form = (Form)FormByTab[CurrentClickTab];
                    this.contextMenuBar1.SetContextMenuEx(this.tabControl1, this.buttonItem16);
                    
                    switch (form.GetType().ToString())
                    {
                        case "DeckBuilder2.frmCardView":
                            this.contextMenuBar1.SetContextMenuEx(this.tabControl1, ((frmCardView)form).buttonItem1);
                            break;
                        case "DeckBuilder2.frmDeckEdit":
                            this.contextMenuBar1.SetContextMenuEx(this.tabControl1, ((frmDeckEdit)form).buttonItem2);
                            break;
                        case "DeckBuilder2.frmDeckView":
                            this.contextMenuBar1.SetContextMenuEx(this.tabControl1, ((frmDeckView)form).buttonItem2);
                            break;
                    }
                     
                }
                else
                {
                    this.contextMenuBar1.SetContextMenuEx(this.tabControl1, null);
                    //toolStripMenuItem21.Enabled = false;
                }
            }
            */
        }

        private void tabControl1_DouClick(object sender, EventArgs e)
        {
            
        }

        private void tabControl1_TabItemClose(object sender, DevComponents.DotNetBar.TabStripActionEventArgs e)
        {
            DevComponents.DotNetBar.TabItem tab = tabControl1.SelectedTab;
            if (tab != null)
            {
                Form form = (Form)FormByTab[tab];
                if (form != null)
                {
                    form.Close();
                    if (form != null)
                        e.Cancel = true;
                }
            }
        }

        public void CurrentTabChanged()
        {
            tabControl1.SelectedTab.PredefinedColor = DevComponents.DotNetBar.eTabItemColor.Default;
            foreach (DevComponents.DotNetBar.TabItem tab in tabControl1.Tabs)
                if (tab != tabControl1.SelectedTab && tab.PredefinedColor != DevComponents.DotNetBar.eTabItemColor.Silver)
                    tab.PredefinedColor = DevComponents.DotNetBar.eTabItemColor.Default;
        }

        private void toolStripMenuItem21_Click(object sender, EventArgs e)
        {
            CloseFormByTab(CurrentClickTab);
        }

        private void toolStripMenuItem22_Click(object sender, EventArgs e)
        {
            toolStripMenuItem7_Click(null, null);
        }

        private void CloseFormByTab(DevComponents.DotNetBar.TabItem closetab)
        {
            CloseFormByTab(closetab, true);
        }

        private void CloseFormByTab(DevComponents.DotNetBar.TabItem closetab, Boolean refresh)
        {
            if (closetab != null)
            {
                Form form = (Form)FormByTab[closetab];
                if (form != null)
                {
                    form.Close();
                    if (refresh)
                        tabControl1.Refresh();
                }
            }
        }

        private void buttonItem17_Click(object sender, EventArgs e)
        {
            DevComponents.DotNetBar.TabItem CloseTab = CurrentClickTab;
            if (CloseTab == null)
            {
                CloseTab = tabControl1.SelectedTab;
            }

            CloseFormByTab(CloseTab);
        }

        private void buttonItem19_Click(object sender, EventArgs e)
        {
            DevComponents.DotNetBar.TabItem CloseTab = CurrentClickTab;
            if (CloseTab == null)
            {
                CloseTab = tabControl1.SelectedTab;
            }

            if (CloseTab != null)
            {
                Form form = (Form)FormByTab[CloseTab];
                if (form != null)
                    form.Activate();

                for (int i = tabControl1.Tabs.IndexOf(CloseTab) - 1; i > 0; i--)
                    CloseFormByTab(tabControl1.Tabs[i], false);
                tabControl1.Refresh();
            }
        }

        private void buttonItem20_Click(object sender, EventArgs e)
        {
            DevComponents.DotNetBar.TabItem CloseTab = CurrentClickTab;
            if (CloseTab == null)
            {
                CloseTab = tabControl1.SelectedTab;
            }

            if (CloseTab != null)
            {
                Form form = (Form)FormByTab[CloseTab];
                if (form != null)
                    form.Activate();

                int j = tabControl1.Tabs.IndexOf(CloseTab);
                for (int i = tabControl1.Tabs.Count - 1; i > j; i--)
                    CloseFormByTab(tabControl1.Tabs[i], false);
                tabControl1.Refresh();
            }
        }

        private void buttonItem21_Click(object sender, EventArgs e)
        {
            Global.frmDeckEditHolder.Activate();
            foreach (Form form in this.MdiChildren)
            {
                form.Close();
            }
            //for (int i = tabControl1.Tabs.Count - 1; i >= 0; i--)
            //    CloseFormByTab(tabControl1.Tabs[i], false);
            tabControl1.Refresh();
        }

        private void buttonItem1_PopupFinalized(object sender, EventArgs e)
        {
            buttonItem4.Enabled = true;
            buttonItem5.Enabled = true;
        }

        private void buttonItem2_PopupFinalized(object sender, EventArgs e)
        {
            buttonItem6.Enabled = true;
            buttonItem7.Enabled = true;
        }

        private void virtualListView1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("DeckBuilder2.DragCard"))
            {
                DragCard dragcard = (DragCard)e.Data.GetData("DeckBuilder2.DragCard");
                if (dragcard.RemoveFrom == DeckType.None)
                    e.Effect = DragDropEffects.Copy;
                else
                    e.Effect = DragDropEffects.Move;
            }
        }

        private void virtualListView1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("DeckBuilder2.DragCard"))
            {
                DragCard dragcard = (DragCard)e.Data.GetData("DeckBuilder2.DragCard");
                if (dragcard.FromObject != sender)
                    Global.frmDeckEditHolder.DoDragDrop(dragcard, DeckType.MainDeck);
            }
        }

        private void tabControl1_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("DeckBuilder2.DragCard"))
            {
                DevComponents.DotNetBar.TabItem tab = (DevComponents.DotNetBar.TabItem)TabByForm[Global.frmDeckEditHolder];
                Point p = new Point();
                p = tabControl1.PointToClient(new Point(e.X, e.Y));
                if (tab == null || !tab.DisplayRectangle.Contains(p))
                {
                    e.Effect = DragDropEffects.Move;
                }
                else
                {
                    DragCard dragcard = (DragCard)e.Data.GetData("DeckBuilder2.DragCard");
                    if (dragcard.RemoveFrom == DeckType.None)
                        e.Effect = DragDropEffects.Copy;
                    else
                        e.Effect = DragDropEffects.Move;
                }
            }
        }

        private void tabControl1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("DeckBuilder2.DragCard"))
            {
                DragCard dragcard = (DragCard)e.Data.GetData("DeckBuilder2.DragCard");
                DevComponents.DotNetBar.TabItem tab = (DevComponents.DotNetBar.TabItem)TabByForm[Global.frmDeckEditHolder];
                //int posx = e.X - this.Location.X;
                //if (tab == null || posx < tab.DisplayRectangle.Left || tab.DisplayRectangle.Right < posx)
                Point p = new Point();
                p = tabControl1.PointToClient(new Point(e.X, e.Y));
                if (tab == null || !tab.DisplayRectangle.Contains(p))
                {
                    Global.frmDeckEditHolder.DoDragDrop(dragcard, DeckType.None);
                }
                else
                {
                    Global.frmDeckEditHolder.DoDragDrop(dragcard, DeckType.MainDeck);
                }
            }
        }

        private void virtualListView1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (!string.Equals(DB2Config.GetInstance().GetSetting("NoDrag"), "True", StringComparison.OrdinalIgnoreCase))
            {
                VirtualListView listViewEx1 = virtualListView1;
                ListViewItem item = (ListViewItem)e.Item;
                if (item == null)
                    return;

                if (item.Selected == false)
                {
                    item.Selected = true;
                    listViewEx1.Update();
                }

                DragCard dragcard = new DragCard();
                dragcard.Card = CardLibrary.GetInstance().GetCardByName(item.Text);
                dragcard.FromObject = listViewEx1;
                dragcard.RemoveName = item.Text;
                switch (item.Group.Name)
                {
                    case "listViewGroup1":
                        dragcard.RemoveFrom = DeckType.MainDeck;
                        break;
                    case "listViewGroup2":
                        dragcard.RemoveFrom = DeckType.SideDeck;
                        break;
                    case "listViewGroup3":
                        dragcard.RemoveFrom = DeckType.FusionDeck;
                        break;
                }
                listViewEx1.DoDragDrop(dragcard, DragDropEffects.Move);
            }
        }

        public void SaveLayout(string filename)
        {
            string dir = Global.appPath + "//Layout//";
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);
            try
            {
                dotNetBarManager1.SaveLayout(dir + filename);
            }
            catch
            {
            }
        }

        public void LoadLayout(string filename)
        {
            string file = Global.appPath + "//Layout//" + filename;
            if (System.IO.File.Exists(file))
            {
                try
                {
                    dotNetBarManager1.LoadLayout(file);
                }
                catch
                {
                }
            }
        }

        private void frmMain_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized)
                IsMaximized = true;
            else if (this.WindowState == FormWindowState.Normal)
                IsMaximized = false;
        }
    }
}