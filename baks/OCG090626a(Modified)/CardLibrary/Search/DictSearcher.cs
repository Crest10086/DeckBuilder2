using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using BaseCardLibrary.DataAccess;
using BaseCardLibrary.Search;
using Lucene.Net.Search;
using Lucene.Net.QueryParsers;
using Lucene.Net.Analysis;
using Microsoft.VisualBasic;
using Tools;

namespace BaseCardLibrary.Search
{
    public class SearchResult
    {
        public CardDescription[] Cards = null;
        public string KeyWord = null;
    }

    public class WordList
    {
        public string[] Words = null;
        public int CurrentIndex = -1;
    }

    public class oldDictSearcher
    {
        CardLibrary cardLibrary = null;
        ArrayList resultlist = null;
        Hashtable nametable = null;
        Hashtable japnametable = null;
        Hashtable ennametable = null;


        public oldDictSearcher()
        {
            cardLibrary = CardLibrary.GetInstance();
            CardDescription[] cards = cardLibrary.GetCards();

            nametable = new Hashtable(cards.Length);
            japnametable = new Hashtable(cards.Length);
            ennametable = new Hashtable(cards.Length);

            for (int i = 1; i <= cards.Length; i++)
            {
                CardDescription card = cards[i - 1];
                nametable.Add(card.name, card.ID);
                //if (card.japName != "")
                //    japnametable.Add(card.japName, i);
                //if (card.enName != "")
                //    ennametable.Add(card.enName, i);
                japnametable[card.japName] = card.ID;
                ennametable[card.enName] = card.ID;
            }
        }

        private void AddToResultList(CardDescription card)
        {
            foreach (Object o in resultlist)
            {
                CardDescription c = (CardDescription)o;
                if (c.ID == card.ID)
                    return;
            }

            resultlist.Add(card);
        }

        static Regex WordRegex = new Regex(@"(([0-9]+)|([A-Za-z]+)|([\u2E80-\u9FA5]+))", RegexOptions.Compiled);
        public static string GetMainWord(string sen, int loc)
        {
            MatchCollection matches = WordRegex.Matches(sen);
            for (int i = 0; i < matches.Count; i++)
            {
                Match match = matches[i];
                if (loc >= match.Index && loc <= match.Index + match.Length)
                {
                    return match.Value;
                }
            }

            return "";
        }

        private static WordList GetWordList(string sen, int loc)
        {
            WordList wl = new WordList();
            ArrayList resultlist = new ArrayList();
            MatchCollection matches = WordRegex.Matches(sen);
            for (int i = 0; i < matches.Count; i++)
            {
                Match match = matches[i];
                resultlist.Add(match.Value);
                if (loc >= match.Index && loc <= match.Index + match.Length)
                {
                    wl.CurrentIndex = i;
                }
            }
            wl.Words = (string[])resultlist.ToArray(typeof(string));

            return wl;
        }

        static Regex LetterRegex = new Regex(@"[\u2E80-\u9FA5]", RegexOptions.Compiled);
        private static WordList GetLetterList(string sen, int loc)
        {
            WordList wl = new WordList();
            ArrayList resultlist = new ArrayList();
            MatchCollection matches = LetterRegex.Matches(sen);
            for (int i = 0; i < matches.Count; i++)
            {
                Match match = matches[i];
                resultlist.Add(match.Value);
                if (loc >= match.Index && loc <= match.Index + match.Length)
                {
                    wl.CurrentIndex = i;
                }
            }
            wl.Words = (string[])resultlist.ToArray(typeof(string));

            return wl;
        }

