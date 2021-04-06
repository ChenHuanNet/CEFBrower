using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulatorEC
{
    public class TransCodeResult
    {
        /// <summary>
        /// 错误信息的展示
        /// </summary>
        public string showapi_res_error { get; set; }
        /// <summary>
        /// 本次请求id
        /// </summary>
        public string showapi_res_id { get; set; }
        /// <summary>
        /// 易源返回标志,0为成功，其他为失败 0成功-1，系统调用错误-2，可调用次数或金额为0
        /// </summary>
        public int showapi_res_code { get; set; }
        /// <summary>
        /// 消息体的JSON封装，所有应用级的返回参数将嵌入此对象
        /// </summary>
        public TransCode showapi_res_body { get; set; }

    }

    public class TransCode
    {
        /// <summary>
        ///  本次请求id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 识别出来的字符
        /// </summary>
        public string Result { get; set; }
        /// <summary>
        /// 业务成功标示，为0标示成功并扣费
        /// </summary>
        public int ret_code { get; set; }
    }
}
