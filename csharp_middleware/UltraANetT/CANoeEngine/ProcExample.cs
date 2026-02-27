using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CANoe;

namespace CANoeEngine
{
    public class ProcExample
    {
        #region 常规变量
        private const string CaplNamespaceNameD = "DeviceInfo";
        private const string CaplNamespaceNameM = "mutualVar";
        // CANoe对象
        private CANoe.System _mCANoeSystem;
        private CANoe.Namespaces _mCANoeNamespaces;
        private CANoe.Namespace _mCANoeNamespaceGeneral_D;
        private CANoe.Variables _mCANoeVariablesGeneral_D;
        private CANoe.Namespace _mCANoeNamespaceGeneral_M;
        private CANoe.Variables _mCANoeVariablesGeneral_M;
        public ProcCANoe _canoe;
        #endregion

        #region 交互变量
        /// <summary>
        /// 用例编码
        /// </summary>
        private CANoe.Variable _startTest;

        private CANoe.Variable _testscriptNameState;
        /// <summary>
        /// 执行过程中提示
        /// </summary>
        private CANoe.Variable _testScriptName;
        /// <summary>
        /// 执行过程中提示
        /// </summary>
        private CANoe.Variable _endTest;
        /// <summary>
        /// 执行过程中提示
        /// </summary>
        private CANoe.Variable _bufferValue;
        /// <summary>
        /// 执行过程中提示
        /// </summary>
        private CANoe.Variable _bufferFlag;
        /// <summary>
        /// 执行过程中提示
        /// </summary>
        private CANoe.Variable _testCaseResultState;
        
        /// <summary>
        /// 执行过程中提示
        /// </summary>
        private CANoe.Variable _startScreenshot;
        /// <summary>
        /// 执行过程中提示
        /// </summary>
        private CANoe.Variable _endScreenshot;
        /// <summary>
        /// Log
        /// </summary>
        private CANoe.Variable _logPath;
        /// <summary>
        /// 0xF191:汽车制造商ECU硬件号
        /// </summary>
        private CANoe.Variable _carManufacturerECUHardwareNumber;
        /// <summary>
        /// 0xF188:汽车制造商ECU软件号
        /// </summary>
        private CANoe.Variable _carManufacturerECUSoftware;
        /// <summary>
        /// ECU批次编号
        /// </summary>
        private CANoe.Variable _ECUBatchNumber;
        /// <summary>
        /// ECU制造日期
        /// </summary>
        private CANoe.Variable _ECUManufacturingDate;
        /// <summary>
        /// 汽车制造商软件版本号
        /// </summary>
        private CANoe.Variable _softwareVersionNumber;
        /// <summary>
        /// 汽车制造商备用零部件编号
        /// </summary>
        private CANoe.Variable _sparePartsNumberOfAutomobileManufacturers;
        /// <summary>
        /// 系统供应商ECU硬件号
        /// </summary>
        private CANoe.Variable _systemVendorECUHardwareNumber;
        /// <summary>
        /// 系统供应商ECU软件号
        /// </summary>
        private CANoe.Variable _systemVendorECUSoftware;
        /// <summary>
        /// 系统供应商ECU软件版本号
        /// </summary>
        private CANoe.Variable _systemVendorECUSoftwareVersionNumber;
        /// <summary>
        /// 系统供应商硬件版本号
        /// </summary>
        private CANoe.Variable _systemVendorHardwareVersionNumber;
        /// <summary>
        /// 系统供应商公司名称代码
        /// </summary>
        private CANoe.Variable _systemVendorNameCode;
        /// <summary>
        /// VIN码
        /// </summary>
        private CANoe.Variable _VINCode;
        /// <summary>
        /// 开始执行标志
        /// </summary>
        private CANoe.Variable _startDeviceInfo;

        private CANoe.Variable _runEmptyInstryction;

        private CANoe.Variable _dtcTestInformation;
        #endregion

