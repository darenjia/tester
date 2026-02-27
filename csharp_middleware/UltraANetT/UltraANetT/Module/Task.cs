using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using DevExpress.LookAndFeel;
using DevExpress.Skins;
using DevExpress.Spreadsheet;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraTreeList.Nodes;
using FileEditor;
using ProcessEngine;
using UltraANetT.Form;
using UltraANetT.Interface;
using DevExpress.XtraEditors.Controls;
using System.Linq;
using DevExpress.XtraBars.Docking;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraLayout.Utils;
using DBEngine;
using System.Text.RegularExpressions;
using DevExpress.Data.Helpers;

namespace UltraANetT.Module
{
    public partial class Task : XtraUserControl, IDraw, ITree
    {
        private readonly ProcShow _show = new ProcShow();
        private readonly ProcStore _store = new ProcStore();
        private LogicalControl _LogC = new LogicalControl();
        private ProcLog Log = new ProcLog();
        private readonly IDraw _draw;
        public INode NodeStr;
        private readonly ITree _tree;
        private byte[] _cacheTemplate;
        private string Role;
        private DataRow _dr;
        private DataRow _drEml;
        private DataTable _dt;
        private int _countItem = 0;
        private CheckState state;
        private bool isNorFirst = false;
        private List<object> listItem = new List<object>();
        private List<object> listSeg = new List<object>();
        private bool sameModule = false;
        /// <summary>
        /// 字典类型的任务信息
        /// </summary>
        private Dictionary<string, object> _dictTask = new Dictionary<string, object>();

        /// <summary>
        /// 当前操作类别
        /// </summary>
        private DataOper _currentOperate;

        /// <summary>
        /// 当前操作节点级别
        /// </summary>
        private string NodeAddStr = "";

        TreeListNode node = null;
        private CheckedListBoxItem clbCache;
        private string Operate;
        private GridHitInfo downHitInfo; //鼠标左键按下去时在GridView中的坐标
        private bool isVirFist;
        private bool isModuleWithSupplierDown = false;

        #region DTC用变量
        private Dictionary<string, string> _dictDTC;
        private string _selectedEmlName = string.Empty;
        private readonly SearchDTCByExaModule _searchDtc = new SearchDTCByExaModule();
        #endregion
        //private GridHitInfo upHitInfo; //鼠标左键弹起来时在GridView中的坐标
        public Task()
        {
            _draw = this;
            _tree = this;
            InitializeComponent();
            _draw.InitGrid();
            _draw.InitDict();
            _tree.LoadTreeList(GlobalVar.TreeXmlPath);
            _tree.DrawTreeColor();
            _tree.BingEvent();
            InitControl();
            Role = _LogC.RoleSelect(GlobalVar.UserName);
            RoleFunction(Role);
            ExpandTree();
            LoadTxtFounderItem();
            var departmentList = _store.GetRegularByEnum(EnumLibrary.EnumTable.Task);
            ConvertDataBaseModule(departmentList);
            hideContainerRight.Visible = false;
            //InitDataGrid();
            ShieldRight();

        }

        private void ConvertDataBaseModule(IList<object[]> dtList)
        {
            DataTable dtTask = new DataTable();

            var listCol = new List<string>();
            foreach (GridColumn col in gvTask.Columns)
                listCol.Add(col.FieldName);
           foreach (var colName in listCol.ToArray())
                dtTask.Columns.Add(new DataColumn(colName, typeof(object)));
            //从数据库中查询指定表的数据信息
            IList < object[] > newTaskList = new List<object[]>();
            if (dtList.Count > 0)
            {
                object[] rowl = ConvertTaskList(dtList[dtList.Count - 1]);
                newTaskList.Add(rowl);
                int count = 0;
                foreach (var dept in dtList)
                {
                    if (count < dtList.Count - 1)
                    {
                        object[] row = ConvertTaskList(dept);
                        newTaskList.Add(row);
                        
                    }
                    count++;
                }
                foreach (var dept in newTaskList)
                {
                    dtTask.Rows.Add(dept);
                }
            }
            foreach (DataRow dr in dtTask.Rows)
            {
                string strModule = ConvertModuleJsonToString(dr["TaskName"].ToString(), dr["Module"].ToString());
                dr["Module"] = strModule.Remove(strModule.Length - 1);
            }
            gcTask.DataSource = dtTask;
        }

        private object[] ConvertTaskList(object[] list)
        {
            object[] row = new object[16];
            row[0] = list[0];
            row[1] = list[1];
            row[2] = list[2];
            row[3] = list[3];
            row[4] = list[4];
            if (string.IsNullOrWhiteSpace(list[11].ToString()))
            {
                row[5] = "未配置用例";
            }
            else
            {
                row[5] = "已配置用例";
            }
            row[6] = list[5];
            row[7] = list[6];
            row[8] = list[7];
            row[9] = list[8];
            row[10] = list[9];
            row[11] = list[10];
            row[12] = list[11];
            row[13] = list[12];
            row[14] = list[13];
            row[15] = list[14];
            return row;
        }
        private void LoadTxtFounderItem()
        {
            IList<string> emNoList = _store.GetSingnalCol(EnumLibrary.EnumTable.Employee, 0);
            IList<string> emNameList = _store.GetSingnalCol(EnumLibrary.EnumTable.Employee, 1);
            IList<string> emRoleList = _store.GetSingnalCol(EnumLibrary.EnumTable.Employee, 2);
            txtFounder.Properties.Items.Clear();
            for (int i = 0; i < emNameList.Count; i++)
            {
                if (emRoleList[i] == "管理员" || emRoleList[i] == "配置员")
                    txtFounder.Properties.Items.Add(emNameList[i] + "(" + emNoList[i] + ")");
            }
        }

        private void ExpandTree()
        {
            if (GlobalVar.CurrentVNode.Count != 0)
            {
                taskTree.ExpandToLevel(1);
                for (int i = 0; i < taskTree.Nodes.Count; i++)
                {
                    string name = taskTree.Nodes[i].GetValue("colName").ToString();
                    if (name == GlobalVar.CurrentVNode[0])
                    {
                        for (int j = 0; j < taskTree.Nodes[i].Nodes.Count; j++)
                        {
                            string secondName = taskTree.Nodes[i].Nodes[j].GetValue("colName").ToString();
                            if (secondName != GlobalVar.CurrentVNode[1])
                            {
                                taskTree.Nodes[i].Nodes[j].Expanded = false;
                            }
                        }
                    }
                    else
                        taskTree.Nodes[i].Expanded = false;
                }
            }
        }

        private void RoleFunction(string role)
        {
            switch (role)
            {
                case "administer":
                case "superadminister":
                    CMSTable.Enabled = true;
                    CMSNode.Enabled = true;
                    CMSRound.Enabled = true;
                    CMSStage.Enabled = true;
                    CMSTask.Enabled = true;
                    cmsExpEdit.Enabled = true;
                    break;
                case "configurator":
                    CMSNode.Enabled = true;
                    CMSRound.Enabled = true;
                    CMSStage.Enabled = true;
                    CMSTask.Enabled = true;
                    cmsExpEdit.Enabled = true;
                    break;
                case "tester":
                    CMSTable.Enabled = false;
                    CMSNode.Enabled = false;
                    CMSRound.Enabled = false;
                    CMSStage.Enabled = false;
                    CMSTask.Enabled = false;
                    cmsExpEdit.Enabled = false;
                    break;
                default:
                    break;
            }
        }

        private enum DataOper
        {
            Add = 0,
            Update = 1,
            Del = 2
        }

        private void gvTaskRowClick(Dictionary<string, string> dictTask)
        {
            if (dictTask["TaskName"] != "通信DTC")
            {
                lcgTaskEml.Visibility = LayoutVisibility.Never;
                return;
            }
            else
            {
                lcgTaskEml.Visibility = LayoutVisibility.Always;
            }

            _dictTask["TaskNo"] = dictTask["TaskNo"];
            _dictTask["TaskRound"] = dictTask["TaskRound"];
            _dictTask["TaskName"] = dictTask["TaskName"];
            _dictTask["CANRoad"] = dictTask["CANRoad"];
            _dictTask["Module"] = GetModuleToJson(dictTask["TaskName"], dictTask["Module"]);
            _dictTask["TestType"] = dictTask["TestType"];
            string strModule =
                ConvertModuleJsonToString(_dictTask["TaskName"].ToString(), _dictTask["Module"].ToString());

            var i = dictTask["TaskNo"].ToString().Split('-');
            List<string> TaskNo = new List<string>()
            {
                i[0],
                i[1],
                i[2],
            };
            GlobalVar.CurrentVNode = TaskNo;
            NodeStr.SetVNode(GlobalVar.CurrentVNode);
            BindGvDTC(dictTask);
            lcgTaskEml.Text = "相关测试用例：" + _dictTask["TaskNo"] + "-" + _dictTask["TaskRound"] + "-" +
                              _dictTask["TaskName"] + "-" + _dictTask["CANRoad"] + "-" +
                              strModule.Remove(strModule.Length - 1);
            lcgTaskEml.AppearanceGroup.ForeColor = Color.Blue;
            _dr["IsConfigExap"] = "已绑定DTC信息";
            //if(Role == "tester")
            //    btnEdit.Enabled = false;
            //else
            //    btnEdit.Enabled = true;
        }

        private void CreateGridView(Dictionary<string, List<object>> listDrScheme)
        {
            gvTaskEml.Columns.Clear();
            foreach (KeyValuePair<string, List<object>> scheme in listDrScheme)
            {
                if (scheme.Key != "AssessItemRelevant")
                    if (bool.Parse(scheme.Value[2].ToString()))
                    {
                        GridColumn col = new GridColumn();
                        col.Caption = scheme.Value[1].ToString();
                        col.Name = scheme.Key;
                        col.FieldName = scheme.Key;
                        col.Visible = true;
                        gvTaskEml.Columns.AddRange(new GridColumn[] {col});
                    }
                if (scheme.Key == "AssessItemRelevant")
                {
                    GridColumn col = new GridColumn();
                    col.Caption = scheme.Value[1].ToString();
                    col.Name = scheme.Key;
                    col.FieldName = scheme.Key;
                    col.Visible = false;
                    gvTaskEml.Columns.AddRange(new GridColumn[] { col });
                }

            }
            GridColumn colCheck = new GridColumn();
            colCheck.Caption = "评价项目相关信息";
            colCheck.Name = "CheckItem";
            colCheck.FieldName = "CheckItem";
            colCheck.Visible = true;
            gvTaskEml.Columns.AddRange(new GridColumn[] { colCheck });
        }

        private void taskTree_MouseDown(object sender, MouseEventArgs e)
        {
            var node = taskTree.CalcHitInfo(new Point(e.X, e.Y)).Node;
            //if (node == null) return;
            var level = node?.Level;
            SortTaskTable(node, level);
            switch (e.Button)
            {
                case MouseButtons.Left:
                    downHitInfo = gvTask.CalcHitInfo(new Point(e.X, e.Y));
                    if (node != null && node.Level == 5)
                    {
                        var nodes = _tree.GetCurrentNode(node);
                        GlobalVar.CurrentTsNode = nodes;
                        NodeStr.SetVNode(GlobalVar.CurrentTsNode);
                    }

                    if (node != null && node.Level == 2)
                    {
                        GlobalVar.CurrentVNode = GetCurrent3Node(node);
                        NodeStr.SetVNode(GlobalVar.CurrentVNode);
                    }
                    if (node != null && node.Level == 3)
                    {
                        GlobalVar.CurrentVNode = GetCurrent4Node(node);
                        List<string> list = new List<string>
                        {
                        GlobalVar.CurrentVNode [0],
                        GlobalVar.CurrentVNode [1],
                        GlobalVar.CurrentVNode [2],
                        };
                        NodeStr.SetVNode(list);
                    }
                    if (node != null && node.Level == 4)
                    {
                        GlobalVar.CurrentVNode = GetCurrent5Node(node);
                        List<string> list = new List<string>
                        {
                        GlobalVar.CurrentVNode [0],
                        GlobalVar.CurrentVNode [1],
                        GlobalVar.CurrentVNode [2],
                        };
                        NodeStr.SetVNode(list);
                    }
                    break;
                case MouseButtons.Right:
                    if (node != null && node.Level == 2)
                    {
                        var nodes = GetCurrent3Node(node);
                        Dictionary<string, object> dictFileLink = new Dictionary<string, object>();
                        dictFileLink.Add("VehicelType", nodes[0]);
                        dictFileLink.Add("VehicelConfig", nodes[1]);
                        dictFileLink.Add("VehicelStage", nodes[2]);
                        IList<object[]> ListAuth = _store.GetSpecialByEnum(EnumLibrary.EnumTable.Authorization_Auth,
                            dictFileLink);
                        if (ListAuth.Count == 0)
                        {
                            XtraMessageBox.Show("请先到车型管理中对该车型授权...");
                            return;
                        }
                        IList<object[]> FileLink = _store.GetSpecialByEnum(EnumLibrary.EnumTable.FileLinkByVehicel,
                            dictFileLink);
                        if (FileLink.Count != 0)
                        {
                            //IEnumerable<object[]> fileList = FileLink.Where(t => t[10].ToString() == "");
                            //if (FileLink.Count == fileList.Count())
                            //{
                            //    XtraMessageBox.Show("车辆配置工作未完成，请先返回上一步完成用例表上传工作..");
                            //    return;
                            //}
                        }
                        else
                        {
                            XtraMessageBox.Show("车辆配置工作未完成，请先返回上一步完成配置表上传工作..");
                            return;
                        }
                    }
                    _tree.ShowCmsByNode(node);
                    break;
            }
        }

