using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ZMDetection.Tools;

namespace ZMDetection.Models
{
    [Serializable]
    [XmlRoot("CommunicationParam")]
    public class CommunicationParam
    {
        #region 单例化
        public CommunicationParam() { }
        private static readonly object _lockObj = new object();
        private static CommunicationParam? _communicationParam;
        public static CommunicationParam CommParamConfig
        {
            get
            {
                // 如果类的实例不存在则创建，否则直接返回
                if (_communicationParam == null)
                {
                    // 当第一个线程运行到这里时，此时会对locker对象 "加锁"。
                    // 当第二个线程运行该方法时，首先检测到locker对象为"加锁"状态，该线程就会挂起等待第一个线程解锁
                    // lock语句运行完之后（即线程运行完之后）会对该对象"解锁"
                    lock (_lockObj)
                    {
                        // 如果类的实例不存在则创建
                        if (_communicationParam == null)
                        {
                            _communicationParam = new CommunicationParam();
                        }
                    }
                }
                return _communicationParam;
            }
            set
            {
                _communicationParam = value;
            }
        }
        #endregion

        #region 参数
        /// <summary>
        /// PLCIP地址
        /// </summary>
        public string PLCIPAddress { get; set; } = "127.0.0.1";
        /// <summary>
        /// PLC端口号
        /// </summary>
        public int PLCPort { get; set; } = 5000;
        /// <summary>
        /// MES服务端监听端口
        /// </summary>
        public int MESPort { get; set; } = 6000;
        #endregion
    }
}
