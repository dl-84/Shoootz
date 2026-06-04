using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Shoootz.Store.Adapter.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Shooters",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Birthyear = table.Column<int>(type: "integer", nullable: false),
                    Club = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Firstname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Lastname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Startnumber = table.Column<int>(type: "integer", nullable: false),
                    Team = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shooters", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "ShotInfos",
                columns: table => new
                {
                    MenuId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MenuItemName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MenuPointName = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShotInfos", x => x.MenuId);
                }
            );

            migrationBuilder.CreateTable(
                name: "Shots",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    Count = table.Column<int>(type: "integer", nullable: false),
                    DecValue = table.Column<double>(type: "double precision", nullable: false),
                    DiscType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Distance = table.Column<double>(type: "double precision", nullable: false),
                    IsHot = table.Column<bool>(type: "boolean", nullable: false),
                    IsValid = table.Column<bool>(type: "boolean", nullable: false),
                    ShotDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ShooterId = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    ShotInfoMenuId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shots_Shooters_ShooterId",
                        column: x => x.ShooterId,
                        principalTable: "Shooters",
                        principalColumn: "Id"
                    );
                    table.ForeignKey(
                        name: "FK_Shots_ShotInfos_ShotInfoMenuId",
                        column: x => x.ShotInfoMenuId,
                        principalTable: "ShotInfos",
                        principalColumn: "MenuId"
                    );
                }
            );

            migrationBuilder.CreateIndex(name: "IX_Shots_ShooterId", table: "Shots", column: "ShooterId");

            migrationBuilder.CreateIndex(name: "IX_Shots_ShotInfoMenuId", table: "Shots", column: "ShotInfoMenuId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Shots");

            migrationBuilder.DropTable(name: "Shooters");

            migrationBuilder.DropTable(name: "ShotInfos");
        }
    }
}
