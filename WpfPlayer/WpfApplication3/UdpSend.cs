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
        public static double  count;
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
            Debug.WriteLine("Send HighData...{0}",count++);
            Debug.WriteLine(ModbusUdp.ByteToHexStr(data)+"mo");
            
        }

        public static void SendWrite(double pos )
        {
            byte[] data;
            ushort addr;
            ushort len;
            byte[] array;          //data+addr+len 
            byte[] Data;           //最终发送的数据
            pos=pos*1000;
            Debug.WriteLine(pos);
            addr = 0;
            len = 10;
            data = new byte[len];       
            try
            {
                data[0] = Module.actionFile[(int)(3 * (pos / 50))];         //1号缸            
                data[1] = Module.actionFile[(int)(3 * (pos / 50) + 1)];     //2号缸
                data[2] = Module.actionFile[(int)(3 * (pos / 50) + 2)];     //3号缸
                data[3] = 0;                                                //4号缸
                data[4] = 0;                                                //5号缸
                data[5] = 0;                                                //6号缸
                data[6] = 0;                                                //保留
                data[7] = 0;                                                //保留
                data[8] = Module.effectFile[(int)(2 * (pos / 50))];         //座椅特效  
                data[9] = Module.effectFile[(int)(2 * (pos / 50) + 1)];     //环境特效 
                Debug.WriteLine((int)(3 * (pos / 50)));
            }
            catch(Exception e)
            {
                Debug.WriteLine(e);
            }
            array = Mcu.ModbusUdp.ArrayAdd(addr, len, data);
            Data = Mcu.ModbusUdp.MBReqWrite(array);
            UdpSendData(Data, Data.Length, UdpInit.RemotePoint);
            //return Data;

        }

        public static byte [] SendRead()
        {
            byte[] data;
            ushort addr;
            ushort len;
            byte[] array;          //data+addr+len 
            byte[] Data;           //最终发送的数据

            addr = 0;
            len = 10;
            data = new byte[0];
            
            array = Mcu.ModbusUdp.ArrayAdd(addr, len, data);
            Data = Mcu.ModbusUdp.MBReqRead(array);

            return Data;

        }

        public static byte [] SendWriteChip()
        {
            byte[] data;
            ushort addr;
            ushort len;
            byte[] array;          //data+addr+len 
            byte[] Data;           //最终发送的数据

            addr = 0;
            len = 32;
            data = new byte[len];
            data[0] = Module.deadlineYY;
            data[1] = Module.deadlineMM;
            data[2] = Module.deadlineDD;
            data[3] = 2;
            data[4] = 4;
            data[5] = Module.deadlineOrPermanent;
            data[6] = Module.YY;
            data[7] = Module.MM;
            data[8] = Module.DD;
            data[9] = 11;
            data[10] = 12;

            array = Mcu.ModbusUdp.ArrayAdd(addr, len, data);
            Data = Mcu.ModbusUdp.MBReqWriteChip(array);

            return Data;

        }

        public static byte [] SendReadChip()
        {
            byte[] data;
            ushort addr;
            ushort len;
            byte[] array;          //data+addr+len 
            byte[] Data;           //最终发送的数据

            addr = 0;
            len = 32;
            data = new byte[0];

            array = Mcu.ModbusUdp.ArrayAdd(addr, len, data);
            Data = Mcu.ModbusUdp.MBReqReadChip(array);
            return Data;
        }

        public static byte [] SendGetId()
        {
            byte[] Data;           //最终发送的数据
            Data = Mcu.ModbusUdp.MBReqUuid();
            return Data;
        }

        public static byte [] SendGetTimeCode()
        {
            byte[] data;
            ushort addr;
            ushort len;
            byte[] array;          //data+addr+len 
            byte[] Data;           //最终发送的数据

            addr = 0;
            len = 4;
            data = new byte[0];

            array = Mcu.ModbusUdp.ArrayAdd(addr, len, data);
            Data = Mcu.ModbusUdp.MBReqGetTimeCode(array);
            return Data;
        }




        public static void Send()
        {
           
            //byte[] data ;
            //ushort addr ;      
            //ushort len ;
            //byte[] array;          //data+addr+len 
            byte[] Data;           //最终发送的数据

            while (true)
            {
                if (UdpConnect.flagValue == true)   //连接成功标志
                {
                    switch (flagSend)              //发送标志
                    {
                        case 101:
                        //addr = 0;
                        //len = 10;
                        //data = new byte[len];
                        //array = Mcu.ModbusUdp.ArrayAdd(addr, len, data);
                        Data = Mcu.ModbusUdp.MBReqMonitor(1);
                            break;
                        case 102:                   //read_ram
                            //addr = 0;
                            //len = 10;
                            //data = new byte[len];
                            //array = Mcu.ModbusUdp.ArrayAdd(addr, len, data);
                            //Data = Mcu.ModbusUdp.MBReqWrite(array);
                            Data=SendRead();
                            break;
                        case 103:                  //write_ram
                            //addr = 0;
                            //len = 10;
                            //data = new byte[0];
                            //array = Mcu.ModbusUdp.ArrayAdd(addr, len, data);
                            //Data = Mcu.ModbusUdp.MBReqRead(array);

                             Data = SendRead();
                            break;
                        case 104:                //read_falsh
                            //addr = 0;
                            //len = 32;
                            //data = new byte[len];
                            //array = Mcu.ModbusUdp.ArrayAdd(addr, len, data);
                            //Data = Mcu.ModbusUdp.MBReqWriteChip(array);
                            Data = SendReadChip();
                            break;
                        case 105:               //write_falsh
                            //addr = 0;
                            //len = 32;
                            //data = new byte[0];
                            //array = Mcu.ModbusUdp.ArrayAdd(addr, len, data);
                            //Data = Mcu.ModbusUdp.MBReqReadChip(array);
                            Data = SendWriteChip();
                            break;
                        case 106:             //GetId
                            //Data = Mcu.ModbusUdp.MBReqUuid();
                            Data = SendGetId();
                            flagSend = 0;
                            break;
                        case 107:            //GetTimeCode
                            //addr = 0;
                            //len = 4;
                            //data = new byte[0];
                            //array = Mcu.ModbusUdp.ArrayAdd(addr, len, data);
                            //Data = Mcu.ModbusUdp.MBReqGetTimeCode(array);
                            Data = SendGetTimeCode();
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

