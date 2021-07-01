namespace LibNuriMotor.Struct
{
    /// <summary>
    /// 아이디 설정
    /// </summary>
    public class NuriID : BaseStruct
    {
        /// <summary>
        /// 변경 후 ID
        /// </summary>
        public byte AfterID { get; set; }
        /// <summary>
        /// 피드백 모드
        /// </summary>
        public byte Protocol { get; set; }
        public NuriID() : base() { }
    }
}
