using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.LookAndFeel;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Columns;
using Model;
using ProcessEngine;
using UltraANetT.Interface;
using System.IO;
using DevExpress.XtraLayout.Utils;
using DBEngine;
using System.Text;
using System.Linq;

namespace UltraANetT.Form
{
    public partial class UploadDBCAndTply : DevExpress.XtraBars.Ribbon.RibbonForm, IDraw
    {
        private Dictionary<string, object> _dictDBC = new Dictionary<string, object>();
        private Dictionary<string, object> _dictTask = new Dictionary<string, object>();
        private readonly ProcShow _show = new ProcShow();
        private ProcStore _store = new ProcStore();
        private ProcFile _file = new ProcFile();
        private ProcLog Log = new ProcLog();
        private readonly IDraw _draw;
        private string tplyPath;
        private string tplVehicel;
        private Dictionary<string, object> _dictFLBV = new Dictionary<string, object>();
        private string DbcName = "";

        /// <summary>
        /// 当前操作类别
        /// </summary>
        private DataOper _currentOperate;

        /// <summary>
        /// 定义一行数据
        /// </summary>
        private DataRow _dr;

        public UploadDBCAndTply()
        {
            InitializeComponent();
            _draw = this;
            _draw.InitDict();
            _draw.InitGrid();
            InitCAN();
            LoadUpLoaderItem();
            GetDataBaseTply();
            txtUpLoader.Text = GlobalVar.UserName;
            barStaticItem1.Caption= GlobalVar.UserName;
            txtUpLoader.ReadOnly = true;
            //dateUpload.Text = DateTime.Now.ToString("yyyy-MM-dd");
            dateUpload.DateTime = DateTime.Now;
            dateUpload.ReadOnly = true;
            InitBusProtocol();
        }

        //初始化总线协议
        private void InitBusProtocol()
        {
            List<String> list = new List<string>();
            list.Add("ISO11898");
            list.Add("J1939");
            for (int i = 0; i < list.Count; i++)
            {
                comboBoxEdit1.Properties.Items.Add(list[i]);
            }

            comboBoxEdit1.SelectedItem = "ISO11898";
        }

        private void InitCAN()
        {
            var dept = _store.GetSingnalCol(EnumLibrary.EnumTable.Segment, 0);
            foreach (var item in dept)
                cbCAN.Properties.Items.Add(item);

        }

        private enum DataOper
        {
            Add = 0,
            Update = 1,
            Del = 2
        }

        private enum MessageBoxOper
        {
            Null = 0,
            Success = 1,
            Fail = 2
        }

        void IDraw.InitDict()
        {
            _dictDBC.Add("VehicelType", "");
            _dictDBC.Add("VehicelConfig", "");
            _dictDBC.Add("VehicelStage", "");
            _dictDBC.Add("DBCName", "");
            _dictDBC.Add("BelongCAN", "");
            _dictDBC.Add("DBCContent", null);
            _dictDBC.Add("ImportUser", "");
            _dictDBC.Add("ImportTime", "");
            _dictDBC.Add("FormerDBCName", "");
            _dictDBC.Add("CANType", "");

            _dictFLBV.Add("VehicelType", "");
            _dictFLBV.Add("VehicelConfig", "");
            _dictFLBV.Add("VehicelStage", "");
            _dictFLBV.Add("Tply", "");
            _dictFLBV.Add("TplyDescrible", "");
        }

        void IDraw.InitGrid()
        {
            var coList = new List<string>();
            foreach (GridColumn col in gvDBC.Columns)
                coList.Add(col.FieldName);
            var dt = new DataTable();
            foreach (var colName in coList)
                dt.Columns.Add(new DataColumn(colName, typeof(object)));
            //var dt = _show.DrawDtFromMultiple(coList.ToArray(), EnumLibrary.EnumTable.DBC);
            _dictDBC["VehicelType"] = GlobalVar.CurrentVNode[0];
            _dictDBC["VehicelConfig"] = GlobalVar.CurrentVNode[1];
            _dictDBC["VehicelStage"] = GlobalVar.CurrentVNode[2];
            IList<object[]> vehicel1 = _store.GetSpecialByEnum(EnumLibrary.EnumTable.DBC, _dictDBC);
            IList<object[]> vehicel = Tran(vehicel1);
            if (vehicel.Count > 0)
            {
                dt.Rows.Add(vehicel[vehicel.Count - 1]);
                int count = 0;
                foreach (var dept in vehicel)
                {
                    if (count < vehicel.Count - 1)
                        dt.Rows.Add(dept);
                    count++;
                }
            }

            gcDBC.DataSource = dt;
        }

