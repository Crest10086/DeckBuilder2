using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.OleDb;
using System.Data.SQLite;
using Lucene.Net.Search;
using Lucene.Net.Documents;

namespace BaseCardLibrary.DataAccess
{
    public delegate void ReadProcessChangedInvoker(int total, int current);

    public abstract class CardsReader
    {
        public const int MinCapacity = 4000;

        public virtual CardDescription[] Read(string source)
        {
            return Read(source, null);
        }

        public virtual CardDescription[] Read(string source, ReadProcessChangedInvoker processchanged)
        {
            return new CardDescription[] { };
        }
    }

    public class yxwpReader: CardsReader
    {
        public override CardDescription[] Read(string filename)
        {
            return Read(filename, null);
        }

        public override CardDescription[] Read(string filename, ReadProcessChangedInvoker processchanged)
        {
            if (!File.Exists(filename))
                return new CardDescription[0];

            int total = File.ReadAllLines(filename).Length;

            StreamReader sr = null;
            ArrayList cards = new ArrayList(MinCapacity);

            sr = new StreamReader(filename, Encoding.GetEncoding("GB2312"));
            int i = 0;

            while (!sr.EndOfStream)
            {
                string s = sr.ReadLine();
                cards.Add(ParseCard(s));
                i++;
                if (processchanged != null)
                    processchanged.Invoke(total, i);
            }
            sr.Close();

            return (CardDescription[])cards.ToArray(typeof(CardDescription));

        }

        private CardDescription ParseCard(string info)
        {
            CardDescription card = new CardDescription();

            string[] infos = info.Split('^');

            //处理文本
            for (int i = 0; i < infos.Length; i++)
            {
                //空字符串不处理
                if (infos[i].Length > 0)
                {
                    //过滤开始的空格
                    while (infos[i][0] == ' ')
                    {
                        infos[i] = infos[i].Substring(1, infos[i].Length - 1);
                        if (infos[i].Length == 0)
                            break;
                    }

                    //转换双引号
                    infos[i] = infos[i].Replace("\"", "\"\"");
                }
            }

            //自编序号
            card.ID = int.Parse(infos[12]);

            //卡片类型
            card.sCardType = infos[8] + infos[9] + infos[0];
            if (string.Equals(card.sCardType, "普通怪兽"))
                card.sCardType = "通常怪兽";

            //中文名称
            card.name = infos[1];

            //星级
            try
            {
                card.level = int.Parse(infos[2]);
            }
            catch
            {
                card.level = -1;
            }

            //属性
            card.element = infos[3];

            //种族
            card.tribe = infos[4];

            //攻击
            card.atk = infos[5];
            if (card.element.Equals(""))
            {
                card.atk = "";
                card.atkValue = -10000;
            }
            else if (card.atk.Equals("?") || card.atk.Equals("？"))
                card.atkValue = -1;
            else
            {
                try
                {
                    card.atkValue = int.Parse(card.atk);
                }
                catch
                {
                    card.atkValue = -2;
                }
            }

            //防御
            card.def = infos[6];
            if (card.element.Equals(""))
            {
                card.def = "";
                card.defValue = -10000;
            }
            else if (card.def.Equals("?") || card.def.Equals("？"))
                card.defValue = -1;
            else
            {
                try
                {
                    card.defValue = int.Parse(card.def);
                }
                catch
                {
                    card.defValue = -2;
                }
            }

            //7未知
            //if (!infos[7].Equals(""))
            {
                //MessageBox.Show(infos[7]);
            }

            //卡包
            card.package = infos[10];

            //效果
            card.effect = infos[11];

            //日文名称
            card.japName = infos[13];

            //罕见度
            card.infrequence = infos[14];

            //限制
            card.limit = 3;


            return card;
        }
    }

    public class DIYReader : CardsReader
    {
        public override CardDescription[] Read(string filename)
        {
            return Read(filename, null);
        }

