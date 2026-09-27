using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shoootz.Store.Adapter.SQLite.Migrations
{
    /// <inheritdoc />
    public partial class AddShooterGender : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(name: "Gender", table: "Shooters", type: "INTEGER", nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Gender", table: "Shooters");
        }
    }
}
