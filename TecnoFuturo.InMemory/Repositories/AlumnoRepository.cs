using Microsoft.Extensions.DependencyInjection;
using TecnoFuturo.Core;
using TecnoFuturo.Core.DTOs;
using TecnoFuturo.Core.Entities;
using TecnoFuturo.Core.Repositories;


namespace TecnoFuturo.InMemory.Repositories;

public class AlumnoRepository : IAlumnoRepository
{

    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, Alumno> _alumnos = [];

    public AlumnoRepository(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IReadOnlyList<AlumnoDTO> ObtenerAlumnos()
    {
        return [.. _alumnos.Values.Select(x => ToMap(x))];
    }

    public IReadOnlyList<AlumnoDTO> ObtenerAlumnosPorCicloFormativo(string cicloFormativoId)
    {
        return [.. _alumnos.Values.Where(a => a.CicloFormativoId == cicloFormativoId).Select(ToMap)];
    }

    public IReadOnlyList<AlumnoDTO> ObtenerAlumnosPorCentro(int centroId)
    {
        return [.. _alumnos.Values.Where(a => a.CentroId == centroId).Select(ToMap)];
    }

    public AlumnoDTO? ObtenerAlumnoPorNif(string nif)
    {
        return _alumnos.TryGetValue(nif, out var alumno) ? ToMap(alumno) : null;
    }

    public AlumnoDTO InsertarAlumno(Alumno alumno)
    {
        if (_alumnos.ContainsKey(alumno.Nif))
        {
            throw new ArgumentException("El alumno ya existe", nameof(alumno));
        }

        var centroRepository = _serviceProvider.GetRequiredService<ICentroRepository>();
        var centro = centroRepository.ObtenerCentroPorId(alumno.CentroId) ?? throw new ArgumentException("El centro especificado no existe", nameof(alumno));
        var cicloRepository = _serviceProvider.GetRequiredService<ICicloFormativoRepository>();
        var ciclo = cicloRepository.ObtenerCicloFormativoPorId(alumno.CicloFormativoId) ?? throw new ArgumentException("El ciclo formativo especificado no existe", nameof(alumno));
        if (ciclo.CentroId != alumno.CentroId)
        {
            throw new ArgumentException("El ciclo formativo no pertenece al centro del alumno", nameof(alumno));
        }

        _alumnos[alumno.Nif] = alumno;
        return ToMap(alumno);
    }

    public AlumnoDTO ModificarAlumno(Alumno alumno)
    {
        if (!_alumnos.ContainsKey(alumno.Nif))
        {
            throw new ArgumentException("El alumno no existe", nameof(alumno));
        }

        var centroRepository = _serviceProvider.GetRequiredService<ICentroRepository>();
        var centro = centroRepository.ObtenerCentroPorId(alumno.CentroId) ?? throw new ArgumentException("El centro especificado no existe", nameof(alumno));
        var cicloRepository = _serviceProvider.GetRequiredService<ICicloFormativoRepository>();
        var ciclo = cicloRepository.ObtenerCicloFormativoPorId(alumno.CicloFormativoId) ?? throw new ArgumentException("El ciclo formativo especificado no existe", nameof(alumno));
        if (ciclo.CentroId != alumno.CentroId)
        {
            throw new ArgumentException("El ciclo formativo no pertenece al centro del alumno", nameof(alumno));
        }

        _alumnos[alumno.Nif] = alumno;
        return ToMap(alumno);
    }

    public bool BorrarAlumno(string nif)
    {
        if (!_alumnos.ContainsKey(nif))
        {
            return false;
        }
        return _alumnos.Remove(nif);
    }
    private AlumnoDTO ToMap(Alumno a)
    {
        return new AlumnoDTO
        (
             a.Nif,
             a.Nombre,
             a.Email,
             a.Direccion ?? string.Empty,
             a.Telefono ?? string.Empty
        );
    }
}