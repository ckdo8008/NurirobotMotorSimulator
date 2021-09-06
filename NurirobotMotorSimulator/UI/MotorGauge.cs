using LibNuriMotor;
using LibNuriMotor.Enum;
using LibNuriMotor.Struct;
using NetMQ;
using NetMQ.Sockets;
using NurirobotMotorSimulator.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Composition;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Shapes;

namespace NurirobotMotorSimulator.UI
{
    [TemplatePart(Name = ContainerPartName, Type = typeof(Grid))]
    [TemplatePart(Name = SpeedPartName, Type = typeof(Path))]
    [TemplatePart(Name = SpeedBasePartName, Type = typeof(Path))]
    [TemplatePart(Name = CurrentPartName, Type = typeof(Path))]
    [TemplatePart(Name = CurrentBasePartName, Type = typeof(Path))]
    [TemplatePart(Name = MoterIDName, Type = typeof(ComboBox))]
    [TemplatePart(Name = MoterTypeName, Type = typeof(ComboBox))]
    public sealed class MotorGauge : Control
    {

        public static readonly DependencyProperty POSValueProperty =
    DependencyProperty.Register(
        nameof(POSValue),
        typeof(double),
        typeof(MotorGauge),
        new PropertyMetadata(
            0.0,
            new PropertyChangedCallback(OnPOSPropertyChanged)));

        public static readonly DependencyProperty SpeedValueProperty =
    DependencyProperty.Register(
        nameof(SpeedValue),
        typeof(double),
        typeof(MotorGauge),
        new PropertyMetadata(
            0.0,
            new PropertyChangedCallback(OnSpeedPropertyChanged)));

        public static readonly DependencyProperty CurrentValueProperty =
    DependencyProperty.Register(
        nameof(CurrentValue),
        typeof(double),
        typeof(MotorGauge),
        new PropertyMetadata(
            0.0,
            new PropertyChangedCallback(OnCurrentPropertyChanged)));

        public double POSValue {
            get { return (double)GetValue(POSValueProperty); }
            set { SetValue(POSValueProperty, value); }
        }

        public double SpeedValue
        {
            get { return (double)GetValue(SpeedValueProperty); }
            set { SetValue(SpeedValueProperty, value); }
        }

        public double CurrentValue
        {
            get { return (double)GetValue(CurrentValueProperty); }
            set { SetValue(CurrentValueProperty, value); }
        }

        public static readonly DependencyProperty POSneedleBrushProperty =
    DependencyProperty.Register(nameof(POSneedleBrush), typeof(SolidColorBrush), typeof(MotorGauge), new PropertyMetadata(null));

        public SolidColorBrush POSneedleBrush
        {
            get { return (SolidColorBrush)GetValue(POSneedleBrushProperty); }
            set { SetValue(POSneedleBrushProperty, value); }
        }

        public static readonly DependencyProperty POSTickBrushProperty =
DependencyProperty.Register(nameof(POSTickBrush), typeof(SolidColorBrush), typeof(MotorGauge), new PropertyMetadata(null));

        public SolidColorBrush POSTickBrush
        {
            get { return (SolidColorBrush)GetValue(POSTickBrushProperty); }
            set { SetValue(POSTickBrushProperty, value); }
        }

        public static readonly DependencyProperty SpeedTickBrushProperty =
DependencyProperty.Register(nameof(SpeedTickBrush), typeof(SolidColorBrush), typeof(MotorGauge), new PropertyMetadata(null));

        public SolidColorBrush SpeedTickBrush
        {
            get { return (SolidColorBrush)GetValue(SpeedTickBrushProperty); }
            set { SetValue(SpeedTickBrushProperty, value); }
        }

        public static readonly DependencyProperty SpeedneedleBrushProperty =
DependencyProperty.Register(nameof(SpeedneedleBrush), typeof(SolidColorBrush), typeof(MotorGauge), new PropertyMetadata(null));

        public SolidColorBrush SpeedneedleBrush
        {
            get { return (SolidColorBrush)GetValue(SpeedneedleBrushProperty); }
            set { SetValue(SpeedneedleBrushProperty, value); }
        }

        public static readonly DependencyProperty CurrentneedleBrushProperty =
DependencyProperty.Register(nameof(CurrentneedleBrush), typeof(SolidColorBrush), typeof(MotorGauge), new PropertyMetadata(null));

        public SolidColorBrush CurrentneedleBrush
        {
            get { return (SolidColorBrush)GetValue(CurrentneedleBrushProperty); }
            set { SetValue(CurrentneedleBrushProperty, value); }
        }

        public static readonly DependencyProperty TrailBrushProperty =
    DependencyProperty.Register(nameof(TrailBrush), typeof(Brush), typeof(MotorGauge), new PropertyMetadata(null));

        public static readonly DependencyProperty ScaleBrushProperty =
            DependencyProperty.Register(nameof(ScaleBrush), typeof(Brush), typeof(MotorGauge), new PropertyMetadata(null));

        public Brush TrailBrush
        {
            get { return (Brush)GetValue(TrailBrushProperty); }
            set { SetValue(TrailBrushProperty, value); }
        }

        public Brush ScaleBrush
        {
            get { return (Brush)GetValue(ScaleBrushProperty); }
            set { SetValue(ScaleBrushProperty, value); }
        }

        public static readonly DependencyProperty SpeedTickWidthProperty =
            DependencyProperty.Register(nameof(SpeedTickWidth), typeof(double), typeof(MotorGauge), new PropertyMetadata(26.0));

        public static readonly DependencyProperty MotorRPMProperty =
    DependencyProperty.Register(nameof(MotorRPM), typeof(int), typeof(MotorGauge), new PropertyMetadata(3000));

        public int MotorRPM
        {
            get { return (int)GetValue(MotorRPMProperty); }
            set { SetValue(MotorRPMProperty, value); }
        }

        public double SpeedTickWidth
        {
            get { return (double)GetValue(SpeedTickWidthProperty); }
            set { SetValue(SpeedTickWidthProperty, value); }
        }

