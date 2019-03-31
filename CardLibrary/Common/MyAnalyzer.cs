using System;
using System.Collections.Generic;
using System.Text;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;

namespace BaseCardLibrary.Common
{
    //
    public class MyFilter : TokenFilter
    {
        Token CurrentToken = null;
        int CurrentIndex = 0;
        char[] buffer = null;
        public static char Separator = ' ';
        bool NeedAddSeparator = false;

        public MyFilter(TokenStream in_Renamed)
            : base(in_Renamed)
        {
            CurrentToken = null;
            CurrentIndex = 0;
            NeedAddSeparator = false;
        }

        [Obsolete(@"The returned Token is a ""full private copy"" (not re-used across calls to Next()) but will be slower than calling {@link #Next(Token)} or using the new IncrementToken() method with the new AttributeSource API.")]
        public override Token Next()
        {
            return Next(null);
        }

        private bool isLetterOrDigit(char c)
        {
            c = char.ToLower(c);
            if (c >= 'a' && c <= 'z')
                return true;
            if (c >= '0' && c <= '9')
                return true;
            return false;
        }

        [Obsolete("The new IncrementToken() and AttributeSource APIs should be used instead.")]
        public override Token Next(Token result)
        {
            if (CurrentToken == null)
            {
                CurrentToken = input.Next();

                if (CurrentToken == null)
                    return null;

                buffer = CurrentToken.TermBuffer();
                if (CurrentToken.TermLength() > 0 && isLetterOrDigit(buffer[0]))
                {
                    CurrentIndex = 0;
                }
                else
                {
                    Token ct = CurrentToken;
                    CurrentToken = null;
                    NeedAddSeparator = false;
                    return ct;
                }
            }

            int i = CurrentToken.StartOffset() + CurrentIndex;

            if (NeedAddSeparator)
            {
                Token st = new Token(Separator.ToString(), i-1, i-1);
                NeedAddSeparator = false;
                return st;
            }

            Token t = new Token(buffer[CurrentIndex++].ToString(), i, i);
            if (CurrentIndex == CurrentToken.TermLength())
            {
                CurrentToken = null;
                NeedAddSeparator = true;
            }

            return t;
        }
    }


    public class MyAnalyzer : Analyzer
    {
        string[] stopWords = null;

        public MyAnalyzer(System.String[] stopWords)
        {
            this.stopWords = stopWords;
        }

        public override TokenStream TokenStream(System.String fieldName, System.IO.TextReader reader)
        {
            StandardAnalyzer sa = new StandardAnalyzer(MyLucene.GetLuceneVersion(), StopFilter.MakeStopSet(stopWords));
            TokenStream result = sa.TokenStream(fieldName, reader);
            result = new MyFilter(result);
            return result;
        }

        public override TokenStream ReusableTokenStream(System.String fieldName, System.IO.TextReader reader)
        {
            StandardAnalyzer sa = new StandardAnalyzer(MyLucene.GetLuceneVersion(), StopFilter.MakeStopSet(stopWords));
            TokenStream result = sa.TokenStream(fieldName, reader);
            result = new MyFilter(result);
            return result;
        }
    }

    public class LetterDigitTokenizer : Tokenizer
    {
        private int OffsetIndex = 0;
        private char[] buff = null;
        public static char Separator = ' ';
        private bool NeedAddSeparator = false;

        /// <summary>Construct a new LetterTokenizer. </summary>
        public LetterDigitTokenizer(System.IO.TextReader in_Renamed)
            : base(in_Renamed)
        {
            OffsetIndex = 0;
            NeedAddSeparator = false;
            string s = input.ReadToEnd();
            if (s == null)
                buff = new char[0];
            else
                buff = s.ToCharArray();
        }

        protected internal virtual char Normalize(char c)
        {
            return Char.ToLower(c);
        }

        [Obsolete(@"The returned Token is a ""full private copy"" (not re-used across calls to Next()) but will be slower than calling {@link #Next(Token)} or using the new IncrementToken() method with the new AttributeSource API.")]
        public override Token Next()
        {
            while (OffsetIndex < buff.Length)
            {
                char c = buff[OffsetIndex];
                if (IsTokenChar(c))
                {
                    if (NeedAddSeparator)
                    {
                        Token t = new Token(Separator.ToString(), OffsetIndex, OffsetIndex);
                        NeedAddSeparator = false;
                        return t;
                    }
                    else
                    {
                        Token t = new Token(Normalize(c).ToString(), OffsetIndex, OffsetIndex);
                        OffsetIndex++;
                        return t;
                    }
                }
                else
                {
                    NeedAddSeparator = true;
                    OffsetIndex++;
                }
            }

            return null;
        }

