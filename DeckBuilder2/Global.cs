using System;
using System.Collections.Generic;
using System.Text;
using BaseCardLibrary.DataAccess;
using BaseCardLibrary.Search;

namespace DeckBuilder2
{
    class Global
    {
        public static int InternalVersion = 1;
        public static int DataVersion = 100000;
        public static string Version = "2.4.1";
        public static string Build = "0725";
        public static string ProgramDebug = "琴酒、清风、触手、sAtAn、莱蒂、cyh、尸体";

        public static frmMain frmMainHolder = null;
        public static frmDeckEdit frmDeckEditHolder = null;
        public static frmPicView frmPicViewHolder = null;
        public static PicLoader largePicLoader = null;

        public static string appPath = "";
        

        public static bool appCloing = false;
        public static bool loadPicEnd = false;
        public static oldDictSearcher searcher = null;
        public static int ReDrawTime = 250;

        public static bool MenuVersionChecked = false;
    }

    public enum DeckType : int
    {
        None,
        MainDeck,
        SideDeck,
        FusionDeck,
        TempDeck
    }

    public class DragCard
    {
        public CardDescription Card = null;
        public DeckType RemoveFrom = DeckType.None;
        public int RemoveIndex = -1;
        public string RemoveName = null;
        public int AddIndex = -1;
        public Object FromObject = null;
    }
}
