using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using DBCEngine;
using DBEngine;

namespace ProcessEngine
{
    public class ProcShow 
    {
        private readonly ProcStore _store = new ProcStore();
        private readonly ProcFile _file = new ProcFile();
        private readonly procDBC _dbcAnalysis = new procDBC();
        private readonly procLDF _ldfAnalysis = new procLDF();
        //private readonly proccessDBC _dbc = new proccessDBC();
        Dictionary<string, string[]> _dictDBC = new Dictionary<string, string[]>();



        public List<string> GetDataFromDbc( List<string> currentVNode,out bool isExistDBC)
        { 
            var dbcList = _store.GetDBCListByVNodeAndCAN(currentVNode);
            List<string> modules = new List<string>();
            foreach (var dbc in dbcList)
            {
                var exValue = 0;
                var dbcName = dbc[3].ToString();
                //var dbcBytes = dbc[5] as byte[];
                var path = AppDomain.CurrentDomain.BaseDirectory + dbc[5];
                if (!File.Exists(path))
                {
                    isExistDBC = false;
                    return null;
                }
                    Dictionary<string,string[]> dict = new Dictionary<string, string[]>();
                //dict = _dbc.GetDataFromDBC(ref exValue, path);
                if(currentVNode[3].Substring(0,3).ToUpper()!="LIN")
                {
                    dict = _dbcAnalysis.GetDataFromDBC(ref exValue, path);
                }
                else
                {
                    dict = _ldfAnalysis.GetDataFromLDF(ref exValue, path);
                }
                string[] Keys = new string[dict.Keys.Count];
                dict.Keys.CopyTo(Keys,0);
                foreach (var key in Keys)
                {
                    if (!modules.Contains(key))
                    {
                        modules.Add(key);
                    }
                }       
            }
            isExistDBC = true;
            return modules;
        }
        //传入数据文件路径获取节点
        public List<string> ObtainNode(bool IsCan, int exValue, string path)
        {
            List<string> modules = new List<string>();
            Dictionary<string, string[]> dict = new Dictionary<string, string[]>();
            if (IsCan)
            {
                dict = _dbcAnalysis.GetDataFromDBC(ref exValue, path);
            }
            else
            {
                dict = _ldfAnalysis.GetDataFromLDF(ref exValue, path);
            }
            string[] Keys = new string[dict.Keys.Count];
            dict.Keys.CopyTo(Keys, 0);
            foreach (var key in Keys)
            {
                if (!modules.Contains(key))
                {
                    modules.Add(key);
                }
            }
            return modules;
        }

        //传入当前节点信息
        public List<string> ObtainCorrntImf(List<string> currentVNode)
        {
            IList<object[]>dbcList = _store.GetDBCListByVNodeAndCAN(currentVNode);
            List<String> list = new List<string>();
            for (int i=0;i< dbcList[0].Length;i++) {
                if (dbcList[0][i] != null)
                {
                    list.Add(dbcList[0][i].ToString());
                }
                else {
                    list.Add("");
                } 
            } 
            return list;
        }

        public Dictionary<string, string[]> GetDataFromLdf(List<string> currentVNode)
        {
            Dictionary<string, string[]> dict = new Dictionary<string, string[]>();
            var dbcList = _store.GetDBCListByVNodeAndCAN(currentVNode);
            foreach (var dbc in dbcList)
            {
                var exValue = 0;
                var dbcName = dbc[3].ToString();
                var path = AppDomain.CurrentDomain.BaseDirectory + dbc[5];
                dict = _ldfAnalysis.GetDataFromLDF(ref exValue, path);
            }
            return dict;
        }

        public string GetSlaveIDFromLdf(List<string> currentVNode,string slaveNode)
        {
            string slaveId = string.Empty;
            var dbcList = _store.GetDBCListByVNodeAndCAN(currentVNode);
            foreach (var dbc in dbcList)
            {
                var path = AppDomain.CurrentDomain.BaseDirectory + dbc[5];
                slaveId = _ldfAnalysis.GetSlaveID(path, slaveNode);
            }
            return slaveId;
        }

