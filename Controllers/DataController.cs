using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WeatherApp.Models;
using WeatherApp.Repositories;
using WeatherApp.Services;

namespace WeatherApp.Controllers
{
    public class DataController : Controller
    {
        private readonly ExcelParserService _excelParserService;
        private readonly WeatherDataRepository _repository;

        public DataController(ExcelParserService excelParserService, WeatherDataRepository repository)
        {
            _excelParserService = excelParserService;
            _repository = repository;
        }

        // GET: Data/Import
        public IActionResult Import()
        {
            return View();
        }

        // POST: Data/Import
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(List<IFormFile> excelFiles)
        {
            if (excelFiles == null || excelFiles.Count == 0)
            {
                ModelState.AddModelError("", "Пожалуйста, выберите хотя бы один Excel-файл для загрузки.");
                return View();
            }

            try
            {
                // Track total imported records
                int totalImportedRecords = 0;
                List<string> processedFiles = new List<string>();
                List<string> failedFiles = new List<string>();

                foreach (var excelFile in excelFiles)
                {
                    if (!excelFile.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                    {
                        ModelState.AddModelError("", $"Файл {excelFile.FileName} имеет неподдерживаемый формат. Пожалуйста, загрузите файлы в формате .xlsx");
                        continue;
                    }

                    try
                    {
                        // Открываем поток файла
                        using (var stream = excelFile.OpenReadStream())
                        {
                            // Парсим данные из файла
                            var weatherData = _excelParserService.ParseExcelFile(stream);

                            if (weatherData.Count > 0)
                            {
                                // Добавляем данные в БД (без очистки для пакетной загрузки)
                                var success = await _repository.SaveWeatherDataAsync(weatherData);

                                if (success)
                                {
                                    totalImportedRecords += weatherData.Count;
                                    processedFiles.Add(excelFile.FileName);
                                }
                                else
                                {
                                    failedFiles.Add(excelFile.FileName);
                                }
                            }
                            else
                            {
                                // Файл был обработан, но данных нет
                                ModelState.AddModelError("", $"Файл {excelFile.FileName} не содержит данных в ожидаемом формате.");
                                failedFiles.Add(excelFile.FileName);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Ошибка при обработке конкретного файла
                        ModelState.AddModelError("", $"Ошибка при обработке файла {excelFile.FileName}: {ex.Message}");
                        failedFiles.Add(excelFile.FileName);
                    }
                }

                // Итоговое сообщение
                if (totalImportedRecords > 0)
                {
                    TempData["Success"] = $"Успешно импортировано {totalImportedRecords} записей о погоде из {processedFiles.Count} файлов.";

                    if (failedFiles.Count > 0)
                    {
                        TempData["Warning"] = $"Не удалось обработать следующие файлы: {string.Join(", ", failedFiles)}";
                    }

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["Error"] = "Не удалось импортировать данные. Проверьте формат файлов.";
                    return View();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Общая ошибка при импорте данных: {ex.Message}");
                return View();
            }
        }

        // GET: Data/List
        public async Task<IActionResult> List(DateTime? startDate, DateTime? endDate, int page = 1)
        {
            if (!startDate.HasValue)
                startDate = DateTime.Today.AddMonths(-1);

            if (!endDate.HasValue)
                endDate = DateTime.Today;

            // Добавляем пагинацию
            int pageSize = 50; // количество записей на странице
            var paginatedData = await _repository.GetWeatherDataByDateRangePaginatedAsync(
                startDate.Value, endDate.Value, page, pageSize);

            ViewBag.StartDate = startDate.Value.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate.Value.ToString("yyyy-MM-dd");
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = paginatedData.TotalPages;
            ViewBag.HasPreviousPage = page > 1;
            ViewBag.HasNextPage = page < paginatedData.TotalPages;

            return View(paginatedData.Items);
        }

        // GET: Data/Details/5
        public async Task<IActionResult> Details(int month, int year)
        {
            if (month < 1 || month > 12)
            {
                month = DateTime.Now.Month;
            }

            if (year < 1900 || year > DateTime.Now.Year)
            {
                year = DateTime.Now.Year;
            }

            var data = await _repository.GetWeatherDataByMonthAndYearAsync(month, year);

            ViewBag.Month = new DateTime(year, month, 1).ToString("MMMM", new System.Globalization.CultureInfo("ru-RU"));
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
                TempData["Success"] = "Все данные о погоде были успешно удалены.";
            }
            else
            {
                TempData["Error"] = "Произошла ошибка при удалении данных.";
            }

            return RedirectToAction("Index", "Home");
        }
    }
}