using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using TecnoFuturo.Core.DTOs;
using TecnoFuturo.Core.Entities;
using TecnoFuturo.Core.Repositories;

namespace TecnoFuturo.Data.MongoRepositories
{
    public class MongoAlumnoRepository : IAlumnoRepository
    {
        private readonly IMongoCollection<Alumno> _alumnos;
        private readonly IServiceProvider _serviceProvider;

        public MongoAlumnoRepository(DatabaseService databaseService, IServiceProvider serviceProvider)
        {
            _alumnos = databaseService.Database.GetCollection<Alumno>("alumnos");
            _serviceProvider = serviceProvider;
        }

        public IReadOnlyList<AlumnoDTO> ObtenerAlumnos()
        {
            var alumnos = _alumnos
                .Find(a => !a.EstaBorrado)
                .ToListAsync()
                .GetAwaiter()
                .GetResult();

            return alumnos.Select(ToMap).ToList();
        }

        public IReadOnlyList<AlumnoDTO> ObtenerAlumnosPorCicloFormativo(string cicloFormativoId)
        {
            var alumnos = _alumnos
                .Find(a => a.CicloFormativoId == cicloFormativoId && !a.EstaBorrado)
                .ToListAsync()
                .GetAwaiter()
                .GetResult();

            return alumnos.Select(ToMap).ToList();
        }

        public IReadOnlyList<AlumnoDTO> ObtenerAlumnosPorCentro(int centroId)
        {
            var alumnos = _alumnos
                .Find(a => a.CentroId == centroId && !a.EstaBorrado)
                .ToListAsync()
                .GetAwaiter()
                .GetResult();

            return alumnos.Select(ToMap).ToList();
        }

        public AlumnoDTO? ObtenerAlumnoPorNif(string nif)
        {
            var alumno = _alumnos
                .Find(a => a.Nif == nif && !a.EstaBorrado)
                .FirstOrDefaultAsync()
                .GetAwaiter()
                .GetResult();

            return alumno == null ? null : ToMap(alumno);
        }

        public AlumnoDTO InsertarAlumno(Alumno alumno)
        {
            var alumnoExistente = _alumnos
                .Find(a => a.Nif == alumno.Nif && !a.EstaBorrado)
                .FirstOrDefaultAsync()
                .GetAwaiter()
                .GetResult();

            if (alumnoExistente != null)
            {
                throw new ArgumentException("El alumno ya existe", nameof(alumno));
            }

            var centroRepository = _serviceProvider.GetRequiredService<ICentroRepository>();
            if (centroRepository.ObtenerCentroPorId(alumno.CentroId) == null)
            {
                throw new ArgumentException("El centro especificado no existe", nameof(alumno));
            }

            alumno.EstaBorrado = false;

            _alumnos.InsertOneAsync(alumno)
                .GetAwaiter()
                .GetResult();

            return ToMap(alumno);
        }

        public AlumnoDTO ModificarAlumno(Alumno alumno)
        {
            var alumnoExistente = _alumnos
                .Find(a => a.Nif == alumno.Nif && !a.EstaBorrado)
                .FirstOrDefaultAsync()
                .GetAwaiter()
                .GetResult();

            if (alumnoExistente == null)
            {
                throw new ArgumentException("El alumno no existe", nameof(alumno));
            }

            var centroRepository = _serviceProvider.GetRequiredService<ICentroRepository>();
            if (centroRepository.ObtenerCentroPorId(alumno.CentroId) == null)
            {
                throw new ArgumentException("El centro especificado no existe", nameof(alumno));
            }

            var update = Builders<Alumno>.Update
                .Set(a => a.Nombre, alumno.Nombre)
                .Set(a => a.Email, alumno.Email)
                .Set(a => a.Direccion, alumno.Direccion)
                .Set(a => a.Telefono, alumno.Telefono)
                .Set(a => a.CentroId, alumno.CentroId)
                .Set(a => a.CicloFormativoId, alumno.CicloFormativoId);

            var resultado = _alumnos.UpdateOneAsync(
                a => a.Nif == alumno.Nif && !a.EstaBorrado,
                update)
                .GetAwaiter()
                .GetResult();

            if (resultado.MatchedCount == 0)
            {
                throw new ArgumentException("El alumno no existe", nameof(alumno));
            }

            return ToMap(alumno);
        }

        public bool BorrarAlumno(string nif)
        {
            var update = Builders<Alumno>.Update
                .Set(a => a.EstaBorrado, true);

            var resultado = _alumnos.UpdateOneAsync(
                a => a.Nif == nif && !a.EstaBorrado,
                update)
                .GetAwaiter()
                .GetResult();

            return resultado.ModifiedCount > 0;
        }

        private static AlumnoDTO ToMap(Alumno a)
        {
            return new AlumnoDTO(
                a.Nif,
                a.Nombre,
                a.Email,
                a.Direccion ?? string.Empty,
                a.Telefono ?? string.Empty
            );
        }
    }
}
