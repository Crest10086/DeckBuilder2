using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Lucene.Net.Search;
using Lucene.Net.Documents;
using BaseCardLibrary.DataAccess;
using BaseCardLibrary.Search;
using Tools;

namespace BaseCardLibrary.Search
{
    public class Card
    {
        public int ID = 0;
        public int Index = 0;
        public string String1 = "";

        public Card(int id, int index)
        {
            ID = id;
            Index = index;
        }
    }

    class CardCompare : System.Collections.IComparer
    {
        Hashtable CardOrder = null;

        public CardCompare(Hashtable cardorder)
        {
            CardOrder = cardorder;
        }

        public int Compare(object x, object y)
        {
            return (int)CardOrder[((Card)x).ID] - (int)CardOrder[((Card)y).ID];
        }


    } 


    public class DeckList
    {
        ArrayList decklist = null;
        Hashtable cardcount = null;
        CardLibrary cardLibrary = CardLibrary.GetInstance();
        Hashtable CardOrder = new Hashtable(CardLibrary.GetInstance().GetCount() + 1);
        int Index = 0;
        int LimitNum = 3;
        int Capacity = 0;
        public bool isChanged = false;

        public DeckList()
        {
            Capacity = 0;
            decklist = new ArrayList();
            cardcount = new Hashtable();
        }

        public DeckList(int maxcapacity)
        {
            Capacity = maxcapacity;
            decklist = new ArrayList(maxcapacity);
            cardcount = new Hashtable(maxcapacity);
        }

        public int Count()
        {
            return decklist.Count;
        }

        public string AddCard(int id)
        {
            return AddCard(id, -1);
        }

        public string AddCard(int id, int index)
        {
            if (decklist.Count >= Capacity && Capacity > 0)
            {
                return "卡组已满！";
            }

            int count = 0;
            if (cardcount[id] != null)
            {
                count = (int)cardcount[id];
            }
            CardDescription card = cardLibrary.GetCardByID(id);
            if (!string.Equals(CLConfig.GetInstance().GetSetting("AllowForbiddenCard"), "True", StringComparison.OrdinalIgnoreCase))
                LimitNum = card.limit;
            if (count >= LimitNum)
            {
                return string.Format("卡片[{0}]超出限制数量！", card.name);
            }

            if (index >= 0 && index < decklist.Count)
                decklist.Insert(index, new Card(id, Index++));
            else
                decklist.Add(new Card(id, Index++));
            cardcount[id] = ++count;
            isChanged = true;

            return null;
        }

        public bool RemoveCard(int index)
        {
            Card card = (Card)decklist[index];
            if (card == null)
                return false;

            cardcount[card.ID] = ((int)cardcount[card.ID] - 1);
            decklist.RemoveAt(index);
            isChanged = true;

            return true;
        }

        public bool RemoveCardByID(int id)
        {
            for (int i = 0; i < decklist.Count; i++)
            {
                Card card = (Card)decklist[i];
                if (card == null)
                    return false;
                if (card.ID == id)
                    return RemoveCard(i);
            }
            return false;
        }

        public bool RemoveCardByName(string name)
        {
            for (int i = 0; i < decklist.Count; i++)
            {
                Card card = (Card)decklist[i];
                if (card == null)
                    return false;
                if (cardLibrary.GetCardByID(card.ID).name == name)
                    return RemoveCard(i);
            }
            return false;
        }

        public CardDescription[] GetCardList()
        {
            if (cardcount.Count > 0)
            {
                bool isnull = true;

                BooleanQuery query = new BooleanQuery();
                foreach (DictionaryEntry d in cardcount)
                {
                    int count = (int)d.Value;
                    if (count > 0)
                    {
                        Lucene.Net.Index.Term term = new Lucene.Net.Index.Term("ID", d.Key.ToString());
                        TermQuery tq = new TermQuery(term);
                        query.Add(tq, BooleanClause.Occur.SHOULD);
                        isnull = false;
                    }
                }

                if (isnull)
                {
                    return new CardDescription[0];
                }
                else
                {
                    MySorter sorter = new MySorter();
                    sorter.AddField("cardType2", SortField.STRING, false);
                    sorter.AddField("atkValue", SortField.INT);
                    sorter.AddField("level", SortField.INT);
                    CardDescription[] result = cardLibrary.Search(query.ToString(), sorter.GetSortFields());

                    return result;
                }
            }
            else
            {
                return new CardDescription[0];
            }
        }

