using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using WeatherApp.Models;
using WeatherApp.Repositories;

namespace WeatherApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly WeatherDataRepository _repository;

        public HomeController(WeatherDataRepository repository)
        {
            _repository = repository;
        }

        public async Task<IActionResult> Index()
        {
            // Получаем список доступных лет с данными
            var years = await _repository.GetAvailableYearsAsync();

            // Если есть данные, выберем последний доступный год для отображения
            int selectedYear = years.Count > 0 ? years[years.Count - 1] : DateTime.Now.Year;

            // Получаем средние температуры по месяцам для выбранного года
            var temperatures = await _repository.GetAverageTemperatureByMonthAsync(selectedYear);

            // Создаем модель представления
            var viewModel = new WeatherDashboardViewModel
            {
                Years = years,
                SelectedYear = selectedYear,
                MonthlyTemperatures = temperatures
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Index(int year)
        {
            // Получаем список всех доступных лет
            var years = await _repository.GetAvailableYearsAsync();

            // Получаем средние температуры для выбранного года
            var temperatures = await _repository.GetAverageTemperatureByMonthAsync(year);

            var viewModel = new WeatherDashboardViewModel
            {
                Years = years,
                SelectedYear = year,
                MonthlyTemperatures = temperatures
            };

            return View(viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}