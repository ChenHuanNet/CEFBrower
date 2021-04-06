using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    /// <summary>
    /// 所有的同步肯定都一样的
    /// </summary>
    public class ReqKey
    {
        static List<string> apilist = new List<string>();
        static ReqKey()
        {
            apilist.Add(validateKey);
            apilist.Add(tasklistApi);
            apilist.Add(monthDataExportApi);
            apilist.Add(cardInfoExportApi);
        }

        #region page关键字 

        public const string loginUrlKey = "login";
        public const string indexUrlKey = "index";
        public const string validateKey = "jcaptcha.jpg";
        #endregion

        #region page

        public const string loginPage = "https://xxxx";

        /// <summary>
        /// 里面的主页面
        /// </summary>
        public const string indexPage = "https://xxxx";


        public const string taskPage = "https://xxxx";
        /// <summary>
        ///  
        /// </summary>
        public const string analysisReportPage = "https://xxxx";
        /// <summary>
        /// 
        /// </summary>
        public const string cardInfoPage = "https://xxxx";
        #endregion

        #region api 

        /// <summary>
        /// 任务列表API
        /// </summary>
        public const string tasklistApi = "/task/list";
        /// <summary>
        /// 导出API地址
        /// </summary>
        public const string monthDataExportApi = "/api/aaaa/xxxxxx"; 

        /// <summary>
        /// 导出API地址
        /// </summary>
        public const string cardInfoExportApi = "/api/bbbbb/aaaa/xxxxx";
        #endregion

        #region 任务类型
        public const string monthFlowTask = "a1";
        public const string cardInfoTask = "a2";
        #endregion

        #region 帮助方法

        public static string IsMyCatchUrl(string url)
        {
            foreach (var item in apilist)
            {
                if (url.Contains(item))
                {
                    return item;
                }
            }
            return null;
        }

        #endregion
    }
}
