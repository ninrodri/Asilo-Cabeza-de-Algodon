using Asilo.Web.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Asilo.Web.Services
{
    public class ReciboPdfService
    {
        public byte[] GenerarRecibo(Cobro cobro)
        {
            var familiar =
                cobro.Paciente?.Familiares?.FirstOrDefault()?.NombreCompleto
                ?? cobro.Paciente?.FamiliarResponsable
                ?? "No registrado";

            var totalPagado =
                cobro.Pagos?.Sum(p => p.Monto) ?? 0;

            var saldo =
                cobro.Total - totalPagado;

            var documento = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);

                    page.Margin(40);

                    page.DefaultTextStyle(
                        x => x.FontSize(10)
                    );

                    page.Header()
                        .Column(column =>
                        {
                            column.Item()
                                .AlignCenter()
                                .Text("ASILO CABEZA DE ALGODÓN")
                                .Bold()
                                .FontSize(18);

                            column.Item()
                                .AlignCenter()
                                .Text("RECIBO DE COBRO")
                                .Bold()
                                .FontSize(14);

                            column.Item()
                                .PaddingTop(5)
                                .AlignCenter()
                                .Text($"No. {cobro.Id}");
                        });

                    page.Content()
                        .PaddingVertical(20)
                        .Column(column =>
                        {
                            column.Spacing(10);

                            // DATOS GENERALES
                            column.Item()
                                .Border(1)
                                .Padding(10)
                                .Column(datos =>
                                {
                                    datos.Spacing(5);

                                    datos.Item()
                                        .Text(text =>
                                        {
                                            text.Span("Paciente: ").Bold();
                                            text.Span(
                                                cobro.Paciente?.NombreCompleto
                                                ?? "No registrado"
                                            );
                                        });

                                    datos.Item()
                                        .Text(text =>
                                        {
                                            text.Span("Familiar responsable: ").Bold();
                                            text.Span(familiar);
                                        });

                                    datos.Item()
                                        .Text(text =>
                                        {
                                            text.Span("Fecha del cobro: ").Bold();
                                            text.Span(
                                                cobro.FechaCobro
                                                    .ToString("dd/MM/yyyy HH:mm")
                                            );
                                        });

                                    datos.Item()
                                        .Text(text =>
                                        {
                                            text.Span("Estado: ").Bold();
                                            text.Span(cobro.Estado);
                                        });
                                });

                            // DETALLE
                            column.Item()
                                .PaddingTop(10)
                                .Text("Detalle de cargos")
                                .Bold()
                                .FontSize(13);

                            column.Item()
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(5);
                                        columns.RelativeColumn(2);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell()
                                            .BorderBottom(1)
                                            .Padding(5)
                                            .Text("Tipo")
                                            .Bold();

                                        header.Cell()
                                            .BorderBottom(1)
                                            .Padding(5)
                                            .Text("Descripción")
                                            .Bold();

                                        header.Cell()
                                            .BorderBottom(1)
                                            .Padding(5)
                                            .AlignRight()
                                            .Text("Monto")
                                            .Bold();
                                    });

                                    foreach (var detalle in cobro.Detalles)
                                    {
                                        table.Cell()
                                            .Padding(5)
                                            .Text(detalle.Tipo);

                                        table.Cell()
                                            .Padding(5)
                                            .Text(detalle.Descripcion);

                                        table.Cell()
                                            .Padding(5)
                                            .AlignRight()
                                            .Text($"Q {detalle.Monto:N2}");
                                    }
                                });

                            // TOTALES
                            column.Item()
                                .PaddingTop(15)
                                .AlignRight()
                                .Column(totales =>
                                {
                                    totales.Spacing(5);

                                    totales.Item()
                                        .Text(
                                            $"Subtotal: Q {cobro.Subtotal:N2}"
                                        );

                                    totales.Item()
                                        .Text(
                                            $"Descuento Fundación: " +
                                            $"{cobro.PorcentajeDescuento:N2}%"
                                        );

                                    totales.Item()
                                        .Text(
                                            $"Monto descontado: " +
                                            $"Q {cobro.MontoDescuento:N2}"
                                        );

                                    totales.Item()
                                        .PaddingTop(5)
                                        .Text(
                                            $"TOTAL A PAGAR: Q {cobro.Total:N2}"
                                        )
                                        .Bold()
                                        .FontSize(13);

                                    totales.Item()
                                        .Text(
                                            $"Total pagado: Q {totalPagado:N2}"
                                        );

                                    totales.Item()
                                        .Text(
                                            $"Saldo pendiente: Q {saldo:N2}"
                                        )
                                        .Bold();
                                });

                            column.Item()
                                .PaddingTop(25)
                                .AlignCenter()
                                .Text(
                                    "Documento generado por el sistema " +
                                    "Asilo Cabeza de Algodón."
                                )
                                .FontSize(9);
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("Página ");
                            text.CurrentPageNumber();
                            text.Span(" de ");
                            text.TotalPages();
                        });
                });
            });

            return documento.GeneratePdf();
        }
    }
}