        private void SortTaskTable(TreeListNode node, int? level)
        {
            Dictionary<string, object> dictStage = new Dictionary<string, object>();
            IList<object[]> vehicel = new List<object[]>();
            if (level == 0)
            {
                string nodes = node.GetDisplayText("colName");
                dictStage["VehicelType"] = nodes;

                dictStage["level"] = "level1";
                vehicel = _store.GetSpecialByEnum(EnumLibrary.EnumTable.TaskTable, dictStage);
                //foreach (var dept in vehicel)
                //    dt.Rows.Add(dept);
                ////gcVechiel.DataSource = null;
                //gcTask.DataSource = dt;
            }
            else if (level == 1)
            {
                List<string> nodes = GetCurrent2Node(node);
                dictStage["VehicelType"] = nodes[0] + "-" + nodes[1];

                dictStage["level"] = "level2";
                vehicel = _store.GetSpecialByEnum(EnumLibrary.EnumTable.TaskTable, dictStage);
                //foreach (var dept in vehicel)
                //    dt.Rows.Add(dept);
                ////gcVechiel.DataSource = null;
                //gcTask.DataSource = dt;
            }
            else if (level == 2)
            {
                List<string> nodes = GetCurrent3Node(node);
                dictStage["TaskNo"] = nodes[0] + "-" + nodes[1] + "-" + nodes[2];

                dictStage["level"] = "level3";
                vehicel = _store.GetSpecialByEnum(EnumLibrary.EnumTable.TaskTable, dictStage);
                //foreach (var dept in vehicel)
                //    dt.Rows.Add(dept);
                ////gcVechiel.DataSource = null;
                //gcTask.DataSource = dt;
            }
            else if (level == 3)
            {
                List<string> nodes = GetCurrent4Node(node);
                dictStage["TaskNo"] = nodes[0] + "-" + nodes[1] + "-" + nodes[2];
                dictStage["TaskRound"] = nodes[3];
                dictStage["level"] = "level4";
                vehicel = _store.GetSpecialByEnum(EnumLibrary.EnumTable.TaskTable, dictStage);

            }
            else if (level == 4)
            {
                List<string> nodes = GetCurrent5Node(node);
                dictStage["TaskNo"] = nodes[0] + "-" + nodes[1] + "-" + nodes[2];
                dictStage["TaskRound"] = nodes[3];
                dictStage["TaskName"] = nodes[4];
                dictStage["level"] = "level5";
                vehicel = _store.GetSpecialByEnum(EnumLibrary.EnumTable.TaskTable, dictStage);

            }
            if (level == 5)
            {
                GlobalVar.CurrentTsNode = _tree.GetCurrentNode(node);

                List<string> nodes = _tree.GetCurrentNode(node);
                dictStage["TaskNo"] = nodes[0] + "-" + nodes[1] + "-" + nodes[2];
                dictStage["TaskRound"] = nodes[3];
                dictStage["TaskName"] = nodes[4];
                dictStage["Module"] = GetModuleToJson(nodes[4],nodes[5]);
                dictStage["level"] = "level6";
                vehicel = _store.GetSpecialByEnum(EnumLibrary.EnumTable.TaskTable, dictStage);
                //foreach (var dept in vehicel)
                //    dt.Rows.Add(dept);
                ////gcVechiel.DataSource = null;
                //gcTask.DataSource = dt;

            }
            if (vehicel.Count != 0)
            {
                ConvertDataBaseModule(vehicel);
                
            }
        }


        void IDraw.InitGrid()
        {
            var listCol = new List<string>();
            foreach (GridColumn col in gvTask.Columns)
                listCol.Add(col.FieldName);
            _dt = _show.DrawDtFromTask(listCol.ToArray());
            gcTask.DataSource = _dt;
            //_dt = _show.DrawDtFromTask(listCol.ToArray());
        }

        private void InitControl()
        {
            cbName.Properties.TextEditStyle = TextEditStyles.DisableTextEditor;
            cbSegment.Properties.TextEditStyle = TextEditStyles.DisableTextEditor;
            cbTestType.Properties.TextEditStyle = TextEditStyles.DisableTextEditor;
            cbeModule.Properties.TextEditStyle = TextEditStyles.DisableTextEditor;
            cbeSupplier.Properties.TextEditStyle = TextEditStyles.DisableTextEditor;
            Dictionary<string, string> vehicel = new Dictionary<string, string>();
            cbSegment.Properties.Items.Clear();
            if (GlobalVar.CurrentVNode.Count >= 3 )
            {
                vehicel.Add("VehicelType", GlobalVar.CurrentVNode[0]);
                vehicel.Add("VehicelConfig", GlobalVar.CurrentVNode[1]);
                vehicel.Add("VehicelStage", GlobalVar.CurrentVNode[2]);
                IList<object[]> exmp = BaseSqlOrder.GetMultipleByQuery(ExcuteSqlCase.FileLinkByVehicel_GetMatch(vehicel));
                cbName.Properties.Items.Clear();
                string currentCAN = "";
                listItem.Clear();
                foreach (var sort in exmp)
                {
                    if (currentCAN != sort[3].ToString())
                    {
                        cbName.Properties.Items.Add(sort[3]);
                        listItem.Add(sort[3]);
                        currentCAN = sort[3].ToString();
                    }
                }
            }
            cbTestType.Properties.Items.Clear();
            lblTaskNo.Text = "";
            IList<string> deList = _store.GetSingnalCol(EnumLibrary.EnumTable.Department, 0);
            cbTesterDept.Properties.Items.Clear();
            foreach (var de in deList)
                cbTesterDept.Properties.Items.Add(de);
            IList<string> emNameList = _store.GetSingnalCol(EnumLibrary.EnumTable.Employee, 1);
            //cbTesterDept.Properties.Items.Clear();
            cbAuthTester.Properties.Items.Clear();
            foreach (var em in emNameList)
                cbAuthTester.Properties.Items.Add(em);
            txtModule.ReadOnly = true;
        }

        private Dictionary<string, object> UserDict()
        {
            _dictTask["EmployeeName"] = GlobalVar.UserName;
            _dictTask["EmployeeNo"] = GlobalVar.UserNo;
            _dictTask["Department"] = GlobalVar.UserDept;
            return _dictTask;
        }

        private Dictionary<string, object> UserDr(bool su)
        {
            UserDict();
            _dictTask["TaskNo"] = _dr["TaskNo"];
            _dictTask["TaskRound"] = _dr["TaskRound"];
            _dictTask["TaskName"] = _dr["TaskName"];
            _dictTask["TestType"] = _dr["TaskName"];
            _dictTask["CANRoad"] = _dr["CANRoad"];
            if (!su)
                _dictTask["Module"] = GetModuleToJson(_dr["TaskName"].ToString(), _dr["Module"].ToString());
            else
                _dictTask["Module"] = _dr["Module"];
            _dictTask["CreateTime"] = DateTime.Parse(_dr["CreateTime"].ToString()).ToString("yyyy-MM-dd");
            _dictTask["Creater"] = _dr["Creater"];
            _dictTask["AuthTester"] = _dr["AuthTester"];
            _dictTask["AuthorizedFromDept"] = _dr["AuthorizedFromDept"];
            _dictTask["Supplier"] = _dr["Supplier"];
            _dictTask["AuthorizationTime"] = DateTime.Parse(_dr["AuthorizationTime"].ToString()).ToString("yyyy-MM-dd");
            _dictTask["InvalidTime"] = DateTime.Parse(_dr["InvalidTime"].ToString()).ToString("yyyy-MM-dd");
            _dictTask["Remark"] = _dr["Remark"];
            return _dictTask;
        }

        void IDraw.Submit()
        {
            var error = "";
            switch (_currentOperate)
            {

                case DataOper.Add:
                    if (ModuleCountByOK(cbTestType.Text, txtModule.Text))
                        return;
                    if (IsEmpty())
                        return;
                    _draw.GetDataFromUI();
                    _store.AddTask(_dictTask, out error);
                    string module = ConvertModuleJsonToString(_dictTask["TaskName"].ToString(),
                        _dictTask["Module"].ToString());
                    //if(module.Length)
                    module = module.Remove(module.Length - 1);
                    //if (NodeAddStr != "")

                    if (error == "")
                    {
                        if (NodeAddStr == "CreateTask")
                        {

                            AddTaskNode(node, _dictTask["TaskName"].ToString(), module);
                        }
                        if (NodeAddStr == "CreateNode")
                        {
                            AddNode(node, module);
                        }
                        Dictionary<string, object> dictConfig = new Dictionary<string, object>
                        {
                            {"EmployeeNo", GlobalVar.UserNo},
                            {"EmployeeName", GlobalVar.UserName},
                            {"OperTable", "任务"},
                            {"AuthTester",_dictTask["AuthTester"]},
                            {"TaskNo", _dictTask["TaskNo"]},
                            {"TaskRound", _dictTask["TaskRound"]},
                            {"TaskName", _dictTask["TaskName"]},
                            {"CANRoad", _dictTask["CANRoad"]},
                            {"Module", _dictTask["Module"]},
                            {"TestType", _dictTask["TestType"]},
                        };
                        Log.WriteLog(EnumLibrary.EnumLog.AddTask, dictConfig);

                    }
                    else
                    {
                        //if (_dictTask["TaskName"].ToString() == "CAN单节点")
                        {
                            XtraMessageBox.Show("表中已有一条相同的任务，不能添加相同的，如需在测试这个，可以新建一个轮次再添加");
                        }
                        //else
                        //{
                        //    XtraMessageBox.Show("表中已有一条相同的任务，不能添加相同的，如需在测试这个，可以新建一个轮次再添加");
                        //}
                        
                    }
                    //NodeAddStr = "";
                    break;

                case DataOper.Update:
                    if (ModuleCountByOK(cbTestType.Text, txtModule.Text))
                        return;
                    if (IsEmpty())
                        return;
                    _draw.GetDataFromUI();
                    _store.Update(EnumLibrary.EnumTable.TaskUpdate, _dictTask, out error);


                    if (error == "")
                    {
                        cbSegment.ReadOnly = false;
                        cbeModule.ReadOnly = false;
                        txtFounder.ReadOnly = false;
                    }
                    if (error == "")
                    {
                        //Log.WriteLog(EnumLibrary.EnumLog.AddTasker, UserDict());
                    }
                    else
                    {
                        XtraMessageBox.Show("表中已有相同的任务，请修改成与表中不同的任务");
                    }
                    break;
                case DataOper.Del:
                    bool su = _store.Del(EnumLibrary.EnumTable.Task, UserDr(false), out error);
                    if (error == "")
                    {
                        Dictionary<string, object> dictConfig = new Dictionary<string, object>
                        {
                            {"EmployeeNo", GlobalVar.UserNo},
                            {"EmployeeName", GlobalVar.UserName},
                            {"OperTable", "任务"},
                             {"AuthTester",_dictTask["AuthTester"]},
                            {"TaskNo", _dictTask["TaskNo"]},
                            {"TaskRound", _dictTask["TaskRound"]},
                            {"TaskName", _dictTask["TaskName"]},
                            {"CANRoad", _dictTask["CANRoad"]},
                            {"Module", _dictTask["Module"]},
                            {"TestType", _dictTask["TestType"]},
                        };
                        Log.WriteLog(EnumLibrary.EnumLog.DelTask, dictConfig);
                    }
                    break;
            }
            if (error == "")
            {
                //Log.WriteLog(EnumLibrary.EnumTable.Authorization, UserDict(_currentOperate));
                VehicelNodes();
                _tree.SaveTreeList(GlobalVar.TreeXmlPath);
                _draw.SwitchCtl(false);
                _draw.InitGrid();
                var departmentList = _store.GetRegularByEnum(EnumLibrary.EnumTable.Task);
                ConvertDataBaseModule(departmentList);

            }

        }

        private bool IsEmpty()
        {
            bool empty = false;
            if (String.IsNullOrWhiteSpace(cbName.Text.Trim()))
            {
                XtraMessageBox.Show("总线类型不能为空");
                empty = true;
                return empty;
            }
            if (String.IsNullOrWhiteSpace(cbSegment.Text.Trim()) && cbName.Text.Trim() != "网关路由")
            {
                XtraMessageBox.Show("网段选择不能为空");
                empty = true;
                return empty;
            }
            if (String.IsNullOrWhiteSpace(cbTestType.Text.Trim()))
            {
                XtraMessageBox.Show("测试类型不能为空");
                empty = true;
                return empty;
            }
            if (string.IsNullOrWhiteSpace(txtModule.Text.Trim()))
            {
                XtraMessageBox.Show("模块不能为空");
                empty = true;
                return empty;
            }
            if (this.dateFound.DateTime > this.dateStart.DateTime)
            {
                XtraMessageBox.Show("创建时间不能在测试有效期之后");
                empty = true;
                return empty;
            }
            else if (this.dateStart.DateTime > this.dateEnd.DateTime)
            {
                XtraMessageBox.Show("测试有效期不能在测试失效期之后");
                empty = true;
                return empty;
            }
            return empty;
        }

