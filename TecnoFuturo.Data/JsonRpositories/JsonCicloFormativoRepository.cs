using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using TecnoFuturo.Core.DTOs;
using TecnoFuturo.Core.Entities;
using TecnoFuturo.Core.Repositories;

namespace TecnoFuturo.Data.JsonRpositories
{
    public class JsonCicloFormativoRepository : ICicloFormativoRepository
    {
        private readonly IServiceProvider _serviceProvider;
        private Dictionary<string, CicloFormativo> _ciclosFormativos = null!;
        private readonly string _ruta;

        public JsonCicloFormativoRepository(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _ruta = Path.Combine(Directory.GetCurrentDirectory(), "cicloformativo.json");
            Cargar();
        }

        public IReadOnlyList<CicloFormativoDTO> ObtenerCiclosFormativos()
        {
            return [.. _ciclosFormativos.Values.Select(ToMap)];
        }

        public IReadOnlyList<CicloFormativoDTO> ObtenerCiclosFormativosPorCentro(int centroId)
        {
            return [.. _ciclosFormativos.Values.Where(c => c.CentroId == centroId).Select(ToMap)];
        }

        public CicloFormativoDTO? ObtenerCicloFormativoPorId(string id)
        {
            return _ciclosFormativos.TryGetValue(id, out var ciclo) ? ToMap(ciclo) : null;
        }

        public CicloFormativoDTO InsertarCicloFormativo(CicloFormativo cicloFormativo)
        {
            var centroRepository = _serviceProvider.GetRequiredService<ICentroRepository>();

            if (centroRepository.ObtenerCentroPorId(cicloFormativo.CentroId) is null) throw new ArgumentException("El centro especificado no existe", nameof(cicloFormativo));

            if (!_ciclosFormativos.TryAdd(cicloFormativo.CicloFormativoId, cicloFormativo))throw new InvalidOperationException("El ciclo formativo ya existe");
            Guardar();
            return ToMap(cicloFormativo);
        }

        public CicloFormativoDTO ModificarCicloFormativo(CicloFormativo cicloFormativo)
        {
            var centroRepository = _serviceProvider.GetRequiredService<ICentroRepository>();
            var centro = centroRepository.ObtenerCentroPorId(cicloFormativo.CentroId);
            if (centro == null)
            {
                throw new ArgumentException("El centro especificado no existe", nameof(cicloFormativo));
            }

            if (!_ciclosFormativos.ContainsKey(cicloFormativo.CicloFormativoId))
            {
                throw new ArgumentException("El ciclo formativo no existe", nameof(cicloFormativo));
            }

            _ciclosFormativos[cicloFormativo.CicloFormativoId] = cicloFormativo;
            Guardar();
            return ToMap(cicloFormativo);
        }

        public bool BorrarCicloFormativo(string id)
        {
            if (!_ciclosFormativos.TryGetValue(id, out var cicloFormativo)) return false;

            var centroRepository = _serviceProvider.GetRequiredService<ICentroRepository>();
            var centro = centroRepository.ObtenerCentroPorId(cicloFormativo.CentroId);
            if (centro == null)
            {
                throw new ArgumentException("El centro especificado no existe");
            }

            var moduloRepository = _serviceProvider.GetRequiredService<IModuloRepository>();
            var modulos = moduloRepository.ObtenerModulosPorCicloFormativo(id);

            if (modulos.Count != 0)
            {
                throw new InvalidOperationException("El ciclo formativo tiene modulos asociados");
            }

            return _ciclosFormativos.Remove(id);
        }
        private CicloFormativoDTO ToMap(CicloFormativo c)
        {
            return new CicloFormativoDTO(
                c.CentroId,
                c.CicloFormativoId,
                c.Nombre,
                c.Turno);
        }
       
        private void Guardar()
        {
            var json = JsonSerializer.Serialize(_ciclosFormativos.Values, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(_ruta, json);
        }

        private void Cargar()
        {
            if (!File.Exists(_ruta))
            {
                return;
            }
            var json = File.ReadAllText(_ruta);
            var lista = JsonSerializer.Deserialize<List<CicloFormativo>>(json);

            if (lista == null)
                return;

            foreach (var ciclo in lista)
            {
                _ciclosFormativos[ciclo.CicloFormativoId] = ciclo;
            }
        }
    }
}
