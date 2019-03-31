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
//using CardXDict;

namespace BaseCardLibrary.Search
{
    public class CardLibrary
    {
        private CardDescription[] Cards = null;
        private static CardLibrary instance = null;
        private static string Path = null;
        private static Lucene.Net.Store.RAMDirectory ramdir = null;
        private static Lucene.Net.Store.RAMDirectory ramdiydir = null;
        private int NonDIYCount = 0;
        private Hashtable DIYMap = null;
        private static bool allowDIY = false;
        private static bool KeepInMemory = false;

        public static bool AllowDIY
        {
            get
            {
                return allowDIY;
            }
            set
            {
                allowDIY = value;
            }
        }

        public static CardLibrary GetInstance()
        {
            if (instance == null)
            {
                CLConfig config = CLConfig.GetInstance();
                Path = System.AppDomain.CurrentDomain.BaseDirectory;
                KeepInMemory = string.Equals(config.GetSetting("KeepInMemory"), "true", StringComparison.OrdinalIgnoreCase);
                if (KeepInMemory)
                {
                    ramdir = new Lucene.Net.Store.RAMDirectory(Path + "CardIndex");
                    CardsReader Reader = new LuceneReader();
                    instance = new CardLibrary(Reader.Read(ramdir));
                    if (Directory.Exists(Path + "DIYCardIndex"))
                    {
                        ramdiydir = new Lucene.Net.Store.RAMDirectory(Path + "DIYCardIndex");
                        instance.AddDIYCards(Reader.Read(ramdiydir));
                    }
                }
                else
                {
                    CardsReader Reader = new LuceneReader();
                    instance = new CardLibrary(Reader.Read(Path + "CardIndex"));
                    if (Directory.Exists(Path + "DIYCardIndex"))
                    {
                        instance.AddDIYCards(Reader.Read(Path + "DIYCardIndex"));
                    }
                }
            }
            return instance;
        }

        public CardLibrary(CardDescription[] cards)
        {
            Cards = cards;
            NonDIYCount = cards.Length;
        }

        public void AddDIYCards(CardDescription[] cards)
        {
            if (cards.Length > 0)
            {
                if (DIYMap == null)
                    DIYMap = new Hashtable();
                for (int i = 0; i < cards.Length; i++)
                {
                    cards[i].cardCamp = CardCamp.DIY;
                    DIYMap.Add(cards[i].ID, Cards.Length + i);
                }

                CardDescription[] newcards = new CardDescription[Cards.Length + cards.Length];
                Cards.CopyTo(newcards, 0);
                cards.CopyTo(newcards, Cards.Length);
                Cards = newcards;
            }
        }

        public CardDescription[] GetCards()
        {
            return Cards;
        }

        public int GetCount()
        {
            if (Cards == null)
                return 0;
            else
                return Cards.Length;
        }

        public int GetDIYCount()
        {
            if (Cards == null)
                return 0;
            else
                return DIYMap.Count;
        }

        public int GetNonDIYCount()
        {
            if (Cards == null)
                return 0;
            else
                return NonDIYCount;
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
                    //BooleanQuery bquery = new BooleanQuery();
                    QueryParser parser = new MultiFieldQueryParser(new string[] {"name", "oldName", "shortName", "japName", "enName", "effect", "cardType", "tribe", "element", "level", "atk", "def", "package", "infrequence" }, AnalyzerFactory.GetAnalyzer());
                    query = parser.Parse(queryString);

                    /*
                    QueryParser parser = new QueryParser("name", AnalyzerFactory.GetAnalyzer());
                    Query q = parser.Parse(queryString);
                    bquery.Add(q, BooleanClause.Occur.SHOULD);

                    parser = new QueryParser("oldName", AnalyzerFactory.GetAnalyzer());
                    q = parser.Parse(queryString);
                    bquery.Add(q, BooleanClause.Occur.SHOULD);

                    parser = new QueryParser("shortName", AnalyzerFactory.GetAnalyzer());
                    q = parser.Parse(queryString);
                    bquery.Add(q, BooleanClause.Occur.SHOULD);

                    parser = new QueryParser("japName", AnalyzerFactory.GetAnalyzer());
                    q = parser.Parse(queryString);
                    bquery.Add(q, BooleanClause.Occur.SHOULD);

                    parser = new QueryParser("enName", AnalyzerFactory.GetAnalyzer());
                    q = parser.Parse(queryString);
                    bquery.Add(q, BooleanClause.Occur.SHOULD);

                    parser = new QueryParser("effect", AnalyzerFactory.GetAnalyzer());
                    q = parser.Parse(queryString);
                    bquery.Add(q, BooleanClause.Occur.SHOULD);

                    parser = new QueryParser("cardType", AnalyzerFactory.GetAnalyzer());
                    q = parser.Parse(queryString);
                    bquery.Add(q, BooleanClause.Occur.SHOULD);

                    parser = new QueryParser("tribe", AnalyzerFactory.GetAnalyzer());
                    q = parser.Parse(queryString);
                    bquery.Add(q, BooleanClause.Occur.SHOULD);

                    parser = new QueryParser("element", AnalyzerFactory.GetAnalyzer());
                    q = parser.Parse(queryString);
                    bquery.Add(q, BooleanClause.Occur.SHOULD);

                    parser = new QueryParser("level", AnalyzerFactory.GetAnalyzer());
                    q = parser.Parse(queryString);
                    bquery.Add(q, BooleanClause.Occur.SHOULD);

                    parser = new QueryParser("atk", AnalyzerFactory.GetAnalyzer());
                    q = parser.Parse(queryString);
                    bquery.Add(q, BooleanClause.Occur.SHOULD);

                    parser = new QueryParser("def", AnalyzerFactory.GetAnalyzer());
                    q = parser.Parse(queryString);
                    bquery.Add(q, BooleanClause.Occur.SHOULD);

                    parser = new QueryParser("package", AnalyzerFactory.GetAnalyzer());
                    q = parser.Parse(queryString);
                    bquery.Add(q, BooleanClause.Occur.SHOULD);

                    parser = new QueryParser("infrequence", AnalyzerFactory.GetAnalyzer());
                    q = parser.Parse(queryString);
                    bquery.Add(q, BooleanClause.Occur.SHOULD);

                    query = bquery;
                     */ 
                }

