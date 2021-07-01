namespace LibNuriMotor
{
    using LibNuriMotor.Enum;
    using LibNuriMotor.Struct;
    using System;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// Nurirobot Motercontrol-RS485
    /// </summary>
    public class NurirobotMC : ICommand
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
            switch ((ProtocolMode)Data[5])
            {
                case ProtocolMode.CTRLPosSpeed:
                    return new NuriPosSpeedAclCtrl
                    {
                        Protocol = Data[5],
                        ID = Data[2],
                        Direction = (Direction)Data[6],
                        Pos = BitConverter.ToUInt16(Data.Skip(7).Take(2).Reverse().ToArray(), 0) * 0.01f,
                        Speed = BitConverter.ToUInt16(Data.Skip(9).Take(2).Reverse().ToArray(), 0) * 0.1f,
                    };
                case ProtocolMode.CTRLAccPos:
                    return new NuriPosSpeedAclCtrl
                    {
                        Protocol = Data[5],
                        ID = Data[2],
                        Direction = (Direction)Data[6],
                        Pos = BitConverter.ToUInt16(Data.Skip(7).Take(2).Reverse().ToArray(), 0) * 0.01f,
                        Arrivetime = Data[9] * 0.1f
                    };
                case ProtocolMode.CTRLAccSpeed:
                    return new NuriPosSpeedAclCtrl
                    {
                        Protocol = Data[5],
                        ID = Data[2],
                        Direction = (Direction)Data[6],
                        Speed = BitConverter.ToUInt16(Data.Skip(7).Take(2).Reverse().ToArray(), 0) * 0.1f,
                        Arrivetime = Data[9] * 0.1f
                    };
                case ProtocolMode.SETPosCtrl:
                    return new NuriPosSpdCtrl
                    {
                        Protocol = Data[5],
                        ID = Data[2],
                        Kp = Data[6],
                        Ki = Data[7],
                        kd = Data[8],
                        Current = (short)(Data[9] * 100)
                    };
                case ProtocolMode.SETSpeedCtrl:
                    return new NuriPosSpdCtrl
                    {
                        Protocol = Data[5],
                        ID = Data[2],
                        Kp = Data[6],
                        Ki = Data[7],
                        kd = Data[8],
                        Current = (short)(Data[9] * 100)
                    };
                case ProtocolMode.SETID:
                    return new NuriID
                    {
                        Protocol = Data[5],
                        ID = Data[2],
                        AfterID = Data[6]
                    };
                case ProtocolMode.SETBaudrate:
                    return new NuriBaudrate
                    {
                        Protocol = Data[5],
                        ID = Data[2],
                        Baudrate = GetBaudrate(Data[6])
                    };
                case ProtocolMode.SETResptime:
                    return new NuriResponsetime
                    {
                        Protocol = Data[5],
                        ID = Data[2],
                        Responsetime = (short)(Data[6] * 100)
                    };
                case ProtocolMode.SETRatedSPD:
                    return new NuriRatedSpeed
                    {
                        Protocol = Data[5],
                        ID = Data[2],
                        Speed = BitConverter.ToUInt16(Data.Skip(6).Take(2).Reverse().ToArray(), 0)
                    };
                case ProtocolMode.SETResolution:
                    return new NuriResolution
                    {
                        Protocol = Data[5],
                        ID = Data[2],
                        Resolution = BitConverter.ToUInt16(Data.Skip(6).Take(2).Reverse().ToArray(), 0)
                    };
                case ProtocolMode.SETRatio:
                    return new NuriRatio
                    {
                        Protocol = Data[5],
                        ID = Data[2],
                        Ratio = BitConverter.ToUInt16(Data.Skip(6).Take(2).Reverse().ToArray(), 0) * 0.1f
                    };
                case ProtocolMode.SETCtrlOnOff:
                    return new NuriControlOnOff
                    {
                        Protocol = Data[5],
                        ID = Data[2],
                        IsCtrlOn = Data[6] == 0
                    };
                case ProtocolMode.SETPosCtrlMode:
                    return new NuriPositionCtrl
                    {
                        Protocol = Data[5],
                        ID = Data[2],
                        IsAbsolutePotionCtrl = Data[6] == 0
                    };
                case ProtocolMode.SETCtrlDirt:
                    return new NuriCtrlDirection
                    {
                        Protocol = Data[5],
                        ID = Data[2],
                        Direction = (Direction)Data[6]
                    };
                case ProtocolMode.RESETPos:
                    return new NuriProtocol
                    {
                        ID = Data[2],
                        Protocol = Data[5]
                    };
                case ProtocolMode.RESETFactory:
                    return new NuriProtocol
                    {
                        ID = Data[2],
                        Protocol = Data[5]
                    };
                case ProtocolMode.REQPing:
                    return new NuriProtocol
                    {
                        ID = Data[2],
                        Protocol = Data[5]
                    };
                case ProtocolMode.REQPos:
                    return new NuriProtocol
                    {
                        ID = Data[2],
                        Protocol = Data[5]
                    };
                case ProtocolMode.REQSpeed:
                    return new NuriProtocol
                    {
                        ID = Data[2],
                        Protocol = Data[5]
                    };
                case ProtocolMode.REQPosCtrl:
                    return new NuriProtocol
                    {
                        ID = Data[2],
                        Protocol = Data[5]
                    };
                case ProtocolMode.REQSpdCtrl:
                    return new NuriProtocol
                    {
                        ID = Data[2],
                        Protocol = Data[5]
                    };
                case ProtocolMode.REQResptime:
                    return new NuriProtocol
                    {
                        ID = Data[2],
                        Protocol = Data[5]
                    };
                case ProtocolMode.REQRatedSPD:
                    return new NuriProtocol
                    {
                        ID = Data[2],
                        Protocol = Data[5]
                    };
                case ProtocolMode.REQResolution:
                    return new NuriProtocol
                    {
                        ID = Data[2],
                        Protocol = Data[5]
                    };
                case ProtocolMode.REQRatio:
                    return new NuriProtocol
                    {
                        ID = Data[2],
                        Protocol = Data[5]
                    };
                case ProtocolMode.REQCtrlOnOff:
                    return new NuriProtocol
                    {
                        ID = Data[2],
                        Protocol = Data[5]
                    };
                case ProtocolMode.REQPosCtrlMode:
                    return new NuriProtocol
                    {
                        ID = Data[2],
                        Protocol = Data[5]
                    };
                case ProtocolMode.REQCtrlDirt:
                    return new NuriProtocol
                    {
                        ID = Data[2],
                        Protocol = Data[5]
                    };
                case ProtocolMode.REQFirmware:
                    return new NuriProtocol
                    {
                        ID = Data[2],
                        Protocol = Data[5]
                    };
                case ProtocolMode.FEEDPing:
                    return new NuriProtocol
                    {
                        ID = Data[2],
                        Protocol = Data[5]
                    };
                case ProtocolMode.FEEDPos:
                    return new NuriPosSpeedAclCtrl
                    {
                        ID = Data[2],
                        Protocol = Data[5],
                        Direction = (Direction)Data[6],
                        Pos = BitConverter.ToUInt16(Data.Skip(7).Take(2).Reverse().ToArray(), 0) * 0.01f,
                        Speed = BitConverter.ToUInt16(Data.Skip(9).Take(2).Reverse().ToArray(), 0) * 0.1f,
                        Current = (short)(Data[11] * 100)
                    };
                case ProtocolMode.FEEDSpeed:
                    return new NuriPosSpeedAclCtrl
                    {
                        ID = Data[2],
                        Protocol = Data[5],
                        Direction = (Direction)Data[6],
                        Speed = BitConverter.ToUInt16(Data.Skip(7).Take(2).Reverse().ToArray(), 0) * 0.1f,
                        Pos = BitConverter.ToUInt16(Data.Skip(9).Take(2).Reverse().ToArray(), 0) * 0.1f,
                        Current = (short)(Data[11] * 100)
                    };
                case ProtocolMode.FEEDPosCtrl:
                    return new NuriPosSpdCtrl
                    {
                        ID = Data[2],
                        Protocol = Data[5],
                        Kp = Data[6],
                        Ki = Data[7],
                        kd = Data[8],
                        Current = (short)(Data[9] * 100)
                    };
                case ProtocolMode.FEEDSpdCtrl:
                    return new NuriPosSpdCtrl
                    {
                        ID = Data[2],
                        Protocol = Data[5],
                        Kp = Data[6],
                        Ki = Data[7],
                        kd = Data[8],
                        Current = (short)(Data[9] * 100)
                    };
                case ProtocolMode.FEEDResptime:
                    return new NuriResponsetime
                    {
                        Protocol = Data[5],
                        ID = Data[2],
                        Responsetime = (short)(Data[6] * 100)
                    };
                case ProtocolMode.FEEDRatedSPD:
                    return new NuriRatedSpeed
                    {
                        Protocol = Data[5],
                        ID = Data[2],
                        Speed = BitConverter.ToUInt16(Data.Skip(6).Take(2).Reverse().ToArray(), 0)
                    };
                case ProtocolMode.FEEDResolution:
                    return new NuriResolution
                    {
                        Protocol = Data[5],
                        ID = Data[2],
                        Resolution = BitConverter.ToUInt16(Data.Skip(6).Take(2).Reverse().ToArray(), 0)
                    };
                case ProtocolMode.FEEDRatio:
                    return new NuriRatio
                    {
                        Protocol = Data[5],
                        ID = Data[2],
                        Ratio = BitConverter.ToUInt16(Data.Skip(6).Take(2).Reverse().ToArray(), 0) * 0.1f
                    };
                case ProtocolMode.FEEDCtrlOnOff:
                    return new NuriControlOnOff
                    {
                        Protocol = Data[5],
                        ID = Data[2],
                        IsCtrlOn = Data[6] == 0
                    };
                case ProtocolMode.FEEDPosCtrlMode:
                    return new NuriPositionCtrl
                    {
                        Protocol = Data[5],
                        ID = Data[2],
                        IsAbsolutePotionCtrl = Data[6] == 0
                    };
                case ProtocolMode.FEEDCtrlDirt:
                    return new NuriCtrlDirection
                    {
                        Protocol = Data[5],
                        ID = Data[2],
                        Direction = (Direction)Data[6]
                    };
                case ProtocolMode.FEEDFirmware:
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
                Data = new byte[data.Length];
                Buffer.BlockCopy(data, 0, Data, 0, Data.Length);

                if (Data[3] + 4 != data.Length)
                    return ret;

                var chksum = GetCheckSum();
                if (Data[4] == chksum)
                {
                    PacketName = ((ProtocolMode)Data[5]).ToString("G");
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
        /// 9. 모터 정격속도 설정(송신)
        /// </summary>
        /// <param name="arg"></param>
        public void PROT_SettingRatedspeed(NuriRatedSpeed arg)
        {
            byte[] data = new byte[2];
            var tmpspd = BitConverter.GetBytes(arg.Speed).Reverse().ToArray();
            Buffer.BlockCopy(tmpspd, 0, data, 0, 2);
            BuildProtocol(arg.ID, 0x04, 0x09, data);
        }

        /// <summary>
        /// 9. 모터 정격속도 설정(송신)
        /// </summary>
        /// <param name="id">장비 아이디</param>
        /// <param name="spd">모터 정격속도 RPM</param>
        public void SettingRatedspeed(byte id, ushort spd)
        {
            PROT_SettingRatedspeed(new NuriRatedSpeed
            {
                ID = id,
                Speed = spd
            });
        }

        /// <summary>
        /// 10. 분해능 설정(송신)
        /// </summary>
        /// <param name="arg"></param>
        public void PROT_SettingResolution(NuriResolution arg)
        {
            byte[] data = new byte[2];
            var tmpspd = BitConverter.GetBytes(arg.Resolution).Reverse().ToArray();
            Buffer.BlockCopy(tmpspd, 0, data, 0, 2);
            BuildProtocol(arg.ID, 0x04, 0x0A, data);
        }

        /// <summary>
        /// 10. 분해능 설정(송신
        /// </summary>
        /// <param name="id">장비 아이디</param>
        /// <param name="res">분해능</param>
        public void SettingResolution(byte id, ushort res)
        {
            PROT_SettingResolution(new NuriResolution
            {
                ID = id,
                Resolution = res
            });
        }

        /// <summary>
        /// 11.감속비설정(송신)
        /// </summary>
        /// <param name="arg"></param>
        public void PROT_SettingRatio(NuriRatio arg)
        {
            byte[] data = new byte[2];
            var tmpspd = BitConverter.GetBytes((ushort)Math.Round(arg.Ratio / 0.1f)).Reverse().ToArray();
            Buffer.BlockCopy(tmpspd, 0, data, 0, 2);
            BuildProtocol(arg.ID, 0x04, 0x0B, data);
        }

        /// <summary>
        /// 11.감속비설정(송신)
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
        /// 12. 제어 On/Off 설정(송신)
        /// </summary>
        /// <param name="arg"></param>
        public void PROT_SettingControlOnOff(NuriControlOnOff arg)
        {
            BuildProtocol(arg.ID, 0x03, 0x0C, new byte[1] { (byte)(arg.IsCtrlOn ? 0x00 : 0x01) });
        }

        /// <summary>
        /// 12. 제어 On/Off 설정(송신)
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
        /// 13. 위치제어 모드 설정(송신)
        /// </summary>
        /// <param name="arg"></param>
        public void PROT_SettingPositionControl(NuriPositionCtrl arg)
        {
            BuildProtocol(arg.ID, 0x03, 0x0D, new byte[1] { (byte)(arg.IsAbsolutePotionCtrl ? 0x00 : 0x01) });
        }

        /// <summary>
        /// 13. 위치제어 모드 설정(송신)
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
        /// 14. 제어 방향 설정(송신)
        /// </summary>
        /// <param name="arg"></param>
        public void PROT_SettingControlDirection(NuriCtrlDirection arg)
        {
            BuildProtocol(arg.ID, 0x03, 0x0E,
                new byte[1] { (byte)(arg.Direction == Direction.CCW ? 0x00 : 0x01) });
        }

        /// <summary>
        /// 14. 제어 방향 설정(송신)
        /// </summary>
        /// <param name="id">장비 아이디</param>
        /// <param name="direction">제어방향</param>
        public void SettingControlDirection(byte id, Direction direction)
        {
            PROT_SettingControlDirection(new NuriCtrlDirection
            {
                ID = id,
                Direction = direction
            });
        }

        /// <summary>
        /// 15. 위치초기화(송신)
        /// </summary>
        /// <param name="id">장비 아이디</param>
        public void PROT_ResetPostion(NuriProtocol arg)
        {
            BuildProtocol(arg.ID, 0x02, 0x0F, new byte[] { });
        }

        /// <summary>
        /// 15. 위치초기화(송신)
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
        /// 16. 공장초기화(송신)
        /// </summary>
        /// <param name="id">장비아이디</param>
        public void PROT_ResetFactory(NuriProtocol arg)
        {
            BuildProtocol(arg.ID, 0x02, 0x10, new byte[] { });
        }

        /// <summary>
        /// 16. 공장초기화(송신)
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
        /// 17. 피드백 요청(송신)
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
        /// 17. 피드백 요청(송신)
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
        /// 18. Ping(수신)
        /// 테스트 개발용
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="isSend">전송여부 기본은 전송안함</param>
        public void PROT_FeedbackPing(NuriProtocol arg, bool isSend = false)
        {
            BuildProtocol(arg.ID, 0x02, (byte)ProtocolMode.FEEDPing, new byte[] { }, isSend);
        }

        /// <summary>
        /// 19. 위치피드백(수신)
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
        /// 20. 속도피드백(수신)
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
        /// 21. 위치제어기 피드백(수신)
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
        /// 22. 속도제어기 피드백(수신)
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
        /// 23. 통신응답시간 피드백(수신)
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="isSend"></param>
        public void PROT_FeedbackResponsetime(NuriResponsetime arg, bool isSend = false)
        {
            BuildProtocol(arg.ID, 0x03, 0xD5, new byte[1] { (byte)(arg.Responsetime / 100) }, isSend);
        }

        /// <summary>
        /// 24. 모터 정격속도 피드백(수신)
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="isSend"></param>
        public void PROT_FeedbackRatedSpeed(NuriRatedSpeed arg, bool isSend = false)
        {
            byte[] data = new byte[2];
            var tmpspd = BitConverter.GetBytes(arg.Speed).Reverse().ToArray();
            Buffer.BlockCopy(tmpspd, 0, data, 0, 2);
            BuildProtocol(arg.ID, 0x04, 0xD6, data, isSend);
        }

        /// <summary>
        /// 25. 분해능 피드백(수신)
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="isSend"></param>
        public void PROT_FeedbackResolution(NuriResolution arg, bool isSend = false)
        {
            byte[] data = new byte[2];
            var tmpspd = BitConverter.GetBytes(arg.Resolution).Reverse().ToArray();
            Buffer.BlockCopy(tmpspd, 0, data, 0, 2);
            BuildProtocol(arg.ID, 0x04, 0xD7, data, isSend);
        }

        /// <summary>
        /// 26. 감속비 피드백(수신)
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="isSend"></param>
        public void PROT_FeedbackRatio(NuriRatio arg, bool isSend = false)
        {
            byte[] data = new byte[2];
            var tmpspd = BitConverter.GetBytes((ushort)Math.Round(arg.Ratio / 0.1f)).Reverse().ToArray();
            Buffer.BlockCopy(tmpspd, 0, data, 0, 2);
            BuildProtocol(arg.ID, 0x04, 0xD8, data, isSend);
        }

        /// <summary>
        /// 27. 제어 On/Off 피드백(수신)
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="isSend"></param>
        public void PROT_FeedbackControlOnOff(NuriControlOnOff arg, bool isSend = false)
        {
            BuildProtocol(arg.ID, 0x03, 0xD9,
                new byte[1] { (byte)(arg.IsCtrlOn ? 0x00 : 0x01) }, isSend);
        }

        /// <summary>
        /// 28. 위치제어 모드 피드백(수신)
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="isSend"></param>
        public void PROT_FeedbackPositionControl(NuriPositionCtrl arg, bool isSend = false)
        {
            BuildProtocol(arg.ID, 0x03, 0xDA,
                new byte[1] { (byte)(arg.IsAbsolutePotionCtrl ? 0x00 : 0x01) }, isSend);
        }

        /// <summary>
        /// 29. 제어 방향 피드백(수신)
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="isSend"></param>
        public void PROT_FeedbackControlDirection(NuriCtrlDirection arg, bool isSend = false)
        {
            BuildProtocol(arg.ID, 0x03, 0xDB,
                new byte[1] { (byte)(arg.Direction == Direction.CCW ? 0x00 : 0x01) }, isSend);
        }

        /// <summary>
        /// 30. 펌웨어 버전 피드백(수신)
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
