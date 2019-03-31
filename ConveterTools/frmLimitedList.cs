using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using BaseCardLibrary.DataAccess;
using BaseCardLibrary.Search;

namespace DeckBuilder2
{
    public partial class frmLimitedList : Form
    {
        public frmLimitedList()
        {
            InitializeComponent();
        }

        private void frmLimitedList_Load(object sender, EventArgs e)
        {
            LimitedListManager llm = LimitedListManager.GetInstance();

            CardLimitedList[] cll = llm.GetItems();
            listBox1.Items.Clear();
            for (int i = 0; i < cll.Length; i++)
            {
                listBox1.Items.Add(cll[i].Name);
            }

            listBox1.SelectedIndex = llm.SelectedIndex;

            if (llm.SelcetedItem != null)
                richTextBox1.Text = llm.SelcetedItem.Desc;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            CardLimitedList[] cll =  LimitedListManager.GetInstance().GetItems();
            if (listBox1.SelectedIndex >= 0 && listBox1.SelectedIndex < cll.Length)
                richTextBox1.Text = cll[listBox1.SelectedIndex].Desc;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex < 0)
            {
                MessageBox.Show("请选择一个默认的禁卡表！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            LimitedListManager llm = LimitedListManager.GetInstance();
            llm.SelectedIndex = listBox1.SelectedIndex;
            CardLibrary.GetInstance().UpdateLimitedList(llm.SelcetedItem);
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
