using ClosedXML.Excel;

namespace SGEB.BLL.Exporters
{
    public class ExcelReportExporter : IReportExporter
    {
        public string Formato => "excel";
        public string ContentType => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        public string Extension => "xlsx";

        public byte[] Exportar(string titulo, string[] columnas, List<string[]> filas)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Reporte");

            for (int c = 0; c < columnas.Length; c++)
                ws.Cell(1, c + 1).Value = columnas[c];
            ws.Row(1).Style.Font.Bold = true;

            for (int r = 0; r < filas.Count; r++)
                for (int c = 0; c < filas[r].Length; c++)
                    ws.Cell(r + 2, c + 1).Value = filas[r][c];

            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }
    }
}