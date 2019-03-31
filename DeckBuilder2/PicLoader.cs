using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using BaseCardLibrary.DataAccess;
using BaseCardLibrary.Search;
using BaseCardLibrary;
using MyTools;

namespace DeckBuilder2
{
    class PicInfo
    {
        public int Index;
        public bool IsNeedShow;

        public PicInfo(int index, bool isNeedShow)
        {
            Index = index;
            IsNeedShow = isNeedShow;
        }
    }

    class PicLoader
    {
        public static string commonImagePath = "";
        public static string commonDiyImagePath = "";
        public static string imagePath = "";
        public static string diyImagePath = "";
        public static string icoPath = "";
        public static string diyIcoPath = "";

        CardLibrary cardLibrary = CardLibrary.GetInstance();
        Stack<PicInfo> NeedShow = null;
        string IcoPath = null;
        string DIYIcoPath = null;
        Hashtable IndexMapper = null;
        int Capacity = 0;
        bool IsRefreshing = false;
        int refreshinterval = 200;
        int loadpicinterval = 20;
        int loadcounter = 0;
        int loadoncenum = 10;

        private int Load(int index)
        {
            CardDescription card = cardLibrary.GetCardByIndex(index);
            if (card == null)
            {
                card = null;
                return 0;
            }

            string filename = null;
            if (card.cardCamp == CardCamp.DIY)
                filename = DIYIcoPath + card.name + ".jpg";
            else
                filename = IcoPath + card.ID.ToString() + ".jpg";

            System.Windows.Forms.ImageList imageList1 = Global.frmMainHolder.imageList1;
            if (System.IO.File.Exists(filename))
            {
                Global.frmMainHolder.imageList1.Images.Add(System.Drawing.Image.FromFile(filename));
                if (loadoncenum > 0 && loadpicinterval > 0)
                {
                    loadcounter++;
                    if (loadcounter == loadoncenum)
                    {
                        Thread.Sleep(loadpicinterval);
                        loadcounter = 0;
                    }
                }
            }
            else
            {
                return 0;
            }

            return imageList1.Images.Count - 1;
        }

        private void DoLoadEnd()
        {
            //System.Windows.Forms.MessageBox.Show("载入图标完成！");
            if (string.Equals(DB2Config.GetInstance().GetSetting("NoVirtualMode"), "True", StringComparison.OrdinalIgnoreCase))
            {
                MethodInvoker Invoker = new MethodInvoker(Global.frmMainHolder.LoadPicEnd);
                Global.frmMainHolder.BeginInvoke(Invoker);
            }
        }

        MethodInvoker PicInvoker = new MethodInvoker(Global.frmMainHolder.PicsLoaded);
        bool NeedRefresh = false;

        //略缩图载入线程
        private void LoadPic()
        {
            int loadedcount = 0;

            while (loadedcount < Capacity)
            {
                PicInfo picinfo;
                lock (NeedShow)
                {
                    picinfo = NeedShow.Pop();
                }

                object o = IndexMapper[picinfo.Index];

                if (o == null)
                {
                    int currentindex = Load(picinfo.Index);
                    lock (IndexMapper)
                    {
                        IndexMapper[picinfo.Index] = currentindex;
                    }
                    loadedcount++;

                    if (picinfo.IsNeedShow)
                    {
                        NeedRefresh = true;
                    }
                    else
                    {
                        if (NeedRefresh && !IsRefreshing)
                        {
                            IsRefreshing = true;
                            Global.frmMainHolder.BeginInvoke(PicInvoker); 
                            NeedRefresh = false;
                            IsRefreshing = false;
                            System.Threading.Thread.Sleep(refreshinterval);
                        }
                    }
                }
            }

            DoLoadEnd();
        }