        private IList<object[]> Tran(IList<object[]> vehicel)
        {
            List<String> list = new List<String>();
            for (int i = 0; i < vehicel.Count; i++)
            {
                list = new List<String>();
                for (int j = 0; j < vehicel[i].Length; j++)
                {
                    // 0=0;1=1;2=2;3=4;4=3;5=9;6=5;7=3;8=6;9=7
                    if (vehicel[i][j] != null)
                    {
                        list.Add(vehicel[i][j].ToString());
                    }
                    else
                    {
                        list.Add("");
                    }
                }

                for (int k = 0; k < list.Count; k++)
                {
                    if (k == 3)
                    {
                        vehicel[i][k] = list[k + 1];
                    }
                    else if (k == 4)
                    {
                        vehicel[i][k] = list[k + 4];
                    }
                    else if (k == 5)
                    {
                        vehicel[i][k] = list[k + 4];
                    }
                    else if (k == 6)
                    {
                        vehicel[i][k] = list[k - 3];
                    }
                    else if (k == 7)
                    {
                        vehicel[i][k] = list[k - 2];
                    }
                    else if (k == 8)
                    {
                        vehicel[i][k] = list[k - 2];
                    }
                    else if (k == 9)
                    {
                        vehicel[i][k] = list[k - 2];
                    }
                }
            }

            return vehicel;
        }

        Dictionary<string, object> IDraw.GetDataFromUI()
        {
            throw new NotImplementedException();
        }

        void IDraw.SetDataToUI(DataRow selectedRow)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 添加员工操作记录时 如果记录是Dictionary类型可直接调用这个方法
        /// </summary>
        /// <param name="Oper">操作类型</param>
        /// <returns></returns>
        private Dictionary<string, object> UserDict()
        {
            _dictDBC["EmployeeName"] = GlobalVar.UserName;
            _dictDBC["EmployeeNo"] = GlobalVar.UserNo;
            _dictDBC["Department"] = GlobalVar.UserDept;
            return _dictDBC;
        }

        /// <summary>
        /// 添加员工操作记录时 如果记录是DataRow类型则调用这个方法 还可以用于DataRow转换成Dictionary </summary>
        /// <param name="Oper">操作类型</param>
        /// <returns></returns>
        private Dictionary<string, object> UserDr()
        {
            UserDict();
            _dictDBC["VehicelType"] = _dr["VehicelType"];
            _dictDBC["VehicelConfig"] = _dr["VehicelConfig"];
            _dictDBC["VehicelStage"] = _dr["VehicelStage"];
            _dictDBC["DBCName"] = _dr["DBCName"];
            _dictDBC["BelongCAN"] = _dr["BelongCAN"];
            _dictDBC["DBCContent"] = _dr["DBCContent"];
            _dictDBC["ImportUser"] = _dr["ImportUser"];
            _dictDBC["ImportTime"] = _dr["ImportTime"];

            return _dictDBC;
        }

