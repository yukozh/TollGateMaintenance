using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using TollGateMaintenance.Hubs;
using TollGateMaintenance.Models;

namespace TollGateMaintenance.Lib
{
    public class NlpReturn
    {
        public int id { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string cont { get; set; }

        /// <summary>
        /// 词性
        /// </summary>
        public string pos { get; set; }

        /// <summary>
        /// 句法修饰关系
        /// </summary>
        public string relate { get; set; }

        public class Arg
        {
            public string type { get; set; }
            public int beg { get; set; }
            public int end { get; set; }
        }

        public class Sem
        {
            public int id { get; set; }
            public string relate { get; set; }
            public int parent { get; set; }
        }

        public IEnumerable<Arg> arg { get; set; }
        public IEnumerable<Sem> sem { get; set; }
    }

    public static class NlpHelper
    {
        private static string[] Devices = new string[] { "自动栏杆机", "自动栏杆", "票据打印机", "票据打印", "打印机", "对讲主机", "对讲分机", "对讲机", "对讲", "计重", "雾灯", "通信信号灯", "费额显示器", "费额显示", "费显", "摄像机", "摄像", "交换机", "光端机", "服务器", "工作站", "车牌识别", "编解码器", "编码器", "解码器", "视频存储", "车牌识别", "地秤", "监视器", "显示器", "字符叠加器", "字符叠加", "图像", "字符", "人井", "电视墙", "稳压电源", "配电柜", "雨棚灯", "镜头", "稳压电源", "电源" };

        private static string[] AdditionalLane = new string[] { "票证室", "监控室" };

        public static bool ContainsNumber(this string self)
        {
            var number = "1234567890一二三四五六七八九十百千万亿";
            foreach (var x in number)
                if (self.Contains(x))
                    return true;
            return false;
        }

        private static string GetLane(IEnumerable<NlpReturn> src)
        {
            try
            {
                var total = string.Join("", src.Select(x => x.cont));
                foreach (var x in AdditionalLane)
                    if (total.IndexOf(x) >= 0)
                        return x;
                var tmp = src.Take(3).ToList();
                tmp = tmp.Where(x => x.pos == "v" || x.pos == "n" || x.pos == "m").ToList();

                // v/n+m 或 m + n形式
                if (tmp.Any(x => x.pos == "m") && tmp.Count >= 2)
                {
                    var index = tmp.IndexOf(tmp.First(x => x.pos == "m")) - 1;
                    if (index >= 0 && ((tmp[index].pos == "v" && tmp[index].cont == "出口") || tmp[index].pos == "n")) // 分词系统错误将出口划分为动词
                        return tmp[index].cont + tmp[index + 1].cont;
                    index++;
                    if (index + 1 < tmp.Count && tmp[index].pos == "m" && tmp[index + 1].pos == "n")
                        return tmp[index].cont + tmp[index + 1].cont;
                }
                //v/n + n 形式
                if (tmp.Any(x => x.pos == "n") && tmp.Count >= 2)
                {
                    var index = 0; //tmp.LastIndexOf(tmp.Last(x => x.pos == "n")) - 1;
                    var matched_nn_or_vn = false;
                    if ((tmp[0].pos == "v" && tmp[0].cont == "出口" && tmp[1].pos == "n") || (tmp[0].pos == "n" && tmp[1].pos == "n"))
                    {
                        index = 0;
                        matched_nn_or_vn = true;
                    }
                    if (!matched_nn_or_vn && tmp.Count == 3)
                    {
                        if ((tmp[1].pos == "v" && tmp[1].cont == "出口" && tmp[2].pos == "n") || (tmp[1].pos == "n" && tmp[2].pos == "n"))
                        {
                            index = 1;
                            matched_nn_or_vn = true;
                        }
                    }
                    if (matched_nn_or_vn)
                    {
                        var num = tmp[index + 1].cont;
                        foreach (var x in Devices)
                        {
                            num = num.Replace(x, "");
                        }
                        return tmp[index].cont + num;
                    }
                }

                // 去动词
                tmp = tmp.Where(x => x.pos != "v" && x.cont != "出口").ToList();

                // 两个连续名词去第二个
                if (tmp.Count >= 2 && tmp.Count(x => x.pos == "n") >= 0)
                {
                    var last_n = tmp.Last(x => x.pos == "n");
                    var index = tmp.IndexOf(last_n);
                    if (index - 1 >= 0 && tmp[index - 1].pos == "n")
                        tmp.Remove(last_n);
                }

                var ret = string.Join("", tmp.Select(x => x.cont));
                return string.IsNullOrWhiteSpace(ret) ? "未指定" : ret;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return "未指定";
            }
        }

