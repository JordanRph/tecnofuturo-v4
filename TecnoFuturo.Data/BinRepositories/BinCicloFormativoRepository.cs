using Microsoft.Extensions.DependencyInjection;
using ProtoBuf;
using TecnoFuturo.Core.DTOs;
using TecnoFuturo.Core.Entities;
using TecnoFuturo.Core.Repositories;

namespace TecnoFuturo.Data.BinRepositories
{
    public class BinCicloFormativoRepository : ICicloFormativoRepository
    {
        private readonly IServiceProvider _serviceProvider;
        private Dictionary<string, CicloFormativo> _ciclosFormativos = null!;
        private readonly string _ruta;

        public BinCicloFormativoRepository(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _ruta = Path.Combine(Directory.GetCurrentDirectory(), "cicloformativo.bin");
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

            if (!_ciclosFormativos.TryAdd(cicloFormativo.CicloFormativoId, cicloFormativo)) throw new InvalidOperationException("El ciclo formativo ya existe");
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
            if (!_ciclosFormativos.TryGetValue(id, out var cicloFormativo))
                return false;

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

            var eliminado = _ciclosFormativos.Remove(id);

            if (eliminado)
                Guardar();

            return eliminado;
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
            try
            {
                using var stream = File.Create(_ruta);
                Serializer.Serialize(stream, _ciclosFormativos.Values);
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
                    _ciclosFormativos = [];
                    return;
                }
                using var stream = File.OpenRead(_ruta);
                var lista = Serializer.Deserialize<List<CicloFormativo>>(stream);
                if(lista == null)
                {
                    _ciclosFormativos = [];
                    return;
                }
                _ciclosFormativos = lista.ToDictionary(c => c.CicloFormativoId);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex); 
                _ciclosFormativos = [];
            }
        }
    }
}
