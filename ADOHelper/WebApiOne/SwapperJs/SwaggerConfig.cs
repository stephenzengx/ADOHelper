using System.Web.Http;
using WebActivatorEx;
using Swashbuckle.Application;
using System;
using WebApiOne;

[assembly: PreApplicationStartMethod(typeof(SwaggerConfig), "Register")]
namespace WebApiOne
{
    /// <summary>
    /// swagger 配置
    /// </summary>
    public class SwaggerConfig
    {
        /// <summary>
        /// 注册服务
        /// </summary>
        public static void Register()
        {
            var thisAssembly = typeof(SwaggerConfig).Assembly;

            GlobalConfiguration.Configuration
                .EnableSwagger(c =>
                {                   
                    c.SingleApiVersion("v1", "ApiDoc");//文档名称
                    c.IncludeXmlComments(GetXmlCommentsPath());//让swagger根据xml文档来解析
                    c.CustomProvider((defaultProvider) => new SwaggerControllerDescProvider(defaultProvider, GetXmlCommentsPath()));//获取控制器的注释方法类
                    c.OperationFilter<SwaggerHeaderFilter>();
                })
                .EnableSwaggerUi(c =>
                {
                    /*
                        None = 0, 不展开
                        List = 1, 只展开操作
                        Full = 2  展开所有
                     */
                    c.DocExpansion((DocExpansion)Enum.Parse(typeof(DocExpansion), Common.ConstHelper.DocExpansionValue));                 
                    c.DocumentTitle("ApiDoc");
                    c.InjectJavaScript(System.Reflection.Assembly.GetExecutingAssembly(), "WebApiOne.Swapper.SwaggerCustom.js");//汉化js
                });
        }

        /// <summary>
        /// Gets the XML comments path.
        /// </summary>
        /// <returns>System.String.</returns>
        private static string GetXmlCommentsPath()
        {
            return $"{System.AppDomain.CurrentDomain.BaseDirectory}/bin/WebApiOne.XML";
        }
    }
}
