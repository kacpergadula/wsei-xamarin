using Android.Locations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AirMonitor.Moduls
{
    public class Installation
    {
        public string Id { get; set; }

        //public Location location { get; set; }

        public Address Address { get; set; }

        //  public double Elevation { get; set; }

        //  [JsonProperty(PropertyName = "airly")]

        //  public bool IsAirlyInstallation { get; set; }
    }
}
