using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.SqlCommand;
using Model;
namespace DBEngine
{
    /// <summary>
    /// 基础存储实现方法
    /// </summary>
    public  class BaseSqlOrder
    {
        #region 查询多条记录指令
        /// <summary>
        /// 查询数据
        /// </summary>
        /// <param name="queryStr">指定查询条件</param>
        /// <returns>返回查询队形</returns>
        public static IList<object[]> GetMultipleByQuery(string queryStr)
        {
        
            using (var session = NHelper.OpenSession())
            {
                var products = session
                    .CreateSQLQuery(queryStr)
                    .List<object[]>();
                session.Flush();
                session.Clear();
                session.BeginTransaction().Commit();
                return products;
            }
        }
        #endregion

        #region
        public static IList<object> GetExapChapterByQuery(string queryStr)
        {

            using (var session = NHelper.OpenSession())
            {
                var products = session
                    .CreateSQLQuery(queryStr)
                    .List<object>();
                session.Flush();
                session.Clear();
                session.BeginTransaction().Commit();
                return products;
            }
        }
        #endregion

        #region 添加信息

        /// <summary>
        /// 添加信息
        /// </summary>
        /// <param name="info">添加信息</param>
        /// <param name="error">异常信息</param>
        public static bool Add(object info,out string error)
        {
            try
            {
                using (var session = NHelper.OpenSession())
                using (var transaction = session.BeginTransaction())
                {
                    session.Save(info);
                    session.Flush();
                    session.Clear();
                    transaction.Commit();
                }
                error = "";
                return true;
            }
            catch (Exception ex)
            {
                error = ex.ToString();
                return false;
            }
        }
        #endregion

        #region 删除指令

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="delStr">删除条件</param>
        /// <param name="error"></param>
        public static bool Del(string delStr,out string error)
        {
            try
            {
                using (var session = NHelper.OpenSession())
                {
                    session.CreateSQLQuery(delStr).ExecuteUpdate();
                    session.Flush();
                    session.Clear();
                    session.BeginTransaction().Commit();
                }
                error = "";
                return true;
            }
            catch (Exception ex)
            {
                error = ex.ToString();
                return false;
            }
           
        }

        #endregion


        #region 更新指令

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="updateStr">删除条件</param>
        /// <param name="error"></param>
        public static bool Update(string updateStr, out string error)
        {
            try
            {
                using (var session = NHelper.OpenSession())
                {
                    session.CreateSQLQuery(updateStr).ExecuteUpdate();
                    session.Flush();
                    session.Clear();
                    session.BeginTransaction().Commit();
                }
                error = "";
                return true;
            }
            catch (Exception ex)
            {
                error = ex.ToString();
                return false;
            }

        }

        #endregion

        #region 是否存在某条记录
        public static bool IsExist(string queryStr)
        {
            using (ISession session = NHelper.OpenSession())
            {
                IList<object> products = session
                    .CreateSQLQuery(queryStr)
                    .List<object>();
                session.Flush();
                session.Clear();
                session.BeginTransaction().Commit();
                return products != null && products.Count > 0;
            }
        }
        #endregion

        #region 得到某一列的数据集合
        public static IList<string> GetSignalColByQuery(string queryStr,int colIndex)
        {
            using (var session = NHelper.OpenSession())
            {
                var products = session
                     .CreateSQLQuery(queryStr)
                     .List<object[]>();
                session.Flush();
                session.Clear();
                session.BeginTransaction().Commit();
                return products.Select(product => product[colIndex].ToString()).ToList();
            }
        }
        #endregion
    }
}
