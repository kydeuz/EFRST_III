using Microsoft.Data.SqlClient;
using SGEB.DAL.Entities;
using SGEB.DAL.Repositories;
using Microsoft.Data.SqlClient;

namespace SGEB.BLL.Services
{
    /// Reglas de negocio de Unidad: validaciones, estados permitidos y
    /// traducción del error de clave duplicada de SQL Server a un mensaje claro.
    public class UnidadService : IUnidadService
    {
        private static readonly string[] EstadosValidos =
        {
            "Disponible", "En servicio", "Mantenimiento", "Fuera de servicio"
        };

        private readonly IUnidadRepository _unidadRepository;

        public UnidadService(IUnidadRepository unidadRepository)
        {
            _unidadRepository = unidadRepository;
        }

        public int RegistrarUnidad(Unidad unidad)
        {
            if (string.IsNullOrWhiteSpace(unidad.CodigoUnidad))
                throw new ArgumentException("El código de la unidad es obligatorio.");

            if (string.IsNullOrWhiteSpace(unidad.TipoUnidad))
                throw new ArgumentException("El tipo de la unidad es obligatorio.");

            unidad.CodigoUnidad = unidad.CodigoUnidad.Trim().ToUpper();
            unidad.TipoUnidad = unidad.TipoUnidad.Trim();
            unidad.Estado = "Disponible";
            unidad.FechaRegistro = DateTime.Now;

            try
            {
                return _unidadRepository.Registrar(unidad);
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                // 2627 / 2601 = clave duplicada (UQ_Unidad_CodigoUnidad)
                throw new InvalidOperationException("Ya existe una unidad con ese código.");
            }
        }

        public List<Unidad> ObtenerTodas() => _unidadRepository.Listar();

        public List<Unidad> ObtenerDisponibles() => _unidadRepository.ListarDisponibles();

        public Unidad? ObtenerPorId(int idUnidad) => _unidadRepository.ObtenerPorId(idUnidad);

        public void CambiarEstado(int idUnidad, string nuevoEstado)
        {
            try
            {
                _unidadRepository.ActualizarEstado(idUnidad, nuevoEstado);
            }
            catch (SqlException ex) when (ex.Number is 50020 or 50021 or 50022)
            {
                string mensaje;

                if (ex.Number == 50020)
                    mensaje = "Una unidad solo pasa a 'En servicio' al asignarla a un incidente.";
                else if (ex.Number == 50022)
                    mensaje = "La unidad no existe.";
                else
                    mensaje = "Estado de unidad inválido.";

                throw new ArgumentException(mensaje, ex);
            }
        }
    }
}
