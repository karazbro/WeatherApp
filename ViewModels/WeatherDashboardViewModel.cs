using System.Collections.Generic;

namespace WeatherApp.Models
{
    public class WeatherDashboardViewModel
    {
        public List<int> Years { get; set; } = new List<int>();
        public int SelectedYear { get; set; }
        public Dictionary<string, double> MonthlyTemperatures { get; set; } = new Dictionary<string, double>();
    }
}