using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Shoootz.Store.Adapter.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddClubEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Club", table: "Shooters");

            migrationBuilder.AddColumn<int>(name: "ClubId", table: "Shooters", type: "integer", nullable: true);

            migrationBuilder.CreateTable(
                name: "Clubs",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clubs", x => x.Id);
                }
            );

            migrationBuilder.CreateIndex(name: "IX_Shooters_ClubId", table: "Shooters", column: "ClubId");

            migrationBuilder.CreateIndex(name: "IX_Clubs_Name", table: "Clubs", column: "Name", unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Shooters_Clubs_ClubId",
                table: "Shooters",
                column: "ClubId",
                principalTable: "Clubs",
                principalColumn: "Id"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Shooters_Clubs_ClubId", table: "Shooters");

            migrationBuilder.DropTable(name: "Clubs");

            migrationBuilder.DropIndex(name: "IX_Shooters_ClubId", table: "Shooters");

            migrationBuilder.DropColumn(name: "ClubId", table: "Shooters");

            migrationBuilder.AddColumn<string>(
                name: "Club",
                table: "Shooters",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true
            );
        }
    }
}
