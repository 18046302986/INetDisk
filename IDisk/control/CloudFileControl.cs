using BaiduBce;
using Chromium.Remote;
using Chromium.WebBrowser;
using CloudManager.config;
using IDisk;
using IDisk.service;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudManager.control
{
    public class CloudFileControl
    {
        private CommonCloudFileService CommonCloudFileService = new CommonCloudFileService();
        private BosConfigService BosConfigService = new BosConfigService();
        private DownloadRecordService DownloadRecordService = new DownloadRecordService();

        private BaiduCloudFileService BaiduCloudFileService = new BaiduCloudFileService();

        private TencentCloudFileService TencentCloudFileService = new TencentCloudFileService();

        public void addFunction(JSObject myObject) {

            //获取全部列表
            var getArrayFromCSFunc = myObject.AddFunction("getArrayFromCSharp");

            getArrayFromCSFunc.Execute += (func, args) =>
            {

                var jsparams = args.Arguments.FirstOrDefault(p => p.IsArray);

                int pageNum = jsparams.GetValue(0).IntValue;
                int pageSize = jsparams.GetValue(1).IntValue;
                Result result = new Result();
                var jsArray = CfrV8Value.CreateArray(1);

                BosConfig bosConfig = Constant.CloudType == 0 ? BaiduBOSAPI.BosConfig : TencentBOSAPI.BosConfig;

                if (bosConfig == null )
                {
                    result.State = 3;
                    result.Msg = "配置未初始化";
                }
                else
                {
                    try
                    {
                        if (Constant.CloudType == 0)
                        {
                            BaiduCloudFileService.UpdateBaiduAll();
                        }
                        else {

                            TencentCloudFileService.UpdateTencentAll();
                        }
                       
                        result.Data = CommonCloudFileService.Page(pageNum, pageSize);
                    } catch (BceServiceException exception) {
                        if (exception.Message.IndexOf("is an overdue bill of your account") != -1)
                        {
                            result.State = 1;
                            result.Msg = "您的百度云账号已欠费，请充值后使用";
                        } 
                     } catch (Exception exception)
                    {
                        Console.WriteLine("Exception caught: {0}", exception);
                        result.State = 4;
                        result.Msg = "配置信息错误";
                    }
                }

                jsArray.SetValue(0, CfrV8Value.CreateString(JsonConvert.SerializeObject(result)));

                args.SetReturnValue(jsArray);
            };

            //刷新
            var refreshCloudFileFunc = myObject.AddFunction("refreshCloudFile");

            refreshCloudFileFunc.Execute += (func, args) =>
            {
                var jsArray = CfrV8Value.CreateArray(1);

                Result result = new Result();

                try
                {
                    if (Constant.CloudType == 0)
                    {
                        BaiduCloudFileService.UpdateBaiduAll();
                    }
                    else {
                        TencentCloudFileService.UpdateTencentAll();
                    }

                    result.Data = CommonCloudFileService.Page(1, 15);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception caught: {0}", e);
                    result.State = 1;
                }
                jsArray.SetValue(0, CfrV8Value.CreateString(JsonConvert.SerializeObject(result)));
                args.SetReturnValue(jsArray);
            };
        }
    }
}