        void IDraw.Submit()
        {
            switch (_currentOperate)
            {
                case DataOper.Add:
                    if (Upload())
                    {
                        Dictionary<string, object> dictConfig = new Dictionary<string, object>
                        {
                            {"EmployeeNo", GlobalVar.UserNo},
                            {"EmployeeName", GlobalVar.UserName},
                            {"OperTable", "DBC"},
                            {"DBCName", _dictDBC["DBCName"]},
                            {"BelongCAN", _dictDBC["BelongCAN"]},
                        };
                        Log.WriteLog(EnumLibrary.EnumLog.AddDBC, dictConfig);
                    }

                    break;
                //删除
                case DataOper.Del:
                    if (XtraMessageBox.Show("该车型的配置信息、拓扑图信息、供应商信息和任务列表中的记录也会被删除，确定删除当前选中记录么？", "提示",
                            MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) ==
                        DialogResult.OK)
                    {
                        if (XtraMessageBox.Show("该车型的配置信息、拓扑图信息、供应商信息和任务列表中的记录也会被删除，请再次确定删除当前选中记录么？", "提示",
                                MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) ==
                            DialogResult.OK)
                        {
                            string error;
                            string strTestChannel = _dr["BelongCAN"].ToString();
                            //删除DBC
                            if (_store.Del(EnumLibrary.EnumTable.DBC, UserDr(), out error))
                            {
                                File.Delete(AppDomain.CurrentDomain.BaseDirectory + _dictDBC["DBCContent"]);
                            }
                            else
                            {
                                XtraMessageBox.Show("删除失败！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            //_store.Del(EnumLibrary.EnumTable.FileLinkByVehicel, UserDr(), out error);
                            //删除拓扑图
                            _store.Del(EnumLibrary.EnumTable.Topology, UserDr(), out error);
                            string[] busType =
                                _file.DBCCANConvert(_dr["BelongCAN"].ToString().Substring(0, 3)).Split('&');
                            foreach (var matchSort in busType)
                            {
                                _dictDBC["MatchSort"] = matchSort;
                                var fList = _store.GetSpecialByEnum(EnumLibrary.EnumTable.FileLinkByVehicelDouble,
                                    _dictDBC);
                                List<Dictionary<string, string>> listNewCfg = new List<Dictionary<string, string>>();
                                foreach (var f in fList)
                                {
                                    try
                                    {
                                        var dictcfg = Json.DerJsonToLDict(f[7].ToString());
                                        foreach (var cfg in dictcfg)
                                        {
                                            if (cfg["TestChannel"] != strTestChannel)
                                            {
                                                listNewCfg.Add(cfg);
                                            }
                                        }

                                        Dictionary<string, object> dictTemp = new Dictionary<string, object>();
                                        dictTemp["VehicelType"] = _dictDBC["VehicelType"];
                                        dictTemp["VehicelConfig"] = _dictDBC["VehicelConfig"];
                                        dictTemp["VehicelStage"] = _dictDBC["VehicelStage"];
                                        dictTemp["MatchSort"] = _dictDBC["MatchSort"];
                                        dictTemp["CfgTemplateName"] = f[5].ToString();
                                        if (listNewCfg.Count > 0)
                                        {
                                            dictTemp["CfgTemplateJson"] = Json.SerJson(listNewCfg);
                                            //修改
                                            _store.Update(EnumLibrary.EnumTable.FileLinkByCfg, dictTemp, out error);
                                        }
                                        else
                                        {
                                            //删除
                                            _store.Del(EnumLibrary.EnumTable.FileLinkByCfg, dictTemp, out error);
                                        }
                                    }
                                    catch (Exception EX_NAME)
                                    {
                                        Debug.Print(EX_NAME.ToString());
                                    }
                                }
                            }

                            Dictionary<string, object> taskNoDict = new Dictionary<string, object>();
                            taskNoDict["TaskNo"] = _dictDBC["VehicelType"] + "-" + _dictDBC["VehicelConfig"] + "-" +
                                                   _dictDBC["VehicelStage"];
                            taskNoDict["CANRoad"] = strTestChannel;
                            //删除任务列表
                            _store.Del(EnumLibrary.EnumTable.TaskByCANRoad, taskNoDict, out error);

                            if (error == "")
                            {
                                _draw.InitGrid();
                                Dictionary<string, object> dictConfig = new Dictionary<string, object>
                                {
                                    {"EmployeeNo", GlobalVar.UserNo},
                                    {"EmployeeName", GlobalVar.UserName},
                                    {"OperTable", "DBC"},
                                    {"DBCName", _dictDBC["DBCName"]},
                                    {"BelongCAN", _dictDBC["BelongCAN"]},
                                };
                                Log.WriteLog(EnumLibrary.EnumLog.DelDBC, dictConfig);
                                File.Delete(AppDomain.CurrentDomain.BaseDirectory + _dictDBC["DBCContent"]);
                            }
                        }
                    }

                    break;
            }
        }

        void IDraw.SwitchCtl(bool isSwitch)
        {
            switch (isSwitch)
            {
                case true:
                    btnUpload.Enabled = true;
                    break;
                case false:
                    btnUpload.Enabled = false;
                    break;
            }
        }




        private void MessageBoxShow(string isString, MessageBoxOper Oper)
        {
            switch (Oper)
            {
                case MessageBoxOper.Null:
                    XtraMessageBox.Show(isString);
                    break;
                case MessageBoxOper.Success:
                    _draw.InitGrid();
                    btSelect.Text = null;
                    XtraMessageBox.Show(isString);
                    _draw.SwitchCtl(false);
                    break;
                case MessageBoxOper.Fail:
                    _draw.InitGrid();
                    XtraMessageBox.Show(isString);
                    break;
            }

        }

        private void tsmiDel_Click(object sender, EventArgs e)
        {
            if (_dr == null)
            {
                XtraMessageBox.Show("请先选中一行后再做删除操作");
                return;
            }

            _currentOperate = DataOper.Del;

            _draw.Submit();
            _dr = null;
        }


        /// <summary>
        ///  </summary>
        private bool Upload()
        {
            string error = "";
            bool DBCRepeat = true;
            if (!File.Exists(btSelect.Text))
            {
                MessageBoxShow("选择的路径文件丢失，请重新选择！", MessageBoxOper.Null);
                return false;
            }

            if (cbCAN.Text == "" || btSelect.Text == "")
            {
                MessageBoxShow("参数为空", MessageBoxOper.Null);
                //Show(DLAF.LookAndFeel, this, "参数为空...", "", new[] { DialogResult.OK }, null, 0, MessageBoxIcon.Information);
            }
            else
            {
                _dictDBC["VehicelType"] = GlobalVar.CurrentVNode[0];
                _dictDBC["VehicelConfig"] = GlobalVar.CurrentVNode[1];
                _dictDBC["VehicelStage"] = GlobalVar.CurrentVNode[2];
                _dictDBC["BelongCAN"] = cbCAN.Text;
                _dictDBC["ImportTime"] = DateTime.Now;
                _dictDBC["ImportUser"] = GlobalVar.UserName;
                _dictDBC["FormerDBCName"] = DbcName;
                bool isCAN = false;
                if (cbCAN.Text.Trim().Contains("CAN"))
                {
                    _dictDBC["CANType"] = comboBoxEdit1.SelectedItem.ToString();
                    isCAN = true;
                }
                else
                {
                    _dictDBC["CANType"] = "";
                    isCAN = false;
                }

                #region 更换数据库文件

                if (DataComparison(_dictDBC))
                {
                    List<string> dbc = new List<string>();
                    dbc.Add(GlobalVar.CurrentVNode[0]);
                    dbc.Add(GlobalVar.CurrentVNode[1]);
                    dbc.Add(GlobalVar.CurrentVNode[2]);
                    dbc.Add(cbCAN.Text.ToString().Trim());
                    bool IsExistDBC;
                    //旧数据库
                    List<string> nodeListOld = _show.GetDataFromDbc(dbc, out IsExistDBC);
                    var exValue = 0;
                    var path = btSelect.Text;
                    //新数据库
                    List<string> nodeListNew = _show.ObtainNode(isCAN, exValue, path);
                    if (nodeListOld.Count != nodeListNew.Count)
                    {
                        //arrAdd：对比旧的数据库文件，新的数据库文件缺少的节点
                        string[] arrAdd = nodeListOld.Except(nodeListNew).ToArray();
                        StringBuilder sb = new StringBuilder();
                        if (arrAdd.Length == 0)
                        {
                            for (int i = 0; i < nodeListOld.Count; i++)
                            {
                                sb.Append(nodeListOld[i]);
                                if (i < nodeListOld.Count - 1)
                                {
                                    sb.Append(",");
                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i < arrAdd.Length; i++)
                            {
                                sb.Append(arrAdd[i]);
                                if (i < arrAdd.Length - 1)
                                {
                                    sb.Append(",");
                                }
                            }
                        }

                        string node = sb.ToString();
                        if (XtraMessageBox.Show("和配置表信息对比，新的数据库文件缺少：" + node + " 节点", "提示", MessageBoxButtons.OKCancel,
                                MessageBoxIcon.Warning) ==
                            DialogResult.OK)
                        {
                            if (XtraMessageBox.Show("再次确定是否更换数据库文件么？", "提示", MessageBoxButtons.OKCancel,
                                    MessageBoxIcon.Warning) ==
                                DialogResult.OK)
                            {
                                List<string> list = _show.ObtainCorrntImf(dbc);
                                Dictionary<string, object> dictDBC = new Dictionary<string, object>
                                {
                                    {"VehicelType", list[0]},
                                    {"VehicelConfig", list[1]},
                                    {"VehicelStage", list[2]},
                                    {"DBCName", list[3]},
                                    {"BelongCAN", list[4]},
                                    {"DBCContent", list[5]},
                                    {"ImportUser", list[6]},
                                    {"ImportTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")},
                                    {"FormerDBCName", DbcName},
                                    {"CANType", comboBoxEdit1.SelectedItem.ToString()},
                                };
                                string errorU;
                                bool success = _store.Update(EnumLibrary.EnumTable.DBC, dictDBC, out errorU);
                                _draw.InitGrid();
                                SaveDBCFile1();
                                if (success)
                                {
                                    MessageBoxShow("更换成功...", MessageBoxOper.Success);
                                }
                                else
                                {
                                    MessageBoxShow("上传失败...", MessageBoxOper.Fail);
                                }
                            }
                        }
                    }
                    else
                    {
                        MessageBoxShow("上传两次的数据库文件是相同的...", MessageBoxOper.Success);
                    }

                    return false;
                }

                #endregion

                if (SaveDBCFile())
                {
                    //上传DBC信息
                    _store.AddDBC(_dictDBC, out error);
                }
                else
                {
                    error = "复制文件时出错。";
                }

                if (error == "")
                {
                    MessageBoxShow("上传成功...", MessageBoxOper.Success);
                    DbcName = "";
                    gvDBC.BestFitColumns();
                    //Show(DLAF.LookAndFeel, this, "上传成功...", "", new[] { DialogResult.OK }, null, 0, MessageBoxIcon.Information);
                    return true;
                }
                else
                {
                    MessageBoxShow("上传失败,请检查是否参数错误...", MessageBoxOper.Fail);
                    //Show(DLAF.LookAndFeel, this, "上传失败,请检查是否参数错误...", "", new[] { DialogResult.OK }, null, 0, MessageBoxIcon.Information);
                }
            }

            return false;
        }

        /// <summary>
        /// 通过查询数据库判断数据库内是否有相同参数
        /// </summary>
        /// <param name="_dictDBC"></param>
        /// <returns></returns>
        private bool DataComparison(Dictionary<string, object> _dictDBC)
        {
            IList<object[]> dataList = _store.GetSpecialByEnum(EnumLibrary.EnumTable.DBCBelongCAN, _dictDBC);
            if (dataList.Count > 0)
                return true;
            return false;
        }

        /// <summary>
        /// 选择文件并获取文件信息并赋值
        /// </summary>
        private void FileAdd()
        {
            GlobalVar.NumberChanges = 1;
            var ofd = new OpenFileDialog();
            if (cbCAN.Text.Substring(0, 3).ToUpper() != "LIN")
            {
                ofd.Filter = "(*.dbc;*.DBC)|*.dbc;*.DBC";
            }
            else
            {
                ofd.Filter = "(*.ldf;*.LDF)|*.ldf;*.LDF";
            }

            var dr = ofd.ShowDialog();
            DbcName = ofd.SafeFileName;
            if (dr != DialogResult.OK)
            {
                return;
            }

            //获取文件后缀名
            string strExtension = System.IO.Path.GetExtension(ofd.SafeFileName);
            //判断文件后缀名并赋值
            if (ExtensionIsRight(strExtension))
            {
                _dictDBC["DBCName"] = GlobalVar.CurrentVNode[0] + "-" + GlobalVar.CurrentVNode[1] + "-" +
                                      GlobalVar.CurrentVNode[2] + "-" + cbCAN.Text + strExtension;
                string dbcVehicel = @"dbc\" + _dictDBC["DBCName"];
                _dictDBC["DBCContent"] = dbcVehicel;
                _draw.SwitchCtl(true);
                btSelect.Text = ofd.FileName;
            }
        }

        private bool SaveDBCFile()
        {
            string filePath = btSelect.Text;
            string dbcPath = AppDomain.CurrentDomain.BaseDirectory + @"dbc\";
            string path = AppDomain.CurrentDomain.BaseDirectory + _dictDBC["DBCContent"];
            if (!Directory.Exists(dbcPath))
                Directory.CreateDirectory(dbcPath);

            bool isrewrite = true; // true=覆盖已存在的同名文件,false则反之
            try
            {
                File.Copy(filePath, path, isrewrite);
            }
            catch (Exception e)
            {
                MessageBoxShow(e.Message, MessageBoxOper.Fail);
                return false;
            }

            return true;
        }

        private bool SaveDBCFile1()
        {
            string filePath = btSelect.Text;
            string dbcPath = AppDomain.CurrentDomain.BaseDirectory + @"dbc\";
            string path = AppDomain.CurrentDomain.BaseDirectory + _dictDBC["DBCContent"];
            bool isrewrite = true; // true=覆盖已存在的同名文件,false则反之
            try
            {
                File.Copy(filePath, path, isrewrite);
            }
            catch (Exception e)
            {
                MessageBoxShow(e.Message, MessageBoxOper.Fail);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 判断文件后缀名是否正确
        /// </summary>
        /// <param name="strExtension">文件后缀名</param>
        /// <returns></returns>
        private bool ExtensionIsRight(string strExtension)
        {
            //判断上传文件后缀是否为dbc

            if (strExtension.ToLower() == ".dbc" || strExtension.ToLower() == ".ldf")
            {
                return true;
            }

            Show(DLAF.LookAndFeel, this, "文件导入异常...", "", new[] {DialogResult.OK}, null, 0, MessageBoxIcon.Information);
            btSelect.Text = "";
            _draw.SwitchCtl(false);
            return false;

        }


        public static DialogResult Show(UserLookAndFeel look, IWin32Window owner, string text, string caption,
            DialogResult[] buttons, Icon icon, int defaultButton, MessageBoxIcon messageBeepSound)
        {

            XtraMessageBoxForm form = new XtraMessageBoxForm();
            Font defaultFont = new System.Drawing.Font("微软雅黑", 9.5F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            form.Appearance.Font = defaultFont;
            return form.ShowMessageBoxDialog(new XtraMessageBoxArgs(look, owner, text, caption, buttons, icon,
                defaultButton));
        }

        private void UpLoadDBC_FormClosed(object sender, FormClosedEventArgs e)
        {
            Dictionary<string, object> dictNode = new Dictionary<string, object>();
            dictNode.Add("VehicelType", GlobalVar.CurrentVNode[0]);
            dictNode.Add("VehicelConfig", GlobalVar.CurrentVNode[1]);
            dictNode.Add("VehicelStage", GlobalVar.CurrentVNode[2]);
            IList<object[]> dbcList = _store.GetSpecialByEnum(EnumLibrary.EnumTable.DbcCheck, dictNode);
            if (dbcList.Count != 0)
            {
                GlobalVar.UpDbcOk = true;
            }

        }

        private void btnSearch_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
            try
            {
                var ofd = new OpenFileDialog();
                ofd.Filter = "(*.jpg,*.jpeg,*.gif,*.bmp;*.png;)|*.jpg;*.jpeg;*.gif;*.bmp;*.png;";
                var dr = ofd.ShowDialog();
                string picName = ofd.SafeFileName;
                if (dr != DialogResult.OK) return;
                tplVehicel = GlobalVar.CurrentVNode[0] + "-" + GlobalVar.CurrentVNode[1] + "-" +
                             GlobalVar.CurrentVNode[2];
                //var stream = ofd.OpenFile();
                tplyPath = @"topology\" + tplVehicel + @"\" + picName;

                _dictFLBV["Tply"] = tplyPath;

                btnSearch.Text = ofd.FileName;
                Bitmap bmp = new Bitmap(ofd.FileName);
                ptTopology.Image = bmp;
                upLoadBtn.Enabled = true;
                //MemoryStream ms = new MemoryStream();
                //bmp.Save(ms, ImageFormat.Jpeg);
                //byte[] arr = new byte[ms.Length];
                //ms.Position = 0;
                //ms.Read(arr, 0, (int)ms.Length);
                //ms.Close();

                //string erroru;
                //pic = BArrayToHex(arr, out erroru);



                //_dictFLBV["Topology"] = pic;
                // ptTopology.Image = Image.FromFile(btnSearch.Text); 
            }
            catch (Exception ex)
            {
                Show(DLAF.LookAndFeel, this, "不是有效地图片文件...", "", new[] {DialogResult.OK}, null, 0,
                    MessageBoxIcon.Information);
                throw;
            }

        }

        private void LoadUpLoaderItem()
        {
            IList<string> emNoList = _store.GetSingnalCol(EnumLibrary.EnumTable.Employee, 0);
            IList<string> emNameList = _store.GetSingnalCol(EnumLibrary.EnumTable.Employee, 1);
            IList<string> emRoleList = _store.GetSingnalCol(EnumLibrary.EnumTable.Employee, 2);
            txtUpLoader.Properties.Items.Clear();
            for (int i = 0; i < emNameList.Count; i++)
            {
                if (emRoleList[i] == "管理员" || emRoleList[i] == "配置员")
                    txtUpLoader.Properties.Items.Add(emNameList[i] + "(" + emNoList[i] + ")");
            }
        }

        private void GetDataBaseTply()
        {
            object firstRow = "";
            _dictFLBV["VehicelType"] = GlobalVar.CurrentVNode[0];
            _dictFLBV["VehicelConfig"] = GlobalVar.CurrentVNode[1];
            _dictFLBV["VehicelStage"] = GlobalVar.CurrentVNode[2];
            var tply = _store.GetSpecialByEnum(EnumLibrary.EnumTable.Topology, _dictFLBV);
            if (tply.Count > 0)
            {
                firstRow = tply[0][3];
            }

            if (firstRow.ToString() != "")
            {
                if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + firstRow))
                {
                    Show(DLAF.LookAndFeel, this, "该车型下的拓扑图疑似被删除，请重新上传...", "", new[] {DialogResult.OK}, null, 0,
                        MessageBoxIcon.Information);
                    toUpload.IsOn = false;
                }
                else
                {
                    ptOldTopology.Image = Image.FromFile(AppDomain.CurrentDomain.BaseDirectory + firstRow);
                    txtTplyDescrible.Text = tply[0][4].ToString();
                    toUpload.IsOn = true;
                }

            }
            else
            {
                toUpload.IsOn = false;
            }

        }

        private void cbCAN_SelectedIndexChanged(object sender, EventArgs e)
        {
            GlobalVar.NumberChanges = 1;
            btSelect.Text = "";
            if (cbCAN.Text != "")
            {
                if (cbCAN.Text.Trim().Length > 3)
                {
                    btSelect.Enabled = true;
                    _dictDBC["BelongCAN"] = cbCAN.SelectedText;
                }
                else
                {
                    XtraMessageBox.Show("所选网段不符合规范，请在网段表处修改此网段");
                    btSelect.Enabled = false;
                }

                if (cbCAN.Text.Trim().Contains("CAN"))
                {
                    layoutControlItem14.Visibility = LayoutVisibility.Always;
                }
                else
                {
                    layoutControlItem14.Visibility = LayoutVisibility.Never;
                }
            }
            else
            {
                btSelect.Enabled = false;
            }
        }

        private void gvDBC_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                //获得光标位置
                var hi = gvDBC.CalcHitInfo(e.Location);
                //判断位置是否在行位置上
                if (!hi.InRow && !hi.InRowCell) return;
                if (hi.RowHandle < 0) return;
                //取一行值
                gvDBC.SelectRow(hi.RowHandle);
                _dr = this.gvDBC.GetDataRow(hi.RowHandle);
            }
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            if (btSelect.Text == "")
            {
                XtraMessageBox.Show("路径为空，请选择相应.dbc .ldf文件");
                return;
            }

            _currentOperate = DataOper.Add;
            _draw.Submit();
        }

