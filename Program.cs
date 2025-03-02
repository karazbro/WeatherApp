using Microsoft.EntityFrameworkCore;
using WeatherApp.Data;
using WeatherApp.Services;
using WeatherApp.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ��������� ������� MVC
builder.Services.AddControllersWithViews();

// ������������ �������� �� ��� PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ������������ ����������� ������
builder.Services.AddScoped<IWeatherRepository, WeatherRepository>();

// ��� ���������� ����������� ������ � ������ ����������
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

// ��������� ������ � ��������� ���������
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

// ��������� �������������: ���������� �� ��������� � Home, �������� � Index
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapDatabaseErrorPage();

app.Run();