        public void Sort(SortField[] sortFields)
        {
            if (cardcount.Count <= 0)
                return;

            BooleanQuery query = new BooleanQuery();
            foreach (DictionaryEntry d in cardcount)
            {
                int count = (int)d.Value;
                if (count > 0)
                {
                    Lucene.Net.Index.Term term = new Lucene.Net.Index.Term("ID", d.Key.ToString());
                    TermQuery tq = new TermQuery(term);
                    query.Add(tq, BooleanClause.Occur.SHOULD);
                }
            }
            CardDescription[] result = cardLibrary.Search(query.ToString(), sortFields);

            int length = result.Length;

            for (int i = 0; i < length; i++)
            {
                int id = result[i].ID;
                CardOrder[id] = i;
            }

            CardCompare cc = new CardCompare(CardOrder);
            decklist.Sort(cc);
        }

        public CardDescription[] GetCards()
        {
            CardDescription[] cards = new CardDescription[decklist.Count];
            for (int i = 0; i < decklist.Count; i++)
            {
                Card card = (Card)decklist[i];
                cards[i] = cardLibrary.GetCardByID(card.ID);
            }

            return cards;
        }

        public Card[] GetList()
        {
            return (Card[])decklist.ToArray(typeof(Card));
        }

        public void SetString1(int index, string string1)
        {
            Card card = (Card)decklist[index];
            card.String1 = string1;
        }

        public int GetCount(int id)
        {
            if (cardcount[id] == null)
                return 0;
            else
                return (int)cardcount[id];
        }

        public bool isChange()
        {
            return isChanged;
        }
    }

    public class Deck
    {
        public DeckList MainDeck = null;
        public DeckList SideDeck = null;
        public DeckList FusionDeck = null;
        public DeckList TempDeck = null;
        public string DeckName = null;

        public Deck()
        {
            Clear();
        }

        public void Clear()
        {
            MainDeck = new DeckList(100);
            SideDeck = new DeckList(100);
            FusionDeck = new DeckList(100);
            TempDeck = new DeckList();
            MainDeck.isChanged = false;
            SideDeck.isChanged = false;
            FusionDeck.isChanged = false;
            TempDeck.isChanged = false;
            DeckName = "未命名";
        }

        public void Sort(SortField[] sortFields)
        {
            MainDeck.Sort(sortFields);
            SideDeck.Sort(sortFields);
            FusionDeck.Sort(sortFields);
            TempDeck.Sort(sortFields);
        }

        public int GetCount(int id)
        {
            return MainDeck.GetCount(id) + SideDeck.GetCount(id) + FusionDeck.GetCount(id);
        }

        public bool isFull(int id)
        {
            CardDescription card = CardLibrary.GetInstance().GetCardByID(id);
            int limit = card.limit;
            string s = CLConfig.GetInstance().GetSetting("AllowForbiddenCard");
            if (string.Equals(s, "True", StringComparison.OrdinalIgnoreCase))
                limit = 3;
            return GetCount(id) >= limit;
        }

        public bool isChange()
        {
            return MainDeck.isChanged || SideDeck.isChanged || FusionDeck.isChanged || TempDeck.isChanged;
        }

        public bool isEmpty()
        {
            return MainDeck.Count() + SideDeck.Count() + FusionDeck.Count() + TempDeck.Count() == 0;
        }

