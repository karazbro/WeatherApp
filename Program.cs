using Microsoft.EntityFrameworkCore;
using WeatherApp.Data;
using WeatherApp.Services;
using WeatherApp.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Добавляем сервисы MVC
builder.Services.AddControllersWithViews();

// Регистрируем контекст БД для PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Регистрируем репозиторий погоды
builder.Services.AddScoped<IWeatherRepository, WeatherRepository>();

// Для подробного отображения ошибок в режиме разработки
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

// Обработка ошибок и настройка конвейера
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();
app.UseRouting();

// Настройка маршрутизации: контроллер по умолчанию – Home, действие – Index
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapDatabaseErrorPage();

app.Run();

