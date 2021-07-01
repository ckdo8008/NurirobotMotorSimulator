namespace LibNuriMotor.Struct
{
    /// <summary>
    /// 분해능
    /// </summary>
    public class NuriResolution : BaseStruct
    {
        /// <summary>
        /// 분해능
        /// </summary>
        public ushort Resolution { get; set; }
        /// <summary>
        /// 피드백 모드
        /// </summary>
        public byte Protocol { get; set; }
        public NuriResolution() : base() { }
    }
}
