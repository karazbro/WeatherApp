using Microsoft.EntityFrameworkCore;
using WeatherApp.Models;

namespace WeatherApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<WeatherData> WeatherData { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Настраиваем индексы для ускорения запросов по дате, месяцу и году
            modelBuilder.Entity<WeatherData>()
                .HasIndex(w => w.Date);

            // Можно добавить дополнительные настройки модели здесь
        }
    }
}