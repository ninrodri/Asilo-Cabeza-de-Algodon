using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Asilo.Web.Migrations
{
    /// <inheritdoc />
    public partial class ActualizarFlujoSolicitudFundacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MedicoReferido",
                table: "SolicitudesMedicas");

            migrationBuilder.AlterColumn<string>(
                name: "Especialidad",
                table: "SolicitudesMedicas",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "EnfermeroId",
                table: "SolicitudesMedicas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAtencion",
                table: "SolicitudesMedicas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MedicoEspecialistaId",
                table: "SolicitudesMedicas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesMedicas_EnfermeroId",
                table: "SolicitudesMedicas",
                column: "EnfermeroId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesMedicas_MedicoEspecialistaId",
                table: "SolicitudesMedicas",
                column: "MedicoEspecialistaId");

            migrationBuilder.AddForeignKey(
                name: "FK_SolicitudesMedicas_Usuarios_EnfermeroId",
                table: "SolicitudesMedicas",
                column: "EnfermeroId",
                principalTable: "Usuarios",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SolicitudesMedicas_Usuarios_MedicoEspecialistaId",
                table: "SolicitudesMedicas",
                column: "MedicoEspecialistaId",
                principalTable: "Usuarios",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SolicitudesMedicas_Usuarios_EnfermeroId",
                table: "SolicitudesMedicas");

            migrationBuilder.DropForeignKey(
                name: "FK_SolicitudesMedicas_Usuarios_MedicoEspecialistaId",
                table: "SolicitudesMedicas");

            migrationBuilder.DropIndex(
                name: "IX_SolicitudesMedicas_EnfermeroId",
                table: "SolicitudesMedicas");

            migrationBuilder.DropIndex(
                name: "IX_SolicitudesMedicas_MedicoEspecialistaId",
                table: "SolicitudesMedicas");

            migrationBuilder.DropColumn(
                name: "EnfermeroId",
                table: "SolicitudesMedicas");

            migrationBuilder.DropColumn(
                name: "FechaAtencion",
                table: "SolicitudesMedicas");

            migrationBuilder.DropColumn(
                name: "MedicoEspecialistaId",
                table: "SolicitudesMedicas");

            migrationBuilder.AlterColumn<string>(
                name: "Especialidad",
                table: "SolicitudesMedicas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MedicoReferido",
                table: "SolicitudesMedicas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