        private static string GetSubQueryString(string sen, float boost)
        {
            string queryString = sen;

            BooleanQuery query = new BooleanQuery();
            QueryParser parser = new QueryParser("name", AnalyzerFactory.GetAnalyzer());
            Query q = parser.Parse(queryString);
            q.SetBoost(boost * 9);
            query.Add(q, BooleanClause.Occur.SHOULD);

            parser = new QueryParser("oldName", AnalyzerFactory.GetAnalyzer());
            q = parser.Parse(queryString);
            q.SetBoost(boost * 7);
            query.Add(q, BooleanClause.Occur.SHOULD);

            parser = new QueryParser("shortName", AnalyzerFactory.GetAnalyzer());
            q.SetBoost(boost * 10);
            q = parser.Parse(queryString);
            query.Add(q, BooleanClause.Occur.SHOULD);

            parser = new QueryParser("japName", AnalyzerFactory.GetAnalyzer());
            q.SetBoost(boost * 8);
            q = parser.Parse(queryString);
            query.Add(q, BooleanClause.Occur.SHOULD);

            parser = new QueryParser("enName", AnalyzerFactory.GetAnalyzer());
            q.SetBoost(boost * 7);
            q = parser.Parse(queryString);
            query.Add(q, BooleanClause.Occur.SHOULD);

            return query.ToString();
        }

        private static string GetQueryString(string sen, int loc)
        {
            string queryString = GetMainWord(sen, loc);

            return GetSubQueryString(queryString, 1) + " \"" + GetSubQueryString(sen, 10) + "\"";
        }

        public static string GetMainSentence(string sen, int loc)
        {
            string stopchars = ",;*、\"[]()「」【】〖〗{}";
            if (sen == null || sen == "")
                return "";

            int start = loc;
            int end = loc;

            while ((start >= 0) && (stopchars.IndexOf(sen[start]) < 0))
                start--;

            while ((end < sen.Length) && (stopchars.IndexOf(sen[end]) < 0))
                end++;


            return sen.Substring(start + 1, end - start - 1);

        }

        //匹配要求名字严格相同
        private void CardMatch(string name)
        {
            if (name == "")
                return;

            CardDescription card = null;

            //按缩写名匹配
            card = cardLibrary.GetCardByShortName(name);
            if (card != null)
            {
                AddToResultList(card);
            }

            //按卡名匹配
            if (nametable.Contains(name))
            {
                int i = int.Parse(nametable[name].ToString());
                AddToResultList(cardLibrary.GetCardByID(i));
            }

            //按日文名匹配
            if (japnametable.Contains(name))
            {
                int i = int.Parse(japnametable[name].ToString());
                AddToResultList(cardLibrary.GetCardByID(i));
            }

            //按英文名匹配
            if (ennametable.Contains(name))
            {
                int i = int.Parse(ennametable[name].ToString());
                AddToResultList(cardLibrary.GetCardByID(i));
            }

            //按曾用名匹配
            card = cardLibrary.GetCardByOldName(name);
            if (card != null)
            {
                AddToResultList(card);
            }
        }

        //搜索可以有部分字符不同，或者部分包含
        private void CardSearch(string name)
        {
            CardDescription[] cards = null;

            //按缩写名搜索
            cards = cardLibrary.Search("shortName:\"" + name + "\"");
            if (cards.Length > 0)
            {
                for (int i = 0; i < cards.Length; i++)
                    AddToResultList(cards[i]);
            }

            //按卡名、日文名、英文名搜索
            cards = cardLibrary.Search(string.Format("name:\"{0}\" japName:\"{0}\" enName:\"{0}\"", name)); //  "name:\"" + name + "\"");
            if (cards.Length > 0)
            {
                for (int i = 0; i < cards.Length; i++)
                    AddToResultList(cards[i]);
            }

            /*
            //按日文名搜索
            cards = cardLibrary.Search("japName:\"" + name + "\"");
            if (cards.Length > 0)
            {
                for (int i = 0; i < cards.Length; i++)
                    AddToResultList(cards[i]);
            }

            //按英文名搜索
            cards = cardLibrary.Search("enName:\"" + name + "\"");
            if (cards.Length > 0)
            {
                for (int i = 0; i < cards.Length; i++)
                    AddToResultList(cards[i]);
            }
             * */

            //按曾用名搜索
            cards = cardLibrary.Search("oldName:\"" + name + "\"");
            if (cards.Length > 0)
            {
                for (int i = 0; i < cards.Length; i++)
                    AddToResultList(cards[i]);
            }

            //按拼音搜索
            if (GetPingyin.GetChineseLength(name) > 8)
                return;
            string[] ss = GetPingyin.converts(name);
            string s = "";
            for (int i = 0; i < ss.Length; i++)
                s += string.Format("pyname:\"{0}\" pyshortName:\"{0}\" pyoldName:\"{0}\" ", ss[i]);
            cards = cardLibrary.Search(s);
            if (cards.Length > 0)
            {
                for (int i = 0; i < cards.Length; i++)
                    AddToResultList(cards[i]);
            }
        }

