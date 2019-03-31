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
using MyTools;

namespace BaseCardLibrary.Search
{
    public class Card
    {
        public int ID = 0;
        public int Index = 0;
        public string Text = "";

        public Card(int id, int index, string text)
        {
            ID = id;
            Index = index;
            Text = text;
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
        MyRandom rand = null;
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
            return AddCard(id, index, "");
        }

        public string AddCard(int id, int index, string text)
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
                decklist.Insert(index, new Card(id, Index++, text));
            else
                decklist.Add(new Card(id, Index++, text));
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
                    sorter.AddField("cardType2", SortField.INT, false, false);
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

        public int GetCount(int id)
        {
            if (cardcount[id] == null)
                return 0;
            else
                return (int)cardcount[id];
        }

        public int GetRuleCount(int id)
        {
            CardDescription card = cardLibrary.GetCardByID(id);
            if (card == null)
                return 0;

            int count = 0;
            int len = decklist.Count;
            for (int i = 0; i < len; i++)
            {
                Card cc = (Card)decklist[i];
                CardDescription card2 = cardLibrary.GetCardByID(cc.ID);
                if (string.Equals(card.cheatcode, card2.cheatcode) || (card.aliasList != "" && string.Equals(card.aliasList, card2.aliasList, StringComparison.Ordinal)))
                    count++;
            }

            return count;
        }

        public bool isChange()
        {
            return isChanged;
        }

        public void Shuffle()
        {
            //if (rand == null)
            //    rand = new MyRandom();

            for (int i = 0; i < decklist.Count; i++)
            {
                if (i % 10 == 0)
                    rand = new MyRandom();
                int j = rand.Next(i, decklist.Count);
                if (i != j)
                {
                    object o = decklist[i];
                    decklist[i] = decklist[j];
                    decklist[j] = o;
                }
            }
        }
    }

    public class Deck
    {


        private DeckList maindeck = null;
        private DeckList sidedeck = null;
        private DeckList fusiondeck = null;
        private DeckList tempdeck = null;
        private string deckname = null;
        private string filename = null;

        public DeckList MainDeck
        {
            get
            {
                return maindeck;
            }
        }

        public DeckList SideDeck
        {
            get
            {
                return sidedeck;
            }
        }

        public DeckList ExtraDeck
        {
            get
            {
                return fusiondeck;
            }
        }

        public DeckList TempDeck
        {
            get
            {
                return tempdeck;
            }
        }

        public string DeckName
        {
            get
            {
                return deckname;
            }
        }

        public string FileName
        {
            get
            {
                return filename;
            }
        }

        public Deck()
        {
            Clear();
        }

        public void Clear()
        {
            maindeck = new DeckList(100);
            sidedeck = new DeckList(100);
            fusiondeck = new DeckList(100);
            tempdeck = new DeckList();
            MainDeck.isChanged = false;
            SideDeck.isChanged = false;
            ExtraDeck.isChanged = false;
            TempDeck.isChanged = false;
            deckname = "未命名";
        }

        public void Sort(SortField[] sortFields)
        {
            MainDeck.Sort(sortFields);
            SideDeck.Sort(sortFields);
            ExtraDeck.Sort(sortFields);
            TempDeck.Sort(sortFields);
        }

        public int GetCount(int id)
        {
            return MainDeck.GetCount(id) + SideDeck.GetCount(id) + ExtraDeck.GetCount(id);
        }

        public int GetRuleCount(int id)
        {
            return MainDeck.GetRuleCount(id) + SideDeck.GetRuleCount(id) + ExtraDeck.GetRuleCount(id);
        }

        public bool isFull(int id)
        {
            CardDescription card = CardLibrary.GetInstance().GetCardByID(id);
            int limit = card.limit;
            string s = CLConfig.GetInstance().GetSetting("AllowForbiddenCard");
            if (string.Equals(s, "True", StringComparison.OrdinalIgnoreCase))
                limit = 3;
            return GetRuleCount(id) >= limit;
        }