        public override CardDescription[] Read(string filename, ReadProcessChangedInvoker processchanged)
        {
            if (!File.Exists(filename))
                return new CardDescription[0];

            int total = File.ReadAllLines(filename).Length;

            StreamReader sr = null;
            ArrayList cards = new ArrayList(MinCapacity);

            sr = new StreamReader(filename, Encoding.GetEncoding("GB2312"));

            int i = 0;
            while (!sr.EndOfStream)
            {
                string s = sr.ReadLine().Trim();
                if (s != "")
                {
                    CardDescription card = new CardDescription();
                    card.name = s.Substring(1, s.Length - 2);
                    card.ID = 60000 + i;
                    card.cardCamp = CardCamp.DIY;
                    card.limit = -5;
                    s = sr.ReadLine().Trim();
                    cards.Add(ParseCard(card, s));
                    i++;
                    if (processchanged != null)
                        processchanged.Invoke(total, i);
                }
            }
            sr.Close();

            return (CardDescription[])cards.ToArray(typeof(CardDescription));

        }

        private CardDescription ParseCard(CardDescription card, string info)
        {
            string[] infos = info.Split(',');

            //处理文本
            for (int i = 0; i < infos.Length; i++)
            {
                //空字符串不处理
                if (infos[i].Length > 0)
                {
                    //过滤开始的空格
                    while (infos[i][0] == ' ')
                    {
                        infos[i] = infos[i].Substring(1, infos[i].Length - 1);
                        if (infos[i].Length == 0)
                            break;
                    }

                    //转换双引号
                    infos[i] = infos[i].Replace("\"", "\"\"");
                }
            }

            //卡片类型
            string s = infos[0].Substring(infos[0].Length-2, 2);
            if (s == "怪兽")
            {
                card.sCardType = infos[0];
                for (int i = 1; i < 7; i++)
                {
                    s = infos[i].Substring(0, 3);
                    string s2 = infos[i].Substring(3);
                    switch (s)
                    {
                        case "种族：":
                            card.tribe = s2;
                            break;
                        case "属性：":
                            card.element = s2;
                            break;
                        case "星级：":
                            try
                            {
                                card.level = int.Parse(s2);
                            }
                            catch
                            {
                                card.level = -1;
                            }
                            break;
                        case "攻击：":
                            //攻击
                            card.atk = s2;
                            if (card.atk.Equals("?") || card.atk.Equals("？"))
                                card.atkValue = -1;
                            else
                            {
                                try
                                {
                                    card.atkValue = int.Parse(card.atk);
                                }
                                catch
                                {
                                    card.atkValue = -2;
                                }
                            }
                            break;
                        case "防御：":
                            card.def = s2;
                            if (card.def.Equals("?") || card.def.Equals("？"))
                                card.defValue = -1;
                            else
                            {
                                try
                                {
                                    card.defValue = int.Parse(card.def);
                                }
                                catch
                                {
                                    card.defValue = -2;
                                }
                            }
                            break;
                        case "效果：":
                            card.effect = s2;
                            break;
                    }
                }
                for (int i = 7; i < infos.Length; i++)
                {
                    card.effect += "," + infos[i];
                }
            }
            else
            {
                s = infos[0].Substring(0, 5);
                if (s == "魔法种类：" || s == "陷阱种类：")
                {
                    card.sCardType = infos[0].Substring(5, 2) + infos[0].Substring(0, 2);
                    card.atk = "";
                    card.atkValue = -10000;
                    card.def = "";
                    card.defValue = -10000;

                    card.effect = infos[1].Substring(3);
                    for (int i = 2; i < infos.Length; i++)
                    {
                        card.effect += "," + infos[i];
                    }
                }
            }
            card.iCardType = CardDescription.CardTypeMapper(card.sCardType);
            
            return card;
        }
    }

    public class LuceneReader : CardsReader
    {
        public override CardDescription[] Read(string dirname)
        {
            return Read(dirname, null);
        }

