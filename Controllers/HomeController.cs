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
            var model = new WeatherDashboardViewModel();
            model.Years = await _repository.GetAvailableYearsAsync();

            if (model.Years.Count == 0)
            {
                return View(model);
            }

            model.SelectedYear = year ?? model.Years.Last();
            model.AverageTemperatures = await _repository.GetAverageTemperatureByMonthAsync(model.SelectedYear);

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}