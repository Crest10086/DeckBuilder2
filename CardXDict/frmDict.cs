using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using XDICTGRB;
using BaseCardLibrary;
using BaseCardLibrary.DataAccess;
using BaseCardLibrary.Search;
using DevComponents.DotNetBar;
using AppInterface;

namespace CardXDict
{
    public partial class frmDict : Office2007Form, IXDictGrabSink  
    {
        //注册全局热键用的常量
        private const int WM_HOTKEY = 0x312; //窗口消息-热键
        private const int WM_ENDSESSION = 0x0016; //关机消息
        private const int MOD_ALT = 0x1;     //ALT
        private const int MOD_CONTROL = 0x2; //CTRL
        private const int MOD_SHIFT = 0x4;   //SHIFT
        private const int VK_SPACE = 0x20;   //SPACE
        private const int VK_F10 = 121;      //F10
        private const int VK_F11 = 122;      //F11
        private const int VK_F12 = 123;      //F12

        private const int hkGrabEnable = 0x21;       //取词开关热键ID
        private const int hkMainFormVisble = 0x22;   //主界面显示隐藏热键ID


        //注册全局热键用的API
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int RegisterHotKey(IntPtr hwnd, int id, int fsModifiers, int vk);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int UnregisterHotKey(IntPtr hwnd, int id);

