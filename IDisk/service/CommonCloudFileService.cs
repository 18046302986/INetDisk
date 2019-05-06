using CloudManager.config;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDisk.service
{
    public class CommonCloudFileService
    {

        public void CreateCloudFileTable()
        {
            Db.CreateTable("create table if not exists cloud_file(" +
                "Id INTEGER  primary key AUTOINCREMENT," +
                "Key TEXT," +
                "Size int," +
                "LastModified text," +
                "DowloadState INTEGER DEFAULT 0," +
                "IsDeleted int DEFAULT 0 ," +
                "Type int " +
                ");");
        }
 
        /// <summary>
        /// 修改下载状态 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="state"></param>
        public void SetDowloadState(String key, int state)
        {
            string sql = "update cloud_file SET DowloadState = " + state + "  WHERE Key = ' " + key + "' and type="+Constant.CloudType;
            Db.Update(sql);
        }

     

        /**
         * 根据key删除
         * */
        public void RemoveByKey(String key)
        {
            string sql = "update cloud_file SET IsDeleted = " + 1 + "  WHERE Key = ' " + key + "' and type=" + Constant.CloudType;
            Db.Update(sql);
        }

        /**
        * 根据多个key删除
        * */
        public void RemoveByKeys(List<CloudFile> cloudFiles,int type)
        {
            string sql = "update cloud_file SET IsDeleted = " + 1 + "  WHERE Key  in (  ";
            for (int sub = 0, size = cloudFiles.Count; sub < size; sub++)
            {
                sql += "'" + cloudFiles[sub].Key + "'";
                if (sub + 1 != size)
                {
                    sql += ",";
                }
            }
            sql += ") and type=" + type;
            Db.Update(sql);
        }

        /**
         * 获取总数
         */
        public int Count(string where)
        {
            string sql = "SELECT count(*) FROM cloud_file where type=" + Constant.CloudType;
            if (where != null)
            {
                sql += " and "+ where;
            }
            IDbConnection connection = new SQLiteConnection("Data Source=" + Db.GetDBPath());

            var query = connection.Query<int>(sql);


            return (int)query.First();
        }

        public Page<CloudFile> Page(int pageNum, int pageSize)
        {
            Page<CloudFile> page = new Page<CloudFile>();
            page.List = Select(" IsDeleted=0 and type=" + Constant.CloudType+" order by id desc limit " + pageSize + " offset " + ((pageNum - 1) * pageSize));
            page.PageNum = pageNum;
            page.PageSize = pageSize;
            page.TotalSize = Count(" IsDeleted=0 ");
            return page;
        }

        public List<CloudFile> SelectAll()
        {
            return Select(null);
        }

        public List<CloudFile> Select(string where)
        {
            string sql = "select * from cloud_file ";
            if (where != null)
            {
                sql += " where " + where;
            }

            using (IDbConnection connection = Db.getDbConnection())
            {

                var query = connection.Query<CloudFile>(sql);

                return (List<CloudFile>)query;
            }
        }

        public CloudFile selectById(int id)
        {
            using (IDbConnection connection = Db.getDbConnection())
            {
                var query = connection.Query<CloudFile>("select * from cloud_file where id='" + id + "'");
                return query.FirstOrDefault();
            }
        }
    }
}
