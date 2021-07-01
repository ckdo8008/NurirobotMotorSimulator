namespace LibNuriMotor.Struct
{
    using LibNuriMotor.Enum;
    /// <summary>
    /// 모터 제어 방향
    /// </summary>
    public class NuriCtrlDirection : BaseStruct
    {
        /// <summary>
        /// 제어 방향
        /// </summary>
        public Direction Direction { get; set; }
        /// <summary>
        /// 피드백 모드
        /// </summary>
        public byte Protocol { get; set; }
        public NuriCtrlDirection() : base() { }
    }
}
