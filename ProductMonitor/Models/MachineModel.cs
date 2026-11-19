using System;
using System.Collections.Generic;
using System.Text;

namespace ProductMonitor.Models
{
    public class MachineModel
    {
        public string MachineName { get; set; }
        public string MachineStatus { get; set; }
        public int PlanCount { get; set; }
        public int FinshedCount { get; set; }
        public string OrderNo { get; set; }
        public double Percent { get { return FinshedCount * 100.0 / PlanCount; }
            set;
        }
    }
}
