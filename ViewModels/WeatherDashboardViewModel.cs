namespace WeatherApp.Models
{
    public class WeatherDashboardViewModel
    {
        public List<int> Years { get; set; } = new List<int>();
        public int SelectedYear { get; set; }
        public Dictionary<string, double> AverageTemperatures { get; set; } = new Dictionary<string, double>();
    }
}