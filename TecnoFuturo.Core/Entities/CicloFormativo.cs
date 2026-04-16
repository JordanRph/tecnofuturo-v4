using ProtoBuf;

namespace TecnoFuturo.Core.Entities;

[ProtoContract]
public class CicloFormativo
{
    [ProtoMember(12)] public int CentroId { get; set; }
    [ProtoMember(13)] public string CicloFormativoId { get; set; } = null!;
    [ProtoMember(14)] public string Nombre { get; set; } = null!;
    [ProtoMember(15)] public Turno Turno { get; set; }
}