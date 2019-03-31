using System;
using System.Collections;
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
using AppInterface;


namespace ConveterTools
{
    public partial class Form1 : DevComponents.DotNetBar.Office2007Form
    {
        string appPath = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void EnableAllButton()
        {
            button1.Enabled = true;
            button2.Enabled = true;
            button3.Enabled = true;
            button4.Enabled = true;
            button5.Enabled = true;
            button6.Enabled = true;
            button7.Enabled = true;
            button8.Enabled = true;
            button9.Enabled = true;
        }

        private void DisableAllButton()
        {
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
            button6.Enabled = false;
            button7.Enabled = false;
            button8.Enabled = false;
            button9.Enabled = false;
        }

        private void ProcessChanged(int total, int current)
        {
            progressBarX1.Maximum = total;
            progressBarX1.Value = current;
            progressBarX1.Text = current.ToString() + "/" + total.ToString();
            Application.DoEvents();
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
                this.Text = "���ݶ����У����Ժ�";
                DisableAllButton();
                CardsReader mReader = new yxwpReader();
                CardLibrary cardLibrary = new CardLibrary(mReader.Read(openFileDialog1.FileName, ProcessChanged));
                this.Text = "���������У����Ժ�";
                CardsSaver lSaver = new LuceneSaver();
                lSaver.Save("CardIndex", cardLibrary.GetCards(), ProcessChanged);
                this.Text = "����ת������";
                EnableAllButton();
                MessageBox.Show("����������ɣ�");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("��Ϊ���㿨�鳤�ڲ����£���ʱ�����ṩ����ת����");
            /*
            MessageBox.Show("��ѡ�����㿨Ƭ����Ŀ¼CardLib����λ��");
            FolderBrowserDialog openFolderDialog1 = new FolderBrowserDialog();
            openFolderDialog1.RootFolder = Environment.SpecialFolder.MyComputer;
            openFolderDialog1.SelectedPath = appPath;
            if (openFolderDialog1.ShowDialog() == DialogResult.OK)
            {
                this.Text = "���ݶ����У����Ժ�";
                CardsReader mReader = new MiniCardXReader();
                CardLibrary cardLibrary = new CardLibrary(mReader.Read(openFolderDialog1.SelectedPath, ProcessChanged));
                this.Text = "���������У����Ժ�";
                CardsSaver lSaver = new LuceneSaver();
                lSaver.Save("CardIndex", cardLibrary.GetCards(), ProcessChanged);
                this.Text = "����ת������";
                MessageBox.Show("����������ɣ�");
            }
             */ 
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string ImageDir = null;
            mAppInterface.GetCardImgDirEx(ref ImageDir);

            if (!Directory.Exists(ImageDir))
                ImageDir = DB2Config.GetInstance().GetSetting("ImagePath");

            if (!Directory.Exists(ImageDir))
                ImageDir = appPath + "\\Image\\";

            FolderBrowserDialog openFolderDialog1 = new FolderBrowserDialog();
            openFolderDialog1.SelectedPath = ImageDir;
            openFolderDialog1.Description = "��ѡ��ͼ����Ŀ¼";
            openFolderDialog1.ShowNewFolderButton = false;

            if (openFolderDialog1.ShowDialog() == DialogResult.OK)
            {
                ImageDir = openFolderDialog1.SelectedPath;
            }
            else
                return;

            this.Text = "ת���У����Ժ�";
            DisableAllButton();

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

            //��������
            int total = 0;
            foreach (string sf in Directory.GetFiles(ImageDir))
            {
                FileInfo f = new FileInfo(sf);
                if (string.Equals(f.Extension, ".jpg", StringComparison.OrdinalIgnoreCase))
                    total++;
            }
            string stotal = total.ToString();
            progressBarX1.Maximum = total;
            int i = 0;
            CardLibrary cardLibrary = CardLibrary.GetInstance();
            foreach (string sf in Directory.GetFiles(ImageDir))
            {
                FileInfo f = new FileInfo(sf);
                if (string.Equals(f.Extension, ".jpg", StringComparison.OrdinalIgnoreCase))
                {
                    string id = f.Name.Substring(0, f.Name.LastIndexOf('.'));
                    try
                    {
                        int id2 = int.Parse(id);
                        CardDescription card = cardLibrary.GetCardByID(id2);
                        if (card == null)
                        {
                            card = cardLibrary.GetCardByCheatCode(id);
                            if (card != null)
                                id = card.ID.ToString();
                            else
                                id = "0";
                        }
                    }
                    catch
                    {
                        CardDescription card = cardLibrary.GetCardByCheatCode(id);
                        if (card != null)
                            id = card.ID.ToString();
                        else
                            id = "0";
                    }
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

                    i++;
                    progressBarX1.Value = i;
                    progressBarX1.Text = i.ToString() + "/" + stotal;
                    Application.DoEvents();
                }
            }

            this.Text = "����ת������";
            EnableAllButton();
            MessageBox.Show("��ɣ�");
        }

