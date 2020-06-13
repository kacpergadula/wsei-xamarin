using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms.Maps;

namespace AirMonitor.Moduls
{
    public class MapLocation
    {
        public string Address { get; set; }
        public string Description { get; set; }
        public Position Position { get; set; }
    }
}
