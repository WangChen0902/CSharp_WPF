using System;
using System.Collections.Generic;
using System.Data;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace CSharp_WPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        bool flag = false;
        SerialPort myPort;
        List<Port> portList = new List<Port>();
        List<Speed> speedList = new List<Speed>();

        public MainWindow()
        {
            InitializeComponent();
            InitCombo();
        }

        private void open_Click(object sender, RoutedEventArgs e)
        {
            if (myPort == null)
            {
                myPort = new SerialPort(selectPort.SelectedValue.ToString(), (int)selectSpeed.SelectedValue);
                myPort.Open();
            }
            PortWrite("1");
        }

        private void close_Click(object sender, RoutedEventArgs e)
        {
            PortWrite("0");
            if (myPort != null && myPort.IsOpen)
            {
                myPort.Close();
            }
        }

        private void send_Click(object sender, RoutedEventArgs e)
        {
            PortWrite("r");
            int m = (int)red.Value;
            string s = (m + 100).ToString();
            PortWrite(s);
        }

        private void PortWrite(string message)
        {
            if(myPort!= null & myPort.IsOpen)
            {
                myPort.Write(message);
            }
        }

        private void InitCombo()
        {
            portList.Add(new Port { Name = "请选择串口名称", Value = "0" });
            portList.Add(new Port { Name = "COM3", Value = "COM3" });
            selectPort.ItemsSource = portList;
            selectPort.DisplayMemberPath = "Name";
            selectPort.SelectedValuePath = "Value";
            selectPort.SelectedIndex = 0;

            speedList.Add(new Speed { Name = "请选择通讯速率", Value = 0 });
            speedList.Add(new Speed { Name = "9600", Value = 9600 });
            selectSpeed.ItemsSource = speedList;
            selectSpeed.DisplayMemberPath = "Name";
            selectSpeed.SelectedValuePath = "Value";
            selectSpeed.SelectedIndex = 0;
        }

        private void logStart_Click(object sender, RoutedEventArgs e)
        {
            flag = false;
            ReadData();
        }

        private void logEnd_Click(object sender, RoutedEventArgs e)
        {
            flag = true;
        }

        private void ReadData()
        {
            myPort.ReadTimeout = 90 * 1000;
            myPort.DataReceived += new SerialDataReceivedEventHandler(this.serialport_DataReceived);
            try
            {
                Thread.Sleep(100);
                
            } finally { }
        }
        //定义事件处理函数

        private void serialport_DataReceived(Object sender, SerialDataReceivedEventArgs e)
        {
            string s = "";
            try
            {
                Thread.Sleep(100);  //（毫秒）等待一定时间，确保数据的完整性 int len        
                s = myPort.ReadLine();
            } finally { }
            string[] ss = s.Split(' ');
            temperature.Text = ss[0];
            light.Text = ss[1];
        }
    }
}
