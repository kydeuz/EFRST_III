using SGEB.DAL.Entities;

namespace SGEB.BLL.Services
{
    public interface IUnidadService
    {
        /// Registra una unidad nueva (siempre nace "Disponible").
        /// Lanza ArgumentException si faltan datos obligatorios e
        /// InvalidOperationException si el código de unidad ya existe.
        int RegistrarUnidad(Unidad unidad);

        List<Unidad> ObtenerTodas();
        List<Unidad> ObtenerDisponibles();
        Unidad? ObtenerPorId(int idUnidad);

        /// Cambia el estado. Estados válidos: Disponible, En servicio,
        /// Mantenimiento, Fuera de servicio.
        void CambiarEstado(int idUnidad, string nuevoEstado);
    }
}
