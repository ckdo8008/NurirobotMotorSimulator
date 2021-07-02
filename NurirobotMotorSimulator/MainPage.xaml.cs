namespace NurirobotMotorSimulator
{
    using NurirobotMotorSimulator.Helpers;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Ports;
    using System.Linq;
    using System.Runtime.InteropServices.WindowsRuntime;
    using System.Threading;
    using System.Threading.Tasks;
    using Windows.Devices.Enumeration;
    using Windows.Devices.SerialCommunication;
    using Windows.Foundation;
    using Windows.Foundation.Collections;
    using Windows.Storage.Streams;
    using Windows.UI.Core.Preview;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Controls.Primitives;
    using Windows.UI.Xaml.Data;
    using Windows.UI.Xaml.Input;
    using Windows.UI.Xaml.Media;
    using Windows.UI.Xaml.Navigation;
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Dictionary<string, SerialPortInfo> devices = new Dictionary<string, SerialPortInfo>();
        
        //Thread thListen = null;
        CancellationTokenSource mCTS;
        Task taskDeviceOpen = null;

        public MainPage()
        {
            this.InitializeComponent();

            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += MainPage_CloseRequested;
            //var devices = DeviceInfo.GetPorts();
            //var devices = new List<SerialPortInfo>();
            var aqs = SerialDevice.GetDeviceSelector();
            var deviceCollection = DeviceInformation.FindAllAsync(aqs).GetAwaiter();
            deviceCollection.OnCompleted(() => {
                var result = deviceCollection.GetResult();
                foreach (var item in result)
                {
                    var serialDevice = SerialDevice.FromIdAsync(item.Id).GetAwaiter();
                    var rr = serialDevice.GetResult();
                    if (rr == null)
                        continue;

                    Debug.WriteLine(rr.PortName);
                    bool isnowusing = false;
                    devices.Add(rr.PortName, new SerialPortInfo { PortName = rr.PortName, IsNowUsing = isnowusing, PortID= item.Id });
                    rr.Dispose();
                }

                foreach (var item in devices)
                {
                    Comports.Items.Add(item.Key);
                }
                if (Comports.Items.Count > 0)
                    Comports.SelectedIndex = 0;
            });

        }

        private void MainPage_CloseRequested(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            CloseTask();
            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested -= MainPage_CloseRequested;
        }

        private void CloseTask()
        {
            //if (thListen != null)
            //{
            //    if (mCTS != null)
            //        mCTS.Cancel();
            //    thListen.Join();
            //}
            if (taskDeviceOpen != null)
            {
                if (mCTS != null)
                    mCTS.Cancel();

                if(!taskDeviceOpen.IsCompleted) 
                    taskDeviceOpen.Wait(100);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Gauge.StartPOS(1000 * 5);
            //thListen = new Thread(
            CloseTask();

            mCTS = new CancellationTokenSource();
            string id = devices[(String)Comports.SelectedValue].PortID;
            string bps = (String)BPS.SelectedValue;
            taskDeviceOpen = new Task(async () =>
            {
                await OpenPort(id, bps, mCTS.Token);
            });
            taskDeviceOpen.Start();
            //thListen = new Thread(new ParameterizedThreadStart(ReadThread));
            //thListen.Start(currentDevice);
        }

        private async Task OpenPort(string id, string bpsstr, CancellationToken token)
        {
            SerialDevice currentDevice = null;
            try
            {
                while (true)
                {
                    var tmp = SerialDevice.FromIdAsync(id).AsTask(token);
                    currentDevice = await tmp;
                    if (currentDevice != null)
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                currentDevice = null;
            }

            if (currentDevice == null)
            {
                Debug.WriteLine("Port is not Opened !!!!");
                return;
            }

            uint bps = uint.Parse(bpsstr);
            currentDevice.BaudRate = bps;
            currentDevice.DataBits = 8;
            currentDevice.StopBits = SerialStopBitCount.One;
            currentDevice.Parity = SerialParity.None;
            currentDevice.Handshake = SerialHandshake.None;
            currentDevice.ReadTimeout = TimeSpan.FromMilliseconds(1);
            currentDevice.WriteTimeout = TimeSpan.FromMilliseconds(1);

            DataReader dataReaderObject = null;
            try
            {
                dataReaderObject = new DataReader(currentDevice.InputStream);
                while (true)
                {
                    await ReadAsync(dataReaderObject, token);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
            finally
            {
                if (dataReaderObject != null)
                {
                    dataReaderObject.DetachStream();
                    dataReaderObject.Dispose();
                }
                if (currentDevice != null)
                    currentDevice.Dispose();
            }
        }

        private async Task ReadAsync(DataReader reader, CancellationToken cancellationToken)
        {
            Task<UInt32> loadAsyncTask;

            uint ReadBufferLength = 256;
            reader.InputStreamOptions = InputStreamOptions.Partial;
            loadAsyncTask = reader.LoadAsync(ReadBufferLength).AsTask(cancellationToken);

            UInt32 bytesRead = await loadAsyncTask;
            byte[] buff = new byte[bytesRead];

            if (bytesRead > 0)
            {
                reader.ReadBytes(buff);
                Recv(buff);
            }
        }

        static int IndexOfBytes(byte[] array, byte[] pattern, int startIndex, int count)
        {
            if (array == null || array.Length == 0 || pattern == null || pattern.Length == 0 || count == 0)
            {
                return -1;
            }
            int i = startIndex;
            int endIndex = count > 0 ? Math.Min(startIndex + count, array.Length) : array.Length;
            int fidx = 0;
            int lastFidx = 0;

            while (i < endIndex)
            {
                lastFidx = fidx;
                fidx = (array[i] == pattern[fidx]) ? ++fidx : 0;
                if (fidx == pattern.Length)
                {
                    return i - fidx + 1;
                }
                if (lastFidx > 0 && fidx == 0)
                {
                    i = i - lastFidx;
                    lastFidx = 0;
                }
                i++;
            }
            return -1;
        }

        static byte[] recvBuffer = new byte[100];
        static int buffIndex = 0;
        static readonly byte[] HEADER = { 0xFF, 0xFE };
        private void Recv(byte[] arg)
        {
            try
            {
                Debug.WriteLine(string.Format("{0}\t Recv : {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), BitConverter.ToString(arg).Replace("-", "")));
                byte[] tmpdata = new byte[buffIndex + arg.Length];
                Array.Copy(recvBuffer, 0, tmpdata, 0, buffIndex);
                Array.Copy(arg, 0, tmpdata, buffIndex, arg.Length);
                int idx = 0;

                // 최소 프로토콜 사이즈보다 부족하면 무시
                if (tmpdata.Length < 6)
                {
                    // 미처리 데이터 적제
                    Array.Copy(tmpdata, idx, recvBuffer, 0, tmpdata.Length);
                    buffIndex = tmpdata.Length;
                    return;
                }

                while (true)
                {
                    var chkpos = IndexOfBytes(tmpdata, HEADER, idx, tmpdata.Length);
                    if (chkpos != -1)
                    {
                        var scndpos = IndexOfBytes(tmpdata, HEADER, chkpos + 1, tmpdata.Length);
                        if (scndpos == -1)
                        {
                            // 다음 데이터는 없음.
                            if (tmpdata[chkpos + 3] + 4 == tmpdata.Length - chkpos)
                            {
                                // 정상 사이즈일 경우
                                var focusdata = tmpdata.Skip(chkpos).ToArray();
                                BroadcastData(focusdata);
                            }
                            else
                            {
                                // 미처리 데이터 적제
                                Array.Copy(tmpdata, chkpos, recvBuffer, 0, tmpdata.Length - chkpos);
                                buffIndex = tmpdata.Length;
                            }
                            buffIndex = 0;
                            break;
                        }
                        else
                        {
                            // 다중 데이터 순차 처리
                            var focusdata = tmpdata.Skip(chkpos).Take(scndpos - chkpos).ToArray();
                            BroadcastData(focusdata);
                            idx = scndpos;
                        }
                    }
                    else
                    {
                        // 미처리 데이터 적제
                        Array.Copy(tmpdata, idx, recvBuffer, 0, tmpdata.Length);
                        buffIndex = tmpdata.Length;
                        break;
                    }
                }
            } 
            catch (Exception e)
            {
                Debug.WriteLine(e);
                buffIndex = 0;
            }
        }

        private void BroadcastData(byte[] arg)
        {
            Debug.WriteLine(string.Format("Protocol : {0}", BitConverter.ToString(arg).Replace("-", "")));
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // 5sec stop
            Gauge.Stop(1000 * 5);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            // Stop
            Gauge.Stop(0);
        }
    }
}
