using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using NPOI.HSSF.UserModel;
using NPOI.OpenXmlFormats.Spreadsheet;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel.Extensions;
using BorderStyle = NPOI.SS.UserModel.BorderStyle;
using FontFamily = NPOI.SS.UserModel.FontFamily;
using HorizontalAlignment = NPOI.SS.UserModel.HorizontalAlignment;
using System.Drawing;

namespace ProcessEngine
{
    public class CreateReport
    {
        private IWorkbook _workbook;
        private ProcFile _file = new ProcFile();
        
        #region ReportColor
        private readonly Color _colorTop = Color.FromArgb(142, 169, 219);
        private readonly Color _colorText = Color.FromArgb(169, 208, 142);
        private readonly Color _colorPath = Color.FromArgb(47, 117, 181);
        private readonly Color _colorOK = Color.FromArgb(0, 176, 80);
        private readonly Color _colorNOK = Color.Red;
        private readonly Color _colorWarn = Color.Yellow;
        private readonly Color _colorBlack = Color.Black;
        private readonly Color _colorWhite = Color.White;
        private readonly Color _colorNA = Color.DeepSkyBlue;
        #endregion
        public static readonly string FontName = "Times New Roman";
        public string ReportDirPath = string.Empty;
        public string ReportXmlPath = string.Empty;
        public DateTime ReportTime = DateTime.Now;
        public Dictionary<string, object> dictReport = new Dictionary<string, object>();

        public bool CreateExcelReport(Dictionary<string, string> dictCover,
            Dictionary<string, List<List<string>>> dictReprot, Dictionary<string, string> dictPath,
            Dictionary<string, Dictionary<string, string>> dictdictTestList,string reportFilesDirPath)
        {
            try
            {
                _workbook = new XSSFWorkbook();
                FillInTempletSheet(dictCover);
                ISheet sheetTestResult = CreateReportTestResultFirstHalf(dictCover);
                var dictTestResult = CreateReportTestList(dictCover, dictReprot, dictPath, dictdictTestList);//详细测试项目
                CreateReportTestResultLastHalf(sheetTestResult, dictTestResult, dictdictTestList);
                ReportTime = dictReport.Keys.Contains("TestTime")
                    ? DateTime.Parse(dictReport["TestTime"].ToString())
                    : DateTime.Now;
                string strTime = ReportTime.ToString("yyyyMMddHHmmss");
                var listTaskNo = dictReport["TaskNo"].ToString().Split('-');
                ReportDirPath = AppDomain.CurrentDomain.BaseDirectory + @"ExcelReport\" + listTaskNo[0] + @"\" +
                                listTaskNo[1] + @"\" + listTaskNo[2] + @"\" + dictReport["TaskRound"] + @"\" +
                                dictReport["TaskName"] + @"\" + dictReport["CANRoad"] + @"\" + dictReport["Module"] + @"\" +
                                strTime + @"\";
                if (!_file.SaveReport(_workbook, reportFilesDirPath, ReportDirPath, ReportXmlPath, strTime))
                {
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "报告保存错误记录.log", e.ToString());
                return false;
            }
        }

