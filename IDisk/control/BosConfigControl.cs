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
    public class BosConfigControl
    {

        private BaiduCloudFileService BaiduCloudFileService = new BaiduCloudFileService();

        private TencentCloudFileService TencentCloudFileService = new TencentCloudFileService();

        private BosConfigService BosConfigService = new BosConfigService();

        public  void addFunction(JSObject myObject) {
            //修改配置
            var editBosConfigFunc = myObject.AddFunction("editBosConfig");
            editBosConfigFunc.Execute += (func, args) =>
            {
                var jsparams = args.Arguments.FirstOrDefault(p => p.IsArray);
                var jsArray = CfrV8Value.CreateArray(1);
                BosConfig bosConfig = new BosConfig();
                bosConfig.AccessKeyId = jsparams.GetValue(0).StringValue;
                bosConfig.AccessKey = jsparams.GetValue(1).StringValue;
                bosConfig.BucketName = jsparams.GetValue(2).StringValue;
                bosConfig.Endpoint = jsparams.GetValue(3).StringValue;
                bosConfig.Id = jsparams.GetValue(4).IntValue;
                bosConfig.AppId = jsparams.GetValue(5).StringValue;
                Result result = new Result();

                BosConfig beforeBosConfig = Constant.CloudType == 0 ? BaiduBOSAPI.BosConfig : TencentBOSAPI.BosConfig;

                try
                {
                    BaiduBOSAPI.SetBosConfig(bosConfig);

                    if (Constant.CloudType == 0)
                    {
                        BaiduBOSAPI.BosConfig = bosConfig;
                        BaiduCloudFileService.UpdateBaiduAll();
                    }
                    else {
                        TencentBOSAPI.BosConfig = bosConfig;
                        TencentCloudFileService.UpdateTencentAll();

                    }

                   
                    BosConfigService.Update(bosConfig);
                    
                }
                catch (Exception exception)
                {
                    Console.WriteLine("Exception caught: {0}", exception);
                    result.State = 4;
                    result.Msg = "配置信息错误，请检查后再次提交";
                    if (Constant.CloudType == 0) {
                        BaiduBOSAPI.SetBosConfig(beforeBosConfig);
                    }
                    else {
                        TencentBOSAPI.BosConfig = beforeBosConfig;
                    }
                   
                }

                jsArray.SetValue(0, CfrV8Value.CreateString(JsonConvert.SerializeObject(result)));
                args.SetReturnValue(jsArray);
            };

            //获取配置信息
            var getBosConfigFunc = myObject.AddFunction("getBosConfig");

            getBosConfigFunc.Execute += (func, args) =>
            {
                var jsArray = CfrV8Value.CreateArray(1);
                Result result = new Result();
                BosConfig config=BosConfigService.Get(" Type="+Constant.CloudType);
                if (config==null) {
                    config = new BosConfig();
                }
                config.Type = Constant.CloudType;
                result.Data = config;
                jsArray.SetValue(0, CfrV8Value.CreateString(JsonConvert.SerializeObject(result)));
                args.SetReturnValue(jsArray);
            };
        }
    }
}