        Regex PunRegex = new Regex("[-!\"#$%&'( )*+,.、/:;<=>?@[\\\\\\]^_'{|}~「」]", RegexOptions.Compiled);
        public SearchResult TopSearch(string SentenceString, int lLoc, int top)
        {
            SearchResult sr = new SearchResult();
            resultlist = new ArrayList(top);


            string simplesen = CharacterSet.BIG5ToGB(SentenceString);
            string mainsen = GetMainSentence(simplesen, lLoc).Trim();

            //先匹配
            CardMatch(mainsen);
            if (sr.KeyWord == null && resultlist.Count > 0)
                sr.KeyWord = mainsen;

            //接着搜索
            string sss = PunRegex.Replace(simplesen, " ");
            CardSearch(sss);
            if (sr.KeyWord == null && resultlist.Count > 0)
                sr.KeyWord = sss;

            sss = PunRegex.Replace(mainsen, " ");
            CardSearch(sss);
            if (sr.KeyWord == null && resultlist.Count > 0)
                sr.KeyWord = sss;

            //如果结果数不足8，进行词扩展搜索
            if (resultlist.Count < top)
            {
                WordList wl = GetWordList(simplesen, lLoc);
                StringBuilder sb = new StringBuilder();

                int i = wl.CurrentIndex + 2;
                while (i >= wl.Words.Length) i--;

                for (; i >= wl.CurrentIndex; i--)
                {
                    int j = wl.CurrentIndex - 2;
                    while (j < 0) j++;

                    for (; j <= wl.CurrentIndex; j++)
                    {
                        sb.Length = 0;
                        for (int k = j; k <= i; k++)
                        {
                            sb.Append(wl.Words[k]);
                            sb.Append(" ");
                        }
                        int precount = resultlist.Count;
                        string ssss = sb.ToString();
                        CardSearch(ssss);
                        if (sr.KeyWord == null && precount < resultlist.Count)
                            sr.KeyWord = ssss.Trim();
                    }
                }
            }

            string stopWord = "的之の・";
            char[] stopWords = { '的', '之', 'の', '・' };
            Regex StopWordsRegex = new Regex(@"(^[\s、的之の・]*)|([\s、的之の・]*$)", RegexOptions.Compiled);

            //如果结果数不足8，进行字扩展搜索
            if (resultlist.Count < top && stopWord.IndexOf(simplesen[lLoc]) < 0)
            {

                WordList wl = GetLetterList(simplesen, lLoc);
                StringBuilder sb = new StringBuilder();

                int i = wl.CurrentIndex + 4;
                while (i >= wl.Words.Length) i--;

                for (; i >= wl.CurrentIndex; i--)
                {
                    int j = wl.CurrentIndex - 2;
                    while (j < 0) j++;

                    for (; j <= wl.CurrentIndex; j++)
                    {
                        sb.Length = 0;
                        for (int k = j; k <= i; k++)
                        {
                            sb.Append(wl.Words[k]);
                            sb.Append(" ");
                        }
                        int precount = resultlist.Count;
                        string ssss = sb.ToString();
                        CardSearch(ssss);
                        if (sr.KeyWord == null && precount < resultlist.Count)
                            sr.KeyWord = StopWordsRegex.Replace(ssss, "");
                    }
                }
            }


            sr.Cards = (CardDescription[])resultlist.ToArray(typeof(CardDescription));
            return sr;
        }


