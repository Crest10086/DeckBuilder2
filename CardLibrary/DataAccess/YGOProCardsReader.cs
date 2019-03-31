using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data.SQLite;

namespace BaseCardLibrary.DataAccess
{

    enum YGOCardType : int
    {
        TYPE_MONSTER = 1,
        TYPE_SPELL = 2,
        TYPE_TRAP = 4,
        TYPE_NORMAL = 16,
        TYPE_EFFECT = 32,
        TYPE_FUSION = 64,
        TYPE_RITUAL = 128,
        TYPE_TRAPMONSTER = 256,
        TYPE_SPIRIT = 512,
        TYPE_UNION = 1024,
        TYPE_DUAL = 2048,
        TYPE_TUNER = 4096,
        TYPE_SYNCHRO = 8192,
        TYPE_TOKEN = 16384,
        TYPE_QUICKPLAY = 65536,
        TYPE_CONTINUOUS = 131072,
        TYPE_EQUIP = 262144,
        TYPE_FIELD = 524288,
        TYPE_COUNTER = 1048576,
        TYPE_FLIP = 2097152,
        TYPE_TOON = 4194304,
        TYPE_XYZ = 8388608,
        TYPE_PENDULUM = 16777216
    }

    enum YGOEffectType : int
    {
        TYPE_MONSTER = 1,
        TYPE_SPELL = 2,
        TYPE_TRAP = 4,
        TYPE_NORMAL = 16,
        TYPE_EFFECT = 32,
        TYPE_FUSION = 64,
        TYPE_RITUAL = 128,
        TYPE_TRAPMONSTER = 256,
        TYPE_SPIRIT = 512,
        TYPE_UNION = 1024,
        TYPE_DUAL = 2048,
        TYPE_TUNER = 4096,
        TYPE_SYNCHRO = 8192,
        TYPE_TOKEN = 16384,
        TYPE_QUICKPLAY = 65536,
        TYPE_CONTINUOUS = 131072,
        TYPE_EQUIP = 262144,
        TYPE_FIELD = 524288,
        TYPE_COUNTER = 1048576,
        TYPE_FLIP = 2097152,
        TYPE_TOON = 4194304,
        TYPE_XYZ = 8388608,
        TYPE_PENDULUM = 16777216
    }

    public class YGOProCardsReader : CardsReader
    {

        static string[] CardTypes = new string[] {
            "怪兽",	//1
            "魔法",	//2
            "陷阱",	//3
            "通常",	//4
            "效果",	//5
            "融合",	//6
            "仪式",	//7
            "陷阱怪兽",	//8
            "灵魂",	//9
            "同盟",	//10
            "二重",	//11
            "调整",	//12
            "同调",	//13
            "衍生物",	//14
            "速攻",	//15
            "永续",	//16
            "装备",	//17
            "场地",	//18
            "反击",	//19
            "反转",	//20
            "卡通",	//21
            "超量",	//22
            "灵摆",	//23
        };

        public override CardDescription[] Read(string filename, ReadProcessChangedInvoker processchanged)
        {
            if (!File.Exists(filename))
                return new CardDescription[0];

            ArrayList cards = new ArrayList(MinCapacity);
            Hashtable ht = new Hashtable(MinCapacity);

            //连接数据库
            SQLiteConnection con = new SQLiteConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data source=" + filename + ";Persist Security Info=False;");
            con.Open();

            //统计记录数
            SQLiteCommand dcc = new SQLiteCommand("Select Count(*) as iCount FROM [Datas]", con);
            SQLiteDataReader creader = dcc.ExecuteReader();
            creader.Read();
            int total = int.Parse(creader["iCount"].ToString());
            creader.Close();


            Hashtable htCheatCode = new Hashtable();

            dcc = new SQLiteCommand(@"select * from datas t1 left join texts t2 on t1.id = t2.id ", con);
            creader = dcc.ExecuteReader();

            int i = 0;
            while (creader.Read())
            {
                CardDescription card = ParseCard(creader, cards);
                if (!ht.Contains(card.name))
                {
                    cards.Add(card);
                    ht.Add(card.name, card);
                }
                else
                {
                    CardDescription cd = (CardDescription)ht[card.name];
                    cd.cheatcode = cd.cheatcode + "," + card.cheatcode;
                }
                i++;
                processchanged.Invoke(total, i);
            }

            creader.Close();
            con.Close();
            return (CardDescription[])cards.ToArray(typeof(CardDescription));
        }

