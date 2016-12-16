using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace SOA.common
{
    /// <summary>
    /// 属性文件读取操作类
    /// </summary>
    public class Properties
    {

        static ObjectCache cache = MemoryCache.Default;
        const string CacheKey = "CacheKey";
        /// <summary>
        /// 获取缓存文件，文件值发生变化重新获取
        /// </summary>
        /// <returns></returns>
        public static string GetValue()
        {
            var content = cache[CacheKey] as string;
            if (content == null)
            {
                string FullPath = AppDomain.CurrentDomain.BaseDirectory;

                var file = FullPath + SOA.config.ConfigConstants.CONFIG_FILE;
                CacheItemPolicy policy = new CacheItemPolicy();
                policy.ChangeMonitors.Add(new HostFileChangeMonitor(new List<string> { file }));

                content = File.ReadAllText(file);
                cache.Set(CacheKey, content, policy);
            }
            else
            {
            }

            return content;
        }




        private StreamReader sr = null;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="strFilePath">文件路径</param>
        public Properties()
        {
            // sr = new StreamReader(strFilePath);
            string str = GetValue();
            byte[] arrb = Encoding.UTF8.GetBytes(str);
            MemoryStream stream = new MemoryStream(arrb);
            sr = new StreamReader(stream);
        }

        /// <summary>
        /// 关闭文件流
        /// </summary>
        public void Close()
        {
            sr.Close();
            sr = null;
        }

        /// <summary>
        /// 根据键获得值字符串
        /// </summary>
        /// <param name="strKey">键</param>
        /// <returns>值</returns>
        public string getProperty(string strKey)
        {
            string strResult = string.Empty;
            string str = string.Empty;
            sr.BaseStream.Seek(0, SeekOrigin.End);
            sr.BaseStream.Seek(0, SeekOrigin.Begin);
            while ((str = sr.ReadLine()) != null)
            {
                if (!str.Contains("#") && !string.IsNullOrEmpty(str) && !str.Contains("---"))
                {
                    if (str.Substring(0, str.IndexOf('=')).Equals(strKey))
                    {
                        strResult = str.Substring(str.IndexOf('=') + 1);
                        break;
                    }
                }
            }
            return strResult;
        }

        /// <summary>
        /// 根据键获得值数组
        /// </summary>
        /// <param name="strKey">键</param>
        /// <returns>值数组</returns>
        private string[] GetPropertiesArray(string strKey)
        {
            string strResult = string.Empty;
            string str = string.Empty;
            sr.BaseStream.Seek(0, SeekOrigin.End);
            sr.BaseStream.Seek(0, SeekOrigin.Begin);
            while ((str = sr.ReadLine()) != null)
            {
                if (str.Substring(0, str.IndexOf('=')).Equals(strKey))
                {
                    strResult = str.Substring(str.IndexOf('=') + 1);
                    break;
                }
            }
            return strResult.Split(',');
        }
    }
}