        public override CardDescription[] Read(string dirname, ReadProcessChangedInvoker processchanged)
        {
            if (dirname == null || dirname.Length <= 0)
                return null;

            if (!Directory.Exists(dirname))
                return null;

            if (dirname[dirname.Length - 1] != '\\')
                dirname += "\\";

            ArrayList cards = new ArrayList(MinCapacity);
            Query query = new MatchAllDocsQuery();
            Lucene.Net.Store.Directory dir = new Lucene.Net.Store.SimpleFSDirectory(new DirectoryInfo(dirname), new Lucene.Net.Store.SimpleFSLockFactory());
            Searcher searcher = new IndexSearcher(dir, true);

            TopDocs td = searcher.Search(query, null, searcher.MaxDoc());
            ScoreDoc[] docs = td.scoreDocs;

            int length = docs.Length;
            for (int i = 0; i < length; i++)
            {
                Document doc = searcher.Doc(docs[i].doc);
                cards.Add(ParseCard(doc));
                if (processchanged != null)
                    processchanged.Invoke(length, i+1);
            }

            searcher.Close();

            return (CardDescription[])cards.ToArray(typeof(CardDescription));
        }

        public CardDescription[] Read(Lucene.Net.Store.Directory dir)
        {
            return Read(dir, null);
        }

        public CardDescription[] Read(Lucene.Net.Store.Directory dir, ReadProcessChangedInvoker processchanged)
        {
            ArrayList cards = new ArrayList(MinCapacity);
            Query query = new MatchAllDocsQuery();
            Searcher searcher = new IndexSearcher(dir, true);
            TopDocs td = searcher.Search(query, null, searcher.MaxDoc());
            ScoreDoc[] docs = td.scoreDocs;

            int length = docs.Length;
            for (int i = 0; i < length; i++)
            {
                Document doc = searcher.Doc(docs[i].doc);
                cards.Add(ParseCard(doc));
                if (processchanged != null)
                    processchanged.Invoke(length, i + 1);
            }

            searcher.Close();

            return (CardDescription[])cards.ToArray(typeof(CardDescription));
        }


        private string GetFieldString(Document doc, string fieldname)
        {
            Field field = doc.GetField(fieldname);
            if (field == null)
                return "";
            else
                return field.StringValue();
        }

        private CardDescription ParseCard(Document doc)
        {
            CardDescription card = new CardDescription();

            card.ID = int.Parse(GetFieldString(doc, "ID"));
            card.name = GetFieldString(doc, "name");
            card.japName = GetFieldString(doc, "japName");
            card.enName = GetFieldString(doc, "enName");
            card.oldName = GetFieldString(doc, "oldName2");
            card.shortName = GetFieldString(doc, "shortName2");
            card.sCardType = GetFieldString(doc, "cardType");
            card.iCardType = CardDescription.CardTypeMapper(card.sCardType);
            if (CardDescription.isMonsterCard(card))
            {
                card.level = int.Parse(GetFieldString(doc, "level"));
                card.pendulumL = int.Parse(GetFieldString(doc, "pendulumL"));
                card.pendulumR = int.Parse(GetFieldString(doc, "pendulumR"));
                card.element = GetFieldString(doc, "element");
                card.tribe = GetFieldString(doc, "tribe");
                card.atk = GetFieldString(doc, "atk");
                card.atkValue = int.Parse(GetFieldString(doc, "atkValue"));
                card.def = GetFieldString(doc, "def");
                card.defValue = int.Parse(GetFieldString(doc, "defValue"));
            }
            card.effectType = GetFieldString(doc, "effectType");
            card.effect = GetFieldString(doc, "effect");
            card.infrequence = GetFieldString(doc, "infrequence");
            card.package = GetFieldString(doc, "package");
            card.limit = int.Parse(GetFieldString(doc, "limit"));
            if (card.limit == -5)
            {
                card.cardCamp = CardCamp.DIY;
                card.limit = 3;
            }
            card.cheatcode = GetFieldString(doc, "cheatcode");
            card.aliasList = GetFieldString(doc, "aliasList");
            card.adjust = GetFieldString(doc, "adjust").Trim();

            string s = GetFieldString(doc, "cardCamp").Trim();

            switch (s)
            {
                case "TCG、OCG":
                    card.cardCamp = CardCamp.BothOT;
                    break;
                case "TCG":
                    card.cardCamp = CardCamp.TCG;
                    break;
                case "OCG":
                    card.cardCamp = CardCamp.OCG;
                    break;
                case "DIY":
                    card.cardCamp = CardCamp.DIY;
                    break;
            }

            return card;
        }
    }

