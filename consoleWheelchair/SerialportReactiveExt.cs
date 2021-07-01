namespace consoleWheelchair
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO.Ports;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Concurrency;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Text;
    public static class SerialportReactiveExt
    {
        public static int PatternAt(byte[] source, byte[] pattern, int startidx)
        {
            for (int i = startidx; i < source.Length; i++)
            {
                if (source.Skip(i).Take(pattern.Length).SequenceEqual(pattern))
                {
                    return i;
                }
            }

            return -1;
        }

        public static int PatternAt(byte[] source, byte[] pattern, int startidx, int length)
        {
            for (int i = startidx; i < length; i++)
            {
                if (source.Skip(i).Take(pattern.Length).SequenceEqual(pattern))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// STX를 비교해서 전문을 수신한다.
        /// </summary>
        /// <param name="this">소켓</param>
        /// <param name="startbytesWith">STX</param>
        /// <param name="timeOut">수신타임아웃</param>
        /// <returns>바이트 배열 감시</returns>
        public static IObservable<byte[]> BufferUntilSTXtoByteArray(this IObservable<char> @this,
            IObservable<byte[]> startbytesWith,
            int timeOut) => Observable.Create<byte[]>(o => {
                var dis = new CompositeDisposable();
                try
                {
                    byte[] buff = new byte[1024];

                    long elapsedTime = 0;
                    int idx = 0;
                    int startbytescount = 0;
                    byte[] startWith = new byte[5];

                    startbytesWith.Subscribe(x => {
                        startWith = x;
                        startbytescount = x.Count();
                        elapsedTime = 0;
                        idx = 0;
                    }).AddTo(dis);

                    var sub = @this.Subscribe(x => {
                        elapsedTime = 0;
                        byte data = (byte)x;
                        buff[idx] = data;
                        idx++;

                        // STX크기보다 버퍼 위치가 커야한다.
                        if (idx > startbytescount + 1)
                        {
                            // STX가 있는지 확인한다.
                            var pos = PatternAt(buff, startWith, 1);
                            if (pos >= 0)
                            {
                                byte[] segment = new byte[pos];
                                Buffer.BlockCopy(buff, 0, segment, 0, segment.Length);
                                o.OnNext(segment);
                                Array.Clear(buff, 0, idx);
                                Buffer.BlockCopy(startWith, 0, buff, 0, startWith.Length);
                                idx = startWith.Length;
                                elapsedTime = 0;
                            }
                        }
                    }).AddTo(dis);

                    Observable.Interval(TimeSpan.FromMilliseconds(1)).Subscribe(_ => {
                        elapsedTime++;
                        // 타임아웃을 초과했는가?
                        if (elapsedTime > timeOut)
                        {
                            if (idx > 0)
                            {
                                // 버퍼가 존재한다.
                                byte[] segment = new byte[idx];
                                Buffer.BlockCopy(buff, 0, segment, 0, segment.Length);
                                o.OnNext(segment);
                            }
                            idx = 0;
                            elapsedTime = 0;
                        }
                    }).AddTo(dis);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }

                return dis;
            });

        public static IObservable<string> BufferUntil(
            this IObservable<char> @this,
            IObservable<char> startsWith,
            IObservable<char> endsWith,
            int timeOut) => Observable.Create<string>(o => {
                var dis = new CompositeDisposable();
                var str = "";

                var startFound = false;
                var elapsedTime = 0;
                var startsWithL = ' ';
                startsWith.Subscribe(sw => {
                    startsWithL = sw;
                    elapsedTime = 0;
                }).AddTo(dis);
                var endsWithL = ' ';
                var ewd = endsWith.Subscribe(ew => endsWithL = ew).AddTo(dis);
                var sub = @this.Subscribe(s => {
                    elapsedTime = 0;
                    if (startFound || s == startsWithL)
                    {
                        startFound = true;
                        str = str + s;
                        if (s == endsWithL)
                        {
                            o.OnNext(str);
                            startFound = false;
                            str = "";
                        }
                    }
                }).AddTo(dis);
                Observable.Interval(TimeSpan.FromMilliseconds(1)).Subscribe(_ => {
                    elapsedTime++;
                    if (elapsedTime > timeOut)
                    {
                        startFound = false;
                        str = "";
                        elapsedTime = 0;
                    }
                }).AddTo(dis);

                return dis;
            });

        public static IObservable<string> BufferUntil(
            this IObservable<char> @this,
            IObservable<char> startsWith,
            IObservable<char> endsWith,
            IObservable<string> defaultValue,
            int timeOut) => Observable.Create<string>(o => {
                var dis = new CompositeDisposable();
                string str = "";

                var startFound = false;
                var elapsedTime = 0;
                var startsWithL = ' ';
                startsWith.Subscribe(sw => {
                    startsWithL = sw;
                    elapsedTime = 0;
                }).AddTo(dis);
                var endsWithL = ' ';
                endsWith.Subscribe(ew => endsWithL = ew).AddTo(dis);
                var defaultValueL = string.Empty;
                defaultValue.Subscribe(dv => defaultValueL = dv).AddTo(dis);
                @this.Subscribe(s => {
                    elapsedTime = 0;
                    if (startFound || s == startsWithL)
                    {
                        startFound = true;
                        str = str + s;
                        if (s == endsWithL)
                        {
                            o.OnNext(str);
                            startFound = false;
                            str = "";
                        }
                    }
                }).AddTo(dis);

                Observable.Interval(TimeSpan.FromMilliseconds(1)).Subscribe(_ => {
                    elapsedTime++;
                    if (elapsedTime > timeOut)
                    {
                        o.OnNext(defaultValueL);
                        startFound = false;
                        str = "";
                        elapsedTime = 0;
                    }
                }).AddTo(dis);

                return dis;
            });

        public static IObservable<EventPattern<SerialDataReceivedEventArgs>> DataReceivedObserver(this SerialPort @this) =>
            Observable.FromEventPattern<SerialDataReceivedEventHandler, SerialDataReceivedEventArgs>(
                h => @this.DataReceived += h,
                h => @this.DataReceived -= h);

        public static IObservable<EventPattern<SerialErrorReceivedEventArgs>> ErrorReceivedObserver(this SerialPort @this) =>
            Observable.FromEventPattern<SerialErrorReceivedEventHandler, SerialErrorReceivedEventArgs>(
                h => @this.ErrorReceived += h,
                h => @this.ErrorReceived -= h);

        public static IObservable<T> ForEach<T>(this IObservable<T[]> @this) =>
                                            Observable.Create<T>(obs => {
                                                return @this.Subscribe(list => {
                                                    foreach (var item in list)
                                                    {
                                                        if (!EqualityComparer<T>.Default.Equals(item, default))
                                                        {
                                                            obs.OnNext(item);
                                                        }
                                                    }
                                                }, obs.OnError, obs.OnCompleted);
                                            });

        public static IObservable<TSource> OnErrorRetry<TSource>(this IObservable<TSource> source) => source.Retry();

        public static IObservable<TSource> OnErrorRetry<TSource, TException>(
            this IObservable<TSource> source,
            Action<TException> onError)
where TException : Exception => source.OnErrorRetry(onError, TimeSpan.Zero);

        public static IObservable<TSource> OnErrorRetry<TSource, TException>(
            this IObservable<TSource> source,
            Action<TException> onError,
            TimeSpan delay)
where TException : Exception => source.OnErrorRetry(onError, int.MaxValue, delay);

        public static IObservable<TSource> OnErrorRetry<TSource, TException>(
            this IObservable<TSource> source,
            Action<TException> onError,
            int retryCount)
where TException : Exception => source.OnErrorRetry(onError, retryCount, TimeSpan.Zero);

        public static IObservable<TSource> OnErrorRetry<TSource, TException>(
            this IObservable<TSource> source,
            Action<TException> onError,
            int retryCount,
            TimeSpan delay)
where TException : Exception => source.OnErrorRetry(onError, retryCount, delay, Scheduler.Default);

        public static IObservable<TSource> OnErrorRetry<TSource, TException>(
                        this IObservable<TSource> source, Action<TException> onError, int retryCount, TimeSpan delay, IScheduler delayScheduler)
                        where TException : Exception
        {
            var result = Observable.Defer(() => {
                var dueTime = (delay.Ticks < 0) ? TimeSpan.Zero : delay;
                var empty = Observable.Empty<TSource>();
                var count = 0;

                IObservable<TSource> self = null;
                self = source.Catch((TException ex) => {
                    onError(ex);
                    return (++count < retryCount)
                    ? (dueTime == TimeSpan.Zero)
                    ? self.SubscribeOn(Scheduler.CurrentThread)
                    : empty.Delay(dueTime, delayScheduler).Concat(self).SubscribeOn(Scheduler.CurrentThread)
                    : Observable.Throw<TSource>(ex);
                });

                return self;
            });

            return result;
        }

        public static IObservable<bool> WhileIsOpen(this SerialControl @this, TimeSpan timespan) =>
            Observable.Defer(() => Observable.Create<bool>(obs => {
                var isOpen = Observable.Interval(timespan).CombineLatest(@this.isOpen, (a, b) => b).Where(x => x);
                return isOpen.Subscribe(obs);
            }));
    }

    /// <summary>
    /// 해제 확장기능
    /// </summary>
    public static class IDisposableExtensions
    {
        /// <summary>
        /// 해제 기능 추가
        /// </summary>
        public static T AddTo<T>(this T disposable, ICollection<IDisposable> container)
            where T : IDisposable
        {
            container.Add(disposable);
            return disposable;
        }
    }
}
