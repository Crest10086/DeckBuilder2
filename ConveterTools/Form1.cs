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
            MessageBox.Show("请选择中中版查卡器数据文件所在位置");
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = appPath;
            openFileDialog1.Filter = "中中版查卡器数据文件 (ocg.yxwp)|ocg.yxwp|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 0;
            openFileDialog1.RestoreDirectory = true;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.Text = "数据读入中，请稍候";
                DisableAllButton();
                CardsReader mReader = new yxwpReader();
                CardLibrary cardLibrary = new CardLibrary(mReader.Read(openFileDialog1.FileName, ProcessChanged));
                this.Text = "索引建立中，请稍候";
                CardsSaver lSaver = new LuceneSaver();
                lSaver.Save("CardIndex", cardLibrary.GetCards(), ProcessChanged);
                this.Text = "辅助转换工具";
                EnableAllButton();
                MessageBox.Show("索引建立完成！");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("因为迷你卡查长期不更新，暂时不再提供数据转换！");
            /*
            MessageBox.Show("请选择迷你卡片数据目录CardLib所在位置");
            FolderBrowserDialog openFolderDialog1 = new FolderBrowserDialog();
            openFolderDialog1.RootFolder = Environment.SpecialFolder.MyComputer;
            openFolderDialog1.SelectedPath = appPath;
            if (openFolderDialog1.ShowDialog() == DialogResult.OK)
            {
                this.Text = "数据读入中，请稍候";
                CardsReader mReader = new MiniCardXReader();
                CardLibrary cardLibrary = new CardLibrary(mReader.Read(openFolderDialog1.SelectedPath, ProcessChanged));
                this.Text = "索引建立中，请稍候";
                CardsSaver lSaver = new LuceneSaver();
                lSaver.Save("CardIndex", cardLibrary.GetCards(), ProcessChanged);
                this.Text = "辅助转换工具";
                MessageBox.Show("索引建立完成！");
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
            openFolderDialog1.Description = "请选择卡图所在目录";
            openFolderDialog1.ShowNewFolderButton = false;

            if (openFolderDialog1.ShowDialog() == DialogResult.OK)
            {
                ImageDir = openFolderDialog1.SelectedPath;
            }
            else
                return;

            this.Text = "转换中，请稍候";
            DisableAllButton();

            //建立文件夹
            string outdirname = appPath + "\\LargeIco\\";
            if (!Directory.Exists(outdirname))
            {
                Directory.CreateDirectory(outdirname);
            }

            //转换大图标
            ImageList imagelist = new ImageList();
            imagelist.ImageSize = new Size(47, 67);
            imagelist.ColorDepth = ColorDepth.Depth32Bit;
            imagelist.TransparentColor = Color.White;

            //计算数量
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

            this.Text = "辅助转换工具";
            EnableAllButton();
            MessageBox.Show("完成！");
        }

        private void BuildPackageList(CardLibrary cardLibrary)
        {
            SortedList sl = new SortedList();
            string[] ss = null;
            int count = cardLibrary.GetNonDIYCount();
            for (int i = 0; i < count; i++)
            {
                ss = cardLibrary.GetCardByIndex(i).package.Split(',', '，');
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

            //MessageBox.Show("请选择天堂狐查卡器数据文件所在位置");
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = YFCCDir + "\\YGODATA\\";
            openFileDialog1.Filter = "天堂狐查卡器数据文件 (YGODAT.DAT;YGOSYS.DB)|YGODAT.DAT;YGOSYS.DB|YGODAT.MDB)|YGODAT.MDB|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 0;
            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.Title = "请选择天堂狐查卡器数据文件所在位置";
            openFileDialog1.FileName = "YGODAT.DAT";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.Text = "数据读入中，请稍候";
                DisableAllButton(); 
                Application.DoEvents();

                CardsReader mReader = new YFCCReader();
                CardLibrary cardLibrary = new CardLibrary(mReader.Read(openFileDialog1.FileName, ProcessChanged));

                //选择禁卡表
                LimitedListManager llm = LimitedListManager.GetInstance();
                llm.LoadFromYFCC(openFileDialog1.FileName);
                frmLimitedList form = new frmLimitedList();
                form.ShowDialog();

                this.Text = "索引建立中，请稍候";
                CardsSaver lSaver = new LuceneSaver();
                lSaver.Save("CardIndex", cardLibrary.GetCards(), ProcessChanged);

                this.Text = "卡包列表生成中，请稍候";
                BuildPackageList(cardLibrary);


                this.Text = "辅助转换工具";
                EnableAllButton();
                MessageBox.Show("索引建立完成！\r\r请接着导入YGOPRO补充数据，否则将不能使用效果分类搜索，并且载入YGOPRO卡组时可能出错！", "温馨提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.Text = "转换中，请稍候";
            CardsReader mReader = new LuceneReader();
            CardLibrary cardLibrary = new CardLibrary(mReader.Read("CardIndex"));
            CardsSaver aSaver = new AllCardsSaver();
            aSaver.Save("allcards.dll", cardLibrary.GetCards());
            this.Text = "辅助转换工具";
            MessageBox.Show("导出完成！");
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
            this.Text = "测试中，请稍候";
            CardsReader mReader = new LuceneReader();
            CardLibrary cardLibrary = new CardLibrary(mReader.Read("CardIndex"));
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "中中版查卡器数据文件 (ocg.yxwp)|ocg.yxwp|All files (*.*)|*.*";
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
            this.Text = "辅助转换工具";
            MessageBox.Show("测试完成！");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            string NBXDir = null;
            mAppInterface.GetAppDirEx(mAppInterface.AppName_NBX, ref NBXDir);
            string DIYPath = NBXDir + "\\data\\diycards.dll";

            this.Text = "数据读入中，请稍候";
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "DIY卡数据文件 (diycards.dll)|diycards.dll|All files (*.*)|*.*";
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
                this.Text = "索引建立中，请稍候";
                CardsSaver lSaver = new LuceneSaver();
                lSaver.Save("DIYCardIndex", cardLibrary.GetCards(), ProcessChanged);
                this.Text = "辅助转换工具";
                MessageBox.Show("索引建立完成！");
            }
            this.Text = "辅助转换工具";
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
            openFolderDialog1.Description = "请选择DIY卡图所在目录";
            openFolderDialog1.ShowNewFolderButton = false;

            if (openFolderDialog1.ShowDialog() == DialogResult.OK)
            {
                DIYImageDir = openFolderDialog1.SelectedPath;
            }
            else
                return;

            this.Text = "转换中，请稍候";
            DisableAllButton();

            //建立文件夹
            string outdirname = appPath + "\\LargeIco\\";
            if (!Directory.Exists(outdirname))
            {
                Directory.CreateDirectory(outdirname);
            }

            //转换大图标
            ImageList imagelist = new ImageList();
            imagelist.ImageSize = new Size(47, 67);
            imagelist.ColorDepth = ColorDepth.Depth32Bit;
            imagelist.TransparentColor = Color.White;

            //计算数量
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

            this.Text = "辅助转换工具";
            EnableAllButton();
            MessageBox.Show("完成！");
        }

        private void button9_Click(object sender, EventArgs e)
        {
            string YFCCDir = "";

            //MessageBox.Show("请选择YGOPRO数据文件所在位置");
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = YFCCDir + "\\YGODATA\\";
            openFileDialog1.Filter = "YGOPRO数据文件 (cards.cdb)|cards.cdb|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 0;
            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.Title = "请选择YGOPRO数据文件所在位置";
            openFileDialog1.FileName = "cards.cdb";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.Text = "数据读入中，请稍候";
                DisableAllButton();
                Application.DoEvents();

                YGOPROIncreasedReader mReader = new YGOPROIncreasedReader();
                CardLibrary cardLibrary = CardLibrary.GetInstance();
                CardDescription[] cards = mReader.Read(openFileDialog1.FileName, cardLibrary.GetCards(), ProcessChanged);

                this.Text = "索引建立中，请稍候";
                CardsSaver lSaver = new LuceneSaver();
                lSaver.Save("CardIndex", cards, ProcessChanged);

                this.Text = "卡包列表生成中，请稍候";
                BuildPackageList(cardLibrary);

                this.Text = "辅助转换工具";
                EnableAllButton();
                MessageBox.Show("索引建立完成！");
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void button11_Click(object sender, EventArgs e)
        {
            string YFCCDir = "";

            //MessageBox.Show("请选择安卓卡查数据文件所在位置");
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = YFCCDir + "\\YGODATA\\";
            openFileDialog1.Filter = "安卓卡查数据文件 (*.sqlite)|*.sqlite|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 0;
            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.Title = "请选择安卓卡查数据文件所在位置";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.Text = "数据读入中，请稍候";
                DisableAllButton();
                Application.DoEvents();

                AndroidReader mReader = new AndroidReader();
                CardDescription[] cards = mReader.Read(openFileDialog1.FileName, ProcessChanged);

                this.Text = "索引建立中，请稍候";
                CardsSaver lSaver = new LuceneSaver();
                lSaver.Save("CardIndex", cards, ProcessChanged);

                this.Text = "卡包列表生成中，请稍候";
                CardLibrary cardLibrary = CardLibrary.GetInstance();
                BuildPackageList(cardLibrary);

                this.Text = "辅助转换工具";
                EnableAllButton();
                MessageBox.Show("索引建立完成！");
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            string YFCCDir = "";

            //MessageBox.Show("请选择YGORPO数据文件所在位置");
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = YFCCDir + "\\YGODATA\\";
            openFileDialog1.Filter = "YGOPRO数据文件 (cards.cdb)|cards.cdb|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 0;
            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.Title = "请选择YGORPO数据文件所在位置";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.Text = "数据读入中，请稍候";
                DisableAllButton();
                Application.DoEvents();

                YGOProCardsReader mReader = new YGOProCardsReader();
                CardDescription[] cards = mReader.Read(openFileDialog1.FileName, ProcessChanged);

                this.Text = "索引建立中，请稍候";
                CardsSaver lSaver = new LuceneSaver();
                lSaver.Save("CardIndex", cards, ProcessChanged);

                this.Text = "卡包列表生成中，请稍候";
                CardLibrary cardLibrary = CardLibrary.GetInstance();
                BuildPackageList(cardLibrary);

                this.Text = "辅助转换工具";
                EnableAllButton();
                MessageBox.Show("索引建立完成！");
            }
        }
    }
}