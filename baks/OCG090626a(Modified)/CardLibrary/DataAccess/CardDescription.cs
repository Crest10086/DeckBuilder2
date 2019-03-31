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
        public string sCardType;         //��Ƭ����
        public int iCardtype;            //��Ƭ���ͱ��
        public int level;               //����
        public string element;          //����
        public string tribe;            //����
        public string effecfType;       //Ч������
        public string atk;              //����
        public int atkValue;            //������
        public string def;              //����
        public int defValue;            //������
        public string effect;           //Ч��
        public string infrequence;      //ϡ����
        public string package;          //����
        public int limit;               //��������
        public string cheatcode;        //8λ����
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
            element = "";
            tribe = "";
            effecfType = "";
            atk = "";
            atkValue = -2;
            def = "";
            defValue = -2;
            effect = "";
            infrequence = "";
            package = "";
            limit = 3;
            cheatcode = "";
            adjust = "";
            associate = "";
            cardCamp = CardCamp.BothOT;
        }

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

            info.Append("��Ƭ���ࣺ");
            info.Append(this.sCardType);
            info.Append("\r\n");

            if (this.level > 0)
            {
                info.Append("�Ǽ���");
                info.Append(this.level.ToString());
                info.Append("\r\n");
            }

            if (this.iCardtype < 4 || this.iCardtype == 6)   //�ǹ���
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

            info.Append("��Ƭ���ࣺ");
            info.Append(this.sCardType);
            info.Append("\r\n");

            if (this.level > 0)
            {
                info.Append("�Ǽ���");
                info.Append(this.level.ToString());
                info.Append("\r\n");
            }

            if (this.iCardtype < 4 || this.iCardtype == 6)   //�ǹ���
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

            info.Append("Ч����");
            info.Append(this.effect);
            info.Append("\r\n");

            return info.ToString();
        }


        public string GetSimpleInfo()
        {
            string info = "";

            if (this.iCardtype == 4)
            {
                info += "ħ�����ࣺ" + this.sCardType.Substring(0,2) + ",";
            }
            else if (this.iCardtype == 5)
            {
                info += "�������ࣺ" + this.sCardType.Substring(0, 2) + ",";
            }
            else
            {
                if (this.iCardtype != 1)
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
    }
}
