using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace WeatherApp
{
    public partial class MainWindow : Window
    {
        private const string OPENWEATHERMAP_API_KEY = "7bb2dc954f6d15a68e1f7acce994f9ec";
        private const string OPENWEATHERMAP_BASE_URL = "http://api.openweathermap.org/data/2.5/weather";
        private const string OPENWEATHERMAP_FORECAST_URL = "http://api.openweathermap.org/data/2.5/forecast";
        private const string WEATHERBIT_API_KEY = "3cdd5fe5451b4bdd9b8561a7d5e16015";
        private const string WEATHERBIT_BASE_URL = "https://api.weatherbit.io/v2.0/current";

        public MainWindow()
        {
            InitializeComponent();

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += UpdateDateTime;
            timer.Start();
        }

        private void UpdateDateTime(object sender, EventArgs e)
        {
            DateTime currentDateTime = DateTime.Now;
            string dateTimeString = $"{currentDateTime:dddd, dd MMMM yyyy HH:mm:ss}";
            CurrentDateTimeLabel.Content = dateTimeString;
        }

        private async void GetWeatherButton_Click(object sender, RoutedEventArgs e)
        {
            string city = CityTextBox.Text;
            if (string.IsNullOrWhiteSpace(city))
            {
                MessageBox.Show("Please enter a city name.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                string weatherDataOpen = await GetWeatherData_open(city);
                var weatherInfoOpen = JsonConvert.DeserializeObject<OpenWeatherMapWeatherInfo>(weatherDataOpen);
                DisplayWeather_open(weatherInfoOpen);

                string weatherDataBit = await GetWeatherData_bit(city);
                var weatherInfoBit = JsonConvert.DeserializeObject<WeatherbitWeatherInfo>(weatherDataBit);
                DisplayWeather_bit(weatherInfoBit);

                string forecastData = await GetForecast(city);
                var forecastInfo = JsonConvert.DeserializeObject<OpenWeatherMapForecastInfo>(forecastData);
                DisplayForecast(forecastInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<string> GetForecast(string city)
        {
            using (HttpClient client = new HttpClient())
            {
                string url = $"{OPENWEATHERMAP_FORECAST_URL}?q={city}&appid={OPENWEATHERMAP_API_KEY}&units=metric";
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
        }

        private void DisplayForecast(OpenWeatherMapForecastInfo forecastInfo)
        {
            if (forecastInfo != null && forecastInfo.list.Length > 0)
            {
                var sb = new StringBuilder();
                sb.AppendLine("5-Day Forecast:");
                var groupedByDate = forecastInfo.list.GroupBy(x => x.dt_txt.Split(' ')[0])
                                                     .Take(5);

                foreach (var group in groupedByDate)
                {
                    var date = group.Key;
                    var dayData = group.First();
                    sb.AppendLine($"{date}: {dayData.main.Temp}°C, {dayData.weather[0].Description}");
                }

                ForecastTextBlock.Text = sb.ToString();
            }
            else
            {
                ForecastTextBlock.Text = "No 5-day forecast information found.";
            }
        }

        private async Task<string> GetWeatherData_open(string city)
        {
            using (HttpClient client = new HttpClient())
            {
                string url = $"{OPENWEATHERMAP_BASE_URL}?q={city}&appid={OPENWEATHERMAP_API_KEY}&units=metric";
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
        }

        private async Task<string> GetWeatherData_bit(string city)
        {
            using (HttpClient client = new HttpClient())
            {
                string url = $"{WEATHERBIT_BASE_URL}?city={city}&key={WEATHERBIT_API_KEY}";
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
        }

        private void DisplayWeather_open(OpenWeatherMapWeatherInfo weatherInfo)
        {
            if (weatherInfo != null)
            {
                WeatherResultTextBlock.Text = $"OpenWeather API (Weather in {weatherInfo.Name}):\n" +
                                              $"Temperature: {weatherInfo.Main.Temp}°C\n" +
                                              $"Humidity: {weatherInfo.Main.Humidity}%\n" +
                                              $"Condition: {weatherInfo.Weather[0]?.Description}";

                string iconName = GetIconName(weatherInfo.Weather[0]?.Description);
                string appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string imagePath = System.IO.Path.Combine(appPath, $"weathericons/{iconName}.png");

                BitmapImage bitmap = new BitmapImage();
                if (File.Exists(imagePath))
                {
                    using (FileStream stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                    {
                        bitmap.BeginInit();
                        bitmap.StreamSource = stream;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                    }
                }
                WeatherIcon.Source = bitmap;
            }
            else
            {
                WeatherResultTextBlock.Text = "No weather information found from OpenWeather API.";
                WeatherIcon.Source = null;
            }
        }

        private void DisplayWeather_bit(WeatherbitWeatherInfo weatherInfo)
        {
            if (weatherInfo != null && weatherInfo.data.Length > 0)
            {
                var data = weatherInfo.data[0];
                WeatherResultTextBlock2.Text = $"WeatherBit API (Weather in {data.city_name}):\n" +
                                               $"Temperature: {data.temp}°C\n" +
                                               $"Humidity: {data.rh}%\n" +
                                               $"Condition: {data.weather?.description}";
            }
            else
            {
                WeatherResultTextBlock2.Text = "No weather information found from WeatherBit API.";
            }
        }

        private string GetIconName(string weatherDescription)
        {
            switch (weatherDescription?.ToLower())
            {
                case "clear sky":
                    return "sunny";
                case "few clouds":
                    return "partlycloudy";
                case "scattered clouds":
                    return "mostlycloudy";
                case "overcast clouds":
                    return "mostlycloudy";
                case "broken clouds":
                    return "cloudy";
                case "shower rain":
                    return "showerrain";
                case "intensity rain":
                    return "showerrain";
                case "heavy intensity rain":
                    return "showerrain";
                case "light rain":
                case "moderate rain":
                    return "rain";
                case "thunderstorm":
                    return "thunderstorm";
                case "snow":
                    return "snow";
                case "mist":
                    return "mist";
                default:
                    return "unknown";
            }
        }

        public class OpenWeatherMapWeatherInfo
        {
            public string Name { get; set; }
            public Main Main { get; set; }
            public Weather[] Weather { get; set; }
        }

        public class Main
        {
            public float Temp { get; set; }
            public int Humidity { get; set; }
        }

        public class Weather
        {
            public string Description { get; set; }
        }

        public class WeatherbitWeatherInfo
        {
            public Data[] data { get; set; }
        }

        public class Data
        {
            public string city_name { get; set; }
            public float temp { get; set; }
            public int rh { get; set; }
            public WeatherDescription weather { get; set; }
        }

        public class WeatherDescription
        {
            public string description { get; set; }
        }

        public class OpenWeatherMapForecastInfo
        {
            public List[] list { get; set; }
        }
        public class List
        {
            public Main main { get; set; }
            public Weather[] weather { get; set; }
            public string dt_txt { get; set; }
        }
    }
}
