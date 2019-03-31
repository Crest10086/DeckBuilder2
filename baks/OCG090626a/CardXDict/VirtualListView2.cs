using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace CardXDict
{
    public class VirtualListView2 : System.Windows.Forms.ListView//DevComponents.DotNetBar.Controls.ListViewEx //
    {

        /// 

        /// Gets or sets the number of System.Windows.Forms.ListViewItem objects contained

        /// in the item cache when in virtual mode.

        /// 

        /// 

        /// The number of System.Windows.Forms.ListViewItem objects contained in the

        /// VirtualListView.VirtualListSize when in virtual mode.

        /// 

        /// 

        /// VirtualListView.VirtualListSize is set to a value less than 0.

        /// 

        /// 

        /// System.Windows.Forms.ListView.VirtualMode is set to true, VirtualListView.VirtualListSize

        /// is greater than 0, and System.Windows.Forms.ListView.RetrieveVirtualItem is not handled.

        /// 

        /// 

        /// Fixes bug defined at http://forums.microsoft.com/MSDN/ShowPost.aspx?PostID=150133&SiteID=1

        /// 

        //[DefaultValue(0)]

        //[RefreshProperties(RefreshProperties.Repaint)]

        public new int VirtualListSize
        {

            get { return base.VirtualListSize; }

            set
            {

                // Must set top value to at least one less than value due to 

                // off-by-one error in base.VirtualListSize

                int topIndex = this.TopItem == null ? 0 : this.TopItem.Index;

                topIndex = Math.Min(topIndex, value - 1);

                if (topIndex > 0)

                    this.TopItem = this.Items[topIndex];


                base.VirtualListSize = value;

            }

        }

        public ListViewItem GetNearestItem(int x, int y)
        {
            if (Items.Count == 0)
                return null;

            ListViewItem nearestitem = null;

            if (View == View.Details)
            {
                nearestitem = GetItemAt(x, y);
                if (nearestitem == null)
                    nearestitem = Items[Items.Count - 1];
            }
            else if (View == View.LargeIcon)
            {
                int count = 0;
                if (VirtualMode)
                {
                    count = VirtualListSize;
                }
                else
                {
                    count = Items.Count;
                }

                

                if (count == 1)
                    nearestitem = Items[0];
                else if (count > 1)
                {
                    nearestitem = GetItemAt(x - 5, y - 10);
                    if (nearestitem == null)
                        nearestitem = GetItemAt(x + 5, y - 10);
                    if (nearestitem == null)
                        nearestitem = GetItemAt(x - 5, y + 10);
                    if (nearestitem == null)
                        nearestitem = GetItemAt(x + 5, y + 10);
                    if (nearestitem == null)
                        nearestitem = GetItemAt(x - 60, y - 10);
                    if (nearestitem == null)
                        nearestitem = GetItemAt(x - 60, y + 10);
                }

                if (nearestitem == null)
                    nearestitem = Items[count - 1];
            }

            return nearestitem;
        }

    }

}
