using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SGEB.DAL.Entities;

namespace SGEB.DAL.Repositories
{
    public class IncidenteUnidadRepository : IIncidenteUnidadRepository
    {
        private readonly string _connectionString;

        public IncidenteUnidadRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SGEBConnection")
                ?? throw new InvalidOperationException(
                    "No se encontró la cadena de conexión 'SGEBConnection' en appsettings.json.");
        }

        public int Asignar(int idIncidente, int idUnidad)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("dbo.sp_IncidenteUnidad_Asignar", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@IdIncidente", idIncidente);
            cmd.Parameters.AddWithValue("@IdUnidad", idUnidad);

            var idOutput = new SqlParameter("@IdIncidenteUnidadNueva", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(idOutput);

            conn.Open();
            cmd.ExecuteNonQuery();

            return (int)idOutput.Value;
        }

        public void Liberar(int idIncidenteUnidad)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("dbo.sp_IncidenteUnidad_Liberar", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@IdIncidenteUnidad", idIncidenteUnidad);

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public List<IncidenteUnidad> ListarPorIncidente(int idIncidente)
        {
            var lista = new List<IncidenteUnidad>();

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("dbo.sp_IncidenteUnidad_ListarPorIncidente", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@IdIncidente", idIncidente);

            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new IncidenteUnidad
                {
                    IdIncidenteUnidad = reader.GetInt32(reader.GetOrdinal("IdIncidenteUnidad")),
                    IdIncidente = reader.GetInt32(reader.GetOrdinal("IdIncidente")),
                    IdUnidad = reader.GetInt32(reader.GetOrdinal("IdUnidad")),
                    FechaHoraAsignacion = reader.GetDateTime(reader.GetOrdinal("FechaHoraAsignacion")),
                    FechaHoraLlegada = reader.IsDBNull(reader.GetOrdinal("FechaHoraLlegada"))
                        ? null : reader.GetDateTime(reader.GetOrdinal("FechaHoraLlegada")),
                    Estado = reader.GetString(reader.GetOrdinal("Estado")),
                    CodigoUnidad = reader.GetString(reader.GetOrdinal("CodigoUnidad")),
                    TipoUnidad = reader.GetString(reader.GetOrdinal("TipoUnidad"))
                });
            }

            return lista;
        }

        public List<UnidadDisponible> ListarUnidadesDisponibles()
        {
            var lista = new List<UnidadDisponible>();

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("dbo.sp_Unidad_ListarDisponibles", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new UnidadDisponible
                {
                    IdUnidad = reader.GetInt32(reader.GetOrdinal("IdUnidad")),
                    CodigoUnidad = reader.GetString(reader.GetOrdinal("CodigoUnidad")),
                    TipoUnidad = reader.GetString(reader.GetOrdinal("TipoUnidad"))
                });
            }

            return lista;
        }
    }
}