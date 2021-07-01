namespace NurirobotMotorSimulator.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO.Ports;
    using Windows.Devices.Enumeration;
    using Windows.Devices.SerialCommunication;

    public class DeviceInfo
    {
        public static SerialPortInfo[] GetPorts()
        {


            //List<string> portNamesList = new List<string>();
            //foreach (var item in deviceCollection)
            //{
            //    var serialDevice = SerialDevice.FromIdAsync(item.Id).GetResults();
            //    var portName = serialDevice.PortName;
            //    portNamesList.Add(portName);
            //}

            List<SerialPortInfo> ret = new List<SerialPortInfo>();
            //var tmps = SerialPort.GetPortNames();
            //if (tmps != null)
            //{
            //    SerialPort sp = new SerialPort();
            //    foreach (var item in tmps)
            //    {
            //        bool isnowusing = false;
            //        try
            //        {
            //            sp.PortName = item;
            //            sp.Open();
            //            sp.Close();
            //        }
            //        catch (Exception e)
            //        {
            //            Debug.WriteLine(e);
            //            isnowusing = true;
            //        }

            //        ret.Add(new SerialPortInfo { PortName = item, IsNowUsing = isnowusing });
            //    }
            //}

            return ret.ToArray();
        }
    }
}
