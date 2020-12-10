using System.Web.Http;
using WebActivatorEx;
using Swashbuckle.Application;
using System;
using WebApiOne;

[assembly: PreApplicationStartMethod(typeof(SwaggerConfig), "Register")]
namespace WebApiOne
{
    /// <summary>
    /// swagger ����
    /// </summary>
    public class SwaggerConfig
    {
        /// <summary>
        /// ע�����
        /// </summary>
        public static void Register()
        {
            var thisAssembly = typeof(SwaggerConfig).Assembly;

            GlobalConfiguration.Configuration
                .EnableSwagger(c =>
                {                   
                    c.SingleApiVersion("v1", "ApiDoc");//�ĵ�����
                    c.IncludeXmlComments(GetXmlCommentsPath());//��swagger����xml�ĵ�������
                    c.CustomProvider((defaultProvider) => new SwaggerControllerDescProvider(defaultProvider, GetXmlCommentsPath()));//��ȡ��������ע�ͷ�����
                    c.OperationFilter<SwaggerHeaderFilter>();
                })
                .EnableSwaggerUi(c =>
                {
                    /*
                        None = 0, ��չ��
                        List = 1, ֻչ������
                        Full = 2  չ������
                     */
                    c.DocExpansion((DocExpansion)Enum.Parse(typeof(DocExpansion), Common.ConstHelper.DocExpansionValue));                 
                    c.DocumentTitle("ApiDoc");
                    c.InjectJavaScript(System.Reflection.Assembly.GetExecutingAssembly(), "WebApiOne.Swapper.SwaggerCustom.js");//����js
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
