using Microsoft.Extensions.DependencyInjection;
using ProtoBuf;
using TecnoFuturo.Core.DTOs;
using TecnoFuturo.Core.Entities;
using TecnoFuturo.Core.Repositories;

namespace TecnoFuturo.Data.BinRepositories
{
    public class BinAlumnoRepository : IAlumnoRepository
    {
        private readonly IServiceProvider _serviceProvider;
        private Dictionary<string, Alumno> _alumnos = null!;
        private readonly string _ruta;

        public BinAlumnoRepository(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _ruta = Path.Combine(Directory.GetCurrentDirectory(), "alumnos.bin");
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
            using var stream = File.Create(_ruta);
            Serializer.Serialize(stream, _alumnos.Values);
        }
        private void Cargar()
        {
            if (!File.Exists(_ruta))
            {
                _alumnos = [];
                return;
            }

            using var stream = File.OpenRead(_ruta);
            var lista = Serializer.Deserialize<List<Alumno>>(stream);
            _alumnos = lista.ToDictionary(x => x.Nif);
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

