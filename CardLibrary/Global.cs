using System;
using System.Collections.Generic;
using System.Text;
using BaseCardLibrary.DataAccess;

namespace BaseCardLibrary
{
    public class Global
    {
        public static string path = null;

        public static string GetPath()
        {
            if (path == null)
            {
                path = System.AppDomain.CurrentDomain.BaseDirectory;
                if (path[path.Length - 1] != '\\')
                    path += "\\";
            }
            return path;
        }
    }
}
