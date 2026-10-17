using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Asilo.Web.Migrations
{
    /// <inheritdoc />
    public partial class SimplificarConsultaGeneral : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Indicaciones",
                table: "ConsultasGenerales");

            migrationBuilder.DropColumn(
                name: "Tratamiento",
                table: "ConsultasGenerales");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Indicaciones",
                table: "ConsultasGenerales",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Tratamiento",
                table: "ConsultasGenerales",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
