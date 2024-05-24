using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace WeatherApp
{
    public partial class MainWindow : Window
    {
        private const string API_KEY = "7bb2dc954f6d15a68e1f7acce994f9ec";
        private const string BASE_URL = "http://api.openweathermap.org/data/2.5/weather";

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
                string weatherData = await GetWeatherDataAsync(city);
                var weatherInfo = JsonConvert.DeserializeObject<WeatherInfo>(weatherData);
                DisplayWeather(weatherInfo);
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
                string weatherData = await GetWeatherDataAsync(city);
                var weatherInfo = JsonConvert.DeserializeObject<WeatherInfo>(weatherData);
                DisplayWeather2(weatherInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<string> GetWeatherDataAsync(string city)
        {
            using (HttpClient client = new HttpClient())
            {
                string url = $"{BASE_URL}?q={city}&appid={API_KEY}&units=metric";
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
        }

        private void DisplayWeather(WeatherInfo weatherInfo)
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
        private void DisplayWeather2(WeatherInfo weatherInfo)
        {
            if (weatherInfo != null)
            {
                WeatherResultTextBlock2.Text = $"Weather in {weatherInfo.Name}:\n" +
                                              $"Temperature: {weatherInfo.Main.Temp}°C\n" +
                                              $"Humidity: {weatherInfo.Main.Humidity}%\n" +
                                              $"Condition: {weatherInfo.Weather[0].Description}";
            }
            else
            {
                WeatherResultTextBlock2.Text = "No weather information found.";
            }
        }
    }

    public class WeatherInfo
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
}
