using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CloudFile
{
    public int Id;
    public string Key;
    /// <summary>
    /// 0为未下载，1为正在下载，2为下载完成
    /// </summary>
    public int DowloadState;
    public Boolean IsDeleted;
    public long Size;
    public string LastModified;
    /// <summary>
    ///  类型 0 为百度云 1为腾讯云
    /// </summary>
    public int Type;
}

