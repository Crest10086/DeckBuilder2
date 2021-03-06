using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using BaseCardLibrary.Common;

namespace BaseCardLibrary.Common
{
    public class AnalyzerFactory
    {

        public static string[] stopWords = { "的", "之", "の", "・" ,"，"};
        public static string[] stopWords2 = { "的", "之", "の", "和", "是", "把"};
        public static string[] stopWords3 = { "," };
        private static Lucene.Net.Analysis.PerFieldAnalyzerWrapper analyzerWrapper = null;

        public static Lucene.Net.Analysis.Analyzer GetAnalyzer()
        {
            //return new StandardAnalyzer(new string[] {"的", "之" });

            if (analyzerWrapper == null)
            {
                analyzerWrapper = new Lucene.Net.Analysis.PerFieldAnalyzerWrapper(new StandardAnalyzer(MyLucene.GetLuceneVersion()));
                analyzerWrapper.AddAnalyzer("name", new MyAnalyzer(stopWords));
                analyzerWrapper.AddAnalyzer("japName", new MyAnalyzer(stopWords));
                analyzerWrapper.AddAnalyzer("oldName", new MyAnalyzer(stopWords));
                analyzerWrapper.AddAnalyzer("shortName", new MyAnalyzer(stopWords));
                analyzerWrapper.AddAnalyzer("effect", new MyAnalyzer(stopWords2));
                analyzerWrapper.AddAnalyzer("adjust", new MyAnalyzer(stopWords2));
                analyzerWrapper.AddAnalyzer("tribe", new Lucene.Net.Analysis.KeywordAnalyzer());
                analyzerWrapper.AddAnalyzer("cheatcode", new KeywordAnalyzer());
                analyzerWrapper.AddAnalyzer("aliasList", new PunctuationAnalyzer());
                analyzerWrapper.AddAnalyzer("cardCamp", new Lucene.Net.Analysis.KeywordAnalyzer());

                analyzerWrapper.AddAnalyzer("enName", new LetterDigitAnalyzer());
                analyzerWrapper.AddAnalyzer("pyname", new SimpleAnalyzer());
                analyzerWrapper.AddAnalyzer("pyshortName", new SimpleAnalyzer());
                analyzerWrapper.AddAnalyzer("pyoldName", new SimpleAnalyzer());
                analyzerWrapper.AddAnalyzer("effectType", new SimpleAnalyzer());
                analyzerWrapper.AddAnalyzer("package", new PunctuationAnalyzer());

                //因为高级搜索的关系，中文的字段名也需要分词

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
                analyzerWrapper.AddAnalyzer("卡包", new PunctuationAnalyzer());

            }

            return analyzerWrapper;
        }
    }
}
                