using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WeatherApp.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WeatherData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Temperature = table.Column<double>(type: "double precision", nullable: true),
                    Humidity = table.Column<int>(type: "integer", nullable: true),
                    DewPoint = table.Column<double>(type: "double precision", nullable: true),
                    Pressure = table.Column<int>(type: "integer", nullable: true),
                    WindDirection = table.Column<string>(type: "text", nullable: false),
                    WindSpeed = table.Column<int>(type: "integer", nullable: true),
                    Cloudiness = table.Column<int>(type: "integer", nullable: true),
                    CloudBase = table.Column<int>(type: "integer", nullable: true),
                    Visibility = table.Column<string>(type: "text", nullable: false),
                    WeatherPhenomena = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeatherData", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WeatherData_Date",
                table: "WeatherData",
                column: "Date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WeatherData");
        }
    }
}
