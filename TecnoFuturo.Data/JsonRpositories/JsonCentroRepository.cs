using Microsoft.Extensions.DependencyInjection;
using TecnoFuturo.Core.DTOs;
using TecnoFuturo.Core.Entities;
using TecnoFuturo.Core.Repositories;
using System.Text.Json;

namespace TecnoFuturo.Data.JsonRpositories
{
    public class JsonCentroRepository : ICentroRepository
    {
        private Dictionary<int, Centro> _centros = null!;
        private readonly IServiceProvider _serviceProvider;
        private readonly string _ruta;

        public JsonCentroRepository(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _ruta = Path.Combine(Directory.GetCurrentDirectory(), "centros.json");
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
            var json = JsonSerializer.Serialize(_centros.Values, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(_ruta, json);
        }

        private void Cargar()
        {
            if (!File.Exists(_ruta))
            {
                _centros = [];
                return;
            }
            var json = File.ReadAllText(_ruta);
            var lista = JsonSerializer.Deserialize<List<Centro>>(json);

            _centros = lista?.ToDictionary(c => c.CentroId) ?? [];
        }
    }
}
