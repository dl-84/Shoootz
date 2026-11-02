using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shoootz.Store.Adapter.SQLite.Migrations
{
    /// <inheritdoc />
    public partial class AddShotIntegrity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEdited",
                table: "Shots",
                type: "INTEGER",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<int>(
                name: "Source",
                table: "Shots",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1
            );

            migrationBuilder.CreateTable(
                name: "ShotChecksums",
                columns: table => new
                {
                    ShotId = table.Column<int>(type: "INTEGER", nullable: false),
                    Hash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShotChecksums", x => x.ShotId);
                }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ShotChecksums");

            migrationBuilder.DropColumn(name: "IsEdited", table: "Shots");

            migrationBuilder.DropColumn(name: "Source", table: "Shots");
        }
    }
}
