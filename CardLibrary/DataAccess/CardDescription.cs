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
        public string name;             //��Ƭ����
        public string japName;          //������
        public string enName;           //Ӣ����
        public string oldName;          //�ɿ���
        public string shortName;        //���
        public string sCardType;        //��Ƭ����
        public int iCardType;           //��Ƭ���ͱ��
        public int level;               //����
        public int pendulumL;           //��̶�
        public int pendulumR;           //�ҿ̶�
        public string element;          //����
        public string tribe;            //����
        public string effectType;       //Ч������
        public string atk;              //����
        public int atkValue;            //������
        public string def;              //����
        public int defValue;            //������
        public string effect;           //Ч��
        public string infrequence;      //ϡ����
        public string package;          //����
        public int limit;               //��������
        public string cheatcode;        //8λ����
        public string aliasList;        //ͬ���������б�
        public string adjust;           //����
        public string associate;        //������Ƭ
        public CardCamp cardCamp;       //��Ƭ����

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
                case "Ч������":
                    index = 0;
                    break;
                case "ͨ������":
                    index = 1;
                    break;
                case "�ںϹ���":
                    index = 2;
                    break;
                case "��ʽ����":
                    index = 3;
                    break;
                case "ͬ������":
                    index = 6;
                    break;
                case "XYZ����":
                    index = 7;
                    break;
                default:
                    index = -1;
                    if (cardType.Length == 4)
                    {
                        if (cardType.Substring(2, 2).Equals("ħ��"))
                            index = 4;
                        else if (cardType.Substring(2, 2).Equals("����"))
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
                    info.Append("��ֹ��");
                    break;
                case 1:
                    info.Append("���ƿ�");
                    break;
                case 2:
                    info.Append("׼���ƿ�");
                    break;
                case -4:
                    info.Append("���Ϳ�");
                    break;
            }

            if (info.Length > 0 && this.cardCamp != CardCamp.BothOT)
                info.Append("��");

            switch (this.cardCamp)
            {
                case CardCamp.TCG:
                    info.Append("TCGר�п�");
                    break;
                case CardCamp.OCG:
                    info.Append("OCGר�п�");
                    break;
                case CardCamp.DIY:
                    info.Append("DIY��");
                    break;
            }

            if (info.Length > 0)
                info.Append("\r\n");

            info.Append("��������");
            info.Append(this.name);
            info.Append("\r\n");

            /*if (this.oldName != "" && this.oldName != this.name)
            {
                info.Append("�ɿ�����");
                info.Append(this.oldName);
                info.Append("\r\n");
            }*/

            if (this.japName != "")
            {
                info.Append("��������");
                info.Append(this.japName);
                info.Append("\r\n");
            }

            if (this.enName != "")
            {
                info.Append("Ӣ������");
                info.Append(this.enName);
                info.Append("\r\n");
            }

            if (this.shortName != "" && this.shortName != this.name)
            {
                info.Append("��ƣ�");
                info.Append(this.shortName);
                info.Append("\r\n");
            }

            if (this.cheatcode != "")
            {
                info.Append("��Ƭ���룺");
                info.Append(this.GetCheatCodeList());
                info.Append("\r\n");
            }

            info.Append("��Ƭ���ࣺ");
            info.Append(this.sCardType);
            info.Append("\r\n");

            if (this.level > 0)
            {
                if (this.iCardType != 7)
                    info.Append("�Ǽ���");
                else
                    info.Append("�׼���");
                info.Append(this.level.ToString());
                info.Append("\r\n");
            }

            if (this.isMonsterCard())   //�ǹ���
            {
                info.Append("���ԣ�");
                info.Append(this.element);
                info.Append("\r\n");

                info.Append("���壺");
                info.Append(this.tribe);
                info.Append("\r\n");

                info.Append("������");
                info.Append(this.atk);
                info.Append("\r\n");

                info.Append("������");
                info.Append(this.def);
                info.Append("\r\n");
            }

            if (this.infrequence != "")
            {
                info.Append("�����ȣ�");
                info.Append(this.infrequence);
                info.Append("\r\n");
            }

            if (this.package != "")
            {
                info.Append("������");
                info.Append(this.package);
                info.Append("\r\n");
            }

            if (this.pendulumL > 0 || this.pendulumR > 0)
            {
                info.Append("��ڿ̶ȣ���");
                info.Append(this.pendulumL.ToString());
                info.Append(" ��");
                info.Append(this.pendulumR.ToString());
                info.Append("\r\n");
            }

            info.Append("Ч����");
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
                    info.Append("��ֹ��");
                    break;
                case 1:
                    info.Append("���ƿ�");
                    break;
                case 2:
                    info.Append("׼���ƿ�");
                    break;
                case -4:
                    info.Append("���Ϳ�");
                    break;
            }

            if (info.Length > 0 && this.cardCamp != CardCamp.BothOT)
                info.Append("��");


            if (this.limit != -4)
            {
                switch (this.cardCamp)
                {
                    case CardCamp.TCG:
                        info.Append("TCGר�п�");
                        break;
                    case CardCamp.OCG:
                        info.Append("OCGר�п�");
                        break;
                    case CardCamp.DIY:
                        info.Append("DIY��");
                        break;
                }
            }

            if (info.Length > 0)
                info.Append("\r\n");

            info.Append("��������");
            info.Append(this.name);
            info.Append("\r\n");

            /*if (this.oldName != "" && this.oldName != this.name)
            {
                info.Append("�ɿ�����");
                info.Append(this.oldName);
                info.Append("\r\n");
            }*/

            if (this.japName != "")
            {
                info.Append("��������");
                info.Append(this.japName);
                info.Append("\r\n");
            }

            if (this.enName != "")
            {
                info.Append("Ӣ������");
                info.Append(this.enName);
                info.Append("\r\n");
            }

            if (this.shortName != "" && this.shortName != this.name)
            {
                info.Append("��ƣ�");
                info.Append(this.shortName);
                info.Append("\r\n");
            }

            if (this.cheatcode != "")
            {
                info.Append("��Ƭ���룺");
                info.Append(this.GetCheatCodeList());
                info.Append("\r\n");
            }

            info.Append("��Ƭ���ࣺ");
            info.Append(this.sCardType);
            info.Append("\r\n");

            if (this.level > 0)
            {
                if (this.iCardType != 7)
                    info.Append("�Ǽ���");
                else
                    info.Append("�׼���");
                info.Append(this.level.ToString());
                info.Append("\r\n");
            }

            if (this.isMonsterCard())   //�ǹ���
            {
                info.Append("���ԣ�");
                info.Append(this.element);
                info.Append("\r\n");

                info.Append("���壺");
                info.Append(this.tribe);
                info.Append("\r\n");

                info.Append("������");
                info.Append(this.atk);
                info.Append("\r\n");

                info.Append("������");
                info.Append(this.def);
                info.Append("\r\n");
            }

            if (this.infrequence != "")
            {
                info.Append("�����ȣ�");
                info.Append(this.infrequence);
                info.Append("\r\n");
            }

            if (this.package != "")
            {
                info.Append("������");
                info.Append(this.package);
                info.Append("\r\n");
            }

            if (this.pendulumL > 0 || this.pendulumR > 0)
            {
                info.Append("��ڿ̶ȣ���");
                info.Append(this.pendulumL.ToString());
                info.Append(" ��");
                info.Append(this.pendulumR.ToString());
                info.Append("\r\n");
            }

            info.Append("Ч����");
            info.Append(this.effect);
            info.Append("\r\n");

            return info.ToString();
        }

        public string GetSimpleInfo()
        {
            string info = "";

            if (isMagicCard(this.iCardType))
            {
                info += "ħ�����ࣺ" + this.sCardType.Substring(0, 2) + ",";
            }
            else if (isTrapCard(this.iCardType))
            {
                info += "�������ࣺ" + this.sCardType.Substring(0, 2) + ",";
            }
            else
            {
                if (this.iCardType != 1)
                    info += this.sCardType + ",";
                else
                    info += "��ͨ����,";
                info += "���壺" + this.tribe + ",";
                info += "���ԣ�" + this.element + ",";
                info += "�Ǽ���" + this.level + ",";
                if (this.atk != "?")
                    info += "������" + this.atk + ",";
                else
                    info += "������" + this.atk + " ,";
                if (this.def != "?")
                    info += "������" + this.def + ",";
                else
                    info += "������" + this.def + " ,";
            }


            info += "Ч����" + this.effect;

            return info;
        }

        public string GetFirstCheatCode()
        {
            int index = aliasList.IndexOf(',');
            if (index == -1)
                index = aliasList.Length;
            return aliasList.Substring(0, index);
        }

        //������ʾPRO����Ķ࿨ͼͬ���������б�
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
