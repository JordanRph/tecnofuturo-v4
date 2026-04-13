using TecnoFuturo.Core.Helpers;

namespace TecnoFuturo.Core.Entities;

public class Alumno : Persona, IInfoDetallada
{
    public int CentroId { get; set; }
    public string CicloFormativoId { get; set; } = null!;

    public string ObtenerFicha()
    {
        throw new NotImplementedException();
    }
}