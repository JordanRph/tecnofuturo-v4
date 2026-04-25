using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using TecnoFuturo.Core.DTOs;
using TecnoFuturo.Core.Entities;
using TecnoFuturo.Core.Repositories;
using TecnoFuturo.Data.Helpers;

namespace TecnoFuturo.Data.JsonRpositories
{
    public class JsonModuloRepository : IModuloRepository
    {
        private IServiceProvider _serviceProvider;
        private Dictionary<int, Modulo> _modulos = null!;
        private readonly JsonHelper _jsonHelper;

        public JsonModuloRepository(JsonHelper jsonHelper, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _jsonHelper = jsonHelper;
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
            _jsonHelper.Guardar("modulos.json", _modulos.Values);
        }
        private void Cargar()
        {
            var datos = _jsonHelper.LeerDatos<Modulo>("modulos.json");
            _modulos = datos == null ? [] : datos.ToDictionary(m => m.ModuloId);
        }
    }
}
