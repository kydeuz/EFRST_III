using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SGEB.DAL.Entities;

namespace SGEB.DAL.Repositories
{
    public class ReportanteRepository : IReportanteRepository
    {
        private readonly string _connectionString;

        public ReportanteRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SGEBConnection")
                ?? throw new InvalidOperationException(
                    "No se encontró la cadena de conexión 'SGEBConnection' en appsettings.json.");
        }

        public int Registrar(Reportante reportante)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("dbo.sp_Reportante_Registrar", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@TipoDocumento", reportante.TipoDocumento);
            cmd.Parameters.AddWithValue("@NumeroDocumento", reportante.NumeroDocumento);
            cmd.Parameters.AddWithValue("@Nombre", reportante.Nombre);
            cmd.Parameters.AddWithValue("@Telefono", (object?)reportante.Telefono ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Correo", (object?)reportante.Correo ?? DBNull.Value);

            var idOutput = new SqlParameter("@IdReportanteNuevo", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(idOutput);

            conn.Open();
            cmd.ExecuteNonQuery();

            return (int)idOutput.Value;
        }

        public Reportante? ObtenerPorDocumento(string tipoDocumento, string numeroDocumento)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("dbo.sp_Reportante_ObtenerPorDocumento", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@TipoDocumento", tipoDocumento);
            cmd.Parameters.AddWithValue("@NumeroDocumento", numeroDocumento);

            conn.Open();
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapearReportante(reader) : null;
        }

        public Reportante? ObtenerPorId(int idReportante)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("dbo.sp_Reportante_ObtenerPorId", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@IdReportante", idReportante);

            conn.Open();
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapearReportante(reader) : null;
        }

        public List<Reportante> Listar()
        {
            var lista = new List<Reportante>();

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("dbo.sp_Reportante_Listar", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(MapearReportante(reader));
            }

            return lista;
        }

        private static Reportante MapearReportante(SqlDataReader reader)
        {
            return new Reportante
            {
                IdReportante = reader.GetInt32(reader.GetOrdinal("IdReportante")),
                TipoDocumento = reader.GetString(reader.GetOrdinal("TipoDocumento")),
                NumeroDocumento = reader.GetString(reader.GetOrdinal("NumeroDocumento")),
                Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                Telefono = reader.IsDBNull(reader.GetOrdinal("Telefono"))
                    ? null : reader.GetString(reader.GetOrdinal("Telefono")),
                Correo = reader.IsDBNull(reader.GetOrdinal("Correo"))
                    ? null : reader.GetString(reader.GetOrdinal("Correo")),
                FechaRegistro = reader.GetDateTime(reader.GetOrdinal("FechaRegistro"))
            };
        }
    }
}