        public DataTable DrawDtFromMultiple(string[] colNames, Dictionary<string, object> SelectValue, EnumLibrary.EnumTable enumTable)
        {
            var dt = new DataTable();
            foreach (var colName in colNames)
                dt.Columns.Add(new DataColumn(colName, typeof(object)));
            //从数据库中查询指定表的数据信息
            var departmentList = _store.GetSpecialByEnum(enumTable, SelectValue);
            foreach (var dept in departmentList)
                dt.Rows.Add(dept);
            return dt;
        }
        /// <summary>
        /// 将多条记录绘制成为数据表格
        /// </summary>
        /// <param name="colNames">表格的列头</param>
        /// <param name="enumTable">表名枚举</param>
        /// <returns>返回数据表格</returns>
        public DataTable DrawDtFromMultiple(string[] colNames, EnumLibrary.EnumTable enumTable)
        {
            var dt = new DataTable();
            foreach (var colName in colNames)
                dt.Columns.Add(new DataColumn(colName, typeof(object)));
            //从数据库中查询指定表的数据信息
            var departmentList = _store.GetRegularByEnum(enumTable);
            if(departmentList.Count>0)
            {
                dt.Rows.Add(departmentList[departmentList.Count - 1]);
                int count = 0;
                foreach (var dept in departmentList)
                {
                    if (count < departmentList.Count - 1)
                        dt.Rows.Add(dept);
                    count++;
                }
            }
            return dt;
        }
        public DataTable DrawDtFromExapChapter(string[] colNames, EnumLibrary.EnumTable enumTable)
        {
            var dt = new DataTable();
            foreach (var colName in colNames)
                dt.Columns.Add(new DataColumn(colName, typeof(object)));
            //从数据库中查询指定表的数据信息
            var departmentList = _store.GetRegularByEnum(enumTable);
            if (departmentList.Count > 0)
            {
                object[] rowl = new object[departmentList[departmentList.Count - 1].Length + 1];
                rowl[0] = 1;
                for (int i = 0; i < departmentList[departmentList.Count - 1].Length; i++)
                    rowl[i + 1] = departmentList[departmentList.Count - 1][i];
                dt.Rows.Add(rowl);
                int count = 2;
                int listCount = 0;
                foreach (var dept in departmentList)
                {
                    if (listCount < departmentList.Count - 1)
                    {
                        object[] row = new object[dept.Length + 1];
                        row[0] = count;
                        for (int i = 0; i < dept.Length; i ++)
                            row[i + 1] = dept[i];
                        dt.Rows.Add(row);
                        count++;
                        listCount ++;
                    }
                }
            }
            return dt;
        }
        public DataTable DrawDtFromQuestionNote(string[] colNames, EnumLibrary.EnumTable enumTable)
        {
            var dt = new DataTable();
            foreach (var colName in colNames)
                dt.Columns.Add(new DataColumn(colName, typeof(object)));
            //从数据库中查询指定表的数据信息
            var departmentList = _store.GetRegularByEnum(enumTable);
            if (departmentList.Count > 0)
            {
                //dt.Rows.Add(departmentList[departmentList.Count - 1]);
                int count = 0;
                foreach (var dept in departmentList)
                {
                    if (count < departmentList.Count)
                        dt.Rows.Add(dept);
                    count++;
                }
            }
            return dt;
        }
        public DataTable DrawDtFromTask(string[] colNames)
        {
            var dt = new DataTable();
           
            foreach (var colName in colNames)
                dt.Columns.Add(new DataColumn(colName, typeof(object)));
            //从数据库中查询指定表的数据信息
            var departmentList = _store.GetRegularByEnum(EnumLibrary.EnumTable.Task);
            if (departmentList.Count > 0)
            {
                dt.Rows.Add(departmentList[departmentList.Count - 1]);
                int count = 0;
                for (int i = 0; i < departmentList.Count - 1; i++)
                {
                    dt.Rows.Add(departmentList[i]);
                }
                
            }
            return dt;
        }



        
    }
}
