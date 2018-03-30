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
using System.IO;
namespace WpfApplication3
{
    public class UdpConnect
    {
        public static bool flagValue = false;
        public static double TimeCode;
        public static string strTimeCode;
        public static string uuid;
        public byte monitorTickRx;             
        public byte monitorTickTimeOut;         
        public static bool registerFlag;

        UdpSend mysend = new UdpSend();


        public UdpConnect()
        {
            Thread ThreadUdpServer = new Thread(new ThreadStart(this.UdpServerTask));
            ThreadUdpServer.IsBackground = true;                                      //设置为后台线程
            ThreadUdpServer.Start();
            //构造函数
        }

        private void UdpServerTask()
        {
            //启动一个新的线程，执行方法this.ReceiveHandle，  
            //以便在一个独立的进程中执行数据接收的操作  
            byte monitorTick = 0;
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
                    Debug.WriteLine("连接"+data);
                }

                else
                {
                    //发送UDP心跳包
                    if (monitorTickRx != monitorTick)
                    {
                        if (monitorTickRx > 0)
                        {
                            monitorTickTimeOut++;
                        }
                    }

                    if (monitorTickTimeOut == 3)     //计时超过3秒，重新连接
                    {
                        flagValue = false;
                        monitorTick = 0;
                        monitorTickRx = 0;
                        monitorTickTimeOut = 0;
                        Debug.WriteLine("Connect lose...");
                    }

                    if (monitorTick < 0xff)
                    {
                        monitorTick++;
                    }
                    else
                    {
                        monitorTick = 0;
                    }

                    data = ModbusUdp.MBReqMonitor(monitorTick);
                    UdpSend.UdpSendData(data, data.Length, UdpInit.RemotePoint);
                    Debug.WriteLine("Connect monitor...");
                }
                Thread.Sleep(1000);
            }
        }

        delegate void ReceiveCallback(int rlen, byte[] data);

        public void SetReceiveData(int rlen, byte[] data)
        {
            byte[] RecData;
            RecData = new byte[rlen];
            Array.Copy(data, 0, RecData, 0, rlen);
            //Debug.WriteLine(RecData);
            Debug.WriteLine(ModbusUdp.ByteToHexStr(RecData));
            if (RecData[0] == 0xff && RecData[1] == 0x6c)
            {
                //要发送数据格式
                double hours = (RecData[6]) * 60 * 60;
                double minutes = (RecData[7]) * 60;
                double seconds = RecData[8];
                double frame = RecData[9] / 20.000;

                // s[i].ToString("X").Length < 2 ? "0" + s[i].ToString("X") : s[i].ToString("X"));
                string strhours = RecData[6].ToString().Length < 2 ? "0" + RecData[6].ToString() : RecData[6].ToString();
                string strminutes = RecData[7].ToString().Length < 2 ? "0" + RecData[7].ToString() : RecData[7].ToString();
                string strseconds = RecData[8].ToString().Length < 2 ? "0" + RecData[8].ToString() : RecData[8].ToString();
                string strframe = RecData[9].ToString().Length < 2 ? "0" + RecData[9].ToString() : RecData[9].ToString();

                strTimeCode = strhours + ":" + strminutes + ":" + strseconds;
                //strTimeCode = RecData[6].ToString() + ":" + RecData[7].ToString() + ":" + RecData[8].ToString() + ":" + RecData[9].ToString();
                TimeCode = hours + minutes + seconds + frame;
                UdpSend.SendWrite(TimeCode);

                //UdpSend.flagSend = (byte)Mcu.ModbusUdp.MBFunctionCode.Write;
            }

            if (RecData[0] == 0xff && RecData[1] == 0x65)        //判断心跳应答
            {
                monitorTickRx = RecData[2];
            }

            if (RecData[0] == 0xff && RecData[1] == 0x6a)       //判断uuid应答
            {
                uuid = "";
                for (int i = 4; i < 28; i++)
                {
                    uuid += RecData[i].ToString("X2");
                }
                Debug.WriteLine("uuid:"+uuid);
                if (registerFlag == true)      //将初始uuid保存在文件当中
                {
                    //File.WriteAllText(@"C:\Users\shuqee\Desktop\shuqee.bin",uuid);
                    File.WriteAllText(Directory.GetCurrentDirectory() + @"\shuqee.bin", uuid);
                    registerFlag = false;
                }

                if (uuid == Module.uuidFile)      //判断uuid是否与初始的一致
                {
                    MessageBox.Show("uuid正确");
                    //Thread.Sleep(1000);
                    UdpSend.flagSend = (byte)Mcu.ModbusUdp.MBFunctionCode.ReadChip;
                }
                //else
                //{
                  //  MessageBox.Show("uuid不正确");
                //}
            }

            if (RecData[0] == 0xff && RecData[1] == 0x68)     //校验日期
            {
                //MessageBox.Show("校验日期");
                string reyear = "20" + RecData[6];
                if (RecData[7] > 31)
                {
                    RecData[7] = 1;
                }
                string remonth = RecData[7].ToString();
                if(RecData[8]>31)
                {
                    RecData[8] = 1;
                }
                string reday = RecData[8].ToString();
                string redate = reyear + "-" + remonth + "-" + reday;
                DateTime dateNow = Convert.ToDateTime(DateTime.Now.ToShortDateString());
                DateTime getDate;
                try
                {
                    getDate = Convert.ToDateTime(redate);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    getDate = Convert.ToDateTime(DateTime.Now.ToShortDateString());
                }

                TimeSpan ts = getDate - dateNow;
                int getday = ts.Days;

                switch (getday)
                {
                    case 9:
                        MessageBox.Show("提示：使用期限还有10天");
                        break;
                    case 5:
                        MessageBox.Show("提示：使用期限还有6天");
                        break;
                    case 2:
                        MessageBox.Show("提示：使用期限还有3天");
                        break;
                    case 1:
                        MessageBox.Show("提示：使用期限还有2天");
                        break;
                    case 0:
                        //MessageBox.Show("提示：使用期限还有1天");
                        break;
                    default:                        
                        break;
                }
            }  //校验日期结尾


            RecData = ModbusUdp.MBRsp(RecData);
            Debug.WriteLine(ModbusUdp.ByteToHexStr(RecData));
            if (RecData != null)
            {
                if (RecData[0] == 0 && RecData[1] == 0 && RecData[2] == 0x01 && RecData[3] == 0x41)
                {
                    flagValue = true;
                    //要发送数据格式
                    UdpSend.flagSend = (byte)Mcu.ModbusUdp.MBFunctionCode.GetId;
                }

                if (RecData[0] == 0 && RecData[1] == 0xff && RecData[2] == 0 && RecData[3] == 0x1)
                {
                    while (true)
                    {
                        byte[] Data = new byte[8];
                        Data[0] = 0xff;
                        Data[1] = 0x64;
                        Data[2] = 0x00;
                        Data[3] = 0x00;
                        Data[4] = 0x01;
                        Data[5] = 0xc8;
                        Data[6] = 0x65;
                        Data[7] = 0xda;

                        UdpSend.UdpSendData(Data, Data.Length, UdpInit.BroadcastRemotePoint);
                        Thread.Sleep(1000);
                    }
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

                //接收UDP数据报，引用参数RemotePoint获得源地址 
                try
                {
                    int rlen = UdpInit.mySocket.ReceiveFrom(data, ref UdpInit.RemotePoint);
                    ReceiveCallback tx = SetReceiveData;
                    tx(rlen, data);

                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    //data = Mcu.ModbusUdp.MBReqConnect();
                    UdpSend.UdpSendData(Mcu.ModbusUdp.MBReqConnect(), Mcu.ModbusUdp.MBReqConnect().Length, UdpInit.BroadcastRemotePoint);
                    Debug.WriteLine("Search server");
                }
            }
        }
    }
}
