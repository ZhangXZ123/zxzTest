using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Diagnostics;
using WpfApplication3.Mcu;
using System.Windows.Threading;

namespace WpfApplication3
{
    public class UdpConnect
    {
      public static bool flagValue = false;
      UdpSend mysend = new UdpSend();

        public UdpConnect()
        {
            Thread ThreadUdpServer = new Thread(new ThreadStart(this.UdpServerTask));
            ThreadUdpServer.IsBackground = true; //设置为后台线程
            ThreadUdpServer.Start();
            //构造函数
        }

        private void UdpServerTask()
        {

            //启动一个新的线程，执行方法this.ReceiveHandle，  
            //以便在一个独立的进程中执行数据接收的操作  
            byte MonitorTick=0;
            Thread thread = new Thread(new ThreadStart(this.ReceiveHandle));
            thread.IsBackground = true; //设置为后台线程
            thread.Start();

            //发送UDP数据包  
            byte[] data;
            
            while (true)
            {
                if (flagValue == false)
                {
                    data = Mcu.ModbusUdp.MBReqConnect();
                    UdpSend.UdpSendData(data, data.Length, UdpInit.BroadcastRemotePoint);
                    Debug.WriteLine("Search server");
                }

                //else
                //{
                //    if
                //    //发送UDP心跳包
                //    if (MonitorTick < 0xff)
                //    {
                //        MonitorTick++;
                //    }
                //    else
                //    {
                //        MonitorTick = 0;
                //    }
                //    data = ModbusUdp.MBReqMonitor(MonitorTick);     
                //    UdpSend.UdpSendData(data, data.Length, UdpInit.RemotePoint);
                //    Debug.WriteLine("Connect monitor...");
                //}
                    Thread.Sleep(1000);
            }

        }

        delegate void ReceiveCallback(int rlen,byte [] data);

        public void SetReceiveData(int rlen,byte [] data)
        {
            byte[] RecData;
            RecData = new byte[rlen];
            Array.Copy(data, 0, RecData, 0, rlen);
            Debug.WriteLine(ModbusUdp.ByteToHexStr(RecData));

            RecData = ModbusUdp.MBRsp(RecData);
            Debug.WriteLine(ModbusUdp.ByteToHexStr(RecData));
            if (RecData != null)
            {
                if (RecData[0] == 0 && RecData[1] == 0 && RecData[2] == 0x01 && RecData[3] == 0x41)
                {
                    flagValue = true;
                    UdpSend.flagSend = (byte)Mcu.ModbusUdp.MBFunctionCode.Write;
                }
            }

        }

        private void ReceiveHandle()
        {
            //接收数据处理线程  
            byte[] data = new byte[1024];
            
            while (true)
            {

                if (UdpInit.mySocket == null || UdpInit.mySocket.Available < 1)
                {
                    Thread.Sleep(10);
                    continue;
                }

                //跨线程调用控件  
                //接收UDP数据报，引用参数RemotePoint获得源地址 
                try
                {
                    int rlen = UdpInit.mySocket.ReceiveFrom(data, ref UdpInit.RemotePoint);
                    ReceiveCallback tx = SetReceiveData;
                    tx(rlen,data);
                    
                }
                catch
                {

                }

            }
        }


    }
}
