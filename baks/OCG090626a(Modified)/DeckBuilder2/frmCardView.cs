using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using BaseCardLibrary.DataAccess;
using BaseCardLibrary.Search;
using Tools;
using Lucene.Net.Search;
using Lucene.Net.Documents;
using Lucene.Net.QueryParsers;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;

namespace DeckBuilder2
{
    public struct MenuInfo
    {
        public DevComponents.DotNetBar.ButtonItem RootMenu;
        public string ClickedText;
        public string SearchText;
    }

    public partial class frmCardView : Form
    {
        CardDescription[] Cards = new CardDescription[0];
        MySorter sorter = new MySorter();
        ListViewItem[] resultItem = new ListViewItem[0];
        int CurrentCardID = 0;
        string CurrentQueryString = "";
        Hashtable RootMenuTable = new Hashtable();
        Hashtable MenuInfoTable = new Hashtable();
        Hashtable SearchTextTable = new Hashtable();

        public frmCardView()
        {
            InitializeComponent();
            Global.frmMainHolder.MDIChildAdded(this);
        }

        private string GetFilter()
        {
            StringBuilder filter = new StringBuilder();
            foreach (DevComponents.DotNetBar.ButtonItem item in SearchTextTable.Keys)
            {
                if (SearchTextTable[item] != null)
                {
                    string s = ((string)SearchTextTable[item]).Trim();
                    if (s != "")
                    {
                        filter.Append("+(");
                        filter.Append(s);
                        filter.Append(") ");
                    }
                }
            }

            return filter.ToString().TrimEnd();
        }

        public void DoSearch()
        {
            DoSearch(textBoxX1.Text);
        }

        public void ReSearch()
        {
            DoSearch(CurrentQueryString);
        }

        public void DoSearch(string queryString)
        {
            pnSameTitle.Visible = true;
            //pnSameTitle.Enabled = false;
            pnSameTitle.Update();

            CardLibrary cardLibrary = CardLibrary.GetInstance();
            string querystring = QueryMapper.Mapper(queryString);

            string filter = QueryMapper.Mapper(GetFilter());
            if (string.Equals(queryString.Trim(), ""))
                CurrentQueryString = filter;
            else if (string.Equals(filter, ""))
                CurrentQueryString = querystring;
            else
                CurrentQueryString = string.Format("+({0}) +({1})", querystring, filter);

            if (CurrentQueryString.Trim().Equals(""))
                Cards = cardLibrary.GetCards(sorter.GetSortFields());
            else
                Cards = cardLibrary.Search(CurrentQueryString, sorter.GetSortFields());


            resultItem = new ListViewItem[Cards.Length];
            for (int i = 0; i < Cards.Length; i++)
            {
                CardDescription card = Cards[i];
                ListViewItem tmpItem = new ListViewItem();
                tmpItem.Text = card.name;
                //tmpItem.UseItemStyleForSubItems = false;
                Color color = tmpItem.ForeColor;
                switch (card.iCardtype)
                {
                    case 0:
                        color = Color.OrangeRed;
                        break;
                    case 4:
                        color = Color.Green;
                        break;
                    case 5:
                        color = Color.Fuchsia;
                        break;
                    case 1:
                        color = Color.SandyBrown;
                        break;
                    case 2:
                        color = Color.DarkOrchid;
                        break;
                    case 3:
                        color = Color.DodgerBlue;
                        break;
                    case 6:
                        color = Color.DarkSlateGray;
                        break;
                }
                tmpItem.ForeColor = color;

                tmpItem.SubItems.Add(card.japName);
                tmpItem.SubItems.Add(card.sCardType);
                tmpItem.SubItems.Add(card.tribe);
                tmpItem.SubItems.Add(card.element);
                if (card.level > 0)
                    tmpItem.SubItems.Add(card.level.ToString());
                else
                    tmpItem.SubItems.Add("");
                tmpItem.SubItems.Add(card.atk);
                tmpItem.SubItems.Add(card.def);
                tmpItem.SubItems.Add(card.ID.ToString());

                if (!listViewEx1.VirtualMode)
                {
                    if (listViewEx1.View == View.Details)
                        tmpItem.ImageIndex = card.iCardtype;
                    else if (listViewEx1.View == View.LargeIcon)
                        tmpItem.ImageIndex = Global.largePicLoader.GetImageIndex(card.ID);
                }
                //listView1.Items.Add(tmpItem);
                resultItem[i] = tmpItem;
            }

            listViewEx1.BeginUpdate();
            if (listViewEx1.VirtualMode)
            {
                View v = listViewEx1.View;
                listViewEx1.View = View.List;
                listViewEx1.VirtualListSize = Cards.Length;
                listViewEx1.View = v;
            }
            else
            {
                listViewEx1.Items.Clear();
                listViewEx1.Items.AddRange(resultItem);
            }
            listViewEx1.EndUpdate();

            this.Text = "结果数：" + Cards.Length;
            CountCards();

            pnSameTitle.Visible = false;
            listViewEx1.Update();
            //pnSameTitle.Enabled = true;

            if (listViewEx1.View == View.LargeIcon && !Global.loadPicEnd)
                CheckPics();
        }

