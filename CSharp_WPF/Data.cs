using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp_WPF
{
    class Data
    {
        public Data()
        {

        }
        public Data(int id, string port, int rate, double t, double l, double r, double g, double y, double b, double w, string send, string recv)
        {
            ID = id;
            Port = port;
            Rate = rate;
            Temperature = t;
            Light = l;
            Red = r;
            Green = g;
            Yellow = y;
            Blue = b;
            White = w;
            Send = send;
            Recv = recv;
        }
        public int ID { get; set; }
        public string Port { get; set; }
        public int Rate { get; set; }
        public double Temperature { get; set; }
        public double Light { get; set; }
        public double Red { get; set; }
        public double Green { get; set; }
        public double Yellow { get; set; }
        public double Blue { get; set; }
        public double White { get; set; }
        public string Send { get; set; }
        public string Recv { get; set; }
    }
}
