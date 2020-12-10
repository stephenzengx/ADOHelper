using DataAccess.Ado;
using DataAccess.DAL;
using DataAccess.Models;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using WebApiOne.Common;

namespace WebApiOne.Controllers
{
    /// <summary>
    /// mssql相关接口
    /// </summary>
    [RoutePrefix("api/mstest")]
    public class MsTestController : ApiController
    {
        private readonly SqlServerHelper _sqlServerHelper = null;

        public MsTestController()
        {
            _sqlServerHelper = new DataAccess.DAL.SqlServerHelper("SqlServerTestDB");
        }

        /// <summary>
        /// 获取所有数据
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK,
        "devinfo:设备信息<br/>" +
        "systemName:系统类型<br/>" +
        "typeName:设备类型<br/>" +
        "devStatus:运行状态<br/>" +
        "measureName:所在区域<br/>" +
        "meterDatas:设备仪表数据<br/>"
        , Type = typeof(List<string>))]
        [HttpGet]
        public HttpResponseMessage List()
        {
            List<TB_Test> list = _sqlServerHelper.GetList_ExecuteSql<TB_Test>("select * from tb_test");

            return ApiHelper.GetJsonResponse(list);
        }

        [HttpGet]
        [Route("{page}/{pageSize}")]
        public HttpResponseMessage ListPage(int page, int pageSize, [FromUri] string name, [FromUri] int age)
        {            
            int total = 0;
            //List<TB_Test> list1 = SqlServerTestAdo.p_tb_test_list(page, pageSize, name, age, ref total, _sqlServerHelper);
            List<SqlParameter> parms = new List<SqlParameter>();
            parms.Add(new SqlParameter("@page", page));
            parms.Add(new SqlParameter("@pagesize", pageSize));
            parms.Add(new SqlParameter("@name", name));
            parms.Add(new SqlParameter("@age", age));

            List<TB_Test> list = _sqlServerHelper.GetList_Procedure<TB_Test>("p_tb_test_list", parms);
            /*msserver 存储过程分页原理 
             * 例如:page 3,pageSize 20 排好序取出前60条,然后倒序后取前20条，然后再排好序输出，很巧妙;
            /*
            select a.* from (      
            select top  2  b.name,b.age,b.id from(      
            select top 20 b.name,b.age,b.id  from      
                tb_test  b  where 1=1  and age> 10 order by b.name asc,b.age asc) b       
            order by  b.name desc,b.age desc) b inner join tb_test  a on a.id = b.id       
            order by b.name asc,b.age asc
             */

            return ApiHelper.GetJsonResponse(list);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("~/api/mstest/custom")]
        public HttpResponseMessage CustomReq()
        {
            return ApiHelper.GetMsgResponse("",0);

            //webapi2 路由
            //https://docs.microsoft.com/zh-cn/aspnet/web-api/overview/web-api-routing-and-actions/routing-in-aspnet-web-api
            //  ~ : 覆盖路由前缀

            var sql = new StringBuilder();
            var strTime = new StringBuilder();
            Random random = new Random();
            List<string> names = new List<string> { "Bob", "Paul", "Herry" };
            Stopwatch sw = new Stopwatch();
            for (int i = 0; i < 1; i++)
            {
                sql.Clear();
                sql.Append("insert into tb_test(name,age) values");
                for (int j = 0; j < 999; j++)
                {
                    var tmp0 = random.Next(3);
                    var tmp1 = random.Next(10000);
                    var tmp2 = random.Next(100);        
                    sql.Append(string.Format("('{0}',{1}),", names[tmp0] + tmp1, tmp2));
                }
                sw.Restart();
                var result = _sqlServerHelper.ExecuteSql(sql.ToString().Substring(0, sql.Length - 1));
                sw.Stop();
                strTime.Append(sw.ElapsedMilliseconds.ToString() + ",");
            }

            return ApiHelper.GetMsgResponse(strTime.ToString(), 0);           
        }

        /// <summary>
        /// 获取列表单行
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}")]
        public HttpResponseMessage Detail(int id)
        {
            //[Route("indexget/{id}")] 
            //属性路由(自定义路由):  http://localhost:52298/indexget/1(未加前缀)   http://localhost:52298/Index/indexget/1(加了前缀)
            //未自定义默认路 :http://localhost:52298/api/Index?id=1 <=> http://localhost:52298/api/Index/1

            List<SqlParameter> paramsList = new List<SqlParameter>
            {
                new SqlParameter("@id",id)
            };

            var modelList = _sqlServerHelper.GetList_ExecuteSql<TB_Test>("select * from tb_test where id = @id", paramsList);

            return ApiHelper.GetJsonResponse(modelList);
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage Post([FromBody] TB_Test model)
        {
            var sql = "insert into tb_test(name,age)" +
                      "values (@name, @age)";

            List<SqlParameter> paramsList = new List<SqlParameter>
            {
                new SqlParameter("@name",model.Name),
                new SqlParameter("@age",model.Age)
            };
            int infectRows = _sqlServerHelper.ExecuteSql(sql, paramsList);

            return ApiHelper.GetMsgResponse("infectRows：" + infectRows);
        }

        /// <summary>
        /// 修改主页信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        public HttpResponseMessage Put([FromBody] TB_Test model)
        {
            var sql = "update tb_test set name=@name,age=@age where id=@id";

            List<SqlParameter> paramsList = new List<SqlParameter>
            {
                new SqlParameter("@id",model.Id),
                new SqlParameter("@name",model.Name),
                new SqlParameter("@age",model.Age)
            };

            _sqlServerHelper.ExecuteSql(sql, paramsList);

            return ApiHelper.GetJsonResponse(model);
        }

        /// <summary>
        /// 删除主页信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{id}")]
        public HttpResponseMessage DeletebyId(int id)
        {
            List<SqlParameter> paramsList = new List<SqlParameter>
            {
                new SqlParameter("@id",id)
            };

            var isSuccess = _sqlServerHelper.ExecuteSql("delete from tb_test where id = @id", paramsList);

            return ApiHelper.GetMsgResponse(isSuccess>0 ? ApiHelper.SUCCESS_DELETE : "数据不存在!");
        }
    }
}