        private void BuildPackageList(CardLibrary cardLibrary)
        {
            SortedList sl = new SortedList();
            string[] ss = null;
            int count = cardLibrary.GetNonDIYCount();
            for (int i = 0; i < count; i++)
            {
                ss = cardLibrary.GetCardByIndex(i).package.Split(',', '��');
                foreach (string s in ss)
                {
                    if (!sl.Contains(s))
                        sl.Add(s, s);
                }
            }

            ss = new string[sl.Count];
            for (int i = 0; i < sl.Count; i++)
                ss[i] = (string)sl.GetByIndex(i);
            File.WriteAllLines(appPath + "\\PackageList.txt", ss);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string YFCCDir = null;
            mAppInterface.GetAppDirEx(mAppInterface.AppName_YFCC, ref YFCCDir);
            string YFCCData = YFCCDir + "\\YGODATA\\YGODAT.DAT";
            if (!File.Exists(YFCCData))
                YFCCData = YFCCDir + "\\YGODATA\\YGOSYS.DB";
            else
                YFCCData = "";

            //MessageBox.Show("��ѡ�����ú��鿨�������ļ�����λ��");
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = YFCCDir + "\\YGODATA\\";
            openFileDialog1.Filter = "���ú��鿨�������ļ� (YGODAT.DAT;YGOSYS.DB)|YGODAT.DAT;YGOSYS.DB|YGODAT.MDB)|YGODAT.MDB|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 0;
            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.Title = "��ѡ�����ú��鿨�������ļ�����λ��";
            openFileDialog1.FileName = "YGODAT.DAT";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.Text = "���ݶ����У����Ժ�";
                DisableAllButton(); 
                Application.DoEvents();

                CardsReader mReader = new YFCCReader();
                CardLibrary cardLibrary = new CardLibrary(mReader.Read(openFileDialog1.FileName, ProcessChanged));

                //ѡ�������
                LimitedListManager llm = LimitedListManager.GetInstance();
                llm.LoadFromYFCC(openFileDialog1.FileName);
                frmLimitedList form = new frmLimitedList();
                form.ShowDialog();

                this.Text = "���������У����Ժ�";
                CardsSaver lSaver = new LuceneSaver();
                lSaver.Save("CardIndex", cardLibrary.GetCards(), ProcessChanged);

                this.Text = "�����б������У����Ժ�";
                BuildPackageList(cardLibrary);


                this.Text = "����ת������";
                EnableAllButton();
                MessageBox.Show("����������ɣ�\r\r����ŵ���YGOPRO�������ݣ����򽫲���ʹ��Ч��������������������YGOPRO����ʱ���ܳ���", "��ܰ��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.Text = "ת���У����Ժ�";
            CardsReader mReader = new LuceneReader();
            CardLibrary cardLibrary = new CardLibrary(mReader.Read("CardIndex"));
            CardsSaver aSaver = new AllCardsSaver();
            aSaver.Save("allcards.dll", cardLibrary.GetCards());
            this.Text = "����ת������";
            MessageBox.Show("������ɣ�");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Office2007ColorTable = DevComponents.DotNetBar.Rendering.eOffice2007ColorScheme.VistaGlass;

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
            this.Text = "�����У����Ժ�";
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
            string NBXDir = null;
            mAppInterface.GetAppDirEx(mAppInterface.AppName_NBX, ref NBXDir);
            string DIYPath = NBXDir + "\\data\\diycards.dll";

            this.Text = "���ݶ����У����Ժ�";
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
                CardLibrary cardLibrary = new CardLibrary(mReader.Read(openFileDialog1.FileName, ProcessChanged));
                this.Text = "���������У����Ժ�";
                CardsSaver lSaver = new LuceneSaver();
                lSaver.Save("DIYCardIndex", cardLibrary.GetCards(), ProcessChanged);
                this.Text = "����ת������";
                MessageBox.Show("����������ɣ�");
            }
            this.Text = "����ת������";
        }

