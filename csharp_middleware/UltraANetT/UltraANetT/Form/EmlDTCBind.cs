using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraBars;
using ProcessEngine;
using DevExpress.XtraEditors;

namespace UltraANetT.Form
{
    public partial class EmlDTCBind : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        private string _selectedName = string.Empty;
        private string _exapID = string.Empty;
        private Dictionary<string, string> _dictVNode = new Dictionary<string, string>();
        private string role;
        private object[] _dtcInfoArray;
        private Dictionary<string, string> _dictDTC;
        private readonly ProcStore _store = new ProcStore();
        private readonly SearchDTCByExaModule _searchDtc = new SearchDTCByExaModule();
        private LogicalControl _LogC = new LogicalControl();
        public EmlDTCBind()
        {
            InitializeComponent();
        }

        public EmlDTCBind(Dictionary<string, string> dictVNode, string strExapID)
        {
            InitializeComponent();
            
            _dictVNode = dictVNode;
            _selectedName = _dictVNode["TaskName"] + "用例表";
            _exapID = strExapID;
            this.Text = _exapID + @" DTC绑定";
            SelectDTCInfo();
            BindListBox();
            role = _LogC.RoleSelect(GlobalVar.UserName);
            RoleFunction(role);
        }

        private void BindListBox()
        {
            List<string> listBind = new List<string>();
            List<string> listNoBind = new List<string>();
            if (_dictDTC.ContainsKey(_exapID))
            {
                string[] strBindArray = _dictDTC[_exapID].Split(',');
                listBind.AddRange(strBindArray);
                listBind.Remove(string.Empty);
            }
            foreach (var dtcInfo in _dtcInfoArray)
            {
                if (!listBind.Contains(dtcInfo.ToString()))
                {
                    listNoBind.Add(dtcInfo.ToString());
                }
            }
            lbcNoBindDTC.Items.Clear();
            lbcBindDTC.Items.Clear();
            lbcNoBindDTC.Items.AddRange(listNoBind.ToArray());
            lbcBindDTC.Items.AddRange(listBind.ToArray());
        }

        private void SaveToDb()
        {
            Dictionary<string, object> dictEml = _dictVNode.ToDictionary(t => t.Key, t => (object)t.Value);
            string strDTCInfo = string.Empty;
            foreach (var item in lbcBindDTC.Items)
            {
                strDTCInfo += item.ToString() + ",";
            }
            strDTCInfo = strDTCInfo.Length > 0 ? strDTCInfo.Substring(0, strDTCInfo.Length - 1) : string.Empty;
            _dictDTC[_exapID] = strDTCInfo;
            dictEml["ContainExmp"] = Json.SerJson(_dictDTC);//任务表此列曾经是保存二次编辑的用例信息，现在无需编辑二次用例后，此处保存绑定的DTC信息
            string error = string.Empty;
            _store.Update(EnumLibrary.EnumTable.TaskDTC, dictEml, out error);
            if (error != string.Empty)
            {
                XtraMessageBox.Show("未知错误，修改失败！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void SelectDTCInfo()
        {
            _dtcInfoArray = GetDTCArray();
            var dictVNode = _dictVNode.ToDictionary(t => t.Key, t => (object)t.Value);
            var listDTCInfo = _store.GetSingnalColByCon(EnumLibrary.EnumTable.DTC, dictVNode, 11);
            var dtcJson = listDTCInfo.Count > 0 ? listDTCInfo[0] : string.Empty;
            _dictDTC = Json.DerJsonToDict(dtcJson) == null
                ? new Dictionary<string, string>()
                : Json.DerJsonToDict(dtcJson);
        }
        private void RoleFunction(string role)
        {
            switch (role)
            {
                case "administer":
                    break;
                case "configurator":
                    break;
                case "tester":
                    break;
                default:
                    break;
            }
        }
        private object[] GetDTCArray()
        {
            Dictionary<string, object> dictCNode = DictConvert(_dictVNode).ToDictionary(t => t.Key, t => (object)t.Value);
            List<Dictionary<string, string>> listConfig = new List<Dictionary<string, string>>();
            IList<object[]> listFilecfg = _store.GetSpecialByEnum(EnumLibrary.EnumTable.FileLinkByVehicelRep,
                dictCNode);
            foreach (var cfglist in listFilecfg)
            {
                if (!string.IsNullOrWhiteSpace(cfglist[7].ToString()))
                {
                    listConfig = Json.DerJsonToLDict(cfglist[7].ToString());
                    break;
                }
            }
            Dictionary<string, string> dictDtc = new Dictionary<string, string>();
            dictDtc = _searchDtc.SearchDtcFaultInfor(listConfig, dictCNode);
            object[] boxitem = new object[dictDtc.Count];
            int j = 0;
            foreach (KeyValuePair<string, string> item in dictDtc)
            {
                boxitem[j] = item.Key;
                j++;
            }
            return boxitem;
        }
        /// <summary>
        /// 把从任务处传进来的车型轮次等信息，转换成可查询DTC的字典
        /// </summary>
        private Dictionary<string, string> DictConvert(Dictionary<string, string> dict)
        {
            Dictionary<string, string> dictVNode = new Dictionary<string, string>();
            var vehicleSplit = dict["TaskNo"].Split('-');
            dictVNode["VehicelType"] = vehicleSplit[0];
            dictVNode["VehicelConfig"] = vehicleSplit[1];
            dictVNode["VehicelStage"] = vehicleSplit[2];
            dictVNode["MatchSort"] = dict["TestType"];
            dictVNode["EmlTemplateName"] = _selectedName;
            dictVNode["CANRoad"] = dict["CANRoad"];
            dictVNode["Module"] = dict["Module"];
            return dictVNode;
        }
        private void lbcNoBindDTC_DoubleClick(object sender, EventArgs e)
        {
            int index = lbcNoBindDTC.SelectedIndex;
            if (index == -1)
                return;
            lbcBindDTC.Items.Add(lbcNoBindDTC.GetItem(index));
            lbcNoBindDTC.Items.RemoveAt(index);

            List<string> listBind = new List<string>();
            foreach (var bindDTC in lbcBindDTC.Items)
            {
                listBind.Add(bindDTC.ToString());
            }
            listBind.Sort();
            lbcBindDTC.Items.Clear();
            lbcBindDTC.Items.AddRange(listBind.ToArray());
            SaveToDb();
        }

        private void lbcBindDTC_DoubleClick(object sender, EventArgs e)
        {
            int index = lbcBindDTC.SelectedIndex;
            if (index == -1)
                return;
            lbcNoBindDTC.Items.Add(lbcBindDTC.GetItem(index));
            lbcBindDTC.Items.RemoveAt(index);

            List<string> listNoBind = new List<string>();
            foreach (var noBindDTC in lbcNoBindDTC.Items)
            {
                listNoBind.Add(noBindDTC.ToString());
            }
            listNoBind.Sort();
            lbcNoBindDTC.Items.Clear();
            lbcNoBindDTC.Items.AddRange(listNoBind.ToArray());
            SaveToDb();
        }
    }
}