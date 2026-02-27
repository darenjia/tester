using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProcessEngine
{


    /// <summary>
    /// 枚举库
    /// </summary>
    public class EnumLibrary
    {
        public static string SelfPath = "";

        public static string CfgPath = "";

        public static string CANSigExam = "";

        public static string CANSigReport = "";

        public static string CANLtgExam = "";

        public static string CANLtgReport = "";

        public static string LINSigExam = "";

        public static string LINSigReport = "";

        public static string LINSigFromExam = "";

        public static string LINSigFromReport = "";

        public static string LINLtgExam = "";

        public static string LINLtgReport = "";

        public static string WifiReport = "";

        public static string WifiExam = "";

        public static string OSEKSigReport = "";

        public static string OSEKSigExam = "";

        public static string OSEKLtnReport = "";

        public static string OSEKLtnExam = "";

        public static string DTCExam = "";

        public static string DTCreport = ""; 

        public static string AutoSARNMExam = "";

        public static string AutoSARNMReport = "";

        public static string AutoSARNMLtgExam = "";

        public static string AutoSARNMLtgReport = "";

        public static string DynamicNMLtgExam = "";

        public static string DynamicNMLtgReport = "";

        public static string DynamicNMExam = "";

        public static string DynamicNMReport = "";

        public static string DynamicNMFromExam = "";

        public static string DynamicNMFromReport = "";

        public static string IndirectExam = "";

        public static string IndirectReport = "";

        public static string IndirectLtgExam = "";

        public static string IndirectLtgReport = "";



        public static string CANSig1939Report = "";

        public static string CANSig1939Exam = "";

        public static string CANLtg1939Report = "";

        public static string CANLtg1939Exam = "";

        public static string AutoSARNM1939Report = "";

        public static string AutoSARNM1939Exam = "";

        public static string AutoSARNMLtg1939Report = "";

        public static string AutoSARNMLtg1939Exam = "";

        public static string DynamicNM1939Report = "";

        public static string DynamicNM1939Exam = "";

        public static string DynamicNMFrom1939Report = "";

        public static string DynamicNMFrom1939Exam = "";

        public static string DynamicNMLtg1939Report = "";

        public static string DynamicNMLtg1939Exam = "";

        public static string Indirect1939Report = "";

        public static string Indirect1939Exam = "";

        public static string IndirectLtg1939Report = "";

        public static string IndirectLtg1939Exam = "";

        public enum EnumTable
        {
            Department,
            Employee,
            Employee_role,
            EmployeePwd,
            EmployeePhoto,
            ExapChapter,
            FaultType,
            Authorization,
            Authorization_Auth,
            Authorization_Table,
            DBC,
            DTC,
            DBCByVehicel, 
            FileLinkByVehicelEml,
            FileLinkByVehicelDouble,
            FileLinkByVehicelCount,
            FileLinkByVehicel,
            FileLinkByVehicelColEml,
            FileLinkByVehicelEml_Col,
            FileLinkByVehicelBau,
            FileLinkByVehicelBaudOnly,
            FileLinkByVehicelRep,
            FileLinkByCfg,
            Topology,
            Task,
            TaskDTC,
            TaskTable,
            TaskByCANRoad,
            TaskDBC,
            TaskTest,
            TaskUpdate,
            ConfigTemp,
            ConfigTemp_Ver,
            ExampleTemp,
            ExampleTemp_Ver,
            ExampleMatch,
            Suppliers,
            ExampleToly,
            Project,
            OperationLog,
            OperationLogToTime,
            Report,
            ReportByTask,
            LoginLog,
            LoginLogToTime,
            DbcCheck,
            FileLinkByVehicelCfg,
            FileLinkByVehicelGet,
            DeviceStatus,
            DeviceStatusCount,
            ReportUpdate,
            ReportSole,
            QuestionNote,
            QuestionNoteSort,
            QuestionNoteByTask,
            QuestionNoteByCondition,
            PassReportNote,
            PassReportNoteByCondition,
            PassReportNoteByTask,
            Segment,
            NodeConfigurationBox,
            DBCBelongCAN,
            FileLinkByVehicelEmlSort,
            ErrorInfo,
            UploadInfo,
            ExampleTempTemplate,
            DBCInformation,//Dbc信息
            TopologySelect,//拓扑图信息查询
            SegmentByName,//根据segmentName查询segment
            Peportlnfo,//报告信息查询
            Example_QueryByBusType,//查询总线对应的测试类型
            LastLoginUser
        }

        public enum EnumTemplate
        {
            ConfigTemplate,
            ExampleTemplate,
        }

        public enum TemStatus
        {
            Independence,
            Dependence
        }

        public enum EnumLog
        {
            AddVehicel,
            DelVehicel,

            Copy,
            AddTask,
            DelTask,

            AddDBC,
            DelDBC,
            Tply,
            UpdateTply,

            CfgTemplate,
            UpdateCfgTemplate,
            EmlTemplate,
            TaskEmlTemplate,
            AddSuppiers,
            DelSuppiers,
            UpdateSuppiers,

            AddEmployee,
            DelEmployee,
            UpdateEmployee,

            AddDepartment,
            DelDepartment,
            UpdateDepartment,

            AddChapter,
            DelChapter,
            UpdateChapter,

            UpdatePath,

            AddErrorType,
            DelErrorType,
            UpdateErrorType,

            AddCfgBox,
            DelCfgBox,
            UpdateCfgBox,

            AddCAN,
            DelCAN,
            UpdateCAN,

            ExportReport
        }

    }
}