        //=================================================================================================
    }


    class DictSearcherNode
    {
        public int ID = 0;
        public int Score = 0;
    }

    class DictSearcherCompare : System.Collections.IComparer
    {
        CardLibrary cardLibrary = null;

        public DictSearcherCompare()
        {
            cardLibrary = CardLibrary.GetInstance();
        }

        public int Compare(object x, object y)
        {
            DictSearcherNode nx = (DictSearcherNode)x;
            DictSearcherNode ny = (DictSearcherNode)y;
            return ny.Score - nx.Score;
        }
    }


    public class DictSearcher
    {
        CardLibrary cardLibrary = CardLibrary.GetInstance();
        ArrayList Result = null;
        ArrayList Result2 = null;

        public DictSearcher()
        {
            Result = new ArrayList(cardLibrary.GetCount());
            for (int i = 0; i < cardLibrary.GetCount(); i++)
            {
                DictSearcherNode node = new DictSearcherNode();
                node.ID = i + 1;
                Result.Add(node);
            }
        }

        static Regex WordRegex = new Regex(@"(([0-9]+)|([A-Za-z]+)|([\u2E80-\u9FA5]+))", RegexOptions.Compiled);
        public static string GetMainWord(string sen, int loc)
        {
            MatchCollection matches = WordRegex.Matches(sen);
            for (int i = 0; i < matches.Count; i++)
            {
                Match match = matches[i];
                if (loc >= match.Index && loc <= match.Index + match.Length)
                {
                    return match.Value;
                }
            }

            return "";
        }

        DictSearcherNode GetSearcherNodeByID(int id)
        {
            if (id < 60000)
                return (DictSearcherNode)Result[id - 1];
            else
                return (DictSearcherNode)Result[id - 60000 + cardLibrary.GetNonDIYCount()];
        }

        private int DoSearchShortName(string keyword, int score)
        {
            string querystring = string.Format("shortName:\"{0}\"", keyword);
            CardDescription[] cards = cardLibrary.Search(querystring);
            if (cards.Length > 0)
            {
                for (int i = 0; i < cards.Length; i++)
                {
                    CardDescription card = cards[i];
                    DictSearcherNode node = GetSearcherNodeByID(card.ID);
                    string[] ss = card.shortName.Split('，');
                    int len = card.shortName.Length;
                    for (int j = 0; j < ss.Length; j++)
                    {
                        if (ss[j].IndexOf(keyword) >= 0 && len > ss[j].Length)
                            len = ss[j].Length;
                    }
                    if (score - len > node.Score)
                        node.Score = score - len;
                }
            }
            return cards.Length;
        }

        private int DoSearchOldName(string keyword, int score)
        {
            string querystring = string.Format("oldName:\"{0}\"", keyword);
            CardDescription[] cards = cardLibrary.Search(querystring);
            if (cards.Length > 0)
            {
                for (int i = 0; i < cards.Length; i++)
                {
                    CardDescription card = cards[i];
                    DictSearcherNode node = GetSearcherNodeByID(card.ID);
                    string[] ss = card.oldName.Split('，');
                    int len = card.oldName.Length;
                    for (int j = 0; j < ss.Length; j++)
                    {
                        if (ss[j].IndexOf(keyword) >= 0 && len > ss[j].Length)
                            len = ss[j].Length;
                    }
                    if (score - len > node.Score)
                        node.Score = score - len;
                }
            }
            return cards.Length;
        }

