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
                default:
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
    }
}
