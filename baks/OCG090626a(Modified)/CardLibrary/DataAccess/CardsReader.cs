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
                lbl.Text = "��ȡ������..." + lc.ToString();
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
                lbl.Text = "��ȡ������..." + lc.ToString();
                lc++;
                Application.DoEvents();
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
            string s = infos[0].Substring(2, 2);
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
                lbl.Text = "��ȡ������..." + lc.ToString();
                lc++;
                Application.DoEvents();
                
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
            card.effecfType = doc.GetField("��ϸ����").StringValue();
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

    public class YFCCReader : CardsReader
    {
        public override CardDescription[] Read(string filename, ProgressBar process, ToolStripStatusLabel lbl)
        {
            ArrayList cards = new ArrayList(MinCapacity);


            //�������ݿ�
            OleDbConnection con = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data source=" + filename + ";Persist Security Info=False;Jet OLEDB:Database Password=paradisefox@sohu.com;");
            con.Open();

            //���뿨Ƭ���� 
            //OleDbCommand dc = new OleDbCommand("Select [CardID], [JPCardName], [SCCardName], [ENCardName], [SCCardType], [SCCardRace], [CardBagNum], [SCCardAttribute], [CardStarNum], [SCCardRare], [CardAtk], [CardDef], [SCCardDepict], [CardPass], [CardAdjust], [CardUnion], [SCCardBan] FROM [YGODATA] Order By [CardID]", con);
            OleDbCommand dc = new OleDbCommand("Select * FROM [YGODATA] Order By [CardID]", con);
            OleDbDataReader reader = dc.ExecuteReader();
            reader.Read();
            int lc = 0;
            while (reader.Read())
            {
                cards.Add(ParseCard(reader));
                lbl.Text = "��ȡ������..." + lc.ToString();
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
            card.iCardtype = CardTypeMapper.Mapper(card.sCardType);

            string sdcardtype = GetFieldString(reader, "SCDCardType").Trim();
            if (sdcardtype.Length > 1)
                if (!string.Equals(card.effect.Substring(0, sdcardtype.Length), sdcardtype))
                    card.effect = sdcardtype + "��" + card.effect;

            return card;
        }
    }
}
