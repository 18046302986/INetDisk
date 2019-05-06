using CloudManager.entity;
using CloudManager.util;
using COSXML;
using COSXML.Auth;
using COSXML.Model.Bucket;
using COSXML.Model.Object;
using COSXML.Model.Tag;
using COSXML.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IDisk
{
    public class TencentBOSAPI
    {
        //方式1， 永久密钥
        //public static string secretId = "AKID0nPljaoyyjEXaSaxO2cy95qNUMzRxgcP"; //"云 API 密钥 SecretId";
        //public static string secretKey = "i390d4KExSkotkMo1xhPtDQP8frsqaAV"; //"云 API 密钥 SecretKey";
        //public static string appid = "1251970926";//设置腾讯云账户的账户标识 APPID
       // public static string bucket = "smile-" + appid; //存储桶，格式：BucketName-APPID
       // public static string region = "ap-beijing"; //设置一个默认的存储桶地域

        //云盘配置信息
        public static BosConfig BosConfig;

        public static CosXmlServer getCosXmlServer()
        {

            long durationSecond = 600;  //secretKey 有效时长,单位为 秒

            QCloudCredentialProvider cosCredentialProvider = new DefaultQCloudCredentialProvider(BosConfig.AccessKeyId, BosConfig.AccessKey, durationSecond);
            //初始化 CosXmlServer
            return new CosXmlServer(getClientConfig(), cosCredentialProvider);
        }

        public static CosXmlConfig getClientConfig()
        {

            CosXmlConfig config = new CosXmlConfig.Builder()
                .SetConnectionTimeoutMs(60000)  //设置连接超时时间，单位 毫秒 ，默认 45000ms
                .SetReadWriteTimeoutMs(40000)  //设置读写超时时间，单位 毫秒 ，默认 45000ms
                .IsHttps(true)  //设置默认 https 请求
                .SetAppid(BosConfig.AppId)  //设置腾讯云账户的账户标识 APPID
                .SetRegion(BosConfig.Endpoint)  //设置一个默认的存储桶地域
               
                .Build();  //创建 CosXmlConfig 对象
            return config;
        }

        /// <summary>
        /// 获取bukect中的所有文件
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="config"></param>
        /// <param name="cosCredentialProvider"></param>
        /// <returns></returns>
        public static List<CloudFile> ListObjects()
        {

            if (BosConfig==null) {
                return null;
            }

            //初始化 CosXmlServer
            CosXmlServer cosXml = getCosXmlServer();
            GetBucketRequest request = new GetBucketRequest(BosConfig.BucketName);
            //设置签名有效时长
            request.SetSign(TimeUtils.GetCurrentTime(TimeUnit.SECONDS), 600);
            request.SetMaxKeys(1 + "");
            //执行请求
            GetBucketResult result = cosXml.GetBucket(request);

            ListBucket listBucket = result.listBucket;

            List<ListBucket.Contents> contents = listBucket.contentsList;

            List<CloudFile> cloudFiles = new List<CloudFile>();

            foreach (ListBucket.Contents content in contents)
            {
                CloudFile cloudFile = new CloudFile
                {
                    Key = content.key,
                    Size = content.size,
                    LastModified = content.lastModified
                };
                cloudFiles.Add(cloudFile);
            }

            while (result.listBucket.nextMarker != null)
            {
                request.SetMarker(result.listBucket.nextMarker);

                //执行请求
                result = cosXml.GetBucket(request);

                foreach (ListBucket.Contents content in result.listBucket.contentsList)
                {
                    CloudFile cloudFile = new CloudFile
                    {
                        Key = content.key,
                        Size = content.size,
                        LastModified = content.lastModified
                    };

                    cloudFiles.Add(cloudFile);
                }
            }

            Console.WriteLine(cloudFiles);
            return cloudFiles;
          
        }

        /// <summary>
        /// 分段下载
        /// </summary>
        /// <param name="key"></param>
        /// <param name="targetFolder"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static DownloadResult DownloadObject(DownloadRecord record, string targetFolder, long start, long end)
        {   
            DownloadResult downloadResult = new DownloadResult();
            Random reum = new Random();
            int randomdata = reum.Next(100000);
            string tempFileName = DateUtil.currentTimeMillis(new DateTime()) + randomdata + ".temp";
            try
            {
                GetObjectRequest request = new GetObjectRequest(BosConfig.BucketName, record.CloudFile.Key, targetFolder, tempFileName);
                request.SetRange(start, end);
                //设置签名有效时长
                request.SetSign(TimeUtils.GetCurrentTime(TimeUnit.SECONDS), 600);
                //执行请求
                GetObjectResult result = getCosXmlServer().GetObject(request);
                Console.WriteLine("msg" + result.GetResultInfo());

                if (String.Equals("OK", result.httpMessage)) {

                } else if (String.Equals("Requested Range Not Satisfiable", result.httpMessage)) {
                    downloadResult.state = 1;
                    File.Delete(targetFolder + @"\" + tempFileName);
                    return downloadResult;
                } else if (String.Equals("Not Found", result.httpMessage)) {
                    downloadResult.state = 2;
                    File.Delete(targetFolder + @"\" + tempFileName);
                    return downloadResult;
                }
                Stream stream = new FileStream(targetFolder + @"\" + tempFileName, FileMode.Open);
               

                downloadResult = FileUtil.saveFileContent(stream, record.FileName, targetFolder);

                File.Delete(targetFolder + @"\" + tempFileName);
            }
            catch (COSXML.CosException.CosClientException clientEx)
            {
                //请求失败
                Console.WriteLine("CosClientException: " + clientEx.Message);
                downloadResult.state = 2;
            }
            catch (COSXML.CosException.CosServerException serverEx)
            {
                //请求失败
                Console.WriteLine("CosServerException: " + serverEx.GetInfo());
                downloadResult.state = 2;
            }
            return downloadResult;
        }
    }
}
