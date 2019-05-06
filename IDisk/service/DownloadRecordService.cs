using CloudManager.config;
using Dapper;
using IDisk.service;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


public class DownloadRecordService
{
    /// <summary>
    /// 创建下载记录表
    /// </summary>
    public static void CreateDownloadRecordTable() {
        Db.CreateTable("create table  if not exists download_record(" +
            "Id Integer primary key AUTOINCREMENT, " +
            "GmtDownload text, " +
            "DownloadState int, " +
            "DownloadSize int, " +
            "Size int, " +
            "CloudFileId int," +
            "Time int," +
            "TargetFolder text," +
            "IsDeleted int ," +
            "Type int ," +
            "FileName text" +
            ")"
        );
    }

    /// <summary>
    /// 更新下载进度
    /// </summary>
    /// <param name="record"></param>
    public void UpdateDownloadSizeAndTime(DownloadRecord record)
    {
        string sql = "update download_record SET downloadSize='{0}',Time='{1}' where id={2} ";
        sql = String.Format(sql, record.DownloadSize,record.Time, record.Id);
        Db.Update(sql);
    }

    public void UpdateFileName(DownloadRecord record)
    {
        string sql = "update download_record SET FileName='{0}' where id={2} ";
        sql = String.Format(sql, record.FileName, record.Id);
        Db.Update(sql);
    }
    /// <summary>
    /// 将下载记录更新为下载完成
    /// </summary>
    /// <param name="id"></param>
    public void UpdateDownloadDone(int id) {
        UpdateDowloadState(id, 2);
    }

    /// <summary>
    ///  将下载记录更新为下载失败
    /// </summary>
    /// <param name="id"></param>
    public void UpdateDownloadError(int id)
    {
        UpdateDowloadState(id, 2);
    }
    /// <summary>
    /// 更新下载状态
    /// </summary>
    /// <param name="id"></param>
    /// <param name="state"></param>
    public void UpdateDowloadState(int id, int state) {
        string sql = "update download_record SET DownloadState='{0}' where Id={1} ";
        sql = String.Format(sql, state, id);
        Db.Update(sql);
    }

    /// <summary>
    ///  插入一条下载记录
    /// </summary>
    /// <param name="record"></param>
    public void Insert(DownloadRecord record)
    {
        int isExist = isExistFileName(record.FileName);
        while (isExist!=0) {
            Random reum = new Random();
            int randomdata = reum.Next(100);

            record.FileName="("+randomdata+")"+ record.FileName;
            isExist = isExistFileName(record.FileName);
        }
        string baseInsert = "INSERT INTO download_record (GmtDownload ,DownloadState ,DownloadSize ,Size,CloudFileId,TargetFolder,Time,IsDeleted,Type,FileName) values";
        string sqltpl = "('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}')";
        baseInsert += String.Format(sqltpl, DateTime.UtcNow, 0, record.DownloadSize, record.Size, record.CloudFileId, record.TargetFolder, 0, 0,record.Type,record.FileName);
        Db.Insert(baseInsert);
    }

    public int isExistFileName(string fileName) {
        using (IDbConnection connection = Db.getDbConnection())
        {
            var data = connection.Query<int>("select count(id) from download_record where FileName='"+fileName+"'");
            return data.FirstOrDefault();
        }
    }