        private void btSelect_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            FileAdd();
        }

        private void upLoadBtn_Click(object sender, EventArgs e)
        {
            _dictFLBV["TplyDescrible"] = txtTplyDescrible.Text;
            _dictFLBV["ImportUser"] = GlobalVar.UserName;
            _dictFLBV["ImportTime"] = DateTime.Now;
            PrepareFile(btnSearch.Text);
        }

        private void PrepareFile(string btnText)
        {
            try
            {
                if (btnText == "")
                {
                    Show(DLAF.LookAndFeel, this, "请选择要上传的文件...", "", new[] {DialogResult.OK}, null, 0,
                        MessageBoxIcon.Information);
                    return; //没有选好文件则跳出方法体外
                }

                //如果选好了文件 
                var TplyList = _store.GetSpecialByEnum(EnumLibrary.EnumTable.Topology, _dictFLBV);

                string error = "";
                if (TplyList.Count != 0)
                {
                    _store.Update(EnumLibrary.EnumTable.Topology, _dictFLBV, out error);
                    Dictionary<string, object> dictConfig = new Dictionary<string, object>
                    {
                        {"EmployeeNo", GlobalVar.UserNo},
                        {"EmployeeName", GlobalVar.UserName},
                        {"OperTable", "车型"},
                        {"VehicelType", GlobalVar.CurrentVNode[0]},
                        {"VehicelConfig", GlobalVar.CurrentVNode[1]},
                        {"VehicelStage", GlobalVar.CurrentVNode[2]}
                    };
                    Log.WriteLog(EnumLibrary.EnumLog.UpdateTply, dictConfig);
                }
                else
                {
                    Dictionary<string, object> dictTylp = new Dictionary<string, object>();
                    dictTylp["VehicelType"] = _dictFLBV["VehicelType"];
                    dictTylp["VehicelConfig"] = _dictFLBV["VehicelConfig"];
                    dictTylp["VehicelStage"] = _dictFLBV["VehicelStage"];
                    dictTylp["Tply"] = _dictFLBV["Tply"];
                    dictTylp["TplyDescrible"] = _dictFLBV["TplyDescrible"];
                    dictTylp["ImportUser"] = _dictFLBV["ImportUser"];
                    dictTylp["ImportTime"] = _dictFLBV["ImportTime"];
                    _store.AddTopology(dictTylp, out error);
                }

                if (error != "")
                {
                    XtraMessageBox.Show("拓扑图上传出错，请联系工程师帮助解决");
                    return;
                }
                else
                {
                    string tplPath = AppDomain.CurrentDomain.BaseDirectory + @"topology\" + tplVehicel + @"\";
                    if (!Directory.Exists(tplPath))
                        Directory.CreateDirectory(tplPath);
                    string path = AppDomain.CurrentDomain.BaseDirectory + tplyPath;
                    if (ptOldTopology.Image != null)
                        ptOldTopology.Image.Dispose();
                    File.Delete(AppDomain.CurrentDomain.BaseDirectory + tplyPath);
                    ptTopology.Image.Save(AppDomain.CurrentDomain.BaseDirectory + tplyPath);

                    Show(DLAF.LookAndFeel, this, "上传成功...", "", new[] {DialogResult.OK}, null, 0,
                        MessageBoxIcon.Information);
                    toUpload.IsOn = true;
                    if (error == "")
                    {
                        Dictionary<string, object> dictConfig = new Dictionary<string, object>
                        {
                            {"EmployeeNo", GlobalVar.UserNo},
                            {"EmployeeName", GlobalVar.UserName},
                            {"OperTable", "车型"},
                            {"VehicelType", GlobalVar.CurrentVNode[0]},
                            {"VehicelConfig", GlobalVar.CurrentVNode[1]},
                            {"VehicelStage", GlobalVar.CurrentVNode[2]}
                        };
                        Log.WriteLog(EnumLibrary.EnumLog.Tply, dictConfig);
                    }

                    upLoadBtn.Enabled = false;
                    ptOldTopology.Image = null;
                    btnSearch.Text = null;
                }
            }
            catch (Exception ex)
            {
                Show(DLAF.LookAndFeel, this, ex.Message.ToString(), "", new[] {DialogResult.OK}, null, 0,
                    MessageBoxIcon.Information);
            }
        }

        private void hllblNextPage_Click(object sender, EventArgs e)
        {
            if (xtbcTab.SelectedTabPage == tabDBC)
            {
                xtbcTab.SelectedTabPage = tabTy;
            }
        }

        private void UploadDBCAndTply_FormClosing(object sender, FormClosingEventArgs e)
        {
            var KF = false;
            if (GlobalVar.NumberChanges != 0)
            {
                if (XtraMessageBox.Show("检测到当前数据未保存！是关闭页面?", "提示", MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Warning) == DialogResult.OK)
                {
                    KF = true;
                    GlobalVar.NumberChanges = 0;
                }
            }
            else
            {
                KF = true;
            }

            if (KF)
            {
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
            }
        }
    }
}