        [Obsolete("The new IncrementToken() and AttributeSource APIs should be used instead.")]
        public override Token Next(Token token)
        {
            while (OffsetIndex < buff.Length)
            {
                char c = buff[OffsetIndex];
                if (IsTokenChar(c))
                {
                    if (NeedAddSeparator)
                    {
                        Token t = new Token(Separator.ToString(), OffsetIndex, OffsetIndex);
                        NeedAddSeparator = false;
                        return t;
                    }
                    else
                    {
                        Token t = new Token(Normalize(c).ToString(), OffsetIndex, OffsetIndex);
                        OffsetIndex++;
                        return t;
                    }
                }
                else
                {
                    NeedAddSeparator = true;
                    OffsetIndex++;
                }
            }

            return null;
        }

        public override void Reset(System.IO.TextReader input)
        {
            this.input = input;
            OffsetIndex = 0;
            NeedAddSeparator = false;
            string s = input.ReadToEnd();
            if (s == null)
                buff = new char[0];
            else
                buff = s.ToCharArray();
        }

        /// <summary>Collects only characters and digits which satisfy
        /// {@link Character#isLetter(char)}.
        /// </summary>
        protected virtual bool IsTokenChar(char c)
        {
            return System.Char.IsLetterOrDigit(c);
        }
    }

    public class LetterDigitAnalyzer : Analyzer
    {
        public override TokenStream TokenStream(System.String fieldName, System.IO.TextReader reader)
        {
            TokenStream result = new LetterDigitTokenizer(reader);
            //result = new LowerCaseFilter(result);
            return result;
        }

        public override TokenStream ReusableTokenStream(System.String fieldName, System.IO.TextReader reader)
        {
            Tokenizer tokenizer = (Tokenizer)GetPreviousTokenStream();
            if (tokenizer == null)
            {
                tokenizer = new LetterDigitTokenizer(reader);
                SetPreviousTokenStream(tokenizer);
            }
            else
                tokenizer.Reset(reader);
            return tokenizer;
        }
    }




    //去除标点符号和空格，英文字母最小化
    public class PunctuationTokenizer : CharTokenizer
    {
        /// <summary>Construct a new WhitespaceTokenizer. </summary>
        public PunctuationTokenizer(System.IO.TextReader in_Renamed)
            : base(in_Renamed)
        {
        }

        /// <summary>Construct a new WhitespaceTokenizer using a given {@link Lucene.Net.Util.AttributeSource.AttributeFactory}. </summary>
        public PunctuationTokenizer(AttributeFactory factory, System.IO.TextReader in_Renamed)
            : base(factory, in_Renamed)
        {
        }

        /// <summary>Converts char to lower case
        /// {@link Character#toLowerCase(char)}.
        /// </summary>
        protected override char Normalize(char c)
        {
            return System.Char.ToLower(c);
        }

        /// <summary>Collects only characters which do not satisfy
        /// {@link Character#isWhitespace(char)}.
        /// </summary>
        protected override bool IsTokenChar(char c)
        {
            return !(System.Char.IsPunctuation(c) || System.Char.IsWhiteSpace(c));
        }
    }

    //只去除标点符号和空格，英文字母最小化的分词器
    public sealed class PunctuationAnalyzer : Analyzer
    {
        public override TokenStream TokenStream(System.String fieldName, System.IO.TextReader reader)
        {
            return new PunctuationTokenizer(reader);
        }

        public override TokenStream ReusableTokenStream(System.String fieldName, System.IO.TextReader reader)
        {
            Tokenizer tokenizer = (Tokenizer)GetPreviousTokenStream();
            if (tokenizer == null)
            {
                tokenizer = new PunctuationTokenizer(reader);
                SetPreviousTokenStream(tokenizer);
            }
            else
                tokenizer.Reset(reader);
            return tokenizer;
        }
    }
}
