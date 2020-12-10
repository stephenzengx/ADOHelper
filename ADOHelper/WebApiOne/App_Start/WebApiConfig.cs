using System.Web.Http;
using WebApiOne.MsgHandlers;

namespace WebApiOne
{
    /// <summary>
    /// webapi 路由配置
    /// </summary>
    public static class WebApiConfig
    {
        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="config"></param>
        public static void Register(HttpConfiguration config)
        {

            //全局异常配置
            config.Filters.Add(new GlobalExceptionFilter());
            //事件处理程序
            config.MessageHandlers.Add(new TokenHandler());

            // 启动属性路由
            config.MapHttpAttributeRoutes();
            //创建默认路由
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
