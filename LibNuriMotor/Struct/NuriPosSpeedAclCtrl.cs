namespace LibNuriMotor.Struct
{
    using LibNuriMotor.Enum;
    /// <summary>
    /// 위치, 속도, 도달시간
    /// </summary>
    public class NuriPosSpeedAclCtrl : BaseStruct
    {

        /// <summary>
        ///  프로토콜
        /// </summary>
        public byte Protocol { get; set; }
        /// <summary>
        /// 진행방향
        /// </summary>
        public Direction Direction { get; set; }
        /// <summary>
        /// 위치(Degree)
        /// </summary>
        public float Pos { get; set; }
        /// <summary>
        /// 속도(RPM)
        /// </summary>
        public float Speed { get; set; }
        /// <summary>
        /// 도달시간(sec)
        /// </summary>
        public float Arrivetime { get; set; }
        /// <summary>
        /// 정격전류
        /// </summary>
        public short Current { get; set; }
        public NuriPosSpeedAclCtrl() : base() { }
    }
}
