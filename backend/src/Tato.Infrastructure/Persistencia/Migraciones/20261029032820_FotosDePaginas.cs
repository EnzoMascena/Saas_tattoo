using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tato.Infrastructure.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class FotosDePaginas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FotoSobre_Alto",
                table: "PaginasPublicas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FotoSobre_Ancho",
                table: "PaginasPublicas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FotoSobre_Id",
                table: "PaginasPublicas",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Foto_Alto",
                table: "PaginasPublicas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Foto_Ancho",
                table: "PaginasPublicas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Foto_Id",
                table: "PaginasPublicas",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FotoSobre_Alto",
                table: "PaginasPublicas");

            migrationBuilder.DropColumn(
                name: "FotoSobre_Ancho",
                table: "PaginasPublicas");

            migrationBuilder.DropColumn(
                name: "FotoSobre_Id",
                table: "PaginasPublicas");

            migrationBuilder.DropColumn(
                name: "Foto_Alto",
                table: "PaginasPublicas");

            migrationBuilder.DropColumn(
                name: "Foto_Ancho",
                table: "PaginasPublicas");

            migrationBuilder.DropColumn(
                name: "Foto_Id",
                table: "PaginasPublicas");
        }
    }
}