        /// 注册热键
        /// </summary>
        /// <param name="hwnd">窗口句柄</param>
        /// <param name="hotKey_id">热键ID</param>
        /// <param name="fsModifiers">组合键</param>
        /// <param name="vk">热键</param>
        private void RegKey(IntPtr hwnd, int hotKey_id, int fsModifiers, int vk)
        {
            bool result;
            if (RegisterHotKey(hwnd, hotKey_id, fsModifiers, vk) == 0)
            {
                result = false;
            }
            else
            {
                result = true;
            }
            if (!result)
            {
                MessageBox.Show("注册热键失败！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 注销热键
        /// </summary>
        /// <param name="hwnd">窗口句柄</param>
        /// <param name="hotKey_id">热键ID</param>
        private void UnRegKey(IntPtr hwnd, int hotKey_id)
        {
            UnregisterHotKey(hwnd, hotKey_id);
        }

        public GrabProxy gp = null;
        DictSearcher searcher = null;
        frmFloat FloatForm = null;
        Point CurrentLocation = new Point(0, 0);
        ListViewItem[] resultItem = null;
        FormWindowState fws = FormWindowState.Normal;
        int lastcaptionclicktime = 0;
        bool firstenter = false;
        EnumMousePointPosition cursor = EnumMousePointPosition.MouseSizeNone;
        bool dragingborder = false;
        Point startdragborderpoint = new Point(0, 0);
        CardDescription[] Cards = new CardDescription[0];
        bool allowexit = false;
        CardDescription CurrentCard = null;
        Icon icon1 = null;
        Icon icon2 = null;
        string NBXDir = "";
        string NBXEDir = "";
        string YFCCDir = "";
        string DIYToolDir = "";
        string DeckBuilderDir = "";
       
        public frmDict()
        {
            InitializeComponent();
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            switch (m.Msg)
            {
                case WM_HOTKEY: //窗口消息-热键
                    switch (m.WParam.ToInt32())
                    {
                        case hkGrabEnable: //热键ID
                            toolStripMenuItem3_Click(null, null);
                            break;
                        case hkMainFormVisble:
                            if (this.Visible)
                                this.WindowState = FormWindowState.Minimized;
                            else
                                notifyIcon1_DoubleClick(null, null);
                            break;
                        default:
                            break;
                    }
                    break;
                case WM_ENDSESSION:     //关机消息
                    allowexit = true;
                    this.Close();
                    break;
                default:
                    break;
            }
        }

        Point GetFloatPoint(Point cursor)
        {
            Point p = new Point();

            if (cursor.X + FloatForm.Width + 10 <= Screen.PrimaryScreen.Bounds.Width)
            {
                if (cursor.Y + FloatForm.Height + 10 <= Screen.PrimaryScreen.Bounds.Height)
                {
                    p = new Point(cursor.X + 10, cursor.Y + 10);
                }
                else
                {
                    int newy = cursor.Y - FloatForm.Height - 10;
                    if (newy >= 0)
                        p = new Point(cursor.X + 10, newy);
                    else
                    {
                        if (cursor.Y * 2 > Screen.PrimaryScreen.Bounds.Height)
                        {
                            p = new Point(cursor.X + 10, 10);
                        }
                        else
                        {
                            p = new Point(cursor.X + 10, Screen.PrimaryScreen.Bounds.Height - FloatForm.Height - 10);
                        }
                    }
                }
            }
            else
            {
                if (cursor.Y + FloatForm.Height + 10 <= Screen.PrimaryScreen.Bounds.Height)
                {
                    p = new Point(cursor.X - FloatForm.Width - 10, cursor.Y + 10);
                }
                else
                {
                    int newy = cursor.Y - FloatForm.Height - 10;
                    if (newy >= 0)
                        p = new Point(cursor.X - FloatForm.Width - 10, newy);
                    else
                    {
                        if (cursor.Y * 2 > Screen.PrimaryScreen.Bounds.Height)
                        {
                            p = new Point(cursor.X - FloatForm.Width - 10, 10);
                        }
                        else
                        {
                            p = new Point(cursor.X - FloatForm.Width - 10, Screen.PrimaryScreen.Bounds.Height - FloatForm.Height - 10);
                        }
                    }
                }
            }

            return p;
        }


        int IXDictGrabSink.QueryWord(string WordString, int lCursorX, int lCursorY, string SentenceString, ref int lLoc, ref int lStart) 
        {
            if (FloatForm.IsMouseIn)
                return 1;

            if (SentenceString.Trim() != "")
            {      
                CurrentLocation = new Point(lCursorX, lCursorY);
                FloatForm.MoveTo(GetFloatPoint(CurrentLocation));

                SearchResult sr = searcher.TopSearch(SentenceString, lLoc, 8);
                if (sr.KeyWord == null)
                {
                    sr.KeyWord = oldDictSearcher.GetMainWord(SentenceString, lLoc);
                    //if (sr.KeyWord.Length > 8)
                    //    sr.KeyWord = SentenceString.Substring(lLoc, 1);
                }
                FloatForm.KeyWord = sr.KeyWord;
                FloatForm.Cards = sr.Cards;
                FloatForm.Show();
                timer1.Start();
            }
            else
            {
                if (FloatForm.HideForm())
                    timer1.Stop();
                else
                    timer1.Start();
            }

            return 1;
        }

        private void frmTest_Load(object sender, EventArgs e)
        {
            //System.IO.FileStream fs = new System.IO.FileStream("ciba.ico", System.IO.FileMode.Create);
            //notifyIcon1.Icon.Save(fs);
            //fs.Close();

            CardLibrary.GetInstance().AllowDIY = true;
            this.Text = "卡片词霸 v" + Global.Version;

            //listView2.Dock = DockStyle.None;
            //listView2.Location = new Point(1, 0);
            //listView2.Size = new Size(listView1.Size.Width - 2, listView1.Height - listView1.ClientRectangle.Height+14);
            listView2.Columns[0].TextAlign = HorizontalAlignment.Left;
            listView2.Columns[1].TextAlign = HorizontalAlignment.Left;

            //if (System.Environment.OSVersion.Version.Major > 5)
            //{
            //    splitContainer1.SplitterDistance = 16;
            //}
            //else
                splitContainer1.SplitterDistance = 16;


            this.Office2007ColorTable = DevComponents.DotNetBar.Rendering.eOffice2007ColorScheme.Blue;
            this.textBoxDropDown1.TextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxDropDown1_KeyPress);
            this.textBoxDropDown1.TextBox.Enter += new System.EventHandler(this.textBoxDropDown1_Enter);
            this.textBoxDropDown1.TextBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.textBoxDropDown1_MouseUp);
            Rectangle rect = new Rectangle();
            rect = Screen.GetWorkingArea(this);
            this.MaximizedBounds = new Rectangle(rect.X - 4, rect.Y, rect.Width + 8, rect.Height);

            Global.cxdconfig = CXDConfig.GetInstance();
            Global.frmDictHolder = this;

            RegisterHotKey(Handle, hkGrabEnable, 0, VK_F11);
            RegisterHotKey(Handle, hkMainFormVisble, 0, VK_F10);

            FloatForm = new frmFloat();
            try
            {
                FloatForm.Opacity = double.Parse(Global.cxdconfig.GetSetting("Opacity"))/100;
            }
            catch
            {
                FloatForm.Opacity = 0.9;
            }

            searcher = new DictSearcher();
            Global.searcher = searcher;
            gp = new GrabProxy();
            gp.GrabEnabled = true;

            try
            {
                gp.GrabInterval = int.Parse(Global.cxdconfig.GetSetting("Interval"));
            }
            catch
            {
                gp.GrabInterval = 30;
            }

            switch (Global.cxdconfig.GetSetting("GetWordMode"))
            {
                case "0":
                    gp.GrabMode = XDictGrabModeEnum.XDictGrabMouse;
                    break;
                case "1":
                    gp.GrabMode = XDictGrabModeEnum.XDictGrabMouseWithCtrl;
                    break;
                case "2":
                    gp.GrabMode = XDictGrabModeEnum.XDictGrabMouseWithShift;
                    break;
                case "3":
                    gp.GrabMode = XDictGrabModeEnum.XDictGrabMouseWithMiddleButton;
                    break;
                default:
                    gp.GrabMode = XDictGrabModeEnum.XDictGrabMouse;
                    break;
            }
            
            gp.AdviseGrab(this);

            icon1 = global::CardXDict.Properties.Resources.ciba;
            icon2 = global::CardXDict.Properties.Resources.cibastop;
            notifyIcon1.Icon = icon1;
        }

        private void toolStripMenuItem1_Click_1(object sender, EventArgs e)
        {
            allowexit = true;
            Application.Exit();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            bool inside = true;

            if (FloatForm.Location.X >= CurrentLocation.X)
            {
                if (CurrentLocation.X - Cursor.Position.X > 10 || Cursor.Position.X - CurrentLocation.X > 50)
                    inside = false;
            }
            else
            {
                if (CurrentLocation.X - Cursor.Position.X > 50 || Cursor.Position.X - CurrentLocation.X > 10)
                    inside = false;
            }

            
            if (FloatForm.Location.Y >= CurrentLocation.Y)
            {
                if (CurrentLocation.Y - Cursor.Position.Y > 10 || Cursor.Position.Y - CurrentLocation.Y > 50)
                    inside = false;
            }
            else
            {
                if (CurrentLocation.Y - Cursor.Position.Y > 50 || Cursor.Position.Y - CurrentLocation.Y > 10)
                    inside = false;
            }


            if (!inside)
            {
                timer2.Start();
            }
            else
            {
                timer2.Stop();
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.ocgsoft.cn");
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            if (gp.GrabEnabled)
            {
                gp.GrabEnabled = false;
                toolStripMenuItem3.Checked = false;
                notifyIcon1.Icon = icon2;
                FloatForm.Visible = false;
            }
            else
            {
                gp.GrabEnabled = true;
                toolStripMenuItem3.Checked = true;
                notifyIcon1.Icon = icon1;
            }
        }

        private void frmDict_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (allowexit || !string.Equals(Global.cxdconfig.GetSetting("HideToTrayOnClose"), "true", StringComparison.OrdinalIgnoreCase))
            {
                notifyIcon1.Visible = false;
            }
            else
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        public void ShowNoResult()
        {
            CurrentCard = null;
            richTextBox1.Clear();
            richTextBox1.AppendText("对不起，没有找到任何卡片！");
            richTextBox1.Visible = true;
            panel2.Visible = true;
            splitContainer1.Visible = false;
        }

        public void ShowSearchList(CardDescription[] cards)
        {
            resultItem = new ListViewItem[cards.Length];
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < cards.Length; i++)
            {
                CardDescription card = cards[i];
                ListViewItem item = new ListViewItem();
                item.Text = card.name;
                item.UseItemStyleForSubItems = false;

                sb.Length = 0;

                if (card.japName.Length > 0)
                {
                    sb.Append("【");
                    sb.Append(card.japName);
                    sb.Append("】");
                }

                if (card.enName.Length > 0)
                {
                    if (sb.Length > 0)
                        sb.Append("、");
                    sb.Append("【");
                    sb.Append(card.enName);
                    sb.Append("】");
                }
                
                if (card.shortName.Length > 0)
                {
                    string[] ss = card.shortName.Split('，');
                    for (int j = 0; j < ss.Length; j++)
                    {
                        if (sb.Length > 0)
                            sb.Append("、");
                        sb.Append("【");
                        sb.Append(ss[j]);
                        sb.Append("】");
                    }
                }

                if (card.oldName.Length > 0)
                {
                    string[] ss = card.oldName.Split('，');
                    for (int j = 0; j < ss.Length; j++)
                    {
                        if (sb.Length > 0)
                            sb.Append("、");
                        sb.Append("【");
                        sb.Append(ss[j]);
                        sb.Append("】");
                    }
                }

                item.SubItems.Add(sb.ToString());
                /*
                Color color = item.ForeColor;
                switch (card.iCardtype)
                {
                    case 0:
                        color = Color.OrangeRed;
                        break;
                    case 4:
                        color = Color.Green;
                        break;
                    case 5:
                        color = Color.Fuchsia;
                        break;
                    case 1:
                        color = Color.SandyBrown;
                        break;
                    case 2:
                        color = Color.DarkOrchid;
                        break;
                    case 3:
                        color = Color.DodgerBlue;
                        break;
                    case 6:
                        color = Color.DarkSlateGray;
                        break;
                }
                item.ForeColor = color;
                */

                resultItem[i] = item;
            }
            
             
            richTextBox1.Visible = false;
            panel2.Visible = false;
            splitContainer1.Visible = true;
            listView1.VirtualListSize = cards.Length;
            
        }

        public void ShowCardInfo(CardDescription card)
        {
            CurrentCard = card;

            richTextBox1.Clear();
            richTextBox1.AppendText("【说明】\n");
            richTextBox1.AppendText(card.GetAllInfo());

            if (card.adjust.Length > 0)
            {
                richTextBox1.AppendText("\n\r【调整】\n");
                richTextBox1.AppendText(card.adjust);
            }

            richTextBox1.Visible = true;
            panel2.Visible = true;
            splitContainer1.Visible = false;

            MyTools.API.SendMessage(richTextBox1.Handle, MyTools.APIConst.WM_VSCROLL, MyTools.APIConst.SB_TOP, IntPtr.Zero);
        }

        public void DoSearch(string searchText)
        {
            textBoxDropDown1.Text = searchText;
            buttonX1_Click(null, null);
        }

        private void buttonX1_Click(object sender, EventArgs e)
        {
            //textBoxDropDown1.TextBox.Focus();
            //textBoxDropDown1.TextBox.SelectAll();
            //firstenter = true;
            
            Cards = searcher.NormalSearch(textBoxDropDown1.Text, 0);

            ShowSearchList(Cards);

            if (Cards.Length == 0)
                ShowNoResult();
            else if (Cards.Length == 1)
                ShowCardInfo(Cards[0]);
              
        }

        private void textBoxDropDown1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar.Equals('\r'))
                buttonX1_Click(null, null);
        }

