using System;
using System.Collections.Generic;
using System.Text;

namespace BaseCardLibrary.Common
{
    public class MyLucene
    {
        public static Lucene.Net.Util.Version GetLuceneVersion()
        {
            return Lucene.Net.Util.Version.LUCENE_29;
        }
    }
}
