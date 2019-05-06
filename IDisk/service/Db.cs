using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Windows.Forms;
using System.Data;
using System.Threading;
using System.Data.SqlClient;
using Dapper;
using System.IO;

class Db
{

    private static readonly object obj = new object();
    public static string GetDBPath()
    {
        return Application.StartupPath + @"\sqlite\cloudfile.sqlite";
    }

    public static void InitDB() {
        if (!File.Exists(Db.GetDBPath())) {
            Directory.CreateDirectory(Application.StartupPath + @"\sqlite\");
            SQLiteConnection.CreateFile(Db.GetDBPath());
        }  
    }

    public static void CreateTable(string sql)
    {
        ExecuteNonQuery(sql, null);
    }

    public static void Insert(string sql) {
        ExecuteNonQuery(sql,null);
    }

    private static int ExecuteNonQuery(string StrSQL,  SQLiteParameter[] SQLiteParams)
    {
        string path = GetDBPath();
        IDbConnection connection = new SQLiteConnection("data source="+path);
        return connection.Execute(StrSQL);
    }

    public static void Update(string sql) {
        ExecuteNonQuery(sql, null);
    }

    public static IDbConnection getDbConnection() {
        string path = GetDBPath();
        return new SQLiteConnection("data source=" + path); 
    }

}

