using Microsoft.Extensions.DependencyInjection;
using TecnoFuturo.Core.DTOs;
using TecnoFuturo.Core.Entities;
using TecnoFuturo.Core.Repositories;
using System.Text.Json;
using TecnoFuturo.Data.Helpers;
using System.Runtime.CompilerServices;

namespace TecnoFuturo.Data.JsonRpositories
{
    public class JsonCentroRepository : ICentroRepository
    {
        private Dictionary<int, Centro> _centros = null!;
        private readonly IServiceProvider _serviceProvider;
        private JsonHelper _jsonHelper;

        public JsonCentroRepository(JsonHelper jsonHelper, IServiceProvider serviceProvider)
        {
            _jsonHelper = jsonHelper;
            _serviceProvider = serviceProvider;
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
            _jsonHelper.Guardar("centros.json", _centros.Values);
        }

        private void Cargar()
        {
            var datos = _jsonHelper.LeerDatos<Centro>("centros.json");
            _centros = datos == null ? [] : datos.ToDictionary(a => a.CentroId);
        }
    }
}
