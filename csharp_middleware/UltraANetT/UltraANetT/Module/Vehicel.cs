using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using DevExpress.LookAndFeel;
using DevExpress.Skins;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraTreeList.Nodes;
using ProcessEngine;
using UltraANetT.Properties;
using FileEditor;
using UltraANetT.Form;
using UltraANetT.Interface;
using DevExpress.XtraBars.Docking;
using DevExpress.XtraTreeList;
using DBEngine;
using DevExpress.XtraRichEdit.API.Word;
using Font = System.Drawing.Font;
using System.Text.RegularExpressions;
using NHibernate.Linq;

namespace UltraANetT.Module
{
   
    public partial class Vehicel : XtraUserControl,IDraw,ITree
    {
        #region 参数变量
        private readonly List<string> _rootNodes = new List<string>();
        private readonly List<List<string>> _secondaryNodes = new List<List<string>>();
        private readonly List<List<List<string>>> _finalNodes = new List<List<List<string>>>();

        private readonly ProcShow _show = new ProcShow();
        private readonly ProcStore _store = new ProcStore();
        private readonly CopyVehicelInfo _copypaste = new CopyVehicelInfo();
        private readonly ITree _tree;
        public  INode NodeStr;
        private readonly IDraw _draw;
        private ProcFile _file = new ProcFile();

        private DataRow _dr;
        /// <summary>
        /// 字典类型的车辆信息
        /// </summary>
        private Dictionary<string, object> _dictVehicel = new Dictionary<string, object>();
        /// <summary>
         
        /// <summary>
        /// 当前操作类别
        /// </summary>
        private DataOper _currentOperate;

        private ProcLog Log = new ProcLog();
        private LogicalControl _LogC = new LogicalControl();
        private PictureEdit _pictCache;
        private bool _isHaveCurrentNode = false;
        private string role;
        private TreeListNode node = null;
        private string Operate;
        private string strAuthorizeTo;
        private bool cmsTable = true;

        List<string> FocusedNodes = new List<string>();//复制阶段时保存的车型
        //List<string> NewNodes = new List<string>();
        #endregion

        public Vehicel()
        {
            InitializeComponent();
            _draw = this;
            _tree = this;
            _tree.BingEvent();
            _tree.DrawTreeColor();
            _tree.LoadTreeList(GlobalVar.TreeXmlPath);
            //GetAllNodesByRank();
            _draw.InitDict();
            _draw.InitGrid();
            ExpandTree();
            VisibleToList();
            CheckForIllegalCrossThreadCalls = false;

            role = _LogC.RoleSelect(GlobalVar.UserName);
            RoleFunction(role);
            if (GlobalVar.VehicelNode != "")
            {
                List<string> vehicelList = new List<string>(GlobalVar.VehicelNode.Split('-'));
                IsConfig(vehicelList);
            }
            hideContainerRight.Visible = false;
            ShieldRight();
        }

        private void ExpandTree()
        {
            if (GlobalVar.CurrentVNode.Count != 0)
            {
                vehicelTree.ExpandToLevel(1);
                for (int i = 0; i < vehicelTree.Nodes.Count; i++)
                {
                    string name = vehicelTree.Nodes[i].GetValue("colName").ToString();
                    if (name == GlobalVar.CurrentVNode[0])
                    {
                        for (int j = 0; j < vehicelTree.Nodes[i].Nodes.Count; j++)
                        {
                            string secondName = vehicelTree.Nodes[i].Nodes[j].GetValue("colName").ToString();
                            if (secondName != GlobalVar.CurrentVNode[1])
                            {
                                vehicelTree.Nodes[i].Nodes[j].Expanded = false;
                            }
                        }
                    }
                    else
                        vehicelTree.Nodes[i].Expanded = false;
                }
            }
        }
        #region 判断当前登录人员的角色
        private void RoleFunction(string role)
        {
            switch(role)
            {
                case "superadminister":
                case "administer":
                    break;
                case "configurator":
                    CMSTable.Enabled = false;
                    CMSConfig.Enabled = false;
                    CMSCreate.Enabled = false;
                    CMSStage.Enabled = false;
                    CMSVehicel.Enabled = false;
                    break;
                case "tester":
                    CMSTable.Enabled = false;
                    CMSConfig.Enabled = false;
                    CMSCreate.Enabled = false;
                    CMSStage.Enabled = false;
                    CMSVehicel.Enabled = false;
                    //btnCheck.Enabled = false;
                    pictFirst.Enabled = false;
                    pictSecond.Enabled = false;
                    
                    break;
                default:
                    break;
            }
        }
        #endregion
        private void vehicelTree_MouseDown(object sender, MouseEventArgs e)
        {
            node = vehicelTree.CalcHitInfo(new Point(e.X, e.Y)).Node;
            TreeNodeMouseDown(node);
            switch (e.Button)
            {
                case MouseButtons.Right:
                    if(cmsTable)
                        _tree.ShowCmsByNode(node);
                    break;
            }
        }

        private void TreeNodeMouseDown(TreeListNode node)
        {
            var level = node?.Level;
            var dt = new DataTable();
            var coList = new List<string>();
            foreach (GridColumn col in gvVechiel.Columns)
                coList.Add(col.FieldName);
            foreach (var colName in coList)
            {
                dt.Columns.Add(new DataColumn(colName, typeof(object)));
            }
            if (level == 2)
            {
                GlobalVar.CurrentVNode = GetCurrentVNode(node);
                bool isSuccess = NodeStr.SetVNode(GlobalVar.CurrentVNode);
                Dictionary<string, object> dictStage = new Dictionary<string, object>();
                dictStage["VehicelType"] = GlobalVar.CurrentVNode[0];
                dictStage["VehicelConfig"] = GlobalVar.CurrentVNode[1];
                dictStage["VehicelStage"] = GlobalVar.CurrentVNode[2];
                dictStage["level"] = "level3";
                IList<object[]> stage = _store.GetSpecialByEnum(EnumLibrary.EnumTable.Authorization_Table, dictStage);
                tabAuthorize.TabPages[0].Text = "[" + GlobalVar.CurrentVNode[0] + "-" + GlobalVar.CurrentVNode[1] + "-" +
                                                GlobalVar.CurrentVNode[2] + "] 授权";
                tabAuthorize.TabPages[1].Text = "[" + GlobalVar.CurrentVNode[0] + "-" + GlobalVar.CurrentVNode[1] + "-" +
                                                GlobalVar.CurrentVNode[2] + "] 配置";
                pageSet.PageEnabled = true;
                foreach (var dept in stage)
                    dt.Rows.Add(dept);
                //gcVechiel.DataSource = null; 
                gcVechiel.DataSource = dt;

                IsConfig(GlobalVar.CurrentVNode);
                if (isSuccess)
                    _isHaveCurrentNode = true;
            }
            else if (level == 1)
            {
                GlobalVar.CurrentVNode.Clear();
                List<string> vNode = GetCurrent2VNode(node);
                bool isSuccess = NodeStr.SetVNode(vNode);
                tabAuthorize.TabPages[0].Text = "车型授权";
                tabAuthorize.TabPages[1].Text = "车型配置";
                pageSet.PageEnabled = false;
                Dictionary<string, object> dictStage = new Dictionary<string, object>();
                dictStage["VehicelType"] = vNode[0];
                dictStage["VehicelConfig"] = vNode[1];

                dictStage["level"] = "level2";
                IList<object[]> config = _store.GetSpecialByEnum(EnumLibrary.EnumTable.Authorization_Table, dictStage);
                foreach (var dept in config)
                    dt.Rows.Add(dept);
                gcVechiel.DataSource = dt;
                if (isSuccess)
                    _isHaveCurrentNode = false;
            }
            else if (level == 0)
            {
                string nodes = node.GetDisplayText("colName");
                GlobalVar.CurrentVNode.Clear();
                tabAuthorize.TabPages[0].Text = "车型授权";
                tabAuthorize.TabPages[1].Text = "车型配置";
                pageSet.PageEnabled = false;
                List<string> vNode = new List<string>();
                vNode.Add(nodes);
                bool isSuccess = NodeStr.SetVNode(vNode);
                Dictionary<string, object> dictStage = new Dictionary<string, object>();
                dictStage["VehicelType"] = nodes;

                dictStage["level"] = "level1";
                IList<object[]> vehicel = _store.GetSpecialByEnum(EnumLibrary.EnumTable.Authorization_Table, dictStage);
                foreach (var dept in vehicel)
                    dt.Rows.Add(dept);
                //gcVechiel.DataSource = null;
                gcVechiel.DataSource = dt;
                if (isSuccess)
                    _isHaveCurrentNode = false;
            }
            //else
            //{
            //    pageSet.PageEnabled = false;
            //}
        }