        public static readonly DependencyProperty MotorIDProperty =
    DependencyProperty.Register(nameof(MotorID), typeof(int), typeof(MotorGauge), new PropertyMetadata(1));
        //static string CustMID = "0";
        public int MotorID
        {
            get { return (int)GetValue(MotorIDProperty); }
            set {
                SetValue(MotorIDProperty, value);

                if (_MotorID != null)
                {
                    _MotorID.SelectedItem = _MotorID.Items.Where(
                        x => ((ComboBoxItem)x).Content.ToString().Equals(MotorID.ToString())
                        ).FirstOrDefault();
                }
            }
        }

        public static readonly DependencyProperty MotorTypeProperty =
DependencyProperty.Register(nameof(MotorType), typeof(string), typeof(MotorGauge), new PropertyMetadata("MC"));

        public string MotorType
        {
            get { return (string)GetValue(MotorTypeProperty); }
            set {
                SetValue(MotorTypeProperty, value);
                if (_MotorType != null)
                {
                    _MotorType.SelectedItem = _MotorType.Items.Where(x => x.Equals(MotorType)).FirstOrDefault();
                }
            }
        }

        /// <summary>
        /// 역회전 여부
        /// </summary>
        private volatile bool IsReverse = false;
        private void SetIsReverse(bool arg)
        {
            IsReverse = arg;
        }

        public bool IsRunning
        {
            get;
            set;
        } = false;

        public bool IsStop
        {
            get;
            set;
        } = false;

        private volatile bool IsWantPOS = false;
        private void SetIsWantPOS(bool arg) => IsWantPOS = arg;

        //private volatile bool IsWantSpeedPOS = false;
        //private void SetIsWantSpeedPOS(bool arg) => IsWantSpeedPOS = arg;

        public double POSStep
        {
            get;
            set;
        } = 0d;

        public double RemainingTime
        {
            get;
            set;
        } = 1000 * 2;

        private DateTime LastTime
        {
            get;
            set;
        } = DateTime.Now;

        private const double Degrees2Radians = Math.PI / 180;

        private static readonly ThemeListener ThemeListener = new ThemeListener();
        private SolidColorBrush _POSneedleBrush;
        private SolidColorBrush _SpeedneedleBrush;
        private SolidColorBrush _CurrentneedleBrush;
        private SolidColorBrush _POSTickBrush;
        private SolidColorBrush _SpeedTickBrush;

        private Brush _trailBrush;
        private Brush _scaleBrush;
        private SolidColorBrush _scaleTickBrush;
        private SolidColorBrush _tickBrush;
        private Brush _foreground;

        private Compositor _compositor;
        private ContainerVisual _root;
        private SpriteVisual _POSneedle;
        private SpriteVisual _Speedneedle;
        private SpriteVisual _Currentneedle;

        private ComboBox _MotorID = null;
        private ComboBox _MotorType = null;

        private const string ContainerPartName = "PART_Container";
        private const string SpeedPartName = "PART_Speed";
        private const string SpeedBasePartName = "PART_SpeedBase";

        private const string CurrentPartName = "PART_Current";
        private const string CurrentBasePartName = "PART_CurrentBase";

        private const string MoterIDName = "PART_MoterID";
        private const string MoterTypeName = "PART_MotorType";

        private DispatcherTimer _Timer;
        private SubscriberSocket subscriber;
        private PublisherSocket publisherSocket;
        private CancellationTokenSource mCTS;

        /// <summary>
        /// 위치제어
        /// </summary>
        private volatile bool IsPosControl = false;
        private void SetIsPosControl (bool arg) => IsPosControl = arg;

        /// <summary>
        /// 제어 온오프 설정
        /// </summary>
        private volatile bool IsControlOnOff = true;
        private void SetIsControlOnOff(bool arg) => IsControlOnOff = arg;

        /// <summary>
        /// 절대위치제어여부
        /// </summary>
        private volatile bool IsAbsolutePotionCtrl = true;
        private void SetIsAbsolutePotionCtrl(bool arg) => IsAbsolutePotionCtrl = arg;

        /// <summary>
        /// 응답 대기 시간
        /// </summary>
        private volatile int ResponseWait = 10;
        private void SetResponseWait(int arg) => ResponseWait = arg;

        private volatile float WantPOS = 0f;
        private void SetWantPOS(float arg) => WantPOS = arg;

        private volatile float WantSpeed = 0f;
        private void SetWantSpeed(float arg) => WantSpeed = arg;

        private volatile float NowPOS = 0f;
        private volatile float NowSpeed = 0f;
        private volatile float NowCurrent = 0f;

        public MotorGauge()
        {
            DefaultStyleKey = typeof(MotorGauge);
            Unloaded += MotorGauge_Unloaded;
            _Timer = new DispatcherTimer();
            _Timer.Interval = TimeSpan.FromMilliseconds(25);
            _Timer.Tick += _Timer_Tick;
            _Timer.Start();
            mCTS = new CancellationTokenSource();
            publisherSocket = new PublisherSocket();
            publisherSocket.Connect("inproc://sendData");
        }
        ~MotorGauge()
        {

        }