        /// <summary>
        /// 将Json转化成正常显示格式
        /// </summary>
        /// <param name="node"></param>
        /// <param name="listNode"></param>
        /// <returns></returns>
        private string ConvertModuleJsonToString(string node,string listNode)
        {
            string module = "";
            if (node != "")
            {
                switch (node)
                {
                    case "CAN通信单元":
                    case "LIN通信主节点":
                    case "LIN通信从节点":
                    case "直接NM单元":
                    case "动力域NM主节点":
                    case "动力域NM从节点":
                    case "间接NM单元":
                    case "通信DTC":
                    case "OSEK NM单元":
                    case "Bootloader":
                    case "网关路由":
                        module = listNode + "/"; 
                        break;
                    case "CAN通信集成":
                    case "LIN通信集成":
                    case "直接NM集成":
                    case "间接NM集成":
                    case "动力域NM集成":
                    case "OSEK NM集成":
                        module = GetMultiNodeString(listNode);
                        break;
                    default:
                        module = @"/";
                        break;
                }
            }
            return module;
        }

        private string GetMultiNodeString(string list)
        {
            Dictionary<string, string> name = Json.DerJsonToDict(list);
            string taskModule = "";
            foreach (KeyValuePair<string, string> item in name)
            {
                if (item.Key == "Virtual")
                {
                    if (taskModule != "")
                    {
                        //taskModule = taskModule.Split(' ')[0];
                        taskModule = taskModule + item.Value + "(" + "虚拟" + "）" + "/";
                    }
                    else
                    {
                        taskModule = item.Value + "(" + "虚拟" + "）" + "/";
                    }
                }
                else if (item.Key == "Normal")
                {
                    taskModule = taskModule + item.Value + "/";
                }
            }
            return taskModule;
        }

