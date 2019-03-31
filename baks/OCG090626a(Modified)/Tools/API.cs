using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Tools
{
    public class APIConst
    {
        public readonly static Int32 WM_VSCROLL = 0x0115;
        public readonly static Int32 EM_SETSEL = 0x00B1;
        public readonly static Int32 SB_TOP = 6; 
    }

    public class API
    {
        [DllImport("User32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        public static extern int SendMessage(
               IntPtr hWnd,　　　// handle to destination window 
               Int32 Msg,　　　 // message 
               Int32 wParam,　// first message parameter 
               IntPtr lParam // second message parameter 
         ); 

    }
}
