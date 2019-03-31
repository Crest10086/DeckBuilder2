using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Lucene.Net.Search;

namespace BaseCardLibrary.Search
{
    class MySorterNode
    {
        public SortField Field = null;
        public bool Unique = false;
    }

    public class MySorter
    {
        ArrayList sortFields = null;

        public MySorter()
        {
            sortFields = new ArrayList();
        }

        public void AddField(string fieldname, int fieldType)
        {
            AddField(fieldname, fieldType, false, true);
        }

        public void AddField(string fieldname, int fieldType, bool unique)
        {
            AddField(fieldname, fieldType, unique, true);
        }

        public void AddField(string fieldname, int fieldType, bool unique, bool reverse)
        {
            MySorterNode node = new MySorterNode();
            node.Field = new SortField(fieldname, fieldType, reverse);
            node.Unique = unique;
            sortFields.Add(node);
        }

        public void SortBy(string field)
        {
            for (int i = 0; i < sortFields.Count; i++)
            {
                MySorterNode node = (MySorterNode)sortFields[i];
                SortField sf = node.Field;
                if (field.Equals(sf.GetField()))
                {
                    if (i == 0)
                    {
                        node.Field = new SortField(sf.GetField(), sf.GetType(), !sf.GetReverse());
                    }
                    sortFields.RemoveAt(i);
                    sortFields.Insert(0, node);

                }
            }
        }

        public SortField[] GetSortFields()
        {
            int validcount = 0;
            int count = sortFields.Count; 
            for (int i = 0; i < count; i++)
            {
                MySorterNode node = (MySorterNode)sortFields[i];
                validcount++;
                if (node.Unique)
                {
                    break;
                }
            }

            SortField[] sfs = new SortField[validcount];
            for (int i = 0; i < validcount; i++)
            {
                sfs[i] = ((MySorterNode)sortFields[i]).Field;
            }

            return sfs;
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
