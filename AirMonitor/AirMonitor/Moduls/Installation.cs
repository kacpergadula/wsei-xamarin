
namespace AirMonitor.Moduls
{
    public class Installation
    {
        public string Id { get; set; }

        public Locatio location { get; set; }

        public Address Address { get; set; }

        //  public double Elevation { get; set; }

        //  [JsonProperty(PropertyName = "airly")]

        //  public bool IsAirlyInstallation { get; set; }
    }
}
