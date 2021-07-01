namespace LibNuriMotor.Struct
{
    /// <summary>
    /// 제어 On/Off 설정
    /// </summary>
    public class NuriControlOnOff : BaseStruct
    {
        /// <summary>
        /// 제어 On/Off
        /// </summary>
        public bool IsCtrlOn { get; set; }
        /// <summary>
        /// 피드백 모드
        /// </summary>
        public byte Protocol { get; set; }
        /// <summary>
        /// 생성장
        /// </summary>
        public NuriControlOnOff() : base() { }
    }
}
