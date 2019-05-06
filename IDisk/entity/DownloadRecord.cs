using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class DownloadRecord
{
  public int Id { set; get; }

  public DateTime GmtDownload { set; get; }
    /// <summary>
    /// 0为正在下载,1为暂停,2为下载完成,3为下载失败,4为取消
    /// </summary>
   public int DownloadState { set; get; }
   public long DownloadSize { set; get; }
   public long Size { set; get; }
   public int CloudFileId { set; get; }
    /// <summary>
    ///  下载的文件信息
    /// </summary>
   public CloudFile CloudFile { set; get; }
   public double Time;
   public string TargetFolder;

   public Boolean IsDeleted=false;
    /// <summary>
    ///  0为百度云 1为腾讯云
    /// </summary>
   public int Type;

   public string FileName;
}

