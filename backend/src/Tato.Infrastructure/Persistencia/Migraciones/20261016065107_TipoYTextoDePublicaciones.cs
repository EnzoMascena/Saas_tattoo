using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tato.Infrastructure.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class TipoYTextoDePublicaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Descripcion",
                table: "Publicaciones",
                type: "nvarchar(2200)",
                maxLength: 2200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Tipo",
                table: "Publicaciones",
                type: "int",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "Publicaciones");

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion",
                table: "Publicaciones",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2200)",
                oldMaxLength: 2200,
                oldNullable: true);
        }
    }
}
