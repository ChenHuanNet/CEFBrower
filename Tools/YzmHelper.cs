using Newtonsoft.Json;
using SimulatorEC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    public class YzmHelper
    {

        public YzmHelper()
        {

        }

        /// <summary>
        /// 获取验证码  showapi解码
        /// </summary>
        /// <param name="userName">优云账号</param>
        /// <param name="password">优云密码</param>
        /// <param name="targetVcLen">验证码长度</param>
        /// <returns>验证码</returns>
        public string MobileValidate(string validCodeUrl)
        {
            string result = null;
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    var vcBytes = GetBytesAsync(validCodeUrl).Result;
                    System.Text.StringBuilder bytesStr = new System.Text.StringBuilder();
                    foreach (var bt in vcBytes)
                    {
                        bytesStr.Append(Convert.ToInt16(bt).ToString());
                        bytesStr.Append(";");
                    }
                    //log.Info($"[SimulatorEC2] [{comId}] MobileValidate vcBytes:{bytesStr.ToString()}");

                    //for (int i = 0; i < 3; i++)
                    //{
                    //    result = captchaHelper.PostAsync(vcBytes).Result;
                    //    if (!string.IsNullOrWhiteSpace(result))
                    //    {
                    //        break;
                    //    }

                    //    if (i == 2 && string.IsNullOrWhiteSpace(result))
                    //    {
                    //        log.Error("获取验证码失败", name, "MobileValidate");
                    //    }
                    //}
                    string res = new ShowApiRequest("http://route.showapi.com/184-5", "123123", "xxxx")
                    .addTextPara("img_base64", Convert.ToBase64String(vcBytes))
                    .addTextPara("typeId", "34")
                    .addTextPara("convert_to_jpg", "0")
                    .addTextPara("needMorePrecise", "0")
                    .post();
                    var ret = JsonConvert.DeserializeObject<TransCodeResult>(res);
                    if (ret.showapi_res_code == 0 && ret.showapi_res_body.ret_code == 0)
                    {
                        result = ret.showapi_res_body.Result;
                    }
                    if (!string.IsNullOrWhiteSpace(result))
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {

                }
            }
            return result;
        }

        /// <summary>
        /// 获取验证码  惊叫解码
        /// </summary>
        /// <returns>验证码</returns>
        public string MobileValidate3(string validCodeUrl)
        {
            string result = string.Empty;
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    var vcBytes = GetBytesAsync(validCodeUrl).Result;
                    System.Text.StringBuilder bytesStr = new System.Text.StringBuilder();
                    foreach (var bt in vcBytes)
                    {
                        bytesStr.Append(Convert.ToInt16(bt).ToString());
                        bytesStr.Append(";");
                    }
                    //一堆16进制打印意义不大注释掉
                    //log.Info($"[SimulatorEC] [{comId}] MobileValidate3 vcBytes:{bytesStr.ToString()}");

                    //提交服务器
                    JingJiaoHttp jingJiaoHttp = new JingJiaoHttp();
                    result = jingJiaoHttp.Post("http://apigateway.jianjiaoshuju.com/api/v_1/yzm.html", vcBytes);

                    if (!string.IsNullOrWhiteSpace(result))
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {

                }
            }


            return result;
        }


        /// <summary>
        /// 获取验证码  惊叫解码
        /// </summary>
        /// <returns>验证码</returns>
        public string MobileValidate3(byte[] vcBytes)
        {
            string result = string.Empty;
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    System.Text.StringBuilder bytesStr = new System.Text.StringBuilder();
                    foreach (var bt in vcBytes)
                    {
                        bytesStr.Append(Convert.ToInt16(bt).ToString());
                        bytesStr.Append(";");
                    }
                    //一堆16进制打印意义不大注释掉
                    //log.Info($"[SimulatorEC] [{comId}] MobileValidate3 vcBytes:{bytesStr.ToString()}");

                    //提交服务器
                    JingJiaoHttp jingJiaoHttp = new JingJiaoHttp();
                    result = jingJiaoHttp.Post("http://apigateway.jianjiaoshuju.com/api/v_1/yzm.html", vcBytes);

                    if (!string.IsNullOrWhiteSpace(result))
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {

                }
            }


            return result;
        }


        async Task<byte[]> GetBytesAsync(string url)
        {
            CookieContainer cookies = new CookieContainer();
            HttpClientHandler handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate };
            handler.AllowAutoRedirect = true;
            handler.UseCookies = true;
            handler.CookieContainer = cookies;
            using (HttpClient httpClient = new HttpClient(handler))
            {
                httpClient.DefaultRequestHeaders.Connection.Add("keep-alive");
                httpClient.Timeout = new TimeSpan(0, 0, 30);

                string userAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36";
                httpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);

                return await httpClient.GetByteArrayAsync(url).ConfigureAwait(false);
            }
        }
    }
}
