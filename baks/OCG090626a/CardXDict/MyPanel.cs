using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace CardXDict
{
    class MyPanel : System.Windows.Forms.Panel
    {
        protected override void OnPaint(PaintEventArgs e)
        {

            base.OnPaint(e);

            /*

            int borderWidth = 1;

            Rectangle rect = new Rectangle(this.Location, this.Size);

            Color borderColor = Color.Blue;

            ControlPaint.DrawBorder(e.Graphics, rect, borderColor,

                                borderWidth, ButtonBorderStyle.Solid, borderColor, borderWidth,

                                ButtonBorderStyle.Solid, borderColor, borderWidth, ButtonBorderStyle.Solid,

                                borderColor, borderWidth, ButtonBorderStyle.Solid);

            */

        }
    }
}
