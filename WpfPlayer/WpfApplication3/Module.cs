using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApplication3
{
    public class Module
    {
        public int returnData;                         //定义全局变量returnData
                                                       //public MainWindow mainwindow = new MainWindow();
        public static bool changeWindow;               //定义静态变量
        public static byte[] chipID = new byte[12];    //用于存储shuqee.bin文件的数据
        public static byte YY;
        public static byte MM;
        public static byte DD;
        public static byte HH;
        public static byte mm;
        public static byte deadlineYY;                 //注册后的期限年
        public static byte deadlineMM;                 //注册后的期限月
        public static byte deadlineDD;                 //注册后的期限日
        public static byte deadlineOrPermanent;        //注册码是否为永久码 
        public static SerialPort com1 = new SerialPort();
        public static byte[] actionFile;
        public static byte[] effectFile;
        public static string uuidFile;

        
        /// <summary>
        /// udp发送动作数据与特效数据函数
        /// </summary>
        /// <param name="data1">1号缸数据</param>
        /// <param name="data2">2号缸数据</param>
        /// <param name="data3">3号缸数据</param>
        /// <param name="data4">环境特效数据</param>
        /// <param name="data5">座椅特效数据</param>
        public static void sendData(byte data1, byte data2, byte data3, byte data4, byte data5)
        {

            byte[] data_buf = new byte[16];
            int data_len = 0;
            UdpClient myUdpClient = new UdpClient();
            IPAddress remoteIP;

            IPAddress.TryParse("192.168.1.109", out remoteIP);

            UInt16 port = 1032;

            IPEndPoint iep = new IPEndPoint(remoteIP, port);
            try
            {

                data_buf[0] = 0xff;
                data_buf[1] = 0x4a;
                data_buf[2] = data1;
                data_buf[3] = data2;
                data_buf[4] = data3;
                data_buf[5] = data4;
                data_buf[6] = data5;
                data_buf[7] = 0x01;
                data_buf[8] = 0xee;
                data_len = 9;
                myUdpClient.Send(data_buf, data_len, iep);
                myUdpClient.Close();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, "发送失败");
            }
        }

        public static void GetNowTime()
        {
            System.DateTime currentDateTime = new System.DateTime();
            currentDateTime = System.DateTime.Now;                      //获取当前时间年月日时分秒
            //int years = currentTime.Year;                             //获取当前年
            //int months = currentTime.Month;                           //获取当前月
            //int days = currentTime.Day;                               //获取当前日

            string strYear = currentDateTime.ToString("yy");             //获取当前年的后两位

            YY = Convert.ToByte(strYear);               //将字符串strYear转换成byte型             
            MM = (byte)currentDateTime.Month;           //将int型当前月转换成byte型
            DD = (byte)currentDateTime.Day;             //将int型当前日转换成byte型
            HH = (byte)currentDateTime.Hour;            //将int型当前时转换成byte型 
            mm = (byte)currentDateTime.Minute;          //将int型当前分转换成byte型

        }


        /// <summary>
        /// 读取校验文件
        /// </summary>
        public static void readUuidFile()
        {
            try
            {
                //读取校验文件“shuqee.bin”
                //byte[] uuidFile = File.ReadAllBytes(@"C: \Users\shuqee\Desktop\shuqee.bin");
                //uuidFile = File.ReadAllText(@"C: \Users\shuqee\Desktop\shuqee.bin");
                uuidFile=File.ReadAllText(Directory.GetCurrentDirectory() + @"\shuqee.bin");
                Debug.WriteLine(uuidFile+"读文件");
            }
            catch
            {
                MessageBox.Show("校验文件不存在，请把校验文件放在软件当前目录");
            }
            
         }



        /// <summary>
        /// 获取芯片id
        /// </summary>
        public static void GetChipId()
        {
            //System.DateTime currentTime = new System.DateTime();
            //currentTime = System.DateTime.Now;    //获取当前时间年月日时分秒
            //string strYear = currentTime.ToString("yy");             //获取当前年的后两位
            //YY = Convert.ToByte(strYear);    //将字符串strYear转换成byte型             
            //MM = (byte)currentTime.Month;    //将int型当前月转换成byte型
            //DD = (byte)currentTime.Day;     //将int型当前日转换成byte型

            GetNowTime();

            byte[] data_buf = new byte[16];
            int data_len = 0;
            UdpClient myUdpClient = new UdpClient();
            IPAddress remoteIP;

            IPAddress.TryParse("192.168.1.109", out remoteIP);

            UInt16 port = 1032;

            IPEndPoint iep = new IPEndPoint(remoteIP, port);
            deadlineYY += YY;
            deadlineMM += MM;
            deadlineDD += DD;

            if (deadlineDD > 30)
            {
                deadlineMM += 1;
                deadlineDD -= 30;
            }
            if (deadlineMM > 12)
            {
                deadlineMM -= 12;
                deadlineYY += 1;
            }

            try
            {
                data_buf[0] = 0xFF;
                data_buf[1] = 0x58;
                data_buf[2] = 0x12;
                data_buf[3] = 0x34;
                data_buf[4] = deadlineYY;
                data_buf[5] = deadlineMM;
                data_buf[6] = deadlineDD;
                data_buf[7] = 0x00;
                data_buf[8] = 0x00;
                data_buf[9] = deadlineOrPermanent;
                data_buf[10] = YY;
                data_buf[11] = MM;
                data_buf[12] = DD;
                data_buf[13] = 0xEE;

                data_len = 14;
                myUdpClient.Send(data_buf, data_len, iep);
                myUdpClient.Close();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, "发送失败");
            }
        }

        /// <summary>
        /// 发送校验代码函数，用于发送指令给串口板，校验信息
        /// </summary>
        public static void sendCheckData()
        {
            byte[] data_buf = new byte[16];
            int data_len = 0;
            UdpClient myUdpClient = new UdpClient();
            IPAddress remoteIP;
            IPAddress.TryParse("192.168.1.109", out remoteIP);
            UInt16 port = 1032;
            IPEndPoint iep = new IPEndPoint(remoteIP, port);
            try
            {
                //读取校验文件“shuqee.bin”
                byte[] checkOutFile = File.ReadAllBytes(@"C: \Users\shuqee\Desktop\shuqee.bin");
                if (checkOutFile[7] != 0x88)
                {
                    checkOutFile[0] ^= 0x33;
                    checkOutFile[1] ^= 0x44;
                    checkOutFile[2] ^= 0x55;
                    checkOutFile[3] ^= 0x66;
                    checkOutFile[4] ^= 0x77;
                    checkOutFile[5] ^= 0x88;
                    checkOutFile[6] ^= 0x99;
                    checkOutFile[7] = 0x88;
                    File.WriteAllBytes(@"C: \Users\shuqee\Desktop\shuqee.bin", checkOutFile);
                }
                checkOutFile[0] ^= 51;
                checkOutFile[1] ^= 68;
                checkOutFile[2] ^= 85;
                checkOutFile[3] ^= 102;
                checkOutFile[4] ^= 119;
                checkOutFile[5] ^= 136;
                checkOutFile[6] ^= 153;

                GetNowTime();
                //System.DateTime currentDateTime = new System.DateTime();
                //currentDateTime = System.DateTime.Now;    //获取当前时间年月日时分秒
                ////int years = currentTime.Year;         //获取当前年
                ////int months = currentTime.Month;       //获取当前月
                ////int days = currentTime.Day;           //获取当前日

                //string strYear = currentDateTime.ToString("yy");             //获取当前年的后两位

                //YY = Convert.ToByte(strYear);               //将字符串strYear转换成byte型             
                //MM = (byte)currentDateTime.Month;           //将int型当前月转换成byte型
                //DD = (byte)currentDateTime.Day;             //将int型当前日转换成byte型
                //byte HH = (byte)currentDateTime.Hour;       //将int型当前时转换成byte型 
                //byte mm = (byte)currentDateTime.Minute;     //将int型当前分转换成byte型

                checkOutFile[0] ^= 52;
                checkOutFile[1] ^= 117;
                checkOutFile[2] ^= 106;
                checkOutFile[3] ^= 123;
                checkOutFile[4] ^= 74;
                checkOutFile[5] ^= 124;
                checkOutFile[6] ^= 143;

                data_buf[0] = 0xff;
                data_buf[1] = 0x47;
                data_buf[2] = YY;
                data_buf[3] = MM;
                data_buf[4] = DD;
                data_buf[5] = HH;
                data_buf[6] = mm;
                data_buf[7] = checkOutFile[0];
                data_buf[8] = checkOutFile[1];
                data_buf[9] = checkOutFile[2];
                data_buf[10] = checkOutFile[3];
                data_buf[11] = checkOutFile[4];
                data_buf[12] = checkOutFile[5];
                data_buf[13] = checkOutFile[6];
                data_buf[14] = 0xee;

                data_len = 15;
                myUdpClient.Send(data_buf, data_len, iep);
                myUdpClient.Close();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, "发送失败");
            }
        }


        /// <summary>
        /// 串口发送动作数据与特效数据函数
        /// </summary>
        /// <param name="data1">1号缸数据</param>
        /// <param name="data2">2号缸数据</param>
        /// <param name="data3">3号缸数据</param>
        /// <param name="data4">环境特效数据</param>
        /// <param name="data5">座椅特效数据</param>
        public void SendBytesData(SerialPort serialPort, byte data1, byte data2, byte data3, byte data4, byte data5)
        {
            //byte[] bytesSend = System.Text.ASCIIEncoding.Default.GetBytes(textBox.Text );
            try
            {
                byte[] bytesSend = new byte[9];
                bytesSend[0] = 0xFF;
                bytesSend[1] = 0x4a;
                bytesSend[2] = data1;
                bytesSend[3] = data2;
                bytesSend[4] = data3;
                bytesSend[5] = data4;
                bytesSend[6] = data5;
                bytesSend[7] = 0x01;
                bytesSend[8] = 0xEE;
                //serialPort.Write(bytesSend,0,bytesSend.Length);          
                serialPort.Write(bytesSend, 0, 9);
            }
            catch
            {
                MessageBox.Show("发送失败");

            }
        }


        /// <summary>
        /// 获取当前帧的具体动作数据与特效数据
        /// </summary>
        /// <param name="pos">传入的影片当前时刻</param>
        public void FlimValue(double pos)
        {
            pos = pos * 1000;
            byte last1 = 0;
            byte last2 = 0;
            byte last3 = 0;
            byte lastt1 = 0;
            byte lastt2 = 0;
            try
            {
                last1 = actionFile[(int)(3 * (pos / 50))];
                last2 = actionFile[(int)(3 * (pos / 50) + 1)];
                last3 = actionFile[(int)(3 * (pos / 50) + 2)];
                lastt1 = effectFile[(int)(2 * (pos / 50))];
                lastt2 = effectFile[(int)(2 * (pos / 50) + 1)];
            }
            catch
            {
                last1 = 0;
                last2 = 0;
                last3 = 0;
                lastt1 = 0;
                lastt2 = 0;
            }
            SendBytesData(com1, last1, last2, last3, lastt1, lastt2);
            //sendData(last1,last2,last3,lastt1,lastt2);
        }


        /// <summary>
        /// 串口初始化
        /// </summary>
        public static void SerialInit()
        {

            com1.PortName = "COM1";
            com1.BaudRate = 9600;
            com1.Parity = Parity.None;
            com1.DataBits = 8;
            com1.StopBits = StopBits.One;
            try
            {
                if (!com1.IsOpen)
                {
                    com1.Open();
                }
                else
                {
                    MessageBox.Show("port is open!");
                }
            }
            catch
            {
                MessageBox.Show("open erro");
            }

        }


        /// <summary>
        /// 读取动作文件跟特效文件
        /// </summary>
        public static void readFile()
        {
            try
            {
                //actionFile = File.ReadAllBytes(@"C: \Users\shuqee\Desktop\A-D");                
                actionFile = File.ReadAllBytes(Directory.GetCurrentDirectory() + @"\A-D");
               // actionFile = File.ReadAllBytes(MainWindow.fileName.Substring(0, MainWindow.fileName.LastIndexOf(".")) + "-D");
                try
                {
                    // effectFile = File.ReadAllBytes(@"C: \Users\shuqee\Desktop\A-T");
                    effectFile = File.ReadAllBytes(Directory.GetCurrentDirectory() + @"\A-T");
                    //effectFile = File.ReadAllBytes(MainWindow.fileName.Substring(0, MainWindow.fileName.LastIndexOf(".")) + "-T");
                }
                catch
                {
                    MessageBox.Show("特效文件不存在，请把A-T文件复制到当前目录");
                    effectFile = null;
                }
            }
            catch
            {
                MessageBox.Show("动作文件不存在，请把A-D文件复制到当前目录");
                actionFile = null;
            }
           // MessageBox.Show("读取完毕");
        }


    }
}
