using TecnoFuturo.Core.Entities;
using TecnoFuturo.Core.DTOs;
namespace TecnoFuturo.Core.Repositories;

public interface IAlumnoRepository
{
    IReadOnlyList<AlumnoDTO> ObtenerAlumnos();
    IReadOnlyList<AlumnoDTO> ObtenerAlumnosPorCicloFormativo(string cicloFormativoId);
    IReadOnlyList<AlumnoDTO> ObtenerAlumnosPorCentro(int centroId);
    AlumnoDTO? ObtenerAlumnoPorNif(string nif);
    AlumnoDTO InsertarAlumno(Alumno alumno);
    AlumnoDTO ModificarAlumno(Alumno alumno);
    bool BorrarAlumno(string nif);
}