using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SGEB.DAL.Entities;

namespace SGEB.DAL.Repositories
{
    public class AtencionCierreRepository : IAtencionCierreRepository
    {
        private readonly string _connectionString;

        public AtencionCierreRepository(IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString("SGEBConnection")
                ?? throw new InvalidOperationException(
                    "No se encontró la cadena de conexión " +
                    "'SGEBConnection' en appsettings.json.");
        }

        public int Registrar(AtencionCierre cierre)
        {
            using var conn = new SqlConnection(_connectionString);

            using var cmd = new SqlCommand(
                "dbo.sp_AtencionCierre_Registrar",
                conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue(
                "@IdIncidente",
                cierre.IdIncidente);

            cmd.Parameters.AddWithValue(
                "@ResultadoCierre",
                cierre.ResultadoCierre);

            cmd.Parameters.AddWithValue(
                "@Observaciones",
                (object?)cierre.Observaciones ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@PersonalInvolucrado",
                (object?)cierre.PersonalInvolucrado ?? DBNull.Value);

            var idOutput = new SqlParameter(
                "@IdCierreNuevo",
                SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };

            cmd.Parameters.Add(idOutput);

            conn.Open();
            cmd.ExecuteNonQuery();

            return (int)idOutput.Value;
        }

        public AtencionCierre? ObtenerPorIncidente(int idIncidente)
        {
            using var conn = new SqlConnection(_connectionString);

            using var cmd = new SqlCommand(
                "dbo.sp_AtencionCierre_ObtenerPorIncidente",
                conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue(
                "@IdIncidente",
                idIncidente);

            conn.Open();

            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return null;

            return MapearAtencionCierre(reader);
        }

        public void MarcarAtendido(int idIncidente)
        {
            using var conn = new SqlConnection(_connectionString);

            using var cmd = new SqlCommand(
                "dbo.sp_Incidente_MarcarAtendido",
                conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue(
                "@IdIncidente",
                idIncidente);

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        private static AtencionCierre MapearAtencionCierre(
            SqlDataReader reader)
        {
            return new AtencionCierre
            {
                IdCierre = reader.GetInt32(
                    reader.GetOrdinal("IdCierre")),

                IdIncidente = reader.GetInt32(
                    reader.GetOrdinal("IdIncidente")),

                FechaHoraCierre = reader.GetDateTime(
                    reader.GetOrdinal("FechaHoraCierre")),

                ResultadoCierre = reader.GetString(
                    reader.GetOrdinal("ResultadoCierre")),

                Observaciones = reader.IsDBNull(
                    reader.GetOrdinal("Observaciones"))
                    ? null
                    : reader.GetString(
                        reader.GetOrdinal("Observaciones")),

                PersonalInvolucrado = reader.IsDBNull(
                    reader.GetOrdinal("PersonalInvolucrado"))
                    ? null
                    : reader.GetInt32(
                        reader.GetOrdinal("PersonalInvolucrado"))
            };
        }
    }
}
