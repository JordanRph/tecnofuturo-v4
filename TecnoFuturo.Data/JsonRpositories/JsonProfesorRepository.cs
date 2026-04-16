using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TecnoFuturo.Core.DTOs;
using TecnoFuturo.Core.Entities;
using TecnoFuturo.Core.Repositories;

namespace TecnoFuturo.Data.JsonRpositories
{
    public class JsonProfesorRepository : IProfesorRepository
    {
        private readonly IServiceProvider _serviceProvider;
        private Dictionary<string, Profesor> _profesores = [];
        private readonly string _ruta = null!;

        public JsonProfesorRepository(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _ruta = Path.Combine(Directory.GetCurrentDirectory(), "profesores.json");
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
            return modulosPorProfesor.Count != 0
                ? throw new InvalidOperationException("El profesor tiene modulos asignados")
                : _profesores.Remove(nif);
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
            var json = JsonSerializer.Serialize(_profesores.Values, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(_ruta, json);
        }
        private void Cargar()
        {
            if (!File.Exists(_ruta))
            {
                _profesores = [];
                return;
            }
            var json = File.ReadAllText(_ruta);
            var lista = JsonSerializer.Deserialize<List<Profesor>>(json);

            _profesores = lista?.ToDictionary(x => x.Nif) ?? [];
        }
    }
}
