using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportsCenter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingDurationLimitsToFacility : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxBookingDurationMinutes",
                table: "Facilities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinBookingDurationMinutes",
                table: "Facilities",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxBookingDurationMinutes",
                table: "Facilities");

            migrationBuilder.DropColumn(
                name: "MinBookingDurationMinutes",
                table: "Facilities");
        }
    }
}
