using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace CANoeEngine
{
    public class ProcCANoeTest
    {
        public Dictionary<string, string> DictSelfCheck = new Dictionary<string, string>();
        public Dictionary<string, string> DictExample = new Dictionary<string, string>();
        private readonly ProcCANoe _canoe = new ProcCANoe();
        private readonly ProcSelfCheck _selfCheck = new ProcSelfCheck();
        private readonly ProcExample _example = new ProcExample();
        /// <summary>
        /// 自检工程路径
        /// </summary>
        private string _selfCheckStr;
        /// <summary>
        /// 用例工程路径
        /// </summary>
        private string _exampleStr;


        private string _exampleReportStr;

        public bool endHFlag = false;
        public bool endDFlag = false;
        public List<string> _dictExampleCache = new List<string>();

        public Dictionary<string, int> _dictNameList = new Dictionary<string, int>();
        public Dictionary<string, int> _dictNameListCopy = new Dictionary<string, int>();

        public List<string> _dictDExampleCache = new List<string>();
        public Dictionary<string, int> _dictDNameList = new Dictionary<string, int>();
        public Dictionary<string, int> _dictDNameListCopy = new Dictionary<string, int>();

        public List<string> _dictHExampleCache = new List<string>();
        public Dictionary<string, int> _dictHNameList = new Dictionary<string, int>();
        public Dictionary<string, int> _dictHNameListCopy = new Dictionary<string, int>();

        public Dictionary<string, string> _dictDeviceInfo = new Dictionary<string, string>();
        public int ExampleTestedCount = 0;
        public int ExampleAllCount = 0;
        public int ExampleAllCountCopy = 0;
        bool isFirst = true;
        private string _exampleCache = "";
        private string _exampleDCache = "";
        public bool IsEndScreen = false;
        public bool IsEndDevice = false;
        public bool IsEndPro = false;
        public int isStartDevice;
        public List<TimeSpan> span = new List<TimeSpan>();
        private Thread threadDeviceInfo;

      
        private bool IsStart = true;

        #region 状态机用变量
        private string _strStateMachinePath = string.Empty;//状态机路径
        private static System.Diagnostics.Process _StateMachineProcess;//状态机进程
        private string _StateMachineProcessName = string.Empty;//状态机进程名称
        #endregion

        public void GetName(string selfPath, string exam, string stateMachinePath)
        {
            _selfCheckStr = selfPath;
            _exampleStr = exam;
            _strStateMachinePath = stateMachinePath;
        }

        #region 状态机方法

        private void StartStateMachine()
        {
            if(string.IsNullOrEmpty(_strStateMachinePath))
                return;
            if (_StateMachineProcess == null)
            {
                _StateMachineProcess = new System.Diagnostics.Process();
                _StateMachineProcess.StartInfo.FileName = _strStateMachinePath;
                _StateMachineProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
                _StateMachineProcess.Start();
            }
            else
            {
                try
                {
                    if (_StateMachineProcess.HasExited) //是否正在运行
                    {
                        _StateMachineProcess.Start();
                    }
                }
                catch (Exception e)
                {
                }
            }
        }

        private void CloseStateMachine()
        {
            if (_StateMachineProcess == null)
            {
                //MessageBox.Show(@"未找到进程！");
                _StateMachineProcessName = string.Empty;
            }
            else
            {
                try
                {
                    if (!_StateMachineProcess.HasExited)
                    {
                        _StateMachineProcess.Kill();
                    }
                    else
                    {
                        if (_StateMachineProcessName != string.Empty)
                        {
                            System.Diagnostics.Process[] pProcess;
                            pProcess = System.Diagnostics.Process.GetProcesses();
                            for (int i = 1; i <= pProcess.Length - 1; i++)
                            {
                                if (pProcess[i].ProcessName == _StateMachineProcessName)   //任务管理器应用程序的名
                                {
                                    pProcess[i].Kill();
                                    break;
                                }
                            }
                        }
                        else
                        {
                            //MessageBox.Show(@"未找到进程！");
                        }
                    }
                }
                catch (Exception e)
                {
                }
                _StateMachineProcess = null;
                _StateMachineProcessName = string.Empty;
            }
        }

        #endregion
        #region 开始样件自检
        /// <summary>
        /// 开始样件自检
        /// </summary>
        /// <returns>返回样件自检结果</returns>
        public bool StartPrototySelfCheck()
        {
            _selfCheck.GetAllSelfCheckVar();
            //将样件自检开始标志赋值为1，即开始测试
            _selfCheck.SetSelfCheckVarValue(ProcSelfCheck.SelfEnumVar.StartPrototypeSelfCheck, 1);
            Thread thread = new Thread(ReadPrototySelfCheckResult);
            thread.Start();
            return true;
        }

        #endregion

        #region 读取样件结果
        /// <summary>
        /// 作为线程启动，循环等待读取样件自检结束标志
        /// </summary>
        private void ReadPrototySelfCheckResult()
        {
            while (true)
            {
                //读取样件自检结果标识
                int isEnd = _selfCheck.GetSelfCheckVarValue(ProcSelfCheck.SelfEnumVar.IsEndPrototySelfCheck);
                if (isEnd == 1)
                {
                    IsEndPro = true;
                    break;
                }
                Thread.Sleep(500);
            }
        }

        #endregion

        #region 开始设备自检

        public bool StartDeviceSelfCheck()
        {
            bool isExist = _canoe.IsExistConfiguration(_selfCheckStr);
            if (isExist)
            {
                if (_canoe.OpenCANoe())
                {
                    StartStateMachine();
                    _canoe.StartOrStopCaNoe();
                }
                _selfCheck._canoe = _canoe;
                _selfCheck.GetAllSelfCheckVar();
                //将设备自检开始标志赋值为1，即开始测试
                _selfCheck.SetSelfCheckVarValue(ProcSelfCheck.SelfEnumVar.StartDeviceSelfCheck, 1);
                Thread thread = new Thread(ReadDeviceSelfCheckResult);
                thread.Start();
                return true;
            }
            return false;
        }

        #endregion

        #region 读取设备结果

        /// <summary>
        ///作为线程启动，循环等待读取设备自检结束标志
        /// </summary>


        private void ReadDeviceSelfCheckResult()
        {
            while (true)
            {
                //读取样件自检结果标识
                int isEnd = _selfCheck.GetSelfCheckVarValue(ProcSelfCheck.SelfEnumVar.IsEndDeviceSelfCheck);
                if (isEnd == 1)
                {
                    IsEndDevice = true;
                    break;
                }
                Thread.Sleep(500);
            }
        }

        #endregion




        /// <summary>
        /// 得到所有用例的数量
        /// </summary>
        /// <param name="count">用例数量</param>
        public void GetAllCount(int count)
        {
            ExampleAllCount = count;
            ExampleAllCountCopy = count;
        }

        /// <summary>
        /// 开始隐性用例测试
        /// </summary>
        /// <param name="objectName">隐性用例名称</param>
        /// <param name="dtcInfo">dtc信息</param>
        /// <returns></returns>
        public int StartHExampleCheck(List<string> objectName, Dictionary<string, string> dtcInfo)
        {

            DateTime baseTime = DateTime.Now;

            List<string> nameList = objectName;
            //赋值用例名称做备份
            List<string> nameCopy = new List<string>();
            nameCopy.AddRange(nameList);
            #region MyRegion
            int nameCount = 0;
            foreach (var name in nameList)
            {
                int iTestCount = 1;
                if (name.Split('@').Length >= 2)
                {
                    int.TryParse(name.Split('@')[1], out iTestCount);
                }
                nameCount += iTestCount;
            }
            ExampleAllCount = nameCount;
            #endregion
            int isEndTest = -5;
            //检查路径
            bool isExist = _canoe.IsExistConfiguration(_exampleStr);
            if (isExist)
            {

                //打开CANoe
                //_canoe.OpenCANoe();
                //_canoe.StartOrStopCaNoe();
                _example._canoe = _canoe;
                //初始化变量
                _example.GetAllExampleVar();
                //设备自检
                threadDeviceInfo = new Thread(GetDevice);
                threadDeviceInfo.Start();
                foreach (var name in nameCopy)
                {
                    if (isFirst)
                    {
                        baseTime = DateTime.Now;
                        isFirst = false;
                        ExampleAllCountCopy = nameCount;
                    }

                    #region 拆分获取测试次数

                    int iTestCount = 1;
                    string testName = string.Empty;
                    if (name.Split('@').Length >= 2)
                    {
                        testName = name.Split('@')[0];
                        int.TryParse(name.Split('@')[1], out iTestCount);
                    }

                    #endregion

                    for (int i = 1; i <= iTestCount; i++)
                    {
                        string exampleName = testName + "@" + i;

                        if (dtcInfo.ContainsKey(testName))
                            _example.SetExampleVarValueByStr(ProcExample.EmpEnumVar.DtcTestInformation,
                                dtcInfo[testName]);
                        //向CANoe赋值用例名称
                        _example.SetExampleVarValueByStr(ProcExample.EmpEnumVar.TestScriptName, exampleName);
                        //必须暂停200毫秒，否可能造成赋值不成功。
                        Thread.Sleep(200);
                        //将开始测试标志赋值为1，代表开始执行
                        _example.SetExampleVarValue(ProcExample.EmpEnumVar.StartTest, 1);
                        Thread.Sleep(200);

                        #region 测试代码，若出现运行不了，可尝试还原

                        //Thread.Sleep(200);
                        //_example.SetExampleVarValueByStr(ProcExample.EmpEnumVar.TestscriptNameState, "Success");
                        //int ss;
                        //string dd;
                        //string qq;
                        //while (true)
                        //{
                        //    ss = _example.GetExmpVarValue(ProcExample.EmpEnumVar.StartTest);
                        //    Thread.Sleep(200);
                        //    qq = _example.GetExmpVarValueFromStr(ProcExample.EmpEnumVar.TestscriptNameState);
                        //    Thread.Sleep(200);
                        //    dd = _example.GetExmpVarValueFromStr(ProcExample.EmpEnumVar.TestScriptName);
                        //    if (ss != 1)
                        //    {
                        //        _example.SetExampleVarValue(ProcExample.EmpEnumVar.StartTest, 1);
                        //        //MessageBox.Show(ss + "赋值1结束");
                        //    }
                        //    if (dd != name)
                        //    {
                        //        _example.SetExampleVarValueByStr(ProcExample.EmpEnumVar.TestScriptName, name);
                        //        //MessageBox.Show(dd + "赋值2结束");
                        //    }
                        //    if (qq != "Success")
                        //    {
                        //        _example.SetExampleVarValueByStr(ProcExample.EmpEnumVar.TestscriptNameState, "Success");
                        //        //MessageBox.Show(qq + "赋值3结束");
                        //    }
                        //    else if (ss == 1 && dd == name && qq == "Success")
                        //        break;
                        //    Thread.Sleep(1000);
                        //}

                        #endregion

                        while (GloalVar.IsHPause)
                        {
                            //当测试过程有测试信息时，此变量更新为1
                            int isReadFlag = _example.GetExmpVarValue(ProcExample.EmpEnumVar.BufferFlag);
                            if (isReadFlag == 1)
                            {
                                //获得测试过程中的测试信息
                                string value = _example.GetExmpVarValueFromStr(ProcExample.EmpEnumVar.BufferValue);
                                //存入缓存等待界面调用显示
                                if (value != _exampleCache)
                                {
                                    if (!_dictHExampleCache.Contains(exampleName))
                                        _dictHExampleCache.Add(exampleName + "用例：" + value);
                                    _exampleCache = value;
                                }
                                //如果出现FF则返回-2 ，代表异常错误。
                                else if (value == "FF")
                                    return -2;

                                //将此变量回归为0，等待下次测试信息的获得
                                bool flag = _example.SetExampleVarValue(ProcExample.EmpEnumVar.BufferFlag, 0);
                                if (!flag)
                                    return -2;
                            }
                            //如果出现-2 ，代表异常错误。
                            else if (isReadFlag == -2)
                                return -2;

                            //获得结束标志
                            isEndTest = _example.GetExmpVarValue(ProcExample.EmpEnumVar.EndTest);
                            //如果结束标志为1，则代表此条用例测试完毕
                            if (isEndTest == 1)
                            {
                                ExampleTestedCount++;
                                //将结束标志重置为0,
                                bool endFlag = _example.SetExampleVarValue(ProcExample.EmpEnumVar.EndTest, 0);
                                //如果重置失败，返回-2，发生异常错误
                                if (!endFlag)
                                    return -2;
                                //得到用例执行结果
                                int result = _example.GetExmpVarValue(ProcExample.EmpEnumVar.TestCaseResultState);
                                //将用例结果存入缓存，准备在界面展示
                                if (!_dictHNameList.ContainsKey(exampleName))
                                    _dictHNameList.Add(exampleName, result);
                                if (!_dictHNameListCopy.ContainsKey(exampleName))
                                    _dictHNameListCopy.Add(exampleName, result);
                                TimeSpan ts = DateTime.Now - baseTime;
                                span.Add(ts);
                                baseTime = DateTime.Now;

                                bool flagH =
                                    _example.SetExampleVarValueByStr(ProcExample.EmpEnumVar.DtcTestInformation, "");
                                if (!flagH)
                                    return -2;
                                break;
                            }
                            else if (isEndTest == -2)
                                return -2;

                            Thread.Sleep(2000);
                        }

                        Thread.Sleep(1000);
                    }
                }

                GloalVar.IsHPause = false;
                endHFlag = true;
            }

            return 0;
        }

        /// <summary>
        /// 运行显性用例，且和隐性用例运行步骤完全相同
        /// </summary>
        /// <param name="objectName">用例名称列表</param>
        /// <param name="dtcInfo">dtc信息</param>
        /// <param name="isPause">前面是否运行过隐性用例</param>
        /// <returns></returns>
        public int StartDExampleCheck(List<string> objectName, Dictionary<string, string> dtcInfo, bool isPause)
        {
            DateTime baseTime = DateTime.Now;

            List<string> nameList = objectName;
            List<string> nameCopy = new List<string>();
            nameCopy.AddRange(nameList);
            #region MyRegion
            int nameCount = 0;
            foreach (var name in nameList)
            {
                int iTestCount = 1;
                if (name.Split('@').Length >= 2)
                {
                    int.TryParse(name.Split('@')[1], out iTestCount);
                }
                nameCount += iTestCount;
            }
            ExampleAllCount = nameCount;
            #endregion
            int isEndTest = -5;
            bool isExist = _canoe.IsExistConfiguration(_exampleStr);
            if (isExist)
            {
                //如果没运行过隐性用例则需要打开CANoe
                //if (!isPause)
                //{
                //    _canoe.OpenCANoe();
                //}

                //_canoe.StartOrStopCaNoe();
                _example._canoe = _canoe;
                _example.GetAllExampleVar();

                foreach (var name in nameCopy)
                {

                    if (isFirst)
                    {
                        baseTime = DateTime.Now;
                        isFirst = false;
                        ExampleAllCountCopy = nameCount;
                    }

                    #region 拆分获取测试次数

                    int iTestCount = 1;
                    string testName = string.Empty;
                    if (name.Split('@').Length >= 2)
                    {
                        testName = name.Split('@')[0];
                        int.TryParse(name.Split('@')[1], out iTestCount);
                    }

                    #endregion

                    for (int i = 1; i <= iTestCount; i++)
                    {
                        string exampleName = testName + "@" + i;


                        if (dtcInfo.ContainsKey(testName))
                            _example.SetExampleVarValueByStr(ProcExample.EmpEnumVar.DtcTestInformation,
                                dtcInfo[testName]);
                        _example.SetExampleVarValueByStr(ProcExample.EmpEnumVar.TestScriptName, exampleName);
                        Thread.Sleep(200);
                        _example.SetExampleVarValue(ProcExample.EmpEnumVar.StartTest, 1);
                        Thread.Sleep(200);
                        _example.SetExampleVarValueByStr(ProcExample.EmpEnumVar.TestscriptNameState, "Success");

                        #region 测试代码，若无法执行可尝试还原

                        //int ss;
                        //string dd;
                        //string qq;
                        //while (true)
                        //{
                        //    ss = _example.GetExmpVarValue(ProcExample.EmpEnumVar.StartTest);
                        //    Thread.Sleep(200);
                        //    qq = _example.GetExmpVarValueFromStr(ProcExample.EmpEnumVar.TestscriptNameState);
                        //    Thread.Sleep(200);
                        //    dd = _example.GetExmpVarValueFromStr(ProcExample.EmpEnumVar.TestScriptName);
                        //    if (ss != 1)
                        //    {
                        //        _example.SetExampleVarValue(ProcExample.EmpEnumVar.StartTest, 1);
                        //        //MessageBox.Show(ss + "赋值1结束");
                        //    }
                        //    if (dd != name)
                        //    {
                        //        _example.SetExampleVarValueByStr(ProcExample.EmpEnumVar.TestScriptName, name);
                        //        //MessageBox.Show(dd + "赋值2结束");
                        //    }
                        //    if (qq != "Success")
                        //    {
                        //        _example.SetExampleVarValueByStr(ProcExample.EmpEnumVar.TestscriptNameState, "Success");
                        //        //MessageBox.Show(qq + "赋值3结束");
                        //    }
                        //    else if (ss == 1 && dd == name && qq == "Success")
                        //        break;
                        //    Thread.Sleep(400);
                        //}

                        #endregion

                        while (GloalVar.IsDPause)
                        {
                            int isReadFlag = _example.GetExmpVarValue(ProcExample.EmpEnumVar.BufferFlag);
                            if (isReadFlag == 1)
                            {
                                string value = _example.GetExmpVarValueFromStr(ProcExample.EmpEnumVar.BufferValue);
                                if (value != _exampleCache)
                                {
                                    if (!_dictDExampleCache.Contains(exampleName))
                                        _dictDExampleCache.Add(exampleName + "用例：" + value);
                                    _exampleDCache = value;
                                }
                                else if (value == "FF")
                                    return -2;

                                bool flag = _example.SetExampleVarValue(ProcExample.EmpEnumVar.BufferFlag, 0);
                                if (flag == false)
                                    return -2;
                            }
                            else if (isReadFlag == -2)
                                return -2;

                            isEndTest = _example.GetExmpVarValue(ProcExample.EmpEnumVar.EndTest);

                            if (isEndTest == 1)
                            {
                                ExampleTestedCount++;
                                bool flag = _example.SetExampleVarValue(ProcExample.EmpEnumVar.EndTest, 0);
                                if (flag == false)
                                    return -2;
                                int result = _example.GetExmpVarValue(ProcExample.EmpEnumVar.TestCaseResultState);
                                //if (!_dictNameListPause.ContainsKey(name))
                                //    _dictNameListPause.Add(name, result);
                                if (!_dictDNameList.ContainsKey(exampleName))
                                    _dictDNameList.Add(exampleName, result);
                                if (!_dictDNameListCopy.ContainsKey(exampleName))
                                    _dictDNameListCopy.Add(exampleName, result);
                                Debug.Print(exampleName + "测试赋值" + result);
                                Debug.Print("_dictDNameListCopy个数=" + _dictDNameListCopy.Count);
                                TimeSpan ts = DateTime.Now - baseTime;
                                span.Add(ts);
                                baseTime = DateTime.Now;
                                bool flagD =
                                    _example.SetExampleVarValueByStr(ProcExample.EmpEnumVar.DtcTestInformation, "");
                                if (flagD == false)
                                    return -2;
                                // _example.SetExampleVarValue(ProcExample.EmpEnumVar.StartTest, 0);
                                break;
                            }
                            else if (isEndTest == -2)
                                return -2;

                            Thread.Sleep(1000);
                        }

                        Thread.Sleep(1000);
                    }
                }
                while (true)
                {
                    if (_dictDNameListCopy.Count > 0)
                    {
                        Thread.Sleep(500);
                        continue;
                    }  
                    GloalVar.IsDPause = false;
                    endDFlag = true;
                    break;
                }
                
            }
            PauseCANoe();
            //_canoe.StartOrStopCaNoe();
            Debug.Print("测试运行时的关闭CANoe，字典个数" + _dictDNameListCopy.Count);
            if (
                MessageBox.Show("是否关闭CANoe软件？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk) ==
                DialogResult.OK)
            {
                _canoe.CloseCANoe();
            }
            return 0;
        }

        public void CloseCANoe()
        {
            _canoe.CloseCANoe();
            CloseStateMachine();
        }

        public void PauseCANoe()
        {
            //_canoe.StartOrStopCaNoe();
            _canoe.PauseCANoe();
            CloseStateMachine();
        }

       /// <summary>
       /// 和CANoe交互得到设备信息并存入相应字典型数据。
       /// </summary>
        private void GetDevice()
        {
            InitDevice();
            while (true)
            {
                isStartDevice = _example.GetExmpVarValue(ProcExample.EmpEnumVar.StartDeviceInfo);
                if (isStartDevice == 1)
                {
                    string _carManufacturerECUHardwareNumber = _example.GetExmpVarValueFromStr(ProcExample.EmpEnumVar.CarManufacturerECUHardwareNumber);
                    string _carManufacturerECUSoftware = _example.GetExmpVarValueFromStr(ProcExample.EmpEnumVar.CarManufacturerECUSoftware);
                    string _ECUBatchNumber = _example.GetExmpVarValueFromStr(ProcExample.EmpEnumVar.ECUBatchNumber);
                    string _ECUManufacturingDate = _example.GetExmpVarValueFromStr(ProcExample.EmpEnumVar.ECUManufacturingDate);
                    string _softwareVersionNumber = _example.GetExmpVarValueFromStr(ProcExample.EmpEnumVar.SoftwareVersionNumber); 
                    string _sparePartsNumberOfAutomobileManufacturers = _example.GetExmpVarValueFromStr(ProcExample.EmpEnumVar.SparePartsNumberOfAutomobileManufacturers);
                    string _systemVendorECUHardwareNumber = _example.GetExmpVarValueFromStr(ProcExample.EmpEnumVar.SystemVendorECUHardwareNumber);
                    string _systemVendorECUSoftware = _example.GetExmpVarValueFromStr(ProcExample.EmpEnumVar.SystemVendorECUSoftware);
                    string _systemVendorECUSoftwareVersionNumber = _example.GetExmpVarValueFromStr(ProcExample.EmpEnumVar.SystemVendorECUSoftwareVersionNumber);
                    string _systemVendorHardwareVersionNumber = _example.GetExmpVarValueFromStr(ProcExample.EmpEnumVar.SystemVendorHardwareVersionNumber);
                    string _systemVendorNameCode = _example.GetExmpVarValueFromStr(ProcExample.EmpEnumVar.SystemVendorNameCode);
                    string _VINCode = _example.GetExmpVarValueFromStr(ProcExample.EmpEnumVar.VINCode);
                    _dictDeviceInfo["carManufacturerECUHardwareNumber"] = _carManufacturerECUHardwareNumber;
                    _dictDeviceInfo["carManufacturerECUSoftware"] = _carManufacturerECUSoftware;
                    _dictDeviceInfo["ECUBatchNumber"] = _ECUBatchNumber;
                    _dictDeviceInfo["ECUManufacturingDate"] = _ECUManufacturingDate;
                    _dictDeviceInfo["softwareVersionNumber"] = _softwareVersionNumber;
                    _dictDeviceInfo["sparePartsNumberOfAutomobileManufacturers"] = _sparePartsNumberOfAutomobileManufacturers;
                    _dictDeviceInfo["systemVendorECUHardwareNumber"] = _systemVendorECUHardwareNumber;
                    _dictDeviceInfo["systemVendorECUSoftware"] = _systemVendorECUSoftware;
                    _dictDeviceInfo["systemVendorECUSoftwareVersionNumber"] = _systemVendorECUSoftwareVersionNumber;
                    _dictDeviceInfo["systemVendorHardwareVersionNumber"] = _systemVendorHardwareVersionNumber;
                    _dictDeviceInfo["systemVendorNameCode"] = _systemVendorNameCode;
                    _dictDeviceInfo["VINCode"] = _VINCode;
                    break;
                }
                Thread.Sleep(1000);
            }
        }
        /// <summary>
        /// 初始化用于存储设备信息的字典型变量
        /// </summary>
        private void InitDevice()
        {
            if (!_dictDeviceInfo.ContainsKey("carManufacturerECUHardwareNumber"))
            {
                _dictDeviceInfo.Add("carManufacturerECUHardwareNumber", "");
                _dictDeviceInfo.Add("carManufacturerECUSoftware", "");
                _dictDeviceInfo.Add("ECUBatchNumber", "");
                _dictDeviceInfo.Add("ECUManufacturingDate", "");
                _dictDeviceInfo.Add("softwareVersionNumber", "");
                _dictDeviceInfo.Add("sparePartsNumberOfAutomobileManufacturers", "");
                _dictDeviceInfo.Add("systemVendorECUHardwareNumber", "");
                _dictDeviceInfo.Add("systemVendorECUSoftware", "");
                _dictDeviceInfo.Add("systemVendorECUSoftwareVersionNumber", "");
                _dictDeviceInfo.Add("systemVendorHardwareVersionNumber", "");
                _dictDeviceInfo.Add("systemVendorNameCode", "");
                _dictDeviceInfo.Add("VINCode", "");
            }
            
        }
    }
}
