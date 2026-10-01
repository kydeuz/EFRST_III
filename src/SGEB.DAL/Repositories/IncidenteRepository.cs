using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SGEB.DAL.Entities;

namespace SGEB.DAL.Repositories
{

    /// Acceso a datos puro: solo abre conexión, ejecuta el SP y mapea filas.
    /// Nada de reglas de negocio aquí (eso vive en SGEB.BLL).
  
    public class IncidenteRepository : IIncidenteRepository
    {
        private readonly string _connectionString;

        public IncidenteRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SGEBConnection")
                ?? throw new InvalidOperationException(
                    "No se encontró la cadena de conexión 'SGEBConnection' en appsettings.json.");
        }

        public int Registrar(Incidente incidente)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("dbo.sp_Incidente_Registrar", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@IdReportante", incidente.IdReportante);
            cmd.Parameters.AddWithValue("@TipoIncidente", incidente.TipoIncidente);
            cmd.Parameters.AddWithValue("@Descripcion", incidente.Descripcion);
            cmd.Parameters.AddWithValue("@Direccion", incidente.Direccion);
            cmd.Parameters.AddWithValue("@Zona", incidente.Zona);
            cmd.Parameters.AddWithValue("@Latitud", (object?)incidente.Latitud ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Longitud", (object?)incidente.Longitud ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Prioridad", incidente.Prioridad);

            var idOutput = new SqlParameter("@IdIncidenteNuevo", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(idOutput);

            conn.Open();
            cmd.ExecuteNonQuery();

            return (int)idOutput.Value;
        }

        public List<Incidente> Listar()
        {
            var lista = new List<Incidente>();

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("dbo.sp_Incidente_Listar", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(MapearIncidente(reader));
            }

            return lista;
        }

        public Incidente? ObtenerPorId(int idIncidente)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("dbo.sp_Incidente_ObtenerPorId", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@IdIncidente", idIncidente);

            conn.Open();
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapearIncidente(reader) : null;
        }

        private static Incidente MapearIncidente(SqlDataReader reader)
        {
            return new Incidente
            {
                IdIncidente = reader.GetInt32(reader.GetOrdinal("IdIncidente")),
                IdReportante = reader.GetInt32(reader.GetOrdinal("IdReportante")),
                TipoIncidente = reader.GetString(reader.GetOrdinal("TipoIncidente")),
                Descripcion = reader.GetString(reader.GetOrdinal("Descripcion")),
                Direccion = reader.GetString(reader.GetOrdinal("Direccion")),
                Zona = reader.GetString(reader.GetOrdinal("Zona")),
                Latitud = reader.IsDBNull(reader.GetOrdinal("Latitud"))
                    ? null : reader.GetDecimal(reader.GetOrdinal("Latitud")),
                Longitud = reader.IsDBNull(reader.GetOrdinal("Longitud"))
                    ? null : reader.GetDecimal(reader.GetOrdinal("Longitud")),
                Prioridad = reader.GetString(reader.GetOrdinal("Prioridad")),
                Estado = reader.GetString(reader.GetOrdinal("Estado")),
                FechaHoraRegistro = reader.GetDateTime(reader.GetOrdinal("FechaHoraRegistro")),
                NombreReportante = reader.GetString(reader.GetOrdinal("NombreReportante")),
                TelefonoReportante = reader.IsDBNull(reader.GetOrdinal("TelefonoReportante"))
                    ? null : reader.GetString(reader.GetOrdinal("TelefonoReportante"))
            };
        }
    }
}