namespace LibNuriMotor.Struct
{
    /// <summary>
    /// 위치제어
    /// </summary>
    public class NuriPositionCtrl : BaseStruct
    {
        /// <summary>
        /// 절대 위치 여부
        /// true: 절대위치
        /// false: 상대위치
        /// </summary>
        public bool IsAbsolutePotionCtrl { get; set; }
        /// <summary>
        /// 피드백 모드
        /// </summary>
        public byte Protocol { get; set; }
        public NuriPositionCtrl() : base() { }
    }
}
