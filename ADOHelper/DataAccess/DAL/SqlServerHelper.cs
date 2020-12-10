using DataAccess.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace DataAccess.DAL
{
    /*
        Database: 在连接打开之后获取当前数据库的名称，或者在连接打开之前获取连接字符串中指定的数据库名。
        DataSource: 获取要连接的数据库服务器的名称。
        ConnectionTimeOut: 获取在建立连接时终止尝试并生成错误之前所等待的时间。
        ConnectionString: 获取或设置用于打开连接的字符串。
        State: 获取描述连接状态的字符串。

        ConnectionStringBuilder和CommandBuilder
        db.Connection.Database;
        db.Connection.DataSource;
        db.Connection.ConnectionString

        db.Connection.State:枚举
            Closed: 连接处于关闭状态。
            Open: 连接处于打开状态。
            Connecting: 连接对象正在与数据源连接。
            Executing: 连接对象正在执行命令。
            Fetching: 连接对象正在检索数据。
            Broken: 与数据源的连接中断。

        using 创建资源 所做的一些工作
            注意：using中生成的资源对象 必须要实现IDisposable 接口
            分配资源。
            把Statement放进try块。
            在finally中 执行资源的Dispose方法，

     连接池的行为可以通过连接字符串来控制，主要包括四个重要的属性：
        Connection Timeout：连接请求等待超时时间。默认为15秒，单位为秒。
        Max Pool Size: 连接池中最大连接数。默认为100。
        Min Pool Size: 连接池中最小连接数。默认为0。
        Pooling: 是否启用连接池。ADO.NET默认是启用连接池的，因此，你需要手动设置Pooling=false来禁用连接池。
     
        SqlConnectionStringBuilder connStr = new SqlConnectionStringBuilder();
        connStr.DataSource = @".\SQLEXPRESS";
        connStr.InitialCatalog = "master";
        connStr.IntegratedSecurity = true; connStr.Pooling = true; //开启连接池
        connStr.MinPoolSize = 0; //设置最小连接数为0
        connStr.MaxPoolSize = 50; //设置最大连接数为50
        connStr.ConnectTimeout = 10; //设置超时时间为10秒
        
        
        SqlCommand(ExecuteNonQuery,SqlDataAdapter, ExecuteScalar)                    
            ExecuteNonQuery: 增删改 返回影响行数
            SqlDataAdapter(SqlCommand) -> SqlDataAdapter.Fill(dataSet):数据适配器,返回DateTable
            ExecuteScalar: 返回单值,用于比如 select count(*)
            ExecuteReader: 执行查询，并返回一个 DataReader 对象。（用的相对较少）
     */

    public class SqlServerHelper : IDisposable
    {
        //注：数据库连接在调用处关
        public SqlConnection Connection;

        /// <summary>
        /// 初始化对象打开连接 读取配置中的链接字符串
        /// </summary>
        /// <param name="connectionString"></param>
        public SqlServerHelper(string connectionString)
        {
            if (ConfigurationManager.ConnectionStrings[connectionString] == null)
                return;

            Open(ConfigurationManager.ConnectionStrings[connectionString].ToString());           
        }

        /// <summary>
        /// 初始化对象打开连接
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="isConn">true:完整的链接字符串.  false:读取配置文件中的链接字符串，</param>
        public SqlServerHelper(string connectionString, bool isConn)
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
        public SqlServerHelper(string dbHost, string dbUser, string dbPassword, string dbName)
        {
            // sql server 的连接字符串
            // "Data Source=localhost;User Id=sa; Password=1996crimdeath; Initial Catalog=TestDB;Persist Security Info=no;Integrated Security=no;"
            Open(string.Concat(new string[]
            {
                "Data Source=",
                dbHost,
                ";User Id=",
                dbUser,
                ";Password=",
                dbPassword,
                ";Initial Catalog=",
                dbName,
                ";Persist Security Info=no;Integrated Security=no;"
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
            {
                return;
            }
            //数据库链接已经打开 先关闭
            if (Connection != null && Connection.State == ConnectionState.Open)
            {
                Connection.Close();
            }

            Connection = new SqlConnection(connectionString);
            Connection.Open();
        }

        /// <summary>
        /// 返回单值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public object ExecuteScalar(string sql, List<SqlParameter> parameters = null) // where T: new()
        {
            if (Connection == null || Connection.State != ConnectionState.Open)
                return new object();

            using (SqlCommand sqlCommand = new SqlCommand(sql, Connection))
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

                return  sqlCommand.ExecuteScalar();
            }
        }

        /// <summary>
        /// 参数化执行sql  增删改
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public int ExecuteSql(string sql, List<SqlParameter> parameters = null)
        {
            if (Connection == null || Connection.State != ConnectionState.Open)
                return -1;

            using (SqlCommand sqlCommand = new SqlCommand(sql, Connection))
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

                return sqlCommand.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 执行sql 返回List T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public List<T> GetList_ExecuteSql<T>(string sql, List<SqlParameter> parameters = null)
        {
            if (Connection == null || Connection.State != ConnectionState.Open)
                return null;

            using (SqlCommand sqlCommand = new SqlCommand(sql, Connection))
            {
                DataSet dataSet = new DataSet();
                DataTable dt = null;

                sqlCommand.CommandType = CommandType.Text;
                sqlCommand.Parameters.Clear();
                if (parameters != null)
                {
                    foreach (SqlParameter current in parameters)
                    {
                        sqlCommand.Parameters.Add(current);
                    }
                }

                using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand))
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
        /// 执行sql 返回DataTable
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DataTable GetDataTable_ExecuteSql(string sql, List<SqlParameter> parameters = null)
        {
            if (Connection == null || Connection.State != ConnectionState.Open)
                return null;

            using (SqlCommand sqlCommand = new SqlCommand(sql, Connection))
            {
                DataSet dataSet = new DataSet();
                DataTable dt = null;

                sqlCommand.CommandType = CommandType.Text;
                sqlCommand.Parameters.Clear();
                if (parameters != null)
                {
                    foreach (SqlParameter current in parameters)
                    {
                        sqlCommand.Parameters.Add(current);
                    }
                }

                using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand))
                {
                    sqlDataAdapter.Fill(dataSet);
                    if (dataSet.Tables.Count == 1)
                    {
                        dt = dataSet.Tables[0];
                    }
                }

                return dt;
            }
        }

        /// <summary>
        /// 执行存储过程 无返回值
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="parameters"></param>
        public void ExecuteProcedure(string procedureName, List<SqlParameter> parameters = null)
        {
            if (Connection == null || Connection.State != ConnectionState.Open)
                return;

            using (SqlCommand sqlCommand = new SqlCommand(procedureName, Connection))
            {
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Parameters.Clear();
                if (parameters != null)
                {
                    foreach (var current in parameters)
                    {
                        sqlCommand.Parameters.Add(current);
                    }
                }

                sqlCommand.Parameters.Add(new SqlParameter("ReturnValue", SqlDbType.Int, 4,
                    ParameterDirection.ReturnValue, false, 0, 0, string.Empty, DataRowVersion.Default, null));
                sqlCommand.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 执行存储过程 返回List T
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public List<T> GetList_Procedure<T>(string procedureName, List<SqlParameter> parameters = null)
        {
            if (Connection == null || Connection.State != ConnectionState.Open)
                return null;

            using (SqlCommand sqlCommand = new SqlCommand(procedureName, Connection))
            {
                DataSet dataSet = new DataSet();
                DataTable dt = null;

                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Parameters.Clear();
                sqlCommand.Parameters.Add(new SqlParameter("ReturnValue", SqlDbType.Int, 4, ParameterDirection.ReturnValue, false, 0, 0, string.Empty, DataRowVersion.Default, null));
                if (parameters != null)
                {
                    foreach (SqlParameter current in parameters)
                    {
                        sqlCommand.Parameters.Add(current);
                    }
                }

                using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand))
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
        /// 执行存储过程 返回DataTable
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DataTable GetDataTable_Procedure(string procedureName, List<SqlParameter> parameters = null)
        {
            if (Connection == null || Connection.State != ConnectionState.Open)
                return null;

            using (SqlCommand sqlCommand = new SqlCommand(procedureName, Connection))
            {
                DataSet dataSet = new DataSet();
                DataTable dt = null;

                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Parameters.Clear();
                sqlCommand.Parameters.Add(new SqlParameter("ReturnValue", SqlDbType.Int, 4, ParameterDirection.ReturnValue, false, 0, 0, string.Empty, DataRowVersion.Default, null));
                if (parameters != null)
                {
                    foreach (SqlParameter current in parameters)
                    {
                        sqlCommand.Parameters.Add(current);
                    }
                }

                using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand))
                {
                    sqlDataAdapter.Fill(dataSet);
                    if (dataSet.Tables.Count == 1)
                    {
                        dt = dataSet.Tables[0];
                    }
                }

                return dt;
            }
        }

        /// <summary>
        /// 执行存储过程 返回DataSet
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DataSet GetDataSet_Procedure(string procedureName, List<SqlParameter> parameters=null)
        {
            if (Connection == null || Connection.State != ConnectionState.Open)
                return null;

            using (SqlCommand sqlCommand = new SqlCommand(procedureName, Connection))
            {
                DataSet dataSet = new DataSet();

                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Parameters.Clear();
                sqlCommand.Parameters.Add(new SqlParameter("ReturnValue", SqlDbType.Int, 4, ParameterDirection.ReturnValue, false, 0, 0, string.Empty, DataRowVersion.Default, null));
                if (parameters != null)
                {
                    foreach (SqlParameter current in parameters)
                    {
                        sqlCommand.Parameters.Add(current);
                    }
                }

                using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand))
                {
                    sqlDataAdapter.Fill(dataSet);

                }

                return dataSet;
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