    /// <summary>
    ///  分页查询未下载完成的文件
    /// </summary>
    /// <param name="pageNum"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    public Page<DownloadRecord> PageDownloadingRecord(int pageNum, int pageSize,int type) {
        Page<DownloadRecord> page = new Page<DownloadRecord>();
        page.List = Select("(DownloadState =1 or DownloadState =0 or DownloadState =3)  and IsDeleted=0 and Type= "+type+" limit " + pageSize + " offset " + ((pageNum - 1) * pageSize));
        page.TotalSize = Count(" (DownloadState =1 or DownloadState =0 or DownloadState =3) and IsDeleted=0 and Type="+ type);
        page.PageNum = pageNum;
        page.PageSize = pageSize;
        return page;
    }
    /// <summary>
    /// 分页查询下载完成的文件记录
    /// </summary>
    /// <param name="pageNum"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    public Page<DownloadRecord> PageDownloadDoneRecord(int pageNum, int pageSize,int type)
    {
        Page<DownloadRecord> page = new Page<DownloadRecord>();
        page.List = Select(" (DownloadState=2 or DownloadState =4 ) and IsDeleted=0 and Type= " + type + " limit " + pageSize + " offset " + ((pageNum - 1) * pageSize));
        page.TotalSize = Count("(DownloadState=2 or DownloadState =4 ) and IsDeleted=0 and Type= "+type);
        page.PageNum = pageNum;
        page.PageSize = pageSize;
        return page;
    }

    /**
   * 获取总数
   */
    public int Count(string where)
    {
        string sql = "SELECT count(*) FROM download_record ";
        if (where != null)
        {
            sql += " where " + where;
        }
 ;
        IDbConnection connection = new SQLiteConnection("Data Source=" + Db.GetDBPath());

        var query = connection.Query<int>(sql);
        return (int)query.First();

    }

    public List<DownloadRecord> selectByIds(List<int> ids) {
        string where = " id in(";
        for (int i = 0, size = ids.Count; i < size; i++) {
            where += ids[i];
            if (i + 1 != size) {
                where += ",";
            }
        }
        where += ")";

        return Select(where);
    }

    public List<DownloadRecord> Select(string where)
    {
        string sql = "SELECT * FROM download_record ";
        if (where != null)
        {
            sql += " where " + where;
        }

        using (IDbConnection connection = Db.getDbConnection()) {
            var records = connection.Query<DownloadRecord>(sql);

            var cloudFILES = connection.Query<CloudFile>(
               "select * from cloud_file where Id in @Ids",
               new { Ids = records.Select(m => m.CloudFileId).Distinct() }
            );


            foreach (DownloadRecord record in records)
            {
                foreach (CloudFile CloudFile in cloudFILES)
                {
                    if (CloudFile.Id == record.CloudFileId) {
                        record.CloudFile = CloudFile;

                    }
                }
            }
            return (List<DownloadRecord>)records;
        }
    }

    public string getLastTargetFolder()
    {
        using (IDbConnection connection = Db.getDbConnection())
        {
            var data = connection.Query<string>("select TargetFolder from download_record order by id desc");
            return data.FirstOrDefault();
        }
    }

    /// <summary>
    /// 获取最后一次插入的id
    /// </summary>
    /// <returns></returns>
    public int getLastId()
    {
        using (IDbConnection connection = Db.getDbConnection())
        {
            var data = connection.Query<int>("select id from download_record order by id desc limit 1");
            return data.FirstOrDefault();
        }
    }

    /// <summary>
    /// 根据id获取下载记录
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public DownloadRecord selectById(int id)
    {
        using (IDbConnection connection = Db.getDbConnection())
        {
            var query = connection.Query<DownloadRecord>("select * from download_record where id='" + id + "'");
            DownloadRecord downloadRecord = query.FirstOrDefault();
            if (downloadRecord != null) {
                downloadRecord.CloudFile = new CommonCloudFileService().selectById(downloadRecord.CloudFileId);
            }
            return downloadRecord;
        }
    }

    /// <summary>
    /// 重新下载未完成的文件
    /// </summary>
    public void StartBaiduDownloading() {
        foreach (DownloadRecord record in Select(" DownloadState=0 and Type=0"))
        {
            DownloadThreadPool.Me.StartDownload(record);
        }
    }

    /// <summary>
    /// 重新下载未完成的文件
    /// </summary>
    public void StartTencentDownloading()
    {
        foreach (DownloadRecord record in Select(" DownloadState=0 and Type=1"))
        {
            DownloadThreadPool.Me.StartDownload(record);
        }
    }

    /// <summary>
    ///  假删除下载记录
    /// </summary>
    /// <param name="ids"></param>
    public void deletes(List<int> ids) {
        string baseSql = "update  download_record set IsDeleted=1 where Id in (";

        foreach (int id in ids) {
            baseSql += id;
            DownloadThreadPool.Me.CancelDowload(id+"");
        }

        baseSql += ")";
        Db.Update(baseSql);
    }
}

