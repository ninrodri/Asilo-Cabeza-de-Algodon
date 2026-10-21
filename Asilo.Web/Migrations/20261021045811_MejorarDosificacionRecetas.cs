using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Asilo.Web.Migrations
{
    /// <inheritdoc />
    public partial class MejorarDosificacionRecetas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ContenidoPresentacion",
                table: "DetallesReceta",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DosisPorToma",
                table: "DetallesReceta",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "DuracionDias",
                table: "DetallesReceta",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FormaFarmaceutica",
                table: "DetallesReceta",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Presentacion",
                table: "DetallesReceta",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TomasPorDia",
                table: "DetallesReceta",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UnidadDosis",
                table: "DetallesReceta",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UnidadPresentacion",
                table: "DetallesReceta",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContenidoPresentacion",
                table: "DetallesReceta");

            migrationBuilder.DropColumn(
                name: "DosisPorToma",
                table: "DetallesReceta");

            migrationBuilder.DropColumn(
                name: "DuracionDias",
                table: "DetallesReceta");

            migrationBuilder.DropColumn(
                name: "FormaFarmaceutica",
                table: "DetallesReceta");

            migrationBuilder.DropColumn(
                name: "Presentacion",
                table: "DetallesReceta");

            migrationBuilder.DropColumn(
                name: "TomasPorDia",
                table: "DetallesReceta");

            migrationBuilder.DropColumn(
                name: "UnidadDosis",
                table: "DetallesReceta");

            migrationBuilder.DropColumn(
                name: "UnidadPresentacion",
                table: "DetallesReceta");
        }
    }
}
