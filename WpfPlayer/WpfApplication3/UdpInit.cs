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

namespace WpfApplication3
{
    class UdpInit
    {
        public static IPEndPoint ipLocalPoint;
        public static EndPoint RemotePoint;
        public static EndPoint BroadcastRemotePoint;
        public static Socket mySocket;

        public  void  udpInit()
        {
        
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
             BroadcastRemotePoint = (EndPoint)(ipep);
            //设置客户机IP，默认为广播地址
            RemotePoint = (EndPoint)(ipep);

            //允许广播
            mySocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);

            UdpConnect myUdpConnect = new UdpConnect(); 
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
}
