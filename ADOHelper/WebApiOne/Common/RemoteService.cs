using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;
using WebApiOne.Common;

namespace Common.Helper
{
    /// <summary>
    /// 调用远程服务
    /// </summary>
    public class RemoteService
    {

        public class MeterData
        {
            public int State { get; set; }

            public int Alarming { get; set; }

        }

        public static string AddressTest => ConstHelper.IsProduct ? "http://112.124.44.131" : "http://139.9.185.50";

        /// <summary>
        /// 获取仪表实时数据
        /// </summary>
        /// <param name="token"></param>
        /// <param name="customerId"></param>
        /// <param name="meterIdList"></param>
        /// <returns></returns>
        public static MeterData GetMeterData(string token,int customerId, string meterIdList)
        {
            var url = AddressTest + ":12700/realdata/query/meterDatas";
            var dic = new Dictionary<string, string>
            {
                { "token", token },
                { "meterIdList", meterIdList },
                { "customerID", customerId.ToString() }
            };

            return new JavaScriptSerializer().Deserialize<MeterData>(Get(url, dic));
        }

        /// <summary>
        /// get请求 通用方法
        /// </summary>
        /// <param name="url"></param>
        /// <param name="dic"></param>
        /// <returns></returns>
        public static string Get(string url, Dictionary<string, string> dic)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(url);
            if (dic!=null && dic.Count>0)
            {
                builder.Append(string.Format("?{0}= {1}", dic.ElementAt(0).Key, dic.ElementAt(0).Value));

                for (int i=1; i<dic.Count; i++)
                {
                    builder.Append("&");
                    builder.AppendFormat("{0}={1}", dic.ElementAt(i).Key, dic.ElementAt(i).Value);
                }           
            }

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(builder.ToString());
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            using (Stream stream = resp.GetResponseStream())
            {
                //获取内容
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// 指定Post地址使用Get 方式获取全部字符串
        /// </summary>
        /// <param name="url">请求后台地址</param>
        /// <param name="dic"></param>
        /// <param name="isJson"></param>
        /// <returns></returns>
        public string Post(string url, Dictionary<string, string> dic, bool isJson=false)
        {
            //初始化请求对象
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
            req.ContentType = isJson ? "application/json" : "application/x-www-form-urlencoded";

            //填入post参数
            StringBuilder builder = new StringBuilder();
            if (dic != null && dic.Count > 0)
            {
                builder.Append(string.Format("?{0}= {1}", dic.ElementAt(0).Key, dic.ElementAt(0).Value));

                for (int i = 1; i < dic.Count; i++)
                {
                    builder.Append("&");
                    builder.AppendFormat("{0}={1}", dic.ElementAt(i).Key, dic.ElementAt(i).Value);
                }
            }
            byte[] data = Encoding.UTF8.GetBytes(builder.ToString());
            req.ContentLength = data.Length;
            using (Stream reqStream = req.GetRequestStream())
            {
                reqStream.Write(data, 0, data.Length);
            }

            //获取响应内容
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            using (Stream stream = resp.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}