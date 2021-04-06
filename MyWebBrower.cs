using CefSharp;
using CefSharp.WinForms;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.ServiceModel.Configuration;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;


namespace WindowsFormsApp1
{
    public partial class MyWebBrower : Form
    {

        string url;
        string username;
        string pwd;
        string dataType = "D";
        YzmHelper yzmHelper = new YzmHelper();
        public ChromiumWebBrowser browser;
        RequestHandler requestHandler;
        bool islogin = false;
        Dictionary<string, bool> pages = new Dictionary<string, bool>();
        Dictionary<string, int> reapter = new Dictionary<string, int>();

        Logger log;
        ConcurrentDictionary<DateTime, string> days;
        int dIndex = 0;
        /// <summary>
        /// 静态任务创建重试次数
        /// </summary>
        int sCreateTaskRepeater = 0;
        List<DateTime> dDateTask = new List<DateTime>();
        /// <summary>
        /// 下载完后是否执行后续逻辑
        /// </summary>
        bool isTest = false;
        /// <summary>
        /// 是否连续执行，还是执行单个
        /// </summary>
        bool isSingle = false;

        public delegate void NotifyFinish(string comId, MyWebBrower form, bool isTest);
        NotifyFinish notifyevent;
        /// <summary>
        /// 是否开始下载
        /// </summary>
        Dictionary<string, int> isDown = new Dictionary<string, int>();
        /// <summary>
        /// 下载事件
        /// </summary>
        public DownloadHandler downloadHandler;

        DateTime opeTime;
        System.Timers.Timer timer;


        int stopTimes = 3;

        public MyWebBrower(Logger log, NotifyFinish notifyevent, bool isSingle, bool isTest)
        {
            InitializeComponent();
            this.log = log;
            this.notifyevent = notifyevent;
            this.isTest = isTest;
            this.isSingle = isSingle;
            pages = new Dictionary<string, bool>();
            reapter = new Dictionary<string, int>();
            dDateTask = new List<DateTime>();


            dDateTask = days.Keys.ToList();

            //v2版本的月数据，都是最新的。所以静态数据不需要缓存
            InitBrowser();

            opeTime = DateTime.Now;
            timer = new System.Timers.Timer(1000 * 60 * 6);
            timer.Elapsed += new System.Timers.ElapsedEventHandler(IsStop);
            timer.Start();
        }

        void IsStop(object o, System.Timers.ElapsedEventArgs e)
        {
            log.Trace($"6分钟检测一次流程是否卡顿,stopTimes=" + stopTimes);
            if (stopTimes <= 0)
            {
                PreLoginOut();
                timer.Stop();
                timer.Dispose();
                return;
            }
            stopTimes--;
            if (Math.Abs((DateTime.Now - opeTime).TotalSeconds) > 60 * 5)
            {

                Task.Run(() =>
                {
                    if (this.url.IndexOf(ReqKey.loginUrlKey) >= 0)
                    {
                        log.Trace($"流程卡主了。可能没登录进去，重新登录一次,{this.username},{this.pwd}");
                        islogin = false;
                        waitLoginResult = false;
                        GotoPage(ReqKey.loginPage);
                    }
                    else
                    {
                        log.Trace($"流程卡主了。跳转任务列表，继续任务");
                        GotoPage(ReqKey.taskPage);
                    }

                });
            }
        }


        public void InitBrowser()
        {

            browser = new ChromiumWebBrowser(ReqKey.loginPage);
            this.Controls.Add(browser);
            browser.Dock = DockStyle.Fill;

            browser.FrameLoadStart += new EventHandler<FrameLoadStartEventArgs>(FrameLoadStartEvent);
            browser.FrameLoadEnd += new EventHandler<FrameLoadEndEventArgs>(FrameLoadEndEvent);
            downloadHandler = new DownloadHandler(null, DownloadResult);
            browser.DownloadHandler = downloadHandler;
            requestHandler = new RequestHandler(this.username);
            browser.RequestHandler = requestHandler;
            // browser.LifeSpanHandler = new OpenPageSelf();
        }