        private void CheckPics()
        {
            System.Threading.Thread.Sleep(Global.ReDrawTime);
            listViewEx1.Refresh();
        }

        private void CountCards()
        {
            int Normal = 0;
            int Effect = 0;
            int Ritual = 0;
            int Fusion = 0;
            int Level4 = 0;
            int Level6 = 0;
            int Level7 = 0;
            int Spell = 0;
            int Trap = 0;

            foreach (CardDescription card in Cards)
            {
                if (card.iCardtype != 4 && card.iCardtype != 5)
                {
                    if (card.level <= 4)
                        Level4++;
                    else if (card.level < 7)
                        Level6++;
                    else
                        Level7++;

                    switch (card.iCardtype)
                    {
                        case 0:
                            Effect++;
                            break;
                        case 1:
                            Normal++;
                            break;
                        case 2:
                            Fusion++;
                            break;
                        case 6:
                            Fusion++;
                            break;
                        case 3:
                            Ritual++;
                            break;
                    }
                }
                else
                {
                    if (card.iCardtype == 4)
                        Spell++;
                    else
                        Trap++;
                }
            }

            Global.frmMainHolder.ShowCardsCount(new int[] { Normal, Effect, Ritual, Fusion, Level4, Level6, Level7, Spell, Trap, Cards.Length });
        }

        private void textBoxX1_ButtonCustomClick(object sender, EventArgs e)
        {
            //listViewEx1.RedrawItems(3000, 3000, false);

            DoSearch();
        }

