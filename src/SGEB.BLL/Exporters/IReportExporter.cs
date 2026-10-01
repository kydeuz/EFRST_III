namespace SGEB.BLL.Exporters
{
    public interface IReportExporter
    {
        string Formato { get; }        // "pdf", "excel", "csv"
        string ContentType { get; }
        string Extension { get; }
        byte[] Exportar(string titulo, string[] columnas, List<string[]> filas);
    }
}