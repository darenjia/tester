using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Windows.Forms;
using DevExpress.Office.Utils;
using DevExpress.XtraCharts;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.Serialization;
using DevExpress.XtraReports.UI;
using DevExpress.XtraRichEdit.Model.History;
using DevExpress.XtraTab;
using ProcessEngine;

namespace ReportEditor
{
    public partial class TestReport : DevExpress.XtraReports.UI.XtraReport
    {
        ProcFile _file = new ProcFile();
        ProcStore _store = new ProcStore();
        private int page = 3;
        private int  y = 0;
        private int x = 41;
        List<Dictionary<string, List<List<string>>>> failTCRe = new List<Dictionary<string, List<List<string>>>>();

        List<Dictionary<string, Dictionary<string, List<string>>>> failTC = new List<Dictionary<string, Dictionary<string, List<string>>>>();
        List<Dictionary<string, Dictionary<string, List<string>>>> all = new List<Dictionary<string, Dictionary<string, List<string>>>>();
        Dictionary<string,Image> imgDict = new Dictionary<string, Image>();
       
        
        /// <summary>
        /// 工具管理中报告查看器入口
        /// </summary>
        /// <param name="dict">报告数据</param>
        /// <param name="remark">一些固定像里面的数据</param>
        /// <param name="moduleInfor">模块信息数据</param>
        /// <param name="errorInfor">错误截图和日志数据</param>
        public TestReport(Dictionary<string, List<Dictionary<string, List<List<string>>>>> dict , Dictionary<string,string> remark,Dictionary<string,List<string>> moduleInfor, Dictionary<string, List<string>> errorInfor)
        {
            InitializeComponent();

            SetTestInformation(remark);
            SetModuleByDictionary(moduleInfor);
            Dictionary<string, object> dictFile = new Dictionary<string, object>();
            dictFile.Add("VehicelType", remark["TaskNo"].Split('-')[0]);
            dictFile.Add("VehicelConfig", remark["TaskNo"].Split('-')[1]);
            dictFile.Add("VehicelStage", remark["TaskNo"].Split('-')[2]);
            dictFile.Add("EmlTemplateName", remark["TaskName"] + "用例表");
            IList<object[]> listFile = _store.GetSpecialByEnum(EnumLibrary.EnumTable.FileLinkByVehicelDouble, dictFile);
            string tplyPath = "";
            IList<object[]> listtpy = _store.GetSpecialByEnum(EnumLibrary.EnumTable.Topology, dictFile);
            if (listtpy.Count != 0)
                tplyPath = AppDomain.CurrentDomain.BaseDirectory + listtpy[0][3].ToString();
            List<object> imageList = new List<object>();
            string title = remark["TaskNo"] + "-" + remark["TaskRound"] + "-" + remark["CANRoad"] + "-" +
                           remark["TaskName"];
            lblReportTitle.Text = title;
            if(tplyPath != "")
            imageList.Add(Image.FromFile(tplyPath));
            imageList.Add("本网络拓扑共包含4个网段，网段一有10各节点，网段二有5各节点，所测模块处于BCAN网段上。");
            imageList.Add(title);
            if (listtpy.Count != 0)
                SetReference(listtpy[0][4].ToString());
            SetTopoloy(imageList);

            List<int> itemList = new List<int>();
            itemList.Add(int.Parse(remark["TestItemCount"]));
            itemList.Add(int.Parse(remark["TestItemCount"]) - int.Parse(remark["TestItemSkipCount"]));
            itemList.Add(int.Parse(remark["TestItemSkipCount"]));
            itemList.Add(int.Parse(remark["TestItemSuCount"]));
            itemList.Add(int.Parse(remark["TestItemCount"]) - int.Parse(remark["TestItemSkipCount"]) - int.Parse(remark["TestItemSuCount"]));
            SetTestPercent(itemList);
            

            FillTableByTestResultsSummary(dict);
            if(dict.Count !=0)
                FillTableByReport(dict);

            //foreach(KeyValuePair<string,List<string>>)

            //Bitmap bmp = new Bitmap(GlobalVar.ImageFilePath + "SBJ.jpg");
            //foreach (KeyValuePair<string, Dictionary<string, List<string>>> tc in _dict)
            //{
            //    imgDict.Add(tc.Key, bmp);
            //}

            if (failTCRe.Count != 0 && errorInfor != null && errorInfor.Count != 0 ){
                NewCreateApendix(failTCRe, errorInfor);
                
            }
            
            //SetTestInformation(GlobalVar.ReportTestOrder);
            //SetReference(GlobalVar.Reference);
            //SetTopoloy(GlobalVar.imgTply);
            //SetTestPercent(GlobalVar.TestItem);
            //lblReportTitle.Text = GlobalVar.ReportTitle;
        }

