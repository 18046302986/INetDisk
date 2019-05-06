using Chromium.Remote;
using Chromium.WebBrowser;
using CloudManager.config;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDisk.control
{
    public class ConstantControl
    {
        public static void addFunction(JSObject myObject) {

            //刷新
            var changeConstantCloudTypeFunc = myObject.AddFunction("changeConstantCloudType");

            changeConstantCloudTypeFunc.Execute += (func, args) =>
            {
                var jsArray = CfrV8Value.CreateArray(1);
                var jsparams = args.Arguments.FirstOrDefault(p => p.IsArray);

                //切换类型
                Constant.CloudType = jsparams.GetValue(0).IntValue;
               
                Result result = new Result();
                jsArray.SetValue(0, CfrV8Value.CreateString(JsonConvert.SerializeObject(result)));
                args.SetReturnValue(jsArray);
            };

        }
    }
}
