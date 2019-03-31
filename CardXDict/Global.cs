using System;
using System.Collections.Generic;
using System.Text;
using BaseCardLibrary.DataAccess;
using CardXDict;
using BaseCardLibrary.Search;

namespace CardXDict
{
    class Global
    {
        public static int InternalVersion = 1;
        public static string Version = "0.9.5";
        public static string Build = "0428";

        public static string appPath = "";
        public static bool appCloing = false;
        public static bool loadPicEnd = false;
        public static DictSearcher searcher = null;
        public static CXDConfig cxdconfig = CXDConfig.GetInstance();
        public static frmDict frmDictHolder = null;
        public static frmFloat frmFloatHolder = null;
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
