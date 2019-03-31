using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using BaseCardLibrary.Common;
using BaseCardLibrary.DataAccess;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Search;

namespace ConveterTools
{
    public partial class FormTest : Form
    {
        public FormTest()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //TokenStream ts = new LetterDigitTokenizer(new System.IO.StringReader("Hiro's Shadow Scout"));
            //TokenStream ts = new StandardTokenizer(new System.IO.StringReader("荷鲁斯之黑炎龙8"));
            MyAnalyzer ma = new MyAnalyzer(new string[0]);
            TokenStream ts = ma.TokenStream("", new StringReader("荷鲁斯之黑炎龙LV8"));

            Token token;
            while ((token = ts.Next()) != null)
            {
                this.richTextBox1.AppendText(token.TermText());
                this.richTextBox1.AppendText(" ");
            }
            ts.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            NumericRangeQuery query = NumericRangeQuery.NewIntRange("num", 0, 1500, true, false);
            richTextBox1.AppendText(query.ToString());
        }
    }
}
