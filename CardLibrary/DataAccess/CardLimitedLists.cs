using System;
using System.Collections.Generic;
using System.Text;

namespace BaseCardLibrary.DataAccess
{
    //禁卡表
    [Serializable]
    public class CardLimitedList
    {
        //名称
        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        //生效日期
        private DateTime effectiveDate;
        public DateTime EffectiveDate
        {
            get { return effectiveDate; }
            set { effectiveDate = value; }
        }

        //禁卡列表
        private string forbiddenList;
        public string ForbiddenList
        {
            get { return forbiddenList; }
            set { forbiddenList = value; }
        }

        //限制卡列表
        private string limitedList;
        public string LimitedList
        {
            get { return limitedList; }
            set { limitedList = value; }
        }

        //准限制卡列表
        private string semiLimitedList;
        public string SemiLimitedList
        {
            get { return semiLimitedList; }
            set { semiLimitedList = value; }
        }

        //禁卡表描述
        private string desc;
        public string Desc
        {
            get { return desc; }
            set { desc = value; }
        }
    }
}
