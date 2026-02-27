using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using CANoe;
using Encoding = System.Text.Encoding;

namespace CANoeEngine
{
    public class ProcCANoe
    {
        #region 常规变量

        // 工程文件的绝对路径
        private String _absoluteConfigurationPath = "";
        // CANoe对象
        public CANoe.Application _mCANoeApp;
        public CANoe.Measurement _mCANoeMeasurement;
        #endregion

        #region 是否存在此工程文件

        /// <summary>
        /// 
        /// </summary>
        /// <param name="absolutePath"></param>
        /// <returns></returns>
        public bool IsExistConfiguration(string absolutePath)
        {
            if (File.Exists(absolutePath))
            {
                _absoluteConfigurationPath = absolutePath;
                return true;
            }
            return false;

        }

        #endregion

        #region 打开CANoe

        /// <summary>
        /// 打开CANoe软件，如果再运行，则会关闭当前运行状态。
        /// </summary>
        /// <returns></returns>
        public bool OpenCANoe()
        {
          
            _mCANoeApp = new Application();
            // 初始化CANoe对象
            _mCANoeMeasurement = (Measurement)_mCANoeApp.Measurement;

            // Stopps a running measurement.
            if (_mCANoeMeasurement.Running)
                _mCANoeMeasurement.Stop();

            if (_mCANoeApp != null)
            {
                // 打开指定路径.cfg文件。
                _mCANoeApp.Open(_absoluteConfigurationPath, true, true);

                // 确保文件正常运行
                var ocresult = _mCANoeApp.configuration.OpenConfigurationResult;
                if (ocresult.result != 0) return false;

                return true;
            }
            else
                return false;
        }

        #endregion

        #region 开始运行或停止运行CANoe

        /// <summary>
        /// 开始运行或停止运行CANoe
        /// </summary>
        /// <returns>0:停止CANoe；1：启动CANoe；2：找不到CANoe对象-1：其他异常。</returns>
        public int StartOrStopCaNoe()
        {
            try
            {
                if (_mCANoeMeasurement == null) return 2;
                if (_mCANoeMeasurement.Running)
                {
                    _mCANoeMeasurement.Stop();
                    return 0;
                }
                _mCANoeMeasurement.Start();
                return 1;
            }
            catch(Exception ex)
            {
                return -1;
            }
        }

        public bool PauseCANoe()
        {
            try
            {
                if (_mCANoeMeasurement == null) return true;
                if (_mCANoeMeasurement.Running)
                {
                    _mCANoeMeasurement.Stop();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion

        #region 关闭CANoe

        /// <summary>
        ///  关闭CANoe
        /// </summary>
        /// <returns>true:关闭成功；false:关闭失败   </returns>
        public bool CloseCANoe()
        {
            try
            {
                if (_mCANoeApp != null)
                {
                    if (_mCANoeMeasurement != null)
                    {
                        if (_mCANoeMeasurement.Running)
                            _mCANoeMeasurement.Stop();
                    }
                    _mCANoeApp.Quit();
                }
                else
                    return false;
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion
       
    }
}