        private int DoSearchName(string keyword, int score)
        {
            string querystring = string.Format("name:\"{0}\"", keyword);
            CardDescription[] cards = cardLibrary.Search(querystring);
            if (cards.Length > 0)
            {
                for (int i = 0; i < cards.Length; i++)
                {
                    CardDescription card = cards[i];
                    DictSearcherNode node = GetSearcherNodeByID(card.ID);
                    if (score - card.name.Length > node.Score)
                        node.Score = score - card.name.Length;
                }
            }
            return cards.Length;
        }

        private int DoSearchJapName(string keyword, int score)
        {
            string querystring = string.Format("japName:\"{0}\"", keyword);
            CardDescription[] cards = cardLibrary.Search(querystring);
            if (cards.Length > 0)
            {
                for (int i = 0; i < cards.Length; i++)
                {
                    CardDescription card = cards[i];
                    DictSearcherNode node = GetSearcherNodeByID(card.ID);
                    if (score - card.japName.Length > node.Score)
                        node.Score = score - card.japName.Length;
                }
            }
            return cards.Length;
        }

        private int DoSearchEnName(string keyword, int score)
        {
            string querystring = string.Format("enName:\"{0}\"", keyword);
            CardDescription[] cards = cardLibrary.Search(querystring);
            if (cards.Length > 0)
            {
                for (int i = 0; i < cards.Length; i++)
                {
                    CardDescription card = cards[i];
                    DictSearcherNode node = GetSearcherNodeByID(card.ID);
                    if (score - card.enName.Length > node.Score)
                        node.Score = score - card.enName.Length;
                }
            }
            return cards.Length;
        }

        private int DoSearchPYName(string querystring, int score)
        {
            CardDescription[] cards = cardLibrary.Search(querystring);
            if (cards.Length > 0)
            {
                for (int i = 0; i < cards.Length; i++)
                {
                    CardDescription card = cards[i];
                    DictSearcherNode node = GetSearcherNodeByID(card.ID);
                    if (score > node.Score)
                        node.Score = score;
                }
            }
            return cards.Length;
        }

        private int DoSearchPYShortName(string querystring, int score)
        {
            CardDescription[] cards = cardLibrary.Search(querystring);
            if (cards.Length > 0)
            {
                for (int i = 0; i < cards.Length; i++)
                {
                    CardDescription card = cards[i];
                    DictSearcherNode node = GetSearcherNodeByID(card.ID);
                    if (score > node.Score)
                        node.Score = score;
                }
            }
            return cards.Length;
        }

        private int DoSearchPYOldName(string querystring, int score)
        {
            CardDescription[] cards = cardLibrary.Search(querystring);
            if (cards.Length > 0)
            {
                for (int i = 0; i < cards.Length; i++)
                {
                    CardDescription card = cards[i];
                    DictSearcherNode node = GetSearcherNodeByID(card.ID);
                    if (score > node.Score)
                        node.Score = score;
                }
            }
            return cards.Length;
        }

        private int GetTokenizerLength(string text)
        {
            int result = 0;

            MyAnalyzer ma = new MyAnalyzer(AnalyzerFactory.stopWords);
            Lucene.Net.Analysis.TokenStream ts = ma.TokenStream("", new System.IO.StringReader(text));

            Lucene.Net.Analysis.Token token;
            while ((token = ts.Next()) != null)
            {
                int len = token.TermLength();
                if (len == 1)
                {
                    char[] buff = token.TermBuffer();
                    if (buff[0] != MyFilter.Separator)
                        result++;
                }
                else
                    result += len;
            }
            ts.Close();

            return result;
        }

        private string GetTokenizerText(string text)
        {
            StringBuilder result = new StringBuilder();

            MyAnalyzer ma = new MyAnalyzer(AnalyzerFactory.stopWords);
            Lucene.Net.Analysis.TokenStream ts = ma.TokenStream("", new System.IO.StringReader(text));

            Lucene.Net.Analysis.Token token;
            while ((token = ts.Next()) != null)
            {
                int len = token.TermLength();
                char[] buff = token.TermBuffer();
                if (len == 1)
                {
                    if (buff[0] != MyFilter.Separator)
                        result.Append(buff, 0, 1);
                }
                else
                {
                    result.Append(buff, 0, len);
                }
            }
            ts.Close();

            return result.ToString();
        }

