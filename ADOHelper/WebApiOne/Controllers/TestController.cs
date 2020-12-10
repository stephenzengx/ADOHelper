using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Common.Helper;
using DataAccess.DAL;
using DataAccess.Helper;
using DataAccess.Models;
using MySql.Data.MySqlClient;
using WebApiOne.Common;

namespace WebApiOne.Controllers
{
    /// <summary>
    /// mysql 测试相关接口
    /// </summary>
    [RoutePrefix("api/test")]
    public class TestController : ApiController
    {
        private readonly DataAccess.DAL.MySqlHelper _mySqlHelper = null;

        public TestController()
        {
            _mySqlHelper = new DataAccess.DAL.MySqlHelper("MySqlTestDB");
        }

        [HttpGet]
        [Route("{page}/{pageSize}")]
        public HttpResponseMessage List(int page, int pageSize,[FromUri]string name,[FromUri]int age)
        {
            var sql = "select * from tb_test where locate(@name,name)>0 and age>@age order by name,id limit @skip,@take";
            List<MySqlParameter> paramsList = new List<MySqlParameter>
            {
                new MySqlParameter("@name",name),
                new MySqlParameter("@age",age),
                new MySqlParameter("@skip",(page-1)*pageSize),
                new MySqlParameter("@take", pageSize)
            };

            List<TB_Test> list = _mySqlHelper.GetList_ExecuteSql<TB_Test>(sql, paramsList);

            return ApiHelper.GetJsonResponse(list);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("~/api/test/custom")]
        public HttpResponseMessage CustomReq()
        {
            //webapi2 路由
            //https://docs.microsoft.com/zh-cn/aspnet/web-api/overview/web-api-routing-and-actions/routing-in-aspnet-web-api
            //  ~ : 覆盖路由前缀

            //var sql = new StringBuilder();
            //var strTime = new StringBuilder();
            //Random random = new Random();
            //List<string> names = new List<string> { "Bob","Paul","Herry"};
            //Stopwatch sw = new Stopwatch();
            //for (int i = 0; i < 100; i++)
            //{
            //    sql.Clear();
            //    sql.Append("insert into tb_test(name,age) values");                
            //    for (int j = 0; j < 10000; j++)
            //    {
            //        var tmp0 = random.Next(3);
            //        var tmp1 = random.Next(10000);
            //        sql.Append(string.Format("('{0}',{1}),", names[tmp0]+tmp1,tmp1));        
            //    }
            //    sw.Restart();
            //    var result = _mySqlHelper.ExecuteScalar(sql.ToString().Substring(0, sql.Length - 1));
            //    sw.Stop();
            //    strTime.Append( (sw.ElapsedMilliseconds/1000).ToString()+",");
            //}

            //return ApiHelper.GetMsgResponse(strTime.ToString(), 0);

            return ApiHelper.GetMsgResponse("", 0);
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

            List<MySqlParameter> paramsList = new List<MySqlParameter>
            {
                new MySqlParameter("@id",id)
            };

            var modelList = _mySqlHelper.GetList_ExecuteSql<TB_Test>("select * from TB_Test where id = @id", paramsList);

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
            var sql = "insert into TB_Test(id,name)" +
                      "values (@id, @name)";

            List<MySqlParameter> paramsList = new List<MySqlParameter>
            {
                new MySqlParameter("@id",model.Id),
                new MySqlParameter("@name",model.Name)
            };
            _mySqlHelper.ExecuteSql(sql, paramsList);

            

            return ApiHelper.GetMsgResponse("count(*)："+ model, 0);
        }

        /// <summary>
        /// 修改主页信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        public HttpResponseMessage Put([FromBody] TB_Test model)
        {
            var sql = "update tb_test set name=@name where id=@id";

            List<MySqlParameter> paramsList = new List<MySqlParameter>
            {
                new MySqlParameter("@id",model.Id),
                new MySqlParameter("@name",model.Name)

            };

            _mySqlHelper.ExecuteSql(sql, paramsList);

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
            List<MySqlParameter> paramsList = new List<MySqlParameter>
            {
                new MySqlParameter("@id",id)
            };

            var isSuccess = _mySqlHelper.ExecuteSql("delete from tb_test where id = @id", paramsList);

            return ApiHelper.GetMsgResponse(isSuccess>0 ? ApiHelper.SUCCESS_DELETE: "数据不存在!", 0);
        }
    }
}
