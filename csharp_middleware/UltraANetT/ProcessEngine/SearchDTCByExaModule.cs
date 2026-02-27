using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProcessEngine
{
    public class SearchDTCByExaModule
    {
        private readonly ProcFile _file = new ProcFile();
        
        private readonly ProcStore _store = new ProcStore();
        /// <summary>
        /// 整理用例名称和配置表里面DTC的对应关系
        /// </summary>
        /// <param name="dictCon">包含车型，配置，阶段，从属CAN/LIN路，模块，用例表名称</param>
        /// <param name="strExapInfor">用例表的Json串</param>
        /// <returns>一个字典型，键名是用例名称，键值是所需的故障类型输入项信息</returns>
        public Dictionary<string,string> SearchDtcDefaltInfor(Dictionary<string, object> dictCon,string strExapInfor)
        {
            Dictionary<string,string> dtcInfor = new Dictionary<string, string>();
            List<Dictionary<string, string>> listConfig = new List<Dictionary<string, string>>();
            string match = _file.SearchBusByEmlName(dictCon["ExapTableName"].ToString());
            dictCon.Add("MatchSort", match);
            //找到用例表
            IList<object[]> listFilecfg = _store.GetSpecialByEnum(EnumLibrary.EnumTable.FileLinkByVehicelRep,
                dictCon);
            Dictionary<string, Dictionary<string, List<object>>> dictExap = new Dictionary<string, Dictionary<string, List<object>>>();
            dictExap = Json.DeserJsonToDDict(strExapInfor);
            foreach (var cfglist in listFilecfg)
            {
                if (!string.IsNullOrWhiteSpace(cfglist[7].ToString()))
                {
                    listConfig = Json.DerJsonToLDict(cfglist[7].ToString());
                    break;
                }
            }
            Dictionary<string, string> dictInfor = new Dictionary<string, string>();
            dictInfor = SearchDtcFaultInfor(listConfig, dictCon);
            foreach (KeyValuePair<string, Dictionary<string, List<object>>> dictChapter in dictExap)
            {
                foreach (var dictExapId in dictChapter.Value)
                {
                    List<object> listExap = dictExapId.Value;
                    Dictionary<string, string> dictass = Json.DerJsonToDict(listExap[0].ToString());
                    foreach (KeyValuePair<string,string> fault in dictInfor)
                    {
                        if (dictass["DTC"] == fault.Key)
                            dtcInfor[dictExapId.Key] = fault.Value;
                    }
                }
            }
            return dtcInfor;
        }

        public Dictionary<string, string> SearchDtcFaultInfor(List<Dictionary<string, string>> listConfig, Dictionary<string, object> dictCon)
        {
            Dictionary<string, string> dictInfor = new Dictionary<string, string>();
            foreach (Dictionary<string, string> dict in listConfig)
            {
                string dtc = "";
                if (dict["TestChannel"] == dictCon["CANRoad"].ToString())
                {
                    if (dict["DUTname"] == dictCon["Module"].ToString())
                    {
                        List<Dictionary<string, string>> listDTC = new List<Dictionary<string, string>>();
                        listDTC = Json.DerJsonToLDict(dict["DTCRelevant"]);
                        foreach (Dictionary<string, string> dtcfault in listDTC)
                        {
                            dictInfor[dtcfault["DTC"]] = dtcfault["DTCHEX"];
                        }
                    }
                }
            }
            return dictInfor;
        }
    }
}
