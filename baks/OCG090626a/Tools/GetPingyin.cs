using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.International.Converters.PinYinConverter;

namespace Tools
{
    public class GetPingyin
    {
        static Regex StopWordsRegex = new Regex(@"[的之の]", RegexOptions.Compiled);
        public static string convert(string senstr)
        {
            string ss = StopWordsRegex.Replace(senstr, "");
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < ss.Length; i++)
            {
                if (ChineseChar.IsValidChar(ss[i]))
                {
                    ChineseChar cc = new ChineseChar(ss[i]);
                    if (ChineseChar.IsValidChar(ss[i]) && cc.PinyinCount > 0)
                    {
                        sb.Append(cc.Pinyins[0].Substring(0, cc.Pinyins[0].Length - 1));
                        sb.Append(" ");
                    }
                    else
                    {
                        sb.Append(ss[i]);
                    }
                }
                else
                {
                    sb.Append(ss[i]);
                }
            }

            return sb.ToString();
        }

        private static string[] getpinyins(ChineseChar cc)
        {
            ArrayList al = new ArrayList();
            for (int i = 0; i < cc.PinyinCount; i++)
            {
                string py = cc.Pinyins[i].Substring(0, cc.Pinyins[i].Length - 1);
                bool added = false;
                foreach (Object o in al)
                    if (o.ToString() == py)
                    {
                        added = true;
                        break;
                    }
                if (!added)
                    al.Add(py);
            }

            return (string[])al.ToArray(typeof(string));
        }

        private static void getallpinyin(ArrayList pinyinlist, StringBuilder py, string sen, int index)
        {
            if (index == sen.Length)
            {
                pinyinlist.Add(py.ToString());
                return;
            }

            if (ChineseChar.IsValidChar(sen[index]))
            {
                ChineseChar cc = new ChineseChar(sen[index]);
                string[] pinyins = getpinyins(cc);
                for (int i = 0; i < pinyins.Length; i++)
                {
                    int len = py.Length;
                    py.Append(pinyins[i]);
                    py.Append(" ");
                    getallpinyin(pinyinlist, py, sen, index + 1);
                    py.Length = len;
                }
            }
            else
            {
                int len = py.Length;
                py.Append(sen[index]);
                getallpinyin(pinyinlist, py, sen, index + 1);
                py.Length = len;
            }
        }

        public static string[] converts(string senstr)
        {
            string ss = StopWordsRegex.Replace(senstr, "");
            ArrayList resultlist = new ArrayList();
            StringBuilder sb = new StringBuilder();

            getallpinyin(resultlist, sb, ss, 0);

            return (string[])resultlist.ToArray(typeof(string));
        }

        public static string convertline(string senstr)
        {
            string[] ss = converts(StopWordsRegex.Replace(senstr, ""));
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < ss.Length; i++)
            {
                sb.Append(ss[i]);
                sb.Append(" 龴 ");
            }

            return sb.ToString();
        }

        public static int GetChineseLength(string sen)
        {
            int len = 0;
            for (int i = 0; i < sen.Length; i++)
                if (ChineseChar.IsValidChar(sen[i]))
                    len++;

            return len;
        }
    }

}
