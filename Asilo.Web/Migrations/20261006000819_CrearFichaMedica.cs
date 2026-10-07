using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Asilo.Web.Migrations
{
    /// <inheritdoc />
    public partial class CrearFichaMedica : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FichasMedicas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PacienteId = table.Column<int>(type: "int", nullable: false),
                    Padecimientos = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Enfermedades = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Psicopatologias = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MedicamentosFrecuentes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Alergias = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ObservacionesMedicas = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FichasMedicas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FichasMedicas_Pacientes_PacienteId",
                        column: x => x.PacienteId,
                        principalTable: "Pacientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FichasMedicas_PacienteId",
                table: "FichasMedicas",
                column: "PacienteId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FichasMedicas");
        }
    }
}
