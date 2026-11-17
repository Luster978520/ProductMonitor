using System;
using System.Collections.Generic;
using System.Text;

namespace ProductMonitor.Models
{
    public class AlarmRecord
    {
        public int SN { get; set; }
        public string Alarm { get; set; }
        public DateTime Time { get; set; }
        public string DuringTime { get; set; }
    }
}
