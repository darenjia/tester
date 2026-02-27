using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraBars.Docking;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Columns;
using ProcessEngine;
using UltraANetT.Interface;

namespace UltraANetT.Module
{
    public partial class FilePathCfg : XtraUserControl
    {
        ProcFile _file = new ProcFile();

        public FilePathCfg()
        {
            InitializeComponent();
            initInformation();
        }

        public void initInformation()
        {
            string firstStr = _file.ReadLocalXml(@"xml\path.xml", @"Product/IsFirst");
            bool isFirst = bool.Parse(firstStr);
            if (isFirst)
            {
                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/SelfPath") == "无")
                    btnSelfCheck.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/SelfPath");
                else
                    btnSelfCheck.Text = AppDomain.CurrentDomain.BaseDirectory +
                                        _file.ReadLocalXml(@"xml\path.xml", @"Product/SelfPath");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/CfgPath") == "无")
                    btncfgExm.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/CfgPath");
                else
                    btncfgExm.Text = AppDomain.CurrentDomain.BaseDirectory +
                                     _file.ReadLocalXml(@"xml\path.xml", @"Product/CfgPath");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/CANSigExam") == "无")
                    btnCANSigExam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/CANSigExam");
                else
                    btnCANSigExam.Text = AppDomain.CurrentDomain.BaseDirectory +
                                         _file.ReadLocalXml(@"xml\path.xml", @"Product/CANSigExam");


                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/CANSigReport") == "无")
                    btnCANSigReport.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/CANSigReport");
                else
                    btnCANSigReport.Text = AppDomain.CurrentDomain.BaseDirectory +
                                           _file.ReadLocalXml(@"xml\path.xml", @"Product/CANSigReport");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/CANLtgExam") == "无")
                    btnCANLtgExam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/CANLtgExam");
                else
                    btnCANLtgExam.Text = AppDomain.CurrentDomain.BaseDirectory +
                                         _file.ReadLocalXml(@"xml\path.xml", @"Product/CANLtgExam");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/CANLtgReport") == "无")
                    btnCANLtgReport.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/CANLtgReport");
                else
                    btnCANLtgReport.Text = AppDomain.CurrentDomain.BaseDirectory +
                                           _file.ReadLocalXml(@"xml\path.xml", @"Product/CANLtgReport");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/LINSigExam") == "无")
                    btnLINSigExam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/LINSigExam");
                else
                    btnLINSigExam.Text = AppDomain.CurrentDomain.BaseDirectory +
                                         _file.ReadLocalXml(@"xml\path.xml", @"Product/LINSigExam");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/LINSigReport") == "无")
                    btnLinSigReport.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/LINSigReport");
                else
                    btnLinSigReport.Text = AppDomain.CurrentDomain.BaseDirectory +
                                           _file.ReadLocalXml(@"xml\path.xml", @"Product/LINSigReport");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/LINLtgExam") == "无")
                    btnLINLtgExam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/LINLtgExam");
                else
                    btnLINLtgExam.Text = AppDomain.CurrentDomain.BaseDirectory +
                                         _file.ReadLocalXml(@"xml\path.xml", @"Product/LINLtgExam");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/LINLtgReport") == "无")
                    btnLINLtgReport.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/LINLtgReport");
                else
                    btnLINLtgReport.Text = AppDomain.CurrentDomain.BaseDirectory +
                                           _file.ReadLocalXml(@"xml\path.xml", @"Product/LINLtgReport");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/WifiReport") == "无")
                    btnWifiReport.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/WifiReport");
                else
                    btnWifiReport.Text = AppDomain.CurrentDomain.BaseDirectory +
                                          _file.ReadLocalXml(@"xml\path.xml", @"Product/WifiReport");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/WifiExam") == "无")
                    btnWifiExam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/WifiExam");
                else
                    btnWifiExam.Text = AppDomain.CurrentDomain.BaseDirectory +
                                       _file.ReadLocalXml(@"xml\path.xml", @"Product/WifiExam");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/OSEKSigReport") == "无")
                    btnOSEKSigReport.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/OSEKSigReport");
                else
                    btnOSEKSigReport.Text = AppDomain.CurrentDomain.BaseDirectory +
                                            _file.ReadLocalXml(@"xml\path.xml", @"Product/OSEKSigReport");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/OSEKSigExam") == "无")
                    btnOSEKSigExam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/OSEKSigExam");
                else
                    btnOSEKSigExam.Text = AppDomain.CurrentDomain.BaseDirectory +
                                          _file.ReadLocalXml(@"xml\path.xml", @"Product/OSEKSigExam");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/OSEKLtnReport") == "无")
                    btnOSEKLtnReport.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/OSEKLtnReport");
                else
                    btnOSEKLtnReport.Text = AppDomain.CurrentDomain.BaseDirectory +
                                            _file.ReadLocalXml(@"xml\path.xml", @"Product/OSEKLtnReport");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/OSEKLtnExam") == "无")
                    btnOSEKLtnExam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/OSEKLtnExam");
                else
                    btnOSEKLtnExam.Text = AppDomain.CurrentDomain.BaseDirectory +
                                          _file.ReadLocalXml(@"xml\path.xml", @"Product/OSEKLtnExam");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/DTCExam") == "无")
                    btnDTCExam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/DTCExam");
                else
                    btnDTCExam.Text = AppDomain.CurrentDomain.BaseDirectory +
                                      _file.ReadLocalXml(@"xml\path.xml", @"Product/DTCExam");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/DTCreport") == "无")
                    btnDTCreport.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/DTCreport");
                else
                    btnDTCreport.Text = AppDomain.CurrentDomain.BaseDirectory +
                                        _file.ReadLocalXml(@"xml\path.xml", @"Product/DTCreport");

                //新 
                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/AutoSARNMExam") == "无")
                    btnAutoSARNMExam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/AutoSARNMExam");
                else
                    btnAutoSARNMExam.Text = AppDomain.CurrentDomain.BaseDirectory +
                                            _file.ReadLocalXml(@"xml\path.xml", @"Product/AutoSARNMExam");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/AutoSARNMReport") == "无")
                    btnAutoSARNMReport.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/AutoSARNMReport");
                else
                    btnAutoSARNMReport.Text = AppDomain.CurrentDomain.BaseDirectory +
                                              _file.ReadLocalXml(@"xml\path.xml", @"Product/AutoSARNMReport");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/AutoSARNMLtgExam") == "无")
                    btnAutoSARNMLtgExam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/AutoSARNMLtgExam");
                else
                    btnAutoSARNMLtgExam.Text = AppDomain.CurrentDomain.BaseDirectory +
                                               _file.ReadLocalXml(@"xml\path.xml", @"Product/AutoSARNMLtgExam");


                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/AutoSARNMLtgReport") == "无")
                    btnAutoSARNMLtgReport.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/AutoSARNMLtgReport");
                else
                    btnAutoSARNMLtgReport.Text = AppDomain.CurrentDomain.BaseDirectory +
                                                 _file.ReadLocalXml(@"xml\path.xml", @"Product/AutoSARNMLtgReport");


                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMExam") == "无")
                    btnDynamicNMExam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMExam");
                else
                    btnDynamicNMExam.Text = AppDomain.CurrentDomain.BaseDirectory +
                                            _file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMExam");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMReport") == "无")
                    btnDynamicNMReport.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMReport");
                else
                    btnDynamicNMReport.Text = AppDomain.CurrentDomain.BaseDirectory +
                                              _file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMReport");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMFromExam") == "无")
                    btnDynamicNMFromExam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMFromExam");
                else
                    btnDynamicNMFromExam.Text = AppDomain.CurrentDomain.BaseDirectory +
                                                _file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMFromExam");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMFromReport") == "无")
                    btnDynamicNMFromReport.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMFromReport");
                else
                    btnDynamicNMFromReport.Text = AppDomain.CurrentDomain.BaseDirectory +
                                                  _file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMFromReport");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMLtgExam") == "无")
                    btnDynamicNMLtgExam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMLtgExam");
                else
                    btnDynamicNMLtgExam.Text = AppDomain.CurrentDomain.BaseDirectory +
                                               _file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMLtgExam");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMLtgReport") == "无")
                    btnDynamicNMLtgReport.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMLtgReport");
                else
                    btnDynamicNMLtgReport.Text = AppDomain.CurrentDomain.BaseDirectory +
                                                 _file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMLtgReport");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/IndirectReport") == "无")
                    btnIndirectReport.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/IndirectReport");
                else
                    btnIndirectReport.Text = AppDomain.CurrentDomain.BaseDirectory +
                                             _file.ReadLocalXml(@"xml\path.xml", @"Product/IndirectReport");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/IndirectExam") == "无")
                    btnIndirectExam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/IndirectExam");
                else
                    btnIndirectExam.Text = AppDomain.CurrentDomain.BaseDirectory +
                                           _file.ReadLocalXml(@"xml\path.xml", @"Product/IndirectExam");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/IndirectLtgReport") == "无")
                    btnIndirectLtgReport.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/IndirectLtgReport");
                else
                    btnIndirectLtgReport.Text = AppDomain.CurrentDomain.BaseDirectory +
                                             _file.ReadLocalXml(@"xml\path.xml", @"Product/IndirectLtgReport");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/IndirectLtgExam") == "无")
                    btnIndirectLtgExam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/IndirectLtgExam");
                else
                    btnIndirectLtgExam.Text = AppDomain.CurrentDomain.BaseDirectory +
                                           _file.ReadLocalXml(@"xml\path.xml", @"Product/IndirectExam");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/BTReport") == "无")
                    btnBTReport.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/BTReport");
                else
                    btnBTReport.Text = AppDomain.CurrentDomain.BaseDirectory +
                                       _file.ReadLocalXml(@"xml\path.xml", @"Product/BTReport");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/BTExm") == "无")
                    btnBTExm.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/BTExm");
                else
                    btnBTExm.Text = AppDomain.CurrentDomain.BaseDirectory +
                                    _file.ReadLocalXml(@"xml\path.xml", @"Product/BTExm");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/LinSigFromReport") == "无")
                    btnLinSigFromReport.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/LinSigFromReport");
                else
                    btnLinSigFromReport.Text = AppDomain.CurrentDomain.BaseDirectory +
                                               _file.ReadLocalXml(@"xml\path.xml", @"Product/LinSigFromReport");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/LinSigFromExam") == "无")
                    btnLinSigFromExam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/LinSigFromExam");
                else
                    btnLinSigFromExam.Text = AppDomain.CurrentDomain.BaseDirectory +
                                             _file.ReadLocalXml(@"xml\path.xml", @"Product/LinSigFromExam");

                //新

                #region 1939
                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/CANSig1939Report") == "无")
                    btnCANSig1939Report.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/CANSig1939Report");
                else
                    btnCANSig1939Report.Text = AppDomain.CurrentDomain.BaseDirectory +
                                                     _file.ReadLocalXml(@"xml\path.xml", @"Product/CANSig1939Report");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/CANSig1939Exam") == "无")
                    btnCANSig1939Exam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/CANSig1939Exam");
                else
                    btnCANSig1939Exam.Text = AppDomain.CurrentDomain.BaseDirectory +
                                                   _file.ReadLocalXml(@"xml\path.xml", @"Product/CANSig1939Exam");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/CANLtg1939Report") == "无")
                    btnCANLtg1939Report.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/CANLtg1939Report");
                else
                    btnCANLtg1939Report.Text = AppDomain.CurrentDomain.BaseDirectory +
                                               _file.ReadLocalXml(@"xml\path.xml", @"Product/CANLtg1939Report");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/CANLtg1939Exam") == "无")
                    btnCANLtg1939Exam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/CANLtg1939Exam");
                else
                    btnCANLtg1939Exam.Text = AppDomain.CurrentDomain.BaseDirectory +
                                             _file.ReadLocalXml(@"xml\path.xml", @"Product/CANLtg1939Exam");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/AutoSARNM1939Report") == "无")
                    btnAutoSARNM1939Report.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/AutoSARNM1939Report");
                else
                    btnAutoSARNM1939Report.Text = AppDomain.CurrentDomain.BaseDirectory +
                                               _file.ReadLocalXml(@"xml\path.xml", @"Product/AutoSARNM1939Report");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/AutoSARNM1939Exam") == "无")
                    btnAutoSARNM1939Exam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/AutoSARNM1939Exam");
                else
                    btnAutoSARNM1939Exam.Text = AppDomain.CurrentDomain.BaseDirectory +
                                             _file.ReadLocalXml(@"xml\path.xml", @"Product/AutoSARNM1939Exam");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/AutoSARNMLtg1939Report") == "无")
                    btnAutoSARNMLtg1939Report.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/AutoSARNMLtg1939Report");
                else
                    btnAutoSARNMLtg1939Report.Text = AppDomain.CurrentDomain.BaseDirectory +
                                                  _file.ReadLocalXml(@"xml\path.xml", @"Product/AutoSARNMLtg1939Report");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/AutoSARNMLtg1939Exam") == "无")
                    btnAutoSARNMLtg1939Exam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/AutoSARNMLtg1939Exam");
                else
                    btnAutoSARNMLtg1939Exam.Text = AppDomain.CurrentDomain.BaseDirectory +
                                                _file.ReadLocalXml(@"xml\path.xml", @"Product/AutoSARNMLtg1939Exam");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNM1939Report") == "无")
                    btnDynamicNM1939Report.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNM1939Report");
                else
                    btnDynamicNM1939Report.Text = AppDomain.CurrentDomain.BaseDirectory +
                                                     _file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNM1939Report");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNM1939Exam") == "无")
                    btnDynamicNM1939Exam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNM1939Exam");
                else
                    btnDynamicNM1939Exam.Text = AppDomain.CurrentDomain.BaseDirectory +
                                                   _file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNM1939Exam");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMFrom1939Report") == "无")
                    btnDynamicNMFrom1939Report.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMFrom1939Report");
                else
                    btnDynamicNMFrom1939Report.Text = AppDomain.CurrentDomain.BaseDirectory +
                                                  _file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMFrom1939Report");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMFrom1939Exam") == "无")
                    btnDynamicNMFrom1939Exam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMFrom1939Exam");
                else
                    btnDynamicNMFrom1939Exam.Text = AppDomain.CurrentDomain.BaseDirectory +
                                                _file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMFrom1939Exam");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMLtg1939Report") == "无")
                    btnDynamicNMLtg1939Report.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMLtg1939Report");
                else
                    btnDynamicNMLtg1939Report.Text = AppDomain.CurrentDomain.BaseDirectory +
                                                      _file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMLtg1939Report");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMLtg1939Exam") == "无")
                    btnDynamicNMLtg1939Exam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMLtg1939Exam");
                else
                    btnDynamicNMLtg1939Exam.Text = AppDomain.CurrentDomain.BaseDirectory +
                                                    _file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMLtg1939Exam");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/Indirect1939Report") == "无")
                    btnIndirect1939Report.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/Indirect1939Report");
                else
                    btnIndirect1939Report.Text = AppDomain.CurrentDomain.BaseDirectory +
                                                     _file.ReadLocalXml(@"xml\path.xml", @"Product/Indirect1939Report");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/Indirect1939Exam") == "无")
                    btnIndirect1939Exam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/Indirect1939Exam");
                else
                    btnIndirect1939Exam.Text = AppDomain.CurrentDomain.BaseDirectory +
                                               _file.ReadLocalXml(@"xml\path.xml", @"Product/Indirect1939Exam");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/Indirect1939LtgReport") == "无")
                    btnIndirectLtg1939Report.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/IndirectLtg1939Report");
                else
                    btnIndirectLtg1939Report.Text = AppDomain.CurrentDomain.BaseDirectory +
                                                 _file.ReadLocalXml(@"xml\path.xml", @"Product/IndirectLtg1939Report");

                if (_file.ReadLocalXml(@"xml\path.xml", @"Product/IndirectLtg1939Exam") == "无")
                    btnIndirectLtg1939Exam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/IndirectLtg1939Exam");
                else
                    btnIndirectLtg1939Exam.Text = AppDomain.CurrentDomain.BaseDirectory +
                                                   _file.ReadLocalXml(@"xml\path.xml", @"Product/IndirectLtg1939Exam");
                #endregion
            }
            else
            {
                btnSelfCheck.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/SelfPath");
                btncfgExm.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/CfgPath");

                btnCANSigExam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/CANSigExam");
                btnCANSigReport.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/CANSigReport");

                btnCANLtgExam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/CANLtgExam");
                btnCANLtgReport.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/CANLtgReport");

                btnLINSigExam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/LINSigExam");
                btnLinSigReport.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/LINSigReport");

                btnLINLtgExam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/LINLtgExam");
                btnLINLtgReport.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/LINLtgReport");

                btnWifiReport.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/WifiReport");
                btnWifiExam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/WifiExam");

                btnOSEKSigReport.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/OSEKSigReport");
                btnOSEKSigExam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/OSEKSigExam");

                btnOSEKLtnReport.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/OSEKLtnReport");
                btnOSEKLtnExam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/OSEKLtnExam");

                btnDTCExam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/DTCExam");
                btnDTCreport.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/DTCreport");

                //新 
                btnAutoSARNMExam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/AutoSARNMExam");
                btnAutoSARNMReport.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/AutoSARNMReport");

                btnAutoSARNMLtgExam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/AutoSARNMLtgExam");
                btnAutoSARNMLtgReport.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/AutoSARNMLtgReport");

                btnDynamicNMExam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMExam");
                btnDynamicNMReport.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMReport");

                btnDynamicNMFromExam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMFromExam");
                btnDynamicNMFromReport.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMFromReport");

                btnDynamicNMLtgExam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMLtgExam");
                btnDynamicNMLtgReport.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMLtgReport");

                btnIndirectReport.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/IndirectReport");
                btnIndirectExam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/IndirectExam");

                btnIndirectLtgReport.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/IndirectLtgReport");
                btnIndirectLtgExam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/IndirectLtgExam");

                btnBTReport.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/BTReport");
                btnBTExm.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/BTExm");

                btnLinSigFromReport.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/LINSigFromReport");
                btnLinSigFromExam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/LINSigFromExam");

                //新 

                #region 1939

                btnCANSig1939Report.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/CANSig1939Report");
                btnCANSig1939Exam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/CANSig1939Exam");

                btnCANLtg1939Report.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/CANLtg1939Report");
                btnCANLtg1939Exam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/CANLtg1939Exam");

                btnAutoSARNM1939Report.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/AutoSARNM1939Report");
                btnAutoSARNM1939Exam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/AutoSARNM1939Exam");

                btnAutoSARNMLtg1939Report.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/AutoSARNMLtg1939Report");
                btnAutoSARNMLtg1939Exam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/AutoSARNMLtg1939Exam");

                btnDynamicNM1939Report.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNM1939Report");
                btnDynamicNM1939Exam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNM1939Exam");

                btnDynamicNMFrom1939Report.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMFrom1939Report");
                btnDynamicNMFrom1939Exam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMFrom1939Exam");

                btnDynamicNMLtg1939Report.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMLtg1939Report");
                btnDynamicNMLtg1939Exam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/DynamicNMLtg1939Exam");

                btnIndirect1939Report.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/Indirect1939Report");
                btnIndirect1939Exam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/Indirect1939Exam");

                btnIndirectLtg1939Report.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/IndirectLtg1939Report");
                btnIndirectLtg1939Exam.Text = _file.ReadLocalXml(@"xml\path.xml", @"Product/IndirectLtg1939Exam");
                #endregion
            }
        }

        private void cmbSegmentType_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void txtSegmentName_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void txtBaud_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }

        private void btnSave(object sender, EventArgs e)
        {
            //自检工程路径
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/CfgPath", btncfgExm.Text);
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/SelfPath", btnSelfCheck.Text);
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/IsFirst", "false");
            //CAN单节点
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/CANSigExam", btnCANSigExam.Text);
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/CANSigReport", btnCANSigReport.Text);
            //CAN集成
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/CANLtgExam", btnCANLtgExam.Text);
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/CANLtgReport", btnCANLtgReport.Text);
            //LIN主节点
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/LINSigExam", btnLINSigExam.Text);
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/LINSigReport", btnLinSigReport.Text);
            //LIN从节点  
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/LINSigFromExam", btnLinSigFromExam.Text);
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/LINSigFromReport", btnLinSigFromReport.Text);
            //LIN集成
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/LINLtgExam", btnLINLtgExam.Text);
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/LINLtgReport", btnLINLtgReport.Text);
            //AUTOSAR NM 单节点(新)
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/AutoSARNMExam", btnAutoSARNMExam.Text);
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/AutoSARNMReport", btnAutoSARNMReport.Text);
            //AUTOSAR NM 集成(新)
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/AutoSARNMLtgExam", btnAutoSARNMLtgExam.Text);
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/AutoSARNMLtgReport", btnAutoSARNMLtgReport.Text);
            //动力域主(新)
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/DynamicNMExam", btnDynamicNMExam.Text);
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/DynamicNMReport", btnDynamicNMReport.Text);
            //动力域从(新)
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/DynamicNMFromExam", btnDynamicNMFromExam.Text);
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/DynamicNMFromReport", btnDynamicNMFromReport.Text);
            //动力域集成(新)
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/DynamicNMLtgExam", btnDynamicNMLtgExam.Text);
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/DynamicNMLtgReport", btnDynamicNMLtgReport.Text);
            //间接NM(新)
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/IndirectReport", btnIndirectReport.Text);
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/IndirectExam", btnIndirectExam.Text);
            //间接NM集成(新)
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/IndirectLtgReport", btnIndirectLtgReport.Text);
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/IndirectLtgExam", btnIndirectLtgExam.Text);
            //OSEK单节点
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/OSEKSigReport", btnOSEKSigReport.Text);
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/OSEKSigExam", btnOSEKSigExam.Text);
            //OSEK集成
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/OSEKLtnReport", btnOSEKLtnReport.Text);
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/OSEKLtnExam", btnOSEKLtnExam.Text);
            //DTC
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/DTCExam", btnDTCExam.Text);
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/DTCreport", btnDTCreport.Text);
            //Bootloader 
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/BTExm", btnBTExm.Text);
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/BTReport", btnBTReport.Text);
            //路由信息
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/WifiReport", btnWifiReport.Text);
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/WifiExam", btnWifiExam.Text);

            //CAN通信单元（J1939）
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/CANSig1939Report", btnCANSig1939Report.Text);
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/CANSig1939Exam", btnCANSig1939Exam.Text);

            //CAN通信集成（J1939）
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/CANLtg1939Report", btnCANLtg1939Report.Text);
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/CANLtg1939Exam", btnCANLtg1939Exam.Text);

            //AUTOSAR NM单元（J1939）
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/AutoSARNM1939Report", btnAutoSARNM1939Report.Text);
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/AutoSARNM1939Exam", btnAutoSARNM1939Exam.Text);

            //AUTOSAR NM集成（J1939）
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/AutoSARNMLtg1939Report", btnAutoSARNMLtg1939Report.Text);
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/AutoSARNMLtg1939Exam", btnAutoSARNMLtg1939Exam.Text);

            //动力域NM主节点（J1939）
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/DynamicNM1939Report", btnDynamicNM1939Report.Text);
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/DynamicNM1939Exam", btnDynamicNM1939Exam.Text);

            //动力域NM从节点（J1939）
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/DynamicNMFrom1939Report", btnDynamicNMFrom1939Report.Text);
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/DynamicNMFrom1939Exam", btnDynamicNMFrom1939Exam.Text);

            //动力域NM集成（J1939）
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/DynamicNMLtg1939Report", btnDynamicNMLtg1939Report.Text);
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/DynamicNMLtg1939Exam", btnDynamicNMLtg1939Exam.Text);
            //间接NM单元（J1939）
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/Indirect1939Report", btnIndirect1939Report.Text);
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/Indirect1939Exam", btnIndirect1939Exam.Text);
            //间接NM集成（J1939）
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/IndirectLtg1939Report", btnIndirectLtg1939Report.Text);
            _file.UpdateLocalXml(@"xml\path.xml", @"Product/IndirectLtg1939Exam", btnIndirectLtg1939Exam.Text);


            EnumLibrary.SelfPath = btnSelfCheck.Text;
            EnumLibrary.CfgPath = btncfgExm.Text;

            EnumLibrary.CANSigExam = btnCANSigExam.Text;
            EnumLibrary.CANSigReport = btnCANSigReport.Text;

            EnumLibrary.CANLtgExam = btnCANLtgExam.Text;
            EnumLibrary.CANLtgReport = btnCANLtgReport.Text;

            EnumLibrary.LINSigExam = btnLINSigExam.Text;
            EnumLibrary.LINSigReport = btnLinSigReport.Text;

            EnumLibrary.LINSigFromExam = btnLinSigFromExam.Text;
            EnumLibrary.LINSigFromReport = btnLinSigFromReport.Text;

            EnumLibrary.LINLtgExam = btnLINLtgExam.Text;
            EnumLibrary.LINLtgReport = btnLINLtgReport.Text;

            EnumLibrary.WifiExam = btnWifiExam.Text;
            EnumLibrary.WifiReport = btnWifiReport.Text;

            EnumLibrary.OSEKSigReport = btnOSEKSigReport.Text;
            EnumLibrary.OSEKSigExam = btnOSEKSigExam.Text;

            EnumLibrary.OSEKLtnReport = btnOSEKLtnReport.Text;
            EnumLibrary.OSEKLtnExam = btnOSEKLtnExam.Text;

            EnumLibrary.DTCExam = btnDTCExam.Text;
            EnumLibrary.DTCreport = btnDTCreport.Text;
            //
            EnumLibrary.AutoSARNMExam = btnAutoSARNMExam.Text;
            EnumLibrary.AutoSARNMReport = btnAutoSARNMReport.Text;

            EnumLibrary.AutoSARNMLtgExam = btnAutoSARNMLtgExam.Text;
            EnumLibrary.AutoSARNMLtgReport = btnAutoSARNMLtgReport.Text;

            EnumLibrary.DynamicNMExam = btnDynamicNMExam.Text;
            EnumLibrary.DynamicNMReport = btnDynamicNMReport.Text;

            EnumLibrary.DynamicNMFromExam = btnDynamicNMFromExam.Text;
            EnumLibrary.DynamicNMFromReport = btnDynamicNMFromReport.Text;

            EnumLibrary.DynamicNMLtgExam = btnDynamicNMLtgExam.Text;
            EnumLibrary.DynamicNMLtgReport = btnDynamicNMLtgReport.Text;

            EnumLibrary.IndirectExam = btnIndirectExam.Text;
            EnumLibrary.IndirectReport = btnIndirectReport.Text;

            EnumLibrary.IndirectLtgExam = btnIndirectLtgExam.Text;
            EnumLibrary.IndirectLtgReport = btnIndirectLtgReport.Text;

            #region 1939
            EnumLibrary.CANSig1939Report = btnCANSig1939Report.Text;
            EnumLibrary.CANSig1939Exam = btnCANSig1939Exam.Text;

            EnumLibrary.CANLtg1939Report = btnCANLtg1939Report.Text;
            EnumLibrary.CANLtg1939Exam = btnCANLtg1939Exam.Text;

            EnumLibrary.AutoSARNM1939Report = btnAutoSARNM1939Report.Text;
            EnumLibrary.AutoSARNM1939Exam = btnAutoSARNM1939Exam.Text;

            EnumLibrary.AutoSARNMLtg1939Report = btnAutoSARNMLtg1939Report.Text;
            EnumLibrary.AutoSARNMLtg1939Exam = btnAutoSARNMLtg1939Exam.Text;

            EnumLibrary.DynamicNM1939Report = btnDynamicNM1939Report.Text;
            EnumLibrary.DynamicNM1939Exam = btnDynamicNM1939Exam.Text;

            EnumLibrary.DynamicNMFrom1939Report = btnDynamicNMFrom1939Report.Text;
            EnumLibrary.DynamicNMFrom1939Exam = btnDynamicNMFrom1939Exam.Text;

            EnumLibrary.DynamicNMLtg1939Report = btnDynamicNMLtg1939Report.Text;
            EnumLibrary.DynamicNMLtg1939Exam = btnDynamicNMLtg1939Exam.Text;

            EnumLibrary.Indirect1939Report = btnIndirect1939Report.Text;
            EnumLibrary.Indirect1939Exam = btnIndirect1939Exam.Text;

            EnumLibrary.IndirectLtg1939Report = btnIndirectLtg1939Report.Text;
            EnumLibrary.IndirectLtg1939Exam = btnIndirectLtg1939Exam.Text;

            #endregion

            MessageBox.Show(@"保存成功..");
        }

        private void btnEdit_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            ButtonEdit btnEdit = sender as ButtonEdit;
            OpenFileDialog _OFD = new OpenFileDialog();
            if (_OFD.ShowDialog() == DialogResult.OK)
            {
                btnEdit.Text = _OFD.FileName;
            }
        }
    }
}
