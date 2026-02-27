using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CANoe;

namespace CANoeEngine
{
    public class ProcSelfCheck
    {
        private const string CaplNamespaceName = "mutualVar";
        // CANoe对象
        private CANoe.System _mCANoeSystem;
        private CANoe.Namespaces _mCANoeNamespaces;
        private CANoe.Namespace _mCANoeNamespaceGeneral;
        private CANoe.Variables _mCANoeVariablesGeneral;
        public  ProcCANoe _canoe;

        #region  交互变量

        /// <summary>
        /// 开始硬件自检(初始值-1)
        /// </summary>
        private Variable _startDeviceSelfCheck;

        /// <summary>
        ///开始样件自检
        /// </summary>
        private CANoe.Variable _startPrototypeSelfCheck;


        /// <summary>
        /// 是否结束样件自检
        /// </summary>
        private CANoe.Variable _isEndPrototySelfCheck;

        /// <summary>
        /// 是否结束硬件自检
        /// </summary>
        private CANoe.Variable _isEndDeviceSelfCheck;

        #endregion

        #region 获得所有自检交互变量

        /// <summary>
        /// 获得所有自检交互变量
        /// </summary>
        public void GetAllSelfCheckVar()
        {
            //CANoe对象初始化且获得系统变量所在的命名空间。
            _mCANoeSystem = (CANoe.System) _canoe._mCANoeApp.System;
            _mCANoeNamespaces = (CANoe.Namespaces) _mCANoeSystem.Namespaces;
            _mCANoeNamespaceGeneral = (CANoe.Namespace) _mCANoeNamespaces[CaplNamespaceName];
            _mCANoeVariablesGeneral = (CANoe.Variables) _mCANoeNamespaceGeneral.Variables;

            //得到变量初始值，此过程必须存在，否则无法赋值
            _startDeviceSelfCheck = (CANoe.Variable) _mCANoeVariablesGeneral["startDeviceSelfCheck"];
            _startPrototypeSelfCheck = (CANoe.Variable) _mCANoeVariablesGeneral["startPrototypeSelfCheck"];
            _isEndPrototySelfCheck = (CANoe.Variable) _mCANoeVariablesGeneral["isEndPrototySelfCheck"];
            _isEndDeviceSelfCheck = (CANoe.Variable) _mCANoeVariablesGeneral["isEndDeviceSelfCheck"];
        }

        #endregion

        #region 获得某个变量值

        /// <summary>
        /// 获得指定变量值
        /// </summary>
        /// <param name="enumVar">通过枚举找到相应变量</param>
        /// <returns></returns>
        public int GetSelfCheckVarValue(SelfEnumVar enumVar)
        {
            switch (enumVar)
            {
                case SelfEnumVar.StartPrototypeSelfCheck:
                    _startPrototypeSelfCheck = (Variable) _mCANoeVariablesGeneral["startPrototypeSelfCheck"];
                    return _startPrototypeSelfCheck.Value;
                case SelfEnumVar.StartDeviceSelfCheck:
                    _startDeviceSelfCheck = (Variable) _mCANoeVariablesGeneral["startDeviceSelfCheck"];
                    return _startDeviceSelfCheck.Value;
                case SelfEnumVar.OscillographCheck:
                case SelfEnumVar.IsEndPrototySelfCheck:
                    _isEndPrototySelfCheck = (Variable) _mCANoeVariablesGeneral["isEndPrototySelfCheck"];
                    return _isEndPrototySelfCheck.Value;
                case SelfEnumVar.IsEndDeviceSelfCheck:
                    _isEndDeviceSelfCheck = (Variable) _mCANoeVariablesGeneral["isEndDeviceSelfCheck"];
                    return _isEndDeviceSelfCheck.Value;
                default:
                    throw new ArgumentOutOfRangeException(nameof(enumVar), enumVar, null);
            }
        }

        #endregion

        #region 设置某个变量值

        /// <summary>
        /// 设置某个变量值
        /// </summary>
        /// <param name="enumVar">通过枚举找到变量</param>
        /// <param name="varValue">设置值内容，仅限int</param>
        /// <returns></returns>
        public bool SetSelfCheckVarValue(SelfEnumVar enumVar, int varValue)
        {
            switch (enumVar)
            {
                case SelfEnumVar.StartDeviceSelfCheck:
                    _startDeviceSelfCheck.Value = varValue;
                    return true;
                case SelfEnumVar.StartPrototypeSelfCheck:
                    _startPrototypeSelfCheck.Value = varValue;
                    return true;
                case SelfEnumVar.IsEndDeviceSelfCheck:
                    _isEndDeviceSelfCheck.Value = varValue;
                    return true;
                case SelfEnumVar.IsEndPrototySelfCheck:
                    _isEndPrototySelfCheck.Value = varValue;
                    return true;
                default:
                    throw new ArgumentOutOfRangeException(nameof(enumVar), enumVar, null);
            }
        }
        #endregion

        #region 自检系统变量枚举数列

        /// <summary>
        /// 系统变量枚举数列
        /// </summary>
        public enum SelfEnumVar
        {
            StartDeviceSelfCheck,
            PowerSupplyCheck,
            OscillographCheck,
            DigitalMultimeterCheck,
            PaNPowerSupplyCheck,

            StartPrototypeSelfCheck,
            IsDBCDepended,
            ExplicitOrImplicit,
            SendMessage,

            IsEndPrototySelfCheck,
            IsEndDeviceSelfCheck,

        }

        #endregion
    }
}
