using Common.Helper;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using WebApiOne.Common;

namespace WebApiOne.MsgHandlers
{
    /// <summary>
    /// http消息处理程序
    /// </summary>
    public class TokenHandler : DelegatingHandler
    {
        /// <summary>
        /// 消息处理程序 方法
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.RequestUri.AbsoluteUri.Contains("swagger"))
                {
                    return await base.SendAsync(request, cancellationToken);
                }

                //预请求放行
                if (request.Method == HttpMethod.Options)
                {
                    HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Headers.Add("Access-Control-Allow-Origin", "*");
                    response.Headers.Add("Access-Control-Allow-Methods", "GET,POST,PATCH,DELETE,PUT,OPTIONS");
                    //response.Headers.Add("Access-Control-Request-Methods", "GET, POST, OPTIONS");
                    response.Headers.Add("Access-Control-Allow-Headers", "*");
                    response.Headers.Add("Access-Control-Allow-Credentials", "true");

                    return response;
                }

                HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");               
                //HttpContext.Current.Response.AddHeader("Access-Control-Request-Methods", "GET, POST, OPTIONS");
                HttpContext.Current.Response.AddHeader("Access-Control-Allow-Methods", "GET,POST,PATCH,DELETE,PUT,OPTIONS");
                HttpContext.Current.Response.AddHeader("Access-Control-Allow-Headers", "*");
                HttpContext.Current.Response.AddHeader("Access-Control-Allow-Credentials", "true");

                IEnumerable<string> authHeads = null;
                if (!request.Headers.TryGetValues("Authorization", out authHeads))
                    return await Task.Factory.StartNew(() =>
                    {
                        return ApiHelper.GetErrorResponse("token expired, relogin",-2, HttpStatusCode.Unauthorized);
                    });
                var token = string.Empty;
                //to do 验证token
                request.Properties.Add("token",token);

                return await base.SendAsync(request, cancellationToken);
            }
            catch (System.Exception e)
            {
                LogHelper.WriteLog(e.Source + " // " + e.StackTrace + " // " + e.Message);
                if (e.InnerException != null)
                    LogHelper.WriteLog(e.InnerException.Message);

                return ApiHelper.GetErrorResponse(ConstHelper.IsStage ? e.Message : ApiHelper.EXCEPTION_REQUEST, -1);
            }                                             
        }
    }
}