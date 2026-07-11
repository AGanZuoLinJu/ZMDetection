using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ZMDetection.Tools
{
    /// <summary>
    /// xml序列化类
    /// </summary>
    public class XmlExSerializer
    {
        /// <summary>
        /// 将对象T序列化到文件中
        /// </summary>
        /// <typeparam name="T">对象标志</typeparam>
        /// <param name="obj">要序列的对象</param>
        /// <param name="path">文件路径</param>
        public static void Serialize<T>(T obj, string xmlPath)
        {
            //获得当前实例的类型
            Type tempType = obj!.GetType();
            //实例化要写入XML的文档
            StreamWriter writer = new StreamWriter(xmlPath);
            try
            {
                //创建与输入类型一致的序列化器
                XmlSerializer serialize = new XmlSerializer(tempType);
                //将对象序列化并写入到指定的XML文档中
                serialize.Serialize(writer, obj);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                writer.Close();
                writer.Dispose();
            }
        }
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xmlPath"></param>
        /// <returns></returns>
        public static T Deserialize<T>(string xmlPath)
        {
            if (!System.IO.File.Exists(xmlPath))
            {
                return default(T)!;
            }
            System.IO.TextReader reader = new System.IO.StreamReader(xmlPath);
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(reader);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                reader.Close();
                reader.Dispose();
            }
        }
    }

    /// <summary>
    /// xml读写类
    /// </summary>
    public class XMLHelper
    {
        public static void Save<T>(T Anyobject, string Path)
        {
            XmlExSerializer.Serialize<T>(Anyobject, Path);
        }

        public static T Load<T>(string Path)
        {
            T anyObj = System.Activator.CreateInstance<T>();
            anyObj = XmlExSerializer.Deserialize<T>(Path);
            return anyObj;
        }
    }
}
