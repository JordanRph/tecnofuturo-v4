using Microsoft.Extensions.DependencyInjection;
using TecnoFuturo.Core.DTOs;
using TecnoFuturo.Core.Entities;
using TecnoFuturo.Core.Repositories;
using TecnoFuturo.Data.Helpers;

namespace TecnoFuturo.Data.JsonRpositories
{
    public class JsonAlumnoRepository : IAlumnoRepository
    {
        private readonly JsonHelper _jsonHelper;
        private readonly IServiceProvider _serviceProvider;
        private Dictionary<string, Alumno> _alumnos = null!;

        public JsonAlumnoRepository(JsonHelper jsonHelper, IServiceProvider serviceProvider)
        {
            _jsonHelper = jsonHelper;
            _serviceProvider = serviceProvider;
            Cargar();
        }

        public IReadOnlyList<AlumnoDTO> ObtenerAlumnos()
        {
            return [.. _alumnos.Values.Select(ToMap)];
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
            if (centroRepository.ObtenerCentroPorId(alumno.CentroId) == null)
            {
                throw new ArgumentException("El centro especificado no existe", nameof(alumno));
            }
            _alumnos[alumno.Nif] = alumno;
            Guardar();

            return ToMap(alumno);
        }

        public AlumnoDTO ModificarAlumno(Alumno alumno)
        {
            if (!_alumnos.ContainsKey(alumno.Nif))
            {
                throw new ArgumentException("El alumno no existe", nameof(alumno));
            }

            var centroRepository = _serviceProvider.GetRequiredService<ICentroRepository>();
            if (centroRepository.ObtenerCentroPorId(alumno.CentroId) == null)
            {
                throw new ArgumentException("El centro especificado no existe", nameof(alumno));
            }

            _alumnos[alumno.Nif] = alumno;
            Guardar();
            return ToMap(alumno);

        }

        public bool BorrarAlumno(string nif)
        {
            var eliminado = _alumnos.Remove(nif);

            if (eliminado)
                Guardar();

            return eliminado;
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

        private void Cargar()
        {
            var datos = _jsonHelper.LeerDatos<Alumno>("alumnos.json");
            _alumnos = datos == null ? [] : datos.ToDictionary(a => a.Nif);
        }

        private void Guardar()
        {
            _jsonHelper.Guardar("alumnos.json",_alumnos.Values);
        }

    }
}
