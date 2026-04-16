using Microsoft.Extensions.DependencyInjection;
using ProtoBuf;
using TecnoFuturo.Core.DTOs;
using TecnoFuturo.Core.Entities;
using TecnoFuturo.Core.Repositories;

namespace TecnoFuturo.Data.BinRepositories
{
    public class BinProfesorRepository : IProfesorRepository
    {
        private readonly IServiceProvider _serviceProvider;
        private Dictionary<string, Profesor> _profesores = [];
        private readonly string _ruta = null!;

        public BinProfesorRepository(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _ruta = Path.Combine(Directory.GetCurrentDirectory(), "profesores.bin");
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
            try
            {
                using var stream = File.Create(_ruta);
                Serializer.Serialize(stream, _profesores.Values);
            }
            catch
            {

            }
        }

        private void Cargar()
        {
            try
            {
                if (!File.Exists(_ruta))
                {
                    _profesores = [];
                    return;
                }
                using var stream = File.OpenRead(_ruta);
                var lista = Serializer.Deserialize<List<Profesor>>(stream);
                _profesores = lista.ToDictionary(p => p.Nif);
            }
            catch { }
        }
    }
}