        void FrameLoadStartEvent(object o, FrameLoadStartEventArgs e)
        {
            opeTime = DateTime.Now;
            try
            {
                this.url = e.Url;

                #region 第一次进主页面，直接进入任务列表
                if (this.url.IndexOf(ReqKey.indexUrlKey) > 0 && IsIn(ReqKey.indexUrlKey))
                {
                    islogin = true;
                    SetPagesTimes(ReqKey.indexUrlKey);
                    Task.Run(() =>
                    {
                        Thread.Sleep(5000);
                        GotoPage(ReqKey.taskPage);
                    });
                }
                #endregion

                #region 任务任务列表
                else if (this.url.IndexOf(ReqKey.taskPage) >= 0 && IsIn(ReqKey.taskPage))
                {
                    SetPagesTimes(ReqKey.taskPage, false);
                    Task.Run(() =>
                    {
                        Thread.Sleep(15000);
                        //先判断今天是否有已经建立的任务
                        if (IsLoadFinish(ReqKey.tasklistApi))
                        {
                            //业务逻辑
                        }
                    });

                }
                #endregion

                //...........
            }
            catch (Exception ex)
            {
                log.Error($"页面加载之前报错 ex:{ex}");
                PreLoginOut();
            }
        }

        bool waitLoginResult = false;


