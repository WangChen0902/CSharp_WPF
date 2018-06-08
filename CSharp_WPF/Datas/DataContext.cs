using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp_WPF.Datas
{
    class DataContext
    {
        public DataContext()
        {

        }
        public DataContext(List<Data> data)
        {
            DataList = data;
        }
        public List<Data> DataList { get; set; }
    }
}
