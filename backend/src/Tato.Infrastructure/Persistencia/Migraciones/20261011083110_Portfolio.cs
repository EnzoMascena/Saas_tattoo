using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tato.Infrastructure.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class Portfolio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxFotosPorPublicacion",
                table: "Estudios",
                type: "int",
                nullable: false,
                defaultValue: 10);

            migrationBuilder.CreateTable(
                name: "Estilos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EstudioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estilos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Publicaciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EstudioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FechaTrabajo = table.Column<DateOnly>(type: "date", nullable: true),
                    EstiloId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ZonaCuerpoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TatuadorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Destacada = table.Column<bool>(type: "bit", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    ConsentimientoConfirmadoPor = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ConsentimientoConfirmadoEn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreadaPor = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreadaEn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ActualizadaEn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    PublicadaEn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Publicaciones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ZonasCuerpo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EstudioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZonasCuerpo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Fotos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EstudioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PublicacionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Ancho = table.Column<int>(type: "int", nullable: false),
                    Alto = table.Column<int>(type: "int", nullable: false),
                    SubidaEn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fotos_Publicaciones_PublicacionId",
                        column: x => x.PublicacionId,
                        principalTable: "Publicaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Estilos_EstudioId_Nombre",
                table: "Estilos",
                columns: new[] { "EstudioId", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fotos_PublicacionId",
                table: "Fotos",
                column: "PublicacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Publicaciones_EstudioId_Estado",
                table: "Publicaciones",
                columns: new[] { "EstudioId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_ZonasCuerpo_EstudioId_Nombre",
                table: "ZonasCuerpo",
                columns: new[] { "EstudioId", "Nombre" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Estilos");

            migrationBuilder.DropTable(
                name: "Fotos");

            migrationBuilder.DropTable(
                name: "ZonasCuerpo");

            migrationBuilder.DropTable(
                name: "Publicaciones");

            migrationBuilder.DropColumn(
                name: "MaxFotosPorPublicacion",
                table: "Estudios");
        }
    }
}
