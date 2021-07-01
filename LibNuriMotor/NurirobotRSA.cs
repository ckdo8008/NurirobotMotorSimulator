namespace LibNuriMotor
{
    using LibNuriMotor.Enum;
    using LibNuriMotor.Struct;
    using System;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// Nurirobot RSA-RS485
    /// </summary>
    public class NurirobotRSA : ICommand
    {
        public string PacketName { get; set; }
        public byte[] Data { get; set; }
        public byte ID { get; set; }

        public byte GetCheckSum()
        {
            if (Data == null)
                return 0;
            else if (Data.Length >= 6)
            {
                int sumval = Data.Select(x => (int)x).Sum() - Data[0] - Data[1] - Data[4];
                return (byte)~(sumval % 256);
            }
            else
                return 0;
        }

        /// <summary>
        /// 프로토콜 작성
        /// </summary>
        /// <param name="id">장비 id</param>
        /// <param name="size">프로토콜 사이즈</param>
        /// <param name="mode">프로토콜 모드</param>
        /// <param name="data">프로토콜 데이터</param>
        /// <param name="isSend">시리얼 포트 전달여부 기본 : 전송</param>
        void BuildProtocol(byte id, byte size, byte mode, byte[] data, bool isSend = true)
        {
            int protocoSize = 6 + data.Length;
            Data = new byte[protocoSize];
            Data[0] = 0xFF;
            Data[1] = 0xFE;
            Data[2] = id;
            Data[3] = size;
            Data[5] = mode;
            Buffer.BlockCopy(data, 0, Data, 6, data.Length);
            Data[4] = GetCheckSum();
        }

        /// <summary>
        /// 통신속도
        /// </summary>
        /// <param name="arg">Baudrate 열거형</param>
        /// <returns>실제 통신 bps</returns>
        int GetBaudrate(byte arg)
        {
            switch ((BaudrateByte)arg)
            {
                case BaudrateByte.BR_110:
                    return 110;
                case BaudrateByte.BR_300:
                    return 300;
                case BaudrateByte.BR_600:
                    return 600;
                case BaudrateByte.BR_1200:
                    return 1200;
                case BaudrateByte.BR_2400:
                    return 2400;
                case BaudrateByte.BR_4800:
                    return 4800;
                case BaudrateByte.BR_9600:
                    return 9600;
                case BaudrateByte.BR_14400:
                    return 14400;
                case BaudrateByte.BR_19200:
                    return 19200;
                case BaudrateByte.BR_28800:
                    return 28800;
                case BaudrateByte.BR_38400:
                    return 38400;
                case BaudrateByte.BR_57600:
                    return 57600;
                case BaudrateByte.BR_76800:
                    return 76800;
                case BaudrateByte.BR_115200:
                    return 115200;
                case BaudrateByte.BR_230400:
                    return 230400;
                case BaudrateByte.BR_250000:
                    return 250000;
                case BaudrateByte.BR_500000:
                    return 500000;
                case BaudrateByte.BR_1000000:
                    return 1000000;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Baudrate bps값을 누리로봇 프로토콜 값으로 변경
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        byte GetBaudrateProtocol(int arg)
        {
            switch (arg)
            {
                case 110:
                    return (byte)BaudrateByte.BR_110;
                case 300:
                    return (byte)BaudrateByte.BR_300;
                case 600:
                    return (byte)BaudrateByte.BR_600;
                case 1200:
                    return (byte)BaudrateByte.BR_1200;
                case 2400:
                    return (byte)BaudrateByte.BR_2400;
                case 4800:
                    return (byte)BaudrateByte.BR_4800;
                case 9600:
                    return (byte)BaudrateByte.BR_9600;
                case 14400:
                    return (byte)BaudrateByte.BR_14400;
                case 19200:
                    return (byte)BaudrateByte.BR_19200;
                case 28800:
                    return (byte)BaudrateByte.BR_28800;
                case 38400:
                    return (byte)BaudrateByte.BR_38400;
                case 57600:
                    return (byte)BaudrateByte.BR_57600;
                case 76800:
                    return (byte)BaudrateByte.BR_76800;
                case 115200:
                    return (byte)BaudrateByte.BR_115200;
                case 230400:
                    return (byte)BaudrateByte.BR_230400;
                case 250000:
                    return (byte)BaudrateByte.BR_250000;
                case 500000:
                    return (byte)BaudrateByte.BR_500000;
                case 1000000:
                    return (byte)BaudrateByte.BR_1000000;
                default:
                    return 0;
            }
        }

        public object GetDataStruct()
        {
            switch ((ProtocolModeRSA)Data[5])
            {
                case ProtocolModeRSA.CTRLPosSpeed:
                    return new NuriPosSpeedAclCtrl
                    {
                        Protocol = Data[5],
                        ID = Data[2],
                        Direction = (Direction)Data[6],
                        Pos = BitConverter.ToUInt16(Data.Skip(7).Take(2).Reverse().ToArray(), 0) * 0.01f,
                        Speed = BitConverter.ToUInt16(Data.Skip(9).Take(2).Reverse().ToArray(), 0) * 0.1f,
                    };
                case ProtocolModeRSA.CTRLAccPos:
                    return new NuriPosSpeedAclCtrl
                    {
                        Protocol = Data[5],
                        ID = Data[2],
                        Direction = (Direction)Data[6],
                        Pos = BitConverter.ToUInt16(Data.Skip(7).Take(2).Reverse().ToArray(), 0) * 0.01f,
                        Arrivetime = Data[9] * 0.1f
                    };
                case ProtocolModeRSA.CTRLAccSpeed:
                    return new NuriPosSpeedAclCtrl
                    {
                        Protocol = Data[5],
                        ID = Data[2],
                        Direction = (Direction)Data[6],
                        Speed = BitConverter.ToUInt16(Data.Skip(7).Take(2).Reverse().ToArray(), 0) * 0.1f,
                        Arrivetime = Data[9] * 0.1f
                    };
                case ProtocolModeRSA.SETPosCtrl:
                    return new NuriPosSpdCtrl
                    {
                        Protocol = Data[5],
                        ID = Data[2],
                        Kp = Data[6],
                        Ki = Data[7],
                        kd = Data[8],
                        Current = (short)(Data[9] * 100)
                    };
                case ProtocolModeRSA.SETSpeedCtrl:
                    return new NuriPosSpdCtrl
                    {
                        Protocol = Data[5],
                        ID = Data[2],
                        Kp = Data[6],
                        Ki = Data[7],
                        kd = Data[8],
                        Current = (short)(Data[9] * 100)
                    };
                case ProtocolModeRSA.SETID:
                    return new NuriID
                    {
                        Protocol = Data[5],
                        ID = Data[2],
                        AfterID = Data[6]
                    };
                case ProtocolModeRSA.SETBaudrate:
                    return new NuriBaudrate
                    {
                        Protocol = Data[5],
                        ID = Data[2],
                        Baudrate = GetBaudrate(Data[6])
                    };
                case ProtocolModeRSA.SETResptime:
                    return new NuriResponsetime
                    {
                        Protocol = Data[5],
                        ID = Data[2],
                        Responsetime = (short)(Data[6] * 100)
                    };
                case ProtocolModeRSA.SETRatio:
                    return new NuriRatio
                    {
                        Protocol = Data[5],
                        ID = Data[2],
                        Ratio = BitConverter.ToUInt16(Data.Skip(6).Take(2).Reverse().ToArray(), 0) * 0.1f
                    };
                case ProtocolModeRSA.SETCtrlOnOff:
                    return new NuriControlOnOff
                    {
                        Protocol = Data[5],
                        ID = Data[2],
                        IsCtrlOn = Data[6] == 0
                    };
                case ProtocolModeRSA.SETPosCtrlMode:
                    return new NuriPositionCtrl
                    {
                        Protocol = Data[5],
                        ID = Data[2],
                        IsAbsolutePotionCtrl = Data[6] == 0
                    };
                case ProtocolModeRSA.RESETPos:
                    return new NuriProtocol
                    {
                        ID = Data[2],
                        Protocol = Data[5]
                    };
                case ProtocolModeRSA.RESETFactory:
                    return new NuriProtocol
                    {
                        ID = Data[2],
                        Protocol = Data[5]
                    };
                case ProtocolModeRSA.REQPing:
                    return new NuriProtocol
                    {
                        ID = Data[2],
                        Protocol = Data[5]
                    };
                case ProtocolModeRSA.REQPos:
                    return new NuriProtocol
                    {
                        ID = Data[2],
                        Protocol = Data[5]
                    };
                case ProtocolModeRSA.REQSpeed:
                    return new NuriProtocol
                    {
                        ID = Data[2],
                        Protocol = Data[5]
                    };
                case ProtocolModeRSA.REQPosCtrl:
                    return new NuriProtocol
                    {
                        ID = Data[2],
                        Protocol = Data[5]
                    };
                case ProtocolModeRSA.REQSpdCtrl:
                    return new NuriProtocol
                    {
                        ID = Data[2],
                        Protocol = Data[5]
                    };
                case ProtocolModeRSA.REQResptime:
                    return new NuriProtocol
                    {
                        ID = Data[2],
                        Protocol = Data[5]
                    };
                case ProtocolModeRSA.REQRatio:
                    return new NuriProtocol
                    {
                        ID = Data[2],
                        Protocol = Data[5]
                    };
                case ProtocolModeRSA.REQCtrlOnOff:
                    return new NuriProtocol
                    {
                        ID = Data[2],
                        Protocol = Data[5]
                    };
                case ProtocolModeRSA.REQPosCtrlMode:
                    return new NuriProtocol
                    {
                        ID = Data[2],
                        Protocol = Data[5]
                    };
                case ProtocolModeRSA.REQFirmware:
                    return new NuriProtocol
                    {
                        ID = Data[2],
                        Protocol = Data[5]
                    };
                case ProtocolModeRSA.FEEDPing:
                    return new NuriProtocol
                    {
                        ID = Data[2],
                        Protocol = Data[5]
                    };
                case ProtocolModeRSA.FEEDPos:
                    return new NuriPosSpeedAclCtrl
                    {
                        ID = Data[2],
                        Protocol = Data[5],
                        Direction = (Direction)Data[6],
                        Pos = BitConverter.ToUInt16(Data.Skip(7).Take(2).Reverse().ToArray(), 0) * 0.01f,
                        Speed = BitConverter.ToUInt16(Data.Skip(9).Take(2).Reverse().ToArray(), 0) * 0.1f,
                        Current = (short)(Data[11] * 100)
                    };
                case ProtocolModeRSA.FEEDSpeed:
                    return new NuriPosSpeedAclCtrl
                    {
                        ID = Data[2],
                        Protocol = Data[5],
                        Direction = (Direction)Data[6],
                        Speed = BitConverter.ToUInt16(Data.Skip(7).Take(2).Reverse().ToArray(), 0) * 0.1f,
                        Pos = BitConverter.ToUInt16(Data.Skip(9).Take(2).Reverse().ToArray(), 0) * 0.1f,
                        Current = (short)(Data[11] * 100)
                    };
                case ProtocolModeRSA.FEEDPosCtrl:
                    return new NuriPosSpdCtrl
                    {
                        ID = Data[2],
                        Protocol = Data[5],
                        Kp = Data[6],
                        Ki = Data[7],
                        kd = Data[8],
                        Current = (short)(Data[9] * 100)
                    };
                case ProtocolModeRSA.FEEDSpdCtrl:
                    return new NuriPosSpdCtrl
                    {
                        ID = Data[2],
                        Protocol = Data[5],
                        Kp = Data[6],
                        Ki = Data[7],
                        kd = Data[8],
                        Current = (short)(Data[9] * 100)
                    };
                case ProtocolModeRSA.FEEDResptime:
                    return new NuriResponsetime
                    {
                        Protocol = Data[5],
                        ID = Data[2],
                        Responsetime = (short)(Data[6] * 100)
                    };
                case ProtocolModeRSA.FEEDRatio:
                    return new NuriRatio
                    {
                        Protocol = Data[5],
                        ID = Data[2],
                        Ratio = BitConverter.ToUInt16(Data.Skip(6).Take(2).Reverse().ToArray(), 0) * 0.1f
                    };
                case ProtocolModeRSA.FEEDCtrlOnOff:
                    return new NuriControlOnOff
                    {
                        Protocol = Data[5],
                        ID = Data[2],
                        IsCtrlOn = Data[6] == 0
                    };
                case ProtocolModeRSA.FEEDPosCtrlMode:
                    return new NuriPositionCtrl
                    {
                        Protocol = Data[5],
                        ID = Data[2],
                        IsAbsolutePotionCtrl = Data[6] == 0
                    };
                case ProtocolModeRSA.FEEDFirmware:
                    return new NuriVersion
                    {
                        ID = Data[2],
                        Version = Data[6]
                    };
                default:
                    return null;
            }
        }

        public bool Parse(byte[] data)
        {
            bool ret = false;
            try
            {
                if (data.Length <= 5)
                    return ret;

                Data = new byte[data.Length];
                Buffer.BlockCopy(data, 0, Data, 0, Data.Length);

                if (Data[3] + 4 != data.Length)
                    return ret;

                var chksum = GetCheckSum();
                if (Data[4] == chksum)
                {
                    PacketName = ((ProtocolModeRSA)Data[5]).ToString("G");
                    if (PacketName == null)
                    {
                        ret = false;
                    }
                    else
                    {
                        ret = true;
                    }
                }
                else
                {
                    ret = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return ret;
        }


        /// <summary>
        /// 1. 위치, 속도제어(송신)
        /// </summary>
        /// <param name="arg">위치, 속도, 도달시간</param>
        public void PROT_ControlPosSpeed(NuriPosSpeedAclCtrl arg)
        {
            byte[] data = new byte[5];
            data[0] = (byte)(arg.Direction == Direction.CCW ? 0x00 : 0x01);
            var tmppos = BitConverter.GetBytes((ushort)Math.Round(arg.Pos / 0.01f)).Reverse().ToArray();
            Buffer.BlockCopy(tmppos, 0, data, 1, 2);
            var tmpspd = BitConverter.GetBytes((ushort)Math.Round(arg.Speed / 0.1f)).Reverse().ToArray();
            Buffer.BlockCopy(tmpspd, 0, data, 3, 2);
            BuildProtocol(arg.ID, 0x07, 0x01, data);
        }

        /// <summary>
        /// 1. 위치, 속도제어(송신)
        /// </summary>
        /// <param name="id">장비 아이디</param>
        /// <param name="direction">위치방향</param>
        /// <param name="pos">위치</param>
        /// <param name="spd">속도</param>
        public void ControlPosSpeed(byte id, byte direction, float pos, float spd)
        {
            PROT_ControlPosSpeed(new NuriPosSpeedAclCtrl
            {
                ID = id,
                Direction = (Direction)direction,
                Pos = pos,
                Speed = spd
            });
        }

        /// <summary>
        /// 2. 가감속 위치제어(송신)
        /// </summary>
        /// <param name="arg"></param>
        public void PROT_ControlAcceleratedPos(NuriPosSpeedAclCtrl arg)
        {
            byte[] data = new byte[4];
            data[0] = (byte)(arg.Direction == Direction.CCW ? 0x00 : 0x01);
            var tmppos = BitConverter.GetBytes((ushort)Math.Round(arg.Pos / 0.01f)).Reverse().ToArray();
            Buffer.BlockCopy(tmppos, 0, data, 1, 2);
            data[3] = (byte)Math.Round(arg.Arrivetime / 0.1f);
            BuildProtocol(arg.ID, 0x06, 0x02, data);
        }

        /// <summary>
        /// 2. 가감속 위치제어(송신)
        /// </summary>
        /// <param name="id">장비 아이디</param>
        /// <param name="direction">위치방향</param>
        /// <param name="pos">위치</param>
        /// <param name="arrive">도달시간(second)</param>
        public void ControlAcceleratedPos(byte id, byte direction, float pos, float arrive)
        {
            PROT_ControlAcceleratedPos(new NuriPosSpeedAclCtrl
            {
                ID = id,
                Direction = (Direction)direction,
                Pos = pos,
                Arrivetime = arrive
            });
        }


        /// <summary>
        /// 3. 가감속 속도제어(송신)
        /// </summary>
        /// <param name="arg"></param>
        public void PROT_ControlAcceleratedSpeed(NuriPosSpeedAclCtrl arg)
        {
            byte[] data = new byte[4];
            data[0] = (byte)(arg.Direction == Direction.CCW ? 0x00 : 0x01);
            var tmpspd = BitConverter.GetBytes((ushort)Math.Round(arg.Speed / 0.1f)).Reverse().ToArray();
            Buffer.BlockCopy(tmpspd, 0, data, 1, 2);
            data[3] = (byte)Math.Round(arg.Arrivetime / 0.1f);
            BuildProtocol(arg.ID, 0x06, 0x03, data);
        }

        /// <summary>
        /// 3. 가감속 속도제어(송신) 
        /// </summary>
        /// <param name="id">장비 아이디</param>
        /// <param name="direction">위치방향</param>
        /// <param name="speed">속도</param>
        /// <param name="arrive">도달시간(second)</param>
        public void ControlAcceleratedSpeed(byte id, byte direction, float speed, float arrive)
        {
            PROT_ControlAcceleratedSpeed(new NuriPosSpeedAclCtrl
            {
                ID = id,
                Direction = (Direction)direction,
                Speed = speed,
                Arrivetime = arrive
            });
        }

        /// <summary>
        /// 4. 위치제어기 설정(송신)
        /// </summary>
        /// <param name="arg"></param>
        public void PROT_SettingPositionController(NuriPosSpdCtrl arg)
        {
            byte[] data = new byte[4];
            data[0] = arg.Kp;
            data[1] = arg.Ki;
            data[2] = arg.kd;
            data[3] = (byte)(arg.Current / 100);
            BuildProtocol(arg.ID, 0x06, 0x04, data);
        }

        /// <summary>
        /// 4. 위치제어기 설정(송신)
        /// </summary>
        /// <param name="id">장비 아이디</param>
        /// <param name="kp">P</param>
        /// <param name="ki">I</param>
        /// <param name="kd">D</param>
        /// <param name="current">정격전류</param>
        public void SettingPositionController(byte id, byte kp, byte ki, byte kd, short current)
        {
            PROT_SettingPositionController(new NuriPosSpdCtrl
            {
                ID = id,
                Current = current,
                kd = kd,
                Ki = ki,
                Kp = kp
            });
        }

        /// <summary>
        /// 5. 속도제어기 설정(송신)
        /// </summary>
        /// <param name="arg"></param>
        public void PROT_SettingSpeedController(NuriPosSpdCtrl arg)
        {
            byte[] data = new byte[4];
            data[0] = arg.Kp;
            data[1] = arg.Ki;
            data[2] = arg.kd;
            data[3] = (byte)(arg.Current / 100);
            BuildProtocol(arg.ID, 0x06, 0x05, data);
        }

        /// <summary>
        /// 5. 속도제어기 설정(송신)
        /// </summary>
        /// <param name="id">장비 아이디</param>
        /// <param name="kp">P</param>
        /// <param name="ki">I</param>
        /// <param name="kd">D</param>
        /// <param name="current">정격전류</param>
        public void SettingSpeedController(byte id, byte kp, byte ki, byte kd, short current)
        {
            PROT_SettingSpeedController(new NuriPosSpdCtrl
            {
                ID = id,
                Current = current,
                kd = kd,
                Ki = ki,
                Kp = kp
            });
        }

        /// <summary>
        /// 6. ID설정(송신)
        /// </summary>
        /// <param name="arg"></param>
        public void PROT_SettingID(NuriID arg)
        {
            BuildProtocol(arg.ID, 0x03, 0x06, new byte[1] { arg.AfterID });
        }

        /// <summary>
        /// 6. ID설정(송신)
        /// </summary>
        /// <param name="id">장비 아이디</param>
        /// <param name="afterid">변경 아이디</param>
        public void SettingID(byte id, byte afterid)
        {
            PROT_SettingID(new NuriID
            {
                ID = id,
                AfterID = afterid
            });
        }

        /// <summary>
        /// 7. 통신속도 설정(송신)
        /// </summary>
        /// <param name="arg"></param>
        public void PROT_SettingBaudrate(NuriBaudrate arg)
        {
            BuildProtocol(arg.ID, 0x03, 0x07, new byte[1] { GetBaudrateProtocol(arg.Baudrate) });
        }

        /// <summary>
        /// 7. 통신속도 설정(송신) 
        /// </summary>
        /// <param name="id">장비 아이디</param>
        /// <param name="bps">통신속도 bps</param>
        public void SettingBaudrate(byte id, int bps)
        {
            PROT_SettingBaudrate(new NuriBaudrate
            {
                ID = id,
                Baudrate = bps
            });
        }

        /// <summary>
        /// 8. 통신응답시간 설정(송신)
        /// </summary>
        /// <param name="arg"></param>
        public void PROT_SettingResponsetime(NuriResponsetime arg)
        {
            BuildProtocol(arg.ID, 0x03, 0x08, new byte[1] { (byte)(arg.Responsetime / 100) });
        }

        /// <summary>
        /// 8. 통신응답시간 설정(송신)
        /// </summary>
        /// <param name="id">장비 아이디</param>
        /// <param name="response">통신응답시간(us)</param>
        public void SettingResponsetime(byte id, short response)
        {
            PROT_SettingResponsetime(new NuriResponsetime
            {
                ID = id,
                Responsetime = response
            });
        }

        /// <summary>
        /// 9.감속비설정(송신)
        /// </summary>
        /// <param name="arg"></param>
        public void PROT_SettingRatio(NuriRatio arg)
        {
            byte[] data = new byte[2];
            var tmpspd = BitConverter.GetBytes((ushort)Math.Round(arg.Ratio / 0.1f)).Reverse().ToArray();
            Buffer.BlockCopy(tmpspd, 0, data, 0, 2);
            BuildProtocol(arg.ID, 0x04, 0x09, data);
        }

        /// <summary>
        /// 9.감속비설정(송신)
        /// </summary>
        /// <param name="id">장비 아이디</param>
        /// <param name="ratio">감속비</param>
        public void SettingRatio(byte id, float ratio)
        {
            PROT_SettingRatio(new NuriRatio
            {
                ID = id,
                Ratio = ratio
            });
        }

        /// <summary>
        /// 10. 제어 On/Off 설정(송신)
        /// </summary>
        /// <param name="arg"></param>
        public void PROT_SettingControlOnOff(NuriControlOnOff arg)
        {
            BuildProtocol(arg.ID, 0x03, 0x0A, new byte[1] { (byte)(arg.IsCtrlOn ? 0x00 : 0x01) });
        }

        /// <summary>
        /// 10. 제어 On/Off 설정(송신)
        /// </summary>
        /// <param name="id">장비 아이디</param>
        /// <param name="isCtrlOn">제어 On/Off</param>
        public void SettingControlOnOff(byte id, bool isCtrlOn)
        {
            PROT_SettingControlOnOff(new NuriControlOnOff
            {
                ID = id,
                IsCtrlOn = isCtrlOn
            });
        }

        /// <summary>
        /// 11. 위치제어 모드 설정(송신)
        /// </summary>
        /// <param name="arg"></param>
        public void PROT_SettingPositionControl(NuriPositionCtrl arg)
        {
            BuildProtocol(arg.ID, 0x03, 0x0B, new byte[1] { (byte)(arg.IsAbsolutePotionCtrl ? 0x00 : 0x01) });
        }

        /// <summary>
        /// 11. 위치제어 모드 설정(송신)
        /// </summary>
        /// <param name="id">장비 아이디</param>
        /// <param name="isAbsolute">절대위치여부</param>
        public void SettingPositionControl(byte id, bool isAbsolute)
        {
            PROT_SettingPositionControl(new NuriPositionCtrl
            {
                ID = id,
                IsAbsolutePotionCtrl = isAbsolute
            });
        }

        /// <summary>
        /// 12. 위치초기화(송신)
        /// </summary>
        /// <param name="id">장비 아이디</param>
        public void PROT_ResetPostion(NuriProtocol arg)
        {
            BuildProtocol(arg.ID, 0x02, 0x0C, new byte[] { });
        }

        /// <summary>
        /// 12. 위치초기화(송신)
        /// </summary>
        /// <param name="id">장비 아이디</param>
        public void ResetPostion(byte id)
        {
            PROT_ResetPostion(new NuriProtocol
            {
                ID = id
            });
        }

        /// <summary>
        /// 13. 공장초기화(송신)
        /// </summary>
        /// <param name="id">장비아이디</param>
        public void PROT_ResetFactory(NuriProtocol arg)
        {
            BuildProtocol(arg.ID, 0x02, 0x0D, new byte[] { });
        }

        /// <summary>
        /// 13. 공장초기화(송신)
        /// </summary>
        /// <param name="id">장비아이디</param>
        public void ResetFactory(byte id)
        {
            PROT_ResetFactory(new NuriProtocol
            {
                ID = id
            });
        }

        /// <summary>
        /// 14. 피드백 요청(송신)
        /// </summary>
        /// <param name="arg"></param>
        public void PROT_Feedback(NuriProtocol arg)
        {
            if (arg.Protocol >= (byte)ProtocolMode.REQPing &&
                arg.Protocol <= (byte)ProtocolMode.REQFirmware)
            {
                BuildProtocol(arg.ID, 0x02, (byte)arg.Protocol, new byte[] { });
            }
        }

        /// <summary>
        /// 14. 피드백 요청(송신)
        /// </summary>
        /// <param name="id">장비아이디</param>
        /// <param name="mode">피드백 모드</param>
        public void Feedback(byte id, byte mode)
        {
            PROT_Feedback(new NuriProtocol
            {
                ID = id,
                Protocol = mode
            });
        }


        /// <summary>
        /// 15. Ping(수신)
        /// 테스트 개발용
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="isSend">전송여부 기본은 전송안함</param>
        public void PROT_FeedbackPing(NuriProtocol arg, bool isSend = false)
        {
            BuildProtocol(arg.ID, 0x02, (byte)ProtocolModeRSA.FEEDPing, new byte[] { }, isSend);
        }

        /// <summary>
        /// 16. 위치피드백(수신)
        /// 테스트 개발용
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="isSend">전송여부 기본은 전송안함</param>
        public void PROT_FeedbackPOS(NuriPosSpeedAclCtrl arg, bool isSend = false)
        {
            byte[] data = new byte[6];
            data[0] = (byte)(arg.Direction == Direction.CCW ? 0x00 : 0x01);
            var tmppos = BitConverter.GetBytes((ushort)(arg.Pos / 0.01f)).Reverse().ToArray();
            Buffer.BlockCopy(tmppos, 0, data, 1, 2);
            var tmpspd = BitConverter.GetBytes((ushort)(arg.Speed / 0.1f)).Reverse().ToArray();
            Buffer.BlockCopy(tmpspd, 0, data, 3, 2);
            data[5] = (byte)(arg.Current / 100);
            BuildProtocol(arg.ID, 0x08, 0xD1, data, isSend);
        }

        /// <summary>
        /// 17. 속도피드백(수신)
        /// 테스트 개발용
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="isSend"></param>
        public void PROT_FeedbackSpeed(NuriPosSpeedAclCtrl arg, bool isSend = false)
        {
            byte[] data = new byte[6];
            data[0] = (byte)(arg.Direction == Direction.CCW ? 0x00 : 0x01);
            var tmpspd = BitConverter.GetBytes((ushort)Math.Round(arg.Speed / 0.1f)).Reverse().ToArray();
            Buffer.BlockCopy(tmpspd, 0, data, 1, 2);
            var tmppos = BitConverter.GetBytes((ushort)Math.Round(arg.Pos / 0.1f)).Reverse().ToArray();
            Buffer.BlockCopy(tmppos, 0, data, 3, 2);
            data[5] = (byte)(arg.Current / 100);
            BuildProtocol(arg.ID, 0x08, 0xD2, data, isSend);
        }

        /// <summary>
        /// 18. 위치제어기 피드백(수신)
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="isSend"></param>
        public void PROT_FeedbackPosControl(NuriPosSpdCtrl arg, bool isSend = false)
        {
            byte[] data = new byte[4];
            data[0] = arg.Kp;
            data[1] = arg.Ki;
            data[2] = arg.kd;
            data[3] = (byte)(arg.Current / 100);
            BuildProtocol(arg.ID, 0x06, 0xD3, data, isSend);
        }

        /// <summary>
        /// 19. 속도제어기 피드백(수신)
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="isSend"></param>
        public void PROT_FeedbackSpeedControl(NuriPosSpdCtrl arg, bool isSend = false)
        {
            byte[] data = new byte[4];
            data[0] = arg.Kp;
            data[1] = arg.Ki;
            data[2] = arg.kd;
            data[3] = (byte)(arg.Current / 100);
            BuildProtocol(arg.ID, 0x06, 0xD4, data, isSend);
        }

        /// <summary>
        /// 20. 통신응답시간 피드백(수신)
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="isSend"></param>
        public void PROT_FeedbackResponsetime(NuriResponsetime arg, bool isSend = false)
        {
            BuildProtocol(arg.ID, 0x03, 0xD5, new byte[1] { (byte)(arg.Responsetime / 100) }, isSend);
        }

        /// <summary>
        /// 21. 감속비 피드백(수신)
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="isSend"></param>
        public void PROT_FeedbackRatio(NuriRatio arg, bool isSend = false)
        {
            byte[] data = new byte[2];
            var tmpspd = BitConverter.GetBytes((ushort)Math.Round(arg.Ratio / 0.1f)).Reverse().ToArray();
            Buffer.BlockCopy(tmpspd, 0, data, 0, 2);
            BuildProtocol(arg.ID, 0x04, 0xD6, data, isSend);
        }

        /// <summary>
        /// 22. 제어 On/Off 피드백(수신)
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="isSend"></param>
        public void PROT_FeedbackControlOnOff(NuriControlOnOff arg, bool isSend = false)
        {
            BuildProtocol(arg.ID, 0x03, 0xD7,
                new byte[1] { (byte)(arg.IsCtrlOn ? 0x00 : 0x01) }, isSend);
        }

        /// <summary>
        /// 23. 위치제어 모드 피드백(수신)
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="isSend"></param>
        public void PROT_FeedbackPositionControl(NuriPositionCtrl arg, bool isSend = false)
        {
            BuildProtocol(arg.ID, 0x03, 0xD8,
                new byte[1] { (byte)(arg.IsAbsolutePotionCtrl ? 0x00 : 0x01) }, isSend);
        }

        /// <summary>
        /// 24. 펌웨어 버전 피드백(수신)
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="isSend"></param>
        public void PROT_FeedbackFirmware(NuriVersion arg, bool isSend = false)
        {
            BuildProtocol(arg.ID, 0x03, 0xFD,
                new byte[1] { arg.Version }, isSend);
        }
    }
}
