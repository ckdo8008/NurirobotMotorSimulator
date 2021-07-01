namespace LibNuriMotor
{
    /// <summary>
    /// 패킷 명령어 인터페이스
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// 패킷 명칭
        /// </summary>
        string PacketName { get; set; }
        /// <summary>
        /// 전달 데이터
        /// </summary>
        byte[] Data { get; set; }
        /// <summary>
        /// 장비 ID
        /// </summary>
        byte ID { get; set; }

        /// <summary>
        /// 파싱
        /// </summary>
        /// <param name="data">수신 데이터</param>
        /// <returns>처리결과</returns>
        bool Parse(byte[] data);
        /// <summary>
        /// 데이터 체크섬 체크
        /// </summary>
        /// <returns>데이터 체크섬</returns>
        byte GetCheckSum();
        /// <summary>
        /// 통신프로토콜 객체 요청
        /// </summary>
        /// <returns>통신프로토콜 객체</returns>
        object GetDataStruct();
    }
}
