using System;
using System.Collections.Generic;
using System.Text;

namespace MyTools
{
    public class FileTools
    {
        //保证路径最后有斜杠
        public static string DirToPath(string dir)
        {
            if (dir == null || dir.Length == 0)
                return null;

            if (dir[dir.Length - 1] == '\\')
                return dir;
            else
                return dir + "\\";
        }

        //保证路径最后无斜杠
        public static string PathToDir(string path)
        {
            if (path == null || path.Length == 0)
                return null;

            if (path[path.Length - 1] == '\\')
                return path.Substring(0, path.Length - 1);
            else
                return path;
        }

        //相对路径转绝对路径
        public static string RelativeToAbsolutePath(string path)
        {
            if (path[0] == '.')
                path = System.AppDomain.CurrentDomain.BaseDirectory + path;

            return path;
        }
    }
}
