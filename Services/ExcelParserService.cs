using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
                // Создаем рабочую книгу NPOI из файлового потока
                var workbook = new XSSFWorkbook(fileStream);

                // Обрабатываем каждый лист (месяц) в файле
                for (int sheetIndex = 0; sheetIndex < workbook.NumberOfSheets; sheetIndex++)
                {
                    var sheet = workbook.GetSheetAt(sheetIndex);

                    // Пропускаем первые 4 строки (заголовки и т.д.)
                    for (int rowIndex = 4; rowIndex <= sheet.LastRowNum; rowIndex++)
                    {
                        var row = sheet.GetRow(rowIndex);
                        if (row == null || IsEmptyRow(row))
                            continue;

                        try
                        {
                            // Парсим данные из строки Excel
                            var weatherData = ParseRow(row);
                            if (weatherData != null)
                            {
                                result.Add(weatherData);
                            }
                        }
                        catch (Exception ex)
                        {
                            // Логируем ошибку и продолжаем с следующей строкой
                            Console.WriteLine($"Ошибка при парсинге строки {rowIndex} на листе {sheet.SheetName}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Логируем общую ошибку при парсинге файла
                Console.WriteLine($"Ошибка при парсинге Excel-файла: {ex.Message}");
                throw;
            }

            return result;
        }

        private bool IsEmptyRow(IRow row)
        {
            // Проверяем, пуста ли строка
            if (row.Cells.Count == 0)
                return true;

            return row.Cells.All(c => c.CellType == CellType.Blank);
        }

        private WeatherData? ParseRow(IRow row)
        {
            // Получаем ячейки
            var dateCell = row.GetCell(0);
            var timeCell = row.GetCell(1);

            if (dateCell == null || timeCell == null)
                return null;

            // Парсим дату и время
            DateTime date;
            TimeSpan time;

            // Парсим дату
            if (dateCell.CellType == CellType.String)
            {
                if (!DateTime.TryParseExact(dateCell.StringCellValue, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    return null;
            }
            else if (dateCell.CellType == CellType.Numeric)
            {
                date = dateCell.DateCellValue ?? DateTime.MinValue;
            }
            else
            {
                return null;
            }

            // Парсим время
            if (timeCell.CellType == CellType.String)
            {
                if (!TimeSpan.TryParseExact(timeCell.StringCellValue, "HH:mm", CultureInfo.InvariantCulture, out time))
                    return null;
            }
            else if (timeCell.CellType == CellType.Numeric)
            {
                time = TimeSpan.FromDays(timeCell.NumericCellValue); // TODO уточнить моментик
            }
            else
            {
                return null;
            }

            // Создаем объект WeatherData
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

        // Вспомогательные методы для получения значений разных типов из ячеек Excel
        private double? GetDoubleValue(ICell cell)
        {
            if (cell == null)
                return null;

            if (cell.CellType == CellType.Numeric)
                return cell.NumericCellValue;

            if (cell.CellType == CellType.String)
            {
                if (double.TryParse(cell.StringCellValue.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
                    return value;
            }

            return null;
        }

        private int? GetIntValue(ICell cell)
        {
            var doubleValue = GetDoubleValue(cell);
            if (doubleValue.HasValue)
                return (int)Math.Round(doubleValue.Value);

            return null;
        }

        private string GetStringValue(ICell cell)
        {
            if (cell == null)
                return null;

            if (cell.CellType == CellType.String)
                return cell.StringCellValue;

            if (cell.CellType == CellType.Numeric)
                return cell.NumericCellValue.ToString(CultureInfo.InvariantCulture);

            return null;
        }
    }
}