        Regex blankRegex = new Regex(@"\s", RegexOptions.Compiled);
        private int DoSearch(string keyword)
        {
            Result2 = new ArrayList(cardLibrary.GetCount());
            for (int i = 0; i < cardLibrary.GetCount(); i++)
            {
                DictSearcherNode node = (DictSearcherNode)Result[i];
                DictSearcherNode newnode = new DictSearcherNode();
                newnode.ID = node.ID;
                newnode.Score = node.Score;
                node.Score = 0;
                Result2.Add(newnode);
            }

            int len = GetTokenizerLength(keyword);
            if (len > 5)
                len = 5;
            int factor = (int)Math.Pow(5, len);
            int ret = 0;
            ret += DoSearchShortName(keyword, 4000 * factor);
            ret += DoSearchName(keyword, 2000 * factor);
            ret += DoSearchJapName(keyword, 1500 * factor);
            ret += DoSearchEnName(keyword, 1500 * factor);
            ret += DoSearchOldName(keyword, 1000 * factor);

            if (GetPingyin.GetChineseLength(keyword) <= 8)
            {
                int pyfactor = (int)Math.Pow(3, len);

                string[] ss = GetPingyin.converts(keyword);
                string s = "";
                for (int i = 0; i < ss.Length; i++)
                    s += string.Format("pyname:\"{0}\"", ss[i]);
                ret += DoSearchPYName(s, 1000 * pyfactor);

                s = "";
                for (int i = 0; i < ss.Length; i++)
                    s += string.Format("pyshortName:\"{0}\"", ss[i]);
                ret += DoSearchPYShortName(s, 1000 * pyfactor);

                s = "";
                for (int i = 0; i < ss.Length; i++)
                    s += string.Format("pyoldName:\"{0}\"", ss[i]);
                ret += DoSearchPYOldName(s, 500 * pyfactor);
            }


            for (int i = 0; i < cardLibrary.GetCount(); i++)
            {
                DictSearcherNode node1 = (DictSearcherNode)Result[i];
                DictSearcherNode node2 = (DictSearcherNode)Result2[i];
                node1.Score += node2.Score;
            }

            return ret;
        }

