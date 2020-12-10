using System.Collections.Generic;
using System.Net.Http;
using System.Web;
using System.Web.Http.Filters;
using System.Web.Script.Serialization;
using WebApiOne.Common;

namespace WebApiOne
{
    /// <inheritdoc />
    public class ActionFilter : ActionFilterAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        public static readonly string KeyToken = "token";

        /// <summary>
        /// 
        /// </summary>
        public static string ConttpyForm = "application/x-www-form-unlencoded";
        /// <summary>
        /// 
        /// </summary>
        public static string ConttpyJson = "application/json";

        private static JavaScriptSerializer _javaScriptSerializer = new JavaScriptSerializer();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuted(HttpActionExecutedContext context)
        {
            //在传统的MVC中  获得控制器名和方法名
            //string controllerName = (string)filterContext.RouteData.Values["controller"];
            //string actionName = (string)filterContext.RouteData.Values["action"];
            //string ClientIp = GetClientIp();

            //webapi获得控制器名和方法名     
            //通过拿到controller和actionname可以进行精确到按钮级别的权限控制 关联用户角色或者具体的用户ID (后台弄一个配置表)      
            // 可将配置一次性放入iis缓存 或者 redis 更新配置更新数据库 以及(更新对应的缓存)
            //string controllerName  = context.ActionContext.ControllerContext.ControllerDescriptor.ControllerName;
            //string actionName = context.ActionContext.ActionDescriptor.ActionName;
            //string portName = context.Request.RequestUri.AbsolutePath; 
            
            //根据
        }

        /// <summary>
        /// 获取客户端Ip
        /// </summary>
        /// <returns></returns>
        public string GetClientIp()
        {
            string result = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(result))
            {
                result = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }
            if (string.IsNullOrEmpty(result))
            {
                result = HttpContext.Current.Request.UserHostAddress;
            }
            if (string.IsNullOrEmpty(result))
            {
                return "0.0.0.0";
            }
            return result;
        }
    }
}