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
        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        [Required]
        [Column(TypeName = "time without time zone")]
        public TimeSpan Time { get; set; }

        // Температура
        public double? Temperature { get; set; }

        // влажность
        public int? Humidity { get; set; }

        // Точка росы
        public double? DewPoint { get; set; }

        //давление
        public int? Pressure { get; set; }

        // Направление ветра
        public string? WindDirection { get; set; }

        // Скорость ветра
        public int? WindSpeed { get; set; }

        // Облачность
        public int? Cloudiness { get; set; }

        // Нижняя граница облачности
        public int? CloudBase { get; set; }

        // Горизонтальная видимость
        public string? Visibility { get; set; }

        // Погодные явления
        public string? WeatherPhenomena { get; set; }

        [NotMapped]
        public int Month => Date.Month;

        [NotMapped]
        public int Year => Date.Year;
    }
}