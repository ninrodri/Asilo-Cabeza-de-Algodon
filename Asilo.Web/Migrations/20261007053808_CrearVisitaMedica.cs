using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Asilo.Web.Migrations
{
    /// <inheritdoc />
    public partial class CrearVisitaMedica : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VisitasMedicas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudMedicaId = table.Column<int>(type: "int", nullable: false),
                    PacienteId = table.Column<int>(type: "int", nullable: false),
                    FechaVisita = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Medico = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Diagnostico = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tratamiento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Indicaciones = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitasMedicas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VisitasMedicas_Pacientes_PacienteId",
                        column: x => x.PacienteId,
                        principalTable: "Pacientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VisitasMedicas_SolicitudesMedicas_SolicitudMedicaId",
                        column: x => x.SolicitudMedicaId,
                        principalTable: "SolicitudesMedicas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VisitasMedicas_PacienteId",
                table: "VisitasMedicas",
                column: "PacienteId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitasMedicas_SolicitudMedicaId",
                table: "VisitasMedicas",
                column: "SolicitudMedicaId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VisitasMedicas");
        }
    }
}