        //测试处进入
        public TestReport(Dictionary<string, List<Dictionary<string, List<List<string>>>>> dict,List<object> dictList , Dictionary<string, List<string>> moduleInfor,Dictionary<string,List<string>> errorInfo)
        {
            InitializeComponent();
            FillTableByTestResultsSummary(dict);
            SetModuleByDictionary(moduleInfor);
            if (dict.Count != 0)
                FillTableByReport(dict);
            
            
            SetTestInformation((Dictionary<string,string>)dictList[0]);
            
            SetReference(dictList[1].ToString());
            SetTopoloy((List<object>)dictList[2]);
            SetTestPercent((List<int>)dictList[3]);
            lblReportTitle.Text = dictList[4].ToString();
            if (failTCRe.Count != 0 && errorInfo != null && errorInfo.Count != 0)
                NewCreateApendix(failTCRe, errorInfo);
            //SetTestInformation(GlobalVar.ReportTestOrder);
            //SetReference(GlobalVar.Reference);
            //SetTopoloy(GlobalVar.imgTply);
            //SetTestPercent(GlobalVar.TestItem);
            //lblReportTitle.Text = GlobalVar.ReportTitle;
        }


        private void SetModuleByDictionary(Dictionary<string, List<string>> moduleInfor)
        {
            float rowHeigt = 0.00F ;
            XRTable table = CreatModuleInforTableTitle(142.04F, 375.75F);
            //List<object> tableList = new List<object>();
            foreach (KeyValuePair<string,List<string>> infor in moduleInfor)
            {

                List<string> rowList = new List<string>();
                rowList.Add(infor.Key);
                foreach (var list in infor.Value)
                    rowList.Add(list);
                XRTableRow row = CreateModuleRow(rowList);
                rowHeigt = row.HeightF;
                table.Rows.Add(row);

            }
            TitleDetail.Controls.Add((XRControl)table);
            float labHeight = rowHeigt * (moduleInfor.Count + 1);
            XRLabel lblModulel = new XRLabel
            {
                Font =
                    new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular,
                        System.Drawing.GraphicsUnit.Point, ((byte)(0))),
                ForeColor = System.Drawing.Color.Black,
                LocationFloat = new DevExpress.Utils.PointFloat(70.82F, 375.75F),
                Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F),
                SizeF = new System.Drawing.SizeF(70.5058F, labHeight),
                
            };
            lblModulel.Borders = ((DevExpress.XtraPrinting.BorderSide)(((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Right)
            | DevExpress.XtraPrinting.BorderSide.Bottom)));
            lblModulel.StylePriority.UseBorders = false;
            lblModulel.StylePriority.UseFont = false;
            lblModulel.StylePriority.UseTextAlignment = false;
            lblModulel.Text = "模块信息";

