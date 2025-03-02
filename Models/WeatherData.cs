using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeatherApp.Models
{
    public class WeatherData
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "timestamp without time zone")]
        public DateTime Date { get; set; }

        [Required]
        public TimeSpan Time { get; set; }

        // Температура воздуха в градусах Цельсия
        public double? Temperature { get; set; }

        // Относительная влажность воздуха в процентах
        public int? Humidity { get; set; }

        // Точка росы в градусах Цельсия
        public double? DewPoint { get; set; }

        // Атмосферное давление в мм рт.ст.
        public int? Pressure { get; set; }

        // Направление ветра
        public string WindDirection { get; set; }

        // Скорость ветра в м/с
        public int? WindSpeed { get; set; }

        // Облачность в процентах
        public int? Cloudiness { get; set; }

        // Нижняя граница облачности в метрах
        public int? CloudBase { get; set; }

        // Горизонтальная видимость в км
        public string? Visibility { get; set; }

        // Погодные явления
        public string? WeatherPhenomena { get; set; }

        // Для группировки по месяцам и годам
        public int Month => Date.Month;
        public int Year => Date.Year;
    }
}