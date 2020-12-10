using System;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Web.Configuration;
using DataAccess.Helper;

namespace DataAccess.DAL
{
    /// <summary>
    /// Mysql数据查询类
    /// </summary>
    public class MySqlHelper :IDisposable
    {
        public MySqlConnection Connection;

        /// <summary>
        /// 初始化对象打开连接 读取配置中的链接字符串
        /// </summary>
        /// <param name="connectionString"></param>
        public MySqlHelper(string connectionString)
        {
            /*
             1. 对于web应用程序而言，建议优先使用WebConfigurationManager; 但该方法不适用于客户端应用程序比如winform，WPF程序。
             2. ConfigurationManager ，即适用于web应用程序，也适用于客户端应用程序，但对于客户端应用程序来说更好。
            
              读取 web.config <appSettings> <add key="" value=""/> 节点
              ConfigurationManager.AppSettings[connectionString]
              ConfigurationManager.ConnectionStrings[connectionString]
                
              WebConfigurationManager.AppSettings[connectionString]
              WebConfigurationManager.ConnectionStrings[connectionString]
             */

            //mysql的链接字符串
            //connectionString="server=localhost;database=TestDB;User ID=root;Password=123456" />
            //connectionString="server=139.9.185.50;database=TestDB;User ID=root;Password=abc123ABC@" />	

            // 读取 web.config <connectionStrings> <add name="" connectionString=""/> 节点
            if (ConfigurationManager.ConnectionStrings[connectionString] == null)
                return;

            Open(ConfigurationManager.ConnectionStrings[connectionString].ToString());
        }

        /// <summary>
        /// 初始化对象打开连接
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="isConn">true:完整的链接字符串.  false:读取配置文件中的链接字符串，</param>
        public MySqlHelper(string connectionString, bool isConn)
        {
            if (isConn)
            {
                Open(connectionString);
                return;
            }

            Open(ConfigurationManager.AppSettings[connectionString]);
        }

        /// <summary>
        /// 主要对比看看server-sql 和mysql的链接字符串
        /// </summary>
        /// <param name="dbHost"></param>
        /// <param name="dbUser"></param>
        /// <param name="dbPassword"></param>
        /// <param name="dbName"></param>
        public MySqlHelper(string dbHost, string dbUser, string dbPassword, string dbName)
        {
            //mysql的链接字符串
            //connectionString="server=localhost;database=TestDB;User ID=root;Password=xx" />
            //connectionString="server=139.9.185.50;database=TestDB;User ID=root;Password=xx" />	
            Open(string.Concat(new string[]
            {
                "server=",
                dbHost,
                ";dbName=",
                dbName,
                ";User Id=",
                dbUser,
                ";Password=",
                dbPassword
            }));
        }

        /// <summary>
        /// 打开数据库连接
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        private void Open(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                return;

            //数据库链接已经打开 先关闭
            if (Connection != null && Connection.State == ConnectionState.Open)
                Connection.Close();

            Connection = new MySqlConnection(connectionString);
            Connection.Open();
        }

        /// <summary>
        /// 返回第一行第一列的数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public object ExecuteScalar(string sql, List<MySqlParameter> parameters = null) // where T: new()
        {
            if (Connection == null || Connection.State != ConnectionState.Open)
                return new object();

            using (MySqlCommand sqlCommand = new MySqlCommand(sql, Connection))
            {
                sqlCommand.CommandType = CommandType.Text;
                sqlCommand.Parameters.Clear();
                if (parameters != null)
                {
                    foreach (var current in parameters)
                    {
                        sqlCommand.Parameters.Add(current);
                    }
                }

                return sqlCommand.ExecuteScalar();
            }
        }

        /// <summary>
        /// 参数化执行sql  增删改
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public int ExecuteSql(string sql, List<MySqlParameter> parameters = null)
        {
            if (Connection == null || Connection.State != ConnectionState.Open)
                return -1;

            using (MySqlCommand sqlCommand = new MySqlCommand(sql, Connection))
            {
                sqlCommand.CommandType = CommandType.Text;
                sqlCommand.Parameters.Clear();

                if (parameters != null)
                {
                    foreach (var current in parameters)
                    {
                        sqlCommand.Parameters.Add(current);
                    }
                }

                return sqlCommand.ExecuteNonQuery() ;
            }
        }

        /// <summary>
        /// 执行sql 返回List T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public List<T> GetList_ExecuteSql<T>(string sql, List<MySqlParameter> parameters = null)
        {
            if (Connection == null || Connection.State != ConnectionState.Open)
                return null;

            using (MySqlCommand sqlCommand = new MySqlCommand(sql, Connection))
            {
                DataSet dataSet = new DataSet();
                DataTable dt = null;

                sqlCommand.CommandType = CommandType.Text;
                sqlCommand.Parameters.Clear();
                if (parameters != null)
                {
                    foreach (MySqlParameter current in parameters)        
                    {
                        sqlCommand.Parameters.Add(current);
                    }
                }

                using (MySqlDataAdapter sqlDataAdapter = new MySqlDataAdapter(sqlCommand))
                {
                    sqlDataAdapter.Fill(dataSet);
                    if (dataSet.Tables.Count == 1)
                    {
                        dt = dataSet.Tables[0];
                    }
                }

                return dt.ToList<T>();
            }
        }

        /// <summary>
        /// 实现IDisposable接口 释放数据库连接
        /// </summary>
        public void Dispose()
        {
            Close();
        }

        /// <summary>
        /// 关闭数据库连接/并释放改连接占用的资源
        /// </summary>
        public void Close()
        {
            if (Connection == null || Connection.State != ConnectionState.Open)
                return;

            Connection.Close();
            Connection.Dispose();
        }
    }
}