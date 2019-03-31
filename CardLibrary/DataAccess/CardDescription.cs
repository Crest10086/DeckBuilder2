using System;
using System.Collections.Generic;
using System.Text;

namespace BaseCardLibrary.DataAccess
{
    public enum CardCamp
    { BothOT, OCG, TCG, DIY};

    public class CardDescription
    {
        public int ID;                  //ID
        public string name;             //卡片名称
        public string japName;          //日文名
        public string enName;           //英文名
        public string oldName;          //旧卡名
        public string shortName;        //简称
        public string sCardType;        //卡片类型
        public int iCardType;           //卡片类型编号
        public int level;               //星数
        public int pendulumL;           //左刻度
        public int pendulumR;           //右刻度
        public string element;          //属性
        public string tribe;            //种族
        public string effectType;       //效果归类
        public string atk;              //攻击
        public int atkValue;            //攻击力
        public string def;              //防御
        public int defValue;            //防御力
        public string effect;           //效果
        public string infrequence;      //稀罕度
        public string package;          //卡包
        public int limit;               //禁限类型
        public string cheatcode;        //8位密码
        public string aliasList;        //同名卡密码列表
        public string adjust;           //调整
        public string associate;        //关联卡片
        public CardCamp cardCamp;       //卡片归属

        public CardDescription()
        {
            ID = 0;
            name = "";
            japName = "";
            enName = "";
            oldName = "";
            shortName = "";
            sCardType = "";
            level = 0;
            pendulumL = 0;
            pendulumR = 0;
            element = "";
            tribe = "";
            effectType = "";
            atk = "";
            atkValue = -2;
            def = "";
            defValue = -2;
            effect = "";
            infrequence = "";
            package = "";
            limit = 3;
            cheatcode = "";
            aliasList = "";
            adjust = "";
            associate = "";
            cardCamp = CardCamp.BothOT;
        }

        public static int CardTypeMapper(string cardType)
        {
            int index = 0;
            switch (cardType)
            {
                case "效果怪兽":
                    index = 0;
                    break;
                case "通常怪兽":
                    index = 1;
                    break;
                case "融合怪兽":
                    index = 2;
                    break;
                case "仪式怪兽":
                    index = 3;
                    break;
                case "同调怪兽":
                    index = 6;
                    break;
                case "XYZ怪兽":
                    index = 7;
                    break;
                default:
                    index = -1;
                    if (cardType.Length == 4)
                    {
                        if (cardType.Substring(2, 2).Equals("魔法"))
                            index = 4;
                        else if (cardType.Substring(2, 2).Equals("陷阱"))
                            index = 5;
                    }
                    break;
            }
            return index;
        }

        public static bool isExtraCard(int iCardType)
        {
            return iCardType == 2 || iCardType == 6 || iCardType == 7;
        }

        public static bool isExtraCard(CardDescription card)
        {
            return isExtraCard(card.iCardType);
        }

        public bool isExtraCard()
        {
            return CardDescription.isExtraCard(this);
        }

        public static bool isMonsterCard(int iCardType)
        {
            return iCardType <= 3 || isExtraCard(iCardType);
        }

        public static bool isMonsterCard(CardDescription card)
        {
            return isMonsterCard(card.iCardType);
        }

        public bool isMonsterCard()
        {
            return CardDescription.isMonsterCard(this);
        }

        public static bool isMagicCard(int iCardType)
        {
            return iCardType == 4;
        }

        public static bool isMagicCard(CardDescription card)
        {
            return isMagicCard(card.iCardType);
        }

        public bool isMagicCard()
        {
            return CardDescription.isMagicCard(this);
        }

        public static bool isTrapCard(int iCardType)
        {
            return iCardType == 5;
        }

        public static bool isTrapCard(CardDescription card)
        {
            return isTrapCard(card.iCardType);
        }

        public bool isTrapCard()
        {
            return CardDescription.isTrapCard(this);
        }

