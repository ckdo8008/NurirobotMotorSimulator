using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace consoleWheelchair
{
    /// <summary>
    /// 연결속도
    /// </summary>
    public enum Baudrate : int
    {
        BR_110 = 110,
        BR_300 = 300,
        BR_600 = 600,
        BR_1200 = 1200,
        BR_2400 = 2400,
        BR_4800 = 4800,
        BR_9600 = 9600,
        BR_14400 = 14400,
        BR_19200 = 19200,
        BR_28800 = 28800,
        BR_38400 = 38400,
        BR_57600 = 57600,
        BR_76800 = 76800,
        BR_115200 = 115200,
        BR_230400 = 230400,
        BR_250000 = 250000,
        BR_500000 = 500000,
        BR_1000000 = 1000000
    }

    public enum BaudrateByte : byte
    {
        BR_110 = 0x00,
        BR_300 = 0x01,
        BR_600 = 0x02,
        BR_1200 = 0x03,
        BR_2400 = 0x04,
        BR_4800 = 0x05,
        BR_9600 = 0x06,
        BR_14400 = 0x07,
        BR_19200 = 0x08,
        BR_28800 = 0x09,
        BR_38400 = 0x0A,
        BR_57600 = 0x0B,
        BR_76800 = 0x0C,
        BR_115200 = 0x0D,
        BR_230400 = 0x0E,
        BR_250000 = 0x0F,
        BR_500000 = 0x10,
        BR_1000000 = 0x11
    }

    /// <summary>
    /// 패리티비트 
    /// </summary>
    public enum Parity : int
    {
        /// <summary>
        /// 패리티 검사를 수행 안함
        /// </summary>
        None = 0,
        /// <summary>
        /// 비트 집합의 비트 합계가 홀수가 되도록 패리티 비트를 설정
        /// </summary>
        Odd = 1,
        /// <summary>
        /// 비트 집합의 비트 합계가 짝수가 되도록 패리티 비트를 설정
        /// </summary>
        Even = 2,
        /// <summary>
        /// 패리티 비트를 1로 설정된 상태로 유지
        /// </summary>
        Mark = 3,
        /// <summary>
        /// 패리티 비트를 0으로 설정된 상태로 유지
        /// </summary>
        Space = 4
    }

    /// <summary>
    /// 정지 비트 수
    /// </summary>
    public enum StopBits : int
    {
        /// <summary>
        /// 정지 비트를 사용 안함
        /// </summary>
        None = 0,
        /// <summary>
        /// 1비트의 정지 비트를 사용
        /// </summary>
        One = 1,
        /// <summary>
        /// 2비트의 정지 비트를 사용
        /// </summary>
        Two = 2,
        /// <summary>
        /// 1.5비트의 정지 비트를 사용
        /// </summary>
        OnePointFive = 3
    }

    /// <summary>
    /// 제어 프로토콜
    /// </summary>
    public enum Handshake : int
    {
        /// <summary>
        /// 핸드셰이크에 제어 없음
        /// </summary>
        None = 0,
        /// <summary>
        /// XON/XOFF 소프트웨어 제어 프로토콜을 사용
        /// </summary>
        XOnXoff = 1,
        /// <summary>
        /// RTS(Request to Send) 하드웨어 흐름 제어를 사용
        /// </summary>
        RTS = 2,
        /// <summary>
        /// RTS(Request to Send) 하드웨어 제어와 XON/XOFF 소프트웨어 제어를 모두 사용
        /// </summary>
        RTSXOnXoff = 3
    }

    /// <summary>
    /// 시리얼 포트 설정
    /// </summary>
    public class SerialPortSetting
    {
        /// <summary>
        /// 포트 연결 명칭
        /// </summary>
        public string PortName { get; set; }
        /// <summary>
        /// 연결 속도
        /// </summary>
        public Baudrate Baudrate { get; set; }
        /// <summary>
        /// 패리티 비트
        /// </summary>
        public Parity Parity { get; set; }
        /// <summary>
        /// 데이터 비트
        /// </summary>
        public int DataBits { get; set; }
        /// <summary>
        /// 정지 비트
        /// </summary>
        public StopBits StopBits { get; set; }
        /// <summary>
        /// 제어 프로토콜
        /// </summary>
        public Handshake Handshake { get; set; }
        /// <summary>
        /// 읽기 타임아웃
        /// </summary>
        /// <remarks>
        /// <para>단위 : ms</para>
        /// <para>기본값 : 10</para>
        /// </remarks>
        public int ReadTimeout { get; set; } = 10;
        /// <summary>
        /// 쓰기 타임아웃
        /// </summary>
        /// <remarks>
        /// <para>단위 : ms</para>
        /// <para>기본값 : 10</para>
        /// </remarks>
        public int WriteTimeout { get; set; } = 10;

        /// <summary>
        /// 시리얼 포트 설정 생성
        /// </summary>
        /// <param name="sPortname">포트 연결 명칭</param>
        /// <param name="eBaudrate">연결 속도</param>
        /// <param name="eParity">패리티 비트</param>
        /// <param name="iDatabits">데이터 비트 수</param>
        /// <param name="eStopBits">정비 비트</param>
        public SerialPortSetting(
            string sPortname,
            Baudrate eBaudrate = Baudrate.BR_9600,
            Parity eParity = Parity.None,
            int iDatabits = 8,
            StopBits eStopBits = StopBits.None)
        {
            this.PortName = sPortname;
            this.Baudrate = eBaudrate;
            this.Parity = eParity;
            this.DataBits = iDatabits;
            this.StopBits = eStopBits;
        }

        /// <summary>
        /// 시리얼 포트 설정 생성
        /// </summary>
        public SerialPortSetting() : this(null) { }
    }
}
