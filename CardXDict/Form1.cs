using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;

namespace CardXDict
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = MyTools.GetPingyin.convertline("命运英雄 血魔-D 123");

            SimpleAnalyzer ma = new SimpleAnalyzer();
            TokenStream ts = ma.TokenStream("", new StringReader(richTextBox1.Text));

            Token token;
            while ((token = ts.Next()) != null)
            {
                this.richTextBox1.AppendText("\n");
                this.richTextBox1.AppendText(token.TermText());
                this.richTextBox1.AppendText(" ");
            }
            ts.Close();
        }
    }
}
