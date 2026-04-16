using Microsoft.Extensions.DependencyInjection;
using TecnoFuturo.Core.DTOs;
using TecnoFuturo.Core.Entities;
using TecnoFuturo.Core.Repositories;

namespace TecnoFuturo.Data.CsvRepositories
{
    public class CsvCentroRepository : ICentroRepository
    {
        private Dictionary<int, Centro> _centros = null!;
        private readonly IServiceProvider _serviceProvider;
        private readonly string _ruta;

        public CsvCentroRepository(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _ruta = Path.Combine(Directory.GetCurrentDirectory(), "centros.csv");
            Cargar();
        }
        public IReadOnlyList<CentroDTO> ObtenerCentros()
        {
            return [.. _centros.Values.Select(ToMap)];
        }

        public CentroDTO? ObtenerCentroPorId(int id)
        {
            return _centros.TryGetValue(id, out var centro) ? ToMap(centro) : null;
        }

        public CentroDTO InsertarCentro(Centro centro)
        {
            if (!_centros.TryAdd(centro.CentroId, centro)) throw new InvalidOperationException("El centro ya existe");

            Guardar();
            return ToMap(centro);
        }

        public CentroDTO ModificarCentro(Centro centro)
        {
            if (!_centros.ContainsKey(centro.CentroId)) throw new InvalidOperationException("El centro no existe");

            _centros[centro.CentroId] = centro;

            Guardar();
            return ToMap(centro);
        }

        public bool BorrarCentro(int id)
        {
            if (!_centros.TryGetValue(id, out var centro))
                return false;

            var cicloFormativoRepository = _serviceProvider.GetRequiredService<ICicloFormativoRepository>();
            var ciclosFormativos = cicloFormativoRepository.ObtenerCiclosFormativosPorCentro(id);
            if (ciclosFormativos.Count != 0)
                throw new InvalidOperationException("El centro tiene ciclos formativos asociados");

            var alumnoRepository = _serviceProvider.GetRequiredService<IAlumnoRepository>();
            var alumnos = alumnoRepository.ObtenerAlumnosPorCentro(id);
            if (alumnos.Count != 0)
                throw new InvalidOperationException("El centro tiene alumnos asociados");

            var profesorRepository = _serviceProvider.GetRequiredService<IProfesorRepository>();
            var profesores = profesorRepository.ObtenerProfesoresPorCentro(id);
            if (profesores.Count != 0)
                throw new InvalidOperationException("El centro tiene profesores asociados");

            var eliminado = _centros.Remove(id);

            if (eliminado)
                Guardar();

            return eliminado;
        }

        private CentroDTO ToMap(Centro c)
        {
            return new CentroDTO(
                c.CentroId,
                c.Nombre,
                c.Direccion,
                c.Telefono
            );
        }
        private void Guardar()
        {
            try
            {
                using var writer = new StreamWriter(_ruta);

                foreach (var c in _centros.Values)
                {
                    var linea = $"{c.CentroId};{c.Nombre};{c.Direccion ?? ""};{c.Telefono ?? ""}";
                    writer.WriteLine(linea);
                }
            }
            catch
            {

            }
        }

        private void Cargar()
        {
            try
            {
                _centros = [];

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

                    if (partes.Length != 4)
                    {
                        throw new FormatException("Número de campos incorrectos.");
                    }

                    if (string.IsNullOrWhiteSpace(partes[0]))
                        throw new FormatException($"Línea {numeroLinea}: El id no puede estar vacío");

                    if (string.IsNullOrWhiteSpace(partes[1]))
                        throw new FormatException($"Línea {numeroLinea}: El nombre no puede estar vacío");

                    if (!int.TryParse(partes[0], out var centroId))
                    {
                        throw new FormatException($"Línea {numeroLinea}: CentroId inválido");
                    }

                    var centro = new Centro
                    {
                        CentroId = centroId,
                        Nombre = partes[1],
                        Direccion = string.IsNullOrWhiteSpace(partes[2]) ? null : partes[2],
                        Telefono = string.IsNullOrWhiteSpace(partes[3]) ? null : partes[3]
                    };
                    

                    if (_centros.ContainsKey(centro.CentroId))
                    {
                        throw new FormatException($"Error en línea {numeroLinea}: ID duplicado ({centro.CentroId})");
                    }

                    _centros[centro.CentroId] = centro;
                }
            }
            catch { }
        }
    }
}

