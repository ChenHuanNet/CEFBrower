using CefSharp;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    /// <summary>
    /// 在自己窗口打开链接
    /// </summary>
    public class OpenPageSelf : ILifeSpanHandler
    {
        public bool DoClose(IWebBrowser browserControl, IBrowser browser)
        {
            return false;
        }

        public void OnAfterCreated(IWebBrowser browserControl, IBrowser browser)
        {

        }

        public void OnBeforeClose(IWebBrowser browserControl, IBrowser browser)
        {

        }

        /// <summary>
        /// 这个是跳转前 
        /// </summary>
        /// <param name="browserControl"></param>
        /// <param name="browser"></param>
        /// <param name="frame"></param>
        /// <param name="targetUrl"></param>
        /// <param name="targetFrameName"></param>
        /// <param name="targetDisposition"></param>
        /// <param name="userGesture"></param>
        /// <param name="popupFeatures"></param>
        /// <param name="windowInfo"></param>
        /// <param name="browserSettings"></param>
        /// <param name="noJavascriptAccess"></param>
        /// <param name="newBrowser"></param>
        /// <returns></returns>
        public bool OnBeforePopup(IWebBrowser browserControl, IBrowser browser, IFrame frame, string targetUrl,
string targetFrameName, WindowOpenDisposition targetDisposition, bool userGesture, IPopupFeatures popupFeatures,
IWindowInfo windowInfo, IBrowserSettings browserSettings, ref bool noJavascriptAccess, out IWebBrowser newBrowser)
        {
            newBrowser = null;
            var chromiumWebBrowser = (ChromiumWebBrowser)browserControl;
            chromiumWebBrowser.Load(targetUrl);
            return true; //Return true to cancel the popup creation copyright by codebye.com.
        }
    }
}
