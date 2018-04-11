using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Security.Cryptography;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Threading;
using System.IO;

namespace WpfApplication3
{
    /// <summary>
    /// Register.xaml 的交互逻辑
    /// </summary>
    public partial class RegisterWin : Window
    {
        public static string userCode;                  //用户码
        public static string registerCode;              //注册码
        public static string random;                    //随机数

        public RegisterWin()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 点击获取用户码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_Click(object sender, RoutedEventArgs e)
        {
            UdpSend.flagSend = (byte)Mcu.ModbusUdp.MBFunctionCode.GetId;
            UdpConnect.registerFlag = true;
            change(textUserCode);
        }

        /// <summary>
        /// 将用户码与随机数显示在text框上
        /// </summary>
        /// <param name="text1">显示在哪个文本框</param>
        public static void change(TextBox text1)
        {
            Random rd = new Random();
            random = rd.Next(255).ToString();
            userCode= random + UdpConnect.uuid;
            text1.Text = random + UdpConnect.uuid;
        }

        /// <summary>
        /// md5 32位加密
        /// </summary>
        /// <param name="传入的参数值password"></param>
        /// <returns>返回经过md5加密的字符串</returns>
        public static string MD5Encrypt32(string password)
        {
            string cl = password;
            string pwd = "";
            MD5 md5 = MD5.Create(); //实例化一个md5对像
                                    // 加密后是一个字节类型的数组，这里要注意编码UTF8/Unicode等的选择　
            try
            {
                byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(cl));
                // 通过使用循环，将字节类型的数组转换为字符串，此字符串是常规字符格式化所得
                for (int i = 0; i < s.Length; i++)
                {
                    // 将得到的字符串使用十六进制类型格式。格式后的字符是小写的字母，如果使用大写（X）则格式后的字符是大写字符 
                    pwd = pwd + (s[i].ToString("X").Length < 2 ? "0" + s[i].ToString("X") : s[i].ToString("X"));
                    //pwd = pwd + s[i].ToString("x");
                }

            }
            catch
            {
                MessageBox.Show("请点击获取用户码");
            }
            return pwd;
        }


        private void textUserCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            MessageBox.Show("进入md5加密");
            registerCode = MD5Encrypt32(MD5Encrypt32(MD5Encrypt32(userCode)));

        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string changeRegisterCode = textRegisterCode.Text;        //注册码文本框的字符串包含期限信息

                string str1 = changeRegisterCode.Substring(0, 4);
                string strThousands = changeRegisterCode.Substring(4, 1);

                string str2 = changeRegisterCode.Substring(5, 6);
                string strHundreds = changeRegisterCode.Substring(11, 1);

                string str3 = changeRegisterCode.Substring(12, 8);
                string strTens = changeRegisterCode.Substring(20, 1);

                string str4 = changeRegisterCode.Substring(21, 10);
                string strUnits = changeRegisterCode.Substring(31, 1);

                string strAdd = changeRegisterCode.Substring(32, 2);
                string str5 = changeRegisterCode.Substring(34, 4);

                //得到最终的注册码
                string SHUQEE = str1 + str2 + str3 + str4 + str5;
                //注册码的期限信息
                int dateAdd = Int32.Parse(strThousands) + Int32.Parse(strHundreds) + Int32.Parse(strTens) + Int32.Parse(strUnits);

                if (dateAdd != 36) //转换成实际的期限
                {
                    Module.deadlineOrPermanent = 0x4;
                    int totalDays = 1000 * (Int32.Parse(strThousands)) + 100 * (Int32.Parse(strHundreds)) + 10 * (Int32.Parse(strTens)) + Int32.Parse(strUnits);
                    Module.deadlineYY = (byte)(totalDays / 365);
                    int remainDays = totalDays % 365;
                    Module.deadlineMM = (byte)(remainDays / 30);
                    Module.deadlineDD = (byte)(remainDays % 30);
                }
                else     //永久码
                {
                    Module.deadlineOrPermanent = 0x9;
                }


                if (SHUQEE == registerCode && dateAdd == Int32.Parse(strAdd))
                {
                    MessageBox.Show("注册成功");
                    Module.GetChipId();
                    this.Hide();
                    MainWindow mainwindow = new MainWindow();
                    mainwindow.Show();
                    Module.chipID[7] = 0x88;
                    Module.chipID[8] = 0x1;
                    Module.chipID[9] = Module.YY;
                    Module.chipID[10] = Module.MM;
                    Module.chipID[11] = Module.DD;

                    Module.YY ^= 0x5a;
                    Module.MM ^= 0x7c;
                    Module.DD ^= 0xB8;

                    Module.chipID[0] ^= 0x33;       //芯片id1   
                    Module.chipID[1] ^= 0x44;       //芯片id2
                    Module.chipID[2] ^= 0x55;       //芯片id3
                    Module.chipID[3] ^= 0x66;       //芯片id4
                    Module.chipID[4] ^= 0x77;       //芯片id5
                    Module.chipID[5] ^= 0x88;       //芯片id6 
                    Module.chipID[6] ^= 0x99;       //芯片id7

                    Module.chipID[0] ^= 0x34;
                    Module.chipID[1] ^= 0x75;
                    Module.chipID[2] ^= 0x6a;
                    Module.chipID[3] ^= 0x7b;
                    Module.chipID[4] ^= 0x4a;
                    Module.chipID[5] ^= 0x7c;
                    Module.chipID[6] ^= 0x8f;

                    File.WriteAllBytes(@"C: \Users\shuqee\Desktop\shuqee.bin", Module.chipID);

                }
                else
                {
                    MessageBox.Show("注册失败");
                }
            }
            catch
            {
                MessageBox.Show("输入的注册码长度不正确，请检查");
            }

            //textRegisterCode.Text = RegisterCode;
            //myClass.sendCheckData();
        }

    }
}
