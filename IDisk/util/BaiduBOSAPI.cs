using BaiduBce;
using BaiduBce.Auth;
using BaiduBce.Services.Bos;
using BaiduBce.Services.Bos.Model;
using CloudManager.entity;
using System;
using System.Collections.Generic;
using System.IO;

public class BaiduBOSAPI
{
 
    //云盘配置信息
    public static BosConfig BosConfig;

    public BaiduBOSAPI()
	{
	}
 
 
    public static BceClientConfiguration config = null;

    public static BosClient BosClient = null;

    private static object BceClientConfiguration_Lock = new object(); //锁同步

    public static void SetBosConfig(BosConfig val) {
        BosClient = null;
        config = null;
        BosConfig = val;
    }
   
    /**
     * 获取单例的配置
     * 
     * */
    public static BceClientConfiguration GetBceClientConfiguration()
    {
        lock (BceClientConfiguration_Lock)
        {
            if (config == null)
            {
                config = new BceClientConfiguration();
                config.Credentials = new DefaultBceCredentials(BosConfig.AccessKeyId, BosConfig.AccessKey);
                config.Endpoint = BosConfig.Endpoint;
            }
        }
         
        return config;
    }

    /**
     * 
     * 获取单例的BOS服务的客户端
     * 
     * */
    public static BosClient GetBosClient()
    {
       
        lock (BceClientConfiguration_Lock)
        {
            if (BosClient == null)
            {
                BosClient = new BosClient(GetBceClientConfiguration());
            }
        }
        return BosClient;
    }

    public static List<BosObjectSummary> ListObjects()
    {

        if (BosConfig==null) {
            return null;
        }
        ListObjectsRequest listObjectsRequest = new ListObjectsRequest() { BucketName = BosConfig.BucketName, MaxKeys = 5 };
        List<BosObjectSummary> allObjects = new List<BosObjectSummary>();
        // 获取指定Bucket下的所有Object信息
        BaiduBce.Services.Bos.Model.ListObjectsResponse listObjectsResponse = GetBosClient().ListObjects(listObjectsRequest);
        allObjects.AddRange(listObjectsResponse.Contents);
        while (listObjectsResponse.NextMarker!=null) {
            listObjectsRequest.Marker = listObjectsResponse.NextMarker;
            listObjectsResponse = GetBosClient().ListObjects(listObjectsRequest);
            allObjects.AddRange(listObjectsResponse.Contents);
        }


        return allObjects;
    }
 
    public static DownloadResult DownloadObject(DownloadRecord record,String targetFolder,long start =0,long end=199)
    {

        GetObjectRequest getObjectRequest = new GetObjectRequest() { BucketName = BosConfig.BucketName, Key = record.CloudFile.Key };

        // 获取 范围内的数据
        getObjectRequest.SetRange(start, end);

        // 获取Object，返回结果为BosObject对象
        try {
            BosObject bosObject = GetBosClient().GetObject(getObjectRequest);

            Stream stream = bosObject.ObjectContent;

            return FileUtil.saveFileContent(stream, record.FileName, targetFolder);
        } catch (BceServiceException BceServiceException) {
            if (BceServiceException.Message.IndexOf("The requested range cannot be satisfied") != -1)
            {
                DownloadResult result2 = new DownloadResult();
                result2.state = 1;
                return result2;
            }
            Console.WriteLine("exception:"+ BceServiceException);
        }
        DownloadResult result = new DownloadResult();
        result.state = 2;
        return result;
    }

}
