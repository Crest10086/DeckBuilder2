using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using BaseCardLibrary.Search;
using BaseCardLibrary.DataAccess;
using Lucene.Net.Search;

namespace DeckBuilder2
{
    public partial class frmDeckView : Form
    {
        Deck deck = new Deck();
        CardDescription[] MainCards = new CardDescription[0];
        CardDescription[] SideCards = new CardDescription[0];
        CardDescription[] FusionCards = new CardDescription[0];
        int CurrentCardID = 0;
        MySorter sorter = new MySorter();
        string filename = "";

        public frmDeckView()
        {
            InitializeComponent();
            Global.frmMainHolder.MDIChildAdded(this);
        }

        private void frmDeckView_Load(object sender, EventArgs e)
        {
            DB2Config config = DB2Config.GetInstance();
            SaveLayout("DefaultDeckViewLayout.xml");

            if (string.Equals(config.GetSetting("SaveLayout"), "True", StringComparison.OrdinalIgnoreCase))
                LoadLayout("DeckViewLayout.xml");

            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "中文名";
            this.columnHeader1.Width = 128;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "日文名";
            this.columnHeader2.Width = 125;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "卡种";
            this.columnHeader3.Width = 65;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "种族";
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "属性";
            this.columnHeader5.Width = 42;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "星级";
            this.columnHeader6.Width = 42;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "攻击";
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "防御";
            // 
            // columnHeader9
            // 
            this.columnHeader9.Text = "自编号";
            this.columnHeader9.Width = 51;

            this.listViewEx1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader7,
            this.columnHeader8,
            this.columnHeader9});


            listViewEx1.LargeImageList = Global.frmMainHolder.imageList1;
            listViewEx2.LargeImageList = Global.frmMainHolder.imageList1;
            listViewEx3.LargeImageList = Global.frmMainHolder.imageList1;
            listViewEx1.SmallImageList = Global.frmMainHolder.imageList2;
            listViewEx2.SmallImageList = Global.frmMainHolder.imageList2;
            listViewEx3.SmallImageList = Global.frmMainHolder.imageList2;

            sorter.AddField("cardType2", SortField.STRING, false);
            sorter.AddField("atkValue", SortField.INT);
            sorter.AddField("defValue", SortField.INT);
            sorter.AddField("ID", SortField.INT, false);
            sorter.AddField("name2", SortField.STRING);
            sorter.AddField("japName2", SortField.STRING);
            sorter.AddField("element", SortField.STRING);
            sorter.AddField("tribe", SortField.STRING);
            sorter.AddField("level", SortField.INT);

