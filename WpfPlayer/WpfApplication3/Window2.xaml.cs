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

namespace WpfApplication3
{
    /// <summary>
    /// Window2.xaml 的交互逻辑
    /// </summary>
    public partial class Window2 : Window
    {
        Module module = new Module();

        public Window2()
        {
            InitializeComponent();
           //Module.SerialInit();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            byte[] data;
            ushort addr;
            ushort len;
            byte[] array;          //data+addr+len 
            byte[] Data;           //最终发送的数据         
            addr = 0;
            len = 10;
            //data = new byte[len];
            
            if (button.Content.ToString()=="上升" )
            {
                button.Content = "下降";
                data =new byte[10]{ 255, 255, 255,0,0,0,0,0,0,0};
                array = Mcu.ModbusUdp.ArrayAdd(addr, len, data);
                Data = Mcu.ModbusUdp.MBReqWrite(array);
                UdpSend.UdpSendData(Data, Data.Length, UdpInit.RemotePoint);
               // module.SendBytesData(Module.com1,255,255,255,0,0);
               
            }
            else
            {
                button.Content = "上升";
                data = new byte[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                array = Mcu.ModbusUdp.ArrayAdd(addr, len, data);
                Data = Mcu.ModbusUdp.MBReqWrite(array);
                UdpSend.UdpSendData(Data, Data.Length, UdpInit.RemotePoint);
               // module.SendBytesData(Module.com1, 0, 0, 0, 0, 0);
               
            }
        }


    }
}
