using System;
using System.Web.Http.Filters;
using Common.Helper;
using WebApiOne.Common;

namespace WebApiOne
{
    /// <summary>
    /// 全局异常过滤器
    /// </summary>
    public class GlobalExceptionFilter : ExceptionFilterAttribute
    {
        /// <summary>
        /// 重写异常拦截方法
        /// </summary>
        /// <param name="actionExecutedContext"></param>
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            Exception e = actionExecutedContext.Exception;
            LogHelper.WriteLog( e.StackTrace + "////" + e.Message);
            if (e.InnerException != null)
            {
                LogHelper.WriteLog("e.InnerException：" + e.InnerException.Message);
            }

            if (ConstHelper.IsStage)
            {
                actionExecutedContext.Response = ApiHelper.GetMsgResponse(e.Message+"," +(e.InnerException!=null? e.InnerException.Message:""));
            }
            else
            {
                actionExecutedContext.Response = ApiHelper.GetMsgResponse("系统错误");
            }
        }
    }
}