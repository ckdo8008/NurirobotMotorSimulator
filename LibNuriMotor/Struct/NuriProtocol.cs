namespace LibNuriMotor.Struct
{
    /// <summary>
    /// 피드백 요청
    /// </summary>
    public class NuriProtocol : BaseStruct
    {
        /// <summary>
        /// 피드백 모드
        /// </summary>
        public byte Protocol { get; set; }
        public NuriProtocol() : base() { }
    }
}