        private void textBoxDropDown1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyValue.Equals('\r'))
                buttonX1_Click(null, null);
        }

        private void frmDict_Shown(object sender, EventArgs e)
        {
            textBoxDropDown1.TextBox.Focus();
            firstenter = false;

            CXDConfig cxdconfig = Global.cxdconfig;
            if (string.Equals(cxdconfig.GetSetting("MinimizeOnLoad"), "true", StringComparison.OrdinalIgnoreCase))
                this.WindowState = FormWindowState.Minimized;
        }

        private void richTextBox1_Click(object sender, EventArgs e)
        {
            
        }

        private void richTextBox1_Enter(object sender, EventArgs e)
        {
            this.textBoxDropDown1.TextBox.Focus();
        }

        private void frmDict_SizeChanged(object sender, EventArgs e)
        {
            textBoxDropDown1.Size = new Size(this.Size.Width - 455 + 376, textBoxDropDown1.Size.Height);
            buttonX1.Location = new Point(textBoxDropDown1.Location.X + textBoxDropDown1.Size.Width + 6, buttonX1.Location.Y);
            listView2.Size = new Size(listView1.Size.Width - 2, listView1.Height - listView1.ClientRectangle.Height + 13);

            if (this.WindowState == FormWindowState.Minimized)
            {
                if (string.Equals(Global.cxdconfig.GetSetting("HideToTrayOnMinimized"), "true", StringComparison.OrdinalIgnoreCase))
                    this.Hide();
            }
            else
                fws = this.WindowState;
        }

        ListViewItem emptyItem = new ListViewItem();
        private void listView1_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            e.Item = emptyItem;
            if (resultItem.Length == 0 || e.ItemIndex >= resultItem.Length)
            {
                //e.Item = emptyItem;
                return;
            }

            e.Item = resultItem[e.ItemIndex];
        }

        //还原窗口
        public void RestoreWindow()
        {
            this.Show();
            this.WindowState = fws;
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            RestoreWindow();
        }

        private void frmDict_StyleChanged(object sender, EventArgs e)
        {

        }

        private void frmDict_Resize(object sender, EventArgs e)
        {

        }

        private void listView2_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            for (int i = 0; i < listView1.Columns.Count; i++)
            {
                listView1.Columns[i].Width = listView2.Columns[i].Width;
            }
        }

        private void ribbonControl1_DoubleClick(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
                this.WindowState = FormWindowState.Maximized;
            else
                this.WindowState = FormWindowState.Normal;
        }

        private void ribbonControl1_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
                this.WindowState = FormWindowState.Maximized;
            else
                this.WindowState = FormWindowState.Normal;
        }

        private void panel1_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
                this.WindowState = FormWindowState.Maximized;
            else
                this.WindowState = FormWindowState.Normal;
        }

        private void ribbonControl1_ItemClick(object sender, EventArgs e)
        {
            Point p = Cursor.Position;
            p = new Point(p.X - this.Location.X, p.Y - this.Location.Y);
            if (p.Y < 30 && p.X > 50 && p.X < ribbonControl1.Width - 50)
            {
                int now = System.Environment.TickCount;

                if (lastcaptionclicktime == 0)
                {
                    lastcaptionclicktime = now;
                }
                else if (now - lastcaptionclicktime < 700)
                {
                    if (this.WindowState == FormWindowState.Normal)
                        this.WindowState = FormWindowState.Maximized;
                    else
                        this.WindowState = FormWindowState.Normal;
                    lastcaptionclicktime = 0;
                }
                else
                {
                    lastcaptionclicktime = now;
                }
            }
        }

        private void textBoxDropDown1_Enter(object sender, EventArgs e)
        {
            //textBoxDropDown1.TextBox.HideSelection = false;
            //textBoxDropDown1.SelectionStart = 0;
            //textBoxDropDown1.SelectionLength = textBoxDropDown1.TextBox.Text.Length;
            //textBoxDropDown1.
            //string s = textBoxDropDown1.Text;
            //textBoxDropDown1.Text = "";

            //Tools.API.SendMessage(textBoxDropDown1.TextBox.Handle, Tools.APIConst.EM_SETSEL, 0, new IntPtr(-1));   
            /*
            if (firstenter)
            {
                firstenter = false;
                buttonX1.Focus();
                buttonX1_Click(null, null);  
            }
            */
            firstenter = true;
        }

        private void textBoxDropDown1_MouseUp(object sender, MouseEventArgs e)
        {
            if (firstenter)
            {
                firstenter = false;
                if (textBoxDropDown1.TextBox.SelectionLength == 0)
                    textBoxDropDown1.TextBox.SelectAll();
                
            }
        }

        private void panelEx1_MouseLeave(object sender, EventArgs e)
        {
            panelEx1.Cursor = System.Windows.Forms.Cursors.Default;
        }

        private void panelEx1_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragingborder)
            {
                if (e.Button != MouseButtons.Left)
                    dragingborder = false;
            }
            else
            {
                Point p = Cursor.Position;
                p = new Point(p.X - this.Location.X, p.Y - this.Location.Y);
                if (p.X < 15)
                {
                    if (p.Y < 10)
                    {
                        panelEx1.Cursor = System.Windows.Forms.Cursors.SizeNWSE;
                        cursor = EnumMousePointPosition.MouseSizeTopLeft;
                    }
                    else if (p.Y + 10 > this.Height)
                    {
                        panelEx1.Cursor = System.Windows.Forms.Cursors.SizeNESW;
                        cursor = EnumMousePointPosition.MouseSizeBottomLeft;
                    }
                    else
                    {
                        panelEx1.Cursor = System.Windows.Forms.Cursors.SizeWE;
                        cursor = EnumMousePointPosition.MouseSizeLeft;
                    }
                }
                else if (p.X + 15 > this.Width)
                {
                    if (p.Y < 10)
                    {
                        panelEx1.Cursor = System.Windows.Forms.Cursors.SizeNESW;
                        cursor = EnumMousePointPosition.MouseSizeTopRight;
                    }
                    else if (p.Y + 10 > this.Height)
                    {
                        panelEx1.Cursor = System.Windows.Forms.Cursors.SizeNWSE;
                        cursor = EnumMousePointPosition.MouseSizeBottomRight;
                    }
                    else
                    {
                        panelEx1.Cursor = System.Windows.Forms.Cursors.SizeWE;
                        cursor = EnumMousePointPosition.MouseSizeRight;
                    }
                }
                else
                {
                    if (p.Y < 10)
                    {
                        panelEx1.Cursor = System.Windows.Forms.Cursors.SizeNS;
                        cursor = EnumMousePointPosition.MouseSizeTop;
                    }
                    else if (p.Y + 10 > this.Height)
                    {
                        panelEx1.Cursor = System.Windows.Forms.Cursors.SizeNS;
                        cursor = EnumMousePointPosition.MouseSizeBottom;
                    }
                    else
                    {
                        panelEx1.Cursor = System.Windows.Forms.Cursors.Default;
                        cursor = EnumMousePointPosition.MouseSizeNone;
                    }
                }
            }
        }

        private void panelEx1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                dragingborder = true;
                startdragborderpoint = e.Location;
            }
        }

        private void panelEx1_MouseUp(object sender, MouseEventArgs e)
        {
            if (dragingborder)
            {
                int neww = 0;
                int newh = 0;
                int newx = 0;
                int newy = 0;

                switch (cursor)
                {
                    case EnumMousePointPosition.MouseSizeTopLeft:
                        neww = this.Width - e.X + startdragborderpoint.X;
                        newh = this.Height - e.Y + startdragborderpoint.Y;
                        newx = this.Location.X + e.X;
                        newy = this.Location.Y + e.Y;
                        break;
                    case EnumMousePointPosition.MouseSizeTop:
                        neww = this.Width;
                        newh = this.Height - e.Y + startdragborderpoint.Y;
                        newx = this.Location.X;
                        newy = this.Location.Y + e.Y;
                        break;
                    case EnumMousePointPosition.MouseSizeTopRight:
                        neww = this.Width + e.X - startdragborderpoint.X;
                        newh = this.Height - e.Y + startdragborderpoint.Y;
                        newx = this.Location.X;
                        newy = this.Location.Y + e.Y;
                        break;
                    case EnumMousePointPosition.MouseSizeRight:
                        neww = this.Width + e.X - startdragborderpoint.X;
                        newh = this.Height;
                        newx = this.Location.X;
                        newy = this.Location.Y;
                        break;
                    case EnumMousePointPosition.MouseSizeBottomRight:
                        neww = this.Width + e.X - startdragborderpoint.X;
                        newh = this.Height + e.Y - startdragborderpoint.Y;
                        newx = this.Location.X;
                        newy = this.Location.Y;
                        break;
                    case EnumMousePointPosition.MouseSizeBottom:
                        neww = this.Width;
                        newh = this.Height + e.Y - startdragborderpoint.Y;
                        newx = this.Location.X;
                        newy = this.Location.Y;
                        break;
                    case EnumMousePointPosition.MouseSizeBottomLeft:
                        neww = this.Width - e.X + startdragborderpoint.X;
                        newh = this.Height + e.Y - startdragborderpoint.Y;
                        newx = this.Location.X + e.X;
                        newy = this.Location.Y;
                        break;
                    case EnumMousePointPosition.MouseSizeLeft:
                        neww = this.Width - e.X + startdragborderpoint.X;
                        newh = this.Height;
                        newx = this.Location.X + e.X;
                        newy = this.Location.Y;
                        break;
                }

                if (neww < this.MinimumSize.Width)
                    neww = MinimumSize.Width;
                if (newh < this.MinimumSize.Height)
                    newh = MinimumSize.Height;

                this.Size = new Size(neww, newh);
                this.Location = new Point(newx, newy);
            }
            dragingborder = false;
        }

        private void listView1_MouseDown(object sender, MouseEventArgs e)
        {
            int clicknum = 2;
            if (string.Equals(Global.cxdconfig.GetSetting("ChooseByClick"), "true", StringComparison.OrdinalIgnoreCase))
                clicknum = 1;

            if (e.Clicks == clicknum && e.Button == MouseButtons.Left)
            {
                ListViewItem item = listView1.GetItemAt(e.X, e.Y);
                if (item == null)
                    return;

                string name = item.Text;
                CardDescription card = CardLibrary.GetInstance().GetCardByName(name);

                this.ShowCardInfo(card);
            }
        }

        private void buttonItem14_Click(object sender, EventArgs e)
        {
            splitContainer1.Visible = true;
            richTextBox1.Visible = false;
            panel2.Visible = false;
            listView1.Focus();
        }

        private void buttonItem16_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(CurrentCard.GetAllInfo());
        }

        private void buttonItem17_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(CurrentCard.adjust);
        }

        private void buttonItem15_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(CurrentCard.name);
        }

        private void buttonItem18_Click(object sender, EventArgs e)
        {
            if (richTextBox1.SelectionLength > 0)
                Clipboard.SetText(richTextBox1.SelectedText);
        }

        private void buttonItem20_Click(object sender, EventArgs e)
        {
            if (richTextBox1.Text.Length > 0)
                Clipboard.SetText(richTextBox1.Text);
        }

        private void buttonItem19_Click(object sender, EventArgs e)
        {
            richTextBox1.SelectAll();
        }

        private void ShowConfigForm()
        {
            frmConfig form = new frmConfig();
            form.ShowDialog(this);
        }

        private void buttonItem12_Click(object sender, EventArgs e)
        {
            ShowConfigForm();
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            ShowConfigForm();
        }

        private void frmDict_FormClosed(object sender, FormClosedEventArgs e)
        {
            UnRegKey(Handle, hkGrabEnable);
            UnRegKey(Handle, hkMainFormVisble);
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (FloatForm.HideForm())
                timer1.Stop();
            else
                timer1.Start();
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            notifyIcon1_DoubleClick(null, null);
        }

        private void buttonItem18_Click_1(object sender, EventArgs e)
        {
            Clipboard.SetText(richTextBox1.SelectedText);
        }

        private void contextMenuBar1_PopupOpen(object sender, PopupOpenEventArgs e)
        {
            if (CurrentCard == null)
            {
                buttonItem15.Enabled = false;
                buttonItem16.Enabled = false;
                buttonItem17.Enabled = false;
            }
            else
            {
                if (richTextBox1.SelectionLength == 0)
                    buttonItem18.Enabled = false;

                if (CurrentCard.adjust.Trim() == "")
                    buttonItem17.Enabled = false;
            }
        }

        private void toolStripMenuItem6_DropDownOpening(object sender, EventArgs e)
        {
            mAppInterface.GetAppDirEx(mAppInterface.AppName_NBX, ref NBXDir);
            NBXDir += "\\NetBattleX.exe";
            if (File.Exists(NBXDir))
                toolStripMenuItem7.Visible = true;
            else
                toolStripMenuItem7.Visible = false;

            mAppInterface.GetAppDirEx(mAppInterface.AppName_NBXE, ref NBXEDir);
            NBXEDir += "\\NBX.Evolution.exe";
            if (File.Exists(NBXEDir))
                toolStripMenuItem8.Visible = true;
            else
                toolStripMenuItem8.Visible = false;

            mAppInterface.GetAppDirEx(mAppInterface.AppName_YFCC, ref YFCCDir);
            YFCCDir += "\\YFCC3.exe";
            if (File.Exists(YFCCDir))
                toolStripMenuItem9.Visible = true;
            else
                toolStripMenuItem9.Visible = false;

            mAppInterface.GetAppDirEx(mAppInterface.AppName_DIYTool, ref DIYToolDir);
            DIYToolDir += "\\DIYer.exe";
            if (File.Exists(DIYToolDir))
                toolStripMenuItem10.Visible = true;
            else
                toolStripMenuItem10.Visible = false;

            DeckBuilderDir = Global.appPath + "\\DeckBuilder2.exe";
            if (File.Exists(DeckBuilderDir))
                toolStripMenuItem11.Visible = true;
            else
                toolStripMenuItem11.Visible = false;
        }

        private void toolStripMenuItem11_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(DeckBuilderDir);
            }
            catch
            {
            }
        }

        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(NBXDir);
            }
            catch
            {
            }
        }

        private void toolStripMenuItem8_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(NBXEDir);
            }
            catch
            {
            }
        }

        private void toolStripMenuItem9_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(YFCCDir);
            }
            catch
            {
            }
        }

        private void toolStripMenuItem10_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(DIYToolDir);
            }
            catch
            {
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void splitContainer1_MouseMove(object sender, MouseEventArgs e)
        {
           
        }
    }

    enum EnumMousePointPosition
    {
        MouseSizeNone = 0,//无

        MouseSizeRight = 1,//拉伸右边框

        MouseSizeLeft = 2,//拉伸左边框

        MouseSizeBottom = 3,//拉伸下边框

        MouseSizeTop = 4,//'拉伸上边框

        MouseSizeTopLeft = 5,//'拉伸左上角

        MouseSizeTopRight = 6,//拉伸右上角

        MouseSizeBottomLeft = 7,//拉伸左下角

        MouseSizeBottomRight = 8,//拉伸右下角

        MouseDrag = 9,//鼠标拖动

    }
}
