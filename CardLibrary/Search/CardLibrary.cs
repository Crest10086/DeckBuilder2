using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using BaseCardLibrary.DataAccess;
using Lucene.Net.Search;
using Lucene.Net.Documents;
using Lucene.Net.QueryParsers;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using BaseCardLibrary.Common;
//using CardXDict;

namespace BaseCardLibrary.Search
{
    public class CardLibrary
    {
        private CardDescription[] Cards = new CardDescription[0];
        private CardDescription[] DiyCards = new CardDescription[0];
        private int maxcardid = 0;
        private static CardLibrary instance = null;
        private static string path = null;
        private static Lucene.Net.Store.Directory indexdir = null;
        private static Lucene.Net.Store.Directory diydir = null;
        private bool allowDIY = false;
        private static bool KeepInMemory = false;
        private Searcher searcher = null;

        public bool AllowDIY
        {
            get
            {
                return allowDIY;
            }
            set
            {
                if (allowDIY != value)
                {
                    allowDIY = value;
                    BuildSearcher();
                }
            }
        }

        private void BuildSearcher()
        {
            if (allowDIY && diydir != null)
            {
                IndexSearcher[] indexsearchers = new IndexSearcher[2];
                indexsearchers[0] = new IndexSearcher(indexdir, true);
                indexsearchers[1] = new IndexSearcher(diydir, true);
                searcher = new MultiSearcher(indexsearchers);
            }
            else
            {
                searcher = new IndexSearcher(indexdir, true);
            }
        }

        public static CardLibrary GetInstance()
        {
            //单实例
            if (instance == null)
            {
                //读取配置
                CLConfig config = CLConfig.GetInstance();

                //获取路径
                path = Global.GetPath();

                //清除多余索引文件
                if (File.Exists(path + "CardIndex\\list.txt"))
                {
                    string[] files = File.ReadAllLines(path + "CardIndex\\list.txt", Encoding.UTF8);

                    foreach (string s in Directory.GetFiles(path + "CardIndex"))
                    {
                        string ss = s.Substring(s.LastIndexOf('\\') + 1);
                        bool inlist = false;
                        foreach (string s2 in files)
                            if (string.Equals(ss, s2, StringComparison.OrdinalIgnoreCase))
                            {
                                inlist = true;
                                break;
                            }

                        if (!(inlist || string.Equals(ss, "list.txt", StringComparison.OrdinalIgnoreCase)))
                            File.Delete(s);
                    }
                }

                //读取主索引
                indexdir = new Lucene.Net.Store.SimpleFSDirectory(new DirectoryInfo(path + "CardIndex"), new Lucene.Net.Store.SimpleFSLockFactory());

                //读取DIY索引
                if (Directory.Exists(path + "DIYCardIndex"))
                {
                    diydir = new Lucene.Net.Store.SimpleFSDirectory(new DirectoryInfo(path + "DIYCardIndex"), new Lucene.Net.Store.SimpleFSLockFactory());
                }

                //是否使用内存索引
                KeepInMemory = string.Equals(config.GetSetting("KeepInMemory"), "true", StringComparison.OrdinalIgnoreCase);
                if (KeepInMemory)
                {
                    indexdir = new Lucene.Net.Store.RAMDirectory(indexdir);

                    if (diydir != null)
                    {
                        diydir = new Lucene.Net.Store.RAMDirectory(diydir);
                    }
                }

                //读取所有卡片信息，建立卡片数据库实例
                LuceneReader Reader = new LuceneReader();
                instance = new CardLibrary(Reader.Read(indexdir));
                if (diydir != null)
                {
                    instance.AddDIYCards(Reader.Read(diydir));
                }

                //建立搜索器实例
                instance.BuildSearcher();
            }

            return instance;
        }

        public CardLibrary(CardDescription[] cards)
        {
            Cards = cards;
            maxcardid = cards[cards.Length - 1].ID;
            UpdateLimitedList(LimitedListManager.GetInstance().SelcetedItem);
        }

        public void AddDIYCards(CardDescription[] cards)
        {
            DiyCards = cards;
        }

        public int GetCount()
        {
            return GetNonDIYCount() + GetDIYCount();
        }

        public int GetDIYCount()
        {
            if (DiyCards == null)
                return 0;
            else
                return DiyCards.Length;
        }

        public int GetNonDIYCount()
        {
            if (Cards == null)
                return 0;
            else
                return Cards.Length;
        }

        public CardDescription[] GetCards()
        {
            return Cards;
        }

        public CardDescription[] GetCards(SortField[] sortFields)
        {
            return Search("", sortFields);
        }

        public CardDescription[] Search(string queryString)
        {
            return Search(queryString, null);
        }

