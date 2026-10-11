using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SGEB.DAL.Entities;

namespace SGEB.DAL.Repositories
{
    /// Acceso a datos puro: solo abre conexión, ejecuta el SP y mapea filas.
    /// Nada de reglas de negocio aquí (eso vive en SGEB.BLL).
    /// Ojo: si el código de unidad se repite, SQL Server lanza SqlException
    /// (2627 o 2601). Aquí NO se captura; lo hace UnidadService en el BLL.
    public class UnidadRepository : IUnidadRepository
    {
        private readonly string _connectionString;

        public UnidadRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SGEBConnection")
                ?? throw new InvalidOperationException(
                    "No se encontró la cadena de conexión 'SGEBConnection' en appsettings.json.");
        }

        public int Registrar(Unidad unidad)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("dbo.sp_Unidad_Registrar", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@CodigoUnidad", unidad.CodigoUnidad);
            cmd.Parameters.AddWithValue("@TipoUnidad", unidad.TipoUnidad);
            cmd.Parameters.AddWithValue("@Placa", (object?)unidad.Placa ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Capacidad", (object?)unidad.Capacidad ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Estacion", (object?)unidad.Estacion ?? DBNull.Value);

            var idOutput = new SqlParameter("@IdUnidadNueva", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(idOutput);

            conn.Open();
            cmd.ExecuteNonQuery();

            return (int)idOutput.Value;
        }

        public List<Unidad> Listar()
        {
            return EjecutarListado("dbo.sp_Unidad_Listar");
        }

        public List<Unidad> ListarDisponibles()
        {
            return EjecutarListado("dbo.sp_Unidad_ListarDisponibles");
        }

        public Unidad? ObtenerPorId(int idUnidad)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("dbo.sp_Unidad_ObtenerPorId", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@IdUnidad", idUnidad);

            conn.Open();
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapearUnidad(reader) : null;
        }

        public void ActualizarEstado(int idUnidad, string nuevoEstado)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("dbo.sp_Unidad_ActualizarEstado", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@IdUnidad", idUnidad);
            cmd.Parameters.AddWithValue("@NuevoEstado", nuevoEstado);

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        // Listar y ListarDisponibles devuelven las mismas columnas,
        // por eso comparten este método.
        private List<Unidad> EjecutarListado(string nombreSp)
        {
            var lista = new List<Unidad>();

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(nombreSp, conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(MapearUnidad(reader));
            }

            return lista;
        }

        private static Unidad MapearUnidad(SqlDataReader reader)
        {
            return new Unidad
            {
                IdUnidad = reader.GetInt32(reader.GetOrdinal("IdUnidad")),
                CodigoUnidad = reader.GetString(reader.GetOrdinal("CodigoUnidad")),
                TipoUnidad = reader.GetString(reader.GetOrdinal("TipoUnidad")),
                Placa = reader.IsDBNull(reader.GetOrdinal("Placa"))
                    ? null : reader.GetString(reader.GetOrdinal("Placa")),
                Capacidad = reader.IsDBNull(reader.GetOrdinal("Capacidad"))
                    ? null : reader.GetInt32(reader.GetOrdinal("Capacidad")),
                Estacion = reader.IsDBNull(reader.GetOrdinal("Estacion"))
                    ? null : reader.GetString(reader.GetOrdinal("Estacion")),
                Estado = reader.GetString(reader.GetOrdinal("Estado")),
                FechaRegistro = reader.GetDateTime(reader.GetOrdinal("FechaRegistro"))
            };
        }
    }
}
