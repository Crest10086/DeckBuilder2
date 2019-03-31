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
            // pbProcess
        }

        public void setInProcess(bool b){
            button1.Enabled = !b;
            button2.Enabled = !b;
            button3.Enabled = !b;
            button4.Enabled = !b;
            button7.Enabled = !b;
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
                this.Text = "转换中，请稍候";
                setInProcess(true);
                CardsReader mReader = new yxwpReader();
                CardLibrary cardLibrary = new CardLibrary(mReader.Read(openFileDialog1.FileName, pbProcess, spStat));
                CardsSaver lSaver = new LuceneSaver();
                lSaver.Save("CardIndex", cardLibrary.GetCards(), pbProcess, spStat);
                this.Text = "辅助转换工具";
                MessageBox.Show("索引建立完成！");
                setInProcess(false);
                pbProcess.Value = 0;
                spStat.Text = "准备就绪";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("请选择迷你卡片数据目录CardLib所在位置");
            FolderBrowserDialog openFolderDialog1 = new FolderBrowserDialog();
            openFolderDialog1.RootFolder = Environment.SpecialFolder.MyComputer;
            openFolderDialog1.SelectedPath = appPath;
            if (openFolderDialog1.ShowDialog() == DialogResult.OK)
            {
                this.Text = "转换中，请稍候";
                setInProcess(true);
                CardsReader mReader = new MiniCardXReader();
                CardLibrary cardLibrary = new CardLibrary(mReader.Read(openFolderDialog1.SelectedPath, pbProcess, spStat));
                CardsSaver lSaver = new LuceneSaver();
                lSaver.Save("CardIndex", cardLibrary.GetCards(), pbProcess, spStat);
                this.Text = "辅助转换工具";
                MessageBox.Show("索引建立完成！");
                setInProcess(false);
                pbProcess.Value = 0;
                spStat.Text = "准备就绪";
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string dirname = DB2Config.GetInstance().GetSetting("ImagePath");

            {
                MessageBox.Show("请选择卡图所在目录");
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

            this.Text = "转换中，请稍候";

            //建立文件夹

            string outdirname = appPath + "\\LargeIco\\";
            if (!Directory.Exists(outdirname))
            {
                Directory.CreateDirectory(outdirname);
            }

            setInProcess(true);

            //转换大图标
            ImageList imagelist = new ImageList();
            imagelist.ImageSize = new Size(47, 67);
            imagelist.ColorDepth = ColorDepth.Depth32Bit;
            imagelist.TransparentColor = Color.White;
            int pc = 0;
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
                pc++;
                spStat.Text = "正在转换："+pc.ToString();
                Application.DoEvents();
            }

            this.Text = "辅助转换工具";
            MessageBox.Show("完成！");
            setInProcess(false);
            spStat.Text = "准备就绪";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            MessageBox.Show("请选择天堂狐查卡器数据文件所在位置");
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = appPath;
            openFileDialog1.Filter = "天堂狐查卡器数据文件 (YGODAT.DAT;YGOSYS.DB)|YGODAT.DAT;YGOSYS.DB|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 0;
            openFileDialog1.RestoreDirectory = true;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.Text = "转换中，请稍候";
                setInProcess(true);
                CardsReader mReader = new YFCCReader();
                CardLibrary cardLibrary = new CardLibrary(mReader.Read(openFileDialog1.FileName, pbProcess, spStat));
                CardsSaver lSaver = new LuceneSaver();
                lSaver.Save("CardIndex", cardLibrary.GetCards(), pbProcess, spStat);
                this.Text = "辅助转换工具";
                MessageBox.Show("索引建立完成！");
                setInProcess(false);
                pbProcess.Value = 0;
                spStat.Text = "准备就绪";
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.Text = "转换中，请稍候";
            CardsReader mReader = new LuceneReader();
            CardLibrary cardLibrary = new CardLibrary(mReader.Read("CardIndex", pbProcess, spStat));
            CardsSaver aSaver = new AllCardsSaver();
            aSaver.Save("allcards.dll", cardLibrary.GetCards(), pbProcess, spStat);
            this.Text = "辅助转换工具";
            MessageBox.Show("导出完成！");
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
            this.Text = "测试中，请稍候";
            CardsReader mReader = new LuceneReader();
            CardLibrary cardLibrary = new CardLibrary(mReader.Read("CardIndex", pbProcess, spStat));
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
            this.Text = "转换中，请稍候";
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
                setInProcess(true);
                CardsReader mReader = new DIYReader();
                CardLibrary cardLibrary = new CardLibrary(mReader.Read(openFileDialog1.FileName, pbProcess, spStat));
                CardsSaver lSaver = new LuceneSaver();
                lSaver.Save("DIYCardIndex", cardLibrary.GetCards(), pbProcess, spStat);
                this.Text = "辅助转换工具";
                MessageBox.Show("索引建立完成！");
                setInProcess(false);
                pbProcess.Value = 0;
                spStat.Text = "准备就绪";
            }
            this.Text = "辅助转换工具";
        }

        private void tmrEvent_Tick(object sender, EventArgs e) {
            Application.DoEvents();
        }

        private void statusStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e) {

        }
    }
}