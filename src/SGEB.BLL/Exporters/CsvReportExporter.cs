using System.Text;

namespace SGEB.BLL.Exporters
{
    public class CsvReportExporter : IReportExporter
    {
        public string Formato => "csv";
        public string ContentType => "text/csv";
        public string Extension => "csv";

        public byte[] Exportar(string titulo, string[] columnas, List<string[]> filas)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(",", columnas.Select(Escapar)));
            foreach (var fila in filas)
                sb.AppendLine(string.Join(",", fila.Select(Escapar)));

            // BOM para que Excel respete tildes y ñ
            return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        }

        private static string Escapar(string valor) => $"\"{valor.Replace("\"", "\"\"")}\"";
    }
}