        /// <summary>
        /// 右侧gridview与左侧Tree联动
        /// </summary>
        /// <param name="oper"></param>
        private void VehicelNodes()
        {
            var taskNodes = taskTree.Nodes;
            string[] task = _dictTask["TaskNo"].ToString().Split('-');
            foreach (TreeListNode Type in taskNodes)
            {
                if (Type.GetDisplayText("colName") == task[0])
                {
                    foreach (TreeListNode Config in Type.Nodes)
                    {
                        if (Config.GetDisplayText("colName") == task[1])
                        {
                            foreach (TreeListNode Stage in Config.Nodes)
                            {
                                if (Stage.GetDisplayText("colName") == task[2])
                                {
                                    foreach (TreeListNode Round in Stage.Nodes)
                                    {
                                        if (Round.GetDisplayText("colName") == _dictTask["TaskRound"].ToString())
                                        {
                                            foreach (TreeListNode TaskName in Round.Nodes)
                                            {
                                                if (TaskName.GetDisplayText("colName") ==
                                                    _dictTask["TaskName"].ToString())
                                                {
                                                    foreach (TreeListNode Module in TaskName.Nodes)
                                                    {
                                                        if (Module.GetDisplayText("colName") ==
                                                            _dictTask["Module"].ToString())
                                                        {
                                                            switch (_currentOperate)
                                                            {
                                                                case DataOper.Del:
                                                                    taskTree.DeleteNode(Module);
                                                                    _tree.SaveTreeList(GlobalVar.TreeXmlPath);
                                                                    break;
                                                            }
                                                            return;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        Dictionary<string, object> IDraw.GetDataFromUI()
        {
            _dictTask["TaskNo"] = lblTaskNo.Text;
            _dictTask["TaskRound"] = txtRound.Text;
            _dictTask["TaskName"] = cbTestType.Text;
            _dictTask["CANRoad"] = cbSegment.Text;

            _dictTask["Module"] = GetModuleString(cbTestType.Text);
            _dictTask["TestType"] = cbName.Text;
            _dictTask["CreateTime"] = dateFound.DateTime.ToString("yyyy-MM-dd");
            _dictTask["Creater"] = txtFounder.Text;
            _dictTask["AuthTester"] = cbAuthTester.Text;
            _dictTask["AuthorizedFromDept"] = cbTesterDept.Text;
            _dictTask["Supplier"] = GetSupplierToString();

            _dictTask["ContainExmp"] = "";
            _dictTask["AuthorizationTime"] = dateStart.DateTime.ToString("yyyy-MM-dd");
            _dictTask["InvalidTime"] = dateEnd.DateTime.ToString("yyyy-MM-dd");
            _dictTask["Remark"] = "";

            string[] task = lblTaskNo.Text.Split('-');
            Dictionary<string, string> dictFileLink = new Dictionary<string, string>();
            dictFileLink.Add("VehicelType", task[0]);
            dictFileLink.Add("VehicelConfig", task[1]);
            dictFileLink.Add("VehicelStage", task[2]);
            //string configType = cbName.SelectedItem.ToString().Split('');
            dictFileLink.Add("MatchSort", cbName.SelectedItem.ToString());

            IList<object> exmp = _store.GetFileByName(dictFileLink);
            var cacheTemplate = exmp[10].ToString();
            _dictTask["ByteExmp"] = cacheTemplate;

            return _dictTask;
        }

        private string GetSupplierToString()
        {
            string str = string.Empty;
            for (int i = 0; i < lbcSupplier.Items.Count; i++)
            {
                if (lbcSupplier.Items.Count - 1 == i)
                {
                    str += lbcSupplier.Items[i].ToString();
                }
                else
                {
                    str += lbcSupplier.Items[i].ToString() + "\r\n";
                }
            }
            return str;
        }

        private string[] SetStringToSupplier(string str)
        {
            string[] striparr = str.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            striparr = striparr.Where(s => !string.IsNullOrEmpty(s)).ToArray();
            return striparr;
        }
        void IDraw.SetDataToUI(DataRow selectedRow)
        {
            InitControl();
            _dictTask["TaskNo"] = selectedRow["TaskNo"];
            _dictTask["TaskRound"] = selectedRow["TaskRound"];
            _dictTask["OldTaskName"] = selectedRow["TaskName"];
            _dictTask["OldCANRoad"] = selectedRow["CANRoad"];
            _dictTask["OldModule"] = selectedRow["Module"];

            lblTaskNo.Text = selectedRow["TaskNo"].ToString();
            txtRound.Text = selectedRow["TaskRound"].ToString();
            cbName.EditValue = selectedRow["TestType"].ToString();
            cbSegment.Text = selectedRow["CANRoad"].ToString();
            cbTestType.Text = selectedRow["TaskName"].ToString();
            txtModule.Text = selectedRow["Module"].ToString();
            InitCbTestType(cbName.Text);

            //SelectModuleType(selectedRow["TaskName"].ToString(), selectedRow["Module"].ToString())

            //SetModuleCheckRadio(selectedRow["Module"].ToString());

            dateFound.DateTime = DateTime.Parse(selectedRow["CreateTime"].ToString());
            txtFounder.Text = selectedRow["Creater"].ToString();
            cbAuthTester.Text = selectedRow["AuthTester"].ToString();
            cbTesterDept.Text = selectedRow["AuthorizedFromDept"].ToString();
            lbcSupplier.Items.Clear();
            lbcSupplier.Items.AddRange(SetStringToSupplier(selectedRow["Supplier"].ToString()));
            //txtSuplier.Text = selectedRow["Supplier"].ToString();
            dateStart.DateTime = DateTime.Parse(selectedRow["AuthorizationTime"].ToString());
            dateEnd.DateTime = DateTime.Parse(selectedRow["InvalidTime"].ToString());
            if (_currentOperate == DataOper.Update)
            {
                cbSegment.ReadOnly = true;
                cbeModule.ReadOnly = true;
                txtFounder.ReadOnly = true;
            }
        }

        #region 模块赋值

        /// <summary>
        /// 多选框模块赋值
        /// </summary>
        /// <param name="Module"></param>
        private void SetModuleCheckList(string Module)
        {
            //foreach (CheckedListBoxItem item in radioGroupModule.Properties.Items)
            //{
            //    if (item.Description == Module)
            //    {
            //        item.CheckState = CheckState.Checked;
            //        break;
            //    }
            //}
        }

        /// <summary>
        /// 单选框模块赋值
        /// </summary>
        /// <param name="Module"></param>
        private void SetModuleCheckRadio(string Module)
        {
            int i = 0;
            bool isSucc = false;
            //SetItem(name);
            //foreach (RadioGroupItem item in radioGroupModule.Properties.Items)
            //{
            //    if (item.Description == Module)
            //    {
            //        radioGroupModule.SelectedIndex = i;
            //        isSucc = true;
            //        break;
            //    }
            //    i++;
            //}
            //if(!isSucc)
            //    radioGroupModule.SelectedIndex = -1;
        }

        #endregion

        void ClearDataToUI()
        {
            cbSegment.Text = null;
            cbName.Text = null;
            cbeModule.Properties.Items.Clear();
            cbeModule.SelectedIndex = -1;
            cbeModule.Text = "";
            cbeSupplier.Properties.Items.Clear();
            cbeSupplier.SelectedIndex = -1;
            cbeSupplier.Text = "";
            cbTestType.SelectedIndex = -1;
            //cbTestType.Enabled = false;
            dateFound.Text = null;
            txtFounder.Text = null;
            cbAuthTester.Text = null;
            cbTesterDept.Text = null;
            lbcSupplier.Items.Clear();
            dateStart.Text = null;
            dateEnd.Text = null;
            cbName.ReadOnly = false;
            cbTestType.ReadOnly = false;
            
            cbSegment.ReadOnly = false;
            txtModule.ReadOnly = false;
            cbeModule.ReadOnly = false;

            cbAuthTester.CheckAll();
            
        }

        void IDraw.InitDict()
        {
            _dictTask.Add("TaskNo", "");
            _dictTask.Add("TaskRound", "");
            _dictTask.Add("TaskName", "");
            _dictTask.Add("CANRoad", "");
            _dictTask.Add("Module", "");
            _dictTask.Add("TestType", "");
            _dictTask.Add("CreateTime", "");
            _dictTask.Add("Creater", "");
            _dictTask.Add("AuthTester", "");
            _dictTask.Add("AuthorizedFromDept", "");
            _dictTask.Add("Supplier", "");
            _dictTask.Add("ContainExmp", "");
            _dictTask.Add("AuthorizationTime", "");
            _dictTask.Add("InvalidTime", "");
            _dictTask.Add("Remark", "");

            _dictTask.Add("ifTask", "");
        }

        bool ITree.SaveTreeList(string xmlPath)
        {
            if (!File.Exists(xmlPath)) return false;
            taskTree.ExportToXml(AppDomain.CurrentDomain.BaseDirectory + xmlPath);
            return true;
        }

        bool ITree.LoadTreeList(string taskXmlPath)
        {
            //获得任务树
            if (!File.Exists(taskXmlPath)) return false;
            taskTree.ImportFromXml(GlobalVar.TreeXmlPath);
            _tree.DrawTreeImage();
            return true;
        }

        void ITree.DrawTreeColor()
        {
            var controlColor = CommonSkins.GetSkin(UserLookAndFeel.Default).Colors.GetColor("Control");
            taskTree.Appearance.Empty.BackColor = controlColor;
            taskTree.Appearance.Row.BackColor = controlColor;
        }

        void ITree.ShowCmsByNode(TreeListNode node)
        {
            if (node == null)
            {
                taskTree.ContextMenuStrip = CMSViewTable;
                return;
            }
            taskTree.FocusedNode = node;
            var level = node.Level;
            switch (level)
            {
                case 2:
                    taskTree.ContextMenuStrip = CMSStage;
                    break;
                case 3:
                    taskTree.ContextMenuStrip = CMSRound;
                    break;
                case 4:
                    taskTree.ContextMenuStrip = CMSTask;
                    break;
                case 5:
                    taskTree.ContextMenuStrip = CMSNode;
                    break;
                default:
                    taskTree.ContextMenuStrip = CMSViewTable;
                    break;
            }
        }

        /// <summary>
        /// 将模块的选项转化成Json
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private string GetModuleString(string node)
        {
            string module = "";
            if (node != "")
            {

                //cbName.Properties.Items.AddRange(type);
                switch (node)
                {
                    case "CAN通信单元":
                    case "LIN通信主节点":
                    case "LIN通信从节点":
                    case "直接NM单元":
                    case "动力域NM主节点":
                    case "动力域NM从节点":
                    case "间接NM单元":
                    case "通信DTC":
                    case "OSEK NM单元":
                    case "Bootloader":
                    case "网关路由":
                        module = GetRadioCheckedNodeString();
                        break;
                    case "CAN通信集成":
                    case "LIN通信集成":
                    case "直接NM集成":
                    case "间接NM集成":
                    case "动力域NM集成":
                    case "OSEK NM集成":
                        module = GetCheckedNodeString();
                        break;
                    default:
                        break;
                }
            }
            return module;
        }

        #region 模块取值

        /// <summary>
        /// 模块取值（集成）
        /// </summary>
        /// <returns></returns>
        private string GetCheckedNodeString()
        {
            var nodeNorStr = "";
            var nodeVirStr = "";
            string module = "";
            Dictionary<string, string> dictModu = new Dictionary<string, string>();
            nodeNorStr = txtModule.Text;
            //foreach (CheckedListBoxItem item in clbNormal.Items)
            //{
            //    if (item.CheckState == CheckState.Checked)
            //        nodeNorStr += item.Description + "/";
            //}
            //foreach (CheckedListBoxItem item in clbVirtual.Items)
            //{
            //    if (item.CheckState == CheckState.Checked)
            //        nodeVirStr += item.Description + "(虚拟)" + "/";
            //}
            if (nodeNorStr != "")
            {
                dictModu["Normal"] = nodeNorStr;
            }
            if (nodeVirStr != "")
            {
                dictModu["Virtual"] = nodeVirStr;
            }
            if (dictModu.Count != 0)
            {
                module = Json.SerJson(dictModu);
            }

            return module;
        }

        /// <summary>
        /// 模块取值（非集成）
        /// </summary>
        /// <returns></returns>
        private string GetRadioCheckedNodeString()
        {
            //int index = radioGroupModule.SelectedIndex;
            //string nodeStr = radioGroupModule.Properties.Items[index].Description;
            //return nodeStr;
            //int index = radioGroupModule.SelectedIndex;
            string nodeStr = txtModule.Text;
            return nodeStr;
        }

        #endregion

        void ITree.StartEdit()
        {
            if (taskTree.FocusedNode == null) return;
            taskTree.OptionsBehavior.Editable = true;
            taskTree.VisibleColumns[0].OptionsColumn.AllowFocus = true;
            taskTree.FocusedColumn = taskTree.VisibleColumns[0];
            taskTree.ShowEditor();
        }

        void ITree.Create_Click(object sender, EventArgs e)
        {
            var tsmi = sender as ToolStripMenuItem;
            List<string> TaskTemp = new List<string>();
            if (tsmi == tsmiCreatRound)
            {
                node = AddNodeNameIndex(taskTree.FocusedNode.Nodes, "NewRound");
                node.StateImageIndex = 3;
                taskTree.FocusedNode = node;
                _tree.StartEdit();
            }
            else if (tsmi == tsmiCreateTask)
            {
                node = taskTree.FocusedNode;
                TaskTemp = GetCurrent4Node(node);
                string taskNo = TaskTemp[0] + "-" + TaskTemp[1] + "-" + TaskTemp[2];
                string round = TaskTemp[3];
                GlobalVar.CurrentVNode.Add(TaskTemp[0]);
                GlobalVar.CurrentVNode.Add(TaskTemp[1]);
                GlobalVar.CurrentVNode.Add(TaskTemp[2]);
                InitControl();
                ClearDataToUI();
                lblTaskNo.Text = taskNo;
                txtRound.Text = round;
                cbSegment.SelectedIndex = -1;
                cbSegment.Enabled = false;
                txtFounder.Text = GlobalVar.UserName;
                dateFound.DateTime = DateTime.Now;
                dateStart.DateTime = DateTime.Now;
                dateEnd.DateTime = DateTime.Now.AddYears(20);
                cbName.SelectedIndex = cbName.Properties.Items.Count > 0 ? 0 : -1;
                cbTestType.ReadOnly = false;
                cbTestType.Enabled = true;
                txtModule.Text = "";
                lbcSupplier.Items.Clear();
                _draw.SwitchCtl(true);
                _currentOperate = DataOper.Add;
                //Dictionary<string, object> dictFileLink = new Dictionary<string, object>();
                //dictFileLink.Add("VehicelType", TaskTemp[0]);
                //dictFileLink.Add("VehicelConfig", TaskTemp[1]);
                //dictFileLink.Add("VehicelStage", TaskTemp[2]);
                //IList<object[]> fileLink = _store.GetSpecialByEnum(EnumLibrary.EnumTable.FileLinkByVehicel, dictFileLink);
                //if (fileLink.Count != 0 && fileLink[0][3].ToString() != "")
                //{
                //    cbName.Text = fileLink[0][3].ToString();
                //    cbName.Enabled = false;
                //}
                NodeAddStr = "CreateTask";
            }
            else if (tsmi == tsmiCreateNode)
            {
                node = taskTree.FocusedNode;
                TaskTemp = GetCurrent5Node(node);
                string taskNo = TaskTemp[0] + "-" + TaskTemp[1] + "-" + TaskTemp[2];
                string round = TaskTemp[3];
                string Name = TaskTemp[4];
                GlobalVar.CurrentVNode[0]=TaskTemp[0];
                GlobalVar.CurrentVNode[1] = TaskTemp[1];
                GlobalVar.CurrentVNode[2] = TaskTemp[2];
                InitControl();
                ClearDataToUI();
                lblTaskNo.Text = taskNo;
                txtRound.Text = round;
                cbTestType.EditValue = Name;
                cbSegment.SelectedIndex = -1;
                cbTestType.ReadOnly = true;
                cbSegment.ReadOnly = true;
                cbSegment.Enabled = false;
                cbName.Enabled = true;
                SetBusType(cbTestType.Text);
                RemoveItemByTestType(cbTestType.Text);
                txtModule.Text = "";
                lbcSupplier.Items.Clear();

                txtFounder.Text = GlobalVar.UserName;
                dateFound.DateTime = DateTime.Now;
                dateStart.DateTime = DateTime.Now;
                dateEnd.DateTime = DateTime.Now.AddYears(20);
                cbName.SelectedIndex = cbName.Properties.Items.Count > 0 ? 0 : -1;

                _draw.SwitchCtl(true);
                _currentOperate = DataOper.Add;
                //Dictionary<string, object> dictFileLink = new Dictionary<string, object>();
                //dictFileLink.Add("VehicelType", TaskTemp[0]);
                //dictFileLink.Add("VehicelConfig", TaskTemp[1]);
                //dictFileLink.Add("VehicelStage", TaskTemp[2]);
                //IList<object[]> fileLink = _store.GetSpecialByEnum(EnumLibrary.EnumTable.FileLinkByVehicel, dictFileLink);
                //if (fileLink.Count != 0 && fileLink[0][3].ToString() != "")
                //{
                //    cbName.Text = fileLink[0][3].ToString();
                //    cbName.Enabled = false;
                //}
                NodeAddStr = "CreateNode";
            }

            taskTree.FocusedNode = node;
            //_draw.InitGrid();
            //var departmentList = _store.GetRegularByEnum(EnumLibrary.EnumTable.Task);
            //ConvertDataBaseModule(departmentList);
            _tree.SaveTreeList(GlobalVar.TreeXmlPath);
            cbTestType.Enabled = true;
        }


        /// <summary>
        /// 根据测试类型来刷新总线和网段
        /// </summary>
        /// <param name="testType"></param>
        private void SetBusType(string testType)
        {
            if (testType != "")
            {
                switch (testType)
                {
                    case "CAN通信单元":
                    case "CAN通信集成":
                    case "直接NM单元":
                    case "直接NM集成":
                    case "动力域NM主节点":
                    case "动力域NM从节点":
                    case "动力域NM集成":
                    case "间接NM单元":
                    case "间接NM集成":
                    case "通信DTC":
                    case "OSEK NM单元":
                    case "OSEK NM集成":
                    case "Bootloader":
                    //case "网关路由":
                        foreach (var item in listItem)
                        {
                            if (item.ToString().Substring(0, 3) == "LIN")
                                cbName.Properties.Items.Remove(item);
                        }
                        foreach (var item in listSeg)
                        {
                            if (item.ToString().Substring(0, 3) == "LIN")
                            {
                                cbSegment.Properties.Items.Remove(item);
                            }
                        }
                        break;
                    case "LIN通信集成":
                    case "LIN通信主节点":
                    case "LIN通信从节点":
                        foreach (var item in listItem)
                        {
                            if (item.ToString().Substring(0, 3) == "CAN")
                                cbName.Properties.Items.Remove(item);
                        }
                        foreach (var item in listSeg)
                        {
                            if (item.ToString().Substring(0, 3) == "CAN")
                            {
                                cbSegment.Properties.Items.Remove(item);
                            }
                        }
                        break;
                    default:
                        break;
                }

            }
            
        }

        private void RemoveItemByTestType(string testType)
        {
            if (!string.IsNullOrWhiteSpace(testType ))
            {
                switch (testType)
                {
                    case "CAN通信单元":
                    case "CAN通信集成":
                    case "直接NM单元":
                    case "直接NM集成":
                    case "动力域NM主节点":
                    case "动力域NM从节点":
                    case "动力域NM集成":
                    case "间接NM单元":
                    case "间接NM集成":
                    case "通信DTC":
                    case "OSEK NM单元":
                    case "OSEK NM集成":
                    case "Bootloader":
                        cbName.Properties.Items.Clear();
                        cbName.Properties.Items.Add("CAN总线");
                        break;
                    case "LIN通信主节点":
                    case "LIN通信从节点":
                    case "LIN通信集成":
                        cbName.Properties.Items.Clear();
                        cbName.Properties.Items.Add("LIN总线");
                        break;
                    case "网关路由":
                        cbName.Properties.Items.Clear();
                        cbSegment.Enabled = false;
                        cbName.Properties.Items.Add("网关路由");
                        break;    
                }
            }
        }
        private List<string> GetCurrent4Node(TreeListNode node)
        {
            List<string> nodes = new List<string>
            {
                node.ParentNode.ParentNode.ParentNode.GetDisplayText("colName"),
                node.ParentNode.ParentNode.GetDisplayText("colName"),
                node.ParentNode.GetDisplayText("colName"),
                node.GetDisplayText("colName")
            };
            return nodes;
        }

        private List<string> GetCurrent5Node(TreeListNode node)
        {
            List<string> nodes = new List<string>
            {
                node.ParentNode.ParentNode.ParentNode.ParentNode.GetDisplayText("colName"),
                node.ParentNode.ParentNode.ParentNode.GetDisplayText("colName"),
                node.ParentNode.ParentNode.GetDisplayText("colName"),
                node.ParentNode.GetDisplayText("colName"),
                node.GetDisplayText("colName")
            };
            return nodes;
        }

        void ITree.ReName_Click(object sender, EventArgs e)
        {
            _tree.StartEdit();
        }

        void ITree.Del_Click(object sender, EventArgs e)
        {
            var tsmi = sender as ToolStripMenuItem;
            TreeListNode node = null;
            List<string> TaskTemp = new List<string>();
            string error = "";
            _dictTask["CANRoad"] = "";
            if (taskTree.FocusedNode == null) return;
            if (tsmi == tsmiDelRound)
            {
                node = taskTree.FocusedNode;
                TaskTemp = GetCurrent4Node(node);
                _dictTask["ifTask"] = "Round";
                _dictTask["TaskNo"] = TaskTemp[0] + "-" + TaskTemp[1] + "-" + TaskTemp[2];
                _dictTask["TaskRound"] = TaskTemp[3];
            }
            else if (tsmi == tsmiDelTask)
            {
                node = taskTree.FocusedNode;
                TaskTemp = GetCurrent5Node(node);
                _dictTask["ifTask"] = "Task";
                _dictTask["TaskNo"] = TaskTemp[0] + "-" + TaskTemp[1] + "-" + TaskTemp[2];
                _dictTask["TaskRound"] = TaskTemp[3];
                _dictTask["TaskName"] = TaskTemp[4];
            }
            else if (tsmi == tsmiDelNode)
            {
                node = taskTree.FocusedNode;
                TaskTemp = _tree.GetCurrentNode(node);
                _dictTask["ifTask"] = "Node";
                _dictTask["TaskNo"] = TaskTemp[0] + "-" + TaskTemp[1] + "-" + TaskTemp[2];
                _dictTask["TaskRound"] = TaskTemp[3];
                _dictTask["TaskName"] = TaskTemp[4];
                _dictTask["Module"] = TaskTemp[5];
            }
            _store.Del(EnumLibrary.EnumTable.Task, _dictTask, out error);
            if (error == "")
            {
                taskTree.DeleteSelectedNodes();
                _tree.SaveTreeList(GlobalVar.TreeXmlPath);
                _draw.InitGrid();
                var departmentList = _store.GetRegularByEnum(EnumLibrary.EnumTable.Task);
                ConvertDataBaseModule(departmentList);
                //_tree.LoadTreeList(GlobalVar.TreeXmlPath);
            }
            else
            {
                XtraMessageBox.Show(error, "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            _dictTask["ifTask"] = "";
        }

        void ITree.BingEvent()
        {
            tsmiCreatRound.Click += _tree.Create_Click;
            tsmiCreateTask.Click += _tree.Create_Click;
            
            tsmiCreateNode.Click += _tree.Create_Click;

            tsmiReRound.Click += _tree.ReName_Click;

            tsmiDelRound.Click += _tree.Del_Click;
            tsmiDelTask.Click += _tree.Del_Click;
            tsmiDelNode.Click += _tree.Del_Click;
        }

        void IDraw.SwitchCtl(bool isSwitch)
        {
            switch (isSwitch)
            {
                case true:
                    btnSubmit.Enabled = true;
                    dpTask.Visibility = DockVisibility.Visible;
                    _countItem = 0;
                    break;
                case false:
                    btnSubmit.Enabled = false;
                    dpTask.Visibility = DockVisibility.Hidden;
                    ClearDataToUI();
                    break;
            }
        }

        List<string> ITree.GetCurrentNode(TreeListNode node)
        {
            List<string> nodes = new List<string>
            {
                node.ParentNode.ParentNode.ParentNode.ParentNode.ParentNode.GetDisplayText("colName"),
                node.ParentNode.ParentNode.ParentNode.ParentNode.GetDisplayText("colName"),
                node.ParentNode.ParentNode.ParentNode.GetDisplayText("colName"),
                node.ParentNode.ParentNode.GetDisplayText("colName"),
                node.ParentNode.GetDisplayText("colName"),
                node.GetDisplayText("colName")
            };
            return nodes;
        }


        private List<string> GetCurrent3Node(TreeListNode node)
        {
            List<string> nodes = new List<string>
            {
                node.ParentNode.ParentNode.GetDisplayText("colName"),
                node.ParentNode.GetDisplayText("colName"),
                node.GetDisplayText("colName")
            };
            return nodes;
        }

        private List<string> GetCurrent2Node(TreeListNode node)
        {
            List<string> nodes = new List<string>
            {
                node.ParentNode.GetDisplayText("colName"),
                node.GetDisplayText("colName")
            };
            return nodes;
        }

        private void tsmiEdit_Click(object sender, EventArgs e)
        {
            OpenDTCBind();
            //FileEditor.pubClass.GlobalVar.TaskNo = _dictTask["TaskNo"].ToString();
            //FileEditor.pubClass.GlobalVar.TaskRound = _dictTask["TaskRound"].ToString();
            //FileEditor.pubClass.GlobalVar.TaskName = _dictTask["TaskName"].ToString();
            //FileEditor.pubClass.GlobalVar.CANRoad = _dictTask["CANRoad"].ToString();
            //FileEditor.pubClass.GlobalVar.Module = _dictTask["Module"].ToString();
            //FileEditor.pubClass.GlobalVar.MatchSort = _dictTask["TestType"].ToString();

            //FileEditor.pubClass.GlobalVar.IsIndependent = false;
            //FileEditor.pubClass.GlobalVar.VNode = GlobalVar.CurrentVNode;
            //FileEditor.pubClass.GlobalVar.BytesEml = _cacheTemplate;
            //FileEditor.Editor editor = new Editor(true);
            //editor.ShowDialog();
        }

        private void OpenDTCBind()
        {
            Dictionary<string, string> dictTask = _dictTask.ToDictionary(t => t.Key, t => t.Value.ToString());
            EmlDTCBind eDTCBind=new EmlDTCBind(dictTask,_drEml["ExapID"].ToString());
            eDTCBind.ShowDialog();
            BindGvDTC(dictTask);
        }

        private void taskTree_HiddenEditor(object sender, EventArgs e)
        {
            taskTree.OptionsBehavior.Editable = false;
            taskTree.VisibleColumns[0].OptionsColumn.AllowFocus = false;
            taskTree.ClearFocusedColumn();

            #region 判断树内创建节点时是否有相同名称节点
            string NodeName = taskTree.FocusedNode.GetDisplayText("colName").Trim();
            if (!(Regex.IsMatch(NodeName, "^[\u4E00-\u9FA5A-Za-z0-9]+$")))
            {
                Show(DLAF.LookAndFeel, this, "请输入合法的字符", "", new[] { DialogResult.OK }, null, 0,
                    MessageBoxIcon.Information);
                _tree.StartEdit();
                return;
            }

            var Parnode = node.ParentNode.Nodes;
            int n = 0;
            for (int i = 0; i < Parnode.Count; i++)
            {
                var name = Parnode[i].GetDisplayText("colName").Trim();
                if (NodeName == name)
                {
                    n++;
                    if (n >= 2)
                    {
                        Show(DLAF.LookAndFeel, this, "已存在相同项，请重新命名", "", new[] {DialogResult.OK}, null, 0,
                            MessageBoxIcon.Information);
                        _tree.StartEdit();
                        return;
                    }
                }
            }

            #endregion

            _tree.SaveTreeList(GlobalVar.TreeXmlPath);
        }

        void ITree.DrawTreeImage()
        {
            for (int i = 0; i < taskTree.Nodes.Count; i++)
            {
                taskTree.Nodes[i].StateImageIndex = 0;
                for (int k = 0; k < taskTree.Nodes[i].Nodes.Count; k++)
                {
                    taskTree.Nodes[i].Nodes[k].StateImageIndex = 1;
                    for (int j = 0; j < taskTree.Nodes[i].Nodes[k].Nodes.Count; j++)
                    {
                        taskTree.Nodes[i].Nodes[k].Nodes[j].StateImageIndex = 2;
                        for (int s = 0; s < taskTree.Nodes[i].Nodes[k].Nodes[j].Nodes.Count; s++)
                        {
                            taskTree.Nodes[i].Nodes[k].Nodes[j].Nodes[s].StateImageIndex = 3;
                            for (int m = 0; m < taskTree.Nodes[i].Nodes[k].Nodes[j].Nodes[s].Nodes.Count; m++)
                            {
                                taskTree.Nodes[i].Nodes[k].Nodes[j].Nodes[s].Nodes[m].StateImageIndex = 4;
                                for (int n = 0;
                                    n < taskTree.Nodes[i].Nodes[k].Nodes[j].Nodes[s].Nodes[m].Nodes.Count;
                                    n++)
                                {
                                    taskTree.Nodes[i].Nodes[k].Nodes[j].Nodes[s].Nodes[m].Nodes[n].StateImageIndex = 5;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void AddTaskNode(TreeListNode treeNode, string taskName, string nodeName)
        {
            if (treeNode == null) return;
            foreach (TreeListNode TaskNameNode in treeNode.Nodes)
            {
                if (TaskNameNode.GetDisplayText("colName") == cbTestType.Text)
                {
                    var Node = TaskNameNode.Nodes.Add(new object[] {nodeName});
                    Node.StateImageIndex = 5;
                    return;
                }
            }
            var node = treeNode.Nodes.Add(new object[] {taskName});
            node.StateImageIndex = 4;
            var secNode = node.Nodes.Add(new object[] {nodeName});
            secNode.StateImageIndex = 5;
        }

        private void AddNode(TreeListNode treeNode, string nodeName)
        {
            if (treeNode == null) return;
            var secNode = treeNode.Nodes.Add(new object[] {nodeName});
            secNode.StateImageIndex = 5;
        }

        public static byte[] HexToBArray(string s)
        {
            s = s.Replace(" ", "");

            byte[] buffer = new byte[s.Length/2];
            for (int i = 0; i < s.Length; i += 2)
                buffer[i/2] = Convert.ToByte(s.Substring(i, 2), 16);

            return buffer;
        }

        private void gvTask_MouseDown(object sender, MouseEventArgs e)
        {
            //获得光标位置
            var hi = gvTask.CalcHitInfo(e.Location);
            //判断位置是否在行位置上
            if (!hi.InRow && !hi.InRowCell)
            {
                _dr = null;
                return;
            }
            if (hi.RowHandle < 0)
            {
                _dr = null;
                return;
            }
            //取一行值
            gvTask.SelectRow(hi.RowHandle);
            _dr = this.gvTask.GetDataRow(hi.RowHandle);

            //if (e.Button == MouseButtons.Left)
            //{
            //    Dictionary<string, string> dictTask = new Dictionary<string, string>
            //{
            //    {"TaskNo", _dr["TaskNo"].ToString()},
            //    {"TaskRound", _dr["TaskRound"].ToString()},
            //    {"TaskName", _dr["TaskName"].ToString()},
            //    {"CANRoad", _dr["CANRoad"].ToString()},
            //    {"Module", _dr["Module"].ToString()},
            //    {"TestType", _dr["TestType"].ToString()}
            //};
            //    gvTaskRowClick(dictTask);

            //}
            _draw.SwitchCtl(false);
            if (e.Button == MouseButtons.Right)
            {
                if (GlobalVar.CurrentVNode.Count == 0)
                {
                    Show(DLAF.LookAndFeel, this, "请先在左侧任务树选好三级车型节点...", "", new[] {DialogResult.OK}, null, 0,
                        MessageBoxIcon.Information);
                }
                if (Role == "administer" || Role == "configurator" || Role == "superadminister")
                    CMSTable.Enabled = true;
            }
        }

        private void gvTaskEml_MouseDown(object sender, MouseEventArgs e)
        {
            _drEml = null;
            //获得光标位置
            var hi = gvTaskEml.CalcHitInfo(e.Location);
            //判断位置是否在行位置上
            if (!hi.InRow && !hi.InRowCell) return;
            if (hi.RowHandle < 0) return;
            //取一行值
            gvTaskEml.SelectRow(hi.RowHandle);
            _drEml = this.gvTaskEml.GetDataRow(hi.RowHandle);
        }

        private void tmsiDel_Click(object sender, EventArgs e)
        {
            _currentOperate = DataOper.Del;
            Operate = "删除";
            if (_dr != null && Role != "superadminister")
            {
                if (IsConfig(_dr, Operate))
                    return;
                _draw.Submit();
                _dr = null;
            }
            else if (_dr != null && Role == "superadminister")
            {
                if (
                    XtraMessageBox.Show("确定要删除当前选中行的数据吗？", "提示", MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Warning) ==
                    DialogResult.OK)
                {
                    if (
                        XtraMessageBox.Show("确定要删除当前选中行的数据吗？", "提示", MessageBoxButtons.OKCancel,
                            MessageBoxIcon.Warning) ==
                        DialogResult.OK)
                    {
                        _draw.Submit();
                        string errorR = "";
                        string errorQ = "";
                        string errorP = "";
                        Dictionary<string, object> dictVehicel = new Dictionary<string, object>();
                        string[] vehicel = _dictTask["TaskNo"].ToString().Split('-');
                        _dictTask["VehicelType"] = vehicel[0];
                        _dictTask["VehicelConfig"] = vehicel[1];
                        _dictTask["VehicelStage"] = vehicel[2];
                        string jsonModule = _dictTask["Module"].ToString();
                        string strModule = ConvertModuleJsonToString(_dictTask["TaskName"].ToString(), _dictTask["Module"].ToString());
                        _dictTask["Module"] =  strModule.Remove(strModule.Length - 1);
                        _store.Del(EnumLibrary.EnumTable.PassReportNoteByTask, _dictTask, out errorP);
                        _store.Del(EnumLibrary.EnumTable.QuestionNoteByTask, _dictTask, out errorQ);
                        _store.Del(EnumLibrary.EnumTable.ReportByTask, _dictTask, out errorR);
                        _dictTask["Module"] = jsonModule;
                        _dr = null;
                    }
                }
            }
        }

        private void tmsiUpdate_Click(object sender, EventArgs e)
        {
            ModifyDataRow();
        }

        private void ModifyDataRow()
        {
            Operate = "修改";
            if (_dr != null && Role != "superadminister")
            {
                if (IsConfig(_dr, Operate))
                    return;
                UpdateRow();

            }
            else if (_dr != null && Role == "superadminister")
            {
                UpdateRow();
            }
        }

        private void UpdateRow()
        {
            ClearDataToUI();
            DrToDict();
            _draw.SetDataToUI(_dr);
            cbTestType.ReadOnly = true;
            cbName.ReadOnly = true;
            cbSegment.ReadOnly = true;
            txtModule.ReadOnly = true;
            cbeModule.ReadOnly = true;
            _currentOperate = DataOper.Update;
            _draw.SwitchCtl(true);
            //_draw.InitGrid();
            //var departmentList = _store.GetRegularByEnum(EnumLibrary.EnumTable.Task);
            //ConvertDataBaseModule(departmentList);
        }

            /// <summary>
            /// 蒋module装换成Json
            /// </summary>
            /// <param name="node"></param>
            /// <param name="listNode"></param>
            /// <returns></returns>
        private string GetModuleToJson(string node, string listNode)
        {
            string module = "";
            if (node != "")
            {
                //cbName.Properties.Items.AddRange(type);
                switch (node)
                {
                    case "CAN通信单元":
                    case "LIN通信主节点":
                    case "LIN通信从节点":
                    case "直接NM单元":
                    case "动力域NM主节点":
                    case "动力域NM从节点":
                    case "间接NM单元":
                    case "通信DTC":
                    case "OSEK NM单元":
                    case "Bootloader":
                    case "网关路由":
                        module = listNode.Split(' ')[0];
                        break;
                    case "CAN通信集成":
                    case "LIN通信集成":
                    case "直接NM集成":
                    case "间接NM集成":
                    case "动力域NM集成":
                    case "OSEK NM集成":
                        module = GetMultiModuleJson(listNode);
                        break;
                    default:
                        break;
                }
            }
            return module;
        }

        private string GetMultiModuleJson(string nodeStr)
        {
            string name = "";
            Dictionary<string, string> module = new Dictionary<string, string>();
            string remModule = nodeStr.Split(' ')[0];
            string[] a = remModule.Split('/');
            if (a.Length > 1)
            {
                string norStr = "";
                string virStr = "";
                foreach (var b in a)
                {
                    string[] nor = b.Split('(');
                    if (nor.Length > 1)
                    {
                        virStr = virStr + b + "/";
                    }
                    else
                    {
                        norStr = norStr + b + "/";
                    }
                }
                if (norStr != "")
                    module.Add("Normal", norStr.Remove(norStr.Length - 1));
                //else
                //    module.Add("Normal", norStr);
                if (virStr != "")
                    module.Add("Virtual", virStr.Remove(virStr.Length - 1));
                //else
                //{
                //    module.Add("Virtual", virStr);
                //}
                name = Json.SerJson(module);
            }
            else
            {
                string[] b = a[0].Split('(');
                if (b.Length < 1)
                {
                    module.Add("Normal", a[0]);
                    name = Json.SerJson(module);
                }
                else
                {
                    module.Add("Normal", "");
                    module.Add("Virtual", a[0]);
                    name = Json.SerJson(module);
                }
            }
            return name;
        }

        private bool IsConfig(DataRow dr, string oper)
        {
            bool isExist = false;
            Dictionary<string, object> dictTask = new Dictionary<string, object>();

            dictTask["TaskNo"] = _dr["TaskNo"];
            dictTask["TaskRound"] = _dr["TaskRound"];
            dictTask["TaskName"] = _dr["TaskName"];
            dictTask["CANRoad"] = _dr["CANRoad"];
            dictTask["Module"] = GetModuleToJson(_dr["TaskName"].ToString(),_dr["Module"].ToString());
            IList<object[]> listTask = _store.GetSpecialByEnum(EnumLibrary.EnumTable.TaskTest, dictTask);

            if (listTask.Count != 0&&listTask[0][11].ToString() != "")
            {
                Show(DLAF.LookAndFeel, this, "该条任务已经提交了任务用例表，不能" + oper +"，如果要" + oper+"请联系超级管理员"+ oper+ "...", "", new[] {DialogResult.OK}, null, 0,
                    MessageBoxIcon.Information);
                isExist = true;
            }
            return isExist;
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                if (lblTaskNo.Text == "")
                {
                    XtraMessageBox.Show("请从左边任务列表处添加任务..");
                    ClearDataToUI();
                    return;
                }
                string[] task = lblTaskNo.Text.Split('-');
                Dictionary<string, object> dictFileLink = new Dictionary<string, object>();
                dictFileLink.Add("VehicelType", task[0]);
                dictFileLink.Add("VehicelConfig", task[1]);
                dictFileLink.Add("VehicelStage", task[2]);
                IList<object[]> ListAuth = _store.GetSpecialByEnum(EnumLibrary.EnumTable.Authorization_Auth, dictFileLink);
                if (ListAuth.Count == 0)
                {
                    XtraMessageBox.Show("请先到车型管理中对该车型授权...");
                    return;
                }
                IList<object[]> FileLink = _store.GetSpecialByEnum(EnumLibrary.EnumTable.FileLinkByVehicel,
                               dictFileLink);
                if (FileLink.Count != 0)
                {
                    //IEnumerable<object[]> fileList = FileLink.Where(t => t[10].ToString() == "");
                    //if (FileLink.Count == fileList.Count())
                    //{
                    //    XtraMessageBox.Show("车辆配置工作未完成，请先返回上一步完成用例表上传工作..");

                    //    return;
                    //}

                }
                else
                {
                    XtraMessageBox.Show("车辆配置工作未完成，请先返回上一步完成配置表上传工作..");
                    return;
                }

                //添加时要确保FileLinkByVehicel内又相应配置 否则报错 这里需要后期修改
                
                _draw.Submit();
                if (sameModule)
                {
                    sameModule = false;
                    return;
                }
                _countItem = 0;
                var departmentList = _store.GetRegularByEnum(EnumLibrary.EnumTable.Task);
                ConvertDataBaseModule(departmentList);
                //IList<object[]> vehicel = _store.GetSpecialByEnum(EnumLibrary.EnumTable.TaskTest, _dictTask);
                //if (vehicel.Count != 0 && vehicel[0][11].ToString() == "")
                //{
                //    var i = lblTaskNo.Text.Split('-');
                //    List<string> TaskNo = new List<string>()
                //    {
                //        i[0],
                //        i[1],
                //        i[2],

                //    };
                //    Dictionary<string, object> _dictRelated = new Dictionary<string, object>
                //    {
                //        {"VehicelType", i[0]},
                //        {"VehicelConfig", i[1]},
                //        {"VehicelStage", i[2]},
                //        { "EmlTemplateName",_dictTask["TaskName"] + "用例表"},
                //        //{ "MatchSort",dictTask["TaskName"]}
                //    };
                //    IList<object[]> flList = _store.GetSpecialByEnum(EnumLibrary.EnumTable.FileLinkByVehicelDouble, _dictRelated);
                //    if (flList.Count != 0 && flList[0][10].ToString() != "")
                //    {
                //        FileEditor.pubClass.GlobalVar.EmlTemplateJson = flList[0][10].ToString();
                //        if (flList[0][13].ToString() == "")
                //        {
                //            var dictTem = new Dictionary<string, object>();
                //            dictTem.Add("Name", flList[0][9].ToString());
                //            IList<object[]> emlTem = _store.GetSpecialByEnum(EnumLibrary.EnumTable.ExampleTemp, dictTem);
                //            foreach (var em in emlTem)
                //            {
                //                if (em[1].ToString() == "V1.0")
                //                {
                //                    DerDictToGridView(flList[0][10].ToString(), em[2].ToString());
                //                    FileEditor.pubClass.GlobalVar.EmlTemplateColJson = em[2].ToString();
                //                }
                //            }
                //        }
                //        else
                //        {
                //            DerDictToGridView(flList[0][10].ToString(), flList[0][13].ToString());
                //            FileEditor.pubClass.GlobalVar.EmlTemplateColJson = flList[0][13].ToString();
                //        }
                //    }
                //    string strModule = ConvertModuleJsonToString(_dictTask["TaskName"].ToString(), _dictTask["Module"].ToString());
                //    lcgTaskEml.Text = "相关测试用例：" + _dictTask["TaskNo"] + "-" + _dictTask["TaskRound"] + "-" +
                //                           _dictTask["TaskName"] + "-" + _dictTask["CANRoad"] + "-" +
                //                           strModule.Remove(strModule.Length - 1) + @"(未配置用例)";
                //    lcgTaskEml.AppearanceGroup.ForeColor = Color.Red;
                //    if (
                //        XtraMessageBox.Show("该任务用例还未配置，是否现在进入用例编辑？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) ==
                //        DialogResult.OK)
                //    {
                //        FileEditor.pubClass.GlobalVar.TaskNo = _dictTask["TaskNo"].ToString();
                //        FileEditor.pubClass.GlobalVar.TaskRound = _dictTask["TaskRound"].ToString();
                //        FileEditor.pubClass.GlobalVar.TaskName = _dictTask["TaskName"].ToString();
                //        FileEditor.pubClass.GlobalVar.CANRoad = _dictTask["CANRoad"].ToString();
                //        FileEditor.pubClass.GlobalVar.Module = _dictTask["Module"].ToString();

                //        FileEditor.pubClass.GlobalVar.ModuleJson = strModule.Remove(strModule.Length - 1);
                //        FileEditor.pubClass.GlobalVar.TextBelongModule =
                //            ConvertModuleJsonToString(_dictTask["TaskName"].ToString(), _dictTask["Module"].ToString()).Remove(ConvertModuleJsonToString(_dictTask["TaskName"].ToString(), _dictTask["Module"].ToString()).Length - 1);
                //        FileEditor.pubClass.GlobalVar.IsIndependent = false;
                //        FileEditor.pubClass.GlobalVar.VNode = GlobalVar.CurrentVNode;
                //        FileEditor.pubClass.GlobalVar.BytesEml = _cacheTemplate;
                //        FileEditor.Editor editor = new Editor(true);
                //        editor.ShowDialog();
                //    }
                //}
            }
            catch (Exception)
            {
                XtraMessageBox.Show("请联系工程师");
            }
        }

        private void EditorShow()
        {
            
        }


        private void DrToDict()
        {
            _dictTask["TaskNo"] = _dr["TaskNo"];
            _dictTask["TaskRound"] = _dr["TaskRound"];
            _dictTask["TaskName"] = _dr["TaskName"];
            _dictTask["CANRoad"] = _dr["CANRoad"];
            _dictTask["Module"] = _dr["Module"];
            _dictTask["CreateTime"] = _dr["CreateTime"];
            _dictTask["Creater"] = _dr["Creater"];
            _dictTask["AuthTester"] = _dr["AuthTester"];
            _dictTask["AuthorizedFromDept"] = _dr["AuthorizedFromDept"];
            _dictTask["Supplier"] = _dr["Supplier"];

            _dictTask["ContainExmp"] = _dr["ContainExmp"];
            _dictTask["AuthorizationTime"] = _dr["AuthorizationTime"];
            _dictTask["InvalidTime"] = _dr["InvalidTime"];
            _dictTask["Remark"] = _dr["Remark"];


        }

        private bool DrawCheckNodes(List<string> nodeList)
        {
            if (nodeList == null)
                return false;
            cbeModule.Properties.Items.Clear();
            foreach (var radioItem in nodeList.Select(item => new RadioGroupItem {Description = item}))
            {
                cbeModule.Properties.Items.Add(radioItem);
                //radioGroupV.Properties.Items.Add(radioItem);
            }
            return true;
        }
        private void gcTask_MouseDown(object sender, MouseEventArgs e)
        {
            CMSTable.Enabled = false;
        }


        public static DialogResult Show(UserLookAndFeel look, IWin32Window owner, string text, string caption,
            DialogResult[] buttons, Icon icon, int defaultButton, MessageBoxIcon messageBeepSound)
        {

            XtraMessageBoxForm form = new XtraMessageBoxForm();
            Font defaultFont = new System.Drawing.Font("微软雅黑", 9.5F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            form.Appearance.Font = defaultFont;
            return
                form.ShowMessageBoxDialog(new XtraMessageBoxArgs(look, owner, text, caption, buttons, icon,
                    defaultButton));
        }

        private void cbeModule_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if(clbCache != null)
            //    clbCache.CheckState = CheckState.Unchecked;
            //clbCache = cbListStandard.Items[e.Index];
            if (isModuleWithSupplierDown)
            {
                isModuleWithSupplierDown = false;
                return;
            }

            if (cbeModule.SelectedIndex < 0)
            {
                cbeSupplier.Properties.Items.Clear();
                lbcSupplier.Items.Clear();
                txtModule.Text = "";
                return;
            }

            Dictionary<string, object> module = new Dictionary<string, object>();
            //int i = radioGroupModule.SelectedIndex;
            module["Module"] = cbeModule.Text;
            var supplierList = _store.GetSpecialByEnum(EnumLibrary.EnumTable.Suppliers, module);
            cbeSupplier.Properties.Items.Clear();
            cbeSupplier.Text = "";
            //radioGroupSuplier.Properties.Items.Clear();
            bool isFind = false;
            cbeSupplier.ReadOnly = false;
            foreach (var su in supplierList)
            {
                if (su[0].ToString() == cbeModule.Text)
                {
                    RadioGroupItem item = new RadioGroupItem();
                    item.Description = su[2].ToString();
                    item.Enabled = true;
                    cbeSupplier.Properties.Items.Add(item);
                    //cbeSupplier.Text = su[2].ToString();
                    //cbeSupplier.ReadOnly = true;
                    isFind = true;
                }
                

            }
            if (!isFind)
            {
                //XtraMessageBox.Show("该模块的供应商还没有上传，请先上传当前模块的供应商");
            }
            //string moduleText = "";
            
            //if (countChecked >= 2)
            //    radioGroupV.Enabled = true;

            
        }

        private void tsmiView_Click(object sender, EventArgs e)
        {
            _draw.InitGrid();
            var departmentList = _store.GetRegularByEnum(EnumLibrary.EnumTable.Task);
            ConvertDataBaseModule(departmentList);
            
        }


        private void cbAuthTester_ButtonClick(object sender, ButtonPressedEventArgs e)
        {

        }

        //  private void clbNormal_SelectedIndexChanged(object sender, EventArgs e)
        //  {
        //      if (!isNorFirst)
        //      {
        //          isNorFirst = true;
        //          return;
        //      }
        //      foreach (CheckedListBoxItem item in clbNormal.Items)
        //      {
        //          if (item.CheckState == CheckState.Checked)
        //          {
        //              _countItem++;
        //              foreach (CheckedListBoxItem itemv in clbVirtual.Items)
        //              {
        //                  if (item.Description == itemv.Description)
        //                  {
        //                      itemv.Enabled = false;
        //                      itemv.CheckState = CheckState.Unchecked;
        //                      break;
        //                  }
        //              }
        //          }
        //      }
        //}





        private void cbTestType_SelectedValueChanged(object sender, EventArgs e)
        {
            cbeModule.Properties.Items.Clear();
            cbeModule.Text = string.Empty;
            cbeSupplier.Properties.Items.Clear();
            cbeSupplier.Text = string.Empty;
            txtModule.Text = string.Empty;
            lbcSupplier.Items.Clear();
            if (cbName.Text == "" || lblTaskNo.Text == "" || cbSegment.Text == "")
                return;
            string[] i = lblTaskNo.Text.Split('-');
            Dictionary<string, object> vehicel = new Dictionary<string, object>
            {
                {"VehicelType", i[0]},
                {"VehicelConfig", i[1]},
                {"VehicelStage", i[2]},
                {"MatchSort", cbName.Text}
            };
            string canRoad = cbSegment.Text;
            string testType = cbTestType.Text;
            IList<object[]> flList = _store.GetSpecialByEnum(EnumLibrary.EnumTable.FileLinkByVehicelRep, vehicel);
            cbeModule.Properties.Items.Clear();
            switch (testType)
            {
                //case "CAN通信单元":
                //case "CAN通信集成":
                //case "LIN通信集成":
                case "直接NM单元":
                case "直接NM集成":
                    cbSegment.Enabled = true;
                    if (flList.Count != 0 && flList[0][7].ToString() != "")
                    {
                        List<Dictionary<string, string>> config = Json.DerJsonToLDict(flList[0][7].ToString());
                        string currTest = "";
                        foreach (Dictionary<string, string> dictCfg in config)
                        {
                            if (dictCfg["TestChannel"] == canRoad)
                            {
                                if (testType.Contains(dictCfg["NodeNetworkAttribute"])) //判断testType内是否包含直接NM
                                {
                                    if (currTest != dictCfg["DUTname"])
                                    {
                                        cbeModule.Properties.Items.Add(dictCfg["DUTname"]);
                                        currTest = dictCfg["DUTname"];
                                    }
                                }
                            }
                        }
                    }
                    break;
                //case "动力域NM集成":
                case "间接NM单元":
                case "间接NM集成":
                    cbSegment.Enabled = true;
                    if (flList.Count != 0 && flList[0][7].ToString() != "")
                    {
                        List<Dictionary<string, string>> config = Json.DerJsonToLDict(flList[0][7].ToString());
                        string currTest = "";
                        foreach (Dictionary<string, string> dictCfg in config)
                        {
                            if (dictCfg["TestChannel"] == canRoad)
                            {
                                if (testType.Contains(dictCfg["NodeNetworkAttribute"])) //判断testType内是否包含间接NM
                                {
                                    if (currTest != dictCfg["DUTname"])
                                    {
                                        cbeModule.Properties.Items.Add(dictCfg["DUTname"]);
                                        currTest = dictCfg["DUTname"];
                                    }
                                }
                            }
                        }
                    }
                    break;
                //case "通信DTC":
                //case "OSEK NM单元":
                //case "OSEK NM集成":
                //case "Bootloader":
                case "网关路由":
                    cbSegment.Enabled = true;
                    if (flList.Count != 0 && flList[0][7].ToString() != "")
                    {
                        List<Dictionary<string, string>> config = Json.DerJsonToLDict(flList[0][7].ToString());
                        foreach (Dictionary<string, string> dictCfg in config)
                        {
                            //if (dictCfg["TestChannel"] == canRoad)
                            {
                                RadioGroupItem item = new RadioGroupItem();
                                item.Description = dictCfg["DUTname"];
                                item.Enabled = true;
                                cbeModule.Properties.Items.Add(item);
                            }
                        }
                    }
                    break;
                case "LIN通信主节点":
                case "LIN通信从节点":
                    cbSegment.Enabled = true;
                    if (flList.Count != 0 && flList[0][7].ToString() != "")
                    {
                        List<Dictionary<string, string>> config = Json.DerJsonToLDict(flList[0][7].ToString());
                        string currTest = "";
                        foreach (Dictionary<string, string> dictCfg in config)
                        {
                            if (dictCfg["TestChannel"] == canRoad)
                            {
                                if (testType.Contains(dictCfg["MasterNodeType"])) //判断testType内是否包含主节点，从节点
                                {
                                    if (currTest != dictCfg["DUTname"])
                                    {
                                        cbeModule.Properties.Items.Add(dictCfg["DUTname"]);
                                        currTest = dictCfg["DUTname"];
                                    }
                                }
                            }
                        }
                    }
                    break;
                case "动力域NM主节点":
                case "动力域NM从节点":
                    cbSegment.Enabled = true;
                    if (flList.Count != 0 && flList[0][7].ToString() != "")
                    {
                        List<Dictionary<string, string>> config = Json.DerJsonToLDict(flList[0][7].ToString());
                        string currTest = "";
                        foreach (Dictionary<string, string> dictCfg in config)
                        {
                            if (dictCfg["TestChannel"] == canRoad)
                            {
                                string strNodeNetworkAttribute = string.Empty;
                                if (testType == "动力域NM主节点")
                                    strNodeNetworkAttribute = "动力域主NM";
                                else
                                    strNodeNetworkAttribute = "动力域从NM";
                                if (strNodeNetworkAttribute.Contains(dictCfg["NodeNetworkAttribute"])) //判断testType内是否包含主节点，从节点
                                {
                                    if (currTest != dictCfg["DUTname"])
                                    {
                                        cbeModule.Properties.Items.Add(dictCfg["DUTname"]);
                                        currTest = dictCfg["DUTname"];
                                    }
                                }
                            }
                        }
                    }
                    break;
                case "通信DTC":
                    cbSegment.Enabled = true;
                    if (flList.Count != 0 && flList[0][7].ToString() != "")
                    {
                        List<Dictionary<string, string>> config = Json.DerJsonToLDict(flList[0][7].ToString());
                        string currTest = "";
                        foreach (Dictionary<string, string> dictCfg in config)
                        {
                            if (dictCfg["TestChannel"] == canRoad) //DiagnosticNode
                            {
                                if (dictCfg["DiagnosticNode"].Contains("是")) //判断testType内是否包含主节点，从节点
                                {
                                    if (currTest != dictCfg["DUTname"])
                                    {
                                        cbeModule.Properties.Items.Add(dictCfg["DUTname"]);
                                        currTest = dictCfg["DUTname"];
                                    }
                                }
                            }
                        }
                    }
                    break;
                default:
                    cbSegment.Enabled = true;
                    if (flList.Count != 0 && flList[0][7].ToString() != "")
                    {
                        List<Dictionary<string, string>> config = Json.DerJsonToLDict(flList[0][7].ToString());
                        string currTest = "";
                        foreach (Dictionary<string, string> dictCfg in config)
                        {

                            if (dictCfg["TestChannel"] == canRoad)
                            {
                                if (currTest != dictCfg["DUTname"])
                                {
                                    cbeModule.Properties.Items.Add(dictCfg["DUTname"]);
                                    currTest = dictCfg["DUTname"];
                                }
                            }
                        }
                    }
                    break;
            }
        }

        private void radioGroupSuplier_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbeSupplier.SelectedIndex < 0)
            {
                lbcSupplier.Items.Clear();
                return;
            }
            //string supplier = "";
            //int index = radioGroupSuplier.SelectedIndex;
            //bool itemChecked = false;
            ////foreach (RadioGroupItem item in radioGroupModule.Properties.Items)
            //{
               
            //    {
            //        itemChecked = true;
            //        int indexM = radioGroupModule.SelectedIndex;
            //        supplier = radioGroupSuplier.Properties.Items[index].Description + "(" + radioGroupModule.Properties.Items[indexM].Description + ")" + ",";

            //    }
            //}
            //if (itemChecked)
            //    if(txtSuplier.Text == "")
            //        txtSuplier.Text = supplier.Remove(supplier.Length - 1);
            //    else
            //        txtSuplier.Text = txtSuplier.Text + "," + supplier.Remove(supplier.Length - 1);
            //else
            //    txtSuplier.Text = "";
        }

        private void cbName_SelectedValueChanged(object sender, EventArgs e)
        {
            if (cbName.Text == "")
                return;
            InitCbTestType(cbName.Text);
            if (cbName.Text != @"网关路由")
            {
                cbSegment.Enabled = true;
                cbSegment.ReadOnly = false;
            }
            else
            {
                cbSegment.Enabled = true;
                cbSegment.ReadOnly = false;
                //cbSegment.Enabled = false;
                //cbSegment.ReadOnly = true;
            }
        }

        private void InitCbTestType(string bus)
        {
            if (cbName.Text == "")
                return;
            string[] i = lblTaskNo.Text.Split('-');
            Dictionary<string, object> vehicel = new Dictionary<string, object>
            {
                {"VehicelType",i[0] },
                {"VehicelConfig",i[1]},
                {"VehicelStage",i[2] },
                {"MatchSort",cbName.Text }
            };
            string canRoad = cbSegment.Text;
            IList<object[]> flList = _store.GetSpecialByEnum(EnumLibrary.EnumTable.FileLinkByVehicelRep, vehicel);
            cbeModule.Properties.Items.Clear();
            cbSegment.Properties.Items.Clear();
            listSeg.Clear();
            switch (cbName.Text.Trim())
            {
                case "CAN总线":
                case "LIN总线":
                case "网关路由":
                    if (flList.Count != 0 && flList[0][7].ToString() != "")
                    {
                        List<Dictionary<string, string>> config = Json.DerJsonToLDict(flList[0][7].ToString());
                        string currTest = "";
                        foreach (Dictionary<string, string> dictCfg in config)
                        {
                            bool same = false;
                            foreach (var item in listSeg)
                            {
                                if (item.ToString() == dictCfg["TestChannel"])
                                {
                                    same = true;
                                    break;
                                }
                            }
                            if (!same)
                            {
                                cbSegment.Properties.Items.Add(dictCfg["TestChannel"]);
                                listSeg.Add(dictCfg["TestChannel"]);
                            }
                        }
                    }
                    break;
                case "网关路由1":
                    if (flList.Count != 0 && flList[0][7].ToString() != "")
                    {
                        List<Dictionary<string, string>> config = Json.DerJsonToLDict(flList[0][7].ToString());

                        foreach (Dictionary<string, string> dictCfg in config)
                        {
                            //if (dictCfg["TestChannel"] == canRoad)
                            {
                                RadioGroupItem item = new RadioGroupItem();
                                item.Description = dictCfg["DUTname"];
                                item.Enabled = true;
                                cbeModule.Properties.Items.Add(item);
                            }
                        }
                    }
                    break;
                default:
                    break;
            }

            if (bus != "")
            {
                cbTestType.Properties.Items.Clear();
                Dictionary<string, object> type = new Dictionary<string, object>
                {
                    {"BusType",bus}
                };
                IList<object[]> flList1 = _store.GetSpecialByEnum(EnumLibrary.EnumTable.Example_QueryByBusType, type);
                for (int j = 0; j < flList1.Count; j++)
                {
                    cbTestType.Properties.Items.Add(flList1[j][3]);
                }
            }
    }

        private List<string> GetTestType(IList<object[]> listExap,string[] type)
        {
            List<string> eaxpList = new List<string>();
            foreach (var list in listExap)
            {
                if (type.Contains(list[9].ToString().Split('用')[0]))
                {
                    eaxpList.Add(list[9].ToString().Split('用')[0]);
                }
            }
            return eaxpList;
        }
    
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (cbeModule.SelectedIndex < 0)
            {
                MessageBox.Show("请选择模块...");
                return;
            }

            //if (cbeSupplier.Text == "")
            //{
            //    MessageBox.Show("请选择当前选中模块对应的供应商...");
            //    return;
            //}
            //if (cbeModule.SelectedIndex < 0 && cbeSupplier.SelectedIndex < 0)
            //{
            //    MessageBox.Show("请选择当前选中模块和对应的供应商...");
            //    return;
            //}
            txtModule.ReadOnly = false;
            //txtSuplier.ReadOnly = false;
            if (ModuleCount(cbTestType.Text, txtModule.Text))
                return;
            int indexM = cbeModule.SelectedIndex;

            string moduleText = cbeModule.Text + @"/";
            string oldMoudleText = txtModule.Text;
            if (oldMoudleText == "")
                oldMoudleText = moduleText.Remove(moduleText.Length - 1);
            else
                oldMoudleText = oldMoudleText + @"/" + moduleText.Remove(moduleText.Length - 1);
            //if (ModuleCount(cbTestType.Text, oldMoudleText))
            //{
            //    return;
            //}


            txtModule.Text = oldMoudleText;
            string supplier = "";
            //int indexS = radioGroupSuplier.SelectedIndex;
            supplier = cbeSupplier.Text + "(" + cbeModule.Text + ")";
            lbcSupplier.Items.Add(supplier);
            txtModule.ReadOnly = true;
        }



        private bool ModuleCount(string testType,string module)
        {
            bool multi = false;
            if (testType != "")
            {
                switch (testType)
                {
                    case "CAN通信单元":
                    case "LIN通信主节点":
                    case "LIN通信从节点":
                    case "直接NM单元":
                    case "动力域NM主节点":
                    case "动力域NM从节点":
                    case "间接NM单元":
                    case "通信DTC":
                    case "OSEK NM单元":
                    case "Bootloader":
                    case "网关路由":
                        string[] countM = module.Split('/');
                        
                        if (countM.Count() == 1 && countM[0] != "")
                        {
                            MessageBox.Show(testType + "测试时只能添加一个测试节点");
                            multi = true;
                        }
                        break;
                    case "CAN通信集成":
                    case "LIN通信集成":
                    case "直接NM集成":
                    case "间接NM集成":
                    case "动力域NM集成":
                    case "OSEK NM集成":
                        string[] countJ = module.Split('/');
                        foreach (var moudle in countJ)
                        {
                            if (moudle == cbeModule.Text)
                            {
                                MessageBox.Show("已经添加了相同的节点");
                                multi = true;
                            }
                        }
                        //if (countJ.Count() <= 1)
                        //{
                        //    MessageBox.Show(testType + "测试时必须添加两个以上测试节点");
                        //    multi = true;
                        //}
                        break;
                   
                    case "诊断协议":
                        //DrawCheckNodes(GlobalVar.LinCollection);
                        break;
                    case "Oskeg":
                        break;
                }

            }
            return multi;
        }
        private bool ModuleCountByOK(string testType, string module)
        {
            bool multi = false;
            if (testType != "")
            {
                switch (testType)
                {
                    case "CAN通信单元":
                    case "LIN通信主节点":
                    case "LIN通信从节点":
                    case "直接NM单元":
                    case "动力域NM主节点":
                    case "动力域NM从节点":
                    case "间接NM单元":
                    case "通信DTC":
                    case "OSEK NM单元":
                    case "Bootloader":
                    case "网关路由":
                        string[] countM = module.Split('/');
                        if (countM.Count() < 1)
                        {
                            MessageBox.Show(testType + "测试时必须添加一个测试节点，不能为空");
                            multi = true;
                            sameModule = true;
                        }
                        break;
                    case "CAN通信集成":
                    case "LIN通信集成":
                    case "直接NM集成":
                    case "间接NM集成":
                    case "动力域NM集成":
                    case "OSEK NM集成":
                        string[] countJ = module.Split('/');
                        if (countJ.Count() <= 1)
                        {
                            XtraMessageBox.Show(testType + "测试时必须添加两个以上测试节点");
                            multi = true;
                            sameModule = true;
                        }
                        break;
                    default:
                        XtraMessageBox.Show("请先选择测试类型");
                        multi = true;
                        sameModule = true;
                        break;
                }

            }
            else
            {
                MessageBox.Show("请先选择测试测试类型");
                multi = true;
                sameModule = true;
            }
            return multi;
        }

        private void btnDel_Click(object sender, EventArgs e)
        {

            if(cbeModule.Text == "" )
            {
                MessageBox.Show("请选择模块...");
                return;
            }
            //if (cbeSupplier.Text == "" )
            //{
            //    MessageBox.Show("请选择当前选中模块对应的供应商...");
            //    return;
            //}
            //if (cbeModule.SelectedIndex < 0 && cbeSupplier.SelectedIndex < 0)
            //{
            //    MessageBox.Show("请选择当前选中模块和对应的供应商...");
            //    return;
            //}
            txtModule.ReadOnly = false;
            string[] module = txtModule.Text.Split('/');
            //int indexM = cbeModule.SelectedIndex;
            string checkModule = cbeModule.Text;
            string newModule = "";
            foreach (var mo in module)
            {
                if (checkModule == mo)
                    continue;
                else

                {
                    if (mo != "")
                        if (newModule == "")
                            newModule = mo + "/";
                        else
                            newModule = newModule + mo + "/";
                    else
                        continue;
                }
            }

            if (newModule != "")
            {
                txtModule.Text = newModule.Remove(newModule.Length - 1);
            }

            else
            {
                txtModule.Text = newModule;
            }
                

            //int indexS = radioGroupSuplier.SelectedIndex;
            string checkSuplier = cbeSupplier.Text;
            checkSuplier = checkSuplier + "(" + checkModule + ")";

            foreach (var item in lbcSupplier.Items)
            {
                if (item.ToString() == checkSuplier)
                {
                    lbcSupplier.Items.Remove(item);
                    break;
                }
            }
            txtModule.ReadOnly = true;
        }


        private void gcTask_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var hi = gvTask.CalcHitInfo(e.Location);
            //判断位置是否在行位置上
            if (!hi.InRow && !hi.InRowCell)
            {
                _dr = null;
                return;
            }
            if (hi.RowHandle < 0)
            {
                _dr = null;
                return;
            }
            //获得选中行的数值
            int rowCount = hi.RowHandle;
            _dr = this.gvTask.GetDataRow(rowCount);
            ModifyDataRow();
        }
        private void ShieldRight()
        {
            ContextMenu emptyMenu_1 = new ContextMenu();
            txtFounder.Properties.ContextMenu = emptyMenu_1;
            ContextMenu emptyMenu_2 = new ContextMenu();
            txtModule.Properties.ContextMenu = emptyMenu_2;
            ContextMenu emptyMenu_3 = new ContextMenu();
            txtRound.Properties.ContextMenu = emptyMenu_3;
            //ContextMenu emptyMenu_4 = new ContextMenu();
            //txtSuplier.Properties.ContextMenu = emptyMenu_3;
        }

        /// <summary>
        /// 为node添加编号
        /// </summary>
        /// <param name="nodes">选中的node下的nodes</param>
        /// <param name="nodeName">创建的名称</param>
        /// <returns></returns>
        private TreeListNode AddNodeNameIndex(TreeListNodes nodes, string nodeName)
        {
            bool ifnodeNameSame = false;
            List<int> listNodeIndex = new List<int>();
            foreach (TreeListNode tnode in nodes)
            {
                if (tnode.GetDisplayText("colName").Length >= nodeName.Length)
                {
                    if (nodeName.Equals(tnode.GetDisplayText("colName")))
                    {
                        ifnodeNameSame = true;
                        continue;
                    }
                    try
                    {
                        string tempStr = tnode.GetDisplayText("colName").Replace(nodeName, "");
                        listNodeIndex.Add(int.Parse(tempStr));
                    }
                    catch (Exception e)
                    {

                    }
                }
            }
            QuickSort(listNodeIndex, 0, listNodeIndex.Count - 1);
            string nodeIndexName;
            if (listNodeIndex.Count > 0)
            {
                nodeIndexName = nodeName + (listNodeIndex[listNodeIndex.Count - 1] + 1);
            }
            else
            {
                nodeIndexName = ifnodeNameSame ? nodeName + "1" : nodeName;
            }
            return nodes.Add(new object[] { nodeIndexName });
        }
        #region 快速排序算法

        private static int Division(List<int> list, int left, int right)
        {
            while (left < right)
            {
                int num = list[left]; //将首元素作为枢轴
                if (num > list[left + 1])
                {
                    list[left] = list[left + 1];
                    list[left + 1] = num;
                    left++;
                }
                else
                {
                    int temp = list[right];
                    list[right] = list[left + 1];
                    list[left + 1] = temp;
                    right--;
                }
            }
            return left; //指向的此时枢轴的位置
        }
        private static void QuickSort(List<int> list, int left, int right)
        {
            if (left < right)
            {
                int i = Division(list, left, right);
                //对枢轴的左边部分进行排序
                QuickSort(list, i + 1, right);
                //对枢轴的右边部分进行排序
                QuickSort(list, left, i - 1);
            }
        }

        #endregion

        private void gvTaskEml_DoubleClick(object sender, EventArgs e)
        {
            OpenDTCBind();
        }

        private void SupplierToUI()
        {
            int index = lbcSupplier.SelectedIndex;
            if (index == -1)
                return;
            string Supplier = lbcSupplier.GetItem(index).ToString();
            if (string.IsNullOrEmpty(Supplier))
                return;
            isModuleWithSupplierDown = true;
            var strArray = Supplier.Split('(');
            if (strArray.Length == 2)
            {
                cbeSupplier.EditValue = strArray[0];
                cbeModule.EditValue = strArray[1].Remove(strArray[1].Length - 1);
            }
            else
            {
                cbeSupplier.SelectedIndex = -1;
                cbeModule.EditValue = strArray[0].Remove(strArray[1].Length - 1);
            }
        }
        private void lbcSupplier_MouseDown(object sender, MouseEventArgs e)
        {
            SupplierToUI();
        }
        #region 下方绑定DTC的GridView
        private void BindGvDTC(Dictionary<string, string> dictVNode)
        {
            _selectedEmlName = dictVNode["TaskName"] + "用例表";
            _dt = new DataTable();
            _dictDTC = SelectDTCInfo(dictVNode);
            InitDTCGrid(_selectedEmlName);
        }
        private Dictionary<string, string> SelectDTCInfo(Dictionary<string, string> dictVNode)
        {
            var dictVNodeo = dictVNode.ToDictionary(t => t.Key, t => (object)t.Value);
            var listDTCInfo = _store.GetSingnalColByCon(EnumLibrary.EnumTable.DTC, dictVNodeo, 11);
            var dtcJson = listDTCInfo.Count > 0 ? listDTCInfo[0] : string.Empty;
            var dictDTC = Json.DerJsonToDict(dtcJson) == null
                ? new Dictionary<string, string>()
                : Json.DerJsonToDict(dtcJson);
            return dictDTC;
        }
        private void InitDTCGrid(string selectedName)
        {
            Dictionary<string, Dictionary<string, string>>  dictdictEmllist = new Dictionary<string, Dictionary<string, string>>();
            var exampleList = _store.GetRegularByEnum(EnumLibrary.EnumTable.ExampleTemp);
            for (int row = 0; row < exampleList.Count; row++)
            {
                Dictionary<string, string> dictEmlList = new Dictionary<string, string>();
                dictEmlList["Version"] = exampleList[row][1].ToString();
                dictEmlList["Content"] = exampleList[row][2].ToString();
                dictEmlList["MatchSort"] = exampleList[row][3].ToString();
                dictEmlList["ImportDate"] = exampleList[row][4].ToString();
                dictEmlList["EmlTemplate"] = exampleList[row][5].ToString();
                dictdictEmllist[exampleList[row][0].ToString()] = dictEmlList;
            }
            if (dictdictEmllist.ContainsKey(selectedName))
            {
                DerDictToGridView(dictdictEmllist[selectedName]["EmlTemplate"], dictdictEmllist[selectedName]["Content"]);
            }
        }
        private void DerDictToGridView(string eml, string colEml)
        {
            var _dictListDrScheme = Json.DeserJsonDList(colEml);
            if (_dictListDrScheme == null)
                return;
            _dt.Clear();
            CreateEmlGridView(_dictListDrScheme);
            InitDataTable();
            _dt = DerJsonToDrs(eml);
            gcTaskEml.DataSource = _dt;
        }
        private void InitDataTable()
        {
            _dt = new DataTable();
            var coList = new List<string>();
            foreach (GridColumn col in gvTaskEml.Columns)
                coList.Add(col.FieldName);
            foreach (var colName in coList.ToArray())
                _dt.Columns.Add(new DataColumn(colName, typeof(object)));
            gcTaskEml.DataSource = _dt;
        }
        /// <summary>
        /// 把Json转换成DataRow,填充进已经搭好框架的DataTable里去
        /// </summary>
        /// <returns></returns>
        private DataTable DerJsonToDrs(string eml)
        {
            var dictChapter = Json.DeserJsonToDDict(eml) == null
                ? new Dictionary<string, Dictionary<string, List<object>>>()
                : Json.DeserJsonToDDict(eml);
            int colCount = gvTaskEml.Columns.Count;
            foreach (KeyValuePair<string, Dictionary<string, List<object>>> scheme in dictChapter)
            {
                object[] obj = new object[colCount];
                int i = 0;
                obj[i] = scheme.Key;
                i++;
                foreach (KeyValuePair<string, List<object>> col in scheme.Value)
                {
                    i = 1;
                    var ss = Json.DerJsonToDict(col.Value[0].ToString());
                    foreach (KeyValuePair<string, string> dict in ss)
                    {
                        obj[i] = dict.Value;
                        i++;
                        if (i == 4) break;
                    }
                    //var dd = col.Value[1].ToString();
                    //obj[i] = dd;
                    //i++;
                    string strBindDTC = "未绑定";
                    if (_dictDTC.ContainsKey(col.Key))
                    {
                        if (!string.IsNullOrEmpty(_dictDTC[col.Key]))
                        {
                            strBindDTC = "已绑定";
                        }
                    }
                    //obj[i] = _dictDTC.ContainsKey(col.Key) ? _dictDTC[col.Key] : string.Empty;
                    obj[i] = strBindDTC;
                    _dt.Rows.Add(obj);
                }
            }
            return _dt;
        }
        /// <summary>
        /// 初始化gvEml
        /// </summary>
        private void CreateEmlGridView(Dictionary<string, List<object>> listDrScheme)
        {
            var listColumns = ListColumns();
            gvTaskEml.Columns.Clear();
            foreach (KeyValuePair<string, List<object>> scheme in listDrScheme)
            {
                if (bool.Parse(scheme.Value[2].ToString()))
                {
                    if (listColumns.Contains(scheme.Key))
                    {
                        GridColumn col = new GridColumn();
                        col.Caption = scheme.Value[1].ToString();
                        col.Name = scheme.Key;
                        col.FieldName = scheme.Key;
                        col.Visible = true;
                        gvTaskEml.Columns.AddRange(new GridColumn[] { col });
                    }
                }
            }
            GridColumn colNew = new GridColumn();
            colNew.Caption = "DTC";
            colNew.Name = "DTC";
            colNew.FieldName = "DTC";
            colNew.Visible = true;
            gvTaskEml.Columns.AddRange(new GridColumn[] { colNew });
        }
        private List<string> ListColumns()
        {
            List<string> listColumns = new List<string>();
            listColumns.Add("Chapter");
            listColumns.Add("ExapID");
            listColumns.Add("ReflectionID");
            listColumns.Add("ExapName");
            return listColumns;
        }
        #endregion

        private void gcTaskEml_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            _drEml = null;
            var hi = gvTaskEml.CalcHitInfo(e.Location);
            //判断位置是否在行位置上
            if (!hi.InRow && !hi.InRowCell) return;
            if (hi.RowHandle < 0) return;
            //获得选中行的数值
            int rowCount = hi.RowHandle;
            _drEml = this.gvTaskEml.GetDataRow(rowCount);
        }
    }
}
