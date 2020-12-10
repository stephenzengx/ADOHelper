using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Common.Helper
{
    public class LogHelper
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("LogHelper");

        public static void WriteLog(string msg)
        {
            log.Info(msg);
        }
    }
}