    /*
     * 因为迷你卡查不再更新，所以暂时不再提供支持
    public class MiniCardXReader : CardsReader
    {
        public override CardDescription[] Read(string filename)
        {
            return Read(filename, null);
        }

        public override CardDescription[] Read(string filename, ReadProcessChangedInvoker processchanged)
        {
            ArrayList cards = new ArrayList(MinCapacity);
            Query query = new MatchAllDocsQuery();
            Searcher searcher = new IndexSearcher(filename);
            Hits hits = searcher.Search(query);

            int length = hits.Length();
            for (int i = 0; i < length; i++)
            {
                Document doc = hits.Doc(i);
                cards.Add(ParseCard(doc));
                if (processchanged != null)
                    processchanged.Invoke(length, i + 1);
            }

            return (CardDescription[])cards.ToArray(typeof(CardDescription));
        }

        private CardDescription ParseCard(Document doc)
        {
            CardDescription card = new CardDescription();

            card.ID = int.Parse(doc.GetField("CardId").StringValue());
            card.name = doc.GetField("中文名").StringValue();
            card.japName = doc.GetField("日文名").StringValue();
            card.enName = doc.GetField("英文名").StringValue();
            card.sCardType = doc.GetField("卡片类型").StringValue();
            if (string.Equals(card.sCardType.Substring(2, 2), "怪兽"))
            {
                card.level = int.Parse(doc.GetField("卡片星级").StringValue());
                card.element = doc.GetField("卡片属性").StringValue();
                card.tribe = doc.GetField("卡片种族").StringValue();
                card.atk = doc.GetField("攻击力").StringValue();
                if (string.Equals(card.atk, "9999"))
                {
                    card.atk = "?";
                    card.atkValue = -1;
                }
                else
                {
                    try
                    {
                        card.atkValue = int.Parse(card.atk);
                    }
                    catch
                    {
                        card.atkValue = -2;
                    }
                }
                card.def = doc.GetField("防御力").StringValue();
                if (string.Equals(card.def, "9999"))
                {
                    card.def = "?";
                    card.defValue = -1;
                }
                else
                {
                    try
                    {
                        card.defValue = int.Parse(card.def);
                    }
                    catch
                    {
                        card.defValue = -2;
                    }
                }
            }
            card.effectType = doc.GetField("详细类型").StringValue();
            card.effect = doc.GetField("效果").StringValue();
            card.infrequence = doc.GetField("罕贵程度").StringValue();
            card.package = doc.GetField("卡包").StringValue();
            card.cheatcode = doc.GetField("CardPass").StringValue();
            card.adjust = doc.GetField("调整").StringValue();
            card.associate = doc.GetField("关联卡片").StringValue();
            string l = doc.GetField("禁限类型").StringValue();
            switch (l)
            {
                case "无限制":
                    card.limit = 3;
                    break;
                case "禁止卡":
                    card.limit = 0;
                    break;
                case "限制卡":
                    card.limit = 1;
                    break;
                case "准限制卡":
                    card.limit = 2;
                    break;
                default:
                    card.limit = -4;
                    break;
            }
            card.iCardtype = CardTypeMapper.Mapper(card.sCardType);

            return card;
        }
    }
    */

    public class YFCCReader : CardsReader
    {
        private Hashtable ht = null;

        public override CardDescription[] Read(string filename)
        {
            return Read(filename, null);
        }

