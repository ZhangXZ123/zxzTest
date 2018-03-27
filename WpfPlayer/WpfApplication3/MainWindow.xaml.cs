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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Forms;
using System.IO;
using System.Windows.Interop;
using System.IO.Ports;
using System.Diagnostics;

namespace WpfApplication3
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        // public object FullScreenHelper { get; private set; }
       //MediaElement mediaElement = new MediaElement();
        
        public static string fileName="";
        Window1 win = new Window1();
        DispatcherTimer timer = null;
        DispatcherTimer timer1 = null;
        Module module = new Module();
        int count;     
        //private Mcu.McuTest myMcuTest = new Mcu.McuTest(); //for test class mcu
        UdpInit  myUdpInit = new UdpInit();
        
        public MainWindow()
        {        
            InitializeComponent();
            Module.readFile();            
            Module.readUuidFile();
            myUdpInit.udpInit();


            //udp程序启动定时器
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(0.05);   //定时器周期为50ms 
            timer.Tick += new EventHandler(timer_tick);
            timer.Start();

            //Module.readFile();
            InitListBox();            // disable for bug
            // Module.SerialInit();
            //System.Windows.Controls.Slider.AddHandler(Slider.MouseLeftButtonUp,new System.Windows.Forms.MouseEventHandler(slider_MouseLeftButtonUp),true);     
            //mediaElement.LoadedBehavior = MediaState.Manual;
            //(Content as Grid).Children.Add(mediaElement);
        }

        private void pause_Click(object sender, RoutedEventArgs e)
        {
            //if (FullScreenHelper.IsFullscreen(this))
            //    FullScreenHelper.ExitFullscreen(this);
            //else
            //    FullScreenHelper.GoFullscreen(this);
            // mediaElement.Pause(); 
            //win.Visibility = Visibility.Visible;
            
            win.pause();
            
        }

        public static List <string> list=new List<string >();

        /// <summary>
        /// 初始化列表，将当前目录的avi文件添加到列表当中
        /// </summary>
        private  void InitListBox()
        {
            //获取软件当前目录的avi文件
            string[] path = Directory.GetFiles( Directory.GetCurrentDirectory(), "*.avi");
            //string[] path = Directory.GetFiles(@"d:\电影", "*.avi");
            for (int i = 0; i < path.Length; i++)
            {          
                string videoName = System.IO.Path.GetFileName(path[i]);
                listBox.Items.Add(videoName);
                list.Add(path[i]);
            }

            path = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.mp4");
            //path = Directory.GetFiles(@"d:\电影", "*.mp4");
            for (int i = 0; i < path.Length; i++)
            {
                //listBox.Items.Add(path[i].Substring(path[i].LastIndexOf('\\') + 1));
                string videoName = System.IO.Path.GetFileName(path[i]);   //获取当前路径的文件名包含后缀
                //listBox.Items.Add(videoName.Substring(0,videoName.LastIndexOf('.')));
                listBox.Items.Add(videoName);
                list.Add(path[i]);
            }

        }


        /// <summary>
        /// 双击窗体全屏
        /// </summary>
        private void myContent_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (FullScreenHelper.IsFullscreen(this))
                FullScreenHelper.ExitFullscreen(this);
            else
                FullScreenHelper.GoFullscreen(this);
           
            win.Visibility = Visibility.Visible;

        }


        /// <summary>
        /// 播放按钮，点击播放影片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void play_Click(object sender, RoutedEventArgs e)
        {
            if (fileName != "")
            {
                //module.readFile();
                win.Visibility = Visibility.Visible;
                //win.Show();
              
                win.play();               
                //媒体文件打开成功
                timer1 = new DispatcherTimer();
                timer1.Interval = TimeSpan.FromSeconds(0.05);   //定时器周期为50ms 
                timer1.Tick += new EventHandler(timer1_tick);
                timer1.Start();
                        
            }
        }


        /// <summary>
        /// 打开文件按钮，点击打开文件框
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void open_Click(object sender, RoutedEventArgs e)
        {
            // string filepath = "";
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "选择文件";
            openFileDialog.Filter = "avi文件|*.avi|mp4文件|*.mp4|所有文件|*.*";
            openFileDialog.FileName = string.Empty;
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.DefaultExt = "avi";
            DialogResult result = openFileDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
            fileName = openFileDialog.FileName;
            //listBox.Items.Add(fileName);
            list.Add(fileName);
            //将影片名字显示在列表当中，不显示路径
            listBox.Items.Add(fileName.Substring(fileName.LastIndexOf('\\')+1));
         
        }
        //public delegate void GreetingDelegate(double time);



        /// <summary>
        /// 停止按钮
        /// </summary>
        private void stop_Click(object sender, RoutedEventArgs e)
        {
            //this.mediaElement.Stop();
            // mediaElement.Close();
            win.stop();
            try
            {
                timer.Stop();
            }
            catch
            {

            }
            //关闭定时器
            listBox.IsEnabled = true;
        }

        


        /// <summary>
        /// 定时器开启事件,显示时间码
        /// </summary>
        private void timer_tick(object sender, EventArgs e)
        { 
            //Slider.Value = Window1.sliderPositionValue;        
            //textBox.Text = Window1.sliderPositionValue.ToString();
            //Slider.Maximum = Window1.sliderMaximum;           
            //textBox1.Text = Window1.currenTime+"/"+ Window1.totalTime;            
            //module.FlimValue(Window1.sliderPositionValue);

            label1.Content = UdpConnect.strTimeCode;

            //textBox1.Text = UdpConnect.strTimeCode;
        }

        /// <summary>
        /// 显示播放进度，发送动作数据
        /// </summary>
        private void timer1_tick(object sender, EventArgs e)
        {
            Slider.Value = Window1.sliderPositionValue;        
            //textBox.Text = Window1.sliderPositionValue.ToString();
            Slider.Maximum = Window1.sliderMaximum;           
            textBox1.Text = Window1.currenTime+"/"+ Window1.totalTime;            
            //module.FlimValue(Window1.sliderPositionValue);
            UdpSend.SendWrite(Window1.sliderPositionValue);
            count++;
            Debug.WriteLine("动作帧数"+count);
        }


        /// <summary>
        /// 将总秒数转换成时间格式00:00:00
        /// </summary>
        /// <param name="timer">传入的秒数</param>
        /*
        private void timerChange(int totalTime)
        {
            int hour = totalTime / 3600;
            int second = totalTime % 3600;
            int mintue = second / 60;
            second = second % 60;
            textBox1.Text = Window1.currenTime + "/" + hour.ToString() + ":" + mintue.ToString() + ":" + second.ToString();
        }
        */

        /// <summary>
        /// 点击listbox列表，点击每一项都触发此方法
        /// </summary>
        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {            
            System.Windows.MessageBox.Show(list[listBox.SelectedIndex]);            
            //fileName= @"D:\电影\"+listBox.SelectedItem.ToString();          
            fileName = list[listBox.SelectedIndex];             
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //mediaElement.Width = ActualWidth;
            //mediaElement.Height = ActualHeight;
        }

        /// <summary>
        /// 快进
        /// </summary>
        private void fastForward_Click(object sender, RoutedEventArgs e)
        {
            win.FastForward();
        }


        /// <summary>
        /// 快退
        /// </summary>
        private void back_Click(object sender, RoutedEventArgs e)
        {
            win.Back();
        }
        

        /// <summary>
        /// 拖动进度条对播放状态进行控制
        /// </summary>
        private void Slider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            //System.Windows.MessageBox.Show(Slider.Value.ToString());
            int forwardPosition = (int)Slider.Value;
            win.sliderChanged(forwardPosition);            
            //win.FastForward();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            //module.com1.Close();
            //System.Windows.MessageBox.Show(MainWindow.fileName.Substring(0,MainWindow.fileName.LastIndexOf(".")));
            Window2 win2 = new Window2();
            win2.ShowDialog();            
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            byte[] Data=new byte[9];
            Data[0] = 0xff;
            Data[1] = 0x67;
            Data[2] = 0x00;
            Data[3] = 0xff;
            Data[4] = 0x00;
            Data[5] = 0x01;
            Data[6] = 0x01;
            Data[7] = 0xec;
            Data[8] = 0x5c;

            UdpSend.UdpSendData(Data, Data.Length, UdpInit.RemotePoint);
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {

            RegisterWin win3 = new RegisterWin();
            win3.Show();
        }


        /// <summary>
        /// 主窗体关闭后，关闭所有进程
        /// </summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}
