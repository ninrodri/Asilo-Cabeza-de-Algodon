using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Asilo.Web.Migrations
{
    /// <inheritdoc />
    public partial class AgregarEspecialidadUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Especialidad",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Especialidad",
                table: "Usuarios");
        }
    }
}