        private static string GetDevice(IEnumerable<NlpReturn> src)
        {
            try
            {
                var tmp = string.Join("", src.Select(x => x.cont));
                var ret = GetDeviceNameFromDictionary(tmp);
                if (ret != null)
                    return ret;
                var lane = GetLane(src);
                if (lane != "未指定" && src.Any(x => x.arg.Any(y => y.type == "A0")))
                {
                    var a0 = src.SelectMany(x => x.arg).Where(x => x.type == "A0");
                    foreach (var x in a0)
                    {
                        var tmp2 = string.Join("", src.Where(y => y.id >= x.beg && y.id <= x.end).Select(y => y.cont));
                        if (tmp2.Contains(lane))
                        {
                            return tmp2.Replace(lane, "|").Split('|')[1];
                        }
                        return tmp2;
                    }
                }
                return "未知设备";
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return "未知设备";
            }
        }

        private static string GetSolutionByTmp(IEnumerable<NlpReturn> src)
        {
            var tmp = src.Where(x => x.arg.Any(y => y.type == "TMP")).FirstOrDefault();
            if (tmp == null)
                return null;
            var begin = Math.Min(tmp.arg.Min(x => x.beg), tmp.id);
            var end = Math.Max(tmp.arg.Max(x => x.end), tmp.id);
            return string.Join("", src.Where(x => x.id >= begin && x.id <= end).Select(x => x.cont));
        }

        private static string GetPhenomenon(IEnumerable<NlpReturn> src)
        {
            try
            {
                var tmp = src.Where(x => x.arg.Any(y => y.type == "A1") && (x.pos == "v" || x.pos == "p")).LastOrDefault();
                if (tmp == null)
                    return "故障";
                var begin = Math.Min(tmp.arg.Where(x => x.type == "A1").Min(x => x.beg), tmp.id);
                var end = Math.Max(tmp.arg.Where(x => x.type == "A1").Max(x => x.end), tmp.id);
                if (begin - 1 > 0 && src.ElementAt(begin - 1).sem.Any(x => x.relate == "mNeg"))
                    begin = begin - 1;
                return string.Join("", src.Where(x => x.id >= begin && x.id <= end).Select(x => x.cont));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return "故障";
            }
        }

        private static string GetSolutionBydContOrmMod(IEnumerable<NlpReturn> src, bool Unsolved = false)
        {
            var tmp = src.Where(x => x.pos == "v" && (x.sem.Any(y => y.relate  == "mMod") || x.sem.Any(y => y.relate == "dCont"))).LastOrDefault();
            if (tmp == null)
                return null;
            var begin = tmp.id;
            var noun = src.FirstOrDefault(x => x.id > tmp.id && x.pos == "n" && x.sem.Any(y => y.relate == "Pat"));
            if (noun == null)
                return null;
            var end = noun.id;
            // 查找并列关系
            if (src.Any(x => x.sem.Any(y => y.relate == "eSelt" && y.parent == end)))
            {
                var end_noun = src.Last(x => x.sem.Any(y => y.relate == "eSelt" && y.parent == end));
                end = end_noun.id;
            }
            return string.Join("", src.Where(x => x.id >= begin && x.id <= end).Select(x => x.cont));
        }

        public static string GetSolution(IEnumerable<NlpReturn> src, bool Unsolved = false)
        {
            try
            {
                var tmp = GetSolutionByTmp(src);
                if (string.IsNullOrEmpty(tmp))
                    tmp = GetSolutionBydContOrmMod(src);
                if (Unsolved)
                    return string.IsNullOrEmpty(tmp) ? "未解决" : tmp;
                else
                    return string.IsNullOrEmpty(tmp) ? "已解决" : tmp;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                if (Unsolved)
                    return "未解决";
                else
                    return "已解决";
            }
        }
        
        private static string GetDeviceNameFromDictionary(string src)
        {
            foreach(var x in Devices)
            {
                if (src.Contains(x))
                {
                    if (x == "图像" || x == "镜头" || x == "摄像")
                        return "摄像机";
                    else if (x == "费额显示器")
                        return "费显";
                    else if (x == "打印机")
                        return "票据打印机";
                    else
                        return x;
                }
            }
            return null;
        }

