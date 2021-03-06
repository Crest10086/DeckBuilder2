using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Tools;

namespace BaseCardLibrary.Search
{
    public class QueryMapper
    {
       
        static Regex regex1 = new Regex(@" (&&?)|(and) ", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static Regex regex2 = new Regex(@" or ", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static Regex regex3 = new Regex(@"\b(中文|卡)名:", RegexOptions.Compiled);
        static Regex regex4 = new Regex(@"\b日文名:", RegexOptions.Compiled);
        static Regex regex5 = new Regex(@"\b(旧卡|曾用)名:", RegexOptions.Compiled);
        static Regex regex6 = new Regex(@"\b(卡种|卡片种类):", RegexOptions.Compiled);
        static Regex regex7 = new Regex(@"\b种族:", RegexOptions.Compiled);
        static Regex regex8 = new Regex(@"\b属性:", RegexOptions.Compiled);
        static Regex regex9 = new Regex(@"\b效果(说明)?:", RegexOptions.Compiled);
        static Regex regex10 = new Regex(@"\b调整:", RegexOptions.Compiled);
        static Regex regex11 = new Regex(@"\b卡包:", RegexOptions.Compiled);
        static Regex regex12 = new Regex(@"\b(罕见|稀有)度:", RegexOptions.Compiled);
        static Regex regex13 = new Regex(@"\b攻(击力?)?:", RegexOptions.Compiled);
        static Regex regex14 = new Regex(@"\b防(御力?)?:", RegexOptions.Compiled);
        static Regex regex15 = new Regex(@"\b(星(级|数)?|等级):", RegexOptions.Compiled);
        static Regex regex16 = new Regex(@"\b禁(限数(|量))?:", RegexOptions.Compiled);
        static Regex regex17 = new Regex(@"\b(?<field>(?:atkValue)|(?:defValue)|(?:level)):(?<num1>\d+)(?:-(?<num2>\d+))?", RegexOptions.Compiled);
        static Regex regex18 = new Regex(@"\b(简称|俗称|缩写):", RegexOptions.Compiled);
        static Regex regex19 = new Regex(@"\b(自?编|序)号:", RegexOptions.Compiled);
        public static string Mapper(string query)
        {
            string s = query.Trim();

            s = CharacterSet.SBCToDBC(s);

            s = regex1.Replace(s, " AND ");

            s = regex2.Replace(s, " OR ");

            s = regex3.Replace(s, "name:");

            s = regex4.Replace(s, "japName:");

            s = regex5.Replace(s, "oldName:");

            s = regex18.Replace(s, "shortName:");

            s = regex6.Replace(s, "cardType:");

            s = regex7.Replace(s, "tribe:");

            s = regex8.Replace(s, "element:");

            s = regex9.Replace(s, "effect:");

            s = regex10.Replace(s, "adjust:");

            s = regex11.Replace(s, "package:");

            s = regex12.Replace(s, "infrequence:");

            s = regex13.Replace(s, "atkValue:");

            s = regex14.Replace(s, "defValue:");

            s = regex15.Replace(s, "level:");

            s = regex16.Replace(s, "limit:");

            s = regex19.Replace(s, "ID:");

            MatchCollection matches = regex17.Matches(s);
            for (int i = 0; i < matches.Count; i++)
            {
                Match match = matches[matches.Count - 1 - i];
                string numlength = "";
                switch (match.Groups["field"].Value)
                {
                    case "atkValue":
                        numlength = "4";
                        break;
                    case "defValue":
                        numlength = "4";
                        break;
                    case "level":
                        numlength = "2";
                        break;
                }
                string s2 = null;
                int n1 = 0;
                int n2 = 9999;

                if (match.Groups["num2"].Success)
                {
                    try
                    {
                        n1 = int.Parse(match.Groups["num1"].Value);
                        n2 = int.Parse(match.Groups["num2"].Value);
                    }
                    catch
                    {
                    }
                    s2 = string.Format("{0}:[{1:D" + numlength + "} TO {2:D" + numlength + "}]", match.Groups["field"].Value, n1, n2);
                }
                else
                {
                    try
                    {
                        n1 = int.Parse(match.Groups["num1"].Value);
                    }
                    catch
                    {
                    }
                    s2 = string.Format("{0}:{1:D" + numlength + "}", match.Groups["field"].Value, n1);
                }

                if (match.Index > 0)
                    s = s.Substring(0, match.Index) + s2 + s.Substring(match.Index + match.Length, s.Length - match.Index - match.Length);
                else
                    s = s2 + s.Substring(match.Index + match.Length, s.Length - match.Index - match.Length);
            }

            //MessageBox.Show(s);
            return s;
        }
    }
}
