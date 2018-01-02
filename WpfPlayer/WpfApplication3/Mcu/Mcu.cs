using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace WpfApplication3.Mcu
{
    public class McuTest
    {
        private Mcu myMcu = Mcu.Instance; //for test class mcu

        public McuTest()
        {
            Thread ThreadMcuTest = new Thread(new ThreadStart(this.McuTestTask));
            ThreadMcuTest.IsBackground = true; //设置为后台线程
            ThreadMcuTest.Start();
            //构造函数
        }

        private void McuTestTask()
        {
            byte[] data = new byte[1];
            data[0] = 100;
            byte[] High = new byte[3];
            High[0] = 64;
            High[1] = 128;
            High[2] = 255;
            while (true)
            {
                //Thread.Sleep(1000);
                //data = ModbusUdp.MBReqUuid();
                //myMcu.McuSend(data, data.Length);
                //Debug.WriteLine("McuTestTask...");
                //Debug.WriteLine(ModbusUdp.ByteToHexStr(data));

                //Thread.Sleep(1000);
                Thread.Sleep(50);
                High[0] += 3;
                High[1] += 3;
                High[2] += 3;
                data = ModbusUdp.MBReqWriteHigh(High[0], High[1], High[2]);
                myMcu.McuSend(data, data.Length);
                Debug.WriteLine("McuTestTask...");
                //Debug.WriteLine(ModbusUdp.ByteToHexStr(data));
            }
        }
    }
    public sealed class Mcu
    {
        private static readonly Mcu _InstanceMcu = new Mcu();
        private McuUdp myMcuUdp;

        //委托
        public delegate void McuReceive(byte[] Data, int len);
        //设置MCU数据接收函数
        public void McuSetReceiveCallBack(McuReceive SetMcuReceiveFun)
        {
            myMcuUdp.UdpSetReceiveCallBack(new McuUdp.UdpReceive(SetMcuReceiveFun));
        }
        //MCU数据发送函数
        public int McuSend(byte[] Data, int Len)
        {
            if (myMcuUdp.UdpConnectCompleteFlag.Value)
            {
                myMcuUdp.UdpSend(Data, Len);
                return 0;
            }
            else
            {
                return -1;
            }
        }

        static Mcu()
        {
            //构造函数
        }

        private Mcu()
        {
            myMcuUdp = new McuUdp();
            //构造函数
        }

        public static Mcu Instance
        {
            get
            {
                return _InstanceMcu;
            }
        }

        private class McuUdp
        {
            private const int MonitorTickTimeoutValue = 3;

            private IPEndPoint ipLocalPoint;
            private EndPoint RemotePoint;
            private Socket mySocket;

            public BoolWithLock UdpConnectCompleteFlag = new BoolWithLock(false);
            private ByteWithLock MonitorTickAck = new ByteWithLock(0);
            private ByteWithLock MonitorTickTimeout = new ByteWithLock(MonitorTickTimeoutValue);

            //委托
            public delegate void UdpReceive(byte[] Data, int len);
            private UdpReceive UdpReceiveFun = null;

            public void UdpSetReceiveCallBack(UdpReceive SetUdpReceiveFun)
            {
                UdpReceiveFun = SetUdpReceiveFun;
            }

            public McuUdp()
            {
                Thread ThreadUdpServer = new Thread(new ThreadStart(this.UdpServerTask));
                ThreadUdpServer.IsBackground = true; //设置为后台线程
                ThreadUdpServer.Start();
                //构造函数
            }

            public void UdpSend(byte[] Data, int Len)
            {
                mySocket.SendTo(Data, Len, SocketFlags.None, RemotePoint);
            }

            private void UdpServerTask()
            {
                byte MonitorTick = 0;

                IPAddress ip;
                int port;

                //得到本机IP，设置UDP端口号    
                IPAddress.TryParse(GetLocalIP(), out ip);
                port = 0; //自动分配端口
                ipLocalPoint = new IPEndPoint(ip, port);

                //定义网络类型，数据连接类型和网络协议UDP  
                mySocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                //绑定网络地址 
                try
                {
                    mySocket.Bind(ipLocalPoint);
                }
                catch
                {
                    MessageBox.Show("网络端口被占用");
                    //System.Environment.Exit(0);
                }

                //设置广播地址
                ip = IPAddress.Broadcast;
                port = 1030;
                IPEndPoint ipep = new IPEndPoint(ip, port);
                EndPoint BroadcastRemotePoint = (EndPoint)(ipep);
                //设置客户机IP，默认为广播地址
                RemotePoint = (EndPoint)(ipep);

                //允许广播
                mySocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);

                //启动一个新的线程，执行方法this.ReceiveHandle，  
                //以便在一个独立的进程中执行数据接收的操作  

                Thread thread = new Thread(new ThreadStart(this.ReceiveHandle));
                thread.IsBackground = true; //设置为后台线程
                thread.Start();

                //发送UDP数据包  
                byte[] data;

                while (true)
                {
                    if (!UdpConnectCompleteFlag.Value)
                    {
                        data = ModbusUdp.MBReqConnect;
                        mySocket.SendTo(data, data.Length, SocketFlags.None, BroadcastRemotePoint);
                        Debug.WriteLine("Search udp server...");
                    }
                    else
                    {
                        if (MonitorTickAck.Value != MonitorTick)
                        {
                            if (MonitorTickTimeout.Value > 0)
                            {
                                MonitorTickTimeout.Value--;
                            }
                        }
                        else
                        {
                            MonitorTickTimeout.Value = MonitorTickTimeoutValue;
                        }

                        if (MonitorTickTimeout.Value == 0)
                        {
                            UdpConnectCompleteFlag.Value = false;
                            MonitorTick = 0;
                            MonitorTickAck.Value = 0;
                            MonitorTickTimeout.Value = MonitorTickTimeoutValue;
                            Debug.WriteLine("Connect lose...");
                            continue;
                        }
                        else
                        {
                            //发送UDP心跳包
                            if (MonitorTick < 0xff)
                            {
                                MonitorTick++;
                            }
                            else
                            {
                                MonitorTick = 0;
                            }
                            data = ModbusUdp.MBReqMonitor(MonitorTick);
                            mySocket.SendTo(data, data.Length, SocketFlags.None, RemotePoint);
                            Debug.WriteLine("Connect monitor...");
                        }

                    }
                    Thread.Sleep(1000);
                }
            }

            private void ReceiveHandle()
            {
                //接收数据处理线程  
                string msg;
                byte[] data = new byte[1024];
                byte[] RecData;

                while (true)
                {

                    if (mySocket == null || mySocket.Available < 1)
                    {
                        Thread.Sleep(10);
                        continue;
                    }
                    //跨线程调用控件  
                    //接收UDP数据报，引用参数RemotePoint获得源地址  
                    int rlen = mySocket.ReceiveFrom(data, ref RemotePoint);
                    if (UdpReceiveFun != null)
                    {
                        UdpReceiveFun(data, rlen);
                    }

                    if ((data[0] == 0xff) && (data[1] == 0x00))
                    {
                        UdpConnectCompleteFlag.Value = true;
                    }

                    if (UdpConnectCompleteFlag.Value)
                    {
                        if ((data[0] == 0xff) && (data[1] == 0x01))
                        {
                            MonitorTickAck.Value = data[2];
                            msg = data[2].ToString();
                            Debug.WriteLine(RemotePoint.ToString() + " : " + msg);
                        }
                        RecData = new byte[rlen];
                        Array.Copy(data, 0, RecData, 0, rlen);
                        RecData = ModbusUdp.MBRsp(RecData);
                        if (RecData != null)
                        {
                            Debug.WriteLine(ModbusUdp.ByteToHexStr(RecData));
                        }
                    }
                }
            }

            private string GetLocalIP()
            {
                try
                {
                    string HostName = Dns.GetHostName(); //得到主机名  
                    IPHostEntry IpEntry = Dns.GetHostEntry(HostName);
                    for (int i = 0; i < IpEntry.AddressList.Length; i++)
                    {
                        //从IP地址列表中筛选出IPv4类型的IP地址  
                        //AddressFamily.InterNetwork表示此IP为IPv4,  
                        //AddressFamily.InterNetworkV6表示此地址为IPv6类型  
                        if (IpEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                        {
                            return IpEntry.AddressList[i].ToString();
                        }
                    }
                    return "";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("获取本机IP出错:" + ex.Message);
                    return "";
                }
            }
        }

        private class DateBuf
        {
            private byte[] ReceiveBuf = new byte[1024];
            private byte[] SendBuf = new byte[1024];
            private int ReceiveLen;
            private int SendLen;

            public DateBuf()
            {
                Array.Clear(ReceiveBuf, 0, ReceiveBuf.Length);
                Array.Clear(SendBuf, 0, SendBuf.Length);
                ReceiveLen = 0;
                SendLen = 0;
            }

            public void SetReceiveBuf(Array Data, int len)
            {

            }
        }

        private class BoolWithLock
        {
            private bool __Value = false;

            public BoolWithLock()
            {

            }

            public BoolWithLock(bool DefaultValue)
            {
                __Value = DefaultValue;
            }

            public bool Value
            {
                get
                {
                    lock (this)
                    {
                        return __Value;
                    }
                }
                set
                {
                    lock (this)
                    {
                        __Value = value;
                    }
                }
            }
        }

        private class ByteWithLock
        {
            private byte __Value = 0;

            public ByteWithLock()
            {

            }

            public ByteWithLock(byte DefaultValue)
            {
                __Value = DefaultValue;
            }

            public byte Value
            {
                get
                {
                    lock (this)
                    {
                        return __Value;
                    }
                }
                set
                {
                    lock (this)
                    {
                        __Value = value;
                    }
                }
            }
        }

        private class ByteArrayWithLock
        {
            private byte[] __Value;
            public byte[] Value
            {
                get
                {
                    lock (this)
                    {
                        return __Value;
                    }
                }
                set
                {
                    lock (this)
                    {
                        __Value = value;
                    }
                }
            }

            public ByteArrayWithLock(int len)
            {
                __Value = new byte[len];
            }
        }
    }
}
