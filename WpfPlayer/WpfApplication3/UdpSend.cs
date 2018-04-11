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
using System.Windows;

namespace WpfApplication3
{
    class UdpSend
    {
        public static byte flagSend;
        public static double count;
        public static int Line;
        public static int rate;
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
            Debug.WriteLine("Send Data{0}", count++);
            Debug.WriteLine(ModbusUdp.ByteToHexStr(data));
        }

        public static void SendWrite(double pos)
        {
            byte[] data;
            ushort addr;
            ushort len;
            byte[] array;          //data+addr+len 
            byte[] Data;           //最终发送的数据
            pos = pos * 1000;
            Debug.WriteLine(pos);
            addr = 0;
            len = 10;
            data = new byte[len];
            int num1 = 3 * (int)(pos / 50);                  //actionFile数组下标
            int num2 = 3 * (int)(pos / 50) + 1;
            int num3 = 3 * (int)(pos / 50) + 2;
            int num4 = 2 * (int)(pos / 50);                  //effectFile数组下标
            int num5 = 2 * (int)(pos / 50) + 1;
            Debug.WriteLine(num1 + " " + num2 + " " + num3);
            try
            {
                if (num3 > Module.actionFile.Length)
                {
                    data[0] = 0;                          //1号缸            
                    data[1] = 0;                          //2号缸
                    data[2] = 0;                          //3号缸
                }
                else
                {
                    data[0] = Module.actionFile[num1];                          //1号缸            
                    data[1] = Module.actionFile[num2];                          //2号缸
                    data[2] = Module.actionFile[num3];                          //3号缸
                }
                data[3] = 0;                                                    //4号缸
                data[4] = 0;                                                    //5号缸
                data[5] = 0;                                                    //6号缸
                data[6] = 0;                                                    //保留
                data[7] = 0;                                                    //保留
                if (num5 > Module.effectFile.Length)
                {
                    data[8] = 0;                                                 //座椅特效  
                    data[9] = 0;                                                 //环境特效 
                }
                else
                {
                    data[8] = Module.effectFile[num4];                          //座椅特效  
                    data[9] = Module.effectFile[num5];                          //环境特效 
                }
                Debug.WriteLine((int)(3 * (pos / 50)));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
            array = Mcu.ModbusUdp.ArrayAdd(addr, len, data);
            Data = Mcu.ModbusUdp.MBReqWrite(array);
            UdpSendData(Data, Data.Length, UdpInit.RemotePoint);
            //return Data;
        }

        /// <summary>
        /// int整型转换成byte数组
        /// </summary>
        /// <param name="value">传入的整型参数值</param>
        /// <returns></returns>
        public static byte[] intToBytes(int value)
        {
            byte[] src = new byte[4];
            src[0] = (byte)((value >> 24) & 0xFF);
            src[1] = (byte)((value >> 16) & 0xFF);
            src[2] = (byte)((value >> 8) & 0xFF);
            src[3] = (byte)(value & 0xFF);
            return src;
        }


        /// <summary>
        /// 三合一驱动器发送指令
        /// </summary>

        public static void QuDong(double pos)
        {
            byte[] data_buf = new byte[38];
            //int data_len = 0;
            byte[] src = new byte[4];

            pos = pos * 1000;
            Debug.WriteLine(pos);
            int num1 = 3 * (int)(pos / 50);                 //actionFile数组下标
            int num2 = 3 * (int)(pos / 50) + 1;
            int num3 = 3 * (int)(pos / 50) + 2;
            int X;                                         //实际脉冲数 
            int Y;
            int Z;

            //实际脉冲数

            if (num3 < Module.actionFile.Length)
            {
                X = Module.actionFile[num1] * 1089;
                Y = Module.actionFile[num2] * 1089;
                Z = Module.actionFile[num3] * 1089;
            }
            else
            {
                X = 0;
                Y = 0;
                Z = 0;
            }


            //震动脉冲数
            /*
            if (rate==2)
            {
                X = 170000;
                Y = 170000;
                Z = 170000;
                rate = 0;
            }
            else
            {
                X = 0;
                Y = 0;
                Z = 0;
                rate++;
            }
            */

            try
            {
                data_buf[0] = 0x55;
                data_buf[1] = 0xaa;
                data_buf[2] = 0;
                data_buf[3] = 0;

                data_buf[4] = 0x14;
                data_buf[5] = 0x01;
                data_buf[6] = 0;
                data_buf[7] = 0;

                data_buf[8] = 0xff;
                data_buf[9] = 0xff;
                data_buf[10] = 0xff;
                data_buf[11] = 0xff;

                src = intToBytes(Line++);
                Array.Copy(src, 0, data_buf, 12, 4);

                //data_buf[12] = 0;
                //data_buf[13] = 0;
                //data_buf[14] = 0;
                //data_buf[15] = 0x01;

                data_buf[16] = 0;
                data_buf[17] = 0;
                data_buf[18] = 0;
                data_buf[19] = 0x32;

                src = intToBytes(X);
                Array.Copy(src, 0, data_buf, 20, 4);

                src = intToBytes(Y);
                Array.Copy(src, 0, data_buf, 24, 4);

                src = intToBytes(Z);
                Array.Copy(src, 0, data_buf, 28, 4);

                //震动参数
                data_buf[32] = 0x0f;
                data_buf[33] = 0xff;
                data_buf[34] = 0x12;
                data_buf[35] = 0x34;
                data_buf[36] = 0x56;
                data_buf[37] = 0x78;

                /*
                //正常播放
                data_buf[32] = 0x12;
                data_buf[33] = 0x34;
                data_buf[34] = 0x56;
                data_buf[35] = 0x78;
                data_buf[36] = 0xab;
                data_buf[37] = 0xcd;
                */
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, "发送失败");
            }
            UdpSendData(data_buf, data_buf.Length, UdpInit.RemotePoint);
            // return data_buf;
        }

        public static byte[] SendRead()
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

        public static byte[] SendWriteChip()
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

        public static byte[] SendReadChip()
        {
            byte[] data;
            ushort addr;
            ushort len;
            byte[] array;          //data+addr+len 
            byte[] Data;           //最终发送的数据

            addr = 0;
            len = 3;
            data = new byte[0];

            array = Mcu.ModbusUdp.ArrayAdd(addr, len, data);
            Data = Mcu.ModbusUdp.MBReqReadChip(array);
            return Data;
        }

        public static byte[] SendGetId()
        {
            byte[] Data;           //最终发送的数据
            Data = Mcu.ModbusUdp.MBReqUuid();
            return Data;
        }

        public static byte[] SendGetTimeCode()
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


        /// <summary>
        /// 根据功能码来判断要发送的指令
        /// </summary>
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
                            Data = Mcu.ModbusUdp.MBReqMonitor(1);
                            break;
                        case 102:                   //read_ram                           
                            Data = SendRead();
                            break;
                        //case 103:                  //write_ram

                        // Data = SendWrite(Window1.sliderPositionValue);
                        // break;
                        case 104:                //read_falsh                           
                            Data = SendReadChip();
                            flagSend = 0;
                            break;
                        case 105:               //write_falsh                           
                            Data = SendWriteChip();
                            break;
                        case 106:             //GetId
                            //Data = Mcu.ModbusUdp.MBReqUuid();
                            Data = SendGetId();
                            flagSend = 0;
                            break;
                        case 107:            //GetTimeCode                           
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

