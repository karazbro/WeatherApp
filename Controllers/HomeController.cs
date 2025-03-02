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
            // �������� ������ ��������� ��� � �������
            var years = await _repository.GetAvailableYearsAsync();

            // ���� ���� ������, ������� ��������� ��������� ��� ��� �����������
            int selectedYear = years.Count > 0 ? years[years.Count - 1] : DateTime.Now.Year;

            // �������� ������� ����������� �� ������� ��� ���������� ����
            var temperatures = await _repository.GetAverageTemperatureByMonthAsync(selectedYear);

            // ������� ������ �������������
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
            // �������� ������ ���� ��������� ���
            var years = await _repository.GetAvailableYearsAsync();

            // �������� ������� ����������� ��� ���������� ����
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