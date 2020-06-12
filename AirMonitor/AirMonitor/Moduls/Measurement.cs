using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AirMonitor.Moduls
{
    public class Measurement
    {
        public int CurrentDisplayValue { get; set; }
        public MeasurementItem Current { get; set; }
        public Installation Installation { get; set; }
    }
}