        //构造函数（略缩图数量， 略缩图路径， DIY略缩图路径）
        public PicLoader(int capacity, string icoPath, string diyIcoPath)
        {
            DB2Config config = DB2Config.GetInstance();

            if (!string.Equals(config.GetSetting("NotShowIco"), "True", StringComparison.OrdinalIgnoreCase))
            {
                //读取参数
                refreshinterval = MyTools.Config.GetIntValue(config.GetSetting("RefreshInterval"), 200);
                loadpicinterval = MyTools.Config.GetIntValue(config.GetSetting("LoadPicInterval"), 20);
                loadoncenum = MyTools.Config.GetIntValue(config.GetSetting("LoadPicOnceNum"), 10); 

                //初始化
                Global.loadPicEnd = false;
                Capacity = capacity;
                IcoPath = FileTools.DirToPath(FileTools.RelativeToAbsolutePath(icoPath));
                DIYIcoPath = FileTools.DirToPath(FileTools.RelativeToAbsolutePath(diyIcoPath));
                NeedShow = new Stack<PicInfo>();
                IndexMapper = new Hashtable(capacity + 1);

                //将所有载入略缩图请求压栈
                for (int i = capacity-1; i >= 0; i--)
                    NeedShow.Push(new PicInfo(i, false));
                
                //在新线程载入略缩图
                Thread WorkThread = new Thread(new ThreadStart(LoadPic));
                WorkThread.IsBackground = true;
                WorkThread.Priority = ThreadPriority.BelowNormal;
                WorkThread.Start();
            }
            else
            {
                DoLoadEnd();
            }
        }

        //根据卡片ID返回略缩图序号
        public int GetLargeIcoIndex(int id)
        {
            if (string.Equals(DB2Config.GetInstance().GetSetting("NotShowIco"), "True", StringComparison.OrdinalIgnoreCase))
            {
                //如果不显示略缩图，直接返回默认略缩图序号0
                return 0;
            }
            else
            {
                //如果显示略缩图
                object o = null;
                int index = cardLibrary.GetCardIndexByID(id);

                //获取略缩图序号
                lock (IndexMapper)
                {
                    o = IndexMapper[index];
                }

                //返回略缩图序号
                if (o == null)
                {
                    //如果还没有载入，将该请求压栈，返回0
                    lock (NeedShow)
                    {
                        NeedShow.Push(new PicInfo(index, true));
                    }
                    return 0;
                }
                else
                    return (int)o;
            }
        }

        //根据卡片ID返回大图文件名
        public static string GetImagePath(int id)
        {
            //先随便找张卡测试一下图片是以密码名来存的，还是以狐查的序号名来存的
            bool isCheatCodeMode = false;
            string testfilename = "92377303.jpg";
            if (File.Exists(PicLoader.commonImagePath + testfilename) || File.Exists(PicLoader.imagePath + testfilename))
                isCheatCodeMode = true;

            if (isCheatCodeMode)
            {
                //如果是密码名保存的，不用尝试OCGSOFT的公用卡图目录，但要考虑多版本卡图的情况
                CardDescription card = CardLibrary.GetInstance().GetCardByID(id);

                string[] ss = card.cheatcode.Split(',');
                foreach (string s in ss)
                {
                    string filename = s.TrimStart(new char[] { '0' }) + ".jpg";
                    string path = PicLoader.imagePath + filename;
                    if (File.Exists(path))
                        return path;
                }
                return null;
            }
            else
            {
                //如果是以序号名来存的，优先使用OCGSOFT的公用卡图目录
                string filename = id.ToString() + ".jpg";
                string path = PicLoader.commonImagePath + filename;
                if (File.Exists(path))
                    return path;

                path = PicLoader.imagePath + filename;
                if (File.Exists(path))
                    return path;

                return null;
            }
        }

        //根据DIY卡片名返回大图文件名
        public static string GetDiyImagePath(string name)
        {
            string path = PicLoader.commonDiyImagePath + name + ".jpg";
            if (!File.Exists(path))
            {
                path = PicLoader.diyImagePath + name + ".jpg";
            }

            return path;
        }
    }
}
