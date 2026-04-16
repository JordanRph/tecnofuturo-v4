using ProtoBuf;
using TecnoFuturo.Core.Helpers;

namespace TecnoFuturo.Core.Entities;

[ProtoContract]
public class Alumno : Persona, IInfoDetallada
{
    [ProtoMember(6)] public int CentroId { get; set; }
    [ProtoMember(7)] public string CicloFormativoId { get; set; } = null!;

    public string ObtenerFicha()
    {
        return $"NOMBRE: {Nombre} " +
           $"NIF: {Nif} " +
           $"EMAIL: {Email} " +
           $"DIRECCION: {Direccion}" + 
           $"Telefono {Telefono}";
    }
}