        //
        //===============================================================================
        //
        public string GetAllInfo()
        {
            StringBuilder info = new StringBuilder("", 200);

            switch (this.limit)
            {
                case 0:
                    info.Append("禁止卡");
                    break;
                case 1:
                    info.Append("限制卡");
                    break;
                case 2:
                    info.Append("准限制卡");
                    break;
                case -4:
                    info.Append("观赏卡");
                    break;
            }

            if (info.Length > 0 && this.cardCamp != CardCamp.BothOT)
                info.Append("、");

            switch (this.cardCamp)
            {
                case CardCamp.TCG:
                    info.Append("TCG专有卡");
                    break;
                case CardCamp.OCG:
                    info.Append("OCG专有卡");
                    break;
                case CardCamp.DIY:
                    info.Append("DIY卡");
                    break;
            }

            if (info.Length > 0)
                info.Append("\r\n");

            info.Append("中文名：");
            info.Append(this.name);
            info.Append("\r\n");

            /*if (this.oldName != "" && this.oldName != this.name)
            {
                info.Append("旧卡名：");
                info.Append(this.oldName);
                info.Append("\r\n");
            }*/

            if (this.japName != "")
            {
                info.Append("日文名：");
                info.Append(this.japName);
                info.Append("\r\n");
            }

            if (this.enName != "")
            {
                info.Append("英文名：");
                info.Append(this.enName);
                info.Append("\r\n");
            }

            if (this.shortName != "" && this.shortName != this.name)
            {
                info.Append("简称：");
                info.Append(this.shortName);
                info.Append("\r\n");
            }

            if (this.cheatcode != "")
            {
                info.Append("卡片密码：");
                info.Append(this.GetCheatCodeList());
                info.Append("\r\n");
            }

            info.Append("卡片种类：");
            info.Append(this.sCardType);
            info.Append("\r\n");

            if (this.level > 0)
            {
                if (this.iCardType != 7)
                    info.Append("星级：");
                else
                    info.Append("阶级：");
                info.Append(this.level.ToString());
                info.Append("\r\n");
            }

            if (this.isMonsterCard())   //是怪兽
            {
                info.Append("属性：");
                info.Append(this.element);
                info.Append("\r\n");

                info.Append("种族：");
                info.Append(this.tribe);
                info.Append("\r\n");

                info.Append("攻击：");
                info.Append(this.atk);
                info.Append("\r\n");

                info.Append("防御：");
                info.Append(this.def);
                info.Append("\r\n");
            }

            if (this.infrequence != "")
            {
                info.Append("罕见度：");
                info.Append(this.infrequence);
                info.Append("\r\n");
            }

            if (this.package != "")
            {
                info.Append("卡包：");
                info.Append(this.package);
                info.Append("\r\n");
            }

            if (this.pendulumL > 0 || this.pendulumR > 0)
            {
                info.Append("灵摆刻度：左");
                info.Append(this.pendulumL.ToString());
                info.Append(" 右");
                info.Append(this.pendulumR.ToString());
                info.Append("\r\n");
            }

            info.Append("效果：");
            info.Append(this.effect);
            info.Append("\r\n");

            return info.ToString();
        }

