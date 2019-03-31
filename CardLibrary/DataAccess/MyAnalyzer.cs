using System;
using System.Collections.Generic;
using System.Text;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;

namespace BaseCardLibrary.DataAccess
{
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
            StandardAnalyzer sa = new StandardAnalyzer(stopWords);
            TokenStream result = sa.TokenStream(fieldName, reader);
            result = new MyFilter(result);
            return result;
        }

        public override TokenStream ReusableTokenStream(System.String fieldName, System.IO.TextReader reader)
        {
            StandardAnalyzer sa = new StandardAnalyzer(stopWords);
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
}