            lblModulel.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            TitleDetail.Controls.Add((XRControl)lblModulel);
            //CreatePage(page, tableList);

        }


     
        /// <summary>
        /// 新的绘制第四章的方法
        /// </summary>
        /// <param name="dict"></param>
        private void FillTableByReport(Dictionary<string, List<Dictionary<string, List<List<string>>>>> dict)
        {
            int node = 1;
            //int i = dict.Count;
            foreach (KeyValuePair<string, List<Dictionary<string, List<List<string>>>>> layer in dict)
            {
                if (layer.Value.Count != 0)
                {

                    List<object> tableList = new List<object>();
                    XRLabel chapter = CreatTitle("4. " + node + " " + layer.Key, 14F);
                    chapter.Location = new Point(0, y);
                    y += 30;
                    tableList.Add(chapter); //
                    int i = layer.Value.Count;

                    for (int k = 0; k < i; k++)
                    {
                        Dictionary<string, List<List<string>>> _dict = layer.Value[k];
                        List<List<string>> tcDict = new List<List<string>>();
                        //tcDict = _dict.Value;
                        //此处给表命名
                        foreach (KeyValuePair<string, List<List<string>>> tc in _dict)
                        {
                            string[] name = tc.Key.Split(' ');
                            string title = name[1] + " " + name[2];
                            XRLabel chapterDetail = CreatTitle(title, 12F);
                            chapterDetail.Location = new Point(x, y);
                            y += 40;
                            tableList.Add(chapterDetail);
                            tcDict = tc.Value;
                        }
                        XRTable table = CreatTableTitle(x, y);
                        y += table.Height;
                        List<bool> bottom = new List<bool>();
                        int n = 1;
                        foreach (var rowDict in tcDict)
                        {
                            XRTableRow Log = new XRTableRow();
                            //List<string> rowlist = rowDict;

                            List<string> newList = new List<string>();
                            newList.Add(n.ToString());
                            n ++;
                            //for (int l = 0; l < rowDict.Count;l++)
                            //{
                            //    if (l == 0)
                            //    {
                            //        continue;
                            //    }
                            //    else
                            //    {
                            //        newList.Add(rowDict[l]);

                            //    }
                            //}
                            newList = NewListSorting(rowDict, newList);
                            //int n = newList.Count;
                            bool col = false;
                            bool num = false;
                            if (newList.Count < 6)
                            {
                                num = true;
                                if (newList[4].ToLower() == "pass")
                                    col = true;
                            }
                            else
                            {
                                if (newList[5].ToLower() == "pass")
                                    col = true;
                            }

                            bottom.Add(col);
                            XRTableRow row = CreatRowBySpan(newList, col, num);
                            table.Rows.Add(row);
                            //if (newList.Count == 7)
                            //{
                            //    Log = CreatLogCellRow(newList[6]);
                            //    table.Rows.Add(Log);
                            //}
                            //else if (newList.Count == 8)
                            //{
                            //    Log = CreatLogCellRow(newList[7]);
                            //    table.Rows.Add(Log);
                            //}
                            //y += row.Height;
                        }


                        XRTableRow rowbottom = CreatColorCellRow(bottom);
                        table.Rows.Add(rowbottom);
                        tableList.Add(table);
                        y = y + table.Height + 20;
                        if (rowbottom.BackColor == Color.Red)
                        {
                            failTCRe.Add(_dict);
                            //Bitmap bmp = new Bitmap(GlobalVar.ImageFilePath + "SBJ.jpg");
                            //foreach (KeyValuePair<string, List<List<string>>> tc in _dict)
                            //{
                            //    imgDict.Add(tc.Key, bmp);
                            //}

                        }
                        //all.Add(_dict);
                    }
                    CreatePage(page, tableList);
                    page++;
                    y = 0;
                    node++;
                }
            }
        }

        #region 创建附录

        /// <summary>
        /// 新的创建附录的方法
        /// </summary>
        /// <param name="failAppen"></param>
        /// <param name="errorImages"></param>
        private void NewCreateApendix(List<Dictionary<string, List<List<string>>>> failAppen,
           Dictionary<string, List<string>> errorImages)
        {
            if (errorImages.Count == 0)
            {

            }
            List<object> tableList = new List<object>();
            XRLabel chapter = CreatTitle("5. 附录", 14F);
            tableList.Add(chapter);
            chapter.Location = new Point(0, y);
            y += 30;
            int Num = failAppen.Count;
            //List<object> tableList = new List<object>();
            for (int k = 0; k < Num; k++)
            {
                Dictionary<string, List<List<string>>> _dict = failAppen[k];
                List<List<string>> tcDict = new List<List<string>>();
              
                
                foreach (KeyValuePair<string, List<List<string>>> Testcase in _dict)
                {
                    foreach (KeyValuePair<string, List<string>> pict in errorImages)
                    {
                        if (pict.Key == Testcase.Key.Split(' ')[1])
                        {
                            ///绘制失败用例数据表格

                            //此处给表命名
                            foreach (KeyValuePair<string, List<List<string>>> tc in _dict)
                            {
                                string title = tc.Key.Split(' ')[1] + " " + tc.Key.Split(' ')[2];
                                XRLabel chapterDetail = CreatTitle(title, 12F);
                                chapterDetail.Location = new Point(x, y);
                                y += 40;
                                tableList.Add(chapterDetail);
                                tcDict = tc.Value;
                            }
                            XRTable table = CreatTableTitle(x, y);
                            //y += table.Height;
                            List<bool> bottom = new List<bool>();
                            int n = 1;
                            foreach (List<string> rowDict in tcDict)
                            {

                                //List<string> rowlist = rowDict.Value;
                                //if (rowDict.Key == "heading")
                                //    continue;
                                List<string> newList = new List<string>();
                                newList.Add(n.ToString());
                                n++;
                                newList = NewListSorting(rowDict, newList);

                                bool col = false;
                                bool num = false;
                                if (newList.Count < 6)
                                {
                                    num = true;
                                    if (newList[4].ToLower() == "pass")
                                        col = true;
                                }
                                else if (newList[5].ToLower() == "pass")
                                    col = true;
                                bottom.Add(col);
                                if (newList.Count < 6)
                                {
                                    //num = true;
                                    if (newList[4].ToLower() == "pass")
                                        continue;
                                }
                                else if (newList[5].ToLower() == "pass")
                                    continue;
                                XRTableRow row = CreatRowBySpan(newList, col, num);
                                table.Rows.Add(row);
                                //y += row.Height;
                            }
                            XRTableRow rowbottom = CreatColorCellRow(bottom);
                            table.Rows.Add(rowbottom);
                            tableList.Add(table);
                            y = y + table.Height;
                            XRTable errorPic = CreatPicTableTitle("错误截图");
                            errorPic.Location = new Point(x, y);
                            tableList.Add(errorPic);
                            y = y + errorPic.Height;

                            ///添加错误截图和日志
                            string path = pict.Value[0];
                            Bitmap bmp = new Bitmap(AppDomain.CurrentDomain.BaseDirectory  + path);
                            XRPictureBox pictureBox = CreatePictureBox(bmp);
                            pictureBox.Location = new Point(x, y);
                            //pictureBox.Sizing = ImageSizeMode.ZoomImage;
                            tableList.Add(pictureBox);
                            y = y + pictureBox.Height;
                            XRLabel file = CreatTitle("记录文件", 12F);
                            file.Location = new Point(x, y);
                            file.BackColor = Color.Silver;
                            file.SizeF = new System.Drawing.SizeF(656F, 32.4722F);
                            tableList.Add(file);
                            y = y + file.Height;
                            string files = pict.Value[1];
                            XRLabel fileLabel = CreatTitle(files, 10F);
                            fileLabel.Multiline = true;
                            fileLabel.Location = new Point(x, y);
                            fileLabel.TextAlignment = TextAlignment.TopLeft;
                            fileLabel.SizeF = new System.Drawing.SizeF(656F, 256F);
                            tableList.Add(fileLabel);
                            y = y + fileLabel.Height + 10;
                            break;
                        }
                    }
                    
                }


                //if (rowbottom.BackColor == Color.Red)
                //    failTC.Add(_dict);

            }
            CreatePage(page, tableList);
            page++;
            y = 0;
            //node++;
        }

        #endregion
        /// <summary>
        /// 新的绘制第三章的方法
        /// </summary>
        /// <param name="dict"></param>
        private void FillTableByTestResultsSummary(Dictionary<string, List<Dictionary<string, List<List<string>>>>> dict)
        {
            List<object> tableList = new List<object>();
            XRLabel chapter = CreatTitle("3. 测试结果汇总", 14F);
            chapter.Location = new Point(0, y);
            y = y + chapter.Height;
            y += 20;
            tableList.Add(chapter);
            XRTable table = CreatCellTableTitle(x, y);
            foreach (KeyValuePair<string, List<Dictionary<string, List<List<string>>>>> layer in dict)
            {
                string strClassification = "";
                string strUsecase = "";
                string strResult = "N/A";
                string strRemark = "";
                int SpanCount;
                bool count = true;

                strClassification = layer.Key;
                foreach (var TC in layer.Value)
                {
                    if (count)
                    {
                        SpanCount = layer.Value.Count;
                        count = false;
                    }
                    else
                    {
                        SpanCount = 1;
                    }
                    foreach (KeyValuePair<string, List<List<string>>> item in TC)
                    {
                        string examName = item.Key ;
                        bool result = true;
                        foreach (var list in item.Value)
                        {
                            if (list[5].ToLower() == "fail") 
                            {
                                strResult = list[5];
                                break;
                            }
                        else
                            {
                                strResult = list[5];
                            }
                            
                            
                        }
                        strUsecase = examName;
                        List<string> TestResults = new List<string>()
                        {
                            strClassification,strUsecase,strResult,strRemark
                        };

                        table.Rows.AddRange(new[] { CreatMergeRowBySpan(TestResults, SpanCount) });
                    }
                    
                }
            }
            table.Rows.AddRange(new[] { CreatBottomColorRow()[0], CreatBottomColorRow()[1], CreatBottomColorRow()[2] });
            tableList.Add(table);
            CreatePage(page, tableList);
            page++;
            y = 0;
        }



        #region 创建页面
        private void CreatePage(int j,List<object> tablelList)
        {
            DetailReportBand testBand = new DetailReportBand();
            testBand.PageBreak = PageBreak.BeforeBand;
            DetailBand testDetailBand = new DetailBand();

            //添加页面内容
            int i = tablelList.Count;
            for (int k = 0; k < i; k ++)
            {
                testDetailBand.Controls.Add((XRControl)tablelList[k]);  
            }
            testDetailBand.Height = 77;

            //创建页面的顺序
            testBand.Level = j;
            testBand.Bands.AddRange(new Band[] {testDetailBand});
            this.Bands.Add(testBand);
        }
        #endregion

        #region 绘制表头
        private XRTable CreatTableTitle(int X,int Y)
        {
            XRTable table = new XRTable();
            table.Location = new Point(X,Y);
            XRTableRow rowTitleOne = new XRTableRow();
            XRTableRow rowTitleTwo = new XRTableRow();
            XRTableCell cell1 = new XRTableCell();
            XRTableCell cell2 = new XRTableCell();
            XRTableCell cell3 = new XRTableCell();
            XRTableCell cell4 = new XRTableCell();

            XRTableCell cell5 = new XRTableCell();
            XRTableCell cell6 = new XRTableCell();
            XRTableCell cell7 = new XRTableCell();
            XRTableCell cell8 = new XRTableCell();
            XRTableCell cell9 = new XRTableCell();
            //XRTableCell cell10 = new XRTableCell();
            rowTitleOne.Cells.AddRange(new[] { cell1, cell2, cell3, cell4 });

            rowTitleTwo.Cells.AddRange(new[] { cell5, cell6, cell7, cell8, cell9 });

            rowTitleOne.BackColor = Color.Silver;
            rowTitleTwo.BackColor = Color.Silver;

            cell1.Text = "Testcase";
            cell1.Weight = 176;
            cell1.RowSpan = 2;

            cell2.Text = "Standard";
            cell2.Weight = 300;

            cell3.Text = "Test Vaule";
            cell3.Weight = 150;
            cell3.RowSpan = 2;

            cell4.Text = "Result";
            cell4.Weight = 150;
            cell4.RowSpan = 2;

            //row2

            cell5.Weight = 176;

            cell6.Text = "Standard";
            cell6.Weight = 150;

            cell7.Text = "Normal";
            cell7.Weight = 150;

            //cell8.Text = "Max";
            //cell8.Weight = 150;

            cell8.Weight = 150;

            cell9.Weight = 150;

            table.Rows.AddRange(new[] {
            rowTitleOne,
            rowTitleTwo});
            table.TextAlignment = TextAlignment.MiddleCenter;
            table.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            table.SizeF = new System.Drawing.SizeF(655.6473F, 60F);
            table.Borders = ((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Top)
                             | DevExpress.XtraPrinting.BorderSide.Right)
                            | DevExpress.XtraPrinting.BorderSide.Bottom;
            //table.LocationFloat = new DevExpress.Utils.PointFloat(22.21972F, 52.47222F);

            return table;
        }

        /// <summary>
        /// 一个四列的固定表头 分别为测试分类，测试用例，结果，备注
        /// </summary>
        /// <returns></returns>
        private XRTable CreatCellTableTitle(int x, int y)
        {
            XRTable table = new XRTable();
            table.Location = new Point(x, y);
            XRTableRow Row = new XRTableRow();
            XRTableCell cell1 = new XRTableCell();
            XRTableCell cell2 = new XRTableCell();
            XRTableCell cell3 = new XRTableCell();
            XRTableCell cell4 = new XRTableCell();
            Row.Cells.AddRange(new[] { cell1, cell2, cell3, cell4 });
            cell1.Weight = 176;
            cell2.Weight = 400;
            cell3.Weight = 100;
            cell4.Weight = 100;
            cell1.Text = "测试分类";
            cell2.Text = "测试用例";
            cell3.Text = "结果";
            cell4.Text = "备注";
            Row.BackColor = Color.Silver;
            table.Rows.AddRange(new[] { Row });
            table.TextAlignment = TextAlignment.MiddleCenter;
            table.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            table.SizeF = new System.Drawing.SizeF(655.6473F, 30F);
            table.Borders = ((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Top)
                             | DevExpress.XtraPrinting.BorderSide.Right)
                            | DevExpress.XtraPrinting.BorderSide.Bottom;
            return table;
        }
        /// <summary>
        /// 创建模块信息表的表头
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private XRTable CreatModuleInforTableTitle(float x, float y)
        {
            XRTable table = new XRTable();
            table.LocationF = new PointF(x,y);
            XRTableRow Row = new XRTableRow();
            XRTableCell cell1 = new XRTableCell();
            XRTableCell cell2 = new XRTableCell();
            XRTableCell cell3 = new XRTableCell();
            XRTableCell cell4 = new XRTableCell();
            XRTableCell cell5 = new XRTableCell();
            cell1.Weight = 100;
            cell2.Weight = 100;
            cell3.Weight = 100;
            cell4.Weight = 100;
            cell5.Weight = 100;
            Row.Cells.AddRange(new[] { cell1, cell2, cell3, cell4, cell5 });

            cell1.Text = "控制器名称";
            cell2.Text = "硬件版本号";
            cell3.Text = "软件版本号";
            cell4.Text = "零件号";
            cell5.Text = "序列号";
            //Row.BackColor = Color.ap;
            table.Rows.AddRange(new[] { Row });
            table.TextAlignment = TextAlignment.MiddleCenter;
            table.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            table.SizeF = new System.Drawing.SizeF(509F, 25F);
            table.Borders = ((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Top)
                             | DevExpress.XtraPrinting.BorderSide.Right)
                            | DevExpress.XtraPrinting.BorderSide.Bottom;
            return table;
        }
        /// <summary>
        /// 图片上方的一个表头
        /// </summary>
        /// <param name="TitleStr">图片表头名称</param>
        /// <returns></returns>
        private XRTable CreatPicTableTitle(string TitleStr)
        {
            XRTable table = new XRTable();
            XRTableRow Row = new XRTableRow();
            XRTableCell cell1 = new XRTableCell();
            Row.Cells.AddRange(new[] { cell1 });
            cell1.Weight = 776;
            cell1.Text = TitleStr;
            cell1.BackColor = Color.Silver;
            table.Rows.AddRange(new[] { Row });
            table.TextAlignment = TextAlignment.MiddleLeft;
            table.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            table.SizeF = new System.Drawing.SizeF(655.6473F, 30F);
            table.Borders = ((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Top)
                             | DevExpress.XtraPrinting.BorderSide.Right)
                            | DevExpress.XtraPrinting.BorderSide.Bottom;
            return table;
        }
        #endregion

        #region 绘制一行表格
        /// <summary>
        /// 第一列可合并的Row
        /// </summary>
        /// <param name="CellText">CellText为表格内数据 表格颜色通过半段CellText[2]来获得</param>
        /// <param name="Span">为该行第一格合并行数</param>
        /// <returns></returns>
        private XRTableRow CreatMergeRowBySpan(List<string> CellText, int Span)
        {
            XRTableRow Row = new XRTableRow();
            XRTableCell cell1 = new XRTableCell();
            XRTableCell cell2 = new XRTableCell();
            XRTableCell cell3 = new XRTableCell();
            XRTableCell cell4 = new XRTableCell();
            cell1.Weight = 176;
            cell2.Weight = 400;
            cell3.Weight = 100;
            cell4.Weight = 100;
            Row.Cells.AddRange(new[] { cell1, cell2, cell3, cell4 });
            cell1.RowSpan = Span;
            cell1.Text = CellText[0];
            cell2.Text = CellText[1];
            cell4.Text = CellText[3];
            if (CellText[2].ToLower() == "pass")
            {
                cell3.Text = CellText[2];
                cell3.BackColor = Color.Green;
            }
            else if (CellText[2].ToLower() == "fail")
            {
                cell3.Text = CellText[2];
                cell3.BackColor = Color.Red;
            }
            Row.TextAlignment = TextAlignment.MiddleCenter;
            cell2.TextAlignment = TextAlignment.MiddleLeft;
            return Row;
        }

        /// <summary>
        /// 返回一个List包含三行底部带颜色的Row
        /// </summary>
        /// <returns></returns>
        private List<XRTableRow> CreatBottomColorRow()
        {
            XRTableRow PassRow = new XRTableRow();
            XRTableRow FailRow = new XRTableRow();
            XRTableRow NARow = new XRTableRow();
            XRTableCell Passcell1 = new XRTableCell();
            XRTableCell Passcell2 = new XRTableCell();
            XRTableCell Failcell1 = new XRTableCell();
            XRTableCell Failcell2 = new XRTableCell();
            XRTableCell NAcell1 = new XRTableCell();
            XRTableCell NAcell2 = new XRTableCell();
            Passcell1.Weight = 176;
            Passcell2.Weight = 600;
            Failcell1.Weight = 176;
            Failcell2.Weight = 600;
            NAcell1.Weight = 176;
            NAcell2.Weight = 600;
            PassRow.Cells.AddRange(new[] { Passcell1, Passcell2 });
            FailRow.Cells.AddRange(new[] { Failcell1, Failcell2 });
            NARow.Cells.AddRange(new[] { NAcell1, NAcell2 });

            Passcell1.Text = "Pass";
            Passcell2.Text = "测试结果全部符合标准规定";
            Failcell1.Text = "Fail";
            Failcell2.Text = "测试结果不符合标准";
            NAcell1.Text = "N/A";
            NAcell2.Text = "未执行";

            PassRow.BackColor = Color.Green;
            FailRow.BackColor = Color.Red;
            PassRow.TextAlignment = TextAlignment.MiddleCenter;
            FailRow.TextAlignment = TextAlignment.MiddleCenter;
            NARow.TextAlignment = TextAlignment.MiddleCenter;

            List<XRTableRow> ListRow = new List<XRTableRow>
            {
                PassRow,FailRow,NARow
            };

            return ListRow;
        }

        /// <summary>
        /// 根据参数返回一个5格或7格的Row
        /// </summary>
        /// <param name="CellText">内有五或七个参数分别对应表格内数据</param>
        /// <param name="CellColor">最后一格内的颜色为绿色还是红色 true为绿色 false为红色</param>
        /// <param name="Span">一个Row内有几个格 true为Row内有五格的状态 false为七格状态</param>
        /// <returns></returns>
        private XRTableRow CreatRowBySpan(List<string> CellText,bool CellColor,bool Span)
        {
            XRTableRow Row = new XRTableRow();
            XRTableCell cell1 = new XRTableCell();
            XRTableCell cell2 = new XRTableCell();
            XRTableCell cell3 = new XRTableCell();
            XRTableCell cell4 = new XRTableCell();
            XRTableCell index = new XRTableCell();
            XRTableCell SmallCell1 = new XRTableCell();
            XRTableCell SmallCell2 = new XRTableCell();
            //XRTableCell SmallCell3 = new XRTableCell();
            index.Weight = 50;
            cell1.Weight = 126;
            cell2.Weight = 300;
            cell3.Weight = 150;
            cell4.Weight = 150;
            SmallCell1.Weight = 150;
            SmallCell2.Weight = 150;
            //SmallCell3.Weight = 100;
            if (Span)
            {
                Row.Cells.AddRange(new[] {index, cell1, cell2, cell3, cell4});
                index.Text = CellText[0];
                cell1.Text = CellText[1];
                cell2.Text = CellText[2];
                cell3.Text = CellText[3];
            }
            else
            {
                Row.Cells.AddRange(new[] {index, cell1, SmallCell1, SmallCell2,  cell3, cell4});
                index.Text = CellText[0];
                cell1.Text = CellText[1];
                SmallCell1.Text = CellText[2];
                SmallCell2.Text = CellText[3];
                //SmallCell3.Text = CellText[4];
                cell3.Text = CellText[4];
                //cell4.Text = CellText
            }
            if (CellColor)
            {
                cell4.BackColor=Color.Green;
                cell4.Text = "Pass";
            }
            else
            {
                cell4.BackColor = Color.Red;
                cell4.Text = "Fail";
            }
            Row.TextAlignment= TextAlignment.MiddleCenter;
            return Row;
        }

        private XRTableRow CreateModuleRow(List<string> CellText)
        {
            XRTableRow Row = new XRTableRow();
            XRTableCell cell1 = new XRTableCell();
            XRTableCell cell2 = new XRTableCell();
            XRTableCell cell3 = new XRTableCell();
            XRTableCell cell4 = new XRTableCell();
            XRTableCell cell5 = new XRTableCell();
            
            cell1.Weight = 100;
            cell2.Weight = 100;
            cell3.Weight = 100;
            cell4.Weight = 100;
            cell4.Weight = 100;
           
            Row.Cells.AddRange(new[] { cell1, cell2, cell3, cell4, cell5 });
                
            cell1.Text = CellText[0];
            cell2.Text = CellText[1];
            cell3.Text = CellText[2];
            cell4.Text = CellText[3];
            cell5.Text = CellText[4];
            Row.TextAlignment = TextAlignment.MiddleCenter;
            Row.SizeF = new System.Drawing.SizeF(509F, 25F);
            return Row;
        }

