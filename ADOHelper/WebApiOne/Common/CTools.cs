using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace Common.Helper
{
    public static class CTools
    {
        //public static int ConvertStringToInt32(string strInt)
        //{
        //    int.TryParse(strInt, out var outResult);

        //    return outResult;
        //}

        public static bool IsContaintsItems<T>(List<T> list)
        {
            return list != null && list.Count > 0;       
        }

        /// <summary>
        /// 将modelSource字段值赋值给 modelChange字段
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="modelChange"></param>
        /// <param name="modelSource"></param>
        public static void InitUpdateModelInfo<T>(T modelChange, T modelSource)
        {
            PropertyInfo[] propsChange = modelChange.GetType().GetProperties();

            Type typeSource = modelSource.GetType();

            foreach (var propChange in propsChange)
            {
                propChange.SetValue(modelChange, typeSource.GetProperty(propChange.Name)?.GetValue(modelSource, null));
            }
        }
    }
}