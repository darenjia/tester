using System;
using System.Collections.Generic;
using FileEditor.Control;

namespace FileEditor.pubClass
{
    public class GlobalVar
    {
        public static bool UserLogin = false;
        public static string UserName = "";
        public static string UserRole = "";
        public static string UserNo = "";
        public static string UserDept = "";
        public static string ModuleJson = "";
        public static string EmlTemplateJson = "";
        public static string EmlTemplateColJson = "";
        public static string JsonTxtPath = @"temporary\配置表列编辑.txt";
        public static Dictionary<string, Dictionary<string, string>> DictdictCfgJson =
            new Dictionary<string, Dictionary<string, string>>();
        /// <summary>
        /// 
        /// </summary>
        public static bool IsCfgDisable = false;
        /// <summary>
        /// 
        /// </summary>
        public static bool IsEmpDisable = false;
        /// <summary>
        /// 
        /// </summary>
        public static bool IsProjectDisable = false;
        /// <summary>
        /// 
        /// </summary>
        public static bool IsColumEdit = false;

        /// <summary>
        /// 
        /// </summary>
        public static string SelectName = "";
        public static bool IsIndependent = true;
        /// <summary>
        /// 
        /// </summary>
        public static bool IsOnlyEml  = true;
        /// <summary>
        /// 
        /// </summary>
        public static int CANSignalCfgMergedCol = 6;
        /// <summary>
        /// 
        /// </summary>
        public static int CANSignalExmpMergedCol = 14;
        public static Dictionary<string,string> dictAdd = new Dictionary<string, string>();
        public static string strEvent;
        /// <summary>
        /// 车型
        /// </summary>
        public static string TaskNo;
        /// <summary>
        /// 任务轮数
        /// </summary>
        public static string TaskRound;
        /// <summary>
        /// 用例相关的Json存放
        /// </summary>
        public static string DictAssessItem = "";
        /// <summary>
        /// 用例表相关的GridView的Json
        /// </summary>
        public static List<Dictionary<string, object>> DictAssessGrid = new List<Dictionary<string, object>>();
        /// <summary>
        /// 任务名称
        /// </summary>
        public static string TaskName;
        /// <summary>
        /// 网段
        /// </summary>
        public static string CANRoad;
        /// <summary>
        /// 模块
        /// </summary>
        public static string Module;
        /// <summary>
        /// 总线类型
        /// </summary>
        public static string MatchSort;
        /// <summary>
        /// txtBelongNode.里面的
        /// </summary>
        public static string TextBelongModule;
        /// <summary>
        /// 改动或增加的一列
        /// </summary>
        public static List<object> ChangList = new List<object>();
        /// <summary>
        /// 
        /// </summary>
        public static List<string> VNode = new List<string>();
        /// <summary>
        /// 
        /// </summary>
        public static string TemporaryFilePath = AppDomain.CurrentDomain.BaseDirectory + @"temporary\";
        /// <summary>
        /// 
        /// </summary>
        public static string TemporaryEmltxtPath = AppDomain.CurrentDomain.BaseDirectory + @"temporary\";
        /// <summary>
        /// 
        /// </summary>
        public static Dictionary<string, List<object>> ListDrScheme = new Dictionary<string, List<object>>();
        /// <summary>
        /// 
        /// </summary>
        public static List<string> ListCAN = new List<string>();
        /// <summary>
        /// 以Dict形式存储配置表的模板信息
        /// </summary>
        public static List<Dictionary<string,object>> DictCfgTemp = new List<Dictionary<string, object>>();
        /// <summary>
        /// 以Dict形式存储用例表的模板信息
        /// </summary>
        public static List<Dictionary<string, object>> DictEmpTemp = new List<Dictionary<string, object>>();
        /// <summary>
        /// 配置表的模板信息
        /// </summary>
        public static IList<object[]> ListCfgTemp = new List<object[]>();
        public static string VersionCfgTem;
        public static string VersionEmlTem;
        /// <summary>
        /// 用例表的模板信息
        /// </summary>
        public static IList<object[]> ListEmpTemp = new List<object[]>();
        /// <summary>
        /// 
        /// </summary>
        public static Dictionary<string,string[]>GetNodeFromDBC = new Dictionary<string, string[]>();
        /// <summary>
        /// 
        /// </summary>
        public static List<Dictionary<string, string[]>> GetNodeList = new List<Dictionary<string, string[]>>();
        /// <summary>
        /// 
        /// </summary>
        public static Dictionary<string, List<Dictionary<string,List<Dictionary<string, string>>>>> ExamCache = new Dictionary<string, List<Dictionary<string, List<Dictionary<string, string>>>>>();
        /// <summary>
        /// 
        /// </summary>
        public static Dictionary<string,byte[]> BytesCfgTemplate = new Dictionary<string, byte[]>();
        /// <summary>
        /// 配置表点击左侧树的Item名称
        /// </summary>
        public static string CfgClickItemName = "";
        /// <summary>
        ///  用例表点击左侧树的Item名称
        /// </summary>
        public static string EmlClickItemName = "";

        public static byte[] BytesEml;

        public static CfgTemplate CfgCache;

        public static EmlTemplate EmlCache;
        public static bool CfgTemplateIsOk = false;
        public static bool EmlTemplateIsOk = false;

        public static bool isRun = false;
    }
}