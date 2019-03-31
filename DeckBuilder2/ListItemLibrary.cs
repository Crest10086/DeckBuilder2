using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using BaseCardLibrary.DataAccess;
using BaseCardLibrary.Search;

namespace DeckBuilder2
{
    class ListViewItemFactory
    {
        public static ListViewItem GetItemByCard(CardDescription card, bool largeicon)
        {
            if (card == null)
                return null;

            ListViewItem item = new ListViewItem();
            item.Text = card.name;
            Color color = item.ForeColor;
            switch (card.iCardType)
            {
                case 0:
                    color = Color.OrangeRed;
                    break;
                case 4:
                    color = Color.Green;
                    break;
                case 5:
                    color = Color.Fuchsia;
                    break;
                case 1:
                    color = Color.SandyBrown;
                    break;
                case 2:
                    color = Color.DarkOrchid;
                    break;
                case 3:
                    color = Color.DodgerBlue;
                    break;
                case 6:
                    color = Color.DarkSlateGray;
                    break;
                case 7:
                    color = Color.Black;
                    break;
            }
            item.ForeColor = color;

            item.SubItems.Add(card.japName);
            item.SubItems.Add(card.sCardType);
            item.SubItems.Add(card.tribe);
            item.SubItems.Add(card.element);
            if (card.level > 0)
                item.SubItems.Add(card.level.ToString());
            else
                item.SubItems.Add("");
            item.SubItems.Add(card.atk);
            item.SubItems.Add(card.def);
            item.SubItems.Add(card.ID.ToString());
            item.ImageIndex = GetImageIndexByID(card.ID, largeicon);

            return item;
        }

        public static ListViewItem GetItemByID(int id, bool largeicon)
        {
            int index = CardLibrary.GetInstance().GetCardIndexByID(id);
            return GetItemByIndex(index, largeicon);
        }

        private static ListViewItem GetItemByIndex(int index, bool largeicon)
        {
            CardDescription card = CardLibrary.GetInstance().GetCardByIndex(index);
            return GetItemByCard(card, largeicon);
        }

        public static int GetImageIndexByID(int id, bool largeicon)
        {
            int index = CardLibrary.GetInstance().GetCardIndexByID(id);
            return GetImageIndexByIndex(index, largeicon);
        }

        private static int GetImageIndexByIndex(int index, bool largeicon)
        {
            CardDescription card = CardLibrary.GetInstance().GetCardByIndex(index);

            if (largeicon)
                return Global.largePicLoader.GetLargeIcoIndex(card.ID);
            else
                return card.iCardType;
        }
    }
}