        private void ShowSelectedVehicel()
        {
            var dt = new DataTable();
            var coList = new List<string>();
            foreach (GridColumn col in gvVechiel.Columns)
                coList.Add(col.FieldName);
            foreach (var colName in coList)
            {
                dt.Columns.Add(new DataColumn(colName, typeof(object)));
            }
            bool isSuccess = NodeStr.SetVNode(GlobalVar.CurrentVNode);
            Dictionary<string, object> dictStage = new Dictionary<string, object>();
            dictStage["VehicelType"] = GlobalVar.CurrentVNode[0];
            dictStage["VehicelConfig"] = GlobalVar.CurrentVNode[1];
            dictStage["VehicelStage"] = GlobalVar.CurrentVNode[2];
            dictStage["level"] = "level3";
            IList<object[]> stage = _store.GetSpecialByEnum(EnumLibrary.EnumTable.Authorization_Table, dictStage);
            tabAuthorize.TabPages[0].Text = "[" + GlobalVar.CurrentVNode[0] + "-" + GlobalVar.CurrentVNode[1] + "-" +
                                            GlobalVar.CurrentVNode[2] + "] 授权";
            tabAuthorize.TabPages[1].Text = "[" + GlobalVar.CurrentVNode[0] + "-" + GlobalVar.CurrentVNode[1] + "-" +
                                            GlobalVar.CurrentVNode[2] + "] 配置";
            foreach (var dept in stage)
                dt.Rows.Add(dept);
            gcVechiel.DataSource = dt;

            IsConfig(GlobalVar.CurrentVNode);
            if (isSuccess)
                _isHaveCurrentNode = true;
        }

        private void IsConfig(List<string> VNode)
        {
            if (VNode.Count == 3||VNode.Count >5)
            {

                Dictionary<string, object> dictNode = new Dictionary<string, object>();
                dictNode.Add("VehicelType", VNode[0]);
                dictNode.Add("VehicelConfig", VNode[1]);
                dictNode.Add("VehicelStage", VNode[2]);
                IList<object[]> config = _store.GetSpecialByEnum(EnumLibrary.EnumTable.Authorization_Auth, dictNode);
                //pictureEdit4.Image = Resources.True;
                if (config.Count == 0)
                {
                    pictureEdit1.Image = Resources.unable;
                    pictureEdit2.Image = Resources.unable;
                    pictureEdit3.Image = Resources.unable;
                }
                else
                {
                    IList<object[]> dbcList = _store.GetSpecialByEnum(EnumLibrary.EnumTable.DbcCheck, dictNode);
                    IList<object[]> List = _store.GetSpecialByEnum(EnumLibrary.EnumTable.FileLinkByVehicel, dictNode);
                    var tplyList = _store.GetSpecialByEnum(EnumLibrary.EnumTable.Topology, dictNode);
                    if (dbcList.Count == 0)
                    {
                        pictureEdit1.Image = Resources.unable;
                        pictureEdit2.Image = Resources.unable;
                        pictureEdit3.Image = Resources.unable;
                        //pictureEdit4.Image = Resources.False;
                    }
                    else 
                    {
                        pictureEdit1.Image = Resources.able;
                        if (tplyList.Count != 0)
                        {
                            bool isCongig = false;
                            foreach (var item in tplyList)
                            {
                                if (item[3] == null||item[3].ToString() == "" || item[3].ToString() == "System.Byte[]")
                                    pictureEdit2.Image = Resources.unable;
                                else if (item[3] != null && item[3].ToString() != "" && item[3].ToString() != "System.Byte[]")
                                {
                                    pictureEdit2.Image = Resources.able;
                                    isCongig = true;
                                }
                                if (isCongig)
                                    break;
                            }
                        }
                        else
                        {
                            pictureEdit2.Image = Resources.unable;
                        }

                        bool isConfig = false;
                        pictureEdit3.Image = Resources.unable;
                        foreach (var item in List)
                        {
                            if (item[7] == null || item[7].ToString() == "" || item[7].ToString() == "System.Byte[]")
                                pictureEdit3.Image = Resources.unable;
                            else if (item[7] != null && item[7].ToString() != "" && item[7].ToString() != "System.Byte[]")
                            {
                                pictureEdit3.Image = Resources.able;
                                isConfig = true;
                            }
                            if (isConfig)
                                break;
                        }
                        //if (List.Count == 0 || List[0][10] == null || List[0][10].ToString() == "" || List[0][10].ToString() == "System.Byte[]")
                        //{
                        //    pictureEdit2.Image = Resources.unable;
                        //    pictureEdit3.Image = Resources.unable;
                        //    //pictureEdit4.Image = Resources.False;

                        //}
                        //else if (List[0][10] != null || List[0][10].ToString() != "" || List[0][10].ToString() != "System.Byte[]")
                        //{
                        //    pictureEdit2.Image = Resources.able;
                        //    if (List[0][4].ToString() != "")
                        //        pictureEdit3.Image = Resources.able;
                        //    else
                        //        pictureEdit3.Image = Resources.unable;
                        //}
                        //else
                        //{
                        //    pictureEdit2.Image = Resources.unable;
                        //    pictureEdit3.Image = Resources.unable;
                        //}
                    }
                }
            }

        }

        /// <summary>
        /// 隐藏任务节点
        /// </summary>
        private void VisibleToList()
        {
            if (vehicelTree.Nodes == null)
                return;
            for (var i0 = 0; vehicelTree.Nodes.Count > i0; i0++)
            {
                for (var i1 = 0; vehicelTree.Nodes[i0].Nodes.Count > i1; i1++)
                {
                    for (var i2 = 0; vehicelTree.Nodes[i0].Nodes[i1].Nodes.Count > i2; i2++)
                    {
                        for (var i3 = 0; vehicelTree.Nodes[i0].Nodes[i1].Nodes[i2].Nodes.Count > i3; i3++)
                        {
                            vehicelTree.Nodes[i0].Nodes[i1].Nodes[i2].Nodes[i3].Visible = false;
                        }
                    }
                }
            }
        }




        private void tabAuthorize_Click(object sender, EventArgs e)
        {
            if (tabAuthorize.SelectedTabPage == pageSet)
            {
                dpVehicel.Visible = false;
                

            }
            
        }

        void IDraw.InitGrid()
        {
            var coList = new List<string>();
            foreach (GridColumn col in gvVechiel.Columns)
                coList.Add(col.FieldName);
            var dt = _show.DrawDtFromMultiple(coList.ToArray(), EnumLibrary.EnumTable.Authorization);
            gcVechiel.DataSource = dt;
            //cbDepartment.Properties.Items.Clear();
            //IList<string> deList = _store.GetSingnalCol(EnumLibrary.EnumTable.Department,0);
            //foreach (var de in deList)
            //    cbDepartment.Properties.Items.Add(de);
            
            IList<object[]> Em = _store.GetRegularByEnum(EnumLibrary.EnumTable.Employee);

            txtCreater.Properties.Items.Clear();
            cbAuthTo.Properties.Items.Clear();
            for (int i = 0; i < Em.Count; i++)
            {
                if (Em[i][2].ToString() == "管理员")
                {
                    if (Em[i][1].ToString() == GlobalVar.UserName)
                    {
                        txtCreater.Properties.Items.Add(Em[i][1].ToString() + "(" + Em[i][0].ToString() + "-" + Em[i][3].ToString() + ")");
                    }
                }
                if (Em[i][2].ToString() == "管理员" || Em[i][2].ToString() == "配置员")
            {
                    cbAuthTo.Properties.Items.Add(Em[i][1].ToString() + "(" + Em[i][0].ToString() + "-" + Em[i][3].ToString() + ")");
            }
        }
        }

        private enum DataOper
        {
            Add = 0,
            Update = 1,
            Del = 2
        }