        public override CardDescription[] Read(string filename, ReadProcessChangedInvoker processchanged)
        {
            ArrayList cards = new ArrayList(MinCapacity);


            //连接数据库
            OleDbConnection con = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data source=" + filename + ";Persist Security Info=False;Jet OLEDB:Database Password=paradisefox@sohu.com;");
            con.Open();

            //读取效果种类列表
            ht = new Hashtable();

            OleDbCommand dcc = new OleDbCommand("Select * FROM [YGOEFFECT]", con);
            OleDbDataReader creader = null;
            try
            {
                //如果数据库中存在效果种类列表则优先读取数据库中的记录
                creader = dcc.ExecuteReader();

                while (creader.Read())
                {
                    string id = GetFieldString(creader, "ID");
                    string s = MyTools.CharacterSet.SBCToDBC(GetFieldString(creader, "EFFECT"));
                    ht.Add(id, s);
                    if (id.Length == 1)
                    {
                        id = "0" + id;
                        ht.Add(id, s);
                    }
                }
                creader.Close();
            }
            catch
            {

            }

            //从补丁文件里补充效果种类列表
            try
            {
                string[] ss = File.ReadAllLines(Global.GetPath() + "Patch\\EffectTypeList.dat", System.Text.Encoding.GetEncoding("gb2312"));
                foreach (string s in ss)
                {
                    string[] ss2 = s.Split('\t');
                    if (!ht.ContainsKey(ss2[0]))
                        ht.Add(ss2[0], MyTools.CharacterSet.SBCToDBC(ss2[1]));
                }
            }
            catch
            {
            }

            //统计记录数
            dcc = new OleDbCommand("Select Count(*) as iCount FROM [YGODATA]", con);
            creader = dcc.ExecuteReader();
            creader.Read();
            int total = int.Parse(creader["iCount"].ToString());
            creader.Close();

            //读入卡片数据 
            //OleDbCommand dc = new OleDbCommand("Select [CardID], [JPCardName], [SCCardName], [ENCardName], [SCCardType], [SCCardRace], [CardBagNum], [SCCardAttribute], [CardStarNum], [SCCardRare], [CardAtk], [CardDef], [SCCardDepict], [CardPass], [CardAdjust], [CardUnion], [SCCardBan] FROM [YGODATA] Order By [CardID]", con);
            OleDbCommand dc = new OleDbCommand("Select * FROM [YGODATA] Order By [CardID]", con);
    
            OleDbDataReader reader = dc.ExecuteReader();
            reader.Read();

            int i = 0;
            while (reader.Read())
            {
                cards.Add(ParseCard(reader));
                i++;
                processchanged.Invoke(total, i);
            }

            reader.Close();
            con.Close();
            return (CardDescription[])cards.ToArray(typeof(CardDescription));

        }

        private string GetFieldString(OleDbDataReader reader, string fieldname)
        {
            try
            {
                Object field = reader[fieldname];
                if (field == null)
                    return "";
                else
                    return field.ToString().Trim();
            }
            catch
            {
                return "";
            }
        }

