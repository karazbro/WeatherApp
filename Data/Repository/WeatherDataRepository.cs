using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeatherApp.Data;
using WeatherApp.Models;
using WeatherApp.Utilities;

namespace WeatherApp.Repositories
{
    public class WeatherDataRepository
    {
        private readonly ApplicationDbContext _context;

        public WeatherDataRepository(ApplicationDbContext context)
        {
            _context = context;
            Console.WriteLine("WeatherDataRepository создан успешно.");
        }

        public async Task<bool> SaveWeatherDataAsync(List<WeatherData> weatherData)
        {
            try
            {
                Console.WriteLine($"Попытка сохранить {weatherData.Count} записей.");
                var existingDates = await _context.WeatherData
                    .Select(w => new { w.Date, w.Time })
                    .ToListAsync();

                var newData = weatherData
                    .Where(w => !existingDates.Any(e => e.Date == w.Date && e.Time == w.Time))
                    .ToList();

                Console.WriteLine($"Новых записей для сохранения: {newData.Count}");
                if (newData.Any())
                {
                    await _context.WeatherData.AddRangeAsync(newData);
                    await _context.SaveChangesAsync();
                    Console.WriteLine("Данные успешно сохранены.");
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении данных: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<List<WeatherData>> GetAllWeatherDataAsync()
        {
            return await _context.WeatherData.OrderBy(w => w.Date).ThenBy(w => w.Time).ToListAsync();
        }

        public async Task<List<WeatherData>> GetWeatherDataByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.WeatherData
                .Where(w => w.Date >= startDate && w.Date <= endDate)
                .OrderBy(w => w.Date)
                .ThenBy(w => w.Time)
                .ToListAsync();
        }

        public async Task<PaginatedList<WeatherData>> GetWeatherDataByDateRangePaginatedAsync(
            DateTime startDate, DateTime endDate, int pageNumber, int pageSize)
        {
            var query = _context.WeatherData
                .Where(w => w.Date >= startDate && w.Date <= endDate)
                .OrderBy(w => w.Date)
                .ThenBy(w => w.Time);

            return await PaginatedList<WeatherData>.CreateAsync(query, pageNumber, pageSize);
        }

        public async Task<List<WeatherData>> GetWeatherDataByMonthAndYearAsync(int month, int year)
        {
            return await _context.WeatherData
                .Where(w => w.Date.Month == month && w.Date.Year == year)
                .OrderBy(w => w.Date)
                .ThenBy(w => w.Time)
                .ToListAsync();
        }

        public async Task<bool> DeleteAllWeatherDataAsync()
        {
            try
            {
                _context.WeatherData.RemoveRange(_context.WeatherData);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении данных: {ex.Message}");
                return false;
            }
        }

        public async Task<Dictionary<string, double>> GetAverageTemperatureByMonthAsync(int year)
        {
            var result = await _context.WeatherData
                .Where(w => w.Date.Year == year && w.Temperature.HasValue)
                .GroupBy(w => w.Date.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    AverageTemperature = g.Average(w => w.Temperature!.Value) 
                })
                .OrderBy(x => x.Month)
                .ToListAsync();

            var culture = new System.Globalization.CultureInfo("ru-RU");
            return result.ToDictionary(
                r => new DateTime(year, r.Month, 1).ToString("MMMM", culture),
                r => Math.Round(r.AverageTemperature, 1)
            );
        }

        public async Task<List<int>> GetAvailableYearsAsync()
        {
            return await _context.WeatherData
                .Select(w => w.Date.Year)
                .Distinct()
                .OrderBy(y => y)
                .ToListAsync();
        }

        public async Task<Dictionary<string, Dictionary<string, double>>> GetTemperatureStatisticsByMonthAsync(int year)
        {
            var culture = new System.Globalization.CultureInfo("ru-RU");
            var statistics = new Dictionary<string, Dictionary<string, double>>();

            for (int month = 1; month <= 12; month++)
            {
                var data = await _context.WeatherData
                    .Where(w => w.Date.Year == year && w.Date.Month == month && w.Temperature.HasValue)
                    .ToListAsync();

                if (data.Any())
                {
                    var temps = data.Select(d => d.Temperature!.Value).ToList(); 
                    var monthName = new DateTime(year, month, 1).ToString("MMMM", culture);

                    statistics[monthName] = new Dictionary<string, double>
                    {
                        ["Средняя"] = Math.Round(temps.Average(), 1),
                        ["Минимальная"] = Math.Round(temps.Min(), 1),
                        ["Максимальная"] = Math.Round(temps.Max(), 1)
                    };
                }
            }

            return statistics;
        }
    }
}