        public bool isChange()
        {
            return MainDeck.isChanged || SideDeck.isChanged || ExtraDeck.isChanged || TempDeck.isChanged;
        }

        public bool isEmpty()
        {
            return MainDeck.Count() + SideDeck.Count() + ExtraDeck.Count() + TempDeck.Count() == 0;
        }

        static Regex regex1 = new Regex(@"\[(?<name>.*?)\](\#(?<text>.*?)\#)?", RegexOptions.Compiled);
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
                    string text = "";
                    string lastname = null;
                    if (s[0] == '[')
                    {
                        Match match = regex1.Match(s);
                        string name = match.Groups["name"].Value;
                        text = match.Groups["text"].Value;
                        //string name = s.Substring(s.LastIndexOf('[') + 1, s.LastIndexOf(']') - s.LastIndexOf('[') - 1);
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
                                if (CardDescription.isExtraCard(card))
                                    err = ExtraDeck.AddCard(card.ID, -1, text);
                                else
                                    err = MainDeck.AddCard(card.ID, -1, text);
                                break;
                            case 1:
                                err = ExtraDeck.AddCard(card.ID, -1, text);
                                break;
                            case 2:
                                err = SideDeck.AddCard(card.ID, -1, text);
                                break;
                            case 3:
                                err = TempDeck.AddCard(card.ID, -1, text);
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
                deckname = Regex.Replace(FileName, @".[^.]*$", "");
                deckname = Regex.Replace(deckname, @"^.*\\", "");
                MainDeck.isChanged = false;
                SideDeck.isChanged = false;
                ExtraDeck.isChanged = false;
                TempDeck.isChanged = false;

                return lastList;
            }
            catch
            {
                Clear();
                return null;
            }
        }

        public ArrayList LoadFileByCharSetForPRO(string FileName, string CharSet)
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
                    if (s.StartsWith("!side"))
                    {
                        currentDeck = 2;
                        continue;
                    }
                    else if (s.StartsWith("#wait list"))
                    {
                        currentDeck = 3;
                        continue;
                    }
                    else if (s.Length == 0 || s.StartsWith("#") || s.StartsWith("!"))
                        continue;

                    CardDescription card = null;
                    string lastname = s;
                    if (currentDeck == 3)
                        lastname = s.Substring(1);
                    card = CardLibrary.GetInstance().GetCardByCheatCode(lastname);
                    string text = null;
                    

