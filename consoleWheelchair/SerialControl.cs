namespace consoleWheelchair
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Reactive;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.IO.Ports;
    using System.Diagnostics;
    using System.Threading;
    using System.Collections.Concurrent;

    public class SerialControl
    {
        /// <summary>
        /// 주제 : 시리얼 포트 열림
        /// </summary>
        internal readonly ISubject<bool> isOpen = new ReplaySubject<bool>(1);


        readonly ISubject<byte[]> rawRecviced = new Subject<byte[]>();
        IObservable<byte[]> ObsrawReceived => rawRecviced.Retry().Publish().RefCount();

        /// <summary>
        /// 주제 : 데이트 수신
        /// </summary>
        //private readonly ISubject<byte> dataReceived = new Subject<byte>();

        /// <summary>
        /// 주제 : 발생 에러
        /// </summary>
        private readonly ISubject<Exception> errors = new Subject<Exception>();

        /// <summary>
        /// 주제 : 전달 바이트
        /// </summary>
        private readonly ISubject<Tuple<byte[], int, int>> writeByte = new Subject<Tuple<byte[], int, int>>();

        /// <summary>
        /// 시리얼 포트 설정
        /// </summary>
        private SerialPortSetting _SerialPortSetting = null;

        /// <summary>
        /// 포트 해제 그룹
        /// </summary>
        private CompositeDisposable disposablePort;// = new CompositeDisposable();

        /// <summary>
        /// 데이트 수신 연동
        /// </summary>
        //public IObservable<byte> ObsDataReceived => dataReceived.Retry().Publish().RefCount();

        /// <summary>
        /// 에러 발생 연동
        /// </summary>
        public IObservable<Exception> ObsErrorReceived => errors.Distinct(ex => ex.Message).Retry().Publish().RefCount();

        /// <summary>
        /// 포트 열림 변경 연결
        /// </summary>
        public IObservable<bool> ObsIsOpenObservable => isOpen.DistinctUntilChanged();

        public bool IsOpen { get; private set; }
        public bool IsDisposed { get; private set; } = false;
        //SerialPort serialPort;

        public double MaxProcessTime = 0.0;

        /// <summary>
        /// 포트 연결
        /// </summary>
        private IObservable<Unit> ObsConnect => Observable.Create<Unit>(obs => {
            var dis = new CompositeDisposable();

            // 포트 존재 확인
            // 포트 설정
            var port = new SerialPort(
                _SerialPortSetting.PortName,
                (int)_SerialPortSetting.Baudrate,
                (System.IO.Ports.Parity)(int)_SerialPortSetting.Parity,
                _SerialPortSetting.DataBits,
                (System.IO.Ports.StopBits)(int)_SerialPortSetting.StopBits);
            dis.Add(port);
            port.Close();
            port.Handshake = (System.IO.Ports.Handshake)(int)_SerialPortSetting.Handshake;

            //port.ReadTimeout = _SerialPortSetting.ReadTimeout;
            //port.WriteTimeout = _SerialPortSetting.WriteTimeout;

            Debug.WriteLine(string.Format("Connect Baud : {0}", _SerialPortSetting.Baudrate));
            Debug.WriteLine(string.Format("port.WriteBufferSize : {0}", port.WriteBufferSize));
            Debug.WriteLine(string.Format("port.ReadBufferSize : {0}", port.ReadBufferSize));

            try
            {
                port.Open();
            }
            catch (Exception ex)
            {
                errors.OnNext(ex);
                obs.OnCompleted();
            }

            isOpen.OnNext(port.IsOpen);
            IsOpen = port.IsOpen;

            if (IsOpen)
            {
                port.DiscardInBuffer();
                port.DiscardOutBuffer();
            }
            Thread.Sleep(100);

            dis.Add(port
                .ErrorReceivedObserver()
                .Where(x => x.EventArgs.EventType != SerialError.Frame)
                .Subscribe(e => obs.OnError(new Exception(e.EventArgs.EventType.ToString()))));
            DateTime stDate = DateTime.Now;
            dis.Add(writeByte.Subscribe(x => {
                try
                {
                    if ((bool)(port?.IsOpen))
                    {
                        port?.Write(x.Item1, x.Item2, x.Item3);
                        stDate = DateTime.Now;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    obs.OnError(ex);
                }
            }, obs.OnError));

            Stopwatch sw = new Stopwatch();
            var buf = new byte[4096];
            ConcurrentQueue<byte[]> recvBuffs = new ConcurrentQueue<byte[]>();
            var received = port.DataReceivedObserver()
                .Subscribe(e => {
                    try
                    {
                        if (e.EventArgs.EventType == SerialData.Eof)
                            return;

                        TimeSpan diff = DateTime.Now - stDate;
                        if (MaxProcessTime < diff.TotalMilliseconds)
                        {
                            MaxProcessTime = diff.TotalMilliseconds;
                            Debug.WriteLine(string.Format("Max Term : {0}", MaxProcessTime));
                            if (MaxProcessTime > 100.0)
                                MaxProcessTime = 0.0;
                        }
                        var len = port.Read(buf, 0, buf.Length);
                        var newbuff = new byte[len];
                        Buffer.BlockCopy(buf, 0, newbuff, 0, newbuff.Length);
                        recvBuffs.Enqueue(newbuff);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                        obs.OnError(ex);
                    }
                });
            dis.Add(received);

            byte[] baSTX = new byte[] { 0xFF, 0xFE };
            CancellationTokenSource source = new CancellationTokenSource();
            source.AddTo(dis);
            Task.Run(() => {
                int idx = 0;
                byte[] buffPattern = new byte[4096];
                Stopwatch stopwatch = new Stopwatch();
                try
                {
                    while (!source.Token.IsCancellationRequested)
                    {
                        try
                        {
                            if (recvBuffs.TryDequeue(out byte[] result))
                            {
                                Buffer.BlockCopy(result, 0, buffPattern, idx, result.Length);
                                idx += result.Length;

                                var pos = SerialportReactiveExt.PatternAt(buffPattern, baSTX, 0, idx);
                                if (pos == 0)
                                {
                                    var possec = SerialportReactiveExt.PatternAt(buffPattern, baSTX, 2, idx);
                                    if (possec == -1)
                                    {
                                        if (buffPattern[3] + 4 == idx)
                                        {
                                            byte[] segment = new byte[idx];
                                            Buffer.BlockCopy(buffPattern, 0, segment, 0, segment.Length);
                                            ProtocolReceived.OnNext(segment);
                                            idx = 0;
                                            stopwatch.Stop();
                                            stopwatch.Reset();
                                            continue;
                                        }
                                    }
                                }
                                stopwatch.Reset();
                                stopwatch.Restart();
                            }

                            if (idx > 3)
                            {
                                if (buffPattern[0] == baSTX[0] && buffPattern[1] == baSTX[1])
                                {
                                    var pos = SerialportReactiveExt.PatternAt(buffPattern, baSTX, 2, idx);
                                    if (pos >= 0)
                                    {
                                        byte[] segment = new byte[pos];
                                        Buffer.BlockCopy(buffPattern, 0, segment, 0, segment.Length);

                                        if (pos > 3
                                        && segment[3] + 4 == pos)
                                            ProtocolReceived.OnNext(segment);
#if DEBUG
                                        else
                                        {
                                            Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.FFF") + "\t" + BitConverter.ToString(segment).Replace("-", ""));
                                        }
#endif

                                        Buffer.BlockCopy(buffPattern, pos, buffPattern, 0, idx - pos);
                                        idx -= pos;
                                        stopwatch.Reset();
                                        stopwatch.Restart();
                                    }
                                    else
                                    {
                                        var lenprotocol = buffPattern[3] + 4;
                                        if (lenprotocol < idx)
                                        {
                                            byte[] segment = new byte[lenprotocol];
                                            Buffer.BlockCopy(buffPattern, 0, segment, 0, segment.Length);
                                            ProtocolReceived.OnNext(segment);

                                            Buffer.BlockCopy(buffPattern, lenprotocol, buffPattern, 0, idx - lenprotocol);
                                            idx -= lenprotocol;
                                            stopwatch.Reset();
                                            stopwatch.Restart();
                                        }
                                    }
                                }
                                else
                                {
                                    var pos = SerialportReactiveExt.PatternAt(buffPattern, baSTX, 1, idx);
                                    if (pos > 0)
                                    {
#if DEBUG
                                        byte[] segment = new byte[pos];
                                        Buffer.BlockCopy(buffPattern, 0, segment, 0, segment.Length);
                                        Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.FFF") + "\t" + BitConverter.ToString(segment).Replace("-", ""));
#endif

                                        Buffer.BlockCopy(buffPattern, pos, buffPattern, 0, idx - pos);
                                        idx -= pos;
                                    }
                                }
                            }

                            if (idx > 0
                            && stopwatch.ElapsedMilliseconds > 1000)
                            {
                                if (idx > 3
                                && buffPattern[0] == baSTX[0] && buffPattern[1] == baSTX[1])
                                {
                                    byte[] segment = new byte[idx];
                                    Buffer.BlockCopy(buffPattern, 0, segment, 0, segment.Length);
                                    if (idx > 3
                                    && segment[3] + 4 == idx)
                                        ProtocolReceived.OnNext(segment);
                                    else
                                    {
                                        byte[] tmpError = new byte[8] { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 };
                                        ProtocolReceived.OnNext(tmpError);
                                        Debug.WriteLine("Timeout : " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.FFF") + "\t 데이터 수신 미완");
                                    }
                                }
                                else
                                {
                                    byte[] tmpError = new byte[8] { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 };
                                    ProtocolReceived.OnNext(tmpError);
#if DEBUG
                                    Debug.WriteLine("Timeout : " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.FFF") + "\t STX 이상");
#endif
                                }

                                idx = 0;
                                stopwatch.Stop();
                                stopwatch.Reset();
                            }
                            Thread.Sleep(0);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }, source.Token);

            return Disposable.Create(() => {
                IsOpen = false;
                isOpen.OnNext(false);
                source.Cancel();
                dis.Dispose();
            });
        }).OnErrorRetry((Exception ex) => errors.OnNext(ex)).Publish().RefCount();


        readonly ISubject<byte[]> ProtocolReceived = new Subject<byte[]>();
        public IObservable<byte[]> ObsProtocolReceived => ProtocolReceived.Retry().Publish().RefCount();

        public Task Connect()
        {
            if (_SerialPortSetting == null)
            {
                errors.OnNext(new Exception("시리얼 포트 설정이 없습니다."));
                return Task.CompletedTask;
            }

            if (!SerialPort.GetPortNames().Any(name => name.Equals(_SerialPortSetting.PortName)))
            {
                errors.OnNext(new Exception($"{_SerialPortSetting.PortName}이(가) 없습니다."));
                return Task.CompletedTask;
            }

            if (disposablePort?.Count > 0)
            {
                disposablePort?.Dispose();
                disposablePort = null;
            }
            if (disposablePort == null)
                disposablePort = new CompositeDisposable();

            return disposablePort?.Count == 0 ?
                Task.Run(
                    () => ObsConnect
                        .Subscribe()
                        .AddTo(disposablePort)) :
                Task.CompletedTask;
        }

        public void Disconnect()
        {
            disposablePort?.Dispose();
            disposablePort = null;
        }

        public void Init(SerialPortSetting serialPortSetting)
        {
            _SerialPortSetting = serialPortSetting;
        }

        public void Send(byte[] baData, int iStart = 0, int iLength = -1)
        {
            int count = iLength == -1 ? baData.Length - iStart : iLength;
            writeByte?.OnNext(new Tuple<byte[], int, int>(baData, iStart, count));

        }

        public void Dispose()
        {
            Console.WriteLine("Max Processing Time : {0}ms", MaxProcessTime);
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    disposablePort?.Dispose();
                }

                IsDisposed = true;
            }
        }
    }
}
