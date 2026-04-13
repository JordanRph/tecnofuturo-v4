using TecnoFuturo.Core.Entities;

namespace TecnoFuturo.Core.Validators;

public class CicloFormativoValidator : IValidator<CicloFormativo>
{
    public bool Validate(CicloFormativo entity)
    {
        if (entity.CentroId <= 0)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(entity.CicloFormativoId))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(entity.Nombre))
        {
            return false;
        }

        if (!Enum.IsDefined(typeof(Turno), entity.Turno))
        {
            return false;
        }

        return true;
    }
}