        public string GetAllInfo2()
        {
            StringBuilder info = new StringBuilder("", 200);

            switch (this.limit)
            {
                case 0:
                    info.Append("禁止卡");
                    break;
                case 1:
                    info.Append("限制卡");
                    break;
                case 2:
                    info.Append("准限制卡");
                    break;
                case -4:
                    info.Append("观赏卡");
                    break;
            }

            if (info.Length > 0 && this.cardCamp != CardCamp.BothOT)
                info.Append("、");


            if (this.limit != -4)
            {
                switch (this.cardCamp)
                {
                    case CardCamp.TCG:
                        info.Append("TCG专有卡");
                        break;
                    case CardCamp.OCG:
                        info.Append("OCG专有卡");
                        break;
                    case CardCamp.DIY:
                        info.Append("DIY卡");
                        break;
                }
            }

            if (info.Length > 0)
                info.Append("\r\n");

            info.Append("中文名：");
            info.Append(this.name);
            info.Append("\r\n");

            /*if (this.oldName != "" && this.oldName != this.name)
            {
                info.Append("旧卡名：");
                info.Append(this.oldName);
                info.Append("\r\n");
            }*/

            if (this.japName != "")
            {
                info.Append("日文名：");
                info.Append(this.japName);
                info.Append("\r\n");
            }

            if (this.enName != "")
            {
                info.Append("英文名：");
                info.Append(this.enName);
                info.Append("\r\n");
            }

            if (this.shortName != "" && this.shortName != this.name)
            {
                info.Append("简称：");
                info.Append(this.shortName);
                info.Append("\r\n");
            }

            if (this.cheatcode != "")
            {
                info.Append("卡片密码：");
                info.Append(this.GetCheatCodeList());
                info.Append("\r\n");
            }

            info.Append("卡片种类：");
            info.Append(this.sCardType);
            info.Append("\r\n");

            if (this.level > 0)
            {
                if (this.iCardType != 7)
                    info.Append("星级：");
                else
                    info.Append("阶级：");
                info.Append(this.level.ToString());
                info.Append("\r\n");
            }

            if (this.isMonsterCard())   //是怪兽
            {
                info.Append("属性：");
                info.Append(this.element);
                info.Append("\r\n");

                info.Append("种族：");
                info.Append(this.tribe);
                info.Append("\r\n");

                info.Append("攻击：");
                info.Append(this.atk);
                info.Append("\r\n");

                info.Append("防御：");
                info.Append(this.def);
                info.Append("\r\n");
            }

            if (this.infrequence != "")
            {
                info.Append("罕见度：");
                info.Append(this.infrequence);
                info.Append("\r\n");
            }

            if (this.package != "")
            {
                info.Append("卡包：");
                info.Append(this.package);
                info.Append("\r\n");
            }

            if (this.pendulumL > 0 || this.pendulumR > 0)
            {
                info.Append("灵摆刻度：左");
                info.Append(this.pendulumL.ToString());
                info.Append(" 右");
                info.Append(this.pendulumR.ToString());
                info.Append("\r\n");
            }

            info.Append("效果：");
            info.Append(this.effect);
            info.Append("\r\n");

            return info.ToString();
        }

        public string GetSimpleInfo()
        {
            string info = "";

            if (isMagicCard(this.iCardType))
            {
                info += "魔法种类：" + this.sCardType.Substring(0, 2) + ",";
            }
            else if (isTrapCard(this.iCardType))
            {
                info += "陷阱种类：" + this.sCardType.Substring(0, 2) + ",";
            }
            else
            {
                if (this.iCardType != 1)
                    info += this.sCardType + ",";
                else
                    info += "普通怪兽,";
                info += "种族：" + this.tribe + ",";
                info += "属性：" + this.element + ",";
                info += "星级：" + this.level + ",";
                if (this.atk != "?")
                    info += "攻击：" + this.atk + ",";
                else
                    info += "攻击：" + this.atk + " ,";
                if (this.def != "?")
                    info += "防御：" + this.def + ",";
                else
                    info += "防御：" + this.def + " ,";
            }


            info += "效果：" + this.effect;

            return info;
        }

        public string GetFirstCheatCode()
        {
            int index = aliasList.IndexOf(',');
            if (index == -1)
                index = aliasList.Length;
            return aliasList.Substring(0, index);
        }

        //用于显示PRO自造的多卡图同名卡密码列表
        public string GetCheatCodeList()
        {
            string[] ss = this.aliasList.Split(',');
            if (ss.Length > 1)
            {
                StringBuilder sb = new StringBuilder();
                BaseCardLibrary.Search.CardLibrary cardLibrary = BaseCardLibrary.Search.CardLibrary.GetInstance();
                for (int i = 0; i < ss.Length; i++)
                {
                    if (string.Equals(this.cheatcode, ss[i]) || !cardLibrary.IsExistByCheatCode(ss[i]))
                    {
                        sb.Append(ss[i]);
                        sb.Append(',');
                    }
                }
                sb.Length--;

                return sb.ToString();
            }

            return this.cheatcode;
        }
    }
}
