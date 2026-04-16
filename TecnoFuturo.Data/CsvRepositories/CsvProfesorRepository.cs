using Microsoft.Extensions.DependencyInjection;
using TecnoFuturo.Core.DTOs;
using TecnoFuturo.Core.Entities;
using TecnoFuturo.Core.Repositories;
using TecnoFuturo.Core.Validators;

namespace TecnoFuturo.Data.CsvRepositories
{
    public class CsvProfesorRepository : IProfesorRepository
    {
        private readonly IServiceProvider _serviceProvider;
        private Dictionary<string, Profesor> _profesores = [];
        private readonly string _ruta = null!;

        public CsvProfesorRepository(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _ruta = Path.Combine(Directory.GetCurrentDirectory(), "profesores.csv");
            Cargar();
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
            return _profesores.TryGetValue(nif, out var profesor) ? ToMap(profesor) : null;
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
            Guardar();
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
            Guardar();
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
            if (modulosPorProfesor.Count != 0)
            {
                throw new InvalidOperationException("El profesor tiene modulos asignados");
            }
            var eliminado = _profesores.Remove(nif);

            if (eliminado)
                Guardar();
            return eliminado;
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
        private void Guardar()
        {
            using var writer = new StreamWriter(_ruta);

            foreach (var p in _profesores.Values)
            {
                var linea = $"{p.CentroId};{p.Nif};{p.Nombre};{p.Email ?? ""};{p.Direccion ?? ""};{p.Telefono ?? ""}";
                writer.WriteLine(linea);
            }
        }
        private void Cargar()
        {
            _profesores = [];

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

                if (partes.Length != 6)
                    continue;
                if (!int.TryParse(partes[0], out var centroId))
                    continue;
                if (string.IsNullOrWhiteSpace(partes[1]))
                    continue;

                if (string.IsNullOrWhiteSpace(partes[2]))
                    continue;

                var profesor = new Profesor
                {
                    CentroId = centroId,
                    Nif = partes[1],
                    Nombre = partes[2],
                    Email = string.IsNullOrWhiteSpace(partes[3]) ? null : partes[3],
                    Direccion = string.IsNullOrWhiteSpace(partes[4]) ? null : partes[4],
                    Telefono = string.IsNullOrWhiteSpace(partes[5]) ? null : partes[5],
                    
                };

                var validator = new ProfesorValidator();
                if (validator.Validate(profesor))
                {
                    _profesores.TryAdd(profesor.Nif, profesor);
                }
            }
        }
    }
}