        void ClearDataToUI()
        {
            cbvehicelType.SelectedIndex = 0;
            //dateCreate.Text = null;
            //txtCreater.Text = null;
            //cbAuthTo.Text = null;
            //cbDepartment.Text = null;
            txtRemark.Text = null;
        }

        private Dictionary<string, object> UserDict()
        {
            _dictVehicel["EmployeeName"] = GlobalVar.UserName;
            _dictVehicel["EmployeeNo"] = GlobalVar.UserNo;
            _dictVehicel["Department"] = GlobalVar.UserDept;
            return _dictVehicel;
        }

        private Dictionary<string, object> UserDr()
        {
            UserDict();
            _dictVehicel["VehicelType"] = _dr["VehicelType"];
            _dictVehicel["VehicelConfig"] = _dr["VehicelConfig"];
            _dictVehicel["VehicelStage"] = _dr["VehicelStage"];
            _dictVehicel["CreateTime"] = DateTime.Parse(_dr["CreateTime"].ToString()).ToString("yyyy-MM-dd");
            _dictVehicel["Creater"] = _dr["Creater"];
            _dictVehicel["AuthorizeTo"] = _dr["AuthorizeTo"];
            //_dictVehicel["FromDepartment"] = _dr["FromDepartment"];
            _dictVehicel["AuthorizationTime"] = DateTime.Parse(_dr["AuthorizationTime"].ToString()).ToString("yyyy-MM-dd");
            _dictVehicel["InvalidTime"] = DateTime.Parse(_dr["InvalidTime"].ToString()).ToString("yyyy-MM-dd");
            _dictVehicel["Remark"] = _dr["Remark"];
            return _dictVehicel;
        }