        #region 初始化用例变量
        /// <summary>
        /// 初始化用例变量（同自检）
        /// </summary>
        public void GetAllExampleVar()
        {
            _mCANoeSystem = (CANoe.System)_canoe._mCANoeApp.System;
            _mCANoeNamespaces = (CANoe.Namespaces)_mCANoeSystem.Namespaces;
            _mCANoeNamespaceGeneral_D = (CANoe.Namespace)_mCANoeNamespaces[CaplNamespaceNameD];
            _mCANoeNamespaceGeneral_M = (CANoe.Namespace)_mCANoeNamespaces[CaplNamespaceNameM];
            _mCANoeVariablesGeneral_D = (CANoe.Variables)_mCANoeNamespaceGeneral_D.Variables;
            _mCANoeVariablesGeneral_M = (CANoe.Variables)_mCANoeNamespaceGeneral_M.Variables;

            _startTest = (CANoe.Variable)_mCANoeVariablesGeneral_M["startTest"];
            _testscriptNameState = (CANoe.Variable)_mCANoeVariablesGeneral_M["testscriptNameState"];
            _testScriptName = (CANoe.Variable)_mCANoeVariablesGeneral_M["testScriptName"];
            _endTest = (CANoe.Variable)_mCANoeVariablesGeneral_M["endTest"];
            _bufferValue = (CANoe.Variable)_mCANoeVariablesGeneral_M["bufferValue"];
            _bufferFlag = (CANoe.Variable)_mCANoeVariablesGeneral_M["bufferFlag"];
            _testCaseResultState = (CANoe.Variable)_mCANoeVariablesGeneral_M["testCaseResultState"];


            _carManufacturerECUHardwareNumber = _mCANoeVariablesGeneral_D["carManufacturerECUHardwareNumber"];
            _carManufacturerECUSoftware = _mCANoeVariablesGeneral_D["carManufacturerECUSoftware"];
            _ECUBatchNumber = _mCANoeVariablesGeneral_D["ECUBatchNumber"];
            _ECUManufacturingDate = _mCANoeVariablesGeneral_D["ECUManufacturingDate"];
            _softwareVersionNumber = _mCANoeVariablesGeneral_D["softwareVersionNumber"];
            _sparePartsNumberOfAutomobileManufacturers = _mCANoeVariablesGeneral_D["sparePartsNumberOfAutomobileManufacturers"];
            _systemVendorECUHardwareNumber = _mCANoeVariablesGeneral_D["systemVendorECUHardwareNumber"];
            _systemVendorECUSoftware = _mCANoeVariablesGeneral_D["systemVendorECUSoftware"];
            _systemVendorECUSoftwareVersionNumber = _mCANoeVariablesGeneral_D["systemVendorECUSoftwareVersionNumber"];
            _systemVendorHardwareVersionNumber = _mCANoeVariablesGeneral_D["systemVendorHardwareVersionNumber"];
            _systemVendorNameCode = _mCANoeVariablesGeneral_D["systemVendorNameCode"];
            _VINCode = _mCANoeVariablesGeneral_D["VINCode"];
            _startDeviceInfo = _mCANoeVariablesGeneral_D["startDeviceInfo"];
            _runEmptyInstryction = _mCANoeVariablesGeneral_M["runEmptyInstryction"];
            _dtcTestInformation = _mCANoeVariablesGeneral_M["dtcTestInformation"];
        }
        #endregion


        #region 获得指定用例变量
        /// <summary>
        /// 获得指定用例变量
        /// </summary>
        /// <param name="enumVar">枚举</param>
        /// <returns>得到的用例变量的值</returns>
        public int GetExmpVarValue(EmpEnumVar enumVar)
        {
            try
            {
                switch (enumVar)
                {
                    case EmpEnumVar.StartTest:
                        _startTest = (Variable)_mCANoeVariablesGeneral_M["startTest"];
                        return _startTest.Value;
                  
                    case EmpEnumVar.EndTest:
                        _endTest = (Variable)_mCANoeVariablesGeneral_M["endTest"];
                        return _endTest.Value;
                    case EmpEnumVar.BufferFlag:
                        _bufferFlag = (Variable)_mCANoeVariablesGeneral_M["bufferFlag"];
                        return _bufferFlag.Value;
                    case EmpEnumVar.TestCaseResultState:
                        _testCaseResultState = (Variable)_mCANoeVariablesGeneral_M["testCaseResultState"];
                        return _testCaseResultState.Value;


                    case EmpEnumVar.StartDeviceInfo:
                        _startDeviceInfo = (Variable)_mCANoeVariablesGeneral_D["startDeviceInfo"];
                        return _startDeviceInfo.Value;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(enumVar), enumVar, null);
                }
            }
            catch (Exception ex)
            {
                return -2;
            }
        }
        #endregion

