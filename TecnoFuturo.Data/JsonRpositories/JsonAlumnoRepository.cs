using Microsoft.Extensions.DependencyInjection;
using TecnoFuturo.Core.DTOs;
using TecnoFuturo.Core.Entities;
using TecnoFuturo.Core.Repositories;
using System.Text.Json;

namespace TecnoFuturo.Data.JsonRpositories
{
    public class JsonAlumnoRepository : IAlumnoRepository
    {
        private readonly IServiceProvider _serviceProvider;
        private Dictionary<string, Alumno> _alumnos = null!;
        private readonly string _ruta;

        public JsonAlumnoRepository(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _ruta = Path.Combine(Directory.GetCurrentDirectory(), "alumnos.json");
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

            ValidarAlumno(alumno);

            _alumnos[alumno.Nif] = alumno;
            Guardar();

            return ToMap(alumno);
        }

        public AlumnoDTO ModificarAlumno(Alumno alumno)
        {
            if (!_alumnos.ContainsKey(alumno.Nif)) throw new ArgumentException("El alumno no existe");

            ValidarAlumno(alumno);

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
        private void Guardar()
        {

            var json = JsonSerializer.Serialize(_alumnos.Values, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(_ruta, json);
        }
        private void Cargar()
        {
            if (!File.Exists(_ruta))
            {
                _alumnos = [];
                return;
            }

            var json = File.ReadAllText(_ruta);
            var lista = JsonSerializer.Deserialize<List<Alumno>>(json);

            _alumnos = lista?.ToDictionary(x => x.Nif) ?? [];
        }
        private void ValidarAlumno(Alumno alumno)
        {
            var centroRepository = _serviceProvider.GetRequiredService<ICentroRepository>();
            var centro = centroRepository.ObtenerCentroPorId(alumno.CentroId)
                ?? throw new ArgumentException("El centro no existe");

            var cicloRepository = _serviceProvider.GetRequiredService<ICicloFormativoRepository>();
            var ciclo = cicloRepository.ObtenerCicloFormativoPorId(alumno.CicloFormativoId)
                ?? throw new ArgumentException("El ciclo no existe");

            if (ciclo.CentroId != alumno.CentroId)
                throw new ArgumentException("El ciclo no pertenece al centro");
        }
    }
}
