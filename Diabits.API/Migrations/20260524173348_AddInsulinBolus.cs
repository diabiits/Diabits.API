using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Diabits.API.Migrations
{
    /// <inheritdoc />
    public partial class AddInsulinBolus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InsulinBoluses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [HealthDataPointSequence]"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Units = table.Column<double>(type: "float(4)", precision: 4, scale: 2, nullable: false),
                    CarbGrams = table.Column<double>(type: "float(5)", precision: 5, scale: 1, nullable: true),
                    GlucoseLevel = table.Column<double>(type: "float(3)", precision: 3, scale: 1, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InsulinBoluses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InsulinBoluses_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_InsulinBoluses_UserId",
                table: "InsulinBoluses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_InsulinBoluses_UserId_StartTime",
                table: "InsulinBoluses",
                columns: new[] { "UserId", "StartTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InsulinBoluses");
        }
    }
}
