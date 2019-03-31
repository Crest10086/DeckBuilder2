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

            //�����ı�
            for (int i = 0; i < infos.Length; i++)
            {
                //���ַ���������
                if (infos[i].Length > 0)
                {
                    //���˿�ʼ�Ŀո�
                    while (infos[i][0] == ' ')
                    {
                        infos[i] = infos[i].Substring(1, infos[i].Length - 1);
                        if (infos[i].Length == 0)
                            break;
                    }

                    //ת��˫����
                    infos[i] = infos[i].Replace("\"", "\"\"");
                }
            }

            //�Ա����
            card.ID = int.Parse(infos[12]);

            //��Ƭ����
            card.sCardType = infos[8] + infos[9] + infos[0];
            if (string.Equals(card.sCardType, "��ͨ����"))
                card.sCardType = "ͨ������";

            //��������
            card.name = infos[1];

            //�Ǽ�
            try
            {
                card.level = int.Parse(infos[2]);
            }
            catch
            {
                card.level = -1;
            }

            //����
            card.element = infos[3];

            //����
            card.tribe = infos[4];

            //����
            card.atk = infos[5];
            if (card.element.Equals(""))
            {
                card.atk = "";
                card.atkValue = -10000;
            }
            else if (card.atk.Equals("?") || card.atk.Equals("��"))
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

            //����
            card.def = infos[6];
            if (card.element.Equals(""))
            {
                card.def = "";
                card.defValue = -10000;
            }
            else if (card.def.Equals("?") || card.def.Equals("��"))
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

            //7δ֪
            //if (!infos[7].Equals(""))
            {
                //MessageBox.Show(infos[7]);
            }

            //����
            card.package = infos[10];

            //Ч��
            card.effect = infos[11];

            //��������
            card.japName = infos[13];

            //������
            card.infrequence = infos[14];

            //����
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

            //�����ı�
            for (int i = 0; i < infos.Length; i++)
            {
                //���ַ���������
                if (infos[i].Length > 0)
                {
                    //���˿�ʼ�Ŀո�
                    while (infos[i][0] == ' ')
                    {
                        infos[i] = infos[i].Substring(1, infos[i].Length - 1);
                        if (infos[i].Length == 0)
                            break;
                    }

                    //ת��˫����
                    infos[i] = infos[i].Replace("\"", "\"\"");
                }
            }

            //��Ƭ����
            string s = infos[0].Substring(infos[0].Length-2, 2);
            if (s == "����")
            {
                card.sCardType = infos[0];
                for (int i = 1; i < 7; i++)
                {
                    s = infos[i].Substring(0, 3);
                    string s2 = infos[i].Substring(3);
                    switch (s)
                    {
                        case "���壺":
                            card.tribe = s2;
                            break;
                        case "���ԣ�":
                            card.element = s2;
                            break;
                        case "�Ǽ���":
                            try
                            {
                                card.level = int.Parse(s2);
                            }
                            catch
                            {
                                card.level = -1;
                            }
                            break;
                        case "������":
                            //����
                            card.atk = s2;
                            if (card.atk.Equals("?") || card.atk.Equals("��"))
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
                        case "������":
                            card.def = s2;
                            if (card.def.Equals("?") || card.def.Equals("��"))
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
                        case "Ч����":
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
                if (s == "ħ�����ࣺ" || s == "�������ࣺ")
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
                case "TCG��OCG":
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
     * ��Ϊ���㿨�鲻�ٸ��£�������ʱ�����ṩ֧��
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
            card.name = doc.GetField("������").StringValue();
            card.japName = doc.GetField("������").StringValue();
            card.enName = doc.GetField("Ӣ����").StringValue();
            card.sCardType = doc.GetField("��Ƭ����").StringValue();
            if (string.Equals(card.sCardType.Substring(2, 2), "����"))
            {
                card.level = int.Parse(doc.GetField("��Ƭ�Ǽ�").StringValue());
                card.element = doc.GetField("��Ƭ����").StringValue();
                card.tribe = doc.GetField("��Ƭ����").StringValue();
                card.atk = doc.GetField("������").StringValue();
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
                card.def = doc.GetField("������").StringValue();
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
            card.effectType = doc.GetField("��ϸ����").StringValue();
            card.effect = doc.GetField("Ч��").StringValue();
            card.infrequence = doc.GetField("����̶�").StringValue();
            card.package = doc.GetField("����").StringValue();
            card.cheatcode = doc.GetField("CardPass").StringValue();
            card.adjust = doc.GetField("����").StringValue();
            card.associate = doc.GetField("������Ƭ").StringValue();
            string l = doc.GetField("��������").StringValue();
            switch (l)
            {
                case "������":
                    card.limit = 3;
                    break;
                case "��ֹ��":
                    card.limit = 0;
                    break;
                case "���ƿ�":
                    card.limit = 1;
                    break;
                case "׼���ƿ�":
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


            //�������ݿ�
            OleDbConnection con = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data source=" + filename + ";Persist Security Info=False;Jet OLEDB:Database Password=paradisefox@sohu.com;");
            con.Open();

            //��ȡЧ�������б�
            ht = new Hashtable();

            OleDbCommand dcc = new OleDbCommand("Select * FROM [YGOEFFECT]", con);
            OleDbDataReader creader = null;
            try
            {
                //������ݿ��д���Ч�������б������ȶ�ȡ���ݿ��еļ�¼
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

            //�Ӳ����ļ��ﲹ��Ч�������б�
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

            //ͳ�Ƽ�¼��
            dcc = new OleDbCommand("Select Count(*) as iCount FROM [YGODATA]", con);
            creader = dcc.ExecuteReader();
            creader.Read();
            int total = int.Parse(creader["iCount"].ToString());
            creader.Close();

            //���뿨Ƭ���� 
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
            card.effect = GetFieldString(reader, "SCCardDepict").Replace("��", "");
            card.cheatcode = GetFieldString(reader, "CardPass");
            card.adjust = GetFieldString(reader, "CardAdjust");
            card.associate = GetFieldString(reader, "CardUnion");
            card.oldName = GetFieldString(reader, "CardOnceName");
            card.shortName = GetFieldString(reader, "CardAbbrName");
            string l = GetFieldString(reader, "SCCardBan");

            switch (l)
            {
                case "������":
                    card.limit = 3;
                    break;
                case "��ֹ��":
                    card.limit = 0;
                    break;
                case "���ƿ�":
                    card.limit = 1;
                    break;
                case "׼���ƿ�":
                    card.limit = 2;
                    break;
                default:
                    card.limit = -4;
                    break;
            }

            l = GetFieldString(reader, "CardCamp").Trim();
            switch (l)
            {
                case "TCG��OCG":
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

            if (string.Equals(card.sCardType, "��ͨ����"))
                card.sCardType = "ͨ������";
            card.iCardType = CardDescription.CardTypeMapper(card.sCardType);

            string sdcardtype = GetFieldString(reader, "SCDCardType").Trim();
            if (sdcardtype.Length > 1)
                if (!string.Equals(card.effect.Substring(0, sdcardtype.Length), sdcardtype))
                    card.effect = sdcardtype + "��" + card.effect;

            string seffect = GetFieldString(reader, "CardEfficeType");
            string[] ss = seffect.Split(new char[] { ',', '��' });
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

    /* ׷��YGOPRO��ͬ������Ч���������� */
    public class YGOPROIncreasedReader
    {

        static string[] EffectTypeDescription = new string[] {
            "ħ���ƻ�",	//1
            "�����ƻ�",	//2
            "��Ƭ����",	//3
            "��ȥĹ��",	//4
            "��������",	//5
            "���ؿ���",	//6
            "�����ƻ�",	//7
            "�����ƻ�",	//8
            "�鿨����",	//9
            "�������",	//10
            "��Ƭ����",	//11
            "��ʾ��ʽ",	//12
            "����Ȩ",	//13
            "���ر仯",	//14
            "��ͨ�˺�",	//15
            "��ι���",	//16
            "��������",	//17
            "ֱ�ӹ���",	//18
            "�����ٻ�",	//19
            "������",	//20
            "�������",	//21
            "�������",	//22
            "LP�˺�",	//23
            "LP�ظ�",	//24
            "�ƻ�����",	//25
            "Ч������",	//26
            "ָʾ��",	//27
            "�Ĳ����",	//28
            "�ں����",	//29
            "ͬ�����",	//30
            "XYZ���",	//31
            "Ч����Ч",	//32
        };

        public CardDescription[] Read(string filename, CardDescription[] oricards)
        {
            return Read(filename, oricards, null);
        }

        public CardDescription[] Read(string filename, CardDescription[] oricards, ReadProcessChangedInvoker processchanged)
        {
            ArrayList cards = new ArrayList(oricards.Length);

            //�������ݿ�
            SQLiteConnection con = new SQLiteConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data source=" + filename + ";Persist Security Info=False;");
            con.Open();

            //��ȡͬ��������
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

            //��ȡYGOPRO��Ч����������
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

            //����ͬ�������ݺ�Ч����������
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

    /* ���밲׿�������� */
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


            //�������ݿ�
            SQLiteConnection con = new SQLiteConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data source=" + filename + ";Persist Security Info=False;");
            con.Open();

            //ͳ�Ƽ�¼��
            SQLiteCommand dcc = new SQLiteCommand("Select Count(*) as iCount FROM [YGODATA]", con);
            SQLiteDataReader creader = dcc.ExecuteReader();
            creader.Read();
            int total = int.Parse(creader["iCount"].ToString());
            creader.Close();

            //���뿨Ƭ���� 
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
            card.effect = GetFieldString(reader, "effect").Replace("��", "");
            card.cheatcode = GetFieldString(reader, "cheatcode");
            card.adjust = GetFieldString(reader, "adjust");
            card.oldName = GetFieldString(reader, "oldName");
            card.shortName = GetFieldString(reader, "shortName");
            string l = GetFieldString(reader, "ban");

            switch (l)
            {
                case "������":
                    card.limit = 3;
                    break;
                case "��ֹ��":
                    card.limit = 0;
                    break;
                case "���ƿ�":
                    card.limit = 1;
                    break;
                case "׼���ƿ�":
                    card.limit = 2;
                    break;
                default:
                    card.limit = -4;
                    break;
            }

            l = GetFieldString(reader, "CardCamp").Trim();
            switch (l)
            {
                case "TCG��OCG":
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

            if (string.Equals(card.sCardType, "��ͨ����"))
                card.sCardType = "ͨ������";
            card.iCardType = CardDescription.CardTypeMapper(card.sCardType);

            string sdcardtype = GetFieldString(reader, "CardDType").Trim();
            if (sdcardtype.Length > 1)
                if (!string.Equals(card.effect.Substring(0, sdcardtype.Length), sdcardtype))
                    card.effect = sdcardtype + "��" + card.effect;

            return card;
        }
    }
}