        private void Subscriber()
        {
            if (subscriber != null)
            {
                mCTS.Cancel();
                subscriber.Dispose();
                mCTS.Dispose();
            }

            mCTS = new CancellationTokenSource();
            subscriber = new SubscriberSocket();
            subscriber.Connect("inproc://pushData");
            subscriber.SubscribeToAnyTopic();
            string CustMID = MotorID.ToString();
            byte MotorIDNo = (byte)MotorID;
            bool isMc = MotorType.Equals("MC") ? true : false;
            Debug.WriteLine(string.Format("Subscribe : {0}", CustMID));
            Task t = new Task(() =>
            {
                try
                {
                    while (true)
                    {
                        mCTS.Token.ThrowIfCancellationRequested();
                        string topic = subscriber.ReceiveFrameString();
                        if ("ALL".Equals(topic) || CustMID.Equals(topic))
                        {
                            byte[] messageReceived = subscriber.ReceiveFrameBytes();
                            if (isMc)
                                ReciveMC(messageReceived, this);
                            else
                                ReciveRSA(messageReceived, this);
                        }
                    }
                } catch(Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            });
            t.Start();
        }
        
        private byte GetID()
        {
            byte id = 0x0;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var tmp = Dispatcher.RunAsync(CoreDispatcherPriority.High,
               () =>
               {
                   id = (byte)this.MotorID;
               }).AsTask();
            tmp.Wait();
            sw.Stop();
            //Debug.WriteLine(string.Format("Delay : {0}", sw.ElapsedTicks));
            return id;
        }

        private async void ReciveMC(byte[] arg, MotorGauge obj)
        {
            try
            {
                //Debug.WriteLine(string.Format("MC ====== recv : {0}", BitConverter.ToString(arg).Replace("-", "")));
                NurirobotMC mc = new NurirobotMC();
                mc.Parse(arg);
                byte id = GetID();

                if (string.Equals(mc.PacketName, "CTRLPosSpeed"))
                {
                    var objrecv = (NuriPosSpeedAclCtrl)mc.GetDataStruct();
                    var tmp = Dispatcher.RunAsync(CoreDispatcherPriority.High,
                       () =>
                       {
                           SetWantPOS(objrecv.Pos);
                           SetWantSpeed(objrecv.Speed);
                           //SetIsReverse(objrecv.Direction == Direction.CCW ? true : false);
                           SetIsReverse(POSValue >= objrecv.Pos ? true : false);
                           SetIsWantPOS(true);
                           //SetIsWantSpeedPOS(true);

                           LastTime = DateTime.Now;
                           RemainingTime = 0;
                           IsRunning = true;
                       }).AsTask();
                    tmp.Wait();

                    return;
                }
                else if (string.Equals(mc.PacketName, "CTRLAccPos"))
                {
                    var objrecv = (NuriPosSpeedAclCtrl)mc.GetDataStruct();
                    var tmp = Dispatcher.RunAsync(CoreDispatcherPriority.High,
                       () =>
                       {
                           SetWantPOS(objrecv.Pos);

                           //SetWantSpeed(0);
                           float spd = 0f;
                           var high = Math.Max(POSValue, objrecv.Pos);
                           var low = Math.Min(POSValue, objrecv.Pos);
                           var termpos = high - low;
                           spd = (float)(termpos / (objrecv.Arrivetime * 1000f / 100f) * 7f);
                           SetWantSpeed(spd);
                           //SetIsReverse(objrecv.Direction == Direction.CCW ? true : false);
                           SetIsReverse(POSValue >= objrecv.Pos ? true : false);
                           SetIsWantPOS(true);
                           //SetIsWantSpeedPOS(false);

                           LastTime = DateTime.Now;
                           RemainingTime = objrecv.Arrivetime * 1000;
                           IsRunning = true;
                       }).AsTask();
                    tmp.Wait();
                    return;
                }
                else if (string.Equals(mc.PacketName, "CTRLAccSpeed"))
                {
                    var objrecv = (NuriPosSpeedAclCtrl)mc.GetDataStruct();
                    var tmp = Dispatcher.RunAsync(CoreDispatcherPriority.High,
                       () =>
                       {
                           SetWantPOS(9999f);
                           SetWantSpeed(objrecv.Speed);
                           SetIsReverse(objrecv.Direction == Direction.CCW ? true : false);
                           SetIsWantPOS(false);

                           if (objrecv.Speed == 0)
                           {
                               if (IsRunning)
                               {
                                   LastTime = DateTime.Now;
                                   RemainingTime = objrecv.Arrivetime * 1000;
                                   IsStop = true;
                               }
                           }
                           else
                           {
                               if (!IsRunning)
                               {
                                   LastTime = DateTime.Now;
                                   RemainingTime = objrecv.Arrivetime * 1000;
                                   IsRunning = true;
                               } else
                               {
                                   LastTime = DateTime.Now;
                                   RemainingTime = objrecv.Arrivetime * 1000;
                               }
                           }
                       }).AsTask();
                    tmp.Wait();
                    return;
                }
                else if (string.Equals(mc.PacketName, "SETPosCtrl"))
                {
                    var objrecv = (NuriPosSpdCtrl)mc.GetDataStruct();

                    return;
                }
                else if (string.Equals(mc.PacketName, "SETSpeedCtrl"))
                {
                    var objrecv = (NuriPosSpdCtrl)mc.GetDataStruct();

                    return;
                }
                //else if (string.Equals(mc.PacketName, "SETID"))
                //{
                //    return;
                //}
                //else if (string.Equals(mc.PacketName, "SETBaudrate"))
                //{
                //    return;
                //}
                else if (string.Equals(mc.PacketName, "SETResptime"))
                {
                    var objrecv = (NuriResponsetime)mc.GetDataStruct();
                    var tmp = Dispatcher.RunAsync(CoreDispatcherPriority.High,
                       () =>
                       {
                           SetResponseWait(objrecv.Responsetime);
                       }).AsTask();
                    tmp.Wait();
                    return;
                }
                else if (string.Equals(mc.PacketName, "SETRatedSPD"))
                {
                    var objrecv = (NuriRatedSpeed)mc.GetDataStruct();
                    var tmp = Dispatcher.RunAsync(CoreDispatcherPriority.High,
                       () =>
                       {
                           MotorRPM = objrecv.Speed;
                       }).AsTask();
                    tmp.Wait();

                    return;
                }
                //else if (string.Equals(mc.PacketName, "SETResolution"))
                //{
                //    return;
                //}
                //else if (string.Equals(mc.PacketName, "SETRatio"))
                //{
                //    return;
                //}
                else if (string.Equals(mc.PacketName, "SETCtrlOnOff"))
                {
                    var objrecv = (NuriControlOnOff)mc.GetDataStruct();
                    var tmp = Dispatcher.RunAsync(CoreDispatcherPriority.High,
                       () =>
                       {
                           SetIsControlOnOff(objrecv.IsCtrlOn);
                       }).AsTask();
                    tmp.Wait();

                    return;
                }
                else if (string.Equals(mc.PacketName, "SETPosCtrlMode"))
                {
                    var objrecv = (NuriPositionCtrl)mc.GetDataStruct();
                    var tmp = Dispatcher.RunAsync(CoreDispatcherPriority.High,
                       () =>
                       {
                           SetIsAbsolutePotionCtrl(objrecv.IsAbsolutePotionCtrl);
                       }).AsTask();
                    tmp.Wait();

                    return;
                }
                else if (string.Equals(mc.PacketName, "SETCtrlDirt"))
                {
                    var objrecv = (NuriCtrlDirection)mc.GetDataStruct();
                    var tmp = Dispatcher.RunAsync(CoreDispatcherPriority.High,
                       () =>
                       {
                           SetIsReverse(objrecv.Direction == Direction.CCW);
                       }).AsTask();
                    tmp.Wait();

                    return;
                }
                else if (string.Equals(mc.PacketName, "RESETPos"))
                {
                   var tmp = Dispatcher.RunAsync(CoreDispatcherPriority.High,
                       () =>
                       {
                           POSValue = 0d;
                       }).AsTask();
                    tmp.Wait();

                    return;
                }
                else if (string.Equals(mc.PacketName, "RESETFactory"))
                {
                    return;
                }
                else if (string.Equals(mc.PacketName, "REQPing"))
                {
                    mc.PROT_FeedbackPing(new NuriProtocol
                    {
                        ID = id
                    });
                }
                else if (string.Equals(mc.PacketName, "REQPos"))
                {
                    mc.PROT_FeedbackPOS(new NuriPosSpeedAclCtrl
                    {
                        ID = id,
                        Direction = IsReverse ? Direction.CCW : Direction.CW,
                        Pos = NowPOS,
                        Speed = NowSpeed,
                        Current = (short)(NowCurrent * 10)
                    });
                }
                else if (string.Equals(mc.PacketName, "REQSpeed"))
                {
                    mc.PROT_FeedbackSpeed(new NuriPosSpeedAclCtrl
                    {
                        ID = id,
                        Direction = IsReverse ? Direction.CCW : Direction.CW,
                        Pos = NowPOS,
                        Speed = NowSpeed,
                        Current = (short)(NowCurrent * 10)
                    });
                }
                else if (string.Equals(mc.PacketName, "REQPosCtrl"))
                {
                    mc.PROT_FeedbackPosControl(new NuriPosSpdCtrl
                    {
                        ID = id,
                        //Protocol = (byte)ProtocolMode.FEEDPosCtrl,
                        //Direction = 
                    });
                }
                else if (string.Equals(mc.PacketName, "REQSpdCtrl"))
                {
                    mc.PROT_FeedbackSpeedControl(new NuriPosSpdCtrl
                    {
                        ID = id,
                        //Protocol = (byte)ProtocolMode.FEEDSpdCtrl,
                        //Direction = 
                    });
                }
                else if (string.Equals(mc.PacketName, "REQResptime"))
                {
                    mc.PROT_FeedbackResponsetime(new NuriResponsetime
                    {
                        ID = id,
                        Responsetime = (short)ResponseWait
                    });
                }
                else if (string.Equals(mc.PacketName, "REQRatedSPD"))
                {
                    int speed = 0;
                    var tmp = Dispatcher.RunAsync(CoreDispatcherPriority.High,
                       () =>
                       {
                           speed = MotorRPM;
                       }).AsTask();
                    tmp.Wait();

                    mc.PROT_FeedbackRatedSpeed(new NuriRatedSpeed
                    {
                        ID = id,
                        Speed = (ushort)speed
                    });
                }
                else if (string.Equals(mc.PacketName, "REQResolution"))
                {
                    mc.PROT_FeedbackResolution(new NuriResolution
                    {
                        ID = id
                    });
                }
                else if (string.Equals(mc.PacketName, "REQRatio"))
                {
                    mc.PROT_FeedbackRatio(new NuriRatio
                    {
                        ID = id
                    });
                }
                else if (string.Equals(mc.PacketName, "REQCtrlOnOff"))
                {
                    mc.PROT_FeedbackControlOnOff(new NuriControlOnOff
                    {
                        ID = id,
                        IsCtrlOn = IsControlOnOff
                    });
                }
                else if (string.Equals(mc.PacketName, "REQPosCtrlMode"))
                {
                    mc.PROT_FeedbackPositionControl(new NuriPositionCtrl
                    {
                        ID = id,
                        IsAbsolutePotionCtrl = IsAbsolutePotionCtrl
                    });
                }
                else if (string.Equals(mc.PacketName, "REQCtrlDirt"))
                {
                    mc.PROT_FeedbackControlDirection(new NuriCtrlDirection
                    {
                        ID = id,
                        Direction = IsReverse ? Direction.CCW : Direction.CW
                    });
                }
                else if (string.Equals(mc.PacketName, "REQFirmware"))
                {
                    mc.PROT_FeedbackFirmware(new NuriVersion
                    {
                        ID = id
                    });
                }
                else
                {
                    return;
                }

                //Thread.Sleep(ResponseWait);
                var wait = Wait.Start(ResponseWait);
                wait.Sleep();
                publisherSocket.SendMoreFrame("SEND").SendFrame(mc.Data);
            } catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        private void ReciveRSA(byte[] arg, MotorGauge obj)
        {
            try
            {
                Debug.WriteLine(string.Format("RSA ====== recv : {0}", BitConverter.ToString(arg).Replace("-", "")));
                NurirobotRSA rsa = new NurirobotRSA();
                rsa.Parse(arg);
                byte id = GetID();

                if (string.Equals(rsa.PacketName, "CTRLPosSpeed"))
                {
                    var objrecv = (NuriPosSpeedAclCtrl)rsa.GetDataStruct();
                    var tmp = Dispatcher.RunAsync(CoreDispatcherPriority.High,
                       () =>
                       {
                           SetWantPOS(objrecv.Pos);
                           SetWantSpeed(objrecv.Speed);
                           //SetIsReverse(objrecv.Direction == Direction.CCW ? true : false);
                           SetIsReverse(POSValue >= objrecv.Pos ? true : false);
                           SetIsWantPOS(true);
                           //SetIsWantSpeedPOS(true);

                           LastTime = DateTime.Now;
                           RemainingTime = 0;
                           IsRunning = true;
                       }).AsTask();
                    tmp.Wait();

                    return;
                }
                else if (string.Equals(rsa.PacketName, "CTRLAccPos"))
                {
                    var objrecv = (NuriPosSpeedAclCtrl)rsa.GetDataStruct();
                    var tmp = Dispatcher.RunAsync(CoreDispatcherPriority.High,
                       () =>
                       {
                           SetWantPOS(objrecv.Pos);

                           //SetWantSpeed(0);
                           float spd = 0f;
                           var high = Math.Max(POSValue, objrecv.Pos);
                           var low = Math.Min(POSValue, objrecv.Pos);
                           var termpos = high - low;
                           spd = (float)(termpos / (objrecv.Arrivetime * 1000f / 100f) * 7f);
                           SetWantSpeed(spd);
                           //SetIsReverse(objrecv.Direction == Direction.CCW ? true : false);
                           SetIsReverse(POSValue >= objrecv.Pos ? true : false);
                           SetIsWantPOS(true);
                           //SetIsWantSpeedPOS(false);

                           LastTime = DateTime.Now;
                           RemainingTime = objrecv.Arrivetime * 1000;
                           IsRunning = true;
                       }).AsTask();
                    tmp.Wait();
                    return;
                }
                else if (string.Equals(rsa.PacketName, "CTRLAccSpeed"))
                {
                    var objrecv = (NuriPosSpeedAclCtrl)rsa.GetDataStruct();
                    var tmp = Dispatcher.RunAsync(CoreDispatcherPriority.High,
                       () =>
                       {
                           SetWantPOS(9999f);
                           SetWantSpeed(objrecv.Speed);
                           SetIsReverse(objrecv.Direction == Direction.CCW ? true : false);
                           SetIsWantPOS(false);

                           if (objrecv.Speed == 0)
                           {
                               if (IsRunning)
                               {
                                   LastTime = DateTime.Now;
                                   RemainingTime = objrecv.Arrivetime * 1000;
                                   IsStop = true;
                               }
                           }
                           else
                           {
                               if (!IsRunning)
                               {
                                   LastTime = DateTime.Now;
                                   RemainingTime = objrecv.Arrivetime * 1000;
                                   IsRunning = true;
                               }
                               else
                               {
                                   LastTime = DateTime.Now;
                                   RemainingTime = objrecv.Arrivetime * 1000;
                               }
                           }
                       }).AsTask();
                    tmp.Wait();
                    return;
                }
                else if (string.Equals(rsa.PacketName, "SETPosCtrl"))
                {
                    var objrecv = (NuriPosSpdCtrl)rsa.GetDataStruct();

                    return;
                }
                else if (string.Equals(rsa.PacketName, "SETSpeedCtrl"))
                {
                    var objrecv = (NuriPosSpdCtrl)rsa.GetDataStruct();

                    return;
                }
                //else if (string.Equals(mc.PacketName, "SETID"))
                //{
                //    return;
                //}
                //else if (string.Equals(mc.PacketName, "SETBaudrate"))
                //{
                //    return;
                //}
                else if (string.Equals(rsa.PacketName, "SETResptime"))
                {
                    var objrecv = (NuriResponsetime)rsa.GetDataStruct();
                    var tmp = Dispatcher.RunAsync(CoreDispatcherPriority.High,
                       () =>
                       {
                           SetResponseWait(objrecv.Responsetime);
                       }).AsTask();
                    tmp.Wait();
                    return;
                }
                else if (string.Equals(rsa.PacketName, "SETRatedSPD"))
                {
                    var objrecv = (NuriRatedSpeed)rsa.GetDataStruct();
                    var tmp = Dispatcher.RunAsync(CoreDispatcherPriority.High,
                       () =>
                       {
                           MotorRPM = objrecv.Speed;
                       }).AsTask();
                    tmp.Wait();

                    return;
                }
                //else if (string.Equals(mc.PacketName, "SETResolution"))
                //{
                //    return;
                //}
                //else if (string.Equals(mc.PacketName, "SETRatio"))
                //{
                //    return;
                //}
                else if (string.Equals(rsa.PacketName, "SETCtrlOnOff"))
                {
                    var objrecv = (NuriControlOnOff)rsa.GetDataStruct();
                    var tmp = Dispatcher.RunAsync(CoreDispatcherPriority.High,
                       () =>
                       {
                           SetIsControlOnOff(objrecv.IsCtrlOn);
                       }).AsTask();
                    tmp.Wait();

                    return;
                }
                else if (string.Equals(rsa.PacketName, "SETPosCtrlMode"))
                {
                    var objrecv = (NuriPositionCtrl)rsa.GetDataStruct();
                    var tmp = Dispatcher.RunAsync(CoreDispatcherPriority.High,
                       () =>
                       {
                           SetIsAbsolutePotionCtrl(objrecv.IsAbsolutePotionCtrl);
                       }).AsTask();
                    tmp.Wait();

                    return;
                }
                else if (string.Equals(rsa.PacketName, "SETCtrlDirt"))
                {
                    var objrecv = (NuriCtrlDirection)rsa.GetDataStruct();
                    var tmp = Dispatcher.RunAsync(CoreDispatcherPriority.High,
                       () =>
                       {
                           SetIsReverse(objrecv.Direction == Direction.CCW);
                       }).AsTask();
                    tmp.Wait();

                    return;
                }
                else if (string.Equals(rsa.PacketName, "RESETPos"))
                {
                    var tmp = Dispatcher.RunAsync(CoreDispatcherPriority.High,
                        () =>
                        {
                            POSValue = 0d;
                        }).AsTask();
                    tmp.Wait();

                    return;
                }
                else if (string.Equals(rsa.PacketName, "RESETFactory"))
                {
                    return;
                }
                else if (string.Equals(rsa.PacketName, "REQPing"))
                {
                    rsa.PROT_FeedbackPing(new NuriProtocol
                    {
                        ID = id
                    });
                }
                else if (string.Equals(rsa.PacketName, "REQPos"))
                {
                    rsa.PROT_FeedbackPOS(new NuriPosSpeedAclCtrl
                    {
                        ID = id,
                        Direction = IsReverse ? Direction.CCW : Direction.CW,
                        Pos = NowPOS,
                        Speed = NowSpeed,
                        Current = (short)(NowCurrent * 10)
                    });
                }
                else if (string.Equals(rsa.PacketName, "REQSpeed"))
                {
                    rsa.PROT_FeedbackSpeed(new NuriPosSpeedAclCtrl
                    {
                        ID = id,
                        Direction = IsReverse ? Direction.CCW : Direction.CW,
                        Pos = NowPOS,
                        Speed = NowSpeed,
                        Current = (short)(NowCurrent * 10)
                    });
                }
                else if (string.Equals(rsa.PacketName, "REQPosCtrl"))
                {
                    rsa.PROT_FeedbackPosControl(new NuriPosSpdCtrl
                    {
                        ID = id,
                        //Protocol = (byte)ProtocolMode.FEEDPosCtrl,
                        //Direction = 
                    });
                }
                else if (string.Equals(rsa.PacketName, "REQSpdCtrl"))
                {
                    rsa.PROT_FeedbackSpeedControl(new NuriPosSpdCtrl
                    {
                        ID = id,
                        //Protocol = (byte)ProtocolMode.FEEDSpdCtrl,
                        //Direction = 
                    });
                }
                else if (string.Equals(rsa.PacketName, "REQResptime"))
                {
                    rsa.PROT_FeedbackResponsetime(new NuriResponsetime
                    {
                        ID = id,
                        Responsetime = (short)ResponseWait
                    });
                }
                //else if (string.Equals(rsa.PacketName, "REQRatedSPD"))
                //{
                //    int speed = 0;
                //    var tmp = Dispatcher.RunAsync(CoreDispatcherPriority.High,
                //       () =>
                //       {
                //           speed = MotorRPM;
                //       }).AsTask();
                //    tmp.Wait();

                //    rsa.PROT_FeedbackRatedSpeed(new NuriRatedSpeed
                //    {
                //        ID = id,
                //        Speed = (ushort)speed
                //    });
                //}
                //else if (string.Equals(rsa.PacketName, "REQResolution"))
                //{
                //    rsa.PROT_FeedbackResolution(new NuriResolution
                //    {
                //        ID = id
                //    });
                //}
                else if (string.Equals(rsa.PacketName, "REQRatio"))
                {
                    rsa.PROT_FeedbackRatio(new NuriRatio
                    {
                        ID = id
                    });
                }
                else if (string.Equals(rsa.PacketName, "REQCtrlOnOff"))
                {
                    rsa.PROT_FeedbackControlOnOff(new NuriControlOnOff
                    {
                        ID = id,
                        IsCtrlOn = IsControlOnOff
                    });
                }
                else if (string.Equals(rsa.PacketName, "REQPosCtrlMode"))
                {
                    rsa.PROT_FeedbackPositionControl(new NuriPositionCtrl
                    {
                        ID = id,
                        IsAbsolutePotionCtrl = IsAbsolutePotionCtrl
                    });
                }
                //else if (string.Equals(rsa.PacketName, "REQCtrlDirt"))
                //{
                //    rsa.PROT_FeedbackControlDirection(new NuriCtrlDirection
                //    {
                //        ID = id,
                //        Direction = IsReverse ? Direction.CCW : Direction.CW
                //    });
                //}
                else if (string.Equals(rsa.PacketName, "REQFirmware"))
                {
                    rsa.PROT_FeedbackFirmware(new NuriVersion
                    {
                        ID = id
                    });
                }
                else
                {
                    return;
                }


            }
            catch
            {

            }
        }

        private double easeInSine(double x)
        {
            return 1d - Math.Cos((x * Math.PI) / 2);
        }

        private void _Timer_Tick(object sender, object e)
        {
            //throw new NotImplementedException();
            if (!IsRunning)
                return;
            
            var tmp = POSValue;
            var term = Math.Min((DateTime.Now - LastTime).TotalMilliseconds, RemainingTime);
            bool isminus = false;

            if (!IsStop)
            {
                // 정상
                if (RemainingTime == 0)
                    POSStep = WantSpeed / 60 * 360 * (25f / 1000);
                else
                    POSStep = WantSpeed / 60 * 360 * (25f / 1000) * easeInSine(term / RemainingTime);
            }
            else
            {
                if (RemainingTime == 0)
                    POSStep = 0;
                else
                {
                    POSStep = WantSpeed / 60 * 360 * (25f / 1000) - (WantSpeed / 60 * 360 * (25f / 1000) * easeInSine(term / RemainingTime));
                }

                // 중지
                if (POSStep < 1d)
                {
                    IsRunning = false;
                    IsStop = false;
                    POSStep = 0;
                }
            }

            if (IsReverse)
            {
                if (WantSpeed >= SpeedValue)
                    tmp -= POSStep;
                else
                    tmp += POSStep;

                if (tmp < 0)
                {
                    tmp = 655 + (tmp % 655);
                    isminus = true;
                }
            }
            else
            {
                if (WantSpeed >= SpeedValue)
                    tmp += POSStep;
                else
                    tmp -= POSStep;

                if (tmp < 0)
                {
                    tmp = 655 + (tmp % 655);
                    isminus = true;
                }
            }

            //POSValue = ((int)(tmp * 100) % 65535) / 100f;
            POSValue = tmp;
            if (tmp > 655.35)
                POSValue = tmp - 655.35d;

            SpeedValue = ((long)((POSStep / (25f / 1000) / 360 * 60) * 10000) /  10000f);
            CurrentValue = SpeedValue / 2;
            if (IsWantPOS)
            {
                if (IsReverse)
                {
                    if (WantPOS >= POSValue)
                    {
                        POSValue = WantPOS;
                        SpeedValue = 0;
                        CurrentValue = 0;
                        IsRunning = false;
                        IsStop = false;
                    }
                }
                else
                {
                    if (WantPOS <= POSValue)
                    {
                        POSValue = WantPOS;
                        SpeedValue = 0;
                        CurrentValue = 0;
                        IsRunning = false;
                        IsStop = false;
                    }
                }
                
                if (isminus || tmp > 655.35f)
                {
                    POSValue = WantPOS;
                    SpeedValue = 0;
                    CurrentValue = 0;
                    IsRunning = false;
                    IsStop = false;
                }
            }
            NowPOS = (float)POSValue;
            NowSpeed = (float)SpeedValue;
            NowCurrent = (float)CurrentValue;
            Debug.WriteLine("{3} {0} {1} {2}", term, POSValue, POSStep, DateTime.Now);
        }

        private void MotorGauge_Unloaded(object sender, RoutedEventArgs e)
        {
            Unloaded -= MotorGauge_Unloaded;
            _MotorID.SelectionChanged -= _MotorID_SelectionChanged;
            ThemeListener.ThemeChanged -= ThemeListener_ThemeChanged;
            //KeyDown -= MotorGauge_KeyDown;

        }

        protected override void OnApplyTemplate()
        {
            _foreground = ReadLocalValue(ForegroundProperty) as SolidColorBrush;
            _POSneedleBrush = ReadLocalValue(POSneedleBrushProperty) as SolidColorBrush;
            _POSTickBrush = ReadLocalValue(POSTickBrushProperty) as SolidColorBrush;

            _SpeedTickBrush = ReadLocalValue(SpeedTickBrushProperty) as SolidColorBrush;
            _SpeedneedleBrush = ReadLocalValue(SpeedneedleBrushProperty) as SolidColorBrush;

            _CurrentneedleBrush = ReadLocalValue(CurrentneedleBrushProperty) as SolidColorBrush;

            _MotorID = this.GetTemplateChild(MoterIDName) as ComboBox;
            _MotorID.SelectionChanged += _MotorID_SelectionChanged;
            MotorID = MotorID;

            _MotorType = this.GetTemplateChild(MoterTypeName) as ComboBox;
            _MotorType.SelectionChanged += _MotorType_SelectionChanged;
            MotorType = MotorType;

            Subscriber();

            ThemeListener.ThemeChanged += ThemeListener_ThemeChanged;
            KeyDown += MotorGauge_KeyDown;
            OnColorsChanged();

            base.OnApplyTemplate();
        }

        private void _MotorType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine(_MotorType.SelectedValue);
            string tmp = (string)_MotorType.SelectedValue;
            if (MotorType == tmp)
                return;

            MotorType = tmp;
            Subscriber();
        }

        private void _MotorID_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine(((ComboBoxItem)_MotorID.SelectedItem).Content);
            int tmp = int.Parse((string)((ComboBoxItem)_MotorID.SelectedItem).Content);
            if (MotorID == tmp)
                return;

            MotorID = tmp;
            Subscriber();
        }

