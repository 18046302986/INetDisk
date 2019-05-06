using CloudManager.config;
using CloudManager.entity;
using IDisk;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


public class TencentDownloadThreadPool
{

    public static TencentDownloadThreadPool Me = new TencentDownloadThreadPool();

    private static readonly Mutex Mutex = new Mutex();
    /// <summary>
    /// 每次下载的大小
    /// </summary>
    private static long DownloadSize = 1024 * 1024 * 1;

    private DownloadRecordService DownloadRecordService = new DownloadRecordService();

    private TencentDownloadThreadPool()
    {

    }

    public Hashtable DownloadStates = new Hashtable();
    public Hashtable DownloadRecordMap = new Hashtable();

    /// <summary>
    /// 暂停
    /// </summary>
    /// <param name="key"></param>
    public void PauseDownload(string key)
    {
        Mutex.WaitOne();
        DownloadStates.Remove(key);
        DownloadRecordService.UpdateDowloadState(Convert.ToInt32(key), 1);

        Mutex.ReleaseMutex();
    }

    /// <summary>
    /// 取消下载
    /// </summary>
    /// <param name="key"></param>
    public void CancelDowload(string key)
    {
        DownloadStates.Remove(key);
        DownloadRecordMap.Remove(key);
    }
    /// <summary>
    /// 开始下载
    /// </summary>
    /// <param name="key"></param>
    /// <param name="downloadInfo"></param>
    public void StartDownload(DownloadRecord downloadInfo)
    {
        if (DownloadStates[GetDownloadStateKey(downloadInfo)] == null)
        {
            DownloadStates.Add(GetDownloadStateKey(downloadInfo), true);
        }
        if (DownloadRecordMap[GetDownloadStateKey(downloadInfo)] == null)
        {
            DownloadRecordMap.Add(GetDownloadStateKey(downloadInfo), downloadInfo);
        }
        ThreadStart threadStart = new ThreadStart(Breakpointdownload);//通过ThreadStart委托告诉子线程执行什么方法　　
        Thread thread = new Thread(threadStart);
        thread.Name = GetDownloadStateKey(downloadInfo);
        thread.Start();//启动新线程

        DownloadRecordService.UpdateDowloadState(downloadInfo.Id,0);
    }

    /// <summary>
    /// 下载完成
    /// </summary>
    /// <param name="key"></param>
    public void FinishDownload(string key)
    {

        DownloadRecord record = (DownloadRecord)DownloadRecordMap[key];
        record.DownloadState = 2;
        DownloadRecordService.UpdateDownloadDone(record.Id);

        DownloadStates.Remove(key);
        DownloadRecordMap.Remove(key);
    }

    public Boolean CheckState(string key)
    {
        if (DownloadStates[key] == null)
        {
            return false;
        }
        return true;
    }

    public string GetDownloadStateKey(DownloadRecord downloadInfo)
    {
        return downloadInfo.Id + "";
    }

 

    public void Breakpointdownload()
    {
        string key = Thread.CurrentThread.Name;
        DownloadRecord downloadInfo = (DownloadRecord)DownloadRecordMap[key];

      
      
        while (CheckState(key))
        {

            DateTime startDateTime = DateTime.Now;
            double excuteTime = 0;
            long start = downloadInfo.DownloadSize;
            long end = start + DownloadSize;
            if (start != 0)
            {
                start += 1;
            }

            DownloadResult result = TencentBOSAPI.DownloadObject(downloadInfo, downloadInfo.TargetFolder, start, end);

            TimeSpan ts = DateTime.Now - startDateTime;

            excuteTime = Convert.ToDouble(ts.TotalMilliseconds.ToString()) / 1000;
            if (excuteTime == 0) {
                excuteTime = 0.01;
            }

            downloadInfo.DownloadSize = end;
            downloadInfo.Time = downloadInfo.Time+ excuteTime;
            DownloadRecordService.UpdateDownloadSizeAndTime(downloadInfo);

            Constant.Win.ExecuteJavascript("changeDownloadPercent('" + JsonConvert.SerializeObject(downloadInfo) + "')");


            if (result.isDone())
            {
                FinishDownload(key);
                //调用js通知下载完成 
                Constant.Win.ExecuteJavascript("downloadDone('" + JsonConvert.SerializeObject(downloadInfo) + "')");

                break;
            }
            else if (result.isCloudMiss())
            {
                Constant.Win.ExecuteJavascript("downloadMiss('" + JsonConvert.SerializeObject(downloadInfo) + "')");
                DownloadRecordService.UpdateDowloadState(downloadInfo.Id, 3);
                break;
            }
        }
    }

}

