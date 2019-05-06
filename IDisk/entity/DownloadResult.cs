using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudManager.entity
{
    public class DownloadResult
    {
        /// <summary>
        /// 0为正常，1为下载完成，2为不存在该文件，3为磁盘已满,4为未知异常
        /// </summary>
        public int state=0;

        public Boolean isDone() {
            return state == 1;
        }

        public Boolean isCloudMiss()
        {
            return state == 2;
        }

        public Boolean isDiskFull()
        {
            return state == 3;
        }

        public Boolean isOk()
        {
            return state == 0;
        }

        public Boolean isOtherError() {
            return state == 4;
        }
    }
}
