
using CefSharp;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace WindowsFormsApp1
{
    /// <summary>
    /// FXFT_ BE2004786917,FXFT_2007537659,FXFT_2008432806,FXFT_591632038993,FXFT_2001005032
    /// </summary>
    public partial class Form1 : Form
    {
        public Logger log;
        List<string> finishComIds = new List<string>();
        int index = 0;

        string times = System.Configuration.ConfigurationManager.AppSettings["times"];
        string isTest = System.Configuration.ConfigurationManager.AppSettings["isTest"];
        public Form1()
        {
            InitializeComponent();
            log = NLog.LogManager.GetLogger("");
            //全局只能一次
            Cef.Initialize(new CefSettings());
        }




        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                SchedulerInit();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void CloseOtherWindows()
        {
            try
            {
                //关闭多余窗体
                int num = Application.OpenForms.Count;
                for (int i = 0; i < num; i++)
                {
                    Form f = Application.OpenForms[i];
                    if (f.Name != "Form1")
                    {
                        CloseOtherWindow(f);
                        num = num - 1;
                        i = i - 1;
                    }

                }
            }
            catch (Exception ex)
            {

            }
        }

        delegate void OtherWindowOperate(Form form);

        public void CloseOtherWindow(Form form)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new OtherWindowOperate(CloseOtherWindow), form);
            }
            else
            {
                #region 释放资源
                form?.Close();
                form?.Dispose();
                #endregion
            }
        }

        /// <summary>
        /// 计划任务初始化
        /// </summary>
        void SchedulerInit()
        {
            string[] timeArr = times.Split(',');
            foreach (var time in timeArr)
            {
                //windows计划任务
            }
        }

        /// <summary>
        /// 清除计划任务
        /// </summary>
        void ClearScheduler()
        {
            string[] timeArr = times.Split(',');
            foreach (var time in timeArr)
            {
                //清除计划任务
            }
        }


        /// <summary>
        /// 同步完一个集团之后的逻辑
        /// </summary>
        /// <param name="comId"></param>
        /// <param name="form"></param>
        /// <param name="isSingle"></param>
        void NotifyFinish(string comId, MyWebBrower form, bool isSingle)
        {
            try
            {
                log.Trace($"---------------------------------");
                log.Error($"---------------------------------");
                NextComId(comId, form, isSingle);

            }
            catch (Exception ex)
            {
                log.Error($"开启下一个集团同步浏览器失败ex :{ex}");
            }
        }

        void NextComId(string comId, MyWebBrower form, bool isSingle)
        {
            Task.Run(() =>
            {
                Thread.Sleep(10000);
                form.CloseWindow(form);
                CloseOtherWindows();

                if (!isSingle)
                {
                    finishComIds.Add(comId);
                    index++;

                    Thread.Sleep(2000);

                    MyWebBrower myWebBrower = new MyWebBrower(log, NotifyFinish, isSingle, isTest.ToLower() == "true");
                    OpenWindow MethInvk = new OpenWindow(OpenWebBrower);
                    BeginInvoke(MethInvk, myWebBrower);

                }
            });
        }

        #region 多线程打开浏览器
        delegate void OpenWindow(Form webBrower);

        void OpenWebBrower(Form webBrower)
        {
            webBrower.Show();
        }
        #endregion

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            ClearScheduler();
        }

    }
}
