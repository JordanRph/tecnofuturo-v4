using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TecnoFuturo.Core.DTOs;
using TecnoFuturo.Core.Entities;
using TecnoFuturo.Core.Repositories;

namespace TecnoFuturo.Data.JsonRpositories
{
    public class JsonProfesorRepository : IProfesorRepository
    {
        public bool BorrarProfesor(string nif)
        {
            throw new NotImplementedException();
        }

        public ProfesorDTO InsertarProfesor(Profesor profesor)
        {
            throw new NotImplementedException();
        }

        public ProfesorDTO ModificarProfesor(Profesor profesor)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<ProfesorDTO> ObtenerProfesores()
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<ProfesorDTO> ObtenerProfesoresPorCentro(int centroId)
        {
            throw new NotImplementedException();
        }

        public ProfesorDTO? ObtenerProfesorPorNif(string nif)
        {
            throw new NotImplementedException();
        }
    }
}
