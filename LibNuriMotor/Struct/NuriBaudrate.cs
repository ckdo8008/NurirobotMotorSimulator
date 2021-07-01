namespace LibNuriMotor.Struct
{
    /// <summary>
    /// 통신속도
    /// </summary>
    public class NuriBaudrate : BaseStruct
    {
        /// <summary>
        /// 통식속도
        /// </summary>
        public int Baudrate { get; set; }
        /// <summary>
        /// 피드백 모드
        /// </summary>
        public byte Protocol { get; set; }
        public NuriBaudrate() : base() { }
    }
}