        #region  页面加载之后的事件
        void FrameLoadEndEvent(object o, FrameLoadEndEventArgs e)
        {
            opeTime = DateTime.Now;
            try
            {

                #region  登录流程
                if (this.url.IndexOf(ReqKey.loginUrlKey) > 0)
                {
                    Thread.Sleep(2000);
                    if (islogin && !waitLoginResult)
                    {
                        string js1 = " document.querySelector('.home-plat-choose').children[0].children[1].click() ";
                        browser.GetBrowser().MainFrame.EvaluateScriptAsync(js1);
                        return;
                    }
                    islogin = false;

                    //string js = $"let evt = document.createEvent('HTMLEvents');evt.initEvent('input', true, true); document.querySelectorAll('.el-input__inner')[0].value = '{this.username}'; document.querySelectorAll('.el-input__inner')[0].dispatchEvent(evt); ";

                    string js = $"let evt = document.createEvent('HTMLEvents');evt.initEvent('input', true, true); document.getElementById('userId').value = '{this.username}'; document.getElementById('userId').dispatchEvent(evt); ";

                    browser.GetBrowser().MainFrame.EvaluateScriptAsync(js);

                    //js = $"let evt1 = document.createEvent('HTMLEvents');evt1.initEvent('input', true, true); document.querySelectorAll('.el-input__inner')[1].value = '{this.pwd}'; document.querySelectorAll('.el-input__inner')[1].dispatchEvent(evt1); ";

                    js = $"let evt1 = document.createEvent('HTMLEvents');evt1.initEvent('input', true, true); document.getElementById('password').value = '{this.pwd}'; document.getElementById('password').dispatchEvent(evt1); ";
                    browser.GetBrowser().MainFrame.EvaluateScriptAsync(js);


                    //协议
                    //js = $" document.querySelector('.el-checkbox__input').click(); ";
                    //browser.GetBrowser().MainFrame.EvaluateScriptAsync(js);
                    Task.Run(() =>
                    {

                        Thread.Sleep(5000);
                        for (int i = 0; i < 5; i++)
                        {
                            string yzm = yzmHelper.MobileValidate3(requestHandler.responseCaches[ReqKey.validateKey]);

                            //js = $"let evt2 = document.createEvent('HTMLEvents');evt2.initEvent('input', true, true); document.querySelectorAll('.el-input__inner')[2].value = '{yzm}'; document.querySelectorAll('.el-input__inner')[2].dispatchEvent(evt2); ";
                            js = $"let evt2 = document.createEvent('HTMLEvents');evt2.initEvent('input', true, true); document.getElementById('jcaptcha').value = '{yzm}'; document.getElementById('jcaptcha').dispatchEvent(evt2); ";
                            browser.GetBrowser().MainFrame.EvaluateScriptAsync(js);

                            //js = $"document.querySelectorAll('.el-button--primary')[0].click();";
                            js = $"document.getElementById('loginBtn').click();";
                            browser.GetBrowser().MainFrame.EvaluateScriptAsync(js);
                            waitLoginResult = true;
                            try
                            {
                                Thread.Sleep(1000);
                                //js = $" document.querySelectorAll('.error-message')[0].innerText; ";
                                js = $" document.getElementById('info').innerText; ";
                                var error = browser.GetBrowser().MainFrame.EvaluateScriptAsync(js);
                                if (error != null)
                                {
                                    string errmsg = error.ConfigureAwait(false).GetAwaiter().GetResult()?.Result?.ToString();
                                    if (string.IsNullOrWhiteSpace(errmsg))
                                    {
                                        this.islogin = true;
                                        waitLoginResult = false;
                                        break;
                                    }
                                    else
                                    {
                                        log.Error($" 第{i}次登录失败 ,账号:{this.username} ,密码 :{this.pwd} ,失败原因 : {errmsg}");
                                        if (i == 4)
                                        {
                                            //下一个集团 连续登录失败
                                            PreLoginOut();
                                            return;
                                        }
                                    }
                                }
                                else
                                {
                                    this.islogin = true;
                                    waitLoginResult = false;
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                this.islogin = true;
                                waitLoginResult = false;
                                Thread.Sleep(10000);
                                if (!pages.ContainsKey(ReqKey.indexUrlKey))
                                {
                                    islogin = false;
                                    log.Error($"第{i}次登录过程发生异常 ,ex:{ex}");
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    });
                }
                #endregion
            }
            catch (Exception ex)
            {

                log.Error($" 页面加载完成之后报错 ex:{ex}");
            }

        }
        #endregion


        #region 下载结果通知方法
        void DownloadResult(string comId, string filename)
        {
            log.Trace($"{comId},下载文件成功:{filename}");
            opeTime = DateTime.Now;
            if (string.IsNullOrWhiteSpace(filename))
            {
                //下载失败，需要one more time
                SetPagesTimes(ReqKey.indexUrlKey, true);
                browser.Load(ReqKey.indexPage);//首页会自动调整到下载管理
                return;
            }

            //解析文件信息
            bool isParserOk = true;
            if (!isParserOk)
            {
                log.Error($"$解析文件失败filename=[{filename}]");
            }

            string key = "";
            if (isDownOK(key, dataType) == 3 || isDownOK(key, dataType) < 0)
            {
                //防止重复下载 ，执行相同逻辑
                return;
            }
            if (isDownOK(key, "D") != 2)
            {
                //没有100%，但是文件却下完了，说明文件有缺损，重新下载
                setDownStatus(key, -1);
                log.Trace($"$下载文件不完整filename=[{filename}]");
                log.Error($"$下载文件不完整filename=[{filename}]");
                Thread.Sleep(5000);
                return;
            }
            setDownStatus(key, 3);

            Thread.Sleep(1000);

            var res = true;

            //下载完休息30秒再准备跳转
            Thread.Sleep(30000);

            if (this.dataType == "D")
            {
                if (res)
                {
                    //
                }
                //动态已经同步完了，开始静态同步
                if (dIndex >= dDateTask.Count - 1)
                {
                    this.dataType = "S";
                    GotoPage(ReqKey.taskPage);
                    return;
                }
                else
                {
                    //还有动态任务
                    dIndex++;
                    log.Trace($"前往下一个日期的动态任务");
                    GotoPage(ReqKey.taskPage);
                    return;
                }
            }
            else if (this.dataType == "S")
            {
                PreLoginOut();
            }
            else
            {
                PreLoginOut();
            }
        }
        #endregion

        #region 静态动态，失败重试
        /// <summary>
        /// 失败重试次数记录
        /// </summary>
        /// <param name="key"></param>
        void AutoReapterTimes(string key)
        {
            if (reapter.ContainsKey(key))
            {
                reapter[key]++;
            }
            else
            {
                reapter.Add(key, 1);
            }
        }

        void SetReapterTimes(string key, int times)
        {
            if (reapter.ContainsKey(key))
            {
                reapter[key] = times;
            }
            else
            {
                reapter.Add(key, times);
            }
        }

        /// <summary>
        /// 判断是否失败10次，失败>=10次返回true <10次返回false  动态使用
        /// </summary>
        /// <param name="key"></param>
        bool PreGotoNextTask(string key)
        {
            if (reapter.ContainsKey(ReqKey.monthFlowTask) && reapter[ReqKey.monthFlowTask] >= 10)
            {
                dIndex++;
                SetReapterTimes(ReqKey.monthFlowTask, -1);
                return true;
            }
            return false;
        }
        #endregion

        #region 防止页面二次加载
        bool IsIn(string page)
        {
            if (pages.ContainsKey(page))
            {
                return pages[page];
            }
            else
            {
                return true;
            }
        }

        void SetPagesTimes(string page, bool times = false)
        {
            if (pages.ContainsKey(page))
            {
                pages[page] = times;
            }
            else
            {
                pages.Add(page, times);
            }
        }

        void ClearPagesTimes(string page)
        {
            if (pages.ContainsKey(page))
            {
                pages[page] = true;
            }
        }
        #endregion

        #region 资源判断
        /// <summary>
        /// 判断资源是否加载完成
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool IsLoadFinish(string key)
        {
            int i = 10;
            bool res = true;
            while (true)
            {
                bool result = requestHandler.responseCaches.ContainsKey(key);
                if (result)
                {
                    res = true;
                    break;
                }
                log.Error($"[{key}]尚未加载完成");
                i++;
                if (i > 5)
                {
                    res = false;
                    break;
                }
                Thread.Sleep(1000);
            }
            return res;
        }

        /// <summary>
        /// 获取资源
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public byte[] GetAssent(string key)
        {
            bool result = requestHandler.responseCaches.ContainsKey(key);
            if (result)
            {
                return requestHandler.responseCaches[key];
            }
            return null;
        }
        /// <summary>
        /// 清楚历史资源 
        /// </summary>
        /// <param name="key"></param>
        public void ClearAssent(string key)
        {
            bool result = requestHandler.responseCaches.ContainsKey(key);
            if (result)
            {
                requestHandler.responseCaches.Remove(key);
            }
        }
        #endregion

        #region 跳转页面
        void GotoPage(string url)
        {
            SetPagesTimes(url, true);
            Thread.Sleep(500);
            browser.Load(url);
        }
        #endregion

        #region 发起下载是否成功
        /// <summary>
        /// 发起下载是否成功 
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        bool CreateDownload(string msg)
        {
            try
            {
                var error = browser.GetBrowser().MainFrame.EvaluateScriptAsync("(function() { return document.children[0].innerHTML.indexOf('下载失败'); })();");
                if (error != null)
                {
                    string errmsg = error.ConfigureAwait(false).GetAwaiter().GetResult().Result.ToString();
                    if (!string.IsNullOrWhiteSpace(errmsg) && Convert.ToInt32(errmsg) >= 0)
                    {
                        log.Error($"发起[{msg}]下载失败,准备重试");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return true;
        }

        /// <summary>
        /// 判断任务是否下载成功  -1未开始 1 正在下载 2下载成功 3解析完成
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        int isDownOK(string key, string dataType)
        {
            if (!isDown.ContainsKey(key))
            {
                long receivedBytes = downloadHandler.receivedBytes;
                double pecent = downloadHandler.percent;
                if (receivedBytes > 0 && pecent < 1 && pecent >= 0)
                {
                    log.Trace($"_{dataType}_正在下载中:{Math.Round(pecent * 100, 2)}%");
                    return 1;
                }
                else if (receivedBytes > 0 && pecent >= 1)
                {
                    log.Trace($"_{dataType}_下载完成:{Math.Round(pecent * 100, 2)}%");
                    return 2;
                }
                return -1;
            }

            return isDown[key];
        }

        /// <summary>
        /// 赋值下载状态  -1未开始 1 正在下载 2下载成功
        /// </summary>
        void setDownStatus(string key, int status)
        {
            if (!isDown.ContainsKey(key))
            {
                isDown.Add(key, status);
            }
            else
            {
                isDown[key] = status;
            }
        }

        #endregion

        #region 退出登录
        bool ispreloginout = false;
        bool isloginout1 = false;
        bool isloginout2 = false;
        void LoginOut1()
        {
            try
            {
                string js = " document.getElementsByClassName('el-submenu')[3].querySelectorAll('.el-menu-item')[2].click(); ";
                browser.GetBrowser().MainFrame.EvaluateScriptAsync(js);
                isloginout1 = true;
            }
            catch (Exception)
            {

            }
        }

        void LoginOut2()
        {
            try
            {
                string js = " document.getElementsByClassName('home-plat-choose')[0].querySelectorAll('li')[11].click(); ";
                browser.GetBrowser().MainFrame.EvaluateScriptAsync(js);
                isloginout2 = true;
            }
            catch (Exception)
            {

            }

        }
        void PreLoginOut()
        {
            //ispreloginout = true;
            //GotoPage(ReqKey.indexPage);
            try
            {
                Cef.GetGlobalCookieManager().DeleteCookiesAsync("", "").Wait();
            }
            catch (Exception)
            {

            }
            notifyevent.Invoke(null, this, this.isSingle);
        }
        #endregion




        #region 窗体关闭方法
        delegate void WindowOperate(MyWebBrower form);

        public void CloseWindow(MyWebBrower form)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new WindowOperate(CloseWindow), form);
            }
            else
            {
                #region 释放资源
                days = null;
                dDateTask = null;
                pages = null;
                reapter = null;

                if (timer != null)
                {
                    timer.Stop();
                    timer.Dispose();
                }
                browser?.GetBrowser().CloseBrowser(true);
                browser?.Dispose();
                form?.Close();
                form?.Dispose();
                GC.SuppressFinalize(this);
                GC.Collect();
                #endregion
            }
        }

        #endregion




        /// <summary>
        /// 窗体准备关闭的时候触发的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MyWebBrower_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                days = null;
                dDateTask = null;
                pages = null;
                reapter = null;
                browser?.GetBrowser().CloseBrowser(true);
                browser?.Dispose();

                if (timer != null)
                {
                    timer.Stop();
                    timer.Dispose();
                }
            }
            catch (Exception)
            {
            }

        }
    }


}
