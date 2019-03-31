using System;
using System.Data;
using System.Data.OleDb;
using System.Data.SQLite;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace BaseCardLibrary.DataAccess
{
    public class LimitedListManager
    {
        private string XmlFileName = "LimtedList.xml";
        private static LimitedListManager instance = null;
        private ArrayList LimitedLists = null;
        private int selectedIndex = -1;

        public int SelectedIndex
        {
            get { return selectedIndex; }
            set
            { 
                selectedIndex = value;
                //CLConfig.GetInstance().SetSetting("LimitedList", selectedIndex.ToString());
            }
        }

        public CardLimitedList SelcetedItem
        {
            get 
            {
                if (selectedIndex < 0 || LimitedLists == null || selectedIndex >= LimitedLists.Count)
                    return null;
                return (CardLimitedList)LimitedLists[selectedIndex];
            }
        }

        public CardLimitedList[] GetItems()
        {
            if (LimitedLists == null)
                return new CardLimitedList[0];

            return (CardLimitedList[])LimitedLists.ToArray(typeof(CardLimitedList));
        }

        public static LimitedListManager GetInstance()
        {
            if (instance == null)
                instance = new LimitedListManager();
            return instance;
        }

        public LimitedListManager()
        {
            //LoadFromXML();
            CLConfig config = CLConfig.GetInstance();
        }

        public bool LoadFromYFCC(string filename)
        {
            LimitedLists = new ArrayList();

            //连接数据库
            OleDbConnection con = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data source=" + filename + ";Persist Security Info=False;Jet OLEDB:Database Password=paradisefox@sohu.com;");
            con.Open();

            OleDbCommand dcc = new OleDbCommand("Select * FROM [BANDATA] Order By [Dates], [ID]", con);
            OleDbDataReader creader = null;
            try
            {
                //如果数据库中存在效果种类列表则优先读取数据库中的记录
                creader = dcc.ExecuteReader();

                while (creader.Read())
                {
                    CardLimitedList cll = new CardLimitedList();
                    cll.EffectiveDate = DateTime.Parse(creader["Dates"].ToString());
                    cll.ForbiddenList = creader["Debar"].ToString();
                    cll.LimitedList = creader["Confine"].ToString();
                    cll.SemiLimitedList = creader["SubConfine"].ToString();
                    cll.Desc = creader["BanDesc"].ToString();

                    string s = "OT共用";
                    if (cll.Desc.Contains("游戏王OCG最新禁限卡表") || cll.Desc.Contains("最新禁限Ｏ卡表"))
                        s = "OCG";
                    else if (cll.Desc.Contains("游戏王TCG最新禁限卡表") || cll.Desc.Contains("最新禁限Ｔ卡表"))
                        s = "TCG";
                    cll.Name = cll.EffectiveDate.ToShortDateString() + "　" + s + "禁限卡表";

                    LimitedLists.Add(cll);
                }
            }
            catch
            {
                creader.Close();
                con.Close();
                return false;
            }

            creader.Close();
            con.Close();
            SelectedIndex = -1;
            return true;
        }

        public bool SaveToXML()
        {
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(ArrayList), new Type[] { typeof(CardLimitedList) });
                System.IO.StreamWriter file = new System.IO.StreamWriter(XmlFileName);
                ser.Serialize(file, LimitedLists);
                file.Close();
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool LoadFromXML()
        {
            if (!File.Exists(XmlFileName))
                return false;

            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(ArrayList), new Type[] { typeof(CardLimitedList) });
                System.IO.StreamReader file = new System.IO.StreamReader(XmlFileName);
                LimitedLists = (ArrayList)ser.Deserialize(file);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public CardLimitedList GetLimitedList(int index)
        {
            if (index < LimitedLists.Count)
                return (CardLimitedList)LimitedLists[index];
            else
                return null;
        }
    }
}
