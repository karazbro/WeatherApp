using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WeatherApp.Data;
using WeatherApp.Models;
using WeatherApp.Repositories;
using WeatherApp.Services;
using WeatherApp.Utilities;

namespace WeatherApp.Controllers
{
    public class DataController : Controller
    {
        private readonly ExcelParserService _excelParserService;
        private readonly WeatherDataRepository _repository;
        private readonly ApplicationDbContext _context;

        public DataController(ExcelParserService excelParserService, WeatherDataRepository repository, ApplicationDbContext context)
        {
            _excelParserService = excelParserService;
            _repository = repository;
            _context = context;
            Console.WriteLine("DataController создан успешно.");
        }

        // GET: Data/Import
        public IActionResult Import()
        {
            return View();
        }

        // POST: Data/Import
        [HttpPost]
        public async Task<IActionResult> Import(List<IFormFile> excelFiles)
        {
            Console.WriteLine("Начало импорта данных.");
            if (excelFiles == null || excelFiles.Count == 0)
            {
                TempData["Error"] = "Пожалуйста, выберите хотя бы один Excel-файл.";
                return View();
            }

            int totalImported = 0;
            List<string> processedFiles = new List<string>();
            List<string> failedFiles = new List<string>();

            foreach (var file in excelFiles)
            {
                Console.WriteLine($"Обработка файла: {file.FileName}");
                if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    failedFiles.Add(file.FileName + " (неверный формат)");
                    continue;
                }

                try
                {
                    using (var stream = file.OpenReadStream())
                    {
                        Console.WriteLine($"Чтение файла {file.FileName}");
                        var weatherData = _excelParserService.ParseExcelFile(stream);
                        if (weatherData == null || weatherData.Count == 0)
                        {
                            failedFiles.Add(file.FileName + " (нет данных)");
                            continue;
                        }

                        Console.WriteLine($"Сохранение {weatherData.Count} записей из файла {file.FileName}");
                        bool success = await _repository.SaveWeatherDataAsync(weatherData);
                        if (success)
                        {
                            totalImported += weatherData.Count;
                            processedFiles.Add(file.FileName);
                        }
                        else
                        {
                            failedFiles.Add(file.FileName + " (ошибка сохранения)");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при обработке файла {file.FileName}: {ex.Message}");
                    failedFiles.Add(file.FileName + " (" + ex.Message + ")");
                }
            }

            if (totalImported > 0)
            {
                TempData["Success"] = $"Успешно импортировано {totalImported} записей из {processedFiles.Count} файлов.";
                if (failedFiles.Count > 0)
                {
                    TempData["Error"] = $"Не удалось обработать: {string.Join(", ", failedFiles)}";
                }
            }
            else
            {
                TempData["Error"] = "Не удалось импортировать данные. Проверьте файлы и попробуйте снова.";
            }

            Console.WriteLine("Импорт завершён.");
            return RedirectToAction("Index", "Home");
        }

        // GET: Data/List
        public async Task<IActionResult> List(DateTime? startDate, DateTime? endDate, int page = 1)
        {
            startDate ??= DateTime.Today.AddMonths(-1);
            endDate ??= DateTime.Today;

            int pageSize = 50;
            var paginatedData = await _repository.GetWeatherDataByDateRangePaginatedAsync(startDate.Value, endDate.Value, page, pageSize);

            ViewBag.StartDate = startDate.Value.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate.Value.ToString("yyyy-MM-dd");
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = paginatedData.TotalPages;
            ViewBag.HasPreviousPage = page > 1;
            ViewBag.HasNextPage = page < paginatedData.TotalPages;

            return View(paginatedData);
        }

        // GET: Data/Details?month=5&year=2023
        public async Task<IActionResult> Details(int? month = null, int? year = null)
        {
            month ??= DateTime.Now.Month;
            year ??= DateTime.Now.Year;

            if (month < 1 || month > 12) month = DateTime.Now.Month;
            if (year < 1900 || year > DateTime.Now.Year) year = DateTime.Now.Year;

            var data = await _repository.GetWeatherDataByMonthAndYearAsync(month.Value, year.Value);

            ViewBag.Month = new DateTime(year.Value, month.Value, 1).ToString("MMMM", new System.Globalization.CultureInfo("ru-RU"));
            ViewBag.Year = year;
            ViewBag.AvailableYears = await _repository.GetAvailableYearsAsync();
            ViewBag.SelectedMonth = month;
            ViewBag.SelectedYear = year;

            return View(data);
        }

        // GET: Data/DeleteAll
        public IActionResult DeleteAll()
        {
            return View();
        }

        // POST: Data/DeleteAll
        [HttpPost, ActionName("DeleteAll")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAllConfirmed()
        {
            var result = await _repository.DeleteAllWeatherDataAsync();
            if (result)
            {
                TempData["Success"] = "Все данные успешно удалены.";
            }
            else
            {
                TempData["Error"] = "Ошибка при удалении данных.";
            }
            return RedirectToAction("Index", "Home");
        }
    }
}