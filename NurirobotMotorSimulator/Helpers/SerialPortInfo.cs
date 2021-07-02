using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.SerialCommunication;

namespace NurirobotMotorSimulator.Helpers
{
    /// <summary>
    /// 시리얼 포트 연결 정보
    /// </summary>
    public class SerialPortInfo
    {
        /// <summary>
        /// 시리얼 장치 명칭
        /// </summary>
        public string PortName { get; set; }
        /// <summary>
        /// 현재 사용 중인지 확인
        /// </summary>
        public bool IsNowUsing { get; set; }
        /// <summary>
        /// 해당 포트의 ID
        /// </summary>
        public string PortID { get; set; }
        //public SerialDevice Device { get; set; }
    }
}