                Searcher searcher = null;
                if (KeepInMemory)
                {
                    if (allowDIY && ramdiydir != null)
                    {
                        IndexSearcher[] indexsearchers = new IndexSearcher[2];
                        indexsearchers[0] = new IndexSearcher(ramdir);
                        indexsearchers[1] = new IndexSearcher(ramdiydir);
                        searcher = new MultiSearcher(indexsearchers);
                    }
                    else
                        searcher = new IndexSearcher(ramdir);
                }
                else
                {
                    if (allowDIY && Directory.Exists(Path + "DIYCardIndex"))
                    {
                        IndexSearcher[] indexsearchers = new IndexSearcher[2];
                        indexsearchers[0] = new IndexSearcher(Path + "CardIndex");
                        indexsearchers[1] = new IndexSearcher(Path + "DIYCardIndex");
                        searcher = new MultiSearcher(indexsearchers);
                    }
                    else
                        searcher = new IndexSearcher(Path + "CardIndex");
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
                    QueryParser parser = new QueryParser("name", AnalyzerFactory.GetAnalyzer());
                    Query q = parser.Parse(queryString);
                    bquery.Add(q, BooleanClause.Occur.SHOULD);

                    parser = new QueryParser("oldName", AnalyzerFactory.GetAnalyzer());
                    q = parser.Parse(queryString);
                    bquery.Add(q, BooleanClause.Occur.SHOULD);

                    parser = new QueryParser("shortName", AnalyzerFactory.GetAnalyzer());
                    q = parser.Parse(queryString);
                    bquery.Add(q, BooleanClause.Occur.SHOULD);

                    parser = new QueryParser("japName", AnalyzerFactory.GetAnalyzer());
                    q = parser.Parse(queryString);
                    bquery.Add(q, BooleanClause.Occur.SHOULD);

                    parser = new QueryParser("enName", AnalyzerFactory.GetAnalyzer());
                    q = parser.Parse(queryString);
                    bquery.Add(q, BooleanClause.Occur.SHOULD);

                    query = bquery;
                }

                Searcher searcher = null;
                if (KeepInMemory)
                {
                    if (allowDIY && ramdiydir != null)
                    {
                        IndexSearcher[] indexsearchers = new IndexSearcher[2];
                        indexsearchers[0] = new IndexSearcher(ramdir);
                        indexsearchers[1] = new IndexSearcher(ramdiydir);
                        searcher = new MultiSearcher(indexsearchers);
                    }
                    else
                        searcher = new IndexSearcher(ramdir);
                }
                else
                {
                    if (allowDIY && Directory.Exists(Path + "DIYCardIndex"))
                    {
                        IndexSearcher[] indexsearchers = new IndexSearcher[2];
                        indexsearchers[0] = new IndexSearcher(Path + "CardIndex");
                        indexsearchers[1] = new IndexSearcher(Path + "DIYCardIndex");
                        searcher = new MultiSearcher(indexsearchers);
                    }
                    else
                        searcher = new IndexSearcher(Path + "CardIndex");
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

        public CardDescription GetCardByIndex(int index)
        {
            if (index >= 0 && index < Cards.Length)
            {
                return Cards[index];
            }
            else
                return null;
        }

        public CardDescription GetCardByID(int id)
        {
            if (id <= NonDIYCount)
                return Cards[id - 1];
            else
            {
                if (DIYMap[id] == null)
                    return null;
                else
                    return Cards[(int)DIYMap[id]];
            }
        }

        public CardDescription GetCardByName(string name)
        {
            foreach (CardDescription card in Cards)
            {
                if (string.Equals(name, card.name))
                    return card;
            }

            return null;
        }

        public CardDescription GetCardByJapName(string japname)
        {
            string jname = japname.Replace('-', '£­');

            foreach (CardDescription card in Cards)
            {
                if (string.Equals(jname, card.japName.Replace('-', '£­')))
                    return card;
            }

            return null;
        }

        public CardDescription GetCardByEnName(string name)
        {
            foreach (CardDescription card in Cards)
            {
                if (string.Equals(name, card.enName))
                    return card;
            }

            return null;
        }

        public CardDescription GetCardByOldName(string name)
        {
            foreach (CardDescription card in Cards)
            {
                if (string.Equals(name, card.oldName))
                    return card;

                if (card.oldName.IndexOf(name) >= 0)
                {
                    string[] names = card.oldName.Split(',', '£¬');
                    foreach (string s in names)
                    {
                        if (s.Trim() == name.Trim())
                            return card;
                    }
                }
            }

            return null;
        }

        public CardDescription GetCardByShortName(string name)
        {
            foreach (CardDescription card in Cards)
            {
                if (string.Equals(name, card.shortName))
                    return card;

                if (card.shortName.IndexOf(name) >= 0)
                {
                    string[] names = card.shortName.Split(',', '£¬');
                    foreach (string s in names)
                    {
                        if (s.Trim() == name.Trim())
                            return card;
                    }
                }
            }

            return null;
        }
    }
}
