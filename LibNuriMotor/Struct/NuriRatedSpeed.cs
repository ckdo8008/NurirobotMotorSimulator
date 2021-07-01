namespace LibNuriMotor.Struct
{
    /// <summary>
    /// 모터 정격 속도
    /// </summary>
    public class NuriRatedSpeed : BaseStruct
    {
        /// <summary>
        /// 정격속도(RPM)
        /// </summary>
        public ushort Speed { get; set; }
        /// <summary>
        /// 피드백 모드
        /// </summary>
        public byte Protocol { get; set; }
        public NuriRatedSpeed() : base() { }
    }
}
