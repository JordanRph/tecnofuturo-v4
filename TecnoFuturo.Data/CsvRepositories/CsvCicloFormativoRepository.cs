using Microsoft.Extensions.DependencyInjection;
using TecnoFuturo.Core;
using TecnoFuturo.Core.DTOs;
using TecnoFuturo.Core.Entities;
using TecnoFuturo.Core.Repositories;
using TecnoFuturo.Core.Validators;

namespace TecnoFuturo.Data.CsvRepositories
{
    public class CsvCicloFormativoRepository : ICicloFormativoRepository
    {
        private readonly IServiceProvider _serviceProvider;
        private Dictionary<string, CicloFormativo> _ciclosFormativos = null!;
        private readonly string _ruta;

        public CsvCicloFormativoRepository(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _ruta = Path.Combine(Directory.GetCurrentDirectory(), "cicloformativo.csv");
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
            using var writer = new StreamWriter(_ruta);

            foreach (var c in _ciclosFormativos.Values)
            {
                var linea = $"{c.CentroId};{c.CicloFormativoId};{c.Nombre};{c.Turno}";
                writer.WriteLine(linea);
            }
        }

        private void Cargar()
        {
            _ciclosFormativos = [];

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
                    continue;


                if (string.IsNullOrWhiteSpace(partes[1]))
                    continue;
                if (string.IsNullOrWhiteSpace(partes[2]))
                    continue;
                if (!int.TryParse(partes[0], out var centroId))
                    continue;
                if (!Enum.TryParse<Turno>(partes[3], out var turno))
                    continue;
                var ciclo = new CicloFormativo
                {
                    CentroId = centroId,
                    CicloFormativoId = partes[1],
                    Nombre = partes[2],
                    Turno = turno
                };
                
                var validator = new CicloFormativoValidator();
                if (validator.Validate(ciclo))
                {
                    _ciclosFormativos.TryAdd(ciclo.CicloFormativoId, ciclo);
                } 
            }
        }
    }
}

