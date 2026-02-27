using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ProcessEngine
{
    public class CopyVehicelInfo
    {
        private readonly ProcStore _store = new ProcStore();
        
        private IList<object[]> _cfgList = new List<object[]>();
        private IList<object[]> _dbcList = new List<object[]>();
        private IList<object[]> _authList = new List<object[]>();
        private IList<object[]> _tplyList = new List<object[]>();
        private TableName _curruntName;
        Dictionary<string, Dictionary<string, object>> oldTpyDict = new Dictionary<string, Dictionary<string, object>>();//装已经复制过拓扑图的车型
        public void SearchCopyVehicelByNode(Dictionary<string,object>  dictOldVehicel)
        {
            _dbcList = _store.GetSpecialByEnum(EnumLibrary.EnumTable.DBC, dictOldVehicel);
            _cfgList = _store.GetSpecialByEnum(EnumLibrary.EnumTable.FileLinkByVehicel, dictOldVehicel);
            _authList = _store.GetSpecialByEnum(EnumLibrary.EnumTable.Authorization_Auth, dictOldVehicel);
            _tplyList = _store.GetSpecialByEnum(EnumLibrary.EnumTable.Topology, dictOldVehicel);
        }
        public Dictionary<string,int> PasteVehicelInfoByNode(Dictionary<string, object> dictVehicel, Dictionary<string, object> dictOldVehice)
        {
            Type typeAuth = typeof(Model.Authorization);
            Type typeDBC = typeof(Model.DBC);
            Type typeFileLink = typeof(Model.FileLinkByVehicel);
            Type typeTopology = typeof (Model.Topology);

            Dictionary<string, int> dictFail = new Dictionary<string, int>();
            int iDBC = PasteDataToDataBase(typeDBC, _dbcList,dictVehicel, dictOldVehice);
            if (iDBC != 0)
            {
                dictFail["DBC"] = iDBC;
            }
            int iTply = PasteDataToDataBase(typeTopology, _tplyList, dictVehicel, dictOldVehice);
            if (iTply != 0)
            {
                dictFail["Topology"] = iTply;
            }
            int iAuth = PasteDataToDataBase(typeAuth, _authList,dictVehicel, dictOldVehice);
            if (iAuth != 0)
            {
                dictFail["Authorization"] = iAuth;
            }
            oldTpyDict = new Dictionary<string, Dictionary<string, object>>();
            int iFilelink = PasteDataToDataBase(typeFileLink, _cfgList,dictVehicel, dictOldVehice);
            if (iFilelink != 0)
            {
                dictFail["FileLinkByVehicel"] = iFilelink;
            }
            return dictFail;
        }

        private int PasteDataToDataBase(Type type, IList<object[]> iList, Dictionary<string, object> dictVehicel, Dictionary<string, object> dictOldVehice)
        {
            OperTableName(type);
            int failCount = 0;
            Dictionary<string, object> dict = new Dictionary<string, object>();
            if (iList.Count != 0)
            {
                foreach (var list in iList)
                {
                    int i = 0;
                    string error = "";
                    
                    foreach (var item in type.GetProperties())
                    {
                        bool same = false;
                        foreach (var name in dictVehicel)
                        {
                            if (name.Key == item.Name)
                            {
                                dict[item.Name] = name.Value;
                                i ++;
                                same = true;
                                break;
                            }
                        }
                        if (typeof (DateTime).ToString() == item.PropertyType.FullName)
                        {
                            dict[item.Name] = DateTime.Now;
                            i ++;
                            same = true;
                        }
                        if (item.Name == "InvalidTime")
                        {
                            dict[item.Name] = DateTime.Now.AddMonths(1);
                        }
                        //车型和不是时间的数据就从list中获得
                        if (!same)
                        {
                            dict[item.Name] = list[i];
                            i++;
                        }
                    }
                    if (!AddDataToDataBase(dict, dictOldVehice))
                    {
                        failCount++;
                    }

                }
           }
            return failCount;
        }

        private void OperTableName(Type type)
        {
            if(type.Name == "DBC")
                _curruntName = TableName.DBC;
            else if(type.Name == "Authorization")
            {
                _curruntName = TableName.Authorization;
            }
            else if (type.Name == "Topology")
            {
                _curruntName = TableName.Topology;
            }
            else
            {
                _curruntName = TableName.FileLinkByVehicel;
            }
        }
        private enum TableName
        {
            DBC = 0,
            Topology = 1,
            Authorization = 2,
            FileLinkByVehicel = 3
        }
        private bool AddDataToDataBase(Dictionary<string,object> dictData, Dictionary<string, object> dictOldVehice)
        {
            string error = "";
            bool flag = false;
            switch (_curruntName)
            {
                case TableName.DBC:
                    string oldname = dictOldVehice["VehicelType"] + "-" + dictOldVehice["VehicelConfig"] + "-" + dictOldVehice["VehicelStage"];
                    string newVehicel = dictData["VehicelType"] + "-" + dictData["VehicelConfig"] + "-" + dictData["VehicelStage"];
                    string newdbcName = dictData["DBCContent"].ToString().Replace(oldname, newVehicel);
                    //string dbcName = dictData["DBCContent"].ToString().Split('\\')[1];
                    //string[] name = dbcName.Split('-');
                    //string newName = dictData["VehicelType"] + "-" + dictData["VehicelConfig"] + "-" + dictData["VehicelStage"] + "-" + name[3];
                    dictData["DBCContent"] = newdbcName;
                    flag = _store.AddDBC(dictData, out error);
                    CreateLocalDBCFile(dictData, dictOldVehice);
                    break;
               case TableName.Authorization:
                    flag = _store.AddAuthorization(dictData, out error);
                    break;
               case TableName.FileLinkByVehicel:
                    string oldtpyname = dictOldVehice["VehicelType"] + "-" + dictOldVehice["VehicelConfig"] + "-" + dictOldVehice["VehicelStage"];
                    string newtpyVehicel = dictData["VehicelType"] + "-" + dictData["VehicelConfig"] + "-" + dictData["VehicelStage"];
                    string newtpyName = dictData["Topology"].ToString().Replace(oldtpyname, newtpyVehicel);
                    dictData["Topology"] = newtpyName;
                    flag = _store.AddPasteFileLinkByVehicel(dictData, out error);
                    //CreateLocalTopologyFile(dictData, dictOldVehice);
                    break;
                case TableName.Topology:
                    string oldtpynamet = dictOldVehice["VehicelType"] + "-" + dictOldVehice["VehicelConfig"] + "-" + dictOldVehice["VehicelStage"];
                    string newtpyVehicelt = dictData["VehicelType"] + "-" + dictData["VehicelConfig"] + "-" + dictData["VehicelStage"];
                    string newtpyNamet = dictData["Tply"].ToString().Replace(oldtpynamet, newtpyVehicelt);
                    dictData["Tply"] = newtpyNamet;
                    flag = _store.AddTopology(dictData, out error);
                    CreateLocalTopologyFile(dictData, dictOldVehice);
                    break;
            }
            return flag;
        }

        private void CreateLocalDBCFile(Dictionary<string, object> dictData, Dictionary<string, object> dictOldVehice)
        {
            string oldname = dictOldVehice["VehicelType"] + "-" + dictOldVehice["VehicelConfig"] + "-" + dictOldVehice["VehicelStage"];
            string newVehicel = dictData["VehicelType"] + "-" + dictData["VehicelConfig"] + "-" + dictData["VehicelStage"];
            
            string dbcPath = AppDomain.CurrentDomain.BaseDirectory + @"dbc\";

            string path = AppDomain.CurrentDomain.BaseDirectory + dictData["DBCContent"];
            string olddbcVehicel = dictData["DBCContent"].ToString().Replace(newVehicel,oldname);

            //string olddbcVehicel = @"dbc\" + dictOldVehice["VehicelType"] + "-" + dictOldVehice["VehicelConfig"] + "-" + dictOldVehice["VehicelStage"] + "-" + tpyName.Split('-')[3];
            //string olddbcPath = AppDomain.CurrentDomain.BaseDirectory + @"dbc\";
            string oldpath = AppDomain.CurrentDomain.BaseDirectory + olddbcVehicel;
            //File.Delete(AppDomain.CurrentDomain.BaseDirectory + dbcVehicel);
            if (!Directory.Exists(dbcPath))
                Directory.CreateDirectory(dbcPath);

            bool isrewrite = true; // true=覆盖已存在的同名文件,false则反之
            File.Copy(oldpath, path, isrewrite);
        }

        private void CreateLocalTopologyFile(Dictionary<string, object> dictData, Dictionary<string, object> dictOldVehice)
        {
            string vehicel = dictData["VehicelType"] + "-" + dictData["VehicelConfig"] + "-" + dictData["VehicelStage"] ;
            bool isFind = false;
            foreach (var item in oldTpyDict)
            {
                if (vehicel == item.Key)
                {
                    isFind = true;
                    break;
                }
            }
            if (!isFind)
            {
                string tplnewPath = AppDomain.CurrentDomain.BaseDirectory + @"topology\" + dictData["VehicelType"] + "-" + dictData["VehicelConfig"] + "-" + dictData["VehicelStage"] + @"\";

                string tpyName = dictData["Tply"].ToString().Split('\\')[2];
                string tplynewPath = @"topology\" + dictData["VehicelType"] + "-" + dictData["VehicelConfig"] + "-" + dictData["VehicelStage"] + @"\" + tpyName;
                if (!Directory.Exists(tplnewPath))
                    Directory.CreateDirectory(tplnewPath);
                string path = AppDomain.CurrentDomain.BaseDirectory + tplynewPath;
                string tploldPath = AppDomain.CurrentDomain.BaseDirectory + @"topology\" + dictOldVehice["VehicelType"] + "-" + dictOldVehice["VehicelConfig"] + "-" + dictOldVehice["VehicelStage"] + @"\" + tpyName;
                bool isrewrite = true;
                File.Copy(tploldPath, path, isrewrite);
                oldTpyDict.Add(vehicel, dictData);
            }
            
        }


    }
}