        public CardDescription[] Search(string queryString, SortField[] sortFields)
        {
            try
            {
                if (queryString != null)
                    queryString = queryString.Trim();


                Query query = null;
                if (queryString == null || queryString == "")
                {
                    query = new MatchAllDocsQuery();
                }
                else
                {
                    QueryParser parser = new MultiFieldQueryParser(MyLucene.GetLuceneVersion(), new string[] {"name", "oldName", "shortName", "japName", "enName", "effect", "effectType", "cardType", "tribe", "element", "level", "atk", "def", "aliasList", "package", "infrequence" }, AnalyzerFactory.GetAnalyzer());
                    query = parser.Parse(queryString);
                }

                TopDocs docs = null;

                if (sortFields == null)
                    docs = searcher.Search(query, null, searcher.MaxDoc());
                else
                    docs = searcher.Search(query, null, searcher.MaxDoc(), new Sort(sortFields));

                ScoreDoc[] sdocs = docs.scoreDocs;
                int length = sdocs.Length;
                
                CardDescription[] cards = new CardDescription[length];
                for (int i = 0; i < length; i++)
                {
                    cards[i] = GetCardByIndex(sdocs[i].doc);
                }
                
                return cards;
            }
            catch
            {
                return new CardDescription[0];
            }
        }

        /*
        public CardDescription[] Search2(string queryString, SortField[] sortFields)
        {
            try
            {
                if (queryString != null)
                    queryString = queryString.Trim();


                Query query = null;
                if (queryString == null || queryString == "")
                {
                    query = new MatchAllDocsQuery();
                }
                else
                {
                    BooleanQuery bquery = new BooleanQuery();
                    QueryParser parser = new QueryParser(MyLucene.GetLuceneVersion(), "name", AnalyzerFactory.GetAnalyzer());
                    Query q = parser.Parse(queryString);
                    bquery.Add(q, BooleanClause.Occur.SHOULD);

                    parser = new QueryParser(MyLucene.GetLuceneVersion(), "oldName", AnalyzerFactory.GetAnalyzer());
                    q = parser.Parse(queryString);
                    bquery.Add(q, BooleanClause.Occur.SHOULD);

                    parser = new QueryParser(MyLucene.GetLuceneVersion(), "shortName", AnalyzerFactory.GetAnalyzer());
                    q = parser.Parse(queryString);
                    bquery.Add(q, BooleanClause.Occur.SHOULD);

                    parser = new QueryParser(MyLucene.GetLuceneVersion(), "japName", AnalyzerFactory.GetAnalyzer());
                    q = parser.Parse(queryString);
                    bquery.Add(q, BooleanClause.Occur.SHOULD);

                    parser = new QueryParser(MyLucene.GetLuceneVersion(), "enName", AnalyzerFactory.GetAnalyzer());
                    q = parser.Parse(queryString);
                    bquery.Add(q, BooleanClause.Occur.SHOULD);

                    query = bquery;
                }

                Searcher searcher = null;
                if (allowDIY && diydir != null)
                {
                    IndexSearcher[] indexsearchers = new IndexSearcher[2];
                    indexsearchers[0] = new IndexSearcher(indexdir, true);
                    indexsearchers[1] = new IndexSearcher(diydir, true);
                    searcher = new MultiSearcher(indexsearchers);
                }
                else
                {
                    searcher = new IndexSearcher(indexdir, true);
                }

                Hits hits = null;

                if (sortFields == null)
                    hits = searcher.Search(query);
                else
                    hits = searcher.Search(query, new Sort(sortFields));

                int length = hits.Length();
                CardDescription[] cards = new CardDescription[length];
                for (int i = 0; i < length; i++)
                {
                    Document doc = hits.Doc(i);
                    cards[i] = GetCardByID(int.Parse(doc.GetField("ID").StringValue()));
                }

                return cards;
            }
            catch
            {
                return new CardDescription[0];
            }
        }
         */ 

        public CardDescription GetCardByIndex(int index)
        {
            if (index >= Cards.Length)
            {
                if (index >= Cards.Length + DiyCards.Length)
                    return null;
                else
                    return DiyCards[index - Cards.Length];
            }
            else if (index >= 0)
            {
                return Cards[index];
            }
            else
                return null;
        }

        public CardDescription GetCardByID(int id)
        {
            if (id <= 0)
                return null;

            if (!IsDIYCard(id))
            {
                int count = Cards.Length;
                for (int i = 0; i < count; i++)
                {
                    CardDescription card = Cards[i];
                    if (Cards[i].ID == id)
                        return card;
                }
                return null;
            }
            else
            {
                if (!AllowDIY)
                    return null;

                int newid = id - 60000;
                if (newid >= 0 && newid < DiyCards.Length)
                    return DiyCards[newid];
                else
                    return null;
            }
        }

        private CardDescription GetCardByName(string name, CardDescription[] cards)
        {
            int count = cards.Length;
            for (int i = 0; i < count; i++)
            {
                CardDescription card = cards[i];
                if (string.Equals(name, card.name))
                    return card;
            }

            return null;
        }

