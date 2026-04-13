namespace TecnoFuturo.Core.Entities;

public abstract class Persona
{
    public string Nif { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
}