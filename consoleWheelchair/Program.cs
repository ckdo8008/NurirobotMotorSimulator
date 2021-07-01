using LibNuriMotor;
using LibNuriMotor.Struct;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace consoleWheelchair
{
    class Program
    {

        static SerialControl isc;
        static CancellationTokenSource mCTS;
        static List<PosSpeedAcl> recvDatas  = new List<PosSpeedAcl>();

        static int GetTimeout(string baud)
        {
            int ret = 210;
            // 처리지연에 의한 대기시간 보정상수
            int constWait = 55;
            //int constWait = 120;
            //int constWait = 40;

            switch (baud)
            {
                case "4800":
                    ret = (int)Math.Ceiling(((20f) / (4800f / 10f)) * 1000f);
                    break;
                case "9600":
                    ret = (int)Math.Ceiling(((20f) / (9600f / 10f)) * 1000f);
                    break;
                case "19200":
                    ret = (int)Math.Ceiling(((20f) / (19200f / 10f)) * 1000f);
                    break;
                case "28800":
                    ret = (int)Math.Ceiling(((20f) / (28800f / 10f)) * 1000f);
                    break;
                case "38400":
                    ret = (int)Math.Ceiling(((20f) / (38400f / 10f)) * 1000f);
                    break;
                case "57600":
                    ret = (int)Math.Ceiling(((20f) / (57600f / 10f)) * 1000f);
                    break;
                case "76800":
                    ret = (int)Math.Ceiling(((20f) / (76800f / 10f)) * 1000f);
                    break;
                case "115200":
                    ret = (int)Math.Ceiling(((20f) / (115200f / 10f)) * 1000f);
                    break;
                case "128000":
                    ret = (int)Math.Ceiling(((20f) / (128000f / 10f)) * 1000f);
                    break;
                case "153600":
                    ret = (int)Math.Ceiling(((20f) / (153600f / 10f)) * 1000f);
                    break;
                case "230400":
                    ret = (int)Math.Ceiling(((20f) / (230400f / 10f)) * 1000f);
                    break;
                case "250000":
                    ret = (int)Math.Ceiling(((20f) / (250000f / 10f)) * 1000f);
                    break;
                case "256000":
                    ret = (int)Math.Ceiling(((20f) / (256000f / 10f)) * 1000f);
                    break;
                case "460800":
                    ret = (int)Math.Ceiling(((20f) / (460800f / 10f)) * 1000f);
                    break;
                case "500000":
                    ret = (int)Math.Ceiling(((20f) / (500000f / 10f)) * 1000f);
                    break;
                case "921600":
                    ret = (int)Math.Ceiling(((20f) / (921600f / 10f)) * 1000f);
                    break;
                case "1000000":
                    ret = (int)Math.Ceiling(((20f) / (1000000f / 10f)) * 1000f);
                    break;
                default:
                    ret = 500;
                    break;
            }

            return (int)(ret + constWait);
        }

        static void Run(string comport)
        {
            Console.WriteLine("Device Start =======================");
            isc = new SerialControl();
            CompositeDisposable comdis = null;
            NurirobotMC mc = new NurirobotMC();
            NurirobotMC mcctrl = new NurirobotMC();
            NurirobotMC recvmc = new NurirobotMC();
            Stopwatch sw = new Stopwatch();
            float prevSpeed = 0f;
            float alphavalue = .5f;
            float currSpeed = 0f;
            long changetick = DateTime.Now.Ticks;
            float[] speeds = new float[]{ 0, 10, 20, 30, 40, 50, 60, 70, 60, 50, 30, 20, 10, 0};
            int spdIdx = 0;
            try
            {
                AutoResetEvent mStopWaitHandle = new AutoResetEvent(false);
                comdis = new CompositeDisposable();
                mCTS.Token.ThrowIfCancellationRequested();

                isc.Init(new SerialPortSetting
                {
                    Baudrate = (Baudrate)int.Parse("250000"),
                    DataBits = 8,
                    Handshake = Handshake.None,
                    Parity = Parity.None,
                    PortName = comport,
                    ReadTimeout = 1000,
                    StopBits = StopBits.One,
                    WriteTimeout = 1000
                });
                isc.Connect().Wait();
                if (isc.IsOpen)
                {
                    Console.WriteLine("장치 초기화 중...");
                    isc.ObsProtocolReceived
                        .Subscribe(data =>
                        {
                            try
                            {
                                //Console.WriteLine(string.Format("{0}\t Recv : {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), BitConverter.ToString(data).Replace("-", "")));
                                if (recvmc.Parse(data))
                                {
                                    if (string.Equals(recvmc.PacketName, "FEEDSpeed"))
                                    {
                                        var obj = (NuriPosSpeedAclCtrl)recvmc.GetDataStruct();
                                        var ticks = DateTime.Now.Ticks;
                                        //recvDatas.Add(new PosSpeedAcl
                                        //{
                                        //    Ticks = ticks,
                                        //    Current = obj.Current,
                                        //    POS = obj.Pos,
                                        //    Speed = obj.Speed
                                        //});
                                        //long basetime = ticks - TimeSpan.TicksPerMillisecond * 100 * 5;
                                        //recvDatas.RemoveAll(x => x.Ticks < basetime);

                                        //var lstTicks = (from x in recvDatas
                                        //                select (double)(x.Ticks / TimeSpan.TicksPerMillisecond / 100)).ToArray();
                                        //var lstCurrent = (from x in recvDatas
                                        //              select (double)x.Speed).ToArray();

                                        //AnalyticsUtils.LinearRegression(
                                        //                        lstTicks,
                                        //                        lstCurrent,
                                        //                        0,
                                        //                        lstCurrent.Length,
                                        //                        out double rsquared,
                                        //                        out double yintercept,
                                        //                        out double slope
                                        //                        );

                                        //Console.WriteLine(string.Format(
                                        //    "{0}\t Direction:{2}\t Current:{1}\t Speed:{3}\t Pos:{4}\t count:{5}\t slope:{6}",
                                        //    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                                        //    obj.Current,
                                        //    obj.Direction,
                                        //    obj.Speed,
                                        //    obj.Pos,
                                        //    recvDatas.Count,
                                        //    slope));
                                        //float speed = alphavalue * prevSpeed + (1 - alphavalue) * obj.Speed;
                                        float speed = obj.Speed;
                                        Console.WriteLine(string.Format(
    "{0}\t Direction:{2}\t Current:{1}\t Speed:{3}\t Pos:{4}\t count:{5}\t{6}",
    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
    obj.Current,
    obj.Direction,
    obj.Speed,
    obj.Pos,
    recvDatas.Count,
    speed));

                                        if (changetick + TimeSpan.TicksPerMillisecond * 1000 < ticks)
                                        {
                                            Console.WriteLine(speed);
                                            currSpeed = speed;
                                            mcctrl.ControlAcceleratedSpeed(0x0, (byte)obj.Direction, currSpeed, 0.1f);
                                            isc.Send(mcctrl.Data);
                                            changetick = ticks;
                                        }
                                        //if (changetick + TimeSpan.TicksPerMillisecond * 500 < ticks)
                                        //{
                                        //    if (currSpeed != speed)
                                        //    {
                                        //        Console.WriteLine(speed);
                                        //        currSpeed = speed;
                                        //        mcctrl.ControlAcceleratedSpeed(0x0, (byte)obj.Direction, currSpeed, 0.1f);
                                        //        isc.Send(mcctrl.Data);
                                        //        changetick = ticks;
                                        //    }
                                        //}

                                            //if (changetick + TimeSpan.TicksPerMillisecond * 500 < ticks)
                                            //{
                                            //    if (currSpeed != speed)
                                            //    {
                                            //        Console.WriteLine(speed);
                                            //        mc.ControlAcceleratedSpeed(0x0, (byte)obj.Direction, speed, 0.1f);
                                            //        isc.Send(mc.Data);
                                            //        currSpeed = speed;
                                            //    }
                                            //    changetick = ticks;
                                            //}
                                            //if (obj.Current > 1000)
                                            //{
                                            //    mc.ControlAcceleratedSpeed(0x0, (byte)obj.Direction, obj.Speed, 0.1f);
                                            //    isc.Send(mc.Data);
                                            //}

                                            //if (slope > 0)
                                            //{
                                            //    mc.ControlAcceleratedSpeed(0x0, (byte)obj.Direction, obj.Speed, 0.1f);
                                            //    isc.Send(mc.Data);
                                            //}



                                    }
                                }
                                mStopWaitHandle.Set();
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex);
                            }
                        }).AddTo(comdis);
                    // 제어 모드
                    mc.SettingControlOnOff(0x0, true);
                    mStopWaitHandle.Reset();
                    isc.Send(mc.Data);
                    Thread.Sleep(100);

                    mc.SettingPositionControl(0x0, true);
                    mStopWaitHandle.Reset();
                    isc.Send(mc.Data);
                    Thread.Sleep(100);

                    mc.SettingControlDirection(0x0, LibNuriMotor.Enum.Direction.CCW);
                    mStopWaitHandle.Reset();
                    isc.Send(mc.Data);
                    Thread.Sleep(100);

                    mc.Feedback(0x0, 0xab);
                    mStopWaitHandle.Reset();
                    isc.Send(mc.Data);
                    Thread.Sleep(100);
                    currSpeed = 0f;
                    mc.ControlAcceleratedSpeed(0x0, (byte)LibNuriMotor.Enum.Direction.CW, currSpeed, 0.1f);
                    mStopWaitHandle.Reset();
                    isc.Send(mc.Data);
                    
                    Task t_GetFeedback = new Task(() =>
                    {
                        try
                        {
                            while (true)
                            {
                                sw.Restart();
                                mCTS.Token.ThrowIfCancellationRequested();

                                mStopWaitHandle.Reset();
                                mc.Feedback(0x0, 0xa2);
                                isc.Send(mc.Data);
                                mStopWaitHandle.WaitOne(100);
                                sw.Stop();
                                if (sw.ElapsedMilliseconds < 100)
                                    Thread.Sleep((int)(100 - sw.ElapsedMilliseconds));
                            }
                        } catch
                        {

                        }
                    });
                    t_GetFeedback.Start();


                    while (true)
                    {
                        mCTS.Token.ThrowIfCancellationRequested();
                        Thread.Sleep(100);
                    }
                } else
                {
                    Console.WriteLine("Device Error =======================");
                }
            }
            catch
            {

            }
            //mc.SettingControlDirection(0x0, LibNuriMotor.Enum.Direction.CW);
            //mc.Data;
        }

        static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            mCTS?.Cancel();
            Thread.Sleep(100);
            isc?.Dispose();
            isc = null;
        }

        static void Main(string[] args)
        {
            mCTS = new CancellationTokenSource();
            Console.CancelKeyPress += Console_CancelKeyPress;
            var devices = DeviceInfo.GetPorts();
            List<string> ports = new List<string>();
            foreach (var item in devices)
            {
                if (!item.IsNowUsing)
                {
                    ports.Add(item.PortName);
                }
            }

            if (ports.Count == 0)
            {
                Console.WriteLine("사용 가능한 포트가 없습니다.");
                return;
            }

            if (args.Length == 1)
            {
                var tmpcom = args[0].ToUpper().Trim();
                Debug.WriteLine(tmpcom);
                if (!ports.Contains(tmpcom))
                {
                    Console.WriteLine("해당 포트가 없습니다.");
                    return;
                }
                else
                    Program.Run(tmpcom);
            }
            else
            {
                Program.Run(ports[0]);
            }

        }
    }

    class DeviceInfo
    {
        public static SerialPortInfo[] GetPorts()
        {
            List<SerialPortInfo> ret = new List<SerialPortInfo>();
            var tmps = SerialPort.GetPortNames();
            if (tmps != null)
            {
                SerialPort sp = new SerialPort();
                foreach (var item in tmps)
                {
                    bool isnowusing = false;
                    try
                    {
                        sp.PortName = item;
                        sp.Open();
                        sp.Close();
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e);
                        isnowusing = true;
                    }

                    ret.Add(new SerialPortInfo { PortName = item, IsNowUsing = isnowusing });
                }
            }

            return ret.ToArray();
        }
    }

    public class SerialPortInfo
    {
        /// <summary>
        /// 시리얼 장치 명칭
        /// </summary>
        public string PortName { get; set; }
        /// <summary>
        /// 현재 사용 중인지 확인
        /// </summary>
        public bool IsNowUsing { get; set; }
    }

    public class PosSpeedAcl
    {
        public long Ticks { get; set; }
        public short Current { get; set; }
        public float POS { get; set; }
        public float Speed { get; set; }
    }
}
