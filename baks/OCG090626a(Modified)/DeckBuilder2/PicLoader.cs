using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using BaseCardLibrary.DataAccess;
using BaseCardLibrary.Search;
using BaseCardLibrary;

namespace DeckBuilder2
{
    class PicInfo
    {
        public int Id;
        public bool IsNeedShow;

        public PicInfo(int id, bool isNeedShow)
        {
            Id = id;
            IsNeedShow = isNeedShow;
        }
    }

    class PicLoader
    {
        Stack<PicInfo> NeedShow = null;
        string DirPath = null;
        string DIYPath = null;
        Hashtable IndexMapper = null;
        int Capacity = 0;
        static PicLoader instance = null;
        Mutex mutex = null;
        bool IsRefreshing = false;
        int refreshinterval = 200;
        int loadpicinterval = 20;
        int loadcounter = 0;
        int loadoncenum = 10;

        private int Load(int id)
        {
            CardDescription card = CardLibrary.GetInstance().GetCardByID(id);
            if (card == null)
                return 0;

            string filename = null;
            if (card.cardCamp == CardCamp.DIY)
                filename = DIYPath + card.name + ".jpg";
            else
                filename = DirPath + id.ToString() + ".jpg";

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
                return 0;

            return imageList1.Images.Count - 1;
        }

        //public delegate void cardinvoke(int id);
        MethodInvoker PicInvoker = new MethodInvoker(Global.frmMainHolder.PicsLoaded);
        bool NeedRefresh = false;
        //int RefreshTime = 0;

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

                object o = null;
                lock (IndexMapper)
                {
                    o = IndexMapper[picinfo.Id];
                }

                if (o == null)
                {
                    int currentindex = Load(picinfo.Id);
                    lock (IndexMapper)
                    {
                        IndexMapper[picinfo.Id] = currentindex;
                    }
                    loadedcount++;

                    if (picinfo.IsNeedShow)
                    {
                        NeedRefresh = true;
                        //Global.frmMainHolder.BeginInvoke(new cardinvoke(Global.frmMainHolder.PicLoaded), new Object[] { picinfo.Index });
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

            //System.Windows.Forms.MessageBox.Show("ÔØÈëÍ¼±êÍê³É£¡");
            if (string.Equals(DB2Config.GetInstance().GetSetting("NoVirtualMode"), "True", StringComparison.OrdinalIgnoreCase))
            {
                MethodInvoker Invoker = new MethodInvoker(Global.frmMainHolder.LoadPicEnd);
                Global.frmMainHolder.BeginInvoke(Invoker);
            }
        }

        public PicLoader(int capacity, string dirPath, string diyPath)
        {
            if (!string.Equals(DB2Config.GetInstance().GetSetting("NotShowIco"), "True", StringComparison.OrdinalIgnoreCase))
            {
                refreshinterval = Tools.Config.GetIntValue(DB2Config.GetInstance().GetSetting("RefreshInterval"), 200);
                loadpicinterval = Tools.Config.GetIntValue(DB2Config.GetInstance().GetSetting("LoadPicInterval"), 20);
                loadoncenum = Tools.Config.GetIntValue(DB2Config.GetInstance().GetSetting("LoadPicOnceNum"), 10); 

                Global.loadPicEnd = false;
                Capacity = capacity;
                DirPath = dirPath;
                DIYPath = diyPath;
                if (DirPath[DirPath.Length-1] != '\\')
                    DirPath += "\\";
                if (DIYPath[DIYPath.Length - 1] != '\\')
                    DIYPath += "\\";
                NeedShow = new Stack<PicInfo>();
                mutex = new Mutex();
                for (int i = capacity; i > 0; i--)
                    NeedShow.Push(new PicInfo(i, false));
                IndexMapper = new Hashtable(capacity + 1);

                Thread WorkThread = new Thread(new ThreadStart(LoadPic));
                WorkThread.IsBackground = true;
                WorkThread.Priority = ThreadPriority.BelowNormal;
                WorkThread.Start();
            }
            else
            {
                //MethodInvoker Invoker = new MethodInvoker(Global.frmMainHolder.LoadPicEnd);
                //Global.frmMainHolder.BeginInvoke(Invoker);
                Global.loadPicEnd = true;
            }
        }

        static public PicLoader GetInstance(int capacity, string dirPath, string diyPath)
        {
            instance = new PicLoader(capacity, dirPath, diyPath);
            return instance;
        }

        static public PicLoader GetInstance()
        {
            return instance;
        }

        public int GetImageIndex(int id)
        {
            if (string.Equals(DB2Config.GetInstance().GetSetting("NotShowIco"), "True", StringComparison.OrdinalIgnoreCase))
            {
                return 0;
            }
            else
            {
                object o = null;
                lock (IndexMapper)
                {
                    o = IndexMapper[id];
                }

                if (o == null)
                {
                    lock (NeedShow)
                    {
                        NeedShow.Push(new PicInfo(id, true));
                    }
                    return 0;
                }
                else
                    return (int)o;
            }
        }

        public static int GetImageIndex(BaseCardLibrary.DataAccess.CardDescription card)
        {
            int index = 0;
            switch (card.sCardType)
            {
                case "Ð§¹û¹ÖÊÞ":
                    index = 0;
                    break;
                case "ÆÕÍ¨¹ÖÊÞ":
                    index = 1;
                    break;
                case "ÈÚºÏ¹ÖÊÞ":
                    index = 2;
                    break;
                case "ÒÇÊ½¹ÖÊÞ":
                    index = 3;
                    break;
                default: 
                    if (card.sCardType.Length == 4)
                    {
                        if (card.sCardType.Substring(2, 2).Equals("Ä§·¨"))
                            index = 4;
                        else if (card.sCardType.Substring(2, 2).Equals("ÏÝÚå"))
                            index = 5;
                    }
                    break;
            }
            return index;
        }
    }
}
