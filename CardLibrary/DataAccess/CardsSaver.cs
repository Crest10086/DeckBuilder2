using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Lucene.Net;
using Lucene.Net.Index;
using Lucene.Net.Documents;
using Lucene.Net.Analysis.Standard;
using BaseCardLibrary.Common;
using MyTools;

namespace BaseCardLibrary.DataAccess
{
    public delegate void SaveProcessChangedInvoker(int total, int current);

    public interface CardsSaver
    {
         void Save(string filename, CardDescription[] cards);

         void Save(string filename, CardDescription[] cards, SaveProcessChangedInvoker processchanged);
    }

    public class LuceneSaver:CardsSaver
    {
        public virtual void Save(string dirname, CardDescription[] cards)
        {
            Save(dirname, cards, null);
        }

        public virtual void Save(string dirname, CardDescription[] cards, SaveProcessChangedInvoker processchanged)
        {
            if (dirname == null || dirname.Length <= 0)
                return;

            if (!Directory.Exists(dirname))
                Directory.CreateDirectory(dirname);

            Lucene.Net.Store.Directory dir = new Lucene.Net.Store.SimpleFSDirectory(new DirectoryInfo(dirname), new Lucene.Net.Store.SimpleFSLockFactory());
            IndexWriter writer = new IndexWriter(dir, AnalyzerFactory.GetAnalyzer(), true, IndexWriter.MaxFieldLength.UNLIMITED);

            writer.SetMaxBufferedDocs(100);
            writer.SetMergeFactor(100);


//========================================卡片种类映射表====================================
//
//	         通常  普通  速攻  装备  仪式  场地  反击  永续  效果  融合  同调  XYZ
// 1、怪兽	  0     0                 3                       4     5     6     7
// 2、魔法	  0     0     1     2     3     4           5
// 3、陷阱	  0     0                             1     5            
//
//==========================================================================================

            for (int i = 0; i < cards.Length; i++)
            {
                CardDescription card = cards[i];
                int ict = 0;
                if (card.sCardType.Length > 0)
                {
                    switch (card.sCardType.Substring(card.sCardType.Length-2, 2))
                    {
                        case "怪兽":
                            ict = 1;
                            break;
                        case "魔法":
                            ict = 2;
                            break;
                        case "陷阱":
                            ict = 3;
                            break;
                        default:
                            ict = 4;
                            break;
                    }
                    ict *= 10;

                    switch (card.sCardType.Substring(0, 2))
                    {
                        case "通常":
                            ict += 0;
                            break;
                        case "普通":
                            ict += 0;
                            break;
                        case "速攻":
                            ict += 1;
                            break;
                        case "装备":
                            ict += 2;
                            break;
                        case "仪式":
                            ict += 3;
                            break;
                        case "场地":
                            ict += 4;
                            break;
                        case "反击":
                            ict += 1;
                            break;
                        case "永续":
                            ict += 5;
                            break;
                        case "效果":
                            ict += 4;
                            break;
                        case "融合":
                            ict += 5;
                            break;
                        case "同调":
                            ict += 6;
                            break;
                        case "XY":
                            ict += 7;
                            break;
                        default:
                            ict += 9;
                            break;
                    }
                }


                Document doc = new Document();

                Field ID = new Field("ID", card.ID.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED);
                Field name = new Field("name", card.name, Field.Store.YES, Field.Index.ANALYZED);
                Field name2 = new Field("name2", card.name, Field.Store.NO, Field.Index.NOT_ANALYZED);
                Field japName = new Field("japName", card.japName, Field.Store.YES, Field.Index.ANALYZED);
                Field japName2 = new Field("japName2", card.japName, Field.Store.NO, Field.Index.NOT_ANALYZED);
                Field enName = new Field("enName", card.enName, Field.Store.YES, Field.Index.ANALYZED);
                Field enName2 = new Field("enName2", card.enName, Field.Store.NO, Field.Index.NOT_ANALYZED);
                Field oldName = new Field("oldName", card.oldName.Replace("，", " 龴 "), Field.Store.NO, Field.Index.ANALYZED);
                Field shortName = new Field("shortName", card.shortName.Replace("，", " 龴 "), Field.Store.NO, Field.Index.ANALYZED);
                Field oldName2 = new Field("oldName2", card.oldName, Field.Store.YES, Field.Index.NOT_ANALYZED);
                Field shortName2 = new Field("shortName2", card.shortName, Field.Store.YES, Field.Index.NOT_ANALYZED);

                Field pyname = new Field("pyname", GetPingyin.convertline(card.name), Field.Store.NO, Field.Index.ANALYZED);
                Field pyoldName = new Field("pyoldName", GetPingyin.convertline(card.oldName), Field.Store.NO, Field.Index.ANALYZED);
                Field pyshortName = new Field("pyshortName", GetPingyin.convertline(card.shortName), Field.Store.NO, Field.Index.ANALYZED);

                Field cardType = new Field("cardType", card.sCardType, Field.Store.YES, Field.Index.ANALYZED);
                Field cardType2 = new Field("cardType2", ict.ToString(), Field.Store.NO, Field.Index.NOT_ANALYZED);
                Field effectType = new Field("effectType", card.effectType, Field.Store.YES, Field.Index.ANALYZED);
                Field effect = new Field("effect", card.effect, Field.Store.YES, Field.Index.ANALYZED);
                Field infrequence = new Field("infrequence", card.infrequence, Field.Store.YES, Field.Index.ANALYZED);
                Field package = new Field("package", card.package, Field.Store.YES, Field.Index.ANALYZED);
                Field limit = new Field("limit", card.limit.ToString(), Field.Store.YES, Field.Index.ANALYZED);
                Field cheatcode = new Field("cheatcode", card.cheatcode, Field.Store.YES, Field.Index.ANALYZED);
                Field aliasList = new Field("aliasList", card.aliasList, Field.Store.YES, Field.Index.ANALYZED);
                Field adjust = new Field("adjust", card.adjust, Field.Store.YES, Field.Index.NO);
                Field associate = new Field("associate", card.associate, Field.Store.YES, Field.Index.NO);
                Field cardCamp = new Field("cardCamp", card.cardCamp.ToString(), Field.Store.YES, Field.Index.ANALYZED);

                Field level = null;
                Field pendulumL = null;
                Field pendulumR = null;
                Field element = null;
                Field tribe = null;
                Field atk = null;
                Field atkValue = null;
                Field def = null;
                Field defValue = null;
                if (ict < 20)
                {
                    //怪兽卡
                    level = new Field("level", string.Format("{0:D2}", card.level), Field.Store.YES, Field.Index.NOT_ANALYZED);
                    pendulumL = new Field("pendulumL", string.Format("{0:D2}", card.pendulumL), Field.Store.YES, Field.Index.NOT_ANALYZED);
                    pendulumR = new Field("pendulumR", string.Format("{0:D2}", card.pendulumR), Field.Store.YES, Field.Index.NOT_ANALYZED);
                    element = new Field("element", card.element, Field.Store.YES, Field.Index.NOT_ANALYZED);
                    tribe = new Field("tribe", card.tribe, Field.Store.YES, Field.Index.NOT_ANALYZED);
                    atk = new Field("atk", card.atk, Field.Store.YES, Field.Index.NOT_ANALYZED);
                    atkValue = new Field("atkValue", string.Format("{0:D4}", card.atkValue), Field.Store.YES, Field.Index.NOT_ANALYZED);
                    //atkValue = new NumericField("atkValue", Field.Store.YES, true).SetIntValue(card.atkValue);
                    def = new Field("def", card.def, Field.Store.YES, Field.Index.NOT_ANALYZED);
                    defValue = new Field("defValue", string.Format("{0:D4}", card.defValue), Field.Store.YES, Field.Index.NOT_ANALYZED);
                }
                else
                {
                    //魔陷卡
                    level = new Field("level", "-01", Field.Store.YES, Field.Index.NOT_ANALYZED);
                    pendulumL = new Field("pendulumL", "-01", Field.Store.YES, Field.Index.NOT_ANALYZED);
                    pendulumR = new Field("pendulumR", "-01", Field.Store.YES, Field.Index.NOT_ANALYZED);
                    element = new Field("element", "", Field.Store.YES, Field.Index.NOT_ANALYZED);
                    tribe = new Field("tribe", "", Field.Store.YES, Field.Index.NOT_ANALYZED);
                    atk = new Field("atk", "", Field.Store.YES, Field.Index.NOT_ANALYZED);
                    atkValue = new Field("atkValue", "-0002", Field.Store.YES, Field.Index.NOT_ANALYZED);
                    def = new Field("def", "", Field.Store.YES, Field.Index.NOT_ANALYZED);
                    defValue = new Field("defValue", "-0002", Field.Store.YES, Field.Index.NOT_ANALYZED);
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
                doc.Add(pendulumL);
                doc.Add(pendulumR);
                doc.Add(element);
                doc.Add(tribe);
                doc.Add(effectType);
                doc.Add(atk);
                doc.Add(atkValue);
                doc.Add(def);
                doc.Add(defValue);
                doc.Add(effect);
                doc.Add(infrequence);
                doc.Add(package);
                doc.Add(limit);
                doc.Add(cheatcode);
                doc.Add(aliasList);
                doc.Add(adjust);
                doc.Add(associate);
                doc.Add(cardCamp);

                if (card.iCardType == 1)
                    doc.SetBoost(1);
                else
                    doc.SetBoost(2);

                writer.AddDocument(doc);

                if (processchanged != null)
                    processchanged.Invoke(cards.Length, i + 1);
            }
            writer.Optimize();
            writer.Close();

            //记录当前索引文件夹的文件列表
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
            Save(filename, cards, null);
        }

        public virtual void Save(string filename, CardDescription[] cards, SaveProcessChangedInvoker processchanged)
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