        void IDraw.Submit()
        {

            switch (_currentOperate)
            {
                //添加
                case DataOper.Add:
                    string errorQ;
                    _draw.GetDataFromUI();
                    _store.AddAuthorization(_dictVehicel, out errorQ);
                    _draw.SwitchCtl(false);
                    if (errorQ == "")
                    {
                        Dictionary<string, object> dictConfig = new Dictionary<string, object>
                        {                  
                            { "EmployeeNo",GlobalVar.UserNo},
                            {"EmployeeName",GlobalVar.UserName},
                            {"OperTable","车型"},
                            { "AuthorizeTo",  _dictVehicel["AuthorizeTo"]},
                            {"VehicelType",   _dictVehicel["VehicelType"]},
                            {"VehicelConfig",   _dictVehicel["VehicelConfig"]},
                            {"VehicelStage",  _dictVehicel["VehicelStage"]}
                        };
                        Log.WriteLog(EnumLibrary.EnumLog.AddVehicel, dictConfig);
                        _draw.InitGrid();
                        ShowSelectedVehicel();
                    }
                    else
                    {
                        XtraMessageBox.Show(errorQ, "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    break;
                //更新
                case DataOper.Update:
                    _draw.GetDataFromUI();
                    string errorU;
                    _store.Update(EnumLibrary.EnumTable.Authorization, _dictVehicel, out errorU);
                    _draw.SwitchCtl(false);
                    if (errorU == "")
                    {
                     
                        _draw.InitGrid();
                        //cbDepartment.ReadOnly = false;
                        cbAuthTo.ReadOnly = false;
                    }
                    else
                    {
                        XtraMessageBox.Show(errorU, "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    break;
                //删除
                case DataOper.Del:
                    string error;
                    _store.Del(EnumLibrary.EnumTable.Authorization, UserDr(), out error);
                    if (error == "")
                    {
                        //Log.WriteLog(EnumLibrary.EnumLog.DelVehicelAuthorization, UserDict());
                        _draw.InitGrid();
                        TreeNodeMouseDown(vehicelTree.FocusedNode);
                        Dictionary<string, object> dictConfig = new Dictionary<string, object>
                        {
                            { "EmployeeNo",GlobalVar.UserNo},
                            {"EmployeeName",GlobalVar.UserName},
                            {"OperTable","车型"},
                             { "AuthorizeTo",  _dictVehicel["AuthorizeTo"]},
                            {"VehicelType",   _dictVehicel["VehicelType"]},
                            {"VehicelConfig",   _dictVehicel["VehicelConfig"]},
                            {"VehicelStage",  _dictVehicel["VehicelStage"]}
                        };
                        Log.WriteLog(EnumLibrary.EnumLog.DelVehicel, dictConfig);
                    }
                    else
                    {
                        XtraMessageBox.Show(error, "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    break;
            }
        }

        Dictionary<string, object> IDraw.GetDataFromUI()
        {
            _dictVehicel["VehicelType"] = cbvehicelType.Text;
            _dictVehicel["VehicelConfig"] = cbVehicelConfig.Text;
            _dictVehicel["VehicelStage"] = cbvehicelStage.Text;
            _dictVehicel["CreateTime"] = dateCreate.DateTime.ToString("yyyy-MM-dd");
            _dictVehicel["Creater"] = txtCreater.Text;
            _dictVehicel["AuthorizeTo"] = cbAuthTo.Text;
            //_dictVehicel["FromDepartment"] = cbDepartment.Text;
            _dictVehicel["AuthorizationTime"] = dateAuth.DateTime.ToString("yyyy-MM-dd");
            _dictVehicel["InvalidTime"] = dateInvalid.DateTime.ToString("yyyy-MM-dd");
            _dictVehicel["Remark"] = txtRemark.Text;
            return _dictVehicel;
        }

        void IDraw.SetDataToUI(DataRow selectedRow)
        {
            cbvehicelType.Text = selectedRow["VehicelType"].ToString();
            cbVehicelConfig.Text = selectedRow["VehicelConfig"].ToString();
            cbvehicelStage.Text = selectedRow["VehicelStage"].ToString();
            dateCreate.DateTime = DateTime.Parse(selectedRow["CreateTime"].ToString());
            txtCreater.Text = selectedRow["Creater"].ToString();
            cbAuthTo.Text = selectedRow["AuthorizeTo"].ToString();
            //cbDepartment.Text = selectedRow["FromDepartment"].ToString();
            dateAuth.DateTime = DateTime.Parse(selectedRow["AuthorizationTime"].ToString());
            dateInvalid.DateTime = DateTime.Parse(selectedRow["InvalidTime"].ToString());
            txtRemark.Text = selectedRow["Remark"].ToString();

            //获取修改前的值用作修改时的判断
            _dictVehicel["OldVehicelType"] = selectedRow["VehicelType"];
            _dictVehicel["OldVehicelConfig"] = selectedRow["VehicelConfig"];
            _dictVehicel["OldVehicelStage"] = selectedRow["VehicelStage"];
        }

        void IDraw.InitDict()
        {
            _dictVehicel.Add("VehicelType", "");
            _dictVehicel.Add("VehicelConfig", "");
            _dictVehicel.Add("VehicelStage", "");
            _dictVehicel.Add("CreateTime", "");
            _dictVehicel.Add("Creater", "");
            _dictVehicel.Add("AuthorizeTo", "");
            //_dictVehicel.Add("FromDepartment", "");
            _dictVehicel.Add("AuthorizationTime", "");
            _dictVehicel.Add("InvalidTime", "");
            _dictVehicel.Add("Remark", "");
            _dictVehicel.Add("ifVehicel", "");
        }

        bool ITree.SaveTreeList(string vehXmlPath)
        {
            if (!File.Exists(vehXmlPath)) return false;
            vehicelTree.ExportToXml(AppDomain.CurrentDomain.BaseDirectory + vehXmlPath);
            return true;
        }

        bool ITree.LoadTreeList(string vehXmlPath)
        {
            
            if (!File.Exists(vehXmlPath)) return false;
            vehicelTree.ImportFromXml(AppDomain.CurrentDomain.BaseDirectory + vehXmlPath);

            _tree.DrawTreeImage();
            return true;
        }

        void ITree.DrawTreeColor()
        {
            var controlColor = CommonSkins.GetSkin(UserLookAndFeel.Default).Colors.GetColor("Control");
            vehicelTree.Appearance.Empty.BackColor = controlColor;
            vehicelTree.Appearance.Row.BackColor = controlColor;
        }

        void ITree.ShowCmsByNode(TreeListNode node)
        {
            if (node == null)
            {
                vehicelTree.ContextMenuStrip = CMSCreate;
                return;
            }
            vehicelTree.FocusedNode = node;
            var level = node.Level;
            switch (level)
            {
                case 0:
                    vehicelTree.ContextMenuStrip = CMSVehicel;
                    break;
                case 1:
                    vehicelTree.ContextMenuStrip = CMSConfig;
                    break;
                case 2:
                    vehicelTree.ContextMenuStrip = CMSStage;
                    break;
            }
        }

        void ITree.StartEdit()
        {
            cmsTable = false;
            if (vehicelTree.FocusedNode == null) return;
            vehicelTree.OptionsBehavior.Editable = true;
            vehicelTree.VisibleColumns[0].OptionsColumn.AllowFocus = true;
            vehicelTree.FocusedColumn = vehicelTree.VisibleColumns[0];
            vehicelTree.ShowEditor();
        }

        void ITree.Create_Click(object sender, EventArgs e)
        {
            vehicelTree.ContextMenuStrip = null;
            var tsmi = sender as ToolStripMenuItem;
            if (tsmi == tsmiCreateVehicel)
            {
                node = AddNodeNameIndex(vehicelTree.Nodes, "NewVehicle");
                node.StateImageIndex = 0;
            }
            else if (tsmi == tsmiCreateConfig)
            {
                if (vehicelTree.FocusedNode == null) return;
                node = AddNodeNameIndex(vehicelTree.FocusedNode.Nodes, "NewTask");
                node.StateImageIndex = 1;
            }
            else if (tsmi == tsmiCreateStage)
            {
                if (vehicelTree.FocusedNode == null) return;
                node = AddNodeNameIndex(vehicelTree.FocusedNode.Nodes, "NewStage");
                node.StateImageIndex = 2;
            }
            vehicelTree.FocusedNode = node;
            _tree.StartEdit();
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

        void ITree.ReName_Click(object sender, EventArgs e)
        {
            _tree.StartEdit();
        }

        void ITree.Del_Click(object sender, EventArgs e)
        {
            var tsmi = sender as ToolStripMenuItem;
            TreeListNode node = null;
            List<string> VehicelTemp = new List<string>();
            string error = "";
            if (tsmi == tsmiDelVehicel)//删除车型
            {
                node = vehicelTree.FocusedNode;
                _dictVehicel["ifVehicel"] = "VehicelType";
                _dictVehicel["VehicelType"] = node.GetDisplayText("colName");
                IList<object[]> listDbc = _store.GetSpecialByEnum(EnumLibrary.EnumTable.DBC, _dictVehicel);
                IList<object[]> list = _store.GetSpecialByEnum(EnumLibrary.EnumTable.FileLinkByVehicelCount, _dictVehicel);
                if (listDbc.Count != 0 || list.Count != 0)
                {
                    if (role == "superadminister")
                    {
                        //Show(DLAF.LookAndFeel, this, "该车型已经被配置过了,不能删除，如需删除请先删除授权表中的该车型的所有阶段信息再来删除", "", new[] { DialogResult.OK }, null, 0, MessageBoxIcon.Information);
                        if (XtraMessageBox.Show("该车型的配置信息、拓扑图信息、和任务列表中的记录也会被删除，确定删除当前选中记录么？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) ==
                       DialogResult.OK)
                        {
                            if (XtraMessageBox.Show("该车型的配置信息、拓扑图信息、和任务列表中的记录也会被删除，请再次确定删除当前选中记录么？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) ==
                            DialogResult.OK)
                            { 
                                _dictVehicel["Type"] = "VehicelType";
                                deleteCommon();
                            }
                        }
                    }
                    else
                    {
                        Show(DLAF.LookAndFeel, this, "该车型已经被配置过了,不能删除，如需删除请联系超级管理员", "", new[] { DialogResult.OK }, null, 0, MessageBoxIcon.Information);
                        return;
                    }
                }
            }
            else if (tsmi == tsmiDelConfig)//删除配置
            {
                if (vehicelTree.FocusedNode == null) return;
                node = vehicelTree.FocusedNode;
                VehicelTemp = GetCurrent2VNode(node);
                _dictVehicel["ifVehicel"] = "VehicelConfig";
                _dictVehicel["VehicelType"] = VehicelTemp[0];
                _dictVehicel["VehicelConfig"] = VehicelTemp[1];
                IList<object[]> listDbc = _store.GetSpecialByEnum(EnumLibrary.EnumTable.DBC, _dictVehicel);
                IList<object[]> list = _store.GetSpecialByEnum(EnumLibrary.EnumTable.FileLinkByVehicelCount, _dictVehicel);
                if (listDbc.Count != 0 || list.Count != 0)
                {
                    if (role == "superadminister")
                    {
                        if (XtraMessageBox.Show("该配置下的阶段信息、授权信息、拓扑图信息、和任务列表中的记录也会被删除，确定删除当前选中记录么？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) ==
                      DialogResult.OK)
                        {
                            if (XtraMessageBox.Show("该配置下的阶段信息、授权信息、拓扑图信息、和任务列表中的记录也会被删除，请再次确定删除当前选中记录么？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) ==
                            DialogResult.OK)
                            {
                                //Show(DLAF.LookAndFeel, this, "该配置已经被配置过了,不能删除，如需删除请先删除授权表中的该配置下的所有阶段信息再来删除", "", new[] { DialogResult.OK }, null, 0, MessageBoxIcon.Information);
                                _dictVehicel["Type"] = "VehicelConfig";
                                deleteCommon();
                            }
                        }
                    }
                    else
                    {
                        Show(DLAF.LookAndFeel, this, "该配置已经被配置过了,不能删除，如需删除请联系超级管理员", "", new[] { DialogResult.OK }, null, 0, MessageBoxIcon.Information);
                        return;
                    }
                }
            }
            else if (tsmi == tsmiDelStage)//删除节点
            {
                if (vehicelTree.FocusedNode == null) return;
                node = vehicelTree.FocusedNode;
                VehicelTemp = GetCurrentVNode(node);
                _dictVehicel["ifVehicel"] = "";
                _dictVehicel["VehicelType"] = VehicelTemp[0];
                _dictVehicel["VehicelConfig"] = VehicelTemp[1];
                _dictVehicel["VehicelStage"] = VehicelTemp[2];
                IList<object[]> listDbc = _store.GetSpecialByEnum(EnumLibrary.EnumTable.DBC, _dictVehicel);
                IList<object[]> list = _store.GetSpecialByEnum(EnumLibrary.EnumTable.FileLinkByVehicelCount, _dictVehicel);
                if (listDbc.Count != 0 || list.Count != 0)
                {
                    if (role == "superadminister")
                    {
                        if (XtraMessageBox.Show("该阶段下的阶段信息、授权信息、拓扑图信息、和任务列表中的记录也会被删除，确定删除当前选中记录么？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) ==
                     DialogResult.OK)
                        {
                            if (XtraMessageBox.Show("该阶段下的阶段信息、授权信息、拓扑图信息、和任务列表中的记录也会被删除，请再次确定删除当前选中记录么？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) ==
                            DialogResult.OK)
                            {
                                _dictVehicel["Type"] = "VehicelStage";
                                deleteCommon();
                                // Show(DLAF.LookAndFeel, this, "该阶段已经被配置过了,不能删除，如需删除请先删除授权表中的该阶段信息再来删除", "", new[] { DialogResult.OK }, null, 0, MessageBoxIcon.Information);
                                // return;
                            }
                        }
                    }
                    else
                    {
                        Show(DLAF.LookAndFeel, this, "该阶段已经被配置过了,不能删除，如需删除请联系超级管理员", "", new[] { DialogResult.OK }, null, 0, MessageBoxIcon.Information);
                        return;
                    }
                } 
            }
            _store.Del(EnumLibrary.EnumTable.Authorization, _dictVehicel, out error);
            _store.Del(EnumLibrary.EnumTable.Topology, _dictVehicel, out error);
            if (error == "")
            {
                //Log.WriteLog(EnumLibrary.EnumLog.DelVehicelAuthorization, UserDict());
                vehicelTree.DeleteSelectedNodes();
                _tree.SaveTreeList(GlobalVar.TreeXmlPath);
                //_draw.InitGrid();
                GetAllNodesByRank();
                GlobalVar.CurrentVNode.Clear();
                GlobalVar.CurrentVNode.Add(@"当前节点：未选中");
                bool isSuccess = NodeStr.SetVNode(GlobalVar.CurrentVNode);
                if (isSuccess)
                {
                    pictureEdit1.Image = null;
                    pictureEdit2.Image = null;
                    pictureEdit3.Image = null;
                }
                _isHaveCurrentNode = false;
            }
            else
            {
                XtraMessageBox.Show(error, "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            _dictVehicel["ifVehicel"] = "";
            TreeNodeMouseDown(vehicelTree.FocusedNode);
        }
        private void deleteCommon()//delete
        {
            string error = "";
            _store.Del(EnumLibrary.EnumTable.Authorization, _dictVehicel, out error);
            _draw.InitGrid();
            //第二：删除DBC相关
            //查找此车型下所有相关dbc(1；删除数据库的信息2；删除本地的文件信息)

            IList<object[]> ALLdbc = DataComparisonSelect(_dictVehicel);
            int dd = 0;
            for (int j = 0; j < ALLdbc.Count; j++)
            {
                dd++;
                File.Delete(AppDomain.CurrentDomain.BaseDirectory + ALLdbc[j][5].ToString().Trim());//删除存在本地项目下的拓扑图

                Dictionary<string, object> _DBCVehicel = new Dictionary<string, object> {
                                { "VehicelType",ALLdbc[j][0]},
                                { "VehicelConfig",ALLdbc[j][1]},
                                { "VehicelStage",ALLdbc[j][2]},
                                { "DBCName",ALLdbc[j][3]},
                                { "BelongCAN",ALLdbc[j][4]},
                                { "DBCContent",ALLdbc[j][5]},
                                { "ImportUser",ALLdbc[j][6]},
                                { "ImportTime",ALLdbc[j][7]},
                                { "FormerDBCName",ALLdbc[j][8]},
                                { "CANType",ALLdbc[j][9]},
                                { "MatchSort",""},
                            };
                //删除数据库DBC
                _store.Del(EnumLibrary.EnumTable.DBC, _DBCVehicel, out error);
                ////删除数据库拓扑图
                _store.Del(EnumLibrary.EnumTable.Topology, _DBCVehicel, out error);

                string[] busType =
                    _file.DBCCANConvert(ALLdbc[j][4].ToString().Substring(0, 3)).Split('&');

                foreach (var matchSort in busType)
                {
                    _DBCVehicel["MatchSort"] = matchSort;
                    //删除配置表
                    _store.Del(EnumLibrary.EnumTable.FileLinkByVehicelColEml, _DBCVehicel, out error);
                }

                string[] taskType =
                     _file.DBCCANConvertTestType(ALLdbc[j][4].ToString().Substring(0, 3)).Split('&');
                Dictionary<string, object> taskNoDict = new Dictionary<string, object>();
                taskNoDict["TaskNo"] = ALLdbc[j][0] + "-" + ALLdbc[j][1] + "-" +
                                       ALLdbc[j][2];

                foreach (var matchSort in taskType)
                {
                    taskNoDict["TaskName"] = matchSort;
                    //删除任务列表
                    _store.Del(EnumLibrary.EnumTable.TaskTable, taskNoDict, out error);
                }
                //操作的信息写入日志
                Dictionary<string, object> dictConfig1 = new Dictionary<string, object>
                                {
                                    {"EmployeeNo",GlobalVar.UserNo},
                                    {"EmployeeName",GlobalVar.UserName},
                                    {"OperTable","DBC"},
                                    {"DBCName",    ALLdbc[j][3]},
                                    {"BelongCAN",  ALLdbc[j][4]},
                                };
                Log.WriteLog(EnumLibrary.EnumLog.DelDBC, dictConfig1);
                File.Delete(AppDomain.CurrentDomain.BaseDirectory + ALLdbc[j][5]);
            }
            //查找此车型下所有相关拓扑图(1；删除数据库的信息2；删除本地的文件信息)

            IList<object[]> ALLTopology = SelectALLTopologyByVehiceType(_dictVehicel);
            for (int i = 0; i < ALLTopology.Count; i++)
            {
                File.Delete(AppDomain.CurrentDomain.BaseDirectory + ALLTopology[i][3].ToString().Trim());//删除存在本地项目下的拓扑图
            }
        }
        //根据车型查找此车型下的所有DBC
        private IList<object[]> DataComparisonSelect(Dictionary<string, object> _dictDBC)
        {
            IList<object[]> dataList = _store.GetSpecialByEnum(EnumLibrary.EnumTable.DBCInformation, _dictDBC);
            return dataList;
        }
        //根据车型查找此车型下的所有拓扑图
        private IList<object[]> SelectALLTopologyByVehiceType(Dictionary<string, object> _dictDBC)
        {
            IList<object[]> dataList = _store.GetSpecialByEnum(EnumLibrary.EnumTable.TopologySelect, _dictDBC);
            return dataList;
        }

        #region 获得节点数组
        /// <summary>
        /// 
        /// </summary>
        private void GetAllNodesByRank()//考虑递归版本是否更好
        {
            cbvehicelType.Properties.Items.Clear();
            _rootNodes.Clear();
            _secondaryNodes.Clear();
            _finalNodes.Clear();
            var treeList = vehicelTree.Nodes.ToList();

            for (var i = 0; i < treeList.Count; i++)//获得一级节点集合
            {
                _rootNodes.Add(treeList[i].GetDisplayText("colName"));
                var secondaryNodesCache = new List<string>();
                var finalNodeCacheF = new List<List<string>>();
                for (var j = 0; j < treeList[i].Nodes.Count; j++) //获得二级节点集合
                {
                    secondaryNodesCache.Add(treeList[i].Nodes[j].GetDisplayText("colName"));
                    var finalNodeCache = new List<string>();
                    for (var k = 0; k < treeList[i].Nodes[j].Nodes.Count; k++) //获得三级节点集合
                    {
                        finalNodeCache.Add(treeList[i].Nodes[j].Nodes[k].GetDisplayText("colName"));
                    }
                    finalNodeCacheF.Add(finalNodeCache);
                }
                _secondaryNodes.Add(secondaryNodesCache);
                _finalNodes.Add(finalNodeCacheF);
            }
            //像车型下拉框赋值
            cbvehicelType.Properties.Items.AddRange(_rootNodes);
        }

        #endregion
        
        #region 获得当前车型节点集合

        /// <summary>
        /// 获得当前车型节点集合
        /// </summary>
        /// <param name="node">当前节点</param>
        /// <returns></returns>
        private static List<string> GetCurrentVNode(TreeListNode node)
        {
            List<string> nodes = new List<string>
            {
                node.ParentNode.ParentNode.GetDisplayText("colName"),
                node.ParentNode.GetDisplayText("colName"),
                node.GetDisplayText("colName")
            };
            return nodes;
        }


        private static List<string> GetCurrent2VNode(TreeListNode node)
        {
            List<string> nodes = new List<string>
            {
                node.ParentNode.GetDisplayText("colName"),
                node.GetDisplayText("colName")
            };
            return nodes;
        }

        #endregion

        #region 绑定事件
        void ITree.BingEvent()
        {
            pictFirst.MouseClick += PictMouse_Click;
            pictSecond.MouseClick += PictMouse_Click;

            tsmiReNameConfig.Click += _tree.ReName_Click;
            tsmiReNameVehicel.Click += _tree.ReName_Click;

            tsmiDelStage.Click += _tree.Del_Click;
            tsmiDelConfig.Click += _tree.Del_Click;
            tsmiDelVehicel.Click += _tree.Del_Click;

            tsmiCreateVehicel.Click += _tree.Create_Click;
            tsmiCreateConfig.Click += _tree.Create_Click;
            tsmiCreateStage.Click += _tree.Create_Click;
        }
        #endregion

        #region 按钮事件
        private void PictMouse_Click(object sender, EventArgs e)
        {
            var pl = sender as PictureEdit;
            if (pl == null) return;
            //if (GlobalVar.CurrentVNode.Count < 3)
            //{
            //    Show(DLAF.LookAndFeel, this, "请选择好左侧车型列表中的车型-配置-阶段...", "", new[] { DialogResult.OK }, null, 0, MessageBoxIcon.Information);
            //    return;
            //}
            if (!_isHaveCurrentNode)
            {
                Show(DLAF.LookAndFeel, this, "请选择好左侧车型列表中的车型-配置-阶段...", "", new[] { DialogResult.OK }, null, 0, MessageBoxIcon.Information);
                return;
            }
            Dictionary<string, object> dictNode = new Dictionary<string, object>();
            dictNode.Add("VehicelType", GlobalVar.CurrentVNode[0]);
            dictNode.Add("VehicelConfig", GlobalVar.CurrentVNode[1]);
            dictNode.Add("VehicelStage", GlobalVar.CurrentVNode[2]);

            //IList<object[]> authList = _store.GetSpecialByEnum(EnumLibrary.EnumTable.Authorization_Auth, dictNode);
            DateTime nowTime = DateTime.Now;
            
            if (pl == pictFirst)
            {
                if (_pictCache != null)
                    Recognize();
                if (!_isHaveCurrentNode)
                {
                    Show(DLAF.LookAndFeel, this, "请选择好左侧车型列表中的车型-配置-阶段...", "", new[] { DialogResult.OK }, null, 0, MessageBoxIcon.Information);
                    return;
                }
                IList<object[]> config = _store.GetSpecialByEnum(EnumLibrary.EnumTable.Authorization_Auth, dictNode);
                if (config.Count == 0)
                {
                    Show(DLAF.LookAndFeel, this, "请先对该车型授权...", "", new[] { DialogResult.OK }, null, 0, MessageBoxIcon.Information);
                    return;
                }

                if (role == "configurator")
                {
                    var i = config[0][5].ToString().Split('(');
                    List<string> nameList = new List<string>()
                        {
                            i[0],
                            i[1]
                        };
                    if (GlobalVar.UserName != nameList[0])
                    {
                        Show(DLAF.LookAndFeel, this, "您没有对此车型配置的权限...", "", new[] { DialogResult.OK }, null, 0, MessageBoxIcon.Information);
                        return;
                    }
                }
                if (DateTime.Compare(nowTime, Convert.ToDateTime(config[0][7].ToString())) < 0)
                {
                    XtraMessageBox.Show(DLAF.LookAndFeel, this, "当前日期不在该车型的授权时间内，目前还不能配置，如要修改日期请联系超级管理员...", "", new[] { DialogResult.OK }, null, 0, MessageBoxIcon.Information);
                    return;
                }
                if (DateTime.Compare(nowTime, Convert.ToDateTime(config[0][8].ToString())) > 0)
                {
                    XtraMessageBox.Show(DLAF.LookAndFeel, this, "当前日期已经超出该车型的授权失效时间，已不能配置，如要修改日期请联系超级管理员...", "", new[] { DialogResult.OK }, null, 0, MessageBoxIcon.Information);
                    return;
                }
                pictFirst.Image = Resources.first_active;
                XtraForm upDBC = new UploadDBCAndTply();
                upDBC.ShowDialog();
                _pictCache = pictFirst;
                if (GlobalVar.UpDbcOk)
                {
                    pictureEdit1.Image = Resources.able;
                    GlobalVar.UpDbcOk = false;
                }
                else
                    pictureEdit1.Image = Resources.unable;

                IList<object[]> listFile = _store.GetSpecialByEnum(EnumLibrary.EnumTable.FileLinkByVehicel, dictNode);
                IList<object[]> dbcList = _store.GetSpecialByEnum(EnumLibrary.EnumTable.DbcCheck, dictNode);
                var tplyList = _store.GetSpecialByEnum(EnumLibrary.EnumTable.Topology, dictNode);
                if (dbcList.Count ==0)
                    pictureEdit1.Image = Resources.unable;
                else
                    pictureEdit1.Image = Resources.able;
                bool isCongig = false;
                if (tplyList.Count != 0)
                {
                    foreach (var item in tplyList)
                    {
                        if (item[3] == null || item[3].ToString() == "" || item[3].ToString() == "System.Byte[]")
                            pictureEdit2.Image = Resources.unable;
                        else if (item[3] != null && item[3].ToString() != "" && item[3].ToString() != "System.Byte[]")
                        {
                            pictureEdit2.Image = Resources.able;
                            isCongig = true;
                        }
                        if (isCongig)
                            break;
                    }
                }
                else
                {
                    pictureEdit2.Image = Resources.unable;
                }
            }
            else if (pl == pictSecond)
            {
                if (_pictCache != null)
                    Recognize();
                if (!_isHaveCurrentNode)
                {
                    Show(DLAF.LookAndFeel, this, "请选择好左侧车型列表中的车型-配置-阶段...", "", new[] { DialogResult.OK }, null, 0, MessageBoxIcon.Information);
                    return;
                }
                IList<object[]> dbcList = _store.GetSpecialByEnum(EnumLibrary.EnumTable.DbcCheck, dictNode);
                IList<object[]> config = _store.GetSpecialByEnum(EnumLibrary.EnumTable.Authorization_Auth, dictNode);
                //var tplyList = _store.GetSpecialByEnum(EnumLibrary.EnumTable.Topology, dictNode);
                if (config.Count == 0)
                {
                    Show(DLAF.LookAndFeel, this, "请先对该车型授权...", "", new[] { DialogResult.OK }, null, 0, MessageBoxIcon.Information);
                    return;
                }

                if (role == "configurator")
                {
                    var i = config[0][5].ToString().Split('(');
                    List<string> nameList = new List<string>()
                    {
                        i[0],
                        i[1]
                    };
                    if (GlobalVar.UserName != nameList[0])
                    {
                        Show(DLAF.LookAndFeel, this, "您没有权限对此车型配置...", "", new[] {DialogResult.OK}, null, 0,
                            MessageBoxIcon.Information);
                        return;
                    }
                }

                if (DateTime.Compare(nowTime, Convert.ToDateTime(config[0][7].ToString())) < 0)
                {
                    XtraMessageBox.Show(DLAF.LookAndFeel, this, "当前日期不在该车型的授权时间内，目前还不能配置，如要修改日期请联系超级管理员...", "", new[] { DialogResult.OK }, null, 0, MessageBoxIcon.Information);
                    return;
                }
                if (DateTime.Compare(nowTime, Convert.ToDateTime(config[0][8].ToString())) > 0)
                {
                    XtraMessageBox.Show(DLAF.LookAndFeel, this, "当前日期已经超出该车型的授权失效时间，已不能配置，如要修改日期请联系超级管理员...", "", new[] { DialogResult.OK }, null, 0, MessageBoxIcon.Information);
                    return;
                }
                //if (tplyList.Count == 0)
                //{
                //    Show(DLAF.LookAndFeel, this, "请先完成第二步车型拓扑图上传工作...", "", new[] { DialogResult.OK }, null, 0, MessageBoxIcon.Information);
                //    return;
                //}

                pictSecond.Image = Resources.second_active;
                FileEditor.pubClass.GlobalVar.IsIndependent = false;
                FileEditor.pubClass.GlobalVar.VNode = GlobalVar.CurrentVNode;
                FileEditor.pubClass.GlobalVar.IsEmpDisable = true;
                var node = GlobalVar.CurrentVNode;
                string VehTemp = node[0] + "-" + node[1] + "-" + node[2];
                XtraForm editor = new Editor(false);
                editor.ShowDialog();
                _pictCache = pictSecond;
                Dictionary<string, object> dictCNode = new Dictionary<string, object>();
                dictCNode.Add("VehicelType", GlobalVar.CurrentVNode[0]);
                dictCNode.Add("VehicelConfig", GlobalVar.CurrentVNode[1]);
                dictCNode.Add("VehicelStage", GlobalVar.CurrentVNode[2]);
                IList<object[]> listFile = _store.GetSpecialByEnum(EnumLibrary.EnumTable.FileLinkByVehicel, dictCNode);
                bool isCongig = false;
                foreach (var item in listFile)
                {
                    if (item[7] == null || item[7].ToString() == "" || item[7].ToString() == "System.Byte[]")
                        pictureEdit3.Image = Resources.unable;
                    else if (item[7] != null && item[7].ToString() != "" && item[7].ToString() != "System.Byte[]")
                    {
                        pictureEdit3.Image = Resources.able;
                        isCongig = true;
                    }
                    if (isCongig)
                        break;
                }
            }
        }

        #endregion

        void IDraw.SwitchCtl(bool isSwitch)
        {
            switch (isSwitch)
            {
                case true:
                    btnSubmit.Enabled = true;
                    dpVehicel.Visibility = DockVisibility.Visible;
                    break;
                case false:
                    btnSubmit.Enabled = false;
                    dpVehicel.Visibility = DockVisibility.Hidden;
                    break;
            }
        }

        private void gvVechiel_MouseDown(object sender, MouseEventArgs e)
        {
            _dr = null;
            _draw.SwitchCtl(false);
            //获得光标位置
            var hi = gvVechiel.CalcHitInfo(e.Location);
            //判断位置是否在行位置上
            if (!hi.InRow && !hi.InRowCell)
            {
                _dr = null;
                gcVechiel.ContextMenuStrip = null;
                return;
            }

            if (hi.RowHandle < 0)
            {
                _dr = null;
                gcVechiel.ContextMenuStrip = null;
                return;
            }
            //取一行值
            gvVechiel.SelectRow(hi.RowHandle);
            _dr = this.gvVechiel.GetDataRow(hi.RowHandle);
            drLinkTree();
        }

        private void drLinkTree()
        {
            if (_dr != null)
            {
                gcVechiel.ContextMenuStrip = CMSTable;
                GlobalVar.CurrentVNode.Clear();
                GlobalVar.CurrentVNode =
                    new List<string>
                    {
                        _dr["VehicelType"].ToString(),
                        _dr["VehicelConfig"].ToString(),
                        _dr["VehicelStage"].ToString()
                    };
                bool isSuccess = NodeStr.SetVNode(GlobalVar.CurrentVNode);
                tabAuthorize.TabPages[0].Text = "[" + GlobalVar.CurrentVNode[0] + "-" + GlobalVar.CurrentVNode[1] + "-" +
                                                GlobalVar.CurrentVNode[2] + "] 授权";
                tabAuthorize.TabPages[1].Text = "[" + GlobalVar.CurrentVNode[0] + "-" + GlobalVar.CurrentVNode[1] + "-" +
                                                GlobalVar.CurrentVNode[2] + "] 配置";
                IsConfig(GlobalVar.CurrentVNode);
                if (isSuccess)
                    _isHaveCurrentNode = true;
                pageSet.PageEnabled = true;
                ExpandTree();
            }
        }

        private void Recognize()
        {
            if (_pictCache == pictFirst)
                pictFirst.Image = Resources.first;
            if (_pictCache == pictSecond)
                pictSecond.Image = Resources.second;
        }

        List<string> ITree.GetCurrentNode(TreeListNode node)
        {
            throw new NotImplementedException();
        }

        private void vehicelTree_HiddenEditor(object sender, EventArgs e)
        {
            vehicelTree.OptionsBehavior.Editable = false;
            vehicelTree.VisibleColumns[0].OptionsColumn.AllowFocus = false;
            vehicelTree.ClearFocusedColumn();

            #region 判断树内创建节点时是否有相同名称节点

            string NodeName = vehicelTree.FocusedNode.GetDisplayText("colName").Trim();
            if (!(Regex.IsMatch(NodeName, "^[\u4E00-\u9FA5A-Za-z0-9]+$")))
            {
                Show(DLAF.LookAndFeel, this, "请输入合法的字符", "", new[] { DialogResult.OK }, null, 0,
                            MessageBoxIcon.Information);
                _tree.StartEdit();

                return;
            }

            TreeListNodes Parnode;
            if (node.Level==0)
            {
                Parnode = vehicelTree.Nodes;
            }
            else
            {
                Parnode = node.ParentNode.Nodes;
            }
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
            GetAllNodesByRank();
            cmsTable = true;
        }

        void ITree.DrawTreeImage()
        {
            for (int i = 0; i < vehicelTree.Nodes.Count; i++)
            {
                vehicelTree.Nodes[i].StateImageIndex = 0;
                for (int k = 0; k < vehicelTree.Nodes[i].Nodes.Count; k++)
                {
                    vehicelTree.Nodes[i].Nodes[k].StateImageIndex = 1;
                    for (int j = 0; j < vehicelTree.Nodes[i].Nodes[k].Nodes.Count; j++)
                        vehicelTree.Nodes[i].Nodes[k].Nodes[j].StateImageIndex = 2;
                }
            }
        }

        private void tsmiAdd_Click(object sender, EventArgs e)
        {
            VehicelAdd();
        }

        private void VehicelAdd()
        {
            Dictionary<string, object> Vehicel = new Dictionary<string, object>();
            Vehicel.Add("level", "level3");
            Vehicel.Add("VehicelType", GlobalVar.CurrentVNode[0]);
            Vehicel.Add("VehicelConfig", GlobalVar.CurrentVNode[1]);
            Vehicel.Add("VehicelStage", GlobalVar.CurrentVNode[2]);
            var VehList = _store.GetSpecialByEnum(EnumLibrary.EnumTable.Authorization_Table, Vehicel);
            if (VehList.Count != 0)
            {
                Show(DLAF.LookAndFeel, this, "该车型-配置-阶段已经被授权，无法重复授权...", "", new[] { DialogResult.OK }, null, 0, MessageBoxIcon.Information);
                return;
            }
            _currentOperate = DataOper.Add;
            _draw.SwitchCtl(true);

            ClearDataToUI();
            cbvehicelType.Text = GlobalVar.CurrentVNode[0];
            cbVehicelConfig.Text = GlobalVar.CurrentVNode[1];
            cbvehicelStage.Text = GlobalVar.CurrentVNode[2];
            dateCreate.DateTime = DateTime.Now;
            //txtCreater.SelectedIndex = 0;
            txtCreater.Text = GlobalVar.UserName;
            dateAuth.DateTime = DateTime.Now;
            dateInvalid.DateTime = DateTime.Now.AddYears(20);
        }

        private void tsmiUpdate_Click(object sender, EventArgs e)
        {
            _currentOperate = DataOper.Update;
            Operate = "修改";
            if (_dr != null && role != "superadminister")
            {
                if (IsConfig(_dr,Operate))
                    return;
                _draw.SetDataToUI(_dr);
                _draw.SwitchCtl(true);
                strAuthorizeTo = cbAuthTo.Text;
                
            }
            else if (_dr != null && role == "superadminister")
            {
                _draw.SetDataToUI(_dr);
                _draw.SwitchCtl(true);
                strAuthorizeTo = cbAuthTo.Text;
                //cbDepartment.ReadOnly = true;
                cbAuthTo.ReadOnly = true;
            }
        }

        private void tsmiDel_Click(object sender, EventArgs e)
        {
            Operate = "删除";
            if (_dr != null && role != "superadminister")
            {
                _currentOperate = DataOper.Del;
                if (IsConfig(_dr, Operate))
                    return;
                _draw.Submit();
                _dr = null;
            }
            else if (_dr != null && role == "superadminister")
            {
                if (
                    XtraMessageBox.Show("确认要删除当前选中行的数据吗？", "提示", MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Warning) ==
                    DialogResult.OK)
                {
                    if (
                        XtraMessageBox.Show("再次确认要删除当前选中行的数据吗？", "提示", MessageBoxButtons.OKCancel,
                            MessageBoxIcon.Warning) ==
                        DialogResult.OK)
                    {
                        _currentOperate = DataOper.Del;
                        _draw.Submit(); string error; 
                        Dictionary<string,object> taskNoDict = new Dictionary<string, object>();
                        taskNoDict["TaskNo"] = _dictVehicel["VehicelType"] + "-" + _dictVehicel["VehicelConfig"] + "-" +
                                               _dictVehicel["VehicelStage"];
                        _store.Del(EnumLibrary.EnumTable.Report, taskNoDict, out error);
                        _store.Del(EnumLibrary.EnumTable.TaskDBC, taskNoDict, out error);
                        
                        _dr = null;
                        if (error == "")
                        {
                            //Log.WriteLog(EnumLibrary.EnumLog.DelVehicelAuthorization, UserDict());
                            _draw.InitGrid();
                        }
                    }
                 }
            }
        }

        private bool IsConfig(DataRow dr,string oper)
        {
            bool isExist = false;
            Dictionary<string,object> dictVehicel = new Dictionary<string, object>();
            dictVehicel["VehicelType"] = dr["VehicelType"];
            dictVehicel["VehicelConfig"] = dr["VehicelConfig"];
            dictVehicel["VehicelStage"] = dr["VehicelStage"];
            IList<object[]> listDbc = _store.GetSpecialByEnum(EnumLibrary.EnumTable.DBC, dictVehicel);
            IList<object[]> list = _store.GetSpecialByEnum(EnumLibrary.EnumTable.FileLinkByVehicel, dictVehicel);
            if (listDbc.Count != 0 || list.Count != 0)
            {
                Show(DLAF.LookAndFeel, this, "该车型-配置-阶段已经被配置过了，不能" + oper + "...", "", new[] { DialogResult.OK }, null, 0, MessageBoxIcon.Information);
                isExist = true;
            }
            return isExist;
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            GlobalVar.NumberChanges = 0;
            if (GlobalVar.CurrentVNode.Count == 0)
            {
                if(_currentOperate!=DataOper.Update)
                {
                    XtraMessageBox.Show("请从左侧车型列表处授权", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ClearDataToUI();
                    return;
                }
            }
            if (this.dateAuth.DateTime < this.dateCreate.DateTime)
            {
                XtraMessageBox.Show("授权时间不能在配置开始时间之后");
                return;
            }
            else if (this.dateInvalid.DateTime < this.dateAuth.DateTime)
            {
                XtraMessageBox.Show("配置开始时间不能在配置结束时间之后");
                return;
            }
            _draw.Submit();
            ClearDataToUI();
        }
        public static DialogResult Show(UserLookAndFeel look, IWin32Window owner, string text, string caption, DialogResult[] buttons, Icon icon, int defaultButton, MessageBoxIcon messageBeepSound)
        {
            XtraMessageBoxForm form = new XtraMessageBoxForm();
            Font defaultFont = new System.Drawing.Font("微软雅黑", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            form.Appearance.Font = defaultFont;
            return form.ShowMessageBoxDialog(new XtraMessageBoxArgs(look, owner, text, caption, buttons, icon, defaultButton));
        }

        private void gcVechiel_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if(e.Button==MouseButtons.Right) return;
            var hi = gvVechiel.CalcHitInfo(e.Location);
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
            _dr = this.gvVechiel.GetDataRow(rowCount);
            _currentOperate = DataOper.Update;
            Operate = "修改";
            if (_dr != null && role != "superadminister")
            {
                if (IsConfig(_dr, Operate))
                    return;
                _draw.SetDataToUI(_dr);
                _draw.SwitchCtl(true);
                strAuthorizeTo = cbAuthTo.Text;
            }
            else if (_dr != null && role == "superadminister")
            {
                _draw.SetDataToUI(_dr);
                _draw.SwitchCtl(true);
                strAuthorizeTo = cbAuthTo.Text;
                //cbDepartment.ReadOnly = true;
                cbAuthTo.ReadOnly = true;
            }
        }

        private void vehicelTree_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var node = vehicelTree.CalcHitInfo(new Point(e.X, e.Y)).Node;
            if (node == null)
            {
                return;
            }
            vehicelTree.FocusedNode = node;
            var level = node.Level;
            if (level == 2)
            {
                VehicelAdd();
            }
        }

        private void vehicelTree_FocusedNodeChanged(object sender, FocusedNodeChangedEventArgs e)
        {
            if (vehicelTree.FocusedNode != null)
            {
                TreeNodeMouseDown(vehicelTree.FocusedNode);
            }
        }


        private Dictionary<string, object> VehicelListToDict(List<string> VehicelList)
        {
            if (VehicelList.Count != 3) return null;
            Dictionary<string, object> DictVehicel = new Dictionary<string, object>();
            DictVehicel.Add("VehicelType", VehicelList[0]);
            DictVehicel.Add("VehicelConfig", VehicelList[1]);
            DictVehicel.Add("VehicelStage", VehicelList[2]);
            return DictVehicel;
        } 

        private void tsmiCopyStage_Click(object sender, EventArgs e)
        {
            if (vehicelTree.FocusedNode == null) return;
            tsmiPasteStage.Enabled = true;
            FocusedNodes = GetCurrentVNode(vehicelTree.FocusedNode);
        }

        private void tsmiPasteStage_Click(object sender, EventArgs e)
        {
            try
            {
                Dictionary<string, object> oldVehicel = new Dictionary<string, object>();
                Dictionary<string, object> newVehicel = new Dictionary<string, object>();
                if (vehicelTree.FocusedNode == null) return;
                List<string> NewNodes = GetCurrent2VNode(vehicelTree.FocusedNode);
                List<string> Nodes = new List<string>();
                foreach (TreeListNode fnode in vehicelTree.FocusedNode.Nodes)
                {
                    Nodes.Add(fnode.GetDisplayText("colName"));
                }
                ReName rn=new ReName(NewNodes,Nodes);
                rn.ShowDialog();
                if (rn.DialogResult == DialogResult.OK)
                {
                    NewNodes.Clear();
                    NewNodes = GlobalVar.RenameList;
                    node = vehicelTree.FocusedNode.Nodes.Add(new object[] { NewNodes[2] });
                    node.StateImageIndex = 2;
                    _tree.SaveTreeList(GlobalVar.TreeXmlPath);
                    GetAllNodesByRank();
                    GlobalVar.CurrentVNode = NewNodes;
                    ExpandTree();
                    oldVehicel = VehicelListToDict(FocusedNodes);
                    newVehicel = VehicelListToDict(NewNodes);
                    _copypaste.SearchCopyVehicelByNode(oldVehicel);
                    Dictionary<string,int> dictfail = new Dictionary<string, int>();
                    dictfail = _copypaste.PasteVehicelInfoByNode(newVehicel,oldVehicel);
                    string warning = "";
                    if(dictfail.Keys.Contains("Authorization"))
                    {
                        warning = String.Format("车型授权表中有" + dictfail["Authorization"] +"项粘贴失败");
                    }
                    if (dictfail.Keys.Contains("DBC"))
                    {
                        if (string.IsNullOrEmpty(warning))
                        {
                            warning = String.Format("DBC表中有" + dictfail["DBC"] + "项粘贴失败");
                        }
                        else
                        {
                            warning = String.Format("，DBC表中有" + dictfail["DBC"] + "项粘贴失败");
                        }
                    }
                    if (dictfail.Keys.Contains("FileLinkByVehicel"))
                    {
                        if (string.IsNullOrEmpty(warning))
                        {
                            warning = String.Format("FileLinkByVehicel表中有" + dictfail["FileLinkByVehicel"] + "项粘贴失败");
                        }
                        else
                        {
                            warning = String.Format("，FileLinkByVehicel表中有" + dictfail["FileLinkByVehicel"] + "项粘贴失败");
                        }
                    }
                    if (!string.IsNullOrEmpty(warning))
                    {
                        XtraMessageBox.Show(warning);
                    }
                    Dictionary<string, object> dictConfig = new Dictionary<string, object>
                    {
                        { "EmployeeNo",GlobalVar.UserNo},
                        {"EmployeeName",GlobalVar.UserName},
                        {"OperTable","复制粘贴"},
                        {"oldVehicelType", oldVehicel["VehicelType"]},
                        {"oldVehicelConfig",   oldVehicel["VehicelConfig"]},
                        {"oldVehicelStage",   oldVehicel["VehicelStage"]},
                        {"newVehicelType",   newVehicel["VehicelType"]},
                        {"newVehicelConfig",   newVehicel["VehicelConfig"]}
                    };
                    Log.WriteLog(EnumLibrary.EnumLog.Copy, dictConfig);
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("出错了，请先查看本地相关的拓扑图文件和DBC或LDF文件是否存在，不是文件问题的话请联系工程师解决");
            }
        }
        private void ShieldRight()
        {
            ContextMenu emptyMenu = new ContextMenu();
            txtCreater.Properties.ContextMenu = emptyMenu;
            txtRemark.Properties.ContextMenu = emptyMenu;
        }

        private void tsmiView_Click(object sender, EventArgs e)
        {
            _draw.InitGrid();
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

        private void txtRemark_MouseUp(object sender, MouseEventArgs e)
        {
            GlobalVar.NumberChanges = 1;
        }
    }
}