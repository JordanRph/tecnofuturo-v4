using System.Text.RegularExpressions;
using TecnoFuturo.Core.Entities;

namespace TecnoFuturo.Core.Validators;

public partial class ModuloValidator : IValidator<Modulo>
{
    public bool Validate(Modulo entity)
    {
        if (string.IsNullOrWhiteSpace(entity.CicloFormativoId))
        {
            return false;
        }

        if (entity.ModuloId <= 0)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(entity.Nombre))
        {
            return false;
        }

        if (entity.Horas <= 0 || entity.Horas > 12)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(entity.ProfesorNif))
        {
            if (!ProfesorNifRegex().IsMatch(entity.ProfesorNif))
            {
                return false;
            }
        }

        return true;
    }

    [GeneratedRegex(@"^([0-9]{8}|[XYZxyz][0-9]{7})[a-zA-Z]$")]
    private static partial Regex ProfesorNifRegex();
}