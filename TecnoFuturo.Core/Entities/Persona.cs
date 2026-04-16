using ProtoBuf;
namespace TecnoFuturo.Core.Entities;

[ProtoContract]
[ProtoInclude(100, typeof(Alumno))]
[ProtoInclude(101, typeof(Profesor))]
public abstract class Persona
{
    [ProtoMember(1)]public string Nif { get; set; } = null!;
    [ProtoMember(2)]public string Nombre { get; set; } = null!;
    [ProtoMember(3)] public string? Email { get; set; }
    [ProtoMember(4)] public string? Direccion { get; set; }
    [ProtoMember(5)] public string? Telefono { get; set; }
}