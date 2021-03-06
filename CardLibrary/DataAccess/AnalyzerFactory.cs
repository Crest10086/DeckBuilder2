using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;


namespace BaseCardLibrary.DataAccess
{
    public class AnalyzerFactory
    {

        public static string[] stopWords = { "的", "之", "の", "・" ,"，"};
        public static string[] stopWords2 = { "的", "之", "の", "和", "是", "把"};

        private static Lucene.Net.Analysis.PerFieldAnalyzerWrapper analyzerWrapper = null;

        public static Lucene.Net.Analysis.Analyzer GetAnalyzer()
        {
            //return new StandardAnalyzer(new string[] {"的", "之" });

            if (analyzerWrapper == null)
            {
                analyzerWrapper = new Lucene.Net.Analysis.PerFieldAnalyzerWrapper(new StandardAnalyzer());
                analyzerWrapper.AddAnalyzer("name", new MyAnalyzer(stopWords));
                analyzerWrapper.AddAnalyzer("japName", new MyAnalyzer(stopWords));
                analyzerWrapper.AddAnalyzer("oldName", new MyAnalyzer(stopWords));
                analyzerWrapper.AddAnalyzer("shortName", new MyAnalyzer(stopWords));
                analyzerWrapper.AddAnalyzer("effect", new MyAnalyzer(stopWords2));
                analyzerWrapper.AddAnalyzer("adjust", new MyAnalyzer(stopWords2));
                analyzerWrapper.AddAnalyzer("tribe", new Lucene.Net.Analysis.KeywordAnalyzer());
                analyzerWrapper.AddAnalyzer("cheatcode", new Lucene.Net.Analysis.KeywordAnalyzer());
                analyzerWrapper.AddAnalyzer("cardCamp", new Lucene.Net.Analysis.KeywordAnalyzer());

                analyzerWrapper.AddAnalyzer("enName", new LetterDigitAnalyzer());
                analyzerWrapper.AddAnalyzer("pyname", new SimpleAnalyzer());
                analyzerWrapper.AddAnalyzer("pyshortName", new SimpleAnalyzer());
                analyzerWrapper.AddAnalyzer("pyoldName", new SimpleAnalyzer());
                analyzerWrapper.AddAnalyzer("effectType", new SimpleAnalyzer());

                //中文的字段名在搜索前已经全部转为了英文字段名，所以无分词的必要
                /*
                analyzerWrapper.AddAnalyzer("中文名", new MyAnalyzer(stopWords));
                analyzerWrapper.AddAnalyzer("日文名", new MyAnalyzer(stopWords));
                analyzerWrapper.AddAnalyzer("旧卡名", new MyAnalyzer(stopWords));
                analyzerWrapper.AddAnalyzer("曾用名", new MyAnalyzer(stopWords));
                analyzerWrapper.AddAnalyzer("简称", new MyAnalyzer(stopWords));
                analyzerWrapper.AddAnalyzer("俗称", new MyAnalyzer(stopWords));
                analyzerWrapper.AddAnalyzer("缩写", new MyAnalyzer(stopWords));
                analyzerWrapper.AddAnalyzer("效果", new MyAnalyzer(stopWords2));
                analyzerWrapper.AddAnalyzer("效果说明", new MyAnalyzer(stopWords2));
                analyzerWrapper.AddAnalyzer("调整", new MyAnalyzer(stopWords2));
                analyzerWrapper.AddAnalyzer("种族", new Lucene.Net.Analysis.KeywordAnalyzer());
                 */ 
            }

            return analyzerWrapper;
        }
    }
}
                