        static Regex stopwrods = new Regex(@"[的之の]", RegexOptions.Compiled);
        public SearchResult TopSearch(string SentenceString, int lLoc, int top)
        {

            SearchResult sr = new SearchResult();

            //转为简体
            //string simplesen = CharacterSet.BIG5ToGB(SentenceString);
            //这里应该是全角转半角
            string simplesen = CharacterSet.SBCToDBC(SentenceString);
            //过滤stopwords
            simplesen = stopwrods.Replace(simplesen, " ");
            //过滤标点符号
            StringBuilder sb = new StringBuilder();
            int skip = 0;
            for (int i = 0; i < simplesen.Length; i++)
            {
                if (Char.IsLetterOrDigit(simplesen[i]))
                {
                    sb.Append(simplesen[i]);
                }
                else
                {
                    sb.Append(' ');
                    if (i <= lLoc)
                        skip++;
                }
            }
            simplesen = sb.ToString();
            if (simplesen.Length == 0)
                return sr;

            //清0
            for (int i = 0; i < Result.Count; i++)
            {
                if (i < cardLibrary.GetNonDIYCount())
                    ((DictSearcherNode)Result[i]).ID = i + 1;
                else
                    ((DictSearcherNode)Result[i]).ID = i + 60000 - cardLibrary.GetNonDIYCount();
                ((DictSearcherNode)Result[i]).Score = 0;
            }
            Hashtable SearchedKeyword = new Hashtable();
            string lMainWord = "";
            int lMainWordLenth = 0;
            string rMainWord = "";
            int rMainWordLenth = 0;

            for (int i = lLoc; i >= 0; i--)
            {
                string s = simplesen.Substring(i, lLoc - i + 1);
                string trims = GetTokenizerText(s);
                if (trims.Length > 0 && !SearchedKeyword.ContainsKey(trims))
                {
                    SearchedKeyword.Add(trims, 0);
                    if (DoSearch(s) > 0)
                    {
                        if (trims.Length > lMainWordLenth)
                        {
                            int blank = 0;
                            while (s[blank] == ' ')
                                blank++;
                            lMainWord = SentenceString.Substring(i + blank, lLoc - i + 1 - blank);
                            lMainWordLenth = trims.Length;
                        }
                    }
                    else
                        break;
                    
                }
            }

            for (int i = lLoc + 1; i < simplesen.Length; i++)
            {
                string s = simplesen.Substring(lLoc, i - lLoc + 1);
                string trims = GetTokenizerText(s);
                if (trims.Length > 0 && !SearchedKeyword.ContainsKey(trims))
                {
                    SearchedKeyword.Add(trims, 0);
                    if (DoSearch(s) > 0)
                    {
                        if (trims.Length > rMainWordLenth)
                        {
                            int blank = 0;
                            while (s[blank] == ' ')
                                blank++;
                            rMainWord = SentenceString.Substring(lLoc + blank, i - lLoc + 1 - blank);
                            rMainWordLenth = trims.Length;
                        }
                    }
                    else
                        break;
                }
            }

            DictSearcherCompare dsc = new DictSearcherCompare();
            Result.Sort(dsc);

            sr.Cards = new CardDescription[top];
            if (rMainWord.Length > 1)
            {
                if (GetTokenizerText(SentenceString.Substring(lLoc, 1)).Length > 0)
                    sr.KeyWord = lMainWord + rMainWord.Substring(1);
                else
                    sr.KeyWord = lMainWord + rMainWord;
            }
            else
                sr.KeyWord = lMainWord;
            for (int i = 0; i < top; i++)
            {
                sr.Cards[i] = cardLibrary.GetCardByID(((DictSearcherNode)Result[i]).ID);
            }

            return sr;
        }

        public CardDescription[] AllSearch(string SentenceString, int Top)
        {
            //转为简体
            //string simplesen = CharacterSet.BIG5ToGB(SentenceString);
            //这里应该是全角转半角
            string simplesen = CharacterSet.SBCToDBC(SentenceString);
            //过滤stopwords
            simplesen = stopwrods.Replace(simplesen, " ");
            //过滤标点符号
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < simplesen.Length; i++)
            {
                if (Char.IsLetterOrDigit(simplesen[i]))
                {
                    sb.Append(simplesen[i]);
                }
                else
                {
                    sb.Append(' ');
                }
            }
            simplesen = sb.ToString();
            if (simplesen.Length == 0)
                return new CardDescription[0];

            //清0
            for (int i = 0; i < Result.Count; i++)
            {
                ((DictSearcherNode)Result[i]).ID = i + 1;
                ((DictSearcherNode)Result[i]).Score = 0;
            }

            DoSearch(simplesen);

            DictSearcherCompare dsc = new DictSearcherCompare();
            Result.Sort(dsc);

            ArrayList al = new  ArrayList();
            if (Top == 0)
                Top = cardLibrary.GetCount();

            for (int i = 0; i < Top; i++)
            {
                DictSearcherNode node = (DictSearcherNode)Result[i];
                if (node.Score > 0)
                {
                    CardDescription card = cardLibrary.GetCardByID(node.ID);
                    al.Add(card);
                }
                else
                    break;
            }

            return (CardDescription[])al.ToArray(typeof(CardDescription));
        }
    }
}