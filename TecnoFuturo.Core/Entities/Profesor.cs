using ProtoBuf;
using TecnoFuturo.Core.Helpers;

namespace TecnoFuturo.Core.Entities;

[ProtoContract]
public class Profesor : Persona, IInfoDetallada
{
    [ProtoMember(21)] public int CentroId { get; set; }

    public string ObtenerFicha()
    {
        return $"NOMBRE: {Nombre} " +
            $"NIF: {Nif} " +
            $"EMAIL: {Email}" +
            $"DIRECCIÓN: {Direccion}"+
            $"TELÉFONO: {Telefono}";
    }
}