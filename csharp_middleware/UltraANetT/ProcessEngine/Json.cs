using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace ProcessEngine
{
    public class Json
    {
        public static string SerJson(Dictionary<string, string> dictBaud)
        {
            return JsonConvert.SerializeObject(dictBaud);
        }
        public static string SerJson(Dictionary<string, List<string>> dictBaud)
        {
            return JsonConvert.SerializeObject(dictBaud);
        }
        public static string SerJson(Dictionary<string, List<object>> dictConfig)
        {
            return JsonConvert.SerializeObject(dictConfig);
        }
        public static string SerJson(Dictionary<string, Dictionary<string, List<object>>> dictExmp)
        {
            return JsonConvert.SerializeObject(dictExmp);
        }

        public static string SerJson(Dictionary<string, List<List<string>>> drReport)
        {
            return JsonConvert.SerializeObject(drReport);
        }

        public static string SerJson(List<Dictionary<string,string>> ListDict)
        {
            return JsonConvert.SerializeObject(ListDict);
        }
       
        public static string SerJson(Dictionary<string, List<Dictionary<string, string>>> ListDict)
        {
            return JsonConvert.SerializeObject(ListDict);
        }

        public static string SerJson(Dictionary<string, Dictionary<string, string>> dictBaud)
        {
            return JsonConvert.SerializeObject(dictBaud);
        }

        public static Dictionary<string, string> DerJsonToDict(string jsonStr)
        {
            var dictExmp =
                  JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonStr);
            return dictExmp;
        }
        public static Dictionary<string, List<Dictionary<string, string>>> DerJsonDictLd(string jsonStr)
        {
            var dictExmp =
                  JsonConvert.DeserializeObject<Dictionary<string, List<Dictionary<string, string>>>>(jsonStr);
            return dictExmp;
        }

        public static Dictionary<string, Dictionary<string, string>> DerJsonToDictDict(string jsonStr)
        {
            var dictExmp =
                JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(jsonStr);
            return dictExmp;
        }

        public static Dictionary<string, object> DerJsonToDictO(string jsonStr)
        {
            var dictExmp =
                  JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonStr);
            return dictExmp;
        }

        public static List<Dictionary<string, string>> DerJsonToLDict(string jsonStr)
        {
            var listExmp = new List<Dictionary<string, string>>();
            if (jsonStr != "")
                listExmp = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(jsonStr);
            return listExmp;
        }
        /// <summary>
        /// 解析用例Json
        /// </summary>
        /// <param name="jsonStr"></param>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, List<object>>> DeserJsonToDDict(string jsonStr)
        {
            var dictExmp =
                JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, List<object>>>>(jsonStr);
            return dictExmp;
        }      
        public static Dictionary<string, List<object>> DeserJsonDList(string jsonStr)
        {
            var dictFile =
              JsonConvert.DeserializeObject<Dictionary<string, List<object>>>(jsonStr);
            return dictFile;
        }
        public static Dictionary<string, List<string>> DeserJsonDListStr(string jsonStr)
        {
            var dictFile =
              JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(jsonStr);
            return dictFile;
        }
        public static Dictionary<string, List<List<string>>> DeserJsonDStrLList(string jsonStr)
        {
            var dictFile =
              JsonConvert.DeserializeObject<Dictionary<string, List<List<string>>>>(jsonStr);
            return dictFile;
        }
        public static List<Dictionary<string,string>> DeserJsonDictStrList(string jsonStr)
        {
            var dictFile =
              JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(jsonStr);
            return dictFile;
        }
        //Dictionary<string, Dictionary<string, List<object>>> GetExmpFormJson(string taskName);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<Dictionary<string, object>> DeSerJsonFromFile(string path)
        {
            List<Dictionary<string, object>> listJson = new List<Dictionary<string, object>>();
            //读取文件
            if (!File.Exists(path))
            {
                return null;
            }

            //FileStream fs = File.Create(path);
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            //if (fs == null)
            //    fs = new FileStream(path, FileMode.Open);

            StreamReader mStreamReader = new StreamReader(fs, Encoding.Default);

            mStreamReader.BaseStream.Seek(0, SeekOrigin.Begin);
            while (true)
            {
                var strLine = mStreamReader.ReadLine();
                if (string.IsNullOrEmpty(strLine))
                    break;
                //反序列化
                Dictionary<string, object> drJson = JsonConvert.DeserializeObject<Dictionary<string, object>>(strLine);
                listJson.Add(drJson);
            }
            fs.Close();
            mStreamReader.Close();
            return listJson;
        }
    }
}
