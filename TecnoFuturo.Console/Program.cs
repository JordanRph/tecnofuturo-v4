using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TecnoFuturo.App.Configuraciones;
using TecnoFuturo.App.Servicios;
using TecnoFuturo.Console.Servicios;
using TecnoFuturo.Core;
using TecnoFuturo.Core.Entities;
using TecnoFuturo.Core.Repositories;
using TecnoFuturo.Core.Servicios;
using TecnoFuturo.InMemory.Repositories;

namespace TecnoFuturo.Console;

class Program
{
    static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        // 1. Cargar explícitamente el appsettings.json (si no lo hace por defecto)
        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        // 2. VINCULACIÓN (Binding):
        // Decimos: "Toma la sección 'ConfiguracionCentro' del JSON y rellena la clase ConfiguracionCentro"
        builder.Services.Configure<ConfiguracionCentro>(builder.Configuration.GetSection(nameof(ConfiguracionCentro)));

        // 3. Registramos nuestro servicio consumidor
        builder.Services.AddSingleton<IMensageServicio, MensajeServicio>();
        builder.Services.AddSingleton<ICentroRepository, CentroRepository>();
        builder.Services.AddSingleton<IModuloRepository, ModuloRepository>();
        builder.Services.AddSingleton<IProfesorRepository, ProfesorRepository>();
        builder.Services.AddSingleton<IAlumnoRepository, AlumnoRepository>();
        builder.Services.AddSingleton<ICicloFormativoRepository, CicloFormativoRepositoryLista>();
        builder.Services.AddSingleton<CentroServicio>();
        var host = builder.Build();

        // 4. Ejecución
        var servicio = host.Services.GetRequiredService<CentroServicio>();
        var cicloRepository = host.Services.GetRequiredService<ICicloFormativoRepository>();
        var moduloRepository = host.Services.GetRequiredService<IModuloRepository>();
        var profesorRepository = host.Services.GetRequiredService<IProfesorRepository>();
        var alumnoRepository = host.Services.GetRequiredService<IAlumnoRepository>();
        var centroId = 1;
        // Ciclos
        cicloRepository.InsertarCicloFormativo(new CicloFormativo
        {
            CentroId = centroId,
            CicloFormativoId = "DAW",
            Nombre = "Desarrollo de Aplicaciones Web",
            Turno = Turno.Matutino
        });

        cicloRepository.InsertarCicloFormativo(new CicloFormativo
        {
            CentroId = centroId,
            CicloFormativoId = "DAM",
            Nombre = "Desarrollo de Aplicaciones Multiplataforma",
            Turno = Turno.Vespertino
        });

        // Modulos
        moduloRepository.InsertarModulo(new Modulo
        {
            CicloFormativoId = "DAW",
            ModuloId = 101,
            Nombre = "Programacion",
            Horas = 7
        });

        moduloRepository.InsertarModulo(new Modulo
        {
            CicloFormativoId = "DAW",
            ModuloId = 102,
            Nombre = "Bases de Datos",
            Horas = 6
        });

        moduloRepository.InsertarModulo(new Modulo
        {
            CicloFormativoId = "DAM",
            ModuloId = 201,
            Nombre = "Entornos de Desarrollo",
            Horas = 4
        });

        moduloRepository.InsertarModulo(new Modulo
        {
            CicloFormativoId = "DAM",
            ModuloId = 202,
            Nombre = "Lenguajes de Marcas",
            Horas = 4
        });

        // Profesores
        profesorRepository.InsertarProfesor(new Profesor
        {
            CentroId = centroId,
            Nif = "12345678A",
            Nombre = "Sebastian",
            Email = "sebastian@murciaeduca.es",
            Direccion = "Calle Mayor 1",
            Telefono = "600111222"
        });

        profesorRepository.InsertarProfesor(new Profesor
        {
            CentroId = centroId,
            Nif = "87654321B",
            Nombre = "Diego",
            Email = "Diego@murciaeduca.es",
            Direccion = "Avenida Europa 12",
            Telefono = "600333444"
        });

        // Asignar profesores a modulos
        moduloRepository.ModificarModulo(new Modulo
        {
            CicloFormativoId = "DAW",
            ModuloId = 101,
            Nombre = "Programacion",
            Horas = 8,
            ProfesorNif = "12345678A"
        });

        moduloRepository.ModificarModulo(new Modulo
        {
            CicloFormativoId = "DAM",
            ModuloId = 201,
            Nombre = "Entornos de Desarrollo",
            Horas = 4,
            ProfesorNif = "12345678A"
        });
        moduloRepository.ModificarModulo(new Modulo
        {
            CicloFormativoId = "DAW",
            ModuloId = 102,
            Nombre = "Bases de Datos",
            Horas = 6, 
            ProfesorNif = "87654321B"
        });
        moduloRepository.ModificarModulo(new Modulo
        {
            CicloFormativoId = "DAM",
            ModuloId = 202,
            Nombre = "Lenguajes de Marcas",
            Horas = 4,
            ProfesorNif = "87654321B"
        });
        // Alumnos
        alumnoRepository.InsertarAlumno(new Alumno
        {
            CentroId = centroId,
            CicloFormativoId = "DAW",
            Nif = "11111111C",
            Nombre = "Carlos Ruiz",
            Email = "carlos@carlos.com",
            Direccion = "Calle Manzano",
            Telefono = "611111111"
        });

        alumnoRepository.InsertarAlumno(new Alumno
        {
            CentroId = centroId,
            CicloFormativoId = "DAW",
            Nif = "22222222D",
            Nombre = "Marta Gil",
            Email = "marta@marta.com",
            Direccion = "Calle Sagasta",
            Telefono = "622222222"
        });

        alumnoRepository.InsertarAlumno(new Alumno
        {
            CentroId = centroId,
            CicloFormativoId = "DAM",
            Nif = "33333333E",
            Nombre = "Sergio Navarro",
            Email = "sergio@sergio.com",
            Direccion = "Avenida Centro 15",
            Telefono = "633333333"
        });
        servicio.Run();
    }
}