using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Asilo.Web.Migrations
{
    /// <inheritdoc />
    public partial class CrearRelacionCobroVisitas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CobrosVisitas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CobroId = table.Column<int>(type: "int", nullable: false),
                    VisitaMedicaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CobrosVisitas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CobrosVisitas_Cobros_CobroId",
                        column: x => x.CobroId,
                        principalTable: "Cobros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CobrosVisitas_VisitasMedicas_VisitaMedicaId",
                        column: x => x.VisitaMedicaId,
                        principalTable: "VisitasMedicas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CobrosVisitas_CobroId",
                table: "CobrosVisitas",
                column: "CobroId");

            migrationBuilder.CreateIndex(
                name: "IX_CobrosVisitas_VisitaMedicaId",
                table: "CobrosVisitas",
                column: "VisitaMedicaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CobrosVisitas");
        }
    }
}
