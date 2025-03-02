using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using WeatherApp.Models;

namespace WeatherApp.Services
{
    public class ExcelParserService
    {
        public List<WeatherData> ParseExcelFile(Stream fileStream)
        {
            var result = new List<WeatherData>();

            try
            {
                Console.WriteLine("Создание XSSFWorkbook...");
                var workbook = new XSSFWorkbook(fileStream);
                Console.WriteLine($"Количество листов: {workbook.NumberOfSheets}");

                for (int sheetIndex = 0; sheetIndex < workbook.NumberOfSheets; sheetIndex++)
                {
                    var sheet = workbook.GetSheetAt(sheetIndex);
                    if (sheet == null)
                    {
                        Console.WriteLine($"Лист {sheetIndex} пустой.");
                        continue;
                    }
                    Console.WriteLine($"Обработка листа: {sheet.SheetName} (всего строк: {sheet.LastRowNum + 1})");

                    
                    for (int rowIndex = 4; rowIndex <= sheet.LastRowNum; rowIndex++)
                    {
                        Console.WriteLine($"Попытка обработать строку {rowIndex}...");
                        var row = sheet.GetRow(rowIndex);
                        if (row == null)
                        {
                            Console.WriteLine($"Строка {rowIndex} отсутствует.");
                            continue;
                        }

                        if (IsEmptyRow(row))
                        {
                            Console.WriteLine($"Строка {rowIndex} пуста или пропущена. Кол-во ячеек: {row.Cells.Count}");
                            continue;
                        }

                        var weatherData = ParseRow(row);
                        if (weatherData != null)
                        {
                            result.Add(weatherData);
                            Console.WriteLine(
                                $"Добавлена запись: " +
                                $"{weatherData.Date:dd.MM.yyyy} " +
                                $"{SafeTimeToString(weatherData.Time)} " +
                                $"(Темп: {weatherData.Temperature}, Влажность: {weatherData.Humidity})");
                        }
                        else
                        {
                            Console.WriteLine($"Строка {rowIndex} пропущена (не удалось распарсить). Проверяем значения:");
                            LogRowValues(row);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при чтении Excel-файла: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
            }

            Console.WriteLine($"Успешно обработано {result.Count} записей.");
            return result;
        }

        private bool IsEmptyRow(IRow row)
        {
            if (row == null || row.Cells.Count == 0) return true;
            return row.Cells.All(c => c.CellType == CellType.Blank || string.IsNullOrWhiteSpace(GetCellValueAsString(c)));
        }


        private WeatherData? ParseRow(IRow row)
        {
            try
            {
                Console.WriteLine("Начало парсинга строки...");

                // Получаем ячейки даты и времени
                var dateCell = row.GetCell(0);
                var timeCell = row.GetCell(1);

                if (dateCell == null || timeCell == null)
                {
                    Console.WriteLine("Дата или время отсутствуют в нужных столбцах.");
                    return null;
                }

                // Парсинг даты
                string? dateStr = GetCellValueAsString(dateCell)?.Trim();
                if (string.IsNullOrEmpty(dateStr))
                {
                    Console.WriteLine($"Дата пустая или некорректна: {dateStr} (Тип ячейки: {dateCell?.CellType})");
                    return null;
                }

                if (!TryParseDate(dateStr, out DateTime date))
                {
                    Console.WriteLine($"Неверный формат даты: {dateStr} (Тип ячейки: {dateCell?.CellType})");
                    return null;
                }

                // Парсинг времени
                string? timeStr = GetCellValueAsString(timeCell)?.Trim();
                if (string.IsNullOrEmpty(timeStr))
                {
                    Console.WriteLine($"Время пустое или некорректно: {timeStr} (Тип ячейки: {timeCell?.CellType})");
                    return null;
                }

                if (!TryParseTime(timeStr, out TimeSpan time))
                {
                    Console.WriteLine($"Неверный формат времени: {timeStr} (Тип ячейки: {timeCell?.CellType}). Пропускаем строку.");
                    return null;
                }

                // Собираем модель WeatherData
                var weatherData = new WeatherData
                {
                    Date = date,
                    Time = time,
                    Temperature = GetDoubleValue(row.GetCell(2)),     
                    Humidity = GetIntValue(row.GetCell(3)),           
                    DewPoint = GetDoubleValue(row.GetCell(4)),        
                    Pressure = GetIntValue(row.GetCell(5)),           
                    WindDirection = GetStringValue(row.GetCell(6)),   
                    WindSpeed = GetIntValue(row.GetCell(7)),          
                    Cloudiness = GetIntValue(row.GetCell(8)),         
                    CloudBase = GetIntValue(row.GetCell(9)),          
                    Visibility = GetStringValue(row.GetCell(10)),     
                    WeatherPhenomena = GetStringValue(row.GetCell(11))
                };

                return weatherData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при парсинге строки: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return null;
            }
        }


        private bool TryParseDate(string? dateStr, out DateTime date)
        {
            date = DateTime.MinValue;
            if (string.IsNullOrEmpty(dateStr))
            {
                return false;
            }

            
            string[] formats = { "dd.MM.yyyy", "d.M.yyyy" };
            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(dateStr, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                {
                    return true;
                }
            }

            // Пробуем общий парсинг
            if (DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                return true;
            }

            
            if (double.TryParse(dateStr.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double serialDate))
            {
                if (serialDate > 0 && serialDate < 50000) 
                {
                    date = DateTime.FromOADate(serialDate);
                    return true;
                }
            }

            return false;
        }


        private bool TryParseTime(string? timeStr, out TimeSpan time)
        {
            time = TimeSpan.Zero;
            if (string.IsNullOrEmpty(timeStr))
            {
                return false;
            }

            // Наиболее часто встречающиеся форматы
            string[] formats = { "HH:mm", "H:mm", "hh:mm", "h:mm" };
            foreach (var format in formats)
            {
                if (TimeSpan.TryParseExact(timeStr, format, CultureInfo.InvariantCulture, out time))
                {
                    
                    if (IsTimeWithin24Hours(time))
                        return true;
                }
            }

            // Пробуем общий парсинг
            if (TimeSpan.TryParse(timeStr, CultureInfo.InvariantCulture, out time))
            {
                if (IsTimeWithin24Hours(time))
                {
                    Console.WriteLine($"Успешно распарсено время общим парсингом: {time}");
                    return true;
                }
            }

            
            if (double.TryParse(timeStr.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double serialTime))
            {
                double fractionalPart = serialTime % 1.0; 
                if (fractionalPart < 0) fractionalPart = 0; 
                var possibleTime = TimeSpan.FromDays(fractionalPart);
                if (IsTimeWithin24Hours(possibleTime))
                {
                    Console.WriteLine($"Успешно распарсено время как числовое: {possibleTime}");
                    time = possibleTime;
                    return true;
                }
            }

            return false;
        }


        private bool IsTimeWithin24Hours(TimeSpan time)
        {
            if (time < TimeSpan.Zero) return false;
            return time < TimeSpan.FromHours(24);
        }


        private string SafeTimeToString(TimeSpan time)
        {
            try
            {
                return time.ToString(@"hh\:mm");
            }
            catch
            {
                return time.ToString();
            }
        }


        private string? GetCellValueAsString(ICell? cell)
        {
            if (cell == null) return null;
            switch (cell.CellType)
            {
                case CellType.String:
                    return cell.StringCellValue?.Trim();

                case CellType.Numeric:

                    if (DateUtil.IsCellDateFormatted(cell))
                    {

                        return string.Format(CultureInfo.InvariantCulture, "{0:dd.MM.yyyy HH:mm}", cell.DateCellValue);

                    }

                    return cell.NumericCellValue.ToString(CultureInfo.InvariantCulture);

                case CellType.Boolean:
                    return cell.BooleanCellValue.ToString();

                case CellType.Blank:
                    return null;

                default:

                    return cell.ToString()?.Trim();
            }
        }

        private double? GetDoubleValue(ICell? cell)
        {
            if (cell == null) return null;
            string? value = GetCellValueAsString(cell);
            if (string.IsNullOrEmpty(value)) return null;

            value = value.Replace(',', '.').Replace(" ", "");
            if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
            {
                return result;
            }
            Console.WriteLine($"Не удалось распарсить число: {value}");
            return null;
        }

        private int? GetIntValue(ICell? cell)
        {
            var doubleValue = GetDoubleValue(cell);
            if (doubleValue.HasValue)
            {

                return (int)Math.Round(doubleValue.Value);
            }
            return null;
        }

        private string? GetStringValue(ICell? cell)
        {
            if (cell == null) return null;
            string? value = GetCellValueAsString(cell);
            return string.IsNullOrEmpty(value) ? null : value.Trim();
        }


        private void LogRowValues(IRow row)
        {
            for (int i = 0; i < row.LastCellNum; i++)
            {
                var cell = row.GetCell(i);
                string? value = GetCellValueAsString(cell) ?? "null";
                Console.WriteLine($"Столбец {i}: {value} (Тип: {cell?.CellType})");
            }
        }
    }
}
