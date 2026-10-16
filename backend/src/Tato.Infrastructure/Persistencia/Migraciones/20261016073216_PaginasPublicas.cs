using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tato.Infrastructure.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class PaginasPublicas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PaginasPublicas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EstudioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TatuadorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Direccion = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Bio = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Instagram = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    WhatsApp = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    Sobre = table.Column<string>(type: "nvarchar(2200)", maxLength: 2200, nullable: true),
                    Ubicacion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Horarios = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Visible = table.Column<bool>(type: "bit", nullable: false),
                    CreadaEn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ActualizadaEn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaginasPublicas", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaginasPublicas_Direccion",
                table: "PaginasPublicas",
                column: "Direccion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaginasPublicas_EstudioId",
                table: "PaginasPublicas",
                column: "EstudioId",
                unique: true,
                filter: "[TatuadorId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PaginasPublicas_TatuadorId",
                table: "PaginasPublicas",
                column: "TatuadorId",
                unique: true,
                filter: "[TatuadorId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaginasPublicas");
        }
    }
}
