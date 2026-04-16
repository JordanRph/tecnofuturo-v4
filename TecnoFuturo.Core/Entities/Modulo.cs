using ProtoBuf;
using TecnoFuturo.Core.Helpers;

namespace TecnoFuturo.Core.Entities;

[ProtoContract]
public class Modulo : IInfoDetallada
{
    [ProtoMember(16)] public string CicloFormativoId { get; set; } = null!;
    [ProtoMember(17)] public int ModuloId { get; set; }
    [ProtoMember(18)] public string? Nombre { get; set; }
    [ProtoMember(19)] public int Horas { get; set; }
    [ProtoMember(20)] public string? ProfesorNif { get; set; }

    public string ObtenerFicha()
    {
        return $"MODULO: {Nombre} " +
           $"CODIGO: {ModuloId} " +
           $"HORAS: {Horas} " +
           $"PROFESOR: {ProfesorNif}";
    }
}