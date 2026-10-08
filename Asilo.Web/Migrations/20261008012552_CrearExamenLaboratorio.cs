using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Asilo.Web.Migrations
{
    /// <inheritdoc />
    public partial class CrearExamenLaboratorio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExamenesLaboratorio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PacienteId = table.Column<int>(type: "int", nullable: false),
                    VisitaMedicaId = table.Column<int>(type: "int", nullable: false),
                    NombreExamen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaSolicitud = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Resultado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaResultado = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamenesLaboratorio", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamenesLaboratorio_Pacientes_PacienteId",
                        column: x => x.PacienteId,
                        principalTable: "Pacientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamenesLaboratorio_VisitasMedicas_VisitaMedicaId",
                        column: x => x.VisitaMedicaId,
                        principalTable: "VisitasMedicas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExamenesLaboratorio_PacienteId",
                table: "ExamenesLaboratorio",
                column: "PacienteId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamenesLaboratorio_VisitaMedicaId",
                table: "ExamenesLaboratorio",
                column: "VisitaMedicaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExamenesLaboratorio");
        }
    }
}
