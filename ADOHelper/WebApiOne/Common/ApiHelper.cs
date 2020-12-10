using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WebApiOne.Common
{
    public class ApiHelper
    {
        internal class ResultClass
        {
            public int State { get; set; }

            public string Message { get; set; }

            public string ErrorMessage { get; set; }

            public object Record { get; set; }            
        }

        #region tips
        public static readonly string SUCCESS_ADD = "新增成功";
        public static readonly string SUCCESS_UPDATE = "更新成功";
        public static readonly string SUCCESS_DELETE = "删除成功";
        public static readonly string SUCCESS_BIND = "绑定成功";
        public static readonly string SUCCESS_UNBIND = "解绑成功";
        public static readonly string ERROR_PARAM = "参数错误";
        public static readonly string ERROR_NOTFOUND = "数据不存在,刷新页面重试";
        public static readonly string ERROR_DUPLICATED_BIND = "重复绑定,请检查";
        public static readonly string EXCEPTION_REQUEST = "请求异常,请检查";
        #endregion

        public static JsonSerializerSettings serialSetting;
        
        //静态代码块
        static ApiHelper()
        {
            serialSetting = GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings;
            serialSetting.DateTimeZoneHandling = DateTimeZoneHandling.Local;     
            //serialSetting.ContractResolver = new CamelCasePropertyNamesContractResolver();//驼峰式
            serialSetting.DateFormatString = "yyyy-MM-dd HH:mm:ss";
        }

        public static HttpResponseMessage GetJsonResponse(object obj,string message="操作成功")
        {
            return new HttpResponseMessage
            {
                Content = new StringContent(
                        JsonConvert.SerializeObject(new ResultClass {
                            State = 0,
                            Message = message,
                            Record = obj                          
                        }, serialSetting),
                        System.Text.Encoding.GetEncoding("UTF-8"),
                        "application/json")
            };
        }

        public static HttpResponseMessage SuccessAddResponse(string id)
        {
            return GetJsonResponse(id,SUCCESS_ADD);
        }
        public static HttpResponseMessage SuccessUpdateResponse()
        {
            return GetMsgResponse(SUCCESS_UPDATE);
        }
        public static HttpResponseMessage SuccessDeleteResponse()
        {
            return GetMsgResponse(SUCCESS_DELETE);
        }


        public static HttpResponseMessage ErrorParamResponse(string apppend="")
        {
            return GetErrorResponse(ERROR_PARAM+apppend);
        }

        public static HttpResponseMessage ErrorNotFoundResponse()
        {
            return GetErrorResponse(ERROR_NOTFOUND);
        }

        public static HttpResponseMessage GetErrorResponse(string errMessage = "系统异常", int state=-1,  HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            return new HttpResponseMessage
            {
                Content = new StringContent(
                        JsonConvert.SerializeObject(new ResultClass
                        {
                            State = state,
                            ErrorMessage = errMessage
                        }, serialSetting),
                        System.Text.Encoding.GetEncoding("UTF-8"),
                        "application/json"),
                StatusCode = statusCode
            };
        }

        public static HttpResponseMessage GetMsgResponse(string message = "操作成功", int state = 0)
        {
            return new HttpResponseMessage
            {
                Content = new StringContent(
                        JsonConvert.SerializeObject(new ResultClass
                        {
                            State = state,
                            Message = message
                        }, serialSetting),
                        System.Text.Encoding.GetEncoding("UTF-8"),
                        "application/json")
            };
        }
    }
}