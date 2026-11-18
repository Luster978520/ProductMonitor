using System;
using System.Collections.Generic;
using System.Text;

namespace ProductMonitor.Models
{
    public class WorkShopModel
    {
        public string WorkShopName { get; set; }
        public int Totle { get; set; }
        public int WorkShopCount { get; set; }
        public int WaitCount { get; set; }
        public int WrongCount { get; set; }
        public int StopCount { get; set; }
       
    }
}