        #region 获得指定用例变量
        public string GetExmpVarValueFromStr(EmpEnumVar enumVar)
        {
            try
            {
                switch (enumVar)
                {
                    case EmpEnumVar.TestScriptName:
                        _testScriptName = (Variable) _mCANoeVariablesGeneral_M["testScriptName"];
                        return _testScriptName.Value;
                    case EmpEnumVar.TestscriptNameState:
                        _testscriptNameState = (Variable)_mCANoeVariablesGeneral_M["testscriptNameState"];
                        return _testscriptNameState.Value;
                    case EmpEnumVar.BufferValue:
                        _bufferValue = (Variable) _mCANoeVariablesGeneral_M["bufferValue"];
                        return _bufferValue.Value;
                    case EmpEnumVar.CarManufacturerECUHardwareNumber:
                        _carManufacturerECUHardwareNumber =
                            (Variable) _mCANoeVariablesGeneral_D["carManufacturerECUHardwareNumber"];
                        return _carManufacturerECUHardwareNumber.Value;
                    case EmpEnumVar.CarManufacturerECUSoftware:
                        _carManufacturerECUSoftware = (Variable) _mCANoeVariablesGeneral_D["carManufacturerECUSoftware"];
                        return _carManufacturerECUSoftware.Value;
                    case EmpEnumVar.ECUBatchNumber:
                        _ECUBatchNumber = (Variable) _mCANoeVariablesGeneral_D["ECUBatchNumber"];
                        return _ECUBatchNumber.Value;
                    case EmpEnumVar.ECUManufacturingDate:
                        _ECUManufacturingDate = (Variable) _mCANoeVariablesGeneral_D["ECUManufacturingDate"];
                        return _ECUManufacturingDate.Value;
                    case EmpEnumVar.SoftwareVersionNumber:
                        _softwareVersionNumber = (Variable) _mCANoeVariablesGeneral_D["softwareVersionNumber"];
                        return _softwareVersionNumber.Value;
                    case EmpEnumVar.SparePartsNumberOfAutomobileManufacturers:
                        _sparePartsNumberOfAutomobileManufacturers =
                            (Variable) _mCANoeVariablesGeneral_D["sparePartsNumberOfAutomobileManufacturers"];
                        return _sparePartsNumberOfAutomobileManufacturers.Value;
                    case EmpEnumVar.SystemVendorECUHardwareNumber:
                        _systemVendorECUHardwareNumber =
                            (Variable) _mCANoeVariablesGeneral_D["systemVendorECUHardwareNumber"];
                        return _systemVendorECUHardwareNumber.Value;
                    case EmpEnumVar.SystemVendorECUSoftware:
                        _systemVendorECUSoftware = (Variable) _mCANoeVariablesGeneral_D["systemVendorECUSoftware"];
                        return _systemVendorECUSoftware.Value;
                    case EmpEnumVar.SystemVendorECUSoftwareVersionNumber:
                        _systemVendorECUSoftwareVersionNumber =
                            (Variable) _mCANoeVariablesGeneral_D["systemVendorECUSoftwareVersionNumber"];
                        return _systemVendorECUSoftwareVersionNumber.Value;
                    case EmpEnumVar.SystemVendorHardwareVersionNumber:
                        _systemVendorHardwareVersionNumber =
                            (Variable) _mCANoeVariablesGeneral_D["systemVendorHardwareVersionNumber"];
                        return _systemVendorHardwareVersionNumber.Value;
                    case EmpEnumVar.SystemVendorNameCode:
                        _systemVendorNameCode = (Variable) _mCANoeVariablesGeneral_D["systemVendorECUSoftware"];
                        return _systemVendorNameCode.Value;
                    case EmpEnumVar.VINCode:
                        _VINCode = (Variable) _mCANoeVariablesGeneral_D["VINCode"];
                        return _VINCode.Value;
                    case EmpEnumVar.RunEmptyInstryction:

                        _runEmptyInstryction = _mCANoeVariablesGeneral_M["runEmptyInstryction"];
                        return _runEmptyInstryction.Value;
                    case EmpEnumVar.DtcTestInformation:
                        _dtcTestInformation = _mCANoeVariablesGeneral_M["_dtcNeedInformation"];
                        return _dtcTestInformation.Value;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(enumVar), enumVar, null);
                }
            }
            catch (Exception ex)
            {
                return "FF";
            }

        }
        #endregion

        #region 设置某个变量值

        /// <summary>
        /// 
        /// </summary>
        /// <param name="enumVar"></param>
        /// <param name="varValue"></param>
        /// <returns></returns>
        public bool SetExampleVarValue(EmpEnumVar enumVar, int varValue)
        {
            try
            {
                switch (enumVar)
                {
                    case EmpEnumVar.StartTest:
                        _startTest.Value = varValue;
                        return true;
                    
                    case EmpEnumVar.EndTest:
                        _endTest.Value = varValue;
                        return true;
                    case EmpEnumVar.BufferFlag:
                        _bufferFlag.Value = varValue;
                        return true;
                    case EmpEnumVar.TestCaseResultState:
                        _testCaseResultState.Value = varValue;
                        return true;


                        return true;
                    case EmpEnumVar.LogPath:
                        _logPath.Value = varValue;
                        return true;
                    case EmpEnumVar.StartDeviceInfo:
                        _startDeviceInfo.Value = varValue;
                        return true;
                    case EmpEnumVar.RunEmptyInstryction:
                        _runEmptyInstryction.Value = varValue;
                        return true;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(enumVar), enumVar, null);
                }
            }
            catch (Exception)
            {

                return false;
            }
           
        }
        #endregion

        #region 设置某个变量值

        /// <summary>
        /// 设置某个变量值
        /// </summary>
        /// <param name="enumVar">枚举</param>
        /// <param name="varValue">赋值内容</param>
        /// <returns></returns>
        public bool SetExampleVarValueByStr(EmpEnumVar enumVar, string varValue)
        {
            try
            {
                switch (enumVar)
                {
                    case EmpEnumVar.TestScriptName:
                        _testScriptName.Value = varValue;
                        return true;
                    case EmpEnumVar.TestscriptNameState:
                        _testscriptNameState.Value = varValue;
                        return true;
                    case EmpEnumVar.BufferValue:
                        _bufferValue.Value = varValue;
                        return true;
                    case EmpEnumVar.DtcTestInformation:
                        _dtcTestInformation.Value = varValue;
                        return true;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(enumVar), enumVar, null);
                }
            }
            catch (Exception)
            {

                return false;
            }
          
        }
        #endregion

        #region 用例变量枚举数列

        /// <summary>
        /// 系统变量枚举数列
        /// </summary>
        public enum EmpEnumVar
        {
            StartTest,
            TestscriptNameState,
            TestScriptName,
            EndTest,
            BufferValue,
            BufferFlag,
            LogPath,
            TestCaseResultState,

            CarManufacturerECUHardwareNumber,
            CarManufacturerECUSoftware,
            ECUBatchNumber,
            ECUManufacturingDate,
            SoftwareVersionNumber,
            SparePartsNumberOfAutomobileManufacturers,
            SystemVendorECUHardwareNumber,
            SystemVendorECUSoftware,
            SystemVendorECUSoftwareVersionNumber,
            SystemVendorHardwareVersionNumber,
            SystemVendorNameCode,
            VINCode,
            StartDeviceInfo,
            RunEmptyInstryction,
            DtcTestInformation
        }

        #endregion
    }
}
