using CloudManager.util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDisk.service
{
    public class TencentCloudFileService
    {
        private CommonCloudFileService CommonCloudFileService = new CommonCloudFileService();

        /// <summary>
        /// 更新腾讯云的文件
        /// </summary>
        /// <param name="allCloudFiles"></param>
        public void UpdateTencentAll()
        {

            List<CloudFile> allCloudFiles =TencentBOSAPI.ListObjects();

            if (allCloudFiles == null)
            {
                allCloudFiles = new List<CloudFile>();
            }
            //获取数据库中所有的文件
            List<CloudFile> dbCloudFiles = CommonCloudFileService.Select(" isDeleted =0 and Type=1");

            List<CloudFile> addFiles = new List<CloudFile>();

            for (int sub = 0, size = allCloudFiles.Count; sub < size; sub++)
            {
                CloudFile tempBosObjectSummary = allCloudFiles[sub];

                CloudFile cloudFileResult = null;

                Boolean isFind = false;
                for (int innerSub = 0, innerSize = dbCloudFiles.Count; innerSub < innerSize; innerSub++)
                {
                    CloudFile tempCloudFile = dbCloudFiles[innerSub];

                    if (string.Equals(tempCloudFile.Key, tempBosObjectSummary.Key))
                    {
                        isFind = true;
                        cloudFileResult = tempCloudFile;
                        break;
                    }

                }
                //如果发现 则删除 避免重复匹配，以及筛选已删除的文件
                if (isFind)
                {
                    dbCloudFiles.Remove(cloudFileResult);
                }
                else
                {
                    //如果未匹配到 则表示为新增的文件
                    addFiles.Add(tempBosObjectSummary);
                }

            }

            if (dbCloudFiles != null && dbCloudFiles.Count > 0)
            {
                CommonCloudFileService.RemoveByKeys(dbCloudFiles,1);
            }

            if (addFiles.Count > 0)
            {
                InsertTencentAll(addFiles);
            }

        }
 

        /**
         * 保存所有的百度云文件记录
         * 
         * */
        public void InsertTencentAll(List<CloudFile> cloudFiles)
        {
            DateTimeFormatInfo dtFormat = new DateTimeFormatInfo();
            dtFormat.ShortDatePattern = "yyyy-MM-ddThh:mm:ss";
          
            string baseInsert = "INSERT INTO cloud_file (Key,DowloadState,IsDeleted,Size,LastModified,type) values";
            string sqltpl = "('{0}','{1}','{2}','{3}','{4}',1)";

          

            for (int sub = 0, size = cloudFiles.Count; sub < size; sub++)
            {
                CloudFile cloudFile = cloudFiles[sub];
                DateTime dateTime = Convert.ToDateTime(cloudFile.LastModified, dtFormat);
                baseInsert += string.Format(sqltpl, cloudFile.Key, 0, 0, cloudFile.Size, DateUtil.currentTimeMillis(dateTime) + "");
                if (sub + 1 != size)
                {
                    baseInsert += ",";
                }
            }
            Db.Insert(baseInsert);
        }


    }
}
