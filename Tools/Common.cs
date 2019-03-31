using System;
using System.Collections.Generic;
using System.Text;

namespace Tools
{
    public class Common
    {
        public static string DirToPath(string dir)
        {
            if (dir == null || dir.Length == 0)
                return null;

            if (dir[dir.Length - 1] == '\\')
                return dir;
            else
                return dir + "\\";
        }
    }
}
