using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProcessEngine
{
    public class LogicalControl
    {
        Dictionary<string ,object > _dict = new Dictionary<string ,object>();
        private ProcStore _store = new ProcStore();

        /// <summary>
        /// 角色判断
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public string RoleSelect(string userName)
        {
            string role;
            _dict.Add("ElyName",userName);
            IList<object[]> list = _store.GetSpecialByEnum(EnumLibrary.EnumTable.Employee_role, _dict);
            if (list[0][2].ToString() == "超级管理员")
            {
                role = "superadminister";
            }
            else if ( list[0][2].ToString()=="管理员")
            {
                role = "administer";
            }
            else if (list[0][2].ToString() =="配置员")
            {
                role ="configurator";
            }
            else
            {
                role = "tester";
            }
            return role;
        }
        

    }
}
