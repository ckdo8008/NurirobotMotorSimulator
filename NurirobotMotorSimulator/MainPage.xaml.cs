using NurirobotMotorSimulator.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 빈 페이지 항목 템플릿에 대한 설명은 https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x412에 나와 있습니다.

namespace NurirobotMotorSimulator
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            //var devices = DeviceInfo.GetPorts();
            var devices = new List<SerialPortInfo>();
            var aqs = SerialDevice.GetDeviceSelector();
            var deviceCollection = DeviceInformation.FindAllAsync(aqs).GetAwaiter();
            deviceCollection.OnCompleted(() => {
                var result = deviceCollection.GetResult();
                foreach (var item in result)
                {
                    var serialDevice = SerialDevice.FromIdAsync(item.Id).GetAwaiter();
                    var rr = serialDevice.GetResult();
                    Debug.WriteLine(rr.PortName);
                    //SerialPort sp = new SerialPort();
                    bool isnowusing = false;
                    try
                    {
                        //sp.PortName = rr.PortName;
                        //sp.Open();
                        //sp.Close();
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e);
                        isnowusing = true;
                    }
                    devices.Add(new SerialPortInfo { PortName = rr.PortName, IsNowUsing = isnowusing });
                }

                foreach (var item in devices)
                {
                    if (!item.IsNowUsing)
                        Comports.Items.Add(item.PortName);
                }
                if (Comports.Items.Count > 0)
                    Comports.SelectedIndex = 0;
            });

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Gauge.StartPOS(1000 * 5);
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
