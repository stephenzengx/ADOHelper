using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using DataAccess.DAL;
using DataAccess.Models;

namespace DataAccess.Ado
{
    public class SqlServerTestAdo
    {
        /// <summary>
        /// 增/改
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static bool p_tb_useraccount_save(TB_UserAccount obj, SqlServerHelper db)
        {
            if (obj == null) return false;

            List<SqlParameter> parms = new List<SqlParameter>();
            parms.Add(new SqlParameter("@UserId", obj.UserId));
            parms.Add(new SqlParameter("@Age", obj.Age));
            parms.Add(new SqlParameter("@UserName", obj.UserName));

            SqlParameter pRetval = new SqlParameter("@retval", SqlDbType.Int, 4);
            pRetval.Direction = ParameterDirection.ReturnValue;
            parms.Add(pRetval);

            db.ExecuteProcedure("p_tb_test1_save", parms);

            return (pRetval.Value == DBNull.Value ? 0 : Convert.ToInt32(pRetval.Value))>0;
        }

        /// <summary>
        /// 详情 byid
        /// </summary>
        /// <param name="id"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static TB_Test p_tb_useraccount_detail(int id, SqlServerHelper db)
        {
            List<SqlParameter> parms = new List<SqlParameter>();
            parms.Add(new SqlParameter("@id", id));
            
            DataTable dt =  db.GetDataTable_Procedure("p_tb_test_detail", parms);
            if (dt==null || dt.Rows.Count <= 0)
                return null;

            var c = new TB_Test();
            c.Id = (dt.Rows[0]["id"] == DBNull.Value) ? 0 : Convert.ToInt32(dt.Rows[0]["id"]);
            c.Age = (dt.Rows[0]["age"] == DBNull.Value) ? 0 : Convert.ToInt32(dt.Rows[0]["age"].ToString());
            c.Name = (dt.Rows[0]["name"] == DBNull.Value) ? string.Empty : dt.Rows[0]["name"].ToString();

            return c;
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static bool p_tb_useraccount_delete(int id, SqlServerHelper db)
        {
            List<SqlParameter> parms = new List<SqlParameter>();
            parms.Add(new SqlParameter("@id", id));

            SqlParameter pRetval = new SqlParameter("@retval", SqlDbType.Int, 4);
            pRetval.Direction = ParameterDirection.ReturnValue;
            parms.Add(pRetval);

            db.ExecuteProcedure("p_tb_test_delete", parms);

            return (pRetval.Value == DBNull.Value ? 0 : Convert.ToInt32(pRetval.Value)) > 0;
        }

        /// <summary>
        /// list 分页
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <param name="total"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static List<TB_Test> p_tb_test_list(int page, int pagesize,string name,int age, ref int total, SqlServerHelper db)
        {
            List<SqlParameter> parms = new List<SqlParameter>();
            parms.Add(new SqlParameter("@page", page));
            parms.Add(new SqlParameter("@pagesize", pagesize));
            parms.Add(new SqlParameter("@name", name));
            parms.Add(new SqlParameter("@age", age));

            SqlParameter pTotal = new SqlParameter("@total", SqlDbType.Int, 4);
            pTotal.Direction = ParameterDirection.InputOutput;
            pTotal.Value = total;
            parms.Add(pTotal);
            
            DataTable dt =  db.GetDataTable_Procedure("p_tb_test_list", parms);

            total = (pTotal.Value == DBNull.Value) ? 0 : Convert.ToInt32(pTotal.Value);

            if (dt == null || dt.Rows.Count <= 0)
                return null;

            List<TB_Test> list = new List<TB_Test>();
            for (int i = 0; i <= dt.Rows.Count - 1; i++)
            {
                var c = new TB_Test();
                c.Id = (dt.Rows[i]["id"] == DBNull.Value) ? i : Convert.ToInt32(dt.Rows[i]["id"]);
                c.Age = (dt.Rows[i]["age"] == DBNull.Value) ? i : Convert.ToInt32(dt.Rows[i]["age"].ToString());
                c.Name = (dt.Rows[i]["name"] == DBNull.Value) ? string.Empty : dt.Rows[i]["name"].ToString();

                list.Add(c);
            }

            return list;
        }
    }
}
