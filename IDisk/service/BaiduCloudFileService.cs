using BaiduBce.Services.Bos.Model;
using CloudManager.util;
using Dapper;
using IDisk.service;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

class BaiduCloudFileService
{

    private CommonCloudFileService CommonCloudFileService = new CommonCloudFileService();
 
    /// <summary>
    /// 更新百度云所有文件
    /// </summary>
    /// <param name="bosObjectSummarys"></param>
    public void UpdateBaiduAll() {
        List<BosObjectSummary> bosObjectSummarys = BaiduBOSAPI.ListObjects();
        if (bosObjectSummarys==null) {
            bosObjectSummarys = new List<BosObjectSummary>();
        }
        //获取数据库中所有的文件
        List<CloudFile> dbCloudFiles = CommonCloudFileService.Select(" isDeleted =0 and Type=0");
 
        List<BosObjectSummary> addFiles = new List<BosObjectSummary>();

        for (int sub = 0, size = bosObjectSummarys.Count; sub < size; sub++)
        {
            BosObjectSummary tempBosObjectSummary = bosObjectSummarys[sub];

            CloudFile cloudFileResult = null;

            Boolean isFind = false;
            for (int innerSub = 0, innerSize = dbCloudFiles.Count; innerSub < innerSize; innerSub++)
            {
                CloudFile tempCloudFile = dbCloudFiles[innerSub];

                if (string.Equals(tempCloudFile.Key, tempBosObjectSummary.Key)) {
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
            else {
            //如果未匹配到 则表示为新增的文件
                addFiles.Add(tempBosObjectSummary);
            }

        }
        
        if (dbCloudFiles!=null&&dbCloudFiles.Count>0) {
            CommonCloudFileService.RemoveByKeys(dbCloudFiles,0);
        }

        if (addFiles.Count>0) {
            InsertBaiduAll(addFiles);
        }

    }

    /**
     * 保存所有的百度云文件记录
     * 
     * */
    public void InsertBaiduAll(List<BosObjectSummary> bosObjectSummarys) {

        string baseInsert= "INSERT INTO cloud_file (Key,DowloadState,IsDeleted,Size,LastModified,type) values";
        string sqltpl = "('{0}','{1}','{2}','{3}','{4}',0)";
        for (int sub = 0, size = bosObjectSummarys.Count; sub < size; sub++) {
            BosObjectSummary tempBosObjectSummary = bosObjectSummarys[sub];
            baseInsert += string.Format(sqltpl, tempBosObjectSummary.Key,  0, 0, tempBosObjectSummary.Size, DateUtil.currentTimeMillis(tempBosObjectSummary.LastModified)+"");
            if (sub+1!=size) {
                baseInsert += ",";
            }
        }
        Db.Insert(baseInsert);
    }

}
