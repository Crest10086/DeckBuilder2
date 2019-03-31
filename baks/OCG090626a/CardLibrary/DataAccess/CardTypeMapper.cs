using System;
using System.Collections.Generic;
using System.Text;

namespace BaseCardLibrary.DataAccess
{
    class CardTypeMapper
    {
        public static int Mapper(string cardType)
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
                default:
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
    }
}
