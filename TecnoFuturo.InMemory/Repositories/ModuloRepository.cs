using Microsoft.Extensions.DependencyInjection;
using TecnoFuturo.Core.Entities;
using TecnoFuturo.Core.Repositories;
using TecnoFuturo.Core.DTOs;
namespace TecnoFuturo.InMemory.Repositories;

public class ModuloRepository : IModuloRepository
{
    private IServiceProvider _serviceProvider;
    private readonly Dictionary<int, Modulo> _modulos = [];
    
    public ModuloRepository(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public IReadOnlyList<ModuloDTO> ObtenerModulos()
    {
        return [.. _modulos.Values.Select(ToMap)];
    }

    public IReadOnlyList<ModuloDTO> ObtenerModulosPorCicloFormativo(string cicloFormativoId)
    {
        return [.. _modulos.Values.Where(m => m.CicloFormativoId == cicloFormativoId).Select(ToMap)];
    }

    public IReadOnlyList<ModuloDTO> ObtenerModulosPorProfesor(string profesorNif)
    {
        return [.. _modulos.Values.Where(m => m.ProfesorNif == profesorNif).Select(ToMap)];
    }

    public ModuloDTO? ObtenerModuloPorId(int id)
    {
        return _modulos.TryGetValue(id, out var m) ? ToMap(m) : null;
    }

    public ModuloDTO InsertarModulo(Modulo modulo)
    {
        var cicloFormativoRepository = _serviceProvider.GetRequiredService<ICicloFormativoRepository>();
        var cicloFormativo = cicloFormativoRepository.ObtenerCicloFormativoPorId(modulo.CicloFormativoId);

        return cicloFormativo == null
            ? throw new ArgumentException("El ciclo formativo no existe", nameof(modulo.CicloFormativoId))
            : _modulos.TryAdd(modulo.ModuloId, modulo)
            ? ToMap(modulo)
            : throw new InvalidOperationException("El modulo ya existe");
    }

    public ModuloDTO ModificarModulo(Modulo modulo)
    {
        var cicloFormativoRepository = _serviceProvider.GetRequiredService<ICicloFormativoRepository>();
        var cicloFormativo = cicloFormativoRepository.ObtenerCicloFormativoPorId(modulo.CicloFormativoId) ?? throw new ArgumentException("El ciclo formativo no existe", nameof(modulo.CicloFormativoId));
        if (!_modulos.ContainsKey(modulo.ModuloId))
        {
            throw new ArgumentException("El modulo no existe", nameof(modulo.ModuloId));
        }
        _modulos[modulo.ModuloId] = modulo;
        return ToMap(modulo);
    }

    public bool BorrarModulo(int id)
    {
        if (_modulos.TryGetValue(id, out var modulo))
        {
            var cicloFormativoRepository = _serviceProvider.GetRequiredService<ICicloFormativoRepository>();
            var cicloFormativo = cicloFormativoRepository.ObtenerCicloFormativoPorId(modulo.CicloFormativoId);
        
            if (cicloFormativo == null)
            {
                throw new ArgumentException("El ciclo formativo no existe", nameof(modulo.CicloFormativoId));
            }
            
            return _modulos.Remove(id);
        }
        return false;
    }
    private ModuloDTO ToMap(Modulo m)
    {
        return new ModuloDTO(
            m.CicloFormativoId,
            m.ModuloId,
            m.Nombre,
            m.Horas,
            m.ProfesorNif ?? string.Empty);
    }
}