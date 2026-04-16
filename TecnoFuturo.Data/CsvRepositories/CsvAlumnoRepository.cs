
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using TecnoFuturo.Core.DTOs;
using TecnoFuturo.Core.Entities;
using TecnoFuturo.Core.Repositories;

namespace TecnoFuturo.Data.CsvRepositories
{
    public class CsvAlumnoRepository : IAlumnoRepository
    {
        private readonly IServiceProvider _serviceProvider;
        private Dictionary<string, Alumno> _alumnos = null!;
        private readonly string _ruta;

        public CsvAlumnoRepository(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _ruta = Path.Combine(Directory.GetCurrentDirectory(), "alumnos.csv");
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
            using var writer = new StreamWriter(_ruta);
            
                foreach (var a in _alumnos.Values)
                {
                    var linea = $"{a.Nif};{a.Nombre};{a.Email ?? ""};{a.Direccion ?? ""};{a.Telefono ?? ""};{a.CentroId};{a.CicloFormativoId}";
                    writer.WriteLine(linea);
                }
            
        }
        private void Cargar()
        {
            _alumnos = [];

            if (!File.Exists(_ruta))
                return;

            using var reader = new StreamReader(_ruta);
            string? linea;
            int numeroLinea = 0;

            while ((linea = reader.ReadLine()) != null)
            {
                numeroLinea++;

                if (string.IsNullOrWhiteSpace(linea))
                    continue;

                var partes = linea.Split(';');

                if (partes.Length != 7)
                {
                    throw new FormatException("Número de campos incorrectos.");
                }

                if (string.IsNullOrWhiteSpace(partes[0]))
                    throw new FormatException($"Línea {numeroLinea}: El nif no puede estar vacío");

                if (string.IsNullOrWhiteSpace(partes[1]))
                    throw new FormatException($"Línea {numeroLinea}: El nombre no puede estar vacío");

                if (string.IsNullOrWhiteSpace(partes[6]))
                    throw new FormatException($"Línea {numeroLinea}: El CicloFormativoId no puede estar vacío");

                if (!int.TryParse(partes[5], out var centroId))
                {
                    throw new FormatException($"Línea {numeroLinea}: CentroId inválido");
                }

                var alumno = new Alumno
                {
                    Nif = partes[0],
                    Nombre = partes[1],
                    Email = string.IsNullOrWhiteSpace(partes[2]) ? null : partes[2],
                    Direccion = string.IsNullOrWhiteSpace(partes[3]) ? null : partes[3],
                    Telefono = string.IsNullOrWhiteSpace(partes[4]) ? null : partes[4],
                    CentroId = centroId,
                    CicloFormativoId = partes[6]
                };

                if (_alumnos.ContainsKey(alumno.Nif))
                {
                    throw new FormatException($"Error en línea {numeroLinea}: NIF duplicado ({alumno.Nif})");
                }

                _alumnos[alumno.Nif] = alumno;
            }
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
