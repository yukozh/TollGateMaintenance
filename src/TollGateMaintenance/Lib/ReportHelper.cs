using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TollGateMaintenance.Lib
{
    public static class ReportHelper
    {
        public static string GetManagement(string src)
        {
            var tmp = src.Replace("\r", "").Split('\n').ToList();
            var index = tmp.IndexOf("管理处：");
            if (index < 0 || index + 1 >= tmp.Count)
                return null;
            return tmp[index + 1].Trim();
        }

        public static string GetTollGate(string src)
        {
            var tmp = src.Replace("\r", "").Split('\n').ToList();
            var index = tmp.IndexOf("收费站：");
            if (index < 0 || index + 1 >= tmp.Count)
                return null;
            return tmp[index + 1].Trim();
        }

        public static IEnumerable<string> GetSolvedIssues(string src)
        {
            var ret = new List<string>();
            var tmp = src.Replace("\r", "").Split('\n').ToList();
            var beginIndex = tmp.IndexOf(tmp.FirstOrDefault(x => x.StartsWith("解决的问题：")));
            var endIndex = tmp.IndexOf(tmp.FirstOrDefault(x => x.StartsWith("遗留问题及未问题未解决原因：") || x.StartsWith("遗留问题及未解决原因：")));
            for (var i = beginIndex; i < endIndex; i++)
                ret.Add(tmp[i]);

            // 处理用户将内容填写在冒号后，没换行的情况
            if (ret.Count > 0)
            {
                if (string.IsNullOrWhiteSpace(ret.First().Replace("解决的问题：", "").Trim()))
                {
                    ret.RemoveAt(0);
                }
                else
                {
                    if (ret.Count == 1)
                    {
                        ret[0] = ret[0].Substring(5);
                    }
                }
            }
            // 处理分号、句号、空格
            var fh = ret[0].Split('；');
            var jh = ret[0].Split('。');
            var kg = ret[0].Split(' ');
            var dkg = ret[0].Split('　');
            if (Math.Max(Math.Max(Math.Max(Math.Max(fh.Count(), jh.Count()), kg.Count()), dkg.Count()), ret.Count) == ret.Count)
            {

            }
            else if (Math.Max(Math.Max(Math.Max(Math.Max(fh.Count(), jh.Count()), kg.Count()), dkg.Count()), ret.Count) == fh.Count())
            {
                ret = ret[0].Split('；').ToList();
            }
            else if (Math.Max(Math.Max(Math.Max(Math.Max(fh.Count(), jh.Count()), kg.Count()), dkg.Count()), ret.Count) == jh.Count())
            {
                ret = ret[0].Split('。').ToList();
            }
            else if (Math.Max(Math.Max(Math.Max(Math.Max(fh.Count(), jh.Count()), kg.Count()), dkg.Count()), ret.Count) == dkg.Count())
            {
                ret = ret[0].Split('　').ToList();
            }
            else
            {
                ret = ret[0].Split(' ').ToList();
            }
            for (var i = 0; i < ret.Count; i++)
            {
                // 处理顿号
                if (ret[i].IndexOf('、') >= 0 && ret[i].IndexOf('、') <= 8)
                {
                    ret[i] = ret[i].Substring(ret[i].IndexOf('、') + 1).Trim();
                }
                // 处理点
                if (ret[i].IndexOf('.') >= 0 && ret[i].IndexOf('.') <= 8)
                {
                    ret[i] = ret[i].Substring(ret[i].IndexOf('.') + 1).Trim();
                }
                // 处理结尾
                if (ret[i].EndsWith("；"))
                {
                    ret[i] = ret[i].Substring(0, ret[i].Length - 1);
                }
            }
            return ret;
        }

        public static IEnumerable<string> GetUnsolvedIssues(string src)
        {
            var ret = new List<string>();
            var tmp = src.Replace("\r", "").Split('\n').ToList();
            var beginIndex = tmp.IndexOf(tmp.FirstOrDefault(x => x.StartsWith("遗留问题及未解决原因：") || x.StartsWith("遗留问题及未问题未解决原因")));
            var endIndex = tmp.IndexOf(tmp.FirstOrDefault(x => x.StartsWith("意见和建议：")));
            for (var i = beginIndex; i < endIndex; i++)
                ret.Add(tmp[i]);

            // 处理用户将内容填写在冒号后，没换行的情况
            if (ret.Count > 0)
            {
                if (string.IsNullOrWhiteSpace(ret.First().Replace("遗留问题及未问题未解决原因：", "").Replace("遗留问题及未解决原因：", "").Trim()))
                {
                    ret.RemoveAt(0);
                }
                else
                {
                    if (ret.Count == 1)
                    {
                        ret[0] = ret[0].Substring(5);
                    }
                }
            }
            // 处理分号、句号、空格
            var fh = ret[0].Split('；');
            var jh = ret[0].Split('。');
            var kg = ret[0].Split(' ');
            var dkg = ret[0].Split('　');
            if (Math.Max(Math.Max(Math.Max(Math.Max(fh.Count(), jh.Count()), kg.Count()), dkg.Count()), ret.Count) == ret.Count)
            {

            }
            else if (Math.Max(Math.Max(Math.Max(Math.Max(fh.Count(), jh.Count()), kg.Count()), dkg.Count()), ret.Count) == fh.Count())
            {
                ret = ret[0].Split('；').ToList();
            }
            else if (Math.Max(Math.Max(Math.Max(Math.Max(fh.Count(), jh.Count()), kg.Count()), dkg.Count()), ret.Count) == jh.Count())
            {
                ret = ret[0].Split('。').ToList();
            }
            else if (Math.Max(Math.Max(Math.Max(Math.Max(fh.Count(), jh.Count()), kg.Count()), dkg.Count()), ret.Count) == dkg.Count())
            {
                ret = ret[0].Split('　').ToList();
            }
            else
            {
                ret = ret[0].Split(' ').ToList();
            }
            for (var i = 0; i < ret.Count; i++)
            {
                // 处理顿号
                if (ret[i].IndexOf('、') >= 0 && ret[i].IndexOf('、') <= 8)
                {
                    ret[i] = ret[i].Substring(ret[i].IndexOf('、') + 1).Trim();
                }
                // 处理点
                if (ret[i].IndexOf('.') >= 0 && ret[i].IndexOf('.') <= 8)
                {
                    ret[i] = ret[i].Substring(ret[i].IndexOf('.') + 1).Trim();
                }
                // 处理结尾
                if (ret[i].EndsWith("；"))
                {
                    ret[i] = ret[i].Substring(0, ret[i].Length - 1);
                }
            }
            return ret;
        }

    }
}
