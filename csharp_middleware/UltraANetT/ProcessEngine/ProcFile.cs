using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;
using DBCEngine;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using OSEKCLASS;
using NPOI.SS.UserModel;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using NPOI.XSSF.UserModel;

namespace ProcessEngine
{
    public class ProcFile
    {
        private static readonly XmlDocument Doc = new XmlDocument();
        private readonly procDBC _dbcAnalysis = new procDBC();
        private readonly ProcStore _store = new ProcStore();
        procLDF _ldf = new procLDF();
        Dictionary<string, int> dictDtc = new Dictionary<string, int>();

        #region 网关路由表用变量
        static AutoResetEvent _myEvent = new AutoResetEvent(false);
        private static int _intThreadCount;
        private Dictionary<string, string> _dictTestChannel = new Dictionary<string, string>();
        private Dictionary<string, string> _dictDUTInfo = new Dictionary<string, string>();
        private List<Dictionary<string, string>> _listdictDMRRouteInfo = new List<Dictionary<string, string>>();
        private List<Dictionary<string, string>> _listdictIMRRouteInfo = new List<Dictionary<string, string>>();
        private List<Dictionary<string, string>> _listdictDSRRouteInfo = new List<Dictionary<string, string>>();
        private List<Dictionary<string, string>> _listdictISRRouteInfo = new List<Dictionary<string, string>>();
        private List<Dictionary<string, string>> _listdictCLRRouteInfo = new List<Dictionary<string, string>>();
        #endregion
        public string SearchBusByEmlName(string emlName)
        {
            string BusType = "";
            if (emlName != "")
            {
                //cbName.Properties.Items.AddRange(type);
                switch (emlName)
                {
                    case "CAN通信单元用例表":
                    case "CAN通信集成用例表":
                    case "直接NM单元用例表":
                    case "直接NM集成用例表":
                    case "动力域NM主节点用例表":
                    case "动力域NM从节点用例表":
                    case "动力域NM集成用例表":
                    case "间接NM单元用例表":
                    case "间接NM集成用例表":
                    case "通信DTC用例表":
                    case "OSEK NM单元用例表":
                    case "OSEK NM集成用例表":
                    case "Bootloader用例表":
                        BusType = "CAN总线";
                        break;
                    case "LIN通信主节点用例表":
                    case "LIN通信从节点用例表":
                    case "LIN通信集成用例表":
                        BusType = "LIN总线";
                        break;
                    case "网关路由用例表":
                        BusType = "网关路由";
                        break;
                    default:
                        break;

                }
            }
            return BusType;
        }
        public string SearchEmlTemName(string emlName)
        {
            string BusType = "";
            if (emlName != "")
            {
                //cbName.Properties.Items.AddRange(type);
                switch (emlName)
                {
                    case "CAN通信单元用例表":
                    case "直接NM单元用例表":
                    case "动力域NM主节点用例表":
                    case "动力域NM从节点用例表":
                    case "间接NM单元用例表":
                    case "通信DTC用例表":
                    case "OSEK NM单元用例表":
                    case "Bootloader用例表":
                        BusType = "CAN通信单元";
                        break;
                    case "CAN通信集成用例表":
                    case "直接NM集成用例表":
                    case "间接NM集成用例表":
                    case "动力域NM集成用例表":
                    case "OSEK NM集成用例表":
                        BusType = "CAN通信集成";
                        break;
                    case "LIN通信主节点用例表":
                        BusType = "LIN通信主节点";
                        break;
                    case "LIN通信从节点用例表":
                        BusType = "LIN通信从节点";
                        break;
                    case "LIN通信集成用例表":
                        BusType = "LIN通信集成";
                        break;
                    case "网关路由用例表":
                        BusType = "网关路由";
                        break;
                    default:
                        break;

                }
            }
            return BusType;
        }

        #region ini转换

        /// <summary>
        /// 创建CAN单节点ini文件
        /// </summary>
        /// <param name="folder">暂未使用</param>
        /// <param name="configName">文件名称</param>
        /// <param name="dictHead">头数据</param>
        /// <param name="dictNody">真实节点数据</param>
        /// <param name="dictLocal">本地事件</param>
        /// <param name="dictVirNode">虚拟节点</param>
        /// <param name="dbcPath">dbc路径</param>
        /// <returns></returns>
        public string CreateCfginiFromCANS(string folder, string configName, Dictionary<string, string> dictHead,
            Dictionary<string, string> dictNody, List<Dictionary<string, string>> dictLocal, List<string> dictVirNode,
            string dbcPath)
        {
            string error = "";
            int i = 0;
            try
            {
                var folderPath = AppDomain.CurrentDomain.BaseDirectory + "\\configini\\" + configName + "\\";
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                StreamWriter sw = new StreamWriter(folderPath + configName + ".ini", false, Encoding.Default);
                //文件头
                sw.WriteLine("[PathInfo]");
                sw.WriteLine("DBCPathFile=" + AppDomain.CurrentDomain.BaseDirectory + dbcPath);
                foreach (var item in dictHead)
                {
                    if (item.Key.ToString().Trim().Equals("CddFileName"))
                    {
                        string strCddFileName = dictHead["CddFileName"].ToString().Trim() == "--"
                            ? string.Empty
                            : dictHead["CddFileName"].ToString().Trim();
                        sw.WriteLine("CddFileName=" + strCddFileName);
                        dictHead.Remove("CddFileName");
                        break;
                    }
                }

                WriteDUTHead(ref sw, dictHead);
                WriteDUTCANNodeToSig(ref sw, dictNody);
                // WriteLocalToSig(ref sw, dictLocal);
                //WriteVirNode(ref sw, dictVirNode);
                sw.Flush();
                sw.Close();

                return error;
            }
            catch (Exception e)
            {
                error = e.ToString();
                return error;
            }
        }

        public string CreateCfginiFromGateway(string folder, string configName,string excelPath)
        {
            string error = "";
            int type = 0;
            try
            {
                var folderPath = AppDomain.CurrentDomain.BaseDirectory + "\\configini\\" + configName + "\\";
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);
                string strIniPath = folderPath + configName + ".ini";
                XSSFWorkbook workbook = new XSSFWorkbook(excelPath);
                List<string> listSheetName = new List<string>();
                for (int i = 0; i < workbook.NumberOfSheets; i++)
                {
                    listSheetName.Add(workbook.GetSheetAt(i).SheetName);
                }
                Debug.Print("开始解析");
                if (listSheetName.Contains("GW_information"))
                {
                    //开始解析
                    AnalysisGW_information(workbook.GetSheet("GW_information"));
                    //解析完成把此sheet名称从List内删除，后续所有List进行多线程解析
                    if (listSheetName.Contains("GW_information"))
                    {
                        listSheetName.Remove("GW_information");
                    }
                }
                //开始进行线程池设置
                _intThreadCount = listSheetName.Count;
                ThreadPool.SetMinThreads(1, 1);
                ThreadPool.SetMaxThreads(5, 5);
                for (int i = 0; i < listSheetName.Count; i++)
                {
                    switch (listSheetName[i].Trim().ToLower())
                    {
                        case "directmessage_routingtable":
                            ThreadPool.QueueUserWorkItem(AnalysisDirectMessage_Routingtable, workbook.GetSheet(listSheetName[i]));
                            break;
                        case "indirectmessage_routingtable":
                            ThreadPool.QueueUserWorkItem(AnalysisIndirectMessage_Routingtable, workbook.GetSheet(listSheetName[i]));
                            break;
                        case "dependentsignal_routingtable":
                            ThreadPool.QueueUserWorkItem(AnalysisDependentSignal_Routingtable, workbook.GetSheet(listSheetName[i]));
                            break;
                        case "independentsignal_routingtable":
                            ThreadPool.QueueUserWorkItem(AnalysisIndependentSignal_Routingtable, workbook.GetSheet(listSheetName[i]));
                            break;
                        case "canlinsignal_routingtable":
                            ThreadPool.QueueUserWorkItem(AnalysisCANLINSignal_Routingtable, workbook.GetSheet(listSheetName[i]));
                            break;
                        default:
                            Interlocked.Decrement(ref _intThreadCount);
                            break;
                    }
                }
                _myEvent.WaitOne();
                workbook.Close();
                Debug.Print("开始生成ini...");
                error = CreateINI(strIniPath);
                return error;
            }
            catch (Exception e)
            {
                error = e.ToString();
                return error;
            }
        }

        #region 网关路由表用方法

        private string CreateINI(string strIniPath)
        {
            StreamWriter sw = new StreamWriter(strIniPath, false, Encoding.Default);
            try
            {
                sw.WriteLine("[DUTInfo]");
                foreach (var dutinfo in _dictDUTInfo)
                {
                    sw.WriteLine("DUT." + dutinfo.Key + "=" + dutinfo.Value);
                }

                sw.WriteLine("[DMRRouteInfo]");
                sw.WriteLine("DMR_num=" + _listdictDMRRouteInfo.Count);
                int intDMR = 0;
                foreach (var dictDMRRouteInfo in _listdictDMRRouteInfo)
                {
                    foreach (var keyDMRRouteInfo in dictDMRRouteInfo)
                    {
                        sw.WriteLine("DMR[" + intDMR + "]." + keyDMRRouteInfo.Key + "=" + keyDMRRouteInfo.Value);
                    }

                    intDMR++;
                }

                sw.WriteLine("[IMRRouteInfo]");
                sw.WriteLine("IMR_num=" + _listdictIMRRouteInfo.Count);
                int intIMR = 0;
                foreach (var dictIMRRouteInfo in _listdictIMRRouteInfo)
                {
                    foreach (var keyIMRRouteInfo in dictIMRRouteInfo)
                    {
                        sw.WriteLine("IMR[" + intIMR + "]." + keyIMRRouteInfo.Key + "=" + keyIMRRouteInfo.Value);
                    }

                    intIMR++;
                }

                sw.WriteLine("[DSRRouteInfo]");
                sw.WriteLine("DSR_num=" + _listdictDSRRouteInfo.Count);
                int intDSR = 0;
                foreach (var dictDSRRouteInfo in _listdictDSRRouteInfo)
                {
                    foreach (var keyDSRRouteInfo in dictDSRRouteInfo)
                    {
                        sw.WriteLine("DSR[" + intDSR + "]." + keyDSRRouteInfo.Key + "=" + keyDSRRouteInfo.Value);
                    }

                    intDSR++;
                }

                sw.WriteLine("[ISRRouteInfo]");
                sw.WriteLine("ISR_num=" + _listdictISRRouteInfo.Count);
                int intISR = 0;
                foreach (var dictISRRouteInfo in _listdictISRRouteInfo)
                {
                    foreach (var keyISRRouteInfo in dictISRRouteInfo)
                    {
                        sw.WriteLine("ISR[" + intISR + "]." + keyISRRouteInfo.Key + "=" + keyISRRouteInfo.Value);
                    }

                    intISR++;
                }

                sw.WriteLine("[CLRRouteInfo]");
                sw.WriteLine("CLR_num=" + _listdictCLRRouteInfo.Count);
                int intCLR = 0;
                foreach (var dictCLRRouteInfo in _listdictCLRRouteInfo)
                {
                    foreach (var keyCLRRouteInfo in dictCLRRouteInfo)
                    {
                        sw.WriteLine("CLR[" + intCLR + "]." + keyCLRRouteInfo.Key + "=" + keyCLRRouteInfo.Value);
                    }

                    intCLR++;
                }
            }
            catch (Exception ex)
            {
                sw.WriteLine(ex.ToString());
                sw.Flush();
                sw.Close();
                return ex.Message;
            }
            sw.Flush();
            sw.Close();
            return string.Empty;
        }

