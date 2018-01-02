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
            if(button.Content.ToString()=="上升" )
            {
                button.Content = "下降";
                module.SendBytesData(Module.com1,255,255,255,0,0);
               
            }
            else
            {
                button.Content = "上升";
                module.SendBytesData(Module.com1, 0, 0, 0, 0, 0);
               
            }
        }


    }
}
