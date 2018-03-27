using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WpfApplication3.Mcu;
using System.Management;

namespace WpfApplication3
{
    class UdpInit
    {
        public static IPEndPoint ipLocalPoint;
        public static EndPoint RemotePoint;
        public static EndPoint BroadcastRemotePoint;
        public static Socket mySocket;

        /// <summary>
        /// udp初始化
        /// </summary>
        public  void  udpInit()
        {        
            IPAddress ip;
            int port;
            string localIp;
            localIp = GetLocalIP();
            string[] splitIp = localIp.Split('.');

            if (localIp=="127.0.0.1")
            {
                MessageBox.Show("网络没连接,请连接网络");
                Debug.Write("获取不到IP地址");
            }

            //判断能否获取到ip地址
            if(splitIp[0]=="169")
            {
                SetNetworkAdapter();
            }


            //得到本机IP，设置UDP端口号    
            bool b=IPAddress.TryParse(localIp, out ip);

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
            BroadcastRemotePoint = (EndPoint)(ipep);
            //设置客户机IP，默认为广播地址
            RemotePoint = (EndPoint)(ipep);

            //允许广播
            mySocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            UdpConnect myUdpConnect = new UdpConnect(); 
        }


        /// <summary>
        /// 修改静态IP地址
        /// </summary>
        private void SetNetworkAdapter()
        {
            ManagementBaseObject inPar = null;
            ManagementBaseObject outPar = null;
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                if (!(bool)mo["IPEnabled"])
                    continue;

                //设置ip地址和子网掩码 
                inPar = mo.GetMethodParameters("EnableStatic");
                inPar["IPAddress"] = new string[] { "192.168.1.112", "192.168.1.249" }; // 1.备用 2.IP
                inPar["SubnetMask"] = new string[] { "255.255.255.0", "255.255.255.0" };
                outPar = mo.InvokeMethod("EnableStatic", inPar, null);

                //设置网关地址 
                inPar = mo.GetMethodParameters("SetGateways");
                inPar["DefaultIPGateway"] = new string[] { "192.168.1.1", "192.168.16.254" }; // 1.网关;2.备用网关 
                outPar = mo.InvokeMethod("SetGateways", inPar, null);

                //设置DNS 
                inPar = mo.GetMethodParameters("SetDNSServerSearchOrder");
                inPar["DNSServerSearchOrder"] = new string[] { "211.97.168.129", "202.102.152.3" }; // 1.DNS 2.备用DNS 
                outPar = mo.InvokeMethod("SetDNSServerSearchOrder", inPar, null);
                break;
            }
        }

        /// <summary>
        /// 获取本地ip地址
        /// </summary>
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
                return "192.168.1.109";
            }
            catch (Exception ex)
            {
                MessageBox.Show("获取本机IP出错:" + ex.Message);
                return "";
            }
        }
    }
}
