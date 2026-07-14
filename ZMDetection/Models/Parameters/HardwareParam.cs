using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZMDetection.Tools;

namespace ZMDetection.Models
{
    public class HardwareParam
    {
        public HardwareParam()
        {

        }

        private static readonly object _lockObj = new object();
        private static HardwareParam? _hardwareParam;
        public static HardwareParam HardwareParamConfig
        {
            get
            {
                if (_hardwareParam == null)
                {
                    lock (_lockObj)
                    {
                        if (_hardwareParam == null)
                        {
                            _hardwareParam = new HardwareParam();
                        }
                    }
                }
                return _hardwareParam;
            }
            set
            {
                _hardwareParam = value;
            }
        }

        #region <<<属性
        /// <summary>
        /// 光源亮度
        /// </summary>
        public int LightSource { get; set; } = 200;
        /// <summary>
        /// 相机曝光时间
        /// </summary>
        public int CamExposureTime { get; set; } = 15000;
        /// <summary>
        /// 相机增益
        /// </summary>
        public int CamGian { get; set; } = 1;

        /// <summary>
        /// 相机X方向标定
        /// </summary>
        public double CamXCalibration { get; set; } = 1.0;
        /// <summary>
        /// 相机Y方向标定
        /// </summary>
        public double CamYCalibration { get; set; } = 1.0;
        #endregion
    }
}
