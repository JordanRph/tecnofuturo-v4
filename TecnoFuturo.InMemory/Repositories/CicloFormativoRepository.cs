using Microsoft.Extensions.DependencyInjection;
using TecnoFuturo.Core.Entities;
using TecnoFuturo.Core.Repositories;
using TecnoFuturo.Core.DTOs;
namespace TecnoFuturo.InMemory.Repositories;

public class CicloFormativoRepository : ICicloFormativoRepository
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, CicloFormativo> _ciclosFormativos = [];
    
    public CicloFormativoRepository(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public IReadOnlyList<CicloFormativoDTO> ObtenerCiclosFormativos()
    {
        return [.. _ciclosFormativos.Values.Select(ToMap)];
    }

    public IReadOnlyList<CicloFormativoDTO> ObtenerCiclosFormativosPorCentro(int centroId)
    {
        return [.. _ciclosFormativos.Values.Where(c => c.CentroId == centroId).Select(ToMap)];
    }

    public CicloFormativoDTO? ObtenerCicloFormativoPorId(string id)
    {
        return _ciclosFormativos.TryGetValue(id, out var c) ? ToMap(c) : null;
    }

    public CicloFormativoDTO InsertarCicloFormativo(CicloFormativo cicloFormativo)
    {
        var centroRepository = _serviceProvider.GetRequiredService<ICentroRepository>();
        var centro = centroRepository.ObtenerCentroPorId(cicloFormativo.CentroId);
        if (centro == null)
        {
            throw new ArgumentException("El centro especificado no existe", nameof(cicloFormativo));
        }
        
        if (_ciclosFormativos.TryAdd(cicloFormativo.CicloFormativoId, cicloFormativo))
        {
            return ToMap(cicloFormativo);
        }

        throw new InvalidOperationException("El ciclo formativo ya existe");
    }

    public CicloFormativoDTO ModificarCicloFormativo(CicloFormativo cicloFormativo)
    {
        var centroRepository = _serviceProvider.GetRequiredService<ICentroRepository>();
        var centro = centroRepository.ObtenerCentroPorId(cicloFormativo.CentroId);
        if (centro == null)
        {
            throw new ArgumentException("El centro especificado no existe", nameof(cicloFormativo));
        }
        
        if (!_ciclosFormativos.ContainsKey(cicloFormativo.CicloFormativoId))
        {
            throw new ArgumentException("El ciclo formativo no existe", nameof(cicloFormativo));
        }
        
        _ciclosFormativos[cicloFormativo.CicloFormativoId] = cicloFormativo;
        return ToMap(cicloFormativo);
    }

    public bool BorrarCicloFormativo(string id)
    {
        if (!_ciclosFormativos.TryGetValue(id, out var cicloFormativo)) return false;
        
        var centroRepository = _serviceProvider.GetRequiredService<ICentroRepository>();
        var centro = centroRepository.ObtenerCentroPorId(cicloFormativo.CentroId);
        if (centro == null)
        {
            throw new ArgumentException("El centro especificado no existe");
        }

        var moduloRepository = _serviceProvider.GetRequiredService<IModuloRepository>();
        var modulos = moduloRepository.ObtenerModulosPorCicloFormativo(id);
        
        if (modulos.Count != 0)
        {
            throw new InvalidOperationException("El ciclo formativo tiene modulos asociados");
        }
        
        return _ciclosFormativos.Remove(id);
    }
    private CicloFormativoDTO ToMap(CicloFormativo c)
    {
        return new CicloFormativoDTO(
            c.CentroId,
            c.CicloFormativoId,
            c.Nombre,
            c.Turno);
    }
}