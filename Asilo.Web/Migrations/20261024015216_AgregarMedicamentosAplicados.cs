using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Asilo.Web.Migrations
{
    /// <inheritdoc />
    public partial class AgregarMedicamentosAplicados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MedicamentosAplicados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VisitaMedicaId = table.Column<int>(type: "int", nullable: false),
                    Medicamento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CantidadAplicada = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TiempoAplicacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaAplicacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegistradoPorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicamentosAplicados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicamentosAplicados_Usuarios_RegistradoPorId",
                        column: x => x.RegistradoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MedicamentosAplicados_VisitasMedicas_VisitaMedicaId",
                        column: x => x.VisitaMedicaId,
                        principalTable: "VisitasMedicas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MedicamentosAplicados_RegistradoPorId",
                table: "MedicamentosAplicados",
                column: "RegistradoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicamentosAplicados_VisitaMedicaId",
                table: "MedicamentosAplicados",
                column: "VisitaMedicaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MedicamentosAplicados");
        }
    }
}
