using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DeckBuilder2;
using BaseCardLibrary;
using BaseCardLibrary.DataAccess;
using BaseCardLibrary.Search;


namespace ConveterTools
{
    public partial class Form1 : Form
    {
        string appPath = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("��ѡ�����а�鿨�������ļ�����λ��");
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = appPath;
            openFileDialog1.Filter = "���а�鿨�������ļ� (ocg.yxwp)|ocg.yxwp|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 0;
            openFileDialog1.RestoreDirectory = true;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.Text = "ת���У�����΢";
                CardsReader mReader = new yxwpReader();
                CardLibrary cardLibrary = new CardLibrary(mReader.Read(openFileDialog1.FileName));
                CardsSaver lSaver = new LuceneSaver();
                lSaver.Save("CardIndex", cardLibrary.GetCards());
                this.Text = "����ת������";
                MessageBox.Show("����������ɣ�");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("��ѡ�����㿨Ƭ����Ŀ¼CardLib����λ��");
            FolderBrowserDialog openFolderDialog1 = new FolderBrowserDialog();
            openFolderDialog1.RootFolder = Environment.SpecialFolder.MyComputer;
            openFolderDialog1.SelectedPath = appPath;
            if (openFolderDialog1.ShowDialog() == DialogResult.OK)
            {
                this.Text = "ת���У�����΢";
                CardsReader mReader = new MiniCardXReader();
                CardLibrary cardLibrary = new CardLibrary(mReader.Read(openFolderDialog1.SelectedPath));
                CardsSaver lSaver = new LuceneSaver();
                lSaver.Save("CardIndex", cardLibrary.GetCards());
                this.Text = "����ת������";
                MessageBox.Show("����������ɣ�");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string dirname = DB2Config.GetInstance().GetSetting("ImagePath");

            {
                MessageBox.Show("��ѡ��ͼ����Ŀ¼");
                FolderBrowserDialog openFolderDialog1 = new FolderBrowserDialog();
                openFolderDialog1.RootFolder = Environment.SpecialFolder.MyComputer;
                string dirpath = DB2Config.GetInstance().GetSetting("ImagePath");
                if (!Directory.Exists(dirpath))
                    dirpath = appPath + "\\Image\\";
                if (!Directory.Exists(dirpath))
                    dirpath = appPath;

                openFolderDialog1.SelectedPath = dirpath;

                if (openFolderDialog1.ShowDialog() == DialogResult.OK)
                {
                    dirname = openFolderDialog1.SelectedPath;
                }
                else
                    return;
            }

            this.Text = "ת���У�����΢";

            //�����ļ���

            string outdirname = appPath + "\\LargeIco\\";
            if (!Directory.Exists(outdirname))
            {
                Directory.CreateDirectory(outdirname);
            }

            //ת����ͼ��
            ImageList imagelist = new ImageList();
            imagelist.ImageSize = new Size(47, 67);
            imagelist.ColorDepth = ColorDepth.Depth32Bit;
            imagelist.TransparentColor = Color.White;
            foreach (string sf in Directory.GetFiles(dirname))
            {
                FileInfo f = new FileInfo(sf);
                if (string.Equals(f.Extension, ".jpg", StringComparison.OrdinalIgnoreCase))
                {
                    string id = f.Name.Substring(0, f.Name.LastIndexOf('.'));
                    string filename = outdirname + "\\" + id + ".jpg";
                    if (!File.Exists(filename))
                    {
                        try
                        {
                            imagelist.Images.Add(Image.FromFile(f.FullName));
                            imagelist.Images[0].Save(filename, System.Drawing.Imaging.ImageFormat.Jpeg);
                            imagelist.Images.RemoveAt(0);
                        }
                        catch
                        {
                        }
                    }
                }
            }

            this.Text = "����ת������";
            MessageBox.Show("��ɣ�");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            MessageBox.Show("��ѡ�����ú��鿨�������ļ�����λ��");
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = appPath;
            openFileDialog1.Filter = "���ú��鿨�������ļ� (YGODAT.DAT;YGOSYS.DB)|YGODAT.DAT;YGOSYS.DB|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 0;
            openFileDialog1.RestoreDirectory = true;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.Text = "ת���У�����΢";
                CardsReader mReader = new YFCCReader();
                CardLibrary cardLibrary = new CardLibrary(mReader.Read(openFileDialog1.FileName));
                CardsSaver lSaver = new LuceneSaver();
                lSaver.Save("CardIndex", cardLibrary.GetCards());
                this.Text = "����ת������";
                MessageBox.Show("����������ɣ�");
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.Text = "ת���У�����΢";
            CardsReader mReader = new LuceneReader();
            CardLibrary cardLibrary = new CardLibrary(mReader.Read("CardIndex"));
            CardsSaver aSaver = new AllCardsSaver();
            aSaver.Save("allcards.dll", cardLibrary.GetCards());
            this.Text = "����ת������";
            MessageBox.Show("������ɣ�");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            DB2Config.GetInstance().SetSetting("FirstTimeRun", "False");
            appPath = Application.ExecutablePath;
            for (int i = appPath.Length - 1; i >= 0; i--)
                if (appPath[i] == '\\')
                {
                    appPath = appPath.Substring(0, i);
                    break;
                }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            this.Text = "�����У�����΢";
            CardsReader mReader = new LuceneReader();
            CardLibrary cardLibrary = new CardLibrary(mReader.Read("CardIndex"));
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "���а�鿨�������ļ� (ocg.yxwp)|ocg.yxwp|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 0;
            openFileDialog1.RestoreDirectory = true;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText("test.txt", "", Encoding.GetEncoding("GB2312"));
                int i = 0;
                StreamReader sr = new StreamReader(openFileDialog1.FileName, Encoding.GetEncoding("GB2312"));
                while (!sr.EndOfStream)
                {
                    string s = sr.ReadLine();
                    string[] ss = s.Split('^');
                    string n = ss[1];
                    i++;
                    if (cardLibrary.GetCardByName(n) == null && cardLibrary.GetCardByOldName(n) == null)
                    {
                        File.AppendAllText("test.txt", string.Format("{0}   {1}\r\n", i, n), Encoding.GetEncoding("GB2312"));
                    }
                }
                sr.Close();
            }
            this.Text = "����ת������";
            MessageBox.Show("������ɣ�");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            this.Text = "ת���У�����΢";
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "DIY�������ļ� (diycards.dll)|diycards.dll|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 0;
            openFileDialog1.RestoreDirectory = true;
            string dirpath = DB2Config.GetInstance().GetSetting("DeckPath") + "..\\";
            if (!Directory.Exists(dirpath))
                dirpath = appPath + "\\Image\\";
            if (!Directory.Exists(dirpath))
                dirpath = appPath;
            openFileDialog1.InitialDirectory = dirpath;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                CardsReader mReader = new DIYReader();
                CardLibrary cardLibrary = new CardLibrary(mReader.Read(openFileDialog1.FileName));
                CardsSaver lSaver = new LuceneSaver();
                lSaver.Save("DIYCardIndex", cardLibrary.GetCards());
                this.Text = "����ת������";
                MessageBox.Show("����������ɣ�");
            }
            this.Text = "����ת������";
        }
    }
}