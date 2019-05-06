using CloudManager.config;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;

class BosConfigService
{

    public void CreateBOSConfigTable()
    {
        Db.CreateTable("create table if not exists bos_config(" +
          "Id Integer primary key AUTOINCREMENT, " +
          "AccessKeyId text , " +
          "AccessKey text, " +
          "BucketName text, " +
          "Endpoint text," +
          "IsRight int," +
          "AppId int ," +
          "Type int)");
    }

    /// <summary>
    /// 更新是否正确的状态
    /// </summary>
    /// <param name="bosConfig"></param>
    public void UpdateIsRight(BosConfig bosConfig) {
        string sql = "update bos_config SET ,isRight={0} where Id={1} ";
        sql = String.Format(sql, bosConfig.isRight, bosConfig.Id);
        Db.Update(sql);
    }

    public void Update(BosConfig bosConfig) {
        if (bosConfig.Id==0) {
            bosConfig.Type = Constant.CloudType;
            Insert(bosConfig);
            return;
        }
        string sql = "update bos_config SET AccessKeyId='{0}',AccessKey='{1}',BucketName='{2}',Endpoint='{3}' ,AppId='{4}'where Id={5} ";
        sql = String.Format(sql, bosConfig.AccessKeyId, bosConfig.AccessKey, bosConfig.BucketName, bosConfig.Endpoint,bosConfig.AppId, bosConfig.Id);
        Db.Update(sql);
    }

    public void Insert(BosConfig bosConfig)
    {
        string baseInsert = "INSERT INTO bos_config (AccessKeyId ,AccessKey ,BucketName ,Endpoint ,isRight,type,AppId ) values";
        string sqltpl = "('{0}','{1}','{2}','{3}','{4}','{5}','{6}')";
        baseInsert += String.Format(sqltpl, bosConfig.AccessKeyId, bosConfig.AccessKey, bosConfig.BucketName, bosConfig.Endpoint,1,bosConfig.Type,bosConfig.AppId);
        Db.Insert(baseInsert);
    }

    /// <summary>
    ///  获取 当前云盘类型的配置
    /// </summary>
    /// <param name="where"></param>
    /// <returns></returns>
    public BosConfig Get(string where) {
        string sql = "SELECT * FROM bos_config ";
        if (where != null)
        {
            sql  += " where " + where;
        }
        sql += " limit 1";
        IDbConnection connection = new SQLiteConnection("Data Source=" + Db.GetDBPath());

        var query = connection.Query<BosConfig>(sql);
        if (query.Count()==0) {
            return null;
        }
        return (BosConfig)query.First();
    }
}

