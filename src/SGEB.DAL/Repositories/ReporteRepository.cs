using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SGEB.DAL.Entities;

namespace SGEB.DAL.Repositories
{
    public class ReporteRepository : IReporteRepository
    {
        private readonly string _connectionString;

        public ReporteRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SGEBConnection")
                ?? throw new InvalidOperationException(
                    "No se encontró la cadena de conexión 'SGEBConnection' en appsettings.json.");
        }

        public List<EstadisticaTipo> ObtenerEstadisticasPorTipo()
        {
            var lista = new List<EstadisticaTipo>();

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("dbo.sp_Reportes_EstadisticasPorTipo", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                int ordMin = reader.GetOrdinal("MinutosPromedioAtencion");
                lista.Add(new EstadisticaTipo
                {
                    TipoIncidente = reader.GetString(reader.GetOrdinal("TipoIncidente")),
                    TotalIncidentes = reader.GetInt32(reader.GetOrdinal("TotalIncidentes")),
                    Cerrados = reader.GetInt32(reader.GetOrdinal("Cerrados")),
                    SinAsignar = reader.GetInt32(reader.GetOrdinal("SinAsignar")),
                    MinutosPromedioAtencion = reader.IsDBNull(ordMin) ? null : reader.GetInt32(ordMin)
                });
            }
            return lista;
        }

        public List<EstadisticaZona> ObtenerEstadisticasPorZona()
        {
            var lista = new List<EstadisticaZona>();

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("dbo.sp_Reportes_EstadisticasPorZona", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new EstadisticaZona
                {
                    Zona = reader.GetString(reader.GetOrdinal("Zona")),
                    TotalIncidentes = reader.GetInt32(reader.GetOrdinal("TotalIncidentes")),
                    Cerrados = reader.GetInt32(reader.GetOrdinal("Cerrados"))
                });
            }
            return lista;
        }
    }
}