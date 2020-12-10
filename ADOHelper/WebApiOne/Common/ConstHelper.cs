using System;
using System.Collections.Generic;
using System.Configuration;

namespace WebApiOne.Common
{
    public class ConstHelper
    {
        public static readonly string BaseDIR = AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// swagger 展示配置  None:0 不展开 / List:1 只展开操作 / Full:2 展开所有
        /// </summary>
        public static string DocExpansionValue
        {
            get
            {
                var _DocExpansion = ConfigurationManager.AppSettings["DocExpansion"];

                return _DocExpansion == null ? "1" : _DocExpansion;
            }
        }

        /// <summary>
        /// 测试环境
        /// </summary>
        public static bool IsStage
        {
            get
            {
                var modeSetting = ConfigurationManager.AppSettings["Mode"];
                return modeSetting != null && modeSetting.ToString().Equals("Stage", StringComparison.CurrentCultureIgnoreCase);
            }
        }

        /// <summary>
        /// 生产环境
        /// </summary>
        public static bool IsProduct
        {
            get
            {
                var modeSetting = ConfigurationManager.AppSettings["Mode"];
                return modeSetting != null && modeSetting.ToString().Equals("Product", StringComparison.CurrentCultureIgnoreCase);
            }
        }
    }
}