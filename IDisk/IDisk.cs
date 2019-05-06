using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileManager
{
    using BaiduBce;
    using Chromium;
    using Chromium.Remote;
    using CloudManager.config;
    using CloudManager.control;
    using global::IDisk;
    using global::IDisk.control;
    using global::IDisk.service;
    using NetDimension.NanUI;
    using Newtonsoft.Json;
    using System.Data.SQLite;

    public partial class IDisk : WinFormium
	{
        private   CommonCloudFileService CommonCloudFileService = new CommonCloudFileService();
        private   BosConfigService BosConfigService = new BosConfigService();
        private   DownloadRecordService DownloadRecordService = new DownloadRecordService();

        public IDisk() : base("http://res.app.local/asserts/index.html")
		{

            InitializeComponent();

            Constant.Win = this;

            LoadHandler.OnLoadStart += LoadHandler_OnLoadStart;
            LoadHandler.OnLoadEnd += LoadHandler_OnLoadEnd;

            //register the "my" object
            var myObject = GlobalObject.AddObject("my");

            BosConfigControl bosConfigControl = new BosConfigControl();
            bosConfigControl.addFunction(myObject);

            CloudFileControl cloudFileControl = new CloudFileControl();
            cloudFileControl.addFunction(myObject);

            DownloadRecordControl downloadRecordControl = new DownloadRecordControl();
            downloadRecordControl.addFunction(myObject);

            ConstantControl.addFunction(myObject);
        }

        private void LoadHandler_OnLoadEnd(object sender, Chromium.Event.CfxOnLoadEndEventArgs e)
        {

#if DEBUG
            Chromium.ShowDevTools();
            #endif  
            ExecuteJavascript("initAllFile()");
        }

        private void LoadHandler_OnLoadStart(object sender, Chromium.Event.CfxOnLoadStartEventArgs e)
		{
            //初始化数据库
            Db.InitDB();
            //初始化表
            BosConfigService.CreateBOSConfigTable();
            CommonCloudFileService.CreateCloudFileTable();
            DownloadRecordService.CreateDownloadRecordTable();

            //初始化配置信息
            BaiduBOSAPI.SetBosConfig(BosConfigService.Get(" Type=0"));
            TencentBOSAPI.BosConfig = BosConfigService.Get(" Type=1");
            //继续下载未下载完成的文件

            DownloadRecordService.StartBaiduDownloading();
            DownloadRecordService.StartTencentDownloading();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
          
        }
    }
}