        public ArrayList LoadFileByCharSet(string FileName, string CharSet)
        {
            try
            {
                Clear();

                StreamReader sr = new StreamReader(FileName, System.Text.Encoding.GetEncoding(CharSet));
                int lastNumber = 0;
                int currentDeck = 0;
                ArrayList lastList = new ArrayList();

                while (!sr.EndOfStream)
                {
                    string s = sr.ReadLine().Trim();
                    if (s.StartsWith("===="))
                    {
                        currentDeck = 1;
                        continue;
                    }
                    else if (s.StartsWith("####"))
                    {
                        currentDeck = 2;
                        continue;
                    }
                    else if (s.StartsWith("$$$$"))
                    {
                        currentDeck = 3;
                        continue;
                    }
                    else if (s.Length == 0)
                        continue;

                    CardDescription card = null;
                    string lastname = null;
                    if (s[0] == '[')
                    {
                        string name = s.Substring(s.LastIndexOf('[') + 1, s.LastIndexOf(']') - s.LastIndexOf('[') - 1);
                        lastname = name;
                        card = CardLibrary.GetInstance().GetCardByName(name);
                        if (card == null)
                            card = CardLibrary.GetInstance().GetCardByOldName(name);
                    }
                    else
                    {
                        string japname = CharacterSet.JPSBCToDBC(s);
                        lastname = japname;
                        card = CardLibrary.GetInstance().GetCardByJapName(japname);
                    }

                    if (card != null)
                    {
                        string err = null;
                        switch (currentDeck)
                        {
                            case 0:
                                if (card.iCardtype == 2 || card.iCardtype == 6)
                                    err = FusionDeck.AddCard(card.ID);
                                else
                                    err = MainDeck.AddCard(card.ID);
                                break;
                            case 1:
                                err = FusionDeck.AddCard(card.ID);
                                break;
                            case 2:
                                err = SideDeck.AddCard(card.ID);
                                break;
                            case 3:
                                err = TempDeck.AddCard(card.ID);
                                break;
                        }
                        if (err != null)
                        {
                            lastNumber++;
                            lastList.Add(card.name);
                        }
                    }
                    else
                    {
                        lastNumber++;
                        lastList.Add(lastname);
                    }
                }

                sr.Close();
                DeckName = Regex.Replace(FileName, @".[^.]*$", "");
                DeckName = Regex.Replace(DeckName, @"^.*\\", "");
                MainDeck.isChanged = false;
                SideDeck.isChanged = false;
                FusionDeck.isChanged = false;
                TempDeck.isChanged = false;

                return lastList;
            }
            catch
            {
                Clear();
                return null;
            }
        }
            
        public string LoadFile(string FileName)
        {
            try
            {
                string cs = CharacterSet.GetCharSet(FileName);
                if (cs == null)
                    cs = "gb2312";

                ArrayList lastList = LoadFileByCharSet(FileName, cs);

                if (cs != "gb2312" && (lastList == null || lastList.Count > 20))
                    lastList = LoadFileByCharSet(FileName, "gb2312");

                if (lastList == null || lastList.Count > 20)
                    lastList = LoadFileByCharSet(FileName, "Unicode");

                if (lastList == null)
                    return "卡组读入失败！";

                int lastNumber = lastList.Count;
                if (lastNumber > 0)
                {
                    string s = lastNumber.ToString() + "张卡片读入失败！\n\r";
                    for (int i = 0; i < lastList.Count; i++)
                    {
                        string ss = (string)lastList[i];
                        s += "\n\r[" + ss + "]";
                    }
                    return s;
                }

                return null;
            }
            catch
            {
                Clear();
                return "卡组读入失败！";
            }
        }