        private void button8_Click(object sender, EventArgs e)
        {
            string DIYImageDir = null;
            mAppInterface.GetDIYCardImgDirEx(ref DIYImageDir);

            if (!Directory.Exists(DIYImageDir))
                DIYImageDir = DB2Config.GetInstance().GetSetting("DIYImagePath");

            if (!Directory.Exists(DIYImageDir))
            {
                string NBXDir = null;
                mAppInterface.GetAppDirEx(mAppInterface.AppName_NBX, ref NBXDir);
                if (!Directory.Exists(NBXDir))
                {
                    string NBXPath = DB2Config.GetInstance().GetSetting("NBXPath");
                    FileInfo fi = new FileInfo(NBXPath);
                    NBXDir = fi.Directory.FullName + "\\";
                }
                DIYImageDir = NBXDir + "\\data\\diyimg\\";
            }

            FolderBrowserDialog openFolderDialog1 = new FolderBrowserDialog();
            openFolderDialog1.SelectedPath = DIYImageDir;
            openFolderDialog1.Description = "��ѡ��DIY��ͼ����Ŀ¼";
            openFolderDialog1.ShowNewFolderButton = false;

            if (openFolderDialog1.ShowDialog() == DialogResult.OK)
            {
                DIYImageDir = openFolderDialog1.SelectedPath;
            }
            else
                return;

            this.Text = "ת���У����Ժ�";
            DisableAllButton();

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

            //��������
            int total = 0;
            foreach (string sf in Directory.GetFiles(DIYImageDir))
            {
                FileInfo f = new FileInfo(sf);
                if (string.Equals(f.Extension, ".jpg", StringComparison.OrdinalIgnoreCase))
                    total++;
            }
            string stotal = total.ToString();
            progressBarX1.Maximum = total;
            int i = 0;
            foreach (string sf in Directory.GetFiles(DIYImageDir))
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

                    i++;
                    progressBarX1.Value = i;
                    progressBarX1.Text = i.ToString() + "/" + stotal;
                    Application.DoEvents();
                }
            }

            this.Text = "����ת������";
            EnableAllButton();
            MessageBox.Show("��ɣ�");
        }

        private void button9_Click(object sender, EventArgs e)
        {
            string YFCCDir = "";

            //MessageBox.Show("��ѡ��YGOPRO�����ļ�����λ��");
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = YFCCDir + "\\YGODATA\\";
            openFileDialog1.Filter = "YGOPRO�����ļ� (cards.cdb)|cards.cdb|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 0;
            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.Title = "��ѡ��YGOPRO�����ļ�����λ��";
            openFileDialog1.FileName = "cards.cdb";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.Text = "���ݶ����У����Ժ�";
                DisableAllButton();
                Application.DoEvents();

                YGOPROIncreasedReader mReader = new YGOPROIncreasedReader();
                CardLibrary cardLibrary = CardLibrary.GetInstance();
                CardDescription[] cards = mReader.Read(openFileDialog1.FileName, cardLibrary.GetCards(), ProcessChanged);

                this.Text = "���������У����Ժ�";
                CardsSaver lSaver = new LuceneSaver();
                lSaver.Save("CardIndex", cards, ProcessChanged);

                this.Text = "�����б������У����Ժ�";
                BuildPackageList(cardLibrary);

                this.Text = "����ת������";
                EnableAllButton();
                MessageBox.Show("����������ɣ�");
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void button11_Click(object sender, EventArgs e)
        {
            string YFCCDir = "";

            //MessageBox.Show("��ѡ��׿���������ļ�����λ��");
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = YFCCDir + "\\YGODATA\\";
            openFileDialog1.Filter = "��׿���������ļ� (*.sqlite)|*.sqlite|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 0;
            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.Title = "��ѡ��׿���������ļ�����λ��";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.Text = "���ݶ����У����Ժ�";
                DisableAllButton();
                Application.DoEvents();

                AndroidReader mReader = new AndroidReader();
                CardDescription[] cards = mReader.Read(openFileDialog1.FileName, ProcessChanged);

                this.Text = "���������У����Ժ�";
                CardsSaver lSaver = new LuceneSaver();
                lSaver.Save("CardIndex", cards, ProcessChanged);

                this.Text = "�����б������У����Ժ�";
                CardLibrary cardLibrary = CardLibrary.GetInstance();
                BuildPackageList(cardLibrary);

                this.Text = "����ת������";
                EnableAllButton();
                MessageBox.Show("����������ɣ�");
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            string YFCCDir = "";

            //MessageBox.Show("��ѡ��YGORPO�����ļ�����λ��");
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = YFCCDir + "\\YGODATA\\";
            openFileDialog1.Filter = "YGOPRO�����ļ� (cards.cdb)|cards.cdb|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 0;
            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.Title = "��ѡ��YGORPO�����ļ�����λ��";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.Text = "���ݶ����У����Ժ�";
                DisableAllButton();
                Application.DoEvents();

                YGOProCardsReader mReader = new YGOProCardsReader();
                CardDescription[] cards = mReader.Read(openFileDialog1.FileName, ProcessChanged);

                this.Text = "���������У����Ժ�";
                CardsSaver lSaver = new LuceneSaver();
                lSaver.Save("CardIndex", cards, ProcessChanged);

                this.Text = "�����б������У����Ժ�";
                CardLibrary cardLibrary = CardLibrary.GetInstance();
                BuildPackageList(cardLibrary);

                this.Text = "����ת������";
                EnableAllButton();
                MessageBox.Show("����������ɣ�");
            }
        }
    }
}