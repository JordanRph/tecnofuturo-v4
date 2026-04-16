using TecnoFuturo.Core.Entities;
using TecnoFuturo.Core.Repositories;
using TecnoFuturo.Core.DTOs;

namespace TecnoFuturo.Console.Extensions;

public static class Extensiones
{
    public static void MostrarInformacion(this CentroDTO centro)
    {
        System.Console.WriteLine(centro);
    }

    public static void MostarInformacion(this CicloFormativoDTO cicloFormativo)
    {
        System.Console.WriteLine(cicloFormativo);    }

    public static void MostarCiclosFormativos(this CentroDTO centro, ICicloFormativoRepository cicloFormativoRepository)
    {
        var ciclosFormativos = cicloFormativoRepository.ObtenerCiclosFormativosPorCentro(centro.CentroId);
        if (ciclosFormativos.Count == 0)
        {
            System.Console.WriteLine("NO HAY CICLOS FORMATIVOS");
            return;
        }

        System.Console.WriteLine(new string('-', 85));
        System.Console.WriteLine(" CICLOS FORMATIVOS");
        System.Console.WriteLine(new string('-', 85));
        foreach (var ciclosFormativo in ciclosFormativos)
        {

            System.Console.WriteLine(ciclosFormativo);
            System.Console.WriteLine(new string('-', 85));
        }
    }

    public static void MostrarModulos(this CicloFormativoDTO cicloFormativo, IModuloRepository moduloRepository,
    IProfesorRepository profesorRepository)
    {
        var modulos = moduloRepository.ObtenerModulosPorCicloFormativo(cicloFormativo.CicloFormativoId);

        if (modulos.Count == 0)
        {
            System.Console.WriteLine("NO HAY MODULOS REGISTRADOS");
            return;
        }

        System.Console.WriteLine(new string('-', 85));

        foreach (var modulo in modulos)
        {
            string nombreProfesor = "SIN PROFESOR ASIGNADO";

            if (!string.IsNullOrWhiteSpace(modulo.Profesor))
            {
                var profesor = profesorRepository.ObtenerProfesorPorNif(modulo.Profesor);
                nombreProfesor = profesor != null ? profesor.Nombre : "PROFESOR NO ENCONTRADO";
            }

            System.Console.WriteLine(modulo);
            System.Console.WriteLine(new string('-', 85));
        }
    }

    public static void MostrarProfesores(this CentroDTO centro, IProfesorRepository profesorRepository)
    {
        var profesores = profesorRepository.ObtenerProfesoresPorCentro(centro.CentroId);
        if (profesores.Count != 0)
        {
            System.Console.WriteLine(new string('-', 85));
            foreach (var profesor in profesores)
            {
                System.Console.WriteLine(profesor);
                System.Console.WriteLine(new string('-', 85));
            }
        }
        else
        {
            System.Console.WriteLine("NO HAY PROFESORES REGISTRADOS");
        }
    }


    public static void MostrarAlumnos(this CicloFormativoDTO cicloFormativo, IAlumnoRepository alumnoRepository)
    {
        var alumnos = alumnoRepository.ObtenerAlumnosPorCicloFormativo(cicloFormativo.CicloFormativoId);

        if (alumnos.Count != 0)
        {
            System.Console.WriteLine(new string('=', 102));
            System.Console.WriteLine($" ALUMNOS MATRICULADOS EN {cicloFormativo.Nombre,-50}");
            System.Console.WriteLine(new string('-', 102));
            foreach (var alumno in alumnos)
            {
                System.Console.WriteLine(alumno);
                System.Console.WriteLine(new string('-', 102));
            }

        }
        else
        {
            System.Console.WriteLine("NO HAY ALUMNOS MATRICULADOS");
        }

    }

    public static void MostrarResumen(this CentroDTO centro, ICicloFormativoRepository cicloFormativoRepository, IAlumnoRepository alumnoRepository)
    {
        var ciclosFormativos = cicloFormativoRepository.ObtenerCiclosFormativosPorCentro(centro.CentroId);
        var alumnosCentro = alumnoRepository.ObtenerAlumnosPorCentro(centro.CentroId);
        System.Console.WriteLine($"RESUMEN DEL CENTRO : {centro.Nombre}");
        System.Console.WriteLine($" => Numero de ciclos formativos : {ciclosFormativos.Count:N0}");
        System.Console.WriteLine($" => Numero de alumnos matriculados : {alumnosCentro.Count:N0}");
        if (ciclosFormativos.Count == 0)
        {
            System.Console.WriteLine("NO HAY CICLOS FORMATIVOS");
            return;
        }

        if (alumnosCentro.Count == 0)
        {
            System.Console.WriteLine("NO HAY ALUMNOS MATRICULADOS");
            return;
        }

        foreach (var ciclosFormativo in ciclosFormativos)
        {
            var alumnosPorCiclo = alumnoRepository.ObtenerAlumnosPorCicloFormativo(ciclosFormativo.CicloFormativoId);
            System.Console.WriteLine($"Alumnos en {ciclosFormativo.Nombre} : {alumnosPorCiclo.Count:N0}");
        }
    }
}