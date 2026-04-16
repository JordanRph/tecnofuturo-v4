using Microsoft.Extensions.DependencyInjection;
using TecnoFuturo.Core.DTOs;
using TecnoFuturo.Core.Entities;
using TecnoFuturo.Core.Repositories;
using TecnoFuturo.Core.Validators;
namespace TecnoFuturo.Data.CsvRepositories
{
    public class CsvModuloRepository : IModuloRepository
    {
        private IServiceProvider _serviceProvider;
        private Dictionary<int, Modulo> _modulos = null!;
        private readonly string _ruta;

        public CsvModuloRepository(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _ruta = Path.Combine(Directory.GetCurrentDirectory(), "modulos.csv");
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
            using var writer = new StreamWriter(_ruta);

            foreach (var m in _modulos.Values)
            {
                var linea = $"{m.CicloFormativoId};{m.ModuloId};{m.Nombre ?? ""};{m.Horas};{m.ProfesorNif ?? ""}";
                writer.WriteLine(linea);
            }
        }
        private void Cargar()
        {
            _modulos = [];

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

                if (partes.Length != 5)
                    continue;

                if (string.IsNullOrWhiteSpace(partes[0]))
                    continue;

                if (!int.TryParse(partes[1], out var moduloId))
                    continue;
                if (!int.TryParse(partes[3], out var horas))
                    continue;

                var modulo = new Modulo
                {
                    CicloFormativoId = partes[0],
                    ModuloId = moduloId,
                    Nombre = string.IsNullOrWhiteSpace(partes[2]) ? null : partes[2],
                    Horas = horas,
                    ProfesorNif = string.IsNullOrWhiteSpace(partes[4]) ? null : partes[4]

                };
                var validator = new ModuloValidator();
                if (validator.Validate(modulo))
                {
                    _modulos.TryAdd(modulo.ModuloId, modulo);
                }
            }
        }
    }
}
