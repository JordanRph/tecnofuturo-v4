using TecnoFuturo.Core.Helpers;

namespace TecnoFuturo.Core.DTOs
{
    public record CentroDTO(int CentroId, string Nombre, string? Direccion, string? Telefono) : IInfoDetallada
    {
        public string ObtenerFicha()
        {
            return $"CENTRO: {Nombre} " +
               $"ID: {CentroId} " +
               $"DIRECCION: {Direccion} " +
               $"TELEFONO: {Telefono}";
        }
    }
}