        private void MotorGauge_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            double step = 1;
            var ctrl = Window.Current.CoreWindow.GetKeyState(VirtualKey.Control);
            if (ctrl.HasFlag(CoreVirtualKeyStates.Down))
            {
                step = 10;
            }

            if ((e.Key == VirtualKey.Left) || (e.Key == VirtualKey.Down))
            {
                var tmppos = Math.Min(360, POSValue - step);
                POSValue = tmppos >= 0?  tmppos: 360;
                var tmpspeed = Math.Min(5000, SpeedValue - step);
                SpeedValue = tmpspeed >= 0 ? tmpspeed: 5000;
                var tmpcurrent = Math.Min(5000, CurrentValue - step);
                CurrentValue = tmpcurrent >= 0 ? tmpcurrent : 5000;
                e.Handled = true;
                return;
            }

            if ((e.Key == VirtualKey.Right) || (e.Key == VirtualKey.Up))
            {
                POSValue = (POSValue + step) % 360;
                SpeedValue = (SpeedValue + step) % 5000 ;
                CurrentValue = (CurrentValue + step) % 5000;
                e.Handled = true;
            }
        }

        private void ThemeListener_ThemeChanged(ThemeListener sender)
        {
            OnColorsChanged();
        }

        

        private static void OnPOSPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            OnPOSValueChanged(sender);
        }

        private static void OnSpeedPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            OnSpeedValueChanged(sender);
        }

        private static void OnCurrentPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            OnCurrentValueChanged(sender);
        }

        private void OnColorsChanged()
        {
            if (ThemeListener.IsHighContrast)
            {
                ClearBrush(_POSneedleBrush, POSneedleBrushProperty);
                ClearBrush(_SpeedneedleBrush, SpeedneedleBrushProperty);
                ClearBrush(_POSTickBrush, POSTickBrushProperty);
                ClearBrush(_SpeedneedleBrush, SpeedneedleBrushProperty);
                ClearBrush(_SpeedTickBrush, SpeedTickBrushProperty);
                ClearBrush(_CurrentneedleBrush, CurrentneedleBrushProperty);
                ClearBrush(_foreground, ForegroundProperty);
            }
            else
            {
                RestoreBrush(_POSneedleBrush, POSneedleBrushProperty);
                RestoreBrush(_SpeedneedleBrush, SpeedneedleBrushProperty);
                RestoreBrush(_POSTickBrush, POSTickBrushProperty);
                RestoreBrush(_SpeedneedleBrush, SpeedneedleBrushProperty);
                RestoreBrush(_SpeedTickBrush, SpeedTickBrushProperty);
                RestoreBrush(_CurrentneedleBrush, CurrentneedleBrushProperty);
                RestoreBrush(_foreground, ForegroundProperty);
            }

            OnScaleChanged(this);
        }

        private void ClearBrush(Brush brush, DependencyProperty prop)
        {
            if (brush != null)
            {
                ClearValue(prop);
            }
        }

        private void RestoreBrush(Brush source, DependencyProperty prop)
        {
            if (source != null)
            {
                SetValue(prop, source);
            }
        }

        private static void OnScaleChanged(DependencyObject d)
        {
            MotorGauge gauge = (MotorGauge)d;

            OnFaceChanged(gauge);
        }

        private static void OnFaceChanged(DependencyObject d)
        {
            MotorGauge gauge = (MotorGauge)d;

            var container = gauge.GetTemplateChild(ContainerPartName) as Grid;
            if (container == null || DesignTimeHelpers.IsRunningInLegacyDesignerMode)
            {
                return;
            }

            gauge._root = container.GetVisual();
            gauge._root.Children.RemoveAll();
            gauge._compositor = gauge._root.Compositor;

            SpriteVisual tick;
            for (double i = 0; i <= 359; i += 30)
            {
                tick = gauge._compositor.CreateSpriteVisual();
                tick.Size = new Vector2(3f, 6f);
                tick.Brush = gauge._compositor.CreateColorBrush(gauge.POSTickBrush.Color);
                tick.Offset = new Vector3(100 - (3f / 2), 0.0f, 0);
                tick.CenterPoint = new Vector3(3f / 2, 100.0f, 0);
                tick.RotationAngleInDegrees = (float)gauge.ValueToAngle(i);
                gauge._root.Children.InsertAtTop(tick);
            }

            // 배경
            var speedpart = gauge.GetTemplateChild(SpeedBasePartName) as Path;
            if (speedpart != null)
            {
                var pg = new PathGeometry();
                var pf = new PathFigure();
                pf.IsClosed = false;
                var middleOfScale = 100 - 35d - (26d / 2);
                pf.StartPoint = gauge.ScalePoint(270d, middleOfScale);
                var seg = new ArcSegment();
                seg.SweepDirection = SweepDirection.Clockwise;
                seg.IsLargeArc = 45d > (270d + 180);
                seg.Size = new Size(middleOfScale, middleOfScale);
                seg.Point = gauge.ScalePoint(45d, middleOfScale);
                pf.Segments.Add(seg);
                pg.Figures.Add(pf);
                speedpart.Data = pg;
            }

            var currentpart = gauge.GetTemplateChild(CurrentBasePartName) as Path;
            if (currentpart != null)
            {
                var pg = new PathGeometry();
                var pf = new PathFigure();
                pf.IsClosed = false;
                var middleOfScale = 100 - 50d - (26d / 2);
                pf.StartPoint = gauge.ScalePoint(135d, middleOfScale);
                var seg = new ArcSegment();
                seg.SweepDirection = SweepDirection.Clockwise;
                seg.IsLargeArc = 225d > (135d + 180);
                seg.Size = new Size(middleOfScale, middleOfScale);
                seg.Point = gauge.ScalePoint(225d, middleOfScale);
                pf.Segments.Add(seg);
                pg.Figures.Add(pf);
                currentpart.Data = pg;
            }

            gauge._POSneedle = gauge._compositor.CreateSpriteVisual();
            gauge._POSneedle.Size = new Vector2(5f, 25f);
            gauge._POSneedle.Brush = gauge._compositor.CreateColorBrush(gauge.POSneedleBrush.Color);
            gauge._POSneedle.CenterPoint = new Vector3(5f / 2, 100f, 0);
            gauge._POSneedle.Offset = new Vector3(100 - (5f / 2), 100 - 100f, 0);
            gauge._root.Children.InsertAtTop(gauge._POSneedle);

            gauge._Speedneedle = gauge._compositor.CreateSpriteVisual();
            gauge._Speedneedle.Size = new Vector2(5f, 70f);
            gauge._Speedneedle.Brush = gauge._compositor.CreateColorBrush(gauge.SpeedneedleBrush.Color);
            gauge._Speedneedle.CenterPoint = new Vector3(5f / 2, 100f - 30f, 0);
            gauge._Speedneedle.Offset = new Vector3(100 - (5f / 2), 100 - 100f + 30f, 0);
            gauge._root.Children.InsertAtTop(gauge._Speedneedle);

            gauge._Currentneedle = gauge._compositor.CreateSpriteVisual();
            gauge._Currentneedle.Size = new Vector2(3f, 55f);
            gauge._Currentneedle.Brush = gauge._compositor.CreateColorBrush(gauge.CurrentneedleBrush.Color);
            gauge._Currentneedle.CenterPoint = new Vector3(3f / 2, 100f - 45f, 0);
            gauge._Currentneedle.Offset = new Vector3(100 - (3f / 2), 100 - 100f + 45f, 0);
            gauge._root.Children.InsertAtTop(gauge._Currentneedle);

            OnPOSValueChanged(gauge);
            OnSpeedValueChanged(gauge);
            OnCurrentValueChanged(gauge);

        }

        private static void OnPOSValueChanged(DependencyObject d)
        {
            MotorGauge gauge = (MotorGauge)d;

            if (!double.IsNaN(gauge.POSValue))
            {
                if (gauge._POSneedle != null)
                {
                    //gauge._POSneedle.StartAnimation
                    gauge._POSneedle.RotationAngleInDegrees = (float)(gauge.POSValue / 655.35d * 720f);
                }
            }
        }

        private static void OnCurrentValueChanged(DependencyObject d)
        {
            MotorGauge gauge = (MotorGauge)d;

            if (!double.IsNaN(gauge.CurrentValue))
            {
                if (gauge._Currentneedle != null)
                {
                    gauge._Currentneedle.RotationAngleInDegrees = (float)ValueToCurrentAngle(gauge.CurrentValue);
                }
            }
        }

        private static void OnSpeedValueChanged(DependencyObject d)
        {
            MotorGauge gauge = (MotorGauge)d;

            if (!double.IsNaN(gauge.SpeedValue))
            {
                float angle = (float)ValueToSpeedAngle(gauge.SpeedValue, gauge.MotorRPM);
                if (gauge._Speedneedle != null)
                {
                    gauge._Speedneedle.RotationAngleInDegrees = angle;
                }

                var speedpart = gauge.GetTemplateChild(SpeedPartName) as Path;
                if (speedpart != null)
                {
                    var pg = new PathGeometry();
                    var pf = new PathFigure();
                    pf.IsClosed = false;
                    var middleOfScale = 100 - 35d - (26d / 2);
                    pf.StartPoint = gauge.ScalePoint(270d, middleOfScale);
                    var seg = new ArcSegment();
                    seg.SweepDirection = SweepDirection.Clockwise;
                    seg.IsLargeArc = 45d > (270d + 180);
                    seg.Size = new Size(middleOfScale, middleOfScale);
                    seg.Point = gauge.ScalePoint(angle, middleOfScale);
                    pf.Segments.Add(seg);
                    pg.Figures.Add(pf);
                    speedpart.Data = pg;
                }
            }
        }

        private static double ValueToSpeedAngle(double value, int motorRPM)
        {
            if (value < 0)
                return 270;

            if (value > motorRPM)
                return 405;

            return ((value - 0) / (motorRPM - 0) * (405 - 270)) + 270;
        }

        private static double ValueToCurrentAngle(double value)
        {
            if (value < 0)
                return 135;

            if (value > 5000)
                return 225;

            return ((value - 0) / (5000 - 0) * (225 - 135)) + 135;
        }

        private Point ScalePoint(double angle, double middleOfScale)
        {
            return new Point(100 + (Math.Sin(Degrees2Radians * angle) * middleOfScale), 100 - (Math.Cos(Degrees2Radians * angle) * middleOfScale));
        }

        private double ValueToAngle(double value)
        {
            // Off-scale on the left.
            if (value < 0)
            {
                return 0;
            }

            // Off-scale on the right.
            if (value > 360)
            {
                return 360;
            }

            return ((value - 0) / (360 - 0) * (360 - 0)) + 0;
        }

        private Storyboard _POSStoryboard;
        public void StartPOS(double time)
        {
            LastTime = DateTime.Now;
            RemainingTime = time;
            IsRunning = true;
        }

        public void Stop(double time)
        {
            if (IsRunning)
            {
                LastTime = DateTime.Now;
                RemainingTime = time;
                IsStop = true;
            }
        }

    }
}
