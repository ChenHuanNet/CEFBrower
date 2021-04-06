using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SimulatorEC
{
    public class JingJiaoHttp
    {

        public JingJiaoHttp()
        {

        }

        public string Post(string url, byte[] fileByte)
        {
            string ret = string.Empty;
            try
            {
                Dictionary<string, string> headers = new Dictionary<string, string>();
                headers.Add("appCode", "xxxx");
                headers.Add("appKey", "xxx");
                headers.Add("appSecret", "xxxx");

                var body = string.Format("v_pic={0}&v_type=ne5", Convert.ToBase64String(fileByte));
                string res = PostUrl(url, body, headers);
           
                if (!string.IsNullOrWhiteSpace(res))
                {
                    var result = Newtonsoft.Json.JsonConvert.DeserializeObject<JjItem>(res);
                    if (result != null && result.errCode.Equals(0))
                    {
                        ret = result.v_code;
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return ret;
        }


        /// <summary> 
        /// POST请求Url，发送数据
        /// </summary> 
        string PostUrl(string url, string postData, Dictionary<string, string> headers, string Referer = "")
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(postData);

                // 设置参数 
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                CookieContainer cookieContainer = new CookieContainer();
                request.CookieContainer = cookieContainer;
                request.AllowAutoRedirect = true;
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;

                if (headers != null)
                {
                    foreach (var item in headers)
                    {
                        request.Headers.Add(item.Key, item.Value);
                    }
                }


                if (!string.IsNullOrEmpty(Referer))
                {
                    request.Referer = Referer;
                }
                Stream outstream = request.GetRequestStream();
                outstream.Write(data, 0, data.Length);
                outstream.Close();

                //发送请求并获取相应回应数据 
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                //直到request.GetResponse()程序才开始向目标网页发送Post请求 
                Stream instream = response.GetResponseStream();
                StreamReader sr = new StreamReader(instream, Encoding.UTF8);
                //返回结果网页（html）代码 
                string content = sr.ReadToEnd();
                return content;

            }
            catch (Exception ex)
            {

                return string.Empty;
            }
        }

    }
}
