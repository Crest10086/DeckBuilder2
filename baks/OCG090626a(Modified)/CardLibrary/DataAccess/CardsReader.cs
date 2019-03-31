using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.OleDb;
using Lucene.Net.Search;
using Lucene.Net.Documents;
using System.Windows.Forms;

namespace BaseCardLibrary.DataAccess
{

    public abstract class CardsReader
    {
        public const int MinCapacity = 4000;
        public virtual CardDescription[] Read(Lucene.Net.Store.Directory dir, ProgressBar process, ToolStripStatusLabel lbl)
        {
            return new CardDescription[] { };
        }
        public virtual CardDescription[] Read(string source, ProgressBar process, ToolStripStatusLabel lbl)
        {
            return new CardDescription[] { };
        }
    }

    public class yxwpReader: CardsReader
    {
        public override CardDescription[] Read(string filename, ProgressBar process, ToolStripStatusLabel lbl)
        {
            StreamReader sr = null;
            ArrayList cards = new ArrayList(MinCapacity);

            sr = new StreamReader(filename, Encoding.GetEncoding("GB2312"));
            int lc = 0;
            while (!sr.EndOfStream)
            {
                string s = sr.ReadLine();
                cards.Add(ParseCard(s));
                lbl.Text = "读取数据中..." + lc.ToString();
                lc++;
                Application.DoEvents();
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
        public override CardDescription[] Read(string filename, ProgressBar process, ToolStripStatusLabel lbl)
        {
            StreamReader sr = null;
            ArrayList cards = new ArrayList(MinCapacity);

            sr = new StreamReader(filename, Encoding.GetEncoding("GB2312"));

            int i = 0;
            int lc = 0;
            while (!sr.EndOfStream)
            {
                string s = sr.ReadLine().Trim();
                if (s != "")
                {
                    CardDescription card = new CardDescription();
                    card.name = s.Substring(1, s.Length - 2);
                    card.ID = 60000 + i++;
                    card.cardCamp = CardCamp.DIY;
                    card.limit = -5;
                    s = sr.ReadLine().Trim();
                    cards.Add(ParseCard(card, s));
                }
                lbl.Text = "读取数据中..." + lc.ToString();
                lc++;
                Application.DoEvents();
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
            string s = infos[0].Substring(2, 2);
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
                }
            }
            card.iCardtype = CardTypeMapper.Mapper(card.sCardType);
            
            return card;
        }
    }

    public class LuceneReader : CardsReader
    {
        public override CardDescription[] Read(string dirname, ProgressBar process, ToolStripStatusLabel lbl)
        {
            if (dirname == null || dirname.Length <= 0)
                return null;

            if (!Directory.Exists(dirname))
                return null;

            if (dirname[dirname.Length - 1] != '\\')
                dirname += "\\";

            if (File.Exists(dirname + "list.txt"))
            {
                string[] files = File.ReadAllLines(dirname + "list.txt", Encoding.UTF8);

                foreach (string s in Directory.GetFiles(dirname))
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
                        File.Delete(dirname + ss);
                }
            }

            ArrayList cards = new ArrayList(MinCapacity);
            Query query = new MatchAllDocsQuery();
            Searcher searcher = new IndexSearcher(dirname);
            Hits hits = searcher.Search(query);

            int length = hits.Length();
            for (int i = 0; i < length; i++)
            {
                Document doc = hits.Doc(i);
                cards.Add(ParseCard(doc));
            }

            return (CardDescription[])cards.ToArray(typeof(CardDescription));
        }

        public override CardDescription[] Read(Lucene.Net.Store.Directory dir, ProgressBar process, ToolStripStatusLabel lbl)
        {
            ArrayList cards = new ArrayList(MinCapacity);
            Query query = new MatchAllDocsQuery();
            Searcher searcher = new IndexSearcher(dir);
            Hits hits = searcher.Search(query);

            int length = hits.Length();
            for (int i = 0; i < length; i++)
            {
                Document doc = hits.Doc(i);
                cards.Add(ParseCard(doc));
            }

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
            card.iCardtype = CardTypeMapper.Mapper(card.sCardType);
            if (card.iCardtype <= 3 || card.iCardtype == 6)
            {
                card.level = int.Parse(GetFieldString(doc, "level"));
                card.element = GetFieldString(doc, "element");
                card.tribe = GetFieldString(doc, "tribe");
                card.atk = GetFieldString(doc, "atk");
                card.atkValue = int.Parse(GetFieldString(doc, "atkValue"));
                card.def = GetFieldString(doc, "def");
                card.defValue = int.Parse(GetFieldString(doc, "defValue"));
            }
            card.effecfType = GetFieldString(doc, "effecfType");
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

    public class MiniCardXReader : CardsReader
    {
        public override CardDescription[] Read(string filename, ProgressBar process, ToolStripStatusLabel lbl)
        {
            ArrayList cards = new ArrayList(MinCapacity);
            Query query = new MatchAllDocsQuery();
            Searcher searcher = new IndexSearcher(filename);
            Hits hits = searcher.Search(query);

            int length = hits.Length();
            int lc = 0;
            for (int i = 0; i < length; i++)
            {
                Document doc = hits.Doc(i);
                cards.Add(ParseCard(doc));
                lbl.Text = "读取数据中..." + lc.ToString();
                lc++;
                Application.DoEvents();
                
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
            card.effecfType = doc.GetField("详细类型").StringValue();
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

    public class YFCCReader : CardsReader
    {
        public override CardDescription[] Read(string filename, ProgressBar process, ToolStripStatusLabel lbl)
        {
            ArrayList cards = new ArrayList(MinCapacity);


            //连接数据库
            OleDbConnection con = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data source=" + filename + ";Persist Security Info=False;Jet OLEDB:Database Password=paradisefox@sohu.com;");
            con.Open();

            //读入卡片数据 
            //OleDbCommand dc = new OleDbCommand("Select [CardID], [JPCardName], [SCCardName], [ENCardName], [SCCardType], [SCCardRace], [CardBagNum], [SCCardAttribute], [CardStarNum], [SCCardRare], [CardAtk], [CardDef], [SCCardDepict], [CardPass], [CardAdjust], [CardUnion], [SCCardBan] FROM [YGODATA] Order By [CardID]", con);
            OleDbCommand dc = new OleDbCommand("Select * FROM [YGODATA] Order By [CardID]", con);
            OleDbDataReader reader = dc.ExecuteReader();
            reader.Read();
            int lc = 0;
            while (reader.Read())
            {
                cards.Add(ParseCard(reader));
                lbl.Text = "读取数据中..." + lc.ToString();
                lc++;
                Application.DoEvents();
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
            card.effect = GetFieldString(reader, "SCCardDepict");
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
            card.iCardtype = CardTypeMapper.Mapper(card.sCardType);

            string sdcardtype = GetFieldString(reader, "SCDCardType").Trim();
            if (sdcardtype.Length > 1)
                if (!string.Equals(card.effect.Substring(0, sdcardtype.Length), sdcardtype))
                    card.effect = sdcardtype + "：" + card.effect;

            return card;
        }
    }
}
