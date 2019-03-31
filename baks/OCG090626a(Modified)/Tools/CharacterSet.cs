using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualBasic;
using Mozilla.NUniversalCharDet;

namespace Tools
{
    public class CharacterSet
    {
        /*
        private static char[] HANKAKU_KATAKANA = { '｡', '｢', '｣', '､', '･',
        'ｦ', 'ｧ', 'ｨ', 'ｩ', 'ｪ', 'ｫ', 'ｬ', 'ｭ', 'ｮ', 'ｯ', 'ｰ', 'ｱ', 'ｲ',
        'ｳ', 'ｴ', 'ｵ', 'ｶ', 'ｷ', 'ｸ', 'ｹ', 'ｺ', 'ｻ', 'ｼ', 'ｽ', 'ｾ', 'ｿ',
        'ﾀ', 'ﾁ', 'ﾂ', 'ﾃ', 'ﾄ', 'ﾅ', 'ﾆ', 'ﾇ', 'ﾈ', 'ﾉ', 'ﾊ', 'ﾋ', 'ﾌ',
        'ﾍ', 'ﾎ', 'ﾏ', 'ﾐ', 'ﾑ', 'ﾒ', 'ﾓ', 'ﾔ', 'ﾕ', 'ﾖ', 'ﾗ', 'ﾘ', 'ﾙ',
        'ﾚ', 'ﾛ', 'ﾜ', 'ﾝ', 'ﾞ', 'ﾟ' };

        private static char[] ZENKAKU_KATAKANA = { '。', '「', '」', '、', '・',
        'ヲ', 'ァ', 'ィ', 'ゥ', 'ェ', 'ォ', 'ャ', 'ュ', 'ョ', 'ッ', 'ー', 'ア', 'イ',
        'ウ', 'エ', 'オ', 'カ', 'キ', 'ク', 'ケ', 'コ', 'サ', 'シ', 'ス', 'セ', 'ソ',
        'タ', 'チ', 'ツ', 'テ', 'ト', 'ナ', 'ニ', 'ヌ', 'ネ', 'ノ', 'ハ', 'ヒ', 'フ',
        'ヘ', 'ホ', 'マ', 'ミ', 'ム', 'メ', 'モ', 'ヤ', 'ユ', 'ヨ', 'ラ', 'リ', 'ル',
        'レ', 'ロ', 'ワ', 'ン', '゛', '゜' };
         */

        private static Hashtable ht = new Hashtable();
        private static Hashtable ht2 = new Hashtable();
        private static bool inited = false;

        private static void addmap(char c1, char c2)
        {
            ht.Add(c1, c2);
            ht2.Add(c2, c1);
        }

        private static void init()
        {
            addmap('·', '・');
            addmap(' ', '　');
            //addmap('－', '-');

            inited = true;
        }


        public static string JPDBCToSBC(string source)
        {
            if (!inited)
                init();

            string s = DBCToSBC(source);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                if (ht.ContainsKey(s[i]))
                    sb.Append((char)ht[s[i]]);
                else
                    sb.Append(s[i]);
            }

            return sb.ToString();
        }

        public static string JPSBCToDBC(string source)
        {
            if (!inited)
                init();

            string s = source;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                if (ht2.ContainsKey(s[i]))
                    sb.Append((char)ht2[s[i]]);
                else
                    sb.Append(s[i]);
            }

            s = sb.ToString();
            return SBCToDBC(s);
        }

        //半角转全角
        public static string DBCToSBC(string source)
        {
            return Strings.StrConv(source, VbStrConv.Wide, 1);

            /*
            //半角转全角：
            char[] c = source.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == 32)
                {
                    c[i] = (char)12288;
                    continue;
                }
                if (c[i] < 127)
                    c[i] = (char)(c[i] + 65248);
            }
            return new string(c);
             */

            /*
            char[] c=source.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
             byte[] b = System.Text.Encoding.Unicode.GetBytes(c, i, 1);
             if (b.Length == 2)
             {
                 if (b[1] == 0)
                 {
                     b[0] = (byte)(b[0] - 32);
                     b[1] = 255;
                     c[i] = System.Text.Encoding.Unicode.GetChars(b)[0];
                 }
             }
            }*/
        }

        //全角转半角
        public static string SBCToDBC(string source)
        {
            //return Strings.StrConv(source, VbStrConv.Narrow, 1);
            //这里不可以直接用Strings.StrConv，会有识别问题
            
            if (source.Length == 0)
                return "";

            System.Text.StringBuilder result = new System.Text.StringBuilder(source.Length, source.Length);
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i] >= 65281 && source[i] <= 65373)
                {
                    result.Append((char)(source[i] - 65248));
                }
                else if (source[i] == 12288)
                {
                    result.Append(' ');
                }
                else
                {
                    result.Append(source[i]);
                }
            }
            return result.ToString();
            
        }

        //繁体转简体
        public static string BIG5ToGB(string source)
        {
            return Strings.StrConv(source, VbStrConv.SimplifiedChinese, 0);
        }

        //识别一个文本文件的字符集
        public static string GetCharSet(string filename)
        {
            try
            {
                byte[] pReadByte = new byte[0];
                FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
                BinaryReader r = new BinaryReader(fs);
                r.BaseStream.Seek(0, SeekOrigin.Begin);    //将文件指针设置到文件开
                pReadByte = r.ReadBytes((int)r.BaseStream.Length);
                UniversalDetector Det = new UniversalDetector(null);
                Det.HandleData(pReadByte, 0, pReadByte.Length);
                Det.DataEnd();
                return Det.GetDetectedCharset();
            }
            catch
            {
                return null;
            }
        }
    }
}