        private void textBoxX1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar.Equals('\r'))
                DoSearch();
        }

        private void textBoxX1_ButtonCustom2Click(object sender, EventArgs e)
        {
            textBoxX1.Text = "";
        }

        private void panelEx1_Resize(object sender, EventArgs e)
        {
            textBoxX1.Size = new Size(panelEx1.Size.Width - 24, textBoxX1.Size.Height);
        }

        private void listViewEx1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewEx1.SelectedIndices != null)
                if (listViewEx1.SelectedIndices.Count == 1)
                {
                    CurrentCardID = Cards[listViewEx1.SelectedIndices[0]].ID;
                }
            ShowCurrentCard();
        }

        private void ShowCurrentCard()
        {
            if (CurrentCardID > 0)
            {
                Global.frmMainHolder.ShowCard(CurrentCardID);
            }
        }

        private string FormatMenuText(string s)
        {
            if (s == null)
                return "　　　　";

            s = CharacterSet.DBCToSBC(s);

            switch (s.Length)
            {
                case 0:
                    return "　　　　";
                case 1:
                    return "　　　" + s;
                case 2:
                    return "　　" + s;
                case 3:
                    return "　" + s;
                default:
                    return s;
            }
        }

        private void MenuItem_Click(object sender, EventArgs e)
        {
            if (MenuInfoTable.ContainsKey(sender))
            {
                MenuInfo menuinfo = (MenuInfo)MenuInfoTable[sender];
                DevComponents.DotNetBar.ButtonItem rootmenu = menuinfo.RootMenu;
                rootmenu.Text = FormatMenuText(menuinfo.ClickedText);
                
                if (rootmenu.Text == (string)RootMenuTable[rootmenu])
                {
                    SearchTextTable[rootmenu] = null;
                    rootmenu.ForeColor = Color.DimGray;
                    rootmenu.FontBold = false;
                }
                else
                {
                    SearchTextTable[rootmenu] = menuinfo.SearchText;
                    rootmenu.ForeColor = Color.Black;
                    rootmenu.FontBold = true;
                }
                //if (menuinfo.ClickedText == menuinfo.RootMenu.Text)
            }

            DoSearch();
        }

        private void ResetItem_Click(object sender, EventArgs e)
        {
            foreach (DevComponents.DotNetBar.ButtonItem item in RootMenuTable.Keys)
            {
                item.Text = (string)RootMenuTable[item];
                item.ForeColor = Color.DimGray;
                item.FontBold = false;
            }
            SearchTextTable.Clear();
            DoSearch();
        }

        private void LoadSubMenu(XmlNode MenuNode, DevComponents.DotNetBar.ButtonItem MenuItem, DevComponents.DotNetBar.ButtonItem RootMenu)
        {
            SortedList MenuItems = new SortedList(new MySort());
            foreach (XmlNode node in MenuNode.ChildNodes)
            {
                XmlElement xe = (XmlElement)node;
                switch (node.Name)
                {
                    case "MenuItem":
                        DevComponents.DotNetBar.ButtonItem menuitem = new DevComponents.DotNetBar.ButtonItem();
                        menuitem.ImagePaddingHorizontal = 8;
                        menuitem.Text = xe.GetAttribute("Text");
                        MenuInfo menuinfo = new MenuInfo();
                        menuinfo.RootMenu = RootMenu;
                        menuinfo.SearchText = node.SelectSingleNode("SearchText").InnerText;
                        if (node.Attributes["ClickedText"] == null)
                            menuinfo.ClickedText = menuitem.Text;
                        else
                            menuinfo.ClickedText = xe.GetAttribute("ClickedText");
                        MenuInfoTable.Add(menuitem, menuinfo);
                        menuitem.Click += new EventHandler(MenuItem_Click);
                        MenuItems.Add(int.Parse(xe.GetAttribute("Id")), menuitem);
                        break;
                    case "Separator":
                        DevComponents.DotNetBar.LabelItem labelitem = new DevComponents.DotNetBar.LabelItem();
                        labelitem.BackColor = System.Drawing.Color.Transparent;
                        labelitem.BorderSide = DevComponents.DotNetBar.eBorderSide.Bottom;
                        labelitem.BorderType = DevComponents.DotNetBar.eBorderType.SingleLine;
                        labelitem.DividerStyle = true;
                        labelitem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(21)))), ((int)(((byte)(110)))));
                        labelitem.SingleLineColor = System.Drawing.Color.FromArgb(((int)(((byte)(197)))), ((int)(((byte)(197)))), ((int)(((byte)(197)))));
                        MenuItems.Add(int.Parse(xe.GetAttribute("Id")), labelitem);
                        break;
                    case "SubMenuItem":
                        DevComponents.DotNetBar.ButtonItem menuitem2 = new DevComponents.DotNetBar.ButtonItem();
                        menuitem2.ImagePaddingHorizontal = 8;
                        menuitem2.Text = xe.GetAttribute("Text");
                        LoadSubMenu(node, menuitem2, RootMenu);
                        MenuItems.Add(int.Parse(xe.GetAttribute("Id")), menuitem2);
                        break;
                }
            }

            foreach (object o in MenuItems.Values)
                MenuItem.SubItems.Add((DevComponents.DotNetBar.BaseItem)o);
        }

        private void LoadMenu()
        {
            try
            {
                if (!System.IO.File.Exists(Global.appPath + "\\MenuList.xml"))
                {
                    string s = global::DeckBuilder2.Properties.Resources.MenuList;
                    System.IO.File.WriteAllText(Global.appPath + "\\MenuList.xml", s, System.Text.Encoding.UTF8);
                }

                bar1.Items.Clear();
                RootMenuTable = new Hashtable();
                MenuInfoTable = new Hashtable();
                SearchTextTable = new Hashtable();

                XmlDocument doc = new XmlDocument();
                doc.Load(Global.appPath + "\\MenuList.xml");
                XmlNode rootnode = doc.SelectSingleNode("//MenuList");

                SortedList MenuItems = new SortedList(new MySort());
                foreach (XmlNode node in rootnode.ChildNodes)
                {
                    XmlElement xe = (XmlElement)node;
                    DevComponents.DotNetBar.ButtonItem menuitem = new DevComponents.DotNetBar.ButtonItem();
                    menuitem.ForeColor = System.Drawing.Color.DimGray;
                    menuitem.ImagePaddingHorizontal = 8;
                    menuitem.Text = xe.GetAttribute("Text");
                    RootMenuTable.Add(menuitem, menuitem.Text);
                    LoadSubMenu(node, menuitem, menuitem);
                    MenuItems.Add(int.Parse(xe.GetAttribute("MenuId")), menuitem);
                }

                foreach (object o in MenuItems.Values)
                    this.bar1.Items.Add((DevComponents.DotNetBar.ButtonItem)o);

                DevComponents.DotNetBar.ButtonItem resetitem = new DevComponents.DotNetBar.ButtonItem();
                resetitem.ForeColor = System.Drawing.SystemColors.MenuText;
                resetitem.ImagePaddingHorizontal = 8;
                resetitem.ItemAlignment = DevComponents.DotNetBar.eItemAlignment.Far;
                resetitem.Name = "buttonItem25";
                resetitem.Text = "重置";
                resetitem.Click += new System.EventHandler(this.ResetItem_Click);

                DevComponents.DotNetBar.ButtonItem changeitem = new DevComponents.DotNetBar.ButtonItem();
                changeitem.ForeColor = System.Drawing.SystemColors.MenuText;
                changeitem.ImagePaddingHorizontal = 8;
                changeitem.ItemAlignment = DevComponents.DotNetBar.eItemAlignment.Far;
                changeitem.Name = "buttonItem32";
                changeitem.Text = "切换视图";
                changeitem.Click += new System.EventHandler(this.buttonItem4_Click);

                this.bar1.Items.AddRange(new DevComponents.DotNetBar.ButtonItem[] { resetitem, changeitem });
            }
            catch
            {
                MessageBox.Show("菜单载入发生异常！", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void frmCardView_Load(object sender, EventArgs e)
        {
            LoadMenu();

            listViewEx1.LargeImageList = Global.frmMainHolder.imageList1;
            listViewEx1.SmallImageList = Global.frmMainHolder.imageList2;

            sorter.AddField("ID", SortField.INT, false);
            sorter.AddField("atkValue", SortField.INT);
            sorter.AddField("defValue", SortField.INT);
            sorter.AddField("name2", SortField.STRING);
            sorter.AddField("japName2", SortField.STRING);
            sorter.AddField("cardType2", SortField.STRING, false);
            sorter.AddField("element", SortField.STRING);
            sorter.AddField("tribe", SortField.STRING);
            sorter.AddField("level", SortField.INT);

            DB2Config config = DB2Config.GetInstance();
            if (string.Equals(config.GetSetting("SaveLayout"), "True", StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(config.GetSetting("CardView"), "List", StringComparison.OrdinalIgnoreCase))
                {
                    listViewEx1.View = View.Details;
                }
                else
                {
                    listViewEx1.View = View.LargeIcon;
                }
            }
        }

        private void listViewEx1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            switch (e.Column)
            {
                case 0:
                    sorter.SortBy("name2");
                    break;
                case 1:
                    sorter.SortBy("japName2");
                    break;
                case 2:
                    sorter.SortBy("cardType2");
                    break;
                case 3:
                    sorter.SortBy("tribe");
                    break;
                case 4:
                    sorter.SortBy("element");
                    break;
                case 5:
                    sorter.SortBy("level");
                    break;
                case 6:
                    sorter.SortBy("atkValue");
                    break;
                case 7:
                    sorter.SortBy("defValue");
                    break;
                case 8:
                    sorter.SortBy("ID");
                    break;
            }

            DoSearch();
        }

        private void expandableSplitter1_SplitterMoving(object sender, SplitterEventArgs e)
        {
            expandableSplitter1.SplitPosition = 45;
        }

        private void expandableSplitter1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (expandableSplitter1.SplitPosition != 45)
                expandableSplitter1.SplitPosition = 45;
        }

        ListViewItem emptyItem = new ListViewItem();
        private void listViewEx1_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            e.Item = emptyItem;
            if (Cards.Length == 0 || e.ItemIndex >= Cards.Length)
            {
                //e.Item = emptyItem;
                return;
            }

            e.Item = resultItem[e.ItemIndex];
            if (listViewEx1.View == View.Details)
                e.Item.ImageIndex = Cards[e.ItemIndex].iCardtype;
            else if (listViewEx1.View == View.LargeIcon)
            {
                // System.IO.File.AppendAllText("debug.txt", e.ItemIndex.ToString() + "\r\n");
                e.Item.ImageIndex = Global.largePicLoader.GetImageIndex(Cards[e.ItemIndex].ID);
            }
            else
            {
                e.Item.ImageIndex = 0;
            }
        }

        private void buttonItem3_Click(object sender, EventArgs e)
        {
            listViewEx1.Update();
        }

        private void buttonItem4_Click(object sender, EventArgs e)
        {
            pnSameTitle.Visible = true;
            pnSameTitle.Enabled = false;
            pnSameTitle.Update();

            if (!listViewEx1.VirtualMode)
            {
                if (listViewEx1.View == View.LargeIcon)
                {
                    for (int i = 0; i < Cards.Length; i++)
                    {
                        resultItem[i].ImageIndex = Cards[i].iCardtype;
                    }
                    listViewEx1.View = View.Details;
                }
                else if (listViewEx1.View == View.Details)
                {
                    for (int i = 0; i < Cards.Length; i++)
                    {
                        resultItem[i].ImageIndex = Global.largePicLoader.GetImageIndex(Cards[i].ID);
                    }
                    listViewEx1.View = View.LargeIcon;
                }
            }
            else
            {
                if (listViewEx1.View == View.Details)
                    listViewEx1.View = View.LargeIcon;
                else if (listViewEx1.View == View.LargeIcon)
                    listViewEx1.View = View.Details;
            }

            pnSameTitle.Visible = false;
            pnSameTitle.Enabled = true;
        }

        private void buttonItem3_Click_1(object sender, EventArgs e)
        {
            sorter.SortBy("name2");
            DoSearch();
        }

        private void buttonItem9_Click(object sender, EventArgs e)
        {
            sorter.SortBy("japName2");
            DoSearch();
        }

        private void buttonItem10_Click(object sender, EventArgs e)
        {
            sorter.SortBy("cardType2");
            DoSearch();
        }

        private void buttonItem11_Click(object sender, EventArgs e)
        {
            sorter.SortBy("tribe");
            DoSearch();
        }

        private void buttonItem12_Click(object sender, EventArgs e)
        {
            sorter.SortBy("element");
            DoSearch();
        }

        private void buttonItem13_Click(object sender, EventArgs e)
        {
            sorter.SortBy("level");
            DoSearch();
        }

        private void buttonItem14_Click(object sender, EventArgs e)
        {
            sorter.SortBy("atkValue");
            DoSearch();
        }

        private void buttonItem15_Click(object sender, EventArgs e)
        {
            sorter.SortBy("defValue");
            DoSearch();
        }

        private void buttonItem16_Click(object sender, EventArgs e)
        {
            sorter.SortBy("ID");
            DoSearch();
        }

        private void buttonItem17_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonItem1_PopupOpen(object sender, DevComponents.DotNetBar.PopupOpenEventArgs e)
        {
            if (listViewEx1.SelectedIndices == null || listViewEx1.SelectedIndices.Count == 0)
            {
                buttonItem6.Enabled = false;
                buttonItem7.Enabled = false;
                buttonItem8.Enabled = false;
            }
            else
            {
                CardDescription card = null;
                try
                {
                   card = Cards[listViewEx1.SelectedIndices[0]];
                }
                catch
                {
                    card = null;
                }
                if (card == null)
                {
                    buttonItem6.Enabled = false;
                    buttonItem7.Enabled = false;
                    buttonItem8.Enabled = false;
                    return;
                }

                if (card.iCardtype == 2 || card.iCardtype == 6)
                {
                    buttonItem6.Text = "至额外卡组";
                    buttonItem6.Enabled = true;
                    buttonItem7.Enabled = true;
                    buttonItem8.Enabled = true;
                }
                else
                {
                    buttonItem6.Enabled = true;
                    buttonItem7.Enabled = true;
                    buttonItem8.Enabled = true;
                }
            }
        }

        private void buttonItem6_Click(object sender, EventArgs e)
        {
            if (listViewEx1.SelectedIndices == null || listViewEx1.SelectedIndices.Count == 0)
                return;
            CardDescription card = Cards[listViewEx1.SelectedIndices[0]];
            if (card.iCardtype == 2 || card.iCardtype == 6)
                Global.frmDeckEditHolder.AddToFusionDeck(card.ID);
            else
                Global.frmDeckEditHolder.AddToMainDeck(card.ID);
        }

        private void buttonItem7_Click(object sender, EventArgs e)
        {
            if (listViewEx1.SelectedIndices == null || listViewEx1.SelectedIndices.Count == 0)
                return;
            CardDescription card = Cards[listViewEx1.SelectedIndices[0]];
            if (card != null)
                Global.frmDeckEditHolder.AddToSideDeck(card.ID);
        }

        private void buttonItem8_Click(object sender, EventArgs e)
        {
            if (listViewEx1.SelectedIndices == null || listViewEx1.SelectedIndices.Count == 0)
                return;
            CardDescription card = Cards[listViewEx1.SelectedIndices[0]];
            Global.frmDeckEditHolder.AddToTempDeck(card.ID);
        }

        private void listViewEx1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Clicks == 2 && e.Button == MouseButtons.Left)
            {
                ListViewItem item = listViewEx1.GetItemAt(e.X, e.Y);
                if (item == null)
                    return;

                int id = int.Parse(item.SubItems[8].Text);
                CardDescription card = CardLibrary.GetInstance().GetCardByID(id);

                if (card.iCardtype == 2 || card.iCardtype == 6)
                {
                    Global.frmDeckEditHolder.AddToFusionDeck(card.ID);
                }
                else
                {
                    Global.frmDeckEditHolder.AddToMainDeck(card.ID);
                }
            }
        }

        private void buttonItem1_PopupClose(object sender, EventArgs e)
        {
            buttonItem6.Text = "至主卡组";
            buttonItem6.Enabled = true;
            buttonItem7.Enabled = true;
            buttonItem8.Enabled = true;
        }

        private void frmCardView_Enter(object sender, EventArgs e)
        {
            ShowCurrentCard();
            CountCards();
            //if (listViewEx1.VirtualMode == true)
                //listViewEx1.Refresh();
        }

        private void listViewEx1_Enter(object sender, EventArgs e)
        {
            if (listViewEx1.SelectedIndices != null)
                if (listViewEx1.SelectedIndices.Count == 1)
                {
                    CurrentCardID = Cards[listViewEx1.SelectedIndices[0]].ID;
                }
            ShowCurrentCard();
        }

        private void listViewEx1_SizeChanged(object sender, EventArgs e)
        {
            //if (listViewEx1.VirtualMode == true && Global.frmMainHolder.ActiveControl == this)
            //    listViewEx1.Refresh();
        }

        private void textBoxX1_Enter(object sender, EventArgs e)
        {
            buttonItem7.Enabled = false;
        }

        private void textBoxX1_Leave(object sender, EventArgs e)
        {
            buttonItem7.Enabled = true;
        }

        private void frmCardView_Activated(object sender, EventArgs e)
        {
            if (listViewEx1.VirtualMode == true)
                listViewEx1.Refresh();
        }

        private void textBoxX1_ImeModeChanged(object sender, EventArgs e)
        {
            if (textBoxX1.ImeMode != ImeMode.On || textBoxX1.ImeMode != ImeMode.OnHalf)
                textBoxX1.ImeMode = ImeMode.On;
        }

        private void frmCardView_TextChanged(object sender, EventArgs e)
        {
            Global.frmMainHolder.MDIChildTextChanged(this);
        }

        private void frmCardView_FormClosed(object sender, FormClosedEventArgs e)
        {
            Global.frmMainHolder.MDIChildClosed(this);
        }

        private void listViewEx1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("DeckBuilder2.DragCard"))
            {
                DragCard dragcard = (DragCard)e.Data.GetData("DeckBuilder2.DragCard");
                if (dragcard.FromObject == sender)
                    e.Effect = DragDropEffects.None;
                else
                {
                    if (dragcard.RemoveFrom == DeckType.None)
                        e.Effect = DragDropEffects.Copy | DragDropEffects.Move;
                    else
                        e.Effect = DragDropEffects.Move;
                }
            }
        }

        private void listViewEx1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("DeckBuilder2.DragCard"))
            {
                DragCard dragcard = (DragCard)e.Data.GetData("DeckBuilder2.DragCard");
                Global.frmDeckEditHolder.DoDragDrop(dragcard, DeckType.None);
            }
        }

        private void listViewEx1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (!string.Equals(DB2Config.GetInstance().GetSetting("NoDrag"), "True", StringComparison.OrdinalIgnoreCase) && e.Button == MouseButtons.Left)
            {
                Application.DoEvents();

                ListViewItem item = (ListViewItem)e.Item;//listViewEx1.GetItemAt(e.X, e.Y);
                if (item == null)
                    return;

                if (item.Selected == false)
                {
                    item.Selected = true;
                    listViewEx1.Update();
                }

                DragCard dragcard = new DragCard();
                int id = int.Parse(item.SubItems[8].Text);
                dragcard.Card = CardLibrary.GetInstance().GetCardByID(id);
                dragcard.FromObject = listViewEx1;
                listViewEx1.DoDragDrop(dragcard, DragDropEffects.Copy);
            }
        }

        private void frmCardView_FormClosing(object sender, FormClosingEventArgs e)
        {
            DB2Config config = DB2Config.GetInstance();
            if (string.Equals(config.GetSetting("SaveLayout"), "True", StringComparison.OrdinalIgnoreCase))
            {
                if (listViewEx1.View == View.Details)
                    config.SetSetting("CardView", "List");
                else
                    config.SetSetting("CardView", "Ico");
            }
        }
    }
}