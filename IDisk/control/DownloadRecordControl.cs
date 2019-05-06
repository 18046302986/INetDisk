using BaiduBce;
using Chromium.Remote;
using Chromium.WebBrowser;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using IDisk.service;
using CloudManager.config;

namespace CloudManager.control
{
    public class DownloadRecordControl
    {
        private CommonCloudFileService CommonCloudFileService = new CommonCloudFileService();
        private BosConfigService BosConfigService = new BosConfigService();
        private DownloadRecordService DownloadRecordService = new DownloadRecordService();

        public  void addFunction(JSObject myObject) {
            //添加下载记录
            var addDownloadRecordFunc = myObject.AddFunction("addDownloadRecord");
            addDownloadRecordFunc.Execute += (func, args) =>
            {

                var jsparams = args.Arguments.FirstOrDefault(p => p.IsArray);
                var jsArray = CfrV8Value.CreateArray(1);

                DownloadRecord record = new DownloadRecord();
                record.CloudFileId = jsparams.GetValue(0).IntValue;

                Result result = new Result();

                int code = 0;

                try
                {
                    record.TargetFolder = selectFolder();
                    if (record.TargetFolder == null)
                    {
                        code = 1;
                    }
                    else
                    {
                        record.CloudFile = CommonCloudFileService.selectById(record.CloudFileId);
                        record.Type = record.CloudFile.Type;
                        record.FileName = record.CloudFile.Key;
                        DownloadRecordService.Insert(record);
                        record.Id = DownloadRecordService.getLastId();
                        //开始下载
                        if (record.Type == 0)
                        {
                            DownloadThreadPool.Me.StartDownload(record);
                        }
                        else
                        {
                            TencentDownloadThreadPool.Me.StartDownload(record);
                        }
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine("Exception caught: {0}", exception);
                    result.State = 1;
                    result.Msg = "系统繁忙";
                }

                jsArray.SetValue(0, CfrV8Value.CreateString(JsonConvert.SerializeObject(result)));
                jsArray.SetValue(1, CfrV8Value.CreateString(code.ToString()));
                args.SetReturnValue(jsArray);

            };

            //添加下载记录
            var addDownloadRecordsFunc = myObject.AddFunction("addDownloadRecords");
            addDownloadRecordsFunc.Execute += (func, args) =>
            {

                var jsparams = args.Arguments.FirstOrDefault(p => p.IsArray);
                int length = jsparams.GetValue(0).IntValue;
                var jsArray = CfrV8Value.CreateArray(1);

                Result result = new Result();

                int code = 0;

                try
                {
                    string folderPath = selectFolder();
                    if (folderPath == null)
                    {
                        code = 1;
                    }
                    else
                    {
                        //同时下载多个文件
                        List<DownloadRecord> records = new List<DownloadRecord>();

                        for (int i=1,size= length;i<size;i++ )
                        {
                            DownloadRecord record = new DownloadRecord();
                            record.CloudFileId = jsparams.GetValue(i).IntValue;

                            if (record.CloudFileId == 0)
                            {
                                continue;
                            }

                            record.CloudFile = CommonCloudFileService.selectById(record.CloudFileId);
                            record.Type = record.CloudFile.Type;
                            record.TargetFolder = folderPath;


                            if (i > 6)
                            {
                                record.DownloadState = 1;
                                records.Add(record);
                            }
                            else
                            {
                                records.Add(record);

                            }
                            record.FileName = record.CloudFile.Key;
                            DownloadRecordService.Insert(record);

                            record.Id = DownloadRecordService.getLastId();


                            if (record.Type == 0)
                            {
                                DownloadThreadPool.Me.StartDownload(record);
                            }
                            else {
                                TencentDownloadThreadPool.Me.StartDownload(record);
                            }
                            
                        }  
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine("Exception caught: {0}", exception);
                    result.State = 1;
                    result.Msg = "系统繁忙";
                }

                jsArray.SetValue(0, CfrV8Value.CreateString(JsonConvert.SerializeObject(result)));
                jsArray.SetValue(1, CfrV8Value.CreateString(code.ToString()));
                args.SetReturnValue(jsArray);

            };


            //批量修改下载状态
            var toggerDownloadStatesFunc = myObject.AddFunction("toggerDownloadStates");
            toggerDownloadStatesFunc.Execute += (func, args) =>
            {
                var jsparams = args.Arguments.FirstOrDefault(p => p.IsArray);
                var jsArray = CfrV8Value.CreateArray(1);
               
                int state = jsparams.GetValue(0).IntValue;

                List<int> ids = new List<int>();

                for (int i = 1; i < jsparams.ArrayLength; i++)
                {
                    if (state == 1)
                    {
                        DownloadThreadPool.Me.PauseDownload(jsparams.GetValue(i).IntValue + "");
                        TencentDownloadThreadPool.Me.PauseDownload(jsparams.GetValue(i).IntValue + "");
                    }
                    else if (state == 0)
                    {
                        DownloadRecord record = DownloadRecordService.selectById(jsparams.GetValue(i).IntValue);
                        record.CloudFile = CommonCloudFileService.selectById(record.CloudFileId);

                        if (record.Type == 0)
                        {
                            DownloadThreadPool.Me.StartDownload(record);
                        }
                        else
                        {
                            TencentDownloadThreadPool.Me.StartDownload(record);
                        }
                        
                    }
                }
                jsArray.SetValue(0, CfrV8Value.CreateString(JsonConvert.SerializeObject(new Result())));
                args.SetReturnValue(jsArray);
            };


            //修改下载状态
            var toggerDownloadStateFunc = myObject.AddFunction("toggerDownloadState");
            toggerDownloadStateFunc.Execute += (func, args) =>
            {
                var jsparams = args.Arguments.FirstOrDefault(p => p.IsArray);
                var jsArray = CfrV8Value.CreateArray(1);
                int id = jsparams.GetValue(0).IntValue;
                int state = jsparams.GetValue(1).IntValue;

                if (state == 1)
                {
                    DownloadThreadPool.Me.PauseDownload(id + "");
                    TencentDownloadThreadPool.Me.PauseDownload(id + "");
                }
                else if (state == 0)
                {
                    DownloadRecord record = DownloadRecordService.selectById(id);
                    record.CloudFile = CommonCloudFileService.selectById(record.CloudFileId);
                    if (record.Type == 0)
                    {
                        DownloadThreadPool.Me.StartDownload(record);
                    }
                    else
                    {
                        TencentDownloadThreadPool.Me.StartDownload(record);
                    }
                }
                else if (state == 4)
                {
                    //停止下载
                }

                jsArray.SetValue(0, CfrV8Value.CreateString(JsonConvert.SerializeObject(new Result())));
                args.SetReturnValue(jsArray);
            };

            //查询下载记录
            var listDownloadRecordFunc = myObject.AddFunction("listDownloadRecord");
            listDownloadRecordFunc.Execute += (func, args) =>
            {
                var jsparams = args.Arguments.FirstOrDefault(p => p.IsArray);
                var jsArray = CfrV8Value.CreateArray(1);
                int queryState = jsparams.GetValue(0).IntValue;
                int pageNum = jsparams.GetValue(1).IntValue;
                int pageSize = jsparams.GetValue(2).IntValue;

                Page<DownloadRecord> page = null;

                Result result = new Result();

                try
                {
                    if (queryState == 0)
                    {
                        page = DownloadRecordService.PageDownloadingRecord(pageNum, pageSize,Constant.CloudType);
                    }
                    else
                    {
                        page = DownloadRecordService.PageDownloadDoneRecord(pageNum, pageSize, Constant.CloudType);
                    }
                    result.Data = page;
                }
                catch (Exception exception)
                {
                    Console.WriteLine("Exception caught: {0}", exception);
                    result.State = 1;
                    result.Msg = "系统繁忙";
                }

                jsArray.SetValue(0, CfrV8Value.CreateString(JsonConvert.SerializeObject(result)));
                args.SetReturnValue(jsArray);
            };


            //查询下载记录
            var openDownloadFileFolderFunc = myObject.AddFunction("openDownloadFileFolder");
            openDownloadFileFolderFunc.Execute += (func, args) =>
            {
                var jsparams = args.Arguments.FirstOrDefault(p => p.IsArray);
                var jsArray = CfrV8Value.CreateArray(1);
                int id = jsparams.GetValue(0).IntValue;
                openFolder(DownloadRecordService.selectById(id));
                jsArray.SetValue(0, CfrV8Value.CreateString(JsonConvert.SerializeObject(new Result())));
                args.SetReturnValue(jsArray);
            };

            //删除下载记录
            var deleteDownloadRecordFunc = myObject.AddFunction("deleteDownloadRecords");
            deleteDownloadRecordFunc.Execute += (func, args) =>
            {
                var jsparams = args.Arguments.FirstOrDefault(p => p.IsArray);
                List<int> ids = new List<int>();

                for (int i = 0; i < jsparams.ArrayLength; i++)
                {
                    ids.Add(jsparams.GetValue(i).IntValue);
                }

                DownloadRecordService.deletes(ids);

                var jsArray = CfrV8Value.CreateArray(1);
               
                jsArray.SetValue(0, CfrV8Value.CreateString(JsonConvert.SerializeObject(new Result())));
                args.SetReturnValue(jsArray);
            };
        }

        private void openFolder(DownloadRecord recorde) {
            Process p = new Process();
            p.StartInfo.FileName = "explorer.exe";
            p.StartInfo.Arguments = @" /select, "+ recorde.TargetFolder+@"\"+@recorde.FileName;
            p.Start();
        }

        private String selectFolder()
        {

            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "设置下载存储路径";

            string savePath = null;


            dialog.SelectedPath = DownloadRecordService.getLastTargetFolder();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                savePath = dialog.SelectedPath;

            }
            else
            {
            }
            return savePath;
        }
    }
}
