using ProtoBuf;
using TecnoFuturo.Core.Helpers;

namespace TecnoFuturo.Core.Entities;

[ProtoContract]
public class Centro : IInfoDetallada
{
    [ProtoMember(8)]public int CentroId { get; set; }
    [ProtoMember(9)] public string Nombre { get; set; } = null!;
    [ProtoMember(10)] public string? Direccion { get; set; }
    [ProtoMember(11)] public string? Telefono { get; set; }

    public string ObtenerFicha()
    {
        return $"CENTRO: {Nombre} " +
           $"ID: {CentroId} " +
           $"DIRECCION: {Direccion} " +
           $"TELEFONO: {Telefono}";
    }
}