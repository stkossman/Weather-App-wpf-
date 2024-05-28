using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace WeatherApp
{
    public partial class MainWindow : Window
    {
        private const string OPENWEATHERMAP_API_KEY = "7bb2dc954f6d15a68e1f7acce994f9ec";
        private const string OPENWEATHERMAP_BASE_URL = "http://api.openweathermap.org/data/2.5/weather";
        private const string WEATHERBIT_API_KEY = "3cdd5fe5451b4bdd9b8561a7d5e16015";
        private const string WEATHERBIT_BASE_URL = "https://api.weatherbit.io/v2.0/current";

        public MainWindow()
        {
            InitializeComponent();
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
                string weatherData = await GetWeatherData_open(city);
                var weatherInfo = JsonConvert.DeserializeObject<OpenWeatherMapWeatherInfo>(weatherData);
                DisplayWeather_open(weatherInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void GetWeatherButton2_Click(object sender, RoutedEventArgs e)
        {
            string city = CityTextBox1.Text;
            if (string.IsNullOrWhiteSpace(city))
            {
                MessageBox.Show("Please enter a city name.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                string weatherData = await GetWeatherData_bit(city);
                var weatherInfo = JsonConvert.DeserializeObject<WeatherbitWeatherInfo>(weatherData);
                DisplayWeather_bit(weatherInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                WeatherResultTextBlock.Text = $"Weather in {weatherInfo.Name}:\n" +
                                              $"Temperature: {weatherInfo.Main.Temp}°C\n" +
                                              $"Humidity: {weatherInfo.Main.Humidity}%\n" +
                                              $"Condition: {weatherInfo.Weather[0].Description}";
            }
            else
            {
                WeatherResultTextBlock.Text = "No weather information found.";
            }
        }

        private void DisplayWeather_bit(WeatherbitWeatherInfo weatherInfo)
        {
            if (weatherInfo != null && weatherInfo.data.Length > 0)
            {
                var currentWeather = weatherInfo.data[0];
                WeatherResultTextBlock2.Text = $"Weather in {currentWeather.city_name}:\n" +
                                               $"Temperature: {currentWeather.temp}°C\n" +
                                               $"Humidity: {currentWeather.rh}%\n" +
                                               $"Condition: {currentWeather.weather.description}";
            }
            else
            {
                WeatherResultTextBlock2.Text = "No weather information found.";
            }
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
}
