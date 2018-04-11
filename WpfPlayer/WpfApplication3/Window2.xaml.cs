using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Threading;
using System.Windows.Threading;

namespace WpfApplication3
{
    /// <summary>
    /// Window2.xaml 的交互逻辑
    /// </summary>
    public partial class Window2 : Window
    {
        Module module = new Module();
        DispatcherTimer timer = null;
        CheckBox[] checkEv;
        CheckBox[] checkChair;
        byte eEffect = 0;
        byte cEffect = 0;
        byte[] data;        
        byte[] array;          //data+addr+len 
        byte[] Data;        
        byte data1;                     //1号缸数据             
        byte data2;                     //2号缸数据
        byte data3;                     //3号缸数据
        public Window2()
        {
            InitializeComponent();
            checkInit();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(0.05);
            timer.Tick += new EventHandler(timer_tick);
            timer.Start();
        }

        private void timer_tick(object sender, EventArgs e)
        {
            data = new byte[10] { data1, data2, data3, 0, 0, 0, 0, 0, eEffect, cEffect };
            array = Mcu.ModbusUdp.ArrayAdd(0, (ushort)data.Length, data);
            Data = Mcu.ModbusUdp.MBReqWrite(array);
            UdpSend.UdpSendData(Data, Data.Length, UdpInit.RemotePoint);
        }

        private void checkInit()
        {
             checkChair = new CheckBox [8]{checkBox, checkBox1, checkBox2, checkBox3, checkBox4, checkBox5, checkBox6, checkBox7 };
             checkEv =new CheckBox[8]{checkBox_Copy, checkBox1_Copy, checkBox2_Copy, checkBox3_Copy, checkBox4_Copy, checkBox5_Copy,checkBox6_Copy, checkBox7_Copy};
        }
        private void button_Click(object sender, RoutedEventArgs e)
        {

            if (button.Content.ToString()=="上升" )
            {
                button.Content = "下降";
                if(checkBox8.IsChecked==true)
                {
                    data1 = 255;
                }
                if (checkBox9.IsChecked == true)
                {
                    data2 = 255;
                }
                if (checkBox10.IsChecked == true)
                {
                    data3 = 255;
                }
                     
            }
            else
            {
                if (checkBox8.IsChecked == true)
                {
                    data1 = 0;
                }
                if (checkBox9.IsChecked == true)
                {
                    data2 = 0;
                }
                if (checkBox10.IsChecked == true)
                {
                    data3 = 0;
                }
                button.Content = "上升";
                        
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
         
            
            byte[] aa = new byte[8];
            byte[] bb = new byte[8];

            for(int i=0;i<8;i++)                     //判断环境特效有无选中，若有就给该位置赋值
            {  
               if(checkEv[i].IsChecked==true)
                {
                    aa[i] = (byte)Math.Pow(2, i);
                }
            }

            for (int i = 0; i < 8; i++)            //判断座椅特效有无选中，若有就给该位置赋值
            {
                if (checkChair[i].IsChecked == true)
                {
                    bb [i] = (byte)Math.Pow(2, i);
                }
            }

            for (int i = 0; i < 8; i++)
            {
                eEffect += aa[i];
            }

            for (int i = 0; i < 8; i++)
            {
                cEffect += bb[i];
            }
           
            if (button1.Content.ToString() == "特效开")
            {
                button1.Content = "特效关";
               
            }
            else
            {
                button1.Content = "特效开";
               
            }
        }

        
    }
}
