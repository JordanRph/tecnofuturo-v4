using Microsoft.Extensions.DependencyInjection;
using TecnoFuturo.Core.Entities;
using TecnoFuturo.Core.Repositories;
using TecnoFuturo.Core.DTOs;
namespace TecnoFuturo.InMemory.Repositories;

public class ProfesorRepository : IProfesorRepository
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, Profesor> _profesores = [];
    
    public ProfesorRepository(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public IReadOnlyList<ProfesorDTO> ObtenerProfesores()
    {
        return [.. _profesores.Values.Select(ToMap)];
    }

    public IReadOnlyList<ProfesorDTO> ObtenerProfesoresPorCentro(int centroId)
    {
        return [.. _profesores.Values.Where(p => p.CentroId == centroId).Select(ToMap)];
    }

    public ProfesorDTO? ObtenerProfesorPorNif(string nif)
    {
        var profesor = _profesores.GetValueOrDefault(nif);
        return profesor == null ? null : ToMap(profesor);
    }

    public ProfesorDTO InsertarProfesor(Profesor profesor)
    {
        var centroRepository = _serviceProvider.GetRequiredService<ICentroRepository>();
        if (centroRepository.ObtenerCentroPorId(profesor.CentroId) == null)
        {
            throw new ArgumentException("El centro especificado no existe", nameof(profesor));
        }

        if (_profesores.ContainsKey(profesor.Nif))
        {
            throw new ArgumentException("El profesor ya existe", nameof(profesor));
        }

        _profesores[profesor.Nif] = profesor;
        return ToMap(profesor);
    }

    public ProfesorDTO ModificarProfesor(Profesor profesor)
    {
        var centroRepository = _serviceProvider.GetRequiredService<ICentroRepository>();
        if (centroRepository.ObtenerCentroPorId(profesor.CentroId) == null)
        {
            throw new ArgumentException("El centro especificado no existe", nameof(profesor));
        }

        if (!_profesores.ContainsKey(profesor.Nif))
        {
            throw new ArgumentException("El profesor no existe", nameof(profesor));
        }

        _profesores[profesor.Nif] = profesor;
        return ToMap(profesor);
    }

    public bool BorrarProfesor(string nif)
    {
        var profesor = _profesores.GetValueOrDefault(nif);

        if (profesor == null)
        {
            throw new ArgumentException("El profesor no existe", nameof(nif));
        }

        var moduloRepository = _serviceProvider.GetRequiredService<IModuloRepository>();
        var modulosPorProfesor = moduloRepository.ObtenerModulosPorProfesor(nif);
        return modulosPorProfesor.Count != 0
            ? throw new InvalidOperationException("El profesor tiene modulos asignados")
            : _profesores.Remove(nif);
    }
    private ProfesorDTO ToMap(Profesor p)
    {
        return new ProfesorDTO(
            p.Nif,
            p.Nombre,
            p.Email,
            p.Direccion ?? string.Empty,
            p.Telefono ?? string.Empty);
    }
}