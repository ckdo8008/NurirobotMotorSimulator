namespace LibNuriMotor.Struct
{
    /// <summary>
    /// 펌웨어버전 피드백
    /// </summary>
    public class NuriVersion : BaseStruct
    {
        /// <summary>
        /// 버전
        /// </summary>
        public byte Version { get; set; }
        public NuriVersion() : base() { }
    }
}
