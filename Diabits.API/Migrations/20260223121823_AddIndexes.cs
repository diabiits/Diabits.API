using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Diabits.API.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "mmolL",
                table: "GlucoseLevels",
                type: "float(3)",
                precision: 3,
                scale: 1,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(3,1)",
                oldPrecision: 3,
                oldScale: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Workouts_UserId_StartTime",
                table: "Workouts",
                columns: new[] { "UserId", "StartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Steps_UserId_StartTime",
                table: "Steps",
                columns: new[] { "UserId", "StartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_SleepSessions_UserId_StartTime",
                table: "SleepSessions",
                columns: new[] { "UserId", "StartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Medications_UserId_StartTime",
                table: "Medications",
                columns: new[] { "UserId", "StartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_HeartRates_UserId_StartTime",
                table: "HeartRates",
                columns: new[] { "UserId", "StartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_GlucoseLevels_UserId_StartTime",
                table: "GlucoseLevels",
                columns: new[] { "UserId", "StartTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Workouts_UserId_StartTime",
                table: "Workouts");

            migrationBuilder.DropIndex(
                name: "IX_Steps_UserId_StartTime",
                table: "Steps");

            migrationBuilder.DropIndex(
                name: "IX_SleepSessions_UserId_StartTime",
                table: "SleepSessions");

            migrationBuilder.DropIndex(
                name: "IX_Medications_UserId_StartTime",
                table: "Medications");

            migrationBuilder.DropIndex(
                name: "IX_HeartRates_UserId_StartTime",
                table: "HeartRates");

            migrationBuilder.DropIndex(
                name: "IX_GlucoseLevels_UserId_StartTime",
                table: "GlucoseLevels");

            migrationBuilder.AlterColumn<decimal>(
                name: "mmolL",
                table: "GlucoseLevels",
                type: "decimal(3,1)",
                precision: 3,
                scale: 1,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float(3)",
                oldPrecision: 3,
                oldScale: 1);
        }
    }
}
