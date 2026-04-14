using TecnoFuturo.Core.Entities;
using TecnoFuturo.Core.DTOs;
namespace TecnoFuturo.Core.Repositories;

public interface ICicloFormativoRepository
{
    IReadOnlyList<CicloFormativoDTO> ObtenerCiclosFormativos();
    IReadOnlyList<CicloFormativoDTO> ObtenerCiclosFormativosPorCentro(int centroId);
    CicloFormativoDTO? ObtenerCicloFormativoPorId(string id);
    CicloFormativoDTO InsertarCicloFormativo(CicloFormativo cicloFormativo);
    CicloFormativoDTO ModificarCicloFormativo(CicloFormativo cicloFormativo);
    bool BorrarCicloFormativo(string id);
}