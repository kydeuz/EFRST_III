using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace SGEB.BLL.Exporters
{
    public class PdfReportExporter : IReportExporter
    {
        public string Formato => "pdf";
        public string ContentType => "application/pdf";
        public string Extension => "pdf";

        public byte[] Exportar(string titulo, string[] columnas, List<string[]> filas)
        {
            QuestPDF.Settings.License = LicenseType.Community; // sin esto lanza excepción

            return Document.Create(doc =>
            {
                doc.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.Header().Text(titulo).FontSize(16).Bold();
                    page.Content().PaddingTop(15).Table(t =>
                    {
                        t.ColumnsDefinition(c => { foreach (var _ in columnas) c.RelativeColumn(); });
                        t.Header(h => { foreach (var col in columnas) h.Cell().Text(col).Bold(); });
                        foreach (var fila in filas)
                            foreach (var celda in fila)
                                t.Cell().Text(celda);
                    });
                });
            }).GeneratePdf();
        }
    }
}