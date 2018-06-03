using System;
using System.Collections.Generic;
using System.IO.Ports;
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
using System.Threading;
using System.Diagnostics;
using System.Collections;
using System.Windows.Threading;


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
        private delegate void MyDelegate();
        double time = 0;
        double t = 160;
        double l = 160;

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
                flag = true;
            }
            PortWrite("1");
        }
        private void close_Click(object sender, RoutedEventArgs e)
        {
            PortWrite("0");
            if (myPort != null && myPort.IsOpen)
            {
                myPort.Close();
                flag = false;
            }
        }
        private void send_Click(object sender, RoutedEventArgs e)
        {
            PortWrite("r");
            int r = (int)red.Value;
            int g = (int)green.Value;
            int y = (int)yellow.Value;
            int b = (int)blue.Value;
            int w = (int)white.Value;
            string sr = r.ToString();
            string sg = g.ToString();
            string sy = y.ToString();
            string sb = b.ToString();
            string sw = w.ToString();
            PortWrite(sr + " ");
            PortWrite(sg + " ");
            PortWrite(sy + " ");
            PortWrite(sb + " ");
            PortWrite(sw + " ");
        }
        private void logStart_Click(object sender, RoutedEventArgs e)
        {
            {
                Thread myThread = new Thread(StartThread);
                myThread.IsBackground = true;
                myThread.Start();
            }
        }
        private void logEnd_Click(object sender, RoutedEventArgs e)
        {
            flag = false;
            Thread.CurrentThread.Abort();
        }
        private void sendModBus_Click(object sender, RoutedEventArgs e)
        {
            if (flag)
            {
                try
                {
                    byte address = Convert.ToByte(addressNo.Text, 16);
                    byte regNumber = Convert.ToByte(registerNo.Text, 16);
                    byte regCount = Convert.ToByte(registerCount.Text, 16);
                    byte read = Convert.ToByte(readContent.Text, 16);
                    byte write = Convert.ToByte(writeContent.Text, 16);

                    MyModbus modbus = new MyModbus();
                    byte[] text = modbus.GetReadFrame(address, write, regNumber, regCount, 8);
                    myPort.Write(text, 0, 8);
                    sendMessage.Items.Add(BitConverter.ToString(text));
                }
                catch(Exception exception)
                {
                    MessageBox.Show(exception.ToString());
                }
            }
        }
        private void receiveModBus_Click(object sender, RoutedEventArgs e)
        {
            if(flag)
            {
                try
                {
                    returnMessage.Items.Add(myPort.ReadLine());
                }
                catch
                {
                    MessageBox.Show("Error!");
                }
            }
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

        private void StartThread()
        {
            while (flag)
            {
                Dispatcher.BeginInvoke(new MyDelegate(GetData));
                Thread.Sleep(100);
            }
        }

        private void GetData()
        {
            string data = myPort.ReadLine();
            string s1 = "", s2 = "";
            int i = 0;

            for(;i<data.Length;i++)
            {
                if(data[i]!=' ')
                {
                    s1 += data[i];
                }
                else
                {
                    i++;
                    break;
                }
            }

            for(;i<data.Length;i++)
            {
                s2 += data[i];
            }

            Line line1 = new Line();
            line1.Stroke = Brushes.Red;
            Line line2 = new Line();
            line2.Stroke = Brushes.Blue;
            line1.X1 = time; line1.Y1 = t;
            line2.X1 = time; line2.Y1 = l;
            temperature.Text = s1;
            t = double.Parse(s1);
            t = 160 - t / 1000 * 160;
            light.Text = s2;
            l = double.Parse(s2);
            l = 160 - l / 1000 * 160;
            time++;
            line1.X2 = time; line1.Y2 = t;
            line2.X2 = time; line2.Y2 = l;
            canvas.Children.Add(line1);
            canvas.Children.Add(line2);
            
        }

        private void ShowResult(byte[] resbuffer)
        {
            MyModbus modbus = new MyModbus();
            returnMessage.Items.Add(modbus.SetText(resbuffer));
        }

    }
}
