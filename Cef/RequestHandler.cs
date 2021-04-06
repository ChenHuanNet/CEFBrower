using CefSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    public class RequestHandler : IRequestHandler
    {
        string userName;
        public RequestHandler(string userName)
        {
            this.userName = userName;
        }

        public Dictionary<string, byte[]> responseCaches = new Dictionary<string, byte[]>();

        private ResponseDataHandler filter = null;
        //public event Action<byte[]> NotifyData;

        public bool GetAuthCredentials(IWebBrowser browserControl, IBrowser browser, IFrame frame, bool isProxy,
            string host, int port, string realm, string scheme, IAuthCallback callback)
        {

            return false;
        }

        public IResponseFilter GetResourceResponseFilter(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response)
        {
            var url = new Uri(request.Url);
            string key = ReqKey.IsMyCatchUrl(url.AbsoluteUri);
            if (key != null)
            {
                this.filter = new ResponseDataHandler(key);
                filter.NotifyData += filter_NotifyData;
                return filter;
            }

            return null;
        }

        void filter_NotifyData(string url, byte[] data)
        {
            if (responseCaches.ContainsKey(url))
            {
                responseCaches[url] = data;
            }
            else
            {
                responseCaches.Add(url, data);
            }

            //try
            //{
            //    if (responseCaches.ContainsKey(url))
            //    {
            //        responseCaches[url] = data;
            //    }
            //    else
            //    {
            //        responseCaches.Add(url, data);
            //    }

            //    if (url.Equals(ReqKey.validateKey))
            //    {
            //        string NewImageName = "jcaptcha.jpg";//ImageName(CenterId);//获得图片的名字
            //        string ImagePath = AppDomain.CurrentDomain.BaseDirectory + $"/Downloads/Images/{this.userName}/";
            //        if (!Directory.Exists(ImagePath))
            //        {
            //            Directory.CreateDirectory(ImagePath);
            //        }
            //        ImagePath += NewImageName;
            //        if (!File.Exists(ImagePath))
            //        {
            //            File.Delete(ImagePath);
            //        }

            //        MemoryStream ms = new MemoryStream(data);
            //        FileStream fs = new FileStream(ImagePath, FileMode.OpenOrCreate);
            //        ms.WriteTo(fs);

            //        ms.Close();
            //        ms.Dispose();

            //        fs.Close();
            //        fs.Dispose();
            //        ;
            //    }
            //    else
            //    {
            //        // string json = Encoding.UTF8.GetString(data);
            //    }
            //}
            //catch (Exception ex)
            //{

            //    throw ex;
            //}

        }

        public bool OnBeforeBrowse(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request,
            bool isRedirect)
        {
            return false;
        }

        public CefReturnValue OnBeforeResourceLoad(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
        {
            return CefReturnValue.Continue;
        }

        public bool OnCertificateError(IWebBrowser browserControl, IBrowser browser, CefErrorCode errorCode, string requestUrl, ISslInfo sslInfo, IRequestCallback callback)
        {
            return true;
        }

        public bool OnOpenUrlFromTab(IWebBrowser browserControl, IBrowser browser, IFrame frame, string targetUrl, WindowOpenDisposition targetDisposition, bool userGesture)
        {
            return false;
        }

        public void OnPluginCrashed(IWebBrowser browserControl, IBrowser browser, string pluginPath)
        {

        }

        public bool OnProtocolExecution(IWebBrowser browserControl, IBrowser browser, string url)
        {
            return false;
        }

        public bool OnQuotaRequest(IWebBrowser browserControl, IBrowser browser, string originUrl, long newSize, IRequestCallback callback)
        {
            return false;
        }

        public void OnRenderProcessTerminated(IWebBrowser browserControl, IBrowser browser, CefTerminationStatus status)
        {

        }

        public void OnRenderViewReady(IWebBrowser browserControl, IBrowser browser)
        {

        }

        public void OnResourceLoadComplete(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response, UrlRequestStatus status, long receivedContentLength)
        {
            if (request.Url.Contains("/task/list"))
            {
                if (receivedContentLength > 0)
                {

                }
            }

        }

        public void OnResourceRedirect(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, ref string newUrl)
        {

        }

        public void OnResourceRedirect(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response, ref string newUrl)
        {

        }

        public bool OnResourceResponse(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response)
        {
            //NOTE: You cannot modify the response, only the request  
            // You can now access the headers  
            //var headers = response.ResponseHeaders;  
            try
            {
                var content_length = int.Parse(response.ResponseHeaders["Content-Length"]);
                if (this.filter != null)
                {
                    this.filter.SetContentLength(content_length);
                }
            }
            catch { }
            return false;
        }

    }
}
