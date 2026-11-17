using System;
using System.Collections.Generic;
using System.Text;

namespace testSerial
{
    public class DeviceResult
    {
        public string DeviceCode { get; set; } = "";
        public string Barcode { get; set; } = "";
        public double Temperature { get; set; }
        public bool IsPassed { get; set; }
    }
}
