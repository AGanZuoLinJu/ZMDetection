using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using PCBDetection.Tools;

namespace PCBDetection.Models
{
    [Serializable]
    [XmlRoot("VisionParam")]
    public class VisionParam
    {
        #region <<<单例化
        public VisionParam()
        {

        }

        private static readonly object _lockObj = new object();
        private static VisionParam? _visionParam;
        public static VisionParam VisionParamConfig
        {
            get
            {
                // 如果类的实例不存在则创建，否则直接返回
                if (_visionParam == null)
                {
                    lock (_lockObj)
                    {
                        // 如果类的实例不存在则创建
                        if (_visionParam == null)
                        {
                            _visionParam = new VisionParam();
                        }
                    }
                }
                return _visionParam;
            }
            set
            {
                _visionParam = value;
            }
        }
        #endregion

        #region <<<属性
        /// <summary>
        /// 机种名
        /// </summary>
        [XmlAttribute("RecipeName")]
        public string? RecipeName { get; set; }
        #endregion

        #region <<<传统算法检测相关
        public double DefectLong { get; set; } = 0;
        public double DefectWidth { get; set; } = 0;
        public double DefectHeight { get; set; } = 0;
        #endregion
    }
}
