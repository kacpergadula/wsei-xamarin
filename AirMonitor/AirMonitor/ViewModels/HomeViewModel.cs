using System;
using System.Windows.Input;
using AirMonitor.Views;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using System.Web;
using System.Globalization;
using Newtonsoft.Json;
using System.Net;
using System.Linq;
using AirMonitor.Moduls;
using Xamarin.Forms.Maps;

namespace AirMonitor.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        private readonly INavigation _navigation;

        public HomeViewModel(INavigation navigation)
        {
            _navigation = navigation;
            Initialize();
        }

        private List<MapLocation> _locations;
        public List<MapLocation> Locations
        {
            get => _locations;
            set => SetProperty(ref _locations, value);
        }

        private async Task Initialize()
        {
            var location = await Geolocation.GetLastKnownLocationAsync();
            //var location = new Location(50, 20);
            var installations = await GetInstallations(location, maxResults: 4);
            var data = await GetMeasurementsForInstallations(installations);
            Items = new List<Measurement>(data);
            Locations = Items.Select(x => new MapLocation
            {
                Address = x.Installation.Address.Description,
                Description = "CAQI: " + x.CurrentDisplayValue,
                Position = new Position(x.Installation.location.Latitude, x.Installation.location.Longitude)
            }).ToList();
        }

        private List<Measurement> _items;
        public List<Measurement> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }



        private async Task<IEnumerable<Installation>> GetInstallations(Location location, double maxDistanceInKm = 3, int maxResults = -1)
        {
            if (location == null)
            {
                System.Diagnostics.Debug.WriteLine("No location data.");
                return null;
            }

            var query = GetQuery(new Dictionary<string, object>{
                { "lat" , location.Latitude },
                { "lng", location.Longitude },
                { "maxDistanceKM" , maxDistanceInKm},
                { "maxResults" , maxResults}
            });

            var url = GetAirlyApiUrl(App.AirlyApiInstallationUrl, query);

            var response = await GetHttpResponseAsync<IEnumerable<Installation>>(url);

            return response;
        }


        private async Task<IEnumerable<Measurement>> GetMeasurementsForInstallations(IEnumerable<Installation> installations)
        {
            if (installations == null)
            {
                System.Diagnostics.Debug.WriteLine("No installations data.");
                return null;
            }

            var measurements = new List<Measurement>();

            foreach (var installation in installations)
            {
                var query = GetQuery(new Dictionary<string, object>
                {
                    { "installationId", installation.Id }
                });
                var url = GetAirlyApiUrl(App.AirlyApiMeasurementUrl, query);

                var response = await GetHttpResponseAsync<Measurement>(url);

                if (response != null)
                {
                    response.Installation = installation;
                    response.CurrentDisplayValue = (int)Math.Round(response.Current?.Indexes?.FirstOrDefault()?.Value ?? 0);
                    measurements.Add(response);
                }
            }

            return measurements;
        }

        private string GetQuery(IDictionary<string, object> args)
        {
            if (args == null) return null;

            var query = HttpUtility.ParseQueryString(string.Empty);

            foreach (var arg in args)
            {
                if (arg.Value is double number)
                {
                    query[arg.Key] = number.ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    query[arg.Key] = arg.Value?.ToString();
                }
            }

            return query.ToString();
        }

        private string GetAirlyApiUrl(string path, string query)
        {
            var builder = new UriBuilder(App.AirlyApiUrl);
            builder.Port = -1;
            builder.Path += path;
            builder.Query = query;
            string url = builder.ToString();

            return url;
        }

        private static HttpClient GetHttpClient()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(App.AirlyApiUrl);

            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");
            client.DefaultRequestHeaders.Add("apikey", App.AirlyApiKey);
            return client;
        }

        private async Task<T> GetHttpResponseAsync<T>(string url)
        {
            try
            {
                var client = GetHttpClient();
                var response = await client.GetAsync(url);

                if (response.Headers.TryGetValues("X-RateLimit-day", out var daylimit) &&
                    response.Headers.TryGetValues("X-RateLimit-Reaming-day", out var dayLimitTeaming))
                {
                    System.Diagnostics.Debug.WriteLine($"Day limit : {daylimit?.FirstOrDefault()}, reaining : {dayLimitTeaming?.FirstOrDefault()}");
                }

                switch ((int)response.StatusCode)
                {
                    case 200:
                        var content = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<T>(content);
                        return result;
                    case 429:
                        System.Diagnostics.Debug.WriteLine("Too many requests");
                        break;
                    default:
                        var errorContent = await response.Content.ReadAsStringAsync();
                        System.Diagnostics.Debug.WriteLine($"Response error:{errorContent}");
                        return default;
                }
            }
            catch (JsonReaderException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
            catch (WebException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

            return default;

        }

        private ICommand _goToDetailsCommand;
        public ICommand GoToDetailsCommand => _goToDetailsCommand ?? (_goToDetailsCommand = new Command(OnGoToDetails));

        private void OnGoToDetails()
        {
            _navigation.PushAsync(new DetailsPage());
        }
    }
}