        private void AnalysisGW_information(object objSheet)
        {
            XSSFSheet sheet = objSheet as XSSFSheet;
            //开始解析代码
            Dictionary<string, string> gwInfo = new Dictionary<string, string>();
            for (int i = 0; i <= sheet.LastRowNum; i++)
            {
                IRow row = sheet.GetRow(i);
                var listCell = row.Cells;
                if (listCell.Count > 1 && GetCellValue(listCell[0]).ToString() == "网段名称")
                {
                    int index = 0;
                    for (int j = 1; j < listCell.Count; j++)
                    {
                        _dictTestChannel[GetCellValue(listCell[j]).ToString()] =
                            GetCellValue(sheet.GetRow(i + 4).Cells[j]).ToString();
                        gwInfo["TerminalR[" + index + "]"] =
                            GetCellValue(sheet.GetRow(i + 1).Cells[j]).ToString() == "无" ? "0" : "1";
                        gwInfo["CAN_Protocol[" + index + "]"] =
                            GetCellValue(sheet.GetRow(i + 2).Cells[j]).ToString() == "11898" ? "0" : "1";
                        gwInfo["CAN_ByteOrder[" + index + "]"] =
                            GetCellValue(sheet.GetRow(i + 3).Cells[j]).ToString().ToLower() == "motorola" ? "0" : "1";
                        index++;
                    }
                }
                else
                {
                    switch (GetCellValue(listCell[0]).ToString().Trim())
                    {
                        case "节点名称":
                            gwInfo["Name"] = GetCellValue(listCell[1]).ToString();
                            break;
                        case "包含网段数量":
                            gwInfo["TestChannelNum"] = GetCellValue(listCell[1]).ToString();
                            break;
                        case "节点配置盒ID":
                            gwInfo["Slavebox"] = GetCellValue(listCell[1]).ToString();
                            break;
                        case "12V/24V系统":
                            gwInfo["SystemType"] = GetCellValue(listCell[1]).ToString().ToUpper() == "12V" ? "0" : "1";
                            break;
                    }
                }
            }
            _dictDUTInfo = gwInfo;
        }

