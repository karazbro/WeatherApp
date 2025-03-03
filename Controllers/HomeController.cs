using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WeatherApp.Models;
using WeatherApp.Repositories;
using WeatherApp.Utilities;

namespace WeatherApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly WeatherDataRepository _repository;

        public HomeController(WeatherDataRepository repository)
        {
            _repository = repository;
        }

        public async Task<IActionResult> Index(int? year)
        {
            var model = new WeatherDashboardViewModel
            {
                Years = await _repository.GetAvailableYearsAsync(),
                SelectedYear = year ?? DateTime.Now.Year,
                AverageTemperatures = await _repository.GetAverageTemperatureByMonthAsync(year ?? DateTime.Now.Year) ?? new Dictionary<string, double>()
            };
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}