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
    public partial class frmDeckEdit : Form
    {
        Deck deck = new Deck();
        CardDescription[] Cards = new CardDescription[0];
        CardDescription[] MainCards = new CardDescription[0];
        CardDescription[] SideCards = new CardDescription[0];
        CardDescription[] FusionCards = new CardDescription[0];
        CardDescription[] TempCards = new CardDescription[0];
        int CurrentCardID = 0;
        MySorter sorter = new MySorter();

        public Deck GetCurrentDeck()
        {
            return deck;
        }

        public frmDeckEdit()
        {
            InitializeComponent();
            Global.frmMainHolder.MDIChildAdded(this);
        }

        private void frmDeckEdit_Load(object sender, EventArgs e)
        {
            DB2Config config = DB2Config.GetInstance();

            SaveLayout("DefaultDeckEditLayout.xml");
            if (string.Equals(config.GetSetting("SaveLayout"), "True", StringComparison.OrdinalIgnoreCase))
                LoadLayout("DeckEditLayout.xml");

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
            listViewEx4.LargeImageList = Global.frmMainHolder.imageList1;
            listViewEx1.SmallImageList = Global.frmMainHolder.imageList2;
            listViewEx2.SmallImageList = Global.frmMainHolder.imageList2;
            listViewEx3.SmallImageList = Global.frmMainHolder.imageList2;
            listViewEx4.SmallImageList = Global.frmMainHolder.imageList2;

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
                if (string.Equals(config.GetSetting("DeckEdit1"), "List", StringComparison.OrdinalIgnoreCase))
                    listViewEx1.View = View.Details;
                else
                    listViewEx1.View = View.LargeIcon;
                if (string.Equals(config.GetSetting("DeckEdit2"), "List", StringComparison.OrdinalIgnoreCase))
                    listViewEx2.View = View.Details;
                else
                    listViewEx2.View = View.LargeIcon;
                if (string.Equals(config.GetSetting("DeckEdit3"), "List", StringComparison.OrdinalIgnoreCase))
                    listViewEx3.View = View.Details;
                else
                    listViewEx3.View = View.LargeIcon;
                if (string.Equals(config.GetSetting("DeckEdit4"), "List", StringComparison.OrdinalIgnoreCase))
                    listViewEx4.View = View.Details;
                else
                    listViewEx4.View = View.LargeIcon;
            }

            bar1.DockTabControl.AllowDrop = true;
            bar1.DockTabControl.DragEnter += new DragEventHandler(bar1_DragEnter);
            bar1.DockTabControl.DragDrop += new DragEventHandler(bar1_DragDrop);
        }

        private void ShowCurrentCard()
        {
            if (CurrentCardID > 0)
            {
                Global.frmMainHolder.ShowCard(CurrentCardID);
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

        public void ShowQuickView()
        {
            VirtualListView listView = Global.frmMainHolder.virtualListView1;
            CardLibrary cardLibrary = CardLibrary.GetInstance();
            CardDescription[] maincards = deck.MainDeck.GetCardList();
            CardDescription[] sidecards = deck.SideDeck.GetCardList();
            CardDescription[] fusioncards = deck.FusionDeck.GetCardList();
            ListViewItem[] resultItems = new ListViewItem[maincards.Length + sidecards.Length + fusioncards.Length];
            int index = 0;

            int sum = 0;
            foreach (CardDescription card in maincards)
            {
                ListViewItem item = new ListViewItem();
                item.Text = card.name;
                int count = deck.MainDeck.GetCount(card.ID);
                item.SubItems.Add(count.ToString());
                sum += count;
                item.ImageIndex = card.iCardtype;
                item.Group = listView.Groups[0];
                resultItems[index++] = item;
            }
            

            sum = 0;
            foreach (CardDescription card in sidecards)
            {
                ListViewItem item = new ListViewItem();
                item.Text = card.name;
                int count = deck.SideDeck.GetCount(card.ID);
                item.SubItems.Add(count.ToString());
                sum += count;
                item.ImageIndex = card.iCardtype;
                item.Group = listView.Groups[1];
                resultItems[index++] = item;
            }
            

            sum = 0;
            foreach (CardDescription card in fusioncards)
            {
                ListViewItem item = new ListViewItem();
                item.Text = card.name;
                int count = deck.FusionDeck.GetCount(card.ID);
                item.SubItems.Add(count.ToString());
                sum += count;
                item.ImageIndex = card.iCardtype;
                item.Group = listView.Groups[2];
                resultItems[index++] = item;
            }
            

            listView.Items.Clear();
            listView.Items.AddRange(resultItems);
            ShowQuickViewCount();
        }

        public void ShowQuickViewCount()
        {
            CountCards();
            VirtualListView listView = Global.frmMainHolder.virtualListView1;
            listView.Groups[0].Header = string.Format("主卡组（{0:D2}）", deck.MainDeck.Count());
            listView.Groups[1].Header = string.Format("副卡组（{0:D2}）", deck.SideDeck.Count());
            listView.Groups[2].Header = string.Format("额外卡组（{0:D2}）", deck.FusionDeck.Count());

        }

        public void ShowDeck()
        {
            MainCards = deck.MainDeck.GetCards();
            SideCards = deck.SideDeck.GetCards();
            FusionCards = deck.FusionDeck.GetCards();
            TempCards = deck.TempDeck.GetCards();

            ShowQuickView();
            ShowDeckList(MainCards, listViewEx1);
            ShowDeckList(SideCards, listViewEx2);
            ShowDeckList(FusionCards, listViewEx3);
            ShowDeckList(TempCards, listViewEx4);
        }

        public DeckType GetDeck(VirtualListView listview)
        {
            if (listview == null)
                return DeckType.None;

            switch (listview.Name)
            {
                case "listViewEx1":
                    return DeckType.MainDeck;
                case "listViewEx2":
                    return DeckType.SideDeck;
                case "listViewEx3":
                    return DeckType.FusionDeck;
                case "listViewEx4":
                    return DeckType.TempDeck;
            }

            return DeckType.None;
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
            VirtualListView listview = CurrentListView();
            if (listview == null)
                return null;

            switch (listview.Name)
            {
                case "listViewEx1":
                    return MainCards;
                case "listViewEx2":
                    return SideCards;
                case "listViewEx3":
                    return FusionCards;
                case "listViewEx4":
                    return TempCards;
            }

            return null;
        }

        public void ShowDeckCount()
        {
            if (deck.isEmpty())
                Global.frmMainHolder.bar1.Text = "当前卡组";
            else
                Global.frmMainHolder.bar1.Text = "当前卡组—" + deck.GetDeckCamp();

            dockContainerItem1.Text = string.Format("主卡组（{0:D2}）", MainCards.Length);
            dockContainerItem2.Text = string.Format("副卡组（{0:D2}）", SideCards.Length);
            dockContainerItem3.Text = string.Format("额外卡组（{0:D2}）", FusionCards.Length);
            dockContainerItem4.Text = string.Format("候选卡表（{0:D2}）", TempCards.Length);
        }

        public delegate void ShowDeckCountInvoker();

        public void CountCards()
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
                    else if (card.iCardtype == 4)
                        Spell++;
                    else if (card.iCardtype == 5)
                        Trap++;
                    else if (card.iCardtype == 2 || card.iCardtype == 6)
                        Fusion++;
                } 
            }

            
            Global.frmMainHolder.ShowCardsCount(new int[] { Normal, Effect, Ritual, Fusion, Level4, Level6, Level7, Spell, Trap, MainCards.Length + FusionCards.Length });
            //刷新界面上的信息要以委托的方式
            this.BeginInvoke(new ShowDeckCountInvoker(this.ShowDeckCount), null);
        }

        public bool isChange()
        {
            return deck.isChange();
        }

        public bool isEmpty()
        {
            return deck.isEmpty();
        }

        public void LoadDeck(string filename)
        {
            string err = deck.LoadFile(filename);
            //UpdateDeckTotal();

            if (err == null)
                MessageBox.Show("卡组读入完毕！");
            else
                MessageBox.Show(err);
            this.Text = string.Format("编辑：{0}", deck.DeckName);

            if (deck.isEmpty())
                Global.frmMainHolder.bar1.Text = "当前卡组";
            else
                Global.frmMainHolder.bar1.Text = "当前卡组—" + deck.GetDeckCamp();

            deck.Sort(sorter.GetSortFields());
            ShowDeck();

            dockContainerItem1.Text = string.Format("主卡组（{0:D2}）", MainCards.Length);
            dockContainerItem2.Text = string.Format("副卡组（{0:D2}）", SideCards.Length);
            dockContainerItem3.Text = string.Format("额外卡组（{0:D2}）", FusionCards.Length);
            dockContainerItem4.Text = string.Format("候选卡堆（{0:D2}）", TempCards.Length);
        }

        public void LoadDeck()
        {
            OpenFileDialog opdlg = new OpenFileDialog();
            opdlg.Filter = "卡组文件(*.txt;*.deck)|*.txt;*.deck|所有文件(*.*)|*.*";
            opdlg.InitialDirectory = DB2Config.GetInstance().GetSetting("DeckPath");
            if (opdlg.ShowDialog() == DialogResult.OK)
            {
                string filename = opdlg.FileName;
                LoadDeck(filename);
            }
            CountCards();
        }

        private void buttonItem32_Click(object sender, EventArgs e)
        {
            if (deck.isChange() && !deck.isEmpty())
            {
                if (MessageBox.Show("卡组尚未保存，是否读入新卡组？", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }
            }
            LoadDeck();
        }

        ListViewItem emptyItem = new ListViewItem();
        private void listViewEx1_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            e.Item = emptyItem;

            VirtualListView listViewEx = (VirtualListView)sender;
            CardDescription[] cards = null;
            switch (listViewEx.Name)
            {
                case "listViewEx1":
                    cards = MainCards;
                    break;
                case "listViewEx2":
                    cards = SideCards;
                    break;
                case "listViewEx3":
                    cards = FusionCards;
                    break;
                case "listViewEx4":
                    cards = TempCards;
                    break;
            }

            if (cards == null)
                return;

            if (e.ItemIndex >= 0 && e.ItemIndex < cards.Length)
            {
                CardDescription card = null;
                try
                {
                    card = cards[e.ItemIndex];
                }
                catch
                {
                    return;
                }
                if (card == null)
                    return;

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
                if (listViewEx.View == View.Details)
                    tmpItem.ImageIndex = card.iCardtype;
                else
                    tmpItem.ImageIndex = Global.largePicLoader.GetImageIndex(card.ID);

                e.Item = tmpItem;
            }
        }

        private void listViewEx3_SelectedIndexChanged(object sender, EventArgs e)
        {
            VirtualListView listViewEx = CurrentListView();
            Cards = CurrentCards();
            if (listViewEx == null || Cards == null)
                return;

            if (listViewEx.SelectedIndices != null)
                if (listViewEx.SelectedIndices.Count == 1)
                {
                    int i = listViewEx.SelectedIndices[0];
                    if (i < Cards.Length)
                    {
                        CurrentCardID = Cards[i].ID;
                        ShowCurrentCard();
                    }
                }
        }

        private void buttonItem17_Click(object sender, EventArgs e)
        {
            VirtualListView listViewEx1 = CurrentListView();
            if (listViewEx1 == null)
                return;

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

        private void frmDeckEdit_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Global.appCloing)
            {
                DB2Config config = DB2Config.GetInstance();
                if (string.Equals(config.GetSetting("SaveLayout"), "True", StringComparison.OrdinalIgnoreCase))
                {
                    SaveLayout("DeckEditLayout.xml");
                    if (listViewEx1.View == View.Details)
                        config.SetSetting("DeckEdit1", "List");
                    else
                        config.SetSetting("DeckEdit1", "Ico");
                    if (listViewEx2.View == View.Details)
                        config.SetSetting("DeckEdit2", "List");
                    else
                        config.SetSetting("DeckEdit2", "Ico");
                    if (listViewEx3.View == View.Details)
                        config.SetSetting("DeckEdit3", "List");
                    else
                        config.SetSetting("DeckEdit3", "Ico");
                    if (listViewEx4.View == View.Details)
                        config.SetSetting("DeckEdit4", "List");
                    else
                        config.SetSetting("DeckEdit4", "Ico");
                }
            }
            else
                e.Cancel = true;
        }

        public void AddToMainDeck(int id)
        {
            AddToMainDeck(id, -1);
        }

        public void AddToMainDeck(int id, int index)
        {
            if (deck.isFull(id))
            {
                CardDescription card = CardLibrary.GetInstance().GetCardByID(id);
                MessageBox.Show(string.Format("卡片[{0}]的数量超出限制！", card.name));
                return;
            }
            if (index < 0)
                deck.MainDeck.AddCard(id);
            else
                deck.MainDeck.AddCard(id, index);
            MainCards = deck.MainDeck.GetCards();
            ShowQuickView();
            ShowDeckList(MainCards, listViewEx1);
            CountCards();
        }

        public void AddToSideDeck(int id)
        {
            AddToSideDeck(id, -1);
        }

        public void AddToSideDeck(int id, int index)
        {
            if (deck.isFull(id))
            {
                CardDescription card = CardLibrary.GetInstance().GetCardByID(id);
                MessageBox.Show(string.Format("卡片[{0}]的数量超出限制！", card.name));
                return;
            }
            if (index < 0)
                deck.SideDeck.AddCard(id);
            else
                deck.SideDeck.AddCard(id, index);
            SideCards = deck.SideDeck.GetCards();
            ShowQuickView();
            ShowDeckList(SideCards, listViewEx2);
            CountCards();
        }

        public void AddToFusionDeck(int id)
        {
            AddToFusionDeck(id, -1);
        }

        public void AddToFusionDeck(int id, int index)
        {
            if (deck.isFull(id))
            {
                CardDescription card = CardLibrary.GetInstance().GetCardByID(id);
                MessageBox.Show(string.Format("卡片[{0}]的数量超出限制！", card.name));
                return;
            }
            if (index < 0)
                deck.FusionDeck.AddCard(id);
            else
                deck.FusionDeck.AddCard(id, index);
            FusionCards = deck.FusionDeck.GetCards();
            ShowQuickView();
            ShowDeckList(FusionCards, listViewEx3);
            CountCards();
        }

        public void AddToTempDeck(int id)
        {
            AddToTempDeck(id, -1);
        }

        public void AddToTempDeck(int id, int index)
        {
            if (deck.TempDeck.GetCount(id) == 0)
            {
                if (index < 0)
                    deck.TempDeck.AddCard(id);
                else
                    deck.TempDeck.AddCard(id, index);
                TempCards = deck.TempDeck.GetCards();
                ShowQuickView();
                ShowDeckList(TempCards, listViewEx4);
                CountCards();
            }
        }

        public void RemoveFromMainDeck(int index)
        {
            deck.MainDeck.RemoveCard(index);
            MainCards = deck.MainDeck.GetCards();
            ShowQuickView();
            ShowDeckList(MainCards, listViewEx1);
            //CountCards();
        }

        public void RemoveFromSideDeck(int index)
        {
            deck.SideDeck.RemoveCard(index);
            SideCards = deck.SideDeck.GetCards();
            ShowQuickView();
            ShowDeckList(SideCards, listViewEx2);
            CountCards();
        }

        public void RemoveFromFusionDeck(int index)
        {
            deck.FusionDeck.RemoveCard(index);
            FusionCards = deck.FusionDeck.GetCards();
            ShowQuickView();
            ShowDeckList(FusionCards, listViewEx3);
            CountCards();
        }

        public void RemoveFromTempDeck(int index)
        {
            deck.TempDeck.RemoveCard(index);
            TempCards = deck.TempDeck.GetCards();
            ShowQuickView();
            ShowDeckList(TempCards, listViewEx4);
            CountCards();
        }

        public void RemoveFromMainDeck(string name)
        {
            deck.MainDeck.RemoveCardByName(name);
            MainCards = deck.MainDeck.GetCards();
            ShowQuickView();
            ShowDeckList(MainCards, listViewEx1);
            CountCards();
        }

        public void RemoveFromSideDeck(string name)
        {
            deck.SideDeck.RemoveCardByName(name);
            SideCards = deck.SideDeck.GetCards();
            ShowQuickView();
            ShowDeckList(SideCards, listViewEx2);
            CountCards();
        }

        public void RemoveFromFusionDeck(string name)
        {
            deck.FusionDeck.RemoveCardByName(name);
            FusionCards = deck.FusionDeck.GetCards();
            ShowQuickView();
            ShowDeckList(FusionCards, listViewEx3);
            CountCards();
        }

        public void RemoveFromTempDeck(string name)
        {
            deck.TempDeck.RemoveCardByName(name);
            TempCards = deck.TempDeck.GetCards();
            ShowQuickView();
            ShowDeckList(TempCards, listViewEx4);
            CountCards();
        }

        public bool AddMainCard(int id)
        {
            if (deck.isFull(id))
            {
                CardDescription card = CardLibrary.GetInstance().GetCardByID(id);
                MessageBox.Show(string.Format("卡片[{0}]的数量超出限制！", card.name));
                return false;
            }
            deck.MainDeck.AddCard(id);
            MainCards = deck.MainDeck.GetCards();
            return true;
        }

        public bool AddSideCard(int id)
        {
            if (deck.isFull(id))
            {
                CardDescription card = CardLibrary.GetInstance().GetCardByID(id);
                MessageBox.Show(string.Format("卡片[{0}]的数量超出限制！", card.name));
                return false;
            }
            deck.SideDeck.AddCard(id);
            SideCards = deck.SideDeck.GetCards();
            return true;
        }

        public bool AddFusionCard(int id)
        {
            if (deck.isFull(id))
            {
                CardDescription card = CardLibrary.GetInstance().GetCardByID(id);
                MessageBox.Show(string.Format("卡片[{0}]的数量超出限制！", card.name));
                return false;
            }
            deck.FusionDeck.AddCard(id);
            FusionCards = deck.FusionDeck.GetCards();
            return true;
        }

        public bool AddTempCard(int id)
        {
            if (deck.GetCount(id) > 0)
            {
                return false;
            }

            deck.TempDeck.AddCard(id);
            TempCards = deck.TempDeck.GetCards();
            return true;
        }

        public void RemoveMainCard(int id)
        {
            deck.MainDeck.RemoveCardByID(id);
            MainCards = deck.MainDeck.GetCards();
        }

        public void RemoveSideCard(int id)
        {
            deck.SideDeck.RemoveCardByID(id);
            SideCards = deck.SideDeck.GetCards();
        }

        public void RemoveFusionCard(int id)
        {
            deck.FusionDeck.RemoveCardByID(id);
            FusionCards = deck.FusionDeck.GetCards();
        }

        public void RemoveTempCard(int id)
        {
            deck.TempDeck.RemoveCardByID(id);
            TempCards = deck.TempDeck.GetCards();
        }

        private void buttonItem2_PopupOpen(object sender, DevComponents.DotNetBar.PopupOpenEventArgs e)
        {
            VirtualListView listViewEx = CurrentListView();
            CardDescription[] Cards = CurrentCards();
            if (listViewEx == null || Cards == null)
                return;

            if (listViewEx.SelectedIndices == null || listViewEx.SelectedIndices.Count == 0)
            {
                buttonItem28.Enabled = false;
                buttonItem29.Enabled = false;
                buttonItem30.Enabled = false;
                buttonItem4.Enabled = false;
                buttonItem5.Enabled = false;
            }
            else
            {
                if (listViewEx == listViewEx1)
                {
                    buttonItem28.Enabled = false;
                    buttonItem29.Enabled = true;
                    buttonItem30.Enabled = true;
                    buttonItem4.Enabled = true;
                    buttonItem5.Enabled = true;
                }
                else if (listViewEx == listViewEx2)
                {
                    int j = listViewEx.SelectedIndices[0];
                    CardDescription card = null;
                    if (j >= 0 && j < Cards.Length)
                        card = Cards[j];
                    if (card != null && (card.iCardtype == 2 || card.iCardtype == 6))
                    {

                        buttonItem28.Text = "至额外卡组";
                    }
                    else
                    {
                        buttonItem28.Text = "至主卡组";
                    }

                    buttonItem28.Enabled = true;
                    buttonItem29.Enabled = false;
                    buttonItem30.Enabled = true;
                    buttonItem4.Enabled = true;
                    buttonItem5.Enabled = true;
                }
                else if (listViewEx == listViewEx3)
                {
                    buttonItem28.Enabled = false;
                    buttonItem29.Enabled = true;
                    buttonItem30.Enabled = true;
                    buttonItem4.Enabled = true;
                    buttonItem5.Enabled = true;
                }
                else
                {
                    int j = listViewEx.SelectedIndices[0];
                    CardDescription card = null;
                    if (j >= 0 && j < Cards.Length)
                        card = Cards[j];
                    if (card != null && (card.iCardtype == 2 || card.iCardtype == 6))
                    {
                        buttonItem28.Text = "至额外卡组";
                    }
                    else
                    {
                        buttonItem28.Text = "至主卡组";
                    }
                    buttonItem28.Enabled = true;
                    buttonItem29.Enabled = true;
                    buttonItem30.Enabled = false;
                    buttonItem4.Enabled = false;
                    buttonItem5.Enabled = true;
                }
            }
        }

        private void buttonItem28_Click(object sender, EventArgs e)
        {
            VirtualListView listViewEx = CurrentListView();
            CardDescription[] Cards = CurrentCards();
            if (listViewEx == null || Cards == null)
                return;

            if (listViewEx.SelectedIndices == null || listViewEx.SelectedIndices.Count == 0)
                return;

            CardDescription card = Cards[listViewEx.SelectedIndices[0]];

            if (listViewEx == listViewEx4)
            {
                //RemoveFromTempDeck(listViewEx.SelectedIndices[0]);
                if (card.iCardtype == 2 || card.iCardtype == 6)
                    AddToFusionDeck(card.ID);
                else
                    AddToMainDeck(card.ID);
            }
            else if (listViewEx == listViewEx2)
            {
                RemoveFromSideDeck(listViewEx.SelectedIndices[0]);
                if (card.iCardtype == 2 || card.iCardtype == 6)
                    AddToFusionDeck(card.ID);
                else
                    AddToMainDeck(card.ID);
            }
            
            CountCards();
        }

        private void buttonItem29_Click(object sender, EventArgs e)
        {
            VirtualListView listViewEx = CurrentListView();

            if (listViewEx.SelectedIndices == null || listViewEx.SelectedIndices.Count == 0)
                return;

            CardDescription[] Cards = CurrentCards();
            int i = listViewEx.SelectedIndices[0];
            if (i >= Cards.Length)
                return;

            CardDescription card = Cards[i];
            if (listViewEx == listViewEx3)
            {
                if (!(card.iCardtype == 2 || card.iCardtype == 6))
                    return;
                RemoveFromFusionDeck(listViewEx.SelectedIndices[0]);
            }
            else if (listViewEx == listViewEx1)
            {
                if (card.iCardtype == 2 || card.iCardtype == 6)
                    return;
                RemoveFromMainDeck(listViewEx.SelectedIndices[0]);
            }
            else if (listViewEx == listViewEx4)
            {
                
            }
            AddToSideDeck(card.ID);
            //CountCards();
        }

        private void buttonItem4_Click(object sender, EventArgs e)
        {
            VirtualListView listViewEx = CurrentListView();
            CardDescription[] Cards = CurrentCards();
            if (listViewEx == null || Cards == null)
                return;
            if (listViewEx.SelectedIndices == null || listViewEx.SelectedIndices.Count == 0)
                return;
            int id = Cards[listViewEx.SelectedIndices[0]].ID;
            if (listViewEx == listViewEx1)
            {
                AddToMainDeck(id);
            }
            else if (listViewEx == listViewEx2)
            {
                AddToSideDeck(id);
            }
            else
            {
                AddToFusionDeck(id);
            }
            CountCards();
        }

        private void buttonItem5_Click(object sender, EventArgs e)
        {
            VirtualListView listViewEx = CurrentListView();
            CardDescription[] Cards = CurrentCards();
            if (listViewEx == null || Cards == null)
                return;
            if (listViewEx.SelectedIndices == null || listViewEx.SelectedIndices.Count == 0)
                return;
            int index = listViewEx.SelectedIndices[0];
            if (listViewEx == listViewEx1)
            {
                RemoveFromMainDeck(index);
            }
            else if (listViewEx == listViewEx2)
            {
                RemoveFromSideDeck(index);
            }
            else if (listViewEx == listViewEx3)
            {
                RemoveFromFusionDeck(index);
            }
            else
            {
                RemoveFromTempDeck(index);
            }

            CountCards();
        }

        public void ClearDeck()
        {
            if (deck.isChange() && !deck.isEmpty())
            {
                if (MessageBox.Show("卡组尚未保存，是否清空？", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }
            }
            deck.Clear();
            ShowDeck();
            CountCards();
            this.Text = "卡组编辑";
            Global.frmMainHolder.bar1.Text = "当前卡组";
        }

        private void buttonItem33_Click(object sender, EventArgs e)
        {
            ClearDeck();
        }

        public bool SaveDeck()
        {
            SaveFileDialog opdlg = new SaveFileDialog();
            opdlg.Filter = "卡组文件(*.deck)|*.deck|CGI卡组文件(*.txt)|*.txt|文本文档(*.txt)|*.txt|所有文件(*.*)|*.*";
            try
            {
                opdlg.FilterIndex = int.Parse(DB2Config.GetInstance().GetSetting("SaveType"));
            }
            catch
            {
                opdlg.FilterIndex = 0;
            }
            opdlg.InitialDirectory = DB2Config.GetInstance().GetSetting("DeckPath");
            opdlg.FileName = deck.DeckName;
            if (opdlg.ShowDialog() == DialogResult.OK)
            {
                DB2Config.GetInstance().SetSetting("SaveType", opdlg.FilterIndex.ToString());
                if (deck.MainDeck.Count() < 40)
                    if (MessageBox.Show("主卡组小于40张，是否保存？", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    {
                        return false;
                    }

                if (deck.MainDeck.Count() > 60)
                    if (MessageBox.Show("主卡组大于60张，是否保存？", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    {
                        return false;
                    }

                if (deck.SideDeck.Count() > 15)
                    if (MessageBox.Show("副卡组大于15张，是否保存？", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    {
                        return false;
                    }

                if (deck.FusionDeck.Count() > 15)
                    if (MessageBox.Show("额外卡组大于15张，是否保存？", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    {
                        return false;
                    }


                bool flag = false;

                //if (opdlg.FileName.EndsWith(".cgi.txt", StringComparison.CurrentCultureIgnoreCase))
                if (opdlg.FilterIndex == 2)
                    flag = deck.SaveFileForCGI(opdlg.FileName);
                else
                    flag = deck.SaveFile(opdlg.FileName);

                if (flag)
                {
                    MessageBox.Show("卡组保存完毕！");
                    this.Text = string.Format("编辑：{0}", deck.DeckName);
                    return true;
                }
                else
                {
                    MessageBox.Show("卡组保存失败！");
                    return false;
                }
            }
            else
                return false;
        }

        private void buttonItem1_Click(object sender, EventArgs e)
        {
            SaveDeck();
        }

        private void dotNetBarManager1_DockTabClosing(object sender, DevComponents.DotNetBar.DockTabClosingEventArgs e)
        {
            buttonItem33_Click(null, null);
            e.Cancel = true;
        }

        private void buttonItem2_PopupFinalized(object sender, EventArgs e)
        {
            buttonItem28.Enabled = true;
            buttonItem29.Enabled = true;
            buttonItem30.Enabled = true;
            buttonItem4.Enabled = true;
            buttonItem5.Enabled = true;
            buttonItem28.Text = "至主卡组";
        }

        private void frmDeckEdit_Activated(object sender, EventArgs e)
        {
            CountCards();
        }

        private void frmDeckEdit_Enter(object sender, EventArgs e)
        {
            CountCards();
        }

        private void buttonItem30_Click(object sender, EventArgs e)
        {
            VirtualListView listViewEx = CurrentListView();
            CardDescription[] Cards = CurrentCards();
            if (listViewEx == null || Cards == null)
                return;
            if (listViewEx.SelectedIndices == null || listViewEx.SelectedIndices.Count == 0)
                return;
            CardDescription card = Cards[listViewEx.SelectedIndices[0]];
            if (listViewEx == listViewEx1)
                RemoveFromMainDeck(listViewEx.SelectedIndices[0]);
            else if (listViewEx == listViewEx2)
                RemoveFromSideDeck(listViewEx.SelectedIndices[0]);
            else if (listViewEx == listViewEx3)
                RemoveFromFusionDeck(listViewEx.SelectedIndices[0]);
            AddToTempDeck(card.ID);
            CountCards();
        }

        private void listViewEx1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Clicks == 2 && e.Button == MouseButtons.Left)
            {
                VirtualListView listViewEx = (VirtualListView)sender;
                ListViewItem item = listViewEx.GetItemAt(e.X, e.Y);
                if (item == null)
                    return;

                int id = int.Parse(item.SubItems[8].Text);
                CardDescription card = CardLibrary.GetInstance().GetCardByID(id);

                if (listViewEx == listViewEx4)
                {
                    if (card.iCardtype == 2 || card.iCardtype == 6)
                    {
                        Global.frmDeckEditHolder.AddToFusionDeck(card.ID);
                    }
                    else
                    {
                        Global.frmDeckEditHolder.AddToMainDeck(card.ID);
                    }
                }
                else if (listViewEx == listViewEx1)
                {
                    Global.frmDeckEditHolder.RemoveFromMainDeck(listViewEx.SelectedIndices[0]);
                    Global.frmDeckEditHolder.AddToSideDeck(card.ID);
                }
                else if (listViewEx == listViewEx2)
                {
                    Global.frmDeckEditHolder.RemoveFromSideDeck(listViewEx.SelectedIndices[0]);
                    Global.frmDeckEditHolder.AddToMainDeck(card.ID);
                }
                else
                {
                    Global.frmDeckEditHolder.AddToFusionDeck(card.ID);
                }
            }
        }

        private void frmDeckEdit_TextChanged(object sender, EventArgs e)
        {
            Global.frmMainHolder.MDIChildTextChanged(this);
        }

        private void frmDeckEdit_FormClosed(object sender, FormClosedEventArgs e)
        {
            Global.frmMainHolder.MDIChildClosed(this);
        }

        private void listViewEx1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("DeckBuilder2.DragCard"))
            {
                DragCard dragcard = (DragCard)e.Data.GetData("DeckBuilder2.DragCard");
                if (dragcard.RemoveFrom == DeckType.None || dragcard.RemoveFrom == DeckType.TempDeck)
                    e.Effect = DragDropEffects.Copy | DragDropEffects.Move;
                else
                    e.Effect = DragDropEffects.Move;
            }
        }


        private void listViewEx1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("DeckBuilder2.DragCard"))
            {
                DragCard dragcard = (DragCard)e.Data.GetData("DeckBuilder2.DragCard");
                VirtualListView listViewEx = (VirtualListView)sender;
                Point p = listViewEx.PointToClient(new Point(e.X, e.Y));
                ListViewItem item = listViewEx.GetNearestItem(p.X, p.Y);
                if (item != null)
                {
                    dragcard.AddIndex = item.Index;

                }
                DoDragDrop(dragcard, GetDeck((VirtualListView)sender));
            }
        }

        private void bar1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("DeckBuilder2.DragCard"))
            {
                DragCard dragcard = (DragCard)e.Data.GetData("DeckBuilder2.DragCard");
                if (dragcard.RemoveFrom == DeckType.None || dragcard.RemoveFrom == DeckType.TempDeck)
                    e.Effect = DragDropEffects.Copy | DragDropEffects.Move;
                else
                    e.Effect = DragDropEffects.Move;
            }
        }

        private void bar1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("DeckBuilder2.DragCard"))
            {
                DragCard dragcard = (DragCard)e.Data.GetData("DeckBuilder2.DragCard");
                DevComponents.DotNetBar.TabStrip tabstrip = (DevComponents.DotNetBar.TabStrip)sender;
                DevComponents.DotNetBar.TabItem currenttab = null;
                Point p = new Point();
                p = tabstrip.PointToClient(new Point(e.X, e.Y));
                foreach (DevComponents.DotNetBar.TabItem tab in tabstrip.Tabs)
                {
                    ;
                    if (tab.DisplayRectangle.Contains(p))
                    {
                        currenttab = tab;
                        break;
                    }
                }

                VirtualListView listview = null;
                if (currenttab != null)
                {
                    switch (currenttab.Text.Substring(0, 3))
                    {
                        case "主卡组":
                            listview = listViewEx1;
                            break;
                        case "副卡组":
                            listview = listViewEx2;
                            break;
                        case "额外卡":
                            listview = listViewEx3;
                            break;
                        case "候选卡":
                            listview = listViewEx4;
                            break;
                    }
                }
                DoDragDrop(dragcard, GetDeck(listview));
            }
        }

        private void dotNetBarManager1_BarTearOff(object sender, EventArgs e)
        {
            if (sender.GetType() == typeof(DevComponents.DotNetBar.Bar))
            {
                DevComponents.DotNetBar.Bar bar = (DevComponents.DotNetBar.Bar)sender;
                bar.ControlAdded += new ControlEventHandler(bar_ControlAdded);

            }
        }

        private void bar_ControlAdded(object sender, ControlEventArgs e)
        {
            if (sender.GetType() == typeof(DevComponents.DotNetBar.Bar))
            {
                DevComponents.DotNetBar.Bar bar = (DevComponents.DotNetBar.Bar)sender;
                if (bar.DockTabControl != null)
                {
                    bar.DockTabControl.AllowDrop = true;
                    bar.DockTabControl.DragEnter += new DragEventHandler(bar1_DragEnter);
                    bar.DockTabControl.DragDrop += new DragEventHandler(bar1_DragDrop);
                }
            }
        }

        public void DoDragDrop(DragCard dragcard, DeckType AddTo)
        {
            CardDescription card = dragcard.Card;
            if ((card.iCardtype == 2 || card.iCardtype == 6) && (AddTo == DeckType.MainDeck || AddTo == DeckType.SideDeck))
                AddTo = DeckType.FusionDeck;
            if (!(card.iCardtype == 2 || card.iCardtype == 6) && AddTo == DeckType.FusionDeck)
                AddTo = dragcard.RemoveFrom;

            if (dragcard.RemoveFrom == AddTo && (dragcard.AddIndex < 0 || dragcard.AddIndex == dragcard.RemoveIndex))
                return;

            if (dragcard.RemoveIndex >= 0)
            {
                switch (dragcard.RemoveFrom)
                {
                    case DeckType.None:
                        break;
                    case DeckType.MainDeck:
                        deck.MainDeck.RemoveCard(dragcard.RemoveIndex);
                        MainCards = deck.MainDeck.GetCards();
                        break;
                    case DeckType.SideDeck:
                        deck.SideDeck.RemoveCard(dragcard.RemoveIndex);
                        SideCards = deck.SideDeck.GetCards();
                        break;
                    case DeckType.FusionDeck:
                        deck.FusionDeck.RemoveCard(dragcard.RemoveIndex);
                        FusionCards = deck.FusionDeck.GetCards();
                        break;
                    case DeckType.TempDeck:
                        if (AddTo == DeckType.None)
                        {
                            deck.TempDeck.RemoveCard(dragcard.RemoveIndex);
                            TempCards = deck.TempDeck.GetCards();
                        }
                        break;
                }
            }
            else if (dragcard.RemoveName != null)
            {
                switch (dragcard.RemoveFrom)
                {
                    case DeckType.None:
                        break;
                    case DeckType.MainDeck:
                        deck.MainDeck.RemoveCardByName(dragcard.RemoveName);
                        MainCards = deck.MainDeck.GetCards();
                        break;
                    case DeckType.SideDeck:
                        deck.SideDeck.RemoveCardByName(dragcard.RemoveName);
                        SideCards = deck.SideDeck.GetCards();
                        break;
                    case DeckType.FusionDeck:
                        deck.FusionDeck.RemoveCardByName(dragcard.RemoveName);
                        FusionCards = deck.FusionDeck.GetCards();
                        break;
                    case DeckType.TempDeck:
                        if (AddTo == DeckType.None)
                        {
                            deck.TempDeck.RemoveCardByName(dragcard.RemoveName);
                            TempCards = deck.TempDeck.GetCards();
                        }
                        break;
                }
            }

            if (dragcard.RemoveFrom != AddTo)
            {
                switch (dragcard.RemoveFrom)
                {
                    case DeckType.None:
                        break;
                    case DeckType.MainDeck:
                        ShowDeckList(MainCards, listViewEx1);
                        break;
                    case DeckType.SideDeck:
                        ShowDeckList(SideCards, listViewEx2);
                        break;
                    case DeckType.FusionDeck:
                        ShowDeckList(FusionCards, listViewEx3);
                        break;
                    case DeckType.TempDeck:
                        if (AddTo == DeckType.None)
                        {
                            ShowDeckList(TempCards, listViewEx4);
                        }
                        break;
                }
            }
              
            
            switch (AddTo)
            {
                case DeckType.None:
                    ShowQuickView();
                    ShowQuickViewCount();
                    break;
                case DeckType.MainDeck:
                    if (card.iCardtype == 2 || card.iCardtype == 6)
                        Global.frmDeckEditHolder.AddToFusionDeck(card.ID, dragcard.AddIndex);
                    else
                        Global.frmDeckEditHolder.AddToMainDeck(card.ID, dragcard.AddIndex);
                    break;
                case DeckType.SideDeck:
                    if (card.iCardtype == 2 || card.iCardtype == 6)
                        Global.frmDeckEditHolder.AddToFusionDeck(card.ID, dragcard.AddIndex);
                    else
                        Global.frmDeckEditHolder.AddToSideDeck(card.ID, dragcard.AddIndex);
                    break;
                case DeckType.FusionDeck:
                    if (card.iCardtype == 2 || card.iCardtype == 6)
                        Global.frmDeckEditHolder.AddToFusionDeck(card.ID, dragcard.AddIndex);
                    else
                        Global.frmDeckEditHolder.AddToMainDeck(card.ID, dragcard.AddIndex);
                    break;
                case DeckType.TempDeck:
                    Global.frmDeckEditHolder.AddToTempDeck(card.ID, dragcard.AddIndex);
                    break;
            }
        }

        private void listViewEx1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (!string.Equals(DB2Config.GetInstance().GetSetting("NoDrag"), "True", StringComparison.OrdinalIgnoreCase) && e.Button == MouseButtons.Left)
            {
                VirtualListView listViewEx = (VirtualListView)sender;
                ListViewItem item = (ListViewItem)e.Item;
                if (item == null)
                    return;

                if (item.Selected == false)
                {
                    item.Selected = true;
                    listViewEx.Update();
                }

                DragCard dragcard = new DragCard();
                int id = int.Parse(item.SubItems[8].Text);
                dragcard.Card = CardLibrary.GetInstance().GetCardByID(id);
                dragcard.FromObject = listViewEx;
                dragcard.RemoveFrom = GetDeck((VirtualListView)sender);
                dragcard.RemoveIndex = item.Index;
                if (dragcard.RemoveFrom == DeckType.TempDeck)
                    listViewEx.DoDragDrop(dragcard, DragDropEffects.Copy | DragDropEffects.Move);
                else
                    listViewEx.DoDragDrop(dragcard, DragDropEffects.Move);
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