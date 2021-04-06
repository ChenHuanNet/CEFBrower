using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApp1.Entity;

namespace WindowsFormsApp1
{
    public class DownloadHandler : IDownloadHandler
    {
        public DateTime SyncDate = DateTime.Now.Date;
        public double percent = 0;
        public long receivedBytes = 0;
        public delegate void NotifyResult(string comId, string filename);
        List<DownLoadDto> isDownFinish = new List<DownLoadDto>();

        string comId;
        NotifyResult notify;
        public DownloadHandler(string comId, NotifyResult notify)
        {
            this.notify = notify;
            this.comId = comId;
        }

        public void OnBeforeDownload(IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback)
        {
            //WebBrowser ie = new WebBrowser();
            //ie.Navigate(downloadItem.Url);
            if (!callback.IsDisposed)
            {
                using (callback)
                {
                    List<DownLoadDto> keys = new List<DownLoadDto>();
                    foreach (var item in isDownFinish)
                    {
                        if (item.Key.Contains(this.comId))
                        {
                            keys.Add(item);
                        }
                    }

                    foreach (DownLoadDto item in keys)
                    {
                        isDownFinish.Remove(item);
                    }

                    receivedBytes = 0;
                    percent = 0;
                    // string path = AppDomain.CurrentDomain.BaseDirectory + string.Format(@"\Downloads\{0}\{1}\", this.comId, dt.ToString("yyyy-MM-dd"));
                    string path = string.Format(@"C:\Users\Administrator\Desktop\Downloads\{0}\{1}\", this.comId, SyncDate.ToString("yyyy-MM-dd"));

                    if (!System.IO.Directory.Exists(path))
                    {
                        System.IO.Directory.CreateDirectory(path);
                    }

                    string dateType = "";
                    string name = downloadItem.SuggestedFileName;
                    string ext = name.Substring(name.LastIndexOf('.'), name.Length - name.LastIndexOf('.'));// .csv
                    if (name.IndexOf("业务月用量趋势报表明细") >= 0 || name.IndexOf("统计分析月累计量明细导出") >= 0 || name.IndexOf("业务月用量明细报表") >= 0)
                    {
                        dateType = "D";
                        if (SyncDate == DateTime.Now.Date)
                            name = "D_" + SyncDate.AddDays(-1).ToString("yyyyMMdd") + ext;//可能数据量很小,直接下载了,SyncDate会=今天
                        else
                            name = "D_" + SyncDate.ToString("yyyyMMdd") + ext;
                    }
                    else if (name.IndexOf("SIM卡信息列表") >= 0)
                    {
                        dateType = "S";
                        name = "S_" + SyncDate.ToString("yyyyMMdd") + ext;
                    }



                    string fileName = path + name;

                    string key = this.comId + "_" + downloadItem.SuggestedFileName;
                    DownLoadDto dto = new DownLoadDto();
                    dto.FileName = fileName;
                    dto.Key = key;
                    dto.Status = 1;
                    if (!isDownFinish.Exists(x => x.Key == key))
                    {
                        isDownFinish.Add(dto);
                    }


                    //路径可能有权限问题  ,暂时只能放在桌面上，不清楚为什么
                    callback.Continue(fileName,
                         false);
                }
            }


        }
        public void OnDownloadUpdated(IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback)
        {
            receivedBytes = downloadItem.ReceivedBytes;
            percent = downloadItem.PercentComplete * 1.0 / 100;
            if (downloadItem.IsComplete)
            {
                //成功了会走这里，失败了不会走这里
                string fullpath = downloadItem.FullPath;
                //成功通知回调事件
                Task.Run(() =>
                {
                    notify.Invoke(comId, fullpath);
                    try
                    {
                        browser?.CloseBrowser(true);
                    }
                    catch (Exception ex)
                    {

                    }
                });
            }
            else if (downloadItem.PercentComplete >= 100)
            {
                string key = this.comId + "_" + downloadItem.SuggestedFileName;
                var tmp = isDownFinish.Find(x => x.Key == key);
                if (tmp != null && tmp.Status == 1)
                {
                    tmp.Status = 2;
                    //成功通知回调事件
                    Task.Run(() =>
                    {
                        notify.Invoke(comId, tmp.FileName);
                        try
                        {
                            browser?.CloseBrowser(true);
                        }
                        catch (Exception ex)
                        {

                        }
                      
                    });
    
                }
            }
            if (downloadItem.IsCancelled)
            {
                //失败走这里
                notify.Invoke(comId, null);
            }

        }
        //public bool OnDownloadUpdated(CefSharp.DownloadItem downloadItem)
        //{
        //    return false;
        //}


    }
}
