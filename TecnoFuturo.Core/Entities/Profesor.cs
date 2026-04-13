using TecnoFuturo.Core.Helpers;

namespace TecnoFuturo.Core.Entities;

public class Profesor : Persona, IInfoDetallada
{
    public int CentroId { get; set; }

    public string ObtenerFicha()
    {
        return $"NOMBRE: {Nombre} " +
            $"NIF: {Nif} " +
            $"EMAIL : {Email}";
    }
}