        private void FillInTempletSheet(Dictionary<string, string> dictCover)
        {
            _workbook = new XSSFWorkbook(@"ExcelTemplate\"+ dictReport["TaskName"] + ".xlsx");
            ISheet coverSheet = _workbook.GetSheet("封皮");
            IRow coverRow = coverSheet.GetRow(0);
            coverRow = coverSheet.GetRow(2);
            coverRow.Cells[1].SetCellValue(dictReport.Keys.Contains("Module") ? dictReport["Module"].ToString() : "-");
            coverRow.Cells[3].SetCellValue(ReportTime.ToString("yyyy-MM-dd HH:mm:ss"));
            coverRow = coverSheet.GetRow(3);
            coverRow.Cells[1].SetCellValue(dictReport.Keys.Contains("TaskNo") ? dictReport["TaskNo"].ToString() : "-");
            coverRow.Cells[3].SetCellValue(dictReport.Keys.Contains("TestUser") ? dictReport["TestUser"].ToString() : "-");
            IRow dbcRow = coverSheet.GetRow(4);
            dbcRow.Cells[1].SetCellValue(dictCover.Keys.Contains("通信矩阵版本号") ? dictCover["通信矩阵版本号"].ToString() : "-");
            IRow hNumRow = coverSheet.GetRow(5);
            hNumRow.Cells[1].SetCellValue(dictCover.Keys.Contains("硬件版本号") ? dictCover["硬件版本号"].ToString() : "-");
            IRow sNumRow = coverSheet.GetRow(6);
            sNumRow.Cells[1].SetCellValue(dictCover.Keys.Contains("软件版本号") ? dictCover["软件版本号"].ToString() : "-");
        }

        private ISheet CreateReportTestResultFirstHalf(Dictionary<string, string> dictCover)
        {
            ISheet sheetTestResult = _workbook.CreateSheet("测试结果");
            #region 设置列宽
            sheetTestResult.SetColumnWidth(0, 18 * 256);
            sheetTestResult.SetColumnWidth(1, 14 * 256);
            sheetTestResult.SetColumnWidth(2, 14 * 256);
            sheetTestResult.SetColumnWidth(3, 18 * 256);
            sheetTestResult.SetColumnWidth(4, 18 * 256);
            sheetTestResult.SetColumnWidth(5, 20 * 256);
            #endregion
            
            #region 内容抬头
            CellRangeAddress cellRange0_0_0_5 = new CellRangeAddress(0, 0, 0, 5);
            sheetTestResult.AddMergedRegion(cellRange0_0_0_5);
            IRow row0 = sheetTestResult.CreateRow(0);
            row0.CreateCell(0).SetCellValue("测试总览");
            row0.Height = 20 * 20;
            RangeAddStyle(cellRange0_0_0_5, HorizontalAlignment.Left, VerticalAlignment.Center, 14, false,
                _colorWhite, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                _colorBlack, _colorTop, _workbook, sheetTestResult);
            IRow row1 = sheetTestResult.CreateRow(1);
            row1.Height = 15 * 20;
            row1.CreateCell(0);
            row1.CreateCell(1).SetCellValue("测试编号");
            row1.CreateCell(2).SetCellValue("测试项目");
            CellRangeAddress cellRange1_1_2_4 = new CellRangeAddress(1, 1, 2, 4);
            sheetTestResult.AddMergedRegion(cellRange1_1_2_4);
            row1.CreateCell(5).SetCellValue("测试结果");
            CellAddStyle(row1.Cells[0], HorizontalAlignment.Center, VerticalAlignment.Center, 9, false,
                _colorBlack, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                _colorBlack, _colorText, _workbook, sheetTestResult);
            CellAddStyle(row1.Cells[1], HorizontalAlignment.Left, VerticalAlignment.Center, 9, false,
                _colorBlack, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                _colorBlack, _colorText, _workbook, sheetTestResult);
            RangeAddStyle(cellRange1_1_2_4, HorizontalAlignment.Left, VerticalAlignment.Center, 9, false,
                _colorBlack, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                _colorBlack, _colorText, _workbook, sheetTestResult);
            CellAddStyle(row1.Cells[5], HorizontalAlignment.Center, VerticalAlignment.Center, 9, false,
                _colorBlack, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                _colorBlack, _colorText, _workbook, sheetTestResult);
            #endregion
            return sheetTestResult;
        }

        private void CreateReportTestResultLastHalf(ISheet sheetTestResult, Dictionary<string, string> dictTestResult,
            Dictionary<string, Dictionary<string, string>> dictdictTestList)
        {
            #region 内容部分
            int rowIndex = 2;
            foreach (var dictTestList in dictdictTestList)
            {
                int rowTittleIndex = rowIndex;
                foreach (var TestList in dictTestList.Value)
                {
                    List<string> ListRow = new List<string>();
                    ListRow.Add(TestList.Key);
                    ListRow.Add(TestList.Value);
                    if (dictTestResult.ContainsKey(TestList.Key))
                    {
                        ListRow.Add(dictTestResult[TestList.Key]);
                    }
                    else
                    {
                        ListRow.Add("N/T");
                    }

                    IRow iRow = sheetTestResult.CreateRow(rowIndex);
                    ICell iCell1 = iRow.CreateCell(1);
                    iCell1.SetCellValue(ListRow[0]);
                    CellAddStyle(iCell1, HorizontalAlignment.Center, VerticalAlignment.Center, 12, false,
                        _colorBlack, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                        _colorBlack, _colorWhite, _workbook, sheetTestResult);

                    CellRangeAddress cellRangeCell2 = new CellRangeAddress(rowIndex, rowIndex, 2, 4);
                    sheetTestResult.AddMergedRegion(cellRangeCell2);
                    iRow.CreateCell(2).SetCellValue(ListRow[1]);
                    RangeAddStyle(cellRangeCell2, HorizontalAlignment.Left, VerticalAlignment.Center, 10, false,
                        _colorBlack, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                        _colorBlack, _colorWhite, _workbook, sheetTestResult);

                    ICell iCell3 = iRow.CreateCell(5);
                    iCell3.SetCellValue(ListRow[2]);
                    switch (ListRow[2].Trim().ToLower())
                    {
                        case "ok":
                            CellAddStyle(iCell3, HorizontalAlignment.Center, VerticalAlignment.Center, 9, false,
                                _colorBlack, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                                _colorBlack, _colorOK, _workbook, sheetTestResult);
                            break;
                        case "nok":
                            CellAddStyle(iCell3, HorizontalAlignment.Center, VerticalAlignment.Center, 9, false,
                                _colorBlack, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                                _colorBlack, _colorNOK, _workbook, sheetTestResult);
                            break;
                        case "warn":
                            CellAddStyle(iCell3, HorizontalAlignment.Center, VerticalAlignment.Center, 9, false,
                                _colorBlack, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                                _colorBlack, _colorWarn, _workbook, sheetTestResult);
                            break;
                        case "n/t":
                            CellAddStyle(iCell3, HorizontalAlignment.Center, VerticalAlignment.Center, 9, false,
                                _colorBlack, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                                _colorBlack, _colorWhite, _workbook, sheetTestResult);
                            break;
                    }
                    rowIndex++;
                }
                CellRangeAddress cellRange = new CellRangeAddress(rowTittleIndex, rowIndex - 1, 0, 0);
                sheetTestResult.AddMergedRegion(cellRange);
                sheetTestResult.GetRow(rowTittleIndex).CreateCell(0).SetCellValue(dictTestList.Key);
                RangeAddStyle(cellRange, HorizontalAlignment.Center, VerticalAlignment.Center, 10, false,
                    _colorBlack, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                    _colorBlack, _colorWhite, _workbook, sheetTestResult);
            }
            #endregion
            #region 注解部分
            rowIndex++;
            CellRangeAddress cellRangeNoteTittle = new CellRangeAddress(rowIndex, rowIndex, 0, 1);
            sheetTestResult.AddMergedRegion(cellRangeNoteTittle);
            sheetTestResult.CreateRow(rowIndex).CreateCell(0).SetCellValue("注解：");
            RangeAddStyle(cellRangeNoteTittle, HorizontalAlignment.Left, VerticalAlignment.Center, 12, false,
                _colorBlack, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                _colorBlack, _colorWhite, _workbook, sheetTestResult);
            rowIndex++;
            IRow iRowNote1 = sheetTestResult.CreateRow(rowIndex);
            iRowNote1.CreateCell(0).SetCellValue("不通过");
            iRowNote1.CreateCell(1).SetCellValue("NOK");
            CellAddStyle(iRowNote1.Cells[0], HorizontalAlignment.Center, VerticalAlignment.Center, 10, false,
                _colorBlack, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                _colorBlack, _colorWhite, _workbook, sheetTestResult);
            CellAddStyle(iRowNote1.Cells[1], HorizontalAlignment.Center, VerticalAlignment.Center, 10, false,
                _colorBlack, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                _colorBlack, _colorNOK, _workbook, sheetTestResult);
            rowIndex++;
            IRow iRowNote2 = sheetTestResult.CreateRow(rowIndex);
            iRowNote2.CreateCell(0).SetCellValue("通过");
            iRowNote2.CreateCell(1).SetCellValue("OK");
            CellAddStyle(iRowNote2.Cells[0], HorizontalAlignment.Center, VerticalAlignment.Center, 10, false,
                _colorBlack, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                _colorBlack, _colorWhite, _workbook, sheetTestResult);
            CellAddStyle(iRowNote2.Cells[1], HorizontalAlignment.Center, VerticalAlignment.Center, 10, false,
                _colorBlack, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                _colorBlack, _colorOK, _workbook, sheetTestResult);
            rowIndex++;
            IRow iRowNote3 = sheetTestResult.CreateRow(rowIndex);
            iRowNote3.CreateCell(0).SetCellValue("不通过但可以接受");
            iRowNote3.CreateCell(1).SetCellValue("NOK BUT ACCEPT");
            CellAddStyle(iRowNote3.Cells[0], HorizontalAlignment.Center, VerticalAlignment.Center, 10, false,
                _colorBlack, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                _colorBlack, _colorWhite, _workbook, sheetTestResult);
            CellAddStyle(iRowNote3.Cells[1], HorizontalAlignment.Center, VerticalAlignment.Center, 10, false,
                _colorBlack, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                _colorBlack, _colorOK, _workbook, sheetTestResult);
            rowIndex++;
            IRow iRowNote4 = sheetTestResult.CreateRow(rowIndex);
            iRowNote4.CreateCell(0).SetCellValue("警告");
            iRowNote4.CreateCell(1).SetCellValue("WARN");
            CellAddStyle(iRowNote4.Cells[0], HorizontalAlignment.Center, VerticalAlignment.Center, 10, false,
                _colorBlack, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                _colorBlack, _colorWhite, _workbook, sheetTestResult);
            CellAddStyle(iRowNote4.Cells[1], HorizontalAlignment.Center, VerticalAlignment.Center, 10, false,
                _colorBlack, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                _colorBlack, _colorWarn, _workbook, sheetTestResult);

            rowIndex++;
            IRow iRowNote5 = sheetTestResult.CreateRow(rowIndex);
            iRowNote5.CreateCell(0).SetCellValue("与评价标准不符");
            iRowNote5.CreateCell(1).SetCellValue("N/A");
            CellAddStyle(iRowNote5.Cells[0], HorizontalAlignment.Center, VerticalAlignment.Center, 10, false,
                _colorBlack, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                _colorBlack, _colorWhite, _workbook, sheetTestResult);
            CellAddStyle(iRowNote5.Cells[1], HorizontalAlignment.Center, VerticalAlignment.Center, 10, false,
                _colorBlack, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                _colorBlack, _colorNA, _workbook, sheetTestResult);

            rowIndex++;
            IRow iRowNote6 = sheetTestResult.CreateRow(rowIndex);
            iRowNote6.CreateCell(0).SetCellValue("未进行测试");
            iRowNote6.CreateCell(1).SetCellValue("N/T");
            CellAddStyle(iRowNote6.Cells[0], HorizontalAlignment.Center, VerticalAlignment.Center, 10, false,
                _colorBlack, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                _colorBlack, _colorWhite, _workbook, sheetTestResult);
            CellAddStyle(iRowNote6.Cells[1], HorizontalAlignment.Center, VerticalAlignment.Center, 10, false,
                _colorBlack, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                _colorBlack, _colorWhite, _workbook, sheetTestResult); 

            #endregion
        }


        #region 测试列表页面部分

        /// <summary>
        /// 创建测试列表等页面 例：物理层测试，数据链路层测试等
        /// </summary>
        /// <param name="dictCover">自检xml解析数据，包含数据库内存储的测试人员信息</param>
        /// <param name="dictReprot">报告xml解析的数据</param>
        /// <param name="dictPath">报告xml解析的路径信息</param>
        /// <param name="dictdictTestList">全部测试列表</param>
        /// <returns>返回测试结果值用于测试结果页</returns>
        private Dictionary<string, string> CreateReportTestList(Dictionary<string, string> dictCover,
            Dictionary<string, List<List<string>>> dictReprot, Dictionary<string, string> dictPath,
            Dictionary<string, Dictionary<string, string>> dictdictTestList)
        {
            dgCreateReportTestList handler = new dgCreateReportTestList(CreateReportTestListInvoke);
            List<IAsyncResult> ListIAsyncResult = new List<IAsyncResult>();
            Dictionary<string, string> dictResult = new Dictionary<string, string>();

            foreach (var dictTestList in dictdictTestList)
            {
                #region 判断当前页面内是否有测试项 如和没有测试项则跳出本次循环 不生成当前页面
                bool sheetTrue = false;
                List<string> listReportIndex = new List<string>();
                foreach (var keyReport in dictReprot)
                {
                    listReportIndex.Add(keyReport.Key.Split('@')[0]);
                }
                foreach (var Test in dictTestList.Value)
                {
                    if (listReportIndex.Contains(Test.Key))
                    {
                        sheetTrue = true;
                        break;
                    }
                }
                if (!sheetTrue)
                    continue;
                #endregion
                ISheet sheetTestList = _workbook.CreateSheet(dictTestList.Key);
                ListIAsyncResult.Add(handler.BeginInvoke(dictTestList, sheetTestList, dictCover,
                    dictReprot, dictPath, null, null));
            }
            foreach (var iResult in ListIAsyncResult)
            {
                var dict = handler.EndInvoke(iResult);
                foreach (var keyValue in dict)
                {
                    dictResult[keyValue.Key] = keyValue.Value;
                }
            }
            var listTemp = dictResult.Keys.ToList();
            listTemp.Sort(new NumericSortInString());
            dictResult = listTemp.ToDictionary(t => t, t => dictResult[t]);
            //dictResult = dictResult.OrderBy(r => r.Key).ToDictionary(r => r.Key, r => r.Value);//根据Key值对字典型排序
            return dictResult;
        }

        public delegate Dictionary<string, string> dgCreateReportTestList(
            KeyValuePair<string, Dictionary<string, string>> keydictTestList, ISheet sheetTestList,
            Dictionary<string, string> dictCover,
            Dictionary<string, List<List<string>>> dictReprot, Dictionary<string, string> dictPath);

        private Dictionary<string, string> CreateReportTestListInvoke(KeyValuePair<string, Dictionary<string, string>> dictTestList,
            ISheet sheetTestList, Dictionary<string, string> dictCover,
            Dictionary<string, List<List<string>>> dictReprot, Dictionary<string, string> dictPath)
        {
            Dictionary<string, string> dictResult = new Dictionary<string, string>();

            #region 抬头
            for (int i = 0; i < 3; i++)
            {
                sheetTestList.SetColumnWidth(i, 30 * 256);//设置列宽
            }
            sheetTestList.SetColumnWidth(3, 50 * 256);
            //创建Row中的Cell并赋值

            IRow row0 = sheetTestList.CreateRow(0);
            row0.CreateCell(0).SetCellValue(dictTestList.Key);
            row0.Height = 27 * 20;//设置列高
            CellRangeAddress cellRange0_0_0_3 = new CellRangeAddress(0, 0, 0, 3);
            sheetTestList.AddMergedRegion(cellRange0_0_0_3);
            RangeAddStyle(cellRange0_0_0_3, HorizontalAlignment.Center, VerticalAlignment.Center, 20, true,
                _colorBlack, BorderStyle.Medium, BorderStyle.Medium, BorderStyle.Medium, BorderStyle.Medium,
                _colorBlack, _colorTop, _workbook, sheetTestList);

            #endregion

            int intRowIndex = 1;
            foreach (var KeyTestList in dictTestList.Value)
            {
                bool bOK = false;
                bool bNOK = false;
                bool bWarn = false;
                Dictionary<string, int> dictReportIndex = new Dictionary<string, int>();
                foreach (var keyReport in dictReprot)
                {
                    int iCaseIndex = 1;
                    int.TryParse(keyReport.Key.Split('@')[1], out iCaseIndex);
                    dictReportIndex[keyReport.Key.Split('@')[0]] = iCaseIndex;
                }
                if (!dictReportIndex.ContainsKey(KeyTestList.Key))
                    continue;
                //if (!dictReprot.ContainsKey(KeyTestList.Key))
                //    continue;

                #region 空白行
                IRow rowWhite = sheetTestList.CreateRow(intRowIndex);
                rowWhite.Height = 18 * 20;
                CellRangeAddress cellRangeWhite = new CellRangeAddress(intRowIndex, intRowIndex, 0, 3);
                sheetTestList.AddMergedRegion(cellRangeWhite);
                RangeAddStyle(cellRangeWhite, HorizontalAlignment.Center, VerticalAlignment.Center, 9, false,
                    _colorBlack, BorderStyle.Medium, BorderStyle.Medium, BorderStyle.Medium, BorderStyle.Medium,
                    _colorBlack, _colorWhite, _workbook, sheetTestList);
                intRowIndex++;
                #endregion

                #region 主标题行
                IRow iRowTop = sheetTestList.CreateRow(intRowIndex);
                iRowTop.CreateCell(0).SetCellValue(KeyTestList.Value + "[" + KeyTestList.Key + "]" + " 第" + dictReportIndex[KeyTestList.Key] + "次测试");
                rowWhite.Height = 18 * 20;
                CellRangeAddress cellRangeTop = new CellRangeAddress(intRowIndex, intRowIndex, 0, 3);
                sheetTestList.AddMergedRegion(cellRangeTop);
                RangeAddStyle(cellRangeTop, HorizontalAlignment.Center, VerticalAlignment.Center, 14, true,
                    _colorBlack, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Medium,
                    _colorBlack, _colorWhite, _workbook, sheetTestList);
                intRowIndex++;
                #endregion

                #region 标题行
                IRow iRowTittle = sheetTestList.CreateRow(intRowIndex);
                rowWhite.Height = 18 * 20;
                iRowTittle.CreateCell(0).SetCellValue("测试项目");
                iRowTittle.CreateCell(1).SetCellValue("测试数值");
                iRowTittle.CreateCell(2).SetCellValue("测试结果");
                iRowTittle.CreateCell(3).SetCellValue("测试标准");
                CellAddStyle(iRowTittle.Cells[0], HorizontalAlignment.Center, VerticalAlignment.Center, 10, true,
                    _colorBlack, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                    _colorBlack, _colorWhite, _workbook, sheetTestList);
                CellAddStyle(iRowTittle.Cells[1], HorizontalAlignment.Center, VerticalAlignment.Center, 10, true,
                    _colorBlack, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                    _colorBlack, _colorWhite, _workbook, sheetTestList);
                CellAddStyle(iRowTittle.Cells[2], HorizontalAlignment.Center, VerticalAlignment.Center, 10, true,
                    _colorBlack, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                    _colorBlack, _colorWhite, _workbook, sheetTestList);
                CellAddStyle(iRowTittle.Cells[3], HorizontalAlignment.Center, VerticalAlignment.Center, 10, true,
                    _colorBlack, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                    _colorBlack, _colorWhite, _workbook, sheetTestList);
                intRowIndex++;
                #endregion

                #region 内容行

                foreach (var ListReport in dictReprot[KeyTestList.Key + "@" + dictReportIndex[KeyTestList.Key]])
                {
                    IRow iRowInfo = sheetTestList.CreateRow(intRowIndex);
                    iRowInfo.Height = 15 * 20;
                    if (ListReport[3].ToLower() != "merge")
                    {
                        if (string.IsNullOrWhiteSpace(ListReport[2]))
                        {
                            ListReport[3] = "noresult";
                            iRowInfo.CreateCell(1);
                            iRowInfo.CreateCell(2);
                        }
                        else
                        {
                            iRowInfo.CreateCell(1).SetCellValue(ListReport[2]);
                            iRowInfo.CreateCell(2).SetCellValue(ListReport[3]);
                        }
                        iRowInfo.CreateCell(0).SetCellValue(ListReport[0]);
                        iRowInfo.CreateCell(3).SetCellValue(ListReport[1]);
                    }
                    else
                    {
                        iRowInfo.CreateCell(0).SetCellValue(ListReport[0]);
                    }
                    switch (ListReport[3].ToLower())
                    {
                        case "merge":
                            CellRangeAddress cellRangeInfo = new CellRangeAddress(intRowIndex, intRowIndex, 0, 3);
                            sheetTestList.AddMergedRegion(cellRangeInfo);
                            RangeAddStyle(cellRangeInfo, HorizontalAlignment.Left, VerticalAlignment.Center, 10,
                                true,
                                _colorBlack, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                                BorderStyle.Medium,
                                _colorBlack, _colorWhite, _workbook, sheetTestList);
                            break;
                        case "ok":
                            bOK = true;
                            CellAddStyle(iRowInfo.Cells[0], HorizontalAlignment.Center, VerticalAlignment.Center,
                                10, false,
                                _colorBlack, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                                _colorBlack, _colorWhite, _workbook, sheetTestList);
                            CellAddStyle(iRowInfo.Cells[1], HorizontalAlignment.Center, VerticalAlignment.Center,
                                10, false,
                                _colorBlack, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                                _colorBlack, _colorWhite, _workbook, sheetTestList);
                            CellAddStyle(iRowInfo.Cells[2], HorizontalAlignment.Center, VerticalAlignment.Center,
                                10, false,
                                _colorBlack, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                                _colorBlack, _colorOK, _workbook, sheetTestList);
                            CellAddStyle(iRowInfo.Cells[3], HorizontalAlignment.Center, VerticalAlignment.Center,
                                10, false,
                                _colorBlack, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                                _colorBlack, _colorWhite, _workbook, sheetTestList);
                            break;
                        case "nok":
                            bNOK = true;
                            CellAddStyle(iRowInfo.Cells[0], HorizontalAlignment.Center, VerticalAlignment.Center,
                                10, false,
                                _colorBlack, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                                _colorBlack, _colorWhite, _workbook, sheetTestList);
                            CellAddStyle(iRowInfo.Cells[1], HorizontalAlignment.Center, VerticalAlignment.Center,
                                10, false,
                                _colorBlack, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                                _colorBlack, _colorWhite, _workbook, sheetTestList);
                            CellAddStyle(iRowInfo.Cells[2], HorizontalAlignment.Center, VerticalAlignment.Center,
                                10, false,
                                _colorBlack, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                                _colorBlack, _colorNOK, _workbook, sheetTestList);
                            CellAddStyle(iRowInfo.Cells[3], HorizontalAlignment.Center, VerticalAlignment.Center,
                                10, false,
                                _colorBlack, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                                _colorBlack, _colorWhite, _workbook, sheetTestList);
                            break;
                        case "warn":
                            bWarn = true;
                            CellAddStyle(iRowInfo.Cells[0], HorizontalAlignment.Center, VerticalAlignment.Center,
                                10, false,
                                _colorBlack, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                                _colorBlack, _colorWhite, _workbook, sheetTestList);
                            CellAddStyle(iRowInfo.Cells[1], HorizontalAlignment.Center, VerticalAlignment.Center,
                                10, false,
                                _colorBlack, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                                _colorBlack, _colorWhite, _workbook, sheetTestList);
                            CellAddStyle(iRowInfo.Cells[2], HorizontalAlignment.Center, VerticalAlignment.Center,
                                10, false,
                                _colorBlack, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                                _colorBlack, _colorWarn, _workbook, sheetTestList);
                            CellAddStyle(iRowInfo.Cells[3], HorizontalAlignment.Center, VerticalAlignment.Center,
                                10, false,
                                _colorBlack, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                                _colorBlack, _colorWhite, _workbook, sheetTestList);
                            break;
                        case "noresult":
                            CellAddStyle(iRowInfo.Cells[0], HorizontalAlignment.Center, VerticalAlignment.Center,
                                10, false,
                                _colorBlack, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                                _colorBlack, _colorWhite, _workbook, sheetTestList);
                            CellAddStyle(iRowInfo.Cells[3], HorizontalAlignment.Center, VerticalAlignment.Center,
                                10, false,
                                _colorBlack, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                                _colorBlack, _colorWhite, _workbook, sheetTestList);
                            break;
                    }
                    intRowIndex++;
                }
                #endregion

                #region 路径行
                IRow iRowPath = sheetTestList.CreateRow(intRowIndex);
                iRowPath.CreateCell(0).SetCellValue("测试记录");
                iRowPath.Height = 16 * 20;
                CellRangeAddress cellRangePath = new CellRangeAddress(intRowIndex, intRowIndex, 0, 2);
                sheetTestList.AddMergedRegion(cellRangePath);
                RangeAddStyle(cellRangePath, HorizontalAlignment.Center, VerticalAlignment.Center, 10, true,
                    _colorBlack, BorderStyle.Thin, BorderStyle.Medium, BorderStyle.Medium, BorderStyle.Thin,
                    _colorBlack, _colorWhite, _workbook, sheetTestList);
                var iPathCell = iRowPath.CreateCell(3);
                iPathCell.SetCellValue(KeyTestList.Key + KeyTestList.Value);
                XSSFHyperlink link = new XSSFHyperlink(HyperlinkType.Unknown);
                if (dictPath.ContainsKey(KeyTestList.Key + "@" + dictReportIndex[KeyTestList.Key]))
                {
                    if (string.IsNullOrWhiteSpace(dictPath[KeyTestList.Key + "@" + dictReportIndex[KeyTestList.Key]]))
                    {
                        iPathCell.SetCellValue("暂无路径");
                        CellAddStyle(iPathCell, HorizontalAlignment.Center, VerticalAlignment.Center, 12, false,
                            _colorBlack, BorderStyle.Thin, BorderStyle.Medium, BorderStyle.Thin, BorderStyle.Medium,
                            _colorBlack, _colorWhite, _workbook, sheetTestList);
                    }
                    else
                    {
                        link.Address = dictPath[KeyTestList.Key + "@" + dictReportIndex[KeyTestList.Key]];
                        iPathCell.Hyperlink = link;
                        CellAddStyle(iPathCell, HorizontalAlignment.Center, VerticalAlignment.Center, 12, false,
                            _colorPath, BorderStyle.Thin, BorderStyle.Medium, BorderStyle.Thin, BorderStyle.Medium,
                            _colorBlack, _colorWhite, _workbook, sheetTestList);
                    }
                }
                else
                {
                    iPathCell.SetCellValue("暂无路径");
                    CellAddStyle(iPathCell, HorizontalAlignment.Center, VerticalAlignment.Center, 12, false,
                        _colorBlack, BorderStyle.Thin, BorderStyle.Medium, BorderStyle.Thin, BorderStyle.Medium,
                        _colorBlack, _colorWhite, _workbook, sheetTestList);
                }

                intRowIndex++;
                #endregion

                #region 获得测试项目的结果
                if (bNOK)
                {
                    dictResult[KeyTestList.Key] = "NOK";
                }
                else if (bWarn)
                {
                    dictResult[KeyTestList.Key] = "WARN";
                }
                else if (bOK)
                {
                    dictResult[KeyTestList.Key] = "OK";
                }

                #endregion

            }
            return dictResult;
        }

        #endregion


        /// <summary>
        /// 一定范围内配置风格
        /// </summary>
        /// <param name="crAddress">加边框的范围</param>
        /// <param name="hAlignment">水平对齐方式</param>
        /// <param name="vAlignment">垂直对齐方式</param>
        /// <param name="fontSize">字体大小</param>
        /// <param name="fontBold">字体是否粗体</param>
        /// <param name="fontColor">字体颜色</param>
        /// <param name="btStyle">上边框样式</param>
        /// <param name="bdStyle">下边框样式</param>
        /// <param name="blStyle">左边框样式</param>
        /// <param name="brStyle">右边框样式</param>
        /// <param name="bColor">边框颜色</param>
        /// <param name="backCellColor">单元格背景颜色</param>
        /// <param name="workbook">Excel表</param>
        /// <param name="sheet">页签</param>
        private void RangeAddStyle(CellRangeAddress crAddress, HorizontalAlignment hAlignment,
            VerticalAlignment vAlignment, short fontSize, bool fontBold, Color fontColor, BorderStyle btStyle,
            BorderStyle bdStyle, BorderStyle blStyle, BorderStyle brStyle,
            Color bColor, Color backCellColor, IWorkbook workbook, ISheet sheet)
        {
            //如果判断Excel表不是xlsx格式的，则无风格化，如需要xls格式的情况 则需要单独处理
            if(workbook.GetType().Name.ToLower()!= "xssfworkbook") return;

            XSSFCellStyle style = ExcelStyle(hAlignment, vAlignment, fontSize, fontBold, fontColor, btStyle, bdStyle, blStyle, brStyle, bColor,
                backCellColor, workbook);
            //ICell iCell = CellUtil.GetCell(CellUtil.GetRow(crAddress.FirstRow, sheet), crAddress.FirstColumn);
            //sheet.GetRow(crAddress.FirstRow).GetCell(crAddress.FirstColumn, MissingCellPolicy.CREATE_NULL_AS_BLANK);
            //sheet.GetRow(crAddress.FirstRow).GetCell(crAddress.FirstColumn);
            //iCell.CellStyle = style;

            #region 对范围内的表格进行风格化配置
            for (int i = crAddress.FirstRow; i <= crAddress.LastRow; i++)
            {
                IRow row = CellUtil.GetRow(i, sheet);
                for (int j = crAddress.FirstColumn; j <= crAddress.LastColumn; j++)
                {
                    style.BorderTop = BorderStyle.Thin;
                    style.BorderBottom = BorderStyle.Thin;
                    style.BorderLeft = BorderStyle.Thin;
                    style.BorderRight = BorderStyle.Thin;
                    //if (i == crAddress.FirstRow)
                    //    style.BorderTop = btStyle;
                    //if (i == crAddress.LastRow)
                    //    style.BorderBottom = bdStyle;
                    //if (j == crAddress.FirstColumn)
                    //    style.BorderLeft = blStyle;
                    //if (j == crAddress.LastColumn)
                    //    style.BorderRight = brStyle;
                    ICell singleCell = CellUtil.GetCell(row, (short)j);
                    singleCell.CellStyle = style;
                }
            }
            #endregion
        }

        private void CellAddStyle(ICell iCell, HorizontalAlignment hAlignment,
            VerticalAlignment vAlignment, short fontSize, bool fontBold, Color fontColor, BorderStyle btStyle,
            BorderStyle bdStyle, BorderStyle blStyle, BorderStyle brStyle,
            Color bColor, Color backCellColor, IWorkbook workbook, ISheet sheet)
        {
            //如果判断Excel表不是xlsx格式的，则无风格话，如需要xls格式的情况 则需要单独处理
            if (workbook.GetType().Name.ToLower() != "xssfworkbook") return;
            XSSFCellStyle style = ExcelStyle(hAlignment, vAlignment, fontSize, fontBold, fontColor, btStyle, bdStyle, blStyle, brStyle, bColor,
                backCellColor, workbook);
            iCell.CellStyle = style;
        }

        /// <summary>
        /// 返回一个设置完的style
        /// </summary>
        /// <param name="hAlignment">水平对齐方式</param>
        /// <param name="vAlignment">垂直对齐方式</param>
        /// <param name="fontSize">字体大小</param>
        /// <param name="fontBold">字体是否粗体</param>
        /// <param name="fontColor">字体颜色</param>
        /// <param name="btStyle">上边框样式</param>
        /// <param name="bdStyle">下边框样式</param>
        /// <param name="blStyle">左边框样式</param>
        /// <param name="brStyle">右边框样式</param>
        /// <param name="bColor">边框颜色</param>
        /// <param name="backCellColor">单元格背景颜色</param>
        /// <param name="workbook">Excel表</param>
        /// <returns></returns>
        private XSSFCellStyle ExcelStyle(HorizontalAlignment hAlignment,
            VerticalAlignment vAlignment, short fontSize, bool fontBold, Color fontColor, BorderStyle btStyle,
            BorderStyle bdStyle, BorderStyle blStyle, BorderStyle brStyle,
            Color bColor, Color backCellColor, IWorkbook workbook)
        {
            XSSFCellStyle style = (XSSFCellStyle)workbook.CreateCellStyle();

            #region 文本对齐方式
            style.Alignment = hAlignment;
            style.VerticalAlignment = vAlignment;
            //style.ShrinkToFit = true;//自适应字体大小
            style.WrapText = true;//自动换行
            #endregion

            #region 设置单元格文字样式
            XSSFFont xFont = (XSSFFont)workbook.CreateFont();
            xFont.FontName = FontName;
            xFont.FontHeightInPoints = fontSize;
            xFont.IsBold = fontBold;
            xFont.SetColor(new XSSFColor(fontColor));
            style.SetFont(xFont);
            #endregion

            #region 边框样式
            //style.BorderBottom = bdStyle;
            //style.BorderLeft = blStyle;
            //style.BorderRight = brStyle;
            //style.BorderTop = btStyle;
            style.BorderBottom = BorderStyle.Thin;
            style.BorderLeft = BorderStyle.Thin;
            style.BorderRight = BorderStyle.Thin;
            style.BorderTop = BorderStyle.Thin;
            style.SetBottomBorderColor(new XSSFColor(bColor));
            style.SetLeftBorderColor(new XSSFColor(bColor));
            style.SetRightBorderColor(new XSSFColor(bColor));
            style.SetTopBorderColor(new XSSFColor(bColor));
            #endregion

            #region 背景颜色
            style.SetFillForegroundColor(new XSSFColor(backCellColor));
            style.SetFillBackgroundColor(new XSSFColor(backCellColor));
            style.FillPattern = FillPattern.SolidForeground;
            #endregion

            return style;
        }
    }
}
