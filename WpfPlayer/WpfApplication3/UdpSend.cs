using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using WpfApplication3.Mcu;

namespace WpfApplication3
{
    class UdpSend
    {
        public static byte flagSend;
        public UdpSend()
        {
            Thread ThreadUdpSend = new Thread(new ThreadStart(Send));
            ThreadUdpSend.IsBackground = true; //设置为后台线程
            ThreadUdpSend.Start();
            //构造函数

        }

        public static void UdpSendData(byte[] data, int len, EndPoint ip)
        {
          
            UdpInit.mySocket.SendTo(data, len, SocketFlags.None, ip);
            Debug.WriteLine("Send HighData...");
            Debug.WriteLine(ModbusUdp.ByteToHexStr(data));
            
        }


        public static void Send()
        {
           
            byte[] data ;
            ushort addr ;      
            ushort len ;
            byte[] array;          //data+addr+len 
            byte[] Data;           //最终发送的数据
            
            while (true)
            {
                if (UdpConnect.flagValue == true)   //连接成功标志
                {
                    switch (flagSend)              //发送标志
                    {
                        case 102:                   //write_ram
                            addr = 0;
                            len = 10;
                            data = new byte[len];
                            array = Mcu.ModbusUdp.ArrayAdd(addr, len, data);
                            Data = Mcu.ModbusUdp.MBReqWrite(array);
                            break;
                        case 103:                  //read_ram
                            addr = 0;
                            len = 10;
                            data = new byte[0];
                            array = Mcu.ModbusUdp.ArrayAdd(addr, len, data);
                            Data = Mcu.ModbusUdp.MBReqRead(array);
                            break;
                        case 104:                //write_falsh
                            addr = 0;
                            len = 32;
                            data = new byte[len];
                            array = Mcu.ModbusUdp.ArrayAdd(addr, len, data);
                            Data = Mcu.ModbusUdp.MBReqWriteChip(array);
                            break;
                        case 105:               //read_falsh
                            addr = 0;
                            len = 32;
                            data = new byte[0];
                            array = Mcu.ModbusUdp.ArrayAdd(addr, len, data);
                            Data = Mcu.ModbusUdp.MBReqReadChip(array);
                            break;
                        case 106:             //GetId
                            Data = Mcu.ModbusUdp.MBReqUuid();
                            break;
                        case 107:            //GetTimeCode
                            addr = 0;
                            len = 4;
                            data = new byte[0];
                            array = Mcu.ModbusUdp.ArrayAdd(addr, len, data);
                            Data = Mcu.ModbusUdp.MBReqGetTimeCode(array);
                            break;
                        default:                        
                            Data = new byte[0];
                            break;

                    }

                    UdpSend.UdpSendData(Data, Data.Length, UdpInit.RemotePoint);
                    
                }
                Thread.Sleep(1000);
            }
        }
    }
}