#endregion


        /// <summary>
        /// 根据上面行数传进来的List<bool>来判断该行为红色还是绿色
        /// </summary>
        /// <param name="CellColor">一个List内有多行bool值，如内有一个为False则该行变为红色值为Fail，如没有则为绿色值为Pass</param>
        /// <returns></returns>
        private XRTableRow CreatColorCellRow(List<bool> CellColor)
        {
            XRTableRow Row = new XRTableRow();
            XRTableCell cell1 = new XRTableCell();
            cell1.Weight = 776;
            Row.Cells.AddRange(new[] { cell1 });
            int i = 0;
            foreach (bool cell in CellColor)
            {
                if (!cell)
                    i++;
            }
            if (i == 0)
            {
                cell1.BackColor = Color.Green;
                cell1.Text = "Pass";
                Row.BackColor = Color.Green;
            }
            else
            {
                cell1.BackColor = Color.Red;
                cell1.Text = "Fail";
                Row.BackColor = Color.Red;
            }
            Row.TextAlignment = TextAlignment.MiddleCenter;
            return Row;
        }



        #region 控件
        /// <summary>
        /// 创建一个pictureBox控件，存放拓扑图
        /// </summary>
        /// <param name="image">要存放的图片</param>
        /// <returns></returns>
        private XRPictureBox CreatePictureBox(Image image)
        {
            XRPictureBox pictureBox = new XRPictureBox();
            pictureBox.Image = image;
            pictureBox.Size = new Size(656, 582);
            pictureBox.Sizing = ImageSizeMode.StretchImage;
            return pictureBox;
        }

        private XRLabel CreatTitle(string titleName, Single size)
        {
            XRLabel lblTitle = new XRLabel
            {
                Font =
                    new System.Drawing.Font("微软雅黑", size, System.Drawing.FontStyle.Regular,
                        System.Drawing.GraphicsUnit.Point, ((byte)(0))),
                ForeColor = System.Drawing.Color.Black,
                LocationFloat = new DevExpress.Utils.PointFloat(1.059638E-05F, 0F),
                Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F),
                SizeF = new System.Drawing.SizeF(511.1111F, 52.4722F),
                RowSpan = 3
            };

            lblTitle.StylePriority.UseFont = false;
            lblTitle.StylePriority.UseForeColor = false;
            lblTitle.StylePriority.UseTextAlignment = false;
            string[] title = titleName.Split(' ');
            lblTitle.Text = titleName;

            lblTitle.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
            return lblTitle;
        }
        #endregion

        #region 固定项赋值
        private void SetTestInformation(Dictionary<string, string> dict)
        {

            this.lblTester.Text = dict["TestUser"];
            this.lblTime.Text = dict["TestTime"];
            this.lblTestPosition.Text = dict["TestPosition"];
            this.lblTestType.Text = dict["TestType"];
            this.lblTestRound.Text = dict["TaskRound"];

            
        }
        /// <summary>
        /// 给参考文献处赋值
        /// </summary>
        /// <param name="refName"></param>
        private void SetReference(string refName)
        {
            this.xlReference.Text = refName;
        }
        /// <summary>
        /// 拓扑图和拓扑图说明
        /// </summary>
        /// <param name="listTply"></param>
        private void SetTopoloy(List<object> listTply)
        {
            if(listTply.Count == 2)
                this.lblReportNo.Text = GlobalVar.ReportTitle + "的轮网络通信测试报告。";
            else if(listTply.Count == 3)
            {
                this.lblReportNo.Text = listTply[2] + "的轮网络通信测试报告。";
            }
            this.xrLabel36.Text = listTply[1].ToString();
            this.xpbToply.Image = (Image)listTply[0];
            
        }
        /// <summary>
        /// 测试概况表赋值
        /// </summary>
        /// <param name="itemList"></param>
        private void SetTestPercent(List<int> itemList)
        {
            this.testItemCount.Text = itemList[0].ToString();
            this.testedItems.Text = itemList[1].ToString();
            this.passItem.Text = itemList[2].ToString();
            this.successItem.Text = itemList[3].ToString();
            this.failItem.Text = itemList[4].ToString();
            this.testedItemPer.Text = (int.Parse(testedItems.Text) * 1.0 / int.Parse(testItemCount.Text)).ToString("P1");
            this.passItemPer.Text = (int.Parse(passItem.Text) * 1.0 / int.Parse(testItemCount.Text)).ToString("P1");
            this.successItemPer.Text = (int.Parse(successItem.Text) * 1.0 / int.Parse(testItemCount.Text)).ToString("P1");
            this.failItemPer.Text = (int.Parse(failItem.Text) * 1.0 / int.Parse(testItemCount.Text)).ToString("P1");
        }
        #endregion

        /// <summary>
        /// 清除List里为""的项并重新返回一个List
        /// </summary>
        /// <param name="ListTemp"></param>
        /// <returns></returns>
        private List<string> ListSorting(List<string> ListTemp)
        {
            List<string> ListSorting = new List<string>();
            foreach (var Temp in ListTemp)
            {
                if (Temp != "")
                {
                    ListSorting.Add(Temp);
                }
            }
            return ListSorting;
        }

        private List<string> NewListSorting(List<string> ListTemp, List< string> newList )
        {
            //List<string> ListSorting = new List<string>();
            for (int i = 0; i < ListTemp.Count; i++)
            {
                if (i == 0)
                {
                    continue;
                }
                else
                {
                    newList.Add(ListTemp[i]);
                   
                }
            }
           
            return newList;
        }

    }
}
