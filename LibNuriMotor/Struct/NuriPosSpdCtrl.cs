namespace LibNuriMotor.Struct
{
    /// <summary>
    /// 위치제어기/속도제어기 설정
    /// </summary>
    public class NuriPosSpdCtrl : BaseStruct
    {
        /// <summary>
        /// 모드
        /// </summary>
        public byte Protocol { get; set; }
        public byte Kp { get; set; }
        public byte Ki { get; set; }
        public byte kd { get; set; }
        /// <summary>
        /// 정격전류
        /// </summary>
        public short Current { get; set; }
        public NuriPosSpdCtrl() : base() { }
    }
}