        private CardDescription ParseCard(OleDbDataReader reader)
        {
            CardDescription card = new CardDescription();
            card.ID = int.Parse(GetFieldString(reader, "CardID")) - 1;
            card.japName = GetFieldString(reader, "JPCardName");
            card.name = GetFieldString(reader, "SCCardName");
            card.enName = GetFieldString(reader, "ENCardName");
            card.sCardType = GetFieldString(reader, "SCCardType");
            card.tribe = GetFieldString(reader, "SCCardRace");
            card.package = GetFieldString(reader, "CardBagNum");
            card.element = GetFieldString(reader, "SCCardAttribute");
            try
            {
                card.level = int.Parse(GetFieldString(reader, "CardStarNum"));
            }
            catch
            {
                card.level = -1;
            }
            card.infrequence = GetFieldString(reader, "SCCardRare");
            card.atk = GetFieldString(reader, "CardAtk");
            card.def = GetFieldString(reader, "CardDef");
            card.effect = GetFieldString(reader, "SCCardDepict").Replace("　", "");
            card.cheatcode = GetFieldString(reader, "CardPass");
            card.adjust = GetFieldString(reader, "CardAdjust");
            card.associate = GetFieldString(reader, "CardUnion");
            card.oldName = GetFieldString(reader, "CardOnceName");
            card.shortName = GetFieldString(reader, "CardAbbrName");
            string l = GetFieldString(reader, "SCCardBan");

            switch (l)
            {
                case "无限制":
                    card.limit = 3;
                    break;
                case "禁止卡":
                    card.limit = 0;
                    break;
                case "限制卡":
                    card.limit = 1;
                    break;
                case "准限制卡":
                    card.limit = 2;
                    break;
                default:
                    card.limit = -4;
                    break;
            }

            l = GetFieldString(reader, "CardCamp").Trim();
            switch (l)
            {
                case "TCG、OCG":
                    card.cardCamp = CardCamp.BothOT;
                    break;
                case "TCG":
                    card.cardCamp = CardCamp.TCG;
                    break;
                case "OCG":
                    card.cardCamp = CardCamp.OCG;
                    break;
                case "DIY":
                    card.cardCamp = CardCamp.DIY;
                    break;
            }

            if (string.Equals(card.atk, "9999"))
            {
                card.atk = "?";
                card.atkValue = -1;
            }
            else
            {
                try
                {
                    card.atkValue = int.Parse(card.atk);
                }
                catch
                {
                    card.atkValue = -2;
                }
            }
            if (string.Equals(card.def, "9999"))
            {
                card.def = "?";
                card.defValue = -1;
            }
            else
            {
                try
                {
                    card.defValue = int.Parse(card.def);
                }
                catch
                {
                    card.defValue = -2;
                }
            }

            if (string.Equals(card.sCardType, "普通怪兽"))
                card.sCardType = "通常怪兽";
            card.iCardType = CardDescription.CardTypeMapper(card.sCardType);

            string sdcardtype = GetFieldString(reader, "SCDCardType").Trim();
            if (sdcardtype.Length > 1)
                if (!string.Equals(card.effect.Substring(0, sdcardtype.Length), sdcardtype))
                    card.effect = sdcardtype + "：" + card.effect;

            string seffect = GetFieldString(reader, "CardEfficeType");
            string[] ss = seffect.Split(new char[] { ',', '，' });
            StringBuilder sb = new StringBuilder();
            foreach (string s in ss)
            {
                string s2 = (string)ht[s];
                sb.Append(s2);
                sb.Append(",");
            }
            if (sb.Length > 0)
                sb.Length--;
            card.effectType = sb.ToString();

            return card;
        }
    }

    /* 追加YGOPRO的同名卡和效果分类数据 */
    public class YGOPROIncreasedReader
    {

        static string[] EffectTypeDescription = new string[] {
            "魔陷破坏",	//1
            "怪兽破坏",	//2
            "卡片除外",	//3
            "送去墓地",	//4
            "返回手牌",	//5
            "返回卡组",	//6
            "手牌破坏",	//7
            "卡组破坏",	//8
            "抽卡辅助",	//9
            "卡组检索",	//10
            "卡片回收",	//11
            "表示形式",	//12
            "控制权",	//13
            "攻守变化",	//14
            "贯通伤害",	//15
            "多次攻击",	//16
            "攻击限制",	//17
            "直接攻击",	//18
            "特殊召唤",	//19
            "衍生物",	//20
            "种族相关",	//21
            "属性相关",	//22
            "LP伤害",	//23
            "LP回复",	//24
            "破坏耐性",	//25
            "效果耐性",	//26
            "指示物",	//27
            "赌博相关",	//28
            "融合相关",	//29
            "同调相关",	//30
            "XYZ相关",	//31
            "效果无效",	//32
        };

        public CardDescription[] Read(string filename, CardDescription[] oricards)
        {
            return Read(filename, oricards, null);
        }

