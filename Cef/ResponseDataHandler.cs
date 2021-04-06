using CefSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    public class ResponseDataHandler : IResponseFilter
    {
        public event Action<string, byte[]> NotifyData;
        private int contentLength = 0;
        private List<byte> dataAll = new List<byte>();
        private string url;
        public ResponseDataHandler(string url)
        {
            this.url = url;
        }

        public void SetContentLength(int contentLength)
        {
            this.contentLength = contentLength;
        }

        public FilterStatus Filter(System.IO.Stream dataIn, out long dataInRead, System.IO.Stream dataOut, out long dataOutWritten)
        {
            try
            {
                if (dataIn == null)
                {
                    dataInRead = 0;
                    dataOutWritten = 0;

                    return FilterStatus.Done;
                }

                dataInRead = dataIn.Length;
                dataOutWritten = Math.Min(dataInRead, dataOut.Length);

                dataIn.CopyTo(dataOut);
                dataIn.Seek(0, SeekOrigin.Begin);
                byte[] bs = new byte[dataIn.Length];
                dataIn.Read(bs, 0, bs.Length);
                dataAll.AddRange(bs);

                if (dataAll.Count == this.contentLength || this.contentLength == 0)
                {
                    // 通过这里进行通知  
                    NotifyData(url, dataAll.ToArray());
                    return FilterStatus.Done;
                }
                else if (dataAll.Count < this.contentLength)
                {
                    dataInRead = dataIn.Length;
                    dataOutWritten = dataIn.Length;

                    return FilterStatus.NeedMoreData;
                }
                else
                {
                    return FilterStatus.Error;
                }
            }
            catch (Exception ex)
            {
                dataInRead = dataIn.Length;
                dataOutWritten = dataIn.Length;

                return FilterStatus.Done;
            }
        }

        public bool InitFilter()
        {
            return true;
        }

        void IDisposable.Dispose()
        {
            dataAll = null;
        }
    }
}
