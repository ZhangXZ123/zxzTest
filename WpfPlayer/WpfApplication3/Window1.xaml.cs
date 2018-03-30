using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WpfApplication3
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class Window1 : Window
    {
        MediaElement mediaElement = new MediaElement();
        //MainWindow mainwindow = new MainWindow();
        DispatcherTimer timer = null;
        public static double sliderPositionValue;          //进度条的当前位置                
        public static double sliderMaximum;                //进度条的最大值
        public static int playstate;                       //影片的播放状态  playstate=0没播放，playstate=1正在播放中，playstate=2暂停
        public static string currenTime;
        public static string totalTime;

        public Window1()
        {
            InitializeComponent();

            mediaElement.LoadedBehavior = MediaState.Manual;
            mediaElement.MediaOpened += mediaElement_MediaOpened;     //注册MediaOpened事件
            mediaElement.MediaEnded += mediaElement_MediaEnded;       //注册MediaEnded事件
            (Content as Grid).Children.Add(mediaElement);
            this.WindowStyle = WindowStyle.None;
            this.WindowState = WindowState.Maximized;
        }

        public void stop()
        {
            this.mediaElement.Stop();

            mediaElement.Close();
            playstate = 0;
            timer.Stop();
            //this.Close();
            //listBox.IsEnabled = true;
        }

        public void play()
        {
            if (MainWindow.fileName != "" && playstate == 0)
            {
                Module.readFile();
                mediaElement.Source = new Uri(MainWindow.fileName, UriKind.Relative);

                mediaElement.Play();

                if (FullScreenHelper.IsFullscreen(this))
                    FullScreenHelper.ExitFullscreen(this);
                else
                    FullScreenHelper.GoFullscreen(this);
                mediaElement.Width = ActualWidth;
                mediaElement.Height = ActualWidth;
                playstate = 1;
                return;

            }
            if (playstate == 2)
            {
                mediaElement.Play();
                playstate = 1;
            }
        }


        public void pause()
        {
            mediaElement.Pause();
            playstate = 2;

        }

        public void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            mediaElement.Width = ActualWidth;
            mediaElement.Height = ActualHeight;
        }


        /// <summary>
        /// 影片结束后事件，可用来执行影片播放结束想要执行的动作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            this.mediaElement.Stop();
            mediaElement.Close();
            MessageBox.Show("影片播放结束");
            playstate = 0;
            timer.Stop();
            //单曲循环
            //mediaElement.Position = new TimeSpan();
            //mediaElement.Play();

            //列表循环
            //for (int i = 0; i < MainWindow.list.Count; i++)
            //{
            //    if (MainWindow.fileName == MainWindow.list[i])
            //    {
            //        if (i == MainWindow.list.Count - 1)
            //        {
            //            this.mediaElement.Source = new Uri(MainWindow.list[0]);                        
            //            this.mediaElement.Play();
            //        }
            //        else
            //        {
            //            this.mediaElement.Source = new Uri(MainWindow.list[i + 1]);
            //            this.mediaElement.Play();
            //        }
            //        break;
            //    }
            //}
        }

        public void sliderChanged(int sliderValue)
        {
            //mediaElement.Position = TimeSpan.FromSeconds(sliderValue);
            //mediaElement.Position = new TimeSpan(0,0,0,0,sliderValue);
            //mediaElement.Play();

            mediaElement.Position = TimeSpan.FromSeconds(sliderValue);
            //MessageBox.Show(sliderValue.ToString());

        }



        /// <summary>
        /// 影片播放后事件，play动作已经执行完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void mediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {            
            sliderMaximum = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
            totalTime = mediaElement.NaturalDuration.ToString().Substring(0, 8);
            //媒体文件打开成功
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(0.05);
            timer.Tick += new EventHandler(timer_tick);
            timer.Start();

        }

        public void timer_tick(object sender, EventArgs e)
        {
            //获取影片当前状态总秒数
            sliderPositionValue = mediaElement.Position.TotalSeconds;
            UdpSend.SendWrite(sliderPositionValue);

            //UdpSend.QuDong(sliderPositionValue);
            //sliderPositionValue = mediaElement.Position.
            //获取影片当前状态小时
            //string hour = mediaElement.Position.Hours.ToString();
            //获取影片当前状态分钟
            //string minute = mediaElement.Position.Minutes.ToString();
            //获取影片当前状态秒
            //string second = mediaElement.Position.Seconds.ToString();
            //totalTime = hour + ":" + minute + ":" + second;
            //获取影片当前状态时间，格式为00:00:00
            //totalTime = mediaElement.Position.ToString();
            currenTime = mediaElement.Position.ToString().Substring(0, 8);
        }

        /// <summary>
        /// 快进
        /// </summary>
        public void FastForward()
        {
            mediaElement.Position = mediaElement.Position + TimeSpan.FromSeconds(10);
        }


        /// <summary>
        /// 快退
        /// </summary>
        public void Back()
        {
            mediaElement.Position = mediaElement.Position - TimeSpan.FromSeconds(10);
        }

        /// <summary>
        /// 双击窗体，退出全屏
        /// </summary>
        private void myContent_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (FullScreenHelper.IsFullscreen(this))
                FullScreenHelper.ExitFullscreen(this);
            else
                FullScreenHelper.GoFullscreen(this);
            //this.Visibility = Visibility.Hidden;
            this.Hide();
        }
    }
}