        private void AnalysisDirectMessage_Routingtable(object objSheet)
        {
            XSSFSheet sheet = objSheet as XSSFSheet;
            Dictionary<int, string> dictTestChannelInCellIndex = new Dictionary<int, string>();
            string strSourceNetworkCache = string.Empty;//用来记录最新的Source Network是哪个网段，作为缓存，节省向上找的时间
            //开始解析代码
            if (sheet.LastRowNum < 3)
            {
                Debug.Print("DirectMessage_Routingtable内未找到数据");
            }
            else
            {
                var testChannelRow = sheet.GetRow(1);
                for (int i = 0; i < testChannelRow.Cells.Count; i++)
                {
                    if (_dictTestChannel.ContainsKey(GetCellValue(testChannelRow.Cells[i]).ToString()))
                    {
                        dictTestChannelInCellIndex[i] = GetCellValue(testChannelRow.Cells[i]).ToString();
                    }
                }

                int intErrorCount = 0;
                for (int i = 3; i <= sheet.LastRowNum; i++)
                {
                    Dictionary<string, string> dictDMRRow = new Dictionary<string, string>();
                    var listRow = sheet.GetRow(i).Cells;
                    if (string.IsNullOrEmpty(GetCellValue(listRow[0]).ToString().Trim()))
                    {
                        Debug.Print("DirectMessage_Routingtable内第" + (i + 1) + "行解析错误，报文名称为空，程序已跳过此行。");
                        intErrorCount++;
                        if (intErrorCount >= 3)
                        {
                            
                            Debug.Print("报文名称连续3行为空，结束此页解析。");
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    dictDMRRow["MessageName"] = GetCellValue(listRow[0]).ToString().Trim();
                    dictDMRRow["RouteID"] = GetCellValue(listRow[1]).ToString().Trim();
                    dictDMRRow["RouteDLC"] = GetCellValue(listRow[2]).ToString().Trim();
                    dictDMRRow["SourceCycleTime"] = GetCellValue(listRow[3]).ToString().Trim().ToLower() == "event"
                        ? "-1"
                        : GetCellValue(listRow[3]).ToString().Trim();
                    if (!string.IsNullOrEmpty(GetCellValue(listRow[4]).ToString().Trim()))
                    {
                        strSourceNetworkCache = GetCellValue(listRow[4]).ToString().Trim();
                    }

                    dictDMRRow["SourceNetwork"] = _dictTestChannel.ContainsKey(strSourceNetworkCache)
                        ? _dictTestChannel[strSourceNetworkCache]
                        : string.Empty; //一旦没有找到该网段，则为空
                    dictDMRRow["DesRouteID"] = GetCellValue(listRow[5]).ToString().Trim();
                    int intDestNetNum = 0;
                    foreach (var keyCellIndex in dictTestChannelInCellIndex)
                    {
                        if (GetCellValue(listRow[keyCellIndex.Key]).ToString().Trim() == "是")
                        {
                            dictDMRRow["DestNetworkNum[" + intDestNetNum + "].DestinationNetwork"] =
                                _dictTestChannel[keyCellIndex.Value];
                            dictDMRRow["DestNetworkNum[" + intDestNetNum + "].DestCycleTime"] =
                                GetCellValue(listRow[keyCellIndex.Key + 1]).ToString().Trim().ToLower() == "event"
                                    ? "-1" : GetCellValue(listRow[keyCellIndex.Key + 1]).ToString().Trim();
                            intDestNetNum++;
                        }
                    }

                    dictDMRRow["DestNetNum"] = intDestNetNum.ToString();
                    _listdictDMRRouteInfo.Add(dictDMRRow);
                }
            }
            //完成后记得减去
            Interlocked.Decrement(ref _intThreadCount);
            if (_intThreadCount == 0)
            {
                _myEvent.Set();
            }
        }

        private void AnalysisIndirectMessage_Routingtable(object objSheet)
        {
            XSSFSheet sheet = objSheet as XSSFSheet;
            Dictionary<int, string> dictTestChannelInCellIndex = new Dictionary<int, string>();
            string strSourceNetworkCache = string.Empty;//用来记录最新的Source Network是哪个网段，作为缓存，节省向上找的时间
            //开始解析代码
            if (sheet.LastRowNum < 3)
            {
                Debug.Print("IndirectMessage_Routingtable内未找到数据");
            }
            else
            {
                var testChannelRow = sheet.GetRow(1);
                for (int i = 0; i < testChannelRow.Cells.Count; i++)
                {
                    if (_dictTestChannel.ContainsKey(GetCellValue(testChannelRow.Cells[i]).ToString()))
                    {
                        dictTestChannelInCellIndex[i] = GetCellValue(testChannelRow.Cells[i]).ToString();
                    }
                }
                int intErrorCount = 0;
                for (int i = 3; i <= sheet.LastRowNum; i++)
                {
                    Dictionary<string, string> dictIMRRow = new Dictionary<string, string>();
                    var listRow = sheet.GetRow(i).Cells;
                    if (string.IsNullOrEmpty(GetCellValue(listRow[0]).ToString().Trim()))
                    {
                        Debug.Print("IndirectMessage_Routingtable内第" + (i + 1) + "行解析错误，报文名称为空，程序已跳过此行。");
                        intErrorCount++;
                        if (intErrorCount >= 3)
                        {
                            Debug.Print("报文名称连续3行为空，结束此页解析。");
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    dictIMRRow["MessageName"] = GetCellValue(listRow[0]).ToString().Trim();
                    dictIMRRow["RouteID"] = GetCellValue(listRow[1]).ToString().Trim();
                    dictIMRRow["RouteDLC"] = GetCellValue(listRow[2]).ToString().Trim();
                    dictIMRRow["SourceCycleTime"] = GetCellValue(listRow[3]).ToString().Trim().ToLower() == "event"
                        ? "-1" : GetCellValue(listRow[3]).ToString().Trim();
                    if (!string.IsNullOrEmpty(GetCellValue(listRow[4]).ToString().Trim()))
                    {
                        strSourceNetworkCache = GetCellValue(listRow[4]).ToString().Trim();
                    }
                    dictIMRRow["SourceNetwork"] = _dictTestChannel.ContainsKey(strSourceNetworkCache)
                        ? _dictTestChannel[strSourceNetworkCache]
                        : string.Empty; //一旦没有找到该网段，则为空
                    dictIMRRow["DesRouteID"] = GetCellValue(listRow[5]).ToString().Trim();
                    int intDestNetNum = 0;
                    foreach (var keyCellIndex in dictTestChannelInCellIndex)
                    {
                        if (GetCellValue(listRow[keyCellIndex.Key]).ToString().Trim() == "是")
                        {
                            dictIMRRow["DestNetworkNum[" + intDestNetNum + "].DestinationNetwork"] =
                                _dictTestChannel[keyCellIndex.Value];
                            dictIMRRow["DestNetworkNum[" + intDestNetNum + "].DestCycleTime"] =
                                GetCellValue(listRow[keyCellIndex.Key + 1]).ToString().Trim().ToLower() == "event"
                                    ? "-1" : GetCellValue(listRow[keyCellIndex.Key + 1]).ToString().Trim();
                            intDestNetNum++;
                        }
                    }

                    dictIMRRow["DestNetNum"] = intDestNetNum.ToString();
                    _listdictIMRRouteInfo.Add(dictIMRRow);
                }
            }
            //完成后记得减去
            Interlocked.Decrement(ref _intThreadCount);
            if (_intThreadCount == 0)
            {
                _myEvent.Set();
            }
        }
        private void AnalysisDependentSignal_Routingtable(object objSheet)
        {
            XSSFSheet sheet = objSheet as XSSFSheet;
            Dictionary<int, string> dictTestChannelInCellIndex = new Dictionary<int, string>();
            Dictionary<string, Dictionary<string, string>> dictdictTestChannelCache =
                new Dictionary<string, Dictionary<string, string>>();
            string strSourceNetworkCache = string.Empty;//用来记录最新的Source Network是哪个网段，作为缓存，节省向上找的时间
            string strMessageNameCache = string.Empty;
            string strMessageIDCache = string.Empty;
            string strDLCCache = string.Empty;
            string strSourceCycleCache = string.Empty;
            //开始解析代码
            if (sheet.LastRowNum < 3)
            {
                Debug.Print("DependentSignal_Routingtable内未找到数据");
            }
            else
            {
                var testChannelRow = sheet.GetRow(1);
                for (int i = 0; i < testChannelRow.Cells.Count; i++)
                {
                    if (_dictTestChannel.ContainsKey(GetCellValue(testChannelRow.Cells[i]).ToString()))
                    {
                        dictTestChannelInCellIndex[i] = GetCellValue(testChannelRow.Cells[i]).ToString();
                        Dictionary<string, string> dictTestChannelCache = new Dictionary<string, string>();
                        dictTestChannelCache["MessageName"] = string.Empty;
                        dictTestChannelCache["MessageID"] = string.Empty;
                        dictTestChannelCache["DestinationCycle"] = string.Empty;
                        dictdictTestChannelCache[dictTestChannelInCellIndex[i]] = dictTestChannelCache;
                    }
                }
                int intErrorCount = 0;
                for (int i = 3; i <= sheet.LastRowNum; i++)
                {
                    Dictionary<string, string> dictDSRRow = new Dictionary<string, string>();
                    var listRow = sheet.GetRow(i).Cells;
                    if (string.IsNullOrEmpty(GetCellValue(listRow[0]).ToString().Trim()))
                    {
                        Debug.Print("DependentSignal_Routingtable内第" + (i + 1) + "行解析错误，报文名称为空，程序已跳过此行。");
                        intErrorCount++;
                        if (intErrorCount >= 3)
                        {
                            Debug.Print("报文名称连续3行为空，结束此页解析。");
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    dictDSRRow["SignalName"] = GetCellValue(listRow[0]).ToString().Trim();
                    dictDSRRow["AbsentValue"] = GetCellValue(listRow[1]).ToString().Trim();
                    if (!string.IsNullOrEmpty(GetCellValue(listRow[2]).ToString().Trim()))
                    {
                        strMessageNameCache = GetCellValue(listRow[2]).ToString().Trim();
                    }
                    dictDSRRow["MessageName"] = strMessageNameCache;
                    if (!string.IsNullOrEmpty(GetCellValue(listRow[3]).ToString().Trim()))
                    {
                        strMessageIDCache = GetCellValue(listRow[3]).ToString().Trim();
                    }
                    dictDSRRow["RouteID"] = strMessageIDCache;
                    if (!string.IsNullOrEmpty(GetCellValue(listRow[4]).ToString().Trim()))
                    {
                        strDLCCache = GetCellValue(listRow[4]).ToString().Trim();
                    }
                    dictDSRRow["RouteDLC"] = strDLCCache;
                    if (!string.IsNullOrEmpty(GetCellValue(listRow[5]).ToString().Trim()))
                    {
                        strSourceCycleCache = GetCellValue(listRow[5]).ToString().Trim();
                    }
                    dictDSRRow["SourceCycleTime"] = strSourceCycleCache;
                    dictDSRRow["SourceStartBit"] = GetCellValue(listRow[6]).ToString().Trim();
                    dictDSRRow["SourceBitLength"] = GetCellValue(listRow[7]).ToString().Trim();
                    if (!string.IsNullOrEmpty(GetCellValue(listRow[8]).ToString().Trim()))
                    {
                        strSourceNetworkCache = GetCellValue(listRow[8]).ToString().Trim();
                    }
                    dictDSRRow["SourceNetwork"] = _dictTestChannel.ContainsKey(strSourceNetworkCache)
                        ? _dictTestChannel[strSourceNetworkCache]
                        : string.Empty; //一旦没有找到该网段，则为空
                    int intDestNetNum = 0;
                    foreach (var keyCellIndex in dictTestChannelInCellIndex)
                    {
                        if (GetCellValue(listRow[keyCellIndex.Key]).ToString().Trim() == "是")
                        {
                            dictDSRRow["DestNetworkNum[" + intDestNetNum + "].DestinationNetwork"] =
                                _dictTestChannel[keyCellIndex.Value];
                            if (!string.IsNullOrEmpty(GetCellValue(listRow[keyCellIndex.Key + 1]).ToString().Trim()))
                            {
                                dictdictTestChannelCache[keyCellIndex.Value]["MessageName"] =
                                    GetCellValue(listRow[keyCellIndex.Key + 1]).ToString().Trim();
                            }
                            dictDSRRow["DestNetworkNum[" + intDestNetNum + "].DesMessageName"] =
                                dictdictTestChannelCache[keyCellIndex.Value]["MessageName"];

                            if (!string.IsNullOrEmpty(GetCellValue(listRow[keyCellIndex.Key + 2]).ToString().Trim()))
                            {
                                dictdictTestChannelCache[keyCellIndex.Value]["MessageID"] =
                                    GetCellValue(listRow[keyCellIndex.Key + 2]).ToString().Trim();
                            }
                            dictDSRRow["DestNetworkNum[" + intDestNetNum + "].DesRouteID"] =
                                dictdictTestChannelCache[keyCellIndex.Value]["MessageID"];

                            if (!string.IsNullOrEmpty(GetCellValue(listRow[keyCellIndex.Key + 3]).ToString().Trim()))
                            {
                                dictdictTestChannelCache[keyCellIndex.Value]["DestinationCycle"] =
                                    GetCellValue(listRow[keyCellIndex.Key + 3]).ToString().Trim();
                            }
                            dictDSRRow["DestNetworkNum[" + intDestNetNum + "].DestinationCycleTime"] =
                                dictdictTestChannelCache[keyCellIndex.Value]["DestinationCycle"];

                            dictDSRRow["DestNetworkNum[" + intDestNetNum + "].DestinationStartBit"] =
                                GetCellValue(listRow[keyCellIndex.Key + 4]).ToString().Trim();
                            dictDSRRow["DestNetworkNum[" + intDestNetNum + "].DestinationBitLength"] =
                                GetCellValue(listRow[keyCellIndex.Key + 5]).ToString().Trim();
                            intDestNetNum++;
                        }
                    }

                    dictDSRRow["DestNetNum"] = intDestNetNum.ToString();
                    _listdictDSRRouteInfo.Add(dictDSRRow);
                }
            }

            //完成后记得减去
            Interlocked.Decrement(ref _intThreadCount);
            if (_intThreadCount == 0)
            {
                _myEvent.Set();
            }
        }
        private void AnalysisIndependentSignal_Routingtable(object objSheet)
        {
            XSSFSheet sheet = objSheet as XSSFSheet;
            Dictionary<int, string> dictTestChannelInCellIndex = new Dictionary<int, string>();
            Dictionary<string, Dictionary<string, string>> dictdictTestChannelCache =
                new Dictionary<string, Dictionary<string, string>>();
            string strSourceNetworkCache = string.Empty;//用来记录最新的Source Network是哪个网段，作为缓存，节省向上找的时间
            string strMessageNameCache = string.Empty;
            string strMessageIDCache = string.Empty;
            string strDLCCache = string.Empty;
            string strSourceCycleCache = string.Empty;
            //开始解析代码
            if (sheet.LastRowNum < 3)
            {
                Debug.Print("IndependentSignal_Routingtable内未找到数据");
            }
            else
            {
                var testChannelRow = sheet.GetRow(1);
                for (int i = 0; i < testChannelRow.Cells.Count; i++)
                {
                    if (_dictTestChannel.ContainsKey(GetCellValue(testChannelRow.Cells[i]).ToString()))
                    {
                        dictTestChannelInCellIndex[i] = GetCellValue(testChannelRow.Cells[i]).ToString();
                        Dictionary<string, string> dictTestChannelCache = new Dictionary<string, string>();
                        dictTestChannelCache["MessageName"] = string.Empty;
                        dictTestChannelCache["MessageID"] = string.Empty;
                        dictTestChannelCache["DestinationCycle"] = string.Empty;
                        dictdictTestChannelCache[dictTestChannelInCellIndex[i]] = dictTestChannelCache;
                    }
                }
                int intErrorCount = 0;
                for (int i = 3; i <= sheet.LastRowNum; i++)
                {
                    Dictionary<string, string> dictISRRow = new Dictionary<string, string>();
                    var listRow = sheet.GetRow(i).Cells;
                    if (string.IsNullOrEmpty(GetCellValue(listRow[0]).ToString().Trim()))
                    {
                        Debug.Print("IndependentSignal_Routingtable内第" + (i + 1) + "行解析错误，报文名称为空，程序已跳过此行。");
                        intErrorCount++;
                        if (intErrorCount >= 3)
                        {
                            Debug.Print("报文名称连续3行为空，结束此页解析。");
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    dictISRRow["SignalName"] = GetCellValue(listRow[0]).ToString().Trim();
                    dictISRRow["TimeoutValue"] =
                        GetCellValue(listRow[1]).ToString().Trim().ToLower().Replace(" ", "") == "lastvalue"
                            ? "-1" : GetCellValue(listRow[1]).ToString().Trim();
                    dictISRRow["DefaultValue"] = GetCellValue(listRow[2]).ToString().Trim();
                    dictISRRow["MaxValue"] = GetCellValue(listRow[3]).ToString().Trim();
                    dictISRRow["ErrorValue"] = GetCellValue(listRow[4]).ToString().Trim();
                    if (!string.IsNullOrEmpty(GetCellValue(listRow[5]).ToString().Trim()))
                    {
                        strMessageNameCache = GetCellValue(listRow[5]).ToString().Trim();
                    }
                    dictISRRow["MessageName"] = strMessageNameCache;
                    if (!string.IsNullOrEmpty(GetCellValue(listRow[6]).ToString().Trim()))
                    {
                        strMessageIDCache = GetCellValue(listRow[6]).ToString().Trim();
                    }
                    dictISRRow["RouteID"] = strMessageIDCache;
                    if (!string.IsNullOrEmpty(GetCellValue(listRow[7]).ToString().Trim()))
                    {
                        strDLCCache = GetCellValue(listRow[7]).ToString().Trim();
                    }
                    dictISRRow["RouteDLC"] = strDLCCache;
                    if (!string.IsNullOrEmpty(GetCellValue(listRow[8]).ToString().Trim()))
                    {
                        strSourceCycleCache = GetCellValue(listRow[8]).ToString().Trim();
                    }
                    dictISRRow["SourceCycleTime"] = strSourceCycleCache;
                    dictISRRow["SourceStartBit"] = GetCellValue(listRow[9]).ToString().Trim();
                    dictISRRow["SourceBitLength"] = GetCellValue(listRow[10]).ToString().Trim();
                    if (!string.IsNullOrEmpty(GetCellValue(listRow[11]).ToString().Trim()))
                    {
                        strSourceNetworkCache = GetCellValue(listRow[11]).ToString().Trim();
                    }
                    dictISRRow["SourceNetwork"] = _dictTestChannel.ContainsKey(strSourceNetworkCache)
                        ? _dictTestChannel[strSourceNetworkCache]
                        : string.Empty; //一旦没有找到该网段，则为空
                    int intDestNetNum = 0;
                    foreach (var keyCellIndex in dictTestChannelInCellIndex)
                    {
                        if (GetCellValue(listRow[keyCellIndex.Key]).ToString().Trim() == "是")
                        {
                            dictISRRow["DestNetworkNum[" + intDestNetNum + "].DestinationNetwork"] =
                                _dictTestChannel[keyCellIndex.Value];
                            if (!string.IsNullOrEmpty(GetCellValue(listRow[keyCellIndex.Key + 1]).ToString().Trim()))
                            {
                                dictdictTestChannelCache[keyCellIndex.Value]["MessageName"] =
                                    GetCellValue(listRow[keyCellIndex.Key + 1]).ToString().Trim();
                            }
                            dictISRRow["DestNetworkNum[" + intDestNetNum + "].DesMessageName"] =
                                dictdictTestChannelCache[keyCellIndex.Value]["MessageName"];

                            if (!string.IsNullOrEmpty(GetCellValue(listRow[keyCellIndex.Key + 2]).ToString().Trim()))
                            {
                                dictdictTestChannelCache[keyCellIndex.Value]["MessageID"] =
                                    GetCellValue(listRow[keyCellIndex.Key + 2]).ToString().Trim();
                            }
                            dictISRRow["DestNetworkNum[" + intDestNetNum + "].DesRouteID"] =
                                dictdictTestChannelCache[keyCellIndex.Value]["MessageID"];

                            if (!string.IsNullOrEmpty(GetCellValue(listRow[keyCellIndex.Key + 3]).ToString().Trim()))
                            {
                                dictdictTestChannelCache[keyCellIndex.Value]["DestinationCycle"] =
                                    GetCellValue(listRow[keyCellIndex.Key + 3]).ToString().Trim();
                            }
                            dictISRRow["DestNetworkNum[" + intDestNetNum + "].DestinationCycleTime"] =
                                dictdictTestChannelCache[keyCellIndex.Value]["DestinationCycle"];

                            dictISRRow["DestNetworkNum[" + intDestNetNum + "].DestinationStartBit"] =
                                GetCellValue(listRow[keyCellIndex.Key + 4]).ToString().Trim();
                            dictISRRow["DestNetworkNum[" + intDestNetNum + "].DestinationBitLength"] =
                                GetCellValue(listRow[keyCellIndex.Key + 5]).ToString().Trim();
                            intDestNetNum++;
                        }
                    }

                    dictISRRow["DestNetNum"] = intDestNetNum.ToString();
                    _listdictISRRouteInfo.Add(dictISRRow);
                }
            }

            //完成后记得减去
            Interlocked.Decrement(ref _intThreadCount);
            if (_intThreadCount == 0)
            {
                _myEvent.Set();
            }
        }
        private void AnalysisCANLINSignal_Routingtable(object objSheet)
        {
            XSSFSheet sheet = objSheet as XSSFSheet;
            Dictionary<int, string> dictTestChannelInCellIndex = new Dictionary<int, string>();
            Dictionary<string, Dictionary<string, string>> dictdictTestChannelCache =
                new Dictionary<string, Dictionary<string, string>>();
            string strSourceNetworkCache = string.Empty;//用来记录最新的Source Network是哪个网段，作为缓存，节省向上找的时间
            string strMessageNameCache = string.Empty;
            string strMessageIDCache = string.Empty;
            string strDLCCache = string.Empty;
            string strSourceCycleCache = string.Empty;
            //开始解析代码
            if (sheet.LastRowNum < 3)
            {
                Debug.Print("CANLINSignal_Routingtable内未找到数据");
            }
            else
            {
                var testChannelRow = sheet.GetRow(1);
                for (int i = 0; i < testChannelRow.Cells.Count; i++)
                {
                    if (_dictTestChannel.ContainsKey(GetCellValue(testChannelRow.Cells[i]).ToString()))
                    {
                        dictTestChannelInCellIndex[i] = GetCellValue(testChannelRow.Cells[i]).ToString();
                        Dictionary<string, string> dictTestChannelCache = new Dictionary<string, string>();
                        dictTestChannelCache["MessageName"] = string.Empty;
                        dictTestChannelCache["MessageID"] = string.Empty;
                        dictTestChannelCache["DestinationCycle"] = string.Empty;
                        dictdictTestChannelCache[dictTestChannelInCellIndex[i]] = dictTestChannelCache;
                    }
                }
                int intErrorCount = 0;
                for (int i = 3; i <= sheet.LastRowNum; i++)
                {
                    Dictionary<string, string> dictCLRRow = new Dictionary<string, string>();
                    var listRow = sheet.GetRow(i).Cells;
                    if (string.IsNullOrEmpty(GetCellValue(listRow[0]).ToString().Trim()))
                    {
                        Debug.Print("CANLINSignal_Routingtable内第" + (i + 1) + "行解析错误，报文名称为空，程序已跳过此行。");
                        intErrorCount++;
                        if (intErrorCount >= 3)
                        {
                            Debug.Print("报文名称连续3行为空，结束此页解析。");
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    dictCLRRow["SignalName"] = GetCellValue(listRow[0]).ToString().Trim();
                    dictCLRRow["TimeoutValue"] =
                        GetCellValue(listRow[1]).ToString().Trim().ToLower().Replace(" ", "") == "lastvalue"
                            ? "-1" : GetCellValue(listRow[1]).ToString().Trim();
                    dictCLRRow["DefaultValue"] = GetCellValue(listRow[2]).ToString().Trim();
                    dictCLRRow["MaxValue"] = GetCellValue(listRow[3]).ToString().Trim();
                    dictCLRRow["ErrorValue"] = GetCellValue(listRow[4]).ToString().Trim();
                    if (!string.IsNullOrEmpty(GetCellValue(listRow[5]).ToString().Trim()))
                    {
                        strMessageNameCache = GetCellValue(listRow[5]).ToString().Trim();
                    }
                    dictCLRRow["MessageName"] = strMessageNameCache;
                    if (!string.IsNullOrEmpty(GetCellValue(listRow[6]).ToString().Trim()))
                    {
                        strMessageIDCache = GetCellValue(listRow[6]).ToString().Trim();
                    }
                    dictCLRRow["RouteID"] = strMessageIDCache;
                    if (!string.IsNullOrEmpty(GetCellValue(listRow[7]).ToString().Trim()))
                    {
                        strDLCCache = GetCellValue(listRow[7]).ToString().Trim();
                    }
                    dictCLRRow["RouteDLC"] = strDLCCache;
                    if (!string.IsNullOrEmpty(GetCellValue(listRow[8]).ToString().Trim()))
                    {
                        strSourceCycleCache = GetCellValue(listRow[8]).ToString().Trim();
                    }
                    dictCLRRow["SourceCycleTime"] = strSourceCycleCache;
                    dictCLRRow["SourceStartBit"] = GetCellValue(listRow[9]).ToString().Trim();
                    dictCLRRow["SourceBitLength"] = GetCellValue(listRow[10]).ToString().Trim();
                    if (!string.IsNullOrEmpty(GetCellValue(listRow[11]).ToString().Trim()))
                    {
                        strSourceNetworkCache = GetCellValue(listRow[11]).ToString().Trim();
                    }
                    //判断是否是LIN
                    if (strSourceNetworkCache.Contains("LIN"))
                    {
                        dictCLRRow["SourceNetwork"] = strSourceNetworkCache.ToLower().Replace("lin", string.Empty);
                    }
                    else
                    {
                        dictCLRRow["SourceNetwork"] = _dictTestChannel.ContainsKey(strSourceNetworkCache)
                            ? _dictTestChannel[strSourceNetworkCache]
                            : string.Empty; //一旦没有找到该网段，则为空
                    }
                    int intDestNetNum = 0;
                    foreach (var keyCellIndex in dictTestChannelInCellIndex)
                    {
                        if (GetCellValue(listRow[keyCellIndex.Key]).ToString().Trim() == "是")
                        {
                            dictCLRRow["DestNetworkNum[" + intDestNetNum + "].DestinationNetwork"] =
                                _dictTestChannel[keyCellIndex.Value];
                            if (!string.IsNullOrEmpty(GetCellValue(listRow[keyCellIndex.Key + 1]).ToString().Trim()))
                            {
                                dictdictTestChannelCache[keyCellIndex.Value]["MessageName"] =
                                    GetCellValue(listRow[keyCellIndex.Key + 1]).ToString().Trim();
                            }
                            dictCLRRow["DestNetworkNum[" + intDestNetNum + "].DesMessageName"] =
                                dictdictTestChannelCache[keyCellIndex.Value]["MessageName"];

                            if (!string.IsNullOrEmpty(GetCellValue(listRow[keyCellIndex.Key + 2]).ToString().Trim()))
                            {
                                dictdictTestChannelCache[keyCellIndex.Value]["MessageID"] =
                                    GetCellValue(listRow[keyCellIndex.Key + 2]).ToString().Trim();
                            }
                            dictCLRRow["DestNetworkNum[" + intDestNetNum + "].DesRouteID"] =
                                dictdictTestChannelCache[keyCellIndex.Value]["MessageID"];

                            if (!string.IsNullOrEmpty(GetCellValue(listRow[keyCellIndex.Key + 3]).ToString().Trim()))
                            {
                                dictdictTestChannelCache[keyCellIndex.Value]["DestinationCycle"] =
                                    GetCellValue(listRow[keyCellIndex.Key + 3]).ToString().Trim();
                            }
                            dictCLRRow["DestNetworkNum[" + intDestNetNum + "].DestinationCycleTime"] =
                                dictdictTestChannelCache[keyCellIndex.Value]["DestinationCycle"];

                            dictCLRRow["DestNetworkNum[" + intDestNetNum + "].DestinationStartBit"] =
                                GetCellValue(listRow[keyCellIndex.Key + 4]).ToString().Trim();
                            dictCLRRow["DestNetworkNum[" + intDestNetNum + "].DestinationBitLength"] =
                                GetCellValue(listRow[keyCellIndex.Key + 5]).ToString().Trim();
                            intDestNetNum++;
                        }
                    }

                    dictCLRRow["DestNetNum"] = intDestNetNum.ToString();
                    _listdictCLRRouteInfo.Add(dictCLRRow);
                }
            }

            //完成后记得减去
            Interlocked.Decrement(ref _intThreadCount);
            if (_intThreadCount == 0)
            {
                _myEvent.Set();
            }
        }

        private object GetCellValue(ICell cell)
        {
            object value = string.Empty;
            try
            {
                if (cell.CellType != CellType.Blank)
                {
                    switch (cell.CellType)
                    {
                        case CellType.Numeric:
                            // Date comes here
                            if (DateUtil.IsCellDateFormatted(cell))
                            {
                                value = cell.DateCellValue;
                            }
                            else
                            {
                                // Numeric type
                                value = cell.NumericCellValue;
                            }
                            break;
                        case CellType.Boolean:
                            // Boolean type
                            value = cell.BooleanCellValue;
                            break;
                        case CellType.Formula:
                            value = cell.CellFormula;
                            break;
                        default:
                            // String type
                            value = cell.StringCellValue;
                            break;
                    }
                }
            }
            catch (Exception)
            {
                value = "";
            }
            return value;
        }

        #endregion

        public string CreateDTCini(string folder, string configName, Dictionary<string, string> dictHead,
           List<Dictionary<string, string>> listDtcByte, List<Dictionary<string, string>> listDtc, string dbcPath)
        {
            string error = "";
            int i = 0;
            GetID();
            try
            {
                var folderPath = AppDomain.CurrentDomain.BaseDirectory + "\\configini\\" + "DTC\\";
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                StreamWriter sw = new StreamWriter(folderPath + configName + ".ini", false, Encoding.Default);
                //文件头
                sw.WriteLine("[PathInfo]");
                sw.WriteLine("DBCPathFile=" + AppDomain.CurrentDomain.BaseDirectory + dbcPath);
                WriteDTCHead(ref sw, dictHead);
                WriteDTCMsg(ref sw, listDtcByte);
                WriteDTCHex(ref sw, listDtc);
                sw.Flush();
                sw.Close();

                return error;
            }
            catch (Exception e)
            {
                error = e.ToString();
                return error;
            }
        }

        public string CreateCfginiFromLINS(string folder, string configName, Dictionary<string, string> dictHead,
            Dictionary<string, string> dictNody, List<Dictionary<string, string>> dictLocal, List<string> dictVirNode,
            string dbcPath, bool isMinor)
        {
            string error = "";
            int i = 0;
            try
            {
                var folderPath = AppDomain.CurrentDomain.BaseDirectory + "\\configini\\" + configName + "\\";
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);
                StreamWriter sw = new StreamWriter(folderPath + configName + ".ini",false, Encoding.Default);
                //文件头
                sw.WriteLine("[PathInfo]");
                sw.WriteLine("LDFPathFile=" + AppDomain.CurrentDomain.BaseDirectory + dbcPath);
                WriteDUTHead(ref sw, dictHead); 
                WriteDUTLINNodeToSig(ref sw, dictNody, isMinor, dbcPath);
                //if (isMinor)
                //    WriteVirNode(ref sw, dictVirNode);
                sw.Flush();
                sw.Close();
                return error;
            }
            catch (Exception e)
            {
                error = e.ToString();
                return error;
            }
        }

        public string CreateCfginiFromCANM(string folder, string configName, Dictionary<string, string> dictHead,
            List<Dictionary<string, string>> dictBody, List<List<Dictionary<string, string>>> dictLocal,
            List<string> dictVirNode, string dbcPath)
        {
            string error = "";
            int i = 0;
            try
            {
                var folderPath = AppDomain.CurrentDomain.BaseDirectory + "\\configini\\" + configName + "\\";
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);
                StreamWriter sw = new StreamWriter(folderPath + configName + ".ini", false, Encoding.Default);
                //文件头
                sw.WriteLine("[PathInfo]");
                if (configName.Contains("LIN"))
                {
                    sw.WriteLine("LDFPathFile=" + AppDomain.CurrentDomain.BaseDirectory + dbcPath);
                }
                else
                {
                    sw.WriteLine("DBCPathFile=" + AppDomain.CurrentDomain.BaseDirectory + dbcPath);
                }
                WriteDUTHead(ref sw, dictHead);
                WriteDUTCANNode(ref sw, dictBody);
                WriteLocal(ref sw, dictLocal);
                //WriteVirNode(ref sw, dictVirNode);
                sw.Flush();
                sw.Close();
                return error;
            }
            catch (Exception e)
            {
                error = e.ToString();
                return error;
            }
        }

        public string CreateCfginiFromLINM(string folder, string configName, Dictionary<string, string> dictHead,
            List<Dictionary<string, string>> dictBody, List<List<Dictionary<string, string>>> dictLocal,
            List<string> dictVirNode, string dbcPath)
        {
            string error = "";
            int i = 0;
            try
            {
                var folderPath = AppDomain.CurrentDomain.BaseDirectory + "\\configini\\" + configName + "\\";
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);
                StreamWriter sw = new StreamWriter(folderPath + configName + ".ini", false, Encoding.Default);
                //文件头
                sw.WriteLine("[PathInfo]");
                sw.WriteLine("LDFPathFile=" + AppDomain.CurrentDomain.BaseDirectory + dbcPath);
                WriteDUTHead(ref sw, dictHead);
                WriteDUTLINNode(ref sw, dictBody);
                WriteLocal(ref sw, dictLocal);
                WriteVirNode(ref sw, dictVirNode);
                sw.Flush();
                sw.Close();
                return error;
            }
            catch (Exception e)
            {
                error = e.ToString();
                return error;
            }
        }


        public List<string> GetVirNode(List<string> nodes, string dbcPath)
        {

            GetOSEK osekInfo = new GetOSEK();
            List<string> virNode = new List<string>();
            int exValue = 0;
            foreach (var node in nodes)
            {
                string virNodeStr = osekInfo.GetNMStationAddressFromDBC(ref exValue,
                    AppDomain.CurrentDomain.BaseDirectory + dbcPath, node);
                if (virNodeStr != "")
                    virNode.Add(virNodeStr);
            }
            return virNode;
        }

        public List<string> GetLINVirNode(List<string> nodes, string dbcPath)
        {

            List<string> virNode = new List<string>();
            int exValue = 0;
            foreach (var node in nodes)
            {
                string virNodeStr = _ldf.GetSlaveID(AppDomain.CurrentDomain.BaseDirectory + dbcPath, node);
                if (virNodeStr != "")
                    virNode.Add(virNodeStr);
            }
            return virNode;
        }

        private void WriteDUTHead(ref StreamWriter sw, Dictionary<string, string> dictHead)
        {
            sw.WriteLine("[DUTInfo]");
            string[] names = new string[dictHead.Keys.Count];
            dictHead.Keys.CopyTo(names, 0);
            if (dictHead.Count == 3)
            {      
                foreach (var name in names)
                    sw.WriteLine(name +"=" + dictHead[name]);              
            }
            else
            {
                foreach (var name in names)
                {
                    if(name == "TestSUTNum")
                        continue;
                    sw.WriteLine(name + "=" + dictHead[name]);
                }
            }
        }

        private void WriteDTCHead(ref StreamWriter sw, Dictionary<string, string> dictHead)
        {
            sw.WriteLine("[DUTInfo]");
            sw.WriteLine("DUT.CddFileName=" + dictHead["CddFileName"]);
            sw.WriteLine("DUT.RequestID=" + dictHead["RequestID"]);
            sw.WriteLine("DUT.ResponseID=" + dictHead["RespondID"]);
            sw.WriteLine("DUT.DUT.DiagInitalTime=" + dictHead["InitTimeofDiag"]);
        }

        private void WriteDUTLINNodeToSig(ref StreamWriter sw, Dictionary<string, string> dictBody, bool isLINMinor,
            string dbcPath)
        {
            string slaveID = _ldf.GetSlaveID(AppDomain.CurrentDomain.BaseDirectory + dbcPath, dictBody["Name"]);
            sw.WriteLine("DUT.Name=" + dictBody["Name"]);
            sw.WriteLine("DUT.SystemType=" + dictBody["SystemType"]);
            if (isLINMinor)
            {
                //sw.WriteLine("DUT.SlaveID=" + dictBody["SlaveID"]);
                sw.WriteLine("DUT.Crystal=" + dictBody["Crystal"]);
            }
            ////else { 
            ////    sw.WriteLine("DUT.MasterID=" + dictBody["MasterID"]);
            ////}
            sw.WriteLine("DUT.LocalEventNum=" + dictBody["LocalEventNum"]);
            if (dictBody["LocalEventNum"].ToString().Trim() != "0")
            {
                for (int i=0;i< Convert.ToInt32(dictBody["LocalEventNum"].ToString().Trim());i++)
                {
                    sw.WriteLine("DUT.LocalEvent[" + i + "]=" + dictBody["LocalEvent[" + i + "]"].Remove(0, 2));
                    sw.WriteLine("DUT.LocalEventValid[" + i + "]=" + IsEffective(dictBody["LocalEventValid[" + i + "]"]));
                }
            } 
        }

        private void WriteDTCMsg(ref StreamWriter sw, List<Dictionary<string, string>> listDtc)
        {
            sw.WriteLine("DiagRelateMsg_Num=" + listDtc.Count);
            for (int i = 0; i < listDtc.Count; i++)
            {
                sw.WriteLine("DiagRelateMsg_ID[" + i + "]=" + listDtc[i]["MessageID"]);
                sw.WriteLine("DiagRelateMsg_Cyele[" + i + "]=" + listDtc[i]["Period"]);
                sw.WriteLine("DiagRelateMsg_Byte[" + i + "][0]=" + listDtc[i]["ByteFirst"]);
                sw.WriteLine("DiagRelateMsg_Byte[" + i + "][1]=" + listDtc[i]["ByteSecond"]);
                sw.WriteLine("DiagRelateMsg_Byte[" + i + "][2]=" + listDtc[i]["ByteThird"]);
                sw.WriteLine("DiagRelateMsg_Byte[" + i + "][3]=" + listDtc[i]["ByteFourth"]);
                sw.WriteLine("DiagRelateMsg_Byte[" + i + "][4]=" + listDtc[i]["ByteFive"]);
                sw.WriteLine("DiagRelateMsg_Byte[" + i + "][5]=" + listDtc[i]["ByteSixth"]);
                sw.WriteLine("DiagRelateMsg_Byte[" + i + "][6]=" + listDtc[i]["ByteSeventh"]);
                sw.WriteLine("DiagRelateMsg_Byte[" + i + "][7]=" + listDtc[i]["ByteEighth"]);
            }
        }
        private void WriteDTCHex(ref StreamWriter sw, List<Dictionary<string, string>> listDtc)
        {
            sw.WriteLine("TestCaseNum=" + listDtc.Count);
            sw.WriteLine("[DiagTestInfo]");

            for (int i = 0; i < listDtc.Count; i++)
            {
                sw.WriteLine("TC_DTC[" + i + "].name=" + listDtc[i]["name"]);
                sw.WriteLine("TC_DTC[" + i + "].DTCHEX=" + listDtc[i]["DTCHEX"]);
                sw.WriteLine("TC_DTC[" + i + "].Type=" + dictDtc[listDtc[i]["FaultType"]]);
                switch (dictDtc[listDtc[i]["FaultType"]])
                {
                    case 1:
                        string[] value = listDtc[i]["DTCMessage"].Split(',');
                        sw.WriteLine("TC_DTC[" + i + "].LowVoltagemin=" + value[0]);
                        sw.WriteLine("TC_DTC[" + i + "].LowVoltagemax=" + value[1]);
                        sw.WriteLine("\r\n");
                        break;
                    case 2:
                        string[] valueU = listDtc[i]["DTCMessage"].Split(',');
                        sw.WriteLine("TC_DTC[" + i + "].UpVoltagemin=" + valueU[0]);
                        sw.WriteLine("TC_DTC[" + i + "].UpVoltagemax=" + valueU[1]);
                        sw.WriteLine("\r\n");
                        break;
                    case 3:
                        sw.WriteLine("TC_DTC[" + i + "].LostMsg=" + listDtc[i]["DTCMessage"]);
                        sw.WriteLine("\r\n");
                        break;
                    case 4:
                        sw.WriteLine("TC_DTC[" + i + "].LostDUT=" + listDtc[i]["DTCMessage"]);
                        sw.WriteLine("\r\n");
                        break;
                    case 5:
                        string[] valueS = listDtc[i]["DTCMessage"].Split(',');
                        sw.WriteLine("TC_DTC[" + i + "].InvaildSg=" + valueS[0]);
                        sw.WriteLine("TC_DTC[" + i + "].InvaildSgValue=" + valueS[1]);
                        sw.WriteLine("\r\n");
                        break;
                    case 6:
                        sw.WriteLine("TC_DTC[" + i + "].BusOffNum=" + listDtc[i]["DTCMessage"]);
                        sw.WriteLine("\r\n");
                        break;
                    case 7:
                        sw.WriteLine("\r\n");
                        break;
                    case 8:
                        sw.WriteLine("\r\n");
                        break;
                    case 9:
                        sw.WriteLine("TC_DTC[" + i + "].SwitchTime=" + listDtc[i]["DTCMessage"]);
                        sw.WriteLine("\r\n");
                        break;
                }
            }
        }

        private void GetID()
        {
            IList<object[]> dtcTypes = _store.GetRegularByEnum(EnumLibrary.EnumTable.FaultType);
            int i = 1;
            foreach (object[] type in dtcTypes)
            {
                if (!dictDtc.ContainsKey(type[0].ToString()))
                    dictDtc.Add(type[0].ToString(), i);
                i++;
            }
        }


        private void WriteDUTCANNodeToSig(ref StreamWriter sw, Dictionary<string, string> dictNody)
        {
            string[] names = new string[dictNody.Keys.Count];
            dictNody.Keys.CopyTo(names, 0);
            foreach (var name in names)
            {
                if (name == "TerminalR")
                {
                    sw.WriteLine("DUT.TerminalR=" + IsorNotResistance(dictNody["TerminalR"]));
                    continue;
                }
                if (name == "NodeType")
                {
                    sw.WriteLine("DUT.NodeType=" + Network(dictNody["NodeType"]));
                    continue;
                }
                if (name == "EngineStartRelated")
                {
                    sw.WriteLine("DUT.EngineStartRelated=" + Related(dictNody["EngineStartRelated"]));
                    continue;
                }
                if (name.Contains("CRCType"))
                {
                    sw.WriteLine("DUT." + name + "=" + IsCRCType(dictNody[name]));
                    continue;
                }
                if (name.Contains("LocalEvent["))
                {
                    sw.WriteLine("DUT." + name + "=" + dictNody[name].Remove(0, 2));
                    continue;
                }
                if (name.Contains("LocalEventValid"))
                {
                    sw.WriteLine("DUT." + name + "=" + IsEffective(dictNody[name]));
                    continue;
                }
                
                sw.WriteLine("DUT."+ name +"=" + dictNody[name]);
            }
            //sw.WriteLine("DUT.Name=" + dictNody["Name"]);
            //sw.WriteLine("DUT.SlaveboxID=" + dictNody["SlaveboxID"]);
            //sw.WriteLine("DUT.TerminalR=" + IsorNotResistance(dictNody["TerminalR"]));
            //sw.WriteLine("DUT.NodeType=" + Network(dictNody["NodeType"]));
            //sw.WriteLine("DUT.EngineStartRelated=" + dictNody["EngineStartRelated"]);
            //sw.WriteLine("DUT.NMBaseAddress=" + dictNody["NMBaseAddress"]);
            //sw.WriteLine("DUT.NMStationAddress=" + dictNody["NMStationAddress"]);
        }

        private void WriteDUTCANNode(ref StreamWriter sw, List<Dictionary<string, string>> dictBody)
        {
            for (int i = 0; i < dictBody.Count; i++)
            {
                string[] names = new string[dictBody[i].Keys.Count];
                dictBody[i].Keys.CopyTo(names, 0);
                foreach (var name in names)
                {
                    if (name == "TerminalR")
                    {
                        sw.WriteLine("DUT[" + i + "].TerminalR=" + IsorNotResistance(dictBody[i]["TerminalR"]));
                        continue;
                    }
                    if (name == "NodeType")
                    {
                        sw.WriteLine("DUT[" + i + "].NodeType=" + Network(dictBody[i]["NodeType"]));
                        continue;
                    }
                    if (name == "EngineStartRelated")
                    {
                        sw.WriteLine("DUT[" + i + "].EngineStartRelated=" + Related(dictBody[i]["EngineStartRelated"]));
                        continue;
                    }
                    if (name == "CRCType")
                    {
                        sw.WriteLine("DUT[" + i + "].CRCType=" + IsCRCType(dictBody[i]["CRCType"]));
                        continue;
                    }
                    if (name == "Type")
                    {
                        sw.WriteLine("DUT[" + i + "].Type=" + NodeType(dictBody[i]["Type"]));
                        continue;
                    }
                    sw.WriteLine("DUT[" + i + "]." + name + "=" + dictBody[i][name]);
                }
                //sw.WriteLine("DUT[" + i + "].Name=" + dictBody[i]["Name"]);
                //sw.WriteLine("DUT[" + i + "].SlaveboxID=" + dictBody[i]["SlaveboxID"]);
                //sw.WriteLine("DUT[" + i + "].TerminalR=" + IsorNotResistance(dictBody[i]["TerminalR"]));
                //sw.WriteLine("DUT[" + i + "].NodeType" + Network(dictBody[i]["NodeType"]));
                //sw.WriteLine("DUT[" + i + "].EngineStartRelated=" + dictBody[i]["EngineStartRelated"]);
                //sw.WriteLine("DUT[" + i + "].NMBaseAddress=" + dictBody[i]["NMBaseAddress"]);
                //sw.WriteLine("DUT[" + i + "].NMStationAddress=" + dictBody[i]["NMStationAddress"]);
            }
        }

        private void WriteDUTLINNode(ref StreamWriter sw, List<Dictionary<string, string>> dictBody)
        {
            for (int i = 0; i < dictBody.Count; i++)
            {
                string[] names = new string[dictBody[i].Keys.Count];
                dictBody[i].Keys.CopyTo(names, 0);
                foreach (var name in names)
                {
                    if (name == "Crystal")
                    {
                        sw.WriteLine("DUT.Crystal=" + IsJZ(dictBody[i]["Crystal"]));
                        continue;
                    }
                    if (name == "MasterType")
                    {
                        sw.WriteLine("DUT.MasterType=" + NodeType(dictBody[i]["MasterType"]));
                    }

                    sw.WriteLine("DUT[" + i + "]." + name + "=" + dictBody[i][name]);
                }

                
             
                //sw.WriteLine("DUT[" + i + "].Name=" + dictBody[i]["Name"]);
                //sw.WriteLine("DUT[" + i + "].SlaveboxID=" + dictBody[i]["SlaveboxID"]);
                //sw.WriteLine("DUT[" + i + "].Crystal=" + dictBody[i]["Crystal"]);
                //sw.WriteLine("DUT[" + i + "].MasterType=" + dictBody[i]["MasterType"]);

            }
        }

        private void WriteLocalToSig(ref StreamWriter sw, List<Dictionary<string, string>> dictLocal)
        {
            sw.WriteLine("DUT.LocalEventNum=" + dictLocal.Count);
            for (int i = 0; i < dictLocal.Count; i++)
            {
                string[] names = new string[dictLocal[i].Keys.Count];
                dictLocal[i].Keys.CopyTo(names, 0);
                foreach (var name in names)
                {
                    if (name == "LocalEventIO")
                    {
                        sw.WriteLine("DUT.LocalEvent[" + i + "].LocalEventIO=" + dictLocal[i]["LocalEventIO"].Remove(0, 2));
                        continue;
                    }
                    if (name == "EnableLevel")
                    {
                        sw.WriteLine("DUT.Localevent[" + i + "].EnableLevel=" + IsEffective(dictLocal[i]["EnableLevel"]));
                        continue;
                    }
                    sw.WriteLine("DUT[" + i + "]." + name + "=" + dictLocal[i][name]);
                }
                //sw.WriteLine("DUT.LocalEvent[" + i + "].LocalEventIO=" + dictLocal[i]["LocalEventIO"].Remove(0,2));
                //sw.WriteLine("DUT.Localevent[" + i + "].EnableLevel=" + IsEffective(dictLocal[i]["EnableLevel"]));
                //sw.WriteLine("DUT.Localevent[" + i + "].LocalEventName=" + dictLocal[i]["LocalEventName"]);
            }

        }

        private void WriteLocal(ref StreamWriter sw, List<List<Dictionary<string, string>>> dictLocal)
        {
            for (int i = 0; i < dictLocal.Count; i++)
            {
                sw.WriteLine("DUT[" + i + "].LocalEventNum=" + dictLocal[i].Count);
                for (int j = 0; j < dictLocal[i].Count; j++)
                {
                    string[] names = new string[dictLocal[i][j].Keys.Count];
                    dictLocal[i][j].Keys.CopyTo(names, 0);
                    foreach (var name in names)
                    {
                        if (name == "LocalEventIO")
                        {
                            sw.WriteLine("DUT[" + i + "].LocalEvent[" + j + "]=" + dictLocal[i][j]["LocalEventIO"].Remove(0, 2));
                            continue;
                        }
                        if (name == "EnableLevel")
                        {
                            sw.WriteLine("DUT[" + i + "].LocalEventValid[" + j + "]=" + IsEffective(dictLocal[i][j]["EnableLevel"]));
                            continue;
                        }
                            //sw.WriteLine("DUT[" + i + "].LocalEvent[" + j + "]." + name + "=" + dictLocal[i][j][name]);
                    }
                }
            }
        }

        private void WriteVirNode(ref StreamWriter sw, List<string> dictVirNode)
        {
            sw.WriteLine("DUT.VirNodeNum=" + dictVirNode.Count);
            for (int i = 0; i < dictVirNode.Count; i++)
            {
                sw.WriteLine("DUT.VirNodeID[" + i + "]=" + dictVirNode[i]);
            }
        }

        #endregion

        #region 报告相关

        public Dictionary<string, List<List<string>>> AnalysisXml(string xmlPath)
        {
            try
            {
                Dictionary<string, List<List<string>>> dictReport = new Dictionary<string, List<List<string>>>();

                //得到根节点
                XmlDocument doc = new XmlDocument();
                doc.Load(xmlPath);
                XmlElement rootElem = doc.DocumentElement;
                if (rootElem == null) return null;

                XmlNodeList testcaseNodes = rootElem.GetElementsByTagName("testcase");

                foreach (XmlNode testcaseNode in testcaseNodes)
                {

                    XmlNodeList desNodes = testcaseNode.SelectNodes("teststep//tabularinfo//description");
                    XmlNodeList rowNodes = testcaseNode.SelectNodes("teststep//tabularinfo//row");
                    string title = testcaseNode.SelectNodes("ident")[0].InnerText;
                    XmlNodeList resultNodes = testcaseNode.SelectNodes("teststep");
                    List<List<string>> rowsContent = new List<List<string>>();
                    int j = 0;
                    List<string> desList = new List<string>();
                    List<string> resultList = new List<string>();
                    foreach (XmlNode des in desNodes)
                    {
                        desList.Add(des.InnerText);
                    }

                    foreach (XmlNode res in resultNodes)
                    {
                        resultList.Add(res.Attributes[4].InnerText);
                    }

                    foreach (XmlNode row in rowNodes)
                    {
                        List<string> celList = new List<string>();
                        celList.Add(desList[j]);
                        //celList.Add(resultList[j]);
                        for (int i = 0; i < row.ChildNodes.Count; i++)
                        {
                            string name = row.ChildNodes[i].InnerText;
                            celList.Add(name);
                        }
                        celList.Add(resultList[j]);
                        rowsContent.Add(celList);
                        j++;
                    }

                    dictReport.Add(title, rowsContent);


                }
                return dictReport;
            }
            catch (Exception ex)
            {
                return null;
            }

        }


        public Dictionary<string, List<Dictionary<string, List<List<string>>>>> DerReportToNewDict(
            Dictionary<string, List<List<string>>> dictReport,
            Dictionary<string, Dictionary<string, List<object>>> dictExap)
        {
            Dictionary<string, string> ExampleJsonList = new Dictionary<string, string>();


            Dictionary<string, List<Dictionary<string, List<List<string>>>>> newReport =
                new Dictionary<string, List<Dictionary<string, List<List<string>>>>>();

            foreach (KeyValuePair<string, Dictionary<string, List<object>>> ExapOne in dictExap)
            {
                var ExapToOne = ExapOne.Value.ToArray();
                List<Dictionary<string, List<List<string>>>> newList =
                    new List<Dictionary<string, List<List<string>>>>();
                foreach (KeyValuePair<string, List<object>> ExapTwo in ExapToOne)
                {
                    var ExapToTwo = ExapTwo.Value[0];

                    ExampleJsonList = Json.DerJsonToDict(ExapToTwo.ToString());
                    string key = ExampleJsonList["ExapID"];
                    foreach (KeyValuePair<string, List<List<string>>> dictreport in dictReport)
                    {
                        var report = dictreport.Key;
                        if (key == report)
                        {

                            Dictionary<string, List<List<string>>> dictRep =
                            new Dictionary<string, List<List<string>>>();
                            string name = ExampleJsonList["ExapID"] + " " + ExampleJsonList["ReflectionID"] + " " + ExampleJsonList["ExapName"]; ;
                            dictRep.Add(name , dictreport.Value);
                            newList.Add(dictRep);
                            //newReport[ExapOne.Key.ToString()] = newList;
                            break;
                        }
                    }

                }
                newReport[ExapOne.Key.ToString()] = newList;
            }
            return newReport;
        }

        #endregion



        #region 压缩和解压

        /// <summary>
        /// 在软件根目录下创建指定文件夹
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        public string CreateFolder(string folder)
        {
            var path = AppDomain.CurrentDomain.BaseDirectory + folder + @"\";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
        /// <summary>
        /// 解压文件
        /// </summary>
        /// <param name="targetFile">目标路径</param>
        /// <param name="fileDir">原路径</param>
        /// <returns></returns>
        public string UnZipFile(string targetFile, string fileDir)
        {
            var rootFile = " ";
            try
            {
                //读取压缩文件(zip文件)，准备解压缩
                var s = new ZipInputStream(File.OpenRead(targetFile.Trim()));
                ZipEntry theEntry;
                var path = fileDir;
                //解压出来的文件保存的路径

                //根目录下的第一个子文件夹的名称
                while ((theEntry = s.GetNextEntry()) != null)
                {
                    var rootDir = Path.GetDirectoryName(theEntry.Name);
                    //得到根目录下的第一级子文件夹的名称
                    if (rootDir.IndexOf("\\", StringComparison.Ordinal) >= 0)
                    {
                        rootDir = rootDir.Substring(0, rootDir.IndexOf("\\", StringComparison.Ordinal) + 1);
                    }
                    var dir = Path.GetDirectoryName(theEntry.Name);
                    //根目录下的第一级子文件夹的下的文件夹的名称
                    var fileName = Path.GetFileName(theEntry.Name);
                    //根目录下的文件名称
                    if (dir != " ")
                        //创建根目录下的子文件夹,不限制级别
                    {
                        if (!Directory.Exists(fileDir + "\\" + dir))
                        {
                            path = fileDir + "\\" + dir;
                            //在指定的路径创建文件夹
                            Directory.CreateDirectory(path);
                        }
                    }
                    else if (dir == " " && fileName != "")
                        //根目录下的文件
                    {
                        path = fileDir;
                        rootFile = fileName;
                    }
                    else if (dir != " " && fileName != "")
                        //根目录下的第一级子文件夹下的文件
                    {
                        if (dir.IndexOf("\\", StringComparison.Ordinal) > 0)
                            //指定文件保存的路径
                        {
                            path = fileDir + "\\" + dir;
                        }
                    }

                    if (dir == rootDir)
                        //判断是不是需要保存在根目录下的文件
                    {
                        path = fileDir + "\\" + rootDir;
                    }

                    //以下为解压缩zip文件的基本步骤
                    //基本思路就是遍历压缩文件里的所有文件，创建一个相同的文件。
                    if (fileName != String.Empty)
                    {
                        FileStream streamWriter = File.Create(path + "\\" + fileName);

                        byte[] data = new byte[2048];
                        while (true)
                        {
                            var size = s.Read(data, 0, data.Length);
                            if (size > 0)
                            {
                                streamWriter.Write(data, 0, size);
                            }
                            else
                            {
                                break;
                            }
                        }

                        streamWriter.Close();
                    }
                }
                s.Close();

                return rootFile;
            }
            catch (Exception ex)
            {
                return "1; " + ex.Message;
            }
        }



        public void ZipFile(string strFile, string strZip)
        {
            if (strFile[strFile.Length - 1] != Path.DirectorySeparatorChar)
                strFile += Path.DirectorySeparatorChar;
            ZipOutputStream s = new ZipOutputStream(File.Create(strZip));
            s.SetLevel(6); // 0 - store only to 9 - means best compression
            Zip(strFile, s, strFile);
            s.Finish();
            s.Close();
        }

        private void Zip(string strFile, ZipOutputStream s, string staticFile)
        {
            if (strFile[strFile.Length - 1] != Path.DirectorySeparatorChar) strFile += Path.DirectorySeparatorChar;
            Crc32 crc = new Crc32();
            string[] filenames = Directory.GetFileSystemEntries(strFile);
            foreach (string file in filenames)
            {

                if (Directory.Exists(file))
                {
                    Zip(file, s, staticFile);
                }

                else // 否则直接压缩文件
                {
                    //打开压缩文件
                    FileStream fs = File.OpenRead(file);

                    byte[] buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Length);
                    string tempfile = file.Substring(staticFile.LastIndexOf("\\", StringComparison.Ordinal) + 1);
                    ZipEntry entry = new ZipEntry(tempfile)
                    {
                        DateTime = DateTime.Now,
                        Size = fs.Length
                    };

                    fs.Close();
                    crc.Reset();
                    crc.Update(buffer);
                    entry.Crc = crc.Value;
                    s.PutNextEntry(entry);

                    s.Write(buffer, 0, buffer.Length);
                }
            }
        }

        #endregion

        #region 读取本地xml指定节点的值

        /// <summary>
        ///     读取本地xml指定节点的值(返回为空时,读取失败)
        /// </summary>
        /// <param name="xmlName">xml文件名称</param>
        /// <param name="nodeStr">指点节点</param>
        /// <returns>返回节点值</returns>
        public string ReadLocalXml(string xmlName, string nodeStr)
        {

            try
            {
                Doc.Load(AppDomain.CurrentDomain.BaseDirectory + xmlName); //加载本地文件
                //分析文件
                var nodeValue = Doc.SelectSingleNode("//" + nodeStr);
                if (nodeValue == null) return string.Empty;
                var value = nodeValue.InnerText.Trim() == "无" ? string.Empty : nodeValue.InnerText.Trim();
                return value;
            }
            catch(Exception ex)
            {
                return string.Empty;
            }
        }

        #endregion

        #region 更改本地XML指定节点的值

        /// <summary>
        ///     更改本地XML指定节点的值
        /// </summary>
        /// <param name="xmlName">xml文档的名称</param>
        /// <param name="nodeStr">指定节点</param>
        /// <param name="value">节点的值</param>
        /// <returns>返回更改成功与否</returns>
        public bool UpdateLocalXml(string xmlName, string nodeStr, string value)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(AppDomain.CurrentDomain.BaseDirectory + xmlName);
                XmlNode xnuser = doc.SelectSingleNode("//" + nodeStr);
                if (xnuser != null) xnuser.FirstChild.InnerText = value == string.Empty ? "无" : value;
                doc.Save(AppDomain.CurrentDomain.BaseDirectory + xmlName);
                return true;
            }
            catch(Exception e)
            {
                return false;
            }
        }

        #endregion

        #region  十进制 转 十六进制

        /// <summary>
        ///     十进制 转 十六进制
        /// </summary>
        /// <param name="decimalNum">十进制整数</param>
        /// <returns>十六进制字符串</returns>
        public string DecToHex(int decimalNum)
        {
            string s = decimalNum.ToString("X2");
            if (s.Length%2 == 0)
            {
                return s;
            }
            s = "0" + s;
            return s;
        }

        #endregion

        /// <summary>
        /// 从自检结果的.txt文件中得到本次自检结果
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="selfResult"></param>
        /// <returns></returns>
        public Dictionary<string, List<string>> ReadSelfCheckResultFormTxt(string filePath, ref Dictionary<string, bool> dictselfResult)
        {
            StreamReader sr = new StreamReader(filePath, Encoding.Default);
            Dictionary<string, List<string>> drSelf = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> drDevSelf = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> drDUTSelf = new Dictionary<string, List<string>>();
            //sr.BaseStream.Seek(0, SeekOrigin.Begin);
            string nextLine;

            bool flag = true;
            while ((nextLine = sr.ReadLine()) != null)
            {
                if(nextLine.Trim()==string.Empty)
                    continue;
                string selfCheckStr = nextLine;
                if (selfCheckStr.ToLower().Contains("finalresults"))//FinalResults
                {
                    string testResult = selfCheckStr.Split('=')[1];
                    if (!testResult.ToLower().Contains("normal"))//Normal
                    {
                        if (flag)
                            dictselfResult["Devinfo"] = false;
                        else
                            dictselfResult["DUTState"] = false;
                    }   
                    continue;
                }
                if (nextLine.ToLower() == "[devinfo]")//Devinfo
                {
                    flag = true;
                    continue;
                }
                if (nextLine.ToLower() == "[dutstate]")//DUTState
                {
                    flag = false;
                    continue;
                }
                if (flag)
                {
                    List<string> list = new List<string>();
                    string content = sr.ReadLine();
                    string result = sr.ReadLine();
                    list.Add(content.Split('=')[1]);
                    list.Add(result.Split('=')[1]);
                    drSelf.Add(nextLine.Split('=')[1] + "-" + content.Split('=')[1], list);
                    drDevSelf.Add(nextLine.Split('=')[1] + "-" + content.Split('=')[1], list);
                }
                else
                {
                    List<string> list = new List<string>();
                    string state = sr.ReadLine();
                    if (state != null) list.Add(state.Split('=')[1]);
                    drSelf.Add(nextLine.Split('=')[1], list);
                    drDUTSelf.Add(nextLine.Split('=')[1], list);
                }   
            }

            //if (dictselfResult["Devinfo"] == false && dictselfResult["DUTState"] == false)
            //{
            //    return drSelf;
            //}
            //else if (dictselfResult["Devinfo"] == false)
            //{
            //    return drDevSelf;
            //}
            //else if (dictselfResult["DUTState"] == false)
            //{
            //    return drDUTSelf;
            //}
            return drSelf;
        }
        /// <summary>
        /// 拷贝文件夹
        /// </summary>
        /// <param name="srcdir"></param>
        /// <param name="desdir"></param>
        public void CopyDirectory(string srcdir, string desdir)
        {
            string folderName = srcdir.Substring(srcdir.LastIndexOf("\\") + 1);
            string desfolderdir = desdir + "\\" + folderName;
            if (desdir.LastIndexOf("\\") == (desdir.Length - 1))
            {
                desfolderdir = desdir + folderName;
            }
            string[] filenames = Directory.GetFileSystemEntries(srcdir);
            foreach (string file in filenames)// 遍历所有的文件和目录
            {
                if (Directory.Exists(file))// 先当作目录处理如果存在这个目录就递归Copy该目录下面的文件
                {
                    string currentdir = desfolderdir + "\\" + file.Substring(file.LastIndexOf("\\") + 1);
                    if (!Directory.Exists(currentdir))
                    {
                        Directory.CreateDirectory(currentdir);
                    }
                    CopyDirectory(file, desfolderdir);
                }
                else // 否则直接copy文件
                {
                    string srcfileName = file.Substring(file.LastIndexOf("\\") + 1);
                    srcfileName = desfolderdir + "\\" + srcfileName;
                    if (!Directory.Exists(desfolderdir))
                    {
                        Directory.CreateDirectory(desfolderdir);
                    }
                    File.Copy(file, srcfileName);
                }
            }//foreach 
        }//function end


        #region 标识符转化
        public string DBCCANConvert(string node)
        {
            string CAN = "CAN总线&J1939总线&网关路由";
            string LIN = "LIN总线";
            switch (node)
            {
                case "CAN":
                    return CAN;
                case "LIN":
                    return LIN;
                default:
                    return "";
            }
        }
        public string DBCCANConvertTestType(string node)
        {
            string CAN = "CAN通信单元&CAN通信集成&直接NM单元&直接NM集成&动力域NM主节点&动力域NM从节点&动力域NM集成&间接NM单元&间接NM集成&通信DTC&OSEK NM单元&OSEK NM集成&Bootloader&网关路由";
            string LIN = "LIN通信主节点&LIN通信从节点&LIN通信集成";
            switch (node)
            {
                case "CAN":
                    return CAN;
                case "LIN":
                    return LIN;
                default:
                    return "";
            }
        }
        
        public int CANConvertToType(string type)
        {
            switch (type)
            {
                case "CAN通信单元":
                case "直接NM单元":
                case "动力域NM主节点":
                case "动力域NM从节点":
                case "间接NM单元":
                case "OSEK NM单元":
                case "Bootloader":
                case "通信DTC":
                    return 0;
                case "LIN通信主节点":
                case "LIN通信从节点":
                    return 1;
                case "网关路由":
                    return 2;
                case "CAN通信集成":
                case "LIN通信集成":
                case "直接NM集成"://AUTOSAR NM集成
                case "动力域NM集成":
                case "间接NM集成":
                    return 3;
                default:
                    return -1;
            }
        }

        public int IsorNotResistance(string type)
        {
            switch (type)
            {
                case "有终端电阻":
                    return 1;
                case "无终端电阻":
                    return 0;
                default:
                    return -1;

            }
        }

        public int Network(string type)
        {
            switch (type)
            {
                case "直接网络管理":
                case "直接NM":
                case "动力域主NM":
                case "动力域从NM":
                    return 1;
                case "间接网络管理":
                case "间接NM":
                    return 0;
                default:
                    return -1;

            }
        }

        public int IsCRCType(string type)
        {
            switch (type)
            {
                case "CRC8":
                    return 0;
                case "字节异或":
                    return 1;
                case "高四位异或":
                    return 2;
                default:
                    return -1;
            }
        }
        public int Related(string type)
        {
            switch (type)
            {
                case "相关":
                    return 1;
                case "无关":
                    return 0;
                default:
                    return -1;

            }
        }

        public int IsEffective(string type)
        {
            switch (type)
            {
                case "高有效":
                    return 1;
                case "低有效":
                    return 0;
                default:
                    return -1;
            }
        }

        public int NodeType(string type)
        {
            switch (type)
            {
                case "主节点":
                    return 1;
                case "从节点":
                    return 0;
                default:
                    return -1;
            }
        }

        public int IsJZ(string type)
        {
            switch (type)
            {
                case "有晶振":
                    return 1;
                case "无晶振":
                    return 0;
                default:
                    return -1;
            }
        }
        #endregion


        #region 一键上传功能
        /// <summary>
        /// 实现ftp文件上传功能
        /// </summary>
        public  string UpLoadFile(Dictionary<string,string>  dictUpload,string localPath)
        {
            // 获得文件流
            FileInfo fileInfo = new FileInfo(localPath);
            try
            {

                //检查路径参数
                if (dictUpload["UploadPath"] == "")
                    return "1";
                //检查Ip地址和端口
                if (dictUpload["IP"] == "")
                    return "2";
                if (dictUpload["Port"] == "")
                    return "3";
                //检查用户名和密码
                if (dictUpload["User"] == "")
                    return "4";
                if (dictUpload["Password"] == "")
                    return "5";

                string fileName = fileInfo.Name;
                //设置FTP协议
                var reqFtp = (FtpWebRequest)WebRequest.Create(new Uri("ftp://" + dictUpload["IP"] + ":" + dictUpload["Port"] + "//" + dictUpload["UploadPath"] + "//" + fileName));
                reqFtp.Method = WebRequestMethods.Ftp.UploadFile;
                reqFtp.UseBinary = true;

                reqFtp.Credentials = new NetworkCredential(dictUpload["User"], dictUpload["Password"]);



                FileStream fs = fileInfo.OpenRead();
                Stream ftpStream = reqFtp.GetRequestStream();

                const int bufferSize = 2048;
                byte[] buffer = new byte[bufferSize];


                // 开始上传
                if (ftpStream != null)
                {
                    int readCount = fs.Read(buffer, 0, bufferSize);
                    while (readCount > 0)
                    {
                        ftpStream.Write(buffer, 0, readCount);
                        readCount = fs.Read(buffer, 0, bufferSize);
                    }
                }
                if (ftpStream != null) ftpStream.Close();
                fs.Close();
                return "8";
            }
            catch (Exception ex)
            {
                return "10";
            }
        }
        #endregion


        #region 判断文件夹内是否有xml

        /// <summary>
        /// 获取文件夹内是否有指定格式文件，如没有返回值则为空。内部会自行判断传进来的是否为文件夹，如非文件夹则会返回上一级查找
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="fileExtension">要筛选的文件后缀名</param>
        /// <returns></returns>
        public string IfFolderExistSiftExtension(string path, string fileExtension)
        {
            string dirPath = path;
            dirPath = IfFolderExist(dirPath);
            if (string.IsNullOrEmpty(dirPath))
                return string.Empty;
            DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
            var filesPath = dirInfo.GetFiles("*." + fileExtension);
            string filePath = filesPath.Length > 0 ? filesPath[0].FullName : string.Empty;
            return filePath;
        }

        private string IfFolderExist(string path)
        {
            string dirPath = path;
            try
            {
                if (!Directory.Exists(path))
                {
                    dirPath = IfFolderExist(Path.GetDirectoryName(path));
                }
            }
            catch (Exception e)
            {
                dirPath = string.Empty;
            }
            return dirPath;
        }
        #endregion

        #region Excel表报告解析相关
        public Dictionary<string, List<List<string>>> AnalysisXmlReport(string xmlPath)
        {
            try
            {
                Dictionary<string, List<List<string>>> dictReport = new Dictionary<string, List<List<string>>>();
                Dictionary<string, Dictionary<int, List<List<string>>>> dictAllReportInfo =
                    new Dictionary<string, Dictionary<int, List<List<string>>>>();
                //得到根节点
                XmlDocument doc = new XmlDocument();
                doc.Load(xmlPath);
                XmlElement rootElem = doc.DocumentElement;
                if (rootElem == null) return null;

                XmlNodeList testcaseNodes = rootElem.GetElementsByTagName("testcase");

                foreach (XmlNode testcaseNode in testcaseNodes)
                {

                    XmlNodeList desNodes = testcaseNode.SelectNodes("teststep//tabularinfo//description");
                    XmlNodeList rowNodes = testcaseNode.SelectNodes("teststep//tabularinfo//row");

                    int iTestIndex = 1;
                    var titleName = testcaseNode.SelectNodes("title").Count > 0
                        ? testcaseNode.SelectNodes("title")[0].InnerText
                        : string.Empty; //获取测试用例名称
                    if (titleName.Split('@').Length >= 2)
                    {
                        int.TryParse(titleName.Split('@')[1], out iTestIndex); //获取此用例名称为第几次测试
                    }
                    #region 用于停止后生成报告，解析时发现ident没有元素时 此条数据废弃
                    var titleList = testcaseNode.SelectNodes("ident");
                    if (titleList.Count <= 0)
                    {
                        continue;
                    }
                    #endregion
                    string title = testcaseNode.SelectNodes("ident")[0].InnerText;
                    XmlNodeList resultNodes = testcaseNode.SelectNodes("teststep");
                    List<List<string>> rowsContent = new List<List<string>>();
                    int j = 0;
                    List<string> desList = new List<string>();
                    List<string> resultList = new List<string>();
                    foreach (XmlNode des in desNodes)
                    {
                        desList.Add(des.InnerText);
                    }

                    foreach (XmlNode res in resultNodes)
                    {
                        if (res.SelectNodes("tabularinfo//description").Count > 0)//如当前testcase下没有tabularinfo//description节点，则舍弃该数据
                        {
                            resultList.Add(res.Attributes[4].InnerText);
                        }
                    }

                    foreach (XmlNode row in rowNodes)
                    {
                        List<string> celList = new List<string>();
                        celList.Add(desList[j]);
                        for (int i = 0; i < row.ChildNodes.Count; i++)
                        {
                            string name = row.ChildNodes[i].InnerText;
                            celList.Add(name);
                        }

                        celList.Add(resultList[j]);

                        #region 删除第一个内容，判断List内有几个相同项

                        celList.RemoveAt(0);
                        //=>为Lambda表达式，括号内相当于(delegate (string str) { return str.Equals(celList[0]); })
                        List<string> ListSame = celList.FindAll(str => str.Equals(celList[0]));
                        if (ListSame.Count >= 3)
                        {
                            celList[celList.Count - 1] = "merge";
                            //continue;
                        }
                        else
                        {
                            switch (celList[celList.Count - 1].Trim().ToLower())
                            {
                                case "pass":
                                    celList[celList.Count - 1] = "OK";
                                    break;
                                case "fail":
                                    celList[celList.Count - 1] = "NOK";
                                    break;
                                case "warn":
                                    celList[celList.Count - 1] = "WARN";
                                    break;
                                case "n/t":
                                    celList[celList.Count - 1] = "N/T";
                                    break;
                            }
                        }

                        #endregion

                        rowsContent.Add(celList);
                        j++;
                    }

                    if (!dictAllReportInfo.ContainsKey(title))
                        dictAllReportInfo[title] = new Dictionary<int, List<List<string>>>();
                    if (!dictAllReportInfo[title].ContainsKey(iTestIndex))
                        dictAllReportInfo[title][iTestIndex] = new List<List<string>>();
                    dictAllReportInfo[title][iTestIndex] = rowsContent;
                }

                dictReport = ReportDataCleaning(dictAllReportInfo);
                string jsonReport = Json.SerJson(dictReport);
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "log\\ReportJson.json", jsonReport);
                return dictReport;

            }
            catch (Exception ex)
            {
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "log\\ReportJson.json", ex.ToString());
                return new Dictionary<string, List<List<string>>>();
            }

        }
        /// <summary>
        /// 筛选出需要生成报告的数据
        /// </summary>
        /// <param name="dictdictListListData"></param>
        /// <returns></returns>
        private Dictionary<string, List<List<string>>> ReportDataCleaning(Dictionary<string, Dictionary<int, List<List<string>>>> dictdictListListData)
        {
            Dictionary<string, List<List<string>>> dictReport = new Dictionary<string, List<List<string>>>();
            foreach (var keydictListListData in dictdictListListData)
            {
                Dictionary<int, string> dictListResult = new Dictionary<int, string>();
                foreach (var keyListList in keydictListListData.Value)
                {
                    string strResult = string.Empty;
                    foreach (var ListList in keyListList.Value)
                    {
                        if (ListList[3].ToUpper() == "MERGE")
                            continue;
                        if (strResult == string.Empty)
                        {
                            strResult = ListList[3].ToUpper();
                        }
                        else
                        {
                            if (ListList[3].ToUpper() == "WARN")
                            {
                                strResult = strResult.ToUpper() != "NOK" ? ListList[3].ToUpper() : strResult;
                            }
                            else if (ListList[3].ToUpper() == "NOK")
                            {
                                strResult = ListList[3].ToUpper();
                            }
                        }
                    }
                    dictListResult[keyListList.Key] = strResult;
                }

                int iOK = 0;
                int iNOK = 0;
                int iWARN = 0;
                for (int i = 1; i <= dictListResult.Count; i++)
                {
                    if (dictListResult[i] == "OK")
                    {
                        iOK = iOK == 0 ? i : iOK;
                    }
                    else if (dictListResult[i] == "WARN")
                    {
                        iWARN = iWARN == 0 ? i : iWARN;
                    }
                    else if (dictListResult[i] == "NOK")
                    {
                        iNOK = iNOK == 0 ? i : iNOK;
                    }
                }

                if (iNOK != 0)
                {
                    dictReport[keydictListListData.Key + "@" + iNOK] = keydictListListData.Value[iNOK];
                }
                else if (iWARN != 0)
                {
                    dictReport[keydictListListData.Key + "@" + iWARN] = keydictListListData.Value[iWARN];
                }
                else if (iOK != 0)
                {
                    dictReport[keydictListListData.Key + "@" + iOK] = keydictListListData.Value[iOK];
                }
            }
            return dictReport;
        }

        public Dictionary<string, string> AnalysisXmlReportPath(string xmlPath)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(xmlPath);
                XmlElement rootElem = doc.DocumentElement;
                if (rootElem == null) return null;

                XmlNodeList testPathNodes = rootElem.GetElementsByTagName("testcase");
                Dictionary<string, string> dictPath = new Dictionary<string, string>();
                foreach (XmlNode testPathNode in testPathNodes)
                {
                    #region 用于停止后生成报告，解析时发现ident没有元素时 此条数据废弃
                    var identList = testPathNode.SelectNodes("ident");
                    var titleList = testPathNode.SelectNodes("title");
                    if (titleList.Count <= 0 || identList.Count <= 0)
                    {
                        continue;
                    }
                    #endregion
                    string ident = testPathNode.SelectNodes("ident")[0].InnerText;
                    string title = testPathNode.SelectNodes("title")[0].InnerText;
                    XmlNodeList PathNodes = testPathNode.SelectNodes("miscinfo//info//description");
                    int titleIndex = 1;
                    if (title.Contains("@"))
                    {
                        int.TryParse(title.Split('@')[1], out titleIndex);
                    }
                    dictPath[ident + "@" + titleIndex] = PathNodes.Count > 0 ? PathNodes[0].InnerText : string.Empty;
                }
                return dictPath;
            }
            catch (Exception e)
            {
                return new Dictionary<string, string>();
            }
        }

        public Dictionary<string, string> AnalysisXmlReportCover(string xmlPath)
        {
            try
            {
                //得到根节点
                XmlDocument doc = new XmlDocument();
                doc.Load(xmlPath);
                XmlElement rootElem = doc.DocumentElement;
                if (rootElem == null) return null;

                //车和家测试信息相关 得到sut节点
                XmlNodeList testsutNodes = rootElem.GetElementsByTagName("sut");

                Dictionary<string, string> dictSut = new Dictionary<string, string>();
                foreach (XmlNode testsutNode in testsutNodes)
                {
                    //车和家测试信息相关 得到sut下的info节点
                    XmlNodeList sutNodes = testsutNode.SelectNodes("info");
                    string strInfoName = string.Empty;
                    string strInfoValue = string.Empty;
                    foreach (XmlNode Node in sutNodes)
                    {
                        XmlNodeList nodeListInfo = Node.ChildNodes;
                        foreach (XmlNode nodeInfo in nodeListInfo)
                        {
                            if (nodeInfo.Name.ToLower() == "name")
                                strInfoName = nodeInfo.InnerText;
                            else if (nodeInfo.Name.ToLower() == "description")
                                strInfoValue = nodeInfo.InnerText;
                        }
                        dictSut[strInfoName] = strInfoValue;
                    }
                }
                string jsonCover = Json.SerJson(dictSut);
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "log\\ReportCover.json", jsonCover);
                return dictSut;
            }
            catch (Exception e)
            {
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "log\\ReportCover.json", e.ToString());
                return new Dictionary<string, string>();
            }
        }
        #endregion

        #region Excel报告保存相关

        public bool SaveReport(IWorkbook workbook, string testRecordDirPath, string reportDirPath, string reportXmlPath,
            string reportTime)
        {
            bool boolSuccess = false;
            try
            {
                string reportInfoPath = reportDirPath + @"测试报告\";
                string reportPath = reportDirPath + "TestReport" + reportTime + ".xlsx";
                if (!Alphaleonis.Win32.Filesystem.Directory.Exists(reportDirPath))
                {
                    Alphaleonis.Win32.Filesystem.Directory.CreateDirectory(reportDirPath);
                }

                if (!string.IsNullOrEmpty(testRecordDirPath))
                {
                    CopyDir(testRecordDirPath, reportInfoPath);
                }

                if (File.Exists(reportXmlPath))
                {
                    Alphaleonis.Win32.Filesystem.File.Copy(reportXmlPath,
                        reportDirPath + "TestReport" + reportTime + ".xml", true);
                    string reportHtmlPath = Alphaleonis.Win32.Filesystem.Path.ChangeExtension(reportXmlPath, "html");
                    Alphaleonis.Win32.Filesystem.File.Copy(reportHtmlPath,
                        reportDirPath + "TestReport" + reportTime + ".html", true);
                }

                try
                {
                    Alphaleonis.Win32.Filesystem.Directory.Delete(testRecordDirPath, true);
                    Alphaleonis.Win32.Filesystem.Directory.CreateDirectory(testRecordDirPath);
                }
                catch
                {

                }

                FileStream sw = File.Create(reportPath);
                workbook.Write(sw);
                sw.Close();
                sw.Dispose();
                boolSuccess = true;
            }
            catch (Exception e)
            {
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "报告保存错误记录.log", e.ToString());
                boolSuccess = false;
            }

            return boolSuccess;
        }

        private void CopyDir(string srcPath, string aimPath)
        {
            try
            {
                // 检查目标目录是否以目录分割字符结束如果不是则添加
                if (aimPath[aimPath.Length - 1] != Path.DirectorySeparatorChar)
                {
                    aimPath += Alphaleonis.Win32.Filesystem.Path.DirectorySeparatorChar;
                }
                // 判断目标目录是否存在如果不存在则新建
                if (!Alphaleonis.Win32.Filesystem.Directory.Exists(aimPath))
                {
                    Alphaleonis.Win32.Filesystem.Directory.CreateDirectory(aimPath);
                }
                // 得到源目录的文件列表，该里面是包含文件以及目录路径的一个数组
                // 如果你指向copy目标文件下面的文件而不包含目录请使用下面的方法
                // string[] fileList = Directory.GetFiles(srcPath);
                string[] fileList = Alphaleonis.Win32.Filesystem.Directory.GetFileSystemEntries(srcPath);
                // 遍历所有的文件和目录
                foreach (string file in fileList)
                {
                    // 先当作目录处理如果存在这个目录就递归Copy该目录下面的文件
                    if (Alphaleonis.Win32.Filesystem.Directory.Exists(file))
                    {
                        CopyDir(file, aimPath + Alphaleonis.Win32.Filesystem.Path.GetFileName(file));
                    }
                    // 否则直接Copy文件
                    else
                    {
                        Alphaleonis.Win32.Filesystem.File.Copy(file, aimPath + Alphaleonis.Win32.Filesystem.Path.GetFileName(file), true);
                    }
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }
        #endregion
    }

    #region 重写，对字符串内的数字进行排序

    ///<summary>
    ///主要用于文件名的比较。
    ///</summary>
    public class NumericSortInString : IComparer<Object>
    {
        // Calls CaseInsensitiveComparer.Compare with the parameters reversed.
        ///<summary>
        ///比较两个字符串，如果含用数字，则数字按数字的大小来比较。
        ///</summary>
        ///<param name="x"></param>
        ///<param name="y"></param>
        ///<returns></returns>
        int IComparer<object>.Compare(object x, object y)
        {
            if (x == null || y == null)
                throw new ArgumentException("Parameters can't be null");
            string fileA = x as string;
            string fileB = y as string;
            char[] arr1 = fileA.ToCharArray();
            char[] arr2 = fileB.ToCharArray();
            int i = 0, j = 0;
            while (i < arr1.Length && j < arr2.Length)
            {
                if (char.IsDigit(arr1[i]) && char.IsDigit(arr2[j]))
                {
                    string s1 = "", s2 = "";
                    while (i < arr1.Length && char.IsDigit(arr1[i]))
                    {
                        s1 += arr1[i];
                        i++;
                    }
                    while (j < arr2.Length && char.IsDigit(arr2[j]))
                    {
                        s2 += arr2[j];
                        j++;
                    }
                    if (int.Parse(s1) > int.Parse(s2))
                    {
                        return 1;
                    }
                    if (int.Parse(s1) < int.Parse(s2))
                    {
                        return -1;
                    }
                }
                else
                {
                    if (arr1[i] > arr2[j])
                    {
                        return 1;
                    }
                    if (arr1[i] < arr2[j])
                    {
                        return -1;
                    }
                    i++;
                    j++;
                }
            }
            if (arr1.Length == arr2.Length)
            {
                return 0;
            }
            else
            {
                return arr1.Length > arr2.Length ? 1 : -1;
            }
            //            return string.Compare( fileA, fileB );
            //            return( (new CaseInsensitiveComparer()).Compare( y, x ) );
        }
    }

    #endregion
}