        public CardDescription[] Read(string filename, CardDescription[] oricards, ReadProcessChangedInvoker processchanged)
        {
            ArrayList cards = new ArrayList(oricards.Length);

            //连接数据库
            SQLiteConnection con = new SQLiteConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data source=" + filename + ";Persist Security Info=False;");
            con.Open();

            //读取同名卡数据
            Hashtable htCheatCode = new Hashtable();

            SQLiteCommand dcc = new SQLiteCommand(@"select t3.*, t4.[alias_list] as alias_list from
  (select t2.[id],case t2.[alias] when 0 then t2.[id] else t2.[alias] end as alias from datas t2) as t3
left join
  (select t1.[alias], t1.[alias] || ',' || group_concat(t1.[id]) as alias_list from datas t1 where t1.[alias] <> 0 group   by t1.[alias]) as t4
on t3.[alias] = t4.[alias]
where not t4.[alias_list] is null", con);
            SQLiteDataReader creader = null;
            try
            {
                creader = dcc.ExecuteReader();

                while (creader.Read())
                {
                    string cheatcode = GetFieldString(creader, "id").PadLeft(8, '0');
                    string cheatcodelist = GetFieldString(creader, "alias_list");
                    htCheatCode.Add(cheatcode, cheatcodelist);
                }
                creader.Close();
            }
            catch
            {

            }

            //读取YGOPRO的效果分类数据
            Hashtable htEffectType = new Hashtable();

            dcc = new SQLiteCommand("Select * FROM [datas]", con);
            creader = null;
            try
            {
                creader = dcc.ExecuteReader();

                while (creader.Read())
                {
                    string cheatcode = GetFieldString(creader, "id").PadLeft(8, '0');
                    int Effect = int.Parse(creader["category"].ToString());
                    htEffectType.Add(cheatcode, Effect);
                }
                creader.Close();
            }
            catch
            {

            }

            //更新同名卡数据和效果分类数据
            for (int i = 0; i < oricards.Length; i++)
            {
                CardDescription card = oricards[i];
                card.effectType = "";
                StringBuilder sb = new StringBuilder();

                int categorynum = 0;
                if (htEffectType.ContainsKey(card.cheatcode))
                    categorynum = (int)htEffectType[card.cheatcode];
                if (categorynum != 0)
                {
                    int index = 0;
                    int num;
                    for (num = 1; index < 0x20; num = num << 1)
                    {
                        if ((num & categorynum) != 0L)
                        {
                            sb.Append(EffectTypeDescription[index]);
                            sb.Append(",");
                        }
                        index++;
                    }

                    if (sb.Length > 0)
                        sb.Length--;
                    card.effectType = sb.ToString();
                }

                if (htCheatCode.ContainsKey(card.cheatcode))
                {
                    string[] ccl = ((string)htCheatCode[card.cheatcode]).Split(',');
                    sb = new StringBuilder();
                    for (int j = 0; j < ccl.Length; j++)
                    {
                        sb.Append(ccl[j].PadLeft(8, '0'));
                        sb.Append(',');
                    }
                    sb.Length--;
                    card.aliasList = sb.ToString();
                }

                cards.Add(card);
            }

            return (CardDescription[])cards.ToArray(typeof(CardDescription));
        }

        private string GetFieldString(SQLiteDataReader reader, string fieldname)
        {
            try
            {
                Object field = reader[fieldname];
                if (field == null)
                    return "";
                else
                    return field.ToString().Trim();
            }
            catch
            {
                return "";
            }
        }
    }

    /* 导入安卓卡查数据 */
    public class AndroidReader : CardsReader
    {
        private Hashtable ht = null;

        public override CardDescription[] Read(string filename)
        {
            return Read(filename, null);
        }

        public override CardDescription[] Read(string filename, ReadProcessChangedInvoker processchanged)
        {
            ArrayList cards = new ArrayList(MinCapacity);


            //连接数据库
            SQLiteConnection con = new SQLiteConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data source=" + filename + ";Persist Security Info=False;");
            con.Open();

            //统计记录数
            SQLiteCommand dcc = new SQLiteCommand("Select Count(*) as iCount FROM [YGODATA]", con);
            SQLiteDataReader creader = dcc.ExecuteReader();
            creader.Read();
            int total = int.Parse(creader["iCount"].ToString());
            creader.Close();

            //读入卡片数据 
            //OleDbCommand dc = new OleDbCommand("Select [CardID], [JPCardName], [SCCardName], [ENCardName], [SCCardType], [SCCardRace], [CardBagNum], [SCCardAttribute], [CardStarNum], [SCCardRare], [CardAtk], [CardDef], [SCCardDepict], [CardPass], [CardAdjust], [CardUnion], [SCCardBan] FROM [YGODATA] Order By [CardID]", con);
            SQLiteCommand dc = new SQLiteCommand("Select * FROM [YGODATA] Order By [id]", con);

            SQLiteDataReader reader = dc.ExecuteReader();

            int i = 0;
            while (reader.Read())
            {
                cards.Add(ParseCard(reader));
                i++;
                processchanged.Invoke(total, i);
            }

            reader.Close();
            con.Close();
            return (CardDescription[])cards.ToArray(typeof(CardDescription));

        }