            if (string.Equals(config.GetSetting("SaveLayout"), "True", StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(config.GetSetting("DeckView1"), "List", StringComparison.OrdinalIgnoreCase))
                    listViewEx1.View = View.Details;
                else
                    listViewEx1.View = View.LargeIcon;
                if (string.Equals(config.GetSetting("DeckView2"), "List", StringComparison.OrdinalIgnoreCase))
                    listViewEx2.View = View.Details;
                else
                    listViewEx2.View = View.LargeIcon;
                if (string.Equals(config.GetSetting("DeckView3"), "List", StringComparison.OrdinalIgnoreCase))
                    listViewEx3.View = View.Details;
                else
                    listViewEx3.View = View.LargeIcon;
            }
        }

        private void ShowDeckList(CardDescription[] Cards, VirtualListView listview)
        {
            ListViewItem[] resultItem = new ListViewItem[Cards.Length];
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

                if (!listview.VirtualMode)
                {
                    if (listview.View == View.Details)
                        tmpItem.ImageIndex = card.iCardtype;
                    else
                        tmpItem.ImageIndex = Global.largePicLoader.GetImageIndex(card.ID);
                }
                //listView1.Items.Add(tmpItem);
                resultItem[i] = tmpItem;
            }

            listview.BeginUpdate();
            if (listview.VirtualMode)
            {
                View v = listview.View;
                listview.View = View.List;
                listview.VirtualListSize = Cards.Length;
                listview.View = v;
            }
            else
            {
                listview.Items.Clear();
                listview.Items.AddRange(resultItem);
            }
            listview.EndUpdate();
        }

        public void ShowDeck()
        {
            MainCards = deck.MainDeck.GetCards();
            SideCards = deck.SideDeck.GetCards();
            FusionCards = deck.FusionDeck.GetCards();

            ShowDeckList(MainCards, listViewEx1);
            ShowDeckList(SideCards, listViewEx2);
            ShowDeckList(FusionCards, listViewEx3);
        }

        private void LoadDeck()
        {
            OpenFileDialog opdlg = new OpenFileDialog();
            opdlg.Filter = "卡组文件(*.txt;*.deck)|*.txt;*.deck|所有文件(*.*)|*.*";
            opdlg.InitialDirectory = DB2Config.GetInstance().GetSetting("DeckPath");
            if (opdlg.ShowDialog() == DialogResult.OK)
            {
                filename = opdlg.FileName;
                string err = deck.LoadFile(filename);
                //UpdateDeckTotal();

                if (err == null)
                {
                    MessageBox.Show("卡组读入完毕！");
                    string deckname = Regex.Replace(opdlg.FileName, @".[^.]*$", "");
                    deckname = Regex.Replace(deckname, @"^.*\\", "");
                    this.Text = string.Format("{0}", deckname);
                    deck.Sort(sorter.GetSortFields());
                    ShowDeck();
                }
                else
                    MessageBox.Show(err);
            }
            dockContainerItem1.Text = string.Format("主卡组（{0:D2}）", MainCards.Length);
            dockContainerItem2.Text = string.Format("副卡组（{0:D2}）", SideCards.Length);
            dockContainerItem3.Text = string.Format("额外卡组（{0:D2}）", FusionCards.Length);
            CountCards();
        }

        ListViewItem emptyItem = new ListViewItem();
        private void listViewEx1_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (MainCards.Length == 0 || e.ItemIndex >= MainCards.Length)
            {
                e.Item = emptyItem;
                return;
            }

            CardDescription card = MainCards[e.ItemIndex];
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
            e.Item = tmpItem;

            if (listViewEx1.View == View.Details)
                e.Item.ImageIndex = MainCards[e.ItemIndex].iCardtype;
            else
                e.Item.ImageIndex = Global.largePicLoader.GetImageIndex(MainCards[e.ItemIndex].ID);
        }

        private void listViewEx2_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (SideCards.Length == 0 || e.ItemIndex >= SideCards.Length)
            {
                e.Item = emptyItem;
                return;
            }

            CardDescription card = SideCards[e.ItemIndex];
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
            e.Item = tmpItem;

            if (listViewEx2.View == View.Details)
                e.Item.ImageIndex = SideCards[e.ItemIndex].iCardtype;
            else
                e.Item.ImageIndex = Global.largePicLoader.GetImageIndex(SideCards[e.ItemIndex].ID);
        }

        private void listViewEx3_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (FusionCards.Length == 0 || e.ItemIndex >= FusionCards.Length)
            {
                e.Item = emptyItem;
                return;
            }

            CardDescription card = FusionCards[e.ItemIndex];
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
            e.Item = tmpItem;

            if (listViewEx3.View == View.Details)
                e.Item.ImageIndex = FusionCards[e.ItemIndex].iCardtype;
            else
                e.Item.ImageIndex = Global.largePicLoader.GetImageIndex(FusionCards[e.ItemIndex].ID);
        }

        private void bar1_DockTabClosing(object sender, DevComponents.DotNetBar.DockTabClosingEventArgs e)
        {
            
        }

        private void ShowCurrentCard()
        {
            if (CurrentCardID > 0)
            {
                Global.frmMainHolder.ShowCard(CurrentCardID);
            }
        }

        private void listViewEx1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewEx1.SelectedIndices != null)
                if (listViewEx1.SelectedIndices.Count == 1)
                {
                    CurrentCardID = MainCards[listViewEx1.SelectedIndices[0]].ID;
                }
            ShowCurrentCard();
        }

        private void listViewEx2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewEx2.SelectedIndices != null)
                if (listViewEx2.SelectedIndices.Count == 1)
                {
                    CurrentCardID = SideCards[listViewEx2.SelectedIndices[0]].ID;
                }
            ShowCurrentCard();
        }

        private void listViewEx3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewEx3.SelectedIndices != null)
                if (listViewEx3.SelectedIndices.Count == 1)
                {
                    CurrentCardID = FusionCards[listViewEx3.SelectedIndices[0]].ID;
                }
            ShowCurrentCard();
        }

        private void frmDeckView_Activated(object sender, EventArgs e)
        {
            ShowCurrentCard();
            CountCards();
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
            CardDescription[][] DeckCards = new CardDescription[][] { MainCards, FusionCards };

            foreach (CardDescription[] Cards in DeckCards)
            {
                foreach (CardDescription card in Cards)
                {
                    if (card.sCardType.Length < 4)
                        continue;

                    if (card.iCardtype == 1 || card.iCardtype == 0 || card.iCardtype == 3)
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
                            case 3:
                                Ritual++;
                                break;
                            case 6:
                                Ritual++;
                                break;
                        }
                    }
                    else if (card.iCardtype == 4)
                        Spell++;
                    else if (card.iCardtype == 5)
                        Trap++;
                    else if (card.iCardtype == 2 || card.iCardtype == 6)
                        Fusion++;
                }
            }

            Global.frmMainHolder.ShowCardsCount(new int[] { Normal, Effect, Ritual, Fusion, Level4, Level6, Level7, Spell, Trap, MainCards.Length + FusionCards.Length });
        }

        private void buttonItem17_Click(object sender, EventArgs e)
        {
            VirtualListView listViewEx1 = CurrentListView();

            if (listViewEx1.View == View.Details)
            {
                if (!listViewEx1.VirtualMode)
                {
                    CardDescription[] cards = CurrentCards();
                    for (int i = 0; i < listViewEx1.Items.Count; i++)
                        listViewEx1.Items[i].ImageIndex = Global.largePicLoader.GetImageIndex(cards[i].ID);
                }
                listViewEx1.View = View.LargeIcon;
            }
            else
            {
                if (!listViewEx1.VirtualMode)
                {
                    CardDescription[] cards = CurrentCards();
                    for (int i = 0; i < listViewEx1.Items.Count; i++)
                        listViewEx1.Items[i].ImageIndex = cards[i].iCardtype;
                }
                listViewEx1.View = View.Details;
            }
        }

        private void buttonItem32_Click(object sender, EventArgs e)
        {
            LoadDeck();
        }

        private void buttonItem33_Click(object sender, EventArgs e)
        {
            deck.Clear();
            dockContainerItem1.Text = "主卡组（00）";
            dockContainerItem2.Text = "副卡组（00）";
            dockContainerItem3.Text = "额外卡组（00）";
            ShowDeck();
        }

        private void buttonItem19_Click(object sender, EventArgs e)
        {
            sorter.SortBy("name2");
            deck.Sort(sorter.GetSortFields());
            ShowDeck();
        }

        private void buttonItem20_Click(object sender, EventArgs e)
        {
            sorter.SortBy("japName2");
            deck.Sort(sorter.GetSortFields());
            ShowDeck();
        }

        private void buttonItem21_Click(object sender, EventArgs e)
        {
            sorter.SortBy("cardType2");
            deck.Sort(sorter.GetSortFields());
            ShowDeck();
        }

        private void buttonItem22_Click(object sender, EventArgs e)
        {
            sorter.SortBy("tribe");
            deck.Sort(sorter.GetSortFields());
            ShowDeck();
        }

        private void buttonItem23_Click(object sender, EventArgs e)
        {
            sorter.SortBy("element");
            deck.Sort(sorter.GetSortFields());
            ShowDeck();
        }

        private void buttonItem24_Click(object sender, EventArgs e)
        {
            sorter.SortBy("level");
            deck.Sort(sorter.GetSortFields());
            ShowDeck();
        }

        private void buttonItem25_Click(object sender, EventArgs e)
        {
            sorter.SortBy("atkValue");
            deck.Sort(sorter.GetSortFields());
            ShowDeck();
        }

        private void buttonItem26_Click(object sender, EventArgs e)
        {
            sorter.SortBy("defValue");
            deck.Sort(sorter.GetSortFields());
            ShowDeck();
        }

        private void buttonItem27_Click(object sender, EventArgs e)
        {
            sorter.SortBy("ID");
            deck.Sort(sorter.GetSortFields());
            ShowDeck();
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
            deck.Sort(sorter.GetSortFields());
            ShowDeck();
        }

        private void buttonItem35_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public VirtualListView CurrentListView()
        {
            if (this.ActiveControl.GetType() == typeof(VirtualListView))
                return (VirtualListView)this.ActiveControl;
            else
                return null;
        }

        private CardDescription[] CurrentCards()
        {
            switch (CurrentListView().Name)
            {
                case "listViewEx1":
                    return MainCards;
                case "listViewEx2":
                    return SideCards;
                case "listViewEx3":
                    return FusionCards;
            }

            return null;
        }

        private void buttonItem2_PopupOpen(object sender, DevComponents.DotNetBar.PopupOpenEventArgs e)
        {
            VirtualListView listViewEx = CurrentListView();
            CardDescription[] Cards = CurrentCards();  

            if (listViewEx.SelectedIndices == null || listViewEx.SelectedIndices.Count == 0)
            {
                buttonItem28.Enabled = false;
                buttonItem29.Enabled = false;
                buttonItem30.Enabled = false;
            }
            else
            {
                CardDescription card = null;
                try
                {
                    card = Cards[listViewEx.SelectedIndices[0]];
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
                    buttonItem28.Text = "至额外卡组";
                    buttonItem28.Enabled = true;
                    buttonItem29.Enabled = true;
                    buttonItem30.Enabled = true;
                }
                else
                {
                    buttonItem28.Enabled = true;
                    buttonItem29.Enabled = true;
                    buttonItem30.Enabled = true;
                }
            }
        }

        private void buttonItem28_Click(object sender, EventArgs e)
        {
            VirtualListView listViewEx = CurrentListView();
            CardDescription[] Cards = CurrentCards();  
            if (listViewEx.SelectedIndices == null || listViewEx.SelectedIndices.Count == 0)
                return;
            CardDescription card = Cards[listViewEx.SelectedIndices[0]];
            if (card.iCardtype == 2 || card.iCardtype == 6)
                Global.frmDeckEditHolder.AddToFusionDeck(card.ID);
            else
                Global.frmDeckEditHolder.AddToMainDeck(card.ID);
        }

        private void buttonItem29_Click(object sender, EventArgs e)
        {
            VirtualListView listViewEx = CurrentListView();
            CardDescription[] Cards = CurrentCards();
            if (listViewEx.SelectedIndices == null || listViewEx.SelectedIndices.Count == 0)
                return;
            CardDescription card = Cards[listViewEx.SelectedIndices[0]];
            if (card != null)
                Global.frmDeckEditHolder.AddToSideDeck(card.ID);
        }

        private void buttonItem30_Click(object sender, EventArgs e)
        {
            VirtualListView listViewEx = CurrentListView();
            CardDescription[] Cards = CurrentCards();
            if (listViewEx.SelectedIndices == null || listViewEx.SelectedIndices.Count == 0)
                return;
            CardDescription card = Cards[listViewEx.SelectedIndices[0]];
            Global.frmDeckEditHolder.AddToTempDeck(card.ID);
        }

        private void buttonItem36_Click(object sender, EventArgs e)
        {
            if (Global.frmDeckEditHolder.isChange())
            {
                if (MessageBox.Show("卡组尚未保存，是否读入新卡组？", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }
            }
            Global.frmDeckEditHolder.LoadDeck(filename);
            Global.frmDeckEditHolder.Activate();
        }

        private void listViewEx1_MouseDown(object sender, MouseEventArgs e)
        {
            VirtualListView listViewEx = CurrentListView();
            if (e.Clicks == 2 && e.Button == MouseButtons.Left)
            {
                ListViewItem item = listViewEx.GetItemAt(e.X, e.Y);
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

        private void dotNetBarManager1_DockTabClosing(object sender, DevComponents.DotNetBar.DockTabClosingEventArgs e)
        {
            deck.Clear();
            dockContainerItem1.Text = "主卡组（00）";
            dockContainerItem2.Text = "副卡组（00）";
            dockContainerItem3.Text = "额外卡组（00）";
            ShowDeck();
            e.Cancel = true;
        }

        private void buttonItem2_PopupFinalized(object sender, EventArgs e)
        {
            buttonItem28.Text = "至主卡组";
            buttonItem28.Enabled = true;
            buttonItem29.Enabled = true;
            buttonItem30.Enabled = true;
        }

        private void frmDeckView_Enter(object sender, EventArgs e)
        {
            CountCards();
        }

        private void frmDeckView_TextChanged(object sender, EventArgs e)
        {
            Global.frmMainHolder.MDIChildTextChanged(this);
        }

        private void frmDeckView_FormClosed(object sender, FormClosedEventArgs e)
        {
            Global.frmMainHolder.MDIChildClosed(this);
        }

        private void listViewEx1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (!string.Equals(DB2Config.GetInstance().GetSetting("NoDrag"), "True", StringComparison.OrdinalIgnoreCase) && e.Button == MouseButtons.Left)
            {
                Application.DoEvents();

                VirtualListView ListViewEx = (VirtualListView)sender;
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
                dragcard.FromObject = ListViewEx;
                ListViewEx.DoDragDrop(dragcard, DragDropEffects.Copy);
            }
        }

        private void frmDeckView_FormClosing(object sender, FormClosingEventArgs e)
        {
            DB2Config config = DB2Config.GetInstance();
            if (string.Equals(config.GetSetting("SaveLayout"), "True", StringComparison.OrdinalIgnoreCase))
            {
                SaveLayout("DeckViewLayout.xml");
                if (listViewEx1.View == View.Details)
                    config.SetSetting("DeckView1", "List");
                else
                    config.SetSetting("DeckView1", "Ico");
                if (listViewEx2.View == View.Details)
                    config.SetSetting("DeckView2", "List");
                else
                    config.SetSetting("DeckView2", "Ico");
                if (listViewEx3.View == View.Details)
                    config.SetSetting("DeckView3", "List");
                else
                    config.SetSetting("DeckView3", "Ico");
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
    }
}