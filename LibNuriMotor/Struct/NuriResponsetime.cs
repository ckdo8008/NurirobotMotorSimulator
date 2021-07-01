namespace LibNuriMotor.Struct
{
    /// <summary>
    /// 응답시간 설정
    /// </summary>
    public class NuriResponsetime : BaseStruct
    {
        /// <summary>
        /// 통신 응답시간(us)
        /// </summary>
        public short Responsetime { get; set; }
        /// <summary>
        /// 피드백 모드
        /// </summary>
        public byte Protocol { get; set; }
        public NuriResponsetime() : base() { }
    }
}
