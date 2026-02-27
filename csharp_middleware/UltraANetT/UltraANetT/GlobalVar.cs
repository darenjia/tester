using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using DevExpress.XtraNavBar;
using DevExpress.XtraEditors;
using System.Windows.Forms;

namespace UltraANetT
{
    public class GlobalVar
    {
        /// <summary>
        /// xml存放地址
        /// </summary>

        public static string TreeXmlPath = @"xml\Tree.xml";

        public static string UserName = "张康达";

        public static string UserRole = "";

        public static string UserNo = "No.002";

        public static string UserDept = "开发部";

        public static string ModuleJson = "";

        public static string LoginNo = string.Empty;

        public static bool IsUpload = false;
        public static Dictionary<string,object> ReportCopy = new Dictionary<string, object>();
        public static Dictionary<string, List<string>> ErrorInfo = new Dictionary<string, List<string>>();
        /// <summary>
        /// 当前节点及其父节点的名称集合
        /// </summary>
        public static List<string> CurrentVNode = new List<string>();
        /// <summary>
        /// 判断当测试集成时，选择节点配置盒的操作是否正确
        /// </summary>
        public static bool isGetSlaveBoxID = false;
        /// <summary>
        /// 当选择集成测试时，保存所选的节点配置盒的数据信息
        /// </summary>
        public static Dictionary<string, string> dictSlaveBoxID = new Dictionary<string, string>();
        
        /// <summary>
        /// 
        /// </summary>
        public static string TemporaryFilePath = AppDomain.CurrentDomain.BaseDirectory + @"temporary\";
        /// <summary>
        /// 
        /// </summary>
        public static string TestFilePath = AppDomain.CurrentDomain.BaseDirectory + @"xml\";

        //public static List<string> CurrentTkNode = new List<string>();

        public static List<string> CurrentTsNode = new List<string>();
        public static string VehicelNode = "";
        public static NavBarItem OldBarItem = null;

        public static IList<object[]> ListCfgTemp = new List<object[]>();

        public static List<Dictionary<string,object>> DictCfgTemp = new List<Dictionary<string, object>>();

        public static Dictionary<string,string> DictManualReport = new Dictionary<string, string>();

        public static Dictionary<string, List<List<string>>> AutoReport = new Dictionary<string, List<List<string>>>();

        #region Excel报告相关
        public static Dictionary<string, string> ExcelCoverReport = new Dictionary<string, string>();
        public static Dictionary<string, List<List<string>>> ExcelReport = new Dictionary<string, List<List<string>>>();
        public static Dictionary<string, string> ExcelReportPath = new Dictionary<string, string>();
        #endregion

        public static List<Dictionary<string, List<List<string>>>> AutoReportList = new List<Dictionary<string, List<List<string>>>>();
        public  static List<string> RenameList = new List<string>(); 
        public static string EmlName;
        public static string ConName;
        public static bool UserLogin = false;
        public static bool UpDbcOk = false;
        public static bool UpOk = false;
        public static bool UpTopOk = false;
        public static bool IsMaual = false;

        public static int NumberChanges = 0;//页面切换 内容编辑判定
        /// <summary>
        ///     页面切换 要有提示，是否保留当前的操作内容公共执行方法
        /// </summary>
        public static Boolean WhetherPerform()
        {
            var KF = false;
            if (GlobalVar.NumberChanges != 0)
            {
                if (XtraMessageBox.Show("检测到当前数据未保存！是否切换页面?", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                {
                    KF = true;
                    GlobalVar.NumberChanges = 0;
                }
            }
            else { KF = true; }
            return KF;
        }
    }
}