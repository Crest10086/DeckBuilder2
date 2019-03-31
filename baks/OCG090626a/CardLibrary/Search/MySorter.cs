using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Lucene.Net.Search;

namespace BaseCardLibrary.Search
{
    public class MySorter
    {
        ArrayList sortFields = null;

        public MySorter()
        {
            sortFields = new ArrayList();
        }

        public MySorter(string[] fields)
        {
            sortFields = new ArrayList(fields.Length);
            foreach (string f in fields)
            {
                sortFields.Add(new SortField(f, SortField.STRING, true));
            }
        }

        public MySorter(string[] fields, int[] fieldType)
        {
            sortFields = new ArrayList(fields.Length);
            for (int i=0; i<fields.Length; i++)
            {
                sortFields.Add(new SortField(fields[i], fieldType[i], true));
            }
        }

        public MySorter(string[] fields, int[] fieldType, bool reverse)
        {
            sortFields = new ArrayList(fields.Length);
            for (int i = 0; i < fields.Length; i++)
            {
                sortFields.Add(new SortField(fields[i], fieldType[i], reverse));
            }
        }

        public MySorter(SortField[] fields)
        {
            sortFields = new ArrayList(fields.Length);
            foreach (SortField sf in fields)
            {
                sortFields.Add(sf);
            }
        }

        public void AddField(string field, int fieldType)
        {
            sortFields.Add(new SortField(field, fieldType, true));
        }

        public void AddField(string field, int fieldType, bool reverse)
        {
            sortFields.Add(new SortField(field, fieldType, reverse));
        }

        public void SortBy(string field)
        {
            for (int i = 0; i < sortFields.Count; i++)
            {
                SortField sf = (SortField)sortFields[i];
                if (field.Equals(sf.GetField()))
                {
                    if (i == 0)
                    {
                        sf = new SortField(sf.GetField(), !sf.GetReverse());
                    }
                    sortFields.RemoveAt(i);
                    sortFields.Insert(0, sf);

                }
            }
        }

        public SortField[] GetSortFields()
        {
            return (SortField[])sortFields.ToArray(typeof(SortField));
        }
    }

    //继承接口IComparer
    public class MySort : IComparer
    {
        #region IComparer 成员
        public int Compare(object x, object y)
        {
            //排序
            int iResult = (int)x - (int)y;
            if(iResult == 0) iResult = -1;
            return iResult;
        }
        #endregion
    }

}
