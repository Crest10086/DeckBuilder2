using System;
using System.Collections.Generic;
using System.Text;
using BaseCardLibrary.DataAccess;
using BaseCardLibrary.Search;

namespace DeckBuilder2
{
    class Global
    {
        public static string Version = "2.3";
        public static string Build = "0626";
        public static string ProgramDebug = "«Â∑Á°¢¥• ÷°¢sAtAn°¢¿≥µŸ°¢cyh";

        public static frmMain frmMainHolder = null;
        public static frmDeckEdit frmDeckEditHolder = null;
        public static PicLoader largePicLoader = null;
        public static string appPath = "";
        public static bool appCloing = false;
        public static bool loadPicEnd = false;
        public static oldDictSearcher searcher = null;
        public static int ReDrawTime = 250;
        //public static CXDConfig cxdconfig = CXDConfig.GetInstance();
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