        private CardDescription ParseCard(SQLiteDataReader reader, ArrayList Cards)
        {
            CardDescription card = new CardDescription();
            card.ID = Cards.Count + 1;
            card.cheatcode = GetFieldString(reader, "ID").PadLeft(8, '0'); ;
            card.name = GetFieldString(reader, "name");
            card.effect = GetFieldString(reader, "desc");

            int i = GetFieldInt(reader, "OT");
            switch (i)
            {
                case 3:
                    card.cardCamp = CardCamp.BothOT;
                    break;
                case 2:
                    card.cardCamp = CardCamp.TCG;
                    break;
                case 1:
                    card.cardCamp = CardCamp.OCG;
                    break;
                default:
                    card.cardCamp = CardCamp.DIY;
                    break;
            }

            i = GetFieldInt(reader, "level");
            if (i <= 12)
                card.level = i;
            else
            {
                card.level = (int)(i & 0xffff);
                card.pendulumR = (int)(i >> 16 & 0xff);
                card.pendulumL = (int)(i >> 24 & 0xff);
            }


            string s = GetFieldString(reader, "ATTRIBUTE");
            switch (s)
            {
                case "1":
                    card.element = "地";
                    break;
                case "2":
                    card.element = "水";
                    break;
                case "4":
                    card.element = "炎";
                    break;
                case "8":
                    card.element = "风";
                    break;
                case "16":
                    card.element = "光";
                    break;
                case "32":
                    card.element = "暗";
                    break;
                case "64":
                    card.element = "神";
                    break;
                default:
                    card.element = "无";
                    break;
            }

            s = GetFieldString(reader, "RACE");
            switch (s)
            {
                case "1":
                    card.tribe = "战士";
                    break;
                case "2":
                    card.tribe = "魔法师";
                    break;
                case "4":
                    card.tribe = "天使";
                    break;
                case "8":
                    card.tribe = "恶魔";
                    break;
                case "16":
                    card.tribe = "不死";
                    break;
                case "32":
                    card.tribe = "机械";
                    break;
                case "64":
                    card.tribe = "水";
                    break;
                case "128":
                    card.tribe = "炎";
                    break;
                case "256":
                    card.tribe = "岩石";
                    break;
                case "512":
                    card.tribe = "鸟兽";
                    break;
                case "1024":
                    card.tribe = "植物";
                    break;
                case "2048":
                    card.tribe = "昆虫";
                    break;
                case "4096":
                    card.tribe = "雷";
                    break;
                case "8192":
                    card.tribe = "龙";
                    break;
                case "16384":
                    card.tribe = "兽";
                    break;
                case "32768":
                    card.tribe = "兽战士";
                    break;
                case "65536":
                    card.tribe = "恐龙";
                    break;
                case "131072":
                    card.tribe = "鱼";
                    break;
                case "262144":
                    card.tribe = "海龙";
                    break;
                case "524288":
                    card.tribe = "爬虫类";
                    break;
                case "1048576":
                    card.tribe = "念动力";
                    break;
                case "2097152":
                    card.tribe = "幻神兽";
                    break;
                case "4194304":
                    card.tribe = "创造神";
                    break;
                case "8388608":
                    card.tribe = "幻龙";
                    break;
                default:
                    card.tribe = "无";
                    break;
            }

            s = "";
            int cardtype = GetFieldInt(reader, "type");
            if ((cardtype & (int)YGOCardType.TYPE_MONSTER) != 0L)
                s = "怪兽";
            else if ((cardtype & (int)YGOCardType.TYPE_SPELL) != 0L)
                s = "魔法";
            else if ((cardtype & (int)YGOCardType.TYPE_TRAP) != 0L)
                s = "陷阱";

            if (s == "怪兽")
            {
                card.atk = GetFieldString(reader, "atk");
                card.atkValue = int.Parse(card.atk);
                if (card.atkValue < 0)
                    card.atk = "?";

                card.def = GetFieldString(reader, "def");
                card.defValue = int.Parse(card.def);
                if (card.defValue < 0)
                    card.def = "?";
            }
            else
            {
                card.atk = "";
                card.atkValue = -10000;
                card.def = "";
                card.defValue = -10000;
            }


            if (s == "")
                return null;

            if ((cardtype & (int)YGOCardType.TYPE_NORMAL) != 0L)
                s = "通常" + s;
            else if ((cardtype & (int)YGOCardType.TYPE_FUSION) != 0L)
                s = "融合" + s;
            else if ((cardtype & (int)YGOCardType.TYPE_RITUAL) != 0L)
                s = "仪式" + s;
            else if ((cardtype & (int)YGOCardType.TYPE_SYNCHRO) != 0L)
                s = "同调" + s;
            else if ((cardtype & (int)YGOCardType.TYPE_XYZ) != 0L)
                s = "XYZ" + s;
            //else if ((cardtype & (int)YGOCardType.TYPE_PENDULUM) != 0L)
            //    s = "灵摆" + s;
            else if ((cardtype & (int)YGOCardType.TYPE_EFFECT) != 0L)
                s = "效果" + s;
            else if ((cardtype & (int)YGOCardType.TYPE_QUICKPLAY) != 0L)
                s = "速攻" + s;
            else if ((cardtype & (int)YGOCardType.TYPE_CONTINUOUS) != 0L)
                s = "永续" + s;
            else if ((cardtype & (int)YGOCardType.TYPE_EQUIP) != 0L)
                s = "装备" + s;
            else if ((cardtype & (int)YGOCardType.TYPE_FIELD) != 0L)
                s = "场地" + s;
            else if ((cardtype & (int)YGOCardType.TYPE_COUNTER) != 0L)
                s = "反击" + s;

            if (s == "魔法" || s == "陷阱")
                s = "通常" + s;
            card.sCardType = s;
            card.iCardType = CardDescription.CardTypeMapper(card.sCardType);

            if ((cardtype & (int)YGOCardType.TYPE_SPIRIT) != 0L)
                card.effect = "效果·灵魂：" + card.effect;
            if ((cardtype & (int)YGOCardType.TYPE_UNION) != 0L)
                card.effect = "效果·同盟：" + card.effect;
            if ((cardtype & (int)YGOCardType.TYPE_DUAL) != 0L)
                card.effect = "效果·二重：" + card.effect;
            if ((cardtype & (int)YGOCardType.TYPE_FLIP) != 0L)
                card.effect = "效果·反转：" + card.effect;
            if ((cardtype & (int)YGOCardType.TYPE_TOON) != 0L)
                card.effect = "效果·卡通：" + card.effect;

            s = "";

            //是否同调、超量
            if ((cardtype & (int)YGOCardType.TYPE_SYNCHRO) != 0L)
                s = "同调";
            else if ((cardtype & (int)YGOCardType.TYPE_XYZ) != 0L)
                s = "超量";
            else if ((cardtype & (int)YGOCardType.TYPE_EFFECT) != 0L)
                s = "效果";
            else
                s = "通常";

            //是否调整
            if ((cardtype & (int)YGOCardType.TYPE_TUNER) != 0L)
                s += "·调整";

            //是否灵摆
            if ((cardtype & (int)YGOCardType.TYPE_PENDULUM) != 0L)
                s += "·灵摆";

            if (s.Length > 2)
                card.effect = s + "：" + card.effect;

            return card;
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

        private int GetFieldInt(SQLiteDataReader reader, string fieldname)
        {
            try
            {
                string s = GetFieldString(reader, fieldname);
                return int.Parse(s);
            }
            catch
            {
                return -999999;
            }
        }
    }
}
