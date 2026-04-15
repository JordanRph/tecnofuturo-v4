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
    public class JsonModuloRepository : IModuloRepository
    {
        public bool BorrarModulo(int id)
        {
            throw new NotImplementedException();
        }

        public ModuloDTO InsertarModulo(Modulo modulo)
        {
            throw new NotImplementedException();
        }

        public ModuloDTO ModificarModulo(Modulo modulo)
        {
            throw new NotImplementedException();
        }

        public ModuloDTO? ObtenerModuloPorId(int id)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<ModuloDTO> ObtenerModulos()
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<ModuloDTO> ObtenerModulosPorCicloFormativo(string cicloFormativoId)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<ModuloDTO> ObtenerModulosPorProfesor(string profesorNif)
        {
            throw new NotImplementedException();
        }
    }
}
