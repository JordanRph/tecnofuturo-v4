using TecnoFuturo.Core.Helpers;

namespace TecnoFuturo.Core.Entities;

public class Alumno : Persona, IInfoDetallada
{
    public int CentroId { get; set; }
    public string CicloFormativoId { get; set; } = null!;

    public string ObtenerFicha()
    {
        return $"NOMBRE: {Nombre} " +
           $"NIF: {Nif} " +
           $"EMAIL: {Email} " +
           $"DIRECCION: {Direccion}" + 
           $"Telefono {Telefono}";
    }
}