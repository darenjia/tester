using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace ReportEditor
{
    public class GlobalVar
    {

        public static string ImageFilePath = AppDomain.CurrentDomain.BaseDirectory + @"screenshot\";

        public static string ReportPath = "";
        #region 报告中固定项全局变量
        /// <summary>
        /// 拓扑图
        /// </summary>
        public static List<object> imgTply = new List<object>();
        /// <summary>
        /// 拓扑图引用
        /// </summary>
        public static string Reference = "";
        /// <summary>
        /// 报告标题
        /// </summary>
        public static string ReportTitle = "";
        /// <summary>
        /// 
        /// </summary>
        public static List<int> TestItem = new List<int>();
        /// <summary>
        /// 
        /// </summary>
        public static Dictionary<string, string> ReportTestOrder = new Dictionary<string, string>();

        /// <summary>
        /// 以Dict形式存储用例表的模板信息
        /// </summary>
        public static List<Dictionary<string, object>> DictReportTemp = new List<Dictionary<string, object>>();

        /// <summary>
        /// 
        /// </summary>
        public static IList<object[]> ReportCache = new List<object[]>();

        public static bool isRun = false;

        #endregion
    }
}
