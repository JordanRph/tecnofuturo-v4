using TecnoFuturo.Core.Helpers;
namespace TecnoFuturo.Core.DTOs
{
    public record AlumnoDTO(string Nif, string Nombre, string Email, string Direccion, string Telefono) : IInfoDetallada
    {
        public string ObtenerFicha()
        {
            return $"NOMBRE: {Nombre} " +
               $"NIF: {Nif} " +
               $"EMAIL: {Email} " +
               $"DIRECCION: {Direccion}" +
               $"Telefono {Telefono}";
        }
    }
}
