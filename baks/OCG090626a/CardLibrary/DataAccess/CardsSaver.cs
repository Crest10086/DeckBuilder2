using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Lucene.Net;
using Lucene.Net.Index;
using Lucene.Net.Documents;
using Lucene.Net.Analysis.Standard;
using Tools;

namespace BaseCardLibrary.DataAccess
{
    public interface CardsSaver
    {
         void Save(string filename, CardDescription[] cards);
    }

    public class LuceneSaver:CardsSaver
    {
        public virtual void Save(string dirname, CardDescription[] cards)
        {
            if (dirname == null || dirname.Length <= 0)
                return;

            if (!Directory.Exists(dirname))
                Directory.CreateDirectory(dirname);

            IndexWriter writer = new IndexWriter(dirname, AnalyzerFactory.GetAnalyzer(), true);
            
            for (int i = 0; i < cards.Length; i++)
            {
                CardDescription card = cards[i];
                Document doc = new Document();

                Field ID = new Field("ID", card.ID.ToString(), Field.Store.YES, Field.Index.UN_TOKENIZED);
                Field name = new Field("name", card.name, Field.Store.YES, Field.Index.TOKENIZED);
                Field japName = new Field("japName", card.japName, Field.Store.YES, Field.Index.TOKENIZED);
                Field enName = new Field("enName", card.enName, Field.Store.YES, Field.Index.TOKENIZED);
                Field oldName = new Field("oldName", card.oldName.Replace("，", " 龴 "), Field.Store.NO, Field.Index.TOKENIZED);
                Field shortName = new Field("shortName", card.shortName.Replace("，", " 龴 "), Field.Store.NO, Field.Index.TOKENIZED);
                Field oldName2 = new Field("oldName2", card.oldName, Field.Store.YES, Field.Index.NO_NORMS);
                Field shortName2 = new Field("shortName2", card.shortName, Field.Store.YES, Field.Index.NO_NORMS);

                Field pyname = new Field("pyname", GetPingyin.convertline(card.name), Field.Store.NO, Field.Index.TOKENIZED);
                Field pyoldName = new Field("pyoldName", GetPingyin.convertline(card.oldName), Field.Store.NO, Field.Index.TOKENIZED);
                Field pyshortName = new Field("pyshortName", GetPingyin.convertline(card.shortName), Field.Store.NO, Field.Index.TOKENIZED);

                Field cardType = new Field("cardType", card.sCardType, Field.Store.YES, Field.Index.TOKENIZED);
                Field effecfType = new Field("effecfType", card.effecfType, Field.Store.YES, Field.Index.TOKENIZED);
                Field effect = new Field("effect", card.effect, Field.Store.YES, Field.Index.TOKENIZED);
                Field infrequence = new Field("infrequence", card.infrequence, Field.Store.YES, Field.Index.TOKENIZED);
                Field package = new Field("package", card.package, Field.Store.YES, Field.Index.TOKENIZED);
                Field limit = new Field("limit", card.limit.ToString(), Field.Store.YES, Field.Index.TOKENIZED);
                Field cheatcode = new Field("cheatcode", card.cheatcode, Field.Store.YES, Field.Index.TOKENIZED);
                Field adjust = new Field("adjust", card.adjust, Field.Store.YES, Field.Index.NO);
                Field associate = new Field("associate", card.associate, Field.Store.YES, Field.Index.TOKENIZED);

                Field name2 = new Field("name2", card.name, Field.Store.NO, Field.Index.NO_NORMS);
                Field japName2 = new Field("japName2", card.japName, Field.Store.NO, Field.Index.NO_NORMS);
                Field enName2 = new Field("enName2", card.enName, Field.Store.NO, Field.Index.NO_NORMS);

                Field cardCamp = new Field("cardCamp", card.cardCamp.ToString(), Field.Store.YES, Field.Index.TOKENIZED);

                string ct = "";
                if (card.sCardType.Length > 0)
                {
                    switch (card.sCardType.Substring(2, 2))
                    {
                        case "怪兽":
                            ct = "1";
                            break;
                        case "魔法":
                            ct = "2";
                            break;
                        case "陷阱":
                            ct = "3";
                            break;
                        default:
                            ct = "0";
                            break;
                    }
                    switch (card.sCardType.Substring(0, 2))
                    {
                        case "通常":
                            ct += "0";
                            break;
                        case "普通":
                            ct += "1";
                            break;
                        case "速攻":
                            ct += "2";
                            break;
                        case "装备":
                            ct += "3";
                            break;
                        case "仪式":
                            ct += "4";
                            break;
                        case "场地":
                            ct += "5";
                            break;
                        case "反击":
                            ct += "6";
                            break;
                        case "永续":
                            ct += "7";
                            break;
                        case "效果":
                            ct += "8";
                            break;
                        case "融合":
                            ct += "9";
                            break;
                        default:
                            ct += "X";
                            break;
                    }
                }
                Field cardType2 = new Field("cardType2", ct, Field.Store.NO, Field.Index.NO_NORMS);

                Field level = null;
                Field element = null;
                Field tribe = null;
                Field atk = null;
                Field atkValue = null;
                Field def = null;
                Field defValue = null;
                if (ct[0] == '1')
                {
                    level = new Field("level", string.Format("{0:D2}", card.level), Field.Store.YES, Field.Index.UN_TOKENIZED);
                    element = new Field("element", card.element, Field.Store.YES, Field.Index.NO_NORMS);
                    tribe = new Field("tribe", card.tribe, Field.Store.YES, Field.Index.NO_NORMS);
                    atk = new Field("atk", card.atk, Field.Store.YES, Field.Index.UN_TOKENIZED);
                    atkValue = new Field("atkValue", string.Format("{0:D4}", card.atkValue), Field.Store.YES, Field.Index.UN_TOKENIZED);
                    def = new Field("def", card.def, Field.Store.YES, Field.Index.UN_TOKENIZED);
                    defValue = new Field("defValue", string.Format("{0:D4}", card.defValue), Field.Store.YES, Field.Index.UN_TOKENIZED);
                }
                else
                {
                    level = new Field("level", "-01", Field.Store.YES, Field.Index.UN_TOKENIZED);
                    element = new Field("element", "", Field.Store.YES, Field.Index.NO_NORMS);
                    tribe = new Field("tribe", "", Field.Store.YES, Field.Index.NO_NORMS);
                    atk = new Field("atk", "", Field.Store.YES, Field.Index.UN_TOKENIZED);
                    atkValue = new Field("atkValue", "-0002", Field.Store.YES, Field.Index.UN_TOKENIZED);
                    def = new Field("def", "", Field.Store.YES, Field.Index.UN_TOKENIZED);
                    defValue = new Field("defValue", "-0002", Field.Store.YES, Field.Index.UN_TOKENIZED);
                }

                doc.Add(ID);
                doc.Add(name);
                doc.Add(name2);
                doc.Add(japName);
                doc.Add(japName2);
                doc.Add(enName);
                doc.Add(oldName);
                doc.Add(shortName);
                doc.Add(oldName2);
                doc.Add(shortName2);

                doc.Add(pyname);
                doc.Add(pyoldName);
                doc.Add(pyshortName);

                doc.Add(cardType);
                doc.Add(cardType2);
                doc.Add(level);
                doc.Add(element);
                doc.Add(tribe);
                doc.Add(effecfType);
                doc.Add(atk);
                doc.Add(atkValue);
                doc.Add(def);
                doc.Add(defValue);
                doc.Add(effect);
                doc.Add(infrequence);
                doc.Add(package);
                doc.Add(limit);
                doc.Add(cheatcode);
                doc.Add(adjust);
                doc.Add(associate);
                doc.Add(cardCamp);

                if (card.iCardtype == 1)
                    doc.SetBoost(1);
                else
                    doc.SetBoost(2);

                writer.AddDocument(doc);
            }
            writer.Optimize();
            writer.Close();

            if (dirname[dirname.Length - 1] != '\\')
                dirname += "\\";
            string files = "";
            foreach (string s in Directory.GetFiles(dirname))
            {
                string ss = s.Substring(s.LastIndexOf('\\') + 1);
                if (!string.Equals(ss, "list.txt", StringComparison.OrdinalIgnoreCase))
                    files += ss + "\r\n";
            }
            File.WriteAllText(dirname + "list.txt" ,files, Encoding.UTF8);
        }
    }

    public class AllCardsSaver:CardsSaver
    {
        public virtual void Save(string filename, CardDescription[] cards)
        {
            StreamWriter sw = new StreamWriter(filename, false, System.Text.Encoding.Default);
            CardDescription card = null;

            for (int i = 0; i < cards.Length-1; i++)
            {
                card = cards[i];
                string s = card.name;
                sw.WriteLine("[" + s + "]");
                s = card.GetSimpleInfo();
                s = s.Replace("\"\"", "\"");
                sw.WriteLine(s);
            }
            card = cards[cards.Length - 1];
            string ss = card.name;
            sw.WriteLine("[" + ss + "]");
            ss = card.GetSimpleInfo();
            ss = ss.Replace("\"\"", "\"");
            sw.Write(ss);
            sw.Close();
        }
    }
}