        public bool SaveFile(string filename)
        {
            try
            {
                StreamWriter sw = new StreamWriter(filename, false, System.Text.Encoding.GetEncoding("gb2312"));
                CardLibrary cardLibrary = CardLibrary.GetInstance();

                Card[] Cards = MainDeck.GetList();
                foreach (Card card in Cards)
                {
                    CardDescription cd = cardLibrary.GetCardByID(card.ID);
                    sw.WriteLine("[" + cd.name + "]");
                }

                Cards = SideDeck.GetList();
                if (Cards.Length > 0)
                    sw.WriteLine("####");
                foreach (Card card in Cards)
                {
                    CardDescription cd = cardLibrary.GetCardByID(card.ID);
                    sw.WriteLine("[" + cd.name + "]");
                }

                Cards = FusionDeck.GetList();
                if (Cards.Length > 0)
                    sw.WriteLine("====");
                foreach (Card card in Cards)
                {
                    CardDescription cd = cardLibrary.GetCardByID(card.ID);
                    sw.WriteLine("[" + cd.name + "]");
                }

                Cards = TempDeck.GetList();
                if (Cards.Length > 0)
                    sw.WriteLine("$$$$");
                foreach (Card card in Cards)
                {
                    CardDescription cd = cardLibrary.GetCardByID(card.ID);
                    sw.WriteLine("[" + cd.name + "]");
                }

                sw.Close();

                DeckName = Regex.Replace(filename, @".[^.]*$", "");
                DeckName = Regex.Replace(DeckName, @"^.*\\", "");
                MainDeck.isChanged = false;
                SideDeck.isChanged = false;
                FusionDeck.isChanged = false;
                TempDeck.isChanged = false;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SaveFileForCGI(string filename)
        {
            try
            {
                StreamWriter sw = new StreamWriter(filename, false, System.Text.Encoding.Unicode);
                CardLibrary cardLibrary = CardLibrary.GetInstance();

                Card[] Cards = MainDeck.GetList();
                foreach (Card card in Cards)
                {
                    CardDescription cd = cardLibrary.GetCardByID(card.ID);
                    string s = CharacterSet.JPDBCToSBC(cd.japName);
                    sw.WriteLine(s);
                }

                Cards = SideDeck.GetList();
                if (Cards.Length > 0)
                    sw.WriteLine("####");
                foreach (Card card in Cards)
                {
                    CardDescription cd = cardLibrary.GetCardByID(card.ID);
                    sw.WriteLine(CharacterSet.JPDBCToSBC(cd.japName));
                }

                Cards = FusionDeck.GetList();
                if (Cards.Length > 0)
                    sw.WriteLine("====");
                foreach (Card card in Cards)
                {
                    CardDescription cd = cardLibrary.GetCardByID(card.ID);
                    sw.WriteLine(CharacterSet.JPDBCToSBC(cd.japName));
                }

                Cards = TempDeck.GetList();
                if (Cards.Length > 0)
                    sw.WriteLine("$$$$");
                foreach (Card card in Cards)
                {
                    CardDescription cd = cardLibrary.GetCardByID(card.ID);
                    sw.WriteLine(CharacterSet.JPDBCToSBC(cd.japName));
                }

                sw.Close();

                DeckName = Regex.Replace(filename, @".[^.]*$", "");
                DeckName = Regex.Replace(DeckName, @"^.*\\", "");
                MainDeck.isChanged = false;
                SideDeck.isChanged = false;
                FusionDeck.isChanged = false;
                TempDeck.isChanged = false;
                return true;
            }
            catch
            {
                return false;
            }
        }


        public string GetDeckCamp()
        {
            CardDescription[] MainCards = MainDeck.GetCards();
            CardDescription[] SideCards = SideDeck.GetCards();
            CardDescription[] FusionCards = FusionDeck.GetCards();

            int ocg = 0;
            int tcg = 0;
            int diy = 0;

            foreach (CardDescription card in MainCards)
            {
                switch (card.cardCamp)
                {
                    case CardCamp.OCG:
                        ocg++;
                        break;
                    case CardCamp.TCG:
                        tcg++;
                        break;
                    case CardCamp.DIY:
                        diy++;
                        break;
                }
            }

            foreach (CardDescription card in SideCards)
            {
                switch (card.cardCamp)
                {
                    case CardCamp.OCG:
                        ocg++;
                        break;
                    case CardCamp.TCG:
                        tcg++;
                        break;
                    case CardCamp.DIY:
                        diy++;
                        break;
                }
            }

            foreach (CardDescription card in FusionCards)
            {
                switch (card.cardCamp)
                {
                    case CardCamp.OCG:
                        ocg++;
                        break;
                    case CardCamp.TCG:
                        tcg++;
                        break;
                    case CardCamp.DIY:
                        diy++;
                        break;
                }
            }

            if (diy > 0)
                return "DIY卡组";
            else if (ocg > 0)
            {
                if (tcg > 0)
                    return "OT混用卡组";
                else
                    return "OCG专用卡组";
            }
            else
            {
                if (tcg > 0)
                    return "TCG专用卡组";
                else
                    return "标准卡组";
            }
        }
    }
}