                    if (card != null)
                    {
                        string err = null;
                        switch (currentDeck)
                        {
                            case 0:
                                if (CardDescription.isExtraCard(card))
                                    err = ExtraDeck.AddCard(card.ID, -1, text);
                                else
                                    err = MainDeck.AddCard(card.ID, -1, text);
                                break;
                            case 1:
                                err = ExtraDeck.AddCard(card.ID, -1, text);
                                break;
                            case 2:
                                err = SideDeck.AddCard(card.ID, -1, text);
                                break;
                            case 3:
                                err = TempDeck.AddCard(card.ID, -1, text);
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
                deckname = Regex.Replace(FileName, @".[^.]*$", "");
                deckname = Regex.Replace(deckname, @"^.*\\", "");
                MainDeck.isChanged = false;
                SideDeck.isChanged = false;
                ExtraDeck.isChanged = false;
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
            filename = FileName;

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

        public string LoadFileForPRO(string FileName)
        {
            filename = FileName;

            try
            {

                ArrayList lastList = LoadFileByCharSetForPRO(FileName, "ascii");

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
            this.filename = filename;

            try
            {
                StreamWriter sw = new StreamWriter(filename, false, System.Text.Encoding.GetEncoding("gb2312"));
                CardLibrary cardLibrary = CardLibrary.GetInstance();

                Card[] Cards = MainDeck.GetList();
                foreach (Card card in Cards)
                {
                    CardDescription cd = cardLibrary.GetCardByID(card.ID);
                    if (card.Text.Length > 0)
                        sw.WriteLine("[" + cd.name + "]#" + card.Text + "#");
                    else
                        sw.WriteLine("[" + cd.name + "]");
                }

                Cards = SideDeck.GetList();
                if (Cards.Length > 0)
                    sw.WriteLine("####");
                foreach (Card card in Cards)
                {
                    CardDescription cd = cardLibrary.GetCardByID(card.ID);
                    if (card.Text.Length > 0)
                        sw.WriteLine("[" + cd.name + "]#" + card.Text + "#");
                    else
                        sw.WriteLine("[" + cd.name + "]");
                }

                Cards = ExtraDeck.GetList();
                if (Cards.Length > 0)
                    sw.WriteLine("====");
                foreach (Card card in Cards)
                {
                    CardDescription cd = cardLibrary.GetCardByID(card.ID);
                    if (card.Text.Length > 0)
                        sw.WriteLine("[" + cd.name + "]#" + card.Text + "#");
                    else
                        sw.WriteLine("[" + cd.name + "]");
                }

                Cards = TempDeck.GetList();
                if (Cards.Length > 0)
                    sw.WriteLine("$$$$");
                foreach (Card card in Cards)
                {
                    CardDescription cd = cardLibrary.GetCardByID(card.ID);
                    if (card.Text.Length > 0)
                        sw.WriteLine("[" + cd.name + "]#" + card.Text + "#");
                    else
                        sw.WriteLine("[" + cd.name + "]");
                }

                sw.Close();

                deckname = Regex.Replace(filename, @".[^.]*$", "");
                deckname = Regex.Replace(deckname, @"^.*\\", "");
                MainDeck.isChanged = false;
                SideDeck.isChanged = false;
                ExtraDeck.isChanged = false;
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

                Cards = ExtraDeck.GetList();
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

                deckname = Regex.Replace(filename, @".[^.]*$", "");
                deckname = Regex.Replace(deckname, @"^.*\\", "");
                MainDeck.isChanged = false;
                SideDeck.isChanged = false;
                ExtraDeck.isChanged = false;
                TempDeck.isChanged = false;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SaveFileForPRO(string filename)
        {

            try
            {
                StreamWriter sw = new StreamWriter(filename, false, System.Text.Encoding.ASCII);
                CardLibrary cardLibrary = CardLibrary.GetInstance();

                sw.WriteLine("#created by DeckBuilder2");

                //输出主卡组
                Card[] Cards = MainDeck.GetList();
                sw.WriteLine("#main");
                foreach (Card card in Cards)
                {
                    CardDescription cd = cardLibrary.GetCardByID(card.ID);
                    sw.WriteLine(cd.cheatcode);
                }

                //输出额外卡组
                Cards = ExtraDeck.GetList();
                if (Cards.Length > 0)
                    sw.WriteLine("#extra");
                foreach (Card card in Cards)
                {
                    CardDescription cd = cardLibrary.GetCardByID(card.ID);
                    sw.WriteLine(cd.cheatcode);
                }

                //输出副卡组
                Cards = SideDeck.GetList();
                if (Cards.Length > 0)
                    sw.WriteLine("!side");
                foreach (Card card in Cards)
                {
                    CardDescription cd = cardLibrary.GetCardByID(card.ID);
                    sw.WriteLine(cd.cheatcode);
                }

                //输出候选卡表
                Cards = TempDeck.GetList();
                if (Cards.Length > 0)
                    sw.WriteLine("#wait list");
                foreach (Card card in Cards)
                {
                    CardDescription cd = cardLibrary.GetCardByID(card.ID);
                    sw.WriteLine("#" + cd.cheatcode);
                }

                sw.Close();

                deckname = Regex.Replace(filename, @".[^.]*$", "");
                deckname = Regex.Replace(deckname, @"^.*\\", "");
                MainDeck.isChanged = false;
                SideDeck.isChanged = false;
                ExtraDeck.isChanged = false;
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
            CardDescription[] FusionCards = ExtraDeck.GetCards();

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

        public void Shuffle()
        {
            MainDeck.Shuffle();
        }
    }
}
