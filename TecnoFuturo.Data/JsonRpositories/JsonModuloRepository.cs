using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using TecnoFuturo.Core.DTOs;
using TecnoFuturo.Core.Entities;
using TecnoFuturo.Core.Repositories;

namespace TecnoFuturo.Data.JsonRpositories
{
    public class JsonModuloRepository : IModuloRepository
    {
        private IServiceProvider _serviceProvider;
        private Dictionary<int, Modulo> _modulos = null!;
        private readonly string _ruta;

        public JsonModuloRepository(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _ruta = Path.Combine(Directory.GetCurrentDirectory(), "modulos.json");
            Cargar();
        }

        public IReadOnlyList<ModuloDTO> ObtenerModulos()
        {
            return [.. _modulos.Values.Select(ToMap)];
        }

        public IReadOnlyList<ModuloDTO> ObtenerModulosPorCicloFormativo(string cicloFormativoId)
        {
            return [.. _modulos.Values.Where(m => m.CicloFormativoId == cicloFormativoId).Select(ToMap)];
        }

        public IReadOnlyList<ModuloDTO> ObtenerModulosPorProfesor(string profesorNif)
        {
            return [.. _modulos.Values.Where(m => m.ProfesorNif == profesorNif).Select(ToMap)];
        }

        public ModuloDTO? ObtenerModuloPorId(int id)
        {
            return _modulos.TryGetValue(id, out var m) ? ToMap(m) : null;
        }

        public ModuloDTO InsertarModulo(Modulo modulo)
        {
            var cicloFormativoRepository = _serviceProvider.GetRequiredService<ICicloFormativoRepository>();

            if (cicloFormativoRepository.ObtenerCicloFormativoPorId(modulo.CicloFormativoId) is null)
                throw new ArgumentException("El ciclo formativo no existe", nameof(modulo));

            if (!_modulos.TryAdd(modulo.ModuloId, modulo)) throw new InvalidOperationException("El modulo ya existe");

            Guardar();

            return ToMap(modulo);
        }

        public ModuloDTO ModificarModulo(Modulo modulo)
        {
            var cicloFormativoRepository = _serviceProvider.GetRequiredService<ICicloFormativoRepository>();
            var cicloFormativo = cicloFormativoRepository.ObtenerCicloFormativoPorId(modulo.CicloFormativoId) ?? throw new ArgumentException("El ciclo formativo no existe", nameof(modulo.CicloFormativoId));
            if (!_modulos.ContainsKey(modulo.ModuloId))
            {
                throw new ArgumentException("El modulo no existe", nameof(modulo.ModuloId));
            }
            _modulos[modulo.ModuloId] = modulo;
            Guardar();
            return ToMap(modulo);
        }

        public bool BorrarModulo(int id)
        {
            if (_modulos.TryGetValue(id, out var modulo))
            {
                var cicloFormativoRepository = _serviceProvider.GetRequiredService<ICicloFormativoRepository>();
                var cicloFormativo = cicloFormativoRepository.ObtenerCicloFormativoPorId(modulo.CicloFormativoId);

                if (cicloFormativo == null)
                {
                    throw new ArgumentException("El ciclo formativo no existe", nameof(modulo.CicloFormativoId));
                }
                var eliminado = _modulos.Remove(id);
                if (eliminado)
                    Guardar();
            }
            return false;
        }
        private ModuloDTO ToMap(Modulo m)
        {
            return new ModuloDTO(
                m.CicloFormativoId,
                m.ModuloId,
                m.Nombre,
                m.Horas,
                m.ProfesorNif ?? string.Empty);
        }
        private void Guardar()
        {
            var json = JsonSerializer.Serialize(_modulos.Values, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(_ruta,json);
        }
        private void Cargar()
        {
            if (!File.Exists(_ruta))
            {
                _modulos = [];
                return;
            }
            var json = File.ReadAllText(_ruta);
            var lista = JsonSerializer.Deserialize<List<Modulo>>(json);

            _modulos = lista?.ToDictionary(x => x.ModuloId) ?? [];
        }
    }
}