        private string GetFieldString(SQLiteDataReader reader, string fieldname)
        {
            try
            {
                Object field = reader[fieldname];
                if (field == null)
                    return "";
                else
                    return field.ToString().Trim();
            }
            catch
            {
                return "";
            }
        }

        private CardDescription ParseCard(SQLiteDataReader reader)
        {
            CardDescription card = new CardDescription();
            card.ID = int.Parse(GetFieldString(reader, "id"));
            card.japName = GetFieldString(reader, "japName");
            card.name = GetFieldString(reader, "name");
            card.enName = GetFieldString(reader, "enName");
            card.sCardType = GetFieldString(reader, "SCardType");
            card.tribe = GetFieldString(reader, "tribe");
            card.package = GetFieldString(reader, "package");
            card.element = GetFieldString(reader, "element");
            try
            {
                card.level = int.Parse(GetFieldString(reader, "level"));
            }
            catch
            {
                card.level = -1;
            }
            try
            {
                card.pendulumL = int.Parse(GetFieldString(reader, "pendulumL"));
                card.pendulumR = int.Parse(GetFieldString(reader, "pendulumR"));
            }
            catch
            {
                card.pendulumL = 0;
                card.pendulumR = 0;
            }
            card.infrequence = GetFieldString(reader, "infrequence");
            card.atk = GetFieldString(reader, "atk");
            card.def = GetFieldString(reader, "def");
            card.effect = GetFieldString(reader, "effect").Replace("　", "");
            card.cheatcode = GetFieldString(reader, "cheatcode");
            card.adjust = GetFieldString(reader, "adjust");
            card.oldName = GetFieldString(reader, "oldName");
            card.shortName = GetFieldString(reader, "shortName");
            string l = GetFieldString(reader, "ban");

            switch (l)
            {
                case "无限制":
                    card.limit = 3;
                    break;
                case "禁止卡":
                    card.limit = 0;
                    break;
                case "限制卡":
                    card.limit = 1;
                    break;
                case "准限制卡":
                    card.limit = 2;
                    break;
                default:
                    card.limit = -4;
                    break;
            }

            l = GetFieldString(reader, "CardCamp").Trim();
            switch (l)
            {
                case "TCG、OCG":
                    card.cardCamp = CardCamp.BothOT;
                    break;
                case "TCG":
                    card.cardCamp = CardCamp.TCG;
                    break;
                case "OCG":
                    card.cardCamp = CardCamp.OCG;
                    break;
                case "DIY":
                    card.cardCamp = CardCamp.DIY;
                    break;
            }

            if (string.Equals(card.atk, "9999"))
            {
                card.atk = "?";
                card.atkValue = -1;
            }
            else
            {
                try
                {
                    card.atkValue = int.Parse(card.atk);
                }
                catch
                {
                    card.atkValue = -2;
                }
            }
            if (string.Equals(card.def, "9999"))
            {
                card.def = "?";
                card.defValue = -1;
            }
            else
            {
                try
                {
                    card.defValue = int.Parse(card.def);
                }
                catch
                {
                    card.defValue = -2;
                }
            }

            if (string.Equals(card.sCardType, "普通怪兽"))
                card.sCardType = "通常怪兽";
            card.iCardType = CardDescription.CardTypeMapper(card.sCardType);

            string sdcardtype = GetFieldString(reader, "CardDType").Trim();
            if (sdcardtype.Length > 1)
                if (!string.Equals(card.effect.Substring(0, sdcardtype.Length), sdcardtype))
                    card.effect = sdcardtype + "：" + card.effect;

            return card;
        }
    }
}
