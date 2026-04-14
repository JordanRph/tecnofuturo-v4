using Microsoft.Extensions.DependencyInjection;
using TecnoFuturo.Core.Entities;
using TecnoFuturo.Core.Repositories;
using TecnoFuturo.Core.DTOs;

namespace TecnoFuturo.InMemory.Repositories;

public class CentroRepository : ICentroRepository
{
    private readonly Dictionary<int, Centro> _centros = [];
    private readonly IServiceProvider _serviceProvider;

    public CentroRepository(IServiceProvider serviceProvider)
    {
       _serviceProvider = serviceProvider;
    }
    
    public IReadOnlyList<CentroDTO> ObtenerCentros()
    {
        return _centros.Values.Select(x => ToMap(x)).ToList(); ;
    }

    public CentroDTO? ObtenerCentroPorId(int id)
    {
        var centro = _centros.GetValueOrDefault(id);
        return centro is null ? null : ToMap(centro);
    }

    public CentroDTO InsertarCentro(Centro centro)
    {
        if (!_centros.TryAdd(centro.CentroId, centro))
        {
            throw new InvalidOperationException("El centro ya existe");
        }

        return ToMap(centro);
    }

    public CentroDTO ModificarCentro(Centro centro)
    {
        if (!_centros.ContainsKey(centro.CentroId))
        {
            throw new InvalidOperationException("El centro no existe");
        }

        _centros[centro.CentroId] = centro;
        return ToMap(centro);
    }

    public bool BorrarCentro(int id)
    {
        if (!_centros.TryGetValue(id, out var centro)) return false;
        
        var cicloFormativoRepository = _serviceProvider.GetRequiredService<ICicloFormativoRepository>();
        var ciclosFormativos = cicloFormativoRepository.ObtenerCiclosFormativosPorCentro(id);
        if (ciclosFormativos.Count != 0)
        {
            throw new InvalidOperationException("El centro tiene ciclos formativos asociados");
        }
        
        var alumnoRepository = _serviceProvider.GetRequiredService<IAlumnoRepository>();
        var alumnos = alumnoRepository.ObtenerAlumnosPorCentro(id);
        if (alumnos.Count != 0)
        {
            throw new InvalidOperationException("El centro tiene alumnos asociados");
        }

        var profesorRepository = _serviceProvider.GetRequiredService<IProfesorRepository>();
        var profesores = profesorRepository.ObtenerProfesoresPorCentro(id);
        if (profesores.Count != 0)
        {
            throw new InvalidOperationException("El centro tiene profesores asociados");
        }
        
        return _centros.Remove(id);
    }
    private CentroDTO ToMap(Centro c)
    {
        return new CentroDTO
        (
            c.CentroId,
            c.Nombre,
            c.Direccion,
            c.Telefono
        );
    }
}