        private static async Task<List<List<NlpReturn>>> CallApiUntilSucceeded(string src)
        {
            using (var client = new HttpClient() { BaseAddress = new Uri(Startup.Config["Ltp:Host"]) })
            {
                HttpResponseMessage result = null;
                for (var i = 0; i < 100000; i++)
                {
                    result = await client.GetAsync($"/analysis/?api_key={ Startup.Config["Ltp:Key"] }&text={ WebUtility.UrlEncode(src) }&pattern=all&format=json");
                    if (!result.IsSuccessStatusCode)
                    {
                        await Task.Delay(400);
                    }
                    else
                    {
                        break;
                    }
                }
                var json = await result.Content.ReadAsStringAsync();
                try
                {
                    var obj = JsonConvert.DeserializeObject<List<List<List<NlpReturn>>>>(json).First();
                    return obj;
                }
                catch
                {
                    if (json.IndexOf("请开启JavaScript并刷新该页.") >= 0)
                        return await CallApiUntilSucceeded(src);
                    throw;
                }
            }
        }

        public static async Task<IEnumerable<DeviceIssue>> AnalyzeSolved(IEnumerable<string> src, int total, IHubContext<TgmHub> hub, Guid id)
        {
            var ret = new List<DeviceIssue>();
            var current = 1;
            foreach (var x in src)
            {
                if (string.IsNullOrWhiteSpace(x))
                    continue;
                var obj1 = await CallApiUntilSucceeded(x);
                var lane = GetLane(obj1.First());
                // 去歧义后分析
                var obj2 = obj1;
                if (obj1.Count == 1)
                    obj2  = await CallApiUntilSucceeded(x.Replace(lane, ""));
                for (var i = 0; i < obj2.Count(); i++)
                {
                    var y = obj2[i];
                    var s = new DeviceIssue
                    {
                        Lane = lane,
                        Name = obj1.Count == obj2.Count ? GetDevice(obj1[i]) : GetDevice(obj2[i]),
                        IsSolved = true,
                        Solution = GetSolution(y),
                        Phenomenon = GetPhenomenon(y),
                        Raw = x
                    };
                    s.Phenomenon = PatchPhenomenon(s, string.Join("", y.Select(z => z.cont)));
                    ret.Add(s);
                }
                hub.Clients.All.OnAnalyzeProcessChanged(id, (float)current++ / total);
            }
            return ret;
        }

        public static async Task<IEnumerable<DeviceIssue>> AnalyzeUnsolved(IEnumerable<string> src, int total, IHubContext<TgmHub> hub, Guid id, int existed)
        {
            var ret = new List<DeviceIssue>();
            var current = existed + 1;
            foreach (var x in src)
            {
                if (string.IsNullOrWhiteSpace(x))
                    continue;
                var obj1 = await CallApiUntilSucceeded(x);
                var lane = GetLane(obj1.First());
                // 去歧义后分析
                var obj2 = obj1;
                if (obj1.Count == 1)
                    obj2 = await CallApiUntilSucceeded(x.Replace(lane, ""));

                for (var i = 0; i < obj2.Count(); i++)
                {
                    var y = obj2[i];
                    var s = new DeviceIssue
                    {
                        Lane = lane,
                        Name = obj1.Count == obj2.Count ? GetDevice(obj1[i]) : GetDevice(obj2[i]),
                        IsSolved = false,
                        Solution = GetSolution(y, true),
                        Phenomenon = GetPhenomenon(y),
                        Raw = x
                    };
                    s.Phenomenon = PatchPhenomenon(s, string.Join("", y.Select(z => z.cont)));
                    ret.Add(s);
                }
                
                hub.Clients.All.OnAnalyzeProcessChanged(id, (float)current++ / total);
            }
            return ret;
        }

        private static string Punctuation = "，。；、！“”？（）";

        public static string PatchPhenomenon(DeviceIssue src, string current)
        {
            if (src.Phenomenon != "故障")
                return src.Phenomenon;
            var ret = src.Raw.Replace(src.Lane, "").Replace(src.Solution, "");
            if (ret.IndexOf(src.Name) >= 0)
                ret = ret.Substring(ret.IndexOf(src.Name) + src.Name.Length);
            // 去标点
            foreach (var p in Punctuation)
                ret = ret.Trim(p);
            for(var i = 0; i < ret.Length; i++)
            {
                if (Punctuation.Contains(ret[i]))
                {
                    ret = ret.Substring(0, i);
                    break; 
                }
            }
            if (!string.IsNullOrEmpty(ret))
                return ret;
            else
                return src.Phenomenon;
        }

        public static async Task<IEnumerable<DeviceIssue>> Analyze(IEnumerable<string> solved, IEnumerable<string> unsolved, IHubContext<TgmHub> hub, Guid id)
        {
            var total = solved.Count() + unsolved.Count();
            var ret = await AnalyzeSolved(solved, total, hub, id);
            ret = ret.Concat(await AnalyzeUnsolved(unsolved, total, hub, id, solved.Count())).ToList();
            return ret;
        }
    }
}
