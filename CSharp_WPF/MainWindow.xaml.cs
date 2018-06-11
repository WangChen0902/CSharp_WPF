using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Threading;
using System.Windows.Threading;
using System.Web.Script.Serialization;
using System.Text;
using System.Windows.Forms;
using System.IO;

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
        List<Data> dataList = new List<Data>();
        string json = "";
        int id = 1;
        double time = 0;
        double t = 160;
        double tC = 0;
        double l = 160;
        double rV = 0;
        double gV = 0;
        double yV = 0;
        double bV = 0;
        double wV = 0;
        string sent = "";
        string recv = "";

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
        }
        private void close_Click(object sender, RoutedEventArgs e)
        {
            if (myPort != null && myPort.IsOpen)
            {
                myPort.Close();
                flag = false;
            }
        }

        private void send_Click(object sender, RoutedEventArgs e)
        {
            rV = red.Value;
            gV = green.Value;
            yV = yellow.Value;
            bV = blue.Value;
            wV = white.Value;
            int[] LED = new int[5];
            LED[0] = (int)rV;
            LED[1] = (int)gV;
            LED[2] = (int)yV;
            LED[3] = (int)bV;
            LED[4] = (int)wV;
            string[] sLED = new string[5];
            for (int i = 0; i < 5; i++)
            {
                if (LED[i] == 10)
                {
                    sLED[i] = "00" + LED[i].ToString();
                }
                else
                {
                    sLED[i] = "000" + LED[i].ToString();
                }
            }


            writeContent.Text = sLED[0] + sLED[1] + sLED[2] + sLED[3] + sLED[4];

            Color c = Color.FromScRgb(255, (float)1 / 5 * LED[0] + (float)1 / 10 * LED[1], (float)1 / 5 * LED[2] + (float)1 / 10 * LED[3], (float)1 / 5 * LED[4]);
            color.Fill = new SolidColorBrush(c);
        }

        private void drawStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (flag)
                {
                    PortWrite("s");
                    Thread drawThread = new Thread(StartDraw);
                    drawThread.Start();
                }
            }
            catch(Exception exception)
            {
                System.Windows.MessageBox.Show(exception.ToString());
            }
        }
        private void drawEnd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PortWrite("e");
                Thread.CurrentThread.Abort();
            }
            catch(Exception exception)
            {
                System.Windows.MessageBox.Show(exception.ToString());
            }
        }

        private void logStart_Click(object sender, RoutedEventArgs e)
        {
            if (flag)
            {
                Thread logThread = new Thread(StartSave);
                logThread.Start();
            }
            
        }
        private void logEnd_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder stringBuilder = new StringBuilder();
            JavaScriptSerializer js = new JavaScriptSerializer();
            string path = "";

            js.Serialize(dataList, stringBuilder);
            json = stringBuilder.ToString();
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.ShowDialog();
            if (fbd.SelectedPath != string.Empty)
            {
                path = fbd.SelectedPath + "\\info.json";
            }
            File.WriteAllText(path, json);
            try
            {
                Thread.CurrentThread.Abort();
            }
            catch (Exception exception)
            {
                System.Windows.MessageBox.Show(exception.ToString());
            }
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
                    List<byte> bWrite = new List<byte>();
                    List<string> sWrite = new List<string>();
                    for (int j=0; j<writeContent.Text.Length; j += 2)
                    {
                        sWrite.Add(writeContent.Text.Substring(j, 2));
                    }
                    for(int j=0; j<sWrite.Count; j++)
                    {
                        bWrite.Add(Convert.ToByte(sWrite[j], 16));
                    }
                    byte[] myWrite = bWrite.ToArray();
                    byte count = (byte)myWrite.Length;
                    byte[] myCount = new byte[1];
                    myCount[0] = count;

                    MyModbus modbus = new MyModbus();
                    byte[] text = modbus.GetReadFrame(address, (byte)0x10, regNumber, regCount, 8);
                    List<byte> bText = new List<byte>();
                    int i = 0;
                    for (int j=0;j<6;j++)
                    {
                        bText.Add(text[i]);
                        i++;
                        if (i == 6)
                        {
                            bText.Add(myCount[0]);
                            foreach (byte wb in myWrite)
                            {
                                bText.Add(wb);
                            }
                        }
                    }
                    byte[] tmpText = bText.ToArray();
                    ushort crc = MyModbus.CRC16(tmpText, 0, tmpText.Length - 3);
                    bText.Add(MyModbus.WORD_HI(crc));
                    bText.Add(MyModbus.WORD_LO(crc));
                    byte[] myText = bText.ToArray();
                    
                    myPort.Write(myText, 0, myText.Length);
                    myPort.Write(";");
                    sendMessage.Items.Add(BitConverter.ToString(myText));
                    //returnMessage.Items.Add(BitConverter.ToString(text));
                    byte[] getBytes = new byte[32];
                    myPort.Read(getBytes, 0, 32);
                    returnMessage.Items.Add(BitConverter.ToString(getBytes));
                    byte[] recvBytes = new byte[32];
                    myPort.Read(recvBytes, 0, 32);
                    returnMessage.Items.Add(BitConverter.ToString(recvBytes));
                    sent = writeContent.Text;
                }
                catch(Exception exception)
                {
                    System.Windows.MessageBox.Show(exception.ToString());
                }
            }
        }
        private void receiveModBus_Click(object sender, RoutedEventArgs e)
        {
            if(flag)
            {
                try
                {
                    byte address = Convert.ToByte(addressNo.Text, 16);
                    byte regNumber = Convert.ToByte(registerNo.Text, 16);
                    byte regCount = Convert.ToByte(registerCount.Text, 16);
                    /*List<byte> bRead = new List<byte>();
                    List<string> sRead = new List<string>();
                    for (int j = 0; j < readContent.Text.Length; j += 2)
                    {
                        sRead.Add(writeContent.Text.Substring(j, 2));
                    }
                    for (int j = 0; j < sRead.Count; j++)
                    {
                        bRead.Add(Convert.ToByte(sRead[j], 16));
                    }
                    byte[] myRead = bRead.ToArray();
                    byte count = (byte)myRead.Length;
                    byte[] myCount = new byte[1];
                    myCount[0] = count;
                    */
                    MyModbus modbus = new MyModbus();
                    byte[] text = modbus.GetReadFrame(address, (byte)0x03, regNumber, regCount, 8);
                    myPort.Write(text, 0, 8);
                    myPort.Write(";");
                    sendMessage.Items.Add(BitConverter.ToString(text));

                    /*List<byte> bText = new List<byte>();
                    int i = 0;
                    for (int j = 0; j < 2; j++)
                    {
                        bText.Add(text[i]);
                        i++;
                        if (i == 6)
                        {
                            bText.Add(myCount[0]);
                            foreach (byte rb in myRead)
                            {
                                bText.Add(rb);
                            }
                        }
                    }
                    byte[] tmpText = bText.ToArray();
                    ushort crc = MyModbus.CRC16(tmpText, 0, tmpText.Length - 3);
                    bText.Add(MyModbus.WORD_HI(crc));
                    bText.Add(MyModbus.WORD_LO(crc));
                    byte[] myText = bText.ToArray();*/

                    //returnMessage.Items.Add(BitConverter.ToString(myText));
                    byte[] getBytes = new byte[32];
                    myPort.Read(getBytes, 0, 32);
                    returnMessage.Items.Add(BitConverter.ToString(getBytes));
                    byte[] recvBytes = new byte[32];
                    myPort.Read(recvBytes, 0, 32);
                    returnMessage.Items.Add(BitConverter.ToString(recvBytes));
                    recv = readContent.Text;
                }
                catch(Exception exception)
                {
                    System.Windows.MessageBox.Show(exception.ToString());
                }
            }
        }

        private void restart_Click(object sender, RoutedEventArgs e)
        {
            dataList.Clear();
            id = 1;
            json = "";
            time = 0;
            t = 160;
            tC = 0;
            l = 160;
            rV = 0;
            gV = 0;
            yV = 0;
            bV = 0;
            wV = 0;
            sent = "";
            recv = "";
            canvas.Children.Clear();
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            Thread.CurrentThread.Abort();
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

        private void StartDraw()
        {
            while (flag)
            {
                try
                {
                    Dispatcher.BeginInvoke(new MyDelegate(GetData));
                    Thread.Sleep(100);
                }
                catch (Exception exception)
                {
                    System.Windows.MessageBox.Show(exception.ToString());
                }
            }
        }
        private void StartSave()
        {
            while (flag)
            {
                try
                {
                    Dispatcher.BeginInvoke(new MyDelegate(SaveData));
                    Thread.Sleep(500);
                }
                catch (Exception exception)
                {
                    System.Windows.MessageBox.Show(exception.ToString());
                }
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
            t = double.Parse(s1);
            tC = (1.0 / (1.0 / (25 + 273.15) + 1.0 / 3435.0 * (Math.Log(1024.0 / t) - 1.0)) - 273.15);
            temperature.Text = tC.ToString();
            t = 160 - t / 1000 * 160;
            l = double.Parse(s2);
            light.Text = s2;
            l = 160 - l / 1000 * 160;
            time++;
            line1.X2 = time; line1.Y2 = t;
            line2.X2 = time; line2.Y2 = l;
            canvas.Children.Add(line1);
            canvas.Children.Add(line2);
            
        }
        private void SaveData()
        {
            Data data = new Data(id, myPort.PortName, myPort.BaudRate, tC, l, rV, gV, yV, bV, wV, sent, recv);
            dataList.Add(data);
            id++;
        }
    }
}