        public CardDescription GetCardByName(string name)
        {
            CardDescription card = GetCardByName(name, Cards);

            if (card == null && AllowDIY)
            {
                card = GetCardByName(name, DiyCards);
            }

            return card;
        }

        public CardDescription GetCardByJapName(string japname, CardDescription[] cards)
        {
            string jname = japname.Replace('-', '－');

            int count = cards.Length;
            for (int i = 0; i < count; i++)
            {
                CardDescription card = cards[i];
                if (string.Equals(jname, card.japName.Replace('-', '－')))
                    return card;
            }

            return null;
        }

        public CardDescription GetCardByJapName(string japname)
        {
            CardDescription card = GetCardByJapName(japname, Cards);

            if (card == null && AllowDIY)
            {
                card = GetCardByJapName(japname, DiyCards);
            }

            return card;
        }

        public CardDescription GetCardByEnName(string enname, CardDescription[] cards)
        {
            int count = cards.Length;
            for (int i = 0; i < count; i++)
            {
                CardDescription card = cards[i];
                if (string.Equals(enname, card.enName))
                    return card;
            }

            return null;
        }

        public CardDescription GetCardByEnName(string enname)
        {
            CardDescription card = GetCardByEnName(enname, Cards);

            if (card == null && AllowDIY)
            {
                card = GetCardByEnName(enname, DiyCards);
            }

            return card;
        }

        public CardDescription GetCardByOldName(string oldname)
        {
            int count = Cards.Length;
            for (int i = 0; i < count; i++)
            {
                CardDescription card = Cards[i];

                if (string.Equals(oldname, card.oldName))
                    return card;

                if (card.oldName.IndexOf(oldname) >= 0)
                {
                    string[] names = card.oldName.Split(',', '，');
                    foreach (string s in names)
                    {
                        if (s.Trim() == oldname.Trim())
                            return card;
                    }
                }
            }

            return null;
        }

        public CardDescription GetCardByShortName(string shortname)
        {
            int count = Cards.Length;
            for (int i = 0; i < count; i++)
            {
                CardDescription card = Cards[i];

                if (string.Equals(shortname, card.shortName))
                    return card;

                if (card.shortName.IndexOf(shortname) >= 0)
                {
                    string[] names = card.shortName.Split(',', '，');
                    foreach (string s in names)
                    {
                        if (s.Trim() == shortname.Trim())
                            return card;
                    }
                }
            }

            return null;
        }

        public CardDescription GetCardByCheatCode(string cheatcode)
        {
            string code = cheatcode.PadLeft(8, '0');

            int count = Cards.Length;

            for (int i = 0; i < count; i++)
            {
                CardDescription card = Cards[i];
                if (card.cheatcode.Equals(code))
                    return card;
            }

            for (int i = 0; i < count; i++)
            {
                CardDescription card = Cards[i];
                if (card.cheatcode.Contains(code))
                    return card;
            }

            return null;
        }

        public bool IsExistByCheatCode(string cheatcode)
        {
            string code = cheatcode.PadLeft(8, '0');

            int count = Cards.Length;

            for (int i = 0; i < count; i++)
            {
                CardDescription card = Cards[i];
                if (card.cheatcode.Equals(code))
                    return true;
            }

            return false;
        }

        public bool IsDIYCard(int ID)
        {
            return ID > maxcardid;
        }

        //根据卡片ID返回卡片序号
        //注意：该函数不保证返回的卡片序号有效
        public int GetCardIndexByID(int id)
        {
            if (IsDIYCard(id))
            {
                return id + Cards.Length - 60000;
            }
            else
            {
                int count = Cards.Length;
                for (int i = 0; i < count; i++)
                {
                    CardDescription card = Cards[i];
                    if (Cards[i].ID == id)
                        return i;
                }
                return -1;
            }
        }

        //更新禁卡表
        public bool UpdateLimitedList(CardLimitedList limitedlist)
        {
            if (limitedlist == null)
                return false;

            for (int i = 0; i < Cards.Length; i++)
                Cards[i].limit = 3;

            string[] ss = limitedlist.ForbiddenList.Split(',');
            for (int i = 0; i < ss.Length; i++)
            {
                //狐查的ID号比图查要多1
                int id = int.Parse(ss[i]) - 1;
                SetLimit(id, 0);
            }

            ss = limitedlist.LimitedList.Split(',');
            for (int i = 0; i < ss.Length; i++)
            {
                int id = int.Parse(ss[i]) - 1;
                SetLimit(id, 1);
            }

            ss = limitedlist.SemiLimitedList.Split(',');
            for (int i = 0; i < ss.Length; i++)
            {
                int id = int.Parse(ss[i]) - 1;
                SetLimit(id, 2);
            }

            return true;
        }

        private bool SetLimit(int id, int limit)
        {
            for (int i = 0; i < Cards.Length; i++)
            {
                CardDescription card = Cards[i];
                if (card.ID == id)
                {
                    card.limit = limit;
                    return true;
                }
            }

            return false;
        }
    }
}
